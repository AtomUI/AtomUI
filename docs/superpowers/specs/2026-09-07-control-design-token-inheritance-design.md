# Control Design Token 继承架构设计

> 历史记录：本文涉及的旧控件注册/裁剪方案已被 [TypeMap 正式架构](../../architecture/foundations/aot-and-trimming.md)替代，不再作为维护约束。其余内容保留当时的设计上下文。


> 当时记录的状态：2026-09-07 已获用户确认，待按实施计划执行。
>
> 范围：仅适用于 `AbstractControlDesignToken` 下的 Control Own Token，不包含 Global DesignToken、Seed/Map/Alias Token。

## 1. 结论

AtomUI 支持 Control Design Token 的单继承定义复用。`[ControlDesignToken]` 继续作为唯一标记，由 CLR 类型形态表达角色：

- 标记的 `abstract` 类是可复用、无 identity 的非终端 Token 层；
- 标记的 `sealed` 具体类是绑定 Control identity 的终端 Token；
- Generator 沿终端继承链收集每个显式标记抽象层的属性，在生成期扁平化为终端 Control 独立的 Own Token schema；
- 继承只复用源码定义与默认计算，不继承 identity、配置、snapshot 或资源作用域；
- 运行时不保存或遍历 Token 继承关系，不新增反射、动态构造或基类 fallback。

终端重写 `CalculateTokenValues` 时遵循标准 C# 语义：继承链中的后续层必须将
`base.CalculateTokenValues(isDarkMode)` 作为第一条可执行语句且只调用一次。Generator 对缺失、重复、条件性调用和错误参数转发执行构建期诊断。

## 2. 目标与非目标

目标：

- 允许 Desktop、Mobile 和第三方 Control 包复用平台无关的 Control Own Token 属性与计算；
- 保留每个终端 Control 的 exact CLR type、identity、descriptor、资源键和配置隔离；
- 支持同程序集、跨程序集和 NuGet metadata reference 中的抽象 Token 层；
- 保持直接终端的现有 schema 和运行时行为不变；
- 属性从终端移动到抽象层时，在名称、类型和终端 identity 不变的前提下保持相同 schema revision；
- 保持生成结果确定、AOT-safe，并在构建期拒绝歧义结构。

非目标：

- Global DesignToken 继承；
- 多重继承、Token fragment 组合、interface schema 或泛型 Token；
- Token 属性 override、virtual property 或 `new` 隐藏；
- identity、配置、snapshot 或资源作用域继承；
- 运行时程序集扫描、Attribute 读取、Token 基类遍历或 identity fallback；
- Desktop/Mobile 同名 identity 的并存策略；
- 自动执行每层 evaluator 的新运行时 pipeline；
- 在没有两个真实复用方时提前创建抽象 Token。

## 3. 类型与 Identity 模型

### 3.1 统一 Attribute

不增加 `ControlDesignTokenBase`、`ControlDesignTokenDefinition` 或第二种 Attribute。现有 Attribute 保持：

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ControlDesignTokenAttribute : Attribute
{
}
```

`Inherited = false` 是硬约束。继承链每一层都必须主动声明 `[ControlDesignToken]`，Generator 不把 Attribute 的继承结果当作 opt-in。

### 3.2 两种角色

抽象复用层示例：

```csharp
[ControlDesignToken]
internal abstract class CommonButtonToken : AbstractControlDesignToken
{
    public double ContentHeight { get; set; }
    public Thickness ContentPadding { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        ContentHeight = EffectiveGlobalToken.ControlHeight;
        ContentPadding = EffectiveGlobalToken.InputPadding;
    }
}
```

抽象层只贡献属性与默认值计算，不匹配 Control，不拥有 identity、slot、factory、descriptor、资源键或 Registration Unit。它可以独立编译、被多个终端复用并形成多层单继承链；必须非泛型且名称以 `Token` 结尾。

终端层示例：

```csharp
[ControlDesignToken]
internal sealed class DesktopButtonToken : CommonButtonToken
{
    public BoxShadows DefaultShadow { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        DefaultShadow = EffectiveGlobalToken.BoxShadows;
    }
}
```

终端必须具体、`sealed`、非泛型，继续按当前约定匹配 public Control，是链中唯一拥有 identity 的类型，并生成完整 schema、factory、evaluator、资源键和注册产物。具体 `[ControlDesignToken]` 不能作为另一 Token 的基类。

合法形态：

```text
AbstractControlDesignToken
  └── [ControlDesignToken] abstract CommonInteractiveToken
        └── [ControlDesignToken] abstract CommonButtonToken
              ├── [ControlDesignToken] sealed DesktopButtonToken
              └── [ControlDesignToken] sealed MobileButtonToken
```

终端也可以直接继承 `AbstractControlDesignToken`。除根类型外，终端与根之间每个中间层都必须是显式标记的抽象 Token。

### 3.3 Identity 与值隔离

抽象层没有 `ControlTokenIdentity`，终端各自拥有 identity：

```text
CommonButtonToken  → no identity
DesktopButtonToken → DesktopButtonTokens.Identity
MobileButtonToken  → MobileButtonTokens.Identity
```

不得从抽象 Token 名称、CLR 基类、Control 基类或资源位置推导 identity。未注册 exact Control type 时继续失败，不回退到基类 Control 或 Token identity。

两个终端继承同一属性时，各自拥有 descriptor、slot、配置和值：

```csharp
theme.ControlTokens
    .For(DesktopButtonTokens.Identity)
    .Set(DesktopButtonTokens.ContentPadding, desktopPadding);

theme.ControlTokens
    .For(MobileButtonTokens.Identity)
    .Set(MobileButtonTokens.ContentPadding, mobilePadding);
```

一个终端的 override、ThemeContext 或 snapshot 不影响 sibling 终端。

## 4. 生成与运行模型

### 4.1 扁平终端 Schema

Generator 把终端继承链展开为单个 Own Token schema：

```text
DesktopButtonToken
  inherited: ContentHeight, ContentPadding
  declared:  DefaultShadow

DesktopButton OwnTokens
  ContentHeight
  ContentPadding
  DefaultShadow
```

运行时 descriptor 不记录声明层、继承深度或抽象 Token 类型。继承元数据仅用于生成期诊断。

现有运行时流程保持不变：factory 直接构造终端 Token，`ThemeCompiler` 注入 Effective Global Token，evaluator 调用终端虚方法，随后应用 Own Token override 并投影资源。原则上不修改 `ControlTokenDescriptor`、`ThemeCompiler`、`ThemeSchemaRegistry`、`ThemeSnapshot`、`ThemeTokenResourceProvider`、`ControlTokenAccessor` 或 `TokenResourceBinder`；若实现需要运行时继承查找，必须暂停并重新评审。

### 4.2 Generator 流水线

Generator 继续用 `ForAttributeWithMetadataName` 发现当前 compilation 中的源码类型：

```text
[ControlDesignToken] abstract        → validate reusable layer
[ControlDesignToken] sealed concrete → terminal candidate
[ControlDesignToken] concrete open   → error
```

抽象层独立编译时只验证结构、属性和计算方法，不匹配 Control，也不生成运行时产物。终端处理顺序为：

```text
discover terminal
  → validate base chain
  → resolve matching Control
  → collect each layer's declared properties
  → validate properties and calculation chain
  → flatten and sort schema
  → generate terminal descriptor/resources/registration
```

结构或属性失败后不生成部分 schema；可以继续报告同阶段的独立错误，但要抑制无效模型引起的级联诊断。

### 4.3 继承链与属性收集

从终端 `INamedTypeSymbol` 沿 `BaseType` 遍历到准确的 `AbstractControlDesignToken`：

- 根以外每层自身都有 `[ControlDesignToken]`；
- 中间层为 `abstract`，终端为非抽象 `sealed`；
- 每层都是顶级类，且自身及 containing type 链均非泛型；
- 链中没有具体中间 Token 或未标记普通类。

每层只读取直接声明的属性。合法属性必须是实例、非 indexer，普通 getter/setter 均可由终端程序集的生成代码访问，未标记 `[NotTokenDefinition]`，不是 `init`-only、`required`、显式接口实现、virtual、abstract、override 或 `new` 隐藏属性。值类型继续服从现有 `TokenValueConverterRegistry` 运行时契约，本次继承生成器不新增 converter 白名单诊断。

使用 `Dictionary<string, FlattenedControlTokenProperty>` 检测名称冲突，不依赖对象相等的 `HashSet` 静默去重。扁平属性模型至少保存名称、值类型、原始声明层和 location、终端类型、是否继承；声明层只用于诊断。生成 delegate 统一转换为终端类型：

```csharp
static token => ((DesktopButtonToken)token).ContentPadding;
static (token, value) =>
    ((DesktopButtonToken)token).ContentPadding = (Thickness)value!;
```

以下情况构建失败：不同层同名、派生层 `new` 隐藏、同名不同类型、与任一 Global Token 同名，或错误 symbol 合并导致重复收集。完整合并后按名称以 `StringComparer.Ordinal` 排序并分配连续 slot；源码顺序、继承深度、声明程序集和遍历顺序不影响结果。

### 4.4 增量与跨程序集

跨程序集基类的属性名称/类型、`[NotTokenDefinition]`、abstract/concrete、generic、Attribute 或 `CalculateTokenValues` 签名变化，必须使下游终端生成结果失效。测试使用真实 metadata reference，不能只在同一 syntax tree 模拟。

抽象基类的方法体由其自身程序集的 Generator 验证。下游可从 metadata 验证结构与签名，但不重新证明已编译的方法体；下游源码 override 仍执行完整语义检查。

## 5. 默认值计算链

当某层声明 `CalculateTokenValues` override 且位于复用链的后续层时，方法必须：

1. 使用 block body；
2. 第一条可执行语句调用 `base.CalculateTokenValues(isDarkMode)`；
3. 原样转发当前参数；
4. 整个方法体仅调用一次直接 base；
5. 在 base 返回后计算本层属性。

不允许将 base 调用放入条件、异常块、循环、lambda 或局部函数，也不允许在调用前写入 Token。当前层未 override 时正常继承基类实现。

直接继承 `AbstractControlDesignToken` 的终端可以不调用根类型空实现；第一层抽象 Token 重写根方法时同样不要求。后续层再次 override 时必须调用直接基类。

## 6. 诊断契约

所有继承契约诊断使用 `Error` severity。

| ID | 含义 |
| --- | --- |
| `ATOMUIGEN013` | 继承链无效：未标记中间层、具体中间层或没有到达根类型 |
| `ATOMUIGEN020` | 具体终端 `[ControlDesignToken]` 未 `sealed` |
| `ATOMUIGEN021` | 抽象层或终端 Token 使用泛型 |
| `ATOMUIGEN022` | 继承属性同名、隐藏或形成重复 schema key |
| `ATOMUIGEN023` | 属性形态无效：indexer、不可访问访问器、`init`-only、`required`、virtual/abstract/override 等 |
| `ATOMUIGEN024` | `CalculateTokenValues` base 调用缺失、重复、非首条或参数错误 |
| `ATOMUIGEN025` | 抽象 Token 名称不符合 `*Token` 约定 |
| `ATOMUIGEN026` | Control Token 声明为嵌套类型；位于泛型容器时仍使用 `ATOMUIGEN021` |

现有职责保持：`ATOMUIGEN012` 表示终端名称不能产生合法 Control 名称，`014` 表示没有匹配 public Control，`015` 表示匹配多个 Control，`019` 表示扁平 Own Token 与 Global Token 同名。

诊断定位到最接近的可修改声明：终端或定义层类型、中间基类、冲突成员或 override 方法。metadata 基类没有源码 location 时，消息包含完整 metadata type name。

## 7. 跨包、兼容性与 AOT

### 7.1 跨程序集与 ownership

- `public abstract` Token 是第三方可继承的正式 API；
- `internal abstract` Token 可通过正常 `InternalsVisibleTo` 供 AtomUI 下游包使用；
- Generator 不绕过 C# accessibility；
- public 抽象 Token 的属性名称、类型和默认计算语义属于兼容性契约。

共享抽象 Token 放在最低且语义正确的共同上游：跨产品 Control/primitive 的视觉语义放在 `AtomUI.Controls` 对应源码附近；Desktop-only、Mobile-only 语义留在各自产品包；Control 产品语义不下沉到 `AtomUI.Core`，`AtomUI.Controls.Shared` 也不因代码复用接管视觉主题语义。

共享下沉必须至少有两个真实终端使用，名称、语义和默认计算依赖一致，平台差异可由终端表达，并能接受共享属性变更影响所有派生终端。

抽象 Token 不拥有 Registration Unit、linked registration fragment、factory 或动态 root。终端 factory 的静态类型引用自然保留 CLR 基类链；独立 NuGet 抽象 Token 包是普通程序集依赖，不通过运行时扫描加载。

### 7.2 Desktop/Mobile identity 边界

当前 identity 由 Catalog 与 ControlName 组成。Desktop 和 Mobile 同名 Control 同时注册时可能冲突，但继承设计不改变 identity 规则：产品包互斥注册时保持现状；若未来需要并存，另行设计产品包 identity 命名空间，不用 Token 基类、CLR namespace 或 runtime fallback 规避。

### 7.3 Schema 兼容

已是 `sealed` 且直接继承根类型的终端，其 descriptor、identity、slot、Token 名称/类型、resource key、factory、evaluator、schema revision 和 AXAML 用法保持不变。

属性从终端移到抽象层时，只要终端类型、名称、类型、stage、排序、resource key 和 identity 不变，schema revision 必须相同。fingerprint 不包含声明层、继承深度、抽象类型名或抽象层程序集。

所有 internal 非 sealed 终端机械增加 `sealed`。公开的具体 `ColorPickerToken` 也必须封闭，这是 Public API breaking change：在目标主版本迁移并写入 breaking API/release 文档；第三方复用改为继承公开抽象层，不保留非 sealed 终端兼容壳。

### 7.4 性能与 AOT

单终端生成复杂度目标为 `O(inheritance depth + flattened property count)`；同一抽象层可按 `INamedTypeSymbol` 在单次生成中缓存验证结果。

运行时仍为每个终端创建一个 builder，descriptor 数量等于扁平 Own Token 数量。抽象层不产生 descriptor、snapshot 层或资源字典，不增加主题订阅、资源查找分支、运行时继承遍历或 warmed lookup 分配。

生成代码继续使用 `new TerminalToken()` 和静态强类型 delegate。禁止生成 `PropertyInfo`、`GetProperties`、`BaseType` runtime 遍历、`Activator.CreateInstance`、`MakeGenericType`、runtime Attribute 读取或 Token 层类型数组。

## 8. 验证矩阵

结构与属性：

- 直接 sealed 终端保持旧生成结果；
- abstract Token 可独立编译且不生成 identity；
- 单层、多层、共享基类和跨程序集终端成功；
- 非 sealed、泛型、未标记/具体中间层、错误根类型失败；
- 继承属性、`[NotTokenDefinition]`、accessibility 与现有 converter 运行时契约正确；
- indexer、virtual/override/new、重复名称、Global 冲突失败；
- slot 和生成源码顺序确定。

计算与隔离：

- 正确首条 base 调用及无 override 场景成功；
- 缺失、重复、条件性、非首条、错误参数和 expression body 失败；
- Light/Dark 参数贯穿多层，基类值先计算，终端继续派生；
- 用户 override 在完整默认计算后应用；
- sibling 终端配置和值互不影响。

跨程序集与 schema：

- 建立真实 `SharedTokens.dll -> PlatformControls.dll` metadata fixture，覆盖 public、internal+IVT、增量失效和抽象包无注册产物；
- 比较“属性全在终端”和“部分移入抽象层”，名称、类型、slot、resource key、schema revision 完全一致。

产品验证：Generator focused tests、Core theme tests、相关 Control tests、生成源码无反射断言、跨程序集 fixture trimmed build、Gallery Desktop NativeAOT publish 和启动 smoke，以及 `git diff --check`。

## 9. 实施分期与文件范围

### Phase 1：契约与 RED 测试

重写现有继承诊断测试，建立 abstract/sealed 分类；添加继承链、属性、计算链、跨程序集 fixture，并固定直接终端生成结果与 schema 等价基线。

### Phase 2：Generator 实现

将 `ControlTokenPropertyWalker` 改为终端继承链解析，扩充 `ControlTokenInfo`，增加诊断并保持 writer/runtime descriptor 结构；验证直接终端无 schema 变化。

### Phase 3：封闭现有终端

所有具体 `[ControlDesignToken]` 增加 `sealed`。internal 类型机械迁移，公开 `ColorPickerToken` 单独审查并记录 breaking change；此阶段不同时抽取共享基类。

### Phase 4：按真实复用提取抽象层

每次只迁移一个真实 Control 家族，先建立 schema 等价测试再移动属性与计算。Desktop/Mobile 共享层放入正确共同上游；Mobile 尚未实现的 Control 不提前创建空抽象 Token。

核心修改范围：

- `src/AtomUI.Core/Theme/DesignTokens/ControlDesignTokenAttribute.cs`
- `src/AtomUI.Generator/DesignToken/ControlTokenPropertyWalker.cs`
- `src/AtomUI.Generator/DesignToken/TokenInfo.cs`
- `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticIds.cs`
- `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticDescriptors.cs`

可能适配但不应改变运行时契约：`ControlThemeInfo.cs`、`GeneratedThemeSchemaWriter.cs`、`ResourceKeyClassWriter.cs`。主要测试在 `AtomUI.Generator.Tests`、`AtomUI.Core.Tests/Theme`、受影响 Control tests 和新增 metadata fixture。

文档同步：`control-token-guidelines.md`、theming runtime、第三方 Control package 指南、Mobile Theme/Token 架构，以及 `ColorPickerToken` breaking API/release 文档。

## 10. 验收标准

- abstract `[ControlDesignToken]` 可独立存在且不生成 identity；
- sealed `[ControlDesignToken]` 可继承一层或多层显式标记 abstract Token；
- 跨程序集和第三方包继承可用，继承属性完整进入终端 schema；
- Generator 在构建期验证默认值计算链；
- 直接终端 schema 不变，属性移入抽象层不改变 schema revision；
- 所有具体终端 Token 均为 `sealed`；
- 无运行时反射、扫描、继承遍历或基类 identity fallback；
- 配置、snapshot 和资源值按终端 identity 完全隔离；
- Generator、Core、相关 Control、AOT publish 和启动 smoke 均通过。
