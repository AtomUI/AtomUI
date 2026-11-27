# DataGrid 行拖拽关联问题调查：#522 / #523 / #524

日期：2026-11-01。首轮范围：读取反馈、构建项目外临时 Demo、通过框架输入脚本尝试复现并分析；首轮不修改生产代码。
用户随后明确要求修复已确认的视口跳动，修复阶段记录见文末。

## 原始证据与验收契约

- [#522](https://github.com/AtomUI/AtomUI/issues/522)：行拖到首行后看不到更新，上拉不恢复，下拉才恢复。已下载并检查原视频关键帧。
- [#523](https://github.com/AtomUI/AtomUI/issues/523)：行拖到可见末行外后取消，原行消失，上拉不恢复，下拉才恢复。已下载并检查原视频关键帧。
- [#524](https://github.com/AtomUI/AtomUI/issues/524)：同一行向末尾、行首拖拽后出现两类 Debug 断言；反馈者无法稳定复现，未提供录屏。
- 用户明确将三个 issue 作为同一问题调查。本轮沿共同的「拖拽 → 自动滚动/取消或提交 → 范围呈现 → 指针再次进入」链路分析，分别记录观察结果。

| AC | 来源、前置与操作 | 独立预期及观测 |
| --- | --- | --- |
| AC-01 首行放置 | #522；30 行本地可移动源，滚动位置为顶端；将第 6 行拖到首行 | 排序生效，目标行立即出现在首位；检查集合顺序、首个显示 slot、offset、行文本与实际截图 |
| AC-02 可见末行放置 | 同一拖拽契约；分别将第 1/2 行拖到当前可见末行，指针不越过视口 | 不发生无输入依据的视口跳动；对比首行是否参与重排与实际 offset |
| AC-03 越界取消 | #523；拖第 1 行越过底部，继续发送移动直至自动滚动和取消 | 取消后数据不变，捕获与拖拽视觉清理；原行可通过返回顶端重新显示 |
| AC-04 原始恢复动作 | #522/#523 视频中的滚动条操作 | 真实拖动滑块至顶端/向下/再返回，并独立记录几何命中、offset、显示 slot 和截图；滚轮另作对照 |
| AC-05 生命周期 | #524；同一实例、同一数据项往返拖拽，补充越界取消及放手后立即移动指针 | 不发生 OwningGrid 为空或滚动偏移不一致断言；检查实际事件、容器所有者和最终状态 |
| AC-UX 布局保留 | 原视频及 Gallery `DragRowDataGrid2` 源码 | 保留 Height=400、表头、30 行、溢出、纵向滚动条和外层滚动；不得靠删除原条件隐藏问题 |

## 实际环境与驱动

- 源码：`64048e11c`，开始调查时工作树干净。
- macOS 26.6.2 / arm64；.NET 10.0.8；Avalonia 12.1.4；AtomUI 6.2.10。
- 实际后端：Headless + Skia，`UseHeadlessDrawing=false`；使用真实 AtomUI Desktop/DataGrid 注册与 `AtomUI.Desktop.Controls.Window`。
- 窗口实际 ClientSize=1000×800，RenderScaling=1。目标表格 Height=400，行视口高度 350，实测行高 53 DIP。
- 原场景含前一个四行表格、标题和外层 ScrollViewer；目标数据复用 Gallery 的 30 行内容结构。外层实际 offset=139，目标视口完整可见。
- Demo 项目引用当前工作区 DataGrid 和 Generator 工程；程序集输出位于项目外。测试程序集名复用已授权的 friend assembly，仅用于读取 internal 几何和呈现状态，不改生产可见性。
- 驱动使用 `Window.MouseMove/MouseDown/MouseUp/MouseWheel`，经过 Avalonia raw pointer 和实际命中路径；滑块同样用鼠标输入拖动。记录 routed event 来源及捕获目标，不直接调用拖拽处理方法或设置被测滚动偏移。
- `TraceListener.Fail` 将 Debug 断言转成带栈的异常，命中时返回非零；不吞掉断言继续执行。普通断言失败返回 2，异常返回 3。
- 关键状态在布局和现有动效等待后通过 `CaptureRenderedFrame()` 捕获。已查看原视频关键帧、首行失败帧、末尾取消及返回帧，以及滑块返回顶端后的关键帧。
- 平面 A/B 只移除外层滚动和前一个示例，目标表格、数据、主题、高度和输入轨迹保持一致。

本次实际执行入口（临时工程将在调查结束后清理，命令为执行记录）：

```sh
demo=/private/tmp/atomui-issues522-524-demo-3oN0AZ
evidence=/private/var/folders/5w/_dgb_6815871tzq0vy_x_w1w0000gn/T/scripted-visual-acceptance-_q9wwfze/artifacts
dotnet build "$demo/Repro.csproj" --nologo -v:q
dotnet "$demo/bin/Debug/net10.0/AtomUI.Desktop.Controls.DataGrid.Tests.dll" top "$evidence/nested"
dotnet "$demo/bin/Debug/net10.0/AtomUI.Desktop.Controls.DataGrid.Tests.dll" top "$evidence/flat" --flat
dotnet "$demo/bin/Debug/net10.0/AtomUI.Desktop.Controls.DataGrid.Tests.dll" top-scrollbar "$evidence/nested"
dotnet "$demo/bin/Debug/net10.0/AtomUI.Desktop.Controls.DataGrid.Tests.dll" bottom-hold-scrollbar "$evidence/nested"
dotnet "$demo/bin/Debug/net10.0/AtomUI.Desktop.Controls.DataGrid.Tests.dll" repeat "$evidence/burst" --burst
```

其他已实现并执行的 case：`top-edge`、`bottom`、`bottom-edge`、`bottom-second`、`bottom-hold`、`roundtrip`、`cancel`、`stress`、`repeat`、`top-scrollbar`、`bottom-hold-scrollbar`。`--burst` 在 MouseUp 之后、布局泵和等待之前立即发送 12 个跨列/跨行悬停输入；`repeat` 在同一个 Grid 上反复移动 Id=6。

## 结果与原因

### AC-01：稳定复现目标行不可见，FAIL

起始数据为 `1,2,3,4,5,6,7…`，offset=0，显示 slot=0…6。将第 6 行拖到首行：

- `RowReordering=1`，`RowReordered=1`，数据变为 `6,1,2,3,4,5,7…`，证明排序已经提交。
- 放手后 offset=53，NegVerticalOffset=0，显示 slot=1…7；屏幕显示 `1,2,3,4,5,7,8`，Id=6 被放到视口上方。
- 所有显示行的数据和新 Index 对应正确；不是集合 Move 没执行，也不是显示行仍绑定旧集合。
- 原场景和平面 A/B 得到相同的 53 DIP、slot=1；明确检查目标首行可见性的两个运行都以退出码 2 失败。
- 向上滚轮一格后 offset=3，slot=0，Id=6 已重新出现。当前源码未复现 #522 的完整「向上也无法恢复」序列。

当前复现现象可追到旧内容锚点恢复：

1. `DataGridLocalSource.MoveAsync` 改变集合并发出 `Invalidated`。
2. `DataGrid.RangeLoading.cs / ApplyRangeSourceInvalidation` 捕获旧呈现的首个完整行锚点，此时为 Id=1、屏幕位置 0。
3. 新数据排序后 Id=1 变为 slot=1。
4. `DataGridPresentationIndex.RestoreAnchor` 按 RowKey 找到新 slot，恢复偏移为该行的新顶部 53。
5. `CommitRangePresentation` 落实这个偏移，拖到首位的 Id=6 因此被卷到视口上方。

这里采用的内容锚点策略适合保持普通失效前后的内容位置；用于用户主动行排序时，会抵消用户刚完成的可见放置效果。

### AC-02：首行参与排序时的额外跳动，FAIL

同样从顶端起始、不越界、不触发边缘自动滚动：

| 操作 | 放手后数据前缀 | offset | 首个显示内容 |
| --- | --- | --- | --- |
| 第 1 行拖到可见末行 | `2,3,4,5,6,7,1,8…` | 318 | Id=1，slot=6 |
| 第 2 行拖到同一末行 | `1,3,4,5,6,7,2,8…` | 0 | Id=1，slot=0 |

改变的变量是源行是否为原首行。两者都以旧 Id=1 为锚点，结果准确对应新位置 `6×53=318` 和 `0×53=0`，支持 AC-01 的原因分析。

### AC-03：取消可复现，滚动卡住未复现，PARTIAL

- 第 1 行越过视口底部后继续移动，实际 offset=75.08333333333326，首个 slot=1。
- 原行离开显示范围后会话取消；`RowReordering=1`、`RowReordered=0`，集合仍为 `1,2,3…`，最终 DraggedRowIndex=null、捕获释放。
- 屏幕确实不再显示 Id=1，但当前它处于真实滚动位置上方；向上滚轮后 offset=25.083333333333258、slot=0，Id=1 重新出现在顶部的部分可见行中。
- 这复现了「越界后取消、原行暂时不可见」的外观，未复现「向上无法恢复」；不能将两者等同。
- 会话有效性要求源行仍是已显示元素。自动滚动使其离开当前范围后，后续移动/卸载路径会取消会话；这也解释当前版本的越界取消行为。

### AC-04：滚动条输入

两个场景均完成滑块「向上 → 向下 → 再向上」输入；所检验的恢复操作脚本与视觉均 PASS。

| 场景 | 放手后 offset / 首个 slot | 上拉后 | 下拉后 | 再上拉后 |
| --- | --- | --- | --- | --- |
| 第 6 行移到首位 | 53 / 1 | 0 / 0，首行显示 Id=6 | 371.9298245614036 / 7 | 0 / 0，首行显示 Id=6 |
| 第 1 行越界取消 | 75.08333333333326 / 1 | 0 / 0，首行显示 Id=1 | 371.9298245614036 / 7 | 0 / 0，首行显示 Id=1 |

滑块实际宽 8、高 75.28301886792453；鼠标起点为实际 thumb 中心，x=974。下拉位移 80 DIP，向上位移分别为 -100、-150 DIP；路由事件与数值证实到达并响应。已查看两个场景上拉至顶后的图像，首行文本、表头、内部滚动条和外层布局符合记录。当前版本的滚轮、滑块都能向上恢复，因此原视频的完整滚动卡住仍为 NOT_REPRODUCED。

### AC-05：异常未复现，INCONCLUSIVE

- 在同一 Grid 上对 Id=6 进行了 12 次首尾往返放置，实际 `RowReordering=12`、`RowReordered=12`。
- 另执行 8 轮底部自动滚动、反向移动及返回顶端的取消压力场景；以及放手后立即跨行/跨列移动的输入。
- 本轮未观测到 `OwningGrid != null` 或 `_verticalOffset==0 && NegVerticalOffset==0 && FirstScrollingSlot>0` 断言，已采样显示行均保持有效所有者。
- 第二个异常栈的 `ScrollSlotsByHeight` 当前已在 `IsRangePresentationActive` 时提前改走 `QueueRangeScrollOffset`；`a79560ce2c` 引入了这条路径。原栈中的后半段算法不能直接视为当前 Range 模式仍走的执行路径。
- 第一个栈指出的生命周期风险仍存在可调查入口：`DataGridCell.OnPointerEntered` 只检查 OwningRow，而 `DetachFromDataGrid` 可清空该行 OwningGrid；但是否有实际可命中的旧容器、具体事件时序，本轮未取得失败证据。
- #524 为 Windows / .NET 10.0.14，本轮为 macOS 上 Headless/Skia / .NET 10.0.8，未验证 Windows 原生输入与合成器时序，也未验证 macOS 原生后端。未执行历史源码版本。

## 后续修复边界

本轮确认了共同的拖拽、视口与容器生命周期调查范围，以及当前可稳定复现的重排锚点问题；尚不能证明两个异常与该锚点问题完全同根。

后续应分别处理主动排序后的视口定位，以及自动滚动导致源行卸载时的会话/命中生命周期。不能以强制刷新、延时、隐藏滚动条、取消高度约束或只吞掉 OwningGrid 断言作为根因修复。主动排序的锚点策略还需覆盖拖动行在首行、目标在首行和普通外部数据失效，避免修复排序跳动后破坏正常的内容锚定。

## 验证与清理

- 共 16 次独立场景运行，34 次实际 `RowReordering`，20 次成功 `RowReordered`；其余为取消场景。总异常数为 0。
- 明确检查首行可见性的原宿主、平面 A/B 各失败一次，退出码均为 2。其他运行返回 0 仅表示该运行已设置的断言通过，不能抵消 AC-01/02 的已观察失败，也不代表三个 issue 均不存在。
- 临时 Demo 构建成功；最后一次构建 0 errors、1 个预期的 AVA3001 私有 API 提示。首次重建源码另有原有 CS0649 字段警告。
- 无生产修改、无永久测试、无提交；不运行 affected-wide 或全量回归。本轮属于调查，未实施修复，因此没有修复后绿灯证据。
- 总体结论：FAIL（主动行排序后的可见结果/视口保持合同失败）；#523 原始完整症状和 #524 异常的结论仍受上述复现范围限制。
- `evidence.py audit` 与 cleanup 的审计均为 clean、findings=[]；项目内未发现本轮取证二进制。两段原视频、抽帧及全部 Demo 截图均已清理。
- 所有本轮 Demo 进程已退出；项目外临时 Demo、驱动、日志和专用编译产物已删除。仓库仅保留本文文字调查记录，未留下临时测试或埋点。

## 用户授权后的修复阶段

### 实现与合同

- 拖动及 `MoveRowAsync` 的源调用共用执行入口；每个 Grid 的 AsyncLocal 只承载该源调用的移动请求，finally 恢复原值，跨 await 不持有 Grid 或行容器。
- 自失效事件在接收线程捕获移动请求并显式传递到 UI，不依赖 Dispatcher 传播执行上下文；Query/snapshot 匹配时使用位置锚点。
- 位置锚点保持首个可见 slot 与行内偏移，不跟随旧首行 RowKey；新行过矮、无法容纳原行内偏移时恢复到该 slot 顶部，保证目标行可见。
- 普通外部失效、同源其他 Grid、移动失败或外部失效取消后的重载仍使用内容锚点。未增加 Public API、AXAML、Token、绑定、订阅、timer 或 CTS。
- 代码限于 `DataGrid.Mutations.cs`、`DataGrid.RangeLoading.cs`、`DataGrid.RowReorder.cs` 和 `Data/Virtualization/DataGridPresentationIndex.cs`；同步 DataGrid 的 overview、implementation、范围数据源设计和 changelog。

### RED → GREEN 与视觉复核

临时测试精确命令：

```sh
dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.Issue522 --nologo -v:q --logger 'console;verbosity=normal'
```

- 原实现：10 个用例中 7 个按 offset/可见行合同失败，3 个外部失效及失败/取消对照通过。不是编译失败、Skip 或零发现。
- 初次修复：同样 10 个全部通过；可变行高部分首行的补充验证后 11 个通过。
- 独立评审发现旧行内偏移大于替换行高度时会跨过 slot。纯锚点用例先得到 slot=2 而非 1；真实 UI 用例先得到 slot=1 而非 0。恢复到原 slot 顶部后最终 13 个临时用例全部通过，0 skipped，用例阶段总耗时 5.2098 秒。
- 覆盖真实指针重排、API 移动、同步/异步自失效、不发通知的异步源、非零视口、外部插入、同源双 Grid、失败、外部取消，以及可变行高的部分首行。

项目外临时 Skia Demo 位于 `/private/tmp/atomui-issue522-fix-KM9R2q/demo`，引用当前源码；执行入口为：

```sh
dotnet build /private/tmp/atomui-issue522-fix-KM9R2q/demo/Repro.csproj --no-restore --nologo -v:q
evidence=/private/var/folders/5w/_dgb_6815871tzq0vy_x_w1w0000gn/T/scripted-visual-acceptance-i09opug8/artifacts
# phase 为实际运行的 before 或 after，输入轨迹相同。
phase=after
dotnet /private/tmp/atomui-issue522-fix-KM9R2q/demo/bin/Debug/net10.0/AtomUI.Desktop.Controls.DataGrid.Tests.dll top "$evidence/$phase"
dotnet /private/tmp/atomui-issue522-fix-KM9R2q/demo/bin/Debug/net10.0/AtomUI.Desktop.Controls.DataGrid.Tests.dll bottom "$evidence/$phase"
```

该驱动使用相同完整历史执行修复前后：初态 → 12 个真实拖动移动事件 → 放手 → 向下滚轮 → 滑块返回顶端。原固定高度、30 行、前一示例、标题、内外滚动、主题和尺寸均保留。每次采样等待至少 700ms，并要求连续 3 个实际 Skia 帧相同；超过 3 秒不稳定时报错，未关闭动效。

| 场景 | 修复前放手后 | 修复后放手后 | 脚本及查看结论 |
| --- | --- | --- | --- |
| 第 6 行放到首行 | offset=53，slot=1，Id=6 不可见 | offset=0，slot=0，首行立即显示 Id=6 | 前 FAIL，后 PASS；已查看首行、表头与滚动条 |
| 第 1 行放到第 6 个可见行 | offset=265，首个显示内容为 Id=1，视口向下跳动 | offset=0，首行显示 Id=2，Id=1 位于第 6 行 | 前 FAIL，后 PASS；已查看完整可见顺序 |
| 滚轮与滑块 | 驱动真实命中输入 | 滚轮使 offset>0，滑块回顶后 offset=0 | 两个主场景均 PASS，首行立即可见 |

Demo 前两个主场景均退出 2；最终源码后两个主场景均退出 0，集合移动事件均恰好 1 次。已查看最终截图；本次只声明 Headless/Skia 验证，不声称 Windows 或 macOS 原生后端通过。

### 评审、验证边界与测试处置

- 一次独立只读评审：0 Critical、1 Important、0 Minor。Important 已按上述额外失败证据修复并转绿；不再进行第二轮重复评审。
- #524 Windows 原生断言仍未复现；未对其增加吞错、延迟或防御性补丁，风险仍为原生异常待验证。
- 第三方 Source 若把通知转交不传播调用执行上下文的独立调度器，现有 Invalidated 协议无法关联移动请求，该通知继续视为外部失效；这类通知仍可能沿用内容锚定，未声称在本轮覆盖。
- 并行任务提交了 `0c1db3691` 的右冻结列缩放修改，本轮保持其内容和历史，不评审或修改该功能。最终验证基于该提交后的实际工作树。
- 临时测试在通过后已删除，新增常驻测试数为 0。已有 `Local_Source_Row_Reorder_Moves_Observable_Collection_Item` 改用 30 行并断言溢出、offset=0 和首个 slot=0，保留原有 ghost 布局、集合移动和显示顺序验证；用旧内容锚点策略确认该现有用例能发现本次故障。未新增常驻 helper、fixture 或独立测试。其他临时场景为本次按需验证，不声称已全部进入长期自动回归。
- 源码最终版本启用 `EnableTrimAnalyzer=true`、`EnableAotAnalyzer=true` 的 DataGrid 构建完成，0 errors；现有 `DataGridBoundColumn.cs:221` 的 ReflectionBinding 仍产生 IL2026/IL3050，另有原有 CS0649 字段警告。该构建不是原生发布验证，本次没有新增反射或 AOT suppression。
- 最终 4 个生产文件成员审计无 findings；未改变 partial 控件主文件的公开合同或静态初始化依赖。
- 取证 cleanup 审计 clean、findings=[]、audit_error=null，移除 17 个登记文件；最终截图已经查看。Demo、13 个临时用例及其 helper 均已删除，无本轮长运行 Demo 进程。
- 强化后的已有排序用例在旧策略下以 offset=106（预期 0）失败，恢复修复后通过，发现 1/执行 1，无 skipped。
- 最终 focused 确认在冻结输入后完成：`iteration-passed`，100.068 秒；DataGrid 316、Desktop 相关 28、Gallery 相关 9，共 353 个测试通过。选择器为 5 个明确的修改路径，覆盖 3/15 个项目、49 个相关类；没有执行 affected-wide 或全量回归。
- 本阶段结论：已确认的主动排序视口问题 PASS，修复前后同条件失败/通过成立；#523 完整旧症状和 #524 Windows 异常继续保留前述边界，不将其一并宣称已修复。

最终模块命令：

```sh
python3 scripts/verification/test.py run --scope iterate \
  --path src/AtomUI.Desktop.Controls.DataGrid/DataGrid.Mutations.cs \
  --path src/AtomUI.Desktop.Controls.DataGrid/DataGrid.RangeLoading.cs \
  --path src/AtomUI.Desktop.Controls.DataGrid/DataGrid.RowReorder.cs \
  --path src/AtomUI.Desktop.Controls.DataGrid/Data/Virtualization/DataGridPresentationIndex.cs \
  --path tests/AtomUI.Desktop.Controls.DataGrid.Tests/Interaction/DataGridRangeMutationTests.cs
```

修复阶段未创建分支/worktree或提交，改动先留在当前工作树供用户审查。

### 用户审查与提交授权

用户随后明确要求提交全部改动，并指出 `TryCaptureRangeMutation` 的 IDE 可空检查警告。该 helper 改为 nullable out 参数，使用
`NotNullWhen(true)` 表达成功输出非空；直接捕获实际可空字段，保留 source、capability、presentation、generation 的运行时检查。
没有删除 guard 或压制诊断。随后精确执行 `DataGridRangeMutationTests`、`DataGridSourceLifecycleTests` 和
`DataGridQueryControllerTests`，34 个测试通过，0 failed、0 skipped，耗时 2 秒。按用户明确授权提交本轮全部改动。
