# Select Semantic Part 改造 · 真机视觉验收步骤

> 状态：**待视觉验收**。改造代码与测试已完成；以下步骤是唯一的真机外观验收依据。
> 未收到用户回传截图/录屏前，不宣称视觉验收通过。

## 背景

本次 Select 语义部件改造涉及四处可能影响渲染的变更，需要重点确认：

1. `SelectTheme.axaml` 新增 9 处静态 marker，并新增 `ContentLeftAddOn` 前缀投影节点（`LeftAddOn`
   内容的承载方式变化，带前缀的 Select 需重点回归）。
2. 共享资产微调：`SelectHandleTheme.axaml` 的清除按钮已含 `semantic-clear` inert class marker，
   `TagTheme.axaml` 的 `itemContent` / `itemRemove` marker 已存在（Cascader 改造时引入，Select 复用）；
   这些是 inert class，理论上零视觉影响，需抽查确认。
3. `SelectTheme.axaml` 的 `PopupFrame` 改为静态节点承载 `semantic-popup-root`（弹层视觉不变，
   但候选列表改为运行时装入该根节点）。
4. `SelectShowCase.axaml` 宿主从 `GalleryStickyTabsHost` 迁移到 `GalleryShowCaseHost` 并新增
   Semantic Parts 页签（示例内容本身不变，页面壳层结构有预期变化）。

Select 对外开放的语义部件：`root`、`prefix`、`content`、`placeholder`、`input`、`suffix`、
`clear`、`item`、`itemContent`、`itemRemove`、`popup.root`、`popup.list`、`popup.listItem`。
Semantic Parts 页签中的预览为多选模式并预选两项、开启过滤，标签、搜索输入与弹层同时可见。

## 操作路径与判定标准

准备：运行 AtomUIGallery（`controlgallery/AtomUIGallery.Desktop`），进入
Data Entry（数据录入）→ Select 页面。

### 步骤 1：Semantic Parts 页签

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 1.1 | 点击页面顶部 "Semantic Parts" 页签 | 页面切换到语义预览：出现一个带 `prefix` 文字的多选 Select，预选两个标签，候选弹层钉住常开，页面其余区域无遮罩阻挡（light-dismiss 遮罩被抑制） | 全景 |
| 1.2 | 在预览右侧部件列表中逐个选中 13 个 Part（root / prefix / content / placeholder / input / suffix / clear / item / itemContent / itemRemove / popup.root / popup.list / popup.listItem） | 每个 Part 都有对应高亮框；`popup.root` / `popup.list` / `popup.listItem` 在钉住弹层上高亮，`item` / `itemContent` / `itemRemove` 在预选标签上高亮 | 逐部件或分组 |

### 步骤 2：Custom Semantic Part styling 示例

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 2.1 | 滚动到 "自定义语义结构的样式" 示例 | 出现两个带 `prefix` 的 Select：上方 Outlined（object styles）、下方 Filled（function styles） | 全景 |
| 2.2 | 观察 Outlined 控件 | prefix 文字为 `#BFBFBF` 浅灰；无选中时占位符文字为 `#1890FF` 蓝色 | 局部 |
| 2.3 | 观察 Filled 控件 | 后缀箭头为 `#1890FF` 蓝色；两个控件等高（Middle 32px），Filled 灰底铺满圆角框上下无白边 | 局部 |
| 2.4 | 点击打开 Outlined 控件弹层 | 弹层外框为 `#1890FF` 蓝色 1px 边框；候选项文字为 `#272727` 深色 | 弹层展开 |
| 2.5 | 点击打开 Filled 控件弹层 | 弹层外框为 `#CCCCCC` 灰色 1px 边框；候选项文字为 `#1890FF` 蓝色 | 弹层展开 |

### 步骤 3：Examples 回归抽查（共享资产影响面）

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 3.1 | 回到 "Examples" 页签，滚动到基本用法 / 多选 / 前缀后缀示例 | 单选、多选、带 prefix/suffix 的 Select 与改造前外观一致：清除按钮、展开箭头、标签、占位符均正常，无错位或丢失 | 全景 |
| 3.2 | 打开任一多选 Select 的候选弹层 | 候选列表正常渲染，无异常背景/边框，候选项 hover/selected 视觉一致 | 弹层展开 |

## 判定标准

- 步骤 1.1：预览弹层钉住常开、无遮罩 → light-dismiss 抑制机制生效（对应 `AbstractSelect` 既有机制，Select 无需新增）。
- 步骤 1.2：13 个 Part 均可解析高亮，尤其跨视觉根的 `popup.*` 与运行时创建的 `item` / `popup.list` /
  `popup.listItem` → marker 注入与路由正确。
- 步骤 2：语义样式命中（prefix 灰、占位符蓝、Filled 箭头蓝、弹层边框与候选项颜色差异化）→ 生成
  `Select*Style` 跨模板/跨视觉根命中目标。
- 步骤 3：共享 `SelectHandleTheme` / `TagTheme` / `AddOnDecoratedBoxTheme` 的 inert marker 未改变既有视觉。

## 证据要求

按上表回传截图或录屏。图像不入库：验收完成后仓库只保留本文字步骤与结论，不保存截图。
