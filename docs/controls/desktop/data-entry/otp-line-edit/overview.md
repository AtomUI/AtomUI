# OtpLineEdit 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.OtpLineEdit` 一次性验证码输入控件的设计定位、公共契约、状态模型、视觉主题关系和兼容边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [OtpLineEdit 桌面版实现原理](implementation.md)，Token 专项设计见 [OtpLineEdit Token 设计](token.md)，设计和契约变化记录见 [OtpLineEdit Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/LineEdit/Views/LineEditShowCase.axaml` |
| 控件状态 | Design |

OtpLineEdit 是 AtomUI 桌面数据录入体系中的一次性验证码输入控件，用于短信验证码、邮件验证码、二次验证代码、恢复代码短段输入等场景。它以单个文本值作为数据契约，以多个视觉 cell 表达逐位输入体验，并接入 AtomUI 的输入尺寸、输入表面、Form、native validation error、主题 Token 和 Gallery 文档体系。

OtpLineEdit 不负责发送验证码、倒计时、后端校验、风险控制、二维码扫描、密钥绑定或业务 Captcha 逻辑。业务侧的“发送验证码”按钮、倒计时和接口状态应组合在 Form 或业务控件中，验证码输入本身由 OtpLineEdit 承担。

## 2. 设计语言

OtpLineEdit 的设计语言来自“单一验证码值”和“分格输入表面”的组合。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 验证码语义 | 用户输入一个短文本验证码，提交时作为一个整体值。 | `Text`、`Length`、`Completed`。 |
| 分格表达 | 每个字符拥有独立视觉 cell，增强位数感知和粘贴反馈。 | cell、focus cell、separator。 |
| 输入约束 | 控制允许输入的字符范围和写入前格式化。 | `InputMode`、`Formatter`、`MaxLength` 派生自 `Length`。 |
| 安全显示 | 可用掩码隐藏每个字符，但不改变提交值。 | `IsMasked`、`MaskChar`。 |
| 输入密度 | 控件在表单、弹窗和验证流程中的尺寸等级。 | `SizeType=Large/Middle/Small/Custom`。 |
| 输入表面 | cell 边框和背景的视觉强度。 | `Outlined`、`Filled`、`Borderless`、`Underlined`。 |
| 反馈状态 | 输入的校验和业务状态。 | `Default`、`Error`、`Warning`，error 以 `DataValidationErrors` 为真源。 |

`OTP` 作为产品文案使用全大写；代码类型和 API 使用 `.NET` PascalCase 缩写形式 `Otp`，例如 `OtpLineEdit`、`OtpLineEditInputMode`、`OtpLineEditCompletedEventArgs`。

## 3. API 与契约模型

OtpLineEdit 的公共 API 以文本值、长度、输入约束、显示辅助和输入外观为核心。`Text` 是唯一对外值源，支持双向绑定；视觉 cell 不暴露独立值属性。

基础值 API：

| API | 语义 |
| --- | --- |
| `Text` | 当前验证码文本，支持双向绑定，是 Form、验证和完成事件的唯一值源。 |
| `Length` | 验证码位数，默认 `6`；控件按该值生成 cell，并把 `Text` 限制在该长度内。 |
| `PlaceholderText` | 空 cell 的占位提示；多字符占位按 cell 展示策略分配。 |
| `IsReadOnly` | 允许查看和复制当前验证码文本，但禁止编辑、删除、粘贴和清除。 |
| `IsAllowClear` | 是否显示清除入口；清除动作必须进入统一 `Clear()` 语义。 |

输入约束 API：

| API | 语义 |
| --- | --- |
| `InputMode` | 输入字符范围，支持文本、数字、字母数字等稳定模式。 |
| `Formatter` | 写入 `Text` 前的归一化函数，用于大写转换、空白裁剪或业务字符过滤。 |
| `IsMasked` | 是否以掩码显示已输入字符。 |
| `MaskChar` | 掩码字符，默认使用密码输入常见圆点。 |

视觉与布局 API：

| API | 语义 |
| --- | --- |
| `SizeType` | 输入尺寸密度，类型为 `CustomizableSizeType`。 |
| `StyleVariant` | 输入表面样式，复用 `InputControlStyleVariant`。 |
| `Status` | 显式输入反馈状态；最终视觉由 `InputControlFrame.EffectiveStatus` 计算，native validation error 以 `DataValidationErrors` 为唯一真源。 |
| `Separator` | 分隔符内容，仅参与视觉展示，不进入 `Text`。 |
| `SeparatorTemplate` | 分隔符内容模板。 |

事件与方法：

| API | 语义 |
| --- | --- |
| `Completed` | 当归一化后的 `Text.Length == Length` 时触发，事件参数携带完整文本。 |
| `Clear()` | 清空 `Text`，重置 active cell 到起始位置，并通知 Form value changed。 |
| `Focus()` | 使控件进入输入状态，active cell 定位到第一个空 cell；全部填满时定位到最后一个 cell。 |

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_RootPanel` | `Panel` | 承载 cell、separator 和清除入口的根布局区域。 |
| `PART_CellsHost` | `ItemsControl` 或等价 host | 根据 `Length` 生成 cell 和 separator。 |
| `PART_ClearButton` | `InputClearIconButton` | 清除当前验证码文本。 |
| `PART_FormFeedBack` | `ContentPresenter` | Form feedback 内容承载。 |

稳定伪类：

| 伪类 | 语义 |
| --- | --- |
| `:focus` | 控件拥有键盘焦点，active cell 显示焦点视觉。 |
| `:error` | Avalonia native validation error 状态，由 `DataValidationErrors` 驱动。 |
| `:warning` | AtomUI warning 扩展状态，仅在无 native error 时生效。 |
| `:readonly` | 只读状态。 |
| `:filled` | 所有 cell 均有字符。 |
| `:empty` | `Text` 为空。 |

## 4. 行为与状态模型

OtpLineEdit 的交互优先级：

```text
Disabled
> ReadOnly
> Native Error
> Form Error
> Form Warning
> Explicit Warning
> Explicit Error
> Focus
> PointerOver
> Normal
```

值归一化流程：

```text
User input / Paste / Form.SetValue / Binding update
      ↓
Formatter
      ↓
InputMode filter
      ↓
Length clamp
      ↓
Text
      ↓
Cell presentation + Form value changed + Completed check
```

输入行为：

- 单字符输入写入 active cell 对应位置，随后 active cell 移到下一个空 cell。
- 多字符输入或粘贴从 active cell 起顺序分发，超过 `Length` 的字符被丢弃。
- 点击后方空 cell 时，active cell 回到第一个空 cell，避免跳过中间空位。
- `Backspace` 在当前 cell 有值时清除当前字符；当前 cell 为空时回到前一位并清除。
- `Delete` 清除当前 cell，不改变后续字符顺序。
- `Left` / `Right` 在有效 cell 范围内移动 active cell。
- `Home` / `End` 分别定位到起始 cell 和最后一个可编辑 cell。

完成状态：

```text
IsCompleted = !string.IsNullOrEmpty(Text) && Text.Length == Length
```

`Completed` 只由从未完成状态进入完成状态的有效写入触发。外部绑定写入已完成文本时同样遵守完成事件语义，但重新写入相同文本不重复触发。

Form 集成以 `Text` 作为表单值。错误校验状态以 Avalonia `DataValidationErrors` 为真源；Form validator 产生的 error 写入 OtpLineEdit 根控件的 native validation 通道。内部 cell 不维护独立 error 状态，所有 cell 的 error 视觉来自根控件 effective status 投射。

## 5. 视觉与主题模型

OtpLineEdit 使用“根输入控件 + cell presenter + separator + action 区”的视觉分层：

| 主题 | 职责 |
| --- | --- |
| `OtpLineEditTheme.axaml` | 根模板、cell host、清除入口、Form feedback、focus/error/warning/disabled 视觉投射。 |
| `OtpLineEditCellTheme.axaml` | 单个 cell 的输入表面、字符显示、placeholder、mask 和 active 状态。 |

视觉状态必须与 AtomUI 输入体系一致：

- `SizeType` 控制 cell 高度、宽度、字号和间距。
- `StyleVariant` 与 `LineEdit` 保持一致，支持 `Outlined`、`Filled`、`Borderless` 和 `Underlined` 四种输入表面；`Filled` 使用填充背景，`Borderless` 移除边框，`Underlined` 只保留下边线。
- `Status` 形成 `ExplicitStatus`，在无 native error 且无更高优先级 Form 状态时参与有效状态计算。
- `DataValidationErrors.HasErrors=true` 时，根控件和所有 cell 呈现 error 视觉。
- `IsMasked=true` 只改变字符展示，不改变 `Text`、复制、Form 值或 Completed 事件。
- `Separator` 只占据视觉布局位置，不参与输入、复制、验证或长度计算。

`OtpLineEditToken` 定义 cell 宽度和 cell 间距。separator 的默认外边距属于模板布局常量；边框、背景、focus shadow、disabled、error、warning 等输入表面语义由 `InputControlFrameTheme` 与 SharedToken 统一提供，cell 文本字号复用 SharedToken 的输入字号。

## 6. 控件家族或集成关系

OtpLineEdit 属于 Data Entry 输入控件家族，与 LineEdit、SearchEdit、TextArea、NumericUpDown、DatePicker、TimePicker、Select、TreeSelect 等控件共享输入尺寸、输入表面、Form 和 native validation 语义。

集成关系：

- `LineEdit` 输入家族：提供命名风格、输入尺寸、状态优先级、清除按钮和 Form 集成参考。
- `InputControlFrame`：提供输入表面、variant、effective status 和 CompactSpace 视觉语义；OTP cell 只投射根控件状态，不拥有第二套状态源。
- `InputClearIconButton`：提供清除入口的稳定视觉和交互语义。
- `IFormItemAware` / `IFormItemFeedbackAware`：提供表单值、Form 扩展状态和 feedback 内容接入；error 由 `DataValidationErrors` 投射。
- `CustomizableSizeType`：提供 Large/Middle/Small/Custom 尺寸契约。
- Gallery ShowCase：提供用户可运行示例和源码片段。

## 7. 兼容性不变量

维护 OtpLineEdit 时必须保持以下不变量：

- `Text` 是唯一对外值源，内部 cell 不暴露独立绑定值。
- `Length` 只控制 cell 数量和最大文本长度，不把 separator 或 mask 计入长度。
- `Formatter`、`InputMode`、`Length` 的执行顺序必须稳定。
- 外部绑定、Form.SetValue、用户输入和粘贴必须进入同一归一化路径。
- `Completed` 只在从未完成进入完成时触发，不因视觉刷新或模板重建重复触发。
- `IsMasked` 和 `Separator` 只影响视觉，不改变 `Text`。
- error 状态必须以 `DataValidationErrors` 为最高优先级，不建立独立错误系统。
- Form reset、验证成功或重新验证只能清理由 Form 写入的 error，不能清除 ViewModel 或 binding 写入的 native validation error。
- 重新套用模板不能泄漏 cell 事件、清除按钮事件、binding 或 Form feedback 订阅。
- 动态 cell 生成必须由 `Length` 和 `Text` 派生，不能形成第二套验证码状态。

## 8. 专项模型

### 8.1 单一文本值模型

OtpLineEdit 以 `Text` 表达完整验证码。内部 cell 的显示字符由 `Text[index]` 派生，active cell 只记录编辑位置，不拥有字符值。删除、粘贴和格式化都先计算新文本，再一次性写回 `Text`。

### 8.2 粘贴分发模型

粘贴文本按 `Formatter` 和 `InputMode` 处理后，从 active cell 起写入。已有字符按位置被覆盖，未覆盖位置保持原值。该模型避免粘贴时先清空再逐格恢复导致的闪烁，也避免内部 cell 与根 `Text` 不同步。

### 8.3 分隔符模型

设置 `Separator` 后，分隔符自动插入到每个 cell 之间，只由视觉 host 渲染，不参与焦点导航、输入位置、`Text.Length`、复制、Form 值或验证。

分隔符字形由 `OtpSeparatorPresenter` 按当前字体的 glyph metrics 实测墨迹盒（XBearing/YBearing/Width/Height，设计单位按字号缩放），把墨迹中心平移到行盒中心：任意字符、字体和字号都自动居中，无需逐字符人工校准；度量不可用或字形无墨迹时回退为不平移。非文本内容（自定义 `SeparatorTemplate`）不做处理，居中由内容自行负责。

### 8.4 掩码模型

`IsMasked=true` 时，已输入 cell 显示 `MaskChar`，空 cell 仍显示 placeholder。掩码不改变 `Text`，也不影响 `Completed` 判断。复制或提交时始终使用真实 `Text`。

### 8.5 DataValidationErrors 模型

OtpLineEdit 的 native validation error 挂在控件根节点。内部 cell 通过根控件 effective status 获取 error 视觉。控件不提供独立 `ErrorMessage`、`HasError` 或 cell 级 error 集合。

### 8.6 Cell 边框定制模型

每个 cell 都是一个独立 `InputControlFrame`，其边框由 cell 主题的状态机拥有（rest、pointerover、pressed、active、error、warning、disabled）。定制入口有两个，优先级从高到低：

| 入口 | 作用域 | 说明 |
| --- | --- | --- |
| `CellBorderBrush` | cell 专用 | 覆盖所有 cell 的 `BorderBrush`，优先于根 `BorderBrush`。置空后回落到根 `BorderBrush`。 |
| 根 `BorderBrush` | 控件级 | 通用输入族语义；以 LocalValue 中继到每个 cell 的帧节点。 |

```xml
<Style Selector="atom|OtpLineEdit.semantic-root-border">
    <Setter Property="BorderBrush" Value="#1677FF" />
</Style>
```

两者都以 LocalValue 写入 cell 帧，因此定制期间该属性槽的 hover / pressed / active / status 变色冻结
（focus 的 `BoxShadow` 光晕不受影响）；两个入口都置空后恢复 cell 主题状态机。`Background` 不参与本中继：
`OtpLineEdit` 自身的 `Background` 是控件级透明背景，中继到 cell 会覆盖 cell 主题的填充色，因此保持不发布。

## 9. 文档导航与验证策略

关联文档：

- [OtpLineEdit 桌面版实现原理](implementation.md)
- [OtpLineEdit Token 设计](token.md)
- [OtpLineEdit Changelog](changelog.md)

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| Public API | `Text` 双向绑定、`Length` 裁剪、`InputMode` 过滤、`Formatter` 顺序、`Completed` 触发。 |
| 交互状态 | 单字符输入、粘贴分发、Backspace、Delete、方向键、只读、禁用、清除和 focus cell。 |
| Form/Validation | Form value、Form reset、Form trigger、`DataValidationErrors` error 投射和 warning 优先级。 |
| AXAML/Theme | template part、SizeType、StyleVariant、Status、focus、disabled、masked、separator、clear、feedback。 |
| Token | `OtpLineEditToken`、Token 类型、生成数据和主题引用一致。 |
| Gallery | 走查基础验证码、数字模式、粘贴填充、分隔符、掩码、Form 验证和 v6.0.8 示例标记。 |
| 文档 | `overview.md`、`implementation.md`、`token.md`、`changelog.md` 链接有效，Data Entry 分类入口包含 OtpLineEdit。 |
