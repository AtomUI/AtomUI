# Tour Changelog

本文档记录 Tour 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-22

- Changed
  - `TourLayer` 由 `int.MaxValue - 98` 改为 `int.MaxValue - 400`（T7），固定低于 Dialog（Avalonia `OverlayLayer`）、高于 TopLevel Drawer；此前与 Dialog 同号，先后取决于插入顺序。
  - 已知后果：在已打开的模态 Dialog 内启动 Tour 时，遮罩会落到 Dialog 之下。Tour 不负责 Dialog，Gallery 也没有这种用法，视为不受支持的偶然行为。
  - `TourLayer` 解析 `VisualLayerManager` 时跳过表面宿主，直达 TopLevel 宿主，因此 Tour 遮罩不会被关进 Drawer、Message 等提升表面；Gallery Tour 舞台那种未标记的嵌套 `VisualLayerManager` 仍是有效作用域。
- API
  - Tour 的 public / protected API 不变。
- Docs
  - 层归属见 [视觉层规范](../../../../architecture/systems/rendering/visual-layers.md)。
- Known limits
  - Windows 未实测（本次 Windows 10 实机验证只覆盖全屏标题栏、Window Drawer 与 FloatButton）；macOS 与 Linux 未实测。当前 Tour 层级证据为 Headless 测试。

## 2026-11-08

- Changed
  - 主题改读控件有效 Token：Tour 与只属于 Tour 的步骤视图以 `TourTokenResource` 读取，TourStep、DefaultTourIndicator 以各自 TokenResource
    读取。
  - 弹层到目标的间距改由主题投射的内部属性承载（`PopupMarginToAnchor` Token 加步骤 Gap 偏移），打开期间随 Token 与 Gap 变化重算，关闭或
    分离的 Tour 不再重算（借鉴 9c01e0cc7）；此前 Token 只在步骤显示时读取一次。
  - 遮罩颜色不再在初始化时复制进每个步骤：步骤未设置 `MaskColor` 时显示期间使用 Tour 的主题值（`ColorBgMask`），主题与 `Controls[Tour]`
    变化即时生效。

## 2026-09-10

- Semantic Part
  - Publish 13 semantic parts (`root` + 12 `popup.*`) aligned with the upstream Tour semantic part structure; `Tour.SemanticParts.cs` declares the parts, and `TourTheme` / `TourStepTheme` / `TourStepsViewTheme` add the static `.semantic-*` markers while default visuals keep using non-semantic selectors.
  - Mount the shared `TourLayer` mask to the active tour as its logical child (attach on show, release on close, re-attach for the next owner) and implement `ISemanticPartCrossRootProvider`, so owner-scoped semantic styles and the gallery highlight session reach the mask across the visual-root boundary; ownership follows "the tour that opens it owns it".
  - Rework `DefaultTourIndicator` into a template hosting code-materialized `Ellipse` dots: dots carry only the `semantic-popup-indicator` marker and the `active` class; size/color/spacing visuals resolve from the theme because `/template/` selector chains cannot match materialized nodes without a `TemplatedParent`, so the dot styles are declared on the template element and flow down the logical tree; keep the `N*size + (N+1)*spacing` measure contract.
- API
  - Promote `IsPopupPinnedOpen` from internal to public for semantic previews and design-time inspection.
- Gallery
  - Add the Semantic Parts tab with a default-open pinned tour on a 600px stage (antd `_semantic.tsx` alignment: anchor button, cover step, mask pointer-event pass-through) and a semantic style-class example with object/function variants driven by the generated `Tour<Part>Style` classes.
- Docs
  - Add `semantic-part.md` (13-part contract: fields, presence conditions, cardinality, selector usage, customization boundaries) and synchronize `overview.md` / `implementation.md`.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record Tour as the semantic owner for Tour.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-07-06

- API
  - Keep `CurrentIndex` as default `BindingMode.TwoWay` and make `IsOpen` default to `BindingMode.TwoWay` for controlled tour state.
- Docs
  - Document `IsOpen` and `CurrentIndex` as controlled state rather than Form validation values.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Tour`.
  - Align generated output paths with `controls/tour/index-cn.md` and `controls/tour/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Tour desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Tour desktop architecture documentation under `docs/controls/desktop/data-display/tour/overview.md`.
  - Add Tour implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Tour control-level changelog.
  - Add Tour Token documentation covering TourToken.
