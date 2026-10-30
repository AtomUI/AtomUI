# DatePicker Semantic Part 改造 · 真机视觉验收步骤

> 状态：**待视觉验收**。改造代码与测试已完成；以下步骤是唯一的真机外观验收依据。
> 未收到用户回传截图/录屏前，不宣称视觉验收通过。

## 背景

本次 DatePicker 语义部件改造涉及四处可能影响渲染的变更，需要重点确认：

1. 共享资产 `InfoPickerInputTheme.axaml` 新增 6 处 inert marker，并把 `ContentLeftAddOn` 改为
   `AddOnContentPresenter` 投影节点承载（与 Select 家族同构）。单选 DatePicker、TimePicker、
   RangeTimePicker 的触发区都走该模板，需重点回归。
2. 共享资产 `PickerClearUpButtonTheme.axaml` 的清除按钮新增 `semantic-clear` inert marker。
3. 共享资产 `InfoPickerInputTheme.axaml` 为 `PART_InfoInputBox` 新增按档 `MaxHeight`
   （Large/Middle/Small 三档，Custom 不限）；带超大语义 Padding 或超大字体的输入会被裁剪对齐，
   正常档位视觉不变。
4. `RangeDatePickerTheme.axaml`（Range 自有模板）新增 7 处 marker 与同样的 prefix 投影节点；
   带 prefix 的 RangeDatePicker 需重点回归。

对外开放的语义部件：`DatePicker` 12 个（`root`、`prefix`、`input`、`suffix`、`clear`、`popup.root`、
`popup.container`、`popup.header`、`popup.body`、`popup.content`、`popup.cell`、`popup.footer`）；
`RangeDatePicker` 13 个（另加 `secondaryInput`；`popup.content` 在双月布局下为 2 个实例）。
Semantic Parts 页签中的预览为 RangeDatePicker：预选当前日期起 16 天的范围、带文字 prefix，
弹层钉住常开，双月日历、两端输入、清除按钮与后缀同时可见。

## 操作路径与判定标准

准备：运行 AtomUIGallery（`controlgallery/AtomUIGallery.Desktop`），进入
Data Entry（数据录入）→ DatePicker 页面。

### 步骤 1：Semantic Parts 页签

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 1.1 | 点击页面顶部 "Semantic Parts" 页签 | 页面切换到语义预览：出现一个带 prefix 文字的 RangeDatePicker，两端已选日期，预览舞台足够高，双月弹层在输入框下方展开，不遮挡页面 Header 与 Tabs；右侧部件列表可滚动到底，popup.cell / popup.footer 均可滚出 | 全景 |
| 1.2 | 在预览右侧部件列表中逐个选中 13 个 Part（root / prefix / input / secondaryInput / suffix / clear / popup.root / popup.container / popup.header / popup.body / popup.content / popup.cell / popup.footer） | 每个 Part 都有对应高亮框；`popup.*` 在钉住弹层上高亮（`popup.content` 命中两张月表）；`input` / `secondaryInput` 分别命中起止输入框 | 逐部件或分组 |

### 步骤 2：Custom Semantic Part styling 示例

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 2.1 | 滚动到 "自定义语义部件样式" 示例 | 出现上方单选 DatePicker（Object styles）与下方 Filled 变体 RangeDatePicker（function styles）；示例卡片 Tag 显示当前版本号（如 `v6.1.7`），不再是 `v6.0.8` / `v6.0.7` | 全景 |
| 2.2 | 观察单选 DatePicker | 选中日期文字为斜体；prefix 图标为 `#1890FF` 蓝色；后缀清除/箭头区域前景为蓝色 | 局部 |
| 2.3 | 点击打开单选 DatePicker 弹层 | 弹层盒子边框为 `#1890FF` 蓝色 1px | 弹层展开 |
| 2.4 | 观察下方 RangeDatePicker | 触发框边框为 `#722ED1` 紫色；两端输入文字为斜体；打开弹层后弹层外框为紫色 1px 且浮动箭头自动隐藏（带边框弹层呈现为干净面板）、日期格子前景为紫色、底部按钮区边框为紫色；Filled 变体灰底铺满 | 弹层展开 |

### 步骤 3：Examples 回归抽查（共享资产影响面）

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 3.1 | 回到 "Examples" 页签，走查基本用法（五种 PickerMode）、范围选择、尺寸（Large/Middle/Small/Custom）与状态示例 | 触发区外观与改造前一致：高度档位、字号、占位符、清除按钮、后缀均正常，无错位或丢失 | 全景 |
| 3.2 | 打开任一 DatePicker 弹层 | 日历面板渲染正常：头部导航、日期格子、范围/选中视觉、底部 Now/Today/Confirm 按钮无异常 | 弹层展开 |
| 3.3 | 打开带时间的 RangeDatePicker（NeedConfirm 示例中 `IsShowTime=True`） | 单月+时钟布局正常；双月示例仍为双月 | 弹层展开 |
| 3.4 | 进入 TimePicker 页面，走查基本用法与带清除的示例 | TimePicker 触发区视觉与改造前一致（共享模板 marker 为 inert；prefix 投影不影响无 prefix 场景）；RangeTimePicker 同 | 全景 |

## 判定标准

- 步骤 1.1：预览弹层钉住常开、向下展开且不遮挡 Header/Tabs → pinned-open、light-dismiss 抑制与预览舞台高度生效。
- 步骤 1.2：13 个 Part 均可解析高亮，尤其跨视觉根的 `popup.*` 与运行时注入的 `popup.cell` →
  marker 注入与路由正确。
- 步骤 2：语义样式命中（斜体日期、prefix 蓝、后缀蓝、弹层蓝框；触发框/弹层紫框、斜体输入、紫色格子）与 Tag 显示
  当前版本 → 生成 `DatePicker*Style` / `RangeDatePicker*Style` 跨模板/跨视觉根命中目标。
- 步骤 3：共享 `InfoPickerInputTheme` / `PickerClearUpButtonTheme` / `AddOnDecoratedBoxTheme` 的
  inert marker、prefix 投影与输入框 MaxHeight 未改变 DatePicker / TimePicker / RangeTimePicker
  既有视觉。

## 证据要求

按上表回传截图或录屏。图像不入库：验收完成后仓库只保留本文字步骤与结论，不保存截图。
