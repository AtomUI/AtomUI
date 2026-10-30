# AtomUI Control Token 设计规范

本文档定义 `AtomUI.Controls`、`AtomUI.Desktop.Controls`、DataGrid、ColorPicker 和第三方 AtomUI Control 包的
Control Token、ControlTheme Asset 与组合主题规则。完整编译、snapshot 和资源发布模型见
[AtomUI 主题系统架构](../../architecture/systems/theming/runtime.md)。单个 Control 的 `token.md` 只记录 Own Token 语义和必要的
Global Token 使用示例；全部 Global Token 都可覆盖，不在单 Control 文档中复制支持清单。面向主题作者的完整定制流程见
[主题定制指南](../../guides/theming/customization.md)。Control Own Token 的定义继承、终端封闭、跨程序集复用和扁平 schema
契约见 [Control Design Token 继承架构](../../architecture/systems/theming/control-design-token-inheritance.md)。

控件注册采用逐控件 TypeMap 目标契约；架构采纳与源码迁移状态统一见
[AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md)。本文的 owner、片段和资产规则是新实现必须满足的
要求，不表示当前源码已经完成迁移。完整注册模型见 [控件注册契约](../../architecture/foundations/control-registration-contracts.md)。

## Token 分层

```text
Global Token
    -> Control-specific Global Override
Effective Global Token
    -> calculate Control Own Token defaults
    -> Control Own Token Override
Effective Control Token
    -> ControlTheme
    -> Semantic Part Theme
    -> Template / Runtime visual state
```

| 层 | 职责 |
| --- | --- |
| Global Token | 定义全局颜色、尺寸、排版、动效、交互值和算法输入。 |
| Effective Global Token | 保存 Global Token 在某个 Control identity 下的有效值。 |
| Control Own Token | 定义只属于一个 Control 的稳定设计语义。 |
| Effective Control Token | 合并该 Control 的 Effective Global Token 与 Own Token。 |
| Theme Variables | 保存 Control 实例在当前状态下的最终视觉变量。 |
| ControlTheme / Template | 消费 Token 和 Theme Variables，完成状态映射与渲染。 |

## Control identity

每个对外可主题化的 Control 都拥有独立 `ControlTokenIdentity`。identity 不由以下信息推断：

- `ControlTheme.TargetType`。
- Control CLR 继承关系。
- ControlTheme `BasedOn` 关系。
- internal Part、Presenter、Decorator 或 Host 类型。
- 运行时资源位置或 Visual/Logical parent。

生成器根据正常 Control/Token 关联生成 `XxxTokens.Identity`，其中保存真实 owner Type。正常 C# 配置和 imperative
查询使用这一强类型入口。字符串 Catalog/Id 或 `new ControlTokenIdentity(...)` 只用于配置数据和查找，匹配已经保留的
schema，不承担动态保留职责；不能以裸字符串代替已知 Control 的类型化入口。

identity 的相等性、hash、`==` 和 `!=` 仍只比较 Catalog/Id；owner Type 是额外的可验证数据，不能通过自动生成的
record equality 改变字符串身份语义。配置、RequiredTokenOwners 和其他 identity-keyed 合并必须在去重前校验：两侧
owner 不同则失败；仅一侧带 owner 时保留带 owner 的 canonical identity。不能只替换 Dictionary value 而遗失原 key
中的 owner 信息。最终 registry 再检查 owner 与真实 descriptor 一致。

Catalog 是包级 identity 命名空间，不是 CLR namespace，也不是主题资产的 TargetType。AtomUI 内置包统一使用
`AtomUI`；第三方包的生成 target 默认使用项目 `AssemblyName`，本包声明的 Token key 和 descriptor 使用本包 Catalog，
跨包资源依赖保留各 Token owner 已有的 Catalog。主题资产属于声明它的包；`ExportedThemes.TargetType` 不直接产生
Token identity，也不改变已有 owner Catalog。资产使用 `RequiredTokenOwners` 表达所需 identity 与实际 owner Type，
不再用单一资产 owner identity 混合表示主题导出与 Token 依赖。

## Own Token 定义

```csharp
public sealed class Rating : TemplatedControl
{
}

[ControlDesignToken]
internal sealed class RatingToken : AbstractControlDesignToken
{
    public double StarSize { get; set; }
    public double StarGap { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        StarSize = EffectiveGlobalToken.ControlHeight;
        StarGap  = EffectiveGlobalToken.UniformlyMarginXS;
    }
}
```

约束：

- `RatingToken.cs` 只在 Rating 存在 Own Token 时创建。
- Own Token 定义层和终端类型都使用无参数 `[ControlDesignToken]` 标记，供生成器稳定发现；Attribute 不隐式继承，
  继承链每一层都要显式标记。
- `[ControlDesignToken]` 不携带 Control 类型、identity、ID 或主题资产路径；Control 关联继续由命名和目录约定建立。
- 标记的 `abstract` 类型是无 identity 的定义复用层；标记的具体类型是匹配 Control 的终端 Token，必须 `sealed`。
- 终端可以直接继承 `AbstractControlDesignToken`，也可以继承一层或多层显式标记的抽象 Token；不能继承具体 Token，
  也不能在链中插入未标记类型。
- 抽象层和终端都必须是非泛型顶级类；Token 属性禁止 virtual、abstract、override、indexer、`new` 隐藏、`init`-only setter 和
  `required`。
- Own Token 禁止与任一 Global Token 同名。
- 禁止泛型 `[ControlDesignToken<TControl>]`、手写 ID 或 Theme Asset glob。
- 拥有公开可配置 Token 契约的 Control 即使没有 Own Token，也拥有 identity，并可覆盖完整 Global Token schema。
  仅有资源的内部 presenter 不因此获得虚构的 Token identity。

多个 Control 需要相同值时，先判断它是否属于全局设计语言。属于时提升为 Global Token；不属于时分别定义语义
清晰的 Own Token。只有至少两个真实终端共享同一 Control 视觉语义、默认计算依赖一致且平台差异仍可由终端表达时，
才提取无 identity 的抽象 `[ControlDesignToken]` 层；不能为了可能的复用提前建立空继承层。

抽象层可以位于另一个程序集。对第三方开放时使用 `public abstract`，仅供已知第一方包复用时可以使用
`internal abstract` 与正常的 `InternalsVisibleTo`。跨 Desktop/Mobile 的产品视觉语义放在最低且正确的共同上游
`AtomUI.Controls`，不因复用下沉到 `AtomUI.Core` 或 `AtomUI.Controls.Shared`。

### 默认值计算继承

Generator 只为终端生成 evaluator，默认计算使用 C# virtual dispatch。继承链后续层 override
`CalculateTokenValues(bool isDarkMode)` 时，第一条可执行语句必须且只能调用一次
`base.CalculateTokenValues(isDarkMode)`，随后再计算本层属性。base 调用不能是条件性的，也不能转发其他参数。

直接继承 `AbstractControlDesignToken` 的终端和直接重写根方法的第一层抽象 Token 不要求调用根类型空实现；某层没有
override 时正常继承基类实现。定义 public 抽象 Token 的包必须启用兼容版本 Generator，使方法体契约在产生 metadata
前得到验证。

## 可配置 Token 契约

Control 配置允许使用的 Token 为：

```text
Configurable Control Tokens = All Global Tokens + Current Control Own Tokens
```

`ControlTokenDescriptor` 只保存 exact CLR type、identity 和 `OwnTokens`，不保存 Global Token 消费白名单。Binder
先匹配当前 Control Own Token，未命中时匹配完整 Global Token schema；两处都不存在才报错。Own Token 与任意
Global Token 禁止同名，因此扁平配置和统一 `XxxTokenResource` 不需要来源限定符。

## Global Token 消费不进入 Schema

Own Token 计算可以直接读取任意 Effective Global Token：

```csharp
StarSize = EffectiveGlobalToken.ControlHeight;
```

ControlTheme 也可以直接使用任意 Global Token：

```xml
<Setter Property="Foreground" Value="{atom:RatingTokenResource ColorPrimary}" />
```

生成器不扫描这两处访问，也不扫描 C# Binding 来形成依赖清单。合法 Global Token 即使没有被当前 Control 消费也
允许配置；它可能没有视觉效果，但不能泄漏到其他 Control 或 Global snapshot。`Global`/`Custom` Control 算法
可以从一个局部 Seed/Map 覆盖派生其他 Token，这是当前 Control 内的正常间接效果。

## 用户配置

```csharp
theme.ControlTokens
    .For(RatingTokens.Identity)
    .Set(GlobalTokens.ControlHeight, 36)
    .Set(GlobalTokens.ColorPrimary, Colors.Red)
    .Set(RatingTokens.StarGap, 8);
```

前两个值只覆盖 Rating 的 Effective Global Token，最后一个值覆盖 Rating Own Token。其他 Control 和真正的
Global Token snapshot 不受影响。名称未出现在完整 Global Token schema 或 Rating Own Token schema 时产生明确
错误；合法但未消费的 Global Token 允许没有实际效果。

## AXAML 访问契约

```xml
<!-- 永远读取全局值。 -->
<Setter Property="FontFamily" Value="{atom:SharedTokenResource FontFamily}" />

<!-- 读取 Rating 的 Effective Global Token。 -->
<Setter Property="MinHeight" Value="{atom:RatingTokenResource ControlHeight}" />
<Setter Property="Foreground" Value="{atom:RatingTokenResource ColorPrimary}" />

<!-- 读取 Rating Own Token。 -->
<Setter Property="Margin" Value="{atom:RatingTokenResource StarGap}" />
```

`RatingTokenResource` 同时读取两类 Token，消费语法不写 `Global=` 或 `Own=`。生成器产生强类型
`RatingTokenKey` 和 MarkupExtension 构造参数。主题作者可以选择 Rating Own Token 和 Global Token schema 中的
全部候选；错误名称在 AXAML 编译期失败，运行时不解析字符串。

生成扩展的 Global 与 Own 分支都携带实际 owner 的资源键。直接 boxed Token enum 仍属于支持的资源键形式；Control、
TokenResourceExtension、TokenKey 和 OwnTokenKind 的使用都应保留对应注册片段。把 enum 转为裸整数或仅做常量计算
不代表使用控件资源。Token-only、生成 identity-only 的代码不能依赖 owner 控件稍后才被构造来补注册。

选择规则：

- 希望某值永远跟随当前 ThemeContext 的全局结果时使用 `SharedTokenResource`。
- 希望用户能够在 Rating 配置下局部覆盖时使用 `RatingTokenResource`。
- ControlTheme 不声明 `ControlTokenScope.Identity`，也不存在 ambient fallback。

## C# 访问契约

普通 Global Token 与当前 Control 的 Effective Global Token 必须使用不同 API：

```csharp
TokenResourceBinder.CreateGlobalTokenBinding(
    this, ForegroundProperty, SharedTokenKind.ColorPrimary);

TokenResourceBinder.CreateControlTokenBinding(
    this, ForegroundProperty, SharedTokenKind.ColorPrimary);
```

内部对象由公开 owner 使用 `owner/target/targetProperty/token` 重载绑定。命令式读取使用
`ControlTokenAccessor.Capture(owner)`，同一次计算只读取一次 snapshot。生成 descriptor 提供 exact CLR type 到
identity/slot 的 AOT-safe 映射；基类 Effective Global 读取使用注册派生类型的 identity，Own Token 不随继承替换。
未注册 exact type 必须失败，不能回退到基类 identity，也不能使用隐藏 StyledProperty 转运 Token。

## ControlTheme Asset

标准 Control 的最小结构：

```text
Rating/
+-- Rating.cs
+-- RatingToken.cs            optional
\-- Themes/
    \-- RatingTheme.axaml
```

多个主题资产直接增加文件：

```text
Themes/
+-- DatePickerTheme.axaml
+-- DatePickerPresenterTheme.axaml
\-- RangeDatePickerTheme.axaml
```

构建集成把 `Themes/**/*.axaml` 提供给普通生成器，输出资产 factory、导出的主题 key/TargetType、RequiredTokenOwners、
资源作用域/显式 include、Semantic Part 契约和注册片段。主题导出与 Token owner 是独立事实，不能用文件目录或单一
OwnerIdentity 替代它们。
开发者不编写 `ControlThemeAssets` Attribute、Theme Module、手工 manifest、逐主题 identity 或只用于聚合的额外
AXAML。只有一个 ControlTheme 时就只有一个主题文件。

主题和资源作者必须遵循：

- 只有真正导出的顶层主题建立 Control → asset 关联；嵌套模板 TargetType 和局部定制主题不产生反向默认主题归属。
- 自定义 `SearchButtonTheme` 不能使普通 Button 使用反向保留 SearchEdit；顶层命名主题则按其真实导出保留。
- 同一字典导出多个主题时保留完整资产，由实际 factory 引用进入链接闭包；多个片段引用同一 AssetId 只挂载一次。
- StaticResource 在本地、词法父作用域和明确 include 内解析。不同私有字典的同名 key 可以合法并存，不能全局合并或
  按名字猜依赖。未在声明范围内提供的必需包内 StaticResource 必须诊断。
- 开放宿主 DynamicResource 作为普通外部主题输入，不自动保留控件。无关、未 include 的私有同名 key 不足以判定冲突。
- include 顺序、包提交顺序和包内明确资源优先级在 full/trimmed 中保持一致；先收集，全部包完成后再校验和挂载。
  链接器可达性循环与资源构造循环必须区分，后者不能交给 TypeMap 修复。

## Semantic Part 与 Semantic Part Theme

Semantic Part 的基础契约是 `.semantic-*` Selector、稳定 ContractType 和生成式 descriptor，完整设计见
[AtomUI Semantic Part 系统设计](../../architecture/systems/theming/semantic-parts.md)。Token 只表达 Control 的稳定设计值，不为
每个 Part 创建独立 Token identity，也不使用模板节点名称扩展 Token schema。

每个 Control 的语义 descriptor 只有一个生成 factory，完整注册和 TypeMap 片段共用。生成的专用 Semantic Style 也必须
成为 owner 片段的保留条件，不能假设 route 字符串会使 owner 自动可达。Semantic registry 与 Token schema 一起在
首个控件创建前冻结；Style 构造函数不得追加注册，示例继续通过专用 Style 声明式定制。

当某个 Part 是真实 public Control，并且允许用户完整替换其 ControlTheme 时，可以额外通过强类型
`ControlTheme?` 属性开放 Semantic Part Theme：

```csharp
public ControlTheme? SearchButtonTheme { get; set; }
```

Semantic Part Theme 是 `SelectorAndTheme` Part 的可选扩展，不是所有 Semantic Part 的默认机制。它不创建 Token
identity，不使用字符串 Part 名称或 `Dictionary<string, ControlTheme>`。Part 继续使用自己的 identity；Part Theme
可以显式读取 owner 和 Part 的 TokenResource。

SearchEdit 模板直接组合 public Button：

```text
SearchEditTokenResource -> 输入区域、拼接边框、圆角、状态投射和搜索语义
ButtonTokenResource     -> Button 基础尺寸、字体、颜色和交互视觉
SearchButtonTheme       -> 稳定的搜索按钮定制入口
```

不能创建一个 TargetType 属于 SearchButton、基础视觉属于 Button、Token scope 又属于 LineEdit 的 internal
Control。组合关系不能通过借用 Token identity 表达。

## Theme Variables 与命名

Own Token 表达稳定 Control 语义，例如 `DefaultHoverColor`、`PrimaryShadow`、`ContentFontSizeLG`。不要使用
`Panel1Padding`、`FrameBorderPointerOverBrush` 等模板节点名称，也不要展开颜色、variant 和状态的笛卡尔积。

`IsPressed`、`IsPointerOver`、`IsLoading`、模板节点可见性和实例 API 解析结果属于运行时状态。需要通过 Selector
参与状态映射的最终变量使用 internal StyledProperty Theme Variables，不进入 Control Token。

## 第三方 Control 包

第三方按相同约定提供 Control、可选 Own Token 和主题资产，并只调用一次生成的包级注册入口：

```csharp
builder.UseAcmeControls();
```

作者提供普通 Provider，并通过生成 helper 表达正常入口和必要生命周期回调。完整与选中注册共用逐项 descriptor、Token、
Semantic 和资产 factory，不运行时扫描程序集或 AXAML。作者不手写 TypeMap、AOT 分支、入口身份 Attribute、裁剪名单、
逐 Theme 注册代码、泛型 Token Attribute 或 glob Attribute。

第三方与第一方采用同一逐控件/真实资产契约；包和目录都不是保留粒度。SDK 自动交付普通生成器和发布后端，应用引导
仅读 Package marker。仅引用包不会自动创建 Provider；缺包、错误 owner 或不兼容后端必须明确失败，不能整包兜底。

完整项目与入口示例见
[第三方 AtomUI Control Package 指南](../../guides/theming/third-party-control-packages.md)。

## 诊断与启动校验

以下问题在普通包编译中可静态证明时必须构建失败：

- Own Token 与 Global Token 同名。
- 具体 `[ControlDesignToken]` 未 `sealed`，或作为另一 Token 的基类。
- Token 继承链包含泛型、未标记中间层、具体中间层或没有到达 `AbstractControlDesignToken`。
- 继承属性同名、隐藏、形态无效，或形成重复 schema key。
- `CalculateTokenValues` 的直接 base 调用缺失、重复、不是第一条可执行语句或未原样转发参数。
- TokenResource 引用了不存在或归属错误的 Token。
- Control、Token、主题资产和 identity 约定存在歧义。
- 冲突 identity、资产/主题导出定义，或必需包内资源无法按声明作用域解析。
- Semantic Part Theme 的 TargetType 与属性契约不兼容。
- Semantic Part 的 ContractType 与模板 marker 类型不兼容，或 Part 名称、class、cardinality 与 descriptor 不一致。
- internal 实现类型被错误声明为独立 Token owner。

配置规范化必须在身份去重前检查 owner 冲突。跨包依赖在全部入口返回后统一校验；缺少 RequiredTokenOwners、未注册
exact CLR type、无效配置名称、资源与语义契约不完整时启动失败，不能回退到基类 identity 或追加全包注册。
新注册诊断的语义与预留状态见 [编译期诊断规范](compiler-diagnostics-guidelines.md)，不沿用旧 LINK ID。

## 验证要求

| 改动类型 | 验证要求 |
| --- | --- |
| 新增 Control | 验证 identity、可选 Own Token、语义/主题片段、包入口和无运行时发现路径；内部资源控件不伪造 Token owner。 |
| 修改 Own Token | 验证强类型 key、默认计算、override 和 AXAML 消费。 |
| 修改 Own Token 继承层 | 验证继承链诊断、属性扁平化、base 计算顺序、跨程序集增量失效、sibling 隔离和 schema revision 等价。 |
| 修改 Control 对 Global Token 的使用 | 验证 Effective Global 覆盖、Global fallback、算法间接派生和跨 Control 隔离。 |
| 修改 Semantic Part | 验证 selector marker、ContractType、cardinality、模板变体、Style-only 保留与 descriptor 一致性。 |
| 修改 Semantic Part Theme | 额外验证 owner/Part Token 分工、TargetType 和默认/实例替换。 |
| 修改生成器 | 验证确定性、增量构建、Token/identity-only 保留，以及 NativeAOT、trimmed CoreCLR、Browser 裁剪运行和 Browser AOT。 |
| 修改主题视觉 | 验证 Light/Dark、Control 算法和相关 Control 家族。 |

涉及 identity 合并时覆盖 raw→typed、typed→raw、正确 typed→错误 typed 及反序，确认 owner 校验不被 Dictionary 去重吞掉。
涉及资源时同时验证共同资产优先级与未用资产删除；独立原型通过不能替代真实产品宿主验收。
