# AtomUI 文档

AtomUI 文档按知识责任组织。跨模块系统设计、单个源码模块、具体 Control、工程规范、Gallery 和过程记录分别维护，
同一稳定契约只由一份正式文档拥有。

## 阅读入口

- [整体架构](architecture/overview.md)：系统分层、源码包边界和核心运行链路。
- [架构基础](architecture/foundations/overview.md)：项目依赖、运行平台、启动注册、构建和打包。
- [跨模块系统](architecture/systems/overview.md)：主题、字体、本地化、统一图片加载、Control 基础设施和渲染系统的统一入口。
- [源码模块](modules/overview.md)：按源码项目或发布包理解职责和实现边界。
- [Control 文档](controls/overview.md)：按平台和类别查看具体 Control 的设计、实现、Token 和变更。
- [使用指南](guides/overview.md)：按任务查看主题定制与后续接入指南。
- [Reference](reference/overview.md)：版本化格式、协议和稳定公共契约。
- [工程规范](engineering/overview.md)：开发、AOT、诊断、文档、测试和发布规则。
- [Gallery 文档](gallery/overview.md)：Gallery 组织、ShowCase 和平台维护说明。
- [Releases](releases/overview.md)：按版本查看 API 变化与迁移示例。
- [Strategy](strategy/overview.md)：产品与产业战略分析。
- [Superpowers](superpowers/)：设计过程、实施计划和阶段进度，不作为正式架构的事实来源。

## 文档职责

```text
docs/
|-- overview.md
|-- architecture/   # 跨模块当前架构与带统一状态声明的预实现架构
|-- modules/        # 单个源码项目或发布包
|-- controls/       # 单个 Control 或紧密 Control 家族
|-- guides/         # 面向任务的使用和接入指南
|-- reference/      # 版本化格式、协议和公共契约
|-- engineering/    # 开发规范、工作流和检查清单
|-- gallery/        # AtomUIGallery 工程文档
|-- releases/       # 版本 API 变化
|-- strategy/       # 产品与产业战略分析
|-- AI/             # AI 工具消费层和配置驱动的生成文档
`-- superpowers/    # 设计与实施过程记录
```

`modules/` 与 `controls/` 的边界必须保持清楚：模块文档解释包如何组织、注册和加载；Control 文档解释具体控件的
公共契约、行为、模板、Token 和维护不变量。

## 当前专题

- [Control 基础设施架构](architecture/systems/control-infrastructure/overview.md)
- [统一图片加载架构](architecture/systems/image-loading/overview.md)
- [Control 基础设施使用指南](guides/control-infrastructure/overview.md)
- [字体子系统架构](architecture/systems/typography/overview.md)
- [渲染架构](architecture/systems/rendering/overview.md)
- [Popup Anchor 作用域检查](engineering/development/popup-anchor-scope.md)
- [Native 模块](modules/native/overview.md)
- [Windowing 系统](architecture/systems/windowing/overview.md)
- [AOT 与裁剪架构](architecture/foundations/aot-and-trimming.md)
- [TypeMap 注册管线](architecture/foundations/aot-typemap-registration.md)
- [控件与资源注册契约](architecture/foundations/control-registration-contracts.md)
- [Browser TypeMap 链接](architecture/foundations/aot-browser-linking.md)
- [AOT 编程规范](engineering/development/aot-programming-guidelines.md)
- [Control 开发规范](engineering/development/control-development-guidelines.md)
- [Control 文档规范](engineering/contributing/control-documentation-guidelines.md)

## AtomUI Mobile

AtomUI Mobile 位于独立仓库 [AtomUI/AtomUI.Mobile](https://github.com/AtomUI/AtomUI.Mobile)，以 NuGet 方式消费本仓库的包。下列 Mobile 入口均指向该仓库的正式文档；iOS 与
Android 工程环境文档仍由本仓库维护。

- [Mobile 系统架构](architecture/systems/mobile/overview.md)：指向 AtomUI.Mobile 的 Runtime、Overlay、Gesture 与验证契约。
- [Mobile Controls 模块](modules/mobile-controls/overview.md)：指向 AtomUI.Mobile 的包、上游集成与打包说明。
- [Mobile Control 能力清单](controls/mobile/overview.md)：指向 AtomUI.Mobile 的能力清单与状态模型。
- [Apple iOS 工程环境](engineering/platforms/apple-ios/overview.md)：稳定工具链、签名、资源、Simulator/真机和验证规则。
- [Android 工程环境](engineering/platforms/android/overview.md)：Host、命令、版本和平台证据尚未验证的责任模型。
- [Mobile Gallery](gallery/platforms/mobile-gallery.md)：指向 AtomUI.Mobile 的 Gallery 与宿主边界。

随着专题完成职责迁移，本入口同步切换到新的 Architecture、Guide、Reference 或 Module 路径，不保留重复副本。
