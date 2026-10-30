# NavMenu Semantic Part 契约

本文档定义 `NavMenu` 家族对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。NavMenu 的整体设计见
[NavMenu 桌面版架构设计](overview.md)，descriptor 与真实模板节点映射见
[NavMenu 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. 上游基线与 Owner 边界

上游基线为 6.6.3 稳定发布源码公开的 Semantic DOM 契约。公开键路径共 12 个，分为三组：

```text
Menu：      root、itemTitle、list、item、itemIcon、itemContent
SubMenu：   subMenu.item、subMenu.itemTitle、subMenu.list、subMenu.itemContent、subMenu.itemIcon
Popup：     popup.root
```

键由真实节点消费，语义分工必须按上游读法理解，不能按名字猜测：

- `itemTitle` / `list` 与 `subMenu.itemTitle` / `subMenu.list` 是 **ItemGroup（分组）的标题与列表**，不是子菜单自身的
  容器。`menu.tsx` 把 `itemTitle` / `list` 作为 ItemGroup 的 `listTitle` / `list` 传入 rc-menu，`SubMenu.tsx` 把
  `subMenu.itemTitle` / `subMenu.list` 以同样方式传入子菜单内的 ItemGroup；落点是 `.ant-menu-item-group-title` 与
  `.ant-menu-item-group-list`。
- `item` / `itemIcon` / `itemContent` 描述**一级菜单项**，`subMenu.item` / `subMenu.itemIcon` / `subMenu.itemContent`
  描述**子菜单内的菜单项**。`MenuItem.tsx` 用 `firstLevel` 分支在两组键之间二选一，二者落在同一个 DOM class
  （`.ant-menu-item`、`ant-menu-item-icon`、`.ant-menu-title-content`）上——区分来自语义键，不来自 DOM class。
- `popup.root` 是子菜单弹层框体。上游模式语义：`itemTitle` / `list` 在 horizontal 模式不生效，`popup` 在 inline 模式
  不生效。

本契约的硬性对齐目标是：**Part 名称逐字等于上述 12 个键路径**，不新增、不改名、不合并。

### 1.1 唯一 owner

`NavMenu` 家族只有一个 public owner：

| 类型 | 可见性 | 是否持有 descriptor |
| --- | --- | --- |
| `NavMenu` | public | 是，12 个键路径全部声明在它上面（`root` 隐式）。 |
| `NavMenuItem` | internal | 否。 |
| `NavMenuGroupItem` | internal | 否。 |
| `NavMenuDividerItem` | internal | 否。 |
| `NavMenuPopupFrame` | internal | 否，是 `popup.root` 的物理节点。 |
| `HorizontalNavMenuItemHeader` / `InlineNavMenuItemHeader` | public | 否，只承载 `itemIcon` / `itemContent` 的 marker。 |
| `VerticalNavMenuItemHeader` | internal | 否，同上。 |

Semantic Part 的 owner 必须是 non-generic public Control，`ContractType` 必须是 public `StyledElement`。容器类型是
internal，既不能成为第二个 owner，也不能作为 `ContractType`，因此**不能**像 `TreeView` / `TreeViewItem` 那样按递归
owner 拆分；菜单项容器的 `ContractType` 取其最近的 public 基类 `HeaderedSelectingItemsControl`。

这与 `TreeView` 的差别是事实性的：`TreeViewItem` 是 public，所以它能成为递归 owner；`NavMenuItem` 不是。

### 1.2 层级区分靠互斥 marker，不靠 DOM class

上游用"同一个 DOM class + 两个语义键"表达一级与子菜单两级。AtomUI 只有一个 owner，无法用 owner scope 区分层级，
而 `>>` descendant 会同时命中一级项，因此层级必须编码在 **marker class** 上：

| 语义层级 | 菜单项容器 marker | 分组容器 marker |
| --- | --- | --- |
| 一级（`IsTopLevel=true`） | `.semantic-item` | `.semantic-scope-group` |
| 子菜单内（`IsTopLevel=false`） | `.semantic-sub-menu-item` | `.semantic-sub-menu-group` |

两个层级互斥：`NavMenuEntryContainerCoordinator` 在 prepare 时先移除另一层级的类，再幂等补齐当前层级的类。分组的
"一级"按语义层级而非容器层级判定——位于一级分组内部的项与分组仍是一级（`context.IsTopLevel` 由最近的结构 owner
决定，分组对层级透明）。

`.semantic-scope-group` / `.semantic-sub-menu-group` 是路由跳点，不是 Part，不发布 descriptor。

### 1.3 route 使用 `>>`

容器由 `ItemsControl` 在运行时生成，不在 `NavMenu` 自身模板内，因此 route 无法用 `/template/` 到达；层级 marker 也
不是"owner 的模板节点"。这里使用的正是 `>>` 的既有语义：沿逻辑祖先链定位锚点。所有非 root Part 的 route 都以 `>>`
开头，按生成器 route 语法（以 `>>` 开头仅允许 `CrossNestedOwners=true` 的部件）声明 `CrossNestedOwners`。

弹层框体还额外位于 `Popup.Child` 属性值子树，`popup.root` 因此同时声明 `CrossVisualRoot=true`，与 Drawer 对内部容器

子菜单内的容器与文本节点（`subMenu.item`、`subMenu.itemIcon`、`subMenu.itemContent`、`subMenu.itemTitle`、
`subMenu.list`）在 Vertical / Horizontal 模式下同样位于菜单项模板的 `Popup.Child` 子树：它们不是 `NavMenu` 模板内的节点，
预览（以及任何按 `CrossVisualRoot` 扫描附加根的消费者）必须进入弹层根才能解析到它们，因此这 5 个 Part 也声明
`CrossVisualRoot=true`。Inline 模式下这些节点位于主视觉树内，声明不影响其可达性。
弹层内容的处理一致。

所有 Part 的 `Since` 统一为 `6.2.0`。

## 2. Semantic Parts

`NavMenu` descriptor 的 Part 集合（`root` 隐式，其余 11 个按路径排序）：

| Part | SelectorClass | SelectorRoute | ContractType | Cardinality | CrossVisualRoot | CrossNestedOwners | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `root` | 不适用 | 不适用 | `NavMenu` | `Single` | `false` | `false` | `false` |
| `item` | `.semantic-item` | `>> .semantic-item` | `HeaderedSelectingItemsControl` | `Multiple` | `false` | `true` | `true` |
| `itemContent` | `.semantic-item-content` | `>> .semantic-item /template/ .semantic-scope-header /template/ .semantic-item-content` | `ContentPresenter` | `Multiple` | `false` | `true` | `true` |
| `itemIcon` | `.semantic-item-icon` | `>> .semantic-item /template/ .semantic-scope-header /template/ .semantic-item-icon` | `IconPresenter` | `Multiple` | `false` | `true` | `true` |
| `itemTitle` | `.semantic-item-title` | `>> .semantic-scope-group /template/ .semantic-item-title` | `ContentPresenter` | `Multiple` | `false` | `true` | `true` |
| `list` | `.semantic-list` | `>> .semantic-scope-group /template/ .semantic-list` | `ItemsPresenter` | `Multiple` | `false` | `true` | `true` |
| `popup.root` | `.semantic-popup-root` | `>> .semantic-popup-root` | `Border` | `Multiple` | `true` | `true` | `true` |
| `subMenu.item` | `.semantic-sub-menu-item` | `>> .semantic-sub-menu-item` | `HeaderedSelectingItemsControl` | `Multiple` | `true` | `true` | `true` |
| `subMenu.itemContent` | `.semantic-sub-menu-item-content` | `>> .semantic-sub-menu-item /template/ .semantic-scope-header /template/ .semantic-sub-menu-item-content` | `ContentPresenter` | `Multiple` | `true` | `true` | `true` |
| `subMenu.itemIcon` | `.semantic-sub-menu-item-icon` | `>> .semantic-sub-menu-item /template/ .semantic-scope-header /template/ .semantic-sub-menu-item-icon` | `IconPresenter` | `Multiple` | `true` | `true` | `true` |
| `subMenu.itemTitle` | `.semantic-sub-menu-item-title` | `>> .semantic-sub-menu-group /template/ .semantic-sub-menu-item-title` | `ContentPresenter` | `Multiple` | `true` | `true` | `true` |
| `subMenu.list` | `.semantic-sub-menu-list` | `>> .semantic-sub-menu-group /template/ .semantic-sub-menu-list` | `ItemsPresenter` | `Multiple` | `true` | `true` | `true` |

生成 Style 类型（`AtomUI.Theme.Styling`）：

```text
item                  -> NavMenuItemStyle
itemIcon              -> NavMenuItemIconStyle
itemContent           -> NavMenuItemContentStyle
itemTitle             -> NavMenuItemTitleStyle
list                  -> NavMenuListStyle
popup.root            -> NavMenuPopupRootStyle
subMenu.item          -> NavMenuSubMenuItemStyle
subMenu.itemIcon      -> NavMenuSubMenuItemIconStyle
subMenu.itemContent   -> NavMenuSubMenuItemContentStyle
subMenu.itemTitle     -> NavMenuSubMenuItemTitleStyle
subMenu.list          -> NavMenuSubMenuListStyle
```

`root` 是隐式 Part：不声明 `.semantic-root` marker，不生成 Style，通过 owner 属性、owner-scoped Style 或替换
ControlTheme 定制。

### 2.1 Part 说明

- `item` / `subMenu.item`：菜单项容器。一级与子菜单两级共用同一个 public 容器类型，靠互斥 marker 区分。承载 header
  呈现、子菜单展开、选择路径、禁用、命令投影与 hover / pressed / keyboard-active 状态。一级项包含位于一级分组内部的
  项；子菜单项包含位于子菜单内分组内部的项。
- `itemIcon` / `subMenu.itemIcon`：菜单项图标区域，节点是 header 模板中的 `IconPresenter#ItemIconPresenter`。节点常驻，
  `Icon` 为 null 时只是隐藏，marker 不增删。
- `itemContent` / `subMenu.itemContent`：菜单项文字内容区域，节点是 header 模板中的 `ContentPresenter#ItemTextPresenter`。
  inline collapsed 下 `VerticalNavMenuItemHeader` 用 `CollapsedTitlePresenter` 呈现首字符，该节点不属于本 Part。
- `itemTitle` / `subMenu.itemTitle`：分组标题区域，节点是 `NavMenuGroupItem` 模板中的
  `ContentPresenter#PART_HeaderPresenter`。上游 horizontal 模式不渲染一级分组标题，AtomUI 下节点存在但被主题隐藏。
- `list` / `subMenu.list`：分组列表区域，节点是 `NavMenuGroupItem` 模板中的 `ItemsPresenter#PART_ItemsPresenter`。
- `popup.root`：子菜单弹层框体，节点是 Horizontal / Vertical 模板中的 `NavMenuPopupFrame#PART_PopupFrame`。Inline 模式
  在视觉树内展开，模板没有弹层节点，因此该模式的实例数为 0。

## 3. marker 放置与路由

`root` 是隐式 Part，不声明 `.semantic-root` marker。非 root Part 的 marker 放置：

| marker | 放置方式 | 位置 |
| --- | --- | --- |
| `.semantic-item` / `.semantic-sub-menu-item` | 运行时按层级幂等注入 | `NavMenuEntryContainerCoordinator` 的 `NavMenuItem` prepare 分支。 |
| `.semantic-scope-group` / `.semantic-sub-menu-group` | 运行时按层级幂等注入 | 同一协调器的 `NavMenuGroupItem` prepare 分支（分组路由跳点，不是 Part）。 |
| `.semantic-scope-header` | 静态模板节点 | `NavMenuItemTheme.axaml` 三个 mode 模板的 header 节点（路由跳点，不是 Part）。 |
| `.semantic-item-icon` / `.semantic-sub-menu-item-icon` | 静态模板节点 | 三个 header 主题的 `IconPresenter#ItemIconPresenter`；同一节点同时携带两个层级 marker，由容器 anchor 决定命中哪个 Part。 |
| `.semantic-item-content` / `.semantic-sub-menu-item-content` | 静态模板节点 | 三个 header 主题的 `ContentPresenter#ItemTextPresenter`；同上。 |
| `.semantic-item-title` / `.semantic-sub-menu-item-title` | 静态模板节点 | `NavMenuGroupItemTheme.axaml` 的 `ContentPresenter#PART_HeaderPresenter`；同上。 |
| `.semantic-list` / `.semantic-sub-menu-list` | 静态模板节点 | `NavMenuGroupItemTheme.axaml` 的 `ItemsPresenter#PART_ItemsPresenter`；同上。 |
| `.semantic-popup-root` | 静态模板节点 | `NavMenuItemTheme.axaml` 的 Horizontal / Vertical 模板 `NavMenuPopupFrame#PART_PopupFrame`。 |

### 3.1 route 组合符契约与 `>>` 的特殊含义

三个组合符的语义由 Avalonia 选择器实现确定，不是可互换的"宽松 / 严格"写法：

| 组合符 | Avalonia 实现 | 匹配条件 |
| --- | --- | --- |
| `/template/` | `TemplateSelector` | 当前节点的直接 `TemplatedParent` 必须匹配前一段。 |
| `>` | `ChildSelector.Evaluate` → `ILogical.LogicalParent` | 当前节点的**直接逻辑父级**必须匹配前一段，不检查更远祖先。 |
| `>>` | `DescendantSelector.Evaluate` → 逐级 `LogicalParent` | 沿**完整逻辑祖先链**匹配任意一级；祖先链上有动态条件时保留 activator。 |

`>>` 在 AtomUI 中是专用机制，有三个必须遵守的边界：

1. **它是唯一能到达"属性值子树"锚点的步进。** 挂在属性值上的子树（如 `Popup.Child` 内容、不参与 `TemplatedParent`
   传播的节点）没有 `/template/` 关系，只能用 `>>` 从 owner 沿逻辑祖先链定位锚点。这正是它存在的理由，而不是
   "宽松匹配"。
2. **以 `>>` 开头只允许 `CrossNestedOwners=true` 的部件。** 生成器按 route 语法校验：route 只能以 `/template/`、`>`
   或（仅 `CrossNestedOwners=true` 部件）`>>` 开始，否则报 `ATOMUIGEN027`。route 中间的 `>>` 不受该限制（先例：
   `popup.list` 使用 `/template/ .semantic-popup-root >> .semantic-popup-list`）。
3. **它不会在最近的语义 owner 处停止。** `DescendantSelector` 遍历全部逻辑祖先，因此不能把 `>>` 当作面向用户的
   Selector 放宽手段，也不能用它替代 owner scope；公共 route 里的 `>>` 必须由显式 marker 类约束命中范围。这也是
   NavMenu 必须用互斥层级 marker 的原因：`>> .semantic-item` 会同时命中一级项，只有把层级编码进 class 才能隔离。

### 3.2 为什么不能用 `/template/` 或 `>`

- **不能用 `>`**：菜单项容器的直接逻辑父级是生成它的 `ItemsControl`——一级项是 `NavMenu`，但**一级分组内部的项**是
  该 `NavMenuGroupItem`，子菜单项是其父 `NavMenuItem`。`>` 无法统一覆盖，且无法区分 `item` 与 `subMenu.item`。
- **不能用 `/template/`**：容器不是 `NavMenu` 模板里的节点，而是运行时生成物；`popup.root` 的节点还在 `Popup.Child`
  属性值子树里。
- **层级隔离必须靠 class**：见 3.1 第 3 条。

### 3.3 可达性依据（源码事实）

- 容器由 `ItemsControl` 生成，Avalonia 的 `PanelContainerGenerator.InsertContainer` 会执行
  `itemsControl.AddLogicalChild(container)`，因此容器的逻辑祖先链最终回到 `NavMenu`，`DescendantSelector` 可达；这条
  链与容器渲染在普通视觉树还是 `PART_Popup` 内无关。
- 模板内 Popup 会为内容传播 `TemplatedParent`，但 NavMenu 的容器位于容器模板之外，因此容器 route 不依赖该传播；
  `popup.root` 依赖 `Popup.Child` 的逻辑父级是 `Popup`、`Popup` 在 `NavMenuItem` 模板内，从而挂回逻辑祖先链。
- header 跳点与 icon / content 节点之间、分组跳点与标题 / 列表之间都是 `/template/` 父子模板关系。

以下变化会破坏上述结论，届时应按本系统机制修正 route，而不是删除 Part 或改用面向用户的宽松 Selector：

- 若某个 Part 的锚点被移入属性值子树（失去 `TemplatedParent` 传播），route 必须继续用 `>>` 定位锚点并声明
  `CrossNestedOwners`；跨独立可视根时再叠加 `CrossVisualRoot`。
- 若弹层内容改为由控件代码动态创建（`TemplatedParent` 链断在新控件模板边界），`/template/` 与 `>>` 均不可达，必须在
  owner 模板内提供静态宿主节点承载 marker，不能新增运行时 VisualTree 搜索。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期类型上下文；
它不参与 `.semantic-*` 的身份匹配。菜单项容器的真实类型 `NavMenuItem` 是 internal，因此 `ContractType` 取其最近 public
基类 `HeaderedSelectingItemsControl`（与 TreeView 对 internal `NodeSwitcherButton` 使用公开 `ToggleButton` 同一决策）。
`popup.root` 的真实节点 `NavMenuPopupFrame` 是 internal，ContractType 使用其公开基类 `Border`。

## 4. Selector 用法

应用级样式先限定 owner `atom|NavMenu`，再通过生成的 Semantic Style 进入 Part：

```xml
<Application.Styles>
    <Style Selector="atom|NavMenu">
        <!-- 一级菜单项与一级分组 -->
        <atom:NavMenuItemStyle x:SetterTargetType="HeaderedSelectingItemsControl">
            <Setter Property="Margin" Value="0,0,0,2" />
        </atom:NavMenuItemStyle>
        <atom:NavMenuItemIconStyle x:SetterTargetType="atom:IconPresenter">
            <Setter Property="Width" Value="16" />
            <Setter Property="Height" Value="16" />
        </atom:NavMenuItemIconStyle>
        <atom:NavMenuItemContentStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Foreground" Value="#1677FF" />
        </atom:NavMenuItemContentStyle>
        <atom:NavMenuItemTitleStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:NavMenuItemTitleStyle>
        <atom:NavMenuListStyle x:SetterTargetType="ItemsPresenter">
            <Setter Property="Margin" Value="0,0,0,4" />
        </atom:NavMenuListStyle>

        <!-- 子菜单内的菜单项与分组 -->
        <atom:NavMenuSubMenuItemStyle x:SetterTargetType="HeaderedSelectingItemsControl">
            <Setter Property="Margin" Value="0,0,0,2" />
        </atom:NavMenuSubMenuItemStyle>
        <atom:NavMenuSubMenuItemIconStyle x:SetterTargetType="atom:IconPresenter">
            <Setter Property="Width" Value="14" />
            <Setter Property="Height" Value="14" />
        </atom:NavMenuSubMenuItemIconStyle>
        <atom:NavMenuSubMenuItemContentStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="FontStyle" Value="Italic" />
        </atom:NavMenuSubMenuItemContentStyle>
        <atom:NavMenuSubMenuItemTitleStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="FontSize" Value="12" />
        </atom:NavMenuSubMenuItemTitleStyle>
        <atom:NavMenuSubMenuListStyle x:SetterTargetType="ItemsPresenter">
            <Setter Property="Margin" Value="0,0,0,4" />
        </atom:NavMenuSubMenuListStyle>

        <!-- 子菜单弹层框体 -->
        <atom:NavMenuPopupRootStyle x:SetterTargetType="Border">
            <Setter Property="CornerRadius" Value="8" />
        </atom:NavMenuPopupRootStyle>
    </Style>
</Application.Styles>
```

对特定 class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|NavMenu.dense[Mode=Inline]">
    <atom:NavMenuItemStyle x:SetterTargetType="HeaderedSelectingItemsControl">
        <Setter Property="MinHeight" Value="32" />
    </atom:NavMenuItemStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `HeaderedSelectingItemsControl.semantic-item` 或 `:is(...)` 变体。
- `IconPresenter.semantic-item-icon`、`ContentPresenter.semantic-item-content`、`Border.semantic-popup-root`。
- 直接复制 `>> .semantic-* /template/ ...` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 依赖 `PART_*`、internal 类型（`NavMenuItem`、`NavMenuPopupFrame`）、Name 或视觉祖先顺序。
- 用一级 Part 命中子菜单内容，或用 `subMenu.*` 命中一级项：层级由互斥 marker 固定，`>>` 本身不做层级隔离。

## 5. 状态与数量语义

数量契约以已实例化的 AtomUI 内置容器为边界。除 `root` 外全部 Part 都是 `RuntimeCreated`，marker 随容器实例存在，
不随数据项迁移、不随状态切换增删。

| 场景 | root | item | subMenu.item | itemContent / itemIcon / itemTitle / list | subMenu.itemContent / itemIcon / itemTitle / list | popup.root |
| --- | --- | --- | --- | --- | --- | --- |
| Inline：N 个一级容器（含一级分组内部的项）、G 个一级分组 | 1 | N | 每个子菜单层级容器 1 | 每个一级项各 1 / `itemTitle`、`list` 每个已实现分组各 1 | `subMenu.*` 内容每个子菜单项各 1；分组键每个子菜单内分组各 1 | 0 |
| Vertical / Horizontal 同结构 | 1 | N | 同上 | 同上 | 同上 | 每个已实现菜单项容器 1 |
| inline collapsed 临时弹层 | 1 | N | 同上 | 同上 | 同上 | 每个已实现菜单项容器 1 |
| 三层嵌套 | 1 | N | 第二、三层每个容器 1 | 同上 | 同上 | 同上 |
| 空集合 | 1 | 0 | 0 | 0 | 0 | 0 |
| 无 `Icon` 的菜单项 | 1 | N | 同上 | `itemIcon` / `subMenu.itemIcon` 节点隐藏但 marker 仍在 | 同左 | 同上 |
| Horizontal 一级分组 | 1 | N | 同上 | `itemTitle` 节点隐藏但 marker 仍在 | 同左 | 同上 |
| 选择 / hover / pressed / keyboard-active / disabled / dark style | 1 | 不变 | 不变 | 不变 | 不变 | 不变 |
| 展开 / 收起 | 1 | 不变 | 随已实现子容器数 | 不变 | 随已实现子分组数 | 随已实现容器数 |
| 容器回收 / re-template / 集合重置 | 1 | 随容器数 | 随容器数 | 随容器数 | 随容器数 | 随模板变体 |

注意：同一节点同时携带一级与子菜单两个层级 marker（例如每个 header 的 icon 节点同时有 `.semantic-item-icon` 与
`.semantic-sub-menu-item-icon`），因此按 class 统计会同时计入两组计数；每个 Part 的实际命中由 route 的容器 anchor
决定，两组计数互不重叠。分隔线不承载任何 marker。

Vertical / Horizontal 及 inline collapsed 临时弹层下，`subMenu.*` 的节点位于菜单项模板的 `Popup.Child` 子树：弹层未
打开时它们不在视觉树上，`subMenu.*` 与 `popup.root` 的可定位目标数为 0；弹层打开后二者同时获得目标。Inline 模式下
`subMenu.*` 位于主视觉树内，`popup.root` 没有弹层节点、实例数恒为 0。按 `CrossVisualRoot` 契约消费这些 Part 的实现
（如 Gallery 语义预览）必须在弹层打开后把 `Popup.Child` 作为附加根交给解析器，不能只扫描主视觉树。

## 6. 尺寸基线

NavMenu 没有 `SizeType` 分档，视觉基线由 `NavMenuToken` 与全局 token 表达：

- 菜单项高度 `ItemHeight`、内容外边距 `ItemContentMargin`、内边距 `ItemContentPadding`、圆角 `ItemBorderRadius`。
- 图标尺寸 `ItemIconSize` 与间距 `IconMargin`；inline collapsed 下切换到 `CollapsedIconSize`。
- 分组标题 `GroupTitleFontSize` / `GroupTitleColor` / `GroupTitleLineHeight`。
- Horizontal 一级高度 `MenuHorizontalHeight`、文字行高 `HorizontalLineHeight`、活动指示条 `ActiveBarHeight` /
  `ActiveBarScaleX`；活动条本身不进入 Part。
- 弹层 `MenuPopupMinWidth` / `MenuPopupMaxWidth` / `MenuPopupMaxHeight` / `MenuPopupContentPadding` 与
  `PopupToken.PopupCornerRadius`。

Semantic Style 覆盖 `item` / `subMenu.item` 的 `Padding` / `MinHeight`、图标键的 `Width` / `Height`、内容键的
`FontSize` / `LineHeight`、标题键的 `FontSize`、`popup.root` 的 `Padding` / `CornerRadius` 时，应验证：header 自然测量
不被固定高度破坏，icon 与文字不重叠，inline 缩进 `InlineItemIndentUnit` 仍连续，Horizontal 活动指示条仍与选中项对齐，
弹层 `MinWidth` 与 `PopupMaxWidth` / `PopupMaxHeight` 约束仍生效。固定 `Height` / `Width` / Min/Max Setter 不作为公共
定制主路径；需要内容驱动增长时优先使用 `MinHeight`。

## 7. 定制边界

以下区域明确不属于 NavMenu Semantic Part：

- 根 Header / Footer 区域：`PART_HeaderPresenter`、`PART_FooterPresenter`。它们是 AtomUI 对上游 Menu 的扩展，上游
  没有对应键。
- Horizontal 底部分割线 `PART_HorizontalLine` 与一级活动指示条 `PART_ActiveIndicator`：上游无对应键。
- inline collapsed 首字符节点 `CollapsedTitlePresenter`。
- Inline 子菜单内容容器 `PART_ChildItemsViewport` / `PART_ChildItemsFrame` / `ChildItemsPresenter`：`subMenu.item`
  描述的是该容器**内部**的菜单项，不是容器自身。
- 分隔线 `NavMenuDividerItem` 及其模板 `PixelAlignedBorder`。
- 分组容器 `NavMenuGroupItem` 自身（其标题与列表通过 `itemTitle` / `list` / `subMenu.itemTitle` / `subMenu.list` 定制）。
- 用户 `HeaderTemplate` / `ItemTemplate` 生成的子树、`Icon` 具体内容与 `CollapsedTooltip` / `ToolTip` 内容。
- 三个 `*NavMenuItemHeader` 实现、`NavMenuPopupFrame`、`NavMenuEntryContainerCoordinator`、`NavMenuSemanticNavigator`
  等内部协作类型与 `PART_*` 名称、Name、模板层级。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；若最终布局仍被 owner 或中间节点的
`Height`、Min/Max、`Padding`、`Margin`、缩进或自绘逻辑约束，应按跨节点布局约束排查，不能解释为 Semantic Style
优先级失效。

## 8. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality、改变层级 marker 的互斥语义，或者让任一
内置模板缺少 marker，均属于公共主题契约变更。

验证至少覆盖：

- `NavMenu` descriptor 的 Part 名称逐字等于上游 12 个键路径，顺序为
  `root`、`item`、`itemContent`、`itemIcon`、`itemTitle`、`list`、`popup.root`、`subMenu.item`、`subMenu.itemContent`、
  `subMenu.itemIcon`、`subMenu.itemTitle`、`subMenu.list`；字段值与 §2 表格一致；`NavMenuItem`、`NavMenuGroupItem`、
  `NavMenuDividerItem` 不持有 descriptor。
- 层级隔离：一级容器只带 `.semantic-item`，子菜单容器只带 `.semantic-sub-menu-item`；`item` 的 `>>` route 不命中子菜单
  容器，`subMenu.item` 的 `>>` route 不命中一级容器；一级分组内部的项仍算一级，子菜单内分组内部的项仍算子菜单项。
- 容器在 owner 之间转移、回收复用、re-template 与集合重置后 marker 不重复、不残留另一层级。
- 三种 mode 与 inline collapsed 下 route 可达；Vertical / Horizontal 与临时弹层内 `subMenu.item` 仍命中；Inline 模式
  `popup.root` 为 0，Popup 打开、关闭、重新打开后 marker 数量不变。
- 选择、hover、pressed、keyboard-active、disabled、dark style、`IsItemBackgroundEnabled` 切换只改变有效视觉属性与
  伪类，不增删 marker。
- owner-scoped Semantic Style（生成的 Style 类型）与 `x:SetterTargetType` 可以编译并命中对应最低 public 类型；
  `popup.root` 的 setter 在 PopupRoot 与 OverlayPopupHost 两条路径都生效。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator；NativeAOT 路径不依赖反射或运行时
  扫描。
- 高密度与虚拟化预算：marker 数量与容器数成正比，不随打开/关闭、hover 或选择变化；容器回收后不保留 class listener。
