# SplitButton Semantic Part 契约

本文档定义 SplitButton 控件公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[SplitButton 桌面版架构设计](overview.md)，真实模板、marker 映射与状态流见
[SplitButton 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

SplitButton 是唯一 Semantic owner，公开 7 个 Semantic Part。弹层侧 5 个部件语义对齐上游 Dropdown 的
Semantic DOM（`root` / `itemTitle` / `item` / `itemContent` / `itemIcon`）：上游 Dropdown 语义部件全部位于弹层侧，
`root` 是弹层根、`itemTitle` 是菜单分组标题、`item` / `itemIcon` / `itemContent` 是菜单项及菜单项内部槽位。AtomUI
保留 `root` 作为 owner 自身的隐式 Part（由生成器统一注册），因此上游的弹层根 `root` 映射为 `popup.root`；
`itemTitle` 对应上游的分组标题（`ant-menu-item-group-title`），由 `MenuItemGroup` 的标题 ContentPresenter 承载。

触发侧 2 个部件（`primary` / `secondary`）是 AtomUI 的显式能力补充，上游没有对应部件：上游 deprecated
`Dropdown.Button` 的两个触发 Button 由调用方自持（可自行加 class / 改样式），而 AtomUI SplitButton 的两个触发
Button 是模板内部件，调用方只有 `Content` / `Icon` / `OpenIndicator` 内容级注入口，不发布则完全不可定制。这与
DropdownButton 不同——DropdownButton 继承 Button，天然继承触发侧 `icon` / `content` 部件；SplitButton 是
ContentControl，无此继承路径。声明位于 `SplitButton.SemanticParts.cs` partial 文件；`root` 为隐式 Part，不在该
文件中显式声明。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `SplitButton` |
| Part | `root` |
| Selector | SplitButton 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `SplitButton` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | SplitButton owner |
| 职责 | SplitButton root 是动作内容、弹层数据、命令与状态的组织边界。 |
| 相关 API | 全部 SplitButton public API |
| 相关 Token | SplitButtonToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `primary`

| 字段 | 值 |
| --- | --- |
| Owner | `SplitButton` |
| Part | `primary` |
| Selector | `.semantic-primary` |
| SelectorRoute | `/template/ .semantic-primary` |
| Style Type | `SplitButtonPrimaryStyle` |
| ContractType | `Button` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | SplitButton 模板 `PART_PrimaryButton`（主命令按钮，静态 marker） |
| 职责 | 触发侧主命令按钮区域，承载主动作内容、图标与状态视觉（AtomUI 补充部件，上游无对应）。 |
| 相关 API | `Content`、`Icon`、`Command`、`IsPrimaryButtonType`、`IsDanger`、`SizeType` |
| 相关 Token | ButtonToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `secondary`

| 字段 | 值 |
| --- | --- |
| Owner | `SplitButton` |
| Part | `secondary` |
| Selector | `.semantic-secondary` |
| SelectorRoute | `/template/ .semantic-secondary` |
| Style Type | `SplitButtonSecondaryStyle` |
| ContractType | `Button` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | SplitButton 模板 `PART_SecondaryButton`（次级下拉触发按钮，静态 marker） |
| 职责 | 触发侧次级下拉触发区域，承载 `OpenIndicator` 与弹层触发状态视觉（AtomUI 补充部件，上游无对应）。 |
| 相关 API | `OpenIndicator`、`Flyout`、`TriggerType`、`Placement` |
| 相关 Token | ButtonToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `SplitButton` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `>> .semantic-popup-root` |
| Style Type | `SplitButtonPopupRootStyle` |
| ContractType | `ArrowDecoratedBox` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `MenuFlyoutPresenter.OnApplyTemplate` 定位到的弹层根视觉面 `ArrowDecoratedBox`（模板应用时注入 marker，其逻辑祖先链经 Popup `PlacementTarget` 回到 SplitButton） |
| 职责 | 下拉菜单弹层的根视觉面，承载菜单项集合与弹层根视觉（边框 / 背景 / 圆角由 `ArrowDecoratedBox` 渲染，对应上游的 `root`）。 |
| 相关 API | `Flyout`、`MenuItem.Items`、`MenuItem.Header`、`MenuItem.Icon` |
| 相关 Token | MenuToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `itemTitle`

| 字段 | 值 |
| --- | --- |
| Owner | `SplitButton` |
| Part | `itemTitle` |
| Selector | `.semantic-item-title` |
| SelectorRoute | `>> .semantic-item-title-group /template/ .semantic-item-title` |
| Style Type | `SplitButtonItemTitleStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `MenuItemGroup` 模板 `GroupTitlePresenter`（`ContentPresenter`，`MenuItemGroup.OnApplyTemplate` 时注入 marker；分组容器本身带 `semantic-item-title-group` 中间标记类） |
| 职责 | 菜单分组标题节点（对应上游的 `itemTitle`，即 `ant-menu-item-group-title`）。 |
| 相关 API | `MenuItemGroup.Header`、`MenuItemGroup.HeaderTemplate` |
| 相关 Token | MenuToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `SplitButton` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `>> .semantic-item` |
| Style Type | `SplitButtonItemStyle` |
| ContractType | `MenuItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `MenuFlyoutPresenter.CreateContainerForItemOverride` / `PrepareContainerForItemOverride` 与 `MenuItem.CreateContainerForItemOverride` / `PrepareContainerForItemOverride` 容器路径生成的 `MenuItem`（顶层与任意嵌套层级的子菜单项，回收复用时 marker 保持不变） |
| 职责 | 弹层中的单个菜单项容器，承载该项的状态、内容、图标与子菜单（对应上游的 `item`）。 |
| 相关 API | `MenuItem.Header`、`MenuItem.Icon`、`MenuItem.Items` |
| 相关 Token | MenuToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `itemIcon`

| 字段 | 值 |
| --- | --- |
| Owner | `SplitButton` |
| Part | `itemIcon` |
| Selector | `.semantic-item-icon` |
| SelectorRoute | `>> .semantic-item /template/ .semantic-item-icon` |
| Style Type | `SplitButtonItemIconStyle` |
| ContractType | `IconPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `MenuItem` 模板 `ItemIconPresenter`（`IconPresenter`，`MenuItem.OnApplyTemplate` 时注入 marker） |
| 职责 | 菜单项模板内的图标节点（对应上游的 `itemIcon`）。 |
| 相关 API | `MenuItem.Icon` |
| 相关 Token | MenuToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `itemContent`

| 字段 | 值 |
| --- | --- |
| Owner | `SplitButton` |
| Part | `itemContent` |
| Selector | `.semantic-item-content` |
| SelectorRoute | `>> .semantic-item /template/ .semantic-item-content` |
| Style Type | `SplitButtonItemContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `MenuItem` 模板 `ItemTextPresenter`（`ContentPresenter`，`MenuItem.OnApplyTemplate` 时注入 marker） |
| 职责 | 菜单项模板内的文本内容节点（对应上游的 `itemContent`）。 |
| 相关 API | `MenuItem.Header`、`MenuItem.HeaderTemplate` |
| 相关 Token | MenuToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

## 2. 职责与存在条件

- `primary` / `secondary` 为静态 marker（`CrossVisualRoot=false`、`RuntimeCreated=false`）：marker 写在
  `SplitButtonTheme.axaml` 模板的 `PART_PrimaryButton` / `PART_SecondaryButton` 上，随模板应用与重套用保持。SplitButton
  只有一个内置模板变体，两部件在模板应用后始终物化。触发侧按钮的内部结构（图标 presenter、内容 presenter、
  波纹装饰）属于 Button 自身的语义契约（`icon` / `content`），SplitButton 不穿透重复发布。
- `popup.root` / `itemTitle` / `item` / `itemIcon` / `itemContent` 为 `CrossVisualRoot=true` + `RuntimeCreated=true`：
  弹层由 `MenuFlyout.CreatePresenter` 在打开时运行时创建，生成器豁免宿主模板 marker 校验，由控件行为测试兜底。`>>`
  首步进沿 Popup `PlacementTarget`（模板内 `PART_SecondaryButton`）建立的逻辑祖先链从 owner 定位弹层根，沿用
  FlyoutHost → FlyoutPresenter 跨视觉根先例，owner-scoped Style 因此可达。
- **子菜单跨视觉根**：嵌套子菜单在父 `MenuItem` 自有的 Popup（独立视觉根）中物化。`item` 的 marker 同时在
  `MenuFlyoutPresenter` 与 `MenuItem` 的 `CreateContainerForItemOverride` / `PrepareContainerForItemOverride`
  两条容器路径注入，任意嵌套层级的子菜单项都携带 `.semantic-item`；这些节点的逻辑祖先链经父 `MenuItem` 回到
  owner，`>> .semantic-item` 路由仍然可达。`popup.root` 只标注顶层弹层的 `ArrowDecoratedBox`（`Single`），
  子菜单层级不重复标注弹层根。
- `itemIcon` / `itemContent` 的路由经中间跳步 `.semantic-item` 承转：该跳步是同控件已声明的 RuntimeCreated
  部件 `item`（ContractType=`MenuItem`）。这两个部件同样标记 `RuntimeCreated=true`，与其余 `popup.*` 一致，
  生成器豁免宿主模板 marker 校验；`CrossNestedOwners=true` 让 owner-scoped Style 沿逻辑祖先链越过嵌套 owner
  边界，运行时按 descendant + `/template/` 命中菜单项实例的 `ItemIconPresenter` / `ItemTextPresenter`。
- `itemTitle` 的路由经中间跳步 `.semantic-item-title-group` 承转：分组标题由 `MenuItemGroup` 承载，
  `MenuItemGroup` 容器本身带 `semantic-item-title-group` 中间标记类，标题 `GroupTitlePresenter` 在
  `MenuItemGroup.OnApplyTemplate` 注入 `.semantic-item-title`。`itemTitle` 声明 `CrossNestedOwners=true`，
  使 owner-scoped Style 沿逻辑祖先链越过嵌套 owner（分组可位于子菜单层级）命中分组标题节点。
- **存在条件**：`popup.*` 与 `item*` 仅在 `Flyout` 为 `MenuFlyout`（或其派生）且弹层打开（或 Gallery 语义预览经
  `IsPopupPinnedOpen` 钉住常开）时物化——SplitButton 的 `Flyout` 属性类型是通用 `Flyout`，挂普通 `Flyout` /
  非菜单弹层时不产生弹层侧语义部件；`primary` / `secondary` 随模板应用始终物化，与 `Flyout` 类型无关。
  `itemIcon` 仅在 `MenuItem.Icon` 非空时可见，`item` / `itemContent` 随菜单项集合创建、销毁与回收，
  `itemTitle` 仅随分组菜单项物化。

## 3. 数量语义

`primary` / `secondary` / `popup.root` / `itemIcon` / `itemContent` 为 `Single`：SplitButton 恰好一个主按钮与一个
次级触发按钮，每个弹层恰好一个弹层根视觉面，每个 `MenuItem` 模板恰好一个图标节点 / 内容节点。状态变化（弹层
开合、菜单项集合变化、图标有无、Flyout 类型切换）只切换物化范围或可见性，不改变 marker 身份。

`item`、`itemTitle` 为 `Multiple`：`item` 随菜单项集合在弹层中创建与销毁，顶层与任意嵌套子菜单层级统一注入
marker，容器回收复用时 marker 保持不变；`itemTitle` 随分组菜单项物化，每个 `MenuItemGroup` 恰好一个标题节点。

## 4. Selector 用法

生成的 Semantic Style 类型命名为 `SplitButton<PartPathPascalCase>Style`，如 `SplitButtonPrimaryStyle`、
`SplitButtonSecondaryStyle`、`SplitButtonPopupRootStyle`、`SplitButtonItemTitleStyle`、`SplitButtonItemStyle`、
`SplitButtonItemContentStyle`、`SplitButtonItemIconStyle`（命名空间 `AtomUI.Theme.Styling`，AXAML 命名空间
`https://atomui.net`）。`root` 不生成 Style 类型，owner 级 Setter 写在外层普通 Style 上。

推荐写法（owner 嵌套 Style + 语义 class；AtomUI 类型在 `x:SetterTargetType` 中带 `atom:` 前缀，Avalonia 类型
不带前缀）：

```xml
<StackPanel Spacing="24">
    <atom:SplitButton Classes="semantic-object-style-demo"
                      IsPrimaryButtonType="True"
                      Content="Split Action">
        <atom:SplitButton.Flyout>
            <atom:MenuFlyout>
                <atom:MenuItem Header="Profile" />
                <atom:MenuItem Header="Settings"
                               Icon="{antdicons:AntDesignIconProvider Kind=SettingOutlined}" />
                <atom:MenuSeparator />
                <atom:MenuItem Header="Logout"
                               Foreground="{atom:SharedTokenResource ColorError}"
                               Icon="{antdicons:AntDesignIconProvider Kind=LogoutOutlined}" />
            </atom:MenuFlyout>
        </atom:SplitButton.Flyout>
        <atom:SplitButton.Styles>
            <Style Selector="atom|SplitButton.semantic-object-style-demo">
                <atom:SplitButtonPrimaryStyle x:SetterTargetType="atom:Button">
                    <Setter Property="FontWeight" Value="Bold" />
                </atom:SplitButtonPrimaryStyle>
                <atom:SplitButtonSecondaryStyle x:SetterTargetType="atom:Button">
                    <Setter Property="Opacity" Value="0.9" />
                </atom:SplitButtonSecondaryStyle>
                <atom:SplitButtonPopupRootStyle x:SetterTargetType="atom:ArrowDecoratedBox">
                    <Setter Property="BorderBrush" Value="#d9d9d9" />
                    <Setter Property="CornerRadius" Value="4" />
                </atom:SplitButtonPopupRootStyle>
                <atom:SplitButtonItemStyle x:SetterTargetType="atom:MenuItem">
                    <Setter Property="Foreground" Value="#1677ff" />
                </atom:SplitButtonItemStyle>
            </Style>
        </atom:SplitButton.Styles>
    </atom:SplitButton>
</StackPanel>
```

`x:SetterTargetType` 必须写 `ContractType` 对应的类型：`primary` / `secondary` / `item` 为
`Button` / `Button` / `MenuItem`（AtomUI 类型，带 `atom:` 前缀）；`itemContent` / `itemTitle` 为
`ContentPresenter`（Avalonia 类型，不带前缀）；`popup.root` 为 `ArrowDecoratedBox`、`itemIcon` 为
`IconPresenter`（AtomUI 类型，带 `atom:` 前缀）。`popup.root` 目标是弹层根视觉面 `ArrowDecoratedBox`，边框 /
圆角直接设置其 `BorderBrush` / `BorderThickness` / `CornerRadius`；`itemIcon` 的目标是 `IconPresenter`，着色需
设置其 `IconBrush`（`IconPresenter` 会把 `IconBrush` relay 到内部图标的 `StrokeBrush` / `FillBrush`），而非
`TextElement.Foreground`。

生成 Style 由 `Nesting()` 展开 `>>` 与 `/template/` 步进跨模板边界与视觉根命中目标。不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约（`root` 没有
  `.semantic-root` class，owner 级 Setter 写在外层普通 Style 上）。
- 把 `Control.semantic-item`、`:is(MenuItem).semantic-item` 等 `ContractType` 写入 Part 身份 selector；Part
  身份只写 `.semantic-*`。
- 手动复制 `>> .semantic-item /template/ .semantic-item-icon` 等完整 route；route 是生成 Style 的
  owner-relative 内部路由，应用侧始终使用生成的 `SplitButton*Style`。
- 穿过 `ItemTemplate`、`HeaderTemplate` 等用户模板继续匹配内部 Visual。
- 通过 `primary` / `secondary` 部件穿透触发按钮内部结构定制 `icon` / `content`——那是 Button 自身的语义
  契约，应使用 Button 的生成 Style 或 `Icon` / `Content` API。

## 5. 定制边界

以下区域不属于 SplitButton Semantic Part：

- **触发按钮的内部实现节点**：`primary` / `secondary` 只承诺两个 `Button` 容器本体；按钮内部的图标
  presenter、内容 presenter、波纹装饰属于 Button 自身的语义契约（`icon` / `content`），SplitButton 不重复发布。
- **菜单项的内部实现节点**：`item` 只承诺 `MenuItem` 容器本体；`itemIcon` / `itemContent` 只承诺 `MenuItem`
  模板内的 `ItemIconPresenter` / `ItemTextPresenter` 节点。菜单项选中指示、快捷手势、子菜单箭头、分隔符等更深
  的内部结构由 Menu 家族拥有，不经 SplitButton 重复发布。
- **弹层 Popup 宿主与定位**：弹层的定位、钉住打开、动画与滚动由共享 Popup / Flyout 契约承担，`popup.root` 只
  覆盖弹层根视觉面 `ArrowDecoratedBox`（边框 / 背景 / 圆角）。
- **接缝分隔线**：primary 形态两个按钮间的分隔线是 SplitButton 主题对 antd `Space.Compact` solid 组合规则的
  内置还原（颜色按 `IsDanger` 取 `colorPrimaryHover` / `colorErrorHover`，次按钮 hover 时隐藏），由控件
  Render 绘制，不作为定制入口发布。
- `PART_*` 名称、internal 类型与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker（`primary` / `secondary`）只提供应用样式命中点，不改变默认
属性优先级或增加状态订阅。Semantic Style 服从 Avalonia 原生属性优先级。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`（含把公共基类承诺收窄为具体实现类型）、改变
cardinality，或让任一内置模板变体缺少 marker，均属于公共主题契约变更。

与上游 antd Dropdown `DropdownSemanticType` 的对照差异（有意保持）：

- 上游弹层根 `root` 映射为 AtomUI `popup.root`：AtomUI 的 `root` 是 owner 自身隐式 Part，语义与上游 Dropdown 的
  `root`（弹层根）不同，故以 `popup.root` 命名消歧，对应关系在 §1 明确标注。
- 上游弹层分组声明 `itemTitle` 分组标题槽；AtomUI 用 `MenuItemGroup` 承载分组标题（标题即 `itemTitle` 的
  `GroupTitlePresenter`，对应 antd 的 `ant-menu-item-group-title`），故保留 `itemTitle`。
- 上游没有触发侧语义部件；AtomUI 额外发布 `primary` / `secondary`，作为对上游组合结构差异的显式能力补充：
  上游消费者自持触发按钮，AtomUI 消费者的触发按钮是模板内部件（理由见 §1）。

验证至少覆盖：

- owner descriptor 只包含 §1 的 7 个 Part，字段值与本文一致（`item` / `itemTitle`=`Multiple`、
  `popup.*` + `item*`=`CrossVisualRoot` + `RuntimeCreated`，`itemIcon` / `itemContent` / `itemTitle` 另含
  `CrossNestedOwners`，`primary` / `secondary` 为静态 marker），见
  `tests/AtomUI.Desktop.Controls.Tests/Buttons/SplitButtonSemanticPartTests.cs`。
- `SplitButtonTheme.axaml` 的内置模板为 `PART_PrimaryButton` / `PART_SecondaryButton` 携带静态
  `semantic-primary` / `semantic-secondary` marker，模板重套用后保持。
- `popup.root` marker 在 `MenuFlyoutPresenter.OnApplyTemplate` 注入到弹层根视觉面 `ArrowDecoratedBox`；`item`
  marker 在 `MenuFlyoutPresenter` 与 `MenuItem` 容器路径注入，覆盖顶层与嵌套子菜单；`itemIcon` / `itemContent`
  marker 在 `MenuItem.OnApplyTemplate` 注入，`itemTitle` marker 在 `MenuItemGroup.OnApplyTemplate` 注入，模板
  重套用后保持。
- 生成的 `SplitButton*Style` 可编译并命中触发侧与弹层目标节点（含钉住常开时 `popup.*` 的解析）。
- 弹层开合、子菜单物化与容器回收不改变 marker 身份与数量语义；默认主题不消费 `.semantic-*`，未声明用户
  Semantic Style 时不增加 selector activator。
- Gallery Semantic Parts Tab 经 `IsPopupPinnedOpen` 钉住弹层常开，7 个 Part 均可解析高亮；弹层根由 Gallery
  页面在 `Loaded` / `Opened` 时显式注册进 `SemanticPartPreview.AdditionalRoots`（`SemanticPartHighlightSession`
  只自动发现 owner 模板内 Popup，Flyout 代码创建的弹层需显式注册，同 DropdownButton 模式）。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
