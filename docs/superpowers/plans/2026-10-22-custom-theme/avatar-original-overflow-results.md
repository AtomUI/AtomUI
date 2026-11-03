# CT15 Avatar 原overflow恢复路径

2026-11-02，复用原3metadata/11Own与48对限定批准，原未通过不自动归FW1/FW2。只恢复本会话9417 Program及9431/9448/9544编译修订、9633 Popup顺序、9876/9948/9956实际承载背景修正；不执行历史命令或改产品源。

原900×700/scale1、四算法、三独立Avatar（文字/图标/8×8借用位图）及四成员/显示二的Group；原20帧popup分支按open-default→open-own→open-restore→closed→retired，正常motion/Dispatcher、最少700ms/12连续raw稳定帧。Own仍47与Group间距19；原字串/位图/尺寸和实际配方背景保持。

完整重放20性质检查PASS。open-default→open-restore再次为Light12/max8、Dark12/max7，bbox两者均(59,178)-(73,180)；Compact/DarkCompact均0，精确复现原失败。没有用重建一侧、挑零帧或修改容差消差。

坐标可能处于主场景独立Avatar而非popup；目前仅在稳定捕获后追加只读focus/pointer/绝对bounds观测，重新执行同一历史中。产品源码无变化；当前外部工程/媒体尚在，未发布新AOT。原closed旧新/cross和完整Avatar/CT15仍保留，后续清理与精确模块待完成。


只读观测批同20检查通过，所有步骤焦点为空、受影响Avatar无pointer-over；坐标确认差异在主场景图标Avatar（绝对50,148、32×32）底边，不是overflow内容。普通Group位于50,264；图像Avatar在50,206。正在以完整原前缀后的同一PixelAlignedBorder Frame失效重录作单变量，不先扩展独立模型；不触碰已关闭CT12/FW1边界。

Frame/Avatar/承载背景三个同值重绘对照均没有消除残留：原20检查各通过，普通明始终12/max8、暗12/max7，两Compact0。Frame有效描边0、普通圆角16/32盒、Compact圆角14/28盒，明背景#3f000000、暗#3fffffff，Opacity1、Frame.ClipToBounds=false。完整承载黑→原配方背景恢复仍同12像素；这与已有FW2“背景重绘0”的前证不同，不能据类似大小自动转归FW2。下一只读核对实际边框geometry/cache参数，不修改基础组件、半径或裁剪。


真实helper只读字段批20检查通过：所有阶段BackgroundGeometryCache=null，实际走CenterBorder均匀零描边的DrawRectangle简单分支；size/radius按32/16→47/23.5→32/16恢复，两Compact28/14→47/23.5→28/14，scale1。排除了StreamGeometry缓存未重建的假设。当前只做一条官方12.1.4直接DrawRectangle圆形尺寸往返对照，不改产品或PixelAlignedBorder/CT12结论。


第一官方对照只做32/28→47→原直径，四组A/A、恢复、重录及背景恢复全0，未复现；该对照尚未含原Green填充往返，不用于原失败结论。当前在同一参数模型补齐原Green→半透明填充，不新建模型/项目；首Rect重载编译错误仅为驱动问题已修正。


官方圆对照：仅尺寸、补原Green填充往返、补实际Bounds/裁剪三个版本，各四条件恢复均0，未复现原12像素。最后一版同时补Bounds与裁剪，不能用于单变量归因；未据此提出产品修正。至此停止该模型分支，保留原失败/定位/近似限制，不转归FW2，不发布无新增产品风险的AOT。已查看原open-default/open-restore完整图；原文字、图像、Group和overlay全部保持。

收尾：330取证官方audit/cleanup clean后删除，外部244文件/1187645680字节及marker已删除。无项目内临时编译输入或产品变化，复用匹配清理后程序集，独占共享输出锁精确AvatarGroup13PASS/0Skip、827ms；Common Avatar7与最终NET820.57s按未受影响原证据复用，不重跑全量或发布AOT。原失败继续阻塞限定出口，完整CT15不关闭。

额外具体源码线索核对：真实helper保留非空零宽Pen，而最初纯模型用null。官方12.1.4绘制流会保留该资源、Skia末端跳过零宽stroke；单个参数模型比较null/可变Pen0/不可变Pen0及原颜色/尺寸往返，明暗六组恢复均0，未复现。该假设排除，不改PenUtils/基础家族；24媒体audit/cleanup clean后删除，外部90文件/569446403字节与marker清理。无产品或测试编译输入变化，不为纯外部负对照再跑模块/AOT。


用户再次要求按收敛方法继续后，停止所有独立近似模型，恢复同一原场景，在原20帧捕获之外仅增加真实Frame.Render到RenderTargetBitmap与真实Frame即时Visual渲染两个分离输出。两个即时后端各自只比较原初值/恢复，不与合成器图混作同一基线；另跑未调用即时Render的原分支验证观测不改变原12像素。该实验排除消费者实际绘制参数/输出与合成器记录的责任层级，不请求改变既有批准或替换依赖。当前无生产/项目测试修改，临时工程和媒体尚在，结果/清理待完成。

同一真实场景进一步分层：Frame直接绘制、Frame即时Visual、完整图标Avatar即时Visual、900×700整窗即时Visual，四算法初值/恢复全部严格0；合成器仍12/8、12/7、0、0。输出均非空，普通Frame855个非零Alpha像素、Avatar34种颜色、整窗630000个非零Alpha像素，关键整窗/Avatar图已实际查看。没有以即时后端的绿替代合成器失败。

一次冻结诊断AOT发布/执行成功，实际29064272字节，SHA256 `72dce6bdf62f180aa27973c629b3733bf4f74d1f46dbf5570488ed368c22965b`，0 IL/CS warning、五条既有Homebrew linker warning单列。AOT20性质检查通过；同16项真实即时恢复均0。52张跨运行中48零，closed四张仍非零L88/max8、D57/max7、C53/max12、DC44/max13，全部保留，不拿它们支持新的归属。

这把本次两条open恢复的现象收敛为真实场景的保留式呈现差异；没有独立官方最小反例，因此不宣称完整框架根因。当前仅补原Own阶段作为真实即时输出的检错正对照；新增managed日志/捕获属于诊断扩展，不冒称重新发布同一AOT。后续清理/精确验收仍待。


真实Frame输出分层首批：原分支20检查与追加即时输出分支20检查均PASS，合成器依然普通明12/max8、暗12/max7、两Compact0。真实Frame.Render直接绘到RenderTargetBitmap，以及Frame即时Visual各自初值/恢复四算法全部RGBA0；Background为ImmutableSolidColorBrush、Opacity1，Color/Size/Radius/CenterBorder/零描边/无shadow完全恢复。即时输出不代替合成器门禁，也未消除原失败。下一仅增加同一真实Avatar与整窗即时渲染，继续区分父级/相邻数据和合成器结果，不造近似模型。

真实即时Own正对照：整窗L25766/D24779/C25150/DC23780像素检出、max255，恢复四组0，20检查通过。334取证官方audit/cleanup clean后删除，外部445文件/1179655232字节和marker清理。无项目内临时编译输入，复用匹配清理后程序集，独占锁精确AvatarGroup13PASS/0Skip、1s；当前产品未改，Common Avatar7/最终NET820.57s按未受影响范围复用。仅[AR1/AR2两路径](avatar-open-restore-visual-review.md)提出新的有限调查裁决，未自动转归，closed/其他边界保持。

AR1/AR2已获用户选项1的有限调查归属，原话/请求ID/限制集中登记在[限定表](avatar-open-restore-visual-review.md#限定调查归属批准)；非零仍FAIL，不认定与原FW2同根因，其他边界不变。
