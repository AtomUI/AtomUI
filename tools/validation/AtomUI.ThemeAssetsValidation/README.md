# Theme asset validation

```bash
dotnet run --project tools/validation/AtomUI.ThemeAssetsValidation/AtomUI.ThemeAssetsValidation.csproj -c Release
```

The tool commits a real Headless Avalonia application with the official control packages and Gallery. It forces each selected
theme resource factory, including deferred exported themes and known resource keys, through its normal application owner
bridge. It fails on a factory or contract exception, reports per-package materialization counts, and disposes the scope before
printing the final summary. CI runs this independently of test selection.

This checks factory materialization and ownership. Native window rendering, control-template layout and user interaction
continue to require their focused UI tests and platform validation.
