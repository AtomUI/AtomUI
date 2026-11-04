# 执行记录 — plan: docs/superpowers/plans/2026-11-02-popup-motion-implementation.md

## 范围与基线

- 当前目录/分支：AtomUIV6，release/6.0；基线 4513cfd2898c66d90e2d137f8ac8fc2d171f5598。
- 用户本轮授权：开始全面重构，特别注意动画性能。先前 58 份文档改动保留。
- 不创建分支/worktree，不提交，不运行未授权全量回归。
- 默认接入是行为修复；性能比较使用同效果显式动画基线，不能把无动画的旧实现当作同工作负载。
- Ruling: 按当前明确研发授权直接执行本计划，不重复索取实施批准；采用 inline 执行与最终独立审阅，避免共享 Popup 接口被多份并发实现反复修改。
- Ruling: 覆盖用户的当前目录/分支规则，技能默认隔离、提交和隐藏目录脚本不适用；计划与唯一记录手工维护在 docs/superpowers，临时驱动在测试 Temporary/ 或系统临时目录。
- Pre-flight: Tasks 3/4 消费 Task 2 的规格与默认对；Task 5 继续使用已有请求身份，不能新增并行关闭状态机。Task 4 的派生默认视图只读，公开 Motion 只向实际 Popup 单向传递。

## 任务状态

| 任务 | 状态 | 有效证据 |
| --- | --- | --- |
| 1 原始入口与基线 | 完成 | 12 项真实主题 RED；2 项显式同轨迹基线通过 |
| 2 共享规格/几何/主题默认 | 完成 | 几何、优先级及实际模板 RED→GREEN |
| 3 模板入口 | 完成 | 真实模板/容器复用与原始入口已通过，范围 Picker 用共享 selector 覆盖派生模板 |
| 4 非视觉/Flyout/Tooltip/可选包 | 完成 | Desktop32 + DataGrid2 GREEN；三个拥有项目构建0警告0错误 |
| 5 生命周期与性能收敛 | 完成 | 请求/取消/反向与原生包络、真实时钟和hover均有证据；性能只报有效观测 |
| 6 清理与最终验收 | 完成 | 原生/hover/性能、AOT、最终定向验证和清理均已完成；未覆盖平台明确保留 |

## 验证记录

- RED：真实主题参数化驱动 12/12 失败，覆盖原始 11 条入口及 AutoComplete 对照；9 个模板入口/Flyout/ContextMenu 缺默认定义，AutoComplete 轨迹为现有 ZoomBig。命令：dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore -p:BuildProjectReferences=false --filter FullyQualifiedName~Temporary.PopupMotion.PopupDefaultMotionTests。有效运行 11.8882s；此前临时驱动同名 namespace 编译问题已修正，不计为 RED。
- 默认接入修复前的源码仍未修改，接下来运行同轨迹临时生命周期/分配基线。

- 基线（真实主题 Headless Overlay，400 次受控 tick）：SlideUp/ZoomBig 的实际内容 Measure/Arrange 均为 0/0；LayoutUpdated 通知均51，说明通知数不能当作真实内容布局次数。含反射时钟/Dispatcher 的线程分配为 6,853,928 / 6,859,096B，不能称为 Motion 自身分配或显示器 FPS。
- Ruling: 基线的 LayoutUpdated 非零来自观测层通知，改为同时记录通知与真实内容 MeasureOverride/ArrangeOverride，不把通知误判为动画重新布局；同一驱动用于后测。
- Task 1: complete。原始测试仅因控件/命名空间重名做驱动修正，不改变失败断言或控件触发条件。

- Task 2：几何最小接口 RED 12/12（NotImplemented，仅用于先编译契约测试），实现后几何12项及共享 Popup 默认/自定义/null/ClearValue 共13项 GREEN，Desktop 构建0警告0错误。
- 配置默认使用编译 AXAML 绑定，几何相等直接复用定义；未引入 Core API、额外时钟或逐帧几何订阅。增加 TooltipZoomBig 内部规格只区分全局圆角 Token，轨迹仍共用 ZoomBig。
- Task 3 首批仅接入 ComboBox/Select/Cascader/TreeSelect 四个模板，再验证原始场景和性能边界，避免无验收批量扩散。

- 首批四控件真实模板 GREEN 4/4（Select、TreeSelect、ComboBox、Cascader），原失败断言不变；编译0警告0错误。
- Ruling: 四控件批量审查属于默认行为接入，不声称加载提速；未移动 AXAML 视觉、未增加逐帧订阅。接下来用同显式轨迹基线复核共享播放成本，再推广相同声明式规格接线。

- 模板/owner 集成 GREEN 15/15（含原始11通路、AutoComplete、默认覆盖、菜单容器复用、Host自定义配置）；Desktop编译0警告0错误。范围 Picker 不是复用基类 Template，而是替换 Template，故以 InfoPickerInputTheme 共享 selector 接入全部派生控件。
- 额外 RED：Host attach 将用户 FadeIn 覆盖成ZoomBig；Tooltip 真实Popup为200ms而非100ms；ColorPicker/GradientColorPicker的placement改动重新启用显式null；DataGrid菜单/树筛选Top放置仍固定SlideUp（各2项）。这些不是测试驱动错误。DataGrid测试项目首次缺assets，完成精确项目restore后取得有效RED。
- Ruling: 保留 AbstractColorPicker.SetupPopupProperties 既有 protected virtual 扩展钩子及调用时机，默认体只说明主题职责；不为避免空钩子而添加无意义刷新或破坏派生API。
- 共享播放复测：400tick，Slide/Zoom内容Measure/Arrange仍0/0，LayoutUpdated仍51；含探针分配6,875,848/6,858,336B，仅受控smoke，不声明速度或分配改善百分比。

- Task 4 GREEN：Desktop 临时契约32/32、DataGrid2/2；Desktop/ColorPicker/DataGrid 拥有项目均0警告0错误。Tooltip真实100ms、ColorPicker显式null、DataGrid Top方向与既有原始入口均通过。

- Task 5有效RED：组件关闭开关未传至ComboBox/Select/TreeSelect实际Popup；直接Popup入场/离场中禁用不收敛；稳定态改变placement后离场仍用旧定义；真正false→true反向切换原点；Closed回调更换Flyout导致过期注册（再次ShowAt抛RequestedPlacement空值）；全局EnableMotion在组件local true时不停止当前播放；入场排队前关闭仍启动可见离场。
- 已通过的重入契约：同目标透明准备重入保留新租约，不新增Subject/准备框架；400tick默认定义未重建，自定义Origin保持。
- 下一修复保持一个关闭状态机：布局后读取离场定义、用现有版本退役播放、默认反向保留活动原点；用可观察关闭阶段在AXAML声明Overlay离场输入，不加代码样式回退。

- Task 5第一轮修复后全部53项临时Desktop契约GREEN（含10个父级禁用、11个生命周期/策略/输入契约），Desktop构建0警告0错误。全局策略独立于public local开关，由PopupTheme绑定内部IsMotionAllowed；无新增Core接口/每帧订阅。
- 关闭状态只发布布尔派生只读值，Overlay用编译AXAML绑定声明离场命中。默认反向仅在原点变化时创建私有播放定义，未修改发布默认或用户对象。
- Host重入使用资源注册租约：先退役租约，再物理关闭，最后释放定位转发；关闭回调期间的Flyout替换在释放结束后按最新属性登记。新增1个嵌套Composite与清理委托/注册，不增加动画tick对象；静态映射取代逐条注册逻辑，保持原转发优先级。
- 待验证原生包络：alignment原点可能位于表面外；正式设计要求真实需要时扩展透明缓冲并保留屏幕内容位置。现未实现，不能由Headless通过覆盖。

- Host→独立Flyout恢复另有RED：RequestedPlacement释放后原Placement仍为Custom。改为对Placement的metadata coerce投影，不备份/覆盖用户基线；释放Requested后恢复Bottom并可再次独立Show。此后临时全部+既有PopupLifecycle/FlyoutPinned/ThemePopupInheritance合批89/89 GREEN，构建0警告0错误。
- 成员审计首轮11文件/10控件187项，集中在方法/接口顺序、内部API region、wrapper顺序及直接属性字段邻接；单独执行机械整理并比对成员token快照和初始化依赖，不改变方法体或扩大拆文件。

- 机械布局整理完成：11文件/10控件审计0问题；前后imports、类型头、enum与逐成员token指纹完全相同。静态映射14个初始化依赖顺序保留；未修改任何方法体或新增拆分文件。临时Roslyn库存/比较JSON在系统tmp，收尾清理。
- 原生脚本使用真实InfoFlyout Click Showcase与已有NativeMotionProbe；新临时scenario通过同实例static→close→animated→close比较NoArrow/NoShadow/Margin20，不使用Computer Use/坐标点击。首次诊断Popup属性反射仅NonPublic未命中已修正，不计作产品失败。

- 原生有效RED：真实Gallery InfoFlyout Click，Avalonia.Native.PopupImpl，RenderScaling=2，无箭头/阴影、Margin20；同实例static与animated的内容屏幕坐标均1235,511，客户区216×116 DIP，实际ZoomBig原点12,-10，起始绘制minY=-2 DIP（4px越过物理窗口），已观察中间透明度。
- Ruling: 通过native模板标准PART_ContentPresenter暴露PopupSurface.Margin为框架positioning Deflate，使用现有透明缓冲协议保持内容坐标；增加1个轻量静态ContentPresenter（非动态视觉/替代内容树），而不是修改业务Margin、钳制参考原点、增加Core接口或私有反射。代价为native物理宿主一次性节点与绑定，需验证原生/样式/释放。此为依据实际裁剪对原先“无需额外wrapper”假设的必要修正。
- 首次接入该静态Content边界使旧TemplateBinding无法跨Content子树，真实内容不入视觉树；正在用显式编译逻辑祖先绑定恢复同一PopupRoot.Content。驱动未改为跳过表面，原原生场景保持不变。

- 原生包络 GREEN：同场景 PopupImpl/scale2，静态与动画内容坐标同为1235,511；原客户区216×116增为216×118 DIP，Surface(0,2,216,116)、原点(12,-10)，起始minY从-2变0。保持同样内容/箭头关闭/无阴影条件。额外固定Height=150的原生RED内容148/client150，约束补偿后GREEN内容150/client152，未移除高度约束。
- 最终独立审阅：发现2项Important，无其他阻塞。有效RED为Completing通知重开后IsOpen=false、ContextMenu取消Closing尚未触发；Flyout必须显式IsMotionEnabled=true后有效RED（原测试遗漏开关时通过，不计RED）。修复为现有关闭状态机增加同步Requesting询问owner，取消保留表面和子菜单；Completing保持到物理Closed。追加重复Hide→Show同目标的有效RED（第二次Hide提前拆宿主），修复复用既有退出与反向入口。没有新增并行状态机或每帧订阅。
- 首轮合批96/96 GREEN（包括4个native包络数学用例与3项审阅回归）；随后追加重复Hide/同目标Show与ContextMenu快捷键单次Closing合同，等待最终合批。
- 原生像素边界校准：固定屏幕ROI A/A差异0、MarginToAnchor+1 DIP差异35,962像素、恢复差异0。首次主对照因透明窗口后方其他应用内容变化导致全图差异，不能据此判产品失败或通过。保留该记录；主对照改用同API截取本进程原生窗口透明PNG，另核对物理窗口位置与全局内容坐标，不对图片裁剪/对齐/遮罩。

- 用户实际崩溃反馈：附件报告进程AtomUI.GalleryPerformance PID96159，2026-11-02 21:36，SIGABRT；对应本次native-trace已写COMPLETE后退出时的未处理CustomPopupPlacementCallback缺失异常，不能将COMPLETE标记当进程成功。调用链为Window detach→FlyoutHost释放绑定→Flyout生命周期关闭未强制结束物理离场→仍挂载Custom Popup重新定位。修复在强制HideCore及CloseForLifecycle先按物理会话关闭Popup，再清理Flyout/定位绑定；使用已有_hasRaisedOpened只读IsHostOpen，无新增状态字段或每帧成本。原4阶段相同native脚本完整重放，最终exit0；截图中间帧和静态/终态同时通过，崩溃故障保留为有效RED。
- 原生独立窗口RGBA验收：A/A=0差异；静态对动画终态=0差异（576×376 backing px），物理窗口位置同为1133,306、288×188 DIP；中间帧对终态158,024差异像素。已打开中间帧/终态检查，内容、圆角、箭头均保留，未裁剪、对齐或遮罩图片。此前固定globalROI的1DIP检错仍单独有效；主图透明背景从外部桌面隔离。
- 真实时钟InfoFlyout/原生PopupRoot三轮开关：6个动作全部有中间值，入场观测19/18/24点、离场10/14/25点；默认ZoomBig/200ms，全部正常退出。首次第二轮丢中间帧实际为驱动保留已销毁旧Surface（actor??=），已改为每次Opened获取本次真实Surface，不修改产品时长或轨迹。
- 清理后首次受影响范围1554项：1550通过、4失败。两个disabled夹具把开关放在内部容器/relay而不是公开配置owner，修正为Menu、DropdownButton owner禁用后原即时关闭断言保留。两个RangeDatePicker快速重开确认测试在Slide过程中用静态布局坐标点击；实测clicks=0、pressed=false、实际hit=LightDismissOverlayLayer，匹配当前composition变换位置（布局TranslatePoint不含该变换）。保留快速重开并使用既有ManualMotionClock推进终态后执行原物理点击与日期断言，不关闭动画。相关8项合批通过；没有改Picker业务源码。
- 用户追加hover验收。真实Gallery InfoFlyout Hover原生框架raw input经当前InputRoot与真实hit-test：进入、内容内保持、离开、再次进入、退出中移回、最终离开6阶段全部通过，Opened/Closed计数依次1/0、1/0、1/1、2/1、2/1、2/2，114次中间透明度变化，exit0。未调用IsOpen或ShowAt驱动hover，原delay/内容/箭头/阴影保留。输入是框架raw管线，不宣称OS物理鼠标验证。
- Headless追加hover驱动3项因IsPointerOver未到达断言失败，校准坐标后仍无法有效触发；该分支按连续3次限制停止，不计作产品RED/通过。改用已经校验输入身份的真实Gallery原生raw管线验证Tooltip/Overflow及菜单，避免继续搭近似模型。native初次IInputRoot直接cast失败为Avalonia12源API差异；临时探针读取实际TopLevel.InputRoot后已能命中，不新增产品反射。
- 真实Gallery Tooltip hover：default ShowDelay保留，ZoomBig/Fast100ms、PopupRoot，入场/离场累计21个中间值，状态和正常退出通过。OverflowTip与菜单补测进行中。

- OverflowTip真实Gallery锚点固定100 DIP溢出，沿用默认1200ms show delay；通过raw hover打开PopupRoot，ZoomBig/100ms、18个中间透明度变化，进入/离开与exit0通过。Tooltip对应21个中间值。临时改变只属于OverflowTip专用场景，未当成原始InfoFlyout或Tooltip对照。
- NavMenu顶层hover发现有效产品RED：折叠Gallery导航根项收到raw移动且IsPointerOver=true，但不打开。现有DefaultNavMenuInteractionHandler.OpenWithDelay仅允许“非顶层且父Popup打开”，与正式Horizontal/Vertical hover合同冲突。单变量改为顶层可打开、嵌套仍要求父Popup有效；同一原生raw场景GREEN：200ms ZoomBig、34个中间透明度变化、进入打开/移出关闭、exit0。没有改延迟、输入选择或Inline行为。新增源码仅1行条件，不做该旧文件成员重排。
- 清理原临时测试后受影响Desktop范围1554/1554通过（58s，0跳过），新增NavMenu单行修复后该模块310/310通过（18s）；DataGrid清理后14/14通过。未运行全量回归。三个已有fixture文件仅校准配置owner/输入采样时机，未新增永久测试。
- 最终成员审计14文件/10控件0问题；NavMenu handler仅方法体单行变更、无声明布局变化。正式文档本地链接首轮487个无缺失。
- 最终原生性能使用现有run_native_motion.py，scenario popup、3独立进程、各3 warmup+10 measured opens/closes、450ms动作窗口；没有并行自己的构建/测试。无同工作负载native旧版基线，不报告原生提速百分比或display FPS。

- 最终原生性能结果（仅当前成本smoke，3进程/每方向30动作，macOS26.6.2、Avalonia12.1.4、scale2、1300×900）：60/60动作均有真实时钟中间值且进程全部exit0。open/close的Processed间隔P50=8.3314/8.33455ms，P99=13.7737/16.2397ms；Rendered P99=17.194/20.7034ms、max=60.8129/22.0065ms。包含观测的UI分配P50每open482104B/close156872B、进程分配599856B/261824B；CPU P50每450ms观测窗口89.3095/75.44ms。框架layout pass每open3/close2（动作准备与收尾），layout duration P99=5.3093/0.0259ms，不等于内容逐帧布局。
- 限制：Rendered有一次60.81ms长间隔；未采display present、未冻结最低设备/刷新模式、无同效果native旧版成本对照。不能声明正式60/120FPS达标或原生提速百分比；上述CPU/分配含框架、页面与探针，不能标为Motion自身成本。未为改善数字缩短200ms、去阴影、去内容或改成静态。
- 临时测试已全部删除；tools/performances三个临时注入文件已精确恢复，git diff为零。保留3个既有fixture修正、0净新增永久测试。NativeAOT发布进行中。

- NativeAOT已进入native codegen；诊断如实保留：ReactiveUI/ReactiveUI.Avalonia的IL2104/IL3053，以及当前HEAD原有DataGridBoundColumn.CreateOneWayBinding对ReflectionBinding的IL2026/IL3050（文件本轮diff为空，HEAD同一构造调用存在）。本轮Popup默认动效源码未产生新增对应诊断；不因scope外告警宣称整个项目AOT零警告，也不顺带改列绑定实现。

- NativeAOT最终exit0，发布脚本restore资产与osx-arm64输出校验均通过，file确认Mach-O arm64。已有DataGrid/ReactiveUI诊断如上，另有本机Homebrew dylib部署版本高于macOS12.0目标的ld warning；不声明零告警或旧系统运行通过。未对AOT二进制进行新一轮GUI启动，Release真实宿主运行与AOT发布证据分开。
- 最终清理后再次精确验证Popup/Flyout关闭、原4回归及NavMenu交互合同54/54通过，不叠加计算重复测试。恢复后的性能工具重新构建，--native-motion --help仅有原有7类场景，不保留临时输入/截图探针。

- 收尾验证：精确Popup/Flyout/原4回归/NavMenu交互合同54/54通过（4s），无跳过；恢复的性能工具构建0警告0错误，help只含原有7类场景。未再次扩大/重复1554项范围。
- 清理：evidence.py审计findings=[]、status=clean，已删除本次全部登记媒体运行目录；已删除全部任务系统tmp源码、Swift辅助器、JSON/日志/布局审计库存、性能原始数据与专用AOT发布目录，共123个任务路径。原有常规开发产物保留；用户附件与系统崩溃报告未删除。仓库不保留媒体、原始日志或临时探针；0净新增永久测试，tools diff为空。
- 交付边界：默认规格及正式控件文档已同步，按当前分支工作未创建branch/worktree、未提交。没有Windows/X11/Wayland新动效实机或display present/最低设备帧率认证；不宣称全量回归、跨平台全部通过、零AOT告警或速度百分比。

- 最终文本/作用域检查：60份变动文档共488个本地链接均存在，git diff --check通过；工具源码diff为空、Temporary/PopupMotion与登记媒体目录均不存在。当前分支release/6.0、HEAD仍为4513cfd2898c66d90e2d137f8ac8fc2d171f5598；94个变动路径包含原先设计文档，未创建分支/worktree或提交。
