# CT-11 M3：精确波纹与 Splash 合成动效

本包实现默认时序与精确输入分离，不替代完整家族、原生最终显示面或各平台验收。

旧 consumer 把显式透明度800ms变成850ms，Splash缺少周期、段长比及关闭动效入口。新增 Global Alias `WaveSizeMotionDuration` / `WaveOpacityMotionDuration`，默认保持 Slow / 正Slow+50ms（Slow为0则0）；两项 Easing Seed 默认 CubicEaseOut。消费端不再加50ms，EnableMotion=false最终归零。共享Wave和Transition复用原生owner/context/resource绑定，取实际模板owner的Effective Global，无登记的独立primitive回退Shared。

生成器按完整CLR类型识别Core的OwnerTokenResource；外部同短名资源继续检查owner。实际ATOMUIREG002失败修复后，2临时+8已有用例通过（3.6358秒）。公共Button实际鼠标输入的painter取得800/600ms、Linear/Sine曲线，同实例恢复300/350及Cubic；Button400与Dropdown800、Transition700与200分离。Core默认/Slow依赖/0/精确覆盖/EnableMotion共4项通过（1.9052秒）。共享绑定完整图GC也通过，Desktop最终临时3/3（3.795秒）。

Splash原生CycleDuration默认1.2s、SegmentRatio默认0.3、IsMotionEnabled默认true，可用AXAML style Setter定制；启动主题只读取准备阶段Shared.EnableMotion。关闭动效/周期0保留居中静态段，比值0/无画刷不维持循环；隐藏、确定进度、分离停止循环。分离清理自定义视觉和订阅。非法周期/比例及准备阶段关闭动效已验证。

自审发现可变SplineEasing仍跨Transition owner共享：第一owner改X1=0.9，第二owner及prototype也变0.9，实际失败。修复复用既有TokenValueOperations的有限AOT copy，冻结声明快照，每owner复制Spline/Spring，已知无状态曲线可共享，未知类型拒绝。该修复及其他Controls临时5项+已有Splash3项最终8/8通过（2.1759秒）。

Skia/Headless Drawing=false，Avalonia12.1.4.0，520×340、scale1，实际Splash模板及四角进度条。默认对照只临时固定合成时钟输入300ms，不改形状/样式/循环，逐字节恢复：Light/Dark A/A0/0，1DIP检错2306/2317；默认旧→新0/0，周期与比例修改1346/1346，恢复0/0，静止连续帧0/0，重新启用0/0。关键默认、修改、暗色静止和实时图像已查看。

另用真实CompositionNow：移动帧差3347/4297，关闭后连续帧0/0。只读回调计数停止增长：Light关闭280/280、隐藏374/374、分离374/374；Dark300/300、402/402、402/402。实际时钟关闭计数202/202、200/200。分离后_indicatorVisual=null。临时计数与固定时钟均在finally删除并逐字节恢复。

独立Wave primitive的真实动画时钟：默认140ms透明度0.0815563/0.1007007，500ms已结束；定制800ms时400ms仍播放（0.0566498/0.0562432），950ms结束。此证据不替代Button原生最终显示面的闪动验收。

外部驱动命令：`dotnet run --project /private/tmp/atomui-motion-exact-visual/Visual.csproj -- <artifacts> before|after|live [dark]`。临时helper在Controls.Tests/Temporary/CustomThemeMotionExact，鼠标与GC验证在Desktop.Tests同名Temporary目录。引用测试程序集产生的Main冲突通过显式StartupObject=Program消除，夹具Window命名空间冲突改用完整类型；均不计作产品失败。

当前macOS26/arm64 NativeAOT实际运行读取模板原生属性：Wave800/600/Linear、Transition700、同wave恢复300/350，PASS。链接使用本机openssl@3/brotli；最低OS26与目标12的链接警告保留，不声明旧OS或其他平台通过。最后easing复制修复后的再次AOT、清理与focused/NET8结果待补。

最终出口：easing修复后重新发布并实际运行NativeAOT，数值及同实例恢复再次PASS；临时AOT工程及产物删除。清理后Core45/Controls3/Generator63/Desktop76全部通过；NET8 Desktop含Core/Common/Shared构建0警告0错误17.67秒。14项临时用例、全部helper/驱动和29个媒体文件删除，audit clean。4文件/3个控件成员审计0问题，652个本地文档链接无缺失，diff check clean。作者自审已修复可变easing串值；本包无剩余阻断发现。其余literal集合及全部owner按家族继续，未借此标为已完成；Button原生闪动和其他平台仍分别验收。
