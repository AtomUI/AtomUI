# CT-11 动效总出口执行计划

> 单 agent / executing-plans，在现有目录和分支执行；先完成 CT11 后汇报，不创建分支/worktree，不提交或推送。

**Goal:** 完成实际 owner 的动态时长、Wave/Splash 与消费路径出口。
**Architecture:** 复用 TokenTransitions、OwnerTokenResource、原生优先级和时钟。断链先复现，再做最小根因修复。
**Tech Stack:** 官方 Avalonia 12.1.4、NET10 focused、NET8、当前Mac NativeAOT。
**Spec:** [原计划](motion-consumers.md)、[任务合同](tasks.md)、[消费规范](../../../engineering/development/theming/consumption.md)、[验收规范](../../../engineering/development/theming/verification.md)。

## 约束与审查重点

- 不重做已有 M1–M4 与已批准按钮连续动画切片；补足逐路径证据。
- 不能用资产物化或其他家族颜色/几何验收代替 CT11 动效消费。
- 内部节点须验证真实公开 owner、selector 和触发状态，不能以 standalone 节点冒充。
- 混合时长槽、Easing/Delay、原生 LocalValue 优先级、明确0与清除、释放分别验证。
- 保留 NavMenu 此前宽度恢复失败，不恢复被否决字段或框架补丁；本轮时长修复不改变恢复算法。
- 临时验证清理后精确模块验证，不跑 full/change；新可见差异单独交审。

## Task 1：已知时长旁路

- [x] 读取 S02/L11 和 M1–M4 证据，核对唯一未勾选总出口。
- [x] 源码扫描发现 NavMenu 整体宽度动画直接读取 Application 时长。
- [x] 临时验证局部Mid800、Control Mid1200、清除/Local优先级和真实收窄轨迹；夹具异常不计RED。
- [x] 若有效复现，用 AXAML 写入既有 MotionDuration 属性、消费端读取有效值；默认200ms/曲线/计时器/宽度恢复算法不改。
- [x] 同断言GREEN、默认前后视觉及原生AOT；具体配置差异交审。

## Task 2：逐路径出口

- [x] 核对 M4 的83集合、M2四模板及其他动效入口的实际owner、目标、状态、更新/清除、视觉及释放。
- [x] 共享Wave所有owner及持续动画开关补缺证，不以代表用例代替完整相关消费路径。
- [x] CT11断链依次闭合；其余平台明确留CT25，不迁移CT11本身缺口后标完成。

### 开关入口审计（2026-10-29）

实际 schema 中119个非抽象、公开、可无参构造且非TopLevel的动效owner，全部挂载真实Window与Provider、顶层true/该identity false；无夹具异常，73个不匹配。BorderBeam有既有独立装饰合同及常驻正对照，不能改变其默认Global边界；排除后118个、72个目标不匹配，须重跑确认。

根因候选：仍在读Shared资源的根/基类主题，以及Button/IconButton固定identity用于派生公开owner。拟对有效RED对应的既有开关Setter使用现有OwnerTokenResource，不新增运行时机制；其他动效时长Shared旁路分别先取得真实节点失败。保留Splash无独立identity的Shared合同、BorderBeam独立装饰合同、OTP等已有明确父owner接线。

NavMenu四有效RED/一默认正对照；首次候选仅接Shared后4PASS1FAIL（Control1200实际800），随后改NavMenuTokenResource，最终五项均PASS。原先只RunJobs(Normal)使真实timer未推进的夹具错误已修正为原常驻测试的Background/SystemIdle泵；该假阳性不算动效通过。

后续72个开关消费不匹配已由56个现有Setter文件修复：Shared入口使用OwnerTokenResource，Button/IconButton/ToggleIconButton继承主题的开关也采用真实identity。无新增属性/引擎；根审计118/118 false覆盖通过。反向true审计又发现8项，但源码表明是原本无Global开关消费者的native/composition API，不可把元数据有bool属性等同有Global默认：Tour两个indicator无循环动画，FormItemDecorator透传子控件，NavMenu两个header由模板传入，ScrollBarThumb由ScrollBar模板传入，UploadTrigger由Upload的Template优先级relay传入；SplitView原生显式开关默认false。保持这8项原有默认及Native API，不添加getter式假能力；它们另验默认false/Local true/false和实际父子链。原反向8个FAIL记录保留为审计预期过宽，不计产品故障或豁免。

代码时长13用例初次均RED。12项通过后，CollapseItem仍307ms而期望1300；源码/目录确认CollapseItem是独立identity，原断言错误地配置父Collapse。改成实际目标identity后回放其旧Shared主题仍有效RED，其他12PASS；随后恢复Owner候选。不得把这个错误归咎框架或移除CollapseItem identity。

### 跨根实证与当前候选

- Drawer、Overlay Presenter/Surface、Window Surface、Count/Dot Badge 六个真实打开/物化用例全部有效RED；五个主题时长入口接已知公开owner后5PASS/1FAIL（Window）。
- WindowDialog的逻辑owner是嵌套scope中的Dialog，物理OS owner是外层Window；原Window.Show(owner)丢失该scope。复用原有ThemeContextLease，Prepare优先已存在的逻辑owner context，再回退原物理owner，初始六项均PASS。未改OS owner、输入、尺寸或新建另一套lease。
- 加热更新/0/清除后Drawer及两个Overlay输入停在旧1200/1300，三项有效FAIL；其他三项PASS。
- 单变量实验仅把实际presenter/container加入owner原生LogicalChildren：两组“不加入”FAIL（800→456仍800），两组“加入”PASS。首次Drawer派生探针未登记导致准备错误，最终改为测试内反射访问真实原生集合，未改产品类型或注册；最终2FAIL/2PASS保留。
- 当前候选让Drawer/Overlay按打开/关闭对称加入/移除真正的LogicalChildren；不新增资源订阅、bridge或缓存。两个浮动Host原子按钮挂在外层Layer并丢失scope，原800实际200，两项RED；同样修正逻辑成员关系后作用域与热更新/移除/重挂均PASS。
- 浮动候选同时暴露位置回归：584,424变0,0（40×40仍在），两项明确几何RED已记录，不能以数值开关通过掩盖。根因是旧定位代码把逻辑Parent当视觉Layer。当前第二候选读取真正VisualParent，并在视觉attach/detach对称管理既有SizeChanged订阅；同一几何断言正在验证，尚不能宣称最终视觉通过。
- 只读模板探针扩展为121根/3762记录/96目标类/99个source style，已识别更多真正状态；仍须精确映射93组声明、补缺项与硬断言。此探针不是最终验收，不把零尺寸/外层窗口节点计作公开owner视觉通过。

后续出口：46项临时切片通过；三类DataGrid实际集合及开关2PASS、GalleryBase1PASS，Pagination两个实际icon及两个历史Indicator primitive补验3PASS。最新矩阵以source/selector精确匹配85组，四条独立包路径加Pagination/兼容两条达到92/93，本机Windows专用CaptionButton真实平台义务明确归CT25。

资源图GC覆盖8种关闭场景、33对象全部回收；当前Mac真实NSWindow的managed与arm64 NativeAOT实际运行各357检查/120 owner通过。外部驱动Opened重复启动的无效轮已终止，改为一次性事件后重跑；不得记作产品故障或有效通过。

成员审计发现存量布局问题后另步整理5文件；最终10C#文件/9控件/0问题，布局前后的完整token snapshot JSON严格相同。NET8三个产品出口均零警告错误；清理前6个图/交互namespace原有912PASS0Skip。候选明暗各522帧，基线回放当前在受控finally恢复事务内进行；未完成RGBA对比、媒体/测试清理、清理后验证前不关闭CT11。

## Task 3：结案

- [x] 清理临时源/helper/工程/媒体，最终精确模块验证、NET8、成员审计及diff检查。
- [x] 作者另步审查，更新证据与任务表；所有子项实际通过后才标CT11完成并汇报。

## 2026-10-29 结案

两份限定视觉结论均已收到，2261媒体文件清理且audit clean；最终源码SHA一致，13临时源与466原生工程产物已删除，同一最终树991项聚焦验证通过。作者另步自审无未决合同缺项，CT11完成；CT25及完整家族边界按总结果保留。本轮不提交、不推送，先向用户汇报。
