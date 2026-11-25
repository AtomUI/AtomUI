# AtomUI 主题定义 XML v2 规范

本文定义 AtomUI 主题定义文件的 v2 标准格式。它是 `ThemeDocumentReader`、`ThemeDefinitionBinder`、编译主题包的
生成器、主题编辑器、CI 校验器和第三方主题包共同遵守的协议。设计依据见
[设计语言架构方案](../../superpowers/specs/2026-11-22-design-language-architecture-design.md)第 7 章。

规范配套文件：

- XML namespace：`https://atomui.net/schemas/theme/v2`
- XML Schema：[schemas/atomui-theme-v2.xsd](schemas/atomui-theme-v2.xsd)（规范性文件，与本文冲突时以 XSD 为准）
- 完整示例：[examples/daybreak-blue.theme.xml](examples/daybreak-blue.theme.xml)
- 随 Ant Design 语言包发布的主题：`src/DesignLanguages/AntDesign/AtomUI.DesignLanguages.AntDesign/Themes/DaybreakBlue.theme.xml`

本文中的“必须”“不得”“应当”和“可以”是规范性要求。v1 格式已不再被读取，换写方法见第 14 节。

## 1. 设计结论

- 一个 XML 文件只定义一个主题。XML namespace 就是格式版本，不使用额外的 `Version` 属性。
- **主题只描述配置，不描述计算。** 计算全部属于设计语言；主题只说「属于哪种语言、固定哪些轴、覆盖哪些值」。
  v2 没有 `<Algorithms>`，风格、明暗、密度是根元素上的属性。
- 主题必须声明所属语言（`Language`）。写在根上的轴是**固定**的；不写的轴是**开放**的，由最终用户的选择或作用域决定。
- 一个主题可以覆盖所有变体：明暗、密度、风格、形态不同时取值不同的部分写在 `<When>` 条件段里，不需要每个变体一份文件。
- Token 只有 `Name` 与 `Value`；类型、阶段（Seed / Map / Alias / 控件 Token）和赋值方式由 Token Schema 决定，XML 不重复声明。
  可调种子可以标记 `Locked`。
- Control 用 `(Catalog, Id)` 作稳定身份，不根据 CLR 类型名或程序集扫描推断。v1 的 Control 算法四态取消，由两条规则取代（第 6.2 节）。
- 没有 `IsDefault`：应用的初始主题由应用在启动配置里指定（`WithInitialTheme(ThemeSelection)`），不由某个文件声明。
- 文档不支持 include、import、继承另一个文件、表达式、脚本或环境变量替换。组合靠分层（第 8 节），不靠文件间引用。
- XSD 负责结构、基础词法、容量与局部唯一性；Binder 负责与注册表相关的语义校验（语言、轴的取值、Token、Control、`Locked`）。
- 主题定义文件只产生不可变的主题定义，不能直接修改在线主题资源。

主题定义文件应使用 `<theme-id>.theme.xml` 扩展名约定。文件名只用于来源定位和诊断，不参与主题身份。

## 2. 完整示例

下面的文档属于 Ant Design（`AtomUI.DesignLanguages.AntDesign`）。它在启用了桌面控件与桌面 DataGrid 的应用里可以完整绑定
（`builder.UseDesktopControls(); builder.UseDesktopDataGrid(); builder.UseAntDesign(language => language.AddDesktopDataGrid());`）：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Theme xmlns="https://atomui.net/schemas/theme/v2"
       Id="AcmeBrand"
       Name="Acme Brand"
       Language="AntDesign">
  <Tokens>
    <Token Name="ColorPrimary" Value="#0F6CBD" Locked="true" />
    <Token Name="ColorLink" Value="#0F6CBD" />
    <Token Name="BorderRadius" Value="4" />
  </Tokens>

  <Controls>
    <Control Catalog="AtomUI" Id="Button">
      <Tokens>
        <Token Name="ColorPrimaryHover" Value="#115EA3" />
        <Token Name="ContentFontSize" Value="15" />
      </Tokens>
    </Control>
    <Control Catalog="AtomUI" Id="ToolTip" Appearance="Dark" />
    <Control Catalog="AtomUI" Id="DataGrid" Density="Compact">
      <Tokens>
        <Token Name="HeaderBg" Value="#FAFAFA" />
      </Tokens>
    </Control>
  </Controls>

  <When Appearance="Dark">
    <Tokens>
      <Token Name="ColorBgContainer" Value="#0B0B0F" />
    </Tokens>
    <Controls>
      <Control Catalog="AtomUI" Id="DataGrid">
        <Tokens>
          <Token Name="HeaderBg" Value="#18181B" />
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

这一个文件定义了：

1. Ant Design 主题，风格、明暗、密度全部开放，跟随选择与作用域。
2. 主色 `#0F6CBD`，并且 `Locked`：最终用户的品牌色选择不会替换它（`ColorPrimary` 是 Ant Design 声明的可调种子）。
   `ColorLink`、`BorderRadius` 是种子，在语言推导之前生效，由它们派生的值跟着变。
3. Button 看到的 `ColorPrimaryHover`（Map）只对 Button 替换成 `#115EA3`，不触发派生；`ContentFontSize` 是 Button 的控件 Token。
4. ToolTip 始终用暗色取值，DataGrid 始终紧凑：这两个控件各自成为独立的求值单元（第 6.3 节）。
5. 暗色下容器背景与表头背景另有覆盖；移动形态下圆角加大。

## 3. 文档结构

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

元素顺序是协议的一部分：`Tokens`、`LanguageTokens`、`Controls`、`When` 依次出现，`When` 内部同样按
`Tokens`、`LanguageTokens`、`Controls` 的顺序。容器一旦出现就不能为空：空 `Tokens`、空 `LanguageTokens`、空 `Controls`
是 XSD 结构错误。`When` 与 `Control` 本身在 XSD 中允许没有子元素，它们是否为空由 Binder 判定（`ATMTHM3005`、`ATMTHM3001`）。

XSD 中的局部唯一性约束：

| 约束 | 范围 |
| --- | --- |
| `UniqueGlobalTokenName`、`UniqueLanguageTokenName`、`UniqueControlIdentity` | 基础段的 `Tokens`、`LanguageTokens`、`Controls` |
| `UniqueWhenTokenName`、`UniqueWhenLanguageTokenName`、`UniqueWhenControlIdentity` | 每个 `When` 内部 |
| `UniqueControlTokenName` | 每个 `Control` 的 `Tokens` |

同一个 Token 或 Control 可以同时出现在基础段与条件段里；那是分层覆盖（第 7 节），不是重复。

## 4. Theme

根元素必须是 v2 namespace 中的 `Theme`。

| 属性 | 必须 | 类型 | 语义 |
| --- | --- | --- | --- |
| `Id` | 是 | `Identifier` | 主题目录内稳定且唯一的身份，不从文件名推断 |
| `Name` | 是 | 1..128 字符（`xs:token`） | 面向用户显示的名称 |
| `Language` | 是 | `Identifier` | 所属设计语言的身份（如 `AntDesign`），必须已启用 |
| `Style` | 否 | `Identifier` | 写了即固定为该风格，必须是该语言声明的风格；不写则开放，缺省为语言的默认风格 |
| `Appearance` | 否 | `Light` / `Dark` | 写了即固定；不写则开放 |
| `Density` | 否 | `Identifier` | 写了即固定，必须是该语言声明的密度档；不写则开放 |

Theme `Id`、`Language`、`Style`、`Density`、Control `Catalog` / `Id`、Token `Name` 与 `When` 的条件值使用同一 Identifier 词法：

```text
[A-Za-z_][A-Za-z0-9_.-]{0,127}
```

Identifier 区分大小写，使用 ordinal 比较。`DaybreakBlue` 和 `daybreakblue` 是两个不同身份。

- 主题不声明形态（Idiom）：形态由界面根决定（桌面窗口为 `Desktop`，移动界面根为 `Mobile`）。主题可以用
  `<When Idiom="…">` 为不同形态给不同的值。
- **固定只约束面向最终用户的选择**：主题固定了明暗，`ThemeSelection.WithAppearance` 对它不生效，并在解析结果的
  `IgnoredAxes` 中标明；界面据此禁用对应开关。开发者在代码里写的 `ThemeConfig` 与局部 `ThemeConfigProvider` 不受固定约束。
- Ant Design 的轴：风格只有 `Default`；密度 `Default`、`Compact`；形态 `Desktop`、`Mobile`；可调种子 `ColorPrimary`。
  其他语言的轴由各自的语言包声明，见 `IThemeManager.Languages`。
- 每个已启用的语言在主题目录里另有一个隐式条目 `$<LanguageId>`（例如 `$AntDesign`），代表「该语言、不带主题文件」的默认外观。
  主题文件不得也无法使用以 `$` 开头的 `Id`（不符合 Identifier 词法）。

## 5. Tokens、LanguageTokens 与 Token

```xml
<Token Name="ColorPrimary" Value="#1677FF" />
```

| 属性 | 必须 | 语义 |
| --- | --- | --- |
| `Name` | 是 | Token 的稳定名称 |
| `Value` | 是 | 1..4096 字符的词法值，词法见第 9 节 |
| `Locked` | 否 | 严格布尔 `true` / `false`，默认 `false`；规则见 5.3 |

同一个容器内 `Name` 不得重复；声明顺序不影响语义。

### 5.1 Tokens：全局 Token

`<Tokens>` 覆盖全局 Token（Seed / Map / Alias，由核心声明，名称见 `AtomUI.Core` 的 `DesignToken`）。Binder 按 Schema 判定
Token 的阶段，并据此把值放进求值管线的对应位置：

- **Seed**：在语言推导之前生效。它是推导的输入，由它派生的 Map、Alias、控件 Token 都跟着变。
- **Map / Alias**：作为显式值，在语言推导之前**冻结**：推导对这些槽位的写入被忽略，读取得到显式值，所以后面由它算出的
  值也跟着变。

未注册的名称（`ATMTHM2003`）、类型转换失败（`ATMTHM3002`）都是错误。XML 不能通过 `Type`、`Stage` 一类属性改写 Schema 结论。

### 5.2 LanguageTokens：语言 Token

`<LanguageTokens>` 覆盖主题所属语言的语言 Token：该语言自己的词汇，全局 Token 里没有对应位置的角色、几何与色阶。规则与
`<Tokens>` 完全相同：其中标为种子的在语言推导之前生效，其余作为冻结的显式值生效。名称必须属于该语言的语言 Token 类，
否则报 `ATMTHM2004`。

Ant Design 的语言 Token 类 `AntDesignToken` 没有属性（全局词汇本来就是 Ant Design 的形状），所以 `Language="AntDesign"` 的主题
不能写 `<LanguageTokens>`。下面的示例只用来说明形状，`Acme` 是一个虚构的、声明了 `BaseColor`（种子）与 `Ring` 两个语言
Token 的语言，在没有启用它的应用里绑定会报 `ATMTHM2001`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Theme xmlns="https://atomui.net/schemas/theme/v2"
       Id="AcmeRose"
       Name="Acme Rose"
       Language="Acme"
       Style="Nova">
  <Tokens>
    <Token Name="ColorPrimary" Value="#E11D48" />
  </Tokens>
  <LanguageTokens>
    <Token Name="BaseColor" Value="#71717A" />
    <Token Name="Ring" Value="#FB7185" />
  </LanguageTokens>
</Theme>
```

### 5.3 Locked

`Locked="true"` 表示最终用户的选择不能替换这个种子：`ThemeSelection.WithSeed` / `WithLanguageSeed` 对它不生效，并在解析结果的
`IgnoredSeeds` 中标明；`ThemeInfo.LockedSeeds` 列出它，界面据此不提供对应色板。

`Locked` 只能写在：

- 基础段的 `<Tokens>` 或 `<LanguageTokens>` 里；
- 一个**种子**上；
- 并且该种子是语言声明的**可调种子**（`DesignLanguageAxes.AdjustableSeeds`；Ant Design 只有 `ColorPrimary`）。

违反任一条（写在条件段里、写在 Control 的 Token 上、写在 Map / Alias 上、写在非可调种子上）都报 `ATMTHM3004`。
与轴的固定一样，`Locked` 只约束最终用户的选择，不约束开发者写的 `ThemeConfig`。

### 5.4 陈旧值提示

语言可以声明每个可调种子派生出哪些 Token（`DesignLanguageAxes.SeedDerivations`）。主题没有锁定某个可调种子、却在基础段
（全局 `<Tokens>` 或 Control 的 `<Tokens>`）显式写了它的派生 Token 时，Binder 给出 `ATMTHM3006` 警告：用户换了种子之后，
那个值不会跟着变。Ant Design 没有声明派生集合，因此 Ant Design 主题不会收到这条警告。

## 6. Controls 与 Control

```xml
<Control Catalog="AtomUI" Id="Button">
  <Tokens>
    <Token Name="ColorPrimaryHover" Value="#0958D9" />
    <Token Name="ContentFontSize" Value="14" />
  </Tokens>
</Control>
```

| 属性 | 必须 | 类型 | 语义 |
| --- | --- | --- | --- |
| `Catalog` | 是 | `Identifier` | 注册该控件的命名域。AtomUI 主仓的控件为 `AtomUI`，第三方必须使用自己稳定的 Catalog，不得借用 `AtomUI` |
| `Id` | 是 | `Identifier` | 该命名域内的控件，等于公开控件类名（`Button`、`ToolTip`、`LineEdit`、`DataGrid`…） |
| `Appearance` | 否 | `Light` / `Dark` | 该控件固定使用的明暗；只能写在基础段 |
| `Density` | 否 | `Identifier` | 该控件固定使用的密度，必须是语言声明的密度档；只能写在基础段 |

- 同一个 `Controls` 容器内 `(Catalog, Id)` 不得重复。Control 必须至少有一个轴属性或一个 `Tokens`，否则报 `ATMTHM3001`。
- 未注册的 Control 报 `ATMTHM2002`。Control 的身份必须在启动时冻结的注册表里存在：XML 里出现的控件所属的控件包必须已启用。

### 6.1 一个 Tokens 集合表达两类覆盖

Control 的 `Tokens` 同时承载：

- 该控件的**控件 Token**；
- 该控件所见的任意**全局 Token**（只对这个控件生效）。

Binder 先按该控件的控件 Token 匹配，未命中再按全局 Token 分类；两处都没有报 `ATMTHM2003`。控件 Token 与全局 Token 禁止同名
（Token 声明时保证；编译主题包的生成器遇到同名会报歧义错误）。合法但未被该控件消费的全局 Token 允许配置，可能没有视觉效果。

控件级的全局覆盖只影响该控件，不改变其他控件，也不改变作用域的全局快照。语言的控件取值规则对该控件视图写入的覆盖与这里是
同一份控件视图，优先级最低：主题定义的 Control 配置高于语言推导，作用域与运行时配置又高于主题定义。

### 6.2 v1 的 Control 算法四态由两条规则取代

| v1 要回答的问题 | v2 的规则 |
| --- | --- |
| 在控件上覆盖一个种子，要不要为它重新派生？（`Disabled` / `Global`） | 永远按阶段处理：覆盖**种子**就为该控件重新求一份全局 Token；覆盖 **Map、Alias 或控件 Token** 就只替换那一个值，不触发派生 |
| 控件要不要用一条不同的算法链？（`Custom`，例如表格固定紧凑） | 在 Control 上固定轴：`Density="Compact"`、`Appearance="Dark"` |

只想替换某一个值而不触发派生时，覆盖对应的 Map、Alias 或控件 Token，而不是种子。

### 6.3 求值单元

覆盖了种子或固定了轴的控件自成一个**求值单元**：管线按它的有效变体（所在作用域的有效轴，再叠加它固定的轴）为它单独求一份
全局 Token，再求它的视图；条件段也按这个有效变体匹配（第 7 节）。只写 Map、Alias、控件 Token 的控件与所在作用域共用全局值，
只存相对全局的差量。

## 7. When 条件段

```xml
<When Appearance="Dark" Density="Compact">
  <Tokens>
    <Token Name="ColorBgContainer" Value="#141414" />
  </Tokens>
</When>
```

| 属性 | 取值 |
| --- | --- |
| `Style` | 该语言声明的某个风格 |
| `Appearance` | `Light` / `Dark` |
| `Density` | 该语言声明的某个密度档 |
| `Idiom` | 该语言声明的某个形态 |

- 至少写一个条件属性，至少有一个子容器，否则报 `ATMTHM3005`。条件的取值不在语言声明的范围内报 `ATMTHM3003`。
- 没写的属性不参与匹配；条件之间是「且」。需要「或」时多写一个条件段。
- 条件对**求值单元的有效变体**求值。求值单元是一个作用域，或一个固定了轴（或覆盖了种子）的控件。因此同一份主题在亮色根、
  其中的暗色局部作用域、固定为暗色的 ToolTip 上分别匹配，各得其值。
- 条件段里不能固定轴：不能写 Control 的 `Appearance` / `Density`（`ATMTHM3001`），也不能写 `Locked`（`ATMTHM3004`）。轴先确定，
  条件后求值，两者不互相依赖。
- 条件写了主题自身固定的轴的另一个取值时（主题固定 `Appearance="Light"` 却有 `<When Appearance="Dark">`），Binder 给出
  `ATMTHM3007` 警告：该段只会在作用域或控件显式改用暗色时生效。
- 两个条件段的条件完全相同时给出 `ATMTHM3008` 警告：后一个替换前一个的同名值。

**应用顺序。** 一次求值中，先应用基础段，再按**文档顺序**应用所有匹配的条件段，后者覆盖前者。合并规则：全局 Token 与语言
Token 按名称覆盖；Control 先按 `(Catalog, Id)`、再按 Token 名称覆盖。Control 固定的轴只来自基础段。

## 8. 与运行时配置的分层

主题定义只是取值的一层。对每个求值单元，取值从低到高依次叠加：

```text
语言推导（按该求值单元的有效变体）
<- 所选主题定义：基础段，然后各匹配的条件段（文档顺序）
<- 应用初始配置（WithInitialTheme 带的 ThemeConfig）
<- 运行时请求（含 ThemeSelection 带来的种子）
<- 父作用域的有效配置
<- 局部 ThemeConfigProvider.Config
```

每一层的种子都在语言推导之前生效；语言 Token、全局 Token、控件 Token 的显式值分别在对应推导之前冻结。

轴的确定先于取值：

```text
根：     语言默认 <- 选择（被主题固定的轴取主题的值） <- 应用初始配置 <- 运行时请求
作用域： 父作用域的有效轴 <- 本作用域 Config 中写明的轴
控件：   所在作用域的有效轴 <- 主题定义与配置中该 Control 固定的轴
形态：   界面根给出；作用域可以写明覆盖
```

明暗与密度是两个独立的轴，各自继承：父作用域是暗色、子作用域只写 `Density = Compact`，结果是暗色紧凑。

## 9. Token Value 词法

`Value` 先经过 XML 1.0 属性解码，再交给 Token descriptor 的强类型 parser。解析使用 `CultureInfo.InvariantCulture`，不依赖
当前系统语言。词法与 v1 相同：

| 目标类型 | 规范词法 | 示例 |
| --- | --- | --- |
| `string` | XML 解码后的原值，不自动 trim | `Alibaba Sans` |
| `bool` | 小写 `true` 或 `false` | `true` |
| `int` | 十进制整数，不使用分组符 | `14` |
| `double` / `float` | 有限十进制或科学计数法，`.` 为小数点 | `1.5` |
| `Color` | `#RRGGBB` 或 `#AARRGGBB` | `#1677FF` |
| `TimeSpan` | invariant constant format | `00:00:00.1000000` |
| `Thickness` | 1、2 或 4 个逗号分隔的有限数字 | `8,4,8,4` |
| `CornerRadius` | 1、2 或 4 个逗号分隔的有限数字 | `6` |
| `Point` / `Size` | 两个逗号分隔的有限数字 | `12,8` |
| enum | descriptor 生成的区分大小写名称 | `Round` |

Brush、FontFamily、BoxShadows、Easing 和其他复合值由对应 descriptor 声明 parser 与规范 formatter；第三方 Token 的 parser /
formatter 由 AtomUI 生成器写入 descriptor，不得手工注册，也不得退回 `TypeConverter` 反射发现或 `Convert.ChangeType`。
内容身份基于转换后的类型化值，不基于原始字符串。

## 10. 校验管线与诊断

```text
UTF-8 bytes
    |
    v
secure XmlReader + compiled v2 XSD        ATMTHM1xxx
    |
    v
ThemeDocument
    |
    v
ThemeDefinitionBinder + ThemeSchemaRegistry   ATMTHM2xxx / 3xxx
    |
    v
typed immutable theme definition + diagnostics
```

校验分为五层，任一层失败都不产生可发布的部分定义：

1. XML 良构：编码、标签、属性引用和 namespace 正确。
2. XSD：元素顺序、数量、属性、基础词法和局部唯一性正确。
3. 注册表绑定：语言、Control、Token 的身份存在。
4. 值转换：每个 Token 值转换为 Schema 指定的强类型值。
5. 语义校验：轴的取值、`Locked`、条件段、Control 的形状。

XSD 不能替代 Binder：它不知道当前进程启用了哪些语言、控件包和 Token，也不知道语言声明了哪些风格与密度。

每条诊断包含稳定代码与级别、来源身份、1-based 行列、元素路径（例如
`/Theme/When[1]/Controls/Control[@Catalog='AtomUI'][@Id='DataGrid']/Tokens/Token[@Name='HeaderBg']`）以及不依赖异常文本的消息。

### 10.1 Reader（结构）

| 代码 | 级别 | 条件 |
| --- | --- | --- |
| `ATMTHM1001` | Error | 不是良构 XML，或文档没有 `Theme` 根元素 |
| `ATMTHM1002` | Error / Warning | XSD 校验失败：元素顺序、数量、属性、词法、局部唯一性（XSD 校验警告按 Warning 报告） |
| `ATMTHM1003` | Error | 超出资源上限：文件字节数或元素总数（第 11 节） |
| `ATMTHM1004` | Error | 根元素不在 v2 namespace。根元素在 v1 namespace 时，消息说明 v1 不再被读取，并给出换写命令（第 14 节） |

### 10.2 Binder（绑定）

| 代码 | 级别 | 条件 |
| --- | --- | --- |
| `ATMTHM2001` | Error | `Language` 指定的设计语言未启用；此时停止绑定，不再报告其他问题 |
| `ATMTHM2002` | Error | Control 的 `(Catalog, Id)` 未注册 |
| `ATMTHM2003` | Error | 全局 `<Tokens>` 里的名称不是已注册的全局 Token；或 Control `<Tokens>` 里的名称既不是该控件的控件 Token，也不是全局 Token |
| `ATMTHM2004` | Error | `<LanguageTokens>` 里的名称不是所属语言的语言 Token |

### 10.3 Binder（值与语义）

| 代码 | 级别 | 条件 |
| --- | --- | --- |
| `ATMTHM3001` | Error | Control 在条件段里写了 `Appearance` / `Density`；或 Control 既没有轴属性也没有 Token |
| `ATMTHM3002` | Error | Token 值不能转换为 Schema 指定的类型 |
| `ATMTHM3003` | Error | `Style`、`Density`（根、Control、条件段）或条件段的 `Idiom` 不是语言声明的取值 |
| `ATMTHM3004` | Error | `Locked` 写在条件段里、Control 的 Token 上、非种子或非可调种子上 |
| `ATMTHM3005` | Error | 条件段没有任何条件属性，或没有任何子容器 |
| `ATMTHM3006` | Warning | 未锁定某个可调种子，却在基础段显式写了语言声明的、由它派生的 Token（5.4） |
| `ATMTHM3007` | Warning | 条件与主题自身固定的风格、明暗或密度矛盾 |
| `ATMTHM3008` | Warning | 两个条件段的条件完全相同 |

v1 的 `ATMTHM3101`–`ATMTHM3108`（设计语言分组、品牌色与中性色的 Seed / Fixed、变体重复）随对应元数据一起废止，代码不复用。

### 10.4 其他分段

| 范围 | 责任 |
| --- | --- |
| `ATMTHM4xxx` | `ThemeConfig` 规范化（`ATMTHM4001`–`4007`：未知 Token、Control、语言，轴取值，值转换）、主题目录、用户主题目录与跨来源冲突 |
| `ATMTHM5xxx` | 主题编译：语言缺失、变体不受语言支持、未知语言 Token |
| `ATOMUITOKEN002` | 编译主题包在构建期绑定失败（生成器报告，带 XML 行列） |

## 11. 安全与资源边界

Reader 必须满足：

- `DtdProcessing = Prohibit`，`XmlResolver = null`。
- `ValidationType = Schema`，启用 `ProcessIdentityConstraints` 与 `ReportValidationWarnings`；不启用 `ProcessInlineSchema` 或
  `ProcessSchemaLocation`，只使用 `AtomUI.Core` 内嵌并预编译的 v2 XSD。
- 不读取 namespace URI 或 `xsi:schemaLocation` 指向的网络资源，不展开外部实体，不执行 XInclude，不解释处理指令。
- XSD 在首次使用时编译一次，之后复用同一个不可变 schema set。
- 使用 validating `XmlReader` 单次流式生成 `ThemeDocument`，不为正常路径构建 `XDocument`。

默认上限：

| 项目 | 上限 |
| --- | --- |
| 单文件 UTF-8 字节数 | 4 MiB |
| XML 元素总数 | 65536 |
| 每个 `Tokens` / `LanguageTokens` 容器的 Token | 4096 |
| 每个 `Controls` 容器的 Control | 1024 |
| 条件段 `When` | 64 |
| Identifier 长度 | 128 字符 |
| `Name`（显示名称）长度 | 128 字符 |
| Token Value 长度 | 4096 字符 |

`ThemeDocumentReaderOptions` 可以调低字节数与元素数的运行时上限，但不能高于默认值。触发限制返回 `ATMTHM1003`，不静默截断。
v1 的「单算法链 64」上限随算法链一起取消。

用户主题目录中的主题不能加载代码、程序集、脚本或网络资源；目录的枚举、容量与路径安全规则见
[主题系统架构](../../architecture/systems/theming/runtime.md)。

## 12. 规范化与内容身份

Reader 保留源码位置和声明顺序；Binder 输出类型化的不可变定义。内容指纹基于规范化后的类型化结果：

- namespace 版本与主题元数据（`Id`、`Name`、`Language`、固定的轴、锁定的种子）；
- 语言身份与语言修订；
- 按槽位排序的全局 Token 与语言 Token；
- 按身份排序的 Control 配置；
- **按文档顺序**的条件段；
- 注册表修订。

空白、属性顺序、注释、Token 与 Control 的声明顺序不影响指纹；条件段的顺序影响指纹（它决定覆盖顺序）。语言程序集与取值包
程序集的修订取自确定性构建的程序集标识，推导代码一变它就变，缓存据此失效；不需要手工维护 `revision`。

指纹与来源摘要只用于定位缓存候选，命中后仍比较不可变的来源 key 与类型化结构 key。

规范 writer 应使用 UTF-8、两个空格缩进、双引号属性和本文定义的元素顺序，并按 Token 名称与 Control `(Catalog, Id)` 做 ordinal
排序；条件段保持作者的顺序。

## 13. 来源与编译

同一套语义适用于所有来源：

| 来源 | 注册 | 可刷新 | 失败处理 |
| --- | --- | --- | --- |
| 编译进包的主题（`AtomUIThemeDefinition` 构建项） | 生成的 `ThemePackageRegistration` | 否 | 属于损坏的发布产物，构建或启动失败 |
| 应用或包内的 `avares://` XML | 显式列出的 Resolver | 否 | 同上 |
| 用户主题目录（`*.theme.xml`） | `UseUserThemeDirectory()` | `ReloadThemesAsync()` 手动刷新 | 首次加载失败不阻止启动；整个目录是一个刷新单元 |
| 程序化来源 | 自定义 `IThemeDefinitionResolver` | 由来源声明 | 按来源类型 |

- 任意来源之间重复的 `Theme.Id` 是错误，没有先到先得或后者覆盖。
- 编译进包的主题在构建期用同一个 Reader 与 XSD 校验，并对照所引用程序集携带的构建契约（语言身份与语言 Token、全局 Token、
  各控件包的 Token）完成绑定，生成类型化的主题程序；运行时不解析 XML。条件段编译为对变体的分支，种子、Map、Alias、控件 Token
  保留各自的阶段。生成接口见 [编译主题 ABI v4](compiled-theme-abi-v4.md)。
- 当前所选的具名主题因刷新而消失时，回到「同一语言、同样的变体、不带主题」（`$<LanguageId>`），而不是换成另一个主题。

## 14. 从 v1 换写

Reader 不兼容 v1：v1 文档在 Reader 边界失败，报 `ATMTHM1004`。现有主题文件用工具链的转换命令一次性换写：

```bash
dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=<directory> [-p:AtomUIThemeDefinitionOutput=<directory>]
```

- 命令读取输入目录顶层的全部 `*.theme.xml`，按下表换写为 v2；输出目录省略时原地替换，并删除被合并进其他文件的变体文件。
- 同一语言、同一风格的多个变体文件（亮色、暗色、紧凑、暗色紧凑）合并为一个文件：所有变体一致的值留在基础段，其余写进带
  `Appearance` 与 `Density` 条件的 `<When>`；保留下来的是亮色、默认密度的那个文件名。只有一个文件时保留它算法链所固定的轴。
- 每个输出文件写出后都用 v2 Reader 回读；回读失败的文件作为错误报告。
- 需要作者判断的条目以 `ATMTHM1005` 警告报告，不自动改写；结构性问题以 `ATMTHM1004` 错误报告。

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

换写后的文件引用的语言、风格、Token 与 Control 仍须在应用的注册表中存在；转换命令不访问注册表，绑定问题在下一次构建或加载时报告。

## 15. 版本演进

v2 namespace 与 XSD 发布后保持不可变，只允许不改变验证结果的文档澄清。破坏性格式变更必须使用新 namespace，并提供独立 XSD、
Reader 与换写命令。Reader 只接受显式注册的 namespace，不猜测版本，不把无 namespace 文档当作 v2，也不建立旧格式 adapter。

## 16. 一致性验收

主题定义实现必须具备以下测试：

- XSD 自身可以被 .NET `XmlSchemaSet` 编译；标准示例通过 XSD 和 Binder。
- 未声明 namespace、v1 namespace、错误元素顺序、未知属性、空容器和重复身份被拒绝；v1 文档的诊断给出换写命令。
- 未启用的语言、未声明的风格 / 密度 / 形态、未知 Control / Token / 语言 Token、非法 Value 产生带行列及路径的诊断。
- `Locked` 的每种非法位置都被拒绝；锁定的种子不被选择替换。
- 条件段按求值单元的有效变体匹配，按文档顺序覆盖；控件固定轴后自成求值单元。
- 控件级覆盖种子会重新派生，覆盖 Map / Alias / 控件 Token 只替换该值。
- DTD、外部实体、超限文件和超限元素数量在发布定义前失败。
- 相同类型化内容的不同 XML 排版得到相同指纹；条件段顺序不同则指纹不同。
- 编译主题包与动态 XML 对同一文档得到相同的绑定结果与指纹。
- Schema 校验、Reader 和 Binder 的正常路径通过 NativeAOT 验证，不执行反射扫描或动态代码生成。
