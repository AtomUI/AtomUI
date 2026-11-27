# 方案二：显式选择控件族，包内生成完整注册闭包

> 过程记录：本轮结论已同步到 [正式 AOT 架构](../../architecture/foundations/aot-and-trimming.md)。本文保留方案比较、实验或迁移过程，不再作为平行的长期规范；事实以正式文档为准。


> 2026-09-27，历史备选，用户已明确否决，不再推进。目标原为覆盖 .NET 8，应用只选择公开控件族，内部依赖由包负责。
> 不要求拆分现有 NuGet；不改变现有控件、Token、主题和资源 API。

## 1. 应用接口

新增 overload，以下是拟议接口：

```csharp
builder.UseDesktopControls(static controls => controls
    .AddWindow()
    .AddButton()
    .AddDatePicker());
```

`DesktopControlSetBuilder` 是公开的受限选择器，`AddXxx()` 由 Package generator 生成。
应用只选择公开控件族；不填写 Presenter、Cell、Popup、Token、主题 URI 或内部依赖。
不采用枚举加全量 switch，也不先建立全包 descriptor 字典，否则可能重新 root 全部工厂。

既有无参数 `UseDesktopControls()` 保留其兼容行为；无参数自动裁剪选择方案一或过渡期旧管线。
显式 overload 在所有构建模式均选择同一控件集合，便于开发期发现遗漏。

## 2. 依赖如何封装

包生成阶段为每个公开入口计算闭包，生成扁平的 leaf fragment 调用：

```csharp
public DesktopControlSetBuilder AddDatePicker()
{
    AddOnce("DatePicker", GeneratedDatePickerFragment.Add);
    // 实际条目完全来自经过验证的包内闭包；这里不虚构真实依赖名单。
    AddDatePickerDependencyFragments();
    return this;
}
```

`AddDatePickerDependencyFragments` 的输出也是直接 leaf 调用，不再调用另一个公开 `AddXxx()`。
同一闭包的每个 Unit 只出现一次。多个公开入口共享的片段由本次选择器中的 UnitId 集合去重。
去重仅影响启动时收集；未调用入口对应的方法及其引用仍可被官方 linker 删除。

不能把 delegate 装进一个全包静态表。每个 `AddXxx` 只引用自己闭包中的 leaf factories。

## 3. 包生成阶段仍需要做什么

这是该方案与 TypeMap 的主要差异：.NET 8 不会替 AtomUI 得出主题注册闭包，因此包内分析仍必须存在。

- 保留 Directory 模式的 Control/资源 ownership。
- 保留包内直接 C#/AXAML 依赖发现及 SCC/闭包计算。
- 复用现有 `LinkedRegistrationPackageManifestGenerator` 的事实分析和 `RegistrationUnitGraphPlanner`，将输出改成同程序集的扁平 C# 调用。
- 普通第三方 Package 模式只生成一个整包入口，不承担控件族分析。
- 无法证明某个公开入口的闭包时，该入口扩大为整包，并发出原因诊断；strict 包构建失败。
- 包生产阶段验证最终 AXAML/generated IL 能被闭包覆盖；不把生成器互相不可见的问题转嫁给消费应用。

不能宣称只写一个 `AddButton` 就能删除全部依赖分析。消除的是应用及传递消费程序集的全局推导，保留的是可在包自身验证的局部推导。

生产图不要继续使用对普通静态 helper 调用的无界递归；沿用已有直接候选与结构预算。
若一个控件族闭包过大，修正真实模块边界，而不是回到递归 `AddDependencies`。

## 4. 注册时序

```text
PrepareDesktopPackageCore
→ 创建平台 Provider 与选择器
→ 执行用户的静态选择 callback
→ 收集 PackageShared、所有选中 leaf fragment
→ 校验并提交一个 ControlPackageRegistration
→ CompleteDesktopPackageCore
→ 现有 ThemeManager 启动与冻结
```

所有片段共用一个 builder；不得为每个控件创建 Provider、重复注册 Language 或重复运行 initializer。
Semantic Part descriptor 与 Token、Theme 同步收集。
资源覆盖顺序由明确的 Package/资源优先级决定，不由用户 `.AddXxx()` 的书写顺序意外决定。
需要保留现有 deferred theme wrappers，并验证跨资源 `BasedOn`、命名 `StaticResource`。

## 5. 遗漏与动态输入

- 显式集合遗漏了实际使用的控件时，开发构建也必须报可定位错误，不能等 NativeAOT 才出现空白控件。
- 复用/增强 Theme Registry 缺失诊断，包含 CLR 类型、Package 和对应 `AddXxx()`；对没有 Token 查找的控件，需要控件主题启用处的轻量检查。
- 不以全局 usage analyzer 验证遗漏，否则重新引入本方案试图删除的体系。
- 字符串动态创建需要显式选择该族，同时满足官方对构造函数等成员的裁剪要求；选择主题不等于保留任意反射成员。
- 未调用可选包入口仍属于配置错误，不自动启用包。

## 6. 文件与组件

| 文件/组件 | 结果 |
| --- | --- |
| `ThemeManagerBuilderExtensions.cs` | 新增显式 overload，复用 Package Core |
| 生成的 `DesktopControlSetBuilder.g.cs` | 新增公开 `AddXxx`；扁平闭包输出 |
| `GeneratedThemeSchemaWriter` / `ThemeAssetManifestWriter` | 保留叶子片段和资源工厂 |
| 包内 dependency analyzer / graph planner | 保留，输出职责改为包内 closed registration 方法 |
| `AotTrimControlPackageRegistrationBuilder` | 复用收集和提交能力；补齐 Semantic Part |
| 应用 usage analyzer / Application Plan | 显式路径删除 |
| Sidecar codec/hash/resolution/consumer IL recovery | 显式路径删除 |
| linked context 递归传播与 Plan owner | 显式路径删除 |

如果仍要同时维持旧版无参数自动裁剪，这些组件只能从新路径移除，不能当场从仓库删除。

## 7. 实测

临时目录：`/tmp/atomui-typemap-proof-cqmdilba/explicit`。
环境：SDK 10.0.300、目标 .NET 8.0.28 NativeAOT、Avalonia 12.1.2、macOS arm64。

实验使用应用程序集和控件程序集；真实 AXAML 模板创建内部 Presenter。
应用显式 `.AddButton().AddButton()`；包内扁平闭包注册 Button 和 Presenter。

- 发布后执行通过，注册仍在第一次控件实例化前冻结。
- 两次选择同一控件族，注册结果只有 Button、Presenter 各一个。
- 实际 ControlTemplate 构造出 MyPresenter。
- ILC map 有 ButtonRegistration、PresenterRegistration，无 UnusedRegistration、UnusedControl。
- 该实验手写模拟生成器输出，尚未证明现有包分析器能为所有真实 AtomUI 控件生成正确闭包。

## 8. 验收与代价

必须验证 `.AddDatePicker()` 的完整真实闭包、混合选择顺序、重复选择、循环、平台资源、Semantic Part、动态 roots、遗漏诊断。
同 SDK/RID 比较 full、显式和现有自动计划，不能用 .NET 8 与 .NET 10 的实验二进制比较体积。

收益：消费应用无分析、无 Sidecar、无额外链接阶段，.NET 8 可用，运行时为普通静态调用。
代价：应用维护公开控件族集合；Package 仍维护自动依赖分析；扁平闭包可能产生重复生成代码，但不会在运行时重复提交。
