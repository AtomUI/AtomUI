# CT15 Icon 真实保留绘制假设核对

仅核对新源码线索，不改变既有CT12/13/14、Icon端点批准或FW1/FW2：生成AntDesign图标的StaticInstructions共享，DrawingInstruction.Draw会修改/恢复Geometry.Transform；此前Tag identity guard并未覆盖AntDesign的实际非identity zoom矩阵。

先用真实UserOutlined、CloseOutlined、ExclamationCircleFilled，32×32/Orange/native ViewBox，在同一Skia宿主比较Icon.Render直接输出与DrawingGroup保留后输出的全RGBA内存像素。三项全部PASS（67/2/1ms，合计4.1969s宿主），没有取得“保留绘制丢失变换”的有效RED；不按这个假设改Core或追加克隆缓存。首临时夹具PixelRect重载编译错误已纠正，不计产品RED。

临时三case/一源已删除，没有媒体、外部工程或产品变更。该结果只排除本次丢失变换反例，不自动解释原Tag/Avatar像素失败。下一步仅查原实际绘制命令是否保留对源Geometry的订阅；没有通知链证据即停止分支，不追加同类模型。清理后精确Icon门禁随后登记。

## 真实合成器反证与候选变更

第一组三项只覆盖DrawingGroup的平台快照路径，不能排除实际合成器引用问题。清理后既有Icon三项通过（533ms）。本地精确Avalonia12.1.4 tag对象`2d79ba55fae1b1e71c69f93a668080c9029e6a31`中，RenderDataDrawingContext保留Geometry本体，TransformChanged注册序列化，Geometry.SerializeChanges读取当前PlatformImpl；没有切换参考checkout或依赖。

同一真实三图标、32×32/Orange、透明Avalonia Window，无模板/chrome/位移，先取真实合成器帧再直接Render防观测改变基线；尺寸前提通过。有效RED为UserOutlined233、CloseOutlined120、ExclamationCircleFilled180像素。临时夹具Window namespace冲突编译错误经显式Avalonia alias纠正，不计RED。

候选仅在DrawingInstruction记录时传入Geometry.Clone，捕获该次临时Transform后仍按原finally恢复共享源；不改笔宽/FillRule/Opacity/矩阵算法，不加缓存/订阅/强引用或重绘补丁。同三项全部GREEN（604/407/404ms，宿主6.2849s）。这是可能改变默认合成器图标的实际绘制修正，旧NET8/AOT收据不能直接代表新Core源码；需同一冻结候选完成精确模块、真实原Tag/Avatar调用链及必要AOT/NET8。尚未认定原恢复像素根因、未扩大FW2或关闭CT15。

## 完整候选与原调用链

一行原型仍编辑调用方源数据（2次Changed），并阻断调用方几何热更新；两有效反证均FAIL。最终先克隆再变换，不编辑源几何；对已连接视觉根的当前Icon获取路径Geometry.Changed订阅，以弱引用通知失效，unload/detach释放。复用既有CompositeDisposable/Clone，不新增缓存、令牌、属性或静态控件引用。

六临时用例PASS（4s）：三真实图标合成器/即时一致、无调用方几何假编辑、热更新含离树/重挂恢复、外部几何与Window存活条件下Icon可回收。两源/一控件成员审计0问题。原Core运行产物SHA256为`67c9b16018136214118f1fb63923272f29388788d1f42e9004f057854a67659c`；最终候选Release Core为`75ceddec42c2c20d06d4a1e7e76fd05f5e9a9d685408f1aa123e1e4220ba3439`。

一个外部参数化驱动复用原完整Tag/Avatar及12真实图标尺寸检查，保留before运行副本，不反复切换产品源码。原三图标×12/32/37/67共12项合成器全部FAIL；候选12项全PASS，即时输出兼容另表精确比较，默认合成器SVG将按既定缩放修正，不能声称默认像素不变。Tag原主104/校准4/原溢出12、Avatar原主76/校准4/popup20两版业务均PASS。候选Tag Global清除四算法0，原本轮5/0/7/0；其他原Dark/DC历史非零不覆盖。Avatar open恢复本轮旧/候选仍12/max8、12/max7、0、0，没有修复AR1/AR2，原FW2调查及FAIL保持。

新增原生公开几何合同驱动七项：原版6PASS/1假编辑FAIL，原版热更新与重挂本来通过；最终候选7PASS，确认快照未破坏它们，不将其伪称新功能。外部辅助编译缺UseDesktopControls namespace已补，不计产品RED。统一Driver增加该独立模式，原Tag/Avatar/12图标操作逻辑不变；原v1 runtime与结果保留，不覆盖旧批。

最终源码两文件SHA冻结；同一候选正在一次AOT发布，后续真实执行、默认限定表、所有非零分类、临时清理及新的精确模块/NET8门禁仍必需。之前20.57s NET8不代表本候选；无新视觉批准或完整CT15结论。

## 一次冻结 AOT 与最终精确比较

AOT46808144字节，SHA256 `3bb622bb108f60861ab0185501b62ab5e361b35e09e7f365985acf52c6e0febd`，0 IL/CS警告，5条既有Homebrew最低OS linker warning单列。候选/AOT各12真实图标、Tag104+校准4+溢出12、Avatar76+校准4+popup20及原生几何7检查均PASS，六临时xUnit另PASS。两Core源冻结未变，未重复发布。

每版356图，候选/AOT353对RGBA0；三非零仍FAIL：Dark Avatar closed57/max7、DarkCompact closed44/max13、Compact Tag group-scroll4/max1。332份完整布局/状态sidecar旧新及跨运行均逐字同。旧新356对中20零/336非零；12即时图旧新全部0，而12合成器图为明确矩阵缩放修正，见[限定表](icon-geometry-snapshot-visual-review.md)，另列Tag/Avatar四算法8默认对。已查看真实UserOutlined67旧新、Avatar默认及完整Compact Tag图；不使用mask/crop/align/像素容差。

Tag从原源码可明确配对的52恢复（含独立校准/原大矩阵返回）本轮旧34零/18非零，候选及AOT各46零/6非零。剩余：Dark own-clear24/max1，Compact own-clear2/max1、native-clear5/max1，DarkCompact own-clear21/max1、native-clear7/max2、large-restore12/max1。Global-clear、输入/group恢复等虽改善为0，不把六非零计通过，也不覆盖其他历史失败。Avatar两open恢复依旧严格12/max8与12/max7。

比较脚本初次误将open-default→closed当恢复，后又尝试引用另一进程main默认；均不是原popup关闭前基线，已删除该错误配对，不作为产品失败或通过。popup模式原本没有保存pre-open默认，因此本轮只报告closed跨运行比较，原closed恢复账本保持。初次比较在AOT未收齐时的289对/缺67仅属进行中输出，不作为最终缺帧或通过；最终356图齐全。

最终只请求共同渲染规则修正及表内20个具体例证的限定批准，不将336旧新或其他未采样下游默认自动判为视觉通过。Core属于CT10共享基础，137/274与CT12/13/14既定批准不改写；新默认作用范围必须明示。临时清理、精确模块与新NET8随后登记。

作者另步自审：DrawingInstruction保留原矩阵乘法顺序、Pen与Opacity，不再修改共享源；几何Clone是已用于边界计算的官方现有机制。Icon只在连接视觉根首次绘制时取得源几何事件，弱引用通知当前owner，unload/detach双入口释放，不在静态指令缓存控件。六临时及AOT七项验证调用方几何、重挂、存活外部源下退出。StyleKey/属性注册/初始化顺序未变，未替换官方依赖或修改Scalar/CT11动效。可能改变全部DrawingInstruction消费者的默认SVG范围已明确告知，尚待视觉裁决；本自审非独立审查，不替代剩余失败。

Final gate:6 temporary cases/8421 bytes removed;2070 evidence files+manifest cleaned (audit clean,removed2071);external822 files/3233947432 bytes,all drivers/runtimes/publish/marker removed. Cleaned source focused Desktop78PASS(2s)+Common Avatar7PASS(373ms),new Desktop NET8 Release0warnings/0errors21.16s. Source freeze unchanged,member2files/1control0issues,scoped doc links and diff check clean. No permanent tests,full regression or commit. Visual shared-rule/default20examples still pending.
