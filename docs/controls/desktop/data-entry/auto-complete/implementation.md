# AutoComplete 桌面版实现原理

本文档描述 AutoComplete 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计与 API 契约见 [AutoComplete 桌面版架构设计](overview.md)，候选列表状态契约见 [候选列表统一交互设计](../select/candidate-interaction-design.md)，变化记录见 [AutoComplete Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [AutoComplete Token 设计](token.md)。

Popup 接入边界：`AbstractAutoComplete` 负责业务状态和内容准备，candidate Popup 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

本文档覆盖 AutoComplete 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/AutoComplete/AbstractAutoComplete.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoComplete.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteLineEdit.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteOption.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompletePseudoClass.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteSearchEdit.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteSearchEditBox.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteTextArea.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteTextAreaBox.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteThemeConstants.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteToken.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/CompactSpaceAwareAutoComplete.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/CompletePopulatedEventArgs.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/CompletePopulatingEventArgs.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/CompleteValueChangedEventArgs.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/DataLoad/CompleteOptionsLoadResult.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/DataLoad/CompleteOptionsLoadedEventArgs.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/DataLoad/ICompleteOptionsAsyncLoader.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/IAutoCompleteOption.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/Themes/AbstractAutoCompleteTheme.axaml`
- `src/AtomUI.Desktop.Controls/AutoComplete/Themes/AbstractAutoCompleteTheme.cs`
- `src/AtomUI.Desktop.Controls/AutoComplete/Themes/AutoCompleteSearchEditTheme.axaml`
- `src/AtomUI.Desktop.Controls/AutoComplete/Themes/AutoCompleteTextAreaTheme.axaml`
- `src/AtomUI.Desktop.Controls/AutoComplete/Themes/AutoCompleteTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `AbstractAutoComplete`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractAutoCompleteTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `AutoComplete`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `AutoCompleteLineEditBox`：模板协作类型，承载内容展示、宿主或视觉边界。
- `AutoCompleteSearchEdit`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `AutoCompleteSearchEditBox`：模板协作类型，承载内容展示、宿主或视觉边界。
- `AutoCompleteTextArea`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `AutoCompleteTextAreaBox`：模板协作类型，承载内容展示、宿主或视觉边界。
- `AutoCompleteToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `CompactSpaceAwareAutoComplete`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `CompleteOptionsLoadResult`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 输入 box 复用 `AbstractTextInput` 和 `InputControlFrame`；AutoComplete 只拥有候选、过滤、popup 和异步状态，不复制输入表面状态 selector。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

AutoComplete 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`ClearIcon`、`ContentLeftAddOn`、`ContentLeftAddOnTemplate`、`ContentRightAddOn`、`ContentRightAddOnTemplate`、`DefaultValue`、`FilterValue`、`FilterValueSelector`、`OptionTemplate`、`OptionsAsyncLoader` 等 14 项。
- 选择与集合：`CaretIndex`、`ClearSelectionOnLostFocus`、`DisplayCandidateCount`、`Filter`、`IsShowCount`。
- 交互与状态：`IsAllowClear`、`IsAutoFocus`、`IsAutoSize`、`IsCompletionEnabled`、`IsDropDownOpen`、`IsLoading`、`IsMotionEnabled`、`IsOperating`、`IsSearchOnEnterEnabled`、`IsPopupMatchSelectWidth`、`IsReadOnly` 等 14 项。
- 视觉与布局：`MaxDropDownHeight`、`PlaceholderForeground`、`Placement`、`SearchButtonStyle`、`SizeType`、`StyleVariant`。
- 动效与异步：`AsyncLoadDebounce`、`AsyncLoadTimeout`。
- 其他稳定入口：`Lines`、`MaxLength`、`MinimumPrefixLength`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_Popup`：承载弹层宿主、打开关闭或候选内容。

## 6. 交互与事件处理

AutoComplete 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 弹层、窗口或 overlay 类路径必须稳定处理打开、关闭、取消、重复打开和宿主失活。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 输入类路径必须保持 Form、validation、clear、placeholder 和键盘行为一致。

稳定事件路径包括 `SelectionChanged`、`ValueChanged`。事件参数和触发时机属于兼容边界。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- ItemsSource、selection、checked、expanded、filter、paging 或 upload task 的集合同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。

候选交互必须遵循统一 active candidate 流：`CandidateList` 保存唯一候选 owner，鼠标命中可用项时迁移该状态但不滚动，键盘导航复用同一写入路径并可滚动到可见位置；容器回收、过滤或 source 重建时只恢复 owner 状态，不把 `:pointerover` 当作独立候选来源。`Enter` 从 active candidate 读取提交目标，不能从旧的 `SelectedItem` 或指针命中状态重新推导。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 AutoComplete 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
- Semantic Part marker、selector class、route、`ContractType` 与 cardinality（见 [AutoComplete Semantic Part 契约](semantic-part.md)）；宿主部件 marker 由嵌套输入控件模板携带，跨 owner 回收路径不泄漏 marker。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
