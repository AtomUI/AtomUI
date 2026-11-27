# Gallery Masonry 稳定缩放设计

> 状态：2026-09-07 已获用户确认，待按实施计划执行。
>
> 证据来源：用户提供的 `Screen Recording 2026-09-06 at 23.16.47.mov`、真实 ComboBox Gallery
> 视觉树的 headless resize probe、Container Query A/B、EventPipe trace，以及仓库中正式 `Masonry` 的既有实现。

## 1. 结论

Gallery `ShowCaseMasonryPanel` 改为固定采用稳定列归属语义：第一次有效 `Arrange` 仍按当前最短列完成平衡布局；之后只要有效列数没有变化，已经排列过的可见、非 Full Span 子控件就保持原列。卡片宽度、高度和纵向位置仍随窗口宽度实时重算；只有有效列数变化时才重新建立全部列归属。

这是一项已经获批的 Gallery 可观察行为修复，不是性能优化伪装：同一列数下由文本换行、延迟内容实例化或宽度变化引起的左右换列被定义为错误行为。Container Query、实时窗口缩放、最终宽度重测、延迟加载、sticky header 和滚动契约全部保留。

性能工作分成独立的证据门禁：先用真实 ComboBox ShowCase 建立 resize 基线，再验证稳定列修复没有速度或分配回退。除稳定列带来的布局稳定性外，不预先承诺任何额外性能改动；只有热点达到每帧至少 `1 ms` 或 ShowCase 测量成本至少 `5%`，并且候选改动在相同口径下产生可重复收益，才单独立项。

## 2. 已验证问题

### 2.1 视频现象

录像中的闪动不是整个窗口清空，也不是主题资源重新加载。`prefix and suffix`、`Pre/Post tab` 和 `Status` 三张卡片在最大化过程的中间宽度左右换列，并在约两帧、约 33 ms 后换回。第一次最大化还出现约 41.7 ms 的长帧；第二次明显更顺，说明视觉不连续与冷路径耗时同时存在，但不是同一个问题。

### 2.2 Container Query 排除证据

真实 Gallery probe 从窗口宽度 1300 扫到 1728 时：

- Container Query 仅在窗口宽度约 1600 时发生一次 `ExtraLarge -> ExtraExtraLarge`；
- 后三张卡片的列交换发生在约 1639 和 1693，对不上 Container Query 边界；
- 200 次宽度往返中，跨 CQ 边界与不跨 CQ 边界的平均布局耗时都约为 0.4 ms，没有性能台阶；
- 正常尺寸与最大化终点的列序列相同，只有中间宽度出现瞬时不同序列。

因此 Container Query 只是同一次窗口 resize 中的邻近事件，不是卡片闪动的因果来源，本设计不修改它。

### 2.3 代码根因

`ShowCaseMasonryPanel.CalculateLayout` 每次调用都重新从 `columnHeights` 中选择最短列。卡片说明文字会随列宽换行，延迟内容也会改变卡片高度；上游高度的细小变化会改变后续卡片当次计算出的最短列，使后续卡片在左右列之间跳动。

`ArrangeOverride` 在 Measure 的有效宽度与最终宽度不同的时候重新测量子控件，这是正确性修复，负责避免使用错误列宽得到的陈旧高度。它让最终宽度对应的真实布局及时可见，但不是应当撤销的逻辑。撤销它会重新引入高度滞后一帧、错误纵向位置或重叠风险。

正式 `AtomUI.Desktop.Controls.Masonry` 已经提供 `StableColumns`，其规则正好覆盖本问题：按 `Control` 引用保存列、只在 Arrange 后提交、列数改变时重建、新容器走当前最短列。Gallery 的轻量面板是在正式 Masonry 增加该策略前后独立演化的子集，缺少同一稳定语义。

## 3. 用户体验契约

### 3.1 必须保持

- 最大化、恢复和手工拖拽窗口期间继续实时响应宽度，不等待 resize 结束。
- `MinItemWidth`、`MaxColumns`、`ColumnGap`、`RowGap` 的公共 API、默认值、StyledProperty 和绑定语义不变。
- 第一次有效排列仍采用最短列算法，保持初始瀑布流平衡。
- 同一列数下，卡片宽度随容器变化，文本可以重新换行，卡片高度和 Y 可以变化。
- Full Span 卡片仍从当前最高列之后开始，并把全部列高度推进到同一基线。
- 新增卡片进入它被首次排列时的当前最短列。
- 有效列数变化时重新计算全部列归属；例如 `2 -> 1` 或 `1 -> 2` 是合法响应式重排。
- `ShowCasePanel` 的 Browser 渐进挂载、desktop 全量挂载、viewport 延迟内容实例化和内部/外部滚动模式保持不变。
- `GalleryStickyTabsHost` 的 Header、StickyContent、Content、sticky mirror、滚动位置和 scrollbar 行为保持不变。

### 3.2 明确改变

- 同一有效列数下，已有可见、非 Full Span 卡片不再因为自身或前序卡片高度变化而换到另一列。
- 初始布局可能依赖页面第一次有效 Arrange 的宽度；这与正式 Masonry 的 `StableColumns` 语义一致。稳定性优先于每一个中间宽度都重新追求全局最短列。

### 3.3 非目标

- 不把 `ShowCaseMasonryPanel` 替换为 ItemsControl 或正式 `Masonry`。
- 不新增 Gallery `LayoutStrategy` 公共属性；Gallery 文档页没有需要动态选择 Reflow 的产品场景。
- 不改变 Container Query breakpoint 或 Window 最大化实现。
- 不增加 WindowState、TopLevel、resize started/ended 订阅。
- 不使用 debounce、Dispatcher 延迟、忽略标志、透明度遮盖或补间动画隐藏换列。
- 不在本修复中重写 TextBlock/TextLayout，也不为任意宽度做近似、量化或跳帧测量。
- 不因当前 `ScrollViewer` 猜测增加重复配置；已核实 Avalonia 12 的 `HorizontalScrollBarVisibility` 默认是 `Disabled`。

## 4. 稳定列状态模型

### 4.1 状态所有者

状态完全由 `ShowCaseMasonryPanel` 实例拥有：

```csharp
private readonly Dictionary<Control, int> _stableColumns =
    new(ReferenceEqualityComparer.Instance);
private int _stableColumnCount;
private bool _hasStableAssignments;
```

布局结果同时携带每个 child 的列号和 Full Span 标记：

```csharp
private readonly record struct MasonryLayout(
    double Width,
    double Height,
    int ColumnCount,
    List<Rect> Rects,
    List<int> Columns,
    List<bool> FullSpans);
```

不按 child index 保存长期状态。`ShowCasePanel.Children.Insert` 或 Browser 渐进挂载会改变索引；Control 引用才代表需要保持列归属的同一张卡片。

### 4.2 计算规则

对每个普通 child：

1. 先计算当前 `columnHeights` 的最短列。
2. 如果还没有已提交布局、列数变化、child 不在映射内或映射越界，使用最短列。
3. 否则使用已提交列。
4. 使用真实目标列宽 Measure child，并用真实 DesiredSize 更新该列高度。

对不可见 child，布局结果写入默认 Rect、列号 `-1`、`FullSpans=false`；对 Full Span child，沿用现有排布规则并标记 `FullSpans=true`。

### 4.3 提交规则

列归属只能在一次成功 `ArrangeOverride` 排列完所有 child 后提交。`MeasureOverride` 可能使用试探性宽度，也可能与最终 Arrange 宽度不同，不能让它冻结错误状态。

提交时清空并从当前 `Children` 重建字典，只保留：

- `IsVisible=true`；
- 非 Full Span；
- 结果列号位于 `[0, ColumnCount)`。

这一重建同时清理已删除、已隐藏和已变成 Full Span 的 child 引用。提交后记录本次 `ColumnCount` 并设置 `_hasStableAssignments=true`。

### 4.4 生命周期

`OnDetachedFromVisualTree` 清理 Measure 缓存和稳定列字典。这里没有事件订阅、binding、timer、动态视觉节点或跨对象 disposable。稳定字典的 acquire 是首次 Arrange 后提交，release 是 detach；每次 Arrange 的重建负责 panel 存活期间的成员移除。

## 5. 性能边界

### 5.1 已知成本

当前真实 ComboBox 页面约有 10 个 ShowCaseItem、约 990 个 visual descendants。resize 单次 headless 更新约分配 110–180 KB；trace 的主要热点位于 TextBlock/TextLayout 创建、文本换行、Measure/Arrange 和 GC。`ShowCaseMasonryPanel.CalculateLayout` 是热点链的一部分，但 panel 自身的数组和十余个 Rect 不是主要分配来源。

稳定列修复预计减少跨列位置变化和相关脏区，但在测量前不得声明具体时间收益。子控件目标宽度改变时仍必须重新 Measure；为减少测量次数而保留陈旧文本高度违反正确性优先原则。

### 5.2 测量场景

在 `AtomUI.GalleryPerformance` 增加真实路由 resize probe：

- ShowCase 固定为可通过 `--showcase combobox` 选择的真实 ComboBox 路由；
- 宽度序列固定重放 `1300 -> 1728 -> 1300`，步长 1 DIP；
- 记录每个 width update 的 elapsed、当前线程 allocation、Gen0 次数、materialized item 数、有效列数和按 Control 引用计算的换列次数；
- 同时支持产品态延迟加载和强制全部 materialized 的稳态，以区分内容实例化与纯布局；
- warmup 5、measured iterations 30；标准 Gallery cold navigation 另跑至少 10 个独立进程；
- baseline 和修复后必须使用相同 build configuration、字体、宽度序列、warmup 和 iteration。

### 5.3 接受与停止条件

- 正确性硬门槛：同一列数下已有 child 的换列次数由非零降为 `0`。
- 稳定列修复不得让 resize p95 或每轮 allocation 在同口径下恶化超过 `5%`；若超过，先定位并修正回退。
- Container Query 触发次数不能因修复改变。
- 额外性能优化资格：候选热点必须达到每帧 `>= 1 ms` 或 Gallery 成本 `>= 5%`。
- 额外性能改动保留条件：p95 至少下降 `15%` 或 allocation 至少下降 `20%`，且其他主指标不恶化超过 `5%`。
- 同一目标三轮实现与测量没有主指标改善，回退全部 perf-only 代码，只保留正确性修复和有复用价值的测量工具。

## 6. 方案审计

```text
Optimization request type: narrow root-cause correctness fix + evidence-gated performance investigation
Control: ShowCaseMasonryPanel（218 LOC）
Primary responsibilities: 已挂载 ShowCase child 的列数计算、测量、瀑布流排列和 Full Span 排列
State owners: panel 拥有 Measure cache；新增稳定列字典也由 panel 单独拥有
Lifecycle acquire/release pairs: Arrange 提交稳定映射 / detach 清空；无新增订阅、timer、binding、动态视觉
Data/selection/key flows: 无
Template parts and generated containers: 由 ShowCasePanel 的 PART_MainPanel 承载；desktop 全量挂载，Browser 渐进挂载
Existing tests / missing regression: 已有 Measure cache、最终宽度重测、最短列、Full Span、列数测试；缺少同列数 resize 稳定性、实例插入、列数切换和 Measure/Arrange 提交边界
Dead or duplicate implementation scan: 正式 Masonry 已有同类稳定列实现；复用其语义，不抽取跨程序集 helper
Potential API/behavior/render changes: 无 API/theme 变化；同列数不换列是已获用户批准的可观察行为修复
Recommended phases: 先测 baseline；TDD 落稳定列；边界回归；真实 Gallery 前后测量；Native 视频验收
What will not be changed in this pass: Container Query、Window、ScrollViewer、延迟加载策略、TextLayout、主题/模板、公开 LayoutStrategy
```

## 7. 拒绝的替代方案

### 7.1 debounce 或等待最大化结束

拒绝。它会让实时拖动布局滞后，并把跳动推迟到结束时；不同平台的 resize/window-state 时序也会引入脆弱状态机。

### 7.2 禁用 Container Query

拒绝。CQ A/B 已证明不是换列原因；禁用会破坏 Gallery 其他响应式布局，属于移除触发条件而不是修根因。

### 7.3 撤销最终宽度重测

拒绝。该路径保证 child DesiredSize 与最终列宽一致，撤销会恢复陈旧高度、纵向位置滞后或重叠。

### 7.4 整体替换为正式 Masonry

拒绝。正式 Masonry 是 ItemsControl 语义；Gallery 面板直接管理 Children，并有 ShowCaseItem 专属 Span 和 Browser 渐进挂载。整体替换会扩大逻辑树、container、延迟加载和 API 风险。

### 7.5 抽取跨程序集共享布局引擎

本次不做。两个面板的输入契约、响应式能力和事件输出不同；为少量稳定列逻辑增加跨模块抽象不符合 YAGNI。实现必须保持与正式 Masonry 的行为测试对齐，未来出现第三个真实复用方时再评估抽取。

## 8. 验收

自动化覆盖：

- narrow-first 和 wide-first 两个方向的同列数稳定性；
- Measure 宽度与 Arrange 最终宽度不同时，以最终 Arrange 结果作为首次提交；
- 插入新 child 后按 Control 引用保留已有 child 列；
- 有效列数变化时重新分配；
- 现有最终宽度重测、最短列初始布局、Full Span 和列数测试继续通过；
- GalleryBase、AtomUIGallery 相关测试、Gallery Desktop build 和 `git diff --check` 通过。

真实产品验收：

- ComboBox 页面保持录像中的滚动位置，连续至少 20 次最大化/恢复；
- 第 8–10 张卡片不再左右交换；
- sticky header、垂直 scrollbar、scroll offset、inner scrolling 和 scroll chaining 不变；
- 分别记录首次与预热后的帧间隔；
- 若仍有长帧但没有卡片换列，把它作为独立性能证据处理，不把视觉正确性修复判为失败，也不声称性能问题已经全部解决。
