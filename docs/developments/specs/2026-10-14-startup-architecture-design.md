# AtomUI 启动架构重构设计

日期：2026-10-14。状态：已确认。步骤 1 对 §0、§4、§5、§10、§11 的修订见
[步骤 1 子 spec](2026-10-14-startup-step1-lifecycle-and-splash-design.md) §2，本文已同步。
本设计不考虑与 6.x 公开 API 的兼容，只追求启动架构简洁与高性能。现有方案的问题清单见
[研发日志 §7](../progress/2026-10-12-startup-optimization-rd-log.md#7-2026-10-14-全系列只读审查发现)。

## 0. 摘要

启动只做三件事：构建期把所有包链接成一份校验过的启动镜像；运行时在后台准备内核（含全局 Token），
在 `RegisterServices()` 里先显示只依赖全局 Token 的 Splash，再一次提交；Splash 盖住应用的主窗体构建。
控件、主题资产、语言目录全部在首次使用时物化。
官方与生态控件包保持 `builder.UseXxx()` 入口并显式启用，启用本身只是数据操作；构建期看不到的包可在运行时追加。
分级启动（Deferred、BootstrapRegistration、演化、就绪探测）整体删除。

预测（Gallery，macOS arm64，Release JIT，自 Main 起算，详见 §12）：Splash 首帧约 0.37–0.52 s；主窗体完整首帧
从 6.2.5 的约 2.2 s 降到 1.45–1.65 s；内核关键路径从约 1.07 s 降到 40–90 ms。

## 1. 背景

- 注册阶段的成本几乎全是首次执行：Desktop 注册收集新进程首次 207 ms，同进程再跑 0.5 ms；启动期首次编译约
  1500 个生成方法（[启动恢复调查](../progress/2026-10-13-startup-recovery.md)）。
- 启动工作量随"包里有多少"增长：Gallery 首屏只物化 33/315 个主题资产、14/183 个控件 Token。
- 运行时每次启动重算构建期已确定的事实：生成契约指纹、跨包 owner、schema 冲突、全部语言翻译覆盖。
- 分级启动只把工作挪到首帧之后：窗口提前约 0.7 s，Shell 反而晚约 0.2 s，并带来闭包维护、演化提交等问题。

## 2. 目标、非目标与成功标准

**目标**

1. 视觉：尽早出现与应用风格一致、可定制的 Splash。
2. 总时长：主窗体完整首帧与全部就绪显著缩短，不以"先出窗口"代替收益。
3. 架构：内核单一状态、单次提交；无部分注册、无演化；应用与包作者零额外声明。
4. 覆盖开发期 JIT、发布版 JIT、NativeAOT、浏览器 WASM 四种形态。

**非目标**：保持 6.x API 兼容；非 Avalonia 的原生 Splash；优化应用自身的 Shell 代码（只给指引）。

**成功标准**：§12.4 的验收线全部满足，测量方法见 §13。

## 3. 设计原则

1. 构建期能确定的事在构建期做完：链接、规范化、校验。
2. 运行时按用付费：常驻的只有索引，内容首次使用时物化。
3. 内核一个状态、提交一次。
4. 关键路径并行：内核准备与 Avalonia 平台初始化重叠。
5. Splash 用简易控件是性能要求，不是能力限制。
6. 启动自带可观测性，不依赖临时探针。

## 4. 总体结构

```
构建期
  控件包：生成包清单（纯数据）＋包内校验
  应用：链接全部引用包清单 → 启动镜像（索引、slot、资源键优先级、语言目录、主题定义）＋跨包校验，失败即构建失败

运行期
  Main ─ UseAtomUI(...) ─┬─ 后台 Prepare：加载镜像、全局 Token、当前语言索引
                         └─ Avalonia 平台初始化
  RegisterServices ─ Splash 首帧（只依赖全局 Token）─ Commit（一次）
  Initialize / OnFrameworkInitializationCompleted ─ 应用建主窗体（Splash 覆盖）
  主循环 ─ 主窗体首个渲染批次 ─ Splash 淡出 ─ Ready
  之后 ─ 控件描述、Token、主题资产、语义、语言目录按首次使用物化
```

## 5. 启动时序

对齐 Avalonia 12.1.3 `AppBuilder.SetupUnsafe`（`src/Avalonia.Controls/AppBuilder.cs:366-385`）：

```
BuildAvaloniaApp(): .UseAtomUI(builder => ...)        执行配置回调（只做声明），启动后台 Prepare
SetupUnsafe:
  平台初始化 ‖ 后台 Prepare
  AfterPlatformServicesSetup
  创建 Application
  RegisterServices()  ← 生成的重写：base.RegisterServices(); AtomUIStartup.Attach(this);
                         Attach：等待 Prepare → 按策略显示 Splash 并驱动 Dispatcher 至首帧 → Commit
  Initialize()        ← App.axaml；Prism 在此创建并 Show Shell
  AfterApplicationSetup / AfterSetup
  OnFrameworkInitializationCompleted ← 传统写法在此创建主窗体
lifetime.Start()：主循环；UseMainWindow 工厂（如使用）；主窗体首帧后关闭 Splash → Ready
```

- **挂接方式（已定：生成器自动）**：生成器为 `AppBuilder.Configure<TApp>()` 中的 `TApp`（须为 partial）生成
  `RegisterServices()` 重写。应用已自行重写或 `TApp` 非 partial 时报诊断，提示手动调用 `AtomUIStartup.Attach(this)`。
- **误配守卫**：`UseAtomUI` 注册 `AfterApplicationSetup` 检查，本实例未执行 `Attach` 即抛错并指明挂接方法；
  不允许控件静默缺主题。Commit 之前只有 Splash 在运行，其约束见 §10。
- **回调按实例执行**：配置回调对每个 `Application` 实例执行一次；headless 按测试隔离时，之后的实例在
  `Attach` 中同步重跑（步骤 1 子 spec §3）。
- **提交早于 App.axaml**：App.axaml 可以使用 AtomUI 的 StaticResource；应用样式排在 ThemeManager 之后，覆盖顺序自然。

## 6. 内核

**Prepare（后台，纯数据）**：加载启动镜像；按初始主题请求与 ThemeConfig 的全局部分计算全局 Token 全集；
解析 FollowSystem；建立当前语言索引。不接触 Avalonia 对象、可变 Builder 或应用回调。

**Commit（UI 线程，一次）**：注册字体；创建并挂载 ThemeManager、Token provider、静态资源层（只含索引与惰性代理）、
语言 provider；按启用顺序运行各包的 `OnCommit` 钩子（§7.1）。之后除运行时追加（§7.3）外不再有任何注册窗口或演化。

**惰性层**：控件描述在首次查找时从镜像物化；控件 Token 快照沿用独立惰性 cell；主题字典沿用惰性代理；
主题定义的控件层在该控件首次编译时绑定；语言目录按目录物化，切换语言时只建目标语言。

**线程**：后台只运行框架生成的纯数据步骤；自定义主题来源、evaluator 等未声明线程安全的回调回到 UI 线程。

**出错与取消**：Prepare/Commit 失败 → Splash 切错误态、写 Error 日志、`Completion` 失败；关闭 Splash → 取消启动并
主动 Shutdown；不允许静默失败。

## 7. 控件包、包清单与启动镜像

### 7.1 包入口契约（官方与生态统一）

AtomUI 生态的官方、开源与商业控件包都通过 `builder.UseXxx()` 接入，新架构保留这一形态，只替换其下层：

```csharp
public static IAtomUIBuilder UseAcmeCharts(this IAtomUIBuilder builder, Action<AcmeChartsOptions>? configure = null) =>
    GeneratedControlPackage.Register(builder, configure, static package => package
        .DependsOn(static b => b.UseDesktopControls())
        .OnCommit(static context => { /* 轻量：UI 线程、Application 已存在 */ })
        .OnFirstUse(static context => { /* 本包首个控件物化时执行，如商业许可校验 */ }));
```

- 启用保持显式：`UseXxx()` 是包传配置、声明依赖、挂接钩子的位置；不采用"引用即启用"。
- `UseXxx()` 在 AppBuilder 阶段执行，平台尚未初始化：只做声明，不得访问 Avalonia 对象或 `Application`。
- 钩子只有两个时机：`OnCommit`（计入关键路径，须轻量，由 EventSource 按包计时）与 `OnFirstUse`（按用付费）。
- 删除 `complete` 回调中的语言模块注册（语言目录属于包清单）和 `IControlThemesProvider` 对象（资产顺序由镜像决定）。
- 依赖用 `DependsOn` 声明并自动启用；未启用的包中的控件被使用时，首次查找主题即抛错并指明应调用的 `UseXxx()`。

### 7.2 包清单与构建期链接

- **包清单**：控件包生成物从"每控件一组方法"改为嵌入程序集的单个数据常量（控件身份、Token schema、资产与资源键、
  语义部件、语言目录、TypeMap 键、清单格式版本）。编码格式由子项目 2 设计。
- **链接范围**：应用工程直接或传递引用的全部 AtomUI 包，包括只提供二进制的商业包；清单格式版本不匹配时给出构建诊断，
  提示用对应版本的生成器重新构建该包。
- **应用链接**：生成器读取清单并生成启动镜像，完成跨包校验：Token owner、语义冲突、身份重复、资源键冲突、
  支持语言的翻译覆盖、主题定义与 schema 的匹配。全部错误为构建诊断。
- **运行时**：只核对镜像戳记，不再计算指纹、排序或构建校验 registry。
- **裁剪**：镜像只存字符串与 TypeMap 键，不引用具体类型；被裁掉的控件在 TypeMap 中查不到即视为不存在。

### 7.3 运行时追加

构建期看不到的包（插件目录、按需加载的模块）通过同一个入口追加：
`application.GetAtomUI().LoadPackages(builder => builder.UseAcmeCharts())`。链接与校验复用构建期同一套链接器实现；
只追加 slot、资源键与语言目录，不重算全局 Token；每次追加只发出一次资源变更通知。成本只由使用动态加载的应用承担。

## 8. 主题

- **两层**：全局层在 Prepare 计算；控件层按控件惰性编译，复用现有 cell 与 `CanReuseGlobal`。
- **主题定义**：构建任务调用 Core 的真实 codec 编译主题（替代生成器里的字面量重新实现）；按全局层/控件层分开绑定；
  在应用链接时对照实际 schema 校验，运行时不再按含版本号的程序集身份比较，版本耦合问题随之消失。
- **静态资源层**：单一 provider，键索引来自镜像；工厂失败保留现有可重试语义，并由 CI 全量物化检查兜底（§13）。

## 9. 本地化

目录编译为数据；Prepare 只建当前语言索引；目录按首次查询物化；切换语言时构建目标语言；翻译覆盖在应用链接时校验，
不再推迟到运行时。

## 10. Splash

**归属**：`src/AtomUI.Desktop.Controls.Extras/Splash` 全部迁入 `AtomUI.Controls`，命名空间 `AtomUI.Controls`；
Extras 与 Desktop 不保留 Splash 代码。

**组成**：`Splash` 是模板控件，在 Commit 之前显示，只依赖全局 Token（作为 `SplashWindow` 的窗口局部资源提供）
和系统默认字体；没有控件 Token 和 Semantic Part。只新增一个无模板、在 `Render` 中绘制的 `SplashProgressBar`；
文字、Logo、图标直接用 Avalonia 的 `TextBlock` / `Image` / `Path`。动画只用合成层（渲染线程）。公开属性只保留
Logo、BackgroundImage、Title、Message、Progress、IsIndeterminate、Status；布局完全由 ControlTheme 决定。
`SplashWindow` 继承 Avalonia `Window`，透明无边框、随内容定尺寸。

**定制**：默认主题；或用 `UseSplash(...).WithControlTheme<TTheme>()` 指定开发者编写的 x:Class 资源字典
（Splash 早于 App.axaml，自定义主题不能放在 App.axaml）；或 Off。可使用全部 Token 资源。

**策略**：`Off`；`Always`；`Auto`（默认）在 Commit 时按 `RuntimeFeature.IsDynamicCodeCompiled` 判定：JIT（含 ReadyToRun）
显示，AOT 不显示；无窗口宿主（headless、设计器）不显示。不设最短显示时长。

**宿主**：桌面用 `SplashWindow`；浏览器用 index.html 加载层，主视图就绪后移除；移动端将来用原生启动屏，Splash 视图可作遮罩。

**交接**：主窗体（`UseMainWindow` 工厂创建，或应用/Prism 自行创建并赋给 lifetime）首个渲染批次完成后，Splash 淡出关闭。
同时使用 `UseMainWindow` 与自行赋值 `MainWindow` 时报错。

**文档约束**：模板直接计入 Splash 首帧；不放重量级控件；背景图控制分辨率与体积；自定义内容只做展示。

## 11. 对外 API

```csharp
public static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<App>()
              .UseAtomUIPlatformDetect()
              .WithAtomUIDefaultOptions()
              .UseAtomUI(builder => builder
                  .UseAlibabaSansFont()
                  .WithInitialTheme(IThemeManager.DEFAULT_THEME_ID)
                  .UseLanguages(LanguageTags.EnUS, [LanguageTags.EnUS, LanguageTags.ZhCN])
                  .UseDesktopControls()
                  .UseDesktopDataGrid()
                  .UseDesktopColorPicker()
                  .UseSplash(SplashMode.Auto, splash => splash
                      .WithLogo(...)
                      .WithBackgroundImage(...)
                      .WithTitle("...")
                      .WithControlTheme<MySplashTheme>())
                  .UseMainWindow(async context =>
                  {
                      await LoadAppDataAsync(context.Progress);
                      return new MainWindow();
                  }));

public partial class App : Application   // Prism 应用为 PrismApplication，写法相同
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
```

- 配置方法全部返回 `IAtomUIBuilder` 以便链式调用。不调用 `UseSplash` 即不显示 Splash。单视图宿主的
  `UseMainView` 推迟到浏览器 / 移动端步骤。
- DataGrid、ColorPicker 与生态包一样写在同一配置链中，不再延迟注册；`UseXxx()` 调用顺序无关，依赖由 `DependsOn`
  自动启用。启动时只启用镜像中的索引，首次使用该包的控件时才物化主题、Token 与语言目录（§7.1）。
- `IAtomUIStartup`：Preparing → Committed → Presenting → Ready，另有 Failed / Canceled；`Completion` 在 Ready 时完成；
  进度包含框架阶段与应用汇报的步骤。没有主窗体的应用在 Committed 后即 Ready。
- 删除 `Application.UseAtomUI(...)`。

**Prism**：Prism 在 `Initialize()` 中创建并 Show Shell（PrismLibrary/Prism `91b8986c`，
`src/Avalonia/Prism.Avalonia/PrismApplicationBase.cs:36-70,151-154`）。内核在 `RegisterServices()` 提交，因此 Shell
创建时主题已就绪；`RegisterTypes` 中可把 ThemeManager、Localizer 注册进容器；Splash 在 Prism 同步初始化期间保持显示。

## 12. 性能预测与验收线

### 12.1 依据

| 来源 | 内容 |
|---|---|
| [研发日志 §0.1](../progress/2026-10-12-startup-optimization-rd-log.md) | 6.2.5 Gallery 基线（macOS，Release/Debug/AOT，自进程启动，8 对） |
| [10-12 验证记录](../progress/2026-10-12-startup-algorithm-validation.md) | Gallery 同步/分期三模式（自 Main，各 10 进程），Launch 与 Main 的差约 50–65 ms |
| [研发日志 §2.2](../progress/2026-10-12-startup-optimization-rd-log.md) | Gallery 同步 UseAtomUI 579 ms 分段 |
| [BootstrapRegistration 记录](../progress/2026-10-14-bootstrap-registration.md) | 当前 HEAD Gallery（自 Main，6 对） |
| [启动恢复调查](../progress/2026-10-13-startup-recovery.md) | ReadyToRun 骨架实验 |
| [Windows 结果](../progress/2026-10-11-windows-startup-algorithm-results.md) | Windows 空窗口与纯 Avalonia 对照 |

### 12.2 推算模型

```
主窗体完整首帧 ≈ 平台初始化 + max(0, Prepare − 平台初始化) + Commit + Splash（JIT 时）
               + 应用主窗体构建与首帧 − Splash 预热的 Avalonia 共享路径
```

取值（JIT，Gallery）：平台初始化 0.25–0.30 s；Prepare 0.08–0.15 s（被平台初始化完全覆盖）；Commit 0.04–0.09 s
（字体约 18 ms、原 initializer 约 53 ms 中转为 `OnCommit` 的部分、挂载）；Splash 0.08–0.13 s；应用主窗体构建与首帧以 10-12 同步模式
推出约 1.1 s，另加按需物化 0.02–0.06 s、减去预热 0.03–0.06 s。以 10-12 同步模式 Shell 批次 1.955 s 为起点，
移除 UseAtomUI 关键路径 0.49–0.54 s 后得到 1.45–1.63 s。

### 12.3 预测值

Gallery，macOS arm64，自 Main 起算，中位数。可信度：高＝由同口径实测直接推出；中＝跨批次推算；低＝尚无实测。

| 形态 / 指标 | 6.2.5 | 当前 HEAD | 新架构预测 | 相对 6.2.5 | 可信度 |
|---|---:|---:|---:|---:|:---:|
| JIT：首个可见画面 | Opened ≈2.13 s | chrome＋占位 1.14 s | Splash 0.37–0.52 s | −75%～−83% | 中 |
| JIT：主窗体完整首帧 | ≈2.2–2.3 s | Shell Ready 1.97 s | 1.45–1.65 s | −25%～−37% | 中 |
| JIT：全部就绪 | ≈2.47 s | ≈2.0 s（Shell Ready） | 1.55–1.85 s | −25%～−37% | 中 |
| JIT：内核关键路径 | 1.07 s | 0.28 s（仅 Stage 1） | 0.04–0.09 s | −92%～−96% | 高 |
| JIT＋MultiCoreJit（第二次起）：主窗体完整首帧 | — | — | 1.15–1.45 s | — | 低 |
| ReadyToRun 发布：主窗体完整首帧 | — | — | 0.85–1.05 s | — | 低（骨架实验外推） |
| ReadyToRun 发布：Splash 首帧 | — | — | 0.25–0.35 s | — | 低 |
| AOT：主窗体完整首帧（不显示 Splash） | Opened ≈0.46 s | 未测（10-12 分期版 Shell 批次 0.54 s） | 0.30–0.34 s | −26%～−35% | 中 |
| AOT：内核关键路径 | 0.20 s | 未测（10-12 分期版 Stage 1 为 7.4 ms） | 5–15 ms | −93%～−98% | 中 |

Windows 空窗口（Release JIT，Launch→根 Render，Splash 关闭）：纯 Avalonia 0.53 s；`ee1db4001` 1.86 s；`914ce40ef` 1.66 s；
预测 0.82–0.92 s（Commit 0.04–0.09 s，加 AtomUI Window chrome 额外 0.25–0.30 s），约 −45%～−51%，可信度中。

内核初始化托管分配（Windows 空窗口）：`914ce40ef` 5.27 MB，预测降到 1–2 MB（只构造镜像索引与全局 Token），可信度低。
WASM 目前没有启动基线，步骤 0 建立后再给预测。

### 12.4 验收线

基线统一用步骤 0 在同一测量程序上重测的 6.2.5 与 HEAD；每组至少 10 个交错冷启动进程。

1. JIT 主窗体完整首帧 ≤ 6.2.5 × 0.75；AOT ≤ 6.2.5 × 0.8。
2. JIT Splash 首帧 ≤ 同机纯 Avalonia 空窗口首帧 + 0.15 s。
3. 全部就绪不劣于 HEAD；AOT 下任何指标不劣于 6.2.5。
4. 内核关键路径（Commit）JIT ≤ 0.1 s，AOT ≤ 0.02 s。
5. 未达线时先定位关键路径；不降低验收线，也不通过删减功能或推迟主窗体内容达标。

## 13. 诊断与性能治理

- 内建 `AtomUIStartup` EventSource：Prepare、Commit、Splash、主窗体各阶段打点，默认关闭，零侵入。临时探针一律删除。
- 扩展 `tools/performances/AtomUI.StartupPerformance`：覆盖空窗口与 Gallery；开发期 JIT、ReadyToRun、AOT、WASM；
  记录 Splash 首帧、主窗体完整首帧、全部就绪、UI 最长卡顿。
- 预算写入 `docs/architecture/systems/startup/verification.md`，只紧不松。
- CI 增加主题资产全量物化检查，弥补惰性工厂把失败推迟到首次使用的风险。

## 14. 删除清单

- 分级启动：`Deferred`、`[BootstrapRegistration]` 及 25 个控件与 5 个资源类上的标注、`StageTwoEvolutionPlan`、
  `StartupTopLevelReadiness`、`StartupLifetimeReadiness`、ThemeManager/Localization 的演化与 Stage 2 窗口、部分包补齐。
- `RegisterPartitionedPackage` / `EnsurePartitionedPackage`。
- 运行时指纹校验、排序与 registry 构建校验；启动时构建全部语言快照。
- `Application.UseAtomUI(...)`；Extras 中的 Splash；生成器中的主题字面量重新实现。
- 各包入口中 `complete` 回调里的语言模块注册、各包的 `XxxThemesProvider`；Gallery 对 Extras、ColorPicker、DataGrid、
  GalleryControls 的 `Deferred` 包装。
- 未跟踪的 `controlgallery/AtomUIGallery/TemporaryStartupTimeline.cs`。

## 15. 文档调整

- 新建 `docs/architecture/systems/startup/`：`overview.md`、`lifecycle.md`、`splash.md`、`verification.md`。
- 新建 `docs/guides/startup/overview.md`：默认、自定义 ControlTheme、Off 三种用法与性能规则，含 Prism 示例。
- 新建 `docs/guides/startup/package-authoring.md`：面向开源与商业包作者，说明入口契约、`DependsOn`/`OnCommit`/`OnFirstUse`、
  清单格式版本、运行时追加和从 6.x 迁移的步骤。
- `docs/releases/` 写迁移说明：启动入口、Splash 替换、删除的 API。
- 删除 `docs/controls/desktop/feedback/splash/`、`staged-startup-evolution.md`、`lazy-control-registration.md`、
  `unreleased-startup-api-changes*.md`；更新 `startup-and-registration.md`、`architecture/systems/overview.md`、`docs/overview.md`
  及桌面反馈分类导航；Gallery 的 Splash 示例与测试随之改写。

## 16. 实施路线

每步单独 spec → plan → 实施 → 验收，完成后再进入下一步。

| 步骤 | 内容 | 退出条件 |
|---|---|---|
| 0 | 测量程序与基线（6.2.5、HEAD、纯 Avalonia）；原型验证：主循环前驱动 Dispatcher 画出 Splash（重点 macOS）、合成层动画在 UI 线程忙时是否继续、主窗体交接、MultiCoreJit 收益、生成器读取引用清单的成本 | 基线入库；每个原型给出可行/不可行与替代方案 |
| 1 | 启动管线、`RegisterServices` 挂接与守卫、Splash 迁入 `AtomUI.Controls`、主窗体交接、删除分级启动、EventSource | Gallery 与 Prism 示例在三个桌面平台可用；验收线 2 达标 |
| 2 | 包入口契约（`UseXxx` 新实现与钩子）、包清单、启动镜像、构建期校验、控件描述按需物化、运行时追加、包作者指南 | 验收线 4 达标；裁剪产物选择集合不变；Gallery 与一个模拟生态包（含 `OnFirstUse`、运行时追加）通过 |
| 3 | 本地化数据化、按目录物化、构建期覆盖校验 | 语言切换与覆盖校验通过 |
| 4 | 主题定义两层绑定，构建任务使用真实 codec | 字面量差分全覆盖；版本耦合消除 |
| 5 | MultiCoreJit / ReadyToRun 默认策略 | 依步骤 0 数据决定是否默认开启 |
| 6 | 主窗体 chrome 构建成本 | 验收线 1 达标 |

## 17. 风险与待验证项

- 主循环开始前驱动 Dispatcher：macOS 若不可行，同步创建窗口的模式（Prism、传统写法）退化为不显示 Splash，
  内核收益不受影响；`UseMainWindow` 模式不受此限制。
- 合成层动画在 UI 线程长时间忙碌时能否持续，决定 Splash 是否会"冻住"。
- 启动镜像与 TypeMap 裁剪的配合、生成器读取大量引用清单的耗时。
- Avalonia 设计器也会调用 `BuildAvaloniaApp`：设计模式下不启动后台 Prepare、不显示 Splash。
- 生成的 `RegisterServices` 重写与应用代码冲突：靠诊断与运行时守卫兜底。
- 生态包必须用新生成器重新构建（不提供兼容层）：靠清单格式版本诊断和包作者指南降低迁移成本。
- 包在 `UseXxx()` 或 `OnCommit` 中做重活会拖慢所有使用它的应用：EventSource 按包计时，指南给出预算。
- 运行时追加不得与已启用包发生资源键或身份冲突（校验即拒绝），因此不会改变已解析资源的结果。
- 主窗体构建仍以 JIT 为主，内核架构无法消除，只能靠预编译与 chrome 优化（步骤 5、6）。
- 预测值来自不同批次的实测推算，以步骤 0 重测为准；与预测偏差超过区间时，先修正模型再继续实施。
