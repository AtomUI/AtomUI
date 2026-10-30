# NumericUpDown Semantic Part 改造 · 真机视觉验收步骤

> 状态：通过（用户回传截图驱动，两处问题已修复并经用户确认；2026-09-03 用户确认整体视觉验收完成）。
>
> ## 验收结论记录（文字证据）
>
> | 日期 | 证据 | 结论 | 修复记录 |
> | --- | --- | --- | --- |
> | 2026-08-30 | 用户截图：Input 模式下 clear 图标贴顶、未垂直居中 | 确认为缺陷（headless 复测：按钮 desired 14×14、valign=Top，面板被后缀内容撑高后贴顶） | `f6bd623d9` 两模板 `PART_ClearButton` 显式 `VerticalAlignment="Center"`，回归测试锁定偏差 ≤1px |
> | 2026-08-30 | 用户截图（antd 对照）：语义样式示例未按 antd 呈现根边框着色 | 确认为缺陷（根 BorderBrush 中继链缺失，Frame 状态机接管边框） | `0f14fc0e5` 复刻 `AbstractTextInput` 的 LocalValue 中继（`RelayFrameBorderBrush`），示例重做为 Object 蓝 #1677FF / Function 紫 #722ED1，严格对齐 antd |
> | 2026-08-30 | 用户回话「确认」 | 上述两处修复的真机渲染经用户确认通过 | 无 |
> | 2026-08-30 | 用户截图：Spinner 模式两个示例的数值文本已垂直居中 | Spinner 文本偏顶缺陷修复经用户截图确认通过（`379e03ac5`） | 无 |
> | 2026-09-03 | 用户会话确认「NumericUpDown 已经完成视觉验收」 | Examples 各示例整体走查、Semantic Parts 页签 hover 高亮四边完整性、深浅色主题、Spinner 模式交互全部通过，验收闭环 | 无 |

## 背景

本次改造涉及两处可能影响渲染的模板变更，需要重点确认：

1. `NumericUpDownTheme.axaml` 两个模板新增 `InnerLeftContent` 包装 presenter（prefix part）。
2. `NumericUpDownSpinnerTheme.axaml`（Spinner 模式）将 `InnerLeftContent` / `InnerRightContent`
   的呈现位置从 spinner 自有 presenter 迁移到 decorated box 的 `ContentLeftAddOn` / `ContentRightAddOn`
   槽位（与 Input 模式同构）。+/− 按钮位置不变。

## 操作路径与判定标准

准备：运行 AtomUIGallery（`controlgallery/AtomUIGallery.Desktop`），进入
Data Entry → NumberUpDown 页面。

### 步骤 1：Examples 面板逐项检查（回归）

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 1.1 | 查看 Basic usage / Spinner mode / Hide handle / String mode / Keyboard / Mouse wheel / Min-Max / Decimal / Sizes / Variants / Disabled / Pre-Post tab / With clear icon / Prefix and suffix / Status 各示例 | 与改造前渲染一致；无布局错位、无多余间距 | 每项 1 张或全景 1 张 |
| 1.2 | 查看 Prefix and suffix 示例（Input 与 Spinner 两种模式） | `$` 前缀与 `kg` 后缀位置：Input 模式在文本两侧、+/− handle 外侧；Spinner 模式在文本两侧、左右按钮内侧。与 6.1.x 版本渲染对比无漂移 | 2 张（两模式各 1） |
| 1.3 | With clear icon 示例：输入文本后观察清除按钮 | 清除按钮出现在后缀区域，位置与改造前一致 | 1 张 |

### 步骤 2：Semantic Parts 页签（新功能）

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 2.1 | 进入页面顶部 "Semantic Parts" 页签（首次进入才加载） | 出现两组 Preview：NumericUpDown（Input 模式）与 Spinner mode，各含 root/prefix/input/suffix/clear 五个 Part 条目 | 1 张 |
| 2.2 | Hover / Pin 某个 Part 条目（如 `prefix`） | 对应区域高亮描边完整可见（含四边），无裁剪缺失 | 2 张（prefix、clear 各 1） |
| 2.3 | 查看 "Custom Semantic Part styling" 示例（面板底部） | 第一组：前缀 `$` 与数值文本为蓝色；第二组：后缀与清除按钮呈半透明 | 1 张 |

### 步骤 3：交互回归（可录屏替代 3.1–3.3 截图）

| # | 操作 | 预期现象 |
| --- | --- | --- |
| 3.1 | Spinner 模式：点击 +/− 各 2 次 | 数值正常步进；按钮视觉与改造前一致 |
| 3.2 | Input 模式：hover 输入框 | 浮动 handle 出现，位置正常 |
| 3.3 | 两种模式下输入文本触发清除按钮、点击清除 | 值清空，按钮隐藏 |
| 3.4 | 切换 Window 深浅色主题 | 两种主题下 Semantic Parts 页签与样式示例无异常 |

## 判定标准

- 所有截图与 6.1.6 基线（改造前 release/6.0）渲染一致，仅 Semantic Parts 页签与样式示例为新增。
- 未回传截图前，本次改造标注「待视觉验收」，不宣称通过。
