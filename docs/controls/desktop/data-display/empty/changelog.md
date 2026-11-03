# Empty Changelog

本文档记录 Empty 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-04

- Fix
  - 在 `AbstractEmpty` 重新挂树时按当前来源与主题资源同步图片，保持同一实例及模板节点。
  - 在资源齐备并生成完整 SVG 后提交图片，资源暂缺时保留已有结果，避免摘挂过程中清空插画。
  - 统一图片来源校验与结果提交，来源全部为空时清空，相同结果不重置 renderer。
- Docs
  - 明确图片派生状态由 `AbstractEmpty` 单一维护，定义模板应用、重新挂树和挂树期间属性变化的同步职责。
  - 定义先生成完整结果再提交、资源暂缺时保留已有结果、来源明确清空时移除图片，以及重复提交幂等的维护契约。
  - 澄清 EmptyToken 的尺寸与间距职责、预设图形的 SharedToken 配色来源，以及摘挂和模板重套用时的 Semantic target 身份。
  - 补充同一实例摘挂、自定义来源、离树期间来源或主题变化的验证要求。

## 2026-11-01

- 将六个 Own 字段迁移到 DependencyDefaultsV2 revision 1，保留四算法默认、名称、类型、key 和 slot；预设插画与描述接入实际 Empty Global，并复用可选字体解析。
- 修复离树图片来源变更未回放；保留自定义 SVG 的原内容与颜色，以及互斥来源合同。
- 修复原生 Footer 换模板后焦点丢失；弱引用与 generation 取消旧回调，并保留后来用户焦点、对象、文本和 caret。
- 三个专用 Semantic Style 继续是部件定制入口。临时验证、正常 Skia/AOT、清理后模块验收及限定视觉结论在 CT15 结果中逐项记录，完整家族与 CT15 不因此关闭。


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
