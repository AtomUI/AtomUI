# AtomUI 边框渲染架构

本文档定义 AtomUI 在不同 DPI、不同 render scale 和不同显示器下绘制边框的架构原则。目标是在普通屏、3.5K、4K 和跨屏窗口移动场景中，让控件边框保持稳定、自然并符合 AtomUI 视觉语义，同时不向使用者暴露额外配置。

> 实现与验证边界：第 7 节的绘制与裁剪职责分离由 `DashedBorder` 统一实现，厚度换算和圆角算法保持原有语义。
> 共享实现、真实消费模板与原生平台的验收证据分开记录，具体边界见第 8.3 节。

## 1. 设计目标

- 用户无感知：业务代码、AXAML 使用方式、控件 API 和 Design Token 语义不改变。
- 保留设计语义：`LineWidth = 1` 仍表示设计系统中的 1 DIP 基础线宽，不被改写成屏幕相关值。
- 稳定物理观感：普通边框在整数 scale 下保持设计 DIP，在非整数 scale 下避免被取整放大成更粗的物理像素线。
- 复用成熟圆角算法：圆角绘制必须继续使用 AtomUI 已引入的 Avalonia / WinUI 派生算法，不手写新的圆角合成逻辑。
- 架构集中：屏幕 scale、layout rounding、hairline 等规则必须集中在共享 helper 或明确语义中，不能散落在各控件渲染代码里。
- 绘制职责独立：`Render()` 及其调用链只计算绘制参数、维护内部绘制缓存和记录绘制命令，不写入控件或子控件的 Visual 状态。
- 视觉等价：调整缓存与裁剪更新的职责时，保持正常场景的布局、绘制参数、裁剪几何和输入命中范围一致。

## 2. 背景结论

AtomUI 把基础线宽定义为普通设计 Token：Seed 层声明 `LineWidth = 1`，Alias 和 Control Token
在需要时继续消费该语义值。Token 层不按分辨率、显示器或 render scale 动态改写线宽。

AtomUI 在设计层保持稳定的 1 DIP 语义，并在绘制层完成物理像素适配。设计层不承担显示器差异，渲染层负责把
设计厚度转换为当前显示器 scale 下稳定的实际绘制厚度。

## 3. 分层模型

边框渲染分为三层。

### 3.1 设计语义层

设计语义层只关心设计系统值：

```text
DesignToken.LineWidth = 1
DesignToken.BorderThickness = new Thickness(LineWidth)
ControlToken / SharedTokenResource
```

这一层不读取显示器 DPI，不读取 `TopLevel.RenderScaling`，不做 `1 / scale` 计算。Token 是主题级语义，而 DPI scale 是 visual root / monitor 级运行时状态。窗口跨屏移动时，scale 可能变化；把运行时 scale 写进 token 会破坏主题语义，也会让不同窗口和不同显示器之间出现错误共享。

### 3.2 渲染线宽层

渲染线宽层负责把设计厚度转换为当前 render scale 下适合绘制的厚度。它不能简单调用 `LayoutHelper.RoundLayoutThickness()`，因为在 Windows 1.5、1.75 等非整数 scale 下，1 DIP 会被取整成 2 个物理像素，使基础边框明显变粗。

AtomUI 普通控件边框采用以下规则：

```text
if UseLayoutRounding and LayoutScale is non-integer:
    RenderThickness = BorderThickness / LayoutScale
else:
    RenderThickness = BorderThickness
```

这样 1 DIP 边框在 1.5 scale 下绘制为 2/3 DIP，最终落到 1 个物理像素；在 macOS Retina 2x 这类整数 scale 下仍保持 1 DIP，因此与网页 1px 边框的观感一致。

该逻辑应集中在内部 helper 中，例如：

```text
BorderUtils.BuildRenderScaleAwareThickness(Layoutable owner, Thickness thickness)
```

helper 的职责只限于 render scale 下的视觉线宽换算，不承担 token 读取、不承担业务控件状态判断。

### 3.3 绘制层

绘制层只接收已经确定的渲染输入：

```text
Bounds.Size
RenderThickness
CornerRadius
BackgroundSizing
Background
BorderBrush
DashStyle / BoxShadow
```

AtomUI 自绘边框控件必须在调用 `BorderRenderHelper` 或构建几何之前，把 `BorderThickness` 转换为 `RenderThickness`。

### 3.4 自绘边框的样式类型

`PixelAlignedBorder` 和 `DashedBorder` 是 AtomUI 自绘控件，不是 Avalonia `Border`。模板样式必须按真实类型选择：

```xml
<Style Selector="^ /template/ atom|PixelAlignedBorder#Frame">
    <Setter Property="BorderBrush" Value="..." />
</Style>
```

禁止通过 `StyleKeyOverride => typeof(Border)` 让自绘控件伪装成 Avalonia `Border`。运行时 selector 匹配使用控件的 `StyleKey`，AXAML 编译器则根据 selector 的目标类型解析未限定的 Setter 属性；如果自绘控件拥有独立注册的 `Background`、`BorderBrush` 等属性，这种伪装会让 `Border#...` selector 匹配成功，但 Setter 写入 Avalonia `Border` 的属性槽，而渲染器读取 AtomUI 自绘边框的属性槽，最终出现样式已匹配但边框或背景没有生效的问题。

派生自绘边框应使用自己的真实类型 selector。不得为了保留旧 `Border#...` selector 添加类型伪装；替换模板节点类型时，必须同时迁移对应 selector 和查找该节点的运行时测试。

## 4. Button 的绘制链路

Button 是边框策略的基准控件。它必须保持用户无感知。

### 4.1 普通 Button

普通 Button 模板使用 AtomUI 自绘边框 primitive。该 primitive 在未设置 `StrokeDashArray` 时绘制实线边框，内部通过 render-scale-aware thickness 保持不同 DPI 下的稳定视觉线宽：

```text
SharedToken.BorderThickness
    ↓
Button.BorderThickness
    ↓
Button.EffectiveBorderThickness
    ↓
ButtonTheme.axaml: DashedBorder#Frame.BorderThickness
    ↓
DashedBorder.GetRenderThickness(scale)
    ↓
BorderRenderHelper.Render(... RenderThickness ...)
```

普通 Button 不在 `Button` 类里手动计算 scale。它的合理边框来自 AtomUI 边框 primitive 内部的 `RenderThickness`，该值由 `BorderUtils.BuildRenderScaleAwareThickness()` 统一计算。

维护要求：

- 不在 `Button.ConfigureEffectiveBorderThickness()` 中做 DPI 或 scale 计算。
- `EffectiveBorderThickness` 继续表达控件状态后的设计厚度，例如 bordered / borderless。
- 普通 Button、DropdownButton 以及 Browser Button 备用主题中的普通 Frame 都应使用同一自绘边框 primitive，避免不同主题下边框厚度策略分裂。

### 4.2 Dashed Button

虚线 Button 模板使用同一个 AtomUI 边框 primitive，并设置 `StrokeDashArray`：

```text
SharedToken.BorderThickness
    ↓
Button.BorderThickness
    ↓
Button.EffectiveBorderThickness
    ↓
ButtonTheme.axaml: DashedBorder#Frame.BorderThickness
    ↓
DashedBorder.GetRenderThickness(scale)
    ↓
BorderRenderHelper.Render(... RenderThickness, StrokeDashArray ...)
```

该 primitive 应具备内部 render thickness 缓存：

- `BorderThickness` 或 `UseLayoutRounding` 改变时使缓存失效。
- 当前 `LayoutHelper.GetLayoutScale(this)` 改变时使缓存失效。
- `Render()` 使用 `RenderThickness`。
- `MeasureOverride()` / `ArrangeOverride()` 继续使用原始 `BorderThickness`，避免渲染线宽换算改变布局尺寸。

这样普通 Button 和 Dashed Button 在同一 scale 下得到一致的视觉厚度，只在 stroke dash 样式上不同。

## 5. 圆角算法边界

圆角是边框绘制中最容易出错的部分。AtomUI 已有成熟实现：

- `src/AtomUI.Controls.Shared/Utils/BorderRenderHelper.cs`
- `src/AtomUI.Controls.Shared/Utils/RoundRectGeometryBuilder.cs`

这些代码已经按 Avalonia / WinUI 思路处理圆角：

- `CornerRadius` 定义在 border stroke 中线。
- `BackgroundSizing.InnerBorderEdge` 使用内边界。
- `BackgroundSizing.OuterBorderEdge` 使用外边界。
- `BackgroundSizing.CenterBorder` 使用 stroke 中线。
- 复杂边框通过 outer rounded geometry 排除 inner rounded geometry 得到真实边框区域。

新的边框一致性机制不得替换该算法。正确做法是只改变传入算法的 `borderThickness`：

```text
旧输入：RoundRectGeometryBuilder(..., BorderThickness, CornerRadius, ...)
新输入：RoundRectGeometryBuilder(..., RenderThickness, CornerRadius, ...)
```

禁止做法：

- 手动 snap 圆角 arc 的点。
- 为每个控件单独手写 `ArcTo` 圆角。
- 把圆角半径按 scale 简单除法处理。
- 对不同控件采用不同的内外圆角合成规则。

## 6. 自绘控件接入规则

凡是控件自己绘制边框、自己创建 `Pen`、自己调用 `BorderRenderHelper` 或自己构建圆角几何，都必须先判断它绘制的是哪一种线。

### 6.1 普通控件边框

普通控件边框包括 Button、Input、GroupBox、OptionButton、TreeView item hover 背景边框、Badge ribbon 背景边框等。这类边框表达控件轮廓，应使用 `RenderScaleAware` 语义：

```text
BorderThickness
    ↓
BorderUtils.BuildRenderScaleAwareThickness
    ↓
BorderRenderHelper / RoundRectGeometryBuilder / Pen
```

### 6.2 发丝线和分割线

Separator、MenuSeparator、DataGrid grid line、TreeView node line、Card action separator 等需要逐个分类。它们不一定是控件轮廓，有些更接近 CSS hairline 或视觉分割线。

这类线不能混用普通边框 helper。应显式选择一种语义：

| 语义 | 使用场景 | 厚度规则 |
|---|---|---|
| `RenderScaleAware` | 控件轮廓、需要保持基础物理像素观感的线 | 非整数 scale 下除以 render scale；整数 scale 下保持设计 DIP |
| `Hairline` | 视觉分割线，希望无论设计 token 如何都保持 1 个物理像素 | 内部 helper 明确计算，不伪装成 token `BorderThickness` |

普通控件边框必须通过 `BorderUtils.BuildRenderScaleAwareThickness()` 进入共享策略，不能在各控件中重复手写 render scale 除法或 `RoundLayoutThickness` 取整逻辑。

## 7. 绘制参数、内容裁剪与生命周期

### 7.1 状态所有权与单向依赖

边框 primitive 将数值计算、绘制缓存和 Visual 状态维护分开；缩放检查不是修改全部状态的公共入口。

| 职责 | Owner | 输入与输出 | 更新边界 |
| --- | --- | --- | --- |
| 渲染厚度计算与缓存 | `DashedBorder` 与 `BorderUtils` | 原始 `BorderThickness`、`UseLayoutRounding`、当前 layout scale → `RenderThickness` | 只更新内部数值缓存，不调用裁剪维护或 Visual 失效方法 |
| 边框绘制几何与 Pen 缓存 | `BorderRenderHelper` | 绘制尺寸、有效厚度、圆角、背景绘制范围、画刷、虚线和阴影 → 绘制命令 | 可在绘制期间维护内部缓存，不修改控件及子控件的 Visual 属性 |
| 内容圆角裁剪 | `DashedBorder` | 当前子控件尺寸、Padding、有效厚度和圆角 → 子坐标系中的 `Child.Clip` | 只在缩放通知、布局完成和既有属性更新入口维护 |
| 小数缩放尾边处理 | `PixelAlignedBorder` 复用基类计算 | 当前缩放、祖先裁剪链与 Bounds → 绘制尺寸 | 保留既有算法，只影响边框绘制尺寸，不改变子控件布局 |

单向依赖为：

```text
Render：当前 scale → 有效厚度 / 绘制尺寸 → BorderRenderHelper → 绘制命令
裁剪维护入口：当前 scale / 已排列的 Child.Bounds / Padding / CornerRadius → 裁剪几何 → Child.Clip
```

渲染厚度的计算不能调用 `UpdateClip()`，裁剪维护也不能经由厚度计算再次进入自身。
裁剪维护将计算出的有效厚度作为几何输入；几何构建函数不订阅事件、不写 Visual 属性。
`Render()` 一次读取当前 scale，厚度和尾边绘制尺寸使用同一个值，避免隐式读取不同阶段的缓存。

### 7.2 缓存失效与裁剪更新入口

数值缓存和绘制几何缓存分别按实际依赖失效：

- `BorderThickness`、`UseLayoutRounding` 或当前 layout scale 变化，使渲染厚度缓存失效。
- 绘制尺寸、有效厚度、`CornerRadius`、`BackgroundSizing` 或虚线启用状态变化，使相关绘制几何缓存失效。
- 画刷与虚线参数按现有 Pen 维护规则更新，不扩大几何重建范围。

控件在 `Render()` 中仍需核对当前 scale，使未改变设计厚度的跨屏窗口得到正确绘制厚度；这个核对过程只影响内部
绘制参数缓存。它不能通过 setter、裁剪释放、几何对象修改或失效调用，间接改变已交给 Visual 使用的状态。

内容裁剪的维护入口如下：

| 入口 | 裁剪维护职责 |
| --- | --- |
| `ArrangeOverride()` 完成子控件排列后 | 使用当前 `Child.Bounds.Size` 更新裁剪；Padding、内容尺寸或布局约束的变化由该入口收敛 |
| `ClipContentToCornerRadius` 变化 | 开启时按现有条件应用裁剪；关闭时仅释放本控件仍拥有的裁剪，并解除缩放订阅 |
| `BorderThickness`、`CornerRadius`、`BackgroundSizing` 变化 | 保持既有属性更新语义，使用当前已排列的尺寸维护裁剪 |
| `Child` 移除或替换 | 释放旧子控件上仍由本控件拥有的裁剪；新子控件排列后建立自己的裁剪 |
| 所属 `TopLevel.ScalingChanged` | 在绘制前用新 scale 和当前已排列的子控件尺寸更新裁剪 |

绘制缓存失效不等于立即重写 `Child.Clip`。运行时切换 `UseLayoutRounding` 的额外裁剪刷新属于独立行为变更，
不能混入绘制与裁剪的职责调整；改变这类既有入口必须单独评估视觉与命中范围。

### 7.3 缩放时序与订阅生命周期

Avalonia 的平台缩放通知先更新 `TopLevel` 的 scale、使子树布局失效，再通过 `ScalingChanged` 通知订阅者。
边框在该通知中读取当前 layout scale，按第 3.2 节的相同公式计算厚度并维护裁剪。

必须覆盖两种顺序：

1. 缩放先于布局：通知阶段维护现有尺寸对应的裁剪；正常排列完成后，使用最终尺寸更新裁剪，然后绘制。
2. 缩放晚于布局、早于绘制：通知阶段直接用当前已排列的尺寸和新厚度建立该帧裁剪；随后绘制读取同一缩放对应的
   参数。平台已请求的下一次正常布局完成后，再按最终尺寸校正；不能把新缩放的裁剪延迟到下一帧才建立。

订阅由边框实例拥有，只在已挂载且启用内容圆角裁剪时保持；未启用裁剪的普通边框不增加逐实例缩放订阅。
挂载、裁剪开关和卸载均需同步订阅所有权；卸载或关闭裁剪时解除订阅，再次挂载到同一或另一 `TopLevel` 时，
仅与当前 root 建立一次订阅。首次排列负责按当前 root 和最终尺寸建立裁剪。

detach 本身不改变现有裁剪的视觉语义；旧子控件裁剪的释放仍遵循第 7.4 节的所有权规则。
缩放通知使用正常生命周期完成状态更新，不额外安排 timer、Dispatcher 延迟、强制布局或为重复渲染设置抑制标记。
订阅管理和计算路径使用静态类型与公共事件，不引入反射、私有 API 或按使用控件区分的回退路径。

### 7.4 裁剪坐标与所有权

`ClipContentToCornerRadius` 默认关闭。开启时，`DashedBorder` 在子控件的局部坐标系中建立内边缘裁剪，计算保持：

```text
ChildRect = Rect(Child.Bounds.Size)
GeometryInput = ChildRect.Inflate(Padding + RenderThickness)
Clip = 现有圆角构建器(GeometryInput, RenderThickness, CornerRadius, InnerBorderEdge)
```

构建器会再次按厚度收缩输入，因此裁剪边界为子控件区域向外扩展 Padding 后的内边缘，不能额外缩小内容。
圆角半径、非对称边框处理和坐标转换继续使用现有算法，不单独按 scale 换算圆角。

没有子控件、子控件尺寸非正、关闭裁剪或后端无法正确执行几何包含性判断时，按现有规则释放本控件管理的裁剪。
释放必须同时核对原子控件和几何对象引用；外部已替换的 Clip 不得被释放逻辑清空。
更新或释放附着的几何对象均属于 Visual 状态维护，不得出现在 `Render()` 的调用链中。

### 7.5 视觉与集成兼容边界

职责调整保持 Public API、默认值、Token、ControlTheme、模板节点、selector、语义部件以及外部定制入口不变。
相同布局与输入下，必须保留原始 `BorderThickness` 的布局结果、第 3.2 节的绘制厚度、相同的裁剪几何输入，
以及虚线、背景、阴影和尾边处理规则。正常场景的像素与命中差异不能作为消除渲染异常的代价。

ListBox、ListView 和 Expander 默认开启根 Frame 的内容圆角裁剪，统一消费 primitive 的生命周期契约。
Button、输入框、DataGrid、Tab、Card 等其他消费者不增加专用缩放分支。所有消费者保留原始高度约束、内容、
滚动条、嵌套滚动和裁剪链；不得通过移除圆角内容裁剪、改成矩形裁剪或更改布局掩盖更新时序问题。

## 8. 测试与验证

### 8.1 线宽与绘制参数

验证 scale 至少覆盖：

```text
1.0
1.25
1.5
1.75
2.0
```

重点场景：

- 普通 Button 和 Dashed Button 在同一 scale 下边框粗细一致。
- 普通屏、3.5K、4K 下同一控件没有明显忽粗忽细。
- 圆角处无断裂、无角部厚薄突变、无背景漏线。
- 窗口跨屏移动后，自绘边框缓存失效并按新 scale 绘制。
- `UseLayoutRounding = false` 时，自绘控件尊重原始 `BorderThickness`。
- GroupBox 这类复杂边框在 Header gap、透明背景和不同圆角下仍稳定。

自动化测试优先级：

- helper 单元测试：验证非整数 scale 下 1 DIP 会转换为 1 个物理像素，整数 scale 下保持设计 DIP。
- `DashedBorder` 渲染输入测试：确认传入 helper 的 thickness 与共享策略一致。
- 控件视觉回归：Button、Dashed Button、GroupBox、OptionButton。
- `git diff --check` 作为文档和代码改动的收尾检查。

### 8.2 裁剪时序与视觉等价

- 保留真实子控件裁剪，在布局完成后、绘制前注入平台缩放通知，使用真实 compositor 取得相同条件下的修复前后证据；
  同时断言绘制期间没有 Clip 写入或修改已附着裁剪几何，不能只断言未抛异常。
- 在相同环境和每档 scale 下比较静态及缩放往返的对应稳定帧，核对边框厚度、圆角边缘、背景、虚线、阴影、Bounds、
  子控件位置和几何命中范围。正常对照场景要求零差异像素；差异应追踪到输入与生命周期，不能直接放宽阈值。
  自动隐藏滚动条等动画应比较同一确定终态；动画进行中的交互与时序独立复核，不把不同动画进度的帧作为等价对照。
- 瞬时缩放状态单独核对当前已排列尺寸和新厚度对应的裁剪，确认绘制前已更新，避免仅比较最终稳定帧而漏掉一帧旧裁剪。
- 覆盖裁剪开关、零尺寸、非对称边框和圆角、Padding、子控件替换、外部 Clip 替换、重新挂载和不同 scale 的 root；
  核对旧订阅释放、新订阅唯一性和旧子控件清理。
- 使用真实 ListBox、ListView、Expander 模板验证圆角内容、hover/selected 背景、滚动条与命中行为；
  保留受限高度、内容量和原有滚动条件。其他消费者按共享机制和差异路径选择代表性验证，不复制整套排列组合。
- 默认 Headless 绘图后端可能按几何命中能力降级为不应用裁剪；直接调用 `Render(DrawingContext)` 也不经过 compositor
  的更新保护。这两种测试均不能单独证明裁剪更新时序正确，像素与时序验收使用 Headless + Skia 和真实 compositor。

临时验证的存放、清理、测试去留和最终模块验证遵循
[测试价值与生命周期](../../../engineering/development/test-value-and-lifecycle.md)；不因补充时序验证默认扩大常驻测试集或运行全量回归。

### 8.3 实现与平台验证边界

`DashedBorder` 的数值缓存只服务绘制参数计算，内容裁剪由挂载期间的缩放订阅与既有布局、属性入口维护。
共享实现的完成证据必须对应正式源码，包含第 8.2 节的失败前证、同条件修复后验证和实际模板复核；
临时原型的对照或文档更新本身不能代替这些证据。

当前正式源码的验证覆盖 Headless + Skia 的真实 compositor、消费模板对照及列表输入，并完成 macOS 原生窗口中
真实按钮触发的布局后缩放通知往返、列表选择与滚动复核。显式通知注入验证更新时序，不等于原生移屏复现。

原生窗口移屏、Windows DPI 切换以及 Linux/Wayland 小数缩放与 resize 需分别记录实际验证范围和结果。
这些平台操作尚未执行；显式缩放通知的成功不能推断这些平台路径已通过。零差异像素仅比较同一环境、同一后端的对应状态，
不要求不同操作系统或后端的抗锯齿输出逐像素相同。

## 9. 维护不变量

- Design Token 不读取 DPI / scale。
- `LineWidth = 1` 在 token 层表示 1 DIP 设计线宽；在非整数 scale 的渲染层会转换为 1 个物理像素。
- 普通 Button 使用 AtomUI 自绘边框 primitive，不做额外 DPI 特判。
- 自绘普通边框必须使用 `BorderUtils.BuildRenderScaleAwareThickness()`。
- 圆角绘制继续使用 `RoundRectGeometryBuilder`，不新增平行算法。
- 普通边框与 hairline 必须语义分离。
- scale 变化必须使自绘边框缓存失效。
- 绘制参数计算与内容裁剪维护保持单向依赖，`Render()` 不能写入或间接修改 Visual 状态。
- 缩放通知到绘制之间的裁剪更新与挂载订阅由边框 primitive 统一拥有，不在消费控件中增加特殊处理。
- 原始视觉和交互契约是职责调整的验收条件，不能通过放宽像素差异阈值或移除触发条件完成验收。
- 新增自绘边框控件时，必须在实现文档或代码结构中说明其边框语义。
