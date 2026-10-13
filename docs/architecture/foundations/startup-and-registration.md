# 启动与注册链路

> 控件注册与裁剪采用已实现的 TypeMap 体系；本地验证、未完成门槛与发布状态统一见
> [AOT 与裁剪架构](aot-and-trimming.md#1-状态与事实边界)。

AtomUI 的启动链路分为平台默认配置和主题控件注册两部分。

## AppBuilder 默认配置

入口位于 `src/AtomUI.Core/AppBuilderExtensions.cs`：

```csharp
AppBuilder.Configure<App>()
          .UseReactiveUI()
          .UseAtomUIPlatformDetect()
          .WithAtomUIDefaultOptions()
          .StartWithClassicDesktopLifetime(args);
```

`UseAtomUIPlatformDetect()` 位于 `AtomUI.Desktop.Controls`。Linux 下选择顺序为：显式
`AtomUIWindowingPlatform`、`ATOMUI_WINDOWING_PLATFORM`、非空 `WAYLAND_DISPLAY`、X11/XWayland。
不要只根据 `XDG_SESSION_TYPE` 选择 Wayland；headless/framebuffer 应用应直接配置自己的后端。
选择 Wayland 时，该入口会在创建首个 toplevel 前设置 `ForceDrawnDecorations = true` 并禁用服务端装饰协商，确保
AtomUI Window 从首帧开始使用 CSD，不能先显示 compositor 标题栏再切换为 AtomUI 标题栏。

`WithAtomUIDefaultOptions()` 当前设置：

- Windows 10：`AngleEgl/Software` 渲染回退与 `WinUIComposition`、`DirectComposition`、`LowLatencyDxgiSwapChain`、`RedirectionSurface` 顺序的合成回退策略。
- Windows 11+：`AngleEgl/Software` 渲染回退与 `WinUIComposition`、`DirectComposition`、`LowLatencyDxgiSwapChain`、`RedirectionSurface` 顺序的合成回退策略。
- macOS Avalonia Native 渲染优先级：OpenGL、Metal、Software。
- X11 平台选项：`EnableDrawnDecorations = true`。
- 字体 fallback：`Microsoft YaHei`。

Windows 选项通过公开 `Win32PlatformOptions` 强类型配置，不使用运行时反射。详细边界见
[Windows live resize 与窗口装饰架构](../systems/windowing/windows-live-resize.md)。

这一步只配置 Avalonia 平台选项，不注册 AtomUI 控件主题。

## Application 注册 AtomUI

入口位于 `src/AtomUI.Core/ApplicationExtensions.cs`：

```csharp
public override void Initialize()
{
    AvaloniaXamlLoader.Load(this);

    this.UseAtomUI(builder =>
    {
        builder.UseLanguages(
            LanguageTags.EnUS,
            [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW]);
        builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
        builder.UseAlibabaSansFont();
        builder.UseDesktopControls();
        builder.UseDesktopColorPicker();
        builder.UseDesktopDataGrid();
    });
}
```

`UseAtomUI()` 会创建根 `IAtomUIBuilder` 及相互独立的 `LocalizationBuilder`、`ThemeManagerBuilder`。应用生成的
本地化 bootstrap 先注册应用 Catalog 和外部语言包 Bundle，随后执行用户注册动作；最后分别构建全部支持语言
Snapshot 与主题 schema、ControlTheme asset manifest、首个 ThemeSnapshot、Root ThemeContext 和唯一 ThemeManager。

构建后的主题运行流见 [AtomUI 主题系统](../systems/theming/runtime.md)。简化顺序是：

1. 本地化 Builder 冻结 Catalog/Bundle Registry，为全部支持语言生成完整 Snapshot，并以默认语言初始化稳定
   `LanguageResourceProvider`、`ILanguageManager` 与 `ILocalizer`。
2. 主题 Builder 解析 Application Id，收集生成式 Control descriptor、ControlTheme asset manifest、
   `IThemeDefinitionResolver`、算法 descriptor 和不可变初始 `ThemeRequest` 模板。
3. `ThemeSchemaRegistry` 构建并冻结 exact CLR type/identity、完整 Global Token schema、Control Own Token schema、
   资产导出主题、RequiredTokenOwners 和 Semantic Part 契约。跨包校验在全部包收集后执行；无效 descriptor、
   重复 type/identity、错误 Token owner、资源 URI 或 Semantic Part Theme 冲突在此失败。
4. `ThemeCatalog` 执行内置、应用资源及可选用户目录 Resolver，并通过统一 Reader 和 Binder 生成 typed theme
   definition。静态来源失败终止启动；用户来源失败时使用静态 Catalog 启动并保留 diagnostics。
5. FollowSystem 在编译前解析初始系统 appearance，并选择完整的 Light/Dark request 模板。
6. `ThemeCompiler` 在 ThemeManager 挂载前同步生成首个不可变 `ThemeSnapshot`。
7. Root ThemeContext 和唯一的 `ThemeTokenResourceProvider` 使用该 snapshot 初始化；ThemeManager 同时准备向
   每个 TopLevel 注入 Root ThemeContext 的全局 style。
8. ThemeManager 与本地化 Provider 以完整资源状态挂载到 Application，显式设置匹配 snapshot 的 Avalonia Light/Dark variant，
   并且只发布一次初始 Token 资源通知。
9. 后续主题更新由 ThemeManager 的五阶段 `ThemeTransaction` 提交；语言更新由 `LanguageManager` 独立交换预构建 Snapshot。

如果应用需要首帧就是暗色或紧凑主题，应在 builder 阶段通过 `ThemeConfig` 配置初始主题算法，而不是在
`UseAtomUI()` 之后调用运行期切换 API：

```csharp
this.UseAtomUI(builder =>
{
    builder.WithInitialTheme(
        IThemeManager.DEFAULT_THEME_ID,
        new ThemeConfigBuilder()
            .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark)
            .Build());
    builder.UseDesktopControls();
});
```

`ThemeConfigBuilder` 只用于一次性构造并在 `Build()` 时防御性复制；传入 Manager 或
`ThemeConfigProvider.Config` 的 `ThemeConfig` 及其集合均不可变。运行时更新必须替换完整 Config，不能修改已
提交对象中的 Algorithms、Tokens 或 Controls 集合。

应用启动后的主题变化使用 `IThemeManager.ApplyThemeAsync(ThemeRequest)`。AtomUI 的主题 id 和 Compact
算法不编码进 Avalonia `ThemeVariant`；运行时只根据已提交 snapshot 设置 Avalonia Light 或 Dark。

应用可以显式启用用户主题目录：

```csharp
this.UseAtomUI(builder =>
{
    builder.WithApplicationId("AtomUIGallery");
    builder.UseUserThemeDirectory();
});
```

未显式设置 Application Id 时，默认使用具体 Application 类型所在程序集的简单名称；`Application.Name` 不作为
目录身份。默认目录为 `Environment.SpecialFolder.ApplicationData/{ApplicationId}/Themes`。运行时调用
`IThemeManager.ReloadThemesAsync()` 手动刷新；主题系统不使用 `FileSystemWatcher` 自动监听。

局部主题由继承 `ThemeVariantScope` 的 `ThemeConfigProvider` 建立。Provider 首次 attach 在内容可见前同步创建
稳定 ThemeContext 和唯一 ResourceProvider；后续 Config 替换由同一个 ThemeManager 事务化处理。普通
Popup/Flyout 通过逻辑树自然继承，独立 Window/Dialog/Notification TopLevel 必须从显式 owner 获得
`ThemeContextLease` 和宿主私有 ResourceBridge；无 owner 静态 API 使用根主题。

## 分级启动（Staged Startup）

> 状态：2026-10-12 工作树已完成算法与提交边界重构，保持 opt-in；性能和平台证据单独验收。
> 资源优先级、跨代缓存身份、快照生命周期及跨主题/语言发布的职责见[分级启动演化与提交边界](staged-startup-evolution.md)。

`UseAtomUI()` 默认把全部注册与初始化在窗口出现前同步完成。对于注册面较大的应用（如 Gallery 的全部
可选包），可以通过**分级启动**把"完整 AtomUI chrome 首帧"与"框架完备初始化"拆成两个阶段：窗口出现
只等待 chrome 闭包（分区关键集 + 公共语言 + 关键字体）。指定宿主的可见根完成 compositor rendered batch，
且普通前台窗口收到请求的激活事件后，调度剩余注册。
这不证明显示器实际呈现。机制是**纯 opt-in**——不调用 `Deferred` 的应用走同步构建路径。

```csharp
this.UseAtomUI(builder =>
{
    builder.UseLanguages(LanguageTags.EnUS, [LanguageTags.EnUS, LanguageTags.ZhCN]);
    builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
    builder.UseDesktopControls();            // Stage 1：分区包按关键集注册（见下节）

    builder.Deferred(() =>                   // Stage 2 注册块（按声明序执行）
    {
        builder.UseDesktopColorPicker();
        builder.UseDesktopDataGrid();
    });
});
```

`Deferred` 支持控件包、主题定义来源、本地化 Catalog/Bundle 和语言配置。服务、初始主题、FollowSystem 模板、
字体、应用标识、initializer 与 Deferred 声明只能在初始配置窗口调用；后置或晚期调用立即抛错。
主题 initializer 的任意副作用不能参加 Stage 2 原子提交。成功封存统一检查包失败状态与未完成注册。

### 统一启动结果

`Application.GetStartup()` 返回该应用唯一的 `IAtomUIStartup`，同步与分阶段模式共用契约。
尚未调用 `UseAtomUI()` 时抛出明确的初始化错误。`State` 描述当前状态：

| 路径 | 状态顺序 |
| --- | --- |
| 同步 | `Configuring → Ready` |
| 分阶段 | `Configuring → WaitingForHost → Initializing → Ready` |
| 失败或宿主取消 | 从进行中的状态进入 `Failed` 或 `Canceled`，终态不可覆盖 |

`Completion` 是可重复等待的框架就绪任务。成功为 `RanToCompletion`；失败保留原始异常；应用启动取消为
`Canceled`。发布终态前先关闭注册输入、释放宿主等待者及 staging hooks。启动已成功后释放应用服务不会
把结果改成取消。晚创建窗口、single-view 与多个观察者都能读取同一个结果。

```csharp
var startup = Application.Current!.GetStartup();
await startup.Completion.WaitAsync(windowCancellation);
// 在 UI 线程安排实际页面或 shell 构建。
```

`WaitAsync` 的调用者取消只结束自身等待；应用启动只受自身宿主生命周期控制。运行期间取消先关闭注册
能力与等待订阅，待准备任务退出并完成清理后，`Completion` 才以取消结束。

`ProgressChanged` 提供可选的 UI 线程进度，参数含 `State` 和 `Detail`。订阅不会重放，处理器异常逐个隔离并
记录 System warning；读取终态必须使用 `State`/`Completion`。同步模式不增加后台工作或 Dispatcher 调度。
旧 `IAtomUIStartupEvents`、`AtomUIStartupEvents`、`StartupStage`、`StartupStageEventArgs` 及两处
`GetStartupEvents()` 已移除，迁移见[待发布启动 API 迁移](../../releases/unreleased-startup-api-changes.zh-CN.md)。

Gallery 构造加载态后等待 `Completion`，成功后将 Shell 构建排入 Background，失败展示异常信息。
窗口关闭或卸载取消自身等待和未执行的构建操作；晚创建窗口同样构建内容。Shell 构建/布局自身仍不可抢占，
其成本单独测量。框架 Ready 不代表 Shell 已绘制，宿主就绪证据见
[就绪、取消与重入](staged-startup-evolution.md#4-就绪取消与重入)。

### 控件粒度分区

`builder.UseDesktopControls()` 的包可以由生成器按**关键集**分区：csproj 声明
`AtomUICriticalControls`（逗号分隔的控件简单名，如 `Window,WindowTitleBar,Menu,Icon,Popup`；该值通过
`.editorconfig` 暴露给生成器，不能使用会被解释为注释起始符的分号），
生成器发射 `CollectCritical/CollectDeferred` 两段收集，并调用独立 ABI 入口
`RegisterPartitionedPackage` / `EnsurePartitionedPackage`。Stage 1 只执行关键集收集
（chrome 闭包先行），余量在 Stage 2 注册窗口由 `RunDeferredCollects` 收集为与包同 id/provider 的
「余量段」，经 `MergeStageTwoInto` 并入后重建 registry（v2）并按当前请求重应用主题（控件快照零
物化——首查编译，见 runtime.md §7/§8）。

- 未声明 `AtomUICriticalControls` 的包不分区，发射保持 6 参调用原样；使用匹配 schema 协议的老生成包在分期应用
  中回退为整包 `Deferred` 粒度（保留旧入口 ABI，历史指纹协议边界见[注册契约](control-registration-contracts.md#71-分级演化的身份和资源顺序)）。**方向是单向的**：分区包（新生成器产出，独立分区入口调用）
  必须配新 AtomUI.Core（含 `RegisterPartitionedPackage` / `EnsurePartitionedPackage`）——旧 Core 上加载会以 MissingMethodException 失败，同 F2
  陈旧包先例方向；条件（net8）后端不支持分区，声明了 `AtomUICriticalControls` 的 net8 兼容包会被
  生成器显式降级为不分区并给出 `ATOMUIREG009` warning；该回退保持完整注册和可构建性。
  仓库 Desktop 项目仅对兼容 net10.0 的目标声明关键集；net8 构建不传入该配置，直接生成完整单阶段入口，
  正常多目标构建不产生回退警告。该条件按目标框架兼容性判断，保留现代平台后缀目标的分区配置。
- 关键集必须同时覆盖 descriptor、窗口装饰资产、标题栏、菜单和真实平台依赖。仅统计 headless 查询过的
  `{Window, WindowTitleBar}` token 槽位不能证明完整 chrome 闭包；应联合资产断言和原生首帧视觉/交互证据。
- 裁剪路径的等价性由生成器保证：`CandidateKeysCritical ∪ CandidateKeysDeferred` 与未分区的全量
  `CandidateKeys` 逐项一致。

### Stage 2 演化与失败语义

- **主题**：注册窗口（受控重开/再冻结）内完成 Deferred 块 → 冻结本地化输入 → 余量段收集与并入 →
  准备 registry、目录、根/局部快照、Semantic registry 和静态资源表。已有 provider/owner 保持稳定。
- **本地化**：增量准备 Catalog 行、全部支持语言 Snapshot 和定义，以匹配的 registry/revision 读取视图交换
  （manager/provider/localizer 与订阅者零身份变化）；未被覆盖的既有键值不变，新键即时可查，Stage 2 后切换
  语言不会退回 Stage 1 Snapshot。Stage 1 是中间态：应用级语言包
  bundle 可能先于其目标包的 catalog 到达（catalog 随包后置），未解析引用在 Stage 1 被容忍、在
  Stage 2 终态严格校验。
- **提交与失败**：`StageTwoEvolutionPlan` 先校验主题/作用域与语言版本，再无回调交换双方有效状态，最后统一发布。
  提交前失败后继续使用 Stage 1；一次性输入在成功、失败或取消后释放。详见[提交边界](staged-startup-evolution.md#3-capturepreparecommitcorepublish)。

### 已知约束

- 同步模式共用启动结果对象，`UseAtomUI()` 返回时任务已经完成，不引入后台初始化或调度。
- 分区包的余量资源按 provider 以 `(Phase, Order, AssetId)` 全局归并落地——合并字典查找自尾向头
  扫描，资源序不得按段拼接（否则既有资源首查被迫构造余量段全部字典，非分期路径也会变慢）。
- AOT：分期不引入反射/表达式树；`Deferred` 只存 `Action` 委托。发布门（Gallery staged +
  StartupPerformance 非 staged 双 AOT 冒烟）为常规验收项。

## ThemeManagerBuilder 收集内容

`ThemeManagerBuilder` 在构建前收集以下内容：

- `ControlTokenDescriptors`：每个对外可主题化 Control 的 exact CLR type、生成式 identity、可选 Own Token schema、
  强类型构造和资源投影；没有 Own Token 的 Control 也必须注册 descriptor。
- `ControlThemeAssetManifests`：生成式 AssetId/URI、导出的命名或默认主题、RequiredTokenOwners、Semantic Theme
  关联、资源优先级与构建期校验结果。internal 主题目标不被迫拥有 Token descriptor。
- `ControlThemesProviders`：AXAML 主题 Provider。
- `ThemeDefinitionResolvers`：内置资源、应用 `avares://` 资源和可选用户配置目录的统一主题来源解析器。
- `ThemeAlgorithmDescriptors`：`ThemeAlgorithm.Default`、`Dark`、`Compact` 的生成式 descriptor。
- `InitialThemeRequests`：固定主题或 FollowSystem 的 Light/Dark 不可变 root request 模板。
- `ModuleInitializers`：与 ThemeLoaded 等主题生命周期无关的模块初始化回调。

构建时会把这些内容注册到 `ThemeManager`。

## 控件包注册顺序

应用仍通过 `UseDesktopControls()`、`UseDesktopDataGrid()`、`UseDesktopColorPicker()` 等正常包入口启用能力。
入口调用生成的注册 helper，不声明方法名身份或 AOT 专用分支。统一顺序是：

1. 检查当前 builder 的重复或递归注册，在 prepare 前拒绝非法进入。
2. 执行 prepare；Desktop 明确先调用 Common，并保持 Dialog input capture 的初始化位置。
3. 创建平台 Provider，收集公共包资源和选中的逐控件片段。普通非裁剪模式使用全部片段；发布后端提供精细选择结果。
4. 只检查包内结构与重复身份，暂存 Token、Semantic descriptor 和资源 factory，分配 PackageCommitOrdinal。
5. 执行 complete，注册语言模块及主题 initializer；DataGrid/ColorPicker 仍在各自包提交后登记语言。
6. configure 完成后统一验证跨包 RequiredTokenOwners、语义和 schema，再按包提交次序和包内资源规则创建、挂载并冻结。

Common 与其他控件包使用同一个 TypeMap 模型。图片服务、codec、语言属于其明确的 Package Core；公共控件与主题不再因为
同处 Common 就被无条件完整注册。包可被 NuGet 引用和 bootstrap 发现，但不会因此执行 Provider 或 initializer。

NativeAOT/CoreCLR 使用官方 TypeMap；Browser 裁剪与 AOT 使用官方 ILLink 标记结果的编译期转换后端。
查询和代理去重只在启动收集阶段进行；不扫描程序集发现控件，不遍历依赖图，也不在控件实例化后追加注册。
普通首次主题初始化和运行时更新仍共用既有 ThemeManager 时序。完整契约见
[TypeMap 注册管线](aot-typemap-registration.md)和[控件与资源注册契约](control-registration-contracts.md)。

同一 builder 的第二次公共包入口报错，失败不能通过重试全量注册掩盖；不同 builder 的状态相互独立。
共享服务自身已定义的幂等/合并语义（例如 UseImageLoading）不因此改变。

## 图片加载注册

图片系统复用 `UseAtomUI()` 构建窗口，不引入进程静态初始化或 Control 首次加载时的延迟注册：

```csharp
this.UseAtomUI(builder =>
{
    builder.UseImageLoading(options =>
    {
        options.MaxConcurrentDownloads = 6;
        options.MaxConcurrentDecodes = 4;
    });
    builder.UseDesktopControls();
});
```

顺序与幂等规则固定为：

1. `UseImageLoading()` 在 Shared 的 Builder accumulator 中登记一个应用级 `ImageLoader` owned-service factory，并合并用户配置。
2. `UseCommonControls()` 调用同一 `UseImageLoading()` 默认入口，再显式注册 Controls 拥有的 trusted
   `avares` SVG codec；默认值不得覆盖用户显式值。
3. `UseDesktopControls()` 仍先调用 `UseCommonControls()`，不另建 Desktop loader；DataGrid、ColorPicker 和 Extras 也不重复注册。
4. configure callback 返回后，Core 冻结图片 options 与 codec registration；Shared pipeline 静态构造每个 source kind 的 reader，
   然后构建 Localization、Theme 和 owned services。
5. `ApplicationScope` 按注册顺序 attach owned services；`ImageLoader` attach 后才由
   `Application.GetImageLoader()` 可见。
6. 任一步失败按已成功 attach 的逆序回滚；`ApplicationScope` 销毁时按逆序 detach/dispose loader，再完成既有 Theme/
   Localization 销毁。

重复调用 `UseImageLoading()` 只合并同一个应用注册，不能产生多个 loader。source reader 按 kind 唯一，codec 按稳定 Id 和
Version 去重；冲突在启动阶段失败。`UseCommonControls()` 的 Package Core 通过真实静态调用保留
service factory、raster reader/codec 和 Asset SVG codec；其存在由明确服务契约决定，不依赖应用使用分析或程序集扫描。

Core 的 owned-service 机制是通用生命周期能力，不包含图片类型；完整设计见
[管线、并发与生命周期](../systems/image-loading/pipeline-and-lifecycle.md)。图片公共配置、控件 API 与平台限制见
[公共契约](../systems/image-loading/public-contracts.md)和
[平台、性能与 AOT](../systems/image-loading/platforms-and-aot.md)。

## 源生成事实

控件包通过普通 Generator 生成单项事实，而不是发布时重新分析应用：

- Control/Token descriptor 工厂、带真实 owner 的 `XxxTokens.Identity` 和强类型资源键。
- 单 Control 的 Semantic descriptor 工厂、专用 Semantic Style 与保留条件。
- 独立 ThemeAsset 描述和 deferred resource factory，包含导出主题、必要 Token owner 与稳定资源顺序。
- 逐控件片段、唯一条件 key 的 TypeMap、纯字符串候选表和 Package marker。
- `GeneratedLanguageModuleRegistration`：沿用本地化系统的静态 Catalog/Bundle 注册。

应用普通生成器只读取解析后的 Package marker，输出 TypeMapAssemblyTarget；不读取控件使用列表或方法体。
全量与选中路径调用同一单项工厂，普通模式的聚合集合不得意外进入裁剪分支。

带 owner 的 identity 在字符串相等去重前规范化：raw/typed 合并保留更强 owner，冲突 owner 立即报错，不能让 Dictionary
丢掉后输入的类型信息。资产、配置和注册表遵守同一规则。

包作者维护正常的 provider 和 prepare/complete 语义，生成器负责注册实现与后端选择。正常新增控件无需维护依赖图、控件名单
或额外 AOT 配置；内部主题、命名主题和显式资源 include 遵守[控件与资源注册契约](control-registration-contracts.md)。
