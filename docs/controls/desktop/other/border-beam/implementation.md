# BorderBeam 桌面版实现原理

本文档描述 BorderBeam 桌面版的内部边界感知、颜色归一、动画生命周期和连续流光渲染。公共设计与 API 契约见 [BorderBeam 桌面版架构设计](overview.md)，Token 语义见 [BorderBeam Token 设计](token.md)，变化记录见 [BorderBeam Changelog](changelog.md)。

## 1. 实现定位

BorderBeam 的实现目标是在不修改内容控件模板、不拦截内容交互的前提下，绘制贴合内容有效边界的动态流光。实现文档聚焦包装控件、presenter 渲染、感知接口和动画资源释放。

BorderBeam 不表达业务状态，不承担 focus、validation、selected 或 error 等状态逻辑。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/BorderBeam/BorderBeam.cs`：公共 API、Content 几何感知、effective geometry 和
  `ColorStops` 集合变更通知。
- `src/AtomUI.Desktop.Controls/BorderBeam/BorderBeamPresenter.cs`：internal 渲染层，负责颜色归一、动画生命周期、
  单时钟多相位路径采样以及边框环内的流动高光绘制。
- `src/AtomUI.Desktop.Controls/BorderBeam/IBorderBeamAwareControl.cs`：被装饰控件边界感知接口。
- `src/AtomUI.Desktop.Controls/BorderBeam/BorderBeamGeometry.cs`：边框厚度和圆角几何值。
- `src/AtomUI.Desktop.Controls/BorderBeam/BorderBeamColorStop.cs`：单个渐变停靠点。
- `src/AtomUI.Desktop.Controls/BorderBeam/BorderBeamColorStops.cs`：颜色停靠点集合支持。
- `src/AtomUI.Desktop.Controls/BorderBeam/BorderBeamToken.cs`：BorderBeam 专属 Token。
- `src/AtomUI.Desktop.Controls/BorderBeam/Themes/BorderBeamTheme.axaml`：内容层和 presenter 层装配。

## 3. 核心类职责

`BorderBeam` 继承 `ContentControl`，负责公共属性、感知内容订阅、effective geometry，以及向默认主题暴露颜色、
几何和 motion 输入。它不持有 presenter 引用，也不在 owner 层构造渐变 brush。

`BorderBeamPresenter` 是渲染与动画 owner，负责根据 bounds、border thickness、corner radius、outset、beam size、
count、gradient stops 和 progress 构造当前帧 brush、裁剪和流光绘制。它只拥有一套 animation state，按多个
等距 phase 绘制同一光束视觉。它不参与命中测试，也不读取内容控件状态。

`IBorderBeamAwareControl` 是窄契约，只暴露边界几何和几何变化事件。BorderBeam 不通过类型判断或反射读取特定控件的 internal 属性。

## 4. 状态与数据流

状态流：

```text
BorderBeam public API
  Content / Color / ColorStops / Outset
  BorderThickness / CornerRadius
  IsMotionEnabled / Duration / BeamSize / Count
      ↓
Geometry awareness
  Content implements IBorderBeamAwareControl
    → BorderBeamGeometry
  otherwise
    → BorderBeam explicit geometry
      ↓
ControlTheme TemplateBinding
  effective border thickness
  effective corner radius
  color inputs / outset / motion inputs
      ↓
BorderBeamPresenter
  normalize gradient
  own one animation progress
  build one path metrics value per render
  render border-ring clip and N phase beams
```

边界来源优先级：

1. `Content` 实现 `IBorderBeamAwareControl` 时，使用 `GetBorderBeamGeometry()` 返回值。
2. `Content` 未实现接口时，使用 `BorderBeam.BorderThickness` 和 `BorderBeam.CornerRadius`。
3. 未显式设置时，使用 Theme 默认值。

颜色来源优先级：

```text
ColorStops.Count > 0
  → normalize stops
else if Color != null
  → [Color 0%, Color 100%]
else
  → theme default gradient
```

## 5. 组合结构模型

默认 `BorderBeamTheme.axaml` 直接表达内容层与装饰层，没有 Adorner、Popup、动态生成容器或额外宿主。

```text
BorderBeam (public)
└─ Grid (ControlTemplate composition node)
   ├─ ContentPresenter#PART_ContentPresenter (template-stable)
   └─ BorderBeamPresenter#PART_BeamPresenter (internal-observable)
```

协作节点：

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `BorderBeam` | public `ContentControl` | C# + default ControlTheme | 应用 / VisualTree | 全部 BorderBeam public API | public | 用户直接创建、绑定和样式化的根控件。 |
| `PART_ContentPresenter` | `ContentPresenter` | `BorderBeamTheme.axaml` | ControlTemplate | `Content`、`ContentTemplate`、内容对齐 | template-stable | 可用于维护模板契约，不承载 BorderBeam 状态。 |
| `PART_BeamPresenter` | internal `BorderBeamPresenter` | `BorderBeamTheme.axaml` | ControlTemplate | 颜色、几何、motion、时长、尺寸和数量的可观察渲染 | internal-observable | 只用于理解渲染与生命周期，应用不能直接依赖其类型或属性。 |
| `IBorderBeamAwareControl` content | public opt-in interface | 被装饰内容实例 | `BorderBeam` subscription | effective border thickness / corner radius | public | 宿主可以实现窄几何契约；BorderBeam 不推断宿主 internal 状态。 |

`PART_ContentPresenter` 和 `PART_BeamPresenter` 是默认模板的稳定部件。internal presenter 的 CLR 类型和内部
属性不是用户定制 API；替换模板的应用应通过 BorderBeam public/internal 模板绑定契约提供等价装饰层，且不得
让该层参与命中测试。

## 6. 生命周期与模板接入

`BorderBeam` 不在 `OnApplyTemplate` 中获取、缓存或释放 `PART_ContentPresenter` 与 `PART_BeamPresenter` 引用。
默认 ControlTheme 通过 `TemplateBinding` 将内容和 effective state 单向传入两个模板部件，包括从
`BorderBeam.Count` 到 presenter `Count` 的固定转发。模板实例及其视觉生命周期由 Avalonia 的 ControlTemplate
管理。

`Content` 变化时，BorderBeam 解除旧内容的 `BorderBeamGeometryChanged` 订阅，并在新内容实现
`IBorderBeamAwareControl` 时订阅新事件。attach 时重新确认当前内容订阅和 effective geometry；detach 时解除
aware-content 订阅。几何变化事件只刷新 BorderBeam 自身 effective state，不要求内容控件重新模板化。

`ColorStops` 由 BorderBeam 实例持有。替换集合时解除旧集合的 `CollectionChanged` 并订阅新集合；集合结构变化
通过 `ColorStopsProperty` 通知模板绑定和 presenter。Presenter 在 attach 或集合引用变化时建立自己的集合订阅，
并在 detach 或引用替换时释放。

`AttachedToVisualTree`、`DetachedFromVisualTree`、有效可见性、实例级 `IsMotionEnabled` 和 bounds 变化共同决定
动画启动与停止。有效可见性包含 BorderBeam 自身和 Visual 祖先链；持续 Avalonia animation 使用
`PlaybackBehavior.OnlyIfVisible`，因此隐藏页面中的实例暂停 animation clock。detached 或实例 motion 关闭时必须
取消动画并释放内部取消资源。默认主题不把全局 `EnableMotion` 绑定到 `BorderBeam.IsMotionEnabled`，避免全局普通
交互动效开关停止 BorderBeam 的持续流光。

`Count` 只在 presenter 侧进入 `AffectsRender`。修改它不会重新应用模板、重新创建 presenter、重启 Animation 或
替换 CancellationTokenSource。Gallery 的悬停示例通过 Style 切换 `IsMotionEnabled`，因此复用上述已有启动、取消
和释放路径，不增加 pointer 事件订阅。

## 7. 交互与事件处理

BorderBeam 不处理 pointer、keyboard、focus、command 或 drag/drop 事件。`PART_BeamPresenter` 必须 `IsHitTestVisible=false`。

内部事件只包括：

- content geometry changed。
- collection changed for `ColorStops`。
- motion / duration / beam size / count / visibility / bounds 变化。
- theme resource 更新导致默认颜色或参数变化。

这些变化只能刷新装饰层，不改变内容控件状态。只有 motion、duration 和 visibility 进入动画重启判断；beam size、
count 和主题资源变化只使当前 presenter 重绘。

## 8. 内部算法与关键流程

### 8.1 几何归一

有效边框和圆角来自感知接口或 BorderBeam 自身属性。Presenter 的 `Bounds` 与内容层共享同一布局边界，`Outset=null` 时直接使用该边界绘制边框环，不改变 `Measure`、`Arrange` 或任何布局尺寸；`Outset` 非空时仅在 Render 阶段按显式值偏移绘制范围。圆角半径应按 Avalonia 圆角矩形规则夹取，避免半径超过控件尺寸时产生自交路径。

### 8.2 渐变归一

`ColorStops.Percent` 输入区间固定为 `0~100`。内部将停靠点映射到可见高光段，并为尾部透明渐隐保留空间。停靠点需要 clamp 并排序，避免非法输入破坏 brush。

单色 `Color` 映射为可见段单色加透明尾迹。无显式颜色时使用主题默认渐变。

### 8.3 动画生命周期

动画使用 internal `Progress` 从 `0` 到 `1` 循环，周期由 `Duration` 控制。`Progress` 变化只触发 `BorderBeamPresenter` 重绘。

循环由实例级原生无限 `Style.Animations` 作用域推进，不在每个周期重新创建有限 RunAsync/Task/CTS。
时长或运行条件变化时释放旧作用域再按当前参数建立新运行，detach 释放作用域和视口监听。
`EffectiveViewport` 缓存真实几何，只有与绘制边界（包含 Outset）相交时运行；滚出视口停止，重新进入后恢复。

实例级 `IsMotionEnabled=false`、有效不可见、detached 或 bounds 无有效尺寸时，动画必须停止或暂停。此时不应继续产生 UI 线程 invalidation。

### 8.4 多光束相位与路径度量

一次 render 首先把 public `Count` 归一为 `effectiveCount = Math.Max(1, Count)`。第 `index` 个光束使用：

```text
NormalizeProgress(Progress + index / effectiveCount)
```

所有光束共享同一套 `Progress`、`Duration`、颜色、尺寸、透明度和边框环裁剪。Presenter 为当前 `renderBounds` 与
`BeamSize` 构建一个只读 `BorderBeamPathMetrics` 值，保存圆角矩形关键点、周长和退化边界的 fallback point；
随后所有 phase 都从该值采样，避免按光束数量重复计算圆角和周长。现有 bounds-based `GetPointAtProgress` 重载
委托给同一 metrics 路径，保持原有调用与测试语义。

### 8.5 渲染连续性

BorderBeamPresenter 应以一个连续圆角矩形运动路径驱动流光。边框环裁剪跟随被装饰容器的有效边框和圆角；运动路径的转角半径按 `BeamSize` 建立平滑圆角，避免高光段在容器真实小圆角处发生可见面积突变。

高光段按路径切线旋转，并使用 `90% / 50%` 锚点模型：流光矩形局部坐标的 `90% / 50%` 位置落在运动路径上。该模型让光束提前进入转角并保持尾迹连续，不应分成四条互不衔接的直线分别绘制。

## 9. 资源、性能与 AOT 边界

BorderBeam 不使用反射，不访问内容控件 internal 属性或 template part。集成通过 `IBorderBeamAwareControl` 完成。

实例级 motion 未启用时不应启动循环动画。隐藏祖先下的动画必须暂停；动画取消资源必须在 detached、模板替换、
实例 motion 关闭或动画重启时释放。Content 替换只更新 effective geometry，不重启动画。

ColorStops 使用实例级集合，避免共享默认集合。集合变更应触发渐变重建和 presenter 重绘，不应重建整个模板。

`Count=N` 的结构成本为：1 个 presenter、1 个 Animation、1 个 CancellationTokenSource、每帧 1 次路径指标构建和
N 次光束 draw call。额外持久内存保持 `O(1)`，单帧绘制成本为 `O(N)`；实现不创建 phase 集合、每光束 Control、
每光束 animation 或跨帧路径缓存。

## 10. 维护不变量

内部重构必须保持以下不变量：

- BorderBeam 包装内容，不修改内容模板。
- beam presenter 不参与命中测试。
- 默认模板通过 `TemplateBinding` 连接 presenter，不在 BorderBeam 代码中持有模板部件引用。
- 默认模板始终只有一个 beam presenter；多光束共享一个 animation owner。
- 感知接口只暴露边框厚度和圆角。
- content 替换时旧事件订阅必须释放。
- 实例 motion 关闭或 detached 后不持续 invalidation。
- 渐变尾迹连续，圆角转弯处不分段卡顿。
- 非统一圆角只影响边框环裁剪，不直接拆分运动路径。
- public `ColorStops.Percent` 仍按 `0~100` 解释。
- public `Count` 默认值为 `1`，非正值按一个光束渲染；变化只重绘，不重启动画。
- hover 是调用方 Style 对 `IsMotionEnabled` 的组合，不新增事件处理或控件伪类。

## 11. 测试与验证

验证范围：

- `Content` 为普通控件时使用 BorderBeam 自身几何。
- `Content` 实现 `IBorderBeamAwareControl` 时使用控件返回几何，并响应几何变化事件。
- `ColorStops` 优先级高于 `Color`。
- 单色、多 stop、默认渐变和非法 percent clamp。
- `Count` 默认值、模板转发、等距 phase、非正值回退以及实际 draw call 数量。
- 统一圆角和非统一圆角下流光转角连续。
- 实例级 `IsMotionEnabled=false`、自身或祖先不可见、detached 和零尺寸不产生持续 animation tick。
- presenter 不拦截内容点击、焦点和键盘事件。
- Gallery hover、同一 Multiple beams 示例内 `Count=3` / `Count=2` 对照、标准 Avalonia `Border` 自定义容器示例、Customized color 六组预设，以及 Duration `3s` / `6s` / `12s`、BeamSize `100` / `56` / `160`、BorderThickness `2` 线宽对照的 Style、SourceKey、延迟加载结构和四语资源。
- Show on hover、Multiple beams、Custom container、Duration、Size 和 Line width 六个本次新增 ShowcaseItem 使用 AtomUI 当前版本 `v6.1.8` 的 `BadgeText`。
- 文档改动运行 `git diff --check`。
