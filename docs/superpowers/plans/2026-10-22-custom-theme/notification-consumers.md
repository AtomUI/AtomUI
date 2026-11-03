# CT15 Notification 实际消费者计划（2026-10-31）

3 metadata/17 Own，沿原目录feature/custom-theme单agent执行，无分支/worktree/提交；Card和Message限定审查等待期间继续本包。CT12/13/14关闭范围、既有CT11机制、CT25/FW1及官方12.1.4保持。17字段已有引用不等于实际状态/显示/释放验证。

## 合同与初始核对

NotificationCard拥有标题、原Content/Actions、Default与四状态、自定义Icon、关闭及运行期progress Part；WindowNotificationManager拥有稳定集合、位置/Stack/时长/hover/点击及释放，内部ProgressBar不虚构独立身份。保留384默认宽度、真实close22命中面、标题/描述/操作和可选择内容，进度覆盖层/滚动/裁剪/焦点/键盘不通过隐藏或移除触发条件规避。

Ruling：复用已有效的默认Notification图标数据工厂与CT11协调器/快照/计时证据；本包不因Message刚做过迁移而重复重写这段非binding逻辑。稳定模板状态绑定优先AXAML；动态manager创建卡片状态必须模板前有效，以明确owner获取/释放。Own17迁V2 Revision1，含原不可变双stop渐变，名称/type/key/slot/公式/常量不变，不改成纯色或丢掉底槽。

疑点先复现：Card detach清progress且将_layout置空，而attach不重新配置progress；重套模板清初始进度时长可能把剩余值当总时长；模板前apply后attach可能重复close事件；两manager状态订阅释放与原生清除；全局/字体与IconPresenter/close的独立公共identity需要显式Card owner。不得仅凭源码疑点修复。

## 执行顺序

- [x] Temporary/CT15Notification：17 Own、四算法68默认/type/key/slot、两owner字体/状态/close、专用八Part/manager list与运行期progress、Native优先、有效生命周期RED；实际close左右中/Enter和原关闭/时长合同。
- [x] 完整成员/初始化审计，纯移动tokens相同；复用有限字体继承，保留标题角色，正确获取/释放运行关系；动态Part仅在真实有限宽Grid跨列创建，不改为永久占位或丢掉原内容。
- [x] 真实Skia/正常Dispatcher、四算法/RTL/五Type/持久与有限进度、Own/Global/Native/专用Style/清除；新旧同历史，A/A与独立1DIP检错，当前Mac NativeAOT。检查共享锁，不调通用容差，不替换依赖。
- [x] 临时验证/专用工程/媒体清理，再按最终源码精确Notification/受影响共享模块、NET8、成员与文档/CSV/总表及作者另步自审。限定视觉后审查；完整CT15不按局部包关闭。

共享Stack原16/8/背板几何及真实Host/六位置与Message P2合并核对；完整Notification尚未验收，不把默认迁移当出口。


初始14为9FAIL/5PASS，4项标题/正文混用Native/AtomUI SelectableTextBlock的夹具错误已纠正，不能计产品RED；当前正确14为8FAIL/6PASS（3.4098s），其中17 Own及旧close假设已通过。新增动态启用progress需求用例为15中9FAIL/6PASS（3.9663s），证明manager原只有截止唤醒，动态打开进度不刷新调度。

13文件/3Control初始35成员问题，纯移动后0，逐成员token快照完全相同，静态注册及字段初始化次序保持。修复仅真实反例：保留同一有限生命周期总时长，模板/隐藏/重挂只释放视觉part，Template变更才清布局引用；重新attach恢复part；已挂载的progress需求变化复用manager现有Refresh，不在Show初始化中触发额外回调。动态manager状态两项有owner释放，稳定presenter状态迁AXAML。两owner字体/真实Global与独立primitive source接线，17默认V2含原immutable渐变且68记录相同。首轮44中43PASS/1过渡中间色，补正常Dispatcher循环350ms后44PASS；专用八Part/list、真实close左右中/Enter及三个WeakReference后完整54PASS（5.6662s）。保持已有非binding默认图标工厂、motion/快照/计时器结构，不改矩阵原数值容差或官方依赖。


本包执行/清理及精确验收见[结果](notification-consumers-results.md)：55验证，清理后33/NET8零/成员13-3-0；22临时/434工程/598取证清理。勾选只表示当前工程和记录切片完成，非完整Notification/CT15关闭；[36对及root有效值](notification-consumers-visual-review.md)待限定批准，两暗恢复4/max16与共享P2/原生最终面继续开放。


P2归属复核：[独立记录](feedback-stack-p2-ownership-results.md)以正式架构/CT10真实输入为准，16/8及背板产品几何不自动成为Global映射；原待核对线索不作为批准。37项精确现有几何验收通过，真实host/其余业务/像素/平台与完整家族仍开放。
