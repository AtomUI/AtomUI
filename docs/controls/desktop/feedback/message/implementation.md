# Message 桌面版实现原理

本文档描述 Message 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Message 桌面版架构设计](overview.md)，共用堆叠与计时算法见 [Feedback 堆叠基础设施](../../../../architecture/systems/control-infrastructure/feedback-stack.md)，变化记录见 [Message Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Message Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Message 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Message/IMessage.cs`
- `src/AtomUI.Desktop.Controls/Message/IMessageManager.cs`
- `src/AtomUI.Desktop.Controls/Message/Message.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageCard.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageCard.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageCardPseudoClass.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageCardToken.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageType.cs`
- `src/AtomUI.Desktop.Controls/Message/Themes/MessageCardTheme.axaml`
- `src/AtomUI.Desktop.Controls/Message/Themes/WindowMessageManagerTheme.axaml`
- `src/AtomUI.Desktop.Controls/Message/WindowMessageManager.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackStackPresenter.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackStackPanel.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackLifetimeScheduler.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackCardMotion.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackCardMotionCoordinator.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/IFeedbackStackItem.cs`
- `src/AtomUI.Desktop.Controls/Message/WindowMessageManager.SemanticParts.cs`
- `src/AtomUI.Core/Animations/Motion`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `Message`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MessageCard`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MessageCardToken`：internal 控件 Token scope，负责从全局 token 派生控件语义变量。
- `WindowMessageManager`：拥有稳定卡片集合、public Stack 配置、TopLevel host、生命周期调度与用户回调清理。
- `WindowFeedbackLayer`：作为窗口反馈 manager 的宿主，按最近成功提交的 `Show` 原子移动直接 manager 子项；不拥有卡片、
  scheduler 或用户回调。
- `FeedbackStackPresenter`：消费稳定 ItemsSource，管理整体 hover 和 Message 静态背板状态。
- `FeedbackStackPanel`：按共享几何契约测量和排列，不拥有内容或生命周期。
- `FeedbackLifetimeScheduler`：按单调 deadline 调度有限时长项；空闲时没有活动 timer。
- `FeedbackCardMotion`：把 Message 的 Position 归一为 64 DIP translate/fade 和统一 Ant easing，不改变 card scale。
- `FeedbackCardMotionCoordinator`：协调当前 actor 的进入/退出状态、取消和完成提交，并在 retemplate、detach、dispose 时同步解除 transition 引用。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Message 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Show`、`MaxItems`、`Message`、`MessageType`、`Icon`。
- Stack 与生命周期：`IsStackEnabled`、`StackThreshold`、`IsPauseOnHover`、`IMessage.Expiration`、`DestroyAll()`；Stack
  只参与视觉投影和实际 hover 暂停，不能改写或替代展示时长。
- 交互与状态：`IsClosed`、`IsClosing`、`IsMotionEnabled`。
- 视觉与布局：`Position`。

`IsClosing` 和 `IsClosed` 是 MessageCard 的 public 业务状态。`FeedbackCardMotionCoordinator` 持有业务阶段、版本和当前
`PART_Frame`；Core Motion 拥有真实播放、连续替代与属性覆盖。模板替换使旧完成失效，禁用和 closed 状态立即收敛。
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
- Gallery 页面分别惰性创建普通示例 manager 与 Stack 示例 manager；页面 detach 时两者都必须 `Dispose()` 并清空引用。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_Frame`：承载根视觉、边框、背景或尺寸基线。
- `PART_HeaderContainer`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_IconContent`：展示图标、状态图标或操作图标。
- `PART_Items`：`ItemsControl` 级稳定入口，承载共享 presenter 和 panel；不能再由 manager 直接修改 `Panel.Children`。
- `PART_Message`：稳定模板协作入口，重命名前必须同步主题和实现。

## 6. 交互与事件处理

Message 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 值提交或命令触发必须保持继承控件的事件顺序。

稳定事件路径包括 `MessageClosed`。事件参数和触发时机属于兼容边界。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- ItemsSource、selection、checked、expanded、filter、paging 或 upload task 的集合同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。
- MessageCard 的关闭请求、模板状态回放和最终 `IsClosed` 提交必须经过同一个关闭动效执行状态流。
- MessageCard 直接对阴影 Frame 使用共享 Feedback motion；按 Position 从对应边缘平移 64 DIP 并插值 opacity，
  scale 全程为 `1`，与外层队列位置 transform 分离。
- 关闭从当前呈现值连续替代入场，不等待旧任务清理；模板替换、detach 和 dispose 使旧完成失效，资源按句柄释放。
- FeedbackStackPanel 把展开位置表达为相对稳定顶部/底部锚点的 transform，使新增、移除和 Stack 切换只更新目标
  transform；不得先改写屏幕位置再通过 Dispatcher 执行补偿动画。motion-disabled 时直接排列可见终态，并保持隐藏项
  的有效展开 Bounds，以避免无意义的 transform 属性写入和 layout-invalid 重试。
- `Show` 在 UI thread 同步创建并登记卡片，加入稳定 collection 后更新 MaxItems 和 Stack 投影；不使用延迟队列重新取得旧模板的 children。
- Stack 判定使用活动项数量严格大于有效阈值；Message 折叠只保留最新真实卡片可见，并由 AXAML 中两个静态背板表达深度。
- 背板在展开期间仍跟随最新活动卡片的尺寸，通过 opacity 与 RenderTransform 的快速过渡表达显隐；收拢时不能从零
  重新增长宽度。两层背板由零尺寸 Panel 叠放，不参与队列 extent 或命中；顶部、底部锚定及宽度变化过渡遵循共享几何契约。
- 生命周期调度只保存 deadline / remaining 与 card 协作引用。有限时长项无论 Stack 是否开启都登记 deadline；零时长项
  不创建登记。只有 lifecycle detach 或实际 hover 才能暂停，折叠状态自身不能暂停。继续和 deadline 到期由一个
  manager 级 scheduler 完成，不为每项创建 timer。
- scheduler 在本次唤醒结束时清空扫描临时列表，关闭回调完成后读取最新单调时刻再安排下一次唤醒；空闲 manager 不得
  因复用列表保留已关闭卡片，也不得把回调耗时再次计入后续 deadline。
- `MaxItems` 淘汰先固定本批最旧活动项，再按创建顺序请求关闭；同步关闭或回调修改集合不得改变已选批次。
- `Show` 在 UI thread 同步创建卡片并加入稳定 collection，随后在生命周期登记、MaxItems 淘汰及其潜在用户回调之前激活
  当前宿主 manager；重入到其他 manager 的后续 `Show` 必须保留为最终栈顶。
- `DestroyAll()` 固定调用时的卡片批次并倒序请求关闭；回调中新建的卡片不属于外层批次，manager dispose 后停止请求。
  正在关闭项不会重复关闭，清理仍由 card 的 `MessageClosed` 单一路径提交。
- Gallery Stack 示例每次只同步创建一个零时长 Information 消息，并复用一个递增序号；Enabled 与 Threshold 直接更新
  Stack 示例 manager 的现有投影，`DestroyAll()` 只清空该 manager 且不重置序号。普通示例 manager 保持运行时默认
  `IsStackEnabled=false`，因此 Stack 示例配置不能泄漏到其他示例。配置行按 NumericUpDown 的行高垂直居中各子组，
  `BadgeText="v6.1.9"` 交由 ShowCaseItem 的既有 RibbonBadge 模板呈现，不创建专用角标视觉。

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
- 没有有限时长活动项时 scheduler 不保持活动 timer；没有 Notification 进度需求时只安排最近 deadline。
- Gallery Stack 示例的零时长消息不创建 scheduler entry、异步循环、取消令牌或逐项 timer；两个示例 manager 都只在
  第一次实际操作时创建，并在页面 detach 时释放。
- 折叠背板由 AXAML 静态创建，不随 show 次数增加视觉对象。
- 性能修改必须使用同一 Message 场景比较基线与优化后的 mean、median、P95，并证明主要指标无可测量回退。

## 9. 维护不变量

维护 Message 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
- `IsClosing` / `IsClosed` public 状态不得与 internal Motion 合并；重复调度不得创建并行退出动效。
- `MaxItems <= 0` 必须保持无限语义；Stack 不能通过提前关闭旧项模拟折叠。
- Stack 不能隐式把有限时长改为永久展示，也不能在未 hover 时暂停 scheduler；Gallery 的永久 Stack 示例必须通过
  `Expiration=TimeSpan.Zero` 显式表达。
- template detach、rehost、DestroyAll、用户回调异常与 dispose 均必须释放 scheduler entry、presenter、host 订阅、card owner 与 delegate。

## 10. 测试与验证

推荐验证：

- `CloseMotionExecutionTests` 验证关闭属性变化与模板重套用同时请求退出动效时只启动一次 motion，并只提交一次
  `IsClosed=true`。
- Feedback motion 测试验证 64 DIP 方向、scale 恒等、完整时长 opacity 插值、Ant ease-in-out 参数以及取消后旧 actor
  不提交 completion。
- `FeedbackCardMotionCoordinatorTests` 使用真实 actor 与可控时钟验证六种 Position 的入场中关闭，旧入场清理后退出目标
  保持到完整时长结束，且关闭只完成一次。
- `FeedbackStackPanelTests` 覆盖阈值边界、最新项顺序、Message 背板、hover gap、运行时配置和 Top/Bottom 几何。
- `MessageStackAnimationTests` 用可控动画时钟验证背板在卡片收拢期间淡入、hover 反转连续、内容宽度变化、motion / Stack
  开关，以及空队列和展开队列不被透明背板扩高。
- `FeedbackLifetimeSchedulerTests` 以可控时钟覆盖最近 deadline、剩余时长、普通/Stack 暂停、空闲停表及关闭回调耗时后的重调度；
  `WeakReference` 用例验证 scheduler 保持存活时，最后一批已到期或已在关闭的项也能回收。
- manager 测试覆盖 Stack 开启但未 hover 的有限时长消息保持计时登记且未暂停，以及零时长 Stack 消息不创建 scheduler。
- `FeedbackManagerStackTests` 覆盖无动画批量淘汰的最旧优先顺序，以及关闭回调中的 Dispose、嵌套 DestroyAll 和新增消息；
  `WeakReference` 用例验证 manager dispose 后的 manager、presenter、card 与回调 owner 对象图释放；跨 manager 激活还必须
  覆盖 A/B/A 顺序、Notification 混合宿主、栈顶无操作快路径、单次 collection Move 和零 attach/detach。
- Gallery 行为测试覆盖普通与 Stack 示例 manager 隔离、Stack 控件初值、运行时切换、交替文案、局部 DestroyAll 和 detach 释放。
- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

## 11. Semantic Part 接入

Message 公开两个 owner 的 Semantic Part：`MessageCard`（notice）与 `WindowMessageManager`（list）。声明集中在与
owner 同目录的 `MessageCard.SemanticParts.cs` 与 `WindowMessageManager.SemanticParts.cs`，owner 本体保持 partial。
公共契约与逐 Part 说明见 [Message Semantic Part 契约](semantic-part.md)。

### 11.1 Descriptor 与 marker

| Owner | Part | SelectorClass | SelectorRoute | ContractType | 声明位置 |
| --- | --- | --- | --- | --- | --- |
| `MessageCard` | `wrapper` | `semantic-wrapper` | `/template/ .semantic-wrapper` | `DockPanel` | `MessageCardTheme.axaml` 内 `DockPanel#PART_HeaderContainer` |
| `MessageCard` | `icon` | `semantic-icon` | `/template/ .semantic-icon` | `IconPresenter`（`AtomUI.Controls.IconPresenter`） | `MessageCardTheme.axaml` 内 `IconPresenter#PART_IconContent` |
| `MessageCard` | `title` | `semantic-title` | `/template/ .semantic-title` | `Avalonia.Controls.SelectableTextBlock` | `MessageCardTheme.axaml` 内 `SelectableTextBlock#PART_Message` |
| `WindowMessageManager` | `listContent` | `semantic-list-content` | `/template/ .semantic-list-content` | `ItemsControl` | `WindowMessageManagerTheme.axaml` 内 `FeedbackStackPresenter#PART_Items` |

`root` 由生成器隐式加入 descriptor，不要求 `.semantic-root` marker。四个非 root Part 都是 owner 自身
`ControlTheme` 的静态模板节点，因此显式声明 `RuntimeCreated=false`，生成器按 owner 主题资产（derived/本体
Theme.axaml）做 marker 数量与 `ContractType` 兼容性静态校验，无需跨模板豁免。

marker 使用模板静态 class 属性语法书写，值是静态 `True`：

```xml
<DockPanel Name="PART_HeaderContainer" Classes.semantic-wrapper="True" LastChildFill="True">
```

`title` 的节点类型必须与 `ContractType` 精确一致：主题中无前缀的 `SelectableTextBlock` 解析为
`Avalonia.Controls.SelectableTextBlock`；AtomUI 的 `atom:SelectableTextBlock` 是另一类型，二者不可混用，否则
生成器 `ATOMUIGEN033` 类型不兼容校验会拒绝该 Part。

### 11.2 运行时创建路径

Message 没有任何运行时创建的 Part：

- `MessageCard` 由 `WindowMessageManager.Show` 以 `new MessageCard { ... }` 创建，但其模板由自身
  `ControlTheme` 实例化，三个卡片级 marker 都随模板静态就位，root 为隐式 owner。
- `WindowMessageManager` 有两个 public 构造：`WindowMessageManager(TopLevel? host)` 与无参 `WindowMessageManager()`。
  传入 host 时经 `InstallFromTopLevel` 挂到 TopLevel 反馈层并投影安全区外边距；传 `null` 或使用无参构造时控件不安装
  到任何层，作为普通可放置控件由调用方放进自己的视觉树（上游把 list 改为内联容器的等价物），两种路径都在模板应用后
  取得 `PART_Items`；`listContent` marker 静态声明在 manager 模板内。

卡片实例本身不携带任何 semantic class；`Show(IMessage, string[]? classes)` 的 `classes` 参数只追加用户业务 class，不参与
Semantic Part 身份。因此卡片增删、`MaxItems` 清理与超时/手动关闭只影响实例数量，不改变 marker 结构。

### 11.3 生命周期与布局

- MessageCard 模板重套用时重新取得 actor，旧执行取消、旧模板节点释放；marker 随新模板重建。
- manager 的稳定卡片集合与 scheduler 不依赖 presenter 实例。持续 detach 的延后关闭和 `Dispose` 的最终清理
  遵循第 5 节生命周期规则；同轮 host 迁移不清空集合。
- `wrapper` 是 DockPanel，icon 停靠左侧、title 填充，间距来自 `MessageIconMargin`。
- `listContent` 实际为 `FeedbackStackPresenter`，公共 `ContractType` 为 `ItemsControl`。其内部 panel 维护项间距和
  堆叠布局；应用通过生成 Style 定制尺寸、对齐，不能访问内部间距 API。
- `Grid#PART_StackHost.Margin` 绑定 owner `Padding`，默认取 `MessageTopMargin`。六种 Position 在模板应用和属性
  修改时更新，StackHost 按方位紧贴队列，两个静态背板不增加 host 的 extent 或 hover 命中范围。

### 11.4 性能与 AOT

- 四个 marker 在模板初始化时各执行一次 `Classes.Set`，不建立 Binding 或持久订阅。
- 默认主题不消费 `.semantic-*`；未声明用户 Semantic Style 时不创建独立 selector activator。
- descriptor、`StyleType`、Part 常量与 XML namespace 映射全部由生成器静态产生，不引入反射、程序集扫描或运行时
  AXAML 解析。
