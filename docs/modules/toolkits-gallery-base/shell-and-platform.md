# GalleryBase Shell 与平台宿主设计

本文档细化 GalleryBase 的共享 Shell、Desktop Window 适配、Browser View、品牌区域、标题栏菜单和平台差异。当前 GalleryBase 已承载 Workspace ViewModel、导航运行时、共享侧边栏/内容布局、Browser OverlayLayer 和媒体断点；产品侧只保留窗口菜单、字体、产品导航视图适配和启动代码。

## 设计目标

- Desktop 和 Browser 共用同一套品牌、导航、路由、Workspace ViewModel 和 Shell 布局。
- 产品侧只提供配置，不重写 Shell 布局。
- Shell 视觉使用 AtomUI 控件和 Token，但不写入任何产品品牌默认值。
- Desktop 差异和 Browser 差异封装在平台宿主边界。
- 支持未来扩展搜索、面包屑、页面元信息和响应式布局。

## Shell 组成

通用 Shell 结构：

```text
GalleryShell
  Sidebar
    BrandArea
    Navigation
    FooterLinks / Version
  ContentHost
    RoutedViewHost
  OptionalTopSeparator
  PlatformTitleBarMenu
```

`GalleryShellView` 负责 Sidebar、BrandArea、FooterLinks、VersionTag、`RoutedViewHost` 和导航/内容分隔线。Shell 不负责 Demo 页面内部布局。ShowCase 页面继续使用 `GalleryStickyTabsHost`、`ShowCasePanel` 和 `ShowCaseItem`。

### 可折叠 Sidebar

GalleryBase 通过显式 NavMenu host 契约为产品导航视图提供可选的 Sidebar 折叠能力：

```csharp
public interface IGallerySidebarNavMenuHost
{
    NavMenu SidebarNavMenu { get; }
    Control? SidebarHeaderAction { get; }
}
```

产品导航视图只有在实现该接口时才启用可折叠布局；普通 `Control` 导航视图继续使用固定
`GalleryShellConfiguration.SidebarWidth`，保持既有兼容行为。GalleryBase 不通过名称、视觉树遍历或模板 part
查找产品导航视图内部的 `NavMenu`。

折叠状态和宽度遵守以下所有权：

- `NavMenu.IsInlineCollapsed` 是唯一折叠状态源，Shell 和产品 ViewModel 不保存第二份镜像状态。
- 展开宽度由 `GalleryShellConfiguration.SidebarWidth` 写入 host 暴露的根 `NavMenu.Width`。
- 折叠宽度由 `NavMenu.InlineCollapsedWidth` 及其主题 Token 决定，GalleryBase 不重复配置默认值。
- 可折叠 Sidebar 使用 `Auto,*` 列布局，Sidebar 宽度单向跟随 `NavMenu.Width` 的有效值，使 NavMenu
  的折叠/展开 motion 同时驱动品牌区、导航/内容分隔线和内容区边界。
- `SidebarHeaderAction` 是产品导航视图提供的、尚未挂入其他视觉树的可选操作控件。Shell 只把它放入品牌区
  右侧，不拥有其命令、图标、文案或折叠状态。
- 品牌区使用左侧品牌内容与右侧 Header Action 两个独立视觉单元。展开状态下 Action 位于 Sidebar 右上角；
  折叠状态下只隐藏品牌内容，Action 在折叠宽度内保持居中和可交互。
- Sidebar 在宽度 motion 期间裁剪品牌和 footer 内容；完整品牌内容和 footer 在折叠状态下隐藏。
- NavMenu Header 不承载 Sidebar 折叠入口，避免把全局布局操作夹在品牌区与第一个导航项之间。
- `GalleryShellView.Dispose()` 必须释放从 NavMenu 到 Sidebar 的属性绑定，不能让 Shell 或导航 ViewModel
  因宽度同步而被长期保留。

折叠只改变呈现和有效交互模式，不重建导航 entry，不修改路由、`SelectedItem`、默认路径或展开路径。
Desktop 与 Browser 共用同一 `GalleryShellView`，因此 Browser 内容区的 media breakpoint 继续由折叠后的
`ContentHost` 实际宽度自然驱动。

## 共享 ViewModel

```csharp
public class GalleryWorkspaceViewModel : ReactiveObject, IScreen, IDisposable
{
    public RoutingState Router { get; }
    public GalleryNavigationViewModel Navigation { get; }

    public IReadOnlyList<ThemeInfo> AvailableThemes { get; }
    public string CurrentThemeId { get; }
    public ThemePreference AppearanceMode { get; }
    public bool IsLightAppearanceMode { get; }
    public bool IsDarkAppearanceMode { get; }
    public bool IsSystemAppearanceMode { get; }

    public ReactiveCommand<string, Unit> SwitchThemeCommand { get; }
    public ReactiveCommand<ThemePreference, Unit> SetAppearanceModeCommand { get; }
    public ReactiveCommand<bool, Unit> ToggleDarkModeCommand { get; }
    public ReactiveCommand<bool, Unit> ToggleCompactModeCommand { get; }
    public ReactiveCommand<bool, Unit> ToggleMotionCommand { get; }
    public ReactiveCommand<bool, Unit> ToggleWaveSpiritCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToZhCNCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToZhTWCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToEnUSCommand { get; }
}
```

职责：

- 持有 ReactiveUI Router。
- 持有导航 ViewModel。
- 投影 ThemeManager 已提交的主题目录、当前 Theme Id 和 Light/Dark/Follow System 状态。
- 转发主题、Appearance、紧凑、动效、Wave Spirit 和语言切换命令；所有主题维度通过一次完整 ThemeRequest 提交。
- Follow System 模式订阅平台 appearance source，平台变化时只提交完整的新主题状态，不直接修改资源。
- 监听 `ThemeChanged`、`ThemeCatalogChanged` 和语言变化并更新菜单状态。
- 释放时解绑 ThemeManager、LanguageManager 和系统 appearance 订阅，并释放 `GalleryNavigationViewModel`。

`GalleryWorkspaceViewModel` 不能知道具体产品页面类型。产品可以通过继承或组合方式提供自己的导航 ViewModel 类型别名，例如 AtomUI Gallery 的 `WorkspaceWindowViewModel` 继承 `GalleryWorkspaceViewModel`，并把 `CaseNavigation` 暴露为产品侧兼容属性。

宿主 View/Window 关闭或 Browser 根视图卸载时，如果其生命周期不是进程级单例，应调用 `Dispose()`。当前 AtomUI Gallery 的 `WorkspaceWindowViewModel` 继承该基类，因此同样获得导航诊断 timer 和语言事件的释放边界。

## Desktop 宿主适配

```csharp
public sealed class GalleryShellView : UserControl, IDisposable
```

AtomUI Gallery 的 `WorkspaceWindow` 保留产品窗口菜单和标题栏事件处理，然后在 code-behind 中创建 `GalleryShellView`：

```csharp
var shellView = new GalleryShellView(configuration, navigationView, viewModel.Router);
```

产品宿主可以在框架完备后单独调度这一步；GalleryBase 不拥有 Core 的分级启动状态机。
当前 Desktop Gallery 的加载态、Completion 等待及窗口取消策略见 [Desktop 分级启动](../../gallery/platforms/overview.md#desktop-分级启动)。
共享 Workspace/Navigation 构造函数保持即时行为，未增加隐式延迟工厂。

职责：

- 产品窗口创建并绑定产品 `GalleryWorkspaceViewModel` 派生类型。
- 产品窗口配置 AtomUI Window title bar 和菜单事件。
- `GalleryShellView` 应用 Sidebar、品牌、footer 和 routing content host。
- 产品导航视图需要 Sidebar 折叠时，实现 `IGallerySidebarNavMenuHost`，显式暴露根 `NavMenu` 和可选的
  `SidebarHeaderAction`；不能通过 Shell 穿透产品视图查找或修改内部模板。
- Shell 通过最近的 `IMediaBreakAwareControl` 感知外层媒体断点，并在 `md` 及以下自动设置
  `NavMenu.IsInlineCollapsed=True`，在 `lg` 及以上恢复展开。手动折叠/展开在当前 compact/regular 断点区间内保留；
  当断点跨区间变化时重新应用响应式默认状态。
- 产品窗口处理 caption button 可见性、移动、缩放、置顶等窗口行为。

Desktop 不负责：

- 注册产品页面。
- 创建产品导航树。
- 写死 sidebar、footer、logo、链接或 routing host。

## Browser 宿主

```csharp
public class GalleryBrowserShellView : UserControl, IScreen, IMediaBreakAwareControl, IDisposable
{
}
```

职责：

- 创建并绑定产品提供的 `GalleryWorkspaceViewModel`。
- 使用 `GalleryShellView` 渲染侧边栏和内容路由区。
- 配置 Browser 需要的 overlay layers。
- 根据自身 Shell 宽度维护外层 media breakpoint，并为 routed content 提供内容区 media owner。
- 在 detach 时释放 Shell 和 Workspace ViewModel。

AtomUI Gallery 的 `BrowserGalleryView` 继承 `GalleryBrowserShellView`，只提供字体、`WorkspaceWindowViewModel` 工厂和 `CaseNavigation` 视图工厂。

## Branding 渲染

Branding 区域由 `GalleryBrandingOptions` 驱动：

```text
BrandArea
  LogoPresenter
  Optional AppName
```

规则：

- 有 Logo 时优先展示 Logo。
- 无 Logo 但有 AppName 时展示文本。
- 字符串 Logo 当前按 SVG 资源路径渲染，并使用 Shell 默认尺寸约束。
- 产品可通过 `GalleryBrandingOptions.Logo` 提供自定义 Control。

Footer 区域：

```text
Footer
  LinkButtons
  VersionTag
```

规则：

- `Links` 为空且 `VersionText` 为空时 footer 默认隐藏。
- `Links` 不为空时渲染超链接按钮。
- `VersionText` 不为空时渲染 Tag。
- 链接图标由产品配置提供；GalleryBase 不默认显示官网、GitHub 或 Gitee。

## 标题栏菜单

标题栏菜单分三组：

| 菜单 | 配置开关 | 职责 |
|---|---|---|
| Window Options | `IsWindowOptionsMenuEnabled` | 控制 caption button、移动、缩放 |
| Theme | `IsThemeMenuEnabled` | 主题目录、Light/Dark/Follow System、紧凑、动效、WaveSpirit |
| Language | `IsLanguageMenuEnabled` | 切换应用支持的 `LanguageTag` |

AtomUIGallery Desktop 窗口根据 `AvailableThemes` 动态建立同组 Radio 项，并以 `CurrentThemeId` 设置选中态；菜单不
写死主题 Id、名称或颜色。主题选择、Appearance 三态和运行时算法组合的完整规则见
[GalleryBase 主题与本地化设计](theming-localization.md)。

语言菜单第一阶段提供 AtomUI 已支持的语言：

- `zh-CN`
- `zh-TW`
- `en-US`

如果后续把菜单改成数据驱动，应读取 `ILanguageManager.SupportedLanguages`；语言集合不属于 `ThemeManager`。

## 内容宿主

内容区使用 ReactiveUI `RoutedViewHost`：

```csharp
RoutedViewHost = new RoutedViewHost
{
    Router         = router,
    PageTransition = null,
    ClipToBounds   = true
};
```

规则：

- Shell 不设置页面 DataContext。
- 页面 ViewModel 由路由创建。
- View 由 ViewLocator 创建。
- Shell 不缓存页面；缓存策略交给 ReactiveUI 和页面内部 controller。

## 媒体断点

Browser 宿主实现 `IMediaBreakAwareControl`，用于发布外层 Shell 断点；routed content 内部由独立内容区 media owner
发布内容区域断点。

断点来源：

- Desktop 第一阶段不由 GalleryBase 强制提供，继续依赖 Window/AtomUI 现有机制。
- Browser 外层由 `GalleryBrowserShellView` 根据自身 Shell 宽度计算，供 Sidebar、品牌区和其他 Shell chrome
  消费。
- Browser routed content 内部由 GalleryBase 的内容区 media owner 根据 `GalleryShellView.ContentHost` 宽度计算，使页面控件继续按
  实际内容区域响应式布局。

规则：

- Shell chrome 不得用 `ContentHost` 宽度反推外层断点，避免 Sidebar 自动收起/展开改变内容宽度后形成反馈震荡。
- 侧边栏宽度变化会自然影响内容区断点，但不会反向改变外层 Shell 断点。
- 响应式消费仍通过 `IMediaBreakAwareControl` / `MediaBreakPointChanged` 和最近 owner 规则完成；不要在产品
  `CaseNavigation` 或单个 Showcase 中监听尺寸并补丁处理。

## OverlayLayer

Browser 宿主需要初始化 `VisualLayerManager`：

- OverlayLayer
- PopupOverlayLayer
- LightDismissOverlayLayer

这是 AtomUI popup/flyout/tour/badge 等控件在 Browser Gallery 中正常工作的必要条件。GalleryBase 封装这段逻辑，产品不再复制反射访问 `VisualLayerManager` 私有属性的代码。

## 崩溃日志

Desktop 崩溃日志当前仍由产品启动项目处理。`GalleryPlatformOptions` 已保留 `EnableDesktopCrashLog` 和 `CrashLogDirectoryName`，但 GalleryBase 尚未提供统一 crash logger helper。

规则：

- 产品可以替换为自己的日志系统。
- 如果后续新增 GalleryBase helper，不能强制包裹 `Main`，也不能吞异常。

## 测试要求

- Desktop 产品窗口不包含具体产品 logo URI、footer 链接或 routing host。
- Browser 产品视图不包含具体产品 logo URI、footer 链接、sidebar 构造或 OverlayLayer 反射代码。
- Desktop 和 Browser 都使用 `GalleryWorkspaceViewModel`。
- Workspace ViewModel 可释放并释放导航运行时。
- Footer 在 links/version 为空时隐藏。
- 标题栏菜单按配置开关显示或隐藏。
- Browser OverlayLayer 初始化方法存在于 GalleryBase Browser 宿主中。
- Browser 不再手写重复导航树。
- 可折叠 NavMenu host 的 Sidebar 宽度跟随 NavMenu 有效宽度，连续折叠/展开不产生独立 Shell 动画状态。
- 普通导航 `Control` 不实现 host 契约时继续使用固定 Sidebar 宽度。
- 折叠和展开不得重建导航 entry，且不能改变路由、选中节点或展开路径。
