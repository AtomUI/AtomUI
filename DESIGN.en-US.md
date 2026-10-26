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
  error-border-hover: '#FFA39E'
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

### Dark visual reference

This table comes from an actual `DaybreakBlue + Default + Dark` snapshot for static designs and previews, using CSS colors. Applications continue to consume dark themes through algorithms and tokens; composite translucent text over the actual surface.

| Token | Light | Dark |
| --- | --- | --- |
| `ColorBgLayout` | `#F5F5F5` | `#000000` |
| `ColorBgContainer` | `#FFFFFF` | `#141414` |
| `ColorBgElevated` | `#FFFFFF` | `#1F1F1F` |
| `ColorText` | `#000000E0` | `#FFFFFFD8` |
| `ColorTextSecondary` | `#000000A5` | `#FFFFFFA5` |
| `ColorTextDisabled` | `#0000003F` | `#FFFFFF3F` |
| `ColorBorder` | `#D9D9D9` | `#424242` |
| `ColorBorderSecondary` | `#F0F0F0` | `#303030` |
| `ColorPrimary` | `#1677FF` | `#1668DC` |

## Typography

The body baseline is 14 DIP, with weight 400 for ordinary body text and controls and weight 600 for title-level emphasis.
Selection primarily uses existing colors, indicators, and control states; individual controls may use their defined selected weight, as Select options do.

```yaml
typography:
  display-lg: { fontFamily: &ui-font 'Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, sans-serif', fontSize: 38px, fontWeight: 600, lineHeight: 46px }
  headline-lg: { fontFamily: *ui-font, fontSize: 30px, fontWeight: 600, lineHeight: 38px }
  headline-md: { fontFamily: *ui-font, fontSize: 24px, fontWeight: 600, lineHeight: 32px }
  headline-sm: { fontFamily: *ui-font, fontSize: 20px, fontWeight: 600, lineHeight: 28px }
  title-lg: { fontFamily: *ui-font, fontSize: 16px, fontWeight: 600, lineHeight: 24px }
  title-md: { fontFamily: *ui-font, fontSize: 14px, fontWeight: 600, lineHeight: 22px }
  body-lg: { fontFamily: *ui-font, fontSize: 16px, fontWeight: 400, lineHeight: 24px }
  body-md: { fontFamily: *ui-font, fontSize: 14px, fontWeight: 400, lineHeight: 22px }
  body-sm: { fontFamily: *ui-font, fontSize: 12px, fontWeight: 400, lineHeight: 20px }
```

The nine typography roles reuse the complete family chain through `&ui-font` / `*ui-font` within one YAML block; after parsing, every role still has its own `fontFamily` value.

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
  SizeUnit: 4px
  SizeXXS: 4px
  SizeXS: 8px
  SizeSM: 12px
  Size: 16px
  SizeMD: 20px
  SizeLG: 24px
  SizeXL: 32px
  SizeXXL: 48px
  ControlHeightSM: 24px
  ControlHeight: 32px
  ControlHeightLG: 40px
```

YAML keys use actual token names. Consume them through the table below instead of guessing values from SM / XS suffixes.

| YAML key | AtomUI Token | Default (DIP) |
| --- | --- | --- |
| `spacing.SizeUnit` | `SizeUnit` | 4 |
| `spacing.SizeXXS` | `SizeXXS` | 4 |
| `spacing.SizeXS` | `SizeXS` | 8 |
| `spacing.SizeSM` | `SizeSM` | 12 |
| `spacing.Size` | `Size` | 16 |
| `spacing.SizeMD` | `SizeMD` | 20 |
| `spacing.SizeLG` | `SizeLG` | 24 |
| `spacing.SizeXL` | `SizeXL` | 32 |
| `spacing.SizeXXL` | `SizeXXL` | 48 |
| `spacing.ControlHeightSM` | `ControlHeightSM` | 24 |
| `spacing.ControlHeight` | `ControlHeight` | 32 |
| `spacing.ControlHeightLG` | `ControlHeightLG` | 40 |
Application layouts consume appropriate tokens such as `UniformlyPadding*` and `UniformlyMargin*`; internal control geometry uses the corresponding Own Tokens.

32/24/40 are the standard height baselines from `ControlHeight` / `ControlHeightSM` / `ControlHeightLG`. Controls may use `MinHeight`,
and content or layout constraints can affect their final size. Choose control size APIs instead of fixing every height to 32. Border compensation produces meaningful
padding values such as 7, 11, and 15; preserve those derivations. A 4 DIP rhythm does not require every template coordinate to be divisible by 4.

Page backgrounds, content containers, and floating surfaces use `ColorBgLayout`, `ColorBgContainer`, and `ColorBgElevated`, respectively. The latter two are both white in the light theme,
but retain their separate roles for dark themes, branding, and elevation. Adapt responsive structure to available space and tasks using existing breakpoint tokens;
do not impose Web page widths or Gallery showcase layouts on desktop windows.

### Layout controls and page patterns

`Form` / `FormItem` own fields, labels, validation, and submission; `Space` arranges gaps within a control group; `FlexPanel` handles horizontal distribution, wrapping, and alignment; `Row` / `Col` provide a 24-column grid. Use Avalonia `Grid` star-sized rows and columns for remaining page space, giving scrollable regions a defined available size.

Place these AXAML fragments in a View's content area. Declare `xmlns="https://github.com/avaloniaui"`, `xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"`, and `xmlns:atom="https://atomui.net"` on its root and specify the ViewModel through `x:DataType`. Applications supply the bindings and handle business operations after submission.

#### Form page

Use `FormLayout="Vertical"` to place labels with their fields; choose `Horizontal` for wider forms or `Inline` for short filter bars. Put validation rules in `FormItem.Validators` and keep errors beside their fields. Preserve entered values and let users locate the first error. Prefer Form's own spacing tokens instead of hard-coding 8 or 24 on every field.

```xml
<atom:Form FormLayout="Vertical" ValidateTrigger="OnSubmit">
    <atom:FormItem LabelText="显示名称" FieldName="displayName" IsRequired="True">
        <atom:FormItem.Validators>
            <atom:FormValidatorProvider>
                <atom:FormStringNotEmptyValidator Message="请输入显示名称" />
            </atom:FormValidatorProvider>
        </atom:FormItem.Validators>
        <atom:LineEdit Text="{Binding DisplayName, Mode=TwoWay}" />
    </atom:FormItem>
    <atom:FormItem>
        <atom:SubmitButton Content="保存" ButtonType="Primary" />
    </atom:FormItem>
</atom:Form>
```

#### Toolbar and data grid

Put search, filters, and the list's main action in the toolbar; let the grid occupy remaining height. The grid owns its scrolling region; avoid another unbounded scrolling container in the same direction. Wrap the toolbar when space is limited. Enable the independent package with `UseDesktopDataGrid()` first. `DataSource` must be an `IDataGridSource`; adapt local collections with `DataGridLocalSource.Create(...)` rather than assigning ordinary collections to `DataGrid.ItemsSource`. See [DataGrid documentation](docs/controls/desktop/data-display/data-grid/overview.md) for sources, columns, and AOT data contracts.

```xml
<Grid RowDefinitions="Auto,*" RowSpacing="{atom:SharedTokenResource Spacing}">
    <atom:FlexPanel Direction="Row" Wrap="Wrap" AlignItems="Center"
                    ColumnSpacing="{atom:SharedTokenResource SpacingXS}"
                    RowSpacing="{atom:SharedTokenResource SpacingXS}">
        <atom:LineEdit MinWidth="160" atom:Flex.Grow="1" PlaceholderText="搜索" />
        <atom:Button Content="新建" Color="Primary" Variant="Solid" />
    </atom:FlexPanel>
    <atom:DataGrid Grid.Row="1" ItemsSource="{Binding DataSource}"
                   AutoGenerateColumns="True" />
</Grid>
```

#### Master-detail layout

Place a list and detail area side by side in the same window, with selection driving the details. Provide a clear empty state before selection, and preserve relevant filter and scroll context when records change. For narrow windows, stack the regions or navigate from the list to details instead of shrinking both columns beyond readability. This example illustrates the two-column structure without prescribing a fixed window width.

```xml
<Grid ColumnDefinitions="*,*" ColumnSpacing="{atom:SharedTokenResource SpacingLG}">
    <atom:ListBox ItemsSource="{Binding Items}"
                  SelectedItem="{Binding SelectedItem, Mode=TwoWay}" />
    <atom:Card Grid.Column="1" Header="详情">
        <TextBlock Text="{Binding SelectedItem}" TextWrapping="Wrap" />
    </atom:Card>
</Grid>
```

#### Settings page

Group settings by task and use Form to align labels and fields. Make immediate application versus explicit saving clear: this example submits through “保存设置”. If changed to an immediate switch, the application must handle persistence failures with item-level feedback and state restoration. Do not combine conflicting autosave behavior and unsaved-change indicators.

```xml
<atom:Form FormLayout="Horizontal">
    <atom:FormItem LabelText="接收通知" FieldName="notifications">
        <atom:ToggleSwitch IsChecked="{Binding NotificationsEnabled, Mode=TwoWay}" />
    </atom:FormItem>
    <atom:FormItem LabelText="显示名称" FieldName="displayName">
        <atom:LineEdit Text="{Binding DisplayName, Mode=TwoWay}" />
    </atom:FormItem>
    <atom:FormItem>
        <atom:SubmitButton Content="保存设置" ButtonType="Primary" />
    </atom:FormItem>
</atom:Form>
```

### Page acceptance rules

- Keep one main action within a single clear decision; independent task areas may each have their own main action.
- At standard density, use `SpacingXS` (8) within small groups and `Spacing` (16) or `SpacingLG` (24) between ordinary groups as appropriate. Read tokens so compact mode and theme overrides remain effective.
- Use Form layouts and tokens for labels, fields, and errors; focus or scrolling must reach invalid items after validation fails.
- After narrowing the window or increasing text size, main actions remain reachable, scrollbars remain visible, and fixed heights do not clip long labels or errors.
- Preserve keyboard opening, closing, and focus return in menus, and owner/placement contracts in overlays. Hover effects must not replace keyboard focus.
- Reuse default control motion. With `EnableMotion` disabled, layout and state changes remain complete and the interface does not require animation completion to become usable.

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
  BorderRadiusXS: 2px
  BorderRadiusSM: 4px
  BorderRadius: 6px
  BorderRadiusLG: 8px
```

| YAML key | AtomUI Token | Default (DIP) |
| --- | --- | --- |
| `rounded.BorderRadiusXS` | `BorderRadiusXS` | 2 |
| `rounded.BorderRadiusSM` | `BorderRadiusSM` | 4 |
| `rounded.BorderRadius` | `BorderRadius` | 6 |
| `rounded.BorderRadiusLG` | `BorderRadiusLG` | 8 |
| `rounded.none` | Square-corner constant; no token | 0 |

`none` means square corners. Circular avatars, status dots, and circular buttons use their control's shape API; an extremely large radius is not a universal runtime token.
Keep shapes coherent within a group and express branding through the base radius and control capabilities. Individual controls may have independent radius tokens, as ToolTip does.

## Components

This section uses representative controls and states to illustrate AtomUI's visual and interaction rules; see [control documentation](docs/controls/desktop/overview.md) for the full control catalog and detailed contracts.

The control visual roles below record default light appearances. YAML `components` is a standard format field whose entries describe controls.
Only properties confirmed by current tokens and theme assets are listed; controls handle omitted properties.
`padding` uses CSS top/right/bottom/left order; convert it to Avalonia left/top/right/bottom order before copying it into AXAML.

```yaml
components:
  button-primary: { backgroundColor: '{colors.primary}', textColor: '{colors.on-primary}', typography: '{typography.body-md}', rounded: '{rounded.BorderRadius}', height: '{spacing.ControlHeight}', padding: 4px 15px }
  button-primary-hover: { backgroundColor: '{colors.primary-hover}' }
  button-primary-active: { backgroundColor: '{colors.primary-active}' }
  button-default: { borderColor: '{colors.outline}', borderWidth: 1px, backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', typography: '{typography.body-md}', rounded: '{rounded.BorderRadius}', height: '{spacing.ControlHeight}', padding: 4px 15px }
  button-default-hover: { textColor: '{colors.primary-hover}', borderColor: '{colors.primary-hover}' }
  button-default-active: { textColor: '{colors.primary-active}', borderColor: '{colors.primary-active}' }
  button-default-disabled: { borderColor: '{colors.outline}', backgroundColor: '{colors.disabled-fill}', textColor: '{colors.on-surface-disabled}' }
  line-edit: { borderColor: '{colors.outline}', borderWidth: 1px, backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', typography: '{typography.body-md}', rounded: '{rounded.BorderRadius}', height: '{spacing.ControlHeight}' }
  select: { borderColor: '{colors.outline}', borderWidth: 1px, backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', typography: '{typography.body-md}', rounded: '{rounded.BorderRadius}', height: '{spacing.ControlHeight}', padding: 4px 11px }
  select-option-selected: { backgroundColor: '{colors.primary-container}', textColor: '{colors.on-surface}', typography: '{typography.title-md}' }
  card: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', rounded: '{rounded.BorderRadiusLG}', padding: 12px }
  card-large: { backgroundColor: '{colors.surface}', textColor: '{colors.on-surface}', rounded: '{rounded.BorderRadiusLG}', padding: 24px }
  dialog: { backgroundColor: '{colors.surface-elevated}', textColor: '{colors.on-surface}', rounded: '{rounded.BorderRadiusLG}' }
  nav-menu-inline-item-selected: { backgroundColor: '{colors.primary-container}', textColor: '{colors.primary}', typography: '{typography.body-md}' }
  tab-control-tab-active: { textColor: '{colors.primary}', typography: '{typography.body-md}' }
  data-grid-header: { backgroundColor: '{colors.surface-container}', textColor: '{colors.on-surface}', typography: '{typography.title-md}', padding: 16px }
  tag: { backgroundColor: '{colors.surface-layout}', textColor: '{colors.on-surface}', typography: '{typography.body-sm}', rounded: '{rounded.BorderRadiusSM}', padding: 0px 7px }
  tool-tip: { backgroundColor: '{colors.inverse-surface}', textColor: '{colors.inverse-text}', typography: '{typography.body-md}', rounded: '{rounded.BorderRadiusSM}', padding: 8px 12px }
  menu-item-hover: { backgroundColor: '{colors.hover-fill}', textColor: '{colors.on-surface}' }
  alert-success: { backgroundColor: '{colors.success-container}', textColor: '{colors.on-surface}', rounded: '{rounded.BorderRadiusLG}', padding: 8px 12px }
  alert-warning: { backgroundColor: '{colors.warning-container}', textColor: '{colors.on-surface}', rounded: '{rounded.BorderRadiusLG}', padding: 8px 12px }
  alert-error: { backgroundColor: '{colors.error-container}', textColor: '{colors.on-surface}', rounded: '{rounded.BorderRadiusLG}', padding: 8px 12px }
  alert-info: { backgroundColor: '{colors.info-container}', textColor: '{colors.on-surface}', rounded: '{rounded.BorderRadiusLG}', padding: 8px 12px }
  dot-badge-error: { backgroundColor: '{colors.error}', width: 6px, height: 6px }
  tag-blue: { backgroundColor: '{colors.primary-container}', textColor: '{colors.primary-active}', typography: '{typography.body-sm}', rounded: '{rounded.BorderRadiusSM}', padding: 0px 7px }
  line-edit-hover: { borderColor: '{colors.primary-hover}' }
  line-edit-focus: { borderColor: '{colors.primary}', boxShadow: '0 0 0 2px #0591FF19' }
  line-edit-error: { borderColor: '{colors.error}' }
  line-edit-error-focus: { borderColor: '{colors.error}', boxShadow: '0 0 0 2px #FF26050F' }
  select-hover: { borderColor: '{colors.primary-hover}' }
  select-focus: { borderColor: '{colors.primary}', boxShadow: '0 0 0 2px #0591FF19' }
  select-error: { borderColor: '{colors.error}' }
  select-error-focus: { borderColor: '{colors.error}', boxShadow: '0 0 0 2px #FF26050F' }
  line-edit-error-hover: { borderColor: '{colors.error-border-hover}' }
  select-error-hover: { borderColor: '{colors.error-border-hover}' }
```

### Borders and interaction states

These states describe the default Outlined appearance of `LineEdit` and `Select`; Filled, Borderless, and Underlined variants use their own themes. State entries override selected properties of the base appearance; unlisted properties retain their base visuals.

`borderColor`, `borderWidth`, and `boxShadow` are preserved DESIGN.md extension properties that produce lint warnings; rendering support varies by consumer. Tools supporting them can read them directly; otherwise use the table below and actual control themes. YAML shadows use CSS syntax; runtime code continues to consume the corresponding tokens.

| Appearance / state | AtomUI source and behavior |
| --- | --- |
| Default border | `LineWidth` (1 DIP) and `ColorBorder`; Button uses Own Token `DefaultBorderColor` |
| Default button hover / pressed | Text and border change together through `DefaultHoverColor` / `DefaultHoverBorderColor` and `DefaultActiveColor` / `DefaultActiveBorderColor` |
| Input hover | `ColorPrimaryHover`, preserving content and size |
| Input focus | `ColorPrimary` + `InputActiveShadow`, with spread from `ControlOutlineWidth` (2 DIP) |
| Input error / error-hover / error-focus | `ColorError` / `ColorErrorBorderHover`; focused invalid fields use `InputErrorActiveShadow` |
| Default button disabled | `ColorTextDisabled`, `ColorBgContainerDisabled`, `BorderColorDisabled`; Text / Link variants retain their own transparent-background rules |
| Button keyboard focus | Show FocusVisual using `ColorFocusBorder` and `FocusVisualBorderThickness` (2 DIP); hover is not a substitute |

| Control | YAML entries | Visual and interaction rules |
| --- | --- | --- |
| `Button` | `button-*` | Use the primary color for the main action and reduce emphasis for supporting actions. Preserve default, hover, pressed, focus, disabled, and loading distinctions. Use `ButtonType` or `Color` / `Variant` APIs. |
| `LineEdit` | `line-edit*` | Keep inputs and buttons on a consistent scale. Borders, focus, and shadows identify the input location; labels, help text, and validation form a stable structure. Placeholder text does not communicate focus. |
| `Select` | `select*` | Single-selection triggers follow the input system. Selected and hovered options have different meanings; multiple selection uses the control's own tags, wrapping, and sizing rules. |
| `CheckBox` / `RadioButton` / `ToggleSwitch` | — | Distinguish multiple selection, exclusive selection, and immediate switching. Preserve checked, applicable indeterminate, keyboard-focus, and disabled feedback. |
| `Card` | `card` / `card-large` | Organize related information through header, body, and actions. Borders and variants express hierarchy; nested controls follow shared spacing. |
| `Dialog` family | `dialog` | Use elevated surfaces and masks to establish modality, with clear confirmation and cancellation. Window and overlay forms follow their own hosting and layout contracts. |
| `NavMenu` | `nav-menu-inline-item-selected` | Selection communicates the current location. Vertical/inline selection uses a pale primary background; horizontal navigation uses its indicator structure. Desktop command `Menu` / `ContextMenu` serves a separate purpose. |
| `TabControl` | `tab-control-tab-active` | Standard tabs use primary text and an indicator for the active state. `CardTabControl` has a separate card structure whose background must be preserved. |
| `DataGrid` | `data-grid-header` | Keep headers distinct from data rows, with separate sorting, selection, and hover states. Enable the independent DataGrid package as needed and follow its data and column APIs. |
| `Tag` / `DotBadge` and related controls | `tag*` / `dot-badge-error` | Tags express categories; badges express counts or status. Pair important states with understandable text instead of relying on colored dots alone. |
| `Alert` | `alert-*` | Status icons, pale backgrounds, and body text work together. Plain and description variants use their respective padding. |
| `ToolTip` / `Flyout`, `FlyoutHost` | `tool-tip` | Tooltips use inverse surfaces; the existing Flyout system hosts interactive content. Let controls handle placement, arrows, focus, and dismissal. |
| `MenuItem` and related command items | `menu-item-hover` | Hover identifies the pointer target while retaining readable text. Use existing keyboard and submenu behavior appropriate to the scenario. |

**Default control sizes and padding:**

- Card's large `BodyPaddingLG` is 24 DIP; default middle `BodyPadding` is 12, and small is 8.
- `ToolTip.ContentPadding` is 8 vertically and 12 horizontally in DIP.
- Button's default Own Token `ContentPadding` is AXAML `15,4,15,4`, or YAML `4px 15px`, including border and content-line-height compensation.
- Dialog window content padding is `24,16,24,16`; overlay content uses `24,0,24,0`, while headers and footers each use their own layout and tokens.
- Default Tag uses `ColorBgLayout`; categorical color variants use the corresponding palette. DataGrid defaults to Large, with composited `HeaderBg` and 16 DIP cell padding in the header.

Prefer existing control sizes and variants for new interfaces. Customize appearance through public properties, tokens, or dedicated Semantic Part Styles, and verify layout and interaction with real content.

### Padding order reference

A single value applies to all edges in both syntaxes. CSS pairs are vertical/horizontal; Avalonia pairs are horizontal/vertical. This table covers all non-uniform padding above; convert four values from CSS top/right/bottom/left to AXAML left/top/right/bottom.

| Control | CSS | AXAML |
| --- | --- | --- |
| `Button` | `4px 15px` | `15,4,15,4` |
| `Select` | `4px 11px` | `11,4,11,4` |
| `Tag` | `0px 7px` | `7,0,7,0` |
| `ToolTip` / `Alert` | `8px 12px` | `12,8,12,8` |

### Control selection and minimal usage

| Control | Implemented key properties and values |
| --- | --- |
| `Button` | `ButtonType="Default / Primary / Dashed / Link / Text"`; or `Color="Primary"` + `Variant="Solid / Outlined / Filled / Text / Link / Dashed"`; `SizeType="Large / Middle / Small"` |
| `LineEdit` | `Text`, `PlaceholderText`, `StyleVariant="Outlined"`, `SizeType="Middle"`; delegate form validation to `FormItem` |
| `Select` | `Mode="Single / Multiple / Tags"`, `OptionsSource`, `SelectedOption` / `SelectedOptions`; options follow the `ISelectOption` contract |
| `Form` / `FormItem` | `FormLayout="Horizontal / Vertical / Inline"`; `LabelText`, `FieldName`, `Validators`, `ValidateTrigger="OnChanged / OnBlur / OnSubmit"` |
| `Space` | `Orientation="Horizontal / Vertical"`, `SizeType="Small / Middle / Large"`; use `ItemSpacing` / `LineSpacing` for explicit gaps |
| `FlexPanel` | `Direction="Row / Column"`, `Wrap="NoWrap / Wrap / WrapReverse"`, `AlignItems="Center"`, `ColumnSpacing` / `RowSpacing` |
| `Row` / `Col` | `Gutter="16"`, `Span="12"`; responsive columns may use `Xs="24"`, `Md="12"`, within a 24-column grid |
| `DataGrid` | `ItemsSource : IDataGridSource`, `AutoGenerateColumns="True"`, `SizeType="Large"`; explicit columns and queries follow the dedicated data contracts |
| `ToggleSwitch` | Bind `IsChecked` to a Boolean setting; the application explicitly chooses batch saving or immediate application |

Slashes in this table list alternatives; supply one value per property. This button explicitly uses the primary solid variant and middle size. Do not combine ButtonType with conflicting Color/Variant settings.

```xml
<atom:Button Content="保存" Color="Primary" Variant="Solid" SizeType="Middle" />
```

This `Space` uses small spacing (8 DIP at standard density) while preserving each button's height and visual states.

```xml
<atom:Space Orientation="Horizontal" SizeType="Small">
    <atom:Button Content="取消" />
    <atom:Button Content="保存" ButtonType="Primary" />
</atom:Space>
```

These AXAML fragments use the root namespaces specified in Layout. See [Form](docs/controls/desktop/data-entry/form/overview.md), [Space](docs/controls/desktop/layout/space/overview.md), [FlexPanel](docs/controls/desktop/layout/flex-panel/overview.md), and [Grid](docs/controls/desktop/layout/grid/overview.md) documentation for more examples.

### Overlay and feedback selection

| Scenario | Choice and boundary |
| --- | --- |
| The current task requires confirmation, cancellation, or completed input | `Dialog`, preserving modal focus and dismissal contracts |
| Keep main-page context while viewing or editing longer details | `Drawer`, entering from an edge and giving content a separate area |
| A brief confirmation next to its triggering action | `PopupConfirm`, such as confirming deletion of one record; use Dialog for complex input |
| Persistent explanation, warning, or error near a page region or field | `Alert`; prefer `FormItem` feedback for field validation |
| A short operation result, such as a successful save | Display a `Message` through `WindowMessageManager`; do not replace field errors with it |
| A non-modal notification with a title or detailed content | Display a `Notification` through `WindowNotificationManager` |

`Message` / `Notification` are feedback data objects, not same-named AXAML visual controls. Use the current window's managers or public owner-aware entry points; business handling and theme context follow Customization below.

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
