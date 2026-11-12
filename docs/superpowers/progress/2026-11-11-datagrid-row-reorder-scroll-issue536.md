# DataGrid 行拖动滚动丢失修复（Issue #536）

> 分支 `release/6.0`，基线 `9fb2fadc1`；在当前工作目录直接修改，未提交。

## 问题

拖动 DataGrid 行时，滚轮滚动或把指针拖到顶部/底部边界外触发自动滚动，一旦源行滚出视口，下一次指针移动即取消拖动，
幽灵行消失。源行仍在视口内时滚动不受影响。

## 根因

拖动会话绑定在源行控件实例上，而该实例会被虚拟化回收：

- `IsRowReorderSessionValid` 要求源 slot 可见、`DisplayData.GetDisplayedElement(slot)` 仍是源行、`Row.Index` 不变；
  `HandleRowReorderPointerMoved` 先做该校验，源行滚出后直接 `CancelRowReorder`。
- Pointer capture 挂在源行内的 `DataGridRowReorderHandle` 上；范围呈现下回收会把行移出 `RowsPresenter.Children`，handle 的
  卸载、detach、模板重套用路径也都会取消会话。
- 边界自动滚动由拖动自身在 `GetRowReorderTarget` 中发起，源行滚出 5～6 行后必然自我取消。
- 范围呈现下滚轮与自动滚动均经 `QueueRangeScrollOffset` 异步生效，插入目标是在滚动生效前按旧布局命中的。

## 修复

- Pointer capture 改由 `DataGridRowsPresenter` 持有；presenter 转发 move / release / capture lost，handle 只负责按下，
  不再因卸载、detach 或模板重套用结束会话。
- 会话有效性改为数据身份：源 RowKey 仍位于其已提交 slot，加上原有的 owner、Source、Query、snapshot、能力校验。
  超过拖动阈值前仍要求源行在显示（`RowReordering` 需要报告真实行）。
- presenter 每次 arrange 后若显示视口（首个滚动 slot、`NegVerticalOffset`）不同于上次命中目标时的视口，以 Background 优先级
  按最后指针位置重新解析插入目标；该路径不改 ghost 偏移、不触发自动滚动。presenter 的 move/release 仅在自身持有 capture 时转发。
- 删除不再使用的 `RowReorderItemsMatch` 与按 handle 取消会话的重载。无 Public API、AXAML、Token 变化。

## 验证

- 视觉复现（修复前，原生 `screencapture -l`，`atom:Window`，Height=400、50 行）：A/A=0、1 DIP 可检出；滚轮 1 格（源行仍可见）
  PASS；滚轮 6 格、底部边界、顶部边界三个场景均在源行 slot 离开视口后的第一次移动丢失会话。
- RED：临时测试 4 个（滚轮滚出后移动松开、滚轮后不移动直接松开、底部/顶部边界自动滚动）在原实现全部按“会话丢失”断言失败；
  修复后全部通过。第一版仅在滚轮处理中同步刷新目标，因范围滚动异步生效仍落后一格，改为 arrange 驱动后通过。
- 既有用例：`Local_Source_Row_Reorder_Moves_Observable_Collection_Item` 与
  `Source_Invalidation_Cancels_Range_Drag_And_Releases_Pointer_Capture` 断言 capture 为 handle，属于实现细节变化，改为 RowsPresenter；
  其“拖动持有 capture、失效后释放”的契约不变。
- 视觉复核（修复后，同条件）：Gate、AC1～AC5 全部 PASS。滚轮 6 格后插入指示行 = 指针下行（8 = 8）；底部/顶部边界自动滚动
  各有 7 步源行不可见而会话保持；滚轮 6 格后不移动直接松开，Row 02 落在指针下的 Row 09 之后。

- 最终模块验证（清理临时测试后）：`run --scope iterate` 指定 3 个生产文件与 `DataGridRangeMutationTests.cs`，`iteration-passed`
  （114.6 秒），DataGrid 326、Desktop 相关 28、Gallery 相关 9 个测试通过；`git diff --check` 无问题。

## 测试处置

- 临时测试 `Temporary/issue536/` 已删除。
- 在 `DataGridRangeMutationTests` 保留 1 个常驻用例
  `Row_Reorder_Survives_Wheel_Scrolling_Source_Row_Out_And_Drops_Under_Pointer`：
  - 契约：拖动中滚动回收源行后会话继续，松开落在指针下的行；违反时拖动丢失或落到错误位置。
  - 独立检错：既有用例都不在拖动中滚动；编译与其他检查无法发现会话重新绑定行控件或目标不随滚动更新。
  - 长期收益：UI 输入与滚动属于规范要求最小保障的类别，且该回归在重构会话/capture 时容易无声复发。
  - 成本：单个 Headless 用例约 1～2 秒，属于 DataGrid 模块既有选测范围。对原实现以“插入指示不在指针下的行”失败，修复后通过。
- Demo 与截图已删除或留在会话临时目录，未进入仓库。

## 后续：边界外幽灵行可见与持续自动滚动

用户要求一并修复两项既有行为：指针在边界外时幽灵行被裁剪不可见；自动滚动只在指针移动时发生。

- 实现：ghost 偏移夹在 `[0, CellsEstimatedHeight - 源行高]`；`CellsEstimatedHeight` 才是行区域视口高，presenter 的
  `DesiredSize` 在拖动中包含 ghost 行，不能当作视口。自动滚动改由会话级 `DispatcherTimer`（30ms）驱动，越边时立即滚一步，
  之后按越界距离滚动、单次上限 20，回到视口内、无法继续滚动或会话清理时停止；指针移动本身不再滚动，速度不随移动频率变化。
  `GetRowReorderTarget` 只做命中，不再滚动。
- 视觉复核发现持续滚动中插入高亮偶尔落后一行：投递的刷新与下一次 tick 的视口应用（Input 优先级）交错时，会按未重排的行
  边界命中却记录为新视口。刷新改为 presenter 布局无效时跳过、等下一次 arrange 重投。停到末尾再松开的落点在修复前后都正确
  （AC12），偏差只在滚动进行中且高亮与落点一致；修复后滚动中高亮落在被 ghost 覆盖的边缘行。
- RED/GREEN：临时测试 5 个（顶/底 ghost 停靠、顶/底静止持续滚动、回到视口与取消即停）。Headless 的 `RunJobs` 不触发
  `DispatcherTimer`（探针 400ms 0 次），改用 `Dispatcher.UIThread.MainLoop` 并以 10ms 布局计时器代替渲染循环；用错误泵法取得的
  首次 RED 不计，在原实现上以正确泵法重新取得 RED（ghost 位于 -168 / 577，持续滚动 0 行），修复后全部通过。
- 脚本视觉验收（原生 `screencapture -l`，`atom:Window`，Height=400、50 行）：一次运行因用户实体光标悬停在窗口上导致 A/A
  不为 0，增加“每次截图前检测光标并移开窗口”后 Gate A/A=0；Gate、AC1～AC12 全部 PASS。ghost 停靠：顶部 149.5、底部 500 与视口
  边缘重合，`#FAFAFA` 背景覆盖采样带 100%；静止 1 秒滚动 12～13 行；回到视口内与取消后 600ms 内 scroll 不变；静止滚到末尾原地
  松开落在末行之后。
- 测试处置：临时测试全部删除，不再新增常驻用例。持续滚动在 Headless 依赖真实计时与约 1 秒等待，稳定性与成本不满足常驻准入；
  ghost 停靠属于简单几何且人工使用立即可见。保留前述滚轮常驻用例。
- 最终模块验证（清理临时测试后）：`run --scope iterate` 指定 3 个生产文件与 `DataGridRangeMutationTests.cs`，`iteration-passed`
  （137.5 秒），DataGrid 326、Desktop 相关 28、Gallery 相关 9 个测试通过；`git diff --check` 无问题。
