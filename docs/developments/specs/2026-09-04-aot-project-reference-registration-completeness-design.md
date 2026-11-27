# AOT ProjectReference 注册闭包完整性设计

> 历史记录：本文涉及的旧控件注册/裁剪方案已被 [TypeMap 正式架构](../../architecture/foundations/aot-and-trimming.md)替代，不再作为维护约束。其余内容保留当时的设计上下文。


> 当时记录的状态：2026-09-05 已实现并通过源码 ProjectReference、预编译 DLL 与缺失入口回归验证。
>
> 关联问题：[AtomUI/AtomUI#453](https://github.com/AtomUI/AtomUI/issues/453)。
>
> 本文细化多项目 AOT/Trim 发布中的注册闭包完整性。实现稳定后，长期契约同步到
> [`docs/architecture/foundations/aot-linked-registration-pipeline.md`](../../architecture/foundations/aot-typemap-registration.md)
> 和 [`docs/engineering/development/aot-programming-guidelines.md`](../../engineering/development/aot-programming-guidelines.md)。

## 1. 结论

采用“**源码项目自动传播 linked context + 预编译消费程序集构建期恢复 + 缺失入口编译期失败**”的两层方案：

1. 当最终应用因 `PublishAot`、`PublishTrimmed`、`RunAOTCompilation` 或显式严格模式进入 linked build 时，AtomUI 的
   `buildTransitive` 资产自动把内部属性 `AtomUILinkedPublish=true` 和 `AtomUIRegistrationPlanOwner=false` 追加到每个
   `ProjectReference` 的 `AdditionalProperties`。传播在 MSBuild 项目图中递归发生。
2. 可重编译的引用项目加载 linked-publish analyzer，产生包含本程序集 Entry、Control、UnitRoot、PackageRoot usage 的 companion
   sidecar；最终应用合并全部 sidecar，生成精确静态注册计划。
3. 无法随当前发布图重编译、没有 companion sidecar 的普通消费 DLL，由 Build Task 在构建期读取 PE 元数据和 IL。若 DLL 引用了已知
   AtomUI control package，则为对应 package 生成保守 `PackageRoot`；若 IL 中存在已知注册入口的直接调用，再生成 `Entry`。
4. package 已被使用但整个闭包中没有 `Entry` 时，继续复用 `ATOMUILINK008` 在编译期阻止发布，不允许生成一个运行时必然失败的空计划。
5. `AotTrimRegistrationPlanRegistry` 仍是运行时最后防线，但不提供自动 full-registration fallback，也不承担构建图恢复职责。

该方案修复 issue #453，同时保留普通构建零 linked-analysis 成本、运行时零动态发现、静态证据充分时的 Unit 级裁剪收益。

## 2. 已验证的根因

issue #453 的最小等价项目结构是：

```text
Desktop Host (Exe，项目内 PublishAot=true)
  `-- Shared Application (Library)
        `-- App.Initialize() 调用 builder.UseDesktopControls()
```

已复现的实际链路为：

1. Host 自身把本地 `PublishAot=true` 归一化为 `AtomUILinkedPublish=true`，并成为 Application Plan owner。
2. MSBuild 不把项目文件中的普通属性自动当作全局属性传给 `ProjectReference`；Shared 因此仍以
   `AtomUILinkedPublish=false` 构建。
3. 普通 `AtomUI.Generator` 物理排除了 `LinkedRegistrationUsageGenerator`，所以 Shared assembly 不含
   `UseDesktopControls()` 的 Entry usage。
4. Host 能从 AtomUI package assembly 提取 package/unit declaration，却不能从普通 Shared assembly 恢复源代码 usage。
5. Application Plan 的 invoked package 集合只来自 Entry usage。集合为空时仍会生成并安装一个不包含
   `AtomUI.Desktop.Controls` 的计划。
6. 启动时 `UseDesktopControls()` 调用 `AotTrimRegistrationPlanRegistry.ApplyPackage`，最终抛出“plan does not contain package”。

在同一复现上全局传入 `-p:AtomUILinkedPublish=true` 后，Shared 会生成 companion sidecar，Host plan 会出现
`case "AtomUI.Desktop.Controls"`，证明缺陷位于构建上下文传播，而不是 Desktop Controls 的运行时入口逻辑。

仅在 Host 添加：

```xml
<AtomUIPackageRoot Include="AtomUI.Desktop.Controls" />
```

会得到 `ATOMUILINK008`，因为 PackageRoot 只能声明动态使用边界，不能制造缺失的 `UseDesktopControls()` Entry。这也说明当前运行时异常把
`AtomUIPackageRoot` 作为统一建议并不准确。

## 3. 不变量

### 3.1 应用用户零额外配置

正常源码项目继续只需要真实产品入口：

```csharp
builder.UseDesktopControls();
```

用户不需要知道或手工传入 `AtomUILinkedPublish`。`AtomUIPackageRoot` 只用于真实动态使用，不作为构建传播缺陷的补丁。

### 3.2 普通构建零 linked-analysis 成本

- Debug 和未开启 AOT/Trim 的 Release 不加载 `AtomUI.Generator.LinkedPublish.dll`。
- 不创建 AXAML usage 文件、companion sidecar 或 Application Plan。
- 不因为项目引用了 AtomUI 就隐式开启 linked build。

### 3.3 Application Plan 唯一所有者

- 最终 `Exe`/`WinExe` 是默认 plan owner。
- 所有通过 ProjectReference 传播 linked context 的子项目都强制得到 `AtomUIRegistrationPlanOwner=false`。
- 子项目只能贡献 declaration/usage sidecar，不能安装第二份 plan。

### 3.4 运行时零恢复逻辑

运行时不得扫描程序集、读取 sidecar、捕获缺包异常后重试 full registrar，或根据反射结果改变计划。所有恢复与扩大都在构建期完成。

### 3.5 正确性单调扩大

证据状态只有：

- `Exact`：源码项目随发布图重编译，保留精确 Unit 闭包。
- `PackageFallback`：预编译消费 DLL 只能证明 package 使用，保留整个 package。
- `Invalid`：证明 package 使用但找不到注册入口，构建失败。

不存在可能漏注册的 best-effort 计划。

## 4. 子项目 A：ProjectReference linked context 传播

### 4.1 属性边界

在 `build/AtomUI.LinkedRegistration.targets` 通过两次幂等更新覆盖完整 SDK 项目图：

1. evaluation 阶段更新项目文件中已经声明的 runtime `ProjectReference`；
2. `IncludeTransitiveProjectReferences` 完成后、`AssignProjectConfiguration` 之前，更新 SDK 从 assets 展开的传递引用。

analyzer 类型的 `ProjectReference` 不参与传播。evaluation 阶段使用私有 sentinel item 防止同一 targets 被重复导入时再次追加；每个引用
使用 `AtomUILinkedContextApplied` metadata 防止第二阶段重复处理。

```xml
<ItemGroup Condition="'$(AtomUILinkedPublish)' == 'true' and '@(_AtomUIProjectReferenceLinkedContextApplied)' == ''">
  <ProjectReference Update="@(ProjectReference->WithMetadataValue('OutputItemType', ''))">
    <AdditionalProperties>%(ProjectReference.AdditionalProperties);AtomUILinkedPublish=true;AtomUIRegistrationPlanOwner=false</AdditionalProperties>
    <AtomUILinkedContextApplied>true</AtomUILinkedContextApplied>
  </ProjectReference>
  <_AtomUIProjectReferenceLinkedContextApplied Include="$(MSBuildThisFileFullPath)" />
</ItemGroup>

<Target Name="AtomUIApplyLinkedContextToTransitiveProjectReferences"
        BeforeTargets="AssignProjectConfiguration"
        DependsOnTargets="IncludeTransitiveProjectReferences"
        Condition="'$(AtomUILinkedPublish)' == 'true' and '$(TargetFramework)' != ''">
  <ItemGroup>
    <ProjectReference Update="@(ProjectReference)"
                      Condition="'%(ProjectReference.OutputItemType)' == '' and '%(ProjectReference.AtomUILinkedContextApplied)' != 'true'">
      <AdditionalProperties>%(ProjectReference.AdditionalProperties);AtomUILinkedPublish=true;AtomUIRegistrationPlanOwner=false</AdditionalProperties>
      <AtomUILinkedContextApplied>true</AtomUILinkedContextApplied>
    </ProjectReference>
  </ItemGroup>
</Target>
```

实现时保持单行属性值或使用等价 XML，但语义必须一致。

使用 `AdditionalProperties` 而不是 `Properties`：前者合并到 SDK 已设置的 Configuration、Platform、TargetFramework 等属性，后者可能覆盖
项目引用协议已有属性。参见 Microsoft 的
[MSBuild build race 指南](https://learn.microsoft.com/en-us/visualstudio/msbuild/fix-intermittent-build-failures?view=visualstudio)
及 [dotnet/msbuild#12684](https://github.com/dotnet/msbuild/issues/12684)。

### 4.2 为什么不传播 PublishAot

只传播 AtomUI 内部 linked context，不传播：

- `PublishAot`
- `PublishTrimmed`
- `RunAOTCompilation`
- `RuntimeIdentifier`
- `SelfContained`

这些发布属性属于最终产物，传给类库、analyzer 或 Build Task 项目会改变它们的构建语义。子项目只需要编译期 usage 收集，不需要成为
NativeAOT 产物。

### 4.3 递归和幂等

Shared 收到 `AtomUILinkedPublish=true` 后，其导入的同一 targets 会继续更新自己的 ProjectReference，所以 Host → Shared → Feature
可以自然递归。双阶段处理同时覆盖 SDK 写入 `_TransitiveProjectReferences` 的扁平化引用，避免 Host 以普通上下文抢先构建 Feature 并覆盖
linked 产物。

## 5. 子项目 B：预编译消费程序集恢复

### 5.1 发现范围

不能扫描整个 Target Framework 引用集合。Build Task 先从 canonical package sidecar 得到已知 control package assembly 名称，再只选择满足
以下条件的普通引用程序集：

1. 该程序集没有 formal companion/package sidecar；
2. 该程序集自身不是已知 control package producer；
3. 其 AssemblyRef 直接引用至少一个已知 control package assembly。

新增 `DiscoverLinkedRegistrationConsumerReferencesTask` 负责这个筛选。筛选只读 PE metadata table，不解析方法体。

### 5.2 IL 恢复范围

`LinkedRegistrationAssemblyUsageExtractor` 读取被选中程序集：

- 对每个引用到的已知 package 生成 `PackageRoot(packageId)`；
- 遍历有方法体的 MethodDefinition；
- 只识别 IL `call`、`callvirt`、`ldftn`、`ldvirtftn` 操作数指向的 MethodDefinition、MemberReference 或 MethodSpecification；
- 将目标规范化为 `Namespace.Type[+Nested].Method`，与 package sidecar 的 `EntryMethods` 做 ordinal 精确匹配；
- 匹配后生成 `Entry(packageId)`；
- 不推断任意 wrapper、反射字符串、delegate target graph 或间接调用图。

入口方法契约已禁止重载，因此 metadata type + method name 足以与现有 manifest identity 对齐。

### 5.3 生成的恢复 sidecar

预编译消费 DLL 的 sidecar：

```json
{
  "assembly": { "name": "MyApp.Shared" },
  "packages": [],
  "usages": [
    { "kind": "PackageRoot", "identity": "AtomUI.Desktop.Controls", "source": "MyApp.Shared.dll" },
    { "kind": "Entry", "identity": "AtomUI.Desktop.Controls", "source": "MyApp.Shared.dll" }
  ],
  "fallbacks": [
    { "packageId": "AtomUI.Desktop.Controls", "reason": "ExtractedConsumerAssembly" }
  ]
}
```

`PackageRoot` 保证完整 package 注册，`Entry` 证明注册入口存在，fallback reason 保存证据来源。`ExtractedConsumerAssembly` 与现有
`ExtractedManifest` 一样是正常 delivery mode，Application Plan 不为它发出动态代码警告。

### 5.4 编译期失败

如果预编译 DLL 只产生 `PackageRoot` 而没有任何当前应用或引用 sidecar 产生相同 package 的 `Entry`，现有
`LinkedPackageEntryMissing`/`ATOMUILINK008` 阻止生成 plan。

诊断应包含 package ID 和消费程序集文件名，例如：

```text
ATOMUILINK008: Package 'AtomUI.Desktop.Controls' is used by 'MyApp.Shared.dll',
but its UseXxxControls() registration entry is not invoked.
```

这比允许发布并在启动时失败更安全，也不需要新增重复诊断编号。

## 6. 构建流水线顺序

最终 `CollectAtomUIProjectReferenceSidecars` 顺序为：

```text
formal package/project sidecar candidates
  -> canonical package declarations
  -> missing AtomUI producer metadata extraction
  -> re-resolve canonical package sidecars
  -> discover ordinary consumer references against package assembly catalog
  -> extract consumer PackageRoot + Entry evidence
  -> final canonical sidecar resolution
  -> AdditionalFiles for Application Plan generator
```

不得新增嵌套 `MSBuild Projects=...` 来重新构建引用项目。源码项目由正常 ProjectReference graph 传播解决；二进制恢复只读取已经解析的
`ReferencePath`。

## 7. 运行时行为

`AotTrimRegistrationPlanRegistry` 保持：

- 没有 plan：抛错；
- plan 中没有 package：抛错；
- 不尝试 full registrar fallback。

异常信息改为区分构建期可见性和真实动态 usage，明确说明：

1. 确认 `UseXxxControls()` 从应用或随发布图重编译的项目引用中可见；
2. 预编译库应随当前发布图重编译或升级到携带 linked sidecar 的版本；
3. 只有编译期未知的动态控件使用才添加 `AtomUIPackageRoot`。

## 8. 拒绝的方案

### 8.1 运行时自动 full fallback

拒绝。它要求 full registrar 及其静态依赖始终可达，破坏 Unit 裁剪收益；同时把构建闭包缺陷隐藏到运行时分支。

### 8.2 所有普通构建加载 linked-publish generator

拒绝。它违反普通构建零 linked-analysis 成本，并让普通类库为一个发布模式承担额外语义分析。

### 8.3 只自动传播 AtomUIPackageRoot

拒绝。PackageRoot 不能证明注册入口已调用；issue #453 的最小复现会从运行时异常变成 `ATOMUILINK008`，但并未修复 usage 传递。

### 8.4 只改异常信息

拒绝。错误仍然发生在发布产物启动阶段，不满足构建期闭包完整性。

## 9. 验收矩阵

### 9.1 源码 ProjectReference

- Host 项目文件本地 `PublishAot=true`，Shared 中调用 `UseDesktopControls()`，不传内部命令行属性即可生成包含 Desktop package 的 plan。
- Host → Shared → Feature 三层传播成功。
- 引用已有 `AdditionalProperties` 被保留。
- 子项目 `AtomUIRegistrationPlanOwner=false`，不产生第二个 plan marker。
- 普通 Debug/Release 不产生 linked sidecar。
- clean、incremental 和 no-op build 没有重复属性或 sidecar 冲突。

### 9.2 预编译 DLL

- 普通预编译 DLL 调用 entry 并引用 package：产生 Entry + PackageRoot，计划走 full package fallback，运行成功。
- 普通预编译 DLL 引用 package 但不调用 entry：`ATOMUILINK008`，不生成 plan marker。
- 不引用 control package 的普通 DLL：不产生恢复 sidecar。
- package producer 的 formal sidecar 优先于 metadata extraction；consumer recovery 不覆盖 producer sidecar。
- Desktop、DataGrid、ColorPicker、Extras 的 entry identity 均能匹配。

### 9.3 发布模式

- Trimmed JIT 产物运行。
- NativeAOT 产物运行。
- `BuildProjectReferences=false` / `--no-build` 使用预编译 DLL 时走构建期恢复。
- 普通构建不新增 runtime reflection、sidecar runtime asset 或 publish output 文件。

## 10. 文档影响

实现后同步修改：

- `docs/architecture/foundations/aot-linked-registration-pipeline.md`：补充 ProjectReference context propagation 和 binary recovery。
- `docs/engineering/development/aot-programming-guidelines.md`：删除正常源码项目必须手工传
  `-p:AtomUILinkedPublish=true` 的要求；保留该属性仅供内部验证。
- issue/发布说明：说明 6.1.4 引入的多项目 AOT 构建闭包问题已在构建层修复，无公开 API 变化。

## 11. 兼容性结论

- 公共 C# API：无变化。
- AXAML/API 行为：无变化。
- 普通构建：无 linked-analysis 成本变化。
- 源码 AOT/Trim 项目：从缺失 plan 修复为精确 Unit plan。
- 预编译消费 DLL：可能从错误产物变为 package 级 full fallback，体积增加是正确性优先的明确选择。
- 运行时：无反射、扫描或自动恢复。
