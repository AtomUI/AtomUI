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

## Desktop 分级启动

2026-10-12 更新：Desktop Gallery 显式设置 Application.Name，并在首阶段注册 Desktop 的完整 chrome 闭包和
GalleryBase 标题栏。加载态保留窗口尺寸、原生按钮和窗内菜单；Workspace ViewModel 的菜单命令与状态保持原构造行为。
非 chrome 包和 Gallery 语言模块通过 Deferred 注册。

Gallery 的附加内置主题随 `UseGalleryControls` 在第二阶段进入目录。Workspace ViewModel 通过 Core 的
`ThemeCatalogChanged` 更新 AvailableThemes，窗口菜单沿用现有属性通知生成主题项；主题切换、手动目录刷新和
两阶段演化共用这条目录订阅链路。

WorkspaceWindow 构造加载态后读取 `Application.GetStartup().Completion`，使用窗口自己的 CancellationToken
等待框架就绪。成功后把 `BuildDeferredContent` 排入 Background，构建导航和 `GalleryShellView`；失败在加载态
显示异常信息。启动已经完成后才创建的窗口也能立即读取同一结果；同步宿主使用相同流程。

窗口关闭（含尚未 Show 的窗口）或卸载时取消自身等待及未执行的构建操作，并释放 Shell/ViewModel 和菜单订阅。
窗口的取消不影响应用启动或其他观察者。Shell 已存在时不重复构建。

启动状态机、线程和宿主就绪证据归 [Core 分级演化](../../architecture/foundations/staged-startup-evolution.md)；
Shell 所有权见 [GalleryBase Shell](../../modules/toolkits-gallery-base/shell-and-platform.md)。
首帧、框架完成和 Shell 绘制是三个独立里程碑，不用完成任务推断实际呈现。
