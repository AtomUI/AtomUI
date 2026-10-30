# 启动重构步骤 1：启动生命周期与 Splash 设计

日期：2026-10-14。状态：五节设计已与用户逐节确认，等待审阅本文件。
上层设计：[启动架构重构设计](2026-10-14-startup-architecture-design.md)（下称"总 spec"）。
前置结论：[步骤 0 结果](../progress/2026-10-14-startup-step0-results.md)。
问题清单：[研发日志 §7](../progress/2026-10-12-startup-optimization-rd-log.md#7-2026-10-14-全系列只读审查发现)。

## 0. 摘要

步骤 1 用新的启动生命周期替换分级启动：配置回调在 `AppBuilder` 阶段执行，全局 Token 在平台初始化期间于后台
计算；`RegisterServices` 中先显示只依赖全局 Token 的 Splash，再一次性完成内核提交；应用随后构建主窗体，
主窗体首个渲染批次完成后 Splash 淡出。分级启动（Deferred、BootstrapRegistration、Stage 2 演化、宿主就绪探测）
整体删除；Splash 从 Extras 迁入 `AtomUI.Controls` 并改为轻量实现。包注册机制（`UseXxx` 与生成的注册代码）
保持现状，包清单与启动镜像属于步骤 2。

## 1. 范围

**包含**：启动入口与生命周期；后台 Prepare；`RegisterServices` 挂接生成器与误配守卫；Splash（迁移、轻量化、
默认主题、自定义主题、交接）；`UseMainWindow`；`IAtomUIStartup` 新状态；删除分级启动；调用点迁移（测试宿主、
注册夹具、工具、Gallery）；启动 EventSource；文档迁移。

**不包含**：包入口契约与钩子、包清单、启动镜像、控件描述按需物化、运行时追加（步骤 2）；本地化数据化（步骤 3）；
主题定义两层绑定与版本耦合（步骤 4）；MultiCoreJit / ReadyToRun 策略（步骤 5）；主窗体 chrome 成本（步骤 6）；
浏览器加载层、`UseMainView`、移动端 Splash（后续步骤）。

## 2. 已确认的决策（含与总 spec 的差异）

| 编号 | 决策 | 与总 spec 的关系 |
|---|---|---|
| D1 | 时序为"全局 Token → Splash → 完整提交"。提交期间 UI 线程被占用，Splash 依靠渲染线程上的合成层动画保持活动（步骤 0 S2 已证）。 | 修改总 spec §5（原为先提交、后 Splash）。 |
| D2 | 配置回调在 `AppBuilder` 阶段、主线程、平台初始化之前执行；字体注册与应用级语言注册推迟到提交；Splash 只用系统默认字体。 | 细化总 spec §5、§7.1。 |
| D3 | 配置回调对每个 `Application` 实例执行一次。第一个实例在 `UseAtomUI` 中提前执行并启动后台 Prepare；同一 `AppBuilder` 上之后的实例（headless 按测试隔离）在 `Attach` 中同步重跑。 | 新增。 |
| D4 | 误配守卫改为 `AfterApplicationSetup` 检查：`Attach` 未执行即抛出带修复说明的异常。不做逐控件守卫。 | 修改总 spec §5"运行时守卫"。 |
| D5 | Splash 只依赖全局 Token；删除 `SplashToken` 与 Semantic Part；文字、Logo、状态图标直接用 Avalonia 的 `TextBlock` / `Image` / `Path`，只新增 `SplashProgressBar`。 | 细化总 spec §10。 |
| D6 | 主窗体按确定性规则判定（§3.5），不以"第一个打开的窗口"兜底。 | 细化总 spec §10。 |
| D7 | 验收线 2 仍是步骤 1 的退出条件。 | 与总 spec §16 一致。 |
| D8 | `UseMainView` 推迟到浏览器 / 移动端步骤。 | 修改总 spec §11。 |
| D9 | 不调用 `UseSplash` 即不显示 Splash；调用时模式默认 `Auto`。 | 明确总 spec §10、§11。 |

## 3. 启动时序

```
Main → BuildAvaloniaApp()
  AppBuilder.Configure<App>()…UseAtomUI(builder => …)                 [主线程，平台初始化前]
    1. 读取 Dispatcher.UIThread，把 UI 线程固定为主线程（先于任何后台线程）
    2. 执行配置回调，得到 builder（只有声明；字体、应用语言在提交时执行）
    3. 启动后台 Prepare（纯数据）
    4. appBuilder.With(startup)；appBuilder.AfterApplicationSetup(守卫)
StartWithClassicDesktopLifetime → SetupUnsafe
  平台初始化 ‖ 后台 Prepare
  new App() → RegisterServices() → 生成的重写：base.RegisterServices(); AtomUIStartup.Attach(this);
    Attach：取得本实例的 builder 与 Prepare 结果 → 判定并显示 Splash → 提交
  Initialize()                         App.axaml；Prism 在此创建并 Show Shell
  AfterApplicationSetup                守卫：Attach 未执行则抛出
  OnFrameworkInitializationCompleted   传统写法在此创建主窗体
主循环：判定主窗体 → 等其首个渲染批次 → Splash 淡出关闭 → Ready
```

### 3.1 `UseAtomUI`（AppBuilder 阶段）

- 签名：`public static AppBuilder UseAtomUI(this AppBuilder appBuilder, Action<IAtomUIBuilder> configure)`，
  位于 `AtomUI.Core`，命名空间 `AtomUI`。同一 `AppBuilder` 只能调用一次。
- 应用类型取 `appBuilder.ApplicationType`（`Configure<TApp>()` 与 `Configure<TApp>(Func<TApp>)` 均会设置），
  用于推导默认应用标识。回调照常提前执行；回调返回后，若应用类型为 null 且回调中没有调用 `WithApplicationId`，
  则不启动后台 Prepare，改在 `Attach` 中用实例类型补齐应用标识后同步执行（默认应用标识决定用户主题目录，
  不能用占位值做 Prepare）。
- 设计模式（`Design.IsDesignMode`，预览器在调用 `BuildAvaloniaApp` 之前设置）或单线程运行时
  （`OperatingSystem.IsBrowser()` / `OperatingSystem.IsWasi()`）不提前执行，全部在 `Attach` 中同步完成。
- 其余情况：固定 UI 线程 → 执行回调 → 启动后台 Prepare。回调抛出的异常原样同步抛出。
- 启动对象通过 `appBuilder.With(startup)` 交给 `Attach`（`SetupUnsafe` 开头写入 `AvaloniaLocator`；
  headless 每个测试都会重新写入同一个对象）。

### 3.2 后台 Prepare

- **输入**：builder 中的初始主题请求、FollowSystem 的浅色 / 深色请求、主题定义来源（含用户主题目录）、应用标识。
  回调返回后 builder 不再被修改，Prepare 只读。
- **工作**：用"仅全局"模式的 registry（`GeneratedThemeSchema.GetGlobalTokens()` + 算法，不含控件描述）
  加载主题目录、规范化配置（去掉 `ThemeConfig.Controls`）、编译全局层，得到全局 Token 值表与全局资源表。
  配置了 FollowSystem 时同时编译浅色、深色两套。
- **仅全局绑定**：用户主题目录里的 JSON 主题可以带控件段，普通 registry 遇到未注册控件会报 `ATMTHM2002` 并拒绝
  该主题。"仅全局"模式下，`ThemeDefinitionBinder` 跳过控件段且不产生诊断；控件段只影响控件层 Token，全局层结果
  不受影响。编译主题绑定器不改：内置编译主题只引用全局 Token（已核实：`RequireControl` 0 处），将来若出现带控件段的
  编译主题，Prepare 失败按 §6 退化为不提前显示 Splash，不影响启动。
- **输出**：不可变的 `PreparedStartup`：每种外观的全局层快照。提交时把对应快照作为初始主题编译的
  `ReusableParent` 传入，`ThemeCompiler.CanReuseGlobal` 成立，不重复编译全局层。Prepare 的主题目录不含控件段，
  不在提交时复用；提交照常用完整 registry 加载目录。
- **纯数据约束**：后台线程不得访问 `Dispatcher`、不得创建 `AvaloniaObject`、不得设置 Avalonia 属性。
  已核实全局层满足这一点：`DesignToken` 是普通类，全局 Token 没有画刷类型，颜色投影为
  `ImmutableSolidColorBrush`。违反约束时 `VerifyAccess` 会因 UI 线程已固定而抛出，失败会传到 `Attach`。

### 3.3 `Attach`

`public static void AtomUIStartup.Attach(Application application)`，对同一实例幂等。没有 `UseAtomUI` 配置时直接返回，
纯 Avalonia 应用不受影响。步骤：

1. **取得 builder**：本实例是第一个实例时取提前执行的 builder；否则新建 builder 并同步执行回调。
2. **取得 Prepare 结果**：提前执行的，等待后台任务完成（上限 30 s）；否则同步执行 Prepare。之后的实例同步重算
   （JIT 已预热，开销很小）。Prepare 失败或超时都只写 Warning，本次不显示 Splash，照常提交（§6）。
3. **选外观**：配置了 FollowSystem 时读取平台外观，挑选浅色或深色全局层。
4. **判定 Splash**（全部满足才显示）：已调用 `UseSplash`；模式为 `Always`，或为 `Auto` 且
   `RuntimeFeature.IsDynamicCodeCompiled`；`ApplicationLifetime` 是 `IClassicDesktopStyleApplicationLifetime`；
   不是设计模式。
5. **显示 Splash**：创建 `SplashWindow`，窗口资源 = 全局资源表 + Splash 主题；`Show()` 后用
   `Dispatcher.UIThread.MainLoop(token)` 驱动到首个渲染批次。上限 5 s，由线程级计时器取消 token
   （`DispatcherFrame.Continue = false` 会取消平台运行循环的 token，已核实能唤醒循环）。
   平台不支持受控运行循环（`MainLoop` 抛 `PlatformNotSupportedException`）或创建失败时，写日志并跳过 Splash。
6. **提交**（§3.4）。
7. 登记主循环开始后的交接工作（§3.5）；状态 → `Committed`。

### 3.4 提交（一次）

顺序与现在的 `UseAtomUI` 一致，只有两处变化：应用语言注册移到这里，字体注册改为 initializer。

1. 应用级语言注册：`application is IGeneratedApplicationLanguageBootstrap` 时调用 `RegisterApplicationLanguages`。
   已核实与注册顺序无关（`LanguageCatalogRegistry.Create` 按 CatalogId 排序 catalog，按语言 / 来源排序 bundle）。
2. 构建本地化宿主。
3. 构建 `ThemeManager`：补全全部包片段（BootstrapRegistration 删除后，片段一律推迟到这里），
   使用 Prepare 的全局层与主题目录。
4. 创建 owned services。
5. `ApplicationScope.InitializeApplication`：挂载资源与样式，`ThemeManager.InitializeApplication`（复用全局层），
   注册作用域，挂接 owned services。
6. 执行 initializers（`UseAlibabaSansFont` / `UseAlibabaPuHuiTiFont` 改为在此注册字体集合；字体在文字排版时才解析，
   而提交期间不会排版主窗体内容）。

任何一步失败按 §6 处理。

### 3.5 主循环与交接

`Attach` 在提交后 `Post` 一个回调，它在主循环开始后执行（此时 `OnFrameworkInitializationCompleted` 已返回、
生命周期已启动）。判定主窗体：

1. 配置了 `UseMainWindow`：执行工厂（UI 线程，可 await），把返回的窗口赋给 `desktop.MainWindow` 并 `Show()`；
   同时应用自行给 `desktop.MainWindow` 赋值视为配置错误。
2. 否则 `desktop.MainWindow` 已赋值（传统写法、Prism）：就是它。
3. 否则视为没有主窗体：立即关闭 Splash，状态 → `Ready`。

主窗体确定后状态 → `Presenting`；等它 `Opened` 后的首个渲染批次（上限 3 s，超时写 Warning 并直接交接，
防止最小化或被遮挡的窗口让 Splash 永不关闭），然后 Splash 合成层淡出 150 ms 并关闭，状态 → `Ready`。
没有 Splash 时同样按此规则推进状态。

### 3.6 状态与进度

`IAtomUIStartup.State`：`Preparing → Committed → Presenting → Ready`，另有 `Failed`、`Canceled`。
`Completion` 在 `Ready` 时成功完成，失败时带原始异常，取消时为已取消。框架只在状态切换时发布进度，
不伪造百分比；Splash 默认显示不确定进度，只有应用通过 `UseMainWindow` 的 `context.Progress` 汇报时才显示确定进度。
所有失败都写 Error 日志（修复研发日志 §7 第 4 项"失败只设异常、不写日志"）。

### 3.7 宿主差异

| 宿主 | 提前执行 | Splash | 说明 |
|---|---|---|---|
| 桌面（经典生命周期） | 是 | 按 §3.3 判定 | 主路径 |
| headless 测试 | 第一个实例是 | 否（无生命周期） | 之后的实例在 `Attach` 中同步重跑回调 |
| 设计器 | 否 | 否 | 仍然提交，预览器能渲染 AtomUI 控件 |
| 浏览器 / WASI | 否 | 否 | 同步 Prepare，避免单线程阻塞等待造成死锁 |

## 4. 挂接生成器与守卫

- **生成器**（`AtomUI.Generator`）：Application 识别规则与 `ApplicationLanguageHostInfo` 一致
  （继承 `Avalonia.Application`、partial、顶层、非泛型），独立实现以免启动生成器依赖本地化类型；为**每个**具体的
  partial Application 生成：

  ```csharp
  public override void RegisterServices()
  {
      base.RegisterServices();
      global::AtomUI.AtomUIStartup.Attach(this);
  }
  ```

  不沿用语言生成器"只允许一个具体 Application"的规则（测试工程常有多个应用类）。
- **诊断**：类中已手写 `RegisterServices` 重写且方法体内没有调用 `AtomUIStartup.Attach` 时报错误诊断，
  提示在 `base.RegisterServices()` 之后调用；已调用则不生成。抽象类不生成（由派生类生成，`Attach` 幂等）。
- **生成器的可得性**：NuGet 消费者经 buildTransitive 自动获得；仓库内工程需要显式 Analyzer 引用。
  按仓库约定，普通测试工程不引用生成器，测试应用类手写上面的重写（同时锁定手动挂接这条公开契约）。
- **守卫**：`UseAtomUI` 注册 `AfterApplicationSetup` 回调（执行于 `Initialize` 之后、
  `OnFrameworkInitializationCompleted` 之前）；本实例没有执行 `Attach` 即抛出异常，说明"把应用类声明为 partial，
  或手动调用 `AtomUIStartup.Attach(this)`"。
- **约束**（写入文档）：不要在应用构造函数中创建控件（早于 `RegisterServices`）。

## 5. Splash

### 5.1 类型（`src/AtomUI.Controls/Splash/`，命名空间 `AtomUI.Controls`）

- `Splash : TemplatedControl`：`Logo`、`BackgroundImage`（`IImage?`）、`Title`、`Message`（`string?`）、
  `Progress`（`double`，0–1）、`IsIndeterminate`（默认 true）、`Status`（`SplashStatus.Loading` / `Error`）。
  布局完全由 ControlTheme 决定；可选模板部件 `PART_CloseButton`（任意元素，错误态下点击即关闭）。
  错误态另支持 Esc 关闭。
- `SplashWindow : Avalonia.Controls.Window`：自带极简模板，不依赖任何主题。`WindowDecorations = None`（Avalonia 12 的名称）、
  `SizeToContent = WidthAndHeight`、`CanResize = false`、`WindowStartupLocation = CenterScreen`、交接前 `Topmost`、
  `TransparencyLevelHint = [Transparent]`；平台不支持透明时，`Splash` 得到 `:opaque` 伪类，
  默认主题据此退化为方角、无阴影。Wayland 不允许应用定位窗口，窗口位置由合成器决定。
- `SplashProgressBar : Control`：无模板，在 `Render` 中绘制轨道与指示条；属性 `Value`、`IsIndeterminate`、
  `TrackBrush`、`IndicatorBrush`、`CornerRadius`。不确定进度用合成层动画（渲染线程）。
- `SplashMode`：`Off`、`Always`、`Auto`。`SplashStatus`：`Loading`、`Error`。

### 5.2 默认主题

`src/AtomUI.Controls/Splash/Themes/SplashDefaultTheme.axaml`（x:Class 资源字典）。按控件目录规范放在 `Themes/`，
但项目给它设置 `Splash/SplashDefaultTheme.axaml` 的逻辑路径；包主题资产生成器按逻辑路径判断，因此不会登记
这份启动前资源字典，由 `SplashWindow` 在提交前直接 `new` 加载。卡片（`ColorBgContainer`、`BorderRadiusLG`、`BoxShadowSecondary`），
可铺满背景图（UniformToFill）；居中纵列依次为 Logo（64 px）、标题（`FontSizeHeading4`、`ColorText`）、
消息（`FontSize`、`ColorTextSecondary`）、进度条（指示 `ColorPrimary`、轨道 `ColorFillSecondary`）；
错误态消息用 `ColorError`，配错误图标（`Path`）与右上角关闭图标。只使用 `{atom:SharedTokenResource …}`
（即以 `SharedTokenKind` 为键的 `DynamicResource`，与全局资源表的键一致，已核实），不绑定字体 Token。

### 5.3 配置 API

```csharp
// AtomUI.Controls
public static IAtomUIBuilder UseSplash(this IAtomUIBuilder builder,
                                       SplashMode mode = SplashMode.Auto,
                                       Action<SplashOptionsBuilder>? configure = null);

public sealed class SplashOptionsBuilder
{
    // 配置回调早于平台初始化执行，此时无法创建位图；图片在显示 Splash 时于 UI 线程加载。
    public SplashOptionsBuilder WithLogo(Uri source);                  // avares:// 或文件 URI
    public SplashOptionsBuilder WithLogo(Func<IImage> factory);        // 矢量图等自定义来源
    public SplashOptionsBuilder WithBackgroundImage(Uri source);
    public SplashOptionsBuilder WithBackgroundImage(Func<IImage> factory);
    public SplashOptionsBuilder WithTitle(string title);
    public SplashOptionsBuilder WithMessage(string message);
    public SplashOptionsBuilder WithControlTheme<TTheme>() where TTheme : ResourceDictionary, new();
}

// AtomUI.Core
public static IAtomUIBuilder UseMainWindow(this IAtomUIBuilder builder,
                                          Func<AtomUIMainWindowContext, Task<Window>> factory);

public sealed class AtomUIMainWindowContext
{
    public IProgress<AtomUIStartupProgress> Progress { get; }   // 更新 Splash 的 Message 与确定进度
    public CancellationToken CancellationToken { get; }         // 用户关闭 Splash 时取消
}

// AtomUI.Core。Message 为 null 表示不改消息；Fraction 为 null 表示保持不确定进度，否则取 0–1。
public readonly record struct AtomUIStartupProgress(string? Message, double? Fraction = null);
```

- `AtomUI.Core` 不引用 `AtomUI.Controls`。`UseSplash` 向 builder 登记一个内部的 Splash 呈现器
  （由 `AtomUI.Controls` 实现），`Attach` 只通过该接口显示 Splash、汇报进度、切换错误态、交接。
- 自定义主题：`TTheme` 是 x:Class 资源字典，内含 `TargetType="atom:Splash"` 的隐式 ControlTheme；
  它合并在默认主题之后，同键优先。泛型 `new()` 对裁剪与 AOT 安全。
- 删除：Extras 的 `SplashService`、`ISplashService`、静态 API、`SplashOptions`（含 `MinimumShowDuration`）、
  `SplashToken`、Semantic Part 及其生成的注册条目。

### 5.4 自定义主题约束（硬约束，写入文档）

只能使用 Splash 控件、Avalonia 基础元素（`Border`、`Panel`、`Image`、`TextBlock`、`Path` 等）和全局 Token；
不能放 AtomUI 控件（提交前没有主题）；不能依赖自定义字体；图片应小（在关键路径上解码）；
模板直接计入 Splash 首帧。

## 6. 出错与取消

| 失败点 | 处理 |
|---|---|
| 配置回调抛错 | 在 `UseAtomUI`（或之后实例的 `Attach`）中同步抛出原异常。 |
| 后台 Prepare 失败 | Warning 日志，不显示 Splash，照常提交。Prepare 用的是"仅全局"registry，失败可能只是该模式的限制（例如将来出现带控件段的编译主题）；真实的主题错误由提交时的完整 registry 原样抛出（初始主题失败抛 `ThemeLoadException`，原来只记诊断的仍只记诊断）。 |
| 等待 Prepare 超过 30 s | Warning 日志（附诊断：疑似后台代码访问了 UI 线程），不显示 Splash，照常提交。 |
| Splash 创建或 pump 失败 | Error 日志，跳过 Splash，启动继续。 |
| Splash 首帧等待超过 5 s | Warning 日志，启动继续。 |
| 提交失败 | 按现有逻辑回滚（释放 ThemeManager、本地化宿主、owned services，注销作用域）。有 Splash：切换错误态并驱动 Dispatcher 直到用户关闭，然后从 `Attach` 重抛原异常，传到应用 `Main`。无 Splash：立即重抛。 |
| `UseMainWindow` 工厂抛错 | Splash 切换错误态；用户关闭后把原异常投递到 Dispatcher 抛出，传到 `Main`。 |
| 交接前用户关闭 Splash | 取消：`Completion` 为已取消，调用 `Shutdown()`。提交期间的关闭请求在主循环开始后处理。 |
| 主窗体在首帧前被关闭 | 取消。 |
| 主窗体首帧超过 3 s 未到 | Warning 日志，直接交接。 |

错误态只用 Avalonia 基础元素渲染，不显示任何框架文字，只显示错误消息，因此不依赖本地化。

## 7. 诊断

`AtomUIStartupEventSource`（名称 `AtomUI-Startup`），默认关闭，`IsEnabled()` 之外零开销。事件：配置回调、
后台 Prepare、等待 Prepare、Splash 显示与首帧、提交（各段：本地化、包片段、ThemeManager、owned services、
initializers）、主窗体判定与首帧、Ready / Failed / Canceled。测量程序用进程内 `EventListener` 读取分段；
NativeAOT 默认裁掉 EventSource（`EventSourceSupport=false`），AOT 测量沿用测量程序自身打点。

## 8. 删除与迁移

**删除（`src/`）**
- 分级启动编排：`Deferred`（公开扩展与 builder 内部列表）、`StageTwoEvolutionPlan`、`StartupTopLevelReadiness`、
  `StartupLifetimeReadiness`、`ApplicationScope` 中的 Stage 2 调度、就绪探测与取消分支。
- 演化：`ThemeManager` 注册演化整套（Prepare / Validate / Commit / Publish / Abort）、`ThemeRegistrationEvolution`、
  仅服务于演化的 `ThemeRegistrationInput` 路径；`ThemeManagerBuilder` 的 Stage 2 窗口、`MergeStageTwoInto`、
  `Build(staged)`；`ControlRegistrationState` 的重开与关窗；`LocalizationBuilder` 的 Stage 2 窗口、
  `CaptureStageTwoEvolution`、`allowUnresolvedBundles`、`BuiltContext`；语言侧演化入口。
- BootstrapRegistration：特性类、30 处标注（25 个控件、5 个资源类）、生成器的 `IsBootstrap` 发射与
  `ControlRegistrationFragmentAttribute.IsBootstrap`。
- 分区 ABI：`RegisterPartitionedPackage` / `EnsurePartitionedPackage`，生成器对 `AtomUICriticalControls` 的支持。
- `Application.UseAtomUI(...)`；内部实现类 `AtomUIStartup` 改名，`AtomUIStartup` 这个名字留给公开的静态入口。
- Extras 中的 Splash 全部。Extras 包本身保留为空壳（用户决定）：工程、包标识与 `UseDesktopExtras()` 保留，
  暂无控件，留给以后 Ant Design 之外的稳定控件。
- 未跟踪且无引用的 `controlgallery/AtomUIGallery/TemporaryStartupTimeline.cs`。

**保留**：`GetThemeManager` / `GetLocalizer` / `GetLanguageManager` / `GetStartup`。

**迁移**
- 测试宿主：7 个测试应用文件 + 11 个直接调用旧入口的测试文件，改用 `AppBuilder.UseAtomUI`，手写挂接。
- 注册夹具：ProductApp、Host / Browser / NamedResources 消费者、语言包端到端 Consumer，按最终用户方式迁移（生成器挂接）。
- 工具：Performance、DataGridPerformance、TabOverflowPerformance、StartupPerformance；StartupPerformance 增加
  Splash 场景，记录 Splash 首帧，用于验收线 2。
- Gallery：Desktop、Browser 应用类迁移并删除 `Deferred` 包装；Desktop 启动使用默认 Splash 与 Gallery Logo；
  删除工作区窗口的加载占位；Splash 示例页改为页内预览（默认主题、自定义主题）。
- 文档（31 个）：删除 `docs/architecture/foundations/staged-startup-evolution.md`、
  `docs/architecture/foundations/lazy-control-registration.md`、`docs/releases/unreleased-startup-api-changes.md`、
  `docs/releases/unreleased-startup-api-changes.zh-CN.md`、`docs/controls/desktop/feedback/splash/`；新增
  `docs/architecture/systems/startup/`（overview、lifecycle、splash、verification）与 `docs/guides/startup/overview.md`
  （默认用法、自定义 ControlTheme、关闭、性能规则、Prism 示例）；其余引用旧入口的文档同步改写；`docs/releases/`
  写迁移说明，并补齐系列中未记录的破坏性变更（研发日志 §7 第 13 项）。包作者指南属于步骤 2。

## 9. 测试处置

遵守[测试价值与生命周期规范](../../engineering/development/test-value-and-lifecycle.md)。

- **删除存量**：已删除功能的用例（分级启动编排、Stage 2 演化、分区 ABI、`IsBootstrap` 发射），涉及
  `ApplicationScopeTests`、`ApplicationLocalizationStartupTests`、`ControlRegistrationRuntimeTests`、
  `TypeMapRegistrationGeneratorTests` 四个文件中的相关用例；按规范 §6 格式记录删除原因。
- **迁移**：18 个调用旧入口的测试文件改用新入口，继续保护原有契约。
- **临时验证**：放在 `/private/tmp/atomui-test-startup-step1/` 或 `tests/<项目>/Temporary/startup-step1/`，验证后删除。
- **常驻候选（3 条，收尾时逐条按准入四条判定并写理由）**：
  1. 生成器：生成 `RegisterServices` 挂接；手写重写缺少 `Attach` 时报诊断。
  2. 提交早于 `Initialize`：`Initialize` 中构造的 AtomUI 控件已有主题（Prism 契约）。
  3. Splash 默认主题只依赖全局 Token：用全局资源表解析默认主题，不缺资源。
- 按测试隔离的 headless 重跑回调不新增测试：现有测试套件即以该模式运行。

## 10. 验证与退出条件

1. 改动模块逐个 `python3 scripts/verification/test.py run --scope iterate --path …`；不跑全量（除非用户要求）。
2. 性能（步骤 0 测量程序，同机交错各 10 轮，优先空闲机器；有负载如实标注）：
   - **验收线 2**：JIT Release 下 Splash 首帧 ≤ plain 首帧 + 0.15 s。未达线先定位，不降低验收线。
   - **不退步**：Gallery JIT 主窗体完整首帧不差于 HEAD `60dd72afb` 基线（配对中位差）；AOT 不差于 6.2.5。
   - **记录**：主窗体首帧之后的最长 UI 卡顿（HEAD 约 0.5 s 的 Stage 2 冻结应消失）。
3. NativeAOT：Gallery 与测量程序发布成功并能启动；`Auto` 模式下 AOT 不显示 Splash。
4. Prism：`/private/tmp` 下临时 Prism.Avalonia 示例（引用工作区源码）验证 Shell 有主题、Splash 正确交接；验证后删除。
5. 浏览器：Browser Gallery 构建并启动（同步 Prepare、无 Splash、不死锁）；本机缺 wasm 工具链时至少构建通过并说明。
6. 设计器：手动确认预览器中 AtomUI 控件正常显示。
7. 三个桌面平台：macOS 由实施者执行；Windows、Linux（X11、Wayland）由用户按清单执行 Gallery 启动、Prism 示例、
   提前关闭、错误态。
8. `git diff --check`、文档链接检查；按规范 §8 写交付记录。

**退出条件**：Gallery 与 Prism 示例在三个桌面平台可用；验收线 2 达标；上述验证全部完成或如实列出缺口。

## 11. 研发日志 §7 映射

| 项 | 归属 |
|---|---|
| 7.1 窗口提前而内容未提前；首帧后 UI 长任务 | 随分级启动删除消失 |
| 7.1 ReadyToRun 未采用 | 步骤 5（步骤 0 另发现 Toolchain 构建边界阻挡 R2R 发布） |
| 1 chrome 模板子控件缺 ControlTheme | 随删除消失：窗口出现前全部片段已提交 |
| 2 主题包按含版本号的程序集身份匹配 | 步骤 4；若在步骤 4 之前发版，须单独修复 |
| 3 激活等待无超时 | 随删除消失；新交接不等待激活，首帧等待上限 3 s |
| 4 Stage 2 失败静默 | 本次修复：所有失败写 Error 日志 |
| 5 主题资源工厂失败推迟到首次查找 | 后续：CI 全量物化检查（总 spec §13） |
| 6 首帧后同步长任务 | 随删除消失 |
| 7 Gallery 首阶段菜单标签为空 | 随删除消失 |
| 8 staged 放置约束无诊断 | 随删除消失 |
| 9 Motion 语义变化引出的回归 | 与启动无关，单独修复 |
| 10 编译主题字面量无一致性测试 | 步骤 4 |
| 11 主题语义错误推迟到启动 | 步骤 4 |
| 12 未发布即"遗留"的分区 ABI | 本次删除 |
| 13 发布说明与事实不符 | 本次修复：删除旧说明，迁移说明补齐真实的破坏性变更 |
| 14 构建期元数据残留在 AOT 产物 | 步骤 2 |
| 15 生成器开销 | 步骤 2 / 4 |
| 16 ABI 文档与实现不符 | 步骤 4 |
| 17 本地化演化边角问题 | 随删除消失 |
| 18 死代码 | 随删除消失 |
| 19 资源类方案的副作用 | 步骤 2 |
| 20 线程模型不一致 | 后续（步骤 2 统一）；本次的后台 Prepare 不触及资源层 |
| 21 小项 | 本次：`Complete` 重写、`Deferred` 删除、分区指纹删除、Gallery 占位删除 |
| 22 测量口径 | 步骤 0 已解决（同口径 A/B、每变体 10 轮交错） |
| 23 提交范围过宽 | 流程约束：步骤 1 的提交按关注点拆分，等用户指令 |
| 24 仓库卫生 | 本次删除临时探针；`docs/superpowers/plans/` 已不再被忽略；其余后续清理 |

## 12. 风险

- **跨平台 pump**：S1 只在 macOS 验证。Windows、X11 的 dispatcher 实现了受控运行循环（经典桌面生命周期本身依赖它），
  但实际首帧行为待步骤 1 验证；不可行时该平台退化为不显示 Splash，内核收益不受影响。
- **提交后应用级样式作用到 Splash**：`ThemeManager` 的 TopLevel 样式、FlowDirection 样式、App.axaml 的样式会作用到
  已打开的 Splash 窗口。取值来自同一主题，预期无可见变化，实施时核实。
- **后台纯数据约束被后续代码破坏**：UI 线程已固定，违反时 `VerifyAccess` 立即抛出；另有 30 s 等待上限与 EventSource 分段。
- **第三方包在 `UseXxx` 中调用平台 API**：回调现在早于平台初始化执行。文档写明约束，步骤 2 的包契约正式规定。
- **生成器识别边界**：嵌套类、泛型应用类不生成挂接，由守卫给出修复说明。
- **验收线 2 未达**：先用 EventSource 分段定位，不降低验收线（总 spec §12.4 第 5 条）。
