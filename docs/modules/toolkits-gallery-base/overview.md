# AtomUI.Toolkits.GalleryBase 模块概览

`AtomUI.Toolkits.GalleryBase` 是面向 AtomUI 生态的 Gallery 应用底座库。它不承载任何具体产品的示例内容，而是提供搭建产品 Gallery 所需的配置、导航、路由、Shell ViewModel、展示控件、主题和本地化基础设施。

该模块可以依赖 AtomUI 作为默认 UI 实现，包括 AtomUI 控件、主题 Token、语言系统和桌面/浏览器平台能力；但它自身必须保持产品中立，不能写入 AtomUI 控件库、AtomIdea 或其他产品的品牌、页面、示例和业务文案。

## 当前平台边界

产品中立不等于当前实现平台中立。`AtomUI.Toolkits.GalleryBase.csproj` 直接引用 `AtomUI.Desktop.Controls`，其 Shell、
展示控件和主题依赖图当前面向 Desktop/Browser Gallery。Mobile Foundation 不能依赖该包，否则会把 Desktop 产品控件带入
Mobile 依赖闭包。

目标 Mobile Gallery 从独立最小 Shell 起步。只有 Mobile 与现有 Gallery 在真实实现中出现稳定、无 Desktop 语义的重复后，
才通过单独批准的设计提取共享 Gallery contracts；不能仅因包名包含 `GalleryBase` 就承诺 Mobile 复用。详见
[Mobile Gallery](../../gallery/platforms/mobile-gallery.md)。

## 职责

- 提供可配置的 Gallery Shell 基础，包括共享 Workspace ViewModel、内容路由状态、导航 ViewModel、主题切换和语言切换命令。
- 提供 Demo 展示基础控件，包括 `ShowCasePanel`、`ShowCaseItem`、瀑布流布局、Sticky 场景导航和延迟创建机制；
  Semantic Part Preview 由 GalleryBase 承担独立 Tab、真延迟创建和临时 Adorner 检查基础设施。
- 提供产品侧注册模型，让产品通过配置注册品牌、导航树、路由、页面工厂、链接和版本信息。
- 提供 Desktop 与 Browser Gallery 宿主可共用的配置、导航和路由结构，减少每个产品重复维护当前 Gallery 底层。
- 提供 GalleryBase 自身的主题 Token、ControlTheme 和 Shell 文案本地化。

## 非职责

- 不包含任何具体产品的 Demo 页面、ViewModel、示例数据或 API 表格。
- 不包含 AtomUI 官网链接、社区二维码、安装命令、产品 banner、产品 logo 等品牌资产。
- 不直接引用 `AtomUIGallery.ShowCases.*` 或任何其他产品命名空间。
- 不通过反射自动扫描产品页面作为第一阶段能力；产品侧应显式注册导航和路由，以兼容 AOT、裁剪和 Browser 体积控制。

## 关键入口

- `ThemeManagerBuilderExtensions.UseGalleryBase(...)`
- `GalleryBaseOptions`
- `GalleryBaseConfiguration`
- `GalleryRouteRegistry`
- `GalleryNavigationBuilder`
- `GalleryNavigationViewModel`
- `GalleryWorkspaceViewModel`
- `GalleryShowCaseHost`
- `SemanticPartPreview`

## 关键目录

| 目录 | 说明 |
|---|---|
| `Controls/` | Gallery 展示控件、瀑布流布局、Sticky Tabs、场景 lazy controller 和 Semantic Part Preview 基础设施 |
| `Shell/` | Gallery 共享 Workspace ViewModel、Shell 布局、Browser 宿主、OverlayLayer 和媒体断点 |
| `Navigation/` | 导航节点模型、导航构建器、NavMenu 适配 |
| `Routing/` | 路由注册、ViewModel 工厂、ReactiveUI ViewLocator 适配 |
| `Theming/` | GalleryBase Token、ControlThemesProvider、主题注册扩展 |
| `Localization/` | GalleryBase 本地化抽象，例如 `IGalleryLocalizedText` |
| `Runtime/` | 诊断和运行时选项，例如关闭 ShowCase 延迟创建 |

## 对外关系

`AtomUI.Toolkits.GalleryBase` 位于 AtomUI 上层工具库层。它依赖 AtomUI UI 基础设施，但不反向被 AtomUI 核心控件包依赖。

该模块以 `AtomUI.Toolkits.GalleryBase` NuGet 包发布，版本跟随主库 `AtomUIVersion`，并纳入 `release-nuget-packages.yml` 与 `scripts/PublishToLocalSources.ps1` 的主包发布链路。

典型消费方是具体产品 Gallery 应用：

- `AtomUIGallery`
- 未来的 AtomIdea Gallery
- 未来其他基于 AtomUI/Avalonia、且接受当前 Desktop/Browser 依赖边界的产品 Gallery

## 推荐阅读

- [architecture.md](architecture.md)
- [configuration.md](configuration.md)
- [navigation-routing.md](navigation-routing.md)
- [shell-and-platform.md](shell-and-platform.md)
- [showcase-controls.md](showcase-controls.md)
- [Semantic Part Gallery Preview](../../gallery/authoring/semantic-part-preview.md)
- [source-code-display.md](source-code-display.md)
- [theming-localization.md](theming-localization.md)
- [Gallery ShowCase 页面设计](../../gallery/authoring/gallery-showcase-design-pattern.md)
- [启动与注册链路](../../architecture/foundations/startup-and-registration.md)
