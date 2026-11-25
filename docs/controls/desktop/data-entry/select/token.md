# Select Token 设计

本文档定义 `AtomUI.Desktop.Controls.SelectToken` 的专属语义、分类、使用范围和兼容边界。共享输入表面分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Select 整体架构见 [Select 桌面版架构设计](overview.md)，内部实现原理见 [Select 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Select Changelog](changelog.md)。

## 1. 定位

SelectToken 是 Select 的控件级 Token scope，描述多选标签、候选项、候选弹层 padding 和 Select 输入内容 padding。输入表面的通用边框、圆角、状态色、focus ring、disabled 背景、CompactSpace 和 AddOn 结构来自 `InputControlFrameTheme`、SharedToken 和 PopupHostToken；SelectToken 不复制这些共享职责。

SelectToken 不承载以下状态：

- `OptionsSource`、`Options`、`SelectedOption`、`SelectedOptions`、`DefaultValues` 等数据状态。
- `FilterValue`、`IsDropDownOpen`、`IsLoading`、`Status`、`Mode` 等运行状态。
- 候选项 hover、pressed、selected、disabled 的当前实例状态。
- Popup 当前宽度、高度、offset、placement 或异步加载结果。

## 2. Token 分类

### 2.1 多选标签 Token

- `MultipleItemBg`
- `MultipleItemHeight`
- `MultipleItemHeightSM`
- `MultipleItemHeightLG`
- `MultipleSelectorBgDisabled`
- `MultipleItemColorDisabled`
- `FixedItemMargin`（Reserved）

这些 Token 控制 Multiple / Tags 模式下 `SelectTag` 的背景、尺寸和禁用态颜色；标签间距由标签面板的 `SpacingXXS` 决定。
`FixedItemMargin` 对应 antd 的内部常量 `INTERNAL_FIXED_ITEM_MARGIN`，当前没有消费者，标为 `[ReservedToken]`，见 §7。高度按 SharedToken 的控件高度和基础 padding 计算，使 Large / Middle / Small 三种预设尺寸与输入框高度对齐。

### 2.2 候选项 Token

- `OptionSelectedColor`
- `OptionSelectedIndicatorColor`
- `OptionSelectedFontWeight`
- `OptionSelectedBg`
- `OptionActiveBg`
- `OptionBackgroundInset`
- `OptionPadding`
- `OptionFontSize`
- `OptionHeight`
- `InputFontSizeSM`

这些 Token 控制 `SelectCandidateListItem` 的选中、active、内边距、文本和行高基准。`OptionPadding`（antd `optionPadding`）
是候选行内边距。`InputFontSizeSM` 是 Small 选择框本身（选中值、占位文字）的字号，默认等于 `FontSize`（与 antd 一致），
PrimeNG 取 `FontSizeSM`；它定义在 `AbstractSelectInputToken`，Cascader 与 TreeSelect 共用。弹层最大高度使用实例 `ItemHeight` 计算，主题默认值来自 `OptionHeight`，因此调整 `OptionHeight` 同时改变
候选行最小高度和弹层可视行数对应的高度。

`OptionBackgroundInset` 只内缩候选项背景，不改变行高、内容位置或命中区域。Shadcn 在背景上下各保留一个主题线宽。

候选行位于 Popup 的弹层内容中（Popup 是 owner 透明容器，候选行继承 Select 的 owner）；`SelectCandidateListItemTheme` 的全部视觉（前景、
背景、内边距、行高、禁用色、多选指示器颜色与尺寸）以 `SelectTokenResource` 显式读取 Select 的有效值，
`Controls[Select]` 的覆盖作用于候选项。候选项沿用 ListViewItem 的共享状态和度量。

### 2.3 输入内容 padding Token

- `SingleModePadding`
- `SingleModePaddingSM`
- `SingleModePaddingLG`
- `MultiModePadding`
- `MultiModePaddingSM`
- `MultiModePaddingLG`
- `MultiModePrefixIndent`
- `MultiModePrefixIndentSM`
- `MultiModePrefixIndentLG`
- `SelectAffixPadding`（Reserved）

这些 Token 控制 Select 输入区域和多选内容区域的 padding。单选模式主要使用输入框默认内容布局；多选和 Tags 模式根据是否已有选中项切换普通 padding 与 multi mode padding。

`MultiModePadding*` 的左值服务于标签区域（较小的起点），`MultiModePrefixIndent*` 作为 `PART_ContentLeftAddOn`（prefix）的额外左 Margin，使多选 / Tags 模式下有选中项时 prefix 的左内缩与单选模式一致（对齐 Ant Design：selector 小左内缩 + prefix 自身 margin 补偿）。两者之和等于对应尺寸的单选水平内边距。

`SizeType=Custom` 在主题中与 `Middle` 共享 `SingleModePadding` 和 `MultiModePadding` 分支。它不是独立 Token 组。

`SelectAffixPadding`（antd `selectAffixPadding`）当前没有消费者：prefix 由 `MultiModePrefixIndent*` 对齐，suffix 位于输入帧
padding 内，标为 `[ReservedToken]`，见 §7。

### 2.4 弹层 Token

- `PopupContentPadding`

该 Token 控制 `PopupFrame` 内部 padding，并参与 `MaxPopupHeight` 计算。

### 2.5 外框形态 Token

- `TransparentOutlineBorder`
- `BottomOutlineBorder`
- `MutedSelectIndicator`

由设计语言选择的输入框外框形态：`TransparentOutlineBorder` 为真时外框在常态与悬停时保持透明边框，`BottomOutlineBorder`
为真时只画底边。应用显式设置了对应全局 Token（`ColorBorder`、`ColorPrimaryHover`、`BorderThickness`）时以显式值为准。
`InputControlFrame` 在主题快照变化时读取。Ant Design 与 PrimeNG 均为 `false`；Shadcn Luma、Rhea 透明边框，Sera 只画底边。 `MutedSelectIndicator` 为真时选择指示器使用 `ColorTextDescription`（应用显式设置了
`ColorTextQuaternary` 时不生效），Shadcn Luma 打开。

## 3. 控件专项模型中的 Token 使用

SelectToken 使用路径：

```text
SharedToken / InputControlFrameTheme
   ↓
SelectToken
   ↓
SelectTheme / SelectAddOnDecoratedBoxTheme / SelectTagTheme / SelectCandidateListItemTheme
   ↓
Select input content + tags + candidate list + popup frame
```

运行时派生尺寸：

```text
MaxPopupHeight = ItemHeight * DisplayPageSize
               + PopupContentPadding.Top
               + PopupContentPadding.Bottom
```

`AbstractSelect` 成组读取候选项高度和弹层内边距。这两项任一更新都会刷新高度上限，弹层模板继续使用最终的内边距与尺寸。

`MaxPopupHeight`、`EffectivePopupWidth`、`SelectedCount`、`IsSelectionEmpty` 和 `IsEffectiveFilterEnabled` 都是实例运行状态，不是 Token。

## 4. 控件家族影响

SelectToken 直接影响 Select。TreeSelect 有独立 `TreeSelectToken`，ComboBox 有独立 `ComboBoxToken`，普通 LineEdit / NumericUpDown 不使用 SelectToken。

Cascader 与 TreeSelect 目前仍复用部分 SelectToken：多选内容 padding 与前缀缩进（`MultiModePadding*`、`MultiModePrefixIndent*`、
`SingleModePadding*`、`MultipleItemHeight*`）、多选标签 `SelectTag` 的视觉，以及 TreeSelect 的 `PopupContentPadding`。这些值跟随
`Controls[Select]` 而不是 `Controls[Cascader]` / `Controls[TreeSelect]`；拆分需要一层无 identity 的抽象 Token，见 §7。

调整 SelectToken 时必须评估：

- `SelectTheme.axaml`
- `SelectAddOnDecoratedBoxTheme.axaml`
- `SelectCandidateListItemTheme.axaml`
- `SelectTagTheme.axaml`
- Select token.md 语义说明
- `SelectShowCasePageTests`

## 5. 兼容性要求

SelectToken 属于 Select 主题契约。`SelectToken` 是公开类，主题算法的控件阶段会直接写它的属性，因此类型本身、生成的 `SelectTokenKind`、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名或删除现有 Token。
- 不把实例数据、过滤状态、loading 状态、选择集合、popup 打开状态或 Form 状态迁移为 Token。
- 不在 SelectToken 中复制 `InputControlFrame` 的边框、圆角、focus、error、warning 或 disabled 通用状态 Token。
- 不把 `DisplayPageSize`、`MaxCount`、`MaxTagCount` 或 `IsResponsiveTagMode` 变成 Token；它们是实例行为属性。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改多选标签 Token | 验证 Multiple / Tags 模式下 Large / Middle / Small / Custom 标签高度、间距、关闭按钮和 disabled 状态。 |
| 修改候选项 Token | 验证候选项高度、active/selected 视觉、键盘导航滚动和 `DisplayPageSize` 下的可视行数一致性。 |
| 修改输入 padding Token | 验证单选、多选、Tags、空选择、有选择、prefix/suffix/addon 和 CompactSpace 对齐。 |
| 修改 `PopupContentPadding` | 验证 popup 内边距、候选列表裁剪、loading/empty 视觉和最大高度计算。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |

## 7. 主题读法与裁决记录（Select 家族迁移）

- `SelectTheme` 与仅属于 Select 的部件（`SelectAddOnDecoratedBox`、`SelectResultOptionsBox`、`SelectCandidateListItem`）以
  `SelectTokenResource` 读取 Global Token；Select、Cascader、TreeSelect 共用的部件（`SelectHandle`、`SelectMaxCountIndicator`、
  `SelectTagAwareTextBox`）以 `OwnerTokenResource` 读取宿主的有效值。`SelectHandle` 的指示图标是注册控件 IconPresenter，
  其尺寸在 `SelectHandle` 上解析后向下绑定。
- 输入表面的状态色（hover、focus、error、warning、filled）由 `InputControlFrameTheme` 统一驱动；`SelectAddOnDecoratedBoxTheme`
  只保留下拉展开时的激活边框，并在其后重新投射根 `BorderBrush`。根 `BorderBrush` / `Background` 经 `HostBorderBrush` /
  `HostBackground` 投射，不再以 LocalValue 中继。
- 过滤输入 `SelectFilterTextBox` 跟随宿主 `FontSize`，不再读取 LineEdit 的 `InputFontSize*`（原读法因 `IsCustomFontSize=True`
  从未生效）。

| Token | 裁决 |
| --- | --- |
| `OptionPadding` | Active：候选行内边距（PrimeNG 预设由算法的 Select 控件阶段从尺寸 Token 计算，Aura 为 `12,8`）。 |
| `SelectAffixPadding` | Reserved：`antd Select selectAffixPadding`，前缀由 `MultiModePrefixIndent*` 对齐、后缀位于输入帧 padding 内，暂无独立的前后缀内边距。 |
| `FixedItemMargin` | Reserved：`antd INTERNAL_FIXED_ITEM_MARGIN`，多选标签由标签面板间距与 `MultipleItemHeight*` 布局，暂不施加标签外边距。 |

生效与隔离测试：`tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/SelectFamilyEffectTests*.cs`。

## 独立 Token 身份与共享计算

Select、Cascader、TreeSelect 的标签高度/背景、禁用标签、字体和单/多选 padding 共享 AbstractSelectInputToken，
各终端从自己的有效 Global/Own 计算并经具体内部样式输入应用；`Controls[Select]` 不再改变 Cascader/TreeSelect 的输入表面。
Cascader Small 空多选使用 Small padding。TreeSelect 弹层内的内部树跟随 TreeSelect，实际树展示字段由其 Own 配置，
通过纯值函数复用树公式，不把树字段添加给 Select/Cascader；过滤高亮默认仍为 TreeSelect 的 ColorPrimary。
TreeSelectTreeView 是无身份部件，不再借 TreeView 的 StyleKey 截断宿主 owner；公开 TreeView 和 CascaderView 仍各自独立。
