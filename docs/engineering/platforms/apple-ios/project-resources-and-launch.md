# Apple iOS 项目资源与启动

本文维护 Avalonia iOS Host 的最小结构、AppDelegate、项目属性、资源编译、AppIcon 和原生 LaunchScreen 规则。路径使用语义
占位符；实际目录以真实 Host 项目为准。

## 最小 Host 结构

一个可维护的 iOS Host 至少需要表达：

```text
<ios-host>/
├── <ios-project>.csproj
├── App.cs
├── <root-view>.cs
├── Assets/
└── Platforms/iOS/
    ├── AppDelegate.cs
    ├── Entitlements.plist
    ├── Info.plist
    └── Resources/
        ├── Assets.xcassets/
        └── LaunchScreen.storyboard
```

具体文件可以按现有应用架构调整，但 Host 必须清晰拥有 iOS 生命周期、plist、entitlements、asset catalog 和 storyboard；
Mobile Control 包不拥有这些应用资源。

## Avalonia AppDelegate

iOS 入口继承 `AvaloniaAppDelegate<App>`，创建与应用一致的 Avalonia Builder，并通过 iOS Host 启动 UIKit lifetime：

```csharp
[Register("AppDelegate")]
public class AppDelegate : AvaloniaAppDelegate<App>
{
    protected override AppBuilder CreateAppBuilder() =>
        App.BuildAvaloniaApp().UseiOS();

    internal static void Main(string[] args) =>
        UIApplication.Main(args, null, typeof(AppDelegate));
}
```

实际 Host 需要显式注册 Mobile capability adapter；AppDelegate 只负责平台组合，不承载 Control API 或跨平台状态机。

## 项目属性与资源 Item

目标框架和最低系统版本遵循仓库集中构建配置。项目至少表达 executable、single-project、应用身份、plist、entitlements、
AppIcon 和三类资源输入：

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>&lt;ios-target-framework&gt;</TargetFramework>
  <SupportedOSPlatformVersion>&lt;minimum-ios-version&gt;</SupportedOSPlatformVersion>
  <AvaloniaSingleProject>true</AvaloniaSingleProject>
  <ApplicationTitle>&lt;application-title&gt;</ApplicationTitle>
  <ApplicationId>&lt;bundle-id&gt;</ApplicationId>
  <AppIcon>AppIcon</AppIcon>
  <IncludeAllAppIcons>true</IncludeAllAppIcons>
  <AppBundleManifest>Platforms/iOS/Info.plist</AppBundleManifest>
  <CodesignEntitlements>Platforms/iOS/Entitlements.plist</CodesignEntitlements>
</PropertyGroup>

<ItemGroup>
  <AvaloniaResource Include="Assets/**/*.*" />
  <ImageAsset Include="Platforms/iOS/Resources/Assets.xcassets/**/*.*" />
  <InterfaceDefinition Include="Platforms/iOS/Resources/LaunchScreen.storyboard" />
</ItemGroup>
```

真实属性名和默认值必须以当前 .NET iOS/Avalonia 项目验证为准；不要从一次 Demo 的最低系统版本推导长期支持矩阵。

## Avalonia 资源

SVG、图片和其他 Avalonia 资产使用 assembly resource URI，不使用工作目录或文件系统相对路径：

```text
avares://<assembly-name>/Assets/<resource-file>
```

资源不显示或启动阶段失败时，先验证 assembly name、资源 Build Action、URI 大小写和 bundle 内容，再判断具体图片库或平台支持。

## AppIcon

AppIcon 放在 asset catalog 的 `AppIcon.appiconset` 中，并通过 `ImageAsset` 编译。有效产物应在最终 `.app` bundle 中包含
`Assets.car`。

稳定规则：

- AppIcon 资源名与项目 `AppIcon` 设置一致。
- 必需尺寸由 asset catalog metadata 声明，不用普通 AvaloniaResource 替代。
- AppIcon PNG 使用明确的不透明背景，避免系统图标呈现黑底或不可预测合成。
- 更换图标后同时验证源 PNG、`Assets.car` 和设备安装结果；系统主屏缓存可能保留旧图标。

源文件 alpha/尺寸可以使用 `sips` 检查，bundle 结果以实际 `.app` 中的 `Assets.car` 为准。

## LaunchScreen

iOS 启动页使用原生 storyboard，并通过 `InterfaceDefinition` 编译。`Info.plist` 声明：

```xml
<key>UILaunchStoryboardName</key>
<string>LaunchScreen</string>
```

有效产物应在 `.app` bundle 中包含 `LaunchScreen.storyboardc`。把 `.storyboard` 或 `.xib` 当普通文件复制到 bundle，不能替代
Interface Builder 编译和 plist 关联。

## LaunchScreen 与 Avalonia 坐标

UIKit LaunchScreen 和 Avalonia 正式页面都可使用 point 作为布局单位，但不是同一布局树：

- LaunchScreen 位于 UIKit storyboard 根视图坐标。
- 正式页面位置由 Avalonia `TopLevel`、root view、padding、Safe Area 和控件测量共同决定。
- 图片/SVG 的 viewBox、透明边界和可见像素边界可能不同。

需要视觉连续时使用测量流程，不把某次设备校准值写成通用约束：

1. 在目标 Simulator/真机运行正式页面。
2. 使用 `TranslatePoint`、`Bounds` 或截图像素检测记录 logo 可见中心和尺寸。
3. 把测量结果转换到同设备 UIKit point 坐标。
4. 根据页面布局关系推导 storyboard constraint，而不是反复盲调常量。
5. 重新构建并确认 `LaunchScreen.storyboardc` 已更新。
6. 使用同型号 Simulator 截图和真机体验复核切换。

页面结构、Safe Area、素材尺寸或设备族变化后必须重新测量。一次校准数据见日期化
[iOS Gallery 预览验证](../../../developments/progress/2026-08-03-apple-ios-gallery-preview-validation.md)。
