# Spin Token

Spin 与 SpinIndicator 的运行身份分别是 `AtomUI:Spin`、`AtomUI:SpinIndicator`。七个 Own 仅在 SpinToken 定义；SpinIndicator 复用 Spin 的 typed Own 资源，不建立第二套 Own。内部 SpinIndicatorDotPanel 是四点布局面板，没有独立 fragment。公共行为见 [overview](overview.md)，部件见 [semantic-part](semantic-part.md)。

SpinToken 使用 DependencyDefaultsV2 revision 1。每个默认只依赖表内 Global；下面的中间尺寸不是其他 Own 字段，单独覆盖 IndicatorSize 不会重算 DotSize。

| Own | 默认公式 | 消费 |
| --- | --- | --- |
| IndicatorSize | ControlHeightLG / 2 + 2 | Middle/Custom 外框 |
| IndicatorSizeSM | ControlHeightLG × 0.35 + 1 | Small 外框 |
| IndicatorSizeLG | ControlHeight + 4 | Large 外框 |
| DotSize | ((ControlHeightLG / 2 − UniformlyMarginXXS / 2) / 2) × 0.75 | Middle/Custom 点径 |
| DotSizeSM | ((ControlHeightLG × 0.35 − UniformlyMarginXXS / 2) / 2) × 0.75 | Small 点径 |
| DotSizeLG | ((ControlHeight − UniformlyMarginXXS) / 2) × 0.75 | Large 点径 |
| IndicatorDuration | MotionDurationSlow × 4 | 两公开控件 MotionDuration |

四算法原默认：普通/暗为外框 SM/M/L = 15/22/36、点径 = 4.5/6.75/10.5；Compact/暗Compact 为 13.25/19.5/32、3.84375/5.8125/9。四种时长均1200ms。单位为 DIP、TimeSpan；不由名称推导额外 Own 比例或限制。

Spin 的局部 Global FontFamily/FontSize 通过可选 ControlTheme 输入进入根与真实 Tip；未明确配置时保留实际字体继承，原生字体值优先。ColorPrimary 经 Spin 模板进入 Tip 和组合 Indicator，ColorBgMask 进入启用的 Mask，SpacingXXS 进入 section。独立 SpinIndicator 的 ColorPrimary 取自身有效 Global。原语义角色保持。

SizeType=Custom 默认使用 Middle 指标；原生 IndicatorSize、DotSize 和 MotionDuration 优先。点径0沿原语义可见性，不转换为业务开关；实际合成器时长小于1ms时保留原1ms防护。IsSpinning、Tip、IsTipVisible、CustomIndicator、遮罩选项都是实例状态，不进入 Token。

Semantic 定制只用生成的专用 AXAML Style，不能改写节点属性替代 Style。定制只覆盖目标视觉，不改变 Owner、Own 身份或持续动画状态。平台、全域极值与完整家族验收以限定结果记录为准，不因默认迁移完成而标通过。
