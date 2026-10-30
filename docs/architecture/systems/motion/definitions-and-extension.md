# Motion 定义与第三方扩展

本文拥有[Motion 系统](overview.md)的定义、构建和扩展合同。执行与终态语义见[播放协议](playback-and-lifecycle.md)。

## 基类与配置

`AbstractMotion` 是动效定义的抽象基类。公共 `PlayAsync(Control target, CancellationToken cancellationToken = default)`
是非 virtual 入口，返回 `Task<MotionResult>`。子类实现受保护的
`Build(MotionBuilder builder, in MotionContext context)`。

配置包括可选 Duration/Easing、Delay、Origin 与 FillMode。具体默认时长/曲线由动效家族与有效组件参数确定，
FillMode 的默认与延时语义由[播放协议](playback-and-lifecycle.md#终态与-fillmode)定义。

```csharp
var motion = new ZoomInMotion
{
    Duration = TimeSpan.FromMilliseconds(300)
};
var result = await motion.PlayAsync(surface, cancellationToken);
```

AbstractMotion 子类是可由 CLR/AXAML 配置的定义对象；准备阶段生成本次不可变运行快照。配置及准备在目标 UI 线程发生，
播放中的修改只影响后续请求。共享实例不得保存目标、运行任务、取消源、句柄或订阅。
同一配置对象跨 Dispatcher 使用不属于默认支持范围。

参数优先级为本次显式请求覆盖、实例显式配置、组件默认值、家族默认值。组件接入不能通过修改共享定义覆盖时长。
Token 的动态绑定属于 ControlTheme/控件资源宿主，准备时读取有效值，不在普通 Motion 对象上建立永久资源订阅。
禁用动效和首次状态基线不等同于某种曲线或 Token。

## 家族

| 子类族 | 语义 |
| --- | --- |
| FadeInMotion / FadeOutMotion | 透明度入场与离场 |
| ZoomInMotion / ZoomOutMotion 及 big/方向变体 | 缩放、透明度与明确的变换原点 |
| SlideUpInMotion / SlideUpOutMotion 等四方向变体 | 对应轴缩放与透明度 |
| 各方向 MoveIn/Out 子类 | 平移与透明度 |
| ExpandMotion / CollapseMotion | 自然内容占位和视口变化，需要布局能力 |
| SequenceMotion / ParallelMotion | 顺序或并行时间轴，仍是 AbstractMotion 定义 |

子类可以共用内部轨迹构建逻辑，但不各自实现播放与释放。内置实现使用和第三方相同的公开构建合同。
具体端点、方向原点、曲线控制点与组件默认值由[内置规格](built-in-motion.md)统一拥有。

## 构建合同

`MotionContext` 提供本次有效目标尺寸、缩放、已解析参数及必要初始值的只读快照，不暴露可随意改写的目标。
`MotionBuilder` 声明强类型通道、关键帧、分段曲线与端点；构建完成后失效，不得保存到字段或继续写入。

```csharp
public sealed class RiseInMotion : AbstractMotion
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

Build 同步执行，只构建轨迹，不启动任务、订阅或独立帧循环。几何需重新解析时重新准备请求，不逐帧重复构建配置。
具体 fluent 方法属于该构建合同的表达方式，签名冻结前须通过第三方库及编译 AXAML 的真实接入验证。

## 通道与曲线

准备阶段校验有限数值、时间范围、目标适用性、变换顺序和同一时间段的通道冲突，失败发生在目标属性写入之前。
位移显式区分 DIP 与目标参考尺寸比例；百分比不得误用父元素或屏幕尺寸。Origin 与变换顺序一起进入规格验证。
opacity 的合法范围与曲线超调分别处理；不将所有缓动输出统一钳制到 0–1。

CSS cubic-bezier 按输入时间求解 `BezierX(u)=t`，再取得 `BezierY(u)`。框架同名 easing 不能替代指定控制点。
曲线参数集中维护，Fast/Mid/Slow 使用有效 Token。现有求解满足误差要求时直接复用，不能只因算法名称重新实现。
不可变系数可以共享；几何和目标相关结果不跨控件缓存，缓存不持有控件树。

## 组合

SequenceMotion 在同一时间轴排列子段，明确各段起止、间隔及未声明起点如何承接上一段已解析终点。
ParallelMotion 同时开始，按子段自身时长结束，整体覆盖范围取最长子段。显式父 Duration 按比例缩放整个时间轴；
父 Delay 只应用一次，子段局部 Delay 保留。子段保留自身曲线，父级不得再次施加全局非线性 easing。

首版 ParallelMotion 的不同子项不得声明同一目标的同一通道，即使通过局部 Delay 排成不同时间段也拒绝；
需要依次写同一通道时使用 SequenceMotion，不采用最后写入获胜。
组合建立一个请求，内部不调用子项 PlayAsync。仅单个子段结束不等于组合完成。
取消、异常或被替代使整个组合结束，未开始的子段不再执行。

子项 FillMode 只允许默认 Both，其他模式在准备阶段拒绝；子项不建立独立运行/终态记录。
Parallel 的子项在局部 Delay 内持有其起点，结束后持有终点直至组合结束。
Sequence 后续子段的局部 Delay 保持上一段末值，不提前覆盖前段；第一段无先前值的通道持有该段起点。
进入下一子段时显式起点与上一段终点不同表示有意跳变；需要连续性时使用继承前段终点的构建表达。
组合整体的 FillMode 独立控制组合 Delay 和整体结束后的持有/恢复，不逐段释放属性覆盖。

多目标共享时间轴按实际消费需求接入，不以逐帧属性 Follow 建立同步。首版不默认引入弹簧或物理模拟引擎。

## 第三方与 AOT

开发者以普通 .NET/NuGet 库提供 AbstractMotion 子类，通过直接构造或编译 AXAML 消费。普通子类无需全局注册或程序集发现。
自定义 Easing/插值器必须是可重入纯计算，算法对象在运行期间保持不可变。库冻结本次配置引用，不克隆任意第三方对象的内部状态。
能力不足的属性使用明确的原生路径，不能承诺所有扩展都在合成端运行。

自定义属性采用强类型属性句柄与显式插值器，按具体属性身份适配。不得以宽泛类型注册抢占其他属性的 handler。
属性适用性依据 Avalonia 实际注册关系，支持 AddOwner；类型化端点直接调用属性验证器，不进入反射式对象转换。
布局扩展消费[独立布局合同](targets-and-layout.md#布局能力)，不依赖内部 Actor 或 Desktop 友元权限。
扩展自身的运行资源由单次执行拥有，目标与缓存不得通过静态字段被长期保留。

扩展不得依赖运行时程序集扫描、动态泛型构造、字符串属性发现或表达式动态编译。发布要求遵守
[AOT 编程规范](../../../engineering/development/aot-programming-guidelines.md)。

## AXAML 接入

内置类型统一使用 `atom:` 前缀，映射到 `https://atomui.net`。
`Motion` 是拥有附加属性的 sealed 接入类型，私有构造函数，不承载动效实例配置或构建逻辑。
`Motion.Enter`、`Motion.Leave` 的值均为 `AbstractMotion`；`Motion.IsVisible` 表达该接入口拥有的目标可见状态。
六个附加属性继续以 Motion 为注册 owner；公开命名空间、XMLNS 映射、名称和默认值保持一致。
实例播放调用 `definition.PlayAsync`；目标级恢复、请求身份和显隐完成分别使用 `Motion.Reset`、`Motion.IsCurrent`、`Motion.GetVisibilityTask`。
SequenceMotion/ParallelMotion 的 Children、构造参数及 MotionBuilder 的组合入口接受 AbstractMotion 定义。

```xml
<Border xmlns:atom="https://atomui.net"
        atom:Motion.IsVisible="{Binding IsOpen}">
    <atom:Motion.Enter>
        <atom:ZoomInMotion />
    </atom:Motion.Enter>
    <atom:Motion.Leave>
        <atom:ZoomOutMotion />
    </atom:Motion.Leave>
</Border>
```

使用该可见性入口后，不为同一目标再绑定另一条 IsVisible 控制来源。普通状态首次附着直接建立当前状态；
只有显式入场意图播放入场。该入口不接管 Popup/Dialog 的物理宿主、焦点或业务事件。
