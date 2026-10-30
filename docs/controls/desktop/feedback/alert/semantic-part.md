# Alert Semantic Part 契约

本文档定义 Alert 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。Alert 的整体设计见
[Alert 桌面版架构设计](overview.md)，真实模板节点、状态与关闭按钮生命周期见
[Alert 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Alert 只有一个 public descriptor owner：`Alert`。

| Part | Selector | Style Type | ContractType | Cardinality | AtomUI 节点 | 稳定性 |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | Alert 本身 | 不适用 | `Alert` | `Single` | owner | stable since 6.2.0 |
| `icon` | `.semantic-icon` | `AlertIconStyle` | `Icon` | `Multiple` | 四个 `AlertType` 图标 | stable since 6.2.0 |
| `section` | `.semantic-section` | `AlertSectionStyle` | `StackPanel` | `Single` | 消息与描述布局 | stable since 6.2.0 |
| `title` | `.semantic-title` | `AlertTitleStyle` | `Control` | `Multiple` | `MessageLabel` 与 `MarqueeLabel` | stable since 6.2.0 |
| `description` | `.semantic-description` | `AlertDescriptionStyle` | `Label` | `Single` | `DescriptionLabel` | stable since 6.2.0 |
| `actions` | `.semantic-actions` | `AlertActionsStyle` | `ContentPresenter` | `Single` | `ExtraActionPresenter` | stable since 6.2.0 |
| `close` | `.semantic-close` | `AlertCloseStyle` | `IconButton` | `Single` | `PART_CloseBtn` | stable since 6.2.0 |

六个 selector Part 的 `SelectorRoute` 均为 `/template/ .semantic-<name>`，`Customization` 为 `Selector`，
`CrossVisualRoot=false`，`RuntimeCreated=false`。root 的 `Customization` 为 `Root`，不生成 `.semantic-root` 或 Style Type。

## 2. Part 说明

### 2.1 `root`

root 是 Alert 控件实例本身，负责 `Type`、内容、关闭请求和其余 Part 的样式作用域。适合定制 `Background`、`BorderBrush`、
`BorderThickness`、`StrokeDashArray`、`CornerRadius`、`Padding`、Margin、对齐、透明度与整体可见性。默认模板通过
`PixelAlignedBorder` 投影标准表面属性；该内部 frame 不是独立 Part。

### 2.2 `icon`

icon 由 Success、Info、Warning、Error 四个静态 `Icon` target 实现。`Type` 只决定当前显示哪一个图标，`IsShowIcon=false`
隐藏其共同父容器；两种状态都不删除或重新标记 target，因此 cardinality 为 `Multiple`。

适合定制 Opacity、Width、Height、Margin、对齐和 Icon 支持的画刷属性。Semantic Style 必须同时适用于四个替代实现，不能通过
`#SuccessIcon` 等内部 Name 把一个状态提升为新的公共 Part。

### 2.3 `section`

section 是纵向组织 title 与 description 的唯一 `StackPanel`。它始终存在，适合定制 `Spacing`、Margin、对齐、Opacity 和
`ClipToBounds`。section 不包含左侧图标、右侧 actions 或 close，也不公开 `DockPanel#RootLayout` 的停靠顺序。

### 2.4 `title`

title 由普通 `Label` 和 `MarqueeLabel` 两个静态替代 target 实现。`IsMessageMarqueeEnabled` 在两者之间切换可见性，两个节点都
消费 `Message`，所以 cardinality 为 `Multiple`，最低稳定 `ContractType` 为 `Control`。

适合通过 `TextElement` attached property 定制字体、前景色和排版，也可以调整 Background、Padding、Margin、Opacity 与对齐。
Semantic Style 不负责切换跑马灯状态或修改 `Message` ownership。

### 2.5 `description`

description 是唯一 `Label` target。`Description` 为 null 或空字符串时节点隐藏但不销毁。适合定制字体、Foreground、
Background、Margin、Padding、Opacity、换行和对齐；不包含用户消息或其他 Part。

### 2.6 `actions`

actions 是承载 `ExtraAction` 的唯一 `ContentPresenter`。`ExtraAction=null` 时 presenter 隐藏，重新赋值时复用同一 target。
适合定制 Background、Padding、Margin、CornerRadius、Opacity 和对齐。调用方提供的 `Control` 子树不属于 Alert 的
Semantic Part，不能继续依赖其内部结构。

### 2.7 `close`

close 是唯一 `IconButton` target。`IsClosable=false` 时按钮隐藏但仍存在。它适合定制 Background、Padding、Margin、
CornerRadius、Opacity、对齐以及 `IconButton` 的图标尺寸。点击只触发 `CloseRequest`；Alert 不自动从视觉树移除，也不拥有
关闭动画，因此不存在 closing motion 阶段的额外 marker。

## 3. Selector 用法

生成的 Style Type 封装 owner-relative route，应用不直接复制 `/template/` selector：

```xml
<Style Selector="atom|Alert.semantic-style-demo">
    <Setter Property="BorderBrush" Value="#CCCCCC" />
    <Setter Property="BorderThickness" Value="2" />
    <Setter Property="StrokeDashArray" Value="4,2" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="12" />
</Style>

<Style Selector="atom|Alert.semantic-object-demo">
    <atom:AlertIconStyle x:SetterTargetType="atom:Icon">
        <Setter Property="Width" Value="18" />
        <Setter Property="Height" Value="18" />
    </atom:AlertIconStyle>
    <atom:AlertSectionStyle x:SetterTargetType="StackPanel">
        <Setter Property="TextElement.FontWeight" Value="Medium" />
    </atom:AlertSectionStyle>
</Style>

<Style Selector="atom|Alert.semantic-function-demo">
    <Setter Property="Background" Value="#1A52C41A" />
    <Setter Property="BorderBrush" Value="#B7EB8F" />
    <atom:AlertIconStyle x:SetterTargetType="atom:Icon">
        <Setter Property="FillBrush" Value="#52C41A" />
    </atom:AlertIconStyle>
</Style>
```

Gallery 的 `Object styles` 与 `Function styles` 两条示例按 Ant Design Alert Semantic DOM 示例的内容和视觉值映射；AtomUI
使用 owner-scoped AXAML Style 表达相同结果，不引入 React `classNames` / `styles` API。

不得使用以下写法：

- `.semantic-root`、`DockPanel#RootLayout`、`#SuccessIcon`、`#MessageLabel` 或其他 Name selector 作为应用主题契约。
- 为 icon 的四种 `AlertType` 或 title 的两种呈现建立第二套 semantic alias。
- 复制 `/template/ .semantic-*` 完整 route 作为主要用户写法。
- 穿过 actions 的用户内容继续匹配其内部 Visual。
- 用 Semantic Style 改写 `Type`、`Message`、`Description`、`ExtraAction`、`IsClosable` 或跑马灯状态 ownership。

## 4. 状态与数量语义

| 状态 | icon | section | title | description | actions | close |
| --- | --- | --- | --- | --- | --- | --- |
| 默认无可选内容 | 4 个 target，父容器隐藏 | 1 | 2，普通标题可见 | 1，隐藏 | 1，隐藏 | 1，隐藏 |
| `IsShowIcon=true` | 4 个 target，仅当前 `Type` 可见 | 1 | 2 | 不变 | 不变 | 不变 |
| `IsMessageMarqueeEnabled=true` | 不变 | 1 | 2，跑马灯可见 | 不变 | 不变 | 不变 |
| Description 非空 | 不变 | 1 | 2 | 1，可见 | 不变 | 不变 |
| ExtraAction 非 null | 不变 | 1 | 2 | 不变 | 1，可见 | 不变 |
| `IsClosable=true` | 不变 | 1 | 2 | 不变 | 不变 | 1，可见 |
| `Type` 切换 | target 身份不变，仅可见图标变化 | 1 | 2 | 1 | 1 | 1 |
| 模板重套用 | 4 个新 target | 1 个新 target | 2 个新 target | 1 个新 target | 1 个新 target | 1 个新 target |

隐藏状态通过 `IsVisible` 表达，不改变 descriptor 或 marker 数量。模板重套用时旧 close button 事件订阅必须解除，新 close button
重新订阅；其余 Part 不创建 C# binding、订阅或运行时 marker。

## 5. 定制边界

以下区域不属于 Alert Semantic Part：

- `PixelAlignedBorder`、`DockPanel#RootLayout`、图标共同父 `Panel` 和内部 Dock 顺序。
- `SuccessIcon`、`InfoIcon`、`WarningIcon`、`ErrorIcon` 的 Name 与具体图标类型。
- title/description 文本内部 run、MarqueeLabel 的动画实现和 close button 的内部 Icon presenter。
- `ExtraAction` 提供的用户 Control 子树。
- `AlertToken`、`CloseIcon` 默认值回退、伪类同步和 `CloseRequest` 事件处理逻辑。

默认 ControlTheme 不得消费 `.semantic-*` 作为自身样式机制。Alert 不提供 Semantic Part Theme 替换，不跨 VisualRoot，也不在
运行时搜索或发现 target。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality、移除静态 marker，或让默认主题依赖
`.semantic-*`，均属于公共主题契约变更。

验证至少覆盖：

- descriptor 中只有七个已批准 Part，字段值与本文一致。
- 六个生成 Style Type 可以在 AXAML 中编译并命中全部真实 target。
- 内置模板存在十个静态 marker target，不出现 `.semantic-root`。
- 四种 `Type`、icon/description/actions/close 显隐和跑马灯切换不改变 target 身份。
- 模板应用后增删 `ExtraAction` 会同步更新 actions 可见性和 `:has-extra-action`。
- 模板重套用释放旧 close button 订阅，并恢复全部 marker。
- 默认主题不消费 `.semantic-*`，未声明用户 Style 时不增加反射、VisualTree 搜索或运行时 selector 组装。
- Gallery Semantic Preview 只在首次选择 Semantic Parts Tab 后创建，并展示七项 Part 描述。
- Gallery 样式示例只使用 owner-scoped 生成 Style，不依赖内部 Name、原始 route 或固定高度。
