# CT-17 P1-B：圆形尺寸与状态结果

状态：进行中。沿现有feature/custom-theme和checkout执行，不创建分支／worktree，不自动提交。P1-A两项颜色／圆角接线已获用户视觉批准；本包字体、图标及状态显示修复也已获用户视觉批准，不代表完整Progress家族完成。

## B1 尺寸与优先级

在原96×96 Circle／Dashboard、360×240真实Skia Headless窗口和明暗scope上，最小字号、最小图标及本地字号跨resize三组共12项全部有效失败。Own值已经进入内部属性，但字号仍20.4、图标仍21.333；调用方FontSize=31在120尺寸下被覆盖成24。源码只在尺寸／EffectiveSizeType变化时更新，并以local值写owner FontSize及模板图标宽高。

代码现仅暴露运行时计算的ExtraInfoFontSize／ExtraInfoIconSize，字体用ControlTheme的CompiledBinding投射，图标宽高用template style的TemplateBinding投射；min Own变化刷新指标、可见性及测量。保持原计算和默认，不用代码覆盖字体或Part尺寸。两类公开owner的本地FontSize／ClearValue、Own更新／清除、compiled专用Indicator Style的34尺寸覆盖、Own改为42后的退出恢复均通过。14项功能检查通过，额外两项视觉驱动后共16项通过。

最初绿色检查有4项在后续完成态断言失败，是验证代码错误地要求Exception在100时显示成功图标；第二次又错误地要求两图标都隐藏。只读状态记录证明原模型在Exception／100仍显示异常图标，Normal／100才显示成功图标；修正验证合同后12项通过，再补专用Style两项。该两次错误预期不计产品RED，也未据此修改产品状态行为。随后按正式overview明确的状态合同另开B2反例。

整文件成员布局另行整理：45个Roslyn成员token指纹逐项相同、5问题降为0，纯布局patch独立保存；之后才加入行为修改。

## B1 合成视觉

720×520、root宽640、真实opaque背景；Circle／Dashboard各Large／Middle／Small自动尺寸加96定制尺寸共8个owner。正常motion，同实例默认→定制scope字号28→图标36＋异常→完成→清Own、FontSize31及112尺寸→恢复96。每版独立main／A/A／1 DIP，捕获前核对实际明暗、Value终值、measure／arrange、非动画及真实compositor commit。

72帧对照，默认及重置旧新明暗均0，同版本默认→重置四组均0；24组A/A全部0，24组1 DIP全部检出（最少9,124像素）。明／暗Own字号差异1,478／1,468，异常图标场景1,782／1,774，完成408／406，本地字体1,766／1,750。差异只涉及下面两个定制尺寸owner的信息内容，上面六个自动尺寸无变化。已实际查看旧／新明色字号、暗色异常、明色本地字体及暗色差异图。

首轮驱动因测试namespace中的SizeType重名而编译失败，已用global枚举别名修正并清理空marker。下一轮旧版暗色异常准备有一次5秒稳定超时，未捕获实际未稳flag，不能擅自归因；加入失败状态诊断后，相同尺寸／motion／门槛的完整旧新重录通过。失败准备的20帧未纳入像素结论，按marker清理。后续若再现则保留逐属性flag调查，不延长任意等待或降低门槛。

此录制同时暴露旧／新共有的百分比与异常图标重叠，B1的尺寸差异通过不表示状态画面合格；B2继续修复，再形成可审查的完整候选。

## B1 真实 Mac 与 AOT

生产UseAtomUIPlatformDetect／WithAtomUIDefaultOptions宿主，不启用测试region选项；720×440、实际scale2。managed与Mach-O arm64 NativeAOT各36项固定检查通过，分别3.295／2.719秒。验证同实例明暗、min字号／图标更新和重置、本地FontSize跨resize／Own优先级及ClearValue、compiled专用Indicator Style覆盖及退出恢复。

managed构建0警告／0错误；NativeAOT发布43.876秒成功，保留5条本机Homebrew dylib最低macOS版本链接警告。此处确认新增内部属性的编译绑定经trim／AOT后实际生效，不代替原生最终显示面的像素验收。专用工程暂供同包B2状态检查复用，整体分包清理时删除。

## B2 状态互斥

实际截图显示百分比与异常图标重叠。正式overview明确Success用成功图标、Exception用异常图标，Normal／Active在完成后使用成功色；现有模板缺少异常／成功时隐藏百分比的规则，Success／completed还隐藏成功图标，Exception／completed又被完成规则改成成功色。先用同一实例的状态往返、真实完成transition、disabled及信息区开关验证，再局部修复Circle／Dashboard，线形／步骤的状态合同另验。

B2新增4项状态反例全部有效失败；后续字体／状态／位置及完整对照结果见下文。临时产物清理和最终聚焦出口继续，不提前标记整个家族完成。

## B2 根因与修复

按正式overview的状态合同补4项有效RED，确认异常／成功时百分比未隐藏。局部ControlTheme现让Circle／Dashboard的结果信息互斥，保留显式Success完成态成功图标，Exception完成态保持错误色，并保持disabled优先。未修改线形／步骤的状态规则。新增状态4项及前序14项合计18通过。

稳定驱动补查曾得到7项超时。逐属性记录证明布局及Value已经稳定，唯独StrokeBrush仍在中间色且IsAnimating=true。只把临时Thread.Sleep／RunJobs循环换成有界DispatcherFrame＋DispatcherTimer后，同样4项图标轨迹由1通过／3超时转4通过；保留正常motion和5秒门槛，产品动效源码没有改动。后续完整状态与视觉均用真实dispatcher frame，不能再把该超时归为布局或颜色合同失败。

随后4项固定96尺寸的居中反例得到center X=59而应为48。原因是Canvas的固定本地尺寸截断了子内容DesiredSize变化，owner保留上次零尺寸图标的位置；仅在owner Measure中InvalidateArrange的单变量没有改善，已移除。现有CircleProgressPanel统一安排信息区域，新增内部CircleProgressIndicatorPanel负责内容测量和居中；默认模板改用该实际信息面板，owner继续保留旧Canvas自定义模板路径。模板几何和语义indicator仍是实际Panel，无透明代理、事件订阅或代码样式回退。22项功能／状态／优先级／居中检查通过，整文件成员审计0问题。

## B1／B2 联合视觉复录

两个版本只切换五份对应源码，旧版为e9bc9e707加不变的P1-A；当前字节备份并在finally逐项恢复。保持720×520、8公开owner、原96定制尺寸、正常motion、明暗scope和完整顺序，新增默认异常、成功、成功100、异常100、禁用异常100及恢复阶段。旧版2项捕获通过，候选22项功能＋2项捕获通过，144帧均opaque。48组独立A/A全部0、48组1 DIP全部检出（最少8,958像素）；旧新普通默认、状态恢复、最终重置的6组均0，同版本两类恢复8组均0。全图同坐标RGBA比较，无mask／对齐／容差。

以下像素变化仅涉及下方两个定制尺寸Circle／Dashboard，上面六个自动尺寸owner为0。已查看新旧异常、完成成功、完成异常、禁用及前序字体／本地值关键画面。

| 场景／输入 | 明色像素 | 暗色像素 | 变化含义 |
| --- | ---: | ---: | --- |
| 默认Normal、状态恢复、最终重置 | 0 | 0 | 原默认外观保持 |
| 默认Exception／50 | 954 | 948 | 去掉重叠百分比、异常图标居中 |
| 默认Success／50 | 872 | 866 | 去掉重叠百分比、成功图标居中 |
| 默认Success／100 | 132 | 132 | 恢复缺失的成功图标 |
| 默认Exception／100 | 4,175 | 4,175 | 原错误状态的成功色改为错误色、图标居中 |
| 默认Exception／100 disabled | 334 | 336 | 禁用图标位置修复，保留禁用语义 |
| Own最小字号28 | 1,478 | 1,468 | 既有最小字号配置实时生效 |
| Own字号28／图标36＋Exception | 1,124 | 1,118 | 尺寸实时生效、信息互斥并居中 |
| Own图标36＋Normal／100 | 408 | 406 | 成功图标尺寸生效 |
| 本地FontSize31、resize112 | 1,766 | 1,750 | 调用方字号不再被尺寸计算覆盖 |

位置调整同时改变部分隐藏零尺寸节点坐标及信息面板的DesiredSize，这是改由内容测量而非Canvas本地Width／Height的内部布局差异；没有把其几何记录写成全部相同。语义indicator的可见默认区域保持实际内容条带，公开ContractType仍为Panel，编译专用Style的覆盖／退出恢复继续通过。

## B2 最终源码的真实 Mac／AOT

生产默认Native／Metal宿主、scale2、720×440，同实例两主题；managed与Mach-O arm64 NativeAOT各96项检查通过，分别6.737／6.006秒。额外覆盖实际状态信息互斥、每轴0.5物理像素内的居中、Success／100图标、Exception／100错误色和disabled色、恢复Normal／50。managed构建25.773秒，0警告／0错误；AOT发布43.163秒，保留5条本机Homebrew最低系统版本链接警告。

这些验证确认最终新增内部面板及编译绑定在trim／AOT后实际工作；仍不冒充原生最终显示面的像素验收。当前候选视觉已获用户独立批准，不包含Git提交授权。极小／超大信息尺寸、其余线形／步骤动态字段、完整Global／性能及全家族矩阵继续P1后续。

## 独立视觉批准与清理

用户明确允许本表的圆形尺寸与状态修复，覆盖所示Own字号／图标、本地字体及默认结果状态的预期变化；不包含Git提交或原生最终像素验收。B1旧候选及最终B2媒体均按marker整体清理，合计244文件，媒体审计0发现。删除本包5个临时源／编译AXAML、6个驱动／备份；净新增常驻case0。真实原生专用工程整体删除430文件、1,088,413,176字节，保留源码及实际DLL／AOT文件的文本SHA256记录，未保留二进制或媒体。

最终清理树的11个精确Desktop类204通过、0失败，30.802秒；唯一保留的Temporary源是另案Range生命周期既有失败反例。本包NET8 Release构建27.391秒通过，0警告／0错误；6个C#文件成员审计0问题，不扩大本地回归范围；同源码Linux arm64的对应204项48.073秒通过，测试宿主差异如实限定；整个CT-17继续其余字段与形态。
