# 多设计语言主题架构设计

日期：2026-11-07。状态：设计草案第四版，待用户审阅；产品实现未启动。

基线：`release/6.0`（`ae5ac0a32`，AtomUI 6.2.13，Avalonia 12.1.5）。分支 `feature/custom-theme` 的全部改动不作为参考，本文不复用其设计、术语或实现。

版本演进：第一版"Ant 基线 + 按语言补丁样式"被否决。第二版改为"一份控件代码、N 套对等 ControlTheme、独立主题包"。第三版按用户定义改为层级模型（设计语言 ⊃ 主题定义），并把 Token 派生策略交给语言。第四版拿掉最后一处特殊：**Ant Design 对 AtomUI 不再有任何特殊地位，它是第一个、也是参照用的设计语言包 `AtomUI.Themes.Ant`；控件包不含任何 ControlTheme、算法或主题定义；Core 不知道任何一种设计语言。**

## 1. 目标

让 AtomUI 应用在运行时切换多种设计语言（Ant Design、Material、shadcn、Bootstrap、PrimeNG 风格……），效果对齐 ant.design 首页 "Flexible theme customization" 的预设切换器：同一棵控件树，不重建、控件属性上的状态不丢，切换后整体外观变为另一套设计语言；同一应用内允许不同子树使用不同设计语言。

成功标准：

1. Gallery 增加一个与 ant.design 预览区等价的"设计语言预览"页，至少提供 Ant、Material、shadcn、Bootstrap 四套，切换在一次 UI 线程 Publish 内完成，输入框文字与选区、选中项、焦点保持。
2. 每种设计语言是独立 NuGet 包，Ant 也是；不引用不占产物，引用未激活只登记一张表，激活后只物化页面上出现过的控件主题。
3. 控件是 lookless 的，与 Avalonia 自身控件同一标准：控件包只含行为与契约（`[TemplatePart]`、`[PseudoClasses]`、Semantic Part、Own Token 字段），任何视觉值都在语言包里；没有注册任何语言时控件没有外观，像没有引用 Fluent 的 Avalonia 应用一样，默认外观由 `AtomUI` 元包带上的 `AtomUI.Themes.Ant` 提供。
4. 第三方控件包与应用可以为任一设计语言提供自己的 ControlTheme、主题定义与 Token，走与 AtomUI 自己的语言包完全相同的协议。
5. 不依赖 Avalonia 私有成员、不 hook 框架内部、不修改 Avalonia；仅使用公开且在 12.1.5 源码中核对过的行为。
6. 对 `release/6.0` 的改动，除新增能力外只允许三类：简化代码、提升性能、架构优化；每一处在第 9 节登记理由。
7. 机制与平台无关：桌面与未来的移动控件包共用同一套定义与注册协议。

## 2. 参考实现：ant.design 的切换器是怎么做的

来源：`ant-design/.dumi/pages/index/components/ThemePreview/previewThemes/*.ts`（master）。14 个预设全部是普通 `ConfigProvider` 配置对象：

| 机制 | 用途 | 使用预设数 |
| --- | --- | --- |
| `theme.token` | 全局 Token（颜色、圆角、字体、控件高度……） | 14/14 |
| `theme.components` | 逐组件 Token | 14/14 |
| `theme.algorithm` | 明暗算法 | 14/14 |
| 逐组件 `classNames` / `styles` | 对语义 DOM 区域注入 CSS（渐变、硬投影、backdrop blur、霓虹阴影、全大写、自定义 ripple） | 7/13 |
| 结构/模板替换、图标替换、`cssVar`、`componentSize` | 无 | 0 |

预览区 JSX 固定；切换只改 class 与注入的 CSS，预览子树整体重挂载。结论：**一个预设 = 语言的视觉层 + 它自己的色板 + 逐组件 Token**。antd 的视觉层是运行时生成的 CSS；Avalonia 里视觉层就是 ControlTheme，是编译产物。AtomUI 的 Token 层照抄 antd（`ThemeConfig` 已经是它），视觉层按 Avalonia 的方式交付：一个语言一套 ControlTheme，由语言包携带。

## 3. 核心模型

### 3.1 层级：设计语言 ⊃ 主题定义 ⊃ 运行时配置

```text
Design language   Ant                   Material                 Shadcn                Prime（直接指定式）
  包              AtomUI.Themes.Ant     AtomUI.Themes.Material   AtomUI.Themes.Shadcn  AtomUI.Themes.Prime
  ControlTheme    全部控件              部分控件                 无                    部分控件
  回落            无（必须完整）        Ant                      Ant                   Ant
  Token 派生      Ant 计算器 + Alias    HCT 计算器，Alias 复用 Ant 复用 Ant，覆盖语义色  值表
  默认主题        DaybreakBlue          MaterialPurple           ShadcnZinc            PrimeAura
  主题定义        DaybreakBlue、DustRed … / 通用主题在任何语言下可用
  变体            每个语言声明自己支持的变体 id（Default / Dark / Compact / 自定义）及其算法
  运行时配置      ThemeRequest.Config、局部 ThemeConfigProvider.Config
```

| 概念 | 拥有什么 | 不拥有什么 |
| --- | --- | --- |
| Core | Token 词汇表（Seed/Map/Alias 字段名）、Own Token 字段 schema 的注册与资源 ID 合同、派生管线、事务、作用域、注册表、校验 | 任何具体的值、算法、ControlTheme、主题定义 |
| 控件包 | 控件代码、`PART_*` 契约、Semantic Part 声明、Own Token 字段定义 | 任何 ControlTheme、视觉默认值、算法、主题定义 |
| 设计语言包 | ControlTheme 集；Token 派生策略（算法集、Alias 计算）；语言基础 Token；逐控件 Own Token 默认；主题定义（含默认主题）；回落目标 | 色板之外的用户配置；控件行为 |
| 主题定义 | 一组 Token 值（色板为主）；所属语言或通用；Appearance 元数据 | 任何 ControlTheme、模板、样式 |
| 运行时配置 | Token/控件/算法覆盖；局部作用域的语言切换 | 新的 Token 字段或控件 identity |

**不能"不特殊"的一条**：全局 Token 的词汇表来自 Ant Design 的分层与命名，但它是所有语言的 ControlTheme 共同消费的契约，留在 Core。能搬出 Core 的是值与派生，搬不走的是词汇；每种语言把自己的概念映射到这套词汇上（3.6）。

利弊评估：

- 利：模型彻底对称，Core 与注册表没有任何"回落 Ant"分支；控件包变成纯行为与契约，6.2 的五条契约从自律变为结构强制；Core 去掉三个计算器、调色板、Alias 公式与 DaybreakBlue；应用只带所选语言；AtomUI 自己的 Ant 包与第三方语言包走同一协议。
- 弊：应用必须注册至少一个语言才有外观，`AtomUI` 元包默认带 `AtomUI.Themes.Ant`，没有语言时按 Avalonia 的方式处理（控件无外观，启动记一条诊断，不抛异常）；语言必须显式声明回落目标（3.3），不声明则必须完整覆盖；控件要按 6.2 的 lookless 标准逐个重构，现状有 111 处代码赋视觉属性与 16 处命令式 Token 绑定要迁出；Own Token 默认求值从控件包搬到语言包是最大的一块迁移，分两步做（11）；可选控件包的 Ant 主题拆成 `AtomUI.Themes.Ant.DataGrid` 等小包，包数翻倍；一次性迁移约 380 个 AXAML，机械但量大。

### 3.2 Token 合并顺序

```text
library defaults（词汇表的类型默认值，不是任何语言的值）
  → design language 基础 Tokens
  → design language 逐控件 Token 默认
  → theme definition Tokens             （所属语言的专属主题，或通用主题）
  → root ThemeRequest.Config
  → 局部 ThemeConfigProvider.Config     （沿父链继承；Inherit=false 回到"语言 + 语言默认主题"基线）
```

现有 `ThemeConfigMerger` 增加两层输入，不新建配置对象。合并得到的是每个 Token 的**显式值**；显式值按 Stage 注入 3.5 的派生管线：Seed 值在算法前写入，Map 值在算法后写入，Alias 值在 Alias 计算后写入。这是 `ThemeCompiler.CompileGlobalBuilder` 今天的顺序，不变。

### 3.3 回落链

语言定义可以声明一个 `Fallback` 语言。所有"本语言没定义"的东西沿回落链解析：

| 缺什么 | 回落到 |
| --- | --- |
| 某控件的 ControlTheme | 回落语言的该控件 ControlTheme |
| 某控件的 Own Token 默认求值 | 回落语言的求值器 |
| 派生后仍未指定的 Map/Alias Token | 回落语言的派生结果（即用回落语言的算法链与 Alias 计算补齐缺口） |
| 某个变体 id 的算法 | 不回落；语言不支持的变体 → `Failed`（3.5） |

规则：

- 回落链无环，启动校验；回落目标必须已注册，否则启动失败并指明缺包。
- 链末端的语言（如 Ant）必须完整：每个已注册控件都有 ControlTheme 与 Own Token 求值器，派生后没有未指定 Token；启动校验列出全部缺口并失败。
- 回落链在注册期解析成每个 `(语言, 控件)` 的最终工厂与每个语言的最终派生计划，运行时查表不走链。

### 3.4 语言与主题的组合与回退

- 根请求必须给出语言；主题 id 可省略，省略即该语言的默认主题。只注册了一个语言时它就是初始语言；注册多个时 `WithInitialTheme` 必须指定。
- 语言专属主题只能在其所属语言下使用；在别的语言下请求它，规范化阶段 `Failed` 并给诊断。
- 只换语言（主题 id 省略）时：当前主题是通用主题则沿用；是语言专属主题则换成新语言的默认主题。结果写进 `ThemeState`。
- 局部 `ThemeConfigProvider` 指定语言但未指定主题时套同一规则；Popup、独立窗口沿现有资源链继承。
- 用户目录的主题 XML 可声明 `DesignLanguage`；声明了未注册的语言时进入 Catalog 诊断、不可选。用户目录不得定义设计语言。

### 3.5 Token 派生策略归语言所有

`release/6.0` 的全局 Token 管线是 Ant 的：25 个 Seed 经 `Default/Dark/Compact` 三个计算器派生 112 个 Map，再由 `DesignToken.CalculateAliasTokenValues` 固定公式算出 118 个 Alias。本设计把派生策略交给语言，Core 只保留管线：

```text
ThemeDerivation（每个语言一份）
  Algorithms      变体 id → IThemeAlgorithm 工厂          Ant：三个现有计算器（搬入 Ant 包）
                                                            Material：HCT 调色板实现的 Default/Dark，可不支持 Compact
                                                            Prime 类：Core 提供的 TokenTableAlgorithm，一张值表一个变体
  AliasCalculator Map → Alias                              Ant：现有公式（搬入 Ant 包）；其他语言可自写或经回落复用
  （缺口）        派生后仍未指定的 Map/Alias                 沿 3.3 回落链补齐；链末端语言不得有缺口
```

管线只有一条：

```text
seed  = library defaults + 显式 Seed 值
map   = language.Algorithms[请求变体…].Evaluate(seed) + 显式 Map 值
alias = language.AliasCalculator(map) + 显式 Alias 值
gaps  = fallback chain
```

- `ThemeAlgorithm` 从闭合枚举改为开放标识符（字符串 id），`Default`、`Dark`、`Compact` 只是约定常量；语言可注册自己的变体 id。请求与控件级 `Custom` 算法的变体 id 在当前语言的 `Algorithms` 中解析，不支持的 id → `Failed`；`DesignLanguageInfo.SupportedAlgorithms` 供 UI 禁用不可用开关。
- 外观仍由算法描述符的 `AppearanceEffect` 决定；直接指定式语言的 Dark 值表声明 `AppearanceEffect.Dark`。
- 直接指定式语言的值表就是 Map/Alias 阶段的一组 Token 值；基础值表写在语言定义里，色板变体写成该语言专属的主题定义 XML（Token 可以是任意 Stage，XML v1 已支持）。"一个预设 = 一张完整表"对应"一个语言 + 一个主题定义"。
- Control Own Token 的默认求值（今天各控件的 `CalculateTokenValues`）是视觉值，归语言所有：语言包为控件提供求值器，缺省沿回落链取。Own Token 的**字段**仍在控件包，它是契约；语言只能给值，不能增减字段。
- `NormalizeFinalBehavior`（动效开关等最终行为规范化）与语言无关，保持在管线末尾。

### 3.6 语言扩展 Token（本轮不实现）

Material 的色调角色（`primaryContainer`、`onPrimary`、五级 elevation）在 Core 词汇表里没有一一对应。首批实现要求语言把自己的概念映射到现有词汇（`primaryContainer → ColorPrimaryBg`、`onPrimary → ColorTextLightSolid`、elevation → 三级 BoxShadow），并把映射不了的概念列成清单。清单出来后再决定是否允许语言包在启动注册期扩展全局 schema；那会要求运行期追加的语言不得携带扩展 Token，因为 schema 启动后冻结。

### 3.7 激活模型：Theme 属性走作用域资源

每个作用域的 `ThemeContext` 已持有有效配置和唯一的 Token ResourceProvider。只给这个 Provider 增加一种可回答的键：

```text
ControlThemeKey(controlType)
    -> 当前作用域语言在该类型上（经回落链解析后）的 ControlTheme，惰性物化
    -> 注册表无此类型：不命中（Unset）；启动校验保证已注册控件在链末端一定命中
```

ThemeManager 启动时按注册表为每个已注册控件类型建立一条样式并挂入自己的 `Styles`：

```text
Style OfType<T>  { Setter(StyledElement.Theme, DynamicResource(ControlThemeKey(T))) }
```

- 隐式 ControlTheme 查找（以类型为 key 的资源）不再是 AtomUI 控件的主题来源；控件包不导出任何以类型为 key 的 ControlTheme 资源。应用自己的 `Theme` 本地值优先级更高，仍然赢。
- DynamicResource 在目标控件上实例化、沿目标控件的资源链解析，天然按作用域取值；局部 `ThemeConfigProvider`、Popup、带 owner 的独立窗口全部沿用现有资源链。
- 切换语言就是一次普通事务：Publish 阶段 Provider 发出 `ResourcesChanged`，Theme 绑定重新取值；只有主题实例真的变化的控件重套模板，只换 Token 的语言零重套。

### 3.8 保真度边界与升级阶梯

一份控件代码不可能 100% 还原每种设计语言的控件外观，任何单代码库方案都不能；ant.design 首页的 MUI/Bootstrap 预设是"像 MUI 的 antd"，Avalonia Fluent 也不是 WinUI 的像素复刻。本设计的目标是**AtomUI 适配版**：预览集合中的控件在该语言下静态视觉与反馈动效达到适配，每个语言包维护一份已知偏差清单。不承诺也不验收 100%。

差距分五层，覆盖能力与吸收点分别为：

| 层 | 例子 | 覆盖能力 | 吸收点 |
| --- | --- | --- | --- |
| 静态视觉 | 颜色、圆角、边框、阴影、字体、密度 | 接近完全 | Token、ControlTheme |
| 渲染能力 | backdrop blur、渐变描边、文字全大写、阴影 spread | 受 Avalonia 渲染器限制 | 能做的用主题；做不到的登记偏差 |
| 反馈与动效 | Material 从点击点扩散的 ripple，Ant 的 wave 外扩 | 可覆盖 | 语言包自带基元（6.1 `Primitives/`） |
| 行为与状态模型 | 浮动标签、Select 以 Menu 弹出、Snackbar 位置规则 | 部分 | 状态已是伪类/属性的靠模板；行为不同的给控件加全语言共用的属性；再不行登记偏差 |
| 组件清单 | Material 的 FAB、Chip、Navigation Rail | 不覆盖 | 语言包只提供视觉，不发明控件；需要时另立控件包，不在本方案范围 |

遇到差距按固定阶梯处理，不得跳级也不得绕过：

1. Token（语言基础 Token、Own Token 默认、主题定义）；
2. ControlTheme（`BasedOn` 回落主题加 Setter，或完整换模板）；
3. 语言包自带基元：装饰器、呈现器、转换器，自身也是 lookless 的非可视类或控件，只被该语言的模板引用；
4. 给控件增加一个所有语言都能用的属性开关（例如 `LabelPlacement`），由语言主题设默认值；
5. 登记为该语言包的已知偏差，写明原因（渲染能力、行为差异、有意不做）。

禁令：控件代码里不得出现按设计语言分支的逻辑，不得读取当前语言 id。差异只能表现为"控件多了一个所有语言可用的开关"或"语言包多了一个基元"。

## 4. 公开 API

全部位于 `AtomUI.Core`，命名空间 `AtomUI.Theme`；资源键位于 `AtomUI.Theme.Resources`。

```csharp
public readonly record struct ThemeAlgorithm(string Id)
{
    public static readonly ThemeAlgorithm Default = new("Default");
    public static readonly ThemeAlgorithm Dark    = new("Dark");
    public static readonly ThemeAlgorithm Compact = new("Compact");
}

public interface IThemeAliasCalculator { void Calculate(DesignToken map); }

public sealed class ThemeDerivation
{
    public static ThemeDerivation Create(IThemeAliasCalculator aliasCalculator, params ThemeAlgorithmDescriptor[] algorithms);
    public static ThemeDerivation Tables(ThemeTokenTable @default, ThemeTokenTable? dark = null, ThemeTokenTable? compact = null);
    public static ThemeDerivation Inherit { get; }        // 全部沿回落链取；Shadcn/Bootstrap 用它
    public ThemeDerivation WithAlgorithm(ThemeAlgorithmDescriptor descriptor);
    public IReadOnlyDictionary<ThemeAlgorithm, ThemeAlgorithmDescriptor> Algorithms { get; }
    public IThemeAliasCalculator? AliasCalculator { get; }  // null = 回落
}

public sealed class DesignLanguageDefinition
{
    public DesignLanguageDefinition(string id, string displayName, string defaultThemeId,
        ThemeDerivation derivation, string? fallback = null, ThemeConfig? tokens = null,
        IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig>? controls = null);
    public string Id { get; }
    public string DisplayName { get; }
    public string DefaultThemeId { get; }      // 该语言专属或通用的已注册主题
    public ThemeDerivation Derivation { get; }
    public string? Fallback { get; }           // null = 链末端，必须完整
    public ThemeConfig? Tokens { get; }        // 任意 Stage 的显式值；Algorithms/Controls/Inherit 非默认时构造失败
    public IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig> Controls { get; }
}

// 一个程序集为某个语言贡献的视觉集：ControlTheme、Own Token 默认求值器。由生成器产出。
public sealed class DesignLanguageThemeSet
{
    public DesignLanguageThemeSet(string languageId,
        IEnumerable<ControlThemeAssetDescriptor> themeAssets,            // TargetType 即键
        IEnumerable<ControlTokenDefaultsRegistration> tokenDefaults);    // (ControlTokenIdentity, 求值器)
    public string LanguageId { get; }
    public IReadOnlyList<ControlThemeAssetDescriptor> ThemeAssets { get; }
    public IReadOnlyList<ControlTokenDefaultsRegistration> TokenDefaults { get; }
}

public interface IThemeManagerBuilder
{
    void AddDesignLanguage(DesignLanguageDefinition definition);
    void AddDesignLanguageThemes(DesignLanguageThemeSet themes);
    void WithInitialTheme(string designLanguage, string? themeId = null, ThemeConfig? config = null);
    void WithFollowSystemThemes(ThemeRequest light, ThemeRequest dark);
    // 其余不变；DEFAULT_THEME_ID 常量删除
}

public sealed record ThemeRequest(string DesignLanguage, string? ThemeId, ThemeConfig? Config, ThemeTransitionReason Reason);
public sealed record ThemeInfo(string Id, string Name, ThemeAppearance Appearance, bool IsDefault, string? DesignLanguage, Color? AccentColor);
public sealed record DesignLanguageInfo(string Id, string DisplayName, string DefaultThemeId, string? Fallback,
    IReadOnlyList<ThemeAlgorithm> SupportedAlgorithms, int ThemedControlCount);

public interface IThemeManager
{
    IReadOnlyList<DesignLanguageInfo> AvailableDesignLanguages { get; }
    IReadOnlyList<ThemeInfo> AvailableThemes { get; }
    void AddDesignLanguage(DesignLanguageDefinition definition, params DesignLanguageThemeSet[] themes); // 运行期追加，5.4
    event EventHandler? DesignLanguagesChanged;
    // 其余不变
}

public sealed class ThemeConfig        { public string? DesignLanguage { get; } /* 局部作用域切语言；根请求忽略 */ }
public sealed class ThemeConfigBuilder { public ThemeConfigBuilder WithDesignLanguage(string? id); }
public sealed record ThemeState        { public string DesignLanguage { get; } /* ThemeId 为回退后的实际主题 */ }
```

主题 XML v1 增加可选根属性 `DesignLanguage`，省略即通用主题；`IsDefault` 改由语言定义的 `DefaultThemeId` 决定，XML 上不再声明。编译主题包与用户目录 Resolver 读取同一属性。

AXAML 侧新增标记扩展，供语言主题派生回落语言的主题：

```xml
<ControlTheme x:Key="{x:Type atom:Button}" TargetType="atom:Button"
              BasedOn="{atom:FallbackControlTheme atom:Button}">
    <Setter Property="CornerRadius" Value="{atom:ButtonTokenResource BorderRadiusLG}" />
</ControlTheme>
```

`FallbackControlTheme` 在物化时按所在语言包的回落链取实例；物化只发生在激活之后、UI 线程上，注册表已冻结。

不提供：运行时从文件加载样式、JSON/XML 承载样式、配方对象、AI 生成入口。

## 5. 运行时

### 5.1 注册表与启动校验

`DesignLanguageRegistry`（ThemeManager 持有）：

- `Definitions`：`Id → DesignLanguageDefinition`；重复 Id、`Fallback` 未注册或成环、`DefaultThemeId` 不可解析 → 启动失败。
- `Themes`：`(Id, Type) → DeferredControlThemeResourceProvider`，复用现有惰性 provider；同一 `(Id, Type)` 重复 → 启动失败。
- `TokenDefaults`：`(Id, ControlTokenIdentity) → 求值器`；重复 → 启动失败。
- 主题集引用无定义的 Id → 启动失败并指出缺包。
- 注册期按回落链解析出每个 `(语言, 控件)` 的最终 ControlTheme 工厂与求值器，以及每个语言的最终派生计划；链末端语言对每个已注册控件都必须有二者，派生后不得有未指定 Map/Alias，否则启动失败并列出全部缺口。
- 一个语言都没注册 → 不失败：控件 lookless，无模板即无外观，与未引用 Fluent 的 Avalonia 应用一致；启动在 `ThemeCatalogDiagnostics` 记一条 `ATMTHM` 诊断提示引用 `AtomUI.Themes.Ant` 或其他语言包。
- 算法描述符随语言登记：`[ThemeAlgorithm]` 标注的计算器由生成器归入所在语言包。Core 不含任何计算器。
- Catalog（`CompiledThemeCatalog`）为每个主题定义记录 `DesignLanguage`，并提供"语言 → 可用主题"的分组查询。

### 5.2 一次切换的事务路径

沿用现有 Capture / Prepare / CommitCore / Publish / Complete，不加阶段：

1. **Capture**：按 3.4 解析 `(语言, 主题)`，变体 id 在该语言的 `Algorithms` 中解析为描述符；未注册语言、专属主题跨语言、未知主题 id、不支持的变体 id 在 Normalizer 失败，`Failed` 返回诊断。
2. **Prepare**（可后台）：Normalizer 按 3.2 合并；编译器用该语言解析后的派生计划（3.5）与 Own Token 求值器产出 Snapshot。缓存键含语言、派生计划修订与变体链。不物化任何 ControlTheme。
3. **CommitCore**（UI 线程，无通知）：替换 Snapshot 引用，不变。
4. **Publish**（UI 线程）：既有 Token 资源通知同时驱动 Token 绑定与 Theme 绑定重新取值；Theme 绑定首次命中新语言的某个类型时才物化该 ControlTheme。`RequestedThemeVariant`、事件不变。
5. **Complete**：`ThemeState` 记录实际生效的语言与主题。

### 5.3 Provider 回答 Theme 键

`ThemeTokenResourceProvider.TryGetResource` 增加一个分支：键为 `ControlThemeKey` 时，取 `Snapshot.EffectiveConfig.DesignLanguage`，在注册表查解析后的 `(语言, 键.ControlType)` 工厂；命中返回物化后的 ControlTheme，未命中返回 `false`。键对象按类型缓存为单例，保证 DynamicResource 的键相等。

### 5.4 按需与运行期追加

- 不引用语言包：不进产物。
- 引用未激活：只登记定义、它自带的主题定义和 `(Id, Type) → 工厂`、`(Id, Identity) → 求值器` 两张表，AXAML 不解析。
- 激活：只物化被查询过的类型；切回后实例留在惰性 provider 中，上界为"已激活过的语言 × 出现过的控件类型"。
- 运行期追加：语言包不新增 Token schema、不新增控件 identity，只往注册表和 Catalog 加行；`IThemeManager.AddDesignLanguage` 在 UI 线程追加、重跑回落解析与完整性校验、派发 `DesignLanguagesChanged` 与 `ThemeCatalogChanged`。JIT 应用可配合 `AssemblyLoadContext` 在运行中装载语言包；NativeAOT 无运行期装配加载，只用前三条。

### 5.5 与 Semantic Part、Own Token 的关系

- 每种语言的 ControlTheme 都必须完整实现该控件的 Semantic Part descriptor（非 Optional marker 齐全）；现有 ThemeAssets 分析器对 ControlTheme 的校验作用于所有语言包，Ant 包也不例外。
- 用户的 Semantic Part Style 继续写在 `Application.Styles` / 局部 StyleHost，对任何语言的主题都生效。
- Own Token 字段由控件包定义，语言只能提供默认求值与静态覆盖，不能增减字段。一种语言需要新的可调值时，那是给控件加 Own Token，所有语言共享。

## 6. 包与作者约定

### 6.1 包布局

```text
AtomUI.Controls / AtomUI.Desktop.Controls / …DataGrid   控件包：代码、PART_*、Semantic Part 声明、Own Token 字段；无 AXAML 主题
                                                          （Avatar/Avatar.cs、Avatar.SemanticParts.cs、AvatarToken.cs）

AtomUI.Themes.Ant/                                        第一个语言包，链末端，必须完整
+-- AtomUI.Themes.Ant.csproj                              <AtomUIDesignLanguage>Ant</AtomUIDesignLanguage>
+-- AntDesignLanguage.cs                                  Id、显示名、默认主题 DaybreakBlue、Derivation、无 Fallback
+-- Algorithms/                                           Default/Dark/Compact 三个计算器、调色板、Alias 计算器（自 Core 搬入）
+-- Definitions/DaybreakBlue.theme.xml                    自 Core 搬入，DesignLanguage="Ant"
+-- TokenDefaults/Buttons/ButtonTokenDefaults.cs          自控件包 ButtonToken.CalculateTokenValues 搬入
\-- Themes/Buttons/ButtonTheme.axaml …                    自控件包 Themes/ 搬入，目录与控件包同构

AtomUI.Themes.Ant.DataGrid / .ColorPicker / .Extras       可选控件包的 Ant 主题，按需引用

AtomUI.Themes.Material/                                   派生式，Fallback = Ant
+-- MaterialDesignLanguage.cs
+-- Algorithms/MaterialDefaultCalculator.cs …             HCT 调色板；Alias 回落 Ant
+-- Definitions/MaterialPurple.theme.xml …
+-- Primitives/RippleDecorator.cs …                       本语言模板专用的 lookless 基元（3.8 阶梯第 3 级）
+-- TokenDefaults/…                                       需要改默认的控件
+-- Themes/…                                              需要换形态的控件，可 BasedOn="{atom:FallbackControlTheme …}"
\-- DEVIATIONS.md                                         已知偏差清单：项、原因、所属阶梯

AtomUI.Themes.Prime/                                      直接指定式，Fallback = Ant
+-- PrimeDesignLanguage.cs                                Derivation = ThemeDerivation.Tables(base, dark)
+-- Definitions/PrimeAura.theme.xml                       预设 = 专属主题定义，Token 直接写 Map/Alias 值
\-- Themes/…

AtomUI.Themes.Shadcn/                                     Fallback = Ant，Derivation = Inherit
+-- ShadcnDesignLanguage.cs                               只覆盖语义色、圆角、高度
\-- Definitions/ShadcnZinc.theme.xml                      零 AXAML
```

`AtomUI` 元包默认引用 `AtomUI.Themes.Ant`，`UseAtomUI()` 开箱即是 Ant；引用其他语言包后调用其 `UseDesignLanguage<T>()` 注册。

生成器规则：项目设置了 `AtomUIDesignLanguage` 时，`Themes/**/*.axaml` 中以类型为键的 ControlTheme 登记到该语言的主题集，`[ThemeAlgorithm]` 计算器与 `[ThemeAliasCalculator]` 登记到该语言的派生策略，`TokenDefaults/**` 中带 `[ControlTokenDefaults(typeof(XxxToken))]` 的求值器登记到该语言，`Definitions/*.theme.xml` 作为编译主题定义登记并强制 `DesignLanguage` 等于该语言；产出 `GeneratedDesignLanguageRegistration.Register(builder)`。控件包（未设置该属性）内出现 ControlTheme、计算器或主题定义 → 构建错误。

一个程序集只属于一个语言。第三方控件包要支持 Ant 或 Material，各建 `Vendor.Controls.Themes.Ant`、`Vendor.Controls.Themes.Material`，引用对应语言包取定义，按同一目录约定放主题。

构建期校验（新增诊断号沿用 `ATOMUIGEN` 序列）：

- 语言包内 ControlTheme 必须以类型为键；`TargetType` 必须是引用的控件包中已注册的 Control；同一程序集内同一 `TargetType` 只出现一次。
- 非 Optional Semantic marker 齐全（现有校验）。
- `FallbackControlTheme` 的类型参数必须与 `TargetType` 一致，且所在语言声明了 `Fallback`。
- `Definitions/*.theme.xml` 的 `DesignLanguage` 必须等于项目的 `AtomUIDesignLanguage`。
- 控件包内不得含 ControlTheme、计算器、主题定义或 Own Token 求值方法。

### 6.2 控件 lookless 标准：一份代码如何承载多套主题

控件不感知语言，也不自带外观。标准与 Avalonia 自身控件相同：控件类 = 属性 + 行为 + 对命名部件的接线；模板、样式、每一个视觉值都由主题提供。语言差异分三档，各有吸收点：

| 差异档 | 例子 | 吸收点 | 对控件的要求 |
| --- | --- | --- | --- |
| 值差异 | 主色、圆角、高度、字体、线宽、阴影、间距、动效开关 | 语言基础 Token、Own Token 默认求值、主题定义 | ControlTheme 的每个视觉值来自 Token 或控件属性，无字面量 |
| 区域差异 | 渐变、硬投影、模糊、图标尺寸与位置、hover 效果、有无 ripple | `BasedOn` 回落主题的派生 ControlTheme，加 Setter 与嵌套 Style | 可能被重绘的区域都是 Semantic Part；状态经伪类/属性暴露 |
| 结构差异 | Material Switch 圆钮在轨道外、浮动标签、Bootstrap 勾选框形状 | 完整 ControlTheme 换模板 | 代码与模板的契约只有 `PART_*` 与行为属性；可选 PART 缺失时容错 |

这正是 Avalonia Fluent / Simple 两套主题共用一份控件代码的做法。

**现状基线**（`AtomUI.Controls` + `AtomUI.Desktop.Controls`，119 个公开控件，2026-11-07 统计）：

| 项目 | 数量 | 处置 |
| --- | --- | --- |
| 代码直接赋视觉属性（`Background`/`Foreground`/`CornerRadius`/`Width`…） | 111 处 | 违规，迁到主题 Setter 或删除 |
| 代码命令式绑定 Token（`TokenResourceBinder`、`ControlTokenAccessor`） | 16 处 / 8 文件 | 违规，迁到主题 |
| `Render` 自绘重写 | 38 个 | 允许；画刷、笔、几何参数必须来自主题设置的属性 |
| `[TemplatePart]` / `[PseudoClasses]` 声明 | 47 / 60 | 覆盖率不足，补齐为每个控件的正式契约 |
| C# 构建的 ControlTheme | 0 | 已满足 |

**lookless 规则**（逐控件二元审计，构建期能查的进分析器）：

1. **视觉值不进代码。** 控件代码不得出现 `Brush`、`Color`、`BoxShadows`、`CornerRadius`、`Thickness`、字号、尺寸的字面量或由 Token 计算得出的值；不得命令式绑定 Token。自绘控件把画刷、笔、几何参数暴露为 StyledProperty，在 `Render` 中只读取，默认值由主题 Setter 给。
2. **状态可以进代码，视觉不行。** 由用户输入解析出的离散状态（如 Button 的 `EffectiveVariant`、`EffectiveIsDanger`）允许作为内部 StyledProperty 或伪类存在，供主题选择器使用；由状态映射出的视觉值（如 `VariantShadow`、按 `SizeType` 算出的 `EffectiveCornerRadius`）不允许，改为主题里的 `^[SizeType=Large]` 之类 Setter。判据：属性类型是枚举/布尔/数值计数就是状态，是画刷/阴影/圆角/厚度/字号就是视觉。
3. **模板契约显式且最小。** 每个控件用 `[TemplatePart(Name, Type, IsRequired)]` 声明代码依赖的全部部件，用 `[PseudoClasses]` 声明全部伪类；`OnApplyTemplate` 只经 `NameScope` 取部件，可选部件为空时行为降级不抛；不在代码中按类型或 Name 搜索可视树。
4. **几何从属性推导，不写常量。** 布局、命中、动效原点使用模板设置的属性与 Token 绑定值。
5. **Semantic marker 是主题的义务。** 每种语言的主题保持同一组非 Optional marker；控件代码不负责 marker。
6. **没有主题也能构造、运行行为。** 控件在无模板下 `Measure/Arrange` 返回零尺寸、事件与命令照常；这是 lookless 的验收用例。

设计语言只覆盖视觉。行为差异不属于主题；确有需要时按 3.8 的阶梯给控件增加所有语言可用的属性，由语言主题设默认值；控件代码永远不感知当前语言。

### 6.3 平台

机制在 Core，定义、主题定义、ControlTheme 与 Own Token 默认在语言包，控件包不含任何语言文件。移动控件包出现后，`AtomUI.Themes.Ant.Mobile`、`AtomUI.Themes.Material.Mobile` 按同一约定为移动控件提供主题；触控尺寸等差异写在它们的 Own Token 默认里，不进语言定义。

## 7. Gallery

- 工作台顶栏：先选设计语言，再在该语言下选主题（语言专属 + 通用），与明暗、紧凑并列；不支持的变体按 `SupportedAlgorithms` 禁用。
- 新增"设计语言预览"展示页：复刻 ant.design 预览区的控件集合，页面内一个 `ThemeConfigProvider` 承载预览，切换只改该 Provider 的 `Config.DesignLanguage`；同页可同时放两个不同语言的 Provider 做对照。
- Gallery 引用 `AtomUI.Themes.Ant`（含 DataGrid/ColorPicker/Extras 子包）、`Material`、`Shadcn`、`Bootstrap`。

## 8. 性能、内存与 AOT

- 启动：ThemeManager 多挂一组按类型的样式（每个已注册控件类型一条，约 380 条 `OfType` 选择器）。`OfType` 匹配是类型相等比较。隐式 ControlTheme 的资源查找同时被取消，净成本接近零；若实测可见，改为一条 `:is(Control)` 样式加自定义 `IBinding` 按目标 `StyleKey` 计算键，spike 顺带测两者差距。
- 每个控件多一个 DynamicResource 订阅（Theme）；AtomUI 控件已有数十个 Token 订阅，增量可忽略。
- 切换：只换 Token 的语言与今天的主题切换成本相同；换主题的语言在被覆盖类型上重套模板，代价小于 ant.design 的整树重挂载。目标：Gallery 预览页（约 60 个控件）Publish 在 16 ms 内，首次激活含物化不超过 50 ms；以 Stopwatch 计。
- 内存：惰性 provider 上界见 5.4；Snapshot 缓存键含语言与主题，`LazyControlSnapshots` 不变。
- AOT：定义、主题定义、ControlTheme 与求值器都是静态构造 + 编译产物，无反射、无 `Activator`、无字符串类型名。

## 9. 对 release/6.0 的改动登记

| 改动 | 类别 | 理由 |
| --- | --- | --- |
| 全部控件包的 `Themes/**/*.axaml`、Core 的三个计算器、调色板、Alias 公式、`DaybreakBlue.theme.xml` 搬入 `AtomUI.Themes.Ant`（可选包的进各自 Ant 子包） | 架构优化 | 控件包只剩行为与契约；Core 不含任何语言的值与算法 |
| 各控件 `XxxToken.CalculateTokenValues` 搬入 `AtomUI.Themes.Ant/TokenDefaults`，Token 类只剩字段 | 架构优化 | Own Token 默认是 Ant 的视觉值；控件包不含视觉值 |
| 控件 lookless 重构：迁出 111 处代码视觉赋值与 16 处命令式 Token 绑定，计算型视觉属性改为主题 Setter，补齐 `[TemplatePart]`/`[PseudoClasses]`，自绘控件改为读取主题设置的属性 | 架构优化 | 与 Avalonia 控件同一标准；没有它，语言包替换主题时控件代码里的 Ant 视觉值会漏出来 |
| `DesignToken.CalculateAliasTokenValues` 抽为 `IThemeAliasCalculator` | 架构优化 | Token 定义类只剩数据 |
| `ThemeAlgorithm` 由闭合枚举改为开放 id；算法描述符按语言登记 | 新能力所需 + 架构优化 | 派生策略归语言所有 |
| 删除 `IThemeManager.DEFAULT_THEME_ID`；`WithInitialTheme` 改为语言优先 | 新能力所需 | Core 不再知道 DaybreakBlue |
| `ThemeRequest` 增加 `DesignLanguage`，`ThemeId` 可空；`ThemeInfo`/`ThemeState` 携带语言；XML v1 增加 `DesignLanguage` 属性，`IsDefault` 改由语言定义决定 | 新能力所需 | 层级模型要求请求与目录都知道语言 |
| `ThemeConfig` 增加 `DesignLanguage`（仅局部作用域）；Normalizer/Merger 增加语言基础与逐控件默认两层 | 新能力所需 | 复用既有合并器 |
| `ThemeTokenResourceProvider` 多回答一种键；ThemeManager 启动挂按类型的 Theme 样式；控件包不再导出类型键 ControlTheme 资源 | 新能力所需 | 主题来源从挂树时的隐式查找改为随作用域变化的显式绑定 |
| `DesignLanguageRegistry`、定义/主题集类型、回落解析与完整性校验、`FallbackControlTheme` 标记扩展、Catalog 按语言分组 | 新能力所需 | 复用 `ControlThemeAssetDescriptor` 与惰性 provider |
| 根 Context 样式选择器由 `OfType<TopLevel>()` 改为 `Is<TopLevel>().PropertyEquals(ParentProperty, null)` | 架构优化，可独立提交 | 现样式把根 Context 以样式值写到 PopupRoot 上，覆盖它从 Popup 继承的局部 Context |

不改动：事务结果枚举、Own Token 字段与资源 ID 合同、资源扩展、Semantic Part 生成、`ThemeContextLease`、编译主题包的 ABI、用户主题目录的安全边界。不引入别名、兼容层或过渡 API。

## 10. 验证

单元（`AtomUI.Core.Tests`，测试工程引用 `AtomUI.Themes.Ant`）：

- 合并顺序：语言基础 < 逐控件默认 < 主题定义 < 根配置 < 局部配置，逐对验证；显式值按 Seed/Map/Alias 阶段注入位置正确。
- 回落链：ControlTheme、Own Token 求值器、Map/Alias 缺口三类回落各自验证；成环、目标未注册、链末端不完整 → 启动失败并列出缺口；无语言注册 → 启动成功、记诊断、控件无模板。
- lookless：每个控件在无任何语言注册的宿主中构造、挂树、`Measure/Arrange`、触发事件与命令不抛；分析器对控件包内视觉字面量、命令式 Token 绑定、计算型视觉属性报错；`[TemplatePart]` 声明与 `OnApplyTemplate` 实际查找一致、与 Ant 主题的 `PART_*` 名一致。
- 派生策略：派生式语言的算法只在该语言下解析；值表语言产出与表一致、Dark 表给出 Dark 外观；不支持的变体 id → `Failed`；`SupportedAlgorithms` 与注册一致。
- Ant 回归：只注册 `AtomUI.Themes.Ant` 时，现有全部主题与控件测试结果不变（算法、Alias、Own 默认与 ControlTheme 只是搬家）。
- 组合与回退：专属主题跨语言 → `Failed`；只换语言时通用主题沿用、专属主题回退到新语言默认，`ThemeState` 反映实际值。
- Theme 键：覆盖类型取到本语言主题、未覆盖类型取到回落主题；物化恰好一次；A→B→A 后实例数不变。
- 作用域：根、`ThemeConfigProvider` 子树、Popup、带 owner 的独立窗口四种宿主下 Theme 绑定取到各自作用域语言的主题。
- 运行期追加：`AddDesignLanguage` 后列表更新、完整性重校验、再切换生效。
- 泄漏：关闭窗口、移除 Provider 后 WeakReference 回收；惰性 provider 不持有控件。

集成（`AtomUI.Desktop.Controls.Tests` / Gallery 测试）：

- 每套语言 × 预览页：切换后关键区域按已采用的容差策略对比各自基准；基准是该语言包自己的参考渲染，不是原设计系统的截图。
- 保真度：每个语言包的 `DEVIATIONS.md` 与预览集合逐控件核对，预览集合中未登记偏差的项必须在适配范围内；分析器对控件包内读取语言 id 或按语言分支的代码报错。
- 状态保持：切换前后 TextBox 文本与选区、ListBox 选中项、焦点元素相同；被换模板的控件记录模板内状态丢失范围。
- 每种语言主题的 Semantic Part 预览能解析全部非 Optional 部件。
- 用户显式设置 `Theme` 的控件不被语言切换改写。

性能：第 8 节两项计时在 Gallery headless 宿主下由测试断言。

## 11. 实施顺序

1. **Spike（半天）**：在 `release/6.0` 上用临时样式验证并记录：样式 Setter 改变已挂载控件的 `Theme` 后实时重套模板且 `PART_*` 查找正常；DynamicResource 不命中时 `Theme` 回到 null；作用域 Provider 的 `ResourcesChanged` 能驱动 Popup 内与 lease 窗口内控件的 Theme 绑定；约 380 条 `OfType` 样式对 Gallery 首页套样式耗时的影响。任一不成立即停下修订本设计。
2. **拆出 Ant 包（机械迁移，先行）**：新建 `AtomUI.Themes.Ant` 及可选包子包；搬入全部 ControlTheme AXAML、三个计算器、调色板、Alias 计算器、DaybreakBlue；生成器支持 `AtomUIDesignLanguage` 并禁止控件包含主题；`AtomUI` 元包引用 Ant 包；现有测试全部通过。此时只有一个语言，运行时仍走隐式查找，行为不变。
3. **Own Token 默认搬迁**：各 `XxxToken.CalculateTokenValues` 搬入 Ant 包 `TokenDefaults/`，Token 类只剩字段；生成器登记 `(语言, identity) → 求值器`；测试不变。
3a. **控件 lookless 重构**（与第 4、5 步并行，按控件家族分批）：先落分析器（视觉字面量、命令式 Token 绑定、计算型视觉属性三条诊断），再逐家族迁出 111 处视觉赋值与 16 处 Token 绑定、把 `VariantShadow`/`EffectiveCornerRadius` 一类计算型视觉属性改为 Ant 主题的 Setter、补齐 `[TemplatePart]`/`[PseudoClasses]`、为 38 个自绘控件补画刷/笔属性；每批以"无语言宿主下行为测试通过 + Ant 外观关键区域不变"验收。第 6 步的 Material 主题只能覆盖已完成 lookless 的控件。
4. **Core 派生层**：`ThemeAlgorithm` 开放 id、`IThemeAliasCalculator`、`ThemeDerivation`、值表算法、按语言登记算法；Ant 回归测试保持通过。
5. **Core 语言层**：注册表、回落解析与完整性校验、`ThemeRequest`/`ThemeInfo`/`ThemeState`/XML v1 属性、`ThemeConfig.DesignLanguage`、合并层、回退规则、Theme 键与 Provider 分支、启动样式、`FallbackControlTheme`、运行期追加；控件包不再导出类型键资源。
6. **语言包**：`AtomUI.Themes.Shadcn`、`Bootstrap`（`Inherit` 派生 + 色板主题）；`AtomUI.Themes.Material`（HCT 派生）先覆盖 Button、Input 家族、Select、Checkbox/Radio/Switch、Steps、Progress、Tag、Card、Alert、Dialog、Notification；一个直接指定式最小示例语言进测试工程。
7. **Gallery**：切换器与预览页；验证第 10 节。
8. 扩展 Material 到其余控件与可选包；按 3.6 列出映射不了的 Token 概念清单。

每一步独立提交、独立验收；第 2、3 步是纯搬运但建立了后续全部约束；第 6 步之后即可在 Gallery 看到 ant.design 式切换效果。

## 12. 已决策事项（供审阅时推翻）

| 问题 | 决策 | 替代方案及为何不选 |
| --- | --- | --- |
| Ant 的地位 | 普通语言包 `AtomUI.Themes.Ant`，控件包与 Core 不含任何语言的值 | Ant 内置于控件包：永远存在"回落 Ant"特例，契约靠自律 |
| 语言未覆盖的控件 / 缺口 | 显式 `Fallback` 回落链，注册期解析，链末端必须完整 | 隐式回落 Ant：Core 知道 Ant；无回落：Material 要先写满 95 个主题才能用 |
| Own Token 默认求值归谁 | 语言包，经回落链取；字段留控件包 | 留控件包：控件包含 Ant 视觉值，与"不特殊"矛盾 |
| Token 词汇表归谁 | Core | 也交给语言：所有语言的 ControlTheme 失去共同契约 |
| 语言与主题定义的关系 | 层级：语言 ⊃ 主题；主题专属或通用 | 正交轴：需要裁决优先级，目录无法分组 |
| 视觉层的形态 | 一个语言一套 ControlTheme，独立包 | 补丁样式：Ant 特殊化、目录膨胀、改选择器 |
| 语言如何到达控件 | `Theme` 属性经 DynamicResource 走作用域 Provider | 可继承附加属性 + PropertyEquals：多一条机制，不能换模板 |
| Token 派生归谁 | 语言拥有算法集与 Alias 计算；一条管线，值表算法覆盖直接指定式 | 全局算法：直接指定式语言只能硬压 230 个值 |
| 变体 id 的形态 | 开放字符串 id | 闭合枚举：语言无法声明自己的变体 |
| 只换语言时的主题 | 通用沿用，专属回退到新语言默认 | 一律回退或一律沿用都丢信息 |
| 无语言注册时 | 与 Avalonia 一致：控件 lookless 无外观，启动记诊断不抛 | 启动失败：把"没有主题"当错误，与 lookless 模型矛盾，也阻碍无 UI 的单元测试 |
| 控件代码里允许留什么 | 解析后的离散状态（枚举/布尔）可留作内部属性或伪类；画刷/阴影/圆角/厚度/字号一律迁到主题 | 全部迁出：状态选择器失去依据；全部保留：语言换主题时 Ant 视觉值漏出 |
| 语言扩展全局 Token | 本轮不做，先映射并列清单 | 现在开放 schema 扩展：证据不足 |
| 保真度目标 | AtomUI 适配版 + 每语言偏差清单；差距按 Token → 主题 → 基元 → 控件开关 → 偏差的阶梯处理 | 承诺 100%：单代码库做不到，参照物也没做到；不设阶梯：实现时会把语言分支塞进控件代码 |
| 语言包能否带代码 | 能带 lookless 基元与转换器，只供自己的模板用 | 只许 AXAML：ripple 一类反馈差异只能登记为偏差 |
| 可选控件包的语言主题 | 独立小包 `AtomUI.Themes.<Lang>.<Pkg>` | 放回控件包：重新把主题塞进控件包 |
