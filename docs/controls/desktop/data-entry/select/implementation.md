# Select 桌面版实现原理

本文档描述 Select 桌面版的输入壳体、选项集合、选择同步、过滤、Tags 动态选项、候选弹层、异步加载、Form 和 Token 资源边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计与 API 契约见 [Select 桌面版架构设计](overview.md)，鼠标与键盘的统一候选状态见 [Select 候选交互设计](candidate-interaction-design.md)，Token 语义见 [Select Token 设计](token.md)，变化记录见 [Select Changelog](changelog.md)。

Popup 接入边界：`AbstractSelect` 负责业务状态和内容准备，`PART_Popup` 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

Select 的实现以 `AbstractSelect` 为选择与弹层基类，输入表面复用 `InputControlFrame` / `AddOnDecoratedBox`，`Select` 本体负责选择业务状态，候选列表和结果区域由内部控件承载。实现文档聚焦 Select 自身的状态编排、生命周期和维护边界，不重新说明 ListView 的通用虚拟化、shared frame 的输入外观或 PopupHost 的全局资源规则。

Tags 模式的动态选项必须作为 Select 自身运行时状态维护。用户声明的 `Options`、绑定的 `OptionsSource` 和异步加载结果都属于用户选项源，Select 不得为了创建自定义 tag 写入这些源集合。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Select/AbstractSelect.cs`：输入壳体、弹层状态、公共输入属性、Form / CompactSpace / Motion 接口和 popup 生命周期。
- `src/AtomUI.Desktop.Controls/Select/Select.cs`：public Select API、protected 扩展 hook、用户选项源同步、有效候选选项同步、选择同步、过滤输入、Tags 动态选项、键盘和指针处理。
- `src/AtomUI.Desktop.Controls/Select/Select.SemanticParts.cs`：Select 的 13 个 Semantic Part 声明（见 [Select Semantic Part 契约](semantic-part.md)）。
- `src/AtomUI.Desktop.Controls/Select/Select.AsyncOptionsLoad.cs`：异步候选加载和私有加载完成流程。
- `src/AtomUI.Desktop.Controls/Select/SelectOption.cs`：`ISelectOption` 和默认 `SelectOption`。
- `src/AtomUI.Desktop.Controls/Select/SelectCandidateList.cs`：统一 active candidate、鼠标与键盘候选导航、提交取消、最大选择数和隐藏已选项。
- `src/AtomUI.Desktop.Controls/Select/SelectCandidateListItem.cs`：active candidate 和 committed selection 的容器状态投影。
- `src/AtomUI.Desktop.Controls/Select/SelectResultOptionsBox.cs`：多选结果标签和过滤输入承载。
- `src/AtomUI.Desktop.Controls/Select/SelectHandle.cs`：右侧展开、loading、清除和 Form feedback 图标。
- `src/AtomUI.Desktop.Controls/Tooltip/OverflowTip.cs`：共享溢出 tooltip attached behavior，供单选结果和多选 tag 复用。
- `src/AtomUI.Desktop.Controls/Select/DataLoad/*`：异步候选加载接口、结果和事件参数。
- `src/AtomUI.Desktop.Controls/Select/SelectToken.cs`：Select 控件 Token。
- `src/AtomUI.Desktop.Controls/Select/Themes/*.axaml`：Select 根模板、候选列表、结果标签、handle、输入壳体和 token 样式。

## 3. 核心类职责

`AbstractSelect` 是 Select 家族的选择与弹层边界。它维护 `IsDropDownOpen`、popup placement、弹层打开/关闭事件、打开期间可见性订阅、TopLevel 失活订阅、Form 状态映射和 shared frame 的 CompactSpace 投射；它不重新计算输入表面有效状态。

`Select` 是选项选择协调器。它维护用户选项源、运行时动态选项、有效候选选项、`SelectedOption`、`SelectedOptions`、`Mode`、过滤值、默认值映射和候选列表同步。它不直接绘制候选项或标签，而是把状态传给内部控件。

`SelectCandidateList` 是候选选择边界。它继承 `ListView`，拥有唯一 active candidate，负责把鼠标移动和键盘导航映射到同一候选状态，并处理过滤、分组、虚拟化容器、最大选择数禁用和 `Commit/Cancel` 事件。完整交互不变量见 [Select 候选交互设计](candidate-interaction-design.md)。

`SelectResultOptionsBox` 是多选结果边界。它根据 `SelectedOptions` 创建 `SelectTag`，并在过滤启用时插入 `SelectFilterTextBox`。

`OverflowTip` 是跨选择类控件复用的显示增强边界。Select 模板把 `IsShowOverflowTip`、`OverflowTipDelay`、`OverflowTipPlacement` 和当前展示文本传给该 attached behavior；单选显示节点以外层 `SelectAddOnDecoratedBox` 作为 `PlacementTarget`，避免 tooltip 左边按内部文本 padding 对齐；`SelectResultOptionsBox` 动态创建 tag 时把同一组设置绑定到每个 `SelectTag`。它只在视觉溢出时托管自己写入的 `ToolTip.Tip`，用户显式设置的 tooltip 不会被覆盖或清理。

`SelectHandle` 是右侧操作边界。它根据 loading、过滤、展开状态选择当前图标，并把清除按钮点击转换为 `ClearRequestedEvent`。

## 4. 状态与数据流

初始化流：

```text
Select()
  → RegisterTokenResourceScope(SelectToken)
OnInitialized()
  → if FilterValueSelector == null, use HeaderFilterPropertySelector
OnLoaded()
  → ConfigureDefaultValues()
```

选项源流：

```text
OptionsSource changed
  → remember current selection identity
  → update user option source
  → rebuild effective options from user option source + runtime tag options
  → remap selected option by ItemKey or Content, preserving runtime tag options when no user option matches
  → if no remap, ConfigureDefaultValues()
```

选择同步流：

```text
CandidateList.Commit / SelectionChanged
  → SelectedOption or SelectedOptions
  → NotifyFormValueChanged
  → SelectionChanged event
  → sync CandidateList.SelectedItem / SelectedItems
  → update SelectedCount / placeholder / result visible state
```

候选交互流：

```text
PointerMoved / Up / Down
  → resolve enabled non-group candidate
  → update CandidateSelectedIndex + CandidateSelectedItem
  → project IsCandidateSelected to realized container
  → SelectCandidateListItemTheme active visual
Enter / pointer click
  → commit or toggle the same interaction target
  → SelectedOption / SelectedOptions
```

active candidate 是提交前交互状态，不写入 `ISelectOption`，也不提前改变 public selection。鼠标路径只迁移候选，不滚动列表；键盘路径在共享状态写入完成后把候选滚动到可见区域。

过滤流：

```text
SelectFilterTextBox.TextChanged
  → FilterValue = trimmed text or null
  → CandidateList filters by Filter + FilterValue + FilterValueSelector
  → Tags mode may create runtime dynamic option
  → rebuild effective options
  → Dispatcher.Post(SyncSelectionToCandidateList)
```

Form 流：

```text
Form.SetValue(object?)
  → Single: SelectedOption
  → Multiple/Tags: SelectedOptions
Form.GetValue()
  → Single: SelectedOption
  → Multiple/Tags: SelectedOptions
Form.ClearValue()
  → clear current selection
DataValidationErrors
  → InputControlFrame.EffectiveStatus
ValidateStatus
  → FormStatus → InputControlFrame + feedback
FeedbackControl
  → FormFeedback → SelectHandle
```

## 5. 生命周期与模板接入

`AbstractSelect.OnApplyTemplate()` 负责稳定模板 part 接入：

- 重新计算 popup 最大高度。
- 解除旧 `Popup.Opened` / `Popup.Closed` 订阅。
- 获取新的 `PART_Popup` 和 `PART_InputControlFrame`（具体类型为 `SelectAddOnDecoratedBox`）。
- 订阅新 popup 的 opened / closed。

`Select.OnApplyTemplate()` 在基类接入前调用 `ClearPopupContent()`，确保旧候选列表、popup frame 和事件订阅被释放。基类接入后，Select 获取 `PART_SingleFilterInput`，设置 `Popup.OverlayInputPassThroughElement`，并重新配置 placeholder、选择空状态、单选结果、伪类、过滤输入、有效搜索状态和 `SelectHandle` 输入状态 binding。

单选结果和多选 tag 的溢出提示不创建独立 popup 或 timer。模板和动态 tag 只声明 `OverflowTip` attached properties，实际展示延迟与位置继续由 `ToolTip.ShowDelay` 和 `ToolTip.Placement` 处理；禁用 `IsShowOverflowTip` 或文本不再溢出时只清理由 `OverflowTip` 自己写入的 tooltip。

候选弹层内容是懒创建：

```text
OpenDropDown()
  → EnsurePopupContent()
  → create PopupFrame if null
  → create SelectCandidateList if null
  → bind candidate list to Select properties
  → subscribe SelectionChanged / Commit / Cancel
  → Popup.Child = PopupFrame
```

`ClearPopupContent()` 必须解除 `_candidateList.SelectionChanged`、`Commit`、`Cancel`，清空 `ItemsSource`，移除 `TemplatedParent`，断开 `PopupFrame.Child` 和 `Popup.Child`，并清空 `_candidateListActivated`。

空候选状态由 `EnsurePopupContent()` 中的绑定驱动：`AbstractSelect.IsShowEmptyIndicator`、`EmptyIndicator` 和 `EmptyIndicatorTemplate` 绑定到候选列表的对应 `ListView` 属性。`SelectCandidateList.ConfigureEmptyIndicator()` 覆写必须同时维护 `IsEffectiveEmptyVisible` 与 `IsDefaultEmptyIndicatorVisible`：未提供自定义 `EmptyIndicator` / `EmptyIndicatorTemplate` 时显示主题内置的默认 `Empty`（`PresetImage=Simple`，对应 Ant Design 的 No data）；`EmptyIndicatorPadding` 不在此处接线，由 `ListViewTheme` 按尺寸提供默认值。

Popup 打开期间，`AbstractSelect.PopupOpened()` 创建 `SubscriptionsOnOpen`，订阅自身和祖先可见性变化；Popup 关闭时清空这些订阅。控件 detach 时释放 TopLevel deactivation 订阅，窗口失活时关闭弹层。

## 6. 交互与事件处理

候选交互由 [Select 候选交互设计](candidate-interaction-design.md) 定义。`SelectCandidateList` 维护唯一 active candidate；`IsCandidateSelected` 是容器投影，`:pointerover` 只作为鼠标命中输入。

键盘路径：

- `Select.OnKeyDown()` 先让已打开的 `_candidateList.HandleKeyDown()` 处理候选导航、提交和取消。
- `Up/Down` 在当前有效候选视图中迁移 active candidate，并在候选变化后滚动到可见区域。
- `Enter` 提交或切换当前 active candidate，不能从鼠标或键盘之外的视觉状态推导提交目标。
- `F4`、`Alt+Up/Down` 切换弹层。
- 弹层关闭时 `Up/Down`、`Enter`、`Space` 打开弹层。
- 弹层打开时 `Escape` 关闭弹层。
- `SelectResultOptionsBox.KeyDownEvent` 在 tunnel / bubble 阶段进入 `HandleFilterInputKeyDown()`，用于多选删除最后一个 tag。

指针路径：

- 候选项 PointerMoved 只在鼠标进入另一个可用候选项时迁移 active candidate；同项移动必须快速返回。
- 鼠标候选迁移不调用 `ScrollCandidateItemIntoView()`，不提交选择，也不扫描全部候选容器。
- 指针移动到 group 或不可用候选项时保留原 active candidate；`:pointerover` 不独立绘制候选背景。
- popup 内部点击不关闭弹层。
- 多选 tag 的关闭按钮点击不触发外层打开/关闭切换。
- 控件按下时设置标准 `:pressed`，释放时根据状态打开或关闭弹层。

候选提交：

```text
SelectCandidateList.Commit
  → Single: SelectedOption = CandidateList.SelectedItem
  → Multiple/Tags: SelectedOptions = CandidateList.SelectedItems copy
  → IsDropDownOpen = false
```

`SelectedOption` 和 `SelectedOptions` 都是默认 `TwoWay` 的绑定入口。`SelectedOptions` 采用属性替换作为用户交互回写方式；如果外部绑定源提供 `INotifyCollectionChanged` 集合，集合原地 `Add` / `Remove` / `Reset` 也会进入同一条选择刷新路径，更新 Form 值通知、`SelectionChanged`、已选标签、候选列表选择和 `SelectedCount`。

候选取消：

```text
SelectCandidateList.Cancel
  → IsDropDownOpen = false
```

## 7. 内部算法与关键流程

### 7.1 默认值映射

`ConfigureDefaultValues()` 只在当前选择为空时使用 `DefaultValues`。单选取第一个默认值，多选和 Tags 遍历所有默认值。比较顺序为：

```text
DefaultValueCompareFn(defaultValue, option)
  else defaultValue.ToString() == option.Content?.ToString()
```

### 7.2 选项替换后的选择保留

`OptionsSource` 变化前，控件先把当前选择转换为稳定 identity：

```text
option.ItemKey?.ToString()
  else option.Content?.ToString()
```

用户选项源更新并重建有效候选选项后，控件按 identity 查找新 `ISelectOption` 实例，尽量保留用户选择。找不到匹配项时，单选回到清空选择或默认值映射；多选和 Tags 模式需要额外保留仍然有效的已选运行时动态选项。

### 7.3 用户选项源、运行时动态选项和有效候选选项

Select 必须分离三类选项状态：

```text
用户选项源
  = XAML 内容子项 Options
  + OptionsSource
  + OptionsLoader result

运行时动态选项
  = Tags 模式根据过滤输入创建的 IsDynamicAdded 选项

有效候选选项
  = 用户选项源 + 运行时动态选项
```

`SelectCandidateList.ItemsSource` 应消费有效候选选项，而不是直接消费 `Options`。这样可以保持 `OptionsSource` 的 ItemsSource 契约，避免在外部集合处于只读绑定语义时写入运行时 tag。

Tags 模式下，搜索文本非空且当前候选列表没有结果时，控件创建 `IsDynamicAdded=true` 的 `SelectOption`，并加入运行时动态选项集合。如果该动态选项未被选中且输入内容不再匹配，或者选中项变化后不再包含它，控件只从运行时动态选项集合中移除。

删除 tag 时，如果被删除选项是动态选项，只更新 `SelectedOptions`、运行时动态选项集合和有效候选选项。不得修改用户 `OptionsSource`，也不得向 XAML 内容子项 `Options` 执行 `Add` / `Remove`。

`OptionsSource` 替换或异步加载结果到达后，Select 先按 `ItemKey`，再按 `Content` 建立新旧 option identity 映射。已选动态选项在用户选项源中找到等价正式选项时应映射为正式选项，并清理对应动态选项；找不到时保留已选动态选项，保证用户已经创建的 tag 不因外部数据刷新丢失。

### 7.4 最大选择数

`SelectCandidateList.ConfigureOptionsForMaxCount()` 在 `SelectedItems.Count >= MaxCount` 时禁用所有未选候选项，已选候选项保持启用以允许取消。未达到上限时，容器启用状态回到 `ISelectOption.IsEnabled`。

### 7.5 弹层尺寸

`ConfigureMaxDropdownHeight()` 使用：

```text
MaxPopupHeight = ItemHeight * DisplayPageSize
               + PopupContentPadding.Top
               + PopupContentPadding.Bottom
```

`ConfigurePopupMinWith()` 在 `IsPopupMatchSelectWidth=true` 时把 `EffectivePopupWidth` 设置为 Select 当前宽度，否则设置为 `0`。

### 7.6 右侧内容和 handle binding

Select 根模板中的稳定 template part 关系优先由 AXAML 表达。右侧内容区域位于 `SelectAddOnDecoratedBox.ContentRightAddOn` 内容边界内，内部节点不能依赖 `TemplateBinding` 取得 Select，因此使用 `$parent[atom:Select]` compiled ancestor binding：

- `SelectMaxCountIndicator` 通过 compiled ancestor binding 接收 `MaxCount`、`SelectedCount` 和 `IsShowMaxCountIndicator`。
- `PART_ContentRightAddOnPresenter` 通过 compiled ancestor binding 接收 `ContentRightAddOn`、`ContentRightAddOnTemplate`，并用 `ObjectConverters.IsNotNull` 控制显示。
- `SelectHandle` 通过 compiled ancestor binding 接收 Form feedback、loading 图标、展开图标、过滤状态、启用状态、motion、loading、clear、选择为空和下拉打开状态。

`SetupSelectHandleInputStateBindings()` 只保留 frame layout part 到 `SelectHandle` 的 hover / pressed sibling 状态转发。这个关系的 source 是同一模板中的 sibling part，不是 templated parent，无法用 `TemplateBinding` 或 selector 在不改变模板契约的前提下表达；它只转发交互状态，不改变 `InputControlFrame` 对 variant、effective status、error、warning 或 disabled 的唯一 ownership。该 binding 在每次重新应用模板前释放旧 `CompositeDisposable`，生命周期与模板 part 获取路径一致。

### 7.7 内部抑制状态

Select 有两个内部抑制状态：

- `IgnorePropertyChange` 用于内部恢复 `IsDropDownOpen` 时跳过一次 property changed 分发。
- `_ignoreSyncSelection` 用于候选列表提交时跳过一次 public selection 到候选列表的回写。

这两个状态必须通过成对 helper 进入和退出。即使目标属性没有发生实际变化，helper 也必须恢复抑制状态，避免下一次外部属性变化被误吞。

## 8. 资源、性能与 AOT 边界

Select 不依赖运行时反射发现模板结构。模板协作通过固定 template part、显式类型、属性绑定和事件完成。

资源和生命周期边界：

- `SubscriptionsOnOpen` 只在 popup 打开期间持有可见性订阅，popup 关闭时清空。
- `_deactivationSubscription` 在 attach 时创建，detach 时释放。
- `_selectHandleInputStateBindings` 每次模板接入前释放旧绑定，仅持有 frame layout part → SelectHandle 的 hover / pressed sibling 状态转发，不承载输入表面状态归一。
- `_candidateList` 的事件订阅和 `ItemsSource` 必须在 `ClearPopupContent()` 中释放。
- active candidate 在 popup 关闭、popup 内容释放、detach、过滤上下文变化和候选失效时清除；容器回收只清理本地投影。
- `SelectHandle` 订阅 `FormFeedback.ValidateStatus` 时必须在 feedback 变化和 logical detach 时释放，并在 logical attach 时按当前 feedback 状态重新建立。
- 异步加载通过 `AsyncSearchLoadCoordinator` 处理超时、取消和跳过旧结果。

AOT 边界：

- `SelectToken` 通过 token generator 显式注册，生成 `SelectTokenKind` 和 `SelectTokenResourceExtension`。
- API 与 Token 契约由控件文档、源码 public surface、Token 类型或生成数据维护。
- `OptionTemplate`、AddOn 模板和 EmptyIndicator 模板是 XAML 模板入口，不依赖运行时成员扫描。

候选交互热路径边界：

- 同一候选项内的 PointerMoved 使用 source index 比较后 O(1) 返回。
- 候选迁移只更新旧、新两个已准备容器；不得逐项写入全部 `ItemCount`。
- 鼠标路径不滚动，键盘路径每次候选变化最多执行一次滚动。
- 事件源解析和状态投影不创建长期订阅、每项 handler、timer 或运行时动态发现。

## 9. 维护不变量

内部重构必须保持以下不变量：

- `AbstractSelect` 继续持有输入壳体、弹层、Form、CompactSpace 和 Motion 的基础契约。
- `Select` 继续持有选择、过滤、用户选项源、Tags 运行时动态选项、有效候选选项和异步加载状态。
- `OptionsSource` 写入不能破坏 `Options` 的内容集合语义。
- Tags 运行时动态选项不能写入用户 `OptionsSource`，也不能写入 XAML 内容子项 `Options`。
- 候选列表必须绑定到有效候选选项源，不能直接绑定到只读用户选项源。
- `SelectCandidateList` 必须是 active candidate 的唯一 owner；鼠标和键盘不能分别维护候选状态。
- `CandidateSelectedIndex`、`CandidateSelectedItem` 和已准备容器的 `IsCandidateSelected` 必须指向同一候选。
- `:pointerover` 只能产生鼠标候选迁移请求，不能独立决定 Select 候选背景或 `Enter` 提交目标。
- active candidate 迁移不能提前修改 public selection，selected 视觉必须继续覆盖候选 active 视觉。
- 选择同步中的 `_ignoreSyncSelection` 只用于防止候选列表和 public selection 相互递归，必须通过成对 helper 恢复，不能吞掉外部选择变化。
- `IgnorePropertyChange` 只用于内部恢复下拉开关状态，必须通过成对 helper 恢复，不能影响下一次外部 `IsDropDownOpen` 变化。
- `Tags` 动态选项只在 `Tags` 模式创建和清理，生命周期由 Select 内部运行时动态选项集合拥有。
- 单选过滤输入在弹层关闭时显示已选项文本，弹层打开且可过滤时清空为搜索输入。
- 多选搜索输入关闭弹层时只读并清空。
- 弹层取消事件必须能阻止打开或关闭。
- 重新套用模板和 detach 不能保留旧候选列表、旧 popup child、旧 template part 绑定或旧 TopLevel 订阅。
- Semantic Part marker 的维护边界：`SelectTheme.axaml` 承载触发区静态 marker（`semantic-scope-input`、`semantic-prefix`、`semantic-suffix`、`semantic-scope-handle`、`semantic-content`、`semantic-placeholder`、`semantic-input`、`semantic-scope-tags`、`semantic-popup-root`）；共享 `SelectHandleTheme.axaml` 承载清除按钮的 `semantic-clear` marker（`clear` Part 声明 `CrossNestedOwners=true`，生成器沿 SelectHandle 主题链校验）；共享 `TagTheme.axaml` 承载 `itemContent` / `itemRemove` marker。运行时注入点：`SelectResultOptionsBox` 创建标签时追加 `SelectSemanticParts.ItemClass`；`Select.EnsurePopupContent` 创建候选列表时追加 `SelectSemanticParts.PopupListClass` 并显式 `SetTemplatedParent(this)`；`SelectCandidateList.CreateContainerForItemOverride()` 向容器追加 `SelectSemanticParts.PopupListItemClass`。marker 随容器实例创建一次，prepare/clear/recycle 路径不得增删。
- `SelectToken` 不承载选项数据、过滤值、loading、选择集合、popup 打开状态或 Form 状态。

## 10. 测试与验证

验证范围：

- `SelectShowCasePageTests` 和 snapshot，覆盖 Gallery 示例和源码片段。
- 单选、多选、Tags 三种模式的选择、清除、默认值和过滤行为。
- 键盘候选后移动鼠标、鼠标候选后按方向键以及鼠标候选后按 `Enter` 的统一 active candidate 行为。
- 鼠标停留在旧项时，键盘候选必须成为唯一候选视觉；selected 项继续保持已确认选择视觉。
- group、disabled、hidden、`MaxCount` 不可新增选择项和虚拟化回收不能产生无效或残留 active candidate。
- `OptionsSource` 替换后的选择保留。
- `Mode=Tags`、`OptionsSource` 和无匹配过滤输入组合下，运行时动态 tag 创建、提交和删除不能修改用户选项源，也不能触发 ItemsSource 只读异常。
- `OptionsLoader` 成功、超时、取消和错误通知。
- `MaxCount`、`IsHideSelectedOptions`、`MaxTagCount`、`IsResponsiveTagMode`。
- `Large/Middle/Small/Custom` 尺寸下单选输入、多选 tag、handle 和 popup 对齐。
- native validation error、Form 扩展状态、feedback 图标、CompactSpace 边框折叠和 popup open/close 事件取消。
- `SelectSemanticPartTests`：descriptor 十三个 Part 契约、模板静态 marker 清单、默认主题不消费 semantic selector、生成 Style 命中触发区目标、标签部件样式命中、前缀部件内联呈现、popup 部件 marker 暴露。
- 文档改动至少运行 `git diff --check` 并检查相对链接存在。
