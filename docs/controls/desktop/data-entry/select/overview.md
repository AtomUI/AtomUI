# Select 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.Select` 的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Select 桌面版实现原理](implementation.md)，鼠标与键盘的统一候选状态见 [Select 候选交互设计](candidate-interaction-design.md)，Select Token 的专项设计见 [Select Token 设计](token.md)，设计和契约变化记录见 [Select Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `AbstractSelect`，其 internal `IsPopupPinnedOpen` 只供测试和内部诊断使用；设置为 true 时保持 `IsDropDownOpen` 并 relay 到 `PART_Popup`，设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

Select 公开 13 个 Semantic Part（`root`、`prefix`、`content`、`placeholder`、`input`、`suffix`、`clear`、`item`、`itemContent`、`itemRemove`、`popup.root`、`popup.list`、`popup.listItem`），与 Ant Design Select 的 Semantic Part 语义对齐；完整 Part 表、Selector 用法与定制边界见 [Select Semantic Part 契约](semantic-part.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/Select` |
| 控件状态 | Stable |

Select 是 AtomUI 桌面数据录入体系中的选项选择控件，用于在受控选项集合中完成单选、多选和标签化选择。它由输入壳体、已选结果区域、过滤输入、候选弹层、候选列表、异步加载和 Form / CompactSpace 集成组成，面向表单、筛选器、配置项和可搜索选项输入场景。

Select 的职责是选择一个或多个 `ISelectOption`，或在 `Tags` 模式下基于用户输入临时创建动态选项。它不负责远程服务协议、权限过滤、业务对象持久化、复杂树形选择、级联选择或富文本标签编辑；这些能力应由业务层或 TreeSelect / Cascader 等专用控件承担。

## 2. 设计语言

Select 的设计语言来自输入框、已选结果和候选弹层的组合。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 选择模式 | 控件选择单个选项、多个选项或允许用户创建标签。 | `Mode=Single/Multiple/Tags`。 |
| 选项数据 | 候选项可来自本地集合、内容子项或异步 loader。 | `OptionsSource`、`Options`、`OptionsLoader`。 |
| 选择结果 | 单选结果显示为文本，多选和 tags 显示为可关闭标签。 | `SelectedOption`、`SelectedOptions`、`MaxTagCount`。 |
| 搜索过滤 | 输入搜索文本后过滤候选项。 | `IsFilterEnabled`、`FilterValue`、`Filter`、`FilterValueSelector`。 |
| 输入表面 | 控件边框、背景、尺寸、状态和附加内容。 | `StyleVariant`、`Status`、`SizeType`、Addon。 |
| 弹层语义 | 候选列表按 placement 打开，并可匹配控件宽度。 | `IsDropDownOpen`、`Placement`、`IsPopupMatchSelectWidth`。 |
| 异步加载 | 打开控件时按上下文加载候选数据。 | `OptionsLoader`、`OptionsAsyncLoadContext`、`IsLoading`。 |

`Custom` 尺寸不是 Select 的第四套专属 Token。主题层把 `SizeType=Custom` 归入 `Middle` 的默认字体和 padding 分支；用户显式设置尺寸属性时由 Avalonia 属性优先级决定最终布局。

## 3. API 与契约模型

Select 的公共 API 分布在 `AbstractSelect` 和 `Select` 两层。`AbstractSelect` 提供输入壳体、弹层、清除、状态、尺寸、Addon、Form 和 CompactSpace 契约；`Select` 提供选项、选择、过滤、分组、异步加载和模式契约。

选项与选择 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `Mode` | `SelectMode` | 选择模式，默认 `Single`。 |
| `OptionsSource` | `IEnumerable<ISelectOption>?` | 外部候选项集合。Select 只读取该集合，不把 Tags 运行时动态选项写回该集合。 |
| `Options` | `ItemCollection` | XAML 内容子项入口。它表达用户声明的静态候选项，不承载 Tags 模式运行时动态选项。 |
| `OptionTemplate` | `IDataTemplate?` | 候选项显示模板，默认显示 `ISelectOption.Header`。 |
| `SelectedOption` | `ISelectOption?` | 单选模式当前选项，默认 `TwoWay` 绑定并启用 Avalonia data validation。 |
| `SelectedOptions` | `IList<ISelectOption>?` | 多选和 Tags 模式当前选项集合，默认 `TwoWay` 绑定并启用 Avalonia data validation。 |
| `DefaultValues` | `IList<object>?` | 加载后按值匹配默认选中项。 |
| `DefaultValueCompareFn` | `Func<object, ISelectOption, bool>?` | 默认值匹配自定义比较函数。 |
| `SelectionChanged` | event | `SelectedOption` 或 `SelectedOptions` 改变时触发。 |

过滤与列表 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `IsFilterEnabled` | `bool` | 是否启用搜索过滤。`Tags` 模式始终形成有效过滤能力。 |
| `FilterValue` | `object?` | 当前过滤输入值。 |
| `Filter` | `IValueFilter?` | 候选项过滤器。为空时使用 ListView 默认过滤行为。 |
| `FilterValueSelector` | `DefaultFilterValueSelector?` | 从候选项中提取过滤文本。默认使用 `HeaderFilterPropertySelector`。 |
| `IsGroupEnabled` | `bool` | 是否启用候选项分组。 |
| `GroupPropertySelector` | `DefaultFilterValueSelector?` | 分组值 selector。 |
| `IsDefaultActiveFirstOption` | `bool` | 当前 public surface 的一部分；维护时需要先确认候选激活路径再改变其语义。 |
| `IsHideSelectedOptions` | `bool` | 多选候选列表中隐藏已选项。 |
| `AutoScrollToSelectedOptions` | `bool` | 候选列表打开或选择同步时滚动到已选项。 |
| `DisplayPageSize` | `int` | 候选弹层可视行数，用于计算最大高度，默认 `10`。 |
| `MaxCount` | `int` | 多选最大可选数量，默认 `int.MaxValue`。 |

选择绑定语义：

- `SelectedOption` 是 `Mode=Single` 的唯一选择状态入口；用户选择、清除、Form 写值和 ViewModel 写值都通过该属性同步。
- `SelectedOptions` 是 `Mode=Multiple/Tags` 的唯一选择状态入口；用户选择变化会回写绑定源，绑定源替换集合或对 `INotifyCollectionChanged` 集合执行 `Add` / `Remove` / `Reset` 时，Select 会同步标签、候选列表和计数状态。
- `OptionsSource` 始终只是候选项来源。即使 `SelectedOptions` 绑定到可变集合，Tags 模式运行时动态选项也只进入选择集合和内部有效候选集合，不写回 `OptionsSource`。

弹层与异步 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `IsDefaultOpen` | `bool` | 当前 public surface 的一部分；默认打开行为需与 `IsDropDownOpen` 状态保持兼容。 |
| `IsDropDownOpen` | `bool` | 候选弹层打开状态。 |
| `Placement` | `SelectPopupPlacement` | 候选弹层展开位置，默认 `BottomEdgeAlignedLeft`。 |
| `IsPopupMatchSelectWidth` | `bool` | 弹层最小宽度是否匹配 Select 宽度，默认 `true`。 |
| `ShouldUseOverlayPopup` | `bool` | 是否使用 overlay popup 宿主，默认 `true`。 |
| `OptionsLoader` | `ISelectOptionsAsyncLoader?` | 异步候选加载器。 |
| `OptionsAsyncLoadContext` | `object?` | 传给异步 loader 的上下文。 |
| `AsyncLoadTimeout` | `TimeSpan` | 异步加载超时时间，默认 `10` 秒。 |
| `IsLoading` | `bool` | 异步加载或 loading 状态，只读公开。 |
| `OptionsLoading` / `OptionsLoaded` | event | 开始加载和加载完成通知。 |
| `DropDownOpening` / `DropDownClosing` | event | 弹层打开/关闭前通知，可取消。 |
| `DropDownOpened` / `DropDownClosed` | event | 弹层打开/关闭后通知。 |

输入表面 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `SizeType` | `CustomizableSizeType` | 输入尺寸密度，支持 `Large/Middle/Small/Custom`。 |
| `StyleVariant` | `InputControlStyleVariant` | 输入表面样式。 |
| `Status` | `InputControlStatus` | 显式输入反馈状态；最终视觉由 `InputControlFrame.EffectiveStatus` 计算，native validation error 以 `DataValidationErrors` 为唯一真源。 |
| `PlaceholderText` / `PlaceholderForeground` | `string?` / `IBrush?` | 空选择时的占位文本和颜色。 |
| `IsAllowClear` / `ClearIcon` | `bool` / `PathIcon?` | 清除入口和图标。 |
| `SuffixIcon` / `SuffixLoadingIcon` | `PathIcon?` | 普通展开指示和 loading 指示。 |
| `LeftAddOn` / `RightAddOn` | `object?` | 外部左右 AddOn。 |
| `ContentLeftAddOn` / `ContentRightAddOn` | `object?` | 内部左右内容。 |
| `IsMotionEnabled` | `bool` | 输入壳体、handle、候选列表和 popup 动效开关。 |
| `IsShowOverflowTip` | `bool` | 选中结果文本或多选 tag 视觉溢出时是否显示完整内容 tooltip，默认 `true`。 |
| `OverflowTipDelay` | `int` | 溢出 tooltip 打开前的延迟时间，单位毫秒，默认 `1200`。 |
| `OverflowTipPlacement` | `PlacementMode` | 溢出 tooltip 相对选中结果文本或多选 tag 的位置，默认 `TopEdgeAlignedLeft`。 |

多选标签 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `MaxTagCount` | `int?` | 最大直接展示标签数量。 |
| `IsResponsiveTagMode` | `bool` | 使用响应式标签布局。 |
| `MaxTagPlaceholder` | `string?` | 当前 public surface 的一部分；默认模板使用剩余数量标签表达折叠信息。 |
| `IsShowMaxCountIndicator` | `bool` | 是否展示最大数量指示。 |

候选项契约：

| 类型 | 语义 |
| --- | --- |
| `ISelectOption.Header` | 候选项显示内容。 |
| `ListItemData.Content` | 候选项值和默认值匹配的主要输入。 |
| `ListItemData.ItemKey` | 候选项稳定标识，优先用于选项替换后的选择映射。 |
| `ListItemData.IsEnabled` | 候选项启用状态。 |
| `ISelectOption.IsDynamicAdded` | Tags 模式下用户输入生成的运行时动态选项标记。 |

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_InputControlFrame` | `SelectAddOnDecoratedBox` | `InputControlFrame` / AddOnDecoratedBox 组合、Addon、EffectiveStatus、CompactSpace 和 hover/pressed 状态承载；选择专用节点只扩展内容布局。 |
| `PART_SingleFilterInput` | `SelectFilterTextBox` | 单选模式结果显示和搜索输入。 |
| `SelectedOptionsBox` | `SelectResultOptionsBox` | 多选和 Tags 模式已选标签与搜索输入。 |
| `PART_SelectMaxCountIndicator` | `SelectMaxCountIndicator` | 最大选择数量提示。 |
| `PART_ContentRightAddOnPresenter` | `ContentPresenter` | 用户内部右侧内容承载。 |
| `PART_SelectHandle` | `SelectHandle` | 展开、loading、清除和 Form feedback 图标入口。 |
| `PART_Popup` | `Popup` | 候选弹层宿主。 |
| `PopupFrame` | `Border` | 懒创建的候选弹层外壳。 |
| `PART_CandidateList` | `SelectCandidateList` | 懒创建的候选项列表、键盘导航和提交/取消。 |

Select 的稳定伪类包括 `:dropdownopen`；输入表面通过 `InputControlFrame` 统一表达 `StyleVariant`、`EffectiveStatus`、`:pressed`、`:disabled` 和 CompactSpace，选择专用伪类只表达 popup、候选和结果状态。

## 4. 行为与状态模型

Select 的核心状态流：

```text
OptionsSource / Options / OptionsLoader
      ↓
用户选项源
      ↓
Effective options = 用户选项源 + Tags 运行时动态选项
      ↓
SelectCandidateList
      ↓
SelectedOption / SelectedOptions
      ↓
SingleFilterInput / SelectedOptionsBox
      ↓
Form value + SelectionChanged
```

选择模式语义：

- `Single` 使用 `SelectedOption` 作为唯一表单值，内部单行过滤输入负责展示当前 `Header`。
- `Multiple` 使用 `SelectedOptions` 作为表单值，已选项以 `SelectTag` 展示。
- `Tags` 以 `Multiple` 为基础，始终启用有效过滤，并在过滤结果为空且输入非空时创建 `IsDynamicAdded=true` 的运行时动态选项。

弹层交互优先级：

```text
Disabled / invisible / window deactivated
> DropDownClosing cancellation
> DropDownOpening cancellation
> IsDropDownOpen
> pointer / keyboard open request
```

键盘行为：

- 弹层打开时按键优先交给 `SelectCandidateList.HandleKeyDown()`。
- `Enter` 在单选模式提交候选项，在多选模式切换候选项选中状态。
- `Escape` 取消候选并关闭弹层。
- `F4` 或 `Alt+Up/Down` 切换弹层。
- 弹层关闭时，`Up/Down`、`Enter`、`Space` 可打开弹层。
- 多选和 Tags 模式下，过滤输入为空时 `Backspace/Delete` 删除最后一个已选项。

候选交互：

- `SelectCandidateList` 维护唯一 active candidate，鼠标移动和键盘导航必须更新同一个候选状态。
- 鼠标在可用候选项上实际移动时，该项成为 active candidate；鼠标路径不滚动候选列表，也不提交公共选择。
- `Up/Down` 在当前有效候选视图中移动 active candidate，并可以把键盘候选滚动到可见区域。
- `Enter` 只提交或切换当前 active candidate，候选视觉目标和提交目标必须一致。
- active candidate 与已确认选择相互独立；已选项继续保持 selected 视觉和公共选择语义。
- `:pointerover` 只表达指针命中事实，不能与 `IsCandidateSelected` 分别绘制两个候选高亮。

完整状态、Theme、虚拟化、性能和验证边界见 [Select 候选交互设计](candidate-interaction-design.md)。

清除行为通过 `SelectHandle.ClearRequestedEvent` 冒泡到 Select，并调用 `ClearValue()` 清空 `SelectedOption` 和 `SelectedOptions`。

## 5. 视觉与主题模型

Select 的默认视觉由 Select 专属主题、`InputControlFrame` / AddOnDecoratedBox、ListView 和 PopupHost 协作完成。

| 主题或资源 | 职责 |
| --- | --- |
| `SelectTheme.axaml` | 根模板、输入壳体、单选输入、多选结果区域、handle、popup 和基础 selector。 |
| `InputControlFrameTheme.axaml` | 输入框 variant、effective status、hover、pressed、disabled、CompactSpace 和 motion。 |
| `SelectAddOnDecoratedBoxTheme.axaml` | dropdown open、多选 padding、handle 和选择结果布局；不重复实现 frame 状态 selector。 |
| `SelectResultOptionsBoxTheme.axaml` | 多选标签布局、响应式标签布局和搜索输入承载。 |
| `SelectCandidateListTheme.axaml` | 候选列表基础 ListView 主题和默认候选模板。 |
| `SelectCandidateListItemTheme.axaml` | 候选项 active、selected、disabled 和隐藏已选项视觉；active 只由统一候选状态驱动。 |
| `SelectTagTheme.axaml` | 多选标签高度、背景、关闭按钮和禁用态。 |
| `SelectHandleTheme.axaml` | 展开、loading、清除、过滤指示和 Form feedback 图标。 |
| `PopupHostToken` | popup 阴影、圆角和 anchor margin。 |
| `SelectToken` | 多选标签、候选项、popup padding 和输入 padding。 |

选中结果的完整内容提示复用 `OverflowTip` attached behavior。模板只在单选文本和多选 tag 上声明 `IsShowOverflowTip` / `OverflowTipDelay` / `OverflowTipPlacement`，实际 tooltip 只在文本视觉宽度不足时写入 `ToolTip.Tip`，且不会覆盖用户手动设置的 tooltip。

候选弹层内容采用懒创建模型。`PART_Popup` 属于模板稳定 part；`PopupFrame` 和 `PART_CandidateList` 在打开前由 C# 创建并设置 `TemplatedParent`，关闭或重新套用模板时释放引用和事件订阅。

## 6. 控件家族或集成关系

Select 属于 Data Entry 选择控件家族，与 LineEdit、NumericUpDown、DatePicker、TimePicker、TreeSelect 等共享输入尺寸、variant、status、Addon、Form 和 CompactSpace 语义。

集成关系：

- `AbstractSelect`：Select 家族选择、弹层、Form、CompactSpace、Motion 和业务状态契约，并把输入状态投射到 shared frame。
- `SelectCandidateList`：基于 `ListView` 的候选列表，负责过滤、分组、虚拟化、候选导航和提交取消。
- `SelectResultOptionsBox`：多选和 Tags 已选结果展示。
- `SelectHandle`：右侧操作入口，负责展开指示、loading、清除和 Form feedback。
- `ISelectOptionsAsyncLoader`：异步候选加载边界。
- `IFormItemAware` / `IFormItemFeedbackAware`：将选择值、Form 扩展状态和 feedback 接入 Form；error 由 `DataValidationErrors` 投射到 `InputControlFrame`。
- `ICustomizableSizeTypeAware`：接入支持 `Custom` 的输入尺寸模型。

## 7. 兼容性不变量

维护 Select 时必须保持以下不变量：

- `Mode=Single` 使用 `SelectedOption`，`Mode=Multiple/Tags` 使用 `SelectedOptions`。
- `SelectedOption` 与 `SelectedOptions` 必须保持默认 `TwoWay` 绑定；`SelectedOptions` 绑定到 `INotifyCollectionChanged` 集合时，集合原地变化也必须刷新内部选择投影。
- `SelectionChanged` 必须在选择属性变化时继续触发，并包含模式、旧值和新值。
- `OptionsSource`、`Options` 和异步加载结果表达用户选项源；Tags 模式运行时动态选项不得写入这些用户选项源。
- 候选列表必须使用用户选项源和 Tags 运行时动态选项合成后的有效选项源。
- `OptionsSource` 变化必须按 `ItemKey` 优先、`Content` 兜底映射已有选择；已选 Tags 动态选项在没有正式选项可映射时必须保留。
- `DefaultValues` 只在当前选择为空时应用。
- `Tags` 模式必须保持有效过滤能力，并只在该模式下创建动态选项。
- `MaxCount` 达到上限时，未选候选项不可继续选择，已选候选项仍可取消。
- `IsHideSelectedOptions` 不能隐藏分组标题导致空状态判断错误。
- 鼠标和键盘必须共享唯一 active candidate，任意时刻最多显示一个未确认候选高亮。
- `:pointerover` 不能独立成为 Select 候选视觉或提交状态 owner。
- active candidate 变化不能提前修改 `SelectedOption` 或 `SelectedOptions`，`Enter` 必须提交当前视觉候选。
- 弹层打开、关闭事件的取消语义不能被绕过。
- 窗口失活、控件不可见或祖先不可见时必须关闭弹层。
- 重新套用模板或 detach 时必须释放旧 popup 内容、候选列表事件订阅、opened 期间订阅和 TopLevel deactivation 订阅。
- `SizeType=Custom` 必须继续以 `Middle` 作为未显式覆盖时的默认视觉基线。
- Template part、Token 名称、伪类和主题 selector 不得在未授权情况下重命名或删除。

如果实现某项能力时无法保持这些不变量，应先停止实现，说明原因、影响范围、替代方案和迁移方式，并获得授权。

## 8. 专项模型

### 8.1 选择模式模型

`SelectMode` 定义三种模式：

- `Single`：候选列表为单选，提交后关闭弹层，结果显示在 `PART_SingleFilterInput`。
- `Multiple`：候选列表为多选，结果显示为 `SelectTag` 集合，提交和关闭相互独立。
- `Tags`：多选基础上允许把过滤输入创建为运行时动态选项。动态选项属于 Select 内部状态，不属于用户 `Options` / `OptionsSource`。

### 8.2 过滤与 Tags 动态选项模型

`FilterValue` 来自单选过滤输入或多选结果区域内的搜索输入。`Tags` 模式在候选总数为 0 且过滤文本非空时创建运行时动态选项，该选项的 `Header` 和 `Content` 都等于输入文本，并标记为 `IsDynamicAdded=true`。

Select 必须保持三层选项模型：

```text
用户选项源 = Options / OptionsSource / OptionsLoader result
运行时动态选项 = Tags 模式由输入创建的 IsDynamicAdded 选项
有效候选选项 = 用户选项源 + 运行时动态选项
```

候选列表只消费有效候选选项。动态选项的创建、选择、删除和清理只能影响运行时动态选项集合及有效候选选项，不能修改用户传入的 `OptionsSource` 或 XAML 内容子项 `Options`。当新的用户选项源中出现与已选动态选项相同 identity 的正式选项时，Select 可以把选择映射到正式选项，并清理对应动态选项，避免候选项重复。

### 8.3 异步加载模型

打开下拉时，如果存在 `OptionsLoader` 且当前实例还没有成功加载过候选，Select 调用 loader。成功结果写入 `OptionsSource` 并刷新弹层；超时、取消和未知错误通过 `OptionsLoaded` 返回状态码。

### 8.4 Custom SizeType 模型

`SizeType=Custom` 共享 `Middle` 字体和 padding 默认分支。该语义必须同时传递给 `SelectAddOnDecoratedBox`、`SelectFilterTextBox`、`SelectResultOptionsBox` 和 `SelectTag`，保证单选、多选和 Tags 模式尺寸基线一致。

## 9. 文档导航与验证策略

关联文档：

- [Select 桌面版实现原理](implementation.md)
- [Select 候选交互设计](candidate-interaction-design.md)
- [Select Semantic Part 契约](semantic-part.md)
- [Select Token 设计](token.md)
- [Select Changelog](changelog.md)

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| Public API | `Mode`、选项源、选择值、过滤、异步加载、弹层事件、Form 和 CompactSpace 语义。 |
| 状态行为 | 单选、多选、Tags、清除、统一 active candidate、鼠标/键盘切换、最大选择数、隐藏已选项、默认值映射和动态选项清理。 |
| AXAML / Template | 稳定 template part、`:dropdownopen`、AddOnDecoratedBox selector、PopupFrame 和懒创建候选列表。 |
| Token | 多选标签、候选项、popup padding、输入 padding 和 `Custom` 尺寸默认分支。 |
| Gallery / Docs | Select API 摘要、Token 语义、ShowCase 示例和本目录文档链接。 |
