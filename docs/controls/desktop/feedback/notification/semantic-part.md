# Notification Semantic Part 契约

本文档定义 Notification 对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。Notification 的整体设计见
[Notification 桌面版架构设计](overview.md)，内部实现原理见 [Notification 桌面版实现原理](implementation.md)，Token 设计见
[Notification Token 设计](token.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Notification 由两个 public owner 组成，因此公开两个独立 descriptor：

| Owner | 职责 |
| --- | --- |
| `NotificationCard` | 单条通知卡片（上游 notice），承载 `root`、`wrapper`、`icon`、`section`、`title`、`description`、`actions`、`close`、`progress` 九个 Part。 |
| `WindowNotificationManager` | 服务型通知宿主（上游 notification list），承载 `root`（= 上游 `list`）与 `listContent` 两个 Part。 |

上游 `NotificationSemanticType` 的 `classNames` / `styles` 为
`{ list?, listContent?, wrapper?, root?, title?, description?, actions?, icon?, section?, close?, progress? }` 十一个键
（上游 6.6.3 稳定发布源码 `notification/interface.ts`，语义 demo
`notification/demo/_semantic.tsx`）。上游 DOM 嵌套为
`list > listContent > root > [ wrapper > (icon, section > (title, description)), actions, close, progress ]`；
`root`、`icon`、`title`、`description`、`actions` 自 6.0.0 起公开，`wrapper`、`section`、`close`、`progress`、
`list`、`listContent` 自 6.4.0 起公开。

AtomUI 把十一个上游 Part 一一映射到两个 owner 的真实节点：

```text
WindowNotificationManager (root，对应上游 list)
  └─ FeedbackStackPresenter#PART_Items (listContent，公共契约 ItemsControl，Margin 消费 manager.Padding)
       └─ ItemsPresenter / FeedbackStackPanel
            └─ NotificationCard (root，对应上游 notice root)
                 └─ Border#Frame (表面投影，Padding=0)
                      └─ FeedbackStackTransitionSnapshotHost#PART_StackTransitionSnapshotHost
                           └─ Grid#PART_Layout
                                ├─ Border#ContentBox (消费 card.Padding)
                                │    └─ StackPanel
                                │         ├─ DockPanel#Wrapper (wrapper)
                                │         │    ├─ IconPresenter#IconPresenter (icon)
                                │         │    └─ StackPanel#Section (section)
                                │         │         ├─ SelectableTextBlock#HeaderTitle (title)
                                │         │         └─ ContentPresenter#Content (description)
                                │         └─ ContentPresenter#ActionsContainer (actions)
                                ├─ IconButton#PART_CloseButton (close，右上角覆盖)
                                └─ NotificationProgressBar#ProgressBar (progress，运行时创建、跨列贴底)
```

上游 `root` 就是 notification notice 本身，因此映射到 `NotificationCard` owner；上游 `list` 是承载全部 notice 的
定位容器，映射到 `WindowNotificationManager` owner。两个 owner 各自拥有隐式 `root`，不额外声明 `.semantic-root` marker。

结构与布局边界：

- manager 铺满宿主反馈层，安全区外边距由 `TopLevelMarginBinder` 提供；列表内边距由 owner `Padding` 提供，
  投影到 presenter 的 `Margin`。`Position` 决定六种对齐，内联使用时定位作用于调用方分配的布局范围。
- `listContent` 公共类型为 `ItemsControl`。实际 presenter 和 panel 管理可变高度卡片的堆叠、展开与项间距。
- `Frame` 是模板根和 Motion 目标，绘制背景、边框、阴影。card `Padding` 由内部 `ContentBox` 消费；
  `Frame` 自身零内边距，使关闭按钮偏移从边框内侧计算，进度条能够贴底。
- `close` 与内容在 Grid 第一行重叠，右上对齐；`progress` 运行时加入第二行并跨两列，以有限的卡片内容宽度测量。
  两者均不嵌入 `wrapper` / `section` / `actions` 的正文流。快照 host 只拥有折叠过渡期间的内容快照，不公开 Part。
- `wrapper` 使用 DockPanel，icon 的右外边距取 `NotificationIconMargin`，并与标题首行顶部对齐。
  `section` 使用 StackPanel，其 `Spacing` 取 `NotificationSectionSpacing`。

两个 owner 的 descriptor `Since` 统一为 `6.2.0`（AtomUI Semantic Part 首版约定，不逐 Part 记录上游小版本）。

以下类型不持有独立 Semantic descriptor：

- `Notification`（`INotification` 的实现）是服务调用的数据对象，不是 Control。
- `INotification` / `INotificationManager` 是服务契约接口。
- `NotificationPseudoClass`、`NotificationPosition`、`NotificationType` 是状态与枚举类型。
- `NotificationCardToken`、`NotificationProgressBar`、`NotificationProgressBarVisibleConverter` 与各 Motion 类型是
  Token scope、内部进度绘制与动效实现；Semantic Part 不产生 Token identity。

### 1.1 `NotificationCard`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `root` |
| Selector | NotificationCard 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `NotificationCard` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard owner（表面投影到 `Border#Frame`，动效由 Frame 承载） |
| 职责 | 通知项根元素：承载 `Title`、`Content`、`Icon`、`Actions`、`NotificationType`、`IsClosing`、`IsClosed`、`IsMotionEnabled` 与进入/退出动效；根表面（背景、边框、圆角、阴影、内边距）投影到模板中的 `Border#Frame`。对应上游 notice root。 |
| 相关 API | `Title`、`Content`、`Icon`、`Actions`、`ActionsTemplate`、`NotificationType`、`IsClosing`、`IsClosed`、`IsMotionEnabled`、`Close()`、`NotificationClosed` |
| 相关 Token | `NotificationBg`、`NotificationPadding`、SharedToken（`BoxShadows`、`BorderRadiusLG`） |
| 稳定性 | stable since 6.2.0 |

#### `wrapper`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `wrapper` |
| Selector | `.semantic-wrapper` |
| SelectorRoute | `/template/ .semantic-wrapper` |
| Style Type | `NotificationCardWrapperStyle` |
| ContractType | `DockPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `DockPanel#Wrapper` |
| 职责 | 图标与内容区域的包裹元素：决定 icon 与 section 的排列方向、对齐与图标间距。对应上游 notice wrapper（flex 行，`align-items: flex-start`）。 |
| 相关 API | `Icon`、`Title`、`Content`（决定子节点可见性） |
| 相关 Token | `NotificationIconMargin` |
| 稳定性 | stable since 6.2.0 |

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `/template/ .semantic-icon` |
| Style Type | `NotificationCardIconStyle` |
| ContractType | `IconPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `IconPresenter#IconPresenter` |
| 职责 | 状态图标元素：尺寸、画刷与行高；`NotificationType` 决定默认图标与状态色（Information = `ColorPrimary`，Success = `ColorSuccess`，Warning = `ColorWarning`，Error = `ColorError`）。对应上游 notice icon。 |
| 相关 API | `Icon`、`NotificationType` |
| 相关 Token | `NotificationIconSize`、`NotificationIconMargin`、SharedToken（`ColorPrimary`、`ColorSuccess`、`ColorWarning`、`ColorError`） |
| 稳定性 | stable since 6.2.0 |

#### `section`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `section` |
| Selector | `.semantic-section` |
| SelectorRoute | `/template/ .semantic-section` |
| Style Type | `NotificationCardSectionStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `StackPanel#Section` |
| 职责 | 包含标题与描述的内容区域元素：决定两者的纵向间距与对齐。对应上游 notice section（flex column，`gap: marginXS`）。 |
| 相关 API | `Title`、`Content` |
| 相关 Token | `NotificationSectionSpacing` |
| 稳定性 | stable since 6.2.0 |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-title` |
| Style Type | `NotificationCardTitleStyle` |
| ContractType | `AtomUI.Desktop.Controls.SelectableTextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `atom:SelectableTextBlock#HeaderTitle`（AtomUI 类型，主题中带 `atom:` 前缀） |
| 职责 | 标题元素：文本颜色、字号、行高与右侧留白（为关闭按钮预留）。对应上游 notice title。 |
| 相关 API | `Title` |
| 相关 Token | SharedToken（`FontSizeLG`、`FontHeightLG`、`ColorTextHeading`）、`NotificationTitlePadding` |
| 稳定性 | stable since 6.2.0 |

#### `description`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `description` |
| Selector | `.semantic-description` |
| SelectorRoute | `/template/ .semantic-description` |
| Style Type | `NotificationCardDescriptionStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `ContentPresenter#Content` |
| 职责 | 描述元素：正文颜色、字号、行高与换行。对应上游 notice description（在标题下方，不与关闭按钮同排，因此不预留右侧空间）。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | SharedToken（`FontSize`、`FontHeight`、`ColorText`） |
| 稳定性 | stable since 6.2.0 |

#### `actions`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `actions` |
| Selector | `.semantic-actions` |
| SelectorRoute | `/template/ .semantic-actions` |
| Style Type | `NotificationCardActionsStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `ContentPresenter#ActionsContainer` |
| 职责 | 操作组元素：位于 notice 右下角，承载调用方传入的操作内容；`Actions` 为空时节点隐藏。对应上游 notice actions。 |
| 相关 API | `Actions`、`ActionsTemplate` |
| 相关 Token | `NotificationActionsMargin` |
| 稳定性 | stable since 6.2.0 |

#### `close`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `close` |
| Selector | `.semantic-close` |
| SelectorRoute | `/template/ .semantic-close` |
| Style Type | `NotificationCardCloseStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `IconButton#PART_CloseButton` |
| 职责 | 关闭按钮覆盖元素：位置（右上角）、尺寸、圆角、默认/悬停/按下颜色与聚焦样式。对应上游 notice close（absolute top-right）。 |
| 相关 API | `Close()`、`NotificationClosed` |
| 相关 Token | `NotificationCloseButtonSize`、`NotificationCloseButtonPadding`、`NotificationCloseButtonMargin`、SharedToken（`BorderRadiusSM`、`IconSizeSM`、`ColorIcon`、`ColorIconHover`、`ColorBgTextHover`、`ColorBgTextActive`） |
| 稳定性 | stable since 6.2.0 |

#### `progress`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `progress` |
| Selector | `.semantic-progress` |
| SelectorRoute | `/template/ .semantic-progress` |
| Style Type | `NotificationCardProgressStyle` |
| ContractType | `Control` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `NotificationCard` 运行时创建的 `NotificationProgressBar#ProgressBar`（贴卡片底边，左右内缩 `BorderRadiusLG`） |
| 职责 | 进度覆盖元素：展示自动关闭的剩余时间。对应上游 notice progress（absolute bottom）。 |
| 相关 API | `IsShowProgress`、`Expiration` |
| 相关 Token | `NotificationProgressHeight`、`NotificationProgressBg`、`NotificationProgressMargin` |
| 稳定性 | stable since 6.2.0 |

### 1.2 `WindowNotificationManager`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `WindowNotificationManager` |
| Part | `root` |
| Selector | WindowNotificationManager 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `WindowNotificationManager` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | WindowNotificationManager owner（宿主层/full-screen 覆盖层；无宿主构造时为内联可放置实例） |
| 职责 | 通知列表根元素：承载 `Position`、`MaxItems`、`IsMotionEnabled`、`IsPauseOnHover`，管理宿主层安装、通知队列、超时关闭与宿主 detach；对应上游 `list` 的定位/层级/宽度与边缘内边距语义。 |
| 相关 API | `Position`、`MaxItems`、`IsMotionEnabled`、`IsPauseOnHover`、`Padding`、`IsStackEnabled`、`StackThreshold`、`Show(INotification)`、`DestroyAll()`、`Dispose()` |
| 相关 Token | `NotificationTopMargin`、`NotificationBottomMargin`、SharedToken（`EnableMotion`） |
| 稳定性 | stable since 6.2.0 |

#### `listContent`

| 字段 | 值 |
| --- | --- |
| Owner | `WindowNotificationManager` |
| Part | `listContent` |
| Selector | `.semantic-list-content` |
| SelectorRoute | `/template/ .semantic-list-content` |
| Style Type | `WindowNotificationManagerListContentStyle` |
| ContractType | `ItemsControl` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | WindowNotificationManager 模板中的 `FeedbackStackPresenter#PART_Items` |
| 职责 | 通知列表内容元素：notice 的排列方向、顺序、对齐与项间距；对应上游 `listContent` 的 notice 排列/间距语义。 |
| 相关 API | `Position`、`MaxItems`、`IsStackEnabled`、`StackThreshold` |
| 相关 Token | SharedToken（`EnableMotion`、`UniformlyMargin`） |
| 稳定性 | stable since 6.2.0 |

隐式 `root` 不声明 `.semantic-root` marker。`NotificationCard` 的 `wrapper`、`icon`、`section`、`title`、
`description`、`actions`、`close` marker 静态声明在 `NotificationCardTheme.axaml` 模板内；`progress` 由控件运行时
创建并在创建路径注入 semantic class；`WindowNotificationManager` 的 `listContent` marker 静态声明在
`WindowNotificationManagerTheme.axaml` 模板内。

## 2. Part 说明

### 2.1 NotificationCard root

`root` 是 `NotificationCard` owner 本身，每条可见通知恰好一个。它承载 `Title`、`Content`、`Icon`、`Actions`、
`NotificationType` 内容/状态 API，`IsClosing` / `IsClosed` 关闭状态，以及 `IsMotionEnabled` 与进出场动效。

它负责：

- 单条通知的根语义与状态机：`Close()` 置 `IsClosing`，关闭动效完成后提交一次 `IsClosed=true` 并抛出
  `NotificationClosed`。
- 根视觉表面：背景（`NotificationBg`）、圆角（`BorderRadiusLG`）、阴影（`BoxShadows`）投影到 `Border#Frame`；
  内边距（`NotificationPadding`）投影到 `Frame` 内部的 `Border#ContentBox`（原因见 §1 的 padding box 说明）。
  卡片**不承担**通知间距：`root` 的几何等于可见卡片本身（四边零外边距），
  通知间距由内部 `FeedbackStackPresenter` / `FeedbackStackPanel` 管理（见 §5）。
- 作为 `wrapper` / `icon` / `section` / `title` / `description` / `actions` / `close` / `progress`
  owner-scoped Selector 的作用域边界。

适合通过 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`BoxShadow`、`Padding` 等 owner 侧 Setter
定制整卡视觉——背景/圆角/阴影/边框经模板内 `Border#Frame` 的 `TemplateBinding` 投影到卡片外框，`Padding` 经
`Border#ContentBox` 投影到内容内边距，是上游 `root` styles
（背景色、圆角、阴影、内边距、边框）的 AtomUI 等价表达。`root` 不表示模板中的
`Border#Frame` 或 `Grid#PART_Layout` 节点本身。

### 2.2 wrapper / icon / section / title / description / actions

六个 Part 在 `NotificationCard` 模板内各对应一个常驻节点，cardinality 均为 `Single`：

- `wrapper` 承载 `DockPanel#Wrapper`：icon 停靠左侧、section 填充剩余宽度；`NotificationIconMargin` 提供图标与
  文本之间的间距。适合定制 `Padding`、`Margin`、`HorizontalAlignment` 等布局属性。
- `icon` 承载 `IconPresenter#IconPresenter`：`Icon` 为空且 `NotificationType` 为默认时隐藏；设置类型或自定义
  `Icon` 时显示。适合定制 `Width`、`Height`、`Margin`、`IconBrush`。
- `section` 承载 `StackPanel#Section`：标题与描述的共同父级，`NotificationSectionSpacing` 提供两者纵向间距。
  适合定制 `Spacing`、`Orientation` 等布局属性。
- `title` 承载 `atom:SelectableTextBlock#HeaderTitle`：`Title` 文本呈现，支持文本选择。适合定制 `Foreground`、
  `FontSize`、`LineHeight`、`Padding`。
- `description` 承载 `ContentPresenter#Content`：`Content` 呈现，支持 `ContentTemplate` 与文本换行。适合定制
  `Foreground`、`FontSize`、`LineHeight`、`Padding`。
- `actions` 承载 `ContentPresenter#ActionsContainer`：`Actions` 为空时整体隐藏；非空时按默认右上角对齐显示。
  适合定制 `Margin`、`HorizontalAlignment` 与内容模板。

`icon` 的可见性由 `Icon` / `NotificationType` 数据状态驱动；Semantic Style 覆盖 `IsVisible` 会绕过数据状态机，属于
不推荐用法。

### 2.3 close / progress

两个 Part 是覆盖层，cardinality 均为 `Single`：

- `close` 承载 `IconButton#PART_CloseButton`，放在 `Grid#PART_Layout` 上并以右上角对齐，因此不占用 `wrapper` /
  `actions` 的排版空间，对齐上游 absolute top-right。适合定制位置（`Margin`）、尺寸、圆角与交互色。
- `progress` 由 `ConfigureProgressBar` 运行时创建 `NotificationProgressBar#ProgressBar` 并注入
  `semantic-progress` class，只在 `IsShowProgress` 为真且 `Expiration` 非空时存在；节点贴卡片底边、左右内缩
  `BorderRadiusLG`，对齐上游 absolute bottom。`IsShowProgress` 关闭或 `Expiration` 清空时节点与 marker 一起释放。
  适合定制 `Margin`、`Height`（通过 `NotificationProgressHeight`）与进度画刷。

### 2.4 WindowNotificationManager root / listContent

两个 Part 在 `WindowNotificationManager` 内各对应一个稳定节点，cardinality 均为 `Single`：

- `root` 承载 manager owner：管理 `Position`、列表 `Padding`、稳定卡片集合、`MaxItems`、Stack 配置与生命周期。
  宿主构造安装到 `WindowFeedbackLayer`（回退 `AdornerLayer`）；无参构造或传入 `null` 时由调用方放进视觉树。
- `listContent` 承载 `FeedbackStackPresenter#PART_Items`，公共 `ContractType` 为 `ItemsControl`。它通过
  `ItemsSource` 消费稳定卡片集合；内部 panel 按 `Position`、卡片高度和 Stack 状态计算排列、层级与间距。

通过 owner `Padding` 定制列表边缘间隔，通过生成的 `WindowNotificationManagerListContentStyle` 定制
`ItemsControl` 的宽度、最小宽度、对齐等公共属性。`Spacing`、`ReverseOrder` 不是该 Part 的公共属性。
内部反馈栈维护展开间距和折叠层级，不提供新的应用 API。

## 3. Selector 用法

应用级样式先限定 owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|NotificationCard">
        <atom:NotificationCardWrapperStyle x:SetterTargetType="DockPanel">
            <Setter Property="Margin" Value="0" />
        </atom:NotificationCardWrapperStyle>
        <atom:NotificationCardIconStyle x:SetterTargetType="atom:IconPresenter">
            <Setter Property="IconBrush" Value="#1677FF" />
        </atom:NotificationCardIconStyle>
        <atom:NotificationCardTitleStyle x:SetterTargetType="atom:SelectableTextBlock">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:NotificationCardTitleStyle>
        <atom:NotificationCardDescriptionStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Foreground" Value="#3F6600" />
        </atom:NotificationCardDescriptionStyle>
    </Style>

    <Style Selector="atom|WindowNotificationManager">
        <atom:WindowNotificationManagerListContentStyle x:SetterTargetType="ItemsControl">
            <Setter Property="MinWidth" Value="320" />
        </atom:WindowNotificationManagerListContentStyle>
    </Style>
</Application.Styles>
```

对特定 class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|NotificationCard:error">
    <atom:NotificationCardIconStyle x:SetterTargetType="atom:IconPresenter">
        <Setter Property="IconBrush" Value="#CF1322" />
    </atom:NotificationCardIconStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `atom|IconPresenter.semantic-icon` 或 `:is(atom|IconPresenter).semantic-icon`。
- 直接复制 `/template/ .semantic-wrapper` 等 route 作为用户主路径；route 只属于 descriptor 与生成 Style 的
  实现元数据。
- 依赖 `PART_*`、`Name` 或视觉祖先顺序。
- 通过 Semantic Style 设置 `icon` 的 `IsVisible` 绕过 `Icon` / `NotificationType` 数据状态机。
- 通过 Semantic Style 设置 `progress` 的存在性；该节点由 `IsShowProgress` 与 `Expiration` 数据状态决定。

## 4. 状态与数量语义

数量契约以已实例化的 AtomUI 内置节点为边界。除 `progress` 外，各 Part 都是 owner 自身 `ControlTheme` 的静态模板
节点（`RuntimeCreated=false`），marker 随 owner 实例存在，不随数据项迁移；`progress` 是 RuntimeCreated Part，
只在进度可见时存在。

| 场景 | NotificationCard 各 Part | WindowNotificationManager root / listContent | 说明 |
| --- | --- | --- | --- |
| 单条通知 | 各 1 | 各 1 | 每条可见通知一个 NotificationCard。 |
| N 张已实例化卡片 | 各 N | 各 1 | manager 只有一个模板实例与一个 `PART_Items`；动态 progress 仍按其存在条件计数。 |
| `Icon` 为 null 且默认类型 | `icon` marker 保留，节点 `IsVisible=false` | — | 高亮与命中只针对有效可见实例。 |
| Stack 折叠 / 展开 | 已实例化卡片的 marker 数量保持 | 各 1 | 隐藏层仍属于卡片集合；可见性影响高亮与命中，不销毁 marker。 |
| 自定义 `Icon` | `icon` marker 保留 | — | 自定义图标不新增节点。 |
| `Actions` 为空 | `actions` marker 保留，节点 `IsVisible=false` | — | 提供操作后节点显示，不新增节点。 |
| `IsShowProgress=false` 或 `Expiration` 为空 | `progress` 节点与 marker 都不存在 | — | `Optional` 语义由运行时创建路径表达。 |
| 显示进度 | `progress` 恰 1 | — | 节点贴底覆盖，不改变其它 Part 数量。 |
| 超时自动关闭 | 关闭后随卡片移除 | 各 1 | `NotificationClosed` 后 manager 从稳定卡片集合移除卡片。 |
| 手动 `Close()` | 同上 | 各 1 | 与超时走同一关闭动效状态流。 |
| 超过正数 `MaxItems` | 最旧活动项关闭后移除 | 各 1 | 关闭中的卡片不计入活动项；`MaxItems <= 0` 不限制。 |
| 持续宿主 detach | 卡片关闭后移除 | root 随 manager 卸载 | detach 级联完成后仍未重新入树时关闭卡片；同轮重新入树保留队列。 |
| `Dispose` | 全部移除 | root 随 manager 卸载 | 解绑 presenter、释放 scheduler、card owner、回调和宿主层。 |

## 5. 尺寸基线

Notification 没有 `SizeType` 分档，视觉基线由 `NotificationCardToken` 与全局 Token 常量表达：

- 卡片背景 `NotificationBg`（`ColorBgElevated`），内边距 `NotificationPadding`
  （垂直 `UniformlyPaddingMD`，水平 `UniformlyPaddingLG`，上下左右对称），严格取上游
  `notificationPadding = paddingMD paddingLG` 的同名值，不做比例缩放。
- 圆角 SharedToken `BorderRadiusLG`，阴影 SharedToken `BoxShadows`。
- 宽度 `NotificationWidth`（384），对齐上游 `width: 384`。
- `wrapper` 的图标间距 `NotificationIconMargin`（右 `UniformlyMarginSM`），对齐上游 notice wrapper `gap: marginSM`。
- `section` 的标题与描述间距 `NotificationSectionSpacing`（`UniformlyMarginXS`），对齐上游 `gap: marginXS`。
- 标题右侧留白 `NotificationTitlePadding`（右 `UniformlyPaddingLG`），对齐上游
  `.notice-closable` 的 `padding-inline-end: paddingLG`；描述在标题下方、不与关闭按钮同排，按上游
  `.notice-title + .notice-description { padding-inline-end: 0 }` 不预留右侧空间。
- `close` 尺寸 `NotificationCloseButtonSize`（`ControlHeightLG * 0.55`，对齐上游 `notificationCloseButtonSize`）、
  内间距 `NotificationCloseButtonPadding`（`PaddingXXS`）、覆盖层偏移 `NotificationCloseButtonMargin`
  （上 `UniformlyPaddingMD`、右 `UniformlyPaddingLG`），对齐上游 `top: notificationPaddingVertical` 与
  `inset-inline-end: notificationPaddingHorizontal`。
- `actions` 上边距 `NotificationActionsMargin`（`UniformlyMarginSM`），对齐上游 `margin-top: marginSM`。
- `progress` 高度 `NotificationProgressHeight`（2）、左右内缩 `NotificationProgressMargin`（`BorderRadiusLG`）、
  彩色进度值画刷 `NotificationProgressBg`（`ColorPrimaryBorderHover -> ColorPrimary` 渐变）、底槽
  `NotificationProgressTrackBg`（`ColorFillQuaternary`，对齐上游 `rgba(0, 0, 0, 0.04)`），对齐上游 progress
  覆盖层「底槽 + 进度值」两层绘制；缺底槽时剩余时间不可见。
- 文本字号/行高/颜色：标题 SharedToken `FontSizeLG` / `FontHeightLG` / `ColorTextHeading`，描述 SharedToken
  `FontSize` / `FontHeight` / `ColorText`。上游 notice title 为 `fontSizeLG` / `lineHeightLG`（16 / 1.5，行高 24），
  section 为 `gap: marginXS`（8）。
- 图标垂直对齐：上游 notice wrapper 是 `display: flex; align-items: flex-start`，图标与标题行顶部对齐。AtomUI
  的 `IconPresenter` 在 `DockPanel` 中默认 `Stretch` 且带显式 `Height`，会被垂直居中而低一截，因此主题显式设置
  `VerticalAlignment="Top"`。

`WindowNotificationManager.Padding` 的默认值由 owner 的 `Position` 选择器提供：顶部位置使用
`NotificationTopMargin`，底部位置使用 `NotificationBottomMargin`。前者为左/上/右 `UniformlyMarginLG`、下 0；
后者为左/下/右 `UniformlyMarginLG`、上 0。应用显式设置 Padding 可以覆盖默认值；切换位置仍保留显式值。
`NotificationPadding` 只表达卡片内容内边距，不承担 manager 的边缘定位。

### 5.1 list / listContent / root 的几何归属

| 区域 | AtomUI 节点 | 承担 |
| --- | --- | --- |
| list | `WindowNotificationManager`（`root`） | `Position` 与 `Padding`，Padding 投影到 presenter.Margin |
| listContent | `FeedbackStackPresenter#PART_Items`（公共契约 `ItemsControl`） | 实际队列范围；内部 panel 管理排列、堆叠与项间距 |
| notice | `NotificationCard`（`root`） | 单条通知表面，卡片自身零外边距 |

TopRight 的上边与右边间隔分别取 manager `Padding.Top` / `Padding.Right`；BottomLeft 对应下边和左边。
列表紧贴可见队列，空白区域不应扩大 hover 命中范围。卡片 root 的 Bounds 与表面 Border 一致；列表按定位边
锚定，不要求相对整个 manager 的四边间隔相等。

### 5.2 单预览合并两个 owner

对齐上游 `notification#semantic-dom` 的单预览形态：一个 `SemanticPartPreview` 通过 `SemanticOwners` 声明
`WindowNotificationManager` 与 `NotificationCard` 两个 owner，Part 列表按 owner 声明顺序合并（11 项）。`root` 等
重名路径在合并列表里靠 `SemanticPartDescription.OwnerType` 消歧，列表行的 owner 标签用于区分归属；高亮按 Part
所属 owner 解析，只会在该 owner 的实例作用域内命中。多 owner 时每一条描述都必须声明 `OwnerType`，否则报错而不是
静默归属。

`NotificationCard` 提供公开无参构造，因此预览里的卡片可以直接声明式实例化（manager 构造路径仍用于运行时队列）。

### 5.3 反馈层样式作用域

宿主构造下 `WindowNotificationManager` 会被挂进 `WindowFeedbackLayer`，该层在页面视觉树之外。样式作用域实测结论与
Message 一致：页面视觉树内的 `Style` / `Styles` 命中不到反馈层卡片，样式必须落在 manager 自身、Window 或
Application 之一。Gallery 的 `Custom Semantic Part styling` 示例把生成的专用 Style 类写在页面 AXAML 的
`UserControl.Resources` 里（声明式、类型安全），再由 code-behind 挂到 manager 的 `Styles`；挂载是唯一的代码步骤，
不涉及任何部件属性改写。

资源查找必须走声明该 `Styles` 的资源宿主。写在 `UserControl.Resources` 的样式只有该页面自己的
`Resources.TryGetResource(...)` 能看到；用 `Application.Current.TryGetResource(...)` 查找会返回 false，样式被静默跳过、
卡片回落默认外观（本页最初即因此失效，已由
`Notification_Semantic_Style_Buttons_Produce_Styled_Cards` 端到端锁定）。若把 `Styles` 放到 `Application.Resources`，
才改用 `Application.Current` 查找，二者不能混用。

状态分支（如 `semantic-error-style-demo:error`）要求卡片的 `NotificationType` 伪类成立。`NotificationCard` 在挂到视觉树
后由 `SetupNotificationTypePseudoClasses` 设置 `:error` / `:success` 等伪类，因此按类型分支的样式只在类型伪类生效后
命中；`Show` 路径会把 `NotificationType` 传给卡片，满足该前提。

## 6. 定制边界

以下区域明确不属于 Notification Semantic Part：

- 关闭动效执行状态（Motion）、`Close()` 调度与 `IsClosed` 提交路径是
  行为状态，不是 Part。
- `Border#Frame`、`Grid#PART_Layout`、`Border#ContentBox`、`StackPanel` 是模板结构节点：`root` 的表面投影到 `Frame`，
  内边距投影到 `ContentBox`，但节点名称、数量与层级不属于 `root` 契约。
- 稳定卡片集合、`FeedbackLifetimeScheduler`、`MaxHostLayerRetryCount` 重试与宿主层
  安装/卸载是 manager 的行为实现，不是 Part。
- `NotificationCard` 的默认图标选择（`SetupDefaultNotificationIcon`）与 `NotificationType` 伪类是行为状态；
  `icon` Part 只表达图标元素的视觉。
- `NotificationProgressBar` 的绘制算法（剩余时间比例、`Render` 矩形）是实现细节；`progress` Part 只表达该覆盖
  元素的视觉区域。
- `Notification`、`INotification`、`INotificationManager` 数据/契约类型。

Semantic Style 服从 Avalonia 原生属性优先级。Notification 没有用户可见的颜色状态机入口：卡片状态色由
`NotificationType` 属性 Selector 在主题内设置，用户 Semantic Style setter 覆盖主题 setter，样式移除后主题恢复。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少 marker，
均属于公共主题契约变更。

验证至少覆盖：

- descriptor 中 `NotificationCard` 只有 `root`、`wrapper`、`icon`、`section`、`title`、`description`、`actions`、
  `close`、`progress`，`WindowNotificationManager` 只有 `root`、`listContent`，字段值与本文表格一致；
  `Notification`、`INotification`、`INotificationManager`、`NotificationCardToken`、`NotificationProgressBar`
  不持有 descriptor。
- 两个 owner 的主题资产各自携带批准 marker：`NotificationCardTheme.axaml` 为 `semantic-wrapper:DockPanel`、
  `semantic-icon:IconPresenter`、`semantic-section:StackPanel`、`semantic-title:SelectableTextBlock`、
  `semantic-description:ContentPresenter`、`semantic-actions:ContentPresenter`、`semantic-close:IconButton`；
  `WindowNotificationManagerTheme.axaml` 为 `semantic-list-content:FeedbackStackPresenter`；
  `progress` 不进静态模板 marker，由运行时路径注入。
- 全部 `NotificationType`、自定义 `Icon`、`Actions` 有无、进度开关、多项 queue、关闭与 detach 后 marker 数量符合
  第 4 节表。
- owner-scoped Semantic Style（九个生成 Style 类型）与 `x:SetterTargetType` 可以编译并命中对应最低 public 类型。
- 首次 attach 前 `Show` 保留卡片；持续 detach 关闭卡片，同轮重新入树保留队列；`Dispose` 释放卡片、scheduler 与宿主层引用。
- 六种 `Position` 下，默认与非默认 `Padding` 均按当前定位边投影；运行时修改内边距不能扩大队列 hover 命中区。
- `listContent` 的生成 Style 以 `ItemsControl` 为目标命中宽度、对齐等公共属性，不能依赖内部 panel 的项间距属性。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。

### 7.1 堆叠与动态部件验证

- standalone `NotificationCard` 与 manager 创建的卡片均初始化 motion coordinator，模板应用和属性修改不依赖 host。
- 正文、关闭按钮和 runtime progress 使用同一模板 owner；进度条跨列、贴底且可获得有限测量宽度。
- render-only Frame 负责进出场，外层卡片负责队列投影；折叠快照的完成、取消和卸载均释放位图与旧节点引用。
- 六种 `Position`、运行时切换、非默认 `Padding` 与 Stack 配置不改变 Part 身份；可见数量由活动卡片和堆叠状态决定。
