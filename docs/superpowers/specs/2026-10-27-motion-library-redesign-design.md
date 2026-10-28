# AtomUI Motion 动效库重设计方案

日期：2026-10-27。状态：重构方向已采纳；本文保留方案决策与实施阶段记录，产品实现尚未完成。
正式合同现由 [Motion 系统架构](../../architecture/systems/motion/overview.md)拥有，当前差异由其验证页维护。

本方案将 Motion 重建为可继承的动效定义，统一单次播放协议，取消 Actor 执行前提，并以真实 Gallery 场景验证性能。
Ant Design 是内置动效的规格依据，核心类型采用通用命名。旧 Motion API 不作为兼容约束；控件布局、输入与业务合同仍需保持。
已完成的 NavMenu 延时驱动移除继续保留，本方案不把既有 P0/P1 工作重新列为未完成项。

## 1 设计决策与边界

用户已明确的方向：`Motion` 抽象基类、具体动效子类、允许第三方扩展、取消 Actor 依赖、以稳定 60 FPS 为性能底线。
本文提出的参数、结果类型、异常和性能阈值属于待 review 的具体设计，不表示已经实现或测得。

| 路线 | 优点 | 代价与结论 |
| --- | --- | --- |
| 保留 Actor 并精简内部 | 迁移量较小 | 强制宿主与职责耦合仍在，不符合此次重设计方向 |
| 统一 Motion 播放协议，使用原生执行与专用布局能力 | 定义可扩展，能按成本选择路径 | 推荐；须验证合成执行的完成、中断与命中能力 |
| 自建全量动画引擎 | 控制范围大 | 重复实现调度与执行基础设施，当前缺少必要性证据 |

范围包括入场、离场、有限关键帧、内容展开与必要的组合。普通 hover/checked/颜色过渡继续由 ControlTheme 声明；
持续旋转、自绘过程反馈保留适合自身的原生路径，不为统一命名而全部搬入显式 Motion。
不以缩短动画、改变 Push/Overlay 行为、取消阴影或隐藏复杂内容作为默认性能优化。

## 2 公开模型与调用方式

```text
Motion
├── FadeInMotion / FadeOutMotion
├── ZoomInMotion / ZoomOutMotion 及所需 big、方向变体
├── SlideUpInMotion / SlideUpOutMotion 等四方向变体
├── MoveUpInMotion / MoveUpOutMotion 等四方向变体
├── ExpandMotion / CollapseMotion
├── SequenceMotion / ParallelMotion
└── 第三方自定义 Motion 子类
```

具体方向实现可以共享内部轨迹构建逻辑，无须复制播放、取消与清理代码。

```csharp
var motion = new ZoomInMotion
{
    Duration = TimeSpan.FromMilliseconds(300)
};
var result = await motion.PlayAsync(surface, cancellationToken);
```

公共核心是 `Motion`；扩展所需辅助契约为 `MotionBuilder`、只读 `MotionContext`，以及表达结果的 `MotionResult`/`MotionOutcome`。
不公开执行器注册中心，不要求应用选择后端，不要求目标继承专用动画控件。
拟定公共命名空间为 `AtomUI.Animations`，实现继续位于 AtomUI.Core；AXAML 统一使用 `xmlns:atom="https://atomui.net"`。
该 XML 命名空间已有对 AtomUI.Animations 的映射，新类型沿用 `atom:Motion`、`atom:ZoomInMotion` 等写法。

`PlayAsync` 是基类非 virtual 入口，返回 `Task<MotionResult>`。子类实现受保护的
`Build(MotionBuilder builder, in MotionContext context)`，只声明轨迹。
基础配置包括可选 Duration/Easing、Delay、Origin 和 FillMode，默认值由具体动效与有效 Token 决定。

为支持普通 AXAML 对象配置，Motion 是可配置的 CLR 定义对象；每次准备产生不可变运行快照。
配置与准备发生在目标 UI 线程，播放中的配置变更只影响下一次请求。第三方也不得把目标和运行资源存入定义对象。
外部取消令牌可以从其他线程发出信号，属性释放和视觉操作仍归目标 UI 线程执行。

参数优先级为本次控件请求覆盖、Motion 显式配置、具体动效默认 Token/参数。
控件请求覆盖使用内部接入边界，不修改共享 Motion 对象；第三方可以直接配置实例参数。
Token 绑定保留在控件/ControlTheme 的资源宿主上，不在普通 Motion 对象上建立永久资源订阅。

## 3 目标、主题与 Actor 替代

目标使用真实 `Control`，例如 Border、Panel、ContentPresenter。纯视觉动效优先复用已有模板节点。
目标适配在内部完成；不把 Actor 换名后再次要求所有目标继承一个 ContentControl。

| 旧 Actor 职责 | 新归属 |
| --- | --- |
| 内容承载 | 真实控件模板与 ContentPresenter |
| 双 transform、矩阵复制 | 类型化动画通道与单次投影 |
| ActiveMotionRun、Animating | 当前执行及其目标/owner 关联 |
| 完成路由事件 | 播放结果；业务事件由业务控件拥有 |
| Measure/Arrange | 独立布局能力 |
| 默认裁剪 | 模板明确声明的视口与阴影边界 |
| Follow 多路绑定 | 多目标共享时间轴，按实际需要接入 |
| Popup 首帧预隐藏 | Popup 宿主显示前的准备流程 |

普通目标不增加强制包装节点。独立变换、裁剪或布局确实需要边界时，由 ControlTheme 静态提供并明确用途。
不在播放时重挂载内容或改造逻辑/视觉父子关系。缺少能力时准备阶段失败，不静默改变几何或交互。
纯视觉效果不统一强制 ClipToBounds；阴影、溢出、装饰层和滚动各自保留真实裁剪合同。

AXAML 能等价表达的模板/状态关系继续使用 ControlTheme；Transitions 放在 style setter，不直接放进模板内容。
迁移时同步校准 Template Part、Semantic Part 专用 Style 路由和资源归属，不能以代码定制回退掩盖样式失效。

## 4 第三方扩展合同

第三方与内置子类使用相同构建入口。以下为拟定用法，辅助方法签名在能力原型后冻结。

```csharp
public sealed class RiseInMotion : Motion
{
    public double DistanceRatio { get; set; } = 0.08;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Opacity.From(0).To(1);
        builder.TranslateY.From(context.TargetSize.Height * DistanceRatio).To(0);
        builder.Scale.From(0.96).To(1);
    }
}
```

Context 只提供本次有效几何、缩放、参数和必要的初始值快照，不把可随意改写的目标控件传入 Build。
Builder 在准备阶段使用，完成后失效；不能存到字段中。Build 同步执行，不启动任务、订阅或帧循环。
需要重新解析几何时重新准备对应请求，不逐帧重复创建关键帧配置。

| 扩展范围 | 合同 |
| --- | --- |
| 内置视觉通道 | 声明 opacity、translation、scale、rotation、origin 等轨迹，后端由库选择 |
| 自定义曲线 | 可重入纯计算；允许合法超调，非法数值在执行前/求值时有明确错误路径 |
| 自定义属性 | 强类型属性句柄、显式插值器，验证目标适用性；按具体属性匹配，不抢占宽泛全局类型 |
| 布局扩展 | 声明自然内容、占位和视口的专门合同；参与真实布局与输入验收 |

标准扩展可复用优化路径；自定义任意插值和布局不会自动获得同等性能保证，诊断必须说明实际执行路径与成本。
第三方以普通 .NET/NuGet 库分发，通过直接构造和编译 AXAML 使用。普通子类无需程序集扫描或全局发现。
属性适配优先局部显式指定；不得重现 ITransform 宽泛注册遮蔽 TransformOperations 精确处理器的问题。

## 5 轨迹、曲线与组合

轨迹由类型化通道、关键帧、分段曲线、时长、Delay、原点及端点策略组成。
准备阶段校验有限数值、合法时间范围、通道适用性和同一时间段的写入冲突，再产生执行数据。
目标几何和资源相关快照不跨控件缓存；不可变曲线/固定参数可复用，有界缓存不持有控件。
位移单位显式区分 DIP 与目标参考尺寸的比例；百分比不误用父元素尺寸。变换顺序与 origin 一起进入规格对照。

CSS cubic-bezier 先解 `BezierX(u)=t` 再计算 `BezierY(u)`。时间归一化限制在合法范围，缓动输出不统一钳到 0–1，
属性自身的合法范围单独处理。框架同名 Circular/Quint/Cubic 函数不能代替指定 CSS 控制点。
优先验证现有 Spline 实现是否满足误差要求，需要时再实现有括区间保护的求解；不提前引入大规模采样表。

SequenceMotion 将子段放入同一时间轴，并明确各段起止、间隔和中性值从上一段已解析终点继承的规则。
ParallelMotion 使用相同起始时间，按子段自身时长结束，整体时长取最长覆盖范围。
父级 Duration 若显式指定，按比例缩放整个组合时间轴；父级 Delay 只应用一次，子段局部 Delay 保留。
子段保留自身曲线，父级不再次施加全局非线性 easing；不适用的组合参数应在准备时拒绝。
同一目标同一通道在并行重叠区间重复写入时报错，不采用最后写入获胜。
组合只建立一次请求所有权，内部不嵌套调用子项 PlayAsync；否则会造成相互替代。

多目标同步是组合时间轴的进一步能力，仅在实际消费者需要时接入；以同一时间基准提交，不通过 Actor Follow 同步属性。
首版不默认引入弹簧、物理引擎、FLIP 或额外后端插件系统。

## 6 单次播放与结果

生命周期为准备、取得本次请求所有权、提交、播放、稳定态提交、释放。没有变化的通道直接跳过。
准备阶段的配置错误发生在属性写入之前；途中异常进入同一释放协议。

| 结果 | 含义与 owner 行为 |
| --- | --- |
| Completed | 当前请求到达目标；零时长/禁用时可不播放，WasAnimated=false |
| Superseded | 新请求替代，不执行旧业务完成动作 |
| Cancelled | 外部取消；撤销当前执行覆盖，恢复最新底层基线，不默认停在中间帧 |
| Detached | 目标或 owner 生命周期失效，释放，不操作新模板或新宿主 |
| NotReady | 目标未挂载或尚未完成有效布局，未取得请求所有权；owner 可重新请求或按稳定态关闭 |

底层 PlayAsync 不等待布局：准备时未就绪立即返回 NotReady，且不替代目标上已有请求。不支持的通道则是明确异常。
有效几何可以包含合法零尺寸，不能仅以宽高大于零判断就绪。布局通道另外检查自然内容与视口能力。
控件接入层可在展示准备中安排一次布局完成后的核对，仍未就绪就走无动画稳定流程；不保留无限等待。
真实故障通过异常传播，fire-and-forget 接入口统一观测和记录，不吞掉故障。
结果包含请求标识；owner 在关闭、移除或销毁前重新核对当前请求、目标/模板身份和最新业务目标，不能仅检查 Completed。

完成只来自本次真正启动的有限通道的可靠完成结果，不用 `Ease(t)≈1` 判结束。
watchdog 只处理异常停滞，包含实际 Delay 与时长；触发记录为故障/超时，不冒充正常成功。
目标就绪时，零时长、禁用动效先短路并按端点合同提交稳定态，不建立播放实例、帧订阅或定时器。

FillMode 使用明确的标准语义：None 结束后恢复基线；Forward 持有终点；Backward 在 Delay 期间持有起点、结束后恢复；
Both 在 Delay 期间持有起点并在结束后持有终点。内置有限入/离场默认 Both，避免离场结束到 owner 隐藏之间闪回。
不填充 Delay 起点的模式在延时期间保留当前底层状态。外部取消一律恢复基线，不提交 Forward 终点；替代走下节交接。
Forward/Both 的终点转交给目标侧的终态记录，只保留呈现值和可撤销覆盖，不保留 Motion、CTS、任务、帧订阅或 watchdog。
终态记录由下一次成功请求、Motion.Reset(target)、目标 detach、模板/owner 失效释放。Reset 只在无活动播放时使用；
活动播放中调用 Reset 明确报错，调用者先取消并等待完成。底层绑定在持有期间继续更新，释放后恢复最新有效值。
None/Backward 模式的离场不提供终点持有；需要异步关闭宿主的 owner 应使用 Both/Forward，不依赖 await continuation 的时机。
资源验收分别统计活动运行与有意保留的终态覆盖：终态持有不是仍在播放，但在 Reset/detach 后必须归零。

## 7 替代、反向与属性所有权

每个目标/owner 关联当前请求，不建立进程级强引用控件注册表。首版一个目标只允许一个显式 Motion 请求。
新请求准备成功后取得身份、捕获可用呈现值并完成交接，再使旧请求整体 Superseded；不产生部分替代和剩余通道任务。
需要同目标并行效果时使用 ParallelMotion，内部通道仍归同一次请求。无冲突的主题状态属性可以继续独立工作，
但主题 Transition 与显式 Motion 不得同时争用同一有效属性。

相同目标端点的重复状态请求幂等，不重播；重复的一次性 PlayAsync 调用则表示显式新播放请求。
替代对新旧共有通道要求呈现值连续；旧请求独有通道恢复最新基线。跨家族未声明通道不保证连续，
需要完整连续性时须显式声明对应通道及终态或使用组合。速度连续不是通用保证，也不宣称 Ant 本身保证这一行为。
未中断轨迹按 Ant 校准，中断策略单独验收；不为提高 FPS 静默改变时长、轨迹或振幅。

属性覆盖必须可撤销。应用底层绑定、主题和集合在覆盖期间仍可更新，结束后恢复最新有效值，不写回旧快照。
应用 RenderTransform 与动效 delta 的组合顺序须明确；必要时使用模板中显式边界隔离，不替换用户 transform 后丢失更新。
下一请求不继承上一请求未声明的终态覆盖；准备成功后统一重建其呈现边界，避免 Forward 值在下一家族结束时重新出现。
取消/替代后呈现值如何保持及交接必须通过原生能力验证，不把客户端基值当成合成端当前值。

模板替换先使旧目标登记失效，再释放旧运行；取消、异常、正常完成和 owner 销毁共享同一资源释放入口。
资源成对覆盖目标关联、事件、订阅、动画句柄、临时属性值、布局策略和宿主引用；弱引用不能替代释放。
运行资源在结束时释放，转交给终态记录的呈现覆盖按第 6 节的 Reset/目标生命周期规则释放。

## 8 执行路径与框架能力门槛

| 动效职责 | 执行路径 | 必须满足 |
| --- | --- | --- |
| 纯 opacity、translate、scale 等 | 能力满足时使用 Composition | 完成、替代、终态、变换组合和命中均正确 |
| 合成能力不足的有限轨迹 | Avalonia 原生时钟/Animation | 可靠完成，统一请求所有权，不引入独立周期时钟 |
| 内容展开和真实占位变化 | 原生动画进度与专用布局能力 | 自然内容、真实视口、输入与父布局合同 |
| 普通状态样式 | ControlTheme 原生 Transition | 初始化基线、资源与绑定合同继续成立 |

选择路径发生在准备阶段，不向应用暴露后端开关。运行中不未经交接切换后端。
原生 Animation 优先直接消费适用轨迹；不能直接表达的组合须验证同一框架时钟下的数值推进方案。
不提前假定所有组合均能转换为一个 Animation，也不创建另一套 Task.Delay/DispatcherTimer 动画循环。

现有 Avalonia 12.1.4 的 RequestCommitAsync 表示修改已应用到渲染线程，不是动画结束或屏幕呈现完成。
Composition 的可靠完成、反向时当前呈现值、变换后命中与客户端基值协调必须先做能力原型。
只有通过合同的组合才进入合成路径；否则使用原生路径并同样接受性能门槛。
若必须新增框架公开能力，应单独提出依赖方案，不以运行时反射内部成员或正常计时器兜底冒充支持。

手工帧回调只用于确有需要的自绘/运行期计算，复用框架 RAF；有多个实际消费者时才按 TopLevel 合并。
空闲不请求帧，使用单调时间，不硬编码 16ms，不补投递陈旧更新；一次动画的通道使用同一时间基准。

## 9 布局能力与内容展开

ExpandMotion/CollapseMotion 使用真实内容占位和视口，不把一般矩阵布局算法作为展开基础。
Core 只依赖独立布局能力合同，具体承载原语位于 AtomUI.Controls，并在模板中静态声明。
该能力包含自然内容测量、期望占位、实际 Bounds/视口和内容排列；普通 Control 不默认具备这些能力。
高级第三方可以实现相同布局合同；缺少能力时准备期拒绝布局通道，不伪装成 RenderTransform 动画。
布局接口的精确签名和 AOT 可消费性在第一阶段验证并冻结，不将现有内部接口直接当成已开放能力。

内容保持自然尺寸；进度改变占位和真实视口，方向决定锚点，裁剪和命中使用相同几何。
按实际内容/约束失效重新计算目标，不能把每帧调用 Measure 等同每帧重测整个子树。
宽度变化引起换行时必须正确重排，缓存不能复用失效结果；动态内容变大时按当前呈现值及原截止时间重定向。
嵌套滚动、边界滚动链、可见滚动条、Margin/Min/Max、布局舍入、DPI 和阴影均参与验收。

TreeView 仍使用旧布局 Actor，须验证迁移后的展开方向、占位、子项命中和键盘行为，再删除旧算法。
新旧视觉规格不一致之处列为独立行为差异 review，不能归为纯性能变化。

## 10 显示隐藏与业务控件接入

拟定附加入口为 Motion.IsVisible、Motion.Enter、Motion.Leave；Enter/Leave 的类型均为 Motion。
该入口把目标可见性纳入其管理，应用不同时为同一目标建立另一条 IsVisible 控制来源。
它适用于应用节点显示隐藏，不替代 Popup/Dialog 物理宿主的业务生命周期。

```xml
<Border xmlns:atom="https://atomui.net"
        atom:Motion.IsVisible="{Binding IsOpen}">
    <atom:Motion.Enter>
        <atom:ZoomInMotion />
    </atom:Motion.Enter>
    <atom:Motion.Leave>
        <atom:ZoomOutMotion />
    </atom:Motion.Leave>
    <!-- 原有内容 -->
</Border>
```

首次附着默认直接建立当前状态，Appear 只有显式请求时播放；控件主动打开弹层仍按打开意图执行入场。
离场期间节点保留；完成后隐藏。中途反向核对当前请求，最新目标优先；卸载不等待无意义的离场。
输入策略由控件合同声明并测试，不能因合成方便一律禁用整个入场期间的点击。

Popup 在宿主显示前准备初始 opacity/transform，避免删除 PopupMotionActor 后引入首帧闪现。
Close 请求先保留物理宿主，当前离场完成且关闭意图仍有效时才执行 CloseCore；不可播放时按 owner 的稳定关闭流程收敛。
焦点、light-dismiss、遮罩、Open/Closed、Pending/Completing 及业务完成仍归 Popup/Dialog/Drawer。
BackTop、TreeView、FeedbackStack 各自的 guard/取消缺陷随接入统一协议修复，不能只替换类名而保留旧错误流程。

## 11 内置动效规格

内置版本基线采用本次已核验的 Ant Design 6.6.8；来源与逐文件对照见[源码审查](../progress/2026-10-27-motion-scene-ant-design-audit.md)。
后续版本升级需按变更复核，不自动跟随上游静默改变外观。

| 家族或消费者 | 对齐目标 |
| --- | --- |
| Fade | opacity 0↔1、linear，默认 Mid |
| Slide | scaleX/Y .8↔1，方向原点正确；进入 OutQuint、离开 InQuint 的原始贝塞尔参数 |
| Move | 两端点纯平移 ±100% 与 opacity；移除额外中段及 UpIn 单方向缩放 |
| Zoom | 普通 .2、big/方向 .8 的对应端点；入 OutCirc、出 InOutCirc |
| 内容 collapse | 自然内容的 height/opacity 与 InOut；实际组件时长按来源校准 |
| Select | 对照实际 slide-up 选择，不统一改为 Zoom |
| Modal | 对照普通 zoom、实际 origin 与入场 300ms/离场 200ms |
| Drawer | 对照其独立的平移、opacity 与曲线，不套用通用 Move 参数 |

Token 的 Fast/Mid/Slow 和原始 motionEase 参数集中维护；组件选择、时长覆盖和原点在对应主题/控件拥有。
默认视觉变化属于规格校准，和保持外观的性能优化分别记录、分别 review。

## 12 性能预算与验收

60Hz 显示器目标贴合约 60 次/秒刷新；120/144Hz 设备分别按 8.33/6.94ms 预算设计，不在库中限帧到 60。
60Hz 每周期为 16.67ms。UI 与渲染流水线需分别满足截止时间，不能只用 Motion 耗时代表整帧成本。
性能承诺绑定最低硬件、系统、渲染后端、DPI/分辨率和负载；这些验收参数在实现前冻结，当前尚未取得用户指定的最低配置。

| 项目 | 拟定目标，不是实测结果 |
| --- | --- |
| Motion 自身运行管理 | 20 个内置视觉动画并发时，P99 ≤ .5ms/帧；框架插值、布局、绘制和 GPU 成本另列 |
| 稳态主动分配 | Motion 自身 0B/帧目标；框架及总分配、GC 同时报告，不能藏掉整进程成本 |
| 额外逐帧 Task/Dispatcher 投递 | 0 |
| 纯视觉效果主动使布局失效 | 0 |
| 空闲调度 | 0 个持续帧回调/周期 timer |
| 全场景呈现 | 贴合目标刷新率；拟定漏刷新周期比例 ≤ .5%，无连续三周期丢失，参考负载中无 ≥50ms 长间隔 |

漏刷新指标按固定刷新测试中的平台呈现时间戳计算，需明确起止、刷新周期、采样误差、可变刷新模式及重复呈现的处理。
比例定义为漏掉的刷新机会数/应有刷新机会数；正常 VSync 等待不计作 CPU 工作。
20 目标是管理开销微基准，真实 Gallery、较多目标压力及首次播放必须另测，不能据微基准宣布页面达标。

纯视觉优先合成；布局仅更新必要范围；启动前完成通道解析，逐帧不重建关键帧、列表、LINQ、绑定和资源查找。
不可变数据可复用，启动/收尾分配单列；持续动画按明确可见性/视口策略运行，有限离场不因暂停而永远不完成。
UI 线程同步构建页面、纹理上传、阴影和透明叠加同样参与场景预算，必要优化发生在其真实 owner。

Release 真 Gallery 验收包括 Popup/Dialog/Drawer、NavMenu 侧栏/子菜单、Collapse/Expander/TreeView、Feedback 堆叠、
导航与动画并发、快速反向、动态内容、换模板及卸载重挂载。分别记录首次、预热、重复多进程样本与压力样本。
记录实际呈现帧间隔 P50/P95/P99、漏刷新、首帧等待、UI/布局/渲染时间、CPU、分配、GC 和结束后的资源数量。
现有属性/Processed/Rendered 探针继续用于定位，但 batch Rendered 不是显示器 present，需平台呈现追踪交叉验证。
不通过 30FPS 原视频、RAF 回调数或短动画平均 FPS 宣称达到 60/120FPS。

## 13 模块与资源归属

| 模块 | 所有权 |
| --- | --- |
| AtomUI.Core | Motion 定义、构建合同、单次执行、曲线、类型化通道和内部后端适配 |
| AtomUI.Controls | 可复用布局/视口原语和必要模板接入能力 |
| Desktop 及可选控件包 | 业务状态、物理宿主、组件参数、模板/语义部件与交互 |
| 第三方库 | 自己的 Motion 子类、参数、纯曲线/插值与符合合同的布局扩展 |
| Gallery/性能工具 | 原生场景驱动、帧时、分配、资源和交互验收 |

先复用现有包，不新增运行时项目。Core 不反向依赖 Controls/Desktop，第三方基础视觉动效不依赖 Desktop 初始化。
资源覆盖有释放句柄，owner/目标关联在 detach、模板替换、异常和关闭时解除；缓存不得保留目标树。
不引入运行时程序集扫描、动态泛型构造、字符串属性发现、Expression.Compile 或靠整包保留规避裁剪。
NativeAOT、trimmed CoreCLR 与目标宿主的发布验证跟随实际改动风险，不从普通构建通过推导发布可用。

## 14 交付阶段与评审门槛

1. 能力验证：核验合成完成/反向/命中、原生轨迹与组合表达、取消前后端点、AXAML 参数配置及第三方布局合同；冻结 API 和性能环境。
2. 执行核心：Motion 基类、构建与验证、参数快照、请求身份、属性句柄、统一释放和可靠原生执行。
3. 视觉动效与扩展：先迁 Fade/Zoom/Slide/Move，再接第三方样例、自定义属性和组合；逐项校准 Ant 规格。
4. 控件与布局：迁 Popup/Dialog/Drawer、Feedback、展开控件与 TreeView，保存业务、首帧、滚动和语义样式合同。
5. 清除旧体系：消费者和公开迁移说明完成后删除 Actor 类族、双 transform 属性、旧矩阵布局及无消费旧基础设施，不留长期双实现。
6. 性能与发布验收：在冻结矩阵中完成真实呈现、交互、生命周期和发布验证，记录收益与剩余瓶颈后交用户 review。

每阶段先用既有或临时证据验证，按项目测试生命周期清理；最终只运行改动模块的精确验证。
不自行触发全量回归，不在用户审查前提交；不创建分支/worktree。阶段顺序不构成当前产品实施授权。

## 15 需在能力验证阶段确定的事项

- 用户确认最低性能设备、平台范围、分辨率/DPI、实际并发和内容规模；开发机采样不替代这份支持矩阵。
- 合成路径的可靠完成/反向/命中能力是否足够，是否需要单独的框架依赖方案。
- 布局扩展接口与具体原语的精确签名、AXAML 接入、组合边界和 AOT 发布可消费性。
- 基于参考设备复核拟定预算和漏刷新阈值；门槛冻结后不得通过换负载、缩短动画或改交互来宣布通过。

以上事项被明确列为前置验证，不将未知框架能力或尚未测得的速度当作本方案的既成收益。

## 来源

- [MotionScene 源码审查](../progress/2026-10-27-motion-scene-ant-design-audit.md)：版本、数值比较、注册失败与源码风险的证据边界。
- [既有 P0/P1 交付](../progress/2026-10-27-animation-p0-p1-results.md)：已完成修改及其测量限制。
- [Ant 规格定义](https://github.com/ant-design/ant-design/tree/984592267de88fbf9cdaecc96a56629e721c709d/components/style/motion)、[curve Token](https://github.com/ant-design/ant-design/blob/984592267de88fbf9cdaecc96a56629e721c709d/components/theme/themes/seed.ts)。
- [rc-motion 状态协调](https://github.com/react-component/motion/blob/595a9ccd66d6739ceeca335edb9dd2c1dbb5a76f/src/hooks/useStatus.ts)、[Avalonia 合成提交语义](https://github.com/AvaloniaUI/Avalonia/blob/12.1.4/src/Avalonia.Base/Rendering/Composition/Compositor.cs)。
