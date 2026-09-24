// Azure resources for the Mahjong web app: App Service (Linux, .NET 10), Azure SQL (free
// serverless offer) and Azure Communication Services email. Deployed by .github/workflows/infra.yml.

@description('Base name for all resources. The web app is reachable at https://<appName>.azurewebsites.net, so it must be globally unique.')
@minLength(3)
@maxLength(40)
param appName string

param location string = resourceGroup().location

@description('App Service plan SKU. F1 is free (60 CPU-minutes/day, no custom domain); B1 or S1 for real traffic.')
@allowed([ 'F1', 'B1', 'B2', 'S1', 'P0v3' ])
param appServiceSku string = 'F1'

@description('''Database tier. Free is the Azure SQL free offer (serverless, monthly allowance, pauses when
the allowance runs out). Basic, S0-S3 are fixed-price tiers that never pause. Moving off Free is
one-way; the infra workflow switches the free offer off first.''')
@allowed([ 'Free', 'Basic', 'S0', 'S1', 'S2', 'S3' ])
param databaseTier string = 'Free'

param sqlAdminLogin string

@secure()
param sqlAdminPassword string

@description('Where email data is stored; must be a Communication Services data location.')
param emailDataLocation string = 'United States'

@secure()
param googleClientId string = ''
@secure()
param googleClientSecret string = ''
@secure()
param microsoftClientId string = ''
@secure()
param microsoftClientSecret string = ''
@secure()
param facebookAppId string = ''
@secure()
param facebookAppSecret string = ''

@description('The main domain for the site (e.g. mahjong.haus). Other addresses redirect to it. Empty: no redirect.')
param canonicalHost string = ''

param adsClientId string = ''
param adsRailSlot string = ''
param adsBannerSlot string = ''
param adsResultsSlot string = ''

var isFree = appServiceSku == 'F1'
var databaseName = 'mahjong'

var isFreeDatabase = databaseTier == 'Free'
var databaseSku = isFreeDatabase
  ? { name: 'GP_S_Gen5_2', tier: 'GeneralPurpose', family: 'Gen5', capacity: 2 }
  : databaseTier == 'Basic' ? { name: 'Basic', tier: 'Basic' } : { name: databaseTier, tier: 'Standard' }

// Free offer: 100,000 vCore-seconds and 32 GB a month; when the monthly allowance runs out the
// database pauses until the next month instead of billing. Basic holds up to 2 GB, Standard 250 GB.
var databaseProperties = isFreeDatabase
  ? {
      useFreeLimit: true
      freeLimitExhaustionBehavior: 'AutoPause'
      autoPauseDelay: 60
      minCapacity: json('0.5')
      maxSizeBytes: 34359738368
    }
  : {
      useFreeLimit: false
      maxSizeBytes: databaseTier == 'Basic' ? 2147483648 : 268435456000
    }

// ---------- Database ----------

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: '${appName}-sql'
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// 0.0.0.0 is Azure's shorthand for "allow Azure services", which lets the web app connect.
resource allowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: databaseSku
  properties: databaseProperties
}

// ---------- Email ----------

resource emailService 'Microsoft.Communication/emailServices@2023-04-01' = {
  name: '${appName}-email'
  location: 'global'
  properties: {
    dataLocation: emailDataLocation
  }
}

resource emailDomain 'Microsoft.Communication/emailServices/domains@2023-04-01' = {
  parent: emailService
  name: 'AzureManagedDomain'
  location: 'global'
  properties: {
    domainManagement: 'AzureManaged'
    userEngagementTracking: 'Disabled'
  }
}

resource communication 'Microsoft.Communication/communicationServices@2023-04-01' = {
  name: '${appName}-acs'
  location: 'global'
  properties: {
    dataLocation: emailDataLocation
    linkedDomains: [ emailDomain.id ]
  }
}

// ---------- Web app ----------

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${appName}-plan'
  location: location
  kind: 'linux'
  sku: {
    name: appServiceSku
  }
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      // Two *.runtimeconfig.json files are published (server and WebAssembly client), so the
      // platform can't guess the entry point; without this it serves its default page.
      appCommandLine: 'dotnet Mahjong.Web.dll'
      alwaysOn: !isFree
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      // Health checks need a paid plan; the deploy workflow smoke-tests /healthz either way.
      healthCheckPath: isFree ? null : '/healthz'
      connectionStrings: [
        {
          name: 'DefaultConnection'
          type: 'SQLAzure'
          connectionString: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${databaseName};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;'
        }
      ]
      appSettings: [
        // App Service terminates HTTPS in front of the app; this makes sign-in redirects use https.
        { name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED', value: 'true' }
        { name: 'Authentication__Google__ClientId', value: googleClientId }
        { name: 'Authentication__Google__ClientSecret', value: googleClientSecret }
        { name: 'Authentication__Microsoft__ClientId', value: microsoftClientId }
        { name: 'Authentication__Microsoft__ClientSecret', value: microsoftClientSecret }
        { name: 'Authentication__Facebook__AppId', value: facebookAppId }
        { name: 'Authentication__Facebook__AppSecret', value: facebookAppSecret }
        { name: 'Email__ConnectionString', value: communication.listKeys().primaryConnectionString }
        { name: 'Email__SenderAddress', value: 'DoNotReply@${emailDomain.properties.mailFromSenderDomain}' }
        { name: 'Site__CanonicalHost', value: canonicalHost }
        { name: 'Ads__ClientId', value: adsClientId }
        { name: 'Ads__RailSlot', value: adsRailSlot }
        { name: 'Ads__BannerSlot', value: adsBannerSlot }
        { name: 'Ads__ResultsSlot', value: adsResultsSlot }
      ]
    }
  }
}

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output sqlServerName string = sqlServer.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = database.name
output databaseTier string = databaseTier
