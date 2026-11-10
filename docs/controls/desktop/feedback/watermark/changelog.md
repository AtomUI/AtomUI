# Watermark Changelog

本文档记录 Watermark 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-11

- Fix
  - `TextGlyph` 写入主题前景时，若属性回调把共享字形重入地挂到另一 owner，外层调用不再覆盖内层保存的订阅与前景句柄；最后离开 owner 后不再残留主题订阅。
- Docs
  - 澄清离树前景契约：从未挂到 owner 为 `Brushes.Black`，离开 owner 后保留最后的主题前景（与实现和测试一致）。

- 未显式指定的文本字形前景与字号随作用域 ColorTextBase/FontSizeLG 更新；显式值、0.3 透明度和未挂载元数据默认值保持。
- 默认绑定随现有活动 owner 会话释放，MarkupExtension 不再以默认本地值遮蔽主题。

## 2026-11-07

- API
  - `TextGlyph` 新增 `FontFamily`（默认 `FontFamily.Default`）、`FontWeight`（`Normal`）、`FontStyle`（`Normal`）。三者变化都会以 `new Typeface(family, style, weight)` 重建格式化文本。
  - `TextGlyphExtension.Foreground` 改为 `IBrush?` 且无默认值，只在设置时赋值；此前它强制写入 `Brushes.Black`。
  - `WatermarkGlyph` 新增 `private protected virtual OnResourceOwnerChanged(IResourceHost?)`，在 `UpdateResourceOwner` 末尾调用，仅供 `TextGlyph` 使用。
  - 新增 `internal sealed WatermarkTileRenderer`：`Render(DrawingContext, Size area, Size cell, WatermarkGlyph glyph)`，供 AtomUI.Mobile 的 WaterMark 复用。
- Behavior
  - 平铺循环（网格步长、首个可见行列裁剪、交错偏移、镜像和有效性保护）从 `Watermark.Render` 原样移入 `WatermarkTileRenderer`。网格步长为 `cell + HorizontalSpace/VerticalSpace`，cell 大于字形尺寸时字形在 cell 内居中；Desktop 中 cell 等于字形尺寸，不额外变换。旋转矩阵按（cell 尺寸，`Rotate`）缓存。
  - `TextGlyph` 的文本方向固定为 `LeftToRight`（此前为 `RightToLeft`）。
  - `TextGlyph.Foreground` 在字形拥有 `StyledElement` 资源 owner 时默认取 owner 的 `ColorText` 全局 Token，以 Template 优先级写入：显式赋值优先，`ClearValue` 回到主题值；每次挂到 owner 时按当前主题解析，并订阅主题变化，最后一个 owner 离开时释放订阅。从未挂到 owner 的字形为默认 `Brushes.Black`；离开 owner 后保留最后一次解析的主题前景、不再跟随主题，重新挂上时按当前主题重新解析。
- Desktop 影响
  - 默认前景从黑色改为跟随主题文本色 `ColorText`，字形 `Opacity` 默认仍为 0.3：浅色主题下外观与此前的黑色乘 0.3 接近（无头渲染对照中默认文字最深像素亮度 179 变为 188，背景 255）；深色主题下文字由比背景更暗的黑色变为比背景更亮（背景 20，最深像素 14 变为 20，最亮像素 20 变为 79）。需要固定颜色的应用应显式设置 `Foreground`。
  - 文字方向为从左到右；平铺位置、旋转、交错与镜像与此前一致。
  - 通过 `Watermark.Glyph` 附加属性使用水印的用法不需要改动：`Watermark` 在挂树时为字形接入资源 owner。
- Tests
  - 新增 `WatermarkTileRendererTests`：旋转单元覆盖区域、cell 大于字形时居中、奇数列镜像与奇数行偏移、跳过不可见平铺并保持网格相位、矩阵缓存只在 cell 或旋转变化时重建、字体属性、文本方向、前景随主题（含离树期间切换与显式值优先）、字形离开 owner 后不再跟随主题。

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
