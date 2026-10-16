# GalleryBase ShowCase 控件设计

本文档细化 GalleryBase 中展示控件的迁移和稳定边界。ShowCase 控件是 Gallery 页面最核心的复用资产，负责让示例内容以文档化、可滚动、可延迟创建的方式呈现。

## 设计目标

- 复用当前 Gallery 中已经验证过的 ShowCase 页面范式。
- 保留控件名称和 XAML 使用方式，降低现有 `AtomUIGallery` 迁移成本。
- 把控件与 AtomUI 产品内容分离，使其他产品可以使用同一套展示控件。
- 保持 Browser 端渐进挂载和 ShowCaseItem 延迟创建能力。
- 为复杂 Demo 页面提供统一场景切换和 lazy tab 缓存。
- 为 Semantic Part Preview 提供独立 Tab、真延迟创建和临时 Adorner 高亮职责，且不修改产品 Control。

## 控件清单

GalleryBase 展示控件清单：

| 类型 | 职责 |
|---|---|
| `ShowCaseItem` | 示例卡片，包含标题、描述、内容和延迟内容模板 |
| `ShowCasePanel` | 示例集合宿主，支持瀑布流、内部滚动开关、viewport 延迟创建 |
| `ShowCaseMasonryPanel` | 瀑布流布局面板 |
| `GalleryShowCaseHeader` | 标准 ShowCase 文档页头，统一标题、标签、简介和 metadata 信息区 |
| `GalleryStickyTabsHost` | 文档式页面宿主，Header、StickyContent、Content 同一滚动上下文 |
| `GalleryStickyTabsPanel` | StickyContent 吸顶布局实现 |
| `GalleryShowCaseScenarioController` | Icon、Palette 等特殊示例分组的 lazy 创建和缓存 |
| `GalleryReactiveUserControl<TViewModel>` | ReactiveUI View 激活辅助基类 |
| `GalleryShowCaseRuntimeOptions` | 诊断开关和延迟创建运行时控制 |

Semantic Part Preview 基础设施：

| 类型 | 职责 |
|---|---|
| `GalleryShowCaseHost` | 组合 Header、标准 Examples/Semantic Parts Tab、内容 lazy factory 和 sticky host |
| `SemanticPartPreview` | 展示真实 Control、descriptor 列表、Pin/Hover/Info 交互 |
| `SemanticPartHighlightSession` | owner-scoped 解析、Adorner 创建、Popup 临时订阅和确定性释放；internal |
| `SemanticPartAdorner` | 在目标所在 VisualRoot 中绘制无命中的高亮边框；internal |

完整架构见 [Semantic Part Gallery Preview](../../gallery/authoring/semantic-part-preview.md)。

## 命名策略

第一阶段不把 `ShowCase` 改成 `DemoCase` 或其他更中立名称。

原因：

- 当前所有 ShowCase 页面已经大量使用 `ShowCasePanel` 和 `ShowCaseItem`。
- 名称表达的是 Gallery 页面中的展示案例，不是 AtomUI 产品专有概念。
- 同时迁移程序集和重命名控件会造成快照、XAML、文档和测试大范围 churn。

如果后续需要中立别名，可以新增类型别名或派生包装，但不在第一阶段做破坏性重命名。

## ShowCaseItem

核心属性：

```csharp
public class ShowCaseItem : ContentControl
{
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsOccupyEntireRow { get; set; }
    public ShowCaseItemSpan Span { get; set; }
    public bool IsDeferredContentEnabled { get; set; }
    public IDataTemplate? DeferredContentTemplate { get; set; }
    public object? DeferredContent { get; set; }
    public double DeferredPlaceholderHeight { get; set; }
    public bool IsDeferredContentMaterialized { get; }
}
```

延迟创建规则：

- `IsDeferredContentEnabled=false` 时保持普通 `ContentControl` 行为。
- `IsDeferredContentEnabled=true` 且有 `DeferredContentTemplate` 时，内容只在 `MaterializeDeferredContent()` 后创建。
- 已创建内容不回收，避免复杂控件状态、焦点、Popup/Flyout 生命周期被破坏。
- `DeferredContent` 为空时使用 `DataContext` 构建模板。
- DataContext 变化后，已创建内容需要同步 DataContext。
- 延迟内容尚未创建时使用 AtomUI `Skeleton` 作为占位；`DeferredPlaceholderHeight` 表示预期内容高度，默认值为
  `160`，`DeferredPlaceholderCornerRadius` 控制 Skeleton 占位框圆角。当示例内容高度不同且需要避免物化时的布局跳动时，
  页面必须把 `DeferredPlaceholderHeight` 设置为实际预期高度。

XAML 使用规范：

```xml
<gallery:ShowCaseItem Title="Basic"
                      Description="Basic usage"
                      IsDeferredContentEnabled="True">
    <gallery:ShowCaseItem.DeferredContentTemplate>
        <DataTemplate x:DataType="vm:CurrentShowCaseViewModel">
            <!-- demo content -->
        </DataTemplate>
    </gallery:ShowCaseItem.DeferredContentTemplate>
</gallery:ShowCaseItem>
```

迁移后的新 ShowCase 不允许把复杂 demo 内容直接写在 `ShowCaseItem.Content`。

## ShowCasePanel

核心属性：

```csharp
public class ShowCasePanel : TemplatedControl
{
    public double MinItemWidth { get; set; }
    public int MaxColumns { get; set; }
    public double ColumnGap { get; set; }
    public double RowGap { get; set; }
    public Thickness ContentMargin { get; set; }
    public bool IsScrollEnabled { get; set; }
    public bool IsDeferredLoadingEnabled { get; set; }
    public int InitialDeferredLoadItemCount { get; set; }
    public int DeferredLoadBatchSize { get; set; }
    public double DeferredLoadViewportBuffer { get; set; }
    public Controls Children { get; }
}
```

职责：

- 把 `ShowCaseItem` 挂载到 `ShowCaseMasonryPanel`。
- 控制是否使用内部 ScrollViewer。
- 在 Browser 端渐进挂载首批 item，降低初始渲染压力。
- 使用一个 panel 级 `EffectiveViewportChanged` 监听 materialize 近 viewport item。
- 初始数量和视口批量只计算同时启用 `IsDeferredContentEnabled` 且提供 `DeferredContentTemplate` 的卡片；其他卡片不占配额，也不触发后续批次重投。

滚动规则：

| 场景 | `IsScrollEnabled` |
|---|---|
| 老式独立示例面板 | `true` |
| `GalleryStickyTabsHost` 内 Examples | `false` |
| 特殊全视口页面 | 由页面自己决定，但必须避免双垂直滚动条 |

## ShowCaseMasonryPanel

布局规则：

- 根据可用宽度、`MinItemWidth`、`MaxColumns` 计算列数。
- 第一次有效排列时，普通 item 放入当前最短列；排列完成后按 item control 实例提交列归属。
- 有效列数不变时，已经提交的普通 item 保持原列；宽度、高度和纵向位置仍按当前约束重新计算。
- 新挂载、恢复可见或不再占满整行的 item 在首次重新进入普通布局时放入当前最短列。
- 有效列数变化时清除旧列假设，并按当前最短列重新建立全部归属。
- `ShowCaseItemSpan.Full` 或 `IsOccupyEntireRow=true` 占满整行，不进入普通列归属。
- 测量与实际排列宽度不同时，按排列宽度重新测量子项，并在输入测量宽度不变的后续布局中沿用该宽度，避免分数缩放下重复失效；输入宽度变化或脱离视觉树时清除该记录。
- 不做虚拟化，只负责已挂载 children 的测量和排列。

该面板保持简单，不引入 ItemsControl、ItemsSource 或数据模板。ShowCase 页面是文档式页面，不是无限列表。

## GalleryShowCaseHeader

`GalleryShowCaseHeader` 用于替代各 ShowCase 页面在 `GalleryStickyTabsHost.Header` 中重复手写的标题、Tag、简介和 metadata 区域。它只负责页头展示，不接管 Tab、Examples、API、Design Token 或页面 code-behind 生命周期。

职责：

- 渲染控件名、分类 Tag、状态 Tag、引入版本 Tag。
- 渲染 subtitle、description。
- 渲染标准 metadata：namespace、package、base class。
- 统一 Header margin、标题字号、Tag 垂直居中、WrapPanel 换行、metadata 卡片边框和文本省略。
- 把 `Namespace`、`Package`、`Base class` 这类通用 label 收敛到 GalleryBase 语言资源。

非职责：

- 不创建或切换 `TabStrip`。
- 不管理 `ScenarioContentHost`。
- 不创建旧 API/Token sidecar。
- 不承载 ShowCase demo 内容。
- 不读取产品路由、导航、ViewModel 或反射发现页面元数据。

核心属性：

```csharp
public class GalleryShowCaseHeader : TemplatedControl
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? CategoryTagColor { get; set; }
    public string? Status { get; set; }
    public string? StatusTagColor { get; set; }
    public string? IntroducedVersion { get; set; }
    public string? IntroducedVersionTagColor { get; set; }
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? Namespace { get; set; }
    public string? Package { get; set; }
    public string? BaseClass { get; set; }
    public double MetadataLabelWidth { get; set; }
    public double MetadataValueWidth { get; set; }
}
```

默认值：

| 属性 | 默认值 | 说明 |
|---|---|---|
| `CategoryTagColor` | `blue` | 分类 Tag 默认使用蓝色 |
| `StatusTagColor` | `success` | 稳定状态默认使用成功色；Preview 页面显式覆盖为 `processing` |
| `IntroducedVersionTagColor` | `blue` | 引入版本 Tag 使用蓝色，与分类区分靠位置和文本 |
| `MetadataLabelWidth` | token 默认值 | label 的最小宽度由主题控制，长翻译可在行内自然扩展 |
| `MetadataValueWidth` | token 默认值 | value 的历史宽度提示；当前行布局中 value 优先使用剩余空间，窄宽度下不得用它撑开列 |

渲染规则：

- `Category`、`Status`、`IntroducedVersion` 为空或空白时，对应 Tag 不渲染。
- Tag 顺序固定为 category、status、introduced version。
- `IntroducedVersion` Tag 使用 Tag 默认的 `Filled` Variant，不提供独立的边框开关。
- `Subtitle` 或 `Description` 为空或空白时，对应文本行不渲染。
- `Namespace`、`Package`、`BaseClass` 为空或空白时，对应 metadata 项不渲染。
- 三个 metadata 值全部为空时，metadata 卡片不渲染。
- metadata 顺序固定为 namespace、package、base class。
- metadata 每一项占一行，label/value 使用行内两列布局；长 label 不得覆盖 value，长 value 只有在占满当前行剩余空间后才允许省略。
- metadata label/value 使用同一套 metadata 字体度量，默认跟随全局 UI 字体，避免 label 和内容因默认字体不同产生视觉垂直错位；等宽字体只用于代码展示类控件。

标准用法：

```xml
<gallery:GalleryStickyTabsHost.Header>
    <gallery:GalleryShowCaseHeader
        Title="AutoComplete"
        Category="{gallery:AutoCompleteShowCaseLangResource ComponentCategory}"
        Status="{gallery:AutoCompleteShowCaseLangResource ComponentStatusStable}"
        Subtitle="{gallery:AutoCompleteShowCaseLangResource PageSubtitle}"
        Description="{gallery:AutoCompleteShowCaseLangResource PageDescription}"
        Namespace="AtomUI.Desktop.Controls"
        Package="AtomUI.Desktop.Controls"
        BaseClass="AbstractAutoComplete" />
</gallery:GalleryStickyTabsHost.Header>
```

带引入版本和 Preview 状态的用法：

```xml
<gallery:GalleryShowCaseHeader
    Title="Splash"
    Category="{gallery:SplashShowCaseLangResource ComponentCategory}"
    Status="{gallery:SplashShowCaseLangResource ComponentStatusPreview}"
    StatusTagColor="processing"
    IntroducedVersion="{gallery:SplashShowCaseLangResource ComponentIntroducedVersion}"
    Subtitle="{gallery:SplashShowCaseLangResource PageSubtitle}"
    Description="{gallery:SplashShowCaseLangResource PageDescription}"
    Namespace="AtomUI.Controls"
    Package="AtomUI.Controls"
    BaseClass="TemplatedControl"
    MetadataValueWidth="240" />
```

迁移边界：

- 已迁移的主 ShowCase 页面只替换 Header 内部重复布局。
- 不同时抽取完整 `GalleryShowCaseDocumentPage`，因为 `TabStrip` 名称、`ScenarioContentHost`、`ExamplesContent`、lazy content 工厂和页面事件仍然由各 ShowCase 自己控制。
- Header 抽取不能改变 Examples、API、Design Token、ViewModel 或 code-behind 行为。
- 所有主 ShowCase 页面都使用 `GalleryShowCaseHeader`；`AutoComplete`、`Splash`、`BorderBeam` 保留为标准 Stable、Preview + 引入版本、无状态但有引入版本三类组合的回归样本。

## GalleryStickyTabsHost

结构：

```text
GalleryStickyTabsHost
  Header
  StickyContent
  Content
```

职责：

- 提供页面级 ScrollViewer。
- 让 Header、StickyContent、Content 共用一个滚动上下文。
- StickyContent 到达顶部后保持可见。
- 通过 sticky elevation（提升真实 StickyContent 宿主）解决窗口级 Adorner 覆盖 sticky tabs 的问题。
- `IsContentHeightBounded=true` 时宿主在 code-behind 中把 Content 宿主 MaxHeight 计算为页面视口减去 Header 与
  StickyContent 后的剩余高度（`UpdateContentMaxHeight`，随 ScrollChanged/LayoutUpdated 与属性变化更新），
  供 Semantic Parts 单个 Preview 的 Part 列表填满剩余高度并在列表内部滚动；其余情况恢复无限高度。

规则：

- 吸顶期间真实 StickyContent 宿主被提升进 AtomUI 受控 adorner 层，作为低 ZIndex 子项，低于 Drawer、Dialog、Tour 等真正浮层；原 sticky 槽位由占位元素保留布局空间。
- 不得用 VisualBrush 等像素快照实现吸顶镜像：Avalonia 12 只在 brush 创建时录制一次合成内容，选中态、hover 与窗口尺寸变化都不会刷新。
- 取消吸顶、StickyContent 置空、宿主 detach 或模板重放时，宿主必须完整归还 `GalleryStickyTabsPanel` 并清理显式尺寸、Canvas 定位与提升期间的 DataContext 覆盖。
- 不在滚动时销毁或重建 TabStrip。
- detach 时释放 ScrollViewer、StickyPanel、LayoutUpdated 和 sticky elevation 资源。

## GalleryShowCaseScenarioController

典型场景：

- Icon 的 Outlined、Filled、TwoTone 分组。
- Palette 的 Light、Dark 分组。
- 其他确实属于示例内容的互斥视图。

职责：

- 监听 `TabStrip.SelectionChanged`。
- 默认示例内容可以预先存在。
- 非默认示例分组首次切换时创建。
- 创建后缓存，避免重复构建 DataGrid。
- DataContext 变化时同步到已创建内容。
- detach 时清理 lazy content cache。

使用方式：

```csharp
_scenarioController = new GalleryShowCaseScenarioController(
    ScenarioTabs,
    ScenarioContentHost,
    CreateScenarioContent,
    ExamplesContent);
```

`CreateScenarioContent` 仍由特殊产品 ShowCase 页面提供。标准控件页面、Semantic Parts、API 和 Design Token 不使用该
controller。

## Semantic Part Preview

GalleryBase 拥有以下实现边界：

- `GalleryShowCaseHost` 组合现有 Header、sticky host、Examples 和真延迟的 Semantic Parts 内容根；内容根可以承载一个或多个
  独立 `SemanticPartPreview`，宿主统一管理其激活、停用、DataContext 和释放。
- `SemanticPartPreview` 读取冻结 descriptor，呈现真实 Control、Part 列表和用户侧代码片段。
- internal `SemanticPartHighlightSession` 负责一次有效 Hover/Pin 选择期间的 owner-scoped 解析与资源释放。
- `SemanticPartTargetResolver` 支持跨嵌套控件语义部件（`CrossNestedOwners=true`，如 AutoComplete 内嵌输入控件的
  content/placeholder）：候选解析允许穿过其他已注册语义 owner 的视觉子树，路由锚点类仍要求出现在候选祖先链上。
  popup.* 部件位于独立可视根，Preview 演示控件通过 `IsDropDownOpen` + `IsPopupPinnedOpen` 钉住弹层保持打开，
  使弹层内部件可解析、可高亮；钉住语义由产品控件负责在弹层打开前抑制 light-dismiss 遮罩（见 AutoComplete
  changelog），预览基础设施不干预弹层行为。
- internal `SemanticPartAdorner` 只负责无命中的高亮绘制，不进入产品 Control 的模板或状态模型。
- 具体产品 Gallery 只提供演示 Control、本地化职责描述和页面接入；Button 是首个完整样例，不是架构 owner。

页面模型、目标解析算法、Popup/独立宿主规则、Adorner 生命周期、性能预算和验证矩阵统一见
[Semantic Part Gallery Preview](../../gallery/authoring/semantic-part-preview.md)。本模块文档不复制这些跨 GalleryBase 与
产品 Gallery 的稳定规则。

## GalleryReactiveUserControl

职责：

- 提供 `ViewModel` StyledProperty。
- 同步 `DataContext` 与 `ViewModel`。
- 提供 `WhenActivated` 生命周期。
- 在 view loaded/unloaded 时激活或停用 `IActivatableViewModel`。

该类型可以继续作为产品 ShowCase 页面基类，减少产品侧重复 ReactiveUI 生命周期代码。

## Runtime Options

`GalleryShowCaseRuntimeOptions` 提供：

- 环境变量关闭延迟创建。
- F5/F6 自动切页压测时运行时关闭或恢复延迟创建。
- 事件通知已存在 panel 更新 materialization 状态。
- 环境变量值在进程内缓存；普通读取不再访问进程环境，只有显式设置
  `IsDeferredLoadingDisabled` 或调用 `ResetDeferredLoadingDisabledOverride()` 时才重新读取并更新有效状态。

环境变量沿用：

```text
ATOMUI_GALLERY_DISABLE_SHOWCASE_DEFERRED
```

该名称可在迁移后保留，避免破坏现有性能和诊断脚本。后续可增加新名称并兼容旧名称。

## 主题资源

ShowCase 控件主题迁入 GalleryBase：

- `ShowCaseItemTheme.axaml`
- `ShowCasePanelTheme.axaml`
- `GalleryShowCaseHeaderTheme.axaml`
- `GalleryStickyTabsHostTheme.axaml`
- `GalleryShowCaseHostTheme.axaml`
- `SemanticPartPreviewTheme.axaml`

主题必须使用 GalleryBase 的 XAML namespace：

```xml
xmlns:gallery="https://atomui.net/toolkits/gallery-base"
```

## 测试要求

- `ShowCaseItem.DeferredContentTemplate` 在 materialize 前不创建内容。
- materialize 后不重复创建内容。
- DataContext 变化同步到已 materialize 内容。
- `ShowCasePanel` 只使用一个 panel 级 viewport 监听。
- 混合可延迟与不可延迟卡片时，初始和视口批次跳过不可实例化卡片，并能继续创建后面的有效内容。
- Browser 渐进挂载不影响 deferred content 策略。
- Masonry full span item 占满整行。
- `GalleryShowCaseHeader` 可按属性组合渲染分类、状态、引入版本和 metadata。
- `GalleryShowCaseHeader` 对空值 Tag、空值文本行和空值 metadata 项执行隐藏规则。
- `GalleryShowCaseHeader` 的 Tag 行在窄宽度下换行且不挤压标题。
- `GalleryShowCaseHeader` metadata label 使用 GalleryBase 通用语言资源。
- Sticky host detach 后归还被提升的 sticky 宿主并释放资源。
- Scenario controller 首次切换创建 lazy content，后续切换复用缓存。
- Semantic Part Preview 按正式设计验证真延迟 factory、owner-scoped 解析和确定性释放。
- Semantic Part 内容根包含多个 Preview 时，factory 仍只构建一次，全部 Preview 在 Tab 切换时同步激活或停用，并在宿主 detach
  时逐一释放。
- Semantic Part Preview 使用单外框双栏和平铺分隔行；技术元数据在右栏延迟显示，代码示例在双栏下方全宽延迟显示。
- GalleryBase 测试负责宿主与 Preview 基础设施；具体产品 Gallery 测试负责 Button 等页面接入和描述数据一致性。
