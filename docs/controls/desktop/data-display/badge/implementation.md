# Badge 桌面版实现原理

本文档描述 Badge 桌面控件家族的源码职责、运行时组合、状态流、Adorner 生命周期、Semantic Part 节点映射和维护不变量。公共设计与 API 契约见 [Badge 桌面版架构设计](overview.md)，三个 owner 支持的 Part、Selector 与逐 Part 定制边界见 [Badge Semantic Part 契约](semantic-part.md)，Semantic Part 系统级规则见 [AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)，Token 语义见 [Badge Token 设计](token.md)，变化记录见 [Badge Changelog](changelog.md)。

## 1. 实现定位

Badge 采用 owner-managed runtime visual 模型。`CountBadge`、`DotBadge`、`RibbonBadge` 是 public 状态 owner，但不依赖自己的 ControlTemplate；它们在附加、显示或目标切换时创建内部 Adorner 控件，并由内部 Adorner 的 ControlTheme 产生最终视觉。

Semantic Part 必须复用这条既有生命周期：descriptor 和 marker 只公开稳定定制职责，不增加并行视觉树、运行时查找服务或新的主题属性。具体属性注册、颜色值和动效帧仍以源码和 Theme 为准。

## 2. 源码文件结构

### 2.1 Public owner 与共享状态

| 源码 | 职责 |
| --- | --- |
| `src/AtomUI.Controls/Badge/AbstractCountBadge.cs` | CountBadge public 属性、零值归一、运行时宿主、AdornerLayer retry 和 attach/detach。 |
| `src/AtomUI.Controls/Badge/AbstractDotBadge.cs` | DotBadge public 属性、standalone/target 模式切换、运行时宿主和 AdornerLayer 生命周期。 |
| `src/AtomUI.Controls/Badge/AbstractRibbonBadge.cs` | RibbonBadge public 属性、inline child 管理、测量和排列。 |
| `src/AtomUI.Desktop.Controls/Badge/CountBadge.cs` | 桌面 public owner、Count Adorner factory 和 Token 投影入口。 |
| `src/AtomUI.Desktop.Controls/Badge/DotBadge.cs` | 桌面 public owner、Dot Adorner factory 和 Token 投影入口。 |
| `src/AtomUI.Desktop.Controls/Badge/RibbonBadge.cs` | 桌面 public owner、Ribbon Adorner factory 和颜色投影入口。 |

### 2.2 Internal runtime visual 与 Theme

| 源码 | 职责 |
| --- | --- |
| `AbstractCountBadgeAdorner.cs` / `CountBadgeAdorner.cs` | 数量文本计算、显示隐藏动效、定位、阴影和主题宿主。 |
| `AbstractDotBadgeAdorner.cs` / `DotBadgeAdorner.cs` | 状态点动效、模式布局、定位和主题宿主。 |
| `DotBadgeIndicator.cs` | 使用 `DrawingContext` 绘制状态点与阴影。 |
| `AbstractRibbonBadgeAdorner.cs` / `RibbonBadgeAdorner.cs` | Ribbon 测量、排列、背景与折角绘制。 |
| `CountBadgeToken.cs` / `DotBadgeToken.cs` / `RibbonBadgeToken.cs` | 三种视觉的内部 Token scope。 |
| `CountBadgeAdornerTheme.axaml` | 数量 indicator 模板与尺寸变体。 |
| `DotBadgeAdornerTheme.axaml` | standalone 和 target mode 两套状态点模板。 |
| `DotBadgeIndicatorTheme.axaml` | 状态点绘制属性的默认值。 |
| `RibbonBadgeAdornerTheme.axaml` | Ribbon 文本模板与绘制参数。 |

## 3. 核心类职责

- `AbstractCountBadge`、`AbstractDotBadge`、`AbstractRibbonBadge` 是 public API、有效状态和运行时 child 生命周期 owner。
- `CountBadge`、`DotBadge`、`RibbonBadge` 提供桌面具体类型、internal Adorner factory 和桌面 Token 投影入口；Semantic descriptor 声明属于这三个具体 owner，attribute 不从共享基类继承。
- 三个 `Abstract*BadgeAdorner` 是内部模板和布局宿主，接收 owner 单向投影的状态，不反向拥有 public API。
- `DotBadgeIndicator` 与 Ribbon render helper 只负责绘制，不成为 Semantic Part owner。
- 内部 Adorner 可以承载 semantic marker，但其 CLR 类型、ControlTheme key 和 template part 名称不成为应用 API。

## 4. 状态与数据流

```text
Count / Status / Text / Color / Offset / Placement / Visibility / Motion
  -> public Badge owner
  -> effective visibility, mode, text and parsed brush
  -> internal Adorner Avalonia properties
  -> ControlTheme selector and template bindings
  -> Measure / Arrange / Render / motion
```

- Count owner 把 `Count`、`OverflowCount`、`Size`、`Offset`、`IsMotionEnabled` 绑定到 Adorner；`BadgeColor` 解析后写入 Adorner。Adorner 计算 `CountText`。
- Dot owner 把 `Status`、`Text`、`Offset`、`IsMotionEnabled` 绑定到 Adorner；`DotColor` 解析后写入 Adorner。状态色由 Adorner Theme 映射。
- Ribbon owner 把 `Text`、`Offset`、`Placement` 绑定到 Adorner；`RibbonColor` 解析后写入 Adorner。
- `BadgeIsVisible`、Count 零值规则和 `DecoratedTarget` nullability 决定运行时宿主是否存在以及采用 standalone 还是 target mode。
- Semantic descriptor 不参与状态计算；marker 不随状态反复增删。

## 5. 组合结构模型

### 5.1 CountBadge

```text
CountBadge
  DecoratedTarget?               (owner child)
  CountBadgeAdorner?             (runtime-created)
    Border PART_IndicatorSurface
      Panel RootLayout
        Border BadgeIndicator
        TextBlock BadgeText
```

无 `DecoratedTarget` 时，Adorner 是 CountBadge 的普通视觉和逻辑子节点。有目标时，目标保持为 CountBadge 子节点，Adorner 的 visual parent 切换为 Avalonia `AdornerLayer`。

### 5.2 DotBadge

standalone 模板：

```text
DotBadge
  DotBadgeAdorner               (runtime-created)
    DockPanel RootLayout
      Border PART_IndicatorSurface
        DotBadgeIndicator
      Label Label
```

target mode 模板：

```text
DotBadge
  DecoratedTarget              (owner child)
  AdornerLayer                 (visual host)
    DotBadgeAdorner            (runtime-created)
      DockPanel RootLayout
        Border PART_IndicatorSurface
          DotBadgeIndicator
```

target mode 不创建 Label。`DecoratedTarget` 在 `null` 与非 `null` 之间切换时，owner 销毁旧 DotBadgeAdorner 并按新模式重建，使两套 ControlTemplate 不共享残留状态。

### 5.3 RibbonBadge

```text
RibbonBadge
  DecoratedTarget?             (runtime child)
  RibbonBadgeAdorner?          (runtime-created inline child)
    Panel                      (template root)
      TextBlock PART_LabelPart
```

RibbonBadge 不进入 Avalonia `AdornerLayer`。有目标时，owner 在同一最终区域排列目标和 RibbonBadgeAdorner；无目标时，owner 的期望尺寸来自 RibbonBadgeAdorner。Ribbon 背景和折角由 `AbstractRibbonBadgeAdorner.Render()` 绘制，只有文本是独立 Visual。

### 5.4 Semantic Part 节点映射

公共 Part 含义、存在条件与支持用法由 [Badge Semantic Part 契约](semantic-part.md) 维护；下表只定义 descriptor 与真实运行时节点之间的实现映射。

| Owner | Part | Marker 节点 | ContractType | Cardinality | CrossVisualRoot | RuntimeCreated | Marker 形式 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `CountBadge` | `root` | owner 本身 | `CountBadge` | `Single` | `false` | `false` | 隐式 root，不加 class。 |
| `CountBadge` | `indicator` | `Panel#RootLayout` | `Control` | `Optional` | `true` | `true` | Adorner Theme 中静态 `Classes.semantic-indicator="True"`。 |
| `DotBadge` | `root` | owner 本身 | `DotBadge` | `Single` | `false` | `false` | 隐式 root，不加 class。 |
| `DotBadge` | `indicator` | `DotBadgeIndicator#Indicator` | `Control` | `Optional` | `true` | `true` | 两套 Adorner Theme 模板均静态添加 marker。 |
| `RibbonBadge` | `root` | owner 本身 | `RibbonBadge` | `Single` | `false` | `false` | 隐式 root，不加 class。 |
| `RibbonBadge` | `indicator` | 运行时 `RibbonBadgeAdorner` | `Control` | `Optional` | `false` | `true` | C# 创建时使用生成的 semantic class 常量。 |
| `RibbonBadge` | `content` | `PART_LabelPart` | `Avalonia.Controls.TextBlock` | `Optional` | `false` | `true` | Adorner Theme 中静态 `Classes.semantic-content="True"`。 |

所有非 root Part 都标记 `RuntimeCreated=true`，因为 public owner 的可达视觉由 C# 创建的内部 Adorner 生命周期建立。该元数据描述真实创建模型，也避免把内部 Adorner Theme 误当成 public owner 的静态 ControlTemplate；不能用于掩盖普通静态模板缺失。

Count 的 `indicator` 是实际内容 Panel，Dot 的 `indicator` 是实际状态点。私有 `PART_IndicatorSurface` 是静态 Border，
在语义节点外承载有限动效，隔离应用的 indicator 变换与 Motion 变换；专用 Style 的 route 和 Control 契约保持不变。
Ribbon 的绘制发生在 Adorner 自身，其 `indicator` marker 仍位于运行时 Adorner。

明确排除 Count 的 Border/TextBlock 拆分、Dot standalone Label、`RootLayout`、Ribbon 折角 Geometry、render helper、`DecoratedTarget`、motion phase 和所有 internal CLR identity。

### 5.5 Selector 与跨根 owner

Avalonia 12 的 descendant selector 沿 `ILogical.LogicalParent` 向上匹配；`/template/` 精确读取目标节点的 `TemplatedParent`。Badge 的 runtime Adorner 不是 public owner 的 template child，其内部模板节点的 `TemplatedParent` 是 internal Adorner。因此完整 selector 使用：

```xml
<Style Selector="atom|CountBadge">
    <atom:CountBadgeIndicatorStyle x:SetterTargetType="Control" />
</Style>
```

`CountBadgeIndicatorStyle` 封装 descriptor 的完整 owner-relative route；用户不得复制 logical descendant 或 Adorner 内部模板路径。

CountBadge 和 DotBadge 的 target mode 必须同时维持：

```text
visual parent  = AdornerLayer
logical parent = Badge owner
style owner    = Badge owner
adorned target = Badge owner
```

Attach 顺序与 Avalonia 12 原生 attached Adorner 模型一致：先建立 Adorner 到 Badge owner 的 logical parent，再加入 `AdornerLayer.Children`。由于 child 已有 logical parent，Panel 只接管 visual child。Detach 时先从 `AdornerLayer.Children` 移除，再清理 logical parent 和 `AdornedElement` 关联。

不得通过 VisualTree 全局扫描、复制 owner Styles、新增 host Theme、伪造 `TemplatedParent` 或暴露 internal Adorner 来补偿错误 owner。RibbonBadge 的 Adorner 是 owner inline child，不需要跨根处理。

## 6. 生命周期与模板接入

### 6.1 创建与附加

- 三个 owner 都在首次需要显示时调用 factory；同一模式内复用已有 Adorner。
- Count/Dot target mode 通过 `AdornerLayer.GetAdornerLayer(owner)` 查找宿主。宿主尚未建立时最多重试 30 次，首次使用 Loaded priority，后续间隔约 16ms。
- standalone 模式直接调用 owner 的 child attach helper，建立 visual、logical 和 inheritance owner。
- Ribbon 始终调用 owner 的 child attach helper，不进入 AdornerLayer。
- Semantic class 在节点创建或模板初始化时设置一次。

### 6.2 隐藏与退出动效

- Count 在 `Count=0 && !IsZeroVisible` 时归一 `BadgeIsVisible=false`。为保证退出动效仍显示旧数值，Adorner 可以暂存 `CountText`，动效结束后再移除。
- Count/Dot 启用动效且已加载时，隐藏流程等待退出动效完成再从 owner 或 AdornerLayer 移除；禁用动效或 owner detach 时立即拆除。
- Count/Dot 共用 `BadgeMotionSession` 管理就绪、请求版本和物理移除，Core Motion 管理真实播放及连续替代。
  新显示意图使旧隐藏完成失效；同一 adorned element 和 layer 保持挂载，不通过重挂载制造反向。
  准备覆盖、模板替换、禁用和 detach 按 owner 释放；standalone Dot 保持静态显示，不补播入场。
- Ribbon 没有退出动效；隐藏时立即移除 RibbonBadgeAdorner。有 `DecoratedTarget` 时目标继续保留并参与布局。

### 6.3 Target 切换与 owner detach

- Count 在 standalone 与 target mode 间移动同一 Adorner，并重新建立正确宿主关系。
- Dot 在 `DecoratedTarget` 的 nullability 变化时释放旧 Adorner 并重建；同一模式内替换目标不改变公开 Part identity。
- Ribbon 替换目标时先移除旧 target，再按 `BadgeIsVisible` 决定是否同时附加 Ribbon。
- owner detach 时取消 retry、Loaded 回调、motion binding 和 pending motion，从视觉宿主移除 Adorner，并清除 logical parent、`AdornedElement` 和 owner 引用。
- 重新附加时按当前 public 属性重新建立运行时视觉，不复用失效的跨根关系。

## 7. 交互与事件处理

Badge 没有控件专属 pointer、keyboard 或 command 状态机。输入、focus 和 automation 仍由 Badge owner、`DecoratedTarget` 及其各自基类处理。

- runtime Adorner 不因 semantic marker 获得新的 focus、pointer capture 或 automation owner。
- `DecoratedTarget` 的命中测试、focus 和可访问名称仍由目标自身决定。
- motion 只控制 indicator 的显示隐藏反馈，不改变 public 状态语义。
- 隐藏 Part 从视觉宿主移除后不得保留可访问节点或输入引用。

## 8. 内部算法与关键流程

### 8.1 数量文本

Count Adorner 使用 `Count > OverflowCount ? $"{OverflowCount}+" : $"{Count}"` 计算展示文本。由非零切换到隐藏零值时暂存旧文本，避免退出动效显示错误的 `0`；再次显示零值时主动刷新。

### 8.2 Dot 模式切换

Dot standalone 模板包含 Label，target 模板不包含 Label。nullability 变化必须重建 Adorner，而不是在旧模板节点之间手工搬运状态。两套模板都实现相同 `.semantic-indicator` marker。

### 8.3 Ribbon 布局与绘制

Ribbon target mode 以 target 的最终尺寸为 owner 尺寸；standalone 模式的期望尺寸由文本主体和折角共同决定。Ribbon 的
背景、圆角和折角继续由 `AbstractRibbonBadgeAdorner.Render()` 绘制，不为 Semantic Part 增加新 Visual。

Ribbon 绘制必须先建立同一个布局模型，再从该模型派生文本主体、内容区域和折角几何。模型输入为：

- target 或 standalone 可用尺寸。
- `Placement`，决定 Start/End 锚点和需要关闭的主体底部圆角。
- public `Offset`，只表达实例级整体位置修正。
- Ribbon Token offset，表达 target mode 的外伸量和垂直避让量。
- Ribbon 折角尺寸、折角变换和折角加深量。
- `PART_LabelPart.DesiredSize`，表达文本、行高和 padding 后的主体尺寸。

模型输出必须至少包含：

| 输出 | 职责 |
| --- | --- |
| Body rect | Ribbon 文本主体的真实背景边界，也是内容 TextBlock 的 arrange 基准。 |
| Content rect | 文本内容在主体中的可用区域；由 Theme 中的 TextBlock padding 和 line height 形成。 |
| Fold triangle | 折角暗面三角形，必须与 Body rect 的 Start/End 锚点共享同一条边。 |
| Desired size | standalone 模式的 owner 尺寸；包含主体和折角完整视觉。 |

`Placement=End` 时，主体向 target 的 End 侧外伸；`Placement=Start` 时，主体向 Start 侧外伸。折角不单独反推自身
`Bounds` 后再对齐主体，而是从 Body rect 的外伸侧底边直接生成三角点位。这样可以保证主体、折角和 target 边界共享同一坐标系，
避免折角被主体覆盖成深色竖条、脱离主体成为方块，或在不同文字长度下漂移。

Ribbon Token offset 的两个坐标不得在实现中混用：

| Token 坐标 | 语义 |
| --- | --- |
| `X` | target mode 下主体相对 target 边界的水平外伸距离，也是默认折角宽度来源。 |
| `Y` | target mode 下主体相对 target 上边缘的垂直避让距离，也是默认折角高度来源。 |

如果实现需要把外伸距离、垂直避让和折角尺寸拆成更细的内部值，可以在 Adorner 内部布局模型中命名拆分，但不能改变
既有 public API、TokenResource key、Theme setter 或 Gallery 可见默认外观。只有确认当前 Token 语义不足以表达默认视觉时，
才允许引入新 Token，并同步 `token.md`、Theme 引用、生成资源和可视回归。

折角暗面使用 `RibbonColor` 派生色和 `BadgeRibbonCornerDarkenAmount` 计算，不单独读取 public `RibbonColor` 字符串。
`BadgeRibbonCornerTransform` 若参与折角形状，必须在折角点位或完整 geometry cache key 中体现；不能只把 transform 附在
cached geometry 上，同时继续用 transform 前的 bounds 做布局。影响 Ribbon 布局或折角形状的属性必须触发对应的
measure、arrange 或 render 失效，至少包括 `Text`、`Placement`、`Offset`、`BadgeRibbonOffset`、
`BadgeRibbonCornerTransform`、`BadgeRibbonCornerDarkenAmount` 和 `RibbonColor`。

渲染回归不能只验证 TextBlock 的位置或 owner 的 inline tree。Ribbon 折角必须通过 Skia/headless 捕获后的像素断言验证：
三角暗面区域为加深色，主体背景区域为 Ribbon 主色，三角外背景区保持目标或窗口背景色。验证矩阵至少覆盖 `Start`/`End`、
standalone/target、长短文本、预设色与自定义色，以及 target 右边缘 stretch 布局。

### 8.4 Semantic Style 排查

- Setter 未命中：检查 marker、logical parent、owner scope 和 `x:SetterTargetType`。
- Setter 已命中但视觉不符合预期：检查 Adorner 定位、内部固定尺寸、裁剪、Margin、Transform 和目标 bounds。
- 只在 target mode 失败：检查 visual parent 与 logical/style owner 是否被错误合并为 AdornerLayer。
- 不得用固定 Width/Height、复制 Style 或额外 wrapper 掩盖 owner 关系错误。
- 布局型 Setter 必须同时验证 standalone、target mode、显示隐藏、Count 尺寸档和 Ribbon Start/End。

### 8.5 Gallery 跨根接入

Badge ShowCase 的延迟 Semantic Parts 内容根创建三个独立 Preview。CountBadge 和 DotBadge 使用 target mode 覆盖跨根路径；
每个 Preview 在加载后通过 `AdornerLayer.GetAdornerLayer(owner)` 获取 owner 所在原生层，并只选择满足
`AdornerLayer.GetAdornedElement(child) == owner` 的 runtime Adorner 加入 `AdditionalRoots`。Preview 卸载时清空该集合，
不得缓存脱离 VisualTree 的 Adorner。

不能把整个 `AdornerLayer` 作为 additional root。一个 Window 可以同时承载多个 Badge、焦点 Adorner、验证 Adorner 和其他
视觉层内容；扩大到共享 layer 会破坏 owner scope，并把无关节点带入目标解析。RibbonBadge 的 indicator 和 content 位于 owner
inline visual tree，不建立 additional root。

Gallery Desktop 宿主通过 AtomUI Window 的 Avalonia 12 `VisualLayerManager` 提供原生 `AdornerLayer`。Headless 测试必须使用
等价的 `VisualLayerManager` 宿主验证 Count/Dot target mode，不能用缺少 Adorner 层的裸 Window 代替真实环境，也不能为了测试
在 ShowCase 页面内部新增一层私有 Adorner host。

## 9. 资源、性能与 AOT 边界

Badge Semantic Part 的默认运行时成本仅包括 descriptor 静态数据和既有视觉节点上的静态 class：

- descriptor、Part 名称、selector class 和类型 identity 由 Generator 静态产生，不使用反射发现。
- AXAML marker 在模板初始化时执行一次 `Classes.Set`，不创建 Binding 或持久状态同步。
- Ribbon indicator 在 Adorner factory 中使用生成常量执行一次 class 添加。
- logical parent 调整复用既有 attach/detach 路径，不增加 VisualTree 扫描、布局监听或全局事件。
- Control 包不查询 Semantic Part registry，也不创建 Gallery Preview、highlight Adorner 或 descriptor ViewModel。
- AtomUI 默认 ControlTheme 不使用 `.semantic-*` selector，因此未声明用户 Semantic Style 时不创建对应 class activator。
- 应用声明 Semantic Style 后，Avalonia 只为实际候选节点维护 selector 激活；同一 Part 的 Setter 应合并到一个 Style，并在批量 Badge 场景验证 listener 释放。

运行时逻辑不得使用 `Type.GetType`、`Assembly.GetTypes`、动态代码生成、字符串属性路径或反射扫描寻找 Part。Gallery 对已实例化 marker 的查找属于延迟创建的工具层，不得进入 Badge 包。

## 10. 维护不变量

- descriptor owner 只能是 `CountBadge`、`DotBadge`、`RibbonBadge`，不能是 shared abstract base 或 internal Adorner。
- Count/Dot 只有 `root/indicator`；Ribbon 只有 `root/indicator/content`。
- Count/Dot `indicator` marker 位于实际 Panel/状态点，有限动效位于外侧私有表面；Ribbon indicator 位于 runtime Adorner，content 位于 `PART_LabelPart`。
- 所有非 root Part 保持 `Optional + Selector + RuntimeCreated`；Count/Dot indicator 保持 `CrossVisualRoot=true`。
- Badge public owner selector 使用 descriptor 的 direct-child/template route，不使用 logical descendant、类型前缀 class 或
  internal 类型。Count/Dot runtime adorner 必须携带 `.semantic-scope-indicator`；Ribbon adorner 自身携带
  `.semantic-indicator`。
- Count/Dot target mode 的 visual parent 与 logical/style owner 必须分离，detach 时对称清理。
- Dot standalone 与 target 两套模板必须实现同一个 indicator marker 契约。
- Ribbon 背景与折角继续由 Render 绘制，不为了 Semantic Part 新增视觉节点。
- Ribbon 文本主体、内容区域和折角必须由同一个布局模型派生；折角几何不得通过独立 bounds、独立 transform 和独立偏移与主体拼接。
- Ribbon Token offset 的 X/Y 语义必须保持水平外伸与垂直避让；如实现内部拆分折角尺寸，不能改变现有 TokenResource key 或默认外观。
- 任何影响 Ribbon 折角几何或主体位置的属性变化都必须触发布局或渲染失效；不能依赖旧 cached geometry 偶然复用。
- marker 在节点生命周期内静态存在，不表达 visible、status、placement 或 motion phase。
- `DecoratedTarget`、内部 Label、Count 文本拆分、折角和 motion actor identity 保持非公开。
- 默认 Theme 不消费 semantic class；实现不引入反射、扫描、额外常驻监听或新的默认视觉对象。

## 11. 测试与验证

| 范围 | 必须验证的事实 |
| --- | --- |
| Descriptor | 三个 owner 的 Part 名称、顺序、ContractType、cardinality、customization、cross-root、runtime 和 since 值。 |
| Count standalone | 可见、零值显示、溢出、Small、隐藏与退出动效下 indicator 数量和 marker 稳定。 |
| Count target mode | AdornerLayer 中只有一个 indicator；logical owner 为 CountBadge；owner-scoped Style 可命中；detach 后无残留。 |
| Dot standalone | indicator 存在且 Label 不带 semantic marker；状态、文本和颜色更新不重建 class。 |
| Dot target mode | 两套模板 marker 一致；模式切换重建后 marker、owner 和 selector 仍正确。 |
| Ribbon | standalone/target、Start/End、隐藏保留 target；indicator 与 content 分别命中且不存在跨根 metadata；折角与主体共享坐标模型。 |
| Ribbon 渲染 | Skia/headless 像素回归覆盖折角三角暗面、主体主色、三角外背景、长短文本、预设色/自定义色和 stretch target 右边缘。 |
| Selector | 实例 `.semantic-*` 与 Application owner descendant selector 命中；`/template/` 不作为支持用法。 |
| 生命周期 | AdornerLayer retry、目标替换、快速显示隐藏、motion cancellation、window close 和重复 attach/detach。 |
| 性能 | 默认 Theme 无 `.semantic-*` selector；批量实例 class listener 和 detach 释放符合预算；无 runtime registry 查询。 |
| Gallery | Semantic Parts Tab 首次选择前不创建三个 Preview 或演示 owner；Count/Dot 只登记各自 runtime Adorner；Ribbon 保持 inline；多 Preview 随宿主统一激活、停用和释放。 |
| AOT | Generator 输出为静态代码，NativeAOT 发布不依赖反射或动态代码。 |
