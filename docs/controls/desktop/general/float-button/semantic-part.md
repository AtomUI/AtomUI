# FloatButton Semantic Part 契约

本文档定义 FloatButton 家族公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。FloatButton 的整体设计见
[FloatButton 桌面版架构设计](overview.md)，真实模板与生命周期见 [FloatButton 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`FloatButton` 与 `BackTopFloatButton` 公开 `root`、`icon`、`content` 三个职责区域，与上游稳定语义保持同名；
`FloatButtonGroup` 公开 `root`、`trigger`、`list` 三个职责区域。三个 Host 类型不是 Semantic owner：它们在
Overlay Layer 中创建的真实控件才是 owner，Host 自身不声明任何 Part。

### 1.1 `FloatButton`

| Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | owner | `FloatButton` | `Single` | `Root` | `false` | `false` |
| `icon` | `.semantic-icon` | `IconPresenter` | `Single` | `Selector` | `false` | `false` |
| `content` | `.semantic-content` | `ContentPresenter` | `Optional` | `Selector` | `false` | `false` |

`icon` 在 Circle 与 Square 两套模板中都存在；`content` 只在 Square 模板中存在（Circle 模板只有图标），因此使用
`Optional`。`root` 不声明 `.semantic-root` marker。

### 1.2 `BackTopFloatButton`

| Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | owner | `BackTopFloatButton` | `Single` | `Root` | `false` | `false` |
| `icon` | `.semantic-icon` | `IconPresenter` | `Single` | `Selector` | `false` | `false` |
| `content` | `.semantic-content` | `ContentPresenter` | `Optional` | `Selector` | `false` | `false` |

`BackTopFloatButton` 在实际 Panel 上声明显隐动效，与 `FloatButton` 共享相同的 Part 名称与数量语义。
上游体系的 BackTop 没有公开语义 API；AtomUI 按自身模板事实提供同等契约。

### 1.3 `FloatButtonGroup`

| Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | owner | `FloatButtonGroup` | `Single` | `Selector` | `false` | `false` |
| `trigger` | `.semantic-trigger` | `FloatButton` | `Optional` | `Selector` | `false` | `false` |
| `list` | `.semantic-list` | `TemplatedControl` | `Single` | `Selector` | `false` | `false` |

`trigger` 是 Click/Hover 触发模式模板中的主按钮；Default 模式模板没有 trigger，因此使用 `Optional`。`list` 是菜单项
容器 `FloatButtonItemsControl`（internal 类型），在 Default 与触发模式模板中都存在；`ContractType` 取公开基类
`TemplatedControl`，覆盖 Background、CornerRadius、Padding 等常用 Setter。

## 2. Selector 用法

应用侧只使用 owner-scoped selector 和生成的 Style 类型，不复制 `/template/` 路径：

```xml
<Style Selector="atom|FloatButton.semantic-style-demo">
    <atom:FloatButtonIconStyle x:SetterTargetType="atom:IconPresenter">
        <Setter Property="IconBrush" Value="#722ed1" />
    </atom:FloatButtonIconStyle>
    <atom:FloatButtonContentStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="FontSize" Value="13" />
    </atom:FloatButtonContentStyle>
</Style>
```

`BackTopFloatButtonIconStyle`、`BackTopFloatButtonContentStyle`、`FloatButtonGroupTriggerStyle` 与
`FloatButtonGroupListStyle` 使用同一规则。Semantic Style 只覆盖视觉属性和布局属性，不改变 `Shape`、`ButtonType`、
`IsOpen`、`Trigger` 或动画状态。

Group 内的子项按钮是独立的 `FloatButton` owner，可以在 group-scoped 后代 selector 下继续使用
`FloatButtonIconStyle` / `FloatButtonContentStyle` 定制，不需要 group 级别的 item Part：

```xml
<Style Selector="atom|FloatButtonGroup.semantic-style-demo atom|FloatButton">
    <atom:FloatButtonIconStyle x:SetterTargetType="atom:IconPresenter">
        <Setter Property="IconBrush" Value="#1677ff" />
    </atom:FloatButtonIconStyle>
</Style>
```

## 3. Template 与状态边界

- `FloatButton` 的 Circle/Square 双模板由 `Shape` 切换；`icon` marker 在两套模板中静态存在，`content` 只在 Square
  模板中存在。模板随 Shape 重套用后由新模板重新提供同一 marker 契约。
- `BackTopFloatButton` 的模板在 `Panel#PART_VisibilityRoot` 上声明 Motion；`IsActive` 驱动该表面的可见性，marker 身份和
  数量保持不变。
- `FloatButtonGroup` 的 Default 模板只有 `list`；Click/Hover 模板中 `trigger` 与包裹 list 的静态菜单表面是
  Canvas 下的 sibling。`IsOpen` 驱动 `PART_MenuSurface` 可见性与位移动画，不增删 marker 节点。
  list 由模板直接创建，关闭时随祖先表面隐藏，开关切换保持同一实例。
- `trigger` Part 的契约边界止于公开 `FloatButton` 控件本身；其模板内的 `icon`/`content` 由 `FloatButton` owner 的
  descriptor 负责，不由 `FloatButtonGroup` 重复公开。
- Badge（`DotBadgeAdorner` / `CountBadgeAdorner`）是 Badge 家族的运行时创建控件，拥有独立 descriptor；
  FloatButton 模板中的 `PART_BadgeLayout` 只是内部定位宿主，不属于任何 Part。
- ToolTip 是独立的 overlay 控件家族，通过附加属性接入，不属于 FloatButton 的 Part。
- `FloatButtonSeparatorLayer`、菜单动效表面、`BackgroundFrame`/`Frame` 表面层和 `EmptyControl` 占位都是内部
  Composition 节点，不属于 Semantic Part。

## 4. 尺寸与布局基线

按钮尺寸由 `FloatButtonToken.FloatButtonSize`（Circle 的 Width/Height，Square 的 Width 与 MinHeight）和
`FloatButtonIconSize` 控制；Circle 的 CornerRadius 由控件按实际高度的一半投影。Semantic Style 不以固定尺寸替代
这些分支；Square 的 `content` 行高来自 `DescriptionLineHeight`。

Group 的 `MenuPlacement` 决定 `list` 的 Orientation 与展开方向；Trigger 模式下 group 的测量尺寸收敛为 trigger 按钮
尺寸。`IsEmbedMode=True`（group 子项）时按钮使用嵌入视觉：无阴影、Square 缩小圆角并合并外框间距，这些分支由
`AbstractFloatButtonTheme` 的 embed selector 负责，Semantic Style 不应复制。

## 5. 验证不变量

- `FloatButton` descriptor 只包含 `root/icon/content`；`BackTopFloatButton` 相同；`FloatButtonGroup` 只包含
  `root/trigger/list`。
- Desktop 所有叶子 FloatButton 主题均声明静态 `Classes.semantic-*="True"`，不使用 Binding 或 `False` marker；
  Circle/Square 与 Default/Click/Hover 各模板分支的 marker 与 cardinality 一致。
- 默认主题不消费 `.semantic-*` selector；Semantic Part 只增加静态 class、descriptor 和生成 Style。
- Gallery Semantic Parts Tab 延迟创建 `FloatButton`、`FloatButtonGroup`（IsOpen 展示 trigger 与 list）和
  `BackTopFloatButton` Preview；Examples Tab 的既有示例树保持不变。
- Semantic 示例标题使用 `Custom Semantic Part styling`，文案只描述 AtomUI owner-scoped Style；Tag 使用当前版本
  `GalleryVersionInfo.DisplayVersion`（`v6.1.3`）。
