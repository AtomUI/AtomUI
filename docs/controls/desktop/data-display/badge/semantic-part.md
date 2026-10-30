# Badge Semantic Part 契约

本文档定义 `CountBadge`、`DotBadge` 和 `RibbonBadge` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。
Badge 的整体设计见 [Badge 桌面版架构设计](overview.md)，真实 Adorner 节点、跨根挂载和生命周期见
[Badge 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

三个可实例化 owner 各自拥有独立 descriptor。同名 `indicator` 表达相近的产品职责，不表示 owner 共享运行时节点、状态或 Theme。

### 1.1 `CountBadge`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `CountBadge` |
| Part | `root` |
| Selector | CountBadge 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `CountBadge` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | CountBadge owner |
| 职责 | CountBadge root 是数量、可见性、颜色、尺寸、定位和目标组合的状态 owner。 |
| 相关 API | 全部 CountBadge public API |
| 相关 Token | CountBadgeToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `CountBadge` |
| Part | `indicator` |
| Selector | `.semantic-indicator` |
| SelectorRoute | `> .semantic-scope-indicator /template/ .semantic-indicator` |
| Style Type | `CountBadgeIndicatorStyle` |
| ContractType | `Control` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 完整数量徽标区域 |
| 职责 | CountBadge indicator 表示完整数量徽标视觉。 |
| 相关 API | `Count`、`OverflowCount`、`IsZeroVisible`、`BadgeColor`、`Size`、`Offset` |
| 相关 Token | CountBadgeToken |
| 稳定性 | stable since 6.2.0 |

### 1.2 `DotBadge`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `DotBadge` |
| Part | `root` |
| Selector | DotBadge 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `DotBadge` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | DotBadge owner |
| 职责 | DotBadge root 是状态、文本、颜色、可见性、定位和目标组合的状态 owner。 |
| 相关 API | 全部 DotBadge public API |
| 相关 Token | DotBadgeToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `DotBadge` |
| Part | `indicator` |
| Selector | `.semantic-indicator` |
| SelectorRoute | `> .semantic-scope-indicator /template/ .semantic-indicator` |
| Style Type | `DotBadgeIndicatorStyle` |
| ContractType | `Control` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 状态点区域 |
| 职责 | DotBadge indicator 表示状态点视觉和统一动效边界，不包含独立模式的说明文本。 |
| 相关 API | `Status`、`DotColor`、`Offset`、`BadgeIsVisible` |
| 相关 Token | DotBadgeToken |
| 稳定性 | stable since 6.2.0 |

### 1.3 `RibbonBadge`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `RibbonBadge` |
| Part | `root` |
| Selector | RibbonBadge 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `RibbonBadge` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | RibbonBadge owner |
| 职责 | RibbonBadge root 是文本、颜色、位置、可见性和目标组合的状态 owner。 |
| 相关 API | 全部 RibbonBadge public API |
| 相关 Token | RibbonBadgeToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `RibbonBadge` |
| Part | `indicator` |
| Selector | `.semantic-indicator` |
| SelectorRoute | `> .semantic-indicator` |
| Style Type | `RibbonBadgeIndicatorStyle` |
| ContractType | `Control` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 完整 Ribbon 区域 |
| 职责 | RibbonBadge indicator 表示完整 Ribbon 视觉、定位和绘制边界。 |
| 相关 API | `RibbonColor`、`Placement`、`Offset`、`BadgeIsVisible` |
| 相关 Token | RibbonBadgeToken |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `RibbonBadge` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `> .semantic-indicator /template/ .semantic-content` |
| Style Type | `RibbonBadgeContentStyle` |
| ContractType | `Avalonia.Controls.TextBlock` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | Ribbon 文本区域 |
| 职责 | RibbonBadge content 表示 Ribbon 的文本展示与排版区域。 |
| 相关 API | `Text` |
| 相关 Token | RibbonBadgeToken、SharedToken typography |
| 稳定性 | stable since 6.2.0 |

所有 root 都是隐式 Part，不添加 `.semantic-root`。所有非 root Part 都由既有 runtime Adorner 生命周期创建，因此为
`Optional + RuntimeCreated`。CountBadge 与 DotBadge 在 target mode 下把 indicator 显示在 Avalonia `AdornerLayer`，所以
`CrossVisualRoot=true`；RibbonBadge 始终保持 owner inline visual tree。

## 2. CountBadge Parts

### 2.1 CountBadge root

root 是 `CountBadge` owner 本身，在实例生命周期内始终存在且恰好一个。它负责数量归一、零值显示规则、溢出文本、颜色、尺寸、
定位、可见性、动效开关和 `DecoratedTarget` 组合。

适合在 root 上定制 owner 的 Margin、Opacity、对齐、整体 Effect 以及基于 public 属性的状态组合。root 不等于 runtime
Adorner、数量背景、数量文本或 `AdornerLayer`，也不承诺这些内部节点的类型和层级。

### 2.2 CountBadge indicator

indicator 是完整数量徽标视觉，统一覆盖背景、数量文本和显示隐藏动效边界。它不会把背景与文本拆成两个公共 Part。

indicator 只有在数量徽标的 runtime visual 已创建时存在。`Count=0 && IsZeroVisible=false`、显式隐藏、owner 未附加或退出动效
完成后可以不存在；启用退出动效时，它可以在隐藏请求后短暂保留。standalone 模式位于 owner 普通子树，target mode 位于
`AdornerLayer`，但 logical/style owner 始终保持当前 CountBadge。

适合定制 Opacity、Effect、RenderTransform、Width、Height、Min/Max 和 Margin 等 `Control` 通用属性。布局型 Setter 会参与
Badge 的测量与定位，必须同时验证 standalone、target mode、Default/Small、零值、溢出和显示隐藏。

## 3. DotBadge Parts

### 3.1 DotBadge root

root 是 `DotBadge` owner 本身，负责状态、显式颜色、可见性、定位、独立文本和 `DecoratedTarget` 组合。standalone 与 target
mode 的模板变化不改变 root identity。

适合在 root 上定制 owner 级 Margin、Opacity、对齐和状态组合。root 不包含 `DecoratedTarget` 的内部视觉，也不把
`AdornerLayer` 作为自己的语义区域。

### 3.2 DotBadge indicator

indicator 只表示状态点及其统一动效边界，不包含 standalone 模式显示的说明文本。说明文本未进入公共 Part 契约，应用需要定制
该文本时应使用现有 public API、owner 级排版能力或自有组合，而不能依赖内部 Label。

indicator 在可见 runtime visual 建立后存在；显式隐藏、owner 未附加、退出动效完成或模式切换重建期间可以不存在。
standalone/target 两套内部模板实现同一个 `.semantic-indicator` 契约。target mode 虽跨 VisualRoot，owner-scoped Style 仍沿
logical/style owner 命中。

适合定制 Opacity、Effect、RenderTransform 和通用尺寸属性。不得依赖状态点的绘制类型、内部 Name、Label、motion actor 或
当前模板容器。

## 4. RibbonBadge Parts

### 4.1 RibbonBadge root

root 是 `RibbonBadge` owner 本身，负责文本、颜色、Start/End 位置、偏移、可见性和 `DecoratedTarget` 组合。隐藏 Ribbon 时
`DecoratedTarget` 继续保留，root 不应被当作目标内容本身。

适合在 root 上定制 owner 级 Margin、Opacity、对齐和状态组合。root 不公开 Ribbon 折角、背景 Geometry、render helper 或
目标内容内部视觉。

### 4.2 RibbonBadge indicator

indicator 表示完整 Ribbon 表面，包括其最终绘制、定位和内容承载边界。Ribbon 背景与折角由 runtime Adorner 自身绘制，因此
indicator 不拆分为背景、折角或阴影 Part。

indicator 在 Ribbon 可见且 runtime visual 已创建时存在；显式隐藏或 owner detach 后不存在。它是 owner 的 inline runtime
child，不跨 VisualRoot。适合定制 Opacity、Effect、RenderTransform 和通用布局属性；布局型 Setter 必须验证 standalone、
target mode、Start/End 和不同文本长度。

### 4.3 RibbonBadge content

content 表示 Ribbon 内唯一文本展示区域，ContractType 为 `TextBlock`。它与 indicator 同时创建和移除，因此为 `Optional`。

适合定制 Foreground、FontSize、FontWeight、FontStyle、Opacity、Margin 和文本对齐。它只覆盖 Ribbon `Text` 的展示节点，不
包含 `DecoratedTarget`、用户目标内容或绘制背景。

## 5. Selector 用法

Badge 的 runtime adorner 是 public owner 的直接 logical child。用户通过生成的 Semantic Style 访问 Part；生成类型内部保留
Count/Dot 的 `.semantic-scope-indicator` 与 Ribbon 的完整 route，用户不需要复制这些边界：

```xml
<Application.Styles>
    <Style Selector="atom|CountBadge">
        <atom:CountBadgeIndicatorStyle x:SetterTargetType="Control">
            <Setter Property="Opacity" Value="0.85" />
        </atom:CountBadgeIndicatorStyle>
    </Style>

    <Style Selector="atom|RibbonBadge">
        <atom:RibbonBadgeContentStyle x:SetterTargetType="TextBlock">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:RibbonBadgeContentStyle>
    </Style>
</Application.Styles>
```

按实例或业务状态限定时，把 class 放在 owner selector 上；生成 Style 仍然由 owner 类型和 descriptor route 保证作用域：

```xml
<Style Selector="atom|DotBadge.semantic-custom[Status=Success]">
    <atom:DotBadgeIndicatorStyle x:SetterTargetType="Control">
        <Setter Property="Opacity" Value="0.9" />
    </atom:DotBadgeIndicatorStyle>
</Style>
```

不得使用以下 Selector：

- `atom|CountBadge /template/ .semantic-indicator`：runtime Adorner 不是 CountBadge template child。
- `atom|CountBadge .semantic-indicator`：logical descendant 会穿透 `DecoratedTarget` 中嵌套的其他 Badge owner。
- `Control.semantic-indicator`、`:is(Control).semantic-indicator` 或 `TextBlock.semantic-content`。
- 直接复制 runtime adorner 的完整 route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- internal Adorner 类型、`PART_*`、当前布局容器或 `AdornerLayer` 子节点顺序。

## 6. 状态与数量语义

| 场景 | root | indicator | content | 说明 |
| --- | --- | --- | --- | --- |
| owner 未附加 | 1 | 0 | 0 | descriptor 可查询，runtime visual 尚未建立。 |
| standalone 且可见 | 1 | 1 | 仅 Ribbon 为 1 | indicator 位于 owner 普通子树。 |
| target mode 且可见 | 1 | 1 | 仅 Ribbon 为 1 | Count/Dot indicator 跨 VisualRoot；Ribbon 保持 inline。 |
| `BadgeIsVisible=false` | 1 | 最终为 0 | 最终为 0 | Count/Dot 启用退出动效时可以短暂保留 indicator。 |
| Count 零值且不显示零 | 1 | 最终为 0 | 不适用 | `Count` 与 `IsZeroVisible` 共同决定可见性。 |
| Dot standalone/target 切换 | 1 | 重建为 1 | 不适用 | 内部模板变化，公开 Part 名称和类型不变。 |
| Ribbon Start/End 切换 | 1 | 1 | 1 | 只改变布局和绘制位置，不改变 Part 数量。 |

## 7. 定制边界

以下区域明确不属于 Badge Semantic Part：

- `DecoratedTarget` 及其内部视觉。
- CountBadge 的背景、数量文本和溢出文本拆分。
- DotBadge standalone 说明 Label。
- `RootLayout`、motion actor、motion phase 和内部控件类型。
- Ribbon 折角 Geometry、阴影、颜色计算、定位 wrapper 和 render helper。
- 共享 `AdornerLayer` 以及同一层中的其他 Adorner。

Semantic marker 不改变 focus、hit testing、automation owner、可访问名称或 Badge 状态计算。颜色、数量、状态、位置、显示规则和
动效开关仍由对应 public API 表达；Semantic Style 只覆盖已经公开 Part 的视觉属性。

## 8. 兼容性与验证

删除或重命名 Part、修改 selector class 或 selector route、收窄 `ContractType`、改变 cardinality/cross-root/runtime 语义，
或者破坏 runtime logical/style owner，均属于公共主题契约变更。

验证至少覆盖：

- 三个 owner 的 descriptor 字段、Part 顺序和独立作用域。
- Count/Dot standalone、target mode、显示隐藏、退出动效和 detach 后 marker 数量。
- Count/Dot target mode 的 visual parent 为 `AdornerLayer`，logical/style owner 仍为对应 Badge owner。
- Dot 两套模板实现同一 indicator；Ribbon indicator/content 同步创建和释放。
- owner-scoped 生成 Style 在 Count/Dot target mode 与 Ribbon inline visual 中均可命中；descriptor route 只作为生成与诊断依据。
- 默认主题不消费 `.semantic-*`，Control 包不扫描 VisualTree 或查询 runtime registry。
- Gallery 只把当前 owner 对应的具体 runtime Adorner作为 additional root，不把共享 `AdornerLayer` 整体加入 Preview。
- Generator 静态输出和 NativeAOT 路径不依赖反射或动态代码。
