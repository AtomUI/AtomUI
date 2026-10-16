# 跨模块系统

本目录用于承载同时跨越多个源码项目、运行时层或构建层的当前系统架构，以及具有统一预实现 notice 的已批准目标系统。

系统文档必须定义稳定模型、职责 owner、数据流、生命周期、兼容性和验证不变量。单个模块的实现结构继续放在
`docs/modules/`；使用步骤和示例进入 `docs/guides/`；精确格式和协议进入 `docs/reference/`。

- [启动系统](startup/overview.md)：启动入口与生命周期、后台 Prepare、一次性提交、Splash 与主窗体交接。
- [主题系统](theming/overview.md)：主题运行时、Token、Semantic Part、定制 Guide 和 XML Reference。
- [字体子系统](typography/overview.md)：字体资源注册、字体族回退、字号阶梯派生和文本度量边界。
- [本地化系统](localization/overview.md)：运行时、Generator、Build Tasks、语言包和 XLIFF Reference。
- [Control 基础设施](control-infrastructure/overview.md)：异步加载、过滤、响应式共享契约，以及内容展开与收起的统一动效设计。
- [统一图片加载系统](image-loading/overview.md)：当前统一 Source、应用级 loader、两级缓存、内容安全与控件 API。
- [渲染系统](rendering/overview.md)：边框渲染、视觉层职责和跨树宿主选择。
- [Windowing 系统](windowing/overview.md)：窗口合成、CSD 所有权、平台能力和验证矩阵。
- [Mobile 系统](mobile/overview.md)：预实现的移动 Runtime、Viewport、Overlay、Gesture、Theme/API 映射和双平台验证契约。
