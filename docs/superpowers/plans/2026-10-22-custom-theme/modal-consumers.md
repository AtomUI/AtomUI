# CT15 Dialog/MessageBox 实际消费者计划（2026-10-31）

输入Dialog 11 metadata/17 Own、MessageBox 2 metadata/2 Own。沿指定原目录feature/custom-theme单agent，无新分支/worktree/提交；Card/Message/Notification限定审查待用户回复，继续本包。正式文档入口为desktop/feedback/modal，库存doc_paths原空不表示无文档。

## 原证据与职责

复用CT11 motion-exit-results的已生根/未生根Overlay、Window ThemeContextLease、DialogResourceBridge及真实LogicalChildren/placement-target回退，357本机/NativeAOT检查及既定合成接受范围；不重写桥接，不将该动效通过代替CT15其他消费。CT12/13/14关闭与CT25/FW1边界不变，官方12.1.4不换。

Dialog只拥有会话与展示意图/Host尺寸，Surface为双Host实际显示，Header/ButtonBox/Caption/Mask/Resizer内部目标按真实控制器owner；MessageBox是独立公开身份并复用Surface，不拥有隐藏Dialog。保留原Content/CustomButtons、modal owner/焦点/键盘、veto/异步关闭、加载/确认态、拖动/缩放/滚动/裁剪和尺寸结构下限，不移除原触发条件。

Ruling：17+2默认迁V2 Revision1。MinHeight原读取HeaderPadding，应声明强类型Own上游参数（ControlTokenDefault属性的依赖列表），显式HeaderPadding参与默认，明确MinHeight仍优先；其余原Global/常量/类型/key/slot/名称保持。不得照名字改Host结构下限或让MessageBox MinWidth降低Dialog结构。

先取得真实显示/字体/Global/原生请求反例，并记录attached/unattached、Overlay/Window及Dialog/MessageBox的实际继承和默认，不能假设controller字体已到Surface。复用已有IControlThemeGlobalOwnerSource提供终端owner，保留独立公开Button/Caption等职责；模板可达绑定AXAML优先，运行期双Host关系/私有内容投射写明代码兜底原因和释放。

## 顺序

- [ ] Temporary/CT15Modal：19 Own/四算法76默认/type/key/slot、MinHeight上游及明确值/0、真实display Global/字体/Native、两Host/attached与placement回退、专用两套8Part真实命中及隔离；保留完整尺寸/内容/焦点/关闭合同。
- [ ] 逐真实反例修复，完整成员/初始化审计、纯移动tokens相同；不重构已通过的Session/资源桥/调度状态机，不绕过官方依赖或像素容差。
- [ ] 正常Dispatcher/默认motion实际Overlay及当前Mac Window/AOT验证；业务状态/切换、scope热更新/清除/关闭/释放，A/A与独立1DIP，原内容/高度/滚动/裁剪保持。
- [ ] 临时源/外部工程/媒体清理后精确Modal与受影响模块验收、NET8、正式五文档/CSV/总表、作者另步自审和具体限定视觉。完整CT15不因局部默认迁移/分包通过关闭。


首次7项3有效FAIL/4PASS（3.2837s），扩至Window rooted/unrooted及具体终端11项7FAIL/4PASS（3.6574s）。真实Headless Window基线：unrooted controller=$Default/12/Black，而HostWindow/Surface为系统复合字体/14/#e0000000；rooted controller与Surface同为14/系统字体。不能把所有控制器默认值直接relay到Surface，否则unrooted默认降为12并破坏原契约。

Ruling：Surface复用IControlThemeGlobalOwnerSource映射真实Dialog或MessageBox Global，保留实际宿主原生继承作为未配置回退；显式控制器本地/Style字体请求才在运行关系层投射，高于主题Global。观察优先级变化必须覆盖“显式12与controller继承12数值相同”而Surface当前14的情况，不能只监听有效值变化或用数值相等推断未配置。此为三属性有限投射，不新增公开旗标/Scope引擎、不改DialogResourceBridge/ThemeContextLease。


同值显式12请求已有独立RED（3.2983s）。优先级探针显示官方12.1.4普通PropertyChanged在同值写入/清除时不通知，且Animatable封闭OnPropertyChangedCore，不能用先前设想的覆盖回调；探针编译错误不计产品RED。该边界尚未修复，不以新flag、轮询、反射或更换依赖绕过；先推进确定的19 Own/Global目标和可观察Native请求，保留同值请求为开放验证项，不能据此声称完整Modal/CT15完成。


合同纠正：前述“控制器原生字体必须转发”Ruling撤销。Modal semantic-part §root明确只拥有会话/内容/结果/事件，DialogTheme的Width/Height=0及Host*映射也证明控制器不是显示面；继承来的FontSize/Foreground未声明为Surface请求。先前Native控制器/同值写入测试属新行为假设，不能据此研发转发/扩大FW1。试验字体relay及SerialDisposable/diagnostic helper全部删除；保留Surface真正宿主原生继承，Global通过实际终端owner映射，原生定制以实际内容/专用Part验证。原测试观察与试验失败照录，不改写为原版bug。此纠正避免未授权地把原Window14回退降低到控制器12。


当前候选16临时+既有精确Modal namespace共292PASS（23.4285s）。19默认四算法76记录逐条相同；17 Dialog字段、2 MessageBox字段、HeaderPadding到MinHeight有效依赖、具体终端Global及两套Overlay/Window专用样式已验证。Surface/Overlay资源owner复用现有代理接口；早期internal Surface作为SourceControlType的未注册回退报错已改为AnchorControlType，Transient未挂载阶段安全Shared回退，进入真实树后映射公共终端。保留Session和资源桥原机制。

C#实际触及Surface/OverlayPresenter两控制，4文件含Token2的成员审计0；方法纯移动指纹一致，其他43文件盘点中的未触及既有问题不据此改写。真实Skia/当前Mac NativeAOT、完整19字段矩阵/热重挂/GC、清理后验收及正式文档/CSV出口尚未完成；不得把292或76默认作为CT15/Modal完成。


当前工作树暂时清理：16临时用例的3源/18532 bytes已删除，外部文字备份仅供下一次视觉驱动重建；新建取证目录尚无截图，按合同清理。全Modal namespace前置292PASS/23.4285s，删除临时源后再按最终树运行276既有模块及NET8。此为当前工程检查点，非Modal/CT15完成，也不申请尚未执行的视觉通过；正式19字段/Host视觉仍继续。


当前清理后276既有精确namespace检查PASS/0Skip（19.3779s）；NET8见当前日志，当前未执行真实Modal视觉/AOT，不作为整包或CT15完成。未新增常驻用例、未提交；全部未通过/限定批准范围按各独立结果保留。


2026-10-31继续：Card32、Message8、Notification36及各两有效值纠正已获用户限定批准，原排除项不变。Modal真实Skia驱动保留960×740 owner、520×420 Host请求、140高ScrollViewer、24行原文、两标准按钮与正常Motion，四算法×Dialog/MessageBox×Overlay/Window同历史；先候选，再独立校准、原目录56文件精确回放并finally恢复，再NativeAOT。字体原生定制通过实际内容及专用Part，未恢复控制器字体relay。外部探针初次局部变量作用域编译错误已修正，不计产品RED。


真实主采样完成：四算法×两公开owner×Overlay/Headless Window×11阶段=176帧；managed与NativeAOT各224检查0失败，旧版同历史224检查32FAIL。旧新160/176精确0，另16均为明确Global配置；JIT/AOT176/176精确0，六类业务恢复×16场景全部0。原回放56文件finally恢复并逐文本一致。

Ruling：首版独立1DIP校准错误覆盖Surface的产品定位RenderTransform，清除后Overlay丢失定位，产生8对243359–336324像素恢复失败；这是探针破坏定位合同，不是产品反例、不能归FW1。保留首轮数字，校准只移动原本无RenderTransform的semantic-container，ClearValue恢复，三个版本重新独立执行；主采样没有校准写入，因此176主帧不重跑。补充实际叶子字体、长寿命provider下GC与专用Style声明核验；新临时测试首次编译的Shouldly表达式树/同名Grid命名空间错误只计探针错误，不计产品RED。


最终本切片：21临时+276既有297PASS，新增4个mask/shadow/header/caption目标检查PASS；25临时5源30819 bytes全部删除。有效旧/候选/AOT各16组校准A/A/恢复全0、1DIP全检出，176跨运行主对全0；误覆盖定位与未完成导航渲染的两类探针失败已定位并保留，不改产品规避。清理后276PASS/0Skip、NET8零20.63s、成员4/2/0及精确56源恢复；912 PNG/912树记录/manifest共1825取证清理且审计clean。当前19 Own均Active、137/274及NeedsDecision11保持。详见[结果](modal-consumers-results.md)与[16对限定视觉](modal-consumers-visual-review.md)，完整Modal和CT15继续开放。


限定批准登记：2026-10-31，用户明确回复“批准表内16对Global变化，其余未通过与完整CT15保留”（request_user_input_async / call_62f69b5d81a14a9ab06d7d6f5bb64392 / item 0）。仅Modal表内16对Global获限定批准，真实OS输入及完整Modal/CT15、其他未通过保留，不授权Git提交。
