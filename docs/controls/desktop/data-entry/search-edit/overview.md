# SearchEdit 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.SearchEdit` 的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，Control Token 分层见 [AtomUI Control Token 设计规范](../../../../engineering/development/control-token-guidelines.md)，内部实现原理见 [SearchEdit 桌面版实现原理](implementation.md)，设计和契约变化记录见 [SearchEdit Changelog](changelog.md)。SearchEdit 拥有独立 Control identity，但当前没有 Own Token，因此不创建 `SearchEditToken` 或独立 `token.md`。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/LineEdit` |
| 控件状态 | Stable |

SearchEdit 是 AtomUI 桌面数据录入体系中的搜索输入框，用于把单行文本输入和明确的搜索触发按钮组合成一个一体化输入控件。它继承 `LineEdit` 的文本编辑、输入表面、native validation error 投射、清除按钮、密码 reveal、内部前后缀、Form 和 CompactSpace 能力，并在右侧加入固定搜索按钮。

SearchEdit 的职责是承载搜索关键字和搜索触发事件。它不负责候选项管理、自动完成、远程请求、过滤算法、搜索结果展示或异步任务编排。需要候选项和 popup 的搜索输入时，应使用 `AutoCompleteSearchEdit`；需要普通文本输入时，应使用 `LineEdit`。

## 2. 设计语言

SearchEdit 的设计语言来自输入框与搜索 action 的一体化组合。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 搜索语义 | 用户输入查询文本，并通过按钮或 Enter 键请求搜索。 | `Text`、`PlaceholderText`、`SearchRequested`。 |
| 输入密度 | 控件在表单、工具栏和筛选区中的尺寸等级。 | `SizeType=Large/Middle/Small/Custom`。 |
| 输入表面 | 输入框边框和背景强度。 | `Outlined`、`Filled`、`Borderless`、`Underlined`。 |
| 搜索按钮强调度 | 搜索 action 是普通附加按钮还是主操作按钮。 | `SearchButtonStyle=Default/Primary`。 |
| 运行状态 | 搜索操作进行中，按钮显示 loading 并阻止重复请求。 | `IsOperating=true`。 |
| 输入反馈 | 搜索条件的校验或业务状态。 | `Status=Default/Error/Warning`。 |

`Custom` 尺寸不是 SearchEdit 的第四套专属 Token。它以 `Middle` 作为未显式设置时的视觉基线，并允许用户通过 `Height`、`FontSize`、`Padding` 等常规属性覆盖实际尺寸。

## 3. API 与契约模型

SearchEdit 的公共 API 由继承的 `LineEdit` 文本输入 API 和搜索专项 API 组成。继承 API 的文本编辑、选择、滚动、只读、清除、reveal、Form 和 CompactSpace 语义必须保持与 LineEdit 一致。

SearchEdit 专项 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `SearchButtonStyle` | `SearchButtonType` | 搜索按钮样式，支持 `Default` 和 `Primary`。 |
| `SearchButtonText` | `object?` | 搜索按钮显示文本；未设置时按钮以搜索图标为主要视觉。 |
| `IsOperating` | `bool` | 搜索进行中状态；为 `true` 时显示按钮 loading 并阻止重复触发搜索请求。 |
| `IsSearchOnEnterEnabled` | `bool` | 是否允许未处理的 Enter `KeyUp` 触发搜索请求，默认值为 `true`。 |
| `SearchButtonTheme` | `ControlTheme?` | 搜索按钮 Semantic Part Theme；`TargetType` 必须兼容 public `Button`。 |
| `SearchRequested` | `RoutedEvent<SearchRequestedEventArgs>` | 搜索请求事件；按钮和 Enter 键共用该事件，并提供查询文本快照与触发来源。 |

继承输入 API 的 SearchEdit 约束：

| API | 语义 |
| --- | --- |
| `Text` / `PlaceholderText` | 搜索关键字和空文本提示。 |
| `SizeType` | 输入尺寸密度，类型为 `CustomizableSizeType`。 |
| `StyleVariant` | 输入表面样式。 |
| `Status` | 显式输入反馈状态；最终视觉由 `InputControlFrame.EffectiveStatus` 计算，native validation error 以 `DataValidationErrors` 为唯一真源。 |
| `IsAllowClear` / `ClearIcon` | 搜索文本清除入口。 |
| `InnerLeftContentTemplate` / `InnerRightContentTemplate` | 输入框内部前后缀内容模板。 |
| `LeftAddOn` / `LeftAddOnTemplate` | 输入框左侧外部附加内容。 |
| `IsMotionEnabled` | 内部按钮和输入壳体动效开关。 |

SearchEdit 的右侧外部 add-on 位置由搜索按钮占用。维护时不应把用户 `RightAddOn` 与搜索按钮混用；需要放在输入框内部右侧的内容应使用 `InnerRightContent`。

稳定 template part：

| Template Part | 类型 | 所属主题 | 职责 |
| --- | --- | --- | --- |
| `PART_InputControlFrame` | `SearchEditDecoratedBox` | `SearchEditTheme.axaml` | 输入表面、effective status、CompactSpace 和搜索按钮组合入口；具体 decorated box 只扩展搜索布局。 |
| `PART_RightAddOn` | `Button` | `SearchEditDecoratedBoxTheme.axaml` | public Button 语义部件，承载图标、文字、loading、按钮样式和点击事件。 |
| `PART_ContentFrame` | `Border` | `SearchEditDecoratedBoxTheme.axaml` | 文本输入框视觉边框和背景。 |
| `PART_ScrollViewer` | `ScrollViewer` | `SearchEditTheme.axaml` | 文本滚动区域。 |
| `PART_TextPresenter` | `InputTextPresenter` | `SearchEditTheme.axaml` | 文本显示、光标、选择和密码 reveal。 |
| `Placeholder` | `TextBlock` | `SearchEditTheme.axaml` | 空文本占位提示。 |
| `PART_ClearButton` | `InputClearIconButton` | `SearchEditTheme.axaml` | 清除当前搜索文本。 |
| `PART_RevealButton` | `RevealButton` | `SearchEditTheme.axaml` | 密码 reveal 兼容入口。 |
| `InnerRightContentPresenter` | `ContentPresenter` | `SearchEditTheme.axaml` | 输入框内部右侧内容承载。 |

## 4. 行为与状态模型

SearchEdit 的文本输入行为继承 LineEdit，搜索请求由按钮或 Enter 键进入统一管线：

```text
Button#PART_RightAddOn.Click
  ↓
SearchEditDecoratedBox.HandleSearchButtonClick
  ↓
SearchEdit.RaiseSearchRequested(Button)
  ↓
if !IsOperating raise SearchRequested

Enter KeyUp when IsSearchOnEnterEnabled && !Handled
  ↓
mark KeyUp handled
  ↓
SearchEdit.RaiseSearchRequested(EnterKey)
  ↓
if !IsOperating raise SearchRequested
```

`SearchRequestedEventArgs.Query` 保存触发时的 `Text` 快照，`Trigger` 使用 `Button` 或 `EnterKey` 区分来源。`IsOperating=true` 只表示搜索正在进行。它不改变 `Text`、不自动禁用文本编辑、不管理异步任务，也不清空搜索结果；业务层负责在搜索开始和结束时设置该属性。

状态优先级：

```text
Disabled
> Native Error
> Form Error
> Form Warning
> Explicit Warning
> Explicit Error
> Searching button loading
> Focus
> PointerOver / Pressed
> Normal
```

`IsEnabled=false` 会传递给搜索按钮，使 frame 和按钮一起进入 disabled 视觉。`DataValidationErrors`、Form warning/status 和用户 `Status` 分别通过 frame 的 native validation、`FormStatus` 和 `Status` 输入参与 `EffectiveStatus`；最终由 `InputControlFrame.EffectiveStatus` 按共享优先级投射到输入表面和搜索按钮。`SearchButtonStyle` 只控制按钮强调度，不改变文本编辑、清除、Form 或搜索事件语义。

## 5. 视觉与主题模型

SearchEdit 使用三层主题协作：

| 主题 | 职责 |
| --- | --- |
| `SearchEditTheme.axaml` | SearchEdit 根模板、文本 presenter、placeholder、clear/reveal/inner-right 内容和 frame 组合。 |
| `InputControlFrameTheme.axaml` | 输入表面 variant、effective status、边框、背景、focus/hover/pressed、disabled、CompactSpace 和 motion。 |
| `SearchEditDecoratedBoxTheme.axaml` | 搜索按钮、左右布局、搜索按钮 style 和 z-index 关系；不重复实现 frame 状态 selector。 |
| `SearchButtonTheme.axaml` | 搜索按钮在不同输入表面中的背景、前景和状态色。 |

SearchEdit 拥有独立 `ControlTokenIdentity`，但不定义 Own Token。它继承 `LineEdit` 的行为并不意味着继承或借用
LineEdit identity；Control 级配置中的任意已注册 Global Token 都绑定到 SearchEdit 自己的 Effective Global
Token。合法但没有被当前主题直接或间接消费的 Global Token 可以没有视觉效果。

主题资源边界：

| Token 来源 | 用途 |
| --- | --- |
| `SearchEditTokenResource` | 读取 SearchEdit Effective Global Token，仅负责 SearchEdit 专属组合值；通用输入表面值由 frame theme 从 SharedToken 消费。 |
| `InputControlFrameTheme` / `SharedTokenResource` | 负责输入表面边框、背景、圆角、focus shadow、状态色、disabled 和 CompactSpace；不由 SearchEditDecoratedBox 重复实现。 |
| `ButtonTokenResource` | 由真实 Button 和 `SearchButtonTheme` 显式读取 Button Own/Effective Global Token，负责按钮基础视觉。 |
| `SharedTokenResource` | 读取真正的 Global Token，只用于不响应 SearchEdit Control 级覆盖的共享值。 |

`SearchButtonTheme` 的 `TargetType` 是 Button，但它是由 SearchEdit 主题实际构造的定制主题，不是全局导出的 Button 默认主题；不能据 TargetType 反向保留 SearchEdit。它可以同时使用
`SearchEditTokenResource` 与 `ButtonTokenResource`；这是显式跨 Control 资源引用，不是 SearchEdit 借用 Button
或 LineEdit identity。

搜索按钮必须与输入框视觉上组成单一控件。Custom 高度下，搜索按钮的可视 `Frame` 高度必须跟随 `SearchEditDecoratedBox` 的实际布局高度，避免按钮边框和输入框边框错位。

## 6. 控件家族或集成关系

SearchEdit 是 LineEdit 家族的搜索专用入口：

- 继承 `LineEdit`：复用文本输入、输入状态、Form、清除按钮、内部内容和 CompactSpace。
- 使用 `SearchEditDecoratedBox`：在 `InputControlFrame` / `AddOnDecoratedBox` 基础上把搜索按钮纳入一体化布局。
- 使用 public `Button` 作为 `PART_RightAddOn`：Button 保留自己的 identity，输入表面和 status 组合视觉由 SearchEdit owner theme 投射。
- 被 `AutoCompleteSearchEditBox` 复用：AutoComplete 搜索输入框内部使用 SearchEdit 样式和搜索按钮契约。
- Gallery 中 SearchEdit 示例与 LineEdit 页面共存；继承 API 以 LineEdit 文档为入口，SearchEdit identity 与资源边界以本文第 5 节为准。

## 7. 兼容性不变量

维护 SearchEdit 时必须保持以下不变量：

- 不改变继承自 LineEdit 的 `Text`、选择、光标、清除、reveal、Form 和 CompactSpace 语义。
- 不删除或重命名 `SearchButtonStyle`、`SearchButtonText`、`IsOperating`、`IsSearchOnEnterEnabled`、`SearchRequested`、`SearchTriggerSource` 或 `SearchRequestedEventArgs`。
- `IsOperating=true` 必须阻止按钮和 Enter 键产生重复搜索请求，但不得自动管理异步任务或修改 `Text`。
- `IsSearchOnEnterEnabled=false` 时不得消费 Enter 键；已经标记为 handled 的 Enter 键不得触发搜索。
- `SearchRequestedEventArgs.Query` 必须是触发时的查询文本快照，`Trigger` 必须准确表示 `Button` 或 `EnterKey`。
- 搜索按钮的 `IsEnabled`、`SizeType` 和 loading 必须跟随 SearchEdit；按钮组合视觉必须响应 `InputControlFrame.EffectiveStatus` 和 `StyleVariant`。
- 右侧外部 add-on 位置属于搜索按钮；内部右侧内容必须继续由 `InnerRightContent` 承载。
- SearchEdit 的按钮边框和输入框边框必须在 Large、Middle、Small 和 Custom 高度下严格对齐。
- `SizeType=Custom` 必须以 Middle 作为未显式覆盖时的视觉基线。
- SearchEdit 保持独立 Control identity；当前不新增 Own Token，也不得借用 LineEdit 或 Button identity。
- `SearchButtonTheme` 必须继续以 public Button 为 TargetType，并显式区分 SearchEdit 组合语义与 Button 基础视觉；通用输入表面由 frame theme 提供。
- 重新套用模板时必须释放旧搜索按钮 click 订阅。

## 8. 专项模型

### 8.1 搜索按钮样式模型

`SearchButtonStyle=Default` 表示搜索按钮是输入框的普通右侧 action。Outlined 下按钮 hover/pressed 可提升 z-index，使按钮边框与输入框边框在激活时不互相遮挡。

`SearchButtonStyle=Primary` 表示搜索按钮是主操作入口。Outlined 下按钮保持激活 z-index，使主按钮和输入框边框组合稳定。

非 Outlined 表面中，搜索按钮使用 `ButtonType=Text`，避免在 Filled、Borderless 和 Underlined 场景中出现独立边框破坏输入表面。

### 8.2 自定义尺寸模型

SearchEdit 的 `SizeType=Custom` 走 LineEdit 家族的 Custom size 规则。未设置本地尺寸属性时，主题提供 Middle 的字号、line height、圆角和 padding；用户设置 `Height`、`FontSize` 等属性后，输入框内容和搜索按钮必须使用同一个实际布局高度。

### 8.3 AutoComplete 集成模型

`AutoCompleteSearchEdit` 暴露 SearchEdit 的搜索按钮属性、`IsOperating`、`IsSearchOnEnterEnabled` 和 `SearchRequested`，并在模板内部使用 `AutoCompleteSearchEditBox`。`AutoCompleteSearchEditBox` 继承 SearchEdit 并把 `StyleKeyOverride` 指向 SearchEdit，使搜索输入视觉与搜索请求语义保持一致。AutoComplete 的候选项、popup、异步加载和选择状态不属于 SearchEdit 控件职责。

## 9. 文档导航与验证策略

关联文档：

- [SearchEdit 桌面版实现原理](implementation.md)
- [SearchEdit Changelog](changelog.md)
- [LineEdit 桌面版架构设计](../line-edit/overview.md)
- [AtomUI Control Token 设计规范](../../../../engineering/development/control-token-guidelines.md)

路由说明：

- 5 个 Part 的 `SelectorRoute` 全部以 `/template/ .semantic-scope-input-frame` 为锚点，经
  `SearchEditDecoratedBoxTheme.axaml` 里的 `semantic-scope-prefix` / `semantic-scope-suffix` 作用域类收敛到具体节点。
  `button` 的 `RuntimeCreated=true`（节点由 `SearchEditDecoratedBoxTheme.axaml` 的 `PART_RightAddOn` 承载，
  以 public `Button` 为 `ContractType`）。
- `SearchEditTheme.axaml` 另含 `semantic-content`（`Panel`）与 `semantic-placeholder`（`TextBlock`）两个
  **非 Part 的路由/视觉辅助 marker**，它们不注册 descriptor、不生成 Style，也不属于对外语义契约。
  维护时不要把它们当作 Public Part 使用。

Token 说明：

- SearchEdit 当前没有 Own Token，因此没有专属 `token.md`；Token 边界按第 5 节说明独立 SearchEdit identity、完整 Effective Global Token、Button Semantic Part 和显式跨 Control 资源边界。

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| Public API | 搜索按钮属性、`IsOperating`、`IsSearchOnEnterEnabled`、`SearchRequested`、继承文本输入 API 和 Form API。 |
| 状态 | `IsOperating`、Enter 搜索开关、disabled、focus、hover、pressed、error、warning、clear/reveal。 |
| AXAML/Theme | template part、搜索按钮 style、z-index、SizeType、Custom 高度、Borderless/Filled/Outlined/Underlined。 |
| Token | 验证 SearchEdit exact identity、无 Own Token、任意 Global Token 配置、`SearchEditTokenResource` fallback，以及与 `ButtonTokenResource`、`SharedTokenResource` 和 `InputControlFrameTheme` 的显式边界。 |
| Gallery | 走查 SearchEdit 基础、状态、尺寸、Custom、loading、disabled、内部右侧内容和 AutoComplete 搜索示例。 |
| 回归测试 | 运行 SearchEdit 行为与布局测试、LineEdit Gallery 示例测试和 `git diff --check`。 |
