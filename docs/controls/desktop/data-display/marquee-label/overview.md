# MarqueeLabel 桌面版架构设计

MarqueeLabel 是 AtomUI.Desktop.Controls 的连续文字展示控件，继承共享 AbstractMarqueeLabel 与 Avalonia TextBlock，用于 Alert 等组合区域。它保留完整 Text，在受限宽度中绘制连续文字；没有独立 Gallery 页面或公开 Semantic Part manifest。

公共模型由继承的文本、字体、前景、尺寸、FlowDirection 与 CycleSpace、MoveSpeed 两个参数组成。CycleSpace 控制相邻文字副本的间距，MoveSpeed 是原有速度参数。Root 原生属性与普通 Style 可覆盖主题；内部 PivotOffset 不是业务 API 或 Token，没有 SizeType/Custom、弹层或控件专属键盘/关闭事件。

文字适合可用宽度时不滚动，偏移复位；溢出时使用原循环动画。Hover 在原视口内暂停于可见位置，离开恢复；隐藏和卸载遵循既有可见性及取消机制。文本或字体造成真实宽度变化时按当前宽度重建，旧完成回调不能清除新动画。

ControlTheme 声明两个 Own 与可选字体/前景，未明确配置保持真实继承，原生 Local/Style 优先。主题不改变原文本、裁剪、测量、方向或时长公式，不靠扩宽或关闭裁剪使滚动消失。未公开 Semantic manifest，不将私有动画或文字副本提升为 Part。

维护入口：[实现](implementation.md)、[Token](token.md)、[Changelog](changelog.md)。[CT15 结果](../../../../superpowers/plans/2026-10-22-custom-theme/marquee-label-consumers-results.md)记录阶段范围和未覆盖边界；Headless 不代替真实 OS、全部平台或极值。
