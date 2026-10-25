# C9／棋盘格：自然合成限定差异审查表（已获用户批准）

本表只申请以下31对实际历史帧的限定审查，不设置比较器容差，不继承C1–C8批准，也不适用于原生最终显示面、其他平台或后续场景。

## 固定环境与历史

Mac arm64、Avalonia12.1.4 Headless/Skia、RenderScaling=1、产品Window ClientSize1200×820，正常动效和region dirty clipping保持。源码基线393d718b38a704a1d29ef6a94168c856b4249188；候选四文件SHA见[结果](color-spectrum-keyboard-results.md)。两类完整公开View分别在Light／Dark下，独立main／aa／dip实例，主根Margin.Left24，DIP正对照25。

完整历史：公开Alpha输入60→61及HEX输入#7BAA62准备 → default（无焦点）→ 外部Before按钮聚焦并固定原始Tab49次／渐变52次 → focus → 显式VS并原始Up → vs-up → 移出焦点、恢复SV、相同公开输入准备 → unfocused → Own tile6 → 整个provider离树改tile10、同实例回树 → detached10 → 清除Own与Components本地值、相同输入准备、无焦点 → reset。Own恢复比较的起点为unfocused。

源前缀old-final／new-final是正式自然合成；old-direct／new-direct是独立即时Skia诊断，不混作自然基线。第一轮旧Alpha初值同显61未发通知的探索轨迹不作为正式兼容比较，原数值保留在文字账本。

## 诊断边界

正式55/56组A/A为0，56/56组1DIP检出；四组旧新默认及四组最终reset均为0。表内只有A/A、相同阶段旧新和同版本Own往返的非零差异；预期焦点／Up／棋盘格变化另列结果，不计作环境噪声。

独立新旧即时Skia的56组A/A、16组同实例Own恢复及16组新旧兼容阶段均为0；32对恢复／新旧兼容阶段的505／516节点观测表完全相同，包含Bounds、矩阵、可见性、裁剪、文字字体／画刷、棋盘标量与Image源Bitmap SHA。这里证明的是所采诊断轨迹，不能证明每一正式历史帧的内部缓存原因。即时渲染的预期焦点、Up与棋盘格修复仍有对应变化；正式自然合成帧保持原样，不用即时渲染替换。

## 每对上限

每行仅允许所列点集、实际变化像素数上限及单通道差值上限；alpha均为255。坐标为完整1200×820图的整数物理像素。X+1只适用于明确标出的dip角色，其余点集不平移。实际比较继续零通用容差并保留原失败计数。

| ID | 比较 | 精确帧对 | 像素上限 | 单通道上限 | 离散点集 |
| --- | --- | --- | ---: | ---: | --- |

| N01 | restore | `old-final-light-plain-main-unfocused.png` → `old-final-light-plain-main-reset.png` | 13 | 2 | C1 |
| N02 | restore | `old-final-light-plain-aa-unfocused.png` → `old-final-light-plain-aa-reset.png` | 13 | 2 | C1 |
| N03 | restore | `old-final-light-plain-dip-unfocused.png` → `old-final-light-plain-dip-reset.png` | 13 | 2 | C1，X+1 |
| N04 | restore | `old-final-light-gradient-main-unfocused.png` → `old-final-light-gradient-main-reset.png` | 27 | 6 | C2 |
| N05 | restore | `old-final-light-gradient-aa-unfocused.png` → `old-final-light-gradient-aa-reset.png` | 27 | 6 | C2 |
| N06 | restore | `old-final-light-gradient-dip-unfocused.png` → `old-final-light-gradient-dip-reset.png` | 27 | 6 | C2，X+1 |
| N07 | restore | `old-final-dark-plain-main-unfocused.png` → `old-final-dark-plain-main-reset.png` | 34 | 2 | C3 |
| N08 | restore | `old-final-dark-plain-aa-unfocused.png` → `old-final-dark-plain-aa-reset.png` | 34 | 2 | C3 |
| N09 | restore | `old-final-dark-plain-dip-unfocused.png` → `old-final-dark-plain-dip-reset.png` | 34 | 2 | C3，X+1 |
| N10 | restore | `old-final-dark-gradient-main-unfocused.png` → `old-final-dark-gradient-main-reset.png` | 61 | 5 | C4 |
| N11 | restore | `old-final-dark-gradient-aa-unfocused.png` → `old-final-dark-gradient-aa-reset.png` | 61 | 5 | C4 |
| N12 | restore | `old-final-dark-gradient-dip-unfocused.png` → `old-final-dark-gradient-dip-reset.png` | 61 | 5 | C4，X+1 |
| N13 | restore | `new-final-light-plain-main-unfocused.png` → `new-final-light-plain-main-reset.png` | 12 | 1 | C5 |
| N14 | restore | `new-final-light-plain-aa-unfocused.png` → `new-final-light-plain-aa-reset.png` | 12 | 1 | C5 |
| N15 | restore | `new-final-light-plain-dip-unfocused.png` → `new-final-light-plain-dip-reset.png` | 12 | 1 | C5，X+1 |
| N16 | restore | `new-final-light-gradient-main-unfocused.png` → `new-final-light-gradient-main-reset.png` | 27 | 6 | C2 |
| N17 | restore | `new-final-light-gradient-aa-unfocused.png` → `new-final-light-gradient-aa-reset.png` | 27 | 6 | C2 |
| N18 | restore | `new-final-light-gradient-dip-unfocused.png` → `new-final-light-gradient-dip-reset.png` | 27 | 6 | C2，X+1 |
| N19 | restore | `new-final-dark-plain-main-unfocused.png` → `new-final-dark-plain-main-reset.png` | 27 | 1 | C6 |
| N20 | restore | `new-final-dark-plain-aa-unfocused.png` → `new-final-dark-plain-aa-reset.png` | 27 | 1 | C6 |
| N21 | restore | `new-final-dark-plain-dip-unfocused.png` → `new-final-dark-plain-dip-reset.png` | 27 | 1 | C6，X+1 |
| N22 | AA | `new-final-dark-gradient-main-vs-up.png` → `new-final-dark-gradient-aa-vs-up.png` | 7 | 1 | C7 |
| N23 | restore | `new-final-dark-gradient-main-unfocused.png` → `new-final-dark-gradient-main-reset.png` | 57 | 5 | C8 |
| N24 | restore | `new-final-dark-gradient-aa-unfocused.png` → `new-final-dark-gradient-aa-reset.png` | 57 | 5 | C8 |
| N25 | restore | `new-final-dark-gradient-dip-unfocused.png` → `new-final-dark-gradient-dip-reset.png` | 57 | 5 | C8，X+1 |
| N26 | old-new | `old-final-light-plain-main-unfocused.png` → `new-final-light-plain-main-unfocused.png` | 1 | 2 | C9 |
| N27 | old-new | `old-final-light-plain-main-tile6.png` → `new-final-light-plain-main-tile6.png` | 1 | 2 | C9 |
| N28 | old-new | `old-final-dark-plain-main-unfocused.png` → `new-final-dark-plain-main-unfocused.png` | 7 | 2 | C10 |
| N29 | old-new | `old-final-dark-plain-main-tile6.png` → `new-final-dark-plain-main-tile6.png` | 7 | 2 | C10 |
| N30 | old-new | `old-final-dark-gradient-main-unfocused.png` → `new-final-dark-gradient-main-unfocused.png` | 5 | 1 | C11 |
| N31 | old-new | `old-final-dark-gradient-main-tile6.png` → `new-final-dark-gradient-main-tile6.png` | 5 | 1 | C11 |

## 离散坐标

### C1

`y=188: x=50,51,52；y=189: x=49,51；y=190: x=49；y=191: x=51,52；y=193: x=49,50；y=267: x=323；y=357: x=52；y=363: x=52`

### C2

`y=95: x=262,524；y=98: x=264,266；y=99: x=258,267；y=100: x=268；y=101: x=268；y=102: x=268；y=103: x=268；y=104: x=267；y=105: x=264,265,266；y=188: x=50,51,52；y=189: x=49,51；y=190: x=49；y=191: x=51,52；y=193: x=49,50；y=287: x=323；y=357: x=52；y=363: x=52`

### C3

`y=102: x=28；y=188: x=51；y=189: x=49,51,52；y=190: x=49,50；y=191: x=48,50,51,52；y=193: x=49,50；y=194: x=51,52；y=195: x=49,50；y=196: x=51,52；y=267: x=323,328；y=268: x=328,330；y=279: x=321；y=280: x=323,328；y=354: x=52；y=356: x=52；y=357: x=52；y=358: x=52；y=363: x=28,29,52；y=364: x=52`

### C4

`y=95: x=262,267,524,529；y=96: x=267,529；y=97: x=522,523；y=98: x=264,265,266；y=99: x=258,267,533；y=100: x=268；y=101: x=268；y=102: x=28,268；y=103: x=268；y=104: x=259；y=105: x=264,265,266；y=106: x=269；y=108: x=262,524；y=188: x=51；y=189: x=49,51,52；y=190: x=49,50；y=191: x=48,50,51,52；y=193: x=49,50；y=194: x=51,52；y=195: x=49,50；y=196: x=51,52；y=287: x=323,328；y=288: x=328,330；y=299: x=321；y=300: x=323,328；y=336: x=322,472；y=354: x=52；y=356: x=52；y=357: x=52；y=358: x=52；y=363: x=28,29,52；y=364: x=52`

### C5

`y=188: x=50,51,52；y=189: x=49,51；y=190: x=49；y=191: x=51,52；y=193: x=49,50；y=357: x=52；y=363: x=52`

### C6

`y=102: x=28；y=188: x=51；y=189: x=49,51,52；y=190: x=49,50；y=191: x=48,50,51,52；y=193: x=49,50；y=194: x=51,52；y=195: x=49,50；y=196: x=51,52；y=354: x=52；y=356: x=52；y=357: x=52；y=358: x=52；y=363: x=28,29,52；y=364: x=52`

### C7

`y=311: x=406；y=312: x=393,406；y=314: x=393,406；y=316: x=393,394`

### C8

`y=95: x=262,267,524,529；y=96: x=267,529；y=97: x=522,523；y=98: x=264,265,266；y=99: x=258,267,533；y=100: x=268；y=101: x=268；y=102: x=28,268；y=103: x=268；y=104: x=259；y=105: x=264,265,266；y=106: x=269；y=108: x=262,524；y=188: x=51；y=189: x=49,51,52；y=190: x=49,50；y=191: x=48,50,51,52；y=193: x=49,50；y=194: x=51,52；y=195: x=49,50；y=196: x=51,52；y=287: x=323,328；y=288: x=328,330；y=300: x=323；y=354: x=52；y=356: x=52；y=357: x=52；y=358: x=52；y=363: x=28,29,52；y=364: x=52`

### C9

`y=267: x=323`

### C10

`y=267: x=323,328；y=268: x=328,330；y=279: x=321；y=280: x=323,328`

### C11

`y=299: x=321；y=300: x=323,328；y=336: x=322,472`

## 审查状态

2026-10-25用户明确允许棋盘格修复并接受本表31对限定差异；31对仍保留原始RGBA计数，只在本表范围内接受。此表不包含Git授权、完整家族或原生最终像素验收。

审查后媒体已清理：457张PNG及manifest共458个文件，Git媒体审计clean，仓库外登记目录已删除。只保留数值／SHA文字账本；不扩大到C10或其他历史。
