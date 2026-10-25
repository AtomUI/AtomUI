# CT25：Linux包消费总体超时的阶段取证

> 使用superpowers:executing-plans，在当前checkout独立执行，无subagent／新分支／worktree，不自动提交。

**Goal:** 定位OfficialPtBr_Package_Graph_Is_Consumable在真实Linux两次600000ms总超时的具体阶段和累计耗时，保留原包消费及负对照，不盲目延长超时。

**Architecture:** 既有RunProcessUnchecked在显式开启的CI文字journal中记录真实子进程开始、结束、取消／阶段超时。相同stage_id关联PID、命令、UTC和耗时；原进程结果、stdout、stderr及异常保持。未配置journal时没有新增IO，journal写入失败不能遮蔽子进程失败。

**Tech:** 现有C# xUnit v3、Process、逐行JSON文本和现有Actions artifacts。

## Review Focus

- 首轮Linux其他八项目5664项通过；包消费项目4通过、1在600000ms超时。同SHA重试仍该总超时，不能再假定一次网络抖动。
- Mac该项目五项共483.332s，但不据此推断Linux阶段、网络或CPU；实际阶段记录才决定修正。
- 不跳过用例、删消费者／损坏包负对照，不增大600s或120s／300s阶段上限。
- journal不记录环境／凭证，只有既有内部构建参数和阶段；输出为仓库允许的文字证据，不产出媒体。
- 新验证均临时，最终删除并精确执行受影响模块；CI源码需再次按具体快照批准。

### Task 1: 取得可审查的阶段journal

- [x] Temporary/CT25StageTrace建立反例：阻塞子进程尚未完成时可读start，完成后相同ID／PID与耗时；非零结果及stdout／stderr完整；无效journal不能取代原失败。
- [x] 最小修改既有测试进程helper，环境变量显式开启。CI只配置该文字路径并保留现有artifact，不改变测试选择、过程超时或发布。
- [x] 清理临时源与产物；精确选测及工作流／成员／文档自审。实际Linux取得阶段证据后，再决定耗时／测试隔离修复；本步通过不表示超时已解决。

## 临时前后与清理结果

四项实际子进程探针，原版2通过／2失败（1.7345s），候选4通过（1.5248s）：门闩子进程尚未完成时已可读start，放行后相同stage_id／PID及正耗时；非零7与两输出流保持；无效journal不遮蔽原非零9；未配置不新增文件。没有调整原Fixture或阶段timeout，临时测试／旧源备份已删除，所有探针finally都等待自己的子进程并删除各自系统临时根。

清理后既有Repository_Build_Path_Overrides_Isolate_Outputs_And_Tool_Assets精确1项通过，1.9997s；真正dotnet msbuild阶段0.4818872s、exit0，journal的开始／完成匹配。该检查验证隔离路径和实际写入，不代表OfficialPtBr总体超时修复；Linux阶段数据仍须实际CI。工作流只多一个显式环境路径与同名artifact文本路径，无测试选择／发布变动。工具与文档审查另行回填。

## 第九批具体审查快照

作者自审通过：journal由显式CI环境开启、同进程lock整行追加、失败保留真实子进程结果与原异常，未改任何消费者、超时数值或NuGet发布。工具policy模块8项11.534s通过；环境变量和artifact文字路径逐一配对，git diff --check通过。临时四项已删除后，原真实MSBuild隔离路径1项通过，输出journal开始／结束ID、PID、exit0均核对。

本批仅5文件：workflow、既有Localization进程helper、affected verification说明、第八轮执行状态与本计划。C4绘制、C5异步位图、临时探针、媒体及全部二进制不纳入。拟在现有feature/custom-theme创建一个提交、推送，在第八轮全部job结束后运行三OS／两Browser验证；不创建分支／worktree，不发布NuGet。需用户按具体快照批准后执行，本批取证并不表示总体超时已修复。


## 已批准执行及第九轮真实结果

用户明确批准5文件快照411621891a39，创建769a26e28后逐一核对5个最终blob并推送既有feature/custom-theme；第八轮全部job结束后调度[第九轮40160735288](https://github.com/AtomUI/AtomUI/actions/runs/40160735288)，未取消其他取证job。没有NuGet发布、分支/worktree创建、超时数值或消费者变更。本次Git授权已执行完毕，不能扩大到C4/C5或后续修复。

第九轮完整结束：三NativeAOT／两Browser、Mac与Linux各6290项选测通过；Windows在Grid的Group_Headers_Carry_The_Body_Row_Marker处308/309，未继续后续项目。其异步分组提交时序已有独立受控RED/GREEN，后续批次处理。第八轮Windows原11项Desktop失败均已通过；另两个本地化失败仍需保留，第九轮没有运行到它们，不能因其他平台通过改记Windows通过。

Linux文字journal完整54行、27个stage_id全部有开始和完成，PID逐项配对；包消费项目5/5共469.14s。Official包图的Generator pack55.13s，四模块包13.22/6.53/8.20/8.05s，聚合6.99s；full/partial/dormant-corrupted/active-corrupted四次build分别102.35/87.51/83.86/88.64s，最后一次exit1为原有损坏包激活负对照。该次没有阶段取消或超时，未改600/120/300秒上限。它证明本轮完整包图通过，不足以倒推前两次600秒的具体原因，原失败记录保留。

Windows第八轮build-host.log清理占用覆盖了原测试退出原因；真实阶段journal尚未取得。本机3项目真实dotnet build以原参数／禁止server复用参数各一次，构建退出后lsof均无日志句柄所有者（3.783/1.692s）；临时工程及进程已清理。该探针没有证实Windows的句柄来源，未据此修改工具链或用清理重试掩盖故障。继续利用真实Windows阶段记录确定下一步。
