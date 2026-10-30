# Masonry Changelog

本文档记录 Masonry 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-09-15

- Behavior
  - feat(Masonry): 对齐 Ant Design item 动效（入场淡入/位置滑动/离场淡出）与 RTL 镜像。
  - New items fade in (`MotionDurationSlow`, 300ms default); existing items glide to new positions with `RenderTransform` translate (no fade); removed items fade out in place through a transient ghost layer (`MotionDurationFast`, 100ms default), keeping the `.semantic-item` marker during the leave window, aligned with antd 6.6.3 Masonry motion semantics.
  - Easing is `CubicEaseOut`, equivalent to antd `motionEaseOut`; global `EnableMotion=false` degrades all motions to instantaneous.
  - RTL arranges visual rects mirrored (`x' = width - right`), matching antd `-rtl` semantics, without changing logical order.
- Theme
  - Feed `MotionDuration` / `LeaveMotionDuration` from `MotionDurationSlow` / `MotionDurationFast` tokens via ControlTheme setters; add the `PART_MotionGhostLayer` hosting canvas to the default template.
- Implementation
  - Drive motions from `MasonryPanel` layout state (old/new arrange rects) with Animation-priority preset/release via the `SetValue` disposable handle; register an `ITransform` keyframe animator (`MasonryItemTransformAnimator`).
  - Override `Panel.ChildrenChanged` to collect removed children and release ghosts before visual-children synchronization on re-add.
- Docs
  - Extend semantic-part state table with the ghost window, add the motion chapter to implementation.md, and regenerate LLMS sources.
  - Rework the Gallery semantic style demo copy from the antd React terms (`classNames` / `styles` / semantic DOM / absolute positioning / flex layout) to Avalonia/AtomUI concepts (owner-scoped styles, `MasonryItemStyle`, style matched by class or property state), aligned with the wording used by the Spin showcase and guarded by page tests.
- Verification
  - Add Masonry motion tests covering duration tokens, appear, glide, release, restart, leave ghost lifecycle, zero-duration degradation and RTL mirroring.

## 2026-08-26

- API
  - Add `LayoutStrategy` with `StableColumns` as the default and `Reflow` as the opt-in classic shortest-column strategy.
  - Add the public `MasonryLayoutStrategy` enum.
- Behavior
  - Commit `StableColumns` assignments after Arrange and associate them with direct item-container references rather than indexes.
  - Preserve existing item-container column assignments while resizing or changing child DesiredSize without changing the effective column count.
  - Place new item containers in the current shortest column without moving existing containers; remove deleted or invisible containers from the next committed snapshot.
  - Rebuild assignments when the strategy, effective column count, or explicit `Masonry.Column` / `Masonry.Span` rules change.
  - Keep `Reflow` as the strategy that recomputes every automatic assignment from current measured heights.
- Theme
  - Forward `LayoutStrategy` from `Masonry` to the default internal `MasonryPanel` through `MasonryTheme.axaml`.
- Docs
  - Define the complete strategy matrix, first-Arrange commit boundary, async-content behavior, invalidation rules, Gallery wording, and generated LLMS ownership.
- Verification
  - Cover the default strategy, stable resize behavior, item-container identity, column-count invalidation, theme forwarding, and explicit `Reflow` behavior with Masonry layout tests.

## 2026-08-25

- Performance
  - Reuse the complete layout produced by `MeasureOverride` during `ArrangeOverride` when the effective width is unchanged.
  - Recalculate only for a changed effective width or after the measured-layout cache is invalidated, while preserving non-virtualized layout and outer scrolling semantics.
- Verification
  - Add regression coverage for same-width Arrange reuse and width-change recalculation.

## 2026-08-20

- Semantic Part
  - Add `root` / `item` Semantic Part contract for Masonry, aligned with Ant Design 6.6.0 Masonry Semantic DOM.
  - Mark prepared item containers with `.semantic-item` without adding wrappers or changing the Masonry layout engine.
  - Expose root chrome through the inherited `Background` / `BorderBrush` / `BorderThickness` / `CornerRadius` / `Padding` theme binding, and add Gallery object/function style examples for the semantic demo.
  - Add Gallery Semantic Parts preview, custom Semantic Part styling example, descriptor tests and item marker lifecycle tests.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Masonry`.
  - Align generated output paths with `controls/masonry/index-cn.md` and `controls/masonry/semantic-cn.md`.

## 2026-06-19

- Docs
  - 新增 `implementation.md`，记录 Masonry 布局引擎、响应式解析、container 元数据、布局事件和维护不变量。
  - 将 `overview.md` 收敛为控件定位、公共契约、状态模型、主题边界和验证入口。
  - 在 Layout 分类入口中登记 Masonry 实现原理文档。

## 2026-06-17

- Docs
  - 新增 Masonry 桌面版架构设计文档。
  - 明确 Masonry 采用 Avalonia 原生布局容器模型，作为布局原语而非数据组件。
  - 明确 Masonry 支持两种内容提供方式：直接放置子元素（子元素本身即容器）和 `ItemsSource` 数据绑定（由基类生成 `ContentPresenter`）。
  - 明确 Masonry 不承担数据源管理、内容渲染、图片加载、滚动容器、虚拟化或项模板职责。
  - 明确 `Masonry` 位于 `AtomUI.Desktop.Controls` 命名空间，对应 `atom:` 前缀；瀑布流仅提供桌面版本，不建立跨平台抽象层。
  - 精确化视觉包装层约束：不主动插入包装层；数据绑定场景由 `ItemsControl` 固有机制产生的 `ContentPresenter` 不在约束范围内。
  - 补充 item container 元数据契约：`Masonry.Column`、`Masonry.Span` 由 `MasonryPanel.Children` 直接 child 消费，`ItemsSource` 场景应设置到生成容器层。
  - 补充 `LayoutChanged` 派发语义：只通知有效列和整行状态变化，避开 layout pass，同一布局周期合并等价通知，并定义空布局通知行为。
  - 补充键盘与焦点模型：logical order、焦点顺序、读屏顺序和容器生成顺序保持源集合顺序，不按视觉列顺序重排。
  - 补充默认值与无效值归一规则，要求 Gallery API 表与公共属性默认值保持一致。
  - 补充 Masonry 响应式布局模型，对齐共享响应式机制和 参考 Masonry `columns` / `gutter` 语义。
  - 明确响应式断点变化只更新 effective state 并触发布局失效，不在 breakpoint 回调中执行完整布局。
- API
  - 公开控件类型由 `MasonryPanel`（`Panel`）调整为 `Masonry`（`ItemsControl`），直接承担数据绑定入口与子项布局元数据 attached property 容器职责。
  - `Masonry` 同时承载 `Masonry.Column`、`Masonry.Span` attached property，attached holder 与控件合并为同一类型。
  - 明确 `ColumnInfo` 为 `ResponsiveInt?`，通过 `StyledProperty<ResponsiveInt?>` 承载响应式列数配置。
  - 明确 `Gutter` 为 `ResponsiveGutter?`，通过 `StyledProperty<ResponsiveGutter?>` 承载响应式水平/垂直间距配置。
  - 定义列数优先级：`ColumnInfo` 当前断点命中时优先，其次 `ColumnCount`，最后使用 `MinColumnWidth + MaxColumnCount + AvailableWidth` 的容器自适应列数。
  - 定义间距优先级：`Gutter` 当前断点命中时优先，未命中时回退到 `ColumnGap` / `RowGap`。
  - 明确 scalar `Gutter` 表示水平和垂直两个维度使用同一固定间距。
  - 明确 `Gutter` 水平和垂直维度独立解析，未声明维度不得隐式覆盖对应兼容属性。
  - 明确 `Masonry.Column`、`Masonry.Span` 是布局影响 attached property，值变化必须触发父级 `MasonryPanel` 重新测量。
  - 明确 `Masonry` 继承 `ItemsControl.ItemsPanel` 公共 API；显式替换 `ItemsPanel` 即表示替换 Masonry 默认布局引擎。
  - 布局引擎 `MasonryPanel` 派生自 `Panel`，标记 `internal`，仅作为 `Masonry` 的默认 `ItemsPanel` 装配，不暴露给开发者。
  - 删除 `IsFreshLayoutEnabled` 属性，子元素尺寸变化由 Avalonia layout lifecycle 自动处理，不需要额外监听机制。
  - `Masonry` 不 override `ItemsControl` 的 `NeedsContainer`、`CreateContainer` 容器生成方法，而是在 `PrepareContainerForItemOverride` 中补齐 `.semantic-item` marker；两种内容提供方式的容器层级仍由基类决定。
- Theme
  - `MasonryTheme.axaml` 装配 `ItemsPresenter` 与 internal `MasonryPanel`，通过 `RelativeSource` 或等价机制把 `Masonry` 布局属性传递给 `MasonryPanel`。
  - 明确默认 `MasonryPanel` 必须接收 `ColumnInfo` 与 `Gutter` 绑定，保持响应式属性与布局引擎同步。
  - 明确 Theme 必须尊重 `ItemsControl.ItemsPanel` 公共契约，不能通过主题结构使继承 API 失效。
  - 明确 Theme 只提供布局默认表现，不绘制子项背景、边框、阴影或卡片状态。
- Verification
  - 补充响应式 cascade、fallback、水平/垂直 gutter 独立解析和 breakpoint 变化触发布局失效的验证要求。
  - 补充 Public API 验证要求：`ColumnInfoProperty` 与 `GutterProperty` 必须作为 Avalonia `StyledProperty` 暴露。
- Token
  - 明确 Masonry 当前不定义专属 Token，不创建 `token.md`。
