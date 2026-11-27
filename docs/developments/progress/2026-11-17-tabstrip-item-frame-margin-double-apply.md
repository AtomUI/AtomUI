# TabStripItem Margin 双重应用修复（选中背景右侧偏窄）

日期：2026-11-17
状态：已修复，待用户审查与真机视觉验收（未提交）

## 现象

Gallery → Navigation → TabStrip 页 → Semantic Parts 页签下「Custom Semantic Part styling」示例（Examples 列表最后一项）中，线型 TabStrip 选中 Tab 的浅蓝背景块右边缘比底部墨条窄一截。用户截图实测（2x 物理像素）：背景 1050–1122，墨条 1050–1130，右侧差 8 物理像素 = 4 逻辑像素，正好等于示例语义 Style 的 `Margin="0,0,4,0"` 右边距；左侧齐平（左边距为 0）。

## 根因

`BaseTabStripItemTheme.axaml`（及同族的 `BaseTabItemTheme` / `CardTabItemTheme` / `CardTabStripItemTheme`）模板根 `Border#Frame` 声明了 `Margin="{TemplateBinding Margin}"`：

1. item 的 `Margin` 已由布局系统在外圈应用一次；模板内 TemplateBinding 再应用一次 → 背景框（画在 Frame 上）缩进到 `selected.Bounds` 之内。
2. 墨条由 `TabSelectionIndicator.ResolveBounds` 的 `TabIndicatorMode.Full` 按 `selected.Bounds` 绘制，不缩进。
3. 内置线型 token `HorizontalItemMargin` 默认全 0，默认主题下两者无错位，缺陷不可见；一旦通过生成语义 Style（`TabStripItemStyle` 等）给 item 设非 0 Margin 即触发。

## 修复

移除上述 4 个 item 模板 `Frame` 上的 `Margin="{TemplateBinding Margin}"`，Margin 语义回归"由布局系统应用一次"：背景框 == item Bounds == 墨条范围；默认 token 为 0，默认视觉不变。修复同时消除了 Margin 被双计导致的 item 间距虚增。

## 验证（TDD）

- 临时测试（`tests/AtomUI.Desktop.Controls.Tests/Temporary/tabstrip-item-frame-margin/`，已删除）复现：线型 Frame 70 vs Bounds/墨条 74；卡片 Frame 104 vs Bounds 110。修复后转绿。
- 清洁树聚焦回归：Desktop.Controls.Tests TabControl 命名空间 283/283；Rendering.Tests TabControl 过滤 95/95；Gallery TabStrip/TabControl 页面测试 22/22。
- `git diff --check` 干净；改动仅 4 个 axaml 主题文件。
- 验证 CLI `--scope iterate --path src/AtomUI.Desktop.Controls/TabControl` 被 HEAD 上既有全仓审计问题阻断（Shadcn/PrimeNG 未登记产物等，与本次无关），改用等价精确 filter。
