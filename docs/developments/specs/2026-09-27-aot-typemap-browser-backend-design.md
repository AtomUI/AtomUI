# TypeMap 替换：浏览器链接后端

> 过程记录：本轮结论已同步到 [正式 AOT 架构](../../architecture/foundations/aot-and-trimming.md)。本文保留方案比较、实验或迁移过程，不再作为平行的长期规范；事实以正式文档为准。


> 配套 [总体设计](2026-09-27-aot-typemap-replacement-design.md)。浏览器自动精细裁剪是硬要求，不允许全包 fallback。

## 1. 为什么需要这个后端

.NET 10 Mono 的公开 TypeMapping 方法直接抛 NotSupportedException；其 ILLink 已有 TypeMap 条件标记能力。
因此采用编译期转换，不在浏览器调用尚未实现的 API，也不运行时枚举裁剪残留的 Attribute。

微软提供 custom step 扩展入口，可在 MarkStep 完成后、Sweep 前执行。AtomUI 自有步骤只把已选映射转换为自己的代码，
不访问内部 TypeMapHandler，不修改 BCL，不分析 C#/AXAML/应用 IL 调用图，不重新计算依赖闭包。

## 2. 普通生成输出

每个 Package 有一个内部、静态、非泛型、返回 `IReadOnlyDictionary<string,Type>` 的 map accessor。
它的原始方法体直接调用具体 Group 的 TypeMapping API，并带供编译器扩展识别的隐藏 marker。

```csharp
[GeneratedTypeMapAccessor(typeof(AcmeMap))]
[DynamicDependency(nameof(MapStorage.Create), typeof(MapStorage))]
[DynamicDependency(nameof(MapStorage.Add), typeof(MapStorage))]
static IReadOnlyDictionary<string, Type> GetMap()
    => TypeMapping.GetOrCreateExternalTypeMapping<AcmeMap>();
```

MapStorage 为确定的非泛型闭合 helpers，只构造/填充 string→Type 字典；不能引用任何具体 Control/代理。
helpers 及其传递实现必须在 Mark 前保留，不能在 Mark 后引入新的未分析依赖。
NativeAOT 路径不加载该转换器，继续执行官方 API。

## 3. 编译步骤

建议独立构建项目 `AtomUI.TypeMap.Linker`，与普通 Generator、Build.Tasks 宿主物理隔离。

1. 验证工具链版本、accessor/helper ABI 和处理阶段。
2. 仅读带 Package marker 的已解析程序集，定位已被标记的自有 accessor。
3. 读取同一具体 Group 中被官方 `Annotations.IsMarked` 标记的 TypeMap 声明。
4. 验证目标 Type 和 helpers 已保留；对 duplicate key、身份冲突或异常程序集状态失败。
5. 以确定顺序改写自有 accessor，只引用现有选中目标。
6. 清理被替换方法的调试序列点/旧局部变量和异常区间，写入转换完成标识。
7. 后置验证所有可达自有 accessor 均已转换，且这些 accessor 不再调用 Mono TypeMapping；未完成阻止发布。检查自己的生成 ABI，不借此扩展为任意应用代码调用图分析。

生成结果等价于：

```csharp
var map = MapStorage.Create();
MapStorage.Add(map, "dependency", typeof(DependencyRegistration).TypeHandle);
MapStorage.Add(map, "used", typeof(UsedRegistration).TypeHandle);
return map;
```

不把全部候选代理写入该方法。unused 类型仍应被 Sweep 删除。
合法的空应用/未启用包可以有零个可达 accessor；它与“已知存在 accessor 但转换步骤没运行”必须区分。

## 4. MSBuild 与版本边界

新 `build/AtomUI.Registration.targets` 只负责模式选择、feature switch、后端工具加载和产物检查。
只在 Browser/Mono 的真实裁剪发布中启用步骤；普通 Debug 使用全量的同一事实源，不调用 Mono TypeMapping。
Browser AOT 顺序必须是 `普通编译 → ILLink+转换 → Mono AOT → bundling`，不能在 AOT 之后修改 IL。

custom step DLL、其依赖、配置和 ABI 版本必须纳入 ILLink 增量输入；SDK 默认 `_RunILLink` Inputs 不足以覆盖扩展 DLL 变化。
更换后端工具后必须重新链接；no-op 发布则复用合法产物。ProjectReference 不传播旧 linked context。

发布工具随产品 NuGet 自动交付，不要求用户手加 `_ExtraTrimmerArgs`。原型中的该参数只是实验接线。
消费多个包时只能加载一份同版本转换器；不允许依次运行多个版本修改同一 slot。

绑定已验收的 SDK、ILLink、WASM workload 组合。custom-step 入口有官方文档，但不是跨版本稳定 ABI；上游正在讨论迁移该机制。
未验证版本明确报错，不能退回全包注册、跳过转换或运行时扫描。

## 5. 本次浏览器实测

位置：`/private/tmp/atomui-typemap-browser-4xd0hmxy`。
工具链：SDK 10.0.300、ILLink.Tasks 10.0.8、WebAssembly/Mono pack 10.0.10。
扩展原型：`/tmp/atomui-typemap-lowering-probe-t4vtpm92`。

实际通过浏览器加载三个发布结果：

| 产物 | 结果 |
| --- | --- |
| 未转换的官方 TypeMapping，Release 浏览器发布 | 明确出现 NotSupportedException，作为负例 |
| Mark 后转换的 Release 裁剪发布 | 已用代理和间接依赖注册，unused 不在映射，执行 PASS |
| 同源码 `RunAOTCompilation=true` 发布 | 构建日志 AOT'ing 5 assemblies，浏览器执行 PASS |

两个正例可见输出：

```text
BROWSER_MATERIALIZED_PROBE_BEGIN
indirect=DependencyControl
registrations=used,dependency
UsedControl
BROWSER_MATERIALIZED_PROBE_PASS
```

两个发布后的 Probe.dll 均保留 Used/Dependency 代理，不含 UnusedRegistrationAttribute 与 UnusedControl 名称。
CoreCLR 侧额外用 Cecil 检查确认未用 TypeDefinition 缺失、转换后 slot 无 TypeMapping 调用，并有禁用步骤的失败对照。

这些是单程序集机制证据，不是完整 AtomUI Browser 的发布验收。跨程序集、多个 Group、NuGet 和真实控件仍须覆盖。

## 6. 正式后端必须增加的验证

- 完全解析的程序集/类型身份，不能像原型只比较 FullName 和 scope 简称。
- 跨程序集 target、传递 target、可选包未启用、多 Group、空 map、循环保留、多个条件共用 proxy。
- Runtime 部署程序集实际为 Link 操作，不能在 copy/skip 状态上假装转换成功。
- 删除/损坏转换器、ABI 不匹配、缺 helper、残留 slot 均有失败测试。
- 后端 DLL 改动使增量链接重新运行；内容不变不重复发布工作。
- Browser interpreter 与 AOT 均检查 unused 控件/资源代理缺失，不能只以浏览器能启动为通过。
- 浏览器运行时无 assembly scan、无旧 Sidecar、无 full fallback、无自定义依赖图。

## 7. 官方依据

- [Custom IL trimmer steps](https://github.com/dotnet/runtime/blob/v10.0.12/docs/tools/illink/custom-steps.md)
- [在指定阶段加入 custom step](https://github.com/dotnet/runtime/blob/v10.0.12/docs/tools/illink/illink-options.md#adding-custom-illink-steps)
- [官方 TypeMap 模型](https://github.com/dotnet/runtime/blob/v10.0.12/docs/design/features/typemap.md)
- [Mono TypeMapping 尚未实现](https://github.com/dotnet/runtime/blob/v10.0.12/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/TypeMapping.cs)
- [扩展 API 演进风险](https://github.com/dotnet/runtime/issues/107211)
- [同 key 多条件尚不支持](https://github.com/dotnet/runtime/issues/120160)
