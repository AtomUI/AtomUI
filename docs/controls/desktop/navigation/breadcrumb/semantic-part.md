# Breadcrumb Semantic Part 契约

本文档定义 `Breadcrumb` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。`Breadcrumb` 的整体设计见
[Breadcrumb 桌面版架构设计](overview.md)，实现与生命周期见 [Breadcrumb 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

`Breadcrumb` 的公开语义结构与参考实现的 Semantic DOM 结构对齐：`root`、`item`、`separator`。其中 `root` 是隐式
owner，`item` 表示每个导航条目容器，`separator` 表示条目容器之间的兄弟分隔元素。参考实现的 DOM 是条目与分隔符
交替的平铺列表，因此 `separator` 不是 `item` 的组成部分：N 个条目产生 N-1 个分隔符，每个分隔符尾随其前一条目，
最后一条目没有分隔符。

## 1. Semantic Parts

| Part | AtomUI 节点 | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `root` | `Breadcrumb` owner | owner | `Breadcrumb` | `Single` | `Root` | `false` | `false` |
| `item` | 条目容器 | `> .semantic-item` | `BreadcrumbItem` | `Multiple` | `Selector` | `false` | `true` |
| `separator` | 条目容器之间的兄弟分隔元素 | `> .semantic-separator` | `ContentPresenter` | `Multiple` | `Selector` | `false` | `true` |

`root` 不声明 `.semantic-root` marker，也不生成独立 Style type。`item` 和 `separator` 都是运行时创建的语义标记：
`item` 由 `Breadcrumb` 在 `CreateContainerForItemOverride` 与 `PrepareContainerForItemOverride` 中通过生成的
`BreadcrumbSemanticParts.ItemClass` 挂到每个 `BreadcrumbItem` 容器上；`separator` 由 `Breadcrumb` 在分隔符创建路径
中通过 `BreadcrumbSemanticParts.SeparatorClass` 挂到每个运行时创建的兄弟分隔 `ContentPresenter` 上。分隔符是
`Breadcrumb` 的逻辑子级（供 `> .semantic-separator` 路由匹配）、items panel 的视觉子级（参与布局），不进入 panel 的
`Children` 集合，避免污染条目生成器基于面板位置的容器索引。`separator` 的标记数量为条目数量减一，每个分隔符尾随
其前一条目，最后一条目没有分隔符。

内置主题不依赖静态 `.semantic-*` marker：`BreadcrumbTheme.axaml` 与 `BreadcrumbItemTheme.axaml` 都不包含静态
semantic class，语义契约完全由 runtime marker 与生成的 `Style` 类型表达。分隔符是运行时创建的兄弟节点，无法被
`^ /template/` 主题 selector 覆盖，因此默认前景色与间距由 `Breadcrumb` 通过 `TokenResourceBinder` 控件 Token 绑定
提供（`SeparatorColor` / `SeparatorMargin`），而不是模板字面值，从而允许应用 Semantic Style 覆盖该颜色。

带导航能力的条目（`IsNavigateResponsive=True`）的链接前景色由主题 selector `^ /template/ ContentPresenter#Content`
提供（绑定 `LinkColor` Token），直接落在内容呈现器上——对齐参考实现的链接锚点着色规则（`.ant-breadcrumb-item a`）。因此
item 级 Semantic Style（如 `Foreground`）不会改变链接条目的文字颜色：链接条目始终使用 `LinkColor`，只有普通条目的
文字颜色可被 item 样式定制。悬浮时的 `LinkHoverColor` 同样作用在内容呈现器上。

## 2. Selector 用法

应用侧使用 owner-scoped selector 和生成的 Style 类型，不手写 `/template/` 路径：

```xml
<Style Selector="atom|Breadcrumb.semantic-demo">
    <Setter Property="Padding" Value="8" />

    <atom:BreadcrumbItemStyle x:SetterTargetType="atom:BreadcrumbItem">
        <Setter Property="Foreground" Value="#1890ff" />
    </atom:BreadcrumbItemStyle>

    <atom:BreadcrumbSeparatorStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Foreground" Value="#73000000" />
    </atom:BreadcrumbSeparatorStyle>
</Style>
```

`BreadcrumbItemStyle` 和 `BreadcrumbSeparatorStyle` 位于 `AtomUI.Theme.Styling` 命名空间，由语义生成器根据
`Breadcrumb.SemanticParts.cs` 生成。`ContractType` 只给出 Setter 所需的最低稳定类型，不参与 selector 匹配。

## 3. Root 定制边界

`root` 的定制直接作用于 `Breadcrumb` owner 自身。`Breadcrumb` 继承 `TemplatedControl` 的 `Background`、
`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding` 样式属性作为 root frame 定制入口，用于对齐参考实现
对 root 的边框、背景与内边距定制（对应参考实现 `styles.root` 的 `borderColor`、`borderWidth`、`backgroundColor`、
`padding`、`borderRadius`）：

| 属性 | 类型 | 作用 |
| --- | --- | --- |
| `Background` | `IBrush?` | root 背景填充。 |
| `BorderBrush` | `IBrush?` | root 边框画笔。 |
| `BorderThickness` | `Thickness` | root 边框厚度。 |
| `CornerRadius` | `CornerRadius` | root 圆角。 |
| `Padding` | `Thickness` | root 内边距。 |

边界约定：

- `Padding + BorderThickness` 构成 frame inset，`MeasureOverride` 用 inset 收缩测量约束并外扩 desired size，
  `ArrangeOverride` 用 inset 收缩排布区域，`Render` 通过 `BorderRenderHelper` 绘制背景、边框与圆角。
- root frame 属性直接继承 `TemplatedControl` 的同名成员，默认值为零/空值，默认外观与布局不变。
- root frame 属性是普通样式属性，应用侧可直接在 `Style` 中设置，也可以与 `BreadcrumbItemStyle` /
  `BreadcrumbSeparatorStyle` 组合表达完整定制。
- `Breadcrumb` 主题默认 `HorizontalAlignment=Stretch`：root 是块级元素，铺满可用宽度，root frame 定制
  （如 antd style-class 的边框卡片）会填满容器而不是收缩到内容宽度。
- 不提供 `BorderDashArray` / `BorderDashOffset`：Ant Design 的 Breadcrumb style-class demo 不使用虚线边框。

## 4. 不变量

- `Breadcrumb` 的内置主题不消费 `.semantic-*` selector；主题默认视觉只通过 Token、精确 selector 和伪类表达。
- runtime marker 的 class 常量一律引用生成类型 `BreadcrumbSemanticParts`，不在代码中硬编码字符串。
- `separator` 是条目容器的兄弟节点而不是 `item` 的组成部分：N 个条目产生 N-1 个分隔符，最后一条目无尾随分隔符。
- `item` 与 `separator` 的标记在 container recycle 与模板重新应用后必须恢复，不能因回收丢失。
- 链接条目的文字颜色由内容呈现器上的 `LinkColor` Token 决定，应用侧 item 样式不得覆盖；主题默认根对齐为 `Stretch`。
- 契约验证见 `tests/AtomUI.Desktop.Controls.Tests/Breadcrumb/BreadcrumbSemanticPartTests.cs` 与
  `tests/AtomUI.Desktop.Controls.Tests/Breadcrumb/BreadcrumbRootFrameTests.cs`。
