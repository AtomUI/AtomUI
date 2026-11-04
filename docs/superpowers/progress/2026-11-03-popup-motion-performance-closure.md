# 执行记录 — plan: docs/superpowers/plans/2026-11-03-popup-motion-performance-closure.md

## 范围与当前输入

用户：把这些问题都解决好。复用当前feature/motion / HEAD4513cfd28，124个已暂存路径；本轮起点工作区无未暂存内容。授权修复和验证，仍只暂存不commit，不创建分支/worktree。

## 审计与执行决策

- 狭义正确性/性能闭环；责任owner是NavMenu交互关闭策略、Popup物理会话、Core单目标播放；探针不得关闭交互以换取稳定。
- 先恢复原来的3进程API场景，临时只读观测CloseRequested调用链、IsSubMenuOpen变化、Window失活/平台失焦、原始指针事件与几何，采样顺序不改；诊断运行不作为性能样本。
- 每个订阅都在场景Dispose解除。Core策略订阅仍随活动请求释放，不将目标或订阅放到共享Motion定义。
- 上一轮原始数据已按规则清理；文字记录和有效功能证据复用，不重复全套控件设计审阅。通过读取被reset的旧commit cb38b5b61可恢复原始成本基线（只读Git对象，不创建ref）；必要时仅当前目录做有恢复保障的基线构建。

## 状态

| 项 | 状态 | 证据 |
| --- | --- | --- |
| 原场景根因 | 完成 | 真实失活调用链与单变量原生重放；保持产品失焦关闭 |
| 请求分配 | 完成 | 旧版基线5456→5432B；6进程同驱动确认，400tick分配相同 |
| 性能/生命周期验收 | 完成 | 9项专项、6进程成本、页面10冷样本/重复、原生3进程、AOT通过 |
| 清理/审阅/暂存 | 完成 | 唯一整体审阅无Critical/Important；临时源码/产物已删除，只暂存 |

- 原参数第一批3进程通过；第二批process3有效复现。只读事件链：2775.42ms请求open且active=true，2876.13ms原生Window.Deactivated，2876.36ms平台LostFocus，2882.78msCloseRequested(expected=open,active=false)，随后item true→false。之前存在真实原始指针从窗口内部移动到x<0离窗；不是动效自行消失、裁剪或默认定义失效。产品失活关闭符合UX；探针未区分采样中断与功能失败是根因。保留原始失败记录，不禁用失焦/hover关闭来稳定采样。
- 有效分配RED：1000次有效策略读取实测24,000B。TokenResourceUtils泛型Enum参数传入object资源键时每次装箱；先修为固定只读boxed key，仍按目标ActualThemeVariant动态解析资源，不缓存布尔值。

- 受控原生A/B已验证根因：在同一原场景open后80ms用另一个原生Window.Activate产生失活，菜单按UX物理关闭；新probe正确输出EXPECTED_DEACTIVATION_DISMISSED_POPUP、Status=Interrupted、AcceptedSamples=0，exit3。无取消菜单失活逻辑、无重试或丢弃失败混入统计。临时失活注入已移除。
- 报表守门临时测试有效RED3→GREEN3：中断报告必须明确拒绝，popup缺少中间动画值必须拒绝，观测回调cadence翻倍必须拒绝。Schema升3，记录测量条件；比较不再把回调间隔下降当性能收益，cadence相差超过10%拒绝CPU/分配对比。
- Core策略读取RED 24,000B/1000次→GREEN0B，固定boxed key只缓存资源键而非有效布尔值。每请求委托改为无目标捕获的静态回调，事件到来时查询现有弱表的当前播放owner；获取/释放仍同一个try/finally，空闲不订阅。
- Generic自定义Popup有效RED：没有默认工厂消费几何，仍发2次MotionGeometry通知。跳过空默认pair路径；DatePicker/ColorPicker工厂仍接收非空测量几何。9项定向最终GREEN，包含3禁用阶段、替代请求、持有Task/定义时GC、零分配读取、3几何用例。
- GC驱动修正：异步构建helper可能保留临时栈根，改同步NoInlining并断言Task已完成；合批仍需像上轮一样ForceRenderTimerTick清空合成队列，最终9项通过。未放宽WeakReference、未清空Content/DataContext，不将未静止渲染队列算泄漏。

- 当前第一次完整固定tick样本（禁用tiered JIT）120动作有效：Slide关闭准备5744→5432B，Zoom5864→5552B；各减少312B。Slide打开65480→64144B，Zoom65600→64264B；各减少1336B。两个方向400tick分配逐项相同、实际内容Measure/Arrange0；Generic MotionGeometry通知2→0。与上轮原始关闭基线5456B比较当前少24B，但最终报告将优先使用相同驱动的紧邻前后结果，避免混用时钟/JIT条件。
- 为消除微秒级测量的CPU升频/JIT启动混杂，固定tick驱动COMPlus_TieredCompilation=0，每类型至少5预热循环且500ms热身，按前后交错多进程采样；同一驱动程序集复制到两个本地运行产物，产品程序集hash区分版本。运行产物不是源码隔离副本，不创建Git工作区。
- 唯一fresh整体review完成，无有证据的Critical/Important。方法学限制：自然原生合并cadence中位数不保证每动作tick数相同，因此CPU结论以固定tick数据为主，不用原生回调频率宣称显示FPS。未启动第二轮review。

- 6个交错进程（B/A、A/B、B/A），同驱动SHA、COMPlus_TieredCompilation=0、每类型500ms且至少5次预热、每方向每进程30动作全部通过。准备分配每进程完全一致：Slide开65480→64144B、关5744→5432B；Zoom开65600→64264B、关5864→5552B。400tick分配前后逐项一致。准备median时间Slide开109.35→101.25µs/关12.70→9.00µs，Zoom开110.45→97.80µs/关13.10→9.05µs；tick时间变化−0.78%至+1.73%，小于同版进程约5%的A/A波动，不宣称帧循环提速。
- 为独立核对最初旧实现，读取既有cb38b5b61 Git对象，仅在当前目录暂置源文件构建一个本地旧运行产物，finally逐字节恢复当前源码并验证index不变，无分支/worktree/源码副本开发。相同驱动/预热/时钟下旧实现测得Slide开64168B/关5456B、Zoom开64288B/关5576B；当前每种准备均少24B。此前新增288B已消除。旧实现/修复前/修复后400tick字节数完全相同。

- DatePicker真实Gallery加载比较（headless，两个运行产物同一驱动程序集、相同页面源码/2011视觉节点，每版10独立cold子进程，3预热+10重复导航）：cold mean1184.83→1174.16ms、median1189.17→1172.92ms、P951207.46→1203.32ms；重复mean216.86→213.33ms、median214.50→213.35ms、P95222.49→216.29ms。cold分配40393.64→40341.31KB，重复38004.56→38004.05KB。无页面加载回退；差值小，不作为明显页面提速宣称。
- 正式Schema3原生API场景：3进程、每3warmup/10测量/450ms全部exit0，各20个正式开/关动作，Status=Completed、每动作至少11个中间值。与最初失败使用同一场景/数据/操作顺序，产品失焦/hover策略未禁用。受控失活exit3记录仍独立保留至清理，不混入成功统计。
- 已删除Temporary/MotionPerformanceClosure以及工具CostProbe/临时注入，清理后测试程序集不含临时类型。Desktop1220/1220（50s）、Core1/1、DataGrid4/4通过，未运行全量回归。此前9项专项及3项报告守门测试通过后删除。
- 本轮稳定源码osx-arm64 NativeAOT发布exit0、产物validation passed；原有ReactiveUI IL2104/IL3053、未修改DataGridBoundColumn.ReflectionBinding IL2026/IL3050与Homebrew dylib目标版本警告保留，无本轮新代码告警。未将发布通过等同AOT版交互或显示FPS验证。
- Popup成员审计1文件/1控件/0问题。控制API/主题/渲染规格未改变；未新增实例字段/配置属性，静态键与回调不持有目标，获取/释放仍对称。性能复杂度负担：Core去除2个每请求委托局部变量、增加3个无目标静态缓存；Popup增加空默认pair的一次准备期判断；工具增加采样输入/失活观察，随原观测对象释放，不进入产品依赖。

- 最终工具清理后Release构建0警告/0错误，正式源码原生烟测2动作exit0；源码中无临时失活注入、CostProbe或临时测试类型。文件链接检查、git diff --check通过。本轮没有产生截图/视频等媒体；专用运行产物、旧版/前版/当前bundle、采样、恢复快照、原始事件追踪、临时Python测试和日志均已删除。
- 本轮系统tmp清理：删除专用目录及576个文件，目录剩余0；没有删除用户附件或常规开发构建产物。当前feature/motion与HEAD4513cfd28保留，不创建分支/worktree，不commit/push。
