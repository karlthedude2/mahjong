<#
.SYNOPSIS
    One-time setup so GitHub Actions can deploy to Azure without stored passwords (OIDC).

.DESCRIPTION
    Creates the resource group, an Entra app registration that trusts this GitHub repository,
    and gives it Contributor on the resource group. Then prints the GitHub secrets and
    variables to add. Run it once, signed in with `az login` as someone who can create app
    registrations and assign roles.

.EXAMPLE
    ./infra/bootstrap.ps1 -AppName mahjong-karl -Location eastus
#>
param(
    [Parameter(Mandatory)] [string] $AppName,
    [string] $ResourceGroup = "rg-$AppName",
    [string] $Location = "eastus",
    [string] $Repository = "karlthedude2/mahjong"
)

$ErrorActionPreference = "Stop"

$account = az account show | ConvertFrom-Json
Write-Host "Subscription: $($account.name) ($($account.id))"

# Each family of Azure services must be switched on ("registered") once per subscription. The
# deploy identity can't do this itself, because it only has access to the resource group.
foreach ($namespace in "Microsoft.Web", "Microsoft.Sql", "Microsoft.Communication") {
    Write-Host "Registering $namespace (if needed)..."
    az provider register --namespace $namespace --wait --output none
}

Write-Host "Creating resource group $ResourceGroup in $Location..."
az group create --name $ResourceGroup --location $Location --output none

Write-Host "Creating app registration for GitHub Actions..."
$app = az ad app create --display-name "$AppName-github-deploy" | ConvertFrom-Json
az ad sp create --id $app.appId --output none 2>$null

# The deploy and infra workflows run in the "production" environment; main is trusted too.
$subjects = @{
    "github-production" = "repo:$($Repository):environment:production"
    "github-main"       = "repo:$($Repository):ref:refs/heads/main"
}
foreach ($name in $subjects.Keys) {
    $credential = @{
        name      = $name
        issuer    = "https://token.actions.githubusercontent.com"
        subject   = $subjects[$name]
        audiences = @("api://AzureADTokenExchange")
    } | ConvertTo-Json -Compress
    $file = New-TemporaryFile
    Set-Content -Path $file -Value $credential -Encoding utf8
    az ad app federated-credential create --id $app.appId --parameters "@$file" --output none
    Remove-Item $file
}

Write-Host "Granting Contributor on $ResourceGroup..."
$scope = "/subscriptions/$($account.id)/resourceGroups/$ResourceGroup"
az role assignment create --assignee $app.appId --role Contributor --scope $scope --output none

Write-Host ""
Write-Host "Done. In GitHub (Settings > Secrets and variables > Actions), add:" -ForegroundColor Green
Write-Host ""
Write-Host "Secrets:"
Write-Host "  AZURE_CLIENT_ID        = $($app.appId)"
Write-Host "  AZURE_TENANT_ID        = $($account.tenantId)"
Write-Host "  AZURE_SUBSCRIPTION_ID  = $($account.id)"
Write-Host "  SQL_ADMIN_PASSWORD     = <a strong password you choose>"
Write-Host "  GOOGLE_CLIENT_ID / GOOGLE_CLIENT_SECRET, MICROSOFT_CLIENT_ID / MICROSOFT_CLIENT_SECRET,"
Write-Host "  FACEBOOK_APP_ID / FACEBOOK_APP_SECRET  (optional; see docs/deploy.md)"
Write-Host ""
Write-Host "Variables:"
Write-Host "  APP_NAME               = $AppName"
Write-Host "  AZURE_RESOURCE_GROUP   = $ResourceGroup"
Write-Host "  SQL_ADMIN_LOGIN        = mahjongadmin   (or any name you like)"
Write-Host "  ADS_CLIENT_ID, ADS_RAIL_SLOT, ADS_BANNER_SLOT, ADS_RESULTS_SLOT  (optional, after AdSense approval)"
Write-Host ""
Write-Host "Also create an environment named 'production' (Settings > Environments)."
