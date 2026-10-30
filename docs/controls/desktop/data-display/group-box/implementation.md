# GroupBox 桌面版实现原理

本文档描述 GroupBox 桌面版的内部模板接入、自绘边框、Header 缺口几何、Semantic Part marker 映射和维护边界。公共设计与 API 契约见 [GroupBox 桌面版架构设计](overview.md)，Semantic Part 契约见 [GroupBox Semantic Part 契约](semantic-part.md)，Token 语义见 [GroupBox Token 设计](token.md)，变化记录见 [GroupBox Changelog](changelog.md)。

## 1. 实现定位

GroupBox 的实现重点是以较少视觉层级表达 fieldset 式 Header 缺口边框，并支持透明或半透明背景。实现文档只描述模板 part、测量数据、渲染缓存、Semantic Part marker 和失效条件。

GroupBox 不实现交互状态，不管理子项集合，不承担折叠或 Form 逻辑。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/GroupBox/GroupBox.cs`：公共 API、template part 获取、测量 bounds、渲染和失效逻辑。
- `src/AtomUI.Desktop.Controls/GroupBox/GroupBox.SemanticParts.cs`：`[SemanticPart]` 声明（`header` / `icon` / `title` / `content`），只承载声明与空 partial class 块。
- `src/AtomUI.Desktop.Controls/GroupBox/GroupBoxToken.cs`：GroupBox 控件 Token。
- `src/AtomUI.Desktop.Controls/GroupBox/Themes/GroupBoxTheme.axaml`：模板结构、Semantic marker、Header 对齐、TokenResource 引用和默认视觉属性。

## 3. 核心类职责

`GroupBox` 继承 `ContentControl`，负责 Header API、Content 承载和自绘边框。它从模板 part 获取 Header 与 Frame 的 bounds，并在 `Render` 中按有效几何绘制背景和边框。

`GroupBoxToken` 提供内容内边距、Header 外边距、Header 内容内边距和图标间距等组件级语义值。

`GroupBoxTheme.axaml` 负责提供可测量的 Header 结构，不负责遮挡边框线。

## 4. 状态与数据流

渲染状态流：

```text
Header public properties
  HeaderTitle / HeaderIcon / HeaderTitlePosition / font properties
      ↓
Template measure
  PART_Frame bounds
  PART_HeaderContent bounds
      ↓
Render input
  background / border brush / border thickness / corner radius
  header gap rect
      ↓
Draw background + border geometry with gap excluded
```

Header 内容变化、字体变化、icon 可见性变化和标题位置变化会影响 Header bounds。边框厚度、圆角、背景和边框 brush 变化会影响绘制几何。

## 5. 生命周期与模板接入

`OnApplyTemplate` 实际解析两个 part 引用：

- `PART_Frame`（`Border`）→ `_frame`，布局测量与边框 bounds 参考。
- `PART_HeaderContent`（`Decorator`，现为 `Border`）→ `_headerContentContainer`，缺口几何来源。

`PART_HeaderContainer`、`PART_HeaderIconPresenter`、`PART_HeaderPresenter`、`PART_ContentPresenter` 是模板中稳定存在的
节点名称：它们由主题 selector（`Panel#PART_HeaderContainer` 的 `Margin`、`Decorator#PART_HeaderContent` 的对齐）或
`TemplateBinding` 消费，但 **`OnApplyTemplate` 不解析这些名称**，控件代码也不持有它们的引用。这些名称作为 stable
template part 名称属于兼容性不变量（见 [overview.md §7](overview.md#7-兼容性不变量)），但它们的获取路径是视觉树/主题，
不是控件的 `NameScope.Find`。引入 Semantic Part 不改变这一分工。

模板接入后，GroupBox 依赖 layout pass 产生 `PART_HeaderContent` 与 `PART_Frame` 的有效 bounds。bounds 变化后需要使渲染缓存失效并触发重绘。

更换模板时必须清理旧 part 引用，避免旧视觉节点 bounds 被继续用于缺口计算。

`MeasureOverride` 必须测量 `PART_Frame`，而不是只依赖 `ContentControl` 默认内容测量。`PART_Frame` 包含 Header 容器、`PART_ContentPresenter` 和由 Token 注入的内容内边距，因此它的 `DesiredSize` 才能完整表达 GroupBox 自动高度。该设计保证未设置显式高度时，内容增多会推动 GroupBox 高度增长，而不会被 Header 通道、边框或内容内边距挤压。

### 5.1 Semantic Part marker 所有权与模板映射

`GroupBox` 在 `GroupBox.SemanticParts.cs` 中以 `[SemanticPart]` 声明 `header`、`icon`、`title`、`content` 四个非 root Part；
`root` 由生成器隐式加入。marker 落点与节点映射：

| Part | Marker 落点节点 | 节点类型 | `ContractType` |
| --- | --- | --- | --- |
| `header` | `PART_HeaderContent` | `Border` | `Border` |
| `icon` | `PART_HeaderIconPresenter` | `AtomUI.Controls.IconPresenter` | `IconPresenter` |
| `title` | `PART_HeaderPresenter` | `AtomUI.Desktop.Controls.TextBlock` | `Avalonia.Controls.TextBlock` |
| `content` | `PART_ContentPresenter` | `ContentPresenter` | `ContentPresenter` |

marker 使用静态 `Classes.semantic-*="True"` 声明在 `GroupBoxTheme.axaml` 的单一 `ControlTemplate` 内，一次性随模板创建，
不由 C# 在运行时注入，也不随状态增删：

- 四个 Part 的 `RuntimeCreated=false`，因此不携带显式 `SelectorRoute`；生成器把静态根模板 route 规范化为
  `/template/ .<SelectorClass>`。
- `icon` 与 `title` 位于 `header` 节点内部（`Border` → `StackPanel`），但都在第一层模板边界内且 `TemplatedParent` 为
  GroupBox owner，因此单一 `/template/` 路由足够，不使用 `>>` 或第二层 `/template/`。
- GroupBox 没有 `CrossVisualRoot=true`、`RuntimeCreated=true`、`Multiple` 或 `Optional` Part，没有 `.semantic-scope-*` 锚点。

`header` 的承载节点类型在本次改造中由 `Decorator` 提升为 `Border`（`Border : Decorator`），目的是让 `header` 能提供
`Background`：Avalonia `Decorator` 只有 `Child` 与 `Padding`，没有 `Background`。该提升对实现的影响：

- `OnApplyTemplate` 的 `e.NameScope.Find<Decorator>("PART_HeaderContent")` 不变（`Border` 是 `Decorator` 子类）。
- `CalculateHeaderGapBounds` 继续读取该节点的 `Bounds` 与 `TranslatePoint`，缺口宽度语义不变。
- 主题中两个以节点类型开头的 selector（`Decorator#PART_HeaderContent`）需要同步为 `Border#PART_HeaderContent`，否则
  `HeaderContentPadding` 与 `HeaderTitlePosition` 对齐 Setter 会因类型不匹配而失效。这是本次改造必须一起验证的点。

### 5.2 marker 与自绘几何的边界

可见分组边框与背景由 `GroupBox.Render` 通过几何绘制（`_backgroundGeometryCache` / `_borderGeometryCache`）产出，不落在
任何模板节点上：`PART_Frame` 没有设置 `Background` / `BorderBrush`，只投影 `CornerRadius` 与 `Margin`。因此：

- `root` 是 `BorderBrush`、`Background`、`BorderThickness`、`CornerRadius` 的唯一定制入口，四个 marker 节点不参与分组边框
  绘制。
- 给 `header` 设置 `Background` 不会与缺口冲突：缺口通过 `CombinedGeometry` 从边框几何中**排除**，而不是用 Header 背景
  去遮挡边框线，因此不透明 Header 背景既不会还原出边框短线，也不影响 `Background="Transparent"` 的既有契约。
- marker 只向节点的 `Classes` 集合添加稳定字符串，不改变 `MeasureOverride` / `ArrangeOverride` / `Render` 的任何分支，
  也不引入新的失效条件。

## 6. 交互与事件处理

GroupBox 不订阅 pointer、keyboard、focus、drag/drop 或 command 事件。Header 不是按钮，也不是折叠触发器。

与实现相关的事件只来自模板 part 尺寸变化或属性变化。Header bounds、Frame bounds、边框厚度和圆角变化应触发布局或渲染失效。

## 7. 内部算法与关键流程

Header 缺口渲染流程：

1. 绘制内容区域背景，背景允许透明、半透明和普通实色。
2. 从 `PART_HeaderContent` 获取相对 GroupBox 的实际位置和尺寸。
3. 按 `PART_Frame` bounds、`BorderThickness` 和 `CornerRadius` 建立边框几何。
4. 从边框几何中排除 Header gap 几何。
5. 绘制最终边框区域。

该模型避免把 Header 背景色作为遮挡层，因此透明背景下标题区域不会露出多余边框短线。

缓存策略：

- 尺寸、边框厚度、圆角或 Header bounds 变化时重建几何。
- 普通重绘复用缓存。
- DPI 半像素对齐应在几何构建阶段处理，避免边框断裂或重叠。

自动高度测量流程：

1. `MeasureOverride` 测量 `PART_Frame`。
2. `PART_Frame` 内部的 DockPanel 同时测量 Header 容器和内容 Presenter。
3. `PART_ContentPresenter` 将 `Padding` 作为 Margin 应用于内容区域。
4. GroupBox 将模板根节点 `DesiredSize` 返回给父布局，使父容器按完整 fieldset 高度分配空间。

## 8. 资源、性能与 AOT 边界

GroupBox 渲染应使用 Avalonia 绘制 API 和稳定 template part，不使用反射读取模板内部状态。

Header 缺口通过几何排除实现，不增加遮挡用背景层。该方式保持 VisualTree 简洁，也避免透明背景下依赖父背景颜色。

Token 通过动态资源进入 Theme，不应在 `Render` 中主动查找全局资源。

## 9. 维护不变量

内部重构必须保持以下不变量：

- Header 缺口计算基于 `PART_HeaderContent` 的实际 bounds。
- `PART_Frame` 保持边框绘制的布局参考。
- `PART_Frame` 必须参与 GroupBox 测量，自动高度不能退化为只测量裸 Content。
- 不用 Header 背景遮挡边框线来模拟缺口。
- 透明背景、半透明背景和普通背景走同一渲染模型。
- Header 图标隐藏时不保留额外图标占位。
- GroupBox 不新增点击、折叠或选择行为。
- Token 只表达布局和视觉默认值，不承载实例 bounds 或渲染缓存。
- 四个 Semantic marker 只作为静态 `Classes.semantic-*="True"` 存在于 `GroupBoxTheme.axaml` 单一模板内，不由代码在运行时
  增删，也不参与 `Render`、测量或失效条件。
- `PART_HeaderContent` 保持 `Decorator` 兼容类型（当前 `Border`）与 `PART_HeaderContent` 名称，缺口继续以它的 `Bounds` 为准。
- 可见分组边框与背景只由 `GroupBox.Render` 绘制；不得为 Semantic Part 在模板中新增承载 `Background` / `BorderBrush` 的
  边框层来替代自绘几何。

## 10. 测试与验证

验证范围：

- 透明背景下 Header 标题下方无多余边框线。
- 普通背景、半透明背景和复杂父背景下缺口稳定。
- Header `Left`、`Center`、`Right` 三种位置缺口正确。
- 带图标和无图标时 Header 宽度、缺口宽度和内容布局正确。
- 未设置显式高度时，GroupBox 高度包含 Header、内容内边距和内容自身期望高度。
- 不同边框厚度、圆角和 DPI 下边框不断裂。
- Semantic Part 契约：descriptor 数量/顺序/字段、四个静态 marker 的存在与节点类型、四个生成 Style 各自精确命中一个节点、
  任意状态组合下命中数量不变，以及 `header` 背景不还原边框短线，见 [semantic-part.md §7](semantic-part.md#7-兼容性与验证)。
- Header 背景与缺口：在不透明 Header 背景、半透明背景与 `Background="Transparent"` 三种情况下，标题下方都不出现边框短线。
- 控件文档、Token 语义和 Gallery 示例与控件实现一致。
- 文档改动运行 `git diff --check`。
