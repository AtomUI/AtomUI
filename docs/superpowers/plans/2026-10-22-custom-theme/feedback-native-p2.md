# CT15 P2 原生反馈宿主与输入验证

沿现有目录feature/custom-theme单agent执行；不分支/worktree/委派/提交/全量。复用既有137metadata/274Own与Message/Notification分包，当前仅补真实macOS原生宿主证据，不以本包替代所有家族出口。

## 本轮合同与判据

| AC | 来源与前置 | 操作与独立预期 |
| --- | --- | --- |
| N1 | 共享feedback-stack第5节；960×720原Window、标题及原正文、scale2 | 六位置，两种自然高度；同原客户端边界，卡片gap16；不改变尺寸/内容/NoWrap换取通过 |
| N2 | Message原模板NoWrap | 原长单行内容保持；显式三行独立自然增高，不能把单行裁剪描述为新修复 |
| N3 | Notification公开close/callback/键盘 | AppKit鼠标down/up及space经过Avalonia.Native到达原控件，close一次、card退出；观测包含handled事件 |
| N4 | 共享stack hover合同 | 保留五项/变高内容；折叠三真实层，native move进入展开五项、gap16命中保持展开、离开折叠 |
| N5 | 共享单调deadline/hover | 1500ms通知，300ms后hover、停1900ms仍保留；离开从剩余时长关闭一次 |
| N6 | 四算法与方向 | Default/Dark/Compact/Dark+Compact原生最终面，RTL同实例/全文/自然高度，恢复LTR；不宣称任意极值无裁剪 |
| N7 | scripted-visual-acceptance | 独立校准manager的完整原生窗口A/A、+1DIP及恢复，精确RGBA零通用容差，不裁剪/对齐/遮罩 |
| N8 | 平台/AOT与清理合同 | 官方Avalonia12.1.4、源码ProjectReference、实际NSWindow身份；managed与当前Mac NativeAOT分别启动，观察原图；取证/driver清理后精确模块 |

## 已定位驱动因素与裁决

Ruling：现合同的窗口host边缘定位没有排除caption区，MessageNoWrap是现有模板合同；两者保留，不从可见重叠/裁剪推导未经批准的产品变化。否则可能悄悄改变窗口锚点/内容合同。

首批未固定启动算法且白色原内容，暗/系统配方导致图面低对比；不是有效Default基线。明确Default后实际Light/画刷恢复。保留该首图观察为驱动前证，不记默认通过。

初次close与space回调成功但普通事件订阅未收到已处理事件；handled-events-too只读观测后到达为真。先前False是观测不足，不修改产品事件处理。

NSApplication.sendEvent可投递click/key，但本机合成MouseMoved不进入Avalonia root。源码精确tag12.1.4（2d79ba55）AvnView.mouseMoved调用native mouseEvent；改用AppKit公开NSResponder.mouseMoved入口后实际root记录坐标并展开/gap保持/离开折叠全通过。此为应用内合成Cocoa输入，经Avalonia.Native，不冒充外部物理鼠标或全OS输入自动化。前批原6个hover/间距FAIL保留为未投递输入，未修改产品hover。

一批截图窗口像素尺寸变化而首行身份仍960×720，缺少逐帧窗口状态记录，不接受该批固定尺寸结论；后续每capture独立断言ClientSize960×720与Normal。相同内容保留，不以最大最小尺寸锁绕过。

## 当前状态

原生direct-view探针40检查/0FAIL，已查看三行、展开五项/gap、折叠图。完整六位置/四算法/RTL/时长与独立校准managed/AOT执行中，结果、清理和最终模块尚未完成。原P1非零、Notification暗恢复、其他平台、完整P2/CT15仍开放；CT12/13/14及CT25/FW1原始边界不变。


## 执行增量（未清理，非完成报告）

未激活固定版：managed/AOT各147项属性/输入/捕获检查通过；首次managed139项与初始40/30探针分别留账。147版包含白色原宿主正文明确Black（保留原白底可读性），不改变目标反馈控件字体/颜色。四算法目标可见，Dark的白色宿主背景为原fixture，不据此判断应用全局背景。

独立原生Message校准：A/A0，+1DIP12940/max213；恢复3497/max2，差异在卡片上方阴影，不是正文glyph。首次无阴影Avalonia Border/TextBlock校准A/A/恢复0，+1DIP4186/max255。后加相同尺寸226.5×30、point367/686、radius8及三段BoxShadow的纯Avalonia Border对照：A/A/恢复均0，有阴影+1DIP4724/max213，无阴影+1DIP348/max224。不把消息框残留归给通用阴影或FW1。

公开Window.Activate之后完整managed148项通过，独立Message恢复仍3497/max2。外部OS窗口阴影会随激活状态使完整截取2056×1576/2144×1664切换；窗口ClientSize960×720及Normal检查均真，外部阴影不同不能用移动对齐/裁剪/容差比较。该轮有一张framework位移帧尺寸不同，不接受其跨尺寸数字。AOT147版同样混合外部阴影尺寸，跨运行像素尚不能通过；先前完全同尺寸已完成11对中10零/1张首激活阶段8310/max3，不推广为全部静态通过。

AOT147产物24845168字节，SHA256 18b769b9b5c20b7932766a9971cd9e89d780d14f2f88df0e7e16b709b02a66af；零IL/CS警告，五条原Homebrew macOS26对目标12的ld warning单列。冻结驱动SHA256 1e0b4a941550d6175e82496f88f8263ac70475f7fd91750a4f4defb1fa60454e，使用相同内容/正常motion。原30/40/139/147与失败批均分别保存，不合并伪称一个批次。

当前正在定位原生采样外部阴影与Message恢复残留，后续清理和最终精确模块仍必需；没有产品源修改，不关闭P2或CT15。


## 限定定位增量

固定激活版managed/AOT各148检查通过；AOT24861712字节，SHA256 ef66ee6d699d725a7dd972cdfc609226d9b992c1740a3c6a96d7d8771a234e5f。各33帧均完整，但跨运行31对外部窗口阴影尺寸不一致，仅余2对同尺寸：1零、首激活long-message457/max2；不接受整批跨运行像素通过。managed自身一张framework位移帧也被激活/外部阴影尺寸影响。不能只核ClientSize就推导整个OS画布一致。

单独诊断用系统screencapture -o取得完整1920×1440原生窗口矩形（不捕获外部窗口阴影），标题和全部反馈卡片阴影仍在画面内；没有后处理裁剪/对齐/遮罩，不能将这一诊断重定义为先前完整OS阴影基线。捕获前通过公开Activate并观测IsActive，保留原窗口CanResize与实际尺寸，不锁min/max避开触发。

normal-motion与独立no-motion-control的两组39属性/捕获检查通过；两组A/A0、位移12970/9645 max213。正常入场基线到移动恢复3497/max2，后续Window.InvalidateVisual及frame.InvalidateVisual仍同3497；禁动效对照恢复及root redraw为0。正常移动恢复帧与禁动效初帧完整RGBA精确相同；差异在正常入场初始阴影，不是恢复几何/正文变化。禁动效末张frame redraw顶部caption另7362/max200，不算控制恢复通过，不把无motion对照替代正常motion。

纯Avalonia Border淡入/平移、同shadow/226.5×30/位置367,686的进一步对照：final opacity1/identity、A/A0、位移4758/max213、恢复0；最后root redraw时窗口从1920×1440变2440×1912且尺寸断言FAIL，首激活断言也FAIL（19检查17PASS/2FAIL），整批不作为通过。只报告前四帧的实际比较，不用其最后尺寸变化判通用框架根因。

已查看native AOT Dark+Compact RTL、normal恢复与原差异图；真实原内容/标题/默认NoWrap、展开gap/键盘关闭和悬停计时结论仍保持。尚在正常入场初始重绘/实际opacity的单变量定位；无产品修改、不归FW1、不改官方依赖或容差。


## 初始重绘定位结果

最终独立initial-redraw两组51项检查0FAIL。正常motion初始A/A0；只重绘Window仍0；只使原PART_Frame失效重绘即出现3497/max2变化，随后移动恢复帧与其完全相同。无motion组初始Window/frame重绘、恢复均0。各祖先和Frame/MotionActor/Card的有效Opacity与client composition Opacity精确1，actor/card变换为identity；没有通过给Part写外观或去掉原内容构造成功。

这定位了“正常入场后的初始Frame绘制记录”与后续稳定绘制之间的差异，尚未证明具体生产根因；不能据此在产品NotifyCompleted补任意InvalidateVisual、关闭motion或更换shadow作为修复。纯Avalonia Border及直接淡入对照未复现，故不归通用框架/FW1；该3497/max2仍未通过。

本轮没有反馈控件运行代码变化。工程有效结论为原生六位置/自然高度、完整长文值与NoWrap、鼠标close/Space到达、五卡片hover/gap/leave、真实deadline暂停恢复、四算法/RTL与当前Mac NativeAOT的上述限定检查；捕获与微差不是全家族/全平台通过。进入清理及最终精确模块；后续继续Timeline P3恢复等真实出口，不在本切片终止CT15。


## 清理、验收与作者自审

2026-11-01 P2原生宿主补证：native NSWindow960×720/scale2，managed/AOT完整各147及固定激活各148检查，六位置/变高gap16、原NoWrap全文/显式换行、Cocoa click/Space、五卡片hover/gap/leave、1500ms真实悬停剩余计时、四算法RTL通过。应用内合成Cocoa经Avalonia.Native，不冒充外部物理/其他OS输入。完整OS阴影画布随激活变化，跨运行像素不通过；正常入场初Frame与稳定绘制3497/max2差异定位，初始frame重绘触发、Window重绘无效，纯Border/淡入不复现，不归FW1或扩大容差。248取证/447原会话(411driver)/两AOT已清理，audit clean；清理后精确feedback padding/panel/motion与GroupBox70PASS/0Skip、1s，NET8零警告零错误21.10s。见feedback-native-p2.md；无反馈运行代码改变，GroupBox仅三Reserved XML说明同步。原失败、完整P2/CT15及CT25/FW1保留，不提交。

所有正常/失败/不完整原生批次媒体共248文件经evidence cleanup删除，findings=[]、removed=true；外部447文件1226411738字节（driver411，两publish及冻结驱动文本）删除，native marker删除。启动的native进程已全部退出；无CUA、无全量回归、无新永久测试。独立最终门禁的run.py及marker在门禁后清理。

作者另步自审：不通过修改真实反馈shadow、motion、窗口min/max、NoWrap、标题或原内容消除差异；专用Style入口保持，读取Part/InvalidateVisual仅为临时单变量诊断。当前只确认列明原生业务路径；OS阴影画布差异、初Frame3497/max2、原P1恢复及其他平台/家族仍未通过。非独立评审，未批准新的视觉差异或提交。

## 2026-11-02 原历史复原与假设排除

复原本会话33718初始Program及其后18次字符串/片段修订，只解释源文本变换，不执行历史命令或Git操作。确切最终驱动25926字符，SHA256 `39f5f57c144fd390d1a8e69b901b0fab4dcc68cf75ca3cec9ff2e5e8d20b7631`。原960×720/scale2、Dark+Compact、BottomCenter、完整Independent native calibration文本、226.5×30/点367,686/三段阴影、正常motion及no-motion独立对照、激活和捕获顺序均恢复。

最初只恢复初始Program后另接诊断的简化重放25检查通过，但没有复现3497，不能当作原失败修复；完整恢复18次历史后51检查通过，原生整窗1920×1440的A/A与Window重绘均0，初次Frame重录及之后移动恢复、Window/Frame重绘均严格3497/max2，bbox(739,1334)-(1183,1369)。1DIP12970/max213；无motion组各恢复0、1DIP9645/max213。已查看原完整默认与Frame重录图，原全文/标题/尺寸/窗口内完整阴影保持，无裁剪/对齐/容差。

运行时只读观测否定双层变换假设：正常Actor为identity MatrixTransform，内部Presenter.RenderTransform为null，并没有重复应用变换。单变量省略启动Default主题、以及单独省略每次Capture中的Activate（250ms等待与IsActive检查保持），两批各51检查通过，正常组仍3497/max2、无motion恢复仍0，排除这两个因素。

官方ContentControl/ContentPresenter/Border的不可变TransformOperations与可变Translate→Matrix对照，两者同226.5×30/点367,686、原三段阴影及300ms easing。可变组A/A、Frame重录和位移恢复全0，1DIP4758/max213，没有复现原残留；不可变组一张Frame重录IsActive=false，整批37中36检查通过/1失败，不计整批成功。该失活帧7395/max215单列；两组A/A0、位移恢复0不覆盖原3497。

未获得新的根因证据，停止这个分支，不复制更多近似工程、不重复AOT、不改产品或官方依赖、不归入FW1/FW2。原3497仍阻塞自己的原生渲染出口，不拖住其他独立路径。当前生产与测试编译输入未变；清理后的精确模块、媒体与会话清理记录见后续收尾。

本轮80取证文件经官方audit/cleanup clean后删除，findings=[]；原生进程均已退出。没有项目内临时编译输入，复用匹配清理后程序集，独占共享产物锁运行精确MessageSemanticPart/FeedbackManagerPadding为17PASS/0FAIL/0Skip、1s。外部会话及marker已删除；未新增永久测试、AOT或NET8构建。原同源NET8和AOT业务收据继续按原范围复用，不宣称本轮重发AOT。
