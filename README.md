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

```pwsh
dotnet publish src/BankAccountGenerator -c Release -o publish
```

Upload `publish/wwwroot`.

* **Azure Static Web Apps**: app location `publish/wwwroot` (or `src/BankAccountGenerator` with output
  `wwwroot` after publish), no API. `staticwebapp.config.json` is included.
* **GitHub Pages** (served from `/<repo>/`): after publishing, change `<base href="/" />` in
  `publish/wwwroot/index.html` to `<base href="/<repo>/" />`, and add an empty `.nojekyll` file.
