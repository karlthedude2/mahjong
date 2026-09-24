# Deploying the web version to Azure

The site runs on **Azure App Service** (Linux, .NET 10) with an **Azure SQL** database and
**Azure Communication Services** for account emails. GitHub Actions builds, tests, migrates
the database and deploys on every push to `main`.

| Workflow | When | What it does |
|---|---|---|
| `ci.yml` | Every push and pull request | Runs all tests, publishes the web app, builds the desktop app |
| `infra.yml` | By hand | Creates or updates the Azure resources from `infra/main.bicep` |
| `deploy.yml` | Push to `main` (or by hand) | Tests, migrates the database, deploys, then checks `/healthz` |

## Costs and limits of the default (cheapest) setup

- **App Service F1 (free).**
  - 60 CPU-minutes a day, which is plenty, because the game runs in the browser.
  - The app sleeps when idle, so the first visit after a quiet spell takes a few seconds.
  - No custom domain, so the site lives at `https://<APP_NAME>.azurewebsites.net`.
  - No staging slot.
  - A subscription can have several free plans (currently up to 10 per region), so this can sit
    alongside other free apps you run.
- **Azure SQL free offer.**
  - Up to **10 free databases per subscription**, each with its own monthly allowance of
    100,000 vCore-seconds and 32 GB.
  - If the allowance runs out, the database pauses until next month instead of charging you.
  - **All free databases in a subscription must be in the same region.** If you already have one,
    deploy to that region (see step 1).
- **Email.** Pay per message, fractions of a cent each.

To upgrade, run `infra.yml` and pick `B1` (about $13/month; always on, health checks) or `S1`
(adds staging slots and autoscale).

## One-time setup

### 1. Azure sign-in for GitHub (OIDC, no stored passwords)

1. Sign in with `az login`.
2. Choose an app name. It becomes part of the site's URL, so it must be globally unique.
3. Run:

```powershell
./infra/bootstrap.ps1 -AppName mahjong-karl -Location eastus
```

Choose `-Location` carefully. Everything is created in that region, and Azure requires all of a
subscription's free databases to share one region. If you already have a free Azure SQL database,
use its region: in the Azure portal, open that database and read **Location** on its Overview
page, then pass the short name (for example `westus2` for "West US 2").

It creates the resource group and an app registration that trusts this repository, and gives it
Contributor on the resource group. When it finishes, it prints the values to add to GitHub.

### 2. GitHub settings

In **Settings > Environments**, the `production` environment is created automatically the first
time the deploy workflow runs; you can also create it yourself. It belongs to this repository only,
so an environment with the same name in another repository doesn't matter. Optionally, open it and
add yourself under **Required reviewers** to approve each deploy.

In **Settings > Secrets and variables > Actions**
(`https://github.com/<owner>/<repo>/settings/secrets/actions`), add each of the following as its own
entry. Secrets go on the **Secrets** tab with **New repository secret**; variables go on the
**Variables** tab with **New repository variable**.

| Kind | Name | Value |
|---|---|---|
| Secret | `AZURE_CLIENT_ID` | Printed by `bootstrap.ps1` |
| Secret | `AZURE_TENANT_ID` | Printed by `bootstrap.ps1` |
| Secret | `AZURE_SUBSCRIPTION_ID` | Printed by `bootstrap.ps1` |
| Secret | `SQL_ADMIN_PASSWORD` | A strong password you choose (Azure requires 3 of: upper, lower, digit, symbol) |
| Variable | `APP_NAME` | The app name you chose |
| Variable | `AZURE_RESOURCE_GROUP` | Printed by `bootstrap.ps1` (`rg-<APP_NAME>`) |
| Variable | `SQL_ADMIN_LOGIN` | e.g. `mahjongadmin` |

Sign-in providers are optional. Each one switches on once its keys are set (see step 3):

| Secret | Provider |
|---|---|
| `GOOGLE_CLIENT_ID` | Google |
| `GOOGLE_CLIENT_SECRET` | Google |
| `MICROSOFT_CLIENT_ID` | Microsoft |
| `MICROSOFT_CLIENT_SECRET` | Microsoft |
| `FACEBOOK_APP_ID` | Facebook |
| `FACEBOOK_APP_SECRET` | Facebook |

Ads are optional too (see step 5). Each of these is its own variable: `ADS_CLIENT_ID`,
`ADS_RAIL_SLOT`, `ADS_BANNER_SLOT` and `ADS_RESULTS_SLOT`.

### 3. Sign-in providers

Create an OAuth app with each provider. The redirect URIs are:

| Provider | Where | Redirect URI |
|---|---|---|
| Google | [Google Cloud console](https://console.cloud.google.com/apis/credentials) > Create OAuth client ID > Web application | `https://<APP_NAME>.azurewebsites.net/signin-google` |
| Microsoft | [Entra admin center](https://entra.microsoft.com) > App registrations > New registration. Choose "Accounts in any organizational directory and personal Microsoft accounts", then create a client secret. | `https://<APP_NAME>.azurewebsites.net/signin-microsoft` |
| Facebook | [Meta for Developers](https://developers.facebook.com/apps) > Create app > Facebook Login | `https://<APP_NAME>.azurewebsites.net/signin-facebook` |

Facebook needs business verification before people outside your developer account can sign in.

### 4. Create the resources and deploy

1. In GitHub, go to **Actions**, select **Deploy Azure infrastructure**, and choose **Run workflow**.
2. Push or merge to `main`. **Deploy web app** migrates the database and publishes the site.

Run the infrastructure workflow again whenever you change a secret or variable. That's how new
values reach the web app's settings.

### 5. Google AdSense

1. Apply at [adsense.google.com](https://adsense.google.com) once the site is live. Google
   reviews the site before approving it.
2. After approval, create three **display ad units**:
   - a vertical one for the side rails;
   - a horizontal one for the banner on small screens;
   - a rectangle for the results dialog.
3. Set `ADS_CLIENT_ID` (`ca-pub-...`) and the three slot IDs as GitHub variables.
4. Run **Deploy Azure infrastructure** again.

Until these are set, the site shows labelled placeholders where the ads will go.

How the ads are placed:
- Guests see ads on both sides of the board, plus one in the results dialog after a win. The
  "Play again" button waits 5 seconds.
- Signed-in players see a single side ad and no results ad.
- AdSense rules forbid encouraging clicks and placing ads in pop-up windows. The results ad is an
  ordinary ad unit inside the page's own dialog, next to the score breakdown, and the dialog can
  always be used normally.

## Running locally

The web projects target .NET 10, which needs the .NET 10 SDK and Visual Studio 2026 (or VS Code,
or the `dotnet` command line). Visual Studio 2022 can still build and run the desktop game, but
it can't load the web projects.

```powershell
dotnet run --project web/Mahjong.Web
```

- **Database:** SQL Server LocalDB (installed with Visual Studio). It's created and migrated
  automatically in Development.
- **Email:** without an email connection string, the confirmation link appears on the page after
  you register, and in the log.
- **Hidden layouts:** Development shows hidden layouts such as **Test**, which is handy for
  finishing a game quickly.
- **Sign-in keys:** to try a provider locally, store its keys with user secrets:

```powershell
dotnet user-secrets --project web/Mahjong.Web set "Authentication:Google:ClientId" "<id>"
dotnet user-secrets --project web/Mahjong.Web set "Authentication:Google:ClientSecret" "<secret>"
```

Also register `https://localhost:7015/signin-google` as a redirect URI.

## How scores are protected

- Signed-in games get their seed from the server. The browser sends back the list of moves and
  when each one happened.
- The server replays those moves with the same rules code (`Mahjong.Core`) and rejects any
  impossible move.
- The score is computed by the server; any score the browser claims is ignored.
- The game clock must match the real time played, within 30 seconds. Pausing works for everyone:
  the browser tells the server when a ranked game is paused and resumed, and the server times the
  pause itself, so paused time is left out without trusting the browser. The board is hidden
  while paused.
- A player can still play well with help, but forged scores, impossible games and faked times are
  rejected.
