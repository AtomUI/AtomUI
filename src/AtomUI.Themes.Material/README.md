# AtomUI.Themes.Material

Status: implemented and validated locally; not published to NuGet.

A compiled preset approximating baseline Material 3 colors, density and elevation through AtomUI's existing Ant Design controls. Registration preserves the `DaybreakBlue` default.

```csharp
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Themes.Material;
using AtomUI.Theme;

appBuilder.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.Theme.UseMaterialThemes();
    builder.Theme.WithInitialTheme("Material");
});
```

XML v1 is compiled to typed static registration at build time. Startup does not read XML, scan assemblies, register controls or select vendor-specific algorithms. The package references Core and Desktop Controls because Dark profiles need typed CheckBox and RadioButton overrides; the generator analyzer is build-only. Register the Desktop controls separately as shown above.

## Appearance and density profiles

| Light | Dark | Light compact | Dark compact |
|---|---|---|---|
| `Material` | `Material.Dark` | `Material.Compact` | `Material.DarkCompact` |

Select the exact ID using `WithInitialTheme` or `ApplyThemeAsync(new ThemeRequest("Material.DarkCompact", null, ThemeTransitionReason.UserRequest))`. Gallery uses its existing generic profile suffix convention and compiled package origin.

There is no automatic runtime profile dispatch. Applying Dark or Compact algorithms to `Material` alone retains its explicit Map/Alias pins; it is not equivalent to selecting the named profile. Named Dark profiles provide dark surfaces, light body text and dark on-primary text. Compact profiles reduce control height 40 to 36 through the existing Compact algorithm (derived SM 30 to 27, LG 50 to 45) and horizontal content padding 12 to 8; radius, font weight and shadows remain fixed.

## Mapping and sources

This preset deliberately uses the approved baseline palette and dimensions, rather than tracking later Material redesigns. Colors follow the official [Android Material 3 example](https://developer.android.com/develop/ui/views/theming/dynamic-colors#create_your_theme_with_tokens) and [Material 3 v0.161 reference palette](https://github.com/material-components/material-components-web/blob/master/packages/mdc-tokens/v0_161/_md-ref-palette.scss), with [role mappings](https://github.com/material-components/material-components-web/blob/master/packages/mdc-tokens/v0_161/_md-sys-color.scss) (checked 2026-11-10). The elevation mapping is an AtomUI approximation.

| AtomUI token | Light | Dark |
|---|---|---|
| `ColorPrimary` seed | `#6750A4` | `#D0BCFF` |
| `ColorTextLightSolid` | `#FFFFFF` | `#381E72` |
| `ColorBgBase`, `ColorBgContainer`, `ColorBgLayout` | `#FFFBFE` | `#1C1B1F` |
| `ColorTextBase`, `ColorText` | `#1C1B1F` | `#E6E1E5` |
| `ColorTextSecondary`, `ColorTextDescription` | `#49454F` | `#CAC4D0` |
| `ColorBgElevated` | `#FFFBFE` | `#313033` |
| `ColorBorder`, `ColorBorderSecondary` | `#79747E`, `#CAC4D0` | `#938F99`, `#49454F` |
| `BorderRadius`, `ControlHeight`, `FontWeightStrong` | `4`, `40`, `500` | same |
| `BoxShadowsSecondary` (approximate elevation 1) | `0 1 3 0 #33000000` | same |
| `BoxShadows` (approximate elevation 3) | `0 4 8 3 #26000000` | same |

Dark primary and related states are derived from the light-purple seed by AtomUI's existing Dark algorithm; they are not exact Material state-layer colors. Other semantic colors, disabled states, component-specific geometry and typography continue to use AtomUI defaults and algorithms. `FontWeightStrong` is a global strong-text token, not an override of every control's own font weight.

Dark CheckBox selected marks use a control-local `ColorWhite #381E72`, and RadioButton selected dots use own `RadioColor #381E72`, matching the on-primary role against the pale primary. Root `ColorWhite` remains white. Disabled marks retain AtomUI's disabled styling and application-local Foreground values retain priority.

## 不支持 / Unsupported

- Material ripple interaction (涟漪); AtomUI's existing wave behavior remains.
- Floating labels (浮动标签) and Material-specific field structure.
- Automatic all-uppercase text (全大写).
- Exact elevation layering, tonal overlays, Material state layers, component shapes or pixel equivalence. The two shadow tokens approximate elevations 1 and 3; they do not provide a complete elevation system.
