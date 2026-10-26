# Breadcrumb Changelog

本文档记录 Breadcrumb 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-26

- Token
  - 八项默认迁移为 V2 静态默认函数，保留 Global 公式、identity、资源 key、类型及明确 Own／零值／撤回语义。
- Docs
  - 按实际条目、链接和兄弟分隔符消费者说明 Own；纠正将 Shared 图标尺寸列为 Own 的描述，区分 Uniformly 与 Thickness 字段。

## 2026-10-20

- Fixed
  - `BreadcrumbItem` 使用构造阶段建立的共享初始化会话，完整初始外观及加载完成后恢复过渡；模板变更和分离时复位。

## 2026-09-18

- 修复 separator Margin 重复测量与偏移。
- 分隔符按差异更新，合并调度；视觉卸载时取消待更新操作并释放集合订阅、资源绑定及逻辑/视觉挂载。

## 2026-08-21

- Code
  - Add `Breadcrumb` semantic parts for `root` / `item` / `separator` and generate `BreadcrumbItemStyle` and `BreadcrumbSeparatorStyle` contract types through the semantic-part generator.
  - Apply the `item` runtime marker during container creation and the `separator` runtime marker in `BreadcrumbItem.OnApplyTemplate`, keeping built-in themes free of static `.semantic-*` markers.
  - Move the separator default foreground into a theme style (`^ /template/ ContentPresenter#Separator`) so Semantic Part styles can override it.
  - Move the link foreground onto the item content presenter (mirroring the Ant Design `.ant-breadcrumb-item a` rule) so item-level Semantic Part styles cannot recolor link items.
  - Stretch the Breadcrumb root by default so root frame styling fills the available width like the Ant Design block-level root.
  - Add root frame appearance properties (`Background`, `BorderBrush`, `BorderThickness`, `CornerRadius`, `Padding`) to `Breadcrumb`, rendered through `BorderRenderHelper`, so the Ant Design style-class root decoration (border, background and padding) can be expressed directly on the control.
  - Make `Padding + BorderThickness` participate in the measure/arrange frame inset so items stay inside the root border.
  - Restructure the separator out of the item: separators are now sibling `ContentPresenter` nodes interleaved between item containers by the internal `BreadcrumbItemsPanel` (N items produce N-1 separators, each trailing its preceding item), matching the Ant Design flat-list DOM where the separator is a sibling of the item.
  - Manage sibling separator lifecycle in `Breadcrumb` (logical children of the breadcrumb, visual children of the items panel), bind each separator to its preceding container's `Separator`/`SeparatorTemplate`, and move the default separator foreground/margin to `TokenResourceBinder` control-token bindings.
  - Extract the sibling separator lifecycle into the internal `BreadcrumbSeparatorManager` so `Breadcrumb` keeps only the item-state and template-apply entry points.
  - Inherit the root frame properties (`Background`, `BorderBrush`, `BorderThickness`, `CornerRadius`, `Padding`) from `TemplatedControl` instead of re-registering them on `Breadcrumb`, eliminating the CS0108 hide warnings.
- Gallery
  - Migrate `BreadcrumbShowCase` to `GalleryShowCaseHost` with a Semantic Parts preview for `root` / `item` / `separator` aligned with the Ant Design semantic demo.
  - Add a `Custom Semantic Part styling` example aligned with the Ant Design `style-class` demo: an object-style root border with blue item color and dimmed separator, plus a function-style root border with purple item color.
  - Add localization units for the semantic part descriptions, the style demo title/description and the demo item labels (en-US / zh-CN / zh-TW / pt-BR).
- Tests
  - Add `BreadcrumbRootFrameTests` covering layout inset, legacy default layout, the appearance API surface and the default stretch alignment.
  - Add `BreadcrumbSemanticPartTests` covering descriptors, runtime markers, generated style routing and link color isolation on the content presenter.
  - Update `BreadcrumbSemanticPartTests` for the sibling separator route and N-1 count semantics and add coverage for sibling interleaving and per-item separator overrides.
  - Extend `BreadcrumbShowCasePageTests` with document layout, semantic preview materialization, official style values and approved localization copy checks.
  - Update `BreadcrumbShowCasePageTests` separator count assertions for the sibling structure.
- Docs
  - Add the dedicated `Breadcrumb Semantic Part` contract document and update the Breadcrumb overview and implementation docs.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Breadcrumb`.
  - Align generated output paths with `controls/breadcrumb/index-cn.md` and `controls/breadcrumb/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Breadcrumb desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Breadcrumb desktop architecture documentation under `docs/controls/desktop/navigation/breadcrumb/overview.md`.
  - Add Breadcrumb implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Breadcrumb control-level changelog.
  - Add Breadcrumb Token documentation covering BreadcrumbToken.
