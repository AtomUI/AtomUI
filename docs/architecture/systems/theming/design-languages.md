# 设计语言架构

本文描述当前设计语言架构，第 12 节记录[应用注册与启动 API](#应用注册与启动-api)。
原始方案见[设计语言架构方案](../../../superpowers/specs/2026-11-22-design-language-architecture-design.md)，当前实现与原方案的差异见第 14 节。运行时的发布层（快照、作用域、事务、
资源查询）见[主题系统架构](runtime.md)，作者视角的写法见[编写设计语言](../../../guides/theming/writing-design-languages.md)，
应用视角见[设计语言包指南](../../../guides/theming/design-language-packages.md)。

## 1. 核心思想

**词汇是契约，设计语言是契约的完整实现。**

| 角色 | 负责 | 不负责 |
| --- | --- | --- |
| 词汇 | 有哪些 Token、各自表达什么。全局词汇由核心声明，控件 Token 由各控件包在控件旁声明 | 任何取值 |
| 取值 | 每个 Token 等于多少、怎么从种子推出来 | 全部归设计语言，核心与控件不含取值规则 |
| 控件实现 | 只消费 Token | 不知道当前是哪种语言；模板不得判断语言 |

由此得出几条贯穿全局的规则：

- Ant Design、Shadcn、PrimeNG 以及任何第三方语言是**平级**的，谁也不叠在谁上面；Ant Design 自己也只用公开 API 写成。
- 控件不认识语言，语言不认识控件，两者只在**取值包**里相遇。OSS 控件、商业产品、第三方控件走同一条路径。
- 差异只通过 Token 表达，包括行为差异：它们是 Token 上有类型的开关（例如 `AbstractButtonGeometryToken.DisabledByOpacity`），
  没有无类型的旁路通道。
- 求值是单遍的；显式值是输入，不是事后补丁。
- 每个取值都有负责人：缺少推导在启动时报错，不留到运行时，也没有跨语言的隐式回退。

## 2. 概念

| 概念 | 代码中的形态 | 所在 |
| --- | --- | --- |
| 全局 Token | `DesignToken`：Seed / Map / Alias 三层，纯声明 | 核心 `AtomUI.Theme.DesignTokens` |
| 控件 Token | `[ControlDesignToken]` 的 `public partial` 类，派生自 `AbstractControlDesignToken`，纯声明 | 控件包，与控件同目录 |
| 语言 Token | `[LanguageDesignToken]` 的 `partial` 类，派生自 `LanguageDesignToken` | 语言包 |
| 设计语言 | `[DesignLanguage]` 类，派生自 `DesignLanguage<TLanguageToken>` | 语言包 |
| 变体 | `DesignVariant(Style, Appearance, Density, Idiom)` | 运行时值 |
| 写入目标 | `ControlTokenTarget<TToken>` 的 `Own` 与 `Global` 分别承载控件自身 Token 和该控件可见的全局 Token | 控件求值期间 |
| 控件取值规则 | `[ControlTokenRule]` 类，实现 `IControlTokenRule<TLanguageToken, TToken>` | 取值包 |
| 主题 | 语言 + 固定的轴 + 种子 + 显式覆盖的一份配置：XML v2 主题定义，或 `ThemeConfig` | 应用或任意包 |
| 选择 | `ThemeSelection`：最终用户选的语言、具名主题、明暗、密度与种子 | 运行时值 |

契约类型都在 `AtomUI.Theme.Languages`（`src/AtomUI.Core/Theme/Languages/`），描述符在 `AtomUI.Theme.Schema`。

## 3. 四类包与依赖规则

| 类别 | 命名 | 包含 | 引用 |
| --- | --- | --- | --- |
| 核心 | `AtomUI.Core` | 全局 Token 声明、语言契约、单遍管线、主题定义、选择与注册、中性工具（`AtomUI.Theme.Derivation`） | — |
| 控件包 | `AtomUI.Controls`、`AtomUI.Desktop.Controls`、`AtomUI.Desktop.Controls.ColorPicker`、`AtomUI.Desktop.Controls.DataGrid`… | 控件、ControlTheme、控件 Token 声明及其生成物（资源键、Schema、`XxxTokenResource`、`TokenSchemaDigest`） | 核心、所依赖的控件包 |
| 语言包 | `AtomUI.DesignLanguages.<Lang>` | 语言类、语言 Token、种子默认值与全局推导、具名主题、`Use<Lang>()` | 核心（派生语言另引用基语言的语言包） |
| 取值包 | `<控件包名>.Themes.<Lang>` | 该控件包每个控件 Token 类在该语言下的控件取值规则、`Add<控件包>()` | 对应控件包、对应语言包、所依赖控件包在同一语言下的取值包 |

- 每个带控件 Token 的控件包对应「语言数」个取值包，一对一，不合并：可选的控件包（ColorPicker、DataGrid）必须保持可选。
  没有控件 Token 的控件包（`AtomUI.Desktop.Controls.Extras`）不需要取值包。
- 仓库里按语言归拢：`src/DesignLanguages/<Lang>/` 下放语言包与该语言的全部取值包，解决方案分组 `/DesignLanguages/<Lang>/`。
  `src/DesignLanguages/AntDesign/`、`Shadcn/`、`PrimeNG/` 各有语言包与 4 个取值包。
- 应用开发者的心智模型只有一条：**用了哪个控件包，就装它的 `.Themes.<Lang>`**。语言包与 `AtomUI.Controls.Themes.<Lang>` 经
  传递依赖带入。名字里的 `DesignLanguages`（复数）避免与核心类型 `DesignLanguage` 冲突。
- 取值包只使用 Token 类及其属性的类型，不调用控件行为代码，不含控件或 ControlTheme。
- 语言包与取值包不使用 `InternalsVisibleTo` 访问核心或控件包：第一方没有特权通道。

构建期由生成器检查依赖方向，违反报 `ATOMUIGEN053`：语言包引用了控件包
（`src/AtomUI.Generator/DesignLanguages/DesignLanguageRegistrationGenerator.cs:68`）、控件包引用了语言一侧
（`src/AtomUI.Generator/ThemeSchema/TokenSchemaDigestWriter.cs:44`）、取值包里声明了控件
（`ControlTokenRuleAnalysis.ReportControlsInValuePackage`）。自带通用取值规则的控件包（如 GalleryBase）不受最后一条限制。

## 4. 词汇层

三类 Token 用同一套机制：`partial` 属性由生成器实现（不是 `partial` 时报 `ATOMUIGEN049`）、生成 Schema、带冻结语义的 setter、
可在主题与 `ThemeConfig` 里显式覆盖。

| | 全局 Token | 语言 Token | 控件 Token |
| --- | --- | --- | --- |
| 声明在 | 核心 | 语言包 | 控件包 |
| 表达 | 跨语言通用的角色 | 该语言独有的角色、几何、色阶 | 某个控件的取值与行为开关 |
| 谁给值 | 语言的 `DeriveGlobal`；对单个控件，由控件取值规则在其视图中覆盖 | 语言的 `DeriveLanguage` | 控件取值规则 |
| 谁能读 | 语言、全部控件取值规则、模板（经控件视图）、应用 | 该语言的全局推导与控件取值规则 | 该控件的模板与代码 |
| 主题覆盖 | `<Tokens>`、`ThemeConfig.Tokens` | `<LanguageTokens>`、`ThemeConfig.LanguageTokens` | `<Control>/<Tokens>`、`ThemeConfig.Controls` |
| Schema 阶段（`TokenStage`） | `Seed` / `Map` / `Alias` | `Seed`（语言种子）/ `Language` | `Control` |

- **全局 Token** 保留 Ant Design 形状的 Seed / Map / Alias 词汇与语义；语义说明是规范性的，在任何语言下表达同一个角色。成对角色
  （背景与其上的前景，例如 `ColorPrimary` 与 `ColorTextLightSolid`、`ControlItemBgActive` 与 `ControlItemColorActive`）成对声明、
  成对给值。
- **控件 Token** 只表达全局里没有的东西：「这个控件上的某个全局值」不是新的控件 Token，而是控件视图里对那个全局 Token 的覆盖。
  Token 类不含计算方法、默认值初始化器或对全局 Token 的引用。并存状态各有其名；行为开关是有类型的 Token。只被推导读取、
  模板不消费的 Token 标 `[ReservedToken]`。
- **语言 Token** 承载全局词汇装不下的概念（Shadcn 的 `--input`、`--ring`）。读取规则：一个值在全局 Token 里有位置时从全局读，只有
  全局没有的概念才从语言 Token 读，这样用户对全局 Token 的覆盖对所有语言一致生效。语言 Token 不得与全局 Token 同名（注册表构建
  时报错），只能是 `Seed` 或 `Language` 阶段。它不作为资源发布，控件模板看不见它；快照在内部保存它的值表
  只为求值复用。`AntDesignToken` 没有属性：Ant Design 的全局词汇就是它自己的形状。

## 5. 语言契约与四个轴

Ant Design、PrimeNG 和 Shadcn 的包内求值结构与命名见[语言包 Token 求值设计](language-token-evaluation.md)。

```csharp
public abstract class DesignLanguage<TLanguageToken> : DesignLanguage
    where TLanguageToken : LanguageDesignToken, new()
{
    public abstract string Id { get; }
    public abstract DesignLanguageAxes Axes { get; }

    public abstract void DefineSeeds(in DesignVariant variant, DesignToken seed, TLanguageToken languageSeed);
    public abstract void DeriveLanguage(in DesignVariant variant, DesignToken seed, TLanguageToken language);
    public abstract void DeriveGlobal(in TokenEvaluationContext<TLanguageToken> context, DesignToken token);
}
```

- 三个方法分别写全该变体下的全局与语言种子默认值、非种子语言 Token，以及 Map 与 Alias Token。
  `DeriveGlobal` 是一次运行时阶段，入口先应用 Map 规则再应用 Alias 规则。显式 Map / Alias 值在调用前冻结，Alias 读取已生效的 Map 值。
- 所有方法是输入的纯函数，可能在后台线程并发调用。通用色彩数学调用核心的中性工具（`PaletteGenerator`、`ColorMap`、
  `CalculatorUtils`），不得调用另一种语言。
- `Id` 必须返回常量字符串（构建契约从常量读取），符合 Identifier 词法。

`DesignLanguageAxes(Styles, DefaultStyle, Densities, Idioms, AdjustableSeeds)` 声明语言在各轴上提供的取值：

| 轴 | 类型 | 取值集合 | 谁决定当前取值 |
| --- | --- | --- | --- |
| 风格 | `DesignStyle` | 语言声明；Ant Design 只有 `Default` | 主题或选择 |
| 明暗 | `ThemeAppearance` | 固定 `Light` / `Dark`，与 Avalonia 主题变体对应，不需要声明 | 主题、选择（可跟随系统）或作用域 |
| 密度 | `DesignDensity` | 语言声明；核心预定义 `Default`、`Compact` | 主题、选择或作用域 |
| 形态 | `DesignIdiom` | 语言声明；核心预定义 `Desktop`、`Mobile` | 界面根；作用域可以显式覆盖 |

风格、密度、形态是开放的标识符值类型，新的语言或设备形态不需要改核心。明暗怎么算、每个密度档怎么算是语言自己的规则
（Ant Design 的 `MapTokenRules` 先跑默认算法，紧凑再叠紧凑算法，暗色再叠暗色算法，都是语言包的内部实现）。
请求了语言未声明的取值时，配置提交或主题应用时失败（`ATMTHM4005`、`ATMTHM5002`、`ThemeSelectionException`），不做静默替换。
语言独有的其他选择（Base Color、对比度等级）不新增轴，用语言种子表达。

## 6. 种子

- **全局种子**：全局 Token 的 Seed 层（主色、状态色、基础圆角、高度、字号、字体…），所有语言共用。
- **语言种子**：语言 Token 中标 `[DesignTokenKind(DesignTokenKind.Seed)]` 的属性，只属于该语言。

叠加顺序是 **语言默认种子（`DefineSeeds`）< 各层显式种子**（主题定义、应用初始配置、运行时请求、作用域，见第 10 节）。
品牌色、中性基调都通过种子表达，换了种子，语言推出全部派生值。

语言另外声明两项元数据：

- **可调种子**（`Axes.AdjustableSeeds`）：适合交给最终用户更换的种子（Ant Design：`ColorPrimary`）。它们必须是种子（注册表构建时
  检查）；界面据此提供色板；主题只能对它们写 `Locked="true"`（否则 `ATMTHM3004`）。
- **种子的派生集合**（`Axes.SeedDerivations`，可选）：每个可调种子派生出哪些 Token。主题未锁定某个种子却显式写了它的派生 Token 时，
  Binder 给出 `ATMTHM3006` 警告。

## 7. 派生语言

派生语言继承基语言的种子默认值、语言 Token、全局推导与全部控件取值规则，只重写不同的部分，用于品牌化。代码里它不是一个特殊的
声明，而是两条普通规则的结果：

1. 派生语言的语言 Token 类派生自基语言的语言 Token 类（单继承），例如 `SampleToken : AntDesignToken`。
2. 派生语言类本身是 `DesignLanguage<SampleToken>`，三个方法委托给基语言的一个实例再改写差异；`TokenEvaluationContext` 的构造函数是公开的，
   把同一个语言 Token 对象当作基语言的 Token 传入。

控件取值规则的查找沿语言 Token 类型的继承链进行（第 8 节），所以派生语言只为改动的控件写推导，其余落到基语言的推导上。基语言的取值包
仍要登记，但不必启用基语言本身：派生语言的入口用 `new DesignLanguageBuilder<AntDesignLanguage>(language.Root)` 调用基语言取值包的
`Add…()`。派生语言有自己的 `Id`，在目录里有自己的隐式条目。三种第一方语言之间不建立派生关系。

完整可运行的样例是 `tests/fixtures/DesignLanguageParity/`：`Sample.DesignLanguage`（派生自 Ant Design 的品牌语言）、`Sample.Controls`
（只有声明的样例控件包）、`Sample.Controls.Themes.AntDesign` 与 `Sample.Controls.Themes.Sample`（两个取值包，后者只覆盖 Badge，Tag 沿
继承链取 Ant Design 的推导），由 `tests/AtomUI.DesignLanguages.Parity.Tests` 验证。

## 8. 控件取值规则

公开接口与生成注册见[控件 Token 取值规则设计](control-token-rules.md)。

```csharp
[ControlTokenRule]
internal sealed class AntDesignSampleBadgeTokenRule : IControlTokenRule<AntDesignToken, SampleBadgeToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<SampleBadgeToken> target)
    {
        target.Own.BadgeBackground = target.Global.ColorPrimary;
        target.Own.BadgeSize       = target.Global.ControlHeightSM;
    }
}
```

- **身份是类型。** 推导通过泛型参数绑定到语言 Token 类型与控件 Token 类型；风格、明暗、密度、形态都是 `context.Variant` 里的参数。
  一种语言 Token 类型、一个控件只有一个推导，重复登记在注册表构建时报
  `Duplicate control token rule for '…' and control '…'.`。
- **写入目标。** `target.Own` 必须写全（构建期 `ATOMUIGEN050`；基类在别的包里声明的属性由契约测试检查）；不得先读后写
  （`ATOMUIGEN051`）。`target.Global` 读时得到该控件的值，写时只对该控件生效，**只能写 Map 与 Alias**：写种子在运行时直接抛出
  （`a control token rule may override Map and Alias tokens only, not seeds.`），包装为 `ThemeDerivationException`。语言要让
  某个控件的取值与全局不同时，模板读的是表达这个角色的 Map / Alias Token（如 `BorderRadiusMD`、`InputActiveBorderColor`）
  或控件 Token（如 Button 的 `LinkColor`、`DangerAccentColor`），语言在视图里覆盖前者、在控件 Token 上给后者赋值。
- **共享逻辑**放在取值包内的普通辅助类；跨端共用的抽象 Token 层（`AtomUI.Controls`）的取值由 `AtomUI.Controls.Themes.<Lang>` 提供公开的
  共享推导方法，各端取值包调用。

**组件配方**由语言 Token 提供。Shadcn 和 PrimeNG 的取值包通过 `context.Language` 和 `target.Global` 读取表格、列表项、
弹层与输入框各组角色。配方在本次控件取值规则期间按需读取源 Token，适用于同一语言的控件取值包及产品取值包。主题通过覆盖配方所读的
语言或全局 Token 改变取值。PrimeNG 的桌面取值包通过公开角色读取预设组件值与状态色阶，预设规则表和调色实现由语言包维护。

**查找只有一条规则**：沿当前语言的语言 Token 类型的继承链，由最具体到最一般，取第一个为该控件登记的推导。

```text
SampleToken : AntDesignToken : LanguageDesignToken

IControlTokenRule<SampleToken, SampleBadgeToken>            derived language's own derivation
IControlTokenRule<AntDesignToken, SampleTagToken>           base language's derivation
IControlTokenRule<LanguageDesignToken, RatingToken>         language-independent (generic) derivation
```

绑定到根类型 `LanguageDesignToken` 的**通用取值规则**只能读全局 Token，在任何语言下都能给出取值，供不打算逐语言适配的第三方控件使用；
第一方控件包不用它代替语言专属推导。

## 9. 单遍管线与冻结语义

每个**求值单元**（一个作用域，或一个覆盖了种子 / 固定了轴的控件）执行一次，每一步只执行一次，没有重跑、没有收敛判定、没有写入
记账（`ThemeCompiler.CompileUnitGlobal`、`ThemeCompiler.CompileControl`）：

```text
1  language and variant   style, appearance, density from theme, selection and scope; idiom from the interface root
2  seeds                  DefineSeeds, then the explicit seeds of every layer
3  language tokens        freeze explicit language tokens, DeriveLanguage
4  global tokens          freeze explicit Map and Alias tokens, DeriveGlobal
5  final normalization    language-independent core rules (EnableMotion=false zeroes the standard durations;
                          EnableWaveSpirit=false zeroes the wave range and start opacity)
6  control views          per control, lazily on first query: freeze the control's explicit Map/Alias and
                          control tokens, run its derivation
7  publish                hand the snapshot to the runtime publish layer
```

**冻结语义。** 进入第 3、4、6 步之前，编译器把各层显式给出的值写入 Token 对象并冻结：生成的 setter 是
`if (IsFrozen(nameof(X))) return; field = value;`，推导对冻结项的写入被忽略，读取得到显式值。因此：

- 推导就是普通的顺序赋值，不需要也不能判断「用户是否设置过」；
- 用户显式改了 A，后面由 A 算出的 B 自动跟随，不论同类还是跨类（语言 Token → 全局 Token → 控件 Token）；
- 优先级只有一条：**语言推导 < 主题显式值 < 更内层作用域的显式值**。

冻结按 Token **名称**记录（`AbstractDesignToken.Freeze(string)`，内部是一个按序数比较的名称集合），第 5 步之前清空。Token 对象只在
求值期间存在，渲染与资源查找读冻结后的值表。

**错误模型。** 语言的推导抛出时整次编译失败（`Design language '…' failed in its language|global derivation.`）；控件取值规则抛出或缺少
推导时，在首次查询该控件时抛出 `ThemeDerivationException`（`… failed to apply the token rule for control '…'.`），不使整次应用失败。

**发布层不变。** 本架构只替换「值怎么算」；按槽位的值表、`ThemeSnapshot`、只存相对全局差量的控件快照、惰性控件槽、两级缓存、
事务与发布顺序、零分配资源查询都沿用（见[主题系统架构](runtime.md)第 8、9、12、13 节与[惰性物化](lazy-materialization.md)）。
没有覆盖的控件与作用域共用同一份全局资源；推导或配置改写过全局 Token 的控件只存差量。缓存键含语言身份与修订、变体与全部显式值。

## 10. 作用域与控件级配置

- 作用域（`ThemeConfigProvider`）可以覆盖种子、三类 Token 中的任意值、明暗、密度、形态，也可以整体换语言
  （`ThemeConfigBuilder.WithLanguage`）。每个作用域是一个求值单元。
- 控件级配置（主题定义的 `<Control>`、`ThemeConfig.Controls`）写的就是该控件的视图，与控件取值规则写的是同一份，优先级为
  「语言推导 < 主题定义 < 作用域与运行时配置」。规则只有两条，取代 v1 的四种控件算法模式：
  1. 只写 Map、Alias 或控件 Token：只替换那一项，不触发派生。
  2. 覆盖了种子，或固定了 `Appearance` / `Density`：该控件自成求值单元，管线为它单独求一份全局 Token（条件段按它的有效变体匹配），再求
     它的视图。固定的轴只影响该控件，不改变所在作用域的 Avalonia 主题变体。覆盖的种子在视图里就是写的值（语言的全局推导对种子的细化
     或替换不作用于控件级取值）。
- 根上显式写的全局 Token 改变的是共享全局值，语言对某个控件视图的覆盖仍然生效（与 antd 组件级 Token 优先于全局 Token 一致）。要改
  这个控件，写它的控件级配置；要让一类控件一起变，覆盖它们共同依据的语言 Token。

**轴的确定**先于取值：

```text
root:     language default <- selection (axes the theme fixes win) <- initial config <- runtime request
scope:    parent scope's effective axes <- axes written in this scope's Config
control:  scope's effective axes <- axes the definition and the configs fix for that control
idiom:    the interface root; Desktop when nothing gives it; a scope may override it
```

**取值的分层**（低到高，对每个求值单元各做一次）：语言推导 ← 所选主题定义（基础段，再按文档顺序应用匹配的 `<When>`）← 应用初始配置
← 运行时请求（含选择带来的种子）← 父作用域有效配置 ← 局部 `ThemeConfigProvider.Config`。明暗与密度各自继承：父作用域暗色、子作用域
只写 `Density = Compact`，结果是暗色紧凑。`Inherit = false` 的两种含义见[合并规则](runtime.md#43-合并规则)。

## 11. 主题与选择

**主题只描述配置，不描述计算。** XML v2 主题必须声明 `Language`；根上写了 `Style` / `Appearance` / `Density` 即固定，不写即开放；变体之间
的差异写在 `<When>` 条件段里，一个主题一个文件；没有 `<Algorithms>`、`IsDefault`。格式见
[主题定义 XML v2](../../../reference/theming/theme-definition-xml-v2.md)，编译进包的主题走[编译主题包](compiled-theme-packages/overview.md)
（ABI 4）。

**目录。** `IThemeManager.Themes` 列出所有具名主题（`ThemeInfo`：语言、固定的轴、`LockedSeeds`、`AccentColor`），另为每个已启用语言列出
隐式条目 `$<LanguageId>`（`ThemeInfo.LanguageDefaultId`，`IsLanguageDefault = true`），代表「该语言、不带主题文件」的默认外观。语言本身
不需要任何主题文件。任意来源之间重复的 `Theme.Id` 是错误。当前所选主题因目录刷新而消失时，回到同一语言的隐式条目，变体不变。

**选择。** `ThemeSelection` 是面向最终用户的一层；`IThemeManager.ApplyAsync(ThemeSelection)` 应用它，`CurrentSelection` 记录它：

```csharp
await themeManager.ApplyAsync(ThemeSelection
    .Of<AntDesignLanguage>()
    .WithTheme("DaybreakBlue")
    .WithAppearance(ThemeAppearance.Dark)
    .WithDensity(DesignDensity.Compact)
    .WithSeed("ColorPrimary", "#52C41A"));
```

`ThemeSelectionResolver.Resolve` 是纯函数，只有三条规则，没有回退搜索：(1) 语言已启用、主题属于该语言、风格与密度在语言声明的范围内，否则
抛 `ThemeSelectionException`；(2) 主题固定的轴胜出，被忽略的轴记在 `ThemeSelectionResult.IgnoredAxes`；(3) 主题锁定的种子不被替换，记在
`IgnoredSeeds`。`FollowSystemAppearance()` 由主题管理器按平台当前明暗解析，平台变化时重新应用。界面所需的三样只读数据是
`IThemeManager.Languages`（`DesignLanguageInfo`）、`Themes` 与 `CurrentSelection` / `CurrentTheme`。

## 12. 注册、启动与校验

### 显式取值注册

应用可通过语言构建器直接登记自定义取值，或在缺少生成的应用启动映像时登记取值。

```csharp
.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.UseDesktopDataGrid();

    builder.UseAntDesign(language => language.AddDesktopControls().AddDesktopDataGrid());

    builder.WithInitialTheme(ThemeSelection.Of<AntDesignLanguage>());
})
```

- `Use<Lang>()` 由语言包提供：调用生成的 `DesignLanguageRegistration.Register`（经 `IThemeManagerBuilder.AddDesignLanguage`
  登记 `DesignLanguageDescriptor`），并注册语言自带的编译主题包。
- `Add<控件包>()` 由取值包提供，是 `DesignLanguageBuilder<TLanguage>` 上的扩展方法：调用生成的
  `GeneratedControlTokenRuleRegistration.Register`（经 `AddControlTokenRules` 登记 `ControlTokenRuleDescriptor`），并带上所依赖控件包的取值
  （`AddDesktopControls()` 调用 `AddCommonControls()`）。它**只登记取值**，不启用控件包。
- 控件包由 `Use<控件包>()` 启用，应用至少启用一种语言。显式登记的取值优先用于对应的控件包与语言组合。
- 注册表由生成代码产出（静态、无捕获的委托），不依赖反射，满足裁剪与 AOT。语言与取值包的修订取自程序集 MVID（确定性构建），不手工维护
  revision。

**配置提交时的校验**（注册表构建，任一失败即启动失败，`ThemeSchemaRegistry.ValidateTokenRuleCoverage`）：

1. **覆盖**：每个已启用语言 × 每个已启用控件里有控件 Token 的类，都能按第 8 节规则找到推导；否则报
   `Controls without a token rule in an enabled design language: '<control>' under '<language>' (install and enable '<assembly>.Themes.<language>')`。
2. **Token Schema 摘要**：控件包生成 `[assembly: TokenSchemaDigest]`，取值包为它覆盖的每个控件包生成
   `[assembly: CoversTokenSchema(assemblyName, digest)]`；两者不一致（应用升级了控件包而取值包是旧的）即失败，消息写出两个包名与两个摘要。
3. **初始选择**：语言已启用，风格、明暗、密度在语言声明的范围内（选择解析的规则 1）。
4. **描述符自洽**：语言 Token 只用 `Seed` / `Language` 阶段且不与全局 Token 同名、可调种子是种子、推导不重复；默认风格不在
   声明的风格里时，`DesignLanguageDescriptor` 构造即失败。

运行时经 `LoadPackages` 追加包时执行同样的校验，另加一条：新推导若会被已提交的语言为已提交的控件找到（会改变已提交的取值），整次追加
被拒绝（`ThemeSchemaRegistry.ValidateAppendedTokenRules`）。构建期另有 `ATOMUIGEN052`：取值包缺少它所覆盖控件包某个控件 Token 类的推导。

**契约测试库** `tests/AtomUI.Theme.ContractTests`（`DesignLanguageContract.Verify`）在语言声明的每个变体下检查 `Identity`、`LanguageComplete`、
`GlobalComplete`、`ControlComplete`、`Pure`、`Deterministic`、`Readable`，第三方语言与取值包可以直接复用。

### 应用注册与启动 API

应用各声明一次要启用的控件包和设计语言，为各语言指定可选的默认主题，再选择应用的初始语言。Gallery 的语言选择由应用配置负责，
`UseGalleryControls()` 登记 Gallery 控件包及其控件依赖。

```csharp
builder.UseDesktopControls();
builder.UseDesktopColorPicker();
builder.UseDesktopDataGrid();
builder.UseGalleryControls();

builder.UseAntDesign(ant => ant.WithDefaultTheme(AntDesignThemes.DaybreakBlue));
builder.UsePrimeNG();
builder.UseShadcn();

builder.WithInitialDesignLanguage<AntDesignLanguage>();
```

- **取值包匹配。** 生成器根据应用引用的取值包生成静态注册入口。提交配置时，按已启用的「控件包 × 语言」匹配注册入口，
  并保留覆盖、Token Schema 摘要和语言继承校验。可选控件包保持独立，未启用的控件包不参与匹配。绑定使用直接调用，支持裁剪和 NativeAOT。
- **匹配结果。** 唯一候选自动选用。缺少必需的取值包时报出控件包、语言和应引用的包。多个候选时报出冲突来源，
  应用可通过 `ant.UseValues(GeneratedControlTokenRuleRegistration.Package)` 指定对应取值包的生成入口。包引用与注册顺序不决定选用哪一套。
- **语言内的默认主题。** `WithDefaultTheme` 只记录所属语言的具名主题，提交时校验其归属。首次切入该语言且没有显式选择时使用这个默认主题；
  没有配置时使用语言本身的默认风格与隐式主题。显式选择的主题或风格优先，主题已有的固定轴和锁定种子继续生效。
- **初始根选择。** `WithInitialDesignLanguage<TLanguage>()` 选择已启用语言及它的默认主题。只启用一种时可省略；
  启用多种时必须显式选择。恢复上次完整选择的应用可使用 `WithInitialTheme(ThemeSelection)`。两个入口互斥，同时设置时报错。

#### 启动求值边界

应用可登记多种设计语言。启动时，初始根选择决定唯一需要求值的语言。其余语言只参与描述符、主题目录与取值包登记及静态覆盖校验。
这些步骤和语言菜单均不得为尚未应用的语言创建 Token 或调用 `DefineSeeds`、语言推导、全局推导或控件取值规则。
该语言首次被根、作用域或预览实际应用时，才按目标变体求值。目录展示使用声明的元数据，控件快照仍按资源查询求值。

隐式主题的色样与种子文本来自语言声明的 `DefaultThemeSeeds`。未声明时目录省略这些展示值。主题计算仍由
`DefineSeeds` 提供真实的变体种子。系统明暗配置指向不同语言时，启动准备阶段只登记两个选择，等系统外观确定后编译对应语言。

## 13. 形态与扩展点

形态由界面根决定，应用切换主题、明暗、密度时形态不变；语言在三个方法里都可以按 `variant.Idiom` 给值，同一份种子在两种形态下得到两套语言
Token 与全局 Token。Ant Design 语言声明 `Desktop` 与 `Mobile`；主仓里没有界面根给出 `Mobile`，形态缺省为 `Desktop`，移动端的界面根与
`AtomUI.Mobile.Controls.Themes.AntDesign` 在 AtomUI.Mobile 仓库里落地。桌面控件与移动控件各有各的 Token 类型，只有 `AtomUI.Controls` 里
真正跨端的控件共用 Token 类型。

| 想做的事 | 做法 | 改动核心或既有包吗 |
| --- | --- | --- |
| 换品牌色、圆角、字体 | 具名主题或 `ThemeConfig` 覆盖种子 | 不需要 |
| 在某种语言上做品牌化 | 具名主题；需要不同推导规则时写派生语言 | 不需要 |
| 新增一种设计语言 | 一个语言包，加上为要支持的控件包各写一个取值包 | 不需要 |
| 为别人的控件包追加一种语言 | 引用它的控件包与语言包，写一个取值包 | 不需要 |
| 新增一个控件包 | 控件包 + 各语言取值包；或只提供通用取值规则；或控件与推导放在同一个程序集 | 不需要 |
| 语言需要自己的输入或概念 | 语言种子、语言 Token | 不需要 |
| 给语言增加风格、密度档 | 在该语言的 `Axes` 里声明 | 只改该语言包及其取值包 |
| 新增设备形态 | 新的 `DesignIdiom` 标识；语言声明支持；界面根给出 | 不需要 |
| 新增主题来源 | 自定义 `IThemeDefinitionResolver` | 不需要 |
| 运行时加载带控件的插件 | 插件带控件包与取值包，经 `LoadPackages` 追加 | 不需要 |

有意保留的边界：模板只有一套，语言不能自带模板（结构差异由控件增加能力并以行为开关控制）；全局词汇是 Ant Design 形状的，其他语言的概念
靠语言 Token 承载；取值代码随语言数增长，不提供跨语言的隐式回退。

## 14. 与方案的差异

以下是当前代码与方案不一致之处，以代码为准：

| 方案 | 代码 | 依据 |
| --- | --- | --- |
| 6.3：冻结是每个 Token 对象上按槽位的一组位 | 按名称冻结：生成的 setter 检查 `IsFrozen(nameof(X))`，冻结项是名称集合。抽象中间层的同一属性在不同终端类里槽位不同，按名称省去逐终端的映射表 | `src/AtomUI.Core/Theme/DesignTokens/AbstractDesignToken.cs` 的 `IsFrozen` / `Freeze` / `TokenFreezeSet` |
| 14 的覆盖校验：取值包为其控件包的每个 Token 类声明一条规则 | 派生语言只写改动的控件，其余由启动时的覆盖校验沿继承链确认 | `ControlTokenRuleAnalysis.ReportMissingCoverage`（`ATOMUIGEN052`） |
| 5.6：语言「声明一个基语言」 | 没有基语言声明：派生关系就是语言 Token 类的继承加上对基语言实例的委托；派生语言的取值包引用基语言的取值包以便登记它们（方案 3.3 的引用规则没有列出这一例外，`ATOMUIGEN053` 也不检查取值包之间的引用） | `tests/fixtures/DesignLanguageParity/Sample.Controls.Themes.Sample/Sample.Controls.Themes.Sample.csproj` |
| 8 的校验 2：控件包要求的形态都在已启用语言声明的形态内 | 未实现：控件包注册不声明形态，形态缺省 `Desktop` | `src/AtomUI.Core/Theme/Configuration/NormalizedThemeConfig.cs:81`、`src/AtomUI.Core/Theme/ControlPackageRegistration.cs` |
| 12.3：读集记录与更细的结构共享 | 只记录控件取值规则对全局 Token 的读集：子求值单元只改了 Map / Alias 时，读集不相交的控件复用父单元的结果。语言 Token 由各控件共享、可能并发求值，不在其上记读集：语言 Token 一变，全部控件重算；改了种子或调色板时同样全部重算 | `src/AtomUI.Core/Theme/Compilation/GlobalTokenReadSet.cs`、`ThemeCompiler.FindReadSetReuse` |
| 5.5：组件配方（Table、ListItem、Overlay、Field…） | Shadcn 与 PrimeNG 提供 Table、ListItem、Overlay、Field 配方，沿用各桌面取值包的现有映射。Shadcn 另提供 Chart 系列色，PrimeNG Table 提供条纹背景角色。Touch 属于移动形态，Ant Design 的跨产品配方仍由消费阶段核对 | `ShadcnComponentRecipes.cs`、`PrimeNGComponentRecipes.cs`、`AntDesignToken.cs` |
| 7.10：编译主题「ABI v2」 | 编号为 ABI 4（1–3 已用过），即方案所说的第二代格式 | `docs/reference/theming/compiled-theme-abi-v4.md` |
| 8：追加包执行与启动相同的四项校验 | 另加一条：追加的推导不得改变已提交语言下已提交控件的取值 | `src/AtomUI.Core/Theme/Schema/ThemeSchemaRegistry.cs` 的 `ValidateAppendedTokenRules` |

## 相关文档

- [设计语言架构方案](../../../superpowers/specs/2026-11-22-design-language-architecture-design.md)：完整设计依据与待定点。
- [主题系统架构](runtime.md)：配置合并、编译器、快照、缓存、作用域、事务与资源发布。
- [编写设计语言](../../../guides/theming/writing-design-languages.md)：语言包与取值包的写法、构建期诊断与契约测试。
- [设计语言包指南](../../../guides/theming/design-language-packages.md)：应用引用哪些包、如何选择语言与具名主题。
- [第三方 AtomUI Control Package 指南](../../../guides/theming/third-party-control-packages.md)：控件包的 Token 声明与取值包。
- [主题定义 XML v2](../../../reference/theming/theme-definition-xml-v2.md)与[编译主题 ABI v4](../../../reference/theming/compiled-theme-abi-v4.md)。
