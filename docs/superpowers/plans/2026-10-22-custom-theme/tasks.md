# 精确主题优化与重构任务目录

本文的进度表是CT任务状态的唯一总入口；全局约束与阶段出口见[路线图](overview.md)。下表是任务包，CT-00–CT-02的详细步骤见[首批执行计划](readiness.md)；其余包须在前置出口通过后细化文件、接口和验证，不作为现成产品实现计划。

后续 Windows 新增授权与本机交付统一转到 [Windows 验收记录](../../progress/2026-11-05-windows-theme-acceptance.md)。
当前产品修正、修补包/引擎与专项结果见[Windows 缺口补齐](../../progress/2026-11-06-windows-theme-gap-closure.md)。
下表保留先前限定出口的历史状态；2026-11-06 已补按原记录重建的31-owner/40控件 Windows 托管及 NativeAOT 原生场景验证。
当前场景通过不等于恢复历史脚本的逐字身份，其他平台、原 mixed-host 失败及批准边界不自动扩大。

## 0. 执行进度

2026-11-05续作：已核对既有feature/custom-theme、HEAD35f9cc8f5及全部原改动；当前详细证据只见[CT25当前记录](platform-current-exit.md#2026-11-05-续作有限执行与证据账本)。保留后续表格的历史过程，不把旧阶段文字覆盖各家族最后出口。原724项目录与1244字段无重复主键，原90组与CT26逐项表双向一致。

2026-10-24新增授权：完成CT-10全部子任务，并继续完成CT-11至CT-26；沿当前分支和worktree、不自动提交、不用subagent。CT-24后由用户明确暂缓，本轮先不做；其余工作继续。

2026-10-25用户要求完成全部进行中任务，当前执行集合为CT-11–CT-19及CT-25；不把分包通过、视觉批准或提交当整项完成。

更新日期：2026-10-30。工作位置为`feature/custom-theme`及现有worktree，源码审计基线`85407138f`；阶段提交`79f2fc00e`记录截至CT-08A的成果，本轮已持续推进至CT-19颜色选择及CT-25平台分包，机制及各分包清理后聚焦证据见下表，本轮修改继续交用户审查，不自动提交，整体重构尚未完成。CT-00/CT-01完成的是目录与源码分类，不表示控件或主题机制已经修复。**已完成CT-04/CT-05及CT-06机制和DataGrid17项兼容接入；Own V2持续按家族迁移，DataGrid78字段和ColorPicker24字段的后续默认计划已有独立结果；完整控件家族已完成CT12–CT17六项（各既定批准范围；CT25/FW1及CT17明确排除项独立保留）**，不能把准备工作换算成视觉验收完成率。

| ID | 状态 | 已完成与证据 | 下一步／未完成门槛 |
| --- | --- | --- | --- |
| CT-00 | 已完成（目录） | [已提交目录](control-inventory.csv)721条；本轮按实际模板新增内部信息面板1条，当前722个主键无重复；原720条双向基线见[目录范围](baseline.md#目录范围) | 305个owner待决保留给对应家族，不伪造identity |
| CT-01 | 已完成（源码分类） | [字段记录](token-contract-review.csv)95个Own终端／1195字段，默认依赖及消费者源码；90组ID映射校验通过 | 123未决字段、124合同决策由对应家族处理；无Approved字段，不代表实际消费已验收 |
| CT-02 | 已完成（代表基线） | [执行清单](readiness.md#task-ct-02建立代表失败与host性能基线)、[结果](baseline.md)：12个目标反例失败／1正对照通过；Skia A/A及1DIP检错；实际Window/Popup Host；启动/热更新和释放样本 | 独立审查Critical/阻断Important均0；原生最终显示面无输入闪动仍归CT-12/CT-25专项，不能声明无闪烁 |
| CT-03 | 已完成（接口合同） | [正式具体接口](../../../architecture/systems/theming/own-token-evaluation.md#9-ct-03具体构建与运行接口)及[实施计划](own-defaults.md)已核对旧路径／版本／跨包／格式边界 | API已由CT-04/05实现；尚未发布，不代表95个产品Token已迁移 |
| CT-04 | 已完成（实现与专项） | [CT-04A/B清单](own-defaults.md#task-ct-04a可用的版本化声明与分派合同)：版本分派／生成计划；20个临时生成用例、75项聚焦对照；.NET8构建、跨包macOS26/arm64 NativeAOT运行通过 | 已随CT-05清理并通过机制审查；没有迁移产品控件或批准全平台 |
| CT-05 | 已完成（实现与专项） | [实现与验证](own-defaults-results.md)：有效Own／存在位、算法／appearance、缓存、惰性／重试及scope；清理后Core47／Generator63通过，跨包AOT实际Compiler通过 | 机制最终审查Critical/Important/Minor均0；进入CT-06，不代表产品控件已迁移 |
| CT-06 | 已完成（机制与17项兼容） | [结果](aliases-results.md)：实际消费者4/4；产品Skia37对差异0/1DIP检错8649像素；清理后Core58/Generator64/DataGrid22通过，NET8构建和当前Mac动态AOT通过；作者自审完成 | 全部临时源/媒体已清理；完整DataGrid家族、compiled literal别名NativeAOT及原生显示面仍按CT-18/CT-25验收 |
| CT-07 | 已完成（最小原型） | [结果](qualification-results.md)：9/9PASS，Card编译专用Style、重置、优先级、资源拒绝及10代frame清理；清理后Card11/Core21/Generator1通过，作者自审完成 | 临时源已清理；产品品牌装配/跨根事务、性能与最终视觉仍属CT-08/CT-25 |
| CT-08 | 已完成（机制） | [A结果](style-assets-results.md)；[B结果](style-hosts-results.md)：Context和Application/Provider/Window lease接线、退役；临时Core19+补充1/Desktop3通过，清理后Core50/Desktop16、NET8零警告，代表Skia前后0像素差异，作者自审完成 | [C结果](style-transactions-results.md)：统一协调/继承重置/NoOp、完整stamp、版本1表现结果和有界清理；临时Core17/Desktop3通过，清理后Core58/Desktop17、NET8零警告，默认前后/品牌恢复0像素差异；作者自审通过。公开注册/manifest属CT-09，多平台/性能/全控件仍待后续验收 |
| CT-09 | 已完成（机制） | [计划](brand-manifests.md)、[结果](brand-manifests-results.md)：显式规范登记/能力、版本1JSON与完整/TokenOnly导出、选择身份及同runtime事务；临时30/30通过并清理 | 清理后Core115/Desktop17、NET8零警告；默认HEAD79前后0px、10次完整往返0px、真实两类Popup、公开仅登记0px；作者自审完成。工作台/AI/全控件及平台仍待CT-22–CT-26 |
| CT-10 | 已完成（共享消费） | [S1–S5计划](shared-consumers.md)、[总结果](shared-consumers-final-results.md)与[104项实际消费者](shared-consumers-catalog.md)：10根owner排版/前景、25既有身份、内部/公开子Control边界闭环；前序箭头/两类Popup/框体/图标/启动分包合并验收。本轮59+2临时验证已清理、净增常驻0；明暗26同轨迹旧新默认/恢复0、Mac AOT/JIT36帧0；最终Desktop960及Combo13/Core3、NET8零警告，作者自审与媒体清理完成 | CT-10无未决子任务；完整家族Own/业务状态/持续动效/平台专项继续CT-11–CT-26，不计作这些任务已通过 |
| CT-11 | 已完成（动效消费合同） | [总出口结果](motion-exit-results.md)合并M1–M4：Wave/Splash精确输入、93组动态集合（92组本机实际路径、Windows专用1组归CT25）、74个owner开关断链及代码时长/跨根热更新修复完成。当前Mac managed/NativeAOT各357检查，33对象回收，NET8三个产品出口零警告错误；明暗1044对完整轨迹及补充定位已审，12对暗色传播与8对有限合成历史差异均获限定批准。13临时源、466原生工程文件及2261媒体文件清理；最终聚焦991PASS0Skip，成员10文件/9控件/0问题，作者另步自审完成 | CT11合同内无未决子任务；[Windows专用及有限合成差异](motion-exit-compositor-review.md)、其他Host原生最终像素/输入继续CT25。CT12–CT20完整家族状态不因共享机制完成而关闭；本批未提交/推送 |
| CT-12 | 已完成（既定批准范围；CT25保留） | [最后工程出口](buttons-box-shadow-finish-results.md)：九owner/133Own（128有效+5历史）、V11公开BoxShadow接通；当前默认NSWindow managed/AOT各1236检查，52帧阶段内与恢复0、跨运行12对0，指针/按下0。六临时/441外部文件/121媒体已清理，最后Desktop172/Gallery16/Rendering1共189PASS、NET8零、成员1/1/0。 | [最后限定视觉](buttons-box-shadow-final-visual-review.md)已获用户明确批准，V11及完整CT12既定出口关闭。原始Skia24/max1、旧混合宿主两对3968/max2等按CT25保留，不设通用容差；Split Custom140仍CT20；未提交/推送。 |
| CT-13 | 已完成（既定批准范围；CT25/FW1保留） | [输入目录](inputs.md)；[R结果](input-resize-results.md)：ResizeHandleSize命中/可见绘制及owner线宽已接通；默认指示线恢复可见经用户批准（明暗88px），统一阶段配置清除0px；真实拖拽/滚动、Mac AOT通过，临时源/媒体清理后395项与NET8零警告。[F1结果](input-font-results.md)：LineEdit三档Own/Global字号及字体族、原生继承/优先级已接通，明暗四状态及清除0px、Mac AOT通过；[G1记录](input-global-results.md)：58处资源投射、四owner真实消费与当前Mac AOT通过，清理后Input136/NET8零警告；默认状态文字颜色已获用户批准、其他边缘/往返像素差异归CT25未决。[V1结果](input-defaults-results.md)：16字段迁移V2、96值相等，同轨迹新旧默认/配置/恢复0px，Mac AOT通过；清理后Core33/Generator1/Input-OTP198及NET8零警告。[F2记录](input-font-remaining-results.md)：三owner可选字体族、Search全尺寸Global字号及Embedded隔离通过；新旧四默认状态0、原生API新旧六阶段0，字体清除合成边缘现象原来即存在且继续CT25；临时源/媒体清理后Core26/Desktop195及NET8零警告。[G2记录](input-frame-owner-results.md)：共享frame及clear/reveal的95处资源接入真实owner，内容路线/同scope迁移和GC通过；26控件明暗六默认阶段0，真实点击48+12/主题。Mac NativeAOT运行通过且与JIT四图0，临时源/媒体清理后Core21/Desktop525及NET8零警告；配置往返/性能出口仍开放。[G3-A结果](input-otp-global-results.md)：OTP45处有效Global消费、17项专项及真实输入通过；96格明暗六默认阶段/清除0，Mac AOT运行及JIT对照四图0；临时源/媒体清理后OTP65及NET8零警告。[B1/B2A/B2B](inputs.md)已按各自结果完成；[C排版结果](input-otp-typography-results.md)已接通可选根字体与分隔符字号，20默认/22AOT画面0px、临时11已清理、最终OTP66/Core1与NET8通过。[D颜色结果](input-otp-color-results.md)完成用户批准的普通／禁用前景修复：73项、批准原型四图0、38AOT-JIT0，临时清理后OTP66及NET8零警告；其余输入选择继续，不能以分包代表家族完成 | 逐输入／选择控件及状态整改，保留焦点与输入 ；用户2026-10-24明确允许默认分隔符ColorText／ColorTextDisabled颜色修复，按[颜色结果](input-otp-color-results.md)完成独立消费验证，不再等待批准 |
| CT-14 | 已完成（既定批准范围；CT25/FW1保留） | [家族结果](container-family-exit-results.md)、[85项矩阵](container-family-exit-matrix.md)：37身份/258Own（221Active+13Derived+20Reserved+4Legacy）；Global/字体、109V2与12Own消费、Custom/Part/回收完成本机工程。临时清理后Desktop1014/Core17、NET8零；当前Mac managed556/AOT557、218资源一致，媒体/工程已清理 | [最终限定视觉](container-family-visual-review.md)V1–V7已获用户明确批准；完整CT14既定出口关闭，保留CT25/FW1原始失败及既定边界，不扩大容差；未提交 |
| CT-15 | 完成（既定批准范围） | 137 metadata / 274 Own; 26 contract groups. [Final finite exit](feedback-final-exit-results.md): approved software Skia adopted; original named restores verified, scoped visual approvals registered; cleaned focused 483+7+77+276=843 PASS; NET8 and actual Gallery AOT integration verified. Details: [adoption](feedback-region-sdk-adoption-results.md), [native input](feedback-native-input-delivery-results.md). | Original FAIL/exception/missing-frame records, INPUT_NOT_PAIRED, platform and CT25/FW1/FW2 boundaries retained; no full regression, branch/worktree/subagent or commit. |
| CT-16 | Complete within approved scope; CT25/FW1/FW2 retained | [Finite family exit](date-time-family-exit.md): 22 metadata / 51 Own; prior D1-D4-C reused. Actual Global/field consumers, strict row-height boundaries, fonts, scope/reparent, dedicated styles and state/size exits verified. JIT/AOT 202 full frames zero; actual NSWindow managed/AOT each 258 checks. After cleanup Desktop357 + Core15 = 372 PASS; NET8 zero warnings/errors. | V1 approved: default workday marker, 32 pixels per theme; no pixels outside the two markers. CT25/FW1/FW2 and old FAIL/INPUT_NOT_PAIRED retained; [Height/placement follow-up](date-time-popup-height-follow-up.md) complete with P1 approved; cleaned focused133 PASS / NET8 zero. No full regression, branch/worktree/subagent or commit. |
| CT-17 | 已完成（本轮既定批准范围） | [家族出口及集中批准登记](drawing-family-exit.md#批准登记与最终结论)：50 metadata/92 Own（87消费+5批准兼容）；141临时已清理，最终Desktop152+Gallery22=174PASS，NET8零警告；当前Mac managed/AOT各648检查。V1–V5/C1/S1均已明确批准 | AOT/JIT309/310为0；命名S1为11像素/max7，仅按具体批准接受，原严格FAIL保留，不设通用容差或转CT25/FW。独立Steps、完整CT20及其他既定边界不扩张；未提交 |
| CT-18 | 已完成（既定限定批准范围） | [家族权威出口](data-grid-family-exit.md)：42 metadata/78 Own=48 Active+17 Alias+13 Reserved；106工程精确PASS，临时清理后76PASS/NET8零/14文件6控件审计0；Mac managed/AOT各974，152对RTB与标准96对最终显示cross0；D1–D5/R1及表列N1已批准 | [批准集中登记](data-grid-family-exit.md#批准登记与最终结论)；N1的26对原生显示FAIL按具体接受保留，历史异常/无效帧、CT25/FW边界不改写；全部临时源/媒体/driver专用产物已清理，原有改动及暂存区保留；未提交，不自动开展其他CT |
| CT-19 | Complete within explicitly approved scope | [Final authority and approval](color-picker-family-exit.md):22 metadata/24 Active Own; V1-V3 and R1/R2 approved. Follow-up39 focused/cleaned27/NET8 zero/member3-0-0; current Mac AOT verified. | 660 images/896 comparisons:854 expected,42 named S1 titlebar FAIL explicitly accepted and retained. Route migration, V3 evidence limits, C9 history and CT25/FW retained; all task artifacts cleaned; no commit. |
| CT-20 | 已完成（既定限定批准范围） | [权威出口/批准登记](custom-size-exit.md)：58行/45文件及扩展入口；默认144对0，Mac managed/AOT各292；清理后25类274PASS/NET8零/成员6-3-0 | V1–V4已批准，原G1有效RED保留；后续Native修复见CT25权威记录，原范围与其他严格FAIL不变 |
| CT-21 | 已完成（既定限定批准范围） | [权威出口](gallery-tools-exit.md)：146目录（22/19/105）/41 Active+1兼容保留/23新增Own，默认20对0，Mac managed/AOT各28；清理后128PASS/NET8零 | T1–T3已批准，唯一登记见权威出口；中间图不当最终屏幕证据，其他平台及原FAIL保留 |
| CT-22 | 已完成（有限工程与验收） | [权威出口](brand-workbench-exit.md)：8配方×明暗、16完整往返RGBA0；真实hover/pressed、尺寸/嵌套/Popup；Mac managed/AOT各152 | 适配配方非第三方像素复刻；清理后门禁与CT23合批，原平台/严格FAIL保留 |
| CT-23 | 已完成（手动闭环工程） | [权威出口](brand-workbench-exit.md)：workspace主题菜单、schema/有效值/隔离预览/应用/文件保存/完整往返/diff/恢复/取消；Core定义锚定与真实scope receipt | 原实例/过期/释放通过；清理后Gallery33/Core63/Icon3、NET8零；常驻净增0；CT24排除，未提交 |
| CT-24 | 未开始（用户暂缓） | 用户2026-10-24明确“这个先不做”，本轮不接AI服务 | 后续需要另行恢复授权；不再将实际endpoint／模型信息列作本轮阻塞 |
| CT-25 | 已完成（限定出口；Windows开放） | [当前权威记录](platform-current-exit.md#g1fw1最终结论与另步作者审查)：原G1经Native hook在官方12.1.4上修复，NET10/NET8/Mac NativeAOT完整轨迹各280；清理后Window/Buttons362PASS/0Skip，14项临时保护已删。原G2具体接受与具名包/后端/显示证据按各自源码身份保留 | G1仅认证原Window.Styles场景；Windows专项继续OPEN，CT24暂缓；旧mixed/未配对/范围外及严格FAIL不改写。本次Git按用户最新提交push指令执行；未授权NuGet发布 |
| CT-26 | 已完成（本轮限定交付收口） | [最终审查](final-exit-review.md#交付状态)：724目录/1244字段/原90组双向一致；正式模块文档、具名验收、精确门禁、作者另步自审、批准与临时清理已核对 | 全平台认证与已明确保留的开放项不算完成；独立Steps原排除、CT19具体接受及CT24暂缓保持；不自动提交 |

2026-10-25最新平台出口：[第十批](platform-tenth-followups.md)已按批准提交abd62d849／9e0855940并推送，[Actions40163904747](https://github.com/AtomUI/AtomUI/actions/runs/40163904747)八个job均结束。三NativeAOT／两Browser通过，三OS前八测试项目各5664项通过；Windows import及Grid原失败通过，后续仍因build-host.log清理占用失败，Linux实际包消费600秒总限失败，Mac包消费通过后Gallery的Tour命中1项失败。未运行到的后续项目不计通过；[第十一轮取证](platform-eleventh-followups.md)继续定位，不宣称平台最终通过。

CT19追加[C6输入重挂结果](color-picker-input-lifecycle-results.md)：离树权威颜色／格式变化回放，无变化保留草稿／校验／光标，格式选择改用原生模板绑定；临时20项与Mac managed／NativeAOT各308检查通过，清理后Desktop23、NET8 Release及成员审计通过。324帧及独立直接渲染证据、59对精确限定已获用户批准，临时源码／工程／媒体已清理；仅C6切片完成，已随获批第十一批提交0c6d8445679e，完整CT19／原生像素验收继续。

CT25追加[第十一批](platform-eleventh-followups.md)：消费工程私有NuGet路径／feed与共享source分离，full先构建全部源码、其余消费者真实编译并复用源产物；缺产物正对照明确失败，清理后完整Official四图约136秒通过。Windows仅增加占用PID只读取证和原消费／清理双异常保留，Tour仅追加失败几何；其原始CI根因尚未关闭。清理后Tour2项及C6既有23项／NET8证据通过，四份临时源已删除。13文件获批提交0c6d8445679e并推送，[第十一轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40168600469)运行中；C6已包含在该提交，完整CT19／CT25仍开放，后续C7不在本批授权范围。

第十一轮最终七个job成功，仅Windows选测因三个.NET Host持有build-host.log清理失败；Mac／Linux各6290项通过、Windows5664加本地化4项通过，三OS各27个子进程阶段完整配对。[第十二批](platform-twelfth-followups.md)在日志所属ILC8构建子进程关闭持久服务器及节点复用；调用边界两项RED→GREEN、实际托管编译／help／身份复用和清理、8项工具检查及已提交产品基线的完整Official约141秒通过。五文件获批提交b8325380f671并推送，[实际第十二轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40172623312)的三NativeAOT／两Browser、Mac／Linux选测各6405项通过；Windows本地化集成5项、实际Official及清理通过，原日志占用本轮未复现。Windows随后因Linker反例加载后的Empty.dll删除失败，5993项中5992通过，后续Gallery412项未运行；[第十三轮候选](platform-thirteenth-followups.md)继续定位与验证，CT25仍开放。C7不在本批提交范围。

第十三批4文件获用户批准，提交f24a9aba7710并推送；[三OS／两Browser第十三轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40175124825)运行中。坏IL反例改从同一输出读取内存镜像，保留真实JIT异常及receipt拒绝；Mac精确13项与最终Linker模块89项通过，Windows确认仍待CI。本批不含C7，不代表完整CT25验收。

CT19追加[C7色谱结果](color-spectrum-lifecycle-results.md)：异步与fast统一请求顺序、归一范围／退化轴及共享helper完成，受控26项、有效亮暗514项和Mac managed／NativeAOT各184项通过。140帧与28组1DIP、独立直接Skia及公开输入对照完成；所示修复与41对限定合成差异获用户批准，全部临时驱动／工程／媒体已清理，最终Release23项、NET8及成员审计通过。C7切片待独立Git审查，C8的公开Components默认与实际模板不一致、既有键盘可达性继续；完整CT19未关闭。

最新执行：C7已随获批第十四批分为纯布局3240285cc与修复3e7efb341并推送。[第十三轮](platform-thirteenth-followups.md)Windows原Linker删除失败未复现，6404／6405通过，仅Gallery Tour卡片命中失败；Mac／Linux各6405通过。[第十四轮](platform-fourteenth-followups.md)Windows／Linux各6405全通过、Mac6404／6405，仅Gallery Timeline卡片命中失败；两轮的三平台NativeAOT及两Browser均通过。Tour／Timeline间歇性命中问题仍按实际几何定位，不能因另一次通过而关闭CT25。

CT19 [C8短设计](color-spectrum-components.md)的SV公开默认值与六模式接线已获用户明确批准；[结果](color-spectrum-components-results.md)记录14项有效RED→GREEN、扩展22项通过并删除、清理后ColorPicker27项及NET8 Release零警告通过。192帧包含有效亮暗候选494项、64组1DIP全检出；真实Mac managed／NativeAOT各310项通过。独立直接Skia32组A/A、12组恢复与6组新旧默认／SV／恢复均0；[30对自然合成限定差异](color-spectrum-components-noise-review.md)（2–23像素／通道最大2）已提交用户单独审查，不继承C1–C7批准。媒体／外部工程待审查后清理，Git未授权；既有键盘及完整家族仍开放。CT25 [第十五批调查](platform-fifteenth-followups.md)已建立排队滚动更新的两项有效RED→GREEN，原用例与临时探针5项通过，探针删除后原三个精确用例3秒通过；仅修改两个Gallery测试的实际帧准备，未改产品输入／布局，实际平台确认待下一轮获批CI。

状态规则：`未开始`→`规划中`→`进行中`→`最终审查中`→`已完成`。有真实外部障碍才标`受阻`并写明条件；依赖尚未满足不伪报外部阻塞。每项开始、验证、清理和审查后在此表更新状态和证据链接，详细步骤在其子计划勾选，实际数值写入结果记录。中途问题或状态问答不会撤销继续执行授权；处理后回到当前任务，不以一条总结代替继续工作。提交须单独遵守用户的明确提交指令。用户已明确要求后续所有工作由主agent独立完成，不再使用subagent；后续最终审查由作者另起审查步骤执行，如实标明自审边界。

CT-13追加[G3-B1结果](input-otp-geometry-results.md)：三处OTP几何与专用Cell/Separator Style命中接通；明暗六默认阶段/清除RGBA0，实际输入与NativeAOT Headless/Skia通过、JIT/AOT十八图0。临时测试/工程/媒体清理后OTP66与NET8零警告通过。B2/C与整个家族保持进行中。

CT-13追加[G3-B2A结果](input-otp-caret-results.md)：caret显隐及偏移全部移至AXAML、字号更新/恢复有效；明暗六非闪烁阶段新旧RGBA0、最终NativeAOT与JIT十二图0。临时验证/媒体清理后OTP66、NET8零警告和成员审计通过。B2B时间线、C排版继续。

CT-13追加[G3-B2B结果](input-otp-blink-results.md)：独立周期/缓动Own与原生每光标Style接通，默认1s线性往返、装饰motion=false及隐藏暂停相位保持；明暗16张静态新旧RGBA0，最终NativeAOT/JIT十八图0，实际时间线/输入/GC通过。7项临时验证、工程及媒体已清理，最终OTP66、NET8零警告、成员审计和24个文档链接通过。C根字体及separator排版继续，完整家族验收仍为0。

当前增量目录为721项（CT00基线720之外新增内部caret叶节点），Own字段1221项（CT01基线1195之外累计26项），V2十三个终端历史175字段（新字段26另计）；资源ID1480／96范围，B2B只追加两个新ID，原1478编号不变。这些是目录／机制计数，不换算为完整控件验收进度。

本次阶段提交前补验：Core启动/lease/资源29项、Generator资源键55项、Controls相关消费4项、Desktop按钮/输入/OTP530项，合计618项聚焦检查通过；NET8 Release零警告/错误。此为本次提交检查，不等同全量回归或CT26最终验收。

CT-16追加[D4-C页头结果](date-viewer-header-results.md)：范围Header以Style级编译绑定跟随实际Panel，专用Header Style可覆盖及恢复；Content默认2302／2060像素对齐差异已获批准，Compact不变。原边缘命中2失败→候选11项功能／输入／生命周期通过；Mac实际NativeAOT72项通过。5份临时源、专用发布与媒体已清理，最终日期337项及NET8零警告通过。原生最终显示面、既有暗色RGB1和保留owner时旧模板部件仍归CT25，完整日期家族保持进行中。

## 1. 基础合同与机制

| ID | 依赖 | 工作和边界 | 独立交付/验收 |
| --- | --- | --- | --- |
| CT-00 | 无 | 对照产品public Control、生成注册、Token、Semantic、主题导出和Gallery建立完整主目录；内部目标/抽象层/无Own控件单独分类 | 精确程序集/metadata name/identity/owner/平台/源路径及文档入口；候选不冒充完整Control数 |
| CT-01 | CT-00 | 核对每个终端Own语义及默认依赖，区分Active/Derived/Legacy/Reserved/Deprecated；明确历史字段、别名和未公开能力 | 分类记录及逐Control文档修订范围；无消费者不能标Active已交付，不为字段补造不存在功能 |
| CT-02 | CT-00/CT-01 | 复现代表性owner、Own依赖、框体、优先级、自绘、别名和动态问题；建立视觉兼容比较矩阵及同版本A/A对照，冻结真实Host、原始UX和采样条件 | 可重建失败配方、可重复视觉基线及每个Control的前后比较安排；未复现/未取证项不填通过 |
| CT-03 | CT-01/CT-02 | 明确新版Own声明、可访问静态默认函数、存在状态、依赖/重定义、descriptor/evaluator修订与旧路径迁移的具体API | 正式文档中的版本及公开签名审定；旧whole evaluator、slot与schema边界保持；作为CT-04/CT-05唯一接口输入 |
| CT-04 | CT-03 | Generator读取同/跨程序集元数据，验证依赖图并生成扁平静态求值顺序；声明层移动不改终端schema | 循环/未知输入/类型/重定义/版本混合诊断，确定输出、跨程序集和实际AOT消费 |
| CT-05 | CT-04 | Compiler按显式存在状态保留用户值，其余默认读取有效上游；缓存纳入evaluator修订，保持独立惰性槽 | 上游覆盖/显式下游/恢复/0/null/明暗/算法均成立；先用Button、Skeleton、ColorPicker证明再迁移 |
| CT-06 | CT-01/CT-02 | 别名在每个配置层先规范化，再按层合并；DataGrid17项规范字段与旧Table入口保持兼容 | 同层不同值冲突、同值归一、跨层正常覆盖；公开字段到真实消费者，导出及internal历史字段有版本决策 |
| CT-07 | CT-01/CT-02 | 以最小原型证明owner资格工厂、嵌套继承/重置、selector优先级和候选资源隔离 | 父padding/重置子foreground反例通过；实例/状态/专用Style覆盖符合原生规则；没有运行期反射或找节点改值兜底 |
| CT-08 | CT-07 | ThemeManager协调准备、接线、预激活、再校验/Token提交、不可回滚Publish与退役；版本化表现结果和跨根lease | 已打开/新建Native及Overlay一致；重入/stamp/激活/恢复/退役故障独立注入；普通观察者warning与表现失效区分 |
| CT-09 | CT-08 | 独立版本化品牌组合manifest及注册资产引用/修订/能力要求；复用Token编译与现有格式边界 | Token-only与完整配方范围明确，往返等价；未知资产/Control/版本有诊断，XML v1/ABI v1不静默变化 |

主要文件边界：

- CT-00/CT-01：现有审计CSV、控件公开合同、主题导出；参考[注册构建器](../../../../src/AtomUI.Generator/Registration/RegistrationModelBuilder.cs)、[Token定义遍历](../../../../src/AtomUI.Generator/DesignToken/ControlTokenPropertyWalker.cs)和[Semantic Part设计](../../../architecture/systems/theming/semantic-parts.md)。不为目录清点新增产品运行时注册路径。
- CT-03–CT-05：[ControlTokenDescriptor](../../../../src/AtomUI.Core/Theme/Schema/ControlTokenDescriptor.cs)、[ThemeCompiler](../../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs)、[配置规范化](../../../../src/AtomUI.Core/Theme/Configuration/ThemeConfigNormalizer.cs)、[继承模型](../../../../src/AtomUI.Generator/DesignToken/ControlTokenInheritanceModel.cs)、[校验器](../../../../src/AtomUI.Generator/DesignToken/ControlTokenCalculationValidator.cs)、[schema生成](../../../../src/AtomUI.Generator/ThemeSchema/GeneratedThemeSchemaWriter.cs)及[生成入口](../../../../src/AtomUI.Generator/TokenResourceKeyGenerator.cs)。
- CT-06：[DataGridToken](../../../../src/AtomUI.Desktop.Controls.DataGrid/DataGridToken.cs)、实际模板/消费者及同一规范化边界；不能只改默认值测试。
- CT-07–CT-09：[ThemeManager](../../../../src/AtomUI.Core/Theme/ThemeManager.cs)、[事务](../../../../src/AtomUI.Core/Theme/ThemeTransaction.cs)、[Scope](../../../../src/AtomUI.Core/Theme/ThemeScope.cs)、[Provider](../../../../src/AtomUI.Core/Theme/ThemeConfigProvider.cs)、[专用Style生成](../../../../src/AtomUI.Generator/SemanticParts/SemanticPartManifestWriter.cs)及现有资源资产/格式所有者。是否新增文件、属性或结果类型由各子计划明确。

## 2. 共享消费与逐控件迁移

| ID | 依赖 | 家族/主要职责 | 独立交付/验收 |
| --- | --- | --- | --- |
| CT-10 | CT-02 | shared/Effective Global选择、InputControlFrame/AddOnDecoratedBox、root/frame、Icon、箭头、排版、Splash/TextSelectionHandle及跨根字体 | 正确公开owner和public子Control身份；既有实例/已打开弹层更新；无本地值压制或资源泄漏 |
| CT-11 | CT-02 | 动态Transition、motion/wave输入、WaveSpirit实际easing/时长/范围 | owner变化/主题切换更新且可释放，Transitions保留style setter；自绘实际消费公开输入 |
| CT-12 | CT-05/CT-10/CT-11 | Button、HyperLinkButton、DropdownButton、SplitButton、OptionButton及实际按钮派生项 | 行高/padding依赖、字重、阴影、shape恢复、三档公式与字段兼容；各identity独立 |
| CT-13 | CT-10/CT-11；有Own依赖者加CT-05 | LineEdit/TextArea/NumericUpDown/OTP、Select/ComboBox、CheckBox/Radio等输入选择 | 字体、frame、option padding、禁用态、resize/下划线几何、public属性和Part有效；保留输入与焦点 ；用户2026-10-24明确允许默认分隔符ColorText／ColorTextDisabled颜色修复，按[颜色结果](input-otp-color-results.md)完成独立消费验证，不再等待批准 |
| CT-14 | 已完成（既定批准范围；CT25/FW1保留） | [家族结果](container-family-exit-results.md)、[85项矩阵](container-family-exit-matrix.md)：37身份/258Own（221Active+13Derived+20Reserved+4Legacy）；Global/字体、109V2与12Own消费、Custom/Part/回收完成本机工程。临时清理后Desktop1014/Core17、NET8零；当前Mac managed556/AOT557、218资源一致，媒体/工程已清理 | [最终限定视觉](container-family-visual-review.md)V1–V7已获用户明确批准；完整CT14既定出口关闭，保留CT25/FW1原始失败及既定边界，不扩大容差；未提交 |
| CT-15 | CT-10/CT-11 | Alert、Card、Message/Notification/Dialog、ToolTip/PopupConfirm、Timeline、Avatar、Tag/Badge、FloatButton、ImagePreviewer/Carousel及Result | 实际区域/关闭图标/弹层尺寸/字体/阴影/形态入口有效；稳定Part；插画/命名色保留内容语义 |
| CT-16 | Complete within approved scope; CT25/FW1/FW2 retained | [Finite family exit](date-time-family-exit.md): 22 metadata / 51 Own; prior D1-D4-C reused. Actual Global/field consumers, strict row-height boundaries, fonts, scope/reparent, dedicated styles and state/size exits verified. JIT/AOT 202 full frames zero; actual NSWindow managed/AOT each 258 checks. After cleanup Desktop357 + Core15 = 372 PASS; NET8 zero warnings/errors. | V1 approved: default workday marker, 32 pixels per theme; no pixels outside the two markers. CT25/FW1/FW2 and old FAIL/INPUT_NOT_PAIRED retained; [Height/placement follow-up](date-time-popup-height-follow-up.md) complete with P1 approved; cleaned focused133 PASS / NET8 zero. No full regression, branch/worktree/subagent or commit. |
| CT-17 | CT-05/CT-10/CT-11；不含Own依赖者不等待CT-05 | Skeleton、Progress各形态、Rate、Slider、Switch、Separator、Watermark及相关绘制目标 | 标题/渐变/线宽/字体/几何实际消费；极值、细线及动态缓存；固定几何与开放形态明确 |
| CT-18 | CT-06/CT-10/CT-11 | DataGrid表头/单元格/过滤器/选择列/拖拽/展开图形及虚拟化 | 全部规范字段/别名、内部非Visual对象、自绘线条及Custom；原有滚动、裁剪、冻结和回收保持 |
| CT-19 | Complete within explicitly approved scope | [Final authority and approval](color-picker-family-exit.md):22 metadata/24 Active Own; V1-V3 and R1/R2 approved. Follow-up39 focused/cleaned27/NET8 zero/member3-0-0; current Mac AOT verified. | 660 images/896 comparisons:854 expected,42 named S1 titlebar FAIL explicitly accepted and retained. Route migration, V3 evidence limits, C9 history and CT25/FW retained; all task artifacts cleaned; no commit. |
| CT-20 | CT-01/CT-02；逐家族协调 | 58处Custom/Middle候选逐属性分离基础默认与预设；扩展至CT-00发现的其他尺寸入口 | 每处有实际职责及结论，Custom不接管其未拥有维度；保留三档、方向、Min/Max和输入几何 |

CT-12–CT-19的边界由CT-00精确Control目录补齐：表中示例不能排除未列出的Control，也不能以某个共享修复替代每个实际模板、状态和Part的验证。跨家族同名字段不自动继承配置。

全部控件任务共同遵守[视觉一致性硬门槛](../../../engineering/development/theming/verification.md#视觉一致性硬门槛)：各Control修改前采样，修改后同环境/配置/状态比较；正常和适用交互状态、明暗、尺寸/密度及弹层均有结论。未解释或未经批准的差异为零才可完成，不能只证明Token或Part已生效。共享修复涉及的消费者逐项纳入，CT-21工具壳也不豁免。

CT-10重点路径：[InputControlFrame](../../../../src/AtomUI.Desktop.Controls/Primitives/InputControlFrame.cs)、[框体主题](../../../../src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/Themes/InputControlFrameTheme.axaml)、[ArrowIndicator](../../../../src/AtomUI.Controls/Primitives/ArrowDecoratedBox/ArrowIndicator.cs)、Core/Icon及实际跨根host。CT-11参考[WaveSpiritDecorator](../../../../src/AtomUI.Controls/Primitives/WaveSpiritDecorator.cs)和[主题](../../../../src/AtomUI.Desktop.Controls/Primitives/Themes/WaveSpiritDecoratorTheme.axaml)。

各家族子计划必须列出具体C#/AXAML、测试、控件overview/implementation/token/semantic-part/changelog及Gallery示例，说明哪些原合同保持、哪些需要版本决策。CT-20的结论随对应家族提交，不另起一次无差别批量修改。

## 3. 品牌、工具和交付

| ID | 依赖 | 工作和边界 | 独立交付/验收 |
| --- | --- | --- | --- |
| CT-21 | CT-10/CT-11 | GalleryBase中立工具壳、SemanticPartPreview、元数据/页头、代码查看器 | 工具自己的排版/密度Token，不污染被预览控件owner；代码字体/语法主题及合理断点单独说明 |
| CT-22 | CT-08/CT-09；完整交付加CT-12–CT-20 | Ant默认、shadcn、Bootstrap及零圆角/粗线/大字/紧凑/关闭动效配方 | 配方多维进入真实渲染；状态/尺寸/嵌套/弹层一致；缺Host能力有诊断 |
| CT-23 | CT-09/CT-21；完整交付加CT-22 | 复用workspace目录/主题菜单，完成草稿/有效值/预览/应用/保存、diff/恢复、完整导入导出 | 同一runtime，兄弟预览隔离，已有实例状态保持，Token与表现结果不混淆；取消/过期不覆盖新草稿 |
| CT-24 | 未开始（用户暂缓） | 用户2026-10-24明确“这个先不做”，本轮不接AI服务 | 后续需要另行恢复授权；不再将实际endpoint／模型信息列作本轮阻塞 |
| CT-25 | 机制及各家族逐批接入；最终等CT-04–CT-23适用任务 | 注册/资产/跨程序集、真实trim/NativeAOT、Browser与各OS、性能/生命周期对照 | 每个Host/平台分别有证据；无全量实例化、反射旁路、无界缓存或旧Visual保留；无证据平台不填通过 ；[Picker模板终版](picker-template-results.md)已修复旧部件保留／有效模板替换会话，34项临时及45相邻通过、Mac managed／AOT各851检查通过，临时源与媒体已清理、所示视觉及限定2像素已获批准；完整日期家族及平台最终像素仍待验收 ；用户允许使用github.com/AtomUI/AtomUI的Actions验证其他平台，明确禁止发布到nuget.org；[多平台方案](platform-validation.md)已检查验证流程且AtomUI凭证可用；cfb1d6668已获批提交推送并运行首轮Actions：3原生、2 Browser通过，3选测失败；[后续修复](platform-followups.md)20文件经批准分两次提交为938f6e3c9、1fe3358ec并推送；[第二轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40061147545)已结束：3原生／2 Browser通过，3选测失败；Mac Desktop4,128与Generator465通过后有2项包元数据断言失败，Linux Desktop9失败，Windows选择路径失败；[第二轮后续包](platform-second-followups.md)12文件获批提交847c441c1并推送；[第三轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40068141876)五个运行时通过，三个选测失败（Windows旧清单测试的Win32 shell入口、Mac Desktop6项、Linux Desktop12项）；[第三轮后续包](platform-third-followups.md)34文件获批分两次提交fc9896fb6／e9bc9e707并推送；[第四轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40089581608)已结束，5运行时通过，3选测失败；Mac／Linux Desktop各4128通过，Windows 6个POSIX stub Skip使证据不完整，Gallery Mac debounce1／Linux点击3失败，见[第四轮后续](platform-fourth-followups.md)，整体仍开放；[第五轮跟进](platform-fifth-followups.md)：29文件批准后提交975fc8e34／57ae294d4并推送；本轮3 NativeAOT／2 Browser及Mac选测6272项通过，Windows DataGrid2失败／Linux Desktop隔离1失败，整轮failure；[第六轮候选](platform-sixth-followups.md)已完成W15提交顺序及W13所属dispatcher机制修复，清理后基础15／桌面35／Grid73与NET8通过；W14补充失败阶段后随获批30文件提交ef09198b9运行[第六轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40149666278)，三原生／两Browser通过，Windows W14失败、Linux Gallery19高亮失败、Mac取编译器源码遇网络失败并已对同SHA重试；该阶段处置见[第七轮执行记录](platform-seventh-followups.md)，最新平台状态见[第二十二轮结果](platform-twenty-second-results.md) |
| CT-26 | CT-12–CT-23/CT-25；AI交付另核CT-24 | 汇总逐Control/问题处置及视觉兼容矩阵，清理临时验证/废弃接线，同步正式文档与迁移资料 | 全目录有结论，90组每个子项闭合或有范围/兼容决策；无未解释/未经批准视觉差异，最终focused验证和审查完成后再申请提交 ；按用户决定，本轮排除暂缓的CT24 |

CT-21/CT-23使用现有[GalleryBase](../../../../src/AtomUI.Toolkits.GalleryBase)及[CustomizeTheme](../../../../controlgallery/AtomUIGallery/ShowCases/General/CustomizeTheme)；产品品牌及生成服务属于Gallery，稳定中立能力才下沉GalleryBase。CT-22/CT-24不得另外建立ThemeManager、Token字典或Style优先级引擎。

## 4. 审计追踪与新增发现

[映射表](problem-task-map.csv)为每个已有审计ID给出唯一主任务。协作任务描述共享修复、同组其他Control或最终证据义务；不代表主任务做完就能跳过协作方验收。TH/EVT及原始位置继续读原问题矩阵。

目录或运行验证产生的新问题先补审计事实与负责任务，再安排整改；不能只追90组而漏掉未被静态候选命中的控件。兼容保留不等于删除字段，范围外必须说明语义或平台原因。

本轮新增机制事实（不冒充原M0源码清单的已修控件）：

| 发现 | 责任与处置 | 当前证据/状态 |
| --- | --- | --- |
| 直接移除原生Styles后OTP模板separator仍保留已移除专用Style的紫色，旧源／候选相同 | CT-25核查原生Remove仅递归逻辑子树与模板子树清理；CT-08托管lease已有桥接，不能声称修复了外部直接Remove | [颜色结果](input-otp-color-results.md)保留失败；class资格停用后移除的相同轨迹通过 |
| RangeDateViewer Content页头按PanelWidth固定宽度，而日期Content列更宽，标题与面板不齐 | CT-16 D4-C已核定并修复；保留Content／ShowWeek／原宽高触发条件 | 用户批准默认对齐差异；[结果](date-viewer-header-results.md)记录11项功能／输入／生命周期、实际Mac NativeAOT72项、清理后日期337项及NET8通过 |
| D4-A暗色配置恢复24个字形RGB1、同值旧资源启用10个及AOT禁用10个 | CT-25精确视觉出口继续核查，不能以容差或同源类型推定通过 | [D4-A结果](date-viewer-global-results.md)保留全图计数／范围和未闭合结论，功能及最终337项通过 |
| TimelineItem自用Padding未声明测量失效，Own清除后DesiredHeight19而原39、后一项上移20 | CT-15在真实Measure owner补PaddingProperty依赖 | [P2结果](timeline-typography-results.md)：有效反例2FAIL、单变量866→0、修复后四次恢复0；临时清理后Core13/Desktop118与NET8通过 |
| inactive Style.Resources仍进入资源搜索，theme-only字典可能被HasResources遗漏 | CT-08准备期检查完整资源结构 | [CT-07反例](qualification-results.md)、[A实现](style-assets-results.md)已验证，B已接入实际宿主 |
| 原生Remove与Add对逻辑/继承子树覆盖不同，模板frame可残留 | CT-08 lease退役先失活，再清frame及验证释放 | B已实测Card10代及PopupRoot/Overlay旧Source清理；完整品牌协调、性能和原生OS仍待验 |
| 后赋资格root不能自动证明子规则Nesting，独立子selector可绕过资格 | CT-08A重新执行原生Nesting校验并在失败时不标记图 | A内部准备已实现/验证；不使用私有selector反射或找节点改值 |
| 旧OfType根selector不匹配真实Window StyleKey；更近Window复制同Context资产会抬高优先级 | CT-08B独立根资格及实际styling祖先去重 | 两项有效RED后通过，应用trigger11不再被复制资产24覆盖 |
| 候选追加尾部会改变应用Style覆盖；保留图Remove已修改后抛错会丢失当前组 | CT-08B相邻插入、原索引恢复及聚合清理错误 | 有效RED后通过；普通应用对象/顺序保持，原生部分owner错误逐项清理/可重试 |
| 工厂/初次Mount回调重入与空host可绕过阶段；完成句柄保留旧图 | CT-08B先登记pending/ready门槛、显式阶段及完成后释放 | 失败/通过及GC证据见B结果；Manager级重入和对外结果仍属C |
| Token准备提前创建表现属性通知；外层cleanup尚不能报告完整品牌状态 | CT-08B惰性创建；CT-08C统一协调和版本1结果 | 外层失败责任已接管，恢复/退役故障和重试通过；完整品牌公开入口仍属CT-09 |

任务拆分以独立可审查的合同为准：一项默认依赖、一条真实消费链或一个状态/形态合同可以独立交付；不按文件数量或任意固定工期拆分。暂不估算全项目日期，先以CT-00–CT-02确定真实规模。

2026-10-25最新：C8的30对限定差异已获用户接受，全部临时驱动／媒体清理并通过审计；获批第十五批15文件原样提交393d718b3、推送后运行[Actions40188740837](https://github.com/AtomUI/AtomUI/actions/runs/40188740837)。C8本机切片出口完成，三OS／两Browser实际结果仍待本轮；键盘审计及完整CT19／CT25继续，不计作家族完成。

第十五轮最终：获批393d718b3的[Actions40188740837](https://github.com/AtomUI/AtomUI/actions/runs/40188740837)8个job全成功；Windows／Linux／Mac选测各6405／6405、Gallery各412／412，三NativeAOT及两Browser通过，三平台本地化各27组子进程阶段完整配对。该轮不含后续C9及透明棋盘生命周期研发；CT19／CT25完整出口继续。用户已批准[C9色谱键盘短设计](color-spectrum-keyboard.md)，自然焦点和方向反例正在分阶段验证。

CT19追加[C9与棋盘格结果](color-spectrum-keyboard-results.md)：已批准的自然焦点／默认adorner与VS方向修复、两类可见棋盘离树更新已实现。原焦点8项失败与四组有效VS Up反例、两类slider／ColorBlock重挂反例均转GREEN，24项扩展检查通过并删除；清理后原ColorPicker27项、NET8 Release零警告及3文件成员审计通过。正式168帧、56组1DIP全检出；四组默认／四组最终reset为0，独立直接Skia56组A/A／16恢复／16旧新兼容为0，32对状态表一致。[31对自然限定差异](color-spectrum-keyboard-noise-review.md)及棋盘重挂视觉已送单独审查，媒体尚在仓库外等回复。真实Mac managed388；NativeAOT首轮383／388的四事件缺失未证明原因，trace与原二进制复跑各388通过不抹去首轮；滑轨键盘、原生最终像素及完整家族继续，Git本批未授权。

2026-10-25用户明确允许C9棋盘格重挂视觉修复及表内31对限定差异；457张PNG和manifest共458文件已清理，Git媒体审计clean，C9临时源／外部驱动已删除。既有原27项、NET8 Release及成员审计证据保持。用户同时批准[C10单分量滑轨短设计](color-slider-keyboard.md)，按[实施计划](color-slider-keyboard-plan.md)继续；两项授权均不包含Git，C9首轮AOT输入缺失及完整家族／原生最终显示面仍开放。

CT19 [C10单分量键盘结果](color-slider-keyboard-results.md)验证中：具体ColorSlider单一入口复用既有步长／边界及颜色链，不改共享多stop；空值与双stop正常数据分别原8键盘RED／候选26 GREEN，独立原Alpha舍入2项保留。真实Mac managed／NativeAOT各八窗口5688项、1248次目标按键均送达。自然384帧的128组1DIP全部检出、八新旧默认0、216对公共树记录一致；有限72对1–20像素／单通道≤2的合成差异正在独立直接Skia核对，尚未申请扩大任何既有批准。临时C#及媒体待完成判断后清理；C9首轮AOT缺失原因与完整CT19／CT25仍开放，没有新Git授权。

2026-10-25 C10最新：用户已接受[72对精确限定自然差异](color-slider-keyboard-noise-review.md)；[本机切片结果](color-slider-keyboard-results.md)完成。空值／正常双stop数据各26项、Mac managed／NativeAOT各5688项／各1248次目标按键通过；640帧的128组1DIP全检出，直接128AA／32恢复／40兼容0，416对公开树记录一致。两份临时C#、外部驱动及645张PNG／manifest共646文件已清理；最终原ColorPicker27项、NET8 Release零警告、四控件完整成员及媒体审计／文档／作者另步自审通过。当前等待新的具体Git审查，不能沿用第十五批授权；完整CT19／CT25及C9首轮AOT原因仍开放。

第十六批25文件获用户批准，按冻结c2f3a8337d64创建cdc7ba70f（纯成员布局）及204c44149（C9／C10功能和文档），全部最终blob相同，已推送并运行[三OS／两Browser验证40207571681](https://github.com/AtomUI/AtomUI/actions/runs/40207571681)。调度前同分支无未结束手动验证，没有取消／重试／发布NuGet；TreeView后续研发排除。本轮实际结果及完整家族／CT25仍开放；授权已执行，本段不包含新Git许可。

CT14追加[N3-T TreeView结果](container-tree-defaults-results.md)：12个Global-only默认接入V2，原Global／Control Global消费本已正常，保留默认值、类型、key枚举数值与slot。两目录选中色归兼容保留，未新增交互。168帧的56AA／24恢复／28新旧均0、56组1DIP全检出，Mac managed／AOT各304项资源检查；临时源／外部宿主／169媒体文件清理后原57项、NET8零警告通过。首轮Light外层Host证据单独保留，实际明暗补充已完成；原生最终像素与完整CT14仍开放，尚无本批Git授权。

第十六轮最终：获批204c44149的[Actions40207571681](https://github.com/AtomUI/AtomUI/actions/runs/40207571681)已结束，7/8 job成功，整轮failure。三NativeAOT／两Browser、Windows与Linux选测各6405/6405通过；Mac已执行5199项，其中5198通过，唯一记录失败为NavMenu展开动效中间宽度采样，其后1206项未执行。Windows／Linux本地化各27组阶段完整配对（含一项预期损坏feed反例exit1），Mac未进入该阶段。本轮未取消／重试／发布；[第十七批候选](platform-seventeenth-followups.md)以受控反例修正采样，实际CI结论待新批准后验证。完整家族与CT25继续开放。

N3-TR 的[Transfer默认求值结果](container-transfer-defaults-results.md)完成本机机制切片出口：三个scope共11项V2、明暗22资源/类型/key枚举数值/slot不变，原Global/Control Global消费本已正常。Mac managed/AOT各684项、清理后原44项及NET8零警告通过；用户已接受[34对精确限定自然差异](container-transfer-defaults-noise-review.md)，原始失败保留，203张PNG及manifest共204文件已清理且媒体审计clean，CSV仅回填11条Token与3条Control。第十七批待审14文件原始blob已固定为可校验补丁，此段及目录更新属后续未提交差异，不扩大其授权；完整Transfer家族、原生最终像素、其他Host、N2/N3/N4和CT14仍开放。

Transfer后续核对并修正文档中的无效专用Style、SetterTargetType及Semantic owner混写：旧2项FAIL，修正后4项通过，原文编译AXAML的8Style/9Setter命中两分区/三条目，临时源清理后原44项通过。另确认TreeTransfer owner分页wrapper为internal，原34对及684原生脚本该阶段仅为内部模板诊断；[纯公开API补充](container-transfer-public-results.md)60自然帧、20组1DIP全检出、30对资源/几何一致，80直接绘制的40AA/16恢复/40新旧全0；新14对自然历史差异于2026-10-26单独获批，141媒体文件已清理，原始计数保留；TreeTransfer.ListWidthLG经共享公开属性标识的旧/新版明暗16阶段核对，归Active条件消费者，无新增树分页API。产品三份SHA保持，完整CT14及其他平台/原生最终像素未关闭。

2026-10-26 Transfer公开API补充：用户单独接受PUB01–PUB14及Q01–Q05的精确限定自然差异；140张PNG与manifest共141文件已清理、Git媒体审计clean，原始比较失败及计数保留。三Token源码SHA及清理后44项证据不变；TreeTransfer.ListWidthLG随后核定为既有Active条件消费者；其他Host／原生最终像素及完整CT14仍开放。该视觉批准本身不含Git批准，后续整合授权与执行见下段。

2026-10-26 第十七轮闭环：用户批准27文件整合快照f277f370248e，已单提交并推送ff0d3d572；[本轮实际结果](platform-seventeenth-results.md)的8个job全部成功，三OS各6405/6405选测、各26项NativeAOT、两Browser各23项实际运行检查，源码身份相同。三OS本地化各27对阶段完整，370主题资产物化通过。未取消／重试／发布NuGet；原14文件已含于整合提交，不再重复执行。选测专项义务、所有家族最终原生像素及完整CT11–CT19／CT25仍开放，NavMenu首次展开产品方案仍待批准。下一片按既有N3计划推进Breadcrumb八项默认函数，不将CI绿灯当家族验收。

2026-10-26 CT14 N3-B [Breadcrumb切片结果](container-breadcrumb-defaults-results.md)：八项Global-only默认接入V2，明暗16默认、288条资源/类型/key/slot保持；正式84帧28AA/12恢复/42新旧均0，28组1DIP检出，Mac managed136/AOT137且80记录一致。所有临时源、169媒体文件、419文件原生工程清理后，原18项及NET8零警告通过；CSV回填8Token/1Control。仅完成本机默认机制切片，图标/键盘/完整业务状态、N2/N4、其他Host及原生最终像素仍开放；本片未获Git提交授权，不复用上一批。

2026-10-26 第十八批授权已执行：13文件快照f8e4279bfc31提交并推送93ef4ba8d，[第十八轮](platform-eighteenth-results.md)8/8job成功，三OS各6405选测、三NativeAOT各26及两Browser各23；所有源码一致，370主题资产与各27对子进程阶段完整。仅完成所列平台分包，不关闭CT25或完整家族。

2026-10-26 CT14 N3-P [Pagination本机切片结果](pagination-input-wiring-results.md)：14默认V2，明暗28默认及168记录保持；Mini与Simple背景接线的6处反例及保护检查通过，Mac managed206／AOT207且140记录一致。四份临时C#及419文件原生工程清理，原57项、NET8零警告与单文件成员审计通过；14Token/2Control回填，Reserved总数18。用户已批准所示视觉修复及表内N01–N10精确限定差异，159媒体文件清理且审计clean，原始失败计数保留。本机切片出口完成，不关闭完整CT14、其他Host或原生最终像素；本批未提交，Git范围另审。

2026-10-26 第十九批授权已执行：16文件快照4e0dbc3e188c提交并推送0b5e70c71，[第十九轮](platform-nineteenth-results.md)8/8job成功；三OS各6405项、三NativeAOT各26及两Browser各23，所有源码一致，各370资产及27对本地化阶段完整。未取消／重试／发布；该轮不含后续Expander迁移，不关闭完整CT25。

2026-10-26 CT14 N3-E [Expander本机结果](container-expander-defaults-results.md)：13默认V2、明暗26默认和182资源记录保持；96帧32AA／12恢复／48新旧均0，32组1DIP检出。Mac实际NSWindow managed245／AOT246且182记录一致；两临时源、97媒体和419文件原生工程清理，原40项／NET8零警告、单Token成员审计通过。13Token／1Control回填，Reserved总数20；仅本机默认机制切片完成，完整家族／其余Global／输入／连续动效／其他Host及原生最终像素仍开放。本批未提交，Git另审。

2026-10-26 第二十批授权已执行：13文件快照07512eeba937提交并推送6bd607676，[第二十轮](platform-twentieth-results.md)8/8job成功，三OS各6405项、三NativeAOT各26及两Browser各23；370资产及27对本地化阶段各OS完整。未取消／重试／发布；不含后续Segmented，完整CT25仍开放。

2026-10-26 CT14 N3-S [Segmented本机结果](segmented-defaults-results.md)：14默认V2、三条Own依赖及自动高度0下限按批准设计修复，原版4失败/1框架正对照→候选5通过；280条metadata一致、28默认不变，18条值预期变化。原/候选各288鼠标及192方向键，Mac managed1037／AOT1038且336记录一致；所示布局及四对精确14点差异已批准，原始FAIL保留。四临时源、339媒体及419文件原生工程清理，原30项、NET8零警告及成员审查通过；14Token／2Control回填、Reserved仍20。本机默认依赖切片完成，完整家族／其他Host／原生最终像素继续，Git另审。


2026-10-27 第二十一轮核对：[Actions40291050592](platform-twenty-first-results.md)的attempt1已结束，8/8 job成功；三OS各6405/6405选测、三NativeAOT各26、两Browser各23，源码均278429b50，各370资产和27对本地化阶段完整。未重复调度/取消/重试/发布，第二十一批Git授权已消耗；不包含后续Tab或NavMenu研发，不关闭完整CT25。

2026-10-27 CT14 N3-TAB：[本机结果](container-tabs-defaults-results.md)的34默认V2、一条CardPadding依赖及已批准添加按钮状态恢复已验证。四项默认原3FAIL1PASS→候选4PASS，四项历史35/51→29；最终11项扩展通过，Mac managed1062/AOT1063及408资源相同。所示布局与[N01–N16精确差异](tabs-layout-noise-review.md)已批准，原始FAIL保留；8临时源、303媒体、423文件原生工程清理，原Tab283项/NET8零警告/成员审计通过。回填34Token与4Control，HorizontalMargin仍Undecided，Reserved仍20；直接原生Styles.Remove仍残留91而期望56，不列为已修复，完整家族/其他Host/原生最终像素保持开放。新Git快照须另审。

NavMenu首次展开的[接续TDD结果](nav-menu-initial-collapse-motion-results.md)：原始15项13FAIL2PASS，保存最后coercion输入的候选13PASS2FAIL，被低优先级Style覆盖及Animation恢复反例否决。普通Avalonia Border最小coercion反例也把Animation360恢复成Local300，独立1FAIL保留。产品NavMenu.cs已恢复HEAD，临时源删除，原有307项通过不证明本Bug修复；必须修订有效宽度取值方案，CT14/CT25继续开放，不能重新安装未经验证的镜像字段。


2026-10-27继续执行：用户要求依次完成剩余全部任务，范围扩展至CT11–CT23、CT25–CT26，CT24保持明确暂缓。第二十二批27文件已获批原样提交推送346732602a，三OS/两Browser验证8/8成功，见[第二十二轮结果](platform-twenty-second-results.md)，不作为完整家族验收。

CT14 [HorizontalMargin分类](tab-horizontal-margin-results.md)完成：完整源码/模板/Gallery搜索确认无内置消费者，归Legacy兼容保留，不删除字段或接成别名，运行C#除XML注释外逐字不变；原Tab283项、0Skip及成员审计通过。该字段不再Undecided，容器Reserved仍20。NavMenu/直接Style移除按已批准[FW1源码验证](framework-restore-validation-plan.md)继续，尚未修改正式依赖。

2026-10-27 FW1本地源码验证与恢复完成：69临时/扩展通过、清理后47原有通过、NET8/NET10 API16148记录一致；实际Mac managed/AOT各152项通过，原NavMenu仍为失败负对照。参考源码及XamlX已恢复/删除；用户决定继续官方12.1.4，正式恢复缺陷和NavMenu仍开放。见[源码验证结果](framework-restore-validation-results.md)。

2026-10-27 CT11/CT12：[Hyper loading接线](buttons-hyper-loading-results.md)及[连续动效开关](buttons-continuous-motion-results.md)分包完成并获用户精确视觉批准。原5FAIL/4FAIL分别变为通过；四owner明暗8个默认同相位帧对0，停动四项0、重启有效；最终临时10项、实际Mac managed/AOT各63项、清理后精确模块132项通过，NET8零警告。三份临时源码、148PNG+manifest、外部423文件全部删除。仅主agent作者自审，未提交/未调度Actions；完整CT11/CT12/CT25不关闭。下一项继续核查Hyper icon-only/danger状态激活。

2026-10-27 CT12：[Hyper icon-only/danger](buttons-hyper-states-results.md)原始6FAIL→最终7PASS，四档指标与实际输入状态通过；同步夹具UI时钟阻塞的2654像素失败保留并以等时异步单变量定位，最终当前颜色到达目标的硬断言及明暗配置/退出恢复均0，预期2944/2976精确差异已获用户批准。Mac managed/AOT各129PASS、清理后132项、NET8零警告通过；两份临时源码、132PNG+manifest与外部423文件清理。CT12逐字段/完整家族出口继续，CI不代替验收；无提交授权。

2026-10-27 CT12：[Hyper root绘制](buttons-hyper-root-paint-results.md)8原始消费/Style失败接通；自审3项root默认值失败改用Frame编译空值回退，最终14项、Mac managed/AOT各63项、清理后132项及NET8零警告通过。默认/恢复0，所示定制精确差异已批准。SimpleButton已获用户核定保留原生包装，Native委托/三点输入/局部像素0验证并同步目录；Hyper禁用Own边框在请求线宽时进入真实Frame。两份临时源码、66PNG+manifest、外部423文件删除。[九owner出口核对](buttons-exit-audit.md)保持全家族开放及9/5、4/2原始差异、长寿命Flyout关系、CT20溢出边界；无提交或新Actions。

2026-10-27 CT13 [CR1默认依赖结果](input-check-radio-defaults-results.md)：CR1：11默认V2与三条有效Own边；原5FAIL→最终6PASS，明暗22条默认/key/ID/slot一致；managed/AOT各51，清理后原51项/NET8零警告；直接AA/恢复0，精确配置变化与自然90/76、3/6已单独批准，CT25合成专项及完整家族保持开放。两临时源码、423文件原生工程、268PNG及manifest共269文件已清理且audit clean；产品无刷新补丁，未提交。下一项继续RadioBgColor分类及其余输入消费。

2026-10-27 CT13 CR2：CR2：完整产品源码/模板/Gallery/测试搜索仅有RadioBgColor声明和默认，无内置消费者；选中背景与边框直接读取有效Global.ColorPrimary。归Legacy/CompatibilityRetained，保留Color、key/ID/slot、V2默认及配置/资源读取；不新增背景行为或别名，CT23按历史保留展示。产品输入与CR1最终51项/NET8一致。见[CR1结果中的兼容分类](input-check-radio-defaults-results.md#radiobgcolor-兼容分类)。不关闭完整家族或CT25。

2026-10-27 CT13 [CR3 Global结果](input-check-radio-global-results.md)：CR3：六AXAML/38个已有Global资源入口接到四个公开owner与内部指示器；8有效RED加修正算法后的2有效RED→15PASS，0/恢复扩展14PASS；Mac managed/AOT各54，清理后原51项、NET8零警告。默认/输入原→新0，精确配置5015/5015及三点恢复3/3已批准；原始失败保留CT25。临时2源、419文件原生工程、108PNG+manifest全部清理，无Git/完整家族批准。内部列表继续Group模板绑定；时长既有Owner消费已验证，未重复改造。下一项核查字体继承及其余输入选择消费者。

2026-10-27 CT13 [CR4字体结果](input-check-radio-typography-results.md)：CR4：四个公开owner复用内部真实继承父只读桥与三输入字体转换器，八个AXAML Setter显式消费有效Global FontFamily/FontSize；默认/清除继承、Style/Local及相等值存在位正确。8RED→13合同/GC通过（7对象回收），完整默认/父字体Frame通过，224条非配置数据一致；Mac managed/AOT各8窗口433项，NET8零警告34.62s，清理后原51项。配置字体及N01–N07精确25/32点差异已批准，原始FAIL留CT25。两临时源、419文件原生工程、314PNG+manifest均清理；非全家族，无Git授权。本包仅作者另步自审；下一步继续余下输入/选择消费者和完整家族出口。

2026-10-27 CT13 [CB1 ComboBox默认结果](input-combo-box-defaults-results.md)：CB1：ComboBox11项Global-only默认V2 rev1，无Own边；原注册协议1FAIL/旧GlobalOwn正对照1PASS→候选3PASS，明暗22条默认/类型/key/ID/slot相同。保留三行cap/四项/滚动条节点可见/真实DownEnter，22对新旧完整帧及AA/复位/配置恢复0；1DIP7643/3933检出。Mac产品Window/Overlay managed/AOT各102，NET8零警告24.82s；两临时源、419文件工程、44PNG+manifest清理，原ComboBox69PASS/0Skip，单Token审计0问题。仅默认机制，ItemDisabledColor/HandleHoverColor消费及完整家族/CT25继续，无Git。

2026-10-28 CT13 [CB2禁用色](input-combo-box-disabled-results.md)：CB2：禁用Foreground唯一资源入口接Own ItemDisabledColor；原1FAIL/既有HandleHover与Frame2PASS→3PASS20s，明暗仅显式disabled文字各402点已单独批准，其他/AA/复位0。Mac managed/AOT各42、NET8零警告23.79s；两临时源/419文件工程/49媒体清理，原69项。专用Style优先级扩展另由CB3闭环，非全家族，无Git。

2026-10-28 CT13 [CB3专用Style路由](input-combo-box-item-route-results.md)：CB3：popup.listItem真实直接逻辑父为ComboBox，原从视觉PopupFrame查逻辑后代不命中；route改> .semantic-popup-list-item，marker/ContractType/跨根/Multiple保持，无代码属性回退。原4FAIL→4PASS22s，真实编译专用Style类/Setter核对、Own更新/撤回及文本通过；预编译x:Class managed/AOT各54，消除夹具URI入口IL2026，无抑制。八对精确Style变化已单独批准，其余20对/AA/复位0。临时C#/AXAML、334文件工程、57媒体清理，原69项/NET8零警告24.89s；完整家族/CT25继续，无Git。

2026-10-28 CT13 [S1 Select默认协议](input-select-defaults-results.md)：S1：Select25项Global-only V2 rev1；50默认/type/key/ID/slot前后相同，4临时+原前缀5PASS1s；原前缀断言保留仅改生成入口。正确主题背景48对旧新/480状态记录0差异；严格原/候选启用恢复Light77(max1)/Dark155(max3)仍FAIL，直接0，保留CT25。Mac managed/AOT各256；NET8零18.57s，成员1/0/0；两临时源、422文件原生工程、207媒体清理且auditclean；清理后原三模块148PASS0Skip6s。仅作者另步自审，完整家族/其余消费开放，无Git。

2026-10-28 CT13 [CB4显示区候选](input-combo-box-display-colors-results.md)：真实原版6FAIL/4PASS加Warning2FAIL→最终13PASS；只接占位画刷及限制禁用时错误/警告文字，撤销无效根getter假设和多余根Setter。managed/AOT各108、NET8零18.47s、临时4源/430文件原生工程/134媒体清理、原Combo69/Gallery8通过；新26对具体视觉范围待单独批准，未验收、无Git；完整家族和CT25继续。

2026-10-28：CB4的[限定视觉范围](input-combo-box-display-colors-visual-review.md)已获本次独立批准；候选完成本机切片出口，原始失败及纠正保留。CB4：占位默认/公开画刷与禁用错误警告文字修复独立视觉已批准；8有效RED→13PASS，260根记录/getter不变；managed/AOT108各、NET8零18.47s；临时4源/430文件工程/134媒体清理后Combo69/Gallery8通过。原根localForeground实际禁用绘制与其余完整家族/CT25仍开放，无Git。

2026-10-28 CT13/CT15 [NU1候选](input-numeric-spinner-defaults-results.md)：NU1候选：8默认V2rev1、FilledHandleBg读取有效Own.HandleBg；16默认/type/key-ID-slot一致，原5FAIL3PASS→8PASS，528状态记录相同。managed/AOT246各，NET8零20.98s；2临时源/422文件工程/565媒体清理，原65/Gallery1通过。直接绘制26非修复阶段0、18背景变化；自然N01-N14具体差异待独立批准，完整家族/CT25开放，无Git。

2026-10-28 NU1：8默认V2rev1及Filled<-HandleBg有效Own边完成；16默认/type/key-ID-slot相同。原5FAIL3PASS→8PASS；managed/AOT246各、NET8零20.98s；临时2源/422文件工程/565媒体清理、原65/Gallery1通过。18背景修复与N01-N14已获独立批准，原自然/直接/同版计数保留，完整CT13/15/20/25及绘制历史问题开放，无Git。

2026-10-28 [NU2-A候选](input-numeric-inline-icons-results.md)：NU2-A候选：4个加减图标尺寸入口改Numeric有效Global.IconSize；默认14/Input7不变，子IconButton颜色身份保留。原4FAIL1PASS→6PASS；160根/320动作非尺寸记录相同。managed/AOT454各、NET8零23.53s；临时2源/422文件工程/81媒体清理、原Numeric49PASS2s。四对22/0图标变化及四对自然小差异待独立批准，不沿用NU1范围，无Git。

2026-10-28 NU2-A：四图标尺寸入口读取Numeric有效Global，保持Input7/Spinner默认14与子颜色身份；4RED→6PASS，managed/AOT454各、NET8零23.53s，临时2源/422文件工程/81媒体清理后原49PASS。四配置glyph变化与四对精确自然差异已独立批准，完整家族/CT25开放，无Git。

2026-10-28 [NU2-B候选](input-spinner-stroke-results.md)：NU2-B候选：两处SpinnerBorderThickness Shared改真实Owner，原4目标FAIL→5PASS；320根记录不变，managed/AOT196各、NET8零19.48s；临时2源/422文件工程/161媒体清理后原65PASS2s。18配置描边变化及6对默认hover自然差异待独立批准，完整家族/CT25开放，无Git。

2026-10-28 NU2-B：两内部SpinnerBorderThickness改真实Owner；4RED→5PASS，managed/AOT196各、NET8零19.48s，临时2源/422文件工程/161媒体清理、原65/Gallery1通过。18描边修复与6默认hover自然差异已独立批准，完整家族/CT25开放，无Git。

2026-10-28 [NU3结果](input-numeric-divider-results.md)：Numeric inline两分隔边绑定父BorderThickness对应Right/Left，4RED→5PASS；managed/AOT各590、NET8零20.56s，临时2源/422文件工程/142媒体清理后Numeric49PASS。自然/直接26对中16均0、10显式厚度变化已获[独立批准](input-numeric-divider-visual-review.md)；不关闭CT13/CT20/CT25，无Git。

2026-10-28 [S2结果](input-select-option-padding-results.md)：Select候选项Own.OptionPadding接通Frame，原4RED→6PASS、原生固定弹层managed/AOT各2044、NET8零19.92s；临时4源/426文件工程/177媒体清理后Select71PASS。自然/直接44对计数相同，24对默认/清除等0、20对配置变化已获[独立批准](input-select-option-padding-visual-review.md)；完整家族/原生像素与输入/CT25仍开放，无Git。

2026-10-28 [S3结果](input-select-internal-owner-results.md)：三选择owner的6处内部间距及1处计数资源改用实际owner，6有效间距RED及3纠正后的颜色RED→10PASS；managed/AOT各290、NET8零18.12s，临时3源/422文件工程/113媒体清理后精确三模块148PASS。两绘制后端各14非修复对0，14配置变化已获[独立批准](input-select-internal-owner-visual-review.md)；原始无效画刷类型断言/纠正过程保留，完整家族/CT25仍开放，无Git。

2026-10-28 [S4结果](input-select-handle-results.md)：手柄父组合IconSize及反馈SpacingXS修复，6RED/6正对照→13PASS；原颜色/真实hover清除和三编译专用Suffix Style恢复保持。最终managed/AOT各4202、NET8零19.88s，成员1文件/1控件/0问题、50成员整理前后指纹一致；临时5源/426文件工程/201媒体清理后三模块148PASS。默认/恢复24对0，[26对配置视觉](input-select-handle-visual-review.md)已独立批准，含Select Middle/Custom配置23时32→33自然高度；完整家族/CT25开放，无Git。

2026-10-28 [S5](input-select-legacy-fields.md)：SelectAffixPadding/FixedItemMargin完成Legacy/CompatibilityRetained源码分类，保留默认/类型/资源与配置合同，不增造内置效果；正式文档纠正SingleModePadding三档。C#仅XML注释，非注释源码严格相等；成员1/0/0、原Token1PASS19ms，无新增测试/媒体/原生工程、无视觉变化或Git，完整家族开放。

2026-10-28 [S6结果](input-select-option-metrics-results.md)：公开候选字体、Own最小行高与均匀普通行DisplayPageSize预算接通；8RED/1正对照→12PASS，本轮26s，原生managed/AOT各1442。两后端各60对、24默认/恢复0及36配置变化，见[限定审查](input-select-option-metrics-visual-review.md)，已获用户明确限定批准。4临时源、426工程文件、241媒体已清理且audit clean；精确SelectControl71PASS4s、NET8零警告/错误、成员2文件/2控件/0问题。完整家族与CT25保持开放；整任务完成后commit授权已在原会话收到，当前不提前提交。

2026-10-28 N3-SC13 [TreeSelect/Cascader默认结果](container-select-defaults-results.md)：1+12字段V2 rev1，2实际V1 RED→5PASS9s；104原/新值与type/key/ID/slot一致，两后端56对旧新及16对恢复均0。managed/AOT各123，字重别名均按精确600/400校验；NET8零20.42s、成员2Token文件/0Control/0问题。3临时源、422工程文件（1,228,223,829 bytes）、113媒体已清理且audit clean；精确两模块77PASS6s。仅本机默认迁移，DropdownHeight与行数预算的消费者所有权、标签身份、完整家族及CT25仍开放。用户已同意下一高度包取Own上限与更小页数预算，具体视觉另审；当前不提前Git提交。

2026-10-28 CHT [Cascader高度结果](input-cascader-dropdown-height-results.md)：已按用户选择合同合并Own上限与较小名义页数预算；6有效RED→最终15PASS24s，原生实际Overlay/Native managed及AOT各1044，156内容不减为144。NET8零27.02s、成员1辅助文件/0Control/0问题；6临时源、426工程文件（1,220,202,448 bytes）、313媒体已清理且audit clean，精确Cascader51PASS3s。两后端各78对/70变化，[限定审查](input-cascader-dropdown-height-visual-review.md)包括默认320→180、自然列宽193→186（单变量证明确由实现标签范围导致），2026-10-29用户已明确批准本表限定范围（高度320→180、自然列宽193→186、末行部分可见）。其余8对0，原整Panel复位3像素保留CT25；完整家族/其他模式/列/平台开放，不提前Git提交。

2026-10-29 用户明确要求提交当前 custom-theme 快照；本次授权立即提交当前任务相关改动，不等待全部任务结束。提交不表示完整家族/整项任务完成，不代替Cascader高度包70对限定视觉批准；不含推送、发布或新建分支/worktree。原整任务完成后提交授权与本次快照提交分别记录。

2026-10-29 CT11总出口完成，CT25继续保留[8对限定合成历史差异](motion-exit-compositor-review.md)、Windows专用CaptionButton真实状态及其他Host原生最终像素/输入。两份限定批准不构成通用容差或平台通过。

2026-10-29 CT13 [S7 add-on结果](input-addon-owner-results.md)：三个内部主题69处Shared来源改为各自公开owner有效Global，12RED→19专项、Mac managed/AOT各325；临时2源/430工程文件/89媒体清理后精确三模块148PASS7s、NET8零21.20s。默认/hover/默认错误等24对和直接清除2对0；[8组配置与2组自然清除边缘](input-addon-owner-visual-review.md)已获用户限定批准，原始1/6点保留CT25，不放宽容差。Cascader CHT已在本会话明确获限定批准；完整CT13仍进行中，CT12代码和进度未改，不提交。

2026-10-29 CT13 [CB5结果](input-combo-box-global-results.md)：手柄3Global/公开候选1Global资源改读准确identity，3RED+1positive→8PASS；Mac实际Overlay与Native managed/AOT各355，NET8零19.32s；4临时源/427工程文件/289媒体清理后ComboBox69PASS2s。144对中96对默认/清除等0，24配置场景在两后端各有精确变化；[限定视觉](input-combo-box-global-visual-review.md)包含Middle/Custom自动高度32→33，已获用户明确批准。无API/Own/route/C#生产改动；字体与完整CT13继续，无提交。

2026-10-30 CT13 [AM1默认](input-autocomplete-mentions-defaults-results.md)完成7字段V2、70原值/meta相同及隔离22对RGBA0；[AM2指标](input-autocomplete-mentions-metrics-results.md)完成padding依赖与候选最小行高消费，6RED→联合18PASS，Mac managed/AOT543各、NET8零17.52s；8临时源/427工程文件/162媒体与工具fixture清理，最终28+1PASS及4文件/2Control/0成员问题。纯结构404成员指纹和初始化序列相同，行为仅两个回调改变；检查器guard/ref识别已用3临时fixture验证。[48个popup视觉](input-autocomplete-mentions-metrics-visual-review.md)包含默认预算96→100与viewport92→96，待限定审查，不代替完整CT13。S7与CB5已分别获批，字体/其余Global/Own/状态/家族出口继续，无提交。

2026-10-30 CT13 [AM3字体结果](input-autocomplete-mentions-typography-results.md)：四root有效字体与标准API声明式进入真实输入，9消费者+2glyph行框RED→24PASS2s；16默认实际文本值不变，多行root8个getter与原可见预设对齐。当前Mac managed/AOT各1686、NET8零20.25s、成员3文件2Control0问题；5临时源/7备份/427工程文件/89媒体清理后精确52PASS2s。[6对配置字体/Local变化及root值对齐](input-autocomplete-mentions-typography-visual-review.md)待限定批准，16对默认/恢复等0；原暗局部主题混合白表面禁用低对比仍归CT25，匹配暗表面补充不替换原历史。AM2视觉、其余Global/Part/状态/Form/Upload与完整家族继续，无提交。

2026-10-30 CT13 [AM4](input-autocomplete-mentions-surfaces-results.md): 9actualRED->15PASS1s;8dedicatedStyles/12GC,4sizes/transparent/foreign/local/clear/retemplate/scope. Mac managed+AOT834each,NET8zero18.58s/member1aux0control0;7temps28008B/2copies/native427/media201clean,final52PASS2s. Natural48pairs:28zero/8configured/12edge1or5(max1)rawFAIL;direct40nonconfigured0/8configured. [Scoped review](input-autocomplete-mentions-surfaces-visual-review.md) pending for8configs/2loading layouts/12edges;CT25open. [Remaining audit](input-autocomplete-mentions-remaining-audit.md):16body/caret/selection RED,Form8+Upload13 V1,Part/state/family exit open. No Git.

2026-10-30 CT13 [AM5 owner](input-autocomplete-mentions-global-owner-results.md):16body/caret/selection+4standardForeground+4nativeResources RED ->39finalPASS2s;qualifiedSourceControlType86input+4SearchButton refs,4readonlyprivate-owner members,47oldmember fingerprints equal/member4files2controls0. Rejected per-instance ResourceProvider/Core-snapshot prototype fully removed;final uses existing native resource observers and no Token dictionary/scope/cache/global subscription. Mac managed+AOT6117each/NET8zero20.29s;10temps/15copies/3codeinventories/native439/media481clean;finalDesktop157PASS6s/Core22PASS207ms. [48scene/gutter review](input-autocomplete-mentions-global-owner-visual-review.md) pending:44natural default/restores0,4exactclear edgeFAIL2/2/13/15 (max1/2/4/6) CT25 retained;direct24nonconfig0. AM2/3/4 notapproved by continuation. FU1 Form8+Upload13 defaults next;fullCT13open/noGit.

2026-10-30 CT13 [FU1 defaults](input-form-upload-defaults-results.md):Form8+Upload13 V2rev1 Global-only/constants,2protocolRED+2snapshotpositive->7finalPASS1s;168values/types/key-ID/slot exactequal incl47.5*2.55=121.12499999999999;14Skia old/newpairsRGBA0,3Formlayouts/4Uploadmodes/drop and20activeOwn targets observed. VerticalLabelPadding Legacy/CompatibilityRetained resource-only,20/30/40/50 addsnolayout. Mac managed+AOT699each,NET8zero25.57s/member2aux0/Generator1PASS.5temps19728B/2copies/native424/media29clean;finalForm+Upload167PASS2s;2stalesourceassertions updatedwithout productionmotionchange. NoGit/fullCT13open;remaining Global/fonts/Parts/states next.

2026-10-30 CT13 [FU2 public surfaces](input-form-upload-global-surfaces-results.md):6publicthemes54Shared->exacttypedGlobal refs;12complete-contract actualRED1s->15PASS4s,3compiled dedicatedLabel/List/ItemStyles hit+withdraw4uploadmodes,4weakgraphs/Local/scope/retemplate. Mac managed+AOT81each,NET8zero19.59s;9temps25554B/6copies/native427/media71clean;finalFormUpload167PASS2s after2old source assertions updated. [Scoped review](input-form-upload-global-surfaces-visual-review.md) pending4config/disabledscenes+darkclear2max1 rawFAIL;natural9/10nonconfigzero,direct10/10zero. Font/DecoratorDarkExtra inheritedcontrast/internalUpload remainingGlobal/otherInput parts-states-familyexit stillopen. NoGit/noCT12production-progressedits.

2026-10-30 human approvals:AM5 andFU2 scoped review tables explicitly approved(call_137bdbb52074425385b7e76e4759ef93:0/1);CT25 original raw failures retained. Not fullCT13/otherfonts/platforms/Git/tolerance. AM2/AM3/AM4 still pending.

2026-10-30 [FU3 realfont audit](input-form-upload-typography-audit.md):7public FontFamily consumers realRED1s,1default-record positive. Root23:Form/FormItem label14,Decorator/Upload/Trigger/DropZone23,DropAreaHeader16. Member7files7controls22existingissues;no fontproductionedits.2audittemps/14copies/1codeinventory/media21clean. AM5/FU2 scopedapproval recorded,CT25retained;AM2/3/4pending;fullCT13incomplete.

2026-10-30 CT13工程出口完成：FU3七公开字体族、FU4七根可选字号/暗色正文与冒号、Upload内部11主题69Global来源及圆形往返、三选择私有过滤输入颜色全部接通。52临时/28对象GC、真实Mac managed及AOT各810、NET8零及成员9/8/0；16临时源/42源副本与含源码inventory/427原生文件/223媒体全部清理。最后精确Desktop719/Core12/Generator1/Gallery23共755PASS0Skip；CT12六临时及31保护hash保持，无新分支/worktree/agent/提交/全量回归。目录91项与Own93逐条对账；4Legacy不虚构消费。AM2/AM3/AM4/FU3/FU4五张限定视觉待明确批准，已批准范围保持，CT25/FW1独立保留。见[家族结果](input-family-exit-results.md)、[核对矩阵](input-family-exit-matrix.md)、[最后限定视觉汇总](input-final-visual-review.md)。

2026-10-30 最终关闭：用户明确批准AM2/AM3/AM4/FU3/FU4五表限定范围（request_user_input_async / call_5f004b68efc344048653a3d95ae55694 / item 0），原话“批准五表限定范围，保留 CT25/FW1”。CT13研发、清理、755项精确验收及当前Mac managed/AOT各810验证记录与全部限定视觉批准对齐，CT13已完成并关闭。CT25/FW1独立保留，不扩大其他字体/平台、恢复模式或像素容差；CT12代码/进度及六临时保持，未提交。见[最终批准](input-final-visual-review.md)及[结果](input-family-exit-results.md)。

2026-10-30，用户已明确回复“批准本表限定范围，保留 CT25”（request_user_input_async / call_a2fe796a8df0481aadd7e4393060b87e / item 0）。公开BoxShadow工程、默认原生验证、清理后189项及限定视觉批准已全部完成，CT12在既定批准范围完成并关闭。原始24/max1、两对3968/max2及其他既定历史仍按CT25保留，不设通用容差，不扩大其他平台/背景/恢复模式，无Git授权。


## 2026-10-30 最终批准与关闭登记

用户通过 `request_user_input_async / call_656cee76f0f3440cb7dfe8d373826c6d / item 0` 明确回复：**“批准本表限定范围，保留 CT25/FW1”**。本次 V1–V7 限定视觉批准与工程、清理、Desktop1014/Core17、NET8零警告及当前Mac managed556/AOT557证据已对齐，CT14在既定批准范围完成并关闭。

原始像素失败、TabStrip指示线复位、coercion/Style退役及其他既定CT25/FW1边界保持，不将其记为产品修复或平台像素通过，不扩大容差、其他字体/配置/Host/平台。CT12、CT13和Cascader既定高度批准保持；本次批准不授权Git提交、推送、发布或依赖替换。仅同步文字状态，产品源码不变，沿用相同最终输入的已完成验收。


2026-10-31 CT15：用户分别批准Card表内32对及两属性纠正、Message P1表内8对及root有效值、Notification表内36对及root有效值；批准原话与请求ID分别登记在三份限定表和CSV。所有排除项、P2及完整CT15保留，无Git授权。


2026-10-31 CT15 Avatar当前切片：[结果](avatar-consumers-results.md)、[48对限定视觉已批准](avatar-consumers-visual-review.md)、[作者另步自审](avatar-consumers-review.md)。3metadata/11Own全Active V2R1、44默认/type/key/slot与自然文字14保持；连续Custom/Circle、字体/owner/Group投射、membership退出及真实overflow热资源发布修复，39匹配验证、managed/AOT各100检查，136跨运行134零；两版主业务恢复各36零。closed/往返微差全部保留。18临时/1843取证/411driver/551原会话清理，最终Controls7/Desktop14及NET8零、成员5-2-0。仅48有意变化已获用户限定批准；完整Avatar/CT15、OS/Browser/Popup过程时相保留。下一[Tag](tag-consumers.md)已有4有效RED/5PASS、36默认导出，尚未改产品；NeedsDecision8及137/274不变，无Git。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_7b292d7ef454420fb8fc0603ac3e3f4c / item 0）。仅本表48对有意变化获批准；closed/往返微差、OS/Browser/Popup过程时相及完整Avatar/CT15继续保留，不授权提交。


2026-10-31 CT15 Tag：[结果](tag-consumers-results.md)、[26对限定视觉已批准](tag-consumers-visual-review.md)、[作者另步自审](tag-consumers-review.md)。8metadata/9Own全Active V2R1、36默认/type/key/slot保持；真实字体/owner/关闭glyph、container退役强链修复，84匹配验证、managed/AOT各120，164跨运行159零；原微差及派生输入大值影响保留，不改CT13批准。18临时/1090取证/外部driver与会话清理，最终66/NET8零、成员18-4-0。NeedsDecision余7，137/274不变；仅26有意帧已获限定批准，完整Tag/CT15不关闭。下一[Badge](badge-consumers.md)13metadata/20Own仅准备反例，未改源，无Git。


限定批准登记：2026-10-31，用户明确回复“批准表内26对及列明派生影响，其余未通过与完整CT15保留”（request_user_input_async / call_5cea4f7e59a243dcab027c42807d9848 / item 0）。仅26对与列明SelectTag继承close37/0导致的换行/原高度裁剪影响获批准；恢复、5对跨运行微差、原CT13批准边界、OS/Browser及完整Tag/CT15保留，不授权提交。


2026-10-31 CT15 Badge当前切片：[结果](badge-consumers-results.md)、[48对限定视觉已批准](badge-consumers-visual-review.md)、[作者另步自审](badge-consumers-review.md)。13metadata/20Own全Active V2R1、80默认/type/key/slot保持；真实owner字体/跨根发布/专用Style/实例颜色clear与投射退役修复。57所选56PASS、Count composition GC1FAIL保留；managed/AOT各72检查、164跨运行全零，4无定制默认旧新零，显隐/重挂及校准恢复非零不豁免。25临时用例3源26658字节、2524取证、419driver/15publish（原会话共537文件）已清理，清理后既有Badge32PASS/0Skip及NET8零警告零错误23.02s，成员22-7-0。NeedsDecision7与137/274不变；仅申请48对明确变化，完整Badge/CT15和CT25/FW1边界保留。下一按实际依赖推进ImagePreviewer/Carousel/Result，无Git提交。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_d223a18435b5484787082d74b8ee393e / item 0）。仅Badge表内48对有意变化获批准；Count composition GC1FAIL、显隐/重挂/校准恢复非零、其余108对及完整Badge/CT15继续保留，不授权提交。


ImagePreviewer current checkpoint: [execution](image-previewer-consumers.md). 13 metadata/14 Own, 56 default records unchanged, 142 focused checks passed (25 temporary/117 existing), member audit 27 files/12 controls/0 issues with exact pure-move fingerprints. Public owner/optional switch size/Global-font/real dedicated styles/Overlay bridge repairs implemented; normal-Skia old/new/AOT replay is in progress. Temporary cleanup and final verification pending; full family and CT15 remain open. Next [Carousel](carousel-consumers.md) is read-only preparation, no new product changes during shared sampling.


2026-10-31 CT15 ImagePreviewer：[结果](image-previewer-consumers-results.md)、[104对限定视觉已批准](image-previewer-consumers-visual-review.md)、[作者另步自审](image-previewer-consumers-review.md)。13metadata/14Own V2R1、56默认/type/key/slot保持；可选切换尺寸、公开owner/Global字体、七部件专用Style与Overlay桥修复。142临时阶段全通过（25临时+117既有）；managed/AOT各200检查、448帧，384静态355跨运行零、29静态及64持续态非零保留。三版各32恢复全零，独立A/A/恢复零、1DIP各408/max155检出。3临时源18995字节、1401取证、原505会话（含driver414/publish15）已清理；清理后117PASS/0Skip及NET8零警告零错误22.76s，成员27-12-0纯移动指纹同。NeedsDecision余6，137/274不变。仅申请104明确变化，其他非零、OS/Browser/输入/极值及完整ImagePreviewer/CT15保留；下一Carousel6/7按有效反例推进，无提交。


限定批准登记：2026-10-31，用户明确回复“批准表内104对，其余未通过与完整CT15保留”（request_user_input_async / call_5593304c893e46a296348b6914a561c3 / item 0）。仅ImagePreviewer表内104对明确变化获批准；其余171对旧新非零、29静态/64持续态跨运行非零、OS/Browser/输入/极值及完整ImagePreviewer/CT15继续保留，不授权提交。


2026-10-31 CT15 Carousel：[结果](carousel-consumers-results.md)、[44对限定视觉已批准](carousel-consumers-visual-review.md)、[作者另步自审](carousel-consumers-review.md)。6metadata/7Own V2R1、28默认/type/key/slot保持；可选ArrowOffset、Global/字体/通知、空集、投射/进度/模板与caller-owned页退出修复。26聚焦+2持续工作检查通过，managed/AOT各48/128帧，全部112静态跨运行0、候选/AOT各44恢复0；4跨运行时相与16旧新持续态保留。25临时1源17042字节、481取证、原516会话（含driver408/publish15）清理；清理后3PASS/0Skip（含两项原持续工作）及NET8零警告零错误18.04s。成员最终9-4-0，两阶段纯移指纹同。NeedsDecision余5，137/274不变；仅44明确静态申请批准，平台/输入/极值/完整家族与CT25/FW1保留。下一Result2/15，无提交。


Result当前切片：[执行](result-consumers.md)、[结果](result-consumers-results.md)、[作者自审](result-consumers-review.md)。2metadata/15Own，60默认精确保持；19匹配验证通过，候选四算法Skia68检查通过，旧新/AOT进行中。字体/Global/脱离状态回放修复，五专用Style及七状态/原插画内容保留，临时清理和最终模块/NET8尚未完成，不关闭家族。之后按[剩余交叉输入](feedback-cross-family-consumers.md)继续，不重复ButtonSpinner/既有批准机制。


限定批准登记：2026-10-31，用户明确回复“批准表内44对，其余未通过与完整CT15保留”（request_user_input_async / call_aa6a1cc8835941ae997eada0a60a4e92 / item 0）。仅Carousel表内44对静态变化获批准；其余16对旧新持续态、4对跨运行时相、平台/输入/极值及完整Carousel/CT15继续保留，不授权提交。


2026-10-31 CT15 Result：[结果](result-consumers-results.md)、[60对限定视觉已批准](result-consumers-visual-review.md)、[作者另步自审](result-consumers-review.md)。2metadata/15Own V2R1、60默认/type/key/slot保持；字体/Global/attach回放、逐glyph Template优先typed relay与退出修复。24匹配验证（15临时+9既有）、managed/AOT各72检查/132帧，跨运行132全0、候选/AOT各32恢复0、四默认旧新0。原pre-glyph384帧/18恢复失败账本保留，最终旧132中60有意差异含Native glyph27×31/Orange优先修正。3临时源16187字节、846取证、原501会话（含driver414/publish15）已清理；清理后9PASS/0Skip及NET8零警告零错误17.21s。成员8-2-0，早期7项纯移指纹同、后功能变化单列。NeedsDecision5与137/274不变；资源GC判据不足/平台/自然极值及完整Result/CT15保留，不扩CT25/FW1。下一按既定归属推进FloatButton13/8与剩余交叉，无提交。


限定批准登记：2026-10-31，用户明确回复“批准表内60对及列明原生优先修正，其余边界与完整CT15保留”（request_user_input_async / call_99a149043b344680ba0a26e4e74cb239 / item 0）。仅Result表内60对与自定义glyph27×31/Orange原生优先修正获批准；原pre-glyph384帧/18失败文本、GC判据不足、平台/极值/完整Result与CT15继续保留，不授权提交。


2026-10-31 CT15 FloatButton：[结果](float-button-consumers-results.md)、[65对限定视觉已批准](float-button-consumers-visual-review.md)、[两offset Reserved已批准](float-button-token-ruling.md)、[作者另步自审](float-button-consumers-review.md)。13metadata/8Own V2R1、32默认/type/key/slot精确保持；6Active真实消费者、2legacy scalar保留未决，NeedsDecision5不减。95临时阶段通过（49临时+46既有），BackTop正常显隐/精确滚动/Caller绑定、badge/成员退出、真实Style与Group reflow修复；managed/AOT各72检查68PASS/4最终恢复FAIL，200跨运行166零（静态162/168零，6静态/28定时非零保留）。仅65明确静态与两Reserved提案待限定审查，原文字/窄宽/字体23换行及窗口上缘裁剪列明；其余115旧新非零、主恢复、平台/极值/GC及完整家族未关闭。3临时源32518字节、777取证、537原会话（含driver414/publish15）与6最终文件清理；清理后Float46PASS/0Skip、NET8零警告零错误25.23s，成员19源/10含注册类型0问题。137/274与CT12/13/14既定批准、CT25/FW1边界不变，不提交。下一[Drawer3/13](drawer-consumers.md)已只读准备，按实际反证继续。


限定批准登记：2026-10-31，用户明确回复“批准65对及两项Reserved，其余保留”（request_user_input_async / call_e7be1ded2cd04cea8d90b50314dba528 / item 0）。仅FloatButton表内65对明确静态（含列明原窄宽换行/窗口上缘裁剪）与CircleBadgeOffset/SquareBadgeOffset两项Reserved兼容分类获批准；两字段仍无scalar消费者，不改写成Active/ConsumerVerified，不缩分母。NeedsDecision由5降为3，仅余GroupBox三字段。其余115旧新非零、34跨运行非零、恢复/平台/GC/极值及完整FloatButton/CT15保留，CT12/13/14既定批准和CT25/FW1不变，不授权提交。


2026-11-01 CT15 Drawer：[结果](drawer-consumers-results.md)、[20对限定视觉已批准](drawer-consumers-visual-review.md)、[作者自审](drawer-consumers-review.md)。3metadata/13Own V2R1、52默认精确保持；73聚焦（35临时+38既有），Global/可选字体、标题省略和两层重模板回放修复。原预修复588帧/8中途失败、校准失败47帧及旧AOT错批196帧留账。最终有效三版各196帧，候选/AOT各116检查、164静态跨运行0、各68恢复0。20明确静态已获限定批准，21旧新/20跨运行定时非零保留。35临时3源25362字节、1518取证、550原会话与4最终文件清理；清理后38PASS/0Skip、NET8零警告17.97s，成员7-3-0。137/274与Own未决3保持，完整家族/CT15/平台/GC/极值及CT25/FW1保留，不提交。下一[Tour7/9](tour-consumers.md)，未启动余35metadata/71Own。


2026-11-01 CT15 Tour：[结果](tour-consumers-results.md)、[44静态已获限定批准](tour-consumers-visual-review.md)、[作者自审](tour-consumers-review.md)。7metadata/9Own全Active V2R1、36默认精确保持；60聚焦（31临时+29既有）通过，退投射/动态margin/12专用Style/动作及模板/目标跟随与失锚修复。非CSD最终flip报告按实际margin输入校正，CSD/旧批准不改；成员16源/7控件0问题。候选/AOT各212帧/112检查，cross198零/14非零，各80主恢复零；旧版同动作模板异常只到140帧、缺72不能称完整，old独立cal16帧另计。仅44明确静态已获限定批准，2静态micro/12跨运行定时/16旧新定时及缺帧后历史/自然极值/GC/平台/完整CT15保留。31临时3源29446字节、1448取证、614原会话（driver415/两publish各15）与4最终文件清理；清理后29PASS/0Skip、NET8零警告17.12s。137/274与Own未决3保持，下一[Flyouts4/11](flyouts-consumers.md)，未完成当前家族之外未启动余28metadata/62Own，不提交。


2026-11-01 CT15 Flyouts：[结果](flyouts-consumers-results.md)、[48对待限定视觉审查](flyouts-consumers-visual-review.md)、[作者自审](flyouts-consumers-review.md)。4metadata/11Own全Active V2R1、44默认/type/key/slot保持；40聚焦（23临时+17既有）通过，实际owner字体/背景、跨scope与热Popup offset、真实Tree click/scroll和pinned重模板修复。候选/AOT各196帧/144检查PASS，180静态cross0、12定时非零；旧版144主帧/100检查40FAIL、缺52，独立16cal不填主历史。48明确静态全cross0，其他4恢复micro/12定时、原裁剪与暗Own配色、平台/完整出口保留；有效背景null→Own及Compact Pointer8→4列明。23临时3源25325字节、1191取证、488原会话（driver320/两publish各15）与6最终文件清理；最终Flyouts17PASS/0Skip、NET8零警告19.83s，成员14源/7控件0。137/274与Own未决3保持，完整家族/CT15及CT25/FW1开放；下一[Collapse2/15](collapse-consumers.md)，当前家族之外未启动24metadata/51Own，不提交。


2026-11-01 CT15 Collapse：[结果](collapse-consumers-results.md)、[12静态待限定视觉审查](collapse-consumers-visual-review.md)、[作者自审](collapse-consumers-review.md)。2metadata/15Own全Active V2R1、60默认精确保持；75临时阶段73PASS/2GC FAIL，Global字体/原角色、runtime item退休和四专用Style验证完成，GC保留。三版各220帧，候选/AOT各128检查PASS，204静态cross全0、11定时非零；old/new196零24非零，仅12明确静态待审。三版各76主恢复0，另4 Global恢复5/max1；原长文/Extra/520宽/裁剪与暗色显式低对比列明，GC原始和各独立/整批失败不映射FW1。25临时3源21834字节、723取证、518原会话（driver414/最终publish15）及6最终文件清理；清理后50PASS/0Skip与NET8零警告18.27s，成员5源/2控件17→0纯指纹和字段序列同。137/274与Own未决3保持，完整家族/CT15开放；下一[Descriptions6/10](descriptions-consumers.md)，其他未启动22metadata/36Own，不提交。


2026-11-01 CT15 Descriptions：[结果](descriptions-consumers-results.md)、[16静态待限定视觉审查](descriptions-consumers-visual-review.md)、[作者自审](descriptions-consumers-review.md)。6metadata/10Own全Active V2R1、40默认精确保持；运行只有Root独立Own10、五模型/内部视觉无独立fragment，维持原身份不缩输入。81聚焦PASS（48临时+33既有），Global/角色、generated cell退休/Native parent/focus/集合/offtree快照修复。主三版164静态、候选/AOT124检查PASS，静态cross全0及56+4恢复0；仅16有意变化待审。原主Native缺40/异常保留，无模板原生探针不算glyph证据；独立可见TextBox候选/AOT44帧44检查/cross全0，原初始4后异常/缺40不填。520宽三列窄槽/长Wrap/下缘裁剪及暗低对比列明，平台/完整出口/CT15开放。48临时3源24053字节、1033取证、569原会话（driver415/两publish各15）与6最终文件清理；清理后33PASS/0Skip、NET8零警告18.03s，成员8源/4注册属性类型16→0纯指纹和字段序列同。137/274与Own未决3保持，下一[GroupBox1/7](group-box-consumers.md)，其他未启动16metadata/26Own，不提交。


2026-11-01 CT15 GroupBox：[结果](group-box-consumers-results.md)、[40静态待限定视觉审查](group-box-consumers-visual-review.md)、[三Reserved待裁决](group-box-token-ruling.md)、[作者自审](group-box-consumers-review.md)。1metadata/7Own V2R1、28默认保持，四Active真实消费；三scalar resolved0/非零但无几何/RGBA消费，仍NeedsDecision。42聚焦PASS（23临时+19既有），实际Global/字体/图标owner与固定外框child-only缺口重绘修复，观察者旧0/当前1退休通过。三版各164帧，候选/AOT各64检查、164cross全0，68主恢复0，Global恢复4/7/9/9 max1保留。40有意静态含28缺口修复/12Global与原生；固定420宽长标题裁剪/暗半标题低对比列明，正文offset180视口外不夸大glyph证据。23临时3源19113字节、545取证、503原会话（driver414/publish15）及6最终文件清理；清理后19PASS/0Skip和NET8零警告18.08s，成员3源/1属性类型0问题。137/274、Own未决3与既定批准保持，完整家族/CT15开放；下一[Empty2/6](empty-consumers.md)，其他未启动15metadata/19Own，不提交。


2026-11-01 CT15 Empty：[结果](empty-consumers-results.md)、[20静态待限定视觉审查](empty-consumers-visual-review.md)、[作者自审](empty-consumers-review.md)。2metadata/6Own全Active V2R1、24默认/type/key/slot保持；37聚焦PASS（24临时+13既有），实际Global/可选字体/三专用Style、离树图片和原生Footer弱焦点回放修复。三版各180帧，候选/AOT各96检查PASS，156静态cross全0、56主恢复及独立A/A/restore各4全0；仅20明确静态已获限定批准。旧32FAIL、19旧新与10跨运行输入持续态非零、原暗色低对比及平台/完整路径/极值/GC/完整CT15保留。24临时3源16381字节、545取证、508原会话（driver414/publish15）及4最终文件清理；清理后13PASS/0Skip（573ms，命令27.72s）、NET8零警告18.85s，成员6源/1含注册属性类型0问题。137/274与Own未决3保持，下一[Statistic4/2](statistic-consumers.md)，其他未启动13metadata/13Own，不提交。


2026-11-01 CT15 Statistic：[结果](statistic-consumers-results.md)、[12静态待限定视觉审查](statistic-consumers-visual-review.md)、[作者自审](statistic-consumers-review.md)。4metadata/2Own V2R1、8默认/type/key/slot保持，四Global身份Own数量0/2/0/0；34+1个不同用例PASS（30临时+5既有），实际Global/字体/六专用Style、原生输入弱焦点与完成倒计时重建分类修复。三版各212帧，候选/AOT各140检查PASS，128静态cross全0、52主恢复及独立A/A/restore各4全0；仅12明确静态待审。旧44FAIL、72旧新/46跨运行持续态非零、原字号0官方异常/正极值裁剪/显式暗低对比及平台/GC/完整CT15保留。30临时3源22642字节、641取证、534原会话（driver414/publish15）及4最终文件清理；清理后5PASS/0Skip（Debug416ms，命令26.96s）与NET8零警告18.08s，成员8源/4含注册属性类型0问题。137/274与Own未决3保持，下一[QRCode2/2](qr-code-consumers.md)，其他未启动9metadata/11Own，不提交。


2026-11-01 CT15 QRCode：[结果](qr-code-consumers-results.md)、[16静态待限定视觉审查](qr-code-consumers-visual-review.md)、[作者自审](qr-code-consumers-review.md)。2metadata/2Own V2R1、8默认/type/key/slot保持；43不同用例PASS（40所选+11补充中3新，35临时+8既有），原Global/字体/专用Cover、空Value清码、Bitmap/刷新退休与原生输入弱focus修复。错误背景480帧与未运行publish排除，修正scope后有效三版各240帧，候选/AOT各140检查PASS，176静态cross全0，四默认0；仅16明确静态待审。三版主恢复52零/8微差（L/C两清除各1/max2，D/DC各12/max1），旧44FAIL/46旧新与41cross持续态非零、原裁剪/方角Cover/低对比/ECI/平台/GC/完整CT15保留。35临时3源24025字节、1221取证、566原会话（driver415/两publish各15）及4最终文件清理；最终8PASS/0Skip（Debug706ms，命令29.01s）、NET8零警告19.04s，成员6源/1注册属性类型0。137/274与Own未决3保持，下一[MarqueeLabel2/2](marquee-label-consumers.md)，未启动7metadata/9Own，不提交。


限定批准登记：2026-11-01，用户明确回复“批准表内20对，其余边界保留”（request_user_input_async / call_cf78d1ab3bc3431289c5246103fe223e / item 0）。仅Drawer表内20对明确静态变化获批准；其余原失败、缺帧、恢复/持续态、平台/GC/极值和完整家族/CT15继续保留，不授权提交。


限定批准登记：2026-11-01，用户明确回复“批准表内44对，其余边界保留”（request_user_input_async / call_e83d092a248045288230fca221d69589 / item 0）。仅Tour表内44对明确静态变化获批准；其余原失败、缺帧、恢复/持续态、平台/GC/极值和完整家族/CT15继续保留，不授权提交。 Primary大min仍沿原生slide/重叠，不批准“极值不重叠”。


2026-11-01 CT15 MarqueeLabel：[结果](marquee-label-consumers-results.md)、[8静态已获限定批准](marquee-label-consumers-visual-review.md)、[作者自审](marquee-label-consumers-review.md)。2metadata/2Own V2R1、8默认/type/key/slot保持；22所选PASS（21临时+既有1），Owner Global/原生Style、真实宽度重建和旧完成退休修复。首批空遮挡43帧排除，有效三版各172帧，managed/AOT各100检查PASS、112静态cross全0、各44主恢复与独立A/A/restore各4全0。仅8明确静态已获限定批准；48旧新/55跨运行持续态非零，原4倍时长、0速度异常、窄宽与字体23裁剪及平台/GC/完整CT15保留。21临时1源12070字节、564取证、500原会话（driver408/publish15）及4最终文件清理；最终10PASS/0Skip（637ms，命令36.86s）、NET8零警告21.98s，成员3源/1属性类型0问题。137/274与GroupBox未决3不变，下一[Spin5/7](spin-consumers.md)，未启动只余5metadata/7Own，不提交。


限定批准登记：2026-11-01，用户明确回复“批准表内8对，其余边界保留”（request_user_input_async / call_492fb90497c742dfbc2801dac1500c71 / item 0）。仅MarqueeLabel表内8对Global/跨scope字体颜色变化获批准；48旧新/55跨运行持续态非零、首批43帧排除、原4倍时长/0速度异常、裁剪、平台/GC/完整CT15继续保留，不授权提交。


2026-11-01 CT15 Spin：[结果](spin-consumers-results.md)、[46主静态已获限定批准](spin-consumers-visual-review.md)、[作者自审](spin-consumers-review.md)。5metadata/7Own V2R1，四算法28默认保持；50所选全PASS（34临时+既有16），Owner字体/Global、仅模板自定义、原生glyph尺寸/Style热更新与原内容弱focus修复。最终三版各172帧、managed/AOT各124检查PASS、136静态cross全0；独立无Controls默认12对旧新/cross零，各56主恢复与A/A/restore各4零。旧124中28FAIL、32旧新/32cross持续态及8独立cal旧新非零、OS/Browser/GC/完整周期与完整CT15保留。86错误驱动帧及480 pre-hot-style帧独立留账。34临时3源18621字节、1114取证、原540会话（driver420/两publish各15）及4最终文件清理；最终16PASS/0Skip（663ms，命令36.54s）、NET8零警告21.80s（命令22.05s），成员8源/2控件0。137/274与GroupBox Own未决3保持；实际未启动家族归零，复用ButtonSpinner4/8的CT13批准，不代表CT15完成。下一回到Timeline P3持续端点/Part恢复与[Count退出保留](badge-count-retirement.md)等真实出口，无提交。


限定批准登记：2026-11-01，用户明确回复“批准表内46对及列明原生优先修正，其余边界保留”（request_user_input_async / call_4176ecaa6e314f4da281e28a8657ee2f / item 0）。仅Spin表内46对主静态与列明glyph尺寸/普通Style优先修正获批准；32持续态cross非零、8独立cal旧新非零、平台/GC/完整周期及完整家族/CT15保留，不授权提交。


2026-11-01 CT15 P3-C：[端点结果](timeline-endpoint-results.md)、[96独立端点已获限定批准](timeline-endpoint-visual-review.md)、[作者自审](timeline-endpoint-review.md)。官方12.1.4显式0首key在整周期产生NaN，原8Timeline+4Native有效FAIL；terminal-only4和RecordingEasing1单变量定位。Core Icon用明确0隐式起点+唯一2π终点修正，70所选+时钟访问1项PASS；不改周期/easing/显隐/内容/布局/依赖。三版各752帧、managed/AOT各648检查PASS，旧504PASS/144FAIL；720受控相位及16校准cross全0，96独立变化+64同帧重复，其余576原新受控/校准零，正常clock各15非零保留。18临时2源14672字节、2817取证、原470会话（driver412/publish15）及4最终文件清理；最终53PASS/0Skip（命令30.80s）、Core NET8零警告3.37s。成员1源/1控件24→0纯指纹/字段序列同。只关闭当前消费者端点缺陷的工程验证，不改写旧FAIL或CT12/13/14、CT25/FW1；Part恢复/平台/完整家族与CT15仍开放，137/274及Own未决3不变。下一核对Spin完整循环与已记录Count退出保留，不重复已批准静态分包。


限定批准登记：2026-11-01，用户明确回复“批准表内96对，其余边界保留”（request_user_input_async / call_d2f7bcbd00ab46e7b6cd3610f38aa4bb / item 0）。仅P3-C表内96个独立端点变化获批准；64张second为相同图像复核，不新增产品状态。各15真实clock非零、Part恢复max1、原失败账本、平台及完整P3/CT15继续保留。官方显式0模型NaN仍可复现，不改CT25/FW1，不授权提交。


2026-11-01 CT15 Spin循环：[结果](spin-cycle-results.md)、[308独立变化已获限定批准](spin-cycle-visual-review.md)、[作者自审](spin-cycle-review.md)。原12旋转/初始opacity/稳态FAIL，次正规模型7PASS/1FAIL，最小正规模型8PASS；实际36+8不同用例通过（28临时+既有16），保持原Alternate/Delay/曲线与Native合同。最终三版各2372帧，managed/AOT各3004检查PASS，旧2624PASS/380FAIL，完整无缺帧；2356受控/就绪/显隐/校准cross零，16真实clock非零保留。old-new592非零=308独立变化+268同帧复核+16真实时相；原1160就绪不足帧独立排除，内容/过渡未删除。28临时1源10258字节、9375取证、原469会话（driver412/publish15）和4最终文件清理；最终16PASS/0Skip（581ms，命令31.29s）、NET8零警告21.04s，成员1-1-0。137/274及Own未决3、此前46静态和P3-C96限定批准保持，CT12/13/14与CT25/FW1不改写，完整家族/CT15开放。下一按[Count退出保留](badge-count-retirement.md)继续，不提交。


限定批准登记：2026-11-01，用户明确回复“批准表内308对，其余边界保留”（request_user_input_async / call_122c5e1eef6b4d7ca6e0c98bd9c0c55b / item 0）。仅Spin循环表内308独立起点/端点/错峰/重新可见变化获批准，51行整帧等价合并及268同帧second复核不扩大产品状态。16真实clock非零、原12FAIL/模型及就绪坏批、其他恢复/平台/GC与完整CT15保留；原46静态/P3-C96批准不扩大，不授权提交。


2026-11-01 CT15 Count退出：[因果/执行](badge-count-retirement.md)、[结果](badge-count-retirement-results.md)、[72静态零变化核对](badge-count-retirement-visual-review.md)、[作者自审](badge-count-retirement-review.md)。原standalone保留adorner GC与新增layer target GC分别由公开composition成员滞后、public association清理时机定位；只在项目成员退出同步public关系及先清association后离树，不写private字段或改依赖。41及target35所选PASS，累计44不同用例（12临时+既有32）；正常三轮show/hide/重挂/模式往返和64×32原按钮左中右点击+Space到达。最终三版各88帧，managed/AOT各108检查含20normal回收全PASS，旧100PASS/8GC FAIL；72静态old-new/cross全0，无新增有意视觉变化，13旧新/16cross真实时相非零保留。12临时2源12902字节、304取证、原492会话（driver411/publish15）及4最终文件清理；最终32PASS/0Skip（759ms，命令30.58s）、NET8零警告19.66s，成员2-2-0。原57中Count GC失败保持历史，当前两已定位路径已修正，不关闭其他Badge/CT15边界；137/274、Own未决3、原48批准、CT12/13/14及CT25/FW1不改写。下一核对Collapse原回收失败与其他真实出口，不提交。


2026-11-01 CT15 Collapse回收复现核对：[结果](collapse-retirement-audit.md)。八不同严格NoInlining条件Debug/Release四批全PASS，原内容/520宽/120viewport900extent/motion及Global/Own/专用Style实际命中和资格退出保持；本轮未复现旧失败、不改产品、不把旧75中2GC FAIL或Debug/Release不一致倒写通过。8临时3源5529字节与14原会话清理，最终精确50PASS/0Skip（命令16.44s），复用相同源码NET8零警告证据。旧GC仍未定位，其他恢复/平台/原12视觉等待与完整CT15继续保留，无全量/Git。


2026-11-01 CT15 P2：[共享归属与几何核对](feedback-stack-p2-ownership-results.md)。正式架构和CT10已有归属将16/8及背板定义为共享产品几何，不推导未经批准的Spacing映射/新Own，不重复研发。无源变化，精确managerPadding/panelGeometry37PASS/0Skip（900ms，命令10.74s），临时会话清理；该终态几何门禁不替代真实host/正常motion/像素/其他业务平台。原Message/Notification非零与完整P2/CT15继续开放，137/274保持，无全量/Git。


## GroupBox三项兼容分类批准登记

2026-11-01 用户明确回复“批准三项 Reserved，其余边界保留”（request_user_input_async / call_c8732844a7a841caacc4385b24e13bd3 / item 0）。仅TextPaddingInline、OrientationMarginPercent、VerticalMarginInline归ReservedDocumented；保持名称/类型/默认/key/slot及原布局，无几何消费者，不标Active/ConsumerVerified。40对视觉仍待限定批准，恢复4/7/9/9 max1及其他家族/完整CT15边界保留，不授权提交。CT15 Own未决由3减为0，原137metadata/274Own及CT12/13/14、CT25/FW1不变。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。


## Empty限定批准登记

2026-11-01 用户明确回复“仅批准 Empty 表内20对，其余保留”（request_user_input_async / call_630c3c2e47fe460594f8888610f2e347 / item 0）。仅Empty表内20对Global/跨scope字体颜色获限定批准；原32失败、19旧新/10跨运行输入持续态非零、原裁剪/低对比、平台/完整家族与CT15保留，不授权提交。此前没有引用明确回复的“已批准”摘要不作为批准来源，本条为明确登记。


2026-11-01 Result严格退出补证：六不同临时用例Debug/Release均PASS，包含provider/window持续保留、当前Presenter持父链正对照及释放、保留已替换caller glyph而owner退出；实际scope93/23、Native27×31和120viewport900extent保持。首次Debug2回收观测FAIL由断言暂存Presenter定位，NoInlining观测后4PASS；不改产品、不把原INCONCLUSIVE倒写为修复。单临时源6584字节已删除，清理后精确Result9PASS/0Skip、745ms；产品未变，复用相同最终源码NET8零警告。原384/18、其他恢复/平台/极值和完整CT15保留，见result-retirement-audit.md。


2026-11-01 P2原生宿主补证：native NSWindow960×720/scale2，managed/AOT完整各147及固定激活各148检查，六位置/变高gap16、原NoWrap全文/显式换行、Cocoa click/Space、五卡片hover/gap/leave、1500ms真实悬停剩余计时、四算法RTL通过。应用内合成Cocoa经Avalonia.Native，不冒充外部物理/其他OS输入。完整OS阴影画布随激活变化，跨运行像素不通过；正常入场初Frame与稳定绘制3497/max2差异定位，初始frame重绘触发、Window重绘无效，纯Border/淡入不复现，不归FW1或扩大容差。248取证/447原会话(411driver)/两AOT已清理，audit clean；清理后精确feedback padding/panel/motion与GroupBox70PASS/0Skip、1s，NET8零警告零错误21.10s。见feedback-native-p2.md；无反馈运行代码改变，GroupBox仅三Reserved XML说明同步。原失败、完整P2/CT15及CT25/FW1保留，不提交。


2026-11-01 Timeline P3 Part恢复定位：四算法/八专用Style32独立A-B-A，32AA0、实际命中/实例/几何恢复；8 icon/title恢复0，其他24严格非零。明暗item/rail采集值相同，背景根重绘均0。无AtomUI独立Avalonia12.1.4工程managed/AOT各20检查17PASS/3恢复FAIL，rail各14/max30、Opacity19/max14，15帧cross全0；公开开关试验未消除，不改产品/依赖/容差。23临时case/3源28395字节、446取证及339会话848665821字节已清理；最终精确Timeline52PASS/0Skip/1s，产品未变复用NET8零警告21.10s。FW2限定归属提案待用户批准，原P3-B102/82、FW1/CT25及CT12/13/14和完整CT15保留，见timeline-part-restore-audit.md、feedback-partial-redraw-visual-review.md，不提交。


## FW2限定归属批准登记

2026-11-01 用户明确回复“仅批准表列路径归 CT25/FW2，其余保留”（request_user_input_async / call_58d9ca164cbe4d43b4254cc21caa6b70 / item 0）。仅feedback-partial-redraw-visual-review.md列出的3个无AtomUI独立反例及24个当前Timeline局部恢复路径归CT25/FW2继续调查；非零仍FAIL，不设置或扩大任何像素容差，不修改既有FW1。原P3-B102/102/82/82、其他家族/native/平台/GC及完整CT15均保留，不授权提交。


2026-11-01 Message恢复定位：四配方×entry/shift/Own/Global的16独立场景，实际值/几何/四card身份恢复、AA0；managed/AOT各224检查220PASS/4严格零恢复FAIL，76帧66cross零/10非零，16完整背景重绘cross全0且每配方四条历史的重绘图彼此0。原正常初始/局部恢复仍非零，不把重绘当修复；新Message路径尚不在已批准FW2范围，原P1与native3497/max2均保留。4临时case/1源9518字节、331取证、427会话1068266516字节已清理，audit clean；最终精确Message Semantic/managerPadding17PASS/0Skip/1s，产品未变复用NET8零警告21.10s。见message-restore-audit.md，完整Message/CT15未关闭，不提交。


2026-11-01 Notification恢复定位：四算法/六卡/1280×900/384宽MaxHeight380及4/10公开输入保持，colour/global八A-B-A。所有数值/gradient stops/几何/原引用/命中通过；managed/AOT各128检查124PASS/4严格恢复FAIL，40原帧cross全0，各配方两完整背景重绘参考彼此0。原初始/局部恢复仍非零，旧两暗图4/max16不倒写；新两像素位置为卡片外缘/阴影，非进度输入丢失。不在原FW2批准表，未改归属。4临时/1源10908字节、174取证、427会话1107631045字节已清理，audit clean；清理后精确29PASS/0Skip/980ms，产品未变复用NET8零警告21.10s；见notification-restore-audit.md，完整家族/CT15保留，无提交。


2026-11-01 Modal原生输入/Tab修复：原生Overlay Tab进入owner input/opener，匹配原AtomUI Window宿主有效RED4项1PASS/3FAIL；仅在既有IsModal=True AXAML Style声明TabNavigation=Cycle，modeless默认Continue与热切换/嵌套LTR-RTL保持。7临时+276原模块283PASS；最终同源managed/AOT Dialog/MessageBox×Overlay/NSWindow各261PASS/exit0，Cocoa点击/选择/输入/Tab、async veto/Escape/重开Enter/恢复焦点、520×420Surface/140viewport/24行/23Menlo保持。旧原生偶发输入/焦点失败和shutdown观察者坏批留账，不声称全修。7临时/1源8644字节、38取证、425会话1125334263字节清理audit clean；清理后精确276PASS/0Skip17s、NET8零警告零错误19.98s。见modal-native-input.md；只有生产主题3行导航增量，原CT11 owner绑定保留，完整Modal/CT15和既定FW1/FW2边界不变，不提交。


2026-11-01 ToolTip/PopupConfirm原生出口：默认400ms hover、两种native popup/Overlay真实Root，Tooltip120/900与Confirm140/900原长文/箭头/两按钮保持；应用内Cocoa hover/leave、实际取消确认各一次、Escape light dismiss、已打开Global23→25均验证。最终同源managed/AOT各75PASS/exit0；首hover未打开原因未证实保留，初诊断反射IL2026产物未执行，移除后零IL/CS warning重发，无suppression。无产品源改动；17取证及439会话1220947773字节已清理audit clean，最终精确67PASS/0Skip/1s，复用当前相同源码NET8零警告19.98s。旧四颜色/Compact恢复、FW1/FW2原限与完整CT15保留，见tooltip-popup-confirm-native-input.md，不提交。


2026-11-01 Marquee正常周期补证：原220×28/Clip/全文及4×时长保持，default150/native450两组2临时PASS；AOT2874检查PASS/exit0，各两次实际回绕与offset有限/范围、隐藏600ms冻结/显示恢复通过。default实测周期13053.818ms，AOT13047.828ms；native450实测4374.079ms/AOT4373.230ms，期望13053.333/4351.111ms，误差界来自实际采样间隔，非像素容差。不冻结clock或作跨时相RGBA0声明。2临时/1源4618字节、16取证、303会话448595997字节已清理audit clean；最终精确原Marquee显隐模块1PASS/0Skip25ms（本轮不是旧10项），产品未变复用NET8零警告19.98s。原48/55持续态非零、4倍语义/0速度异常、平台/GC与完整CT15保留，见marquee-cycle-audit.md，不提交。


2026-11-01 Carousel正常周期补证：三临时用例PASS（45.1134s），default3000/native700真实页序均0,1,2,0,1,2,0；同源AOT38检查PASS/exit0。隐藏暂停/恢复/公开停止、有限末页与空集均验证，原caller-owned内容/120viewport900extent和normal motion保持。3临时/1源5375字节、5取证和310会话757588934字节已清理audit clean，清理后Carousel3项PASS（与Float46共49项/6.9191s）。无Carousel产品源修改/新视觉批准，原16旧新/4cross持续态、平台/极值与完整CT15保留，见carousel-cycle-audit.md。


2026-11-01 BackTop Target退出：两条NoInlining有效GC RED，唯一ConfigureTarget订阅条件限定attach后，三临时用例PASS/10.8843s，同源AOT21检查PASS/exit0；原120viewport900extent/450ms/400阈值/全文/滚动条、换目标/旧目标退出/reattach/Space回顶和之后可滚动保持。AOT46245192字节SHA256 bcacc57aff05811cd365287dc12aae28b662d9a175c31e7b281a9164c68c325a，零IL/CS、五既有Homebrew linker warning。3临时/1源5170字节及309会话757297643字节已清理，无新增媒体；清理后Float46+Carousel3共49PASS/0Skip/6.9191s；NET8最终源码构建中。仅修正这两条资源获取路径，不关闭原恢复/其他GC/平台/极值与完整CT15。见float-button-target-retirement.md，不提交。


清理后最终门禁：Float46与Carousel3精确共49PASS/0Skip/6.9191s；Desktop NET8 Release最终源码零警告零错误28.37s。全程共享run.lock非阻塞独占，无全量。成员1源/2控件0问题，确切原未提交快照到当前仅一个条件替换；注册/初始化/API/Token/布局均无增量。作者另步自审，非独立审查；订阅获取/退出与再次挂载配对，未改原动效模型或已批准家族。两条具体泄漏已修正，其他保留项不倒写。最终门禁helper/marker随读取清理，不自动提交。


2026-11-01 Tour共享mask退出：原两条严格GC FAIL，公开Dispose三绑定单变量使两条均释放，ClearValue无效留账。当前owner持CompositeDisposable并在close/detach/transfer释放，5临时PASS/20.5281s、同源AOT20检查PASS/exit0；重开/Step当前颜色/目标移动30/第二owner接管及旧owner不干扰均通过。5临时1源6866字节与311会话757466582字节已清理，无新增媒体；清理后实际Tour21+placement8共29PASS/0Skip，NET8零警告零错误17.61s、成员1/1/0。见tour-retirement-audit.md。只关闭当前共享mask持有链，不倒写旧缺72帧/18旧新/14cross非零或其他边界；原44限定批准/完整CT15保持，不提交。


清理后最终Drawer37PASS/0Skip/4.5875s，Desktop NET8 Release0警告0错误16.76s。成员1源/1控件0问题、git diff --check通过；相对此前未提交源码仅获取条件增量，无注册/初始化/API/布局改变。作者另步自审，非独立审查。最终helper/marker读取后清理，当前两具体泄漏退出通过；其他原恢复/定时/平台/极值与完整CT15继续。


最终结果：六不同场景Debug均PASS（原四12.4786s/新增二8.5300s），Release均PASS（四13.0868s/二7.4010s）；没有产品反例或源码修改，不把重复运行计为更多用例。6临时case/1源5368字节与7会话文件25204字节已删除，无媒体。清理后实际重建精确Statistic5+QRCode8+Empty13+Spin16+Marquee1共43PASS/0Skip/3.8264s；最终产品源码未变，复用Drawer最终NET8零警告零错误16.76s。git diff --check通过。作者另步自审确认provider/window保持存活与NoInlining观测，没有强持有被测对象；只补本六条对象退出，不关闭其他恢复/持续态/平台/极值或完整CT15。


四不同场景Debug4PASS/0Skip/17.7252s，Release4PASS/0Skip/17.5788s；实际cross root均ImagePreviewerDialog（当前Headless Window，不是NSWindow/Browser）。公开IsOpen/IsCurrentLoaded/cross root打开判据均通过；关闭移除或打开直接移除后root退出且owner可回收。调用方32×16 borrowed WriteableBitmap和ObservableCollection仍保持活着，原标题/Source不变，退出后调用方继续Add成功；不释放调用方图片/不清集合制造回收。

无产品失败/源码修改。4临时case/1源3877字节已删除，无媒体；清理后真实重建精确ImagePreviewer117PASS/0Skip/15.2674s。当前产品源码未变，复用同源NET8零警告零错误16.76s，git diff --check通过。最终会话/marker读取后清理，作者另步自审非独立审查。该四条本体GC不关闭旧171旧新/29静态及64持续态cross非零、平台/极值或完整CT15。


清理结果：当前10临时case/2源14466字节已删除；官方evidence cleanup clean/findings=[]、removed=true/550文件。外部111会话文件569551767字节（含独立工程/原失败与坏模型日志）及marker删除。清理后实际重建MessageSemanticPartTests13PASS/0Skip/3.7454s；首namespace误写Feedback而Padding实际为FeedbackStack，未把未匹配项计入，随后同一重建程序集精确Padding4PASS/0Skip/3.1545s，合计17不同既有用例。未改产品源码，复用相同最终源码NET8零警告零错误16.76s。最终helper/marker随读取清理，无新常驻测试、无全量或提交。

作者另步自审：纯模型默认图0只用于确认图元等价，独立裁剪错误已纠正并保留反证；不将未复现产品历史的模型解释为框架修复。原真实恢复仍FAIL，当前无新视觉批准或FW2归属请求，完整Message/CT15保持开放。


通知顺序补验已闭环：1有效RED原[1,0]→修复后1临时+21Tour共22PASS/0Skip/7.6984s；2390字节单临时源已删除。清理后真实重建Tour/placement29PASS/0Skip/5.7930s，最终源码Desktop NET8 Release0警告0错误17.41s，成员1/1/0与diff check通过。原AOT20明确保留为此前mask绑定增量证据，本次仅既有调用顺序/重复通知修正，没有新增AOT敏感路径，不冒称本次重新发布。最终会话/marker随读取清理。作者自审补验发现并纠正本轮回归，不影响其他既定批准/原失败、未关闭完整CT15。


最终清理后精确Timeline52PASS/0Skip/4.5166s，实际重建测试程序集；本轮无产品源码改动，复用相同最终源码NET8零警告零错误17.41s。1130取证经官方audit/cleanup clean删除，外部454会话文件1374532913字节、全部恢复的历史命令/驱动/发布产物与marker均删除；最后helper读取后清理。没有执行历史命令中的Git或旧源码替换，没有新增永久测试/全量/分支/worktree/提交。

作者另步自审：原主批64/440保持完整；组合/Content/排除Content与重绘分别分账，透明懒滚动条节点例外明确披露；208跨运行/96只读图全0不替代恢复失败。8路径限定归属表已完成工程、清理、精确门禁，等待用户是否交FW2调查的明确裁决，未自动修改原归属、FW1或完整P3/CT15结论。


## 原 P3-B 八路径限定归属批准登记

用户明确回复“仅批准表列8路径归CT25/FW2，其余保留”（request_user_input_async / call_cd2233cdfe1848fc889d777f91b52754 / item 0）。仅timeline-original-part-visual-review.md列出的4条原P3-B组合恢复102/102/82/82及4条原值Content0.7恢复124/102/98/82交CT25/FW2继续调查；非零仍FAIL，不增像素容差、不改既有FW1，不把透明滚动条懒节点差别抹去。其他原故障账本、家族/原生平台/完整P3与CT15保留，不授权提交。此前3独立模型/24路径批准范围不自动扩成其他家族。


最终清理后精确MessageSemanticPart13+FeedbackManagerPadding4共17PASS/0Skip/4.5267s，实际重建；本轮无产品源码变化，复用相同最终源码NET8零警告零错误17.41s。1191取证与795会话2176410897字节已清理audit clean，最后helper/marker记录后删除；没有新增永久测试、全量或Git操作。作者另步自审，非独立审查；原动效/内容/滚动/选择/关闭合同未改，独立模型与真实主批限制明确分账。仅34条有限调查路径提案待用户裁决，完整CT15及其他排除项保持开放。


## Message 原历史34路径限定调查批准登记

用户明确回复“仅批准表列34路径归CT25/FW2调查，其余保留”（request_user_input_async / call_8107eb29ed024cfbb7dd7fd40dec2aed / item 0）。仅message-original-visual-review.md表列28当前业务恢复、4独立检错路径及2无AtomUI队列重排反例交CT25/FW2继续调查。模型未精确复现原全部像素的限制保持，不宣称完整框架根因；非零仍FAIL、不增容差、不关产品动效、不改FW1。原native3497、其他36跨运行非零、旧失败批、其他家族与完整CT15均保留，不授权提交。


清理后精确Card20PASS/0Skip/4.7581s，实际重建测试程序集；本轮无生产源码变化，复用相同最终源码NET8零警告零错误17.41s。官方cleanup910取证、外部790会话2173296792字节与全部恢复/诊断/发布源码已删除，marker删除；最终helper记录后清理。作者另步自审：原88图完整，单变量clear范围限制已纠正、Frame重录因果与独立差分精确对应，严格4FAIL未改为PASS；无永久测试/全量/分支/worktree/提交。

仅card-original-visual-review.md表列40路径提出FW2继续调查，尚待用户明确裁决。原312/110非零仍FAIL、旧新Global完整对比与其他原边界/完整CT15不变，不将当前调查当整个家族关闭。


最终清理后NotificationCardTheme/NotificationSemanticPart精确29PASS/0Skip/4.4735s，实际重建；本轮无生产源码变化，复用相同最终源码NET8零警告零错误17.41s。313取证经官方audit/cleanup clean后删除，外部438会话1374918333字节、全部历史恢复/原驱动/发布文件与marker删除；最终helper记录后清理。作者另步自审确认旧新两对与候选自身恢复/后续新ABA4FAIL不混报，没有以新绿倒写原账本。仅两对旧残留消失的限定接受待用户裁决，完整CT15继续。


## Card 40路径限定调查批准登记

用户明确回复“仅批准表列40路径归CT25/FW2调查，其余保留”（request_user_input_async / call_c8333768d41d4ba692fea52d54ee7dea / item 0）。仅card-original-visual-review.md表列原32恢复、4同值Frame重录及4独立图元路径交CT25/FW2继续调查；非零仍FAIL，不增容差、不打产品重绘补丁、不改FW1/官方依赖。原旧新Global非零、其他边界与完整CT15保留，不授权提交。


## Notification 原两对恢复限定接受登记

用户明确回复“仅接受表列两对恢复差异，其余保留”（request_user_input_async / call_fdb5ce1da6f04f16b707b0e5d6b02d29 / item 0）。仅notification-original-visual-review.md列出的原Dark/Dark+Compact旧新global-restore各4/max16，作为旧版清除残留消失、候选回到共同默认的两对差异获限定接受。不宣称渲染根因已定，不把历史FAIL改写成当时PASS；后续新ABA4FAIL、平台/其他边界与完整CT15保留，不授权提交。


当前续作清理后Notification精确29PASS/0Skip/4.3151s，实际重建测试程序集；无生产源码变化，复用相同最终源码NET8零警告零错误17.41s。4临时理论/单源11785字节、460取证、779外部会话2174450601字节和marker已删除，audit clean；最后helper记录后清理。作者另步自审确认原正常4FAIL、no-motion3FAIL、独立模型7FAIL均保留，不把独立复现或跨运行0当像素PASS。18路径有限调查提案待用户裁决，未自动纳入FW2；原两个旧新差异的接受、其他家族/平台/完整CT15保持各自边界。


## Notification 新 ABA 18路径有限调查批准登记

用户明确回复“仅批准表列18路径归CT25/FW2调查，其余保留”（request_user_input_async / call_3c0dd46bb06948bfa8d56cec286a3aff / item 0）。仅notification-aba-visual-review.md所列8真实ABA、3真实初始静态重绘、4纯入场、3纯静态交CT25/FW2继续调查；非零仍FAIL，不增容差、不关产品动效、不加产品重绘补丁。两暗色独立入场各差1像素的限制保持，不认定原全部框架根因；其他边界、FW1/官方依赖与完整CT15均保留，不授权提交。
