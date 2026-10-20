# AtomUI.Core 模块概览

`AtomUI.Core` 是 AtomUI 的框架入口和主题基础设施项目，RootNamespace 为 `AtomUI`。它承载主题、Token、动画、MotionScene、资源加载、反射扩展和通用工具，并引用独立的 `AtomUI.Localization` 运行时。

## 职责

- 提供 `AppBuilder.WithAtomUIDefaultOptions()` 与 `AppBuilder.UseAtomUI()` 两个主要入口，以及 `ApplicationStartup.Attach`
  挂接点和启动编排（见[启动系统](../../architecture/systems/startup/overview.md)）。
- 管理唯一运行时 `ThemeManager`、五阶段主题事务、稳定 ThemeContext、局部作用域图以及独立 TopLevel 的
  context lease/resource bridge。
- 承接类型化主题包注册与默认赋值；保留动态 XML Reader/Binder，派生算法仅由现有运行时执行。
- 定义生成式全局/Control Own Token schema、所有 Control 可覆盖完整 Global Token 的规则、不可变
  `ThemeSnapshot`、稳定资源键，以及 `SharedTokenResource`/`XxxTokenResource` 强类型资源扩展。
- 定义 Semantic Part 声明、不可变 descriptor、冻结 registry 与 `IThemeManager.SemanticParts` 运行时查询入口；
  Avalonia Selector 继续负责实际样式匹配。
- 组合根 `IAtomUIBuilder` 的主题与本地化子 Builder，并把 `ILanguageManager`、`ILocalizer` 暴露给应用。
- 提供动画 Transition、MotionScene、颜色与几何工具。
- 依赖 `AtomUI.Native` 支撑窗口级底层能力。

## 关键目录

主题实现按照 [主题系统架构](../../architecture/systems/theming/runtime.md) 的职责边界组织：

| 目录 | 说明 |
|---|---|
| `Theme/` | 唯一运行时 ThemeManager、五阶段事务、ThemeContext、作用域图、TopLevel lease/bridge 及公开主题入口 |
| `Theme/Algorithms/` | 算法契约、默认/暗色/紧凑计算器、调色板和颜色计算辅助 |
| `Theme/Configuration/` | ThemeConfig、ControlThemeConfig、配置规范化、继承与覆盖合并 |
| `Theme/Definitions/` | 编译主题包 ABI/注册、主题来源、默认选择、动态 XML v1 读取、语法模型、Schema 绑定和 diagnostics |
| `Theme/Schema/` | 生成式 Token schema、Control/算法 descriptor、ControlTheme asset descriptor 与 manifest 契约 |
| `Theme/SemanticParts/` | Semantic Part Attribute、数量/定制枚举、Control descriptor 与冻结 registry |
| `Theme/Compilation/` | 纯 ThemeCompiler、不可变 ThemeSnapshot 和 Snapshot 缓存 |
| `Theme/Resources/` | Snapshot-backed ResourceProvider、全局 SharedTokenResource、生成式 XxxTokenResource、ControlTheme asset manifest、resolver 和样式加载辅助 |
| `Theme/DesignTokens/` | 仅供编译阶段使用的 DesignToken、ControlToken builder、定义和 value converter |
| `Localization/` | 根 Builder 的本地化扩展；Catalog、Snapshot、Manager 和资源扩展位于 `AtomUI.Localization` 项目 |
| `Collections/` | ItemsSource 集合适配与弱事件转发 |
| `Animations/` | Avalonia Transition 扩展；共享状态动效初始化的职责与接入边界见专项架构及验证文档 |
| `MotionScene/` | 进入、离开、移动、折叠等 Motion 抽象，以及供上层控件复用的 internal 动效执行生命周期词汇 |
| `Assets/Themes/` | 内置主题定义 |
| `Reflection/`、`Input/`、`Media/`、`Utils/` | 面向上层控件复用的内部工具 |

## 对外关系

`AtomUI.Core` 被 `AtomUI.Controls.Shared`、`AtomUI.Controls`、`AtomUI.Icons.*`、`AtomUI.Fonts.*`、
`AtomUI.Desktop.Controls` 等项目引用。各 Control 包通过源生成器为每个对外可主题化 Control 提供独立 exact CLR
type/identity、允许零 Own Token 的 descriptor、ControlTheme asset manifest 和一次包级注册入口；Core 冻结
registry 后不扫描上层程序集或 AXAML。它对多个上层项目开放 `InternalsVisibleTo`，因此修改内部 API 时需要同时
检查上层 Control 包。

Core 拥有 `AbstractControlDesignToken`、`ControlDesignTokenAttribute` 和终端 descriptor 的运行时协议，但不拥有具体
Control 家族的共享视觉语义。跨 Desktop/Mobile 的抽象 Control Token 定义属于 `AtomUI.Controls`；Generator 在编译期
扁平化这些定义，Core 运行时不遍历继承链。完整契约见
[Control Design Token 继承架构](../../architecture/systems/theming/control-design-token-inheritance.md)。

## 集合与 Motion 生命周期

`ItemCollection` 使用 Avalonia 的静态集合弱事件订阅外部 ItemsSource。适配器存活期间持续转发集合变更与 Reset，
换源时解除旧源订阅，且不要求消费控件通过清空用户数据释放引用；长寿命源不会反向保留适配器及其订阅控件。

`AbstractMotion.Run` 和 `RunAsync` 共用执行内核。每次执行持有独立的 animation/transition 快照和取消状态，同一 actor
的新运行取消旧运行；旧执行只能释放自己的 transition，不能重置新执行的 origin、transform 或 Animating 状态。
正常完成才通知 Completed；取消以 `OperationCanceledException` 传播，失败直接传播给异步调用者。无返回任务的 `Run`
观察取消和异常，并将执行失败记录到 Control 日志。Transition 等待所有完成信号，任意失败立即传播，保留无属性变化时的
有限超时兜底；成功、失败、取消均释放订阅并清理执行状态。清理中的属性观察者或 transition 释放抛错时，
仍继续其余清理并释放当前运行的所有权；多个失败以聚合异常报告，同时保留原始执行异常。

Move Motion 原位替换自身动画配置，复用实例不会累计旧动画，也不移除或重排调用者加入 `Animations` 的动画。`Follow` 的绑定由 actor 持有，`UnFollow` 成对释放绑定并退出
跟随模式，保留解绑瞬间的可见值。上述执行状态均为内部实现，不增加公开生命周期 API。

状态型过渡的共享协议由
[状态动效初始化架构设计](../../architecture/systems/control-infrastructure/state-motion-initialization.md) 定义：Core 拥有
初始化会话、可撤销过渡抑制与目标登记，具体控件拥有完整基线和几何，ControlTheme 保留过渡声明；初始状态直接
提交，完成后开放后续过渡。新控件按同一协议接入，过程反馈型动画保留独立语义。

该协议是共享架构契约，不能据此推断所有能力已进入源码。当前 `StateMotion.IsReady` 接入、公开属性覆盖原型、
实际性能证据与通用会话的验收差异统一见
[状态动效初始化验证](../../architecture/systems/control-infrastructure/state-motion-initialization-verification.md)。

## 图片加载生命周期职责

Core 增加内部 `IAtomUIOwnedService`、Builder factory 收集和 `ApplicationScope` 的 attach/回滚/逆序 dispose。该机制供
Shared 的 `ImageLoader` 使用，但 Core 不定义图片 Source、请求、缓存、HTTP、codec 或控件状态，也不反向引用
`AtomUI.Controls.Shared`。

owned service 在平台就绪后的配置回调中收集、提交前一次性冻结，按注册顺序 attach，启动失败和应用销毁按逆序清理。图片 loader 的
Application 映射由 Shared 的 `ImageLoaderStore` 管理；Core 不增加 `ApplicationImageLoader`、图片 service locator 或进程级
静态 cache。完整生命周期见
[图片加载管线、并发与生命周期](../../architecture/systems/image-loading/pipeline-and-lifecycle.md)。

## 相关文档

- [内容展开与收起动效设计](../../architecture/systems/control-infrastructure/content-expansion.md)：共享动效职责与上层控件接入契约，实现证据由各控件维护。
- [状态动效初始化架构设计](../../architecture/systems/control-infrastructure/state-motion-initialization.md)：初始化会话、可撤销抑制、目标生命周期、新控件接入与性能契约。
- [状态动效初始化验证](../../architecture/systems/control-infrastructure/state-motion-initialization-verification.md)：当前源码、专项原型与目标架构的差异，以及行为、性能和发布验收证据。
- [主题系统架构](../../architecture/systems/theming/runtime.md)
- [编译主题包](../../architecture/systems/theming/compiled-theme-packages/overview.md)：Core 拥有统一执行语义、生成 ABI 与生命周期，不依赖构建工具程序集。
- [Control Design Token 继承架构](../../architecture/systems/theming/control-design-token-inheritance.md)
- [Semantic Part 系统设计](../../architecture/systems/theming/semantic-parts.md)
- [AtomUI 本地化系统架构](../../architecture/systems/localization/overview.md)
- [主题定制指南](../../guides/theming/customization.md)
- [主题定义 XML v1 规范](../../reference/theming/theme-definition-xml-v1.md)
- [AtomUI Theme Definition XML Schema v1](../../reference/theming/schemas/atomui-theme-v1.xsd)
- [启动与注册链路](../../architecture/foundations/startup-and-registration.md)
- [运行平台策略](../../architecture/foundations/runtime-platforms.md)
- [统一图片加载系统](../../architecture/systems/image-loading/overview.md)
