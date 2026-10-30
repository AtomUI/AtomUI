# Tooltip Semantic Part 契约

本文档定义 Tooltip 公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[Tooltip 桌面版架构设计](overview.md)，descriptor、marker 与真实模板节点的映射见
[Tooltip 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts 总览

Tooltip 只有一个 public owner：`ToolTip`，公开 3 个语义键：`root`、`container`、`arrow`（与上游 Tooltip 的
语义部件结构 `root` / `container` / `arrow` 对齐）。声明位于
`src/AtomUI.Desktop.Controls/Tooltip/ToolTip.SemanticParts.cs` partial 文件。

`container` 与 `arrow` 均声明 `CrossNestedOwners=true`：`ToolTip` 的控件模板把自己的内容直接交给共享原语
`ArrowDecoratedBox#PART_ArrowDecorator`（带有 `.semantic-scope-arrow-decorated-box` scope 锚点），两个 marker 由
`ArrowDecoratedBox` 自身模板携带。`/template/` 链不能直接跨越嵌套 owner 进入 `ArrowDecoratedBox` 模板，因此这两个
Part 由生成 Style 借助 scope 路由从 `ToolTip` 穿透到共享模板命中，解析器同时被允许跨越嵌套 owner 边界。

各 Part 的 Selector、SelectorRoute、ContractType 以 `ToolTip.SemanticParts.cs` 为准。

### 1.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ToolTip` |
| Part | `root` |
| Selector | 不适用（owner 本身，无 marker） |
| SelectorRoute | 不适用 |
| ContractType | `ToolTip` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ToolTip` 控件本体 |
| 职责 | 工具提示的根语义区域，承载内容、弹层打开状态与主题入口的组织边界 |
| 相关 API | `Content`、`IsMotionEnabled` |
| 相关 Token | `ToolTipToken` / SharedToken |
| 稳定性 | stable since 6.2.0 |

### 1.2 `container`

| 字段 | 值 |
| --- | --- |
| Owner | `ToolTip` |
| Part | `container` |
| Selector | `.semantic-container` |
| SelectorRoute | `/template/ .semantic-scope-arrow-decorated-box /template/ .semantic-container` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `Border#PART_ContentDecorator`（共享 `ArrowDecoratedBox` 模板） |
| 职责 | 内容盒，承载内边距、背景、圆角与文本样式，内部 `ContentPresenter` 展示用户内容 |
| 相关 API | `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`（模板绑定）、`Content` |
| 相关 Token | `ToolTipToken.ToolTipBackground`、`ToolTipToken.ToolTipCornerRadius`、`ToolTipToken.ContentPadding` |
| 稳定性 | stable since 6.2.0 |

### 1.3 `arrow`

| 字段 | 值 |
| --- | --- |
| Owner | `ToolTip` |
| Part | `arrow` |
| Selector | `.semantic-arrow` |
| SelectorRoute | `/template/ .semantic-scope-arrow-decorated-box /template/ .semantic-arrow` |
| ContractType | `ArrowIndicator` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ArrowIndicator#PART_ArrowIndicator`（共享 `ArrowDecoratedBox` 模板） |
| 职责 | 指向锚定控件的箭头指示器 |
| 相关 API | `IsArrowVisible`、`ArrowSize`、`Background`（`FilledColor` 模板绑定） |
| 相关 Token | `ArrowDecoratedBoxToken.ArrowStrokeColor`、`ArrowDecoratedBoxToken.ArrowStrokeThickness`、`ArrowDecoratedBoxToken.ArrowSize` |
| 稳定性 | stable since 6.2.0 |

## 2. Part 说明

- `root`：`ToolTip` owner 本身，是内容、打开状态与主题入口的组织边界。`root` 不生成 Semantic Style，
  owner 级 Setter 写在外层普通 Style 上。
- `container`：内容盒 `Border`。`ToolTipTheme.axaml` 通过 `atom|ArrowDecoratedBox#PART_ArrowDecorator` selector
  把 `FontSize`、`MaxWidth`、`Foreground`、`MinHeight`、`Padding`、`CornerRadius` 等 token 值挂到
  `ArrowDecoratedBox`，由模板绑定流入内容盒；`Background` 由 `ToolTip` 的 `ToolTipBackground` 模板绑定流入。
- `arrow`：箭头指示器 `ArrowIndicator`，填充色来自 `ArrowDecoratedBox` 的 `Background` 模板绑定，描边与尺寸来自
  `ArrowDecoratedBoxToken`。箭头不存在于 `ToolTipTheme.axaml` 自己的模板节点中，只通过共享
  `ArrowDecoratedBox` 模板间接呈现。

以下节点不属于任何 Semantic Part：

- `ToolTip` 模板中的 `ContentPresenter#PART_ContentPresenter` 是稳定 template part，但承载的用户内容子树只属于
  `container` 的内容流，不单独发布 marker。
- `ArrowDecoratedBox#PART_ArrowDecorator` 自身只携带 scope 锚点 `.semantic-scope-arrow-decorated-box`，
  该 class 只用于生成 Style 的 owner-relative 路由，不是 Semantic Part marker。
- 共享 `ArrowDecoratedBox` 模板中的布局节点（`DockPanel#ContentLayout`、`Panel#ArrowPositionLayout`、
  `LayoutTransformControl#PART_ArrowIndicatorLayout`）属于视觉层级，不发布 marker。

## 3. Selector 用法

生成的 Semantic Style 类型命名为 `<Owner><PartPathPascalCase>Style`，即 `ToolTipContainerStyle` 与
`ToolTipArrowStyle`（命名空间 `AtomUI.Theme.Styling`，AXAML 命名空间 `https://atomui.net`）。`root` 不生成
Style 类型。

推荐写法（owner 嵌套 Style + 语义 class）：

```xml
<StackPanel.Styles>
    <Style Selector="atom|ToolTip.semantic-styles-demo">
        <atom:ToolTipContainerStyle x:SetterTargetType="Border">
            <Setter Property="Background" Value="#D9F0F0F0" />
            <Setter Property="CornerRadius" Value="16" />
        </atom:ToolTipContainerStyle>
        <atom:ToolTipArrowStyle x:SetterTargetType="atom:ArrowIndicator">
            <Setter Property="FilledColor" Value="#1890FF" />
        </atom:ToolTipArrowStyle>
    </Style>
</StackPanel.Styles>
<atom:ToolTip Classes="semantic-styles-demo" Content="Tip text" />
```

两个部件均声明 `CrossNestedOwners=true`，由生成 Style 的 `Nesting()` 跨越嵌套 owner 模板边界命中目标，用户使用
时只写上述形式。不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `Border.semantic-container` 等 `ContractType` 写入 Part 身份 selector。
- 直接复制 `/template/ .semantic-scope-*` route；scope class 只用于生成 Style 的 owner-relative 路由。

## 4. 状态与数量语义

`root` 与 `container` 是静态模板节点，`Single`：任何状态变化（打开/关闭、`PresetColor`/`Color` 切换、动效）只
改变有效视觉值，不增删 marker。

`arrow` 为 `Optional`（零或一）：marker 节点始终存在于共享 `ArrowDecoratedBox` 模板中，但箭头的有效可见实例是条件性的：

- `IsArrowVisible=false` 时 `LayoutTransformControl#PART_ArrowIndicatorLayout` 折叠，箭头不参与渲染。
- `ArrowDecoratedBox` 处于 `:bordered` 状态时共享主题隐藏 `Panel#ArrowPositionLayout`，箭头同样不可见。

主题结构解析器只把 effectively visible、尺寸有效且祖先 opacity 大于 0 的候选视为实例，因此箭头被隐藏时该 Part
没有实例，配合 `Optional` 不要求任何内置模板变体保持箭头可见。状态切换不改变 marker 身份。

## 5. 定制边界

以下区域不属于 Tooltip Semantic Part：

- **弹层宿主与定位**：Tooltip 的物理显示由共享 Popup 钉住打开契约承担，`root`/`container`/`arrow` 不覆盖
  弹层定位、关闭时机或钉住状态。
- **共享原语的其他消费方**：`.semantic-container` 与 `.semantic-arrow` marker 位于共享
  `ArrowDecoratedBoxTheme.axaml`，其他使用 `ArrowDecoratedBox` 的控件（DatePicker、TimePicker 等）共享同一
  marker 节点；对 marker 的结构性修改属于共享原语主题契约，必须同时评估全部消费方。
- **用户内容子树**：`ContentPresenter` 内的用户内容、任何 `ContentTemplate` 内部节点不发布 marker，
  Selector 不得穿过用户内容继续匹配。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态订阅。
Semantic Style 服从 Avalonia 原生属性优先级。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`、改变 cardinality，或让任一内置模板变体
缺少 marker，均属于公共主题契约变更。marker 位于共享 `ArrowDecoratedBoxTheme.axaml`，任何变更必须同步评估
`ArrowDecoratedBox` 的家庭消费者。

验证至少覆盖：

- descriptor 只包含 §1 的 3 个键，字段值与本文一致。
- `ToolTipTheme.axaml` 的 `ArrowDecoratedBox` 节点携带 `.semantic-scope-arrow-decorated-box` 锚点；
  `ArrowDecoratedBoxTheme.axaml` 的内容盒与箭头节点分别携带 `.semantic-container` / `.semantic-arrow` marker。
- 生成的 `ToolTipContainerStyle` / `ToolTipArrowStyle` 可编译并跨越嵌套 owner 模板边界命中最低 public
  `ContractType`（见 `tests/AtomUI.Desktop.Controls.Tests/Tooltip/ToolTipSemanticPartTests.cs`）。
- `IsArrowVisible` 与 `:bordered` 状态不改变 marker 身份；箭头隐藏时解析结果为零实例，与 `Optional` 一致。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Gallery Semantic Parts Tab 中 3 个 Part 均可解析高亮。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。