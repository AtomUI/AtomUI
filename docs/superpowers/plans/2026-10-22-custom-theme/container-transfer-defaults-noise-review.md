# Transfer 默认迁移：已接受的限定自然合成差异

用户于2026-10-26明确接受本表34对限定差异（“接受表内34对限定差异，继续验收”）。仅覆盖以下既有采样，来源为[Transfer N3-TR](container-transfer-defaults.md)。不修改比较器、不使用遮罩／对齐／通用容差；原始失败计数保留。批准只适用本文列出的Host、版本、历史、离散坐标、每对像素数及单通道上限，不适用于其他控件、平台、尺寸、输入、重放历史或原生最终显示面。

## Host、源码与历史

证据边界补充：本表历史中的 TreeTransfer 分页阶段由 internal IsPaginationEnabled 驱动，属于既有模板分支诊断，不代表公开分页能力。批准仍只针对原有34对像素；独立公开API补充不沿用本表容差。

Mac arm64、Avalonia Headless/Skia真实合成器，actual AtomUI Window，scale1，1840×620，en-US，默认产品字体。ListTransfer／TreeTransfer各Width760、Height320，独立TransferListView Width240／Height320；数据18项与parent/child树，初始目标item-1与child、选中item-2。每个版本/明暗/轨迹独立Window，main与AA同历史；DIP轨迹只让外层测试Panel左距增加1DIP。完整阶段default→global→control→own→page→selected→reset，正常motion，最小650ms及实际最长Transition后要求三连续同帧、上限4s。首轮基础Avalonia Window缺完整主题的22帧不属于此表，保留为准备失败。

Global为ControlHeightLG56、LineWidth3、FontHeight21、PaddingSM24、MarginXXS7；Control阶段分别List高度56／内距24、Tree高度62／内距26及分页MarginXXS7。Own逐字段值、分页310/330、选择item-3／parent及完整复位在本轮数值记录中固定。HeaderHeight0保留为明确配置，未通过改模板/关闭motion消差。原版3Token为HEAD204c44149中的V1，候选仅11默认函数V2；全字段/类型/key枚举/slot明暗22行相同。

`diag-*`为独立只读诊断：同一完整七阶段历史，在default/page/selected/reset自然采样后增加两次直接Skia绘制，并观测739–907个实际Visual的资源、字体、矩阵、画刷及状态。其自然历史与正式采样分别列名，不互相替代。

## 判断边界

正式84帧：28组1DIP全检出；24/28 AA为0，38/42新旧为0；12恢复仍有原版/候选共有47（明）或39（暗）像素差异，单通道1/2。独立直接Skia32组AA、16恢复、32新旧全0；诊断自然AA及其余新旧/恢复另有14对非零，全部列入本表，未丢弃任何已比较的失败。

42组正式资源/主要几何/targetKeys/selectedKeys一致。完整诊断28组匹配历史新旧与28组AA，除两处WaveSpirit的LastWaveSize/LastWaveOpacity残余数值外，其余观测相同；这些残余位于树侧(764,171)/(790,193)及恢复后的(773,183)/(790,216)，与本表非零像素不相交，不能因此声称整棵绘制状态完全相同，也未证明每一个历史合成帧的内部原因。

恢复时分页创建的子树由739增长到907节点，两版一致；直接按数组序号比较初始/恢复树会因懒创建节点插入而错配，不能用作恢复全树相等证据。已单独核对非零区域对应CheckboxIndicator、PixelAlignedBorder、mark的几何/选中/画刷/矩阵相同；真实差异仅在已列checkbox与disabled按钮边缘，仍需本次限定审查。原生managed/AOT各684项资源/重挂检查通过，不是原生像素证明。

## 每对限定

所有输入/输出alpha均为255且不变；单通道限值针对RGBA绝对差。每个P集合是离散像素坐标全集，不将其包围框当作容差区域。

| ID | 类别 | before → after | 坐标集合 | 像素数上限 | 单通道上限 |
| --- | --- | --- | --- | ---: | ---: |
| TR01 | restore | `old-light-main-default.png` → `old-light-main-reset.png` | P01 | 47 | 1 |
| TR02 | restore | `old-light-aa-default.png` → `old-light-aa-reset.png` | P01 | 47 | 1 |
| TR03 | restore | `old-light-dip-default.png` → `old-light-dip-reset.png` | P02 | 47 | 1 |
| TR04 | AA | `old-dark-main-page.png` → `old-dark-aa-page.png` | P03 | 4 | 3 |
| TR05 | restore | `old-dark-main-default.png` → `old-dark-main-reset.png` | P04 | 39 | 2 |
| TR06 | restore | `old-dark-aa-default.png` → `old-dark-aa-reset.png` | P04 | 39 | 2 |
| TR07 | restore | `old-dark-dip-default.png` → `old-dark-dip-reset.png` | P05 | 39 | 2 |
| TR08 | AA | `new-light-main-page.png` → `new-light-aa-page.png` | P03 | 4 | 4 |
| TR09 | restore | `new-light-main-default.png` → `new-light-main-reset.png` | P01 | 47 | 1 |
| TR10 | restore | `new-light-aa-default.png` → `new-light-aa-reset.png` | P01 | 47 | 1 |
| TR11 | restore | `new-light-dip-default.png` → `new-light-dip-reset.png` | P02 | 47 | 1 |
| TR12 | AA | `new-dark-main-selected.png` → `new-dark-aa-selected.png` | P06 | 20 | 2 |
| TR13 | AA | `new-dark-main-reset.png` → `new-dark-aa-reset.png` | P07 | 4 | 3 |
| TR14 | restore | `new-dark-main-default.png` → `new-dark-main-reset.png` | P04 | 39 | 2 |
| TR15 | restore | `new-dark-aa-default.png` → `new-dark-aa-reset.png` | P04 | 39 | 2 |
| TR16 | restore | `new-dark-dip-default.png` → `new-dark-dip-reset.png` | P05 | 39 | 2 |
| TR17 | oldNew | `old-light-main-page.png` → `new-light-main-page.png` | P03 | 4 | 4 |
| TR18 | oldNew | `old-dark-main-selected.png` → `new-dark-main-selected.png` | P06 | 20 | 2 |
| TR19 | oldNew | `old-dark-aa-page.png` → `new-dark-aa-page.png` | P03 | 4 | 3 |
| TR20 | oldNew | `old-dark-aa-reset.png` → `new-dark-aa-reset.png` | P07 | 4 | 3 |
| TR21 | naturalDiagAA | `diag-old-light-main-selected-natural.png` → `diag-old-light-aa-selected-natural.png` | P08 | 18 | 4 |
| TR22 | naturalDiagAA | `diag-old-light-main-reset-natural.png` → `diag-old-light-aa-reset-natural.png` | P07 | 4 | 4 |
| TR23 | naturalDiagAA | `diag-new-light-main-selected-natural.png` → `diag-new-light-aa-selected-natural.png` | P08 | 18 | 4 |
| TR24 | diagnosticOldNew | `diag-old-light-main-selected-natural.png` → `diag-new-light-main-selected-natural.png` | P08 | 18 | 4 |
| TR25 | diagnosticRestore | `diag-old-light-main-default-natural.png` → `diag-old-light-main-reset-natural.png` | P01 | 47 | 1 |
| TR26 | diagnosticRestore | `diag-new-light-main-default-natural.png` → `diag-new-light-main-reset-natural.png` | P01 | 47 | 1 |
| TR27 | diagnosticOldNew | `diag-old-light-aa-selected-natural.png` → `diag-new-light-aa-selected-natural.png` | P08 | 18 | 4 |
| TR28 | diagnosticOldNew | `diag-old-light-aa-reset-natural.png` → `diag-new-light-aa-reset-natural.png` | P07 | 4 | 4 |
| TR29 | diagnosticRestore | `diag-old-light-aa-default-natural.png` → `diag-old-light-aa-reset-natural.png` | P09 | 51 | 4 |
| TR30 | diagnosticRestore | `diag-new-light-aa-default-natural.png` → `diag-new-light-aa-reset-natural.png` | P01 | 47 | 1 |
| TR31 | diagnosticRestore | `diag-old-dark-main-default-natural.png` → `diag-old-dark-main-reset-natural.png` | P04 | 39 | 2 |
| TR32 | diagnosticRestore | `diag-new-dark-main-default-natural.png` → `diag-new-dark-main-reset-natural.png` | P04 | 39 | 2 |
| TR33 | diagnosticRestore | `diag-old-dark-aa-default-natural.png` → `diag-old-dark-aa-reset-natural.png` | P04 | 39 | 2 |
| TR34 | diagnosticRestore | `diag-new-dark-aa-default-natural.png` → `diag-new-dark-aa-reset-natural.png` | P04 | 39 | 2 |

## 离散坐标集合

以下均为整图物理像素，原点左上；同一行列出对应x值，除此之外不接受差异。

### P01（47点）

y=251: x=15,17,24,26；y=252: x=14,15,16,25,27；y=253: x=13,14,28；y=254: x=13,14,27,28；y=255: x=13,28；y=262: x=13,28；y=263: x=13,14,27,28；y=264: x=13,14,28；y=265: x=14,15,16,25,27；y=266: x=15,16,17,24,25,26,951,953,969,971；y=267: x=950,951,952,970,972。

### P02（47点）

y=251: x=16,18,25,27；y=252: x=15,16,17,26,28；y=253: x=14,15,29；y=254: x=14,15,28,29；y=255: x=14,29；y=262: x=14,29；y=263: x=14,15,28,29；y=264: x=14,15,29；y=265: x=15,16,17,26,28；y=266: x=16,17,18,25,26,27,952,954,970,972；y=267: x=951,952,953,971,973。

### P03（4点）

y=212: x=26；y=213: x=27；y=224: x=27；y=225: x=26。

### P04（39点）

y=251: x=16,17,24,25；y=252: x=14,15,16,25,26,27；y=253: x=14,27；y=254: x=13,14,27；y=255: x=13,28；y=262: x=13,28；y=263: x=13,14,27；y=264: x=14,27；y=265: x=14,15,16,25,26,27；y=266: x=17,24,25,951,969,970,971；y=267: x=950,951。

### P05（39点）

y=251: x=17,18,25,26；y=252: x=15,16,17,26,27,28；y=253: x=15,28；y=254: x=14,15,28；y=255: x=14,29；y=262: x=14,29；y=263: x=14,15,28；y=264: x=15,28；y=265: x=15,16,17,26,27,28；y=266: x=18,25,26,952,970,971,972；y=267: x=951,952。

### P06（20点）

y=244: x=14,15,16,25,26,27；y=245: x=14,27；y=246: x=14,27；y=255: x=14,27；y=256: x=14,27；y=257: x=14,15,16,25,26,27。

### P07（4点）

y=252: x=26；y=253: x=27；y=264: x=27；y=265: x=26。

### P08（18点）

y=244: x=14,15,16,25,27；y=245: x=14；y=246: x=14,27；y=255: x=14,27；y=256: x=14,27；y=257: x=14,15,16,25,26,27。

### P09（51点）

y=251: x=15,17,24,26；y=252: x=14,15,16,25,26,27；y=253: x=13,14,27,28；y=254: x=13,14,27,28；y=255: x=13,28；y=262: x=13,28；y=263: x=13,14,27,28；y=264: x=13,14,27,28；y=265: x=14,15,16,25,26,27；y=266: x=15,16,17,24,25,26,951,953,969,971；y=267: x=950,951,952,970,972。

本次接受只覆盖限定视觉差异，不含Git授权或完整家族验收。203张场景／差异PNG与manifest共204文件已清理；本文保留数值与实际观察，不再引用已删除图片。对本轮历史重复运行、其他位置出现新差异或超过任何一行上限，仍必须失败并另行定位。
