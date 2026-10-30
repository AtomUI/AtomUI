# Mentions 桌面版实现原理

本文档描述 Mentions 桌面版的内部模板组合、触发符识别、候选弹层、同步/异步加载、过滤、候选插入、Form 和 Token 资源边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计与 API 契约见 [Mentions 桌面版架构设计](overview.md)，候选列表状态契约见 [候选列表统一交互设计](../select/candidate-interaction-design.md)，Token 语义见 [Mentions Token 设计](token.md)，变化记录见 [Mentions Changelog](changelog.md)。

Popup 接入边界：`Mentions` 负责业务状态和内容准备，internal Popup 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

Mentions 的实现以 `MentionTextArea` 为输入内核，`Popup` 和 `CandidateList` 为候选选择层，`Mentions` 本体负责状态编排、数据加载、过滤、弹层生命周期和 Form 集成。实现文档聚焦 Mentions 自身的状态流，不重新说明 TextArea 的文本布局、选择渲染和输入表面细节。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Mentions/Mentions.cs`：public API、状态流、候选加载、过滤、弹层生命周期、键盘处理和 Form 接口。
- `src/AtomUI.Desktop.Controls/Mentions/Mentions.SemanticParts.cs`：Mentions 的 9 个 Semantic Part 声明（见 [Mentions Semantic Part 契约](semantic-part.md)）。
- `src/AtomUI.Desktop.Controls/Mentions/MentionTextArea.cs`：内部 TextArea，负责触发符扫描、过滤值同步和候选插入。
- `src/AtomUI.Desktop.Controls/Mentions/MentionOption.cs`：候选项接口和默认 record。
- `src/AtomUI.Desktop.Controls/Mentions/DataLoad/IMentionOptionsAsyncLoader.cs`：异步候选加载接口。
- `src/AtomUI.Desktop.Controls/Mentions/DataLoad/MentionOptionsLoadResult.cs`：异步加载结果。
- `src/AtomUI.Desktop.Controls/Mentions/*EventArgs.cs`：候选触发、填充、加载和选项事件参数。
- `src/AtomUI.Desktop.Controls/Mentions/MentionsPlacementMode.cs`：弹层展开方向。
- `src/AtomUI.Desktop.Controls/Mentions/MentionPseudoClass.cs`：`:candidateopen` 伪类常量。
- `src/AtomUI.Desktop.Controls/Mentions/MentionsToken.cs`：候选弹层尺寸 Token。
- `src/AtomUI.Desktop.Controls/Mentions/Themes/MentionsTheme.axaml`：Mentions 模板、popup、loading 和候选列表视觉。

## 3. 核心类职责

`Mentions` 是状态协调器。它不直接编辑文本 run，也不渲染候选项；它接收 `MentionTextArea` 的候选打开/关闭请求，协调本地/异步候选数据，维护候选视图，控制 popup 生命周期，并把候选提交结果写回内部文本区域。

`MentionTextArea` 是文本输入和 trigger 检测边界。它继承 `TextArea`，复用 `AbstractTextInput` 的多行输入、清除、状态、Form feedback、尺寸和 `InputControlFrame` 输入表面，并增加 `TriggerPrefix`、`FilterValue`、`IsDropDownOpen`、`CandidateOpenRequest` 和 `CandidateCloseRequest`。

`CandidateList` 是候选选择边界和 active candidate owner。Mentions 只依赖 `ICandidateList` 的 `ItemsSource`、`SelectedItem`、`Commit`、`Cancel` 和 `HandleKeyDown()`，不直接管理候选项容器；鼠标候选迁移、键盘导航和 `Enter` 提交必须最终读取同一 active candidate。

`IMentionOptionsAsyncLoader` 是异步数据边界。控件只调用 `LoadAsync(context, token)`，不假设远程协议、缓存策略或错误显示方式。

## 4. 状态与数据流

初始化流：

```text
OnInitialized
  → if Value == null, Value = DefaultValue
  → if TriggerPrefix == null, TriggerPrefix = ["@"]
  → if ClearIcon == null, ClearIcon = CloseCircleFilled
  → ConfigurePopupPlacement()
```

文本触发流：

```text
MentionTextArea.Text / CaretIndex changed
  → scan backward until whitespace/control char
  → find one-character TriggerPrefix
  → compute trigger bounds and predicate
  → CandidateOpenRequest / CandidateCloseRequest
```

候选打开流：

```text
CandidateOpenRequest
  → CandidateTriggered
  → compute popup offsets
  → IsDropDownOpen = true
  → DropDownOpening
  → OpenDropDown
  → debounce timer or PopulateDropDown
```

候选填充流：

```text
PopulateDropDown
  → TryPopulateAsync(FilterValue)
  → else Populating(FilterValue)
  → if !Cancel PopulateComplete()
  → RefreshView()
  → Populated(view)
```

Form 流：

```text
Form.SetValue(object?) → Value
Value changed          → IFormItemAware.ValueChanged
Form.GetValue()        → Value
Form.ClearValue()      → Value = null
DataValidationErrors   → InputControlFrame.EffectiveStatus
ValidateStatus         → FormStatus → InputControlFrame + FormFeedback
FeedbackControl        → FormFeedback
```

## 5. 生命周期与模板接入

`Mentions.OnApplyTemplate()` 必须解除旧模板 part 的事件订阅，再接入新 part：

- 旧 `_textArea` 解除 `CandidateOpenRequest` 和 `CandidateCloseRequest`。
- 旧 `_popup` 解除 `Opened` 和 `Closed`。
- `CandidateList` setter 解除旧列表的 `Commit`、`Cancel` 和 `ItemsSource`，再接入新列表。
- 新 `MentionTextArea` 设置 `Owner=this`。
- 新 `Popup` 设置 `OverlayInputPassThroughElement=_textArea`。
- 每次模板接入后重新调用 `ConfigurePopupPlacement()`。

`OnAttachedToVisualTree()` 订阅 TopLevel deactivation，窗口失活时关闭弹层。`OnDetachedFromVisualTree()` 释放该订阅。

Popup 打开时创建 `_subscriptionsOnOpen`，订阅自身 `IsVisible`、`IsEnabled` 和祖先可见性变化；Popup 关闭时释放这些订阅。控件不可见、禁用或祖先不可见时，必须关闭弹层。

`AsyncLoadDebounce` 变化时必须停止旧 `DispatcherTimer`、解除旧 Tick 订阅并清空字段，然后根据新值创建新 timer。

## 6. 交互与事件处理

键盘交互：

- `CandidateList.HandleKeyDown()` 优先处理弹层打开状态下的按键。
- `Escape` 取消当前候选，清空 `FilterValue` 并关闭弹层。
- `Enter` 在弹层打开时提交 `CandidateList.SelectedItem`。
- `F4` 切换 `IsDropDownOpen`。
- 弹层关闭时，`Down` 打开弹层；若该按键被 XY focus 导航占用，则不打开。

指针交互：

- `MentionTextArea` 的 pointer pressed/released 转交给 owner。
- 点击 popup 内部时保持弹层。
- 弹层打开时点击非文本区域会关闭弹层。
- 普通按下状态通过标准 `:pressed` 伪类表达。

候选项的 `:pointerover` 只表示指针命中事实，不单独绘制候选 active 背景。候选列表把鼠标移动到可用项转换为 active candidate 迁移；键盘路径复用同一 owner，且只有键盘路径负责滚动到可见区域。

候选提交：

```text
CandidateList.Commit
  → if SelectedItem is IMentionOption
  → InsertCandidateOption(option)
  → IsDropDownOpen = false
  → focus MentionTextArea
```

候选取消：

```text
CandidateList.Cancel / Escape
  → FilterValue = null
  → IsDropDownOpen = false
  → focus MentionTextArea
```

## 7. 内部算法与关键流程

### 7.1 触发符扫描

`MentionTextArea.CheckTriggerState()` 从当前 caret 向前扫描。遇到空白或控制字符立即停止。扫描到单字符 `TriggerPrefix` 后，控件记录触发字符、触发 bounds 和过滤文本，并进入 `TriggerState=true`。

`HandleCaretIndexChanged()` 使用相同规则计算 `FilterValue`。当 caret 离开 mention 片段或文本为空时，`FilterValue` 被置空。

### 7.2 Popup 定位

打开候选时，Mentions 通过 `MentionTextArea.GetTextPresenterBounds()` 获取文本 presenter 在输入框中的偏移，再叠加触发字符 bounds 计算 popup 偏移。

`Placement=Bottom` 时，popup 以触发字符下方为参考；`Placement=Top` 时，popup 以触发字符上方为参考。`ConfigurePopupPlacement()` 将 Mentions 自己的 placement 枚举映射为 Avalonia `PlacementMode`。

### 7.3 视图缓存与过滤

`OptionsSource` 变化时，控件构建 `_items` 缓存。若集合实现 `INotifyCollectionChanged`，控件使用弱订阅同步 add/remove/replace/reset。

`RefreshView()` 根据 `_items`、`FilterValue`、`Filter` 和 `FilterValueSelector` 生成 `_view`。过滤运行中再次触发刷新时，通过 `_cancelRequested` 退出当前过滤，避免过期结果覆盖新视图。

过滤匹配文本的选择顺序：

```text
FilterValueSelector(option)
  else option.Header
  else option.Value
  else option.Key
```

### 7.4 异步加载

`TryPopulateAsync()` 在存在 `OptionsAsyncLoader` 时设置 `_asyncLoadCoordinator.Timeout`，将 `IsLoading` 置为 true，并启动 `PopulateAsync()`。

`PopulateAsync()` 通过 `AsyncSearchLoadCoordinator` 调用 loader。被跳过的旧结果不会更新 UI。成功结果触发 `OptionsLoaded`，再在 UI 线程写入 `OptionsSource` 并调用 `PopulateComplete()`。超时、取消和未知错误会映射到 `RpcStatusCode.Timeout`、`Cancelled` 或 `Unknown`，并通过 `OptionsLoaded` 通知业务层。

### 7.5 候选插入

`MentionTextArea.InsertMentionOption()` 先创建 undo/redo 快照，再从 caret 向前找到当前触发符位置，并选中触发符到 caret 的文本区间。插入值优先使用 `option.Value`，其次使用 `option.Header`。

`Split` 为单字符时，插入值以 split 和触发符包裹；没有 split 时，插入值使用空格分隔。该流程保证 mention 文本不会与前后普通文本直接粘连。

### 7.6 Popup 高度计算

`ConfigureMaxPopupHeight()` 使用：

```text
MaxPopupHeight = ItemHeight * DisplayCandidateCount
               + PopupContentPadding.Top
               + PopupContentPadding.Bottom
```

`ItemHeight` 和 `PopupContentPadding` 来自 `MentionsToken`，`DisplayCandidateCount` 是实例属性。

## 8. 资源、性能与 AOT 边界

Mentions 不依赖运行时反射发现模板结构。模板协作通过固定 template part、接口、事件和 owner 引用完成。

资源和生命周期边界：

- `_collectionChangeSubscription` 使用弱订阅，但 `OptionsSource` 变化时仍必须 dispose。
- `_subscriptionsOnOpen` 只在 popup 打开期间存在，popup 关闭时必须释放。
- `_deactivationSubscription` 必须在 detach 时释放。
- `_delayTimer` 更换时必须停止旧 timer 并解除 Tick。
- `CandidateList` 替换时必须清空旧列表 `ItemsSource`，避免旧视图保留。
- 异步加载通过 `AsyncSearchLoadCoordinator` 处理超时、取消和跳过旧结果，不在控件里保留业务任务。

AOT 边界：

- `MentionsToken` 通过 token generator 显式注册，生成 `MentionsTokenKind` 和 `MentionsTokenResourceExtension`。
- API 与 Token 契约由控件文档、源码 public surface、Token 类型或生成数据维护。
- `OptionTemplate` 和 `EmptyIndicatorTemplate` 是 XAML 模板入口，不依赖运行时成员扫描。

## 9. 维护不变量

内部重构必须保持以下不变量：

- `Value` 和 `MentionTextArea.Text` 继续双向同步；`Value` 作为 Form 值必须默认 `TwoWay` 并启用 Avalonia 数据验证。
- `TriggerPrefix` 默认值、单字符识别和空白边界不能改变。
- `FilterValue` 必须随 caret 和文本变化更新。
- 打开候选弹层必须先触发 `CandidateTriggered`，再走打开和填充流程。
- 弹层打开/关闭必须尊重 `DropDownOpening` / `DropDownClosing` 的取消结果。
- 异步 loader 结果不能让过期请求覆盖当前候选视图。
- 候选提交必须使用 `MentionTextArea.InsertMentionOption()`，保留 undo/redo 和 selection 语义。
- 重新套用模板不能泄漏旧 part 的事件订阅。
- MentionsToken 只服务 popup 尺寸，不承载候选数据、过滤值、loading 或输入状态。
- Semantic Part marker 的维护边界：`MentionsTheme.axaml` 承载 `semantic-scope-input`（`MentionTextArea` 节点）、`semantic-popup-root`（`PopupFrame` Border）、`semantic-popup-list`（`PART_CandidateList`）静态 marker；`prefix` / `content` / `placeholder` / `input` / `clear` 的 marker 位于共享 `TextAreaTheme.axaml` 与 `TextAreaDecoratedBoxTheme.axaml`（`CrossNestedOwners=true`，生成器沿 `MentionTextArea` → `TextArea` 主题链校验）；`popup.listItem` 的 marker 由共享 `CandidateList.CreateContainerForItemOverride()` 注入（`RuntimeCreated=true`）。marker 随容器实例创建一次，prepare/clear/recycle 路径不得增删。

## 10. 测试与验证

验证范围：

- `MentionsShowCasePageTests` 覆盖 Gallery 页面结构、示例 snapshot、源码片段和延迟加载规则。
- Gallery 走查基础用法、variant、异步加载、自定义触发符、disabled/read-only、placement、status、auto-size 和 clear 示例。
- 控件行为验证应覆盖触发符扫描、候选过滤、异步加载成功/失败/超时、键盘导航、候选提交、Form value 和模板重应用事件释放。
- `MentionsSemanticPartTests`：descriptor 九个 Part 契约、模板静态 marker 清单、默认主题不消费 semantic selector、生成 Style 命中输入区目标、popup 部件 marker 暴露。
- Token 改动需验证 `MentionsTokenKind`、`MentionsTokenResourceExtension`、`MentionsTheme.axaml` 引用和 Token 类型、生成数据和 token.md 语义说明一致。
- 文档改动运行 `git diff --check`，并检查相对链接存在。
