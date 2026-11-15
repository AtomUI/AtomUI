# Carousel Changelog

本文档记录 Carousel 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-14

- Carousel 采用共享的 `AbstractCarousel` 选择和自动播放模型。启用自动播放且有多页可用时才建立计时器。
- 窗口最小化、控件隐藏、应用失活和滑动交互期间暂停翻页及进度动画。恢复后从完整周期开始。
- 空 Carousel 的前进和后退操作保留空选中状态。

## 2026-11-11

- Theme
  - 指示点设置空 `FocusAdorner`（Ant Design 指示点 `outline: none`），键盘焦点改为复用悬停透明度：未选中的点在 `:focus-visible` 时与 `:pointerover` 一样显示 0.75 透明度。选中点保持不透明，与 Ant Design 选中点悬停时一致。

## 2026-10-20

- Fixed
  - `CarouselPageIndicator` 的选中宽度和透明度由共享初始化会话管理，首次选中页直接呈现。自动播放进度使用独立 Animation，其播放、可见性跟踪和取消流程保持独立。

## 2026-08-25

- Fixed
  - Pause Carousel autoplay and selected-page progress while effectively invisible, then restart both from a synchronized cycle.
- Docs
  - Document timer, progress animation and effective-visibility ownership.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Carousel`.
  - Align generated output paths with `controls/carousel/index-cn.md` and `controls/carousel/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Carousel desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Carousel desktop architecture documentation under `docs/controls/desktop/data-display/carousel/overview.md`.
  - Add Carousel implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Carousel control-level changelog.
  - Add Carousel Token documentation covering CarouselToken.
