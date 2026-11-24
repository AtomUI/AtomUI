# 控件样式属性 API 与回归验收方案

2026-11-24 · 公开 API 和实施范围已确认

## 1. 目标和范围

把调用方需要直接设置且具有稳定视觉含义的值定义为控件的标准属性 API。应用可以在 AXAML、Style Setter、绑定和 C# 中设置这些属性。属性按用途命名，模板按 Avalonia 的属性优先级消费实际值。一次迁移横跨 C# 与 AXAML 时，以完整的声明和消费链为单位。

方案覆盖当前桌面控件和可选取色器包中盘点到的 14 个 `*ThemeInputs.cs` 文件。其中 11 个主要负责属性声明，3 个分别负责固定样式值、FloatButton 外观绑定和通用 Token 绑定。首批只验证 Button 家族和取色器家族，其余按下表分批处理。

经评审的公开属性、内部迁移和验证记录见 [重构确认清单](../plans/2026-11-24-control-style-property-refactor-review.md)。逐项确认公开所有者、语义、默认来源、有效范围及兼容性，再确定具体实施批次。Token 配置、资源系统、生成器和公开模板契约继续作为兼容基线。

## 2. 样板属性及命名

普通属性、附加属性、默认值和模板绑定的正式规则见 [控件属性形态与命名](../../engineering/development/control-development-guidelines.md#控件属性形态与命名)。

| 现有名称 | 建议名称 | 语义依据 |
| --- | --- | --- |
| `Button.ThemeContentPadding` | `Button.ContentPadding` | 公开样板。普通内容布局的内边距，作用范围按现有形状与状态核对。 |
| `Button.ThemeCirclePadding` | `Button.CirclePadding` | 圆形状态的内边距，列入逐项 API 评审。 |
| `ColorPickerThemeInputs.ThemeColorSpectrumHeight` | `SpectrumHeight` | 色谱区域高度，列入逐项 API 评审。 |
| `ColorPickerThemeInputs.ThemeColorPickerSliderTrackSize` | `ColorPickerView.SliderTrackHeight`、`GradientColorPickerView.SliderTrackHeight` | 公开样板。控制取色器视图中滑轨的高度。 |
| `ColorPickerThemeInputs.ThemeColorPickerSliderThumbSize` | `SliderThumbSize` | 手柄宽度和高度，列入逐项 API 评审。 |
| `ButtonThemeInputs` | `ButtonStyleValues` | 禁用透明度和下划线等固定样式值 |

### 公开 API 使用示例

```xml
<atom:Button ContentPadding="12,4" />
<atom:ColorPickerView SliderTrackHeight="6" />
```

以上是计划中的 API。未设置时，现有主题仍通过各自的 Token 提供默认值。设值后模板消费有效值。移除局部值或绑定时，回到当前样式和 Token 的计算结果。`Button.Padding` 仍由原属性优先级决定，局部 `Padding` 能覆盖默认模板对它的样式赋值。每个 API 另行记录适用状态、单位、允许值及默认语义。

普通和渐变取色器视图先使用其现有共享视图基类声明同语义属性。弹出式 `ColorPicker` 和 `GradientColorPicker` 按需作为额外的公开所有者评审。它们在运行时创建编辑视图。若公开外层入口，须先验证它的未设置状态仍由编辑视图自己的 Token 提供默认值，并验证设置、清除、重新打开和更换 presenter 时的优先级及释放。

## 3. 现有文件处置

| 现有文件或声明 | 归属和处置 |
| --- | --- |
| `Button.ThemeInputs.cs` | 普通 `StyledProperty`。样板 `ContentPadding` 进入 `Button.cs` 公共属性区，其他成员按 API 评审结果分区，与 Button、SubmitButton、ResetButton 的 AXAML 一起迁移。 |
| `ColorPickerThemeInputs.cs` | 逐项确认哪些值成为视图的公开属性。仍承担内部共享传值的声明用 `ColorPickerAttachedProperties`，与普通、渐变取色器及共享部件的 C#、AXAML 一起迁移。 |
| `DateViewerCell`、`MenuPopupScrollHost`、`SelectTag`、`TreeViewItemHeader` 的 `.ThemeInputs.cs` | 由现有部件注册的附加属性。按家族迁移属性名、使用处及 `.AttachedProperties.cs` 文件名。 |
| `DatePickerPresenter`、`TimePickerPresenter`、`RangeInfoPickerInputTheme` 的 `.ThemeInputs.cs` | 由现有部件或 ControlTheme 类型注册的附加属性。按 DatePicker、TimePicker 和共享范围输入的实际引用合批。 |
| `DialogSurface`、`ImageViewer` 的 `.ThemeInputs.cs` | 附加属性也供运行时托管或独立部件读取。迁移时同步更新 C# 属性引用，并覆盖宿主挂接、关闭、替换后的实际行为。 |
| `ButtonThemeInputs.cs` | 固定样式值。按上表原位改名，AXAML 的 `x:Static` 引用同步更新。 |
| `FloatButtonThemeInputs.cs` | 同时注册附加属性、提供静态值并拥有运行时订阅。高风险批次按完整类原位改名为 `FloatButtonAppearanceBinding`，同时更新 AXAML 选择器、静态引用和三个使用方。 |
| `OwnerThemeInputs.cs` | 它实际负责把宿主 Token 动态绑定到属性，是现有绑定基础设施。高风险批次迁移为 `PartTokenBinder`，并核对调用方传入的属性、优先级和生命周期。 |

每批先列出旧名、新名、注册所有者和引用处，再统一更新。一项包括 Avalonia 注册名、`XxxProperty` 字段、已有的 CLR 包装或 `GetXxx`/`SetXxx`、Selector、Setter、`TemplateBinding`、`CompiledBinding`、`x:Static`、C# 调用和相关测试。字符串逐项区分属性名、Token 名和资源键。仍是历史事实的 changelog 可以保留旧名。

已有公开、受保护或文档暴露的契约按兼容基线审查。仅作内部改名的普通或附加属性使用现有逻辑所有者。独立注册类改名时，关联的 AXAML 所有者引用随类型一起改名。新增标准属性时逐项记录注册元数据、模板消费点、Token 回落和清除路径，并更新对应控件文档及适用的 Gallery API 和示例。

## 4. 实施顺序

| 批次 | 交付和验收 |
| --- | --- |
| 1. 建立基线并迁移样板 | `Button.ContentPadding` 和两个公开取色器视图的 `SliderTrackHeight`。核对未设置的基线、显式设置、切换主题、清除覆盖和取色器性能。 |
| 2. 迁移各家族属性 | 日期单元格、Menu、Select、TreeView、两个 Picker presenter 和范围输入主题。先确定调用方应该在哪个公开控件上设置，再按共享引用依赖更新同一组消费点。 |
| 3. 迁移涉及生命周期的使用处 | Dialog / MessageBox、ImagePreviewer 和 FloatButton。以各自的宿主打开路径验收。已有托管和独立路径进入各自的用例。 |
| 4. 汇总证据 | 在最终源码上按改动模块完成 focused 验证。逐批记录命名例外、已执行的验收、临时验证清理和仍待验证的条件。 |

执行以现有工作区为准。每批的源码先稳定，再用同一宿主和配置集中验证。同一输入的有效结果可复用。发现一项偏差时先定位该批具体属性或绑定链。

## 5. 验收门槛

### 正确性

- **完整性。** 编译受影响的 C# 和 AXAML。用旧名清单核对源码、编译绑定路径、限定属性和选择器，以及按字符串读取源码或属性的测试。逐项说明剩余命中的用途。
- **行为等价。** 每个受影响的控件家族覆盖实际改动的消费路径。Button 检查普通、圆形、仅图标状态和代表性尺寸下的 `Padding`，以及显式设置 `Padding` 的优先级。取色器检查普通和渐变视图各自覆盖滑轨尺寸，以及主题更新后各自的实际高度。其余家族优先用现有的渲染及 owner 隔离用例，补足受影响的状态选择、Popup 或模板路径。
- **可设置性。** 用正常的 AXAML、应用 Style Setter、绑定和 C# 设置公开样板，核对实际绘制值及适用状态。验收显式零值或其他有效边界值、局部覆盖、主题切换期间的覆盖以及清除后回落到当前主题值。
- **契约。** 对照属性注册和 AXAML 变更核对属性类型、默认值、继承设置、绑定优先级和选择器命中。明确公开 API 新增项，复核已有公开 API、Token 键、Semantic PART、模板部件名称和主题资源键的兼容性。涉及生成注册、裁剪或打包的差异由影响验证报告确定额外义务。

### 性能

- 先对照绑定类型、数量和创建位置。内部改名批次的注册数量与逻辑所有者按基线核对。公开 API 批次只为已评审的属性增加必要注册，样板检查默认路径的实例、模板项和运行时订阅数量。记录显式覆盖路径的额外工作，并核对避免重复绑定到同一个值。
- 用取色器首次应用模板及重复发布主题，以及有大量重复单元的日期视图作前后对照。采用同一机器、SDK、构建配置、实例数、数据和预热方式，记录每次操作的托管分配、耗时中位数及基线波动。若相同证据已存在则复用。默认路径增添额外观察者，或测得可复现的性能退化时退回相关改动。超出波动的异常需要解释后再验收。

### 资源释放

- 对照 `OwnerThemeInputs.Bind`、`BindFromOwner` 和 FloatButton 的现有订阅创建点、替换点、解除挂接和 `Dispose`，核对附加值清除及释放顺序。资源查找起点和主题变体来源按原场景核对。跨运行时部件的公开属性如需绑定，由现有 presenter 或绑定所有者管理更换和释放，并以旧部件回收验证。
- 复用 Dialog 已有的回收测试及宿主 Token 用例。对受影响且缺少等价证据的可回收部件，使用临时 `WeakReference` 探针，覆盖关闭或移除宿主、解除模板或挂接、排空 dispatcher 后的回收，以及重新挂接是否出现重复回调。探针使用现有真实主题和绑定入口。
- 通过条件为在与基线相同的释放后状态下，相关控件、部件和绑定能回收，重复打开关闭后的对象数和回调次数无持续增长。

### 最终验证

先复用具体控件家族的测试和现有渲染用例。补充验证按项目临时测试规范放置，验证后清理临时源码、运行产物和媒体，再对清理后的工作树执行 `run --scope iterate --path …` 或精确的 `dotnet test --filter`，并执行 `git diff --check`。每项证据记录源码快照、环境、入口、实际结果和适用范围。正确性偏差、可复现的性能退化、对象仍被持有或必要证据缺失时，相关批次仍待验收。

现有验证入口包括 [取色器资源隔离用例](../../../tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/ColorPickerFamilyEffectTests.Isolation.cs)、[日期单元格主题用例](../../../tests/AtomUI.Desktop.Controls.Tests/DateViewer/DateViewerThemeTests.cs)、[托管 Dialog 的 Token 用例](../../../tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/FeedbackFamilyEffectTests.HostedOwnerInputs.cs)、[Dialog 生命周期用例](../../../tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogLifecycleTests.cs) 和 [FloatButton 渲染用例](../../../tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/ButtonFamilyEffectTests.FloatButton.cs)。公开契约规则见 [控件研发标准](../../engineering/development/control-development-guidelines.md)。范围和选测方式见 [按改动影响选择验证](../../engineering/workflows/affected-verification.md) 和 [测试价值与生命周期](../../engineering/development/test-value-and-lifecycle.md)。
