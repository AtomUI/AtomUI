# Shadcn CheckBox 勾选框变圆形修复（误用按钮圆角阶梯）

日期：2026-11-17
状态：已修复，待用户审查与真机视觉验收（未提交）
分支：`fix/shadcn`（worktree：`/Users/chinboy/Projects/dotnet/AtomUIV6-fix-shadcn`）

## 现象

Shadcn（尤其 **Luma**）下勾选框外轮廓变成**圆形**，看起来像单选框。用户提问“这个设计就是这样的吗”。

## 结论：不是设计

官方附录 C（`docs/superpowers/specs/2026-11-15-shadcn-latest-alignment-data.md` §C checkbox）逐 Style 给出：

| Style | 官方 class | 圆角 |
|---|---|---|
| Vega / Nova / Mira | `rounded-[4px]` | 4 |
| Maia | `rounded-[6px]` | 6 |
| **Luma** | `rounded-[5px]` | **5** |
| Rhea | `rounded-[5px]` | 5 |
| Lyra / Sera | `rounded-none` | 0 |

## 根因

1. `CheckBoxTheme.axaml` 把勾选框 `CornerRadius` 绑到全局 `BorderRadiusSM`；
2. Shadcn 的全局圆角来自**按钮**阶梯：`BorderRadiusSM = style.ButtonRadiusSM`，Luma/Maia 为 **26**；
3. 指示器只有 16px（`ControlInteractiveSize`），半径 26 超过半高 8 → 按 CSS 圆角语义被钳到 8 = **正圆**。

即：控件缺一个控件级圆角覆写，退化到按钮阶梯后被“正确”地钳成了圆。f19f3a9fe 的钳制修复没有错，它只是把
“本来就不该给勾选框的 26”如实地画成了圆。

## 修复（源头）

1. `ShadcnStyle` 记录新增 `CheckBoxRadius` 字段，8 个 Style 按附录 C 填值（4/4/6/0/4/5/0/5）。
2. 新增 `src/AtomUI.Themes.Shadcn/Algorithms/Controls/ShadcnCheckBoxTokens.cs`
   （`IControlTokenAlgorithm<CheckBoxToken>`，注册 8 Style × 明暗共 16 条），在控件作用域写
   `BorderRadiusSM = style.CheckBoxRadius`；`CheckBoxTheme` 与 `CheckBoxToken` 未改动。

## 验证（TDD）

- **RED**：`tests/AtomUI.Themes.Tests/Shadcn/ShadcnControlTests.CheckBox_Uses_The_Style_Checkbox_Radius`
  （8 Style 逐个断言 `CheckBox` 作用域 `BorderRadiusSM`）——实现前 6 红（Nova/Vega/Maia/Mira/Luma/Rhea；
  Lyra/Sera 因按钮圆角本就是 0 而通过）。
- **GREEN**：实现后 8/8 绿。
- **渲染实测**（临时探针，已删除）：Luma 勾选框恢复 5px 圆角方框，Nova 4px、Maia 6px，勾选/未选/半选三态一致。
- **模块复验**：`AtomUI.Themes.Tests` 335/335；`Desktop.Controls.Tests`（CheckBox 过滤）13/13；
  `AtomUIGallery.Tests`（CheckBox 过滤）3/3；`git diff --check` 干净。

## 同类风险（未在本次范围内修）

其他仍直接复用全局 `BorderRadius*` 的控件在 Luma/Maia 下可能同样被钳成圆形或过圆，附录 C 对其中部分给了专用值：
Calendar 日期格（`--cell-radius`）、Badge（`rounded-4xl`/`rounded-3xl`/`rounded-full` 因 Style 而异）、
Tabs、Alert、Table、Slider、Progress 等。建议按同一“控件级圆角阶段”模式做一次系统盘点后再批量修。
