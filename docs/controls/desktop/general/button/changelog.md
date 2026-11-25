# Button Changelog

本文档记录 Button 控件级设计、API、主题契约、Token 和实现结构的变化。它用于维护控件设计历史，不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

记录原则：

- 只记录会影响 Button 设计理解、兼容边界、实现架构或维护方式的变化。
- 不记录临时讨论、纯格式化或没有长期价值的实现细节。
- 架构文档始终描述最新设计状态；历史变化记录在本文档。

## 2026-11-24

- API
  - `Button.ContentPadding` 成为可设置的标准属性，SubmitButton 和 ResetButton 继承该入口并使用各自的 Token 默认值。

## 2026-11-22

- Token
  - `ButtonThemeRecipe` 删除，它的开关改为 `AbstractButtonGeometryToken` 上的七个公开 Own Token：`RecipeVariants`、
    `OutlineOnlyDefaultShadow`、`DisabledByOpacity`、`DisabledOpacity`、`PressMoves`、`UnderlineLinkOnHover`、
    `UnderlineLinkAtRest`（见 [Button Token 设计](token.md) 2.8）。各设计语言的取值包在 Button 的控件推导里给它们赋值，不再经
    `AppearanceSettings` 传递对象；Button 在主题快照变化时读取。各设计语言渲染不变。

## 2026-11-20

- Token
  - `ButtonThemeRecipe` 拆为相互独立的开关：`UsesRecipeVariants`、`DisabledOpacity`、`PressMoves`、`UnderlineLinkOnHover`；Button 新增对应的内部直接属性 `DisablesWithOpacity`、`ThemeDisabledOpacity`、`PressMoves`、`UnderlinesLinkOnHover`，主题选择器按开关生效。无 recipe 的 Ant Design 与既有 Shadcn recipe 行为不变；PrimeNG recipe 关闭变体配方与按压位移，禁用透明度 0.6，链接悬停下划线。
- Performance
  - 去掉按钮的 10 个可继承中转属性。Button、SubmitButton、ResetButton 的主题把 Own Token 赋给按钮自身的
    普通属性，按钮主题用编译绑定和 `TemplateBinding` 读取，值不再沿按钮内容继承。渲染不变，每个按钮分配约少 13 KB（约 5%）。

- 改善 Shadcn 暗色 Button 的链接文字、危险描边与虚线变体在普通、悬停和按下状态的可读性。
- 按 Shadcn 各风格调整按钮阴影、键盘焦点环和禁用态。Vega 中性描边按钮使用 `shadow-xs`。实心、虚线及其余风格的中性描边按钮默认平贴。

## 2026-11-14

- Performance
  - Skip theme-token capture while the Button has no themed ancestor. Properties set from XAML (ButtonType, IsDanger, IsGhost, Color, Variant) arrive before the Button is attached; each used to throw and catch an `InvalidOperationException`. `OnAttachedToLogicalTree` still configures the variant variables, so the values are unchanged.
  - Bind the icon slot visibility to an internal `EffectiveIconVisible` property (`Icon is Icon && IsIconVisible && !IsLoading`, the old converter rule) instead of a three-way MultiBinding with reflection bindings, and the `ShadowsFrame` shadow setter to `{TemplateBinding VariantShadow}`. Same values and the same binding priorities; rendering is pixel-identical in every state.
- API
  - Desktop 和 Mobile 的 Button 共用 `AbstractButton` 的通用属性、伪类和颜色变体解析。四种 Button 枚举仍使用原 CLR 全名，由 Desktop 程序集转发到 `AtomUI.Controls`。
  - 新增 `ButtonColor.Success`、`ButtonColor.Warning` 和 `ButtonShape.Rectangular`。
- Theme
  - 新语义色使用全局状态色阶，矩形使用直角轮廓。
- Fix
  - 已挂载的按钮增删 `Icon` 后即时更新 `:icononly`。
- Desktop 影响
  - 旧枚举值及旧属性字段、方法和路由事件的签名保留。旧客户端可加载新版程序集。
- Tests
  - Button 及相关定向测试通过 136 项。Headless/Skia 对比正式 6.2.14 和候选版，浅色与深色的八种 Desktop 状态尺寸一致，差异位于两次采样也会变动的加载图标抗锯齿范围内。

## 2026-11-11

- Theme
  - 键盘焦点改用共享的 `FocusOutlineAdorner`（Ant Design `genFocusOutline`）：3px `ColorPrimaryBorder` 描边，向外偏移 1px，跟随圆角，且不被宿主裁剪；原全局方角 `DefaultFocusAdorner` 已移除。
  - 移除三套按钮模板与 DropdownButton 模板中画在边框内侧的 `Border#FocusVisual` 及其 `:focus-visible` 样式，`FocusAdorner` 不再置空；`Button` 通过 `IFocusOutlineShapeProvider` 提供 `EffectiveCornerRadius`，圆形 / 胶囊按钮的描边保持对应形状。
- Tests
  - `ButtonKeyboardNavigationTests` 改为断言键盘聚焦挂上 `FocusOutlineAdorner`；`ButtonFocusVisualRenderingTests` 宿主补充 `VisualLayerManager`（AdornerLayer 由窗口视觉层提供）。

## 2026-11-10

- Theme：终端 Own 与实际 owner 对齐，共享计算不传播其他控件的覆盖；字段迁移与保留范围见 [Token 设计](token.md)。

## 2026-11-08

- Changed
  - `ButtonTheme`、`HyperLinkButtonTheme`、`IconButtonTheme`、`ToggleIconButtonTheme` 的全局 Token 改读各自的
    `XxxTokenResource`，`Controls[Button]` 等控件级覆盖对圆角、高度、图标尺寸、禁用色和动效开关生效；默认视觉不变。
  - 变体颜色与阴影改为绑定 ControlTheme 使用的同一组资源键（Own Token 的 `ButtonTokenKind`、Global Token 的有效值键），
    Application 与局部 Resources 覆盖、`ThemeConfigProvider` 覆盖都作用于变体颜色，资源变化后自动刷新；预设色仍来自快照色板。
  - `FontWeight` Own Token 类型改为 `FontWeight` 并驱动 Button 字重；`DefaultBorderColorDisabled` 用于 Default 颜色的禁用边框。
  - Circle / Round 形状的圆角改写入内部 `EffectiveCornerRadius`，不再以本地值覆盖公开 `CornerRadius`；SplitButton 的拼接圆角经内部
    `HostCornerRadius` 写入，内部 Button 的公开 `CornerRadius` 保留主题与用户值。
- Removed
  - 删除无消费者的 Own Token：`ExtraContentItemSpacing`、`ExtraContentMargin`、`ExtraContentMarginLG`、`ExtraContentMarginSM`、
    `GutterToFlyout`，以及 HyperLinkButton 的 `BorderColorDisabled`；`GroupBorderColor` 标为 Reserved。
- Fixed
  - HyperLinkButton 维护 `:icononly`、`:loading`、`:danger` 伪类，图标按钮内边距、加载图标与危险色生效。
- Tests
  - Button 家族 Token 生效测试（`ButtonFamilyEffectTests`）覆盖全部 Active Own Token 与消费的 Global Token，并验证
    `Controls[Button]` 与根 Token 的作用域隔离。

## 2026-08-31

- Fix
  - Remove Button-family Browser-only theme override assets and require Browser registration to use the shared Button, DropdownButton and IconButton theme assets.
  - Remove the Desktop control package asset-path selector that rewrote shared theme assets to `Themes/Browser/` overrides.
- Tests
  - Add regression coverage that rejects Button-family Browser theme override assets in source and generated theme manifests.
  - Add Desktop.Controls coverage that rejects Browser-specific theme asset folders while keeping exact Browser unsupported-control identity filtering.

## 2026-08-27

- API (Breaking)
  - Remove the public `CustomBackground` property, the internal `HasCustomBackground` flag and the template `CustomBackgroundLayer` overlay from Button and DropdownButton. The property duplicated the standard `Background` responsibility during the period when root customization was broken.
  - Migration: set the standard `Background` (or `BorderBrush`) directly on the button. Semantic change: a customized surface now keeps the custom value across hover, pressed and disabled states (matching inline style semantics), instead of fading out to the state background.
- Theme
  - Drop the custom background overlay layer and its selector rules from the Button family themes. The frame `Frame` renders `Background` / `BorderBrush` directly via `TemplateBinding`.
- Tests
  - Rewrite the custom-background gating tests into root surface contract tests: local `Background` (including gradients) renders on the frame and survives pointerover / pressed / disabled; wave brush keeps resolving from final visual properties; source contract asserts no `CustomBackground` remains in `Button.cs` or Button family themes.

## 2026-08-13

- Docs
  - Split the complete public Semantic Part contract into `semantic-part.md`; keep `overview.md` focused on the supported Part
    summary and `implementation.md` focused on marker-to-node mapping.

## 2026-08-12

- Fix
  - Resolve Button wave color from the final root `BorderBrush` and `Background` immediately before playback, so Theme state,
    Semantic root Style and ordinary user Style share one visual color source.
  - Reject transparent, white and non-solid final brushes as wave colors, clear stale local wave values when no valid color
    exists, and keep `CustomBackground` outside the wave color pipeline.
- Theme Contract
  - Use `MinHeight` rather than fixed `Height` for the Large, Middle and Small Button size baselines, allowing content and
    Semantic Part layout setters to expand the natural measured height.
  - Keep `SizeType=Custom` free of a preset height baseline while retaining Middle defaults for typography, padding, corner
    radius and icon metrics.
- Layout
  - Apply the Button owner layout constraints before deriving Circle and Round geometry so preset `MinHeight` cannot produce
    an ellipse or a vertically stretched icon-only Button.
- Docs
  - Define the diagnostic boundary between Semantic Style priority and cross-node layout constraints, and record the required
    verification matrix for content/icon layout setters across size, shape, loading and shared Button theme variants.
- Gallery
  - Add a deferred, full-width Button example that demonstrates object/function Semantic Part styling through owner-scoped
    selectors, including stable content colors across Button interaction states.

## 2026-08-11

- Theme Contract
  - Separate the `.semantic-*` selector identity from `ContractType`; use `x:SetterTargetType` as the AXAML Setter type context.
  - Define class activator cost as opt-in application styling cost and keep Button built-in themes free of Semantic Style rules.
  - Use static `Classes.semantic-*="True"` markers in shared Button templates while keeping application selectors unchanged.

## 2026-08-10

- Theme Contract
  - Add the Button Semantic Part contract with implicit `root`, `.semantic-icon`, and `.semantic-content`.
  - Define `icon` as `Control` / `Multiple`, covering both user and loading icon implementations.
  - Define `content` as `ContentPresenter` / `Single` and keep `root` free of a `.semantic-root` marker.
  - Apply the same marker contract to all shared Button template variants and register the generated descriptor statically.

## 2026-08-03

- Design
  - Define `IconWidthProperty` / `IconHeightProperty` and `IconWidth` / `IconHeight` as the Button-owned public icon sizing contract.
  - Define `TemplateBinding` from Button and DropdownButton user/loading icon parts to the owner sizing properties, replacing external deep-template sizing selectors.
  - Preserve existing visuals by keeping ordinary and non-loading icon-only icons on `IconSize*`, while reserving `OnlyIconSize*` for icon-only loading defaults.
  - Keep DropdownButton `OpenIndicator` sizing independent and keep SplitButton outside this composite-control API change.

## 2026-07-20

- Fix
  - Align the default Text hover background with `ColorFillTertiary`.
  - Make the default Text pressed background depend directly on `ColorFill` instead of the generic text-action alias.
  - Align the Danger Text hover background with `ColorErrorBg`.
- Refactor
  - Make `EffectiveColor + EffectiveVariant` and the resulting `Variant*` theme variables the single Button color-state owner.
  - Remove duplicated `ButtonType`, Danger and Ghost color matrices from desktop and Browser ControlThemes.
- Tests
  - Cover Default, Primary and Danger Text normal, hover and pressed colors.
  - Cover compatibility Button visual projection, Primary Text component-theme refresh, and the direct Default Text pressed-token dependency.

## 2026-06-26

- Docs
  - Add LLMS export source mapping to Button architecture documentation.
  - Document Button package, .NET namespace, AXAML namespace, Gallery path and stable status as LLMS metadata.
  - Add Button semantic parts for root, wave, shadow, surface, custom background, content layout, loading icon, user icon and content.
  - Align Button LLMS export paths with `controls/button/index-cn.md` and `controls/button/semantic-cn.md`.
  - Clarify that `CustomBackgroundLayer` is an internal-stable semantic area for LLMS output, not a user customization template part.

## 2026-06-19

- Docs
  - Add `implementation.md` for Button internal state normalization, template integration, wave coordination and maintenance invariants.
  - Refactor `overview.md` to focus on design positioning, public contracts, behavior state, visual theme model and validation entry points.
  - Add Button implementation documentation to the General category entry.
  - Document the Button `SizeType=Custom` contract: Custom uses Middle defaults unless existing Button sizing properties are locally set, and does not introduce Button-specific `Custom*` metrics.
  - Clarify that Button themes must provide Custom size defaults at a priority that local `Height`, `Padding`, `FontSize` and related properties can override.
- API
  - Change Button family `SizeType` contracts for `Button`, `DropdownButton`, `SplitButton` and `HyperLinkButton` to `CustomizableSizeType`.
- Theme
  - Add `SizeType=Custom` selectors for Button, Browser Button, DropdownButton, SplitButton and HyperLinkButton themes, using Middle size token defaults.
  - Bind Button family frame heights to the owning control `Height` so local height values can override Custom defaults.
- Tests
  - Add Button custom size contract tests for API type, Custom default metrics and local property overrides.

## 2026-06-17

- Feature
  - Add the Button `Color + Variant` public API model with nullable `ButtonColor?` and `ButtonVariant?` properties.
  - Add the Button `CustomBackground` public API for controlled custom normal-state background overlays.
  - Add effective Button state normalization for compatibility with `ButtonType`, `IsDanger` and `IsGhost`.
  - Add internal StyledProperty theme variables for variant text, background, border and shadow states.
  - Define custom background overlays as visual-only Button surfaces that do not alter `Color + Variant` state, shadow, border, text brushes or wave brush.
  - Map preset colors through the active shared token palette instead of Button-private color tables.
  - Support colorful Button rendering in desktop and Browser Button themes.
  - Add a Gallery Color/Variant matrix example and document the new API rows.

- Fix
  - Keep the standard Button frame background under `CustomBackgroundLayer` to avoid a blank background flash when pointer hover leaves.
  - Render `CustomBackgroundLayer` above the Button frame so custom gradient backgrounds cover the normal-state border.

- Docs
  - Establish Button desktop architecture documentation under `docs/controls/desktop/general/button/overview.md`.
  - Add dedicated Button Token design documentation under `docs/controls/desktop/general/button/token.md`.
  - Introduce per-control changelog under `docs/controls/desktop/general/button/changelog.md`.
  - Adopt the per-control documentation directory structure for Button.
  - Define `Color + Variant` as the current Button design model while preserving `ButtonType` and `IsDanger` as compatibility entries.
  - Define the `CustomBackground` visual overlay model for gradient-style Button surfaces.
  - Document Button template contract, behavior priorities, Button family coordination, and Token boundaries.
  - Move global Token design rules out of Button Token documentation into `docs/engineering/development/control-token-guidelines.md`.
  - Promote the Button documentation structure into the global control documentation guideline at `docs/engineering/contributing/control-documentation-guidelines.md`.
