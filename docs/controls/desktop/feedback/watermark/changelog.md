# Watermark Changelog

本文档记录 Watermark 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-04

- Fixed
  - 为非 Visual WatermarkGlyph 接入生成式作用域资源宿主，修复语言资源文本未解析导致的空白水印。
  - 资源挂载与装饰层生命周期配对，支持目标资源更新、语言切换、迁移及共享 Glyph 的存活 owner 恢复。
  - 修复同步资源回调中清除或共享挂载 Glyph 时的 attachment 丢失；非有限尺寸、偏移及不前进的平铺步长不进入绘制。
  - 大负偏移直接跳过完全被裁剪的行列，保留旋转、交错与镜像的原索引相位，避免逐个绘制不可见水印。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Watermark`.
  - Align generated output paths with `controls/watermark/index-cn.md` and `controls/watermark/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Watermark desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Watermark desktop architecture documentation under `docs/controls/desktop/feedback/watermark/overview.md`.
  - Add Watermark implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Watermark control-level changelog.
  - Document that Watermark does not require a dedicated Token document and records its theme dependencies in the overview.
