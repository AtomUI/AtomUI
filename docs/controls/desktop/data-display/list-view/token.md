# ListView Token 设计

本文档定义 `AtomUI.Desktop.Controls.ListViewToken` 的 ListView 专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。ListView 整体架构见 [ListView 桌面版架构设计](overview.md)，内部实现原理见 [ListView 桌面版实现原理](implementation.md)，设计和契约变化记录见 [ListView Changelog](changelog.md)。

## 1. 定位

ListViewToken 是 ListView 的组件级设计变量层。它把全局颜色、尺寸、间距和状态色转换为 ListView root、ListViewItem、组标题、分页器间距和 selected indicator 可消费的语义值。

ListViewToken 服务以下主题和控件：

- `ListViewTheme.axaml`
- `ListViewItemTheme.axaml`
- `ListView`
- `ListViewItem`

ListViewToken 不承载 `ItemsSource`、`SelectedIndex`、`SelectedItem`、`SelectedItems`、`IsSelected`、`IsFiltering`、`FilterValue`、`PageIndex`、`PageSize`、`IsOperating`、`IsEffectiveEmptyVisible`、`VirtualIndex` 等实例状态。这些状态由 C# 状态模型、collection view、容器属性和 theme selector 处理。

## 2. Token 分类

ListViewToken 当前按 ListView 语义分为六类。

### 2.1 Root 结构 Token

- `ContentPadding`

`ContentPadding` 控制 ListView root 内容区 padding，作用于 `ListViewTheme` 的 root `Frame`。当前默认值为 `Thickness(0)`：条目表面直接贴合 root 外框内边缘，列表紧凑感由条目高度与分割线表达，不再在内容区与外框之间预留内边距。

### 2.2 条目文字 Token

- `ItemColor`
- `ItemHoverColor`
- `ItemSelectedColor`
- `ItemDisabledColor`

这些 Token 表达普通数据项在普通、hover、selected 和 disabled 状态下的文字颜色语义。

默认映射：

- 普通和 hover 使用 `SharedToken.ColorTextSecondary`。
- selected 使用 `SharedToken.ColorText`。
- disabled 使用 `SharedToken.ColorTextDisabled`。

组标题文字不使用这些 Token，而使用 `GroupHeaderColor`。

### 2.3 条目背景 Token

- `ItemBgColor`
- `ItemHoverBgColor`
- `ItemSelectedBgColor`

这些 Token 表达普通数据项的普通、hover 和 selected 背景。

默认映射：

- 普通背景使用透明色。
- hover 背景使用 `SharedToken.ColorBgTextHover`。
- selected 背景使用 `SharedToken.ControlItemBgActive`。

`ItemHoverBg` 和 `ItemSelectedBg` 是 ListView public styled property，theme 默认值来自这些 Token，允许实例级覆盖。组标题保持普通背景，不应用 hover / selected 背景规则。

### 2.4 条目结构间距 Token

- `ItemPaddingSM`
- `ItemPadding`
- `ItemPaddingLG`
- `ItemMargin`

这些 Token 控制 ListViewItem 的密度：

- `ItemPaddingSM`、`ItemPadding`、`ItemPaddingLG` 分别对应 small、middle/custom、large 尺寸，只表达水平 padding（垂直 padding 为 0），条目高度由 SizeType 分支的 `MinHeight` 控制。
- `ItemMargin` 控制条目之间的垂直间距，当前默认值为 `Thickness(0)`：条目纵向密度由分割线表达，条目之间以及条目与外框之间不留间距。

条目最小高度来自 SharedToken，由 SizeType selector 选择。条目表面保持直角，主题不设置条目圆角。条目底部分割线使用 SharedToken `ColorSplit` 与 1 DIP 线宽（条目 `BorderThickness` 默认 `0,0,0,1`），不在 ListViewToken 中重复定义。

### 2.5 分页 Token

- `PaginationMargin`

`PaginationMargin` 控制 `TopPaginationPresenter` 和 `BottomPaginationPresenter` 的外边距。它只表达分页器相对列表内容的空间关系，不表达页码、页大小、当前页、单页隐藏或分页器可见性。

### 2.6 分组与选中标记 Token

- `GroupHeaderColor`
- `SelectedIndicatorMargin`

`GroupHeaderColor` 控制组标题项文字颜色，默认来自 `SharedToken.ColorTextDescription`。

`SelectedIndicatorMargin` 控制 selected indicator 与内容区域之间的距离。图标颜色和尺寸来自 SharedToken，不在 ListViewToken 中重复定义。

## 3. 控件专项模型中的 Token 使用

ListViewToken 主要参与以下专项模型：

- SizeType：条目 padding 来自 `ItemPaddingSM`、`ItemPadding`、`ItemPaddingLG`，root 圆角和条目最小高度来自 SharedToken；条目不设圆角。
- data item state：`ItemColor`、`ItemHoverColor`、`ItemSelectedColor`、`ItemDisabledColor` 和背景 Token 表达普通条目的状态视觉。
- group item：`GroupHeaderColor` 表达组标题弱化文字，不参与普通 selected 背景模型。
- pagination：`PaginationMargin` 表达分页器与列表内容的外边距。
- selected indicator：`SelectedIndicatorMargin` 控制右侧图标与内容间距，图标尺寸和颜色来自 SharedToken。

Token 不参与 selection 更新、filter 计算、sort、group build、page move、container recycling、empty indicator 判定或 operating overlay。

## 4. 控件家族影响

ListViewToken 影响 ListView 体系：

- `ListView` root theme 使用 `ContentPadding` 和 `PaginationMargin`。
- `ListViewItem` theme 使用条目文字、背景、padding、margin、group header color 和 selected indicator margin。
- ListView token.md 说明 `ListViewToken` 的稳定语义，Gallery List ShowCase 只提供示例。

ListViewToken 不影响 ListBoxToken。ListView 和 ListBox 有相似的列表条目语义，但它们各自拥有独立 Token ID 和 resource scope。

## 5. 兼容性要求

维护 ListViewToken 时必须保持以下要求：

- 不擅自重命名、删除或改变 `ListViewTokenKind` 对应 Token 语义。
- 不把 selection、filter、sort、group、pagination、empty、operating 或 virtualizing context 状态写入 Token。
- 不把 `SizeType` 拆成实例状态 Token；SizeType 由 theme selector 选择已有尺寸 Token 和 SharedToken。
- `PaginationMargin` 只控制分页器外边距，不表达分页器 visibility、align 或 page state。
- `GroupHeaderColor` 只控制组标题视觉，不表达分组算法或 group key。
- 默认值应继续从 SharedToken 派生，保持 light / dark 主题一致性。
- Token 变更必须同步 Theme 引用和 token.md 语义说明。

## 6. 验证策略

ListViewToken 变更验证：

- `ListViewTokenKind` 与 token.md 语义说明保持一致。
- `ContentPadding` 保持 `Thickness(0)`，条目与外框内边缘贴合，不造成 ScrollViewer、EmptyIndicator、Spin 和分页器重叠。
- `ItemColor`、`ItemHoverColor`、`ItemSelectedColor` 和 `ItemDisabledColor` 在 light / dark 主题下可读。
- `ItemBgColor`、`ItemHoverBgColor` 和 `ItemSelectedBgColor` 能正确传递到普通 ListViewItem hover / selected 状态。
- `ItemPaddingSM`、`ItemPadding`、`ItemPaddingLG` 与 SizeType selector 对齐。
- `ItemMargin` 保持 `Thickness(0)`，列表扫描密度由分割线表达。
- `PaginationMargin` 在 top、bottom 和 both 分页器场景下保持稳定间距。
- `GroupHeaderColor` 在分组列表中可读且弱于普通条目。
- `SelectedIndicatorMargin` 不造成内容和 selected indicator 重叠。
