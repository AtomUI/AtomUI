# BorderBeam 多光束与悬停展示设计

> 状态：2026-09-08 已实现；控件与 Gallery 全量测试、Desktop Gallery 构建和 LLMS 一致性验证已通过。
> 视觉走查由本地调试 Skill 输出的当前工作区绝对二进制身份与用户截图共同确认；CUA 不接受裸 Mach-O
> 绝对路径，整个过程未回退到同名应用或 bundle id。
>
> 上游基线：Ant Design `6.6.3`，重点参考 `BorderBeam.tsx`、`BorderBeamEffect.tsx`、
> `style/index.ts`、`util.ts` 与 `demo/hover.tsx`。

当前实现契约见 [BorderBeam 桌面版架构设计](../../controls/desktop/other/border-beam/overview.md) 与
[BorderBeam 桌面版实现原理](../../controls/desktop/other/border-beam/implementation.md)。上游版本和源码证据见
[Ant Design 6.6.3 release](https://github.com/ant-design/ant-design/releases/tag/6.6.3)、
[`BorderBeam.tsx`](https://github.com/ant-design/ant-design/blob/6.6.3/components/border-beam/BorderBeam.tsx) 和
[`demo/hover.tsx`](https://github.com/ant-design/ant-design/blob/6.6.3/components/border-beam/demo/hover.tsx)。

## 1. 结论

AtomUI 保留现有 `BorderBeam` 包装控件和单一 `BorderBeamPresenter` 渲染架构，并补充两项能力：

- 通过 public `Count` 控制沿同一边界均匀分布的光束数量；
- 通过现有 `IsMotionEnabled` 与 `:pointerover` 样式组合表达“仅悬停时显示”，不新增
  `ShowOnHover` 公共属性。

多光束共享一套动画时钟、边框环裁剪、渐变和路径度量。`Count` 只增加同一次 render 中的绘制次数，
不按光束数量增加 Visual、Presenter、timer 或 cancellation owner。

本设计保持现有 `BorderThickness`、`Outset`、`IsMotionEnabled`、Token、Template Part 和命中测试语义。
独立 `LineWidth`、系统 reduced-motion 映射、宿主几何自动发现以及跨悬停的动画相位恢复不属于本设计。

## 2. 上游语义基线

Ant Design `6.6.3` 的 BorderBeam 由宿主接入、单个 effect 和样式算法三层组成：

- BorderBeam 获取唯一可引用子元素，读取宿主实际边框宽度，并把 effect portal 到宿主元素；
- 每个 effect 沿相同的圆角矩形 `offset-path` 运动；
- `count` 个 effect 使用相同 `duration`，通过等距负延迟形成均匀相位；
- mask 只暴露边框环，光束本体与内容命中测试隔离；
- hover 示例只通过样式切换 effect 的透明度和 animation play state，不形成控件属性。

AtomUI 不复制 DOM、portal、CSS mask 或 `offset-path` 结构，只保持以下可观察语义：

- 光束沿连续圆角边界运动；
- 多光束在同一周期内等距分布；
- 光束不参与输入命中测试；
- hover 是可组合的展示策略，不是 BorderBeam 固有状态；
- 默认单光束用法和既有实例配置保持兼容。

## 3. 范围与非目标

### 3.1 范围

- 增加 `BorderBeam.Count` StyledProperty 和 CLR wrapper。
- 将 `Count` 通过默认 ControlTheme 转发给 internal presenter。
- 在一套 `Progress` 动画中计算并绘制多个等距 phase。
- 复用一次 render 内的路径度量、边框环 clip 和 brush。
- 在 Gallery 增加 `Multiple beams` 与 `Show on hover` 稳定示例；前者在同一示例中成对展示 `Count=3` 与
  `Count=2`。
- 同步控件测试、Gallery 测试、控件文档和 LLMS 来源。

### 3.2 非目标

- 不新增 `ShowOnHover`、`Trigger`、`PlaybackState` 或类似公共 API。
- 不新增独立 `LineWidth`，不拆分现有 `BorderThickness` 的边界定位与光束宽度语义。
- 不改变 `IsMotionEnabled` 与全局 `SharedToken.EnableMotion` 的既有解耦关系。
- 不让 Card、Button、GroupBox 或输入壳体自动实现 `IBorderBeamAwareControl`。
- 不改成 Adorner、attached behavior、动态视觉节点或每光束一个 presenter。
- 不在本设计中修正 `BorderBeamColorStop` 单项属性变化订阅。
- 不承诺离开再进入 hover 后从完全相同的动画 phase 继续。

## 4. 用户体验契约

### 4.1 多光束

- `Count=1` 与现有 BorderBeam 的默认视觉、周期和资源占用模型一致。
- `Count=N` 时显示 N 个使用相同颜色、尺寸、透明度、边框几何和周期的光束。
- 相邻光束沿路径相隔 `1/N` 周期，运行期间保持等距。
- 修改 `Count` 立即使 presenter 重绘，不重建 ControlTemplate，也不重启动画时钟。
- `Count<1` 不抛异常，其有效值按 `1` 处理。
- 边界无有效尺寸、`BeamSize<=0`、光束不可见或边框厚度不可见时，保持现有不渲染规则。

### 4.2 悬停展示

- 未悬停时不显示流光，也不让隐藏流光持续产生 UI 线程 animation invalidation。
- 指针进入 BorderBeam 的内容区域后显示并启动流光；离开后隐藏并停止。
- 内容控件继续拥有 pointer、keyboard、focus、command 和业务状态。
- `PART_BeamPresenter` 始终保持 `IsHitTestVisible=false`。
- 悬停样式是调用方或 Gallery 示例的组合策略，不改变没有该样式类的实例。

Gallery 的标准组合采用：

```xml
<Style Selector="atom|BorderBeam.show-on-hover">
    <Setter Property="IsMotionEnabled" Value="False" />
</Style>

<Style Selector="atom|BorderBeam.show-on-hover:pointerover">
    <Setter Property="IsMotionEnabled" Value="True" />
</Style>
```

该样式依赖 BorderBeam 根控件的 `:pointerover`，不穿透内容控件模板，也不选择 internal presenter。

## 5. Public API 与有效状态

新增公共契约：

```csharp
public static readonly StyledProperty<int> CountProperty =
    AvaloniaProperty.Register<BorderBeam, int>(nameof(Count), 1);

public int Count
{
    get => GetValue(CountProperty);
    set => SetValue(CountProperty, value);
}
```

`Count` 的 public 默认值为 `1`。渲染层计算：

```text
effectiveCount = max(1, Count)
```

无效输入在 effective-state 边界归一，不通过异常、模板重建或额外状态对象处理。`Count` 是实例行为参数，
不属于 Design Token；主题可以通过普通 Style 设置它，但默认主题不为它增加 selector 或资源 key。

既有属性语义保持不变：

- `Duration` 是所有光束共享的完整路径周期；
- `BeamSize` 是每个光束共享的高光矩形基准尺寸；
- `ColorStops`、`Color` 和默认主题渐变按既有优先级解析一次，并由所有光束共享；
- `Outset`、effective border thickness 和 effective corner radius 对全部光束使用同一边界；
- `IsMotionEnabled=false` 隐藏 presenter 并停止该实例的持续动画。

## 6. 架构与职责

```text
BorderBeam (public state owner)
└─ ControlTheme
   └─ Grid
      ├─ ContentPresenter#PART_ContentPresenter
      └─ BorderBeamPresenter#PART_BeamPresenter
         ├─ one Progress value
         ├─ one Animation
         ├─ one CancellationTokenSource
         ├─ one border-ring clip per render
         └─ N phase samples and draw calls
```

| Owner | 输入 | 输出 | 不负责 |
| --- | --- | --- | --- |
| `BorderBeam` | public API、Content、感知几何 | effective geometry、模板绑定值 | 光束路径采样和绘制 |
| `BorderBeamPresenter` | effective geometry、颜色、motion、`Count`、`Progress` | 边框环内的 N 个光束 | 内容交互、宿主业务状态 |
| `BorderBeamPathSampler` | render bounds、path corner radius、phase | 路径点和切线 | 动画生命周期和主题资源 |
| `BorderBeamTheme.axaml` | public/internal Avalonia 属性 | 内容层与 presenter 组合 | 几何推导和动态创建视觉节点 |

`Count` 通过默认 ControlTheme 的 `TemplateBinding` 从 public control 单向传入 presenter，并在 presenter 侧进入
`AffectsRender<BorderBeamPresenter>`。BorderBeam 代码不获取或缓存模板部件引用，也不为 `Count` 增加第二份
effective state。

## 7. 多光束算法

### 7.1 输入与坐标系

- `Progress`：`[0, 1)` 的归一化周期进度；
- `Count`：public 整数输入，通过 `effectiveCount` 归一；
- `renderBounds`：应用 DPI-aware border thickness 与 `Outset` 后的 presenter 局部 DIP 边界；
- `BeamSize`：DIP 单位的光束矩形边长及运动路径圆角输入；
- 路径输出：presenter 局部坐标中的位置和单位切线。

### 7.2 相位分布

第 `i` 个光束的 phase 为：

```text
phase(i) = NormalizeProgress(Progress + i / effectiveCount)
where i ∈ [0, effectiveCount)
```

它与相同周期上的等距负 animation delay 等价，同时避免 N 套时间线。`Count` 改变只改变当前 render 的 phase
集合；共享 `Progress` 保持连续。

### 7.3 Render 流程

一次 `Render` 按以下顺序执行：

1. 验证 motion、bounds、beam size、opacity 与有效 border thickness。
2. 计算 render-scale-aware border thickness 和应用 `Outset` 后的 `renderBounds`。
3. 构建一次 outer/inner combined border-ring geometry。
4. 构建一次渐变 brush 和光束局部 bounds。
5. 建立当前 bounds 与 `BeamSize` 对应的路径度量。
6. push 一次 opacity 和 border-ring geometry clip。
7. 对每个 phase 采样路径点与切线，push transform 并绘制同一个光束矩形。

路径度量包含圆角矩形关键点、路径周长和退化边界的 fallback point。直线与圆角椭圆弧的累计消耗在每次 phase
采样时基于这份指标进行。指标在一次 render 中只构建一次，所有光束共享；不引入跨帧实例缓存或跨控件全局状态。

## 8. 动画与生命周期

Presenter 继续拥有唯一 `Animation` 和 `CancellationTokenSource`：

- attach 且满足有效可见性、有效尺寸、正 `Duration` 与 motion enabled 时运行；
- detach、实例 motion disabled 或其他既有停止条件成立时取消并释放 CTS；
- 持续 Avalonia 动画继续使用 `PlaybackBehavior.OnlyIfVisible`，隐藏祖先下不持续运行；
- `Count` 变化不创建、取消或重建 animation；
- `Duration` 变化继续按既有规则重启动画；
- hover 示例通过切换 `IsMotionEnabled` 复用同一 acquire/release 路径。

纯装饰动画恢复时允许从初始 phase 重启；本设计不把暂停位置提升为业务状态，也不增加 wall-clock owner。

## 9. 资源、性能与 AOT 边界

- Visual 数量与 `Count` 无关，默认模板仍只有一个 internal presenter。
- animation、CTS、brush、clip 和路径度量 owner 数量与 `Count` 无关。
- 单帧绘制时间复杂度为 `O(Count)`，额外持久内存为 `O(1)`。
- 不为每个光束分配集合、闭包、Animation、Transform 对象或 Control。
- 高 `Count` 会线性增加 draw call；公共契约不承诺高数量光束的固定帧预算。
- 不新增反射、运行时类型发现、动态程序集扫描或 source-generator 注册要求。
- `CountProperty` 和 presenter property 使用静态 Avalonia 属性注册，对 trimming 与 NativeAOT 无新增风险。

## 10. 兼容性与定制边界

- `Count` 是 additive public API，默认值 `1` 保持已有实例视觉。
- `PART_ContentPresenter`、`PART_BeamPresenter` 和默认 ControlTheme key 保持不变。
- `BorderThickness` 继续同时参与边框环宽度和 `Outset=null` 时的默认外扩，不引入 Ant Design 独立
  `lineWidth` 的语义拆分。
- `IsMotionEnabled` 继续是实例级开关，默认主题不绑定全局 motion 配置。
- `IBorderBeamAwareControl` 继续是显式 opt-in 几何契约；不通过反射或宿主模板 part 推断边界。
- 应用可以用 Style 设置 `Count` 或组合 hover 行为，但不能依赖 internal presenter 类型或其属性。
- 替换 ControlTemplate 的应用必须继续提供内容层和不参与命中测试的 beam presenter，并转发 `Count`。

## 11. Gallery 与文档契约

Gallery 增加两个独立、可复制的稳定示例：

- `border-beam-multiple-beams`：在同一纵向示例中以 `Count=3` 和 `Count=2` 两张卡片展示数量变化与默认周期
  共享关系；
- `border-beam-show-on-hover`：展示 class、`:pointerover` 与 `IsMotionEnabled` 的组合。

现有文档级示例同步补稳定 `SourceKey`，但不因示例新增而改变页面滚动、延迟挂载或 Masonry 语义。
所有标题、说明和卡片文案进入 BorderBeam Gallery 的本地化资源。

代码落地时同步以下当前设计来源：

- `docs/controls/desktop/other/border-beam/overview.md`：加入 `Count` 公共语义、多光束状态摘要和 hover
  组合边界；
- `implementation.md`：加入单时钟、多 phase render、路径度量复用和生命周期不变量；
- `changelog.md`：记录 API、Theme binding、Gallery 和实现结构变化；
- `token.md`：仅确认 `Count` 不属于 Token，不增加新 Token；
- LLMS 产物通过生成器更新，不手工编辑。

## 12. 验证要求

### 12.1 纯逻辑

- `Count=1` 的 phase 只包含当前 `Progress`。
- `Count=2/3/8` 的 phase 均匀分布。
- `Progress` 接近 `1` 时各 phase 正确归一到 `[0, 1)`。
- `Count<=0` 的 effective value 为 `1`。
- 同一组路径度量对不同 phase 返回连续位置和切线。

### 12.2 控件与主题

- public `Count` 默认值、Style/binding 语义和模板转发正确。
- 修改 `Count` 不替换 presenter，不新增 animation owner。
- 单光束既有几何、颜色和 motion 测试继续通过。
- 多光束与 uniform/non-uniform corner radius、Outset、单色和多 stop 组合正确。
- presenter 继续不参与命中测试。
- hover 样式在未悬停时隐藏并停止，在悬停时显示并启动。
- 隐藏祖先、detach 和重新 attach 继续满足持续视觉工作生命周期。

### 12.3 Gallery、文档与产品走查

- Gallery 的多光束和 hover 示例具有稳定 `SourceKey`，多光束示例同时包含 `Count=3` 与 `Count=2`，四套
  本地化资源完整。
- Light/Dark 与不同 render scale 下，光束数量、边框贴合和圆角连续性可辨认。
- hover 卡片左、中、右区域都能触发根控件 `:pointerover`，卡片内容交互不受影响。
- Gallery 页面测试、控件定向测试、主题契约检查、LLMS verify 和 `git diff --check` 通过。

## 13. 设计审计

```text
Change type: additive public API + Gallery composition example
Control: BorderBeam
Package and namespace: AtomUI.Desktop.Controls / AtomUI.Desktop.Controls
Primary responsibilities: 内容包装、有效边界解析、持续装饰性边框流光
State owners: BorderBeam owns public/effective state; BorderBeamPresenter owns Progress/Animation/CTS
Lifecycle pairs: aware-content subscribe/unsubscribe; ColorStops collection switch; presenter attach/detach; animation start/cancel
Template parts: PART_ContentPresenter and PART_BeamPresenter
Data flow: public API -> effective geometry/theme values -> presenter -> path samples -> clipped draw calls
API risk: L1, Count defaults to 1; hover adds no API
Theme risk: Count TemplateBinding only; existing key, parts, selectors and Tokens remain
AOT risk: none beyond static Avalonia property registration
Required regressions: phase distribution, one-clock invariant, template forwarding, hover hit testing, effective visibility
Out of scope: LineWidth split, reduced-motion policy change, built-in aware hosts, ColorStop item mutation, adorner rewrite
```
