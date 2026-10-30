# Empty Semantic Part 契约

本文档定义 Empty 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。Empty 的整体设计见
[Empty 桌面版架构设计](overview.md)，真实模板节点与生命周期见 [Empty 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Empty 公开 `root`、`image`、`description` 和 `footer` 四个 Semantic Part。该契约以 AtomUI 的 public API、静态
ControlTemplate 和 Avalonia 样式优先级作为实现事实。

### 1.1 `Empty`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Empty` |
| Part | `root` |
| Selector | Empty 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Empty` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Empty owner |
| 职责 | 空状态的根布局、整体对齐、可见性和根视觉样式 owner。 |
| 相关 API | `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`、`StrokeDashArray` 及标准布局属性 |
| 相关 Token | EmptyToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `image`

| 字段 | 值 |
| --- | --- |
| Owner | `Empty` |
| Part | `image` |
| Selector | `.semantic-image` |
| SelectorRoute | `/template/ .semantic-image` |
| Style Type | `EmptyImageStyle` |
| ContractType | `Control` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `PART_SvgImage` 对应的 `Avalonia.Svg.Svg` |
| 职责 | 展示内置 Default/Simple 图形或 `ImagePath`、`ImageSource` 指定的 SVG 图形。 |
| 相关 API | `PresetImage`、`ImagePath`、`ImageSource`、`SizeType` |
| 相关 Token | `EmptyImgHeight`、`EmptyImgHeightMD`、`EmptyImgHeightSM`、图形颜色资源 |
| 稳定性 | stable since 6.2.0 |

#### `description`

| 字段 | 值 |
| --- | --- |
| Owner | `Empty` |
| Part | `description` |
| Selector | `.semantic-description` |
| SelectorRoute | `/template/ .semantic-description` |
| Style Type | `EmptyDescriptionStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 描述 `TextBlock` |
| 职责 | 展示本地化默认描述或调用方提供的 `Description`。 |
| 相关 API | `Description`、`IsDescriptionVisible`、`SizeType` |
| 相关 Token | `DescriptionMargin`、`DescriptionMarginSM`、SharedToken 文本颜色 |
| 稳定性 | stable since 6.2.0 |

#### `footer`

| 字段 | 值 |
| --- | --- |
| Owner | `Empty` |
| Part | `footer` |
| Selector | `.semantic-footer` |
| SelectorRoute | `/template/ .semantic-footer` |
| Style Type | `EmptyFooterStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Footer `ContentPresenter` |
| 职责 | 承载创建、刷新、返回或其他空状态后续操作。 |
| 相关 API | `Footer`、`FooterTemplate` |
| 相关 Token | `FooterMargin`、SharedToken |
| 稳定性 | stable since 6.2.0 |

## 2. Part 说明

### 2.1 `root`

`root` 是 Empty 控件实例本身，不增加 `.semantic-root` marker。它适合定制 `Background`、`BorderBrush`、
`BorderThickness`、`CornerRadius`、`Padding`、`StrokeDashArray`、`Margin`、`HorizontalAlignment`、`VerticalAlignment`、
`Opacity` 和整体可见性。背景、边框、圆角、Padding 和虚线节奏由模板私有表面 `DashedBorder` 投影；
调用方不得依赖该 `DashedBorder` 或内部 `StackPanel`，它们只是当前表面承载和纵向排列的实现节点。

### 2.2 `image`

`image` marker 静态放置在 `PART_SvgImage`。`PresetImage`、`ImagePath` 和 `ImageSource` 只改变同一 renderer 的内容，不替换
Semantic target，也不改变 cardinality。`ContractType` 使用 `Control`，避免把公共定制契约锁死在 `Avalonia.Svg.Svg`；调用方可以
安全定制通用布局、尺寸、透明度和变换属性，但不得通过 Semantic Style 接管 `Source`、`Path` 或内置图形配色状态。

### 2.3 `description`

`description` marker 静态放置在 `TextBlock`。`Description` 为 `null` 或空字符串时节点仍属于模板结构；
`IsDescriptionVisible=false` 通过 `TemplateBinding` 隐藏节点，不动态创建或删除 Visual。因此 descriptor 按真实模板节点定义为
`Single`，可见性不改变 marker 数量。

### 2.4 `footer`

`footer` marker 静态放置在 Footer `ContentPresenter`。`Footer=null` 时 Presenter 隐藏，非空时由 `FooterTemplate`、显式 Control、
字符串或 Avalonia DataTemplate 机制生成内容。Presenter 始终由 Empty owner 管理，用户 Footer 内容创建的子树不继承 `footer`
身份，也不属于 Empty 的其他 Semantic Part。

`Footer` 与 `FooterTemplate` 是 Empty 6.0 新增的 public StyledProperty。Empty 保持继承 `TemplatedControl`，不通过切换到
`ContentControl` 扩大或改变既有基类契约。

## 3. Selector 用法

生成的 Semantic Style 封装 owner-relative `SelectorRoute`，用户只需要在普通 Empty owner Style 中嵌套目标 Style，并显式提供
`x:SetterTargetType`：

```xml
<Application.Styles>
    <Style Selector="atom|Empty">
        <atom:EmptyImageStyle x:SetterTargetType="Control">
            <Setter Property="Opacity" Value="0.72" />
        </atom:EmptyImageStyle>

        <atom:EmptyDescriptionStyle x:SetterTargetType="atom:TextBlock">
            <Setter Property="Foreground" Value="DarkSlateBlue" />
        </atom:EmptyDescriptionStyle>

        <atom:EmptyFooterStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Margin" Value="0,16,0,0" />
        </atom:EmptyFooterStyle>
    </Style>
</Application.Styles>
```

对特定业务 class 定制时，把限定条件放在 owner 一侧：

```xml
<Style Selector="atom|Empty.compact-empty">
    <atom:EmptyImageStyle x:SetterTargetType="Control">
        <Setter Property="Height" Value="40" />
    </atom:EmptyImageStyle>
</Style>
```

不得使用以下写法：

- `.semantic-root` 或模板外层 `StackPanel` selector。
- `Svg#PART_SvgImage`、`TextBlock#Description` 或 Footer Presenter Name。
- `Control.semantic-image`、`:is(Control).semantic-image` 等类型前缀。
- 复制 `/template/ .semantic-*` 完整 route 作为主要用户写法。
- 进入 Footer 用户内容、SVG 内部绘制树或其他嵌套控件模板继续匹配 Empty Part。

## 4. 状态与数量语义

| 状态 | root | image | description | footer |
| --- | --- | --- | --- | --- |
| 默认描述、无 Footer | 1 | 1 | 1，可见 | 1，隐藏 |
| `IsDescriptionVisible=false` | 1 | 1 | 1，隐藏 | 1，按内容决定可见性 |
| `Description=null` 或空字符串 | 1 | 1 | 1，内容为空 | 1，按内容决定可见性 |
| `Footer=null` | 1 | 1 | 1 | 1，隐藏 |
| `Footer` 为字符串、Control 或模板化数据 | 1 | 1 | 1 | 1，可见 |
| Default/Simple/ImagePath/ImageSource | 1 | 1，同一 target | 1 | 1 |
| Large/Middle/Small | 1 | 1 | 1 | 1 |
| 模板重套用 | 1 个新 root owner | 1 个新 target | 1 个新 target | 1 个新 target |

Static marker 表达模板职责，内容存在性通过 `IsVisible` 和 Presenter 内容表达。不得为了模拟条件节点而在属性变化时动态创建、删除或
重新标记 Visual。

## 5. 尺寸、布局与 Token 边界

Empty 只支持 `Large`、`Middle`、`Small` 三档 `SizeType`：

| SizeType | image 高度 | description 上边距 | footer 上边距 |
| --- | --- | --- | --- |
| Large | `EmptyImgHeight` | `DescriptionMargin` | `FooterMargin` |
| Middle | `EmptyImgHeightMD` | `DescriptionMarginSM` | `FooterMargin` |
| Small | `EmptyImgHeightSM` | `DescriptionMarginSM` | `FooterMargin` |

Footer 间距不随 SizeType 改变，用于保持 footer 与前一内容区域之间的稳定 spacing。Semantic Style 可以覆盖 image `Height`、
description/footer `Margin` 等布局属性；最终 Measure/Arrange 仍受 root 对齐、调用方约束和 Footer 内容自身尺寸影响。
默认主题不得通过本地值或 C# 写入压制应用级 Semantic Setter。

## 6. 定制边界

以下区域明确不属于 Empty Semantic Part：

- 模板表面 `DashedBorder`、内部 `StackPanel`、`PART_SvgImage` 名称和 Description/Footer 节点 Name。
- 内置 Default/Simple SVG 的 path、shape、brush 和绘制子树。
- Footer 用户内容创建的 Button、文本、布局容器或 DataTemplate 子树。
- `BuiltInImageBuilder`、内部颜色属性和 TokenResourceBinder 等实现细节。
- 使用 Empty 作为 ListBox、ListView、TreeView 或 Cascader 默认 empty indicator 时的外部 Presenter。

Empty 不提供 Semantic Part Theme 替换，也不跨 VisualRoot。所有非 root Part 均使用 Selector customization；默认 ControlTheme 不得
消费 `.semantic-*` 作为自身样式机制。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality、移除任一 marker，或者使 Footer API 与 Presenter
语义不一致，均属于公共主题契约变更。

验证至少覆盖：

- descriptor 中只有 `root`、`description`、`footer`、`image`，字段值与本文一致。
- `EmptyImageStyle`、`EmptyDescriptionStyle`、`EmptyFooterStyle` 生成并能在 AXAML 中编译。
- 默认模板三个 marker 均为静态 `Classes.semantic-*="True"`，不出现 `.semantic-root`。
- `IsDescriptionVisible` 直接控制 description 节点可见性，模板应用前设置、运行时切换和 re-template 后都正确。
- Footer 支持 `null`、字符串、Control、`FooterTemplate` 和运行时替换，且旧内容不被 owner 保留。
- Default、Simple、ImagePath、ImageSource 切换不改变 image marker 身份。
- Large、Middle、Small 的 image 高度、description margin 和 footer margin 使用完整一致的尺寸基线。
- root 的背景、边框、圆角、Padding 和 `StrokeDashArray` 投影生效，三个生成 Style 在 Application、局部 Styles 与
  owner class scope 中遵循 Avalonia 样式优先级。
- Gallery Semantic Preview 只在首次选择 Semantic Parts Tab 后创建。
- 默认主题不消费 `.semantic-*`；未声明用户 Semantic Style 时不增加 VisualTree 搜索、反射或运行时 selector 组装。
