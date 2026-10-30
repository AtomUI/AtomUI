# Space Semantic Part 契约

本文档定义 `Space` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。`Space` 的整体设计见
[Space 桌面版架构设计](overview.md)，实现与生命周期见 [Space 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

`Space` 的公开语义结构与 Ant Design 6 的 Semantic DOM 结构对齐：`root`、`item`、`separator`。其中 `root` 是隐式
owner，`item` 表示每个直接子项，`separator` 表示位于子项之间的分隔元素。

## 1. Semantic Parts

| Part | AtomUI 节点 | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `root` | `Space` owner | owner | `Space` | `Single` | `Root` | `false` | `false` |
| `item` | 直接子项 | `> .semantic-item` | `Control` | `Multiple` | `Selector` | `false` | `true` |
| `separator` | 直接分隔项 | `> .semantic-separator` | `Control` | `Multiple` | `Selector` | `false` | `true` |

`root` 不声明 `.semantic-root` marker，也不生成独立 Style type。`item` 和 `separator` 都是运行时创建的语义标记：
`item` 挂在 `Space.Children` 的每个直接子项上，`separator` 挂在由 `SplitTemplate` 构建并插入相邻子项之间的
分隔控件上。`separator` 的数量始终等于 `Children.Count - 1`，当 `SplitTemplate` 为空时不创建任何分隔项。

`Space` 没有独立的 `SpaceTheme.axaml`，因此内置主题不依赖静态 `.semantic-*` marker；语义契约完全由 runtime marker
与生成的 `Style` 类型表达。

## 2. Selector 用法

应用侧使用 owner-scoped selector 和生成的 Style 类型，不手写 `/template/` 路径：

```xml
<Style Selector="atom|Space.semantic-demo">
    <atom:SpaceItemStyle x:SetterTargetType="Control">
        <Setter Property="Opacity" Value="0.85" />
    </atom:SpaceItemStyle>
    <atom:SpaceSeparatorStyle x:SetterTargetType="Control">
        <Setter Property="Opacity" Value="0.5" />
    </atom:SpaceSeparatorStyle>
</Style>
```

`SpaceItemStyle` 和 `SpaceSeparatorStyle` 位于 `AtomUI.Theme.Styling` 命名空间，由语义生成器根据 `Space.SemanticParts.cs`
生成。`ContractType` 只给出 Setter 所需的最低稳定类型，不参与 selector 匹配；如果分隔符模板是更具体的 control，应用
可以在自己的样式里把 `x:SetterTargetType` 收窄到更具体的公开类型。

## 3. Root 定制边界

`root` 的定制直接作用于 `Space` owner 自身。`Space` 作为 `Control` 派生类没有 Avalonia 的 `Background` / `Border*` /
`Padding` 基础属性，因此 `Space` 注册了自身的 root frame 样式属性，用于对齐 Ant Design 对 root 的边框、背景与内边距
定制（对应 antd `styles.root` 的 `borderWidth`、`borderStyle`、`backgroundColor`、`borderColor`、`padding`）：

| 属性 | 类型 | 作用 |
| --- | --- | --- |
| `Background` | `IBrush?` | root 背景填充。 |
| `BorderBrush` | `IBrush?` | root 边框画笔。 |
| `BorderThickness` | `Thickness` | root 边框厚度。 |
| `CornerRadius` | `CornerRadius` | root 圆角。 |
| `Padding` | `Thickness` | root 内边距。 |
| `BorderDashArray` | `IReadOnlyList<double>?` | root 边框虚线阵列，与 `BorderThickness` 配合表达虚线边框。 |
| `BorderDashOffset` | `double` | root 边框虚线偏移。 |

边界约定：

- root frame 由 `Space` 自身在 `Render` 中绘制（复用 `BorderRenderHelper`），`Space` 没有独立的 `SpaceTheme.axaml`，
  因此内置主题不依赖静态模板或静态 `.semantic-*` marker。
- `Padding + BorderThickness` 构成 frame inset：`MeasureOverride` 先用 inset 收缩测量约束，`ArrangeOverride` 再用 inset
  收缩排布区域，子项永远位于边框与内边距之内；desired size 对 inset 做外扩。
- 这些属性只影响 root 外观与布局边界，不改变 `item` / `separator` 的语义契约；`item` / `separator` 的样式仍通过
  `SpaceItemStyle` / `SpaceSeparatorStyle` 表达。

## 4. Template 与状态边界

- `item` marker 位于 `Space.Children` 中每个直接子项上；它与 `Orientation`、`ItemsAlignment`、`ItemWidth`、`ItemHeight`、
  `ItemSpacing`、`LineSpacing`、`SizeType` 这些布局状态解耦。
- `separator` marker 位于 `SplitTemplate` 生成的分隔控件上，只负责表达“此处是分隔项”这一稳定职责。
- `SplitTemplate` 只是 separator 的来源，不改变 root/item 语义，也不引入额外包装层。
- `SplitTemplate` 在 children 已存在后设置时，会在 attach 后重建同一组 item / separator marker。
- 语义 contract 不要求 `Space` 使用 `ControlTheme` 或静态 template part；它只要求 marker、descriptor 和生成 style 一致。

## 5. 验证不变量

- `Space` descriptor 公开 `root`、`item`、`separator`，其中 `item` 和 `separator` 都是 `Multiple`。
- `root` 仍然是隐式 owner，不存在 `.semantic-root` marker。
- 内置代码和主题文件不使用 `Classes.semantic-*="True"` 静态 marker。
- 运行时 marker 数量与实际 children / separators 数量一致。
- `SplitTemplate` 的设置顺序不会改变最终语义：先加 children 再设 `SplitTemplate`，或先设 `SplitTemplate` 再加 children，结果都应稳定。
- root frame 属性只参与 root 外观与 frame inset 布局，不改变 `item` / `separator` 的语义契约。
- 回归测试见 `tests/AtomUI.Desktop.Controls.Tests/Space/SpaceSemanticPartTests.cs` 与 `tests/AtomUI.Desktop.Controls.Tests/Space/SpaceRootFrameTests.cs`。
