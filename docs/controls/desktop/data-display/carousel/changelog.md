# Carousel Changelog

本文档记录 Carousel 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-11

- Theme
  - 指示点设置空 `FocusAdorner`（Ant Design 指示点 `outline: none`），键盘焦点改为复用悬停透明度：未选中的点在 `:focus-visible` 时与 `:pointerover` 一样显示 0.75 透明度。选中点保持不透明，与 Ant Design 选中点悬停时一致。

## 2026-11-08

- Token
  - `ArrowOffset` 接通切换箭头与边缘的距离（antd `arrowOffset`）；新增 `ArrowOpacity`（AtomUI 扩展，默认 0.2）取代箭头不透明度字面量。
  - 主题改读控件有效 Token；`CarouselPage` 标为 Token owner 透明页面容器。
- Fixed
  - `EffectivePreviousButtonMargin` / `EffectiveNextButtonMargin` 的 setter 误发 `EffectivePaginationMargin` 的变更通知，箭头边距变化不会
    刷新模板；现各自发出正确通知。
  - 可见变化：切换箭头与边缘的距离为 `ArrowOffset`（8px），此前为 `PaginationOffset`（12px）。

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
