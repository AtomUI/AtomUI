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

- 普通和 hover 使用 `SharedToken.ColorTextSecondary`。
- selected 使用 `SharedToken.ColorText`。
- disabled 使用 `SharedToken.ColorTextDisabled`。

disabled 条目的内容 presenter 前景读取 `ItemDisabledColor`（默认 `ColorTextDisabled`，与 antd 的禁用文字色一致）；维护时必须保持 disabled 文本可读性稳定。

### 2.3 条目背景 Token

- `ItemBgColor`
- `ItemHoverBgColor`
- `ItemSelectedBgColor`

这些 Token 表达条目普通、hover 和 selected 背景。

默认映射：

- 普通背景使用透明色。
- hover 背景使用 `SharedToken.ColorBgTextHover`。
- selected 背景使用 `SharedToken.ControlItemBgActive`。

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

条目最小高度来自全局 Token，由 SizeType selector 选择。条目表面保持直角，主题不设置条目圆角。条目底部分割线使用全局 `ColorSplit` 与 `LineWidth`（主题写入 internal `SplitLineWidth`，只画底边）；应用或 Semantic Part 样式设置条目 `BorderThickness` 时整体替换默认分割线。分割线不在 ListBoxToken 中重复定义。

### 2.5 过滤 Token

- `FilterHighlightColor`

`FilterHighlightColor` 控制过滤命中文本高亮前景，默认来自 `SharedToken.ColorError`。过滤是否命中、过滤词、隐藏未命中项和 `FilterResultCount` 属于运行时状态，不进入 Token。

## 3. 控件专项模型中的 Token 使用

ListBoxToken 主要参与以下专项模型：

- SizeType：条目 padding 来自 `ItemPaddingSM`、`ItemPadding`、`ItemPaddingLG`，root 圆角和条目最小高度来自 SharedToken；条目不设圆角。
- selected indicator：`SelectedIndicatorMargin` 控制右侧选中图标与内容之间的间距，图标尺寸（`IconSize`）和颜色（`ColorPrimary`）来自宿主的有效全局 Token。
- hover / selected：`ItemHoverBgColor` 和 `ItemSelectedBgColor` 通过 public property 默认值传递给 ListBoxItem。
- filter：`FilterHighlightColor` 通过 `FilterHighlightForeground` 传递到 `HighlightableTextBlock`。
- CandidateList：CandidateList 基于共享列表结构，条目色彩、间距和过滤高亮通过宿主有效全局值的内部样式输入提供，不借用 ListBox Own。

Token 不参与 selection 更新、CandidateList commit、filter 计算、container recycling 或 empty indicator 判定。

### 3.1 主题读法与 Token owner

- `ListBoxTheme` 与 `ListBoxItemTheme` 也供透明 CandidateList 和 Cascader 过滤列表复用，因此全局 Token 以
  `OwnerTokenResource` 读取（主题前有 `<!-- owner: -->` 标记）。独立 ListBox 使用 ListBox owner 和 Own；透明候选部件
  跟随 Mentions / AutoComplete / Cascader 等实际宿主，宿主内部样式输入覆盖基础列表默认值。
- `ListBoxItem` 是 Token owner 透明的条目容器（`[TokenOwnerTransparent]`），条目继承宿主列表的 owner：`Controls[ListBox]` 作用于条目的
  颜色、高度、选中标记与过渡时长，`Controls[ListBoxItem]` 不作用于条目。
- 选中标记的样式选择器此前指向 `IconPresenter`，与模板中的 `IconTemplatePresenter` 不匹配，`ColorPrimary` / `IconSize` /
  `SelectedIndicatorMargin` 从未生效；现已修正，选中标记显示为主色、`IconSize` 尺寸。

## 4. 控件家族影响

ListBoxToken 影响独立 ListBox；共享结构的部件边界如下：

- `ListBox` root theme 使用 `ContentPadding` 和 `FilterHighlightColor`。
- `ListBoxItem` theme 使用条目文字、背景、padding、margin 和 selected indicator margin。
- `CandidateList` 通过 BasedOn `ListBoxTheme` 复用 root 结构，颜色和条目输入由实际宿主提供。
- `CandidateListItem` 通过 BasedOn `ListBoxItemTheme` 复用条目结构，并以实际宿主输入提供 candidate selected 背景。
- `CascaderViewFilterList` 基于 ListBox，用于过滤结果展示时采用 Cascader 宿主的样式输入。

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

## 独立 Token 身份与共享计算

独立 ListBox 的 Own 契约保持不变。CandidateList/CandidateListItem 是 Mentions/AutoComplete 的透明部件，其实际颜色与水平 padding 读取宿主 globals，不再借 ListBox Own。
