# TreeSelect 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.TreeSelect` 的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [TreeSelect 桌面版实现原理](implementation.md)，TreeSelect Token 的专项设计见 [TreeSelect Token 设计](token.md)，设计和契约变化记录见 [TreeSelect Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `AbstractSelect`，其 internal `IsPopupPinnedOpen` 只供测试和内部诊断使用；设置为 true 时保持 `IsDropDownOpen` 并 relay 到 `PART_Popup`，设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

TreeSelect 公开 13 个 Semantic Part（`root`、`prefix`、`content`、`placeholder`、`input`、`suffix`、`clear`、`item`、`itemContent`、`itemRemove`、`popup.root`、`popup.list`、`popup.listItem`），与 Ant Design TreeSelect 的 Semantic Part 语义对齐（弹层树节点内部槽位由 `TreeViewItem` 契约承载）；完整 Part 表、单选/多选存在条件、Selector 用法与定制边界见 [TreeSelect Semantic Part 契约](semantic-part.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/TreeSelect` |
| 控件状态 | Stable |

TreeSelect 是 AtomUI 桌面数据录入体系中的树形选择控件，用于在层级数据中完成单选、多选和勾选式选择。它组合 Select 家族的输入壳体、过滤输入、弹层和 Form / CompactSpace 集成能力，并使用 TreeView 家族承载树节点、展开、勾选、图标、连线和异步加载。

TreeSelect 的职责是从 `ITreeItemNode` 层级数据中选择一个或多个节点。它不负责远程数据协议、业务权限过滤、级联业务规则、树节点编辑、拖放排序或复杂数据持久化；这些能力应由业务层、TreeView 或专用数据控件承担。

## 2. 设计语言

TreeSelect 的设计语言来自输入框、树形候选弹层和已选结果的组合。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 树形数据 | 候选项按父子层级展示。 | `ItemsSource`、`Items`、`ItemTemplate`。 |
| 选择模式 | 控件支持单选、多选和 checkbox 勾选。 | `IsMultiple`、`IsTreeCheckable`、`IsTreeCheckStrictly`。 |
| 结果展示 | 单选显示节点文本，多选以 tag 展示有效选中节点。 | `SelectedItem`、`SelectedItems`、`ShowCheckedStrategy`。 |
| 搜索过滤 | 输入过滤值后高亮、加粗、展开路径或隐藏不匹配节点。 | `IsFilterEnabled`、`FilterValue`、`Filter`、`FilterStrategy`。 |
| 树视觉 | 控制图标、叶子图标、连线、展开指示和旋转。 | `IsShowIcon`、`IsShowLeafIcon`、`IsShowLine`、`IsShowTreeLine`。 |
| 输入表面 | 继承 Select 家族的边框、尺寸、状态和 AddOn。 | `SizeType`、`StyleVariant`、`Status`、Addon。 |
| 弹层语义 | 候选树在 popup 中打开，可匹配输入宽度。 | `IsDropDownOpen`、`PopupPlacement`、`IsPopupMatchSelectWidth`。 |

`SizeType=Custom` 不是 TreeSelect 的第四套专属 Token。主题层把 `Custom` 归入 `Middle` 的默认字体和 padding 分支；用户显式设置高度等尺寸属性时由 Avalonia 属性优先级决定最终布局。

## 3. API 与契约模型

TreeSelect 继承 `AbstractSelect` 的输入壳体、弹层、清除、状态、尺寸、Addon、Form 和 CompactSpace 契约；自身提供树数据、树选择、过滤和勾选策略契约。

树数据 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `ItemsSource` | `IEnumerable<ITreeItemNode>?` | 外部树节点集合。变化时同步到内部 `Items`。 |
| `Items` | `ItemCollection` | XAML 内容子项入口，也是候选树实际数据入口。 |
| `ItemTemplate` | `IDataTemplate?` | 树节点显示模板，默认显示 `ITreeItemNode.Header`。 |
| `DataLoader` | `ITreeItemNodeLoader?` | 展开节点时加载子节点的数据加载边界。 |
| `TreeDefaultExpandedPaths` | `IList<TreeNodePath>?` | 默认展开路径集合。 |
| `IsDefaultExpandAll` | `bool` | 默认展开全部节点。 |

选择 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `SelectedItem` | `ITreeItemNode?` | 单选模式当前节点；默认 `BindingMode.TwoWay`，启用 Avalonia `DataValidationErrors`。 |
| `SelectedItems` | `IList<ITreeItemNode>?` | 多选或勾选模式当前节点集合；默认 `BindingMode.TwoWay`，启用 Avalonia `DataValidationErrors`，支持 `INotifyCollectionChanged` 原地变更刷新。 |
| `IsMultiple` | `bool` | 是否使用多选结果模型。 |
| `IsTreeCheckable` | `bool` | 是否使用 checkbox 勾选。启用后 TreeSelect 使用多选结果模型。 |
| `IsTreeCheckStrictly` | `bool` | 勾选时父子节点是否严格独立。 |
| `ShowCheckedStrategy` | `TreeSelectCheckedStrategy` | 多选 tag 的展示策略，决定展示全部、父节点或叶子节点。 |
| `MaxCount` | `int` | 继承自 `AbstractSelect` 的最大可选数量。 |
| `Clear()` | method | 清空 `SelectedItem` 和 `SelectedItems`。 |

过滤与树视觉 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `Filter` | `IValueFilter?` | 树节点过滤器；为空时初始化为 contains 过滤。 |
| `FilterValueSelector` | `DefaultFilterValueSelector?` | 从节点中提取过滤文本。 |
| `FilterStrategy` | `TreeFilterStrategy` | 过滤后的高亮、加粗、展开路径和隐藏不匹配策略。 |
| `FilterHighlightForeground` | `IBrush?` | 过滤命中高亮前景色。 |
| `AutoScrollToSelectedItem` | `bool` | 候选树打开或同步时滚动到选中节点。 |
| `IsShowOverflowTip` | `bool` | 单选结果文本或多选 tag 视觉溢出时是否显示完整内容 tooltip，默认 `true`。 |
| `OverflowTipDelay` | `int` | 溢出 tooltip 打开前的延迟时间，单位毫秒，默认 `1200`。 |
| `OverflowTipPlacement` | `PlacementMode` | 溢出 tooltip 相对单选结果文本或多选 tag 的位置，默认 `TopEdgeAlignedLeft`。 |
| `IsShowIcon` / `IsShowLeafIcon` | `bool` | 是否显示节点图标和叶子节点图标。 |
| `IsShowLine` / `IsShowTreeLine` | `bool` | 是否显示层级连线。 |
| `IsSwitcherRotation` | `bool` | 展开图标是否旋转。 |

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_InputControlFrame` | `TreeSelectAddOnDecoratedBox` | `InputControlFrame` / AddOnDecoratedBox 组合、Addon、EffectiveStatus、CompactSpace 和多选布局承载。 |
| `PART_SingleFilterInput` | `SelectFilterTextBox` | 单选模式结果显示和搜索输入。 |
| `SelectedItemsBox` | `SelectTagAwareTextBox` | 多选和勾选模式已选 tag 展示。 |
| `PART_SelectMaxCountIndicator` | `SelectMaxCountIndicator` | 最大选择数量提示。 |
| `PART_ContentRightAddOnPresenter` | `ContentPresenter` | 用户内部右侧内容承载。 |
| `PART_SelectHandle` | `SelectHandle` | 展开、loading、清除和 Form feedback 图标入口。 |
| `PART_Popup` | `Popup` | 候选弹层宿主。 |
| `PopupFrame` | `Border` | 懒创建的候选弹层外壳。 |
| `PART_TreeView` | `TreeSelectTreeView` | 懒创建的树候选控件。 |

## 4. 行为与状态模型

TreeSelect 的核心状态流：

```text
ItemsSource / Items
      ↓
TreeSelectTreeView
      ↓
SelectedItem / SelectedItems
      ↓
SingleFilterInput / SelectedItemsBox
      ↓
Form value + MaxCount state
```

选择模式语义：

- 单选模式使用 `SelectedItem` 作为表单值，候选节点点击后关闭弹层。
- 多选模式使用 `SelectedItems` 作为表单值，候选树使用多选 selection，并通过 tag 展示结果。
- `IsTreeCheckable=true` 使用 checkbox 作为节点切换入口，TreeView selection 不再作为主要选择入口。
- `ShowCheckedStrategy` 只影响多选 tag 展示集合，不改变 `SelectedItems` 的真实值。
- `SelectedItems` 是用户拥有的受控集合。集合引用替换和 `ObservableCollection` 等 `INotifyCollectionChanged` 原地 `Add`、`Remove`、`Reset` 都必须刷新 tag、`SelectedCount`、Form value changed、候选树 selection / checked items 和最大选择数状态。
- 弹层 TreeView 因 `ItemsSource` 换源而清理、回放选中或勾选状态时，TreeSelect 只响应最终变化，不把中间状态重复上报给 Form；已关闭的弹层树不作为选择或勾选输入来源。

过滤行为：

- `FilterValue` 来自单选搜索输入。
- `FilterStrategy` 控制命中高亮、加粗、展开路径和隐藏不匹配节点。
- 弹层关闭后，单选模式清空过滤值并恢复结果显示。

清除行为通过 `SelectHandle.ClearRequestedEvent` 冒泡到 TreeSelect，并调用 `Clear()` 清空当前选择。

## 5. 视觉与主题模型

TreeSelect 的默认视觉由 TreeSelect 专属主题、`InputControlFrame` / Select 家族 AddOnDecoratedBox、TreeView 和 PopupHost 协作完成。

| 主题或资源 | 职责 |
| --- | --- |
| `TreeSelectTheme.axaml` | 根模板、输入壳体、单选输入、多选结果区域、handle、popup 和基础 selector。 |
| `SelectTagAwareTextBox` / `SelectTag` 主题 | 多选 tag 布局、高度、关闭按钮和禁用态。 |
| `TreeView` / `TreeViewItem` 主题 | 树节点缩进、展开、勾选、图标、连线和过滤视觉。 |
| `InputControlFrameTheme` / `SharedToken` | 输入壳体边框、圆角、effective status、focus 和 CompactSpace 视觉。 |
| `PopupHostToken` | popup 阴影、圆角和 anchor margin。 |
| `TreeSelectToken` | TreeSelect 候选弹层最小宽度。 |
| `SelectToken` | TreeSelect 复用的 popup padding、多选 tag 和输入内容 padding。 |

单选结果文本和多选 tag 的完整内容提示复用共享 `OverflowTip` attached behavior。主题通过 `IsShowOverflowTip`、`OverflowTipDelay` 和 `OverflowTipPlacement` 控制提示开关、延迟和位置，实际 tooltip 仅在文本视觉溢出时托管到 `ToolTip`。

右侧 count、content add-on 和 handle 的稳定 template part 状态由 AXAML compiled ancestor binding 表达。C# 中只保留 frame layout part 到 SelectHandle 的 hover / pressed sibling 状态转发，因为该关系不是 templated parent 绑定，不能用 `TemplateBinding` 表达；该转发不改变 `InputControlFrame` 对输入表面状态的唯一 ownership。

## 6. 控件家族或集成关系

TreeSelect 属于 Data Entry 选择控件家族，与 Select、Cascader、DatePicker、TimePicker 等共享输入尺寸、variant、status、Addon、Form 和 CompactSpace 语义。

集成关系：

- `AbstractSelect`：Select 家族输入壳体、弹层、Form、CompactSpace、Motion 和状态契约。
- `TreeView`：树节点容器、展开、勾选、过滤和异步加载基础能力。
- `TreeSelectTreeView`：TreeSelect 专用 TreeView，负责最大选择数状态下的节点容器协作。
- `TreeViewSelectTreeViewItem`：候选树容器，接收最大选择数状态。
- `SelectHandle`：右侧操作入口，负责展开指示、loading、清除和 Form feedback。
- `IFormItemAware` / `IFormItemFeedbackAware`：将选择值、Form 扩展状态和 feedback 接入 Form；error 由 `DataValidationErrors` 投射到 `InputControlFrame`。
- `ICustomizableSizeTypeAware`：接入支持 `Custom` 的输入尺寸模型。

## 7. 兼容性不变量

维护 TreeSelect 时必须保持以下不变量：

- 单选模式使用 `SelectedItem`，多选和勾选模式使用 `SelectedItems`。
- `SelectedItem` 和 `SelectedItems` 必须保持默认双向绑定，并通过 Avalonia `DataValidationErrors` 承接 binding / Form error。
- `SelectedItems` 原地变更必须与集合替换走同一套展示、Form 和候选树同步路径，不能依赖用户重新赋值。
- `IsTreeCheckable=true` 必须继续把 TreeSelect 归入多选结果模型。
- `ShowCheckedStrategy` 只能影响 `EffectiveSelectedItems`，不能改写真实 `SelectedItems`。
- `ItemsSource` 变化必须尽量按节点路径 identity 保留已有选择。
- `Clear()` 必须同时清空单选和多选状态。
- Popup 打开、关闭和 light-dismiss 语义必须继续由 `AbstractSelect` 管理。
- 重新套用模板或 detach 时必须释放旧 popup 内容、TreeView 事件订阅和模板 part relay binding。
- 右侧稳定 template part 优先使用 AXAML binding，不能把可静态表达的绑定重新搬回 C#。
- `SizeType=Custom` 必须继续以 `Middle` 作为未显式覆盖时的默认视觉基线。
- Template part、Token 名称、伪类和主题 selector 不得在未授权情况下重命名或删除。

## 8. 专项模型

### 8.1 勾选展示策略

`TreeSelectCheckedStrategy` 是多选结果展示策略：

- `All`：展示所有已选节点。
- `ShowParent`：当父节点下所有子节点都被选中时，结果展示父节点，隐藏对应子孙节点。
- `ShowChild`：只展示叶子节点。

该策略作用于内部 `EffectiveSelectedItems`，最终由 `SelectedItemsBox` 展示，不改变 `SelectedItems` 的表单值。

### 8.2 路径 identity

TreeSelect 使用节点的 `ItemKey` 或 `Value` 组成 `TreeNodePath`，用于 `ItemsSource` 替换后的选择重映射。节点缺少稳定 identity 时，控件无法可靠地跨数据源替换保留选择。

## 9. 文档导航与验证策略

- [TreeSelect 桌面版实现原理](implementation.md)
- [TreeSelect Semantic Part 契约](semantic-part.md)
- [TreeSelect Token 设计](token.md)
- [TreeSelect Changelog](changelog.md)

验证要求：

| 改动类型 | 验证要求 |
| --- | --- |
| Public API 或 Form 映射 | 验证单选、多选、勾选和 `Clear()` 行为。 |
| 选择同步 | 验证 TreeView selection / checked items 与 `SelectedItem` / `SelectedItems` 双向同步，并覆盖 `SelectedItems` 集合替换和原地变更。 |
| 过滤行为 | 验证单选搜索输入、过滤策略、弹层关闭恢复和 placeholder 状态。 |
| AXAML 或 template part | 验证右侧 AddOn/count/handle binding、hover/pressed relay、popup 内容和 Gallery 示例。 |
| Token 或尺寸 | 验证 Large / Middle / Small / Custom、tag 高度、popup padding 和最小宽度。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |
