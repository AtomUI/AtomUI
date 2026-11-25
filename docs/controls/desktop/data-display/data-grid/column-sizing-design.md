# DataGrid 列宽分配设计

本文档定义 DataGrid 在普通表头、分组表头、空数据和已物化行场景下的列宽测量与分配契约。公共设计入口见 [DataGrid 桌面版架构设计](overview.md)，源码职责与维护入口见 [DataGrid 桌面版实现原理](implementation.md)。

## 1. 设计定位

列宽分配负责把 `DataGridLength` 的公共语义、内容测量结果和当前有限列视口宽度收敛为每个可见列的最终显示宽度。

该设计覆盖：

- `Pixel`、`Auto`、`SizeToHeader`、`SizeToCells` 和 `Star` 五种宽度模式。
- 普通列头与分组列头的内容测量。
- 已物化单元格的内容测量。
- 空数据、数据新增、数据清空和视口尺寸变化。
- 行头、滚动条、冻结列、最小/最大列宽和 filler 列共同参与的布局边界。

列宽求解不依赖某个特定 rows presenter 是否可见或是否参与布局。只要当前布局路径能够提供有效的有限列视口宽度，DataGrid 就能完成同一套列宽求解。

## 2. 设计原则

- `DataGrid` 是列宽状态和列宽求解的唯一 owner；presenter 只提供测量结果或可用视口宽度。
- 有限视口宽度的来源可以随模板状态变化，但 star 分配公式和约束处理只能存在一份。
- 内容驱动列的初始测量与 star 列的剩余空间分配是两个阶段，不能由同一个布尔状态隐式混合。
- 普通表头、分组表头、行和单元格必须消费同一组已解析列宽，不能分别维护并行宽度。
- 空数据视觉继续由模板控制；不能通过强制显示 `DataGridRowsPresenter` 获得列宽输入。
- filler 只表示现有列在约束下确实无法吸收的空间，不能替代 star 列的正常分配。
- `MinWidth`、`MaxWidth`、用户调整、冻结列和滚动布局继续复用统一列宽调整算法，不为特定 presenter 建立旁路。
- range source 和纵向虚拟化只允许已实现的 cell 参与内容测量；列宽求解不能为了发现未加载内容的宽度而请求其他 range、扫描整页之外的数据或扩大 realized window。

## 3. 列宽模型与 Public API

相关公共契约包括：

- `DataGrid.ColumnWidth`：未显式设置列宽时的控件级默认值，默认为 `DataGridLength.Auto`。
- `DataGridColumn.Width`：单列宽度模式、模式参数、期望宽度和显示宽度的入口。
- `DataGrid.MinColumnWidth`、`DataGrid.MaxColumnWidth` 与列级最小/最大宽度：共同形成最终有效约束。
- `DataGridLength` 与 `DataGridLengthUnitType`：定义宽度模式，不暴露 presenter 或求解阶段状态。

列宽计算使用以下术语：

| 术语 | 含义 |
| --- | --- |
| 列视口宽度 | 当前布局能够分配给数据列的有限宽度；不把行头宽度重复计入列宽。 |
| 期望宽度 | `DesiredValue` 表达的内容或显式宽度需求。 |
| 显示宽度 | `DisplayValue` 经有效最小/最大宽度约束后的当前布局宽度。 |
| 可见带边列宽 | 所有可见非 filler 列的当前总宽度，包含列边界计算需要的宽度。 |
| 剩余空间 | 列视口宽度减去可见带边列宽，正值表示可扩展，负值表示需要收缩。 |
| filler 宽度 | 统一列宽调整完成后仍未被真实列吸收的正剩余空间。 |

各宽度模式保持以下语义：

| 模式 | 内容测量 | 有限视口下的显示语义 | 空数据语义 |
| --- | --- | --- | --- |
| `Pixel` | 不依赖内容 | 使用显式宽度，并受有效最小/最大宽度约束。 | 与有数据时一致。 |
| `Auto` | 取表头和已测量单元格内容需求的最大值 | 先确定内容期望宽度，再由 star 列处理剩余空间。 | 没有单元格内容时由表头测量确定期望宽度。 |
| `SizeToHeader` | 只测量表头 | 使用表头期望宽度，并受有效最小/最大宽度约束。 | 与有数据时一致。 |
| `SizeToCells` | 只测量已物化单元格 | 使用当前已知单元格期望宽度，并受有效最小/最大宽度约束。 | 保留当前约束下的基线宽度，数据出现后再由单元格测量增长。 |
| `Star` | 不以内容决定权重 | 按 `Value` 权重分配其他列确定后的有限剩余空间，并受有效最小/最大宽度约束。 | 与有数据时使用相同权重分配。 |

当列视口宽度为正无穷时，不构造虚假的有限剩余空间。列继续使用既有的无限空间退化规则，star 列进入受既有上限约束的非有限分配路径。

## 4. 布局状态策略

列宽求解根据当前能够提供可靠几何输入的布局参与者选择宽度来源：

| 布局状态 | 有限宽度来源 | 内容测量职责 | star 求解 |
| --- | --- | --- | --- |
| 有已物化行 | `DataGridRowsPresenter` / `DataGridCellsPresenter` 形成的 `CellsWidth` | 表头测量 header；cells 测量已物化内容。 | DataGrid 使用 `CellsWidth` 调用统一求解。 |
| 初始空数据 | 当前可见的列头 presenter 的有限 `availableSize.Width` | 普通或分组表头完成 header 测量；`SizeToCells` 保持基线。 | DataGrid 使用列头视口宽度调用统一求解。 |
| 数据清空 | 列头 presenter 接管有限宽度来源 | 保留当前已知内容期望值，并按空数据规则继续布局。 | 不依赖已隐藏的 rows presenter。 |
| 空数据新增首批数据 | rows/cells 路径重新提供 `CellsWidth` | 新物化 cells 可以增长 `Auto` 或 `SizeToCells` 的期望宽度。 | 内容测量完成后重新运行同一求解。 |
| 普通表头 | `DataGridColumnHeadersPresenter` | 测量 `Auto` / `SizeToHeader` header。 | 使用共享求解器。 |
| 分组表头 | `DataGridGroupColumnHeadersPresenter` | 测量叶列和分组 header。 | 使用共享求解器，不复制公式。 |
| 无限宽度 | 无有限列视口 | 按各内容模式继续测量。 | 不执行有限剩余空间分配。 |

`AutoSizingColumns` 只表示初始内容测量尚未完成。它不是“是否允许 star sizing”的永久开关，也不表示 rows presenter 是否存在。初始内容测量完成后，由明确的完成入口标记列的初始期望宽度已确定，再单独执行 star 分配。

## 5. 架构与职责

| 组件 | 输入 | 输出与职责 | 不负责 |
| --- | --- | --- | --- |
| `DataGrid` | 列模型、有效约束、有限列视口宽度、当前内容测量状态 | 持有列宽状态；完成初始 Auto 阶段；调用统一调整算法；触发表头、行、滚动条和 filler 更新。 | 不从空数据视觉反推宽度，不为普通/分组表头复制算法。 |
| `DataGridColumn` | public 宽度模式、列级约束、内容期望宽度 | 保存 `DataGridLength` 的期望值和显示值，提供布局舍入后的实际宽度。 | 不决定整个视口的剩余空间。 |
| `DataGridColumnHeadersPresenter` | 普通表头内容、有限 `availableSize.Width` | 测量 header；在 rows 不参与布局时提供列视口宽度；按已解析宽度重新测量。 | 不直接分配 star 权重，不持有第二套列宽状态。 |
| `DataGridGroupColumnHeadersPresenter` | 分组结构、叶列表头内容、有限 `availableSize.Width` | 与普通表头遵守相同宽度输入契约，并维护分组 header 的组合测量。 | 不建立分组模式专属列宽公式。 |
| `DataGridRowsPresenter` | 行区域可用尺寸、行头和滚动布局 | 在正常数据布局中更新可用于 cells 的几何输入，并消费统一列宽结果。 | 空数据时不承担隐藏视觉之外的列宽求解前置条件。 |
| `DataGridCellsPresenter` | 已物化 cell 内容、`CellsWidth` | 测量 `Auto` / `SizeToCells` 内容，完成正常数据路径的初始内容测量。 | 不独立调整整组 star 列。 |
| `DataGridFillerColumn` | 统一求解后的真实剩余空间 | 投影无法由真实列吸收的正剩余宽度。 | 不填补尚未执行的 star 分配。 |

内部职责可以由两个语义入口表达：

- `CompleteAutoSizing(double availableCellsWidth)`：结束初始内容测量阶段，标记相关列的初始期望宽度已确定，并把有限宽度交给 star 求解。
- `ResolveStarColumnWidths(double availableCellsWidth)`：只负责在给定有限列视口下调整已知列宽。

这两个入口属于内部架构边界，不新增 public/protected API。

## 6. Template 与组合契约

默认 `DataGridTheme.axaml` 同时包含：

- `PART_ColumnHeadersPresenter`：普通表头 presenter。
- `PART_GroupColumnHeadersPresenter`：分组表头 presenter。
- `PART_RowPresenter`：行 presenter。
- `EmptyIndicator`：空数据内容区域。

普通表头和分组表头按 `IsGroupHeaderMode` 互斥显示，但都实现相同的有限宽度提供契约。`PART_RowPresenter` 在 `IsEmptyDataSource=true` 时保持隐藏，`EmptyIndicator` 同时显示。空数据列宽由当前可见表头 presenter 的测量宽度驱动，不改变这些模板可见性规则。

替换 DataGrid 模板时，应用负责保留以下组合语义：

- 至少一个表头 presenter 能在表头可见时获得列区域的真实有限宽度。
- rows/cells 路径提供的宽度必须与表头使用同一列坐标系。
- 行头、垂直滚动条或自定义占位区域消耗的宽度只能扣除一次。
- filler、header、row 和 cell 必须消费同一组 `DataGridColumn.DisplayValue`。

## 7. 核心算法与数据流

### 7.1 有限视口下的 star 求解

输入为已经归一到列坐标系的 `availableCellsWidth`，单位为 Avalonia 设备无关像素。输出为所有可见列受约束后的显示宽度以及可能存在的 filler 宽度。

```text
1. 确认可用宽度是有限值。
2. 刷新可见列数量、star 列数量和可见带边列宽。
3. 初始 Auto 内容测量未完成时，保留当前内容测量结果并退出本轮 star 分配。
4. 没有可见 star 列时退出。
5. adjustment = availableCellsWidth - VisibleEdgedColumnsWidth。
6. adjustment 非零时，从 display index 0 调用统一 AdjustColumnWidths。
7. 重新计算 VisibleEdgedColumnsWidth。
8. 用已解析宽度重新测量 header/cell，并更新滚动条与 filler 投影。
```

`AdjustColumnWidths` 继续负责增长、收缩、star 权重、有效最小/最大宽度和用户调整相关约束。调用方不能先手工修改部分 star 列，再把剩余量交给该算法。

如果所有可调整列达到约束后仍有正剩余空间，剩余量成为 filler；只要 star 列仍能吸收空间，filler 必须为零。

### 7.2 初始 Auto 阶段

初始内容测量按以下顺序收敛：

```text
header presenter measure
  -> Auto / SizeToHeader desired width
realized cells measure (when present)
  -> Auto / SizeToCells desired width
CompleteAutoSizing(availableCellsWidth)
  -> mark initial desired widths determined
  -> ResolveStarColumnWidths(availableCellsWidth)
  -> invalidate headers and rows for resolved-width measure
```

空数据时不存在需要等待的 cell 内容，当前可见表头 presenter 可以在 header 测量完成后结束初始 Auto 阶段。数据随后出现时，新的 cell 内容仍可通过现有 Auto growth 路径提高期望宽度，并重新触发统一分配。

### 7.3 range source、分页与稳定水平 extent

`Auto` 和 `SizeToCells` 的“已测量单元格”严格指当前已经实现并进入正常 measure 的 cells。对于 range source、分页和纵向虚拟化，未请求 range、未实现行以及其他页的数据不属于当前列宽输入。DataGrid 不预取或遍历这些数据来推断全局最大内容宽度，否则会把列宽测量变成隐式 Source I/O，破坏有界 range、首屏延迟和虚拟化复杂度。

因此，纯 `Auto` 列可以在新的已实现内容更宽时自然增长，水平 scrollbar 也可以随真实 extent 增长；这属于内容驱动宽度的既定语义。如果产品要求首屏即具有稳定、内容不被压缩的水平 extent，列定义必须提供确定的几何基线：使用 `Pixel Width` 表达固定宽度，或在保留 `Auto` 增长能力时声明 `MinWidth`，必要时再配合 `MaxWidth`。`Star` 表达的是在有限视口内分配剩余空间，可能压缩内容，不等价于稳定内容宽度；强制 `HorizontalScrollBarVisibility=Visible` 只改变轨道可见性，也不能建立真实 extent。

Gallery 的 Basic Paging 示例采用 `Auto + MinWidth`：首屏列最小宽度总和超过该示例的列视口时自然出现水平 scrollbar，翻页后更宽的已实现内容仍可扩展列宽，同时不读取其他页来预热宽度。

### 7.4 失效与重新计算

以下变化会使列宽输入或结果失效：

- 列视口宽度变化，包括控件 resize、行头有效宽度变化和滚动条布局变化。
- 列新增、移除、显示状态、display index、宽度模式、star 权重或最小/最大宽度变化。
- `Auto`、`SizeToHeader` 或 `SizeToCells` 获得新的内容期望宽度。
- 空数据与有数据状态切换、普通表头与分组表头切换、模板重新应用。
- 用户调整列宽或冻结列布局改变可用区域。

重新计算必须从 DataGrid owner 进入统一路径。若有限宽度、可见列总宽度和有效约束均未变化，求解器应避免重复写入宽度和无效 measure 循环。

## 8. 资源、性能与 AOT 边界

列宽分配是同步纯数值布局逻辑，不引入 DynamicResource、额外 binding、事件订阅、timer、异步任务或视觉对象。

性能约束：

- 复用现有列集合缓存、可见列统计和 `AdjustColumnWidths`，不在每个 presenter 中复制遍历与分配算法。
- adjustment 为零、无 star 列、宽度无限或 Auto 阶段未完成时尽早退出。
- presenter 只在内容测量或视口输入变化后请求重新测量，不能在稳定布局中持续互相失效。
- filler 只做最终投影，不参与反复试算。

该设计不使用反射、动态发现、运行时注册或字符串成员访问，不增加 trimming 或 NativeAOT 风险。

## 9. 兼容性与定制边界

列宽设计不新增或改变 public/protected API、Avalonia 属性、默认值、template part、伪类、ControlTheme key、资源 key 或 DataGrid Token。

以下语义保持稳定：

- `DataGridLength` 五种模式及其 XAML 转换语义。
- 控件级和列级最小/最大宽度优先级。
- 用户调整列宽、冻结列、行头和滚动条继续通过现有布局与 `AdjustColumnWidths` 协作。
- 表头拖拽手柄：普通列与左冻结列的缝调整左侧列，向右拖变宽；右冻结列拥有的缝（其左缘，及两个右冻结列之间的缝）调整右侧的右冻结列，向左拖变宽。右冻结列贴在表格右缘，其前一列可能已滚出视口或被冻结区遮住，不能作为该缝的调整目标。
- 普通表头与分组表头共享同一列显示宽度。
- 空数据继续显示 `EmptyIndicator` 并隐藏 `PART_RowPresenter`。
- 无限宽度环境不伪造有限 star 分配。

自定义模板可以改变视觉组合，但必须提供与默认模板等价的列视口宽度语义。自定义列可以扩展 header/cell 内容和编辑行为，但不能绕过 `DataGridColumn.Width` 或建立独立 star solver。

## 10. 验证要求

列宽验证必须直接覆盖状态、模式和约束组合：

| 验证组 | 场景 |
| --- | --- |
| 空数据基础 | `Auto + * + *`、全 star、`Pixel + *`、`SizeToHeader + *`、`SizeToCells + *`。 |
| 权重 | `1* + 2*` 按有限剩余空间保持权重比例。 |
| 状态切换 | 初始空数据、空视口 resize、空数据新增、再次清空。 |
| 表头变体 | 普通表头和分组表头产生相同的叶列显示宽度。 |
| 约束 | 单列和控件级 min/max、所有 star 达到 `MaxWidth`、收缩到 `MinWidth`。 |
| 布局组合 | 左/右冻结列、行头、垂直和水平滚动条、用户调整列宽。 |
| range 与分页 | `Auto + MinWidth` 首屏形成自然水平 overflow；翻页前后 scrollbar 保持正确；不得为宽度发现请求未显示 range。 |
| filler | star 可吸收剩余空间时为零；所有可调整列受约束后才允许为正。 |
| 无限空间 | 不执行有限 star 剩余空间分配，并保持既有退化行为。 |

实现变更至少运行 DataGrid 定向测试和 `git diff --check`。只有修改模板结构时才需要额外执行主题契约验证；只有引入 AOT 敏感能力时才需要增加 NativeAOT publish 验证。
