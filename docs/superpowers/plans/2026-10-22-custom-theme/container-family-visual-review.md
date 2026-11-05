# CT14 家族最终限定视觉审查

状态：用户已明确批准本表 V1–V7 的限定范围，保留 CT25/FW1。批准仅覆盖本表指定场景；不包含提交、依赖替换、其他平台或通用像素容差。CT12/CT13既定批准及Cascader Own高度上限/页数预算合同保持。

## 场景与版本

HEAD为4560091de，基线包含会话开始时已有未提交改动。本轮78个产品文件初始/候选hash已固定；在现有目录和分支锁定回放，旧、新轨迹结束后候选逐文件hash精确恢复，没有新分支/worktree/隔离checkout。原版480、候选v1 480；Small Pagination的意外间距变化已否决并恢复0，最终v2另480。此前96默认帧作为预检保留，不冒充最终矩阵。

实际Headless/Skia、非HeadlessDrawing，1100×720、scale1、真实应用明暗，main/A/A/外部左距1DIP分别独立实例，完整相同历史default→compact→configured→disabled→reset。适用尺寸档、Nav三模式；Tab保留310×190、12条目、SelectedIndex1、添加按钮、四方向及overflow。非尺寸接口的Expander在本矩阵为默认方向，其原40项及原N3四方向证据保持；不把本矩阵声称为每个控件的全排列。保留高度/裁剪，正常motion，至少700ms并连续三同帧。

## 已批准的功能/外观范围

| 项 | 具体前后合同 | 限定覆盖 |
|---|---|---|
| V1 | ListBox选中标记由未命中的IconPresenter选择器修为真实IconTemplatePresenter；标记可见、读ColorPrimary/IconSize/SelectedIndicatorMargin | 明暗四档及compact，过滤/选择/禁用；根尺寸/viewport不降级 |
| V2 | disabled过滤正文由普通ItemColor改Own ItemDisabledColor；高亮色独立保留 | 明暗普通及显式禁用色；匹配文字/数据/高度不减少 |
| V3 | 可选字体族/根字号与219处Global消费接通实际owner；模板输入保持Template优先级、公开派生身份不回落基类 | 本表FontFamily=monospace、FontSize18与主题/Own配置、撤回；不批准所有外部字体/配置 |
| V4 | Menu顶层normal读取TopLevelItemColor/TopLevelItemBg；打开态读取对应Selected字段，disabled前景优先 | 四档、明暗、同实例打开/关闭；默认Bg从透明改有效ColorBgContainer，因此有色父背景场景应使用Own透明覆盖保留旧透出效果 |
| V5 | Nav横向浅色hover/selected读取专属Own，horizontal/subMenu圆角分工；顶层popup读取Own间距 | 三模式、明暗窗口，配置圆角9/间距14与selected底色；default水平gap由4到8，配置gap14 |
| V6 | Nav背景transition由Linear改声明的CSS ease，Own easing在Control owner实例热更新并复制；Foreground/ActiveBar曲线不改 | 实际transition消费者及当前Mac managed/AOT两次曲线更新；静态矩阵不代替其他连续像素时间线 |
| V7 | Custom退出预设activator，原基础值与S/M/L保持；Small Pagination保留0间距、Custom基础8 | 17分支及精确前后失败/通过；不批准意外Small间距8版本 |

## 原始逐帧比较

最终480旧新对照245零差异、235非零；非零包含上述定制生效与明确默认修复，不能统称噪声。默认/compact非零家族仅ListBox及Dark Menu；前者对应V1/V2，后者对应V4的opaque surface/文字边缘，9对为104–138点、最大通道1，逐对见下表。所有1DIP正对照旧160/新160均检出。

新A/A160对151零、9非零（最多19点）；旧A/A156零、4非零（最多7点）。旧恢复96对69零、27非零；新恢复61零、35非零，最多92点。原始FAIL全部保留，不设置百分比/区域遮罩/缩放对齐/通用容差。

TabStrip复位旧新6对各60点、最大通道Light233/Dark200，位于Small左侧布局指示线；根bounds、viewport、extent、offset、字号/字体记录一致，现有GetBaseValue/动画基准恢复路径仍依赖官方12.1.4。此项不作为已修复或像素通过，继续FW1。Nav复位8/13点及其余自然边缘不扩展既有批准，内部每次原因未证明；具体帧列出供限定审阅，CT25保持开放。

## 精确限定帧对

| kind | A | B | pixels | max channel | bbox |
|---|---|---|---:|---:|---|
| old-new | matrix-old-dark-CardTabControl-aa-reset.png | matrix-v2-dark-CardTabControl-aa-reset.png | 1 | 2 | [149, 530, 150, 531] |
| old-new | matrix-old-dark-CardTabControl-main-reset.png | matrix-v2-dark-CardTabControl-main-reset.png | 1 | 2 | [149, 530, 150, 531] |
| old-new | matrix-old-dark-CardTabStrip-aa-reset.png | matrix-v2-dark-CardTabStrip-aa-reset.png | 5 | 1 | [199, 78, 721, 110] |
| old-new | matrix-old-dark-CardTabStrip-dip-reset.png | matrix-v2-dark-CardTabStrip-dip-reset.png | 5 | 1 | [200, 78, 722, 110] |
| old-new | matrix-old-dark-Menu-aa-compact.png | matrix-v2-dark-Menu-aa-compact.png | 137 | 1 | [40, 77, 624, 416] |
| old-new | matrix-old-dark-Menu-aa-default.png | matrix-v2-dark-Menu-aa-default.png | 104 | 1 | [40, 77, 640, 420] |
| old-new | matrix-old-dark-Menu-aa-reset.png | matrix-v2-dark-Menu-aa-reset.png | 104 | 1 | [40, 77, 640, 420] |
| old-new | matrix-old-dark-Menu-dip-compact.png | matrix-v2-dark-Menu-dip-compact.png | 138 | 1 | [41, 77, 625, 416] |
| old-new | matrix-old-dark-Menu-dip-default.png | matrix-v2-dark-Menu-dip-default.png | 104 | 1 | [41, 77, 641, 420] |
| old-new | matrix-old-dark-Menu-dip-reset.png | matrix-v2-dark-Menu-dip-reset.png | 104 | 1 | [41, 77, 641, 420] |
| old-new | matrix-old-dark-Menu-main-compact.png | matrix-v2-dark-Menu-main-compact.png | 137 | 1 | [40, 77, 624, 416] |
| old-new | matrix-old-dark-Menu-main-default.png | matrix-v2-dark-Menu-main-default.png | 104 | 1 | [40, 77, 640, 420] |
| old-new | matrix-old-dark-Menu-main-reset.png | matrix-v2-dark-Menu-main-reset.png | 104 | 1 | [40, 77, 640, 420] |
| old-new | matrix-old-dark-NavMenu-aa-reset.png | matrix-v2-dark-NavMenu-aa-reset.png | 13 | 1 | [558, 66, 564, 74] |
| old-new | matrix-old-dark-NavMenu-dip-reset.png | matrix-v2-dark-NavMenu-dip-reset.png | 13 | 1 | [559, 66, 565, 74] |
| old-new | matrix-old-dark-NavMenu-main-reset.png | matrix-v2-dark-NavMenu-main-reset.png | 13 | 1 | [558, 66, 564, 74] |
| old-new | matrix-old-dark-TabStrip-aa-reset.png | matrix-v2-dark-TabStrip-aa-reset.png | 60 | 200 | [205, 265, 264, 267] |
| old-new | matrix-old-dark-TabStrip-dip-reset.png | matrix-v2-dark-TabStrip-dip-reset.png | 60 | 200 | [206, 265, 265, 267] |
| old-new | matrix-old-dark-TabStrip-main-reset.png | matrix-v2-dark-TabStrip-main-reset.png | 60 | 200 | [205, 265, 264, 267] |
| old-new | matrix-old-light-NavMenu-aa-reset.png | matrix-v2-light-NavMenu-aa-reset.png | 8 | 1 | [559, 66, 564, 73] |
| old-new | matrix-old-light-NavMenu-dip-reset.png | matrix-v2-light-NavMenu-dip-reset.png | 8 | 1 | [560, 66, 565, 73] |
| old-new | matrix-old-light-NavMenu-main-reset.png | matrix-v2-light-NavMenu-main-reset.png | 8 | 1 | [559, 66, 564, 73] |
| old-new | matrix-old-light-TabStrip-aa-reset.png | matrix-v2-light-TabStrip-aa-reset.png | 60 | 233 | [205, 265, 264, 267] |
| old-new | matrix-old-light-TabStrip-dip-reset.png | matrix-v2-light-TabStrip-dip-reset.png | 60 | 233 | [206, 265, 265, 267] |
| old-new | matrix-old-light-TabStrip-main-reset.png | matrix-v2-light-TabStrip-main-reset.png | 60 | 233 | [205, 265, 264, 267] |
| matrix-v2-aa | matrix-v2-dark-CardTabStrip-main-disabled.png | matrix-v2-dark-CardTabStrip-aa-disabled.png | 3 | 1 | [209, 78, 217, 83] |
| matrix-v2-aa | matrix-v2-dark-CardTabStrip-main-reset.png | matrix-v2-dark-CardTabStrip-aa-reset.png | 5 | 1 | [199, 78, 721, 110] |
| matrix-v2-aa | matrix-v2-dark-Menu-main-configured.png | matrix-v2-dark-Menu-aa-configured.png | 19 | 20 | [97, 80, 608, 417] |
| matrix-v2-aa | matrix-v2-dark-Menu-main-disabled.png | matrix-v2-dark-Menu-aa-disabled.png | 7 | 22 | [81, 82, 608, 394] |
| matrix-v2-aa | matrix-v2-dark-TabControl-main-configured.png | matrix-v2-dark-TabControl-aa-configured.png | 2 | 1 | [382, 91, 411, 105] |
| matrix-v2-aa | matrix-v2-dark-TabStrip-main-configured.png | matrix-v2-dark-TabStrip-aa-configured.png | 3 | 1 | [69, 91, 590, 94] |
| matrix-v2-aa | matrix-v2-light-Menu-main-configured.png | matrix-v2-light-Menu-aa-configured.png | 7 | 2 | [81, 82, 608, 394] |
| matrix-v2-aa | matrix-v2-light-TabControl-main-configured.png | matrix-v2-light-TabControl-aa-configured.png | 2 | 1 | [410, 91, 909, 103] |
| matrix-v2-aa | matrix-v2-light-TabStrip-main-configured.png | matrix-v2-light-TabStrip-aa-configured.png | 7 | 1 | [66, 90, 588, 104] |
| matrix-v2-restore | matrix-v2-dark-CardTabControl-aa-default.png | matrix-v2-dark-CardTabControl-aa-reset.png | 1 | 2 | [149, 530, 150, 531] |
| matrix-v2-restore | matrix-v2-dark-CardTabControl-main-default.png | matrix-v2-dark-CardTabControl-main-reset.png | 1 | 2 | [149, 530, 150, 531] |
| matrix-v2-restore | matrix-v2-dark-CardTabStrip-aa-default.png | matrix-v2-dark-CardTabStrip-aa-reset.png | 2 | 1 | [94, 76, 96, 77] |
| matrix-v2-restore | matrix-v2-dark-CardTabStrip-dip-default.png | matrix-v2-dark-CardTabStrip-dip-reset.png | 2 | 1 | [95, 76, 97, 77] |
| matrix-v2-restore | matrix-v2-dark-CardTabStrip-main-default.png | matrix-v2-dark-CardTabStrip-main-reset.png | 7 | 1 | [94, 76, 721, 110] |
| matrix-v2-restore | matrix-v2-dark-ListTransfer-aa-default.png | matrix-v2-dark-ListTransfer-aa-reset.png | 92 | 2 | [53, 286, 590, 613] |
| matrix-v2-restore | matrix-v2-dark-ListTransfer-dip-default.png | matrix-v2-dark-ListTransfer-dip-reset.png | 92 | 2 | [54, 286, 591, 613] |
| matrix-v2-restore | matrix-v2-dark-ListTransfer-main-default.png | matrix-v2-dark-ListTransfer-main-reset.png | 92 | 2 | [53, 286, 590, 613] |
| matrix-v2-restore | matrix-v2-dark-NavMenu-aa-default.png | matrix-v2-dark-NavMenu-aa-reset.png | 13 | 1 | [558, 66, 564, 74] |
| matrix-v2-restore | matrix-v2-dark-NavMenu-dip-default.png | matrix-v2-dark-NavMenu-dip-reset.png | 13 | 1 | [559, 66, 565, 74] |
| matrix-v2-restore | matrix-v2-dark-NavMenu-main-default.png | matrix-v2-dark-NavMenu-main-reset.png | 13 | 1 | [558, 66, 564, 74] |
| matrix-v2-restore | matrix-v2-dark-Segmented-aa-default.png | matrix-v2-dark-Segmented-aa-reset.png | 1 | 4 | [61, 408, 62, 409] |
| matrix-v2-restore | matrix-v2-dark-Segmented-dip-default.png | matrix-v2-dark-Segmented-dip-reset.png | 1 | 4 | [62, 408, 63, 409] |
| matrix-v2-restore | matrix-v2-dark-Segmented-main-default.png | matrix-v2-dark-Segmented-main-reset.png | 1 | 4 | [61, 408, 62, 409] |
| matrix-v2-restore | matrix-v2-dark-TabStrip-aa-default.png | matrix-v2-dark-TabStrip-aa-reset.png | 76 | 200 | [20, 265, 264, 385] |
| matrix-v2-restore | matrix-v2-dark-TabStrip-dip-default.png | matrix-v2-dark-TabStrip-dip-reset.png | 76 | 200 | [21, 265, 265, 385] |
| matrix-v2-restore | matrix-v2-dark-TabStrip-main-default.png | matrix-v2-dark-TabStrip-main-reset.png | 76 | 200 | [20, 265, 264, 385] |
| matrix-v2-restore | matrix-v2-light-CardTabStrip-aa-default.png | matrix-v2-light-CardTabStrip-aa-reset.png | 2 | 1 | [95, 76, 207, 82] |
| matrix-v2-restore | matrix-v2-light-CardTabStrip-dip-default.png | matrix-v2-light-CardTabStrip-dip-reset.png | 2 | 1 | [96, 76, 208, 82] |
| matrix-v2-restore | matrix-v2-light-CardTabStrip-main-default.png | matrix-v2-light-CardTabStrip-main-reset.png | 2 | 1 | [95, 76, 207, 82] |
| matrix-v2-restore | matrix-v2-light-ListTransfer-aa-default.png | matrix-v2-light-ListTransfer-aa-reset.png | 36 | 1 | [53, 286, 590, 613] |
| matrix-v2-restore | matrix-v2-light-ListTransfer-dip-default.png | matrix-v2-light-ListTransfer-dip-reset.png | 36 | 1 | [54, 286, 591, 613] |
| matrix-v2-restore | matrix-v2-light-ListTransfer-main-default.png | matrix-v2-light-ListTransfer-main-reset.png | 36 | 1 | [53, 286, 590, 613] |
| matrix-v2-restore | matrix-v2-light-NavMenu-aa-default.png | matrix-v2-light-NavMenu-aa-reset.png | 8 | 1 | [559, 66, 564, 73] |
| matrix-v2-restore | matrix-v2-light-NavMenu-dip-default.png | matrix-v2-light-NavMenu-dip-reset.png | 8 | 1 | [560, 66, 565, 73] |
| matrix-v2-restore | matrix-v2-light-NavMenu-main-default.png | matrix-v2-light-NavMenu-main-reset.png | 8 | 1 | [559, 66, 564, 73] |
| matrix-v2-restore | matrix-v2-light-Pagination-aa-default.png | matrix-v2-light-Pagination-aa-reset.png | 1 | 1 | [120, 76, 121, 77] |
| matrix-v2-restore | matrix-v2-light-Pagination-dip-default.png | matrix-v2-light-Pagination-dip-reset.png | 1 | 1 | [121, 76, 122, 77] |
| matrix-v2-restore | matrix-v2-light-Pagination-main-default.png | matrix-v2-light-Pagination-main-reset.png | 1 | 1 | [120, 76, 121, 77] |
| matrix-v2-restore | matrix-v2-light-Segmented-aa-default.png | matrix-v2-light-Segmented-aa-reset.png | 1 | 1 | [61, 408, 62, 409] |
| matrix-v2-restore | matrix-v2-light-Segmented-dip-default.png | matrix-v2-light-Segmented-dip-reset.png | 1 | 1 | [62, 408, 63, 409] |
| matrix-v2-restore | matrix-v2-light-Segmented-main-default.png | matrix-v2-light-Segmented-main-reset.png | 1 | 1 | [61, 408, 62, 409] |
| matrix-v2-restore | matrix-v2-light-TabStrip-aa-default.png | matrix-v2-light-TabStrip-aa-reset.png | 69 | 233 | [23, 265, 264, 385] |
| matrix-v2-restore | matrix-v2-light-TabStrip-dip-default.png | matrix-v2-light-TabStrip-dip-reset.png | 69 | 233 | [24, 265, 265, 385] |
| matrix-v2-restore | matrix-v2-light-TabStrip-main-default.png | matrix-v2-light-TabStrip-main-reset.png | 69 | 233 | [23, 265, 264, 385] |


## 2026-10-30 最终批准与关闭登记

用户通过 `request_user_input_async / call_656cee76f0f3440cb7dfe8d373826c6d / item 0` 明确回复：**“批准本表限定范围，保留 CT25/FW1”**。本次 V1–V7 限定视觉批准与工程、清理、Desktop1014/Core17、NET8零警告及当前Mac managed556/AOT557证据已对齐，CT14在既定批准范围完成并关闭。

原始像素失败、TabStrip指示线复位、coercion/Style退役及其他既定CT25/FW1边界保持，不将其记为产品修复或平台像素通过，不扩大容差、其他字体/配置/Host/平台。CT12、CT13和Cascader既定高度批准保持；本次批准不授权Git提交、推送、发布或依赖替换。仅同步文字状态，产品源码不变，沿用相同最终输入的已完成验收。
