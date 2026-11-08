# AtomUI 多设计语言主题架构

日期：2026-11-07。状态：设计方案，待审阅；未开始实现。
基线：AtomUI `release/6.0`（`ae5ac0a32`，Avalonia 12.1.5）。参照：Ant Design 6.7.0。

## 1. 目标与边界

AtomUI 应用在运行时切换设计语言，效果对齐 ant.design 首页的预设切换器。首批由 AtomUI 提供并做精细的语言是 **Ant Design、shadcn、PrimeNG** 三种；其他语言（Material、Bootstrap 等）后续再说，机制对它们同样成立。要求：

| # | 要求 |
| --- | --- |
| R1 | 同一棵控件树切换，不重建，控件属性上的状态（文本、选中、焦点）不丢 |
| R2 | 子树可以用与根不同的设计语言 |
| R3 | 控件代码只有一份；各设计语言的共性控件大同小异，大同是公开 API 与状态语义 |
| R4 | 设计语言是独立包：不引用不占产物，引用未激活只登记元数据，激活后只物化用到的控件主题 |
| R5 | 第三方能为任一语言提供自己控件的主题，也能提供新的语言，协议与 AtomUI 自己的语言相同 |
| R6 | 切换成本与今天的主题切换同阶；无逐帧成本；内存有上界 |
| R7 | 只用 Avalonia 公开 API，不 hook 私有成员，不修改 Avalonia |
| R8 | 对 release/6.0 的重构只许三类：简化代码、提升性能、架构优化 |
| R9 | 机制与平台无关，桌面与移动控件包共用 |

边界：精确到"控件公开 API 与状态语义"这条线以内；一个设计系统独有的组件（PrimeNG 的 Dock、shadcn 的 Command Palette）不属于主题，另立控件包。不承诺像素级复刻任何设计系统。

## 2. 核心概念

```text
设计语言（Design language）        决定控件长什么样、怎么反馈、Token 怎么派生
  └── 主题定义（Theme definition）  语言之下的色板与微调，不碰 ControlTheme
        └── 运行时配置（ThemeConfig） 应用与局部作用域的覆盖
```

| 概念 | 内容 | 举例 |
| --- | --- | --- |
| 设计语言 | 一套 ControlTheme；Token 派生策略（算法集、Alias 计算）；语言基础 Token；逐控件静态覆盖；逐控件 Own Token 求值器；默认主题；回落目标；少数行为实现（反馈策略、契约部件） | AntDesign、Shadcn、PrimeNG |
| 主题定义 | 一组 Token 值，以色板为主，可按变体（Dark 等）分块给值；属于某个语言，或是通用的（任何语言下可用） | AntDesign 下的 DaybreakBlue、DustRed；Shadcn 下的 Zinc、Slate、Stone（各含明暗两套色值）；PrimeNG 下的 Aura、Lara、Nora；用户目录里的通用色板 |
| 变体 | Default / Dark / Compact 等 id；每个语言声明自己支持哪些、各自怎么算 | Shadcn 与 PrimeNG 的 Dark 是一张值表而非算法；PrimeNG 可以不支持 Compact |
| 运行时配置 | Token、逐控件 Token、算法、是否继承、局部切语言 | 应用级调色；某个面板换成 Shadcn |

全局 Token 的词汇表（Seed/Map/Alias 的字段名）留在 Core，是所有语言的 ControlTheme 共同消费的契约；语言拥有的是值与派生。

## 3. 整体架构

```text
┌─ 应用 ──────────────────────────────────────────────────────────────┐
│ ThemeRequest(语言, 主题?, 配置)   ThemeConfigProvider(局部语言/配置)   │
└───────────────┬─────────────────────────────────────────────────────┘
┌─ 语言包（每语言一个程序集）────────────────────────────────────────────┐
│ 定义 · 算法/Alias 计算器 · 主题定义 XML · ControlTheme · Own 求值器     │
│ 基元 · 行为策略 · 契约部件实现 · FIDELITY.md                            │
└───────────────┬─────────────────────────────────────────────────────┘
┌─ 控件包 ──────┼─────────────────────────────────────────────────────┐
│ lookless 控件代码 · [TemplatePart]/[PseudoClasses] · Semantic Part    │
│ Own Token 字段 · 带语言标签（AntDesign）的 ControlTheme 与求值器        │
└───────────────┬─────────────────────────────────────────────────────┘
┌─ Core ────────┴─────────────────────────────────────────────────────┐
│ Token 词汇表 · 派生管线 · 合并器 · 事务 · 作用域 · 注册表 · 校验          │
│ 每作用域一个 ResourceProvider：回答 Token 键与 ControlTheme 键           │
└─────────────────────────────────────────────────────────────────────┘
```

Core 不认识任何一种设计语言。Ant Design 是默认语言，id 为 `AntDesign`：它的定义、算法、色板在小包 `AtomUI.Themes.AntDesign`，它的 ControlTheme 与 Own 求值器随控件包交付并打 `AntDesign` 标签；注册、校验、回落与其他语言完全相同。`AtomUI.Controls` 引用该小包，`UseAtomUI()` 开箱即是 Ant Design。

## 4. Token 层

### 4.1 合并顺序

```text
库默认（词汇表的类型默认值）
→ 语言基础 Token
→ 语言逐控件静态覆盖（Controls：某控件在该语言下固定的 Token 值）
→ 主题定义 Token（语言专属或通用；含当前变体的分块值）
→ 根 ThemeRequest.Config
→ 局部 ThemeConfigProvider.Config（沿父链继承；Inherit=false 回到"语言 + 语言默认主题"）
```

合并产出每个 Token 的显式值，按 Seed / Map / Alias / Own 阶段注入派生管线；这是现有 `ThemeCompiler` 的顺序，不变。"静态覆盖"是值，"Own 求值器"（4.2）是函数，两者不同：前者参与合并，后者在派生阶段为没有显式值的 Own Token 算默认。

### 4.2 派生策略归语言

```text
seed  = 库默认 + 显式 Seed 值
map   = 语言.算法[请求变体…](seed) + 显式 Map 值        // 链式：每个算法接收 seed 与前一算法结果
alias = 语言.Alias计算(map) + 显式 Alias 值
缺口  = 沿回落链补齐（§7）
own   = 语言.Own求值器[控件](alias) + 显式 Own 值         // 求值器沿回落链取
```

- 变体 id 是开放字符串，`Default/Dark/Compact` 是约定常量。请求与控件级自定义算法的 id 在当前语言中解析，不支持 → 失败并诊断；UI 据 `SupportedAlgorithms` 禁用开关。
- 外观（Light/Dark）由算法描述符声明，与今天一致。
- 派生式语言（Ant Design）提供真算法：Seed 经调色板算法生成 Map，Alias 由公式生成。
- 直接指定式语言（shadcn、PrimeNG）不派生：shadcn 的语义变量（background、foreground、primary、muted、accent、destructive、border、ring、radius…）和 PrimeNG 的 primitive / semantic / component 三层 Token 都是直接给值，明暗各一套。它们用 Core 提供的值表算法：语言级的值表给与色板无关的值（圆角、线宽、阴影、字体），Dark 是另一张表并声明外观为 Dark；色板属于预设，所以 PrimeNG 的 Aura、Lara、Nora 与 shadcn 的 Zinc、Slate、Stone 是各自语言下的专属主题定义，**主题定义按变体分块给值**（默认块 + `Dark` 块），编译时把当前变体链命中的块叠加进显式值。主题 XML 为此增加可选的 `<Variant Id="Dark">` 子块；Ant Design 的主题不需要它。
- 缺口填充的语义：语言没给出的 Map/Alias 沿回落链用回落语言的派生结果补齐，而回落语言是用**本语言的有效 Seed** 派生的。因此值表语言必须同时给出 Seed（`ColorPrimary`、`ColorBgBase` 等），否则补齐出来的是回落语言默认色系。启动校验报告每个语言被补齐的 Token 清单，语言作者据此决定是补值还是接受。
- 第三方只想换几个值的最小语言可以声明派生策略为"继承"，全部沿回落链取。
- 语言扩展全局 Token（PrimeNG 的 50 到 950 十一阶 primitive 调色板、shadcn 的 ring / muted / accent 在 AtomUI 词汇里没有一一对应）：设计保留，不在首批；首批先映射到现有词汇并在 `FIDELITY.md` 记录有损项，证据充分再做。

## 5. 视觉层：ControlTheme 随作用域切换

控件的 ControlTheme 不再靠挂树时的隐式资源查找，而由一条样式显式绑定：

```text
对每个已注册控件类型 T：
  Style OfType<T> { Setter(StyledElement.Theme, DynamicResource(ControlThemeKey(T))) }
```

每个作用域已有的 Token ResourceProvider 多回答一种键：`ControlThemeKey(T)` → 当前作用域语言下 `T` 的 ControlTheme（沿回落链解析后的结果，惰性物化）。

因此：

- 切换语言 = 一次普通主题事务。Publish 阶段的资源变化通知同时驱动 Token 绑定与 Theme 绑定重新取值；只有主题实例真的变化的控件重套模板，只换 Token 的语言零重套。
- 局部 `ThemeConfigProvider`、Popup、带 owner 的独立窗口全部沿现有资源链取到各自作用域的主题，不加任何新的传播机制。
- 用户在实例上显式设置的 `Theme` 本地值优先级更高，不被语言切换改写。
- 控件包不再以类型为键导出 ControlTheme 资源，所以没有隐式主题可回落。没有注册任何语言时键不命中，`Theme` 为 null，控件无模板、无外观，与未引用 Fluent 的 Avalonia 应用一致；启动记一条诊断，不抛。第三方代码里 `{StaticResource {x:Type atom:Button}}` 这类引用改用 `FallbackControlTheme`，属破坏性变更。
- 组合控件的部件主题（`SearchEdit.SearchButtonTheme`、`Calendar.PanelTheme`、`DatePanel.CellTheme` 等）由 owner 的 ControlTheme 以 Setter 提供默认，模板内用 `TemplateBinding` 传给子控件；owner 的主题随语言切换，部件主题随之切换，不需要额外机制。命名的非类型键 ControlTheme（现有 9 个）随所在语言包的资源走。

## 6. 行为层：少数分歧的吸收点

共性控件的分歧绝大多数停在 Token 与 ControlTheme；剩下两类用两种机制，只用于真实分歧点：

| 分歧 | 机制 | 例子 |
| --- | --- | --- |
| 部件行为不同 | **契约部件**：`[TemplatePart("PART_Popup", typeof(ISelectPopupHost))]`，控件只经接口与部件对话，部件实现随语言包交付、由该语言的模板放入；切换语言即换部件 | shadcn 的 Select 弹层是带键盘导航的 Command 式列表；PrimeNG 的下拉带内置过滤框 |
| 反馈不同 | **行为策略**：控件暴露 `StyledProperty<IFeedbackBehavior?>`，语言主题用 Setter 注入；无状态策略单例，有状态的随控件创建与释放；控件不内置任何默认 | Ant Design 的 wave 外扩；PrimeNG 从点击点扩散的 ripple；shadcn 无点击动效、只有焦点环 |

差异必须改变控件公开状态机时，才给控件加一个所有语言都成立的属性，由语言主题设默认值；这是最后手段。处理顺序固定：Token → ControlTheme → 基元或策略 → 契约部件 → 控件开关 → 记入 `FIDELITY.md`。控件代码不得读取语言 id、不得按语言分支。

## 7. 回落与完整性

语言定义声明 `Fallback`。本语言没定义的沿链取：

| 缺 | 回落到 |
| --- | --- |
| 某控件的 ControlTheme | 回落语言的该控件 ControlTheme |
| 某控件的 Own Token 求值器 | 回落语言的求值器 |
| 派生后仍未指定的 Map/Alias | 回落语言的派生结果 |
| 某变体 id 的算法 | 不回落；不支持即失败 |

注册期校验：链无环；回落目标已注册；链末端语言（AntDesign）对每个已注册控件都有 ControlTheme 与求值器、派生后无未指定 Token；同一 `(语言, 控件)` 不重复注册；语言的默认主题可解析。任一不满足启动失败并列出全部缺口。回落链在注册期解析成 `(语言, 控件) → 最终工厂` 表，运行时查表。

## 8. 控件标准：lookless

控件与 Avalonia 自身控件同一标准：控件类 = 属性 + 行为 + 对命名部件的接线；模板、样式、每一个视觉值由主题提供。

| 规则 | 要求 |
| --- | --- |
| 视觉值不进代码 | 不得出现画刷、颜色、阴影、圆角、厚度、字号、尺寸的字面量或由 Token 计算的值；不得命令式绑定 Token；自绘控件把画刷、笔、几何参数暴露为属性，`Render` 只读取 |
| 状态与变量可以，计算不行 | 离散状态（`EffectiveVariant`、`EffectiveIsDanger`）可作为内部属性或伪类供选择器使用；视觉类型的内部属性（`VariantShadow`、`EffectiveCornerRadius`）可作为模板变量存在，但值只能由主题 Setter 按状态赋予 |
| 模板契约显式、最小、按需契约化 | `[TemplatePart(Name, ContractType, IsRequired)]` 列全依赖；只有真实分歧点用接口做契约；`[PseudoClasses]` 列全；`OnApplyTemplate` 只经 `NameScope` 取部件，可选部件为空时降级不抛；不搜索可视树 |
| 行为经策略注入 | `StyledProperty<IXxxBehavior?>` 默认 null，不内置默认 |
| 几何从属性推导 | 不写常量 |
| 没有主题也能运行 | 无模板下 `Measure/Arrange` 零尺寸，事件与命令照常；这是验收用例 |

现状基线（`AtomUI.Controls` + `AtomUI.Desktop.Controls`，119 个控件）：111 处代码赋视觉属性，16 处命令式 Token 绑定，38 个自绘，`[TemplatePart]`/`[PseudoClasses]` 各 47/60 处，C# 构建主题 0 个。分析器先对存量警告，按语言覆盖的控件家族逐批清零转错误；不做全量前置。

公开 API 清理：控件上带有 Ant Design 专属语义的公开属性要随 lookless 一起处理。`IsWaveSpiritEnabled`（8 个控件）描述的是 Ant 的 wave 反馈，改为 `Feedback` 策略属性，wave 的形状参数成为 `WaveFeedback` 自己的附加属性；`IsMotionEnabled`（110 处）是与语言无关的通用开关，保留。重构每个家族时列出该家族的此类属性并逐个裁决。

Semantic marker 是主题的义务：每种语言的 ControlTheme 保持同一组非 Optional marker，用户写的 Semantic Part Style 对任何语言都生效。

### 8.1 重构后一个控件长什么样

以 Button 为例。控件目录保持：

```text
Buttons/
+-- Button.cs                    行为、契约、公开属性、状态解析、部件接线
+-- Button.SemanticParts.cs      [SemanticPart] 声明（不变）
+-- ButtonToken.cs               Own Token 字段 + 参考求值器（生成器打 AntDesign 标签）
\-- Themes/ButtonTheme.axaml     Ant Design 的 ControlTheme（打 AntDesign 标签）
```

`Button.cs` 只剩五类成员：

```csharp
[TemplatePart(PartContent, typeof(ContentPresenter), IsRequired = true)]
[TemplatePart(PartIcon, typeof(IconPresenter))]                 // 可选，缺失时不显示图标
[TemplatePart(PartLoadingIcon, typeof(Control))]
[PseudoClasses(":loading", ":icon-only", ":danger", ":ghost", ":block", ":icon-end")]
public partial class Button : AvaloniaButton, ICustomizableSizeTypeAware
{
    // 1. 公开 API：只表达语义，不表达外观
    public static readonly StyledProperty<ButtonType>      ButtonTypeProperty    = …;
    public static readonly StyledProperty<ButtonColorType> ColorTypeProperty     = …;
    public static readonly StyledProperty<ButtonVariant>   VariantProperty       = …;
    public static readonly StyledProperty<ButtonShape>     ShapeProperty         = …;
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty = …;
    public static readonly StyledProperty<bool>            IsDangerProperty, IsGhostProperty, IsLoadingProperty, IsBlockProperty;
    public static readonly StyledProperty<Icon?>           IconProperty;
    public static readonly StyledProperty<IconPlacement>   IconPlacementProperty;

    // 2. 解析后的离散状态：由公开输入算出，供主题选择器使用；internal
    internal static readonly StyledProperty<ButtonColor>   EffectiveColorProperty   = …;   // Primary / Default / Danger …
    internal static readonly StyledProperty<ButtonVariant> EffectiveVariantProperty = …;   // Solid / Outlined / Dashed / Filled / Text / Link
    internal static readonly StyledProperty<bool>          EffectiveIsBorderedProperty = …;

    // 3. 模板变量：视觉类型，值只由主题 Setter 赋予，代码不计算、不给默认
    public static readonly StyledProperty<CornerRadius>    EffectiveCornerRadiusProperty    = …;
    public static readonly StyledProperty<Thickness>       EffectiveBorderThicknessProperty = …;
    public static readonly StyledProperty<BoxShadows>      VariantShadowProperty            = …;

    // 4. 行为策略：默认 null，由语言主题注入
    public static readonly StyledProperty<IFeedbackBehavior?> FeedbackProperty = …;

    // 5. 部件接线与状态解析
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _content = e.NameScope.Get<ContentPresenter>(PartContent);
        _icon    = e.NameScope.Find<IconPresenter>(PartIcon);          // 可选部件为空时降级
        UpdateEffectiveState();
    }

    private void UpdateEffectiveState()                                  // 只算状态，不算任何视觉值
    {
        (EffectiveColor, EffectiveVariant) = ResolveColorVariant(ButtonType, ColorType, Variant, IsDanger, IsGhost);
        EffectiveIsBordered = EffectiveVariant is not (ButtonVariant.Text or ButtonVariant.Link);
        PseudoClasses.Set(":loading", IsLoading);
        PseudoClasses.Set(":icon-only", Icon is not null && Content is null);
        PseudoClasses.Set(":icon-end", IconPlacement == IconPlacement.End);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Feedback?.OnPressed(this, e.GetPosition(this));                 // 策略为空即无反馈
    }
}
```

不再出现在控件代码里的东西：从 Token 算 `VariantShadow`、按 `SizeType` 算 `EffectiveCornerRadius`、`TokenResourceBinder.Create*` 绑定、wave 装饰器的创建与驱动、任何 `Background = …` / `Foreground = …` 赋值、按语言或主题分支。

它们去了主题。Ant Design 的 `ButtonTheme.axaml` 给模板变量按状态赋值：

```xml
<ControlTheme x:Key="{x:Type atom:Button}" TargetType="atom:Button">
    <Setter Property="Feedback" Value="{x:Static antdesign:WaveFeedback.Instance}" />
    <Setter Property="EffectiveCornerRadius" Value="{atom:ButtonTokenResource BorderRadius}" />
    <Style Selector="^[SizeType=Large]">
        <Setter Property="EffectiveCornerRadius" Value="{atom:ButtonTokenResource BorderRadiusLG}" />
    </Style>
    <Style Selector="^[EffectiveVariant=Solid][EffectiveColor=Primary]">
        <Setter Property="VariantShadow" Value="{atom:ButtonTokenResource PrimaryShadow}" />
    </Style>
    <Setter Property="Template">…模板只消费 EffectiveCornerRadius / VariantShadow / Foreground 等变量，节点带 Semantic marker…</Setter>
</ControlTheme>
```

shadcn 的 `ButtonTheme.axaml` 可以 `BasedOn="{atom:FallbackControlTheme atom:Button}"` 只改变量与策略（`Feedback` 换成焦点环、阴影换 xs、圆角换 shadcn 值），也可以整个换模板；控件代码一行不动。

`ButtonToken.cs` 保持"字段 + 参考求值器"：

```csharp
[ControlDesignToken]
internal sealed class ButtonToken : AbstractControlDesignToken
{
    public BoxShadows PrimaryShadow { get; set; }
    public Color DefaultBg { get; set; }
    // …字段是契约，所有语言共享
    public override void CalculateTokenValues(bool isDarkMode)        // 参考默认，生成器登记为 AntDesign 的求值器
    {
        PrimaryShadow = new BoxShadows(BoxShadow.Parse($"0 {SharedToken.ControlOutlineWidth} 0 {SharedToken.ControlOutline}"));
        DefaultBg     = SharedToken.ColorBgContainer;
    }
}
```

其他语言为 Button 提供自己的求值器（放在语言包 `TokenDefaults/`），或不提供而沿回落链用 Ant Design 的。

## 9. 语言包

### 9.1 构成

```text
AtomUI.Themes.AntDesign/                      小包，只依赖 Core：默认语言的定义与派生
+-- AntDesignLanguage.cs                      Id=AntDesign、显示名、默认主题 DaybreakBlue、派生策略、无 Fallback
+-- Algorithms/                               Default/Dark/Compact 三个计算器、调色板、Alias 计算器（自 Core 搬入）
\-- Definitions/DaybreakBlue.theme.xml …      专属主题定义（自 Core 搬入）
                                              ControlTheme、Own 求值器、wave 基元与 WaveFeedback 策略留在控件包，打 AntDesign 标签
                                              （它们依赖控件类型，不能放进只依赖 Core 的小包；依赖方向：控件包 → 小包 → Core）

AtomUI.Themes.Shadcn/                         <AtomUIDesignLanguage>Shadcn</AtomUIDesignLanguage>，Fallback=AntDesign
+-- ShadcnDesignLanguage.cs                   派生策略 = 值表（Light/Dark 两张），基础 Token：圆角、线宽、字体、焦点环
+-- Definitions/Zinc.theme.xml、Slate、Stone…  专属主题定义，DesignLanguage="Shadcn"
+-- Themes/Buttons/ButtonTheme.axaml …         shadcn 形态的控件：细边框、xs 阴影、焦点环、无动效
+-- TokenDefaults/                            需要改默认的控件的 Own 求值器
+-- Behaviors/ Parts/                         焦点环策略；Command 式弹层部件
\-- FIDELITY.md                               逐控件与 shadcn 参考渲染的对照结论、未覆盖项

AtomUI.Themes.PrimeNG/                        <AtomUIDesignLanguage>PrimeNG</AtomUIDesignLanguage>，Fallback=AntDesign
+-- PrimeNGDesignLanguage.cs                  派生策略 = 值表（primitive/semantic 两层，Light/Dark），基础 Token
+-- Definitions/Aura.theme.xml、Lara、Nora      专属主题定义，DesignLanguage="PrimeNG"
+-- Themes/…                                  PrimeNG 形态的控件
+-- TokenDefaults/ Primitives/ Behaviors/ Parts/  求值器；ripple 基元与策略；带过滤框的下拉部件
\-- FIDELITY.md
```

控件包（`<AtomUIThemesLanguage>AntDesign</AtomUIThemesLanguage>`）继续在各控件的 `Themes/` 放 ControlTheme、在 `XxxToken.cs` 放求值器，并可在 `Behaviors/`、`Primitives/` 放该语言的策略与基元，生成器给它们打 `AntDesign` 标签；控件包不得含语言定义、计算器、主题定义。策略与部件的契约接口（`IFeedbackBehavior`、`ISelectPopupHost` 等）与控件同在控件包或 Core，语言包只提供实现。第三方控件包用同一属性声明自带主题属于哪个语言，为别的语言另建主题程序集。可选控件包（DataGrid、ColorPicker）的 Ant Design 主题随包；其他语言按需 `AtomUI.Themes.<Lang>.<Pkg>`。

### 9.2 生成器

语言包：登记定义、`Themes/**` ControlTheme、`[ThemeAlgorithm]` 与 `[ThemeAliasCalculator]`、`[ControlTokenDefaults]`、`Definitions/*.theme.xml`（强制 `DesignLanguage` 等于包语言）。控件包：登记控件，主题与求值器打标签。两类都产出 `GeneratedDesignLanguageRegistration.Register(builder)`。

构建期校验：类型键 ControlTheme 的 `TargetType` 必须是已注册 Control 且每程序集唯一；非 Optional Semantic marker 齐全；`FallbackControlTheme` 的类型与 `TargetType` 一致且所在语言声明了 `Fallback`；控件代码不得含视觉字面量、命令式 Token 绑定、计算型视觉属性、读取语言 id、内置默认策略或部件实现。

### 9.3 按需

不引用：不进产物。引用未激活：只登记定义、主题定义、`(语言, 类型) → 工厂` 与 `(语言, 控件) → 求值器` 两张表，AXAML 不解析。激活：只物化被查询过的类型；切回后实例留在惰性 provider，上界 = 已激活语言数 × 出现过的类型数。运行期追加：语言包不新增 Token schema 与控件 identity，可在 UI 线程追加并重跑校验；JIT 可配合 `AssemblyLoadContext`，NativeAOT 无运行期装配加载。

## 10. 运行时

### 10.1 组合规则

- 根请求给语言，主题 id 可省略 = 该语言默认主题。只注册一个语言时它是初始语言，多个时必须指定。
- 专属主题跨语言请求 → 失败。只换语言时：通用主题沿用，专属主题换成新语言默认；结果写进 `ThemeState`。
- 局部作用域指定语言未指定主题套同一规则；Popup、独立窗口沿资源链继承。
- 用户目录主题可声明 `DesignLanguage`；声明未注册的语言 → 目录诊断、不可选。用户目录不得定义语言。

### 10.2 事务

沿用 Capture → Prepare → CommitCore → Publish → Complete，不加阶段：Capture 解析语言、主题、变体；Prepare 合并派生出 Snapshot（缓存键含语言、派生计划、变体链；不物化 ControlTheme）；CommitCore 换引用；Publish 发资源变化，Token 与 Theme 绑定重新取值，首次命中的 ControlTheme 在此物化；Complete 记录实际语言与主题。

### 10.3 注册表

`DesignLanguageRegistry`：定义表、`(语言, 类型) → 惰性 ControlTheme provider`、`(语言, 控件) → 求值器`、按语言分组的主题目录。启动时做 §7 的校验并解析回落表。

## 11. 公开 API

```csharp
public readonly record struct ThemeAlgorithm(string Id)
{ public static readonly ThemeAlgorithm Default = new("Default"), Dark = new("Dark"), Compact = new("Compact"); }

public interface IThemeAliasCalculator { void Calculate(DesignToken map); }

public sealed class ThemeDerivation
{
    public static ThemeDerivation Create(IThemeAliasCalculator aliasCalculator, params ThemeAlgorithmDescriptor[] algorithms);
    public static ThemeDerivation Tables(ThemeTokenTable @default, ThemeTokenTable? dark = null, ThemeTokenTable? compact = null);
    public static ThemeDerivation Inherit { get; }
    public ThemeDerivation WithAlgorithm(ThemeAlgorithmDescriptor descriptor);
}

public sealed class DesignLanguageDefinition
{
    public DesignLanguageDefinition(string id, string displayName, string defaultThemeId,
        ThemeDerivation derivation, string? fallback = null, ThemeConfig? tokens = null,
        IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig>? controls = null);
}

public sealed class DesignLanguageThemeSet          // 一个程序集为某语言贡献的 ControlTheme 与求值器，生成器产出
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

主题 XML 增加可选根属性 `DesignLanguage`（省略 = 通用）与可选的 `<Variant Id="Dark">` 子块（该变体命中时叠加的 Token 值）；`IsDefault` 改由语言定义的默认主题决定。AXAML 增加 `{atom:FallbackControlTheme atom:Button}` 标记扩展。

## 12. 性能与内存

| 项 | 成本 | 说明 |
| --- | --- | --- |
| 启动多挂的按类型样式 | 约 380 条 `OfType`，每条一个 Setter | 类型相等比较；隐式 ControlTheme 查找同时取消，净增接近零。实测可见则改为一条样式加按 `StyleKey` 算键的自定义绑定 |
| 每控件一个 Theme 订阅 | 1 / 控件 | 控件已有数十个 Token 订阅 |
| 切换语言 | 一次事务 + 变化类型重套模板 | 只换 Token 零重套 |
| 首次激活语言 | 物化被查询过的 ControlTheme | 惰性，上界有限 |
| 契约部件、策略 | 模板实例化与事件路径 | 无逐帧成本 |
| 未激活语言包 | 定义与两张表 | AXAML 不解析 |

目标：Gallery 预览页（约 60 个控件）切换 Publish 在 16 ms 内，首次激活含物化不超过 50 ms，测试断言。

## 13. 可行性依据与待验证项

**已核对（Avalonia 12.1.5 源码）**：`Theme` 是普通 StyledProperty，样式 Setter 可设置（Setter 只拒绝 Direct 属性）；`Theme` 变化时移除主题帧并在下次套样式时重新应用；DynamicResource 在目标控件处沿其资源链解析（AtomUI 局部作用域 Token 今天正依赖这一点）；选择器从左到右求值、类型不匹配即短路；运行时向样式宿主追加样式会使已挂载后代重新套样式；以类型为键的隐式 ControlTheme 只在挂入逻辑树时解析。

**已核对（AtomUI release/6.0）**：派生管线顺序；每作用域一个 ResourceProvider；惰性 ControlTheme provider；Semantic Part 生成；控件 lookless 基线数字。

**待 spike（半天，任一不成立停下修订）**：
1. **首要风险**：样式 Setter 设置 `Theme` 是否在首次套样式时即生效、改变后是否实时重套模板。Avalonia 在一次 `ApplyStyling` 里先应用 ControlTheme 再应用 Styles；样式给出的 `Theme` 值到达时主题已应用过一次，依赖随后的 `OnControlThemeChanged` 重置标志并由下一次套样式（挂树、测量前）补应用。要验证首帧无闪烁、`PART_*` 查找正常、切换实时。若不成立，备选机制已定：每个控件类型在静态构造中登记一个 `AttachedToLogicalTree` 类处理器，用公开的 `GetResourceObservable(ControlThemeKey)` 把 `Theme` 以样式优先级绑定到作用域资源；语义不变，只是绑定由代码而非样式安装。
2. DynamicResource 不命中时 `Theme` 回到 null，控件无模板且不抛。
3. 作用域 Provider 的资源变化通知能驱动 Popup 内与独立窗口内控件的 Theme 绑定。
顺带测量约 380 条样式对首页套样式耗时、Select 换契约部件前后的模板应用耗时。

**否决的替代方案**：
- 按语言增删样式打补丁：Avalonia 移除样式时模板节点残留，需私有 hook；Ant 成为被打补丁的特例。
- 可继承附加属性 + 属性选择器激活：能切 Setter，不能换模板；多一条传播机制。
- 按语言派生控件类型：用户 XAML 写的是 `atom:Button`，运行时换不了类型。
- 换以类型为键的 ControlTheme 资源：要重挂载，违反 R1。
- 隐式回落 Ant Design：Core 认识某个语言，违反 R5。
- 物理拆出 Ant Design 全部主题：回落链末端永远是它，拆了不省体积，多数百文件迁移与一倍包数。
- 全量前置 lookless 重构：只换值的语言不需要，第一个可见成果推迟数月。

## 14. 实施路线

| 步 | 内容 | 验收 |
| --- | --- | --- |
| 1 | Spike（§13 三项与两项计时） | 结论记录 |
| 2 | 语言协议与 Ant Design 小包：生成器两类程序集属性；控件包主题与求值器原地打标签；Core 的算法、调色板、Alias、DaybreakBlue 搬入 `AtomUI.Themes.AntDesign` | 只有一个语言，运行时行为不变，现有测试全过 |
| 3 | 分析器五条诊断，存量警告，产出清单 | 与 §8 基线一致 |
| 4 | Core 派生层：开放变体 id、Alias 计算器接口、派生策略、值表算法、按语言登记 | Ant Design 回归不变 |
| 5 | Core 语言层：注册表、回落解析与校验、请求/目录/状态/XML 属性、合并层、ControlTheme 键与 Provider、启动样式、`FallbackControlTheme`、运行期追加；控件包不再导出类型键资源 | §15 单元测试 |
| 5a | lookless 重构：预览集合涉及的控件家族先做，警告清零转错误，含契约化与策略化 | 无语言宿主行为测试 + Ant Design 外观不变 |
| 6 | 语言包 shadcn 与 PrimeNG，先覆盖预览集合：Button、Input 家族、Select、Checkbox/Radio/Switch、Steps、Progress、Tag、Card、Alert、Dialog、Notification；各自三套专属主题定义与明暗值表 | §15 集成测试；两套语言的 `FIDELITY.md` 对预览集合逐控件给出结论 |
| 7 | Gallery：顶栏先选语言再选主题；"设计语言预览"页复刻 ant.design 预览区，一个 Provider 承载，同页双 Provider 对照，"复制主题代码"导出 C# 与 XML | 切换效果可见 |
| 8 | shadcn 与 PrimeNG 扩展到其余控件与可选包；`FIDELITY.md` 列词汇限制 | 持续 |
| 9 | 语言扩展 Token，仅当第 8 步证明损失不可接受 | 按证据 |
| 后续 | Material、Bootstrap 等其他语言，按同一协议另立包 | 不在本方案范围 |

量级（诚实估计）：1 到 5 步机制部分数周。第 6 步是真正的大头：两种语言各覆盖 12 个控件家族并做精细，每个家族含 lookless 重构、ControlTheme、Own 求值器、明暗值表、保真度对照，按每语言每家族数天计，两种语言合计以季度为单位；可以按家族分批可见。第 8 步长期。

## 15. 验证

单元：合并顺序逐对；回落链三类与校验；派生式、值表式、不支持变体；只注册 Ant Design 时现有测试不变；组合与回退规则；ControlTheme 键命中、回落、物化一次、A→B→A 实例数不变；根、局部 Provider、Popup、独立窗口四种宿主取到各自语言；运行期追加；WeakReference 回收；无语言宿主下控件可构造与运行。

集成：每语言 × 预览页关键区域按容差策略对比语言包自己的基准；切换前后 TextBox 文本与选区、ListBox 选中、焦点不变；每语言主题的 Semantic Part 预览解析全部非 Optional 部件；用户显式 `Theme` 不被改写；Select 换契约部件后同一套行为用例在两种语言下通过。

性能：§12 两项计时断言。

## 16. 对 release/6.0 的改动登记

| 改动 | 类别 |
| --- | --- |
| Core 的三个计算器、调色板、Alias 公式、DaybreakBlue 搬入 `AtomUI.Themes.AntDesign`；控件包主题与求值器原地打标签 | 架构优化 |
| Alias 公式抽为 `IThemeAliasCalculator`，Token 定义类只剩数据 | 架构优化 |
| 变体枚举改开放 id，算法按语言登记 | 新能力 + 架构优化 |
| 删除 `DEFAULT_THEME_ID`，初始主题改为语言优先 | 新能力 |
| 请求、目录、状态携带语言；XML 增加 `DesignLanguage`，`IsDefault` 改由语言决定 | 新能力 |
| `ThemeConfig.DesignLanguage`；合并器加两层 | 新能力 |
| Provider 回答 ControlTheme 键；启动挂按类型 Theme 样式；控件包不再导出类型键资源 | 新能力 |
| 注册表、定义/主题集类型、回落解析与校验、`FallbackControlTheme` | 新能力 |
| 控件 lookless 重构（分家族） | 架构优化 |
| 根 Context 样式只作用于无逻辑父级的 TopLevel（现样式会覆盖 PopupRoot 从 Popup 继承的局部 Context） | 架构优化，可独立提交 |

不改动：事务结果枚举、Own Token 字段与资源 ID 合同、资源扩展、Semantic Part 生成、`ThemeContextLease`、编译主题包 ABI、用户主题目录安全边界。不引入别名、兼容层或过渡 API。
