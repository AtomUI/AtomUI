# Icon Changelog

本文档记录 Icon 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-02

- 绘图指令先克隆路径再应用本次矩阵，使合成器保留正确的图标缩放，避免恢复共享路径 Transform 后丢失绘制变换；不再为绘制编辑调用方几何。
- 挂载图标通过弱引用转发路径几何变化，卸载/离树释放，保留调用方几何热更新和重挂恢复。该修正可能改变默认 SVG 的实际笔画范围，共同规则及20例证已获限定批准；其他严格失败保持，不视为整个 CT15 已通过。

## 2026-10-25（待完整验收）

- IconPresenter 换图、挂载和卸载同步图标模板归属并退休旧模板主题帧，防止复用图标保留旧输入树；保留 Template 优先级的尺寸／画刷投射与本地覆盖。

## 2026-10-23

- 修复模板图标遗留宿主绑定及显式 child 值被覆盖；两类 Presenter 的画刷/尺寸采用原生 Template 优先级，detach/替换释放并在 attach 恢复；IconPresenter 默认画刷消费自己的 Effective Global。

## 2026-10-13

- Fixed
  - Initialize the primary stroke brush and Pen in their own slot, preserving metadata defaults independently of the fallback brush.

## 2026-08-25

- Fixed
  - Stop loading Icon Compositor animation when the Icon becomes effectively invisible and resume it when visible again.
- Docs
  - Document loading animation visibility tracking and unload/detach cleanup.

## 2026-08-09

- Fixed
  - Make `IconPresenter` and `IconTemplatePresenter` project `IconBrush` to all AtomUI `Icon` brush slots so host foreground states stay synchronized for multi-brush icons.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Icon`.
  - Align generated output paths with `controls/icon/index-cn.md` and `controls/icon/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Icon desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Icon desktop architecture documentation under `docs/controls/desktop/general/icon/overview.md`.
  - Add Icon implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Icon control-level changelog.
  - Add Icon Token documentation covering IconToken.


## 未发布：CT15 Timeline pending整周期

- LoadingAnimation Spin/Pulse使用明确归零的隐式起点及唯一终点keyframe，纠正整周期沿用上一角度；正常相位、周期、停止/显隐与图形数据保持。
- 修改在Core Icon消费者，官方Avalonia12.1.4及CT25/FW1原边界保持；精确受控时钟与真实连续运行分别验证，不将Headless/AOT结论写成OS最终面。


- 2026-11-04：修复 `FallbackColor` 未进入 `FallbackBrush` 的遗留消费断链，保留 White 默认与公共名称/类型/资源编号。
