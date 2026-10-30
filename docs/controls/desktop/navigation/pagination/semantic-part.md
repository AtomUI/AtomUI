# Pagination Semantic Part 契约

本文档定义 `Pagination` 与 `SimplePagination` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。整体设计见
[Pagination 桌面版架构设计](overview.md)，真实模板与生命周期见 [Pagination 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

Pagination 的公开语义结构与 Ant Design 6 的 Semantic DOM 对齐：`PaginationSemanticType = { root, item }`（since 6.2.0）。
`root` 是隐式 owner，`item` 覆盖上一页/下一页按钮与页码项。上游 `rc-pagination` 把 `styles.item` 应用到页码项、上一页/下一页
与 simple pager，但不应用到 jump-prev / jump-next；AtomUI 将这一排除映射到 `JumpPrevious` / `JumpNext` 快速跳页项——这些项动态移除
`semantic-item` marker，不属于 `item` Part。

`SimplePagination` 是 AtomUI 的简洁分页变体，公开同一组 `root` + `item`。其 `item` 只覆盖上一页/下一页两个导航项，
不覆盖快速跳转输入框（`PART_QuickJumper`）与信息指示文本（`PART_InfoIndicator`）。此外 `SimplePagination` 还公开
AtomUI 扩展的 `info` Part，覆盖 `PART_InfoIndicator`（"当前页 / 总页数" 文本），用于格式化分页信息文字；上游
Ant Design 没有对应的 Semantic DOM（simple pager 的文本位于 item 内部），因此 `info` 是 AtomUI 扩展，不参与
`PaginationSemanticType` 对齐。

`AbstractPagination`、`PaginationNav`、`PaginationNavItem` 均不持有独立 Semantic descriptor；它们是基类或 internal
协作类型，不能作为公共 descriptor owner。

## 1. Semantic Parts

| Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | owner | `Pagination` / `SimplePagination` | `Single` | `Root` | `false` | `false` |
| `item` | `.semantic-item` | `ContentControl` | `Multiple` | `Selector` | `false` | `Pagination`: `true`，`SimplePagination`: `false` |
| `info`（仅 `SimplePagination`） | `.semantic-info` | `TextBlock` | `Single` | `Selector` | `false` | `false` |

`root` 是控件自身，承载 `CurrentPage`、`PageSize`、`Total`、`SizeType`、`Align`、`IsShowTotalInfo`、
`IsShowSizeChanger`、`IsShowQuickJumper` 等 public API、主题入口和状态归一，不声明 `.semantic-root` marker。

`item` 的 marker 挂在 `PaginationNavItem`（internal 的 `ContentControl` 派生类型）实例上。`Pagination` 的
`item` 是运行时创建的语义标记，由 `PaginationNavItem` 按 `PaginationItemType` 维护；`SimplePagination` 的
`item` 是内置模板中的静态标记，声明在 `SimplePaginationTheme.axaml` 的上一页/下一页节点上。

## 2. Part 说明

### 2.1 `Pagination`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Pagination` |
| Part | `root` |
| Selector | 不适用（owner 自身） |
| SelectorRoute | 不适用 |
| ContractType | `Pagination` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `Pagination` owner |
| 职责 | 分页控件根语义区域，承载分页状态、布局入口和主题视觉。 |
| 相关 API | `CurrentPage`、`PageSize`、`Total`、`SizeType`、`Align`、`IsShowTotalInfo`、`IsShowSizeChanger`、`IsShowQuickJumper` |
| 相关 Token | Pagination Token + SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是 `Pagination` owner 本身，在控件实例的整个生命周期内始终存在，每个实例恰好一个。它负责：

- 承载分页受控状态 `CurrentPage` / `PageSize` 与 `Total`、`Align`、`SizeType` 等布局与视觉入口。
- 承载 `:disabled` 等交互状态。
- 作为 `item` owner-scoped Selector 的作用域边界。

`PaginationTheme.axaml` 的根模板在布局 `StackPanel#PART_RootLayout` 外包裹 `atom:DashedBorder`，并 TemplateBind
`Background`、`BackgroundSizing`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding` 以及
`BorderDashArray` / `BorderDashOffset`（映射到 `DashedBorder` 的 `StrokeDashArray` / `StrokeDaskOffset`）。
root 视觉定制直接作用于 owner 自身的这些属性，对应 Ant Design `styles.root` 的边框（含虚线）、背景与内边距
定制。默认值全部为空/零，不改变既有默认外观。

root 不表示模板中的 `StackPanel#PART_RootLayout`、`PART_Nav` 或各 presenter 节点；这些内部节点不属于 root 契约。

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `Pagination` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `/template/ .semantic-scope-nav > .semantic-item` |
| ContractType | `ContentControl` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 上一页/下一页导航项与页码指示项（`PaginationNavItem`） |
| 职责 | 承载单个分页导航单元的尺寸、状态与点击语义。 |
| 相关 API | `CurrentPage`、`PageSize`、`PaginationItemType` |
| 相关 Token | `ItemSize`、`ItemSizeSM`、`ItemBg`、`ItemActiveBg` 等 Pagination Token |
| 稳定性 | stable since 6.2.0 |

`item` 覆盖分页导航区域内的所有可交互导航单元：

- 上一页导航项与下一页导航项。
- 区间内的每个页码指示项。

它明确不覆盖快速跳页项：当 `PaginationNavItem.PaginationItemType` 为 `JumpPrevious` 或 `JumpNext` 时，
marker 被动态移除；该单元格后续被复用为页码项时 marker 重新加回。这对应上游 jump-prev / jump-next 不接受
`styles.item` 的行为。

`item` 是运行时创建的语义标记。`PaginationTheme.axaml` 在 `PART_Nav`（`PaginationNav`）上声明
`Classes.semantic-scope-nav="True"` 作为路由作用域；marker 由 `PaginationNavItem` 在初始化与
`PaginationItemType` 变化时同步，不依赖静态模板 marker。`PaginationNav` 固定预建
`Pagination.MaxNavItemCount`（9）个 `PaginationNavItem` 容器，未进入显示区间的容器保持隐藏但仍携带
marker；有效可见 item 数量由当前显示区间决定。

`ContractType` 为 `ContentControl`：`PaginationNavItem` 是 internal 类型，不能作为公共 Setter 依赖的最低类型，
`ContentControl` 同时提供 `Background`、`BorderBrush`、`CornerRadius`、`Padding` 等条目样式能力。

### 2.2 `SimplePagination`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `SimplePagination` |
| Part | `root` |
| Selector | 不适用（owner 自身） |
| SelectorRoute | 不适用 |
| ContractType | `SimplePagination` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `SimplePagination` owner |
| 职责 | 简洁分页控件根语义区域，承载分页状态、布局入口和主题视觉。 |
| 相关 API | `CurrentPage`、`PageSize`、`Total`、`SizeType`、`Align`、`IsReadOnly` |
| 相关 Token | Pagination Token + SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是 `SimplePagination` owner 本身，语义与 `Pagination` 的 `root` 一致。`SimplePaginationTheme.axaml` 的根模板
同样在 `StackPanel#PART_RootLayoutPart` 外包裹 TemplateBind 根视觉属性（含虚线边框属性）的
`atom:DashedBorder`，root 定制作用于 owner 自身。

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `SimplePagination` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `/template/ .semantic-item` |
| ContractType | `ContentControl` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `PART_PreviousNavItem` 与 `PART_NextNavItem`（`PaginationNavItem`） |
| 职责 | 承载简洁模式上一页/下一页导航单元的尺寸、状态与点击语义。 |
| 相关 API | `CurrentPage`、`IsReadOnly`、`IsEnabled` |
| 相关 Token | `ItemSize`、`ItemSizeSM` 等 Pagination Token |
| 稳定性 | stable since 6.2.0 |

`item` 是内置模板中的静态标记：`SimplePaginationTheme.axaml` 在 `PART_PreviousNavItem` 与
`PART_NextNavItem` 上声明 `Classes.semantic-item="True"`，每个内置模板恰好两个 marker。

它明确不覆盖快速跳转输入框 `PART_QuickJumper`（`QuickJumpEdit`）与信息指示文本
`PART_InfoIndicator`，对应上游 simple pager 只对 prev/next 应用 `styles.item` 的行为。

#### `info`

| 字段 | 值 |
| --- | --- |
| Owner | `SimplePagination` |
| Part | `info` |
| Selector | `.semantic-info` |
| SelectorRoute | `/template/ .semantic-info` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `PART_InfoIndicator`（`TextBlock`） |
| 职责 | 承载简洁模式 "当前页 / 总页数" 信息文本的字体、颜色与对齐等文本样式。 |
| 相关 API | `CurrentPage`、`Total`、`IsEnabled` |
| 相关 Token | Pagination Token（`ItemSize`、`ItemSizeSM` 等） |
| 稳定性 | stable since 6.2.0 |

`info` 是内置模板中的静态标记：`SimplePaginationTheme.axaml` 在 `PART_InfoIndicator` 上声明
`Classes.semantic-info="True"`，每个内置模板恰好一个 marker，不随 `CurrentPage`、`IsReadOnly` 或禁用状态增删。

该 Part 用于格式化 "当前页 / 总页数" 信息文本，是 AtomUI 在 `SimplePagination` 上的扩展；上游 Ant Design 的
`PaginationSemanticType` 只有 `root` + `item`，simple pager 的文本节点位于 item 内部，没有独立 Part。

## 3. Selector 用法

应用侧使用 owner-scoped selector 和生成的 Style 类型，不手写 `/template/` 路径：

```xml
<Style Selector="atom|Pagination.semantic-demo">
    <atom:PaginationItemStyle x:SetterTargetType="ContentControl">
        <Setter Property="CornerRadius" Value="999" />
    </atom:PaginationItemStyle>
</Style>

<Style Selector="atom|Pagination[SizeType=Small]">
    <atom:PaginationItemStyle x:SetterTargetType="ContentControl">
        <Setter Property="Background" Value="#4DC8C8C8" />
        <Setter Property="Margin" Value="0,0,4,0" />
        <Style Selector="^:selected">
            <Setter Property="Background" Value="{atom:PaginationTokenResource ItemActiveBg}" />
        </Style>
    </atom:PaginationItemStyle>
</Style>

<Style Selector="atom|SimplePagination.semantic-demo">
    <atom:SimplePaginationItemStyle x:SetterTargetType="ContentControl">
        <Setter Property="Background" Value="#4DC8C8C8" />
    </atom:SimplePaginationItemStyle>
</Style>

<Style Selector="atom|SimplePagination.semantic-demo">
    <atom:SimplePaginationInfoStyle x:SetterTargetType="TextBlock">
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="Foreground" Value="{atom:PaginationTokenResource ItemActiveBg}" />
    </atom:SimplePaginationInfoStyle>
</Style>
```

`PaginationItemStyle`、`SimplePaginationItemStyle` 与 `SimplePaginationInfoStyle` 位于 `AtomUI.Theme.Styling`
命名空间，由语义生成器根据 `Pagination.SemanticParts.cs` / `SimplePagination.SemanticParts.cs` 生成。生成类型已封装
owner 类型保护和 `SelectorRoute`，`PaginationItemStyle` 通过 `/template/ .semantic-scope-nav > .semantic-item` 路由到
分页导航区域。
owner-scoped selector 可以是状态 selector（如 `atom|Pagination[SizeType=Small]`），对应上游把 `styles` 传成
函数、按 props 决定返回值的用法。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期类型
上下文；它不参与 `.semantic-*` 的身份匹配。条目状态（hover、选中、禁用）由 `PaginationNavItemTheme` 的主题
selector 表达；Semantic Style 的 Setter 以 trigger 优先级同时覆盖主题基础值与状态值，如果希望保留主题的某个
状态视觉（如选中态背景），应在生成的 Style 内用嵌套 selector（如 `^:selected`）重新声明该状态的值，例如上面
引用 `PaginationTokenResource.ItemActiveBg` 的写法。

## 4. 状态与数量语义

- `root` 始终恰好一个，不随状态、模板重套用或集合变化增删。
- `Pagination.item` 的 marker 数量跟随 `PaginationNav` 的固定容器池（9 个 `PaginationNavItem`）；有效可见的
  item 数量等于当前显示区间内上一页/下一页加页码项的数量。`JumpPrevious` / `JumpNext` 在当前区间出现时无 marker，被复用为
  页码项后重新获得 marker。
- `SimplePagination.item` 的内置 marker 数量恒为 2（上一页/下一页各一），不随 `CurrentPage`、`IsReadOnly` 或
  禁用状态增删。
- `SimplePagination.info` 的内置 marker 数量恒为 1，不随 `CurrentPage`、`IsReadOnly` 或禁用状态增删。
- 显示区间变化只改变 marker 落在哪些容器上，不要求应用重建样式或重新匹配 selector。
- `PaginationItemType` 是 marker 同步的唯一驱动状态；`IsVisible` 不参与 marker 判定。

## 5. 定制边界

- `root` 的定制直接作用于 owner 自身。可稳定依赖的根视觉属性为 `TemplatedControl` 的 `Background`、
  `BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`，以及 `AbstractPagination` 的
  `BorderDashArray`、`BorderDashOffset`；根模板 `atom:DashedBorder` 对这些属性 TemplateBind（虚线属性映射到
  `StrokeDashArray` / `StrokeDaskOffset`），默认值不改变既有外观。
- `item` 的定制通过生成的 `PaginationItemStyle` / `SimplePaginationItemStyle` 表达，`x:SetterTargetType`
  必须兼容 `ContentControl`；不得在应用样式中依赖 `PaginationNavItem` 类型。
- `info` 的定制通过生成的 `SimplePaginationInfoStyle` 表达，`x:SetterTargetType` 必须兼容 `TextBlock`；适合
  字体、颜色、对齐等文本样式，不得依赖 `PART_InfoIndicator` 节点身份以外的模板结构。
- 布局型 Setter（`Margin`、`Padding`、`Width`）作用于条目的 Measure/Arrange，需按显示区间变化验证 owner 尺寸
  与裁剪边界。
- 快速跳页项、`PART_QuickJumper`、`PART_InfoIndicator`、`PART_TotalInfoPresenter`、
  `PART_SizeChangerPresenter`、`PART_QuickJumperBarPresenter` 明确不属于 `item`，不能通过 Semantic Style 承诺样式。
- `PaginationNav` 上的 `semantic-scope-nav` 是路由作用域标记，不是公开 Semantic Part，应用不应直接依赖。

## 6. 兼容性与验证

- `Pagination` 与 `SimplePagination` 的 descriptor 均公开 `root` + `item`，与 Ant Design 6 的
  `PaginationSemanticType` 对齐；`item` 均为 `Multiple`。`SimplePagination` 额外公开 AtomUI 扩展的
  `info`（`Single`），不属于 `PaginationSemanticType` 对齐范围。
- `root` 是隐式 owner，不存在 `.semantic-root` marker。
- `PaginationTheme.axaml` 只含 `semantic-scope-nav` 作用域标记；`SimplePaginationTheme.axaml` 含两个静态
  `semantic-item` marker 与一个静态 `semantic-info` marker；其余 Pagination 主题文件不含语义 marker。
- `Pagination.item` 的 `SelectorRoute` 为 `/template/ .semantic-scope-nav > .semantic-item`，
  `SimplePagination.item` 与 `SimplePagination.info` 为默认 `/template/ .semantic-item` /
  `/template/ .semantic-info`，删除或重命名 scope 标记、改变 marker 类型或 cardinality 时必须同步生成
  descriptor、主题 marker、运行时代码与回归测试。
- 运行时 marker 行为回归测试见 `tests/AtomUI.Desktop.Controls.Tests/Pagination/PaginationSemanticPartTests.cs`；
  Gallery 预览与样式示例验证见 `tests/AtomUIGallery.Tests/ShowCases/PaginationShowCasePageTests.cs`。
