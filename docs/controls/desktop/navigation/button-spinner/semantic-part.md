# ButtonSpinner Semantic Part 契约

本文档定义 `ButtonSpinner` 公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[ButtonSpinner 桌面版架构设计](overview.md)，真实模板、状态投影与尺寸基线见
[ButtonSpinner 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`ButtonSpinner` 公开 `root`、`content`、`innerLeftContent`、`innerRightContent`、`actions`、`increaseButton`、
`decreaseButton` 七个职责区域（§1.1–1.7），全部为 `Single`。其中 `content`、`innerLeftContent`、`innerRightContent`
位于帧主题、两个按钮位于手柄主题，因此这四个部件声明 `CrossNestedOwners`；`actions` 的 marker 仍在 ButtonSpinner
自己的模板内，按宿主模板本地校验。

ButtonSpinner 是 AtomUI 的带步进按钮输入基座，其公开语义区域对应上游 `InputNumber` 已公开并实际消费的
`root` / `prefix` / `suffix` / `input` / `actions` 键：帧内主内容区映射 `input` 职责，帧内左、右内容槽映射
`prefix` / `suffix` 职责，步进按钮区映射 `actions`。范围依据与准入记录见
[改造设计](../../../superpowers/specs/2026-08-12-semantic-part-control-rollout-design.md)。

### 1.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `root` |
| Selector | ButtonSpinner 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `ButtonSpinner` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | ButtonSpinner owner |
| 职责 | 承载尺寸档、variant、状态、步进开关和 owner-scoped Semantic Style 入口。 |
| 相关 API | `SizeType`、`StyleVariant`、`Status`、`IsSpinEnabled`、`IsButtonSpinnerVisible`、`IsButtonSpinnerFloatable`、`ButtonSpinnerLocation`、`SpinnerHandleWidth`、`IsEnabled` |
| 相关 Token | SharedToken、`ButtonSpinnerToken` |
| 稳定性 | stable since 6.2.0 |

`root` 是控件自身，不声明 `.semantic-root` marker。它适合定制 ButtonSpinner 整体 `Opacity`、对齐和尺寸约束。

**根边框定制：** 可见外框由共享输入帧绘制，其状态机拥有 `BorderBrush`。owner 的 `BorderBrush` 会以
LocalValue 中继到帧节点生效（对齐上游 `styles.root.borderColor` 语义与共享 `AbstractTextInput` 行为），
因此在 owner-scoped Style 里直接写根 `Setter Property="BorderBrush"` 即可定制外框颜色：

```xml
<Style Selector="atom|ButtonSpinner.semantic-object">
    <Setter Property="BorderBrush" Value="#1677FF" />
    <atom:ButtonSpinnerActionsStyle x:SetterTargetType="TemplatedControl">
        <Setter Property="Background" Value="#F0F5FF" />
    </atom:ButtonSpinnerActionsStyle>
</Style>
```

定制期间该属性槽的 hover / focus 变色冻结（LocalValue 优先于帧状态机），focus 的 `BoxShadow` 光晕不受影响；
置空（或不设置）后恢复帧状态机。中继带接管标记：仅当本控件实际写入过该槽位时才在置空时清除，因此
`NumericUpDown` 这种「自身持有中继、内部 spinner 是其派生实现」的嵌套场景不会被外层清除。

### 1.2 `content`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-scope-frame /template/ .semantic-content` |
| Style Type | `ButtonSpinnerContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true`（锚点 `.semantic-scope-frame` 解析到 `ButtonSpinnerDecoratedBox` 主题） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 帧模板内的主内容 presenter（`PART_ContentPresenter`） |
| 职责 | 承载 `Content` 与 `ContentTemplate` 的最终呈现，即 ButtonSpinner 的内容表面。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | 输入尺寸 padding、`FontSize` 继承 |
| 稳定性 | stable since 6.2.0 |

`content` 是 ButtonSpinner 自己的内容表面。ButtonSpinner 不拥有文本编辑面，因此不发布 `input` 部件；派生控件
（如 `NumericUpDown`）自行在自己的 owner 上发布 `input`，其契约不通过 ButtonSpinner 继承。`Content=null` 时
presenter 仍属于静态模板结构。适合定制 `Opacity`、`Margin`、`Padding`、`Foreground` 和对齐；内容子树由用户或
派生控件拥有，不由 `content` 契约继续展开。

### 1.3 `innerLeftContent`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `innerLeftContent` |
| Selector | `.semantic-inner-left-content` |
| SelectorRoute | `/template/ .semantic-scope-frame /template/ .semantic-inner-left-content` |
| Style Type | `ButtonSpinnerInnerLeftContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true`（锚点 `.semantic-scope-frame` 解析到 `ButtonSpinnerDecoratedBox` 主题） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 帧模板内的内容左槽 presenter（`PART_ContentLeftAddOn`，internal `AddOnContentPresenter`） |
| 职责 | 承载 `InnerLeftContent` 与 `InnerLeftContentTemplate` 的最终呈现。 |
| 相关 API | `InnerLeftContent`、`InnerLeftContentTemplate` |
| 相关 Token | `SpacingXXS`、输入尺寸 padding |
| 稳定性 | stable since 6.2.0 |

`innerLeftContent` 对应上游 `prefix` 职责，但**不使用 `semantic-prefix` class**：`NumericUpDown` 为其自有
`prefix` 发布的 route 是宽松后代（`/template/ .semantic-scope-spinner >> .semantic-prefix`），而 ButtonSpinner 的
帧节点位于 `NumericUpDownSpinner` 的子树内。若 ButtonSpinner 额外标记 `semantic-prefix`，该 route 会命中两个节点，
破坏 `NumericUpDown` 已发布的 `Single` 契约与其现有测试（该测试用 `.Single()` 解析目标）。改用与 ButtonSpinner
公开 API 同名的 `innerLeftContent` 可在不修改任何已发布契约的前提下保持唯一目标，且与仓库已有的
`itemContent` / `listContent` 命名法一致。帧槽位在空内容时自折叠，因此该部件呈现的是「有内容时才可见的左槽」。

### 1.4 `innerRightContent`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `innerRightContent` |
| Selector | `.semantic-inner-right-content` |
| SelectorRoute | `/template/ .semantic-scope-frame /template/ .semantic-inner-right-content` |
| Style Type | `ButtonSpinnerInnerRightContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true`（锚点 `.semantic-scope-frame` 解析到 `ButtonSpinnerDecoratedBox` 主题） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 帧模板内的内容右槽 presenter（`PART_ContentRightAddOn`，internal `AddOnContentPresenter`） |
| 职责 | 承载 `InnerRightContent` 与 `InnerRightContentTemplate` 的最终呈现。 |
| 相关 API | `InnerRightContent`、`InnerRightContentTemplate` |
| 相关 Token | `SpacingXXS`、输入尺寸 padding |
| 稳定性 | stable since 6.2.0 |

`innerRightContent` 对应上游 `suffix` 职责，命名理由同 §1.3。它与浮动手柄在 hover 时的 `ContentRightShift`
位移共享同一布局链路：定制其 `Margin` / `Padding` 时要按 §7.1 审计与手柄占位的协调结果。

### 1.5 `actions`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `actions` |
| Selector | `.semantic-actions` |
| SelectorRoute | `/template/ .semantic-scope-frame >> .semantic-actions` |
| Style Type | `ButtonSpinnerActionsStyle` |
| ContractType | `TemplatedControl` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `false`（marker 在 owner 模板内） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 帧内 `SpinnerContent` 承载的步进手柄（internal `ButtonSpinnerHandle`，最低 public 类型 `TemplatedControl`） |
| 职责 | 步进按钮区整体表面：手柄背景、分隔线与外框描边、圆角、内边距和悬浮/浮动呈现。 |
| 相关 API | `IsButtonSpinnerVisible`、`IsButtonSpinnerFloatable`、`ButtonSpinnerLocation`、`SpinnerHandleWidth`、`IsSpinEnabled` |
| 相关 Token | `ButtonSpinnerToken.HandleWidth`、`HandleBg`、`FilledHandleBg`、`HandleBorderColor`、`HandleActiveBg` |
| 稳定性 | stable since 6.2.0 |

`actions` 直接对应上游 `InputNumber` 的 `actions` 键——上游把上、下步进按钮包在同一个 `-actions` 容器内，AtomUI
对应的容器就是 `ButtonSpinnerHandle`。手柄的背景与描边由该节点自身的 `Render` 绘制并在状态变化时重绘，因此它是
步进区唯一稳定的对外表面。

适合定制的属性只有该节点实际参与绘制的成员：`Background`（填充）、`BorderBrush`（分隔竖线、中线以及
hover / pressed 颜色来源）、`CornerRadius`（填充外角）和 `Opacity`。**不适合定制：** 手柄描边宽度来自 internal
`SpinnerBorderThickness`（由 `SharedToken.BorderThickness` 供给），`BorderThickness` 与 `Padding` 在该节点的渲染与
模板中都没有参与，设置它们不会产生视觉变化，不属于本 Part 的稳定定制面。

步进行为、`ValidSpinDirection`、键盘与滚轮路径仍由 `ButtonSpinner` 拥有，不通过 Semantic Style 改写。

手柄在 `IsButtonSpinnerVisible=false`、或浮动模式下未悬浮、或控件 disabled 时保持存在但 `Opacity` 为 0；节点
身份与数量不随这些状态变化，因此是 `Single` 而不是 `Optional`。

### 1.6 `increaseButton`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `increaseButton` |
| Selector | `.semantic-increase-button` |
| SelectorRoute | `>> .semantic-actions /template/ .semantic-increase-button` |
| Style Type | `ButtonSpinnerIncreaseButtonStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true`（以 `.semantic-actions` 为锚点解析到 `ButtonSpinnerHandle` 主题） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 手柄主题内的增加按钮（`IconButton#PART_IncreaseButton`） |
| 职责 | 提供增加步进的交互入口表面。 |
| 相关 API | `IsSpinEnabled`、`ValidSpinDirection`、`IsButtonSpinnerVisible` |
| 相关 Token | `HandleIconSize`、`HandleHoverColor`、`ColorPrimaryActive`、`HandleActiveBg` |
| 稳定性 | stable since 6.2.0 |

`increaseButton` 的最低 public `ContractType` 是 `IconButton`。marker 位于 `ButtonSpinnerHandle` 自己的主题资产
（嵌套控件的模板），因此按 [Semantic Part 系统设计 §3.3.1](../../../../architecture/systems/theming/semantic-parts.md)
以 `.semantic-actions` 为锚点校验与解析。它适合定制 `IconBrush`、`IconWidth` / `IconHeight`、`Background`、
`CornerRadius` 和 `Opacity`。增加按钮的启用状态由 `ValidSpinDirection` 与 `IsSpinEnabled` 推导（`SetButtonUsage`），
不通过 Semantic Style 改写。

### 1.7 `decreaseButton`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `decreaseButton` |
| Selector | `.semantic-decrease-button` |
| SelectorRoute | `>> .semantic-actions /template/ .semantic-decrease-button` |
| Style Type | `ButtonSpinnerDecreaseButtonStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true`（以 `.semantic-actions` 为锚点解析到 `ButtonSpinnerHandle` 主题） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 手柄主题内的减少按钮（`IconButton#PART_DecreaseButton`） |
| 职责 | 提供减少步进的交互入口表面。 |
| 相关 API | `IsSpinEnabled`、`ValidSpinDirection`、`IsButtonSpinnerVisible` |
| 相关 Token | `HandleIconSize`、`HandleHoverColor`、`ColorPrimaryActive`、`HandleActiveBg` |
| 稳定性 | stable since 6.2.0 |

与 `increaseButton` 对称；两个按钮是独立职责，因此分别发布为两个 `Single` 部件，允许差异化定制而无需
`Multiple` 语义。`ButtonSpinnerLocation` 只改变手柄相对内容的位置，不交换这两个按钮的身份。

上游 `InputNumber` 只为整个 `actions` 容器公开一个键，没有为单个上、下按钮公开语义键；本控件额外发布这两个
部件属于显式能力补充，理由与 `SplitButton` 的 `primary` / `secondary` 一致：它们的宿主是模板内部件，不发布则
完全不可定制。

## 2. 模板变体与 route

ButtonSpinner 只有一个内置模板，但步进区有三种可观察呈现，Semantic 契约在全部呈现下一致：

| 呈现 | 触发条件 | 手柄状态 | Part 集合 |
| --- | --- | --- | --- |
| 常驻手柄（**默认**） | `IsButtonSpinnerFloatable=False`（属性默认值） | 常驻可见并占位 | 7 个部件全部存在 |
| 浮动手柄 | `IsButtonSpinnerFloatable=True` | hover 前 `Opacity=0` 且外移 | 7 个部件全部存在 |
| 手柄隐藏 | `IsButtonSpinnerVisible=False` | 存在但 `Opacity=0` | 7 个部件全部存在 |

> **默认是常驻手柄。** `IsButtonSpinnerFloatable` 默认 `False`（见 `ButtonSpinner.cs`），因此未显式设置时
> 手柄常驻可见，不是浮动手柄；浮动手柄必须显式设置 `IsButtonSpinnerFloatable=True`。

`.semantic-scope-frame` 是帧锚点，不属于公开 Part 表；它同时用于跨越「owner 模板 → 帧模板 → 手柄模板」三段
边界。`NumericUpDown` 的 Spinner 模式使用自己的 `NumericUpDownSpinnerTheme` 模板（不经过 `ButtonSpinnerHandle`），
其按钮属于 `NumericUpDown` 的模板节点，不在本契约范围内。

## 3. Selector 用法

生成的 Style Type 已封装 ButtonSpinner owner 类型保护和跨帧、跨手柄的 `SelectorRoute`，应用不直接复制
`/template/` 路径：

```xml
<Style Selector="atom|ButtonSpinner.semantic-style-demo">
    <atom:ButtonSpinnerContentStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Opacity" Value="0.9" />
    </atom:ButtonSpinnerContentStyle>
    <atom:ButtonSpinnerInnerLeftContentStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Foreground" Value="#597EF7" />
    </atom:ButtonSpinnerInnerLeftContentStyle>
    <atom:ButtonSpinnerInnerRightContentStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Opacity" Value="0.82" />
    </atom:ButtonSpinnerInnerRightContentStyle>
    <atom:ButtonSpinnerActionsStyle x:SetterTargetType="TemplatedControl">
        <Setter Property="Background" Value="#F0F5FF" />
    </atom:ButtonSpinnerActionsStyle>
    <atom:ButtonSpinnerIncreaseButtonStyle x:SetterTargetType="atom:IconButton">
        <Setter Property="IconBrush" Value="#1677FF" />
    </atom:ButtonSpinnerIncreaseButtonStyle>
    <atom:ButtonSpinnerDecreaseButtonStyle x:SetterTargetType="atom:IconButton">
        <Setter Property="IconBrush" Value="#1677FF" />
    </atom:ButtonSpinnerDecreaseButtonStyle>
</Style>
```

不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `ContentPresenter.semantic-content`、`TemplatedControl.semantic-actions` 等 `ContractType` 写入 Part 身份 selector。
- 直接复制多层 `/template/ .semantic-scope-*` route；scope class 只用于生成 Style 的 owner-relative 路由。
- 穿过 `InnerLeftContentTemplate` / `InnerRightContentTemplate` / `ContentTemplate` 创建的用户内容继续匹配内部 Visual。
- 把 `ButtonSpinnerDecoratedBox`、`ButtonSpinnerHandle` 或 `NumericUpDownSpinner` 当作 descriptor owner；它们的契约分别属于
  `ButtonSpinner` 与其派生 owner。

## 4. 状态与数量语义

七个部件均为 `Single`。状态变化只切换可见性、有效视觉值或位移，不增删 marker，也不改变对象身份。

| 场景 | root | content | innerLeftContent | innerRightContent | actions | increaseButton | decreaseButton | 说明 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 默认（浮动手柄，未悬浮） | 1 | 1 | 1（折叠） | 1（折叠） | 1（`Opacity=0`） | 1 | 1 | 静态节点均已实例化。 |
| 悬浮展开 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 只改变透明度与位移。 |
| `IsButtonSpinnerFloatable=False` | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 手柄常驻并占位。 |
| `IsButtonSpinnerVisible=False` | 1 | 1 | 1 | 1 | 1（`Opacity=0`） | 1 | 1 | 手柄仍存在。 |
| `InnerLeftContent` / `InnerRightContent` 为空 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 槽位 presenter 自折叠但不移除。 |
| `Content=null` | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 内容 presenter 保持存在。 |
| `ButtonSpinnerLocation` 切换 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 只改变手柄停靠侧与对齐。 |
| disabled / read-only / status / focus / hover | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 状态由 owner 与共享帧投影，marker 不变。 |
| `ValidSpinDirection` 变化 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 只改变按钮 `IsEnabled`。 |
| 模板重套用 | 1（重建） | 1（重建） | 1（重建） | 1（重建） | 1（重建） | 1（重建） | 1（重建） | 模板重建后提供相同 Part 集合。 |

## 5. 定制边界

以下区域明确不属于 ButtonSpinner Semantic Part：

- 外部 `LeftAddOn` / `RightAddOn` 区域及其用户模板内容。这两个区域由共享 `AddOnDecoratedBox` 帧提供，输入家族
  （`LineEdit`、`Select`、`NumericUpDown` 等）统一不为其发布 Part；需要整体替换时使用 owner `ControlTheme`。
- 帧的 hover / focus / status / disabled 状态机（`PART_ContentFrame` 的边框、背景与 `BoxShadow` 投影）。
- 手柄内的分隔线几何、`UniformGrid` 布局、`ButtonSpinnerContentPanel` 的测量/排列算法和 `EffectiveContentPadding` 计算。
- 快速点按、滚轮、键盘、`ValidSpinDirection` 与 `IsSpinEnabled` 行为路径。
- `ContentTemplate` / `InnerLeftContentTemplate` / `InnerRightContentTemplate` 创建的用户子树。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态订阅。
Semantic Style 服从 Avalonia 原生属性优先级，布局结果仍可能受 owner、帧与 presenter 的 Min/Max、Padding、
Margin 与裁剪约束。

## 6. 布局型 Part 的尺寸基线

按 [Semantic Part 系统设计 §7.2](../../../../architecture/systems/theming/semantic-parts.md) 的前置基线审计要求，
开放上述部件的布局类 Setter 前必须遵守以下已确认基线：

| 维度 | 事实 |
| --- | --- |
| 尺寸档 | `Large` / `Middle` / `Custom` / `Small` 四档，`Custom` 与 `Middle` 共用同一套值。 |
| 尺寸属性 owner | owner 不设固定 `Height`；`SizeType` 在帧主题上映射 `MinHeight`（`ControlHeightLG` / `ControlHeight` / `ControlHeightSM`）与 `ContentPadding`（`InputPaddingLG` / `InputPadding` / `InputPaddingSM`）。 |
| 圆角 owner | owner 持有 `CornerRadius`（`BorderRadiusLG` / `BorderRadius` / `BorderRadiusSM`）并绑定到手柄；帧内容框使用 `InnerBoxCornerRadius`。 |
| 手柄宽度 | `SpinnerHandleWidth` 来自 `ButtonSpinnerToken.HandleWidth`，owner 与帧两处都设默认值；`ButtonSpinnerContentPanel` 的 `HandleWidth` 参与测量。 |
| 占位协调 | `IsShowHandle && !IsHandleFloatable` 时 `ConfigureEffectiveContentPadding` 用 `SpinnerHandleWidth` 追加内容 padding；浮动 hover 时改用 `ContentLeftShift` / `ContentRightShift` 位移而非 padding。 |
| 自然测量 | `ButtonSpinner.ArrangeOverride` 按 `DecoratedBox.BorderThickness` 外扩；帧以 `MinHeight` 建立基线，内容驱动增长。 |
| 变体一致性 | 三种步进区呈现共用同一帧模板与同一尺寸映射，`NumericUpDownSpinner` 复用同一帧主题。 |

因此布局型 Setter 必须先选择一套完整 `SizeType` 基线再叠加增量属性，不得跨档位拼接；改动 `innerRightContent` 或
`actions` 的 `Padding` / `Margin` / `Width` 时必须同时验证与手柄占位、`ContentRightShift` 位移和帧裁剪的协调结果。
ButtonSpinner 不存在与尺寸无关的布局特例，也无需为此引入固定 `Height`。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`、改变 cardinality，或让内置模板缺少 marker，
均属于公共主题契约变更。

验证至少覆盖：

- `ButtonSpinner` descriptor 只有 `root`、`content`、`innerLeftContent`、`innerRightContent`、`actions`、
  `increaseButton`、`decreaseButton`，字段值与本文一致，且 `root` 的 `StyleType` 为 `null`。
- 内置模板包含 `semantic-content`、`semantic-inner-left-content`、`semantic-inner-right-content`、
  `semantic-actions`、`semantic-increase-button`、`semantic-decrease-button` 与 `semantic-scope-frame` marker；
  不声明 `.semantic-root`。
- 生成的 `ButtonSpinner*Style` 可以编译，并在三种步进区呈现下命中各自的最低 public `ContractType`。
- **回归护栏：** 新增 marker 后 `NumericUpDown` 的语义部件测试必须仍然全绿——特别是其 `prefix` 的
  `.Single()` 目标解析不得因为帧节点新增 class 而出现第二个匹配；ButtonSpinner 不得复用 `semantic-prefix`。
- 状态切换（悬浮、浮动开关、手柄隐藏、`ValidSpinDirection`、disabled）保持对象身份与 `Single` 数量。
- 默认主题不消费 `.semantic-*`；未声明用户 Semantic Style 时不增加 selector activator。
- Gallery Semantic Parts Tab 延迟创建 Preview，展示三种步进区呈现的强类型 Style 示例，且每个可静态解析的 Part
  均可高亮解析唯一目标。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
- 布局型 Setter 按 §6 基线与 owner `MinHeight`、帧裁剪和手柄占位的协调结果验证。
