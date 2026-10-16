# 控件包启动接入

控件包通过 `IAtomUIBuilder` 扩展方法显式启用。包可以以源码、ProjectReference 或预编译 NuGet 提供；应用生成器
读取编译器引用中的包清单，把直接和传递引用的声明链接为纯数据启动镜像。引用包不等于启用包。

## 包入口

包项目沿用普通 AtomUI Control、Token、Semantic Part 和 AXAML 声明。当前版本的生成器自动生成
`AtomUI.Generated.<AssemblyOwner>.GeneratedControlPackageRegistration.Definition`；空包和只有资产的包也有清单。
不要手写清单、TypeMap key、schema 指纹或代理类型。

以下示例假定包程序集名为 `Acme.Charts`：

```csharp
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Generated.AcmeCharts;
using AtomUI.Registration;

public static class AcmeChartsBuilderExtensions
{
    public static IAtomUIBuilder UseAcmeCharts(
        this IAtomUIBuilder builder,
        Action<ControlPackageOptions>? configure = null)
    {
        return GeneratedControlPackageRegistration.Definition.Register(builder, package =>
        {
            package.DependsOn(static dependency => dependency.UseDesktopControls());
            package.OnCommit(static context =>
            {
                // UI 线程；Application 已创建。仅安排启动必须完成的轻量操作。
            });
            package.OnFirstUse(static context =>
            {
                // 本 Application 第一次实际使用本包时执行，例如初始化包服务。
                // 可释放服务使用 context.Own(service) 交给应用生命周期管理。
            });
            configure?.Invoke(package);
        });
    }
}
```

`UseAcmeCharts()` 在 AppBuilder 配置期只记录声明、依赖和钩子。配置回调不得访问 Avalonia 平台对象或
`Application.Current`，也不应加载主题、创建控件或启动包服务。`DependsOn` 自动启用依赖；重复的依赖声明幂等，
循环依赖被拒绝。应用需要重复确保启用时可由包入口使用 `Definition.Ensure`，不要自行维护进程级启用状态。

应用自有服务的 Factory 只构造服务，不在应用 scope 挂接前消费包控件、语义描述或资源；过早消费会明确失败，
不会跳过 `OnFirstUse`。服务 Attach、应用 initializer 和包 `OnCommit` 中的实际消费会先确保目标包的
`OnCommit` 完成，再执行其 `OnFirstUse`，然后把结果交给调用者。跨包钩子调用有确定顺序；
同包重入或跨包提交循环会拒绝并回滚，不能依赖循环初始化顺序。

包自己的生成式语言模块随 `Definition` 声明。不要在 `OnCommit` 再注册同一个语言模块，也不要在新入口创建旧的
`IControlThemesProvider`。资产顺序和资源键由生成清单提供，Token、主题资产和语义描述仍按首次使用物化。

## 应用入口

```csharp
AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .UseAtomUI(builder =>
    {
        builder.UseLanguages(LanguageTags.EnUS, [LanguageTags.EnUS, LanguageTags.ZhCN]);
        builder.UseAcmeCharts();
    });

public partial class App : Application
{
}
```

顶层、非泛型 `partial Application` 自动获得 `RegisterServices` 挂接与 `IGeneratedStartupImageHost`。
已有手写 `RegisterServices` 时保留 `base.RegisterServices()`，随后调用 `AtomUIStartup.Attach(this)`；生成器仍提供
镜像接口。缺少 `Attach` 是编译错误。

无法使用这种类形态时，保留手动 `Attach`，并在 `UseAtomUI` 配置中调用
`builder.UseStartupImage(AtomUI.Generated.<AssemblyOwner>.GeneratedApplicationStartupImage.Instance)`。
生成器会报告 `ATOMUIREG015` 提醒显式接线；镜像类型属于应用自身程序集。不要从依赖包获取另一应用的镜像。

## 构建期检查

镜像只包含字符串、数值、TypeMap key、资源归属和语言证明数据，不直接引用控件 CLR 类型，也不改变实际裁剪选择。
已引用但未启用包的资源或控件首次被查询时，运行时可根据归属报告缺少包入口。

应用链接检查包/控件/资源身份、Token owner、语义 owner、主题需求和语言目录。编译主题必须匹配应用实际引用的
owner 程序集简单名称、metadata type、Token 值类型、资源键、阶段和静态字面量能力；程序集版本号单独变化不会
被当作契约变化。二进制包缺少清单、格式版本不兼容或数据损坏时，必须用当前生成器重新构建包。

语言证明以实际编码的模块数据为依据。生成器识别真实 `UseAtomUI` 回调或接收 `IAtomUIBuilder` 的配置方法内，
直接写出的 `LanguageTags.*` 和 `LanguageTag.Parse("常量")`，以及它们组成的内联数组/集合。声明的每种非英语语言
必须在最终 `en-US` 回退之前获得全部目录项；例如 `zh-CN` 可以使用 `zh-Hans`，但不能只靠英语补缺。
格式化消息按运行时 `CompositeFormat` 语法验证，并保持英语源文本的占位符索引集合。无论应用当前默认语言是什么，每个目录都必须有完整的英语回退。

从变量、任意方法、外部配置或其他动态表达式取得的语言不会被伪装成静态证明。运行时只有在启用的语言模块集合及
其数据与镜像完全一致、全部支持语言都在证明集合内时才省略重复校验；手工目录、动态语言和包追加继续校验。
诊断 ID 和修复规则见[编译期诊断规范](../../engineering/development/compiler-diagnostics-guidelines.md)。

## 运行时追加

初始提交完成后，在 UI 线程通过当前应用的唯一运行时入口追加包：

```csharp
application.GetAtomUI().LoadPackages(builder => builder.UseAcmeCharts());
```

同一个包入口可用于初始配置和追加；包无需出现在初始镜像中。追加只接受新的包、依赖、包服务与语言数据。
初始主题、字体、支持语言集合、格式文化、主窗体与 Splash 配置不能在此修改。直接重复注册同名包报错；
`DependsOn` 与 `Ensure` 跳过已启用包，仅确保已有包时不会发布新快照或通知。

框架先完成跨包类型、资源、语义与全部支持语言校验，再暂存资源和主题视图，执行新服务 attach 与新包 `OnCommit`。
钩子在 UI 线程可读取暂存资源、主题与语言；后台只读主题状态、语义元数据和 localizer 保持已提交视图。主题、作用域与语言通过同一发布标记切换。失败保留原 registry、slot、revision、值和资源表，
并逆序释放本次新建的服务与 `context.Own(...)` 对象。钩子中的外部订阅或服务必须交给 `Own`；任意外部业务副作用
需要包自身提供对应释放行为。

旧资产的整个查找过程（包括已加载字典中尚未求值的 `AddDeferred` 项）使用已提交资源、主题和语言环境，因此失败追加不会把候选对象写进旧缓存。成功追加保留已有 Control slot、已物化 cell 和全局 Token/资源对象，资源根只发布一次变更。资源工厂仍按首次查询执行，
失败后可以重试；`OnFirstUse` 每个应用每包执行一次，命令式 Own/EffectiveGlobal Token 消费、公开语义 lookup，
以及 `SemanticParts.Controls` 的索引/遍历都会经过此门。仅获取列表 `Count`、纯全局值读取和框架内部不消费描述的
元数据/schema 校验不会触发。钩子失败会缓存其异常。资源通知订阅者抛错不会撤回已成功的提交。
应用销毁后，已取得的运行时入口不能再次追加。初始提交钩子、另一次追加或正在运行的主题变更/重载期间调用会被拒绝；
追加钩子也不能同时变更主题、语言、创建主题作用域或销毁当前应用 scope。

## 启动清单版本

Control package 清单与 `StartupImage` 当前使用版本 **2**。这与条件注册、编译主题和语言数据各自的 ABI 版本无关。
清单保留 Control 的定义程序集/类型、Own Token 键自身的程序集、Token owner 的精确类型，以及语义主题属性和目标类型。
资产编译时的全局资源键 schema 也参与校验；平台适用性由生成的资产谓词按原有 OS 与最低版本 guard 执行。
运行时会在动态链接和资源索引之前排除不适用资产，控件自身的可用性保持独立。

版本 1 的启动清单或镜像不能静默升级；必须用匹配的 Generator/Core 重建包与应用。镜像仍只有纯数据，不创建额外
控件 CLR 类型保留根。

## 包作者验收

发布前用独立消费应用验证真实二进制包，而不只验证同一个源码解决方案：普通 build/pack、仅 NuGet 引用、传递依赖、
只有资产/空包、主题和语言覆盖都应可编译。检查生成镜像没有 `typeof(控件)` 根引用，再按目标后端执行真实裁剪发布；
普通编译通过不代表 NativeAOT 或 Browser 发布已经验收。

运行时追加遵循上述同一包入口与生命周期；包作者不得把应用对象留在进程级静态字段中。

相关边界见[启动系统](../../architecture/systems/startup/overview.md)、
[控件注册契约](../../architecture/foundations/control-registration-contracts.md)与
[AOT 编程规范](../../engineering/development/aot-programming-guidelines.md)。
