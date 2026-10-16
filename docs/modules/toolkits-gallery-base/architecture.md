# AtomUI.Toolkits.GalleryBase 设计文档

`AtomUI.Toolkits.GalleryBase` 的目标是把当前 `AtomUIGallery` 中可复用的 Desktop/Browser Gallery 应用底层抽象成
产品中立的工具库。接受当前 Desktop/Browser 依赖边界的 AtomUI/Avalonia 产品应优先复用 GalleryBase，而不是复制
`AtomUIGallery` 的 Workspace、导航、Browser Shell 和 ShowCase 控件。

GalleryBase 可以依赖 AtomUI 作为 UI 具体实现。这里的中立不是 UI 技术中立，而是产品中立：库内不出现具体产品页面、品牌资产、示例注册和业务文案。

## 当前平台边界

`AtomUI.Toolkits.GalleryBase.csproj` 当前直接引用 `AtomUI.Desktop.Controls`。因此它的产品内容和品牌契约可以保持中立，
但源码依赖、Shell 控件和主题资产仍是 Desktop/Browser-bound；不能把当前包描述为 Mobile 可直接复用的跨平台 Foundation。

Mobile Gallery Foundation 不依赖 GalleryBase，而是建立独立最小 Mobile Shell。后续只有在 Mobile 与 Desktop/Browser Gallery
出现真实、稳定、无 Desktop Control 语义的重复后，才通过单独批准设计提取共享 contracts；提取前不移动现有 API，也不让
Mobile 先依赖 Desktop 再通过构建过滤补偿。目标边界见 [Mobile Gallery](../../gallery/platforms/mobile-gallery.md)。

本文档只描述总架构和边界。各部分的详细设计分布在以下专题文档：

| 文档 | 内容 |
|---|---|
| [configuration.md](configuration.md) | 配置入口、产品模块注册、选项生命周期、配置校验 |
| [navigation-routing.md](navigation-routing.md) | 导航树、路由注册、ViewLocator 适配、导航生命周期 |
| [shell-and-platform.md](shell-and-platform.md) | Desktop Window、Browser View、共享 Shell、标题栏菜单、平台差异 |
| [showcase-controls.md](showcase-controls.md) | ShowCase 控件、延迟创建、sticky host、场景 lazy controller 与 Semantic Part Preview 宿主 |
| [source-code-display.md](source-code-display.md) | ShowCase 源码展示、Drawer 查看器、源码片段 Provider、生成器边界 |
| [theming-localization.md](theming-localization.md) | XAML namespace、Control Token、主题注册、Shell 本地化边界 |

## 设计目标

- 多产品复用：同一套 Desktop/Browser Gallery Shell 能承载 AtomUI、AtomIdea 和接受当前依赖边界的后续产品。
- 显式注册：产品侧通过配置注册品牌、导航、路由和页面工厂，避免反射扫描带来的 AOT、裁剪和 Browser 体积风险。
- 平台共用：Desktop 与 Browser 宿主共享同一套 Gallery 配置和导航路由模型；Mobile 不在当前复用范围内。
- 保留 AtomUI 体验：默认 UI 使用 AtomUI 控件、Token、主题、图标和语言系统。
- 渐进迁移：先抽离可复用底层，再让现有 `AtomUIGallery` 作为第一个消费方迁入，避免一次性重写所有 ShowCase 页面。

## 当前问题

现有 `controlgallery/AtomUIGallery` 同时承担三类职责：

| 职责 | 当前状态 | 迁移目标 |
|---|---|---|
| Gallery 底层控件 | `ShowCasePanel`、`ShowCaseItem`、`GalleryShowCaseHeader`、Sticky Tabs、场景 lazy controller 已位于 GalleryBase | 保持产品中立并由产品 Gallery 复用 |
| Gallery Shell | Sidebar、footer、routing host、Browser overlay 和 media breakpoint 已位于 GalleryBase；产品侧保留标题栏菜单和导航视图适配 | 后续继续抽出可复用标题栏菜单和平台日志 helper |
| AtomUI 产品内容 | AtomUI 示例页面、首页、社区页、logo、链接、版本、语言文案 | 保留在 `AtomUIGallery` |

导航、路由和 Shell 基础已经改为由产品模块显式配置并由 GalleryBase 承载。`AtomUIGalleryModule` 作为第一个消费方注册 AtomUI 的品牌、导航树、路由工厂和 ViewLocator 映射；`CaseNavigationViewModel` 使用 GalleryBase 的 `GalleryNavigationViewModel`，`CaseNavigation` 由 `GalleryNavigationMenuAdapter` 从配置生成 `NavMenuNode` 树；Desktop 和 Browser 共用 `GalleryShellView` 渲染 sidebar、footer 和 routing host，Browser 通过 `GalleryBrowserShellView` 复用 OverlayLayer 和 media breakpoint 逻辑。

## 模块分层

GalleryBase 采用四层结构：

| 层级 | 职责 | 依赖方向 |
|---|---|---|
| 展示控件层 | ShowCase 卡片、瀑布流、Sticky Tabs、延迟创建 | 依赖 Avalonia 和 AtomUI 主题控件 |
| Shell 层 | 共享 Workspace ViewModel、Shell 布局、Browser 基础视图、侧边栏、footer、内容区域 | 依赖展示控件层、AtomUI Desktop 控件、ReactiveUI |
| 注册层 | Branding、Navigation、Routes、Links、Shell options | 不依赖具体产品页面 |
| 产品适配层 | 由消费方实现，注册具体页面、ViewModel、语言资源和资产 | 依赖 GalleryBase |

依赖方向必须保持单向：

```text
AtomUIGallery
  -> AtomUI.Toolkits.GalleryBase
      -> AtomUI.Desktop.Controls / AtomUI.Controls / AtomUI.Core
```

GalleryBase 不能引用 `controlgallery/AtomUIGallery` 或任何产品项目。

## 项目结构

目标项目结构如下：

```text
src/AtomUI.Toolkits.GalleryBase/
  AtomUI.Toolkits.GalleryBase.csproj
  Controls/
  Shell/
    GalleryWorkspaceViewModel.cs
    GalleryShellView.cs
    GalleryBrowserShellView.cs
  Navigation/
  Routing/
  Theming/
  Localization/
  Runtime/
  Properties/AssemblyInfo.cs

tests/AtomUI.Toolkits.GalleryBase.Tests/
  Controls/
  Shell/
  Navigation/
  Routing/
```

`AtomUIGallery` 迁移后的结构保留产品内容：

```text
controlgallery/AtomUIGallery/
  ShowCases/
  Assets/
  GalleryVersionInfo.cs
  AtomUIGalleryModule.cs
  AtomUIGallery.csproj
```

Desktop 和 Browser 启动项目继续保留：

```text
controlgallery/AtomUIGallery.Desktop/
controlgallery/AtomUIGallery.Browser/
```

但它们应只负责平台启动、字体和产品模块注册，不再维护完整 Gallery Shell。

## 命名约定

| 项 | 名称 |
|---|---|
| Project | `AtomUI.Toolkits.GalleryBase` |
| Package | `AtomUI.Toolkits.GalleryBase` |
| Assembly | `AtomUI.Toolkits.GalleryBase` |
| RootNamespace | `AtomUI.Toolkits.GalleryBase` |
| XAML namespace | `https://atomui.net/toolkits/gallery-base` |

XAML 推荐别名仍使用 `gallery`：

```xml
xmlns:gallery="https://atomui.net/toolkits/gallery-base"
```

这样消费方 XAML 仍然表达“Gallery 组件”，但不会和具体产品 Gallery 混淆。

## 核心配置模型

产品通过 `UseGalleryBase` 注册 Gallery：

```csharp
.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.UseGalleryBase(options =>
    {
        options.Branding.AppName = "AtomUI Gallery";
        options.Branding.Logo = "avares://AtomUIGallery/Assets/atomui-oss.svg";
        options.Branding.VersionText = GalleryVersionInfo.DisplayVersion;

        options.Navigation.DefaultRoute = "Overview";
        options.Navigation.AddPage("Overview", "Overview", GalleryIcon.Home);
        options.Navigation.AddGroup("Components", "Components", GalleryIcon.Apps)
            .AddPage("Button", "Button")
            .AddPage("DataGrid", "DataGrid");

        options.Routes.Map<OverviewViewModel, OverviewPage>(
            "Overview",
            screen => new OverviewViewModel(screen),
            () => new OverviewPage());

        options.Routes.Map<ButtonViewModel, ButtonShowCase>(
            "Button",
            screen => new ButtonViewModel(screen),
            () => new ButtonShowCase());
    });
})
```

第一阶段的配置模型如下：

```csharp
public sealed class GalleryBaseOptions
{
    public GalleryBrandingOptions Branding { get; }
    public GalleryNavigationBuilder Navigation { get; }
    public GalleryRouteRegistry Routes { get; }
    public GalleryShellOptions Shell { get; }
    public GalleryPlatformOptions Platform { get; }
}

public sealed class GalleryBrandingOptions
{
    public string AppName { get; set; }
    public object? Logo { get; set; }
    public string? VersionText { get; set; }
    public IList<GalleryLink> Links { get; }
}

public sealed class GalleryShellOptions
{
    public double SidebarWidth { get; set; }
    public Size InitialDesktopWindowSize { get; set; }
    public Size MinDesktopWindowSize { get; set; }
    public bool IsThemeMenuEnabled { get; set; }
    public bool IsLanguageMenuEnabled { get; set; }
}
```

`Logo` 使用 `object?` 是为了允许产品传入字符串 URI、`IImage`、`Control` 或轻量 ViewModel。Shell 负责把它转换为可展示内容。

## 导航模型

导航树必须由产品侧提供，GalleryBase 只负责渲染和路由。

```csharp
public sealed class GalleryNavigationNode
{
    public EntityKey Key { get; }
    public object Header { get; }
    public object? Icon { get; }
    public bool IsRoute { get; }
    public IReadOnlyList<GalleryNavigationNode> Children { get; }
}
```

规则：

- `Key` 是稳定路由身份，不使用自然语言文案。
- `Header` 可以是字符串或 `IGalleryLocalizedText`，语言切换时由 NavMenu 适配器重新解析。
- 分组节点 `IsRoute=false`，点击后只展开或收起。
- 页面节点 `IsRoute=true`，点击后通过 `GalleryRouteRegistry` 创建目标 ViewModel。
- 默认展开路径由配置提供，不能写死为 `Components`。

GalleryBase 内部提供 AtomUI `NavMenu` 适配器，把 `GalleryNavigationNode` 转成 `NavMenuNode`。

## 路由模型

路由注册必须显式：

```csharp
public sealed class GalleryRouteRegistry
{
    public void Map<TViewModel, TView>(
        EntityKey routeKey,
        Func<IScreen, TViewModel> viewModelFactory,
        Func<TView> viewFactory)
        where TViewModel : class, IRoutableViewModel
        where TView : class, IViewFor<TViewModel>;
}
```

GalleryBase 需要维护两个映射：

| 映射 | 用途 |
|---|---|
| `routeKey -> Func<IScreen, IRoutableViewModel>` | 导航点击时创建 ViewModel |
| `ViewModel type -> Func<IViewFor>` | ReactiveUI ViewLocator 创建 View |

`GalleryRouteRegistry` 应提供 `RegisterViews(DefaultViewLocator locator)` 适配方法，让 Desktop 和 Browser 启动时继续使用现有 ReactiveUI ViewLocator 流程。
配置构建完成后，configuration 中的 route registry 是只读快照；产品只能在 options 阶段继续 `Map(...)`。

## Shell 设计

GalleryBase 提供两个宿主：

| 宿主 | 用途 |
|---|---|
| `GalleryShellView` | Desktop 与 Browser 共用的侧边栏、footer 和内容路由布局 |
| `GalleryBrowserShellView` | Browser 单页面基础视图，继承 `UserControl` 并实现 `IScreen`、`IMediaBreakAwareControl` |
| `IGallerySidebarNavMenuHost` | 产品导航视图显式暴露根 `NavMenu` 和可选品牌区 Header Action，使共享 Shell 可以跟随 NavMenu 的有效折叠宽度并摆放产品操作，而不遍历产品视图内部结构 |

两者共享：

- `GalleryWorkspaceViewModel`
- `GalleryNavigationViewModel`
- `GalleryBaseOptions`
- `GalleryRouteRegistry`
- `GalleryShellView`

Desktop 与 Browser 的差异只保留在宿主边界：

- Desktop 负责窗口标题栏、caption button、窗口尺寸和崩溃日志。
- Browser 启动项目负责 `ISingleViewApplicationLifetime` 和浏览器字体策略；`GalleryBrowserShellView` 负责媒体断点和 OverlayLayer 初始化。

产品侧不应再复制侧边栏、footer、NavMenu 事件和 routing host。

## 展示控件迁移

第一阶段迁入以下控件和支持类型，并在同一展示控件层补入 ShowCase Header：

| 类型 | 迁移目标 |
|---|---|
| `ShowCaseItem` | `AtomUI.Toolkits.GalleryBase.Controls` |
| `ShowCasePanel` | `AtomUI.Toolkits.GalleryBase.Controls` |
| `ShowCaseMasonryPanel` | `AtomUI.Toolkits.GalleryBase.Controls` |
| `GalleryShowCaseHeader` | `AtomUI.Toolkits.GalleryBase.Controls` |
| `GalleryStickyTabsHost` | `AtomUI.Toolkits.GalleryBase.Controls` |
| `GalleryStickyTabsPanel` | `AtomUI.Toolkits.GalleryBase.Controls` |
| `GalleryShowCaseScenarioController` | `AtomUI.Toolkits.GalleryBase.Controls` |
| `GalleryReactiveUserControl<TViewModel>` | `AtomUI.Toolkits.GalleryBase.Controls` |
| `GalleryShowCaseRuntimeOptions` | `AtomUI.Toolkits.GalleryBase.Runtime` |

控件名称第一阶段保持不变。它们已经表达 Gallery 页面中的 ShowCase 范式，且当前大量 XAML 依赖这些名称。为追求术语中立而立即改名会制造大量无价值变更。

`GalleryShowCaseHeader` 是针对 ShowCase 页面重复 XAML 的最小公共抽象。它只统一文档页头的 title、Tag、简介和 metadata，不接管 `TabStrip`、Examples 或页面 code-behind。所有主 ShowCase 页面已经迁移到该控件；完整页面壳抽象只有在场景切换和特殊页面差异继续收敛后再评估。

## 主题和 Token

GalleryBase 维护自己的 Control Token：

- `ShowCaseItemToken`
- `ShowCasePanelToken`
- `GalleryShowCaseHeaderToken`
- `GalleryStickyTabsHostToken`
- `GalleryWindowTitleBarToken`

主题注册入口：

```csharp
public static IAtomUIBuilder UseGalleryBase(
    this IAtomUIBuilder builder,
    Action<GalleryBaseOptions>? configure = null);
```

该入口负责：

- 一次注册 GalleryBase 生成的 Control descriptor、可选 Own Token schema 和强类型 Token 资源扩展。
- 按已采纳的 TypeMap 契约注册主题导出、RequiredTokenOwners、逐控件片段与平台 Provider；[迁移状态](../../architecture/foundations/aot-and-trimming.md#1-状态与事实边界)不代表当前源码已完成转换。
- 注册 GalleryBase 生成的 Catalog、内置 XLIFF 和语言模块入口。
- 保存或合并 `GalleryBaseOptions`，供 Shell 构造时读取。

GalleryBase 不维护逐 Control/逐 Theme 注册代码、聚合 AXAML、手工 manifest 或 Token identity。没有 Own Token 的
public 可主题化 Control 仍由生成器提供独立 identity 和 descriptor。

具体产品自己的 Catalog、XLIFF 和主题仍由产品项目通过各自生成的模块入口注册。

## 本地化边界

GalleryBase 只提供 Shell 级语言资源：

- Settings
- Theme
- Language
- Window Options
- Dark Mode
- Compact Mode
- Motion
- Search 或 Navigation 通用文案

产品页面文案必须留在产品项目：

- Overview 页面标题和描述
- Community 页面文案
- Button、DataGrid 等控件 ShowCase 文案
- 控件 API 文档说明
- Design Token 文档说明

语言切换由 AtomUI `ILanguageManager` 驱动。GalleryBase 只负责响应 `LanguageChanged` 并刷新 Shell 文案；产品页面
继续使用各自 Catalog 生成的语言资源扩展和 `ILocalizer`，不再依赖 ThemeManager 的语言状态。

## Browser 与 AOT 约束

GalleryBase 必须避免以下设计：

- 默认反射扫描程序集中的 ViewModel 和 View。
- 依赖无法被裁剪器识别的私有成员构造页面。
- 在 Browser 启动时预创建所有页面。
- 把所有产品 Demo 都放入 GalleryBase 资源。

显式注册是第一阶段唯一支持的页面发现方式。后续如果需要约定式扫描，应作为可选扩展，并提供裁剪标注和 Browser 关闭开关。

## 迁移策略

迁移分为六个阶段：

1. 新建 `AtomUI.Toolkits.GalleryBase` 项目，只接入构建，不改变现有 Gallery 行为。
2. 迁移展示控件、Token、主题和 runtime options，`AtomUIGallery` 引用新库并更新 XAML namespace。
3. 引入 `GalleryBaseOptions`、导航模型和路由注册模型，但先让 `AtomUIGallery` 用配置复刻现有导航树。
4. 替换 `CaseNavigationViewModel` 中的硬编码 ViewModel factory，替换 `CaseNavigation.axaml` 中的硬编码 `NavMenuNode`。
5. 抽出 Desktop 和 Browser 共用 Shell，删除 Browser 中重复手写的 sidebar/footer/routing host。
6. 拆分测试：GalleryBase 控件和 Shell 测试迁入 `AtomUI.Toolkits.GalleryBase.Tests`，AtomUI 产品页面快照测试保留在 `AtomUIGallery.Tests`。

每个阶段必须保持 `AtomUIGallery` 可构建、可运行，并通过已有 Gallery 测试。

当前实现状态：

- 阶段 1 和阶段 2 已完成。
- 阶段 3 和阶段 4 已完成：`GalleryBaseOptions`、`GalleryRouteRegistry`、`GalleryNavigationBuilder`、`GalleryNavigationViewModel`、`GalleryNavigationMenuAdapter` 和 `GalleryLocalizedText<T>` 已落地，`AtomUIGalleryModule` 已成为 AtomUI Gallery 的产品注册入口。
- 阶段 5 已完成共享 Shell 基础抽取：`GalleryWorkspaceViewModel` 提供 Router、主题命令、语言命令和导航 ViewModel；`GalleryShellView` 提供 sidebar、footer 和 routing host；`GalleryBrowserShellView` 提供 Browser overlay 和 media breakpoint。产品侧仍保留窗口菜单和导航视图适配。

## 测试策略

GalleryBase 测试覆盖：

- `ShowCasePanel` 瀑布流布局和延迟创建行为。
- `ShowCaseItem` deferred content materialization。
- `GalleryStickyTabsHost` sticky 和 sticky mirror 行为。
- `GalleryRouteRegistry` 的 route key、ViewModel factory 和 ViewLocator 注册。
- `GalleryNavigationBuilder` 的分组、页面、默认路由和重复 key 校验。
- `GalleryWorkspaceViewModel` 的导航、主题、语言命令。
- Browser 和 Desktop Shell 不包含产品品牌硬编码。

AtomUI Gallery 测试继续覆盖：

- AtomUI 具体导航树。
- AtomUI 首页和社区页。
- 每个 ShowCase 的结构、快照和懒加载 tab。
- AtomUI logo、链接、版本显示等品牌配置。

## 验收标准

- GalleryBase 项目中没有 `AtomUIGallery.ShowCases`、`OverviewViewModel`、`CommunityPage`、`ButtonViewModel` 等产品类型引用。
- 新产品 Gallery 可以只通过 `GalleryBaseOptions` 注册品牌、导航和路由，不复制 Workspace 和 Browser Shell。
- Desktop 与 Browser 使用同一份导航和路由配置。
- `AtomUIGallery` 的视觉和行为保持一致。
- `dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --nologo /nr:false` 通过。
- `dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj --nologo -v:minimal /nr:false` 通过。

## 后续扩展

第一阶段不实现以下能力，但设计上保留空间：

- 导航搜索和过滤。
- 页面元数据生成侧边导航。
- 可选的程序集扫描注册。
- 多 Gallery 主题皮肤。
- Demo 页面模板生成器。
