# QRCode 掩码态圆角失效修复（cover 方框盖住圆角外框）

日期：2026-11-17
状态：已修复，待用户审查与真机视觉验收（未提交）
分支：`fix/shadcn`（worktree：`/Users/chinboy/Projects/dotnet/AtomUIV6-fix-shadcn`）

## 现象

Gallery → Data Display → **QRCode** 页（Examples 页签）中，非 Active 状态的二维码（Loading / Expired / Scanned）
圆角外框消失，整块变成**直角方块**；Four corners 被用户用红框逐个标注。Shadcn Luma/Maia（圆角 26）下最刺眼。

## 根因

`QRCodeTheme.axaml` 模板结构：`PixelAlignedBorder#Frame`（画背景/边框/圆角）→ `Grid` → `Border#Cover`（掩码）。

1. `Cover` 是 `Frame` 的**子节点**；`DashedBorder.Render` 先画自身背景/边框，子节点在其后绘制 → `Cover` 盖在边框之上。
2. `Cover` 没有裁剪、也没有圆角，其背景是 `QRCodeMaskBackgroundColor`（容器色 244/255 alpha，近似不透明白）。
3. 于是掩码铺满**方形**内容框：把边框冲淡到几乎不可见，并在四角把圆角外框整个盖掉 → 圆角视觉失效。

三个状态都复现；默认主题（圆角 8）同样存在，只是方块与白底同色、不易察觉；Luma/Maia 圆角 26 时缺陷最明显。

同一缺陷类在 `ExpanderTheme.axaml` 已有结论与注释（`PART_Frame` 必须开 `ClipContentToCornerRadius`，
否则子节点不透明背景会平铺到方形边界、覆盖圆角），修复沿用该契约。

## 修复

`src/AtomUI.Desktop.Controls/QRCode/Themes/QRCodeTheme.axaml`：`Frame` 增加
`ClipContentToCornerRadius="True"`，把整棵内容子树（二维码图像、Icon、掩码、Loading/Expired/Scanned 内容）
统一裁到圆角**内**框。效果：

- 掩码按圆角形状铺开，四角露出页面底色 → 圆角恢复；
- 裁剪发生在边框内缘，`Frame` 自己画的 1px 边框不受影响（实测边框仍是 `#E5E5E5`）；
- `Active` 态内容本就在内框内，视觉不变；
- 二维码本体（padding 12、圆角 ≤26）仍完整，不裁掉角上的定位块。

## 验证（TDD）

- **RED**：临时渲染测试（绿底 + `Status=Scanned`）确认掩码角外像素 121/172 为掩码白，默认主题 8/23 → 修复前
  Luma/Maia/default 三例全红。
- **GREEN**：开启裁剪后三例全绿；渲染实测确认 Loading / Expired / Scanned 均为圆角卡片，Active 态保持不变。
- **永久回归测试**：新增 `tests/AtomUI.Desktop.Controls.Rendering.Tests/QRCodeMaskCornerRenderingTests.cs`
  （Loading/Expired/Scanned 三态，显式 `CornerRadius=26` 以脱离主题 Token，绿底判角外掩码像素占比）。
  已证明无修复时 3/3 红（`corner0 ... 121/172 corner-void pixels are mask white`），修复后 3/3 绿；
  与既有 `ButtonSpinnerHandleClipRenderingTests` 同一测试形态（同为“内容必须裁到框内”的契约），无新项目/新 fixture。
- **模块复验**：`Rendering.Tests`（QRCode/DashedBorder/Clip 过滤）22/22；`Desktop.Controls.Tests`（QRCode 过滤）
  8/8、全量 4508/4508；`AtomUIGallery.Tests`（QRCode 过滤）3/3；`git diff --check` 干净。
- 临时探针（`Rendering.Tests/Temporary/QRProbe/`）验证后删除。

## 备注

- 该修复在 `AtomUI.Desktop.Controls`，与主题无关：所有主题下的掩码圆角一并修复。
- 与本次分支另外两项（输入胶囊圆角、Shadcn Empty 插画）相互独立，可单独审阅。
