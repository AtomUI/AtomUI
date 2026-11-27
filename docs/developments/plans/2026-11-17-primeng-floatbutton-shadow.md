# PrimeNG FloatButton 阴影对齐官方 实施计划

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** PrimeNG 设计语言（Aura/Lara/Nora × 亮/暗）下 FloatButton 族（FloatButton、BackTopFloatButton、FloatButtonGroup、FloatButtonGroupHost）不再绘制阴影，与官方 `@primeuix/themes` 3.1.1 的按钮族（button 所有 severity、speeddial）无 resting 阴影一致；Ant Design 基线渲染不变。

**Architecture:** 阴影从「AXAML 直绑全局 `BoxShadowsSecondary`」改为「FloatButtonToken 自有槽位 `FloatButtonShadows`」：Ant 基线默认值仍取全局 `BoxShadowsSecondary`（渲染不变），PrimeNG 包新增 `IControlTokenAlgorithm<FloatButtonToken>` 覆盖为空。绑定位置保持在 ControlTheme（符合主题绑定优先约束）。

**Tech Stack:** Avalonia BoxShadows、AtomUI 设计 token 系统（[ControlDesignToken] 源生成槽位）、PrimeNG 预设 token 算法（[ControlTokenAlgorithm]）。

**Spec:** 官方证据 = `@primeuix/themes` 3.1.1 npm 发布包（dist/aura|lara|nora）：`button/index.mjs` 全部 severity 无 resting shadow（仅 `raisedShadow` 供 raised 变体），`speeddial/index.mjs` 仅 gap/transitionDuration，无 shadow token；阴影仅存在于 overlay.{select,popover,modal,navigation}、card、message、toast。背景：用户 2026-11-17 反馈 PrimeNG Aura 下 FloatButton 阴影与官方不符（截图红框标注 Gallery FloatButton 页 Basic 示例 FAB）。

## Global Constraints

- 官方事实（3.1.1 发布包逐字）：`overlay.select/popover/navigation.shadow = "0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -2px rgba(0,0,0,0.1)"`、`overlay.modal.shadow = "0 20px 25px -5px rgba(0,0,0,0.1), 0 8px 10px -6px rgba(0,0,0,0.1)"`、`card.shadow = "0 1px 3px 0 rgba(0,0,0,0.1), 0 1px 2px -1px rgba(0,0,0,0.1)"`——这些仓库已对齐，本任务不得改动。
- Ant Design（默认主题 DaybreakBlue）下 FloatButton 族阴影渲染必须与改动前逐像素一致（值 = 全局 BoxShadowsSecondary）。
- 阴影绑定必须留在 AXAML ControlTheme 的 Setter 中；只更换 token 源，禁止移到 code-behind。
- 新测试一律临时：放 `tests/AtomUI.Themes.Tests/Temporary/PrimengFloatButtonShadow/`，文件头注明「临时验证，修复完成后删除」；验证后删除，全局测试净增为 0。
- 未经用户明确指令不得 git commit；提交信息只备草案。
- 工作位置：worktree `/Users/chinboy/Projects/dotnet/AtomUIV6-fix-primeng-floatbutton-shadow`，分支 `fix/primeng-floatbutton-shadow`（用户 2026-11-17 对话授权）。

## Review Focus

1. **Ant 基线回归**（FloatButton 阴影意外消失/变值）→ Task 1 测试锁定默认主题下 `FloatButtonShadows == 全局 BoxShadowsSecondary`。
2. **PrimeNG 算法注册遗漏**（六个 `[ControlTokenAlgorithm]` 少一个，某 profile 仍带阴影）→ Task 2 测试参数化覆盖全部 12 个 profile。
3. **族内漏改**（Group/Host 方形容器仍绑旧全局槽位）→ Task 2 源码检查四个 AXAML 文件；真机验收步骤覆盖 Group 示例。
4. **生成器槽位缺失**（新属性未生成枚举键，XAML 编译失败）→ Task 2 构建通过即证。
5. **BackTop 同病未修**（与 FloatButton 共用 AbstractFloatButtonTheme，应同修）→ 主题层改动天然覆盖；验收步骤含 BackTop。

---

### Task 1: FloatButtonToken 新增 FloatButtonShadows 自有槽位（Ant 基线不变）

**Files:**
- Modify: `src/AtomUI.Desktop.Controls/FloatButton/FloatButtonToken.cs`
- Test: `tests/AtomUI.Themes.Tests/Temporary/PrimengFloatButtonShadow/PrimeNGFloatButtonShadowTests.cs`（新建，临时）

**Interfaces:**
- Produces: `FloatButtonToken.FloatButtonShadows`（`partial BoxShadows`，源生成器产出 `FloatButtonTokenKind.FloatButtonShadows` 自有槽位）；`CalculateTokenValues` 中 `FloatButtonShadows = EffectiveGlobalToken.BoxShadowsSecondary;`

- [ ] **Step 1: 写失败测试**

临时测试文件（xUnit，Avalonia.Headless），应用默认主题 DaybreakBlue 后断言：
`Own<BoxShadows>(snapshot, "FloatButton", "FloatButtonShadows")` 等于 `Global<BoxShadows>(snapshot, "BoxShadowsSecondary")`（复用 `PrimeNGThemePackageTests` 的 `Own/Global` 快照读取方式）。

- [ ] **Step 2: 运行确认失败**

Run: `dotnet test tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj --framework net10.0 --filter PrimeNGFloatButtonShadow`
Expected: 编译失败（`FloatButtonShadows` 不存在）。

- [ ] **Step 3: 实现属性**

`FloatButtonToken.cs` 增加 `public partial BoxShadows FloatButtonShadows { get; set; }`（双语注释，随文件既有风格）与 `CalculateTokenValues` 赋值行。

- [ ] **Step 4: 运行确认通过**

同 Step 2 命令，Expected: PASS。

### Task 2: AXAML 改绑 + PrimeNG 无阴影覆盖

**Files:**
- Modify: `src/AtomUI.Desktop.Controls/FloatButton/Themes/AbstractFloatButtonTheme.axaml:52-54,80-83`
- Modify: `src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonGroupTheme.axaml:49-51`
- Modify: `src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonGroupHostTheme.axaml:15-17`
- Create: `src/AtomUI.Themes.PrimeNG/Algorithms/Controls/PrimeNGFloatButtonTokens.cs`
- Test: 同 Task 1 临时文件（扩展）

**Interfaces:**
- Consumes: Task 1 的 `FloatButtonShadows` 槽位；`IControlTokenAlgorithm<FloatButtonToken>`（模式见 `PrimeNGToolTipTokens.cs`）。
- Produces: `PrimeNGFloatButtonTokens`（六 attribute：PrimeNG Aura/Aura.Dark/Lara/Lara.Dark/Nora/Nora.Dark，EvaluateOwn 写 `token.FloatButtonShadows = default;`）。

- [ ] **Step 1: 扩展失败测试**

参数化 12 个 profile（PrimeNGAura/Lara/Nora × ""/.Dark/.Compact/.DarkCompact）：断言 `Own<BoxShadows>(snapshot, "FloatButton", "FloatButtonShadows").Count.ShouldBe(0)`；另源码断言四个 AXAML 中不再出现 `BoxShadowsSecondary`、出现 `FloatButtonTokenResource FloatButtonShadows`。

- [ ] **Step 2: 运行确认失败**（同 Task 1 命令，Expected: FAIL——PrimeNG 下仍是全局 md 双层）

- [ ] **Step 3: 实现**

四处 AXAML 的 Setter 改绑 `{atom:FloatButtonTokenResource FloatButtonShadows}`；新建 `PrimeNGFloatButtonTokens.cs`（类注释引用官方 3.1.1 button/speeddial 无阴影证据）。

- [ ] **Step 4: 运行确认通过**

Run: `dotnet test tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj --framework net10.0 --filter PrimeNGFloatButtonShadow`
Expected: PASS（含 Task 1 断言）。

- [ ] **Step 5: 构建验证**

Run: `dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj` 与 `dotnet build src/AtomUI.Themes.PrimeNG/AtomUI.Themes.PrimeNG.csproj`
Expected: 0 error（证明生成器产出新槽位、XAML 编译通过）。

### Task 3: 文档、清理与聚焦验证

**Files:**
- Modify: `src/AtomUI.Themes.PrimeNG/README.md`（阴影对照表加 FloatButton 行，标注官方依据）
- Create: `docs/developments/progress/2026-11-17-primeng-floatbutton-shadow.md`（根因、对比矩阵、修复记录、Message/Notification 已确认不对齐待另立任务）

- [ ] **Step 1: 更新 README 阴影表与 progress 文档**
- [ ] **Step 2: 删除临时测试目录** `tests/AtomUI.Themes.Tests/Temporary/PrimengFloatButtonShadow/`
- [ ] **Step 3: 聚焦验证（iterate，模块路径显式）**

Run:
```
python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Themes.PrimeNG
python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls/FloatButton
```
Expected: 全部通过。
- [ ] **Step 4: `git diff --check` + 备好提交信息草案，交用户审查（不 commit）**

交付说明须含：分支/worktree 信息、对比矩阵、真机视觉验收步骤（含示例入口分类）。
