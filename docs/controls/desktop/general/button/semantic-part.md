# Button Semantic Part 契约

本文档定义 Button 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。Button 的整体设计见
[Button 桌面版架构设计](overview.md)，真实模板节点与生命周期见 [Button 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Button 公开 `root`、`icon` 和 `content` 三个 Semantic Part。Part 名称表达长期稳定的产品职责，不等同于当前模板节点名称。

### 1.1 `Button`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Button` |
| Part | `root` |
| Selector | Button 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Button` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Button owner |
| 职责 | Button root 是动作、状态与根视觉样式的统一 owner。 |
| 相关 API | 全部 Button public API |
| 相关 Token | ButtonToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `Button` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `/template/ .semantic-icon` |
| Style Type | `ButtonIconStyle` |
| ContractType | `Control` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 用户图标与 loading 图标区域 |
| 职责 | 统一表示 Button 的用户图标和 loading 图标视觉职责。 |
| 相关 API | `Icon`、`IsLoading`、`IconPlacement`、`IconWidth`、`IconHeight` |
| 相关 Token | `IconSize*`、`OnlyIconSize*`、`IconMargin` |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `Button` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| Style Type | `ButtonContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 内容展示区域 |
| 职责 | 表示 Button 的用户内容展示与排版区域。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | `ContentFontSize`、`ContentLineHeight`、`FontWeight` |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不添加 `.semantic-root`。`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过
`x:SetterTargetType` 提供 AXAML 编译期类型上下文；它不参与 `.semantic-*` 的身份匹配。

## 2. Part 说明

### 2.1 root

`root` 是 Button owner 本身，在 Button 实例的整个生命周期内始终存在，并且每个 Button 恰好一个。

它负责：

- 承载 `ButtonType`、`Color`、`Variant`、危险态、加载态、尺寸、形状和命令等公共状态。
- 承载 `:pointerover`、`:pressed`、`:disabled` 及 Button 专属伪类。
- 提供最终 `Background`、`BorderBrush`、`Foreground`、`Padding`、`Effect`、圆角和尺寸等根视觉属性。
- 作为 `icon`、`content` owner-scoped Selector 的作用域边界。

适合通过 root 定制 Button 整体背景、边框、阴影、内边距、透明度和状态组合。需要根据 Button 状态改变根样式时，应在
owner Selector 上组合公开属性或伪类。Button 的 Wave 在播放前读取 root 最终生效的边框色和背景色，因此 root 颜色覆盖与
交互反馈使用同一视觉事实源。

root 不表示模板中的 Frame、阴影层、Wave 节点或布局容器；这些节点的名称、数量和层级不属于 root 契约。

### 2.2 icon

`icon` 统一表示用户图标和 loading 图标。每个内置 Button 模板同时保留两个 marker 节点，并通过可见性切换当前呈现的实现，
因此 cardinality 为 `Multiple`，而不是根据 `IsLoading` 在 `Single` 与 `Optional` 之间变化。

它负责：

- 为用户图标与 loading 图标提供一致的局部视觉入口。
- 在 `IconPlacement=Start/End`、loading 和 icon-only 状态下保持相同 Part 身份。
- 允许以 `Control` 为最低类型设置 Width、Height、Margin、Opacity、RenderTransform 等通用视觉属性。

需要让两种图标在所有状态共享同一尺寸时，优先设置 Button 的 `IconWidth` 和 `IconHeight`；只需要局部视觉覆盖时使用
`.semantic-icon`。对 icon 使用布局型 Setter 时必须同时检查普通、loading、icon-only、Circle、Round 和 Desktop/Browser
模板，避免只验证当前可见的一个节点。

icon 不公开具体图标控件类型、loading actor、内部 Name、可见性转换器或用户图标与 loading 图标的模板节点名称。

### 2.3 content

`content` 表示 Button 的唯一用户内容展示区域，每个内置模板恰好一个，cardinality 为 `Single`。内容值可以为 `null`，但
Part 节点仍属于模板稳定结构。

它负责：

- 展示 `Content` 与 `ContentTemplate` 的最终结果。
- 承载内容文字的前景色、透明度、排版和局部布局覆盖。
- 在普通、hover、pressed、disabled 和 loading 状态下保持同一 Part 身份。

适合定制 `Foreground`、`FontSize`、`FontWeight`、`Opacity`、`Padding`、`Margin` 和对齐属性。直接设置 content 的
`Foreground` 时，该值属于 Part 局部样式，不应被 Button root 的 hover/pressed 前景色重新解释。布局型 Setter 会参与 Button
自然测量；预设 `SizeType` 只提供 `MinHeight` 基线，不保证任意 Padding 或 Margin 组合仍保持默认高度。

content 不公开内容模板生成的用户子树、文本内部 presenter、当前布局容器或内容值的具体 CLR 类型。

## 3. Selector 用法

应用级样式先限定 Button owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|Button">
        <atom:ButtonIconStyle x:SetterTargetType="Control">
            <Setter Property="Opacity" Value="0.85" />
        </atom:ButtonIconStyle>

        <atom:ButtonContentStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:ButtonContentStyle>
    </Style>
</Application.Styles>
```

对特定 Button class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|Button.semantic-custom[ButtonType=Primary]">
    <atom:ButtonContentStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Foreground" Value="White" />
    </atom:ButtonContentStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `Control.semantic-icon` 或 `:is(Control).semantic-icon`。
- `ContentPresenter.semantic-content` 或 `:is(ContentPresenter).semantic-content`。
- 直接复制 `/template/ .semantic-*` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 连续穿过子控件模板的多个 `/template/`。
- 依赖 `PART_*`、内部类型、Name 或视觉祖先顺序。

## 4. 状态与数量语义

| 状态 | root | icon | content | 说明 |
| --- | --- | --- | --- | --- |
| 普通 Button | 1 | 2 | 1 | 用户图标与 loading 图标 marker 都存在，主题控制可见性。 |
| `IsLoading=true` | 1 | 2 | 1 | loading 图标显示，用户图标按现有状态规则隐藏；Part 数量不变。 |
| 无 `Icon` | 1 | 2 | 1 | 用户图标节点可以不可见，`icon` 契约仍由模板实现。 |
| content 为 `null` | 1 | 2 | 1 | 内容节点仍存在，Button 可以进入 icon-only 状态。 |
| disabled / hover / pressed | 1 | 2 | 1 | 状态只改变有效视觉属性，不增删 marker。 |
| Desktop / Browser 模板 | 1 | 2 | 1 | 所有内置 Button 叶子模板实现相同 Part 契约。 |

## 5. 定制边界

以下区域明确不属于 Button Semantic Part：

- Wave actor、阴影层和主体 Frame。
- 根布局、图标排列容器、间距 wrapper 和 focus visual。
- 用户 `ContentTemplate` 创建的子树。
- `PART_*` 名称、internal 类型、转换器和 motion phase。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；如果最终布局仍被 owner 或中间节点的
Height、Min/Max、Padding、Margin、裁剪或 Shape 几何限制，应按跨节点布局约束排查，不能把它解释为 Semantic Style
优先级失效。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少 marker，均属于公共
主题契约变更。

验证至少覆盖：

- descriptor 中只有 `root`、`icon`、`content`，字段值与本文表格一致。
- Desktop、Browser 和适用派生模板的 marker 数量与类型一致。
- root 状态样式、icon Multiple 语义和 content 局部颜色在 hover/pressed/loading 下保持有效。
- content Padding、icon Width/Height 等布局 Setter 与三档尺寸、Custom、Circle、Round、loading 组合。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
