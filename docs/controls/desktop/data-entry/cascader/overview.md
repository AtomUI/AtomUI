# Cascader 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.Cascader` 的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，候选列表统一交互见 [候选列表统一交互设计](../select/candidate-interaction-design.md)，内部实现原理见 [Cascader 桌面版实现原理](implementation.md)，Cascader Token 的专项设计见 [Cascader Token 设计](token.md)，设计和契约变化记录见 [Cascader Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。`AbstractSelect` 提供 public `IsPopupPinnedOpen`：设置为 true 时保持 `IsDropDownOpen` 并 relay 到 `PART_Popup`；Cascader 在弹层打开前依据钉住状态抑制 light-dismiss 遮罩，取消钉住后恢复模板默认值，使 Gallery 语义预览等场景可以持续展示 `popup.*` 部件。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

Cascader 公开 13 个 Semantic Part（`root`、`prefix`、`content`、`placeholder`、`input`、`suffix`、`clear`、`item`、`itemContent`、`itemRemove`、`popup.root`、`popup.list`、`popup.listItem`），与上游 Cascader 的 Semantic Part 语义对齐；完整 Part 表、Selector 用法与定制边界见 [Cascader Semantic Part 契约](semantic-part.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/Cascader` |
| 控件状态 | Stable |

Cascader 是 AtomUI 桌面数据录入体系中的级联选择控件，用于在有明确父子层级的数据中按列逐级展开并选择一个或多个选项。它面向地区、组织、分类、资源路径、权限域等“路径就是业务含义”的选择场景。

Cascader 由外层输入控件和内部级联弹层组成。外层 `Cascader` 负责输入表面、popup、选择结果、清除、Form、CompactSpace 和 AddOn 集成；内部 `CascaderView` 负责级联列、展开收起、过滤结果、异步加载和多选勾选。

Cascader 不负责树节点编辑、拖拽排序、远程协议封装、业务权限过滤、复杂图形化层级展示或任意树操作。这些能力应由业务层、TreeView、TreeSelect 或专用数据控件承担。

## 2. 设计语言

Cascader 的设计语言来自输入框、路径选择和多列层级弹层的组合。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 层级数据 | 选项按 parent / children 组成路径。 | `OptionsSource`、`Options`、`ICascaderOption.Children`。 |
| 逐列展开 | 每一级子选项以独立列展示，同级只保持一个展开分支。 | `CascaderViewLevelList`、`ExpandTrigger`。 |
| 选择模式 | 支持单选和多选勾选。 | `IsMultiple`、`SelectedOption`、`SelectedOptions`。 |
| 勾选展示 | 多选结果可展示全部、父级或叶子节点。 | `ShowCheckedStrategy`、`EffectiveSelectedOptions`。 |
| 父级可选 | 非叶子节点可作为选择结果。 | `IsAllowSelectParent`。 |
| 异步加载 | 展开未加载节点时按节点加载子项。 | `DataLoader`、`LoadingIcon`、内部 `CascaderView.AsyncLoadTimeout`。 |
| 搜索过滤 | 输入过滤值后展示完整路径结果。 | `Filter`、`FilterValue`、`FilterHighlightStrategy`。 |
| 输入表面 | 继承输入家族的尺寸、variant、status、Addon、清除和 Form feedback。 | `SizeType`、`StyleVariant`、`Status`、Addon、`SelectHandle`。 |

`SizeType=Custom` 不是 Cascader 的第四套专属 Token。主题层把 `Custom` 归入 `Middle` 默认字体和 padding 分支；用户显式设置高度等尺寸属性时由 Avalonia 属性优先级决定最终布局。

## 3. API 与契约模型

Cascader 通过 `OptionsSource` 和 `Options` 接收 `ICascaderOption` 数据。`CascaderOption` 是轻量默认选项模型，适合代码直接构造和普通层级数据。`BindableCascaderOption` 是绑定型选项模型，继承 `AvaloniaObject` 并暴露 DirectProperty，用于选项属性需要承载 XAML binding、`DynamicResource` 或 owner scoped resource 的场景。

### 3.1 数据模型契约

| 类型 | 语义 |
| --- | --- |
| `ICascaderOption` | Cascader 数据节点契约，包含 header、icon、enabled、checked、checkbox enabled、expanded、leaf、value、children 和 parent。 |
| `CascaderOption` | 默认轻量选项模型，不继承 `AvaloniaObject`，保持普通数据对象定位。 |
| `BindableCascaderOption` | 绑定型选项模型，支持 Avalonia binding target 和 scoped resource host 生命周期。 |
| `ICascaderItemDataLoader` | 节点异步加载边界，按目标节点返回子选项集合和加载状态。 |

`ICascaderOption.Header` 是默认显示内容；`ItemKey` 优先作为稳定 identity；`Value` 是 identity 兜底和业务值承载；`Children` 是层级结构入口；`ParentNode` 用于路径回溯、展开路径和显示路径计算。

### 3.2 选项与选择 API

| API | 类型 | 语义 |
| --- | --- | --- |
| `OptionsSource` | `IEnumerable<ICascaderOption>?` | 外部选项集合。变化时同步到内部 `Options`。 |
| `Options` | `ItemCollection` | XAML 内容子项入口，也是 CascaderView 实际数据入口。 |
| `OptionTemplate` | `IDataTemplate?` | 选项显示模板，默认显示 `ICascaderOption.Header` 并继续以 option 为 DataContext。 |
| `SelectedOption` | `ICascaderOption?` | 单选当前选项，默认 `TwoWay` binding，并启用 Avalonia data validation。 |
| `SelectedOptions` | `IList<ICascaderOption>?` | 多选当前选项集合，默认 `TwoWay` binding，并启用 Avalonia data validation。 |
| `DefaultSelectOptionPath` | `TreeNodePath?` | 单选默认路径。路径段按 `ItemKey` 优先、`Value` 兜底匹配。 |
| `SelectedOptionPath` | internal `string?` | 单选显示路径文本，由已选节点 header 路径组成。 |
| `Clear()` | method | 清空单选、多选和显示路径。 |

### 3.3 行为 API

| API | 类型 | 语义 |
| --- | --- | --- |
| `IsMultiple` | `bool` | 是否启用多选勾选。启用后 `CascaderView.IsCheckable=true`。 |
| `ShowCheckedStrategy` | `TreeSelectCheckedStrategy` | 多选 tag 展示策略，不改变真实 `SelectedOptions`。 |
| `MaxCount` | inherited `int` | 多选最大可选数量。达到上限后未选项进入最大数限制状态。 |
| `IsAllowSelectParent` | `bool` | 是否允许非叶子节点作为选择结果。 |
| `ExpandTrigger` | `CascaderViewExpandTrigger` | 级联展开触发方式，默认 `Click`。 |
| `DataLoader` | `ICascaderItemDataLoader?` | 展开未加载节点时的异步数据加载器。 |
| `CascaderView.AsyncLoadTimeout` | internal view API | 内部弹层 view 的异步加载超时边界，不是外层 `Cascader` 当前 public property。 |
| `Filter` | `IValueFilter?` | 路径过滤器；为空时初始化为 contains 过滤。 |
| `FilterValue` | inherited `object?` | 当前过滤输入值。 |
| `FilterHighlightStrategy` | `TextBlockHighlightStrategy` | 过滤命中高亮策略。 |
| `FilterHighlightForeground` | `IBrush?` | 过滤命中高亮前景色。 |

### 3.4 输入表面 API

Cascader 继承 `AbstractSelect` 的输入表面契约：

| API | 语义 |
| --- | --- |
| `SizeType` | 输入尺寸密度，支持 `Large / Middle / Small / Custom`。 |
| `StyleVariant` | 输入表面样式。 |
| `Status` | 手动输入反馈状态；warning 仅在无 native validation error 时驱动 `:warning`，error 视觉优先由 `DataValidationErrors` 驱动。 |
| `PlaceholderText` / `PlaceholderForeground` | 空选择时的占位文本和颜色。 |
| `IsFilterEnabled` | 是否显示过滤输入并将输入同步为 `FilterValue`。 |
| `IsShowOverflowTip` | 单选路径或多选 tag 视觉溢出时是否显示完整内容 tooltip，默认 `true`。 |
| `OverflowTipDelay` | 溢出 tooltip 打开前的延迟时间，单位毫秒，默认 `1200`。 |
| `OverflowTipPlacement` | 溢出 tooltip 相对单选路径或多选 tag 的位置，默认 `TopEdgeAlignedLeft`。 |
| `IsAllowClear` / `SuffixIcon` / `SuffixLoadingIcon` | 清除、展开指示和 loading 指示入口。 |
| `LeftAddOn` / `RightAddOn` / `ContentLeftAddOn` / `ContentRightAddOn` | 外部和内部 AddOn 内容。 |
| `IsDropDownOpen` / `PopupPlacement` / `ShouldUseOverlayPopup` / `IsPopupMatchSelectWidth` | popup 打开、定位、宿主和宽度匹配契约。 |
| `DropDownOpening` / `DropDownOpened` / `DropDownClosing` / `DropDownClosed` | 继承自 `AbstractSelect` 的 popup 生命周期事件。 |

### 3.5 内部 view 事件契约

`CascaderView` 是内部弹层控件，当前事件用于 `Cascader` 和测试协作，不作为外层 Cascader 的主用户事件模型：

| 事件 | 语义 |
| --- | --- |
| `SelectedOptionsChanged` | 内部多选集合变化。 |
| `ItemAsyncLoaded` | 单个节点异步加载完成。 |
| `ItemExpanded` / `ItemCollapsed` | 内部节点展开 / 收起。 |
| `ItemClicked` / `ItemDoubleClicked` | 内部节点点击 / 双击。 |
| `OptionSelected` | 内部单选选项提交。 |

### 3.6 稳定 template part

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_InputControlFrame` | `CascaderAddOnDecoratedBox` | `InputControlFrame` / AddOnDecoratedBox 组合、Addon、EffectiveStatus、CompactSpace、多选状态和 content padding 承载。 |
| `PART_SingleFilterInput` | `SelectFilterTextBox` | 单选过滤输入，过滤启用时显示。 |
| `SelectedOptionsBox` | `SelectTagAwareTextBox` | 多选结果 tag 展示和多选过滤输入承载。 |
| `PART_SelectMaxCountIndicator` | `SelectMaxCountIndicator` | 最大选择数量提示。 |
| `PART_ContentRightAddOnPresenter` | `ContentPresenter` | 用户内部右侧内容承载。 |
| `PART_SelectHandle` | `SelectHandle` | 展开、loading、清除和 Form feedback 图标入口。 |
| `PART_Popup` | `Popup` | 级联弹层宿主。 |
| `PopupFrame` | `Border` | 弹层外壳、最大高度、最小宽度和 popup padding。 |
| `PART_CascaderView` | `CascaderView` | 级联弹层内容、列、过滤和选项交互。 |
| `PART_ItemsPanel` | `StackPanel` | CascaderView 内部横向列容器。 |
| `PART_RootLevelList` | `CascaderViewLevelList` | CascaderView root 列。 |
| `PART_FilterList` | `CascaderViewFilterList` | CascaderView 过滤结果列表。 |

稳定伪类来自 `AbstractSelect` 和 `CascaderViewItem`：

- `:dropdownopen`、native validation `:error`、AtomUI warning `:warning`、`:pressed`。
- `InputControlFrame` variant 伪类：`:outlined`、`:filled`、`:borderless`、`:underlined`；Cascader 专用伪类只表达 dropdown、候选和结果状态。
- `CascaderViewItem` 使用 `:expanded`、`:checked`、`:selected` 和 checkbox toggle type 伪类。

## 4. 行为与状态模型

核心状态流：

```text
OptionsSource / Options
      ↓
CascaderView.Options
      ↓
CascaderViewLevelList
      ↓
CascaderViewItem
      ↓
SelectedOption / SelectedOptions
      ↓
SelectedOptionPath / EffectiveSelectedOptions
      ↓
input display + Form value
```

单选模式：

- `IsMultiple=false` 时使用 `SelectedOption` 作为真实值。
- 叶子节点点击后提交选择并关闭 popup。
- `IsAllowSelectParent=true` 时，非叶子节点也可提交为选择结果。
- `SelectedOptionPath` 根据当前选项的 parent 链生成 header 路径，用于输入框展示。
- `DefaultSelectOptionPath` 只在当前选择为空或强制刷新路径时应用。

多选模式：

- `IsMultiple=true` 时使用 `SelectedOptions` 作为真实值，并让内部 `CascaderView` 进入 checkable 模式。
- `SelectedOptions` 保留真实勾选集合，`ShowCheckedStrategy` 只计算 `EffectiveSelectedOptions`，用于 tag 展示。
- `SelectedOptions` 支持外部集合替换，也支持 `INotifyCollectionChanged` 集合的原地 `Add`、`Remove`、`Replace`、`Move` 和 `Reset`；这些变化会同步刷新 tag、计数、空状态、Form value 和内部 `CascaderView` 勾选状态。
- `MaxCount` 达到上限时，未选项通过 `IsMaxSelectReached` 进入受限状态；已选项仍可取消。
- 关闭单个 tag 时，目标节点及其子孙会从 `SelectedOptions` 中移除。

展开和异步加载：

- 展开按节点 parent 链从 root 到目标逐级执行。
- 同一级只保持一个已展开分支，展开新分支会折叠同级旧分支。
- 未加载且非 leaf 的节点在有 `DataLoader` 时进入 loading，加载完成后把返回子项加入目标 `Children`。
- 控件 detached 时取消待处理异步加载，避免离开视觉树后继续处理结果。

过滤：

- `FilterValue` 非空且控件 loaded 时，CascaderView 收集路径并按 `Filter` 过滤；普通单选只收集末端路径，允许选择父节点或多选时也收集父路径。
- 过滤结果显示完整路径文本。`Up` / `Down` 在可用结果间循环移动候选，不提前修改选择；`Enter` 只提交当前有效候选，没有候选或没有可用结果时保持选择和 popup 状态不变。
- 路径中任一祖先 disabled 时，该结果不能成为可提交候选。查询更新后，完整原始路径仍在新结果中时保留候选，否则清除。
- 过滤列表拥有独立于树列的 active candidate owner。进入过滤模式、过滤清空、popup 关闭与模板替换时清除不再适用的候选；树列与过滤列不会同时保留候选视觉。
- 单选确认后清空过滤并关闭 popup；多选确认切换子树勾选，清空过滤并保持 popup 打开。搜索中的左右键保留文本编辑语义，非空搜索的 Backspace 不导航到父列。

树形键盘导航：

- 关闭时，`Down`、`Enter` 或 `Space` 打开 popup；同一次打开按键不同时激活或提交首项。已有单选值时恢复其活动路径；`F4` 保留为桌面切换快捷键。
- popup 打开且未过滤时，`Up` / `Down` 只在当前活动节点所属列的同级 enabled option 中循环；尚无候选时在根列开始。键盘导航基于数据节点，不依赖当前已物化的容器，并将目标滚入视口。
- 活动非叶节点显示子列，但候选仍停在该节点；LTR 的 `Right` 从当前候选进入首个 enabled child，`Left` 返回父节点，在根层请求关闭 popup。空搜索的 Backspace 同样返回父层；RTL 的左右方向反转。
- `Enter` 只提交 enabled、非 loading 的当前候选；普通单选父节点不提交也不自动进入子级。`IsAllowSelectParent=true` 时可以提交父节点并关闭 popup；多选则切换子树勾选并保持打开。
- 普通树列的 active candidate 由 `CascaderView` 单一持有；鼠标迁移候选和 hover 展开继续复用该状态，不提前提交选择，也不因鼠标移动滚动列表。
- 一次 Enter 按下/释放周期最多确认一次；组合输入期间不处理候选导航与提交，修饰键组合留给文本输入。`Escape` 关闭 popup，Tab 不提交候选。

Form：

- 单选 Form value 为 `SelectedOption`。
- 多选 Form value 为 `SelectedOptions`。
- Form 校验错误写入同一份 Avalonia `DataValidationErrors`；`SelectedOption` 和 `SelectedOptions` 不维护独立错误状态。
- Form clear 会按当前 `IsMultiple` 清空对应选择状态。

## 5. 视觉与主题模型

Cascader 的默认视觉由 Cascader 根主题、`InputControlFrame` / CascaderAddOnDecoratedBox、PopupHost、CascaderView、CascaderViewItem、SelectTagAwareTextBox、SelectHandle 和 CascaderToken 协作完成。

| 主题或资源 | 职责 |
| --- | --- |
| `CascaderTheme.axaml` | 根模板、输入壳体、单选结果、多选 tag、handle、popup 和 CascaderView 绑定。 |
| `CascaderViewTheme.axaml` | 弹层内部级联列、空状态、过滤列表和默认 option template。 |
| `CascaderViewItemTheme.axaml` | option 行、checkbox、icon、header、展开 / loading icon、hover / expanded / disabled 状态。 |
| `CascaderViewLevelListTheme.axaml` | 单列列表宽度、高度、padding 和滚动行为。 |
| `CascaderToken` | Cascader 输入宽度、列宽、弹层高度、选项高度、padding、状态色和过滤高亮。 |
| `PopupHostToken` | popup margin、阴影和圆角。 |
| SharedToken | 字体、输入高度、图标尺寸、placeholder、disabled、motion 和全局 spacing。 |

单选路径和多选 tag 的完整内容提示复用共享 `OverflowTip` attached behavior。主题只把 `IsShowOverflowTip`、`OverflowTipDelay`、`OverflowTipPlacement` 和当前展示文本传给显示节点；tooltip 仅在视觉溢出时写入，且不覆盖用户手动声明的 `ToolTip.Tip`。

主题不可破坏的视觉边界：

- 输入表面必须继续由 `InputControlFrame` 承载；`CascaderAddOnDecoratedBox` 只扩展多选、dropdown 和 content padding 布局。
- `PART_SelectHandle` 的 hover / pressed / dropdown open / clear / loading 状态必须与输入壳体保持同步。
- `OptionTemplate` 的 DataContext 必须保持为 `ICascaderOption`，不能改为 header 文本。
- Popup 宽度和空状态宽度匹配语义必须保持：普通级联列使用列宽，空状态需要匹配输入宽度。
- 选项行的 checkbox、icon、header、expand / loading icon 间距由 Token 管理，不应在单个模板节点中写死。
- disabled、expanded、pointerover、checked、loading 和 active candidate 状态 selector 不能被绕过；`pointerover` 只触发 active candidate 迁移，不独立绘制第二个候选背景。

## 6. 控件家族或集成关系

Cascader 属于 Data Entry 选择控件家族，与 Select、TreeSelect、DatePicker、TimePicker 等共享输入尺寸、variant、status、Addon、Form、Popup 和 CompactSpace 语义。

集成关系：

- `AbstractSelect`：输入壳体、popup、清除、Form、feedback、AddOn、CompactSpace 和 `CustomizableSizeType` 契约。
- `CascaderAddOnDecoratedBox`：Cascader 专用输入布局扩展，接收多选和 dropdown 状态并复用 shared frame。
- `CascaderView`：弹层内容、级联列、过滤、展开、选择、勾选和异步加载。
- `CascaderViewLevelList`：单级选项列表和虚拟化容器生命周期。
- `CascaderViewItem`：单个选项容器、状态视觉、checked / expanded 事件和绑定型选项同步。
- `SelectTagAwareTextBox` / `SelectTag`：多选结果 tag 展示和 tag close 行为。
- `SelectHandle`：右侧操作入口，负责展开指示、loading、清除和 Form feedback。
- `ICascaderItemDataLoader`：异步加载边界。
- `IFormItemAware` / `IFormItemFeedbackAware`：由 `AbstractSelect` 接入 Form 值、扩展状态和 feedback；error 由 `DataValidationErrors` 投射到 `InputControlFrame`。

## 7. 兼容性不变量

维护 Cascader 时必须保持以下不变量：

- `CascaderOption` 保持轻量数据模型定位，不直接改造成 `AvaloniaObject`。
- 需要 binding target 或动态资源能力时使用 `BindableCascaderOption`。
- `Header` 的容器内容必须继续是 option 对象本身，避免破坏 `OptionTemplate` 的数据上下文。
- `SelectedOption` 和 `SelectedOptions` 的单选 / 多选边界不能混用。
- 键盘候选只能表达当前导航位置，不能通过 `SelectedIndex` 或 `SelectedOption` 提前提交真实选择；disabled 或 loading item 不得成为可提交候选。
- `ShowCheckedStrategy` 只能影响 `EffectiveSelectedOptions`，不能改写真实 `SelectedOptions`。
- `IsAllowSelectParent=false` 时，非 leaf 节点不能作为普通单选结果提交。
- `DefaultSelectOptionPath` 的路径段必须继续按 `ItemKey` 优先、`Value` 兜底匹配。
- `OptionsSource` 变化后必须尽量按路径 identity 保留当前选择。
- 异步加载时 detached 必须取消待处理加载。
- 容器回收、ItemsSource 变化和 detach 时必须释放绑定型选项的 resource host attach、children 集合订阅和属性订阅。
- 右侧稳定 template part 绑定优先使用 AXAML compiled binding，不把可静态表达的绑定搬回 C#。
- Template part、Token 名称、伪类和主题 selector 不得在未授权情况下重命名或删除。

## 8. 专项模型

### 8.1 绑定型选项

`BindableCascaderOption` 是 `ICascaderOption` 的兼容新增实现。它只服务需要 Avalonia binding target 的选项，不替代 `CascaderOption`。资源宿主生命周期由 `[GenerateScopedResourceHost]` 生成，owner 容器负责 attach/release。

绑定型选项支持的稳定同步语义：

- option 到容器：icon、checked、enabled、expanded、checkbox enabled、value、item key、leaf 和 children 变化。
- 容器到 option：checked 和 expanded。
- dynamic resource：优先使用当前 owner resource，再 fallback 到 Application。

### 8.2 路径 identity

Cascader 使用 `ItemKey` 或 `Value` 组成 `TreeNodePath`，用于默认选择、默认展开和 `OptionsSource` 替换后的选择恢复。路径显示文本使用 `Header` 链接，不参与 identity 匹配。

缺少稳定 identity 时，控件无法可靠跨数据源替换保留选择。维护时不能把 header 文本作为 identity 兜底，否则会把展示内容变化误认为业务节点变化。

### 8.3 勾选展示策略

`ShowCheckedStrategy` 复用 `TreeSelectCheckedStrategy`：

- `All`：展示所有真实勾选节点。
- `ShowParent`：当父节点完整覆盖子节点时展示父节点，隐藏对应子孙。
- `ShowChild`：只展示叶子节点。

该策略只影响多选 tag 展示集合，不改变 Form value 和 `SelectedOptions`。

## 9. 文档导航与验证策略

- [Cascader 桌面版实现原理](implementation.md)
- [Cascader Semantic Part 契约](semantic-part.md)
- [Cascader Token 设计](token.md)
- [Cascader Changelog](changelog.md)

验证要求：

| 改动类型 | 验证要求 |
| --- | --- |
| Public API 或数据模型 | 验证 `CascaderOption` 兼容性、`BindableCascaderOption` binding 和资源生命周期。 |
| 选择 / 勾选 / 展开 | 验证单选、多选、父级可选、半选父级、`ShowCheckedStrategy` 和同级展开互斥。 |
| 过滤行为 | 验证过滤输入、完整路径结果、过滤命中高亮、过滤清理和空状态。 |
| 异步加载 | 验证 loading、取消、超时、loaded children、重复加载合并和 detached cancel。 |
| AXAML 或 template part | 验证 AddOnDecoratedBox、右侧 count/content/handle binding、popup、CascaderView、level list 和 option template。 |
| Token 或尺寸 | 验证 Large / Middle / Small / Custom、列宽、选项高度、padding、图标间距和空状态宽度。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |
