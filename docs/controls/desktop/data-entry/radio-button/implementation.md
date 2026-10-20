# RadioButton 桌面版实现原理

本文档描述 RadioButton、RadioButtonGroup、OptionButton 和 OptionButtonGroup 桌面控件家族的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [RadioButton 桌面版架构设计](overview.md)，OptionButtonGroup 的方向与组合几何见 [OptionButtonGroup 方向布局设计](option-button-group-orientation-design.md)，变化记录见 [RadioButton Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [RadioButton Token 设计](token.md)，Semantic Part 契约见 [RadioButton Semantic Part 契约](semantic-part.md)。

## 1. 实现定位

本文档覆盖 RadioButton 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/RadioButton/RadioButton.cs`
- `src/AtomUI.Desktop.Controls/RadioButton/RadioButtonGroup.cs`
- `src/AtomUI.Desktop.Controls/RadioButton/RadioButtonToken.cs`
- `src/AtomUI.Desktop.Controls/RadioButton/Themes/RadioButtonGroupTheme.axaml`
- `src/AtomUI.Desktop.Controls/RadioButton/Themes/RadioButtonTheme.axaml`
- `src/AtomUI.Desktop.Controls/RadioButton/Themes/RadioIndicatorTheme.axaml`
- `src/AtomUI.Controls/RadioButton/AbstractRadioButton.cs`
- `src/AtomUI.Controls/RadioButton/AbstractRadioButtonGroup.cs`
- `src/AtomUI.Controls/RadioButton/RadioButtonGroupCheckedChangedEventArgs.cs`
- `src/AtomUI.Controls/RadioButton/RadioButtonGroupManager.cs`
- `src/AtomUI.Controls/RadioButton/RadioButtonOption.cs`
- `src/AtomUI.Controls/RadioButton/RadioIndicator.cs`
- `src/AtomUI.Desktop.Controls/OptionButtonGroup/OptionButton.cs`
- `src/AtomUI.Desktop.Controls/OptionButtonGroup/OptionButtonGroup.cs`
- `src/AtomUI.Desktop.Controls/OptionButtonGroup/OptionButtonToken.cs`
- `src/AtomUI.Desktop.Controls/OptionButtonGroup/Themes/OptionButtonGroupTheme.axaml`
- `src/AtomUI.Desktop.Controls/OptionButtonGroup/Themes/OptionButtonTheme.axaml`
- `src/AtomUI.Controls/OptionButtonGroup/AbstractOptionButton.cs`
- `src/AtomUI.Controls/OptionButtonGroup/AbstractOptionButtonGroup.cs`
- `src/AtomUI.Controls/OptionButtonGroup/OptionButtonData.cs`
- `src/AtomUI.Controls/OptionButtonGroup/OptionButtonGroupEnums.cs`
- `src/AtomUI.Controls/OptionButtonGroup/OptionCheckedChangedEventArgs.cs`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `AbstractRadioButton`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractRadioButtonGroup`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `RadioButton`：动作触发类型，负责点击、导航或局部操作状态。
- `RadioButtonGroup`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `RadioButtonGroupManager`：数据、状态或行为协作类型，维护集合同步和事件路径。
- `RadioButtonOption`：集合项、节点或容器类型，承载单项状态和模板协作。
- `RadioButtonToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `RadioIndicator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `AbstractOptionButtonGroup`：持有按钮组选择、Orientation 和容器组合位置，绘制外边框、共享分隔线和选中边框。
- `OptionButtonGroup`：创建桌面 `OptionButton` 容器并接入桌面 ControlTheme。
- `AbstractOptionButton`：持有单项内容、图标、checked 状态、GroupOrientation、GroupPositionTrait 和 EffectiveCornerRadius。
- `OptionButton`：桌面 public 按钮式选项容器。
- `OptionButtonToken`：为按钮式选项提供字体、Padding、前景、背景和 checked/disabled 状态颜色。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。
- OptionButtonGroup 是组合方向和位置的 owner；ItemsPanel 和 Item 只消费投影状态，不能反向修改 Group Orientation。

## 4. 状态与数据流

RadioButton 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`CheckedItem`、`DotSizeValue`、`ItemSpacing`。
- 选择与集合：`IsChecked`。
- 交互与状态：`IsMotionEnabled`、`IsWaveSpiritEnabled`。
- 视觉与布局：`DotPadding`、`LineSpacing`、`Orientation`、`PaddingInline`、`Background`、`BorderBrush`、`BorderThickness`、`RadioDotEffectSize`、`RadioInnerBackground`、`RadioSize`。
- OptionButtonGroup：`SelectedIndex` / `SelectedItem` 持有按钮组选中值，`Orientation` 持有排列与组合方向，`ButtonStyle` 持有 Outline/Solid 状态视觉。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

`AbstractRadioButtonGroup.CheckedItem` 是单选组的 Form value 和当前项 source of truth。属性注册默认 `BindingMode.TwoWay` 并启用 Avalonia data validation；用户勾选容器会更新 `CheckedItem`，外部设置 `CheckedItem` 会通过 `SyncCheckedState()` 回放到已实现容器。

`AbstractOptionButtonGroup` 复用 `SelectingItemsControl` 的选择状态。用户勾选、方向键导航、Form value 和外部 `SelectedIndex` / `SelectedItem` 必须通过同一 Selection 收敛。组级 renderer 使用 `SelectedIndex` 定位容器，避免 ItemsSource 模式下把数据项和生成容器进行对象比较。

方向状态按以下路径单向流动：

```text
OptionButtonGroup.Orientation
  -> default StackPanel.Orientation
  -> realized OptionButton.GroupOrientation
  -> measure / arrange
  -> EffectiveCornerRadius
  -> separator / selected outline / Wave
```

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。
- OptionButtonGroup 的 ChildIndex 订阅在 visual attach 时建立、detach 时解除，重新 attach 后重新建立。
- 生成的 OptionButton 容器清理或回收时释放 Group 建立的 relay binding、事件和 ContentTemplate，并重置 GroupOrientation 和 GroupPositionTrait。
- 直接声明的 OptionButton 从 Items 移除或 Reset 时通过集合生命周期释放 Group 建立的 binding、事件和组合状态，不能依赖生成容器清理回调。

稳定 template part 接入点：

- `PART_ItemsPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_WaveSpirit`：稳定模板协作入口，重命名前必须同步主题和实现。

OptionButtonGroup 的 `PART_ItemsPresenter` 使用绑定 Orientation 的标准 StackPanel。OptionButton 的 `PART_WaveSpirit` 使用 EffectiveCornerRadius；横向 ContentLayout 居中，纵向 ContentLayout Stretch 并从内容起始侧对齐。

`RadioButtonTheme` 的模板根节点是一个 `PixelAlignedBorder#Frame`，内含一个 `DockPanel`，左停靠
`RadioIndicator#Indicator`、剩余为 `ContentPresenter#ContentPresenter`。Semantic Part 节点映射如下：
`RadioIndicator#Indicator` 对应 `icon`（`.semantic-icon`），`ContentPresenter#ContentPresenter` 对应
`label`（`.semantic-label`），`root` 是 RadioButton owner 本身。两个 marker 使用静态
`Classes.semantic-*="True"` 标记，不使用 Binding 或运行时赋值。

checked 与 unchecked 不是两个替代实现节点：`RadioIndicator` 通过 `:checked` / `:unchecked` 伪类切换 `Render`
绘制结果，模板中始终只有一个指示圆环节点，因此 `icon` 基数恒为 `Single`。`ContentPresenter` 的 `IsVisible`
绑定到 `Content` 非空，`Content` 为空时节点隐藏但仍存在，因此 `label` 基数也恒为 `Single`。`RadioButtonGroup`
创建/回收的每个 `RadioButton` 容器都套用同一 `RadioButtonTheme`，各自公开同一 `icon` / `label` marker 契约。

`RadioButtonGroup`、`RadioIndicator`、`OptionButton` 与 `OptionButtonGroup` 不持有 Semantic descriptor：上游
Radio.Group 不提供 Semantic API，`RadioIndicator` 是 internal 类型，上游 Radio.Button 也不公开自身
`classNames` / `styles`，因此集合布局、items presenter 与按钮式选项内容不属于 RadioButton 的 Semantic Part 契约。
模板重套用后由新模板重新提供同一 `icon` / `label` marker 契约；选中状态或 `Content` 变化不增删 marker。完整契约见
[RadioButton Semantic Part 契约](semantic-part.md)。

## 6. 交互与事件处理

RadioButton 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 输入类路径必须保持 Form、validation、clear、placeholder 和键盘行为一致。

`RadioButtonGroup.CheckedChanged` 和 `OptionButtonGroup.OptionCheckedChanged` 是两条独立的控件级通知路径。OptionButtonGroup 的方向切换不改变事件时序、SelectionMode 或当前选择；横向使用 Left/Right 导航，纵向使用 Up/Down 导航。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- 内容、命令和视觉状态在模板节点之间的同步。
- `CheckedItem` 的 ViewModel 更新、用户选择、Form set/get/clear 和 `CheckedChanged` 事件通知。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。
- OptionButtonGroup 根据容器总数和索引投影 OnlyOne/First/Middle/Last，集合结构变化后使用 `ContainerFromIndex()` 重放所有已实现容器。
- OptionButton 根据 Orientation、位置和 nominal CornerRadius 计算 EffectiveCornerRadius，不修改 public CornerRadius 保存派生值。
- OptionButtonGroup renderer 使用 Group 本地容器矩形；Horizontal 使用 `DesiredSize` 保留既有自然内容外边框，Vertical 使用 `Bounds.Size` 覆盖 Stretch 或显式宽度；Horizontal 绘制纵向分隔线，Vertical 绘制横向分隔线。
- Solid 模式隐藏与选中项相邻的分隔线；Outline 模式沿主轴起始侧覆盖前一条共享边界。

方向、尺寸、位置、圆角和渲染的完整矩阵见 [OptionButtonGroup 方向布局设计](option-button-group-orientation-design.md)。

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
- OptionButtonGroup 的方向和容器位置更新为 O(realized item count)，只在方向或集合结构变化时执行。
- Group renderer 保持 O(item count)，不得在 Render 热路径创建容器列表、事件订阅或方向策略对象。
- 标准 StackPanel 负责横向自然宽度与纵向等宽排列，不为方向能力新增 VisualTree 层级。

## 9. 维护不变量

维护 RadioButton 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
- OptionButtonGroup Orientation 默认 Horizontal，横向可观察行为保持；纵向宽度遵循 Avalonia Alignment 和 Width。
- Custom 尺寸不使用专属 selector，实例值和 owner-scoped Style 可以接管对应尺寸维度。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
- OptionButtonGroup 覆盖 Orientation x ButtonStyle x SizeType、Stretch/自然/显式宽度、集合变更、非对称圆角、SelectedIndex 数据项模式和多种 render scaling。

## 状态动效初始化

内部 `RadioIndicator` 构造时持有共享初始化会话，挂载后计算的圆点尺寸直接提交，加载后才恢复过渡。初始 checked 不创建波纹任务，待执行波纹在分离和模板变更时取消。
共享目标与生命周期契约见 [状态动效初始化架构](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)。

`AbstractOptionButton` 同样持有会话，覆盖 `OptionButtonGroup` 在容器挂载后赋予的初始 checked 外观；父组的选择模型、按压反馈和后续状态切换保持原有语义。
