# Empty Changelog

本文档记录 Empty 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-08-19

- Gallery
  - Align the semantic style example copy with the Gallery semantic copy convention: the title becomes "Custom Semantic Part styling" and the description describes the owner-scoped semantic part styles. Remove the dom/classNames wording (AtomUI has no DOM concept) and add `ShouldNotContain("semantic dom"/"classNames")` guards to the showcase page test.

## 2026-08-14

- API
  - 为 `AbstractEmpty` 增加 `Footer`、`FooterTemplate` 和 `StrokeDashArray` public StyledProperty，保持
    `TemplatedControl` 基类不变。
- Semantic Part
  - 为 `Empty` 建立 `root`、`image`、`description` 和 `footer` 公共契约，并生成三个 owner-scoped Semantic Style。
  - 在既有 image/description 节点和新增 Footer Presenter 上声明静态 marker，不增加运行时树扫描或动态标记。
- Theme
  - 修复 `IsDescriptionVisible` 未投影到描述节点的问题，并在 `Footer=null` 时隐藏静态 Footer Presenter。
  - 增加 `FooterMargin` Token；通过模板表面 `DashedBorder` 投影 owner 的背景、边框、圆角、Padding 和虚线节奏。
  - 让 `PresetImage`、`ImagePath` 和 `ImageSource` 运行时切换时更新同一 renderer 并清理旧来源。
- Gallery
  - 增加延迟创建的 Empty Semantic Parts Preview 和生成 Style 对照示例。
  - 将 Customize 示例的外部操作按钮迁入 Empty Footer API。
- Tests
  - 覆盖 descriptor、静态 marker、Footer 内容、描述可见性、图片来源切换、root 表面投影、尺寸 Token 和 Gallery 生命周期。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Empty`.
  - Align generated output paths with `controls/empty/index-cn.md` and `controls/empty/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Empty desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Empty desktop architecture documentation under `docs/controls/desktop/data-display/empty/overview.md`.
  - Add Empty implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Empty control-level changelog.
  - Add Empty Token documentation covering EmptyToken.
