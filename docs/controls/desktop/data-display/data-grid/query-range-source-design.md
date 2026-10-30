# DataGrid Query 与 Range Source 设计

本文档定义 DataGrid 的不可变查询、范围数据源、请求协调、虚拟化投影和远端数据交互契约。DataGrid 的整体公共设计见
[DataGrid 桌面版架构设计](overview.md)，内部 ownership 与维护入口见
[DataGrid 桌面版实现原理](implementation.md)，列宽求解见
[DataGrid 列宽分配设计](column-sizing-design.md)，视觉变量见 [DataGrid Token 设计](token.md)。

## 1. 设计定位

DataGrid 使用同一条数据管线承载本地集合、远端服务和其他可按范围读取的数据源：

```text
column / filter / group / programmatic intent
  -> immutable DataGridQuery
  -> DataGridQueryController
  -> DataGridRangeCoordinator
  -> IDataGridSource.FetchAsync(...)
  -> validated immutable DataGridRangeResult
  -> atomic presentation snapshot
  -> realized rows / headers / cells / pagination
```

DataGrid 负责 Query、请求代际、范围缓存、虚拟化投影和视觉状态；Source 负责解释字段、执行查询、形成稳定顺序并返回精确
范围。数据位于内存、HTTP 服务、数据库还是其他存储中，不改变 DataGrid 的状态机。

这一模型覆盖以下职责：

- 排序、过滤和分组的声明式查询。
- 连续虚拟滚动与分页模式下的范围读取。
- Source schema、稳定 row/group key 与 snapshot 一致性。
- 请求取消、迟到结果隔离、缓存、预取、失败回滚和失效刷新。
- 选择、current row、编辑和行移动在未加载数据上的稳定语义。
- Ready 视觉、列虚拟化、冻结列、RowDetails 和嵌套滚动的保持规则。

DataGrid 不内置 ORM、HTTP client、重试 DSL、业务缓存或服务端协议翻译。业务 Source 负责把 Query 映射到自己的协议。

## 2. 设计原则

### 2.1 单一状态源

- `DataGrid.Query` 是排序、过滤和分组条件的唯一 public owner。
- `DataGrid.AppliedQuery` 表示当前已提交 presentation snapshot 对应的 Query，只读且不可由模板或 Source 修改。
- `DataGridRange` 只表达视口读取范围，不属于 Query；滚动和预取不改变业务查询 identity。
- Header、FilterIndicator、SortIndicator、Cell、Pagination 和 Gallery 只投影 DataGrid 状态，不保存并行状态。
- Source 只消费 request 并返回 result，不回调修改 DataGrid Query。
- 本地与远端数据使用同一个 `IDataGridSource` 契约，不存在按来源类型分支的第二条数据路径。

### 2.2 原子一致性

- 一个 presentation snapshot 只组合相同 Source identity、Query revision、data generation 和 Source snapshot id 的 range。
- 已取消、过时、来自旧 Source、来自旧 snapshot 或违反合同的结果不能进入当前视图。
- Query、PageRequest、GroupExpansion、已提交范围、totals 和相关视觉状态以一次 UI-thread transition 提交。
- 刷新期间保留最后一次完整 presentation；失败时整体回退，不展示“新 Query + 旧 rows”的可交互混合状态。
- Source invalidation 失败可以保留旧快照作为 stale 只读展示，但不能继续提交编辑、删除或移动。

### 2.3 有界资源

- 内存与 Source 总行数无关，只与已实现容器、有限 range block、列数和用户显式产生的稀疏状态有关。
- 请求并发、block 数、range 大小、预取距离和 snapshot-expiry 自动重启次数都有确定上限。
- 每个 generation 同时只有一个活动 viewport request scope；过时 scope 的排队工作不能残留在并发闸门前形成历史积压。
- visible block 是前台工作，prefetch 是所属 committed viewport 的可撤销后台工作；后台工作不能获得高于前台缺口的调度顺序。
- Measure、Arrange、container prepare 和 recycle 不调用 Source、不等待异步结果、不分配 range buffer。
- 每项订阅、CancellationTokenSource、cache、pooled array 和 continuation 都有明确 owner 与释放条件。

### 2.4 Ready 视觉保持

- ControlTheme key、Template Part、伪类、Token、默认尺寸、列宽、圆角、滚动条和冻结列契约保持稳定。
- selected row 背景优先级继续高于 sorted cell tint；未选中行的 sorted cell 继续显示 sort tint。
- Query/Source 架构不增加 Ready 状态视觉节点，也不把 AXAML 视觉迁移为 C# 动态节点。
- Loading 在没有已提交 snapshot 且请求实际挂起时复用现有 `Spin` 区域；Refreshing 保持旧 rows 的布局几何与完整不透明度，
  不自动启动 Spin。

### 2.5 AOT 可证明

- Field identity 与显示 binding path 分离。
- Schema 显式声明字段和能力，DataGrid 不从首个 item 或 CLR property 反射推断查询能力。
- 本地 Source 使用静态强类型 getter、comparer 和 filter evaluator。
- 正常路径不使用 `Expression.Compile()`、`PropertyInfo.GetValue`、assembly scan、字符串成员发现或动态泛型构造。

## 3. Query 模型与 Public API

### 3.1 稳定 identity

`DataGridFieldId` 和 `DataGridOperatorId` 是 ordinal、大小写敏感、不可变的字符串值类型：

```csharp
public readonly struct DataGridFieldId : IEquatable<DataGridFieldId>
{
    public DataGridFieldId(string value);
    public string Value { get; }
}

public readonly struct DataGridOperatorId : IEquatable<DataGridOperatorId>
{
    public DataGridOperatorId(string value);
    public string Value { get; }
}
```

构造函数拒绝 null、空字符串、首尾空白和控制字符。FieldId 是数据协议 identity，不是 CLR property path；后端字段名与
客户端显示属性可以不同。

### 3.2 排序、过滤与分组

```csharp
public enum DataGridSortDirection
{
    Ascending,
    Descending
}

public readonly struct DataGridSort : IEquatable<DataGridSort>
{
    public DataGridFieldId Field { get; }
    public DataGridSortDirection Direction { get; }
}

public readonly struct DataGridFilter : IEquatable<DataGridFilter>
{
    public DataGridFieldId Field { get; }
    public DataGridOperatorId Operator { get; }
    public ImmutableArray<DataGridScalar> Values { get; }
}

public readonly struct DataGridGroup : IEquatable<DataGridGroup>
{
    public DataGridFieldId Field { get; }
    public DataGridSortDirection Direction { get; }
}
```

`DataGridScalar` 是确定性不可变判别联合，支持 null、bool、有符号/无符号整数、double、decimal、string、Guid、DateOnly、
TimeOnly 和 DateTimeOffset。它遵守以下归一规则：

- signed 与 unsigned integer kind 保持区分。
- double 拒绝 NaN 和 Infinity，并把 `-0d` 归一为 `0d`。
- DateTimeOffset 归一为 UTC instant。
- string equality/hash 使用 ordinal。
- 枚举由 Source 明确映射为整数或字符串，不把任意 object、delegate 或表达式树写入 Query。

`Sorts` 的顺序就是多列排序优先级；`Filters` 之间按 AND 组合，单个 filter 的 value 语义由 operator 定义；`Groups` 的顺序
就是分组层级。Groups 同时形成领先于 Sorts 的稳定排序，同一 FieldId 不能同时出现在 Groups 和 Sorts 中。

### 3.3 DataGridQuery

```csharp
public sealed class DataGridQuery : IEquatable<DataGridQuery>
{
    public static DataGridQuery Empty { get; }

    public ImmutableArray<DataGridSort> Sorts { get; }
    public ImmutableArray<DataGridFilter> Filters { get; }
    public ImmutableArray<DataGridGroup> Groups { get; }

    public DataGridQuery WithSorts(ImmutableArray<DataGridSort> sorts);
    public DataGridQuery WithFilters(ImmutableArray<DataGridFilter> filters);
    public DataGridQuery WithGroups(ImmutableArray<DataGridGroup> groups);
}
```

Query 通过只读构造和 `WithXxx` 变换，构造时归一 default `ImmutableArray`、验证 identity 与重复项并预计算结构 hash。结构
相同的 Query 是 no-op：不递增 revision、不触发事件、不刷新行、不发送请求。

Query 不包含页码、可见范围、加载状态、选择或 group expansion。这些状态不改变查询命中的业务集合。

### 3.4 DataGrid 数据状态

```csharp
public IDataGridSource? ItemsSource { get; set; }
public DataGridQuery Query { get; set; } = DataGridQuery.Empty;
public DataGridGroupExpansion GroupExpansion { get; set; } = DataGridGroupExpansion.AllExpanded;
public DataGridSelectionState Selection { get; set; } = DataGridSelectionState.Empty;
public DataGridRowKey? CurrentRowKey { get; set; }
public DataGridQuery AppliedQuery { get; }
public DataGridLoadState LoadState { get; }
public Exception? LoadError { get; }
public long TotalItemCount { get; }
public int TotalEntryCount { get; }
public bool IsDataStale { get; }
```

这些成员使用 DirectProperty 表达 runtime data state。`ItemsSource`、`Query`、`GroupExpansion`、`Selection` 和
`CurrentRowKey` 可绑定；Query、GroupExpansion 和 Selection 默认 TwoWay 且不接受 null。Applied、Load、Total 和 Stale
状态只读。

`ItemsSource` 保留 Avalonia 集合控件熟悉的入口名称，但不接受 `IEnumerable`：本地集合先通过
`DataGridLocalSource.Create(...)` 包装，远端数据实现 `IDataGridSource`。Source 的所有权仍属于调用方；DataGrid 只在附着期间
订阅 `Invalidated`，替换或 detach 时退订，但从不代替调用方 dispose Source。

```csharp
public enum DataGridLoadState
{
    Idle,
    Loading,
    Refreshing,
    Ready,
    Error
}
```

操作与观察入口为：

```csharp
public event EventHandler<DataGridQueryChangedEventArgs>? QueryChanged;

public void Reload();
public void SetSort(DataGridFieldId field, DataGridSortDirection? direction,
                    DataGridSortUpdateMode mode = DataGridSortUpdateMode.Replace);
public void ClearSorts();
public void CollapseGroup(DataGridGroupKey key);
public void ExpandGroup(DataGridGroupKey key);
```

`QueryChanged` 不可取消。它在 Query 完成内存提交、revision 递增和视觉投影之后触发，在对应 Source 请求开始之前触发。
事件参数包含 OldQuery、NewQuery、Revision 和 Reason；Reason 区分 External、SortGesture、FilterGesture、GroupChange 和
LoadRollback。事件处理器同步写入新 Query 时，较高 revision 自然使外层请求失效。

`Reload()` 不改变 Query，也不触发 QueryChanged，只创建新的 data generation。Collapse/Expand 只改变不可变
GroupExpansion；目标状态相同是零请求 no-op。

### 3.5 列契约与排序策略

```csharp
public DataGridFieldId? FieldId { get; set; }
public bool? CanUserSort { get; set; }
public DataGridSortDirections SupportedSortDirections { get; set; }
public DataGridColumnSortState SortState { get; }
```

`FieldId` 标识列对应的查询字段；显示 Binding 只负责单元格内容。TemplateColumn 即使没有 Binding，只要声明 FieldId 也能
参与查询。`CanUserSort=null` 表示继承 DataGrid，最终能力由 grid、column、schema、方向交集和编辑事务共同决定：

```text
effectiveCanSort =
    (column.CanUserSort ?? grid.CanUserSortColumns)
    && column.FieldId is valid
    && source.Schema contains field
    && intersection(column directions, schema directions) is not empty
    && no blocking edit transaction
```

`SortState` 是只读 DirectProperty，包含 nullable Direction 和零基 Priority，由 Query 投影。排序循环集中在纯函数
`DataGridSortPolicy` 中：

| 操作 | Query 结果 |
| --- | --- |
| 普通点击未排序列 | 清除其他 sort，把该列放在优先级 0 并使用首个允许方向。 |
| 普通点击唯一排序列 | 原位置循环方向；循环到 None 后清空 Sorts。 |
| 普通点击多排序中的列 | 只保留该列并循环方向。 |
| Shift 点击未排序列 | 保留现有 sort，把该列追加到末尾。 |
| Shift 点击已排序列 | 原优先级循环；到 None 时删除并压缩后续优先级。 |
| `SetSort(..., Replace)` | 只保留指定字段。 |
| `SetSort(..., AppendOrReplace)` | 原位置替换或追加到末尾。 |
| `direction=null` | 删除指定字段并保持其余字段顺序。 |

通常方向序列为 `None -> Ascending -> Descending -> None`；只支持单方向的字段使用
`None -> AllowedDirection -> None`。排序前先提交当前编辑；提交失败时 Query、事件和请求均不变化。

过滤菜单的 checked state 从 `Query.Filters` 派生，确认时一次性构建新 Query。分组入口遵循相同的单向投影规则。

## 4. Source 协议

### 4.1 Schema

```csharp
public sealed class DataGridSourceSchema
{
    public Type ItemType { get; }
    public ImmutableArray<DataGridFieldSchema> Fields { get; }
    public int PreferredRangeSize { get; }
    public int MaximumRangeSize { get; }
}

public sealed class DataGridFieldSchema
{
    public DataGridFieldId Id { get; }
    public Type ValueType { get; }
    public DataGridSortDirections SortDirections { get; }
    public ImmutableArray<DataGridFilterOperatorSchema> FilterOperators { get; }
    public bool CanGroup { get; }
    public DataGridFieldDisplayAccessor? DisplayAccessor { get; }
}

public sealed class DataGridFieldDisplayAccessor
{
    public Type ItemType { get; }
    public Type ValueType { get; }
    public bool CanWrite { get; }
    public string? PropertyChangedName { get; }

    public static DataGridFieldDisplayAccessor Create<TItem, TValue>(
        Func<TItem, TValue> getter,
        Action<TItem, TValue>? setter = null,
        string? propertyChangedName = null);

    public static DataGridFieldDisplayAccessor FromDataMember<TItem>(
        IDataMemberAccessor accessor);
}

[Flags]
public enum DataGridScalarKinds
{
    None = 0,
    Null = 1 << 0,
    Boolean = 1 << 1,
    SignedInteger = 1 << 2,
    UnsignedInteger = 1 << 3,
    Double = 1 << 4,
    Decimal = 1 << 5,
    String = 1 << 6,
    Guid = 1 << 7,
    DateOnly = 1 << 8,
    TimeOnly = 1 << 9,
    DateTimeOffset = 1 << 10
}

public sealed class DataGridFilterOperatorSchema
{
    public DataGridOperatorId Id { get; }
    public int MinimumValueCount { get; }
    public int MaximumValueCount { get; }
    public DataGridScalarKinds AcceptedKinds { get; }
}
```

Schema 在 Source 生命周期内不可变。`Type` 只作为显式 metadata 和列生成输入，不触发成员扫描。Schema 构造时验证：

- `MaximumRangeSize` 位于 `[32, 4096]`，`PreferredRangeSize` 位于 `[32, MaximumRangeSize]`。
- ItemType、FieldId、operator id 和能力组合合法且无重复。
- `DisplayAccessor` 的 ItemType/ValueType 与 Source/field 精确一致；它是显示层可选能力，不参与 Query 翻译或 Source hot path。
- Filter operator 满足 `0 <= MinimumValueCount <= MaximumValueCount <= 1024`，并明确 accepted scalar kinds。
- display-only field 可以声明不支持 sort、filter 或 group；DataGrid 在请求前拒绝超出 schema 的 Query。

`AutoGenerateColumns=true` 只为带 `DisplayAccessor` 的字段创建列。生成结果使用 compiled binding，不把 FieldId 解析成
CLR path；没有 accessor 的远端字段继续使用显式 Column/Binding。`PropertyChangedName=null` 表示任何
`INotifyPropertyChanged` 通知都刷新该字段；显式名称只监听匹配成员。binding 回收时必须同步解除订阅。

内置 `in` operator 至少包含一个 value；清空全部选择表示删除 filter，而不是生成 `in []`。

### 4.2 Entry identity

`DataGridRowKey` 是支持 string、long、ulong 和 Guid 的固定判别联合，默认值无效。复合 key 由 Source 归一为稳定 string 或
Guid。`DataGridGroupKey` 是独立的非空 ordinal string identity，由完整 group path 生成，不能使用页内序号或 object hash。

```csharp
public enum DataGridSourceEntryKind
{
    Data,
    GroupHeader
}

public sealed class DataGridGroupEntry
{
    public DataGridGroupKey Key { get; }
    public DataGridFieldId Field { get; }
    public DataGridScalar Value { get; }
    public int Level { get; }
    public long LeafCount { get; }
}

public readonly struct DataGridSourceEntry
{
    public DataGridSourceEntryKind Kind { get; }
    public DataGridRowKey RowKey { get; }
    public object? Item { get; }
    public DataGridGroupEntry? Group { get; }
    public int WindowDataIndex { get; }
    public long DataIndex { get; }
}
```

Data entry 具有有效且唯一的 RowKey，item 可以为 null。GroupHeader 具有有效 GroupKey，RowKey 无效，两个 data index 都为
`-1`，且不参与选择或编辑。`LeafCount` 表示当前 PageRequest 内该 group 的业务行数，包含 collapse 后不可见的 descendants，
不表示跨页全局聚合数。

### 4.3 Range、PageRequest 与 snapshot

```csharp
public readonly struct DataGridRange
{
    public int StartIndex { get; }
    public int Count { get; }
}

public readonly struct DataGridPageRequest
{
    public long DataStartIndex { get; }
    public int DataCount { get; }
}

public sealed class DataGridGroupExpansion : IEquatable<DataGridGroupExpansion>
{
    public static DataGridGroupExpansion AllExpanded { get; }
    public ImmutableArray<DataGridGroupKey> CollapsedGroups { get; }

    public DataGridGroupExpansion Collapse(DataGridGroupKey key);
    public DataGridGroupExpansion Expand(DataGridGroupKey key);
}

public readonly struct DataGridSnapshotId : IEquatable<DataGridSnapshotId>
{
    public DataGridSnapshotId(string value);
    public string Value { get; }
}

public readonly struct DataGridFetchRequest
{
    public DataGridQuery Query { get; }
    public DataGridPageRequest? PageRequest { get; }
    public DataGridGroupExpansion GroupExpansion { get; }
    public DataGridRange Range { get; }
    public DataGridSnapshotId? ExpectedSnapshot { get; }
    public long QueryRevision { get; }
    public long DataGeneration { get; }
}

public sealed class DataGridRangeResult
{
    public int StartIndex { get; }
    public ImmutableArray<DataGridSourceEntry> Entries { get; }
    public int TotalEntryCount { get; }
    public int WindowDataCount { get; }
    public long TotalDataCount { get; }
    public DataGridSnapshotId Snapshot { get; }
}
```

DataGridRange 拒绝负 StartIndex 和非正 Count；协调器拒绝 Count 超过 Schema.MaximumRangeSize，且不调用零长度 fetch。
DataGridPageRequest 拒绝负 DataStartIndex 和非正 DataCount。DataGridSnapshotId 拒绝 null、空白和控制字符，并只按 ordinal
比较其不透明值。

Range 使用活动窗口内的扁平 display entry 索引。`PageRequest=null` 表示连续滚动；非 null 时，先在 Query 命中的业务行上切
页，再插入 group header、应用 expansion 并 flatten，最后按 Range 切片。Source 不能把 Range.StartIndex 同时解释为全局
业务索引。

GroupExpansion 使用排序、去重的 immutable array。Groups 结构变化或 Source replacement 时回到 AllExpanded；sort、filter
或 PageRequest 变化可以保留 collapsed keys，Source 忽略当前结果中不存在的 key。

### 4.4 精确结果合同

每个 result 必须是 request range 的精确切片：

```text
expectedCount = min(request.Range.Count, max(0, TotalEntryCount - request.Range.StartIndex))
result.StartIndex == request.Range.StartIndex
result.Entries.Length == expectedCount
0 <= TotalEntryCount
0 <= WindowDataCount
0 <= TotalDataCount
```

PageRequest 非 null 时：

```text
WindowDataCount = min(PageRequest.DataCount, max(0, TotalDataCount - PageRequest.DataStartIndex))
```

PageRequest 为 null 时，`WindowDataCount == TotalDataCount`，因此连续模式要求业务行数可由 int 精确表达。未分组窗口满足
`TotalEntryCount == WindowDataCount`；分组或 collapse 后不能从两个 count 推断固定大小关系。每个 Data entry 满足：

```text
0 <= WindowDataIndex < WindowDataCount
DataIndex = (PageRequest?.DataStartIndex ?? 0) + WindowDataIndex
```

同一活动窗口按 display range 拼接后，Data entry 的 WindowDataIndex 与 DataIndex 严格递增；collapse 可以造成 index 间隙，
不能造成重复或倒序。RowKey 在整个 snapshot 内唯一，GroupKey 在当前活动窗口内唯一。

相同 snapshot 的 TotalDataCount 必须恒定；相同 `(snapshot, PageRequest)` 的 WindowDataCount 必须恒定；相同
`(snapshot, PageRequest, GroupExpansion)` 的 TotalEntryCount 必须恒定。一个 generation 的首次 request 使用
`ExpectedSnapshot=null`；后续 request 携带首次成功结果的 snapshot。Source 必须返回相同 snapshot，或抛出
`DataGridSnapshotExpiredException`。

### 4.5 IDataGridSource

```csharp
public interface IDataGridSource
{
    DataGridSourceSchema Schema { get; }

    ValueTask<DataGridRangeResult> FetchAsync(
        DataGridFetchRequest request,
        CancellationToken cancellationToken);

    event EventHandler? Invalidated;
}
```

FetchAsync 可以同步或异步完成，也可以在任意线程完成。RangeCoordinator 在 Source 返回后验证 result，只有最终 presentation
交换与视觉投影回到 UI 线程；Source 返回不可变集合且不在返回后修改 entry。真正同步完成的 Source 必须保持端到端同步，不能
仅因内部去重任务而被人为转换成下一帧完成。任何改变 membership、row key、total 或顺序的数据变更必须触发 Invalidated，或
通过 mutation capability 返回新 snapshot。

Source 必须把传入的 CancellationToken 作为 I/O、等待、投影循环和结果物化的协作取消边界。DataGrid 的正确性不依赖 Source
遵守取消：迟到结果仍会被 generation、viewport scope 和 request identity 拒绝；但 Source 若忽略已经请求的取消，正在执行的
业务 I/O 无法在有界并发内被强制抢占，因此最终 viewport 的响应时间不能获得同等性能保证。协调器在这种情况下仍保持并发
上限，不通过额外线程或无界并发绕过不协作的 Source。

Source 的连接和集合订阅由 Source 的创建者拥有；DataGrid 不 dispose 外部 Source，只在 attach、replace 和 detach 时对称管理
Invalidated 订阅。Invalidated 可以从任意线程触发，DataGrid 调度回 UI 线程后必须重新核对 Source identity。

## 5. 架构与职责

| Owner | 主要输入 | 输出 | 不负责 |
| --- | --- | --- | --- |
| `DataGridQueryController` | 外部 Query、列手势、schema | 已验证 Query、revision、QueryChanged | 数据读取和容器生成 |
| `DataGridRangeCoordinator` | Query revision、generation、DesiredViewport | viewport scope、block lease、前台/预取调度、取消、pending cache、commit/failure | 业务协议翻译 |
| `DataGridViewportRequestScope` | generation、归一化 DesiredViewport、required blocks | 当前 intent identity、visible/prefetch leases、superseded 状态 | Source result 解释和视觉提交 |
| block request scheduler | active scope leases、cache miss | 最多两个 Source work item；visible 优先，prefetch 可撤销 | UI layout 和容器实现 |
| `IDataGridSource` | Query、PageRequest、Expansion、Range、snapshot | 不可变 RangeResult | DataGrid 视觉状态 |
| `DataGridPresentationSnapshot` | 已验证 block、totals、viewport | AppliedQuery、CommittedViewport、稳定 rows | 未完成请求 |
| `DataGridPresentationIndex` | snapshot、block cache、高度元数据 | slot/entry/offset 映射 | Source I/O |
| `DataGridDisplayData` | committed entries | realized row/group 容器与回收池 | Query 执行 |
| `DataGridRowsPresenter` | committed viewport、显示容器 | 同步 Measure/Arrange 与 child index | 异步调度 |
| `DataGridColumn` | FieldId、schema、Query | SortState 与交互能力 | 查询所有权 |

稳定源码 ownership 按职责组织：

```text
DataGrid.cs                         public contract + lifecycle
DataGrid.Query.cs                   query commit and visual projection
DataGrid.RangeLoading.cs            viewport-to-coordinator integration
DataGrid.Virtualization.cs          desired/committed viewport + container bridge
Data/Query/*                        immutable query values and validation
Data/Source/*                       source/schema/request/result contracts
Data/Source/Local/*                 typed local source and projection
Data/Virtualization/*               viewport scope, request scheduler, block cache, snapshot and height metrics
```

DataGrid 主类型保留 public/protected contract 和生命周期入口；辅助类型只围绕稳定 owner 拆分，不按触发点建立 boolean switch、
ignore flag 或 dispatcher 延时。

## 6. 查询、请求与加载状态

### 6.1 单调代际

DataGrid 使用两个只增不减的 long 代际：

- QueryRevision：Query 结构变化时递增。
- DataGeneration：Source replacement、Reload、Invalidated、snapshot expiry、PageRequest 或 GroupExpansion 变化时递增。

每个 request 捕获 Source identity、Query、revision、generation、PageRequest、GroupExpansion、Range 和 expected snapshot。
提交前逐项比较；任一不匹配即丢弃，不依赖 Source 是否遵守 CancellationToken。

`ViewportIntent` 是 generation 内只增不减的调度 identity，不属于 Query 或 DataGeneration。不同的归一化
`DesiredViewport` 递增 intent 并替换 active viewport scope。归一化目标由合法的 FirstVisibleIndex 与 VisibleCount 标识；同一
目标内的行内 offset 变化只更新滚动投影，重复目标只合并等待者，不建立第二个 scope。ScrollDirection 只调整尚未开始的
prefetch 顺序，不改变 visible identity。ViewportIntent 只决定 range 工作的有效性和提交资格，不改变 Source snapshot identity，
也不清除仍可复用的 committed cache。

### 6.2 Query transition

```text
validate query structure and schema
  -> commit pending edit
  -> structural no-op check
  -> cancel prior generation requests
  -> commit Query and increment revision
  -> reset page; reset expansion when Groups structure changed
  -> project column/header/cell/filter/group state
  -> raise QueryChanged
  -> recheck revision after event reentrancy
  -> load page-0 visible range
```

Query、PageRequest 和 GroupExpansion 的关联更新在一个 UI-thread transition 中完成。Source 为 null 时可以预绑定结构合法的
Query；设置非 null Source 时，在 replacement 提交前验证 schema 与当前 Query/列是否匹配。

### 6.3 Load state

```text
ItemsSource == null                    -> Idle
no applied snapshot + active request  -> Loading
applied snapshot + active request     -> Refreshing
visible target atomically committed   -> Ready
non-cancellation failure              -> Error
```

internal `EffectiveIsOperating` 使用以下确定映射驱动现有 Spin：

```text
EffectiveIsOperating = IsOperating || LoadState == Loading
```

Loading 表示没有可展示的已提交 snapshot。Refreshing 期间旧 snapshot 保持完整不透明度并可只读展示，不自动启动 Spin，
edit/delete/move 仍被禁止。用户显式 `IsOperating=true` 时，任意 LoadState 都继续显示 Spin。

只有请求实际挂起时才发布 Loading/Refreshing。同步 Source 或 cache hit 在当前调用中完成验证与原子提交，直接进入 Ready，
不产生瞬时 operating 状态、Spin 闪烁或额外 Dispatcher turn；异步 Source 的状态机与取消语义保持不变。已有 snapshot 的
异步刷新只发布 Refreshing，不改变自动遮罩状态。

Query、PageRequest、GroupExpansion 或目标 viewport 请求失败且存在已应用快照时，DataGrid 丢弃 pending state，一次性恢复
applied Query、PageRequest、Expansion、CommittedViewport、headers、cells、groups、pagination 和 scroll projection，再暴露
LoadError。只有 Query 实际回退才发出一次 `LoadRollback` QueryChanged；page/group/scroll 回退不伪造 QueryChanged。

初次加载失败时保持空 presentation。Invalidated 刷新失败时保留旧快照、设置 `IsDataStale=true` 并禁用 mutation，等待
Reload。Cancellation 和被替代请求不进入 Error，也不触发回退。

### 6.4 Snapshot expiry

同一 generation 任一 request 抛出 `DataGridSnapshotExpiredException` 时，协调器取消该 generation、清空 pending cache、递增
DataGeneration，并以 null expected snapshot 重启当前目标。一次用户操作最多自动重启一次；再次 expiry 进入 Error。

### 6.5 Source replacement

Source replacement 是事务：

1. 不修改当前状态，先验证候选 schema、当前 Query、列和 PageRequest。
2. 提交当前 edit 并结束 mutation session；失败则旧 Source 与 presentation 保持不变。
3. 一次性提交新 Source identity 和 generation，使旧 continuation 失效。
4. 取消旧请求、退订旧 Source，释放旧 snapshot、cache、key resolution 和 totals。
5. 订阅候选 Source；订阅失败进入可诊断 Error，不复活已经失效的旧 owner。
6. 非 null Source 加载首个可见范围；null Source 进入 Idle 和空 presentation。

### 6.6 Viewport request scope 与优先级

每个有效 generation 同时至多拥有一个 active `DataGridViewportRequestScope`。scope 固定以下状态：

- generation epoch 与单调 ViewportIntent。
- 归一化 DesiredViewport 和可见 block 集合。
- 已取得的 visible block lease，以及 visible commit 后允许的 prefetch lease。
- Active、Superseded 或 Drained 生命周期状态。

不同目标替换时采用 retain-before-release：

1. 在 coordinator gate 内验证 generation，并为新 scope 分配更高 ViewportIntent。
2. 新 scope 先取得 cache hit 或仍有效 inflight block 的 visible lease；prefetch work item 被 visible 使用时原位提升优先级。
3. 原子发布新 active scope，使旧 scope 失去提交资格。
4. 在 gate 外把旧 scope 标记为 Superseded 并释放其剩余 visible/prefetch lease。
5. 排队 work item 失去最后一个 lease 时立即从优先级队列和可发现 inflight 映射移除；执行中 work item 失去最后一个 lease 时立即
   请求 Source cancellation。其 task 只负责终态清理，不能被后续 scope 重新发现或提交。

block work item 以 `(generation epoch, block start)` 去重，但只有未取消且持有有效 lease 的 work item 可复用。新旧 target 重叠
时，新 scope 在旧 scope 释放前接管 lease，因此共享 block 不取消、不重新读取；完全离开新目标的 block 才成为 orphaned work。
orphaned work 即使在 Source 忽略取消后正常返回，也只能执行终态清理；它不能写入 active pending cache、改变错误状态或提交
presentation。后续 scope 再次需要同一 block 时创建新的有效 work item，不能附着到取消已经被请求的 task。

调度器只允许 active scope 的工作进入等待队列，并使用两个优先级：

| 优先级 | 工作 | 调度与取消语义 |
| --- | --- | --- |
| Foreground | generation bootstrap、active scope visible block | 空闲槽优先选择；active scope commit 必须等待全部 visible block。 |
| Background | active committed scope 的前后 prefetch block | 仅在没有 foreground 缺口时选择；新 visible intent 到达即释放旧 prefetch lease。 |

generation 尚无 result identity 时，只允许一个 generation-level bootstrap work item 建立 snapshot。它不是某个中间 viewport 的
可丢弃副作用，不因连续 thumb 输入反复重启；bootstrap 完成后直接为当时最新 active scope 规划目标 block。Query、Source、page、
expansion、reload、invalidation、snapshot expiry 或 detach 使 generation 失效时，bootstrap 与全部 viewport work 一起取消。

caller cancellation 只终止该 caller 对 transition 的等待，不得取消仍被 active scope 或其他 waiter 使用的共享 block。scope
supersede、最后一个 lease 释放和 generation 失效是 Source work item 的取消 owner。由这些 token 引发的
`OperationCanceledException` 归类为 Superseded，不设置 LoadError、不回滚 committed rows，也不改变 Spin/opacity；只有当前
active scope 的 visible 非取消异常能够进入可见失败状态。旧 scope continuation 完成时不能把新 scope 控制的
Loading/Refreshing 改为 Ready；prefetch 非取消异常只进入诊断状态，不改变可见 presentation 或 LoadState。

## 7. Range 虚拟化设计

### 7.1 三个索引域

Source 业务索引和 Avalonia 展示索引使用不同类型与语义：

| 索引域 | 类型 | 含义 |
| --- | --- | --- |
| Source data domain | `long` | Query 命中的全局业务行、PageRequest.DataStartIndex、DataIndex、TotalDataCount |
| Window data domain | `int` | 活动连续窗口或当前页内排除 GroupHeader 的 WindowDataIndex、Row.Index、IChildIndexProvider index/count |
| Display slot domain | `int` | Expansion 后的扁平 Slot、Range.StartIndex、TotalEntryCount、纵向 virtual scroll extent |

`DataGridRow.Index` 取 WindowDataIndex，`DataGridRow.Slot` 取 display slot。连续 data/display window 超过 int 上限时抛出
`DataGridPresentationLimitExceededException`；分页仍可通过 long DataStartIndex 读取 `int.MaxValue` 之后的数据，但单个活动
窗口必须保持精确 int 索引。禁止 saturation、取模、截断或把 window-relative index 伪装成 global child index。

### 7.2 单一视觉虚拟化 owner

现有 `DataGridRowsPresenter`、`DataGridDisplayData` 和 row/group container pool 是唯一视觉虚拟化层。不能在其外再套
ItemsRepeater、VirtualizingStackPanel 或第二个 scroll owner。

`DataGridPresentationIndex` 不创建 TotalEntryCount 长度的 IList、placeholder object 或 null row，只保存 totals、当前
snapshot/generation、有限 RangeBlock cache、CommittedViewport、稀疏高度信息和必要 key/index metadata。Slot 直接对应 Source
已按 expansion 处理后的扁平 display index，因此前后 slot 是 O(1) 的 `slot +/- 1`。

RowsPresenter.Children 只包含当前可见、edit-pinned 或 drag-pinned control。`IChildIndexProvider` 的 count 精确等于
WindowDataCount，row child index 精确等于 WindowDataIndex；GroupHeader 不冒充 data child，也不因 total count 生成视觉 child。

### 7.3 DesiredViewport 与 CommittedViewport

虚拟化分为可同步或异步完成的数据规划，以及始终同步的布局两个阶段：

```text
scroll / thumb / keyboard / bring-into-view intent
  -> coalesced DesiredViewport, latest wins
  -> active ViewportRequestScope replace or no-op merge
  -> pure ViewportPlanner: offset -> slot -> required range
  -> retain shared block leases; cancel orphaned visible/prefetch work
  -> foreground RangeCoordinator ensures cache coverage (sync hit or async miss)
  -> UI-thread atomic CommittedViewport swap
  -> one layout pass realizes committed cached entries only
```

缓存命中时可以在同一 UI turn 提交；缓存缺失时保留旧 CommittedViewport 和旧 rows。连续滚轮、惯性或 thumb 输入可以跨多个
Dispatcher turn 产生 intent，但 coordinator 只保留最新 scope 的目标工作：仍被最新目标使用的 block 保留并去重，离开目标的
排队或执行工作立即失效并协作取消。最终 target 不得等待已经排队但尚未进入 Source 的旧 viewport work。Measure、Arrange、
container prepare/recycle 只访问已经验证并 pin 的 block。缺失 committed block 是内部不变量错误，不能返回 null row 或在布局中
补发同步 fetch。

远端等待期间，DataGrid 继续接收并合并滚动意图。目标失败时恢复最后成功的 offset/slot；界面不能出现
闪白 Reset、重复 row 或缺失 row。

### 7.4 Block、预取与 pin

```text
blockSize = clamp(source.Schema.PreferredRangeSize, 32, source.Schema.MaximumRangeSize)
visibleRange = ViewportPlanner 的实际视口覆盖
prefetchRange = visibleRange + one viewport before + one viewport after
requestRange = block-aligned missing intervals inside prefetchRange
```

初次布局从 0 请求一个 bootstrap block，取得 snapshot 后再把请求并发提升到默认上限 2。显式有限 Height 可以在第一次请求时
估算 aligned visible target；presentation 在 RowsPresenter 尚未得到有效可用高度时只提交数据，不提前创建整段容器。首次布局
只实现真正填满 viewport 的 rows。自动高度且 Bounds 仍为 0 的嵌套 DataGrid 在模板 part 就绪后实现一个已提交 bootstrap entry，
打破“无 row 则 DesiredSize 为 0、DesiredSize 为 0 则无 row”的循环，但绝不从 Measure 发起 Source 请求。visible 缺口优先，
随后按滚动方向预取。相同 revision/generation/block 在有效 lease 存续期间最多有一个请求；快速滚动先转移重叠 block lease，
再取消完全离开最新 visible target 的请求。

UI commit 只等待 visibleRange，prefetch 不改变 LoadState、不触发布局、不创建 control，也不阻塞 visible commit。缓存采用有界
LRU。prefetch 只属于产生它的 committed viewport scope；新的 visible intent 到达时，旧 scope 的 queued prefetch 立即移除，
active prefetch 收到协作取消，后续空闲槽先交给 visible 缺口。支撑 realized、edit、drag 或 applied snapshot 的 block 必须 pin；
其 owner 释放前不能 eviction，其余 block 按 LRU 回收。

### 7.5 高度、extent 与滚动锚点

固定行高使用 `offset / rowHeight` 和 `slot * rowHeight`，保持 O(1)。自动行高、GroupHeader 和 RowDetails 使用：

```text
estimatedOffset(slot) = slot * defaultEstimate + sparsePrefixDelta(slot)
```

`SparseHeightDeltaIndex` 是按 block/slot 排序的增广树，只保存 pinned/current-LRU block 内已测 entry 相对默认估值的 delta。
passive block eviction 时删除对应明细，并把样本吸收到按 entry kind/group level 划分的常量大小 HeightEstimator。prefix sum 和
offset-to-slot 查询为 O(log M)，M 受 cache/pin 与用户显式状态上限约束。

自动 `RowHeightEstimate` 属于已提交的 data generation，不能跨 Source replacement、Query/filter、PageRequest 或 Invalidated/Reset
复用。只有新 generation 的 presentation 完成校验并进入原子提交时，才把自动估值重置为 `DefaultRowHeight` 并清空旧的采样进度；
pending 或失败的 generation 不得改变当前 presentation 的估值。新 generation 的首个有效 data-row 实测值重新建立 baseline，并
以该 baseline rebase 新的稀疏高度索引；显式有限 `RowHeight` 始终优先，不参与自动重采样。该策略每个 generation 只增加 O(1)
状态重置，单行测量仍只写一个 O(log M) 稀疏差值，不扫描总行数或历史行。

不能从 slot 0 扫描到 FirstScrollingSlot，也不能按 TotalEntryCount 或所有历史访问行创建 Fenwick array。声明式全部
RowDetails 可见通过 count × estimate 进入基础 extent；只有已测差值和显式单行 override 进入稀疏索引。

估值被真实测量修正时，以首个完整可见 entry key 和 intra-row offset 为锚，保持内容位置。所有 height、extent、Maximum、
ViewportSize 和 offset 运算保持 finite、非负并做 checked/clamped 边界处理。通用 ScrollBar 对有限 Maximum 保持
`Maximum >= Minimum`；小于一个物理像素的有效 travel range 按 `(Maximum - Minimum) * scale` 判断并收敛到 Minimum，不能用负
Maximum 表示“无需滚动”。

新 Query 或 PageRequest 成功后垂直 offset 归零；GroupExpansion 改变时保持操作 group header 的屏幕 Y；Invalidated 刷新优先按
首行 key 恢复，无法解析时落到最近合法 slot。

### 7.6 原子提交与容器回收

目标 range block 全部到齐并通过合同校验后，DataGrid 先计算合法 page/scroll bounds。若 total 变化导致目标越界，丢弃 pending
presentation、递增 generation 并只请求最终合法窗口，不能提交瞬时空页。

无需 clamp 时，UI 线程一次性 pin 新 block、交换 applied snapshot 与 CommittedViewport、更新 totals 和 AppliedQuery、回收离开
范围的容器、生成新范围容器、恢复 selection/current/edit/details 状态、更新 extent/offset/scrollbar，并把 LoadState 置为
Ready。旧 block 在旧容器全部 detach 后才解除 pin。

容器回收必须清理 DataContext、RowKey、DataIndex、WindowDataIndex、Slot、group metadata、selection/current/edit/hover、
RowDetails、列 cell 状态和事件订阅。预取不能 prepare control。

### 7.7 分组、分页与滚动链

Source 使用固定顺序形成 display entries：

```text
filter
  -> stable Groups then Sorts
  -> PageRequest over business rows
  -> insert GroupHeader entries
  -> apply GroupExpansion
  -> flatten
  -> Range over visible entries
```

Source 直接省略 collapsed descendants，DataGrid 不为完整数据建立全局 group-header 或 collapsed-slot table。分组后的 data indices
仍严格递增。

分页总数来自 `TotalDataCount`。PageRequest.DataStartIndex 使用 checked long arithmetic，DataCount 为 int；当前页只允许一个活动
presentation。翻页成功后回到顶部，顶部和底部 Pagination 从同一 applied state 投影，不拥有查询或 totals。

水平列虚拟化、冻结列和 `DataGridCellsPresenter` 的可见列计算保持原 owner。纵向 range commit 不能全量重建 cells。嵌套滚动中，
DataGrid 在内部 offset 未到边界时消费垂直输入，到达边界后把滚动链交给外层 ScrollViewer；不能通过移除高度、overflow 或滚动条
规避这一合同。

## 8. 本地 Source

`DataGridLocalSource<T>` 使用 typed descriptor 描述字段：

```text
DataGridLocalField<T, TValue>
  FieldId
  Func<T, TValue> Getter
  IComparer<TValue> SortComparer
  typed filter evaluators by OperatorId
  scalar converter
```

同一 DataGridQuery 与远端 Source 共享语义，本地投影执行顺序为：

1. 捕获稳定 source snapshot 和 sourceVersion。
2. 在 int index buffer 上执行 filter，不复制业务对象。
3. 按 Groups、Sorts 和原 source ordinal 组成的 comparer 排序 index。
4. PageRequest 切业务行；只在分组路径创建必要 group directory。
5. Range fetch 通过 index 读取原 item 并创建小型 immutable entry buffer。

`DataGridQuery.Empty`、无分页且无分组时使用直接 range fast path：只读取请求区间并生成对应 entry，不创建或缓存全量 index
projection。存在 filter、sort、group 或分页语义时才构建稳定投影；这些 O(N)/O(N log N) 工作仍在后台执行。

原 source ordinal 是最终 tie-break，因此即使底层 sort 不稳定，输出仍有确定顺序。projection cache 使用 sourceVersion 与 Query
作为 identity，并与 page/expansion 派生索引一起受有限 LRU 管理。

可变 UI 集合先在 owner 线程捕获一次引用快照，后台只处理稳定快照。immutable thread-safe list 可以跳过引用快照。所有捕获、
filter、sort 和 group 操作支持 CancellationToken；比较循环不执行 LINQ、反射、字符串 path 解析、文化对象创建或 delegate
组合分配。

## 9. 选择、current 与 mutation

### 9.1 声明式选择

`DataGridSelectionState` 是不可变、规范化的选择 owner：

- `ExplicitKeys`：逐行选择的稳定 RowKey。
- `AllMatchingQuery`：覆盖当前 applied filter membership 的全量选择标记，不保存逐行 key。
- `IndexIntervals`：绑定 Source identity、AppliedQuery 和 snapshot 的不相交全局 DataIndex 区间。
- `ExcludedKeys`：从 select-all 或 interval 中逐行取消的例外。

已加载 entry 的选择判定为：

```text
(AllMatchingQuery matches || key in ExplicitKeys || DataIndex in scoped interval)
  && key not in ExcludedKeys
```

Extended 模式下 Header checkbox 和 Ctrl+A 表示当前 AppliedQuery 命中的全部业务行，包括未加载页；它们不获取全部 key。
Shift 范围选择生成规范化 interval，单行 toggle 只修改 key exception。Single 模式只允许一个 ExplicitKey。

失效规则为：

| 变化 | Selection 处理 |
| --- | --- |
| PageRequest、GroupExpansion | 完整保留。 |
| 仅 Sorts 或 Groups | 保留 AllMatchingQuery 与 ExplicitKeys，清理依赖旧顺序的 intervals。 |
| Filters | 清理 AllMatchingQuery、intervals 和 ExcludedKeys；ExplicitKeys 保留，只有同 key 再出现时投影。 |
| Source.Invalidated | 清理普通 intervals；保留 AllMatchingQuery 与 ExplicitKeys。 |
| Source replacement | 清空 Selection 与 CurrentRowKey。 |

selection 的 Query 相关变换只在新 presentation 成功提交时发生；刷新失败回滚不丢失 applied selection。SelectionChanged 描述
OldSelection/NewSelection 的声明式 delta，不为了生成 item 列表而拉取未加载数据。

### 9.2 Current row

Current row 以 `CurrentRowKey` 和 internal DataIndex hint 表达；key 是 identity，hint 只用于定位优化。可选
`IDataGridKeyLookupSource` 可以把 cache 外 key 解析到当前 Query。没有该能力时保留 key，但不伪造未加载 SelectedItem 或 index。

### 9.3 Mutation capability

读 Source 不被迫实现写能力；能力通过独立接口表达：

```text
IDataGridEditableSource       commit/cancel/add/delete by row key + snapshot
IDataGridMovableSource        move row key relative to before/after key + snapshot
IDataGridKeyLookupSource      resolve selected/current key outside cache
IDataGridBulkSelectionSource  operate on declarative selection outside cache
```

Mutation request 携带 Query、snapshot 和 row key，成功后返回新 snapshot 或触发 Invalidated。行移动使用 source key 与目标邻接
key，不把 view index 当作 Source index。Source 不支持移动或 Query 语义不允许移动时，reorder handle 不进入可提交状态。

用户触发 Query、PageRequest、GroupExpansion 或 Source replacement 前必须提交当前 edit；失败则中止操作。Invalidated 是外部
事实，DataGrid 立即取消 edit/mutation session、把 snapshot 标为 stale 并刷新，不能向已失效 snapshot 提交修改。

## 10. Template 与视觉集成

### 10.1 模板结构

Query/Range Source 复用现有 `DataGridTheme` 结构、RowsPresenter、上下 Pagination、scrollbars、empty state 和 Spin。不增加第二套
rows presenter、overlay 或 scroll owner。`PART_TopPagination` 与 `PART_BottomPagination` 只投影 applied page state；
`PART_RowPresenter` 只承载 committed containers。

`EffectiveIsOperating = IsOperating || LoadState == Loading`。首次异步加载通过 Spin 表达；刷新继续使用同一 Frame、Header 与
scroll geometry，并保持已提交内容完全不透明。Error 通过 LoadError 暴露，不自动创建 popup。

### 10.2 排序与过滤视觉

Query 提交后，DataGrid 建立 FieldId 到 `(direction, priority)` 的小型映射，只更新旧/新 Query 涉及的列：

```text
Query.Sorts
  -> DataGridColumn.SortState
  -> Header.CurrentSortingState
  -> :sort-ascending / :sort-descending
  -> existing DataGridSortIndicator template
```

多排序 priority 进入 automation 与 tooltip 语义，不增加默认可见编号。DataGridCell 不长期订阅 Header sort state；prepare/recycle
直接从 OwningColumn.SortState 初始化 `IsSorting`，Query delta 只更新已实现 cells。

过滤候选内容仍由列过滤入口展示，但选中态和激活态只从 Query.Filters 派生。Header、Indicator 和 Flyout replacement 延续各自
既有 attach/detach、popup pin 与 TopLevel 生命周期，不因异步数据管线建立第二份过滤 owner。

### 10.3 视觉优先级

- selected row 背景高于 BodySortBg。
- 未选中 sorted cell 继续使用 BodySortBg。
- hover、focus、disabled、empty、Title/Footer、上下分页、固定/自动行高和滚动条保持既有 selector 优先级。
- Loading 只允许 Spin 区域出现状态差异；Refreshing 不自动启动 Spin，已提交内容的 opacity、geometry 和命中区域保持稳定；
  Ready 状态不接受未审定 pixel diff。

## 11. 生命周期与合同错误

DataGrid 在 commit 前验证 Source、revision、generation、range、snapshot、total、entry payload、index mapping 和 key uniqueness。
违反协议时抛出或包装为 `DataGridSourceContractException`，进入 Error，且不部分提交。

连续模式无法用 int 精确表达活动数据或 display count 时，Source 抛出
`DataGridPresentationLimitExceededException`。LoadError 必须给出启用分页或缩小 Query 的可操作诊断；不能截断 total 或允许
int overflow。

生命周期配对如下：

| 获取 | Owner | 释放或失效 |
| --- | --- | --- |
| `Source.Invalidated +=` | DataGrid attachment | Source replace、detach；reattach 时重新订阅 |
| generation CTS | RangeCoordinator | Query/generation replace、detach、完成后 Dispose |
| active viewport request scope | RangeCoordinator | 不同 DesiredViewport、generation replacement 或 detach；retain shared lease 后 supersede |
| visible/prefetch block lease | viewport request scope | scope supersede、prefetch 被新 visible intent 撤销、generation replacement 或 detach |
| block work item CTS | request scheduler | 最后一个 lease 释放或 generation 失效时 Cancel；Source task 进入终态后 Dispose |
| Source request CancellationToken | block work item | generation 或 work item 取消；单个 caller 结束等待不取消仍共享的工作 |
| pending blocks | pending snapshot | commit、failure、query replace、detach |
| applied blocks | applied snapshot | snapshot replace、Source replace、detach |
| pooled entry/index arrays | LocalSource/cache block | projection/block eviction 或 Source dispose |
| template part references | DataGrid | 下一次 OnApplyTemplate 开始、detach |

Detach 先失效 revision/generation owner，再取消请求、退订 Source、释放 cache 和 part。迟到 continuation 看到失效 identity 后直接
结束，不能重新接入旧 presentation。

## 12. 性能与 AOT 边界

| 指标 | 设计门槛 |
| --- | --- |
| 初始远端读取 | 不超过首个 aligned visible target，不读取全部数据。 |
| 有效 block 去重 | 同一 revision/generation/block 同时只有一个可发现且持有有效 lease 的 work item；共享目标不重复请求。 |
| 最大请求并发 | 默认不超过 2。 |
| 活动 viewport scope | 每个 generation 最多 1 个。 |
| 过时排队 viewport work | 0；scope supersede 时移出可发现队列并取消。 |
| 最终 viewport 排队 | 不等待尚未进入 Source 的旧 intent；协作 Source 下只受当前 visible fetch wave 与取消收敛开销影响。 |
| Prefetch | 低于 visible 优先级；新 visible intent 到达后旧 prefetch 不再获得调度资格。 |
| 迟到结果提交 | 0。 |
| Cache 大小 | 只与 block/viewport/pin 上限相关，与 TotalDataCount 无关。 |
| Per-cell sort subscription | 0。 |
| Query no-op | 0 请求、0 全行刷新。 |
| Ready 视觉节点 | 不增加。 |
| 每次自动 Refreshing 遮罩过渡 | 0。 |
| Measure/Arrange Source 调用 | 0 fetch、0 wait、0 range-buffer allocation。 |
| Realized controls | 不超过 visible + 一个 editing row + 一个 drag row；prefetch 不创建 control。 |
| 大跨度 offset 定位 | 固定行高 O(1)，可变行高 O(log M)，不从 slot 0 扫描。 |

LocalSource 的 O(N) filter 和 O(N log N) sort 在稳定快照的后台投影上执行，不阻塞 UI thread。时间性能采用重复基线、mean、
median、P95 与 allocation 共同判断；Ready local 场景的视觉结构、订阅数和稳态 allocation 不允许增加。

最终 viewport 延迟不能随一次拖动产生的中间 intent 数线性增长。对遵守 CancellationToken 的固定延迟 Source，最终 target 的
cache miss 延迟由当前 visible fetch wave、取消收敛和一次 UI commit 构成；历史 intent 只允许留下已经进入 Source 且正在响应
取消的有界 active work，不能留下无界 semaphore waiter、continuation 或 prefetch backlog。忽略取消的 Source 仍受最大并发和
迟到提交保护，但其不可抢占 I/O 不计入 DataGrid 可保证的低延迟范围。

所有 Query/Source 类型位于 DataGrid package，不向 Core 或 Shared 引入反向依赖。自动列仅消费 schema 中显式的强类型
`DataGridFieldDisplayAccessor`，可通过 generated `IDataMemberAccessor` 适配，并始终生成 compiled binding；不新增 linker root、
trimming suppression、runtime registration、字符串 Binding 或反射 fallback。

## 13. 兼容性与定制边界

### 13.1 稳定的控件与主题契约

- DataGrid 的 ControlTheme key、Template Part、伪类、Token、Semantic Part 和默认 ready geometry 保持稳定。
- 列宽、冻结列、水平虚拟化、RowDetails、输入命中和嵌套滚动由原 owner 继续维护。
- 自定义模板必须保留 RowsPresenter、scroll owner、Spin 和 Pagination 的语义职责；不能引入并行 virtualizer。
- 自定义 Source 必须严格实现 schema、snapshot、精确切片、key 唯一性和 invalidation 契约。

### 13.2 数据 API 边界

- `ItemsSource : IDataGridSource?` 是唯一数据入口；不存在 `Source` 别名或旧 `IEnumerable` 数据通路。`Query` 是唯一 query owner，`Selection` 是唯一选择 owner。
- Source 不应因本地或远端身份改变 DataGrid 行为；能力只通过 schema 和可选 mutation/lookup/bulk interfaces 暴露。
- 字段绑定、显示模板和业务协议互相独立，FieldId 不等于 CLR path。
- 未加载对象不能通过伪造完整 item collection、placeholder row 或仅处理 cache 的方式冒充全量语义。

### 13.3 平台与宿主

Desktop 与 Browser host 使用相同 Query、Source、snapshot 和虚拟化不变量。平台层可以影响调度时延、pointer/scroll 输入来源和
渲染后端，但不能改变 Query equality、index domain、range result contract、selection identity 或 Ready 视觉语义。

## 14. 验证要求

### 14.1 纯逻辑与 Source 合同

- Query 的归一、validation、equality/hash、no-op、sort policy 和 QueryChanged 重入。
- Schema field/direction/operator/group 能力在 request 前验证。
- 空数据、短尾 range、int 边界、精确 result count、snapshot/totals 恒定和 key 分域唯一。
- PageRequest、display range 与 data index 不混用；group/collapse 后的 index 映射严格递增。
- LocalSource 的稳定 tie-break、typed comparer、组合 query、collection invalidation、取消和 AOT 路径。

### 14.2 并发与失败

- 慢旧 Query、慢旧 Source、忽略 cancellation 的 Source 均不能覆盖新 presentation。
- 相邻或重叠 viewport 先转移共享 block lease，完全离开目标的 block 取消且不会被新 scope 附着到已取消 task。
- 两个旧请求占用或等待并发槽时，新的最终 visible target 会使 orphaned work 收到取消，并在协作 Source 释放槽后优先开始；不需要
  手工完成旧请求才能推进最终 target。
- prefetch 排队或执行时到达新 visible intent，旧 prefetch 失去 lease，新的 visible block 必须先于任何剩余 background work。
- caller cancellation 不取消仍被 active scope 共享的 block；scope/generation cancellation 后 active、queued、inflight 和 CTS
  均回到稳定值。snapshot expiry 只自动重启一次。
- Query/page/group/scroll 失败完整回退；initial error 与 stale refresh error 语义分离。
- Source replace、detach/reattach 的订阅、CTS、cache 和 continuation 全部对称释放。

### 14.3 虚拟化与交互

- SlotCount、WindowDataCount、DataIndex、Row.Index 与 `IChildIndexProvider` 在 0、1、短尾、group 和 int 边界上精确。
- Source probe 证明 Measure、Arrange、prepare 和 recycle 内没有 fetch 或同步等待。
- fixed/variable height 的 offset-to-slot、extent、anchor、RowDetails 和 group collapse 保持稳定。
- 滚轮、惯性、scrollbar line/page、thumb、Home/End/PageUp/PageDown 和 ScrollIntoView 跨多个 Dispatcher turn 仍只提交最终有效
  viewport；中间 intent 不形成排队积压。
- Cache miss 保留旧 rows；成功一次交换，失败恢复旧 offset/slot，无 null、重复或闪白 row。
- block pin、container reset、回收池和高度元数据在往返滚动 10,000 次后不增长。
- 冻结列、水平虚拟化、row details 和 nested scroll chaining 保持正确。
- 既有 `DataGridDetailExpanderColumnRecycleTests` 中的 offset、extent、首行余量、双向回收和 RowDetails 状态用例保持或加强，
  不能删除或放宽断言。

### 14.4 视觉、性能与发布门禁

- Light/Dark、全部 SizeType、普通/分组表头、empty/data、Title/Footer、上下分页、固定/自动行高、冻结列、滚动条和
  RowDetails 通过基线比较。
- sorted selected、sorted unselected、hover、focus、disabled、Loading、Refreshing 和 rollback 视觉满足第 10 节优先级；排序触发的
  Refreshing 必须保持旧 rows 完全可见且不启动 Spin。
- Sort indicator 左/中/右命中与 column resize 边界分别验证，排序手势不能吞掉 resize。
- Browser/Desktop 各验证 local Source 与可控 fake-remote Source，Ready 状态的非预期 pixel diff 直接判定失败。
- 100 万本地行与逻辑远端行覆盖 sort、首屏、连续滚动、快速跳转、offset-to-slot、cache、并发和 allocation。
- 性能基线使用同一提交至少 10 次重复测量并预先冻结噪声区间与 non-inferiority 门槛。
- DataGrid 专用测试、完整 solution tests、DataGrid performance state verifier、AOT/trim verification、Gallery
  NativeAOT publish/startup smoke 和 `git diff --check` 全部通过。

完整验证命令至少包括：

```bash
dotnet test AtomUI.slnx -c Release --no-restore /m:1 /nr:false --nologo -v:minimal
dotnet run --project tools/performances/AtomUI.DataGridPerformance/AtomUI.DataGridPerformance.csproj \
  -c Release --framework net10.0 --no-build -- --verify-states
python3 tests/AtomUI.Registration.Fixtures/run_package_consumers.py --output /tmp/atomui-datagrid-registration --rid osx-arm64 --serve
pwsh -NoLogo -NoProfile -File controlgallery/AtomUIGallery.Desktop/scripts/PublishToLocal.ps1 \
  -publishRootPath /tmp/atomui-datagrid-query-aot \
  -runtime osx-arm64 -buildType Release -publishAot true
git diff --check
```
