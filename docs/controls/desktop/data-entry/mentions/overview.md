# Mentions 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.Mentions` 的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，候选列表统一交互见 [候选列表统一交互设计](../select/candidate-interaction-design.md)，内部实现原理见 [Mentions 桌面版实现原理](implementation.md)，Token 专项设计见 [Mentions Token 设计](token.md)，设计和契约变化记录见 [Mentions Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `Mentions`，其 `IsPopupPinnedOpen` 公开用于 Gallery 语义预览与诊断；设置为 true 时保持 mention candidate open state 并 relay 到 `PART_Popup`，设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

Mentions 公开 9 个 Semantic Part（`root`、`prefix`、`content`、`placeholder`、`input`、`clear`、`popup.root`、`popup.list`、`popup.listItem`），输入区部件复用 TextArea 的跨嵌套 owner 路由，弹层部件与 AutoComplete 同构；完整 Part 表、Selector 用法与定制边界见 [Mentions Semantic Part 契约](semantic-part.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/Mentions` |
| 控件状态 | Stable |

Mentions 是 AtomUI 桌面数据录入体系中的提及输入控件，用于在多行文本输入中识别触发前缀，展示候选项弹层，并把用户选择的候选项插入到文本中。它以 `TextArea` / `AbstractTextInput` 为逻辑输入层，以 `InputControlFrame` 为输入表面，结合 `Popup`、`CandidateList`、同步/异步数据加载、过滤、Form 和 Token 体系，提供类似 `@user`、`#tag` 的文本提及体验。

Mentions 的职责是编辑单个字符串值，并在触发上下文中完成候选项选择和文本插入。它不负责远程服务协议、权限判断、富文本 token 渲染、结构化 mention 实体存储、消息发送或搜索结果管理。业务层需要保存结构化 mention 信息时，应在 `Value` 文本和业务数据之间建立自己的解析模型。

## 2. 设计语言

Mentions 的设计语言来自文本区域、触发符和候选弹层的组合。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 文本语义 | 用户编辑包含 mention 标记的普通文本。 | `Value`、`DefaultValue`、`PlaceholderText`。 |
| 触发语义 | 输入触发符后进入候选选择上下文。 | `TriggerPrefix`、`CandidateTriggered`、`FilterValue`。 |
| 候选数据 | 候选项可来自本地集合或异步 loader。 | `OptionsSource`、`OptionsAsyncLoader`、`Populating`、`Populated`。 |
| 输入表面 | 输入框边框、背景和反馈状态。 | `StyleVariant`、`Status`、`SizeType`。 |
| 弹层位置 | 候选弹层相对触发位置向上或向下展开。 | `Placement=Top/Bottom`。 |
| 多行输入 | 输入区域可固定行数或自动高度。 | `Lines`、`MinLines`、`MaxLines`、`IsAutoSize`。 |
| 加载状态 | 异步候选加载时弹层展示 loading。 | `IsLoading`、`AsyncLoadDebounce`、`AsyncLoadTimeout`。 |

`Custom` 尺寸不是 Mentions 的第四套专属 Token。它沿用 TextArea 的 Custom size 规则：未显式设置时以 `Middle` 为视觉基线，用户可通过常规尺寸属性覆盖实际大小。

## 3. API 与契约模型

Mentions 的公共 API 由文本值、触发符、候选数据、过滤、弹层、输入表面和 Form 集成组成。属性不应被机械理解为独立功能；它们共同描述 mention 输入的状态机。

文本与输入 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `Value` | `string?` | 当前文本值，默认 `TwoWay` 绑定到内部 `MentionTextArea.Text`，并接入 Avalonia `DataValidationErrors`。 |
| `DefaultValue` | `string?` | 初始化时 `Value` 为空时写入的默认文本。 |
| `PlaceholderText` | `string?` | 空文本状态的占位提示。 |
| `IsReadOnly` | `bool` | 只读状态，内部文本区域不可编辑。 |
| `IsAllowClear` / `ClearIcon` | `bool` / `PathIcon?` | 清除按钮入口和图标。 |
| `IsAutoFocus` | `bool` | 控件加载后请求内部文本区域焦点。 |
| `IsAutoSize` | `bool` | 内部 TextArea 根据内容自动调整高度。 |
| `Lines` / `MinLines` / `MaxLines` | `int` | 多行输入显示行数和自动高度边界。 |

候选与触发 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `TriggerPrefix` | `IList<string>?` | mention 触发前缀。当前触发识别只支持长度为 1 的字符串。默认值为 `["@"]`。 |
| `Split` | `string?` | 插入候选项时使用的分隔符。为空时默认用空格包裹 mention 文本。 |
| `OptionsSource` | `IEnumerable<IMentionOption>?` | 本地候选项集合。集合变更会同步到内部缓存和候选视图。 |
| `OptionsAsyncLoader` | `IMentionOptionsAsyncLoader?` | 异步候选加载器。存在时，触发候选弹层会优先使用异步加载。 |
| `OptionTemplate` | `IDataTemplate?` | 候选项显示模板。默认模板显示 `IMentionOption.Header`。 |
| `Filter` | `IValueFilter?` | 候选过滤器。为空时使用 contains 过滤。 |
| `FilterValueSelector` | `DefaultFilterValueSelector?` | 从候选项提取过滤文本的 selector。 |
| `DisplayCandidateCount` | `int` | 弹层可显示的候选项数量，用于计算最大高度。默认 `10`。 |

弹层与异步 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `Placement` | `MentionsPlacementMode` | 候选弹层相对触发位置的展开方向，默认 `Bottom`。 |
| `IsDropDownOpen` | `bool` | 候选弹层打开状态。 |
| `ShouldUseOverlayPopup` | `bool` | 是否使用 overlay popup 宿主，默认 `true`。 |
| `AsyncLoadDebounce` | `TimeSpan` | 异步加载前的防抖时间，默认 `TimeSpan.Zero`，值必须非负。 |
| `AsyncLoadTimeout` | `TimeSpan` | 异步加载超时时间，默认 `10` 秒。 |
| `IsLoading` | `bool` | 异步或事件填充过程中的内部 loading 状态，只读公开。 |

输入表面 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `SizeType` | `CustomizableSizeType` | 输入尺寸密度。 |
| `StyleVariant` | `InputControlStyleVariant` | 输入表面样式。 |
| `Status` | `InputControlStatus` | 显式输入反馈状态；最终视觉由 `InputControlFrame.EffectiveStatus` 计算，native validation error 以 `DataValidationErrors` 为唯一真源。 |
| `ContentLeftAddOn` / `ContentRightAddOn` | `object?` | 内部左右附加内容，传递给内部 `MentionTextArea`。 |
| `ContentLeftAddOnTemplate` / `ContentRightAddOnTemplate` | `IDataTemplate?` | 内部左右附加内容模板。 |
| `IsMotionEnabled` | `bool` | 内部输入壳体、候选列表和 popup 动效开关。 |

候选项契约：

| 类型 | 语义 |
| --- | --- |
| `IMentionOption.Key` | 候选项稳定 key，可作为过滤候选值回退。 |
| `IMentionOption.Header` | 候选项显示内容，默认模板显示该值。 |
| `IMentionOption.Value` | 插入文本时优先使用的值。 |
| `IMentionOption.IsEnabled` | 候选项启用状态，供候选列表视觉和选择逻辑使用。 |

事件契约：

| 事件 | 语义 |
| --- | --- |
| `CandidateTriggered` | 触发符被识别时触发，事件参数包含触发字符。 |
| `Populating` | 候选弹层准备填充前触发，可取消默认填充并由业务手动更新候选集合。 |
| `Populated` | 候选视图刷新后触发，事件参数包含只读候选视图。 |
| `OptionsLoaded` | 异步 loader 完成后触发，包含加载结果或错误状态。 |
| `DropDownOpening` / `DropDownClosing` | 弹层打开/关闭前触发，可取消。 |
| `DropDownOpened` / `DropDownClosed` | 弹层打开/关闭后触发。 |

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_TextArea` | `MentionTextArea` | 文本输入、触发符识别、过滤值同步和候选插入。 |
| `PART_Popup` | `Popup` | 候选弹层宿主。 |
| `PART_CandidateList` | `ICandidateList` / `CandidateList` | 候选项展示、键盘导航、提交和取消。 |
| `PopupFrame` | `Border` | 候选弹层背景、圆角、宽度、高度和 padding。 |
| `LoadingIndicator` | `Spin` | 异步候选加载状态展示。 |

`EmptyIndicator`、`EmptyIndicatorTemplate`、`IsShowEmptyIndicator` 和 `EmptyIndicatorPadding` 是当前 public surface 的一部分；默认 `MentionsTheme` 尚未把这些属性绑定到候选弹层视觉结构中，维护时不能把它们描述为已经稳定生效的默认视觉契约。

## 4. 行为与状态模型

Mentions 的核心状态流：

```text
Value / DefaultValue
      ↓
MentionTextArea.Text
      ↓
Text input + caret position
      ↓
TriggerPrefix scan
      ↓
FilterValue + CandidateOpenRequest
      ↓
Populate / async load / filter
      ↓
CandidateList view
      ↓
Commit option
      ↓
Insert mention text back into Value
```

触发符识别从当前 `CaretIndex` 向前扫描，遇到空白或控制字符停止。只有长度为 1 的 `TriggerPrefix` 项会被识别。触发后，`MentionTextArea` 计算触发字符位置和过滤文本，并向外请求打开候选弹层。

候选填充优先级：

```text
OptionsAsyncLoader != null
  → async LoadAsync(FilterValue, token)
  → OptionsLoaded
  → OptionsSource = result.Data
  → PopulateComplete()

OptionsAsyncLoader == null
  → Populating(FilterValue)
  → if !Cancel PopulateComplete()
```

弹层状态优先级：

```text
Disabled / invisible / window deactivated
> DropDownClosing cancellation
> DropDownOpening cancellation
> IsDropDownOpen
> Candidate trigger state
```

键盘行为：

- 弹层打开时，按键优先交给 `CandidateList.HandleKeyDown()`。
- `Escape` 取消候选并关闭弹层。
- `Enter` 在弹层打开时提交当前候选。
- `F4` 切换弹层打开状态。
- 弹层关闭时，`Down` 可打开弹层，除非该按键被 XY focus 导航占用。

Form 集成以 `Value` 作为表单值。`Value` 是用户拥有的受控文本值，默认双向绑定；错误校验状态通过 `DataValidationErrors` 投射到 shared `InputControlFrame.EffectiveStatus`，驱动外层输入表面和内部 `MentionTextArea`；`NotifyValidateStatus` 只同步 warning、success、validating 等 Form 扩展状态。Form feedback 控件传递给内部 `MentionTextArea`。

## 5. 视觉与主题模型

Mentions 的默认视觉由 `MentionsTheme.axaml` 和内部 TextArea 主题协作：

| 主题或资源 | 职责 |
| --- | --- |
| `MentionsTheme.axaml` | 根模板、`MentionTextArea`、popup、loading、候选列表和 MentionsToken 资源绑定。 |
| `TextAreaTheme.axaml` | 输入表面、placeholder、清除按钮、Form feedback、多行高度和状态视觉。 |
| `CandidateList` 主题 | 候选项容器、选中项、键盘导航和提交取消事件。 |
| `PopupHostToken` | popup 阴影、圆角和 anchor margin。 |
| `MentionsToken` | popup 内容 padding、候选项高度和最小宽度。 |

输入框本体不由 Mentions 自己重写边框和文本 presenter，而是通过内部 `MentionTextArea` 复用 TextArea 的 `AbstractTextInput` 与 `InputControlFrame` 体系。Mentions 的控件级 Token 只服务候选弹层，不承载文本高度、输入边框、状态颜色或 Form feedback。

## 6. 控件家族或集成关系

Mentions 属于 Data Entry 文本输入控件家族，与 LineEdit/TextArea 共享输入表面、状态、尺寸、清除按钮和 Form 语义。

集成关系：

- `MentionTextArea` 继承 `TextArea`，复用多行输入能力，并增加 trigger scan、过滤值同步和候选插入。
- `Popup` 提供候选弹层宿主，可通过 `ShouldUseOverlayPopup` 选择 overlay 宿主。
- `CandidateList` 提供候选项展示、键盘导航、提交和取消。
- `AsyncSearchLoadCoordinator` 协调异步加载、跳过过期结果、取消和超时。
- `IFormItemAware` / `IFormItemFeedbackAware` 将 `Value`、Form 扩展状态和 feedback 接入 Form；error 由 `DataValidationErrors` 投射到 shared `InputControlFrame` 和内部输入框。
- `MentionsToken` 定义候选弹层尺寸相关主题值。

## 7. 兼容性不变量

维护 Mentions 时必须保持以下不变量：

- `Value` 必须继续与内部 `MentionTextArea.Text` 双向同步。
- `DefaultValue` 只在初始化且 `Value` 为空时写入。
- `TriggerPrefix` 默认值为 `["@"]`，当前触发识别只支持单字符前缀。
- 触发扫描不能跨越空白或控制字符。
- `OptionsSource` 集合变化必须同步到内部缓存和候选视图。
- `OptionsAsyncLoader` 存在时必须优先走异步加载，并通过 `OptionsLoaded` 报告结果。
- `AsyncLoadDebounce` 变化必须释放旧 timer，避免重复填充。
- 重新套用模板必须解绑旧 `MentionTextArea`、旧 `Popup` 和旧 `CandidateList` 事件。
- 弹层打开期间的可见性、启用状态和窗口失活必须关闭弹层。
- 候选提交必须通过 `MentionTextArea.InsertMentionOption()` 写回文本，保持 undo/redo 快照和 caret/selection 语义。
- MentionsToken 不承载输入文本、过滤值、候选数据、loading、Form 状态或交互状态。

## 8. 专项模型

### 8.1 候选过滤模型

Mentions 将 `OptionsSource` 缓存为 `List<IMentionOption>`，再根据 `FilterValue` 构造当前候选视图。默认过滤器是 contains。`FilterValueSelector` 存在时使用 selector 返回值；否则按 `Header`、`Value`、`Key` 的顺序选择过滤文本。

过滤过程通过 `_filterInAction` 和 `_cancelRequested` 避免重入时生成过期视图。刷新完成后，`CandidateList` 从当前有效视图建立 active candidate；鼠标移动和键盘导航共享同一候选状态，`Enter` 使用该状态插入 mention。过滤、异步结果替换或候选列表回收时，旧候选必须失效，不能由 pointer-over 视觉继续保留。

### 8.2 异步加载模型

`OptionsAsyncLoader` 接收当前过滤值和 cancellation token。`AsyncSearchLoadCoordinator` 负责超时、取消、跳过过期结果和错误状态归一。成功结果会写入 `OptionsSource` 并继续走 `PopulateComplete()`；失败结果只通过 `OptionsLoaded` 通知业务层。

### 8.3 候选插入模型

提交候选时，控件优先使用 `option.Value`，其次使用 `option.Header`。插入逻辑回溯到当前 mention 触发符位置，选中触发符到当前 caret 的文本片段，然后按 `Split` 或空格规则生成插入文本。

`Split` 为单字符时，插入文本以 split 包裹；未设置 split 时，插入文本默认以空格分隔，避免 mention 与相邻普通文本粘连。

### 8.4 Popup 定位模型

`Placement=Bottom` 映射为 `PlacementMode.BottomEdgeAlignedLeft`，`Placement=Top` 映射为 `PlacementMode.TopEdgeAlignedLeft`。打开候选弹层时，控件根据触发字符 bounds 和 text presenter bounds 计算 popup 偏移，使候选弹层对齐当前触发位置。

## 9. 文档导航与验证策略

关联文档：

- [Mentions 桌面版实现原理](implementation.md)
- [Mentions Semantic Part 契约](semantic-part.md)
- [Mentions Token 设计](token.md)
- [Mentions Changelog](changelog.md)
- [LineEdit Token 设计](../line-edit/token.md)

完整 Part 表、SelectorRoute 与定制边界见 [Mentions Semantic Part 契约](semantic-part.md)。

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| Public API | `Value`、`TriggerPrefix`、候选数据、异步 loader、弹层、输入表面、Form 和事件契约。 |
| 状态 | 触发符识别、过滤值、弹层打开/关闭、loading、disabled、readonly、error、warning、auto-size。 |
| AXAML/Theme | `PART_TextArea`、`PART_Popup`、`PART_CandidateList`、loading、popup shadow、SizeType、StyleVariant。 |
| Token | `PopupContentPadding`、`OptionHeight`、`MinPopupWidth` 的默认值、生成 TokenKind、Token 类型、生成数据和主题引用。 |
| Gallery | 走查基础、variant、异步加载、自定义触发符、disabled/read-only、placement、status、auto-size 和 clear 示例。 |
| 回归测试 | `MentionsShowCasePageTests`、相关控件测试和 `git diff --check`。 |
