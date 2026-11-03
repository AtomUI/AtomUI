# CT15 Card 家族消费计划

沿指定feature/custom-theme目录单agent执行，不创建分支/worktree、不提交，保留已存在CardTheme的CT11 motion改动。Alert28对限定批准已登记，原未通过、CT12/13/14关闭范围及CT25/FW1不变。范围7 metadata/22 Own，不代表完整CT15。

## 审计与职责

Card拥有Header/Extra/Cover/Content/Actions、ContentType、Headerless及有效边框/圆角。CardActionPanel内部均分操作项并自绘分隔线；CardGridContent拥有容器与扩展disposable；CardGridItem拥有布局位置；CardTabsContent转交TabControl选择/显示，CardMetaContent独立拥有四个Part。Size/Motion由当前Card内容关系传递，独立公开子Control各自拥有Global。动态Content不是稳定template part，保留带明确释放归属的C#类型化投射；Grid的行列定义虽是稳定目标，但CLR集合属性不支持Avalonia绑定；实编译AVLN3000证明后保留原类型化投射并注明原因。

存在风险：Grid自带容器不走generator clear，当前三条状态绑定又没有进入已有disposable；IsHoverable未向默认容器投射。Tabs换模板在ItemsSource模式清理只读Items的路径需复现。Small的无条件body Padding会误进入Grid/Tabs，违背已有文档。actions Part声明支持Background/Padding，但内部背景/未投射Padding可能压住实际效果；必须以专用Style和实际几何失败验证，不能只看Tag。

## 22字段与T22决策

- 17已有消费者按真实分支逐一验证（包括两类阴影、Inner/hover、三尺寸、Grid与Tabs）；ExtraColor接现有HeaderExtra的明确Own颜色，ActionsBg虽有静态引用却被Frame原TemplateBinding压成null，二者均按OptionalOverride激活，未配置保留当前继承/已有默认。
- CardHeadPadding/CardPaddingBase未在AXAML历史中有过消费，当前已有三档HeaderPadding/BodyPadding拥有独立职责。保留为Reserved，不凭空创建第二套padding或建立别名。
- TabsMarginBottom源自旧声明的负下边距，但当前Tabs拥有完整TabControl的header+body，没有对应独立head-tabs几何目标；ActionsSpacing也没有既定的margin/padding/gap目标。四个悬空字段均保留资源/type/key/slot/序列化往返，明确Reserved并纠正文档，不为它们发明新功能；该判断依据T22“不为悬空字段凭空创建新功能”。
- 22字段迁V2 Revision1，原Global/常量/BoxShadows构造精确保留，无Own依赖边；四算法88默认/type/key/slot及实际旧默认比较。

## 契约与实施

- [x] Temporary/CT15Card有效RED：Extra、五个排版根/Global owner、自带容器移除/原生disabled/hover、Tabs source重模板、Small特殊body、actions专用Style真实Padding/Background及分隔线几何。保留所有原内容、尺寸、滚动/焦点/键盘；Reset事件原NotSupportedException不改变。
- [x] 复用OwnerTokenResource与Core有限字体/前景继承机制。ControlTheme拥有最终Setters；保留公开本地值、Role字体、独立TabControl/Avatar/IconButton责任，CT11动效机制不重复改造。
- [x] 修复真实生命周期/几何输入的获取释放。模板actions Padding的未消费1需与实际默认0区分：去除无效style默认、用原生0并投射到Frame，保持默认画面；明确专用Padding生效并将自绘分隔线置于实际child坐标。此属性默认纠正单列审查，不暗改像素容差。
- [x] 临时compiled AXAML使用Card六类、CardMeta四类专用Style，实际目标/关键Setter/隔离及无code-behind回退；实际Raw输入及hover左右边界、三档/native尺寸、四内容、loading循环分开验证。
- [x] 真实Skia旧/新及当前Mac AOT、A/A/1DIP/恢复、正常Dispatcher帧循环；同实例切换、资源热更新、重挂/换模板/回收与GC。NSWindow/其他Host和既有合成失败继续原边界。
- [x] 清理临时源/外部工程/媒体，Card精确模块及实际新增触及模块验收、NET8、完整成员/初始化审计、正式文档/CSV/总表和作者另步自审。仅新有意变化提交具体限定审查，不询问是否继续。

公共API/注册名/事件/枚举/Part/ControlTheme key不新增、不删除；保留当前SizeType三档，不创造Custom枚举。Native Width/Height/Padding等定制与专用Part优先。语义部件失效先查真实树和属性优先级，原始FAIL不改成“限制”。

有效初始19项为14FAIL/5PASS，其中派生Grid夹具未指定StyleKey导致1项前置条件误判；补正确StyleKey后旧源码单项重新取得disposable未释放的有效FAIL（3.4312s），候选完整40PASS。扩展原生Padding两项有效RED→修复后完整47PASS；字体实际可见正文、Reserved往返、hover及别名默认验证均包含。方法/区域纯移动token快照相同，完整成员11文件/6控件/0问题。两个需要审查的属性默认纠正是actions的未消费Padding1→原生0、GridItem的根Padding0→实际三档Own值，默认实际渲染仍须前后比较。


本包执行结果见[实际消费者结果](card-consumers-results.md)。36临时+20既有56PASS、managed/AOT各900静态+148持续loading检查、88对静态像素0。以上勾选仅工程/记录切片，不表示视觉已批准或整个Card/CT15通过；[32对及两属性默认纠正](card-consumers-visual-review.md)等待具体限定审查，恢复非零和原边界保留。清理后的精确模块、NET8及成员实测在结果中登记。
