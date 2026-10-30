# Timeline 桌面版架构设计

本文档定义 `Timeline` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，方向与布局的完整策略见 [Timeline 方向与布局设计](orientation-layout-design.md)，Semantic Part 契约见 [Timeline Semantic Part 契约](semantic-part.md)，内部实现原理见 [Timeline 桌面版实现原理](implementation.md)，Timeline Token 的专项设计见 [Timeline Token 设计](token.md)，设计和契约变化记录见 [Timeline Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Timeline` |
| 控件状态 | Stable |

Timeline 是 AtomUI 桌面控件体系中的时间轴控件，用于沿垂直或水平方向按顺序展示事件节点、状态和时间信息。控件使用同一套 Item、Indicator、Panel 和主题结构表达两种方向，不创建方向专用的平行控件家族。

Timeline 不负责日历、列表排序或流程引擎。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Controls/Timeline`
- `src/AtomUI.Desktop.Controls/Timeline`

## 2. 设计语言

Timeline 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Timeline 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Timeline 是 AtomUI 桌面控件体系中的时间轴控件，用于按顺序展示事件节点、状态和时间信息。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Content`、`Label`、`IndicatorIcon`、`Pending`、`PendingIcon`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | `IsReverse`、Pending 状态、可见项顺序和首尾节点状态。 |
| 布局语义 | 主轴方向和内容相对轴线的位置如何组合。 | `Orientation` 决定主轴，`Mode` 决定交叉轴上的 `Start`、`End` 或交替布局。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Timeline Token、方向 selector、Item 模板和 Indicator renderer。 |

## 3. API 与契约模型

Timeline 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Content`、`Label`、`IndicatorIcon`、`Pending`、`PendingIcon` | 定义单项内容、时间标签、节点图标和待处理节点入口。 |
| 集合顺序 | `Items`、`ItemsSource`、`IsReverse` | 维护源顺序、最终视觉顺序和 Pending 项的相邻关系。 |
| 方向与模式 | `Orientation`、`Mode` | 决定主轴方向以及内容位于轴线的 Start、End 或交替侧。 |
| 内部派生状态 | `IsLabelLayout`、`IsOdd`、`IsFirst`、`IsLast`、`NextIsPending` | 由 Timeline 根据可见项视觉顺序单向投影到 Item 和模板。 |
| 视觉与布局 | `IndicatorColor`、`IndicatorIcon`、`IndicatorTailColor`、`IndicatorTailWidth` | 影响节点颜色、形状和轴线渲染；连接线颜色与宽度由 Indicator 属性与 Token 定制。 |
| Semantic Part | `root`、`item`、`itemWrapper`、`itemIcon`、`itemTitle`、`itemContent` | 单一 owner `Timeline` 公开的语义区域，见 [Timeline Semantic Part 契约](semantic-part.md)。 |
| 其他稳定入口 | `Label`、`Pending` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

当前没有抽取到控件专属 public 事件；交互通知主要来自继承事件、命令或 Gallery 可观察状态。

主要公开类型与枚举：

- 类型：`AbstractTimeline`、`AbstractTimelineItem`、`Timeline`、`TimelineItem`。
- 枚举：`TimelineMode`。

`TimelineIndicator`、`TimelineItemPanel` 和 `TimelineStackPanel` 是影响可观察布局与渲染的 internal 协作类型，不属于用户可直接依赖的 public surface。

方向与模式的公共契约为：

```csharp
public enum TimelineMode
{
    Start,
    End,
    Alternate
}
```

- `Orientation` 使用 `Avalonia.Layout.Orientation`，默认值为 `Vertical`。
- `Mode` 默认值为 `TimelineMode.Start`。
- `Start` 和 `End` 是相对 Timeline 轴线的逻辑位置，不是固定的物理 Left/Right。
- `Alternate` 从最终视觉顺序中的第一个可见项开始按 `Start`、`End` 交替。
- Timeline 不提供单个 `TimelineItem` 的 placement 覆盖属性；位置策略由 Timeline 统一拥有。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_IconPresenter` | `IconPresenter` | 展示用户内容、文本、图标或模板化数据。 |

控件专属或内部伪类包括 `OrderEvenPC`、`OrderFirstPC`、`OrderLastPC`、`OrderOddPC`、`PendingItemPC`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 4. 行为与状态模型

Timeline 的状态流按以下路径收敛：

```text
Orientation / Mode / IsReverse / Items / item visibility
  -> Timeline 计算可见项视觉顺序
  -> item effective mode / order / first / last / pending adjacency
  -> internal property / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

方向与模式的稳定语义：

| Orientation | Mode | 无 Label | 存在 Label |
| --- | --- | --- | --- |
| `Vertical` | `Start` | 轴线位于逻辑起始侧，Content 位于结束侧。 | Label 位于起始侧，Content 位于结束侧。 |
| `Vertical` | `End` | Content 位于逻辑起始侧，轴线位于结束侧。 | Content 位于起始侧，Label 位于结束侧。 |
| `Horizontal` | `Start` | 轴线在上，Content 在下。 | 轴线在上，Label 与 Content 依次在下并对轴线居中。 |
| `Horizontal` | `End` | Content 在上，轴线在下。 | Label 与 Content 依次在上，轴线在下。 |
| 任意方向 | `Alternate` | 第一可见项为 Start，后续按 End、Start 交替。 | 使用同一交替规则，并保持所有节点共用同一轴线。 |

`FlowDirection` 只影响垂直 Timeline 的逻辑起始侧和结束侧；水平 Timeline 的 Start/End 分别映射到下方和上方。`IsReverse` 只反转主轴视觉顺序，不交换 Start/End。隐藏项不占用布局槽位，也不参与交替奇偶、首尾和 Pending 相邻关系计算。

## 5. 视觉与主题模型

Timeline 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `TimelineIndicatorTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `TimelineItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `TimelineTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

Timeline 使用 `TimelineToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载方向、Mode、视觉索引或 Pending 相邻状态。水平布局的内容间距优先使用 SharedToken；方向差异由 ControlTheme selector 和布局 Panel 表达。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Timeline 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `AbstractTimeline`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractTimelineItem`：承载单项内容 API，并接收 Timeline 单向投影的方向、Mode 和视觉顺序状态。
- `Timeline`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TimelineIndicator`：内部轴线与节点 renderer，根据 Orientation、首尾状态和自定义图标边界绘制连接线。
- `TimelineItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `TimelineItemPanel`：负责 Label、Indicator 和 Content 在垂直或水平模式中的测量与排列。
- `TimelineStackPanel`：负责主轴方向、可见项顺序、Reverse 排列和水平等宽槽位。
- `TimelineToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- ItemsSource、显式 TimelineItem 和自动生成容器必须进入同一视觉顺序归一流程。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Timeline 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- `TimelineMode` 的稳定值为 `Start`、`End`、`Alternate`；不得重新引入 `Left`、`Right` 或重复值兼容别名。
- `Orientation` 默认值保持 `Vertical`，`Mode` 默认值保持 `Start`。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Start/End、Alternate 首项、RTL、Reverse、隐藏项和 Pending 的既定组合语义。
- 不为单个 TimelineItem 增加与 Timeline 全局 Mode 竞争的位置 owner。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 方向与布局模型

`Orientation` 与 `Mode` 是两个正交维度：Orientation 选择时间轴主轴，Mode 选择内容在交叉轴上的分布策略。完整模式矩阵、等宽规则和轴线算法见 [Timeline 方向与布局设计](orientation-layout-design.md)。

### 8.2 视觉顺序模型

Timeline 是视觉顺序的唯一 owner。控件先过滤不可见项，再应用 `IsReverse`，最后按视觉索引计算 Alternate 奇偶、首尾和 Pending 相邻关系。Panel 和 Indicator 只消费这些派生状态，不重新解释源集合索引。

### 8.3 视觉选项模型

Timeline 的视觉选项通过 public API 归一为 internal state、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态、方向、视觉索引或业务色值。

### 8.4 Semantic Part 模型

Timeline 以单一 owner 公开全部九个 Semantic Part：`root`、`item`、`itemWrapper`、`itemIcon`、
`itemSection`、`itemHeader`、`itemTitle`、`itemContent`、`itemRail`，对齐上游 `TimelineSemanticType`
（Steps 语义去掉 `itemSubtitle`）。为支持上游 `itemSection` / `itemHeader` 结构包裹节点与 `itemRail`
连接线，控件自身的视觉结构补齐为真实节点：新增 `TimelineSectionPanel`（承载原 Label/Indicator/Content
方向化布局）与 header 包裹节点，`TimelineIndicator` 从自绘 renderer 改为 rail/圆点/图标元素组合器，几何
语义逐项保留。完整契约见 [Timeline Semantic Part 契约](semantic-part.md)。

## 9. 文档导航与验证策略

关联文档：

- [Timeline Semantic Part 契约](semantic-part.md)
- [Timeline 桌面版实现原理](implementation.md)
- [Timeline 方向与布局设计](orientation-layout-design.md)
- [Timeline Token 设计](token.md)
- [Timeline Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖可见项视觉顺序、Alternate 奇偶、Reverse、RTL、首尾和 Pending 相邻状态。 |
| 布局算法 | 覆盖垂直/水平、Start/End/Alternate、Label、等宽、换行、零项和无限宽退化。 |
| AXAML/Theme | 检查 Orientation 传递、方向 selector、template part、伪类、资源 key、Light/Dark 和 Browser 主题。 |
| Semantic Part | `Timeline` descriptor 只含 `root`/`item`/`itemWrapper`/`itemIcon`/`itemTitle`/`itemContent`；item 模板四节点 marker 静态常驻、Indicator 模板图标 marker 常驻；item marker 随容器创建/prepare/Pending 路径幂等就位，Mode/Orientation/Reverse/Label 布局切换不增删 marker；owner-scoped Semantic Style 命中最低 public 类型。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
