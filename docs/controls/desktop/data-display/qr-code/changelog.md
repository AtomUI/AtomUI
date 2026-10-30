# QRCode Changelog

本文档记录 QRCode 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

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
