# Cascader 桌面版实现原理

本文档描述 Cascader 桌面版的输入壳体、选项集合、级联展开、选择同步、勾选、过滤、异步加载、绑定型选项和资源生命周期。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计与 API 契约见 [Cascader 桌面版架构设计](overview.md)，候选列表状态契约见 [候选列表统一交互设计](../select/candidate-interaction-design.md)，Token 语义见 [Cascader Token 设计](token.md)，变化记录见 [Cascader Changelog](changelog.md)。

Popup 接入边界：`AbstractSelect` 负责业务状态和内容准备，`PART_Popup` 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

Cascader 的实现由外层 `Cascader` 和内部 `CascaderView` 组成。`Cascader` 负责输入表面、popup、选择结果、默认值、Form 和 AddOn 集成；`CascaderView` 负责弹层内的级联列、过滤、展开、勾选和异步加载。本文档聚焦 Cascader 自身的状态编排、容器生命周期、数据节点边界和维护不变量，不重复 Select 家族输入壳体的通用实现。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Cascader/Cascader.cs`：public API、输入壳体、popup、选择结果、默认路径、Form / CompactSpace 集成和外层交互。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderView.cs`：弹层内容、选项源、选择、过滤入口、空状态、默认展开路径和模板接入。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderView.ExpandAndCollapse.cs`：展开、收起、同级互斥展开、子列创建和默认展开路径。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderView.Check.cs`：多选勾选、子树勾选、父级半选和 `SelectedOptions` 同步。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderView.AsyncItemDataLoad.cs`：异步加载、超时、取消、并发加载合并和 loaded 事件。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderView.Filter.cs`：过滤路径收集、过滤结果列表、过滤计数和高亮文本。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderViewLevelList.cs`：单级列表、容器准备、虚拟化上下文保存和清理。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderViewItem.cs`：选项容器、checked / expanded / selected 事件、leaf 判断和绑定型选项同步。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderOption.cs`：`ICascaderOption`、默认轻量 `CascaderOption` 和层级 helper。
- `src/AtomUI.Desktop.Controls/Cascader/BindableCascaderOption.cs`：绑定型选项模型，使用 scoped resource host generator。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderAddOnDecoratedBox.cs`：Cascader 输入布局扩展，复用 `InputControlFrame` 的输入表面状态。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderViewPanel.cs`：弹层 frame 布局与边框测量。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderViewFilterList.cs`、`CascaderViewFilterListItemData.cs`：过滤结果列表和路径数据。
- `src/AtomUI.Desktop.Controls/Tooltip/OverflowTip.cs`：共享溢出 tooltip attached behavior，供单选路径和多选 tag 复用。
- `src/AtomUI.Desktop.Controls/Cascader/Themes/*.axaml`：Cascader、CascaderView、CascaderViewItem、level list 和 filter list 主题。
- `src/AtomUI.Desktop.Controls/Cascader/CascaderToken.cs`：Cascader 专属 Token。

## 3. 核心类职责

`Cascader` 是用户直接使用的输入控件。它维护 `Options`、`SelectedOption`、`SelectedOptions`、`EffectiveSelectedOptions`、`SelectedOptionPath`、popup 状态、清除入口、默认选择路径和 Form 映射。它继承 `AbstractSelect` 的输入表面、popup、Form feedback、AddOn、CompactSpace、清除和最大选择数量契约。

`CascaderView` 是 popup 内容控件。它把 `OptionsSource` 同步到内部 `Options`，管理 root level list、过滤结果、选项展开、选项选择、多选勾选和异步加载。

`CascaderViewLevelList` 是单列选项列表。它负责创建 `CascaderViewItem`，恢复默认上下文，保存虚拟化上下文，并在容器清理时释放绑定型选项的生命周期资源。

`CascaderViewItem` 是选项容器。它把 option 状态映射为 header、icon、checked、enabled、expanded、checkbox enabled、leaf 和 value。绑定型选项进入容器时，容器同时负责 resource host attach、属性订阅、children 集合订阅和 checked / expanded 反写。

`CascaderOption` 是默认轻量数据模型。它维护 children 与 parent 的基本关系，不承载 Avalonia 属性系统和动态资源。

`BindableCascaderOption` 是绑定型数据模型。它继承 `AvaloniaObject`，通过 DirectProperty 承载选项属性，并使用 `[GenerateScopedResourceHost]` 生成 scoped resource host 能力。

`OverflowTip` 是显示增强边界，不拥有 Cascader 选择状态。`CascaderTheme.axaml` 将单选 `SelectedOptionPath`、过滤输入 placeholder 路径、多选 `SelectTagAwareTextBox` 的 tag 文本以及 `OverflowTipDelay` / `OverflowTipPlacement` 接入该 behavior；单选路径和过滤输入以外层 `CascaderAddOnDecoratedBox` 作为 `PlacementTarget`，避免 tooltip 左边按内部文本 padding 对齐；它只托管自己写入的 `ToolTip.Tip`，避免与业务自定义 tooltip 形成双 owner。

## 4. 状态与数据流

### 4.1 选项源流

```text
Cascader.OptionsSource / Options
      ↓
Cascader.HandleCascaderSourceChanged
      ↓
_options ItemCollection
      ↓
CascaderView.OptionsSource
      ↓
CascaderView.Options
      ↓
PART_RootLevelList.ItemsSource
```

`OptionsSource` 变化时，外层 Cascader 会先捕获当前选择的路径 identity，再替换内部 `_options`。单选模式下按旧路径尝试重映射 `SelectedOption`，失败时应用 `DefaultSelectOptionPath`。多选模式下按每个已选项路径重映射 `SelectedOptions`。

`SelectedOption` 和 `SelectedOptions` 是用户受控值，属性注册默认 `TwoWay` binding，并启用 Avalonia data validation。外层 `Cascader` 是 Form value owner；内部 `CascaderView` 只承载 popup 里的展开、选中和勾选视觉状态，不反向拥有外层值。

`SelectedOptions` 为集合值时，外层会在控件进入视觉树后订阅 `INotifyCollectionChanged`，离开视觉树或集合替换时释放订阅。集合原地 `Add`、`Remove`、`Replace`、`Move`、`Reset` 会刷新 `EffectiveSelectedOptions` 展示快照、`SelectedCount`、`IsSelectionEmpty`、Form value 通知和内部 `CascaderView.SelectedOptions`。

### 4.2 展开流

```text
CascaderViewItem.IsExpanded changed
      ↓
CascaderView.HandleCascaderItemExpanded
      ↓
ExpandItemAsync(option)
      ↓
build root-to-target path
      ↓
collapse sibling expanded branch
      ↓
create child CascaderViewLevelList from option.Children
```

展开目标必须能从当前 root options 中找到完整路径。找不到 root 或路径不完整时，控件把它视为不属于当前 CascaderView 的野数据。

### 4.3 勾选流

```text
CascaderViewItem.IsChecked changed
      ↓
CheckedSubTree / UnCheckedSubTree
      ↓
MarkViewOptionChecked(option)
      ↓
SetupParentNodeCheckedStatus
      ↓
CascaderView.SelectedOptions
      ↓
Cascader.SelectedOptions
      ↓
EffectiveSelectedOptions
```

父级状态按有效子节点计算。有效子节点必须同时满足 enabled 和 checkbox enabled。父级结果为 `true`、`false` 或 `null`，其中 `null` 表达半选。

### 4.4 过滤流

```text
SelectFilterTextBox.TextChanged
      ↓
Cascader.FilterValue
      ↓
CascaderView.FilterItems
      ↓
collect leaf path infos
      ↓
Filter.Filter(selector(pathInfo), FilterValue)
      ↓
CascaderViewFilterList
```

过滤结果只收集叶子路径。结果项的 `Content` 是 header 路径文本，`ExpandItems` 是完整 option 路径，选择结果时回写末级 option。

### 4.5 绑定型选项同步流

```text
CascaderViewItem prepared
      ↓
BindableCascaderOption.AttachResourceHost(owner)
      ↓
option.PropertyChanged -> container state
option.Children.CollectionChanged -> container leaf state
container IsChecked / IsExpanded changed -> option state
      ↓
container clear / detach -> dispose attachment and subscriptions
```

`Header` 的容器值保持为 option 对象本身。默认 `OptionTemplate` 通过 `Header` 绑定显示文本，用户自定义模板也继续以 option 为 DataContext。

## 5. 生命周期与模板接入

`Cascader` 静态构造注册清除、过滤输入、tag close、multiple、OptionsSource 和选择属性变化的 class handler。

`Cascader` 构造时注册 Cascader token scope，并监听 `Options.CollectionChanged`。`OnInitialized()` 设置默认 contains filter。`OnLoaded()` 应用 `DefaultSelectOptionPath`。

`Cascader.OnApplyTemplate()` 重新应用模板时必须先解除旧 `_cascaderView` 的事件订阅，并清空旧 view 的 `OptionsSource`。随后获取 `PART_SingleFilterInput` 和 `PART_CascaderView`，重新订阅内部 view 事件并把 `Options` 转发给 view。

`CascaderView.OnApplyTemplate()` 获取 `PART_ItemsPanel`、`PART_RootLevelList` 和 `PART_FilterList`。Root level list 设置 `Level=1` 和 `OwnerView=this`，filter list 必须解除旧 selection 订阅再订阅新实例。

钉住弹层（`IsPopupPinnedOpen`）的遮罩抑制机制由 `AbstractSelect` 统一承载（与 `AbstractAutoComplete` 同构）：`OnPropertyChanged` 的钉住分支在自动打开前抑制 `PART_Popup.IsLightDismissEnabled`，`OnApplyTemplate` 在 relay 绑定后按钉住状态抑制，尾部 `IsDropDownOpen && !Popup.IsOpen → OpeningDropDown(false)` 负责模板应用后的打开；`PopupClosed` 强制回写 `IsDropDownOpen=false`，覆盖 light-dismiss 关闭路径的状态同步。Cascader 模板的 Popup 因此不声明 `IsOpen` 模板绑定——模板充气阶段的绑定求值会在抑制之前打开弹层并留下不可消除的遮罩层。取消钉住时抑制恢复模板默认 true，下一轮打开恢复常规遮罩行为；该机制只服务 Gallery 语义预览等钉住场景，不改变普通打开/关闭流程。

Semantic Part marker 的维护边界：`CascaderTheme.axaml` 承载触发区静态 marker（`semantic-scope-input`、`semantic-prefix`、`semantic-suffix`、`semantic-scope-handle`、`semantic-content`、`semantic-placeholder`、`semantic-input`、`semantic-popup-root`、`semantic-scope-view`）；`CascaderViewTheme.axaml` 承载 `semantic-scope-frame` 与根列、过滤列表的 `semantic-popup-list` 静态 marker；共享 `SelectHandleTheme.axaml` 承载清除按钮的 `semantic-clear` marker（`clear` Part 声明 `CrossNestedOwners=true`，生成器沿 SelectHandle 主题链校验）。运行时注入点：`CascaderView` 创建子级列时追加 `CascaderSemanticParts.PopupListClass`；`CascaderViewLevelList.CreateContainerForItemOverride()` 与 `CascaderViewFilterList.CreateContainerForItemOverride()` 向容器追加 `CascaderSemanticParts.PopupListItemClass`。marker 随容器实例创建一次，prepare/clear/recycle 路径不得增删。

`CascaderView` 以数据节点 `_keyboardCandidateOption` 持有树列活动位置，从其祖先链解析当前列。过滤列表持有结果候选，两者不以虚拟化容器作为状态 owner。pointer move 仍迁移候选并按 `ExpandTrigger` 展开；键盘激活才滚动目标。查询重建按完整路径保留有效过滤候选，源替换、清空和关闭则清理失效状态。树容器准备时重新投影 `IsCandidateSelectedProperty`，回收时清除，防止高亮进入下一个 option 的复用容器。

`CascaderViewLevelList.ContainerForItemPreparedOverride()` 对每个 `ICascaderOption` 调用 `CascaderViewItem.PrepareCascaderOptionData()`。如果 option 是 `BindableCascaderOption`，容器创建同一生命周期的 `CompositeDisposable`，用于保存 resource host attachment、属性订阅和 children 集合订阅。

`ClearContainerForItemOverride()` 保存虚拟化上下文后，必须先释放绑定型选项订阅，再清空容器属性。这个顺序防止容器清理动作把空值反写到用户数据。

`CascaderViewItem.OnDetachedFromVisualTree()` 释放当前绑定型选项生命周期，防止 detached 容器继续持有 option 或 owner resource host。

`CascaderView.OnDetachedFromVisualTree()` 取消所有待处理异步加载。

## 6. 交互与事件处理

Pointer 路径：

- 外层 Cascader 在 popup 外 pointer release 时切换 `IsDropDownOpen`。
- popup 内 pointer 事件不触发外层关闭。
- tag close button 点击不触发外层 popup toggle。
- `CascaderViewLevelList` 点击 option 时先触发 item click，再按 `ExpandTrigger` 决定是否展开。
- `CascaderViewLevelList` / `CascaderViewFilterList` 的 pointer move 只负责把命中的 enabled item 迁移为 active candidate；普通列的鼠标路径不滚动，过滤列不因 pointer move 无条件清除候选。

键盘路径：

- Cascader 在隧道路由阶段接收搜索输入的候选键，防止 TextBox 提前消费方向键；过滤中的左右键与修饰键保持文本编辑语义，preedit 非空时不导航或提交。
- Cascader 与 CascaderView 的 Enter 按下状态分别属于宿主输入周期和视图转发周期；KeyUp 释放状态，失焦、detach 和模板生命周期不能留下锁定。关闭状态的 Enter/Space/Down 只负责打开。
- 过滤上下键在 displayed result order 中循环，更新 `CandidateSelectedItem` / `CandidateSelectedIndex` 并滚动目标；结果重建通过完整 option 路径重新定位仍有效的候选。Enter 不提供首项兜底，进入与 pointer 结果选择相同的校验和提交路径。
- 树上下键枚举当前活动节点的同级数据。激活分支调用既有展开流程显示子列，不改变活动深度；Right/Left 分别进入可用子项或返回父节点，RTL 映射方向。根层返回、Escape 与单选键盘提交通过 internal `CloseRequested` 交给 popup 宿主。
- 展开在异步加载前标记节点为打开；等待数据期间允许后续分支将其折叠，完成后检查该状态，避免旧分支重新展开。键盘展开后也核对活动节点、源归属、过滤和加载状态，过期请求不再投影候选。
- 单选 Enter 只提交叶节点或允许选择的父节点；多选进入既有 CheckedSubTree/UnCheckedSubTree，遵守禁用与最大选择限制。候选迁移不写 `SelectedIndex`、`SelectedOption` 或 `SelectedOptions`。
- Gallery 的两个独立 SearchEdit 组合在公共宿主转发按键；通过实例级关闭默认 Enter 搜索，统一新查询搜索与当前候选确认。转发完整 KeyDown/KeyUp，焦点保留在输入框，不复制候选算法。

单选路径：

- leaf item selected 或 clicked 时提交 `SelectedOption`。
- `IsAllowSelectParent=true` 时，非 leaf option 也可提交。
- 单选 popup 关闭时清空过滤输入和 `FilterValue`。

多选路径：

- `IsMultiple=true` 时，`CascaderViewItem.CheckedEvent` 触发 subtree check / uncheck。
- `SelectedOptionsChanged` 从内部 view 同步到外层 Cascader。
- 多选结果使用共享 `SelectTagAwareTextBox`；刷新 tags 时保留行内搜索框节点，仅替换 tags 和剩余数量节点，避免丢失输入焦点与待释放的 KeyUp。
- 外部 `SelectedOptions` 替换或原地集合变化同步到内部 view 时按差量更新，只勾选新增项、取消移除项，避免先清空再重选造成状态闪烁。
- tag close 通过 `SelectTag.ClosedEvent` 移除目标 option 及其子孙。

过滤路径：

- `SelectFilterTextBox.TextChangedEvent` 更新 `FilterValue`。
- `CascaderViewFilterList.SelectionChanged` 选择过滤结果，并按模式提交单选或切换子树勾选。

## 7. 内部算法与关键流程

### 7.1 路径 identity

`BuildOptionIdentityPath()` 从当前 option 沿 `ParentNode` 回溯到 root。每一级路径段优先使用 `ItemKey`，缺失时使用 `Value?.ToString()`。任一级没有可用段时，整条路径不可用于重映射。

`TryParseSelectPath()` 按路径段逐级在当前 options 中查找，匹配规则同样是 `ItemKey` 优先、`Value` 兜底。

### 7.2 默认选择路径

`DefaultSelectOptionPath` 只在单选模式生效。它应用时生成 `SelectedOptionPath` 显示文本，但不直接把末级 option 写入 `SelectedOption`，直到用户交互或外部选择状态进入真实选择流。

### 7.3 展开路径

`ExpandItemAsync(ICascaderOption)` 先从目标节点沿 `ParentNode` 回溯到 root，构造路径。只有路径 root 存在于当前 `_options` 中时才继续展开。

每一级展开前会折叠同级已展开项，保证级联列只表达一条当前展开路径。子列由目标节点 `Children` 创建，`OwnerView`、`ItemTemplate`、`IsAllowSelectParent` 和 `ExpandTrigger` 从 CascaderView 继承。

### 7.4 Leaf 判断

容器 leaf 状态由 `CascaderViewItem.ConfigureIsLeaf()` 统一判断。非异步模式下主要按 children 是否为空判断；异步模式下，`option.IsLeaf`、`AsyncLoaded` 和 children 状态共同决定是否显示展开入口。

绑定型选项的 `IsLeaf`、`Children` 属性替换或当前 children 集合内容变化会触发容器重新执行 leaf 判断，不直接绕过 Cascader 的异步加载语义。

### 7.5 勾选父级状态

父级勾选状态按有效子节点计算。有效子节点必须同时满足 enabled 和 checkbox enabled。父级结果为：

- 全部有效子节点 checked：`true`。
- 任一有效子节点 checked 或 indeterminate：`null`。
- 无有效子节点 checked：`false`。

### 7.6 多选展示策略

`BuildEffectiveSelectedOptions()` 根据 `ShowCheckedStrategy` 从真实 `SelectedOptions` 计算展示集合：

- `All` 使用真实集合的展示快照。
- `ShowParent` 保留完整覆盖子节点的父级，隐藏其子孙。
- `ShowChild` 只保留没有 children 的节点。

该计算只影响 tag 展示，不改写真实选中集合。展示快照会在真实集合替换或原地变化时重建，保证 tag 区域不会因为集合实例未变化而保留旧内容。

### 7.7 过滤路径缓存

`FilterItems()` 首次过滤时收集所有叶子路径到 `_allPathInfos`，按路径文本排序并缓存。过滤值、过滤器或选项源变化时会重新过滤；`ClearFilter()` 清理过滤状态、结果和缓存。

### 7.8 绑定型选项资源宿主

`BindableCascaderOption` 不持有容器或 ShowCase。容器准备时临时 attach 到 `CascaderView` 或当前 level list 的 `IResourceHost`，清理时释放 attach。动态资源查找使用 owner resource 优先，再 fallback 到 Application。

## 8. 资源、性能与 AOT 边界

绑定型选项使用 source generator 生成 scoped resource host，不手写 `IResourceHost` / `IThemeVariantHost` 样板代码，不使用运行时反射扫描或字符串 binding。

模板内稳定关系优先放在 AXAML 中表达。右侧 count、content add-on 和 handle 状态使用 compiled ancestor binding；C# 属性同步只用于运行时生成容器和绑定型非 Visual 数据对象。

容器生命周期是绑定型选项的释放边界。`CascaderViewItem` 持有的 resource host attachment、option property subscription、container property subscription 和 children collection subscription 必须在容器 clear 或 detach 时释放。

异步加载由 `AsyncExpandLoadCoordinator` 合并同一节点并发请求并应用超时。`CascaderView` detached 时必须调用 `CancelAll()`，避免已离开视觉树的控件继续处理加载结果。

过滤路径缓存以当前 options 为边界，清除过滤或 options source 变化时必须失效，不能跨数据源复用旧路径。

## 9. 维护不变量

- 不把 `CascaderOption` 改造成 `AvaloniaObject`。
- 新增 binding target 需求时使用独立 bindable 类型。
- 容器清理时先释放订阅，再清空容器属性。
- `Header` 容器值保持 option 对象，不改成 header 文本。
- checked / expanded 的反写只针对 `BindableCascaderOption`，不改变普通 `CascaderOption` 的 CLR 数据模型语义。
- `ShowCheckedStrategy` 不能改写真实 `SelectedOptions`。
- `DefaultSelectOptionPath` 和 `OptionsSource` 重映射必须继续使用 `ItemKey` 优先、`Value` 兜底的路径 identity。
- 展开、过滤、异步加载和勾选不能通过延迟刷新或抑制标记掩盖状态所有权问题。
- 重新套用模板、container recycle、ItemsSource replacement 和 detach 都必须有对应 release 路径。

## 10. 测试与验证

关键测试：

- `CascaderBindableOptionTests`：绑定型选项 parent 维护、容器同步、children 变化、checked / expanded 反写、owner resource 和 WeakReference 生命周期。
- `CascaderLazyLoadTests`：异步加载时弹层保持打开、loading 和 loaded children。
- `CascaderChangeOnSelectTests`：父级可选时立即选择并保持弹层打开。
- `CascaderThemeContractTests`：主题 binding 和模板契约。
- `CascaderSemanticPartTests`：descriptor 十 Part 契约、模板静态 marker 清单、默认主题不消费 semantic selector、生成 Style 命中触发区目标。
- `CascaderPinnedPopupTests`：钉住弹层打开前抑制 light-dismiss、`popup.list` / `popup.listItem` 部件命中、重开与数据源重置后 marker 保持。

验证命令：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Cascader
dotnet build controlgallery/AtomUIGallery/AtomUIGallery.csproj --no-restore
git diff --check
```

## 根表面主题投射

根几何进入实际输入 frame；多选 Custom 的默认紧凑 inset 与显式 inset 分开，标签内在高度保持。现有多选尺寸/前缀参数继续引用 Select 家族资源。

几何默认、原生优先级和组合边界见[共享输入表面](../input-control-architecture-design.md#81-根表面几何与预设默认值)。
