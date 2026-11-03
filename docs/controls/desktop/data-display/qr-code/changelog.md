# QRCode Changelog

本文档记录 QRCode 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-01

- 两 Own 迁移到 DependencyDefaultsV2 revision 1，保留四算法默认、类型、key 和 slot；原字体与边框/padding 角色接入实际 owner Global，保留原生和专用 Cover Style 优先级。
- 修复空 Value 仍显示旧二维码；生成 Bitmap 在替换、清空和卸载时释放，离树更新延迟到重挂，不释放调用方 Icon。
- 刷新订阅在模板替换/卸载退休，重挂只接一次；原生状态输入重模板后以弱引用和 generation 守卫恢复焦点，保留后来用户焦点。
- 保留原编码器、quiet zone、纠错、图标挖空与 public API；编码探针与完整 RGBA 验收分账，临时/清理后证据及未通过项逐条记在 CT15 结果中，不据局部分包关闭完整 CT15。

## 2026-08-14

- Semantic Part
  - Publish the `root` and `cover` Semantic Part contract for QRCode.
  - Keep `cover` as one static template `Border` across Active, Loading, Expired and Scanned state changes.
- Theme
  - Separate the padded QR content surface from the full-root cover overlay while preserving the `Size` square baseline.
  - Project root Background, Border, CornerRadius and Padding settings through the built-in template.
  - Keep the QR bitmap background transparent so semitransparent root backgrounds are not composited twice.
- Gallery
  - Add the deferred Semantic Part preview and align the semantic styling example with the Ant Design 6.6.0 QRCode demo.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `QRCode`.
  - Align generated output paths with `controls/qr-code/index-cn.md` and `controls/qr-code/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete QRCode desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish QRCode desktop architecture documentation under `docs/controls/desktop/data-display/qr-code/overview.md`.
  - Add QRCode implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add QRCode control-level changelog.
  - Add QRCode Token documentation covering QRCodeToken.
