# TimePicker 桌面版实现原理

本文档描述 TimePicker 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计与 API 契约见 [TimePicker 桌面版架构设计](overview.md)，变化记录见 [TimePicker Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [TimePicker Token 设计](token.md)。

Popup 接入边界：`InfoPickerInput` 负责业务状态和内容准备，`PickerPopup` 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

本文档覆盖 TimePicker 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/TimePicker/Localization/TimePickerLangResourceKind.cs`
- `src/AtomUI.Desktop.Controls/TimePicker/Localization/en-US.xlf`
- `src/AtomUI.Desktop.Controls/TimePicker/Localization/zh-CN.xlf`
- `src/AtomUI.Desktop.Controls/TimePicker/Localization/zh-TW.xlf`
- `src/AtomUI.Desktop.Controls/TimePicker/RangeTimePicker.cs`
- `src/AtomUI.Desktop.Controls/TimePicker/Themes/RangeTimePickerTheme.axaml`
- `src/AtomUI.Desktop.Controls/TimePicker/Themes/TimePickerPresenterTheme.axaml`
- `src/AtomUI.Desktop.Controls/TimePicker/Themes/TimePickerTheme.axaml`
- `src/AtomUI.Desktop.Controls/TimePicker/Themes/TimeViewCellTheme.axaml`
- `src/AtomUI.Desktop.Controls/TimePicker/Themes/TimeViewTheme.axaml`
- `src/AtomUI.Desktop.Controls/TimePicker/TimePicker.cs`
- `src/AtomUI.Desktop.Controls/TimePicker/RangeTimePicker.SemanticParts.cs` 与 `src/AtomUI.Desktop.Controls/TimePicker/TimePicker.SemanticParts.cs`：TimePicker 家族两个 Semantic owner 的 Semantic Part 声明（见 [TimePicker Semantic Part 契约](semantic-part.md)）。
- `src/AtomUI.Desktop.Controls/TimePicker/TimePickerPresenter.cs`
- `src/AtomUI.Desktop.Controls/TimePicker/TimePickerToken.cs`
- `src/AtomUI.Desktop.Controls/TimePicker/TimeView/DateTimePickerPanel.cs`
- `src/AtomUI.Desktop.Controls/TimePicker/TimeView/TimeView.cs`
- `src/AtomUI.Desktop.Controls/TimePicker/TimeView/TimeViewCell.cs`
- `src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/InfoPickerTextBox.cs` 与 `Themes/InfoPickerTextBoxTheme.axaml`：TimePicker 输入框使用的 internal 子控件及其文本 presenter、padding 基础视觉。

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

Gallery 确认模式对照示例分别绑定独立的提交值和显示文本，不复用普通 SelectedTime 绑定演示的状态。两种模式按“标题、输入框、提交值”分组等宽展示，空间不足时自动换行。

## 3. 核心类职责

- `DateTimePickerPanel`：布局面板，负责测量、排列、虚拟化或集合内容布局。
- `RangeTimePicker`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TimePicker`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TimePickerPresenter`：模板协作类型，承载内容展示、宿主或视觉边界。
- `InfoPickerTextBox`：internal `AbstractTextInput` 输入子控件，使用 `StyleVariant=Borderless` 表达无 chrome 语义；TimePicker/RangeTimePicker 主题只负责 picker 专用内容和布局。
- `TimePickerToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `TimeView`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TimeViewCell`：集合项、节点或容器类型，承载单项状态和模板协作。
- `TimePickerLangResourceKind`：稳定的本地化 Catalog enum；三个 XLIFF 文件提供随模块发布的内置翻译，生成器负责编译资源表和 XAML 扩展。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

TimePicker 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`IsShowHeader`、`ItemFormat`、`ItemHeight`。
- 选择与集合：`RangeEndSelectedTime`、`RangeStartSelectedTime`、`SelectedTime`、`SelectorRowCount`。
- 交互与状态：`IsNeedConfirm`、`IsShowNow`、`IsChangeOnScroll`。
- 其他稳定入口：`ClockIdentifier`、`DefaultTime`、`MinuteIncrement`、`PanelType`、`RangeEndDefaultTime`、`RangeStartDefaultTime`、`SecondIncrement`。

维护要求：

- `SelectedTime` 必须保持默认 `TwoWay` 绑定和 Avalonia data validation 能力，与 `RangeTimePicker` 的范围选择属性保持一致。
- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

### 候选、输入与提交的所有权

- 宿主保存提交值，`TimePickerPresenter.TempSelectedTime` 保存当前候选；`PreviewTime` 表示悬停，`InputText` 表示正在编辑的原始输入缓冲，三者不能写回竞争宿主 `SelectedTime`。
- 面板选择先形成完整候选，再通知宿主更新文本；悬停结束恢复候选或输入缓冲。宿主文本投影到原生输入部件时不被解释成用户编辑；输入部件自身的文本变化才解析为候选。无效文本阻止确认，关闭时恢复原提交值。
- 统一确认入口读取最新候选，结束本次编辑，再以 `SetCurrentValue` 写回并发出确认通知；已有值、显示锚点、Enter、双击确认和 Now 不分别实现提交副本。
- 无需确认的范围选择在端点切换前提交旧端点；关闭在共享基类清除活动端之前完成候选收口。相同端点值也必须开始新的候选会话，外部写入更新活动端的候选。
- 焦点在输入、列和按钮之间转移不结束编辑；焦点离开整个组合控件才收口。钉住弹层提交后从最新提交值重新开始编辑，生命周期失效释放旧订阅和候选。

### 循环列的滚动模型

`TimeViewCell` 自身承载最小行高；内部 ContentPresenter 按文字的自然高度测量，再通过 `VerticalContentAlignment=Center` 排列。内部 presenter 不重复绑定格子的 MinHeight，避免将文字拉伸至整行高度而从顶部绘制。

时、分、秒列的 ScrollViewer 在主题中使用 `VerticalScrollBarVisibility=Hidden`，只隐藏滚动条，保留滚轮、触控板与键盘滚动。循环序列没有固定首尾，滚动条位置不作为值位置指示；不能将该配置替换为会禁用纵向滚动的 `Disabled`。

`DateTimePickerPanel` 使用物理 ScrollViewer 拥有视口、extent 和 offset。时、分、秒列由 `ShouldLoop=true` 声明循环，行容器按允许值及步长生成并复用；短序列补足实际循环行以覆盖整个视口，AM/PM 保持非循环。循环行按当前 offset 的最近周期定位，越过外侧滚动周期后按整周期重定位，保留可见值、分数偏移和提交状态，首尾没有占位空白。候选选择（包括点击同值）请求对应项垂直居中的滚动位置；普通 offset 变化不推导候选，只有显式 `IsChangeOnScroll` 才选择中心附近的有效值，取整后值未变时保留小幅触控板偏移以允许累计。列内 ScrollViewer 先处理焦点的 BringIntoView 请求，TimeView 截止剩余请求，避免请求沿弹层逻辑父链推动宿主页面滚动。

Pointer 判断使用剪裁后的 ScrollContentPresenter 视口，不能使用完整时间列表 Bounds；行间空白和尾部空白结束当前悬停预览。模板固定属性、footer 可见性/可用性、文字对齐、圆角与布局均声明在 ControlTheme。运行时生成行的 motion 协作使用显式 AvaloniaProperty 绑定，其生命周期随行容器结束。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

- presenter 应在 TimeView part 可用后按 `constraints -> effective selection -> display anchor -> button state` 的顺序回放状态。模板重套用、运行时切换 `ClockIdentifier`、增量变化和受控值变化都必须进入同一同步入口。

弹层 Semantic Part 组装与生命周期：

- 弹层内容由 owner `CreatePickerPresenter()` 在首次打开时运行时创建并经 `PickerPresenter` 属性装入 `PART_Popup` 的内容根盒子；presenter / TimeView 主题模板上的 Semantic marker 随模板应用静态存在，`popup.item` marker 由 `DateTimePickerPanel.RefreshItems` 创建 `TimeViewCell` 时注入。弹层关闭不销毁 marker，列表滚动、容器复用、增量变化与弹层重开后 marker 保持；`OnDetachedFromVisualTree` 释放 owned presenter 后，下次打开重建并重新获得同一组 marker。
- 单值 `TimePicker` 的触发区与 `popup.root` marker 位于共享 `InfoPickerInputTheme.axaml`；`RangeTimePicker` 的对应 marker 位于自有 `RangeTimePickerTheme.axaml` 模板覆写。共享主题中的 marker（含 `PickerClearUpButtonTheme.axaml` 的 `semantic-clear`）同时服务于 DatePicker 与 TimePicker 两个家族的同类 Part；`TimeViewTheme.axaml` 的 `semantic-time-*` marker 对 DatePicker 是 inert class。
- `prefix` 投影节点（`AddOnContentPresenter`）以 `CompiledBinding $parent[atom:InfoPickerInput]` 接收 `ContentLeftAddOn` / `ContentLeftAddOnTemplate`；该投影是宿主模板对 `ContentLeftAddOn` 承载方式的等价重构，`ContentLeftAddOn` 为空的既有用法不受影响。

稳定 template part 接入点：

- `PART_ButtonsFrame`：承载用户触发入口、导航或关闭动作。
- `PART_ButtonsLayout`：承载用户触发入口、导航或关闭动作。
- `PART_ConfirmButton`：承载用户触发入口、导航或关闭动作。
- `PART_ContentPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_FirstSpacer`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_HeaderText`：承载文本输入、过滤、显示或编辑入口。
- `PART_HourHost`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_HourSelector`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_MainFrame`：承载根视觉、边框、背景或尺寸基线。
- `PART_MainLayout`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_MinuteHost`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_MinuteSelector`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_NowButton`：承载用户触发入口、导航或关闭动作。
- `PART_PeriodHost`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_PeriodSelector`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_PickerContainer`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_RootLayout`：承载根视觉、边框、背景或尺寸基线。
- `PART_SecondHost`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_SecondSelector`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_SecondSpacer`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_ThirdSpacer`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_TimeView`：稳定模板协作入口，重命名前必须同步主题和实现。

## 6. 交互与事件处理

时间列的指针选择状态只消费所属活输入根的事件，原始位置先转换到实际 TopLevel 坐标系，再与列区域比较。原生弹层使用自己的输入根，Overlay 使用宿主输入根；窗口退出或 `TimeView` 卸载时清除临时选择状态，其他窗口不触发选择通知。

TimePicker 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 值提交或命令触发必须保持继承控件的事件顺序。

当前没有抽取到控件专属 public 事件；交互语义主要通过继承事件、命令、属性变化和 Gallery 可观察行为体现。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- 内容、命令和视觉状态在模板节点之间的同步。
- 状态变化时避免创建不必要的视觉对象、订阅或动画对象。

输入宽度维护规则：

- `TimePicker` 的默认输入预留宽度以 `ClockIdentifier`、AM/PM 文本和字体对应的最宽格式化时间，与 AtomUI 定义的单选输入基线两者的较大值为准。`RangeTimePicker` 使用同一时间格式宽度与范围输入基线的较大值。
- 单选和范围输入基线是 AtomUI 的稳定视觉契约，由本地化默认提示文本、字体和输入结构共同计算；它不依赖外部项目的文件路径、私有实现或源码快照。
- `PlaceholderText` 和 `SecondaryPlaceholderText` 不参与 `PreferredInputWidth` / `PreferredWidth` 计算；placeholder 只能在已预留的输入内容区域内显示，超出时由文本呈现层使用 ellipsis 省略，不能反向撑大控件默认宽度。
- `Text` 和 `SecondaryText` 只表达当前显示值或 hover preview，不作为 `PreferredInputWidth` / `PreferredWidth` 的计算来源。
- `ClockIdentifier`、AM/PM 文本和字体变化会重新计算格式预留宽度和 AtomUI 默认输入基线；选中值、hover 值和范围端点切换不得改变预留宽度。
- `Width` 显式设置或 `HorizontalAlignment=Stretch` 时，控件总宽交给外部布局系统决定；`PreferredInputWidth`（输入框预留宽度）仍按内容基线计算，保证 placeholder 与选中值之间输入区宽度稳定不跳变。
- 范围选择的两端输入使用同一个格式预留宽度，`RangePickerIndicator` 和 popup placement 只跟随稳定输入框 bounds，不反向驱动输入框测量。
- 范围输入模板的内部 `AddOnDecoratedBox` 和 content presenter 必须在控件内部 stretch；范围整体测量以 `base.MeasureOverride` 的完整宽度为基础，只替换两端输入框宽度为 `PreferredWidth`，不得重新手算 padding、spacing、icon 或 add-on 宽度。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

Semantic Part 尺寸与状态基线矩阵（布局型 Part 进入实现前的事实基线）：

| 项目 | 内容 |
| --- | --- |
| 完整尺寸分支 | `SizeType` 为 `CustomizableSizeType`（`Large` / `Middle` / `Small` / `Custom`），由 `InfoPickerInput` 经 `AddOwner` 提供；`Middle` / `Custom` 使用默认字号，`Large` / `Small` 映射 `FontSizeLG` / `FontSizeSM`。 |
| 布局 owner | 触发区高度基线由共享 `AddOnDecoratedBox`（`InputControlFrame`）的按档 `MinHeight` 拥有，`PART_InfoInputBox` / `PART_SecondaryInfoInputBox` 垂直 stretch，自身不持有固定 `Height`；宽度由 owner 的 `PreferredInputWidth` 驱动（显式 `Width` 或 `Stretch` 时为 `NaN`），`prefix` / `suffix` 是内联 add-on，不拥有独立尺寸。 |
| 布局型 Part 约束 | `input` / `secondaryInput` 的 Semantic Style 参与自然测量，但不得反向改变触发区档位高度；`popup.column` 的 `Width` Setter 覆盖列宿主档位宽度（`ItemWidth` / `PeriodHostWidth`）并参与列布局测量；格子高度由 `DateTimePickerPanel.ItemHeight`（TimePickerToken `ItemHeight`）统一拥有，`popup.item` 的 Semantic Style 只覆盖视觉属性，不承诺 item 级高度；`popup.container` 的 `Padding`、`popup.footer` 的 `Margin` 按 presenter 模板既有 Token（`ButtonsPanelMargin`）为基线做增量覆盖。 |
| 状态矩阵 | 清除模式（clear 可见性）、`IsNeedConfirm` / `IsShowNow`（footer 可见性）、`ClockIdentifier` 12↔24（时段列可见性 + 时间列重建）、`MinuteIncrement` / `SecondIncrement`（item 值序列）、disabled / validation 状态均只切换可见性或视觉值，不增删 marker。 |
| 外部映射 | Ant Design `size=default|small|large` 对应 AtomUI `Middle` / `Small` / `Large` 完整分支；Ant Design 无 custom 档，AtomUI `Custom` 是本地扩展，语义 Setter 在所有档位行为一致。 |
| 失败回归 | 最小回归：在 `SizeType=Small` 下经 `TimePickerInputStyle` 设置显式 `Padding`，断言 owner 触发区实测高度仍等于该档 `AddOnDecoratedBox` 基线（输入框被裁剪对齐而不是撑破行高）。若尺寸基线被错误建立在输入框自身 `Height` 上，该断言红灯（控件高度随语义 Padding 漂移）；以“档位基线归 AddOnDecoratedBox、输入框只参与自然测量”的单一根因修复恢复。 |

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

维护 TimePicker 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
- Semantic Part marker 的维护边界：共享 `InfoPickerInputTheme.axaml` 承载单选触发区静态 marker（`semantic-scope-input`、`semantic-prefix`、`semantic-input`、`semantic-suffix`、`semantic-scope-handle`、`semantic-popup-root`）；`RangeTimePickerTheme.axaml` 承载范围触发区同名 marker 与 `semantic-secondary-input`；共享 `PickerClearUpButtonTheme.axaml` 承载 `semantic-clear` marker（`clear` Part 声明 `CrossNestedOwners=true`，生成器沿 PickerClearUpButton 主题链校验）；`TimePickerPresenterTheme.axaml` 承载 `semantic-popup-container` / `semantic-popup-footer`；`TimeViewTheme.axaml` 承载 `semantic-time-content` / `semantic-time-column`（×4 列宿主）。运行时注入点：`DateTimePickerPanel.RefreshItems` 创建 `TimeViewCell` 时追加 `popup.item` 的生成 selector class 常量。marker 随实例创建一次，列表滚动、容器复用、`ClockIdentifier` 切换、弹层重开和容器回收路径不得增删；`TimeViewTheme` 的 `semantic-time-*` marker 对未声明该契约的 DatePicker 家族保持 inert，共享主题 marker 中的同名 Part 类（`semantic-popup-*`）在两个 picker 家族各自的弹层内互不嵌套。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

Semantic Part 行为验证：

- `tests/AtomUI.Desktop.Controls.Tests/TimePicker/TimePickerSemanticPartTests.cs`：descriptor 契约（TimePicker 11 个、RangeTimePicker 12 个 Part）、宿主/共享/内部主题静态 marker 清单、`popup.item` 运行时注入与滚动复用/rebuild 保持、生成 Style 命中触发区与弹层目标（含 `secondaryInput`、`popup.content`、`popup.column`）、弹层首次打开/关闭/重开 marker 保持、`ClockIdentifier` 12↔24 切换、清除模式与 footer 可见性不增删 marker、默认主题不消费 `.semantic-*`、尺寸基线失败回归。
