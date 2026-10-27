# Tab 最终候选布局与限定自然差异审查（已批准）

本表仅申请所示布局修复与N01–N16这些精确帧对；不设置通用容差，不扩大以前的批准，不包含Git。

## 场景和原始结果

Mac arm64、Headless/Skia、scale1、产品Window1500×1100；四列依次为TabControl、CardTabControl、TabStrip、CardTabStrip，四行为Top、Bottom、Left、Right。每控件310×190、12条目、SelectedIndex1、添加按钮开启、原overflow/裁剪和motion保持。main/aa/dip为独立实例，dip只改变外部左距1 DIP。完整历史为default→Global→Control Global→CardSize56→明确CardPadding3,4,5,6→CardSize0→TitleFontSize22→选中末项并滚动→方向互换→reset；每次覆盖最长Transition且至少700ms。

旧源为278429b50；最终候选为34字段V2、一条CardPadding依赖及两个Card模板尺寸绑定修复。自然最终候选60帧：20组A/A有4组非零，20组1 DIP全部检出，6组恢复均非零。自然旧新60对：24对为下述预期布局变化，兼容目标36对中30对为0，余下6对reset列入本表。原始FAIL及精确计数保持。

独立直接绘制保留相同配置与操作历史，另增加一次RenderTargetBitmap绘制，不代替自然合成器结果：候选20组A/A与6组恢复均0，20组1 DIP全部检出；旧新60对中36个兼容目标均0，另外24对保留预期布局差异。未证明每次自然合成历史差异的内部成因，不把直接绘制0写成原始FAIL消失。

## 所示布局变化

- CardSize56的有效CardPadding由固定16,8变为原公式的16,16；CardSize0得到16,-12，有限负padding合同保持。只影响原有Middle水平Card分支，不新增TitleFontSize依赖或钳制。
- 明确CardPadding3,4,5,6时，添加按钮高度跟随实际header，原先的历史残留不再把按钮与header拉成不同高度；恢复默认及滚动历史同样同步。独立Top/Bottom反例的header最终29，旧按钮35/51，候选均29。
- 四个发生变化的阶段为size56、explicit、size0、scroll，共明暗×三个分支×四阶段=24对。其余默认、Global、Control Global、title22、方向互换、reset属于兼容目标。
- 生成专用Add Style的91×37覆盖及撤销class、三尺寸、四方向、重挂与模板替换已验证；直接原生Styles.Remove残留是另一个尚未闭合的CT25失败，不在本表豁免。

## 精确16对

下列坐标为整图物理像素的半开包围框；“最大通道差”是记录值，不是阈值。每对同时以完整before/after PNG的SHA256冻结，仅限原文件的精确差异。

| ID | 分组 | before → after | 像素 | 最大通道差 | 包围框 |
| --- | --- | --- | ---: | ---: | --- |
| N01 | AA | fixed-light-main-scroll -> fixed-light-aa-scroll | 109 | 3 | [43, 81, 1080, 833] |
| N02 | AA | fixed-light-main-reset -> fixed-light-aa-reset | 45 | 1 | [191, 80, 1080, 928] |
| N03 | old-fixed | old-light-main-reset -> fixed-light-main-reset | 43 | 1 | [199, 80, 1080, 928] |
| N04 | old-fixed | old-light-aa-reset -> fixed-light-aa-reset | 60 | 1 | [183, 81, 1076, 871] |
| N05 | old-fixed | old-light-dip-reset -> fixed-light-dip-reset | 39 | 2 | [192, 81, 1077, 896] |
| N06 | restore | fixed-light-main-default -> fixed-light-main-reset | 114 | 3 | [183, 81, 1131, 928] |
| N07 | restore | fixed-light-aa-default -> fixed-light-aa-reset | 131 | 3 | [183, 80, 1131, 872] |
| N08 | restore | fixed-light-dip-default -> fixed-light-dip-reset | 147 | 3 | [184, 80, 1132, 872] |
| N09 | AA | fixed-dark-main-scroll -> fixed-dark-aa-scroll | 238 | 3 | [31, 79, 1080, 863] |
| N10 | AA | fixed-dark-main-reset -> fixed-dark-aa-reset | 159 | 3 | [186, 79, 1132, 872] |
| N11 | old-fixed | old-dark-main-reset -> fixed-dark-main-reset | 143 | 3 | [185, 79, 1132, 871] |
| N12 | old-fixed | old-dark-aa-reset -> fixed-dark-aa-reset | 65 | 2 | [185, 79, 1077, 871] |
| N13 | old-fixed | old-dark-dip-reset -> fixed-dark-dip-reset | 134 | 3 | [186, 79, 1132, 930] |
| N14 | restore | fixed-dark-main-default -> fixed-dark-main-reset | 157 | 3 | [180, 79, 1132, 870] |
| N15 | restore | fixed-dark-aa-default -> fixed-dark-aa-reset | 270 | 3 | [180, 79, 1131, 872] |
| N16 | restore | fixed-dark-dip-default -> fixed-dark-dip-reset | 222 | 3 | [181, 79, 1132, 871] |

## 审查边界

自然差异位于文字及部分卡片item圆角边缘，最大单通道1–3；叠加图复核修正了最初仅描述为文字/图标边缘的位置说明，表内帧对、坐标、计数及SHA均未改变。没有采用缩放、对齐、裁剪、遮罩、容差或产品强制重绘。精确数值与帧SHA冻结于`.superpowers/sdd/container-tabs-audit/fixed-limited-natural-differences.json`；不扩展到其他运行、其他配置、其他Host、原生最终显示面或完整家族。

用户于2026-10-27明确批准“所示布局修复及N01–N16精确差异”，仅覆盖本表冻结的对象，不包含Git、直接Styles.Remove残留、其他平台或完整家族验收。原始FAIL和计数保持；CT14/CT25没有关闭。媒体清理及最终模块结果见[本机切片结果](container-tabs-defaults-results.md)。
