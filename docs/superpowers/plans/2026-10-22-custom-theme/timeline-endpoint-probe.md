# CT15 P3-C Timeline 整周期端点定位（2026-11-01）

保留P3-B原四候选/四旧源码端点FAIL及52轨迹旧新零，不改写成通过。Spin已完成限定工程/清理并获46对批准，接回此真实出口。只做独立最小Timeline pending探针，不替代原1320×760完整业务历史。

复用CT11既有临时真实Compositor Stopwatch输入，仅临时测试中保存/恢复完整clock状态；产品源码、官方依赖和时钟不改。按0/125/300/750/1000/1125/1300/1750/2000ms取真实Skia完整帧；同一时间连取两帧并读取真实Server RotationAngle，检验模型与已呈现画面是否一致。普通/暗、横/纵四场景，并用public PendingIcon同类型LoadingOutlined作独立对照，不换glyph或关闭动画避开原触发。

先取得有效反例，再用单变量观察定位。无证据不改产品，不自动归入或扩大CT25/FW1，不更换依赖/容差，不在测试中清composition私有引用。临时媒资按官方evidence工具登记、查看、记录、清理，再精确Timeline模块验收；没有产品变化时沿用相同源SHA的NET8证据。

第一批8原生模型用例全部有效FAIL（普通/暗×横/纵×默认/公开同型PendingIcon），共144帧完整：0/125/300/750ms角度0/0.7853982/1.8849558/4.712389，1000/2000ms仍4.712389，首/次捕获像素一致，不是简单漏一帧。四默认场景端点仍50像素，独立Native Icon Spin/Pulse×1s/0.5s四项也全FAIL（原生32尺寸端点158像素）。

单变量仅在临时驱动中把同Duration/Forever/Linear旋转模型改为隐式0起点+唯一terminal keyframe，四场景全PASS（2s），1000/2000ms角度0、完整RGBA回0，其他相位角度与像素轨迹保持。产品尚未修改。先查本地参考HEAD为12.1.3，随后只读git show已有12.1.4 tag的对应求值器，避免混用HEAD版本；原官方依赖未替换。后续用官方运行中的RecordingLinearEasing观测非有限输入作独立因果验证。

官方12.1.4本地tag为2d79ba55fae1b1e71c69f93a668080c9029e6a31。精确tag求值器在progress=0且首key=0时形成(0-0)/(0-0)，非有限easing结果沿用currentValue。独立RecordingLinearEasing实际运行1PASS/293ms：显式模型收到NaN并保持4.712389，implicit模型只收到0/0.75并在1000ms回0；原官方运行缺陷仍可复现，不声称框架被修复或扩大FW1。

最小消费者修正仅Core Icon.cs：启动时把已有RotationAngle明确归零，作为隐式首帧；删除冗余0 keyframe，仅保留1→2π，原Spin/Pulse easing、duration、Forever、stop/显隐/centerpoint均保持，无新增产品helper/API/Token/clock。70所选全PASS（17临时+既有Timeline52/LoadingIcon1，7s），追加临时.NET10 UnsafeAccessorType时钟访问器1PASS/267ms，与原Reflection读到的同一个clock/字段/角度完全一致；仅给外部AOT取证使用，不进入产品。18项临时所在2源14672字节已删除。

第一轮255对原新完整帧215零，40非零仅16个Timeline端点的first/second和8个Native Spin/Pulse端点，Timeline各50/max233明或200暗、32尺寸Native各158/max224。其余相位未变。首外部驱动普通配方188帧/162检查全PASS，构建零警告24.86s；后续三算法/原/AOT冻结同完整历史执行中，未清理/最终验收前不请求批准。


已实际查看四算法完整contact与原始端点，保留原暗色正文低对比。18临时2源14672字节已删除；2817取证文件（原小探针/候选对照/三版2256主帧/四拼图/TSV/manifest）经官方audit/cleanup清理，findings=[]、removed=true。外部driver412文件/publish15文件及原470会话共1074185242字节已删除；只有单独最终门禁会话run.py/cleanup.json/module.log/net8.log暂存，完成后删除。最终Core Icon成员1源/1控件24→0，前后成员token指纹及字段序列同，不重跑相同功能AOT轨迹；清理后精确模块与NET8验证纯整理后的最终源码。


清理后精确模块真实重建：Timeline52+LoadingIcon有效显隐1，共53PASS/0FAIL/0Skip、1s（命令30.80s）；Core NET8 Release零警告零错误3.37s（命令3.57s）。两次均先取得共享输出非阻塞独占锁；最终四文件门禁会话随后删除。正式Icon implementation/changelog、结果/作者自审及两CSV/总表同步。仅96独立端点对已获限定批准，64同时间second为相同图像复核，不扩大为额外产品状态；15真实clock旧新及15cross非零、Part恢复max1、原P3-B及本轮原FAIL账本/平台/完整P3与CT15保留，不提交。


限定批准登记：2026-11-01，用户明确回复“批准表内96对，其余边界保留”（request_user_input_async / call_d2f7bcbd00ab46e7b6cd3610f38aa4bb / item 0）。仅P3-C表内96个独立端点变化获批准；64张second为相同图像复核，不新增产品状态。各15真实clock非零、Part恢复max1、原失败账本、平台及完整P3/CT15继续保留。官方显式0模型NaN仍可复现，不改CT25/FW1，不授权提交。
