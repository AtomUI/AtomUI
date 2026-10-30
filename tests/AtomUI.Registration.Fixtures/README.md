# TypeMap product acceptance fixtures

`Minimal` runs the real AtomUI Button startup. `Complex` adds Expander string/non-string templates, an ordinary third-party package, constant external ResourceInclude, and independent typed registration triggers, boxed-enum local override/update/removal, and isolated OwnTokenKind/TokenKey/extension triggers. Source packages compile normally and contain no authored TypeMap attributes or control-selection data.

Default desktop runs use Avalonia.Headless for fast template assertions. Add `-p:FixtureRealDesktop=true` for the actual native OS backend and `Window.Show`/`Window.Close`. NativeAOT and size evidence must use that option. Both modes use the same ProductApp source, default AtomUI fonts and 600×500 window. `Browser` runs the complex contract through Avalonia.Browser and the automatic product targets.

Examples from the repository root:

```sh
dotnet run --project tests/AtomUI.Registration.Fixtures/Complex/Complex.csproj -c Release
LIBRARY_PATH=/opt/homebrew/lib:/opt/homebrew/opt/openssl@3/lib dotnet publish tests/AtomUI.Registration.Fixtures/Minimal/Minimal.csproj -c Release -r osx-arm64 --self-contained true -p:PublishAot=true -p:FixtureRealDesktop=true -o .artifacts/registration-product/minimal-native
.artifacts/registration-product/minimal-native/AtomUI.Registration.Fixtures
dotnet publish tests/AtomUI.Registration.Fixtures/Browser/Browser.csproj -c Release -p:RunAOTCompilation=false -o .artifacts/registration-product/browser
dotnet publish tests/AtomUI.Registration.Fixtures/Browser/Browser.csproj -c Release -p:RunAOTCompilation=true -o .artifacts/registration-product/browser-aot
```

The Homebrew path is an environment prerequisite on the tested Mac, not a product setting. Record system library deployment-target warnings; this command alone does not establish support for older macOS versions.

For the full-registration size baseline, build the ordinary products once, then use the fixture Inspector to generate a test-only root descriptor for all generated registration fragments:

```sh
dotnet run --project tests/AtomUI.Registration.Fixtures/Inspect/Inspect.csproj -- tests/AtomUI.Registration.Fixtures/Minimal/bin/Release/net10.0/osx-arm64 /absolute/path/full-registration.xml
```

Republish the exact same Minimal configuration with `-p:FixtureFullRegistrationDescriptor=/absolute/path/full-registration.xml`. That descriptor roots only generated fragment proxies; their actual Add methods retain their normal typed controls/assets. There is no shipped full-retention entry or consumer fallback switch. The selected/full comparison must keep SDK, RID, real-desktop option, font inputs, window/UI and native library paths identical.

For an optional compiler size profile, pass `-p:OptimizationPreference=Size` to **both** the selected and full publishes. This is the [official NativeAOT optimization preference](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/optimizing); it can trade throughput for executable size and does not change registration behavior. Keep default-profile measurements and execute both resulting programs. Do not disable globalization, localization, package-core services, or diagnostics to satisfy a size target.

The current post-compatibility-fix macOS 26.6.2 / SDK 10.0.300 / osx-arm64 measurement is 21,193,552 bytes selected versus 46,561,600 bytes full under the default profile, and 20,729,184 versus 45,682,624 bytes under `Size`. Only AlibabaSans is present; no Chinese font package is an input. Both exceed the 18 MiB absolute gate, although both satisfy the 40% relative reduction. These are current verification results, not a released size guarantee. The canonical acceptance status is [AOT and trimming](../../docs/architecture/foundations/aot-and-trimming.md#1-状态与事实边界).

The 2026-09-29 combined reflection-retention and short-key pass produced 20,182,000 / 45,001,952 bytes for selected/full under the default profile, a 55.15% selected reduction. `OptimizationPreference=Size` produced 19,717,728 / 44,139,696 bytes, a 55.33% selected reduction. Every binary reported `PRODUCT_PASS selected`. The selected executables remain 1,307,632 bytes (1.25 MiB) and 843,360 bytes (0.80 MiB), respectively, above the 18 MiB gate.

Size acceptance no longer uses an absolute MiB gate; the 18 MiB figures above are historical. `FluentBaseline` is the official Avalonia template (FluentTheme + Inter) with the same 600×500 window, StackPanel and Button, built with the same fixture settings and no AtomUI package. Publish it with exactly the Minimal command line (it needs no `FixtureRealDesktop` switch) and run it until it prints `BASELINE_PASS fluent`:

```sh
LIBRARY_PATH=/opt/homebrew/lib:/opt/homebrew/opt/openssl@3/lib dotnet publish tests/AtomUI.Registration.Fixtures/FluentBaseline/FluentBaseline.csproj -c Release -r osx-arm64 --self-contained true -p:PublishAot=true -o .artifacts/registration-product/fluent-baseline
```

The framework delta is Minimal selected minus FluentBaseline under one toolchain. The attribution, ratchet and mechanism-detection rules are owned by [AOT and trimming](../../docs/architecture/foundations/aot-and-trimming.md#81-体积评判标准).

The inspector's `selected` mode validates absence using metadata names, without rooting unused classes through `typeof`. Browser runtime success requires `PRODUCT_PASS browser` after the UI loads, not just a successful page load. Mono AOT may strip IL after the receipt gate; receipt byte hashes bind linked/pre-AOT files, while deployed metadata and runtime are checked separately.

`PackageConsumers/Author` proves ordinary authoring from the independently packed `AtomUI.Generator` package, including analyzer dependencies and generated package bootstrap. `PackageConsumers/Host` exercises direct, transitive and precompiled binary consumers; its own Directory.Build files intentionally do not import repository tooling. `NegativeBrowser` deliberately removes both converter and gate in its test project to demonstrate the Mono runtime failure; this is never a supported consumer configuration. `ZeroBrowser` verifies referenced-but-disabled packages produce a valid zero-slot link receipt.

Before a negative absence check, run Inspector's `pretrim` mode on the ordinary `ThirdParty/bin/Release/net10.0` output. It requires both unused types, their real conditional TypeMap entries, and their asset factories to exist. Then `selected` mode requires those types/factories to be absent from linked and deployed metadata.

## Reproduce ordinary package consumption from fresh outputs

Prerequisites: Python 3, the repository-pinned .NET SDK 10.0.300, network access to restore public NuGet dependencies, and the supported Browser/WASM workload (10.0.10). The default desktop RID is `osx-arm64`; pass the actual supported host RID explicitly when appropriate. No pre-existing repository `.artifacts`, local feed, package cache, or plan-workspace script is required.

From the repository root, choose an output directory that does **not** exist:

```sh
python3 tests/AtomUI.Registration.Fixtures/run_package_consumers.py \
  --output /tmp/atomui-package-consumers-fresh --rid osx-arm64 --serve
```

The runner stages the current Git-eligible source files without `.artifacts`, `bin`, or `obj`; the index is not modified. It builds the ordinary adapter/tool prerequisites, packs the nine products/tools into its own feed, writes a source-mapped `NuGet.Config`, and uses separate initially empty source and consumer package caches. It then builds the standalone Generator author and publishes/runs direct, transitive and ordinary binary desktop consumers. Browser publishing uses only package-delivered integration and verifies its required receipt.

With `--serve`, open the printed localhost URL in a browser. The browser sends a local pass signal **only after** the actual `COLD_BROWSER_PASS ordinary binary adapter template` assertion runs. The runner records success and shuts down its server. Without `--serve`, it explicitly records Browser runtime as pending, not passed. All commands, exit codes, package hashes/layout, source inventory, receipt and runtime status are saved beneath the chosen output. Nothing is published externally.

`PackageConsumers/` is deliberately outside the repository tooling imports. Its 20 source/configuration files must remain Git-eligible. The generated output/cache files remain ignored.

## Effective ILLink configuration regression

```sh
python3 tests/AtomUI.Registration.Fixtures/verify_link_signature.py \
  --output /tmp/atomui-link-signature-check
```

This supported-toolchain check exercises the actual preparation target with controlled options and item metadata. For every changed input it first proves an identical no-op preserves signature/receipt/semaphore, then requires the changed input to invalidate evidence. The source host/backend are built explicitly, so it does not depend on a pre-existing Debug generator bundle.

The Complex desktop/Browser fixture also creates NumericUpDown in Spinner mode and verifies its real inherited ButtonSpinner theme, application-level theme replacement and local customization. Minimal remains the same Button/Window size fixture.

Cold package consumption includes an ordinary author named-resource regression: removing its explicit ResourceInclude must fail normal compilation with located ATOMUIREG004; the unchanged explicit-include author then runs untrimmed and with full CoreCLR trimming. Metadata inspection confirms a separate unused exported control is present before trimming and absent afterwards. The same consumer uses a separate typed-key-only dependency; an IL check follows its compiled AXAML factory and requires the real provider ldtoken, and actual trim must preserve that provider without any provider typeof/constructor in host C#. This fixture adds no authored TypeMap or rooting descriptor.

The ordinary author also exports a desktop-only named resource through a real ResourceDictionary class carrying the standard platform attribute. Its portable AuthorControl remains available in Browser, while FixtureDesktopOverlay is absent; ordinary/fulltrim desktop verifies that the same resource class loads. Complex real-desktop mode uses an AtomUI Window with a title-bar AddOn and exercises the OTP internal text-box theme; Minimal size inputs remain unchanged.
