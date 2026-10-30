# Slider Changelog

本文档记录 Slider 控件级设计、API、主题契约、Token 和实现结构的变化。它用于维护控件设计历史，不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

记录原则：

- 只记录会影响 Slider 设计理解、兼容边界、实现架构或维护方式的变化。
- 不记录临时讨论、纯格式化或没有长期价值的实现细节。
- 架构文档始终描述最新设计状态；历史变化记录在本文档。

## 2026-10-03

- Implementation
  - Scope global focus handling to the track input root and compare rail/thumb bounds in track-local coordinates, removing duplicate layout offsets and foreign-window focus clearing.

## 2026-08-20

- Design
  - 公开 Semantic Part：`Slider` 单一 owner 发布 `root` / `rail` / `tracks` / `track` / `handle` 五个 Part，对齐上游
    6.6.0 `SliderSemanticType`（`classNames` / `styles` 均为 `{ root?, tracks?, track?, rail?, handle? }`）。
  - rail / tracks / track 由 `SliderTrack` 自绘几何改为代码创建的 `Border` 元素（Semantic Part `rail` / `tracks` /
    `track`），几何与画刷不变；mark 点 / 标签移入 internal `SliderMarksElement`，元素化前后默认视觉不变。
  - 元素化后 `SliderTrack` 实现 `ICustomHitTest` 保持自身为 pointer 输入命中目标，轨道点击与 thumb 拖动路由与
    自绘时期一致。
  - `tracks` 单值模式跨度对齐上游：`Minimum → Value`（此前 `TracksBrush` 单值模式不绘制，仅影响显式设置
    `TracksBrush` 的场景）。
  - 上游 `tracks` 是仅被定制时才渲染的条件节点，AtomUI 恒渲染（默认透明）；上游 mark 位于 handle 之上的 DOM
    顺序差异与 handle 圆点 `::after` box-shadow 差异已记录为可接受的结构差异。
- Docs
  - 新增 [Slider Semantic Part 契约](semantic-part.md)，定义五个 Part 的 Selector、类型、数量语义、尺寸基线与
    验证契约；同步 overview、implementation 的 LLMS 语义区域与组合结构模型。

## 2026-07-30

- Design
  - Define Slider Range mode around an ordered `RangeValues` collection that supports any number of handles.
  - Define indexed `DisabledHandles`, disabled-handle boundaries, and whole-range `IsDraggableTrack` behavior.
  - Define separate `TrackBarBrush` and `TracksBrush` semantics for segment and overall active-track rendering.
- API
  - Replace the two-value `SliderRangeValue` model with `IReadOnlyList<double> RangeValues` for Range mode.
  - Remove fixed start/end thumb template parts and use dynamically managed `SliderThumb` instances.
- Docs
  - Add the Slider multi-handle design document and synchronize overview, implementation, and Token ownership.

## 2026-07-06

- API
  - Made the former range value property a default `TwoWay` Form value and routed its binding validation errors through Avalonia `DataValidationErrors`.
- Gallery
  - Added a `v6.0.8` range binding example, now migrated to the `RangeValues` API.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Slider`.
  - Align generated output paths with `controls/slider/index-cn.md` and `controls/slider/semantic-cn.md`.

## 2026-06-21

- Docs
  - Establish Slider desktop architecture documentation under `docs/controls/desktop/data-entry/slider/overview.md`.
  - Add Slider implementation documentation for track rendering, thumb layout, mark handling, tooltip synchronization, Form integration and maintenance invariants.
  - Add Slider Token documentation for track, rail, mark, thumb, outline, color and orientation padding tokens.
  - Add Slider documentation links to the Data Entry category entry.
- Implementation
  - Reorder `Slider`, `SliderTrack` and `SliderThumb` members to keep public/protected control API and explicit interface regions before private helpers.
  - Move `SliderThumb` routed event wrappers into a dedicated public event region.
  - Centralize `Slider` pointer handler cleanup in a paired helper without changing pointer behavior.
