# 主题定制精确生效重构：执行进度与接手说明

日期：2026-11-08。分支：`feature/design-language`。工作树：`/Users/chinboy/Projects/dotnet/AtomUIV6/.claude/worktrees/design-language`。

方案：`docs/superpowers/specs/2026-11-07-theme-customization-precision-design.md`。
计划：`docs/superpowers/plans/2026-11-07-theme-customization-precision.md`（含"存量基线"与"待裁决 Own Token"两张表，由控制者维护）。
执行方式：superpowers:subagent-driven-development（每任务一个实现子代理 + 一个评审子代理，修复轮次续用同一实现者，修复后做范围评审）。

## 1. 完成状态

| 任务 | 内容 | 状态 |
| --- | --- | --- |
| 1 | 显式 Own 覆盖先于默认计算（显式位、`IsExplicit`） | 完成 |
| 2 | 22 处 `IsNaN` 哨兵改写、`ATOMUIGEN040` | 完成 |
| 16（新增） | 派生基字段 `IsExplicit` 守卫、`ATOMUIGEN048`，77 处改写 | 完成 |
| 3 | `ThemeScope.TokenOwner` 在默认 ControlTheme 解析时注入 | 完成 |
| 4 | `OwnerTokenResource` | 完成 |
| 5 | 主题资产分析器 041/045（046 已退役） | 完成 |
| 6 | `[ReservedToken]`、Own 引用分析 042、消费图 | 完成 |
| 7 | 控件代码分析器 043/044/047（显式开关 `AtomUIThemeConsumptionAnalysis`） | 完成 |
| 8 | 生效测试骨架（`tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/`） | 完成 |
| 9 | Button 家族 | 完成 |
| 17（新增） | `TokenTransitions`：动效时长随作用域与主题，删除 `SharedTokenResourceValue` | 完成 |
| 18（新增） | TokenResource 在模板内取 Template 优先级 | 完成 |
| 18b（新增） | 退役 `ATOMUIGEN046` | 完成 |
| 10.1 | Input 家族 | 完成 |
| 10.2 | Select 家族（Popup 设为 owner 透明） | 完成 |
| 10.3 | Choice 家族 | 完成 |
| 10.4 | Steps / Progress 家族 | 完成 |
| 10.5 | Tag / Badge 家族（`PresetColorPalette`） | 完成 |
| 10.6 | Feedback 家族 | 完成 |
| 10.7 | Navigation 家族 | 完成 |
| 10.8 | Layout / Container 家族 | 完成 |
| 10.9a | Data 家族前半（List/Tree/Avatar/Timeline/Skeleton/Statistic/Carousel/ImagePreviewer） | 完成，最后提交 `eb8d42c85` |
| 10.9b | Data 家族后半（Calendar/LunarCalendar、DateViewer、DatePicker/TimePicker；无基础 Table） | **完成并评审通过**：原实现及后续修复至 `3e55b3883`；本轮覆盖修复 `485ef3ba7`，定向渲染 410 / 控件 360 / 门禁 250 全过 |
| 10.10a | 录入类剩余控件 | 完成并评审通过，`4bf2f1032`；240 / 205 / 287 定向检查通过，52 Own Active |
| 10.10b | 展示类剩余控件 | 完成并评审通过：`7dcd4cdee` + `6160a423b`；58 Active / 1 Reserved，透明默认修复 7 / 310 定向通过 |
| 10.10c | 窗口、滚动、共享基元与 Controls 包 | 实现与定向评审完成：`f071b5978` + `0070c62e9`；665 主验收 + 40 修复覆盖通过；当前平台窗口/波纹视觉待 15，原生跨平台限制造册 |
| 10.10d | 抽象 Token 层与一致性汇总（见计划 10.10 行） | 未开始 |
| 11 | `AtomUI.Themes.PrimeNG` | 未开始 |
| 12 | Gallery 主题预设页 | 未开始 |
| 13 | Shadcn / Bootstrap / Material | 未开始 |
| 14 | DataGrid / ColorPicker / Extras 家族 | 未开始 |
| 15 | 收尾：性能对照、全量回归、文档同步 | 未开始 |
| 终审 | 整分支评审 | 未开始 |

## 2. 接手步骤

1. 10.9b 已完成本轮评审及覆盖修复 `485ef3ba7`（I1/I2 已通过范围复审）。10.10a 已接续完成并评审通过（`4bf2f1032`）；10.10b 已实现并完成透明默认修复及范围复审（`6160a423b`）；10.10c 已完成实现与定向评审（`0070c62e9`）；下一任务为 10.10d；不要重派已完成家族。两项 minor 与原始历史 RED 凭证丢失的限制见台账，终审统一分拣。
2. 按第 3 节的家族上下文为每个家族派发实现子代理，评审后修复，直到 Task 10 全部完成，再按计划顺序做 11、12、13、14、15 与整分支终审。
3. 每个家族完成后由控制者更新计划的"待裁决 Own Token"表与"存量基线"，单独提交；实现子代理不改计划文件，并且只显式暂存自己的文件。
4. 提交信息单行、无任何署名尾注。

## 3. 家族迁移上下文（派发给每个家族的实现子代理与评审子代理）

# Family migration context (shared by every Task 10.x and Task 14 dispatch)

Read this together with your family brief. It records what earlier tasks built and the rulings that bind every family.

## Step template
Task 9 (Button family) is the template. Its brief is `task-9-brief.md` in this folder; its Steps 1–8 apply to every family:
1. Effect tests first (RED shows the family's gaps).
2. Theme read migration by TargetType identity.
3. Own Token triage (042 list rows for your family in the plan table "待裁决 Own Token"; decide Active / `[ReservedToken("reason")]` / delete; deletions go to the control's `token.md` and the CHANGELOG(.zh-CN) Unreleased Breaking sections; prefer Reserved over delete when the field maps to an antd component token of the same control).
4. AXAML literals (045) and code-side rules (043/044/047), E1–E4 items and borrowed fixes listed for your family. Since Task 18 every TokenResource read written on a ControlTemplate node is Template priority, so triggered state Setters (`:pointerover`, `:disabled`, classes) override it; you no longer need to move template defaults into Style Setters just to unmask states (ATOMUIGEN046 is retired). Property-match selectors (`^[Prop=Value]`) are activators too and override template values (the NavMenu dark group-title fix in Task 18 relied on this); only unconditional Style Setters lose to template values.
5. Family effect tests green.
6. Isolation tests: `Controls[X].Tokens[ColorPrimary]` (or the family's main global) changes X but not an unrelated control; root `Tokens[...]` changes both; a registered control nested in X's template is not affected by X's override.
7. Add the family's directories/controls to `tests/AtomUI.Generator.Tests/ThemeAssets/MigratedFamilyAnalyzerGateTests.cs` (zero 041–045, 047, 048 for migrated paths — 046 is retired; generator-reported 041/042/045/046 ignore path-scoped `.editorconfig`, so this test is the gate) and to the `.editorconfig` 043/044/047 error section used by the Button family (`src/AtomUI.Desktop.Controls/.editorconfig`). Keep family membership consistent between the two.
8. Commits: split as in Task 9, single line, `<type>(<Family>): …`, NO attribution trailers. Commit only green.

Reference implementation to copy: `tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/` — `TokenEffectHarness.AssertHasEffect(...)`, `EffectTestData.For(catalog, controlId)`, `EffectOptions`, `ThemeOwnerReads` (drift guard: parse a primitive theme's `OwnerTokenResource` reads and assert set equality with the hand list), and `ButtonFamilyEffectTests*.cs` (arrange tables per token, isolation tests, Flyout/popup handling).

## Mechanisms available (all committed)
- Compiler: explicit Own overrides are written before `CalculateTokenValues`; evaluators use `IsExplicit(<Control>TokenKind.X)`; base fields read later must be guarded (048 Error).
- AXAML reads: `{atom:XxxTokenResource X}` for TargetTypes with a Token identity; `{atom:OwnerTokenResource X}` for identity-less primitives/parts (reads the hosting registered control's effective globals via inherited `ThemeScope.TokenOwner`; in-template use gets Template priority); `{atom:SharedTokenResource X}` only with an immediately preceding `<!-- shared: reason -->` comment.
- A registered part (has its own identity) resolves `OwnerTokenResource` to itself; parts that must follow the host read the host's `XxxTokenResource` explicitly.
- Owner-transparent containers: `atom:Popup` carries the internal `[TokenOwnerTransparent]` attribute, so it injects no `TokenOwner` and popup content inherits the host control's owner (Task 10.2). Popup's own visuals read `PopupTokenResource` explicitly. Use the same attribute for any other pure container you find whose own identity wrongly cuts the host chain (report it).
- `owner:` marker: 041 reports `OwnerTokenResource` inside a ControlTheme whose TargetType has a Token identity unless the `<ControlTheme>` is immediately preceded by `<!-- owner: reason -->`; allowed only when identity-less parts reuse that theme (e.g. TextBoxTheme for EmbeddedTextBox).
- Embedded editors (EmbeddedTextBox and subclasses) inherit TextBoxTheme's owner reads: a family that embeds one must host `InputFamilyEffectTests.s_textBox` in its owner-read effect rows (precedent: NumericUpDown, Select).
- Root brush relays: use the Input family's internal projection (`HostBorderBrush` / `HostBackground` + `:host-border-brush` / `:host-background`), never LocalValue relays.
- CHANGELOG(.zh-CN) Unreleased must list every user-visible visual change your migration causes (colours, sizes, widths, newly rendered elements).
- Registered parts with no Own tokens that only read the host's tokens get `[TokenOwnerTransparent]` so `Controls[Host]` also reaches their `TokenTransitions` (precedent: SliderTrack/SliderThumb, Task 10.3).
- Opacity dimming follows antd's single root opacity where antd does so (precedent: ToggleSwitch, Task 10.3); never dim a part separately on top of a dimmed root.
- Active means the effect test proves the token's design meaning in a realistic arrange, not a side effect (precedent: Slider.SliderTrackSize ruled Reserved, Task 10.3). Consult antd 6.7.0 at `/Users/chinboy/Projects/ReferenceProjects/ant-design/components/<component>/style/` when the meaning is unclear.
- Transition durations: `<atom:TokenTransitions>` wrapping `<Transitions>`, with `atom:TokenTransitions.Duration="MotionDurationX"` on each transition (never a TokenResource on `Duration`/`Delay`).
- Literals: numeric visual literals in theme AXAML need `<!-- layout -->` on the immediately preceding sibling or must become tokens (borders/dividers → `LineWidth`); 045 covers CornerRadius, BorderThickness, FontSize, StrokeThickness, Width, Height, MinWidth, MinHeight, Padding, Margin, Opacity, FontWeight, Spacing, ZIndex, `*Offset`, signed values; colour literals are never allowed.
- Control code: read tokens only via `ControlTokenAccessor` / `TokenResourceBinder` / theme bindings; recompute on theme change (`ThemeTokenResolver.Subscribe` or bound properties with `AffectsMeasure`); preset colours from snapshot `PresetPalettes`; never write LocalValue into visual StyledProperties — use `SetCurrentValue`, or (ruling) internal properties projected by theme Setters, because in Avalonia 12 Template-priority writes lose to theme Style setters.

## Rulings that bind every family
- Derivation counts as consumption (a field feeding an Active field is Active); a global assigned directly to an Own field counts as consumed only when that field is Active.
- Code consumers in the consumption map are a lower bound; AXAML consumers are authoritative.
- Effect tests never skip: a token that cannot render in any state is Reserved (with reason) or deleted.
- The harness sets `EnableMotion=false` for pixel comparisons; TimeSpan/bool tokens assert read-back values.
- `Application.Current.TryFindResource("<named theme>")` lookups (TabItem, TabStripItem, Menu, DataGridColumn) are scope bypasses for named ControlThemes; fix them by hand in the owning family (resolve from the control itself, e.g. `this.TryFindResource`, or a typed theme property), with a test under a local `ThemeConfigProvider`.
- Do not edit `docs/superpowers/plans/*` (controller updates the plan's triage and baseline tables from your report).
- Shared-infrastructure edits outside your family are allowed only when your family needs them; keep them minimal, tested, and list them in your report.
- Borrowed fixes from `feature/custom-theme` are re-implemented by idea only (`git show <sha> -- <path>` to read), each with a test that fails without it; do not depend on mechanisms absent here (`ControlTypographyBindings`, hop-chain `OwnerTokenResource`, `ThemeStyle*`).

## Report contract
Full report to `task-<id>-report.md` in this folder: per-member read replacements (counts), each 042 decision, fixes, borrowed fixes with tests, effect-test counts per member (passed / Reserved), isolation results, analyzer counts before/after for family paths, shared-infra edits, files changed, self-review, concerns. Reply with ONLY (under 15 lines): Status; commits; one-line test summary; 042 decisions summary; concerns; report path.


## 4. 执行台账（全部裁决、延后的小问题与评审结论）

# SDD ledger — plan: docs/superpowers/plans/2026-11-07-theme-customization-precision.md

Spec: docs/superpowers/specs/2026-11-07-theme-customization-precision-design.md (reachable).
Worktree: /Users/chinboy/Projects/dotnet/AtomUIV6/.claude/worktrees/design-language, branch feature/design-language.
Plan head before Task 1: 208c14c2f.

Ruling: SDD workspace (ledger, briefs, reports, review packages) lives in the session scratchpad `…/scratchpad/sdd/`, not `.superpowers/sdd/` — user memory `no-dot-superpowers-dir` forbids `.superpowers/`; scripts are called with explicit OUTFILE — cost if wrong: none (scratch only).

## Pre-flight scan

| Pair / Task | Produces vs consumes | Finding |
| --- | --- | --- |
| T1 → T2 | `IsExplicit<TKind>(TKind)` with `XxxTokenKind` int slots | consistent; T1 Step 5 contract test guards slot = enum value |
| T1 → T6 | both touch `Theme/DesignTokens/` (new files only in T6) | no file overlap |
| T3 → T4 | `ThemeScope.TokenOwnerProperty : AttachedProperty<ControlTokenIdentity?>` | consistent; T4 reads via `GetObservable` |
| T4 ↔ provider | `ControlTokenResourceKey.Global(identity, kind)` unbound key | already supported in `ThemeTokenResourceProvider.TryGetResource` (verified) |
| T5 → T6 | both edit `TokenResourceKeyGenerator.cs`, `Diagnostics/*`; T6 consumes T5's parsed `XElement`s | sequential tasks, no conflict |
| T6 → T8 | `GeneratedThemeConsumption.OwnTokens/GlobalConsumers` record shapes | names identical in both tasks |
| T8 → T9/T10/T14 | `TokenEffectHarness.AssertHasEffect(...)`, `EffectTestData.For(catalog, id)` | consistent |
| T7 → T9 | analyzer IDs 043/044/047 | consistent with diagnostics doc |
| T11 → T12/T13 | theme Ids `PrimeNGAura/Lara/Nora`, `UsePrimeNGThemes()` | consistent |
| T2 self | DataGrid `TableBorderColor` is `internal`, may lack a TokenKind | plan already gives fallback assertion (border brush color) |
| T3 self | Setter injected after resolve, before first attach | risk: resolution via a provider that does not call `NotifyControlThemeResolved`; T3 tests cover; `StartupResourceProvider` and `StaticThemeResourceProvider` both notify (verified) |
| Global constraints vs T1 | adds `protected IsExplicit` to public `AbstractControlDesignToken` | listed as explicit exception in Global Constraints |
| Rubric conflicts | tests asserting nothing / verbatim duplication mandated? | none found |

Scan clean apart from noted risks; no rulings needed beyond workspace location.

## Task log
Task 1: dispatched implementer (sonnet) at base 208c14c2f
Task 1: implementer DONE_WITH_CONCERNS, commit 5e1828996; Ruling: brief's sample evaluator (unconditional Base=10) was self-inconsistent, test token guards Base with IsExplicit — fixture-only, no product impact; contract test placed in Desktop.Controls.Tests because Core has no Own-token descriptors — accepted
Task 1: review approved (spec ✅, 0 Critical/Important). ⚠️ resolved by controller: Core has no [ControlDesignToken] classes (grep = 0) so contract test in Desktop.Controls.Tests is the right home; NormalizedTokenValue.Descriptor for Own tokens is the descriptor.OwnTokens entry whose Slot is the Own slot (same slot TokenValueTable.Freeze uses); enum/slot ordering pinned by the passing contract test.
Task 1: minor (deferred): Explicit_Zero_Counts_As_Explicit cannot fail on value-based explicitness — use Base="0" and assert Derived==0.
Task 1: minor (deferred): OwnTokenKindContractTests covers only Desktop.Controls assembly; extend to DataGrid/ColorPicker/Extras when their families migrate (Task 14).
Task 1: minor (deferred): OwnTokenKindContractTests reflection uses Single(type.Name==...) — filter by namespace / clearer failure message.
Task 1: minor (deferred): MarkExplicit resizes per word; could presize from OwnTokens.Count. ResetExplicit has no production caller.
Task 1: complete (commits 208c14c2f..5e1828996, review clean)
Task 2: dispatched implementer (sonnet) at base 5e1828996
Task 2: implementer DONE_WITH_CONCERNS, commit ecb2626eb. Concern 1 (real, structural): evaluators assign every Own field unconditionally, so a derived field computed in the same method (DataGrid TableBorderColor = BorderColor) still reads the default, even though ApplyExplicit restores the base field afterwards. Ruling: the "derived fields follow automatically" claim in spec §3.2 / runtime.md §7 only holds when the base field's default assignment is guarded by IsExplicit; add a new task (Task 16, run before Task 3) with analyzer ATOMUIGEN048 (Error: unguarded assignment in CalculateTokenValues to an Own property that is read later in the same method) and the mechanical guard rewrite across all token classes; update spec/runtime/guidelines/diagnostics docs accordingly — cost if wrong: one extra analyzer and ~N guard lines; without it the A′ class of bugs stays.
Task 2: review Needs fixes — Important 1: NavMenuToken.cs:379 `ItemContentMargin != default` sentinel remains; Important 2: systemic unguarded-base-field gap → covered by Ruling above (Task 16). Fix round 1 started (resume implementer).
Task 2: minor (deferred): Button explicit-override tests pass on old sentinel code once Task 1 is in (artificial RED); add recompute scenario.
Task 2: minor (deferred): HyperLinkButton/DropdownButton/Menu/NavMenu lack control-level explicit-override tests.
Task 2: minor (deferred): ATOMUIGEN040 detects only direct IsNaN(<Own property>); note in doc row.
Task 2: minor (deferred): DataGridBorderColorOverrideTests uses Single(Name=="Frame").
Plan: Task 16 (ATOMUIGEN048 + derivation-base guards) appended to plan file, commit pending in docs; brief to extract.
Plan: Task 16 committed in 158ec81ed (plan + spec + runtime + guidelines + diagnostics docs).
Task 2: fix round 1/5 (1 addressed, 0 open — ItemContentMargin default sentinel → IsExplicit; commits ecb2626eb..9ba2166a4)
Task 2: ⚠️ resolved by controller: repo grep found no IsNaN in any *Token.cs outside the 5 files; NavMenuTokenKind.ItemContentMargin exists (used in fix, build green).
Task 2: complete (commits 5e1828996..9ba2166a4, review clean)
Task 16: dispatched implementer (sonnet) at base 9ba2166a4 (runs before Task 3 per ruling)
Task 16: implementer DONE, commits ad38cb541 (analyzer, 14 tests) + 60db4d577 (sweep: Controls 0 | Desktop.Controls 50 | DataGrid 24 | ColorPicker 3 | Extras 0 = 77 guards / 24 files); reviewer dispatched
Task 16: review Needs fixes — Important 1: IsGuarded matches `IsExplicit` and Kind member by name, not symbol. Fix round 1 started (resume implementer).
Task 16: minor (deferred): HasAttribute/IsOwnTokenOfType/controlName/TokenSuffix duplicated between 040 and 048 analyzers — extract shared helper.
Task 16: minor (deferred): 048 tests assert few locations; no multi-statement-block / nested-if / this.IsExplicit / lambda-read tests.
Task 16: minor (deferred): DataGridToken.cs orphaned comment above CellPaddingMD guard; alignment padding and blank lines inside guard blocks; using placement after BOM in ButtonSpinnerToken/SliderToken/TabControlToken.
Task 16: minor (deferred): OptionButtonToken keeps dead `-1` sentinel inside guards; ContentPaddingLG/SM use wrong operands (pre-existing) — fix in Button family task (Task 9).
Task 16: ⚠️ resolved by controller: Controls/Extras report 0 because their token classes assign derived fields from EffectiveGlobalToken only (no Own-field read-after-write) — plausible; zero-048 claim backed by implementer's build output in report.
Task 16: fix round 1/5 (1 addressed, 0 open — symbol-based IsGuarded; commits 60db4d577..e854a30b2)
Task 16: complete (commits 9ba2166a4..e854a30b2, review clean)
Task 3: dispatched implementer (sonnet) at base e854a30b2
Task 3: implementer DONE, commit 77a014192 (5 tests, Core.Tests 377/377); concern: tests call NotifyControlThemeResolved directly, provider call sites unchanged; reviewer dispatched
Task 3: review approved (0 Critical/Important). ⚠️ resolved by controller: resource lookups are UI-thread in Avalonia, lock + CWT sufficient; TryGetControl(Type) throwing for a disabled package is not reachable for a theme shipped by that package — tracked as minor; provider→injection chain to be exercised end-to-end by Task 4's real-control tests.
Task 3: minor (deferred): Replaced_Theme_Without_BasedOn_Has_No_Owner passes trivially; add BasedOn-chain positive companion.
Task 3: minor (deferred): no test through StaticThemeResourceProvider/StartupResourceProvider for the notification wiring (Task 4 integration tests should cover).
Task 3: minor (deferred): ThemeManager field placement; ThemeScope using order; EnsureTokenOwnerSetter should swallow/guard a throwing TryGetControl for disabled packages.
Task 3: complete (commits e854a30b2..77a014192, review clean)
Task 4: dispatched implementer (opus) at base 77a014192
Task 4: implementer DONE, commit a9c5560ea (Core.Tests 387/387; binding built on CompiledBinding + IPropertyAccessor because IBinding/BindingBase.CreateInstance are internal in Avalonia 12). Ruling: brief's "ProvideValue must not touch the service provider" was meant as "must not resolve owner/resources at ProvideValue"; the extension MAY query the service provider for template/priority context so that a direct use on a template element gets Template (not LocalValue) priority, exactly as DynamicResourceExtension does — otherwise our own extension would violate rule P1. Relax the ThrowingServiceProvider test to allow those service requests. Cost if wrong: one extra service lookup at ProvideValue.
Task 4: review Needs fixes — Important 1: direct in-template use gets LocalValue priority (query IAvaloniaXamlIlControlTemplateProvider, set BindingPriority.Template); Important 2: catch(InvalidOperationException) around lookup swallows snapshot materialization failures and sticks to global key (decide "unregistered" via inherited ThemeScope.Context snapshot registry, no exception-based fallback, don't persist fallback). Fix round 1 started (resume implementer).
Task 4: minor (deferred): owner subscription uses target.PropertyChanged for all properties; prefer property-specific observer.
Task 4: minor (deferred): handler-count test helper reflects Avalonia private fields; brittle.
Task 4: minor (deferred): hot-path test measures lookup only; rename/comment.
Task 4: minor (deferred): redundant double refresh on variant-changing publish (same as DynamicResource).
Task 4: minor (deferred): non-StyledElement targets silently UnsetValue; consider debug diagnostic.
Task 4: ⚠️ resolved by controller: scoped ThemeConfigProvider publish relies on root ResourcesChanged propagating down the logical tree (same signal DynamicResource uses) — Task 9 family effect tests run under a ThemeConfigProvider and will exercise it; commit trailers verified by controller via git log (none).
Task 4: fix round 1/5 (2 addressed, 0 open — Template priority in templates; registry-based unregistered check, no exception fallback; commits a9c5560ea..d7328d283)
Task 4: minor (deferred): FirstChanceException-based test is process-wide → possible flake; filter by thread.
Task 4: docs sync (deferred to Task 15): runtime.md §13 describes OwnerTokenResource as "一个继承属性订阅"; implementation is CompiledBinding + target PropertyChanged/ResourcesChanged/ActualThemeVariantChanged — update wording.
Task 4: complete (commits 77a014192..d7328d283, review clean)
Task 5: dispatched implementer (sonnet) at base d7328d283
Task 5: implementer DONE, commit dfe877967 (66 analyzer tests; counts 041 Controls 23 / Desktop 1714, 045 0 / 90, 046 3 / 29); concern: identity resolved only against same-package ControlThemeInfos — cross-package TargetTypes misclassified; reviewer dispatched
Task 5: review Approved-with-follow-ups; Important 1 (plan-mandated): 041 identity set is package-local. Ruling: fix now, not later — union referenced assemblies' ThemeBuildOwnerAttribute Ids (pattern ThemeBuildContracts.cs:48-75) into the identity set, because Desktop themes for Controls-package controls and third-party packages will hit this during family migration; cost if wrong: one extra reference walk per generation. Fix round 1 started.
Task 5: Ruling: brief Step 4 said to write the baseline into the plan file; global constraint reserves plan edits to the controller — controller writes the baseline (041 23/1714, 045 0/90, 046 3/29 before fix round).
Task 5: minor (deferred): tests lack SharedTokenResourceValue negative, Kind=X,Extra form, nested ^ inheritance, multi-state one-report, multi-theme count; MasksSetter test uses ShouldContain; colour Theory mixes a negative case.
Task 5: minor (deferred): 046 misses property-bracket-only states and DynamicResource/StaticResource/TemplateBinding defaults (deliberate narrowing); 041 Mode-before-Kind ordering theoretical; `layout` substring exemption loose; 045 skips negative numbers.
Task 5: note: 12 Splash reads unscanned because SplashDefaultTheme.axaml is Linked outside Themes/ — handle in family task 10.10.
Task 5: fix round 1/5 (1 addressed, 0 open — referenced ThemeBuildOwner Ids unioned into 041 identity set; commits dfe877967..e4d79b37f)
Task 5: minor (deferred): referenced-owner lookup by ToDisplayString string compare (same idiom as ThemeBuildContracts); simple-name matching ignores catalog; no test for absent attribute type.
Task 5: complete (commits d7328d283..e4d79b37f, review clean). Baseline written to plan in 05a3aceb0.
Task 6: dispatched implementer (sonnet) at base 05a3aceb0
Task 6: implementer DONE_WITH_CONCERNS, commits 02b28ffc4 + 541455c41 (585 generator tests; 042 totals Controls 3 / Desktop 110 / ColorPicker 3 / DataGrid 38 = 154).
Ruling: an Own field read inside its own CalculateTokenValues to compute another Own field counts as Active when (transitively) that other field is Active — derivation is consumption; fields that only feed Unreferenced fields stay Unreferenced. Otherwise 042 flags DataGrid.HeaderBg / ContentLineHeight* bases that are genuinely load-bearing. Sent back to implementer before review (correctness concern). Cost if wrong: some base fields hidden from 042 that are actually dead, caught later by the effect tests.
Ruling: a referenced field carrying [ReservedToken] (stale attribute) is out of scope for 042; defer as minor — cost: stale Reserved markers survive until Task 15 doc sync.
Task 6: derivation ruling applied in 508f603dc; 042 totals now Controls 3 / Desktop 99 / ColorPicker 2 / DataGrid 21 = 125; reviewer dispatched
Task 6: review approved (0 Critical/Important). ⚠️ resolved by controller: ControlName == schema ControlId (generator emits ControlTokenIdentity.ForControl(type, "AtomUI", "<ControlName>")). Ruling: GalleryBase/controlgallery 042 warnings are outside the five-package promise surface inventory; triage them in Task 12 (Gallery) — cost: some Gallery-only warning noise until then.
Ruling: the broadened code-consumer rules (any SharedTokenKind.<Global> mention, <Control>TokenKey.<Global>) are ratified; Task 8 treats code consumers as a lower bound and AXAML consumers as authoritative — cost: a few extra global-token effect tests that may need a state setup.
Task 6: minor (deferred): whole-package syntax scan without per-tree text prefilter / not incrementally cached.
Task 6: minor (deferred): derivation misses reads in if-conditions, locals, helpers, non-Own intermediates; AXAML element-text markup not scanned; per-package scope reports Controls-package fields consumed only by higher packages (Icon.FallbackColor, Avatar group fields — check in triage).
Task 6: minor (deferred): test gaps — TokenKey.<Global> branch, OwnerTokenResource outside ControlTheme, EffectiveGlobalToken outside Token class, Reserved non-propagation, compound assignment; collector field placement; 042 filter in shared test helpers vs [ReservedToken] remedy inconsistency.
Task 6: complete (commits 05a3aceb0..508f603dc, review clean). Triage table written to plan in f00f032f7.
Task 7: dispatched implementer (sonnet) at base f00f032f7
Task 7: implementer DONE, commit b174707bf (43 tests; counts 043/044/047: Controls 0/19/11, Desktop 3/4/28, DataGrid 0/0/0, ColorPicker 0/23/8, Extras 0/0/0).
Ruling: 043 stays token-key-only. TabItem/TabStripItem/Menu `Application.Current.TryFindResource(<string ControlTheme key>)` and DataGridColumn runtime-key lookups are named-theme scope bypasses (spec E3), not token reads; they are fixed by hand in Tasks 10.7 / 14 rather than widening 043 — cost: those sites are not machine-guarded.
Ruling: `this.TryFindResource(<Own TokenKind>)` in Window.cs is correctly flagged — one-shot read without subscription violates consumption rule 3 even though it respects scope.
Task 7: review approved-with-fixes; Important 1 (plan-mandated): scope gate (SharedTokenKind resolvable) also fires in downstream apps. Ruling: gate = SharedTokenKind resolvable AND not its defining assembly AND (compilation declares a [ControlDesignToken] type OR analyzer global option build_property.AtomUIThemeAssetProjectDirectory is non-empty); add app-project negative test — cost if wrong: a control package without tokens and without theme assets escapes analysis (none known).
Task 7: Important 2: 044 name-suffix matching over-reaches. Ruling: exact visual member names only; assignment targets must resolve to a field/property symbol whose name equals a visual name after stripping a leading `_`/`s_` and upper-casing the first letter; locals and parameters never reported; add tests incl. `_legacyWindowsFrameBorderThickness` (not reported) and `_borderThickness` (reported) — cost if wrong: some compound-named visual constants slip through.
Task 7: fix round 1 started (resume implementer).
Task 7: minor (deferred): 043 fallback region scans unrelated later statements; `read is false` not treated as negation.
Task 7: minor (deferred): 043 method identity via Avalonia* assembly-name prefix instead of resolved Application/IResourceHost/ResourceNodeExtensions symbols; static-call extension form key index.
Task 7: minor (deferred): 044 GetSymbolInfo on every member access / object creation — add syntactic prefilters for IDE perf; double reports 043+044 on fallback Brushes.X.
Task 7: minor (deferred): 047 set-accessor exemption too wide; test host coupled to TypeMapRegistrationGeneratorTests.Compilation; untested paths BoxShadows.Parse, ControlTokenResourceKey.Own, FontWeight/BoxShadows, nested ControlDesignToken, ctor-local function, generated-code skip.
Task 7: fix round 1/5 (1 addressed, 1 open — exact names ADDRESSED; scope gate NOT addressed in effect: AtomUIThemeAssetProjectDirectory is set unconditionally for every consumer incl. apps; commits b174707bf..cad2daa49)
Ruling (supersedes Ruling A, my premise was wrong): gate = SharedTokenKind resolvable AND not defining assembly AND (compilation declares a [ControlDesignToken] type OR new compiler-visible MSBuild property `AtomUIThemeConsumptionAnalysis` == true). The property defaults to false, is set true in the five first-party control package projects (Controls, Desktop.Controls, DataGrid, ColorPicker, Extras), and third-party control packages opt in (documented in third-party guide + diagnostics doc). Verify with `dotnet msbuild -getProperty` that AtomUIGallery.Desktop evaluates false and the five packages true. Cost if wrong: a third-party package without tokens that forgets to opt in is not analyzed.
Task 7: fix round 2 started (resume implementer).
Task 7: minor (deferred): DeclaresAttributedType full-namespace walk runs up to 3× per compilation; share a per-compilation cache.
Controller: plan baseline 044 row corrected (Desktop 4→2) in working tree — UNCOMMITTED; commit after Task 7 round 2 lands (implementer may stage it — check).
Task 7: fix round 2 implementer DONE, commit 89d953ee0 (650 tests; msbuild: Gallery.Desktop/GalleryBase empty, five packages true; counts unchanged). Note: controller's uncommitted plan baseline edit (044 Desktop 4→2 + E2 note) was swept into 89d953ee0 — content correct, commit subject does not mention it; accepted.
Ruling: GalleryBase declares six [ControlDesignToken] types, so it is a themeable control package and its 10 diagnostics (043×2 GalleryMediaBreakPointResolver.cs:80-81; 044×4 PaletteColorInfo.cs:19,23, GalleryShellView.cs:418,470; 047×4 GalleryCodeViewer.cs:279,299, GalleryShellView.cs:266,279) are legitimate; triage in Task 12 — cost if wrong: 10 warnings of noise in GalleryBase builds.
Task 7: fix round 2/5 (1 addressed, 0 open — explicit AtomUIThemeConsumptionAnalysis opt-in gate; commits cad2daa49..89d953ee0)
Task 7: minor (deferred): ControlCodeScope class <summary> stale (mentions theme assets); no test for padded/case-variant " TRUE "; no end-to-end CompilerVisibleProperty test.
Task 7: complete (commits f00f032f7..89d953ee0, review clean)
Task 8: dispatched implementer (opus) at base 89d953ee0
Task 8: implementer DONE, commit 3bc848718 (33 harness self-tests; Rendering.Tests 110/110).
Ruling: harness sets root EnableMotion=false before pixel comparisons (transitions otherwise hide the second frame's change); TimeSpan/bool tokens use the unmodified config — accepted, matches spec intent "有变化", cost if wrong: motion-only visual regressions are not caught by effect tests (they are covered by TimeSpan value assertions).
Ruling: the spec's root `Tokens[...]` spot check is done per family in Task 9 Step 6 / Task 10 isolation tests, not in the harness — cost: none.
Task 8: note for Task 9: ~68 Button rows show no change in default state; family task must provide arrange setups.
Task 8: review Approved with 2 Important. Important 1 (plan-mandated): BoxShadows perturbation lands outside the control-bounds crop. Ruling: for BoxShadows-typed tokens the compared region is the control bounds inflated by the maximum extent of the baseline and perturbed shadows (|offset| + blur + spread), clamped to the window; update theme-effect-tests.md §3 accordingly — cost if wrong: slightly larger compared area dilutes the ratio for shadow tokens only.
Task 8: Important 2: pixel path untested for Thickness/CornerRadius/BoxShadows → add committed Button cases (ContentPadding, a CornerRadius token, a BoxShadows token such as DefaultShadow/PrimaryShadow in the state where it renders).
Task 8: fix round 1 started (resume implementer).
Task 8: minor (deferred): ResolveThreshold silently lowers a caller-supplied larger threshold for thin-line tokens; perturbed SolidColorBrush keeps original opacity (read-back risk); GetPixelBounds assumes scale 1; Own/global name collision unguarded; EffectTestData duplicated Select/Map; fail-path anchor GutterToFlyout depends on 042 list; Thread.Sleep polling.
Task 8: ⚠️ resolved by controller: consumption sources merged = Controls, Desktop.Controls, DataGrid, ColorPicker (Extras has 0 entries; GalleryBase out of scope); thin-line name set to be re-checked when Task 9 hits a border-width token.
Task 8: fix round 1/5 (2 addressed, 0 open — shadow-extent region; Thickness/BoxShadows/CornerRadius pixel cases; commits 3bc848718..acc3408ed)
Task 8: minor (deferred): no integration test proves the shadow inflation is needed; doc omits negative-spread clamp; pre-existing xUnit1051 warning in Rendering.Tests.
Task 8: complete (commits 89d953ee0..acc3408ed, review clean)
Ruling: OptionButton joins the Button family in Task 9 (its 042 fields are assigned to Task 9 in the plan table) — cost: larger Task 9.
Task 9: dispatched implementer (opus) at base acc3408ed. Carried: Button BorderRadius read via SharedTokenResource (Task 8 finding); ~68 Button rows need arrange setups; OptionButtonToken dead -1 sentinel and wrong-operand ContentPaddingLG/SM (Task 16 minor); Button explicit-override tests lack a recompute scenario (Task 2 minor).
Task 9: implementer DONE_WITH_CONCERNS, commits 23e323571 fcaafcbf4 67b7935d6 f5259356b (family effect tests 309 + 3 isolation; Rendering 431, Desktop.Controls 4155, Generator 670, Gallery 412; family 041–048 214 → 0).
Ruling: generator-reported 041/042/045/046 ignore path-scoped .editorconfig severity; the family gate is the test MigratedFamilyAnalyzerGateTests (asserts zero 041–048 for migrated family paths); every later family adds its paths to that test — cost if wrong: gate only runs with tests, not in plain builds.
Ruling: Transition durations read via SharedTokenResourceValue (Application-wide static) cannot honour per-control motion overrides and 041 does not see it. Structural gap → new Task 17 after Task 10: make transition durations read the control's effective value (or extend OwnerTokenResource/XxxTokenResource to a value form) and extend 041 to flag SharedTokenResourceValue in control themes — cost if wrong: per-control MotionDuration* overrides silently ineffective until Task 17.
Ruling: OwnerTokenResource on a registered part resolving to the part's own identity is by design (registered parts carry their own TokenOwner); parts that must follow the host should read the host's XxxTokenResource explicitly.
Ruling: Avalonia 12 Template-priority writes lose to theme Style setters → guideline P3/P4 text must prefer internal-property projection (P4) over Template-priority writes for nested controls; doc fix in Task 15 — cost: guideline briefly inaccurate.
Plan: Task 17 (SharedTokenResourceValue → scoped motion durations; ~80 files; 041 extension) and Button-family 042 decisions committed in 8230f11f2. Ruling: run Task 17 right after Task 9, before Task 10 families, so families do not migrate twice — cost: Task 10 starts later.
Task 9: review Needs fixes — 4 Important.
Ruling (I1): ZIndex literals get `<!-- layout -->`; BadgeOffset literals get `<!-- layout -->` (antd has no badge-offset token); extend 045's numeric property set with `Spacing`, `ZIndex`, and any attribute name ending in `Offset` (generator change + tests) so later families do not inherit the blind spot — cost if wrong: more 045 warnings to annotate in later families.
Ruling (I2): MigratedFamilyAnalyzerGateTests must not read stale build output: add ProjectReference (ReferenceOutputAssembly=false) to AtomUI.Controls and AtomUI.Desktop.Controls so generated files are fresh, and assert >0 family Active entries matched.
Ruling (I3): owner-read effect rows get a drift guard: the test parses the primitive theme AXAML for OwnerTokenResource reads and asserts set equality with the hand list (consumption map attributes them to shared-primitive, so it cannot drive per-primitive rows) — cost: one AXAML parse per test class.
Ruling (I4): OptionButton ButtonBackground/ButtonCheckedBackground/ButtonPadding have antd Radio counterparts (buttonBg/buttonCheckedBg/buttonPaddingInline) → restore them; wire ButtonPadding into the horizontal part of ContentPadding* if it renders (Active with effect test), otherwise Reserved("antd Radio buttonPaddingInline; …"); ButtonBackground/ButtonCheckedBackground Reserved with antd mapping reason unless wireable; revert their CHANGELOG/token.md deletion entries — cost: three Reserved fields kept.
Ruling (concern 5): DropdownButton/SplitButton consume Button's Own variant tokens, so Controls[DropdownButton].Tokens[ColorText] does not change their default text. Structural: move Button variant Own tokens into an abstract [ControlDesignToken] layer shared by ButtonToken and a new DropdownButtonToken terminal (Control Design Token inheritance is supported). Deferred to new item in Task 10.10 — cost: per-control override of DropdownButton variant colours ineffective until then.
Task 9: fix round 1 started (resume implementer).
Task 9: minor (deferred): Button.Variants.cs `is DropdownButton` identity pick; Blur=3 literal in preset shadow; FontWeight inheritance change not in CHANGELOG; stale comment SplitButtonTheme.axaml:54; .editorconfig lists inert 041/042/045/046 entries; family membership in three places; test helper duplication (settle loops, config builders, reflection on EnablePopupOverlayLayer, `_ = isOwn;`); arranges with user visuals (SteelBlue backgrounds) and OptionButton ColorBorder BorderBrush renders nothing; no regression tests for 047 CornerRadius fixes; Ghost arrange without dark background; generator RemoveAll(!HasOwnToken) ordering unverified.
Controller: plan Task 10.10 row extended with DropdownButton/SplitButton variant-token follow-up — UNCOMMITTED in working tree; commit after Task 9 fix round 1 lands.
Task 9: fix round 1 implementer DONE, commits 047f17aa3 (Generator: 045 Spacing/ZIndex/*Offset/signed numbers; consumption-map rule) + c2642515e (Button). 17 new 045 warnings in other families (Desktop 9: Calendar 1, Slider 2, SplitView 4, TabControl 1, Window 1; DataGrid 4; ColorPicker 3; GalleryBase 1). OptionButton: ButtonPadding Active; ButtonBackground/ButtonCheckedBackground Reserved.
Ruling: `EffectiveGlobalToken.X` assigned directly to an Own field counts as global consumption only when that Own field is Active (reads through locals still count) — consistent with the derivation ruling; globals feeding only Reserved/Unreferenced fields could never pass an effect test. Cost if wrong: a global consumed only via a Reserved field disappears from the map (correct: it has no visible effect).
Controller: plan Task 10.10 edit committed separately.
Task 9: fix round 1/5 (4 addressed, 0 open — layout annotations + 045 widened; gate ProjectReferences + non-empty check; ThemeOwnerReads drift guard; OptionButton antd tokens restored; commits f5259356b..c2642515e)
Task 9: complete (commits acc3408ed..c2642515e, review clean). Plan updated in 4d8200656.
Task 17: dispatched implementer (opus) at base 4d8200656 (runs before Task 10 per ruling)
Task 17: implementer DONE, 7 commits da5666b82..f042790a2 (TokenTransitions: per-target binding, deferred content, cache ≤ 8 per Setter; 79 files migrated; SharedTokenResourceValue deleted; 041 flags it). Core 403, Desktop.Controls 4158, Rendering 437, Generator 691.
Ruling: a duration change replacing the control's Transitions (running transition stops) is acceptable — duration changes only happen on theme publish — cost: a one-frame animation cut on theme switch.
Task 17: review Needs fixes — Important 1: IndicatorScrollBarThumbTheme.axaml:16 transition Duration via SharedTokenResource MotionDurationMid (pre-existing, unscoped); 041 suggests OwnerTokenResource which zeroes Duration on a Transition. Fix: migrate to TokenTransitions; 041 reports any TokenResource on a transition Duration/Delay with the TokenTransitions remedy. Fix round 1 started (resume implementer).
Ruling: two descriptors under ID 041 conform (same root cause: theme reads a token outside the receiving control's effective scope) — keep; cosmetic Title fix optional.
Task 17: minor (deferred): beyond-8 vectors rebuild+republish every refresh (compare last published durations first); TokenTransitions in ControlTemplate gets no factory sharing; non-TimeSpan kind crashes at runtime (validate in DiscoverKinds or analyzer); subscription plumbing duplicated with OwnerTokenBinding (shared base); per-control cost not documented in runtime.md §13; test gaps (LocalValue Transitions survives, IsMotionEnabled activator toggle, beyond-8, WaitUntil silent timeout).
Task 17: fix round 1/5 (1 addressed, 0 open — thumb transition migrated; 041 TokenTransitions remedy for Duration/Delay reads; commits f042790a2..86271c61b)
Task 17: complete (commits 4d8200656..86271c61b, review clean)
Ruling: Task 10 runs one family per dispatch (10.1 … 10.10), each a full implement/review loop like Task 9; brief = Task 10 table + Task 9 text as the step template.
Task 10.1: dispatched implementer (opus) at base 86271c61b
Task 10.1: implementer DONE_WITH_CONCERNS, commits 01c11e325 42a48b93b e986a8f20 41705bbfc (InputFamilyEffectTests 337; Rendering 777, Desktop.Controls 4161, Generator 720, Core 404; family 041–048 236 → 0). 042: LineEdit.InputFontSizeSM Active, TextArea.ResizeHandleSize Active, ButtonSpinner.ControlWidth Reserved; HoverBorderColor not added.
Ruling (R1): a registered control's theme that identity-less parts reuse (TextBoxTheme for EmbeddedTextBox, ButtonSpinnerTheme for NumericUpDownSpinner) may read globals with OwnerTokenResource — standalone it resolves to the control's own injected identity, embedded it resolves to the host; reviewer to confirm 041 accepts this explicitly and not via a blanket hole.
Ruling: HoverBorderColor not added — accepted; PrimeNG hover border difference stays a README known difference (Controls[X].ColorPrimaryHover available).
Ruling: shared visual changes (input focus shadows now render; owner injection limited to registered type affects BasedOn themes) are bug fixes, accepted if tested — reviewer to check BasedOn behaviour.
Ruling: NumericUpDown borrowing ButtonSpinner handle Own Tokens and AutoCompleteSearchEdit/AutoCompleteTextArea borrowing AutoComplete Own Tokens join the 10.10 abstract-token-layer item; SearchEdit's search Button transitions following SearchEdit while colours follow Button also go to 10.10.
Carry to 10.2: Select/ComboBox relay root brushes as LocalValue → move to HostBorderBrush-style internal projection.
Task 10.1: review Needs fixes — 2 Important.
Ruling (I1): R1 allowance must be explicit: 041 reports OwnerTokenResource reads inside a ControlTheme whose TargetType has a Token identity unless the ControlTheme element is immediately preceded by `<!-- owner: reason -->`; TextBoxTheme and ButtonSpinnerTheme get the marker; generator tests (positive without marker, negative with marker, identity-less TargetType unaffected); guidelines AXAML 访问契约 + diagnostics doc 041 row updated — cost: one more comment convention.
Ruling (I2): verified antd 6.7.0 components/input/style/token.ts:136 `mergedFontSizeSM = inputFontSizeSM || mergedFontSize` → small input is 14px. LineEdit `InputFontSizeSM` default must be `InputFontSize` (not FontSizeSM); SearchEdit follows the same token; LG stays `FontSizeLG` (16). Update the effect test and token.md.
Task 10.1: fix round 1 started (resume implementer).
Task 10.1: minor (deferred): NumericUpDown embedded editor hard-set SizeType=Custom so LG/SM typography does not follow size (antd InputNumber LG 16px) → Task 10.10 item; LineEdit preset changes visible in ColorPickerInput (Small), QuickJumperBar, TransferItemDecorator and IsCustomFontSize=False stops ancestor FontSize inheritance — record in CHANGELOG; owner injection exact OfType affects app subclasses w/o StyleKeyOverride — CHANGELOG note; inset shadows invisible on geometry path; drift guard hand list of 13 themes (add discovery check); MapRows swallows ArgumentException; borrowed-fix tests miss suffix presenter; lost indentation in 6 files; intermediate commits not individually verified green.
Controller: plan row LineEdit.InputFontSizeSM corrected (SM follows InputFontSize) — UNCOMMITTED; commit after 10.1 fix round 1 lands.
Task 10.1: fix round 1 implementer DONE, commits e925b4b79 + f103dc1d2. Ruling: small SearchEdit reads its own effective FontSize (SearchEdit has no Own Token; reading LineEdit's InputFontSizeSM would leak Controls[LineEdit] into SearchEdit) — matches antd small=14 — accepted.
Task 10.1: fix round 1/5 (2 addressed, 0 open — `owner:` marker enforced by 041; SM font follows InputFontSize; commits 41705bbfc..f103dc1d2)
Task 10.1: minor (deferred): no test pins default small LineEdit = 14px or explicit InputFontSize moving small size; 041 doc row long.
Task 10.1: complete (commits 86271c61b..f103dc1d2, review clean). Plan commit 57834afde.
Task 10.2: dispatched implementer (opus) at base 57834afde
Task 10.2: implementer DONE_WITH_CONCERNS, commits 6aaaf847d + bec942a5a (SelectFamilyEffectTests 273; Rendering 1048, Generator 750, Desktop.Controls 4161; family 041–048 → 0). 042: Select.OptionPadding Active, ComboBox.ItemDisabledColor Active, Select.SelectAffixPadding + FixedItemMargin Reserved.
Ruling (C1, pending fix round): the registered `atom:Popup` injects its own TokenOwner and breaks the host chain for drop-down content. Popup is a pure container: add a type-level opt-out (e.g. `[TokenOwnerTransparent]` on Popup, honoured by ThemeManager.EnsureTokenOwnerSetter) so Popup content inherits the host's owner; Popup's own visuals keep reading `PopupTokenResource` explicitly. Keep 10.2's explicit host reads in drop-downs (they stay correct); transitions inside drop-downs then follow the host. Tests: Controls[Select] MotionDurationMid reaches drop-down transitions; standalone Popup content falls back to global; Button-family Flyout tests still green — cost if wrong: popup content of every family changes owner (intended).
Ruling (C2): Cascader/TreeSelect reading SelectToken join the 10.10 abstract-token-layer item.
Ruling (C4): visible fixes go into CHANGELOG(.zh-CN) Unreleased (if not already) — reviewer to check.
Task 10.2: review Needs fixes — I1 CHANGELOG missing ComboBox placeholder colour, Cascader disabled-icon tint, TreeSelect 300px min popup width; I2 embedded filter editor's TextBoxTheme owner reads have no rows (add s_textBox host to Select/Cascader/TreeSelect). Fix round 1 = I1 + I2 + C1 Popup owner-transparency (ruling above) incl. flipping Isolation.cs:44 and guideline bullet control-token-guidelines.md:233-237, plus a Controls[Popup] negative case.
Ruling: single-host template parts may read the host's TokenResource explicitly or OwnerTokenResource — both correct after Popup transparency; no churn — cost: two conventions in the codebase.
Task 10.2: minor (deferred): open-state styles (:outline/:filled[IsDropDownOpen]) + re-projection untested; Cascader disabled-icon fix lacks failing-without test; test helpers duplicated (WithControls/SettleWindow/ChangedRatioOf/EnablePopupOverlayLayer) → hoist to Effect/; MapRows swallows ArgumentException; dead IconSize test branch; TreeSelect.ConfigurePopupMinWith duplicates base; report understated indexer LocalValue writes (ComboBox.cs:654,656 etc.); Select.OptionFontSize public StyledProperty dead API.
Task 10.2: fix round 1/5 (3 addressed, 0 open — CHANGELOG; s_textBox rows; Popup [TokenOwnerTransparent]; commits bec942a5a..a4942962c)
Task 10.2: minor (deferred): TokenOwnerTransparent Inherited=false (future Popup subclasses must re-apply); no tests for non-Select popup owner change (ToolTip/Flyout follow host) — intended; PopupTheme still reads SharedTokenResource for motion (Popup migrates in 10.6).
Task 10.2: complete (commits 57834afde..a4942962c, review clean). family-context.md updated with Popup transparency, owner: marker, embedded-editor rows, projection, CHANGELOG rule.
Task 10.3: dispatched implementer (opus) at base a4942962c
Task 10.3: implementer DONE_WITH_CONCERNS, commits 7725c0856 48445c207 5c794b1ae (ChoiceFamilyEffectTests 141; Rendering 1196; gate 81; Desktop.Controls 4161; family 041–048 66 → 0). 042: RadioButton.RadioBgColor Active, Slider.MarginPartWithMark Reserved. E2 SwitchKnob pen → LineWidth. New src/AtomUI.Controls/.editorconfig.
Pending ruling (C1): SliderTrack/SliderThumb are registered parts reading only Slider tokens, so TokenTransitions follow Controls[SliderTrack/Thumb]. Proposed: if they have no Own tokens and nothing reads their identity, mark them [TokenOwnerTransparent] so transitions follow Slider — decide after review.
Task 10.3: review Needs fixes — 3 Important.
Ruling (I1): ToggleSwitch disabled dimming follows antd: one root opacity (`SwitchDisabledOpacity` applied once to the whole switch via a `:disabled` Style Setter on the root), remove the groove-only PushOpacity(SwitchOpacity) dim and the knob's own dim; loading keeps `LoadingBgOpacity`; restore knob default to fully opaque; correct CHANGELOG reason; effect + visual test — cost if wrong: focus outline of a disabled switch also dims (antd does the same).
Ruling (I2): Slider.SliderTrackSize → [ReservedToken] with reason (cross size pinned to ControlHeight, rail inset follows thumb; antd controlSize semantics not implemented); remove the side-effect effect row; note antd default 10 vs AtomUI 12 in token.md — cost: one Reserved field.
Ruling (I3, C1): mark SliderTrack and SliderThumb [TokenOwnerTransparent] (no Own tokens, no identity readers); flip Isolation.cs:128 to SliderTokens.Identity; CHANGELOG claim becomes true.
Task 10.3: fix round 1 started (resume implementer).
Task 10.3: minor (deferred): Isolation.cs:167 CornerRadius(4) equals default; dead MapRows try/catch; SwitchKnob.LoadIndicatorThickness default 0; SwitchKnob delta 2.5 fixed for both sizes (antd (handleSize−iconSize)/2); AbstractToggleSwitch.cs:575-581 LocalValue relays IconSize/Foreground into user icons (Input-family defect class) → follow-up; comment drift in gate test and Desktop .editorconfig; third copy of isolation helpers.
Task 10.3: fix round 1/5 (3 addressed, 0 open — root disabled opacity; SliderTrackSize Reserved (track uses ThumbCircleSizeHover); SliderTrack/Thumb owner-transparent; commits 5c794b1ae..e04d91ed9)
Task 10.3: minor (deferred): toggle-switch implementation.md:163,205 still mention SwitchOpacity; SliderTrackSize explicit overrides silently overwritten (Reserved).
Task 10.3: complete (commits a4942962c..e04d91ed9, review clean)
Task 10.4: dispatched implementer (opus) at base e04d91ed9
Task 10.4: implementer DONE_WITH_CONCERNS, 8 commits 03b8b8226..0b7f58b81 (StepsProgressFamilyEffectTests 217; Rendering 1414; Desktop.Controls 4163; Generator 776; family 041–048 85 → 0). 042: CircleTextColor, LineBorderRadius, ProgressStepMarginInlineEnd, ProgressStepMinWidth Active; ProgressActiveMotionDuration Reserved. Abstract token layers introduced for progress controls (breaking key moves); generator follows abstract layers.
Ruling (C4, verified in source): `TokenResourceExtension<T>.ProvideValue` returns `new DynamicResourceExtension(key)` without calling its ProvideValue, so every generated `XxxTokenResource` (and SharedTokenResource) used on a ControlTemplate node binds at LocalValue and masks `:pointerover`/`:disabled` StyleTrigger setters — the systemic cause of rule P1 defects. New Task 18 (before 10.5): return `new DynamicResourceExtension(key).ProvideValue(serviceProvider)` so template uses get Template priority like DynamicResource/OwnerTokenResource; tests; full regression of all migrated families; CHANGELOG — cost if wrong: some template-node values that relied on beating trigger styles change (that is the intended fix).
Task 10.4: review approved (0 Critical/Important).
Task 10.4: minor (deferred): generator CollectTokenLayers treats any *Token base as layer (stop at [ControlDesignToken]) and second full DescendantNodes walk without cancellation; generator test covers one layer level only; NotifyLineMetricsChanged untested; circle icon visibility uses computed size; stale semantic-part.md:526, implementation.md:50, progress-bar/steps changelog.md not updated; OutlineDotBorderThickness described as antd token (is derived from LineWidthBold); ProgressActiveMotionDuration type double/0 vs antd 2.4s TimeSpan (fix type while Reserved); CHANGELOG omits LineBorderRadius 1000→100; Gallery visual check of unmasked Steps setters pending.
Task 10.4: complete (commits e04d91ed9..0b7f58b81, review clean). Plan 0857823ee.
Task 18: dispatched implementer (opus) at base 0857823ee
Task 18: implementer DONE, commit e42b96cf5; full regression [SUMMARY] TOTAL 8266 passed (07:38). 4 previously masked state setters: 2 unmasked bug fixes in CHANGELOG (dark NavMenu group titles, LunarCalendar workday markers), 2 no-op (ShowCaseItem IsFake).
Ruling (pending review): ATOMUIGEN046 no longer detects real masking (template literals were already Template priority; token reads now too) → retire 046: stop reporting, mark deprecated in the diagnostics registry (ID never reused), remove from MigratedFamilyAnalyzerGateTests and .editorconfig sections, update guideline rule 5 — cost if wrong: loses a readability nudge only.
Note: LanguageResourceExtension has the same missing template detection (localized text in templates masks state setters) — outside the theme scope; record as follow-up for the user.
Task 18: review approved (0 Critical/Important). 046 retirement claim verified.
Task 18: minor (deferred): LanguageResourceExtension (AtomUI.Localization) has the same missing template detection — out of theme scope, surface to user as follow-up.
Task 18: complete (commits 0857823ee..e42b96cf5, review clean)
Ruling: retire ATOMUIGEN046 now as Task 18b (small): remove analyzer logic + descriptor usage, keep the ID constant marked deprecated in AtomUIDiagnosticIds (never reused), registry row → Deprecated with reason, remove from .editorconfig sections and gate test, update guideline rule 5 / theme-effect-tests references.
Task 18b: dispatched implementer (sonnet) at base e42b96cf5
Task 18b: review Needs fixes — customization.md:687 stale masking claim; fix round 1 started (resume implementer). Minor folded in: replace bare hash in retired table with behaviour name.
Task 18b: fix round 1/5 (1 addressed, 0 open — guide row + retired table wording; commits 35b2614dc..03d239452)
Task 18b: complete (commits e42b96cf5..03d239452, review clean)
Task 10.5: dispatched implementer (opus) at base 03d239452
Controller: duplicate Task 18 hand-back received (already processed). Carry to 10.7: effect test for dark NavMenu group title DarkGroupTitleColor; carry to 10.9: effect test for LunarCalendar WorkdayMarkerColor (both unmasked by Task 18).
Task 10.5: implementer DONE_WITH_CONCERNS, commits aef0d8ff5 0f6416d07 ecd2a01f8 aa0445c39 17c3cdec4 (family 110; Rendering 1524; Core 411; Generator 770; family 041 36→0, 042 1→0, 044 14→0, 047 2→0). 042: Tag.TagCloseIconSize Active. New AbstractTagToken + CheckableTagToken (Breaking). E1 badge palette from snapshot; helper src/AtomUI.Controls/Primitives/PresetColorPalette.cs for 10.6.
Ruling: Tag custom-colour light background from palette step 1 instead of antd's fixed HSL 0.95 is accepted (plan text "改用现有调色板算法"; works in dark mode); record as known antd difference in tag token.md — reviewer to check it is recorded.
Carry to 10.10: FloatButton badge colour does not recompute on theme change.
Task 10.5: review Needs fixes — I1 badge preset colours stale after re-attach (no recompute on attach); I2 FloatButton preset BadgeColor set before attach becomes null → error colour (regression introduced here, Gallery FloatButtonShowCase:441); I3 Tag custom-colour antd deviation not recorded.
Ruling (I2): fix FloatButton fully now — resolve badge colour on attach AND subscribe to theme changes via the same PresetColorPalette (this closes the 10.10 FloatButton item too); test set-before-attach and dark switch.
Task 10.5: fix round 1 started (resume implementer).
Task 10.5: minor (deferred): ResolveCustom regenerates palette per update (no cache); badge token.md says ToolTip already reuses helper; App_Style_Overrides theory only Outlined; CHANGELOG lacks FloatButton palette note; TryParse accepts grey; no test for 9-component FormatMatrix.
Task 10.5: fix round 1/5 (3 addressed, 0 open — badge re-attach recompute; FloatButton attach + theme recompute; antd deviation docs; commits 17c3cdec4..a9b5e1507). FloatButton 10.10 item closed by this.
Task 10.5: complete (commits 03d239452..a9b5e1507, review clean)
Task 10.6: dispatched implementer (opus) at base a9b5e1507
Task 10.6: implementer DONE_WITH_CONCERNS, 9 commits ab076d461..55e47ced8 (FeedbackFamilyEffectTests 228; Rendering 1750; Desktop.Controls 4200; Generator 792; family 041 106→0, 042 6→0, 045 13→0, 047 5→0). 042: Alert.CloseIconSize Active, ToolTip.MotionDuration Active; deleted Alert.MessageWithDescriptionMargin, MessageCard.CardHeight, PopupConfirm.PopupMinWidth/MinHeight, NotificationCard.NotificationCloseButtonPadding (Breaking).
Ruling: MessageBox reading Dialog Own tokens → 10.10 abstract-layer item. Skeleton cutting the owner chain for dialog content → decide in 10.9 (Skeleton owner-transparent only if its own visuals read SkeletonTokenResource explicitly). DialogWindow owner = Window via StyleKey → nothing visible; note in 10.10. Popup open motion reads PopupTokenResource → per ruling. PrimeNG Dialog radius via Controls[Dialog].BorderRadiusLG → carry to Task 11 brief.
Task 10.6: review approved (0 Critical/Important).
Task 10.6: minor (deferred): ToolTip preset fallback drops preset when no context (upstream used presetColor.Color()); ToolTip equality guard compares against current ColorBackground; PopupMotionOriginTests InlineData(4,12) non-discriminating; notification/semantic-part.md still says SharedToken in 8 rows; stale test comment Cards.cs:138; borrowed-fix commit 483c961e6 also carries migration; static Dialog/Notification root-context behaviour untested.
Task 10.6: complete (commits a9b5e1507..55e47ced8, review clean). Plan a88e509a9.
Task 10.7: dispatched implementer (opus) at base a88e509a9
Task 10.7: implementer DONE_WITH_CONCERNS, 9 commits 35c42b0c9..137566507 (NavigationFamilyEffectTests 394; Rendering 2144; Desktop.Controls 4198; Generator 834; family 041 181, 042 34, 043 2, 045 9 → 0). 042: NavMenu 16 Active / 3 Reserved / 2 deleted; Menu 7 Active / 1 deleted (+ new GroupTitlePadding); TabControl.HorizontalMargin Reserved; Pagination 2 Active / 2 Reserved. New public API NavMenu item IsDanger (antd Menu item `danger`).
Ruling: ContextMenu/MenuFlyoutPresenter→Menu, card/strip tab hosts→TabControl, SimplePagination→Pagination Own-token borrowing join the 10.10 abstract-layer item. New IsDanger API accepted (antd parity) — must be in CHANGELOG Added and control docs, reviewer to check.
Task 10.7: review Needs fixes — I1 IsDanger docs missing in nav-menu overview/implementation; CHANGELOG names internal NavMenuItem, omits public BaseNavMenuItemHeader.IsDanger. I2 light horizontal top-level radius 8→0 (antd-aligned) unrecorded and CHANGELOG/nav-menu changelog claim unchanged.
Ruling (Minor 3 promoted into this round): MenuEdgeThickness / ControlHeight rows in Tabs pass only via a side effect (perturb to 8 erases the shadow) — per the design-meaning precedent, either prove the token's meaning in a realistic arrange or mark MenuEdgeThickness Reserved; for the ControlHeight global row, keep it only if a realistic arrange shows a meaningful effect, otherwise drop the row with a NotRendered reason.
Ruling (concern 2): tab items' TokenTransitions following the host while their other globals follow Controls[TabItem] — documented, low value; deferred to 10.10 consistency sweep.
Task 10.7: fix round 1 started (resume implementer).
Task 10.7: minor (deferred): NavMenuItemTheme.axaml IsDanger glued on one line; small simple-pager input 50→44 untested; Tabs.cs:257-260 duplicate case bodies; Isolation.cs:159 name vs coverage (MenuFlyoutPresenter); IsDanger mapping copied into 5 sites; Tour detached guard untested; TabStripItem scope rows not shown failing-before; HorizontalLineHeight=46 unverified; selected danger item in light horizontal mode untested.
Task 10.7: fix round 1/5 (3 addressed, 0 open — IsDanger docs; light radius not visible (evidence verified) + docs/test; MenuEdgeThickness Reserved; commits 137566507..22310e089)
Task 10.7: minor (deferred): CHANGELOG could flag 8→0 radius for apps that style light top-level backgrounds; no Gallery danger showcase / no Gallery visual walk-through.
Task 10.7: complete (commits a88e509a9..22310e089, review clean)
Task 10.8: dispatched implementer (opus) at base 22310e089
Task 10.8: implementer DONE_WITH_CONCERNS, commits 253a5d5cd 820c3fc17 0e6a86a4a (LayoutFamilyEffectTests 233; Rendering 2376; Desktop.Controls 4198; gate 182; family 041 124, 042 11, 045 16 → 0). 042: Card.ExtraColor, Separator.HorizontalWithTextGutterMargin Active; Card ActionsSpacing/TabsMarginBottom/CardHeadPadding/CardPaddingBase Reserved; Expander 2 VMargin + GroupBox 3 deleted (Breaking); new Separator.TitleFontWeight.
Ruling (pending fix round): Card's resting shadow is a transparent literal, so presets (PrimeNG Card resting shadow, Material elevation) cannot express it; antd Card has no resting shadow token. Add a Card Own token for the resting shadow (e.g. `RestingShadows`, default empty BoxShadows = current rendering), Active with an effect test, documented as an AtomUI extension for presets — cost: one non-antd token.
Carry: CompactSpaceAddOn borrows Space Own tokens → 10.10; Skeleton owner chain → 10.9.
Task 10.8: review Needs fixes — I1 Separator.HorizontalWithTextGutterMargin on Middle; antd: md/sm size margins win, unsized(large) with text uses gutter.
Ruling (I1): follow antd — move `:has-title` gutter to `[SizeType=Large]` (= antd unsized default by value), record large-titled 24→16 visible change, correct comment/token.md/CHANGELOG; effect row uses a Large titled arrange.
Ruling (resting shadow, from earlier): add Card Own token for the resting shadow replacing the literal at CardTheme.axaml:85; default 3 transparent layers (matches CardShadows layer count for BoxShadowsTransition); Borderless still overrides with BoxShadowsTertiary; hover still CardShadows; effect test; documented as AtomUI extension.
Ruling: Separator.TitleFontWeight (AtomUI-only, default 500 = antd fixed) accepted as an AtomUI extension; document it as such in token.md.
Task 10.8: fix round 1 started (resume implementer).
Task 10.8: minor (deferred): CardPaddingBase could be Active on Large grid items; ghost panels read global ColorTransparent; isolation gate omits Splitter/Masonry; borderless Collapse branch has no row; CardTabsContent padding reads only via derivation; user docs cite plan task numbers and a foreign SHA (card token.md:128, space token.md:36, splitter changelog.md:9) → strip; layout comments followed by blank line.
Carry to Task 15: strip internal plan references (Task 10.x) from control docs in menu, pagination, tab-control, modal, alert, message, notification, popup-confirm token.md (and any others — grep 'Task 1[0-9]').
Task 10.8: fix round 1/5 (3 addressed, 0 open — separator gutter on Large; Card RestingShadows; docs plan refs stripped; commits 0e6a86a4a..731518712)
Task 10.8: minor (deferred): RestingShadows layer count vs hand-set CardShadows not documented; card token.md names PrimeNG/Material presets as examples.
Task 10.8: complete (commits 22310e089..731518712, review clean)
Ruling: Task 10.9 (Data) is too large for one pass → split into 10.9a (List/ListBox/ListView, Tree/TreeView, Avatar, Timeline, Skeleton, Statistic, Carousel, ImagePreviewer) and 10.9b (Calendar/LunarCalendar, DatePicker/TimePicker, basic Table) — cost: one extra review cycle.
Task 10.9a: dispatched implementer (opus)
Task 10.9a: implementer DONE_WITH_CONCERNS, 6 commits 4b9b93368..a9c59f935 (DataFamilyEffectTests 270; Rendering 2647; Desktop.Controls 4198; Generator 897; family 041 152, 042 11, 045 11, 047 3 → 0). 042: ListBox/ListView ItemDisabledColor, Carousel ArrowOffset, Skeleton GradientToColor/TitleHeight Active; TreeView DirectoryNodeSelectedBg/Color, ImagePreviewer ImagePreviewSwitchSize Reserved; Timeline LastItemContentMinHeight deleted; Avatar Group* moved to new AvatarGroupToken; new AtomUI extensions Skeleton ImageColor, Carousel ArrowOpacity, ImagePreviewer ErrorIconSize. Skeleton + CardGridContent owner-transparent.
Carry to 10.10: CandidateList, Transfer, TreeSelect, TimerStatistic, ImageGroupPreviewer borrow other controls' Own tokens; TreeSelect tree owner = TreeView via StyleKey; CandidateListItem/Transfer item containers not owner-transparent; Avatar TextFontSize* is antd iconFontSize* (documented mismatch).
Task 10.9a: review Needs fixes — I1 vacuous ImagePreviewer ColorBorderSecondary guard. Ruling: also promote Minor 2 (TimerStatistic rows with a live seconds clock can pass vacuously) and Minor 3 (gate "Active > 0" guard combined across packages) into this round — same vacuous-guard class, cheap.
Task 10.9a: fix round 1 started (resume implementer).
Task 10.9a: minor (deferred): disabled-switcher wording ("antd colours the whole node") in tree-view token.md:40 and theme comment; ImagePreviewerLoadingVisualTests no longer pins 48 px; weak Skeleton/List motion/Statistic owner facts; WaitForDialog reflects private _openState; SplitLineWidth/IconSize duplication in ListBoxItem/ListViewItem/TreeViewItemHeader/NodeSwitcherButton.
Task 10.9a: fix round 1/5 (3 addressed, 0 open — ImagePreviewer guard equality; TimerStatistic static formatter; per-package gate guards; commits a9c59f935..eb8d42c85). 270→269 was a miscount in the report.
Task 10.9a: complete (commits 303c6740f..eb8d42c85, review clean)
Task 10.9b: dispatched implementer (opus)
Task 10.9b: implementer DONE_WITH_CONCERNS, commits 2537e72cb de8fb1004 56755dfc4 (Data effect 619; Rendering 2997; Desktop.Controls 4198; Generator 921; family 041 105, 042 4, 044 1, 045 14, 047 5 → 0). Scope added DateViewer + Primitives/InfoPickerInput; no basic Table exists. 042: TimePicker ButtonsMargin/PeriodHostWidth Active; RangePickerArrowMargin/RangePickerIndicatorThickness deleted (Breaking). New AtomUI-extension tokens: Calendar.FullCell*, RangeBarGap, RangeBarLabelPadding; LunarCalendar.Marker*; DateViewer Header/FooterBorderThickness, WeekNumberOpacity; DatePicker.HeaderBorderThickness; TimePicker.ItemMargin.
Task 10.9b: REVIEW PENDING (not yet dispatched at handoff). Reviewer must judge: (1) new `Primitives/TokenOwnerForwarding` mechanism (picker→popup presenter inside registered ArrowDecoratedBox; TimeView→columns inside registered ScrollViewers; Calendar→DateViewer) — document as pattern vs alternative; (2) visible change: picker input text ColorTextTertiary while popup open — antd does not dim it → likely remove unless antd evidence; (3) borrowed Own tokens LunarCalendar→Calendar, RangeDateViewer→DateViewer, RangeDatePicker→DatePicker, date-time TimeView→TimePicker, range pickers→InfoPickerInput → 10.10; (4) pre-existing clipped picker focus shadow (range picker shadow rows excluded) → decide fix vs defer.

## 2026-11-10 接续核对

Task 10.9b: 本轮评审已派发（实现范围 `eb8d42c85..21628b4fa`，最后代码提交 `3e55b3883`）；后续修复已覆盖弹层日期面板 owner、输入预览色、焦点阴影与范围条释放。先前交接只记录至 `56755dfc4`，不作为最新状态。
Task 10.10a: 接手时保留 37 个已修改源文件与 7 个未跟踪 DataEntryFamilyEffectTests 文件；`AbstractGeneralProgressBar.IndicatorThickness` 重算属于 Upload 的前置修复。尚无完成或评审结论。
Ruling: 10.9b 本轮评审覆盖全部已提交修复 `eb8d42c85..21628b4fa`，10.10a 未提交实现独立接续 — Git 历史证实原交接后的两轮代码与当前录入类修改分属不同任务 — 错了的代价：若归属误判，需要重新划分评审或提交范围，不丢弃现有改动。
Task 10.9b: 本轮评审 Needs fixes，0 Critical / 2 Important / 2 Minor。I1：Picker 的 BorderRadiusSM 生效行被全局过滤；I2：RangeDateViewer / RangeTimePicker 缺少完整控件覆盖、无关兄弟与根覆盖隔离。新实现者修复第 1 轮，并补后续代码提交的 focused 验证凭证。
Task 10.9b: minor (deferred): 动效测试对空 Transitions 集合可能 vacuous；Calendar/TimePicker 用户文档内部任务引用、DateViewer 的 SharedToken 术语不清。归终审与 Task 15 文档整理。
Task 10.9b: fix round 1/5（2 addressed，0 open；`21628b4fa..485ef3ba7`）：恢复四类 Picker 的 BorderRadiusSM 生效行，增加不可空覆盖守卫；新增 RangeDateViewer / RangeTimePicker 的正向、无关兄弟和根覆盖隔离。有效 RED 1 passed / 5 failed，GREEN 10/10；清理后定向 Rendering 410/410、Controls 360/360、Generator gate 250/250，0 failed / skipped。只提交两个测试文件；接手时 45 个未提交路径逐字节保持不变。
Task 10.9b: 范围复审通过（I1/I2 ADDRESSED，0 新 Critical/Important/Minor）；未重复运行相同测试。历史原始 RED/原始报告未恢复，不冒充本轮验证记录。新门禁属于计划批准的常驻生效/契约测试，未留临时测试代码；原有 xUnit1051 和可选 DataGrid 警告留相应任务与收尾处置。
Task 10.9b: complete（commits `eb8d42c85..485ef3ba7`，review clean；两个 deferred minor 见前条记录）。计划中的 TimePicker 裁决已核准，存量基线补家族验收增量。
Task 10.10a: 实现者已恢复接手改动并开始定向运行；OTP 实际使用独立 OtpTextBox 模板，Mentions 使用 MentionTextArea/TextArea，初始“缺 s_textBox”判断经树拓扑核对纠正。门禁、文档与真实弹层消费仍需完成。
Ruling: OTP 与 Mentions 按各自真实模板及编辑器消费者建立生效行，仅共享 TextBoxTheme 的 EmbeddedTextBox 子类承载 s_textBox 清单 — 真实类型分别为 OtpTextBox : Avalonia TextBox 与 MentionTextArea : TextArea，机械追加未消费主题的行会制造假承诺 — 错了的代价：若拓扑识别遗漏，将漏掉实际编辑器消费者，需以模板命中和 owner 隔离断言纠正。
Task 10.10a: implementer DONE，commit `4bf2f1032`（66 个显式归属文件）；240/240 生效与隔离、205/205 现有定向、287/287 门禁通过，现正独立评审。52 Own 全部 Active，Form.VerticalLabelPadding 已生效，无 Reserved/删除；041 221、042 1、045 6、047 1 → 对应家族全部 0。
Task 10.10a: carry to 10.10d：CandidateList/CandidateListItem 的 ListBox Own 借用；核对 SubmitButton/ResetButton 的注册 identity 后再判断 Button Own 读取是否为独立覆盖泄漏，不仅凭继承/文本读取新增 Token 身份。
Task 10.10a: minor (deferred): 只读成员布局审计发现 16 个存量问题（AbstractGeneralProgressBar 4、FormItem 10、FormItemDecorator 1、CandidateList 1），本轮未新增该类问题，终审按价值分拣；047 仍可能漏检变量目标 SetValue，OTP 的实际 Width 路径已改 AXAML，分析器泛化加固留终审分拣。
Task 10.10a: review PASS（0 Critical / 0 Important / 1 Minor）。独立核对最终 TRX 的 240 个用例与 52 个 Own 行、原源码 RED 31/140、固定因果 RED 18/20；205 / 287 定向输出已审阅，未重复跑测试。
Task 10.10a: minor (deferred): `DataEntryFamilyEffectTests.MapRows` 捕获任意 ArgumentException 后返回空集，未来 OTP 消费图丢失可能保留基元行却漏掉 CellWidth/CellGap 等 Own 行；当前 52 Own 均存在。终审分拣必需成员 fail-fast 或独立 Own 集守卫。
Task 10.10a: complete（commits `67349bc02..4bf2f1032`，review clean；minor 留台账）。计划补 Form.VerticalLabelPadding 裁决与家族基线。10.10d 另须核对有独立身份却沿 ButtonTheme 读取 Button globals/Own 的 SubmitButton/ResetButton，及 MentionTextArea/TextArea 的真实 StyleKey owner 边界。
Task 10.10b: 起点 `9b01a7fd2` 干净工作树。初始定向 RED 55 项：53 passed / 2 failed / 0 skipped，失败为 Empty.ColorTextDescription 与 QRCode.ColorSplit 无像素效果。已枚举 68 处 Shared 读取；SpinIndicator 及 Transfer 条目容器 owner 透明属于本家族的宿主全局/动效路由，独立公开控件 Own 抽象仍留 10.10d。
Ruling: TreeTransfer.ListWidthLG 标为 Reserved 并保留字段和默认值，不新增分页 API 来制造生效 — 唯一消费依赖 IsPaginationEnabled=True，但 TreeTransfer 没有分页 API 且视图 IsSupportPagination=false，实际生效用例为零差异 — 错了的代价：暂留一个不可定制的兼容字段；若发现受支持的真实渲染状态，需要恢复 Active 与相应测试。
Ruling: Transfer 装饰器的宿主布局与搜索装饰量先在无身份基元根解析 OwnerTokenResource，再由 AXAML 模板绑定投射到内部 LineEdit — 直接在已注册 LineEdit 上读取 OwnerTokenResource 会得到 LineEdit 自身而非 Transfer，真实 MarginXS/装饰色用例已失败；编辑器自身身份仍独立 — 错了的代价：若某个量实际应由编辑器负责，需调整该量的归属并更新对应隔离断言。
Ruling: Drawer 在独立 overlay 资源边界复用现有 ThemeContextResourceBridge，随打开建立、移出层释放 — 同一真实打开场景中控件访问器为局部 Red 而 Frame.Background 为 White，单变量桥接 A/B 后背景与资源查询均随局部发布变 Red，证明缺口在资源链而非 Token 计算 — 错了的代价：若边界选择或释放错误，会引入重复资源宿主、失效通知或作用域保留，需用发布、关闭重开及嵌套身份隔离契约纠正。
Ruling: Transfer 保留继承 ListView 分组头的 FontSize/FontSizeSM 生效行，以公开 SourceView + IsGroupEnabled + ListItemData.Group 安排真实分组 — 最初直接设置 GroupListItemData.IsGroupItem 的探针被视图节点同步覆盖，不能证明分组不受支持；ListTransfer.cs 14–43 保留公开 ITransferView 实例，已有生产兼容分组路径已让两条字体行生效 — 错了的代价：若公开聚合场景安排仍不等价，需修正真实 Transfer 场景的测试，不得以错误探针删去消费者。
Ruling: QRCode 显式 Color=null 时以 ControlTokenAccessor 的有效 ColorText 回落，并在该空值路径随主题变化重建、平衡挂载订阅 — AbstractQRCode 写死 Colors.Black 绕过控件有效主题，因果用例在 Controls[QRCode].ColorText 改变后为 0/25600 像素差异，正常非空 Color 应继续有最高优先级 — 错了的代价：依赖显式 null 永远得到不透明黑色的应用将改为主题文本色/透明度；文档需明确此变化，错误生命周期会导致重复位图工作或保留。
Task 10.10b: implementer DONE `7dcd4cdee`（59 文件），213 生效/隔离、136 现有定向、310 门禁及最终 1+5 覆盖检查通过；58 Active / 1 Reserved。独立评审发现 HyperLinkTextBlock 新默认白底盖住 Gallery DataGrid 的悬停/选中行背景，待完整结论后由原实现者修复。
Ruling: HyperLinkTextBlock 保持透明默认背景，同时保留显式用户 Background 到现有 PART_Text 的模板投射 — 新增 ColorBgContainer 默认绘制会在真实 Gallery DataGrid 单元格上形成不透明矩形，原场景与本地 antd typography reset 均要求透明背景，不能仅凭记录可见变化就接受回归 — 错了的代价：依赖本次未发布迁移中新默认白底的用法需显式设置 Background；如透明安排不等价，需修正真实行状态场景而非删除测试。
Task 10.10b: review Changes requested：1 Important/P2（HyperLink 默认白底覆盖真实 DataGrid 行状态），1 Minor/P3。原实现者续修第 1 轮，保留透明默认与显式背景投射；其余资源桥接、QRCode 生命周期、覆盖和证据检查通过。
Task 10.10b: minor (deferred): TransferItemDecorator 新增三个 internal 投射属性置于公共契约前，成员组织待终审分拣，不进入本轮功能修复。
Task 10.10b: fix round 1/5（1 addressed，0 open；`7dcd4cdee..6160a423b`）：仅移除 HyperLink 默认 ColorBgContainer Setter，保留显式 Background 到 PART_Text 的投射。真实 Gallery BasicCaseGrid 原 5 列/4 链接/15 间距/1100×720，16 个链接×行状态严格比较默认与显式透明像素一致，并证明显式 Lime 生效；最终契约对坏 Setter 单变量回退为 RED 1/1，恢复后覆盖 7/7、门禁 310/310 通过。
Task 10.10b: 范围复审 PASS，P2 已解决，无新增阻塞；P3 留台账。原主验收 213 / 136 记录复用，不冒充修改后重跑；最终公开消费图 HyperLink 6→5 行（移除过时背景消费），58 Own Active / 1 Reserved 不变，无临时源/探针残留。
Task 10.10b: complete（commits `9b01a7fd2..6160a423b`，review clean；1 deferred minor）。计划基线与追加 TreeTransfer 裁决单独登记；Transfer Own 借用继续由 10.10d 负责。
Task 10.10c: 起点 `1d7d8c54c` 干净。Icon.FallbackColor 对应实际 FallbackBrush 路径但当前硬编码 White；WindowTitleBar 五个旧字段与现有 Active*/WindowsClose* 消费路径重叠且无独立语义；Splash 受链接逻辑路径过滤而未进入资产分析。
Ruling: Icon.FallbackColor 接入真实 FallbackBrush 消费并随有效主题更新，保留正常与显式用户绘制优先级 — 当前回退画刷硬编码 White，已有 Own 字段的意义就是该真实回退视觉，不能以空消费标记规避 — 错了的代价：若回退路径或优先级识别不当，会误改正常图标或用户颜色，需由真实回退画刷指令与显式颜色的因果/隔离测试纠正。
Ruling: WindowTitleBar 五个旧 Own 字段经直接、派生、跨包消费者核查为死字段后删除，使用现有 Active*/WindowsClose* 平台状态路径写迁移说明 — 无独立消费者或 antd 同控件保留理由，不为旧字段新增竞争 Setter；平台/状态并非一一对应时不伪造替代关系 — 错了的代价：引用旧 XML 名称或生成 TokenKind 的外部应用需迁移；若误删实际有效字段，必须恢复其契约与消费者。
Ruling: Splash 的物理主题源文件进入现有资产分析，逻辑资源键与启动注册行为保持原契约 — 现有链接逻辑路径过滤漏掉主题内 12 个 Shared 读取，修正分析输入而非新建运行时扫描机制 — 错了的代价：若物理/逻辑路径映射或去重错误，会重复诊断或误改主题注册，需链接资产的精确生成器契约纠正。
Ruling: 生效测试 harness 按描述符的已注册类型使用现有泛型 Capture<T>，支持真实派生 Icon — 非泛型访问器要求精确运行时注册，CheckOutlined 等实际派生图标会在读取前失败，不应为测试另造注册近似控件 — 错了的代价：若注册类型选择错误，会测到错误身份的值，需以真实派生图标与原控件身份契约纠正；产品注册行为不变。
Ruling: 真实原生专用 Window Token 保持 Active，以实际消费终点属性或既有原生适配调用验证绑定/局部发布/恢复生命周期，原生绘制未覆盖单列记账 — 当前 headless 不绘制 SystemBarColor、Win10 专用边框与部分原生全屏表面，不能伪造像素通过或因本机不可见把真实平台 Token 降为 Reserved，也不增加未经授权的全平台矩阵 — 错了的代价：即使绑定终点正确，原生 API/绘制行为仍可能有未验证问题；当前平台可见窗口/全屏项交最终视觉核对，未覆盖项不冒充已验证。
Ruling: TextSelectionHandle 仅补 Avalonia 12 必需的透明 Border PART_Indicator，并将原有选择器对齐 :caret/:start/:end，保留 32px 可见形状与位置 — 实际 OnApplyTemplate 因缺 PART_Indicator 抛 KeyNotFoundException，当前 PathIcon-only 模板无法渲染，此处是既有框架契约修复而非模板重构 — 错了的代价：新增必需部件可能改变指针/触摸热点或形状，必须以真实模板、指示部件边界与 owner 颜色契约纠正；原生触摸拖动未验证需单列。
Ruling: WaveSpirit 若现有测试设施无可用确定性时钟，先验证真实 painter 画刷、实际生成的关键帧/范围/透明度/时长及播放取消释放，瞬态像素交当前平台真实视觉核对 — Avalonia 12 时钟不可直接访问，不应为测试另建时钟框架或伪造 Token 派生绘制副作用，现有动画/帧设施须先做一次核查 — 错了的代价：生成与消费终点正确仍可能隐藏实际动画绘制问题，因此不宣称未运行的冻结像素验收，最终真实波纹视觉项保持显式未完成。
Task 10.10c: 完整矩阵 133/133、生成器门禁 347/347 已通过；Indicator 保留原 2px/8px 裁剪几何，真实指针保持不变，非对称圆角扰动可见，移除发布后额外交互仍通过，未改产品 Indicator。Embedding 的 SystemBarColor 具体画刷终点另经 4/4 验证。
Ruling: Embedding 通过既有 EmbeddingThemeConstants 的最小画刷转换器声明式适配严格 SolidColorBrush 终点，不放宽公开属性类型 — 真实 EmbeddableControlRoot 初始化正常，SystemBarColor 为 null 的原因是不可变画刷投射被具体类型拒绝，不能用假 Theme 探针或错误初始化模型掩盖 — 错了的代价：有效输入变化时可能多创建一画刷，若颜色/透明度转换错误会影响原生终点；需真实根、类型、发布/恢复契约纠正，资源查询热路径不新增此转换。
Task 10.10c: implementer DONE，`f071b5978`（58 路径，含误暂存的控制者进度记录），最终 665 项通过：渲染/骨架 169、现有模块 145、生成器 348、启动 3，0 failed/skip；独立评审已派发。65 Own Active，Icon.FallbackColor 生效，WindowTitleBar 五旧字段删除，Native/瞬态/触摸覆盖限制明确保留。
Task 10.10c: minor (deferred): 只读成员布局审计 67 个存量问题（ArrowIndicator 8、FeedbackStackPanel 19、Window 40），无新增方法/大括号问题，终审按价值分拣，不在本轮结构扫除。
Ruling: 保留 `f071b5978` 的现有提交历史，登记控制者进度记录误随源码暂存，计划裁决/基线仍由控制者另提交 — 已核对混入内容均为控制者本轮原有记录，计划文件未混入；用户禁止未经明确指令 amend/rebase，不能为提交粒度重写历史 — 错了的代价：此提交混有过程文档、粒度不理想；若将来需要拆分，须取得明确改写授权，当前内容与审查范围可追溯。
Ruling: Flyout 的 object 资源键契约保持兼容，仅已知 FlyoutHost Own 枚举归一化为对应 Own 键，SharedTokenKind、自定义枚举及已类型化键按原语义在 placement-target 作用域解析 — 作用域迁移不能把任意 Enum 重新解释为 FlyoutHost Own，当前审查已发现错误包装会让全局/自定义键触发不属于 FlyoutHost 的异常 — 错了的代价：若键分类或作用域回退处理错误，外部派生 Flyout 的偏移绑定仍可能失效，需全局/自定义/Own 键的真实打开与发布契约纠正。
Task 10.10c: review Needs fixes（0 Critical / 2 Important/P2 / 0 新 Minor）：I1 通用枚举键被误包为 FlyoutHost Own，真实 Shared.SizeUnit 打开异常；I2 同一 MenuFlyout 活跃切换两个 provider 目标，偏移 16→0，手动 Hide 后重开才 32。原实现者续修第 1 轮；其余 665 凭证与原生/瞬态边界已审阅，不重复整组测试。
Ruling: Flyout 已打开时切换 placement target 必须释放旧会话并在正确开闭阶段建立新目标作用域绑定，不能以旧排序问题豁免本轮承诺 — 实际两个 provider 为 16/32，同一 MenuFlyout ShowAt(B) 得到 0，而 Hide 后重开 B 得到 32，已排除 Token 计算和首次查找，缺口在开闭会话顺序 — 错了的代价：调整阶段可能影响开闭取消、同目标重开或资源释放，须用真实复用、B 发布、A 隔离、关闭重开与已有生命周期契约纠正。
Task 10.10c: fix round 1/5（2 addressed，0 open；`f071b5978..0070c62e9`）：仅已知 FlyoutHostTokenKind 归一化 Own，6 种键保持作用域语义；不同目标先以既有非可取消 HideCore 释放旧会话再获取新会话。真实 fixture RED 3 failed / 7 passed → GREEN 10/10；最终 lifecycle/Pinned 19/19、实际 Flyout/TreeFlyout 21/21，产品构建 0 warnings/errors；源与产物哈希匹配，未重跑不受影响 665 项。
Task 10.10c: 范围复审 PASS（I1/I2 ADDRESSED，无新增问题，未重复跑测试）；修复 6 路径明确排除控制者文档。主实现误带控制者记录已如实更正，不改历史；65 Active / 5 删除的裁决和基线由控制者另记。
Task 10.10c: complete（commits `1d7d8c54c..0070c62e9`，review clean；67 存量布局及原生/瞬态/触摸覆盖限制保留）。当前平台可执行 Window/fullscreen/Wave 视觉项交 Task 15；无 Win10/原生嵌入宿主/触摸硬件的绘制不作通过声明、不新增全平台门槛。10.10d 继续核对 Scroll/Caption 部件的明确宿主 Own 读取及 TreeFlyoutPresenter→Popup，真实宿主部件不机械复制独立身份。
