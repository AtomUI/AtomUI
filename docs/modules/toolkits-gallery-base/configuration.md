# GalleryBase 配置与注册设计

本文档细化 `AtomUI.Toolkits.GalleryBase` 的配置入口、产品模块注册方式、选项生命周期和配置校验规则。配置层是 GalleryBase 的产品中立边界：产品通过配置提供内容，GalleryBase 只消费这些配置并渲染 Shell。

## 设计目标

- 产品只通过显式配置接入 Gallery，不复制 Workspace、导航和 Browser Shell。
- 配置对象能同时驱动 Desktop 和 Browser。
- 配置校验尽早失败，避免运行时点击导航后才发现路由缺失。
- 配置模型不引用具体产品类型，除路由注册的泛型 ViewModel/View 之外不依赖产品程序集。
- 第一阶段不使用反射扫描页面，以兼容 AOT、裁剪和 Browser 体积控制。

## 入口 API

GalleryBase 的主入口是 `UseGalleryBase`：

```csharp
public static class ThemeManagerBuilderExtensions
{
    public static IAtomUIBuilder UseGalleryBase(
        this IAtomUIBuilder builder,
        Action<GalleryBaseOptions>? configure = null);
}
```

职责：

- 创建 `GalleryBaseOptions`。
- 执行产品侧 `configure`。
- 校验配置完整性。
- 注册 GalleryBase Control Token。
- 注册 GalleryBase ControlThemesProvider。
- 通过生成式模块注册把 GalleryBase Shell Catalog 和内置翻译加入 `builder.Localization`。
- 将不可变配置快照保存到 GalleryBase runtime registry，供 Shell 构造时读取。

## 配置对象

第一阶段的根配置对象：

```csharp
public sealed class GalleryBaseOptions
{
    public GalleryBrandingOptions Branding { get; }
    public GalleryNavigationBuilder Navigation { get; }
    public GalleryRouteRegistry Routes { get; }
    public GalleryShellOptions Shell { get; }
    public GalleryPlatformOptions Platform { get; }
}
```

各配置分区职责如下：

| 分区 | 职责 |
|---|---|
| `Branding` | 应用名、Logo、版本、侧边栏底部链接 |
| `Navigation` | 导航树、默认路由、默认展开路径 |
| `Routes` | route key 到 ViewModel/View 的显式映射 |
| `Shell` | Shell 布局、菜单开关、窗口尺寸 |
| `Platform` | Desktop/Browser 差异化开关 |

## Branding 配置

```csharp
public sealed class GalleryBrandingOptions
{
    public string AppName { get; set; } = "Gallery";
    public object? Logo { get; set; }
    public string? VersionText { get; set; }
    public IList<GalleryLink> Links { get; } = [];
}

public sealed record GalleryLink(
    EntityKey Key,
    string Uri,
    object? Icon = null,
    string? ToolTip = null);
```

规则：

- `AppName` 用于窗口名、Browser 页面根语义和日志上下文。
- `Logo` 支持字符串资源 URI、`IImage`、`Control` 或产品侧轻量 ViewModel。
- `VersionText` 只是显示文本，不由 GalleryBase 解析版本号。
- `Links` 渲染在侧边栏 footer；为空时 footer 链接区域不显示。
- GalleryBase 不提供任何默认产品链接。

Logo 渲染策略：

| 输入类型 | 渲染方式 |
|---|---|
| `string` | 作为 `Svg.Path` 或资源 URI 交给内部 logo presenter |
| `IImage` | 使用 `Image` 展示 |
| `Control` | 直接作为内容展示 |
| 其他对象 | 使用 `ContentPresenter` 交给产品侧 DataTemplate |

## Shell 配置

```csharp
public sealed class GalleryShellOptions
{
    public double SidebarWidth { get; set; } = 280;
    public double? CollapsedSidebarHeaderSpacing { get; set; }
    public Size InitialDesktopWindowSize { get; set; } = new(1300, 900);
    public Size MinDesktopWindowSize { get; set; } = new(1040, 720);
    public bool IsThemeMenuEnabled { get; set; } = true;
    public bool IsLanguageMenuEnabled { get; set; } = true;
    public bool IsWindowOptionsMenuEnabled { get; set; } = true;
    public bool IsFooterVisibleWhenEmpty { get; set; }
}
```

规则：

- `SidebarWidth` 必须大于 0。
- `CollapsedSidebarHeaderSpacing` 是 Sidebar 折叠后 `SidebarHeaderAction` 与第一个导航项之间的间距。
  为 `null`（默认）时跟随全局 Token `UniformlyMarginXXS`，与导航项间距一致并随主题变化；设置后使用固定值，
  必须是大于等于 0 的有限值。展开状态不受影响。
- `MinDesktopWindowSize` 不能大于 `InitialDesktopWindowSize`。
- 菜单开关只控制 Shell 菜单是否出现，不改变 AtomUI ThemeManager 本身能力。
- Browser Shell 忽略窗口尺寸，但仍使用 `SidebarWidth`。

## Platform 配置

```csharp
public sealed class GalleryPlatformOptions
{
    public bool ConfigureBrowserOverlayLayers { get; set; } = true;
    public bool EnableBrowserMediaBreakpoints { get; set; } = true;
    public bool EnableDesktopCrashLog { get; set; } = true;
    public string CrashLogDirectoryName { get; set; } = "Gallery";
}
```

`CrashLogDirectoryName` 是目录段，不允许包含路径分隔符。具体产品可以设置为 `AtomUIGallery`、`AtomIdeaGallery` 等。

## 产品模块模式

推荐产品侧定义一个模块类集中注册：

```csharp
public static class AtomUIGalleryModule
{
    public static IAtomUIBuilder UseGalleryControls(this IAtomUIBuilder builder)
    {
        GeneratedLanguageModuleRegistration.Register(builder.Localization);
        builder.UseGalleryBase(options =>
        {
            ConfigureBranding(options.Branding);
            ConfigureNavigation(options.Navigation);
            ConfigureRoutes(options.Routes);
        });
        return builder;
    }
}
```

原因：

- Desktop 和 Browser 启动项目共享同一份配置。
- 产品配置集中维护，避免在两个宿主中漂移。
- 后续接入自动化测试时，可以直接加载产品模块校验导航与路由一致性。

## 配置生命周期

配置生命周期分为四步：

1. `UseGalleryBase` 创建 mutable options。
2. 产品侧修改 options。
3. GalleryBase 执行 `Validate()`，生成 immutable `GalleryBaseConfiguration`。
4. Shell 构造时读取 configuration，不再持有 mutable options。

`GalleryBaseConfiguration` 是运行时快照：`NavigationNodes`、`DefaultOpenKeys` 和 `Routes` 都在 `BuildConfiguration()` 时复制。产品侧继续修改原始 `GalleryBaseOptions.Routes` 不会影响已经构建出来的配置；配置里的 `Routes` 处于只读状态，继续调用 `Map(...)` 会抛出 `GalleryConfigurationException`。

不可变快照示意：

```csharp
public sealed class GalleryBaseConfiguration
{
    public GalleryBrandingConfiguration Branding { get; }
    public IReadOnlyList<GalleryNavigationNode> NavigationNodes { get; }
    public IReadOnlyList<EntityKey> DefaultOpenKeys { get; }
    public EntityKey DefaultRoute { get; }
    public GalleryRouteRegistry Routes { get; }
    public GalleryShellConfiguration Shell { get; }
    public GalleryPlatformConfiguration Platform { get; }
}
```

## 配置校验

GalleryBase 必须在启动阶段校验：

| 校验项 | 失败处理 |
|---|---|
| `DefaultRoute` 为空 | 抛出 `GalleryConfigurationException` |
| `DefaultRoute` 没有对应 route | 抛出 `GalleryConfigurationException` |
| 导航页面节点没有 route | 抛出 `GalleryConfigurationException` |
| `DefaultOpenKeys` 包含空 key | 抛出 `GalleryConfigurationException` |
| `DefaultOpenKeys` 指向不存在节点 | 抛出 `GalleryConfigurationException` |
| `DefaultOpenKeys` 指向页面节点 | 抛出 `GalleryConfigurationException` |
| `DefaultOpenKeys` 重复 | 抛出 `GalleryConfigurationException` |
| route 没有导航入口 | 允许，但记录诊断信息 |
| 导航 key 重复 | 抛出 `GalleryConfigurationException` |
| route key 重复 | 抛出 `GalleryConfigurationException` |
| 配置构建后继续修改配置快照 routes | 抛出 `GalleryConfigurationException` |
| `SidebarWidth <= 0` | 抛出 `GalleryConfigurationException` |
| `CollapsedSidebarHeaderSpacing` 小于 0、NaN 或无穷 | 抛出 `GalleryConfigurationException` |
| link URI 非法 | 抛出 `GalleryConfigurationException` |

异常信息必须包含产品可定位的信息，例如 route key、导航路径、配置属性名。

## 与 AtomUI Builder 的关系

GalleryBase 使用同时暴露 `Theme` 与 `Localization` 的 `IAtomUIBuilder` 作为入口：Control descriptor、主题资产和
Theme Provider 注册到 `builder.Theme`，Catalog 与内置 Translation Bundle 注册到 `builder.Localization`。
两条注册路径互相独立，配置本身不依赖 `Application.Current`。

禁止在配置阶段做这些事：

- 创建 Window。
- 创建页面 ViewModel。
- 读取 TopLevel。
- 访问当前语言资源字符串。
- 加载所有 Demo 页面。

## 测试要求

配置层测试覆盖：

- 空配置时报明确错误。
- 默认路由不存在时报错。
- 导航页面 route 缺失时报错。
- 默认展开 key 必须存在且必须是分组节点。
- 重复 route key 和重复 navigation key 报错。
- 配置构建后 route registry 是只读快照，不受原始 options 后续修改影响。
- 空 links 时 footer 可隐藏。
- Desktop 和 Browser 读取同一份 immutable configuration。
- 配置阶段不创建产品 ViewModel 和 View。
