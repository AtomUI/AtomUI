# AOT 注册入口声明收敛设计

> 历史记录：本文涉及的旧控件注册/裁剪方案已被 [TypeMap 正式架构](../../architecture/foundations/aot-and-trimming.md)替代，不再作为维护约束。其余内容保留当时的设计上下文。


> 日期：2026-08-15
>
> 当时记录的状态：已实现并于 2026-08-15 完成完整发布验证。正式架构契约由
> [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md) 所有；本文记录方案比较、迁移范围和验收结果。

## 背景

旧实现要求参与 linked registration 的 Control Package 在项目文件中维护完整注册方法 metadata 字符串。该属性把 Source
Generator 的 entry manifest 输入泄漏到包作者的 MSBuild 项目中，并与真实 C# 方法形成两份事实来源。
IDE 无法验证字符串；类型、命名空间或方法重命名后配置可能静默失配，最终只在消费应用 linked publish 时表现为缺少入口。

第一方项目还存在跨程序集复制入口的特例：`AtomUI.Controls` 使用 Desktop 的公开方法名作为自己的 entry。该关系没有表达
Common 的真实运行语义，因为 Common 始终由 Desktop 完整注册，不参与 Unit 裁剪。

## 设计决策

注册方法本身成为入口身份的唯一事实来源：

```csharp
using AtomUI.Registration;

[ControlPackageRegistrationEntry]
public static IAtomUIBuilder UseAcmeControls(this IAtomUIBuilder builder)
{
    ArgumentNullException.ThrowIfNull(builder);
    // full/generated registration branches
    return builder;
}
```

`ControlPackageRegistrationEntryAttribute` 是面向 Control Package 作者的编译期公共 API。Attribute 无构造参数，不重复编码
Package ID、包含类型或方法名。Generator 读取标记方法的 `IMethodSymbol`，验证签名并派生 CLR metadata name，然后继续
写入现有版本化 Package manifest。

应用 Generator 和运行时协议不因为作者输入方式而改变：

1. Package Generator 输出由真实符号派生的 entry manifest。
2. 应用 Generator 收集对标记入口的真实调用并写入 Usage。
3. Application Plan 仍按 Package ID 直接调用 Unit fragment 或 full registrar。
4. 运行时仍由用户调用 `UseXxxControls()` 触发注册，不读取 Attribute，不扫描程序集。

## 方法契约

标记方法必须满足：

- 定义在当前 Package 程序集中。
- `static`、非泛型，并且是 `IAtomUIBuilder` 扩展方法。
- 第一个参数是 `this IAtomUIBuilder`。
- 返回值可赋给 `IAtomUIBuilder`。
- 包含类型和方法必须是 public。
- 不存在无法由当前 method identity 唯一表达的标记重载。

同一 Package 可以标记多个入口。典型场景是 `UseDesktopControls()` 和显式 full 的 `UseAllDesktopControls()`；Generator 对
metadata name 使用 ordinal 排序和去重，保证构建可重复。

签名、可见性、重载或声明位置无效时，Package 自身编译必须报告 `ATOMUILINK009`。不得把错误推迟到最终应用 publish。

## Common 边界

`AtomUI.Controls` Common 层不再伪装成独立 linked Package：

- `UseCommonControls()` 不声明 `ControlPackageRegistrationEntryAttribute`。
- Common 不输出 Package、Unit、ControlMap 或 full fragment metadata。
- `UseDesktopControls()` 继续在既有位置调用完整 Common 注册。
- Common 的 descriptor、Theme asset、Language Module 和 Provider 仍保持完整可达。
- 不引入跨 Package Unit 闭包、依赖 manifest 或应用级依赖图。

这与现有运行时语义一致，同时删除 `AtomUI.Controls.csproj` 对 Desktop 方法名的跨程序集字符串复制。

## 保留的 MSBuild 边界

`AtomUIRegistrationPackageId` 暂时保留。它只表达 Package identity，并触发第一方产品包注入 Generator、Build Tasks 和
buildTransitive assets；它不再携带入口方法信息。

本次不把 Package identity 自动推导、产品包识别或 NuGet 资产注入与入口声明迁移绑在一起。后续若要把 Package ID 默认值
收敛到 `$(PackageId)`，必须单独评估非 packable 项目、自定义 AssemblyName/PackageId 和消费包打包链。

## 明确拒绝的方案

### 继续使用 MSBuild 字符串或 Item

把 Property 换成 Item 仍然重复维护 CLR metadata name，也没有符号级重命名安全性，因此不解决根因。

### 根据命名约定发现入口

`Use*Controls` 不能可靠区分 Package 注册、普通配置扩展和兼容别名。命名约定会制造难以诊断的隐式行为。

### 扫描方法体或构建调用图

通过查找 `AotTrimRegistrationPlanRegistry.ApplyPackage()`、full registrar 或私有 helper 调用来猜测公开入口，会受到 helper
拆分、条件分支、生成代码可见性和跨程序集调用影响。该方案复杂且不稳定，不作为作者协议。

### 根据静态 Control 使用自动注册 Package

静态使用只能决定 Package 内保留哪些 Unit，不能绕过用户的 `UseXxxControls()` 调用。可选包 gating、Provider、Localization
和 initializer 顺序必须继续由显式入口控制。

## 实现范围

本次实现已完成：

1. 在 `AtomUI.Registration` 增加 `ControlPackageRegistrationEntryAttribute`。
2. Generator 使用 Roslyn symbol 收集和验证标记方法。
3. Package manifest writer 接收派生后的稳定 entry 列表。
4. 删除旧入口字符串的 Compiler-visible property、options、model 字段和字符串解析。
5. 迁移 Desktop、DataGrid、ColorPicker、Extras 和 GalleryBase 的真实入口。
6. 从 `AtomUI.Controls` 删除 linked Package 声明，并保持完整 Common 注册快照。
7. 更新 Generator、build asset、fixture、第三方包和发布测试。
8. 删除全部项目文件、测试和文档中的旧入口属性名称与配置。
9. 更新 Public API baseline、XML 文档和第三方 Control 指南，使 Attribute 的用途和非运行时语义可被 IDE 发现。

## 验收结果

- 标记方法重命名后无需修改项目文件，manifest 自动更新。
- 无效签名在 Package 项目中产生带源码位置的 `ATOMUILINK009`。
- 多个入口稳定排序，manifest 和生成源码可重复。
- 没有标记入口的程序集不输出 linked Package、Unit、ControlMap 或 full fragment metadata。
- Common full registration 的 descriptor、asset、Catalog、Bundle、Provider 和顺序与迁移前一致。
- 普通 full/generated 行为快照一致。
- trimmed JIT、NativeAOT、WebAssembly AOT 和 Gallery NativeAOT smoke 通过。
- 源码、构建资产、测试和文档中不存在旧入口属性名称或等价配置。
- `git diff --check` 通过。

2026-08-15 的实现验证覆盖 Generator 及 Core 全量测试、Desktop/DataGrid/Gallery 行为测试、五个一方 Package 真实构建、
`verify-aot-trim-registration.sh --quick`、`--full`、WebAssembly AOT，以及 macOS arm64 Gallery NativeAOT 发布和启动 smoke。
