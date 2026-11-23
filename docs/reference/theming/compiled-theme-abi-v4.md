# 编译主题包生成 ABI v4

> 本文固定编译主题包的跨程序集入口：生成器写出的代码与 `AtomUI.Core` 之间的契约。主题文件的语义由
> [主题定义 XML v2 规范](theme-definition-xml-v2.md) 定义；总体生命周期见
> [编译主题包架构](../../architecture/systems/theming/compiled-theme-packages/overview.md)。ABI v1 已废止，见
> [ABI v1](compiled-theme-abi-v1.md)。

ABI 4 对应主题定义格式 v2：程序登记设计语言、固定的轴、锁定的种子与条件段，不再登记算法链、Control 算法模式或
`IsDefault`。ABI 的数字只在生成代码里以字面量 `4` 出现；Core 只接受 `4`，其他值在程序构造时抛
`NotSupportedException`（`Compiled theme ABI '…' is not supported; rebuild the package against the current AtomUI.`）。
旧二进制不做适配，用当前 AtomUI 重新构建即可。

## 1. 注册与程序

以下类型位于 `AtomUI.Theme.Definitions.Compiled`，均标记 `EditorBrowsable(Never)`；应用只调用生成的包注册 helper。

```csharp
public sealed class CompiledThemePackage
{
    public CompiledThemePackage(string id, IReadOnlyList<CompiledThemeProgram> programs);
}

public abstract class CompiledThemeProgram
{
    protected CompiledThemeProgram(
        int abiVersion, string sourceIdentity, string sourceRevision, string contentDigest,
        string programDigest, string id, string name, string language, string? style, int appearance,
        string? density, string[] lockedSeeds, Color? accentColor);

    protected virtual bool IsBackgroundPrepareSafe { get; }
    protected abstract void Describe(CompiledThemeBindingBuilder builder);
    protected abstract void WriteStaticDefaults(CompiledThemeValueWriter writer);
}
```

构造参数：

| 参数 | 含义 |
| --- | --- |
| `abiVersion` | 必须为 `4` |
| `sourceIdentity` | 稳定的来源身份，`<AssemblyName>/<LogicalPath>`；包内唯一 |
| `sourceRevision`、`contentDigest` | 来源的原始字节 SHA-256（生成器两处都写内容摘要） |
| `programDigest` | 生成程序的摘要：ABI、字面量 codec、生成器实现身份、来源、主题元数据、语言契约、每个绑定的 Token 契约与规范值 |
| `id`、`name`、`language` | 根元素的 `Id`、`Name`、`Language`；`id` 与 `language` 须符合 Identifier 词法 |
| `style`、`density` | 根上固定的风格与密度；开放时为 `null` |
| `appearance` | 根上固定的明暗：`0` Light、`1` Dark、`-1` 开放 |
| `lockedSeeds` | 基础段中 `Locked="true"` 的种子名 |
| `accentColor` | 基础段全局 `ColorPrimary` 的值（供 `ThemeInfo.AccentColor`）；未写时为 `null` |

`IThemeManagerBuilder.AddCompiledThemePackage(CompiledThemePackage package)` 是显式注册入口。生成的
`AtomUI.Generated.<Assembly>.GeneratedThemePackageRegistration` 提供 `CreatePackage()` 与
`Register(IThemeManagerBuilder builder)`；前者只创建轻量描述，后者把同一个包提交给 Builder。语言包的 `Use<Lang>()` 入口在内部
调用自己的 `Register`（例如 `UseAntDesign()` 注册 Daybreak Blue），每个 Builder 只注册一次。

`CompiledThemePackage` 要求包内每个程序的 `Id` 与 `sourceIdentity` 各自唯一，程序数不超过 65536。

生成器发射的纯数据程序重写 `IsBackgroundPrepareSafe => true`，允许启动期在工作线程准备；手写程序默认 `false`，其
`Describe` / `WriteStaticDefaults` 在 UI 线程执行。作者只有在全部被调用代码可跨线程、且可与超时回退并发时才应显式覆写。

## 2. 绑定：从程序到已启用的注册表

`Bind` 在运行时注册表冻结后执行，依次检查：

1. `language` 已启用；否则失败（`Compiled theme '…' belongs to design language '…', which is not enabled.`）。
2. 固定的 `style`、`density` 是该语言声明的取值。
3. 每个锁定的种子都是该语言的可调种子。
4. 调用 `Describe`，逐条核对声明（第 3 节）；任一不符抛 `ThemeSchemaException`。

绑定成功后得到与动态 XML 相同形状的主题定义；值在首次需要时才调用 `WriteStaticDefaults` 写出，结果缓存在该定义上。
`Describe` 与 `WriteStaticDefaults` 各自使用 Core 构造的受限 builder / writer，无论成功或失败都在 `finally` 中封存；封存后继续写入
抛 `ObjectDisposedException`。

启动期的「只含全局 Token」注册表没有控件描述：此时 Control 声明登记为占位，Control 的 Token 声明与写入被跳过，全局层照常准备。

## 3. 声明：CompiledThemeBindingBuilder

```csharp
public sealed class CompiledThemeBindingBuilder
{
    public void BeginSection(int index, string? style, int appearance, string? density, string? idiom);

    public void RequireGlobal<T>(int tokenId, object resourceKey, string expectedTokenName,
        TokenStage stage, int valueContractVersion);
    public void RequireLanguage<T>(int tokenId, string expectedTokenName, TokenStage stage, int valueContractVersion);

    public void RequireControl(int controlId, ControlTokenIdentity identity, string expectedCatalog, string expectedId,
        string ownerAssemblyIdentity, string ownerMetadataName, int appearance, string? density);
    public void RequireControlGlobal<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
        TokenStage stage, int valueContractVersion);
    public void RequireOwn<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
        int valueContractVersion);
}
```

**段。** 声明以段组织，与 XML 一一对应：段 `0` 是基础段，不带条件（四个条件参数都为空 / `-1`）；段 `1..n` 是按文档顺序的
`<When>`，至少带一个条件。段必须按序号连续开启；条件值必须是语言声明的风格、密度、形态，否则抛 `ThemeSchemaException`。
其后的 Token 与 Control 声明都属于当前段。

**Token。**

| 方法 | 对应 XML | 目标 |
| --- | --- | --- |
| `RequireGlobal<T>` | 段内 `<Tokens>/<Token>` | 全局 Token；`resourceKey` 为 `SharedTokenKind` 成员 |
| `RequireLanguage<T>` | 段内 `<LanguageTokens>/<Token>` | 所属语言的语言 Token，按名称匹配 |
| `RequireControlGlobal<T>` | Control 的 `<Tokens>` 中的全局 Token | 该控件视图里的全局 Token |
| `RequireOwn<T>` | Control 的 `<Tokens>` 中的控件 Token | 该控件的控件 Token；`resourceKey` 为对应 `XxxTokenKind` 成员 |

每条声明核对精确的 Token 名、`typeof(T)`、阶段（`TokenStage`：Seed / Map / Alias / Control）、资源键与值契约版本（当前为 `1`），
不能凭结构摘要自认证。枚举常量内联后若库版本改变了成员排序，名称不匹配即拒绝，不把旧数值重解释成另一个 Token；包需重新构建。

**Control。** `RequireControl` 的身份使用公开的 `XxxTokens.Identity`，并核对 Catalog、Id、owner 的精确类型与程序集简单名；
程序集版本号不要求相等，运行时不比较 `Assembly.FullName`。`appearance`（`0` / `1` / `-1`）与 `density` 是该 Control 固定的轴，
只能在段 `0` 出现；密度必须是语言声明的密度档。同一段内 `(Catalog, Id)` 不得重复。

**编号。** `tokenId` 在整个程序内唯一、`controlId` 在程序的控件集合内唯一，均为 `0..65535` 的本地序号，不是应用槽位。
同一段、同一 Control 内的 Token 名不得重复（语言 Token 与全局 Token 分开计）。

## 4. 写值：CompiledThemeValueWriter

```csharp
public sealed class CompiledThemeValueWriter
{
    public void BeginSection(int index);
    public void SetGlobal<T>(int tokenId, T value, string canonicalValue);
    public void SetLanguage<T>(int tokenId, T value, string canonicalValue);
    public void SetControlGlobal<T>(int controlId, int tokenId, T value, string canonicalValue);
    public void SetOwn<T>(int controlId, int tokenId, T value, string canonicalValue);
}
```

`WriteStaticDefaults` 按声明的段序逐段 `BeginSection`，再为该段的每个声明写一次值：方法种类、`controlId`、段与 `typeof(T)` 都必须与
声明一致，每个 `tokenId` 恰好写一次；漏写、多写、跨段或跨 Control 写入都抛 `ThemeSchemaException`。值经 `TokenValueCodec<T>.Copy`
复制到本次候选，writer 不暴露资源发布能力。

`canonicalValue` 是构建期 codec 从类型化值生成的规范字符串，不是原始 XML 文本；动态 XML 路径使用同一规范。它进入配置与
缓存的等价协议，生成代码必须保证它与值对应。编译包不是不受信任代码的沙箱：手写实现必须遵守上述契约，不允许捕获应用对象
或伪造生成证据。

写出的结果保留四类写入各自的阶段——全局（种子 / Map / Alias）、语言 Token、Control 的全局覆盖、控件 Token——以及条件段的条件，
不压平成一张最终值表；运行时编译器按求值单元的有效变体匹配条件段、按 v2 的分层规则应用它们。

## 5. 构建契约元数据

Core 声明以下 assembly attribute（同一程序集可重复），记录版本固定为 `1`：

```csharp
ThemeBuildOwnerAttribute(int version, string catalog, string id, string ownerAssemblyIdentity,
    string ownerMetadataName, string identityProvider, string schemaDigest)
ThemeBuildTokenAttribute(int version, string catalog, string ownerId, string name,
    string valueType, string valueTypeAssemblyIdentity, string resourceKeyType,
    string resourceKeyAssemblyIdentity, string resourceKeyMember, int stage, int valueContractVersion)
ThemeBuildLanguageAttribute(int version, string id, string languageTokenType, string languageTokenAssemblyIdentity)
CompiledThemeManifestAttribute(int version, string packageId, string data)
```

- `ThemeBuildOwnerAttribute` / `ThemeBuildTokenAttribute`：控件包为每个控件与每个 Token 写一条，全局 Token 的 `catalog` / `ownerId`
  为空。`schemaDigest` 是规范排序、字段边界明确的 SHA-256，不能代替运行时的注册表校验。
- `ThemeBuildLanguageAttribute`：语言包为它声明的每个设计语言写一条（身份与语言 Token 类），语言 Token 本身作为
  `ThemeBuildTokenAttribute` 记录。编译主题包的生成器据此确认 `Language` 由某个被引用的语言包声明，并绑定 `<LanguageTokens>`。
  风格、密度、形态、可调种子不在构建契约里，它们在启动时由 `Bind` 对照已启用的语言检查（第 2 节）。
- 这些记录写在生成的 `GeneratedThemeBuildContracts.g.cs` 中，随程序集传给引用方；引用方的生成器从直接引用的元数据读取，
  不加载项目 DLL，也不反射 internal Token。非法或重复的记录是构建错误（例如 `Duplicate language build contract: <Id>`）。
- `CompiledThemeManifestAttribute`：每个编译主题包导出一份有界纯数据清单（版本 `1`，Base64），记录每个主题依赖的 Control
  owner 与 Token（scope、名称、值类型、阶段、资源键、值契约版本）。应用的启动链接在构建期对照实际启用的包与 Schema 校验
  这份清单，运行时不扫描 attribute。

## 6. 构建项与生成

```xml
<ItemGroup>
  <AtomUIThemeDefinition Include="Themes/Brand.theme.xml" />
</ItemGroup>
```

- `AtomUIThemeDefinition` 显式列出源文件，可附 `Link` 作为逻辑路径（绝对路径必须带相对 `Link`）。
- `GenerateAtomUICompiledThemeInputs` 目标在每次构建时运行：用权威的 v2 Reader 与 XSD 校验同一份有界字节快照，生成版本化的
  中立输入（`*.atomuithemeinput`，作为 AdditionalFile 交给生成器），带原始 SHA-256、来源位置、规范值与类型化 primitive。
  中立输入的格式版本为 `8`（标记 `atomui-theme-reader-xsd-v2`），是构建工具私有协议，不进入运行时；旧版本输入直接拒绝
  （`Compiled theme input format is out of date; rebuild the project.`）。
- 生成器对照构建契约绑定每个主题：未声明的语言、未知或重复的 Control、未知 Token、控件 Token 与全局 Token 同名、`Locked` 用在
  非种子或条件段 / Control 上、条件段里的 Control 固定轴、没有静态字面量能力的 Token 类型、字面量转换失败，都在 XML 的行列上
  报 `ATOMUITOKEN002`，该主题不生成程序；包内重复的主题 `Id` 同样报错。
- 生成程序只声明并写入静态值；运行时由 ThemeCompiler 在有效配置下执行语言推导、控件推导和快照生命周期。

Core 的内置 codec 拥有字面量解析和规范格式，构建任务源链接相同实现。FontFamily 字面量生成标准公开构造，构造出的 URI、名称与
fallback 顺序必须与动态 XML 一致。
