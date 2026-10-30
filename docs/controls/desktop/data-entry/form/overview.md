# Form 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.Form` 的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。输入控件共享分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Form 桌面版实现原理](implementation.md)，Form Token 的专项设计见 [Form Token 设计](token.md)，设计和契约变化记录见 [Form Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/Form` |
| 控件状态 | Stable |

Form 是 AtomUI 桌面数据录入体系中的表单容器，用于组织多个 `FormItem`，向子输入控件传播尺寸、状态、样式和验证配置，并统一处理表单值收集、验证、提交、重置和动态表单项删除。

Form 的职责是把输入控件接入表单语义。它不负责业务对象持久化、远程提交协议、权限控制、复杂 ViewModel 状态管理或跨页面流程编排；这些能力应由应用层或业务层承载。

## 2. 设计语言

Form 的设计语言来自表单布局、标签语义、输入控件状态和验证反馈的组合。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 布局 | 表单项在水平、垂直或行内布局中排列。 | `FormLayout`、`LabelColInfo`、`WrapperColInfo`。 |
| 标签 | 标签文本、冒号、必填标记、可选标记和 tooltip 表达字段语义。 | `LabelText`、`IsShowColon`、`RequiredMark`、`Tooltip`。 |
| 输入表面 | 表单统一向子控件传播尺寸和输入外观。 | `SizeType`、`StyleVariant`、`IsMotionEnabled`。 |
| 验证 | 表单项按触发时机和策略执行验证器。 | `ValidateTrigger`、`ValidateStrategy`、`ValidateDebounce`。 |
| 反馈 | 错误、警告、成功和验证中状态投射到输入控件及反馈图标。 | `ValidateStatus`、`ValidateResult`、`IsValidateFeedbackEnabled`。 |
| 操作 | 提交、重置和动态删除由专用按钮或事件进入 Form。 | `SubmitButton`、`ResetButton`、`ItemDeleteButton`。 |

`SizeType=Custom` 不是 Form 的第四套专属 token。Form 会把 `CustomizableSizeType` 传递给支持 `ICustomizableSizeTypeAware` 的子控件；对于只支持旧 `ISizeTypeAware` 的子控件，`Custom` 按 `Middle` 映射，保持旧控件的尺寸基线稳定。

## 3. API 与契约模型

Form 的公共契约由 Form、FormItem、FormItemDecorator、FormValidateFeedback、SubmitButton、ResetButton、验证器和表单感知接口组成。

### 3.1 Form 容器契约

| API | 语义 |
| --- | --- |
| `FormLayout` | 控制表单项整体排列。`Horizontal` / `Vertical` 使用纵向表单项列表，`Inline` 使用横向表单项列表。 |
| `LabelColInfo` / `WrapperColInfo` | 水平布局下标签列和内容列的响应式 GridLength 信息。 |
| `LabelAlign` / `LabelWrapping` | 标签对齐和换行策略。 |
| `IsShowColon` | 水平布局下是否显示标签冒号。 |
| `RequiredMark`、`CustomRequireMark`、`CustomOptionalMark` | 必填或可选标记策略。 |
| `SizeType` | 表单统一尺寸密度，支持 `Large/Middle/Small/Custom`。 |
| `StyleVariant` | 传递给支持输入外观变体的子控件。 |
| `IsMotionEnabled` | 传递给支持动效开关的子控件。 |
| `ValidateTrigger` | 表单默认验证触发时机，默认 `OnChanged`，即字段值变化时触发验证。 |
| `IsValidateFeedbackEnabled` | 是否创建并传递验证反馈控件。 |
| `InitialValues` | 表单加载时按 `FieldName` 写入初始值。 |
| `IsFormValid` | 基于表单项验证状态聚合的只读有效性状态。 |
| `Validate()` / `ValidateAsync()` | 手动验证当前可验证表单项。 |
| `Submit()` | 验证成功后收集 `FormValues` 并触发提交。 |
| `Reset()` | 清空表单项值、取消未完成验证并重置验证状态。 |
| `SetFormValues()` | 按 `FieldName` 批量写入表单项值。 |
| `DeleteFormItem()` | 删除指定表单项。 |

事件契约：

| 事件 | 语义 |
| --- | --- |
| `AboutToValidate` | 表单验证开始前触发。 |
| `Validated` | 验证结果需要对外通知时触发；独立验证失败时携带消息，提交成功时携带 values。 |
| `Submitted` | 提交成功后触发，携带收集到的 `FormValues`。 |
| `ItemValueChanged` | 任一表单项值变化后触发，并通知其他表单项。 |
| `ResetCompleted` | 重置流程完成后触发。 |

### 3.2 FormItem 契约

`FormItem` 是单个字段的表单项容器。它承载标签、字段名、验证器、帮助文案、额外内容、内容控件和当前验证状态。

稳定契约：

- `FieldName` 是表单值读写和验证消息聚合的字段键。
- `Content` 必须实现 `IFormItemAware`，除非该表单项显式关闭内容类型验证。
- `Validators` 按 `ValidateStrategy` 执行；error 结果写入内容控件的 `DataValidationErrors`，并同步投射到 `ValidateStatus`、`ValidateResult`、错误消息和 feedback。
- `Warning`、`Validating` 和 `Success` 是 Form 扩展状态，写入内容控件的 `FormStatus`；它们不写成 `DataValidationErrors` error。
- `ValidateTrigger` 可以继承 Form 默认值，也可以在 FormItem 上覆盖。
- `ValidateDebounce` 只影响延迟触发的表单项验证，不改变手动验证和提交验证的契约。
- `Help` 与验证消息共同决定表单项辅助信息区域是否保留空间。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_BodyLayout` | `Grid` | FormItem 标签列和内容列布局。 |
| `PART_LabelLayout` | `Panel` | 标签、必填标记、tooltip、可选标记和冒号承载。 |
| `PART_ContentLayout` | `Panel` | 输入内容、错误消息/帮助文本与 Extra 承载。 |

### 3.3 子控件接入契约

表单内容控件通过以下接口接入 Form：

| 接口 | 职责 |
| --- | --- |
| `IFormItemAware` | 提供值读写、清空、值变化通知和扩展验证状态通知；error 真源仍是内容控件的 `DataValidationErrors`。 |
| `IFormItemFeedbackAware` | 接收 `FormValidateFeedback` 控件，用于在输入框内部展示反馈图标。 |
| `ICustomizableSizeTypeAware` | 接收 `CustomizableSizeType`，包含 `Custom` 尺寸。 |
| `ISizeTypeAware` | 旧尺寸接口；Form 将 `Custom` 映射为 `Middle` 后传递。 |
| `IInputControlStyleVariantAware` | 接收 Form 的 `StyleVariant`。 |
| `IMotionAwareControl` | 接收 Form 的 `IsMotionEnabled`。 |

`FormItemDecorator` 用于把一个或多个输入控件组合成单个表单项内容。它自身实现 Form 接入接口，并把 value、`FormStatus`、feedback、size、motion 和 style variant 转发给子控件；涉及 error 时必须保持 Form-owned `DataValidationErrors` 与子控件 `NativeValidationStatus` 目标一致。

### 3.4 按钮契约

`SubmitButton` 和 `ResetButton` 继承 Button，通过冒泡路由事件与最近的 Form 协作。`SubmitButton.IsWatchValidateResult=true` 时会查找逻辑树或视觉树中的 Form，并根据 `Form.IsFormValid` 控制自身 `IsEnabled`。

## 4. 行为与状态模型

Form 的核心状态流：

```text
Form config
  → Prepare / sync FormItem style values
  → FormItem content via IFormItemAware
  → value changed / blur / submit validation trigger
  → FormItem validators
  → DataValidationErrors for error
  → ValidateStatus / ValidateResult / feedback / messages as projection
  → Form IsFormValid aggregation
  → SubmitButton watch state and submit result
```

验证触发模型：

- `OnChanged` 是 Form 的默认触发时机；内容控件触发 `IFormItemAware.ValueChanged` 后按 `ValidateDebounce` 延迟验证，使提交后或编辑中的错误能够随输入及时更新。
- `OnSubmit` 仅在手动验证或提交时验证，适合显式要求只在提交入口展示错误的表单。
- `OnBlur` 在 FormItem 失去焦点时按 `ValidateDebounce` 延迟验证。
- 手动 `Validate()`、`ValidateAsync()` 和 `Submit()` 直接进入验证流程，不依赖输入变化触发。

验证策略模型：

| 策略 | 行为 |
| --- | --- |
| `StopWhenFirstFailed` | 顺序执行验证器，遇到第一个 error 后停止，同时保留 error 前已产生的 warning。 |
| `Sequential` | 顺序执行所有验证器。 |
| `Parallel` | 并行执行所有验证器并聚合结果。 |

验证结果模型：

- `Error` 会使 Form 聚合为无效状态，并阻止 `Submit()` 继续提交。
- error 状态以内容控件的 `DataValidationErrors.HasErrors` 为最高优先级；Form validators 产生的 error 也写入同一 native validation 通道。
- `Warning` 会展示警告状态和警告消息，但 Form 聚合只把 error 作为提交阻断条件。
- `Validating` 和 `Default` 在 `IsFormValid` 聚合中不视为有效完成状态。
- 重置会取消未完成验证、清空 Form-owned 消息并把表单项扩展状态恢复为 `Default`；它只能清理由 Form 写入的 validation error，不能清掉 binding 或 ViewModel 写入的 native error。

提交与重置模型：

```text
Submit()
  → cancel previous form validation run
  → ValidateAsync()
  → if success, collect FormValues by FieldName
  → Validated(success, values)
  → Submitted(values)
```

```text
Reset()
  → cancel pending form validation
  → mark IsResetting
  → FormItem.ResetItemValue()
  → NotifyReset()
  → IsFormValid = false
  → ResetCompleted
  → clear IsResetting on dispatcher
```

## 5. 视觉与主题模型

Form 视觉由 Form 根模板、FormItem 模板、FormValidateFeedback 模板和按钮主题协作完成。

| 主题或资源 | 职责 |
| --- | --- |
| `FormTheme.axaml` | Form 根模板、ItemsPresenter、表单项排列方向、反馈模板和默认操作图标。 |
| `FormItemTheme.axaml` | 标签、内容、辅助信息、删除按钮、必填标记、冒号和尺寸 selector。 |
| `FormItemDecoratorTheme.axaml` | 组合表单控件的水平内容和 Extra 布局。 |
| `FormValidateFeedbackTheme.axaml` | 根据 `ValidateStatus` 选择成功、警告、错误或验证中反馈内容。 |
| `SubmitButtonTheme.axaml` | 提交按钮默认 primary 风格和本地化文案。 |
| `ResetButtonTheme.axaml` | 重置按钮默认 default 风格和本地化文案。 |
| `FormToken` | 标签颜色、必填标记颜色、标签字体和表单项间距。 |
| `SharedToken` | 输入高度、图标尺寸、状态色、字体和通用 spacing。 |

视觉兼容边界：

- `FormItem` 标签列和内容列必须继续由 `PART_BodyLayout`、`PART_LabelLayout`、`PART_ContentLayout` 协作。
- `FormLayout=Inline` 必须保持横向表单项排列和 `InlineItemSpacing`。
- `FormLayout=Horizontal/Vertical` 必须保持纵向表单项排列。
- `SizeType=Custom` 的 FormItem 标签最小高度默认走 `Middle` 分支，子输入控件的自定义尺寸由子控件自身属性决定。

## 6. 控件家族或集成关系

Form 属于 Data Entry 容器控件，和 LineEdit、SearchEdit、Select、TreeSelect、Cascader、DatePicker、TimePicker、NumericUpDown、Slider、ToggleSwitch 等输入控件通过 `IFormItemAware` 集成。

集成关系：

- `Form`：表单配置、表单项容器、验证和提交编排。
- `FormItem`：字段级布局、验证和内容接入。
- `FormActionsItem`：操作区表单项，默认不参与验证和值模型。
- `FormItemDecorator`：组合多个输入控件并转发表单能力。
- `FormValidateFeedback`：状态反馈内容承载。
- `SubmitButton` / `ResetButton`：通过路由事件进入 Form 行为。
- `FormValidatorProvider` 和内置 validators：XAML 中组织验证器集合。

## 7. 兼容性不变量

维护 Form 时必须保持以下不变量：

- 默认 `ValidateTrigger` 必须为 `OnChanged`，保持字段值变化时触发验证的默认语义。
- `FormItem.Content` 默认必须实现 `IFormItemAware`，否则应保持当前异常语义。
- `FormItem` 重新设置 Content 时必须释放旧内容的值变化订阅和 feedback 引用。
- 新验证运行必须取消旧验证和 debounce，旧异步结果不能覆盖新结果或 reset 后状态。
- `Reset()` 必须取消未完成验证，并避免 reset 引起的值变化触发新验证。
- `Reset()`、验证成功和重新验证只能清理 Form-owned `DataValidationErrors`，不得删除外部 native validation error。
- `Submit()` 只有在没有 error 时才收集值并触发提交事件。
- `Warning` 状态不得按 error 处理，除非获得明确行为变更授权。
- `FormItemDecorator` 必须继续向子控件转发 value、validation status、feedback、size、motion 和 style variant，并保持 native validation error 的目标控件稳定。
- `SubmitButton.IsWatchValidateResult=false` 时不得因为未找到 Form 或 Form 无效而强制禁用。
- Template part、token 名称、ControlTheme key 和验证枚举值不得在未授权情况下重命名或删除。

如果实现某项能力时无法保持这些不变量，应先停止实现，说明原因、影响范围、替代方案和迁移方式，并获得授权。

## 8. 专项模型

### 8.1 布局模型

`FormLayout` 控制 Form 根 ItemsPanel 的排列方向，也决定 FormItem 的默认 `Layout`：

- `Horizontal`：Form 根为纵向列表，FormItem 标签与内容水平排列。
- `Vertical`：Form 根为纵向列表，FormItem 标签与内容垂直排列。
- `Inline`：Form 根为横向列表，FormItem 默认按水平结构呈现。

水平布局下，`LabelColInfo` 和 `WrapperColInfo` 按当前 `MediaBreakPoint` 选择 GridLength，默认比例为 `1*` 和 `3*`。

### 8.2 Required Mark 模型

`RequiredMark` 定义表单项必填或可选标记显示策略：

- `Default`：必填 value item 显示默认星号。
- `Hidden`：不显示必填或可选标记。
- `Optional`：非必填 value item 显示可选文案。
- `Customize`：使用 `CustomRequireMark` 或 `CustomOptionalMark` 及其模板。

### 8.3 验证反馈模型

启用 `IsValidateFeedbackEnabled` 后，FormItem 通过 `FeedbackTemplate` 创建 `FormValidateFeedback`，把自己的 `ValidateStatus` relay 到 feedback，并传给实现 `IFormItemFeedbackAware` 的内容控件。默认 Form feedback 模板使用成功、错误、警告和 loading 图标表达状态；其中错误图标来自 `DataValidationErrors` 驱动的 error 投影，警告、成功和验证中仍由 Form 扩展状态驱动。

### 8.4 Custom SizeType 模型

Form 自身实现 `ICustomizableSizeTypeAware`。FormItem 和 FormItemDecorator 使用统一 helper 转发尺寸：

```text
target implements ICustomizableSizeTypeAware
  → relay CustomizableSizeType as-is
target implements ISizeTypeAware only
  → Large/Large, Small/Small, Middle or Custom/Middle
```

该模型保证新控件可以获得 `Custom` 尺寸，旧控件在 `Custom` 下保持 `Middle` 尺寸基线。

### 8.5 Semantic Part

Form 家族的 Semantic Part 契约由 `FormItem` 声明：`root`、`label`、`content`、`extra`、`help`、`helpItem`
六个职责区域。五个静态 Part 均为 `Single`，来自 `FormItemTheme.axaml` 的唯一内置模板；`helpItem` 是运行时
逐条创建的消息节点（`Multiple`），验证错误与警告消息以逐条 `TextBlock` 呈现，静态 `HelpText` 是 `Help` 文案
的固定实例。`Form` 容器不注册 descriptor，form 级整体定制走 Avalonia 原生 owner 样式；`FormItemDecorator`、
`FormValidateFeedback`、`SubmitButton`、`ResetButton` 和 internal `ItemDeleteButton` 不属于 Form 语义契约。
完整 selector route、`ContractType`、状态矩阵和定制边界见
[Form Semantic Part 契约](semantic-part.md)。

## 9. 文档导航与验证策略

关联文档：

- [Form 桌面版实现原理](implementation.md)
- [Form Semantic Part 契约](semantic-part.md)
- [Form Token 设计](token.md)
- [Form Changelog](changelog.md)

标签附属标记（冒号、必填星号、可选文案、tooltip、自定义 mark）、布局容器、消息文本着色、删除按钮区、
反馈控件和用户内容子树不属于 Semantic Part。完整契约见
[Form Semantic Part 契约](semantic-part.md)。

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| Public API | Form 布局、验证触发、提交、重置、初始值、动态表单项和按钮事件。 |
| 状态行为 | `OnSubmit/OnChanged/OnBlur`、debounce、并发验证取消、warning/error 聚合和 reset 状态。 |
| AXAML / Template | 稳定 template part、required mark、feedback 图标、删除按钮和 inline spacing。 |
| Semantic Part | descriptor 字段、模板 marker、三个作用域的 Selector 命中、状态与布局矩阵下的 `Single` 数量和动态增删项的 marker 生命周期。 |
| Token | 标签颜色、必填标记颜色、标签字体、冒号 margin 和表单项 spacing。 |
| Gallery | Form 示例、源码片段和自定义表单控件示例。 |
| 文档 | 运行 `git diff --check`，检查相对链接存在。 |
