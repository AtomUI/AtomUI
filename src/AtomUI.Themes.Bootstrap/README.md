# AtomUI.Themes.Bootstrap

Status: implemented and validated locally; not published to NuGet.

Design language: `Bootstrap`; brand color is `Fixed` (all four profiles hand-write the focus halo `ColorControlOutline`, which would stay blue after a brand change).

A compiled token preset approximating Bootstrap 6.0 through AtomUI's existing Ant Design control structure. Registration preserves the `DaybreakBlue` default.

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

There is no automatic runtime profile dispatch. Applying Dark or Compact algorithms to `Bootstrap` alone retains its explicit Map/Alias pins; it is not equivalent to selecting the named profile. Dark profiles use the Bootstrap 6 dark body `#080A0C` with `#F1F2F3` body text, retaining the same blue seed; the existing Dark algorithm derives primary/status state colors. Compact profiles use the existing Compact algorithm to reduce height 38 to 34, pin SM/LG heights 32/44 down to 28/40, and reduce horizontal content padding 12 to 8. They retain font size, radius and outline width.

## Mapping and sources

Values are read from the official Bootstrap 6.0 documentation stylesheet (`getbootstrap.com/docs/6.0/dist/css/bootstrap.min.css`, v6.0.0-alpha, checked 2026-11-15); see the [theme utilities](https://getbootstrap.com/docs/6.0/utilities/theme/) page. Bootstrap 6 defines colors in OKLCH and switches light/dark with `light-dark()`; each value below is the sRGB color Chromium renders for it (out-of-gamut chroma is clipped), stored as a fixed hex value.

| AtomUI token | Bootstrap 6 variable | Light | Dark |
|---|---|---|---|
| `ColorPrimary` | `--bs-primary-base` (blue-500) | `#0087FE` | same |
| `ColorSuccess` | `--bs-success-base` (green-500) | `#00B15A` | same |
| `ColorWarning` | `--bs-warning-base` (yellow-500) | `#FFC900` | same |
| `ColorError` | `--bs-danger-base` (red-500) | `#E62845` | same |
| `ColorInfo` | `--bs-info-base` (cyan-500) | `#00B4F8` | same |
| `ColorText` / `ColorTextBase` | `--bs-fg-body` | `#171A1D` | `#F1F2F3` |
| `ColorBgContainer` / `ColorBgBase` / `ColorBgElevated` | `--bs-bg-body` (cards, menus and popovers use it too) | `#FFFFFF` | `#080A0C` |
| `ColorBgLayout` | `--bs-bg-1` | `#F6F7F8` | `#0F1214` |
| `ColorBorder` | `--bs-border-color` (form controls, cards) | `#C6CBCF` | `#444B51` |
| `ColorBorderSecondary` | `--bs-border-subtle` | `#D4D8DB` | `#222629` |
| `ColorControlOutline` | `--bs-primary-focus-ring` | `#6FC8FF` | `#0064BA` |
| `ControlOutlineWidth` | `--bs-focus-ring-width` | `3` | same |
| `ControlHeight` / `SM` / `LG` | `--bs-btn-input-min-height` (default / sm / lg) | `38` / `32` / `44` | same |
| `BorderRadius` / `BorderRadiusLG` | `--bs-btn-input-border-radius` (radius-5) / lg button, card, menu (radius-7) | `8` / `12` | same |
| `PaddingContentHorizontal` | `--bs-btn-input-padding-x` | `12` | same |
| `FontSize` | `--bs-body-font-size` | `16` | same |

`BorderRadiusSM` stays derived (6); it is close to the Bootstrap 6 checkbox radius (33% of 20px) used by AtomUI CheckBox, whereas small buttons use 8. White selected CheckBox marks and RadioButton dots remain appropriate against the blue primary; no per-control overrides are needed.

The Bootstrap 6 focus ring is opaque: `color-mix(in oklch, blue-500 50%, body)` in Light and 75% in Dark, drawn 3px wide with a 1px offset. AtomUI uses the resolved opaque color and width 3 through its existing focus mechanism, so the offset is not reproduced.

Bootstrap 6 itself pairs white contrast text with blue-500, which is about 3.6:1. The preset keeps that pairing; it meets the WCAG 3:1 floor for UI components but not 4.5:1 for body text.

## 不支持 / Unsupported

- Gradient backgrounds (渐变背景).
- Inner shadows (内阴影).
- Bootstrap 6 layered shadows and translucent popup borders; existing AtomUI shadow tokens remain in use.
- Dark text on warning/info solid backgrounds (`--bs-warning-contrast`, `--bs-info-contrast`); AtomUI has a single `ColorTextLightSolid`.
- The `accent`, `inverse` and `secondary` theme colors and their subtle/muted variants.
- Exact Bootstrap focus-ring shape/offset, web layout, component structure and pixel equivalence. Existing AtomUI templates, interactions and shadow rendering remain in use.
