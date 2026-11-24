# 主题提交动效收束

本文拥有主题提交期间动画的处理合同：一次主题提交启动的过渡、关键帧动画和 Motion 播放在启动所在的帧内直接到达终值，
新主题从第一帧起整体呈现。机制属于[Motion 系统](overview.md)的时钟层，由[主题运行时](../theming/runtime.md#12-事务与发布顺序)
的提交点开启。源码为 `AtomUI.Core/Animations/MotionSettle.cs`，验证数据见[验证状态](verification.md#2026-11-12主题提交动效收束)，
过程记录见[开发日志](../../../superpowers/progress/2026-11-12-theme-switch-motion-settle.md)。

## 1. 问题与目标

主题 Publish 在 UI 线程同步发布 variant、资源通知和 `ThemeChanged`。每个绑定 Token 的属性都在 Style 优先级上改值，
Avalonia `Animatable` 因此为带 `Transitions` 的属性各启动一个过渡实例，并在 `MotionDurationMid` 期间逐帧插值。
Gallery Button 页一次明暗切换启动约 200 个过渡，切换后约 12 帧持续插值和重绘。
带过渡与不带过渡的控件一部分渐变、一部分立即变化，同一帧出现新旧主题混合。

这些过渡由主题替换间接触发，不表达任何交互状态变化。目标：

- 提交启动的动画在其启动帧的布局与提交前到达终值，首个呈现帧即为完整新主题。
- 不改变 Token、ControlTheme、`Transitions` 声明或控件状态；窗口结束后过渡照常工作。
- 窗口外零成本；不遍历控件树，不按控件或过渡类型逐个接入。

## 2. 不变量

- 只处理窗口开启后**新订阅**全局动画时钟的观察者；窗口开启前已在运行的动画保持自己的时间。
- 收束让动画按自身语义走到终点（`FillMode`、终值、完成事件不变），不取消、不清除属性、不改写绑定。
- 窗口覆盖提交、Publish、事件回调，以及随后第一帧的布局和提交；该帧渲染任务结束后立即关闭。
- 收束是优化：框架内部成员缺失或时钟不可用时退化为不收束，动画照常播放，主题提交结果不受影响。
- 启动时的初始主题提交不开启窗口。

## 3. 拦截点

Avalonia 12 中三类时间驱动动画最终都订阅同一个渲染循环时钟（`MediaContext` 内部的 `IGlobalClock` 实现）：

| 动画 | 时钟来源 |
| --- | --- |
| `Transitions`（含 AtomUI 自有与第三方类型） | `Animatable` 调用 `transition.Apply(this, Clock ?? IGlobalClock, …)`，`TransitionInstance` 再订阅该时钟 |
| 关键帧 `Animation` / `Style.Animations` | `Animator<T>.Run` 使用 `clock ?? control.Clock ?? Clock.GlobalClock` |
| AtomUI `MotionFrameDriver` | 直接订阅全局时钟，见[帧流水线](frame-pipeline.md#1-帧驱动) |

全局时钟的观察者列表是覆盖全部类型的唯一公共汇合点。被否决的方案：

| 方案 | 否决原因 |
| --- | --- |
| 临时把 `EnableMotion` Token 置为 false | 额外两轮全量资源通知和样式切换，成本高于问题本身；持续动画会被停止重启 |
| 逐控件 `TransitionSuppressionScope` / `DisableTransitions` | 需要遍历所有 TopLevel、Popup 与非视觉 Animatable，O(n) 且覆盖不全 |
| 子类重新实现 `ITransition.Apply` | `TransitionBase.Apply` 为 internal；只能覆盖 AtomUI 自有类型，需替换全部 AXAML 中的原生过渡 |
| 继承的自定义 `Animatable.Clock` | `IClock` 与 `ClockProperty` 在 Avalonia 12 为 internal，程序集强签名无法实现；DispatchProxy / Emit 与 NativeAOT 不兼容 |

## 4. 机制

### 4.1 窗口

`MotionSettle` 由 `ThemeManager` 持有，一个应用一个实例。`Begin()` 返回可嵌套作用域；最外层开启时：

1. 解析全局时钟，读取其观察者列表，复制为“已知集合”（按引用比较）。
2. 订阅一个帧开始探针，并把探针加入已知集合。订阅本身会安排一帧，场景未失效时同样有帧可用。

最外层作用域释放时退订探针并丢弃已知集合。窗口只持有观察者引用，生存期约一帧。

### 4.2 收束动作

对每个不在已知集合中的观察者，依次推送当前帧时间 `T` 和 `T + 1 天`，并把它加入已知集合：

- 时钟消费者以收到的首个脉冲作为本地零点，第二个脉冲是单调前进的大跨度时间，与一次很晚到达的帧等价。
  有限过渡、关键帧动画和 Motion 在这一脉冲内到达终点并结束。
- `ClockBase` 按脉冲差值累计时间；收束后的无限动画只多出一次远端采样，随后按真实时钟继续，只表现为相位偏移。
- 收束产生的属性变化可能触发新的过渡，因此重复扫描，直到没有新观察者，上限 16 轮。

### 4.3 收束时机

| 时机 | 处理对象 |
| --- | --- |
| Publish 及 `ThemeChanged` 完成后，提交方法返回前（同步） | 资源通知、variant 变化和事件回调启动的动画；此时尚未渲染任何帧 |
| 下一帧开始（帧开始探针收到脉冲，布局前） | 提交之后、该帧之前由投递任务启动的动画 |
| 同一帧布局之后、提交之前 | 布局中启动的动画，例如由新尺寸驱动的指示器过渡 |

第三个时机利用渲染循环的顺序：`RenderCore` 先脉冲全部观察者，再循环执行渲染回调（布局）；本轮出现新订阅时
调用 `PulseNewSubscriptions` 补发同帧脉冲，然后再次布局，最后提交合成。帧开始探针在脉冲中订阅一次性的布局后探针，
它属于本帧新订阅，因而在布局之后、提交之前被调用，并收束布局期间启动的观察者。收束引起的布局失效在下一轮回调中完成，
仍早于本帧提交。

### 4.4 窗口结束

`ThemeManager` 在提交方法的 `finally` 中先执行一次同步收束，再调用 `EndAfterNextFrame(scope)`：作用域在帧开始探针下次收到脉冲时，
以 `DispatcherPriority.Loaded` 投递释放。渲染由计时器驱动，而不是排在 Dispatcher 队列中，直接投递的 Loaded 任务会早于下一帧执行。
从脉冲内投递则排在该次渲染任务之后执行，并先于此后排队的输入。提交结束到下一帧之间（至多约一个刷新周期）
Dispatcher 仍可能处理输入，此间启动的交互动画同样会被收束；这是窗口覆盖首帧布局的代价，不再进一步区分来源。

## 5. 接入点

| 提交点 | 开启位置 | 结束位置 |
| --- | --- | --- |
| 根主题事务 `ExecuteTransactionCoreAsync`（用户切换、跟随系统） | CommitCore 前 | `finally`，覆盖 Publish 异常 |
| 目录刷新 `ProcessCatalogReloadAsync` | snapshot 变化时的 CommitCore 前 | `finally` |
| 局部作用域 `ApplyScopeConfig` | 局部 Commit 前 | `finally`，包围 Publish |

开启条件：应用已初始化，且当前在 UI 线程。启动提交没有可收束的界面；若开启，窗口会持续到首帧，把首批窗口的入场动画一并结束。
NoOp、Superseded 和 CommitCore 前失败的事务不开启窗口。局部作用域提交使用同一全局窗口，期间全局新启动的动画都会被收束；
窗口约一帧，不按子树区分。

## 6. 覆盖范围与边界

| 内容 | 处理 |
| --- | --- |
| 任意 `Transitions` 类型，应用与第三方控件 | 覆盖，前提是使用全局时钟 |
| 关键帧 `Animation`、`Style.Animations`、AtomUI Motion | 覆盖 |
| 窗口开启前已在运行的动画（例如正在关闭的菜单） | 不处理，按原时长完成 |
| 同一属性上被主题改值的在途过渡 | `Animatable` 先释放旧实例，替代实例被收束，直接到达新值 |
| 显式指定自定义时钟的动画 | 不覆盖 |
| DispatcherTimer 或合成层驱动的持续动画（Spin、Marquee 等） | 不经过全局时钟，不处理；这些动画不由主题提交启动 |

## 7. 成本模型

- 窗口外：没有订阅、钩子或分支，进入窗口只发生在主题提交点。
- 窗口内：开启时复制一次观察者列表（只含运行中的动画，通常很少）；每个时机扫描一次列表；
  每个新动画两次脉冲，这部分本来就要在后续帧中逐帧执行。
- Gallery Release 实测（Button 页，同进程交替 20 次）：提交增加约 1.6ms，其中包含 204 个动画到达终值的计算；
  切换后的帧工作从约 59ms 降至约 3.2ms，每次切换 UI 线程总工作从 78.4ms 降至 24.7ms；空闲帧开销相同。

## 8. 框架依赖与 AOT

`Reflection/GlobalClockReflection` 按名称读取 `Avalonia.Media.MediaContext+MediaContextClock` 的
`_observers`（`List<IObserver<TimeSpan>>`）和 `_currentAnimationTimestamp`，并以 `DynamicDependency` 声明保留。
每个字段在同一个延迟工厂内解析常量类型名并查找成员，裁剪器可沿该类型值分析字段访问。工厂缓存 `FieldInfo`。
全局时钟服务未注册时跳过收束。首次读取观察者列表遇到成员缺失或类型不符时，记录 Warning 并让动画正常播放。

升级 Avalonia 时逐项核对：

- 上述两个字段的名称、类型和语义。
- `RenderCore` 顺序：脉冲 → 渲染回调（布局）→ 新订阅补发脉冲 → 提交。
- `TransitionInstance`、`ClockBase` 与 `AnimationInstance` 以首个脉冲为本地零点并按差值累计时间。
- `Animatable` 与 `Animator<T>` 仍以 `Clock ?? GlobalClock` 解析时钟。

`MotionSettleTests` 锁定提交首帧终值、布局同帧收束和窗口外正常过渡，升级后若框架顺序变化会直接失败。
NativeAOT 发布下的字段保留须在发布验证中单独确认。

## 9. 验证

- `tests/AtomUI.Core.Tests/Animations/MotionSettleTests.cs`：
  - 真实 `ApplyThemeAsync` 后、任何帧之前，Token 驱动的画刷过渡、`ThemeChanged` 中的本地值过渡和关键帧动画都已到达终值；
    首帧布局新尺寸，布局触发的过渡同帧到位；窗口结束后新过渡保持时长。
  - 只收束窗口内启动的动画；窗口开启前的过渡不受影响；Motion 播放立即完成。
  - 布局中启动的过渡在同一帧到位；窗口外同样的布局变化正常过渡。
- 对照：关闭收束时，主题切换用例在第一个断言失败（颜色仍为旧主题值）。
- 原生证据、截图与限制见[验证状态](verification.md#2026-11-12主题提交动效收束)。
