# TimePicker Semantic Part 契约

本文档定义 TimePicker 家族公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。TimePicker 家族包含两个
Semantic owner：单值 `TimePicker` 与范围 `RangeTimePicker`（语义对齐上游 Ant Design `TimePicker` /
`TimePicker.RangePicker` 共用的分区式 `classNames` / `styles` 契约）。控件整体设计见
[TimePicker 桌面版架构设计](overview.md)，真实模板、marker 映射与生命周期见
[TimePicker 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`TimePicker` 公开 11 个 Semantic Part，`RangeTimePicker` 公开 12 个 Semantic Part（后者多出范围双输入框的
`secondaryInput`）。声明分别位于 `TimePicker.SemanticParts.cs` 与 `RangeTimePicker.SemanticParts.cs` partial 文件。

触发区部件的 marker 分布：

- 单值 `TimePicker` 的宿主模板是共享 `InfoPickerInputTheme.axaml`（`TimePickerTheme.axaml` 纯 `BasedOn` 继承，
  无自有模板）；`RangeTimePicker` 的宿主模板是自有 `RangeTimePickerTheme.axaml` 模板覆写（共享
  `RangeInfoPickerInputTheme.axaml` 保持无 semantic 标注）。两个宿主模板按同一模式标注
  `semantic-scope-input`（AddOnDecoratedBox 节点）、`semantic-input`（`PART_InfoInputBox`）、`semantic-suffix`
  （右侧内容 StackPanel）、`semantic-scope-handle`（PickerClearUpButton 节点）与 `semantic-popup-root`
  （`PART_Popup` 内的 `ArrowDecoratedBox`），Range 模板另标注 `semantic-secondary-input`
  （`PART_SecondaryInfoInputBox`）。
- `prefix` 借用共享 `AddOnDecoratedBoxTheme` 的 `.semantic-scope-prefix` scope 锚点路由到宿主模板
  `AddOnContentPresenter` 投影节点（与 Select、DatePicker 家族同构；投影节点以
  `CompiledBinding $parent[atom:InfoPickerInput].ContentLeftAddOn` 呈现公共 API 值）。
- `clear` 的物理按钮在共享 `PickerClearUpButtonTheme.axaml` 模板内（`PART_ClearButton`），声明
  `CrossNestedOwners=true`，生成器沿 PickerClearUpButton 主题链校验。
- 范围 `RangeTimePicker` 与单值 `TimePicker` 共用同一弹层 presenter（`TimePickerPresenter`），弹层 marker
  对两个 owner 同时成立。

弹层部件的 marker 分布（弹层内容全部由 owner `CreatePickerPresenter` 在首次打开时运行时创建）：

- `popup.container` / `popup.footer` 标注 presenter 主题模板节点：`popup.container` 为
  `TimePickerPresenterTheme.axaml` 的 `DockPanel #PART_MainLayout`，`popup.footer` 为同模板的
  `PixelAlignedBorder #PART_ButtonsFrame`。
- `popup.content` / `popup.column` 标注 `TimeViewTheme.axaml` 模板节点：`popup.content` 为时间列布局容器
  `Grid #PART_PickerContainer`，`popup.column` 为四个列宿主 Panel（`PART_HourHost` / `PART_MinuteHost` /
  `PART_SecondHost` / `PART_PeriodHost`）。
- `popup.item` 为运行时注入：`DateTimePickerPanel.RefreshItems` 创建 `TimeViewCell` 时追加生成
  selector class 常量，覆盖滚动复用与列表滚动路径。
- `popup.*` 除 `popup.root` 外统一声明 `RuntimeCreated=true`（presenter 子树在运行时组装，生成器豁免宿主模板
  marker 校验，由控件行为测试兜底），其中 `popup.root` 为宿主模板静态节点、`RuntimeCreated=false`。
- **TimeView 子树类名避让**：`TimeView` 同时被 DatePicker 带时间弹层内嵌（`DatePickerPresenterTheme.axaml` 与
  `TimedRangeDatePickerPresenterTheme.axaml`）。因此落在 TimeView 子树内的三个 marker 类名使用 `time-` 中缀
  （`semantic-time-content` / `semantic-time-column` / `semantic-time-item`），避开 DatePicker 家族已声明的
  `semantic-popup-content` / `semantic-popup-body` / `semantic-cell` 等类名，防止 DatePicker 弹层上下文中的
  契约误命中（详见 §6 对照差异）。

### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `TimePicker` / `RangeTimePicker` |
| Part | `root` |
| Selector | owner 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `TimePicker` / `RangeTimePicker` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | TimePicker / RangeTimePicker owner |
| 职责 | owner 是时间值、约束、弹层状态、Form 值与验证状态的组织边界。 |
| 相关 API | 全部 TimePicker / RangeTimePicker public API |
| 相关 Token | TimePickerToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

### `prefix`

| 字段 | 值 |
| --- | --- |
| Owner | `TimePicker` / `RangeTimePicker` |
| Part | `prefix` |
| Selector | `.semantic-prefix` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix` |
| Style Type | `TimePickerPrefixStyle` / `RangeTimePickerPrefixStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 宿主模板中承载 `ContentLeftAddOn` 的 `AddOnContentPresenter` 投影节点 |
| 职责 | 输入区内容前缀区域，承载 `ContentLeftAddOn` 用户内容，在内容框内联展示。 |
| 相关 API | `ContentLeftAddOn`、`ContentLeftAddOnTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

### `input`

| 字段 | 值 |
| --- | --- |
| Owner | `TimePicker` / `RangeTimePicker` |
| Part | `input` |
| Selector | `.semantic-input` |
| SelectorRoute | `/template/ .semantic-input` |
| Style Type | `TimePickerInputStyle` / `RangeTimePickerInputStyle` |
| ContractType | `TextBox` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 宿主模板的 `InfoPickerTextBox #PART_InfoInputBox`（起始端输入框） |
| 职责 | 时间文本输入框，承载格式化显示值、占位符与只读/校验状态。 |
| 相关 API | `Text`、`PlaceholderText`、`IsReadOnly`、`PreferredInputWidth` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

### `secondaryInput`（仅 RangeTimePicker）

| 字段 | 值 |
| --- | --- |
| Owner | `RangeTimePicker` |
| Part | `secondaryInput` |
| Selector | `.semantic-secondary-input` |
| SelectorRoute | `/template/ .semantic-secondary-input` |
| Style Type | `RangeTimePickerSecondaryInputStyle` |
| ContractType | `TextBox` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `RangeTimePickerTheme.axaml` 的 `InfoPickerTextBox #PART_SecondaryInfoInputBox`（结束端输入框） |
| 职责 | 范围选择的结束端时间文本输入框，与 `input` 共用格式与宽度基线。 |
| 相关 API | `SecondaryText`、`SecondaryPlaceholderText` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

### `suffix`

| 字段 | 值 |
| --- | --- |
| Owner | `TimePicker` / `RangeTimePicker` |
| Part | `suffix` |
| Selector | `.semantic-suffix` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix` |
| Style Type | `TimePickerSuffixStyle` / `RangeTimePickerSuffixStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 宿主模板中投影给 `ContentRightAddOn` 的水平 StackPanel（含清除按钮与 `PART_ContentRightAddOnPresenter`） |
| 职责 | 输入区后缀区域，承载清除按钮、Form 反馈与用户后缀内容。 |
| 相关 API | `ContentRightAddOn`、`ContentRightAddOnTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

### `clear`

| 字段 | 值 |
| --- | --- |
| Owner | `TimePicker` / `RangeTimePicker` |
| Part | `clear` |
| Selector | `.semantic-clear` |
| SelectorRoute | `>> .semantic-scope-handle /template/ .semantic-clear` |
| Style Type | `TimePickerClearStyle` / `RangeTimePickerClearStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossNestedOwners | `true` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 共享 `PickerClearUpButtonTheme.axaml` 模板内的 `InputClearIconButton #PART_ClearButton` |
| 职责 | 后缀区清除按钮，进入清除模式（hover / focus）时渲染。 |
| 相关 API | `ShowClearButtonPredicate`、`Clear` / `Reset` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `TimePicker` / `RangeTimePicker` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `/template/ .semantic-popup-root` |
| Style Type | `TimePickerPopupRootStyle` / `RangeTimePickerPopupRootStyle` |
| ContractType | `ArrowDecoratedBox` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 单选：`InfoPickerInputTheme.axaml` 中 `PART_Popup` 的 `ArrowDecoratedBox`；范围：`RangeTimePickerTheme.axaml` 中的 `ArrowDecoratedBox` |
| 职责 | 弹层内容根视觉盒子，承载背景、边框、阴影与浮动箭头。 |
| 相关 API | `IsArrowVisible`（经 `IsArrowVisibleEffective`）、`ArrowPosition`、`IsMotionEnabled` |
| 相关 Token | PopupToken |
| 稳定性 | stable since 6.2.0 |

### `popup.container`

| 字段 | 值 |
| --- | --- |
| Owner | `TimePicker` / `RangeTimePicker` |
| Part | `popup.container` |
| Selector | `.semantic-popup-container` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-container` |
| Style Type | `TimePickerPopupContainerStyle` / `RangeTimePickerPopupContainerStyle` |
| ContractType | `DockPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TimePickerPresenterTheme.axaml` 模板的 `DockPanel #PART_MainLayout` |
| 职责 | 时间面板内容容器，组织时间区与底部按钮区的布局。 |
| 相关 API | 无（面板内容布局容器） |
| 相关 Token | TimePickerToken |
| 稳定性 | stable since 6.2.0 |

### `popup.content`

| 字段 | 值 |
| --- | --- |
| Owner | `TimePicker` / `RangeTimePicker` |
| Part | `popup.content` |
| Selector | `.semantic-time-content` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-time-content` |
| Style Type | `TimePickerPopupContentStyle` / `RangeTimePickerPopupContentStyle` |
| ContractType | `Grid` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TimeViewTheme.axaml` 的 `Grid #PART_PickerContainer`（时/分/秒/时段四列布局容器） |
| 职责 | 时间列布局容器，按 12/24 小时制组织全部时间列。 |
| 相关 API | 无（随 `popup.container` 呈现） |
| 相关 Token | TimePickerToken |
| 稳定性 | stable since 6.2.0 |

### `popup.column`

| 字段 | 值 |
| --- | --- |
| Owner | `TimePicker` / `RangeTimePicker` |
| Part | `popup.column` |
| Selector | `.semantic-time-column` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-time-column` |
| Style Type | `TimePickerPopupColumnStyle` / `RangeTimePickerPopupColumnStyle` |
| ContractType | `Panel` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TimeViewTheme.axaml` 的四个列宿主 Panel：`PART_HourHost` / `PART_MinuteHost` / `PART_SecondHost` / `PART_PeriodHost`（各列宽度 owner） |
| 职责 | 单个时间列宿主，承载滚动视口与列宽基线（时/分/秒列宽 = `ItemWidth`，时段列宽 = `PeriodHostWidth`）。 |
| 相关 API | 无（随 `popup.content` 呈现） |
| 相关 Token | TimePickerToken（`ItemWidth`、`PeriodHostWidth`） |
| 稳定性 | stable since 6.2.0 |

### `popup.item`

| 字段 | 值 |
| --- | --- |
| Owner | `TimePicker` / `RangeTimePicker` |
| Part | `popup.item` |
| Selector | `.semantic-time-item` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-time-item` |
| Style Type | `TimePickerPopupItemStyle` / `RangeTimePickerPopupItemStyle` |
| ContractType | `ListBoxItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `DateTimePickerPanel` 视口内运行时创建的 `TimeViewCell`（`RefreshItems` 创建路径，创建时注入 marker） |
| 职责 | 时间格子项，承载可选时间值与选中 / hover 状态视觉。 |
| 相关 API | 无（随 `popup.column` 呈现） |
| 相关 Token | TimePickerToken（`ItemHeight`） |
| 稳定性 | stable since 6.2.0 |

### `popup.footer`

| 字段 | 值 |
| --- | --- |
| Owner | `TimePicker` / `RangeTimePicker` |
| Part | `popup.footer` |
| Selector | `.semantic-popup-footer` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-footer` |
| Style Type | `TimePickerPopupFooterStyle` / `RangeTimePickerPopupFooterStyle` |
| ContractType | `PixelAlignedBorder` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TimePickerPresenterTheme.axaml` 的 `PixelAlignedBorder #PART_ButtonsFrame`（内含 `PART_NowButton` / `PART_ConfirmButton`） |
| 职责 | 面板底部操作区，承载此刻 / 确认按钮。 |
| 相关 API | `IsNeedConfirm`、`IsShowNow` |
| 相关 Token | TimePickerToken |
| 稳定性 | stable since 6.2.0 |

`ContractType` 不参与 selector 匹配，只约束生成 Style 的 `x:SetterTargetType` 与模板校验的类型兼容；internal
实现类型（`InfoPickerTextBox`、`TimePickerPresenter`、`TimeView`、`DateTimePickerPanel`）统一承诺到最低 public
基类或公开包装类型（`TextBox`、`ContentPresenter`、`DockPanel`、`Grid`、`Panel`、`ListBoxItem`、
`PixelAlignedBorder`——footer 物理节点是 `PixelAlignedBorder`，它继承 `DashedBorder` 而非 Avalonia `Border`，
与 DatePicker 家族的 `popup.footer` 契约保持一致）。

## 2. 职责与存在条件

- 触发区 `prefix` / `suffix` 的路由借用共享 `AddOnDecoratedBoxTheme` 的 scope 锚点；marker 本体在宿主模板内，
  不声明 `CrossNestedOwners`。`clear` 的锚点 `semantic-scope-handle` 位于宿主模板 PickerClearUpButton 节点，
  `>>` 首步覆盖该节点位于 `AddOnDecoratedBox.ContentRightAddOn` 属性值子树（无 `TemplatedParent` 传播）的场景，
  生成器沿 PickerClearUpButton 主题链在 `PickerClearUpButtonTheme.axaml` 校验 marker 数量与类型。
- `popup.root` 是宿主模板 `PART_Popup` 的静态子节点（Popup.Child），marker 为静态模板节点标注；
  `popup.container` / `popup.footer` / `popup.content` / `popup.column` 的 marker 静态写在 TimePicker 家族内部
  主题（presenter / TimeView 主题）模板节点上，`popup.item` 由 `DateTimePickerPanel` 创建格子时注入；由于整个
  presenter 子树在首次打开时运行时组装，以上部件统一声明 `RuntimeCreated=true`，生成器豁免宿主模板校验，由行为
  测试验证 marker 存在性与路由命中。
- **单值 / 范围存在条件**：
  - 单值 `TimePicker`：三列（24 小时制）或四列（12 小时制）滚轮；`secondaryInput` 不存在。
  - `RangeTimePicker`：双输入框 + 同一弹层；激活端切换（Start ↔ End）只改写 presenter 的 `SelectedTime`，
    不改变任何 marker。
- `popup.footer` 仅在 `IsButtonsPanelVisible=true` 时可见（`IsNeedConfirm` 提供 Confirm；`IsShowNow` 提供
  Now）；不可见是可见性切换，marker 不增删。
- `clear` 仅在清除模式（hover / focus 且存在可清除值）可见。
- `ClockIdentifier`（12 ↔ 24 小时制）切换：时段列宿主与分隔线走 `IsVisible` 切换，时间列经 `UpdateItems`
  原地重建值序列；`popup.column` / `popup.item` marker 恒存在，不随制式增删。
- `MinuteIncrement` / `SecondIncrement` 只改变格子值序列与数量语义（视口内格子数不变），不改变 marker 身份。
- TimeView 头部（`PART_HeaderText` 与分隔线）在 TimePicker 弹层中 `IsShowHeader=False` 恒隐藏，不属于契约（见
  §5）；该节点在 DatePicker 带时间弹层中的显示归属 DatePicker 上下文。

## 3. 数量语义

`root`、`prefix`、`input`、`suffix`、`clear`、`popup.root`、`popup.container`、`popup.content`、`popup.footer`
均为静态模板节点或每弹层唯一容器，`Single`；状态变化（清除模式、footer 可见性、12/24 小时制、disabled、验证
状态）只切换可见性或有效视觉值，不增删 marker。

`popup.column` 为 `Multiple`：TimeView 模板静态声明 4 个列宿主 marker；12 小时制运行时可见 4 个，24 小时制
时段列隐藏后可见 3 个，marker 本体不增删。

`popup.item` 为 `Multiple`：`DateTimePickerPanel` 按允许值及步长创建
`TimeViewCell`（默认视口下每列最多 60 个）；短步长序列重复有效值以填满循环视口，AM/PM 保持两项。滚动只改变物理偏移与周期位置，行容器保持身份；增量改变时复用或增删容器。创建路径统一注入
marker，保证滚动、增量变化与弹层重开后 marker 保持。

## 4. Selector 用法

生成的 Semantic Style 类型命名为 `<Owner><PartPathPascalCase>Style`，如 `TimePickerPopupRootStyle`、
`RangeTimePickerSecondaryInputStyle`（命名空间 `AtomUI.Theme.Styling`，AXAML 命名空间 `https://atomui.net`）。
`root` 不生成 Style 类型，owner 级 Setter 写在外层普通 Style 上。

推荐写法（owner 嵌套 Style + 语义 class，对齐上游 object / function styles 示例）：

```xml
<StackPanel.Styles>
    <Style Selector="atom|TimePicker.semantic-styles-demo">
        <atom:TimePickerPrefixStyle x:SetterTargetType="ContentPresenter">
            <Style Selector="^ atom|Icon">
                <Setter Property="StrokeBrush" Value="#1890FF" />
            </Style>
        </atom:TimePickerPrefixStyle>
        <atom:TimePickerPopupRootStyle x:SetterTargetType="atom:ArrowDecoratedBox">
            <Setter Property="BorderBrush" Value="#1890FF" />
        </atom:TimePickerPopupRootStyle>
        <atom:TimePickerPopupItemStyle x:SetterTargetType="ListBoxItem">
            <Setter Property="Foreground" Value="#722ED1" />
        </atom:TimePickerPopupItemStyle>
    </Style>
    <Style Selector="atom|RangeTimePicker.semantic-styles-demo">
        <atom:RangeTimePickerInputStyle x:SetterTargetType="TextBox">
            <Setter Property="FontStyle" Value="Italic" />
        </atom:RangeTimePickerInputStyle>
        <atom:RangeTimePickerPopupColumnStyle x:SetterTargetType="Panel">
            <Setter Property="Width" Value="56" />
        </atom:RangeTimePickerPopupColumnStyle>
    </Style>
</StackPanel.Styles>
<atom:TimePicker Classes="semantic-styles-demo" ... />
<atom:RangeTimePicker Classes="semantic-styles-demo" ... />
```

`popup.*` 部件的生成 Style 在弹层打开后命中目标；Gallery Semantic Parts 页签以钉住常开弹层呈现全部弹层部件。
`prefix` 为 `Icon` 时，AtomUI 图标由 `StrokeBrush` / `FillBrush` 驱动而非 `Foreground`，需在
`TimePickerPrefixStyle` 内嵌套 `<Style Selector="^ atom|Icon">` 设置 `StrokeBrush`。
`popup.item` 的样式作用于格子项本体；选中 / hover / disabled 状态视觉由 `TimeViewCell` 伪类承担，Semantic
Style 遵循 Avalonia 原生属性优先级。`popup.column` 的 `Width` Setter 覆盖列宿主的档位宽度基线
（`ItemWidth` / `PeriodHostWidth`），参与自然测量；格子高度由 `DateTimePickerPanel.ItemHeight` 统一拥有，
不属于 item 级可定制属性（见 implementation.md 尺寸基线矩阵）。

不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `ArrowDecoratedBox.semantic-popup-root` 等 `ContractType` 写入 Part 身份 selector。
- 直接复制 `/template/ .semantic-scope-*` route；scope class 只用于生成 Style 的 owner-relative 路由。
- 穿过 `ContentLeftAddOnTemplate`、`ContentRightAddOnTemplate` 等用户模板继续匹配内部 Visual。

## 5. 定制边界

以下区域不属于 TimePicker 家族 Semantic Part：

- **TimeView 头部**：`TextBlock #PART_HeaderText` 与分隔线在 TimePicker 弹层中恒隐藏（`IsShowHeader=False`），
  上游 TimePicker 面板同样无 header 槽位，不发布。
- **列内滚动机制**：`ScrollViewer` 滚动宿主、`DateTimePickerPanel` 的周期定位与容器复用
  （`ShouldLoop`、`SelectorRowCount`）与列间分隔 `Rectangle` 是内部实现，上游无对应槽位，不发布。
- **格子内部结构**：`TimeViewCell` 模板内的 `ContentPresenter` 与 motion 转场由其自身模板拥有，
  `popup.item` 只承诺格子项本体。
- **底部操作按钮本体**：`PART_NowButton` / `PART_ConfirmButton` 由 `popup.footer` 承载，不单独发布（与
  DatePicker 家族一致）。
- **范围指示与换向箭头**：`PART_RangePickerIndicator`、`PART_RangePickerArrow` 是 Range 触发区的附属视觉，
  上游无对应槽位，不发布。
- **输入框内部结构**：占位符、文本 presenter 由 `InfoPickerTextBox`（`EmbeddedTextBox`）内部承载，不经
  TimePicker 发布（上游 TimePicker 亦无 `placeholder` / `content` 槽）。
- **弹层宿主与定位**：`PART_Popup` 的定位、钉住打开、动画、light-dismiss 归共享 Popup 契约；`popup.root`
  只覆盖弹层内容根盒子的视觉。
- **输入表面**：variant、status、边框、背景由共享 `InputControlFrame` / `AddOnDecoratedBox` 承担，归共享输入
  契约。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态
订阅。共享 `InfoPickerInputTheme` / `PickerClearUpButtonTheme` 中的 marker 同时服务于 DatePicker 与 TimePicker
两个家族的同类 Part；`TimeViewTheme` 中的 `semantic-time-*` marker 对未声明该契约的 DatePicker 家族是 inert
class，不影响其弹层视觉与行为。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`（含把公共基类承诺收窄为具体实现
类型）、改变 cardinality，或让任一内置模板变体（单值共享模板、Range 自有模板、presenter 模板、TimeView
模板）缺少 marker，均属于公共主题契约变更。

与上游 Ant Design TimePicker 的对照差异（有意保持）：

- 上游 popup 组五个槽位（`root` / `container` / `content` / `item` / `footer`）全部同名对齐发布：
  `content` 是包住全部时间列的容器（上游为 `${prefixCls}-content` 列区域包裹层），`item` 是列内时间格子，
  `container` 是面板容器，`footer` 是承载 Now / OK 操作区的底部容器。
- 上游未声明 `clear` 槽（清除样式归入 `suffix` 描述）；AtomUI Select 家族将 `clear` 作为独立 Part 发布
  （物理节点在共享 `PickerClearUpButtonTheme` 内），TimePicker 家族与 Select、DatePicker 家族保持一致。
- AtomUI 增加 `popup.column`（上游无列级槽位）：四列滚轮是 AtomUI 时间面板的稳定结构特征，列宿主是各列
  宽度 owner，列级 `Width` / 背景定制可自然生效。
- 上游日期格子槽名 `item` 在 DatePicker 家族更名为 `popup.cell`（日历网格语义）；TimePicker 格子本质是滚轮
  列表项，保持上游 `item` 命名。
- **marker 类名有意偏离**：TimeView 子树内的 `popup.content` / `popup.column` / `popup.item` 使用
  `semantic-time-content` / `semantic-time-column` / `semantic-time-item`，而非常规的 `semantic-popup-content`
  等派生类名。原因是 TimeView 子树被 DatePicker 带时间弹层共享，必须避开 DatePicker 已声明的
  `semantic-popup-content` / `semantic-popup-body` / `semantic-cell` 等类名，防止 DatePicker 用户的
  `popup.content` 等定制连带命中时间面板（DatePicker 契约明确时钟面板不属于其边界）。
- 上游未区分范围双输入框；AtomUI 依据双 `InfoPickerTextBox` 结构事实发布 `input` + `secondaryInput` 两个
  Part（与 RangeDatePicker 一致）。
- 上游 `popup.root` 是定位包装层；AtomUI 的定位层是 `Popup` 本体（不可作为样式目标），故 `popup.root` 落在
  内容根盒子（ArrowDecoratedBox）上、`popup.container` 落在面板内容容器上（与 DatePicker 一致）。

验证至少覆盖：

- owner descriptor 只包含 §1 声明的 Part（TimePicker 11 个、RangeTimePicker 12 个），字段值与本文一致。
- `InfoPickerInputTheme.axaml` 携带单选触发区与 `popup.root` marker；`RangeTimePickerTheme.axaml` 携带范围
  触区、`secondaryInput` 与 `popup.root` marker；`PickerClearUpButtonTheme.axaml` 携带 `clear` marker；共享
  `RangeInfoPickerInputTheme.axaml` 不携带任何 semantic 选择器。
- `TimePickerPresenterTheme.axaml` 携带 `popup.container` / `popup.footer` marker；`TimeViewTheme.axaml`
  携带 `popup.content` / `popup.column`（×4）marker。
- `popup.item` marker 在 `DateTimePickerPanel` 格子创建路径注入；循环与非循环滚动、
  `MinuteIncrement` / `SecondIncrement` 变化、`ClockIdentifier` 12↔24 切换与弹层重开后 marker 保持。
- 生成的 `TimePicker*Style` / `RangeTimePicker*Style` 可编译并命中目标节点（见
  `tests/AtomUI.Desktop.Controls.Tests/TimePicker/TimePickerSemanticPartTests.cs`）。
- 弹层首次打开、关闭、重开与模板重套用后弹层部件 marker 保持；`IsButtonsPanelVisible=false` 时 footer
  marker 存在。
- 状态变化（清除模式、12/24 小时制、disabled、只读）不改变 marker 身份与数量语义。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator；`TimeViewTheme` 的
  `semantic-time-*` marker 不改变共享 DatePicker 弹层的语义部件契约。
- Gallery Semantic Parts Tab 延迟创建 Preview，弹层钉住常开并呈现 12/24 小时制四列，11 个 Part 均可解析
  高亮。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
