# AtomUI 多设计语言主题架构

日期：2026-11-07。状态：设计方案，待审阅；未开始实现。
基线：AtomUI `release/6.0`（`ae5ac0a32`，6.2.13，Avalonia 12.1.5）。参照：Ant Design 6.7.0（`c811baa979`，本地引用项目）。

## 0. 摘要

**要解决的问题**：让 AtomUI 应用像 ant.design 首页那样，在运行时切换 Ant、Material、shadcn、Bootstrap 等多种设计语言，同一棵控件树不重建、状态不丢，子树可以用不同语言，且不损害性能。

**结论**：设计语言是比主题定义更高一层的概念。一种设计语言由四样东西组成：一套 Token 派生策略、一组语言默认 Token、一套 ControlTheme、少数行为实现。它作为独立程序集交付，经与 Ant 完全相同的协议注册；控件代码只有一份，是 lookless 的；切换语言是一次普通的主题事务。

**为什么行**，一句话版本（第 4 节逐条展开）：

1. Token 层 AtomUI 已经有了，和 antd 同构，只需增加"语言"这一合并层与可替换的派生策略。
2. 视觉层用 Avalonia 的 `Theme` 属性承载：它是普通 StyledProperty，可由样式 Setter 设置；Setter 的值是按控件类型的 DynamicResource，由每个作用域已有的 Token ResourceProvider 回答。因此主题切换与 Token 切换走同一条通知，不需要任何新的传播机制，也不需要重建控件树。
3. 行为层差异只在少数部件和反馈上，用接口契约部件和策略属性吸收，成本只在模板实例化与事件路径，不在布局和渲染。
4. 不完整的语言沿显式回落链补齐，注册期解析成表，运行时查表。
5. 以上每一条都只使用 Avalonia 公开 API，其依赖的框架行为已对照 12.1.5 源码核对，剩余三项由半天 spike 确认。

## 1. 问题与要求

ant.design 首页 "Flexible theme customization" 的切换器提供 14 个预设。一个预设 = 全局 Token + 逐组件 Token + 算法，其中约一半预设还对语义区域注入样式（渐变、硬投影、模糊、全大写、自定义 ripple）。DOM 结构在所有预设下一致。

AtomUI 的要求：

| # | 要求 | 说明 |
| --- | --- | --- |
| R1 | 运行时切换，不重建树，控件属性状态不丢 | ant.design 用 `key` 重挂载预览区；AtomUI 不接受重挂载 |
| R2 | 子树可用不同语言 | 沿用现有 `ThemeConfigProvider` 作用域 |
| R3 | 控件代码只有一份 | 各设计语言的共性控件大同小异；大同是公开 API 与状态语义 |
| R4 | 语言是独立包，可按需加载 | 不引用不占产物；引用未激活只登记一张表 |
| R5 | 第三方可提供语言与语言下的控件主题 | 与 AtomUI 自己的语言同一协议 |
| R6 | 不损害性能 | 切换成本与今天的主题切换同阶；无逐帧成本；内存有上界 |
| R7 | 不碰 Avalonia 私有成员，不修改 Avalonia | 只用公开行为 |
| R8 | 对 release/6.0 的重构只许简化、提升性能、架构优化 | 第 11 节逐条登记 |
| R9 | 机制与平台无关 | 桌面与未来移动控件包共用 |

明确不追求：100% 复刻任何设计系统。antd 的 MUI 预设是"像 MUI 的 antd"，Avalonia Fluent 也不是 WinUI 复刻。目标是在"控件公开 API 与状态语义"这条线以内做到精确；线外（组件清单差异）不属于主题。

## 2. 现状：release/6.0 已有什么

| 已有 | 位置 | 对本方案的意义 |
| --- | --- | --- |
| Seed→Map→Alias 三层全局 Token（25/112/118），算法链 `Evaluate(effectiveSeed, previousMap, nextMap)`，按 Stage 注入显式值 | `Theme/Compilation/ThemeCompiler.cs: CompileGlobalBuilder` | 派生管线已是 antd 结构，只需把算法与 Alias 计算变成可替换 |
| Control Own Token：字段 + `CalculateTokenValues(isDark)` 默认求值 | 各控件 `XxxToken.cs` | 与 antd 组件 Token 默认函数同一模式、同一位置 |
| `ThemeConfig { Tokens, Controls, Algorithms, Inherit }`、合并器、规范化器 | `Theme/Configuration/` | 直接加一层输入 |
| `ThemeManager` 五阶段事务（Capture/Prepare/CommitCore/Publish/Complete）、Snapshot 唯一真源 | `Theme/ThemeManager.cs`、`ThemeTransaction.cs` | 切换语言复用，不加阶段 |
| 作用域：`ThemeConfigProvider`、`ThemeContext`、每作用域一个 `ThemeTokenResourceProvider`、独立窗口的 `ThemeContextLease` | `Theme/ThemeScope*.cs`、`Resources/ThemeTokenResourceProvider.cs` | 视觉层的键由同一个 Provider 回答 |
| Token 消费全部是 DynamicResource（`XxxTokenResource`） | `Resources/TokenResourceExtension.cs` | 证明 DynamicResource 在目标控件处按作用域解析，Theme 键可以同路 |
| Semantic Part：`[SemanticPart]` 声明、`.semantic-*` marker、生成的 Part Style | `Theme/SemanticParts/`、Generator | 等价 antd 6 的 Semantic DOM |
| 每控件一个 AXAML ControlTheme，以类型为键注册，惰性物化 | 各控件 `Themes/`、`Resources/DeferredControlThemeResourceProvider.cs` | 语言主题复用同一惰性 provider |

缺的只有三件：没有"语言"这个概念与合并层；算法是闭合枚举、Alias 公式写死；ControlTheme 只能在挂树时隐式解析，不能随作用域切换。

控件代码的 lookless 程度（`AtomUI.Controls` + `AtomUI.Desktop.Controls`，119 个控件）：111 处代码直接赋视觉属性，16 处命令式 Token 绑定（8 个文件），38 个 `Render` 自绘，`[TemplatePart]`/`[PseudoClasses]` 各 47/60 处，C# 构建的主题 0 个。

## 3. 从 Ant Design 6.7.0 借鉴的原则

对照本地引用项目（6.7.0，`c811baa979`）核对后，采用以下六条；源码细节不进本方案。

| 原则 | antd 的做法 | AtomUI 的落法 |
| --- | --- | --- |
| 主题是一个配置对象 | 全局 Token、逐组件 Token、算法链、是否继承外层 | `ThemeConfig` 加一个 `DesignLanguage` 字段 |
| 派生是链式算法，Alias 是独立一层 | 每个算法接收 Seed 与前一算法的结果；Alias 由固定公式生成 | 同构管线；算法与 Alias 计算按语言登记 |
| 组件 Token 的默认是全局 Token 的纯函数，放在组件旁边 | 每个组件自带"从全局 Token 算出组件默认"的函数 | Own Token 求值器留在控件包，按语言打标签 |
| 变体 = 模板 + 变量 | 模板只消费一组变量，每个变体只给变量赋值 | 控件的内部变量属性由主题 Setter 按状态赋值，代码不计算 |
| 语义区域是公开契约 | 每个组件声明可定制的语义区域，上层配置与实例都能按区域注入样式，可依实例状态 | Semantic Part 与属性选择器 |
| 上层配置可给组件开关设默认 | 6.7.0 新增全局 `variant`，组件按"实例 > 上下文 > 默认"解析 | 语言主题给控件开关设默认值 |

两个有用的事实：antd 的 "Ant Design V4" 预设只由算法链加 Token 构成、没有任何样式注入，说明一种外观差异明显的语言可以不出一个 ControlTheme；antd 的 MUI 预设把点击反馈换成了自己的实现，说明反馈行为需要可替换。

antd 没有而 AtomUI 需要的：模板级替换（Web 用 CSS 就能换形态，Avalonia 要换 ControlTheme）、显式回落链、lookless 约束、契约部件。antd 有而 AtomUI 不需要的：CSS 变量模式、hash 隔离、零运行时模式。antd 切换预设时整体重挂载预览区，AtomUI 不这样做。

## 4. 为什么这个方案可行

每一条给出：要解决什么、机制是什么、依据是什么、否决了哪些替代。

### 4.1 Token 层：加一个合并层，派生策略参数化

**要解决**：语言有自己的默认值（圆角、高度、字体、色系），有自己的派生方式（Ant 的调色板算法、Material 的 HCT 色调板、PrimeNG 式直接写满值）。

**机制**：`ThemeConfigMerger` 的输入多两层（语言基础 Token、语言逐控件默认），顺序为库默认 → 语言基础 → 语言逐控件默认 → 主题定义 → 根请求 → 局部配置；显式值仍按 Stage 注入现有管线。算法从 `Default/Dark/Compact` 闭合枚举改为开放 id，算法与 Alias 计算器按语言登记；Core 提供值表算法，让"直接指定式"语言把一张 Map/Alias 值表当算法用。

**依据**：`ThemeCompiler.CompileGlobalBuilder` 今天就是 `seed(+Seed 显式值) → EvaluateAlgorithms → +Map 显式值 → CalculateAliasTokenValues → +Alias 显式值`，与 antd 的派生顺序一致。改动是参数化，不是重写。

**否决**：保留全局算法，让 PrimeNG 式语言用约 230 个显式值硬压 Ant 的推导（可行但语义错误，且 Material 的 HCT 无处安放）。

### 4.2 视觉层：Theme 属性走作用域资源

**要解决**：一个控件在作用域 A 用 Ant 的 ControlTheme，在作用域 B 用 Material 的，切换时不重建树（R1、R2），且不加新机制（R6、R7）。

**机制**：ThemeManager 启动时为每个已注册控件类型挂一条样式

```text
Style OfType<T> { Setter(StyledElement.Theme, DynamicResource(ControlThemeKey(T))) }
```

每个作用域已有的 `ThemeTokenResourceProvider` 多回答一种键：`ControlThemeKey(T)` → 当前作用域语言下 `T` 的 ControlTheme（惰性物化；未注册则不命中，`Theme` 回到 null，走隐式默认）。切换语言 = 一次普通事务，Publish 阶段的 `ResourcesChanged` 同时驱动 Token 绑定和 Theme 绑定重新取值；只有主题实例真的变化的控件重套模板。

**依据**（Avalonia 12.1.5 源码）：
- `StyledElement.ThemeProperty` 是普通 `StyledProperty<ControlTheme?>`（`Avalonia.Base/StyledElement.cs`）；`Setter.Instance` 只拒绝 Direct 属性（`Styling/Setter.cs`），所以样式可以设置它。
- `OnControlThemeChanged` 移除 Theme 帧并置 `_themeApplied=false`，下一次 `ApplyStyling` 重新应用主题。
- DynamicResource 在目标控件上实例化并沿目标控件的资源链解析，这正是 AtomUI 局部作用域 Token 今天能工作的原因；Theme 键与 Token 键同一条路。
- 用户显式设置的 `Theme` 本地值优先级高于样式值，仍然赢。

**否决**：
- 按语言增删 `Styles` 做补丁（第一版）：Avalonia 移除样式时模板节点 frame 不退役，需要私有 hook；Ant 成为被打补丁的特例。
- 可继承附加属性 + `PropertyEquals` 选择器：能切 Setter，不能换模板；多一条传播机制。
- 按语言派生控件类型（`MaterialButton`）：用户 XAML 写的是 `atom:Button`，运行时换不了类型。
- 换以类型为键的 ControlTheme 资源：`ReevaluateImplicitTheme` 只在挂入逻辑树时调用（`StyledElement.cs`），要重挂载，违反 R1。

### 4.3 行为层：契约部件与策略属性

**要解决**：少数真实分歧不在外观而在部件行为（Select 以 Menu 方式弹出、浮动标签）或反馈（Material 从点击点扩散的 ripple，Ant 的 wave 外扩）。

**机制**：
- 契约部件：`[TemplatePart("PART_Popup", typeof(ISelectPopupHost), IsRequired = true)]`，控件只经接口与部件对话，部件实现随语言包交付并由该语言的模板放入。切换语言 = 重套模板 = 换部件。只用于真实分歧点，预计每控件零到两个。
- 行为策略：控件暴露 `StyledProperty<IFeedbackBehavior?>`，语言主题用 Setter 注入；无状态策略是单例，有状态的由策略为控件建轻量状态并在 detach 释放。控件不内置任何语言的默认策略。
- 控件开关是最后手段：只有差异必须改变控件公开状态机时才加属性，语义对所有语言成立，语言主题设默认值。

**依据**：Avalonia 自己的 `TextBox`/`TextPresenter`、`ScrollViewer`/`ScrollContentPresenter` 就是"控件经部件工作"；接口调用都在事件路径，不在 `Measure/Arrange/Render`。

**否决**：全部用控件开关（控件膨胀成所有语言行为的并集）；全部登记为偏差（放弃精确）。

### 4.4 回落链：不完整的语言也能用

**要解决**：Material 不可能一次覆盖 95 个控件；shadcn 只换值不出一个主题。

**机制**：语言定义声明 `Fallback`。本语言没定义的 ControlTheme、Own Token 求值器、派生后仍未指定的 Map/Alias，沿链取；变体 id 不回落（不支持即 `Failed`）。链无环、目标已注册、链末端语言完整，三条在注册期校验并列出缺口；运行时查的是解析后的 `(语言, 控件)` 表。

**否决**：隐式回落 Ant（Core 知道 Ant）；无回落（Material 要先写满才能用）。

### 4.5 lookless 控件：方案成立的必要条件

**要解决**：如果控件代码里从 Token 算出阴影、圆角、颜色，任何语言的主题都盖不掉它们。

**机制**：控件代码只含行为与契约；视觉值只在带语言标签的 ControlTheme、Own Token 求值器、主题定义里。标准见第 6 节。按语言覆盖范围分控件家族推进，分析器先警告后错误。

**依据**：这是 Avalonia 自身控件与 Fluent/Simple 主题的关系；现状 111 处赋值、16 处绑定是已量化的缺口。

**否决**：全量前置重构（Token-only 语言不需要它，第一个可见成果推迟数月）。

### 4.6 Ant 的位置：逻辑对等，物理原地

**要解决**：Core 不能认识任何一种语言（R5），但也不能为了对称做无收益的迁移。

**机制**：控件包的 ControlTheme 与 Own Token 求值器原地不动，由生成器打 `Ant` 标签，走与其他语言相同的注册、校验、回落；Core 里的 Ant 残留（三个计算器、调色板、Alias 公式、DaybreakBlue）搬进一个很小的 `AtomUI.Themes.Ant`。`AtomUI.Controls` 引用它，`UseAtomUI()` 开箱即是 Ant。

**依据**：每种语言的回落链末端都是 Ant，Ant 的视觉永远在产物里；物理拆出约 380 个 AXAML 与 95 个求值器省不下体积，只增加包数。antd 同样把组件 Token 的默认函数放在组件目录。

**保留的边界**：全局 Token 词汇表来自 Ant Design 的分层与命名，但它是所有语言的 ControlTheme 共同消费的契约，留在 Core；各语言把自己的概念映射到这套词汇。

### 4.7 性能：每项成本有界

| 成本 | 量级 | 说明 |
| --- | --- | --- |
| 启动多挂的按类型样式 | 约 380 条 `OfType`，每条一个 Setter | `OfType` 是类型相等比较；隐式 ControlTheme 的资源查找同时取消，净增接近零。若实测可见，改为一条 `:is(Control)` 样式加自定义 `IBinding` 按 `StyleKey` 算键 |
| 每控件一个 Theme 的 DynamicResource 订阅 | 1 / 控件 | AtomUI 控件已有数十个 Token 订阅 |
| 切换语言 | 一次 Token 事务 + 变化类型重套模板 | 只换 Token 的语言零重套；换形态的语言只重套被覆盖类型 |
| 首次激活某语言 | 物化被查询过的 ControlTheme | 惰性，上界 = 已激活语言数 × 出现过的类型数 |
| 契约部件、策略 | 模板实例化与事件路径 | 无逐帧成本 |
| 引用未激活的语言包 | 定义 + 两张表 | AXAML 不解析 |

目标：Gallery 预览页（约 60 个控件）切换 Publish 在 16 ms 内，首次激活含物化不超过 50 ms；Stopwatch 计时，测试断言。

### 4.8 需要 spike 确认的三项

设计依赖的 Avalonia 行为中，以下三项在源码中只确认了一半，半天 spike 在 `release/6.0` 上验证，任一不成立即停下修订：

1. 样式 Setter 改变已挂载控件的 `Theme` 后实时重套模板、`PART_*` 查找正常（`OnControlThemeChanged` 之后谁触发 `ApplyStyling`）。
2. DynamicResource 不命中时 `Theme` 回到 null 并恢复隐式主题。
3. 作用域 Provider 的 `ResourcesChanged` 能驱动 Popup 内与 lease 窗口内控件的 Theme 绑定。

顺带测量：约 380 条 `OfType` 样式对 Gallery 首页套样式的耗时；Select 换契约部件前后的模板应用耗时。

## 5. 设计

### 5.1 概念模型

```text
Design language ⊃ Theme definition ⊃ Runtime config

Design language（程序集）   Ant（随控件包 + 小包）  Material              Shadcn            Prime（直接指定式）
  ControlTheme              全部控件                部分控件              无                部分控件
  Fallback                  无（链末端，必须完整）  Ant                   Ant               Ant
  Token 派生                Ant 计算器 + Alias      HCT 计算器，Alias 回落  Inherit（回落）    值表
  默认主题                  DaybreakBlue            MaterialPurple        ShadcnZinc        PrimeAura
  主题定义                  语言专属的色板主题；通用主题（只调色板）在任何语言下可用
  变体                      每个语言声明支持的变体 id（Default/Dark/Compact/自定义）及算法
```

| 概念 | 拥有 | 不拥有 |
| --- | --- | --- |
| Core | Token 词汇表、Own Token 字段 schema 与资源 ID 合同、派生管线、事务、作用域、注册表、校验 | 任何具体值、算法、ControlTheme、主题定义 |
| 控件包 | 控件代码、`[TemplatePart]`/`[PseudoClasses]`、Semantic Part 声明、Own Token 字段；带语言标签的 ControlTheme 与 Own Token 求值器 | 代码中的视觉值；无标签的主题；算法与主题定义 |
| 语言包 | 定义（Id、显示名、默认主题、派生策略、回落、基础 Token、逐控件默认）；主题定义；需要的 ControlTheme、求值器、基元、部件、策略 | 控件行为；色板之外的用户配置 |
| 主题定义 | 一组 Token 值；所属语言或通用；Appearance | 任何 ControlTheme、模板、样式 |

### 5.2 Token 派生

```text
seed  = library defaults + 显式 Seed 值
map   = language.Algorithms[请求变体…].Evaluate(seed) + 显式 Map 值
alias = language.AliasCalculator(map) + 显式 Alias 值
gaps  = fallback chain
own   = language.TokenDefaults[control](alias) + 显式 Own 值      // 沿回落链取求值器
```

- `ThemeAlgorithm` 是开放 id（`readonly record struct ThemeAlgorithm(string Id)`），`Default/Dark/Compact` 为约定常量。请求与控件级 `Custom` 算法的 id 在当前语言的 `Algorithms` 中解析，不支持 → `Failed`；`DesignLanguageInfo.SupportedAlgorithms` 供 UI 禁用开关。
- 外观由算法描述符的 `AppearanceEffect` 决定；值表语言的 Dark 表声明 `Dark`。
- 直接指定式语言：基础值表在定义里，色板变体是专属主题定义 XML（Token 可写任意 Stage，XML v1 已支持）。
- `NormalizeFinalBehavior`（动效开关等）与语言无关，留在管线末尾。
- 语言扩展全局 Token（Material 的色调角色、五级 elevation）：设计保留，最后按证据实施。首批 Material 先映射到现有词汇并记录有损项。

### 5.3 语言与主题的组合

- 根请求必须给语言；主题 id 可省略 = 该语言默认主题。只注册一个语言时它是初始语言；多个时 `WithInitialTheme` 必须指定。
- 专属主题跨语言 → `Failed`。只换语言时：通用主题沿用，专属主题换成新语言默认；结果写进 `ThemeState`。
- 局部 `ThemeConfigProvider` 指定语言未指定主题套同一规则；Popup、独立窗口沿现有资源链继承。
- 用户目录主题可声明 `DesignLanguage`；声明未注册语言 → Catalog 诊断、不可选。用户目录不得定义语言。
- 无语言注册：控件 lookless 无外观，像未引用 Fluent 的 Avalonia 应用；启动记一条 `ATMTHM` 诊断，不抛。

### 5.4 事务路径

沿用 Capture / Prepare / CommitCore / Publish / Complete：

1. Capture：解析 `(语言, 主题)`，变体 id 解析为描述符；失败即 `Failed`。
2. Prepare（可后台）：按 5.2 合并与派生，产出 Snapshot；缓存键含语言、派生计划修订、变体链。不物化 ControlTheme。
3. CommitCore：替换 Snapshot 引用。
4. Publish：Provider 发 `ResourcesChanged`，Token 与 Theme 绑定重新取值，首次命中的 ControlTheme 在此物化。
5. Complete：`ThemeState` 记录实际语言与主题。

### 5.5 注册表与启动校验

`DesignLanguageRegistry`（ThemeManager 持有）：`Definitions`（Id → 定义）、`Themes`（(Id, Type) → 惰性 provider）、`TokenDefaults`（(Id, Identity) → 求值器）。启动校验：重复 Id、`Fallback` 未注册或成环、`DefaultThemeId` 不可解析、同 (Id, Type) 重复、主题集引用无定义的 Id、链末端语言对任一已注册控件缺 ControlTheme 或求值器、派生后有未指定 Token → 启动失败并列出全部缺口。回落链在注册期解析成表。

运行期追加：语言包不新增 Token schema、不新增控件 identity，`IThemeManager.AddDesignLanguage` 在 UI 线程追加、重跑解析与校验、派发事件。JIT 可配合 `AssemblyLoadContext`；NativeAOT 无运行期装配加载。

### 5.6 公开 API

```csharp
public readonly record struct ThemeAlgorithm(string Id)
{ public static readonly ThemeAlgorithm Default = new("Default"), Dark = new("Dark"), Compact = new("Compact"); }

public interface IThemeAliasCalculator { void Calculate(DesignToken map); }

public sealed class ThemeDerivation
{
    public static ThemeDerivation Create(IThemeAliasCalculator aliasCalculator, params ThemeAlgorithmDescriptor[] algorithms);
    public static ThemeDerivation Tables(ThemeTokenTable @default, ThemeTokenTable? dark = null, ThemeTokenTable? compact = null);
    public static ThemeDerivation Inherit { get; }                     // 全部沿回落链取
    public ThemeDerivation WithAlgorithm(ThemeAlgorithmDescriptor descriptor);
}

public sealed class DesignLanguageDefinition
{
    public DesignLanguageDefinition(string id, string displayName, string defaultThemeId,
        ThemeDerivation derivation, string? fallback = null, ThemeConfig? tokens = null,
        IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig>? controls = null);
}

public sealed class DesignLanguageThemeSet   // 一个程序集为某语言贡献的 ControlTheme 与求值器，由生成器产出
{
    public DesignLanguageThemeSet(string languageId,
        IEnumerable<ControlThemeAssetDescriptor> themeAssets,
        IEnumerable<ControlTokenDefaultsRegistration> tokenDefaults);
}

public interface IThemeManagerBuilder
{
    void AddDesignLanguage(DesignLanguageDefinition definition);
    void AddDesignLanguageThemes(DesignLanguageThemeSet themes);
    void WithInitialTheme(string designLanguage, string? themeId = null, ThemeConfig? config = null);
}

public sealed record ThemeRequest(string DesignLanguage, string? ThemeId, ThemeConfig? Config, ThemeTransitionReason Reason);
public sealed record ThemeInfo(string Id, string Name, ThemeAppearance Appearance, bool IsDefault, string? DesignLanguage, Color? AccentColor);
public sealed record DesignLanguageInfo(string Id, string DisplayName, string DefaultThemeId, string? Fallback,
    IReadOnlyList<ThemeAlgorithm> SupportedAlgorithms, int ThemedControlCount);

public interface IThemeManager
{
    IReadOnlyList<DesignLanguageInfo> AvailableDesignLanguages { get; }
    void AddDesignLanguage(DesignLanguageDefinition definition, params DesignLanguageThemeSet[] themes);
    event EventHandler? DesignLanguagesChanged;
}

public sealed class ThemeConfig        { public string? DesignLanguage { get; } }   // 局部作用域切语言
public sealed class ThemeConfigBuilder { public ThemeConfigBuilder WithDesignLanguage(string? id); }
public sealed record ThemeState        { public string DesignLanguage { get; } }
```

主题 XML v1 增加可选根属性 `DesignLanguage`（省略 = 通用）；`IsDefault` 改由语言定义的 `DefaultThemeId` 决定。AXAML 增加 `{atom:FallbackControlTheme atom:Button}` 标记扩展，供语言主题 `BasedOn` 回落语言的主题。

### 5.7 包布局与生成器

```text
AtomUI.Controls / AtomUI.Desktop.Controls / …          控件包：<AtomUIThemesLanguage>Ant</AtomUIThemesLanguage>
+-- Avatar/Avatar.cs、Avatar.SemanticParts.cs           行为与契约，无视觉值（分析器保证）
+-- Avatar/AvatarToken.cs                               Own Token 字段 + 参考求值器，登记为 Ant
\-- Avatar/Themes/AvatarTheme.axaml                     ControlTheme，登记为 Ant

AtomUI.Themes.Ant/                                      小包：<AtomUIDesignLanguage>Ant</AtomUIDesignLanguage>
+-- AntDesignLanguage.cs                                定义：默认主题 DaybreakBlue、Derivation、无 Fallback
+-- Algorithms/                                         三个计算器、调色板、Alias 计算器（自 Core 搬入）
+-- Behaviors/AntWave.cs                                反馈策略
\-- Definitions/DaybreakBlue.theme.xml …                自 Core 搬入

AtomUI.Themes.Material/                                 <AtomUIDesignLanguage>Material</AtomUIDesignLanguage>，Fallback = Ant
+-- MaterialDesignLanguage.cs
+-- Algorithms/MaterialDefaultCalculator.cs …           HCT；Alias 回落
+-- Definitions/MaterialPurple.theme.xml …
+-- Primitives/、Behaviors/、Parts/                      基元、策略、契约部件实现
+-- TokenDefaults/、Themes/                             需要的控件
\-- FIDELITY.md                                         逐控件对照结论与未覆盖项

AtomUI.Themes.Shadcn/                                   Fallback = Ant，Derivation = Inherit，只有定义与色板主题
AtomUI.Themes.AntV4/                                    只换 Default 算法与组件 Token，零 ControlTheme（派生层验证用例）
```

生成器两类程序集：**语言包**（`AtomUIDesignLanguage=X`）登记定义、`Themes/**` ControlTheme、`[ThemeAlgorithm]`、`[ThemeAliasCalculator]`、`[ControlTokenDefaults]`、`Definitions/*.theme.xml`（强制 `DesignLanguage=X`）；**控件包**（`AtomUIThemesLanguage=X`）登记控件，其 ControlTheme 与求值器打标签 `X`，不得含定义、计算器、主题定义。一个程序集只属于一个语言。第三方控件包用 `AtomUIThemesLanguage` 声明自带主题属于哪个语言；为 Material 另建 `Vendor.Controls.Themes.Material`。

构建期校验：类型键 ControlTheme 的 `TargetType` 必须是已注册 Control 且每程序集唯一；非 Optional Semantic marker 齐全；`FallbackControlTheme` 类型与 `TargetType` 一致且所在语言声明了 `Fallback`；控件代码不得含视觉字面量、命令式 Token 绑定、计算型视觉属性、读取语言 id、内置默认策略或部件实现（存量先警告，按家族转错误）。

### 5.8 Gallery

顶栏先选语言再选主题（专属 + 通用），不支持的变体按 `SupportedAlgorithms` 禁用。新增"设计语言预览"页：复刻 ant.design 预览区控件集合，一个 `ThemeConfigProvider` 承载，切换只改其 `Config.DesignLanguage`；同页两个 Provider 对照；"复制主题代码"导出 C# 与 XML。Gallery 引用 Material、Shadcn、Bootstrap、AntV4。

## 6. 控件 lookless 标准

控件不感知语言，也不自带外观。差异分三档：

| 差异 | 例子 | 吸收点 |
| --- | --- | --- |
| 值 | 主色、圆角、高度、字体、阴影、密度 | 语言基础 Token、Own 求值器、主题定义 |
| 区域 | 渐变、硬投影、图标尺寸、hover 效果、有无 ripple | `BasedOn` 回落主题的派生 ControlTheme；基元；策略 |
| 结构 | Material Switch 圆钮在轨道外、浮动标签 | 完整 ControlTheme 换模板；契约部件 |

六条规则（逐控件二元审计，能查的进分析器）：

1. **视觉值不进代码**。不得出现画刷、颜色、阴影、圆角、厚度、字号、尺寸的字面量或由 Token 计算得出的值；不得命令式绑定 Token。自绘控件把画刷、笔、几何参数暴露为 StyledProperty，在 `Render` 中只读取。
2. **状态与变量可以进代码，计算不行**。离散状态（`EffectiveVariant`、`EffectiveIsDanger`）可作为内部属性或伪类供选择器使用；视觉类型的内部属性（`VariantShadow`、`EffectiveCornerRadius`）可作为模板变量存在（即"模板 + 变量，变体改变量"），但值只能由主题 Setter 按状态赋予（`^[EffectiveVariant=Solid]`、`^[SizeType=Large]`），代码不得计算或给默认。
3. **模板契约显式、最小、按需契约化**。`[TemplatePart(Name, ContractType, IsRequired)]` 列全代码依赖的部件；只有真实分歧点用接口做 `ContractType`，其余具体类型；`[PseudoClasses]` 列全伪类；`OnApplyTemplate` 只经 `NameScope` 取部件，可选部件为空时降级不抛；不按类型或 Name 搜索可视树。
4. **行为经策略属性注入，不内置默认**。`StyledProperty<IXxxBehavior?>` 默认 null。
5. **几何从属性推导，不写常量**。
6. **没有主题也能构造与运行行为**。无模板下 `Measure/Arrange` 零尺寸、事件与命令照常；这是 lookless 的验收用例。

Semantic marker 是主题的义务：每种语言的主题保持同一组非 Optional marker。行为差异不属于主题；需要时给控件加全语言可用的属性，由语言主题设默认值。控件代码不得读取语言 id 或按语言分支。

## 7. 精确度的边界

大同是控件的公开 API 与状态语义，所以代码一份；小异由机制按深度逐级吸收：

| 层 | 覆盖 | 机制 |
| --- | --- | --- |
| 静态视觉 | 完全 | Token、ControlTheme |
| 渲染能力（backdrop blur、渐变描边） | 受 Avalonia 渲染器限制 | 能做的用主题与基元；做不到的记入 `FIDELITY.md` |
| 反馈与动效 | 完全 | 策略或基元 |
| 部件行为 | 完全 | 契约部件 |
| 公开 API 与状态语义 | 不由主题改变 | 必要时加全语言共用的属性 |
| 组件清单（Material 的 FAB、Navigation Rail） | 不覆盖 | 另立控件包 |

处理顺序固定：Token → ControlTheme → 基元/策略 → 契约部件 → 控件开关 → 记录。`FIDELITY.md` 是记录不是门禁；门禁是对语言包自己基准的回归。

## 8. 实施顺序

| 步 | 内容 | 验收 |
| --- | --- | --- |
| 1 | Spike（4.8 三项 + 两项计时） | 结论记录；任一不成立停下修订 |
| 2 | 语言协议与 Ant 小包：生成器两类程序集属性；控件包主题与求值器原地打标签；Core 的算法、调色板、Alias、DaybreakBlue 搬入 `AtomUI.Themes.Ant` | 只有一个语言，运行时仍走隐式查找；现有测试全过 |
| 3 | 分析器：五条诊断，存量为警告并产出清单 | 清单与第 2 节基线一致 |
| 4 | Core 派生层：开放 id、`IThemeAliasCalculator`、`ThemeDerivation`、值表算法、按语言登记 | Ant 回归不变 |
| 5 | Core 语言层：注册表、回落解析与校验、请求/目录/状态/XML 属性、合并层、Theme 键与 Provider 分支、启动样式、`FallbackControlTheme`、运行期追加；控件包不再导出类型键资源 | 第 9 节单元测试 |
| 5a | 控件 lookless 重构：按 Material 预览集合涉及的家族先做，警告清零转错误，含契约化与策略化 | 无语言宿主行为测试 + Ant 外观关键区域不变 |
| 6 | 语言包：Shadcn、Bootstrap、AntV4、Material（预览集合）、一个直接指定式测试语言 | 第 9 节集成测试 |
| 7 | Gallery 切换器与预览页 | ant.design 式切换可见 |
| 8 | Material 扩展到其余控件；`FIDELITY.md` 列出词汇限制 | 持续 |
| 9 | 扩展 Token，仅当第 8 步证明损失不可接受 | 按证据 |

量级：第 1 到 5 步数周；第 6 步含对应家族 lookless 另数周；第 8 步长期。

## 9. 验证

单元（`AtomUI.Core.Tests`）：合并顺序逐对；回落链三类与三条校验；派生式/值表式/不支持变体；Ant 回归（只注册 Ant 时现有测试不变）；组合与回退；Theme 键命中/回落/物化一次/A→B→A 实例数不变；四种宿主（根、Provider 子树、Popup、lease 窗口）的作用域正确；运行期追加；WeakReference 回收；lookless（无语言宿主下控件可构造与运行）。

集成（控件测试 / Gallery 测试）：每语言 × 预览页关键区域按容差策略对比语言包自己的基准；切换前后 TextBox 文本与选区、ListBox 选中、焦点不变；每语言主题的 Semantic Part 预览解析全部非 Optional 部件；用户显式 `Theme` 不被改写；Select 换契约部件后同一套行为用例在两种语言下通过。

性能：4.7 的两项计时在 Gallery headless 宿主下断言。

## 10. 决策表

| 问题 | 决策 | 否决的替代及原因 |
| --- | --- | --- |
| 语言与主题定义的关系 | 层级：语言 ⊃ 主题；主题专属或通用 | 正交：需要裁决谁覆盖谁，目录无法分组 |
| 视觉层形态 | 一个语言一套 ControlTheme | 补丁样式：Ant 特殊化、运行时改选择器、碰框架移除样式缺陷 |
| 语言如何到达控件 | `Theme` 属性经 DynamicResource 走作用域 Provider | 附加属性 + PropertyEquals：不能换模板；派生类型：运行时换不了；换类型键资源：要重挂载 |
| Token 派生归谁 | 语言；一条管线，值表算法覆盖直接指定式 | 全局算法：直接指定式只能硬压 230 个值 |
| 变体 id | 开放字符串 | 闭合枚举：语言无法声明自己的变体或"不支持" |
| 不完整的语言 | 显式回落链，链末端完整 | 隐式回落 Ant：Core 认识 Ant；无回落：Material 不能分步交付 |
| Ant 的地位 | 逻辑对等，Core 不认识；视觉物理留在控件包打标签，定义与派生在小包 | 物理全拆：回落链末端永远是 Ant，拆了不省体积，多 380 个文件迁移与一倍包数 |
| Own Token 默认求值 | 逻辑归语言，物理留 Token 类（antd 同） | 搬入语言包：无收益搬运 |
| 无语言注册时 | 与 Avalonia 一致，无外观，记诊断 | 启动失败：与 lookless 模型矛盾，阻碍无 UI 测试 |
| 控件代码里留什么 | 离散状态与模板变量；计算与默认一律迁出 | 全迁：选择器失去依据；全留：语言换主题时 Ant 视觉漏出 |
| lookless 节奏 | 分析器先上，按语言覆盖分家族 | 全量前置：成果推迟数月 |
| 部件行为替换 | 契约部件 + 策略属性，只用于真实分歧点 | 控件开关：控件膨胀；派生类型：运行时不可切 |
| 精确度目标 | 边界在公开 API 与状态语义；`FIDELITY.md` 记录 | 承诺 100%：任何单代码库做不到 |
| 语言扩展 Token | 设计保留，最后按证据 | 现在做：碰资源 ID 合同，损失尚无证据 |
| 语言包能否带代码 | 能：基元、策略、部件实现，只供自己的主题 | 只许 AXAML：反馈与部件差异只能记偏差 |
| 可选控件包的语言主题 | Ant 随包；其他语言按需 `AtomUI.Themes.<Lang>.<Pkg>` | Ant 也拆：同"Ant 的地位" |

## 11. 对 release/6.0 的改动登记

| 改动 | 类别 | 理由 |
| --- | --- | --- |
| Core 的三个计算器、调色板、Alias 公式、`DaybreakBlue.theme.xml` 搬入 `AtomUI.Themes.Ant`；控件包主题与求值器原地打 `Ant` 标签 | 架构优化 | Core 不含任何语言的值与算法；不做无收益迁移 |
| `DesignToken.CalculateAliasTokenValues` 抽为 `IThemeAliasCalculator` | 架构优化 | Token 定义类只剩数据 |
| `ThemeAlgorithm` 闭合枚举改开放 id；算法按语言登记 | 新能力 + 架构优化 | 派生策略归语言 |
| 删除 `IThemeManager.DEFAULT_THEME_ID`；`WithInitialTheme` 语言优先 | 新能力 | Core 不再知道 DaybreakBlue |
| `ThemeRequest` 加 `DesignLanguage`、`ThemeId` 可空；`ThemeInfo`/`ThemeState` 带语言；XML v1 `DesignLanguage` 属性，`IsDefault` 改由语言决定 | 新能力 | 层级模型 |
| `ThemeConfig.DesignLanguage`；Merger 加两层 | 新能力 | 复用合并器 |
| Provider 回答 `ControlThemeKey`；启动挂按类型 Theme 样式；控件包不再导出类型键资源 | 新能力 | 主题来源从挂树时隐式查找改为随作用域变化的显式绑定 |
| `DesignLanguageRegistry`、定义/主题集类型、回落解析与校验、`FallbackControlTheme`、Catalog 分组 | 新能力 | 复用 `ControlThemeAssetDescriptor` 与惰性 provider |
| 控件 lookless 重构（分家族） | 架构优化 | 方案成立的必要条件 |
| 根 Context 样式选择器由 `OfType<TopLevel>()` 改为 `Is<TopLevel>().PropertyEquals(ParentProperty, null)` | 架构优化，可独立提交 | 现样式把根 Context 以样式值写到 PopupRoot 上，覆盖它从 Popup 继承的局部 Context |

不改动：事务结果枚举、Own Token 字段与资源 ID 合同、资源扩展、Semantic Part 生成、`ThemeContextLease`、编译主题包 ABI、用户主题目录安全边界。不引入别名、兼容层或过渡 API。
