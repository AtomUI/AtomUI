# ListBox Token 设计

本文档定义 `AtomUI.Desktop.Controls.ListBoxToken` 的 ListBox 专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。ListBox 整体架构见 [ListBox 桌面版架构设计](overview.md)，内部实现原理见 [ListBox 桌面版实现原理](implementation.md)，设计和契约变化记录见 [ListBox Changelog](changelog.md)。

## 1. 定位

ListBoxToken 是 ListBox 的组件级设计变量层。它把全局颜色、尺寸、间距和状态色转换为 ListBox root、ListBoxItem、selected indicator 和 filter highlighter 可消费的语义值。

ListBoxToken 服务以下主题和控件：

- `ListBoxTheme.axaml`
- `ListBoxItemTheme.axaml`
- `CandidateListTheme.axaml`
- `CandidateListItemTheme.axaml`
- `CascaderViewFilterListTheme.axaml`
- `ListBox` / `ListBoxItem` / `CandidateList` / `CandidateListItem`

ListBoxToken 不承载 `SelectedItem`、`SelectedItems`、`IsSelected`、`IsFiltering`、`FilterValue`、`FilterResultCount`、`IsEffectiveEmptyVisible`、`VirtualIndex` 等实例状态。这些状态由 C# 状态模型、容器属性和主题 selector 处理。

## 2. Token 分类

ListBoxToken 当前按 ListBox 语义分为五类。

### 2.1 Root 结构 Token

- `ContentPadding`

`ContentPadding` 控制 ListBox root 内容区 padding，作用于 `ListBoxTheme` 的 root `Frame`。当前默认值为 `Thickness(0)`：条目表面直接贴合 root 外框内边缘，列表紧凑感由条目高度与分割线表达，不再在内容区与外框之间预留内边距。

### 2.2 条目文字 Token

- `ItemColor`
- `ItemHoverColor`
- `ItemSelectedColor`
- `ItemDisabledColor`

这些 Token 表达条目在普通、hover、selected 和 disabled 状态下的文字颜色语义。

默认映射：

- 普通和 hover 使用 `Global.ColorTextSecondary`。
- selected 使用 `Global.ColorText`。
- disabled 使用 `Global.ColorTextDisabled`。

disabled内容presenter使用本控件的ItemDisabledColor，默认读取其有效Global.ColorTextDisabled；明确Own覆盖、透明值及清除必须进入实际文字，不再绕过Own直接读取Shared/子节点Global。

### 2.3 条目背景 Token

- `ItemBgColor`
- `ItemHoverBgColor`
- `ItemSelectedBgColor`

这些 Token 表达条目普通、hover 和 selected 背景。

默认映射：

- 普通背景使用透明色。
- hover 背景使用 `Global.ColorBgTextHover`。
- selected 背景使用 `Global.ControlItemBgActive`。

`ItemHoverBg` 和 `ItemSelectedBg` 是 ListBox public styled property，theme 默认值来自这些 Token，允许实例级覆盖。

### 2.4 条目结构间距 Token

- `ItemPaddingSM`
- `ItemPadding`
- `ItemPaddingLG`
- `ItemMargin`
- `SelectedIndicatorMargin`

这些 Token 控制 ListBoxItem 的密度和选中标记间距：

- `ItemPaddingSM`、`ItemPadding`、`ItemPaddingLG` 分别对应 small、middle/custom、large 尺寸，只表达水平 padding（垂直 padding 为 0），条目高度由 SizeType 分支的 `MinHeight` 控制。
- `ItemMargin` 控制条目之间的垂直间距，当前默认值为 `Thickness(0)`：条目纵向密度由分割线表达，条目之间以及条目与外框之间不留间距。
- `SelectedIndicatorMargin` 控制 selected indicator 与内容区域的距离。

条目最小高度来自 SharedToken，由 SizeType selector 选择。条目表面保持直角，主题不设置条目圆角。条目底部分割线使用 SharedToken `ColorSplit` 与 1 DIP 线宽（条目 `BorderThickness` 默认 `0,0,0,1`），不在 ListBoxToken 中重复定义。

### 2.5 过滤 Token

- `FilterHighlightColor`

`FilterHighlightColor` 控制过滤命中文本高亮前景，默认来自 `Global.ColorError`。过滤是否命中、过滤词、隐藏未命中项和 `FilterResultCount` 属于运行时状态，不进入 Token。

## 3. 控件专项模型中的 Token 使用

ListBoxToken 主要参与以下专项模型：

- SizeType：条目 padding 来自 `ItemPaddingSM`、`ItemPadding`、`ItemPaddingLG`，root 圆角和条目最小高度来自 SharedToken；条目不设圆角。
- selected indicator：`SelectedIndicatorMargin` 控制右侧选中图标与内容之间的间距，图标尺寸和颜色来自 SharedToken。
- hover / selected：`ItemHoverBgColor` 和 `ItemSelectedBgColor` 通过 public property 默认值传递给 ListBoxItem。
- filter：`FilterHighlightColor` 通过 `FilterHighlightForeground` 传递到 `HighlightableTextBlock`。
- CandidateList：CandidateList 基于 ListBoxTheme 和 ListBoxItemTheme，继承 ListBoxToken 的条目色彩、间距和过滤高亮语义。

Token 不参与 selection 更新、CandidateList commit、filter 计算、container recycling 或 empty indicator 判定。

## 4. 控件家族影响

ListBoxToken 影响 ListBox 与 CandidateList 体系：

- `ListBox` root theme 使用 `ContentPadding` 和 `FilterHighlightColor`。
- `ListBoxItem` theme 使用条目文字、背景、padding、margin 和 selected indicator margin。
- `CandidateList` 通过 BasedOn `ListBoxTheme` 继承 root 结构和条目 Token。
- `CandidateListItem` 通过 BasedOn `ListBoxItemTheme` 继承条目 Token，并补充 candidate selected 背景规则。
- `CascaderViewFilterList` 基于 ListBox，用于过滤结果展示时继承 ListBoxToken 语义。

ListBoxToken 不影响 ListViewToken。ListView 和 ListBox 有相似的列表条目语义，但它们各自拥有独立 Token ID 和 resource scope。

## 5. 兼容性要求

维护 ListBoxToken 时必须保持以下要求：

- 不擅自重命名、删除或改变 `ListBoxTokenKind` 对应 Token 语义。
- 不把 selection、filter、empty、candidate selected 或 virtualizing context 状态写入 Token。
- 不把 `SizeType` 拆成实例状态 Token；SizeType 由 theme selector 选择已有尺寸 Token 和 SharedToken。
- 不把 `FilterHighlightStrategy` 的策略结果写入 Token。
- 默认值应继续从 SharedToken 派生，保持 light / dark 主题一致性。
- Token 变更必须同步 Theme 引用和 token.md 语义说明。

## 6. 验证策略

ListBoxToken 变更验证：

- `ListBoxTokenKind` 与 token.md 语义说明保持一致。
- `ContentPadding` 保持 `Thickness(0)`，条目与外框内边缘贴合，不造成 ScrollViewer 与 EmptyIndicator 重叠。
- `ItemColor`、`ItemHoverColor`、`ItemSelectedColor` 和 `ItemDisabledColor` 在 light / dark 主题下可读。
- `ItemBgColor`、`ItemHoverBgColor` 和 `ItemSelectedBgColor` 能正确传递到 ListBoxItem hover / selected 状态。
- `ItemPaddingSM`、`ItemPadding`、`ItemPaddingLG` 与 SizeType selector 对齐。
- `ItemMargin` 保持 `Thickness(0)`，列表扫描密度由分割线表达。
- `SelectedIndicatorMargin` 不造成内容和 selected indicator 重叠。
- `FilterHighlightColor` 能传递到过滤高亮文本。
- CandidateList 和 Cascader filter list 继承后的视觉语义稳定。

## 精确默认与禁用文字

本文Global指实际ListBox身份的有效Global。Own采用DependencyDefaultsV2/Revision1，每字段静态函数只读取所声明的Global输入；保留字段、类型、资源ID和原默认公式，显式Own（含透明/零边距）不会被缺省重算覆盖。本轮字段没有Own间依赖，不添加虚构依赖。

ItemDisabledColor进入原disabled选择器下的ContentPresenter.Foreground，保持原节点与优先级。其默认随本控件有效Global.ColorTextDisabled，明确Own优先；清除恢复有效默认。普通/hover/selected及其他公开子Control的Global职责保持，不能把此颜色修复作为整个列表家族验收。

兼容说明：disabled文本现在与普通/hover/selected文字一样由所属列表Own管理。此前直接通过Shared/条目Global的ColorTextDisabled绕过该Own的配置，应改为对应列表的ItemDisabledColor或生成专用Style；默认颜色不变。继承这些基础主题的Candidate/Cascader过滤项、Select候选项、Transfer项与GroupHeader按当前声明复用该Own，完整业务Host与其他owner接线仍须各家族验证。
