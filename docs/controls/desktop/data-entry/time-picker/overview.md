# TimePicker 桌面版架构设计

本文档定义 `TimePicker` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [TimePicker 桌面版实现原理](implementation.md)，TimePicker Token 的专项设计见 [TimePicker Token 设计](token.md)，设计和契约变化记录见 [TimePicker Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `InfoPickerInput`，其 `IsPopupPinnedOpen` 供测试、内部诊断和 Semantic Parts 预览钉住弹层使用；设置为 true 时保持 picker open state 并 relay 到 `PickerPopup`，设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

TimePicker 家族包含 `TimePicker` 与 `RangeTimePicker` 两个 Semantic owner，分别公开 11 个和 12 个 Semantic Part（触发区 `root` / `prefix` / `input` / `suffix` / `clear`，Range 额外有 `secondaryInput`；弹层 `popup.root` / `popup.container` / `popup.content` / `popup.column` / `popup.item` / `popup.footer`），与 Ant Design TimePicker / TimePicker.RangePicker 共用的 Semantic Part 语义对齐（TimeView 头部与列内滚动机制等内部实现不经 TimePicker 发布）；完整 Part 表、单值/范围存在条件、Selector 用法与定制边界见 [TimePicker Semantic Part 契约](semantic-part.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/TimePicker` |
| 控件状态 | Stable |

TimePicker 是 AtomUI 桌面控件体系中的时间选择控件，用于在输入壳体和时间面板之间选择时分秒。

TimePicker 不负责日期选择、时区转换或业务排班模型。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/TimePicker`

## 2. 设计语言

TimePicker 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | TimePicker 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | TimePicker 是 AtomUI 桌面控件体系中的时间选择控件，用于在输入壳体和时间面板之间选择时分秒。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `IsShowHeader`、`ItemFormat`、`ItemHeight`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | selection/checked/active。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | TimePicker Token + ControlTheme。 |

## 3. API 与契约模型

TimePicker 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `IsShowHeader`、`ItemFormat`、`ItemHeight` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 选择与集合 | `RangeEndSelectedTime`、`RangeStartSelectedTime`、`SelectedTime`、`SelectorRowCount` | 维护选择、展开、过滤、分页、分组或集合状态。 |
| 交互与状态 | `IsNeedConfirm`、`IsShowNow`、`IsChangeOnScroll` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 其他稳定入口 | `ClockIdentifier`、`DefaultTime`、`MinuteIncrement`、`PanelType`、`PickerDisplayTime`、`RangeEndDefaultTime`、`RangeStartDefaultTime`、`SecondIncrement` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

当前没有抽取到控件专属 public 事件；交互通知主要来自继承事件、命令或 Gallery 可观察状态。

`SelectedTime` 是 `TimePicker` 的受控 Form 值属性，默认绑定模式为 `TwoWay`，并启用 Avalonia data validation。Form、绑定验证和输入表面错误视觉必须基于 `DataValidationErrors` 投射，不允许另建与 native validation 并行的错误状态。TimePicker/RangeTimePicker 还必须把 `FormStatus` 和显式 `Status` 投射到 shared `InputControlFrame`；TimeView 只拥有时间面板和选择交互状态。

主要公开类型与枚举：

- 类型：`CellDbClickedEventArgs`、`CellHoverEventArgs`、`DateTimePickerPanel`、`RangeTimePicker`、`TimePicker`、`TimePickerPresenter`、`TimeSelectedEventArgs`、`TimeView`、`TimeViewCell`。
- 枚举：`ClockIdentifierType`。
- 本地化 Catalog：`TimePickerLangResourceKind`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_ButtonsFrame` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_ButtonsLayout` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_ConfirmButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_ContentPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_FirstSpacer` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_HeaderText` | `?` | 承载文本输入、过滤、显示或编辑入口。 |
| `PART_HourHost` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_HourSelector` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_MainFrame` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_MainLayout` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_MinuteHost` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_MinuteSelector` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_NowButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_PeriodHost` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_PeriodSelector` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_PickerContainer` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_RootLayout` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_SecondHost` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_SecondSelector` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_SecondSpacer` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_ThirdSpacer` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_TimeView` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

`TimePicker.IsNeedConfirm` 与 `RangeTimePicker.IsNeedConfirm` 默认 `true`，沿用该属性名。`ClockIdentifier` 默认 `HourClock24`，显式 `HourClock12` 保留 12 小时制。`IsChangeOnScroll` 默认 `false`，用于独立控制滚动是否产生候选选择。

宿主的 `SelectedTime` / 范围端点是提交值，Presenter 的候选值驱动输入文本。选择每一列后立即显示完整候选时间，弹层保持打开；悬停只临时预览，离开选项恢复候选文本。`IsNeedConfirm=false` 时关闭或失焦提交最后的有效候选；为 `true` 时普通关闭丢弃未确认候选。OK、Enter 和 Now 使用显式提交流程，Now 在有效步进范围内选取当前时间并提交。已有值与 `PickerDisplayTime` 显示锚点可以通过 OK 显式确认；打开显示锚点本身不提交。

## 4. 行为与状态模型

TimePicker 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- selection/checked/active 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- `SelectedTime` 是单时间选择的唯一用户值 owner；外部绑定、Form set/get、清除和弹层提交都必须收敛到该属性。
- `PickerDisplayTime` 只定义弹出面板打开时的显示锚点；它不得写入 `SelectedTime`，也不得改变 `DefaultTime` 的 reset 语义。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

| 操作 | `IsNeedConfirm=true` | `IsNeedConfirm=false` |
| --- | --- | --- |
| 面板点选 | 更新候选和输入文本，继续编辑。 | 更新候选和输入文本，继续编辑。 |
| 悬停结束 | 恢复当前候选。 | 恢复当前候选。 |
| 普通关闭或焦点离开组合控件 | 丢弃未确认候选，恢复提交值。 | 提交有效候选，通知绑定/Form。 |
| OK / Enter | 校验并提交，然后关闭。 | Enter 校验并提交，OK 隐藏。 |
| Now | 直接提交有效当前时间并关闭。 | 相同行为。 |
| 普通滚动 | 浏览时间列表，不改变候选。 | 相同行为。 |

时、分、秒列循环展示允许值，选中项始终位于视口垂直中心；`00` 上方显示上一周期的 `23` / `59`，首尾不补空白。步长列表按有效值循环，例如分钟步长 15 时 `00` 上方为 `45`。AM/PM 保持两项非循环列表。各列独立滚动；时、分、秒循环列隐藏纵向滚动条，仍支持滚轮、触控板和键盘操作；普通滚动只浏览，不产生候选。时间列的焦点滚动请求止于 TimeView，点击列值不会推动宿主页面滚动。单独的 TimePicker 面板展示 8 行；共享 TimeView 的 DatePicker 宿主保留其显式/默认行数与提交策略。

## 5. 视觉与主题模型

TimePicker 的视觉模型由 `InputControlFrame` 输入表面、InfoPicker 输入子控件、控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。输入表面状态由 shared frame 统一表达，TimePicker 主题只扩展时间面板、范围和弹层内容。

| 主题文件 | 职责 |
| --- | --- |
| `RangeTimePickerTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `InfoPickerTextBoxTheme.axaml` | 通过 `StyleVariant=Borderless` 提供内部时间文本输入的无 chrome 布局。 |
| `InputControlFrameTheme.axaml` | 提供输入表面 variant、effective status、focus、disabled、error、warning、CompactSpace 和 motion。 |
| `TimePickerPresenterTheme.axaml` | 定义布局、内容承载或框架节点视觉。 |
| `TimePickerTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `TimeViewCellTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `TimeViewTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

TimePicker 使用 `TimePickerToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 selection/checked/active 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

TimePicker 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `DateTimePickerPanel`：布局面板，负责测量、排列、虚拟化或集合内容布局。
- `RangeTimePicker`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TimePicker`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TimePickerPresenter`：模板协作类型，承载内容展示、宿主或视觉边界。
- `InfoPickerTextBox`：internal `AbstractTextInput` 输入子控件，负责时间文本编辑、Form/native validation 接入和 Borderless 输入布局。
- `InputControlFrame`：internal 输入表面组合控件，负责 TimePicker/RangeTimePicker 的 variant、effective status、边框、背景和 CompactSpace 视觉。
- `TimePickerToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `TimeView`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TimeViewCell`：集合项、节点或容器类型，承载单项状态和模板协作。
- `TimePickerLangResourceKind`：稳定的本地化 Catalog enum；内置翻译由同目录三种语言 XLIFF 提供并在编译期生成。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 TimePicker 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 选择与当前项模型

TimePicker 的当前项状态必须由单一 owner 推导。public 选择属性、集合项容器和伪类之间只能做单向同步，集合替换、清空和模板重套用时必须回放当前状态。

## 9. 文档导航与验证策略

关联文档：

- [TimePicker 桌面版实现原理](implementation.md)
- [TimePicker Semantic Part 契约](semantic-part.md)
- [TimePicker Token 设计](token.md)
- [TimePicker Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 selection/checked/active、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
