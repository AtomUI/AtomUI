# Shadcn Empty 内置插画被冲淡修复（fill 阶梯塌缩）

日期：2026-11-17
状态：已修复，待用户审查与真机视觉验收（未提交）
分支：`fix/shadcn`（worktree：`/Users/chinboy/Projects/dotnet/AtomUIV6-fix-shadcn`）

## 现象

Gallery → Data Display → **Empty** 页（Examples 页签）在 Shadcn 设计语言下，内置插画被冲淡成近乎无对比的
浅灰单色块：Basic / No description（`PresetImage=Default`）看不出"盒子+气泡"的结构，Size（`PresetImage=Simple`）
的开口盒失去描边与内部填充的层次。用户截图（2600×1800，红框标注三处）实测。

## 根因

1. `EmptyTheme.axaml` 用**全局 fill 阶梯**给内置插画上色：
   `BorderColor←ColorFill`、`ShadowColor←ColorFillTertiary`、`ContentColor←ColorFillQuaternary`、
   `BorderColorSecondary←ColorBorderSecondary`；`BuiltInImageBuilder` 再按这三色拼出 SVG。
2. Shadcn 的颜色映射（设计文档 §5.4、README「颜色映射」表，实现于
   `ShadcnStyleAlgorithm.WriteColors` / `EvaluateAlias`）把**四档 fill 全映射到同一档**：
   - `ColorFill` ← `--accent` = neutral stop 100 = `#F5F5F5`
   - `ColorFillSecondary` ← `--secondary` = stop 100 = `#F5F5F5`
   - `ColorFillTertiary` / `ColorFillQuaternary` ← `--muted` = stop 100 = `#F5F5F5`
   - `ColorBorderSecondary` ← `--border` = stop 200 = `#E5E5E5`
3. 于是插画所有路径都取到同一个 `#F5F5F5`，只剩一个 stop 200 的底影 → 结构消失。
   Ant Design 原版插画用**固定灰阶**（#AEB8C2 / #DCE0E6 / #F5F5F7），AtomUI 改为令牌着色后被这一塌缩击穿。

对照验证（无头 Skia 渲染，同一控件三种主题）：默认主题与 PrimeNG Aura 均正常，**仅 Shadcn 异常** →
确认根因在 Shadcn 侧，与 Empty 模板/几何无关。

## 修复

只修 Shadcn 的 Empty（用户选定范围，其他主题与其他 Shadcn 控件零改动）：

新增 `src/AtomUI.Themes.Shadcn/Algorithms/Controls/ShadcnEmptyTokens.cs`
（`IControlTokenAlgorithm<EmptyToken>`，注册 8 个 Style × 明暗共 16 条），在**控件作用域**内按 Ant 的前景合成
阶梯重建插画用色：

```
ColorFill            = CompositeOver(Foreground, 0.15, Background)
ColorFillSecondary   = CompositeOver(Foreground, 0.06, Background)
ColorFillTertiary    = CompositeOver(Foreground, 0.04, Background)
ColorFillQuaternary  = CompositeOver(Foreground, 0.02, Background)
ColorBorderSecondary = CompositeOver(Foreground, 0.06, Background)
```

用前景/背景合成而非固定灰阶：明色下得 `#DADADA / #F5F5F5` 一类的 Ant 阶梯，暗色下前景翻转为近白，
阶梯自动反向（`#2E2E2E / #141414`），任意 Base Color 下方向都正确。

`EmptyTheme.axaml` 与 `EmptyToken` **未改动**（令牌映射本身没变，变的是 Shadcn 作用域内的取值）；
README 增补一条控件级覆写说明（对应设计文档 §5.4 的完整性要求）。

## 验证（TDD）

- **RED**：在既有 `tests/AtomUI.Themes.Tests/Shadcn/ShadcnControlTests.cs` 新增
  `Empty_Illustration_Keeps_A_Fill_Ramp`（断言 Empty 作用域内 `ColorFill` 相对 `ColorFillTertiary`、
  `ColorFillTertiary` 相对 `ColorFillQuaternary`，都必须沿"远离页面底色"的方向拉开对比）——
  修复前 neutral/zinc × 明暗 4 例全红。
- **GREEN**：实现后 4/4 绿；渲染实测（临时探针，已删除）确认 ShadcnNova / ShadcnLuma 浅色下插画恢复
  "显示器+线条/空心开口盒"的完整结构，ShadcnNova 深色下阶梯分层可见（与 AtomUI 深色既有的
  `rgba(255,255,255,0.15)` 约定一致）。
- **清洁树模块复验**：`AtomUI.Themes.Tests` 327/327；`AtomUI.Desktop.Controls.Tests`（Empty 过滤）86/86；
  `AtomUIGallery.Tests`（Empty 过滤）4/4。
- 临时渲染探针（`Rendering.Tests/Temporary/EmptyProbe/`）验证后删除；未新增永久测试文件
  （断言并入既有 Shadcn 控制令牌测试，无新 fixture / 无新项目）。
- 验证 CLI `--scope iterate --path src/AtomUI.Themes.Shadcn` 被 HEAD 上既有全仓审计问题阻断
  （Shadcn/PrimeNG 未登记产物等，与本次无关），改用等价精确 `dotnet test`。

## 备注

- `url(#linearGradient-1)` 悬空引用是 **Ant Design 原版自带**（官方 `empty.js` 同样未定义该渐变），
  非本次缺陷，未改动。
- 未改 `EmptyTheme.axaml` 的通用映射，因此默认/PrimeNG 等主题的 Empty 外观不变。
- 真机视觉验收步骤见交付说明（待截图回传）。
