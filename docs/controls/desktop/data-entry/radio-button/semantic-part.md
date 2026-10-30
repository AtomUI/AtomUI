# RadioButton Semantic Part 契约

本文档定义 RadioButton 对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。RadioButton 的整体设计见
[RadioButton 桌面版架构设计](overview.md)，真实模板与生命周期见 [RadioButton 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

RadioButton 主控件公开 `root`、`icon` 与 `label` 三个职责区域，与上游稳定 Semantic DOM
（`root` / `icon` / `label`，均 since 6.2.0）对齐。`icon` 对应单选指示圆环区域，由模板中的 `RadioIndicator`
节点承载；`label` 对应文本区域，由模板中的 `ContentPresenter` 节点承载。

`RadioButtonGroup`、`RadioIndicator` 与 `OptionButton` / `OptionButtonGroup` 均不持有独立 Semantic descriptor：

- 上游 Radio.Group 不提供 `classNames` / `styles` / Semantic API（只有单个 Radio 提供），因此 `RadioButtonGroup` 的
  集合容器 `ItemsPresenter` 不是 Semantic Part；Group 只负责创建并管理 `RadioButton` 容器，语义由每个 RadioButton
  owner 各自公开。
- `RadioIndicator` 是 internal 类型，不能作为公共 descriptor owner。
- 上游 Radio.Button 的 props 只继承 `AbstractCheckboxProps`，没有自身的 `classNames` / `styles`；对应 AtomUI 的
  `OptionButton` / `OptionButtonGroup` 因此不公开 Semantic Part。ConfigProvider 上下文透传不得作为准入证据。

因此本控件的 Semantic Part 只由 `RadioButton` owner 公开。

| Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | owner | `RadioButton` | `Single` | `Root` | `false` | `false` |
| `icon` | `.semantic-icon` | `TemplatedControl` | `Single` | `Selector` | `false` | `false` |
| `label` | `.semantic-label` | `ContentPresenter` | `Single` | `Selector` | `false` | `false` |

`root` 是控件自身，承载 `IsChecked`、`Content`、`IsEnabled`、动效与水波开关等 public API、主题入口和状态归一，
不声明 `.semantic-root` marker。`icon` 是模板中的单选指示圆环节点 `Indicator`，对应上游 `icon` 语义；其
`ContractType` 为 `TemplatedControl` 而非 `RadioIndicator`，因为 `RadioIndicator` 是 internal 类型，不能作为公共
Setter 依赖的最低类型。`label` 是模板文本节点 `ContentPresenter`，承载 `Content` 文本内容。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期类型
上下文；它不参与 `.semantic-*` 的身份匹配。

## 2. Part 说明

### 2.1 root

`root` 是 RadioButton owner 本身，在 RadioButton 实例的整个生命周期内始终存在，并且每个 RadioButton 恰好一个。

它负责：

- 承载 `IsChecked`、`Content`、`ContentTemplate`、`IsEnabled` 等公共状态。
- 承载 `:checked`、`:unchecked`、`:pointerover`、`:disabled` 等伪类。
- 提供最终 `Foreground`、`Background`、`Cursor`、对齐和尺寸等根视觉属性。
- 作为 `icon`、`label` owner-scoped Selector 的作用域边界。

适合通过 root 定制 RadioButton 整体前景色、透明度、光标和对齐状态。需要根据选中状态改变根样式时，应在 owner
Selector 上组合公开属性或伪类。

root 不表示模板中的 `PixelAlignedBorder#Frame`、布局容器或指示圆环；这些节点的名称、数量和层级不属于 root 契约。

### 2.2 icon

`icon` 表示单选指示圆环区域，每个内置 RadioButton 模板恰好一个，cardinality 为 `Single`。checked 与 unchecked 是
同一指示节点通过 `:checked` / `:unchecked` 伪类切换绘制结果，不是两个替代实现节点，因此不因选中状态增删 marker。

它负责：

- 为指示圆环提供一致的局部视觉入口。
- 承载指示圆环的尺寸与局部布局覆盖。
- 在 checked、unchecked、hover、disabled 下保持同一 Part 身份。

`icon` 的 `ContractType` 是 `TemplatedControl`，因此 Semantic Style 可以设置 `Background`（指示填充色）、
`BorderBrush`（指示边框色）、`BorderThickness`（指示边框宽度）、`CornerRadius`（指示外形圆角）、`Width`、
`Height`、`Opacity` 等 `TemplatedControl` 继承自 `Control` 的公共属性。`RadioIndicator` 通过自定义 `Render`
绘制指示，并直接消费 `Background`、`BorderBrush`、`BorderThickness` 与 `CornerRadius`；默认主题把
`CornerRadius` 设为 `RadioSize`（即正圆），Semantic Style 覆盖为更小值时得到圆角方形指示。圆点颜色仍由
`RadioIndicator` 内部的 `RadioInnerBackground` 驱动，不属于 `TemplatedControl` 公共契约，也不通过 `icon` Part
对外承诺。需要调整圆点颜色时，应通过主题 SharedToken 或默认主题的 `:checked` / `:disabled` 状态分支实现。

icon 不公开 `RadioIndicator` 具体控件类型、其 `Render` 绘制细节、内部圆点几何计算、水波 actor、内部 `Name`
或选中动画 phase。

### 2.3 label

`label` 表示 RadioButton 的唯一文本区域，每个内置模板恰好一个，cardinality 为 `Single`。`Content` 可以为 `null`，
此时 `ContentPresenter` 通过 `IsVisible` 绑定被隐藏，但节点仍属于模板稳定结构，Part 身份与数量不变。

它负责：

- 展示 `Content` 与 `ContentTemplate` 的最终结果。
- 承载文本前景色、透明度、排版和局部布局覆盖。
- 在 checked、unchecked、hover、disabled 下保持同一 Part 身份。

适合定制 `Foreground`、`FontSize`、`FontWeight`、`FontStyle`、`Opacity`、`Margin` 等属性。默认主题通过
`TextMargin` 控制 label 与指示圆环的间距；Semantic Style 覆盖 `Margin` 时作用于同一节点，不创建平行样式入口。

label 不公开内容模板生成的用户子树、文本内部 presenter 或内容值的具体 CLR 类型。

## 3. Selector 用法

应用级样式先限定 RadioButton owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|RadioButton">
        <atom:RadioButtonIconStyle x:SetterTargetType="TemplatedControl">
            <Setter Property="Opacity" Value="0.85" />
        </atom:RadioButtonIconStyle>

        <atom:RadioButtonLabelStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:RadioButtonLabelStyle>
    </Style>
</Application.Styles>
```

对特定 RadioButton class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|RadioButton.semantic-custom:checked">
    <atom:RadioButtonLabelStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Foreground" Value="#1677ff" />
    </atom:RadioButtonLabelStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `TemplatedControl.semantic-icon` 或 `:is(TemplatedControl).semantic-icon`。
- `ContentPresenter.semantic-label` 或 `:is(ContentPresenter).semantic-label`。
- 直接复制 `/template/ .semantic-*` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 连续穿过子控件模板的多个 `/template/`。
- 依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序。

## 4. 状态与数量语义

| 状态 | root | icon | label | 说明 |
| --- | --- | --- | --- | --- |
| unchecked | 1 | 1 | 1 | 指示圆环 `:unchecked`，marker 数量不变。 |
| checked | 1 | 1 | 1 | 指示圆环 `:checked`，圆点可见，marker 数量不变。 |
| `Content` 为 `null` | 1 | 1 | 1 | label 节点隐藏但仍存在，Part 身份不变。 |
| disabled | 1 | 1 | 1 | 状态只改变有效视觉属性，不增删 marker。 |
| hover / pointerover | 1 | 1 | 1 | 状态只改变边框有效值，不增删 marker。 |

RadioButton 是单选控件，没有 indeterminate 状态；`IsChecked` 只有 `true` / `false` / `null`（`null` 语义上按
unchecked 处理），三种取值共用同一个 `icon` 与 `label` marker。

## 5. 尺寸基线

RadioButton 没有 `SizeType` 或 `ICustomizableSizeTypeAware` 尺寸档位。指示圆环采用固定交互尺寸：`RadioButtonToken`
的 `RadioSize` 派生自 SharedToken `FontSizeLG`，圆点 `DotSize` 派生为 `RadioSize - (DotPadding + LineWidth) * 2`。
因此 RadioButton 的尺寸基线只有一档，不存在多档尺寸组合矩阵。

`icon` 的 `Width` / `Height` 默认由主题 setter 设为 `RadioSize`；Semantic Style 覆盖这两者会改变指示圆环的自然测量
并偏离默认交互尺寸基线，因此不应把固定像素宽高当作无约束布局 Setter。需要调整指示圆环尺寸时，应评估 label 对齐、
行高和表单行高是否仍保持一致，而不是只覆盖 `icon` 单节点。

## 6. 定制边界

以下区域明确不属于 RadioButton Semantic Part：

- 指示圆环内部的圆点颜色（`RadioInnerBackground`）、选中动画和水波 actor。
- `RadioButtonGroup` 的集合布局、`ItemsPresenter` 与容器项生命周期。
- `OptionButton` / `OptionButtonGroup` 的按钮式内容、Outline/Solid、方向和组合几何。
- 用户 `ContentTemplate` 创建的子树。
- `PART_*` 名称、internal 类型、状态转换器和 motion phase。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；如果最终布局仍被 owner 或中间
节点的 Height、Min/Max、Padding、Margin、裁剪限制，应按跨节点布局约束排查，不能把它解释为 Semantic Style 优先级
失效。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少 marker，均
属于公共主题契约变更。

验证至少覆盖：

- descriptor 中只有 `root`、`icon`、`label`，字段值与本文表格一致。
- 内置 RadioButton 模板的 marker 数量与类型一致（`icon`、`label` 各一个，`root` 隐式）。
- checked / unchecked / `IsChecked=null` 切换不增删 marker，`icon` 保持 `Single`。
- `Content` 为 `null` 时 `label` 节点仍存在且 marker 身份不变。
- root 状态样式、icon 局部尺寸/透明度/填充/边框、label 局部前景/字重在 hover/disabled 下保持有效。
- `RadioButtonGroup` 创建/回收 `RadioButton` 容器时，每个容器仍公开同一 descriptor，marker 不跨容器泄漏。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
