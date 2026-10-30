# DataGrid 分页面板 Extra Content 设计

本文定义 DataGrid 分页面板额外区域的 API、对齐模型、模板组合、可见性、生命周期和验证契约。DataGrid 的整体设计见
[DataGrid 桌面版架构设计](overview.md)，内部实现边界见 [DataGrid 桌面版实现原理](implementation.md)。

## 1. 设计定位

DataGrid 分页面板由 Pagination 与可选 Extra Content 两部分组成：

```text
[Pagination]                    [Extra Content]
[Extra Content]                    [Pagination]
```

Extra Content 是分页区域的扩展展示入口，可用于筛选摘要、批量操作、统计信息或其他业务入口。它不参与 Query、
PageRequest、PageSize、Source request 或分页状态计算。

Extra Content 相对于同一条分页面板和 Pagination 垂直居中。

`Pagination` 自身保持原有用户体验：

```text
Total Info -> Page Navigation -> Page Size Selector -> Quick Jumper
```

页大小选择器仍由 `Pagination.IsShowSizeChanger` 控制，并保持在页码导航右侧。DataGrid 的
`IsShowPageSizeSelector` 只转发该可见性，不改变 Pagination 内部布局。

## 2. 设计原则

- Pagination 是页码、页大小、页数和导航意图的唯一状态 owner。
- Extra Content 只属于 DataGrid 分页面板，不成为第二个分页状态 owner。
- Pagination 的默认布局、模板 part、事件顺序和自定义 `SizeChangerTemplate` 体验保持不变。
- Pagination 对齐决定 Pagination 的位置，Extra Content 自动占用相反一侧。
- Extra Content 与 Pagination 必须作为一个有效可见性整体显示或隐藏。
- 没有 Extra Content 时，DataGrid 分页区域的几何与交互保持现有行为。

## 3. Public API

DataGrid 提供四个稳定属性：

```csharp
public object? TopPaginationExtraContent { get; set; }
public IDataTemplate? TopPaginationExtraContentTemplate { get; set; }

public object? BottomPaginationExtraContent { get; set; }
public IDataTemplate? BottomPaginationExtraContentTemplate { get; set; }
```

| API | 默认值 | 语义 |
| --- | --- | --- |
| `TopPaginationExtraContent` | `null` | 顶部分页面板的额外内容。 |
| `TopPaginationExtraContentTemplate` | `null` | 顶部额外内容的可选数据模板。 |
| `BottomPaginationExtraContent` | `null` | 底部分页面板的额外内容。 |
| `BottomPaginationExtraContentTemplate` | `null` | 底部额外内容的可选数据模板。 |

内容解析规则：

- Content 为 `null` 时，不创建或显示对应 Extra Content Presenter。
- Content 非空且 Template 为 `null` 时，直接显示 Content。
- Content 非空且 Template 非空时，使用 Template 生成视觉内容。
- 顶部和底部属性彼此独立，可以使用不同 Content、Template、Control 实例和 DataContext。
- 同一个 `Control` 实例不能同时作为顶部和底部 Content；同时显示两处时必须使用独立实例或各自模板生成视觉。
- Template 替换遵循普通 `ContentPresenter` 生命周期，不缓存旧视觉内容。

## 4. 对齐模型

`TopPaginationAlign` 和 `BottomPaginationAlign` 继续决定 Pagination 的位置，Extra Content 使用相反一侧。

| Pagination Align | Pagination | Extra Content | 布局 |
| --- | --- | --- | --- |
| `Start` | 左对齐 | 右对齐 | `[Pagination] ... [Extra Content]` |
| `End` | 右对齐 | 左对齐 | `[Extra Content] ... [Pagination]` |
| `Center` | 在剩余区域居中 | 右对齐 | `[Extra Content] ... [Pagination centered]` |

`Center` 的稳定语义是：先为右侧 Extra Content 保留自然宽度，再让 Pagination 在剩余区域内居中。不要求 Pagination
相对于包含 Extra Content 的整个宽度绝对居中。

Extra Content 不设置独立对齐 API。它的位置完全由对应 Pagination Align 推导，避免两个属性产生冲突组合。

## 5. 架构与所有权

| 节点 | 类型 | Owner | 职责 | 稳定性 |
| --- | --- | --- | --- | --- |
| `DataGrid` | public control | 应用/VisualTree | 持有 Extra Content API、分页可见性和 Pagination 状态转发。 | public |
| `PART_TopPaginationPanel` | `DockPanel` | DataGrid template | 顶部分页区域的两栏组合与对齐。 | template-stable |
| `PART_BottomPaginationPanel` | `DockPanel` | DataGrid template | 底部分页区域的两栏组合与对齐。 | template-stable |
| `PART_TopPaginationExtraContentPresenter` | `ContentPresenter` | DataGrid template | 顶部 Extra Content 的内容宿主。 | template-stable |
| `PART_BottomPaginationExtraContentPresenter` | `ContentPresenter` | DataGrid template | 底部 Extra Content 的内容宿主。 | template-stable |
| `PART_TopPagination` | `Pagination` | DataGrid template | 顶部分页状态和交互。 | template-stable |
| `PART_BottomPagination` | `Pagination` | 底部分页状态和交互。 | template-stable |

Extra Content Presenter 不保存 DataGrid、Pagination、Source 或 PageRequest 引用。Content 和 Template 由 DataGrid
StyledProperty 投影，应用负责其业务数据生命周期。

## 6. Template 与组合契约

顶部分页面板使用 `DockPanel LastChildFill=True`：

```text
PART_TopPaginationPanel
  -> PART_TopPaginationExtraContentPresenter
  -> PART_TopPagination
```

底部分页面板结构相同：

```text
PART_BottomPaginationPanel
  -> PART_BottomPaginationExtraContentPresenter
  -> PART_BottomPagination
```

布局规则：

- `Start`：Extra Content Presenter dock 到右侧，Pagination 填充剩余区域并左对齐。
- `End`：Extra Content Presenter dock 到左侧，Pagination 填充剩余区域并右对齐。
- `Center`：Extra Content Presenter dock 到右侧，Pagination 填充剩余区域并居中。
- Pagination 内部 `Align` 在组合面板中按 Start 语义参与自然宽度计算，不移动其 page-size changer。
- 分页面板通过 `PaginationPanelMargin` / `PaginationPanelMarginSM` 保留 Start/End 两侧一致的水平外边距；Extra Content 与 Pagination 之间的水平间距沿用 Pagination SizeType 间距。

模板重套用时必须重新获取四个 Extra/Pagination part，先释放旧 presenter 引用，再回放 Content、Template、可见性和对齐状态。

## 7. 可见性与行为

有效可见性由 DataGrid 统一计算，不能只切换内部 Pagination 的 `IsVisible`。

| 条件 | 顶部分页面板 | 底部分页面板 |
| --- | --- | --- |
| `PaginationVisibility=None` | 隐藏 | 隐藏 |
| `PaginationVisibility=Top` | 显示 | 隐藏 |
| `PaginationVisibility=Bottom` | 隐藏 | 显示 |
| `PaginationVisibility=All` | 显示 | 显示 |
| `PageSize=0` | 隐藏 | 隐藏 |
| `IsHideOnSinglePage=True` 且仅一页 | 隐藏 | 隐藏 |
| `IsHideOnSinglePage=False` | 按 PaginationVisibility | 按 PaginationVisibility |

Extra Content 和 Pagination 必须同时进入或退出可见状态。禁止出现独立 Extra Content、独立 Pagination 或仅一侧残留间距。

分页状态流保持不变：

```text
DataGridPageRequest
  -> DataGridFetchRequest
  -> AppliedPageRequest / TotalItemCount
  -> top/bottom Pagination
```

页大小选择器继续通过 `Pagination.PageSize`、`PageChangedEventArgs.PageSize` 和 `DataGridPageRequest` 更新数据请求。
Extra Content 不参与该路径。

## 8. 资源、性能与 AOT

- Extra Content 使用标准 `ContentPresenter`、`Content`、`ContentTemplate`，不引入反射、动态类型发现或运行时注册。
- 没有 Content 时 Presenter 可保持未物化状态，不创建模板视觉。
- 顶部和底部 Presenter 独立物化内容，不共享同一个 Visual parent。
- Template 替换、DataGrid re-template 和 detach 不保留旧 Content Visual 或模板订阅。
- 对齐只参与布局，不在 pointer、scroll、measure 热路径引入额外分配。

## 9. 兼容性与定制边界

- 四个 Extra Content API 为 additive property，默认 `null`。
- `IsShowPageSizeSelector`、`PageSize`、`PageRequest`、`PaginationVisibility` 和 Align API 保持现有语义。
- `Pagination` 不新增 Extra Content API，也不改变默认模板顺序。
- 应用替换 DataGrid 主题时，必须保持四个稳定 part、两栏组合、有效可见性和相反侧对齐规则。
- Extra Content 的颜色、字体、尺寸、交互和可访问性由应用内容负责；DataGrid 只负责位置、可见性和宿主生命周期。

## 10. 验证要求

- API：四个属性默认值、Content/Template 解析、顶部/底部独立内容和运行时替换。
- Layout：Start/End/Center、Extra Content 空/非空、Small/Middle/Large、长内容和窄宽度。
- Visibility：`PaginationVisibility` 全矩阵、`PageSize=0`、`IsHideOnSinglePage` 和模板重套用。
- Pagination UX：默认 ComboBox 与 `SizeChangerTemplate` 都保持 `Page Navigation -> Page Size Selector`。
- State：Extra Content 不改变 `PageRequest`、Source request、页大小变更和顶部/底部同步。
- Lifecycle：Content/Template 替换、re-template、detach 和窗口关闭后不保留旧视觉。
- Gallery：Basic Paging 展示相反侧 Extra Content，并继续展示 Pagination 右侧 page-size selector。
