# Card Semantic Part 契约

本文档定义 `Card` 和 `CardMetaContent` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。Card 的
整体设计见 [Card 桌面版架构设计](overview.md)，descriptor 与真实模板节点映射见
[Card 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`Card` 与 `CardMetaContent` 是两个独立 public owner。同名 `title` 表示相近的产品职责，不表示两个 owner 共享模板节点、
状态或样式作用域。

### 1.1 `Card`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Card` |
| Part | `root` |
| Selector | Card 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Card` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Card owner |
| 职责 | Card root 是外观、尺寸、加载、悬停和内容组合的统一 owner。 |
| 相关 API | 全部 Card public API |
| 相关 Token | CardToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `header`

| 字段 | 值 |
| --- | --- |
| Owner | `Card` |
| Part | `header` |
| Selector | `.semantic-header` |
| SelectorRoute | `/template/ .semantic-header` |
| Style Type | `CardHeaderStyle` |
| ContractType | `DashedBorder` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Header frame |
| 职责 | 表示完整头部表面和标题、额外内容的共同布局边界。 |
| 相关 API | `Header`、`HeaderTemplate`、`Extra`、`ExtraTemplate`、`SizeType`、`IsInnerMode` |
| 相关 Token | Header、Extra、Border、Radius Token |
| 稳定性 | stable since 6.2.0 |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner | `Card` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-title` |
| Style Type | `CardTitleStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Header title presenter |
| 职责 | 表示 Card 标题的展示与排版区域。 |
| 相关 API | `Header`、`HeaderTemplate` |
| 相关 Token | Header typography Token |
| 稳定性 | stable since 6.2.0 |

#### `extra`

| 字段 | 值 |
| --- | --- |
| Owner | `Card` |
| Part | `extra` |
| Selector | `.semantic-extra` |
| SelectorRoute | `/template/ .semantic-extra` |
| Style Type | `CardExtraStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Header extra presenter |
| 职责 | 表示头部尾侧的辅助内容区域。 |
| 相关 API | `Extra`、`ExtraTemplate` |
| 相关 Token | `ExtraColor`、Header padding Token |
| 稳定性 | stable since 6.2.0 |

#### `cover`

| 字段 | 值 |
| --- | --- |
| Owner | `Card` |
| Part | `cover` |
| Selector | `.semantic-cover` |
| SelectorRoute | `/template/ .semantic-cover` |
| Style Type | `CardCoverStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Cover frame |
| 职责 | 表示封面内容的裁剪、圆角和布局边界。 |
| 相关 API | `Cover`、`CoverTemplate` |
| 相关 Token | Shared radius Token |
| 稳定性 | stable since 6.2.0 |

#### `body`

| 字段 | 值 |
| --- | --- |
| Owner | `Card` |
| Part | `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `/template/ .semantic-body` |
| Style Type | `CardBodyStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Body frame |
| 职责 | 表示普通、Meta、Grid、Tabs 和 Loading 内容共享的主体表面。 |
| 相关 API | `Content`、`ContentTemplate`、`IsLoading`、`SizeType` |
| 相关 Token | Body padding Token |
| 稳定性 | stable since 6.2.0 |

#### `actions`

| 字段 | 值 |
| --- | --- |
| Owner | `Card` |
| Part | `actions` |
| Selector | `.semantic-actions` |
| SelectorRoute | `/template/ .semantic-actions` |
| Style Type | `CardActionsStyle` |
| ContractType | `TemplatedControl` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Action panel owner |
| 职责 | 表示底部操作组的完整表面、均分布局和分隔线边界。 |
| 相关 API | `Actions`、`IsMotionEnabled` |
| 相关 Token | Actions、Border、Radius Token |
| 稳定性 | stable since 6.2.0 |

### 1.2 `CardMetaContent`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `CardMetaContent` |
| Part | `root` |
| Selector | CardMetaContent 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `CardMetaContent` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | CardMetaContent owner |
| 职责 | Meta root 是头像、标题和描述组合的统一 owner。 |
| 相关 API | `Avatar`、`Header`、`HeaderTemplate`、`Content`、`ContentTemplate` |
| 相关 Token | Shared typography、spacing Token |
| 稳定性 | stable since 6.2.0 |

#### `section`

| 字段 | 值 |
| --- | --- |
| Owner | `CardMetaContent` |
| Part | `section` |
| Selector | `.semantic-section` |
| SelectorRoute | `/template/ .semantic-section` |
| Style Type | `CardMetaContentSectionStyle` |
| ContractType | `Control` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Meta detail section |
| 职责 | 表示标题与描述共同占用的详情布局区域。 |
| 相关 API | `Header`、`HeaderTemplate`、`Content`、`ContentTemplate` |
| 相关 Token | Shared spacing Token |
| 稳定性 | stable since 6.2.0 |

#### `avatar`

| 字段 | 值 |
| --- | --- |
| Owner | `CardMetaContent` |
| Part | `avatar` |
| Selector | `.semantic-avatar` |
| SelectorRoute | `/template/ .semantic-avatar` |
| Style Type | `CardMetaContentAvatarStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Avatar presenter |
| 职责 | 表示 Meta 头像内容的展示与布局区域。 |
| 相关 API | `Avatar` |
| 相关 Token | Shared spacing Token |
| 稳定性 | stable since 6.2.0 |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner | `CardMetaContent` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-title` |
| Style Type | `CardMetaContentTitleStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Meta title presenter |
| 职责 | 表示 Meta 标题的展示与排版区域。 |
| 相关 API | `Header`、`HeaderTemplate` |
| 相关 Token | Shared heading typography Token |
| 稳定性 | stable since 6.2.0 |

#### `description`

| 字段 | 值 |
| --- | --- |
| Owner | `CardMetaContent` |
| Part | `description` |
| Selector | `.semantic-description` |
| SelectorRoute | `/template/ .semantic-description` |
| Style Type | `CardMetaContentDescriptionStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Meta description presenter |
| 职责 | 表示 Meta 描述内容的展示与排版区域。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | Shared description typography Token |
| 稳定性 | stable since 6.2.0 |

所有 root 都是隐式 Part，不添加 `.semantic-root`。`ContractType` 是 Setter 可以稳定依赖的最低 public 类型，并通过
`x:SetterTargetType` 提供 AXAML 编译期类型上下文；它不参与 `.semantic-*` 的身份匹配。

## 2. Card Parts

### 2.1 root

root 是 Card owner 本身，在 Card 实例生命周期内始终存在且恰好一个。它负责 `StyleVariant`、`SizeType`、`IsLoading`、
`IsInnerMode`、`IsHoverable`、边框、背景、圆角、阴影、动效和全部内容入口，也是其他 Card Part 的 owner-scoped
Selector 边界。

适合在 root 上定制整体背景、边框、圆角、阴影、透明度、Margin 和状态组合。root 不表示模板中的背景 Frame、DockPanel、
Skeleton 或任一内容 presenter；这些节点的 Name、数量和层级不属于 root 契约。

### 2.2 header

header 表示完整头部表面，包含标题与额外内容的共同布局边界。内置模板始终创建该节点；`:headerless` 只把它隐藏，不改变
Part 数量，因此 cardinality 为 `Single`。

适合定制 Background、BorderBrush、BorderThickness、CornerRadius、Padding、MinHeight、Opacity 和 Effect。布局型 Setter
必须覆盖 Large、Middle、Small、headerless 和 inner mode；`SizeType` 仍提供 Header 的默认最小高度、padding 和字体基线。

header 不公开其内部 DockPanel、标题与额外内容的排列算法，也不包含 Cover 或 body。

### 2.3 title

title 是 Header 中唯一标题 presenter。`Header` 为 `null` 或使用 `HeaderTemplate` 时节点仍然存在，内容和值类型可以变化，Part
身份和数量不变。

适合定制 Foreground、FontSize、FontWeight、Opacity、Margin、Padding 和对齐属性。它只覆盖标题 presenter，不进入
`HeaderTemplate` 生成的用户子树。

### 2.4 extra

extra 是 Header 尾侧唯一辅助内容 presenter。`Extra` 为 `null` 或使用 `ExtraTemplate` 时节点仍然存在；headerless 状态下随
整个 header 隐藏。

适合定制 Foreground、FontSize、Opacity、Margin、Padding、Min/Max 和对齐属性。它不公开额外内容内部的按钮、链接、图标或
用户模板子树。

### 2.5 cover

cover 表示封面 frame，而不是封面值本身。内置模板始终创建该节点；`Cover` 为 `null` 时它保持空内容和零自然高度，因此为
`Single`，不是 `Optional`。

适合定制 Background、BorderBrush、BorderThickness、CornerRadius、Padding、Opacity、Effect 和尺寸约束。封面默认启用
裁剪；修改布局、圆角或变换时必须检查有 Header、headerless、不同图片比例和空 Cover。cover 不进入 `CoverTemplate` 或
Image 的内部视觉。

### 2.6 body

body 是 Card 主体的唯一稳定 frame。普通内容、`CardMetaContent`、`CardGridContent`、`CardTabsContent` 和 Skeleton loading
都位于该边界内，内容类型切换不会替换 body Part。

适合定制 Background、BorderBrush、BorderThickness、CornerRadius、Padding、Opacity 和 Effect。布局型 Setter 必须同时验证
三档 `SizeType`、普通/Meta/Grid/Tabs 内容与 Loading；Grid 内容默认由 item 自己提供 padding，Semantic body Padding 是用户
显式覆盖，可能改变该布局。

body 不公开 Skeleton 内部节点、当前 `Content` 的用户子树、Grid item、TabControl 或 Meta 的内部 Part。嵌套
`CardMetaContent` 必须继续以自身 owner Selector 定制。

### 2.7 actions

actions 表示完整底部操作组。内置模板始终创建 action panel；`Actions.Count=0` 时只隐藏该节点，动态增删 action 不改变
Part identity，因此 cardinality 为 `Single`。

适合定制 Background、BorderBrush、BorderThickness、CornerRadius、Padding、MinHeight、Opacity 和 Effect。它以公开
`TemplatedControl` 作为最低类型，不公开 internal action panel 类型。布局型 Setter 必须验证零个、一个和多个 action、动态
集合变化、不同 Card 宽度以及 action 内容高度。

actions 不表示单个 action。内部 `UniformGrid`、Render 阶段绘制的分隔线几何、action 子控件的模板和 `CardActionButton`
内部图标均不属于 Card actions Part。

## 3. CardMetaContent Parts

### 3.1 root

root 是 CardMetaContent owner 本身，在实例生命周期内始终存在且恰好一个。它负责 Avatar、Header 和 Content 三组 public
内容入口，并作为 `section`、`avatar`、`title` 和 `description` 的 owner-scoped Selector 边界。

适合定制整体 Margin、Opacity、对齐、尺寸和 Effect。root 不承诺当前 DockPanel 层级、Dock 顺序或用户内容类型。

### 3.2 section

section 表示 title 与 description 共同占用的详情区域。内置模板始终创建该节点，即使 Header 和 Content 同时为空，
cardinality 仍为 `Single`。

适合定制 Margin、Opacity、Width、Min/Max、对齐和 Effect。`ContractType=Control` 不承诺具体 Panel 类型、Dock 行为或 spacing
属性；需要调整标题和描述自身间距时，应分别定制 title 与 description。

### 3.3 avatar

avatar 是 Meta 头像内容的唯一 presenter。`Avatar=null` 时 presenter 仍存在但没有内容，因此为 `Single`。

适合定制 Width、Height、Margin、Opacity、对齐和 Effect。它不把实际 `Avatar` 控件或 `Avatar` 的模板子树提升为
CardMetaContent Part；实际头像控件继续遵守自身公共契约。

### 3.4 title

title 是 Meta 标题的唯一 presenter。它负责 `Header` / `HeaderTemplate` 的最终展示和标题排版，适合定制 Foreground、
FontSize、FontWeight、Opacity、Margin、Padding 和文本对齐。

title 不进入 `HeaderTemplate` 创建的用户子树。它与 Card owner 的同名 title 通过 owner Selector 隔离。

### 3.5 description

description 是 Meta 描述内容的唯一 presenter。它负责 `Content` / `ContentTemplate` 的最终展示，适合定制 Foreground、
FontSize、FontWeight、Opacity、Margin、Padding 和文本对齐。

description 不进入 `ContentTemplate` 创建的用户子树，也不表示 Card owner 的 body。

## 4. Selector 用法

应用级样式先限定实际 owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|Card">
        <atom:CardHeaderStyle x:SetterTargetType="atom:DashedBorder">
            <Setter Property="Padding" Value="20" />
        </atom:CardHeaderStyle>

        <atom:CardActionsStyle x:SetterTargetType="TemplatedControl">
            <Setter Property="Background" Value="#F5F5F5" />
        </atom:CardActionsStyle>
    </Style>

    <Style Selector="atom|CardMetaContent">
        <atom:CardMetaContentAvatarStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Width" Value="48" />
            <Setter Property="Height" Value="48" />
        </atom:CardMetaContentAvatarStyle>
    </Style>
</Application.Styles>
```

Card 与 CardMetaContent 都公开 `.semantic-title`，必须通过 owner 区分：

```xml
<Style Selector="atom|Card">
    <atom:CardTitleStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="FontWeight" Value="Bold" />
    </atom:CardTitleStyle>
</Style>

<Style Selector="atom|CardMetaContent">
    <atom:CardMetaContentTitleStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="FontWeight" Value="SemiBold" />
    </atom:CardMetaContentTitleStyle>
</Style>
```

不得使用以下 Selector：

- `atom|DashedBorder.semantic-header`、`:is(atom|DashedBorder).semantic-header` 或 `ContentPresenter.semantic-title`。
- 直接复制 `/template/ .semantic-*` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 从 Card 连续穿过 body 内容和 CardMetaContent 模板的多个 `/template/`。
- internal `CardActionPanel` 类型、`PART_ActionPanel`、`PART_GridPanel`、`PART_ItemsPresenter` 或 `PART_TabControl`。
- `HeaderFrame`、`CardContent` 等当前节点 Name，或视觉子节点顺序。

## 5. 状态与数量语义

数量契约以已实例化的 AtomUI 内置模板为边界。模板尚未应用时只有 owner root 存在；模板应用后，各静态 Part 节点保持稳定。

| 场景 | Card root | header | title | extra | cover | body | actions | 说明 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 普通 Card | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 所有静态节点由同一根模板创建。 |
| headerless | 1 | 1 | 1 | 1 | 1 | 1 | 1 | header 及其子节点隐藏，数量不变。 |
| `Cover=null` | 1 | 1 | 1 | 1 | 1 | 1 | 1 | cover frame 为空并保持零自然高度。 |
| `Actions.Count=0` | 1 | 1 | 1 | 1 | 1 | 1 | 1 | action panel 隐藏，数量不变。 |
| `IsLoading=true` | 1 | 1 | 1 | 1 | 1 | 1 | 1 | Skeleton 在 body 内切换展示，不替换 body marker。 |
| Meta / Grid / Tabs 内容 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | ContentType 只改变布局基线，不改变 Card Part 数量。 |
| Actions 动态增删 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 子 action 数量变化不改变 actions Part 数量。 |

| 场景 | CardMetaContent root | section | avatar | title | description | 说明 |
| --- | --- | --- | --- | --- | --- | --- |
| 完整 Meta | 1 | 1 | 1 | 1 | 1 | 所有静态节点由同一模板创建。 |
| 无 Avatar | 1 | 1 | 1 | 1 | 1 | avatar presenter 为空，数量不变。 |
| 无 Header | 1 | 1 | 1 | 1 | 1 | title presenter 为空，数量不变。 |
| 无 Content | 1 | 1 | 1 | 1 | 1 | description presenter 为空，数量不变。 |
| Card Loading | 1 | 1 | 1 | 1 | 1 | Meta owner 可继续存在于 Skeleton 内容中；Card body 的 Part 身份独立。 |

## 6. 定制边界

以下区域明确不属于 Card 家族 Semantic Part：

- Card 根模板中的背景 Frame、DockPanel、Skeleton 内部节点和 ContentType 实现细节。
- Header 内部排列容器、Cover/Body/Title/Extra 的用户模板子树。
- action panel 的 `UniformGrid`、单个 action、分隔线几何和 action 子控件模板。
- `CardGridContent` 的 ItemsPresenter、Grid panel 和动态生成的 `CardGridItem`。
- `CardGridItem` 的 frame、content presenter、row/column/span 和 hover 视觉。
- `CardTabsContent` 内部 TabControl、tab item、tab header、tab content 和 extra content。
- `CardActionButton` 的图标、内容和交互状态。
- CardMetaContent 的具体 Panel 类型、用户 Avatar 子树、HeaderTemplate 和 ContentTemplate 子树。

`CardGridContent`、`CardGridItem`、`CardTabsContent` 和 `CardActionButton` 虽是 public 组合类型，但不声明本次 Card Semantic Part
descriptor。它们的 public API 继续可正常使用；是否拥有独立 Semantic Part 只由各自明确的公共契约决定，不能从 Card body、
actions 或通用控件结构中推导。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；最终布局仍由 Card owner、Part 与中间
节点共同完成 Measure/Arrange。布局型 Setter 必须检查 `SizeType`、Min/Max、Padding、Margin、CornerRadius、裁剪、内容类型和
用户内容自然尺寸，不能把跨节点布局约束解释为 Semantic Style 优先级失效。

`SizeType` 与 Semantic Style 不是互斥入口。owner-scoped Semantic Setter 命中属性后，以更高的样式优先级覆盖 Card Theme
对同一属性提供的值；未被 Setter 覆盖的相关布局属性仍来自当前 `SizeType` 分支。因此，包含布局型 Setter 的样例和应用样式
必须先选择一套完整的 `Large`、`Middle` 或 `Small` 基线，再只覆盖确实需要修改的 Part 属性。不得把一档尺寸的 Header
`Padding`、字体或 Body `Padding` 局部叠加到另一档尺寸的 `MinHeight` 基线上；若产品确实需要混合规格，必须显式接管全部
相互约束的布局属性，并验证 Header、Body、Extra 和内容自然尺寸共同参与的 Measure/Arrange 结果。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少 marker，均属于
公共主题契约变更。把未声明的 Card 子控件提升为新的 Semantic Part owner，也必须先完成独立事实审计和文档审核。

验证至少覆盖：

- `Card` descriptor 只有 `root/header/title/extra/cover/body/actions`，字段值与本文表格一致。
- `CardMetaContent` descriptor 只有 `root/section/avatar/title/description`，并与 Card 同名 title 保持 owner 隔离。
- headerless、空 Cover、空 Actions、Loading、Meta、Grid、Tabs 和动态 Actions 下 marker 数量保持稳定。
- CardMetaContent 的 Avatar/Header/Content 为空时 marker 仍为 Single，用户模板子树不被错误标记。
- owner-scoped class-only template Selector 与 `x:SetterTargetType` 可以编译并命中对应最低 public 类型。
- Card actions marker 停留在 Card 根模板的 action panel owner，不连续穿入 internal action panel 模板。
- `CardGridContent`、`CardGridItem`、`CardTabsContent` 和 `CardActionButton` 不产生 Card 家族额外 descriptor。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射、VisualTree 扫描或运行时 descriptor 发现。
