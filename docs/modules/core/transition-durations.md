# 主题 Transition 时长消费

内部 `TokenTransitionsExtension` 用于 ControlTheme 的 `Transitions` style Setter。Transition 本身不是资源宿主；`SharedTokenResourceValue` 独立使用时仍是应用级字面值，不能据此宣称局部主题会动态更新。

## 归属与声明

集合在实际 Control 上使用原生 MultiBinding。每种时长输入通过内部 `OwnerTokenResourceExtension` 独立订阅有效 ThemeContext 和原生资源；优先使用实际类型或 StyleKey 的规范 Control identity，内部节点优先沿TemplatedParent，Content子树没有模板父时沿逻辑Parent找到登记owner；自身父/模板父、逻辑挂载和上下文变化会重新解析并去重。无登记owner的独立基础节点使用Shared键。控件专属 Global 覆盖不会借用另一同族 identity。

`DurationKind` 指定集合公共时长；未指定时保留各项 `SharedTokenResourceValue` 的时长键，允许 Mid/Fast 混合。没有动态声明的静态 Duration 保持原值。该元数据只描述输入，不参与优先级。

```xml
<Setter Property="Transitions">
    <atom:TokenTransitions DurationKind="MotionDurationMid">
        <atom:SolidColorBrushTransition Property="Background"/>
    </atom:TokenTransitions>
</Setter>
```

## 对象和生命周期

每个实际 Control 持有独立原生 Transitions 及 Transition；主题变化更新 Duration，保持对象身份与模板、焦点和输入状态。Style 定义通过 ConditionalWeakTable 使用弱 owner 键，不通过全局订阅保留已释放的控件。显式本地 Transitions 覆盖和 ClearValue 的恢复继续由 Avalonia 原生优先级处理。

工厂使用有限的显式构造：AtomUI SolidColorBrush、BoxShadows、Rect，以及 Avalonia Double、TransformOperations、Point、Size、Thickness Transition。复制 Property、Delay、Duration、Easing；可变 Spline/Spring easing 经既有有限 Token copy 每owner独立复制，已知无状态类型可共享，未知类型拒绝；未知或派生类型明确拒绝，禁止反射创建。增加类型必须核对实际消费者、AOT 与默认时序。

## 当前迁移边界

已接入 Button、DropdownButtonBase、共享 InputControlFrame 与 RadioIndicator；实际类型、模板 owner、混合时长、作用域往返、本地覆盖及对象释放已验证。其余 literal 消费者仍按所属家族逐条迁移并取得默认视觉对照，不能批量替换后宣告全控件完成。初始化 StateMotion 的 Animation 优先级抑制保留，不能为观察方便移除。

本轮默认明暗、尺寸、禁用、输入错误以及悬停/按下终态均保持零像素差异。动态时长的属性值验收与真实动画时间线验收分别记录；此文不表示所有平台或时钟后端已通过。实施结果见[CT-11 结果](../../superpowers/plans/2026-10-22-custom-theme/motion-transitions-results.md)。

## 波纹 Global 输入

`WaveSizeMotionDuration` 与 `WaveOpacityMotionDuration` 是 Alias，默认分别取 MotionDurationSlow 和正 Slow+50ms；Slow为0则二者为0。两项精确覆盖直接进入 painter，EnableMotion=false 在最终归一化阶段覆盖为0。`WaveSizeEasingCurve` / `WaveOpacityEasingCurve` 是 Seed，默认 CubicEaseOut，沿现有 Easing literal/schema/AOT 构造合同。共享 Wave 主题使用同一 OwnerTokenResource 绑定解析其实际模板 owner，不新增自身 identity。
