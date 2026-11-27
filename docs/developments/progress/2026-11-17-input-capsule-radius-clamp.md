# 输入胶囊大圆角双层弧不同心修复（Luma/Maia InputRadius 超半高）

日期：2026-11-17
状态：已修复，待用户审查与真机视觉验收（未提交）
分支：`fix/shadcn`（worktree：`/Users/chinboy/Projects/dotnet/AtomUIV6-fix-shadcn`，基于 `release/6.0` @ 2af948083；原分支/目录 `fix/input-capsule-radius-clamp` 已按用户要求改名重命名）

## 现象

Shadcn **Luma** 设计语言下，Gallery → Components → General → Icons 页的搜索框（`atom:SearchEdit`）左端胶囊弧线畸形：黑色描边与外层 focus ring 不是同一条弧的同心两层，对角弧区（约行 41–43、66–70）两层之间出现 2–3px 近白楔形断层，左端上下圆角观感不对称。用户截图（2000×1385，2x）红框标注。

## 根因

Shadcn 主题给整个 Input 家族设 `InputRadius = 22`（Luma；Maia 26），Luma 的「胶囊」按钮/输入是 `rounded-4xl` 固定 26/22 的语义，落在 32px 高的 middle 输入上即 **radius 22 > 半高 16**（超半径）。

同一块像素由**两条独立的圆角绘制管线**产出，二者对「超半径」的处理规则不同：

1. **边框 / 背景 / 内容裁剪**：走 `BorderRenderHelper.Update` 的 **WinUI keypoints** 几何路径（非均匀圆角或非 CenterBorder 时必走该路径；输入框 ContentFrame 的 `(22,0,·,22)` 即非均匀 → 必走）。`RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI` 对重叠半径做的是**按比例切分到椭圆弧**（右侧分解 44 = 22.5/21.5，弧半径水平 22.5、垂直 15.5 → 非圆椭圆弧）。
2. **focus ring（BoxShadow）**：走 Skia `DrawRectangle(brush, pen, RoundedRect, BoxShadows)` → `SKRoundRect.SetRectRadii`，Skia 在 `scaleRadii` 中做的是 **CSS 式统一缩放**：f = min(边长 / 相邻半径和) = 16/22 = 0.727 → 半径 16 的**正圆**。

两条弧数学形态不同（椭圆弧 vs 正圆），必然不同心，两层之间露出背景色 → 视觉上的断裂/错位。f19f3a9fe 只修了 Segmented/ButtonSpinner 这类**自绘** RoundedRect，未触及 `BorderRenderHelper` 这条几何路径，因此输入框家族（LineEdit/Select/DatePicker/SearchEdit…）全部漏网。

## 修复

统一按 **CSS 语义**在几何入口缩放一次，让所有层共用同一形状：

1. `AtomUI.Media.CornerRadiusUtils.ScaleToSize(radius, width, height)`（新增）：f = min(宽/(TL+TR), 宽/(BL+BR), 高/(TL+BL), 高/(TR+BR))，f<1 时四角同乘 f；f≥1 原样返回。与现有 `ClampToSize`（每角单独截到 min/2）语义不同：相邻半径之和恰好等于边长时保留整半径，与浏览器/Skia 一致。
2. `BorderRenderHelper.Update`：进入 WinUI keypoints 前先 `ScaleToSize(finalSize)`，背景、描边环、box shadow 三层共用缩放后的半径，天然同心。
3. `DashedBorder.BuildRoundedRectClipFigure`：内容裁剪几何同样先 `ScaleToSize(innerEdgeBounds)`，避免 clip 用椭圆而边框用正圆导致裁剪边缘与边框错位。

## 验证（TDD）

- **RED**：临时渲染测试（`tests/AtomUI.Desktop.Controls.Rendering.Tests/Temporary/InputCapsuleRadius/`，headless Skia 帧）先复现两处失败——「接缝检测」报 `row 41: 2px near-white wedge between the focus ring and the border`；「切线行 fill 探针」报背景未达 CSS 胶囊范围。
- **GREEN**：修复后两测试转绿；逐像素对比确认修复前 (240,240,240)/(255,255,255) 近白楔形在修复后变为 ring→描边直接蓝灰过渡，断层消失。
- **临时测试处置**：渲染临时测试（含临时 dump 探针）验证后删除；`ScaleToSize` 的 5 条契约断言并入既有永久套件 `tests/AtomUI.Core.Tests/Media/CornerRadiusClampTests.cs`（与 f19f3a9fe 的 ClampToSize 测试同家，属同一 CSS 语义契约）。
- **清洁树模块复验**：Core.Tests 635/635、Controls.Shared.Tests 201/201、Controls.Tests 241/241、Rendering.Tests（DashedBorder/Shadow 过滤）98/98 全绿。
- **既有失败归因**：Rendering.Tests 全量有 9 个失败，全部集中在 `Effect.Families.*EffectTests`（Menu/MenuFlyout/NavMenu/ProgressBar 的 token 效果）；在**主工作区 release/6.0（无本修复）**同过滤复跑同样 Failed: 9，证实为 HEAD 既有失败，与本次改动无关。
- `git diff --check` 干净；改动 3 个生产文件 + 1 个测试文件（+88 / −1）。

## 待办

- 真机视觉验收（步骤见交付说明；截图/录屏需用户回传）。
- 验证 CLI `--scope iterate --path ...` 被 HEAD 上既有全仓审计问题阻断（Shadcn/PrimeNG 未登记产物等，与本次无关），改用等价精确 `dotnet test` 模块套件与 filter。
- 合并回 `release/6.0` 需用户审查授权后再执行。
