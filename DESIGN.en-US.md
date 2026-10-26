---
version: alpha
name: AtomUI
description: Design language for AtomUI controls on Avalonia and .NET, with semantic tokens, scoped themes, and typed customization.
---

# AtomUI Design Language

[简体中文（默认）](DESIGN.md) | [English](DESIGN.en-US.md)

## Overview

AtomUI is an Avalonia/.NET control library based on [Ant Design 6's design language](https://github.com/ant-design/ant-design/blob/master/DESIGN.md), serving information-dense applications such as forms, data tables, navigation, and workspaces.
This document describes AtomUI's visual rules, controls, and theme customization using the [Google DESIGN.md format](https://github.com/google-labs-code/design.md/blob/main/docs/spec.md), for designers, developers, and AI tools.

Four design values guide interface generation and review:

- **Natural**: Follow familiar interaction and reading patterns, with clear keyboard focus and appropriate pointer feedback on desktop.
- **Certain**: Make input, selection, loading, success, and failure states recognizable, so users understand outcomes and next steps.
- **Meaningful**: Use visual emphasis to communicate task priority; decoration should support information, actions, and state.
- **Growing**: Apply consistent rules across simple forms, complex data interfaces, and expanding business modules.

The YAML describes the
`DaybreakBlue + ThemeAlgorithm.Default` light baseline without application overrides, generally using controls' default middle size. Algorithms generate dark and compact modes.
The mobile controls package remains target architecture, outside this document's implemented capabilities.

YAML is a portable visual projection for design tools, **not an AtomUI theme configuration file**. Its `px` values correspond to Avalonia device-independent pixels
(DIP), rather than physical pixels; the platform and Avalonia handle scaling. Product code should consume tokens and control APIs.

## Colors

The primary brand color organizes main actions, links, selection, and focus; success, warning, error, and info colors communicate status. Preset palettes support categories and data visualization.
Keep a clear action hierarchy, and combine color with text, icons, or structure when communicating important state.

```yaml
colors:
  primary: '#1677FF'
  primary-hover: '#4096FF'
  primary-active: '#0958D9'
  primary-container: '#E6F4FF'
  success: '#52C41A'
  success-container: '#F6FFED'
  warning: '#FAAD14'
  warning-container: '#FFFBE6'
  error: '#FF4D4F'
  error-container: '#FFF2F0'
  info: '#1677FF'
  info-container: '#E6F4FF'
  surface: '#FFFFFF'
  surface-container: '#FAFAFA'
  surface-layout: '#F5F5F5'
  surface-elevated: '#FFFFFF'
  on-surface: '#000000E0'
  on-surface-variant: '#000000A5'
  on-surface-description: '#00000072'
  on-surface-disabled: '#0000003F'
  on-primary: '#FFFFFF'
  outline: '#D9D9D9'
  outline-variant: '#F0F0F0'
  hover-fill: '#0000000F'
  disabled-fill: '#0000000A'
  inverse-surface: '#000000D8'
  inverse-text: '#FFFFFF'
  mask: '#00000073'
```

| Document role | AtomUI token or calculation source |
| --- | --- |
| `primary` / `primary-hover` / `primary-active` / `primary-container` | `ColorPrimary` / `ColorPrimaryHover` / `ColorPrimaryActive` / `ColorPrimaryBg` |
| `success` / `warning` / `error` / `info` and their `*-container` roles | `ColorSuccess` / `ColorWarning` / `ColorError` / `ColorInfo` and corresponding `*Bg` tokens |
| `surface` / `surface-layout` / `surface-elevated` | `ColorBgContainer` / `ColorBgLayout` / `ColorBgElevated` |
| `surface-container` | The light solid color obtained by compositing `ColorFillAlter` over `ColorBgContainer`, used by DataGrid headers and similar surfaces |
| Four text levels | `ColorText` / `ColorTextSecondary` / `ColorTextDescription` / `ColorTextDisabled` |
| `outline` / `outline-variant` | `ColorBorder` / `ColorBorderSecondary` |
| `hover-fill` / `disabled-fill` | `ColorBgTextHover` / `ColorBgContainerDisabled` |
| `on-primary` / `inverse-text` | `ColorTextLightSolid` |
| `inverse-surface` / `mask` | `ColorBgSpotlight` / `ColorBgMask` |

Preserve alpha in translucent text and fills so they blend with tinted backgrounds. YAML uses CSS `#RRGGBBAA`; Avalonia eight-digit colors use
`#AARRGGBB`. For example, `#000000E0` corresponds to AXAML `#E0000000`. AtomUI stores opacity as bytes: the actual text alpha values are
224/255, 165/255, 114/255, and 63/255 for primary, secondary, description, and disabled text, respectively. Do not reconstruct seeds from displayed gray values.

Preset palette base colors: blue `#1677FF`, purple `#722ED1`, cyan `#13C2C2`, green `#52C41A`, magenta `#EB2F96`,
red `#F5222D`, orange `#FA8C16`, yellow `#FADB14`, volcano `#FA541C`, geekblue `#2F54EB`, gold `#FAAD14`, and lime `#A0D911`.
Use existing palettes and derived gradients for categories instead of adding arbitrary accent colors to individual pages.

Default brand colors do not guarantee WCAG AA for every text pairing; white text on `#1677FF` requires evaluation at the intended font size. For strict accessibility requirements,
adjust complete state-color families through global or Control tokens, and check text, focus, and error feedback on actual backgrounds. Format validation does not establish accessibility conformance.
Composite translucent foregrounds and backgrounds over the actual surface before calculating contrast; comparing RGB channels alone can produce false warnings. Description text and mask colors are general roles that are not individually referenced by every control field.

## Typography

The body baseline is 14 DIP, with weight 400 for ordinary body text and controls and weight 600 for title-level emphasis.
Selection primarily uses existing colors, indicators, and control states; individual controls may use their defined selected weight, as Select options do.

```yaml
typography:
  display-lg: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 38px, fontWeight: 600, lineHeight: 46px }
  headline-lg: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 30px, fontWeight: 600, lineHeight: 38px }
  headline-md: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 24px, fontWeight: 600, lineHeight: 32px }
  headline-sm: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 20px, fontWeight: 600, lineHeight: 28px }
  title-lg: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 16px, fontWeight: 600, lineHeight: 24px }
  title-md: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 14px, fontWeight: 600, lineHeight: 22px }
  body-lg: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 16px, fontWeight: 400, lineHeight: 24px }
  body-md: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 14px, fontWeight: 400, lineHeight: 22px }
  body-sm: { fontFamily: 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 12px, fontWeight: 400, lineHeight: 20px }
```

Heading sizes come from `FontSizeHeading1..5`; body sizes come from `FontSizeLG`, `FontSize`, and `FontSizeSM`; strong weight comes from `FontWeightStrong`.
These are role combinations, not additional AtomUI tokens. Do not create a second type scale merely to match names or force every control to use the same font size.

Runtime `FontFamily` starts with `fonts:AlibabaSans#Alibaba Sans` and ends with Avalonia `$Default`.
YAML projects this into portable family names and a `sans-serif` fallback. Alibaba Sans requires font-package registration; Chinese candidates in the default chain are system fonts,
so the chain does not imply an embedded Chinese font. For deterministic glyphs, register a font package explicitly and configure `FontFamily`; see [font-family resolution](docs/architecture/systems/typography/font-family-resolution.md).

`RelativeLineHeight*` is a multiplier, while Avalonia `TextBlock.LineHeight` is an absolute length. The table uses converted lengths; when consuming runtime tokens,
multiply the corresponding font size by its ratio. `FontHeight*` supplies rounded body-text heights. Changing `FontSize` re-derives the type scale; Dark does not change font sizes.
Current Compact preserves the base font size while reducing spacing and control heights; see [font tokens and derivation](docs/architecture/systems/typography/tokens-and-derivation.md) for the calculation rules.

## Layout

Use a 4 DIP rhythm and group by task relationships: keep labels near fields, related actions near content, and larger gaps between sections.
Alignment, grouping, and stable row heights keep dense data readable. Preserve scrolling, clipping, and overlay boundaries for long content; layout changes must maintain complete interaction behavior.

```yaml
spacing:
  unit: 4px
  xs: 4px
  sm: 8px
  step-12: 12px
  md: 16px
  step-20: 20px
  lg: 24px
  xl: 32px
  xxl: 48px
  control-height-sm: 24px
  control-height: 32px
  control-height-lg: 40px
```

`unit` maps to `SizeUnit`; 4/8/12/16/20/24/32/48 map to `SizeXXS`, `SizeXS`, `SizeSM`, `Size`, `SizeMD`, `SizeLG`, `SizeXL`, and `SizeXXL`.
Document `spacing.sm = 8` maps to `SizeXS`, while `SizeSM = 12` maps to `spacing.step-12`.
Application layouts consume appropriate tokens such as `UniformlyPadding*` and `UniformlyMargin*`; internal control geometry uses the corresponding Own Tokens.

32/24/40 are the standard height baselines from `ControlHeight` / `ControlHeightSM` / `ControlHeightLG`. Controls may use `MinHeight`,
and content or layout constraints can affect their final size. Choose control size APIs instead of fixing every height to 32. Border compensation produces meaningful
padding values such as 7, 11, and 15; preserve those derivations. A 4 DIP rhythm does not require every template coordinate to be divisible by 4.

Page backgrounds, content containers, and floating surfaces use `ColorBgLayout`, `ColorBgContainer`, and `ColorBgElevated`, respectively. The latter two are both white in the light theme,
but retain their separate roles for dark themes, branding, and elevation. Adapt responsive structure to available space and tasks using existing breakpoint tokens;
do not impose Web page widths or Gallery showcase layouts on desktop windows.

## Elevation & Depth

Ordinary content is grouped mainly through borders, spacing, and tonal contrast; popups, tooltips, and dialogs use shadows to express elevation.
Masks express modality. The following are the default snapshot values in Avalonia shadow syntax: offsets, blur, optional spread, and color.

| AtomUI token | Default value |
| --- | --- |
| `BoxShadows` | `0 6 16 #14000000, 0 3 6 -4 #1F000000, 0 9 28 8 #0D000000` |
| `BoxShadowsSecondary` | `0 6 16 #1A000000, 0 3 6 -4 #1F000000, 0 9 28 8 #12000000` |
| `BoxShadowsTertiary` | `0 1 2 #08000000, 0 1 6 -1 #05000000, 0 2 4 #05000000` |

AtomUI's Alias calculation produces these values. Controls such as Card may also define their own shadows and variants; use the corresponding control themes and tokens.
Use the framework's placement, focus, masks, and dismissal behavior for overlays. Avoid adding arbitrary shadows or ZIndex values to conceal hosting, hit-testing, or clipping problems.

Motion establishes continuity between states: `MotionDurationFast` is 100 ms, `MotionDurationMid` is 200 ms,
`MotionDurationSlow` is 300 ms, and `MotionDurationVerySlow` is 800 ms. Reuse curves and transitions provided by controls and MotionScene;
Choose existing motion implementations appropriate to the interaction, keeping timing and rhythm consistent for similar state changes.
`EnableMotion=false` reduces theme motion durations to zero; `EnableWaveSpirit` controls click waves.
Theme authors declare `Transitions` in ControlTheme Style Setters, following the [theme customization contract](docs/guides/theming/customization.md).

## Shapes

Ordinary inputs and buttons retain clear silhouettes, containers use a slightly larger radius, and small tags stay compact.

```yaml
rounded:
  none: 0px
  sm: 2px
  md: 4px
  DEFAULT: 6px
  lg: 8px
```

`DEFAULT` / `lg` / `md` / `sm` map to `BorderRadius` / `BorderRadiusLG` / `BorderRadiusSM` / `BorderRadiusXS`;
`none` means square corners. Circular avatars, status dots, and circular buttons use their control's shape API; an extremely large radius is not a universal runtime token.
Keep shapes coherent within a group and express branding through the base radius and control capabilities. Individual controls may have independent radius tokens, as ToolTip does.

## Components

This section uses representative controls and states to illustrate AtomUI's visual and interaction rules; see [control documentation](docs/controls/desktop/overview.md) for the full control catalog and detailed contracts.

The control visual roles below record default light appearances. YAML `components` is a standard format field whose entries describe controls.
Only properties confirmed by current tokens and theme assets are listed; controls handle omitted properties.
`padding` uses CSS top/right/bottom/left order; convert it to Avalonia left/top/right/bottom order before copying it into AXAML.

```yaml
components:
  button-primary: { backgroundColor: '{colors.primary}', textColor: '{colors.on-primary}', typography: '{typography.body-md}', rounded: '{rounded.DEFAULT}', height: '{spacing.control-height}', padding: 4px 15px }
  button-primary-hover: { backgroundColor: '{colors.primary-hover}' }
  button-primary-active: { backgroundColor: '{colors.primary-active}' }
  button-default: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', typography: '{typography.body-md}', rounded: '{rounded.DEFAULT}', height: '{spacing.control-height}', padding: 4px 15px }
  button-default-hover: { textColor: '{colors.primary-hover}' }
  button-disabled: { backgroundColor: '{colors.disabled-fill}', textColor: '{colors.on-surface-disabled}' }
  input-field: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', typography: '{typography.body-md}', rounded: '{rounded.DEFAULT}', height: '{spacing.control-height}' }
  select-field: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', typography: '{typography.body-md}', rounded: '{rounded.DEFAULT}', height: '{spacing.control-height}', padding: 4px 11px }
  select-option-selected: { backgroundColor: '{colors.primary-container}', textColor: '{colors.on-surface}', typography: '{typography.title-md}' }
  card: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 12px }
  card-large: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 24px }
  modal: { backgroundColor: '{colors.surface-elevated}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}' }
  menu-item-selected: { backgroundColor: '{colors.primary-container}', textColor: '{colors.primary}', typography: '{typography.body-md}' }
  tabs-tab-active: { textColor: '{colors.primary}', typography: '{typography.body-md}' }
  table-header: { backgroundColor: '{colors.surface-container}', textColor: '{colors.on-surface}', typography: '{typography.title-md}', padding: 16px }
  tag: { backgroundColor: '{colors.surface-layout}', textColor: '{colors.on-surface}', typography: '{typography.body-sm}', rounded: '{rounded.md}', padding: 0px 7px }
  tooltip: { backgroundColor: '{colors.inverse-surface}', textColor: '{colors.inverse-text}', typography: '{typography.body-md}', rounded: '{rounded.md}', padding: 8px 12px }
  dropdown-item-hover: { backgroundColor: '{colors.hover-fill}', textColor: '{colors.on-surface}' }
  alert-success: { backgroundColor: '{colors.success-container}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 8px 12px }
  alert-warning: { backgroundColor: '{colors.warning-container}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 8px 12px }
  alert-error: { backgroundColor: '{colors.error-container}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 8px 12px }
  alert-info: { backgroundColor: '{colors.info-container}', textColor: '{colors.on-surface}', rounded: '{rounded.lg}', padding: 8px 12px }
  badge-status-error: { backgroundColor: '{colors.error}', width: 6px, height: 6px }
  tag-blue: { backgroundColor: '{colors.primary-container}', textColor: '{colors.primary-active}', typography: '{typography.body-sm}', rounded: '{rounded.md}', padding: 0px 7px }
```

| Control | Visual and interaction rules |
| --- | --- |
| `Button` | Use the primary color for the main action and reduce emphasis for supporting actions. Preserve default, hover, pressed, focus, disabled, and loading distinctions. Use `ButtonType` or `Color` / `Variant` APIs. |
| `LineEdit` | Keep inputs and buttons on a consistent scale. Borders, focus, and shadows identify the input location; labels, help text, and validation form a stable structure. Placeholder text does not communicate focus. |
| `Select` | Single-selection triggers follow the input system. Selected and hovered options have different meanings; multiple selection uses the control's own tags, wrapping, and sizing rules. |
| `CheckBox` / `RadioButton` / `ToggleSwitch` | Distinguish multiple selection, exclusive selection, and immediate switching. Preserve checked, applicable indeterminate, keyboard-focus, and disabled feedback. |
| `Card` | Organize related information through header, body, and actions. Borders and variants express hierarchy; nested controls follow shared spacing. |
| `Dialog` family | Use elevated surfaces and masks to establish modality, with clear confirmation and cancellation. Window and overlay forms follow their own hosting and layout contracts. |
| `NavMenu` | Selection communicates the current location. Vertical/inline selection uses a pale primary background; horizontal navigation uses its indicator structure. Desktop command `Menu` / `ContextMenu` serves a separate purpose. |
| `TabControl` | Standard tabs use primary text and an indicator for the active state. `CardTabControl` has a separate card structure whose background must be preserved. |
| `DataGrid` | Keep headers distinct from data rows, with separate sorting, selection, and hover states. Enable the independent DataGrid package as needed and follow its data and column APIs. |
| `Tag` / `DotBadge` and related controls | Tags express categories; badges express counts or status. Pair important states with understandable text instead of relying on colored dots alone. |
| `Alert` | Status icons, pale backgrounds, and body text work together. Plain and description variants use their respective padding. |
| `ToolTip` / `Flyout`, `FlyoutHost` | Tooltips use inverse surfaces; the existing Flyout system hosts interactive content. Let controls handle placement, arrows, focus, and dismissal. |
| `MenuItem` and related command items | Hover identifies the pointer target while retaining readable text. Use existing keyboard and submenu behavior appropriate to the scenario. |

**Default control sizes and padding:**

- Card's large `BodyPaddingLG` is 24 DIP; default middle `BodyPadding` is 12, and small is 8.
- `ToolTip.ContentPadding` is 8 vertically and 12 horizontally in DIP.
- Button's default Own Token `ContentPadding` is AXAML `15,4,15,4`, or YAML `4px 15px`, including border and content-line-height compensation.
- Dialog window content padding is `24,16,24,16`; overlay content uses `24,0,24,0`, while headers and footers each use their own layout and tokens.
- Default Tag uses `ColorBgLayout`; categorical color variants use the corresponding palette. DataGrid defaults to Large, with composited `HeaderBg` and 16 DIP cell padding in the header.

Prefer existing control sizes and variants for new interfaces. Customize appearance through public properties, tokens, or dedicated Semantic Part Styles, and verify layout and interaction with real content.

## Do's and Don'ts

- **Do** organize tasks and information hierarchy before choosing controls. Emphasize one primary action for a clear decision and keep supporting actions subordinate.
- **Do** describe surfaces, text, borders, and state with role tokens; change a small seed set and let algorithms derive full scales.
- **Don't** duplicate light-theme brushes in pages, manually invert colors, or replace error/success semantics with unrelated palettes.
- **Do** preserve keyboard focus, disabled, loading, error, selection, and pointer feedback, including long text, scaling, and scrolling.
- **Don't** remove scrollbars, clipping boundaries, overlay placement, or existing interaction merely for visual consistency; do not communicate important information through color alone.
- **Do** use control sizes and derived spacing, maintain coherent adjacent radii and scales, and preserve meaningful border compensation.
- **Do** customize stable parts with generated dedicated Semantic Part Styles and explicit `x:SetterTargetType`.
- **Don't** find nodes by `PART_*`, incidental template structure, or Loaded callbacks and then set their visual properties. Use public theme entry points for complete structural changes.
- **Do** declare style and template bindings in ControlTheme when it can express them equivalently; put `Transitions` in Style Setters.
- **Don't** scan assemblies, reflect over token properties, or recalculate tokens during resource queries for customization. Preserve generated registration and AOT boundaries.
- **Do** treat YAML as a default visual reference, consume themes through AtomUI APIs, and verify against real content, states, and hosts.

## Customization

AtomUI provides seed derivation, control overrides, and nested themes through immutable `ThemeConfig`, Control configuration, and
`ThemeConfigProvider`. Avalonia compiles AXAML ControlThemes; runtime code computes tokens and publishes snapshots.
Packaged `.theme.xml` can generate typed declarations, while external dynamic XML uses a restricted Reader/Binder; both enter the same runtime calculation and commit path.

```text
ThemeDefinition + ThemeConfig + generated schema
  -> Normalize / Merge -> Seed / ordered algorithms / Map / Alias
  -> ThemeSnapshot -> scoped resource provider -> ControlTheme / visual states
  -> per-Control effective globals + Own Tokens (materialized on demand)
```

`ThemeSnapshot` is the sole source of resolved token values. Theme switching prepares candidate state before committing and notifying, keeping root and local scopes consistent.
Static templates, resource keys, and generated registrations remain stable during switching. See [theme runtime](docs/architecture/systems/theming/runtime.md) and [compiled theme packages](docs/architecture/systems/theming/compiled-theme-packages/overview.md) for the complete architecture.

### Choose a customization entry point

| Requirement | AtomUI entry point |
| --- | --- |
| Change branding, base typography, radii, and global states | `ThemeConfigBuilder.WithToken`, preferring Seed overrides |
| Dark, compact, and combined modes | `WithAlgorithms`; current algorithm identities are `Default`, `Dark`, and `Compact` |
| Change only one Control family | `WithControl` + `ControlThemeConfigBuilder`, using generated `XxxTokens.Identity` |
| Create a local page or region theme | `ThemeConfigProvider.Config`, binding or replacing a complete immutable configuration |
| Adjust a stable visual part | Generated dedicated Semantic Part Styles, such as `ButtonContentStyle` and `CardBodyStyle` |
| Replace an entire style, template, or replaceable public child control | `ControlTheme` or the corresponding Semantic Part Theme property |

### Global configuration and algorithms

This code assumes an application initialized with `UseAtomUI`; `application` is the current `Application`. Initiate the request on the UI thread.

```csharp
using AtomUI;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;

var config = new ThemeConfigBuilder()
    .WithToken(nameof(DesignToken.ColorPrimary), "#00B96B")
    .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark, ThemeAlgorithm.Compact)
    .Build();
var result = await application.GetThemeManager()!.ApplyThemeAsync(
    new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, config, ThemeTransitionReason.UserRequest));
// 由调用方处理 result；失败不能假定主题已经切换。
```

Tokens merge by key; algorithm lists are **replaced as a whole**. A local Compact-only list does not automatically retain its parent's Dark algorithm; specify the complete combination.
Mutating a configuration collection in place does not trigger updates; construct and replace the complete value. Local `Inherit=false` returns to library defaults and cuts off parent configuration and selected-theme inheritance.
For root requests, `Inherit=false` retains library defaults and the selected theme definition, dropping only lower-priority application initial overrides.

### Control overrides

```csharp
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;

var buttonConfig = new ThemeConfigBuilder()
    .WithControl(ButtonTokens.Identity, new ControlThemeConfigBuilder()
        .WithAlgorithm(ControlAlgorithmMode.Global)
        .WithToken(nameof(DesignToken.ColorPrimary), "#00B96B")
        .Build())
    .Build();
```

This configuration changes only the effective tokens of the Button identity; it does not create a theme scope for the Button content subtree. Controls can override their Own Tokens
and any registered Global Token. Confirm Own Token names through generated `XxxTokenKey`, control token documentation, and schema.
`Unspecified` inherits the parent policy and ultimately defaults to `Disabled`. `Disabled` does not re-derive Map/Alias values when Control seeds are overridden.
`Global` re-derives using the scope's algorithms; `Custom` uses an explicit ordered algorithm chain, currently composed from the same built-in algorithm identities listed above.

### Local scopes and resource consumption

The following AXAML fragments share `xmlns="https://github.com/avaloniaui"`, `xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"`,
and `xmlns:atom="https://atomui.net"` declarations on the View root. Compiled bindings use the containing View's `x:DataType`. `Config` binds to an immutable configuration property on its ViewModel:

```xml
<atom:ThemeConfigProvider Config="{Binding LocalThemeConfig}">
    <atom:Button Content="保存" ButtonType="Primary" />
</atom:ThemeConfigProvider>
```

AXAML `{atom:SharedTokenResource ColorPrimary}` reads the current ThemeContext's global value.
`{atom:ButtonTokenResource ColorPrimary}` reads Button's effective global value; `ButtonTokenResource` also reads its Own Tokens.
The latter explicitly identifies the Control; identity cannot be inferred from `TargetType`, inheritance, or `BasedOn`.

Ordinary popups inherit themes through logical relationships; independent TopLevels require the framework to attach an explicit owner context. Use public feedback APIs with an owner.
Ownerless static Dialog, Notification, and similar APIs use only the root theme; they cannot infer a local theme from the call site.

### Semantic Part customization

```xml
<atom:Card Header="账户信息">
    <atom:Card.Styles>
        <Style Selector="atom|Card">
            <atom:CardBodyStyle x:SetterTargetType="Border">
                <Setter Property="Padding" Value="24" />
            </atom:CardBodyStyle>
        </Style>
    </atom:Card.Styles>
    <TextBlock Text="账户内容" />
</atom:Card>
```

The dedicated Style explicitly applies 24 DIP body padding. The generator handles SelectorRoute across templates, logical descendants, or visual roots.
Applications access stable contracts through dedicated Styles instead of composing routes or manipulating target nodes in code-behind.
See the [Semantic Part system](docs/architecture/systems/theming/semantic-parts.md) for the contract and the [theme customization guide](docs/guides/theming/customization.md) for usage paths.

## Sources & Maintenance

The sources below own format, design language, and implementation facts. This document distills them for design and application generation; it does not replace full API or architecture documentation.
This document was checked on **2026-10-26**. `version: alpha` identifies the DESIGN.md format version, not AtomUI product maturity or package version.

| Subject | Authoritative source |
| --- | --- |
| Format and tooling rules | [Google DESIGN.md](https://github.com/google-labs-code/design.md), [format specification](https://github.com/google-labs-code/design.md/blob/main/docs/spec.md) |
| Default theme and seeds | [DaybreakBlue](src/AtomUI.Core/Assets/Themes/DaybreakBlue.theme.xml), [DesignToken](src/AtomUI.Core/Theme/DesignTokens/Definitions/DesignToken.cs), [Seed definitions](src/AtomUI.Core/Theme/DesignTokens/Definitions/DesignToken.Seed.cs) |
| Token derivation and value application order | [Algorithms](src/AtomUI.Core/Theme/Algorithms/DefaultThemeVariantCalculator.cs), [ThemeCompiler](src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs), [font derivation](docs/architecture/systems/typography/tokens-and-derivation.md) |
| Theme scopes, resources, and publication | [Theme system entry point](docs/architecture/systems/theming/overview.md), [resource host](docs/architecture/systems/theming/application-resource-host.md), [lazy materialization](docs/architecture/systems/theming/lazy-materialization.md) |
| Control visuals and Own Tokens | [Control documentation](docs/controls/desktop/overview.md) and the corresponding Token / Themes in `src/AtomUI.Desktop.Controls`; DataGrid is owned by its independent package |

Maintenance rules:

1. When default seeds, algorithms, fonts, control tokens, or theme setters change, recheck the values, mappings, and state descriptions here.
2. Design principles, tokens, and control theme rules must follow AtomUI's formal contracts. Assess layout, states, and compatibility when visual behavior changes.
3. Check derived values against actual snapshots and consumption against theme assets. Gallery application overrides must not replace the library-default baseline.
4. New examples use current public APIs, generated token entry points, and dedicated Semantic Part Styles, preserving NativeAOT compatibility.
5. Run `@google/design.md` lint for structure and references, check repository-relative links and example compilation, and review contrast warnings against actual backgrounds and states.
6. Chinese `DESIGN.md` is the default edition; maintain `DESIGN.en-US.md` alongside it. Keep sections, tokens, values, and code examples consistent between editions.
