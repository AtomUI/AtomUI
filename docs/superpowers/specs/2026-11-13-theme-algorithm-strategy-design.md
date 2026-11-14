# AtomUI 主题算法策略化：三阶段算法契约与设计语言预制算法

日期：2026-11-13。状态：设计方案，已过一轮实施者视角审阅，待用户审阅；未开始实现。
基线：`feature/design-language`（`c381abf76`）。前置方案：`2026-11-07-theme-customization-precision-design.md`。
参照：Ant Design 的 `theme.algorithm`（接受用户自定义的 Seed → Map 函数）。

## 1. 目标与边界

### 1.1 为什么做

前置方案用「Ant 的算法 + 写死的覆盖值」做出了 PrimeNG、shadcn、Bootstrap、Material 四个预设包。它证明了可模拟，但留下三个问题：

1. **换主色，状态色不跟。** PrimeNG Aura 把 `ColorPrimaryHover`、`ColorPrimaryActive`、`ColorFocusBorder` 写成固定的 emerald 色值。用户把主色改成蓝色后，悬停、按下和焦点环仍是绿色。
2. **没写死的值按 Ant 的规则派生。** `ColorPrimaryBorder`、`ColorPrimaryBgHover` 等由 Ant 的色板算法从 emerald 算出，和写死的几个值不是同一套规则。
3. **明暗和紧凑不能组合。** 各预设包的 README 都写明：给基础主题换上 Dark 算法不等于选中具名的 Dark profile，因为写死的值不随算法变化。所以每个家族要维护 4 份 XML（PrimeNG 三个家族共 12 份）。

根因相同：一个设计语言由「规则」和「数值」两部分组成，现在只有数值能表达，规则只能是 Ant 的。

### 1.2 目标

1. 主题算法成为可注册、可设置的策略。主题包可以带自己的算法，应用开发者可以自己写算法。Ant 的三个算法是这套契约的第一批实现，不享有特殊通道。
2. 算法可以计算所有 Token：Map 层必须写满；Alias 层和控件 Token 层按需改写，没改写的沿用 Ant 的公式。
3. 提供 PrimeNG 预制算法，并用它重做 PrimeNG 预设包。Material、Bootstrap、shadcn 的算法按同一契约后续实现。
4. 修复全局 Alias 层「显式值不带动派生值」的缺口（见 2.3）。它是 Alias 阶段的前置条件，同时修掉现有 XML 覆盖的同类问题。

### 1.3 不做

- 不引入设计语言抽象层，不按语言切换 ControlTheme，不改控件结构。控件读哪个 Token 不归算法管。
- 不新增全局 Seed Token（例如「surface 色阶」种子）。
- 不做自动 profile 派发（根据系统明暗自动在 `X` 与 `X.Dark` 之间切换），也不做「暗色对应算法」元数据。后果见 4.4 的混合链一段。
- 不在主题 XML 里加 Token 表达式语言。
- 不为 Token 另造一套镜像的「写入器」类型。算法直接写 Token 对象本身（见 4.2 和第 8 节决策 1）。
- Material、Bootstrap、shadcn 三个算法的具体实现不在本文档的实施范围内，本文档只定它们的规则来源和验收方式（见 4.10）。

### 1.4 反目标

下面任何一条成立，即使功能可用也算失败：

- 使用内置 `Default / Dark / Compact` 的主题，编译结果因本方案发生变化（4.5 的修复除外）。
- 性能因本方案变差：主题编译、主题切换、启动、渲染期的资源查找，任何一项出现可测量的退化（门槛见 4.11）。这是用户明确提出的硬约束。
- 算法作者需要了解 Ant 内部的派生依赖才能写对（例如改了 `ControlOutlineWidth` 还得自己重算 `InputActiveShadow`）。
- 算法写入的值盖过了用户在主题里显式写的值。
- 写算法时失去类型检查和 IDE 智能提示（退回到字符串名加字符串值）。
- 只有 AtomUI 自己的主题包能写控件阶段，第三方和应用开发者写不了。

## 2. 现状

### 2.1 已有的结构

- `IThemeAlgorithm.Evaluate(effectiveSeed, previousMap, nextMap)` 是公开接口。`ThemeCompiler.EvaluateAlgorithms` 按链依次调用，不关心实现来自哪里。
- 生成器已经在每个包里收集带 `[ThemeAlgorithm]` 的类，并产出 `GeneratedThemeSchema.GetAlgorithms()`。
- 全局编译顺序（`ThemeCompiler.CompileGlobalBuilder`）：写 Seed 显式值 → 算法链 → 写 Map 显式值 → `CalculateAliasTokenValues()` → 写 Alias 显式值。
- 控件 Token 编译顺序（前置方案的修复 A）：标记并写入显式 Own 值 → `descriptor.Evaluate`（即 `CalculateTokenValues`，内部用 `IsExplicit` 守卫派生字段）→ 再写一次显式值。

### 2.2 封闭点

有三处封闭。

**一、算法身份是枚举。** `ThemeAlgorithm` 只有 `Default / Dark / Compact`，下列位置都绑在它上面：

| 位置 | 绑定方式 |
| --- | --- |
| `ThemeAlgorithmAttribute`、`ThemeAlgorithmDescriptor` | 构造参数是枚举，并用 `Enum.IsDefined` 校验 |
| `ThemeSchemaRegistry` | 以枚举为键，按枚举值排序和计入指纹 |
| `ThemeConfig` / `ControlThemeConfig` 及其 Builder、`ThemeState`、`ThemeTransaction` | 公开属性和参数类型是 `IReadOnlyList<ThemeAlgorithm>` |
| `NormalizedThemeConfig`、`ThemeConfigNormalizer`、`ThemeCacheKey` | 用枚举的整数值做规范化、去重和缓存键 |
| `CompiledThemeBindingBuilder.SetAlgorithms` | 参数是枚举列表 |
| `ThemeDocumentReader`、`atomui-theme-v1.xsd` | 只接受三个字符串 |
| 生成器 `ThemeAlgorithmInfo`、`CompiledThemePackageWriter` | 读枚举常量，产出 `(ThemeAlgorithm)整数` |
| `AtomUI.Toolchain` 的 `GenerateCompiledThemeInputTask` | 构建期把 XML 的算法名转成整数 |
| `ThemeAlgorithmTypeConverter` | 只转换三个内置值 |

`docs/reference/theming/theme-definition-xml-v1.md` 第 5 节明确写了「当前算法集合是封闭的，不支持任意第三方字符串 ID」。本方案修改这条规定。

**二、包里的算法进不了注册表。** `ThemeSchemaRegistry` 的构造函数接受算法列表，但所有构造点传入的都是 Core 自己的 `GeneratedThemeSchema.GetAlgorithms()`（`ThemeManager.cs` 的 4 处、`ThemeManager.PackageLoading.cs` 的 2 处、`ThemeSchemaRegistry.CreateGlobalOnly`）。其他包生成的 `GetAlgorithms()` 是 `internal` 的，没有任何代码读取它；包注册（`GeneratedThemePackageRegistration`、控件包注册）也不携带算法。

**三、控件 Token 类是内部类型。** 带 `[ControlDesignToken]` 的类共 158 个：124 个 `internal sealed` 终端类（例如 `ToolTipToken`、`CardToken`）、32 个 `internal abstract` 中间层、2 个已公开；Own Token 属性共 1182 个。`AtomUI.Desktop.Controls` 也没有对主题包开放 `InternalsVisibleTo`。所以主题包里的代码无法引用这些类型。但 Token 的名称和类型已经是公开契约：对应的 `XxxTokenKind`、`XxxTokenKey` 枚举是公开的，程序集上的 `ThemeBuildToken` 特性也带着每个字段的名称和值类型。

### 2.3 数据

四个预设包浅色主题的覆盖项构成：

| 预设 | 全局覆盖 | 其中颜色 | 逐控件覆盖 | 其中颜色 |
| --- | --- | --- | --- | --- |
| PrimeNG Aura | 33 | 18 | 20 | 1 |
| Shadcn Slate | 39 | 30 | 0 | 0 |
| Bootstrap | 19 | 14 | 0 | 0 |
| Material | 18 | 12 | 0 | 0 |

三点观察：

1. Aura 的 53 项覆盖里只有 19 项是颜色，所以算法不能只算颜色。
2. Aura 的全局覆盖里至少 9 项属于 Alias 阶段（`ColorFocusBorder`、`FocusVisualBorderThickness`、`ControlOutlineWidth`、`ColorTextPlaceholder`、`ColorTextDescription`、`ColorBgContainerDisabled`、`ColorTextLightSolid`、`PaddingContentHorizontal`、`BoxShadows`）。它们在算法之后由固定的 `CalculateAliasTokenValues` 计算，算法写了也会被盖掉。
3. Aura 的逐控件覆盖里有一部分不是控件自己的 Token，而是「只对该控件生效的全局 Token」（例如 Card 的 `BorderRadiusLG 12`、CheckBox 的 `BorderRadiusSM 4`）。控件阶段必须两种都能写。

**全局 Alias 层的缺口。** 显式的 Alias 值在 `CalculateAliasTokenValues()` 之后才写入（`ThemeCompiler.cs:766-767`），`DesignToken` 上也没有 `IsExplicit` 守卫。所以依赖它的派生 Alias 不会跟着变。例如 Aura 把 `ControlOutlineWidth` 设成 0，但 `InputActiveShadow` 的 `Spread` 仍按 `LineWidth * 2` 算出（`DesignToken.cs:127, 175`）。这是从代码顺序读出的结论，没有做渲染实测；实施第 2 步要先写出失败的测试来确认。

## 3. 方案比较

| 方案 | 做法 | 取舍 |
| --- | --- | --- |
| A. 关闭计算，写全量数值表 | 预设主题禁用算法，把所有 Token 写成固定值 | 最精确，但表有几百项，换主色时什么都不跟，明暗紧凑各要一份全表。问题 1、3 更严重 |
| B. 只开放 Map 层算法 | 开放算法身份，算法只写 Map | 改动最小。但焦点环、占位符、禁用底色都在 Alias 层，仍然只能写死；逐控件的派生色也只能写死 |
| C. 三阶段算法（采用） | Map 必写，Alias 和控件 Token 可选改写，Ant 公式兜底 | 规则都能进算法；契约比 B 大，编译器要处理「算法写入的值带动派生值」 |
| D. XML 表达式 | 让 `<Token Value>` 能引用别的 Token 或调用函数 | 色阶生成需要函数，等于在 XML 里重新发明算法，还丢掉类型检查 |

采用 C。用户已确认「算法理论上可以动态算所有 Token 值」这一原则。

## 4. 设计

### 4.1 算法身份开放

`ThemeAlgorithm` 从枚举改为只读结构体，仿照 `ControlTokenIdentity`：

```csharp
public readonly struct ThemeAlgorithm : IEquatable<ThemeAlgorithm>
{
    public ThemeAlgorithm(string catalog, string id);   // both validated by SchemaIdentifier
    public string Catalog { get; }
    public string Id { get; }

    public static ThemeAlgorithm Default { get; }   // AtomUI:Default
    public static ThemeAlgorithm Dark { get; }      // AtomUI:Dark
    public static ThemeAlgorithm Compact { get; }   // AtomUI:Compact
}
```

- 保留类型名和三个成员名，所以 `WithAlgorithms(ThemeAlgorithm.Dark, ThemeAlgorithm.Compact)` 这类调用不用改。
- `Catalog` 没有保留规则，与控件身份的现状一致（控件身份在运行时和生成器里都没有保留检查）。重复的身份由 `ThemeSchemaRegistry` 现有的唯一性检查拒绝。
- 相等性按 `Catalog` 和 `Id` 的序数比较。`ToString()` 输出 `Catalog:Id`。
- `ThemeSchemaRegistry`、`NormalizedThemeConfig`、`ThemeConfigNormalizer`、`ThemeCacheKey` 改为以该结构体为键，按 `Catalog`、`Id` 的序数排序；指纹和缓存键计入 `Catalog`、`Id`、`Revision`、`AppearanceEffect`，不再计入整数值。
- 同一算法链内不得重复同一身份，这条规则不变。

**声明方式。**

```csharp
[ThemeAlgorithm("PrimeNG", "Aura", revision: 1, ThemeAppearanceEffect.Light)]
public sealed class PrimeNGAuraAlgorithm : IThemeAlgorithm, IThemeAliasAlgorithm { ... }
```

Attribute 的参数从枚举改为 `catalog`、`id` 两个字符串。生成器为每个声明了算法的包产出一个公开静态类 `<Catalog>Algorithms`，每个算法一个 `ThemeAlgorithm` 类型的静态属性（`Id` 里的点号去掉，例如 `Aura.Dark` 对应 `PrimeNGAlgorithms.AuraDark`），供 C# 侧取得类型化身份。Core 的三个内置算法继续通过 `ThemeAlgorithm.Default` 等成员取得。

**包到注册表的通道。** 包注册要携带算法描述符，和携带控件 Token 描述符的方式一致。三条路径都要覆盖：启动索引、仅含全局 Token 的启动注册表（`CreateGlobalOnly`）、运行时追加包。追加包时的规则见 4.4。

**XML。**

```xml
<Algorithms>
  <Algorithm Catalog="PrimeNG" Id="Aura.Dark" />
  <Algorithm Id="Compact" />
</Algorithms>
```

- `Catalog` 是可选属性，省略时为 `AtomUI`。现有主题文件不用改。
- XSD 中 `Id` 的类型改为「三个内置值的枚举」与 `IdentifierType` 的并集，这样内置值仍有补全，其他合法标识符也能通过校验。
- 两个错误边界保持不变：标识符不合法在 Reader 边界失败；合法但未注册在 Binder 边界失败，诊断里带 `Catalog:Id` 和源码位置。
- 编译主题包：生成器的构建契约（`ThemeBuildContracts`）新增算法记录（`Catalog`、`Id`、`Revision`、`AppearanceEffect`），随包传递给引用方。主题 XML 引用了未知算法时，构建期报错并带行列号，处理方式与未知 Control 相同。`GenerateCompiledThemeInputTask` 和 `CompiledThemePackageWriter` 改为传递和产出身份，不再用整数。

**AXAML。** `ThemeAlgorithmTypeConverter` 接受 `Default`、`Dark`、`Compact` 和 `Catalog:Id` 两种写法。

### 4.2 三阶段契约

```csharp
// Stage 1 — Map. Required. Unchanged signature.
public interface IThemeAlgorithm
{
    void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap);
}

// Stage 2 — Alias. Optional; implemented by the same class as the Map stage.
public interface IThemeAliasAlgorithm : IThemeAlgorithm
{
    void EvaluateAlias(in ThemeAlgorithmContext context, DesignToken token);
}

// Stage 3 — Control tokens. Optional; one class per control.
public interface IControlTokenAlgorithm<TToken> where TToken : AbstractControlDesignToken
{
    void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal);
    void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TToken token);
}

public readonly struct ThemeAlgorithmContext
{
    public ThemeAppearance Appearance { get; }              // resolved appearance of the whole chain
    public IReadOnlyList<ThemeAlgorithm> Algorithms { get; } // the effective chain, in order
}
```

| 阶段 | 输入 | 可以写什么 | 没写的值 |
| --- | --- | --- | --- |
| Map | 有效 Seed、链上前一个算法的 Map | `nextMap` 上所有 Seed 和 Map 阶段的全局 Token | 没有兜底。单独使用（`previousMap` 为空）时必须写满 Map 阶段的全部 Token |
| Alias | 已写入 Map 显式值、且已按 Ant 公式算过 Alias 的全局 Token | Alias 阶段的全局 Token | Ant 的 `CalculateAliasTokenValues` |
| 控件 · `EvaluateGlobal` | 该控件的有效全局 Token（控件自己的一份副本） | 任意全局 Token，只对该控件生效 | 作用域的全局值 |
| 控件 · `EvaluateOwn` | 该控件最终的有效全局 Token | 该控件的 Own Token | 控件的 `CalculateTokenValues` |

**算法直接写 Token 对象，Token 自己记录写入。** 三个阶段拿到的都是 Token 对象本身：Map 阶段本来就直接写 `DesignToken`，Alias 和控件阶段照此办理。不另造写入器类型。

为了让编译器确切知道算法写了哪些字段，Token 的属性改成 partial 属性，由生成器补上会记账的 setter：

```csharp
// Hand-written. The class becomes public and partial; each token becomes a partial property.
[ControlDesignToken]
public sealed partial class ToolTipToken : AbstractControlDesignToken
{
    /// <summary>Default background of the tooltip.</summary>
    public partial Color ToolTipBackground { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode) { ... }
}

// Generated. The getter stays a plain field read; the setter records only while recording is on.
partial class ToolTipToken
{
    public partial Color ToolTipBackground
    {
        get => field;
        set
        {
            field = value;
            RecordWrite(nameof(ToolTipBackground));
        }
    }
}
```

- **记账按属性名，不按槽位号。** 槽位是终端类全部 Token 名称的序数排序，中间层（例如 `AbstractButtonGeometryToken`）的同一个属性在不同终端类里槽位不同，生成的 setter 无法写死槽位。所以记的是名称，记账结束后由编译器经 Token 描述符解析成槽位，再交给 `MarkExplicit`。不记账时 setter 的路径仍然只是一次空引用判断。
- **记账只在算法的 Alias 和控件阶段运行期间打开。** 其余时间（Ant 默认公式、写入显式值、复制 Token）`RecordWrite` 只做一次「是否在记账」的判断就返回。实现形态受 4.11 的性能约束限制。
- **改动范围。** 158 个控件 Token 类全部改为 `partial`；其中 `AtomUI.Controls`、`AtomUI.Desktop.Controls`、ColorPicker、DataGrid 四个包里的 148 个内部类（116 个终端类加 32 个中间层）改为 `public`，中间层必须一起公开，因为公开类不能继承内部基类。`AtomUI.Toolkits.GalleryBase` 的 8 个类保持 `internal`。1182 个 Own Token 属性和 `DesignToken` 上 255 个全局 Token 属性改为 partial 属性。属性上现有的初始化器（35 处）可以原样保留，已用仓库的 SDK 验证。改动是机械的，用脚本一次完成。Token 属性不是 partial 时生成器报错（`ATOMUIGEN049`）。
- **生成的那一半要在 IDE 设计期可见。** 现有生成器用的是只在完整编译时运行的输出方式，partial 属性的实现放在那里，IDE 会一直报「缺少实现」。所以另起一个轻量的增量生成器专门输出这一半。
- **`CalculateTokenValues` 收窄。** 基类上它现在是 `public virtual`。类公开后，算法拿着 Token 就能调用它，会把 Ant 公式的写入全记到算法名下。所以基类改为 `protected virtual`，各 Token 类改为 `protected override`，生成的那一半提供一个内部入口 `RunDefaultCalculation` 给 `GeneratedThemeSchema` 调用。不用 `protected internal`：它要求有无 `InternalsVisibleTo` 的程序集写不同的 override 修饰符，GalleryBase 和第三方包会编译不过。公开后一个控件 Token 类对外可见的只有构造函数和 Token 属性；`EffectiveGlobalToken`、`IsExplicit` 本来就是 `protected`。
- **「只写本阶段的 Token」改为运行时检查。** 写入器方案能在编译期阻止 Alias 阶段写 Map 字段，这里做不到。改为：阶段结束后编译器检查记下的槽位，发现写了不属于本阶段的字段就报编译诊断，指出算法身份和字段名。4.6 的契约测试也覆盖这一条。Map 阶段今天同样没有编译期限制，三个阶段因此一致。
- **第三方控件。** 第三方自己的控件 Token 类也要按 partial 属性的写法改。本方案不考虑对旧写法的兼容。

**上下文。** `ThemeAlgorithmContext` 告诉各阶段整条链解析出的外观，以及链上有哪些算法。同一个 Alias 或控件阶段的实现要能处理明暗两种外观（见 4.4 的混合链）。

**所有阶段共同的约束：**

- **纯函数、可确定。** 相同输入必须产生相同输出；不读时钟、环境、静态可变状态。编译缓存依赖这一点，编译器也会对同一阶段调用多次（见 4.3）。
- **不改输入。** `effectiveSeed`、`previousMap`、`EvaluateOwn` 的 `effectiveGlobal` 只读。
- **不需要重算派生值。** 算法改了某个字段后，依赖它的 Ant 默认值由编译器重新计算。
- **不要用一个字段自己的当前值去算它的新值**（例如 `token.ContentPadding = token.ContentPadding + 2`）。4.3 的流程会运行算法不止一次，带 `IsExplicit` 守卫的字段第二次读到的是算法上一次写的值，结果会逐轮累加。新值要从 `effectiveGlobal` 或别的字段推出。违反时 4.3 的稳定性检查会报诊断。getter 必须保持纯字段读取（4.11），所以编译器无法在读取时替算法兜住这种写法。
- **尺寸类的值从 Map 的尺寸 Token 推出，不写常量。** 这样后接 `AtomUI:Compact` 时它们会跟着缩小。确实需要区分时可以检查 `context.Algorithms`。

**控件阶段的声明。** 用 Attribute 关联到算法，一个类可以关联多个算法（例如明暗共用）：

```csharp
[ControlTokenAlgorithm("PrimeNG", "Aura", revision: 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", revision: 1)]
internal sealed class PrimeNGToolTipTokens : IControlTokenAlgorithm<ToolTipToken> { ... }
```

- 控件阶段的类可以和算法不在同一个程序集。第三方控件包可以为自己的控件提供某个已有算法下的取值。
- 同一个（算法身份，控件身份）只能有一个实现，重复时注册表报错。
- 应用没有注册对应控件包时，该实现不会被调用。
- 生成器为每个实现产出不依赖反射的类型化调用委托，并把（算法身份，控件身份，`revision`）计入所有持久指纹和缓存键。

### 4.3 编译顺序与优先级

优先级从低到高固定为：

**Ant 默认公式 < 算法 < 主题的显式值（XML 或 `ThemeConfig`）< 更内层作用域的显式值。**

算法写入的值和用户显式值的行为相同：它参与派生（依赖它的默认值会跟着变），并且最终生效（除非被更高优先级的值覆盖）。用户已显式设置的字段，算法的写入不生效。

**全局编译：**

1. 写入 Seed 阶段的显式值。
2. 依次运行算法链的 Map 阶段。
3. 写入 Map 阶段的显式值。
4. Alias 阶段，按下面的「三步流程」执行。链上有多个算法实现了 Alias 阶段时按链的顺序运行，后者覆盖前者。
5. `NormalizeFinalBehavior`。

**控件编译：**

1. 得到该控件的有效全局 Token（现有逻辑，含 `ControlAlgorithmMode` 的四种模式）。
2. 运行控件阶段的 `EvaluateGlobal`，再写入用户对该控件的全局 Token 覆盖。`EvaluateGlobal` 写入的值按 `Disabled` 模式的语义直接生效，不触发 Map 和 Alias 重算。在使用作用域全局 Token 的模式（未指定与 `Disabled`）下，主题显式设置的 Map/Alias 值胜过 `EvaluateGlobal` 的写入（编译器在阶段之后把它写过的这类 Token 写回主题显式值）；Seed 是已被 Map 阶段消费的输入，不写回。`Global` 与 `Custom` 模式不写回，见下文的既有例外。
3. Own Token 阶段，按「三步流程」执行，默认计算是该控件的 `CalculateTokenValues`。

**优先级在控件 `Global` 与 `Custom` 模式下的一个既有例外。** 这两种模式会为该控件从种子重新推导一份全局 Token（`ReevaluateControlGlobal`）：以根主题编译后的全局值为起点重跑算法链和 Alias 计算，然后只写回这个控件自己的全局覆盖。主题在根上显式设置的 Map 与 Alias 值不参与这次重新推导，所以在这个控件的作用域里，它们会被默认公式的结果取代；实现 Alias 阶段之后，也可能被该控件链上算法的 Alias 阶段取代。这是本方案之前就有的行为，本方案没有改动它，上面「算法 < 主题显式值」的顺序在这两种模式下只对该控件自己的覆盖成立。是否改成让根上的显式值也参与重新推导，需要单独决定；它会改变现有主题在这两种模式下的编译结果。

控件阶段使用哪条算法链：`ControlAlgorithmMode.Custom` 用控件自己声明的链，其余三种模式用所在作用域的全局链。`Global` 和 `Custom` 模式重算控件的全局 Token 时（`ReevaluateControlGlobal`），同样要执行 Alias 阶段。

**三步流程**（Alias 阶段与 Own Token 阶段相同）：

1. 标记并写入用户显式值，运行默认计算，再写一次用户显式值。这是没有算法该阶段时的结果，也是今天的行为。链上没有任何算法实现该阶段时，流程到此结束。
2. 打开记账，在 Token 对象上运行算法该阶段，然后关闭记账。Token 记下算法写过的槽位，编译器按槽位取出对应的值。没写任何槽位时流程结束。
3. 把算法写过的槽位和用户显式槽位一起标记为显式，重新运行默认计算（守卫跳过显式槽位，派生字段读到的是算法和用户的值），再运行一次算法该阶段，最后再写一次用户显式值。

第 3 步之后做一次稳定性检查：如果第二次运行写入的槽位或值与第一次不同，把新槽位并入显式集合并重复第 3 步。超过 3 轮仍不稳定时报编译诊断，指出算法身份和不稳定的 Token。正常的算法第一轮就稳定；只有当算法读取的 Ant 默认值又依赖它自己写的值时才会出现第二轮。

内置三算法没有 Alias 和控件阶段，它们的路径在第 1 步结束，没有额外开销。

### 4.4 链的组合与包的加载

**链上每个算法的 Alias 和控件阶段都会运行**，不管它后面还有没有别的算法。阶段实现通过 `context.Appearance` 得知最终外观。

| 链 | 结果 |
| --- | --- |
| `[PrimeNG:Aura]`、`[PrimeNG:Aura.Dark]` | PrimeNG 的浅色、暗色 |
| 以上任一 + `AtomUI:Compact` | 对应的紧凑版。Map 的尺寸由 Compact 缩小，Alias 和控件阶段的尺寸值跟着变 |
| `[PrimeNG:Aura, AtomUI:Dark]`（混合链） | 合法，但不是 PrimeNG 的暗色：Map 的颜色由 Ant 的 Dark 规则重算，PrimeNG 的 Alias 和控件阶段在暗色外观下运行。契约只保证结果可读（见 4.6 的对比度检查） |

Gallery 的全局明暗开关目前是把 `ThemeAlgorithm.Dark` 追加到基础链后面（`GalleryWorkspaceViewModel`），对 PrimeNG 主题得到的就是混合链。本方案不改这个开关。让开关自动换成同族的暗色算法，需要算法声明「我的暗色对应算法是谁」，这属于 1.3 中列出的不做项，留作后续。Gallery 的预设页继续通过具名 profile 切换明暗。

**运行时追加包。** 现有规则是追加包不得改变已提交的编译结果。据此：追加的包可以带来新算法，也可以带来控件阶段的实现，但后者只在「对应的算法或对应的控件是同一次追加注册的」时被接受。为已经存在的算法和已经存在的控件补一个控件阶段实现，会改变已编译的结果，注册时拒绝并给出诊断。算法或控件在任何地方都没有注册的实现（「休眠」实现）与启动时一样被接受并保留：它不会被调用，之后某次追加注册了缺的那一边时，那一边对那次追加是新注册，同样不改变已提交的结果。代价是算法或控件身份写错时没有诊断。

### 4.5 全局 Alias：显式值先于默认计算

把前置方案修复 A 的做法搬到全局 Token 上：

- `DesignToken` 增加按全局槽位记录的显式位图和 `IsExplicit(SharedTokenKind)`，实现方式参照 `AbstractControlDesignToken`。
- `CalculateAliasTokenValues` 中，凡是「本方法内先赋值、后面又被读取」的字段，赋值处加 `IsExplicit` 守卫。例如 `ControlOutlineWidth`、`ColorControlOutline`、`ColorErrorOutline`、`ColorWarningOutline`、`LineWidthFocus`、`ControlPaddingHorizontal`、`ControlPaddingHorizontalSM`。只赋值、不被本方法后续读取的字段不需要守卫，最后的重写会让它们生效。完整清单由实施者枚举。
- 分析器 ATOMUIGEN040/048 的检查范围扩展到 `DesignToken.CalculateAliasTokenValues`，防止以后新增的派生漏掉守卫。
- `CompileGlobalBuilder` 和 `ReevaluateControlGlobal` 的 Alias 段改为：标记并写入 Alias 显式值 → `CalculateAliasTokenValues()` → 再写一次。

这一步独立于算法开放，可以单独合入。它会改变现有主题的编译结果（例如 Aura 的 `InputActiveShadow`），属于修复，要在 CHANGELOG 里说明。

### 4.6 契约测试

提供一套可复用的契约测试，对注册表里的每个算法都运行。Ant 的三个算法也必须通过。主题包的测试项目引用它来验证自己的算法。

| 检查 | 方法 |
| --- | --- |
| 身份与修订 | `Catalog`、`Id` 合法且唯一；`Revision` 大于 0 |
| Map 写满 | 对可单独使用的算法，用两组不同的预填值各运行一次；运行后仍然随预填值不同的 Map 阶段槽位就是没写的字段 |
| 确定性 | 相同输入运行两次，结果逐槽位相等 |
| 不改输入 | 运行前后 `effectiveSeed`、`previousMap`、`EvaluateOwn` 的 `effectiveGlobal` 的快照相等 |
| 响应种子 | 改变 `ColorPrimary` 种子后，`ColorPrimary`、`ColorPrimaryHover`、`ColorPrimaryActive`、`ColorPrimaryBg` 都发生变化 |
| 状态可区分 | 默认种子下 `ColorPrimary`、`ColorPrimaryHover`、`ColorPrimaryActive` 两两不同 |
| 可读性 | `ColorText` 对 `ColorBgContainer` 的对比度不低于 4.5；`ColorTextLightSolid` 对 `ColorPrimary` 不低于 3 |
| 外观一致 | `AppearanceEffect` 为 Dark 的算法，`ColorBgBase` 的亮度低于 `ColorTextBase`；Light 相反 |
| 可组合 | 后接 `AtomUI:Compact` 时控件高度变小，颜色类 Token 不变 |
| 混合链可读 | 后接外观相反的内置算法（浅色算法后接 `AtomUI:Dark`）时，上面的可读性检查仍然通过 |
| 稳定性 | 4.3 的三步流程在 3 轮内稳定 |
| 只写本阶段 | Alias 阶段记下的槽位都属于 Alias 阶段；`EvaluateOwn` 不改 `effectiveGlobal` |

### 4.7 PrimeNG 算法

放在 `AtomUI.Themes.PrimeNG` 包内。

**算法身份。** Aura、Lara、Nora 在 PrimeNG 里是三套不同的语义映射，共用同一套色板和色阶函数。对应到这里是六个算法，共用一个实现，各带一张规则表：

| 身份 | 外观 |
| --- | --- |
| `PrimeNG:Aura`、`PrimeNG:Lara`、`PrimeNG:Nora` | Light |
| `PrimeNG:Aura.Dark`、`PrimeNG:Lara.Dark`、`PrimeNG:Nora.Dark` | Dark |

六个算法都可单独使用。暗色算法接在别的算法之后时只重算颜色，尺寸沿用前一个 Map（与 Ant 的 Dark 一致）。紧凑仍用 `AtomUI:Compact`。Alias 和控件阶段的实现在同一预设的明暗两个算法之间共用，靠 `context.Appearance` 区分。

**种子语义。** `ColorPrimary` 种子是主色色阶的 500 级，明暗主题用同一个种子。现在暗色 XML 把 `ColorPrimary` 写成 emerald-400，改造后暗色主题的种子也是 emerald-500，由暗色算法映射到对应的级。

**色阶生成。** 已从官方包 `@primeuix/styled` 1.2.0 的 `palette` 实现核实：

- 色阶为 50、100、200、300、400、500、600、700、800、900、950，共 11 级，记下标 `i` 为 0 到 10。
- `i ≤ 5`：与白色混合，白色占 `19 × (5 − i)` %。所以 50 级含 95% 白，500 级就是种子本身。
- `i > 5`：与黑色混合，黑色占 `15 × (i − 5)` %。所以 950 级含 75% 黑。
- 混合在 sRGB 各通道上线性加权，结果四舍五入到整数。

**已知色板取原表。** PrimeNG 的默认色是手调的 Tailwind 色板，不等于上面公式的结果。种子等于某个 Tailwind 色板的 500 级时取该色板的原表，否则用公式生成。这样默认预设保持精确，用户自选的颜色也有合理结果。原表要从 PrimeUIX 的 primitive Token 转录，并在 README 记录来源版本；本文档没有核实这些表的数值。

**Aura 的映射表。** 「现值」一列来自现有 XML，是验收基线，必须逐值相等。其余是我的提议，实施时对照 PrimeUIX 的 Aura 预设源码核对后写进 README。

| Token | Light | Dark | 现值（Aura Light / Dark） |
| --- | --- | --- | --- |
| `ColorPrimary` | 主色 500 | 主色 400 | `#10B981` / `#34D399` |
| `ColorPrimaryHover` | 主色 600 | 主色 300 | `#059669` / `#6EE7B7` |
| `ColorPrimaryActive` | 主色 700 | 主色 200 | `#047857` / `#A7F3D0` |
| `ColorPrimaryBg` | 主色 50 | 主色 400，不透明度 16% | `#ECFDF5` / `#2934D399` |
| `ColorPrimaryText` | 主色 700 | 白色，不透明度 87% | `#047857` / `#DEFFFFFF` |
| `ColorPrimaryBgHover`、`ColorPrimaryBorder`、`ColorPrimaryBorderHover`、`ColorPrimaryTextHover`、`ColorPrimaryTextActive` | 提议：100、200、300、600、800 | 提议：按暗色对称取级 | 现由 Ant 派生，无基线 |
| `ColorText` | 中性 700 | 白色 | `#334155` / `#FFFFFF` |
| `ColorTextSecondary` | 中性 500 | 中性 400 | `#64748B` / `#A1A1AA` |
| `ColorBgContainer` | 白色 | 中性 950 | `#FFFFFF` / `#09090B` |
| `ColorBgElevated` | 白色 | 中性 900 | 未覆盖 / `#18181B` |
| `ColorFillTertiary` | 中性 100 | 中性 800 | `#F1F5F9` / `#27272A` |
| `ColorBorderSecondary` | 中性 200 | 中性 700 | `#E2E8F0` / `#3F3F46` |
| `ColorBorder` | 中性 300 | 中性 600 | `#CBD5E1` / `#52525B` |
| `ColorError` | 错误色 400 | 错误色 300 | `#F87171` / `#FCA5A5` |
| `ColorBgMask` | 黑色 40% | 黑色 60% | `#66000000` / `#99000000` |
| 成功、警告、信息各家族；其余 Fill、Bg 类 | 提议：按主色家族的同一取级规则 | 同左 | 现由 Ant 派生，无基线 |

- 中性色阶在算法里是常量：浅色用 slate，暗色用 zinc。用户要换中性色，只能显式覆盖结果 Token。
- 圆角、控件高度、字号的阶梯也由 Map 阶段写。

**Lara 和 Nora 的规则表。** 从各自现有的 8 份 XML 转录，以 XML 的值为验收基线。审阅时对照 XML 看到的差异包括：Nora 的主色取级不同（浅色 600、700、800，暗色 500、400、300），文字和边框取更深的中性级，次要边框透明，暗色焦点环和选中底色的取法不同；Lara 另外设置了 `ColorControlOutline`。这些差异都进各自的规则表，不留在 XML 里，否则换主色时它们不跟，明暗两份 XML 也无法保持一致。

**Alias 阶段（以 Aura 为例）。** 至少覆盖现在 XML 里写死的这些：`ColorFocusBorder` 等于 `ColorPrimary`；`FocusVisualBorderThickness` 为 1；`ControlOutlineWidth` 为 0；`ColorTextPlaceholder` 和 `ColorTextDescription` 等于次要文字色；`ColorBgContainerDisabled` 取中性 200（暗色 700）；`ColorTextLightSolid` 取主色上的对比色（浅色为白，暗色为中性 900）；两组阴影。`PaddingContentHorizontal` 现在浅色写 12、紧凑写 8：如果 Ant 的默认公式在这两种情况下已经给出相同的值，就不要覆盖；否则从 Map 的尺寸 Token 推出。

**控件阶段。** 把现有的逐控件覆盖搬进来。Aura 和 Lara 涉及 6 个控件：Button、CheckBox、Select、Card、Dialog、ToolTip；Nora 的暗色另外涉及 Steps。其中能从别的 Token 算出的值改为计算，例如 `ToolTipBackground` 取中性 700，暗色 Card 的 `ColorBgContainer` 和 `HeaderBg` 取中性 900，Select 的 `OptionPadding` 从尺寸 Token 推出（现在基础 profile 是 `12,8`，紧凑是 `8,6`）。

**验收基线。** 改造前，先把 12 个现有 profile 的完整编译结果（每个全局槽位、每个控件的 Own 槽位）存为快照。改造后逐槽位比较：

- 现有 XML 显式写出的 Token 必须逐值相等。
- 其余出现差异的 Token 要列成清单，逐项说明原因（例如「原由 Ant 色板派生，现按 PrimeNG 取级」或「`InputActiveShadow` 现在跟随 `ControlOutlineWidth`」），清单写进进度台账。
- 快照只用于这次改造的对比，不作为长期测试资产保留；长期保障由契约测试和现有的生效测试承担。

### 4.8 PrimeNG 预设包改造

- 每个主题 XML 只保留：算法链、种子、规则表达不了的常量。
- 12 个具名 profile 的 ID 保留。
- **暗色必须等价：** `PrimeNGAura` 把链换成 `[PrimeNG:Aura.Dark]` 后，编译结果与 `PrimeNGAura.Dark` 逐槽位相等。Lara、Nora 同理。这要求明暗两份 XML 除算法链外内容相同。
- **紧凑以等价为目标：** 基础主题的链后接 `AtomUI:Compact`，应与对应的 `.Compact` profile 相等。现有紧凑 profile 里有些值是单独写死的（例如 SM、LG 高度）。能从尺寸 Token 推出的进算法；推不出的留在紧凑 XML 里，并在 README 里列为「不等价项」。
- README 改写映射与来源一节，更新「换算法不等于选 profile」的说明。

### 4.9 Gallery

局部主题预设页增加一组主色色块。选中 PrimeNG 预设时点选色块，页面里的按钮悬停、按下、焦点环和选中底色都跟着变。这是问题 1 的直接演示，也用作人工验收场景。

### 4.10 其余三个预制算法

本文档不实施，只定规则来源和验收方式。每个算法实施前各补一页映射表，按 4.7 的格式写。

| 算法 | 规则来源 | 说明 |
| --- | --- | --- |
| Material | Material 3 的 HCT 色调色板（`material-color-utilities`） | 是真正的算法，收益最大：现在 12 个颜色全是写死的。需要移植 HCT/CAM16 计算；状态层（悬停、按下的叠加透明度）要合成为实色 |
| Bootstrap | Bootstrap 5.3 的 Sass 规则（`tint-color`、`shade-color`，按钮悬停和按下的加深比例，`-subtle` 系列） | 规则简单，与白或黑按固定比例混合 |
| shadcn | 各中性色族（zinc、slate、stone 等）的原表加角色映射 | 没有生成规则，算法是查表。每个色族一个算法身份，做法同 PrimeNG 的三个预设 |

这一节的具体内容来自我的记忆，没有在本次核实，实施前必须对照各自的官方源码。验收方式与 4.7 相同：现有 XML 显式写出的值必须逐值相等，其余差异列清单。

### 4.11 性能约束

用户的要求是：可以让 Token 自己记录写入，但不能影响性能。本节把这句话落成实现规则和验收门槛。

**为什么渲染期不受影响。** Token 对象只在主题编译期间存在。编译器创建 Token、运行公式、把结果冻结进 `TokenValueTable` 和资源表，然后丢弃 Token（`ThemeCompiler` 的控件编译段）。控件渲染和资源查找读的是冻结后的表，不经过 Token 的属性。所以 setter 的改动只可能影响主题编译和主题切换。

**实现规则：**

- **getter 必须是纯字段读取**（`get => field`），不加任何逻辑。它和自动属性生成的代码相同。
- **setter 只多一次判断。** 不记账时，setter 在写入字段之后只读一次「是否在记账」并跳过。记账状态用一个引用字段表示，空表示不记账，这样每个 Token 实例只多 8 字节，不记账时不分配任何内存。记录槽位的位图在第一次真正记账时才分配。
- **不采用「每次写入都无条件置位」的写法。** 实测它比带判断的写法慢（见下表）。
- **内置三算法不走额外流程。** 它们没有 Alias 和控件阶段，4.3 的三步流程在第 1 步结束，记账从不打开，也不多跑任何一遍默认公式。额外的公式重算只发生在使用了带这两个阶段的算法的主题上。

**小样实测。** 用一个独立的控制台程序模拟 `ToolTipToken` 的默认公式（一次计算写 6 个属性），4000 万次循环，取 9 轮中的最小值：

| 写法 | 每次计算耗时 | 相对自动属性，每次写入多出 |
| --- | --- | --- |
| 自动属性（现状） | 2.453 ns | — |
| partial 属性，setter 带「是否在记账」判断（采用） | 2.502 ns | 0.008 ns |
| partial 属性，setter 无条件置位 | 3.767 ns | 0.219 ns |

- 环境：.NET SDK 10.0.403，运行时 10.0.14，arm64 macOS，Release，JIT。
- 这同时确认了 partial 属性加 `field` 关键字在仓库使用的 SDK 上可以编译。
- 按这个数字估算，一次完整的主题编译写入约 1400 个 Token 属性，总共多出约 0.01 微秒。（这个估算是错的：实测一次冷编译约有 31,600 次写入，「约 0.01 微秒」也随之不成立，实测结果见本节末尾。）
- 局限：样例里基类和 Token 类在同一个程序集，实际代码中基类在 `AtomUI.Core`、Token 类在控件包，跨程序集能否同样内联没有测。样例也没有覆盖 NativeAOT。所以下面的门槛必须在真实仓库里量，不能拿这张表代替。

**验收门槛**（都以改造前同一台机器、同一配置的测量为基线，每项至少 10 次取中位数）：

| 项目 | 门槛 | 工具 |
| --- | --- | --- |
| 默认 Ant 主题的冷编译耗时 | 差异落在基线自身的波动范围内，且不超过 2% | 新增的主题编译基准，放在 `tests/AtomUI.Core.Tests/Theme/Performance` 或 `tools/performances/AtomUI.Performance` |
| 默认 Ant 主题明暗切换耗时 | 同上 | 同上 |
| 默认 Ant 主题编译的分配字节数 | 增量不超过「Token 实例数 × 8 字节」 | 同上，用 `GC.GetAllocatedBytesForCurrentThread` |
| 资源查找零分配、热编译复用快照 | 现有测试原样通过，不改断言 | `ThemeResourceLookupPerformanceTests` |
| 启动耗时 | 差异落在基线波动范围内 | `tools/performances/AtomUI.StartupPerformance` |
| 程序集体积、Gallery 的 NativeAOT 发布体积 | 测量并记录；增幅超过 1% 时停下来回到设计 | 构建产物对比 |

任何一项不过，先改实现（例如把记账判断的形态换掉），仍然不过就回到设计重新评估，不放宽门槛。基线要在实施第 3 步动 Token 属性之前先量好并存档。

**实测结果与用户裁决（2026-11-13）。** 真实仓库里的测量与上面的小样不一致，以真实测量为准：

- 第一次测量：冷编译慢约 6.5%。原因是 `RecordWrite` 被强制内联时，把真正记账的冷路径也带进了每个写入点，编译路径上的机器码多出约 240 KB。小样没暴露这一点，因为小样的冷路径只有一条位运算。
- 改实现之后（不记账的路径保持内联，记账那一段挪进禁止内联的独立方法）：冷编译仍然不过。按门槛所用的统计量（25 分位数）慢 2.3%–2.7%，约 0.018 毫秒；按中位数慢 3.4%–3.7%。按 25 分位数，两棵树各轮的取值范围不重叠；按中位数，两边范围有重叠。明暗切换、分配、体积、编译结果哈希都通过。
- 启动这一项按字面规则（新树的中位数落在基线取值范围内）也不过。越界大多在更快的一侧：普通窗口场景的整体启动耗时比基线快约 27 毫秒，是因为基线三次运行碰巧特别集中。变慢一侧越界的，在登记的 11 个指标里有两项：普通窗口的预编译分段高出基线最大值 0.005 毫秒；带启动屏的提交分段高出基线最大值 0.17 毫秒，差值 0.49 毫秒，小于基线自身的波动。整体启动耗时各轮之间本身相差约 12–24 毫秒（第一次测量时为 20–45 毫秒），看不出 1–2 毫秒的变化；预编译分段在补救前明确慢 1.1–1.7 毫秒，补救后为 +0.17 和 +0.8 毫秒，可能仍残留零点几毫秒的真实开销，这套测量确认不了。
- 剩余差距的来源：一次冷编译约 31,600 次属性写入，94% 是编译器内部对全局 Token 的逐属性复制（117 份副本 × 255 个属性），每次都经过带判断的 setter。
- **用户决定按当前结果通过。** 这是对本节门槛的一次明确放宽，门槛本身并没有达到。放宽的范围是这次验收里没过的两项：冷编译（上述幅度）和启动（上述字面越界）。用户作决定时看到的是冷编译的 25 分位数字（2.3%–2.7%、约 0.018 毫秒）和启动的逐项数字；中位数的 3.4%–3.7% 当时已在台账里，但没有一并交给用户；审阅指出后随即告知了用户。
- 此后的任务以这次验收时的树为新的性能基线，沿用同一套测量方法和判定规则，不得在此基础上再退化。
- 一条已知但未采用的改进：把全局 Token 的复制改成整体克隆，使那 94% 的写入不再经过 setter。它没有实测过，是否单独立项由用户决定。

## 5. 公开 API 变更

| 变更 | 影响 | 迁移 |
| --- | --- | --- |
| `ThemeAlgorithm` 从枚举改为结构体 | 二进制不兼容。源码层面：`switch` 的 `case ThemeAlgorithm.Dark`、`(int)` 或 `(ThemeAlgorithm)1` 转换、`const ThemeAlgorithm`、`Enum.*` 调用不再可用 | 用 `==` 比较；用 `ThemeAlgorithm.Dark` 这类成员取值 |
| `ThemeAlgorithmAttribute` 构造参数改为 `(catalog, id, revision, appearanceEffect)` | 自定义算法的声明要改 | 把枚举参数换成两个字符串 |
| `ThemeAlgorithmDescriptor` 构造参数和 `Algorithm` 属性的类型随之变化 | 手工构造描述符的代码 | 同上 |
| `DefaultThemeVariantCalculator.Algorithm` 等三个 `const` 字段改为 `static readonly` | 把它用在常量上下文的代码 | 改用 `ThemeAlgorithm.Default` |
| 新增 `IThemeAliasAlgorithm`、`IControlTokenAlgorithm<T>`、`ControlTokenAlgorithmAttribute`、`ThemeAlgorithmContext` | 无 | 无 |
| 四个控件包里的 148 个控件 Token 类（含 32 个抽象中间层）从 `internal` 改为 `public partial`，Token 属性改为 partial 属性 | 公开 API 面增大。Token 的名称和类型此前已通过 `XxxTokenKind` 枚举和 `ThemeBuildToken` 特性公开，这次多出的是类型本身 | 第三方控件自己的 Token 类要把类和属性都加上 `partial` |
| `DesignToken` 的 255 个全局 Token 属性改为 partial 属性 | 源码和二进制层面对调用方无变化 | 无 |
| `AbstractControlDesignToken.CalculateTokenValues` 从 `public virtual` 改为 `protected virtual` | 外部不能再直接调用；第三方控件 Token 类的 `public override` 编译不过 | 改为 `protected override` |
| 新增生成的公开类型：每个算法包一个 `<Catalog>Algorithms` | 无 | 无 |
| XML `Algorithm` 元素新增可选的 `Catalog` 属性；`Id` 接受任意合法标识符 | 无，现有文件仍然有效 | 无 |
| 全局 Alias 显式值现在带动派生 Alias（4.5） | 显式设置过 Alias Token 的主题，编译结果可能变化 | 属于修复；CHANGELOG 列出受影响的派生关系 |

以上要写进双语 CHANGELOG 和 `docs/releases` 下的 API 变更文档。

## 6. 验证

- **单元测试**：身份结构体、注册表（多包合并、重复身份、重复的控件阶段实现、追加包的接受与拒绝）、Reader 和 Binder 的两个错误边界、生成器（未知算法的构建期诊断、`<Catalog>Algorithms` 的产出、partial 属性另一半的产出、非 partial Token 属性的诊断、构建契约里的算法记录）；Token 记账（只在打开期间记录、关闭后不记录、槽位与 `XxxTokenKind` 一致、跨继承层级的槽位正确）。
- **契约测试**：4.6 的全部检查，对 Ant 三算法和 PrimeNG 六算法运行。
- **编译器测试**：4.3 的优先级（Ant 默认值 < 算法 < 显式值）；算法写入的字段带动派生字段；用户显式的字段不被算法覆盖；稳定性检查与超限诊断；`ControlAlgorithmMode` 四种模式下 Alias 阶段和控件阶段都被正确执行；4.4 表中的每种链。
- **回归**：内置三算法下，所有现有主题的编译结果逐槽位不变（4.5 引起的差异除外，单独列出）。全量回归通过。
- **生效测试**：新增一条，覆盖 `ControlOutlineWidth` 对输入框焦点阴影的影响。
- **PrimeNG**：4.7 的快照对比；4.8 的暗色等价与紧凑等价；改变主色种子后状态色跟随。
- **性能**：4.11 的全部门槛。PrimeNG 主题的编译耗时单独测量并记录，不设门槛。
- **人工验收**：4.9 的 Gallery 场景，宿主窗口用 `atom:Window`。

## 7. 实施顺序

每一步结束时构建和全量测试都要通过。

1. **身份开放。** 枚举改结构体；Attribute、描述符、注册表、规范化与缓存键、Reader、XSD、Toolchain 任务、生成器构建契约、类型转换器；包到注册表的通道。行为不变。
2. **全局 Alias 显式先行（4.5）。** 先写失败的测试确认缺口，再改。
3. **Token 记账。** 先量好 4.11 的性能基线并存档。然后：生成器产出 partial 属性的另一半和新诊断；用脚本把 `DesignToken` 和全部控件 Token 类改成 partial 属性、控件 Token 类改公开；`CalculateTokenValues` 收窄。行为不变，编译结果逐槽位不变，4.11 的门槛全部通过后才进入下一步。
4. **Alias 阶段。** 接口、上下文、编译器的三步流程、阶段越界的诊断、契约测试套件；Ant 三算法通过契约测试。
5. **控件阶段。** Attribute、注册表、追加包规则、编译器、测试。
6. **PrimeNG 算法与预设包改造。** 先存快照，再实现六个算法，瘦身 12 份 XML，做快照对比。
7. **文档与 Gallery。** 修改 `theme-definition-xml-v1.md` 第 5 节和 `runtime.md` 的相关段落；新增「编写主题算法」指南；更新 `preset-theme-packages.md`、`modules/themes/overview.md`、PrimeNG 的 README、双语 CHANGELOG、API 变更文档；Gallery 色块。
8. **后续（各自单独立项）：** Material、Bootstrap、shadcn 算法；「暗色对应算法」元数据与 Gallery 全局开关。

第 1、2、3 步互不依赖，可以分别合入。第 3 步改动的文件最多，和 `release/6.0` 上游对控件 Token 的修改最容易冲突，建议在一次变基之后立即做完并合入。

## 8. 决策

第 1 条和第 14 条是用户确认过的；其余是我做的裁决，不是用户逐条确认过的内容。第 2、3 条对公开 API 和工作量影响最大，审阅时请重点看。

| # | 决策 | 理由与备选 |
| --- | --- | --- |
| 1 | 算法直接写 Token 对象；控件 Token 类改为公开，Token 属性改为 partial 属性，由生成的 setter 记录写入（用户确认，前提是不考虑兼容性） | 只有一套类型；三个阶段的写法一致；现有按槽位工作的显式保护直接可用。初稿用的是生成的公开写入器（每个控件一个镜像类型，Token 类保持 `internal`），已放弃。其余备选：公开类但靠比对算法前后的值判断写了什么，算法写的值恰好等于默认值时识别不出来；通用写入器加带类型的键，要再造一套键，写起来不如属性赋值；只对官方主题包开放 `InternalsVisibleTo`，第三方写不了控件阶段 |
| 2 | PrimeNG 的每个预设是独立的算法身份（共六个），不给算法加参数 | Aura、Lara、Nora 的取级规则不同；写成 XML 显式值就不能跟随主色，明暗也无法等价。备选是给算法加参数机制，契约更大 |
| 3 | 链上每个算法的 Alias 和控件阶段都运行，靠上下文得知最终外观；不做「暗色对应算法」 | 规则简单，混合链有确定结果。代价是 Gallery 的全局明暗开关对 PrimeNG 主题得到的是混合链，不是 PrimeNG 的暗色 |
| 4 | Alias 和控件阶段是可选的，Ant 公式兜底 | 要求写满 117 个 Alias 和全部控件 Token，没有人写得动 |
| 5 | `ThemeAlgorithm` 保留类型名，从枚举改为结构体，不另起新类型 | 最常见的调用写法不用改；避免枚举和新身份两套 API 并存 |
| 6 | 稳定性检查最多 3 轮，超过报诊断 | 正常的算法 1 轮即稳定；不设上限会掩盖不纯的算法 |
| 7 | XML 中 `Catalog` 可省略，默认 `AtomUI` | 现有主题文件不用改 |
| 8 | 控件阶段的类可以来自任意程序集；追加包时只接受不改变已编译结果的实现 | 第三方控件包可以适配已有算法；与现有的追加包规则一致 |
| 9 | PrimeNG 的中性色阶是算法常量，不做成种子 | 不新增全局 Seed Token |
| 10 | 保留 12 个具名 profile；暗色必须与换链等价，紧凑以等价为目标并列出不等价项 | 暗色的差异都是规则，能进算法；紧凑有几处单独写死的尺寸，未必都能推出 |
| 11 | 契约测试的对比度阈值取 4.5 和 3 | 沿用 WCAG AA 的正文与大号文字阈值 |
| 12 | PrimeNG 主题的编译耗时不设门槛，只记录 | 开销只由使用该算法的主题承担，且发生在编译期，有缓存 |
| 13 | 本文档只把 PrimeNG 定到可实施，其余三个算法各自立项 | 规则来源不同，Material 需要移植 HCT，工作量差别大 |
| 14 | 不能影响性能（用户提出的硬约束）。4.11 里的具体门槛数值（2%、1%、每实例 8 字节）是我定的 | 用户只说了「不能影响性能」，没有给数值。门槛取「落在基线波动范围内」加一个上限，避免把测量噪声当成通过或失败的依据 |
| 15 | 「只写本阶段的 Token」从编译期保证退为运行时诊断加契约测试 | 直接写 Token 对象之后做不到编译期限制。Map 阶段今天也没有这个限制 |
| 16 | 不支持用字段自己的当前值算新值，靠稳定性检查报错 | 要支持就得在 getter 里加逻辑，违反 4.11 |
| 17 | 性能门槛未达到，按实测结果放行：接受冷编译慢 2.3%–2.7%（25 分位）或 3.4%–3.7%（中位数），以及启动一项的字面越界（用户于 2026-11-13 决定） | 实测数字和来源见 4.11 末尾。备选是把全局 Token 的复制改成整体克隆后重测，或退回写入器方案；用户选择直接通过 |

## 9. 留给实施者

- 4.5 中需要守卫的 Alias 字段的完整清单。
- `DesignToken` 显式位图的具体实现，以及它在 `CopyBuilder` 时是否复制。
- 记账的具体形态（`RecordWrite` 放在基类还是由生成器内联展开、记录名称的数据结构、`DesignToken` 与控件 Token 是否共用一份实现），只要满足 4.11 的规则和门槛。跨程序集内联不理想时，可以让生成器把「是否在记账」的判断直接展开在 setter 里。
- 控件 Token 类有继承层级时（例如 `ColorPickerToken` 继承 `AbstractColorPickerToken`），各层 partial 属性的槽位如何对应，沿用生成器现有的 `ControlTokenInheritanceModel`。
- 把属性批量改成 partial 的脚本，以及改完后与上游变基冲突的处理方式。
- `<Catalog>Algorithms` 所在的命名空间。
- 契约测试「两组预填值」的取值方式（每种值类型需要两个不同的样本）。
- 没有 Own Token 类的控件身份如何参与控件阶段。提议：这类控件只支持 `EvaluateGlobal`，另给一个不带类型参数的接口；如果目前没有这样的控件需要适配，可以先不做。
- 运行时从用户主题目录热重载的 XML 引用包内算法时的行为，应与引用包内控件身份的现有行为一致，实施时确认。
- PrimeNG 映射表中标为「提议」的各行、Lara 与 Nora 的规则表、Tailwind 色板原表的转录。
