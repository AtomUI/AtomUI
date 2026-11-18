# QRCode Changelog

本文档记录 QRCode 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-17

- Fixed
  - 掩码态（Loading/Expired/Scanned）不再把圆角外框裁成直角：`cover` 是子节点、绘制在 `Frame` 之上，未裁剪时会铺满方框并盖住边框，圆角视觉失效。`Frame` 现开启 `ClipContentToCornerRadius`，把内容子树统一裁到圆角内框（与 `Expander PART_Frame` 同一契约）；`Active` 态视觉不变。

- Theme
  - 圆角由根节点裁剪统一提供，容器放大圆角（如 Shadcn Luma/Maia 26）时掩码仍保持圆角。

## 2026-11-10

- Fixed
  - 显式 Color=null 时按有效 ColorText（含透明度）生成点阵，随局部主题更新；非空 Color 保持优先，挂树/脱离平衡资源订阅。

- Theme
  - 控件主题读取自身有效全局 Token，局部覆盖与嵌套已注册控件保持隔离；消费范围见 [Token 设计](token.md#7-有效-token-与作用域边界)。

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
