# Carousel Changelog

本文档记录 Carousel 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-31

- Preserve seven Own defaults under DependencyDefaultsV2/revision1 and activate explicitly configured ArrowOffset while keeping the original pagination-offset fallback.
- Correct effective navigation margin notifications and public/composition Global typography and colours.
- Release per-container projections and retired template click handlers; reset effective progress width when motion progress exits.
- Preserve caller-owned page content while retiring old virtualizing-panel membership for template replacement and reattach.
- Keep full CT15 visual/AOT, cleanup and final module acceptance open until evidence is complete.

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

CT15阶段验证：26聚焦+2持续工作用例通过，managed/AOT各48，112静态跨运行精确0；临时材料已清理，最终3精确用例/NET8零警告通过。44对有意静态变化待限定审查，4跨运行时相/16旧新持续态和完整家族边界保留，见[结果](../../../../superpowers/plans/2026-10-22-custom-theme/carousel-consumers-results.md)。
