# 主题算法策略化迁移指南（未发布）

[English](unreleased-theme-algorithms-api-changes.md)

本文说明在 6.2.14 之上主题算法的变更。主题算法现在是可插拔的策略：算法身份从封闭的枚举改为开放的
`(Catalog, Id)`，主题包和应用可以注册自己的算法，一个算法除了 Map Token 还可以计算 Alias Token 和逐控件的 Token。
`AtomUI.Themes.PrimeNG` 是第一个这样做的包。发布版本号尚未确定，发布时由发布负责人把本文并入
`docs/releases/<version>-api-changes.zh-CN.md`。版本要点见[更新日志](../../CHANGELOG.zh-CN.md)，算法作者指南见
[编写主题算法](../guides/theming/writing-theme-algorithms.md)。

## 速查

| 范围 | 6.2.14 | 现在 / 迁移 |
| --- | --- | --- |
| `ThemeAlgorithm` | `enum`（`Default = 0`、`Dark = 1`、`Compact = 2`） | 只读结构体身份 `(Catalog, Id)`；`ThemeAlgorithm.Default` / `Dark` / `Compact` 名称不变。二进制不兼容；`switch`/`case`、`(int)` 转换、`const`、`Enum.*`、把 `default(ThemeAlgorithm)` 当作 Default 都不再可用；用 `==` 比较 |
| `ThemeAlgorithmAttribute` | `(ThemeAlgorithm algorithm, int revision, ThemeAppearanceEffect appearanceEffect)` | `(string catalog, string id, int revision, ThemeAppearanceEffect appearanceEffect)` |
| `ThemeAlgorithmDescriptor` | 构造参数与 `Algorithm` 属性为枚举 | 形状不变，类型为结构体；拒绝 `default(ThemeAlgorithm)` |
| `DefaultThemeVariantCalculator.Algorithm`、`DarkThemeVariantCalculator.Algorithm`、`CompactThemeVariantCalculator.Algorithm` | `public const ThemeAlgorithm` | 已删除；改用 `ThemeAlgorithm.Default` / `Dark` / `Compact` |
| `ThemeAlgorithmTypeConverter`（AXAML） | `DefaultAlgorithm`、`DarkAlgorithm`、`CompactAlgorithm` | `Default`、`Dark`、`Compact` 或 `Catalog:Id`；`...Algorithm` 写法被拒绝 |
| `IThemeManagerBuilder` | — | 新增 `AddThemeAlgorithms`、`AddControlTokenAlgorithms`；自己实现该接口的代码要补上 |
| 新的算法 API | — | `IThemeAliasAlgorithm`、`IControlTokenAlgorithm<TToken>`、`ControlTokenAlgorithmAttribute`、`ThemeAlgorithmContext`、`ThemeAlgorithmContractException`、`ThemeAlgorithmContractViolationKind`、`IControlToken`、`ControlTokenAlgorithmDescriptor`、`ThemeBuildAlgorithmAttribute`；无需操作 |
| 生成的 `<Catalog>Algorithms` | — | 包里每个算法 Catalog 一个公开静态类（例如 `PrimeNGAlgorithms.AuraDark`）；无需操作 |
| `AtomUI.Controls`、`AtomUI.Desktop.Controls`、ColorPicker、DataGrid 的控件 Token 类 | 148 个 `internal`，2 个 `public` | 全部 150 个为 `public partial`（117 个终端类、33 个抽象中间层），Token 属性为 partial 属性。公开 API 面增大；抽象中间层可被继承，其 `protected` 辅助方法属于公开 API |
| 第三方控件 Token 类 | 普通类、自动属性、`public override CalculateTokenValues` | `partial` 类、`partial` 属性（否则 `ATOMUIGEN049`）、`protected override CalculateTokenValues`（否则 CS0507） |
| `DesignToken` 的全局 Token 属性（255 个） | 自动属性 | partial 属性；调用方无变化 |
| `AbstractControlDesignToken.CalculateTokenValues` | `public virtual` | `protected virtual`；外部不能再调用 |
| `AbstractDesignToken` | — | 新增 `protected void RecordWrite(string)`，由生成的 setter 调用，不要自己调用 |
| 主题 XML `<Algorithm>` | `Id` 只能是 `Default`、`Dark`、`Compact` | 新增可选 `Catalog`（缺省 `AtomUI`）；`Id` 接受任意合法标识符。现有文件仍然有效 |
| 全局 Alias 显式值 | 在 Alias 公式之后写入，派生 Alias 不跟随 | 带动由它派生的 Alias（修复）。显式设置过 Alias Token 的主题，编译结果可能变化 |
| 编译主题输入（构建工具中间文件） | 格式版本 4，算法为整数 | 版本 6，算法为 `Catalog`/`Id`，并携带设计语言元数据；旧中间文件报「Compiled theme input format is out of date; rebuild the project.」 |
| 设计语言与品牌色 | — | 新增 `ThemeDensity`（`Normal`、`Compact`）与 `ThemeBrandColorSupport`（`Fixed`、`Seed`）两个枚举；`ThemeInfo` 新增 `init` 属性 `DesignLanguage`、`Density`、`BrandColor`、`Algorithms`（定义自身的算法链）；新增 `ThemeSelection`、`ThemeSelectionResult`、`ThemeSelectionResolver`；`CompiledThemeProgram` 新增受保护的 ABI 2 构造函数。ABI 1 编译包继续加载并取缺省值；无需处理（见下文） |
| PrimeNG 预设 | XML 值表加 Ant 派生值 | 由六个 PrimeNG 算法计算；颜色有可见变化（见下文） |

## `ThemeAlgorithm` 改为身份结构体

```csharp
public readonly struct ThemeAlgorithm : IEquatable<ThemeAlgorithm>
{
    public ThemeAlgorithm(string catalog, string id); // both validated as theme schema identifiers
    public string Catalog { get; }
    public string Id { get; }
    public static ThemeAlgorithm Default { get; } // AtomUI:Default
    public static ThemeAlgorithm Dark { get; }    // AtomUI:Dark
    public static ThemeAlgorithm Compact { get; } // AtomUI:Compact
}
```

相等性按 `Catalog`、`Id` 的序数比较；`ToString()` 返回 `Catalog:Id`（`AtomUI:Dark`，原来是 `Dark`）。

最常见的写法不用改：

```csharp
new ThemeConfigBuilder()
    .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark)
    .Build();
```

依赖枚举的代码要改写：

```csharp
// Before
switch (algorithm)
{
    case ThemeAlgorithm.Dark:
        ...
}
var code = (int)algorithm;
const ThemeAlgorithm fallback = ThemeAlgorithm.Default;

// After
if (algorithm == ThemeAlgorithm.Dark)
{
    ...
}
var key = algorithm.ToString();                       // "AtomUI:Dark"
static readonly ThemeAlgorithm Fallback = ThemeAlgorithm.Default;
```

`default(ThemeAlgorithm)` 不再表示 `Default`：它的 `Catalog` 为空。配置规范化报 `ATMTHM4004`（「Algorithm identity is uninitialized.」），
描述符构造函数抛 `ArgumentException`（「Theme algorithm identity is uninitialized.」）。持久化的整数值
无法换回身份，改为持久化 `Catalog` 与 `Id`。

针对 6.2.14 编译、用到 `ThemeAlgorithm` 的程序集（包括原来输出 `(ThemeAlgorithm)1` 的编译主题程序）必须重新构建。

## 声明算法

```csharp
// Before
[ThemeAlgorithm(ThemeAlgorithm.Dark, 1, ThemeAppearanceEffect.Dark)]

// After
[ThemeAlgorithm("Acme", "Brand.Dark", 1, ThemeAppearanceEffect.Dark)]
```

用自己的 Catalog；`AtomUI` 是三个内置算法的 Catalog。手工构造描述符的形状不变，只是改用结构体：
`new ThemeAlgorithmDescriptor(new ThemeAlgorithm("Acme", "Brand"), 1, ThemeAppearanceEffect.Light, static () => new AcmeBrandAlgorithm())`。

`DefaultThemeVariantCalculator`、`DarkThemeVariantCalculator`、`CompactThemeVariantCalculator` 的
`public const ThemeAlgorithm Algorithm` 已删除。把 `DefaultThemeVariantCalculator.Algorithm` 换成
`ThemeAlgorithm.Default`（`Dark`、`Compact` 同理）。

## AXAML 类型转换器的写法

`ThemeAlgorithmTypeConverter` 现在接受 `Default`、`Dark`、`Compact` 和 `Catalog:Id`。把 AXAML 里的
`DefaultAlgorithm`、`DarkAlgorithm`、`CompactAlgorithm` 换成 `Default`、`Dark`、`Compact`。

## `IThemeManagerBuilder` 新增两个成员

```csharp
IThemeManagerBuilder AddThemeAlgorithms(IEnumerable<ThemeAlgorithmDescriptor> algorithms);
IThemeManagerBuilder AddControlTokenAlgorithms(IEnumerable<ControlTokenAlgorithmDescriptor> algorithms);
```

生成的 `GeneratedThemeAlgorithmRegistration.Register(builder)` 调用它们。只有自己实现 `IThemeManagerBuilder` 的代码
（例如测试替身）需要补上。

## 新的算法阶段

算法现在有三个阶段。Map 阶段（`IThemeAlgorithm.Evaluate`）不变，新增两个可选阶段：

| 类型 | 用途 |
| --- | --- |
| `IThemeAliasAlgorithm : IThemeAlgorithm` | `EvaluateAlias(in ThemeAlgorithmContext, DesignToken)` 写 Alias 阶段的全局 Token，与 Map 阶段是同一个类 |
| `IControlTokenAlgorithm<TToken>` | `EvaluateGlobal` 写只对该控件生效的全局 Token，`EvaluateOwn` 写该控件的 Own Token；每个控件一个类 |
| `ControlTokenAlgorithmAttribute(catalog, id, revision)` | 把控件阶段类关联到算法，可重复 |
| `ThemeAlgorithmContext` | 整条链的 `Appearance` 与链上的 `Algorithms` |
| `ThemeAlgorithmContractException`、`ThemeAlgorithmContractViolationKind` | 阶段写了本阶段之外的 Token、写入不稳定或自身抛异常时抛出 |
| `IControlToken`、`ControlTokenAlgorithmDescriptor` | Token 类生成的控件身份；不用反射的控件阶段描述符 |
| `ThemeBuildAlgorithmAttribute` | 把算法身份传给引用方程序集的构建契约 |

优先级固定为：Ant 默认公式 < 算法 < 主题显式值 < 更内层作用域的显式值。内置主题的编译结果与之前相同。不写算法就
不需要做任何事；写算法见[编写主题算法](../guides/theming/writing-theme-algorithms.md)。

## 控件 Token 类改为 public partial

`AtomUI.Controls`、`AtomUI.Desktop.Controls`、`AtomUI.Desktop.Controls.ColorPicker`、`AtomUI.Desktop.Controls.DataGrid`
有 148 个控件 Token 类（116 个终端类加 32 个抽象中间层）改为公开；加上原本就公开的 `ColorPickerToken` 与
`AbstractColorPickerToken`，这四个包的 150 个控件 Token 类（117 个终端类、33 个抽象中间层）现在都是 `public partial`，主题包可以在控件阶段直接写它们。`AtomUI.Toolkits.GalleryBase` 的 8 个 Token 类保持 `internal`。
Token 的名称和类型此前已通过 `XxxTokenKind` 和 `ThemeBuildToken` 特性公开，这次多出的是类型本身。

公开的抽象中间层（例如 `AbstractButtonGeometryToken`）现在可以被其他程序集继承，它们的 `protected` 成员（例如
`CalculateContentPadding`）从此属于公开 API。

第三方 Token 类要改成新写法：

```csharp
// Before
[ControlDesignToken]
internal sealed class RatingToken : AbstractControlDesignToken
{
    public double StarSize { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        StarSize = EffectiveGlobalToken.ControlHeight;
    }
}

// After
[ControlDesignToken]
public sealed partial class RatingToken : AbstractControlDesignToken
{
    public partial double StarSize { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        StarSize = EffectiveGlobalToken.ControlHeight;
    }
}
```

- 生成器为每个 partial Token 属性产出另一半（`get => field;` 和调用 `RecordWrite` 的 setter）。Token 类或属性不是
  partial 时报 `ATOMUIGEN049`（Error），该属性不产出实现。属性初始化器可以留在声明处。
- `AbstractControlDesignToken.CalculateTokenValues` 现在是 `protected virtual`，写 `public override` 会报 CS0507，
  Token 类之外的代码不能再调用它。
- 类可以保持 `internal`；那样只有本程序集能为它声明控件阶段。

`DesignToken` 的 255 个全局 Token 属性也是 partial 属性，调用方不受影响。

## 全局 Alias 显式值现在带动派生 Alias

此前显式 Alias 值在 `CalculateAliasTokenValues` 之后才写入，由它派生的 Alias Token 仍是公式值。现在显式 Alias 值在
公式之前写入并标记，公式之后再写一次。受影响的派生关系：

| 显式设置的 Token | 现在带动 |
| --- | --- |
| `ControlOutlineWidth` | `InputActiveShadow`、`InputErrorActiveShadow`、`InputWarningActiveShadow`（扩散） |
| `ColorControlOutline` | `InputActiveShadow`（颜色） |
| `ColorErrorOutline` | `InputErrorActiveShadow`（颜色） |
| `ColorWarningOutline` | `InputWarningActiveShadow`（颜色） |
| `LineWidthFocus` | `FocusVisualBorderThickness` |
| `ControlPaddingHorizontal`、`ControlPaddingHorizontalSM`、`UniformlyPaddingSM` | `InputPaddingLG`、`InputPaddingSM`、`InputPadding` |

没有设置这些 Token 的主题编译结果完全不变（`Default`、`Dark`、`Compact` 与 DaybreakBlue 不变）。预设包里 Bootstrap 和
PrimeNG 的 profile 有变化：例如 PrimeNG Aura 的焦点阴影扩散现在是 0（它的 `ControlOutlineWidth` 是 0），Bootstrap 的
输入框焦点阴影由 `0 0 0 2 #190591ff` 变为 `0 0 0 3 #6fc8ff`（Bootstrap 预设同时改为 Bootstrap 6.0 的值，Dark 为 `0 0 0 3 #0064ba`）。如果依赖旧结果，把派生 Token 也显式设置。分析器
`ATOMUIGEN040`、`ATOMUIGEN048` 现在也检查 `DesignToken.CalculateAliasTokenValues`。

## 主题 XML：`<Algorithm>` 的 `Catalog`

```xml
<Algorithms>
  <Algorithm Catalog="PrimeNG" Id="Aura.Dark" />
  <Algorithm Id="Compact" />
</Algorithms>
```

`Catalog` 可选，缺省为 `AtomUI`，现有文件不变。XSD 中 `Id` 的类型是三个内置值与 `Identifier` 的并集。标识符不合法在
Reader 失败；合法但未注册在 Binder 失败（`ATMTHM2001`），编译主题包则在构建期失败（`ATOMUITOKEN002`，
「Unknown algorithm 'Catalog:Id'.」）。同一条链里重复的身份报 `ATMTHM1002`。

## 编译主题输入格式版本 6

Toolchain 任务交给生成器的中立中间文件，格式版本由 4 升为 6：算法项带 `Catalog`、`Id`、行、列，不再是整数（版本 5）；
每个定义还携带 `DesignLanguage`、`Density` 和 `BrandColor`（版本 6）。它是构建工具的私有协议。生成器现在输出运行时
ABI 2 的编译主题程序（携带设计语言元数据和算法链的 `CompiledThemeProgram` 构造函数）；旧生成器构建的 ABI 1 编译包仍可
加载，元数据取缺省值。旧构建留下的中间文件会报「Compiled theme input format is out of date; rebuild the project.」，
重新构建（或清理）项目即可。

## 新增与扩展的诊断

| 诊断 / 异常 | 变化 |
| --- | --- |
| `ATOMUIGEN049`（Error） | 新增：`[ControlDesignToken]` 或 `[GlobalDesignToken]` 的 Token 属性或其所在类不是 partial |
| `ATOMUIGEN040`、`ATOMUIGEN048` | 范围扩展到 `DesignToken.CalculateAliasTokenValues`（Alias 属性，守卫为 `IsExplicit(SharedTokenKind.X)`）；消息改为指明 Kind 枚举和方法名 |
| `ATOMUITOKEN002` | 新的触发情形：主题 XML 引用未知算法、算法构建契约无效或重复、`<Catalog>Algorithms` 成员名撞名、`[ControlTokenAlgorithm]` 类不合规、`[ThemeAlgorithm]` 声明不合规（不是实现 `IThemeAlgorithm` 的非抽象非泛型类、没有公开无参构造函数，或 catalog、id、revision、appearanceEffect 无效；以前会被静默跳过） |
| `ATMTHM1002` | Reader 的新消息：「Theme algorithm 'Catalog:Id' is duplicated.」（判重由 XSD 移入 Reader）；以及仅在 XSD 尚未对该元素报错时出现的「Theme algorithm identity 'Catalog:Id' is not a valid identifier.」 |
| `ATMTHM4004` | 新消息：「Algorithm identity is uninitialized.」 |
| `ThemeSchemaException` | 「Duplicate algorithm identity 'Catalog:Id'.」「Duplicate control algorithm '{algorithm}' for control '{control}'.」 |
| `ThemeLoadException` | 运行时追加包：「Package append cannot add a control algorithm for the already registered algorithm '{algorithm}' and control '{control}'.」 |
| `ThemeAlgorithmContractException` | 可选算法阶段违反契约：`StageBoundary`、`Unstable` 或 `Failed`；算法的 Map 阶段抛异常时也报 `Failed`（内置算法也一样）。控件阶段的违规在该控件第一次被读取时抛出，不是编译期诊断 |

## 设计语言与品牌色

主题定义可以在 `<Theme>` 元素上声明 `DesignLanguage`、`Density`（`Normal` 或 `Compact`，缺省 `Normal`）和 `BrandColor`
（`Seed` 或 `Fixed`，缺省 `Fixed`）。所有新增都源码兼容：`ThemeInfo` 只追加 `init` 属性，`ThemeRequest` 与 `ThemeConfig` 不变。

| API | 说明 |
| --- | --- |
| `ThemeDensity`、`ThemeBrandColorSupport` | 两个新枚举 |
| `ThemeInfo.DesignLanguage`、`Density`、`BrandColor`、`Algorithms` | 新增 `init` 属性；`Algorithms` 是定义自身的算法链，保存为只读副本（赋 `null` 抛 `ArgumentNullException`），参与相等比较 |
| `ThemeSelection`、`ThemeSelectionResult` | 选择，以及解析出的主题 Id、算法链和品牌种子 |
| `ThemeSelectionResolver` | 纯函数 `Resolve`、`DesignLanguages`、`BrandColorSupport`。没有精确变体时，`Resolve` 回退到该语言的 Light + Normal 定义，没有则回退到最近的已声明定义（同 Appearance，其次同 Density，最后取第一个）；只有语言不存在时抛 `ArgumentException` |
| `CompiledThemeProgram` ABI 2 构造函数 | 受保护，供生成代码使用；携带设计语言元数据和算法链。ABI 1 编译包仍可加载 |

`Seed` 主题不得手写品牌派生 Token（`ColorPrimaryHover`、`ColorPrimaryActive`、`ColorPrimaryBg`、`ColorPrimaryBgHover`、
`ColorPrimaryBorder`、`ColorPrimaryBorderHover`、`ColorPrimaryText`、`ColorPrimaryTextHover`、`ColorPrimaryTextActive`、
`ColorControlOutline`），否则报 `ATMTHM3101`；`ColorLink` 不在其中。没有声明 `BrandColor` 的主题按 `Fixed` 处理。Gallery 的 Theme Presets 页只列出编译主题包，因此不再为未声明 `BrandColor` 的编译主题包（包括 ABI 1 编译包）提供品牌色色块；要启用请声明 `BrandColor="Seed"`。`ATMTHM3103`（同一设计语言内 `Appearance` 与
`Density` 重复）和 `ATMTHM3104`（混用 `Seed` 与 `Fixed`）在编译主题包内是 Error，在运行时目录是 Warning：在后的冲突定义退出分组，
仍可按 Id 使用。规则和解析器示例见[主题定制指南](../guides/theming/customization.md)。

## PrimeNG 预设改由算法计算

`AtomUI.Themes.PrimeNG`（尚未发布到 NuGet）现在注册六个算法：`PrimeNG:Aura`、`PrimeNG:Aura.Dark`、`PrimeNG:Lara`、
`PrimeNG:Lara.Dark`、`PrimeNG:Nora`、`PrimeNG:Nora.Dark`，带 Alias 和控件阶段。12 个 profile ID 保留，每份 XML 只剩
算法链和 Seed Token（主色、错误色、圆角、控件高度，Nora 另有 `EnableMotion`），只有 Lara 的两个紧凑 profile 另保留一项 Select
覆盖。可见的变化：

- PrimeNG 主题里的 `ColorPrimary`、`ColorError` 等状态色是种子（色阶的 500 级），由算法映射到实际显示的级。换主色种子
  后，悬停、按下、焦点环和选中底色都会跟着变。
- 十二个 profile 的默认主色改为 **noir**，即 primeng.dev（primeng 22.1.5）的默认主色：`ColorPrimary` 由 emerald
  `#10B981` 改为 `#020617`（slate-950）。中性色 950 级的种子让主色色阶取表面色阶：浅色 `#020617` / `#1E293B` /
  `#334155` 配白字，暗色 `#FAFAFA` / `#E4E4E7` / `#D4D4D8` 配 `#09090B` 字，选中项使用站点的实色 noir 高亮，CheckBox /
  Radio 的选中标记取对比色。要恢复原来的颜色，把 `ColorPrimary` 设为 `#10B981`；其他种子行为不变。下表中的 emerald 值即 emerald
  种子下的值。
- 选中项严格按 PrimeUIX 3.1.1 的 `highlight`：选中底色为 `ControlItemBgActive`（Nora 浅色主色 600 / 悬停 700，暗色
  500 / 400；noir 950 / 700，暗色 50 / 300；Aura 与 Lara 保持浅淡的主色 50 / 100），Select、Cascader、ComboBox、ListBox、
  ListView、TransferListView、TreeView、TransferTreeView、TreeSelect 的选中文字为 `highlight.color`（Aura / Lara 浅色为
  主色 700，不再是正文色）。`ColorPrimaryBg` 保持浅淡主色（Nora 暗色为主色 16%，不再是实色 500），Filled 按钮与 Tag
  不再需要补偿；Nora 暗色 `ColorPrimaryText` 改为可读的主色 400，不再是 zinc-950。日期与时间格子保持浅淡选中底色。
  ToggleSwitch 选中把手按 PrimeUIX（Aura / Lara 暗色 zinc-900，Nora 为对比色）。
- 暗色的主色和错误色变了。旧的暗色 profile 让 `AtomUI:Dark` 对已经是最终值的颜色又推导了一遍：

  | Profile | `ColorPrimary` | `ColorError` |
  | --- | --- | --- |
  | `PrimeNGAura.Dark`、`PrimeNGAura.DarkCompact` | `#2FB685` → `#34D399` | `#D98F8F` → `#FCA5A5` |
  | `PrimeNGLara.Dark`、`PrimeNGLara.DarkCompact` | `#2FB685` → `#34D399` | `#D98F8F` → `#FCA5A5` |
  | `PrimeNGNora.Dark`、`PrimeNGNora.DarkCompact` | `#11A071` → `#10B981` | `#D66363` → `#F87171` |

- 原来由 Ant 公式派生的颜色改按 PrimeNG 色阶取级：成功、警告、信息、错误家族的悬停、按下、底色、边框和文字状态；
  链接色（取主色种子色阶中的一档；浅色下链接取比主色填充更深的级，即 700 / 800 / 900 级，保证白底上的链接文字可读）；中性填充、实色底与提示底色；第三、第四级文字与禁用文字；实色主色上的对比文字（暗色为中性
  900，Nora 暗色为中性 950）。Nora 还把成功、警告、信息的基准色移到 600 级（暗色 500 级）。原来半透明的填充色现在是
  不透明的色阶颜色。
- 除上文的 Alias 派生修复外，算法迁移没有改变任何尺寸、内边距、圆角、字体、线宽、动效或阴影几何。
- `[PrimeNG:Aura.Dark]` 与 `PrimeNGAura.Dark` profile 相同，基础链后接 `AtomUI:Compact` 与 `.Compact` profile 相同
  （Lara 的紧凑 profile 保留一项列出的差异）。在浅色 PrimeNG 链后接 `AtomUI:Dark` 是混合链，不是 PrimeNG 的暗色。

包的 README 列出了每条规则及其来源。

## 性能

Token 写入记账让每个生成的 Token setter 多一次空引用判断，每个 Token 实例多 8 字节。与改造前的树对比实测
（2026-11-13，默认 Ant 主题）：主题冷编译按门槛统计量（25 分位数）慢 2.3%–2.7%（约 0.018 毫秒），按中位数慢
3.4%–3.7%。启动检查按字面规则没有通过（越界大多在更快一侧；变慢一侧，普通窗口的预编译分段高出基线最大值 0.005 毫秒，
带启动屏的提交分段高出 0.17 毫秒）。主题切换、分配、程序集与 NativeAOT 体积、编译结果都通过。这没有达到设计时定的 2%
门槛，项目负责人决定按此结果通过，是对门槛的一次明确放宽。渲染和资源查找不经过 Token 属性，不受影响。

该决定之后加入的全部内容（Alias 阶段与控件阶段）在 2026-11-14 与提交 `3a87bba25`（接受记账代价时的树）对比复测。
内置 Default、Dark、Compact 三条链的冷编译（25 分位数 −0.87% / −1.24% / −1.21%）与主题切换在波动范围内，每次冷编译的
分配（1,138,032 / 1,143,224 / 1,138,416 字节）与编译结果不变；当天这台机器冷编译的分辨力只有 8%–9%，分辨不出 2% 的差异。
启动时即使控件阶段注册表为空也会被 JIT，由此多出约 1 毫秒，这一问题已在发布前发现并修复；现在启动期间 JIT 的方法与基线
相同，耗时在机器约 1 毫秒的分辨力内不变（注册了控件阶段的应用没有测量）。`AtomUI.Core.dll` 增大 2.05%（1,125,888 →
1,148,928 字节），超过计划定的 1% 程序集体积门槛；门槛没有达到，项目负责人在 2026-11-14 决定接受这一增长，这是只限
这一个程序集的一次明确放宽。其他受门槛约束的程序集增长不超过 0.10%，NativeAOT Gallery
可执行文件小 0.09%。使用阶段的主题要为阶段付出代价：`PrimeNGAura` 冷编译约 0.88 毫秒，内置默认主题约 0.74 毫秒（+18%），
分配 1,249,256 字节对 1,138,032 字节（+9.8%）；`PrimeNGAura.Dark` 约 0.89 毫秒（+20%），1,254,568 字节（+10.2%）。
