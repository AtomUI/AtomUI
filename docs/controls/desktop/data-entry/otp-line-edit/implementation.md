# OtpLineEdit 桌面版实现原理

本文档描述 OtpLineEdit 桌面版一次性验证码输入控件的内部状态同步、模板接入、cell 生成、粘贴分发、Form 和 native validation 集成。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计与 API 契约见 [OtpLineEdit 桌面版架构设计](overview.md)，Token 语义见 [OtpLineEdit Token 设计](token.md)，变化记录见 [OtpLineEdit Changelog](changelog.md)。

## 1. 实现定位

OtpLineEdit 的实现以控件根节点作为值、焦点位置、验证状态和模板生命周期 owner。内部 cell 只是视觉和输入事件入口，不形成独立业务值系统。实现文档聚焦 AtomUI 控件层的状态模型、事件路径和生命周期规则，不重新说明操作系统输入法、剪贴板和 Avalonia 文本输入底层实现。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/OtpLineEdit/OtpLineEdit.cs`：公共 API、Avalonia 属性注册、事件、Form 接口、状态入口和模板生命周期。
- `src/AtomUI.Desktop.Controls/OtpLineEdit/OtpLineEdit.SemanticParts.cs`：Semantic Part descriptor 声明（`root`、`cellList`、`cell`、`separator`），cell 与 separator 为 `Multiple` + RuntimeCreated。
- `src/AtomUI.Desktop.Controls/OtpLineEdit/OtpLineEditCell.cs`：内部 cell 控件，承载单字符显示、placeholder、active、mask 和事件回调。
- `src/AtomUI.Desktop.Controls/OtpLineEdit/OtpLineEditSeparatorContext.cs`：separator 模板上下文，提供前后 cell index 和 display index。
- `src/AtomUI.Desktop.Controls/OtpLineEdit/OtpLineEditToken.cs`：cell 宽度和 cell 间距 Token。
- `src/AtomUI.Desktop.Controls/OtpLineEdit/Themes/OtpLineEditTheme.axaml`：根模板、cell host、清除按钮和 feedback 区。
- `src/AtomUI.Desktop.Controls/OtpLineEdit/Themes/OtpLineEditCellTheme.axaml`：cell 的字符显示、mask、placeholder、active/focus/error 状态。

测试与 Gallery：

- `tests/AtomUI.Desktop.Controls.Tests/OtpLineEdit/OtpLineEditBehaviorTests.cs`：值归一化、输入、粘贴、删除、导航和 completed。
- `tests/AtomUI.Desktop.Controls.Tests/OtpLineEdit/OtpLineEditFormValueTests.cs`：Form value、reset、validation 和 `DataValidationErrors`。
- `tests/AtomUIGallery.Tests/ShowCases/LineEditShowCasePageTests.cs`：LineEdit 页面内的 OtpLineEdit 示例入口、v6.0.8 标记和 Form validator Provider 写法。
- `controlgallery/AtomUIGallery/ShowCases/DataEntry/LineEdit/Views/LineEditShowCase.axaml`：Gallery 示例和源码片段来源。

## 3. 核心类职责

`OtpLineEdit` 是 public 控件，也是唯一状态 owner。它负责维护 `Text`、`Length`、active cell index、完成状态、effective status、Form feedback 和模板 part 生命周期。

`OtpLineEditCell` 是 internal visual/input cell。它接收 owner 投射的 display character、placeholder、active、masked、read-only、disabled 和 effective status。它可以把用户输入事件回调给 owner，但不能独立更新验证码值。

`OtpLineEditSeparatorContext` 是 separator 模板上下文。它只描述 separator 所处位置，不引用 owner 控件的可变状态，也不参与输入和验证。

`OtpLineEditToken` 定义 OtpLineEdit 专属布局 Token。输入表面颜色、边框、focus shadow、disabled、error 和 warning 由 `InputControlFrameTheme` 与 SharedToken 表达，cell 文本字号直接复用 SharedToken 的输入字号。

## 4. 状态与数据流

完整值流：

```text
User text input / Paste / Key command / Binding / Form.SetValue
      ↓
NormalizeText(raw)
      ↓
Formatter
      ↓
InputMode filter
      ↓
Length clamp
      ↓
SetCurrentValue(TextProperty, normalized)
      ↓
ProjectTextToCells()
      ↓
Update active index + effective states
      ↓
Form ValueChanged + Completed check
```

cell 投射流：

```text
Text + Length + PlaceholderText + IsMasked + MaskChar + ActiveIndex
      ↓
Cell presentation model
      ↓
OtpLineEditCell visual properties
```

Form 状态流：

```text
Form.SetValue(object?) → Text normalization
Text changed           → IFormItemAware.ValueChanged
Form.GetValue()        → Text
Form.ClearValue()      → Clear()
DataValidationErrors   → root effective status → all cell error projection
ValidateStatus         → FormStatus → InputControlFrame + FormFeedback
Form feedback control  → PART_FormFeedBack
```

状态所有权：

| 状态 | Owner | 说明 |
| --- | --- | --- |
| `Text` | `OtpLineEdit` | 唯一验证码值源。 |
| `Length` | `OtpLineEdit` | 生成 cell 数量和裁剪文本。 |
| active cell index | `OtpLineEdit` | 只表示编辑位置，不拥有字符值。 |
| cell display character | `OtpLineEdit` projection | 从 `Text[index]` 派生。 |
| validation error | Avalonia `DataValidationErrors` on root | error 视觉真源。 |
| warning / validating / success | Form extension state | 不写入 `DataValidationErrors`。 |

## 5. 组合结构模型

### 控件角色图

```text
OtpLineEdit
  -> Panel#PART_RootPanel (OtpLineEditTheme.axaml, template-stable)
     -> ItemsControl#PART_CellsHost (template-stable)
        -> OtpLineEditCell (OtpLineEditCellTheme.axaml, internal-observable)
        -> ContentPresenter (separator, internal-observable)
     -> InputClearIconButton#PART_ClearButton (template-stable)
     -> ContentPresenter#PART_FormFeedBack (template-stable)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `OtpLineEdit` | public control | `OtpLineEdit.cs` | 自身 | 全部 public API | public | 用户直接使用的控件入口。 |
| `PART_RootPanel` | template part | `OtpLineEditTheme.axaml` | `OtpLineEdit` template | 布局、focus、validation | template-stable | 可用于主题维护，不作为用户 API。 |
| `PART_CellsHost` | template part | `OtpLineEditTheme.axaml` | `OtpLineEdit` template | `Length`、`Separator` | template-stable | 只能作为 cell host，不能拥有值。 |
| `OtpLineEditCell` | internal control | `OtpLineEditCell.cs` | `OtpLineEdit` generated cells | `Text` display、keyboard input | internal-observable | 用于理解行为，不指导用户直接依赖。 |
| separator presenter | template visual | `OtpLineEditTheme.axaml` | `OtpLineEdit` generated cells | `Separator` | internal-observable | 只表达视觉分隔符。 |
| `PART_ClearButton` | template part | `OtpLineEditTheme.axaml` | `OtpLineEdit` template | `IsAllowClear`、`Clear()` | template-stable | 主题可维护，事件订阅必须释放。 |
| `PART_FormFeedBack` | template part | `OtpLineEditTheme.axaml` | `OtpLineEdit` template | Form feedback | template-stable | 只承载 Form feedback 内容。 |

## 6. 生命周期与模板接入

构造与初始化规则：

- `OtpLineEdit` 注册 `OtpLineEditToken` resource scope。
- `Length` 默认值为 `6`，小于 `1` 的输入按契约归一到最小有效长度。
- `Text` 的默认值为空字符串或 `null` 时，cell display 统一按空文本处理。
- `Focusable=true`，根控件负责键盘输入和焦点状态。
- cell 内部允许使用轻量 `Avalonia.Controls.TextBox` 承载 `TextPresenter` 和 caret，但真实键盘焦点始终由根 `OtpLineEdit` 持有，cell `TextBox` 不参与 tab/focus owner 竞争。
- cell 的 `StyleVariant` 和有效状态视觉由 `OtpLineEditCellTheme.axaml` 基于 `InputControlFrameTheme` 投射根控件的有效状态；cell 不维护独立的 native validation、Form 或显式 status owner。根控件遵守 disabled/read-only 交互门禁及 shared `EffectiveStatus` 优先级，cell 的 active/hover 只在有效状态允许时叠加。

`OnApplyTemplate` 规则：

- 解除旧 `PART_ClearButton` click 订阅，再绑定新按钮。
- 释放旧 cell host 相关订阅和临时 presenter 引用。
- 获取 `PART_CellsHost` 后根据 `Length` 和 `Text` 建立 cell projection。
- 获取 `PART_FormFeedBack` 后同步当前 Form feedback 内容和可见状态。
- 模板重建不触发 `Completed`，不改变 `Text`，不重置 Form validation error。

detach 规则：

- logical detach 只释放外部 Form feedback subscription，并在 logical attach 时按当前 feedback 状态重新建立。
- clear button click 属于当前模板实例，在重新套用模板前解绑旧 part；detach/reattach 同一模板实例时保持连接。
- cell 输入通过根控件 routed input 入口处理，不建立需要在 detach 时销毁的逐 cell handler。
- 不清除 `DataValidationErrors` 中非 Form 写入的 error。
- 不因为 detach 改变 `Text` 或 completed 状态。

## 7. 交互与事件处理

文本输入事件进入 owner 的统一写入入口：

```text
TextInput(cellIndex, text)
  → ReplaceFromIndex(cellIndex, text)
  → NormalizeText()
  → Set Text
  → MoveActiveIndex()
```

粘贴事件：

```text
Paste(text)
  → Normalize incoming text
  → Overlay text from active index
  → Clamp to Length
  → Set Text once
  → Move active index to first empty or last cell
```

删除事件：

```text
Backspace:
  if current cell has character → remove current character → active moves to previous filled cell
  else → remove previous character → active stays on the new first empty cell

Delete:
  remove current character → active stays on shifted character, or moves to previous filled cell when deleting the last character
```

焦点事件：

- 根控件获得焦点时，active cell 定位到第一个空 cell；全部填满时定位到最后一个 cell。
- active cell 只投射 caret/focus 视觉；不要把真实键盘焦点转移给 cell 内部 `TextBox`，否则 Avalonia `TextBox.IsFocused` 可能在 cell 切换时留下多个 stale focus 视觉。
- active cell 的字符和 caret 应由外层布局整体居中，内部 `TextPresenter` 保持 left text alignment；不能让 `TextPresenter` 自己做 `Center` 文本对齐，否则 glyph 会居中但 caret 坐标仍按未偏移文本计算，表现为光标跑到数字前面。
- cell 内部 `TextBox` 和 scroll presenter 只是 caret host，不是可编辑文本入口；鼠标悬停时必须使用 Arrow cursor，不能显示 `Ibeam`，避免把鼠标指针误认为输入 caret。
- active 空 cell 应隐藏 placeholder，避免 placeholder 字符和 caret 在同一 cell 中重叠；inactive 空 cell 仍显示 placeholder 以提示输入格式。
- cell 内部 `TextPresenter` 的 caret 可能触发 `RequestBringIntoView`；该请求必须在 `OtpLineEdit` 边界被截断，避免 Gallery 或业务页面因验证码控件内部 caret 自动滚动。
- cell pointer press 不直接让 cell 成为值 owner，只把请求 index 交给 owner 归一。
- 请求 index 大于第一个空 cell 时，owner 将 active cell 限制到第一个空 cell。

Completed 事件：

- `Text` 从未完成变为完成时触发。
- `Length` 变化导致当前 `Text` 变为完成时遵守同一规则。
- 纯模板刷新、cell projection 刷新、mask 切换和 separator 切换不触发。

## 8. 内部算法与关键流程

### 8.1 文本归一化

归一化顺序固定为：

```text
raw text
  → Formatter
  → InputMode filter
  → Length clamp
```

`Formatter` 返回 `null` 时按空文本处理。`InputMode` 不应依赖当前 UI 文化做隐式数字转换；数字模式只接受稳定的验证码字符范围。

### 8.2 cell projection

cell projection 不缓存第二份验证码数组。投射模型由 `Length` 次索引遍历生成：

```text
for index in [0, Length)
  realChar = index < Text.Length ? Text[index] : null
  displayChar = IsMasked && realChar != null ? MaskChar : realChar
  isActive = index == ActiveIndex
```

当 `Text`、`Length`、`IsMasked`、`MaskChar`、`PlaceholderText` 或 `ActiveIndex` 变化时，统一刷新 projection。

### 8.3 overlay 写入

粘贴和多字符输入使用 overlay，而不是清空再逐格写入：

```text
before = Text[..activeIndex]
afterStart = activeIndex + incoming.Length
after = afterStart < Text.Length ? Text[afterStart..] : ""
next = before + incoming + after
```

`next` 再进入统一归一化路径。该流程保证 UI 只看到一次 `Text` 变化，避免 cell 先空再恢复造成闪烁。

### 8.4 active cell 计算

active cell 计算规则：

```text
when value changes while OtpLineEdit is not focused:
  if Text.Length < Length:
    ActiveIndex = Text.Length
  else:
    ActiveIndex = Length - 1

when value changes during focused keyboard editing:
  ActiveIndex is decided by the editing command
  ActiveIndex is only clamped into [0, min(Text.Length, Length - 1)]

when focus enters OtpLineEdit:
  if Text.Length < Length:
    ActiveIndex = Text.Length
  else:
    ActiveIndex = Length - 1
```

默认定位到第一个空 cell；删除、方向键和 `Home` / `End` 这类聚焦中的键盘动作是 active cell 的直接 owner。`Text` 属性变更只负责归一化和边界夹取，不能在删除过程中重新抢占 active 位置，否则会出现被删除 cell 先保持 active、下一次按键才移动到前一格的视觉闪烁。

默认定位等价规则：

```text
if Text.Length < Length:
  ActiveIndex = Text.Length
else:
  ActiveIndex = Length - 1
```

方向键和 pointer 请求可以在有效范围内移动 active cell，但不能越过第一个空 cell。

### 8.5 effective status

effective status 计算规则：

```text
if DataValidationErrors.HasErrors:
  Error
else if Form validate status is Warning or Status == Warning:
  Warning
else if Status == Error:
  Error
else:
  Default
```

native error 始终压过 warning 和手动 status。warning 不写入 `DataValidationErrors`。

## 9. 资源、性能与 AOT 边界

资源和生命周期边界：

- 动态生成 cell 是 `Length` 驱动的控件内部结构，必须由 owner 管理创建、复用和释放。
- cell 输入由根控件 routed input 处理，不为每个 cell 建立 owner 外部订阅。
- clear button click 订阅必须在新模板接入前解绑旧按钮。
- Form feedback subscription 必须在 feedback 对象变化和 logical detach 时释放，并在 logical attach 时重新建立。
- separator context 不持有 owner 强引用，避免模板内容长期保留控件实例。

性能边界：

- `Text` 变化只进行 `Length` 范围内的 projection，不扫描外部视觉树。
- 粘贴和多字符输入只写入一次 `Text`，不逐 cell 触发多次 public value changed。
- `Length` 合理限制为短验证码输入场景；超长分格文本不是 OtpLineEdit 的职责。
- `Formatter` 由业务提供时应保持同步、快速和无副作用；控件不为 formatter 引入异步状态。

AOT 边界：

- 不通过运行时反射扫描 public API、template part 或 token。
- Token 类型通过 generator 显式注册。
- API 与 Token 契约由控件文档、源码 public surface、Token 类型或生成数据维护，不依赖 NativeAOT 不友好的运行时成员扫描。
- separator template 和 formatter 不依赖运行时动态类型发现。

## 10. 维护不变量

内部重构必须保持以下不变量：

- `Text` 是唯一验证码值源，cell 不能保存可独立提交的值。
- `OtpLineEdit` 是唯一真实键盘焦点 owner，cell `TextBox` 只能作为 caret host，不允许通过 `Focus()` 主动抢焦点。
- `OtpLineEdit` 允许自身响应外部 `BringIntoView`，但不允许 cell/TextPresenter 发出的内部 `RequestBringIntoView` 冒泡到外层滚动容器。
- 所有写入路径必须复用同一归一化函数。
- `Length` 变化必须同步裁剪 `Text`、重建 cell projection 和更新 active cell。
- `Completed` 不能由模板刷新、mask 切换、separator 切换或 validation 状态刷新触发。
- native validation error 必须从根控件投射到全部 cell，不允许 cell 自行维护 error。
- `IsReadOnly=true` 时禁止输入、粘贴、删除和清除，但保留复制和焦点视觉。
- `IsEnabled=false` 时禁止全部交互入口。
- 模板重建不能泄漏旧按钮事件、binding 或 separator context；logical reattach 后模板按钮交互保持有效，Form feedback subscription 必须恢复。
- separator 和 mask 不参与 `Text`、Form value、复制、验证和 completed 判断。
- 粘贴分发必须通过 overlay 单次写入，避免视觉闪烁和状态中间态暴露。

## 11. 测试与验证

验证范围：

- 默认 `Length=6` 生成 6 个 cell。
- `Length=8` 时外部绑定、Form.SetValue 和用户输入均按 8 位处理。
- `Length` 变小时裁剪 `Text`，并更新 active cell。
- 单字符输入自动移动 active cell。
- 多字符输入和粘贴按 active cell overlay 分发。
- `Formatter` 和 `InputMode` 执行顺序稳定。
- `Backspace`、`Delete`、`Left`、`Right`、`Home`、`End` 行为正确。
- `IsMasked` 只影响显示，不影响 `Text`、Form value 和 Completed。
- `Separator` 只影响视觉，不影响 `Text.Length`。
- `Completed` 只在从未完成进入完成时触发。
- `DataValidationErrors` 驱动根控件和所有 cell 的 error 视觉。
- Form reset 不清除非 Form 写入的 native validation error。
- 模板重建不保留旧模板事件；logical detach 释放外部 feedback 订阅，reattach 后恢复 feedback 观察且当前模板交互继续有效。
- Gallery 示例、源码片段和源码片段可被测试发现。
- 文档改动运行 `git diff --check`，并检查相对链接存在。
