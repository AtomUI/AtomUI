# CT15 Message 实际消费者执行计划（2026-10-31）

沿既定原目录feature/custom-theme单agent执行，不创建分支/worktree、不提交，保留既有CT11等未提交改动。Card限定审查已发出，等待期间继续本包，不把分包当CT15完成。输入2 metadata/6 Own，复用现有FeedbackStack/Motion/scheduler与CT12/13/14既定关闭证据，CT25/FW1及官方12.1.4不动。

## 合同与裁决

MessageCard拥有单条文本、类型/自定义Icon、root/wrapper/icon/title、现有关闭协调；WindowMessageManager拥有稳定集合、listContent、宿主、位置/Stack/生命周期调度与释放。保留原高度自适应、完整可选文本/选择色、图标、队列及指针gap命中、有限/零时长、关闭一次、焦点和键盘，不移除触发条件。没有SizeType，不创造S/M/L/Custom，Native/Part尺寸定制保留。

Ruling：CardHeight未计算默认=0且无消费者，按T30/T22保留Reserved资源/默认/type/key/slot及配置往返，不凭名字制造固定高度或MinHeight。其余五字段逐实际消费验证，默认公式原样保留并迁V2 Revision1。

Ruling：message正文是原有FontSize/ColorText角色，先维持实际默认14/ColorText，将其默认移到root并让原生root值可达真实SelectableTextBlock；这可能改变原仅继承未消费的root有效值，必须单列限定审查。FontFamily复用有限原生继承机制。manager是独立owner，字体值与卡片角色边界不混同。其Global/卡片Global资源按各实际owner接线。

Ruling：默认图标若有效RED证明随类型不更新，先检查AXAML现有五状态Style加Template每实例图标是否等价；本地Avalonia参考12.1.3只作思路，必须用官方12.1.4实编译/运行证明，不替换依赖；不先引入flags或C#回退。

## 顺序

- [x] Temporary/CT15Message有效RED：card/manager Global、根字体/Native优先、动态类型图标与用户Icon/清除；五Own及Reserved/0/默认导出。
- [x] 控件成员完整27问题清理：先纯移动，Roslyn逐成员token/初始化顺序相同后再实现，不借排版改变行为。稳定模板绑定迁AXAML，动态创建card绑定按真实关系释放。
- [x] 明确Owner、五字段V2与Reserved；专用Style实际关键Setter与输入，不做code-behind回退；保留现有关闭/hover/计时及Motion机制。
- [x] 使用真实Skia/正常帧泵，四算法/RTL/原生尺寸、默认/Own/Global/Part/清除与正常Loading持续轨迹；旧新、A/A/1DIP/恢复及当前Mac NativeAOT。工程先检查共享采样锁。
- [x] 结束后删除临时测试/专用工程/媒体，清理后的精确Message与受影响共享模块验收、NET8、成员、正式五文档/CSV/总表及作者另步自审；有意变化限定视觉审批。

Stack几何原16/8及背板常量虽有文档Global语义，实际未全部接入；先取得源码/运行证据，作为独立后续依赖P2，当前P1不得据此关闭完整Message。Notification/Dialog/MessageBox仍后续，不重复共享已完成分包。


P1进度：初始12项为6有效FAIL/6PASS（3.6170s）：默认图标随类型、两owner Global/原生正文、Native动效清除与关闭后manager投射残留。成员纯移动快照完全相同（27→1，最后region名称纠正另验），原注册/字段初始化顺序保留。官方12.1.4编译AXAML Template每实例图标，49项临时/既有检查通过；24默认/type/key/slot及生成key SHA相同。

真实Skia首轮236检查中4FAIL，四配方均为manager卡片默认图标不更新/实际初始缺失。原因是本轮移走C#默认创建后，Show设置Icon=null的Local初值与默认null相同不触发PropertyChanged，遮住Style默认。此为本轮回归，不归因于原版或驱动；补直接/manager初始null两项可失败验证，再恢复原OnApplyTemplate的null清除语义，图标构造继续在AXAML，不新增旗标。首轮媒体已查看缺图标的默认帧，属于失效候选，不纳入最终兼容比较；文本选择/输入/hover/关闭等其余232检查通过。外部目录别名、键盘API签名/partial启动类的探针编译错误均不计产品RED。


null初始化补证为候选2FAIL（3.2053s）→完整51PASS（4.1844s），修正后真实236检查0失败。首次88主帧带人工1DIP检错历史，四配方恢复2/38/33/25像素（max1/29/12/23），A/A各0；原位主基线不可用。旧新42/88零，余有意与微差均保留，不能计为最终兼容通过。按验证合同将1DIP检错移至独立实例，两个版本仍保留相同完整Own/Global/Native/Part/Type/RTL/Stack/选择/关闭业务历史，重新采主帧；不移除业务往返或调容差。该管线在候选源码完全恢复后终止其自有AOT发布子进程，部分外部编译产物后续清理，未换依赖/更改产品。被污染实例所有原始非零保留，不据重跑抹去。


本包工程/清理/精确验收见[结果](message-consumers-results.md)：51验证、29清理后检查、NET8零、成员10/2/0；22临时/434工程/1163取证删除。上列勾选为执行与记录已完成，不表示39 JIT/AOT或其余原FAIL通过。仅[8对及root两有效值](message-consumers-visual-review.md)待限定审查，P2和完整Message/CT15保持开放。


P2归属复核：[独立记录](feedback-stack-p2-ownership-results.md)以正式架构/CT10真实输入为准，16/8及背板产品几何不自动成为Global映射；原待核对线索不作为批准。37项精确现有几何验收通过，真实host/其余业务/像素/平台与完整家族仍开放。
