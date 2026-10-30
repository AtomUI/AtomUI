# Form 桌面版实现原理

本文档描述 Form 桌面版的表单项容器、布局配置传播、值读写、验证编排、反馈控件、按钮路由、动态表单项和 Token 资源边界。输入控件共享分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计与 API 契约见 [Form 桌面版架构设计](overview.md)，Token 语义见 [Form Token 设计](token.md)，变化记录见 [Form Changelog](changelog.md)。

## 1. 实现定位

Form 的实现以 `ItemsControl` 为容器基础，`FormItem` 作为字段级容器，`IFormItemAware` 作为输入控件接入边界。实现文档聚焦 Form 自身的状态编排、验证生命周期、模板接入和维护边界，不重新说明 Avalonia ItemsControl、Button 或通用输入控件的基础行为。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Controls/Form/FormEnums.cs`：布局、必填标记、验证触发、验证策略和验证状态枚举。
- `src/AtomUI.Controls/Form/IForm.cs`：Form 容器操作接口。
- `src/AtomUI.Controls/Form/IFormItem.cs`：FormItem 字段级值读写和验证接口。
- `src/AtomUI.Controls/Form/IFormItemAware.cs`：输入控件接入 Form 的值、清空、状态和事件接口。
- `src/AtomUI.Controls/Form/IFormValidateFeedback.cs`、`FormValidateFeedback.cs`：验证反馈控件契约和基础实现。
- `src/AtomUI.Controls/Form/IFormValidator.cs`、`Validators/*.cs`：验证器契约和内置验证器。
- `src/AtomUI.Controls/Form/FormValues.cs`、`FormValidateMessage.cs`、事件参数类型：表单值和验证事件数据结构。
- `src/AtomUI.Desktop.Controls/Form/Form.cs`：桌面 Form 容器、表单项配置同步、验证聚合、提交、重置、动态删除和 ItemsControl 容器逻辑。
- `src/AtomUI.Desktop.Controls/Form/FormItem.cs`：字段级布局、模板接入、内容接入、Form 配置接收、反馈创建和响应式列宽。
- `src/AtomUI.Desktop.Controls/Form/FormItem.Validation.cs`：FormItem 验证运行、debounce、取消、策略执行、结果应用和逐条消息 items 构建。
- `src/AtomUI.Desktop.Controls/Form/FormItemDecorator.cs`：组合输入控件的表单能力转发。
- `src/AtomUI.Desktop.Controls/Form/FormItem.SemanticParts.cs`：FormItem Semantic Part 声明（`label`、`content`、`extra`、`help`）。
- `src/AtomUI.Desktop.Controls/Form/FormSizeTypeBindingHelper.cs`：`CustomizableSizeType` 到新旧尺寸接口的转发规则。
- `src/AtomUI.Desktop.Controls/Form/SubmitButton.cs`、`ResetButton.cs`：表单操作按钮和路由事件。
- `src/AtomUI.Desktop.Controls/Form/FormToken.cs`：Form 控件 Token。
- `src/AtomUI.Desktop.Controls/Form/Themes/*.axaml`：Form、FormItem、Decorator、Feedback 和操作按钮主题。

## 3. 核心类职责

`Form` 是表单容器和行为协调器。它创建或准备 `FormItem` 容器，将 Form 级配置以 style 优先级同步给表单项，处理 `SubmitButton`、`ResetButton`、`FormItem.ValueChangedEvent`、`FormItem.ValidateChangedEvent` 和 `FormItem.DeleteRequestEvent`，并在提交时收集 `FormValues`。

`FormItem` 是字段级边界。它负责标签和内容布局、响应式列宽、内容控件值变化订阅、尺寸和输入样式转发、验证状态维护、帮助和错误消息展示、feedback 控件创建，以及动态删除请求。

`FormItem.Validation.cs` 是 FormItem 的验证域。它集中持有验证取消源、debounce disposable、验证策略执行、结果聚合和消息 inlines 构建，避免验证生命周期散落在布局或模板逻辑中。

`FormItemDecorator` 是组合控件适配器。它自身作为一个 `IFormItemAware` 暴露给 FormItem，并把表单值、清空、Form 扩展状态和 feedback 转发给内部 `Child`；error 仍以子控件 `DataValidationErrors` 为目标。

`SubmitButton` 和 `ResetButton` 不直接调用父 Form 方法，而是通过冒泡路由事件交给 Form 类处理。`SubmitButton` 在 watch 模式下额外订阅 owner Form 的 `IsFormValid`。

## 4. 状态与数据流

Form 配置同步流：

```text
Form properties
  → PrepareContainerForItemOverride()
  → binding relay for inherited style properties
  → SyncConfigToItem()
  → FormItem LabelAlign / Layout / ValidateTrigger / feedback option
```

FormItem 内容接入流：

```text
FormItem.Content changed
  → release old IFormItemAware.ValueChanged
  → release old IFormItemFeedbackAware feedback
  → attach new IFormItemAware.ValueChanged
  → relay SizeType / Motion / StyleVariant
  → create or replay FormValidateFeedback when enabled
```

值变化流：

```text
content IFormItemAware.ValueChanged
  → FormItem.HandleContentValueChanged()
  → if trigger OnChanged and not resetting, ValidateValueDefer()
  → raise FormItem.ValueChangedEvent
  → Form.HandleFormItemValueChanged()
  → ItemValueChanged
  → notify sibling FormItems
```

验证结果流：

```text
ValidateValueAsync / ValidateValueDefer
  → BeginValidationRun()
  → ExecuteValidatorsAsync()
  → BuildValidationOutcome()
  → ApplyValidationOutcome()
  → write/merge Form-owned DataValidationErrors for error
  → NotifyValidateStatus() → FormStatus for extended visual state
  → FormItem.ValidateChangedEvent
  → Form.IsFormValid aggregation
```

提交值收集流：

```text
Submit()
  → ValidateAsync()
  → CollectValues()
  → FormValues.Add(FieldName, FormItem.GetItemValue())
  → Validated(success, values)
  → Submitted(values)
```

`InitialValues` 只在 Form 首次 loaded 后应用一次。Form 遍历当前 Items，按非空 `FieldName` 匹配字典键并调用 `FormItem.SetItemValue()`。

## 5. 生命周期与模板接入

Form 生命周期：

- 静态构造函数注册影响测量属性，并安装 Submit、Reset、ValueChanged、ValidateChanged 和 DeleteRequest 的 class handler。
- 实例构造函数注册 `FormToken` resource scope，并监听 `LogicalChildren` 和 `Items` 集合变化。
- `CreateContainerForItemOverride()` 创建默认 `FormItem` 并设置 `OwnerForm`。
- `PrepareContainerForItemOverride()` 绑定 Form 级属性到 FormItem，并调用 `PrepareFormItem()` 扩展点。
- `OnApplyTemplate()` 同步配置到当前 items，并配置表单根排列方向。
- `OnLoaded()` 应用 `InitialValues`。

FormItem 生命周期：

- 构造函数注册 `FormToken` resource scope，并监听 `LayoutUpdated` 以更新标签和内容最大宽度。
- `OnInitialized()` 在未设置 tooltip 图标时创建默认问号图标。
- `OnApplyTemplate()` 获取 `PART_BodyLayout`、`PART_LabelLayout`、`PART_ContentLayout`，配置布局，并在启用 feedback 时把 feedback 传给内容控件。
- `OnAttachedToVisualTree()` 查找 `MediaQueryHost`，订阅 break point 变化。
- `OnDetachedFromVisualTree()` 释放 media break 订阅并取消未完成验证。

模板 part 维护要求：

- `PART_BodyLayout`、`PART_LabelLayout` 和 `PART_ContentLayout` 是 FormItem 布局算法的稳定入口。
- `FormTheme.axaml` 中的 `PART_ItemsPresenter` 是 Form ItemsPresenter 的稳定入口。
- 重新套用模板时必须允许布局重新计算，不能把旧 part 的尺寸缓存当作新模板尺寸。

Semantic Part marker 映射（契约定义见 [Form Semantic Part 契约](semantic-part.md)）：

| Part | marker 节点 | 声明方式 |
| --- | --- | --- |
| `label` | `FormItemTheme.axaml` 模板内 `TextBlock#PART_Label` | `Classes="semantic-label"` 静态 marker。 |
| `content` | `FormItemTheme.axaml` 模板内 `ContentPresenter#ContentPresenter` | `Classes="semantic-content"` 静态 marker。 |
| `extra` | `FormItemTheme.axaml` 模板内 `ContentPresenter#ExtraPresenter` | `Classes="semantic-extra"` 静态 marker。 |
| `help` | `FormItemTheme.axaml` 模板内 `StackPanel#ExtraInfoLayout` | `Classes="semantic-help"` 静态 marker。 |
| `helpItem` | `ExtraInfoLayout` 内逐条消息 `TextBlock` 与静态 `TextBlock#HelpText` | 消息节点在代码中用生成的 semantic class 常量添加 marker；`HelpText` 用 `Classes="semantic-help-item"` 静态 marker。 |

descriptor 由 `FormItem.SemanticParts.cs` 的 `[SemanticPart]` 声明经生成器产出；`root` 隐式加入。前四个 Part 均为
静态模板节点，`RuntimeCreated=false`；`helpItem` 是 `Multiple` Part，消息节点由验证结果应用的统一写入点在
`ExtraInfoLayout` 中逐条创建、重建和清空（`RuntimeCreated=true`，顺序为消息在前、`HelpText` 在后）。`Form`、
`FormItemDecorator`、`FormValidateFeedback`、`SubmitButton`、`ResetButton` 不声明 descriptor。

## 6. 交互与事件处理

提交路径：

```text
SubmitButton.OnClick()
  → SubmitButton.SubmitEvent
  → Form.HandleSubmitButtonClick()
  → Form.Submit()
```

重置路径：

```text
ResetButton.OnClick()
  → ResetButton.ResetEvent
  → Form.HandleResetButtonClick()
  → Form.Reset()
```

删除路径：

```text
ItemDeleteButton click
  → FormItem.DeleteRequestEvent
  → Form.HandleFormItemDelete()
  → DeleteFormItem()
```

焦点验证路径：

```text
FormItem.OnLostFocus()
  → if trigger OnBlur and owner not resetting
  → ValidateValueDefer()
```

`SubmitButton.IsWatchValidateResult` 的 owner 查找同时支持逻辑树和视觉树。控件 attach 时配置 owner，detach 时释放 `IsFormValid` 订阅；关闭 watch 模式会释放 owner 并停止自动禁用。

## 7. 内部算法与关键流程

### 7.1 响应式布局计算

`FormItem.ConfigureLayout()` 根据 `Layout`、`LabelColInfo`、`WrapperColInfo` 和 `IsHideItemLabel` 重建 `PART_BodyLayout` 的行列定义。

水平布局：

```text
Column 0 = LabelColInfo at current MediaBreakPoint or 1*
Column 1 = WrapperColInfo at current MediaBreakPoint or 3*
Label and content stay in row 0
```

垂直布局：

```text
Single star column
Row 0 = label
Row 1 = content
```

隐藏标签时：

```text
Single star column
Content occupies row 0 / column 0
```

### 7.2 验证运行与取消

每次新验证都会调用 `BeginValidationRun()`：

```text
CancelPendingValidation()
  → dispose debounce timer
  → cancel and dispose old CancellationTokenSource
create new CancellationTokenSource
```

异步验证完成后再次检查 cancellation token。被取消的旧结果不能写回 `DataValidationErrors`、`ValidateStatus`、消息集合或内容控件状态。

### 7.3 验证策略执行

`ExecuteValidatorsAsync()` 根据 `ValidateStrategy` 选择并行或顺序执行。

- 并行策略先收集所有 validator task，再 `Task.WhenAll()`，最后按原 validator 顺序聚合结果。
- 顺序策略逐个 await，`StopWhenFirstFailed` 在 error 后停止。
- warning 和 error 分别收集消息；存在 error 时最终状态是 `Error`，否则存在 warning 时是 `Warning`，否则是 `Success`。

### 7.4 结果应用、DataValidationErrors 和消息构建

`ApplyValidationOutcome()` 是验证结果唯一写入点。它同时更新：

- `ValidateErrorMessages`
- `ValidateWarningMessages`
- `ValidateStatus`
- `ValidateResult`
- 内容控件 `DataValidationErrors` 中由 Form 拥有的 error
- 内容控件 `NotifyValidateStatus(status)` 扩展状态
- `HasErrorOrWarningMsg`
- 逐条消息 items（`helpItem` 运行时节点）
- `ValidateChangedEvent`

Form validator 产生的 error 必须写入内容控件的 `DataValidationErrors`，使 native binding validation、`DataValidationErrors.HasErrors`、`:error` 和由 `InputControlFrame` 投射的输入表面 error 视觉使用同一条通道。Form 清理时只能移除 Form-owned error，不能调用无差别清理导致 ViewModel 或 binding 写入的 native error 丢失。

错误和警告消息被逐条构建为带 `semantic-help-item` marker 的 `TextBlock`，插入 `ExtraInfoLayout` 并分别使用
`ErrorMessageForeground` 与 `WarningMessageForeground`；静态 `HelpText` 是 `Help` 文案的固定 `helpItem` 实例。
每次重建先清空旧消息节点，再按"消息在前、`Help` 在后"排列。警告没有 Avalonia native validation 等价语义，
因此只保留在 Form 消息、`ValidateStatus=Warning` 和控件扩展视觉状态中，不写入 `DataValidationErrors`。

### 7.5 Form 有效性聚合

Form 通过 `FormItem.ValidateChangedEvent` 更新 `IsFormValid`。

```text
if changed item is Error
  → IsFormValid = false
else
  → all value items must not be Error / Validating / Default
  → IsFormValid = true only when all pass
```

Warning 不会直接把 Form 聚合为无效，但处于 `Default` 或 `Validating` 的 value item 仍会使 `IsFormValid=false`。

### 7.6 尺寸转发

`FormSizeTypeBindingHelper.RelaySizeType()` 处理新旧尺寸接口：

```text
ICustomizableSizeTypeAware target
  → BindUtils.RelayBind same CustomizableSizeType property
ISizeTypeAware target
  → BindUtils.RelayBind with CustomizableSizeType to SizeType conversion
other target
  → Disposable.Empty
```

这类 binding 的目标是运行时内容控件，不是固定模板 part，因此由 FormItem 或 FormItemDecorator 的内容生命周期管理。

## 8. 资源、性能与 AOT 边界

Form 不依赖运行时反射扫描控件能力。控件协作通过显式接口、固定 template part、路由事件、Avalonia property 和 XAML 模板完成。

资源和生命周期边界：

- FormItem 内容变化时释放旧内容的 `ValueChanged` 订阅、feedback 引用和尺寸/style/motion relay binding。
- FormItem detach 时释放 media breakpoint 订阅并取消验证。
- FormItem feedback 变化或禁用 feedback 时释放旧 feedback 状态 relay binding。
- Form 级验证运行通过 `_validationTokenSource` 取消旧提交或验证流程。
- SubmitButton watch 模式在 logical/visual detach 时释放 Form `IsFormValid` 订阅。

AOT 边界：

- `FormToken` 通过 token generator 显式注册，生成 token kind 和资源扩展。
- API 与 Token 契约由控件文档、源码 public surface、Token 类型或生成数据维护。
- Form 验证器是显式类型和接口调用，不依赖动态成员发现。
- AXAML 模板、DataTemplate 和 Form validator provider 是显式 XAML 入口，不依赖运行时 API 扫描。

## 9. 维护不变量

内部重构必须保持以下不变量：

- Form 继续作为 FormItem 的 owner 和配置传播者，FormItem 继续作为字段级值和验证 owner。
- FormItem 的验证逻辑保持集中在验证职责域，不能重新散落到模板、布局或事件 handler 中。
- 新验证、reset、detach 和新 submit 必须取消旧验证运行。
- `IsResetting` 必须阻止 reset 期间的值变化触发验证，并在 dispatcher 队列中恢复。
- `ApplyValidationOutcome()` 必须继续作为 Form-owned `DataValidationErrors`、扩展验证状态、消息、feedback 和事件的统一写入点。
- `Warning` 和 `Error` 的聚合语义不能混淆。
- FormItem 内容替换必须释放旧内容订阅和旧 feedback。
- FormItemDecorator 的 `Child` 必须实现 `IFormItemAware`，并继续转发 feedback 和 validation status。
- 尺寸转发必须继续兼容 `ICustomizableSizeTypeAware` 和旧 `ISizeTypeAware`。
- C# relay binding 只用于运行时内容或 AXAML 无法表达的关系，新增 binding 必须有明确释放路径。
- FormToken 不承载实例数据、验证结果、表单值、loading、提交状态或具体业务字段。

## 10. 测试与验证

现有验证入口：

- `tests/AtomUI.Desktop.Controls.Tests/Form/FormBehaviorTests.cs`：覆盖默认 `OnChanged`、feedback 转发释放、验证取消、reset 取消、并行验证、stop when first failed 和 SubmitButton watch 行为。
- `tests/AtomUI.Desktop.Controls.Tests/Form/FormCustomizableSizeTypeTests.cs`：覆盖 `CustomizableSizeType` 到新旧尺寸接口的转发。
- `tests/AtomUI.Desktop.Controls.Tests/Form/FormSemanticPartTests.cs`：覆盖 descriptor 字段、模板 marker、Selector 命中、状态与布局矩阵、逐条消息项重建和动态增删项的 marker 生命周期。
- `tests/AtomUIGallery.Tests/ShowCases/FormShowCasePageTests.cs`：覆盖 Gallery Form 页面结构、示例 snapshot、源码片段和本地化资源。

维护 Form 时建议按风险选择验证：

| 改动类型 | 验证要求 |
| --- | --- |
| 验证逻辑 | 运行 Form behavior tests，并覆盖取消、reset、warning/error 和提交路径。 |
| 尺寸转发 | 运行 Form customizable size tests，并在 Gallery 检查输入控件高度对齐。 |
| Semantic Part | 运行 Form semantic part tests 与 Generator Semantic 测试，覆盖 marker、Selector 作用域和动态项生命周期。 |
| AXAML / Theme | 运行 Gallery Form showcase tests，并手动检查布局、feedback、required mark 和删除按钮。 |
| Token | 检查 Form Token 语义、FormItem 标签和 spacing。 |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
