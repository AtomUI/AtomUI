# DataGrid 范围数据源行内编辑设计

日期：2026-10-31
范围：`src/AtomUI.Desktop.Controls.DataGrid`
关联：`2026-09-05-datagrid-immutable-query-range-source-design.md` §12.2

## 1. 目标

范围数据源（`IDataGridSource`）接管 DataGrid 后，`BeginEdit` 在范围模式下直接返回 false，`BeginRowEdit`/`CommitRowEdit`
是空壳，操作列的“编辑/保存/取消”只切换按钮状态。本设计补齐两种行内编辑：

- 单元格编辑：F2、双击/再次单击、Tab 进入编辑，每个单元格提交时立即写回数据源。
- 行编辑：操作列“编辑”使整行可编辑单元格同时进入编辑，“保存”一次性提交整行，“取消”丢弃。

所有写入统一走 `IDataGridEditableSource.CommitAsync`，编辑器不得直接写数据项。

## 2. 已定决策

| 问题 | 决策 | 理由 |
| --- | --- | --- |
| 字段是否可编辑 | `DataGridFieldSchema.IsEditable`（构造参数 `isEditable`，默认 false） | 可编辑性是 Source 能力，必须在进入编辑前可知，而不是提交时才失败 |
| 列是否可编辑 | 列未只读、绑定 `DataGridBoundColumn`（有 `Binding` 与 `BindingTarget`）、`FieldId` 指向 `IsEditable` 字段、Source 实现 `IDataGridEditableSource` | 统一收口到 `GetColumnEffectiveReadOnlyState`，F2/指针/Tab/`BeginEdit` 全部经过同一判断 |
| 模板列 | 范围模式下视为只读 | 模板内绑定无法经数据源合同提交 |
| 编辑器取值 | 编辑器使用列绑定的 OneWay 副本；提交时读取 `BindingTarget` 当前值 | TwoWay 会绕过数据源直接写对象；远端 Source 的数据项是快照缓存 |
| 值转换 | 列绑定的 `Converter.ConvertBack`（缺省 `DataGridValueConverter`）转换到字段 `ValueType`，再转成 `DataGridScalar` | 与显示绑定使用同一转换规则；不可转换、不可空字段为空均作为校验错误 |
| 转换失败 | 编辑器设置 `DataValidationErrors`，单元格/行标记 invalid，保持编辑态，`CommitEdit` 返回 false | 与原有校验语义一致，不丢用户输入 |
| 值未变化 | 字段有 `DisplayAccessor` 且当前值等于新值时不发请求 | 避免无意义提交与重载 |
| 同步 `CommitEdit()` 与异步 `CommitAsync` | 本地转换成功后立即退出编辑（乐观退出），异步提交由 grid 观察 | `CommitEdit()` 是同步公共合同；Query/Page/滚动前都依赖它立即完成 |
| 异步提交失败 | 新增 `DataGrid.EditCommitFailed` 事件（RowKey、Values、Exception） | 乐观退出后失败必须可观察，不能静默吞掉；Source 合同本身只用异常表达失败 |
| 提交后刷新 | `DataGridMutationResult.Snapshot` 等于请求 snapshot 表示“当前呈现仍然有效”：不重载，仅重建该行单元格显示内容；不同 snapshot 或 null 按原规则重载 | 每格提交都重载会在 Tab 连续编辑时取消下一格；同 snapshot 重载本身也无意义 |
| 行编辑与当前行 | 行编辑中切换到其他行时按原有语义提交当前行编辑；Esc 取消、Enter 提交 | 保持 `EditingRow.Slot == CurrentSlot` 不变式，与单元格编辑一致 |
| 操作列按钮态 | `DataGridRow.IsEditingMode` 只在整行编辑时为 true | 单元格编辑不应让操作列显示“保存/取消” |

## 3. 数据源合同

### 3.1 `DataGridFieldSchema`

新增 `IsEditable`。`DataGridEditRequest.Values` 只能包含 `IsEditable` 字段。

### 3.2 `DataGridLocalSource<T>`

`DataGridLocalSourceDescriptor<T>.Field` 新增可选 `setter` 与 `fromScalar`。提供 `setter` 的字段 `IsEditable = true`；
`fromScalar` 缺省按 `DataGridScalar` 类型做精确转换（整数范围检查、可空类型、`Null` 只能写入可空/引用类型）。

`DataGridLocalSource<T>` 实现 `IDataGridEditableSource`：

- `CommitAsync`：校验 snapshot 未过期、行存在、字段可编辑；先转换全部值，任一失败抛 `ArgumentException`
  且不写入任何字段（all-or-nothing），再依次调用 setter。
  - 编辑字段出现在请求 Query 的 sort/filter/group 中，或行键因编辑改变：推进版本并发出 `Invalidated`，返回新 snapshot。
  - 否则当前投影顺序与成员不变：清空投影缓存（其他 Query 可能依赖该字段），保留 snapshot，返回请求 snapshot。
- `CancelAsync`：无缓冲，返回当前 snapshot。
- `DeleteAsync`：要求可写 `IList<T>`，否则 `NotSupportedException`；按行键删除，返回新 snapshot。
- `AddAsync`：`NotSupportedException`，本地 Source 无法构造 `T`，新增数据请直接操作底层集合。

## 4. DataGrid 流程

### 4.1 单元格编辑

1. `BeginCellEdit`：列有效只读或当前槽位不是数据行时返回 false；`BeginRowEdit` 在范围模式下记录 `EditingRow`。
2. 编辑器生成：`DataGridBoundColumn` 使用 OneWay 绑定副本（支持 `CompiledBinding` 与 `ReflectionBinding`，其他绑定类型列视为只读）。
3. `EndCellEdit(Commit)`：转换失败 → 校验错误、保持编辑；成功 → 退出编辑并发起 `CommitAsync`（值变化时）。
4. `EndCellEdit(Cancel)`：编辑器从未写源，直接恢复显示元素。

### 4.2 行编辑（操作列）

1. “编辑”：提交当前编辑；设当前单元格到该行，`EditingRow = row`，整行编辑标记为 true，为所有可编辑单元格生成编辑器。
2. “保存”/Enter：逐格转换，任一失败则标记并保持编辑；全部成功后合并变化字段为一次 `CommitAsync`，恢复显示。
3. “取消”/Esc：恢复全部显示元素，不访问 Source。
4. 整行编辑中单击该行单元格不再进入单元格编辑（编辑器已在位）。

### 4.3 生命周期

- 编辑行通过 `IsRowRecyclable` 排除回收；滚动、Query、Page、GroupExpansion、Source 替换前提交编辑（既有行为）。
- `Source.Invalidated` 取消编辑（既有行为，§12.2）。
- 异步提交使用 generation 取消令牌；generation 结束导致的取消不报告失败。

## 5. 公共 API 变化

- `DataGridFieldSchema` 构造函数新增 `bool isEditable = false`，新增 `IsEditable` 属性。
- `DataGridLocalField<T,TValue>` 构造函数新增 `setter`、`fromScalar`，新增 `Setter`、`FromScalar` 属性。
- `DataGridLocalSourceDescriptor<T>.Field` 新增 `setter`、`fromScalar` 可选参数。
- `DataGridLocalSource<T>` 实现 `IDataGridEditableSource`。
- `DataGrid.EditCommitFailed` 事件与 `DataGridEditCommitFailedEventArgs`。
- `DataGridOperationColumn` 由 internal 改为 public sealed（原先无法在 XAML 中声明，整行编辑没有入口）。
- 行为：`BeginEdit`/F2 以当前单元格为前提，不再要求行被选中（`SelectionMode` 默认 `None`，选择与当前项相互独立）。
- 修复：`EditingColumnIndex` 改为读取 `_editingColumnIndex`，原自动属性从未赋值、恒为 0。
- 行为：`BeginEdit` 在范围模式下对可编辑列生效；返回请求 snapshot 的 mutation 不再触发重载。

## 6. 不在本次范围

- 新增行的行内编辑（`AddAsync` 的 UI 流程）。
- 模板列经数据源提交。
- 多 grid 共享同一本地 Source 且按被编辑字段排序时的跨 grid 刷新（未影响当前 Query 的编辑不发 `Invalidated`）。

## 7. 验证

- 临时测试放 `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Temporary/InlineEditing/`，完成后删除，全局测试净增 0。
- 模块验证：`python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls.DataGrid --path tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 可编辑单元格/可编辑行示例视觉验收。
