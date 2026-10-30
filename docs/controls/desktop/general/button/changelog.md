# Button Changelog

本文档记录 Button 控件级设计、API、主题契约、Token 和实现结构的变化。它用于维护控件设计历史，不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

记录原则：

- 只记录会影响 Button 设计理解、兼容边界、实现架构或维护方式的变化。
- 不记录临时讨论、纯格式化或没有长期价值的实现细节。
- 架构文档始终描述最新设计状态；历史变化记录在本文档。

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
