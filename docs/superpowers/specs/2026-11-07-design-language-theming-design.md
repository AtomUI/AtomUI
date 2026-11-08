# 多设计语言主题架构设计

日期：2026-11-07。状态：设计草案第三版，待用户审阅；产品实现未启动。

基线：`release/6.0`（`ae5ac0a32`，AtomUI 6.2.13，Avalonia 12.1.5）。分支 `feature/custom-theme` 的全部改动不作为参考，本文不复用其设计、术语或实现。

版本演进：第一版"Ant 基线 + 按语言补丁样式"被否决（Ant 特殊化、控件目录膨胀、运行时改选择器）。第二版改为"一份控件代码、N 套对等 ControlTheme、独立主题包"，但把设计语言与主题定义当作正交轴。第三版按用户定义改为层级模型：**设计语言是主题定义之上的更广概念；主题定义在语言之下调色与微调，不涉及 ControlTheme；设计语言涉及。** 同版补充：Token 派生策略（算法、Alias 计算、缺口填充）归语言所有，派生式（Ant、Material）与直接指定式（PrimeNG 类）走同一条管线（3.5）。

## 1. 目标

让 AtomUI 应用在运行时切换多种设计语言（Ant Design、Material、shadcn、Bootstrap……），效果对齐 ant.design 首页 "Flexible theme customization" 的预设切换器：同一棵控件树，不重建、控件属性上的状态不丢，切换后整体外观变为另一套设计语言；同一应用内允许不同子树使用不同设计语言。

成功标准：

1. Gallery 增加一个与 ant.design 预览区等价的"设计语言预览"页，至少提供 Ant、Material、shadcn、Bootstrap 四套，切换在一次 UI 线程 Publish 内完成，输入框文字与选区、选中项、焦点保持。
2. 每种设计语言是独立 NuGet 包；不引用不占产物，引用未激活只登记一张表，激活后只物化页面上出现过的控件主题。
3. 第三方控件包与应用可以为任一设计语言提供自己的 ControlTheme、主题定义与 Token，不修改 AtomUI 源码。
4. 不依赖 Avalonia 私有成员、不 hook 框架内部、不修改 Avalonia；仅使用公开且在 12.1.5 源码中核对过的行为。
5. 对 `release/6.0` 的改动，除新增能力外只允许三类：简化代码、提升性能、架构优化；每一处在第 9 节登记理由。
6. 机制与平台无关：桌面与未来的移动控件包共用同一套定义与注册协议。

## 2. 参考实现：ant.design 的切换器是怎么做的

来源：`ant-design/.dumi/pages/index/components/ThemePreview/previewThemes/*.ts`（master）。14 个预设全部是普通 `ConfigProvider` 配置对象：

| 机制 | 用途 | 使用预设数 |
| --- | --- | --- |
| `theme.token` | 全局 Token（颜色、圆角、字体、控件高度……） | 14/14 |
| `theme.components` | 逐组件 Token | 14/14 |
| `theme.algorithm` | 明暗算法 | 14/14 |
| 逐组件 `classNames` / `styles` | 对语义 DOM 区域注入 CSS（渐变、硬投影、backdrop blur、霓虹阴影、全大写、自定义 ripple） | 7/13 |
| 结构/模板替换、图标替换、`cssVar`、`componentSize` | 无 | 0 |

预览区 JSX 固定；切换只改 class 与注入的 CSS，预览子树整体重挂载。结论：**一个预设 = 语言的视觉层 + 它自己的色板 + 逐组件 Token**。antd 的视觉层是运行时生成的 CSS；Avalonia 里视觉层就是 ControlTheme，是编译产物。AtomUI 的 Token 层照抄 antd（`ThemeConfig` 已经是它），视觉层按 Avalonia 的方式交付：一个语言一套 ControlTheme。

## 3. 核心模型

### 3.1 层级：设计语言 ⊃ 主题定义 ⊃ 运行时配置

```text
Design language   Ant                Material              Shadcn            Bootstrap
  视觉层          默认 ControlTheme   Material ControlTheme  （回落 Ant）       （回落 Ant）
  基础 Token      无                  高度/圆角/字体/阴影…    zinc 色系/圆角…    主色/字号/高度…
  默认主题        DaybreakBlue        MaterialPurple         ShadcnZinc        BootstrapBlue
  主题定义        DaybreakBlue        MaterialPurple          ShadcnZinc        BootstrapBlue
  （语言专属）    DustRed …           MaterialTeal …          ShadcnSlate …     …
  通用主题        任何只调色板的主题定义，在任何语言下可用（含用户目录里的主题）
  Token 派生      语言自己的算法集与 Alias 计算：Ant 是派生式，PrimeNG 类语言是直接指定式（3.5）
  变体            Default / Dark / Compact 是请求用的变体 id；每个语言声明自己支持哪些、各自怎么算
  运行时配置      ThemeRequest.Config、局部 ThemeConfigProvider.Config
```

| 概念 | 拥有什么 | 不拥有什么 |
| --- | --- | --- |
| 设计语言 | ControlTheme 集；Token 派生策略（算法集、Alias 计算、缺口填充）；语言基础 Token；逐控件 Token 默认；默认主题 id | 色板选择；用户配置 |
| 主题定义 | 一组 Token 值（色板为主）；所属语言（或通用）；Appearance 元数据 | 任何 ControlTheme、模板、样式 |
| 运行时配置 | Token/控件/算法覆盖；局部作用域的语言切换 | 新的 Token 字段或控件 identity |

利弊评估：

- 利：优先级不再需要裁决（语言基础 < 主题 < 用户配置，"主题是对语言的细化"天然成立）；与 antd 预设的心智一致（预设 = 语言 + 它自己的色板）；目录与 UI 按语言分组、每个语言有默认主题；语言专属主题可以调只在该语言下有意义的 Token，通用主题只碰色板，边界清楚。
- 弊：切换语言时当前主题可能不属于新语言，需要一条确定的回退规则（3.3）；`ThemeRequest`、`ThemeInfo`、跟随系统模板、Catalog 都要带语言，不兼容旧请求；主题 XML v1、编译主题包、用户目录要认可选的 `DesignLanguage` 属性，并在引用未安装语言时给诊断；局部作用域换语言套同一条回退规则。这些都只是多一条确定的规则，没有新增机制，故采用。

### 3.2 Token 合并顺序

```text
library defaults
  → design language 基础 Tokens
  → design language 逐控件 Token 默认
  → theme definition Tokens             （所属语言的专属主题，或通用主题）
  → root ThemeRequest.Config
  → 局部 ThemeConfigProvider.Config     （沿父链继承；Inherit=false 回到"语言 + 语言默认主题"基线）
```

现有 `ThemeConfigMerger` 增加两层输入，不新建配置对象。合并得到的是每个 Token 的**显式值**；显式值按 Stage 注入 3.5 的派生管线：Seed 值在算法前写入，Map 值在算法后写入，Alias 值在 Alias 计算后写入。这是 `ThemeCompiler.CompileGlobalBuilder` 今天的顺序，不变。第二版的 `DarkTokens` 合并层取消：暗色在派生式语言里是算法，在直接指定式语言里是 Dark 变体的值表，统一归 3.5。

### 3.3 语言与主题的组合与回退

- 根请求必须给出语言；主题 id 可省略，省略即该语言的默认主题。
- 语言专属主题只能在其所属语言下使用；在别的语言下请求它，规范化阶段 `Failed` 并给诊断。
- 只换语言（主题 id 省略）时：当前主题是通用主题则沿用；是语言专属主题则换成新语言的默认主题。回退结果写进 `ThemeState`，调用方可见。
- 局部 `ThemeConfigProvider` 指定语言但未指定主题时套同一规则；Popup、独立窗口沿现有资源链继承，不另设规则。
- 用户目录的主题 XML 可声明 `DesignLanguage`；声明了未注册的语言时该主题进入 Catalog 诊断、不可选，不静默变通用。
- 用户目录不得定义设计语言，只能定义主题；设计语言是编译交付的程序集。

### 3.4 激活模型：Theme 属性走作用域资源

每个作用域的 `ThemeContext` 已持有有效配置和唯一的 Token ResourceProvider。只给这个 Provider 增加一种可回答的键：

```text
DesignLanguageThemeKey(controlType)
    -> 当前作用域语言在该类型上注册的 ControlTheme（惰性物化）
    -> 未注册：不命中（Avalonia 置为 Unset，Theme 回到 null，走隐式默认主题）
```

ThemeManager 启动时按注册表为每个导出了类型键默认主题的控件类型建立一条样式并挂入自己的 `Styles`：

```text
Style OfType<T>  { Setter(StyledElement.Theme, DynamicResource(DesignLanguageThemeKey(T))) }
```

- `Theme` 是普通 StyledProperty，样式可以设置；用户显式设置的本地值优先级更高，仍然赢。
- DynamicResource 在目标控件上实例化、沿目标控件的资源链解析，天然按作用域取值；局部 `ThemeConfigProvider`、Popup、带 owner 的独立窗口全部沿用现有资源链，不加新机制。
- 切换语言就是一次普通事务：Publish 阶段 Provider 发出 `ResourcesChanged`，每个控件的 Theme 绑定重新取值；只有主题实例真的变化的控件会重套模板，只换 Token 的语言零重套。

不需要可继承附加属性、PropertyEquals、改选择器或按语言挂卸样式。

### 3.5 Token 派生策略归语言所有

`release/6.0` 的全局 Token 管线是 Ant 的：25 个 Seed 经算法派生 112 个 Map（`Default/Dark/Compact` 三个计算器，闭合枚举，由 Core 全局注册），再由 `DesignToken.CalculateAliasTokenValues` 固定公式算出 118 个 Alias。这套"计算而得"是 Ant Design 的设计选择，不是所有设计语言的共性：Material 3 用 HCT 色调板派生，PrimeNG 的预设是直接写满 primitive/semantic/component 三层值，没有派生算法。本设计把派生策略交给语言：

```text
ThemeDerivation（每个语言一份）
  Algorithms      变体 id → IThemeAlgorithm 工厂          Ant：Default/Dark/Compact 三个现有计算器
                                                            Material：HCT 调色板实现的 Default/Dark，Compact 可不支持
                                                            PrimeNG 类：Core 提供的 TokenTableAlgorithm，一张值表一个变体
  AliasCalculator Map → Alias                              Ant：现有 CalculateAliasTokenValues
                                                            直接指定式：TokenTableAliasCalculator，或直接复用 Ant 的
  GapFill         语言没给出的 Map/Alias 怎么办              Fail（默认，启动时列出缺口）| FillFrom(Ant)
```

管线只有一条，两种语言只是算法实现不同：

```text
seed  = library defaults + 显式 Seed 值
map   = language.Algorithms[请求变体…].Evaluate(seed)      // 直接指定式：从值表复制
        + 显式 Map 值
alias = language.AliasCalculator(map) + 显式 Alias 值
```

规则：

- `ThemeAlgorithm` 从闭合枚举改为开放标识符（字符串 id），`Default`、`Dark`、`Compact` 只是约定好的常量；语言可以注册自己的变体 id（例如 Material 的 `HighContrast`）。请求中的变体 id 在当前语言的 `Algorithms` 中解析，语言不支持的 id → `Failed` 并给诊断；`DesignLanguageInfo.SupportedAlgorithms` 供 UI 禁用不可用开关。
- 外观仍由算法描述符的 `AppearanceEffect` 决定；直接指定式语言的 Dark 值表声明 `AppearanceEffect.Dark`，与今天一致。
- 直接指定式语言的"值表"就是 Map/Alias 阶段的一组 Token 值；基础值表写在语言定义里，色板变体写成该语言专属的主题定义 XML（Token 可以是任意 Stage，现有 XML v1 已支持）。因此 PrimeNG 式的"一个预设 = 一张完整表"直接对应"一个语言 + 一个主题定义"。
- 启动时对每个语言跑一次覆盖检查：用库默认 Seed 走它的 `Default` 链与 Alias 计算，仍为未指定的 Map/Alias Token 按 `GapFill` 处理。`Fail` 让作者在构建或启动时看到完整缺口清单；`FillFrom(Ant)` 用 Ant 的派生补齐未指定项，适合只想覆盖语义色、其余沿用 Ant 推导的语言（shadcn、Bootstrap 即如此）。
- Control Own Token 的派生（各控件 `CalculateTokenValues`）仍以语言产出的全局 Token 为输入，是所有语言共享的默认推导；语言通过定义里的 `Controls` 静态覆盖具体值，语言的 ControlTheme 则自行决定消费哪些 Token（例如 Material 按钮阴影直接消费全局阴影层级而不是 Ant 的 `PrimaryShadow`）。不为语言提供替换控件级派生函数的入口；需要时再立设计。
- `NormalizeFinalBehavior`（动效开关等最终行为规范化）与语言无关，保持在管线末尾。

### 3.6 语言扩展 Token（本轮不实现）

Material 的色调角色（`primaryContainer`、`onPrimary`、五级 elevation）在 Ant 的全局 schema 里没有一一对应。首批实现要求语言把自己的概念映射到现有全局 Token（`primaryContainer → ColorPrimaryBg`、`onPrimary → ColorTextLightSolid`、elevation → 三级 BoxShadow），并把映射不了的概念列成清单。清单出来后再决定是否允许语言包在启动注册期扩展全局 schema（生成 `MaterialTokenResource`、只供该语言的 ControlTheme 消费）；那会要求运行期追加的语言不得携带扩展 Token，因为 schema 启动后冻结。

## 4. 公开 API

全部位于 `AtomUI.Core`，命名空间 `AtomUI.Theme`；资源键位于 `AtomUI.Theme.Resources`。

```csharp
public static class DesignLanguageIds
{
    public const string Ant = "Ant";
    public const string Material = "Material";
    public const string Shadcn = "Shadcn";
    public const string Bootstrap = "Bootstrap";
}

public readonly record struct ThemeAlgorithm(string Id)      // 原闭合枚举改为开放 id
{
    public static readonly ThemeAlgorithm Default = new("Default");
    public static readonly ThemeAlgorithm Dark    = new("Dark");
    public static readonly ThemeAlgorithm Compact = new("Compact");
}

public interface IThemeAliasCalculator { void Calculate(DesignToken map); }

public sealed class ThemeDerivation
{
    public static ThemeDerivation Ant { get; }                                   // Core 内置：三个现有计算器 + 现有 Alias 公式
    public static ThemeDerivation Tables(ThemeTokenTable @default, ThemeTokenTable? dark = null,
        ThemeTokenTable? compact = null);                                        // 直接指定式
    public ThemeDerivation WithAlgorithm(ThemeAlgorithmDescriptor descriptor);   // 覆盖或新增变体
    public ThemeDerivation WithAliasCalculator(IThemeAliasCalculator calculator);
    public ThemeDerivation FillGapsFrom(string languageId);                      // 默认 Fail
    public IReadOnlyDictionary<ThemeAlgorithm, ThemeAlgorithmDescriptor> Algorithms { get; }
    public IThemeAliasCalculator AliasCalculator { get; }
    public string? GapFillLanguage { get; }
}

public sealed class DesignLanguageDefinition
{
    public DesignLanguageDefinition(string id, string displayName, string defaultThemeId,
        ThemeDerivation derivation, ThemeConfig? tokens = null,
        IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig>? controls = null);
    public string Id { get; }
    public string DisplayName { get; }
    public string DefaultThemeId { get; }      // 必须是该语言专属或通用的已注册主题
    public ThemeDerivation Derivation { get; }
    public ThemeConfig? Tokens { get; }        // 任意 Stage 的显式值；Algorithms/Controls/Inherit 非默认时构造失败
    public IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig> Controls { get; }
}

// 一个程序集为某个语言贡献的 ControlTheme 集，由生成器产出；手工构造也允许。
public sealed class DesignLanguageThemeSet
{
    public DesignLanguageThemeSet(string languageId, IEnumerable<ControlThemeAssetDescriptor> themeAssets);
    public string LanguageId { get; }
    public IReadOnlyList<ControlThemeAssetDescriptor> ThemeAssets { get; }   // TargetType 即键
}

public interface IThemeManagerBuilder
{
    void AddDesignLanguage(DesignLanguageDefinition definition);
    void AddDesignLanguageThemes(DesignLanguageThemeSet themes);
    void WithInitialTheme(string designLanguage, string? themeId = null, ThemeConfig? config = null);
    void WithFollowSystemThemes(ThemeRequest light, ThemeRequest dark);   // 不变，请求已带语言
    // 其余不变
}

public sealed record ThemeRequest(
    string DesignLanguage,
    string? ThemeId,                 // null = 该语言默认主题，或按 3.3 回退
    ThemeConfig? Config,
    ThemeTransitionReason Reason);

public sealed record ThemeInfo(string Id, string Name, ThemeAppearance Appearance, bool IsDefault,
    string? DesignLanguage /* null = 通用 */, Color? AccentColor);

public sealed record DesignLanguageInfo(string Id, string DisplayName, string DefaultThemeId,
    IReadOnlyList<ThemeAlgorithm> SupportedAlgorithms, int ThemedControlCount);

public interface IThemeManager
{
    IReadOnlyList<DesignLanguageInfo> AvailableDesignLanguages { get; }
    IReadOnlyList<ThemeInfo> AvailableThemes { get; }       // 不变；ThemeInfo 多了 DesignLanguage
    void AddDesignLanguage(DesignLanguageDefinition definition, params DesignLanguageThemeSet[] themes); // 运行期追加，5.4
    event EventHandler? DesignLanguagesChanged;
    // 其余不变
}

public sealed class ThemeConfig        { public string? DesignLanguage { get; } /* 局部作用域切语言；根请求忽略 */ }
public sealed class ThemeConfigBuilder { public ThemeConfigBuilder WithDesignLanguage(string? id); }
public sealed record ThemeState        { public string DesignLanguage { get; } /* ThemeId 为回退后的实际主题 */ }
```

主题 XML v1 增加可选根属性：

```xml
<Theme xmlns="https://atomui.net/schemas/theme/v1" Id="MaterialTeal" Name="Material Teal"
       Appearance="Light" DesignLanguage="Material">
```

省略 `DesignLanguage` 即通用主题。`IsDefault` 改为由语言定义的 `DefaultThemeId` 决定，XML 上不再声明。编译主题包与用户目录 Resolver 读取同一属性。

AXAML 侧新增一个标记扩展，供语言主题派生默认主题：

```xml
<ControlTheme x:Key="{x:Type atom:Button}" TargetType="atom:Button"
              BasedOn="{atom:DefaultControlTheme atom:Button}">
    <Setter Property="CornerRadius" Value="{atom:ButtonTokenResource BorderRadiusLG}" />
</ControlTheme>
```

`DefaultControlTheme` 在主题物化时同步从注册表取默认主题实例；物化只发生在激活之后、UI 线程上，此时注册表已冻结。

不提供：运行时从文件加载样式、JSON/XML 承载样式、配方对象、AI 生成入口。

## 5. 运行时

### 5.1 注册表

`DesignLanguageRegistry`（ThemeManager 持有）：

- `Definitions`：`Id → DesignLanguageDefinition`，重复 Id 启动失败；`DefaultThemeId` 必须解析到该语言专属或通用的主题，否则启动失败。
- `Themes`：`(Id, Type) → DeferredControlThemeResourceProvider`，复用现有惰性 provider；同一 `(Id, Type)` 两次注册启动失败。
- 主题集引用了没有定义的 Id：启动失败并指出缺少哪个包。
- `Ant` 由 Core 内置：`ThemeDerivation.Ant`，空基础 Token，默认主题 `DaybreakBlue`，主题集就是各控件包现有的类型键导出，不重复登记。
- 算法描述符随语言登记：`[ThemeAlgorithm]` 标注的计算器由生成器归入所在语言包；Core 的三个计算器归 `Ant`。不再有全局算法表。
- 启动时按 3.5 对每个语言做覆盖检查；`GapFill = Fail` 且有缺口 → 启动失败并列出缺失 Token。
- Catalog（`CompiledThemeCatalog`）为每个主题定义记录 `DesignLanguage`，并提供"语言 → 可用主题"的分组查询。

### 5.2 一次切换的事务路径

沿用现有 Capture / Prepare / CommitCore / Publish / Complete，不加阶段：

1. **Capture**：按 3.3 解析 `(语言, 主题)`，并把请求与控件级 `Custom` 算法的变体 id 在该语言的 `Algorithms` 中解析为描述符；未注册语言、专属主题跨语言、未知主题 id、语言不支持的变体 id 在 Normalizer 失败，`Failed` 返回诊断。
2. **Prepare**（可后台）：Normalizer 按 3.2 合并；编译器用该语言的派生策略（3.5）产出 Snapshot。缓存键含语言、派生策略修订与变体链，自然区分。不物化任何 ControlTheme。
3. **CommitCore**（UI 线程，无通知）：替换 Snapshot 引用，不变。
4. **Publish**（UI 线程）：既有的 Token 资源通知同时驱动 Token 绑定与 Theme 绑定重新取值；Theme 绑定首次命中新语言的某个类型时才物化该 ControlTheme。`RequestedThemeVariant`、事件不变。
5. **Complete**：`ThemeState` 记录实际生效的语言与主题。

### 5.3 Provider 回答 Theme 键

`ThemeTokenResourceProvider.TryGetResource` 增加一个分支：键为 `DesignLanguageThemeKey` 时，取 `Snapshot.EffectiveConfig.DesignLanguage`，在注册表查 `(语言, 键.ControlType)`；命中返回物化后的 ControlTheme，未命中返回 `false`。键对象按类型缓存为单例，保证 DynamicResource 的键相等。语言为 `Ant` 时恒不命中，行为与今天完全一致。

### 5.4 按需与运行期追加

- 不引用语言包：不进产物。
- 引用未激活：只登记定义、它自带的主题定义和一张 `(Id, Type) → 工厂` 表，AXAML 不解析。
- 激活：只物化被查询过的类型；切回后实例留在惰性 provider 中，上界为"已激活过的语言 × 出现过的控件类型"。
- 运行期追加：语言包不新增 Token schema、不新增控件 identity，不碰启动时冻结的 `ThemeSchemaRegistry`，只往注册表和 Catalog 加行；`IThemeManager.AddDesignLanguage` 在 UI 线程追加并派发 `DesignLanguagesChanged` 与 `ThemeCatalogChanged`。JIT 应用可配合 `AssemblyLoadContext` 在运行中装载语言包；NativeAOT 无运行期装配加载，只用前三条。

### 5.5 与 Semantic Part、Own Token 的关系

- 语言主题是 ControlTheme，必须像默认主题一样完整实现该控件的 Semantic Part descriptor（非 Optional marker 齐全）；现有 ThemeAssets 分析器对默认主题做的校验原样作用于语言包。
- 用户的 Semantic Part Style 继续写在 `Application.Styles` / 局部 StyleHost，对任何语言的主题都生效，因为 marker 一致。
- Own Token 定义不随语言变化；语言只能改它们的默认值（定义的 `Controls`），不能新增字段。一种语言需要新的可调值时，那是给控件加 Own Token，所有语言共享。

## 6. 语言包作者约定

### 6.1 项目结构

```text
AtomUI.Themes.Material/                         一个语言一个程序集（派生式）
+-- AtomUI.Themes.Material.csproj               <AtomUIDesignLanguage>Material</AtomUIDesignLanguage>
+-- MaterialDesignLanguage.cs                   DesignLanguageDefinition：Id、显示名、默认主题、Derivation、基础 Tokens、Controls
+-- Algorithms/
|   +-- MaterialDefaultCalculator.cs            [ThemeAlgorithm("Default", 1, Light)]，HCT 调色板
|   \-- MaterialDarkCalculator.cs               [ThemeAlgorithm("Dark", 1, Dark)]
+-- Definitions/
|   +-- MaterialPurple.theme.xml                DesignLanguage="Material"，语言默认主题（Seed 值）
|   \-- MaterialTeal.theme.xml
\-- Themes/                                     与控件包同构的目录，ControlTheme
    +-- Buttons/ButtonTheme.axaml               完整主题或 BasedOn 默认主题
    +-- Switch/SwitchTheme.axaml
    \-- …

AtomUI.Themes.Prime/                            直接指定式示例
+-- PrimeDesignLanguage.cs                      Derivation = ThemeDerivation.Tables(base, dark)
+-- Tables/PrimeBaseTable.cs                    Map/Alias 值表；Dark 一张
+-- Definitions/PrimeAura.theme.xml             预设 = 专属主题定义，Token 直接写 Map/Alias 值
\-- Themes/…                                    需要换形态的控件

AtomUI.Themes.Shadcn/
+-- AtomUI.Themes.Shadcn.csproj
+-- ShadcnDesignLanguage.cs                     Derivation = ThemeDerivation.Ant.FillGapsFrom(Ant)，只覆盖语义色与圆角
\-- Definitions/ShadcnZinc.theme.xml            色板，零 AXAML，ControlTheme 回落 Ant
```

生成器规则：项目设置了 `AtomUIDesignLanguage` 时，`Themes/**/*.axaml` 中以类型为键的 ControlTheme 登记到该语言的主题集，`[ThemeAlgorithm]` 计算器登记到该语言的派生策略，`Definitions/*.theme.xml` 作为编译主题定义登记并强制 `DesignLanguage` 等于该语言；产出 `GeneratedDesignLanguageRegistration.Register(builder)`，由 `UseDesignLanguage<MaterialDesignLanguage>()` 一类的扩展方法调用。未设置该属性的程序集（控件包）行为不变，其 ControlTheme 属于 `Ant`，其 `*.theme.xml` 按自身属性决定专属或通用。

一个程序集只属于一个语言。第三方控件包要支持 Material，另建 `Vendor.Controls.Themes.Material`，引用 `AtomUI.Themes.Material` 取定义，按同一目录约定放 ControlTheme。可选控件包（DataGrid、ColorPicker、Extras）的 Material 主题同样是独立小程序集 `AtomUI.Themes.Material.DataGrid`，按需引用。

构建期校验（新增诊断号沿用 `ATOMUIGEN` 序列）：

- 语言包内 ControlTheme 必须以类型为键；`TargetType` 必须是引用的控件包中已注册的 Control。
- 同一程序集内同一 `TargetType` 只能出现一次。
- 非 Optional Semantic marker 齐全（现有校验）。
- `DefaultControlTheme` 的类型参数必须与 `TargetType` 一致。
- `Definitions/*.theme.xml` 的 `DesignLanguage` 必须等于项目的 `AtomUIDesignLanguage`。

### 6.2 控件侧契约：一份代码如何承载多套主题

控件不感知语言。语言差异分三档，各有吸收点：

| 差异档 | 例子 | 吸收点 | 对控件的要求 |
| --- | --- | --- | --- |
| 值差异 | 主色、圆角、高度、字体、线宽、阴影、间距、动效开关 | 语言基础 Token、逐控件默认、主题定义 | ControlTheme 的每个视觉值来自 Token 或控件属性，无字面量 |
| 区域差异 | 渐变、硬投影、模糊、图标尺寸与位置、hover 效果、有无 ripple | `BasedOn` 默认主题的派生 ControlTheme，加 Setter 与嵌套 Style | 可能被重绘的区域都是 Semantic Part；状态经伪类/属性暴露 |
| 结构差异 | Material Switch 圆钮在轨道外、浮动标签、Bootstrap 勾选框形状 | 完整 ControlTheme 换模板 | 代码与模板的契约只有 `PART_*` 与行为属性；可选 PART 缺失时容错 |

这正是 Avalonia Fluent / Simple 两套主题共用一份控件代码的做法。控件须满足的五条契约（逐控件二元审计，不建立字段目录）：

1. 视觉值不进代码：颜色、尺寸、圆角在模板/样式中绑定 Token 或属性；自绘控件把画刷、笔、几何参数暴露为 StyledProperty 后在 Render 中读取。
2. 模板契约最小化：代码依赖的 `PART_*` 列入控件文档；非必需 PART 允许为空；不在代码中按类型或 Name 搜索可视树做视觉赋值。
3. 状态只经伪类和属性表达，代码不直接设置 `Background`、`Foreground` 等视觉属性。
4. 几何从 Token/属性推导，不写常量。
5. Semantic marker 覆盖语言可能重绘的全部区域；语言主题保持同一组非 Optional marker。

设计语言只覆盖视觉。行为差异（浮动标签动画、通知位置规则等）不属于主题；确有需要时给控件增加所有语言可用的属性，由语言主题设默认值。

### 6.3 平台

机制在 Core，定义、主题定义与 ControlTheme 集在语言包，控件包不含任何语言文件。移动控件包出现后，`AtomUI.Themes.Material.Mobile` 按同一约定为移动控件提供 ControlTheme；触控尺寸等差异写在它的逐控件默认里，不进语言定义。iOS/Cupertino、Material 3 这类以移动为主的语言走同一模型。

## 7. Gallery

- 工作台顶栏：先选设计语言，再在该语言下选主题（语言专属 + 通用），与明暗、紧凑并列；`ApplyThemeAsync(new ThemeRequest(language, themeId, config, reason))`。
- 新增"设计语言预览"展示页：复刻 ant.design 预览区的控件集合（输入、选择、日期、开关、步骤、进度、徽标、按钮组、验证码、卡片、通知、对话框等），页面内一个 `ThemeConfigProvider` 承载预览，切换只改该 Provider 的 `Config.DesignLanguage`；同页可同时放两个不同语言的 Provider 做对照。
- Gallery 引用 `AtomUI.Themes.Material` / `Shadcn` / `Bootstrap` 三个包。

## 8. 性能、内存与 AOT

- 启动：ThemeManager 多挂一组按类型的样式（每个导出默认主题的控件类型一条，约 380 条 `OfType` 选择器，每条一个 Setter）。`OfType` 匹配是类型相等比较，每个元素套样式时多约 380 次比较。若实测可见，改为一条 `:is(Control)` 样式加自定义 `IBinding` 按目标 `StyleKey` 计算键，把比较降为零；spike 顺带测这两种的差距。
- 每个控件多一个 DynamicResource 订阅（Theme）；AtomUI 控件已有数十个 Token 订阅，增量可忽略。
- 切换：只换 Token 的语言与今天的主题切换成本相同；换主题的语言在被覆盖类型上重套模板，代价小于 ant.design 的整树重挂载。目标：Gallery 预览页（约 60 个控件）Publish 在 16 ms 内，首次激活含物化不超过 50 ms；以 Stopwatch 计。
- 内存：惰性 provider 上界见 5.4；Snapshot 缓存键含语言与主题，`LazyControlSnapshots` 不变。
- AOT：定义、主题定义与 ControlTheme 集是静态构造 + 编译产物，无反射、无 `Activator`、无字符串类型名。

## 9. 对 release/6.0 的改动登记

| 改动 | 类别 | 理由 |
| --- | --- | --- |
| `ThemeRequest` 增加 `DesignLanguage`，`ThemeId` 可空；`ThemeInfo`/`ThemeState` 携带语言；XML v1 增加 `DesignLanguage` 属性，`IsDefault` 改由语言定义决定 | 新能力所需 | 层级模型要求请求与目录都知道语言；默认主题归属语言比散落在 XML 更一致 |
| `ThemeConfig` 增加 `DesignLanguage`（仅局部作用域使用）；Normalizer/Merger 增加语言基础与逐控件默认两层 | 新能力所需 | 复用既有合并器，不新建配置对象 |
| `ThemeAlgorithm` 由闭合枚举改为开放 id；算法描述符按语言登记，不再有 Core 全局算法表 | 新能力所需 + 架构优化 | 派生策略归语言所有；Core 不再知道有哪些变体 |
| `DesignToken.CalculateAliasTokenValues` 抽为 `IThemeAliasCalculator`，Ant 实现移入 `Algorithms/`；编译器从语言取算法与 Alias 计算器 | 架构优化 | 把 Ant 专有公式从 Token 定义类里拿出来，Token 类只剩数据 |
| `ThemeTokenResourceProvider` 多回答一种键 | 新能力所需 | 复用作用域资源链，不加第二条传播机制 |
| ThemeManager 启动挂一组按类型的 Theme 样式 | 新能力所需 | 把隐式主题查找改为可随作用域变化的显式绑定 |
| `DesignLanguageRegistry`、定义/主题集类型、`DefaultControlTheme` 标记扩展、Catalog 按语言分组 | 新能力所需 | 复用 `ControlThemeAssetDescriptor` 与惰性 provider |
| 根 Context 样式选择器由 `OfType<TopLevel>()` 改为 `Is<TopLevel>().PropertyEquals(ParentProperty, null)` | 架构优化，可独立提交 | 现样式把根 Context 以样式值写到 PopupRoot 上，覆盖它从 Popup 继承的局部 Context |

不改动：事务结果枚举、Own Token 求值、资源扩展、Semantic Part 生成、`ThemeContextLease`、编译主题包的 ABI、用户主题目录的安全边界。不引入别名、兼容层或过渡 API。

## 10. 验证

单元（`AtomUI.Core.Tests`）：

- 合并顺序：语言基础 < 逐控件默认 < 主题定义 < 根配置 < 局部配置，逐对验证；显式值按 Seed/Map/Alias 阶段注入派生管线的位置正确。
- 派生策略：派生式语言的算法只在该语言下解析；直接指定式语言的值表产出与表一致、Dark 表给出 Dark 外观；`GapFill=Fail` 列出全部缺失 Token 并阻止启动，`FillFrom(Ant)` 补齐后无缺口；语言不支持的变体 id → `Failed`；`SupportedAlgorithms` 与注册一致。
- Ant 回归：`ThemeDerivation.Ant` 下全部现有主题测试结果不变（算法与 Alias 公式只是搬家）。
- 组合与回退：语言专属主题跨语言 → `Failed`；只换语言时通用主题沿用、专属主题回退到新语言默认，`ThemeState` 反映实际值；`DefaultThemeId` 不可解析 → 启动失败。
- 注册：未注册语言 → `Failed`；主题集引用无定义的 Id → 启动失败并指明缺包；同 `(Id, Type)` 重复 → 启动失败；用户目录主题声明未安装语言 → Catalog 诊断且不可选。
- Theme 键：Ant 下恒不命中；Material 下覆盖类型命中、未覆盖类型不命中；物化恰好一次；A→B→A 后实例数不变。
- 作用域：根、`ThemeConfigProvider` 子树、Popup、带 owner 的独立窗口四种宿主下 Theme 绑定取到各自作用域语言的主题。
- 运行期追加：`AddDesignLanguage` 后 `AvailableDesignLanguages`、`AvailableThemes` 更新，再切换生效。
- 泄漏：关闭窗口、移除 Provider 后 WeakReference 回收；惰性 provider 不持有控件。

集成（`AtomUI.Desktop.Controls.Tests` / Gallery 测试）：

- 每套语言 × 预览页：切换后关键区域（按钮背景/圆角/阴影、输入框边框、开关轨道、步骤条连接线）按已采用的容差策略对比各自基准。
- 状态保持：切换前后 TextBox 文本与选区、ListBox 选中项、焦点元素相同；被换模板的控件记录模板内状态丢失范围。
- 语言主题的 Semantic Part 预览能解析全部非 Optional 部件。
- 用户显式设置 `Theme` 的控件不被语言切换改写。

性能：第 8 节两项计时在 Gallery headless 宿主下由测试断言。

## 11. 实施顺序

1. **Spike（半天）**：在 `release/6.0` 上用临时样式验证并记录：样式 Setter 改变已挂载控件的 `Theme` 后实时重套模板且 `PART_*` 查找正常；DynamicResource 不命中时 `Theme` 回到 null 并恢复隐式主题；作用域 Provider 的 `ResourcesChanged` 能驱动 Popup 内与 lease 窗口内控件的 Theme 绑定；约 380 条 `OfType` 样式对 Gallery 首页套样式耗时的影响。任一不成立即停下修订本设计。
2. Core 派生层：`ThemeAlgorithm` 开放 id、`IThemeAliasCalculator`、`ThemeDerivation`、值表算法、覆盖检查；Ant 的三个计算器与 Alias 公式搬入 `ThemeDerivation.Ant`，现有主题测试全部保持通过。
3. Core 语言层：`ThemeRequest`/`ThemeInfo`/`ThemeState`/XML v1 属性、`ThemeConfig.DesignLanguage`、合并层、回退规则、注册表与 Catalog 分组、Theme 键与 Provider 分支、启动样式、`DefaultControlTheme`、运行期追加。
4. 生成器：`AtomUIDesignLanguage` 属性、主题集、算法与主题定义登记、五条校验。
5. `AtomUI.Themes.Shadcn`、`AtomUI.Themes.Bootstrap`（`Ant` 派生 + 缺口填充 + 色板主题）；`AtomUI.Themes.Material`（HCT 派生）先覆盖 Button、Input 家族、Select、Checkbox/Radio/Switch、Steps、Progress、Tag、Card、Alert、Dialog、Notification 这组预览集合；一个直接指定式的最小示例语言进测试工程，验证值表路径。
6. Gallery 切换器与预览页；验证第 10 节。
7. 扩展 Material 到其余控件与可选包；按 3.6 列出 Material 映射不了的 Token 概念清单。

每一步独立提交、独立验收；第 5 步之后即可在 Gallery 看到 ant.design 式切换效果。

## 12. 已决策事项（供审阅时推翻）

| 问题 | 决策 | 替代方案及为何不选 |
| --- | --- | --- |
| 语言与主题定义的关系 | 层级：语言 ⊃ 主题；主题要么专属要么通用 | 正交轴（第二版）：需要裁决"谁覆盖谁"，目录无法按语言分组 |
| 视觉层的形态 | 一个语言一套对等的 ControlTheme，独立主题包 | 第一版"Ant 基线 + 补丁样式"：Ant 特殊化、控件目录膨胀、运行时改选择器 |
| 语言如何到达控件 | `Theme` 属性经 DynamicResource 走作用域 Provider | 可继承附加属性 + PropertyEquals：多一条传播机制，且不能换模板 |
| 只换语言时的主题 | 通用主题沿用，专属主题回退到新语言默认 | 一律回退到默认：用户选好的色板无故丢失；一律沿用：专属主题在别的语言下无意义 |
| Token 派生归谁 | 语言拥有算法集、Alias 计算与缺口填充；一条管线，直接指定式用值表算法 | 保持 Ant 全局算法：PrimeNG 类语言只能靠覆盖 230 个值硬压 Ant 的推导，Material 的 HCT 无处安放；两条管线：编译器与缓存分叉 |
| 变体 id 的形态 | 开放字符串 id，`Default/Dark/Compact` 是约定常量 | 保留闭合枚举：语言无法声明自己的变体，也无法表达"不支持 Compact" |
| 语言扩展全局 Token | 本轮不做，先映射到现有 schema 并列缺口清单 | 现在就开放 schema 扩展：影响运行期追加与资源 ID 合同，证据不足 |
| 设计语言能否作为数据文件交付 | 不能，只能是程序集；主题定义仍可是 XML | 数据化 ControlTheme 需自建解析与校验，不 AOT 友好 |
| 主题集引用无定义的 Id | 启动失败 | 隐式造定义会让两个包对同一语言各执一词 |
| 一个程序集能否属于多个语言 | 不能 | 目录与注册都会二义 |
| 首批语言 | Material（换形态）、Shadcn、Bootstrap（只换值） | 与 ant.design 预设对齐，覆盖两类 |
| 已物化的语言主题是否回收 | 不回收，惰性 provider 持有 | 上界有限；回收需要引用计数，收益小 |
