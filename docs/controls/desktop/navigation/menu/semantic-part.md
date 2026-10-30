# Menu Semantic Part 契约

本文档定义 `Menu` 桌面版对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。Menu 的整体设计见
[Menu 桌面版架构设计](overview.md)，descriptor 与真实模板节点映射见
[Menu 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)，NavMenu 家族的同类契约见
[NavMenu Semantic Part 契约](../nav-menu/semantic-part.md)。

## 1. 上游基线与 Owner 边界

上游基线为 6.6.3 稳定发布源码公开的 Semantic DOM 契约。`Menu` 的公开键路径共 12 个，与 `NavMenu` 逐字相同，分为三组：

```text
Menu：      root、itemTitle、list、item、itemIcon、itemContent
SubMenu：   subMenu.item、subMenu.itemTitle、subMenu.list、subMenu.itemContent、subMenu.itemIcon
Popup：     popup.root
```

键由真实节点消费，语义分工必须按上游读法理解，不能按名字猜测：

- `itemTitle` / `list` 与 `subMenu.itemTitle` / `subMenu.list` 是 **ItemGroup（分组）的标题与列表**，不是子菜单自身的
  容器。`menu.tsx` 把 `itemTitle` / `list` 作为 ItemGroup 的 `listTitle` / `list` 传入 rc-menu，`SubMenu.tsx` 把
  `subMenu.itemTitle` / `subMenu.list` 以同样方式传入子菜单内的 ItemGroup；落点是 `.ant-menu-item-group-title` 与
  `.ant-menu-item-group-list`。在 AtomUI 中对应 `MenuItemGroup` 模板的 `ContentPresenter#GroupTitlePresenter` 与
  `ItemsPresenter#PART_ItemsPresenter`。
- `item` / `itemIcon` / `itemContent` 描述**菜单栏一级菜单项**，`subMenu.item` / `subMenu.itemIcon` /
  `subMenu.itemContent` 描述**子菜单内的菜单项**。`MenuItem.tsx` 用 `firstLevel` 分支在两组键之间二选一，二者落在同一个
  DOM class 上——区分来自语义键，不来自 DOM class。
- `popup.root` 是子菜单弹层框体。上游模式语义：`itemTitle` / `list` 在 horizontal 模式不生效，`popup` 在 inline 模式
  不生效。`Menu` 是菜单栏控件，一级项恒为 horizontal 语义，因此**顶层 `itemTitle` / `list` 恒为 0 实例**（与上游
  horizontal 一致）；子菜单内分组仍提供 `subMenu.itemTitle` / `subMenu.list`。

本契约的硬性对齐目标是：**Part 名称逐字等于上述 12 个键路径**，不新增、不改名、不合并。

### 1.1 唯一 owner

`Menu` 家族在 plain Menu 语义边界内只有 `Menu` 一个 owner：

| 类型 | 可见性 | 是否持有 descriptor |
| --- | --- | --- |
| `Menu` | public | 是，12 个键路径全部声明在它上面（`root` 隐式）。 |
| `MenuItem` | public | 否。它同时被 ContextMenu / MenuFlyout / DropdownButton 弹层复用，不能成为 plain Menu 的第二个 owner。 |
| `MenuItemGroup` | public | 否，承载 `itemTitle` / `list` 的物理节点并作为路由跳点。 |
| `MenuSeparator` | public | 否，上游在 plain Menu 没有对应 semantic 键。 |
| `MenuPopupScrollHost` | internal | 否，只承载弹层滚动。 |

与 NavMenu 的关键差别：`MenuItem` / `MenuItemGroup` 是 **public** 类型，但它们不是 plain Menu 专属容器，而是被
ContextMenu、MenuFlyout 与 DropdownButton 弹层共同复用的共享容器。若把它们提升为递归 owner，这些复用方会被迫接受
plain Menu 的层级语义。因此 plain Menu 仍采用"单一 owner + 运行时容器 marker"的模型，`ContractType` 直接用 `MenuItem`
（它是 public，无需退化到基类）。

### 1.2 层级区分靠互斥 marker，且只在 plain Menu 子树内生效

上游用"同一个 DOM class + 两个语义键"表达一级与子菜单两级。AtomUI 只有一个 owner，无法用 owner scope 区分层级，
而 `>>` descendant 会同时命中一级项，因此层级必须编码在 **marker class** 上：

| 语义层级 | 菜单项容器 marker | 分组容器 marker |
| --- | --- | --- |
| 一级（菜单栏第一层） | `.semantic-item` | `.semantic-scope-group` |
| 子菜单内（任意深度） | `.semantic-sub-menu-item` | `.semantic-sub-menu-group` |

两个层级互斥：`MenuSemanticLevelScope` 在容器 prepare 时先移除另一层级的类，再幂等补齐当前层级的类。分组的"一级"
按语义层级而非容器层级判定——位于一级分组内部的项与分组仍是一级（分组对层级透明，`MenuItemGroup` 把自身层级下发给
组内子项）。

**层级只在 plain Menu 子树内下发。** `Menu` 在 prepare 直接子项时下发 `TopLevel`，`MenuItem` / `MenuItemGroup` 给自身
子容器下发 `SubMenu` 或继承分组层级；复用方（ContextMenu、MenuFlyout、DropdownButton 弹层）不下发该状态，容器保持
既有的 `.semantic-item` marker 行为不变。这条边界是必需的：DropdownButton 的既有契约要求在嵌套子菜单里仍使用
`.semantic-item`，不能因为 plain Menu 的层级隔离而回归。

`.semantic-scope-group` / `.semantic-sub-menu-group` 是路由跳点，不是 Part，不发布 descriptor。

### 1.3 route 使用 `>>`

容器由 `ItemsControl` 在运行时生成，不在 `Menu` 自身模板内，因此 route 无法用 `/template/` 到达。所有非 root Part 的
route 都以 `>>` 开头，按生成器 route 语法（以 `>>` 开头仅允许 `CrossNestedOwners=true` 的部件）声明
`CrossNestedOwners`。

弹层框体与子菜单内的容器/文本节点位于菜单项模板的 `Popup.Child` 子树，因此 `popup.root` 与全部 `subMenu.*` 同时声明
`CrossVisualRoot=true`；预览（以及任何按 `CrossVisualRoot` 扫描附加根的消费者）必须进入弹层根才能解析到它们。

菜单项图标与文字节点（`itemIcon` / `itemContent` / `subMenu.itemIcon` / `subMenu.itemContent`）与分组标题/列表节点
（`itemTitle` / `list` / `subMenu.itemTitle` / `subMenu.list`）都声明为**同一模板节点的双层级静态 marker**，由 route 的
容器 anchor 决定命中哪一层；具体落点见第 3 节。

所有 Part 的 `Since` 统一为 `6.2.0`。

## 2. Semantic Parts

`Menu` descriptor 的 Part 集合（`root` 隐式，其余 11 个按路径排序）：

| Part | SelectorClass | SelectorRoute | ContractType | Cardinality | CrossVisualRoot | CrossNestedOwners | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `root` | 不适用 | 不适用 | `Menu` | `Single` | `false` | `false` | `false` |
| `item` | `.semantic-item` | `>> .semantic-item` | `MenuItem` | `Multiple` | `false` | `true` | `true` |
| `itemContent` | `.semantic-item-content` | `>> .semantic-item /template/ .semantic-item-content` | `ContentPresenter` | `Multiple` | `false` | `true` | `true` |
| `itemIcon` | `.semantic-item-icon` | `>> .semantic-item /template/ .semantic-item-icon` | `IconPresenter` | `Multiple` | `false` | `true` | `true` |
| `itemTitle` | `.semantic-item-title` | `>> .semantic-scope-group /template/ .semantic-item-title` | `ContentPresenter` | `Multiple` | `false` | `true` | `true` |
| `list` | `.semantic-list` | `>> .semantic-scope-group /template/ .semantic-list` | `ItemsPresenter` | `Multiple` | `false` | `true` | `true` |
| `popup.root` | `.semantic-popup-root` | `>> .semantic-popup-root` | `Border` | `Multiple` | `true` | `true` | `true` |
| `subMenu.item` | `.semantic-sub-menu-item` | `>> .semantic-sub-menu-item` | `MenuItem` | `Multiple` | `true` | `true` | `true` |
| `subMenu.itemContent` | `.semantic-sub-menu-item-content` | `>> .semantic-sub-menu-item /template/ .semantic-sub-menu-item-content` | `ContentPresenter` | `Multiple` | `true` | `true` | `true` |
| `subMenu.itemIcon` | `.semantic-sub-menu-item-icon` | `>> .semantic-sub-menu-item /template/ .semantic-sub-menu-item-icon` | `IconPresenter` | `Multiple` | `true` | `true` | `true` |
| `subMenu.itemTitle` | `.semantic-sub-menu-item-title` | `>> .semantic-sub-menu-group /template/ .semantic-sub-menu-item-title` | `ContentPresenter` | `Multiple` | `true` | `true` | `true` |
| `subMenu.list` | `.semantic-sub-menu-list` | `>> .semantic-sub-menu-group /template/ .semantic-sub-menu-list` | `ItemsPresenter` | `Multiple` | `true` | `true` | `true` |

生成 Style 类型（`AtomUI.Theme.Styling`）：

```text
item                  -> MenuItemStyle
itemIcon              -> MenuItemIconStyle
itemContent           -> MenuItemContentStyle
itemTitle             -> MenuItemTitleStyle
list                  -> MenuListStyle
popup.root            -> MenuPopupRootStyle
subMenu.item          -> MenuSubMenuItemStyle
subMenu.itemIcon      -> MenuSubMenuItemIconStyle
subMenu.itemContent   -> MenuSubMenuItemContentStyle
subMenu.itemTitle     -> MenuSubMenuItemTitleStyle
subMenu.list          -> MenuSubMenuListStyle
```

`root` 是隐式 Part：不声明 `.semantic-root` marker，不生成 Style，通过 owner 属性、owner-scoped Style 或替换
ControlTheme 定制。

### 2.1 Part 说明

- `item` / `subMenu.item`：菜单项容器。一级与子菜单两级共用同一个 public 容器类型，靠互斥 marker 区分。一级项位于
  菜单栏第一层与一级分组内部；子菜单项位于任意深度子菜单与其内部分组。
- `itemIcon` / `subMenu.itemIcon`：菜单项图标区域。一级节点是 `TopLevelMenuItemTheme` 的
  `IconPresenter#ItemIconPresenter`；子菜单节点是 `MenuItemTheme` 的 `IconPresenter#ItemIconPresenter`。两级模板都常驻该
  节点，`Icon` 为 null 时只是隐藏，marker 不增删。
- `itemContent` / `subMenu.itemContent`：菜单项文字内容区域。一级节点是 `TopLevelMenuItemTheme` 的
  `ContentPresenter#HeaderPresenter`；子菜单节点是 `MenuItemTheme` 的 `ContentPresenter#ItemTextPresenter`。
- `itemTitle` / `subMenu.itemTitle`：分组标题区域，节点是 `MenuItemGroupTheme` 的
  `ContentPresenter#GroupTitlePresenter`。菜单栏一级项不渲染一级分组标题（上游 horizontal 语义），因此 `itemTitle` 在
  顶层解析为 0；子菜单内分组提供 `subMenu.itemTitle`。
- `list` / `subMenu.list`：分组列表区域，节点是 `MenuItemGroupTheme` 的 `ItemsPresenter#PART_ItemsPresenter`。与
  `itemTitle` 同理，`list` 在顶层解析为 0。
- `popup.root`：子菜单弹层框体。一级子菜单的弹层框体在 `TopLevelMenuItemTheme` 内，嵌套子菜单的弹层框体在
  `MenuItemTheme` 内；两个模板的 `Border#PopupFrame` 都带该 marker，因此菜单栏子菜单与嵌套子菜单都覆盖。

## 3. marker 放置与路由

`root` 是隐式 Part，不声明 `.semantic-root` marker。非 root Part 的 marker 放置：

| marker | 放置方式 | 位置 |
| --- | --- | --- |
| `.semantic-item` / `.semantic-sub-menu-item` | 运行时按层级幂等注入 | `Menu` / `MenuItem` / `MenuItemGroup` 的 `PrepareContainerForItemOverride`，统一经 `MenuSemanticLevelScope.ApplyItemLevel`。 |
| `.semantic-scope-group` / `.semantic-sub-menu-group` | 运行时按层级幂等注入 | 同一路径的 `MenuSemanticLevelScope.ApplyGroupLevel`（分组路由跳点，不是 Part）。 |
| `.semantic-item-icon` / `.semantic-sub-menu-item-icon` | 静态模板节点 | `MenuItemTheme.axaml` 的 `IconPresenter#ItemIconPresenter` 与 `TopLevelMenuItemTheme.axaml` 的 `IconPresenter#ItemIconPresenter`；同一节点同时携带两个层级 marker。 |
| `.semantic-item-content` / `.semantic-sub-menu-item-content` | 静态模板节点 | `MenuItemTheme.axaml` 的 `ContentPresenter#ItemTextPresenter` 与 `TopLevelMenuItemTheme.axaml` 的 `ContentPresenter#HeaderPresenter`；同上。 |
| `.semantic-item-title` / `.semantic-sub-menu-item-title` | 静态模板节点 | `MenuItemGroupTheme.axaml` 的 `ContentPresenter#GroupTitlePresenter`；同上。 |
| `.semantic-list` / `.semantic-sub-menu-list` | 静态模板节点 | `MenuItemGroupTheme.axaml` 的 `ItemsPresenter#PART_ItemsPresenter`；同上。 |
| `.semantic-popup-root` | 静态模板节点 | `MenuItemTheme.axaml` 与 `TopLevelMenuItemTheme.axaml` 的 `Border#PopupFrame`。 |

### 3.1 为什么 route 直接是 `>> .semantic-item /template/ .semantic-item-icon`

与 NavMenu 不同，plain Menu 的菜单项容器模板就是 `MenuItem` 自己的 `MenuItemTheme` / `TopLevelMenuItemTheme`，不存在一层
额外的 header 子模板，因此 route 只有一段 `/template/` 步进，不需要 NavMenu 的 `.semantic-scope-header` 跳点。

三个组合符的语义由 Avalonia 选择器实现确定，不能互换：

| 组合符 | Avalonia 实现 | 匹配条件 |
| --- | --- | --- |
| `/template/` | `TemplateSelector` | 当前节点的直接 `TemplatedParent` 必须匹配前一段。 |
| `>` | `ChildSelector.Evaluate` → `ILogical.LogicalParent` | 直接逻辑父级必须匹配前一段。 |
| `>>` | `DescendantSelector.Evaluate` → 逐级 `LogicalParent` | 沿完整逻辑祖先链匹配任意一级。 |

必须遵守的边界：

1. **容器只能靠 `>>` 到达。** 容器是运行时生成物，不在 `Menu` 模板里；`>` 的直接父级在一级分组内部会变成
   `MenuItemGroup`，无法统一覆盖，也无法区分 `item` 与 `subMenu.item`。
2. **以 `>>` 开头只允许 `CrossNestedOwners=true`。** 否则报 `ATOMUIGEN027`。
3. **`>>` 不会在最近语义 owner 处停止。** 因此必须用互斥层级 marker 约束 `>> .semantic-item` 与
   `>> .semantic-sub-menu-item` 的命中范围，不能把 `>>` 当作面向用户的宽松 Selector。

### 3.2 可达性依据（源码事实）

- 容器由 `ItemsControl` 生成，`PanelContainerGenerator.InsertContainer` 会执行 `itemsControl.AddLogicalChild(container)`，
  因此容器的逻辑祖先链回到 `Menu`，`DescendantSelector` 可达；这条链与容器渲染在普通视觉树还是 `PART_Popup` 内无关。
- `Menu` 一级项使用 `TopLevelMenuItemTheme`（`ItemContainerTheme`），子菜单项与分组子项使用默认 `MenuItemTheme`；两级
  模板都在自身模板内提供 icon / content marker，因此 `/template/` 步进分别命中各自节点。
- 弹层框体依赖 `Popup` 在菜单项模板内、`Popup.Child` 的逻辑父级是 `Popup`，从而挂回逻辑祖先链。

以下变化会破坏上述结论，届时应按系统机制修正 route，而不是删除 Part 或改用面向用户的宽松 Selector：

- 若某个 Part 的锚点被移入属性值子树，route 必须继续用 `>>` 定位锚点并声明 `CrossNestedOwners`；跨独立可视根时再叠加
  `CrossVisualRoot`。
- 若新增菜单项模板，必须在新模板内提供同样的静态 marker，否则该模板下的 Part 不可达。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期类型上下文；
它不参与 `.semantic-*` 的身份匹配。plain Menu 家族的类型都是 public，因此 `item` / `subMenu.item` 的 `ContractType` 直接
是 `MenuItem`。

## 4. Selector 用法

应用级样式先限定 owner `atom|Menu`，再通过生成的 Semantic Style 进入 Part：

```xml
<Application.Styles>
    <Style Selector="atom|Menu">
        <!-- 菜单栏一级菜单项 -->
        <atom:MenuItemStyle x:SetterTargetType="MenuItem">
            <Setter Property="Margin" Value="0,0,0,2" />
        </atom:MenuItemStyle>
        <atom:MenuItemIconStyle x:SetterTargetType="atom:IconPresenter">
            <Setter Property="Width" Value="16" />
            <Setter Property="Height" Value="16" />
        </atom:MenuItemIconStyle>
        <atom:MenuItemContentStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Foreground" Value="#1677FF" />
        </atom:MenuItemContentStyle>

        <!-- 子菜单内的菜单项与分组 -->
        <atom:MenuSubMenuItemStyle x:SetterTargetType="MenuItem">
            <Setter Property="MinHeight" Value="32" />
        </atom:MenuSubMenuItemStyle>
        <atom:MenuSubMenuItemIconStyle x:SetterTargetType="atom:IconPresenter">
            <Setter Property="Width" Value="14" />
            <Setter Property="Height" Value="14" />
        </atom:MenuSubMenuItemIconStyle>
        <atom:MenuSubMenuItemContentStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="FontStyle" Value="Italic" />
        </atom:MenuSubMenuItemContentStyle>
        <atom:MenuSubMenuItemTitleStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="FontSize" Value="12" />
        </atom:MenuSubMenuItemTitleStyle>
        <atom:MenuSubMenuListStyle x:SetterTargetType="ItemsPresenter">
            <Setter Property="Margin" Value="0,0,0,4" />
        </atom:MenuSubMenuListStyle>

        <!-- 子菜单弹层框体 -->
        <atom:MenuPopupRootStyle x:SetterTargetType="Border">
            <Setter Property="CornerRadius" Value="8" />
        </atom:MenuPopupRootStyle>
    </Style>
</Application.Styles>
```

对特定 class、属性或伪类定制时，把条件放在 owner 一侧：

```xml
<Style Selector="atom|Menu.dense">
    <atom:MenuItemStyle x:SetterTargetType="MenuItem">
        <Setter Property="MinHeight" Value="28" />
    </atom:MenuItemStyle>
</Style>
```

`itemTitle` / `list` 与 `subMenu.itemTitle` / `subMenu.list` 的用法同理，但如前所述：一级 `itemTitle` / `list` 在 plain
Menu（horizontal）下恒为 0 实例，只有 `subMenu.*` 的两个键在子菜单内分组上生效。菜单栏顶层不渲染分组标题，因此
`MenuItemStyle` / `MenuItemIconStyle` / `MenuItemContentStyle` 对应的 `itemTitle` / `list` 一级键在 plain Menu 下不产生
目标；需要定制分组标题与分组列表时使用 `MenuSubMenuItemTitleStyle` 与 `MenuSubMenuListStyle`。

禁止的定制手段：

- 不得依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序。
- 不得用更宽的 descendant 或 owner scope 替代层级 marker。
- 不得用 `>> .semantic-item /template/ .semantic-item-icon` 之外的宽松 Selector 兼容缺失 marker。

## 5. 兼容性与验证

- 默认主题不消费 `.semantic-*`：marker 只在应用显式声明用户 Semantic Style 时参与匹配。
- Part 不引入运行时 descriptor 查询、以 Visual 为 key 的缓存或永久监听器；marker 用静态 `Classes.semantic-*="True"` 或
  `Classes.Add(<生成常量>)`，不建立 Binding 或 selector activator。
- 复用方（ContextMenu / MenuFlyout / DropdownButton 弹层）的 marker 行为不因 plain Menu 层级隔离而改变。

验证矩阵见 [Menu 桌面版实现原理](implementation.md) 的 Semantic Part 条目，以及
`tests/AtomUI.Desktop.Controls.Tests/Menu` 下的 `MenuSemanticPartTests`（descriptor 契约）与 `MenuSemanticLevelTests`
（真实视觉树层级、复用方边界、声明式钉住时序、模板 marker 存在性）。
