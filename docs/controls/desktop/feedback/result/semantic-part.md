# Result Semantic Part 契约

本文档定义 `Result` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。Result 的整体设计见
[Result 桌面版架构设计](overview.md)，真实模板节点、状态投影与尺寸基线见
[Result 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Result 的 Part 名称与公开 Result Semantic 结构保持一致：`root`、`icon`、`title`、`subTitle`、`extra`、`body`。
普通反馈图标和 403/404/500 异常图是同一 `icon` 职责的静态替代实现，不拆分为状态专属 Part。

### 1.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Result` |
| Part | `root` |
| Selector | Result 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Result` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 结果页 owner |
| 职责 | 承载整体结果布局、状态、表面属性和 Semantic Style 作用域。 |
| 相关 API | 全部 Result public API，包含 `StrokeDashArray` 与继承的标准表面属性 |
| 相关 Token | ResultToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

### 1.2 `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `Result` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `/template/ .semantic-icon` |
| Style Type | `ResultIconStyle` |
| ContractType | `Control` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 普通状态图标区域与异常状态图像 |
| 职责 | 表达 Info、Success、Warning、Error 图标和 403、404、500 图像的替代呈现。 |
| 相关 API | `Status`、`Icon` |
| 相关 Token | `StatusIconSize`、`StatusImageMargin`、`ImageWidth`、`ImageHeight`、状态色 Token |
| 稳定性 | stable since 6.2.0 |

`icon` 同时标记 `PART_StatusIconPresenter` 和 `PART_ErrorCodeImage`。两个 target 始终存在，`Status` 只切换可见性；
Semantic Style 必须适用于两个实现。它适合定制 Margin、Opacity、Width、Height 和对齐。普通 presenter 内由默认状态或 `Icon`
提供的 Child、异常 SVG 的内部图元和 source 文本不属于公开 Part。

### 1.3 `title`

| 字段 | 值 |
| --- | --- |
| Owner | `Result` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-title` |
| Style Type | `ResultTitleStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 主结果标题 presenter |
| 职责 | 展示 Header 内容并提供标题排版边界。 |
| 相关 API | `Header`、`HeaderTemplate`、`HeaderFontSize` |
| 相关 Token | `HeaderFontSize`、`HeaderMargin`、标题色与相对行高 Token |
| 稳定性 | stable since 6.2.0 |

`title` 始终存在，`Header=null` 时通过 `IsVisible=false` 隐藏。它适合定制 Foreground、FontSize、FontStyle、FontWeight、
LineHeight、Margin、Padding、Opacity、换行和对齐；HeaderTemplate 创建的用户子树不属于 Result Semantic Part。

### 1.4 `subTitle`

| 字段 | 值 |
| --- | --- |
| Owner | `Result` |
| Part | `subTitle` |
| Selector | `.semantic-sub-title` |
| SelectorRoute | `/template/ .semantic-sub-title` |
| Style Type | `ResultSubTitleStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 结果副标题 presenter |
| 职责 | 展示可选结果说明并提供副标题排版边界。 |
| 相关 API | `SubHeader`、`SubHeaderTemplate`、`SubHeaderFontSize` |
| 相关 Token | `SubHeaderFontSize`、描述色与相对行高 Token |
| 稳定性 | stable since 6.2.0 |

`subTitle` 始终存在，`SubHeader=null` 时隐藏。它适合定制 Foreground、FontSize、FontStyle、FontWeight、LineHeight、Margin、
Padding、Opacity、换行和对齐；SubHeaderTemplate 创建的用户子树不属于 Result Semantic Part。

### 1.5 `extra`

| 字段 | 值 |
| --- | --- |
| Owner | `Result` |
| Part | `extra` |
| Selector | `.semantic-extra` |
| SelectorRoute | `/template/ .semantic-extra` |
| Style Type | `ResultExtraStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 操作与辅助内容 presenter |
| 职责 | 承载 Extra 内容并提供操作区布局边界。 |
| 相关 API | `Extra`、`ExtraTemplate` |
| 相关 Token | `ExtraMargin` |
| 稳定性 | stable since 6.2.0 |

`extra` presenter 始终存在，横向拉伸到 Result 内容区宽度，并在区域内部居中排列 Extra 内容；`Extra=null` 时保持空内容。
它适合定制 Background、Padding、Margin、Opacity、对齐和 TextAlignment。调用方放入的 Button、Panel 或其他内容子树
拥有自己的主题契约，Result 不继续匹配其内部节点。

### 1.6 `body`

| 字段 | 值 |
| --- | --- |
| Owner | `Result` |
| Part | `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `/template/ .semantic-body` |
| Style Type | `ResultBodyStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 详细结果内容 presenter |
| 职责 | 承载 Content 并提供正文背景、内边距和外边距边界。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | `ContentMargin`、`ContentPadding`、`ColorFillAlter` |
| 稳定性 | stable since 6.2.0 |

`body` 始终存在，`Content=null` 时隐藏。它适合定制 Background、Padding、Margin、Opacity、对齐和 ClipToBounds；
ContentTemplate 创建的用户子树不属于 Result Semantic Part。

## 2. Selector 用法

生成的 Style Type 封装 owner-relative route，应用不直接复制 `/template/` selector：

```xml
<Style Selector="atom|Result.semantic-object-demo">
    <Setter Property="BorderBrush" Value="{atom:SharedTokenResource ColorText}" />
    <Setter Property="BorderThickness" Value="2" />
    <Setter Property="StrokeDashArray" Value="4,2" />
    <Setter Property="Padding" Value="16" />
    <atom:ResultIconStyle x:SetterTargetType="Control">
        <Setter Property="Opacity" Value="0.8" />
    </atom:ResultIconStyle>
    <atom:ResultTitleStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="FontStyle" Value="Italic" />
        <Setter Property="Foreground" Value="#1890FF" />
    </atom:ResultTitleStyle>
    <atom:ResultSubTitleStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="FontWeight" Value="Bold" />
    </atom:ResultSubTitleStyle>
    <atom:ResultExtraStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Background" Value="#F0F0F0" />
        <Setter Property="Padding" Value="8" />
    </atom:ResultExtraStyle>
    <atom:ResultBodyStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Background" Value="#FAFAFA" />
        <Setter Property="Padding" Value="12" />
    </atom:ResultBodyStyle>
</Style>
```

Gallery 的 object 与 status-based function 示例按 Ant Design Result Semantic 示例的内容、顺序和视觉值映射；AtomUI 使用
owner-scoped AXAML Style 表达相同结果，不引入 React `classNames` / `styles` API。

不得使用以下写法：

- `.semantic-root`、`#Header`、`#SubHeader`、`PART_StatusIconPresenter` 或其他 Name selector 作为应用主题契约。
- 为 Info、Success、Warning、Error、403、404、500 建立状态专属 icon Part。
- 复制 `/template/ .semantic-*` route 作为主要用户写法。
- 穿过 HeaderTemplate、SubHeaderTemplate、ExtraTemplate 或 ContentTemplate 的用户内容继续匹配内部 Visual。
- 用 Semantic Style 改写 `Status`、`Icon`、Header、SubHeader、Extra 或 Content 的 ownership。

## 3. 状态与数量语义

| 状态 | icon | title | subTitle | extra | body |
| --- | --- | --- | --- | --- | --- |
| Info / Success / Warning / Error | 2 个 target，普通 presenter 可见 | 1 | 1 | 1 | 1 |
| ErrorCode403 / ErrorCode404 / ErrorCode500 | 2 个 target，异常 SVG 可见 | 1 | 1 | 1 | 1 |
| 自定义 `Icon` | 2 个 target，普通 presenter Child 替换 | 不变 | 不变 | 不变 | 不变 |
| Header / SubHeader 为空 | 不变 | 1，按空值隐藏 | 1，按空值隐藏 | 不变 | 不变 |
| Extra 为空 | 不变 | 不变 | 不变 | 1，空内容 | 不变 |
| Content 为空 | 不变 | 不变 | 不变 | 不变 | 1，隐藏 |
| 模板重套用 | 2 个新 target | 1 个新 target | 1 个新 target | 1 个新 target | 1 个新 target |

隐藏状态通过 `IsVisible` 或空内容表达，不改变 descriptor 或 marker 数量。模板重套用时旧普通图标 presenter 的
PropertyChanged 订阅必须解除，新 presenter 重新订阅；其他 Part 不创建 C# binding、订阅或运行时 marker。

## 4. 定制边界

以下区域不属于 Result Semantic Part：

- `DashedBorder#Frame`、`StackPanel#RootLayout` 和内部纵向排列顺序。
- `PART_StatusIconPresenter.Child`、默认 `PathIcon` 类型、异常 SVG 的内部图元与 Source 文本。
- HeaderTemplate、SubHeaderTemplate、ExtraTemplate 和 ContentTemplate 创建的用户 Visual 子树。
- `StatusIcon`、`StatusIconSize`、`StatusIconBrush`、相对行高与绝对行高内部属性。
- `ResultToken`、状态 selector、默认图标创建和异常图 source 选择逻辑。

默认 ControlTheme 不得消费 `.semantic-*` 作为自身样式机制。Result 不提供 Semantic Part Theme 替换，不跨 VisualRoot，
也不在运行时搜索或发现 target。

## 5. 尺寸基线

Result 没有 `SizeType` 或紧凑模式。默认尺寸由以下单一 Token/API 基线组成：

| 区域 | 尺寸来源 |
| --- | --- |
| root | `FramePadding` |
| 普通 icon | `StatusIconSize`、`StatusImageMargin` |
| 异常 icon | `ImageWidth`、`ImageHeight`、`StatusImageMargin` |
| title | `HeaderFontSize`、`RelativeLineHeightHeading3`、`HeaderMargin` |
| subTitle | `SubHeaderFontSize`、`RelativeLineHeight` |
| extra | `ExtraMargin` |
| body | `ContentMargin`、`ContentPadding` |

Semantic Style 对局部属性的覆盖不能让默认模板依赖固定 Height、MinHeight 或示例像素偏移。图标 target 切换、模板重套用和
Header/SubHeader 字号变化后，控件必须重新使用同一基线完成 Measure 与行高计算。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality、移除静态 marker，或让默认主题依赖
`.semantic-*`，均属于公共主题契约变更。

验证至少覆盖：

- descriptor 中只有六个已批准 Part，字段值与本文一致。
- 五个生成 Style Type 可以在 AXAML 中编译并命中全部真实 target。
- 内置模板存在六个静态 marker target，不出现 `.semantic-root`。
- 七种 `Status`、自定义 `Icon`、Header/SubHeader/Extra/Content 空值不改变 target 身份或数量。
- `extra` target 横向占满 Result 内容区，Extra 内容保持居中；Background 和 Padding 作用于完整操作区域。
- 模板重套用释放旧普通图标 presenter 订阅，并恢复全部 marker、当前图标尺寸、画刷和文本行高。
- 默认主题不消费 `.semantic-*`，未声明用户 Style 时不增加反射、VisualTree 搜索或运行时 selector 组装。
- Gallery Semantic Preview 只在首次选择 Semantic Parts Tab 后创建，并展示六项 Part 描述。
- Gallery object/function 样式示例严格映射 Ant Design Result 示例的控件数量、顺序、文案和视觉值，只使用 owner-scoped 生成 Style。
- Gallery 所有 Semantic Part Tag 使用当前 AtomUI 版本 `v6.1.3`，不复制上游 Semantic DOM 的 since 版本。
