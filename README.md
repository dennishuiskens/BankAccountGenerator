# Bank account generator

Internal test tool that generates syntactically valid Norwegian bank account numbers (11 digits, MOD11
check digit) and matching IBANs. The numbers are not real accounts.

* **Random bank** – picks a bank code from the list itself.
* **Specific bank** – choose a bank from the list, or type the first four digits (bank code).

Blazor WebAssembly + [Radzen](https://blazor.radzen.com/). It is fully static, so it runs on Azure Static Web
Apps, GitHub Pages or any other static host.

## Run locally

```pwsh
dotnet run --project src/BankAccountGenerator
```

## Bank list

The list comes from <https://bits.no/document/iban/> (xlsx). The browser cannot read that file directly
(CORS), so it is converted to `src/BankAccountGenerator/wwwroot/data/banks.json`, which is committed.

To refresh it, set **BankUpdater** as the startup project and run it (or `dotnet run --project src/BankUpdater`).
It downloads the xlsx and rewrites `banks.json`. Pass a path to a local xlsx to skip the download.
Then commit the changed `banks.json`.

## Deploy

### GitHub Pages (automatic)

`.github/workflows/deploy-pages.yml` publishes the site on every push to `main`. One-time setup: in the
GitHub repository go to **Settings > Pages** and set **Source** to **GitHub Actions**. The site is then
available at `https://<user>.github.io/<repository>/`. The workflow sets the `<base href>` for you.

### Azure Static Web Apps

```pwsh
dotnet publish src/BankAccountGenerator -c Release -o publish
```

Upload `publish/wwwroot` (no API). `staticwebapp.config.json` is included.
