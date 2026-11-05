# CT17 绘制家族有限出口

**最终状态（2026-11-04）：CT17 在本轮明确的50 metadata/92 Own及既定批准范围内完成。** V1–V5、C1及S1均已取得下文集中登记的明确批准；原始S1严格FAIL/11像素计数保留，不设通用容差、不扩大未来采样或CT25/FW边界。下文阶段性“待完成/待审查”记录属于历史，当前结论以“批准登记与最终结论”为准。未提交。

2026-11-03 接续，权威执行与结果记录。既有 [drawing-consumers](drawing-consumers.md)、P0、P1-A、P1-B、P1-B3 及 StepsProgressBar 边界的批准和有效证据直接复用。本次只在现有目录和 `feature/custom-theme` 工作，无分支/worktree、子 agent 或提交。

## 有限清单

- [x] Progress 四形态：剩余 Own、Global/字体、信息测量/动态间距/状态与范围、专用 Style、退出与回收。
- [x] Skeleton 组合及各端点：15 Own 的依赖/消费、梯度/连续周期、尺寸与实际内部目标。
- [x] Rate、Slider、ToggleSwitch：7/25/21 Own、Global/字体、实际输入/布局/动态缓存、适用 Custom、生命周期。
- [x] Separator、Watermark、BorderBeam：7/0/4 Own、文字/线宽、glyph 重建/释放、独立 owner 和动画边界。
- [x] 同一参数化驱动：明暗旧新、独立 A/A、1 DIP 检错、完整恢复；当前 Mac managed/NativeAOT 的实际窗口与布局分账，最终显示像素按实际采样后端记录。
- [x] 临时源/驱动/媒体/专用产物清理后精确模块检查、NET8、成员审计、正式文档/CSV/任务表及作者另步自审。

范围为原 CSV 中 50 metadata / 92 Own。独立 Steps 的 9 metadata / 82 Own 不属于本次用户明确列出的绘制家族；StepsProgressBar 属于四种 Progress，已批准边界修复继续有效。不能因此关闭未在本轮范围内的 Steps 行或扩大到 CT20。七个待核定字段为 Progress 三旧槽、Skeleton TitleHeight/GradientToColor 的现有分类、Separator HorizontalWithTextGutterMargin、Slider MarginPartWithMark；实际消费和处置以源码及失败验证为准。

排除 CT12–CT16 重开、完整 CT20、其他 CT 任务与新增平台范围。CT25/FW1/FW2 原始失败、异常、缺帧、未配对及既定归属保留；无通用像素容差。compat/Avalonia.Skia 沿已批准构建，官方缓存和 native 不修改。

## 执行约定

共享产物使用 `.artifacts/verification/run.lock`，构建与采样由持锁父进程串行组织；开始时非阻塞获取成功。原暂存区二进制补丁、277 个既有改动文件指纹及 117 个目标源备份保存在仓库外会话目录；最终核对，不覆盖用户内容。实际备份数量以运行输出为准。

先恢复原场景，针对实际缺口取得有效 RED 后最小修复；同源最终构建与采样合批复用。每轮结果只在本文更新，CSV/任务表只记状态和链接。新增测试默认临时，最终净增常驻为 0。使用既有设计及批准，未重新申请已批准项；作者自审按用户要求自行完成，不委派。

## 结果

当前 106 项临时检查 PASS / 0 FAIL / 0 Skip（2s）：77 个非兼容字段实际 owner/绘制输入配置与清除，P0 后续 16 项反例，可选字体/本地优先级、Separator 排版、Avatar Size 和 Switch loading 开关。92 字段×明暗的 184 行默认值/类型/slot/resource ID 比较相同。临时夹具的编译错误和冻结渐变强转异常不计有效产品 RED。

首批有效 RED 为16/16：TitleHeight 实际11而应23，Own派生失效，四公开 Progress 的 ColorError 未达 StrokeBrush，普通/Steps margin更新后几何不变，Steps标签溢出，调用方Min被改写。随后新增8个实际 owner 的 FontFamily反例、Separator本地字号31被16覆盖与动态padding不重排、Avatar显式Size64仍32、Rate SM/LG仍Middle20、Switch关闭motion仍有Rotation动画。当前候选对应验证已通过。

根因及最小接线：Skeleton/Rate/Slider/Switch/Separator/BorderBeam 六个既有终端的79字段迁DependencyDefaultsV2；Progress13既有V2不重迁。渐变/圆角、Slider留位和ToggleSwitch派生值只声明真实Own依赖。Rate黄色复用现有PresetPalettes按appearance读取缓存，未造palette算法。Global投射改由实际owner，字体复用既有真实InheritanceParent桥，不添加新的查询机制。模板可表达的Avatar Size及Rate SizeType改由AXAML；Title保留SkeletonLine StyleKey，公共主题通过有限类型分流消费TitleHeight。动态Progress留位与Separator间距失效在其实际布局owner更新；计算Min用Template fallback保留用户值。Switch加载保留业务IsLoading，只按已有motion开关/周期更新其动画执行。

统一参数化视觉/原生宿主正在构建；尚未声明本家族完成。


### 2026-11-04 收敛补充

- 4个Custom有效RED（Skeleton三个端点未覆盖时高0、Switch应用字号31被12压住）已修正。Skeleton仅基础Height32回退，不用基础MinHeight压住调用方20；Switch Custom退出预设，几何保留中档基础回退。
- 8个实际字体owner同实例A→B重挂与长寿命provider下回收通过。原同步GC的7个保留未计产品泄漏：固定经过800ms transition及真实帧后，同组8项通过，未以重建替代原实例。临时Case连同4 Custom共12PASS/9s。
- Progress固定25高、owner Global FontSize40/SM12的两个有效RED：实际字号仍40。新AXAML投射PresetFontSize/Small与缓存失效后，2专项+4Custom+原Progress42共48PASS/1s，调用方固定高度未删减。
- 原完整drawing历史中Watermark首次注入会重挂父内容，Slider随后PointerPressed已送达且命中/360宽保持，但Value停50。原Watermark与直接重挂两项均有效RED；OnAttached恢复输入/轨道订阅后，两项及原Slider模块共29PASS/1s。拖动中detach的邻接检查现有路径已通过，不增加猜测性手势取消代码。
- 线形/步骤按overview既有结果合同核对：16项中10FAIL/6PASS，失败是异常完成变成功色、结果文字/图标重叠或缺失及水平内嵌文字被隐藏。限定候选仅调整AbstractLineProgressTheme与普通line内嵌规则；Circle/Dashboard既有批准不重开。
- 3个主类纯成员整理的191个成员代码逐项完全相同（88/62/41）；实际触及10文件的结构检查除Rate四个接口region位置已纠正外无新增问题，最终完整审计仍待源码稳定。未触及的Dashboard常量/StepsPanel旧结构不因宽扫描被纳入修改。
- 宿主的Target名称初始化、类型别名及DirectProperty的IsAnimating判据是夹具失败，不计产品RED。DirectProperty时钟改为读取真实Progress推进；原帧及输入失败保留。资格样本不作为正式旧新/A/A出口。

计数校正：按CSV实际逐行筛选，本轮为50 metadata；独立Steps为9 metadata/82 Own。恢复阶段曾将其误计为10并报告49，现已纠正，范围和92 Own均未扩大。


### 首次完整严格对照（保留失败）

旧/新各310帧，实际完整命令和临时驱动在会话外目录。两版124AA全部0、124DIP全部检出；123/124DIP恢复0，旧暗色disabled一组2像素/max2非零，位置x153/y335–341；对应probe-main也2/max2。旧/新Progress全部默认/最终恢复及状态恢复为0；五兼容槽位24组配置与默认为0。

绘制组默认→最终恢复旧明/暗3472/max29、3433/max16，均在水印文字x144–499/y454–531；新为4276/max235、4252/max196，除同一水印外含Rate文字x3起。各main/AA/DIP重复计数相同，均仍FAIL，未放宽像素容差、覆盖基线或转CT25。旧版六次Slider重挂输入及六次loading退出均保留原FAIL，候选对应十二项通过。工程计数旧2068/新2080包含共同判据与候选修复判据，不把原FAIL填成产品PASS。

实际查看默认/恢复发现Rate默认glyph空白、往返后出现。真实Watermark安装历史的反例确认item.StarColor已准备而缓存glyph.Foreground为null；修正明确断言后1项有效RED，OnAttached按实际已准备输入重建离树glyph及clip后，原反例+Rate原模块+8个回收共12PASS/10s。只增生命周期入口，无强制重绘或延时补丁；原Progress有效图片不受影响，将复用，不整批重跑。

Watermark仍沿同一完整历史读取实际公开glyph/目标模型作单变量定位；尚未确定根因。原失败保留，尚未申请视觉例外或扩大CT25/FW边界。全部139临时检查的结果在Rate补充前成立；更新后的最终临时与清理后出口另记。


### 两项严格恢复根因

同一完整drawing历史只增公开输入只读记录，未改变原采样动作：Watermark初始text/font/ink/rotate等与恢复均相同，实际GetDesiredSize却从96.9140625×16变成96.9140625×21。字号已是16，FormattedText仅修改run字号后保留了原段落行高下限，水印tile步长随错误高度变化。原轨迹临时反例有效失败（期望96.9140625×16、实际×21）；TextGlyph仅在字号变化时重建FormattedText，Foreground继续原位更新，原反例及所有临时项合计141PASS/12s。没有改框架、native、比较器或强制重绘。

Rate先前null前景的明确断言有效失败，attach重建真实输入后通过；补充1文件与Watermark共2处修复，最终产品文件由36变38。原310×2失败帧继续保留，Progress170个候选帧不受这两文件影响而复用；仅以同一修正参数化驱动补140个drawing候选帧，再做NativeAOT和当前Mac实际窗口。14个资格图及ImageGlyph比例的56.00000000000001几何浮点资格失败不作为像素结论；几何判据改为1e-9，RGBA像素比较继续严格0。


### 当前有限余项（2026-11-04）

- 两修复后的drawing补140帧、1172项：明暗6组default→reset、28组DIP恢复、28组1DIP检错均符合；原失败图不覆盖。暗色disabled的一组A/A为11像素/max7，bbox(157,341)–(317,348)，probe-main相同11；仍FAIL。
- 同一完整暗色轨迹只加只读mark模型与直接Skia诊断，三个独立实例的track bounds/矩阵、Menlo12、#3fffffff及mark尺寸一致；直接3组A/A0，当前compositor3组A/A0，诊断前后compositor均0。原new2-main相比当前仍恰11像素，其他两实例0；该次未复现不覆盖原FAIL。因FormattedText度量getter会惰性求值，最终驱动已撤去该观察，仅保留现有LabelSize，继续原动作。
- 新旧默认drawing的差异已逐坐标分解：明812=Rate804+Separator8，暗827=Rate819+Separator8。8个像素是标题字号实际16后的em间隙从旧root12改为16，两侧rail各缩4 DIP；图像占位默认颜色/窗口/其余像素保持。此为尚待限定视觉批准的具体默认变化，不自动豁免。
- NativeAOT首发在链接期因临时宿主未引用Gallery既有Homebrew搜索target失败（ld找不到ssl），无执行结果；工程未安装/更新native。临时宿主现直接复用build/MacOSHomebrewNativeAot.targets，复用已安装库与前次ILC产物重链，后续真实结果另记。
- 当前待完成：当前Mac managed/AOT及其310帧、11像素有限诊断出口/限定审查、五兼容槽位审查、全媒体/临时源清理、最终精确模块/NET8及作者另步自审。原Progress有效对照复用；不重开其他CT、不全量、不提交。


## 当前 Mac 运行与像素分账

最终候选38产品文件，TextGlyph另做13成员纯布局整理（代码逐项相同）；20个实际触及C#文件/12个控件，最后8项TextGlyph结构问题已清零。compat/Avalonia.Skia的58个manifest SHA全部匹配。

NativeAOT真实Mach-O arm64 SHA256：`c54539b5241476f20c70918aded8d857b8fff57e79762cfa2331ed24825f492a`。首发缺Homebrew链接搜索路径失败不计通过；临时宿主直接导入Gallery既有target后重链38.59s，0 IL/CS警告，保留同5条macOS部署版本链接警告，未修改native库/官方NuGet缓存。静态源码指纹在发布/运行前后相同。

| 后端/范围 | 实际结果与边界 |
| --- | --- |
| Headless/真实Skia JIT | Progress复用170帧，drawing最终补140帧；141临时功能/输入/缓存/GC检查通过。原失败批独立保留。 |
| Headless/真实Skia NativeAOT | 310帧齐全，2080检查/261.38s；62AA、62DIP、62DIP恢复、62probe-main及全部12默认恢复/12兼容槽/6状态恢复全符合，静态RGBA无阈值。 |
| 当前源AOT/JIT | 310对中309对RGBA0；仅JIT `new2-drawing-dark-main-disabled`为已登记11/max7，其他模式/主题/阶段0，原失败不改写。 |
| 实际NSWindow managed | scale2、client720×720，明暗progress/drawing四窗口、648检查/77.23s，实际布局/配置/Style/Custom/时钟和退出通过。 |
| 实际NSWindow NativeAOT | 同四窗口/scale2/client、648检查/78.25s，通过；实际进程为上列Mach-O。 |

原生专项是实际窗口与布局/公开API/时钟证据；鼠标输入证据来自Headless真实框架路径（含原Watermark重挂及left/center/right）。未把公开API设值说成物理输入，也未把Headless截图说成OS最终显示面像素。既定其他OS/Host、FW1/FW2及未覆盖平台边界保持，未擅自移交新未决项。

### 限定审查材料（工程和清理门禁完成后请求）

所有下表只引用当前命名帧/输入，不扩为通用像素容差，不包含Git提交。P0/P1既有批准不再次申请。

| 项 | 具体变化/依据 | 当前真实证据 |
| --- | --- | --- |
| V1 线形/步骤结果 | 原Exception/Success在50或100的文字/图标重叠/缺失、异常100误用成功色；改成既有overview结果合同，内嵌保留百分比，disabled不变 | 默认Exception50明210/暗209；Exception100各3260；Success50为614/613；Success100各310像素。正常默认、状态恢复、最终恢复旧新0。Circle既定状态批准不重开。 |
| V2 其余有效配置 | 原同输入Own派生、间距、Global/字体未到真实目标的路径接通；Root约束/专用Style保留 | Progress Own11859/11900、font13376/13418、configuredException3888/3888，完整输入表与实际节点验证集中在本文；不是对所有配置任意改变外观的授权。 |
| V3 Rate/Separator默认 | Rate准备后的前景快照不再为null；标题按实际16字号计算em留白 | 默认明812=Rate804+Separator8，暗827=Rate819+Separator8；8个像素为两侧rail各缩4 DIP。其他默认图像色/几何及画布保持；明暗恢复0。 |
| V4 Skeleton/其余定制与Custom | TitleHeight独立、梯度/圆角/图像/Slider/Switch的Own派生、图像占位可选色、Custom退预设与稳定回退生效 | drawing Own21428/23629，font24379/30169，Custom37980/43511，Custom Style24790/29898；均为同数据/同窗口完整轨迹，并以77字段逐项/清除和真实Style证明。 |
| V5 水印恢复 | 字号16→21→16时实际行高回到16，保留内容、旋转/镜像/比例及原资源owner | 原旧恢复3472/3433非零由21错误行高导致；候选明暗全部恢复0，TextGlyph反例RED→GREEN、ImageGlyph实际adorner/比例/旋转也在相同宿主运行。 |
| C1 五兼容槽位 | Progress三个原零槽、Separator HorizontalWithTextGutterMargin、Slider MarginPartWithMark保留名称/类型/默认/key/slot/编辑入口，不制造几何或动画语义 | 92字段/184默认及编号保持；当前12组兼容配置整帧0，历史旧新24组0。仅申请分类ReservedDocumented，不标Active/ConsumerVerified。 |
| S1 精确采样差异 | 仅当前JIT暗色disabled Slider mark文字命名帧的11像素/max7，bbox x157–317/y341–348；同帧导致一组A/A、probe-main及AOT/JIT非零 | 实际模型/几何一致；补充完整轨迹的直接Skia与compositor均0，原JIT-main独有11，其余两实例及AOT全部0。内部合成原因未完全确定，原FAIL保留；只申请接受这组具体差异，不设阈值、不扩大到未来采样或CT25/FW。 |

已实际查看明暗正常/异常完成、Own/字体、专用Style、Custom、水印文字/图像和默认/恢复关键图；窗口和内容没有删除或放大以消差。S1仍待明确裁决，不能写成完整像素通过。审核依据为主题验收规范的“任何未解释或未经批准的视觉差异都阻断该Control及受影响任务的验收”。


## 清理后的最终工程门禁与作者自审

13份临时源码44,602字节已删除，净新增常驻测试0；两份既有Skeleton/Separator默认测试仅调整到V2真实求值入口。外部Host423文件989,795,635字节、AOT16文件135,745,919字节、资格目录140文件1,226,849字节清理；登记取证目录1983文件已删除，Git媒体审计clean。所有本次窗口/进程均已正常退出；没有把截图/媒体/二进制写入项目或docs/superpowers。

持共享锁，在临时源已清理、最终38产品文件指纹相同的工作树执行：

| 命令范围 | 实际结果 |
| --- | --- |
| Desktop精确namespace ProgressBar/Skeleton/Rate/Slider/Switch/Separator/BorderBeam | 152PASS / 0FAIL / 0Skip，测试2s，含构建46.67s |
| Gallery精确6类 ProgressBar/Skeleton/Slider/SliderSemanticTracks/ToggleSwitch/BorderBeam ShowCase | 22PASS / 0FAIL / 0Skip，测试4s，含构建24.49s |
| Desktop Release NET8含依赖，`--no-restore -m:1 -nr:false` | 0警告/0错误，18.94s |
| `git diff --check` | PASS，0.27s |

最终精确174PASS，不含141临时case或历史分包次数；没有运行全量/affected-wide。50 metadata/92 Own、722/1221全表主键唯一；37份正式控件文档相对链接无失效。暂存区patch字节与开始时完全相同；277个原改动文件中，本任务之外的全部内容指纹无变化，未删除原未跟踪文件。

作者另步自审（按用户要求不委派，不冒充独立评审）：逐项看当前任务前后差异及需求，V2字段名称/类型/key/slot/默认184行保持；真实派生边与明确兼容槽位已区分，未增加新Token/身份/Part。模板所有可表达的尺寸/字体/样式投射均在AXAML；新增代码仅提供真实继承父级、运行时度量/失效和生命周期执行。无找Part回写Background等代码回退、补丁标志、新框架/常驻测试或框架依赖修改。四个类的纯成员整理共204成员逐项代码相同（88+62+41+13），常量顺序和有初始化依赖的字段未改语义；最终20 C#文件/12控件结构审计0问题。新protected声明仅覆盖现有attach生命周期，内部分流/回退为主题工具，不新增对外API。

当前工程、清理和当前Mac运行门禁均已完成；V1–V5功能视觉、C1五兼容分类及S1精确11像素差异等待用户限定裁决。S1仍是原始严格FAIL，未计入PASS，未转CT25/FW或扩大容差；完整CT17不在批准前关闭。未创建分支/worktree、未使用子agent、未提交/推送。

最终会话收尾：暂存区字节及全部任务外既有指纹再次相同；38产品源与AOT/清理后检查候选SHA相同。会话剩余辅助脚本/备份/原始日志共286文件、3470798字节，记录聚合指纹后整体删除；不保留临时驱动或源码备份。Source manifest aggregate SHA256=`09e32fc1e29bcabba26598006b2de46905b85ae573b603e395cab757b23e0850`。当前仅等待两个明确的限定裁决，未授予或执行提交。


## 批准登记与最终结论

2026-11-04收到本次两个明确答复。批准原话、范围和请求ID只在本节登记；任务表和CSV引用本节。

| 登记 | questionItemId | 用户原话 | 严格范围 |
| --- | --- | --- | --- |
| A1 | `["request_user_input_async","call_3d74bf75d7f445759651b27f438cc5ff",0]` | “批准 V1–V5 与 C1 限定范围” | 仅上表V1–V5本轮新增功能视觉修复及C1五槽ReservedDocumented；既有P0/P1批准复用，不重开。保留五槽名称/类型/默认/key/slot/编辑入口，不制造新运行语义，不标Active/ConsumerVerified。 |
| A2 | `["request_user_input_async","call_3d74bf75d7f445759651b27f438cc5ff",1]` | “仅接受表列 S1 具体差异” | 仅命名JIT暗色disabled Slider标记文字帧11像素/max7，以及同帧关联的A/A、probe-main和一对AOT/JIT。内部原因未完全确定和原严格FAIL均保留；不设置通用容差、不扩到未来帧、不转CT25/FW。 |

两项批准均不含Git提交。没有把S1改记为零差异或PASS；它以具体已接受差异关闭本轮验收出口。原2像素旧版DIP恢复、其他历史失败/异常/缺帧/未配对和既定CT25/FW1/FW2边界继续按原记录保留，未扩大批准。

完成范围：Skeleton及实际端点、四种Progress、Rate、Slider、Switch、Separator、Watermark、BorderBeam与实际内部绘制目标；50 metadata、92 Own=87消费字段+5批准兼容字段。141临时检查完成并删除，清理后精确174PASS、NET8零警告/错误，当前Mac managed/NativeAOT实际NSWindow各648检查；AOT/JIT309/310零，余一对按A2精确接受。正式文档、CSV、任务表、作者自审和清理均已闭合；复用既有有效证据，本次仅登记批准和同步状态，不重复构建/发布/采样。

CT17本轮既定范围无剩余工程或待批准项。独立Steps的9 metadata/82 Own、完整CT20、其他CT任务及既定平台/最终OS显示面/物理输入边界不因本次关闭而扩张；CT12–CT16既定结论未重开。始终在现有目录和feature/custom-theme，无分支/worktree、子agent或提交/推送。
