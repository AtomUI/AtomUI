# Message 桌面版架构设计

本文档定义 `Message` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，Message 与 Notification 的共享堆叠算法见 [Feedback 堆叠基础设施](../../../../architecture/systems/control-infrastructure/feedback-stack.md)，内部实现原理见 [Message 桌面版实现原理](implementation.md)，Message Token 的专项设计见 [Message Token 设计](token.md)，设计和契约变化记录见 [Message Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Feedback/Message` |
| 控件状态 | Stable |

Message 是 AtomUI 桌面控件体系中的全局消息控件，用于展示轻量级、自动关闭的操作反馈。

Message 不负责持久通知、模态对话框或页面内 Alert。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Message`

## 2. 设计语言

Message 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Message 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Message 是 AtomUI 桌面控件体系中的全局消息控件，用于展示轻量级、自动关闭的操作反馈。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Message` 内容对象、`Icon`、`MessageType`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | 自动关闭、hover 暂停、Stack 展开/折叠和关闭 motion。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Message Token + ControlTheme。 |

## 3. API 与契约模型

Message 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Show(IMessage, string[]?)`、`MaxItems` | 创建消息并约束活动项上限；`MaxItems <= 0` 表示不限制。 |
| Stack | `IsStackEnabled`、`StackThreshold`、`IsPauseOnHover` | 默认关闭；控制阈值折叠、整体 hover 展开和实际 hover 期间的生命周期暂停，不改变消息时长。 |
| 交互与状态 | `DestroyAll()`、`IsClosed`、`IsClosing`、`IsMotionEnabled` | 清空活动消息，并表达卡片关闭与动效状态。 |
| 视觉与布局 | `Position` | 默认为 `TopCenter`，决定宿主边和横向对齐。 |
| 内容对象 | `Message`、`MessageType`、`IMessage.Expiration` | 表达正文、类型、图标、自动关闭时长与一次性关闭回调。 |

稳定事件包括 `MessageClosed`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`Message`、`MessageCard`、`WindowMessageManager`、`IMessageManager`。
- 枚举：`MessageType`。

Semantic Part owner 为两个 public Control：`MessageCard`（单条消息卡片）与 `WindowMessageManager`（服务型消息宿主）。
两个 owner 各自公开独立 descriptor，完整 Part 表、selector、`ContractType`、cardinality、节点映射与定制边界见
[Message Semantic Part 契约](semantic-part.md)。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_Frame` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_HeaderContainer` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_IconContent` | `?` | 展示图标、状态图标或操作图标。 |
| `PART_Items` | `ItemsControl` | 承载 manager 的稳定卡片集合和共享 Stack panel。 |
| `PART_Message` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

控件专属或内部伪类包括 `Error=:error`、`Information=:information`、`Loading=:loading`、`MessageCardPseudoClass.Error`、`MessageCardPseudoClass.Information`、`MessageCardPseudoClass.Loading`、`MessageCardPseudoClass.Success`、`MessageCardPseudoClass.Warning`、`Success=:success`、`Warning=:warning`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 4. 行为与状态模型

Message 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- collection/filter、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

Message 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `MessageCardTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `WindowMessageManagerTheme.axaml` | 定义弹层、窗口或 overlay 宿主视觉。 |

Message 使用 internal `MessageCardToken` 作为控件 Token scope。Token 只表达卡片背景、padding、图标与外部间距等组件视觉语义，不承载 Stack、剩余时长或关闭状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

列表的公开样式入口是 `WindowMessageManager.Padding` 与生成的 `WindowMessageManagerListContentStyle`。
前者控制队列边缘间隔；后者以 `ItemsControl` 为公共目标，支持宽度、最小宽度与对齐等属性。
项间距和卡片顺序由内部反馈栈管理，不能以 `Spacing` / `ReverseOrder` 作为 listContent 的公开 Setter。

## 6. 控件家族或集成关系

Message 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `Message`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MessageCard`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MessageCardToken`：internal 控件 Token scope，负责从全局 token 派生卡片视觉变量。
- `WindowMessageManager`：数据、状态或行为协作类型，维护集合同步和事件路径。
- 共享 `FeedbackStackPresenter` / `FeedbackStackPanel` / `FeedbackLifetimeScheduler`：维护堆叠布局与生命周期调度，不拥有 Message public 内容语义。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Message 时必须保持以下不变量：

- Stack API、默认值与共享基础设施文档构成当前契约；后续不得仅修改 Message 一侧而造成两个管理器同名 API 语义分叉。
- 带宿主的多个反馈 manager 由 `WindowFeedbackLayer` 按最近成功提交的 `Show` 原子激活；Message 不以 card `ZIndex`
  或 Gallery manager 创建顺序表达跨 manager 层级。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 集合与数据同步模型

Manager 持有稳定集合，模板只通过 `ItemsSource` 消费；重套模板不能清空或重新创建卡片。`MaxItems` 为正数时淘汰最旧活动项，`DestroyAll()` 为全部活动项发起一次正常关闭。业务内容对象不能反向持有 manager 或 presenter。

### 8.2 动效模型

Message 的内容 actor 与外层队列 transform 职责独立但同时运行：TopCenter 默认从宿主上方 `64 DIP` 处以完整时长连续
淡入并移动到终点，scale 始终为 `1`；已有消息同步平滑让位。两类过渡均使用 SharedToken `MotionDurationMid` 和
`cubic-bezier(0.645, 0.045, 0.355, 1)`。折叠时只有最新真实卡片可见，两层深度由静态 AXAML 背板表达；hover 展开
全部。初始投影、禁用 motion、重套模板和卸载路径必须直接收敛或取消旧 actor，不允许延迟 completion 改写当前状态。

### 8.3 视觉选项模型

Message 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

### 8.4 生命周期计时模型

`IsStackEnabled` 默认为 `false`，默认自动关闭时长为 3 秒，零时长永久展示。Stack 与展示时长是两个正交契约：开启
Stack 或超过阈值只改变视觉投影，有限时长消息仍按原 deadline 自动关闭；零时长消息不登记 deadline。一个 manager
只使用一个惰性共享调度器，并按最近 deadline 唤醒；普通状态只暂停实际悬停卡片，Stack 状态只在指针进入整体堆叠时
暂停全部活动卡片。继续计时必须使用剩余时长，不能重置完整时长。

| Stack | 展示时长 | 未 hover 时的布局与生命周期 |
| --- | --- | --- |
| 关闭 | 有限时长 | 全部展开，并按各自 deadline 自动关闭。 |
| 开启 | 有限时长 | 超过阈值时折叠，但仍按各自 deadline 自动关闭。 |
| 开启 | 零时长 | 超过阈值时折叠，并保持展示直到显式关闭。 |

### 8.5 宿主层激活模型

带宿主的 `WindowMessageManager` 成功把新卡片加入稳定集合后，由 `WindowFeedbackLayer` 将该 manager 作为一个原子反馈组
激活到窗口反馈层栈顶。重复向当前栈顶 manager 显示消息不会重排宿主；切换 manager 时只移动直接 manager 子项。
每个 manager 内仍由 `FeedbackStackPanel` 保证最新卡片顺序，多个 manager 的历史卡片不合并为一个全局队列。无宿主的
inline manager 继续由应用视觉树决定层级。该内部契约不新增 Public API，也不改变 Position、Stack、MaxItems、样式作用域
或 `DestroyAll()` 的 manager 边界。

### 8.6 Gallery Stack 示例模型

Gallery 的 Stack 示例使用独立 manager，不与基础、类型、loading 和回调示例共享配置或 `DestroyAll()` 范围。示例本地
状态以 Enabled 开启、Threshold 为 `3` 启动，并为每次打开显式创建零时长消息，以便持续观察折叠、hover 展开、运行时
开关和阈值变化。配置标签、ToggleSwitch 与 NumericUpDown 使用同一垂直中心线，示例卡片以 `v6.1.9` RibbonBadge 标记
能力引入版本。这些值和标记只属于示例场景；控件的 `IsStackEnabled=false` 和默认 3 秒时长不受示例状态影响。

## 9. 文档导航与验证策略

关联文档：

- [Message 桌面版实现原理](implementation.md)
- [Message Token 设计](token.md)
- [Message Changelog](changelog.md)
- [Feedback 堆叠基础设施](../../../../architecture/systems/control-infrastructure/feedback-stack.md)

Semantic Parts 摘要（完整契约见 [Message Semantic Part 契约](semantic-part.md)）：

| Owner | Part | Style Type | ContractType | 节点 | 职责 | 稳定性 |
| --- | --- | --- | --- | --- | --- | --- |
| `MessageCard` | `root` | 不适用（root） | `MessageCard` | owner 本身（表面投影 `Border#PART_Frame`） | 单条消息项根：内容、状态、关闭与进出场动效；对应上游 notice root。 | stable since 6.2.0 |
| `MessageCard` | `wrapper` | `MessageCardWrapperStyle` | `DockPanel` | `DockPanel#PART_HeaderContainer` | 图标与标题的包裹布局；对应上游 notice wrapper。 | stable since 6.2.0 |
| `MessageCard` | `icon` | `MessageCardIconStyle` | `IconPresenter` | `IconPresenter#PART_IconContent` | 状态图标尺寸与画刷；对应上游 notice icon。 | stable since 6.2.0 |
| `MessageCard` | `title` | `MessageCardTitleStyle` | `Avalonia.Controls.SelectableTextBlock` | `SelectableTextBlock#PART_Message` | 消息文本颜色、字号、行高；对应上游 notice title。 | stable since 6.2.0 |
| `WindowMessageManager` | `root` | 不适用（root） | `WindowMessageManager` | owner 本身（宿主层覆盖层；无宿主时为内联实例） | 消息列表根：定位、层级、队列、超时与宿主生命周期；对应上游 list。 | stable since 6.2.0 |
| `WindowMessageManager` | `listContent` | `WindowMessageManagerListContentStyle` | `ItemsControl` | `FeedbackStackPresenter#PART_Items` | 列表内容区域；通过 ItemsControl 公共属性定制尺寸与对齐。 | stable since 6.2.0 |

定制与兼容性摘要：

- 定制入口是生成的强类型 Semantic Style（`MessageCardWrapperStyle` / `MessageCardIconStyle` /
  `MessageCardTitleStyle` / `WindowMessageManagerListContentStyle`），以 owner-scoped 外层 `Style` 嵌套使用，
  并显式声明 `x:SetterTargetType`。应用不手写 `/template/ .semantic-*` route，也不依赖 `PART_*`。
- `root` 由 owner 侧普通 Setter 或 owner API 定制，不生成 Semantic Style 类型。
- `.semantic-*` marker 只在 owner 自身 `ControlTheme` 中静态声明；AtomUI 内置主题不使用 `.semantic-*` 实现默认视觉。
- 删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或让任一内置模板缺少 marker，
  均属于公共主题契约变更（见 [系统架构兼容性表](../../../../architecture/systems/theming/semantic-parts.md)）。
- 行为、关闭状态机、队列与宿主层装卸不属于 Semantic Part；`Message` / `IMessage` / `IMessageManager` /
  `MessageCardToken` 不持有 descriptor。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 collection/filter、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
