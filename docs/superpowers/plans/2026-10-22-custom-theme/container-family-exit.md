# CT14 剩余家族实施与出口

> 使用 executing-plans，现有 `/Users/chinboy/Projects/dotnet/AtomUIV6-custom-theme`、`feature/custom-theme`，单 agent、不创建分支/worktree、不提交。

**Goal:** 完成 CT14 尚未闭合的真实消费者与家族验收，保留已批准合同和 CT25/FW1 原始边界。
**Architecture:** 沿既有 ControlTheme、OwnerTokenResource、ControlTypographyBindings 和 V2 默认机制接线；公开子控件保留精确 identity，内部节点追溯实际 owner。
**Spec:** `container-consumers.md`、主题消费及验收规范。

## 已有结果与边界

85 metadata / 258 Own 为核对输入，含抽象、布局及内部节点，不虚构85公开owner。除 Menu41/NavMenu68 外的既有默认分包保持；HorizontalMargin 已按 Legacy 处置。Cascader 高度为 Own 上限与更小页数预算，限定视觉已批。CT12/CT13 已关闭，本轮不重做或改其结论。

NavMenu 首次折叠/展开原15项反例和框架12.1.4 coercion问题继续按原记录；不接收被否决镜像方案，不升级/修改依赖，不读取私有属性存储。其他平台、原生最终像素及精确历史差异仍按 CT25/FW1 单列，不设通用容差。

## 执行任务

- [x] 1. N2 Global与字体：先临时失败证据、相同历史默认基线，按列表→树/Transfer→Menu/NavMenu→Tab→Breadcrumb/Pagination/Expander/Segmented核对实际节点；Cascader/TreeSelect复用CT13已关闭链，仅补CT14独有列表出口。
- [x] 2. N3 余下 Own：Menu41/NavMenu68 V2及实际Own依赖、Active/Derived/Reserved/Legacy核定；保留字段/ID/type/默认/下游覆盖/0/透明。
- [x] 3. N3 Custom和Semantic Part：三档/方向/基础/实例及专用Style覆盖撤回、实际生成容器与跨根命中，修复断链，核对直接Styles.Remove既有问题。
- [x] 4. N4 状态与生命周期：实际焦点、键盘、选择/展开、滚轮、viewport/extent/裁剪、关闭/换父/重模板/GC，逐目标覆盖，复用已有测试；当前Mac managed/NativeAOT及NET8。
- [x] 5. 默认/配置/恢复脚本化像素与限定审查：A/A、1DIP、同实例完整历史、明暗/密度/尺寸，原失败不覆盖；只批准具体范围。
- [x] 6. 删除本轮临时源/工程/媒体、精确模块最终验收、正式文档/CSV/总表同步及作者另步自审。

## 预检与裁决

- 共享接口：N2使用CT10/CT13既有Owner与字体机制，N3依赖有效Own需CT06 V2；N4使用前三项最终代码。已有迁移不重复。
- Ruling: 遵守用户指定当前工作树、禁子agent/提交；技能默认隔离、委派、提交及隐藏目录脚本不执行，过程记录放本目录，临时工具在系统临时目录。
- 构建前已检查另一custom-theme会话空闲；每次构建/测试经checkout OS锁串行执行，锁被占用则等待，不覆盖采样产物。

### 2026-10-30 工程记录

- 16根字体族16实际RED、扩充可选字号后32RED（1s）；根字体32GREEN（1s），保留继承/Local/清除同实例合同。前两次fixture缺少Configuration命名空间的编译失败不计RED。
- 6根边框Global + Menu/NavMenu V2协议8实际RED（953ms）。Global根主题开始按精确identity接入；V2仍待实施，不混记通过。
- 修改前真实Skia明暗96自然帧已采集：16根、default→disabled→restore，原内容/高度约束/动效保持；准备中的API命名错误不计证据。此仅原始基线，不是完整视觉出口。

- N2/N3：17公开子控件15RED/2原消费正对照后通过；继承模板Transfer两实际Global RED、内部Tree/Transfer图标两RED（14而非31）均已转绿。219处Global引用在36主题迁移到现有owner资源机制，增加可选模板anchor及原Template优先级，公开派生identity保持。
- ContextMenu raw值及显式标记均为monospace、context相同；新鲜绑定读到正确三输入，而原绑定未收到更新。Avalonia12.1.4 Popup仅SetParent、未进入逻辑子集合；复用ThemeContextResourceBridge按挂载/卸载接入后74专项PASS。typed-source A/B无效，已撤回；Core字体日志埋点已恢复原SHA。
- Custom两有效RED（MinHeight32盖住普通Style7）转绿。17个Custom合并分支改基础默认加三档预设，不触碰Cascader输入壳体或高度实现。
- 完整结构阶段26文件/22Control/0问题，成员token指纹与类型头前后相同。检查器原被TreeViewItem同名显式接口属性触发Dictionary重复键崩溃；仅排除显式接口属性的wrapper索引，仍独立检查接口布局。ContextMenu后续生命周期需追加审计。
- Menu/Nav新增12个有效Own RED（6s）：顶层normal/open四色、横向hover/selected四色及圆角、子项圆角、顶层popup间距、背景easing。正在接线；easing通过现有TokenTransitions的owner绑定复制，不把资源绑定挂到无host的TransitionBase。相关默认外观及曲线须单列限定审查。

- 后续有效验证：横向hover最初命中点在标题栏/内部margin，原两项不计RED。保留D.Window、尺寸与模板，绘制后将点转换到实际header中心并断言IsPointerOver；精确撤回两处消费后2FAIL/2PASS，候选4PASS，源hash恢复相同。12项Own最终通过。
- 状态保护发现并修复：派生横向selected前景不可覆盖disabled；过滤disabled正文缺少ItemDisabledColor；ListBox选中标记实际为IconTemplatePresenter，原IconPresenter选择器未命中，宽度NaN而期望31。有效前后证据保留；12套编译专用Style的真实Setter及class退出通过。临时专项123PASS。
- 原家族1014项：1011PASS，3个旧源码断言失败（V1赋值、Shared资源）；更新为V2与owner锚点后精确3PASS。Core相关桥/资源/identity/状态机制17PASS。未运行全量或change范围。
- 当前Mac实际NSWindow/scale2：managed546、NativeAOT547，218默认资源/type/key/slot逐值一致；37身份、raw点击、viewport/extent、OverlayPopupHost和PopupRoot、own easing与编译专用Style通过。初始驱动配置/链接路径错误不计产品RED；沿既有openssl/brotli路径发布，官方12.1.4及缓存未替换。
- 完整视觉v1：旧/新各480PNG，两个完整历史均完成，78个候选源文件hash精确恢复。AA/1DIP、四档适用尺寸、Tab四方向原310x190/12条目/添加按钮、Nav三模式、明暗/compact及default→compact→configured→disabled→reset；原比较原样保留。矩阵捕获Small Pagination间距从0变8的非预期差异，临时有效RED→Small显式0、Custom基础8通过；正在以最终修正源码重跑。

- v1矩阵已逐帧检查：非预期Small Pagination间距8的有效RED后恢复0，Custom基础仍8；其余默认布局/viewport记录保持。最终版本矩阵重跑中。Native最终回放还暴露驱动在场景提交前发送raw点击的就绪问题；保持相同窗口和点位，补实际hit几何的有界等待，不重试点击、不改产品事件代码。原547项成功和该次失败均保留，不以重跑覆盖失败。

最终工程/清理/精确验收见container-family-exit-results.md；用户现已明确批准任务5的限定视觉范围，CT14既定范围全部出口对齐并关闭，CT25/FW1原始问题和边界独立保留。


## 2026-10-30 最终批准与关闭登记

用户通过 `request_user_input_async / call_656cee76f0f3440cb7dfe8d373826c6d / item 0` 明确回复：**“批准本表限定范围，保留 CT25/FW1”**。本次 V1–V7 限定视觉批准与工程、清理、Desktop1014/Core17、NET8零警告及当前Mac managed556/AOT557证据已对齐，CT14在既定批准范围完成并关闭。

原始像素失败、TabStrip指示线复位、coercion/Style退役及其他既定CT25/FW1边界保持，不将其记为产品修复或平台像素通过，不扩大容差、其他字体/配置/Host/平台。CT12、CT13和Cascader既定高度批准保持；本次批准不授权Git提交、推送、发布或依赖替换。仅同步文字状态，产品源码不变，沿用相同最终输入的已完成验收。
