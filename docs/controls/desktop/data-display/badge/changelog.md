# Badge Changelog

本文档记录 Badge 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-02

- Fix
  - Restore the standalone Dot indicator after `BadgeIsVisible` is turned back on by reusing the existing show preparation. Preserve normal motion, rapid reversal, original content and remount behaviour; keep remaining strict pixel differences open.
  - Respect an explicitly hidden Dot when owner visibility changes, preserve its decorated target while hidden, and invalidate layout when runtime children change so the first explicit show after an initially hidden state measures its content.
  - Keep a CountBadge target attached and interactive when the default zero count hides the badge, including target replacement; preserve RibbonBadge's explicit hidden state across owner visibility changes.

- Theme
  - Repaint the existing Dot indicator when only `ShadowColor` changes, including transparent values and clearing back to its theme default. Keep the current size, content, target mode and motion.

## 2026-10-31

- Theme
  - Migrate the twenty Own defaults to DependencyDefaultsV2/revision1 and express Count corner defaults as dependencies on effective Own indicator heights.
  - Apply owner typography through attached TextElement properties while preserving native priority; route internal Global consumption to the public Badge owner.
- Lifecycle
  - Restore direct standalone logical/style ownership for generated Part routes while retaining the visual attach host.
  - Dispose runtime adorner projections and scoped publication bridges on retirement/detach, rebuilding current projections on attach.
- Lifecycle
  - Release instance colour overrides on clear, invalid input and detach while restoring the original token binding and colour mapping.
- Verification
  - Preserve the unresolved Count composition-parent retention boundary and retain failed visibility, reattach and calibration restoration checks. Managed/AOT each ran 72 checks with 164 exact cross-runtime pixel matches; these results do not close the family.

## 2026-10-28

- Motion
  - Count/Dot 改用 `Motion` 子类，移除 Actor、过渡订阅和重复的显隐取消源。
  - 私有 Border 承载动效，indicator 语义节点保留独立样式和变换；专用 Style route 不变。
  - 同一目标和 layer 反向时保持挂载与实际矩阵，旧隐藏请求不能移除新显示状态。
  - 保持计数隐藏期间文本、standalone Dot 静态显示、布局锚定和祖先可见性同步。


## 2026-09-27

- Implementation
  - Rework `RibbonBadgeAdorner` fold layout so the ribbon body and fold triangle are derived from the same coordinate model, and corner transform changes rebuild the rendered fold geometry.
- Tests
  - Add Ribbon fold regression coverage for standalone Start bounds and dynamic corner transform updates.
- Docs
  - Document the RibbonBadge layout and fold rendering contract: body rect, content rect and fold triangle must come from one coordinate model, with pixel-level rendering verification for the fold geometry.
  - Clarify `RibbonBadgeToken` offset, corner transform, darken amount and text padding semantics; align the token scope list with the existing Count/Dot/Ribbon token classes.

## 2026-08-13

- Docs
  - Split the complete `CountBadge`, `DotBadge`, and `RibbonBadge` Semantic Part contracts into `semantic-part.md`; keep the
    overview as a support summary and the implementation document as the runtime node mapping source.

## 2026-08-12

- Semantic Part
  - Add independent descriptors for `CountBadge`, `DotBadge`, and `RibbonBadge` with stable `root`, `indicator`, and ribbon `content` contracts.
  - Preserve Count/Dot owner-scoped styling across Avalonia 12 `AdornerLayer` by separating visual host from logical/style owner.
  - Add static semantic markers to all applicable Badge themes and generated selector-class usage for the runtime Ribbon indicator.
- Gallery
  - Add a truly deferred Semantic Parts Tab with independent Count, Dot, and Ribbon previews.
  - Register only each Count/Dot owner's runtime Adorner as an additional preview root; keep Ribbon on the inline path.
  - Extend `GalleryShowCaseHost` to manage multiple independent previews in one deferred content root.
  - Add a full-width, deferred `v6.1.3` example that demonstrates owner-scoped Semantic Part styling for Count and Ribbon badges.
- Tests
  - Cover descriptor metadata, marker placement, complete owner-scoped selector routes, nested owner isolation, Adorner detach cleanup, deferred Gallery creation, and precise cross-root preview registration.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Badge`.
  - Align generated output paths with `controls/badge/index-cn.md` and `controls/badge/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Badge desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Badge desktop architecture documentation under `docs/controls/desktop/data-display/badge/overview.md`.
  - Add Badge implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Badge control-level changelog.
  - Add Badge Token documentation covering BadgeToken.


## 未发布：CT15 Count退出成员关系

- standalone runtime host退出时同步解除公开composition成员；layer关联在视觉离树前清理。
- 保留正常hide/show、重复重挂、standalone/target原生按钮尺寸与键鼠合同，不修改Token或专用Style。
- 原GC失败账本及其他恢复/平台/家族边界保留；只修本项目成员生命周期，不替换官方依赖或扩大CT25/FW1。
