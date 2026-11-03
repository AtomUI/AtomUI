# MarqueeLabel 桌面版实现原理

AbstractMarqueeLabel 管理测量、动画与绘制，Desktop MarqueeLabel 提供真实继承父级桥接，MarqueeLabelToken 定义两个常量默认，MarqueeLabelTheme.axaml 声明 Root 资源与字体。渲染仍使用原 TextLayout，不创建重复 TextBlock 或修改 Text。

测量比较实际文字宽度和可用宽度。适合时取消动画并复位偏移/宽度缓存；溢出时按当前 TextLayout.Width 核对定义。文本或字体改变后重建终点，保持视口与裁剪；Hover 中的重排不启动滚动。绘制依据 PivotOffset 展示原文本，需要时在相隔 CycleSpace 处绘制第二副本。

原时长为 4 × max(1, distance / MoveSpeed × 1000) 毫秒，distance 是文字宽度与间距之和。不能根据参数名称擅自移除倍数或更换 easing。循环使用 PlaybackBehavior.OnlyIfVisible，不通过停止默认动画取得假静态证据。

每次启动持有独立 Animation 与 CancellationTokenSource。完成回调只在二者仍是当前实例时清字段，避免旧取消任务清除新动画。Cleanup 同步退休 running，取消并释放 CTS；卸载复位重启位置。运行时宽度、相位和取消生命周期无法由主题 Setter 表达，仍在共享基类；可声明资源绑定在 ControlTheme。

可选字体/前景复用有限类型化继承机制，按真实 InheritanceParent 与明确 Control Global 解析，不用反射或树扫描定制文字。没有新增公开 Part、业务状态或注册身份。[公共模型](overview.md)、[Token](token.md)与[验证记录](../../../../superpowers/plans/2026-10-22-custom-theme/marquee-label-consumers-results.md)分别拥有公共合同、参数与阶段证据。
