# Steps 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.Steps` 桌面版的最新设计定位、公共契约、状态模型、交互语义和视觉主题边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，Semantic Part 契约见 [Steps Semantic Part 契约](semantic-part.md)，内部实现原理见 [Steps 桌面版实现原理](implementation.md)，Steps Token 的专项设计见 [Steps Token 设计](token.md)，设计和契约变化记录见 [Steps Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Navigation/Steps` |
| 控件状态 | Stable |

Steps 是线性流程的状态导航条，用于表达当前步骤、已完成步骤、等待步骤、错误步骤和当前步骤的局部进度。它适合安装向导、分步表单、审批流、支付流程和任务流程。

Steps 负责：

- 展示有序步骤及其标题、副标题和详情内容。
- 根据唯一的 `Current` 输入计算每个 item 的有效状态。
- 展示 Indicator、Connector、Dot、Navigation、Inline、Panel 和 Progress 视觉。
- 在启用交互时发出当前步骤变更请求。

Steps 不负责：

- 保存或切换步骤对应的页面内容。
- 业务流程校验、路由、异步任务编排、表单提交或权限控制。
- 维护 Avalonia Selection 状态。
- 在用户激活 item 后自行修改 `Current`。

步骤页面由调用方根据 `Current` 在 Steps 外部切换。`StepsItem.Content` 表示步骤详情，不表示步骤页面。

Steps 的公开语义区域使用 `root`、`item`、`itemWrapper`、`itemIcon`、`itemTitle`、`itemSubtitle`、
`itemSection`、`itemContent` 和 `itemRail` 九个 Semantic Part，与上游步骤语义结构 `StepsSemanticType`
对齐（since 6.2.0）。`root` 是隐式 owner，`item` 覆盖 `StepsItem` 容器，七个 item 子 Part 覆盖 item 模板内
的包裹层、图标、标题、副标题、内容区、详情和连接线。完整契约见 [Steps Semantic Part 契约](semantic-part.md)。

## 2. 设计语言

Steps 表达“有序流程 + 当前进度 + 可选导航请求”。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 顺序 | item 按 `Initial + index` 获得稳定步骤编号。 | Indicator number、Connector。 |
| 当前 | `Current` 是唯一当前步骤输入。 | `IsCurrent`、当前 Indicator。 |
| 历史 | 当前步骤之前且未显式覆盖的 item 为完成。 | `StepsStatus.Finish`。 |
| 等待 | 当前步骤之后且未显式覆盖的 item 为等待。 | `StepsStatus.Wait`。 |
| 异常 | 根当前状态或 item 显式状态可以为错误。 | `StepsStatus.Error`。 |
| 导航 | 激活 item 只发出变更请求。 | `CurrentChangeRequested`。 |
| 反馈 | Wave 表达真实 pointer click，不表达状态变化。 | Indicator Wave。 |
| 密度 | 尺寸控制 Indicator、文字和间距。 | `SizeType`。 |
| 弹性 | 可用宽度不足时 item 连续收缩、标题/副标题/描述文本连续换行。 | flex 份额、`IconContainerSize` 收缩下限、Wrap 文本。 |

`StepsType.Default` 表达标准流程，`Dot` 表达实心点状流程，`OutlineDot` 表达空心点状流程，`Navigation` 强调导航入口，`Inline` 表达紧凑内联流程，`Panel` 表达面板式分段步骤。

## 3. API 与契约模型

### 3.1 根控件契约

`Steps` 继承 `ItemsControl`。它不公开或继承 Selection API。

| API | 类型 | 语义 |
| --- | --- | --- |
| `Current` | `int` | 唯一当前步骤编号，默认 `0`；控件不在激活路径中自行写入。 |
| `Initial` | `int` | 第一个 item 的编号偏移，默认 `0`；不用于初始化或重置 `Current`。 |
| `Status` | `StepsStatus` | 当前步骤的默认状态，默认 `Process`。 |
| `Percent` | `double?` | 当前 Process item 的局部进度；`null` 表示不显示。 |
| `Type` | `StepsType` | `Default`、`Dot`、`OutlineDot`、`Navigation`、`Inline` 或 `Panel`。Panel 强制采用水平等宽布局。 |
| `PanelVariant` | `StepsPanelVariant` | Panel 的视觉变体：`Filled`（默认）或 `Outlined`。其他类型忽略。 |
| `Orientation` | `Orientation` | 步骤排列方向，默认 `Horizontal`。 |
| `TitlePlacement` | `Orientation` | 标题相对 Indicator 的请求布局，默认 `Horizontal`。 |
| `SizeType` | `SizeType` | Indicator、文字和间距尺寸，默认 `Middle`。 |
| `IsItemClickable` | `bool` | 是否允许 item 发出导航请求，默认 `false`。 |
| `Offset` | `int` | Inline 类型前方保留的空 item 单元数量，默认 `0`；其他类型忽略。 |
| `HorizontalContentAlignment` | `HorizontalAlignment` | 垂直 Navigation item 列的水平对齐，默认 `Center`；普通 Steps 和水平 Navigation 不使用。 |
| `IsMotionEnabled` | `bool` | 是否启用 transition 和 Wave，默认来自 SharedToken。 |
| `ItemHeaderForeground` | `IBrush?` | 所有 item 主标题的实例级语义前景色覆盖；默认 `null`，保留当前类型和状态的 Token 视觉。 |
| `ItemSubHeaderForeground` | `IBrush?` | 所有 item 辅助标题的实例级语义前景色覆盖；默认 `null`，保留当前类型和状态的 Token 视觉。 |
| `ItemRailBackground` | `IBrush?` | 所有 item Connector 的实例级语义背景覆盖；默认 `null`，保留当前类型和状态的 Token 视觉。 |
| `Items` / `ItemsSource` / `ItemTemplate` | inherited | 步骤集合和数据模板入口。 |

`Current` 允许小于 `Initial` 或大于所有 item 的编号。Steps 不裁剪、不归一也不回写越界值。

三个 `Item*` 属性只覆盖实例的语义视觉，不参与 `EffectiveStatus`、`ConnectorStatus`、布局或交互计算。它们作用于 `Steps` 拥有的全部直接或生成容器；属性为 `null` 时，主题继续使用 Wait、Process、Finish、Error 以及 Inline 对应的 Token。

### 3.2 事件契约

```csharp
public event EventHandler<StepsCurrentChangeRequestedEventArgs>?
    CurrentChangeRequested;
```

事件仅由可交互 item 的 pointer 或 keyboard 激活路径触发，参数提供目标步骤编号。调用方决定是否更新 `Current`。当前 item 被重复激活时不发出请求。

### 3.3 Item 契约

`StepsItem` 继承 `HeaderedContentControl`。

| API | 类型 | 语义 |
| --- | --- | --- |
| `Header` / `HeaderTemplate` | inherited | 步骤标题。 |
| `SubHeader` / `SubHeaderTemplate` | `object?` / `IDataTemplate?` | 标题旁的辅助信息。 |
| `Content` / `ContentTemplate` | inherited | 步骤详情内容。 |
| `Icon` | `PathIcon?` | 自定义 Indicator 图标。 |
| `Status` | `StepsStatus?` | item 显式状态；`null` 使用根控件自动状态。 |
| `IsEnabled` | inherited | 是否允许该 item 参与交互。 |

`StepsItem` 不实现选择协议，不公开选择态、额外完成态或页面内容 API。

### 3.4 枚举契约

| 枚举 | 成员 | 语义 |
| --- | --- | --- |
| `StepsStatus` | `Wait`、`Process`、`Finish`、`Error` | 根当前状态、item 显式状态和有效状态。 |
| `StepsType` | `Default`、`Dot`、`OutlineDot`、`Navigation`、`Inline`、`Panel` | Steps 的完整视觉类型。Panel 隐藏 Indicator/Connector，使用面板背景、边框和外溢箭头。 |
| `StepsPanelVariant` | `Filled`、`Outlined` | Panel 的填充或描边变体。 |

`StepsType` 同时表达原 Style 和 Indicator 类型，禁止形成 `Navigation + Dot` 等没有明确 Steps 语义的组合。
`OutlineDot` 与 `Dot` 共享布局语义和 Dot 尺寸 Token，但 Indicator 使用透明背景和状态色边框，并且不播放 Indicator Wave。

### 3.5 主题契约

Part 的 Selector、ContractType、数量语义与定制边界以 [Steps Semantic Part 契约](semantic-part.md) 为唯一完整来源。

内部模板节点（不属于 Semantic Part）：

| 节点 | 类型 | 职责 |
| --- | --- | --- |
| `items` | `PART_ItemsPresenter` | 承载 StepsPanel 和 item 容器。 |
| `navigation-arrow` | `NavigationArrow` | Navigation 类型的步骤方向提示。 |
| `navigation-active-indicator` | `NavigationActiveIndicator` | Navigation 当前项的水平底线或垂直右侧线。 |

Steps 不提供控件专属的完成态或选择态伪类。状态主题读取 `EffectiveStatus`、`IsCurrent`、`CanInvoke` 以及 Avalonia 标准 `:pointerover`、`:focus-visible`、`:disabled` 伪类。

`itemWrapper`、`itemIcon`、`itemTitle`、`itemSubtitle`、`itemSection`、`itemContent` 和 `itemRail` 是 `StepsItem` 自身模板内的静态语义 marker，由 `StepsItemTheme.axaml` 声明；应用通过生成的子 Part Style 经 `> .semantic-item /template/ .semantic-item-x` 路由进入 item 模板，不得依赖节点名称以外的模板结构或手写 `/template/` selector。实例级 Header、SubHeader 和 Connector 覆盖由根控件三项 nullable 语义 API 投影，与 Semantic Part 定制互不取代。

## 4. 行为与状态模型

### 4.1 唯一状态流

```text
Current / Initial / Steps.Status / StepsItem.Status / item index
    -> StepNumber / AutomaticStatus
    -> IsCurrent / EffectiveStatus / ConnectorStatus
    -> StepsItem theme / StepsItemIndicator theme
```

模板和容器生命周期只消费状态，不建立或修正状态。

### 4.2 状态算法

对索引为 `index` 的 item：

```text
StepNumber = Initial + index

AutomaticStatus =
    StepNumber == Current ? Steps.Status :
    StepNumber < Current  ? Finish :
                            Wait

EffectiveStatus = StepsItem.Status ?? AutomaticStatus
IsCurrent       = StepNumber == Current
```

`EffectiveStatus` 是 Indicator、Connector、Progress、文字和颜色唯一允许读取的状态。`IsCurrent` 与 `EffectiveStatus` 独立，因此当前 item 可以显式为 Wait、Finish 或 Error。

### 4.3 越界语义

- `Current < Initial`：没有当前 item，未覆盖 item 自动为 Wait。
- `Current >= Initial + ItemCount`：没有当前 item，未覆盖 item 自动为 Finish。
- Items 为空：保留 `Current`，不产生 item 状态。
- Items 动态变化：使用保留的输入重新计算，不执行选择恢复或 Current 归一。

### 4.4 Connector 语义

连接 item `i` 和 `i + 1` 的 Connector 使用下一个 item 的 `EffectiveStatus`，让指向当前错误或当前进行中步骤的线段跟随目标步骤状态：

```text
Connector[i].Status = Item[i + 1].EffectiveStatus
```

最后一个 item 不显示 Connector。

### 4.5 Pointer、键盘与 Wave

可交互条件：

```text
Steps.IsItemClickable
&& Steps.IsEnabled
&& StepsItem.IsEnabled
```

- Pointer 只有在同一 item 内完成 press/release 才视为 click。
- 点击非当前 item：播放目标 Indicator Wave，并发出 `CurrentChangeRequested`。
- 点击当前 item：播放 Wave，不发出请求。
- `Type=OutlineDot` 点击仍按可交互规则发出请求，但不播放 Indicator Wave。
- Enter/Space：非当前 item 发出请求，不播放 Wave。
- 程序化修改 `Current`、Items 变化、模板重套和状态重算都不播放 Wave。
- 不可交互 item 不显示 hand cursor、hover 激活视觉，也不进入 Tab 焦点序列。

## 5. 视觉与主题模型

Steps 使用统一语义模板，而不是按 Type、Orientation 和 TitlePlacement 复制 ControlTemplate。

| 主题文件 | 职责 |
| --- | --- |
| `StepsTheme.axaml` | 根模板、ItemsPresenter、StepsPanel 和根展示输入映射。 |
| `StepsItemTheme.axaml` | 统一 item 语义模板、Panel item frame、状态颜色、Connector、内容和交互视觉。 |
| `StepsItemIndicatorTheme.axaml` | 统一 Indicator 模板、Dot、Icon、状态图标、Progress 和 Wave。 |

运行时组合：

```text
Steps
└── ItemsPresenter#PART_ItemsPresenter
    └── StepsPanel
        └── StepsItem
            └── StepsItemLayoutPanel
                ├── StepsPanelItemFrame#ItemWrapper
                ├── StepsItemIndicator#PART_Indicator
                │   └── WaveSpiritDecorator#PART_WaveSpirit
                ├── StepsItemSectionPanel#Section
                │   ├── ContentPresenter#HeaderPresenter
                │   ├── ContentPresenter#SubHeaderPresenter
                │   └── ContentPresenter#ContentPresenter
                ├── PixelAlignedBorder#Connector
                ├── PathIcon#NavigationArrow
                ├── StepsPanelArrow#PanelArrow
                └── PixelAlignedBorder#NavigationActiveIndicator
```

`StepsPanel` 负责 item 间的 flex/stack 布局；Panel 类型强制水平排列并将每个 item 等宽。`StepsItemLayoutPanel` 负责 item 内固定语义区域、Connector 线宽、Panel 外溢箭头和 Navigation active 线的排列，正文区域（标题、副标题、详情）的分组与对齐由 `StepsItemSectionPanel` 完成。这些面板不创建视觉、不计算状态。item 间的弹性压缩与文本换行契约见 [8.7 弹性压缩与文本换行模型](#87-弹性压缩与文本换行模型)。

Panel 类型的几何规则：

- Indicator 和普通 Connector 不参与可见布局。
- ItemWrapper 覆盖完整 item 单元，PanelArrow 在非末项的外侧拉伸为楔形箭头。
- LTR 箭头向右外溢，RTL 箭头向左外溢；末项不创建可见箭头。
- `Filled` 使用状态背景作为面板表面，并在非首项裁出左侧 notch；`Outlined` 保留共享接缝的箭头边框，非当前 Error 项保持容器背景并使用红色文字和边框，当前 Error 项才使用浅红 active 背景。
`OutlineDot` 复用 `Dot` 的布局路径，只改变 Indicator 的填充、边框和 Wave 语义。

### 5.1 Item 语义样式覆盖

`ItemHeaderForeground`、`ItemSubHeaderForeground` 和 `ItemRailBackground` 是 `Steps` 实例级的语义槽，不是对模板内部节点的公开暴露。`Steps` 将显式值投影到每个 `StepsItem`，再由 `StepsItemTheme.axaml` 在自己的模板边界内分别应用到 Header、SubHeader 和 Connector。

三项 API 分别对应步骤条标题、子标题和 rail 的语义能力，但保持 Avalonia 的强类型 `IBrush?` 契约，不公开任一模板节点。

优先级固定为：

```text
非 null 的 Steps 实例语义样式
    > 当前 Type / EffectiveStatus 对应的 StepsToken
null
    -> 完整回退到当前 Type / EffectiveStatus 对应的 StepsToken
```

运行时修改或清空任一属性必须立即更新所有已实现容器；直接声明的 `StepsItem`、由普通数据项生成的容器以及回收后重新准备的容器遵循同一投影规则。该覆盖不改变 Indicator、Content、Navigation active indicator 或其他未命名语义区域。

有效标题布局：

```text
Orientation == Vertical -> Horizontal
Type == Dot             -> Vertical
Type == OutlineDot      -> Vertical
Type == Inline          -> Vertical
Type == Navigation      -> Horizontal
Type == Panel           -> Horizontal
其他                    -> TitlePlacement
```

## 6. 控件家族或集成关系

Steps 位于 Desktop Navigation 分类，与 Breadcrumb、Pagination、TabControl 和 NavMenu 同属导航体系，但不共享 Selection、路由或页面内容模型。

- `ItemsControl` 提供 Items、ItemsSource、ItemTemplate 和容器生成基础。
- `StepsItem` 是公开 item 容器。
- `StepsItemIndicator`、`StepsPanel`、`StepsItemLayoutPanel`、`StepsItemSectionPanel` 和 `StepsPanelItemFrame` 是 internal-observable 协作控件。
- `StepsToken` 为 Steps、StepsItem 和 Indicator 提供组件级视觉资源。
- Gallery 提供状态、布局、可点击和 Wave 的可运行示例。

Steps 不实现 Form、CompactSpace、Popup、路由或页面内容接口。

## 7. 兼容性不变量

完成本次不兼容重构后，以下契约构成新的稳定边界：

- `Steps` 继承 `ItemsControl`，不得重新引入 Selection 作为第二状态源。
- `Current` 是唯一当前步骤输入，交互路径只发出请求。
- `Initial` 只表示编号偏移，不能在模板生命周期中写入 `Current`。
- `StepsItem.Status` 保持 nullable，并优先于自动状态。
- 主题只能读取 `EffectiveStatus`，不得恢复多套并行的状态输入。
- `Content` 只表示步骤详情，Steps 不保存或投影当前页面内容。
- `Type` 是视觉类型唯一入口，不得恢复独立 Style/IndicatorType 组合。
- `Percent=null` 是 Progress 的唯一关闭语义。
- Wave 只能由真实 pointer click 触发，不得监听 `Current` 或 `IsCurrent`。
- `OutlineDot` 必须保持 Dot 布局、空心状态色边框和无 Wave 语义。
- `PART_ItemsPresenter` 和 `PART_Indicator` 是稳定 template part。
- 外部样式不得依赖 `HeaderPresenter`、`SubHeaderPresenter`、`Connector` 等内部节点名称，也不得手写 `/template/` selector 穿透 `StepsItem`；item 子节点的实例级定制必须通过生成的 Semantic Part Style 表达，Header、SubHeader 和 Connector 的实例级覆盖由 `Steps` 三项 nullable 语义 API 表达。
- 三项 item 语义样式保持 nullable；`null` 必须恢复完整的状态和类型 Token 视觉。
- 每个根、item 和 indicator 主题各保留一套语义模板。
- 水平布局的 item 收缩与文本换行遵循 8.7 弹性模型的份额算法与 `IconContainerSize` 收缩下限。
- 测量与排列共用份额算法，但自然尺寸请求与最终分配宽度分开：测量不申请宿主的富余空间，排列按实际分配宽度伸展；受限文本必须获得足够的换行高度。

## 8. 专项模型

### 8.1 受控 Current 模型

`Current` 是外部受控输入。`CurrentChangeRequested` 是用户导航意图，不表示 Current 已经变化。事件处理器可以同步更新、异步验证后更新或拒绝更新。

### 8.2 Initial 编号模型

`Initial` 参与 `StepNumber = Initial + index`。Indicator 显示 `StepNumber + 1`。它不表示初始选中 item，也不参与模板应用。

### 8.3 Progress 模型

Progress 有效条件：

```text
Percent.HasValue
&& IsCurrent
&& EffectiveStatus == Process
&& Icon == null
&& Type is Default or Navigation
```

`Percent` 被约束到 `0..100`；NaN 和 Infinity 归一为 `null`。

### 8.4 ItemsSource 模型

- 直接 `StepsItem` 使用 `Header`、`SubHeader` 和 `Content`。
- 普通数据项生成 `StepsItem`，数据项进入 `Content`，`ItemTemplate` 负责完整文字区域。
- 容器只保存派生展示状态，不保存业务流程状态。

### 8.5 Inline Offset 模型

`Offset` 对齐 inline steps 的 offset cell 语义，只在 `Type=Inline` 时参与布局。它在可见 item 前方保留同等宽度的空 item 单元，使部分步骤可以和完整步骤条的后续列对齐。

`Offset` 不参与 `StepNumber`、`Current`、`Initial` 或状态计算。声明 `Offset=2` 且只提供 Step 3-5 三个 item 时，`Current=1` 仍表示当前声明集合中的第二个 item。

### 8.6 垂直 Navigation 对齐

`Type=Navigation` 且 `Orientation=Vertical` 时，`HorizontalContentAlignment` 控制整列 item 在可用宽度内的水平对齐。默认 `Center` 对齐导航模式的居中语义；需要贴边或填满容器时可设置为 `Left`、`Right` 或 `Stretch`。

该属性不改变普通垂直 Steps 的左侧流程阅读布局，也不改变水平 Navigation 的等宽布局。

### 8.7 弹性压缩与文本换行模型

水平 Steps 的 item 间布局采用与上游一致的 flex 份额语义：item 宽度与 item 间 rail 长度随可用宽度连续收缩，标题（`HeaderPresenter`）、副标题/时间（`SubHeaderPresenter`）与描述（`ContentPresenter`）文本在受限宽度下换行。压缩与换行是同一测量过程的连续结果，不存在离散阈值。

份额模型由 `StepsPanel` 统一计算。输入为可用宽度、每个 item 的期望尺寸、`Type`、`Orientation` 与有效 `TitlePlacement`，输出为每个 item 的测量与排列宽度：

| 布局模式 | item 份额 | 收缩下限 |
| --- | --- | --- |
| 水平 + 水平标题（`Default` 等） | 非末项 `1 1 auto`，末项 `0 1 auto` | `IconContainerSize` |
| 水平 + 垂直标题（`Dot`、`OutlineDot`、`Inline`、`TitlePlacement=Vertical`） | 全部 item `1 1 0%` 等分 | `IconContainerSize` |
| 水平 `Navigation` | 等分 | `IconContainerSize` |
| 垂直 `Orientation` | 不参与横向份额 | 不适用 |

自然尺寸与伸展契约：

- 未指定宽度时，Steps 向父布局请求能够容纳内容的自然宽度，不因父容器可用宽度增加而扩大尺寸请求。`HorizontalAlignment=Stretch` 在排列阶段消费父容器实际分配的宽度。
- 水平标题的普通布局为非末项预留等宽单元，并让末项保持内容宽度；等宽模式按最宽 item 计算自然单元宽度，Inline 的前导 `Offset` 单元也计入请求。
- 自然宽度包含可见连接线的可绘制线段及其两端间距，不能只容纳图标和标题后让连接线退化为零宽。默认线段长度由既有 SharedToken 间距提供；它是自然尺寸偏好，不是阻止压缩的最小宽度。
- 自然宽度受父容器可用宽度约束；宽度不足时仍执行压缩与换行。自动尺寸弹窗可以依据 Steps 的内容需求确定宽度，固定宽度布局仍可让 Steps 伸展。

收缩与换行契约：

- 空间富余时保持既有伸展语义：非末项伸展、末项保持内容宽度；空间不足时全部 item 按内容占比收缩，末项同样参与收缩。
- 每个 item 的收缩下限是图标容器宽度 `IconContainerSize`；达到下限后 item 不再收缩，文本在剩余宽度内继续换行。
- `StepsPanel` 以自然宽度和可用宽度共同确定的份额测量 item；排列时按最终宽度计算份额。宽度受限时保留换行后的行高，空间富余时允许扩大排列单元。item 内 `StepsItemLayoutPanel` 按扣除图标与间距后的约束测量正文，`StepsItemSectionPanel` 完成标题、副标题与详情的分组测量与排列，任何路径不得以裁剪代替换行。
- 字符串内容经 `StepsStringToTextBlockConverter` 转换为 `TextWrapping="Wrap"` 的 TextBlock，标题、副标题与描述三者均参与换行；换行后 item 高度为标题行（必要时含副标题独立行）与内容行高度之和，行高取全部 item 的最大值。
- 标题行空间不足时标题优先保留，副标题换到标题下方独占一行并保持完整文本宽度；任何布局路径不得以裁剪代替换行。

与上游的已知差异：

- 上游对文本应用 `word-break: break-word`，超长连续字符串也会断开换行；Avalonia `TextBlock` 只支持按词换行，超长连续字符串在极窄宽度下可能溢出或裁剪，必要时通过自定义 `LineBreakingRules` 对齐。
- 上游 v6 已废弃描述区默认最大宽度（默认不设上限）；AtomUI 由 `DescriptionMaxWidth` Token（默认 140）表达描述区最大宽度，Token 保留供显式设置。该上限独立于压缩与换行机制。

## 9. 文档导航与验证策略

关联文档：

- [Steps Semantic Part 契约](semantic-part.md)
- [Steps 桌面版实现原理](implementation.md)
- [Steps Token 设计](token.md)
- [Steps Changelog](changelog.md)

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| Public API | Current、Initial、Status、Percent、Type、Orientation、TitlePlacement、SizeType、IsItemClickable、Offset、HorizontalContentAlignment、IsMotionEnabled、ItemHeaderForeground、ItemSubHeaderForeground、ItemRailBackground、CurrentChangeRequested。 |
| Item API | Header、SubHeader、Content、Icon、nullable Status、IsEnabled。 |
| 状态 | 纯状态算法、显式覆盖、越界、动态 Items、Connector nextStatus。 |
| 交互 | Pointer、Enter/Space、disabled、当前 item 重复激活、受控请求、Wave。 |
| AXAML | 统一模板、两个布局 Panel、稳定 part、运行时布局切换、语义样式只由 StepsItemTheme 在自身模板内消费。 |
| 布局 | 弹性压缩、文本换行、`IconContainerSize` 收缩下限、宽容器伸展回归、自然尺寸与实际分配宽度的分离。 |
| Token | Indicator、Dot、状态色、Connector、Navigation、Inline、Progress。 |
| 生命周期 | 容器 owner 释放、语义样式投影建立与释放、直接/生成/回收容器、模板重套、detach/reattach、Wave part 释放。 |
| Semantic Part | descriptor、路由、marker 同步、生成的 Style 应用与 Gallery 预览。 |
| 文档 | `git diff --check`、相对链接。 |
