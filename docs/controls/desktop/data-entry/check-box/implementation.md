# CheckBox 桌面版实现原理

本文档描述 CheckBox 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [CheckBox 桌面版架构设计](overview.md)，变化记录见 [CheckBox Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [CheckBox Token 设计](token.md)，Semantic Part 契约见 [CheckBox Semantic Part 契约](semantic-part.md)。

## 1. 实现定位

本文档覆盖 CheckBox 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/CheckBox/CheckBox.cs`
- `src/AtomUI.Desktop.Controls/CheckBox/CheckBoxGroup.cs`
- `src/AtomUI.Desktop.Controls/CheckBox/CheckBoxItemsControl.cs`
- `src/AtomUI.Desktop.Controls/CheckBox/CheckBoxToken.cs`
- `src/AtomUI.Desktop.Controls/CheckBox/Themes/CheckBoxGroupTheme.axaml`
- `src/AtomUI.Desktop.Controls/CheckBox/Themes/CheckBoxIndicatorTheme.axaml`
- `src/AtomUI.Desktop.Controls/CheckBox/Themes/CheckBoxItemsControlTheme.axaml`
- `src/AtomUI.Desktop.Controls/CheckBox/Themes/CheckBoxTheme.axaml`
- `src/AtomUI.Controls/CheckBox/AbstractCheckBox.cs`
- `src/AtomUI.Controls/CheckBox/AbstractCheckBoxGroup.cs`
- `src/AtomUI.Controls/CheckBox/AbstractCheckBoxItemsControl.cs`
- `src/AtomUI.Controls/CheckBox/CheckBoxGroupCheckedChangedEventArgs.cs`
- `src/AtomUI.Controls/CheckBox/CheckBoxIndicator.cs`
- `src/AtomUI.Controls/CheckBox/CheckBoxOption.cs`
- `src/AtomUI.Controls/CheckBox/Converters/CheckBoxIndicatorStateConverter.cs`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `AbstractCheckBox`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractCheckBoxGroup`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractCheckBoxItemsControl`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `CheckBox`：模板协作类型，承载内容展示、宿主或视觉边界。
- `CheckBoxGroup`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `CheckBoxIndicator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `CheckBoxIndicatorStateConverter`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `CheckBoxItemsControl`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `CheckBoxOption`：集合项、节点或容器类型，承载单项状态和模板协作。
- `CheckBoxToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

CheckBox 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`CheckedItems`、`ItemSpacing`、`ItemTemplate`、`ItemsSource`。
- 选择与集合：`CheckedMarkBrush`、`CheckedMarkRenderTransform`。
- 交互与状态：`IsMotionEnabled`、`IsWaveSpiritEnabled`、`State`。
- 视觉与布局：`LineSpacing`、`Orientation`、`TristateMarkBrush`、`TristateMarkSize`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

`AbstractCheckBoxGroup.CheckedItems` 是 CheckBoxGroup 的 Form value 和集合选择 source of truth。属性注册默认 `BindingMode.TwoWay` 并启用 Avalonia data validation。内部 `AbstractCheckBoxItemsControl.CheckedItems` 只接收从 public 集合复制出的快照，不能直接持有用户集合引用，避免容器选择回写时向绑定集合重复添加同一项。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- `CheckedItems` 绑定到 `INotifyCollectionChanged` 集合时，订阅由 `AbstractCheckBoxGroup` 拥有；visual tree attach 时建立，detach 或集合替换时释放。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_CheckBoxItems`：承载集合项、布局面板或虚拟化内容。
- `PART_ItemsPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_WaveSpirit`：稳定模板协作入口，重命名前必须同步主题和实现。

`CheckBoxTheme` 的模板根节点是一个 `Border#Frame`，内含一个 `DockPanel`，左停靠 `CheckBoxIndicator#Indicator`、
剩余为 `ContentPresenter#ContentPresenter`。Semantic Part 节点映射如下：`CheckBoxIndicator#Indicator` 对应
`icon`（`.semantic-icon`），`ContentPresenter#ContentPresenter` 对应 `label`（`.semantic-label`），`root` 是
CheckBox owner 本身。两个 marker 使用静态 `Classes.semantic-*="True"` 标记，不使用 Binding 或运行时赋值。

checked、unchecked 与 indeterminate 三态不是三个替代实现节点：`CheckBoxIndicator` 通过内部 `State` 枚举
（`Checked` / `Unchecked` / `Indeterminate`）和 `:checked` / `:unchecked` / `:indeterminate` 伪类切换视觉，模板中
始终只有一个指示框节点，因此 `icon` 基数恒为 `Single`。`ContentPresenter` 的 `IsVisible` 绑定到 `Content` 非空，
`Content` 为空时节点隐藏但仍存在，因此 `label` 基数也恒为 `Single`。

`CheckBoxGroup` 与 `CheckBoxItemsControl` 不持有 Semantic descriptor：上游 Ant Design Checkbox.Group 不提供
Semantic API，`CheckBoxItemsControl` 是 internal 类型，因此集合布局与 items presenter 不属于 CheckBox 的 Semantic
Part 契约。模板重套用后由新模板重新提供同一 `icon` / `label` marker 契约；勾选状态或 `Content` 变化不增删 marker。
完整契约见 [CheckBox Semantic Part 契约](semantic-part.md)。

## 6. 交互与事件处理

CheckBox 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 值提交或命令触发必须保持继承控件的事件顺序。

稳定事件路径包括 `CheckedChanged`。事件参数和触发时机属于兼容边界。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- ItemsSource、selection、checked、expanded、filter、paging 或 upload task 的集合同步。
- `CheckedItems` 集合替换或原地变更后，先更新内部选择快照，再回放已实现容器的 `IsChecked`，最后通知 Form value 和 `CheckedChanged`。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。

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

维护 CheckBox 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
