# DataGrid 行排序视口修复计划

> 执行方式：使用 `superpowers:executing-plans` 在当前目录、当前分支实施，不创建分支或 worktree，不提交。

**目标：** 修复已确认的主动排序后视口跳动；保留普通数据失效时按内容锚定的行为。

**依据：** [调查记录](../progress/2026-11-01-datagrid-row-reorder-issues522-524.md) 的 AC-01/02；用户已明确要求修复。

**技术方案：** 在既有范围重载的锚点中表达按 slot 保持位置；通过每个 Grid 独立、仅在源 MoveAsync 调用执行上下文内有效的请求信息，识别主动移动导致的源失效。失效事件跨线程派发时显式传递请求身份。无自失效的移动完成路径使用同一锚点选择。沿用弱引用完成路径，不持有 UI 容器跨异步等待。

**约束：** 不改 Public API、AXAML、Token、尺寸、滚动条或事件合同；不强制刷新、不用延时修复、不增加永久测试数量。原生 Windows 崩溃未复现，本轮不作无证据修补。

## Task 1：根因修复与验收

- [x] 在 `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Temporary/Issue522/` 创建临时测试：真实指针拖第 6 行至首行、第 1 行至可见末行；保持 Height=400、30 行、内外滚动。断言排序、视口 offset/slot、目标行文本与拖拽状态。
- [x] 精确运行临时测试，取得原实现按目标断言失败的证据。
- [x] 修改现有移动/失效/锚点流程。内部锚点允许只有位置而不指定行或组身份；移动执行上下文在 finally 中恢复。
- [x] 临时覆盖 API 移动、同步/异步自失效、无通知的异步移动、外部失效、取消/失败、同源另一 Grid 与可变行高部分首行；查询和源更换沿用既有模块用例。
- [x] 用项目外 Headless/Skia 驱动取得原场景修复前后截图，检查首行立即可见、末行放置不跳动、滑块/滚轮仍可用。
- [x] 清理临时验证代码、媒体及专用产物，运行 DataGrid 修改模块的 focused 验证和成员布局检查；按 executing-plans 要求作一次独立最终评审，处理影响正确性的发现。

## Review Focus

- 自失效发生在 MoveAsync 返回之前，不能只调整完成后的 offset。
- 普通外部失效和同源其他 Grid 不得误用主动排序的位置策略。
- 异步完成与失效事件跨线程调度不依赖 Dispatcher 隐式保留执行上下文。
- 失败、取消、换源和查询变化不能留下下一次操作会读到的移动上下文。
- 部分首行、非零滚动位置及可变行高需要保持 slot 与行内相对位置。

## 执行记录

- 初始状态：HEAD=`64048e11c`；只有上一轮调查 Markdown 未跟踪。继续在原工作目录内执行。
- 流程适配：仓库禁止新建分支/worktree、擅自提交和 `.superpowers/` 产物；执行记录放本文，临时 brief/日志放系统临时目录。
- 实现范围细化：锚点策略选择放在 `DataGrid.Mutations.cs`，复用 `CaptureFirstCompleteRangeAnchor()`，因此无需修改 `DataGrid.Virtualization.cs`。
- RED：精确临时用例 10 个，7 个因 offset/可见目标错误失败，3 个外部失效及失败/取消对照通过。此前用设置 scrollbar.Value 准备非零视口未触发真实滚动，已改为框架滚轮输入重新取得有效 RED，不计入故障证据。
- GREEN：相同 10 个用例全部通过；补充可变行高、部分首行的位置锚点验证后共 11 个通过。
- 变更审计：4 个生产文件，无新 public/protected API、绑定、订阅、timer、CTS 或反射；新增的 AsyncLocal 仅承载源调用上下文，不是持有异步任务期间的全局忙碌标志。成员检查覆盖 4 文件，0 个已识别带基类控件声明、0 findings；partial 控件主文件的公开合同未改。
- 独立评审：1 Important、0 Critical、0 Minor。较高部分首行被较矮行替换时，旧行内偏移会越过 slot。纯锚点与真实 UI 的临时用例均先失败；位置锚点在无法保留原行内偏移时回到该 slot 顶部后，全部 13 个临时用例通过（5.2098 秒）。普通内容锚点不受此分支影响。
- 评审范围判断：#524 Windows 原生异常仍未复现，本轮不修改其路径，风险为该平台异常仍待验证；独立调度器不传播 MoveAsync 执行上下文的通知没有请求身份，继续按外部失效处理，风险为这类第三方源仍可能采用内容锚定；并行的列头缩放改动不属于本次修复，保持原样，其正确性不由本轮评审背书。视觉验收、清理和最终模块检查按剩余步骤执行，不将尚未运行的门禁视为通过。
- 期间检测到另一个任务提交 `0c1db3691`（右冻结列表头缩放）。本轮未修改该提交或列头文件，最终模块检查以该提交之后的实际工作树为准。
- 长期保障：将已有 `Local_Source_Row_Reorder_Moves_Observable_Collection_Item` 的数据扩为 30 行，并增加溢出、offset 和首 slot 断言，保留原来的集合移动、ghost 与显示顺序覆盖。临时切回旧内容锚点策略后现有用例以 offset=106（预期 0）失败，恢复修复后通过；测试总数不增加。
- 首次清理后的 focused 验证通过：DataGrid 316、Desktop 相关 28、Gallery 相关 9，共 353 个，0 失败（112.592 秒）。因随后强化既有回归用例，最终再次按相同模块及该测试路径确认。
- 强化既有用例后的第一次确认在 discovery 阶段因交付 Markdown 同时更新触发工作树一致性保护而停止，没有执行测试；停止文件写入后以相同 5 个路径重跑。
- Task 1 complete：最终 `run --scope iterate` 显式指定 4 个生产文件及已有回归测试路径，`iteration-passed`（100.068 秒），316+28+9=353 个相关测试通过。成员审计 4 文件无 findings；Skia 前后验收通过；AOT/trim analyzer 构建 0 errors，仅有未触及位置的现有 ReflectionBinding/字段警告。临时测试、Demo 和媒体已清理，不创建提交。
- 用户后续明确授权提交全部改动，并补充 IDE 可空检查反馈；`TryCaptureRangeMutation` 的 out 参数改为 nullable + `NotNullWhen(true)`，去掉捕获时的空值抑制，保留运行时守卫。修改后 34 个 mutation/source lifecycle/query controller 用例通过，随后执行用户授权的提交。
