# CheckBox Semantic Part 契约

本文档定义 CheckBox 对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。CheckBox 的整体设计见
[CheckBox 桌面版架构设计](overview.md)，真实模板与生命周期见 [CheckBox 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

CheckBox 主控件公开 `root`、`icon` 与 `label` 三个职责区域，与上游稳定 Semantic DOM
（`root` / `icon` / `label`，均 since 6.2.0）对齐。`icon` 对应复选框指示框区域，由模板中的 `CheckBoxIndicator`
节点承载；`label` 对应文本区域，由模板中的 `ContentPresenter` 节点承载。

`CheckBoxGroup`、`CheckBoxItemsControl` 与 `CheckBoxIndicator` 均不持有独立 Semantic descriptor：上游 Checkbox.Group
不提供 `classNames` / `styles` / Semantic API（只有单个 Checkbox 提供），且 `CheckBoxItemsControl` 与
`CheckBoxIndicator` 是 internal 类型。因此本控件的 Semantic Part 只由 `CheckBox` owner 公开。

| Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | owner | `CheckBox` | `Single` | `Root` | `false` | `false` |
| `icon` | `.semantic-icon` | `TemplatedControl` | `Single` | `Selector` | `false` | `false` |
| `label` | `.semantic-label` | `ContentPresenter` | `Single` | `Selector` | `false` | `false` |

`root` 是控件自身，承载 `IsChecked`、`IsThreeState`、`Content`、`IsEnabled`、动效与水波开关等 public API、主题入口和
状态归一，不声明 `.semantic-root` marker。`icon` 是模板中的复选框指示框节点 `Indicator`，对应上游 `icon` 语义；其
`ContractType` 为 `TemplatedControl` 而非 `CheckBoxIndicator`，因为 `CheckBoxIndicator` 是 internal 类型，不能作为
公共 Setter 依赖的最低类型，而 `TemplatedControl` 完整覆盖上游 icon 语义所需的 `Background`、`BorderBrush`、
`BorderThickness`、`CornerRadius`、`Width` / `Height` 等公共视觉属性。`label` 是模板文本节点
`ContentPresenter`，承载 `Content` 文本内容。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期类型
上下文；它不参与 `.semantic-*` 的身份匹配。

## 2. Part 说明

### 2.1 root

`root` 是 CheckBox owner 本身，在 CheckBox 实例的整个生命周期内始终存在，并且每个 CheckBox 恰好一个。

它负责：

- 承载 `IsChecked`、`IsThreeState`、`Content`、`ContentTemplate`、`IsEnabled` 等公共状态。
- 承载 `:checked`、`:unchecked`、`:indeterminate`、`:pointerover`、`:disabled` 等伪类。
- 提供最终 `Foreground`、`Background`、`Cursor`、对齐和尺寸等根视觉属性。
- 作为 `icon`、`label` owner-scoped Selector 的作用域边界。

适合通过 root 定制 CheckBox 整体前景色、透明度、光标和对齐状态。需要根据勾选状态改变根样式时，应在 owner
Selector 上组合公开属性或伪类。

root 不表示模板中的 `Border#Frame`、布局容器或指示框；这些节点的名称、数量和层级不属于 root 契约。

### 2.2 icon

`icon` 表示复选框指示框区域，每个内置 CheckBox 模板恰好一个，cardinality 为 `Single`。checked、unchecked 与
indeterminate 三态是同一指示框节点通过内部 `State` 枚举与 `:checked` / `:unchecked` / `:indeterminate` 伪类切换
实现，不是三个替代实现节点，因此不因勾选状态增删 marker。

它负责：

- 为指示框提供一致的局部视觉入口。
- 承载指示框的背景、边框、圆角和尺寸覆盖。
- 在三态、hover、disabled 下保持同一 Part 身份。

`icon` 的 `ContractType` 是 `TemplatedControl`，因此 Semantic Style 可以设置 `Background`、`BorderBrush`、
`BorderThickness`、`CornerRadius`、`Width`、`Height` 等属性。指示框内部的勾选标记（`CheckedMarkBrush`）、中间态标记
（`TristateMarkBrush` / `TristateMarkSize`）和勾选动画变换属于 `CheckBoxIndicator` 的 internal 属性，默认主题按
`:checked` / `:indeterminate` 状态控制，不通过 `icon` Part 对外承诺；需要定制勾选标记或中间态视觉时，应依赖默认
主题状态分支，而不是把 internal 属性当作 Semantic Setter 目标。

icon 不公开 `CheckBoxIndicator` 具体控件类型、其 internal 模板节点（`CheckedMark` / `TristateMark`）、内部 `Name`、
状态转换器或勾选动画 phase。

### 2.3 label

`label` 表示 CheckBox 的唯一文本区域，每个内置模板恰好一个，cardinality 为 `Single`。`Content` 可以为 `null`，此时
`ContentPresenter` 通过 `IsVisible` 绑定被隐藏，但节点仍属于模板稳定结构，Part 身份与数量不变。

它负责：

- 展示 `Content` 与 `ContentTemplate` 的最终结果。
- 承载文本前景色、透明度、排版和局部布局覆盖。
- 在 checked、unchecked、indeterminate、hover、disabled 下保持同一 Part 身份。

适合定制 `Foreground`、`FontSize`、`FontWeight`、`FontStyle`、`Opacity`、`Margin` 等属性。默认主题通过
`TextMargin` 控制 label 与指示框的间距；Semantic Style 覆盖 `Margin` 时作用于同一节点，不创建平行样式入口。

label 不公开内容模板生成的用户子树、文本内部 presenter 或内容值的具体 CLR 类型。

## 3. Selector 用法

应用级样式先限定 CheckBox owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|CheckBox">
        <atom:CheckBoxIconStyle x:SetterTargetType="TemplatedControl">
            <Setter Property="BorderBrush" Value="#1677ff" />
        </atom:CheckBoxIconStyle>

        <atom:CheckBoxLabelStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:CheckBoxLabelStyle>
    </Style>
</Application.Styles>
```

对特定 CheckBox class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|CheckBox.semantic-custom:checked">
    <atom:CheckBoxIconStyle x:SetterTargetType="TemplatedControl">
        <Setter Property="Background" Value="#1677ff" />
    </atom:CheckBoxIconStyle>
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
| unchecked | 1 | 1 | 1 | 指示框 `State=Unchecked`，三态共用同一 marker。 |
| checked | 1 | 1 | 1 | 指示框 `State=Checked`，勾选标记可见，marker 数量不变。 |
| indeterminate | 1 | 1 | 1 | 指示框 `State=Indeterminate`，中间态标记可见，marker 数量不变。 |
| `Content` 为 `null` | 1 | 1 | 1 | label 节点隐藏但仍存在，Part 身份不变。 |
| disabled | 1 | 1 | 1 | 状态只改变有效视觉属性，不增删 marker。 |
| hover / pointerover | 1 | 1 | 1 | 状态只改变边框/背景有效值，不增删 marker。 |

## 5. 尺寸基线

CheckBox 没有 `SizeType` 或 `ICustomizableSizeTypeAware` 尺寸档位。指示框采用固定交互尺寸：`CheckBoxToken`
的 `CheckIndicatorSize` 派生自 SharedToken `ControlInteractiveSize`，勾选标记 `CheckedMarkSize` 派生为
`CheckIndicatorSize * 0.6`，中间态标记 `IndicatorTristateMarkSize` 派生自 `FontSizeLG / 2`。因此 CheckBox 的尺寸
基线只有一档，不存在多档尺寸组合矩阵。

`icon` 的 `Width` / `Height` 默认由主题 setter 设为 `CheckIndicatorSize`；Semantic Style 覆盖这两者会改变指示框的
自然测量并偏离默认交互尺寸基线，因此不应把固定像素宽高当作无约束布局 Setter。需要调整指示框尺寸时，应评估
label 对齐、行高和表单行高是否仍保持一致，而不是只覆盖 `icon` 单节点。

## 6. 定制边界

以下区域明确不属于 CheckBox Semantic Part：

- 指示框内部的勾选标记、中间态标记、勾选动画变换和水波 actor。
- `CheckBoxGroup` 的集合布局、`CheckBoxItemsControl` 的 items presenter 与容器项。
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
- 内置 CheckBox 模板的 marker 数量与类型一致（`icon`、`label` 各一个，`root` 隐式）。
- 三态（checked / unchecked / indeterminate）切换不增删 marker，`icon` 保持 `Single`。
- `Content` 为 `null` 时 `label` 节点仍存在且 marker 身份不变。
- root 状态样式、icon 局部背景/边框/圆角、label 局部前景/字重在 hover/disabled 下保持有效。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
