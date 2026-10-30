# AtomUI.Core 模块概览

`AtomUI.Core` 是 AtomUI 的框架入口和主题基础设施项目，RootNamespace 为 `AtomUI`。它承载主题、Token、动画、Motion、资源加载、反射扩展和通用工具，并引用独立的 `AtomUI.Localization` 运行时。

## 职责

- 提供 `AppBuilder.WithAtomUIDefaultOptions()` 与 `AppBuilder.UseAtomUI()` 两个主要入口，以及 `ApplicationStartup.Attach`
  挂接点和启动编排（见[启动系统](../../architecture/systems/startup/overview.md)）。
  `WithAtomUIDefaultOptions()` 在 Windows 上固定合成回退顺序为 `RedirectionSurface` → `WinUIComposition` →
  `DirectComposition` → `LowLatencyDxgiSwapChain`，以消除 live resize 对向边缘错帧（见
  [Windows Live Resize 与窗口装饰架构](../../architecture/systems/windowing/windows-live-resize.md)）。
- 管理唯一运行时 `ThemeManager`、五阶段主题事务、稳定 ThemeContext、局部作用域图以及独立 TopLevel 的
  context lease/resource bridge。
- 承接类型化主题包注册与默认赋值；保留动态 XML Reader/Binder，派生算法仅由现有运行时执行。
- 定义生成式全局/Control Own Token schema、所有 Control 可覆盖完整 Global Token 的规则、不可变
  `ThemeSnapshot`、稳定资源键，以及 `SharedTokenResource`/`XxxTokenResource` 强类型资源扩展。
- 定义 Semantic Part 声明、不可变 descriptor、冻结 registry 与 `IThemeManager.SemanticParts` 运行时查询入口；
  Avalonia Selector 继续负责实际样式匹配。
- 组合根 `IAtomUIBuilder` 的主题与本地化子 Builder，并把 `ILanguageManager`、`ILocalizer` 暴露给应用。
- 提供动画 Transition、Motion 定义与播放、颜色与几何工具。
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
| `Animations/` | [动画基础设施](animations.md)：框架复用、Transition 初始化、有限 Motion、持续反馈与最小适配 |
| `Animations/Motion/` | Motion 定义、类型化轨迹构建、组合、原生播放、属性覆盖、可见性与公开布局能力 |
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

### Motion 定义与播放

[Motion 系统架构](../../architecture/systems/motion/overview.md)定义共享合同。公开命名空间为 `AtomUI.Animations`，
AXAML 使用 `atom:`。具体动效继承 AbstractMotion 并实现 Build，PlayAsync 对真实 Control 执行，每次准备独立快照。
Motion 是附加属性与目标级操作入口；定义基类更名不改变 `atom:Motion.Enter`、`atom:Motion.IsVisible` 等 AXAML 写法。
Sequence / Parallel 共用一个目标请求身份；相同目标的新成功请求连续替代当前执行，过期请求不能提交新状态。
取消、替代、detach 和未就绪通过 MotionResult 区分，异常通过任务传播。

原生执行以每次独立的非视觉进度源接入框架时钟；属性使用可释放覆盖，完成终态与活动运行分离。
Reset、下一次成功请求或 detach 释放保留覆盖并恢复最新底层值，不写死或清除应用绑定。
可见性入口负责首帧准备、隐藏与挂载生命周期，物理宿主和业务事件仍属于具体控件。

Core 公开 IMotionLayout，Controls 的 ContentViewport 实现自然内容、真实占位与视口；普通视觉目标不需要专用基类。
基础动效不依赖 Desktop 初始化或全局 animator 注册，Core 不反向引用上层包。旧执行体系已移除。
合成后端、NativeAOT 与原生呈现证据的实际状态统一见 [Motion 验证状态](../../architecture/systems/motion/verification.md)。

### 状态动效初始化

状态型过渡的共享协议由
[状态动效初始化架构设计](../../architecture/systems/control-infrastructure/transition-initialization.md) 定义：Core 拥有
初始化会话、可撤销过渡抑制与目标登记，具体控件拥有完整基线和几何，ControlTheme 保留过渡声明；初始状态直接
提交，完成后开放后续过渡。新控件按同一协议接入，过程反馈型动画保留独立语义。

该协议是共享架构契约，不能据此推断所有能力已进入源码。当前 `TransitionInitializationSession.IsReady` 接入、公开属性覆盖原型、
实际性能证据与通用会话的验收差异统一见
[状态动效初始化验证](../../architecture/systems/control-infrastructure/transition-initialization-verification.md)。

## 图片加载生命周期职责

Core 增加内部 `IAtomUIOwnedService`、Builder factory 收集和 `ApplicationScope` 的 attach/回滚/逆序 dispose。该机制供
Shared 的 `ImageLoader` 使用，但 Core 不定义图片 Source、请求、缓存、HTTP、codec 或控件状态，也不反向引用
`AtomUI.Controls.Shared`。

owned service 在平台就绪后的配置回调中收集、提交前一次性冻结，按注册顺序 attach，启动失败和应用销毁按逆序清理。图片 loader 的
Application 映射由 Shared 的 `ImageLoaderStore` 管理；Core 不增加 `ApplicationImageLoader`、图片 service locator 或进程级
静态 cache。完整生命周期见
[图片加载管线、并发与生命周期](../../architecture/systems/image-loading/pipeline-and-lifecycle.md)。

## 应用资源宿主职责

Core 拥有主题与语言资源的应用级挂载编排。ApplicationScope 管理语言 Provider 与 FlowDirection Style，
ThemeManager 管理自己的主题资源字典与根 Context Style；Localization 模块保留语言服务状态的所有权。
共同宿主、覆盖优先级、部分挂载回滚与释放边界由
[应用资源宿主设计](../../architecture/systems/theming/application-resource-host.md)定义。

当前分支已采用共同宿主；实现证据与未覆盖的验收范围见
[资源宿主验证](../../architecture/systems/theming/application-resource-host-verification.md)。

## 相关文档

- [Motion 动效系统架构](../../architecture/systems/motion/overview.md)：定义与扩展、播放/终态、无 Actor 目标、布局、性能及验证边界。
- [内容展开与收起动效设计](../../architecture/systems/control-infrastructure/content-expansion.md)：共享动效职责与上层控件接入契约，实现证据由各控件维护。
- [状态动效初始化架构设计](../../architecture/systems/control-infrastructure/transition-initialization.md)：初始化会话、可撤销抑制、目标生命周期、新控件接入与性能契约。
- [状态动效初始化验证](../../architecture/systems/control-infrastructure/transition-initialization-verification.md)：当前源码、专项原型与目标架构的差异，以及行为、性能和发布验收证据。
- [主题系统架构](../../architecture/systems/theming/runtime.md)
- [应用资源宿主设计](../../architecture/systems/theming/application-resource-host.md)
- [应用资源宿主验证](../../architecture/systems/theming/application-resource-host-verification.md)
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
