# Timeline Changelog

本文档记录 Timeline 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-18

- 将 header/title/content 默认对齐移至 ControlTheme，移除布局阶段的 LocalValue 写入，支持专用 Semantic Style 覆盖、移除及动态方向/Mode 切换。

## 2026-08-19

- Design
  - Add the `semantic-part.md` contract: the single owner `Timeline` publishes all nine upstream parts — `root`, `item`, `itemWrapper`, `itemIcon`, `itemSection`, `itemHeader`, `itemTitle`, `itemContent`, `itemRail` — aligned with the upstream `TimelineSemanticType` (the `StepsSemanticType` keys minus `itemSubtitle`).
  - Support `itemSection` / `itemHeader` / `itemRail` by extending the control structure: add the `TimelineSectionPanel` (carries the former `TimelineItemPanel` label/indicator/content Measure/Arrange logic) and a header wrapper node, and turn `TimelineIndicator` from a self-drawing renderer into an element compositor with a real `PART_Rail` rail element, `PART_Dot` built-in dot and `PART_IconHost` mask host. Rail geometry (first/last clipping) and dot/icon masking keep the previous rendered output pixel-identical under default themes.
  - Move the `itemIcon` part carrier from `IconPresenter#PART_IconPresenter` to the mutually-exclusive `Border#PART_Dot` / `Border#PART_IconHost` pair (ContractType `Border`): the upstream icon element is itself the dot when no custom icon is set, so `itemIcon` styles (e.g. `BorderBrush`) now color the built-in dot ring exactly like the upstream `styles.itemIcon.borderColor`, and reach the icon host when `IndicatorIcon` is present. `PART_IconPresenter` stays as the icon content node (not a part).
  - Align the horizontal Label layout with the upstream same-side model: `Start` places the axis on top with Label and Content stacked below it (centered under the axis); `End` places Label and Content on top with the axis at the bottom; `Alternate` keeps the axis in the middle. Previously Label was placed above the axis in the same-side modes, which no upstream counterpart supports.
  - Document two structural deviations from the upstream DOM (rail owned by the axis column instead of inside `header`; icon host inside the indicator instead of a direct wrapper child) and note that the upstream "Timeline Items" per-item `classNames` block has no AtomUI counterpart.
- Implementation
  - Split the item template into `TimelineItemPanel` (fill-only wrapper) → `TimelineSectionPanel` (new internal Panel carrying the direction-aware header/indicator/content Measure/Arrange and the Start/End alignment logic; the header StackPanel replaces the former direct TextBlock child).
  - Rework `TimelineIndicator` into an element compositor: template parts `PART_Rail` (axis line clipped at the node center on first/last), `PART_Dot` (built-in dot ring, box = dot size + border width with rounded corners to replicate the former pen straddle) and `PART_IconHost` (opaque mask host for `PART_IconPresenter`); remove the self-drawn ellipse/line Render override and pen caches; express icon mode through the `:icon-present` pseudo-class.
  - Inject the runtime-created `.semantic-item` marker on the container creation, pending item creation and prepare paths of `Timeline`.
- Theme
  - Restructure `TimelineItemTheme.axaml` with the section/header nesting and the static `semantic-item-wrapper` / `semantic-item-section` / `semantic-item-header` / `semantic-item-title` / `semantic-item-content` / `semantic-indicator` markers; existing orientation/mode/pending styles are preserved on the new structure.
  - Rework `TimelineIndicatorTheme.axaml` with the rail/dot/icon-host template, the static `semantic-item-rail` marker and the `semantic-item-icon` marker on both the dot and the icon host, the `ColorBgContainer` dot/host masks and the `:icon-present` visibility styles.
- Design
  - Align the vertical item spacing with the upstream `li` structure: the inter-item spacing (`ItemPaddingBottom` / `ItemPaddingBottomLG`) moves from the title/content bottom padding to the item's own `Padding` (upstream `li` paddingBottom), with `AbstractTimelineItem` applying it Decorator-style around the template root so the wrapper/section stay tight. The last item drops the spacing (upstream `li:last-child`), and the indicator extends into the spacing region through the new section `AxisOverflow` binding so the rail stays continuous.
  - Stretch the vertical Label-layout header and content into their full column slots (upstream alternate flex-column semantics) with axis-facing text alignment, so the header keeps a full-slot highlight box even for label-less items; the empty title collapses to invisible (upstream zero-height empty title, marker retained).
  - With this, every part's highlight box matches the upstream Semantic DOM previews: tight, per-item, gapped boxes instead of merged oversized boxes that included the inter-item spacing.
- Design
  - Align the rail geometry with the upstream per-item segment semantics: each vertical rail runs from its own node bottom edge to the next node top edge (crossing the item boundary, hugging the nodes so the line stays visually continuous), and the last item collapses to a zero-extent rail — so the itemRail highlight boxes are separate per-item segments with gaps at the nodes (the upstream Semantic DOM preview shows three boxes for four nodes) instead of one continuous chained box. The horizontal rail keeps running through adjacent items into one continuous axis (nodes sit at the item centers, so a single item-side segment could only cover half of each connector): the first item starts at its own node, the last item ends at its own node, and middle items span the full width with the opaque node masking the crossing. Fix the shared highlight adorner for thin targets at the same time: the primary 2px pen collapsed the stroke rect of a 2px-wide rail to zero width and rendered nothing (the first target is always the primary marker, so exactly the first rail segment went unmarked); the stroke rect now floors at the pen thickness while normal-size targets keep the previous inset stroke.
- Fix
  - The vertical rail spans to the next node's top edge, which crosses the item's bottom boundary. `TemplatedControl`/`ContentControl` default `ClipToBounds=true` was clipping that overflow and leaving a ~3px visible gap between every node. `TimelineIndicator` and `AbstractTimelineItem` now default `ClipToBounds=false` so the rail renders continuously.
- Gallery
  - Migrate `TimelineShowCase` to `GalleryShowCaseHost` with the semantic previews and the showcase page/highlight tests and the examples snapshot.
  - Mirror both upstream Semantic DOM preview blocks: the `Timeline` preview (nine cards, the upstream `_semantic` demo content — four items, no icons) and the `Timeline Items` preview (the upstream `_semantic_items` demo content — two items, short per-item descriptions for the nine parts); add the `Create a services` localized content and the ten new localization keys in all four languages.
  - Add the `timeline-semantic-part` style example mirroring the upstream style-class demo: a horizontal Timeline with the blue `#1890ff` `itemIcon` ring style plus root padding/corner-radius, and a vertical Timeline in a `#A294F9` bordered box (root padding/border via semantic styles) with the purple `itemIcon` ring style; add the `Solve initial network problems` / `Technical testing` localized contents in all four languages. The example title/description follow the Gallery semantic copy convention ("Custom Semantic Part styling", no `dom`/`classNames` wording — AtomUI has no DOM concept). The border requires the `Frame` template to bind `BorderThickness` from the owner, which the theme was missing; add the binding (default thickness stays 0, so default visuals are unchanged).
- Tests
  - Rewrite `TimelineIndicatorTests` as rail/dot/icon element geometry assertions (first/middle/last/single extent, dot ring geometry, icon mask and pseudo-class) and `TimelineSectionPanelTests` with the same layout contracts (including the stacked horizontal Start/End Label slots); add `TimelineSemanticPartTests` covering the nine-part descriptor, static markers, marker lifecycle (itemIcon doubled on the dot/host pair), route styles and zero-extent rail.
- Docs
  - Add `timeline/semantic-part.md` and synchronize `overview.md` (Semantic Part contract row, horizontal Label layout matrix, §8.4 model, §9 LLMS table) and `implementation.md` (§3 roles, §5.1 role diagram, §5.2 collaboration nodes, §5.3 Semantic Part 处置, §8/§9/§11 updates).

## 2026-07-28

- API
  - Define `Orientation` as the Timeline primary-axis selector with `Vertical` as the default.
  - Replace `TimelineMode.Left` and `TimelineMode.Right` with the logical `Start` and `End` values; keep `Alternate` and use `Start` as the default.
  - Keep placement ownership on Timeline without adding an item-level placement API or a second placement enum.
- Design
  - Define the Vertical/Horizontal x Start/End/Alternate layout matrix, logical RTL mapping, visual-order-based Reverse behavior, equal-width horizontal items and item-local text wrapping.
  - Establish visible visual order as the single source for Alternate parity, first/last state, Label layout and Pending adjacency.
- Theme
  - Reuse the existing Timeline, TimelineItem, TimelineStackPanel, TimelineItemPanel and TimelineIndicator composition for both orientations.
  - Define orientation-specific Item spacing and horizontal Indicator rendering without adding a second ControlTemplate or internal horizontal scrollbar.
- Token
  - Replace physical `IndicatorLeftModeMargin` and `IndicatorRightModeMargin` naming with `IndicatorStartModeMargin` and `IndicatorEndModeMargin`.
  - Keep horizontal cross-axis gap on SharedToken instead of duplicating it in TimelineToken.
- Docs
  - Add `orientation-layout-design.md` and synchronize the Timeline overview, implementation and Token boundaries.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Timeline`.
  - Align generated output paths with `controls/timeline/index-cn.md` and `controls/timeline/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Timeline desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Timeline desktop architecture documentation under `docs/controls/desktop/data-display/timeline/overview.md`.
  - Add Timeline implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Timeline control-level changelog.
  - Add Timeline Token documentation covering TimelineToken.
