# 编译主题包生成 ABI v1

> 已按静态声明生成收敛，尚未发布；本次本地范围验证已完成。本文固定跨程序集入口；总体语义和生命周期由
> [编译主题包架构](../../architecture/systems/theming/compiled-theme-packages/overview.md)拥有。

## 1. 注册与程序

以下类型位于 `AtomUI.Theme.Definitions.Compiled`；面向生成器的类型标记 EditorBrowsable(Never)，
应用只调用生成的包注册 helper。

```csharp
public sealed class CompiledThemePackage
{
    public CompiledThemePackage(string id, IReadOnlyList<CompiledThemeProgram> programs);
}

public abstract class CompiledThemeProgram
{
    protected CompiledThemeProgram(int abiVersion, string sourceIdentity, string sourceRevision,
        string contentDigest, string programDigest, string id, string name,
        ThemeAppearance appearance, bool isDefault, Color? accentColor);
    protected abstract void Describe(CompiledThemeBindingBuilder builder);
    protected abstract void WriteStaticDefaults(CompiledThemeValueWriter writer);
}
```

`IThemeManagerBuilder.AddCompiledThemePackage(CompiledThemePackage package)` 是显式注册入口。
`AtomUI.Generated.<Assembly>.GeneratedThemePackageRegistration` 提供 `CreatePackage()` 与
`Register(IThemeManagerBuilder builder)`；前者仅创建轻量描述，后者提交同一个包到 Builder。
生成代码写入 ABI 数字 `1`，不能引用随运行时升级变化的常量来掩盖旧生成代码。

Describe 在实际 registry 上验证全部必要契约，不创建值或调用 evaluator。WriteStaticDefaults 在所选定义首次
需要默认覆盖时执行；成功后冻结并释放工厂/binding 临时引用，失败保留可重试输入。
两个受限 builder/writer 均由 Core 构造，不能被外部 new；Describe/Write 无论成功或失败都在 finally 中终止本次写入能力。
重试创建新 writer，封存或失败后继续写入必须失败，旧 finally 不能清除新执行的候选。

## 2. 绑定与类型化写入

```csharp
public sealed class CompiledThemeBindingBuilder
{
    public void SetAlgorithms(IReadOnlyList<ThemeAlgorithm> algorithms);
    public void RequireGlobal<T>(int tokenId, object resourceKey, string expectedTokenName, TokenStage stage, int valueContractVersion);
    public void RequireControl(int controlId, ControlTokenIdentity identity, string expectedCatalog, string expectedId,
        string ownerAssemblyIdentity, string ownerMetadataName, ControlAlgorithmMode mode, IReadOnlyList<ThemeAlgorithm> algorithms);
    public void RequireControlGlobal<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
        TokenStage stage, int valueContractVersion);
    public void RequireOwn<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName, int valueContractVersion);
}

public sealed class CompiledThemeValueWriter
{
    public void SetGlobal<T>(int tokenId, T value, string canonicalValue);
    public void SetControlGlobal<T>(int controlId, int tokenId, T value, string canonicalValue);
    public void SetOwn<T>(int controlId, int tokenId, T value, string canonicalValue);
}
```

tokenId 在整个程序内唯一、controlId 在程序控件集合内唯一；均为稠密非负本地序号，不能作为应用 slot。
resourceKey 使用现有 `SharedTokenKind` 或对应 `XxxTokenKind`，Control 使用公开 `XxxTokens.Identity`；
实际绑定检查精确 owner Type/定义程序集、资源 key、编译时 Token 名、`typeof(T)`、stage 与值契约版本，不能凭结构摘要自认证。
枚举常量内联后若库版本改变排序，必须拒绝名称不匹配，不能把旧数值重解释成另一个 Token；包需重新构建。
compiled owner 必须有真实 OwnerType，不能接受动态 XML 使用的纯字符串 identity。
重复、缺失、越界、多写、跨 Control 写入及未知枚举均拒绝。值被复制到本次候选；writer 不暴露资源发布能力。

`canonicalValue` 是构建 codec 从类型化值生成的规范字符串，不是原始 XML 文本；动态路径使用同一规范。
它用于现有配置/缓存等价协议，生成代码必须与值对应，差分验证同时比较值和 canonical 字符串。
编译包不是不受信任代码沙箱；手写实现必须遵守上述契约，不允许捕获应用对象或伪造生成证据。

## 3. Descriptor 值契约

`TokenDescriptor.CreateGenerated` 新增末尾 `int valueContractVersion` 的重载，现有入口保留。
v1 描述标准 literal、类型化复制/冻结/恢复与现有资源投影语义；仅受支持的内置 codec 可以声明 v1。
手写委托构造及旧生成入口没有该证据，显式编译覆盖不能绕过其动态 codec。
Global 算法与 Own evaluator 统一在运行时执行；值契约只描述字面量、复制与投影能力。

`TokenValueCodec<T>.Copy(T value)` 是支持域的类型化复制入口；未知引用类型拒绝，不能静默转成字符串。

FontFamily literal 允许生成标准公开构造，保留依赖内部对字体描述的解析；复制已有不可变 FontFamily 不再次解析。
构造的 URI、名称与 fallback 顺序必须与动态 XML 一致；此项单独计费，不能把它算进“零静态解析”的结果。

## 4. 构建契约元数据

Core 声明以下 assembly attributes（同一程序集可重复），namespace 为 `AtomUI.Theme.Definitions.Compiled`：

```csharp
ThemeBuildOwnerAttribute(int version, string catalog, string id, string ownerAssemblyIdentity,
    string ownerMetadataName, string identityProvider, string schemaDigest)
ThemeBuildTokenAttribute(int version, string catalog, string ownerId, string name,
    string valueType, string valueTypeAssemblyIdentity, string resourceKeyType,
    string resourceKeyAssemblyIdentity, string resourceKeyMember, int stage, int valueContractVersion)
```

只含字符串和整数，不产生 Control Type 根表。Global 的 catalog/ownerId 均为空；attribute 所属 IAssemblySymbol
确定契约/identity provider 的生产程序集，ownerAssemblyIdentity 单独确定 owner 定义程序集，两者不要求相同。
值类型和资源键类型也保留定义程序集身份；类型解析必须无歧义，不能猜测同名类型。类型名使用完整 C# 类型表达，资源键与 identityProvider
使用完整类型名，不猜测简单名称。每个精确 owner 下 token 名/key 唯一；记录 version 固定为 1。
schemaDigest 使用规范排序、明确字段边界的 SHA-256，不能代替运行时实际 registry 校验。

同工程契约由共同 transform 直接传递；引用包契约从 Roslyn 元数据读取。引用 metadata 中的非法/重复记录为
构建错误；不能通过加载项目 DLL 或反射 internal Token 来补齐。

## 5. 构建项及诊断

`AtomUIThemeDefinition` 显式指定源文件，可附 Link 作为逻辑路径。Tasks 使用权威 Reader/XSD 校验同一有界字节
快照并生成版本化中立 AdditionalFile，带原始 SHA-256、位置与 diagnostics；这些中间输入不进入运行时。
程序摘要覆盖中立输入、所需 Token 契约、字面量/ABI 与生成实现身份。所有输出按 configuration/framework 隔离。

中立传输版本为 3，仅包含声明文本、来源位置、摘要和诊断；保留记录/字节限额和 payload 摘要校验。
旧版本输入直接拒绝，由配套工具重新生成。它是构建工具私有协议，不进入运行时，运行时生成 ABI 仍为 v1。

Generator 的 `ThemeDefinitions/Literals/` 负责静态字面量解析与 C# 表达生成。HSL/HSV 颜色字面量通过
`ThemeLiteralColorConversion` 转成 ARGB；这不包含主题派生算法。生成程序只注册并写入静态默认值，
由现有 ThemeCompiler 在有效配置下执行运行时算法和快照生命周期。

本次验证状态见[验证文档](../../architecture/systems/theming/compiled-theme-packages/verification.md#8-当前验证状态)。
