# CT14 容器与导航家族工程出口结果

2026-10-30，现有`feature/custom-theme`及`/Users/chinboy/Projects/dotnet/AtomUIV6-custom-theme`；没有新分支/worktree、子agent、提交、推送或全量回归。当前为**工程、清理、清理后精确验收及限定视觉批准全部完成；CT14在既定批准范围已完成并关闭，CT25/FW1独立保留**。

## 完整范围及实现

[85项核对矩阵](container-family-exit-matrix.md)包含37实际公开身份及抽象/内部/布局/数据/委托边界，Own共258项：221 Active、13 Derived、20 Reserved、4 Legacy；无NeedsDecision。既有已完成默认分包不重复，CT12/CT13既定批准及Cascader Own上限/更小页数预算合同不改写。

补齐ListBox/ListView、TreeView、Menu/NavMenu、TabControl/TabStrip、Transfer、Breadcrumb/Pagination/Expander/Segmented及Cascader视图组合的Global/字体。219处原Shared引用在36主题按实际模板owner接入，跳过中间primitive并保留公开派生identity及Template优先级。25个控制类的只读继承桥支持原生继承，不新增字号/字体状态旗标。

Menu41/NavMenu68使用V2 Revision1，10条实际默认依赖保持显式下游优先，明暗218默认/type/key/slot不变。补齐12个原未消费的顶层normal/open、横向hover/selected、圆角、popup间距及背景easing；easing通过现有TokenTransitions的Control owner绑定复制，不在无host TransitionBase直接订阅。3个额外Legacy保留名称/type/ID/default/配置往返，不给重复输入建立别名。

ContextMenu复用ThemeContextResourceBridge，补只有parent而无逻辑子成员时的局部资源发布；挂载接入、上下文更换替换、卸载移除并Dispose。17个Custom分支退出预设activator，保留基础默认及三档；视觉回放发现的Small Pagination间距意外变化已有效RED→恢复0，Custom基础8。禁用横向selected前景优先及过滤disabled正文补齐；选中标记修复真实IconTemplatePresenter选择器。焦点/键盘/选择/展开/滚动/裁剪仍由原流程管理。

## 验证与原始反例

- 字体根32实际RED→GREEN；17公开子控件15RED/2正对照→17PASS。真实文字节点16类、owner回收16类、ContextMenu换owner/重开/GC共33PASS。
- Menu/Nav V2及五根有效Global、Own依赖，旧同断言10FAIL/2PASS→GREEN；Expander曾误观测根属性，已改实际frame并重新证明，错误画刷类型/夹具编译失败均不计RED。
- 12个剩余Own实际反例已修复；hover原坐标不命中被否决，保持原D.Window与尺寸，绘制后转换到真实中心并断言IsPointerOver；精准撤回消费2FAIL/2PASS，恢复4PASS。123临时联合PASS10s，Small间距额外1RED→1PASS716ms。
- 12套真实编译专用Part Style命中关键Setter并在class退出后恢复，未使用Name/Loaded/Unloaded代码回退；未将FW1直接Styles.Remove失败写成“不支持”。
- 当前Mac实际NSWindow/scale2，最终managed556 / NativeAOT557；218资源记录逐条一致。37公开身份、raw点击（先证明actual hit几何）、viewport/extent、OverlayPopupHost/PopupRoot、Own easing热更新及编译专用Style通过。旧547成功及后续一次未就绪点击失败、一次CVDisplayLink -6661启动失败均保留；最终驱动只补有界就绪判断、不重试输入。
- 官方Avalonia12.1.4未升级或替换。发布使用既有openssl/brotli路径；五个macOS26 dylib与最低链接12警告保留，只证明当前macOS26.6.2，不代表macOS12兼容。
- 完整视觉原/新v1/最终v2各480PNG；精确回放后78产品源hash全部恢复。最终480旧新对245零、235非零；AA/恢复原始FAIL及320组1DIP正对照见[限定审查](container-family-visual-review.md)。Small Pagination恢复原默认0，不保留意外8版本。

## 清理后的最终输入

18个临时源/AXAML及专用验证数据共52,564 bytes已删除，常驻测试净增0；仅更新6项既有Nav默认入口及3项既有源码合同断言。清理后真实重建：

- Desktop精确13家族filter：**1014PASS / 0FAIL / 0Skip，32s**。首次1011PASS/3旧源码断言FAIL原样保留，修订V2/owner断言后精确3PASS，再以最终清理输入通过1014。
- Core精确桥/资源/identity/状态合同：**17PASS / 0Skip，347ms**。
- Desktop NET8 Release：**0 warning / 0 error，44.05s**。
- 完整成员审计29文件、24带注册节点（其中1为markup helper）、0问题；结构整理逐成员Roslyn token与类型头指纹相同，初始化依赖复核仅三个const前向引用、无运行期初始化倒置。检查器同名显式接口属性导致的wrapper索引崩溃已修复，保留独立接口检查。
- 1536 PNG、1440几何文本及manifest共2977取证文件清理，媒体audit clean。外部native工程335文件 / 1,227,884,905 bytes已删除；所有本轮进程结束。最终源备份、inventory及临时工具在文本结果保存后清理。

## 作者另步自审与边界

按用户禁止子agent要求，完成作者另步自审，非独立评审。复核ControlTheme/Template优先级、公开/内部owner、Own依赖与0/透明、非Visual宿主、桥接事件释放、重挂/重模板/回收、S/M/L/Custom及原输入几何；未发现新增必须保留的临时helper/状态旗标。无新API/key/identity/Part重命名，既有Gallery API形态不变。

裁决：沿指定目录/分支、不执行技能的默认隔离/委派/提交步骤；过程记录仅Markdown。Legacy三字段以实际替代职责归类、无别名。当前Mac资源/输入不代表其他Host原生最终像素。FW1原coercion/Style退役及Tab指示条基准恢复、CT25原始历史与本表未认定通过的帧继续开放，不扩大容差。无另行搁置的minor；本次[限定视觉表](container-family-visual-review.md)已获明确批准，既定范围内的人审门槛已满足；不据此关闭CT25/FW1。

最终保护复核：Cascader输入主题/装饰壳、TreeSelect输入主题、Button代码/主题、CompoundInputTypographyBindings及ControlTypographyBindings与会话开始SHA相同。最终78产品文件聚合SHA：`1c5f50d887d54cc0b630db4cc3e60e54f642404713d5e88398ac52c9f51dff34`。`git diff --check`通过；计划目录沿既有gitignore本地保存，不擅自改动忽略规则或暂存。


## 2026-10-30 最终批准与关闭登记

用户通过 `request_user_input_async / call_656cee76f0f3440cb7dfe8d373826c6d / item 0` 明确回复：**“批准本表限定范围，保留 CT25/FW1”**。本次 V1–V7 限定视觉批准与工程、清理、Desktop1014/Core17、NET8零警告及当前Mac managed556/AOT557证据已对齐，CT14在既定批准范围完成并关闭。

原始像素失败、TabStrip指示线复位、coercion/Style退役及其他既定CT25/FW1边界保持，不将其记为产品修复或平台像素通过，不扩大容差、其他字体/配置/Host/平台。CT12、CT13和Cascader既定高度批准保持；本次批准不授权Git提交、推送、发布或依赖替换。仅同步文字状态，产品源码不变，沿用相同最终输入的已完成验收。
