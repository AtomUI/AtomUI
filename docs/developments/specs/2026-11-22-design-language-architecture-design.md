# AtomUI 设计语言架构方案

日期：2026-11-22　基线：release/6.0 `8c946e4f5`　状态：已确认（2026-11-22）；第 18 章的待定点按文中现设计执行，实现评审时逐项定稿
范围：AtomUI 主仓（核心、跨端共用控件、桌面控件）、AtomUI.Mobile、全部商业产品库、第三方
前提：不考虑向后兼容，只考虑架构最优

用词：**必须**／**不得** 为强制；**应当** 为默认做法，偏离时在 README 写明理由；**可以** 为许可。
文中的 C# 签名是示意，表达职责与形状，具体命名与签名在实现评审时定稿。

---

## 1. 背景与目标

### 1.1 现状的问题

AtomUI 最初只支持 Ant Design，取值规则因此散落在两处：全局规则在核心（Default / Dark / Compact 计算器、
`CalculateAliasTokenValues`），控件规则在每个控件 Token 类的 `CalculateTokenValues` 里。后来加入的 PrimeNG 与
Shadcn 只能挂在这套结构之外，以补丁方式生效：

| 现象 | 实质 |
| --- | --- |
| 语言算法先跑一遍 Ant 计算器，再改写自己的值；调用前临时改写种子、调用后还原 | 语言是叠在 Ant 之上的补丁 |
| Alias 阶段在 Ant 公式之后抢写，编译器多轮重跑直到稳定，超过 3 轮判 `Unstable` | 两套规则争同一批值 |
| 控件阶段只写与 Ant 不同的值；`EvaluateGlobal` 改写一份全局副本，再让 Ant 的公式在副本上重跑 | 语言的意图藏在别人的公式里，靠重跑与写入记账才能收敛 |
| 语言是算法链上的一个环节：`Nova.Dark` 式身份、每个阶段类 16 个特性、每个风格四份主题 XML、合法但无意义的混链 | 链是 Ant 的概念，其他语言没有这个结构 |
| Token 表达不了的行为差异走 `AppearanceSettings`（`internal object?`）；语言包靠 `InternalsVisibleTo` 才写得出来 | 第一方有特权，第三方与商业产品做不到同样的事 |
| 商业产品只能从全局 Token 推导，在非 Ant 语言下状态塌缩、前景背景失配 | 产品没有按语言给值的通道 |

### 1.2 目标

1. Ant Design、Shadcn、PrimeNG 以及以后的任何设计语言是**平级**的，谁也不叠在谁上面。
2. 桌面端与移动端用**同一套架构**，同一个语言定义服务两端。
3. OSS 控件、商业产品、第三方控件走**同一条路径**，第一方没有任何特权通道。
4. 依赖方向单一且正确：语言不依赖控件实现，控件实现不依赖语言。
5. 每一个取值都有明确的负责人，没有隐式回退，缺失在构建期或启动期暴露。

### 1.3 不做的事

- 不重新设计一套语言中立的全局词汇：全局 Token 保留 Seed / Map / Alias 三层及现有的名字与语义。
- 不按语言拆分控件或 ControlTheme，不做 lookless 重构：控件与模板只有一套。
- 不涉及字体库与应用自用的图标库。控件内置图标属于设计语言，见 4.3。

---

## 2. 总体架构

### 2.1 核心思想

**词汇是契约，设计语言是契约的完整实现。**

- **词汇**：有哪些 Token、各自表达什么。全局词汇由核心定义，控件 Token 由各控件包在控件定义处声明。所有语言共用。
- **取值**：每个 Token 等于多少、怎么从种子推出来。全部归设计语言，核心与控件不含任何取值规则。
- **控件实现**：只消费 Token，不知道当前是哪种语言。

### 2.2 设计原则

1. **控件只声明，不计算。** Token 类是纯声明；`CalculateTokenValues` 与 `CalculateAliasTokenValues` 不复存在。
2. **语言拥有整条推导。** 种子 → 全局 Token → 每个控件的视图（对全局 Token 的覆盖与该控件的控件 Token），由当前语言一次算出。
3. **语言是一等对象。** 主题 = 语言 + 风格 + 明暗 + 密度 + 种子 + 显式覆盖。「算法链」从核心移除。
4. **Token 的声明跟着控件，取值跟着语言。** 控件 Token 声明在控件定义处；它们等于多少、控件看到的全局 Token 是否不同，由各语言的取值包给出。
5. **三类词汇，同一套机制。** 全局 Token 归核心，控件 Token 归控件，装不进全局词汇的概念（Shadcn 的 `--input`、PrimeNG 的
   highlight）成为该语言自己的「语言 Token」。三者的声明、给值、覆盖方式完全相同，核心不为语言的概念建立跨语言抽象。
6. **差异只通过 Token 表达。** 包括行为差异：它们是 Token 上有类型的开关。模板不得判断当前语言。
7. **单遍求值。** 没有重跑、没有收敛判定；显式值是输入而不是事后补丁。
8. **第一方无特权。** Ant Design 自己也只用公开 API 写成。
9. **取值有出处。** 每个取值指到该设计语言官方源码或文档的一处；官方未定义的标为 AtomUI 扩展。

### 2.3 分层

```text
                           AtomUI.Core
               全局词汇 · 语言契约 · 推导管线 · 中性工具
                 ^                              ^
                 |                              |
            <控件包>                    AtomUI.DesignLanguages.<Lang>
   控件 · ControlTheme · 控件 Token 声明   语言包：语言定义、语言 Token、全局推导
                 ^                              ^
                 |                              |
                 +---- <控件包>.Themes.<Lang> ---+
                       取值包：该控件包的控件 Token 在该语言下的控件推导
```

控件包与语言包之间没有任何引用，它们只在取值包里相遇。

### 2.4 概念

| 概念 | 定义 | 所在 |
| --- | --- | --- |
| 全局 Token | Seed（输入）/ Map（色阶与尺寸阶梯）/ Alias（语义角色），共 257 个，纯声明 | 核心 |
| 控件 Token | 某个控件特有的可主题化取值与行为开关，全局 Token 里没有对应项；纯声明 | 控件包，与控件定义在一起 |
| 控件视图 | 一个控件实际读到的全部 Token：全局 Token 加该控件的控件 Token，其中任意一项都可以只对该控件取不同的值。语言、主题、作用域写入的是同一份视图 | 运行时值 |
| 设计语言 | 实现语言契约的类型：声明自己的轴，推导全部全局 Token | 语言包 |
| 变体 | 风格 × 明暗 × 密度 × 形态的一个组合；风格、密度、形态的取值集合由语言声明 | 运行时值 |
| 形态 | 桌面或移动。由界面根决定，不由主题决定 | 运行时值 |
| 语言 Token | 语言自己的词汇：全局 Token 里没有位置的角色、几何与色阶。与全局 Token、控件 Token 同等地位，可被显式覆盖 | 语言包 |
| 组件配方 | 语言 Token 上按官方组件给出的成组只读视图（Table、ListItem、Overlay、Field…） | 语言包 |
| 控件推导 | 「某语言下某个控件的视图怎么算」的一个类 | 取值包 |
| 主题 | 语言 + 风格 + 明暗 + 密度 + 种子 + 显式覆盖的一份配置 | 应用或任意包 |

---

## 3. 包与仓库

### 3.1 四类包

| 类别 | 命名 | 包含 | 引用 |
| --- | --- | --- | --- |
| 核心 | `AtomUI.Core` | 全局 Token 声明；语言契约；推导管线；主题定义、选择、注册；中性工具 | — |
| 控件包 | `AtomUI.Controls`、`AtomUI.Desktop.Controls`、`AtomUI.Mobile.Controls`、`AtomUI.MachGrid`… | 控件、ControlTheme、**控件 Token 的声明**及其生成物（资源键枚举、Schema、Token 标记扩展） | 核心、所依赖的控件包 |
| 语言包 | `AtomUI.DesignLanguages.<Lang>` | 语言定义、语言 Token 声明、语言与全局的推导、组件配方、具名主题 | 核心 |
| 取值包 | `<控件包名>.Themes.<Lang>` | 该控件包的全部控件 Token 在该语言下的控件推导 | 该控件包、对应语言包、所依赖控件包的取值包 |

AtomUI 的控件不是 lookless 的：一个控件的 Token 与它的模板一一对应、一起演进。所以 **Token 的声明留在控件定义处**——
与控件类、ControlTheme 同一个目录、同一个程序集，生成的 Token 枚举与标记扩展在控件包内直接可用。移出控件包的只有
「这些 Token 等于多少」：它因语言而异，归各语言的取值包。

每个控件包对应「语言数」个取值包，一对一，没有例外：

```text
AtomUI.Desktop.Controls                    AtomUI.Desktop.Controls.Themes.AntDesign / .Shadcn / .PrimeNG
AtomUI.Desktop.Controls.ColorPicker        AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign / …
AtomUI.MachGrid                            AtomUI.MachGrid.Themes.AntDesign / …
```

### 3.2 全量清单

```text
主仓（AtomUIV6）
  AtomUI.Core
  AtomUI.DesignLanguages.AntDesign                      语言本身（基础设施，应用不直接引用）
  AtomUI.DesignLanguages.Shadcn
  AtomUI.DesignLanguages.PrimeNG

  AtomUI.Controls                                       跨端共用控件
  AtomUI.Controls.Themes.<Lang>                         随各端的取值包传递进来
  AtomUI.Desktop.Controls
  AtomUI.Desktop.Controls.Themes.<Lang>                 桌面应用引用的就是它
  AtomUI.Desktop.Controls.ColorPicker
  AtomUI.Desktop.Controls.ColorPicker.Themes.<Lang>
  AtomUI.Desktop.Controls.DataGrid
  AtomUI.Desktop.Controls.DataGrid.Themes.<Lang>

移动仓（AtomUI.Mobile）
  AtomUI.Mobile.Core                                    界面根、平台适配、视口、手势（与设计语言无关）
  AtomUI.Mobile.Controls
  AtomUI.Mobile.Controls.Themes.AntDesign               必须
  AtomUI.Mobile.Controls.Themes.<Lang>                  其他语言按规划

各产品仓（以 MachGrid 为例）
  AtomUI.MachGrid
  AtomUI.MachGrid.Themes.AntDesign                      必须
  AtomUI.MachGrid.Themes.<Lang>                         其他语言按规划
```

`<Lang>` 取 `AntDesign`、`Shadcn`、`PrimeNG`。没有控件 Token 的控件包（如 `AtomUI.Desktop.Controls.Extras`）不需要取值包。

**仓库里的组织。** 每个仓库把设计语言相关的工程按语言归拢，磁盘目录与解决方案分组一致，做法与本地化包
（`src/LanguagePacks/pt-BR/`、`/Localization/pt-BR/`）相同：

```text
src/DesignLanguages/AntDesign/        解决方案分组 /DesignLanguages/AntDesign/
    AtomUI.DesignLanguages.AntDesign/
    AtomUI.Controls.Themes.AntDesign/
    AtomUI.Desktop.Controls.Themes.AntDesign/
    AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign/
    AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign/
src/DesignLanguages/Shadcn/           同构
src/DesignLanguages/PrimeNG/          同构
```

移动仓与各产品仓没有语言包，只在同名目录与分组下放自己的取值包。新增一种语言时只新增一个目录与一个分组。

### 3.3 依赖规则

| 引用方 | 可以引用 | 不得引用 |
| --- | --- | --- |
| 控件包 | 核心、所依赖的控件包 | 任何语言包、任何取值包 |
| 语言包 | 核心 | 任何控件包、任何取值包、其他语言包（派生语言引用基语言除外） |
| 取值包 | 对应的控件包、对应的语言包、所依赖控件包在同一语言下的取值包 | 其他语言的语言包与取值包 |

- **控件不认识任何语言，语言不认识任何控件，两者只在取值包里相遇。** OSS 控件与商业产品走完全相同的结构。
- 取值包引用控件包是为了拿到 Token 类型。推导只应使用 Token 类及其属性的类型，不得调用控件的行为代码，也不得在
  取值包里放控件或 ControlTheme。
- 控件包与它的取值包同仓、同版本发布；取值包对语言包声明最低版本。
- 控件包带有自身 Token Schema 的摘要，取值包在构建期记录它所覆盖的摘要。启动时两者不一致（例如应用升级了控件包，
  而第三方取值包还是旧的）即失败并指出不匹配的包，不会带着缺值运行。
- 每个第一方控件包必须有 Ant Design 的取值包；其他语言按产品规划提供。
- `<Lang>` 用官方名的帕斯卡写法：`AntDesign`、`Shadcn`、`PrimeNG`。

**命名与应用开发者的视角。** 两类语言一侧的包用不同的词，避免让人在两个「Themes」之间做选择：

| 包 | 名字里的词 | 谁会直接引用 |
| --- | --- | --- |
| `AtomUI.DesignLanguages.<Lang>` | `DesignLanguages`：它定义语言本身，是基础设施 | 取值包的作者、语言的派生者。应用不直接引用，它随取值包传递进来 |
| `<控件包名>.Themes.<Lang>` | `Themes`：它让这个控件包在该语言下有外观 | 应用开发者 |

应用开发者的心智模型只有一条：**用了哪个控件包，就装它的 `.Themes.<Lang>`**。桌面应用装
`AtomUI.Desktop.Controls.Themes.AntDesign`，用了 DataGrid 再装 `AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign`，用了
MachGrid 再装 `AtomUI.MachGrid.Themes.AntDesign`。`AtomUI.Controls.Themes.<Lang>` 与语言包经传递依赖自动带入，`UseAntDesign()`
等入口定义在语言包里。

`DesignLanguages` 用复数：核心里有名为 `DesignLanguage` 的类型，命名空间若叫 `AtomUI.DesignLanguage`，所有 `AtomUI.*`
命名空间下的代码写 `DesignLanguage` 时都会先解析到这个命名空间而编译失败。复数形式也与 `AtomUI.Icons.*`、
`AtomUI.Fonts.*` 的习惯一致。

### 3.4 为什么这样分

- **Token 声明留在控件包**，是因为它描述的是这个控件的模板需要哪些值。控件作者加一个可主题化的地方，改的是同一个
  目录里的控件、模板和 Token；生成的枚举与标记扩展就在手边。把声明拆到另一个包，换来的只是取值包不必引用控件包，
  而取值包本来就是专属于某一个控件包的。
- **取值移出控件包**，是因为同一个 Token 在不同设计语言下的算法不同。放在控件包里，控件就得知道所有语言，或者只能
  内置一种、其余靠事后改写——这正是现状的问题。
- **语言包不带任何控件的取值**，是因为它要同时服务桌面、移动和所有产品。它只定义语言本身，任何控件包的取值包都可以
  在它之上建立。
- **取值包与控件包一对一**，是因为可选的控件包必须保持可选：把 ColorPicker、DataGrid 的取值并进桌面取值包，就会让
  每个桌面应用都带上这两个控件包。

---

## 4. 词汇层

### 4.1 全局 Token

- 集合、名字、类型与 Seed / Map / Alias 分层由核心定义。每个 Token 的语义说明是规范性的：它在任何语言下表达同一个
  角色，只是取值不同。
- `DesignToken` 是纯声明，不含任何公式。
- 新增全局 Token 的门槛：对多数控件有意义，且每种第一方语言都给得出取值。不得为承载某一种语言独有的概念而新增，
  那类概念放进该语言的语言 Token。
- **成对角色**（背景与其上的前景）必须成对声明。语言必须成对给值并保证可读对比度；控件推导取了背景就必须取成对的
  前景。

  | 背景 | 前景 |
  | --- | --- |
  | `ColorPrimary`、`ColorBgSolid` 族 | `ColorTextLightSolid` |
  | `ControlItemBgActive` 族 | `ControlItemColorActive` |
  | `ColorBgSpotlight` | `ColorTextLightSolid` |
  | `ColorBgContainer`、`ColorBgElevated`、`ColorBgLayout` | `ColorText` 族 |

### 4.2 控件 Token

```csharp
// AtomUI.Desktop.Controls/Tag/TagToken.cs, next to Tag.cs and Themes/TagTheme.axaml
[ControlDesignToken]
public sealed partial class TagToken : AbstractTagToken
{
    /// <summary>Background of a default tag.</summary>
    public partial Color DefaultBg { get; set; }

    /// <summary>Text colour of a default tag; pairs with <see cref="DefaultBg"/>.</summary>
    public partial Color DefaultColor { get; set; }

    /// <summary>Whether a filled status tag uses the solid status fill with the contrast text.</summary>
    public partial bool FilledStatusSolid { get; set; }
}
```

- **控件 Token 只表达全局里没有的东西。** 一个值如果只是「这个控件上的某个全局值」，它不是新的控件 Token，而是该
  控件视图里对那个全局 Token 的覆盖（6.1）。不得声明只是给全局 Token 换个名字的控件 Token。
- Token 类位于控件包，与控件类、ControlTheme 同目录，`public partial`，只有属性声明与语义说明。不得包含计算方法、
  默认值初始化器或对全局 Token 的引用。
- 属性类型可以是核心与 Avalonia 的基础类型（颜色、厚度、圆角、阴影、字重、时长、数值、布尔），也可以是控件包里
  定义的枚举。
- 控件模板与代码中的每个可主题化取值必须来自控件 Token 或全局 Token，不得写死。
- **并存状态各有其名。** 一个控件上可能同时出现的状态（悬停、选中、选中悬停、聚焦、禁用、排序、分组等）各有独立的
  控件 Token，背景类 Token 有对应的前景类 Token。不得让两个状态共用一个 Token。
- **行为开关是 Token。** 取值表达不了的语言差异——禁用用透明度还是灰色、按下是位移还是变色、链接是否带下划线、
  分隔线是否内嵌——声明为有类型的控件 Token 属性，由语言像其他 Token 一样给值。模板通过它切换样式或伪类。不得存在
  任何无类型的旁路通道。
- **语义说明是语言作者的唯一依据**，必须写清画在哪里、表达什么状态、与哪个 Token 成对。
- 抽象中间层只用于在多个 Token 类之间共享声明。
- 控件与 Token 类型的绑定方式沿用现状，由生成器在控件包内建立。

### 4.3 图标与动效也是 Token

控件内部用到的图标（关闭、展开箭头、勾选、加载等）与动效（时长、缓动曲线、进出场方式）属于设计语言的一部分：

- 控件内置图标声明为图标类型的控件 Token，由语言给值；模板从 Token 取图标，不写死某个图标库。取值包因此可以引用
  该语言对应的图标包（Ant Design 的取值包引用 `AtomUI.Icons.AntDesign`）。应用自己使用的图标库不在本方案范围内。
- 动效的时长与曲线是全局 Token，进出场方式等选择是控件 Token 上的枚举开关。
- 关闭动效这类与语言无关的可访问性规则由核心在最终规范化阶段统一执行（6.2），不依赖各语言自觉处理。

### 4.4 Token 的生成物

Token 生成器在控件包内产出：属性实现（含 6.3 的冻结语义）、资源键枚举、Schema 描述、供 AXAML 使用的强类型 Token
标记扩展。它们与现在一样位于控件包的 `DesignTokens` 命名空间，控件代码与模板里的写法和 IDE 智能提示不变；取值包
通过引用控件包使用同一批类型。

---

## 5. 设计语言层

### 5.1 语言契约

```csharp
public abstract class DesignLanguage<TLanguageToken> : DesignLanguage
    where TLanguageToken : LanguageDesignToken, new()
{
    /// <summary>Stable identity, e.g. "Shadcn".</summary>
    public abstract string Id { get; }

    /// <summary>The styles, appearances, densities and idioms this language offers, and its defaults.</summary>
    public abstract DesignLanguageAxes Axes { get; }

    /// <summary>The language's default seeds for a variant: the global seeds and the language's own seeds.</summary>
    public abstract void DefineSeeds(in DesignVariant variant, DesignToken seed, TLanguageToken languageSeed);

    /// <summary>Writes every derived language token.</summary>
    public abstract void DeriveLanguage(in DesignVariant variant, DesignToken seed, TLanguageToken language);

    /// <summary>Writes every Map and Alias token.</summary>
    public abstract void DeriveGlobal(in DerivationContext<TLanguageToken> context, DesignToken token);
}

public readonly record struct DesignVariant(
    DesignStyle Style, ThemeAppearance Appearance, DesignDensity Density, DesignIdiom Idiom);

public readonly ref struct DerivationContext<TLanguageToken>   // Variant, Language, Global
```

- 语言只有三个方法：给种子默认值、推导自己的词汇、推导全局词汇。每个方法必须写全它负责的全部 Token。
- 全局推导不拆成 Map 与 Alias 两个方法：显式值在推导之前已经冻结（6.3），同一个方法里先写 Map 再写 Alias，Alias
  读到的就是用户显式给出的 Map 值。Seed / Map / Alias 作为词汇的分层保留，用于说明 Token 的角色与主题定义里的阶段
  归类。
- 所有方法是输入的纯函数：相同输入给出相同输出，不读时钟、环境或可变静态状态。
- 复用通用的色彩数学时调用核心的中性工具（色板生成、混色、对比度、阴影组合），不得调用另一种语言。

### 5.2 四个轴

| 轴 | 含义 | 取值集合 | 谁决定当前取值 |
| --- | --- | --- | --- |
| 风格 | 语言内部的外观族 | 语言声明。Shadcn：Vega / Nova / Maia / Lyra / Mira / Luma / Sera / Rhea；PrimeNG：Aura / Lara / Nora；Ant Design：单一风格 | 主题或选择 |
| 明暗 | Light / Dark | 固定为两个，对应 Avalonia 的主题变体 | 主题、选择（可以跟随系统）或作用域 |
| 密度 | 尺寸与间距的档位 | 语言声明。核心预定义 `Default` 与 `Compact`，语言可以声明更多档 | 主题、选择或作用域 |
| 形态 | 界面运行的设备形态 | 语言声明。核心预定义 `Desktop` 与 `Mobile`，可以扩展 | 界面根（第 9 章） |

- 风格、密度、形态是开放的标识符集合，不是封闭枚举：新的语言、新的设备形态不需要改核心。
- 明暗怎么算、每个密度档怎么算，是语言自己的规则。Ant Design 的暗色与紧凑算法是 Ant 语言包的内部实现；官方没有
  定义的档位作为 AtomUI 扩展实现，并在 README 标明。
- 请求了语言未声明的取值时，在配置提交或主题应用时失败，不做静默替换。
- 语言独有的其他选择（Shadcn 的 Base Color、其他设计体系的对比度等级等）不新增轴，用语言种子表达（5.3）。

### 5.3 种子

种子是推导的输入，分两类：

- **全局种子**：全局 Token 的 Seed 层——主色、状态色、基础圆角、基础高度、基础字号、字体等。所有语言共用。
- **语言种子**：语言 Token 中标为种子的那部分，只属于该语言。例如 Shadcn 的 Base Color。现有核心里为 Shadcn 增设的
  `ColorNeutral` 种子属于此类，迁入 Shadcn 的语言种子，核心不再为某一种语言保留输入。

取值按以下顺序叠加：**语言默认种子 < 主题种子 < 作用域种子**。品牌色、中性基调等都通过种子表达，不需要为每个组合
准备主题文件。

语言另外声明两项关于种子的元数据，供主题定义与选择使用（7.4、7.8）：

- **可调种子**：哪些种子适合交给最终用户更换（Ant Design：`ColorPrimary`；Shadcn：`ColorPrimary` 与语言种子
  `BaseColor`）。
- **种子的派生集合**（可选）：每个种子派生出哪些 Token，用于提示主题里可能变陈旧的显式值。

### 5.4 语言 Token

全局词汇是 Ant Design 形状的，装不下其他语言的全部概念：Shadcn 的 `--input`、`--card`、`--ring`、`--accent`，PrimeNG 的
surface 色阶与 highlight，在全局 Token 里没有一一对应的位置。把它们硬塞进全局 Token，或者让各个控件推导各自反推，
都是补丁。

解法是承认每种语言有**自己的词汇**，并让它成为与全局 Token、控件 Token 同等地位的第三类 Token：

```csharp
// AtomUI.DesignLanguages.Shadcn
[LanguageDesignToken]
public sealed partial class ShadcnToken : LanguageDesignToken
{
    /// <summary>Base colour family the neutral roles are taken from.</summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color BaseColor { get; set; }

    /// <summary>Border of form fields (--input).</summary>
    public partial Color Input { get; set; }

    /// <summary>Focus ring colour (--ring).</summary>
    public partial Color Ring { get; set; }

    /// <summary>Card surface (--card) and the text on it.</summary>
    public partial Color Card { get; set; }
    public partial Color CardForeground { get; set; }

    /// <summary>Corner radius of form fields for the current style.</summary>
    public partial CornerRadius InputRadius { get; set; }
    // ...
}
```

| | 全局 Token | 语言 Token | 控件 Token |
| --- | --- | --- | --- |
| 声明在 | 核心 | 语言包 | 控件包 |
| 表达 | 跨语言通用的角色 | 该语言独有的角色、几何表、色阶 | 某个控件的取值与行为开关 |
| 谁给值 | 语言的 `DeriveGlobal`；对单个控件，由控件推导在其视图中覆盖 | 语言的 `DeriveLanguage` | 控件推导 |
| 谁能读 | 语言、全部控件推导、控件模板（经控件视图）、应用 | 该语言的全局推导与控件推导 | 该控件的模板与代码 |
| 用户可否显式覆盖 | 可以 | 可以 | 可以 |

- 语言 Token 与另外两类 Token 用同一套机制：`partial` 属性、生成的 Schema、冻结语义、主题定义里可覆盖
  （`<LanguageTokens>`，7.4）。用户把 Shadcn 的 `Input` 改掉，所有读它的控件一起变，不需要逐个控件覆盖。
- 语言 Token 类必须覆盖三类内容，形状由各语言自定，核心不规定共同接口：**角色色**（语义颜色及其前景）、**几何**
  （高度、各类圆角、内边距、字号字重、焦点环）、**色阶**（供取档用的色板）。
- 语言 Token 是该语言的公开 API：新增属性不算破坏性变更，删除或改变语义才算。色阶生成与派生的内部实现不公开，可以
  随官方基线升级重写。
- **读取规则**：一个值在全局 Token 里有位置时，控件推导从全局 Token 读；只有全局词汇里没有的概念才从语言 Token 读。
  这样用户对全局 Token 的覆盖对所有语言的控件一致生效。
- 控件模板不得引用语言 Token——控件包本来就看不见语言包，这一条由依赖规则保证。

### 5.5 组件配方

组件配方是语言 Token 上的只读计算视图：把官方设计体系里某个组件的成组取值收在一起，供不同控件包里的同类控件取用。

| 配方 | 内容 | 使用者 |
| --- | --- | --- |
| Table | 表头底与字、行底、行悬停、行选中底与字、斑马纹、边框、单元格内边距、容器圆角 | 内置 DataGrid、MachGrid |
| ListItem | 项的悬停、选中、聚焦各自的底与字 | 菜单、选择、树、各类列表（两端） |
| Overlay | 面板底、描边环、阴影档、圆角、内边距、遮罩 | 所有弹层（两端） |
| Field | 底、边框、悬停与聚焦边框、焦点环、占位符、禁用 | 输入族、单元格编辑器 |
| Chart | 系列色序列 | Charts |
| Touch（仅移动形态） | 触控目标尺寸、按压反馈、安全区留白 | 移动控件 |

- 配方只收官方有定义的组件，每个字段指到官方的一处取值。官方没有的组件不做配方。
- 配方由语言 Token 与全局 Token 算出，自身不是可覆盖的 Token：要改变它，覆盖它所依据的 Token。
- 配方保证同类控件同源取值：内置 DataGrid 与 MachGrid 读同一个 Table 配方，桌面菜单与移动列表读同一个 ListItem 配方。

### 5.6 派生语言

一个语言可以声明一个基语言，继承它的种子默认值、语言 Token、全局推导与全部控件推导，只重写不同的部分。用途是在
某种语言之上做品牌化（例如企业品牌语言派生自 Ant Design）。

- 派生语言的语言 Token 类必须派生自基语言的语言 Token 类；只有单继承。
- 三种第一方语言之间不得建立派生关系。

---

## 6. 控件推导与管线

### 6.1 控件推导

```csharp
// AtomUI.Desktop.Controls.Themes.Shadcn
[ControlDerivation]
internal sealed class ShadcnSelectDerivation : IControlDerivation<ShadcnToken, SelectToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<SelectToken> view)
    {
        var language = context.Language;
        view.Global.ColorBorder  = language.Input;   // Select 看到的边框色
        view.Global.ColorPrimary = language.Ring;    // Select 看到的焦点色
        view.Token.OptionSelectedColor   = language.AccentForeground;
        view.Token.OptionBackgroundInset = new Thickness(0, view.Global.LineWidth);
        view.Token.MutedSelectIndicator  = context.Variant.Style == ShadcnStyle.Luma;
    }
}
```

- **身份是类型。** 推导类通过泛型参数绑定到语言 Token 类型与控件 Token 类型，写错在编译期报错。风格、明暗、密度、
  形态都是 `context.Variant` 里的参数：一个 Token 类、一种语言，只有一个推导类。
- **推导的输出是该控件的视图。** `view.Token` 是控件 Token，**必须写全**，包括行为开关与从抽象层继承的属性，漏写由
  分析器在构建期报错。`view.Global` 是该控件看到的全局 Token：读时得到该控件的值，写时只对这个控件生效；只写与全局
  不同的项。
- **只能覆盖 Map 与 Alias。** 种子不能在控件推导里覆盖：改种子要重新派生，违反单遍。
- **读取顺序：** `view.Global.X` 若本方法已经写过就返回写入的值，否则返回共享的全局值；`view.Token.X` 只能读本方法
  已经写过的。另可读 `context.Language`、`context.Variant`。
- **显式值优先：** 主题与作用域对该控件写过的值（6.4）在推导之前冻结，推导对它们的写入被忽略（6.3）。
- 推导类位于取值包，通过引用控件包拿到 Token 类型；它只使用 Token 类及其属性的类型，不调用控件的行为代码。
- **共享逻辑**：同一语言内多个控件共用的推导放在取值包内的普通基类或辅助类中（例如按钮族共用一个泛型基类）。
  跨端共用的抽象 Token 层（位于 `AtomUI.Controls`）的取值，由 `AtomUI.Controls.Themes.<Lang>` 提供公开的共享推导
  方法，各端的取值包在自己的推导里调用。

**查找只有一条规则**：沿当前语言的语言 Token 类型的继承链，由最具体到最一般，取第一个为该控件 Token 登记的推导。

```text
BrandToken : AntDesignToken : LanguageDesignToken

IControlDerivation<BrandToken, TagToken>             派生语言自己的推导
IControlDerivation<AntDesignToken, TagToken>         基语言的推导
IControlDerivation<LanguageDesignToken, TagToken>    不绑定语言的通用推导
```

- 派生语言的继承、不绑定语言的通用推导，都是这一条规则的自然结果，不是特例。
- **通用推导**绑定到根类型 `LanguageDesignToken`，因此只能读全局 Token。它供不打算逐语言适配的第三方控件使用，在任何
  语言下都能给出取值。第一方不得用它代替语言专属推导。
- 整条链上都没有登记时，在配置提交时报错（第 8 章），不留到运行时。

### 6.2 单遍管线

```text
1  确定语言与变体        风格、明暗、密度来自主题、选择与作用域；形态来自界面根
2  种子                  DefineSeeds  <- 主题种子 <- 作用域种子
3  语言 Token            DeriveLanguage
4  全局 Token            DeriveGlobal
5  最终规范化            核心执行的与语言无关的规则（例如关闭动效时全部标准时长归零）
6  控件视图             各控件的推导：对全局 Token 的覆盖与控件 Token；按需惰性求值
7  发布快照              交给运行时发布层（6.5）
```

每一步只执行一次，没有重跑，没有收敛判定。

### 6.3 显式值的冻结语义

**显式值是输入，不是事后补丁。** 进入第 3、4、6 步之前，管线先把用户在对应词汇上显式给出的值写入 Token 对象并冻结
这些槽位：推导对冻结槽位的写入被忽略，读取得到显式值。

- 推导代码就是普通的顺序赋值，不需要任何「是否显式」的判断；
- 用户显式改了 A，后面由 A 算出的 B 自动跟随——不论 A 与 B 是同一类 Token 还是跨类（语言 Token → 全局 Token →
  控件 Token）；
- 优先级只有一条：**语言推导 < 主题显式值 < 更内层作用域的显式值**。

实现上，冻结是每个 Token 对象上按槽位的一组位；生成的 setter 在写入前检查对应的位。Token 对象只在求值期间存在，
渲染与资源查找读冻结后的值表，不经过这些属性。

### 6.4 作用域与控件级配置

- 作用域（`ThemeConfigProvider`）可以覆盖种子、三类 Token 中的任意值、明暗、密度、形态，也可以整体换语言。每个
  作用域是一个求值单元，按 6.2 独立求值。
- 针对某一类控件的配置（主题定义与 `ThemeConfig` 里的 Control）写入的就是该控件的视图，与控件推导写入的是同一份，
  优先级为「语言推导 < 主题定义 < 作用域与运行时配置」。它可以写控件 Token，可以写该控件看到的任意全局 Token，也可以
  为它固定明暗或密度（7.5）。
- 只写 Map、Alias 或控件 Token 时只替换那一项，不触发派生。覆盖了种子或固定了轴的控件自成一个求值单元，管线为它单独
  求一份全局 Token，再求它的视图。这两条规则取代现有的四种控件算法模式。
- 根上显式写的全局 Token 改变的是共享全局值；语言对某个控件视图的覆盖仍然生效，与 antd 组件级 Token 优先于全局
  Token 一致。要改这个控件，写它的 Control 配置；要让一类控件一起变，覆盖语言 Token（例如 Shadcn 的 `Input`），因为
  语言的覆盖值就是从它推导出来的。
- 推导是纯函数，可以在后台线程求值并被并发调用。

### 6.5 与运行时发布层的关系

本方案只替换「值怎么算出来」这一层。现有运行时的发布层原样保留，其契约继续有效：

| 保留的机制 | 作用 |
| --- | --- |
| 按槽位的值表与 `ThemeSnapshot` | 全局值只存一份；控件快照只存控件 Token 值与相对全局的差量 |
| `ThemeContext`、`ThemeScopeGraph`、`ThemeConfigProvider` | 嵌套作用域与稳定的上下文 |
| 事务与发布顺序 | 后台准备、UI 线程原子提交、请求合并、陈旧结果丢弃 |
| 惰性控件槽 | 控件 Token 首次查询时才求值，结果与急切求值逐位相同 |
| 两级缓存与容量策略 | 完整快照缓存、控件求值缓存 |
| 资源解析与强类型 Token 键 | `SharedTokenResource`、`XxxTokenResource` 的写法与零分配查询 |
| 主题目录、来源、用户目录刷新 | 见 7.9 |

替换掉的只有编译器的前半段（合并配置之后、冻结值表之前），以及它依赖的算法注册表。

---

## 7. 主题定义、运行时配置与选择

主题系统继续支持用 XML 定义主题，并且编译进包的主题、应用资源里的主题、用户目录里的主题、运行时构造的配置仍是
同一套语义。变化在于主题的含义：v1 的主题要自己声明算法链，一个文件只对应一种明暗与密度；新架构里**主题只描述
配置，不描述计算**——计算全部在语言里，主题只说「用哪种语言、固定哪些轴、覆盖哪些值」。

格式因此升级为 v2（namespace `https://atomui.net/schemas/theme/v2`）。Reader 只接受 v2，不为 v1 建立适配层；v1 到
v2 的换写规则见 7.11。规范性的参考文档与 XSD 在落地步骤 1 中随实现产出，本章是它们的设计依据。

### 7.1 设计结论

- 一个 XML 文件只定义一个主题。namespace 即格式版本。
- 主题必须声明所属语言。没有 `<Algorithms>`；风格、明暗、密度是属性，不是算法。
- **一个主题可以覆盖所有变体。** 明暗、密度、形态不同时取值不同的部分写在 `<When>` 条件段里，不再需要每个变体一份
  文件。
- 主题写在根属性上的轴是**固定**的；不写的轴是**开放**的，由选择或作用域决定。
- Token 只用 `Name` 与 `Value`；类型、阶段、赋值方式由 Schema 决定，XML 不重复声明。种子可以标记 `Locked`。
- Control 用 `(Catalog, Id)` 作稳定身份。Control 的算法四态取消，由两条规则取代（7.5）。
- 文档不支持 include、import、继承另一个文件、表达式、脚本或环境变量替换。组合靠分层（7.7），不靠文件间引用。
- 没有 `IsDefault`。应用的初始主题由应用在启动配置里指定，不由某个文件声明。
- XSD 负责结构、基础词法、容量与局部唯一性；Binder 负责与注册表相关的语义校验。两者的分工与 v1 相同。

### 7.2 文档结构与示例

```text
Theme                         exactly 1
+-- Tokens                    0..1      基础段：全局 Token
|   \-- Token                 1..4096
+-- LanguageTokens            0..1      基础段：所属语言的语言 Token
|   \-- Token                 1..4096
+-- Controls                  0..1      基础段：控件级配置
|   \-- Control               1..1024
|       \-- Tokens            0..1
|           \-- Token         1..4096
\-- When                      0..64     条件段，按文档顺序
    +-- Tokens                0..1
    +-- LanguageTokens        0..1
    \-- Controls              0..1
```

元素顺序是协议的一部分；容器一旦出现就不能为空。

```xml
<?xml version="1.0" encoding="utf-8"?>
<Theme xmlns="https://atomui.net/schemas/theme/v2"
       Id="AcmeBrand"
       Name="Acme Brand"
       Language="Shadcn"
       Style="Nova">
  <Tokens>
    <Token Name="ColorPrimary" Value="#E11D48" Locked="true" />
    <Token Name="ColorLink" Value="#E11D48" />
  </Tokens>

  <LanguageTokens>
    <Token Name="BaseColor" Value="#71717A" />
    <Token Name="Ring" Value="#FB7185" />
  </LanguageTokens>

  <Controls>
    <Control Catalog="AtomUI" Id="Button">
      <Tokens>
        <Token Name="DefaultShadow" Value="none" />
      </Tokens>
    </Control>
    <Control Catalog="AtomUI" Id="ToolTip" Appearance="Dark" />
    <Control Catalog="AtomUI.MachGrid" Id="DataGrid" Density="Compact">
      <Tokens>
        <Token Name="TableHeaderBg" Value="#FAFAFA" />
      </Tokens>
    </Control>
  </Controls>

  <When Appearance="Dark">
    <Tokens>
      <Token Name="ColorBgContainer" Value="#0B0B0F" />
    </Tokens>
    <Controls>
      <Control Catalog="AtomUI.MachGrid" Id="DataGrid">
        <Tokens>
          <Token Name="TableHeaderBg" Value="#18181B" />
        </Tokens>
      </Control>
    </Controls>
  </When>

  <When Idiom="Mobile">
    <Tokens>
      <Token Name="BorderRadius" Value="8" />
    </Tokens>
  </When>
</Theme>
```

这一个文件定义了：Shadcn 的 Nova 风格、玫红品牌色且不允许最终用户更换、基色族与焦点环色、明暗与密度开放；ToolTip 始终用暗色取值，
MachGrid 的表格始终紧凑；暗色下另有两处覆盖，移动端圆角加大。

### 7.3 Theme 根属性

| 属性 | 必须 | 类型 | 语义 |
| --- | --- | --- | --- |
| `Id` | 是 | `Identifier` | 主题目录内稳定且唯一的身份，不从文件名推断 |
| `Name` | 是 | 1..128 字符 | 面向用户显示的名称 |
| `Language` | 是 | `Identifier` | 语言身份（`AntDesign`、`Shadcn`、`PrimeNG`…），必须已启用 |
| `Style` | 否 | `Identifier` | 写了即固定为该风格；必须是该语言声明的风格。不写则开放，缺省为语言的默认风格 |
| `Appearance` | 否 | `Light` / `Dark` | 写了即固定；不写则开放 |
| `Density` | 否 | `Identifier` | 写了即固定；必须是该语言声明的密度档。不写则开放 |

- 主题不声明形态：形态由界面根决定（第 9 章）。主题可以用 `<When Idiom="…">` 为不同形态给不同的值。
- **固定只约束面向最终用户的选择**（7.8）：主题固定了明暗，应用的明暗开关对它不生效，界面据此禁用开关。开发者在
  代码里写的 `ThemeConfig` 与局部作用域不受固定约束。
- `Identifier` 词法沿用 v1：`[A-Za-z_][A-Za-z0-9_.-]{0,127}`，区分大小写，序数比较。
- v1 的 `IsDefault`、`DesignLanguage`、`BrandColor`、`NeutralColor` 取消：分组由 `Language` 与 `Style` 直接给出；品牌色
  与中性色是否可换由语言声明的可调种子与 `Locked` 决定（7.4）。

### 7.4 Tokens 与 Token

```xml
<Token Name="ColorPrimary" Value="#1677FF" />
```

| 属性 | 必须 | 语义 |
| --- | --- | --- |
| `Name` | 是 | 全局 Token 的稳定名称 |
| `Value` | 是 | 1..4096 字符的词法值，词法规则沿用 v1 第 8 节 |
| `Locked` | 否 | 仅用于基础段的种子：`true` 表示最终用户的选择不能替换它。默认 `false` |

- 同一个 `Tokens` 容器内 `Name` 不得重复；声明顺序不影响语义。
- Binder 按 Schema 判定 Token 的阶段，并据此把值放进管线的对应位置（7.7）：种子在语言推导之前生效，Map 与 Alias 的
  值作为冻结的显式值生效。未知 Token、不可写 Token、类型转换失败都是错误。
- `<LanguageTokens>` 覆盖主题所属语言的语言 Token（5.4），规则与 `<Tokens>` 完全相同：其中的种子在语言推导之前生效，
  其余作为冻结的显式值生效。名称必须属于该语言的语言 Token 类。
- **可调种子**由语言声明（Ant Design：`ColorPrimary`；Shadcn：`ColorPrimary` 与语言种子 `BaseColor`）。界面据此决定提供哪些
  色板；主题对某个可调种子写了 `Locked="true"`，对应色板不提供。`Locked` 写在非种子或条件段里是错误。
- **陈旧值提示**：语言可以声明每个种子派生出哪些 Token。主题没有锁定某个种子、却显式写了它的派生 Token 时，Binder
  给出警告——用户换了种子之后那个值不会跟着变。v1 写死在 Reader 里的品牌派生 Token 名单（`ATMTHM3101`、
  `ATMTHM3106`）由此取代，名单的出处回到语言。

### 7.5 Controls 与 Control

```xml
<Control Catalog="AtomUI" Id="Button">
  <Tokens>
    <Token Name="ColorPrimary" Value="#0958D9" />
    <Token Name="ContentFontSize" Value="14" />
  </Tokens>
</Control>
```

| 属性 | 必须 | 语义 |
| --- | --- | --- |
| `Catalog` | 是 | 注册该控件的命名域。主仓为 `AtomUI`，其余各用自己稳定的 Catalog |
| `Id` | 是 | 该命名域内的控件 |
| `Appearance` | 否 | 该控件固定使用的明暗；只能写在基础段 |
| `Density` | 否 | 该控件固定使用的密度；只能写在基础段 |

- 同一个 `Controls` 容器内 `(Catalog, Id)` 不得重复。Control 必须至少有一个轴属性或一个 `Tokens`。
- Control 的 `Tokens` 用一个集合表达两类覆盖，与 v1 相同：该控件的控件 Token，以及该控件所见的任意全局 Token。
  Binder 先按该控件的控件 Token 匹配，未命中再按全局 Token 分类；两处都没有是错误。控件 Token 与全局 Token 禁止同名。
- 控件级的全局覆盖只影响该控件，不改变其他控件，也不改变作用域的全局快照。
- 语言的控件推导写入的覆盖与这里是同一份控件视图，优先级最低（6.4）。

**算法四态取消。** v1 用 `Unspecified / Disabled / Global / Custom` 回答两个问题，v2 各用一条规则直接回答：

| v1 要回答的问题 | v2 的规则 |
| --- | --- |
| 在控件上覆盖一个种子，要不要为它重新派生？（`Disabled` / `Global`） | 永远按阶段处理：覆盖种子就为该控件重新求一份全局视图，覆盖 Map 或 Alias 就只替换那一个值。种子是输入，输入变了派生值跟着变，与全局层的语义一致 |
| 控件要不要用一条不同的算法链？（`Custom`，例如表格固定紧凑） | 在 Control 上固定轴：`Density="Compact"`、`Appearance="Dark"` |

只想替换某一个值而不触发派生时，覆盖对应的 Map、Alias 或控件 Token，而不是种子。

### 7.6 When 条件段

```xml
<When Appearance="Dark" Density="Compact">…</When>
```

| 属性 | 取值 |
| --- | --- |
| `Style` | 该语言声明的某个风格 |
| `Appearance` | `Light` / `Dark` |
| `Density` | 该语言声明的某个密度档 |
| `Idiom` | 该语言声明的某个形态 |

- 至少写一个条件属性，至少有一个子容器。没写的属性不参与匹配。条件之间是「且」。
- 条件对**求值单元的有效变体**求值。求值单元是一个作用域，或一个固定了轴的控件。因此同一份主题定义在亮色根、其中的
  暗色局部作用域、固定为暗色的 ToolTip 上分别匹配，各得其值。
- 一次求值中，先应用基础段，再按文档顺序应用所有匹配的条件段；后者覆盖前者。合并规则是：全局 Token 按名称覆盖，
  Control 先按身份、再按 Token 名称覆盖。
- 条件段里不能固定轴（不能写 Control 的 `Appearance` / `Density`，也不能写 `Locked`）：轴先确定，条件后求值，两者不
  互相依赖。
- 条件写了被主题自身固定的轴的另一个取值时（主题固定 `Appearance="Light"` 却有 `<When Appearance="Dark">`），
  Binder 给出警告：该段只会在作用域或控件显式改用暗色时生效。

### 7.7 运行时配置与分层

`ThemeConfig` 是 C# 与 AXAML 里的同构配置，用于应用初始配置、运行时请求和 `ThemeConfigProvider` 局部作用域：

```text
ThemeConfig                       不可变
+-- Inherit = true
+-- Language?  Style?  Appearance?  Density?  Idiom?      未指定即继承
+-- Tokens                                                全局 Token 覆盖
+-- LanguageTokens                                        当前语言的语言 Token 覆盖
\-- Controls[ControlTokenIdentity]
    +-- Appearance?  Density?
    \-- Tokens                                            控件 Token 与该控件所见的全局 Token
```

- 没有 `Algorithms`。明暗与密度是两个独立字段，各自继承：父作用域是暗色、子作用域只写 `Density = Compact`，结果是
  暗色紧凑。v1 里「算法链整体替换导致父 Dark + 子 Compact 变成亮色紧凑」的行为不复存在。
- `ThemeConfig` 没有条件段：它由代码构造，需要按变体区分时由调用方给出不同的配置。
- 配置在构造后不可修改；替换 `ThemeConfigProvider.Config` 才产生新输入。这一点与现状相同。

**轴的确定**（先于取值）：

```text
根：     语言默认 <- 选择（被主题固定的轴取主题的值） <- 应用初始配置 <- 运行时请求
作用域： 父作用域的有效轴 <- 本作用域 Config 中写明的轴
控件：   所在作用域的有效轴 <- 主题定义与配置中该 Control 固定的轴
形态：   界面根给出；作用域可以写明覆盖
```

**取值的分层**（低到高，对每个求值单元各做一次）：

```text
语言推导（按该求值单元的有效变体）
<- 所选主题定义：基础段，然后各匹配的条件段
<- 应用初始配置
<- 运行时请求（含选择带来的种子）
<- 父作用域的有效配置
<- 局部 ThemeConfigProvider.Config
```

每一层的种子进入管线第 2 步；语言 Token、全局 Token、控件 Token 的显式值分别在第 3、4、6 步之前冻结（6.2、6.3）。

`Inherit = false` 的含义沿用现状并按新模型表述：

- 根请求 `Inherit = false`：保留「语言 + 当前所选主题定义」，丢弃应用初始配置。
- 局部作用域 `Inherit = false`：保留父作用域的语言与形态，丢弃所选主题定义与父作用域的全部显式值，风格、明暗、密度
  回到语言默认，再应用本作用域的配置。

### 7.8 选择

选择是面向最终用户的那一层：选哪种语言、哪个具名主题、明暗、密度、品牌色。

```csharp
await themeManager.ApplyAsync(ThemeSelection
    .Of<ShadcnLanguage>(ShadcnStyles.Nova)
    .WithTheme("AcmeBrand")                    // optional named theme
    .WithAppearance(ThemeAppearance.Dark)
    .WithDensity(ThemeDensity.Compact)
    .WithLanguageSeed<ShadcnToken>(seed => seed.BaseColor = Color.Parse("#78716C")));
```

- 具名主题是可选的。不选主题时，结果就是该语言在所选变体下的默认外观；语言本身不需要带任何主题文件。
- 明暗可以选「跟随系统」：由主题管理器按平台当前的明暗解析成具体取值，平台变化时重新应用。
- 主题管理器对外提供三样只读数据，Gallery 的菜单直接由它们生成：
  - 已启用的语言，及各自的风格、明暗、密度、形态与可调种子；
  - 目录中的具名主题，及各自的语言、固定的轴、锁定的种子、声明的主色、来源；
  - 当前选择与每个作用域的有效变体。
- 选择的解析是纯函数，规则只有三条，没有回退搜索：
  1. 语言必须已启用；所选主题必须属于该语言；所选风格、明暗、密度必须在语言声明的范围内。否则报错。
  2. 主题固定的轴取主题的值，选择里对应的值被忽略，结果里标明。
  3. 主题锁定的种子不被选择替换，结果里标明。
- v1 的 `ThemeSelectionResolver` 需要在一组静态 profile 里找最接近的定义、再增删算法拼出链；v2 里变体是参数，这套
  回退规则连同 `ATMTHM3103`–`ATMTHM3108` 一并取消。

### 7.9 来源与目录

来源的种类、注册方式与事务规则沿用现状：

| 来源 | 注册 | 可刷新 | 失败处理 |
| --- | --- | --- | --- |
| 编译进包的主题（`AtomUIThemeDefinition` 构建项） | 生成的包级注册 | 否 | 属于损坏的发布产物，阻止启动 |
| 应用或包内的 `avares://` XML | 显式列出的 Resolver | 否 | 同上 |
| 用户主题目录（`*.theme.xml`） | `UseUserThemeDirectory()` | 手动刷新 | 首次加载失败不阻止启动；整个目录是一个刷新单元，不提交有效子集 |
| 程序化来源 | 自定义 `IThemeDefinitionResolver` | 由来源声明 | 按来源类型 |

- 语言包、取值包、产品包、应用都可以带具名主题。语言包只带官方有名字的那些。
- 任意来源之间重复的 `Theme.Id` 是错误，没有先到先得或后者覆盖。来源顺序只决定展示顺序。
- 主题引用的语言、风格、Token、Control 必须在当前冻结的注册表中存在。静态来源违反即构建或启动失败；用户目录违反
  即本次目录解析失败，并通过主题目录诊断暴露。
- 用户主题不能加载代码、程序集、脚本、网络资源。用户目录的枚举、容量、路径安全规则不变。
- 应用的初始主题由 `WithInitialTheme(ThemeSelection)` 指定；只启用了一种语言且未指定时，取该语言的默认变体。
  「恰好一个默认主题」这条跨文件约束随 `IsDefault` 一起取消。
- 当前所选的具名主题因刷新而消失时，回到「同一语言、同样的变体、不带主题」，而不是换成另一个主题。

### 7.10 编译、校验与内容身份

**编译主题包。** 包内的主题 XML 在构建期用与运行时相同的 Reader 与 XSD 校验，并对照所引用程序集携带的构建契约
（语言的身份、风格与可调种子；各控件包的 Token Schema）完成绑定，生成类型化的主题程序。运行时不解析 XML。条件段
编译为对变体的分支；种子、Map、Alias、控件 Token 四类写入保留各自的阶段，不压平成一张最终值表。生成 ABI 升为 v2。

**校验管线**与 v1 相同的五层：XML 良构 → XSD → 注册表绑定 → 值转换 → 语义校验；任一层失败都不产生可发布的部分
定义。每条诊断包含稳定代码、级别、来源身份、行列与元素路径。代码分段沿用：`ATMTHM1xxx` 结构、`2xxx` 绑定、
`3xxx` 值与语义、`4xxx` 目录与跨来源冲突。v2 新增的语义校验：

| 检查 | 级别 |
| --- | --- |
| `Language` 未启用；`Style` 不是该语言声明的风格 | 错误 |
| `Locked` 用在非种子、非可调种子或条件段里 | 错误 |
| `<LanguageTokens>` 里的名称不属于该语言的语言 Token 类 | 错误 |
| Control 的轴属性出现在条件段里；Control 既无轴属性也无 `Tokens` | 错误 |
| 条件段没有条件属性，或条件的取值不在语言声明的范围内 | 错误 |
| 未锁定某个种子却显式写了它的派生 Token | 警告 |
| 条件与主题自身固定的轴矛盾 | 警告 |
| 两个条件段的条件完全相同 | 警告 |

具体代码在实现时分配，v1 的 `ATMTHM3101`–`ATMTHM3108` 废止不复用。

**安全与容量**沿用 v1：禁止 DTD 与外部实体、不解析网络资源、只用内置的预编译 XSD、流式读取、单文件 4 MiB、元素
65536、Token 与 Control 的上限不变；新增条件段上限 64，取消算法链上限。

**内容身份。** 指纹基于规范化后的类型化结果：namespace 版本、主题元数据、语言身份与语言修订、按槽位排序的全局
Token 与语言 Token、按身份排序的 Control 配置、按文档顺序的条件段、注册表修订。空白、属性顺序、注释、Token 与 Control 的声明顺序
不影响指纹；条件段的顺序影响。

**修订身份不再手工维护。** v1 要求算法与控件阶段各自声明 `revision`，行为变了必须手工加一。v2 中语言程序集与取值包
程序集的修订取自确定性构建的程序集标识，推导代码一变它就变，缓存据此失效。

**编辑体验。** XSD 提供结构与属性的补全。Token 名称、Control 身份、风格名取决于工程引用了哪些包，不在通用 XSD 里；
工具链可以按工程生成一份带这些枚举的增强 XSD 供编辑器使用，它只用于补全，不参与校验。

### 7.11 v1 到 v2 的换写

Reader 不兼容 v1。现有主题文件按下表一次性换写，工具链提供转换命令：

| v1 | v2 |
| --- | --- |
| `<Algorithms>` 只含内置算法 | `Language="AntDesign"`；链里有 `Dark` → `Appearance="Dark"`；有 `Compact` → `Density="Compact"` |
| `<Algorithm Catalog="Shadcn" Id="Nova.Dark" />` | `Language="Shadcn" Style="Nova" Appearance="Dark"` |
| 同一语言的常规、暗色、紧凑、暗色紧凑四个文件 | 合并为一个文件，不同之处写进 `<When>`；四个文件内容相同时只剩根元素与种子 |
| `Appearance`（对算法链结果的断言） | 仅在要固定明暗时保留，否则删除 |
| `DesignLanguage="PrimeNG.Aura"` | `Language="PrimeNG" Style="Aura"` |
| `BrandColor="Fixed"` / `NeutralColor="Fixed"` | 对应种子加 `Locked="true"`；`Seed` 是缺省行为，删除属性 |
| `IsDefault="true"` | 删除；应用用 `WithInitialTheme` 指定 |
| Control `Algorithm="Global"` | 删除属性（已是固定行为） |
| Control `Algorithm="Disabled"` 且覆盖了种子 | 行为有变：v2 会重新派生。要保持原值，改为覆盖具体的 Map、Alias 或控件 Token。转换命令对这类条目给出提示而不自动改写 |
| Control 内的 `<Algorithms>` | 换成 Control 的 `Appearance` / `Density` 属性 |
| `ThemeConfig.Algorithms` | `ThemeConfig.Appearance` / `Density` / `Style` |

---

## 8. 注册、启动与校验

> 本节保留原方案。后续的取值包自动匹配、语言默认主题、初始语言选择和按需求值边界见
> [应用注册与启动 API](../../architecture/systems/theming/design-languages.md#应用注册与启动-api)。

桌面应用：

```csharp
builder.UseDesktopControls();
builder.UseDesktopDataGrid();
builder.UseMachGrid();

builder.UseAntDesign(language => language.AddDesktopControls().AddDesktopDataGrid().AddMachGrid());
builder.UseShadcn(language => language.AddDesktopControls().AddDesktopDataGrid().AddMachGrid());

builder.WithInitialTheme(ThemeSelection.Of<AntDesignLanguage>());
```

移动应用：

```csharp
builder.UseMobileControls();
builder.UseAntDesign(language => language.AddMobileControls());

builder.WithInitialTheme(ThemeSelection.Of<AntDesignLanguage>());
```

- `Use<Lang>()` 由语言包提供。`Add<控件包>()` 由对应的取值包提供，是语言构建器上的扩展方法；它同时登记所依赖控件包的
  取值（`AddDesktopControls()` 会带上 `AtomUI.Controls` 的取值）。
- 安装包不等于启用，必须调用入口。应用至少启用一种语言。
- 注册表由生成器产出，不依赖反射，满足裁剪与 AOT；未启用的语言与未用到的控件推导可被裁剪。

**配置提交时的校验**（任一不满足即启动失败，并指出缺少哪个包）：

1. 每个已启用的语言 × 每个已启用的控件包，其全部控件 Token 类都能按 6.1 的规则找到推导。
2. 每个已启用的控件包所要求的形态，都在每个已启用语言声明的形态之内。
3. 初始主题的语言已启用，其风格、明暗、密度在该语言声明的范围内。
4. 每个取值包记录的 Token Schema 摘要与实际加载的控件包一致（3.3）。

运行时追加包（插件）时，在追加提交处执行同样的四项校验；失败则整个追加不生效。

没有运行时才暴露的「某控件在某语言下没有取值」。

---

## 9. 形态：桌面与移动

### 9.1 为什么是一个轴

同一种设计语言在两端的取值本来就不同：Ant Design Mobile 的主字号、圆角阶梯、成功 / 警告 / 危险色都与桌面版
Ant Design 不一样。这是语言自己的规则，应当由语言来算，而不是让移动控件各自绕开全局 Token 写死。

```csharp
public readonly record struct DesignIdiom(string Id)
{
    public static DesignIdiom Desktop { get; } = new("Desktop");
    public static DesignIdiom Mobile { get; } = new("Mobile");
}
```

- **形态由界面根决定。** 桌面窗口的根是 `Desktop`；Mobile Core 管理的界面根是 `Mobile`。应用切换主题、明暗、密度时
  形态不变。作用域可以显式覆盖形态（例如在桌面应用里预览移动界面）。
- 语言在 `DefineSeeds`、`DeriveLanguage`、`DeriveGlobal` 中按形态给值。同一份种子在两种形态下得到两套语言 Token 与
  全局 Token。
- 形态是开放的标识符：以后出现新的设备形态时，定义新的标识、由相应的界面根给出、语言声明支持即可，不改核心。
- 语言 Token 里的移动专属部分（`Touch` 配方、移动端的组件配方）只在 `Mobile` 形态下有值。

### 9.2 两端各自的包

| | 桌面 | 移动 |
| --- | --- | --- |
| 平台基础 | — | `AtomUI.Mobile.Core`（与设计语言无关） |
| 控件包 | `AtomUI.Desktop.Controls` 及扩展 | `AtomUI.Mobile.Controls` |
| 取值包 | `AtomUI.Desktop.Controls.Themes.<Lang>` | `AtomUI.Mobile.Controls.Themes.<Lang>` |
| 共用 | `AtomUI.Core`、`AtomUI.DesignLanguages.<Lang>`、`AtomUI.Controls` 及 `AtomUI.Controls.Themes.<Lang>` | 同左 |

- 桌面控件与移动控件是不同的控件，各有各的 Token 类型。即使同名（两端都有 `ButtonToken`）也互不相干，不要求对齐。
- 只有真正跨端共用的控件（`AtomUI.Controls`）才共用 Token 类型；它们的推导可以读 `context.Variant.Idiom` 分别给值。
- 被多个移动控件共用的移动端取值（触控目标尺寸、按压遮罩透明度、禁用透明度、安全区留白）放在语言 Token 里，移动控件
  的推导从那里读，不得在各个推导里各写一份。
- 同时面向两端的商业产品，按端各出控件包与取值包。

### 9.3 各语言的形态支持

| 语言 | Desktop | Mobile |
| --- | --- | --- |
| Ant Design | 官方 Ant Design | 官方 Ant Design Mobile |
| Shadcn | 官方 registry | 官方无移动规范；需要时作为 AtomUI 扩展提供并在 README 标明 |
| PrimeNG | 官方预设 | 同上 |

语言未声明 `Mobile` 形态时，不能与移动控件一起启用（8 的校验 2）。

---

## 10. 三种第一方语言

| | Ant Design | Shadcn | PrimeNG |
| --- | --- | --- | --- |
| 包 | `AtomUI.DesignLanguages.AntDesign` | `AtomUI.DesignLanguages.Shadcn` | `AtomUI.DesignLanguages.PrimeNG` |
| 风格 | 单一 | Vega、Nova（默认）、Maia、Lyra、Mira、Luma、Sera、Rhea | Aura（默认）、Lara、Nora |
| 明暗 | 官方 | 官方 | 官方 |
| 紧凑 | 官方 | AtomUI 扩展 | AtomUI 扩展 |
| 形态 | Desktop、Mobile | Desktop | Desktop |
| 主要种子 | `ColorPrimary` 及功能色、圆角、高度、字号 | `ColorPrimary`（Theme）、语言种子 `BaseColor`（Base Color） | `ColorPrimary`（默认 noir）、功能色 |
| 语言 Token 的核心 | 预设色板与派生规则、尺寸与圆角阶梯 | 风格几何表、surface 角色、Tailwind 色板 | 预设规则表、primary / surface 色阶、highlight、focus ring |
| 现有代码中的来源 | 核心计算器与各控件的 `CalculateTokenValues` | `ShadcnStyle`、`ShadcnSurface`、`ShadcnThemeColor`、`ShadcnTailwindPalette` | `PrimeNGPreset`、`PrimeNGPalette`、`PrimeNGNoir` |
| 具名主题 | DaybreakBlue | 无（按轴选择） | 无（按轴选择） |

每个语言包的 README 必须写明：对标的官方版本、全局 Token 映射表、各轴中哪些是 AtomUI 扩展、不支持清单。

---

## 11. 商业产品与第三方

### 11.1 商业产品

- 每个产品必须提供：控件包（含 Token 声明）与 Ant Design 取值包。Ant Design 下完整正确是产品的发布门槛。
- 其他语言的取值包按产品规划提供。产品文档如实列出支持的语言；未提供取值包的语言不能与该产品一起启用。
- 产品不得自建与本方案并行的主题机制（派生 Token 子类替换工厂、自有主题包描述符等）。品牌化用具名主题或派生语言。
- 取值包的 README 为每个控件声明目标等级：
  - **A 对标**：官方有对应组件或语言包提供了配方，按视觉验收口径对官方实测；
  - **B 协调**：官方无对应组件，用语言 Token组合取值，关键状态人工评审。

### 11.2 第三方

第三方与第一方用完全相同的公开 API，可以按投入选择三种形态：

| 形态 | 做法 | 结果 |
| --- | --- | --- |
| 完整 | 控件包 + 各语言取值包 | 与第一方同等；他人可以引用它的控件包为它追加语言 |
| 单程序集 | 控件、Token 与所支持语言的推导放在同一个程序集，引用这些语言包 | 最省事；代价是控件程序集绑定了它支持的那几种语言 |
| 通用 | 只提供绑定到根类型的通用推导（6.1），不引用任何语言包 | 任何语言下都能用，外观协调但不对标 |

第三方也可以只做语言：一个语言包，加上为现有控件包写的取值包。

---

## 12. 性能设计

### 12.1 基线与要求

现有主题系统有明确的性能模型与实测基线（`docs/architecture/systems/theming/runtime.md` 第 7.2、18 节）：

| 现有基线（默认 Ant Design 主题） | 数值 |
| --- | --- |
| 一次冷编译 | 约 0.74 毫秒，分配约 1.14 MB |
| 一次冷编译的 Token 属性写入 | 约 31,600 次，其中 94% 是为每个控件复制一份全局 Token（117 份 × 255 个属性） |
| PrimeNG Aura 冷编译（带 Alias 与控件阶段） | 约 0.88 毫秒，分配约 1.25 MB |
| 资源查询稳态 | 0 字节 / 次 |

要求：新架构在每一项现有性能验收上**不得退化**，并在下面列出的几项上应当变好。下文所有「应当变好」都是由结构推出的
预期，尚未实测，以步骤 1、2 的测量为准。

### 12.2 由结构消除的开销

| 现有开销 | 来源 | 新架构 |
| --- | --- | --- |
| 每个控件一份完整的全局 Token 副本 | 控件的 `CalculateTokenValues` 读自己的 `EffectiveGlobalToken`；`EvaluateGlobal` 可以改写它 | 控件推导写的是该控件的视图：没有覆盖的控件与全局共用同一份值表，有覆盖的控件只存相对全局的差量。只有覆盖了种子或固定了轴的控件才单独求一份全局 Token。占冷编译 94% 的那批写入不再发生 |
| Alias 与控件阶段的多轮重跑 | 语言算法在 Ant 公式之后抢写，靠重跑收敛 | 单遍，每个推导只执行一次 |
| 写入记账（记录写过的属性名，再解析成槽位） | 需要知道算法写了哪些字段 | 取消。setter 只检查冻结位 |
| 先跑 Ant 计算器再覆盖 | 非 Ant 语言叠在 Ant 之上 | 每种语言只跑自己的推导 |
| 逐身份的算法与控件阶段查表（以值元组为键的字典） | 算法链 × 控件 | 每种语言一张按控件槽位索引的数组 |
| 每个风格四份编译主题程序 | 变体靠文件区分 | 变体是参数，一个主题一份程序 |

### 12.3 保持与新增的机制

**保持不变**（6.5）：按槽位的值表、惰性控件槽、结构共享、两级缓存、事务与请求合并、零分配的资源查询。

**读集与更细的结构共享。** 现有规则是「全局 Token 一变，所有带控件 Token 的控件重算」。新架构里推导是纯函数，输入
可以精确记录：

- 求值期间，Token 的 getter 把被读的槽位记进当前推导的读集（一组位）。读集随求值结果一起缓存。
- 一个求值单元相对父单元只改了若干全局 Token 时，读集与变更集不相交的控件直接复用父单元的结果。
- 语言 Token 同样记录读集。只改 Map、Alias 或语言 Token 中的派生值时，语言 Token 对象与其余控件结果都可以共享。
- 改了种子时一切从头推导，这是正确的：种子变了，任何值都可能变。

读集记录只发生在求值期间；Token 对象不进入渲染与资源查询路径，所以热路径不受影响。

**注册与调用。**

- 语言与控件推导的注册表由生成器产出：每种语言一张按控件槽位索引的数组，元素是静态无捕获委托。不使用反射，
  不在启动路径上实例化以元组为键的泛型字典。
- 没有启用的语言不触碰其注册表，也不加载其取值包程序集。没有用到的控件不求值，其推导方法不会被即时编译。
- 推导不分配：上下文是 `ref struct`，语言 Token 与全局 Token 对象按（语言、变体、种子、显式值）缓存并在单元之间共享。
- 主题程序是类型化的静态赋值，条件段编译为对变体的分支，运行时不解析 XML。

### 12.4 新增的成本

| 成本 | 说明 | 对策 |
| --- | --- | --- |
| 程序集数量增加 | 只用 Ant Design 的桌面应用多出语言包与各控件包的取值包若干程序集；即时编译模式下每个程序集有加载开销 | NativeAOT 与裁剪发布不受影响。即时编译模式下必须对照现有启动基线测量；超出门槛时，把加载推迟到首次求值，或提供合并发布 |
| 取值代码体积 | 每种语言各有一套完整的控件推导 | 未启用的语言可以被裁剪；启用的语言里未用到的控件推导可以被裁剪 |
| 覆盖了种子或固定了轴的控件 | 各自多一次全局推导 | 按（语言、变体、种子、显式值）缓存，相同配置的控件共享；这类控件是少数 |
| 同时启用多种语言 | 每种语言各有一套语言 Token 与全局 Token | 只有实际出现在某个作用域里的语言才求值 |
| getter 的读集判断 | 求值期间每次读多一次空引用判断 | 与现有 setter 的记账判断同量级；不在热路径上 |

### 12.5 性能验收

沿用现有的验收场景与指标（冷启动、热启动、全局切换、局部变更、高频变更、拓扑竞争、跟随系统、资源查询、独立顶层
窗口、生命周期），并增加：

- 语言切换：Ant Design ↔ Shadcn ↔ PrimeNG，含首次切换（冷）与再次切换（缓存命中）。
- 形态：同一语言在桌面根与移动根下各一次冷求值。
- 控件固定轴与控件覆盖种子：验证只多出该控件自己的全局视图，其余控件结果的引用不变。
- 局部作用域只覆盖一个 Alias Token：验证只有读集命中的控件被重算。
- 启动：分别测量只启用 Ant Design、启用三种语言但只使用一种，两者的差值不超过现有启动门槛。

每一步迁移（第 16 章）以改造前的同一棵树为基线，按现有门槛比较。

---

## 13. 扩展点

### 13.1 可以扩展什么

| 想做的事 | 做法 | 需要改动核心或既有包吗 |
| --- | --- | --- |
| 新增一种设计语言 | 一个语言包；为想支持的控件包各写一个取值包 | 不需要 |
| 在某种语言上做品牌化 | 派生语言，或只用具名主题 | 不需要 |
| 为别人的控件包追加一种语言 | 引用它的控件包与语言包，写一个取值包 | 不需要 |
| 新增一个控件包 | 控件包 + 各语言取值包；或只提供通用推导 | 不需要 |
| 给语言增加风格、密度档 | 在语言的轴声明里增加 | 只改该语言包及其取值包 |
| 新增一种设备形态 | 定义一个新的形态标识；语言声明支持；该形态的界面根给出它 | 不需要改核心 |
| 语言需要自己的输入（对比度等级、基色族等） | 语言种子 | 不需要 |
| 语言需要全局词汇里没有的概念 | 语言 Token | 不需要 |
| 新增一种 Token 取值类型 | 由生成器写入词汇描述的解析器与格式化器 | 不需要 |
| 新增主题来源 | 自定义 `IThemeDefinitionResolver` | 不需要 |
| 运行时加载带控件的插件 | 插件带自己的控件包与取值包，经现有的包追加入口加载；加载提交时执行与启动相同的校验 | 不需要 |

### 13.2 用其他设计体系检验

下表不是承诺要做这些语言，而是用它们检验架构装不装得下。

| 设计体系的特征 | 在本架构里的落点 |
| --- | --- |
| 由一个源色生成整套色调板（Material 3） | 语言种子 + `DeriveLanguage` 里的色板生成 |
| 对比度等级（Material 3 的标准 / 中 / 高） | 语言种子；需要给最终用户选择时声明为可调种子 |
| 多档密度（Material 的 0 到 -3） | 语言声明更多密度档 |
| 状态层：悬停与按下是叠在底色上的半透明层 | 控件推导把叠加结果合成为各状态的控件 Token；确需真实叠加层时由控件增加行为开关 |
| 双环焦点指示（Fluent 2） | 控件的行为开关与对应的控件 Token |
| 中性色阶与强调色阶各自独立（Fluent 2、Carbon） | 语言 Token 中的色阶 |
| 每种语言有自己的图标集与动效曲线 | 图标与动效都是 Token（4.4） |
| 平台各有规范（HIG、Material 的移动与大屏） | 形态轴 |

### 13.3 有意保留的边界

- **模板只有一套。** 一种语言需要的结构在现有模板里不存在时，只能由控件包增加能力并以行为开关控制，语言不能自带
  模板。代价是模板的复杂度会随语言增多而上升；换来的是控件行为、可访问性、缺陷修复只有一份。应用层仍可以用
  Avalonia 的方式整体替换某个控件的 ControlTheme，这不属于设计语言机制。
- **全局词汇是 Ant Design 形状的。** 其他语言的概念靠语言 Token 承载，不改全局词汇。
- **取值代码随语言数增长。** 每种语言为每个控件包写全取值，不提供跨语言的隐式回退。

---

## 14. 工具链

| 工具 | 职责 |
| --- | --- |
| Token 生成器 | 为 Token 声明生成属性实现（含冻结语义）、资源键、Schema、Token 标记扩展 |
| 推导完整性分析器 | 控件推导、`DeriveLanguage`、`DeriveGlobal` 未写全应写的 Token 时报错；读取了尚未写入的本 Token 属性时报错 |
| 覆盖校验 | 取值包在构建期校验：对应控件包里的每个 Token 类有且只有一个该语言的推导 |
| 依赖方向校验 | 构建期检查 3.3 的引用规则 |
| 消费分析 | 控件包中写死的颜色、尺寸、时长等报错 |
| 注册生成 | 生成语言与控件推导的注册表；不依赖反射 |
| 契约测试库 | 纯函数性、三类 Token 的完整性、成对角色可读性、各变体（含两种形态）下的确定性 |
| 取值溯源 | 给定一个作用域与一个 Token，说明它的值来自哪一层（语言推导、主题的哪一段、哪个作用域的显式值）。按需重新求值得出，不在快照里常驻额外数据 |
| 设计时与测试支持 | 控件库工程的设计器预览与无头测试需要一种已启用的语言；工具链提供一行启用 Ant Design 及其取值包的测试入口 |

控件包新增或修改控件 Token 后，它的每个取值包会在构建期失败，直到补上取值。

---

## 15. 质量门槛

| 对象 | 门槛 |
| --- | --- |
| 语言包 | 只用公开 API；通过契约测试，覆盖全部风格 × 明暗 × 密度 × 声明的形态；README 完整 |

| 控件包 | Token 类只有声明并满足 4.2；无取值规则、无写死取值、无语言分支；不引用语言一侧的任何包 |
| 取值包 | 只使用控件包的 Token 类型；覆盖校验与完整性分析通过；取值只来自语言 Token、全局 Token 与核心工具；按声明等级完成视觉验收 |
| 对等性 | 主仓保留只引用公开 API 的样例语言包、样例控件包、样例取值包，随常规构建 |

视觉验收沿用现行口径：以 `atom:Window` 为宿主，对全部 showcase 与全部控件做脚本验收，关键区域对比加容差。

---

## 16. 迁移

### 16.1 被移除的机制

| 现有机制 | 去向 |
| --- | --- |
| 各 Token 类的 `CalculateTokenValues`（主仓约 150 个类，Mobile 65 个） | 从 Token 类中移出，成为各控件包的 Ant Design 取值包里的控件推导；Token 类留在原处，只剩声明 |
| `DesignToken.CalculateAliasTokenValues`、Default / Dark / Compact 计算器、`DaybreakBlue` 主题 | 成为 `AtomUI.DesignLanguages.AntDesign`；与语言无关的色彩数学留在核心作为中性工具 |
| 算法链、`IThemeAlgorithm`、`IThemeAliasAlgorithm`、`AtomUI:Default` 等身份、主题 XML 的 `<Algorithms>` | 删除，由语言契约与变体取代 |
| 主题定义 XML v1 及其 XSD、编译主题 ABI v1 | 由 v2 取代（第 7 章），不保留适配层，提供一次性换写命令 |
| 主题 XML 的 `IsDefault`、`DesignLanguage`、`BrandColor`、`NeutralColor` 元数据及 `ATMTHM3101`–`ATMTHM3108` | 删除；分组由 `Language` / `Style` 给出，可换性由可调种子与 `Locked` 给出 |
| `ThemeSelectionResolver` 在静态 profile 间的回退搜索 | 删除，变体是参数，选择的解析只剩三条规则 |
| `ThemeConfig.Algorithms`、算法链整体替换的合并语义 | 删除，明暗与密度是各自继承的独立字段 |
| 算法与控件阶段手工维护的 `revision` | 删除，修订身份取自确定性构建的程序集标识 |
| `IControlTokenAlgorithm` 及 `EvaluateGlobal`、四种控件算法模式 | 删除。由输出控件视图的控件推导（6.1）与控件级配置（6.4）取代；原 `EvaluateGlobal` 中的改写换写为 `view.Global` 上的覆盖 |
| `Nova.Dark` 式逐明暗身份、每个阶段类 16 个特性、每个风格四份主题 XML | 删除，明暗、风格、密度是参数，主题按轴选择 |
| 先跑 Ant 计算器再改写、临时改写种子再还原 | 删除，语言自己写全 |
| 多轮重跑与 `Unstable` 判定、`IsExplicit` 守卫、`ATOMUIGEN048` | 删除，由冻结槽位的单遍求值取代 |
| `AppearanceSettings` 无类型旁路 | 删除，成为行为开关 Token |
| 核心里为 Shadcn 增设的 `ColorNeutral` 种子 | 迁入 Shadcn 的语言种子 `BaseColor` |
| 语言包内部的 `ShadcnStyle`、`ShadcnSurface`、`PrimeNGPreset` 等 `internal` 表 | 整理为各语言公开的语言 Token 与组件配方 |
| 写入记账（`RecordWrite`）与每个控件一份的全局 Token 副本 | 删除；setter 只检查冻结位，控件视图只存相对全局的差量 |
| 语言包对核心与控件库的 `InternalsVisibleTo` | 删除 |
| 移动控件中写死的几何值、借用桌面字号阶梯 | 进入 Ant Design 的移动形态与 `AtomUI.Mobile.Controls.Themes.AntDesign` |
| 产品自建的主题包机制（如 MachGrid 的 BrandWarm） | 删除，用具名主题或派生语言 |

### 16.2 步骤

| 步骤 | 内容 | 验收 |
| --- | --- | --- |
| 1 核心 | 语言契约、三类 Token 与四个轴、单遍管线与冻结语义、读集与结构共享；主题定义 XML v2（参考规范、XSD、Reader、Binder、编译 ABI v2、换写命令）；`ThemeConfig` 与选择的新模型；工具链；移除算法链相关机制 | 契约测试与单元测试；v2 的一致性验收（结构、绑定、条件段、分层、指纹、安全边界）；性能验收不低于现有基线（12.5） |
| 2 Ant Design | 主仓各控件包的 Token 类清为纯声明；建立 `AtomUI.DesignLanguages.AntDesign` 与各控件包的 `.Themes.AntDesign`，把现有全局公式与控件公式原样迁入；`AppearanceSettings` 改为开关 Token | 与改造前的 Ant Design 渲染逐 showcase 零差异，明暗 × 密度 |
| 3 Shadcn 与 PrimeNG | 全局推导改为自己写全；把内部表整理为语言 Token 与组件配方；建立两个桌面取值包并补齐全部控件推导 | 与改造前这两种语言的渲染零差异；有意修正的条目单列 |
| 4 移动端 | Ant Design 增加移动形态（依据 Ant Design Mobile）；Mobile 的 Token 类清为纯声明；建立 `AtomUI.Mobile.Controls.Themes.AntDesign` | 与改造前的移动端渲染对照；因改用官方移动取值产生的差异单列 |
| 5 商业产品 | 各产品的 Token 类清为纯声明，建立 Ant Design 取值包（迁入现有公式），再按规划建立其他语言的取值包；先 MachGrid | Ant Design 下零差异；其他语言按声明等级 |
| 6 收尾 | Gallery 的语言与变体选择改用新模型；各仓现有主题文件换写为 v2；文档；样例包 | — |

- 步骤 2 是机械迁移，也是对核心契约的检验：Ant Design 必须能只靠公开 API 完整写出来。
- 步骤 3 工作量最大：两种语言各要为约 150 个 Token 类写全取值。现在未被覆盖的那部分，起点是该控件在该语言下当前的
  实际取值，再逐个核对是否应改用语言 Token。
- 步骤 4、5 可以在步骤 2 完成后与步骤 3 并行。

---

## 17. 成本与风险

- **取值代码按语言数增长。** 每加一种语言，要为每个受支持的控件包写一套完整的控件推导。这是「语言各自负责取值」的
  直接代价，换来的是没有隐式回退、没有叠加关系、每个取值有明确的负责人。
- **控件加 Token 的成本上升。** 新增一个控件 Token 要同时在覆盖它的每个取值包里给值。这是有意的：不允许出现某语言
  下没人负责的 Token。
- **包数量。** 每个带控件 Token 的控件包有「语言数」个取值包，一对一，不合并。主仓有 4 个这样的控件包，3 种语言
  共 12 个取值包。应用开发者只需要记住「用了哪个控件包，就装它的 `.Themes.<Lang>`」，语言包与共用控件的取值包随
  传递依赖进来。
- **跨仓协同。** 核心契约变更会同时影响主仓、移动仓与各产品仓。步骤 1 的公开面要在步骤 2 用 Ant Design 验证后再
  对外仓开放。
- **零差异迁移的工作量。** 步骤 2、3 要求渲染不变，依赖现有的全量脚本视觉验收；过程中发现的既有视觉问题单独记录、
  单独决策。

---

## 18. 待定

主题定义相关：

- 控件上覆盖种子时一律重新派生（7.5）。这与 v1 的默认行为（`Disabled`，不派生）相反；若有大量现存主题依赖旧行为，
  换写的工作量集中在这里。
- 条件段是否需要「或」与取反。现设计只有「且」，多写一个条件段可以表达「或」。
- `Locked` 与轴的固定只约束最终用户的选择，不约束开发者写的 `ThemeConfig`。是否需要一种连开发者配置也不能越过的
  强锁定，取决于有没有「主题由第三方提供、应用不得改动」的场景。
- 具名主题是否允许不绑定语言（一份只含种子的品牌主题同时用于多种语言）。现设计要求必须声明语言。

其余：

1. 冻结语义对复合类型（`BoxShadows`、`Thickness`）做整值级还是成员级。
2. 完整性分析器对条件分支的要求：强制所有路径写全，还是允许「先写默认、再按条件改写」。
3. 三个语言包的语言 Token 类与组件配方的具体清单与命名：步骤 2、3 评审，步骤 5 用首个产品取值包检验后冻结。
4. Shadcn 与 PrimeNG 是否提供移动形态，以及提供时的取值依据。
5. 语言 Token 是否作为资源发布给应用。现设计不发布：应用通过全局 Token 与控件 Token 取值，避免应用界面绑定到某一种语言。
6. 明暗是否需要第三个取值（高对比度）。现设计固定为 Light / Dark，与 Avalonia 的主题变体对应。
7. 读集带来的结构共享能省多少，取决于实际的作用域用法；若测量显示收益不抵 getter 的判断开销，退回「全局一变、
   带控件 Token 的控件全部重算」的现有规则，其余设计不受影响。
