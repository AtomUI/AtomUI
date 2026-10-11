# GalleryBase 导航与路由设计

本文档细化 GalleryBase 的导航树、路由注册、ReactiveUI ViewLocator 适配和导航生命周期。该部分是产品复用的核心：产品只描述“有哪些页面”和“如何创建页面”，GalleryBase 负责生成导航模型并完成路由。

## 设计目标

- 彻底移除当前 `CaseNavigation.axaml` 和 `CaseNavigationViewModel` 中的产品硬编码。
- 导航树由产品配置生成，支持分组、页面、默认展开路径和默认页面。
- 路由注册显式、可校验、兼容 AOT 和 Browser。
- Desktop 和 Browser 使用同一套导航与路由。
- 导航节点的显示文案和路由身份分离，语言切换不影响路由。

## 导航节点模型

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

字段说明：

| 字段 | 说明 |
|---|---|
| `Key` | 稳定身份；页面节点必须等于 route key，分组节点是导航路径身份 |
| `Header` | 显示内容；可以是字符串、语言资源引用对象或轻量 ViewModel |
| `Icon` | 图标内容；默认适配 AtomUI icon provider 和 Avalonia `IconElement` |
| `IsRoute` | `true` 表示点击后导航；`false` 表示分组 |
| `Children` | 子节点；页面节点第一阶段不允许有子节点 |

## 导航构建器

产品侧使用 builder 构造导航：

```csharp
options.Navigation.DefaultRoute = "Overview";
options.Navigation.DefaultOpenKeys.Add("Components");

options.Navigation
    .AddPage("Overview", "Overview", GalleryIcon.Home)
    .AddPage("Community", "Community", GalleryIcon.Team);

options.Navigation
    .AddGroup("Components", "Components", GalleryIcon.Apps)
    .AddGroup("General", "General", GalleryIcon.Control)
    .AddPage("Button", "Button")
    .AddPage("Icon", "Icon");
```

Builder 规则：

- `AddPage(key, header, icon)` 创建 route 节点。
- `AddGroup(key, header, icon)` 创建分组节点。
- 同一棵树内 `Key` 全局唯一。
- 分组 key 可以没有 route。
- 页面 key 必须有 route。
- Header 不参与唯一性判断。

## 本地化 Header

`Header` 可以是普通字符串，也可以是 `IGalleryLocalizedText`。产品需要语言切换时应使用 `GalleryLocalizedText<TResourceKind>`，把导航文案绑定到产品自己的语言资源：

```csharp
public interface IGalleryLocalizedText
{
    object Resolve();
}
```

产品侧示例：

```csharp
options.Navigation.AddPage(
    "Button",
    new GalleryLocalizedText<CaseNavigationLangResourceKind>(
        CaseNavigationLangResourceKind.General_Button,
        "Button"));
```

页面内部文案仍由产品页面自己的 `{gallery:...LangResource}` 或绑定策略维护，不进入 GalleryBase。

## NavMenu 适配

GalleryBase 提供 NavMenu 适配器：

```csharp
public sealed class GalleryNavigationMenuAdapter
{
    public IReadOnlyList<NavMenuNode> BuildNodes(
        IEnumerable<GalleryNavigationNode> nodes);

    public IList<TreeNodePath> BuildDefaultOpenPaths(
        IEnumerable<EntityKey> defaultOpenKeys);
}
```

职责：

- 把 `GalleryNavigationNode` 转为 AtomUI `NavMenuNode`。
- 设置 `ItemKey`。
- 应用图标。
- 设置默认展开路径。
- 解析 `IGalleryLocalizedText` Header。

该适配器是 AtomUI UI 实现边界。未来如果 GalleryBase 支持其他 UI 实现，可以替换该适配层。

## 路由注册模型

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

注册后生成：

```csharp
public sealed class GalleryRouteDescriptor
{
    public EntityKey RouteKey { get; }
    public Type ViewModelType { get; }
    public Type ViewType { get; }
}
```

产品应显式提供 `viewFactory`，或者使用要求 `TView : new()` 的重载。两种路径都不依赖运行时反射。

`GalleryRouteRegistry` 在 options 阶段是可写 builder，在 `GalleryBaseConfiguration` 中是只读快照。只读快照保留 `ContainsRoute(...)`、`CreateViewModel(...)`、`RegisterViews(...)` 等运行时能力，但不允许继续 `Map(...)` 新路由。

## ViewLocator 适配

现有 Gallery 使用 ReactiveUI `DefaultViewLocator.Map<TViewModel,TView>()`。GalleryBase 应继续适配这个机制：

```csharp
public void RegisterViews(DefaultViewLocator locator)
{
    // Registry stores typed registration delegates and calls locator.Map<TViewModel, TView>(...).
}
```

产品启动项目不直接创建 `ShowCaseViewModule`，而是调用产品模块暴露的 registry：

```csharp
AppBuilder.Configure<GalleryApplication>()
    .UseReactiveUI(build =>
        build.ConfigureViewLocator(locator =>
            AtomUIGalleryModule.RegisterViews(locator)));
```

> **【已更新】** AtomUI Gallery 的 `RegisterViews` 只登记路由，不构建品牌、导航、源码展示或完整应用配置。
> 模块私有的 `Lazy` 只读列表复用固定的类型与延迟工厂；不保存应用、控件实例或本地化结果。应用仍通过
> `UseGalleryBase(AtomUIGalleryModule.Configure)` 创建自己的配置和路由快照。登记不依赖
> `GalleryBaseConfigurationProvider.Current`，因此提前解析或其他产品的当前配置不会改变 AtomUI 的路由集合。

`ConfigureViewLocator` 注册的是 locator 工厂，默认解析时会再次执行登记回调，后续导航也可能产生新的 locator。
当前 ReactiveUI 的 `Map` 每次复制已有映射；私有元数据复用保留每个传入 locator 的全部登记和覆盖语义，
不改变 locator 的生命周期或宿主自定义解析优先级。Desktop 与 Browser 使用相同的产品登记入口。

## 导航 ViewModel

`GalleryNavigationViewModel` 替代产品硬编码的导航 ViewModel。AtomUI Gallery 保留的 `CaseNavigationViewModel` 只是产品类型别名，实际导航逻辑由 GalleryBase 提供：

```csharp
public class GalleryNavigationViewModel : ReactiveObject, IActivatableViewModel, IDisposable
{
    public ReactiveCommand<EntityKey, Unit> NavigateToCommand { get; }
    public ReactiveCommand<TimeSpan, Unit> TestNavigatePagesCommand { get; }
    public ReactiveCommand<Unit, Unit> StopTestNavigatePagesCommand { get; }

    public bool CanNavigateTo(EntityKey routeKey);
}
```

职责：

- 保存当前 route key。
- 根据 route key 从 `GalleryRouteRegistry` 创建 ViewModel。
- 调用 `HostScreen.Router.NavigateAndReset`。
- 避免重复导航到当前页面。
- 提供诊断命令循环切换页面，用于内存和延迟加载压测。
- AtomUI Gallery 的 `CaseNavigation` 监听语言变化后重建 NavMenu，使 `GalleryLocalizedText<TResourceKind>` 重新解析。
- 释放时停止诊断 timer、解绑 activation subscription；如果诊断命令由当前实例启动，则恢复 ShowCase 延迟创建开关。

## 导航流程

```text
User clicks NavMenu item
  -> Product navigation view receives item key
  -> GalleryNavigationViewModel.CanNavigateTo(key)
  -> GalleryRouteRegistry.CreateViewModel(key, hostScreen)
  -> HostScreen.Router.NavigateAndReset(viewModel)
  -> ReactiveUI ViewLocator creates View
  -> RoutedViewHost displays View
```

失败处理：

| 失败点 | 处理 |
|---|---|
| 点击分组节点 | 不导航 |
| route key 不存在 | 忽略点击并记录诊断，配置校验应提前防止 |
| ViewModel factory 抛异常 | 让异常向上冒泡，测试和启动阶段暴露问题 |
| ViewLocator 未注册 View | 抛出带 ViewModel type 的错误 |

## 默认路由

`GalleryNavigationViewModel` 激活时导航到 `DefaultRoute`：

```csharp
Activator.Activated.Subscribe(_ =>
{
    NavigateTo(DefaultRoute);
});
```

规则：

- `DefaultRoute` 必须是页面节点。
- 默认路由不允许是分组节点。
- Browser 和 Desktop 默认路由一致。

## 默认展开节点

`DefaultOpenKeys` 只描述启动时默认展开的导航分组：

- 每个 key 必须存在于导航树。
- 每个 key 必须指向分组节点，不能指向页面 route。
- key 不允许重复。
- 语言切换只重建 NavMenu header 和 icon，不改变这些稳定 key。

## 测试要求

- 导航树重复 key 报错。
- 页面节点没有 route 报错。
- 默认路由不存在报错。
- 默认展开 key 不存在、重复或指向页面节点时报错。
- 点击分组节点不触发 route factory。
- 点击页面节点只创建目标 ViewModel。
- 重复点击当前页面不重复创建 ViewModel。
- 诊断自动切页会暂时关闭 ShowCase 延迟创建，并在停止后恢复。
- Dispose 导航 ViewModel 后诊断 timer 停止；只有当前实例启动的延迟创建覆盖值会被恢复。
- Browser 和 Desktop 使用同一份 `GalleryRouteRegistry`。
