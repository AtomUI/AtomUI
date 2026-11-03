# CT15 Tag / CheckableTag / Group 消费者计划（2026-10-31）

承接 [24metadata/40Own核对](avatar-tag-badge-consumers.md)，本包输入8metadata/9Own；不修改Badge或已关闭家族。Avatar最终旧新/AOT正在顺序采样，本阶段只准备Tag范围、临时反例与原源快照，不并发构建或改动正在回放的Avatar源。原137/274及NeedsDecision8不变。

已读Tag五正式文档、checkable选择专项、AbstractTag/Checkable及三个主题。真实合同：Tag的Color(default/preset/status/custom)×Filled/Solid/Outlined；Closed仅派发事件，不隐含删除或取消API。Checkable复用ToggleButton/Command/Form；Group拥有公开Value选择与Options集合、内部容器和专用item route。保留完整Text/Content、原wrap/尺寸约束/焦点键盘状态，不新增通用SizeType、popup或第二套选择引擎。

先取得TagCloseIconSize明确Own37/0/清空反例，原实际Shared.IconSizeXS与旧Own同默认，是否启用以有效反例与前后视觉表限定审查。其余8Own先验证真实消费者，V2R1默认公式/type/key/slot不得改变。文字FontSize仍TagFontSize角色，FontFamily与明确Global在实际owner消费，Native与生成专用Style优先级逐目标核对。状态色C#现用无owner ThemeTokenResolver，先以Tag Global ColorSuccess等反例判断；不据名字重写palette或颜色算法。RenderScaleAwareBorderThickness目前只在ApplyTemplate写入BorderThickness，动态厚度需保持原生值并测试配置热更。

| AC | 驱动/观察 | 独立预期 |
| --- | --- | --- |
| T1 | 4算法导出9Own默认/type/key/slot，9Own改值/0/清空 | 36记录与旧源逐条同，真实Icon/close/文字/边距/颜色进入目标，未授权变化单列 |
| T2 | Tag自身Global字体/边框/圆角/状态色，Checkable独立owner与Group间距 | 实际注册owner有效，Native/Style覆盖与clear恢复正确；不以Tag Own代替Checkable Global |
| T3 | AXAML声明TagIconStyle/ContentStyle/CloseStyle及GroupItemStyle | 专用Style命中原route/ContractType，关键Setter进入真实目标，无code-behind取节点赋值 |
| T4 | Color×Variant、close事件、替换模板、隐藏/重挂、Group选择/集合 | 原内容/完整状态保持，旧part、集合、值frame、scope与事件可退役 |
| T5 | Skia+NativeAOT、普通/Compact/明暗、RTL、代表尺寸极值、真实框架输入 | 独立A/A/1DIP可检错，同完整历史的旧新/跨运行精确RGBA，未覆盖OS边界保留 |
| T6 | 临时清理后Controls/Desktop Tag精确filter与NET8、成员/指纹 | 不扩大全量，正式文档/CSV/总表/作者另步自审；视觉批准仅覆盖具体限定表 |

此为执行计划，不表示Tag9字段已消费通过或NeedsDecision已减少；不提前计入CT15完成。

共享下游补充：SelectTag从Tag继承，其已有CT13批准范围保持，不能因本包修改AbstractTag/TagTheme悄悄改写输入家族结论。保存22个原源快照，先准备9项临时反例/四算法导出；待Avatar持锁回放全部结束再构建，不对共享产物并发操作。Tag9Own及Global消费者尚未执行通过。

首轮9项执行4有效FAIL/5PASS（2s）：明确TagCloseIconSize37实际Close.IconWidth10；Tag Global FontFamily Menlo实际原继承字体；Tag Global ColorSuccess Orange实际#ff52c41a；Tag Global边框3实际1。Native完整文字/字体/Padding与四算法36默认/type/key/slot导出通过。无产品改动、无Skip、无扩大测试；不是编译错误或零匹配。待Avatar清理后最终模块/NET8结束再按根因实施。

根因处置顺序：9默认迁V2R1保原公式；Close入口改消费其Own（未明确时默认仍10，须旧新四算法精确确认）；Tag/Checkable/Group各Global以真实owner解析，普通Tag状态色保留原palette/亮度公式，不能误把全局palette重写成局部配置；可表达的边框/字体/Template输入回AXAML。新增任何getter/代理前核对现有ThemeTokenResolver和继承owner解析，避免未注册子类或SelectTag抛错；兼容边界以有效测试决定。

首轮修复9PASS/0Skip（2s），36默认/type/key/slot与修改前精确同。TagToken V2R1保留原ColorUtils与字号/边距公式；关闭图标消费原Own；字体用既有optional继承机制；状态色GetEffectiveGlobal按GetType/StyleKey匹配真实注册owner，原无注册回退语义保持。边框由ControlTheme编译绑定RenderScaleAware输入持续更新，移除ApplyTemplate单次代码赋值。扩展验证准备Checkable自身Global、Group spacing、既有TokenTransitions（已通过现有owner机制，不另造Value扩展）、AXAML生成的Tag三Part/Group item Style。第一次fixture用错icon XML namespace为驱动编译失败，已按Gallery真实namespace修正，不计产品RED。

扩展首轮13项3FAIL/10PASS：Checkable checked背景明确ColorPrimary Orange实际#ff1677ff、Group明确spacing31未进入目标为两个有效RED；TokenTransitions明确450ms及数量2已PASS，复用CT11机制不改。Part临时探针用Name=IconPresenter同时命中Tag与close内部同名节点而抛Single异常，为定位器错误，改按已有semantic-icon marker只读筛选，不计产品RED、不改route或使用赋值回退；四个专用Style仍仅AXAML声明。另补Group Global FontFamily独立反例，再实施各owner的共享入口迁移。

独立Group字体有效RED实际原字体而非Menlo；修正marker只读定位后生成专用Part验证PASS（Tag三Part与Group runtime item，原route均真实命中），不改route、不回退code-behind。随后迁Checkable各颜色/边框/圆角与Group spacing到各自owner Global，保留TokenTransitions既有owner时长机制。Checkable字体保留TagFontSize家族默认/Own回退，明确本Checkable Global.FontSizeSM才进入本owner角色，原生字号仍Local优先；Tag/Checkable/Group字体家族复用既有optional物理继承机制。当前14临时联合既有Tag精确模块执行中，不提前宣称通过。

Avatar表内48对已获用户限定批准（call_7b292d7ef454420fb8fc0603ac3e3f4c），其余微差/完整Avatar/CT15与CT12/13/14原边界保留。

14临时+62既有Tag精确模块共76PASS/0Skip（5s），Checkable本owner的checked背景/边框/圆角/字体及clear、Group本owner spacing/字体及原生clear、生成三Part/item route与既有450ms时长全通过。36默认/type/key/slot保持，不动已批准CT11机制或CT13输入实现。下一补9Own真实glyph/布局与0/clear、原键盘/鼠标/关闭和退役、原生优先/资源GC及成员整理，再Skia/AOT。

17项退出验证15PASS/2FAIL：保留已从Options退役、Parent已null的item时旧Group仍存活为有效GC反例；9Own目标/合法Icon/close/padding0在前段通过，但尾段临时断言误把原TagIconSize写14（原36默认导出实际12），编译后再纠正源未改变已执行程序集，故该FAIL为驱动常量错误，不计产品RED，也不改产品默认。真实Enter/鼠标关闭、旧close模板事件退役保持PASS。下一在原触发下单变量清除退休container的ContentTemplate/motion两个Local投射边，定位GC因果；此为临时生命周期取证，不是视觉定制或产品代码回退。

GC单变量诊断结果：清ContentTemplate、清motion、两者均清，三次仍FAIL，故“仅两个Local投射造成保留”的假设被否证，尚未据此修改容器绑定或引入dictionary。与此同时原36默认中的Icon12断言重编译后，9Own/真实glyph/0/clear全PASS（与GC同选2项1PASS1FAIL）。继续只读记录退休节点实际logical/visual/template/inheritance/focus/context关系，先定位引用拓扑再修根因。

只读拓扑确认退休item的logical/visual/template/inheritance parent均空、无focus、无ThemeScope context，排除这些直连。临时强引用路径扫描（跳过弱引用、类型/反射元数据，不进入产品）直接得到item.ValueStore._localValueBindings→IndexerBindingExpression._source(CheckableTagItemsControl)→VisualParent(Group)。此前ClearValue不dispose仍留下该表达式，所以不能据其失败排除投射边；原假设通过真实路径得到证据，保留全部先前失败记录。

修复限定AbstractCheckableTagItemsControl：原两个Indexer Local投射换成既有typed BindUtils.RelayBind并按container持有CompositeDisposable；Prepare先退役旧句柄，ClearContainer/Detach dispose，Attach按实际容器恢复。代码注释说明ControlTheme setter不能等价保留Prepare时机与Local投射优先级，不新增全局跟踪或Scope引擎。当前含17临时、既有Tag及4个SelectTag精确下游验证执行中，不能提前以修改本身判GC通过。

容器句柄修复后本轮81PASS/0Skip（6s），17临时、62既有Tag、实际匹配的2个Select Custom下游；另2个精确全名最初误写Select namespace（真实SelectControl），未将其计作通过，下一轮修正过滤。GC回收通过，临时路径扫描不再找到原强链；先前原链和ClearValue失败原记录保留。补回原“仅option wrapper才投射ContentTemplate”的条件，避免改内部其他输入语义；仍只复用typed handle，不加新选中状态机。成员审查18源/4Control见21问题，纯移动字段/接口组/私有方法和region进行整理，前后指纹将逐项比对。

成员纯整理18源/4Control从21问题归零，完整前后导入、type/member/注册/初始化指纹逐字相同。补detach后旧item不再受Group motion变更、重挂当前template/motion及再次动态更新；修正两个SelectControl精确全名，当前18临时+62既有Tag+4下游应84项匹配执行中。之后进入实际Skia/NativeAOT完整轨迹，不以单元通过关闭分包。

扩展84项83PASS/1FAIL：新退出探针误要求ItemTemplate改变后旧container继续接收新模板；真实框架已将它退役（Parent空且ContentTemplate清空），这是正确释放的表现，原Group实例保持。临时断言改为重新观察同一Group当前实际生成的container，并核对退休对象不再有投射，不修改产品来保留已退役节点；该FAIL不算新产品缺陷。重挂第二template/motion两项在失败前已通过。完整同历史驱动也应始终观察当前真实容器，不能把旧引用冒充当前目标。

最终本轮84PASS/0Skip（7s）：18临时+62既有Tag+4个Select下游，36默认保持；重挂/模板重建后的实际container及退休句柄均通过。当前18源/4Control成员0，纯移动前后完整指纹相同。下一以1000×1200固定Desktop Window/四算法实际内容面，完整颜色×三Variant矩阵、原生/Own/Global/Part、真实focus/press/keyboard/close、Group选择/多选/滚轮与原100高裁剪、SelectTag下游，按相同完整历史旧新/AOT；独立A/A/1DIP校准。此是渲染验收入口，尚未生成任何Tag媒体，不能计视觉通过。

外部Driver首次构建使用macOS /var临时路径，MSBuild transitive reference在其/private规范路径下相对化发生错位（MSB3202）；仅清自身obj/bin并改用resolve后的/private/var绝对路径，项目引用恢复。随后ScrollViewer同名类型歧义以Avalonia.Controls别名消除。这些均为临时驱动错误，不计产品RED。probe包含公开Select多选作为Tag派生下游，原SelectTag高度/字体归属仍复用CT13，不改其源码或闭合结论；新增Tag Own配置下实际派生影响需如实观察并单列，不凭默认回归通过扩大批准。

查看候选default完整66矩阵、Own真实glyph/字体/颜色首帧；Own17/Line31/Padding13,7/TextPadding5,3同时作用66条后自然高度超固定1200，主轨迹原样保留，不缩小刺激值或减少原文字/行数来让它塞入。补独立overflow承载视图，同一固定Window与原Group100高裁剪，新增1120高外层ScrollViewer只用于展示完整大矩阵的top/bottom/restore；三版本相同轨迹，原主视图与新增视图分别判定。SelectTag高度/字体仍CT13原归属，新增Tag Own大值下的派生裁剪影响须单列，不能因旧默认0差异自动批准。


## 清理后精确出口

18临时case/3源25850字节、只读图扫描、1090取证及原外部driver/会话已清理后，精确Tag62+Select4共66PASS/0Skip（2s）。NET8 Release Desktop及Controls引用链0 warning/0 error，19.42s。最终3文件短期日志目录登记后删除；当前无Tag临时验证源/媒体/驱动或会话残留，无全量回归/提交。

84匹配验证、managed/AOT各120检查；164跨运行159零、旧新164中133零。26对已获限定批准（含Tag Close37对派生SelectTag的宽度/换行与原高度裁剪影响），其余整帧、恢复微差和完整Tag/CT15保留。18源/4Control成员0、纯整理指纹同，36默认/type/key/slot保持。正式6文档、8metadata/9Own、总表/自审同步；TagCloseIconSize按实际消费者裁为Active，NeedsDecision8→7，仅字段裁定，不等于视觉或家族批准。


限定批准登记：2026-10-31，用户明确回复“批准表内26对及列明派生影响，其余未通过与完整CT15保留”（request_user_input_async / call_5cea4f7e59a243dcab027c42807d9848 / item 0）。仅26对与列明SelectTag继承close37/0导致的换行/原高度裁剪影响获批准；恢复、5对跨运行微差、原CT13批准边界、OS/Browser及完整Tag/CT15保留，不授权提交。
