# AtomUI 主题定制：精确生效与设计语言模拟

日期：2026-11-07。状态：最终设计方案，待审阅；未开始实现。
基线：AtomUI `release/6.0`（`ae5ac0a32`，Avalonia 12.1.5）。参照：Ant Design 6.7.0 的主题配置模型。

## 1. 目标与边界

AtomUI 自始至终是 Ant Design 设计语言，不引入设计语言抽象层。本方案只做两件事：

1. **证明可模拟**：在 Ant Design 框架内，只通过主题定义（Token、逐控件 Token、算法）模拟出其他设计语言的大致样式，以 NuGet 主题包交付，并在 Gallery 里像 ant.design 首页那样切换。首批包是 **PrimeNG**（重点，含 Aura、Lara、Nora 三个预设）、shadcn、Bootstrap、Material。不追求复刻细节；凡是需要改控件结构或代码才能达到的效果，明确列为不支持。
2. **保证精确生效**：凡是我们声称可定制的 Token，设置后必须在渲染层精确体现。不能出现"主题设置了，但没有效果"。

不做：lookless 全量重构、契约部件、按语言切换 ControlTheme、设计语言包、合并 `feature/custom-theme`。主题编辑器是独立工具，另立项目，不在本次范围；本方案只保证它将来需要的数据（承诺面、消费图）由生成器产出。

## 2. 现状

### 2.1 已有能力

release/6.0 的主题配置模型与 Ant Design 的主题配置一一对应：

| Ant Design | AtomUI |
| --- | --- |
| `token` 全局 Token 覆盖 | `ThemeConfig.Tokens`；XML `<Tokens>` |
| `components[Comp]` 逐组件 Token，含 `algorithm` | `ThemeConfig.Controls`；XML `<Control Catalog Id Algorithm="Unspecified/Disabled/Global/Custom">` |
| `algorithm` 默认 / 暗色 / 紧凑可组合 | `Algorithms`，三者可组合 |
| 组件 Token 的默认是全局 Token 的纯函数，放在组件旁边 | `XxxToken.CalculateTokenValues()`，与控件同目录 |
| 嵌套 `ConfigProvider`、`inherit` | `ThemeConfigProvider`、`Inherit` |
| 运行时切换 | `ApplyThemeAsync`，事务式提交 |
| 主题编辑器 | 无（Gallery 只有写死的示例配置） |
| 预设 | Core 内置 DaybreakBlue；Gallery 自带 4 个色板主题（PolarGreen、SunsetOrange、GoldenPurple、Magenta） |

还多了 antd 没有的：主题 XML 文件与用户主题目录热重载、**编译主题包**（项目里声明 `<AtomUIThemeDefinition Include="x.theme.xml"/>`，生成器产出类型化注册与 `Register(builder)`，经 `AddCompiledThemePackage` 进入同一 Catalog；Gallery 的 4 个主题就是这样做的）、局部作用域事务、Semantic Part。编译主题包就是现成的"主题 NuGet 包"机制。

术语对照：antd 的 component token 对应 AtomUI 的 **Own Token**（控件专属字段）；antd 的 `components.Button.colorPrimary` 这种"只对某组件覆盖的全局 Token"对应 AtomUI 该控件的**有效全局 Token**，主题里用 `ButtonTokenResource ColorPrimary` 读到的就是它。

### 2.2 精确性缺口（对 release/6.0 源码的实测）

| # | 缺口 | 数据 | 根因 |
| --- | --- | --- | --- |
| A | Own Token 覆盖不传导到依赖它的默认值 | `ThemeCompiler` 先 `descriptor.Evaluate`（即 `CalculateTokenValues`）再 `ApplyAllValues(config.OwnTokens)`；5 个 Token 类共 22 处用 `IsNaN` 判断"是否显式设置"，但求值时覆盖尚未写入，永远判为未设置 | 覆盖写入晚于默认计算 |
| A′ | 同一根因的具体案例 | `DataGridToken.CalculateTokenValues` 里 `TableBorderColor = BorderColor`，主题消费的是 `TableBorderColor`；用户设置 `BorderColor` 后无任何效果 | 同上 |
| B | 逐控件的全局 Token 覆盖对一半以上的消费点无效 | 控件主题 AXAML 中 `SharedTokenResource` 1917 处、`XxxTokenResource` 1643 处；`Shared` 永远读全局值，`Controls[Button].ColorPrimary` 对那 76 处 `SharedTokenResource ColorPrimary` 无效 | 见下文结构性原因 |
| C | 声明了但没人消费的 Own Token | 96 个 Token 类 1150 个字段中约 100 个在所属包内无任何引用（DataGrid 的 20 个经内部派生字段间接消费，仍受 A′ 影响） | 字段随文档声明，消费未跟上，或命名不一致 |
| D | 主题 AXAML 里的视觉字面量 | 134 处数值字面量，颜色字面量 0 处。集中在 OverlayDialogResizer（12）、DataGridRowGroupHeader（11）、SplitView（8，分隔线 `Width="1"`）、NumericUpDownSpinner（8）、ScrollBar、Alert、ProgressBar 系列 | 约一半是布局常量，另一半（线宽、分隔线、圆角 0、字号）应是 Token |
| E1 | 代码里写死的颜色 | `BadgeColorUtils` 用 14 个 RGB 常量定义 Badge 预设色，不读主题色板，暗色下也不变；Tag 的自定义色用固定亮度 0.95 算浅底；其余 127 处命中几乎全是 `Colors.Transparent`（合法） | 绕过了主题的 `PresetPalettes` |
| E2 | 代码里写死的几何 | `CalendarRangeBarPanel` 的字号 12、高度 20、内边距 8；`SwitchKnob` 加载指示笔宽 1；共 12 处视觉常量，其余是行为常量属合法 | 应改为 Own Token 或全局 Token |
| E3 | 绕过作用域读资源 | `NavMenu` 用 `Application.Current.TryGetResource` 读 `MotionDurationMid`，失败回落写死的 200 ms；`TabItem` / `TabStripItem` 经 `Application.Current` 查命名主题；`DataGridColumn` 查不到时回落 Application；共约 6 处 | 局部 `ThemeConfigProvider` 的覆盖对这些读取无效 |
| E4 | 代码侧计算视觉值的模式 | 111 处代码赋视觉属性，38 个自绘。Button 的做法是对的：`ControlTokenAccessor` 读控件有效值，`ThemeTokenResolver.Subscribe` 在主题变化时重算 | 模式可用但没有统一约束 |

A、B 是机制缺口，C、D、E 是逐控件的消费缺口。控件代码确实需要优化，但范围是可枚举的有限清单，不是重写；Button 这类核心控件的代码模式已经正确，问题集中在少数控件和历史遗留处。

**缺口 B 的结构性原因**：`InputControlFrame`（90 处）、`AddOnDecoratedBox`（29 处）这类没有自己 Token 身份的共享基元被多个公开控件的模板复用，它们的主题写不了 `LineEditTokenResource`，只能读共享全局值，于是用户对 LineEdit 的逐控件覆盖传不进输入框的边框、圆角与内边距。这不是作者疏忽，是缺一个"读当前所属公开控件的有效值"的机制。

### 2.3 从 `feature/custom-theme` 借鉴的修复与规则

该分支不合并（修复与样式资产子系统、V2 求值、50 个新增 Own 字段缠在同一批提交里）。逐提交核对后，以下修复**真的让主题设置生效且不依赖被删机制**，按思路在 release/6.0 上重做（"依赖"是其 diff 引用分支专有机制的次数）：

| 提交 | 控件 | 修的是什么 | 依赖 |
| --- | --- | --- | --- |
| `473c43b1f` | NavMenu | 分组标题颜色写在模板属性上（LocalValue 优先级），遮蔽了暗色样式的 Setter；移入 Style Setter | 0 |
| `9f4173894` | Steps | 面板箭头描边同一问题，Small 尺寸细描边不生效 | 2 |
| `b1a6fc37a` | Splitter | 分隔条尺寸在测量时一次性 `FindTokenResource`，运行时换主题不重测；改为挂载期绑定属性并 `AffectsMeasure` | 0 |
| `e9e49500f` | FloatButton | `IsActive = …` 以本地值覆盖了用户绑定；改 `SetCurrentValue` | 1 |
| `8af4c1678` | AddOnDecoratedBox | 状态色直接写到附加区呈现器，擦掉用户画刷；改为可撤销的 Template 优先级值 | 0 |
| `daa17c067` | ToolTip | 预设色改为 internal 属性，由主题的无触发 Setter 投射到 `Background`，应用样式与本地值保持更高优先级；色板读快照 | 2 |
| `9ba3924ab` | Tag | 同上模式；预设色板按快照缓存 | 3 |
| `5e587b055` | ProgressBar | 文字几何最小尺寸从 `SetValue(…, Template)` 改为内部属性由主题投射，保留用户 Min 约束 | 2 |
| `b3f82db2f` | Button | 变体画刷经资源键写入以支持作用域覆盖；订阅 `ResourcesChanged` 重算 | 1 |
| `0b4725a24` | DropdownButton | 动效开关改读 owner 有效全局值 | 3 |
| `9c01e0cc7` | Tour | Popup 间距 Token 绑定在打开期间随 Gap 重建 | 0 |
| `fe9c9c583`、`33534c625`、`92631f292` | Badge、Popup、DataGrid | 生命周期与几何正确性修复 | 0–1 |

不借实现、只借问题清单的：`1e690fc2d`、`33c686f3f`（TextBox 字号预设、Button 字重继承，依赖分支新建的 `ControlTypographyBindings`；排版继承问题进 E4 审计）；`417d206fb`、`d9b9696f1`、`aa250220a`、`def92ef74`、`7cea29ce7`（输入与按钮家族、DataGrid、Progress、Steps 的大批"接通主题消费"，与 V2 同提交；它们改动的 `token.md` 就是各控件"哪些 Token 过去没被消费"的清单，用于 3.1 的字段裁决）。该分支的编译器顺序修正与本方案 3.2 一致；其 `OwnerTokenResource` 证明了缺口 B 的范围（`SharedTokenResource` 1917 → 164），但靠跳链观察，本方案改用继承属性（3.3）。`BadgeColorUtils` 该分支未修。

从这些修复提炼出五条可推广的规则：

- **P1** 需要被状态 Setter 覆盖的默认值不能写在模板节点的属性上（LocalValue 优先级），必须写成 Style Setter。
- **P2** 代码里不得一次性 `FindResource` 读 Token 后缓存；改为挂载期绑定到属性，变化驱动重算或重测。
- **P3** 代码写入控件或子节点的值必须用 `SetCurrentValue` 或可撤销的 Template 优先级值，不得以本地值覆盖用户与主题的绑定。
- **P4** 代码算出的视觉值写入 internal 属性，由主题 Setter 投射到公开视觉属性；公开属性留给用户与应用样式。
- **P5** 预设色从快照的 `PresetPalettes` 取并按快照缓存，不在控件实例里重算或写死。

## 3. 方案

### 3.1 承诺面：什么叫"我们声称可定制"

承诺面 = 全部全局 Token + 每个控件的 Active Own Token。每个 Own Token 字段必须属于以下之一，由生成器登记：

| 分类 | 含义 | 要求 |
| --- | --- | --- |
| Active | 有真实消费者 | 所属包的主题 AXAML 或代码至少引用一次；生效测试（3.6）必须通过 |
| Reserved | 暂无消费者，有保留理由 | 字段加 `[ReservedToken("原因")]`；编辑器与文档标为保留，不出现在"可调"列表 |
| 删除 | 既无消费者也无理由 | 删除字段；进入发布说明 |

生成器对每个 Own 字段做引用分析：未引用且未标 Reserved → 构建错误。约 100 个待裁决字段在第 6 节第 2 步逐个处理，custom-theme 分支各控件 `token.md` 的改动作为核对清单。

全局 Token 不要求每个控件都消费，但生成器产出**消费图**（哪个控件的主题与代码消费了哪个全局 Token），供编辑器显示"改这个 Token 影响哪些控件"，供文档生成。

### 3.2 修复 A：Own Token 覆盖先于默认计算

```text
builder = descriptor.CreateBuilder()
builder.AssignEffectiveGlobalToken(effectiveGlobal)
builder.MarkExplicit(config.OwnTokens)            // 写入显式值并置"显式位"
descriptor.Evaluate(builder, appearance)          // CalculateTokenValues 只为未显式的字段算默认
builder.ApplyExplicit()                           // 再写一遍显式值，无论求值器怎么写都以显式为准
```

`AbstractControlDesignToken` 增加 `protected bool IsExplicit(XxxTokenKind kind)`；求值器用它代替 `IsNaN` 哨兵（NaN 对 Color、Thickness 等类型不可用）。分析器禁止新的 `IsNaN` 哨兵，存量 22 处改写。求值器对字段的默认赋值是无条件的，所以"派生字段跟随显式值"要求**被派生的基字段**的默认赋值用 `IsExplicit` 守卫（`if (!IsExplicit(Kind.BorderColor)) BorderColor = …;`）；只被消费、不被其他字段读取的字段不需要守卫，最后一步重写显式值已经保证它。分析器（`ATOMUIGEN048`）报告"同一方法内赋值后又被读取、却没有守卫"的字段，存量一次改写。内部派生字段（如 DataGrid 的 `Table*`）由此从已含显式值的公开字段派生，A′ 随之修复。这是对现有 `CalculateTokenValues` 的最小修正，不引入依赖图、求值版本或别名机制。

### 3.3 修复 B：控件主题只读"控件有效全局值"

| 主题的 TargetType | 全局 Token 的读法 | 数量 |
| --- | --- | --- |
| 有 Token 身份的公开控件（Button、LineEdit、Steps…） | `{atom:XxxTokenResource Name}`，读自己的有效全局值 | 约 1700 处 |
| 没有 Token 身份的共享基元与部件（InputControlFrame、AddOnDecoratedBox、各类 Item / Cell / Presenter） | `{atom:OwnerTokenResource Name}`，读**当前所属公开控件**的有效全局值 | 约 200 处 |

`SharedTokenResource` 只允许出现在应用级样式和明确跨控件共享语义的场合，后者在 AXAML 上以注释标注理由。分析器按 TargetType 是否有 Token 身份判断该用哪种，用错先警告，迁移完成后转错误。

`OwnerTokenResource` 的机制：

- 每个有 Token 身份的公开控件实例带有可继承附加属性 `ThemeScope.TokenOwner`，值是其 identity。写入点是 `ThemeManager` 已有的 ControlTheme 解析钩子：默认 ControlTheme 首次解析时按 TargetType 查 registry，有 descriptor 就注入一个 `Setter(TokenOwner, identity)`（Style 优先级的有效值参与继承；每个 ControlTheme 只注入一次）。控件作者与生成器都不写代码。用户不经 `BasedOn` 完全替换默认 ControlTheme 时失去该 Setter，基元回落全局值。
- 基元与部件在模板内继承到这个值；Popup 内容经逻辑父级继承，独立窗口经现有 lease 继承。
- `OwnerTokenResource` 产出一个绑定：输入是继承到的 owner identity 与 Token kind，输出是作用域 Provider 对 `ControlTokenResourceKey.Global(identity, kind)` 的查询结果；owner 变化或主题变化都触发重取。
- 没有继承到 owner（基元被单独使用）时回落为共享全局值。

与 custom-theme 分支沿 `TemplatedParent → Parent` 跳链并观察每一跳相比：继承属性由 Avalonia 原生传播，无需路由观察，天然跨 Popup 边界，每个绑定只多一个对继承属性的订阅。嵌套的公开控件（SearchEdit 里的 Button）写入自己的 identity、读自己的有效值，与 antd 一致。

替换后 `ThemeConfig.Controls[X].Tokens[全局名]` 对该控件的全部消费点生效，包括其模板里的共享基元，与 antd `components[Comp]` 同义。约 1900 处替换按控件家族分批，每批跑该家族的生效测试。

性能：`XxxTokenResource` 查询路径已存在且零分配，控件级无覆盖时走 `UsesRootSharedResources` 直接读全局表，与 `Shared` 成本相同；`OwnerTokenResource` 多一个继承属性订阅，只用于约 200 处。

### 3.4 修复 C、D、E：逐控件消费审计

控件代码与主题的七条规则，写进分析器：

1. 视觉值（颜色、画刷、阴影、线宽、圆角、字号、尺寸）不得以字面量出现在控件代码里；`Colors.Transparent` 与 0 允许。
2. 读 Token 只经 `ControlTokenAccessor` 或 `XxxTokenResource` / `OwnerTokenResource`；禁止 `Application.Current.TryGetResource` / `TryFindResource` 读 Token，禁止读取失败时回落写死的值。
3. （P2）由 Token 算出的视觉值必须在主题变化时重算（`ThemeTokenResolver.Subscribe` 或挂载期绑定），不得算一次就缓存。
4. （P5）预设色（Badge、Tag、ToolTip、Button 的 preset color）从快照 `PresetPalettes` 取并按快照缓存。
5. （P1）主题里需要被状态 Setter 覆盖的默认值必须写成 Style Setter，不能写在模板节点的属性上。
6. （P3）代码写入控件或子节点的值用 `SetCurrentValue` 或可撤销的 Template 优先级值。
7. （P4）代码算出的视觉值写入 internal 属性，由主题 Setter 投射到公开视觉属性。

按控件家族推进，每个家族一次性处理：

- C：每个未引用 Own 字段裁决为 Active（补消费）、Reserved（加理由）或删除。
- D：134 处 AXAML 字面量逐个判断：布局常量保留并加 `<!-- layout -->` 注释（分析器据此放行），视觉值改为 Token；分隔线与边框线宽统一用 `LineWidth`。
- E1：`BadgeColorUtils` 改读 `PresetPalettes`；Tag 自定义色的浅底改用现有调色板算法。
- E2：`CalendarRangeBarPanel`、`SwitchKnob` 等 12 处几何常量改为 Own Token 或全局 Token。
- E3：`NavMenu`、`TabItem`、`TabStripItem`、`DataGridColumn` 等约 6 处改经作用域读取，删除写死的回落值。
- E4：逐控件核对代码侧视觉计算是否订阅主题变化、是否读控件有效值；不符合的改成 Button 的模式；排版继承（字号、字重）一并核对。
- 2.3 清单里该家族的借鉴修复一并落地。

每个家族的验收是该家族的生效测试全部通过（3.6）。

### 3.5 预设：以主题 NuGet 包交付

预设用现有编译主题包机制交付，一个预设一个 NuGet 包，引用即用；Core 仍只内置 DaybreakBlue。每个包是一个小项目：

```text
AtomUI.Themes.Shadcn/
+-- AtomUI.Themes.Shadcn.csproj     引用 AtomUI.Core；<AtomUIThemeDefinition Include="Themes/*.theme.xml" />
+-- Themes/ShadcnZinc.theme.xml     zinc 中性色、主色近黑、BorderRadius 6、ControlHeight 36、细边框、
|                                   EnableWaveSpirit=false、弱阴影；逐控件 <Control> 覆盖按需
+-- Themes/ShadcnSlate.theme.xml …  同一语言的其他色系
+-- ShadcnThemes.cs                 UseShadcnThemes() 扩展：调用生成的 Register(builder)
\-- README.md                       模拟了什么、放弃了什么
```

生成器把 XML 编译为类型化注册，运行时不读 XML；注册进同一 Catalog，出现在 `AvailableThemes`，与用户目录主题、DaybreakBlue 走同一事务。暗色由现有 Dark 算法从该主题的 Seed 派生；需要时同包可放 `Appearance="Dark"` 的独立定义。

| 包 | 模拟 | 明确不支持（写进 README） |
| --- | --- | --- |
| `AtomUI.Themes.PrimeNG` | Aura（默认）、Lara、Nora 三个预设，见 3.5.1 | ripple、浮动标签、以透明度表达禁用、Filled 字段形态 |
| `AtomUI.Themes.Shadcn` | zinc / slate / stone 三色系，近黑主色，细边框，无 wave，弱阴影 | 焦点环形态、Command 式弹层 |
| `AtomUI.Themes.Bootstrap` | `#0d6efd` 主色、`FontSize 16`、`ControlHeight 38`、`BorderRadius 6` | 渐变背景、内阴影 |
| `AtomUI.Themes.Material` | 紫色主色、`BorderRadius 4`、`ControlHeight 40`、三级 BoxShadow 近似 elevation | ripple、浮动标签、全大写 |

Gallery 现有 4 个色板主题可迁成 `AtomUI.Themes.AntColors` 包，是否迁移由用户定。第三方做自己的预设走完全相同的路径。这些包证明的命题是：Ant Design 的 Token 词汇足以表达这些语言的大致观感；表达不了的部分写在 README 里，不改控件。

#### 3.5.1 PrimeNG：重点预设

依据 `@primeuix/themes` 源码（`presets/{aura,lara,nora}/base/index.ts` 与 button、inputtext、checkbox、select、card、dialog、tooltip 的组件 Token；文档 primeng.dev/theming，v22）。PrimeNG 的 Token 分三层：primitive（22 个色板，每个 50 到 950 十一阶，加 `borderRadius none/xs/sm/md/lg/xl = 0/2/4/6/8/12px`）、semantic（primary、surface、colorScheme.light/dark 下的 text / content / overlay / formField / list / navigation / highlight / mask）、component（逐组件，大多引用 semantic）。这与 AtomUI 的 Seed / Map / Alias / Own 分层可以逐项对应，PrimeNG 本身不派生、全部直接给值，所以预设就是一张值表加少量逐控件覆盖。

默认色板：primary = emerald，surface 浅色 = slate，深色 = zinc。基准字号按 PrimeNG 的 `-compat`（14px 根字号）变体取值，与 AtomUI 的 14px 一致。

**语义 Token 到 AtomUI 全局 Token 的映射（Aura 浅色）**：

| PrimeNG | 值 | AtomUI Token |
| --- | --- | --- |
| primary.color / hoverColor / activeColor | emerald 500 `#10b981` / 600 `#059669` / 700 `#047857` | `ColorPrimary` / `ColorPrimaryHover` / `ColorPrimaryActive` |
| primary.contrastColor | `#ffffff` | `ColorTextLightSolid` |
| highlight.background / color | emerald 50 `#ecfdf5` / 700 | `ColorPrimaryBg` / `ColorPrimaryText` |
| text.color / mutedColor | slate 700 `#334155` / 500 `#64748b` | `ColorText` / `ColorTextSecondary`、`ColorTextDescription` |
| content.background / hoverBackground / borderColor | `#ffffff` / slate 100 / slate 200 `#e2e8f0` | `ColorBgContainer` / `ColorFillTertiary` / `ColorBorderSecondary` |
| formField.borderColor / hoverBorderColor / focusBorderColor | slate 300 `#cbd5e1` / slate 400 / primary | `ColorBorder` / 悬停边框见"差异" / `ColorPrimary` |
| formField.placeholderColor / disabledBackground | slate 500 / slate 200 | `ColorTextPlaceholder` / `ColorFillTertiary` |
| formField.invalidBorderColor | red 400 | `ColorError` |
| borderRadius xs / sm / md / lg | 2 / 4 / 6 / 8 px | `BorderRadiusXS` / `BorderRadiusSM` / `BorderRadius` / `BorderRadiusLG` |
| formField.paddingX / paddingY | 12 / 8 px | `PaddingContentHorizontal 12`；高度由 paddingY 推 `ControlHeight 40`（sm 36，lg 48） |
| formField.sm / lg fontSize | 0.875rem / 1.125rem | `FontSizeSM 12`（compat）/ `FontSizeLG 16` |
| focusRing（按钮）width / color / offset | 1px solid / primary / 2px | `FocusVisualBorderThickness 1` / `ColorFocusBorder`；字段无 outline → `ControlOutlineWidth 0` |
| transitionDuration | 0.2s | `MotionDurationMid 200ms` |
| overlay.select / popover shadow | `0 4px 6px -1px rgba(0,0,0,.1), 0 2px 4px -2px rgba(0,0,0,.1)` | `BoxShadowsSecondary` |
| overlay.modal shadow / radius | `0 20px 25px -5px …` / xl 12px | `BoxShadows` / Dialog 的 Own 圆角 Token |
| mask.background | `rgba(0,0,0,0.4)` | `ColorBgMask` |

**逐控件覆盖（`<Control>`）**：Button `FontWeight 500`、圆角 6；Checkbox 尺寸 20、圆角 4、图标 14；Select 弹层选项内边距 8 / 12、选项圆角 4、列表内边距 4、间距 2；Card 圆角 12、阴影 `0 1px 3px 0 rgba(0,0,0,.1), 0 1px 2px -1px rgba(0,0,0,.1)`、内边距 20；Dialog 圆角 12、内边距 20、标题 20px / 600；Tooltip 背景 slate 700、圆角 6、内边距 8 / 12。

**三个预设的差异**，分别落在各自的 XML：

| | Aura | Lara | Nora |
| --- | --- | --- | --- |
| 圆角 | 字段 6、选项 4、卡片与对话框 12 | 字段 6，选项与导航项 0 | 几乎全部 2 |
| 密度 | paddingY 8，Checkbox 20 | paddingY 10，Checkbox 24，按钮 paddingX 16 | 列表最紧（`0.125rem 0`） |
| 字重与对比 | 按钮 500，文字 slate 700，边框 slate 300 | 按钮 600 | 按钮 700，文字 slate 900，字段边框 slate 500，主色 emerald 600 |
| 焦点 | 按钮 1px 实线外环偏移 2，字段无外环只变边框色 | 无外环，`0 0 0 0.2rem` primary 200 光晕（`ControlOutlineWidth 3`、`ColorControlOutline` primary 200） | 2px 实线外环，字段内缩 1px |
| 动效 | 0.2s | 0.2s | 0s（`EnableMotion=false`） |
| 弹层 | 浅阴影、边框 slate 200 | 更浅阴影 `0 2px 12px` | 阴影同 Aura、边框透明 |

**暗色**：PrimeNG 暗色是显式值表（primary emerald 400 `#34d399`、surface 换 zinc、content 背景 zinc 900 `#18181b`、字段背景 zinc 950、边框 zinc 700）。首版由 AtomUI 的 Dark 算法从 emerald 与 zinc Seed（`ColorBgBase`、`ColorTextBase`）派生，属近似；若对照后差异不可接受，同包增加 `Appearance="Dark"` 的独立定义（AuraDark 等），XML v1 已支持。

**明确不支持**：ripple（PrimeNG 是运行时可选项）、浮动标签、以 `disabledOpacity 0.6` 表达禁用（AtomUI 用禁用色）、Filled 字段形态、rem 根字号缩放。悬停边框色是一处已知差异：AtomUI 输入框悬停边框由 ControlTheme 用主色，PrimeNG Aura 用 slate 400；如需精确，给输入框家族增加"悬停边框色" Own Token，属于 3.4 的 E 类审计项。

PrimeNG 的 Material 预设与 `AtomUI.Themes.Material` 重叠，不重复做。

Gallery 新增"主题预设"页：复刻 ant.design 预览区的控件集合，顶部一排预设图标来自已注册的主题包，点击后对页面内的 `ThemeConfigProvider` 应用对应主题；明暗、紧凑与预设正交组合；"复制主题"导出当前 XML。

### 3.6 生效测试：声称可定制的都要被证明

数据驱动的渲染测试，建在现有 `AtomUI.Desktop.Controls.Rendering.Tests` 的 compositor 帧捕获之上：

- 对每个控件的每个 Active Own Token，以及消费图里该控件消费的全局 Token：渲染基线帧；把该 Token 改为一个明显不同的值（颜色取反色、尺寸加倍、厚度加 4）；再渲染；断言控件边界内的像素差异超过阈值。差异为零即"设置了没效果"，测试失败。
- 用例由生成器从承诺面产出骨架，控件作者只补"把控件摆到什么状态能看见这个 Token"。
- 验收是"有变化"，不是像素精确；像素对照沿用已采用的关键区域容差策略，只用于视觉回归。
- 每个控件家族迁移完成的门禁就是该家族生效测试全绿。

### 3.7 主题编辑器（不在本次范围）

主题编辑器作为独立工具另立项目。本方案为它准备的只有数据：承诺面（Active / Reserved）与全局 Token 消费图由生成器产出为可被工具读取的清单。不在本次新增运行时 API；编辑器需要的 schema 读取接口到那个项目再定。

### 3.8 主题文件格式

保持 XML v1。它与 antd 的 `{ token, components, algorithm }` 同构，已有 Reader、Binder、校验、用户目录与编译主题包。不新增 JSON；编辑器导入导出 XML。若将来需要给设计工具或 AI 一个 JSON 入口，再做一个与 XML 一一映射的读写器，不改模型。

## 4. 对 release/6.0 的改动登记

| 改动 | 类别 | 说明 |
| --- | --- | --- |
| 编译器：显式 Own 覆盖先于 `CalculateTokenValues`，加显式位与 `IsExplicit` | 架构优化 | 修复 A；替换 22 处 `IsNaN` 哨兵 |
| 控件主题内 `SharedTokenResource` → `XxxTokenResource` / `OwnerTokenResource`（1917 处，分家族）；新增 `ThemeScope.TokenOwner` 可继承附加属性与 `OwnerTokenResource` | 架构优化 + 新能力 | 修复 B；公开控件读法成本不变，基元多一个继承属性订阅 |
| Own Token 字段分类（Active / Reserved / 删除）与生成器引用分析 | 架构优化 | 修复 C；删除字段进发布说明 |
| 主题 AXAML 字面量与代码侧消费审计，含 2.3 的借鉴修复（分家族） | 架构优化 | 修复 D、E |
| 生成器：Own 引用分析、全局 Token 消费图、生效测试骨架 | 新能力 | 支撑 3.1、3.6；消费图同时供将来的编辑器工具读取 |
| 分析器：3.4 的七条规则，存量先警告，按家族转错误 | 新能力 | |
| 预设主题 NuGet 包（PrimeNG、Shadcn、Bootstrap、Material；编译主题包机制，Core 不变） | 新能力 | 3.5 |
| Gallery 主题预设页 | 新能力 | 3.5 |

不改动：`ThemeConfig` 模型、XML v1 格式、事务与作用域、资源扩展与 Token 键、Semantic Part、控件结构与模板、公开控件 API、公开运行时 API。

## 5. 验证

- 修复 A：设置 `DataGrid.BorderColor` 后表格边框变色；Button 设置 `ContentFontSize` 后 `ContentLineHeight` 跟随；显式值在任何求值器写法下都保留。
- 修复 B：对每个迁移家族，`Controls[X].Tokens[ColorPrimary]` 只改变 X（含其模板内的共享基元），不改变其他控件；根 `Tokens[ColorPrimary]` 改变全部。
- 承诺面：生成器对未引用且未标 Reserved 的 Own 字段报错；消费图与实际 AXAML 引用一致（抽样核对）。
- 生效测试：每个迁移家族的全部 Active Own Token 与消费的全局 Token 有变化；全家族迁移完成后全绿。
- 借鉴修复：2.3 清单每项附一个可失败的测试（该分支原有测试按本方案读法改写）。
- 预设：每个包可独立引用；预设页渲染无异常，明暗与紧凑组合正常；README 的"不支持"清单与实际一致。PrimeNG 的 Aura / Lara / Nora 三个预设对 3.5.1 映射表逐项核对值，并与 primeng.dev 的 Aura 演示截图按关键区域容差对照一次，结论记入 README。
- 性能：迁移前后 Gallery 首页套样式耗时与主题切换 Publish 耗时同阶（Stopwatch，断言不劣化超过 5%）。

## 6. 实施顺序

| 步 | 内容 | 验收 |
| --- | --- | --- |
| 1 | 修复 A：编译器顺序、显式位、`IsExplicit`；改写 22 处哨兵；DataGrid `BorderColor` 用例 | 现有测试全过 + 第 5 节修复 A 用例 |
| 2 | 生成器：Own 引用分析（先警告）、消费图；`TokenOwner` 属性与 `OwnerTokenResource` | 产出约 100 个待裁决字段清单与消费图 |
| 3 | 生效测试骨架与第一个家族（Button 家族）跑通；同时完成该家族的 B、C、D、E 与借鉴修复 | Button 家族全绿 |
| 4 | 按家族推进：Input 家族（含 InputControlFrame、AddOnDecoratedBox）、Select、Checkbox / Radio / Switch、Steps、Progress、Tag、Card、Alert、Dialog、Notification，再其余 | 每家族全绿后分析器对其转错误 |
| 5 | 预设主题包：先 `AtomUI.Themes.PrimeNG`（Aura、Lara、Nora）与 Gallery 主题预设页，再 Shadcn、Bootstrap、Material | 包可独立引用；预设页可切换；PrimeNG 映射表逐项核对；README 齐全 |
| 6 | 可选控件包（DataGrid、ColorPicker、Extras）走第 3、4 步同样流程 | 同上 |

量级：第 1、2 步各几天；第 3 步建立模板约一周；第 4 步每家族一到三天，十余个家族约一到两个月；第 5 步 PrimeNG 一到两周，其余三个包各数天。第 1 步独立成立，可以先合。

## 7. 决策

| 问题 | 决策 | 理由 |
| --- | --- | --- |
| 要不要设计语言抽象层 | 不要 | Ant Design 是唯一设计语言；其他语言只模拟不复刻 |
| 修复 A 的方式 | 覆盖先写入 + 显式位 + 求值后再写 | 对现有 `CalculateTokenValues` 的最小修正；不引入依赖图与求值版本 |
| 控件主题内全局 Token 的读法 | 公开控件读自己的有效值；共享基元经继承的 owner identity 读所属控件的有效值 | 让逐控件覆盖对全部消费点生效，与 antd 语义一致；不用跳链观察 |
| `feature/custom-theme` | 不合并；逐提交核对后借鉴 13 个有效修复与五条规则，`OwnerTokenResource` 用继承属性重做 | 修复与样式资产、V2、新增字段缠在一起 |
| 无消费者的 Own Token | Active / Reserved / 删除三选一，构建期强制 | 承诺面必须真实 |
| 验收方式 | 生效测试（有变化）为门禁，像素对照只做回归 | 避免零差异门禁与逐表批准 |
| 主题文件格式 | 保持 XML v1 | 与 antd 配置同构，基础设施已全 |
| 预设的交付 | 一个设计语言一个 NuGet 主题包（PrimeNG 包内含 Aura / Lara / Nora 三个主题），用现有编译主题包机制 | 引用即用，不膨胀 Core；第三方走同一路径 |
| PrimeNG 的暗色 | 首版由 Dark 算法从 Seed 派生；对照不可接受再加独立 Dark 定义 | 先用现有机制，证据决定是否加值表 |
| 主题编辑器 | 不在本次范围，另立独立工具 | 本次只产出它需要的承诺面与消费图数据 |
