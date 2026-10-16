# 启动生命周期

本文定义从 `UseAtomUI` 到 `Ready` 的完整生命周期。总体结构与模块职责见[启动系统架构](overview.md)；Splash 本身见
[Splash](splash.md)。

## 1. `UseAtomUI`（AppBuilder 阶段）

```csharp
public static AppBuilder UseAtomUI(this AppBuilder appBuilder, Action<IAtomUIBuilder> configure);
```

位于 `AtomUI.Core`，命名空间 `AtomUI`。同一 `AppBuilder` 只能调用一次，第二次调用抛 `InvalidOperationException`。

- **应用类型**取 `appBuilder.ApplicationType`（`Configure<TApp>()` 与 `Configure<TApp>(Func<TApp>)` 都会设置），
  用于推导默认应用标识。
- **提前执行**：在主线程、平台初始化之前执行配置回调；主题输入符合后台资格时启动 Prepare。执行前先读取 `Dispatcher.UIThread`，
  把 UI 线程固定为主线程：后台代码若误触 Dispatcher 会立即抛出，而不是抢走 UI 线程身份导致平台初始化失败。
- **回调只做声明**：回调执行时 Avalonia 平台尚未初始化，不能访问 `FontManager`、`Application`、`TopLevel`，也不能
  创建位图。字体注册与应用级语言注册推迟到提交时执行。回调抛出的异常原样同步抛出。
- **不提前执行的情况**：设计模式（`Design.IsDesignMode`，预览器在调用 `BuildAvaloniaApp` 之前设置）与单线程运行时
  （`OperatingSystem.IsBrowser()` / `OperatingSystem.IsWasi()`），全部改在 `Attach` 中同步完成。
- **不启动后台 Prepare 的情况**：应用类型为 null 且回调中没有调用 `WithApplicationId`，或主题来源包含未声明后台安全的
  自定义 resolver/手写编译程序。前者不能用占位应用标识读取用户目录；后者在需要显示 Splash 时于 UI 线程同步 Prepare，
  保留 `SplashMode.Always` 的显示契约。官方生成主题与内部用户目录仍满足后台资格。
- 启动配置经 `appBuilder.With(...)` 写入 `AvaloniaLocator`（`SetupUnsafe` 开头写入；headless 每个测试都会重新写入
  同一个对象），守卫经 `AfterApplicationSetup` 注册（§8）。

## 2. 后台 Prepare

- **输入**：初始主题请求、FollowSystem 的浅色 / 深色请求、主题定义来源（含用户主题目录）、应用标识，以及符合预热条件的
  已启用包 definition 快照。后台任务不捕获或读取可变 builder。
- **包选择预热**：仅 `IsTrimmed && RuntimeFeature.IsDynamicCodeCompiled` 的提前后台路径，在主题准备前调用生成的
  `WarmSelection`。该方法用一个生成常量键执行 `GetSelectionMap().TryGetValue`，即使该键已被裁掉也会创建外部映射；不枚举
  候选键、不解析或构造 fragment、不加载主题资产、不执行 `OnCommit` / `OnFirstUse`。此举将裁剪 R2R 的首次 TypeMap
  建表移出 UI 提交期。非裁剪、NativeAOT、Browser/Wasi、旧 net8 注册后端与空包不做该预热；禁止后台准备的自定义主题来源
  仍保持 §1 的线程边界。
- **映射复用**：CoreCLR 的原始 `TypeMapping.GetOrCreateExternalTypeMapping` 调用会重复建表。生成类的未标记
  `GetSelectionMap` 使用线程安全、只保存成功结果的缓存，预热、selection、fragment resolve 与兼容 collector 共用同一实例；
  创建失败后可重试。带 `GeneratedTypeMapAccessor` 标记的 `GetTypeMap` 保留原始调用体，供 Browser 与跨版本后端识别、改写，
  不改变 key、触发类型或裁剪选择。
- **预热入口契约**：`ControlPackageDefinition` 保留五、六参数构造器，默认没有预热回调。七参数构造器是隐藏的生成 ABI，
  显式传入 `warmSelection` 即承诺纯数据、无副作用、线程安全，并能与超时后的同步 selection 并发。框架不通过反射鉴别
  回调是否来自生成器；手写调用者若显式使用该入口，同样承担此契约。
- **工作**：用"仅全局"模式的 schema registry（全局 Token 与算法，不含控件描述）加载主题目录、去掉配置中的控件段、
  编译全局层，得到全局 Token 值与全局资源表。配置了 FollowSystem 时同时编译浅色、深色两套。
- **仅全局绑定**：用户主题目录里的主题可以带控件段。"仅全局"模式下主题定义绑定器跳过控件段且不产生诊断；控件段只
  影响控件层 Token，全局层结果不受影响。
- **输出**：每种外观的全局层快照。提交时把对应快照作为初始主题编译的可复用父层传入，不重复编译全局层；Prepare 加载的
  主题目录不含控件段，不在提交时复用，提交照常用完整 registry 加载目录。
- **纯数据约束**：后台线程不得访问 `Dispatcher`、不得创建 `AvaloniaObject`、不得设置 Avalonia 属性。违反约束时
  `VerifyAccess` 会因 UI 线程已固定而抛出，失败传到 `Attach` 并按 §7 退化。
- **来源资格**：启动期普通 `IThemeDefinitionResolver`（连同它返回的 `IThemeDefinitionSource.OpenRead`）和手写
  `CompiledThemeProgram` 默认在 UI 线程执行。resolver 实现 `IBackgroundThemeDefinitionResolver`、或程序重写
  `IsBackgroundPrepareSafe => true` 时，作者须保证整条回调链可在工作线程运行，并能与 30 秒超时后的 UI 线程回退并发。
  生成器只给自身发射的纯数据程序设置此能力；旧二进制程序默认走 UI 线程。

## 3. `Attach`

```csharp
public static void AtomUIStartup.Attach(Application application);
```

在 `base.RegisterServices()` 之后调用（通常由生成器生成，见 §8）。对同一实例幂等；没有 `UseAtomUI` 配置时直接返回，
纯 Avalonia 应用不受影响。步骤：

1. **取得 builder**：本实例是该配置的第一个应用实例时，取提前执行的 builder；否则新建 builder 并同步执行回调。
2. **校验宿主**：配置了 `UseMainWindow` 但 `ApplicationLifetime` 不是 `IClassicDesktopStyleApplicationLifetime` 时抛出。
3. **取得 Prepare 结果**：提前执行的，等待后台任务完成（上限 30 s）；没有提前执行且输入允许 Prepare 时，只有本次
   会显示 Splash 才同步执行 Prepare，否则跳过（提交会在 UI 线程自行编译全局层）。
4. **选外观**：配置了 FollowSystem 时读取平台外观，选浅色或深色全局层；提交使用同一次读取的结果。
5. **判定并显示 Splash**（见 [Splash](splash.md#2-显示判定)）：显示后驱动 Dispatcher 直到 Splash 首个渲染批次完成，
   上限 5 s。
6. **提交**（§4）。
7. 状态 → `Committed`；登记主循环开始后的交接工作（§5）。

**回调按实例执行**：配置回调对每个 `Application` 实例执行一次。headless 测试按测试隔离时，同一 `AppBuilder` 会先后
创建多个应用实例；第一个实例使用提前执行的结果，之后的实例在 `Attach` 中同步重跑回调，各自拥有独立的 builder 与
内核状态。

## 4. 提交（一次）

1. 应用级语言注册：应用类实现生成的语言 bootstrap 时调用其注册方法（与注册顺序无关，Catalog 与 Bundle 由 registry
   排序）。
2. 构建本地化宿主。
3. 构建 `ThemeManager`：构建已启用包索引，保持片段与资源惰性，使用 Prepare 的全局层与完整主题目录。
4. 创建自有服务（例如图片加载器）。
5. `ApplicationScope.InitializeApplication`：挂载资源与样式、初始化主题（复用全局层）、注册作用域、挂接自有服务。
6. 执行 initializer（`UseAlibabaSansFont` 等字体注册在此执行：字体在文字排版时才解析，提交期间不排版主窗体内容）。

包的生命周期 context 在服务 Attach 前对所有新包就绪。Attach、initializer 或其他包的 `OnCommit` 若实际消费
控件/语义/资源，目标包先完成自己的 `OnCommit`，再执行一次 `OnFirstUse`，不会把未初始化 descriptor 返回。
尚未建立 scope 的服务 Factory 若尝试消费包会明确失败；同包重入及跨包提交循环也会失败并回滚。
包循环的普通提交遍历会跳过此前因实际消费而已提交的包，不会重复执行钩子。

任何一步失败都按已完成步骤逆序回滚（丢弃待处理注册、释放 `ThemeManager`、本地化宿主与自有服务、注销作用域）；
服务 Factory 在 ApplicationScope 创建前失败也释放已构建的 ThemeManager 与包索引，
再按 §7 处理。提交成功后 `Application.GetStartup()`、`GetThemeManager()`、`GetLocalizer()` 等即可使用。

## 5. 主循环与交接

`Attach` 在提交后向 Dispatcher 投递一个回调，它在主循环开始后执行（此时 `OnFrameworkInitializationCompleted` 已返回、
生命周期已启动）。不是经典桌面生命周期（headless、single-view）时不投递，直接进入 `Ready`。判定主窗体：

1. 配置了 `UseMainWindow`：在 UI 线程执行工厂（可 await，经 `AtomUIMainWindowContext.Progress` 汇报进度），把返回的
   窗口赋给 `desktop.MainWindow` 并显示。应用同时自行给 `desktop.MainWindow` 赋值视为配置错误。
2. 否则 `desktop.MainWindow` 已赋值（传统写法、Prism）：就是它。
3. 否则视为没有主窗体：立即关闭 Splash，状态 → `Ready`。

主窗体确定后状态 → `Presenting`；等它 `Opened`（上限 3 s）后的首个渲染批次（上限 3 s），然后 Splash 淡出并关闭，
状态 → `Ready`。两个超时只写 Warning 并直接交接，防止最小化或被遮挡的窗口让 Splash 永不关闭。没有 Splash 时同样按
此规则推进状态。主窗体在等待期间关闭、或用户关闭 Splash 触发启动取消时，关闭/取消信号会立即中断
`Opened` 与合成批次等待，不继续消耗 3 秒超时，也不记作成功首帧或 Ready。

## 6. 状态与进度

```csharp
public interface IAtomUIStartup
{
    AtomUIStartupState State { get; }
    Task Completion { get; }
    event EventHandler<AtomUIStartupProgressEventArgs>? ProgressChanged;
}
```

经 `Application.GetStartup()` 取得；内核提交之前或提交失败后调用会抛出。状态：

```text
Preparing → Committed → Presenting → Ready
     └──────────┴────────────┴──→ Failed / Canceled
```

- 状态只前进；`Ready`、`Failed`、`Canceled` 是终态，进入后不可覆盖。
- `Completion` 在 `Ready` 时成功完成，失败时带原始异常，取消时为已取消。调用者可用 `WaitAsync(CancellationToken)`
  取消自身等待，不影响应用启动。
- `ProgressChanged` 在 UI 线程发布，参数含当前 `State` 与 `Detail`；框架只在状态切换时发布，不伪造百分比。订阅不重放，
  处理器异常逐个隔离并写 Warning 日志；进入终态后清空订阅。
- 所有失败都写 Error 日志。

## 7. 出错与取消

| 失败点 | 处理 |
|---|---|
| 配置回调抛错 | 在 `UseAtomUI`（或之后实例的 `Attach`）中同步抛出原异常。 |
| 后台 Prepare 失败（含 TypeMap 预热） | Warning 日志，不显示 Splash，照常提交。提交时仍执行真实 selection 与完整主题绑定；持续的映射或主题错误按提交失败处理，保留原异常。 |
| 等待 Prepare 超过 30 s | Warning 日志（疑似后台代码访问了 UI 线程），不显示 Splash，照常提交。 |
| Splash 创建或驱动失败 | Error 日志，跳过 Splash，启动继续。 |
| Splash 首帧等待超过 5 s | Warning 日志，启动继续。 |
| 提交失败 | 回滚（§4）。有 Splash：切换错误态并驱动 Dispatcher 直到用户关闭，然后从 `Attach` 重抛原异常，传到应用 `Main`。无 Splash：立即重抛。 |
| `UseMainWindow` 工厂抛错 | 状态 → `Failed`；有 Splash 时切换错误态，用户关闭后把原异常投递到 Dispatcher 抛出，传到 `Main`。 |
| 交接前用户关闭 Splash | 状态 → `Canceled`，取消 `AtomUIMainWindowContext.CancellationToken`，调用 `Shutdown()`。提交期间的关闭请求在主循环开始后处理。 |
| 主窗体在首帧前被关闭 | 状态 → `Canceled`。 |
| 主窗体首帧超过 3 s 未到 | Warning 日志，直接交接。 |

错误态只显示错误消息，不显示任何框架文字，因此不依赖本地化。

## 8. 挂接生成器与守卫

**生成器**（`AtomUI.Generator`，`ApplicationStartupHookGenerator`）为每个具体的 `partial` Application（继承
`Avalonia.Application`、顶层、非泛型）生成：

```csharp
public override void RegisterServices()
{
    base.RegisterServices();
    global::AtomUI.AtomUIStartup.Attach(this);
}
```

- 编译中不存在 `AtomUI.AtomUIStartup` 时不生成。
- 类中已手写 `RegisterServices` 重写：方法体调用了 `AtomUIStartup.Attach` 时不生成；没有调用时报错误诊断
  `ATOMUIGEN039`，提示在 `base.RegisterServices()` 之后调用。抽象类自身不报（无法实例化也无法自行修复，由派生类挂接）。
- 抽象、泛型、嵌套、非 partial 的类不生成，由运行时守卫给出修复说明。
- 同一程序集可以有多个应用类（测试工程常见），每个都生成。
- NuGet 消费者经 buildTransitive 自动获得生成器；仓库内工程需要显式 Analyzer 引用，普通测试工程不引用生成器，测试
  应用类手写上面的重写。

**守卫**：`UseAtomUI` 注册的 `AfterApplicationSetup` 回调在 `Initialize` 之后、`OnFrameworkInitializationCompleted` 之前
执行；本实例没有执行 `Attach` 即抛出 `InvalidOperationException`，说明"把应用类声明为 partial，或在
`base.RegisterServices()` 之后调用 `AtomUIStartup.Attach(this)`"。

**约束**：不要在应用构造函数中创建控件或访问主题——构造函数早于 `RegisterServices`，此时内核尚未提交。

## 9. 宿主差异

| 宿主 | 提前执行 | Splash | 说明 |
|---|---|---|---|
| 桌面（经典生命周期） | 是 | 按[显示判定](splash.md#2-显示判定) | 主路径 |
| headless 测试 | 第一个实例是 | 否（无经典生命周期） | 之后的实例在 `Attach` 中同步重跑回调；提交后直接 `Ready` |
| 设计器 | 否 | 否 | 仍然提交，预览器能渲染 AtomUI 控件 |
| 浏览器 / WASI | 否 | 否 | 同步执行，避免单线程阻塞等待造成死锁；提交后直接 `Ready` |

## 10. 内部入口 `AttachWith`

`AtomUIStartup.AttachWith(Application, Action<IAtomUIBuilder>)`（internal）按显式配置直接提交：不提前执行、不做 Prepare、
不显示 Splash，同一实例重复调用抛出。只供测试与仓库内部工具使用；应用一律走 `UseAtomUI` + `Attach`。
