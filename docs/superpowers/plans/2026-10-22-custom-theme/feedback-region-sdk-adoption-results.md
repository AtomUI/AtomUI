# CT15 软件 Skia 正式采用

2026-11-03本轮工程、原场景重放、批准及清理后门禁完成；整体有限出口和保留范围见[最终结果](feedback-final-exit-results.md)。以下按实际先后保留采用与补证过程，阶段待办由后续结果收敛，不倒写旧失败。

批准原话与请求ID集中在[原提案登记](feedback-region-rendering-repair-proposal.md#正式采用批准登记)，原始因果、失败候选、1632/652与73图形证据只保留在[验证权威记录](feedback-region-sdk-results.md)。本记录只跟踪当前仓库的落地结果，不改写原CT12/13/14或FW1。

## 落地方案

`compat/Avalonia.Skia` 保存精确12.1.4官方源码与MIT许可、声明、签名输入、逐文件原始/本地SHA。独立工程输出相同版本和public-key identity的管理程序集，支持net10/net8，不引用AtomUI Core/控件/Generator/Toolchain。官方native依赖版本及NuGet缓存不变。

仓库targets为明确Gallery host和Avalonia测试项目加入私有ProjectReference；解析包资产后排除其重复官方编译输入，并防止共享bin目录里的子库覆盖所选管理DLL。基本库的公共包依赖与平台设置不变，控件NuGet包不自动携带此源码兼容实现。可通过显式属性关闭接入以做官方基线，未通过不随之视为通过。

## 当前证据

- 原引用来源检查有效RED：Desktop测试只解析到官方包路径。
- 接入后同检查GREEN：Desktop测试及Gallery host唯一编译路径指向兼容项目输出。
- 两TFM工程构建通过；四文件补齐仓库花括号后Release569个方法体IL与格式前全同。
- Desktop相关精确193项通过，运行目录管理DLL SHA与项目输出一致；deps中唯一runtime来源为兼容项目。
- 临时真实Skia接入2项通过（78ms）：验证加载实现/版本，以及原Avatar精确两行裁剪数据的全RGBA恢复；不是新近似模型。旧Rendering测试中xUnit1051 warning保留，未修改无关测试。
- Gallery Debug首构建GalleryBase编译器csc原生exit139，未计产品反证；同源该单模块隔离复核0警告通过（8.08s），完整host后验收继续。

## 后续门禁

本段初始待办中的AOT链接/发布来源和Gallery启动身份已由下节补齐；仍需临时清理及清理后精确模块。不能据采用检查单独关闭完整CT15。GPU、32真实时相和其他原未覆盖路径继续单列。

## Gallery与实际发布接入

Gallery Debug最终构建0警告0错误22.05s；使用项目launcher --no-build启动当前checkout绝对路径，PID76265对应本目录binary，进程持续存活8s、无启动异常，按已核对PID正常停止。Debug运行目录Skia SHA与兼容项目Debug输出全同。未用此启动smoke替代UI输入或完整视觉证据。

正式PublishToLocal NativeAOT流程exit0并通过脚本native输出校验，ILC rsp明确-r指向compat/Avalonia.Skia/bin/Release/net10.0/Avalonia.Skia.dll。发布native binary SHA567e417347eb84a5ef1b6c25aaaed855f7b50acce98952530bfeb2b0ef861c0a，绝对路径启动PID82314、存活8s后由本会话正常停止。ReactiveUI/Avalonia两个未改程序集各IL2104/IL3053、五条Homebrew ld警告保留，未声称发布零警告或已定位这些警告。

发布libSkiaSharp.dylib SHAe09f07ae1df62ded475351f56d8dc8366cd679043f28b65d9dee597a5fd0da6c与官方SkiaSharp.NativeAssets.macOS/3.119.4逐字同；libHarfBuzzSharp.dylib SHAb2d7d39a46954b3d7310540bdf393f398ac4194ee029b995deeb1db0c161c43d与官方macOS/8.3.1.3逐字同。首次查询误限managed包目录未命中，改从实际assets解析到native包后完成对照，未据空匹配声称native被改。

临时2项接入测试4673字节源码已删除，后续精确Rendering模块验收待集中执行；继续ToolTip/PopupConfirm原TC1-4/PC1等余项，当前不因采用或五族通过自动关闭CT15。

## 采用后原余项回放

继续同一外部参数化driver恢复ToolTip/PopupConfirm原6596 fixture、7195完整Program、原style更新及7330圆角17修订，仅抽取文本。夹具外部namespace首编译纠正，未执行任何历史产品替换命令。实际编译与runtime直接使用仓库compat产物，未手工复制修正版DLL。

原120帧/92性质全部PASS，四算法default→colour-restore（TC1–TC4）及default→cancel-restore（含PC1 Compact）各RGBA0，最终content恢复亦按原记录核对中。原全部Own/Global/Native/Part/箭头/宽度/RTL/滚动/模板/图标/取消/关闭历史与1200×900、140视口、24行完整内容保持；没有关motion或改裁剪。

原软件失败在采用后的这些具体路径被修正，原strictFAIL历史和原native输入/AOT范围不倒写；本轮只复用已通过共享backend AOT/原控制注册证据，不把此managed采样称为新Tooltip AOT采样。后续复核GroupBox/QRCode/Collapse/Flyouts各表列原恢复，仍用同一工程/runner，不新造模型。


### 原恢复合批结果（正式采用后）

同一参数工程、同一已加载兼容管理DLL SHA `6f0c90d3bc8f91e27a37d064290564510c57cef02151833af1a759eb80bab06c`；持共享采样独占锁，证据登记顺序执行，最多两个独立冻结进程只读采样。未另造图元模型、关动效、改viewport或加入重绘补丁。

| 原完整历史 | 帧 / 性质 | 本轮严格恢复比较 |
| --- | --- | --- |
| ToolTip / PopupConfirm | 120 / 92 PASS | TC1–4及四算法cancel恢复（含PC1），8/8 RGBA0 |
| GroupBox | 164 / 64 PASS | GB1–4 global→native-clear，4/4 RGBA0 |
| Collapse | 220 / 128 PASS | CR1–4 global→native-clear，4/4 RGBA0 |
| Flyouts | 196 / 144 PASS | FY1–4 host-default→global-clear，4/4 RGBA0 |
| QRCode | 240 / 140 PASS | 四算法parts-clear/icon-clear，8/8 RGBA0 |

以上28条比较全部严格零差，原非零历史不倒写。本轮是managed原历史重放，未称新增各家族AOT；共享backend的既有AOT及未改控件注册证据按原范围复用。已查看当前暗Compact恢复原整图：原长标题截断、暗低对比、滚动偏移和二维码保持，未以改内容或移除限制消差。工程/媒体清理和清理后精确门禁尚待集中完成，当前不以采样结果单独关闭完整家族或CT15。

Card原120帧/240性质PASS，四算法×三尺寸×LTR/RTL的CA1–CA24 left→right→left恢复24/24严格RGBA0。恢复44570夹具时补齐原44588 Header API纠正和44610/44629实际120×32/Left修订；这些是历史恢复输入，不是新产品修改。Alert原19阶段/76帧/756性质PASS，四算法AD1–AD4 disabled恢复4/4严格RGBA0；1274 fixture及1519 LineHeight44保持。已查看当前暗Compact两族恢复整图，原完整文案、Action、标题正文、RTL及原暗低对比保持。新增28恢复与上一批28恢复合计56条归零，历史FAIL保留；临时清理及最终模块门禁仍待集中完成。

Dot阴影原64帧/96性质PASS，四算法×standalone/target的Green→Blue→透明→清除→Green往返8/8严格RGBA0，原Compact独立1/max1两条现归零。Notification原六卡片/1280×900/384宽MaxHeight380、正常入场及公开Expiration10→4完整驱动128/128PASS（原124PASS/4严格FAIL）；8个colour/global ABA恢复及8个后续root-redraw均RGBA0，逐节点有效值、原引用、10/4全部恢复。后续root-redraw只作原诊断，不作为恢复通过依据。旧四FAIL及旧独立模型限制保留，本轮未运行近似模型。

ImagePreviewer正式采用后原最终448帧/200性质第一批完整PASS，四算法×native/Overlay×single/group及独立校准无缺帧；native实际1344×896、Overlay900×650，PNG encoder及14284原最终业务/Style修订保持。已查看当前Dark native reset整图，原PatternImage、完整标题、导航/底部透明度和页面内容保持。冻结同一产物第二次原完整历史只验证原31静态变异及其他384静态跨运行；64真实loading持续态不做未配对像素PASS。当前第二批仍在执行，不提前填写静态结果。

ImagePreviewer两批各448帧/200性质均PASS；384静态同坐标RGBA比较384/384零差，原IP1–IP31所在静态集合均已覆盖。64真实loading图仍未配对相位，未作为PASS。此为同JIT重复两批，没有改称新AOT全384通过；已验证的共享渲染backend AOT和原控件AOT业务证据按原范围复用。原171旧新、历史未采帧与OS/Browser等限定边界仍各自保留，不由本轮静态重复结论扩大。

真实macOS原NM场景：精确复原最终25926字符Program（SHA `39f5f57c144fd390d1a8e69b901b0fab4dcc68cf75ca3cec9ff2e5e8d20b7631`），仅将入口接至统一参数工程；正常motion/no-motion独立对照、960×720/scale2、Dark+Compact/BottomCenter/完整文案与原16捕获顺序保持。采用后的原51性质全部PASS/exit0；normal-motion的A/A、Window初重绘、Frame初重录、1DIP恢复、后续Window/Frame重绘全部RGBA0，原3497/max2对应NM1–NM4现归零；1DIP仍9670/max213有效检出。no-motion同6恢复均0、1DIP同9670/max213。已查看当前1920×1440原生整窗，caption、原host文字、底部完整校准内容及内部阴影保持；未做裁剪/对齐/遮罩或动效取消。官方GPU路径未修改，本结果不推导其他GPU配置通过。

本轮native证据首登记误用了简写文件标签；原实际16帧仍在evidence artifacts内递归审计。采样进程退出后为真实文件分配actual-路径并移动，本体字节未改；修正登记后再清理，不伪称首次路径完全正确。本次是managed native原历史，未声称新增native AOT采样。

Message原80主帧/236性质及独立16校准帧/4性质全部PASS；正常motion下Own/Global/Native/Part/Type/RTL/Stack七恢复×四配方28/28 RGBA0，四独立1DIP恢复4/4 RGBA0，八A/A全0；四1DIP分别10575/9663/10516/9635（max224/224/212/212）有效检出。原3566将人工位移分离到独立实例的最终历史保持，未用最初受扰88帧替代主基线；保留920×700、四类型全文、zero expiration、Stack/真实hover/选择键及关闭一次。当前各原功能/恢复路径已验证，不据此把旧36跨运行或其他排除项自动写PASS。

Badge原160主帧/64性质及16独立校准帧/4性质全部PASS（未将后续只读Dot观察器的88检查冒充本次）。Own/Global/颜色/count/显示/end/RTL/重挂/business九恢复×四算法36/36严格RGBA0，独立A/A与恢复各4均0；1DIP15270/15389/15275/15336（max255/233/255/233）有效。原CountTarget主场景始终12，保留原1000×1050、六owner、完整target与专用Style、真实键鼠及整个显隐/RTL历史，不以夹具Count7误判撤销原差异；原end/RTL/重挂/校准微差现对应路径归零。临时媒体及模块验收仍待集中收尾。

Spin恢复原最终43状态×四算法172帧/124性质完整PASS；每算法主历史之后的独立cal A/A与位移恢复各4均RGBA0，1DIP均1210/max233/170/233/170，独立无Controls plain四恢复0。原8个cal旧新非零涉及显式ControlAlgorithmMode.Global，并非无配置默认；本轮仅当前一致性验证，没有旧版像素对照，因此不擅自将那8个历史未批准旧新比较改PASS。恢复驱动时补齐原DriverStyles后构建0警告，未使用残留失败编译产物。

Tour仅原问题配方Dark/Dark+Compact，各53帧/28性质×两完整重复，四次均PASS；90静态重复比较90/90严格RGBA0，包含原两条navigated 1/max1。原正常open/move时相16帧不配对比较，原Light/Compact及AOT有效业务证据复用。已查看当前DC navigated整图，目标/遮罩/完整内容/按钮保持；未改Primary大min原slide与重叠合同。

集中清理：官方evidence audit/cleanup exit0，findings=[]，删除3311取证文件/清单（57712374字节）；外部统一driver/发布/恢复文本等372文件、975715291字节全部删除。本轮采样进程均已退出，临时2项项目测试此前已删除；真实compat源码/官方Noise产品素材及签名输入保留。精确模块验收在清理后执行，未跑全量或提交。

## 清理后合批门禁

Desktop受影响具名空间及manager padding/placement/Spin recurring共483PASS/0FAIL/0Skip（18s）；Common Avatar 7PASS（369ms）；五个具名Rendering类77PASS（46s）。Rendering既有ButtonFocusVisualRenderingTests:78的xUnit1051警告保留。Desktop NET8 Release零警告零错误24.07s，compat NET8零警告零错误0.54s，git diff --check通过。没有全量回归，外部其他checkout的Molan测试进程未受干预。

Spin八个独立cal旧新比较仍单列：找回原pre-hot-style逐项输出，确认为Dark/DC×cal-a/cal-aa/cal-shift/cal-restore各1002/max42；最终hot-style批只有八条总数，不能把前批数值冒称最终值。原HEAD主题Tip取Shared ColorPrimary，当前主题取实际Spin owner Global。当前只验证同一真实cal输入的单变量专用Style A/B，未改生产源或搭图元模型；不会把八条历史未经审查的变化自动视为批准。

该A/B首批两用例未到截图即被原字号12前置条件阻止：Gallery Rendering host实际继承14，而原standalone Application继承12，属于宿主输入不一致，不是产品RED。下一批在调用方scope显式保留原12字号，背景、420×240、18×18自定义内容、完整Tip、专用Style与四操作保持；仅针对真实前景角色差异，不声称整版旧源码已重新采样。新增临时源仍需清理，随后只重跑对应Rendering门禁；先前483/7与NET8证据按未变源码复用。

A/B完成：两用例PASS；两角色颜色分别#FF1668DC/#FF165BBE，Tip几何/12字号/Alibaba Sans/全文一致，八对各1030/max42；四角色自身恢复全0。前批1002与当前1030差别已披露，不把当前角色对照等同整版旧源码图。已查看原整图；具体条目和限制只见[Spin校准前景限定表](spin-calibration-colour-visual-review.md)。3临时源5876字节、19取证/清单已清理，官方audit clean；清理后Rendering77PASS/0Skip、44s，xUnit1051仍保留。兼容源manifest全部SHA一致，当前运行目录管理DLL与compat项目输出逐字同（6f0c90d3…bab06c）。

SC1–SC8已获用户限定批准，原话/请求ID只见[批准登记](spin-calibration-colour-visual-review.md#限定批准登记)；未因此批准完整CT15或改写原生输入旧失败。

最终原生输入前置条件核对、261有效原场景与清理后Dialog/MessageBox276通过，详见[原生投递结果](feedback-native-input-delivery-results.md)。它纠正验证驱动的输入前提，未改产品焦点代码；旧失败不自动计PASS或移交FW2。与本记录483/7/77构成最终843项清理后精确门禁，整体状态由最终出口统一记录。
