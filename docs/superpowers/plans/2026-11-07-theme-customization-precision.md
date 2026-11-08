# 主题定制精确生效与设计语言模拟 实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 让 AtomUI 声称可定制的每个 Token 设置后都在渲染层精确生效，并以 NuGet 主题包（PrimeNG、Shadcn、Bootstrap、Material）证明只靠主题定义就能模拟其他设计语言的大致观感。

**Architecture:** 保持 `ThemeConfig` / XML v1 / 事务 / 作用域不变。在编译器里把显式 Own Token 覆盖提到默认计算之前（修复 A）；给共享基元一条读"所属控件有效全局值"的路（`ThemeScope.TokenOwner` + `OwnerTokenResource`，修复 B）；用生成器的引用分析、消费图和七条分析器规则把承诺面变成构建期事实；用数据驱动的生效测试做每个控件家族的门禁；预设走现有编译主题包机制。

**Tech Stack:** .NET 10、Avalonia 12.1.5、Roslyn 源生成器与 analyzer（`AtomUI.Generator`）、xunit v3 + Shouldly、Avalonia.Headless + Skia 帧捕获（`AtomUI.Desktop.Controls.Rendering.Tests`）。

**Spec:** `docs/superpowers/specs/2026-11-07-theme-customization-precision-design.md`。正式文档已按方案更新：`docs/architecture/systems/theming/runtime.md`（§1、§7、§13、§21、§22）、`docs/engineering/development/control-token-guidelines.md`、`docs/engineering/development/compiler-diagnostics-guidelines.md`（`ATOMUIGEN040`–`047`）、`docs/engineering/development/theme-effect-tests.md`、`docs/architecture/systems/theming/compiled-theme-packages/overview.md` §11、`docs/guides/theming/preset-theme-packages.md`、`docs/gallery/authoring/theme-presets-page.md`、`docs/modules/themes/overview.md`。实现与文档不一致时以文档为准并在本计划里登记差异。

## Global Constraints

- 基线 `release/6.0`（`ae5ac0a32`），工作分支 `feature/design-language`；不引用、不合并 `feature/custom-theme`。
- 不改动：`ThemeConfig` 模型、XML v1 格式、事务与作用域、Semantic Part、控件结构与模板、公开控件 API、公开运行时 API（新增 `OwnerTokenResourceExtension`、`ReservedTokenAttribute`、`AbstractControlDesignToken.IsExplicit` 除外）。
- 不引入设计语言抽象层、lookless 重构、按语言切换 ControlTheme、JSON 主题格式、主题编辑器运行时 API。
- 预设包名：`AtomUI.Themes.PrimeNG`、`AtomUI.Themes.Shadcn`、`AtomUI.Themes.Bootstrap`、`AtomUI.Themes.Material`；不带 `Like` 等后缀；Core 仍只内置 DaybreakBlue。
- 新诊断 ID 固定为 `ATOMUIGEN040`（Error）、`ATOMUIGEN041`–`047`（Warning），定义在 `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticIds.cs` 与 `AtomUIDiagnosticDescriptors.cs`；每个诊断在 `tests/AtomUI.Generator.Tests` 有正例、反例、边界。
- 控件代码读 Token 只经 `ControlTokenAccessor`、`TokenResourceBinder`、三种 TokenResource；禁止 `Application.Current.TryGetResource` / `TryFindResource` / `FindResource` 读 Token，禁止读取失败回落字面量。
- 性能：Token 资源查询热路径零分配；迁移前后 Gallery 首页套样式与主题切换 Publish 耗时同阶，不劣化超过 5%。
- 提交信息单行、`<type>(<scope>): <subject>`，无任何 AI/agent attribution trailer。
- 测试遵守 `docs/engineering/development/test-value-and-lifecycle.md`：生效测试与契约测试是常驻测试；验证用的临时测试完成后删除。
- 命令：`dotnet test tests/<Project>/<Project>.csproj`；全量回归用 `scripts/run-full-regression.sh`。

## Review Focus

- `Controls[X].Tokens[ColorPrimary]` 对 X 模板里嵌套的另一个公开控件（SearchEdit 里的 Button）不应生效：Button 带自己的 TokenOwner。Task 3 的测试覆盖。
- 基元在 Popup 内容里（Select 下拉的 Option Item）要继承到 owner：Popup 逻辑父级是 Select。Task 4 的测试覆盖。
- 用户显式覆盖的 Own Token 值为 `0`、`Colors.Transparent`、空 Thickness 时也算显式：显式性只看显式位，不看值。Task 1 的测试覆盖。
- 主题切换后 owner 不变但有效值变了，`OwnerTokenResource` 绑定必须重取：资源订阅跟随 Provider 发布。Task 4 的测试覆盖。
- 用户不经 `BasedOn` 完全替换默认 ControlTheme：基元回落全局值而不是抛异常。Task 3 的测试覆盖。

---

### Task 1: 修复 A —— 显式 Own Token 覆盖先于默认计算

**Files:**
- Modify: `src/AtomUI.Core/Theme/DesignTokens/AbstractControlDesignToken.cs`
- Modify: `src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs:517-532`
- Test: `tests/AtomUI.Core.Tests/Theme/ControlOwnTokenExplicitOverrideTests.cs`（新建）
- Test: `tests/AtomUI.Core.Tests/Theme/ThemeGeneratedDescriptorContractTests.cs`（追加一个用例）

**Interfaces:**
- Produces: `AbstractControlDesignToken` 新增 `internal void MarkExplicit(int ownSlot)`、`internal bool IsExplicitSlot(int ownSlot)`、`protected bool IsExplicit<TKind>(TKind kind) where TKind : struct, Enum`（`kind` 的整数值即 Own Token slot）、`internal void ResetExplicit()`。
- Produces: `ThemeCompiler.CompileControl` 的 Own 段固定为 `MarkExplicit(builder, config.OwnTokens)` → `descriptor.Evaluate(builder, appearance)` → `ApplyAllValues(builder, config.OwnTokens)`。

- [ ] **Step 1: 写失败测试（编译器顺序）**

在 `ControlOwnTokenExplicitOverrideTests.cs` 中仿照 `DenseThemeCompilerTests.Control(string id)` 定义一个 `ExplicitTestToken : AbstractControlDesignToken`，两个 Own 字段 `Base`(slot 0)、`Derived`(slot 1)，`Evaluate(appearance)` 里 `Base = 10; if (!IsExplicit(ExplicitTestTokenKind.Derived)) Derived = Base * 2;`，并定义 `enum ExplicitTestTokenKind { Base, Derived }`。用 `ThemeTestSnapshotFactory.Compile(config, descriptor)`：

```csharp
[Fact] public void Explicit_Own_Override_Is_Visible_To_Default_Calculation()
// config: Controls[identity].Tokens["Base"] = "7" → snapshot.Control<double>(identity, 1) == 14
[Fact] public void Explicit_Own_Override_Wins_Over_Evaluator_Write()
// Evaluate 无条件写 Derived = 99；config Derived="5" → Control(identity,1) == 5
[Fact] public void Explicit_Zero_Counts_As_Explicit()
// config Derived="0" → Control(identity,1) == 0，而不是 Base*2
[Fact] public void Unset_Field_Is_Not_Explicit()
// 无 config → IsExplicitSlot(1) == false，Derived == 20
```

- [ ] **Step 2: 运行确认失败**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --filter "FullyQualifiedName~ControlOwnTokenExplicitOverrideTests"`
Expected: 编译失败（`IsExplicit` 不存在）。

- [ ] **Step 3: 实现显式位**

`AbstractControlDesignToken` 增加 `private ulong[]? _explicitSlots;`，`MarkExplicit` 按 slot 置位（按需扩容），`IsExplicitSlot` 读位，`IsExplicit<TKind>` 用 `Unsafe.As<TKind, int>(ref kind)`（生成的 `XxxTokenKind` 底层为 `int`）转 slot。`ThemeCompiler.CompileControl` 中新增 `private static void MarkExplicit(AbstractControlDesignToken builder, IReadOnlyList<NormalizedTokenValue> values)`：对每个 value 调 `value.Descriptor.SetValue(builder, copy)` 并 `builder.MarkExplicit(value.Descriptor.Slot)`；Own 段改为三步顺序。

- [ ] **Step 4: 运行确认通过，并跑全部 Core 主题测试**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`
Expected: 全部 PASS（含 `ControlCompilationCacheTests`、`DenseThemeCompilerTests`，缓存键不受影响）。

- [ ] **Step 5: 契约测试：TokenKind 枚举值等于 Own slot**

在 `ThemeGeneratedDescriptorContractTests.cs` 追加 `Own_Token_Kind_Enum_Values_Match_Descriptor_Slots`：对 Core 自带的 generated schema 中每个有 Own Token 的 descriptor，`Enum.GetValues(kindType)` 的整数值集合等于 `OwnTokens.Select(t => t.Slot)`，名称一一对应。Run 同上，Expected PASS。

- [ ] **Step 6: Commit**

```bash
git add src/AtomUI.Core/Theme tests/AtomUI.Core.Tests/Theme
git commit -m "fix(Theme): 显式 Own Token 覆盖先于默认计算写入并以显式位判断"
```

---

### Task 2: 改写 22 处 `IsNaN` 哨兵、`ATOMUIGEN040`、DataGrid `BorderColor` 用例

**Files:**
- Modify: `src/AtomUI.Desktop.Controls/Buttons/ButtonToken.cs`（6 处）、`Buttons/HyperLinkButtonToken.cs`（4）、`Menu/MenuToken.cs`（6）、`NavMenu/NavMenuToken.cs`（2）、`DropdownButton/DropdownButtonToken.cs`（4）
- Create: `src/AtomUI.Generator/DesignToken/OwnTokenSentinelAnalyzer.cs`
- Modify: `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticIds.cs`、`AtomUIDiagnosticDescriptors.cs`
- Test: `tests/AtomUI.Generator.Tests/DesignToken/OwnTokenSentinelAnalyzerTests.cs`（新建）
- Test: `tests/AtomUI.Desktop.Controls.Tests/Buttons/ButtonTokenExplicitOverrideTests.cs`（新建）
- Test: `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Theme/DataGridBorderColorOverrideTests.cs`（新建）

**Interfaces:**
- Consumes: Task 1 的 `IsExplicit<TKind>`。
- Produces: `AtomUIDiagnosticIds.OwnTokenSentinel = "ATOMUIGEN040"`。

- [ ] **Step 1: 写分析器失败测试**

正例：`[ControlDesignToken] sealed class T : AbstractControlDesignToken { public double A{get;set;} public override void CalculateTokenValues(bool d){ A = !double.IsNaN(A) ? A : 1; } }` → 1 个 `ATOMUIGEN040`，位置在 `double.IsNaN(A)`。反例：同体用 `IsExplicit(TTokenKind.A)` → 0。边界：`CalculateTokenValues` 之外的 `IsNaN`（例如工具方法）不报。测试模式照 `tests/AtomUI.Generator.Tests/DataMemberAccessors` 下现有 analyzer 测试。

- [ ] **Step 2: 运行确认失败**

Run: `dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --filter "FullyQualifiedName~OwnTokenSentinelAnalyzerTests"` → 编译失败。

- [ ] **Step 3: 实现 `OwnTokenSentinelAnalyzer : DiagnosticAnalyzer`**

注册 `SyntaxKind.InvocationExpression` action；条件：所在方法名 `CalculateTokenValues`、所在类型带 `[ControlDesignToken]`、被调方法符号是 `System.Double.IsNaN` / `System.Single.IsNaN`，且参数是本类型 Own 属性的访问。Severity Error。

- [ ] **Step 4: 运行确认通过**（同 Step 2 命令）→ PASS。

- [ ] **Step 5: 写控件级失败测试**

`ButtonTokenExplicitOverrideTests`（Desktop.Controls.Tests，Headless）：通过 `IThemeManager.ApplyThemeAsync` 提交 `Controls[ButtonTokens.Identity].Tokens["ContentFontSize"]="20"`，用 `ControlTokenAccessor.Capture<Button>(button).GetOwn<double>(ButtonTokenKind.ContentLineHeight)` 断言跟随新字号（按 `ButtonToken.CalculateTokenValues` 现有公式计算期望值）。`DataGridBorderColorOverrideTests`：覆盖 `BorderColor`=`#FF0000` 后 `GetOwn<Color>(DataGridTokenKind.TableBorderColor)`（若 `Table*` 是 internal 字段无 Kind，改为断言 DataGrid 模板中 `Name="PART_..."` 边框 `BorderBrush` 颜色为红）。

- [ ] **Step 6: 运行确认失败**

Run: `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --filter "FullyQualifiedName~ButtonTokenExplicitOverrideTests"` 与 DataGrid 对应命令 → FAIL（哨兵永远判为未设置）。

- [ ] **Step 7: 改写 22 处哨兵**

每处 `X = !double.IsNaN(X) ? X : expr` → `if (!IsExplicit(XxxTokenKind.X)) X = expr;`。`DataGridToken` 不含哨兵但 `Table*` 派生字段已在 Task 1 后自动跟随，无需改动；核对 `TableBorderColor = BorderColor` 仍在 `CalculateTokenValues` 内。

- [ ] **Step 8: 运行确认通过，并构建 Desktop.Controls 确认无 `ATOMUIGEN040`**

Run: `dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj -warnaserror:ATOMUIGEN040` → 0 error；Step 6 两个测试 PASS。

- [ ] **Step 9: 登记诊断并 Commit**

`compiler-diagnostics-guidelines.md` 注册表已含 `ATOMUIGEN040`；确认 Owner 列 `DesignToken` 与实现一致。

```bash
git add src tests docs
git commit -m "refactor(Theme): 用 IsExplicit 取代 Own Token 的 IsNaN 哨兵并新增 ATOMUIGEN040"
```

---

### Task 3: `ThemeScope.TokenOwner` 与 ControlTheme 解析时注入

**Files:**
- Modify: `src/AtomUI.Core/Theme/ThemeScope.cs`
- Modify: `src/AtomUI.Core/Theme/ThemeManager.cs:40-42`
- Test: `tests/AtomUI.Core.Tests/Theme/ThemeScopeTokenOwnerTests.cs`（新建）

**Interfaces:**
- Produces: `ThemeScope.TokenOwnerProperty : AttachedProperty<ControlTokenIdentity?>`（`RegisterAttached<ThemeConfigProvider, StyledElement, ControlTokenIdentity?>("TokenOwner", inherits: true)`）、`internal static ControlTokenIdentity? GetTokenOwner(StyledElement e)`。
- Produces: `ThemeManager.NotifyControlThemeResolved(object key, ControlTheme theme)` 在触发事件前调用 `EnsureTokenOwnerSetter(key, theme)`：`key is Type t && Registry.TryGetControl(t, out d)` 且 `theme` 未注入过 → `theme.Setters.Add(new Setter(ThemeScope.TokenOwnerProperty, d.Identity))`；已注入集合用 `ConditionalWeakTable<ControlTheme, object>`。

- [ ] **Step 1: 写失败测试**

在 Core.Tests（`HeadlessTestApp`，参照 `ThemeConfigProviderTests` 的窗口搭建）注册 `ThemeTestControlTypes` 中的 `LineEditThemeTestControl`（有 identity）与一个无 identity 的 `FrameTestPrimitive : TemplatedControl`，为 LineEdit 测试控件提供以 `Type` 为键的 ControlTheme，其模板含 `FrameTestPrimitive`：

```csharp
[Fact] public void Default_ControlTheme_Injects_TokenOwner_Once()
// 解析两次同一 ControlTheme → Setters 中恰好 1 个 TokenOwner Setter
[Fact] public void Template_Primitive_Inherits_Owner_Identity()
// window 内放 LineEdit 测试控件，ApplyTemplate 后模板里的 FrameTestPrimitive.GetValue(TokenOwnerProperty) == LineEditTokens.Identity
[Fact] public void Nested_Registered_Control_Carries_Its_Own_Identity()
// 模板里再放 ButtonThemeTestControl → 它的 TokenOwner 是 Button identity，不是 LineEdit
[Fact] public void Replaced_Theme_Without_BasedOn_Has_No_Owner()
// 给实例设 Theme = new ControlTheme(typeof(LineEditThemeTestControl)) → 基元 TokenOwner == null
```

- [ ] **Step 2: 运行确认失败**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --filter "FullyQualifiedName~ThemeScopeTokenOwnerTests"` → 编译失败。

- [ ] **Step 3: 实现属性与注入**（见 Interfaces；`Dispose` 路径里清空 `ConditionalWeakTable` 不需要，弱表随 theme 回收）

- [ ] **Step 4: 运行确认通过**；再跑整个 Core.Tests 确认 `ThemeManagerTests`、`BaseControlThemeTests` 不受影响。

- [ ] **Step 5: Commit**

```bash
git add src/AtomUI.Core/Theme tests/AtomUI.Core.Tests/Theme
git commit -m "feat(Theme): 默认 ControlTheme 解析时注入可继承的 ThemeScope.TokenOwner"
```

---

### Task 4: `OwnerTokenResourceExtension`

**Files:**
- Create: `src/AtomUI.Core/Theme/Resources/OwnerTokenResourceExtension.cs`
- Create: `src/AtomUI.Core/Theme/Resources/OwnerTokenBinding.cs`（internal）
- Test: `tests/AtomUI.Core.Tests/Theme/OwnerTokenResourceTests.cs`（新建）
- Modify: `tests/AtomUI.Core.Tests/Theme/TokenResourceExtensionTests.cs`（追加 ProvideValue 用例）

**Interfaces:**
- Consumes: Task 3 的 `ThemeScope.TokenOwnerProperty`；现有 `ControlTokenResourceKey.Global(ControlTokenIdentity, SharedTokenKind)`（`ThemeTokenResourceProvider.TryGetResource` 已支持按 identity 解析未绑定键）。
- Produces: `public sealed class OwnerTokenResourceExtension : MarkupExtension { public SharedTokenKind? Kind {get;set;} public OwnerTokenResourceExtension(){} public OwnerTokenResourceExtension(SharedTokenKind kind); public override object ProvideValue(IServiceProvider sp) }`，AXAML 写法 `{atom:OwnerTokenResource ColorBorder}`。
- Produces: `internal sealed class OwnerTokenBinding : IBinding`，与本仓库 Avalonia 12.1.5 中 `DynamicResourceExtension` 支持 Setter 目标的方式一致（先读参照 Avalonia 源码里 `DynamicResourceExtension` / `DynamicResourceBindingExpression`，照其接口实现）：对目标 `StyledElement` 建立 observable——订阅 `target.GetObservable(ThemeScope.TokenOwnerProperty)`，每次 owner 变化换成对 `target.GetResourceObservable(owner is {} id ? ControlTokenResourceKey.Global(id, kind) : kind)` 的订阅并转发值。

- [ ] **Step 1: 写失败测试**

```csharp
[Fact] public void ProvideValue_Returns_Binding_Without_Resolving_Owner()   // ThrowingServiceProvider 不被访问
[Fact] public void Primitive_Reads_Owner_Effective_Global_Value()
// LineEdit 测试控件配置 Controls[LineEdit].Tokens["ColorBorder"]="#FF0000"；模板基元 BorderBrush 绑 OwnerTokenResource ColorBorder → 红
[Fact] public void Primitive_Without_Owner_Falls_Back_To_Global()           // 基元单独放进 window → 等于全局 ColorBorder
[Fact] public void Owner_Change_Retargets_Binding()                        // 把基元从 LineEdit 模板移到 Button 模板（或改 TokenOwner 值）→ 值切换
[Fact] public void Theme_Publish_Refreshes_Owner_Value()                   // ApplyThemeAsync 改 LineEdit 的 ColorBorder → 基元跟随
[Fact] public void Popup_Content_Inherits_Owner()                          // 基元放在 owner 控件打开的 Popup 内容里 → 读到 owner 值
[Fact] public void Query_Hot_Path_Does_Not_Allocate()                      // 参照 ThemeTokenResourceProviderTests 的零分配断言方式，对已建立绑定后的主题切换做 GC.GetAllocatedBytesForCurrentThread 对比
```

- [ ] **Step 2: 运行确认失败** → 编译失败。

- [ ] **Step 3: 实现扩展与绑定**（见 Interfaces；`Kind` 为 null 时抛 `InvalidOperationException("A Token resource key is required.")`，与 `TokenResourceExtension<T>` 一致）

- [ ] **Step 4: 运行确认通过**；跑 Core.Tests 全部。

- [ ] **Step 5: 注册 XAML 命名空间**

确认 `atom` 命名空间（`https://atomui.net`）映射已覆盖 `AtomUI.Theme.Resources`（`SharedTokenResourceExtension` 同目录），`OwnerTokenResource` 在 AXAML 中可用；在 `tests/AtomUI.Desktop.Controls.Tests` 用一个最小 AXAML 字符串经 `AvaloniaRuntimeXamlLoader` 加载验证解析成功。

- [ ] **Step 6: Commit**

```bash
git add src/AtomUI.Core/Theme/Resources tests
git commit -m "feat(Theme): 新增 OwnerTokenResource 让共享基元读取所属控件的有效全局 Token"
```

---

### Task 5: 主题资产分析器 `ATOMUIGEN041` / `045` / `046`

**Files:**
- Create: `src/AtomUI.Generator/ThemeAssets/ThemeAssetConsumptionAnalyzer.cs`（在 `TokenResourceKeyGenerator` 的 asset 转换里对每个已解析 `XElement` 运行，诊断经 `GenerationOutput` 报出，位置用 `Location.Create(path, span, lineSpan)`）
- Modify: `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticIds.cs`、`AtomUIDiagnosticDescriptors.cs`
- Test: `tests/AtomUI.Generator.Tests/ThemeAssets/ThemeAssetConsumptionAnalyzerTests.cs`（新建；用 `ThemeSchemaGeneratorTests` 的 `RunGenerator` + AdditionalText 方式喂 AXAML）

**Interfaces:**
- Consumes: `TokenInfo.ControlThemeInfos`（`HasDescriptor` 判断 TargetType 是否有 Token 身份）。
- Produces: `AtomUIDiagnosticIds.ThemeAssetSharedReadMismatch="ATOMUIGEN041"`、`ThemeAssetVisualLiteral="ATOMUIGEN045"`、`ThemeAssetTemplateAttributeMasksSetter="ATOMUIGEN046"`。放行标记：`SharedTokenResource` 前最近的 XML 注释含 `shared:`；数值字面量前最近注释含 `layout`。

- [ ] **Step 1: 写失败测试**

041 正例：`<ControlTheme TargetType="atom:Button">` 内 `Value="{atom:SharedTokenResource ColorPrimary}"` → 1 个 041；无身份 TargetType（`atom:InputControlFrame`）用 `SharedTokenResource` 且无 `<!-- shared: ... -->` → 1 个 041；反例：`XxxTokenResource` / `OwnerTokenResource` / 带 `shared:` 注释 → 0。
045 正例：`CornerRadius="4"`、`BorderThickness="1"`、`FontSize="12"`、`Background="#FFF"` 各 1 个；反例：`Width="1"` 前有 `<!-- layout -->`、`{atom:...TokenResource ...}` 值、`Opacity="0"`。
046 正例：模板节点 `<Border Name="X" Background="{atom:ButtonTokenResource DefaultBg}"/>` 且同主题存在 `<Style Selector="^:pointerover /template/ Border#X"><Setter Property="Background" .../>` → 1 个 046；反例：默认值在 `<Style Selector="^ /template/ Border#X">` 的 Setter 里 → 0。边界：Selector 不能静态解析到该节点 → 不报。

- [ ] **Step 2: 运行确认失败** → 编译失败。

- [ ] **Step 3: 实现分析器**（纯 `XElement` 遍历；046 的匹配只处理 `/template/ <Type>#<Name>` 形式的 Selector，其余不报）

- [ ] **Step 4: 运行确认通过**；对 `src/AtomUI.Controls`、`src/AtomUI.Desktop.Controls` 构建一次，记录 041/045/046 的数量到 `docs/superpowers/plans/2026-11-07-theme-customization-precision.md` 末尾"存量基线"小节（预期 041 约 1900、045 约 134）。

- [ ] **Step 5: Commit**

```bash
git add src/AtomUI.Generator tests/AtomUI.Generator.Tests docs/superpowers/plans
git commit -m "feat(Generator): 主题资产分析器 ATOMUIGEN041/045/046 校验读法、字面量与模板属性遮蔽"
```

---

### Task 6: `[ReservedToken]`、Own 引用分析 `ATOMUIGEN042`、消费图

**Files:**
- Create: `src/AtomUI.Core/Theme/DesignTokens/ReservedTokenAttribute.cs`（`[AttributeUsage(AttributeTargets.Property)] public sealed class ReservedTokenAttribute(string reason) { public string Reason {get;} }`）
- Create: `src/AtomUI.Generator/DesignToken/OwnTokenReferenceAnalysis.cs`
- Create: `src/AtomUI.Generator/ThemeSchema/GeneratedThemeConsumptionWriter.cs` → 输出 `GeneratedThemeConsumption.g.cs`
- Modify: `src/AtomUI.Generator/TokenResourceKeyGenerator.cs`（接入分析与 writer）
- Modify: `src/AtomUI.Generator/Diagnostics/*`
- Test: `tests/AtomUI.Generator.Tests/DesignToken/OwnTokenReferenceAnalysisTests.cs`、`tests/AtomUI.Generator.Tests/ThemeSchema/GeneratedThemeConsumptionWriterTests.cs`（新建）

**Interfaces:**
- Consumes: Task 5 已解析的资产 `XElement` 集合；`compilation.SyntaxTrees`。
- Produces（每个控件包程序集一份，`AtomUI.Generated.<Asm>` 命名空间，`internal static class GeneratedThemeConsumption`）：
  - `public static readonly OwnTokenEntry[] OwnTokens;` `record struct OwnTokenEntry(string Catalog, string ControlId, string Token, OwnTokenState State, string? ReservedReason)`，`enum OwnTokenState { Active, Reserved, Unreferenced }`
  - `public static readonly GlobalConsumerEntry[] GlobalConsumers;` `record struct GlobalConsumerEntry(string Catalog, string ControlId, string GlobalToken, string Source)`，`Source` ∈ `"axaml"`、`"code"`。
- Produces: `AtomUIDiagnosticIds.OwnTokenUnreferenced="ATOMUIGEN042"`。
- 引用判定（只做文本/语法匹配，不用 SemanticModel）：AXAML 任一属性值含 `{atom:<Control>TokenResource <Name>}` 或 `Kind="<Name>"`；C# 任一 `MemberAccessExpressionSyntax` 文本为 `<Control>TokenKind.<Name>` 或 `<Control>TokenKey.<Name>`。全局消费：AXAML 中 `<Control>TokenResource <Global>` 与 `OwnerTokenResource <Global>`（归到该资产 TargetType 的 owner，无 owner 时记 `"shared-primitive"`）；C# 中 `GetEffectiveGlobal<T>(SharedTokenKind.<Global>)`、`CreateControlTokenBinding(..., SharedTokenKind.<Global>)`、`EffectiveGlobalToken.<Global>`（归到所在 Token 类的控件）。

- [ ] **Step 1: 写失败测试**：Demo 控件 `Button` 的 Own `Height` 被 AXAML 引用 → Active；`Label` 无引用无 Attribute → 1 个 042 + `Unreferenced`；`Gap` 带 `[ReservedToken("等待 Group 实现")]` → Reserved、无诊断；消费图含 `("AtomUI","Button","ColorPrimary","axaml")`。
- [ ] **Step 2: 运行确认失败。**
- [ ] **Step 3: 实现 Attribute、分析与 writer**（`TargetMarkConstants` 加 `ReservedTokenAttribute` 元数据名）。
- [ ] **Step 4: 运行确认通过**；对 `src/AtomUI.Desktop.Controls` 构建，把 042 的清单（预期约 100 条）写入 `docs/superpowers/plans/2026-11-07-theme-customization-precision.md` 末尾"待裁决 Own Token"小节，按控件分组。
- [ ] **Step 5: Commit**

```bash
git add src tests docs/superpowers/plans
git commit -m "feat(Generator): Own Token 引用分析、ReservedToken 与全局 Token 消费图"
```

---

### Task 7: 控件代码分析器 `ATOMUIGEN043` / `044` / `047`

**Files:**
- Create: `src/AtomUI.Generator/ThemeConsumption/ScopedTokenReadAnalyzer.cs`（043）、`VisualLiteralAnalyzer.cs`（044）、`LocalValueVisualWriteAnalyzer.cs`（047）
- Modify: `src/AtomUI.Generator/Diagnostics/*`
- Test: `tests/AtomUI.Generator.Tests/ThemeConsumption/*AnalyzerTests.cs`

**Interfaces:**
- 043：`Application.Current.TryGetResource/TryFindResource/FindResource(...)`、任意对象上 `TryFindResource/FindResource` 的参数是 `SharedTokenKind.*` 或 `*TokenKey.*`；以及 `if (!TryGetResource(...)) x = <字面量>` 形态的回落。
- 044：`Color.FromRgb/FromArgb/FromUInt32/Parse`、`Brushes.<X>`（`Brushes.Transparent` 除外）、`Colors.<X>`（`Colors.Transparent` 除外）、`new SolidColorBrush(<字面量>)`，以及赋给名为 `Background/Foreground/BorderBrush/Fill/Stroke/BorderThickness/CornerRadius/FontSize/StrokeThickness/BoxShadow` 的属性或字段的数值字面量（`0` 除外）。只在控件包程序集（含 `[ControlDesignToken]` 或引用 `AtomUI.Theme.Resources` 的程序集）里运行；`*Token.cs` 内的 `CalculateTokenValues` 不报（那里是 Token 定义）。
- 047：`SetValue(<视觉 StyledProperty>, value)` 不带优先级参数，或 `this.<视觉属性> = value` 出现在 `Control` 派生类型的非构造方法里（`SetCurrentValue` 不报；构造函数内赋默认值不报）。

- [ ] **Step 1: 写失败测试**（每个诊断：正例 2、反例 2、边界 1；044 的边界：字面量经局部变量传入不报）
- [ ] **Step 2: 运行确认失败。**
- [ ] **Step 3: 实现三个 analyzer**（`SyntaxNodeAction`，`SemanticModel` 只用于确认符号类型，不做全编译扫描）。
- [ ] **Step 4: 运行确认通过**；构建 `src/AtomUI.Controls`、`src/AtomUI.Desktop.Controls`，把 043/044/047 的存量数量记入计划末尾"存量基线"（预期 043 约 6、044 约 30 条真实视觉值 + 若干 `Transparent` 已排除、047 数十条）。
- [ ] **Step 5: Commit**

```bash
git add src/AtomUI.Generator tests/AtomUI.Generator.Tests docs/superpowers/plans
git commit -m "feat(Generator): 控件代码主题消费分析器 ATOMUIGEN043/044/047"
```

---

### Task 8: 生效测试骨架

**Files:**
- Create: `tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/TokenEffectHarness.cs`
- Create: `tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/TokenPerturbation.cs`
- Create: `tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/EffectTestData.cs`
- Create: `tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/TokenEffectHarnessTests.cs`
- Modify: 控件包 `Properties/AssemblyInfo.cs` 增加 `InternalsVisibleTo("AtomUI.Desktop.Controls.Rendering.Tests")`（若尚无）

**Interfaces:**
- Consumes: Task 6 的 `GeneratedThemeConsumption.OwnTokens / GlobalConsumers`；`ThemeConfigProvider`、`ControlTokenAccessor`；现有 `window.CaptureRenderedFrame()` 捕获方式（把 `ButtonFocusVisualRenderingTests` 的 `Capture/CountDifferentPixels/Refresh` 提取到 harness）。
- Produces:
  - `static class TokenEffectHarness { static void AssertHasEffect(Func<Control> create, ControlTokenIdentity identity, string token, Action<Control>? arrange = null, double thresholdRatio = 0.005, int channelTolerance = 8); }`
    流程：Window → `ThemeConfigProvider` → `Border(Padding 24)` → 控件；`arrange` 后捕获控件边界内像素为 A；读当前值（Own 经 `ControlTokenAccessor.Capture(control).GetOwn<object>(kindEnum)`，Global 经 `GetEffectiveGlobal`），`TokenPerturbation.Perturb(value)` 得扰动值并 `ThemeTokenValueFormatter.Format`；替换 Provider.Config 为 `Controls[identity].Tokens[token]=扰动值`；`Refresh` 捕获 B；断言差异像素占比 > `thresholdRatio`（像素差异 = 任一通道差 > `channelTolerance`）。`TimeSpan`/`bool` 类型不比像素，改断言 `ControlTokenAccessor` 读到新值并返回。
  - `static class TokenPerturbation { static object Perturb(object value); }` 规则照 `theme-effect-tests.md` §3 表。
  - `static class EffectTestData { static IEnumerable<object[]> For(string catalog, string controlId); }` 从消费图返回 `(token, isOwn)`，排除 Reserved。

- [ ] **Step 1: 写 harness 自测（失败）**：用 `Button` 跑 `AssertHasEffect(() => new Button{Content="Go"}, ButtonTokens.Identity, "ColorPrimary", b => b.ButtonType = ButtonType.Primary)` 期望通过；用一个已知无效果的 Token（Task 6 清单里任选一个 Unreferenced）期望 harness 抛 `ShouldAssertException`。
- [ ] **Step 2: 运行确认失败**（编译失败）。
- [ ] **Step 3: 实现 harness、扰动、数据源。**
- [ ] **Step 4: 运行确认通过**：`dotnet test tests/AtomUI.Desktop.Controls.Rendering.Tests/AtomUI.Desktop.Controls.Rendering.Tests.csproj --filter "FullyQualifiedName~TokenEffectHarnessTests"`。
- [ ] **Step 5: Commit**

```bash
git add tests src/*/Properties
git commit -m "test(Theme): 数据驱动的 Token 生效测试骨架"
```

---

### Task 9: Button 家族迁移（模板任务，首个家族）

家族成员：`Button`、`HyperLinkButton`、`DropdownButton`、`SplitButton`、`FloatButton`（含 `FloatButtonGroup`）、`IconButton`。本任务同时是 Task 10 各家族的操作模板。

**Files:**
- Modify: `src/AtomUI.Desktop.Controls/Buttons/**`、`DropdownButton/**`、`SplitButton/**`、`FloatButton/**` 的 `*Theme.axaml`、`*Token.cs`、`*.cs`
- Create: `tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/ButtonFamilyEffectTests.cs`
- Modify: `src/AtomUI.Desktop.Controls/.editorconfig`（或新建）：对该家族目录 `dotnet_diagnostic.ATOMUIGEN041..047.severity = error`
- Modify: `docs/controls/desktop/general/button/token.md` 等该家族 `token.md`（Reserved / 删除登记）

**Interfaces:**
- Consumes: Task 4 `OwnerTokenResource`、Task 6 消费图与 042 清单、Task 7 分析器、Task 8 harness。

- [ ] **Step 1: 生效测试先行（失败）**

`ButtonFamilyEffectTests`：每个成员一个 `[Theory] [MemberData(nameof(EffectTestData.For), "AtomUI", "Button")]`，`arrange` 字典按 Token 名给状态：`DefaultHoverBg/DefaultHoverColor/DefaultHoverBorderColor` → `((IPseudoClasses)button.Classes).Add(":pointerover")`；`DefaultActive*` → `:pressed`；`PrimaryColor/PrimaryShadow` → `ButtonType.Primary`；`Danger*` → `IsDanger=true`；`Ghost*` → `IsGhost=true` 深色背景；`Text*`/`Link*` → 对应 `ButtonType`；`*SM`/`*LG` → `SizeType`；`OnlyIcon*`/`IconOnyPadding*` → 只带 Icon；`GroupBorderColor` → 放进 `ButtonGroup`。Run → 预期若干 Token 失败（这就是该家族的缺口清单）。

- [ ] **Step 2: B —— 主题读法替换**

`ButtonTheme.axaml`（30 处）等有身份 TargetType 的 `SharedTokenResource X` → `ButtonTokenResource X`；家族内无身份部件（如 `FloatButtonGroup` 内部 Presenter）→ `OwnerTokenResource X`；确属跨控件共享的保留并加 `<!-- shared: 原因 -->`。构建确认该家族目录 041 为 0。

- [ ] **Step 3: C —— 裁决 042 清单中该家族的字段**

逐个：补消费（Active）/ `[ReservedToken("…")]` / 删除（删除同时更新 `token.md` 与 `CHANGELOG` 的 breaking 条目）。构建确认 042 为 0。

- [ ] **Step 4: D / E —— 字面量、作用域旁路、重算、预设色**

- 045：家族 AXAML 的数值字面量逐个判断：布局常量加 `<!-- layout -->`，视觉值改 Token（线宽用 `LineWidth`）。
- 043 / 044 / 047 报告逐条修。
- 借鉴修复落地（按思路重做，附可失败测试）：`b3f82db2f`（Button 变体画刷经资源键写入、订阅重算）、`e9e49500f`（FloatButton `IsActive` 用 `SetCurrentValue`）、`0b4725a24`（DropdownButton 动效开关读 owner 有效值）。
- E4：核对 `ConfigureVariantThemeVariables` 等代码侧计算都在 `ThemeTokenResolver.Subscribe` 回调中重算、读 `ControlTokenAccessor.Capture<Button>`；排版继承（`ContentFontSize`→模板 `FontSize`）生效。

- [ ] **Step 5: 生效测试全绿**

Run: `dotnet test tests/AtomUI.Desktop.Controls.Rendering.Tests/AtomUI.Desktop.Controls.Rendering.Tests.csproj --filter "FullyQualifiedName~ButtonFamilyEffectTests"` → PASS。再跑 `tests/AtomUI.Desktop.Controls.Tests` 的 `Buttons/` 下现有测试。

- [ ] **Step 6: 隔离与作用域验证**

在 `ButtonFamilyEffectTests` 追加：`Controls[Button].Tokens[ColorPrimary]` 改变 Button 像素、不改变同窗口 `Tag` 像素；根 `Tokens[ColorPrimary]` 两者都变；嵌在 `SearchEdit` 里的 Button 不受 `Controls[LineEdit]` 影响。

- [ ] **Step 7: 分析器对该家族转错误**；`.editorconfig` 按目录范围设置；构建 `src/AtomUI.Desktop.Controls` 成功。

- [ ] **Step 8: Commit**（可拆多次：主题读法、字段裁决、代码修复、测试各一次，subject 带 `Button` scope）

```bash
git commit -m "refactor(Button): 控件主题改读有效全局 Token 并补全 Own Token 消费"
git commit -m "test(Button): Button 家族 Token 生效测试全绿并转分析器为错误"
```

---

### Task 10: 其余家族按 Task 9 模板推进

顺序与成员（每个家族一个独立任务实例，步骤与 Task 9 的 Step 1–8 相同，这里只登记差异）：

| 序 | 家族 | 成员 | 家族特有事项 |
| --- | --- | --- | --- |
| 10.1 | Input | `InputControlFrame`（90 处 Shared → `OwnerTokenResource`）、`AddOnDecoratedBox`（29 处）、`LineEdit`、`TextBox`、`TextArea`、`NumericUpDown`、`ButtonSpinner`、`SearchEdit`、`PasswordEdit`、`AutoComplete` | 借鉴 `8af4c1678`（AddOnDecoratedBox 状态色改为可撤销 Template 优先级）；`1e690fc2d` 的 TextBox 字号预设问题按 E4 修；PrimeNG 的"悬停边框色"需求评估是否增加输入框家族 Own Token `HoverBorderColor`（增加则 Active 且有生效测试） |
| 10.2 | Select | `Select`、`ComboBox`、`Cascader`、`TreeSelect`、下拉 Option Item / Presenter | Popup 内基元经逻辑父级继承 owner（Task 4 已测，这里用真实控件再测一次） |
| 10.3 | Choice | `CheckBox`、`RadioButton`、`Switch`（`SwitchKnob` 加载指示笔宽 1 → Token）、`Slider`、`Rate` | E2 `SwitchKnob` |
| 10.4 | Steps / Progress | `Steps`、`ProgressBar` 系列 | 借鉴 `9f4173894`（Steps 箭头描边 P1）、`5e587b055`（ProgressBar 文字几何 P4） |
| 10.5 | Tag / Badge | `Tag`、`Badge`、`RibbonBadge` | E1：`BadgeColorUtils` 改读 `PresetPalettes`；Tag 自定义色浅底改用现有调色板算法；借鉴 `9ba3924ab`、`fe9c9c583` |
| 10.6 | Feedback | `Alert`、`Dialog` / `OverlayDialog`（`OverlayDialogResizer` 12 处字面量）、`Notification`、`Message`、`ToolTip`、`Popconfirm`、`Popup` | 借鉴 `daa17c067`（ToolTip 预设色 P4/P5）、`33534c625`（Popup） |
| 10.7 | Navigation | `NavMenu`（E3：`Application.Current.TryGetResource(MotionDurationMid)` 与 200ms 回落删除）、`Menu`、`TabControl` / `TabItem` / `TabStripItem`（E3）、`Breadcrumb`、`Pagination`、`Tour` | 借鉴 `473c43b1f`（NavMenu 分组标题 P1）、`9c01e0cc7`（Tour 间距绑定随 Gap 重建） |
| 10.8 | Layout / Container | `Card`、`Collapse`、`Splitter`（借鉴 `b1a6fc37a`：尺寸挂载期绑定 + `AffectsMeasure`）、`SplitView`（分隔线 `Width="1"` → `LineWidth`）、`Separator`、`Space`、`Masonry` | |
| 10.9 | Data | `Table`（基础）、`List`、`Tree`、`Avatar`、`Timeline`、`Calendar`（E2 `CalendarRangeBarPanel` 字号 12 / 高度 20 / 内边距 8 → Own Token）、`DatePicker` / `TimePicker`、`Carousel`、`Statistic` | |
| 10.10 | 其余 | `ScrollBar`、`Icon`、`Window` 装饰、`FloatButton` 之外的剩余控件 | 以 `ATOMUIGEN041`–`047` 报告为清单 |

每个家族完成的定义：该家族生效测试全绿；`Controls[X]` 隔离测试通过；分析器对该家族目录转错误；`token.md` 更新；一个或多个 commit。

---

### Task 11: `AtomUI.Themes.PrimeNG` 主题包（Aura、Lara、Nora）

**Files:**
- Create: `src/AtomUI.Themes.PrimeNG/AtomUI.Themes.PrimeNG.csproj`（以 `src/AtomUI.Fonts.AlibabaSans/AtomUI.Fonts.AlibabaSans.csproj` 为模板：`TargetFrameworks=$(AtomUITargetFrameworks)`、引用 `AtomUI.Core` 与 Generator analyzer；`<AtomUIThemeDefinition Include="Themes/*.theme.xml" />`）
- Create: `src/AtomUI.Themes.PrimeNG/Themes/PrimeNGAura.theme.xml`、`PrimeNGLara.theme.xml`、`PrimeNGNora.theme.xml`
- Create: `src/AtomUI.Themes.PrimeNG/PrimeNGThemes.cs`：`public static class PrimeNGThemeManagerBuilderExtensions { public static IThemeManagerBuilder UsePrimeNGThemes(this IThemeManagerBuilder builder) }` 调用生成的 `AtomUI.Generated.AtomUIThemesPrimeNG.GeneratedThemePackageRegistration.Register(builder)`（生成命名空间以实际生成结果为准）
- Create: `src/AtomUI.Themes.PrimeNG/README.md`
- Modify: `AtomUI.slnx`（新建 `/Themes/` Folder）、`scripts/NuGetPackageProjects.ps1`（`Group="Base"; PackageRole="RuntimeProduct"; PackageId="AtomUI.Themes.PrimeNG"`）
- Test: `tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj`（新建，Headless；引用 `AtomUI.Desktop.Controls` 与各主题包）、`tests/AtomUI.Themes.Tests/PrimeNGThemePackageTests.cs`

**Interfaces:**
- Consumes: 编译主题包机制（`AtomUIThemeDefinition` → `GeneratedThemePackageRegistration.Register`）；XML v1（`<Control Catalog="AtomUI" Id="Button"><Tokens><Token Name=... Value=.../></Tokens></Control>`）；值格式见 `docs/reference/theming/theme-definition-xml-v1.md`（`Thickness` 为 `h,v` 或 `l,t,r,b`，`BoxShadows` 用 Avalonia `BoxShadows.Parse` 格式 `offsetX offsetY blur spread #AARRGGBB`，多个以逗号分隔）。
- Produces: 主题 Id `PrimeNGAura`（`IsDefault="false"`）、`PrimeNGLara`、`PrimeNGNora`。

- [ ] **Step 1: 写失败测试**

```csharp
[Fact] public void UsePrimeNGThemes_Registers_Three_Themes()   // AvailableThemes 含 PrimeNGAura/Lara/Nora，默认主题仍是 DaybreakBlue
[Theory] [InlineData("PrimeNGAura")] ... public async Task Each_Preset_Applies_Light_Dark_Compact(string id)  // ApplyThemeAsync 四种算法组合均 Committed
[Fact] public async Task Aura_Maps_Semantic_Tokens()
// 应用 PrimeNGAura 后 snapshot 全局值：ColorPrimary #10B981、ColorPrimaryHover #059669、ColorPrimaryActive #047857、
// ColorText #334155、ColorBorder #CBD5E1、BorderRadius 6、BorderRadiusXS 2、BorderRadiusSM 4、BorderRadiusLG 8、
// ControlHeight 40、ControlHeightSM 36、ControlHeightLG 48、PaddingContentHorizontal 12、MotionDurationMid 200ms、
// ControlOutlineWidth 0、FocusVisualBorderThickness 1、ColorBgMask #66000000
[Fact] public async Task Aura_Button_Overrides()   // Controls[Button].FontWeight == 500；CheckBox CheckIndicatorSize == 20；ToolTip ToolTipBackground == #334155
[Fact] public async Task Nora_Disables_Motion_And_Uses_Radius_2()  // EnableMotion false；BorderRadius 2；ColorPrimary #059669；ColorText #0F172A
[Fact] public async Task Lara_Uses_Outline_Glow()   // ControlOutlineWidth 3；ColorControlOutline == emerald 200 #A7F3D0
```

- [ ] **Step 2: 运行确认失败**（项目不存在）。

- [ ] **Step 3: 写 Aura XML**

`<Tokens>` 按 spec §3.5.1 映射表逐行：`ColorPrimary #10B981`、`ColorPrimaryHover #059669`、`ColorPrimaryActive #047857`、`ColorTextLightSolid #FFFFFF`、`ColorPrimaryBg #ECFDF5`、`ColorPrimaryText #047857`、`ColorText #334155`、`ColorTextSecondary #64748B`、`ColorTextDescription #64748B`、`ColorBgContainer #FFFFFF`、`ColorFillTertiary #F1F5F9`、`ColorBorderSecondary #E2E8F0`、`ColorBorder #CBD5E1`、`ColorTextPlaceholder #64748B`、`ColorError #F87171`、`BorderRadiusXS 2`、`BorderRadiusSM 4`、`BorderRadius 6`、`BorderRadiusLG 8`、`PaddingContentHorizontal 12`、`ControlHeight 40`、`ControlHeightSM 36`、`ControlHeightLG 48`、`FontSizeSM 12`、`FontSizeLG 16`、`FocusVisualBorderThickness 1`、`ColorFocusBorder #10B981`、`ControlOutlineWidth 0`、`MotionDurationMid 00:00:00.2000000`、`BoxShadowsSecondary "0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000"`、`BoxShadows "0 20 25 -5 #1A000000, 0 8 10 -6 #1A000000"`、`ColorBgMask #66000000`。
`<Controls>`：Button `FontWeight 500`；CheckBox `CheckIndicatorSize 20`；Select `OptionPadding "12,8"`、`PopupContentPadding "4"`；Card `BodyPadding "20"`、`CardShadows "0 1 3 0 #1A000000, 0 1 2 -1 #1A000000"`；ToolTip `ToolTipBackground #334155`、`ToolTipCornerRadius 6`、`ContentPadding "12,8"`；Dialog 的圆角与内边距按 `src/AtomUI.Desktop.Controls/Dialog/DialogToken.cs` 实际 Own 字段名写（12 / 20）。Binder 对不存在的名字报错，构建即校验。

- [ ] **Step 4: 写 Lara、Nora XML**（差异按 spec §3.5.1 表：Lara `ControlHeight 44`（paddingY 10）、Checkbox 24、Button `FontWeight 600`、`ControlOutlineWidth 3`、`ColorControlOutline #A7F3D0`、`FocusVisualBorderThickness 0`、Select `OptionPadding` 圆角 0、`BoxShadowsSecondary "0 2 12 0 #1A000000"`；Nora `BorderRadius*` 全 2、Button `FontWeight 700`、`ColorText #0F172A`、`ColorBorder #64748B`、`ColorPrimary #059669`（Hover `#047857`、Active `#065F46`）、`FocusVisualBorderThickness 2`、`EnableMotion false`、`ColorBorderSecondary #00000000`）。

- [ ] **Step 5: 项目、扩展方法、slnx、打包脚本、README**（README 列映射表出处 `@primeuix/themes` 预设源码与 primeng.dev v22、"不支持"清单：ripple、浮动标签、disabledOpacity、Filled、rem 缩放；已知差异：输入框悬停边框色）。

- [ ] **Step 6: 运行确认通过**：`dotnet test tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj` → PASS。

- [ ] **Step 7: 视觉对照**：在 Gallery（Task 12 完成后）或 Rendering.Tests 里渲染 Button / LineEdit / Checkbox / Select / Card 五个控件的 Aura 态，与 primeng.dev Aura 演示截图按关键区域（圆角、高度、主色、边框色）人工对照一次，结论写入 README"已知差异"。

- [ ] **Step 8: Commit**

```bash
git add src/AtomUI.Themes.PrimeNG tests/AtomUI.Themes.Tests AtomUI.slnx scripts/NuGetPackageProjects.ps1
git commit -m "feat(Themes): 新增 AtomUI.Themes.PrimeNG 主题包（Aura、Lara、Nora）"
```

---

### Task 12: Gallery 主题预设页

**Files:**
- Create: `controlgallery/AtomUIGallery/ShowCases/General/ThemePresets/Views/ThemePresetsShowCase.axaml(.cs)`、`ViewModels/ThemePresetsViewModel.cs`、`Localization/ThemePresetsShowCaseLangResourceKind.cs` + `en-US/zh-CN/zh-TW/pt-BR.xlf`
- Modify: `controlgallery/AtomUIGallery/AtomUIGalleryModule.cs:165,266`（`AddPage(ThemePresetsViewModel.ID, Nav(CaseNavigationLangResourceKind.General_ThemePresets, "ThemePresets"))`、`routes.Map(...)`）
- Modify: `controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/CaseNavigationLangResourceKind.cs` 与 4 个 xlf（`General_ThemePresets`）
- Modify: `controlgallery/AtomUIGallery/AtomUIGallery.csproj`（引用 `AtomUI.Themes.PrimeNG`，后续包同样）、`controlgallery/AtomUIGallery/ThemeManagerBuilderExtensions.cs`（`.UsePrimeNGThemes()`）
- Test: `tests/AtomUIGallery.Tests/ThemePresetsShowCaseTests.cs`

**Interfaces:**
- Consumes: `IThemeManager.AvailableThemes`（`ThemeInfo`：Id、Name、声明主色）、`ThemeConfigProvider`、Task 11 的主题包。
- Produces: `ThemePresetsViewModel { IReadOnlyList<ThemePresetItem> Presets; string? SelectedPresetId; bool IsDark; bool IsCompact; ReactiveCommand CopyThemeXml; }`，`record ThemePresetItem(string ThemeId, string Name, Color Accent, AntDesignIconKind? Icon)`；预设条只列 `ThemeInfo` 来源为主题包的主题（排除用户目录与 Gallery 自带色板），图标按 `ThemeId` 前缀映射（`PrimeNG*`、`Shadcn*`、`Bootstrap`、`Material`），无映射画主色圆点。

- [ ] **Step 1: 写失败测试**（`AtomUIGallery.Tests` 现有 Headless 模式）：`Presets` 含 3 个 PrimeNG 项且不含 `PolarGreen`；选中 `PrimeNGAura` 后预览区 `ThemeConfigProvider` 的 snapshot `ColorPrimary == #10B981` 而根主题不变；切 `IsDark` 后 Provider 算法含 Dark；`CopyThemeXml` 输出含 `Id="PrimeNGAura"` 的 XML v1 文本。
- [ ] **Step 2: 运行确认失败。**
- [ ] **Step 3: 实现页面**（预览区固定控件集合见 `docs/gallery/authoring/theme-presets-page.md`；页面不含任何预设专有代码；导出 XML 用 `ThemeDefinition` 的现有 formatter/`ThemeTokenValueFormatter` 拼 XML v1）。
- [ ] **Step 4: 运行确认通过**；`dotnet build controlgallery/AtomUIGallery.Desktop` 成功并手动打开页面切换一次（按 `atomui-gallery-local-debug` 技能流程）。
- [ ] **Step 5: Commit**

```bash
git commit -m "gallery(ThemePresets): 新增主题预设页展示 PrimeNG 等主题包切换"
```

---

### Task 13: Shadcn、Bootstrap、Material 主题包

每个包一个任务实例，结构与 Task 11 相同（csproj、`Themes/*.theme.xml`、`UseXxxThemes()`、README、slnx、打包脚本、`AtomUI.Themes.Tests` 用例、Gallery 引用）。值表：

| 包 | 主题 | 关键值 |
| --- | --- | --- |
| `AtomUI.Themes.Shadcn` | `ShadcnZinc`、`ShadcnSlate`、`ShadcnStone` | `ColorPrimary` 近黑（zinc 900 `#18181B` / slate 900 `#0F172A` / stone 900 `#1C1917`），`ColorTextLightSolid #FAFAFA`，`BorderRadius 6`，`ControlHeight 36`（SM 32、LG 40），`LineWidth 1`，`EnableWaveSpirit false`，`BoxShadows "0 1 2 0 #0D000000"`，中性色阶取对应色板 50–900 填 `ColorBgLayout/ColorBorder/ColorText*` |
| `AtomUI.Themes.Bootstrap` | `Bootstrap` | `ColorPrimary #0D6EFD`、`ColorSuccess #198754`、`ColorWarning #FFC107`、`ColorError #DC3545`、`FontSize 16`、`ControlHeight 38`、`BorderRadius 6`、`ControlOutlineWidth 4`、`ColorControlOutline #409DFF40` |
| `AtomUI.Themes.Material` | `Material` | `ColorPrimary #6750A4`、`ColorTextLightSolid #FFFFFF`、`BorderRadius 4`、`ControlHeight 40`、`BoxShadowsSecondary` / `BoxShadows` 近似 elevation 1 / 3（`"0 1 3 0 #33000000"`、`"0 4 8 3 #26000000"`）、`FontWeightStrong 500` |

每包测试：注册、四种算法组合可应用、关键值断言各 3 个、README"不支持"清单存在。Commit：`feat(Themes): 新增 AtomUI.Themes.<Name> 主题包`。

---

### Task 14: 可选控件包家族（DataGrid、ColorPicker、Extras）

按 Task 9 模板对 `src/AtomUI.Desktop.Controls.DataGrid`（`DataGridRowGroupHeader` 11 处字面量、`DataGridColumn` 的 Application 回落、借鉴 `92631f292`）、`src/AtomUI.Desktop.Controls.ColorPicker`、`src/AtomUI.Desktop.Controls.Extras` 各做一轮；生效测试放各自测试项目并复用 Task 8 的 harness（harness 若需共享，提取到 `tests/AtomUI.Registration.Fixtures` 或新建 `tests/AtomUI.Theme.Testing` 共享项目）。完成定义同 Task 9。

---

### Task 17: 动效时长随主题与控件级覆盖生效（`SharedTokenResourceValue` 替换）

Task 9 实施时发现：主题 AXAML 中 Transition 的 `Duration` 用 `{atom:SharedTokenResourceValue Kind=MotionDurationMid}` 读取（约 80 个文件）。该扩展在 XAML 加载时经 `Application.Current.TryGetResource` 一次性取全局值并写死：根主题切换（含 `EnableMotion=false` 把时长规范化为 0）与 `Controls[X].Tokens[MotionDuration*]` 都不会到达这些 Transition，041 也看不到它。本任务在 Task 10 各家族迁移之前或并行完成，使动效时长与其他 Token 一样随作用域与主题变化。

**Files:**
- Modify: `src/AtomUI.Core/Data/SharedTokenResourceValue.cs`（删除或改为仅供应用级样式的静态值，见 Interfaces）
- Create: `src/AtomUI.Core/Theme/Resources/TransitionDurationBinding.cs`（或在 Avalonia Transition 支持的机制上实现等价物）
- Modify: 全部使用 `SharedTokenResourceValue` 的主题 AXAML（按控件包批量替换）
- Modify: `src/AtomUI.Generator/ThemeAssets/ThemeAssetConsumptionAnalyzer.cs`（041 识别 `SharedTokenResourceValue`）
- Test: `tests/AtomUI.Core.Tests/Theme/TransitionDurationTests.cs`、`tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/` 中追加动效时长用例

**Interfaces:**
- Avalonia `Transition.Duration` 是 `StyledProperty<TimeSpan>`（`Transition<T>` 派生自 `AvaloniaObject`），可接受绑定，但 Transition 不在可视树中、没有资源宿主。先核实 Avalonia 12.1.5 中 Transition 能否经 `Bind(DurationProperty, …)` 跟随宿主控件资源变化；可行时，读法改为与 `XxxTokenResource` / `OwnerTokenResource` 同语义的值绑定，宿主取 Transition 所属 Style 的目标控件（经 `Transitions` 集合所在控件）。不可行时，退而由控件在 `Transitions` 附着时以 `ControlTokenAccessor` 读取有效时长并在 `ThemeTokenResolver.Subscribe` 回调中重建，主题 AXAML 改为声明式标记（例如 `Duration="{atom:MotionDuration Mid}"`）由控件侧解析。两条路线二选一，在报告中写明依据。
- 041 扩展：控件主题中出现 `SharedTokenResourceValue` 一律报告（无 `shared:` 豁免），迁移完成后该扩展只允许出现在非 ControlTheme 的应用级样式中，或直接删除。

- [ ] **Step 1: 写失败测试**：`MotionDurationMid` 根覆盖为 0（或 `EnableMotion=false`）后，某控件 Transition 的 `Duration` 变为 0；`Controls[Button].Tokens[MotionDurationMid]="00:00:00.5000000"` 只改变 Button 的 Transition；主题切换后旧值不残留。
- [ ] **Step 2: 运行确认失败。**
- [ ] **Step 3: 选定并实现机制**（见 Interfaces 的二选一）。
- [ ] **Step 4: 批量替换主题 AXAML 中的 `SharedTokenResourceValue`**，按控件包提交；每个包替换后跑该包测试。
- [ ] **Step 5: 041 扩展与测试**；`MigratedFamilyAnalyzerGateTests` 对已迁移家族不再允许 `SharedTokenResourceValue`。
- [ ] **Step 6: 回归**：Core、Desktop.Controls、Rendering 测试全部通过。
- [ ] **Step 7: Commit**

```bash
git commit -m "fix(Theme): 动效时长改为随作用域与主题变化的 Token 读取"
```

---

### Task 15: 收尾：性能对照、全量回归、文档同步

- [ ] **Step 1: 性能对照**：在 `tests/AtomUI.Core.Tests/Theme/Performance` 增加 `ThemeReadMigrationBenchmarkTests`：Stopwatch 测 Gallery 首页（`AtomUIGallery.Tests` 可加载的首页视图）套样式耗时与一次 `ApplyThemeAsync` Publish 耗时各 20 次取中位数，对照 `release/6.0` 在同机的记录（先在基线分支跑一次记入测试注释），断言不劣化超过 5%。
- [ ] **Step 2: 全量回归**：`scripts/run-full-regression.sh`，以 `[SUMMARY]` 行为准，全绿。
- [ ] **Step 3: 存量分析器全部转错误**：删除各家族目录级 `.editorconfig` 覆盖，改为包级 `ATOMUIGEN041`–`047` = error；构建全解决方案 0 警告。
- [ ] **Step 4: 文档同步**：`docs/controls/*/token.md` 的 Reserved / 删除标注；`CHANGELOG` 记录删除的 Own Token 字段与新增包；`docs/architecture/systems/theming/compiled-theme-packages/overview.md` §11 的包表状态改为"已发布"；`docs/superpowers/plans/...` 末尾"存量基线"更新为 0。
- [ ] **Step 5: Commit**

```bash
git commit -m "docs(Theme): 主题精确生效重构收尾，登记 Reserved/删除字段与预设包"
```

---

### Task 16: 派生基字段的 `IsExplicit` 守卫与 `ATOMUIGEN048`（在 Task 2 之后、Task 3 之前执行）

Task 2 实施时发现：评估器对每个 Own 字段都是无条件赋值，`MarkExplicit → Evaluate → ApplyExplicit` 能保证基字段最终是显式值，但同一方法里由基字段派生的字段（DataGrid `TableBorderColor = BorderColor`、Button `ContentLineHeight` 由 `ContentFontSize` 推）在 Evaluate 期间读到的仍是刚被覆盖回去的默认值。因此"派生字段自动跟随"只在基字段的默认赋值被 `IsExplicit` 守卫时成立。本任务把这条规则变成分析器并一次性改写存量。

**Files:**
- Create: `src/AtomUI.Generator/DesignToken/OwnTokenDerivationBaseAnalyzer.cs`
- Modify: `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticIds.cs`、`AtomUIDiagnosticDescriptors.cs`
- Modify: `src/AtomUI.Controls/**/*Token.cs`、`src/AtomUI.Desktop.Controls/**/*Token.cs`、`src/AtomUI.Desktop.Controls.DataGrid/DataGridToken.cs`、`src/AtomUI.Desktop.Controls.ColorPicker/**/*Token.cs`、`src/AtomUI.Desktop.Controls.Extras/**/*Token.cs`（分析器报告的每一处）
- Test: `tests/AtomUI.Generator.Tests/DesignToken/OwnTokenDerivationBaseAnalyzerTests.cs`
- Test: `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Theme/DataGridBorderColorOverrideTests.cs`（已有，保持通过）

**Interfaces:**
- Produces: `AtomUIDiagnosticIds.OwnTokenDerivationBaseUnguarded = "ATOMUIGEN048"`，Error。规则：在 `[ControlDesignToken]` 类型的 `CalculateTokenValues` 内，对 Own 属性 `P` 的赋值语句没有被 `if (!IsExplicit(<Kind>.P))` 直接包裹，而 `P` 在同一方法体中该赋值之后被读取（出现在任何表达式的右侧，含 `new Thickness(P, …)`、方法参数）→ 报告，位置在该赋值。只读不写、或写后不再读的属性不报；`base.CalculateTokenValues(...)` 调用忽略。
- 修复形态固定为 `if (!IsExplicit(XxxTokenKind.P)) { P = expr; }`，不改变 `expr`。

- [ ] **Step 1: 写分析器失败测试**：正例 `A = 1; B = A * 2;`（A 未守卫 → 1 个 048 在 `A = 1`）；`A = 1; B = new Thickness(A);`（1 个）；反例 `if (!IsExplicit(K.A)) A = 1; B = A * 2;`（0）、`A = 1;` 后无读取（0）、读取发生在赋值之前 `B = A; A = 1;`（0）；边界：赋值在 `if/else` 两个分支里各一次且之后被读取（2 个）。
- [ ] **Step 2: 运行确认失败。**
- [ ] **Step 3: 实现分析器**（数据流只需语法顺序：收集方法体内按位置排序的赋值与读取，对每个 Own 属性判断"存在未守卫赋值且其后有读取"）。
- [ ] **Step 4: 运行确认通过。**
- [ ] **Step 5: 构建全部控件包，按 048 报告逐处加守卫**（不改表达式；守卫使用该字段的 `XxxTokenKind` 成员），直到 `dotnet build` 对 `src/AtomUI.Controls`、`src/AtomUI.Desktop.Controls`、`DataGrid`、`ColorPicker`、`Extras` 以 `-warnaserror:ATOMUIGEN048` 全部通过。记录改写处数到计划末尾"存量基线"。
- [ ] **Step 6: 回归**：`tests/AtomUI.Core.Tests`、`tests/AtomUI.Desktop.Controls.Tests`、`tests/AtomUI.Desktop.Controls.DataGrid.Tests` 全部通过。
- [ ] **Step 7: Commit**

```bash
git commit -m "refactor(Theme): 派生基字段默认赋值加 IsExplicit 守卫并新增 ATOMUIGEN048"
```

---

## 存量基线（Task 5 / 7 执行后填写）

| 诊断 | AtomUI.Controls | AtomUI.Desktop.Controls | DataGrid | ColorPicker | Extras |
| --- | --- | --- | --- | --- | --- |
| ATOMUIGEN041 | 23 | 1714 | | | |
| ATOMUIGEN042 | 3 | 99 | 21 | 2 | 0 |
| ATOMUIGEN043 | 0 | 3 | 0 | 0 | 0 |
| ATOMUIGEN044 | 19 | 2 | 0 | 23 | 0 |
| ATOMUIGEN045 | 0 | 90 | | | |
| ATOMUIGEN046 | 3 | 29 | | | |
| ATOMUIGEN047 | 11 | 28 | 0 | 8 | 0 |
| ATOMUIGEN048（Task 16 已清零；改写处数） | 0 | 50 | 24 | 3 | 0 |

041/045/046 为 Task 5 实测（唯一文件+行，`--no-incremental`）。045 低于计划估计的约 134：其余字面量是 0、`Opacity="1"` 或位于 ControlTheme/Style 之外；主题中颜色字面量为 0。`Splash/Themes/SplashDefaultTheme.axaml` 以 Link 引入、不在 `Themes/` 资产路径下，其 12 处读取未被扫描，在 Task 10.10 处理。DataGrid、ColorPicker、Extras 的 041/045/046 在 Task 14 迁移时统计。

043/044/047 为 Task 7 实测。043 只覆盖 Token 键读取：NavMenu 的 `Application.Current.TryGetResource(MotionDurationMid)` 与 200 ms 回落、Window 的 `this.TryFindResource(WindowTokenKind.DefaultBackground)`。TabItem、TabStripItem、Menu 经 `Application.Current` 按字符串键查命名 ControlTheme，DataGridColumn 转发运行时键，这些是 E3 的命名主题作用域旁路，不在 043 范围内，在 Task 10.7 与 Task 14 手工修复。044 只按精确视觉成员名匹配，`CalendarRangeBarPanel` 的字号 12 / 高度 20 / 内边距 8 与 `SwitchKnob` 笔宽 1 经局部变量或复合名传入，不被报告，在 Task 10.9、10.3 手工修复（E2）。

## 待裁决 Own Token（Task 6 执行后填写）

按控件分组列出 `ATOMUIGEN042` 报告的字段，每行裁决为 Active / Reserved（理由）/ 删除，随家族任务勾销。

Task 6 实测共 125 个字段。被同一 `CalculateTokenValues` 用来派生已消费字段的基字段已按派生链计为 Active，不在此表。
"所属任务"列是负责裁决该字段的家族任务；裁决结果与勾销在该任务提交里更新本表。

| 包 | 控件 | 字段 | 所属任务 | 裁决 |
| --- | --- | --- | --- | --- |
| Controls | Avatar | GroupOverlapping, GroupSpace | 10.9 | |
| Controls | Icon | FallbackColor | 10.10 | |
| Desktop | Button | DefaultBorderColorDisabled, ExtraContentItemSpacing, ExtraContentMargin, ExtraContentMarginLG, ExtraContentMarginSM, FontWeight, GroupBorderColor, GutterToFlyout | 9 | FontWeight、DefaultBorderColorDisabled → Active；GroupBorderColor → Reserved；ExtraContentItemSpacing、ExtraContentMargin(LG/SM)、GutterToFlyout → 删除 |
| Desktop | OptionButton | ButtonBackground, ButtonCheckedBackground, ButtonPadding, ContentLineHeightLG | 9 | ContentLineHeightLG → Active（修正 ContentPadding 运算数后）；ButtonBackground、ButtonCheckedBackground、ButtonPadding → 删除 |
| Desktop | FloatButton | CircleBadgeOffset, SquareBadgeOffset | 9 | 删除 |
| Desktop | LineEdit | InputFontSizeSM | 10.1 | |
| Desktop | ButtonSpinner | ControlWidth | 10.1 | |
| Desktop | TextArea | ResizeHandleSize | 10.1 | |
| Desktop | Select | FixedItemMargin, OptionPadding, SelectAffixPadding | 10.2 | |
| Desktop | ComboBox | ItemDisabledColor | 10.2 | |
| Desktop | RadioButton | RadioBgColor | 10.3 | |
| Desktop | Slider | MarginPartWithMark | 10.3 | |
| Desktop | ProgressBar | CircleTextColor, LineBorderRadius, ProgressActiveMotionDuration, ProgressStepMarginInlineEnd, ProgressStepMinWidth | 10.4 | |
| Desktop | Tag | TagCloseIconSize | 10.5 | |
| Desktop | Alert | CloseIconSize, MessageWithDescriptionMargin | 10.6 | |
| Desktop | MessageCard | CardHeight | 10.6 | |
| Desktop | PopupConfirm | PopupMinHeight, PopupMinWidth | 10.6 | |
| Desktop | ToolTip | MotionDuration | 10.6 | |
| Desktop | NavMenu | CollapsedWidth, DangerItemActiveBg, DangerItemColor, DangerItemHoverColor, DangerItemSelectedBg, DangerItemSelectedColor, DarkDangerItemActiveBg, DarkDangerItemColor, DarkDangerItemHoverColor, DarkDangerItemSelectedBg, DarkDangerItemSelectedColor, HorizontalItemBorderRadius, HorizontalItemHoverBg, HorizontalItemHoverColor, HorizontalItemSelectedBg, HorizontalItemSelectedColor, ItemBackgroundMotionEasing, KeyGestureColor, MenuSubMenuBg, SubMenuItemBorderRadius, TopLevelItemPopupMarginToAnchor | 10.7 | |
| Desktop | Menu | DangerItemColor, DangerItemHoverColor, ItemIconMarginInlineEnd, MenuTearOffHeight, TopLevelItemBg, TopLevelItemColor, TopLevelItemSelectedBg, TopLevelItemSelectedColor | 10.7 | |
| Desktop | TabControl | HorizontalMargin | 10.7 | |
| Desktop | Pagination | InputOutlineOffset, ItemInputBg, ItemLinkBg, PaginationMiniQuickJumperInputWidth | 10.7 | |
| Desktop | Card | ActionsSpacing, CardHeadPadding, CardPaddingBase, ExtraColor, TabsMarginBottom | 10.8 | |
| Desktop | Expander | LeftExpandButtonVMargin, RightExpandButtonVMargin | 10.8 | |
| Desktop | GroupBox | OrientationMarginPercent, TextPaddingInline, VerticalMarginInline | 10.8 | |
| Desktop | Separator | HorizontalWithTextGutterMargin | 10.8 | |
| Desktop | ListBox | ItemDisabledColor | 10.9 | |
| Desktop | ListView | ItemDisabledColor | 10.9 | |
| Desktop | TreeView | DirectoryNodeSelectedBg, DirectoryNodeSelectedColor | 10.9 | |
| Desktop | Timeline | LastItemContentMinHeight | 10.9 | |
| Desktop | TimePicker | ButtonsMargin, PeriodHostWidth, RangePickerArrowMargin, RangePickerIndicatorThickness | 10.9 | |
| Desktop | Carousel | ArrowOffset | 10.9 | |
| Desktop | Skeleton | GradientToColor, TitleHeight | 10.9 | |
| Desktop | ImagePreviewer | ImagePreviewSwitchSize | 10.9 | |
| Desktop | Form | VerticalLabelPadding | 10.10 | |
| Desktop | WindowTitleBar | CloseHoverBackgroundColor, ClosePressedBackgroundColor, ForegroundColor, HoverBackgroundColor, PressedBackgroundColor | 10.10 | |
| ColorPicker | ColorPicker | ColorPickerAlphaInputWidth, ColorPickerInputNumberHandleWidth | 14 | |
| DataGrid | DataGrid | ExpandIconBg, ExpandIconHalfInner, ExpandIconMargin, ExpandIconScale, ExpandIconSize, FilterDropdownMenuBg, FixedHeaderSortActiveBg, HeaderFilterHoverBg, SelectionColumnWidth, SortIndicatorLayoutMargin, TableBodySortBg, TableExpandColumnWidth, TableExpandIconBg, TableFilterDropdownHeight, TableFilterDropdownSearchWidth, TableFixedHeaderSortActiveBg, TableHeaderCellSplitColor, TableHeaderFilterActiveBg, TableHeaderSortBg, TableHeaderSortHoverBg, TableSelectionColumnWidth | 14 | |

Task 9 另删除 HyperLinkButton.BorderColorDisabled（该控件不绘制边框），不在原清单中。

已知限制：派生链只跟踪 `XxxToken.CalculateTokenValues` 内的直接赋值，经局部变量或私有辅助方法转手的基字段仍可能被误报；裁决时先核对这一点。
