# AtomUI 主题定制：精确生效与设计语言模拟

日期：2026-11-07。状态：设计方案，待审阅；未开始实现。
基线：AtomUI `release/6.0`（`ae5ac0a32`，Avalonia 12.1.5）。参照：Ant Design 6.7.0 的主题配置模型。

## 1. 目标与边界

AtomUI 自始至终是 Ant Design 设计语言，不引入设计语言抽象层。本方案只做两件事：

1. **证明可模拟**：在 Ant Design 框架内，只通过主题定义（Token、逐控件 Token、算法）就能模拟出其他设计语言的大致样式，作为内置预设提供并在 Gallery 里像 ant.design 首页那样切换。不追求复刻细节；凡是需要改控件结构或代码才能达到的效果，明确列为不支持。
2. **保证精确生效**：凡是我们声称可定制的 Token，设置后必须在渲染层精确体现。不能出现"主题设置了，但没有效果"。

不做：lookless 全量重构、契约部件、按语言切换 ControlTheme、第三方设计语言包。

## 2. 现状

### 2.1 已有能力

release/6.0 的主题配置模型与 Ant Design 的主题配置一一对应：

| Ant Design | AtomUI |
| --- | --- |
| `token` 全局 Token 覆盖 | `ThemeConfig.Tokens`；XML `<Tokens>` |
| `components[Comp]` 逐组件 Token，含 `algorithm` | `ThemeConfig.Controls`；XML `<Control Catalog Id Algorithm="Unspecified/Disabled/Global/Custom">` |
| `algorithm` 默认/暗色/紧凑可组合 | `Algorithms`，三者可组合 |
| 嵌套 `ConfigProvider`、`inherit` | `ThemeConfigProvider`、`Inherit` |
| 运行时切换 | `ApplyThemeAsync`，事务式提交 |
| 主题编辑器 | 无（Gallery 只有写死的示例配置） |
| 预设 | Core 内置 DaybreakBlue；Gallery 自带 4 个色板主题（PolarGreen、SunsetOrange、GoldenPurple、Magenta），以 `<AtomUIThemeDefinition>` 项编译为主题包 |

还多了 antd 没有的：主题 XML 文件与用户主题目录热重载、**编译主题包**（项目里声明 `<AtomUIThemeDefinition Include="x.theme.xml"/>`，生成器产出类型化注册与 `Register(builder)`，经 `AddCompiledThemePackage` 进入同一 Catalog）、局部作用域事务、Semantic Part。编译主题包就是现成的"主题 NuGet 包"机制。

### 2.2 精确性缺口（对源码的实测）

| # | 缺口 | 数据 | 根因 |
| --- | --- | --- | --- |
| A | Own Token 覆盖不传导到依赖它的默认值 | `ThemeCompiler` 先 `descriptor.Evaluate`（即 `CalculateTokenValues`）再 `ApplyAllValues(config.OwnTokens)`；5 个 Token 类共 22 处用 `IsNaN` 判断"是否显式设置"，但求值时覆盖尚未写入，永远判为未设置 | 覆盖写入晚于默认计算 |
| A′ | 同一根因的具体案例 | `DataGridToken.CalculateTokenValues` 里 `TableBorderColor = BorderColor`，主题消费的是 `TableBorderColor`；用户设置 `BorderColor` 后无任何效果 | 同上 |
| B | 逐控件的全局 Token 覆盖对一半以上的消费点无效 | 控件主题 AXAML 中 `SharedTokenResource` 1917 处、`XxxTokenResource` 1643 处；`Shared` 永远读全局值，`Controls[Button].ColorPrimary` 对那 76 处 `SharedTokenResource ColorPrimary` 无效 | 主题作者在应读"控件有效全局值"处读了"共享全局值" |
| C | 声明了但没人消费的 Own Token | 96 个 Token 类 1150 个字段中约 100 个在所属包内无任何引用（DataGrid 的 20 个经内部派生字段间接消费，仍受 A′ 影响） | 字段随文档声明，消费未跟上，或命名不一致 |
| D | 主题 AXAML 里的视觉字面量 | 134 处数值字面量，颜色字面量 0 处。集中在 OverlayDialogResizer（12）、DataGridRowGroupHeader（11）、SplitView（8，分隔线 `Width="1"`/`BorderThickness="0,0,1,0"`）、NumericUpDownSpinner（8）、ScrollBar、Alert、ProgressBar 系列 | 约一半是布局常量，另一半（线宽、分隔线、圆角 0、字号）应是 Token |
| E1 | 代码里写死的颜色 | `BadgeColorUtils` 用 14 个 RGB 常量定义 Badge 的预设色，不读主题的预设色板，暗色下也不变；Tag 的自定义色用固定亮度 0.95 算浅底；其余 127 处命中几乎全是 `Colors.Transparent`（合法） | 绕过了主题的 `PresetPalettes` |
| E2 | 代码里写死的几何 | `CalendarRangeBarPanel` 的字号 12、高度 20、内边距 8；`SwitchKnob` 加载指示笔宽 1；共 12 处视觉常量，其余是行为常量（拖拽阈值、自动滚动边距）属合法 | 应改为 Own Token 或全局 Token |
| E3 | 绕过作用域读资源 | `NavMenu` 用 `Application.Current.TryGetResource` 读 `MotionDurationMid`，失败回落写死的 200 ms；`TabItem`/`TabStripItem` 经 `Application.Current` 查命名主题；`DataGridColumn` 查不到时回落 Application；共约 6 处 | 局部 `ThemeConfigProvider` 的覆盖对这些读取无效 |
| E4 | 代码侧计算视觉值的模式 | 111 处代码赋视觉属性，38 个自绘。Button 的做法是对的：`ControlTokenAccessor` 读控件有效值，`ThemeTokenResolver.Subscribe` 在主题变化时重算 | 模式可用但没有统一约束；需逐控件确认都订阅了变化、都读有效值 |

A、B 是机制缺口，C、D、E 是逐控件的消费缺口。结论是：控件代码确实需要优化，但范围是可枚举的有限清单，不是重写；Button 这类核心控件的代码模式已经正确，问题集中在少数控件和历史遗留处。

缺口 B 的 1917 处里有一个结构性原因：`InputControlFrame`（90 处）、`AddOnDecoratedBox`（29 处）这类**没有自己 Token 身份的共享基元**被多个公开控件的模板复用，它们的主题没法写 `LineEditTokenResource`，只能退而读共享全局值，于是用户对 LineEdit 的逐控件覆盖传不进输入框的边框、圆角与内边距。这不是作者疏忽，是缺一个"读当前所属公开控件的有效值"的机制。

### 2.3 custom-theme 分支已做的相关修复

该分支在这些缺口上做过大量工作，可作为核对清单，但不直接合并：

| 缺口 | 分支做法 | 评价 |
| --- | --- | --- |
| A | 编译器先写显式 Own 值、求值、再写一遍；另加依赖式默认值 V2、存在位、逐层 Revision | 顺序修正与本方案一致；V2 机制过重，不采用 |
| B | 新增 `OwnerTokenResource`，沿 `TemplatedParent → Parent` 跳链在运行时找注册的公开 owner 并观察路由变化；控件主题中 `SharedTokenResource` 从 1917 处降到 164 处，`OwnerTokenResource` 1664 处 | 方向正确，证明了问题范围；路由靠跳链观察，成本与复杂度偏高，本方案改用可继承附加属性（3.3） |
| E3 | `NavMenu` 的 `Application.Current` 读取已移除 | 同类修复约 20 个 fix 提交（Tag、ToolTip 改读快照色板，Steps、Splitter、AddOnDecoratedBox 等） |
| E1 | `BadgeColorUtils` 的 14 个 RGB 常量未改 | 本方案要修 |
| 整体 | 这些改动与样式资产子系统、50 个新增 Own 字段、V2 求值缠在同一批提交里 | cherry-pick 成本接近重做；只借它的问题清单与修复思路 |

## 3. 方案

### 3.1 承诺面：什么叫"我们声称可定制"

承诺面 = 全部全局 Token + 每个控件的 Active Own Token。每个 Own Token 字段必须属于以下之一，由生成器登记：

| 分类 | 含义 | 要求 |
| --- | --- | --- |
| Active | 有真实消费者 | 其所属包的主题 AXAML 或代码至少引用一次；生效测试（3.6）必须通过 |
| Reserved | 暂无消费者，有保留理由 | 字段加 `[ReservedToken("原因")]`；主题编辑器与文档标为保留，不出现在"可调"列表 |
| 删除 | 既无消费者也无理由 | 删除字段；进入发布说明 |

生成器对每个 Own 字段做引用分析：未引用且未标 Reserved → 构建错误。约 100 个待裁决字段在第 6 节第 2 步逐个处理。

全局 Token 不要求每个控件都消费，但生成器产出**消费图**（哪个控件的主题与代码消费了哪个全局 Token），供主题编辑器显示"改这个 Token 影响哪些控件"，供文档生成。

### 3.2 修复 A：Own Token 覆盖先于默认计算

编译器对每个控件的求值改为：

```text
builder = descriptor.CreateBuilder()
builder.AssignEffectiveGlobalToken(effectiveGlobal)
builder.MarkExplicit(config.OwnTokens)            // 写入显式值并置"显式位"
descriptor.Evaluate(builder, appearance)          // CalculateTokenValues 只为未显式的字段算默认
builder.ApplyExplicit()                           // 再写一遍显式值，保证无论求值器怎么写都以显式为准
```

`AbstractControlDesignToken` 增加 `protected bool IsExplicit(XxxTokenKind kind)`；求值器用它代替 `IsNaN` 哨兵判断（NaN 对 Color、Thickness 等类型不可用）。分析器禁止新的 `IsNaN` 哨兵用法，存量 22 处改写。内部派生字段（如 DataGrid 的 `Table*`）在求值器里从**已含显式值**的公开字段派生，A′ 随之修复。

这是现有 `CalculateTokenValues` 模式的最小修正，不引入依赖图、求值版本或别名机制。

### 3.3 修复 B：控件主题只读"控件有效全局值"

规则按主题所属类型分两种：

| 主题的 TargetType | 全局 Token 的读法 | 数量 |
| --- | --- | --- |
| 有 Token 身份的公开控件（Button、LineEdit、Steps…） | `{atom:XxxTokenResource Name}`，读自己的有效全局值 | 约 1700 处 |
| 没有 Token 身份的共享基元与部件（InputControlFrame、AddOnDecoratedBox、各类 Item/Cell/Presenter） | `{atom:OwnerTokenResource Name}`，读**当前所属公开控件**的有效全局值 | 约 200 处 |

`SharedTokenResource` 只允许出现在应用级样式和明确跨控件共享语义的场合，后者需在 AXAML 上以注释标注理由。分析器根据 TargetType 是否有 Token 身份判断该用哪种，用错报警告，迁移完成后转错误。

`OwnerTokenResource` 的机制：

- 每个有 Token 身份的公开控件在构造时把自己的 identity 写入可继承附加属性 `ThemeScope.TokenOwner`（生成器在注册代码里产出，控件作者不写）。
- 基元与部件在模板内继承到这个值；Popup 内容经逻辑父级同样继承，独立窗口经现有 lease 继承。
- `OwnerTokenResource` 产出一个绑定：输入是继承到的 owner identity 与 Token kind，输出是作用域 Provider 对 `ControlTokenResourceKey.Global(identity, kind)` 的查询结果；owner 变化（重套模板、重新挂载）或主题变化都触发重取。
- 没有继承到 owner（基元被单独使用）时回落为共享全局值。

与 custom-theme 分支沿 `TemplatedParent → Parent` 跳链并观察每一跳的做法相比：继承属性由 Avalonia 原生传播，无需路由观察；天然跨 Popup 边界；每个绑定只多一个对继承属性的订阅。嵌套的公开控件（SearchEdit 里的 Button）写入自己的 identity，读自己的有效值，这与 antd 一致。

替换后 `ThemeConfig.Controls[X].Tokens[全局名]` 对该控件的全部消费点生效，包括它模板里的共享基元，与 antd `components[Comp]` 的语义一致。约 1900 处替换按控件家族分批，每批跑该家族的生效测试。

性能：`XxxTokenResource` 的查询路径已存在且为零分配，控件级无覆盖时走 `UsesRootSharedResources` 直接读全局表，与 `Shared` 成本相同；`OwnerTokenResource` 多一个继承属性订阅，只用于约 200 处基元与部件。

### 3.4 修复 C、D、E：逐控件消费审计

控件代码的规则只有四条，写进分析器：

1. 视觉值（颜色、画刷、阴影、线宽、圆角、字号、尺寸）不得以字面量出现在控件代码里；`Colors.Transparent` 与 0 允许。
2. 读 Token 只经 `ControlTokenAccessor`（控件有效值）或 `XxxTokenResource` 绑定；禁止 `Application.Current.TryGetResource` / `TryFindResource` 读 Token，禁止读取失败时回落写死的值。
3. 代码里由 Token 算出的视觉值必须在主题变化时重算（`ThemeTokenResolver.Subscribe`），不得在模板应用时算一次就缓存。
4. 预设色（Badge、Tag、Button 的 preset color）必须从快照的 `PresetPalettes` 取，随主题与明暗变化。

按控件家族推进，每个家族一次性处理：

- C：每个未引用 Own 字段裁决为 Active（补消费）、Reserved（加理由）或删除。
- D：134 处 AXAML 字面量逐个判断：是布局常量则保留并加 `<!-- layout -->` 注释（分析器据此放行），是视觉值则改为 Token；分隔线与边框线宽统一用 `LineWidth`。
- E1：`BadgeColorUtils` 改读 `PresetPalettes`；Tag 自定义色的浅底改用现有调色板算法而非固定亮度。
- E2：`CalendarRangeBarPanel`、`SwitchKnob` 等 12 处几何常量改为 Own Token 或全局 Token。
- E3：`NavMenu`、`TabItem`、`TabStripItem`、`DataGridColumn` 等约 6 处改经作用域读取，删除写死的回落值。
- E4：逐控件核对代码侧视觉计算是否订阅主题变化、是否读控件有效值；不符合的改成 Button 的模式。

每个家族的验收是该家族的生效测试全部通过（3.6）。

### 3.5 预设：以主题 NuGet 包交付

预设用现有的**编译主题包**机制交付，一个预设一个 NuGet 包，别人引用即用；Core 仍只内置 DaybreakBlue。每个包就是一个小项目：

```text
AtomUI.Themes.Shadcn/
+-- AtomUI.Themes.Shadcn.csproj          引用 AtomUI.Core；<AtomUIThemeDefinition Include="Themes/*.theme.xml" />
+-- Themes/ShadcnZinc.theme.xml              Appearance="Light"：zinc 中性色、主色近黑、BorderRadius 6、ControlHeight 36、
|                                            细边框、EnableWaveSpirit=false、弱阴影；逐控件 <Control> 覆盖按需
+-- Themes/ShadcnSlate.theme.xml …           同一语言的其他色系
+-- ShadcnThemes.cs                      UseShadcnThemes() 扩展：调用生成的 Register(builder)
\-- README.md                                模拟了什么、放弃了什么（焦点环形态、Command 式弹层）
```

生成器把 XML 编译为类型化注册，运行时不读 XML；注册进同一 Catalog，出现在 `AvailableThemes`，与用户目录主题、DaybreakBlue 走同一事务。暗色由现有 Dark 算法从该主题的 Seed 派生，不单独写暗色 XML；需要时同包可放 `Appearance="Dark"` 的独立定义。

首批包：

| 包 | 模拟 | 明确不支持（写进 README） |
| --- | --- | --- |
| `AtomUI.Themes.Shadcn` | zinc / slate / stone 三色系，近黑主色，细边框，无 wave，弱阴影 | 焦点环形态、Command 式弹层 |
| `AtomUI.Themes.Bootstrap` | `#0d6efd` 主色、`FontSize 16`、`ControlHeight 38`、`BorderRadius 6` | 渐变背景、内阴影 |
| `AtomUI.Themes.Material` | 紫色主色、`BorderRadius 4`、`ControlHeight 40`、三级 BoxShadow 近似 elevation | ripple、浮动标签、全大写 |
| `AtomUI.Themes.Variants` | Square、HeavyBorder、LargeType、ReducedMotion 等单维度变体，各一个主题 | 无 |

Gallery 现有的 4 个色板主题也可以迁成 `AtomUI.Themes.AntColors` 包，Gallery 改为引用；是否迁移由用户定。第三方做自己的预设走完全相同的路径：一个项目、若干 `.theme.xml`、一个扩展方法。

它们证明的命题是：Ant Design 的 Token 词汇足以表达这些语言的大致观感；表达不了的部分写在包的 README 里，不改控件。

Gallery 新增"主题预设"页：复刻 ant.design 预览区的控件集合，顶部一排预设图标来自已注册的主题包，点击后对页面内的 `ThemeConfigProvider` 应用对应主题；明暗、紧凑与预设正交组合；"复制主题"按钮导出当前 XML。

### 3.6 生效测试：声称可定制的都要被证明

数据驱动的渲染测试，建在现有 `AtomUI.Desktop.Controls.Rendering.Tests` 的 compositor 帧捕获之上：

- 对每个控件的每个 Active Own Token，以及消费图里该控件消费的全局 Token：渲染基线帧；把该 Token 改为一个与基线明显不同的值（颜色取反色、尺寸加倍、厚度加 4）；再渲染；断言控件边界内的像素差异超过阈值。差异为零即"设置了没效果"，测试失败。
- 用例由生成器从承诺面产出骨架，控件作者只补"把控件摆到什么状态能看见这个 Token"（例如 hover 态才可见的 Token 要先置 hover 伪类）。
- 验收是"有变化"，不是"像素精确"；像素级对照继续沿用已采用的关键区域容差策略，只用于视觉回归。
- 每个控件家族迁移完成的门禁就是该家族生效测试全绿。

### 3.7 Gallery 主题编辑器

参照 ant.design Theme Editor：

- 左侧按全局 Token 分组（Seed / Map / Alias）与按控件分组（Own Token、该控件消费的全局 Token）编辑；只显示 Active，Reserved 灰显并给理由。
- 右侧预览区在独立 `ThemeConfigProvider` 内，编辑即时生效；可切换预览控件集合。
- 导出当前配置为主题 XML；导入 XML；与当前基线 diff。
- 需要一个只读的公开 schema 视图：`IThemeManager.Schema`，暴露 `GlobalTokens` 与 `Controls` 的 descriptor（名称、类型、Stage、Active/Reserved、消费者列表）。这是本方案唯一新增的公开运行时 API。

### 3.8 主题文件格式

保持 XML v1。它与 antd 的 `{ token, components, algorithm }` 同构，已有 Reader、Binder、校验、用户目录与编译主题包。不新增 JSON 格式；编辑器导入导出 XML。若将来需要给设计工具或 AI 一个 JSON 入口，再做一个与 XML 一一映射的读写器，不改模型。

## 4. 对 release/6.0 的改动登记

| 改动 | 类别 | 说明 |
| --- | --- | --- |
| 编译器：显式 Own 覆盖先于 `CalculateTokenValues`，加显式位与 `IsExplicit` | 架构优化 | 修复 A；替换 22 处 `IsNaN` 哨兵 |
| 控件主题内 `SharedTokenResource` → `XxxTokenResource` / `OwnerTokenResource`（1917 处，分家族）；新增 `ThemeScope.TokenOwner` 可继承附加属性与 `OwnerTokenResource` 标记扩展 | 架构优化 + 新能力 | 修复 B；公开控件读法成本不变，基元多一个继承属性订阅 |
| Own Token 字段分类（Active / Reserved / 删除）与生成器引用分析 | 架构优化 | 修复 C；删除字段进发布说明 |
| 主题 AXAML 字面量与代码侧消费审计（分家族） | 架构优化 | 修复 D、E |
| 生成器：Own 引用分析、全局 Token 消费图、生效测试骨架 | 新能力 | 支撑 3.1、3.6、3.7 |
| 分析器：控件主题内 `SharedTokenResource`、`IsNaN` 哨兵、代码直接 `FindResource` 读 Token、未标注字面量 | 新能力 | 存量先警告，按家族转错误 |
| `IThemeManager.Schema` 只读视图 | 新能力 | 编辑器与文档工具 |
| 预设主题 NuGet 包若干（编译主题包机制，Core 不变） | 新能力 | 3.5 |
| Gallery 主题预设页与主题编辑器 | 新能力 | 3.5、3.7 |

不改动：`ThemeConfig` 模型、XML v1 格式、事务与作用域、资源扩展与 Token 键、Semantic Part、控件结构与模板、公开控件 API。

## 5. 验证

- 修复 A：设置 `DataGrid.BorderColor` 后表格边框变色；Button 设置 `ContentFontSize` 后 `ContentLineHeight` 跟随；显式值在任何求值器写法下都保留。
- 修复 B：对每个迁移家族，`Controls[X].Tokens[ColorPrimary]` 只改变 X，不改变其他控件；根 `Tokens[ColorPrimary]` 改变全部。
- 承诺面：生成器对未引用且未标 Reserved 的 Own 字段报错；消费图与实际 AXAML 引用一致（抽样核对）。
- 生效测试：每个迁移家族的全部 Active Own Token 与消费的全局 Token 有变化；全家族迁移完成后全绿。
- 预设：每个预设在预设页渲染无异常，明暗与紧凑组合正常；XML 顶部的"不支持"清单与实际一致。
- 编辑器：编辑 → 预览 → 导出 → 导入往返后有效 Token 相同。
- 性能：迁移前后 Gallery 首页套样式耗时与主题切换 Publish 耗时同阶（Stopwatch，断言不劣化超过 5%）。

## 6. 实施顺序

| 步 | 内容 | 验收 |
| --- | --- | --- |
| 1 | 修复 A：编译器顺序、显式位、`IsExplicit`；改写 22 处哨兵；DataGrid `BorderColor` 用例 | 现有测试全过 + 第 5 节修复 A 用例 |
| 2 | 生成器：Own 引用分析（先警告）、消费图、`IThemeManager.Schema` | 产出约 100 个待裁决字段清单与消费图 |
| 3 | 生效测试骨架与第一个家族（Button 家族）跑通；同时完成该家族的 B、C、D、E | Button 家族全绿 |
| 4 | 按家族推进：Input 家族、Select、Checkbox/Radio/Switch、Steps、Progress、Tag、Card、Alert、Dialog、Notification，再其余 | 每家族全绿后分析器对其转错误 |
| 5 | 预设主题包（Shadcn、Bootstrap、Material、Variants）+ Gallery 主题预设页 | 包可独立引用；预设页可切换；README 的"不支持"清单齐全 |
| 6 | Gallery 主题编辑器 | 往返测试通过 |
| 7 | 可选控件包（DataGrid、ColorPicker、Extras）走第 3、4 步同样流程 | 同上 |

量级：第 1、2 步各几天；第 3 步建立模板约一周；第 4 步每家族一到三天，十余个家族约一到两个月；第 5、6 步各一到两周。第 1 步独立成立，可以先合。

## 7. 决策

| 问题 | 决策 | 理由 |
| --- | --- | --- |
| 要不要设计语言抽象层 | 不要 | Ant Design 是唯一设计语言；其他语言只模拟不复刻 |
| 修复 A 的方式 | 覆盖先写入 + 显式位 + 求值后再写 | 对现有 `CalculateTokenValues` 的最小修正；不引入依赖图与求值版本 |
| 控件主题内全局 Token 的读法 | 公开控件读自己的有效值；共享基元经继承的 owner identity 读所属控件的有效值 | 让逐控件覆盖对全部消费点生效，与 antd 语义一致；不用跳链观察 |
| custom-theme 分支 | 不合并；借问题清单与修复思路，`OwnerTokenResource` 改用继承属性重做 | 修复与样式资产、V2、新增字段缠在一起 |
| 无消费者的 Own Token | Active / Reserved / 删除三选一，构建期强制 | 承诺面必须真实 |
| 验收方式 | 生效测试（有变化）为门禁，像素对照只做回归 | 避免 custom-theme 分支的零差异门禁与逐表批准 |
| 主题文件格式 | 保持 XML v1 | 与 antd 配置同构，基础设施已全 |
| 预设的交付 | 一个预设一个 NuGet 主题包，用现有编译主题包机制 | 引用即用，不膨胀 Core；第三方走同一路径；Gallery 已有 4 个色板主题证明机制可用 |
