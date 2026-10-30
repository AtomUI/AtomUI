# BorderBeam 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.BorderBeam` 桌面版的最新设计定位、公共契约、装饰状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [BorderBeam 桌面版实现原理](implementation.md)，BorderBeam Token 的专项设计见 [BorderBeam Token 设计](token.md)，设计和契约变化记录见 [BorderBeam Changelog](changelog.md)。

参考 `BorderBeam` 的核心语义是为容器边框提供持续流动的装饰性高亮效果。AtomUI 的 BorderBeam 以该设计语义为基准，按 Avalonia 控件模型实现为独立包装控件，并通过显式边界感知接口获取被装饰控件的有效边框和圆角。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Other/BorderBeam` |
| 控件状态 | Stable |

BorderBeam 是 AtomUI 桌面其他类控件中的装饰性包装控件，用于在容器边界上绘制持续流动的高光效果。它强化某个容器的视觉关注度，但不表达焦点态、校验态、选中态、错误态、警告态或任何业务状态。

BorderBeam 的职责是围绕一个内容控件绘制一个或多个流光边界，并在不拦截内容交互的前提下提供数量、颜色、渐变、外扩、圆角、线宽和动效控制。它不拥有内容控件的布局语义，不改变内容控件的输入、焦点、命中测试、状态同步、数据绑定或模板结构。

## 2. 设计语言

BorderBeam 的设计语言是“非业务状态的动态强调”。它通过沿容器边界运行的高光段形成持续注意力提示，但保持内容本身的控件语义不变。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 装饰性 | 流光只是视觉强调，不参与业务状态判断。 | `IsHitTestVisible=false` 的渲染层。 |
| 容器边界 | 流光贴合容器有效边框和圆角。 | `IBorderBeamAwareControl` 或显式 `BorderThickness` / `CornerRadius`。 |
| 品牌强调 | 默认颜色来自主题主色。 | `ColorPrimary`、`ColorPrimaryHover`。 |
| 渐变尾迹 | 用户停靠点映射到可见段，尾部保留透明衰减。 | `ColorStops.Percent` 映射到可见段。 |
| 持续流动 | 默认流光不跟随全局动效开关关闭，保持装饰强调一致可见。 | `IsMotionEnabled` 实例开关。 |
| 均匀分布 | 多个光束共享一个周期，并沿同一边界保持等距。 | `Count` 与共享 `Progress`。 |

BorderBeam 不应绘制成一个新的实体边框，也不应让被装饰控件看起来拥有新的可交互状态。流光层应贴合容器边界，透明尾迹应保持连续，圆角转弯处不应出现断裂。

## 3. API 与契约模型

BorderBeam 的公共 API 采用 Avalonia 属性模型。需要 XAML 设置、Style 设置、绑定、主题参与或动画参与的标量状态定义为 `StyledProperty`。可变集合类状态使用 `DirectProperty` 加实例级集合，避免共享默认集合和构造函数 local value 覆盖外部绑定。

BorderBeam 控件 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `Content` | `object?` | 被装饰内容，继承自 `ContentControl`。 |
| `Color` | `Color?` | 单色流光配置。 |
| `ColorStops` | `AvaloniaList<BorderBeamColorStop>` | 渐变流光停靠点集合。 |
| `Outset` | `Thickness?` | 流光层相对有效边界的渲染偏移；`null` 时贴合有效边界，不额外改变绘制范围。 |
| `BorderThickness` | `Thickness` | 未命中感知接口时使用的边框厚度，同时决定流光线宽；默认值来自共享边框厚度 Token。 |
| `CornerRadius` | `CornerRadius` | 未命中感知接口时使用的圆角。 |
| `IsMotionEnabled` | `bool` | 控制当前实例的流光动画是否启用；默认值不绑定全局 motion 设置。 |
| `Duration` | `TimeSpan` | 流光运行一周的时长；默认值为 `6s`。 |
| `BeamSize` | `double` | 流光高光段的设备无关像素基准尺寸；默认值为 `100`。 |
| `Count` | `int` | 沿同一边界均匀分布的光束数量；默认值为 `1`，小于 `1` 时按 `1` 渲染。 |

`ColorStops` 非空时优先于 `Color`。`ColorStops` 为空且 `Color` 不为空时，使用单色流光。两者均为空时，使用主题默认渐变。

边界感知接口：

```csharp
public interface IBorderBeamAwareControl
{
    BorderBeamGeometry GetBorderBeamGeometry();
    event EventHandler? BorderBeamGeometryChanged;
}

public readonly record struct BorderBeamGeometry(
    Thickness BorderThickness,
    CornerRadius CornerRadius);
```

`IBorderBeamAwareControl` 是被装饰控件向 BorderBeam 暴露有效边界几何的显式契约。它只描述可用于流光贴合的边框厚度和圆角，不暴露控件状态、不暴露模板节点、不要求 BorderBeam 理解被装饰控件的内部实现。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_ContentPresenter` | `ContentPresenter` | 承载被装饰内容。 |
| `PART_BeamPresenter` | internal `BorderBeamPresenter` | 绘制边框流光，不参与命中测试。 |

## 4. 行为与状态模型

BorderBeam 的行为优先级：

```text
IsVisible=false
> IsMotionEnabled=false
> Content geometry changed
> Progress animation
> Normal render
```

BorderBeam 不拦截鼠标、触控、键盘或焦点。流光 presenter 必须 `IsHitTestVisible=false`，内容控件继续承担自身交互。BorderBeam 不改变内容控件的 `IsEnabled`、`:pointerover`、`:pressed`、`:focus`、`:disabled` 或任何伪类。

effective state 由几何状态、颜色状态、数量状态和动效状态组成：

- 几何状态：优先读取 `IBorderBeamAwareControl`，未命中时使用 BorderBeam 自身 `BorderThickness` 与 `CornerRadius`。
- 颜色状态：`ColorStops` 优先，其次 `Color`，最后使用主题默认渐变。
- 数量状态：`effectiveCount = Math.Max(1, Count)`；`Count` 变化只触发 presenter 重绘，不替换模板或重启动画。
- 动效状态：实例级 `IsMotionEnabled`、可见性和有效尺寸共同决定动画是否运行；默认主题不从全局 `EnableMotion` 覆盖该属性。

`Progress` 是所有光束共享的 internal animation state。第 `i` 个光束使用
`NormalizeProgress(Progress + i / effectiveCount)` 计算相位。`Progress` 不形成公共 API，不参与样式选择器，
不允许外部绑定。

“仅悬停时显示”不是 BorderBeam 的内置状态或公共属性。调用方可以用 Style 在默认状态设置
`IsMotionEnabled=false`，并在 BorderBeam 根控件的 `:pointerover` 状态设置为 `true`；该组合会复用现有动画
启动与停止路径，内容控件仍保留完整交互。

## 5. 视觉与主题模型

BorderBeam 的模板由内容层和装饰层组成，视觉树保持必要最小层级。

```text
BorderBeam
└─ Grid
   ├─ ContentPresenter#PART_ContentPresenter
   └─ BorderBeamPresenter#PART_BeamPresenter
```

视觉层级要求：

- `BorderBeamPresenter` 覆盖内容层边界，在一个 presenter 内绘制 `effectiveCount` 个等距光束，但不得改变内容层测量和排列结果。
- `BorderBeamPresenter` 必须 `IsHitTestVisible=false`。
- 不把 beam 层插入被装饰控件模板内部，不修改 Card、Button、GroupBox 等控件的模板结构。
- 不使用全局 `ScopeAwareAdornerLayer` 作为默认实现。
- 不在内容控件未实现感知接口时尝试读取其 internal 属性或模板 part。

BorderBeam Theme 只负责装配内容层和流光 presenter，并设置默认 token 绑定。几何解析、颜色归一和动画生命周期属于 C# 状态模型。

## 6. 控件家族或集成关系

BorderBeam 与 AtomUI 控件家族的几何集成通过 opt-in 的 `IBorderBeamAwareControl` 完成。该接口是宿主主动提供
有效边界的扩展契约，不表示 BorderBeam 会按内容类型自动发现或推断几何。

适配对象应满足两个条件：

- 控件已经拥有稳定的有效边框和有效圆角状态。
- 控件边界是用户自然认为的视觉容器边界。

Card、Button、GroupBox 和输入壳体类控件只有在各自实现该接口后才进入自动几何同步。当前产品控件没有内置
`IBorderBeamAwareControl` 适配；它们仍可被 BorderBeam 包裹，但需要用户通过 `BorderThickness` 和
`CornerRadius` 显式对齐。控件家族不得仅为了接入 BorderBeam 而暴露 internal 主题状态或模板部件。

## 7. 兼容性不变量

BorderBeam 设计和实现必须保持以下不变量：

- BorderBeam 是装饰控件，不改变内容控件的公共 API、主题契约、伪类、事件或方法。
- BorderBeam 不替代焦点态、校验态、选中态、错误态或警告态。
- BorderBeam 不拦截内容控件输入事件。
- BorderBeam 不通过反射读取内容控件 internal 属性。
- BorderBeam 不修改内容控件模板，不依赖内容控件 template part 名称。
- `IBorderBeamAwareControl` 只暴露边界几何，不暴露业务状态。
- `ColorStops.Percent` 的 public 输入范围固定为 `0~100`。
- `Count` 的 public 默认值固定为 `1`；非正值按一个光束渲染。
- 多光束不得按数量增加 Presenter、Visual、Animation 或 cancellation owner。
- 悬停展示通过调用方 Style 组合，不新增或依赖 BorderBeam 专用伪类。
- 动效禁用时不应持续产生 UI 线程动画或 render invalidation。
- 颜色和线宽默认值必须跟随主题 token，支持 light / dark 主题切换。

如果实现某项能力时无法保持这些不变量，应先停止实现，说明原因、影响范围、替代方案和迁移方式，并获得授权。

## 8. 专项模型

### 8.1 边界感知模型

BorderBeam 的核心专项模型是边界感知。控件实现 `IBorderBeamAwareControl` 时，应返回已经考虑自身状态后的最终几何。例如 Button 应返回已经考虑 `ButtonType`、`Variant`、`Shape`、`CompactSpace` 后的有效边框和圆角。

### 8.2 渐变停靠点模型

BorderBeam 渐变停靠点以用户友好的 `0~100` percent 表达。内部渲染时将这些停靠点映射到可见高光段，并保留尾部透明渐隐。

### 8.3 动效模型

BorderBeam 是持续装饰性动效。默认主题不把全局 `SharedToken.EnableMotion` 或 `ThemeManager.IsMotionEnabled` 绑定到 `BorderBeam.IsMotionEnabled`，因此全局关闭普通交互动效时，BorderBeam 流光仍保持运行。只有当前实例显式设置 `IsMotionEnabled=false` 时，流光效果隐藏，内容保持完全可用。

### 8.4 渲染连续性模型

BorderBeamPresenter 应以一个连续圆角矩形运动路径驱动流光。高光段本身按路径切线旋转，并使用与 参考设计体系 `offsetAnchor: 90% 50%` 等价的锚点模型，使光束提前进入转角并保持尾迹连续。

### 8.5 多光束模型

BorderBeamPresenter 只拥有一套 `Progress`、`Animation` 和 `CancellationTokenSource`。一次 render 为当前边界
构建一份路径关键点和周长度量，再按 `effectiveCount` 计算等距相位、采样位置并绘制。光束数量只线性增加
draw call，不增加 Visual 或动画 owner。

### 8.6 悬停组合模型

悬停展示通过 BorderBeam 根控件 `:pointerover` 与现有 `IsMotionEnabled` 组合。未悬停时 presenter 隐藏且持续动画
停止，悬停时重新启动；装饰层始终 `IsHitTestVisible=false`。该策略属于页面或应用 Style，不属于默认主题和
BorderBeam 公共状态。

## 9. 文档导航与验证策略

关联文档：

- [BorderBeam 桌面版实现原理](implementation.md)
- [BorderBeam Token 设计](token.md)
- [BorderBeam Changelog](changelog.md)

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| 文档 | `overview.md`、`implementation.md`、`token.md`、`changelog.md` 链接有效。 |
| C# 状态 | StyledProperty / DirectProperty、边界感知接口、动画生命周期和事件释放。 |
| 渲染 | 默认渐变、单色、多 stop、单/多光束、统一圆角、非统一圆角、Outset、实例禁用 motion 和命中测试。 |
| Token | Light / dark 主题下默认颜色、线宽、圆角和 motion 默认值正确。 |
| Public API | `Count` 默认值与模板转发、普通内容、感知内容、无内容、零尺寸、不可见状态不抛异常。 |
| Gallery | Basic、Show on hover、Multiple beams、Custom container、Non-uniform radius、Customized color、Duration、Size、Line width 的稳定 SourceKey、布局和四语资源；Multiple beams 在同一示例中成对展示 `Count=3` 与 `Count=2`；Custom container 以标准 Avalonia `Border` 验证普通内容的显式几何契约；Customized color 提供六组可切换预设，并同步展示用途、说明和显式颜色/百分比 Tag；Duration 对比 `3s`、`6s`、`12s`，Size 对比默认 `100`、`56`、`160`，Line width 通过匹配 BorderBeam 与内容容器的 `BorderThickness=2` 展示线宽；本次新增的 Show on hover、Multiple beams、Custom container、Duration、Size 和 Line width 均标记 AtomUI 当前版本 `v6.1.8`。 |
