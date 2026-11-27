# 主题切换动效收束：设计、原型与 Gallery 实测

日期：2026-11-12。正式合同见[主题提交动效收束](../../architecture/systems/motion/theme-commit-settle.md)，
验收摘要见[Motion 验证状态](../../architecture/systems/motion/verification.md#2026-11-12主题提交动效收束)。

## 判定

主题切换时由资源变化间接启动的过渡会逐帧插值约 200ms，既浪费 UI 线程工作，又造成新旧主题混合帧。
最终方案在全局动画时钟的观察者层收束主题提交窗口内新启动的动画，使其在启动帧的提交前到达终值。
Gallery Release 实测每次切换 UI 线程总工作从 78.4ms 降至 24.7ms，提交本身增加约 1.6ms，窗口外无开销。

工作树基线 `release/6.0` `3a9339ddf`。产品改动未提交：

- 新增 `src/AtomUI.Core/Animations/MotionSettle.cs`。
- `src/AtomUI.Core/Theme/ThemeManager.cs`：三个提交点开启和结束窗口。
- `src/AtomUI.Core/Reflection/GlobalClockReflection.cs`：读取观察者列表与当前帧时间。
- 新增 `tests/AtomUI.Core.Tests/Animations/MotionSettleTests.cs`。

## 现状分析

- `ThemeManager.Publish` 在 UI 线程同步设置 `RequestedThemeVariant`、发布根与局部 context 的资源通知并派发 `ThemeChanged`，
  过程中没有任何运动管控。
- 主题 AXAML 在 `[IsMotionEnabled=True]` 下声明过渡：82 个文件中有 111 个 `SolidColorBrushTransition`、
  38 个 `DoubleTransition`、17 个 `TransformOperationsTransition`，以及 BoxShadows、Rect、Thickness、Point、Size 过渡。
- `Animatable.OnPropertyChangedCore`（12.1.5 反编译核对）对优先级高于 Animation 的基值变化，先释放同属性旧实例，
  再以 `Clock ?? IGlobalClock` 调用 `transition.Apply`。每个过渡创建 `TransitionInstance`、时钟订阅和 Animation 优先级绑定。
- 已有机制不适用：`TransitionSuppressionScope` 逐控件 O(n)；`DisableTransitions` 需反射遍历全部 Animatable；
  切换 `EnableMotion` Token 会触发两轮全量样式与资源变化；`MotionGate` 只管持续动画。

## 方案演进

1. **重新实现 `ITransition`**：反射确认 `TransitionBase.Apply` 与 `Transition<T>.DoTransition` 均为 internal，
   只能在子类重新实现接口，再用反射委托回调基类。只覆盖 AtomUI 自有类型，需改约 60 处 AXAML 原生过渡并加守卫测试。放弃。
2. **继承的自定义时钟**：`Animatable.ClockProperty` 可继承，`Animator<T>.Run` 也使用 `control.Clock`，
   设想在 TopLevel 样式中注入可“快进”的 `IClock`。原型编译失败：12.1.5 中 `IClock` 和 `ClockProperty` 均为 internal；
   Avalonia 全部程序集强签名，`InternalsVisibleTo` 无法借用；Gallery 支持 NativeAOT，DispatchProxy/Emit 不可用。放弃。
3. **全局时钟观察者收束（采用）**：`MediaContext` 内部时钟的 `_observers` 是所有时间驱动动画的汇合点。
   窗口开启时记录已有观察者，对新观察者推送 `T` 与 `T + 1 天` 两次脉冲完成动画。只读两个字段，不实现内部接口。

`MediaContext.RenderCore` 反编译确认顺序：`Pulse` → 渲染回调循环（布局）→ 有新订阅时 `PulseNewSubscriptions` →
再回调 → 提交合成。布局中启动的动画可在同帧被补发脉冲，这是同帧收束的依据。

## 实现中发现并修正的问题

| 问题 | 现象与根因 | 处理 |
| --- | --- | --- |
| 窗口提前关闭 | Publish 后直接投递 Loaded 释放，会早于下一帧执行：渲染由计时器触发，不在 Dispatcher 队列中 | 帧开始探针收到脉冲后再投递 Loaded，释放排在该次渲染任务之后 |
| 启动提交误伤 | 启动提交开窗后要等首帧才关闭；`DialogLifecycleTests.IsOpen_False_During_Opening_Cannot_Reopen_A_Stale_Session` 中 Dialog 打开动效被收束，`openedCount` 为 1 | 仅在应用初始化后开启窗口 |
| 无时钟环境失败 | `ThemeManagerTransactionTests` 等在无全局时钟的环境下打开窗口抛异常，事务返回 Failed | 时钟缺失时不收束；反射成员缺失、类型不符记录 Warning 后退化 |
| 测试绑定类型 | Token 资源是 `ImmutableSolidColorBrush` 而非 `Color`，`Color` 属性取到默认值 | 测试改用 `IBrush` 属性与 `SolidColorBrushTransition`，通过 `TokenResourceBinder` 绑定 |

## 自动化验证

`MotionSettleTests` 3 项：

1. 真实 `ApplyThemeAsync` 改 ColorPrimary 后，任何帧之前画刷过渡、`ThemeChanged` 本地值过渡和关键帧 Width 均为终值；
   首帧布局与布局触发的过渡同帧到位；窗口结束后新过渡保持 10s 时长。
2. 只收束窗口内启动的动画：窗口前启动的过渡保持中间值，窗口内 `FadeOutMotion` 立即完成，窗口在下一帧后关闭。
3. 布局中启动的过渡在一次强制渲染后到位；窗口外同样变化正常渐变。

对照：把开启条件临时改为 false，用例 1 在第一个断言失败（颜色为旧值 `#1677ff`），确认测试测到机制本身。

回归：Core 369/369、Controls 222/222、Desktop 4302/4302、Gallery 主题相关 17/17。

## Gallery 实测

方法：Release Gallery，经 `launch-gallery-debug.sh --configuration Release --no-build` 启动并确认 PID 与当前 checkout 二进制一致。
临时探针打开 Button 页（约 1754 个视觉元素），按 Gallery 暗色开关的请求形状（当前主题加/减 Dark 算法）自动切换。
记录切换调用耗时、`ThemeChanged` 时刻（Publish 结束）、收束自身耗时与收束数量、新增时钟观察者存活数，
以及切换后 1s 内每帧 UI 线程工作（时钟脉冲到其后首个 Loaded 任务）。探针订阅全局时钟以保持连续帧，两种模式条件相同。

第一轮两种模式分进程运行，开启收束的亮色切换 Publish 看似慢 5–8ms。第二轮改为同一进程按切换对交替开关收束，
Publish 两侧一致，确认前者为进程间差异。最终数据（同进程交替，各 20 次，中位数）：

| 组 | 调用 | Publish | 收束 | 帧工作合计 | 单帧最大 | >1ms 帧 | 启动动画 | 1 帧后存活 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 关闭/暗 | 17.20 | 17.18 | 0 | 59.84 | 9.20 | 14 | 201 | 198 |
| 关闭/亮 | 17.36 | 17.33 | 0 | 58.82 | 8.86 | 14 | 201 | 198 |
| 开启/暗 | 19.20 | 17.45 | 1.60 | 3.21 | 2.96 | 1 | 已收束 | 0 |
| 开启/亮 | 19.19 | 17.61 | 1.66 | 3.25 | 2.97 | 1 | 已收束 | 0 |

单位 ms。每次切换收束 204 个动画；UI 线程总工作（调用加帧工作）中位数：开启 24.72ms（p90 28.76），关闭 78.43ms（p90 80.61）。
无切换的空闲 60 帧两侧均为约 0.32ms。收束 1.6ms 中包含 204 个动画到达终值的计算，关闭时同样的计算分摊在约 12 帧中。

截图：切换到亮色后 50ms 用 `RenderTargetBitmap` 截取窗口。关闭收束时 Default/Dashed 按钮仍为深灰、导航与 Text 按钮文字半透明，
与开启时相比 6.2% 像素差异超过 8，通道最大差 179；开启时已是完整亮色主题。暗色方向的截图恰逢主题已为暗色，属于 NoOp，不作对比。
截图与原始 CSV 留在会话临时目录，不进入仓库。

该机器两种模式单帧均未超过 16.7ms，结论限于 UI 线程工作与视觉一致性，不宣称帧率提升；帧工作口径不是显示器 present。

## 清理

临时探针 `ThemeSwitchProbe.cs`、`GalleryApplication` 挂接、`ThemeManager` 环境变量开关和 `MotionSettle` 计数器均已删除，
检索确认无残留；清理后 Core 369/369 通过。

## 未完成与后续

- NativeAOT 发布下 `MediaContextClock` 两个字段经 `DynamicDependency` 的保留尚未验证。
- Windows、Linux 原生测量与更多页面规模未覆盖。
- 待决定提交与代码审查。
- 旁注（未确认影响，与本改动无关）：`ThemeManager` 的 `_rootContextStyle` 使用 `OfType<TopLevel>()`，只匹配精确类型；
  headless 测试中普通 `Window` 的 `ThemeScope.ContextProperty` 为 null，需另行确认是否符合设计。
