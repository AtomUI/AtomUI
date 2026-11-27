# 方案一：使用 .NET TypeMap 自动选择控件注册片段

> 过程记录：本轮结论已同步到 [正式 AOT 架构](../../architecture/foundations/aot-and-trimming.md)。本文保留方案比较、实验或迁移过程，不再作为平行的长期规范；事实以正式文档为准。


> 2026-09-27，待评审设计。本文不是现行架构契约，也不表示 AtomUI 已完成迁移。
> 目标：保持 `UseDesktopControls()`，自动按使用范围裁剪，注册仍在首帧前完成。
> 用户已选择深入探索本方案，排除显式控件族选择方案。
> 后续全局复评已形成 [完整替换设计](2026-09-27-aot-typemap-replacement-design.md)，包括移除目录 Unit 与浏览器链接后端；本文保留为前期探索记录。
> 已在隔离工作区运行真实 AtomUI NativeAOT 原型；本方案仍未成为产品实现。
> 深入验证、已发现的失败及平台硬边界见 [TypeMap 深入验证](2026-09-27-aot-typemap-validation.md)。

## 1. 决策

在 .NET 10 路径使用官方 `TypeMapAttribute<TGroup>` 表达：

```text
Control 被最终链接器认为可达
    → 对应注册代理被保留
    → 注册代理引用 descriptor、Token、编译后的 AXAML factory
    → AXAML 或 C# 引用另一个 Control
    → 另一个 Control 的注册代理被保留
```

依赖闭包由 ILLink / ILC 自己计算。AtomUI 不再分析应用 C#/AXAML usage，不生成应用 Sidecar 或 Application Plan。
运行时只查询已确定的映射、收集片段和提交注册，不遍历依赖图。

这项设计使用的是 .NET 10 已提供的 API，不依赖修改 ILC，也不需要重复运行 linker。

## 2. 保留与调整的约束

保留：一行入口、Package Core 执行顺序、首帧前冻结、控件族粒度、静态资源工厂、平台过滤、第三方默认整包粒度。

明确调整现行约束：允许启动阶段执行有限次 TypeMap 查询，以及读取已知映射目标上的一个注册代理 Attribute。
这不是零运行时成本，也不能称为完全无反射；它没有程序集发现、动态方法查找或依赖分析。
普通 Debug 可继续使用直接 full registrar，避免 CoreCLR 非裁剪 TypeMap 的程序集 Attribute 处理开销。

.NET 8 没有 TypeMap。若 .NET 8 也必须保持无参数入口的精细自动裁剪，旧管线暂时不能删除；这是迁移决策，不能用 polyfill 掩盖。

## 3. 生成物

以下为拟议输出形态，名称不作为已存在的 Public API。

```csharp
// 由 ordinary generator 输出；应用和控件作者不手写。
#pragma warning disable IL2026 // 条目按 trimTarget 缺失是预期；查询端处理缺失。
[assembly: TypeMap<DesktopThemeMap>(
    "AtomUI.Desktop.Controls/Button",
    typeof(ButtonRegistrationAttribute),
    typeof(Button))]
#pragma warning restore IL2026

[ButtonRegistration]
internal sealed class ButtonRegistrationAttribute : ControlRegistrationFragmentAttribute
{
    public override string UnitId => "AtomUI.Desktop.Controls/Button";

    public override void Add(AotTrimControlPackageRegistrationBuilder builder)
    {
        GeneratedButtonFragment.Add(builder);
    }
}
```

`ControlRegistrationFragmentAttribute` 是 Core 中跨程序集使用的隐藏生成器 ABI，继承 `Attribute`，提供 `UnitId` 和 `Add`。
代理通过自身 Attribute 创建，避免对未知 `Type` 调用 `Activator.CreateInstance`。微软 Android 的 TypeMap 设计也采用此方式。
`IL2026` 只在生成的条件 TypeMap 声明处局部处理；禁止用全局 NoWarn 隐藏其他 AOT 问题。

每个保留条件都有独立字符串键。条件至少覆盖 Control、生成的 TokenResourceExtension，以及会被装箱或作为运行时类型使用的 Token 枚举。
它们可以映射到同一个 Unit 代理；不同 trimTarget 不复用同一个键。
启动端先按代理 Type 去重，再创建 Attribute 并调用一次。Package 粒度则本包所有条件映射到同一个 Package 代理。
现有模型没有完整枚举所有 internal Control，正式生成器必须补齐内部 owner 映射，不能把当前 public ControlMap 当作完整类型清单。

入口保持：

```csharp
builder.UseDesktopControls();
```

入口内部仍依次执行：

```text
PrepareDesktopPackageCore
→ 创建平台 Provider
→ 添加 PackageShared
→ 查询并收集选中的注册片段
→ 提交 ControlPackageRegistration
→ CompleteDesktopPackageCore
→ 现有 ThemeManager 启动、校验和冻结
```

`UseAllDesktopControls()` 继续直接引用 full registrar，显式保留全包。

## 4. TypeMap 不能枚举，具体怎么注册

.NET 10 TypeMap 虽返回 `IReadOnlyDictionary`，但 `Keys`、`Values`、`Count`、`GetEnumerator` 不受支持。
因此绝不能实现为 `foreach (var entry in map)`。

Generator 给每个 Package 输出纯字符串候选键数组。数组不能包含 Control 的 `Type`、工厂 delegate 或代理实例。

```csharp
var map = TypeMapping.GetOrCreateExternalTypeMapping<DesktopThemeMap>();
var selectedProxies = new HashSet<Type>();
foreach (string key in GeneratedDesktopRegistrationKeys.All)
{
    if (!map.TryGetValue(key, out Type? proxyType) || !selectedProxies.Add(proxyType))
        continue;

    var fragment = proxyType.GetCustomAttribute<ControlRegistrationFragmentAttribute>()
        ?? throw new InvalidOperationException("Invalid generated registration proxy.");
    fragment.Add(packageBuilder);
}
packageBuilder.Register();
```

纯字符串清单不会 root 所有控件。启动查询次数为当前 Package 的候选键数；代理实例仅为保留下来的条目创建。
正式实现可缓存本次构建过程中的代理，但不得建立持有应用 Provider/ThemeManager 的全局缓存。
TypeMapping 调用必须在每个包中使用具体、闭合的 Group 类型；不能抽成内部调用 `TypeMapping<TGroup>` 的泛型帮助方法。
共享帮助方法接收已经取得的 dictionary、候选键和 builder。

## 5. 跨程序集与 NuGet

官方 TypeMap 从入口程序集及 `TypeMapAssemblyTarget` 声明的目标收集映射，不能假设所有 NuGet 自动被扫描。

应用 ordinary generator 仅根据已解析引用中的 Package 标记，输出：

```csharp
[assembly: TypeMapAssemblyTarget<DesktopThemeMap>("AtomUI.Desktop.Controls")]
```

Package 标记只包含 Package 身份和 Group 类型，不包含 Control 列表。沿已解析引用元数据读取标记，不读取应用方法体。
这段引导生成合入 ordinary generator；不再维护单独的 linked-publish analyzer、companion Sidecar、hash 合并或旧 DLL usage 恢复。
未调用可选包入口时，不查询该包 Group，也不自动启用该包的 Provider/initializer。

## 6. 资源和依赖边界

### 6.1 C# 和编译后的 AXAML

`new Presenter()`、`typeof(Control)`、AXAML 编译产生的类型引用交给官方链接器分析。
不再由 AtomUI 扫描语法树、遍历方法体或导出 UnitEdge。

### 6.2 字符串资源引用

链接器不能理解任意 `StaticResource "SomeKey"` 与某个资源工厂的业务关系。
必须保留 Package 本地的 Theme ownership 解析和现有 AXAML wrapper 生成：

- 本 Unit 的命名资源、基础主题和 wrapper 随片段直接引用。
- Token 扩展在编译后的 AXAML 中具有真实类型：优先把生成的 Token 扩展/枚举映射到 owner 的 Unit 代理，避免再解析应用 Token 使用。
- 跨 Unit 的任意纯字符串资源关系仍须在包生成阶段转换为真实类型引用或资源工厂引用。
- 类型引用应进入实际被校验的 descriptor/required-type 数据，不能写一个会被消除的空 `typeof` 表达式。
- 应用只使用 `ButtonTokens.Identity` 等静态字符串身份时，不保证触发 TypeMap；必须单独确定强类型 owner 数据或显式动态 root 契约。
- `ThemeAssetManifestWriter` 中需要资源保留却没有任何类型引用的关系，仍须形成链接器可见的引用。
- 无法确认 owner 的资源：Package 构建时报可定位诊断，或把该 Package 声明为整包注册；不能静默漏掉。

这保留了局部资源契约，但删除应用级全局依赖推导。不要把“字符串依赖自动解决”作为方案承诺。

### 6.3 时序与排序

片段只收集 descriptor 和资源 factory，不执行需要另一个片段已经注册的逻辑。
全部收集后统一校验、确定资源优先级并挂载。保留现有 deferred wrapper，避免提前解析依赖尚未挂载的命名资源。
现有依赖排序若实际影响覆盖顺序，必须提炼为资源优先级契约，不能直接换成字母排序。
循环由 linker 可达性处理；运行时片段按 UnitId 去重，无递归注册。

### 6.4 特殊入口

- 只调用静态附加属性，不一定符合 TypeMap 的类型可达性规则；需要主题的访问器应由包生成器增加可观察的 owner 类型引用，或以其真实宿主为 trimTarget。
- `UseAllDesktopControls`、显式动态 root 保留；任意字符串动态创建仍遵守 .NET 的裁剪标注规则。
- 新生成器产生的 Control 创建只要进入最终 IL，就不需要把生成器输出反馈给另一个 Roslyn generator。
- Semantic Part descriptor 必须随同一片段收集，不能只迁移 Token 和 AXAML。

## 7. 文件改造范围

| 文件/组件 | 改造 |
| --- | --- |
| `src/AtomUI.Generator/ThemeSchema/GeneratedThemeSchemaWriter.cs` | 保留 descriptor factory/leaf fragment，生成 TypeMap 和注册代理；避免全控件数组与公共静态状态相互 root |
| `src/AtomUI.Generator/ThemeAssets/ThemeAssetManifestWriter.cs` | 保留局部资源归属、wrapper 和必要类型引用；停止输出 Sidecar UnitEdge |
| `src/AtomUI.Build.Tasks/GenerateThemeAssetWrappersTask.cs` | 保留 |
| `src/AtomUI.Desktop.Controls/ThemeManagerBuilderExtensions.cs` | generated 分支改为包内 TypeMap 查询，保持 Package Core 顺序 |
| `src/AtomUI.Core/Registration/` | 新增代理基类与包注册收集支撑；复用现有 builder，并补齐 Semantic Part 收集 |
| ordinary generator | 新增小型 Package Group 引导输出 |
| `AtomUI.Generator.LinkedPublish` | 新路径不再引用；兼容路线退役后删除 |
| ApplicationRegistrationPlanGenerator / PlanRegistry | 新路径删除 |
| Sidecar writer / resolver / consumer IL extraction | 新路径删除 |
| `build/AtomUI.LinkedRegistration*.targets/props` | 去掉 Sidecar、ProjectReference linked context 和 Plan 校验；仅保留所需的模式选择 |

## 8. 兼容策略

- .NET 10 NativeAOT 和 trimmed CoreCLR：以已验证的 runtime/ILC/ILLink 10.0.8 为初始基线；早期 10.0 补丁版本有跨程序集 TypeMap 缺陷，不笼统承诺全部 .NET 10 工具链。
- 普通 Debug/非裁剪 Release：直接 full registrar，复用 leaf 事实源。
- .NET 8：若要求无参数自动精细裁剪，暂用旧管线；不能承诺一次迁移便删除旧实现。
- Mono/WASM：.NET 10.0.8 的公开 TypeMapping 实现在裁剪与非裁剪运行时均不支持，不能启用本路径。官方不保证 TypeMap Attribute 裁剪后存在，也明确禁止直接读取它们替代公开 API；不采用元数据扫描 adapter 作为正式兼容方案。
- 稳态删除旧管线需要明确收敛支持矩阵；否则只能完成 .NET 10 路径替换。

## 9. 实测与尚未验证

临时验证目录：`/tmp/atomui-typemap-proof-cqmdilba`，不是正式测试工程。

环境：SDK 10.0.300、.NET 10.0.8、Avalonia 12.1.2、macOS arm64。

- 单程序集：去掉映射后注册断言失败；启用条件映射后 NativeAOT 通过。
- 跨程序集：应用调用包内注册入口，应用只通过字符串键查询 TypeMap。
- 真实 AXAML：MyButtonTheme 的 ControlTemplate 引用 MyPresenter；应用没有 `typeof(MyPresenter)` 或 `new MyPresenter()`。
- NativeAOT 和 trimmed JIT 均输出 `registrations=Button,Presenter template=MyPresenter`。
- 注册收集完成并冻结后才实例化 Button 和模板，验证没有 late registration。
- ILC map 保留 ButtonRegistration、PresenterRegistration，缺少 UnusedRegistration 和 UnusedControl。
- 普通 Debug 的映射包含所有三个条目，行为符合未裁剪全量预期。

Homebrew 原生链接产生系统库最低 macOS 版本警告，未隐瞒为无警告发布。
最初以 `Proof.App` 为可执行名的产物被 macOS AMFI 拒绝；改用普通可执行名 `TypeMapProof` 重新构建后通过，未修改系统安全设置。

上述是第一轮实验记录。后续真实 AtomUI ThemeManager、DatePicker 等验证见深入验证文档；NuGet 冷消费、多平台和体积/发布时间仍不能由第一轮数据推断。

## 10. 迁移验收

先只接 Button 与一个真实跨控件主题依赖，复用现有 fixture，锁定 ordinary/generated Registry、Token、Semantic Part 和资源优先级一致。
再扩展到 Desktop 和 optional packages。必须补：循环、多个 Control 共用 Unit、静态附加属性、动态 root、生成代码、未调用 Package 入口、字符串资源依赖。
达到现有 NativeAOT 体积门槛并通过平台矩阵后，才能替换默认发布路径。

## 11. 官方依据

- [TypeMapAttribute 条件保留语义](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.typemapattribute-1?view=net-10.0)
- [TypeMapping intrinsic API](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.typemapping.getorcreateexternaltypemapping?view=net-10.0)
- [.NET 10 TypeMap 不支持枚举的实现](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/TypeMapLazyDictionary.cs)
- [微软 Android TypeMap 与 Attribute 代理设计](https://github.com/dotnet/android/issues/10788)
