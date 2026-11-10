# AtomUI.Themes.Bootstrap

A compiled token preset approximating Bootstrap 5.3 through AtomUI's existing Ant Design control structure. Registration preserves the `DaybreakBlue` default.

```csharp
using AtomUI.Themes.Bootstrap;
using AtomUI.Theme;

appBuilder.UseAtomUI(builder =>
{
    // Register the control packages your application uses separately.
    builder.Theme.UseBootstrapThemes();
    builder.Theme.WithInitialTheme("Bootstrap");
});
```

The package references Core only (plus the build-time generator analyzer). XML v1 is compiled to typed static registration at build time; startup does not read XML, scan assemblies, register controls or select vendor-specific algorithms.

## Appearance and density profiles

| Light | Dark | Light compact | Dark compact |
|---|---|---|---|
| `Bootstrap` | `Bootstrap.Dark` | `Bootstrap.Compact` | `Bootstrap.DarkCompact` |

Select the exact ID with `WithInitialTheme` or `ApplyThemeAsync(new ThemeRequest("Bootstrap.DarkCompact", null, ThemeTransitionReason.UserRequest))`. Gallery groups these using its existing generic profile suffix convention and compiled package origin.

There is no automatic runtime profile dispatch. Applying Dark or Compact algorithms to `Bootstrap` alone retains its explicit Map/Alias pins; it is not equivalent to selecting the named profile. Dark profiles use `#212529` containers with `#DEE2E6` body text, retaining the Bootstrap blue seed; the existing Dark algorithm derives primary/status state colors. Compact profiles use the existing Compact algorithm to reduce height 38 to 34 (derived SM 28.5 to 25.5, LG 47.5 to 42.5), and horizontal content padding 12 to 8. They retain font size, radius and outline width.

## Mapping and sources

The base pins primary `#0D6EFD`, success `#198754`, warning `#FFC107`, error `#DC3545`, font size 16, control height 38, radius 6 and outline width 4. These are token-level approximations rather than Bootstrap web component dimensions in every state. White selected CheckBox marks and RadioButton dots remain appropriate against the blue primary; no per-control overrides are needed.

Colors, light/dark neutral surfaces and radius follow the official [Bootstrap 5.3 CSS variables](https://getbootstrap.com/docs/5.3/customize/css-variables/) (v5.3.8, checked 2026-11-10). Layout uses the tertiary background; elevated surfaces use white in Light and the secondary background in Dark. Borders use the corresponding gray palette. Other states and semantic color variants use existing AtomUI algorithms.

The official [focus-ring variables](https://getbootstrap.com/docs/5.3/helpers/focus-ring/) use a .25rem width and primary `rgba(13,110,253,.25)`. AtomUI uses width 4 at font size 16 and Avalonia **AARRGGBB `#400D6EFD`**, with alpha rounded to 64/255. The draft plan's `#409DFF40` is corrected: Avalonia would interpret it as translucent green, which does not represent Bootstrap's blue focus ring. AtomUI keeps its existing focus mechanism, so the ring is an approximation.

## 不支持 / Unsupported

- Gradient backgrounds (渐变背景).
- Inner shadows (内阴影).
- Exact Bootstrap focus-ring shape/offset, web layout, component structure and pixel equivalence. Existing AtomUI templates, interactions and shadow rendering remain in use.
