# Badge 桌面版架构设计

本文档定义 Badge 桌面控件家族的设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，三个 public owner 的完整 Semantic Part 契约见 [Badge Semantic Part 契约](semantic-part.md)，内部实现原理见 [Badge 桌面版实现原理](implementation.md)，Token 语义见 [Badge Token 设计](token.md)，设计和契约变化记录见 [Badge Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Badge` |
| 控件状态 | Stable |

Badge 用于在目标内容附近显示数量、状态点或角标信息。桌面家族包含三个可实例化的 public owner：

| Owner | 职责 |
| --- | --- |
| `CountBadge` | 显示非负数量、溢出文本以及零值可见性。 |
| `DotBadge` | 显示状态点，可在独立模式下附带状态文本。 |
| `RibbonBadge` | 在目标内容边缘显示带文本的 Ribbon。 |

Badge 不负责通知中心、Tooltip、业务状态存储或目标内容本身的主题定义。`DecoratedTarget` 始终由应用拥有，Badge 只负责组合和定位。

## 2. 设计语言

Badge 以“附加信息不改变主体内容语义”为核心。数量、状态点和 Ribbon 都是目标内容的补充视觉；没有目标时也可以作为独立信息单元使用。

| 维度 | CountBadge | DotBadge | RibbonBadge |
| --- | --- | --- | --- |
| 信息密度 | 紧凑数值或溢出文本。 | 最小状态信号，可选说明文本。 | 突出的短文本标签。 |
| 视觉锚点 | 目标边角或独立徽标。 | 目标边角或独立状态行。 | 目标的 Start/End 上边缘。 |
| 状态表达 | 数量、零值、溢出和尺寸。 | 语义状态色或自定义颜色。 | 文本、颜色、位置和显示状态。 |
| 定制原则 | 公开完整 indicator，不公开背景与文本拆分。 | 公开状态点 indicator，不把 standalone Label 纳入契约。 | 公开完整 indicator 与文本 content，不公开折角几何。 |

Semantic Part 表达跨版本稳定的产品职责，不等同于内部 Adorner、MotionActor、Border、Label 或绘制节点清单。

## 3. API 与契约模型

### 3.1 Public owner

`AbstractCountBadge`、`AbstractDotBadge`、`AbstractRibbonBadge` 是共享 API 与状态基类；桌面用户实例化 `CountBadge`、`DotBadge`、`RibbonBadge`。内部 Adorner、指示器和动效类型不是用户可实例化的公共控件，也不是 Semantic Part descriptor owner。

#### CountBadge

| 契约组 | 成员 | 语义与默认值 |
| --- | --- | --- |
| 内容 | `DecoratedTarget` | 可选的被装饰 `Control`，同时是 XAML content property；默认 `null`。 |
| 数值 | `Count`、`OverflowCount`、`IsZeroVisible` | `Count` 默认 `0` 且被约束为非负值；`OverflowCount` 默认 `99` 且被约束为非负值；零值默认不显示。 |
| 外观 | `BadgeColor`、`Size` | `BadgeColor` 接受预设色名或可解析颜色字符串；`Size` 默认为 `Default`，另支持 `Small`。 |
| 定位 | `Offset` | 调整指示器相对目标或独立布局位置；默认 `(0,0)`。 |
| 状态 | `BadgeIsVisible`、`IsMotionEnabled` | 控制徽标可见性与动效；`BadgeIsVisible` 默认 `true`。 |

#### DotBadge

| 契约组 | 成员 | 语义与默认值 |
| --- | --- | --- |
| 内容 | `DecoratedTarget`、`Text` | `DecoratedTarget` 是可选 XAML content property；`Text` 只在独立模式显示；二者默认 `null`。 |
| 状态 | `Status`、`BadgeIsVisible`、`IsMotionEnabled` | `Status` 可选，支持 `Default`、`Success`、`Processing`、`Error`、`Warning`；`BadgeIsVisible` 默认 `true`。 |
| 外观 | `DotColor` | 接受预设色名或可解析颜色字符串；显式颜色作为状态色之外的实例颜色入口。 |
| 定位 | `Offset` | 调整状态点相对目标的位置；默认 `(0,0)`。 |

#### RibbonBadge

| 契约组 | 成员 | 语义与默认值 |
| --- | --- | --- |
| 内容 | `DecoratedTarget`、`Text` | `DecoratedTarget` 是可选 XAML content property；`Text` 为 Ribbon 文本；二者默认 `null`。 |
| 外观 | `RibbonColor` | 接受预设色名或可解析颜色字符串。 |
| 定位 | `Placement`、`Offset` | `Placement` 默认为 `End`，另支持 `Start`；`Offset` 默认 `(0,0)`。 |
| 状态 | `BadgeIsVisible` | 控制 Ribbon 是否存在；默认 `true`。 |

Badge 家族没有控件专属 public 事件；状态变化通过 Avalonia 属性、继承事件和绑定系统表达。

### 3.2 Semantic Parts

Badge 的三个可实例化 owner 各自拥有独立 descriptor，支持范围如下：

| Owner | Parts | 职责摘要 |
| --- | --- | --- |
| `CountBadge` | `root`、`indicator` | 数量状态 owner 与完整数量徽标视觉。 |
| `DotBadge` | `root`、`indicator` | 状态点 owner 与状态点视觉；不公开 standalone 说明文本。 |
| `RibbonBadge` | `root`、`indicator`、`content` | Ribbon owner、完整 Ribbon 表面与文本区域。 |

完整的 Selector、`ContractType`、cardinality、跨根与运行时元数据、逐 Part 定制说明和排除边界见
[Badge Semantic Part 契约](semantic-part.md)。同名 `indicator` 不表示三个 owner 共享运行时节点、状态或 Theme。

## 4. 行为与状态模型

Badge 状态从 public owner 单向投影到运行时视觉：

```text
Public API
  -> owner visibility / mode / effective text and color
  -> runtime Adorner properties
  -> ControlTheme / Measure / Arrange / Render
```

主要状态规则：

- `Count=0 && IsZeroVisible=false` 时，CountBadge 将徽标归一为隐藏；`Count > OverflowCount` 时显示 `<OverflowCount>+`。
- DotBadge 有 `DecoratedTarget` 时只显示状态点；独立模式可以同时显示状态点和 `Text`。
- RibbonBadge 隐藏时只移除 Ribbon 视觉，不隐藏 `DecoratedTarget`。
- Count/Dot 启用退出动效时，indicator 可以在隐藏请求后短暂保留；动效完成后才从宿主移除。
- Dot 在 standalone 与 target mode 间切换时会重建内部 Adorner，但公开 `indicator` 身份不变。
- Semantic marker 不表达 visible、status、placement 或 motion phase；节点存在时 marker 保持不变。完整状态与 Part 数量矩阵见
  [Badge Semantic Part 契约](semantic-part.md)。

## 5. 视觉与主题模型

Badge owner 本身没有 ControlTemplate。三个 owner 在运行时创建内部视觉宿主，并把 public 属性单向投影给该宿主。

| Owner | 无 `DecoratedTarget` | 有 `DecoratedTarget` |
| --- | --- | --- |
| `CountBadge` | 数量视觉作为 owner 的普通视觉和逻辑子树。 | 目标作为 owner 子节点；数量视觉显示在 Avalonia `AdornerLayer`。 |
| `DotBadge` | 状态点与可选文本作为 owner 的普通视觉和逻辑子树。 | 目标作为 owner 子节点；状态点显示在 Avalonia `AdornerLayer`。 |
| `RibbonBadge` | Ribbon 视觉作为 owner 的普通视觉和逻辑子树。 | 目标与 Ribbon 都由 owner 在同一 inline visual tree 中排列，不进入原生 `AdornerLayer`。 |

CountBadge 和 DotBadge 的跨 VisualRoot 模式只改变 indicator 的 visual parent。其 logical/style owner 仍必须是对应 Badge owner，使资源、实例 `Styles` 和 owner-scoped selector 保持可达。

Badge 的默认视觉由三个内部 Token scope 与四个 ControlTheme 共同提供：

| 资源 | 职责 |
| --- | --- |
| `CountBadgeToken` | 数量徽标高度、字体、颜色、Padding、圆角和阴影。 |
| `DotBadgeToken` | 状态点尺寸、颜色、阴影和独立文本间距。 |
| `RibbonBadgeToken` | Ribbon 外伸、垂直偏移、折角尺寸、折角变换、文本 Padding 和行高。 |
| `CountBadgeAdornerTheme.axaml` | 数量 indicator 的模板、尺寸变体和默认视觉。 |
| `DotBadgeAdornerTheme.axaml` | 状态点、独立文本和 target mode 模板。 |
| `DotBadgeIndicatorTheme.axaml` | 状态点绘制所需的默认属性。 |
| `RibbonBadgeAdornerTheme.axaml` | Ribbon content 模板及绘制参数。 |

Token 只表达组件视觉语义，不保存数量、状态、可见性、目标引用或 motion phase。AtomUI 内置主题不得使用 `.semantic-*` 实现默认视觉。

## 6. 控件家族或集成关系

- `CountBadge`、`DotBadge`、`RibbonBadge` 是彼此独立的 public owner，只共享 Badge 产品语义、颜色解析和共享基础设施。
- `AbstractCountBadge`、`AbstractDotBadge`、`AbstractRibbonBadge` 负责跨包状态和生命周期；Semantic descriptor 属于桌面具体类型。
- `DecoratedTarget` 是应用提供的内容，不是 Badge Semantic Part；Badge 不向目标或其内部节点注入 semantic class。
- Count/Dot 使用 Avalonia 12 原生 `AdornerLayer` 作为 target mode 的视觉宿主；Ribbon 始终使用 owner inline visual tree。
- Gallery 和第三方工具只消费 public owner descriptor，不公开 internal Adorner CLR identity。

## 7. 兼容性不变量

- `CountBadge`、`DotBadge`、`RibbonBadge` 的 public API、默认值和 XAML content property 语义不变。
- 三个 owner 分别拥有自己的 descriptor；内部 Adorner 不成为 descriptor owner。
- `root` 不添加 `.semantic-root`；非 root Part 使用唯一 `.semantic-indicator` 或 `.semantic-content`。
- Count/Dot 跨根 indicator 的 visual parent 可以是 `AdornerLayer`，logical/style owner 必须保持对应 Badge owner。
- Ribbon target mode 保持 inline visual tree；隐藏 Ribbon 时必须保留目标内容。
- Ribbon 文本主体、内容区域和折角必须来自同一个布局模型；`Placement`、Token offset、实例 `Offset` 和折角变换不能各自维护互相漂移的坐标假设。
- Ribbon 折角必须作为完整 indicator 渲染契约的一部分保持三角折叠语义，不能退化为被主体覆盖的深色竖条、独立方块或依赖目标内容裁剪的偶然形状。
- `DecoratedTarget`、内部文本拆分、动效节点名称和绘制几何不得升级为隐式公共契约。
- 删除、重命名 Part、修改 selector class、收窄 ContractType 或改变 cardinality 按公共主题破坏性变更处理。
- Semantic Part 不引入运行时反射、VisualTree 全局扫描、额外常驻监听或默认路径视觉对象。

## 8. 专项模型

### 8.1 Semantic Part 集成

Badge 非 root Part 由既有运行时宿主创建，不是 owner 的 template child。完整 Selector 语法、`x:SetterTargetType`、
布局定制和内部节点排除边界统一见 [Badge Semantic Part 契约](semantic-part.md)。颜色、状态、数量、位置、显示规则和动效开关
仍由对应 public API 表达；Semantic marker 不改变 focus、hit testing、automation owner 或可访问名称。

### 8.2 Gallery Preview 映射

Badge Gallery 的 Semantic Parts 内容根包含三个独立 `SemanticPartPreview`，分别对应 `CountBadge`、`DotBadge` 和
`RibbonBadge`。三个 owner 不共享 descriptor、Part 列表或目标解析作用域；页面只复用同一个延迟创建 Tab 生命周期。

- CountBadge Preview 使用带 `DecoratedTarget` 的 target mode，并把当前 owner 对应的具体 runtime Adorner 作为唯一
  `AdditionalRoots` 项。
- DotBadge Preview 使用同样的 target mode 规则，并且不得把共享 `AdornerLayer` 或其他 Badge 的 Adorner 加入解析范围。
- RibbonBadge Preview 使用 owner inline visual tree，不需要 `AdditionalRoots`。
- 非 root 样式示例使用 descriptor 的完整 route。Count/Dot 通过 direct runtime adorner scope 再进入其模板，Ribbon 通过
  direct indicator child 再进入 content 模板；Preview 直接消费 route，不由页面维护平行 selector 规则。
- Semantic Parts Tab 未首次选择前，三个 Preview、三个演示 owner、descriptor item 和跨根查找均不得创建或执行。

Gallery 预览基础设施和多 owner 内容根的通用生命周期见
[Semantic Part Gallery Preview](../../../../gallery/authoring/semantic-part-preview.md)。

## 9. 文档导航与验证策略

关联文档：

- [Badge 桌面版实现原理](implementation.md)
- [Badge Semantic Part 契约](semantic-part.md)
- [Badge Token 设计](token.md)
- [Badge Changelog](changelog.md)
- [Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档 | 运行 `git diff --check` 并确认相对链接存在。 |
| Public API | 覆盖默认值、非负值归一、零值显示、状态色、位置和 target 组合。 |
| Semantic descriptor | 分别验证三个 owner 的 Part 集合、ContractType、cardinality、cross-root 和 runtime metadata。 |
| Runtime marker | 覆盖 standalone、target mode、显示隐藏、零值和 Dot 模式切换后的 marker 数量与 owner 隔离。 |
| Selector | 使用 descriptor `SelectorRoute` 验证实例 Style 和 owner-scoped Style 命中，并验证 `DecoratedTarget` 中嵌套 Badge 不被外层命中。 |
| 生命周期 | 覆盖 AdornerLayer 延迟可用、attach/detach、退出动效取消、目标替换和重复附加。 |
| Gallery | Semantic Parts Tab 保持延迟创建；三个 owner 使用独立 Preview；Count/Dot 只注册各自具体 runtime Adorner，Ribbon 不使用 additional root。 |
| Ribbon 渲染 | 使用 Skia/headless 像素回归验证 Start/End、target/standalone、不同文本长度和颜色下的折角三角区、主体区和背景区。 |
| 性能与 AOT | 验证默认主题不消费 semantic class、无反射/动态代码，并按 overlay 风险执行 NativeAOT 发布检查。 |
