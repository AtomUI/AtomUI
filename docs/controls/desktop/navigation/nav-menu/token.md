# NavMenu Token 设计

本文档定义 `AtomUI.Desktop.Controls.NavMenuToken` 的 NavMenu 专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。NavMenu 整体架构见 [NavMenu 桌面版架构设计](overview.md)，内部实现原理见 [NavMenu 桌面版实现原理](implementation.md)，设计和契约变化记录见 [NavMenu Changelog](changelog.md)。

## 1. 定位

NavMenuToken 是 NavMenu 的组件级设计变量层。它把全局颜色、尺寸、间距、圆角、字体、动效和 popup 体系转换为 NavMenu 可消费的语义值。

NavMenuToken 服务以下主题：

- `NavMenuTheme.axaml`
- `NavMenuItemTheme.axaml`
- `NavMenuGroupItemTheme.axaml`
- `NavMenuDividerItemTheme.axaml`
- `BaseNavMenuItemHeaderTheme.axaml`
- `HorizontalNavMenuItemHeaderTheme.axaml`
- `VerticalNavMenuItemHeaderTheme.axaml`
- `InlineNavMenuItemHeaderTheme.axaml`

NavMenuToken 不承载 `SelectedItem`、`IsSubMenuOpen`、`IsInSelectedPath`、`IsPointerOverSubMenu`、`Level`、`IsTopLevel` 等实例状态。这些状态由控件状态模型、容器层和主题 selector 处理。

## 2. Token 分类

NavMenuToken 当前按 NavMenu 语义分为八类。

### 2.1 根与容器背景 Token

- `ItemBg`
- `DarkMenuBg`
- `MenuSubMenuBg`

用于 root 菜单背景和子菜单容器背景。`ItemBg` 对应 light root 背景，`DarkMenuBg` 对应 dark root 背景。背景 token 不等同于 header 背景；header 默认背景应保持透明，再由 hover / selected state 决定。

### 2.2 菜单项文字与状态 Token

- `ItemColor`
- `ItemHoverColor`
- `ItemSelectedColor`
- `ItemDisabledColor`
- `HorizontalItemHoverColor`
- `HorizontalItemSelectedColor`
- `KeyGestureColor`（Reserved：当前无快捷键文字节点，保留配置兼容，不是可用视觉入口）
- `GroupTitleColor`
- `GroupTitleLineHeight`
- `GroupTitleFontSize`

用于菜单项前景、快捷键、分组标题和 horizontal 顶层文字状态。`GroupTitleColor`、`GroupTitleLineHeight`、`GroupTitleFontSize` 只服务 `NavMenuGroupItem` 的非交互标题，不应复用菜单项 selected、hover 或 disabled 状态。`ItemSelectedColor` 同时服务 selected leaf 和 selected path ancestor 的前景语义。

### 2.3 菜单项背景与动效 Token

- `ItemHoverBg`
- `ItemActiveBg`
- `ItemSelectedBg`
- `SubMenuItemBg`
- `HorizontalItemHoverBg`
- `HorizontalItemSelectedBg`
- `ItemBackgroundMotionEasing`

用于 header hover、active、selected、背景状态过渡以及 inline submenu 背景块。`SubMenuItemBg` 只应用于 inline child frame，不应被用作 root menu 背景。`ItemBackgroundMotionEasing` 是所有 NavMenu item header 背景 transition 的统一缓动入口，默认值为 CSS `ease` 等价曲线 `0.25,0.1,0.25,1`；Base、Inline、Horizontal 主题必须通过 `NavMenuTokenResource` 消费它，不在 AXAML 中复制曲线常量。

### 2.4 尺寸、圆角与间距 Token

- `ItemHeight`
- `ItemContentMargin`
- `ItemContentPadding`
- `ItemMargin`
- `VerticalItemsPanelSpacing`
- `VerticalChildItemsMargin`
- `VerticalMenuContentPadding`
- `HorizontalItemMargin`
- `ItemBorderRadius`
- `SubMenuItemBorderRadius`
- `InlineCollapsedWidth`
- `CollapsedWidth`

用于菜单项高度、参考 block margin 映射、header 内边距、inline child frame 外距、菜单内容 padding 和 inline collapsed 根宽度。`ItemContentMargin` 表达 item header 的外部 margin；`VerticalChildItemsMargin` 表达 inline submenu 背景块在背景模式下的外部 margin，不是所有 inline 子菜单的无条件间距。

`InlineCollapsedWidth` 是 `Mode=Inline && IsInlineCollapsed=true` 的折叠宽度默认值，初始设计值为 `48`。控件实例上的 `NavMenu.InlineCollapsedWidth` 本地值优先级高于 token setter。该 token 只提供主题默认布局值，不表达实例是否折叠，也不参与 `Vertical` / `Horizontal` 模式测量。

`CollapsedWidth` 是既有折叠宽度 token，保留兼容，不删除、不重命名。新的 inline collapsed 设计、文档和实现应优先使用 `InlineCollapsedWidth` 表达内联折叠菜单宽度，避免把旧 token 继续扩展为多语义 token。

`NavMenu.ItemSpacing` 是实例级布局输入。默认主题按 mode 将 `VerticalItemsPanelSpacing` 投影到 Inline/Vertical 根层，并通过内部 `EntryItemSpacing` 投影到 popup、submenu 和分组的默认 ItemsPanel；Horizontal 根层公开值默认使用 `0`，但其后代默认值仍来自 `VerticalItemsPanelSpacing`。调用方显式设置 `ItemSpacing` 时，包括显式设置为 `0`，该值同时覆盖根与后代默认 ItemsPanel，并作为相邻 entry 容器之间的额外间距叠加在 item 自身 margin 之外；它不改写任何 Token，也不要求自定义 ItemsPanel 消费该属性。

### 2.5 Icon 与箭头 Token

- `ItemIconSize`
- `IconSize`
- `IconMargin`
- `CollapsedIconSize`
- `MenuArrowSize`
- `InlineItemIndentUnit`

用于菜单项图标、horizontal 顶层图标间距、箭头尺寸和 inline 缩进。`InlineItemIndentUnit` 默认来自 `ItemHeight / 2`，使层级缩进与菜单项高度保持比例关系。

`CollapsedIconSize` 继续作为 inline collapsed 顶层图标尺寸 token，默认映射全局 `IconSizeLG`，比普通菜单项使用的 `ItemIconSize=IconSize` 大一档。它只控制图标尺寸，不控制折叠宽度；折叠宽度由 `InlineCollapsedWidth` 或控件本地属性值决定。

### 2.6 Popup Token

- `MenuPopupBg`
- `DarkMenuPopupBg`
- `MenuPopupContentPadding`
- `MenuPopupMinWidth`
- `MenuPopupMaxWidth`
- `MenuPopupMaxHeight`
- `TopLevelItemPopupMarginToAnchor`

用于 `Vertical` / `Horizontal` 弹出式子菜单。Popup 视觉不能直接使用 shared elevated background 绕过组件 token，因为 NavMenu popup 背景和 root/background/dark style 是稳定主题契约。

`MenuPopupMaxHeight` 的默认值为 `ItemHeight * 8`。短菜单按内容自然高度测量；长菜单由 popup 内部滚动容器承载，避免弹出层随条目数量无限增长。主题可以覆盖该 token 调整可见范围，但不应通过改变 `ItemHeight` 间接控制 popup 容量。

### 2.7 Horizontal 导航 Token

- `MenuHorizontalHeight`
- `HorizontalLineHeight`
- `HorizontalItemMargin`
- `HorizontalItemBorderRadius`
- `ActiveBarScaleX`
- `ActiveBarHeight`
- `HorizontalItemHoverBg`
- `HorizontalItemSelectedBg`

用于 horizontal 顶层菜单高度、文字行高、active indicator、水平 item margin 和 light/dark selected 表达。Light style 顶层选中主要通过 active indicator 表达；dark style 可以通过 selected background 表达。

### 2.8 Dark 与 Danger Token

- `DarkItemColor`
- `DarkDangerItemColor`（Reserved：当前无公开危险项入口）
- `DarkItemBg`
- `DarkSubMenuItemBg`
- `DarkItemSelectedColor`
- `DarkItemSelectedBg`
- `DarkItemHoverBg`
- `DarkGroupTitleColor`
- `DarkItemHoverColor`
- `DarkItemDisabledColor`
- `DarkDangerItemSelectedBg`（Reserved：当前无公开危险项入口）
- `DarkDangerItemHoverColor`（Reserved：当前无公开危险项入口）
- `DarkDangerItemSelectedColor`（Reserved：当前无公开危险项入口）
- `DarkDangerItemActiveBg`（Reserved：当前无公开危险项入口）
- `DangerItemColor`（Reserved：当前无公开危险项入口）
- `DangerItemHoverColor`（Reserved：当前无公开危险项入口）
- `DangerItemSelectedColor`（Reserved：当前无公开危险项入口）
- `DangerItemActiveBg`（Reserved：当前无公开危险项入口）
- `DangerItemSelectedBg`（Reserved：当前无公开危险项入口）

Dark token 服务 NavMenu dark style，不应由 light token 自动反推。Danger token 保留菜单危险语义的主题能力；即使当前节点模型没有公开危险节点 API，也不应随意删除或重命名。

## 3. NavMenu 专项模型中的 Token 使用

### 3.1 参考 Menu 间距映射

NavMenu 使用 `ItemContentMargin` 映射 参考设计体系 `itemMarginInline` 与 `itemMarginBlock`：

```text
ItemContentMargin = marginXXS, 0, marginXXS, marginXXS
VerticalItemsPanelSpacing = 0
VerticalMenuContentPadding = 0, marginXXS, 0, 0
```

相邻 item 的垂直间距由 item header 的 bottom margin 表达，不通过 StackPanel spacing 叠加。

上述规则描述主题默认值：`VerticalItemsPanelSpacing=0`，因此默认视觉仍只由 item block margin 决定。开发者显式设置 `NavMenu.ItemSpacing` 时，新增的 panel spacing 是受控实例定制，不应被折算进 `ItemContentMargin` 或写回 Token。

inline 子菜单第一项与父 header 之间不增加额外顶部 margin。inline 子菜单背景块与下一个根项之间的背景模式 gap 由 `VerticalChildItemsMargin` 表达，并且只在 `IsItemBackgroundEnabled=true` 时由主题 selector 应用。

### 3.2 Root、Popup、Header 与 Inline 背景分离

背景相关 token 必须按职责使用：

| 背景职责 | Token |
| --- | --- |
| Light root menu | `ItemBg` |
| Dark root menu | `DarkMenuBg` |
| Light popup menu | `MenuPopupBg` |
| Dark popup menu | `DarkMenuPopupBg` |
| Light inline submenu background block | `SubMenuItemBg` |
| Dark inline submenu background block | `DarkSubMenuItemBg` |
| Header hover | `ItemHoverBg` / `DarkItemHoverBg` |
| Header selected | `ItemSelectedBg` / `DarkItemSelectedBg` |

不能用一个背景 token 同时承担 root、popup、header 和 inline submenu block。这样会破坏 `IsItemBackgroundEnabled`、dark style 和 popup 视觉的独立性。

### 3.3 Selection 与 Horizontal Active Indicator

选中状态使用两组 token：

- 文字：`ItemSelectedColor`、`DarkItemSelectedColor`、`HorizontalItemSelectedColor`
- 背景/指示：`ItemSelectedBg`、`DarkItemSelectedBg`、`HorizontalItemSelectedBg`、`ActiveBarScaleX`、`ActiveBarHeight`

Horizontal light style 的顶层选中主要由 `PART_ActiveIndicator` 表达，背景保持透明。Dark style 顶层选中可以使用 `DarkItemSelectedBg`。

所有 item header 的背景状态切换共用 `ItemBackgroundMotionEasing`。该 Token 只定义缓动曲线；过渡时长继续使用有效的全局 `MotionDurationSlow`，全局关闭 motion 时由主题编译器把标准 duration 归零。

### 3.4 Group 与 Divider

分组标题按语义使用：

| 场景 | Token |
| --- | --- |
| Light group title | `GroupTitleColor`、`GroupTitleFontSize`、`GroupTitleLineHeight` |
| Dark group title | `DarkGroupTitleColor`、`GroupTitleFontSize`、`GroupTitleLineHeight` |
| Group entry spacing | 内部 `EntryItemSpacing` 的当前 effective value；默认由 `VerticalItemsPanelSpacing` 映射，显式 `NavMenu.ItemSpacing` 可以覆盖 |

根层 inline collapsed 和 Horizontal 根层隐藏分组标题，但不改变 title token 的含义；popup 或非根语义层级恢复正常标题视觉。Divider 的线宽和颜色使用 shared `LineWidth` / `ColorSplit` 语义，不新增 NavMenu 专属 divider token。Divider orientation 由 mode 和 semantic top-level 状态决定，不进入 Token。

## 4. 控件家族影响

NavMenuToken 当前服务 NavMenu 及其 internal container/header 体系：

- `NavMenu`
- `NavMenuItem`
- `NavMenuGroupItem`
- `NavMenuDividerItem`
- `HorizontalNavMenuItemHeader`
- `VerticalNavMenuItemHeader`
- `InlineNavMenuItemHeader`
- `BaseNavMenuItemHeader`

Token 修改必须同时评估三种 mode、light/dark 两套主题、root/popup/inline child 三类背景和 Gallery Navigation/Menu Showcase。

NavMenuToken 不服务 Breadcrumb、Pagination、Steps 或 TabControl。导航分类内的其他控件不能直接复用 NavMenuToken 来表达自身状态。

## 5. 兼容性要求

NavMenuToken 属于 NavMenu 主题契约。即使 `NavMenuToken` 是 internal 类型，生成的 `NavMenuTokenKind` 和 AXAML resource 使用点已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名或删除既有 Token。
- 不改变既有 Token 的语义含义。
- 不把实例状态迁移到 Token。
- 不把 root、popup、header、inline submenu block 背景合并为同一职责。
- 不在 Base、Inline 或 Horizontal header 主题中直接构造背景 easing；统一使用 `ItemBackgroundMotionEasing`。
- 不让 `VerticalChildItemsMargin` 在 `IsItemBackgroundEnabled=false` 时影响布局。
- 不把 参考 block margin 映射改为 StackPanel spacing 叠加。
- `VerticalItemsPanelSpacing` 的主题默认值保持 `0`；显式 `NavMenu.ItemSpacing` 可以增加实例级 panel spacing，但不能反向改写 `ItemContentMargin` 或 Token。
- `GroupTitleColor`、`DarkGroupTitleColor`、`GroupTitleLineHeight`、`GroupTitleFontSize` 保持非交互分组标题语义，不参与节点选择、hover 或命令状态。
- 不把 `InlineCollapsedWidth` 写成实例状态；`IsInlineCollapsed` 是状态，`InlineCollapsedWidth` / `NavMenuToken.InlineCollapsedWidth` 是布局输入。
- 不删除或重命名 `CollapsedWidth`；它是既有兼容 token，新的 inline collapsed 宽度语义使用 `InlineCollapsedWidth`。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 新增 NavMenuToken | 检查生成的 `NavMenuTokenKind`、AXAML 引用和默认值计算。 |
| 修改颜色 Token | 覆盖 light / dark root、popup、inline child、header hover、selected path。 |
| 修改背景动效 Token | 覆盖默认曲线、Control Token override、Base/Inline/Horizontal header 消费和 motion disabled。 |
| 修改间距 Token | 运行 `NavMenuLayoutTests`，覆盖 root inset、inline child gap、popup inset 和 `IsItemBackgroundEnabled` true/false。 |
| 修改分组标题 Token | 覆盖 light/dark、root、nested、popup、Horizontal root 和 inline collapsed 标题可见性及排版。 |
| 修改 `VerticalItemsPanelSpacing` 映射 | 覆盖 root、submenu、group 的默认 spacing，并验证显式 `NavMenu.ItemSpacing` 覆盖和纯默认视觉不变。 |
| 修改 inline collapsed 宽度 Token | 覆盖 `InlineCollapsedWidth` 默认值、本地属性覆盖、`Mode=Inline && IsInlineCollapsed=true` 生效和 `Vertical` / `Horizontal` 不受影响。 |
| 修改 popup Token | 覆盖 vertical/horizontal popup 背景、尺寸、padding 和 overlay popup 行为。 |
| 修改 horizontal Token | 覆盖 active indicator、top-level margin、line height 和 dark selected background。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步所有 AXAML 引用和生成文件。 |

## 未公开能力的兼容字段

KeyGestureColor和十个DangerItem/DarkDangerItem颜色字段归Reserved。当前NavMenu条目/数据/状态与模板没有快捷键文字或危险项入口，不能把导出字段当作已公开能力。保留名称、类型、ID、原默认计算及配置往返；Token表/工作台标记历史保留且不可编辑。未来启用须另定义API、状态、Semantic Part和默认兼容合同。Menu的实际KeyGestureColor节点不受此分类影响。
