# 主题算法枚举化设计

## 1. 背景与目标

迁移前，AtomUI 虽已公开成员为 `Default`、`Dark` 和 `Compact` 的 `ThemeAlgorithm` 枚举，主题系统仍使用
字符串表示算法身份。字符串从 `ThemeConfig`、主题 XML 和 source generator 元数据进入系统，随后继续流经
descriptor、registry、normalizer、ThemeState 和 cache key。这使编译器无法检查算法名称，也允许运行时构造
实际上不受支持的算法 ID。

本设计把算法集合定义为封闭枚举。当前只支持三种算法；以后增加算法时扩展 `ThemeAlgorithm`，同时增加实现、
注册信息、XML schema 成员和测试。主题 XML 的 `Id` 文本只属于序列化边界，Reader 读取完成后，主题系统中的
算法身份必须全部使用 `ThemeAlgorithm`。

设计目标：

- 公开配置 API 使用 `ThemeAlgorithm`，不再接受或返回算法字符串。
- XML Reader 输出的 document model 已使用枚举，Binder 不再解析字符串。
- descriptor、attribute、registry、normalized config、state 和 cache key 使用枚举身份。
- generator 从枚举 attribute 参数生成枚举 descriptor，不建立字符串算法注册路径。
- 未知、大小写错误和数字形式的 XML 算法值在读取边界失败并返回带位置的诊断。
- 非法强制转换得到的枚举值在编程式配置和 schema 注册边界被拒绝。
- 保持算法链的顺序、继承、Control custom chain 和主题编译语义不变。

本次不包含：

- 第三方任意字符串算法 ID 或运行时动态算法发现。
- 为旧字符串 API 提供兼容重载。
- 改变 `Default`、`Dark` 或 `Compact` 的计算行为。
- 改变 token、appearance、主题继承或 Control algorithm policy 的语义。

## 2. 方案选择

采用端到端枚举方案。仅把公开 Config 改为枚举会保留内部字符串身份，不能建立单一身份模型；使用自定义强类型
ID 虽可保留开放扩展，但不符合当前封闭算法集合和通过扩展枚举新增算法的要求。

`ThemeAlgorithm` 是算法身份的唯一规范类型。字符串只允许存在于以下边界：

- XML 属性文本及其 XSD 表示。
- 诊断消息和日志格式化结果。
- source generator 最终生成的 C# 源文本。

这些字符串不得存入读取后的主题 document、runtime config、descriptor、registry、state 或 cache identity。

## 3. 公共契约

### 3.1 枚举稳定性

`ThemeAlgorithm` 使用显式数值：

```csharp
public enum ThemeAlgorithm
{
    Default = 0,
    Dark = 1,
    Compact = 2
}
```

后续成员只能追加并分配新的显式值，不能重排、复用或修改已有数值。枚举数值参与 schema revision、fingerprint 和
cache identity，必须视为稳定契约。

### 3.2 Config 与 Builder

公开 API 使用枚举：

```csharp
public IReadOnlyList<ThemeAlgorithm>? ThemeConfig.Algorithms { get; }
public IReadOnlyList<ThemeAlgorithm>? ControlThemeConfig.Algorithms { get; }

public ThemeConfigBuilder WithAlgorithms(params ThemeAlgorithm[] algorithms);
public ControlThemeConfigBuilder WithAlgorithms(params ThemeAlgorithm[] algorithms);
```

`null`、空集合、算法顺序及 Control `Custom` 模式的现有语义保持不变。Builder 继续防御性复制输入；Normalizer
使用 `Enum.IsDefined` 拒绝非法枚举值，并继续拒绝重复算法。

不保留 `params string[]` 重载。这样可确保调用方迁移后不存在继续传播算法字符串的入口，调用方式变为：

```csharp
new ThemeConfigBuilder()
    .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark)
    .Build();
```

### 3.3 注册契约

`ThemeAlgorithmAttribute` 的首个参数从 `string id` 改为 `ThemeAlgorithm algorithm`，公开属性从 `Id` 改为
`Algorithm`。`ThemeAlgorithmDescriptor` 同样使用 `ThemeAlgorithm Algorithm`，不再暴露字符串 `Id`。

这是一项有意的 Public API 破坏性变更。项目内算法声明、生成器测试桩、Gallery 示例和所有调用方已一次性迁移，
不增加 obsolete 字符串兼容层。

## 4. XML 读取边界

### 4.1 Schema

Theme Definition XML v1 保留当前可读格式：

```xml
<Algorithms>
  <Algorithm Id="Default" />
  <Algorithm Id="Dark" />
</Algorithms>
```

XSD 新增封闭的算法枚举类型，并让 `Algorithm/@Id` 使用该类型。允许值严格为 `Default`、`Dark` 和
`Compact`。新增 `ThemeAlgorithm` 成员时必须同步 XSD；schema 与 CLR 枚举不一致由测试阻止。

XML 名称采用区分大小写的枚举成员名。`dark`、`Unknown`、空值和数字文本 `1` 均无效，不能通过
`Enum.TryParse` 的数字兼容行为进入系统。

### 4.2 Reader 与 document model

`ThemeDocumentReader` 在读取 `Algorithm/@Id` 时完成严格转换。`ThemeAlgorithmDocument` 保存：

```text
ThemeAlgorithm Algorithm
ThemeSourceLocation Location
```

读取成功后的 `ThemeDocument` 不保留原始算法字符串。原始文本只可用于当前读取错误的 path/diagnostic；一旦返回
成功 document，字符串生命周期结束。

Reader 继续负责 XML/XSD/结构错误。Binder 接收枚举并通过枚举键查询 registry，负责报告“枚举受支持但当前
schema 未注册实现”的错误。这样可区分非法序列化值与缺失算法实现。

## 5. 运行时数据流

改造后的主题文件路径为：

```text
XML Algorithm/@Id text
    -> XSD validation
    -> strict ThemeAlgorithm parse
    -> ThemeAlgorithmDocument.Algorithm
    -> ThemeSchemaRegistry[ThemeAlgorithm]
    -> ThemeAlgorithmDescriptor.Algorithm
    -> BoundThemeDefinition / NormalizedThemeConfig
    -> compiler / snapshot / ThemeState / cache keys
```

编程式配置路径为：

```text
ThemeConfigBuilder.WithAlgorithms(ThemeAlgorithm[])
    -> ThemeConfig.Algorithms
    -> ThemeConfigNormalizer validation
    -> ThemeSchemaRegistry[ThemeAlgorithm]
    -> normalized config / compiler / snapshot / ThemeState / cache keys
```

具体内部变更：

- `ThemeSchemaRegistry` 使用 `FrozenDictionary<ThemeAlgorithm, ThemeAlgorithmDescriptor>`，按枚举数值稳定排序并
  检测重复枚举身份。
- `TryGetAlgorithm` 接受 `ThemeAlgorithm`。
- `ThemeAlgorithmDescriptor` 以 `Algorithm` 参与 schema revision 和 cache identity。
- `AlgorithmCacheKey` 保存 `ThemeAlgorithm`、revision 和 appearance effect。
- `ThemeState.Algorithms` 返回 `IReadOnlyList<ThemeAlgorithm>`。
- ThemeManager 从 effective descriptors 发布 `descriptor.Algorithm`，不调用 `Id` 或 `ToString()` 构建状态。
- fingerprint/hash 使用枚举值；仅诊断显示时格式化枚举名称。

`BoundThemeDefinition` 和 `NormalizedThemeConfig` 仍可保存 descriptor 链，因为编译需要 factory、revision 和
appearance effect；descriptor 的身份字段必须是枚举。这不属于保留第二套算法身份。

## 6. Source Generator

Generator 从 `ThemeAlgorithmAttribute(ThemeAlgorithm, revision, appearanceEffect)` 的 typed constant 获取算法枚举
值，验证该值对应一个已声明枚举成员，并生成：

```csharp
new ThemeAlgorithmDescriptor(
    ThemeAlgorithm.Dark,
    1,
    ThemeAppearanceEffect.Dark,
    static () => new DarkAlgorithm())
```

Generator 的语义模型不再把 attribute 参数解释为任意算法 ID。生成 C# 本身必然由文本 writer 输出，但生成结果
中的 descriptor 参数是枚举表达式，运行时不生成或保存算法字符串。

算法 descriptor 重复检测由 registry 按枚举键完成。未来增加枚举成员但遗漏实现时，使用该算法的主题会在 Binder
或 Config normalizer 中得到未注册诊断，而不是退回字符串查找。

## 7. 错误处理与兼容性

错误边界如下：

| 输入 | 结果 |
| --- | --- |
| XML `Id="Default"` | Reader 输出 `ThemeAlgorithm.Default` |
| XML `Id="dark"` / `Id="Unknown"` / `Id="1"` | Reader/XSD 失败并保留 source location |
| Config 中 `(ThemeAlgorithm)999` | Normalizer 返回 invalid algorithm diagnostic |
| Descriptor 中 `(ThemeAlgorithm)999` | Descriptor 或 registry 构造失败 |
| 同一链重复枚举 | 保持现有重复算法诊断 |
| 枚举合法但 descriptor 未注册 | Binder/Normalizer 返回 not registered diagnostic |

使用字符串 Builder、读取字符串 `Algorithms` 或构造字符串 descriptor 的调用方源码无法编译。
这是预期迁移信号。由于当前版本分支正在进行 6.0 架构调整，不提供双 API 过渡期。

## 8. 文档与 Gallery

同步更新主题系统文档和 Theme Definition XML 文档：

- 明确 `ThemeAlgorithm` 是封闭身份集合。
- 说明 XML `Id` 必须与枚举成员名严格一致。
- 说明新增算法需要同时扩展枚举、实现/attribute、XSD 和测试。
- 把 Gallery API 说明中的“算法标识”调整为“算法枚举值”。
- Gallery、测试及示例中的 `WithAlgorithms("Dark")` 全部改为枚举调用。

不修改 XML 示例中的可读算法文本，因为它属于正式序列化格式。

## 9. 测试与验收

实施遵循 TDD，先增加或迁移会失败的契约测试，再修改生产代码。覆盖范围：

- `ThemeDocumentReaderTests`：三种合法成员、非法大小写、未知值、数字值、读取后 document 为枚举。
- XSD 测试：XSD 允许值与 `ThemeAlgorithm` 成员集合完全一致。
- `ThemeDefinitionBinderTests`：按枚举绑定、合法但未注册算法诊断。
- `ThemeConfigNormalizerTests`：枚举链、空链默认值、重复值、非法枚举值、Control custom chain。
- `ThemeSchemaRegistryTests`：枚举键查询、重复键和非法枚举拒绝、revision 稳定性。
- `ThemeSchemaGeneratorTests`：attribute 枚举输入和生成的枚举 descriptor 表达式。
- cache/compiler/state 测试：算法顺序、revision、appearance 与枚举身份继续参与 equality/hash。
- Gallery 与 Desktop tests：公开 API 迁移后主题切换行为不变。

验证顺序：

```bash
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore
git diff --check
```

验收条件：主题文件读取成功后，除序列化、诊断和 source-code emission 外，`src` 与 Gallery 运行时代码中不存在以
`string` 表示算法身份的属性、参数、集合、字典键或 cache key；现有三种算法组合和主题切换行为保持不变。
