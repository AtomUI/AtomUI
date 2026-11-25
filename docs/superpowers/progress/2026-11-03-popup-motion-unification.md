# 执行记录 — plan: docs/superpowers/plans/2026-11-03-popup-motion-unification.md

## 授权与边界

用户：按更新后的文档开始重构，统一基类的 OpenMotion/CloseMotion，移除 DefaultMotionPreset 和 IsMotionAllowed；重点验证内存泄漏和动画性能；改完放暂存区，不 commit。
当前 feature/motion / 4513cfd2898c66d90e2d137f8ac8fc2d171f5598，95个已暂存路径保留。用户唯一未暂存源码改动为 PopupMotion.cs 删除未使用的 Avalonia.Controls.Primitives using，保持。
不创建 branch/worktree，不提交，不运行全量回归。

## 审计与决策

- 模式：已批准的 API 统一和架构根调整；不做额外视觉简化/成员重排/拆分。
- 状态 owner：公开组件实例配置、Popup 物理会话、Core 单目标播放；选择、编辑、滚动不迁移。
- 旧问题：预设属性跨主题/metadata/非视觉转发；Popup 同源全局开关镜像；四族基类缺公开配置；共享定义/任务可回收性未取得证据。
- 基类 API 共享已有 Popup 注册；不创建新接口或通用控件基类，TreeView 既有 null 回退不改。
- 获取/释放：模板关系由AXAML；可替换 Flyout 由注册租约；Core 全局策略观察由活动播放；native 输入/定位由物理打开会话。
- Ruling: 当前用户已经授权执行正式设计，直接 inline 进行，不重复设计批准。按项目覆盖技能的默认提交、隐藏工作区和新worktree要求。
- Pre-flight: Core 完成结果是 Popup 离场物理关闭的依赖；默认来源身份与 owner 转发是反向原点/用户覆盖的依赖；几何值模型必须先稳定再接模板/非视觉。
- Ruling: Avalonia TemplateBinding 不支持 OneWayToSource（本地依赖源码已核对）。如派生默认视图需要反向发布，使用显式 compiled binding；不以代码绑定回避模板关系。

## 任务状态

| 项 | 状态 | 证据 |
| --- | --- | --- |
| 1 基线与RED | 完成 | 3进程基线；API/Core有效RED10 |
| 2 Core策略 | 完成 | 活动请求局部观察；禁用完成终点 |
| 3 族API与默认 | 完成 | 13真实模板及绑定更新 |
| 4 非视觉/组合 | 完成 | 关闭通知重入RED2→GREEN2 |
| 5 内存/性能/发布 | 完成（范围见记录） | 内存/原生输入/固定tick成本/AOT发布通过；原生probe不稳定单独记账 |
| 6 清理/审阅/暂存 | 完成 | 一次整体审阅与Important修复完成；临时产物清理，改动暂存供用户审查 |

- 基线：现有原生 popup probe 3进程/各3warmup+10次开关/450ms完成，原始数据保留于系统tmp至最终对比；采样期间未构建和修改src。
- 有效RED：族基类/独立入口9项反射合同缺失，普通Border上Core动态全局禁用后Task仍未结束，共10项失败。前两次临时驱动编译错误（ComboBox命名空间冲突、缺少SharedTokenKind using）不计RED。
- Core修复后动态禁用精确1项GREEN，Core编译0警告0错误。复用现有Phase的Settling完成分支；订阅只随活动请求存在，SingleAssignmentDisposable处理同步初值，无新增开关或公开结果。
- 已删除全部src中的DefaultMotionPreset/PopupMotionPreset/IsMotionAllowed。默认来自只读几何快照及纯工厂，仍复用内置Slide/Zoom；内部默认反向仅创建私有副本。native包络先确认默认对象身份再处理Zoom，不按用户类型猜测归属。
- 主题接线编译通过，Desktop/ColorPicker/DataGrid各0警告0错误。CompiledBinding提供派生默认视图的OneWayToSource，真实配置仍owner→Popup；菜单层级分流用AXAML选择器，不增加转换器/模式属性。
- 编辑期构建纠正：批量匹配误触Popup.IsOpen属性元素已恢复并校验XML；继承Button的Flyout成员遮蔽类型已显式限定；显式CompiledBinding元素命中运行时类型而非扩展，改用现有语义选择器。均非功能RED，不以这些错误作为产品复现。

- 首批29项GREEN：9个入口共享注册、Core动态禁用、13个真实模板的默认/custom/null/ClearValue/关闭开关、编译DatePicker/Fade AXAML在closed/entry/exit/delay/reverse移除页面时5组weak refs、已完成Task和共享定义保留时目标回收。模块合批1583/1583（1554既有+29临时）通过，54s。
- 扩展33项时1项失败：保留DatePicker实例更换模板，旧Popup未回收。旧物理host/播放/逻辑挂载均已结束，排除未关闭和活动clock订阅；原测试保持，未放宽GC或清空DataContext。
- 临时字段图遍历获得第一条强引用：picker._pickerPresenter.Parent.Parent→旧Popup。模板Content子树的声明不在框架visual template retirement遍历中。Popup在TemplatedParent退役时，对仍属于旧owner/旧Popup的Content声明释放模板父关系；普通关闭不清内容。旧Arrow模板父已空、Content为空、Presenter已迁到新frame，但GC尚有另一条旧输入壳引用。
- 原测试恢复后仍失败；对取消clock初始队列的猜测已排除（queuedOld=0/activeOld=0；不保留额外clock推进作为规避）。临时BFS上限不足以穷尽，改为真实heap dump（系统tmp，约496MB）并用SOS gcroot查根，不增近似场景。
- heap根链包含新输入内容→复用CalendarOutlined图标→旧PickerClearUpButton/旧AddOn壳→旧Popup。InfoPicker模板固定关系原来由6个RelayBind在新模板装配后才释放，且跨Content模板节点保留旧关系。正在迁移为AXAML声明式属性绑定：清除模式、Icon、FormFeedback、可见性、ContentRightAddOn与Template；删除该Composite及方法。按主题优先强约束一并迁移，不创建新wrapper/控制器。
- 尝试过的owner级全量模板父预清理不保留：会干扰框架正常模板遍历且未通过原GC。当前仅保留Popup跨Content边界的精确释放，以及上述固定绑定迁移，等待原测试GREEN。
- Ruling: 本轮API补齐是已批准功能调整，不是将未测性能模式推广到更多控件；没有搬AXAML视觉到代码。性能回归仍为门禁，不宣称速度收益。
- Ruling: Gallery正式规范已删除API/Token表和sidecar，不恢复它们。新增API以源码/正式文档维护，只添加DatePicker最小自定义Fade与显式null实例展示，四语XLF同一source文本同步。

- 第二份当前源码heap再次确认原host已关闭，残余根从保留的DatePickerPresenter/DateViewer按钮经过框架Border缓存的CompositionBorderVisual.Parent链进入旧OverlayPopupHost，再由其PopupPositionRequest.Anchor指回旧模板Panel/Popup。Avalonia本地源码Border._borderVisual未在基类Visual.DetachFromCompositor时清空，与实际heap字段一致；不是Motion定义保存目标或活动帧订阅。
- 把清理仅放Surface.OnDetached仍未通过；内容可能先从surface摘走。正在核对模板退役顺序：InfoPicker模板改变先结束实际host，Popup在物理关闭前解除surface合成子关系，之后才解绑模板内容。公开composition API清理，不新增私有字段反射/视觉替身。需原测试最终GREEN，未通过前不计修复。
- 原未证明必要的IconPresenter条件修补已撤回，源码diff为空。头部固定6条RelayBind已按主题优先约束迁移为AXAML，原Composite及方法已删除。
- 当前弱引用用例仍为阻断项，未放宽断言/GC；额外验证渲染批次Rendered已完成，排除仅未送达render线程。heap与原始根路径仅留系统tmp，收尾删除。

- 一次独立整体审阅结束：无Critical，2项Important：按钮旧Flyout关闭通知重入破坏新注册（两参数用例均有效RED，latest.OpenMotion丢失）；旧Popup的GC阻断。其余属性身份/覆盖/默认来源/Core资源租约未见另一重要缺陷。不重复全审循环。
- 按审阅建议，所有未证明有效的Composition.Children.Clear、提前close钩子已撤回，PopupSurface与IconPresenter源码diff为空；不操纵框架合成图。当前保留单一Popup模板Content退役和固定绑定迁移。
- 对缓存前提的当前场景实测：保留presenter当前visual子树中305个Border已detached、GetElementVisual=null而_borderVisual仍非null。新专用Core反射边界仅命名这个field、验证类型、DynamicDependency精确保留，不扫描类型；模板退役物理关闭后、清Content前释放这些已失效cache，再次挂新root者跳过。下一attach按框架创建新visual；等待原GC单变量GREEN和原生/AOT，不预报成功。
- DropdownButton/SplitButton注册改为已有Host同类租约：先退役注册，保留定位到物理关闭，再释放内层转发/owner；按最新属性重新协调，单个binding获取后重入会由已退役租约立即释放。无新ignore布尔状态或逐帧成本。

- 最新当前源码heap（Border cache已归零）确认today command仅1个Button订阅且就在保留presenter当前visual子树，排除旧命令按钮退订缺口，不修改命令。根改为DatePicker.PropertyChanged→AvaloniaPropertyObservable→PropertyEqualsActivator→StyleInstance→旧InfoPickerTextBox→旧输入Panel/Popup。
- 因此InfoPicker模板退役仅补框架遗漏的声明：比较GetTemplateDescendants与全visual中TemplatedParent为本owner的节点，对差集退役模板父及逻辑父；正常框架可遍历节点交回框架处理，避免原先全量预清理的错误。先物理关闭，重入后核对Template身份。原GC仍作阻断验证。
- 单一Border缓存适配现已在保留presenter上命中，原场景staleCaches从305→294→0；这项进展不能单独证明Popup已回收。根字段保留DynamicDependency，任何挂载/当前composition存在的Border跳过；不扩大扫描其它框架类。
- 闭合注册重入两项有效RED修复后均GREEN；一次审阅指出的此项Important已修复，未发起第二次整体review。
- 成员审计对照既有暂存源码：基线96项存量，当前95项，无新增规则/成员问题；不为本轮API补齐大面积重排这些旧文件。临时审计库存只在系统tmp。

- 原换模板GC现在GREEN：框架逻辑卸载的InvalidateStyles只移除Style frame，旧输入框的TemplatedParentTheme仍有PropertyEqualsActivator订阅。精确兼容边界调用框架既有OnTemplatedParentControlThemeChanged释放退役owner声明上的frame；不改全局SetTemplatedParent语义。原测试已通过。
- 为收敛实现，撤回无效的GetTemplateDescendants差集/逻辑父手工清理；仅保留已证实的模板主题退役、Popup跨根Content退役及detached Border cache释放。去除堆转储暂停和诊断反射，原弱引用断言不变，重新合批验证。

- 去除差集清理后，原换模板GC及完整38项均GREEN（9.73s）；另2项400tick测试GREEN：Slide/Zoom内容新增Measure=0、Arrange=0，公开定义引用保持。13模板加入外部绑定源连续替换与null检查，均通过。
- macOS原生真实Gallery自驱动最终退出0：DatePicker新编译Fade示例点击入场9个中间值；替换模板后重新打开304×356日期面板；InfoFlyout原生host hover离场12个中间值，ToolTip原生host hover离场5个中间值，均物理关闭。Gallery仍运行时旧DatePickerShowCase弱引用回收。
- 输入驱动两处纠正属于验证驱动：FlyoutHost容器宽430、实际Button宽94，原指针在容器空白处；改在真实内容按钮中心；Tooltip限定当前ShowCase中的Button，避免取到窗口外层提示宿主。未改产品交互/尺寸/裁剪。原生RawPointerEvent走平台Input，hover进入事件确认到达。
- 即时渲染截图已查看：日期Fade中段、终态/重建模板后日期网格、InfoFlyout文字/箭头、Tooltip文字正常。RenderTargetBitmap仅判断视觉布局，不作为合成器截图或显示呈现FPS。1DIP扰动20,906差异像素，恢复0；媒体留系统tmp待清理。
- 采样曾2次在open阶段提前关闭，未输出合法测量结果；1轮、5轮和完整13轮诊断复放均通过，未得到归因证据。按调查收敛规则停止同类诊断堆叠，不修改产品掩盖交互；保留失败账。移除临时观测后以原始probe集中执行最后3进程采样，只有有效动作可用于对比。

- 原始probe最终3进程全部完成，60个动作均有11–12个中间值。但原早期基线回调median8.51/8.81ms，新采样16.66ms，样本实际节奏不同；CPU/分配下降不作为有效收益百分比。为消除这项混杂，仅在当前checkout保存源码恢复快照，使用已有index的前版产品源码紧邻重采基线，再逐字节恢复当前源码；不创建分支/worktree/隔离开发副本，不修改index。一次受控A/B后收敛，若仍不可比则明确保留性能缺口。

- 当前checkout前版重采第二进程也复现Popup did not settle，故原生probe间歇失败不是本轮API统一独有；失败保留账，不宣称修复了尚未归因的probe问题。finally已逐字节恢复全部源文件并验证index不变；前版重建SHA与原始基线完全一致（Controls 5FC4E4620ABB…、Core 23A31E6E817B…）。
- 收敛到固定时钟真实Popup成本：1秒MotionDuration、每2ms一次、400活动tick；每类型/方向5warmup+30测量。中点Opacity严格(0,1)，确保没有测到静态或已经结束的动画。早期只给定义Duration的驱动被Popup.MotionDuration覆盖，不作为400活动tick证据；驱动已纠正，并由最终40项及成本探针通过。
- 固定时钟每400tick分配（包含同一反射脉冲探针开销）：Slide开67,200B/关64,240B、Zoom开65,920B/关65,360B，前后逐项完全相同；内容新增Measure/Arrange均0。初稿A/A耗时约0.70–1.30ms，波动足以混杂差值，因此不宣称速度收益。
- 根据一次请求的真实分配进一步收敛Core：复用播放请求本身作为资源事件owner，直接观察ResourcesChanged/ActualThemeVariantChanged，finally成对移除；无额外Rx链、镜像状态或控制器，资源查询只在准备/资源变化时进行。首稿关闭准备6,072→当前5,744B，旧实现5,456B；额外分配616→288B，减少328B/53.2%。Slide打开64,392→65,704B（+1,312B）；Zoom对应基线64,512/5,576B，当前65,824/5,864B。增加发生在请求准备，不发生于每帧。复杂度负担：Core新增活动资源事件获取/释放及2个handler重载，无新持久字段；模板兼容增加2个固定成员边界，只在模板退役运行。
- 最终Core事件版临时40/40通过（9.73s）；随后删除整个Temporary/MotionUnification及工具驱动，确认最终测试DLL不存在MotionUnification/DatePickerMotionView。清理后精确模块：Desktop671（37s）、Core1、DataGrid4、Gallery10，全通过，无全量回归。
- 稳定源码NativeAOT osx-arm64发布exit0，产物validation passed；_borderVisual与OnTemplatedParentControlThemeChanged名字在最终原生产物中存在。没有运行AOT版换模板交互，不把发布通过升级为该项运行证据。已有ReactiveUI IL2104/IL3053、未修改DataGridBoundColumn.ReflectionBinding IL2026/IL3050，以及Homebrew dylib目标版本告警保留，无本轮固定反射边界新增告警。
- 最后成员审计95项（原96），按文件/规则/成员对照无新增；3条wrapper-order消息随新属性名单改变，仍是同一存量成员全局顺序提醒。新Open/Close注册与包装顺序一致，不扩张到旧文件机械重排。

- 收尾静态核对：31份AXAML/XLF XML合法，改动Markdown文件链接缺失0，git diff --check通过，src无已退出的3个配置名；临时工具diff为空。Debug Gallery当前源码构建0警告/0错误，已启动精确checkout路径进程，供用户review。

- 最终清理：证据工具audit clean，删除9份媒体及登记辅助文件；系统tmp本任务前缀131个文件/7个目录（含heap、日志、前后采样、AOT专用发布目录、恢复快照及只读审计副本）全部删除，剩余0。项目临时源码/驱动净新增0；常规开发构建产物及用户附件保留。当前Debug Gallery进程24280从本checkout精确路径运行，保留供review。
- 交付边界：保留当前feature/motion与HEAD4513cfd28，不创建新分支/worktree、不commit/push；仅暂存授权改动。功能/回收/固定tick成本与本机发布证据不替代Windows/Linux或显示呈现FPS，也不声称原生probe间歇关闭已解决。

- 暂存核对：124个授权路径，二进制改动0，未暂存/未跟踪文件0；HEAD保持4513cfd2898c66d90e2d137f8ac8fc2d171f5598。仅暂存，无提交。

- 最终过滤范围复核补齐fixture复数/Control命名差异：TreeSelectControl、Popups、TimePickers、DatePickers、SelectControl、DropdownButtonControl、SplitButton及直接相关浮层/Motion用例，无重复前批范围，696/696通过（26s）。清理后Desktop合计1,367，连同Core1/DataGrid4/Gallery10，总计1,382项通过；未运行全量。原671项记录为第一批，不作为完整模块覆盖宣称。
- Debug Gallery启动验证后会话正常exit0，PID文件已由launcher清理；不是崩溃退出，交付时不声称该进程仍在运行。源码、测试程序集与暂存内容未因运行改变。
