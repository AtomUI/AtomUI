# AtomUI AOT 编程规范

这份文档给日常写 AtomUI 代码的人用，定义新增控件、主题、图标、语言资源、Gallery 示例和发布配置时要遵守的规则。
控件注册采用 TypeMap 目标架构；架构采纳、源码迁移状态和删除旧实现的验收边界统一见
[AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md)。本文的注册规则是新实现必须满足的契约，不能据此
推断当前源码或完整 Gallery 已迁移。注册机制见 [TypeMap 注册架构](../../architecture/foundations/aot-typemap-registration.md)，
类型与资源规则见 [控件注册契约](../../architecture/foundations/control-registration-contracts.md)。

目标很简单：

- `src/` 里的库项目在 AOT、trim、single-file analyzer 下不产生项目自身 warning。
- Desktop NativeAOT、trimmed CoreCLR、Browser 裁剪运行和 Browser AOT 都必须通过真实发布、启动及精细裁剪验证。
- AOT 改造不能改变原来的控件行为、绑定语义、异常语义、资源释放边界和关键性能路径。

## 先看这几条

日常开发先记住这 7 条，绝大多数 AOT 问题都能在写代码时避开。

1. 新增功能和修复 bug 时，AOT 兼容是第一设计约束。同一需求有 AOT 友好实现和运行时反射/动态发现实现时，必须选择 AOT 友好实现；能用 source generator 就不要用反射。
2. 不要在 AtomUI 内置路径里新增字符串绑定，例如 `new Binding("Name")` 或 AXAML `ReflectionBinding`。
3. 不要运行时扫描 assembly、type、field、property 来完成内置注册。使用生成式 registry/catalog；TypeMap 注册只查询已知 key，并读取已选代理上的已知注册 Attribute，不扩大为程序集发现或动态方法搜索。
4. 不要用 `UnconditionalSuppressMessage` 掩盖未解决的 trim/AOT 问题。条件 TypeMap 声明和受控访问点只允许有理由、可验证的局部处理；suppression 本身不会保留 metadata，禁止全局关闭 IL2026/IL3050。
5. 替换 AOT 不安全代码时，先确认旧语义，再改实现。尤其是 binding mode、binding priority、初始值、异常包装、dispose 后行为。
6. 新增 subscription、binding、event handler、activation scope、cache 时，必须能说清楚在哪里释放或失效。
7. Analyzer 通过不等于发布产物可用。涉及发布配置、linker、root descriptor 时，要验证真实产物启动、控件行为和未用类型删除。

## 按场景查

| 场景 | 推荐写法 | 避免写法 | Review 重点 |
| --- | --- | --- | --- |
| 控件属性同步 | `AvaloniaProperty`、`GetObservable`、`BindUtils.RelayBind(...)` | `new Binding("Path")` | mode、priority、初始值、dispose 后行为 |
| 模板内 part 同步 | C# 里拿到 template part 后强类型绑定 | AXAML `ReflectionBinding` | template reapply 时旧 part 是否释放 |
| 控件注册 | 普通生成器输出片段与 TypeMap，官方链接器决定保留集合 | `Assembly.GetTypes()`、应用 usage 扫描、构造全集后过滤 | Token/语义/资源是否一致，未用类型是否删除 |
| token converter | generator 生成静态数组 | 运行时扫描 attribute 后 `Activator.CreateInstance` | 数量、顺序、map 行为是否不变 |
| 语言资源 | generated Catalog descriptor + compiled Translation Bundle | `GetFields(...)` 枚举资源字段 | Catalog ID、占位符和回退语义是否不变 |
| 图标创建 | generated factory 或 virtual factory | 扫描 icon assembly 后反射创建 | 非法 kind 的异常包装是否不变 |
| DataGrid 动态 path | `[GenerateDataMemberAccessors]` 或手写 descriptor | 对用户模型直接 `GetProperty(path)` | sort/filter/group/AddNew 是否走 descriptor |
| 非 Visual AvaloniaObject 资源宿主 | `[GenerateScopedResourceHost]` 生成 scoped host 生命周期 | 每个对象手写 `IResourceHost` / `IThemeVariantHost` 样板代码 | owner attach/release、WeakReference、资源更新测试 |
| ReactiveUI view activation | AtomUI/Gallery 自己管理 activation scope | view-side `WhenActivated` extension 反射路径 | Loaded/Unloaded 和 VM 切换释放 |
| 发布配置 | analyzer 加目标后端的真实发布与运行 | 只看普通 build | linker、浏览器转换、未用类型及构建工具部署边界 |

## 适用范围

这份规范覆盖以下项目：

- `src/AtomUI.Core`
- `src/AtomUI.Controls.Shared`
- `src/AtomUI.Controls`
- `src/AtomUI.Desktop.Controls`
- `src/AtomUI.Desktop.Controls.DataGrid`
- `src/AtomUI.Desktop.Controls.ColorPicker`
- `src/AtomUI.Desktop.Controls.Extras`
- `src/AtomUI.Icons.*`
- `src/AtomUI.Generator`
- `controlgallery/AtomUIGallery`
- `controlgallery/AtomUIGallery.Desktop`
- `controlgallery/AtomUIGallery.Browser`

## Binding

### 为什么字符串 binding 有风险

`new Binding("Name")`、`ReflectionBinding` 这类写法把成员访问延迟到运行时：运行时拿到字符串，再去找属性、字段或索引器。NativeAOT 和 trimming 会删除静态代码没有证明会用到的 metadata，所以这类路径很容易出现 analyzer warning，严重时运行时才失败。

AtomUI 内置控件和 Gallery AOT 路径应尽量把绑定改成编译期可见的属性访问。

### AvaloniaObject 之间同步属性

两个 Avalonia object 之间同步属性时，优先使用 `AvaloniaProperty`：

```csharp
disposables.Add(BindUtils.RelayBind(
    source,
    SourceControl.SomeProperty,
    target,
    TargetControl.SomeProperty,
    BindingMode.OneWay,
    BindingPriority.Template));
```

`BindUtils.RelayBind(...)` 的语义要求：

- `OneWay`：从 source 的 `GetObservable(sourceProperty)` 绑定到 target。
- `TwoWay`：保留 source-to-target 和 target-to-source 的双向同步，不能把 target 初始值错误写回 source。
- `OneWayToSource`：只把 target 后续变化写回 source，不能额外制造 source-to-target 行为。
- `OneTime`：也要保持旧 binding 的写入 priority 和释放语义，不能简单写成 `SetCurrentValue(...) + Disposable.Empty`，除非旧语义本来就是一次性写入且无需恢复。

不要再新增字符串 path overload。`BindUtils.RelayBind(object, string, ...)` 这类 API 应保持为兼容边界或编译期错误入口。

### 非 AvaloniaObject 数据源

对 `INotifyPropertyChanged` 类型，可以用属性名过滤通知，但取值必须走强类型 getter：

```csharp
disposables.Add(BindUtils.RelayBind(
    viewModel,
    nameof(MyViewModel.Options),
    static vm => vm.Options,
    optionsControl,
    SomeControl.OptionsProperty));
```

这里的 `nameof(...)` 只用于判断是哪一个属性变了，不允许再用反射按名字读取属性值。

### Template part binding

模板里不要新增 `ReflectionBinding`。template part 之间要同步时，在控件类里拿到 part 后绑定：

```csharp
_templateBindingDisposables?.Dispose();
_templateBindingDisposables = new CompositeDisposable();

if (partA is not null && partB is not null)
{
    partB.Bind(
             TargetProperty,
             partA.GetObservable(SourceProperty),
             BindingPriority.LocalValue)
         .DisposeWith(_templateBindingDisposables);
}
```

这里最容易出错的是生命周期：

- `OnApplyTemplate` 开始时先释放旧的 disposable。
- template reapply 后，旧 part 不能继续被 observable、event handler 或 binding 持有。
- 删除旧 binding 前，要确认旧 binding 是否会跟随动态 effective value 变化。不能因为初始默认值一样就删掉。

### Visual ancestor binding

需要找 visual ancestor 时，使用 `BindUtils.BindVisualAncestor(...)`：

```csharp
_relayBindingDisposables.Add(BindUtils.BindVisualAncestor(
    this,
    target,
    TargetControl.OpenOnProperty,
    typeof(TopLevel),
    priority: BindingPriority.Template));
```

注意：

- `ancestorLevel` 从 1 开始。
- 返回的 disposable 必须在 detach 或 template reapply 时释放。
- 不要用一次性的 `TopLevel.GetTopLevel(this)` 替代动态 ancestor binding，除非旧语义就是只查一次。

## Source Generator

### 什么时候该用 SG

如果代码需要做这些事，优先考虑 source generator：

- 扫描类型并注册。
- 根据 attribute 生成 registry。
- 根据 enum 创建具体类。
- 把资源字段写入 dictionary。
- 根据数据模型生成属性 accessor。
- 为 closed generic 或具体类型生成 factory。
- 为 owner-managed 非 Visual `AvaloniaObject` 生成 scoped `IResourceHost` / `IThemeVariantHost` 生命周期样板代码。

SG 的价值不是“把反射挪个地方”，而是让运行时代码变成普通的强类型 C#。这样 trimmer 能看见类型、构造函数和成员，NativeAOT 也不需要动态代码生成。

非 Visual `AvaloniaObject` 资源宿主类需求统一遵循 [Scoped Resource Host 开发规范](scoped-resource-host.md)。不要在每个描述对象中复制手写资源宿主代码；业务属性保留在主文件，资源宿主生命周期由 generator 生成，owner 控件只负责 attach/release。

### Generator 项目边界

`AtomUI.Generator` 是 Roslyn analyzer 项目，不是运行时依赖。引用方式保持：

```xml
<ProjectReference Include="../AtomUI.Generator/AtomUI.Generator.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false"
                  PrivateAssets="all" />
```

生成文件可以输出到 `GeneratedFiles/`，但项目要继续排除重复编译；这些编译产物默认不跟踪：

```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>GeneratedFiles</CompilerGeneratedFilesOutputPath>
<Compile Remove="$(CompilerGeneratedFilesOutputPath)/**/*.cs"/>
```

`PublishAot=true` 是 MSBuild global property，会沿着 `ProjectReference` 传播。generator 是 `netstandard2.0` analyzer 项目，不应该参与 NativeAOT 发布，所以 generator 项目要把这些属性隔离在本项目内：

```xml
TreatAsLocalProperty="IsAotCompatible;EnableAotAnalyzer;EnableTrimAnalyzer;EnableSingleFileAnalyzer;PublishAot;PublishTrimmed;PublishSingleFile;RunAOTCompilation;SelfContained;RuntimeIdentifier"
```

并且在 generator 项目里显式关闭运行时发布属性。

应用和第三方作者不需要为了 AOT 或 trimming 额外引用生成器或手配链接器参数。产品 SDK 随 NuGet 自动交付同版本
普通 Generator、Build Tasks、浏览器链接后端和 buildTransitive assets；Package identity 默认由正常包身份推导，
不携带注册方法名或裁剪粒度。显式 Generator 引用不能造成重复加载，最终 `@(Analyzer)` 中只能有一份 `AtomUI.Generator.dll`。

维护这条打包链时遵守以下约束：

- Generator、Build Tasks 和浏览器链接后端只能进入 NuGet 的编译期目录，不能进入 `lib/`、普通输出或 publish 目录。
- 多个产品包的入口必须幂等；Analyzer 去重依据 `ResolveReferences` 后的最终编译器输入。
- MSBuild 项目求值阶段的 `Import`、`ItemGroup` 或 item Condition 不得引用 item list；需要检查 `@(Analyzer)` 时放入
  `BeforeTargets="CoreCompile"` 的 Target。
- Release 打包必须先用相同 `AtomUIVersion` 构建 Generator/Build Tasks；修改版本后不能用 `--no-build` 复用旧输出。
- 验证至少覆盖单包、多包、显式 Generator 引用、普通构建、传递引用、预编译消费 DLL、NuGet 冷消费和全部发布后端。

### 生成物必须稳定

改 generator 时要同时看两层：

- writer 代码是不是符合预期。
- `GeneratedFiles/AtomUI.Generator/**` 里的生成物是不是能由当前 writer 稳定复现。

不能只手改生成物，也不能只改 generator 却不检查生成物 diff。之前 review 里已经出现过 generator writer 和提交的生成物不一致，这类问题会让后续维护很难判断真实来源。

包级生成代码统一位于 `AtomUI.Generated.<AssemblyOwner>`。`AssemblyOwner` 必须是由程序集名生成的单一 PascalCase
标识符，点号、连字符、下划线和其他非字母数字字符只作为单词边界，不进入最终标识符。例如：

```text
AtomUI.Controls                  -> AtomUI.Generated.AtomUIControls
AtomUI.Desktop.Controls         -> AtomUI.Generated.AtomUIDesktopControls
AtomUI.Desktop.Controls.DataGrid -> AtomUI.Generated.AtomUIDesktopControlsDataGrid
```

Source Generator、Localization writer、控件注册 metadata 和 AXAML Theme wrapper Build Task 必须复用同一
命名 helper，不得各自维护 sanitizer。手写代码引用生成入口时必须引用当前项目自己的 owner namespace；禁止依赖
`InternalsVisibleTo` 从其他 AtomUI 包调用同名 `Generated*` 类型，因为这种错误可能正常编译却注册错误 Package。

### TypeMap 控件注册

控件注册只有一个普通生成模型。可访问的 public/internal 非泛型 Control 拥有主题、Token 或 Semantic Part 契约时，
生成自己的注册片段；没有独立内容的派生类型不生成空片段。内部 presenter 可以只有资源片段，不伪造公开 Token identity。
目录位置和包大小不决定裁剪粒度；Common 控件也遵循这一模型。

普通生成器负责 Control/Token/Semantic descriptor、资源 factory、条件 TypeMap 和 Package marker。应用引导只读取已解析
程序集引用中的 Package marker，生成 `TypeMapAssemblyTarget<ConcretePackageGroup>`；不扫描用户 C#/AXAML usage，
不为预编译消费 DLL恢复调用图，不生成应用保留计划，也不沿 ProjectReference 传播额外的发布分析上下文。
详见 [控件注册生成器](../../modules/generator/control-registration.md)。

必须满足以下保留规则：

- 一个具体保留条件使用一个唯一字符串 key。多个条件可以指向同一代理，但不能复用同一 key 表达 OR。
- 正常 Control 使用、生成 TokenResource/Token key、Own Token enum、生成 identity 和专用 Semantic Style 的保留条件
  必须覆盖各自契约；不能假设 Token-only 或 Style-only 总会构造 owner 控件。
- 候选表只能包含字符串，不能包含所有 Control Type、代理实例或 factory delegate。运行时逐项 `TryGetValue`，
  按代理 Type 去重后再读取已知的 `ControlRegistrationFragmentAttribute`。
- 每包生成具体闭合的 TypeMap API 调用。共享 Runtime 接收已经取得的映射，不用开放泛型 `ReadMap<TGroup>()` 包装查询。
- 代理只收集自身 descriptor 和资产，不递归注册其他片段。真实类型/工厂引用交给官方 ILLink/ILC 求可达性闭包。
- 静态附加属性访问本身不保证保留 owner。需要 owner 的路径必须包含可观察且参与真实校验的数据引用，不能放置会被消除的
  空 `typeof`。动态创建沿用类型化工厂、DAM/DD 等官方保留机制，不引入字符串注册清单。
- 平台判断必须位于 descriptor、语义和资源工厂引用/构造之前，不能在构造全集后以运行时 predicate 过滤。

平台声明的约定是“控件能力放在控件类型，主题额外限制放在正常资源 CLR 类型”，不按目录推断或维护主题路径名单。
声明归属、可用域算法和迁移边界以[平台分支](../../architecture/foundations/aot-typemap-registration.md#7-平台分支)
及其状态链接为准。不要为移除资源配置而收窄公共控件的平台能力；已退役的自定义平台元数据不再读取。

TypeMap API、触发条件和异常边界见 [TypeMap 契约](../../reference/aot/typemap-contract.md)。它不保证枚举、任意运行时
字符串类型名自动保留或不同后端得到完全相同的最小集合。不得通过异常捕获、全包保留或 late registration 掩盖漏注册。

### Package 入口与冻结

`UseDesktopControls()`、`UseCommonControls()` 和第三方 `UseXxxControls()` 仍是普通包启用入口。Provider、服务、语言、
initializer 与真正公共资源属于 Package Core；入口通过正常 provider factory 和显式 prepare/complete 回调接入生成 helper。
不解析入口方法体猜测顺序，也不让 Package marker 自动启用包。

包入口执行 prepare、创建 Provider、收集公共资源和选中片段、检查包内身份、暂存注册，再执行 complete。跨包 Token owner、
Semantic Part 和 schema 校验，以及资源创建、排序、挂载和冻结，统一在所有配置入口返回后的 Build/Initialize 阶段完成。
首次控件实例化前必须完成冻结；主题切换、scoped Token 和 Style 构造不再追加注册。

公共重复入口在 prepare 前报错；递归注册同包也报错。内部基础包 Ensure 操作与公共重复 Use 区分，保留实际基础包调用顺序。
状态属于 builder 实例，失败后不能继续构建部分成功的应用；不得使用持有 builder/provider 的静态缓存。

资源按真实包提交顺序以及包内明确的 Phase/Order 排序。TypeMap 查询顺序、程序集遍历顺序和选中数量不能改变共同资产的
相对优先级。多个片段引用同一 AssetId 只挂载一次，冲突定义报错；deferred wrapper 必须保留。

### 浏览器编译后端

Browser 精细裁剪使用官方 ILLink 的 TypeMap 标记结果，再通过独立构建工具转换自有 map accessor；不在运行时扫描残留
Attribute，不修改 BCL，也不实现第二套应用依赖图。NativeAOT 使用官方 TypeMap 后端，不加载浏览器转换器。

转换必须位于 Mark 完成后、Sweep 前，只引用已标记的代理和 Mark 前已保留的 helpers。所有可达 accessor 必须转换完成，
且不再调用当前 Mono 后端不可执行的 TypeMapping API。无可达 accessor 的合法空应用与漏执行转换必须区别处理。

编译工具与 Roslyn Generator、运行时库物理隔离；随 SDK 自动接线。后端 DLL、依赖、配置和 ABI 纳入增量输入。
未验收工具链、不兼容 ABI、重复 key、缺 helper 或残留 accessor 都必须阻止发布。不得退回全包注册。
完整要求见 [浏览器链接后端](../../architecture/foundations/aot-browser-linking.md)。

### 第三方 Control Package 检查

普通第三方作者只维护正常产品契约：

1. 引用兼容的 AtomUI 产品 SDK，沿用自动交付的普通 Generator 和构建资产。
2. 提供 Control、可选 Own Token、Semantic Part 和正常编译型 AXAML；资源依赖通过词法作用域和显式 include 表达。
3. 以普通 `UseXxxControls()` 调用生成 registration helper，提供 Provider；有生命周期逻辑时明确传入回调。
4. 不手写 TypeMap、入口身份 Attribute、AOT 分支、控件名单、目录粒度或 linker XML。
5. 验证完整与选中注册共用的 descriptor、资源优先级和行为；发布兼容声明前验证实际目标平台。

操作示例见 [第三方 AtomUI Control Package 指南](../../guides/theming/third-party-control-packages.md)。预编译消费 DLL
无需使用清单，但其引用的控件包必须符合新生成契约；缺包或不兼容包必须明确失败，不能恢复旧扫描与全包兜底。

### 注册 ABI 与生成命名

跨程序集隐藏 ABI 使用中性的注册命名，如 `ControlRegistrationFragmentAttribute`、`ControlPackageRegistrationBuilder`
和 `GeneratedControlPackageRegistration`。公开性仅用于跨程序集生成代码访问，不成为用户手写注册扩展点。
Package Group 是无全包静态引用的隐藏公共类型；生成器内部和输出命名规则由
[控件注册生成器](../../modules/generator/control-registration.md)与 [TypeMap 契约](../../reference/aot/typemap-contract.md)统一维护。

完整与选中分支使用同一片段事实源，由链接器 feature switch 选择。不能依赖包编译时的 Debug/Release 常量，因为同一
NuGet 必须支持非裁剪运行和不同发布后端。模式选择、版本验证和产物检查由构建资产自动完成，用户不配置内部开关。

## Theme / Token

### Control 与 Token 注册

运行时不要扫描 assembly 查找可主题化 Control 或 Control Token。应由 generator 生成
独立的 descriptor factory，完整注册与选中片段调用同一 factory。拥有公开可配置 Token 契约的 Control 即使没有 Own Token，
也拥有 identity 和 descriptor；仅有主题资源的内部 Control 不需要 Token descriptor。不得通过全包静态 descriptor 池
间接保留所有控件。

descriptor 必须直接提供以下静态已知信息：

- exact Control CLR type、`ControlTokenIdentity` 和 registry slot。
- 无参数 `[ControlDesignToken]` 标记的可选 Own Token 类型；Attribute 不携带 Control 类型或 identity。
- 可选的 Own Token builder 直接构造委托。
- Own Token name、value type、stage 和 slot。
- 强类型 parse、set、get 和 resource projection 委托。
- `OwnTokens` schema；Control 的可配置 Global Token 集合始终是完整 Global Token schema，不生成消费白名单。
- 实际 owner Type 与身份验证信息；主题导出、RequiredTokenOwners、Semantic Part 和资产 factory 由对应片段一并收集。

Semantic Part descriptor 必须由 Control 声明和构建输入静态生成。运行时不得扫描 AXAML、ControlTheme、
`Classes` 或 VisualTree 来发现公共 Part，也不得使用 `Dictionary<string, Style>`、反射 Property 查找或动态 Theme
factory 合并 Part 样式。`.semantic-*` marker 只参与 Avalonia 原生 Selector；Gallery 的 VisualTree 高亮属于开发
工具路径，不能进入 Control 运行时。

注册还必须满足：

- Builder 必须原样传递 descriptor，不能丢弃 identity 后退化为 `Type` 注册。
- 内置正常路径不调用 `Activator.CreateInstance`、`Type.GetProperties` 或 `PropertyInfo.GetValue/SetValue`。
- 第三方 Control 包必须使用 AtomUI generator，并只通过一个真实的包级入口调用生成 registration helper，注册 Control、
  可选 Own Token 和主题资产；不提供手写 descriptor、手工 manifest 或反射 fallback 旁路。
- Own Token 可以放在包内正常源码位置并使用 `[ControlDesignToken]` 标记；禁止泛型 Control 参数和手写 ID。
- Control Token 定义继承只允许 Generator 在编译期把显式标记的抽象层扁平化到 `sealed` 终端。抽象层不生成
  identity、descriptor、注册片段或动态 root，运行时不扫描或遍历 Token 基类。完整契约见
  [Control Design Token 继承架构](../../architecture/systems/theming/control-design-token-inheritance.md)。

### Token value converter 注册

不要在运行时通过 `Assembly.GetTypes()`、`IsDefined(...)`、`Activator.CreateInstance(...)` 扫描 converter。应由 `TokenValueConverterRegistrationGenerator` 生成静态注册：

```csharp
ITokenValueConverter[] valueConverters =
[
    new StringTokenValueConverter(),
    new IntegerTokenValueConverter(),
    new DoubleTokenValueConverter()
];
```

改这里时要确认：

- converter 实例数量和旧逻辑一致。
- `TargetType()` 到 converter 的 map 行为不变。
- 新增 converter 后能在 `TokenValueConverterRegistry.g.cs` 里看到生成结果。

### Resource key cache

Resource key cache 应该基于显式 token metadata。不要在资源生成或 theme 切换的 hot path 里反射查找 token property。

cache 要满足：

- key 稳定。
- theme/token 更新时失效边界清楚。
- 不引入无上限增长的缓存。

## Language

### Catalog 与翻译表走生成代码

所有应用、控件和类库使用 `[LanguageCatalog]` enum 与 XLIFF 2.1。`LocalizationGenerator` 在编译期直接生成：

- enum 数字 ID 到 slot 的静态映射。
- `LanguageCatalogDescriptor` 和编译后 `TranslationBundleDescriptor`。
- 强类型 `XxxLangResourceExtension`。
- `GeneratedLanguageModuleRegistration` 和最终应用 bootstrap。

正常路径禁止通过 `Assembly.GetTypes()`、`GetFields()`、`Enum.GetNames()` 或 Attribute 反射发现 Catalog，也禁止
运行时解析 XLIFF。模块主包的 `en-US` 与静态语言包目标 XLIFF 只作为 `AdditionalFiles` 进入最终应用编译；运行时
只保留不可变 Snapshot 和字符串表。

Review 时要看：

- Catalog enum 是否公开、使用稳定显式正整数 ID，且不含别名或 `[Flags]`。
- 所有 Catalog 是否具有完整 `en-US`，目标语言的 unit/占位符契约是否一致。
- 类库包级入口是否直接调用生成的模块注册，不扫描程序集。
- 应用 bootstrap 是否只直接注册静态语言包和 Override Bundle。
- 发布目录是否没有 XLIFF、Build Tasks 或 Generator 程序集。

## Icon

### 图标创建走 generated factory

图标 provider 不要运行时扫描 icon assembly，也不要用 `Activator.CreateInstance(iconType)` 创建图标。生成器负责生成 `GetIconType(kind)` 和 `CreateIcon(kind)`。

`GetIcon(kind)` 的异常包装语义要保留：

```csharp
try
{
    return CreateIcon(kind);
}
catch (Exception ex)
{
    throw new InvalidOperationException($"Create icon {kind} failed", ex);
}
```

这不是细节。非法 kind、构造失败、内部异常的外层异常类型和 message 都属于对外行为。

### Gallery icon catalog

Gallery 展示图标时使用 generated catalog：

```csharp
AntDesignIconCatalog.GetIcons()
```

catalog entry 应包含：

- `Name`
- `ThemeType`
- `Kind`
- `IconType`
- direct `Func<Icon>` creator

Gallery 不要运行时扫描 icon assembly。

### 同类型 icon clone

需要创建同类型 icon 时，不要通过反射构造。使用虚方法：

```csharp
public override Icon CreateInstance()
{
    return new SameIconType();
}
```

要求：

- 每个生成图标 override 后返回同一具体类型。
- base `Icon.CreateInstance()` 默认失败，避免静默创建错误类型。
- clone 后只复制旧逻辑复制过的属性，不能额外覆盖 brush、animation、stroke 等状态。

## DataGrid / Collection 动态数据

### 问题本质

DataGrid、collection sort/filter/group、自动生成列这些场景经常拿到用户数据模型和字符串 property path。这里的动态性是功能需求，不能简单把 warning suppress 掉。

正确方向是：对我们能控制的模型生成 accessor；对用户模型提供显式 descriptor；最后才保留带 warning 的兼容 fallback。

### 推荐写法

可控制的数据模型加 `[GenerateDataMemberAccessors]`：

```csharp
[GenerateDataMemberAccessors]
public partial class PersonRow
{
    public string? Name { get; set; }
    public int Age { get; set; }
}
```

如果集合以接口或基类作为 item type 暴露，并且排序、过滤或分组 path 来自这个接口/基类，也要在对应接口或基类上生成 accessor；不要依赖运行时从首个 item 反推具体类型。

不可加 attribute 的模型，显式传入 `IDataMemberAccessorDescriptor`：

```csharp
var descriptor = new DataMemberAccessorDescriptor<PersonRow>(
    static () => new PersonRow(),
    new IDataMemberAccessor[]
    {
        new DataMemberAccessor<PersonRow, string?>("Name", static row => row.Name),
        new DataMemberAccessor<PersonRow, int>("Age", static row => row.Age)
    });
```

相关逻辑优先从 descriptor 取信息：

- sort
- filter
- group
- AddNew
- auto-generate columns
- data type inference
- read-only metadata

如果 descriptor 不存在，可以进入 RUC fallback，但调用点必须显式看到风险。`RequiresUnreferencedCode` 不会保留成员，它只会把风险传递给调用方，让 analyzer 在 AOT/trim 场景下报警。

所以结论是：AtomUI 内置模型要走生成 accessor；用户如果要 NativeAOT 稳定发布，就要提供 generated 或手写 descriptor。

### 编译期诊断

DataGrid、List、collection view 等使用字符串 path 做排序、过滤、分组、自动列或数据成员读取时，必须优先让问题在编译期暴露，而不是等到 NativeAOT 运行时才失败。

可静态判断的场景必须提供 analyzer warning。诊断 ID、ID 命名、severity、编码组织和测试规则统一维护在 [compiler-diagnostics-guidelines.md](compiler-diagnostics-guidelines.md)。

## Reflection helper

反射 helper 只能存在于明确边界；控件注册另外允许读取已选 TypeMap 代理上的已知注册 Attribute，不扩展为任意类型发现：

- public compatibility API。
- 用户动态模型 fallback。
- 平台或框架能力探测。

保留反射时要写清楚风险：

- 需要成员 metadata 的 `Type` 参数或返回值，要用 `DynamicallyAccessedMembers` 标注。
- 无法证明 trim 安全的 API，要用 `RequiresUnreferencedCode` 标注。
- helper 内不要动态构造泛型类型。优先扫描对象已经实现的 closed interface。
- 不要用 `UnconditionalSuppressMessage` 让调用方误以为代码安全。

可以这样理解：

- DAM 用来告诉 trimmer：“这个 Type 值流到这里时，需要保留哪些成员。”
- RUC 用来告诉调用者：“这个 API 在 trim/AOT 下不保证安全。”
- suppression 只是不显示 warning，不解决问题。

## ReactiveUI

### AtomUI runtime controls

AtomUI runtime 控件不能依赖 ReactiveUI view-side reflection activation path。`ReactiveWindow<TViewModel>` 这类基类要自己完成关键行为：

- 实现 `IViewFor<TViewModel>`。
- 维护 `ViewModel` 和 `DataContext` 同步。
- `Loaded` 时激活当前 `IActivatableViewModel`。
- `Unloaded` 或切换 `ViewModel` 时释放旧 activation disposable。
- 处理 reentrancy，避免 unload 过程中误激活新 VM。

构造函数里空的 `this.WhenActivated(...)` 没有业务价值，应删除。非空 activation block 要迁移到 AOT 友好的本地 activation API。

### Gallery controls

Gallery AOT 路径使用 `GalleryReactiveUserControl<TViewModel>` 替代 `ReactiveUserControl<TViewModel>`。

它要保留 ReactiveUI 的核心使用体验：

- `IViewFor<TViewModel>`。
- `ViewModel` 和 `DataContext` 双向同步。
- Loaded/Unloaded 对应 view activation scope。
- `IActivatableViewModel.Activator.Activate()`。
- `WhenActivated(Action<CompositeDisposable>)` 的注册、激活、卸载释放语义。

也就是说，我们不是放弃 ReactiveUI，而是避开它 view-side extension API 中对 AOT 不友好的表达式/反射路径。

### Gallery binding / command

Gallery code-behind 不要新增 ReactiveUI expression binding：

```csharp
this.OneWayBind(...);
this.BindCommand(...);
this.WhenAnyValue(...);
observable.ToProperty(...);
```

替代写法：

- `GalleryBindingUtils.OneWay(...)`
- `GalleryBindingUtils.BindCommand(...)`
- setter 内显式更新派生属性
- 显式 `BehaviorSubject<T>` 或 `IObservable<T>` 管理 command canExecute

`GalleryBindingUtils.BindCommand` dispose 时要恢复绑定前 command，避免 activation scope 结束后留下旧 command。

## Publish / trimming / NativeAOT

### 日常构建不运行发布分析器

普通 `Debug` 和非 AOT、非裁剪的 `Release` 构建必须关闭 SDK 的 trim、AOT 和 single-file analyzer，避免把发布期静态分析成本带入日常开发。Gallery Desktop 的 Release 项目只保留 `IsTrimmable=true` 和 `IsAotCompatible=true` 兼容性声明；`PublishTrimmed`、`PublishAot` 和自包含发布属性由发布脚本显式传入，不能隐式改变普通构建的 analyzer 开关。

仓库构建按以下输入自动启用对应 analyzer：

- `PublishTrimmed=true`：启用 trim analyzer。
- `PublishAot=true` 或 `RunAOTCompilation=true`：启用 trim 和 AOT analyzer。
- `PublishSingleFile=true`：启用 single-file analyzer。
- 显式传入 `EnableTrimAnalyzer`、`EnableAotAnalyzer` 或 `EnableSingleFileAnalyzer` 时，保留调用方选择，用于专项验证。

普通 Generator 在所有构建中输出同一套 Theme/Token/Semantic/资源片段、TypeMap 和 Package marker。非裁剪运行直接使用
完整片段；裁剪发布选择映射，Browser 再执行编译期转换。不能加载旧 usage analyzer 或产生 Sidecar/应用计划；这些历史
输出是否仍存在于迁移中的源码，由 [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md)的状态边界说明。

### Analyzer 和真实 publish 都要跑

AOT/trim analyzer 通过，只说明静态分析没有发现项目自身 warning。它不等于 trimmed JIT 或 NativeAOT 链接和运行一定成功。涉及控件片段、资源依赖、TypeMap、浏览器后端、发布配置或 native 依赖时，要做对应模式的真实 publish。

Windows 11 上 Gallery Desktop 的 NativeAOT 工具链、发布命令、产物验证和排障记录见 [Windows NativeAOT 发布](../platforms/windows-native-aot-publish.md)。Linux 平台的对应手册见 [Linux NativeAOT 发布](../platforms/linux-native-aot-publish.md)。

库项目 analyzer：

```bash
dotnet build src/AtomUI.Core/AtomUI.Core.csproj -c Release --no-incremental \
  /p:IsAotCompatible=true \
  /p:EnableTrimAnalyzer=true \
  /p:EnableAotAnalyzer=true \
  /p:EnableSingleFileAnalyzer=true \
  /nr:false --nologo -v:minimal
```

Gallery analyzer：

```bash
dotnet build controlgallery/AtomUIGallery/AtomUIGallery.csproj -c Release --no-incremental /m:1 \
  /p:IsAotCompatible=true \
  /p:EnableTrimAnalyzer=true \
  /p:EnableAotAnalyzer=true \
  /p:EnableSingleFileAnalyzer=true \
  /nr:false --nologo -v:minimal
```

Gallery NativeAOT publish：

```bash
dotnet publish controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj \
  -c Release -r osx-arm64 -p:GalleryPublishTrimmed=true \
  -p:GalleryPublishAot=true --self-contained true \
  --nologo -v:minimal
```

注册迁移和体积验收以 [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md)的目标矩阵为准。迁移中的旧验证
脚本不能仅因退出成功就被视为已覆盖 TypeMap 或 Browser 后端。专项脚本成功仍不能代替真实宿主启动 smoke。Theme template 可以静态保留 CLR 类型而漏注册它的 descriptor；这类错误
只有窗口模板应用和首帧布局实际运行时才会暴露。发布后至少确认进程稳定进入主窗口，无 active theme schema、资源加载或
initializer 异常，再主动终止 smoke 进程。

### macOS NativeAOT

当前 macOS 本机发布使用：

```xml
<ItemGroup Condition="'$(PublishAot)' == 'true' and $([MSBuild]::IsOSPlatform('OSX'))">
    <LinkerArg Include="-L/opt/homebrew/lib"
               Condition="Exists('/opt/homebrew/lib/libbrotlienc.dylib')"/>
    <LinkerArg Include="-L/opt/homebrew/opt/openssl@3/lib"
               Condition="Exists('/opt/homebrew/opt/openssl@3/lib/libssl.dylib')"/>
    <LinkerArg Include="-L/usr/local/lib"
               Condition="Exists('/usr/local/lib/libbrotlienc.dylib')"/>
    <LinkerArg Include="-L/usr/local/opt/openssl@3/lib"
               Condition="Exists('/usr/local/opt/openssl@3/lib/libssl.dylib')"/>
</ItemGroup>
```

原因：

- NativeAOT 链接 `System.Net.Security.Native` 时需要 `libssl` / `libcrypto`。
- NativeAOT 链接 `System.IO.Compression.Native` 时还需要 Brotli 原生库。
- Apple Silicon Homebrew 的通用库目录通常是 `/opt/homebrew/lib`，`openssl@3` keg-only 库位于
  `/opt/homebrew/opt/openssl@3/lib`；Intel Homebrew 对应 `/usr/local` 路径。默认 linker 搜索路径可能找不到这些库。
- 仓库内 macOS 验证统一复用 `build/MacOSHomebrewNativeAot.targets`，不要在 Gallery 或 fixture 中重复硬编码路径。
- 该文件只补充 Homebrew OpenSSL/Brotli linker 搜索路径，不进入 NuGet，也不是 AtomUI 的通用 NativeAOT 配置。
  Windows 和 Linux 使用共享 AOT 配置及各自平台工具链，不需要空的对称 targets 文件。

注意：

- 默认不要添加 `-ld_classic`。删除粗粒度 root 后，当前 NativeAOT 产物已经可以用默认 Apple linker 完成链接。
- 如果某个 Xcode/NativeAOT 组合再次触发 `too many large addends`，先验证是否是环境问题，再作为本机或 CI 发布参数处理，不要默认固化到项目文件里。
- 如果 CI 或开发机不是 Homebrew ARM64 路径，需要在发布脚本里提供正确的 linker search path。
- NativeAOT publish 输出的是裸 Mach-O 可执行文件。macOS 图形应用要双击或通过 LaunchServices 启动，需要 `.app` bundle。

### Roots.xml

`Roots.xml` 不能保留解析不到的 assembly：

```xml
<assembly fullname="Some.Old.Assembly" preserve="All"/>
```

无效 root 只会产生 IL2007 warning，不会保留任何东西。删除 stale root 不改变运行时行为。

新增 root 前要说明：

- 为什么不能通过静态引用或 generator 保留。
- preserve 范围为什么不能更小。
- 是否会明显扩大 NativeAOT 体积。

控件动态边界优先使用真实保存并校验的 Control Type、类型化工厂以及匹配的 DAM/DD 注解。字符串 identity 仅匹配已保留
schema，不是动态 root。禁止使用整包 `preserve="All"` 修补注册或浏览器转换缺陷；正常控件接入不需要 root XML。

### Browser WebAssembly 发布

Browser 发布必须同时具备裁剪后的 interpreter 与 `RunAOTCompilation=true` 两条真实运行证据。两条路径都使用同一
逐控件注册模型，不能以普通发布能运行、原型验证通过或关闭 AOT 替代产品验收。

发布顺序固定为普通编译、ILLink 标记与自有 accessor 转换、可选 Mono AOT、bundling。验证工具链组合包括 SDK、ILLink、
WASM workload/runtime pack、浏览器和 UI 宿主；升级其中任一相关组件后，重新确认适配器 ABI、转换与启动行为。
具体后端要求见 [浏览器链接后端](../../architecture/foundations/aot-browser-linking.md)。

排障时区分：

- `dotnet.create()` / runtime 初始化阶段失败：先用最小纯 .NET Browser 程序判断工具链、打包格式和浏览器问题。
- managed `Main` 已进入后失败：检查映射转换、包入口、资源加载与实际 Avalonia 模板。
- 最小程序通过后仍须验证最小 Avalonia 宿主，再验证真实 AtomUI/Gallery；前一级成功不能替代后一级。

`WasmEnableWebcil` 等打包选项按实际验证结果管理，不作为 TypeMap 注册的语义开关。若采用环境修复，保持其他变量一致
验证启动并记录适用条件；不能通过关闭裁剪、全包保留或跳过实际 UI 来隐藏问题。

#### Browser 裁剪与 AOT 验收门槛

每个纳入支持矩阵的 Browser 产品宿主必须证明：

1. Release 裁剪发布在浏览器进入 managed 入口、完成注册冻结，并显示真实控件模板与首帧。
2. 同宿主 `RunAOTCompilation=true` 实际执行 AOT 编译，最终浏览器产物完成相同启动和控件行为验证。
3. 直接使用、Token-only、Semantic Style-only 和间接模板依赖正常注册；未用控件、代理及专属资源确实未被保留。
4. 最终自有 accessor 已转换，不残留不可执行的 TypeMapping 调用；缺失后端或 ABI 不匹配必须是失败对照。
5. 主题切换、scoped Token、平台过滤、资源优先级和未启用可选包行为与目标契约一致。

历史 Gallery 环境曾在 runtime 初始化阶段出现 `remainder by zero`。这个环境失败不构成永久豁免，也不授权关闭 Browser
AOT 验收。独立 TypeMap 原型的 Browser 裁剪与 AOT 成功证明机制可行，不证明完整 Gallery 已迁移；当前产品状态只从
[AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md)读取。未满足上述门槛时报告尚未完成的产品验证，不宣称
浏览器发布支持完成，也不退回旧管线或全包注册。

## Review 时看什么

每个 AOT 改动 review 时，至少回答这些问题：

- 旧实现依赖了哪种 AOT 不安全机制。
- 新实现如何消除这个机制。
- 旧语义是否保持，包括 binding priority、初始值、异常包装、排序、过滤、缓存顺序等。
- 新增 disposable、event handler、binding、activation scope 在哪里释放。
- 是否引入额外 per-instance 成本或 hot path 成本。
- generator 和生成物是否一致。
- analyzer 和必要测试是否通过。
- 是否完成受影响后端的真实发布、启动、精细裁剪与失败对照。

Review 记录应列出实际验证的后端、输入、产物和未完成门槛。正式架构与产品状态以
[AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md)为准，不依赖临时计划或本地实验目录。

## 新增代码自查

新增或修改 AtomUI 代码时，先扫一遍：

- 是否新增 `new Binding("...")`、AXAML `ReflectionBinding` 或字符串 path binding。
- 是否新增 assembly/type/member 扫描。
- 是否新增 `Activator.CreateInstance(Type)`、`Expression.Compile()` 或 `MakeGenericType(...)`。
- 是否新增 ReactiveUI expression/view activation API。
- 是否新增动态 data model path，但没有 descriptor 或 generator。
- 是否新增 DataGrid/List/collection 字符串 path，但没有可在编译期报警的 analyzer 覆盖。
- 是否新增订阅、binding、event handler，但没有 release path。
- 是否新增 source generator 逻辑，但没有检查生成物稳定性。
- 是否新增 suppress trim/AOT warning。
- 是否修改 publish/linker/root descriptor，但没有真实 publish 验证。

建议 grep：

```bash
rg -n "new Binding\\(|ReflectionBinding|Assembly\\.GetTypes|GetFields\\(|GetCustomAttribute|Activator\\.CreateInstance|Expression\\.Compile|MakeGenericType|WhenAnyValue|ToProperty|OneWayBind|BindCommand|ReactiveUserControl" src controlgallery -g '*.cs' -g '*.axaml'
```

grep 命中不一定都是错误，但每个命中都要能说明边界和原因。

## 推荐验证命令

日常验证先按 [按改动影响选择验证](../workflows/affected-verification.md)执行 plan 与对应 scope。以下全解决方案命令
仅用于明确需要全域构建或专项 analyzer 检查的场景，不能替代真实产品发布验证。

全域源码构建：

```bash
dotnet build AtomUI.slnx -c Release --no-incremental /m:1 /nr:false --nologo -v:minimal
```

全解决方案 AOT analyzer：

```bash
dotnet build AtomUI.slnx -c Release --no-incremental /m:1 \
  /p:IsAotCompatible=true \
  /p:EnableTrimAnalyzer=true \
  /p:EnableAotAnalyzer=true \
  /p:EnableSingleFileAnalyzer=true \
  /nr:false --nologo -v:minimal
```

Gallery NativeAOT publish：

```bash
dotnet publish controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj \
  -c Release -r osx-arm64 -p:GalleryPublishTrimmed=true \
  -p:GalleryPublishAot=true --self-contained true \
  --nologo -v:minimal
```

Focused tests：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --nologo -v:minimal /m:1 /nr:false
dotnet test tests/AtomUI.Controls.Shared.Tests/AtomUI.Controls.Shared.Tests.csproj --nologo -v:minimal /m:1 /nr:false
dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --nologo -v:minimal /m:1 /nr:false
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --nologo -v:minimal /m:1 /nr:false
```

Diff hygiene：

```bash
git diff --check
```

## 已知受控边界

下面这些命中不等于必须删除，但必须维持注解和文档边界：

- `TypeHelper` 动态 path fallback。
- `ObjectExtension` / `TypeMemberExtension` 反射 helper。
- DataGrid 对用户 `Binding` / `ReflectionBinding` 的兼容读取。

共同要求是：AtomUI 内置正常路径不用这些 fallback；用户动态场景使用时风险要显式暴露；AOT 用户要有 descriptor、generator 或显式注册这样的稳定替代路径。
