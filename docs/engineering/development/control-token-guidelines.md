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
    -> mark explicit Control Own Token overrides
    -> calculate remaining Control Own Token defaults
    -> re-apply explicit Control Own Token overrides
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
        if (!IsExplicit(RatingTokenKind.StarGap))
        {
            StarGap = StarSize / 4; // StarSize 此时已含用户的显式覆盖
        }
    }
}
```

编译器在调用 `CalculateTokenValues` 前已写入用户的显式 Own Token 覆盖并置显式位，计算结束后再次写入显式值。因此：

- 被其他字段读取的基字段（`ContentFontSize`、DataGrid 的 `BorderColor`）必须以 `if (!IsExplicit(kind)) Field = …;`
  守卫默认赋值，否则派生字段在计算期间读到的是刚被写回的默认值；只被消费的字段不需要守卫，编译器最后一步会重写
  显式值。分析器 `ATOMUIGEN048` 报告未守卫的基字段。
- 需要"用户没设置时才派生"的字段用 `IsExplicit(kind)` 判断；禁止以 `double.NaN`、`default`、`null` 等哨兵值
  判断显式性，Color、Thickness、CornerRadius 等类型没有可用哨兵。
- 内部派生字段（例如 DataGrid 由公开 `BorderColor` 派生的 `TableBorderColor`）从公开字段派生，主题消费派生字段
  时用户覆盖公开字段仍然生效。

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

## Own Token 承诺面

承诺面是主题系统对用户的声明："这些 Token 设置后一定有效果"。承诺面 = 全部 Global Token + 每个 Control 的
Active Own Token。每个 Own Token 字段必须属于以下之一：

| 分类 | 含义 | 要求 |
| --- | --- | --- |
| Active | 有真实消费者 | 所属包的主题 AXAML 或代码至少引用一次；该字段的生效测试必须通过 |
| Reserved | 暂无消费者，有保留理由 | 字段加 `[ReservedToken("原因")]`；文档与工具标为保留，不出现在"可调"列表 |
| 删除 | 既无消费者也无理由 | 删除字段，写入发布说明 |

生成器对每个 Own 字段做包内引用分析：未引用且未标 `[ReservedToken]` 的字段报构建诊断（迁移期为警告，家族迁移完成后
转错误）。Reserved 不是长期状态；字段接通消费后去掉 Attribute，长期没有消费计划的字段删除。

Global Token 不要求每个 Control 都消费。生成器另外产出消费图：哪个 Control 的主题与代码消费了哪个 Global Token。
消费图供生效测试骨架、文档生成和将来的主题工具读取，不进入 schema、不限制配置，也不是支持边界。

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

<!-- 共享基元（无 Token 身份）的主题：读取当前所属公开 Control 的 Effective Global Token。 -->
<Setter Property="BorderBrush" Value="{atom:OwnerTokenResource ColorBorder}" />
```

`RatingTokenResource` 同时读取两类 Token，消费语法不写 `Global=` 或 `Own=`。生成器产生强类型
`RatingTokenKey` 和 MarkupExtension 构造参数。主题作者可以选择 Rating Own Token 和 Global Token schema 中的
全部候选；错误名称在 AXAML 编译期失败，运行时不解析字符串。

生成扩展的 Global 与 Own 分支都携带实际 owner 的资源键。直接 boxed Token enum 仍属于支持的资源键形式；Control、
TokenResourceExtension、TokenKey 和 OwnTokenKind 的使用都应保留对应注册片段。把 enum 转为裸整数或仅做常量计算
不代表使用控件资源。Token-only、生成 identity-only 的代码不能依赖 owner 控件稍后才被构造来补注册。

选择规则由 ControlTheme 的 `TargetType` 是否拥有 Token 身份决定：

| TargetType | 全局 Token 的读法 | 说明 |
| --- | --- | --- |
| 有 Token 身份的公开 Control（Button、LineEdit、Rating…） | `{atom:XxxTokenResource Name}` | 读自己的 Effective Global Token，用户在 `Controls[Xxx]` 下的覆盖生效 |
| 无 Token 身份的共享基元与部件（InputControlFrame、AddOnDecoratedBox、各类 Item / Cell / Presenter） | `{atom:OwnerTokenResource Name}` | 读继承到的 `ThemeScope.TokenOwner` 所指公开 Control 的 Effective Global Token；无 owner 时读全局值 |
| 应用级样式，或明确要求所有 Control 永远共享同一值 | `{atom:SharedTokenResource Name}` | 后者在 AXAML 中加注释说明共享理由 |

- Control 主题里不得用 `SharedTokenResource` 读全局 Token，否则 `Controls[Xxx]` 的覆盖对该消费点无效。生成器按
  TargetType 校验读法，迁移期警告，家族迁移完成后转错误。
- 每个有 Token 身份的公开 Control 的默认 ControlTheme 在首次解析时由 ThemeManager 注入只匹配该类型 StyleKey 的
  `Setter(ThemeScope.TokenOwner, identity)`；Control 作者不写任何代码。`BasedOn` 这个默认主题的未注册部件主题
  （NumericUpDownSpinner）不带走该 identity，继续继承宿主。模板里嵌套的公开 Control 用自己的 TokenResource 读自己的
  有效值（SearchEdit 里的 Button 读 `ButtonTokenResource`）。用户不经 `BasedOn` 完全替换默认 ControlTheme 时，基元回落为全局值。
- 注册控件作为部件（IconPresenter、IconButton、ScrollViewer、Popup）携带自己的 identity：Setter 目标或模板节点在它们之上
  或之内时，`OwnerTokenResource` 解析到该部件。需要跟随宿主的值在无身份层读取后向下绑定，或显式读宿主的 `XxxTokenResource`。
- owner 透明容器：纯容器类注册控件（`atom:Popup`）以 internal `[TokenOwnerTransparent]` 标记，ThemeManager 不为它注入
  owner，弹层内容（下拉候选项、弹层框体）经 Popup 的逻辑父级继承宿主的 owner；容器自身的视觉以自己的 `XxxTokenResource`
  显式读取（Popup 读 `PopupTokenResource`）。只属于一个宿主的弹层内容仍以宿主的 `XxxTokenResource` 显式读取（Select 的候选项读
  `SelectTokenResource`），不依赖继承链；弹层中的注册控件（ComboBoxItem、CascaderView）用自己的 identity 读取 Global Token，
  宿主的 Own Token 仍以宿主 TokenResource 读取。只作为某个宿主模板部件使用、没有 Own Token 的注册控件（SliderTrack、
  SliderThumb，以及只属于一个宿主的项容器 StepsItem）同样标记 `[TokenOwnerTransparent]`，继承宿主 owner，主题以宿主
  `XxxTokenResource` 读取。
- 多个有身份的控件共用一个无身份基础主题（AbstractProgressBarTheme 之于 ProgressBar、CircleProgress 等）且共享 Own Token
  语义时，Own Token 定义放进抽象 `[ControlDesignToken]` 层，由各控件终端继承；基础主题只以 `OwnerTokenResource` 读 Global
  Token，Own Token 由各控件自己的主题以自己的 `XxxTokenResource` 读取，基础主题需要时经内部属性绑定使用。
- 只属于一个宿主的无身份部件可以直接读宿主的 `XxxTokenResource`；被多个宿主共用的部件（SelectHandle、InputControlFrame）
  用 `OwnerTokenResource`。
- 共享主题被无身份的派生部件复用时（TextBoxTheme 之于 EmbeddedTextBox，ButtonSpinnerTheme 之于 NumericUpDownSpinner），
  即使 TargetType 有身份，全局值也以 `OwnerTokenResource` 读取，部件随宿主；在注册控件自身上 owner 就是它自己。这种主题必须在
  `<ControlTheme>` 前紧邻一条 `<!-- owner: 原因 -->` 注释；没有该标记时，有身份 TargetType 上的 `OwnerTokenResource` 报 ATOMUIGEN041。
- `OwnerTokenResource` 只接受全局 Token；基元没有 Own Token。
- ControlTheme 不声明 `ControlTokenScope.Identity`，也不存在 ambient snapshot fallback。
- 过渡时长不分 TargetType，一律把 `<Transitions>` 包在 `<atom:TokenTransitions>` 中，并在过渡上用附加属性
  `atom:TokenTransitions.Duration="MotionDurationMid"`（`SharedTokenKind` 枚举键）声明时长 Token：
  `<Setter Property="Transitions"><atom:TokenTransitions><Transitions><atom:SolidColorBrushTransition Property="Background" atom:TokenTransitions.Duration="MotionDurationMid" /></Transitions></atom:TokenTransitions></Setter>`。
  它按接收 `Transitions` 的 Control 解析（有身份的 Control 读自己的 Effective Global 值，基元读继承 owner 的值，与
  `OwnerTokenResource` 相同），随主题发布、`EnableMotion=false`（时长归零）、`Controls[Xxx]` 覆盖和局部
  ThemeConfigProvider 作用域实时变化；未声明 Token 的过渡保留写死的时长。直接写在 `<Transitions>` 上的值是该样式
  全部目标共享的单个实例，无法按 Control 区分；旧的 `SharedTokenResourceValue`（加载时一次性取应用级值）已删除，
  ControlTheme 中出现即报 ATOMUIGEN041，且没有 `shared:` 豁免。

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

### 控件代码的七条消费规则

控件代码里凡是影响视觉的值都来自主题，并在主题变化时重算。以下规则由分析器检查，不能静态证明的部分由评审检查：

1. 视觉值（颜色、画刷、阴影、线宽、圆角、字号、尺寸）不得以字面量出现在控件代码里；`Colors.Transparent`、`0`
   和行为常量（阈值、计数、索引）允许。
2. 读 Token 只经 `ControlTokenAccessor`、`TokenResourceBinder` 或 AXAML 的三种 TokenResource；禁止
   `Application.Current.TryGetResource`、`TryFindResource`、`FindResource` 读 Token，禁止读取失败时回落写死的值。
   局部 `ThemeConfigProvider` 的覆盖必须对所有读取生效。
3. 由 Token 算出的视觉值必须在主题变化时重算：挂载期绑定到属性并让变化驱动重算或重测（`AffectsMeasure`），
   或 `ThemeTokenResolver.Subscribe`；不得在测量或构造时一次性读 Token 后缓存。
4. 预设色（Badge、Tag、ToolTip、Button 的 preset color）从快照的 `PresetPalettes` 取并按快照缓存，不在控件实例里
   重算，也不以 RGB 常量写死；暗色下预设色必须跟随色板。
5. 主题里需要被状态 Setter（`:pointerover`、`:disabled`、暗色 variant）覆盖的默认值必须写成 Style Setter，不能写在
   模板节点的属性上，默认值与状态值在同一层 Setter 里才能一眼对照（ATOMUIGEN046 检查）。TokenResource 在模板内为
   Template 优先级，会被触发样式（伪类、类选择器）覆盖，但仍盖过同节点的非触发 Style Setter；模板外写在元素属性上
   仍是 LocalValue，写在 Setter 里取该样式的优先级。
6. 代码写入控件或子节点的值用 `SetCurrentValue`，或可撤销的 Template 优先级值；不得以本地值覆盖用户与主题的绑定。
7. 代码算出的视觉值写入 internal 属性，由主题 Setter 投射到公开视觉属性；公开属性留给用户与应用样式，应用样式与
   本地值保持更高优先级。

Button 是参考实现：`ControlTokenAccessor.Capture<Button>` 读控件有效值，`ThemeTokenResolver.Subscribe` 在主题
变化时重算变体画刷，变体画刷经资源键写入以支持作用域覆盖。

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

- 主题 AXAML 里的数值字面量只允许表达布局常量，并以 `<!-- layout -->` 注释标注，分析器据此放行；线宽、分隔线、
  圆角、字号、阴影等视觉值必须读 Token，边框与分隔线线宽统一用 `LineWidth`。颜色字面量不允许出现。
- 需要被状态 Setter 覆盖的默认值写成 Style Setter，不写在模板节点属性上（消费规则 5）。
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
- `CalculateTokenValues` 以 `double.IsNaN` 或其他哨兵判断 Own Token 显式性。
- `CalculateTokenValues` 内对 Own 字段无条件赋值后，同一方法又读取该字段派生其他字段（基字段未用 `IsExplicit` 守卫）。

以下问题由分析器报告，迁移期为警告，所属控件家族迁移完成后由项目 `.editorconfig` 升级为错误：

- 有 Token 身份的 TargetType 的 ControlTheme 用 `SharedTokenResource` 读全局 Token；无 Token 身份的 TargetType 用
  `SharedTokenResource` 而非 `OwnerTokenResource`，且没有共享语义注释。
- Own Token 字段在所属包内无引用且未标 `[ReservedToken]`。
- 控件代码经 `Application.Current` 或 `FindResource` 系列读 Token，或读取失败回落字面量。
- 控件代码里的视觉字面量。
- 主题 AXAML 里未标 `<!-- layout -->` 的数值字面量，或任何颜色字面量。
- 模板节点属性上写了被同一主题状态 Setter 覆盖的默认值。
- 控件代码以 `SetValue` 或属性赋值把 LocalValue 写入视觉属性。

配置规范化必须在身份去重前检查 owner 冲突。跨包依赖在全部入口返回后统一校验；缺少 RequiredTokenOwners、未注册
exact CLR type、无效配置名称、资源与语义契约不完整时启动失败，不能回退到基类 identity 或追加全包注册。
新注册诊断的语义与预留状态见 [编译期诊断规范](compiler-diagnostics-guidelines.md)，不沿用旧 LINK ID。

## 验证要求

| 改动类型 | 验证要求 |
| --- | --- |
| 新增 Control | 验证 identity、可选 Own Token、语义/主题片段、包入口和无运行时发现路径；内部资源控件不伪造 Token owner。 |
| 修改 Own Token | 验证强类型 key、默认计算、override 先于默认计算且最终胜出、AXAML 消费，以及该字段的生效测试。 |
| 新增或裁决 Own Token 字段 | Active 字段补消费并通过生效测试；Reserved 字段写明理由；删除字段写入发布说明。 |
| 修改 Own Token 继承层 | 验证继承链诊断、属性扁平化、base 计算顺序、跨程序集增量失效、sibling 隔离和 schema revision 等价。 |
| 修改 Control 对 Global Token 的使用 | 验证 Effective Global 覆盖、Global fallback、算法间接派生和跨 Control 隔离；共享基元经 `OwnerTokenResource` 读到所属 Control 的覆盖，无 owner 时回落全局值。 |
| 迁移一个控件家族的主题读法 | 该家族全部 Active Own Token 与消费的 Global Token 生效测试全绿；`Controls[X]` 覆盖只改变 X；分析器对该家族转错误。 |
| 修改 Semantic Part | 验证 selector marker、ContractType、cardinality、模板变体、Style-only 保留与 descriptor 一致性。 |
| 修改 Semantic Part Theme | 额外验证 owner/Part Token 分工、TargetType 和默认/实例替换。 |
| 修改生成器 | 验证确定性、增量构建、Token/identity-only 保留，以及 NativeAOT、trimmed CoreCLR、Browser 裁剪运行和 Browser AOT。 |
| 修改主题视觉 | 验证 Light/Dark、Control 算法和相关 Control 家族。 |
| 修改控件代码里的视觉计算 | 验证主题变化后重算、局部 ThemeConfigProvider 覆盖生效、用户本地值与应用样式不被覆盖。 |

生效测试的定义、骨架生成和阈值见[主题生效测试规范](theme-effect-tests.md)。

涉及 identity 合并时覆盖 raw→typed、typed→raw、正确 typed→错误 typed 及反序，确认 owner 校验不被 Dictionary 去重吞掉。
涉及资源时同时验证共同资产优先级与未用资产删除；独立原型通过不能替代真实产品宿主验收。
