# Notification 桌面版架构设计

本文档定义 `Notification` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，Message 与 Notification 的共享堆叠算法见 [Feedback 堆叠基础设施](../../../../architecture/systems/control-infrastructure/feedback-stack.md)，内部实现原理见 [Notification 桌面版实现原理](implementation.md)，Notification Token 的专项设计见 [Notification Token 设计](token.md)，设计和契约变化记录见 [Notification Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Feedback/Notification` |
| 控件状态 | Stable |

Notification 是 AtomUI 桌面控件体系中的通知控件，用于在窗口角落展示可关闭的较重反馈和进度信息。

Notification 不负责即时消息气泡、页面内 Alert 或模态确认。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Notifications`

## 2. 设计语言

Notification 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Notification 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Notification 是 AtomUI 桌面控件体系中的通知控件，用于在窗口角落展示可关闭的较重反馈和进度信息。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Notification` 内容对象、`Title`、`Content`、`Icon`、`NotificationType`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | 自动关闭、进度、hover 暂停、Stack 展开/折叠和关闭 motion。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Notification Token + ControlTheme。 |

## 3. API 与契约模型

Notification 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Show(INotification, string[]?)`、`MaxItems` | 创建通知并约束活动项上限；`MaxItems <= 0` 表示不限制。 |
| Stack | `IsStackEnabled`、`StackThreshold`、`IsPauseOnHover` | 控制阈值折叠、整体 hover 展开和生命周期暂停。 |
| 交互与状态 | `DestroyAll()`、`IsClosed`、`IsClosing`、`IsMotionEnabled`、`IsShowProgress` | 清空活动通知，并表达卡片关闭、进度和动效状态。 |
| 视觉与布局 | `Position`、`ProgressIndicatorBrush`、`ProgressIndicatorThickness` | 默认 `TopRight`；控制宿主位置和进度视觉。 |
| 内容对象 | `Expiration`、`NotificationType`、`Title`、`Content`、`Icon` | 表达正文、类型、自动关闭时长与一次性关闭回调。 |

稳定事件包括 `NotificationClosed`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`Notification`、`NotificationCard`、`NotificationProgressBar`、`WindowNotificationManager`、`INotificationManager`。
- 枚举：`NotificationPosition`、`NotificationType`。

`NotificationType.Default` 表达普通通知语义，默认不显示类型图标，也不投射 success/info/warning/error 状态伪类。`Information`、`Success`、`Warning` 和 `Error` 表达带类型通知语义，在未设置自定义 `Icon` 时使用对应状态图标，并参与状态颜色 selector。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_CloseButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_Items` | `ItemsControl` | 承载 manager 的稳定卡片集合和共享 Stack panel。 |
| `PART_Layout` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

控件专属或内部伪类包括 `BottomCenter=:bottomcenter`、`BottomLeft=:bottomleft`、`BottomRight=:bottomright`、`NotificationPseudoClass.BottomCenter`、`NotificationPseudoClass.BottomLeft`、`NotificationPseudoClass.BottomRight`、`NotificationPseudoClass.TopCenter`、`NotificationPseudoClass.TopLeft`、`NotificationPseudoClass.TopRight`、`TopCenter=:topcenter`、`TopLeft=:topleft`、`TopRight=:topright`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 4. 行为与状态模型

Notification 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- selection/checked/active、loading/async、collection/filter、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

Notification 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `NotificationCardTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `NotificationProgressBarTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `WindowNotificationManagerTheme.axaml` | 定义弹层、窗口或 overlay 宿主视觉。 |

Notification 使用 internal `NotificationCardToken` 作为控件 Token scope。Token 只表达卡片背景、尺寸、padding、图标、进度和外部间距等视觉语义，不承载 Stack、剩余时长或关闭状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

列表的公开样式入口是 `WindowNotificationManager.Padding` 与生成的 `WindowNotificationManagerListContentStyle`。
前者控制队列边缘间隔；后者以 `ItemsControl` 为公共目标，支持宽度、最小宽度与对齐等属性。
项间距和卡片顺序由内部反馈栈管理，不能以 `Spacing` / `ReverseOrder` 作为 listContent 的公开 Setter。

## 6. 控件家族或集成关系

Notification 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `Notification`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationCard`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationProgressBar`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationProgressBarVisibleConverter`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationCardToken`：internal 控件 Token scope，负责从全局 token 派生卡片视觉变量。
- `WindowNotificationManager`：数据、状态或行为协作类型，维护集合同步和事件路径。
- 共享 `FeedbackStackPresenter` / `FeedbackStackPanel` / `FeedbackLifetimeScheduler`：维护堆叠布局与生命周期调度，不拥有 Notification public 内容语义。
- 共享 `FeedbackCardMotion` / `FeedbackCardMotionCoordinator`：维护位置感知的进入/退出曲线、单 actor 执行和取消释放，不进入 public API。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Notification 时必须保持以下不变量：

- Stack API、默认值与共享基础设施文档构成当前契约；后续不得仅修改 Notification 一侧而造成两个管理器同名 API 语义分叉。
- 带宿主的多个反馈 manager 由 `WindowFeedbackLayer` 按最近成功提交的 `Show` 原子激活；Notification 不以 card `ZIndex`
  或 Gallery manager 创建顺序表达跨 manager 层级。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 进度与当前时长模型

`NotificationProgressBar.CurrentExpiration` 是 scheduler 剩余时间经 NotificationCard 对进度模板的单向投影，只为可见进度提供状态。模板不能通过修改该值反向重排 scheduler deadline；隐藏、关闭、禁用进度、重套模板或 detach 时必须停止进度刷新。

### 8.2 集合与数据同步模型

Manager 持有稳定集合，模板只通过 `ItemsSource` 消费；重套模板不能清空或重新创建卡片。`MaxItems` 为正数时淘汰最旧活动项，`DestroyAll()` 为全部活动项发起一次正常关闭。业务内容对象不能反向持有 manager 或 presenter。

### 8.3 动效模型

Notification 的实际 Frame 与外层队列 transform 职责独立但同时运行。卡片按 Position 从宿主边方向的 `64 DIP` 偏移
连续淡入，退出沿同一方向离场，scale 在进入/退出期间始终为 `1`；已有通知同步平滑让位。两类过渡均使用
SharedToken `MotionDurationMid` 和 `cubic-bezier(0.645, 0.045, 0.355, 1)`。超过阈值时最多显示最新三张真实卡片，
实际可见数为 `Min(3, StackThreshold)`，Stack scale 为 `1`、`0.94`、`0.88`。每张卡片以真实测量高度参与 8 DIP
折叠边缘定位；hover 展开全部。关闭项保留最后一次投影独立离场，其余项从关闭开始同步重排。初始投影、禁用 motion、
重套模板和卸载路径收敛或取消旧目标，不允许旧 completion 改写当前状态。进入先保持透明准备覆盖，
布局就绪后由 Motion 同步建立偏移起点并从原生时钟零进度推进，首张和后续卡片使用相同播放协议。

### 8.4 视觉选项模型

Notification 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

### 8.5 生命周期计时模型

`IsStackEnabled` 默认为 `false`，`StackThreshold` 默认为 `3`；应用必须显式开启 Stack。默认自动关闭时长为 4.5 秒，零时长永久展示。一个 manager 只使用一个惰性共享调度器；没有可见进度时按最近 deadline
唤醒，有进度时只刷新可见活动项。Stack 开启时，列表 hover 暂停全部活动卡片，即使数量没有超过阈值；Stack 关闭时
只暂停实际悬停卡片。继续时使用剩余时长。

### 8.6 宿主层激活模型

带宿主的 `WindowNotificationManager` 成功把新卡片加入稳定集合后，由 `WindowFeedbackLayer` 将该 manager 作为一个原子
反馈组激活到窗口反馈层栈顶。重复向当前栈顶 manager 显示通知不会重排宿主；切换 manager 时只移动直接 manager 子项。
每个 manager 内仍由 `FeedbackStackPanel` 保证最新卡片靠近宿主边缘，多个 manager 的历史卡片不合并为一个全局队列。
无宿主的 inline manager 继续由应用视觉树决定层级。该内部契约不新增 Public API，也不改变 Position、Stack、MaxItems、
样式作用域或 `DestroyAll()` 的 manager 边界。

### 8.7 Gallery Stack 示例

Gallery 的 Stack 示例使用独立 manager，不与基础、类型、placement、进度和自定义关闭示例共享配置或 `DestroyAll()`
范围。示例显式以 Enabled 开启、Threshold 为 `3` 启动，每次打开交替创建短内容和长内容的零时长通知，便于持续
观察不同卡片高度下的折叠、hover 展开、运行时开关和阈值变化。配置标签、ToggleSwitch 与 NumericUpDown 使用同一垂直
中心线，示例卡片以 `v6.1.9` RibbonBadge 标记能力引入版本。

## 9. 文档导航与验证策略

关联文档：

- [Notification 桌面版实现原理](implementation.md)
- [Notification Token 设计](token.md)
- [Notification Semantic Part 契约](semantic-part.md)
- [Notification Changelog](changelog.md)
- [Feedback 堆叠基础设施](../../../../architecture/systems/control-infrastructure/feedback-stack.md)

Semantic Part 语义区域：

| Part | AtomUI 节点 | 职责 | 相关 API | 相关 Token | 稳定性 |
| --- | --- | --- | --- | --- | --- |
| `root` | `NotificationCard` / `WindowNotificationManager` owner | 通知项与通知列表根语义区域，承载 public API、状态与主题入口。 | 见 API 与契约模型 | 见视觉与主题模型 | stable |
| `wrapper` | `DockPanel#Wrapper` | 图标与内容区域的包裹元素，决定排列方向、对齐与图标间距。 | `Icon`、`Title`、`Content` | `NotificationIconMargin` | stable |
| `icon` | `IconPresenter#IconPresenter` | 状态图标元素：尺寸、画刷与行高。 | `Icon`、`NotificationType` | `NotificationIconSize`、状态色 SharedToken | stable |
| `section` | `StackPanel#Section` | 标题与描述的内容区域，决定两者纵向间距。 | `Title`、`Content` | `NotificationSectionSpacing` | stable |
| `title` | `atom:SelectableTextBlock#HeaderTitle` | 标题元素：颜色、字号、行高与右侧留白。 | `Title` | `FontSizeLG`、`FontHeightLG`、`NotificationTitlePadding` | stable |
| `description` | `ContentPresenter#Content` | 描述元素：颜色、字号、行高与换行。 | `Content`、`ContentTemplate` | `FontSize`、`FontHeight` | stable |
| `actions` | `ContentPresenter#ActionsContainer` | 操作组元素：notice 右下角的操作入口。 | `Actions`、`ActionsTemplate` | `NotificationActionsMargin` | stable |
| `close` | `IconButton#PART_CloseButton` | 关闭按钮覆盖层：位置、尺寸、圆角与交互色。 | `Close()`、`NotificationClosed` | `NotificationCloseButtonSize`、`NotificationCloseButtonMargin` | stable |
| `progress` | `NotificationProgressBar#ProgressBar`（运行时创建） | 进度覆盖元素：展示自动关闭剩余时间。 | `IsShowProgress`、`Expiration` | `NotificationProgressHeight`、`NotificationProgressBg` | stable |
| `listContent` | `FeedbackStackPresenter#PART_Items` | 列表内容区域；公共契约为 ItemsControl，支持尺寸与对齐定制。 | `Position`、`MaxItems` | `UniformlyMargin` | stable |

完整 Part 表、Selector 用法与定制边界见 [Notification Semantic Part 契约](semantic-part.md)。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 selection/checked/active、loading/async、collection/filter、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
