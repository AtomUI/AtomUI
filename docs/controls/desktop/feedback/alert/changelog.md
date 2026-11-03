# Alert Changelog

本文档记录 Alert 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-30

- 接通CloseIconSize与MessageWithDescriptionMargin的明确Own输入、0与清除；保留未配置时原IconSizeSM及标题Margin，不自动施加历史资源默认。
- 21处模板Global输入选择实际Alert owner；补根可选字体族/字号/前景，保留原生继承、本地值和标题角色字号。
- 四个状态图标的颜色绑定恢复Template优先级，使生成的AlertIconStyle.FillBrush实际生效。
- 10个Own迁移有效默认求值Revision1，名称/type/key/slot与未配置默认不变；关闭请求、ExtraAction、Marquee及静态部件合同保持。

## 2026-08-19

- Gallery
  - Align the semantic style example copy with the Gallery semantic copy convention: the title becomes "Custom Semantic Part styling" and the description describes the owner-scoped semantic part styles. Remove the dom/classNames wording (AtomUI has no DOM concept) and add `ShouldNotContain("semantic dom"/"classNames")` guards to the showcase page test.

## 2026-08-14

- Feature
  - Publish `root`, `icon`, `section`, `title`, `description`, `actions`, and `close` Semantic Parts for Alert.
  - Generate owner-scoped `Alert*Style` types and add static template markers without changing Alert behavior or layout ownership.
  - Add `StrokeDashArray` root surface projection so Semantic Styles can express the Ant Design dashed-border example.
- Fix
  - Recompute `:has-extra-action` when `ExtraAction` changes after template application so pseudo-class and presenter state stay synchronized.
- Gallery
  - Add a deferred Semantic Part preview and reproduce the Ant Design `Object styles` / `Function styles` example with owner-scoped styles.
- Docs
  - Add the complete Alert Semantic Part contract and synchronize overview, implementation, LLMS sources, and verification boundaries.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Alert`.
  - Align generated output paths with `controls/alert/index-cn.md` and `controls/alert/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Alert desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Alert desktop architecture documentation under `docs/controls/desktop/feedback/alert/overview.md`.
  - Add Alert implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Alert control-level changelog.
  - Add Alert Token documentation covering AlertToken.
