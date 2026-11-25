# PrimeNG Message/Notification 卡阴影对齐官方 实施计划

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** PrimeNG 设计语言下 MessageCard 与 NotificationCard 的阴影对齐官方 `@primeuix/themes` 3.1.1 的 message/toast 取值：无 severity（normal/Default）= 各 preset 的 `overlay.popover.shadow`；info/success/warn/error = `0px 4px 8px 0px` 按 severity 着色 4% 单层阴影（blue/green/yellow/red 的 500 档）；Ant Design 基线渲染不变。

**Architecture:** 沿用 FloatButton 轮模式：卡片 token 新增 per-severity 阴影自有槽位（Ant 默认全部 = 全局 `BoxShadows`），ControlTheme 的 severity 样式块改绑各槽位，PrimeNG 包新增两个 `IControlTokenAlgorithm` 写官方值。

**Tech Stack:** 同 FloatButton 轮（Avalonia BoxShadows、ControlDesignToken 槽位、ControlTokenAlgorithm）。

**Spec:** 官方证据 = `@primeuix/themes` 3.1.1 `dist/aura/{message,toast}/index.mjs`（明暗两段同为 `{blue,green,yellow,red}.500` @ `color-mix(..., transparent 96%)` = 4%；toast normal = `{overlay.popover.shadow}`；secondary/contrast 用 surface.500/950，AtomUI 枚举无对应项，不映射）。用户 2026-11-17 指令：PrimeNG 后续优化都在本分支/worktree 进行。上一轮报告把本项列为待办。

## Global Constraints

- 官方值逐字：severity 阴影 `0px 4px 8px 0px`，色 = 500 档 @ 4%（alpha 0x0A，同仓库 10%→0x1A 的取整惯例）；normal = preset 的 `PopoverShadows`（Aura/Nora md 双层、Lara `0 1 3 0` 单层）。
- Ant 基线：所有新槽位默认 = `EffectiveGlobalToken.BoxShadows`，六个 severity 视觉与改前一致。
- 绑定留在 ControlTheme 的 severity 样式块（`[MessageType=X]` / `[NotificationType=X]`），组合关系由 selector 表达。
- Message `Loading` 是 Ant 独有：取 Resting 值（proposal，README 注明）；`WindowMessageManager` 背板 `BoxShadowsTertiary` 是 Ant 装饰结构，不动（已知差异）。
- 临时测试放 `tests/AtomUI.Themes.Tests/Temporary/PrimengMessageShadow/`，验证后删除；无用户授权不 commit。

## Review Focus

1. Ant 基线回归（某 severity 阴影意外变化）→ 测试逐槽位断言默认主题下 == 全局 BoxShadows。
2. Lara 的 Resting 与 Aura 不同 → 测试按 preset 断言 Resting。
3. 槽位遗漏（某个 severity 未接样式或算法未写）→ 12 profile × 全槽位参数化断言 + AXAML 源码检查。
4. ATOMUIGEN042（槽位未被 AXAML 消费）→ 构建即证。
5. 生成器/效果门（无 Message/Notification 效果测试家族，新槽位不引入新门）→ 聚焦套件回归确认。

---

### Task 1: 槽位 + AXAML + PrimeNG 算法（一并实现，规避 ATOMUIGEN042 分步编译失败）

**Files:**
- Modify: `src/AtomUI.Desktop.Controls/Message/MessageCardToken.cs`（RestingShadows + Information/Success/Warning/Error/LoadingShadows，6 槽）
- Modify: `src/AtomUI.Desktop.Controls/Notifications/NotificationCardToken.cs`（RestingShadows + Information/Success/Warning/ErrorShadows，5 槽）
- Modify: `src/AtomUI.Desktop.Controls/Message/Themes/MessageCardTheme.axaml:103` 与五个 `[MessageType=X]` 块
- Modify: `src/AtomUI.Desktop.Controls/Notifications/Themes/NotificationCardTheme.axaml:120` 与四个 `[NotificationType=X]` 块
- Create: `src/AtomUI.Themes.PrimeNG/Algorithms/Controls/PrimeNGMessageCardTokens.cs`、`PrimeNGNotificationCardTokens.cs`（各六算法）
- Test: `tests/AtomUI.Themes.Tests/Temporary/PrimengMessageShadow/PrimeNGMessageShadowTests.cs`

**Interfaces:**
- Produces: `MessageCardToken.{Resting,Information,Success,Warning,Error,Loading}Shadows`、`NotificationCardToken.{Resting,Information,Success,Warning,Error}Shadows`（均 `partial BoxShadows`）。
- 共享 helper：`0 4 8 0` @4%（`PrimeNGSeverityShadow` 静态方法置于其中一个算法类，另一个复用）。

- [ ] **Step 1: 失败测试**——① DaybreakBlue 下两卡全部槽位 == 全局 BoxShadows；② 12 profile × {MessageCard, NotificationCard}：Resting == 该 preset 的 popover 阴影、四色槽位 == `0 4 8 0` + `#0A` + 对应 500 档色、Message Loading == Resting；③ 源码检查：两主题文件卡体不再绑裸 `BoxShadows`、severity 块出现对应槽位绑定。
- [ ] **Step 2: 运行确认失败**（缺槽位/旧绑定）
- [ ] **Step 3: 实现**（token 槽位 → AXAML → 两算法）
- [ ] **Step 4: 运行确认通过**
- [ ] **Step 5: 聚焦回归**：Themes.Tests 全项目 + Desktop.Controls.Tests `~Message|~Notification` + `git diff --check`

### Task 2: 文档与收尾

- [ ] PrimeNG README「Aura control stages」补 MessageCard / NotificationCard 两行（severity 阴影 + Loading proposal + backplate 已知差异）
- [ ] `docs/controls/desktop/feedback/{message,notification}/{token.md,changelog.md}` 同步（分类表 + 2026-11-17 条目）
- [ ] progress 文档追加本项记录；删除临时测试；交付真机验收步骤（不 commit）
