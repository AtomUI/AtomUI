# QRCode Semantic Part 契约

本文档定义 QRCode 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。QRCode 的整体设计见
[QRCode 桌面版架构设计](overview.md)，真实模板节点与生命周期见 [QRCode 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

QRCode 公开 `root` 和 `cover` 两个 Semantic Part。二维码 bitmap、中心图标、刷新按钮和状态内容内部节点不属于公共 Part。

### 1.1 `QRCode`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `QRCode` |
| Part | `root` |
| Selector | QRCode 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `QRCode` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | QRCode owner |
| 职责 | 二维码方形根区域，承载背景、边框、圆角、Padding 和整体布局。 |
| 相关 API | `Size`、`IsBordered` 及继承的 root 表面属性 |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `cover`

| 字段 | 值 |
| --- | --- |
| Owner | `QRCode` |
| Part | `cover` |
| Selector | `.semantic-cover` |
| SelectorRoute | `/template/ .semantic-cover` |
| Style Type | `QRCodeCoverStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 状态 overlay `Border` |
| 职责 | 覆盖完整 root，承载 Loading、Expired、Scanned 状态背景与内容。 |
| 相关 API | `Status`、三组状态内容 API、`RefreshRequested` |
| 相关 Token | `QRCodeMaskBackgroundColor`、SharedToken |
| 稳定性 | stable since 6.2.0 |

## 2. Part 说明

### 2.1 `root`

`root` 是 QRCode 控件实例本身，不增加 `.semantic-root` marker。它适合定制 `Background`、`BorderBrush`、`BorderThickness`、
`CornerRadius`、`Padding`、`Margin`、对齐、透明度和整体可见性。

`Size` 同时拥有 root 外框和二维码 bitmap 的方形边长。Semantic Style 可以改变 root 表面和内容内边距，但不得通过 Width/Height
建立与 `Size` 竞争的第二套尺寸来源。边框和 Padding 都位于固定 `Size × Size` 方形内部。

### 2.2 `cover`

`cover` marker 静态放置在状态 overlay `Border`。`Active` 时节点隐藏；`Loading`、`Expired`、`Scanned` 时同一节点显示，并在内部选择
对应状态内容。可见性变化不改变 marker 数量或 target 身份。

cover 适合定制 `Background`、`CornerRadius`、`Opacity`、`Padding` 和对齐属性。应用不得通过 Semantic Style 接管 `IsVisible`，也不得
进入 Loading、Expired、Scanned 自定义内容子树继续匹配 QRCode Part。

## 3. Selector 用法

生成的 Semantic Style 封装 owner-relative `SelectorRoute`：

```xml
<Style Selector="atom|QRCode.status-emphasis">
    <atom:QRCodeCoverStyle x:SetterTargetType="Border">
        <Setter Property="Background" Value="#F0F5FF" />
        <Setter Property="CornerRadius" Value="8" />
    </atom:QRCodeCoverStyle>
</Style>
```

root 直接在 owner Style 中定制：

```xml
<Style Selector="atom|QRCode.semantic-demo">
    <Setter Property="Background" Value="#E6F7FF" />
    <Setter Property="BorderBrush" Value="#1890FF" />
    <Setter Property="BorderThickness" Value="2" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="16" />
</Style>
```

不得使用以下写法：

- `.semantic-root`、`PixelAlignedBorder#Frame` 或内部布局 Panel selector。
- `Border#Cover`、`Panel#LoadingLayout`、`Panel#ExpiredLayout`、`Panel#ScannedLayout`。
- `Border.semantic-cover`、`:is(Border).semantic-cover` 等类型前缀。
- 复制 `/template/ .semantic-cover` 完整 route 作为主要用户写法。
- 把二维码 Image、中心图标、刷新按钮或用户状态内容当作 QRCode Semantic Part。

## 4. 状态与数量语义

| 状态 | root | cover |
| --- | --- | --- |
| `Active` | 1 | 1，隐藏 |
| `Loading` | 1 | 1，可见，展示 loading 内容 |
| `Expired` | 1 | 1，可见，展示 expired 内容 |
| `Scanned` | 1 | 1，可见，展示 scanned 内容 |
| 自定义状态内容 | 1 | 1，target 不变 |
| Icon 有/无或 IconSize 更新 | 1 | 1；bitmap 重建中心透明挖空区域，Part target 不变。 |
| Value/Color/Background/EccLevel/Size 更新 | 1 | 1；Background 只更新 root，不替换 bitmap。 |
| 模板重套用 | 1 个新 root owner | 1 个新 target |

Static marker 表达稳定模板职责。状态与内容存在性通过 `IsVisible` 和 ContentPresenter 表达，不在属性变化时创建、删除或重新标记 cover。

## 5. 尺寸、布局与绘制边界

`Size` 是 root 和 bitmap 的统一方形基线。模板按以下顺序分配空间：

1. root 保持 `Size × Size`。
2. BorderThickness 占用 root 边缘。
3. Padding 缩进二维码与中心图标内容区。
4. bitmap 在剩余内容区内等比显示。
5. cover 忽略内容 Padding，覆盖完整 root 内部区域。

Semantic Style 可以改变 Border、Padding 和 cover 视觉，但不能改变二维码编码数据、纠错等级、bitmap 像素内容或 IconSize。设置过大的
Padding 会减少二维码可用绘制面积，调用方负责保证最终二维码仍可扫描。

## 6. 定制边界

以下区域明确不属于 QRCode Semantic Part：

- 私有 root 表面 `PixelAlignedBorder`、内容 Padding 容器和内部布局 Panel。
- bitmap `Image`、中心 Icon frame 及其 Image。
- Loading、Expired、Scanned 的默认或用户自定义内容子树。
- `PART_RefreshButton`、Spin、状态文本和状态图标。
- Skia QRCode 生成器、`Bitmap` internal property 和 TokenResourceBinder。

QRCode 不提供 Semantic Part Theme 替换，也不跨 VisualRoot。默认 ControlTheme 不得消费 `.semantic-cover` 作为自身样式机制。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality、移除 cover marker，或者让 cover 不再覆盖完整 root，均属于
公共主题契约变更。

验证至少覆盖：

- descriptor 中只有 `root`、`cover`，字段值与本文一致。
- `QRCodeCoverStyle` 生成并能在 AXAML 中编译。
- 默认模板只存在一个静态 `Classes.semantic-cover="True"`，不出现 `.semantic-root`。
- Active/Loading/Expired/Scanned 切换不改变 cover target 身份。
- 默认与自定义状态内容、刷新事件及 re-template 生命周期正确。
- root 的 Background、BorderBrush、BorderThickness、CornerRadius、Padding 均投影到真实表面。
- `Size` 同时约束 root 与 bitmap 方形基线，布局 Setter 不建立第二套尺寸 owner。
- Gallery Semantic Preview 只在首次选择 Semantic Parts Tab 后创建。
- Gallery Semantic Style 示例逐项保持公开上游示例的双二维码布局、数值、颜色、图标和文案。
- 默认主题不消费 `.semantic-cover`，未声明用户 Semantic Style 时不增加 VisualTree 搜索、反射或运行时 selector 组装。
