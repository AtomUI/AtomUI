# AtomUI 启动系统架构

启动系统负责从 `AppBuilder` 配置到"主窗体完整出现"之间的全部 AtomUI 编排：收集应用配置、在平台初始化期间于后台
准备全局 Token、可选地显示启动 Splash、一次性提交 AtomUI 内核（本地化、主题、自有服务），并在主窗体首个渲染批次
完成后交接。它横跨 `AtomUI.Core`（生命周期与提交）、`AtomUI.Controls`（Splash 控件与呈现器）和
`AtomUI.Generator`（`RegisterServices` 挂接生成器）。

## 设计目标

- 应用只需一个入口：在 `AppBuilder` 链上调用 `UseAtomUI(builder => …)`，应用类声明为 `partial`。
- AtomUI 内核在 `Application.Initialize()` 之前提交完毕：`App.axaml`、Prism 的 Shell、`OnFrameworkInitializationCompleted`
  中创建的主窗体都能直接使用 AtomUI 主题与本地化。
- 不依赖 UI 线程的工作（全局 Token 计算）与平台初始化并行。
- 主窗体出现时就是完整内容；需要更早给用户反馈的应用使用 Splash，而不是"先出空窗口再补内容"。
- 启动只有一个可观察的过程（`IAtomUIStartup`），成功、失败、取消都有确定的终态与日志。
- 无反射、无程序集扫描，兼容裁剪与 NativeAOT。

## 不负责的内容

| 内容 | 所有者 |
|---|---|
| `UseXxx` 包入口、生成的注册代码、控件描述与主题资产 | [控件与资源注册契约](../../foundations/control-registration-contracts.md)、[TypeMap 注册管线](../../foundations/aot-typemap-registration.md) |
| 主题定义、编译与运行时切换 | [主题系统](../theming/overview.md) |
| 语言目录、快照与切换 | [本地化系统](../localization/overview.md) |
| 平台选项（`WithAtomUIDefaultOptions`、`UseAtomUIPlatformDetect`） | [启动与注册链路](../../foundations/startup-and-registration.md) |

## 总体时序

```text
Main → BuildAvaloniaApp()
  AppBuilder.Configure<App>()…UseAtomUI(builder => …)                 [主线程，平台初始化前]
    1. 读取 Dispatcher.UIThread，把 UI 线程固定为主线程（先于任何后台线程）
    2. 执行配置回调，得到 builder（只有声明；字体、应用语言在提交时执行）
    3. 启动后台 Prepare（裁剪 JIT 包的 TypeMap 预热 → 全局 Token 与全局资源表）
    4. appBuilder.With(启动配置)；appBuilder.AfterApplicationSetup(守卫)
StartWithClassicDesktopLifetime → SetupUnsafe
  平台初始化 ‖ 后台 Prepare
  new App() → RegisterServices() → 生成的重写：base.RegisterServices(); AtomUIStartup.Attach(this);
    Attach：取得本实例的 builder 与 Prepare 结果 → 判定并显示 Splash → 一次性提交
  Initialize()                         App.axaml；Prism 在此创建并显示 Shell
  AfterApplicationSetup                守卫：本实例未执行 Attach 则抛出带修复说明的异常
  OnFrameworkInitializationCompleted   传统写法在此创建主窗体
主循环：判定主窗体 → 等其首个渲染批次 → Splash 淡出关闭 → Ready
```

## 模块职责

| 类型 | 程序集 | 职责 |
|---|---|---|
| `AppBuilderExtensions.UseAtomUI` | `AtomUI.Core` | 公开入口：登记配置、提前执行回调、启动后台 Prepare、注册守卫 |
| `AtomUIStartupConfiguration` | `AtomUI.Core` | 每个 `AppBuilder` 一份的配置；保存提前执行的 builder 与 Prepare 任务，供第一个应用实例取走 |
| `AtomUIStartup` | `AtomUI.Core` | 公开挂接点 `Attach(Application)`：对同一实例幂等；没有 `UseAtomUI` 配置时什么也不做 |
| `AtomUIStartupRun` | `AtomUI.Core` | 单个应用实例的启动编排：Prepare 结果、Splash 判定与显示、提交、主窗体交接、取消 |
| `ThemeStartupPreparation` | `AtomUI.Core` | 后台"仅全局"主题准备，产出各外观的全局层快照 |
| `Application.GetAtomUI()` / `IAtomUI.LoadPackages` | `AtomUI.Core` | 初始提交后在 UI 线程原子追加包；复用旧 slot、cell 和全局值，失败回滚新贡献 |
| `AtomUIStartupCommit` | `AtomUI.Core` | 一次性提交本地化、`ThemeManager`、自有服务与 initializer，失败时回滚 |
| `AtomUIStartupProcess` | `AtomUI.Core` | `IAtomUIStartup` 的实现：状态机、`Completion`、进度事件、日志 |
| `AtomUIDispatcherPump` | `AtomUI.Core` | 主循环开始前驱动 Dispatcher（Splash 首帧、错误态等待） |
| `IAtomUISplashPresenter` | `AtomUI.Core`（internal） | Core 与 Splash 实现之间的接缝；Core 不引用 `AtomUI.Controls` |
| `SplashPresenter`、`Splash`、`SplashWindow`、`SplashProgressBar` | `AtomUI.Controls` | Splash 呈现、默认主题、进度、错误态与淡出 |
| `ApplicationStartupHookGenerator` | `AtomUI.Generator` | 为 partial `Application` 生成 `RegisterServices` 挂接；手写重写漏调 `Attach` 时报 `ATOMUIGEN039` |
| `AtomUIStartupEventSource` | `AtomUI.Core` | `AtomUI-Startup` 事件源：分段、状态、Splash 与主窗体首帧 |

## 追加事务

应用的 `ApplicationScope` 同时拥有启动状态和 `IAtomUI` 运行时入口。初始 builder 释放后，追加从活动索引、资源表、
主题快照和语言 registry 获取基线；不会保留或重新执行初始配置委托。临时 builder 以已启用包和服务 ID 为基线，
拒绝初始生命周期与全局配置修改。

候选索引保留已有 Control entry、slot 与资产对象，只给新 Control 分配末尾槽位。共享清单链接器验证精确类型、
Token owner、语义属性/目标、资源键 schema；动态语言数据按全部既有支持语言完整校验。主题 resolver 不重载，
已提交全局 Token 不重新计算；根和嵌套作用域保留旧 cell，并为新控件建立惰性 cell。

资源层在同一 owner bridge 下暂存候选表，新钩子与服务失败时只卸载本次新增对象。UI 钩子读取候选主题、作用域和语言，后台只读查询
仍使用已提交视图；全部可失败操作结束后通过一个共享发布标记切换。旧 provider 的整个查找（包括内部 AddDeferred）临时使用已提交资源、主题和语言环境，避免旧缓存捕获候选值。成功事务从根 Token provider 发出一次资源通知，
子作用域只发布 context 通知。初始提交、追加重入和活跃主题操作互斥，应用销毁负责新旧贡献的统一逆序释放。

包入口、版本和调用示例见[控件包启动接入](../../../guides/startup/package-authoring.md)。

## 文档导航

| 文档 | 所有权 |
|---|---|
| [lifecycle.md](lifecycle.md) | 入口、后台 Prepare、`Attach`、提交、交接、状态机、宿主差异、出错与取消、挂接生成器与守卫 |
| [splash.md](splash.md) | Splash 类型、默认主题与自定义主题约束、显示判定、交接与淡出、已知问题 |
| [verification.md](verification.md) | 测量工具与口径、验收线、`AtomUI-Startup` 事件表、测试覆盖 |
| [启动指南](../../../guides/startup/overview.md) | 应用开发者的接入步骤与可复制示例 |
| [启动与注册链路](../../foundations/startup-and-registration.md) | 平台默认配置、`ThemeManagerBuilder` 收集内容、包注册顺序 |
