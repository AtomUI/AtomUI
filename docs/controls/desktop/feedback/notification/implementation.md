# Notification 桌面版实现原理

本文档描述 Notification 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Notification 桌面版架构设计](overview.md)，共用堆叠与计时算法见 [Feedback 堆叠基础设施](../../../../architecture/systems/control-infrastructure/feedback-stack.md)，Semantic Part 契约见 [Notification Semantic Part 契约](semantic-part.md)，变化记录见 [Notification Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Notification Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Notification 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Notifications/INotification.cs`
- `src/AtomUI.Desktop.Controls/Notifications/INotificationManager.cs`
- `src/AtomUI.Desktop.Controls/Notifications/Notification.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationCard.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationCard.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationPosition.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationProgressBar.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationPseudoClass.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationCardToken.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationType.cs`
- `src/AtomUI.Desktop.Controls/Notifications/Themes/NotificationCardTheme.axaml`
- `src/AtomUI.Desktop.Controls/Notifications/Themes/NotificationProgressBarTheme.axaml`
- `src/AtomUI.Desktop.Controls/Notifications/Themes/WindowNotificationManagerTheme.axaml`
- `src/AtomUI.Desktop.Controls/Notifications/Utils/NotificationProgressBarVisibleConverter.cs`
- `src/AtomUI.Desktop.Controls/Notifications/WindowNotificationManager.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackStackPresenter.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackStackPanel.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackLifetimeScheduler.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackCardMotion.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackCardMotionCoordinator.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/IFeedbackStackItem.cs`
- `src/AtomUI.Desktop.Controls/Notifications/WindowNotificationManager.SemanticParts.cs`
- `src/AtomUI.Core/Animations/Motion`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `Notification`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationCard`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationProgressBar`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationProgressBarVisibleConverter`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationCardToken`：internal 控件 Token scope，负责从全局 token 派生控件语义变量。
- `WindowNotificationManager`：拥有稳定卡片集合、public Stack 配置、TopLevel host、生命周期调度与用户回调清理。
- `WindowFeedbackLayer`：作为窗口反馈 manager 的宿主，按最近成功提交的 `Show` 原子移动直接 manager 子项；不拥有卡片、
  scheduler 或用户回调。
- `FeedbackStackPresenter`：消费稳定 ItemsSource，独立保存列表 hover，并在数量、开关或阈值变化时派生展开与暂停状态。
- `FeedbackStackPanel`：按共享几何契约测量每张卡片，生成 Notification 的变高折叠、关闭投影与 Top/Bottom 镜像投影，并标识展开到折叠的单次过渡边界。
- `FeedbackStackTransitionSnapshotHost`：模板内部正文宿主，只在平铺到折叠期间持有最多一个显式内容位图；外层卡片仍负责背景、阴影和全部 Stack 投影。
- `FeedbackLifetimeScheduler`：按单调 deadline 调度有限时长项，并只为可见进度项安排刷新。
- `FeedbackCardMotion`：把 Position 归一为 64 DIP translate/fade 和统一 Ant easing，不改变 card scale。
- `FeedbackCardMotionCoordinator`：协调当前 actor 的进入/退出状态、取消和完成提交，并在 retemplate、detach、dispose 时同步解除 transition 引用。
- `NotificationCard.SemanticParts.cs` / `WindowNotificationManager.SemanticParts.cs`：只承载 `[SemanticPart]` 声明和空的
  partial class 块，不承载模板节点、Setter 或运行时查找逻辑。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Notification 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Show`、`MaxItems`、`Title`、`Content`、`Icon`、`NotificationType`。
- Stack 与生命周期：`IsStackEnabled`、`StackThreshold`、`IsPauseOnHover`、`DestroyAll()`、`Expiration`、`CurrentExpiration`。
- 交互与状态：`IsClosed`、`IsClosing`、`IsMotionEnabled`、`IsShowProgress`。
- 视觉与布局：`Position`、`ProgressIndicatorBrush`、`ProgressIndicatorThickness`。

manager 主题把方位对应的 Token 写入内部 `ThemePadding`，再用编译绑定为公开 `Padding` 提供普通样式层的默认值。
方位条件选择器不能直接设置公开 `Padding`，否则 `StyleTrigger` 优先级会覆盖应用的普通类型样式。
模板中的 presenter 通过 `TemplateBinding` 将最终 `Padding` 投影为 `Margin`，因此类型样式、条件样式和局部值
都能按正常优先级覆盖默认边距，Token 或方位变化也能继续更新未覆盖的默认值。

`NotificationType.Default` 是普通通知入口，不生成类型图标；带类型通知由 `NotificationType` 映射到 success/info/warning/error 伪类和默认状态图标。自定义 `Icon` 始终优先于类型图标。

`IsClosing` 和 `IsClosed` 是 NotificationCard 的 public 业务状态。`FeedbackCardMotionCoordinator` 持有业务阶段、版本和当前
Frame；Core Motion 拥有真实播放、连续替代与属性覆盖。模板替换使旧完成失效，禁用和 closed 状态立即收敛。
播放中修改 Position/Duration 不重启当前目标，新值在下一次请求生效；关闭只提交一次 `IsClosed=true`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- manager 的卡片 collection 在模板之外创建并保持稳定；新 `PART_Items` 只重新绑定该 collection，旧 presenter 立即解绑。
- 带宿主 manager 的跨 manager 层级只由 host layer 的直接子项顺序表达；激活使用 collection `Move`，不得通过
  `Remove` + `Add` 触发 detach/attach，也不得覆盖 manager/card 的 `ZIndex`。
- manager 首次 attach 前允许 `Show`，卡片进入稳定集合，有限时长登记保持暂停。detach 时先暂停 scheduler，
  并在视觉树级联完成后确认是否仍离树：持续离树关闭当时卡片，同轮重新入树的 host 迁移保留队列。
- `Dispose` 解绑 presenter 的集合与 hover 事件、释放 scheduler 和每张 card 的 owner/回调，再移除宿主层及安全区订阅。
  模板重套用仅更换 presenter，不重新创建卡片。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_CloseButton`：承载用户触发入口、导航或关闭动作。
- `PART_Items`：`ItemsControl` 级稳定入口，承载共享 presenter 和 panel；不能再由 manager 直接修改 `Panel.Children`。
- `PART_Layout`：稳定模板协作入口，重命名前必须同步主题和实现。

## 5.1 Semantic Part marker 接入

通知模板结构直接对齐上游 antd notice DOM，marker 的归属如下：

- `Border#Frame` 通过 `TemplateBinding` 消费 owner 的 `Background` / `BorderBrush` / `BorderThickness` / `CornerRadius` /
  `BoxShadow`，是隐式 `root` 表面的投影节点；owner `Padding` 由内部 `Border#ContentBox` 消费。
- `DockPanel#Wrapper`、`IconPresenter#IconPresenter`、`StackPanel#Section`、`atom:SelectableTextBlock#HeaderTitle`、
  `ContentPresenter#Content`、`ContentPresenter#ActionsContainer`、`IconButton#PART_CloseButton` 在
  `NotificationCardTheme.axaml` 内用 `Classes.semantic-*="True"` 静态声明，由生成器静态校验 cardinality 与
  `ContractType` 兼容性。
- `progress` 是 RuntimeCreated Part：`ConfigureProgressBar` 创建 `NotificationProgressBar` 时注入
  `NotificationCardSemanticParts.ProgressClass`，`ClearProgressBar` 在移除节点时一并释放 marker；生成器不通过源码文本
  搜索证明调用，契约由控件行为测试覆盖。
- `FeedbackStackPresenter#PART_Items` 在 `WindowNotificationManagerTheme.axaml` 内静态声明 `semantic-list-content`，
  公共 `ContractType` 为 `ItemsControl`，内部 panel 维护卡片间距与堆叠投影。
- presenter 的 `Margin` 绑定 manager `Padding`，owner 的 Position selector 选择 `NotificationTopMargin` / `NotificationBottomMargin` 默认 token。
  显式 Padding 覆盖默认值；六种方位的队列保持紧贴内容的 hover 范围。
- `Grid#PART_Layout` 位于折叠快照 host 内，close 与内容在第一行重叠，runtime progress 置于第二行并跨两列，
  避免 Auto 列以无限宽度测量进度条。Frame 只执行 render transform，不把进出场平移带入布局计算。
- standalone 与 manager 构造路径都初始化同一 motion coordinator，首次模板应用及属性变化不要求 manager 存在。

marker 只增加模板节点已有 `Classes` 集合中的稳定字符串，不引入 VisualTree 搜索、运行时 AXAML 解析或反射扫描。

## 6. 交互与事件处理

Notification 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 值提交或命令触发必须保持继承控件的事件顺序。

稳定事件路径包括 `NotificationClosed`。事件参数和触发时机属于兼容边界。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- ItemsSource、selection、checked、expanded、filter、paging 或 upload task 的集合同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。
- NotificationCard 的关闭请求、模板状态回放和最终 `IsClosed` 提交必须经过同一个关闭动效执行状态流。
- NotificationCard 的阴影 Frame 使用共享 Feedback motion；Position 只决定
  `64 DIP` 位移轴和符号，opacity 与 translate 使用完整时长插值且 scale 恒为 `1`。Frame 进入/退出与外层队列位置 /
  Stack scale transform 不得写入同一属性，也不得在动画帧中使卡片 Measure 失效。
- 协调器先以作用域透明度覆盖保持准备态；布局就绪后，Motion 同步建立起点，再从原生时钟零进度推进。
  不另建 RAF 屏障、过渡订阅或周期调度；首 tick 前后都保持准备帧。
- 离场从当前呈现值连续替代入场，不等待旧异步 finally。每次 completion 核对版本、实际 Frame 和关闭意图，
  换模板、detach、dispose 和禁用动效不会留下过期关闭回调。
- FeedbackStackPanel 把展开位置表达为相对稳定宿主边锚点的 transform，使新增、移除和 Stack 切换只更新目标
  transform；不得先改写屏幕位置再通过 Dispatcher 执行补偿动画。motion-disabled 时直接排列可见终态，并保持隐藏项
  的有效展开 Bounds，以避免无意义的 transform 属性写入和 layout-invalid 重试。
- `Show` 在 UI thread 同步创建卡片并加入稳定 collection，随后在生命周期登记、MaxItems 淘汰及其潜在用户回调之前激活
  当前宿主 manager，再更新 Stack 投影；重入到其他 manager 的后续 `Show` 必须保留为最终栈顶。不使用 cleanup queue 或轮询寻找关闭项。
- Stack 判定只统计非 closing、非 closed 的活动项，并使用数量严格大于有效阈值。折叠位置从最新项开始，按
  `nextInset = previousFarEdge + 8 - currentHeight` 使用每张卡片的真实高度计算；最多绘制并命中最新
  `Min(3, StackThreshold)` 张，前三层 scale 为 `1`、`0.94`、`0.88`。其余旧项的折叠目标 opacity 为 `0`，但仍必须在
  同一布局提交中取得最深层 scale `0.88`、placement-aware 半裁剪和对应 inset；不得让完整正文或阴影在收拢途中先穿过
  前层再淡出。transform、clip progress 与 opacity 使用同一 `MotionDurationMid` 和 easing，折叠稳态才跳过旧项绘制与命中。
- `IsClosing=true` 时卡片立即退出活动投影，但保留最后一次 inset、scale、clip、层级和宿主边锚点直到退出 actor 完成；
  其余活动项在同一布局周期重排。投影缓存只由 panel 弱引用持有，卡片移除或 panel 释放后不得形成保留链。
- panel 只在已经完成过一次展开布局后检测到 `expanded -> collapsed` 时请求正文快照；首次直接以折叠态出现、Stack 关闭、
  motion disabled 以及 `collapsed -> expanded` 都不捕获。捕获范围只限折叠稳态仍可见且外层投影发生变化的卡片，最多三张。
- `NotificationCard` 在 panel 写入折叠目标前冻结 `PART_Layout`；写入目标后才登记目标矩阵，避免同步属性通知把快照误判为
  已完成。后续 `RenderTransform` 动画值到达该矩阵时恢复真实布局并释放位图。重新展开、关闭、重套模板、detach、
  `IsMotionEnabled=false` 和 owner release 都走同一个幂等清理入口。
- 快照 host 保留正文原始 opacity 与 hit-test 状态。结束时先恢复原状态，再 `Dispose()` 当前 `RenderTargetBitmap` 并清空引用；
  捕获失败直接使用真实正文继续过渡，不能留下半激活状态。绘制时源矩形取位图物理 `PixelSize`、目标矩形取 host 逻辑
  Bounds；不能用受 DPI 影响的逻辑位图尺寸作为源像素坐标。
- presenter 的 pointer-over 是独立事实，不记录“进入时是否折叠”。当前数量或 Stack 配置变化时重新派生展开状态；Stack
  开启且列表 hover 时，manager 暂停全部活动 deadline，离开后从剩余时长恢复。
- presenter 只保存最后一次真实 pointer 事件的屏幕坐标；resize 或 placement 布局移动改变 bounds 时重新换算并验证命中，
  已移出则同步收拢 Stack 和恢复生命周期。重复 Enter / Exit 必须收敛 panel 状态，不能因缓存布尔值相同而直接返回。
- 生命周期调度不递减 public `Expiration`，只把剩余时间单向投影到可见进度；没有进度刷新需求时按最近 deadline 单次唤醒。
- scheduler 在本次唤醒结束时清空扫描临时列表，关闭回调完成后读取最新单调时刻再安排下一次唤醒；空闲 manager 不得
  因复用列表保留已关闭卡片，也不得把回调耗时再次计入后续 deadline。
- `MaxItems` 淘汰先固定本批最旧活动项，再按创建顺序请求关闭；同步关闭或回调修改集合不得改变已选批次。
- `DestroyAll()` 固定调用时的卡片批次并倒序请求关闭；回调中新建的卡片不属于外层批次，manager dispose 后停止请求。
  集合移除、用户回调与资源释放仍由 `NotificationClosed` 单一路径提交。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- manager、ItemsControl 与 card collection 在 Stack 切换和重套模板之间保持稳定。
- manager 已位于反馈层末尾时，宿主激活只做一次 O(1) 尾项比较；切换 manager 时只对直接 manager 子项执行一次索引查找
  和一次 collection `Move`。不得扫描 card、分配临时集合、创建 timer/任务/订阅或保存额外 manager 引用。
- `MeasureOverride` / `ArrangeOverride` 不允许 LINQ、临时数组、闭包或逐帧 transform 创建。
- 稳态布局必须复用缓存 transform；目标变化最多创建一个 transform 并交给 transition 插值，不增加逐帧 managed 回调。
- Notification 进出场必须使用 render-only actor，不能因 translate/fade 在每帧触发 panel Measure；布局只在集合、测量
  尺寸、位置、Stack 配置或 hover 投影实际变化时失效。
- 默认 Stack 关闭时，普通 Notification 不进入折叠裁剪投影；入场只增加一个一次性帧屏障，四个方向的偏移 transform 静态复用。
- 一个 manager 最多一个惰性 scheduler timer；隐藏旧项在折叠稳态不进行进度刷新、绘制或 hit test，其折叠过渡只复用
  已有 transform 和 clip geometry，不新增 timer、逐帧 managed 回调或强引用缓存。
- 内容快照是 bounded one-shot 资源：一次折叠最多三个，不在普通 Notification、首次折叠稳态或反向展开路径创建；完成
  检测复用卡片已有的 transform 属性通知，不增加 timer、全局事件或独立动画时钟。
- 禁止用永久 `BitmapCache` 代替显式快照；renderer cache 不能保证正文像素在父级 transform / clip 重合成期间保持冻结，
  也不能提供确定的资源释放边界。
- scale、offset 与 opacity 通过 compositor 友好的 transform 更新，不动画 width、height 或 margin。
- 性能修改必须使用同一 Notification 场景比较基线与优化后的 mean、median、P95，并证明主要指标无可测量回退。

## 9. 维护不变量

维护 Notification 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
- `IsClosing` / `IsClosed` public 状态不得与 internal Motion 合并；重复调度不得创建并行退出动效。
- `MaxItems <= 0` 必须保持无限语义；Stack 不能通过提前关闭旧项模拟折叠。
- template detach、rehost、DestroyAll、用户回调异常与 dispose 均必须释放 scheduler entry、presenter、host 订阅、card owner 与 delegate。

## 10. 测试与验证

推荐验证：

- `CloseMotionExecutionTests` 验证关闭属性变化与模板重套用同时请求退出动效时只启动一次 motion，并只提交一次
  `IsClosed=true`。
- Feedback motion 测试验证六种 Position 的 64 DIP 方向、scale 恒等、完整时长 opacity 插值、Ant ease-in-out 参数以及
  取消后旧 actor 不提交 completion。
- `FeedbackCardMotionCoordinatorTests` 使用真实 actor 与可控时钟验证六种 Position 的入场中关闭，旧入场清理后退出目标
  保持到完整时长结束，且关闭只完成一次。
- `FeedbackStackPanelTests` 覆盖阈值 `1/2/3/5`、变高卡片、关闭中的同步重排与退出快照、可见层、scale/offset、clip、
  hover 跨阈值、运行时配置和六种 Position 几何；阈值外旧项必须同步投影到最深 scale、半裁剪和 card opacity 目标。
- `NotificationCardThemeTests` 验证内层使用 render-only motion actor；manager 生命周期测试使用 `WeakReference` 验证
  dispose 后 card 与 actor 均可回收；模板测试同时验证快照 host 只包裹 `PART_Layout`，不包裹外层 Frame。
- 折叠快照测试覆盖：首次折叠不捕获、展开到折叠最多捕获三张、目标 transform 完成释放、折叠中重新展开、关闭、
  retemplate、detach 与 motion disabled 的同步释放，以及释放后真实正文 opacity / hit-test 状态恢复。
- manager 输入测试必须在通用 `VisualLayerManager` 与 Gallery 使用的 AtomUI Window 两条宿主路径覆盖：resize 后重新进入、
  离开，以及 resize 将右/下对齐 Stack 移出静止鼠标位置时的自动收拢。
- `FeedbackLifetimeSchedulerTests` 以可控时钟覆盖最近 deadline、进度刷新、剩余时长、普通/Stack 暂停、空闲停表及关闭回调耗时后的重调度；
  `WeakReference` 用例验证 scheduler 保持存活时，最后一批已到期或已在关闭的项也能回收。
- `FeedbackManagerStackTests` 覆盖无动画批量淘汰的最旧优先顺序，以及关闭回调中的 Dispose、嵌套 DestroyAll 和新增消息；
  `WeakReference` 用例验证 manager dispose 后的 manager、presenter、card、actor 与回调 owner 对象图释放；跨 manager
  激活还必须覆盖 A/B/A 顺序、Message 混合宿主、栈顶无操作快路径、单次 collection Move 和零 attach/detach。
- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
