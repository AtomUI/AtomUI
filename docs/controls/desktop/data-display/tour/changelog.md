# Tour Changelog

本文档记录 Tour 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-01

- 保持九Own的36默认/type/key/slot，迁V2R1；局部Global/可选字体到实际popup与组合目标。
- 修复重挂/退休投射、热Own偏移、专用圆点Style优先、同实例动作和根模板退出。
- 当前目标更换/移动和无效锚点生命周期同步，保留原坐标/内容/导航/pinned合同。
- 当前51聚焦通过、成员15-7-0；完整Skia/AOT/清理后验收及限定批准仍待，不关闭完整CT15。

## 2026-10-29

- Theme / Implementation
  - 共享遮罩纳入当前owner逻辑子集合，响应热时长更新；转交时释放前owner引用，关闭旧Tour不影响新owner。

## 2026-10-24

- 修复同一目标、相同 placement 的连续步骤丢失实际 Popup 翻转状态，保持箭头指向目标；默认画面中卡片随箭头布局上移6 DIP的差异已获批准。
- 将 pinned 状态的模板转发移入 AXAML，模板重建前释放旧翻转回传绑定。

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


CT15最新限定出口：60聚焦、正确managed/AOT各112检查/212帧；44明确静态待批准，原140可比/72缺帧及跨运行14非零保留。31临时/1448取证/614原会话和4最终文件清理，最终Tour21+PopupPlacement8共29通过、NET8零警告。完整家族/平台/GC/极值与CT15仍未关闭，见[限定表](../../../../superpowers/plans/2026-10-22-custom-theme/tour-consumers-visual-review.md)。

## 2026-11-01

- Release shared mask projections when a Tour closes, detaches or transfers the layer to another owner; preserve current values when reopening and avoid retaining retired Tour content through the window.
