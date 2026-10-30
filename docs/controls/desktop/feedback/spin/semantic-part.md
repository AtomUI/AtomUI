# Spin Semantic Part 契约

本文档定义 Spin 家族公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。Spin 的整体设计见
[Spin 桌面版架构设计](overview.md)，真实模板与生命周期见 [Spin 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Spin 主控件公开 `root`、`container`、`mask`、`section`、`indicator`、`description` 六个职责区域，与上游稳定语义
保持同名。`mask` 是 AtomUI 扩展区域，承载嵌套模式的遮罩背景层职责：上游体系的遮罩语义已折叠进全屏模式的根区域，
AtomUI 没有全屏模式，遮罩层是嵌套模式下独立的真实节点。`SpinIndicator` 是独立的公开子控件，
公开 `root` 与 `content`。

### 1.1 `Spin`

| Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | owner | `Spin` | `Single` | `Root` | `false` | `false` |
| `container` | `.semantic-container` | `ContentPresenter` | `Single` | `Selector` | `false` | `false` |
| `mask` | `.semantic-mask` | `Border` | `Single` | `Selector` | `false` | `false` |
| `section` | `.semantic-section` | `StackPanel` | `Single` | `Selector` | `false` | `false` |
| `indicator` | `.semantic-indicator` | `SpinIndicator` | `Single` | `Selector` | `false` | `false` |
| `description` | `.semantic-description` | `TextBlock` | `Single` | `Selector` | `false` | `false` |

`container` 承载用户内容的 `ContentPresenter`，spinning 时承担透明度或高斯模糊反馈；`mask` 是遮罩背景层，只在
`IsMaskBackgroundEnabled` 时呈现 `ColorBgMask`；`section` 是加载区域，承载 `indicator` 与 `description` 并居中；
`indicator` 是主控件模板直接拥有的公开 `SpinIndicator` 子控件，其尺寸分支、动效时长和圆点填充
（`DotBgBrush`）均可经 Semantic Style 定制；`description` 是提示文本节点，对应公共 API
`Tip` / `IsTipVisible`。`root` 不声明 `.semantic-root` marker。

### 1.2 `SpinIndicator`

| Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | owner | `SpinIndicator` | `Single` | `Root` | `false` | `false` |
| `content` | `.semantic-content` | `Control` | `Multiple` | `Selector` | `false` | `false` |

`content` 使用 `Multiple` 是 AtomUI 模板实现的状态替代语义：内置四点指示器 `SpinIndicatorDotPanel` 与自定义指示器
`PART_CustomIndicatorPresenter` 是两个静态 marker target，由 `IsCustomIndicator` 决定同一时刻只有一个可见。
Preview 只高亮当前可见 target；Semantic Style 同时作用于两个替代实现，保证切换自定义指示器后定制仍然存在。

## 2. Selector 用法

应用侧只使用 owner-scoped selector 和生成的 Style 类型，不复制 `/template/` 路径：

```xml
<Style Selector="atom|Spin.semantic-style-demo">
    <atom:SpinSectionStyle x:SetterTargetType="StackPanel">
        <Setter Property="Spacing" Value="12" />
    </atom:SpinSectionStyle>
    <atom:SpinIndicatorStyle x:SetterTargetType="atom:SpinIndicator">
        <Setter Property="DotBgBrush" Value="#00d4ff" />
    </atom:SpinIndicatorStyle>
    <atom:SpinDescriptionStyle x:SetterTargetType="TextBlock">
        <Setter Property="FontSize" Value="14" />
    </atom:SpinDescriptionStyle>
</Style>
```

`SpinContainerStyle`、`SpinIndicatorStyle` 与 `SpinIndicator` owner 的 `SpinIndicatorContentStyle` 使用同一规则。
Semantic Style 只覆盖视觉属性和布局属性，不改变 `IsSpinning`、SizeType 分支、`CustomIndicator` 内容或动画状态。

## 3. Template 与状态边界

- 主控件的 `container`、`mask`、`section`、`indicator` 和 `description` marker 都由 Spin 自己模板提供。
  `container` 与遮罩层组是根布局的 sibling；`mask` 与 `section` 是遮罩层组内的 sibling，`section` 是
  `indicator` 和 `description` 的唯一父容器。
- 所有 marker 节点在模板中静态存在；`IsSpinning` 只切换遮罩层组的 `IsVisible`，`IsTipVisible` 只切换
  `description` 的 `IsVisible`，都不改变 descriptor、marker 身份或数量。
- `indicator` Part 的契约边界止于公开 `SpinIndicator` 控件本身；其模板内的四个圆点
  （`Ellipse.SpinIndicatorDot`）、内置布局面板和自定义指示器 presenter 都是 `SpinIndicator` owner 的内部结构，
  不由 `Spin` owner 公开。
- `SpinIndicator` 的 `content` marker 位于静态内置点阵面板和静态自定义 presenter；不公开 `PART_*` 名称，四个圆点
  不作为独立 Part，Composition 动画对象不属于任何 Part。
- 模板重套用后由新模板重新提供同一 marker 契约；隐藏遮罩层组或切换指示器实现不改变 Part 数量。

## 4. 尺寸与布局基线

`SpinIndicator` 的 Large/Middle/Small 分支由 `SizeType` 与 `SpinToken`（`IndicatorSizeLG/IndicatorSize/IndicatorSizeSM`
和对应 `DotSize*`）控制；`SizeType=Custom` 默认回落到 Middle 尺寸，显式设置 `IndicatorSize` 或 `DotSize` 时以本地值
优先。Semantic Style 不以固定 Width/Height 替代这些完整分支；需要定制尺寸时应设置公开尺寸 API。

主控件的根默认 `HorizontalAlignment=Left`、`VerticalAlignment=Top`；嵌套模式下 `container` 按用户内容测量，
遮罩层组与 `container` 同区域叠加。`description` 默认前景色由主题从 `ColorPrimary` 投影，`section` 间距来自
`SpacingXXS`；Semantic Style 覆盖这些值时作用于同一节点，不创建平行样式入口。

Spinning 反馈在 `container` 上有两条互斥路径：`IsMaskBlurEnabled=True` 时对 `container` 应用高斯模糊，否则把
内容透明度降为 `0.5`（经 `MaskOpacity` transition）。Semantic Style 作用于 `container` 时应保留这两条状态路径，
不改写透明度状态机。

## 5. 验证不变量

- `Spin` descriptor 只包含 `root/container/mask/section/indicator/description`。
- `SpinIndicator` descriptor 只包含 `root/content`，`content` 为两个静态替代 target。
- Desktop 所有叶子 Spin 主题均声明静态 `Classes.semantic-*="True"`，不使用 Binding 或 `False` marker。
- 默认主题不消费 `.semantic-*` selector；Semantic Part 只增加静态 class、descriptor 和生成 Style。
- Gallery Semantic Parts Tab 延迟创建主控件和 `SpinIndicator` Preview；Examples Tab 的既有示例树保持不变。
- Semantic 示例标题使用 `Custom Semantic Part styling`，文案只描述 AtomUI owner-scoped Style；Tag 使用当前版本
  `GalleryVersionInfo.DisplayVersion`（`v6.1.3`）。
