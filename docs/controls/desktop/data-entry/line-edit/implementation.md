# LineEdit 桌面版实现原理

本文档描述 LineEdit 桌面版输入家族的内部状态同步、模板接入、清除/reveal/字数统计、SearchEdit、TextArea resize、Form 和 CompactSpace 集成。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计与 API 契约见 [LineEdit 桌面版架构设计](overview.md)，公开主题区域见 [LineEdit Semantic Part 契约](semantic-part.md)，Token 语义见 [LineEdit Token 设计](token.md)，变化记录见 [LineEdit Changelog](changelog.md)。

## 1. 实现定位

LineEdit 家族的实现以 Avalonia `TextBox` 为文本编辑内核，AtomUI 分为 `AbstractTextInput` 逻辑层和 `InputControlFrame` 视觉层：前者负责文本输入扩展、状态归一和生命周期，后者负责输入表面和有效状态渲染。实现文档聚焦 AtomUI 增强层，不重新说明 Avalonia 文本编辑、选择和滚动算法。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Input/AbstractTextInput.cs`：文本输入逻辑基类，统一 `StyleVariant`、`Status`、`SizeType`、清除、reveal、字数统计、Form、native validation、CompactSpace 和 viewport metrics。
- `src/AtomUI.Desktop.Controls/Primitives/InputControlFrame.cs`：internal 输入表面组合控件，统一边框、背景、圆角、shadow、交互状态、有效状态和 CompactSpace 视觉。
- `src/AtomUI.Desktop.Controls/Input/TextBox.cs`：AtomUI 基础 TextBox，继承 `AbstractTextInput`，提供基础文本编辑模板。
- `src/AtomUI.Desktop.Controls/Input/LineEdit.cs`：标准单行输入框，继承 `AbstractTextInput`，提供外部 AddOn、内部右侧内容绑定和单行布局。
- `src/AtomUI.Desktop.Controls/Input/LineEdit.SemanticParts.cs`：LineEdit 的 `prefix`、`input`、`suffix`、`clear`、`count` Semantic Part 声明；`root` 由 generator 隐式补齐。
- `src/AtomUI.Desktop.Controls/Input/SearchEdit.cs`：搜索输入框，提供搜索按钮样式、搜索按钮文本、加载态和搜索点击事件。
- `src/AtomUI.Desktop.Controls/Input/TextArea.cs`：多行输入框，继承 `AbstractTextInput`，提供固定行数、自动高度和 resize。
- `src/AtomUI.Desktop.Controls/Input/InputTextPresenter.cs`：输入文本 presenter，处理 Avalonia 12 selection foreground 缓存刷新。
- `src/AtomUI.Desktop.Controls/Input/SearchEditDecoratedBox.cs`：SearchEdit 输入壳体与搜索按钮协作。
- `src/AtomUI.Desktop.Controls/Input/TextAreaDecoratedBox.cs`：TextArea 输入壳体、scroll viewer 和 resize 相关协作。
- `src/AtomUI.Desktop.Controls/Input/TextViewportMetrics.cs`：输入控件向同程序集消费方发布有效文本 viewport 宽度的内部度量契约。
- `src/AtomUI.Desktop.Controls/Input/ResizeHandle.cs`：TextArea resize 拖拽入口。
- `src/AtomUI.Desktop.Controls/Input/TextBoxToken.cs`：TextBox 专属内容 padding Token。
- `src/AtomUI.Desktop.Controls/Input/LineEditToken.cs`：单行输入字号 Token。
- `src/AtomUI.Desktop.Controls/Input/TextAreaToken.cs`：TextArea 字号、右侧 padding 和 resize Token。
- `src/AtomUI.Desktop.Controls/Input/Themes/*.axaml`：TextBox、LineEdit、SearchEdit、TextArea 和内部按钮主题。
- `src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/Themes/AddOnDecoratedBoxTheme.axaml`：提供 LineEdit 生成 Style 穿过输入 frame 模板所需的内部 prefix / suffix route scope。

## 3. 核心类职责

`AbstractTextInput` 是文本输入逻辑唯一 owner。它统一维护 `StyleVariant`、`Status`、`SizeType`、`IsMotionEnabled`、清除/reveal/count、placeholder、Form value、native validation 接入、FormStatus、CompactSpace 和 `TextViewportMetrics`，并负责把 owner 的 root `Background` / `BorderBrush` 中继到 frame（见第 5 节）。`TextBox`、`LineEdit`、`TextArea` 只补充单行、多行或 AddOn 专用结构，不重复声明这些共有状态。

`InputControlFrame` 是输入表面视觉唯一 owner。它从 `AbstractTextInput` 接收已经归一化的 `StyleVariant`、`SizeType`、`IsMotionEnabled`、`Status`、`FormStatus`、`DataValidationErrors`、`IsEnabled`、focus/hover/pressed 和 CompactSpace 输入，计算 `EffectiveStatus`，并渲染边框、背景、圆角、shadow、error/warning、disabled 与 motion。它不拥有文本值，也不负责 AddOn 内容布局；`DataValidationErrors` 仍是 frame 上的 native validation 唯一真源，不在 frame 外形成第二套验证源。

`AddOnDecoratedBox` 继承 `InputControlFrame`，只增加 AddOn、内部内容和布局扩展；`SearchEditDecoratedBox`、`TextAreaDecoratedBox` 以及 Select/TreeSelect/Cascader/ButtonSpinner 的专用 decorated box 继续复用该 frame 状态模型，不得重新计算输入表面状态。

`SearchEdit` 在 `LineEdit` 基础上把搜索按钮加入输入壳体。搜索按钮和 Enter 键统一调用 `RaiseSearchRequested()`，再由控件抛出包含查询文本快照和触发来源的 `SearchRequested` 路由事件。

`TextArea` 继承 `AbstractTextInput`，因为多行输入需要不同模板、scroll viewer 接入、固定行数测量和 resize 流程。它直接复用基类状态、尺寸、清除、字数统计和 Form 模型。

`TextViewportMetrics` 是不对用户公开的响应式度量通道。TextBox/TextArea 仍是自身模板结构和文本可视区域的唯一 owner；消费方只能读取发布后的有效宽度，不能获得 `TextPresenter`、`ScrollViewer` 或 NameScope，也不能反向遍历输入控件的 VisualTree。

## 4. 状态与数据流

文本值流：

```text
User input / Form.SetValue
      ↓
Text
      ↓
TextChanged class handler
      ↓
CountText + Form ValueChanged
```

清除按钮流：

```text
IsAllowClear / IsReadOnly / Text / AcceptsReturn
      ↓
IsEffectiveShowClearButton
      ↓
PART_ClearButton.IsVisible
      ↓
Click → Clear()
```

Form 与有效状态流：

```text
Form.SetValue(object?) → Text
TextChanged            → IFormItemAware.ValueChanged
Form.GetValue()        → Text
Form.ClearValue()      → Text = null
DataValidationErrors   → InputControlFrame
Form error             → Form-owned DataValidationErrors entry
Form warning/status    → FormStatus → InputControlFrame
Status                 → InputControlFrame
EffectiveStatus        → Native Error / Form Error > Form Warning > Explicit Warning > Explicit Error > Default
Form feedback control  → FormFeedback + IsFormFeedbackVisible
```

文本 viewport 度量流：

```text
TextBox/TextArea own template parts
      ↓
ScrollViewer.Viewport / Padding + TextPresenter.Margin
      ↓
TextViewportMetrics.ViewportWidth
      ↓
OverflowTip and other internal consumers
```

状态伪类：

- `DataValidationErrors` 是 native error 的唯一真源，并通过模板绑定投射到 frame。
- Form error 由 Form 写入自己的 native validation entry，Form reset 只撤销该 entry。
- Form warning、success、validating 进入 `FormStatus`，不修改用户显式 `Status`；最终是否遮蔽显式状态由 `EffectiveStatus` 优先级决定。
- `Status=Error/Warning` 只形成 `ExplicitStatus`，不写入 native error。
- `InputControlFrame` 根据 `EffectiveStatus` 设置 `:error`、`:warning` 及对应主题状态；`:warning` 仅在最终有效状态为 Warning 时存在。
- `StyleVariant=Outlined/Filled/Borderless/Underlined` 由 frame 统一选择器表达。

## 5. 生命周期与模板接入

`AbstractTextInput.OnInitialized` 在未设置 `ClearIcon` 时写入默认 `CloseCircleFilled`，并建立 Form/native validation 与 viewport 度量边界。控件 token scope 和主题 asset 由 generator 显式注册；TextBox、LineEdit、TextArea 的专属 Token 只表达各自文本布局或尺寸差异。

`OnApplyTemplate` 规则：

- 旧清除按钮 click 订阅必须解除，再绑定新 `PART_ClearButton`。
- `AbstractTextInput` 每次套用模板都重新查找第一个 `InputControlFrame` 后代作为 root 表面中继目标，并把共有状态回放到 frame；frame 负责初始 transitions 抑制、边框厚度和 CompactSpace 状态计算。
- root 表面中继：`OnApplyTemplate` 建立缓存后立即同步当前 `Background` / `BorderBrush`；owner 这两个属性变化时，有值以 `BindingPriority.LocalValue` 写入 frame 同名属性（共享 `TemplatedControl` 属性实例，直接 `SetValue`），置空或 `UnsetValue` 时对 frame `ClearValue`。LocalValue 高于 frame 状态机的 StyleTrigger，因此定制期间 hover / pressed / focus 的边框变色让位；focus 的 `BoxShadow` 是独立属性槽，反馈不丢失。中继在模板重新套用时丢弃旧 frame 引用并重建。
- `TextBox` 的 `PART_InputControlFrame` 必须水平填满 TextBox 已分配宽度；placeholder、短文本和长文本只改变内部文本布局，不改变输入外框宽度。
- `TextBox` 模板通过 `TemplateBinding` 将继承的 `InnerLeftContent` / `InnerRightContent` 交给独立的 `ContentPresenter`。左侧 presenter 停靠在编辑区左侧，右侧 presenter 位于 clear/reveal/Form feedback 之后、count 之前；两者均在文本 scroll viewer 外。模板把内容是否为空映射到布局容器的内部 class，ControlTheme 仅在至少一侧内容非空时为 `AdaptiveSpacingDockPanel` 设置 `SpacingXXS`；面板只为可见且具有宽度的辅助区分配间距。两侧内容均为空时撤销间距 setter，保留原有 clear/reveal/Form feedback/count 场景的编辑宽度。空值 presenter 折叠，避免辅助区内部的 `Spacing` 留下空隙。内容的创建、隐式 DataTemplate 查找、替换和释放由 presenter 管理，不增加代码绑定或内容订阅；重新套用模板时由新模板实例接管当前内容。
- `AddOnDecoratedBox` 派生 part 只接收 frame 状态并建立 AddOn/内部内容布局，不再承担独立状态选择器。
- clear/reveal/form/inner-right/count 等稳定 control-to-template 状态由 AXAML `TemplateBinding` 或显式 typed ancestor binding 表达；`AddOnDecoratedBox.ContentRightAddOn` 跨模板边界内的节点使用 `$parent[atom:<InputType>]` 绑定到对应输入控件。
- `SearchEdit` 获取 `SearchEditDecoratedBox` 后设置 `OwningSearchEdit`，由 decorated box 回调搜索事件。
- `TextArea` 获取 `TextAreaDecoratedBox` 后设置 `Owner`，获取 `ResizeHandle` 后设置 `Owner`。
- `TextBox` / `TextArea` 每次套用模板时先释放旧文本 viewport source 订阅，再由当前模板中的 scroll viewer 和 text presenter 发布新的有效宽度；旧 part 后续变化不得再影响控件度量。
- `TextAreaDecoratedBox` 只在自己的 `OnApplyTemplate` 中获取 `PART_ScrollViewer`，再通过直接 owner 协作把 source 交给 `TextArea`。`TextArea` 和外部 behavior 都不得进入 decorated box 的模板查找该 part。
- 模板 part 的 click、preedit 和 viewport 订阅随当前模板实例存续，只在重新套用模板前释放；logical detach 不销毁仍然有效的模板连接。
- Form feedback 属于外部对象订阅：在 `FormFeedback` 变化时替换，在 logical detach 时释放，并在 logical attach 时按当前 feedback 状态重新建立。
- LineEdit Semantic marker 全部由静态 AXAML 模板创建；模板重套用会创建新模板节点，但单个已应用模板内的 marker 不因内容、状态或可见性变化增删。

模板 part 属于主题契约。需要调整内部视觉时，应优先在 AXAML 中维护静态模板和 selector，不把 clear/reveal/search/resize 视觉搬到 C# 动态创建。

## 6. 交互与事件处理

清除按钮点击进入 `NotifyClearButtonClicked()`，默认调用 `Clear()`。派生控件可以重写 hook，但必须保持 `Text`、Form value 和字数统计同步。

密码 reveal 通过 `RevealButton.IsChecked` 与 `RevealPassword` 双向绑定完成。`IsEnableRevealButton` 只控制按钮可见性，不改变 `PasswordChar` 自身语义。

SearchEdit 搜索请求流程：

```text
Search button click
  → SearchEditDecoratedBox
  → SearchEdit.RaiseSearchRequested(Button)
  → if !IsOperating raise SearchRequested

Enter KeyUp when enabled and unhandled
  → mark handled
  → SearchEdit.RaiseSearchRequested(EnterKey)
  → if !IsOperating raise SearchRequested
```

TextArea resize 流程：

```text
Pointer pressed → NotifyAboutToResize()
Pointer moved   → NotifyResizing(delta)
Pointer released→ NotifyResizeCompleted()
```

`NotifyAboutToResize()` 记录原始高度，并根据 `MinLines` / `MaxLines` 和当前 scroll viewer 垂直开销计算可拖拽高度范围。

## 7. 内部算法与关键流程

### 7.1 字数统计

`IsShowCount=true` 时，输入变化更新 `CountText`，格式为 `当前长度 / MaxLength`。`MaxLength` 来自 Avalonia `TextBox`，当业务需要无限长度时仍应评估显示文案是否合理。

### 7.2 CompactSpace 边框厚度

`InputControlFrame` 根据 `CompactSpaceOrientation` 和最终 `StyleVariant` 计算当前边框厚度、圆角折叠和相邻控件连接关系。`AbstractTextInput` 只发布 CompactSpace 状态，不直接读取具体边框节点；非 outlined 表面由 frame 返回无边框折叠值。

### 7.3 TextArea 固定行数测量

TextArea 在 `IsAutoSize=false`、`Lines>0` 且未显式设置 `Height` 时，使用 `TextLayout` 计算指定行数的文本高度，再叠加 scroll viewer 与 text presenter 的垂直空间，写入 scroll viewer 的 `MinHeight` / `MaxHeight`。

### 7.4 TextArea resize 边界

TextArea resize 以控件当前 `Bounds.Height` 为起点。拖拽时高度被限制在 `MinLines` 和 `MaxLines` 对应的 scroll viewer 高度范围内；若 `MaxLines<=0`，最大值为正无穷。

### 7.5 Selection foreground 刷新

`InputTextPresenter` 在 `SelectionStart`、`SelectionEnd`、`SelectionForegroundBrush` 和 `ShowSelectionHighlight` 变化时调用 `InvalidateTextLayout()`，用于避免 Avalonia 12 文本 run 缓存导致选中文本前景色滞留。

### 7.6 文本 viewport 宽度

文本是否在输入表面内可见，必须以 scroll viewer 的 viewport 为起点，不能使用会随文本内容扩展的 `TextPresenter.Bounds.Width`，也不能由消费方使用 `GetVisualDescendants()` 查找 `PART_TextPresenter`。

当前模板的有效宽度按以下语义计算：

```text
effective width = ScrollViewer.Viewport.Width
                - ScrollViewer horizontal padding
                - TextPresenter horizontal margin
```

外壳 padding、border、左右 AddOn、clear/reveal/feedback/count 等布局占用应由 scroll viewer 的实际 viewport 或 padding 反映，不能在 `OverflowTip` 中复制控件模板补偿。内部度量使用三态：`null` 表示当前模板没有精确 source、`NaN` 表示 source 已接入但布局尚未产生有效宽度、正数表示可消费的有效宽度。

### 7.7 Semantic Part 映射

`LineEdit.SemanticParts.cs`、`SearchEdit.SemanticParts.cs` 与 `TextArea.SemanticParts.cs` 分别声明各自的 public
owner。generator 在编译期补齐 root descriptor，并为各 selector Part 生成强类型 Style；`SearchEdit` 的 `button` Part
为 RuntimeCreated 契约，marker 由 `SearchEditDecoratedBox` 在模板应用后追加。Semantic registry 不通过继承或模板扫描
推断 owner。

模板映射：

| Part | 静态节点 | SelectorRoute |
| --- | --- | --- |
| `root` | LineEdit owner | 不适用 |
| `prefix` | `AddOnContentPresenter.semantic-prefix` | `/template/ .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix` |
| `input` | `InputTextPresenter.semantic-input` | `/template/ .semantic-input` |
| `suffix` | `StackPanel.semantic-suffix` | `/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix` |
| `clear` | `InputClearIconButton.semantic-clear` | `/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear` |
| `count` | `TextBlock.semantic-count` | `/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-count` |

LineEdit template 给自有 `AddOnDecoratedBox` 添加 `.semantic-scope-input-frame`；AddOnDecoratedBox template 给内部左右
`AddOnContentPresenter` 添加 `.semantic-scope-prefix` / `.semantic-scope-suffix`。这些 scope 只用于 owner-relative route，
不是公开 Part，也不能由应用直接依赖。prefix 额外使用 internal `AddOnContentPresenter` 包装；它的最低 public 类型仍是
`ContentPresenter`，并通过既有 `UpdateChild()` 路径同时保留 content、template-only 和 `InnerLeftContentTemplate` 语义。
suffix 维持固定 StackPanel，clear/count 通过直接子级 route 收窄命中范围。

所有 marker 都是静态 class。默认主题不得使用 `.semantic-*` selector 消费它们；用户添加生成 Style 时才创建对应 selector
activator。descriptor、Style 类型和 route 都由编译期 generator 产生，不在运行时反射类型、扫描程序集或遍历 VisualTree。

## 8. 资源、性能与 AOT 边界

LineEdit 家族不依赖运行时反射发现模板结构。固定 control-to-template 状态通过稳定 part、`TemplateBinding` 和 typed ancestor binding 表达；运行时协作通过接口和显式 owner 引用完成。

资源和生命周期边界：

- 固定模板状态不创建 C# relay binding；`ContentRightAddOn` 内容边界内使用 typed ancestor binding，生命周期由模板拥有。
- `_feedbackStatusSubscription` 必须在 `FormFeedback` 变化和 logical detach 时释放，并在 logical attach 时重新建立。
- clear button click 订阅必须在新模板接入前解绑旧按钮。
- preedit 和文本 viewport source 订阅必须在新模板接入前释放旧 source；普通 logical detach 不释放当前模板仍需使用的订阅。
- 文本 viewport source 的 `Viewport`、`Padding` 和 presenter `Margin` 订阅必须由 TextBox/TextArea 持有，并在模板重套用时成组替换。
- TextArea resize 不创建全局订阅；拖拽状态保存在控件实例字段中。
- SharedToken 与 frame 主题表达输入表面值；TextBox/LineEdit/TextArea Token 只表达稳定的字体、padding 和 resize 视觉语义，不承载文本值、清除状态、Form 状态或搜索运行状态。

AOT 边界：

- Token 类型通过 generator 显式注册。
- LineEdit Semantic descriptor 与强类型 Style 通过 generator 静态生成，运行时不反射 `SemanticPartAttribute`，也不扫描模板发现 marker。
- API 与 Token 契约由控件文档、源码 public surface、Token 类型或生成数据维护，不依赖运行时反射扫描。
- 文档中描述的 template part 名称应与 AXAML 和 C# 查找代码保持一致。

## 9. 维护不变量

内部重构必须保持以下不变量：

- `TextChanged` 继续驱动字数统计和 Form value changed。
- 清除按钮可见性不在 AXAML 与 C# 中形成相互冲突的状态源。
- TextBox 外框宽度由父布局和 `Width` / `MinWidth` / `MaxWidth` 契约决定，不能随 placeholder 或当前文本的测量宽度伸缩。
- `IsCustomFontSize=true` 不能被 SizeType 字体样式覆盖。
- `InputControlFrame.EffectiveStatus` 必须遵守 Native Error / Form Error > Form Warning > Explicit Warning > Explicit Error > Default；native error 由 `DataValidationErrors` 唯一提供，Form reset 不得清理外部 native error。
- 所有 `*AddOnDecoratedBox` 只能扩展 frame 布局，不能重新定义 variant/status/error/warning/disabled/motion selector。
- `SearchEdit.IsOperating=true` 必须阻止按钮和 Enter 键产生重复搜索请求。
- `TextArea` 的 fixed lines、auto-size 和 resize 不互相覆盖高度状态。
- 重新套用模板不能泄漏旧按钮 click、旧模板 binding、旧 preedit 或旧 viewport source；logical reattach 后当前模板交互与状态绑定必须保持有效，Form feedback 必须重新订阅。
- TextPresenter margin 是输入模板视觉契约；文本有效宽度由输入控件在模板所有权边界内统一计算并发布，不在业务控件或消费 behavior 中加入隐藏补偿。
- 输入控件不得向内部消费方暴露 `TextPresenter` / `ScrollViewer` 实例；模板结构变化只能影响输入控件自己的度量实现。
- `.semantic-scope-input-frame`、`.semantic-scope-prefix`、`.semantic-scope-suffix` 只能服务 LineEdit 的显式 route，不得提升为公开 Part 或默认主题 selector。
- clear、count、prefix、suffix 的可见性和内容变化不得增删 marker；五个 selector Part 在内置 LineEdit 模板中均保持 `Single`。
- 四个输入主题（LineEdit / TextBox / SearchEdit / TextArea）不得在 owner 层恢复 `Background` / `BorderBrush` 默认值，`TextBox` 模板不得重新绑定这两个属性的 `TemplateBinding`；owner 属性值是中继判定“是否定制”的唯一输入。`EmbeddedTextBox` 这类需要压制 chrome 的内嵌派生在自身 ControlTheme 中显式设置 owner 值，经中继成为 frame LocalValue，不依赖模板绑定。
- root `Background` / `BorderBrush` 中继不得改变共享状态机：未定制路径零 LocalValue 写入，rest / hover / pressed / focus / disabled 视觉与 `LineEditBorderRenderTests` 基线保持一致；定制路径冻结该属性槽的交互态变色，`ClearValue` 后必须完整恢复状态机。

## 10. 测试与验证

验证范围：

- `LineEditShowCasePageTests` 覆盖 Gallery 页面结构、示例 snapshot 和源码片段入口。
- `LineEditSemanticPartTests` 覆盖 descriptor、静态 marker、生成 Style 命中、默认主题不消费 semantic selector、状态身份稳定和尺寸/variant 基线。
- `AddOnContentTemplateTests` 覆盖 prefix 的 content、template-only 与 template 组合路径，防止稳定 wrapper 改变既有 AddOn 内容语义。
- 清除按钮：空文本、非空文本、read-only、TextArea 和 single-line 差异。
- TextBox 布局：显式宽度和父布局分配宽度保持稳定，placeholder、单字符和长文本切换不改变 input frame 宽度。
- 生命周期：基础输入、全部内部派生输入和 AutoComplete 组合宿主在 logical detach/reattach 后保持 reveal、clear、inner-right、IME preedit、viewport 和 Form feedback 连接。
- SizeType：Large/Middle/Small/Custom 字号、高度、line height 和 `IsCustomFontSize` 优先级。
- Variant/status：Outlined、Filled、Borderless、Underlined、Error、Warning、focus、disabled。
- AddOn：外部 left/right AddOn、内部 left/right content、Form feedback、字数统计顺序。
- SearchEdit：Default/Primary 搜索按钮、loading、搜索事件和 disabled。
- TextArea：Lines、MinLines、MaxLines、IsAutoSize、IsResizable、resize 边界和字数统计。
- 文本 viewport：clear/feedback/addon 改变内部可用宽度但 owner Bounds 不变时发布新值；模板重套用后旧 source 不再发布；TextArea scroller padding 和 presenter margin 被正确扣除。
- root 表面：`TextInputRootBrushRelayTests` 覆盖四控件的定制值直达 frame、hover / focus 边框变色让位、focus `BoxShadow` 存活、置空恢复状态机、模板应用前赋值中继和未定制 rest 基线。
- 文档改动运行 `git diff --check`，并检查相对链接存在。

## 状态动效初始化

共享 `InputControlFrame` 在构造时建立初始化会话，覆盖禁用、校验及初始外观同步，加载后恢复最新过渡集合；分离及模板变更时复位。`TextBox` 不再查找当前模板中不存在的 `InnerBoxDecorator` 并投递过渡开关。规则见 [共享状态动效初始化架构](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)。
