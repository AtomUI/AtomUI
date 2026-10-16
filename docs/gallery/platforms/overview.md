# Gallery 平台宿主

Gallery 平台文档区分产品内容、共享展示基础和具体 Host 的运行责任。当前 Desktop/Browser Gallery 已有源码；iOS/Android
Mobile Gallery 仍是批准的预实现架构，不能用 Desktop 或 Headless 结果替代移动平台证据。

| 平台 | 当前状态 | Host 职责 | 内容职责 |
| --- | --- | --- | --- |
| Desktop | 已有实现 | Desktop lifetime、Window、标题栏、窗口状态、平台日志和启动 | 当前 AtomUI Desktop ShowCase、API、Token 和导航 |
| Browser | 已有实现 | SingleView lifetime、Browser 字体/资源、媒体断点和浏览器启动 | 与 Desktop 共享当前 AtomUI Gallery 内容 |
| iOS | 预实现 | iOS 生命周期、启动、签名/部署和 Mobile adapter 注册 | 由目标 Mobile Gallery content 提供 |
| Android | 预实现 | Android 生命周期、启动/部署和 Mobile adapter 注册 | 由目标 Mobile Gallery content 提供 |

## 阅读入口

- [Browser Gallery 主题与宿主](browser-gallery-porting-notes.md)：当前 Browser 主题、资源和宿主约束。
- [Mobile Gallery](mobile-gallery.md)：目标 Mobile content、iOS Host、Android Host 和平台验证 ownership。
- [GalleryBase 模块](../../modules/toolkits-gallery-base/overview.md)：当前 Desktop/Browser-bound 的产品中立展示基础。
- [Gallery ShowCase 页面设计](../authoring/gallery-showcase-design-pattern.md)：当前 Gallery 页面结构与编写规则。

Desktop/Browser 的共享不代表 Mobile 自动复用相同 Shell 或基础包。任何跨产品、跨平台 Gallery contract 都必须以真实重复、
独立依赖边界和验证需求为输入另行设计。

## Desktop 启动

Desktop Gallery 在 `GalleryApplication.Configure` 中声明全部包与 `UseSplash(SplashMode.Auto, …)`（JIT 下显示 Gallery Logo 与
标题，NativeAOT 下不显示）。AtomUI 内核在 `Initialize()` 之前提交，`WorkspaceWindow` 在构造函数中直接构建导航与
`GalleryShellView`，主窗体出现时就是完整内容，没有加载占位。Splash 在主窗体首个渲染批次完成后淡出。

Gallery 的附加内置主题随 `UseGalleryControls` 进入目录。Workspace ViewModel 通过 Core 的 `ThemeCatalogChanged` 更新
AvailableThemes，窗口菜单沿用现有属性通知生成主题项；主题切换与手动目录刷新共用这条目录订阅链路。

启动生命周期见[启动系统](../../architecture/systems/startup/overview.md)；Shell 所有权见
[GalleryBase Shell](../../modules/toolkits-gallery-base/shell-and-platform.md)。
