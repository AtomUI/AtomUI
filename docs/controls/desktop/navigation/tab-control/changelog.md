# TabControl Changelog

本文档记录 TabControl 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-27

- Theme
  - 34项Own默认接入V2；`CardPadding`依赖有效`CardSize`，保留原公式、显式覆盖、资源身份及有限负padding。
  - 卡片添加按钮的尺寸与方向对齐改为主题声明；水平高度响应实际header，专用Add Style覆盖退出后恢复绑定。
  - 水平编译绑定由模板内Style Setter持有NameScope，不新增C#订阅。
- Docs
  - 同步Global/Own owner、三尺寸分支及模板生命周期合同。完整视觉与平台验收按交付记录单独核定。

## 2026-10-20

- Fixed
  - Line 指示条在当前选中项完整几何提交后启用主题过渡；首次显示、重新挂载及重新应用模板直接定位。
  - 相同最终矩阵不重复写入，保留动画中的普通排列；集合清空后废弃旧来源基线。
- Docs
  - 同步 [共享状态动效初始化设计](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)
    及实际接入、生命周期与验证边界。

## 2026-09-15

- Behavior
  - Revalidate the original source and logical tab after closing and selection callbacks; cancel a reorder commit when its callback mutates the source collection.
  - Apply owner close-button defaults to generated containers while preserving item overrides.
  - Abort overflow opening when snapshot notifications or template construction invalidate the session, and release abandoned content before it enters the popup tree.
- Theme
  - Measure card tabs and the add button along the placement axis, tolerate narrow arrangement, and center vertical Line tabs along the correct axis.
  - Respect RTL horizontal wheel direction and boundary scroll chaining while preserving vertical-wheel fallback.
- Verification
  - Replace the failed-open performance probe with a real overlay host and layout validation; supersede the earlier interaction percentages with a fresh matched-baseline report.

## 2026-09-14

- API
  - Add `OverflowPopupTemplate` to `BaseTabControl`, inherited by `TabControl` and `CardTabControl`, with public sealed `TabOverflowPopupContext` and immutable `TabOverflowItem` template data.
- Behavior
  - Align the Gallery custom overflow search with Ant Design Tabs: 200 px popup, clearable prefixed search, case-insensitive contains matching, 300 px scroll viewport, empty state, selected/disabled states, click activation and keyboard navigation.
  - Treat partially clipped headers as overflow and keep owner-controlled selection, close, light-dismiss and placement semantics.
  - Keep focus on the more-button when pointer-opening overflow; do not auto-focus the selected item, first item, popup root or search input.
  - Preserve and fully contain the overflow activator when the owner is narrowed by responsive masonry layout or a smaller final arrange.
  - Render custom-search item headers through the button content pipeline and keep the Popup shadow mask rounded to the final surface, including nested content presenters.
  - Hide the default and searchable overflow list scrollbars without disabling wheel, trackpad or programmatic scrolling, and keep item surfaces at equal left/right inset.
  - Replace the tab-track gradient masks with Ant Design's directional outer overflow shadows for Top, Bottom, Left and Right placement, using transparent paint carriers aligned with the scrolling viewport boundary.
- Architecture
  - Replace the duplicated TabControl/TabStrip viewers and menu items with one internal sealed `TabScrollViewer`, one static Popup shell and one shared default menu implementation.
  - Lazily create and cache popup content, clear session snapshots on ordinary close, and release popup child, cached root, context, subscriptions and owner references on template replacement or detach.
  - Track the final Popup surface through nested content presenters with explicit presenter-chain and corner-radius binding teardown so replaced surfaces remain collectible.
  - Position the static outer-shadow casters outside the scroll viewport and clip their output with an input-transparent Canvas; compiled bindings keep the inward caster faces aligned after resize or placement changes.
  - Correct the current Avalonia Skia shadow-offset transform inside the shared Tab edge renderer so overflow shadows retain their visible strength on Retina and scaled surfaces. Preserve logical shadow tokens and input-transparent clipping.
  - Add production Gallery page regressions for 1×/2× and dynamic display/ancestor scale coverage; assert visible contrast as well as bounds.
  - Add isolated Skia compositor-frame regressions covering all four owners and placements, Light/Dark, scroll boundaries, resize and non-overflowing state.
- Performance
  - Reuse the attached popup content root and clear per-open snapshots on close. Interaction performance is measured with a realized overlay host; see the corrected 2026-09-15 verification record.
  - Eliminate per-instance edge gradient brushes and gradient-stop allocations; scrolling now only updates indicator visibility.
- Token
  - Add `BoxShadowTabsOverflowLeft`, `BoxShadowTabsOverflowRight`, `BoxShadowTabsOverflowTop` and `BoxShadowTabsOverflowBottom`; derive `MenuEdgeThickness` from `ControlHeight` for a shared four-placement shadow contract.
- Gallery
  - Add one shared searchable popup implementation and examples for `TabControl`, `CardTabControl`, `TabStrip` and `CardTabStrip`, including localized placeholder and empty-state text.
  - Make the overflow examples responsive with `MaxWidth` plus stretch alignment so fixed sample width cannot push the activator outside a narrow Gallery column.

## 2026-09-12

- Docs
  - Define the shared `TabControl` / `TabStrip` overflow popup architecture around one internal `TabScrollViewer`, a static template Popup shell, immutable per-open projections and owner-validated actions.
  - Define `OverflowPopupTemplate`, `TabOverflowPopupContext` and `TabOverflowItem` as the customization contract inherited by `TabControl` and `CardTabControl`.
  - Define lazy first-open content creation, cached visual reuse, close-time data clearing, full re-template/detach teardown and weak-owner retention invariants.
  - Add blocking performance, repeated open/close, DynamicResource, stale-context, Gallery navigation and NativeAOT verification gates.

- Token
  - Record the original overflow customization boundary before directional edge-shadow Tokens were introduced on 2026-09-14.

## 2026-08-28

- Behavior
  - Route overflow close requests through `BaseTabControl.CloseTab` so `IsClosable`, `Closing`, cancellation, selection, collection and `Closed` semantics remain unified before the menu item is removed.

- Docs
  - Define overflow menu items as alternate presentations of the source `TabItem`, including propagation of effective `IsClosable` and `HeaderTemplate` semantics.
  - Define `BaseOverflowMenuItemTheme` close-button visibility from `IsClosable`, and require overflow close requests to delegate to `BaseTabControl.CloseTab` so `Closing`, cancellation, selection, collection and `Closed` semantics remain unified.
  - Require canceled or rejected closes to retain both the source tab and its overflow menu item; only a successful owner close may remove the menu item.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record BaseTabControl as the semantic owner for TabControl, with TabControlScrollViewer used only as the relay adapter.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-24

- Token
  - 新增 `TabControlToken.InkBarThickness`，表达选中指示墨条的厚度，默认从 SharedToken 的 `LineWidthBold` 派生
    （默认渲染不变）；`TabControlTheme` 与 `TabStripTheme` 的 `SelectedIndicatorThickness` Setter 由
    `SharedTokenResource LineWidthBold` 改绑 `TabControlTokenResource InkBarThickness`，支持按实例
    `Resources[TabControlTokenKind.InkBarThickness]` 覆盖，对齐 antd Tabs `styles.indicator.height`。
- Semantic Part
  - `TabControl` 公开 `indicator` Part（since 6.2.0，静态 marker `semantic-indicator`，`ContractType` 为 `Border`，
    覆盖 `PART_SelectedItemIndicator` 墨条），生成 `TabControlIndicatorStyle`；墨条仍为 motion actor，尺寸与位移
    运行时维护，Part 样式只承载视觉定制。`CardTabControl` 不公开该 Part（Card 模板无墨条节点，选中态由
    `LineMask` 表达，与 antd Card 型隐藏 ink bar 一致）。
- Gallery
  - Custom Semantic Part styling 示例将墨条厚度设为 4（对齐 antd style-class 示例的 `indicator.height`），四语种
    `SemanticPartStyleDescription` 文案同步更新。
  - Semantic Parts 页签重组：`TabControl` 预览页签带上图标与常显关闭按钮（吸收原 `TabItem` 预览的演示点）并
    增加 `indicator` 描述；`TabItemSemanticPreview` 保留，新增 "TabItem" 标题头并描述 `icon` 与 `close`
    （`root` / `label` 由预览自动补全）。`SemanticPartPreview` 新增 `Title` 属性（可选卡片标题头，默认隐藏）。
    本地化新增 `SemanticIndicatorDescription`，移除不再引用的 `SemanticLabelDescription`，覆盖计数不变
    （4210）。
- Test
  - 新增 `tests/AtomUI.Desktop.Controls.Tests/TabControl/TabControlInkBarThicknessTests.cs`，覆盖默认值、
    控件级 Token 覆盖、选中指示器渲染尺寸与 `TabStrip` 变体；Gallery 运行时测试追加墨条高度断言。
  - `TabControlSemanticPartTests` 更新 descriptor 顺序与 marker 断言并新增 indicator 生成样式测试；
    `SemanticPartPreviewTests` 新增 `Title` 渲染测试；Gallery 结构与本地化测试同步更新。

## 2026-08-23

- Semantic Part
  - 为 `TabControl` 与 `CardTabControl` 公开 `root` + `content` + `item` Semantic Part（since 6.2.0），并为
    `CardTabControl` 额外公开 `add` Part；新增 `TabControl.SemanticParts.cs`、`CardTabControl.SemanticParts.cs`
    descriptor，生成 `TabControlItemStyle`、`TabControlContentStyle`、`CardTabControlAddStyle`、
    `CardTabControlContentStyle`、`CardTabControlItemStyle`（`AtomUI.Theme.Styling`）。
  - `item` 为运行时标记（`RuntimeCreated`），路由 `> .semantic-item`，由 owner 在
    `CreateContainerForItemOverride` / `PrepareContainerForItemOverride` 应用到 `TabItem` 容器，直接提供的
    `TabItem` 实例同样获得 marker；`content` 为静态标记，覆盖内容区 `ContentPresenter`。
  - `TabItem` 公开 `root` + `icon` + `label` + `close`，生成 `TabItemIconStyle`、`TabItemLabelStyle`、
    `TabItemCloseStyle`；选中指示墨条、header extra、overflow 菜单项与 Card `LineMask` 不参与 Semantic Part。
  - 新增 `TabControl.header` 与 `CardTabControl.header`（since 6.2.0，静态 marker `semantic-header`，
    `ContractType` 为 `Border`），覆盖包裹标签条的 header 区域，生成 `TabControlHeaderStyle` /
    `CardTabControlHeaderStyle`（`AtomUI.Theme.Styling`）。
  - root 视觉 TemplateBinding 到模板根：`TabControlTheme` 根由 `Border#Frame` 换为 `PixelAlignedBorder#Frame`，
    两套模板对 `Background` / `BackgroundSizing` / `BorderBrush` / `BorderThickness` / `CornerRadius` / `Padding` /
    `BorderDashArray` / `BorderDashOffset` 全量绑定；`BaseTabControl` 公开 `BorderDashArray` / `BorderDashOffset`。
  - 标签条分隔线改用 internal `SeparatorBorderBrush` / `SeparatorBorderThickness`（同 token 值，渲染不变）；公开
    `BorderBrush` / `BorderThickness` 不再由主题赋值，默认回归 null/0。
  - 新增 `docs/controls/desktop/navigation/tab-control/semantic-part.md`；overview 同步 LLMS 语义区域表与导出
    来源，implementation 同步 Semantic marker 接入点、运行时 marker 同步规则与 Semantic Part 尺寸基线矩阵。
- Theme
  - `TabControlTheme.axaml` 与 `CardTabControlTheme.axaml` 在内容区 `ContentPresenter` 上声明
    `Classes.semantic-content="True"` 静态标记；`CardTabControlTheme.axaml` 在 `PART_AddTabButton` 上声明
    `Classes.semantic-add="True"`。
  - `BaseTabItemTheme.axaml` 与 `CardTabItemTheme.axaml` 在 `ItemIconPresenter`、标题 `ContentPresenter` 与
    `PART_ItemCloseButton` 上声明 `Classes.semantic-icon="True"` / `Classes.semantic-label="True"` /
    `Classes.semantic-close="True"` 静态标记。
- Test
  - 新增 `tests/AtomUI.Desktop.Controls.Tests/TabControl/TabControlSemanticPartTests.cs`，覆盖 descriptor 注册、
    内置主题静态 marker、运行时 item marker、生成 Style 应用、TabItem 子 Part 与 SizeType 尺寸基线。
  - 重写 `tests/AtomUIGallery.Tests/ShowCases/TabControlShowCasePageTests.cs` 并更新展示用例快照，覆盖
    Gallery Semantic Part 预览、样式示例与新增语义本地化文案；同步更新 `CatalogMemberOrder.baseline` 与
    `GalleryCatalogCoverageTests` 的单元计数期望（4175 → 4192），保持枚举、xlf 与编译期翻译契约一致。

## 2026-08-18

- Behavior
  - Preserve each tab header's `HeaderTemplate` in overflow menu items so custom header rendering remains consistent after a tab moves into the overflow menu.

## 2026-07-09

- Docs
  - Define `TabActivationTrigger` for `TabControl`, with `PointerReleased` as the default pointer activation mode and `PointerPressed` as the opt-in immediate activation mode.
  - Document press/release same-Tab activation semantics, cancellation paths, reorder precedence and verification requirements.
  - Define Tab drag reorder API, events, axis model, collection commit semantics, lifecycle cleanup and verification boundaries for `TabControl`.
  - Document that reorder must mutate logical `ItemsSource` / `Items` order instead of visual container order, and selection/content state must follow the same logical item after reorder.
  - Refine the reorder preview as a Chrome-style track-constrained model: dragged tabs move only on the placement main axis, overlapping siblings displace proportionally to avoid empty old slots, half-overlap switches the target index, sibling displacement is animated, the selected indicator follows preview transforms, and the dragged surface remains opaque.
  - Document the vertical placement icon-slot alignment model for mixed icon/no-icon tabs without adding public API or new tokens.
  - Document that default Line `Left` / `Right` spacing and item padding are compact and independent from Card spacing, and that `TabStripPlacement` changes must preserve the selected logical item without refreshing containers.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `TabControl`.
  - Align generated output paths with `controls/tab-control/index-cn.md` and `controls/tab-control/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete TabControl desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish TabControl desktop architecture documentation under `docs/controls/desktop/navigation/tab-control/overview.md`.
  - Add TabControl implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add TabControl control-level changelog.
  - Add TabControl Token documentation covering TabControlToken.
