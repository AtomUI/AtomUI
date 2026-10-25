# C10：自然合成限定差异审查表（已获用户批准）

仅申请以下精确历史帧对的审查；不设置通用容差，不继承C9或更早的有限批准。正常compositor原始差异及失败计数保持，直接Skia只作独立诊断。

## 固定环境与历史

Mac arm64、Avalonia12.1.4 Headless／Skia、scale1、产品Window1200×820，正常动效与region dirty clipping。根左边距main／aa为24、dip为25。基线为已提交393d718b3加本轮已批准的C9候选，唯一A/B变量是具体ColorSlider键盘入口；源SHA见[结果](color-slider-keyboard-results.md)。

两组数据分别保留：old／new为空值View默认数据；old-final／new-final为普通#9C7BAA62、渐变#9C7BAA62→Blue双stop数据。每个Light／Dark与普通／渐变View、每个main／aa／dip实例，执行完全相同轨迹：公开Alpha60→61及HEX #7BAA62准备且无焦点→default；自然Tab到第三分量thumb并同样准备→focus；原始Right两次→third-right；PageUp→third-page；移出焦点并同样准备→neutral；自然Tab到Alpha thumb并同样准备→alpha-focus；Right两次→alpha-right；移出焦点并同样准备→reset。

## 诊断与边界

自然两数据集共128组A/A，114组为0；128组1DIP全部检出。八组新旧默认整图为0；表内只列A/A、相同版本neutral→reset及应兼容的新旧相同阶段的真实非零差异。按已批准设计产生的颜色／文字／焦点数值变化不列作噪声。两个自然数据集的216对完整公开树观察（Bounds／矩阵／可见性／裁剪／字体画刷／Image源Bitmap SHA）完全相同。

独立直接Skia两数据集的128组A/A、32组恢复及40组新旧兼容比较全部为0；直接诊断200对公开树也相同，与自然216对合计416对。所采诊断不能证明每个历史compositor帧内部缓存的原因，不据此写产品重绘或替换自然基线。

## 每对限定

只限下列帧对、离散点集、像素数与通道差值上限。完整图整数物理像素坐标，alpha全255；只有明确dip角色使用X+1。当前不含其他Host、后续历史、Git、完整家族或原生最终显示面。

| ID | 比较 | 精确帧对 | 像素上限 | 通道上限 | 点集 |
| --- | --- | --- | ---: | ---: | --- |
| N01 | AA | `old-light-plain-main-alpha-focus.png` → `old-light-plain-aa-alpha-focus.png` | 2 | 1 | C1 |
| N02 | AA | `old-light-plain-main-alpha-right.png` → `old-light-plain-aa-alpha-right.png` | 2 | 1 | C1 |
| N03 | restore | `old-light-plain-main-neutral.png` → `old-light-plain-main-reset.png` | 1 | 1 | C2 |
| N04 | restore | `old-light-plain-aa-neutral.png` → `old-light-plain-aa-reset.png` | 1 | 1 | C2 |
| N05 | restore | `old-light-plain-dip-neutral.png` → `old-light-plain-dip-reset.png` | 1 | 1 | C2，X+1 |
| N06 | AA | `old-light-gradient-main-reset.png` → `old-light-gradient-aa-reset.png` | 19 | 2 | C3 |
| N07 | restore | `old-light-gradient-main-neutral.png` → `old-light-gradient-main-reset.png` | 1 | 1 | C4 |
| N08 | restore | `old-light-gradient-aa-neutral.png` → `old-light-gradient-aa-reset.png` | 20 | 2 | C5 |
| N09 | restore | `old-light-gradient-dip-neutral.png` → `old-light-gradient-dip-reset.png` | 1 | 1 | C4，X+1 |
| N10 | AA | `old-dark-plain-main-neutral.png` → `old-dark-plain-aa-neutral.png` | 3 | 1 | C6 |
| N11 | restore | `old-dark-plain-main-neutral.png` → `old-dark-plain-main-reset.png` | 5 | 1 | C7 |
| N12 | restore | `old-dark-plain-aa-neutral.png` → `old-dark-plain-aa-reset.png` | 2 | 1 | C8 |
| N13 | restore | `old-dark-plain-dip-neutral.png` → `old-dark-plain-dip-reset.png` | 5 | 1 | C7，X+1 |
| N14 | restore | `old-dark-gradient-main-neutral.png` → `old-dark-gradient-main-reset.png` | 5 | 1 | C9 |
| N15 | restore | `old-dark-gradient-aa-neutral.png` → `old-dark-gradient-aa-reset.png` | 5 | 1 | C9 |
| N16 | restore | `old-dark-gradient-dip-neutral.png` → `old-dark-gradient-dip-reset.png` | 2 | 1 | C10，X+1 |
| N17 | AA | `new-light-plain-main-neutral.png` → `new-light-plain-aa-neutral.png` | 7 | 1 | C11 |
| N18 | restore | `new-light-plain-main-neutral.png` → `new-light-plain-main-reset.png` | 1 | 1 | C2 |
| N19 | restore | `new-light-plain-aa-neutral.png` → `new-light-plain-aa-reset.png` | 8 | 1 | C12 |
| N20 | restore | `new-light-plain-dip-neutral.png` → `new-light-plain-dip-reset.png` | 1 | 1 | C2，X+1 |
| N21 | AA | `new-light-gradient-main-alpha-focus.png` → `new-light-gradient-aa-alpha-focus.png` | 2 | 1 | C13 |
| N22 | restore | `new-light-gradient-main-neutral.png` → `new-light-gradient-main-reset.png` | 1 | 1 | C4 |
| N23 | restore | `new-light-gradient-aa-neutral.png` → `new-light-gradient-aa-reset.png` | 1 | 1 | C4 |
| N24 | restore | `new-light-gradient-dip-neutral.png` → `new-light-gradient-dip-reset.png` | 1 | 1 | C4，X+1 |
| N25 | restore | `new-dark-plain-main-neutral.png` → `new-dark-plain-main-reset.png` | 5 | 1 | C7 |
| N26 | restore | `new-dark-plain-aa-neutral.png` → `new-dark-plain-aa-reset.png` | 5 | 1 | C7 |
| N27 | restore | `new-dark-plain-dip-neutral.png` → `new-dark-plain-dip-reset.png` | 5 | 1 | C7，X+1 |
| N28 | restore | `new-dark-gradient-main-neutral.png` → `new-dark-gradient-main-reset.png` | 5 | 1 | C9 |
| N29 | restore | `new-dark-gradient-aa-neutral.png` → `new-dark-gradient-aa-reset.png` | 5 | 1 | C9 |
| N30 | restore | `new-dark-gradient-dip-neutral.png` → `new-dark-gradient-dip-reset.png` | 5 | 1 | C9，X+1 |
| N31 | old-new | `old-light-gradient-main-alpha-focus.png` → `new-light-gradient-main-alpha-focus.png` | 2 | 1 | C13 |
| N32 | old-new | `old-dark-plain-main-neutral.png` → `new-dark-plain-main-neutral.png` | 2 | 1 | C14 |
| N33 | old-new | `old-dark-plain-main-alpha-focus.png` → `new-dark-plain-main-alpha-focus.png` | 2 | 1 | C14 |
| N34 | old-new | `old-dark-plain-main-reset.png` → `new-dark-plain-main-reset.png` | 2 | 1 | C14 |
| N35 | old-new | `old-dark-gradient-main-neutral.png` → `new-dark-gradient-main-neutral.png` | 2 | 1 | C15 |
| N36 | old-new | `old-dark-gradient-main-alpha-focus.png` → `new-dark-gradient-main-alpha-focus.png` | 2 | 1 | C15 |
| N37 | old-new | `old-dark-gradient-main-reset.png` → `new-dark-gradient-main-reset.png` | 2 | 1 | C15 |
| N38 | restore | `old-final-light-plain-main-neutral.png` → `old-final-light-plain-main-reset.png` | 1 | 1 | C2 |
| N39 | restore | `old-final-light-plain-aa-neutral.png` → `old-final-light-plain-aa-reset.png` | 1 | 1 | C2 |
| N40 | restore | `old-final-light-plain-dip-neutral.png` → `old-final-light-plain-dip-reset.png` | 1 | 1 | C2，X+1 |
| N41 | restore | `old-final-light-gradient-main-neutral.png` → `old-final-light-gradient-main-reset.png` | 1 | 1 | C4 |
| N42 | restore | `old-final-light-gradient-aa-neutral.png` → `old-final-light-gradient-aa-reset.png` | 1 | 1 | C4 |
| N43 | restore | `old-final-light-gradient-dip-neutral.png` → `old-final-light-gradient-dip-reset.png` | 1 | 1 | C4，X+1 |
| N44 | AA | `old-final-dark-plain-main-neutral.png` → `old-final-dark-plain-aa-neutral.png` | 3 | 1 | C6 |
| N45 | restore | `old-final-dark-plain-main-neutral.png` → `old-final-dark-plain-main-reset.png` | 5 | 1 | C7 |
| N46 | restore | `old-final-dark-plain-aa-neutral.png` → `old-final-dark-plain-aa-reset.png` | 2 | 1 | C8 |
| N47 | restore | `old-final-dark-plain-dip-neutral.png` → `old-final-dark-plain-dip-reset.png` | 5 | 1 | C7，X+1 |
| N48 | restore | `old-final-dark-gradient-main-neutral.png` → `old-final-dark-gradient-main-reset.png` | 5 | 1 | C9 |
| N49 | restore | `old-final-dark-gradient-aa-neutral.png` → `old-final-dark-gradient-aa-reset.png` | 5 | 1 | C9 |
| N50 | restore | `old-final-dark-gradient-dip-neutral.png` → `old-final-dark-gradient-dip-reset.png` | 2 | 1 | C10，X+1 |
| N51 | AA | `new-final-light-plain-main-alpha-focus.png` → `new-final-light-plain-aa-alpha-focus.png` | 7 | 1 | C11 |
| N52 | AA | `new-final-light-plain-main-alpha-right.png` → `new-final-light-plain-aa-alpha-right.png` | 7 | 1 | C11 |
| N53 | restore | `new-final-light-plain-main-neutral.png` → `new-final-light-plain-main-reset.png` | 1 | 1 | C2 |
| N54 | restore | `new-final-light-plain-aa-neutral.png` → `new-final-light-plain-aa-reset.png` | 1 | 1 | C2 |
| N55 | restore | `new-final-light-plain-dip-neutral.png` → `new-final-light-plain-dip-reset.png` | 1 | 1 | C2，X+1 |
| N56 | AA | `new-final-light-gradient-main-third-right.png` → `new-final-light-gradient-aa-third-right.png` | 12 | 2 | C16 |
| N57 | restore | `new-final-light-gradient-main-neutral.png` → `new-final-light-gradient-main-reset.png` | 1 | 1 | C4 |
| N58 | restore | `new-final-light-gradient-aa-neutral.png` → `new-final-light-gradient-aa-reset.png` | 1 | 1 | C4 |
| N59 | restore | `new-final-light-gradient-dip-neutral.png` → `new-final-light-gradient-dip-reset.png` | 1 | 1 | C4，X+1 |
| N60 | AA | `new-final-dark-plain-main-third-right.png` → `new-final-dark-plain-aa-third-right.png` | 7 | 1 | C17 |
| N61 | restore | `new-final-dark-plain-main-neutral.png` → `new-final-dark-plain-main-reset.png` | 5 | 1 | C7 |
| N62 | restore | `new-final-dark-plain-aa-neutral.png` → `new-final-dark-plain-aa-reset.png` | 5 | 1 | C7 |
| N63 | restore | `new-final-dark-plain-dip-neutral.png` → `new-final-dark-plain-dip-reset.png` | 5 | 1 | C7，X+1 |
| N64 | AA | `new-final-dark-gradient-main-third-right.png` → `new-final-dark-gradient-aa-third-right.png` | 7 | 1 | C18 |
| N65 | AA | `new-final-dark-gradient-main-third-page.png` → `new-final-dark-gradient-aa-third-page.png` | 8 | 1 | C19 |
| N66 | AA | `new-final-dark-gradient-main-alpha-right.png` → `new-final-dark-gradient-aa-alpha-right.png` | 8 | 1 | C20 |
| N67 | restore | `new-final-dark-gradient-main-neutral.png` → `new-final-dark-gradient-main-reset.png` | 5 | 1 | C9 |
| N68 | restore | `new-final-dark-gradient-aa-neutral.png` → `new-final-dark-gradient-aa-reset.png` | 5 | 1 | C9 |
| N69 | restore | `new-final-dark-gradient-dip-neutral.png` → `new-final-dark-gradient-dip-reset.png` | 5 | 1 | C9，X+1 |
| N70 | old-new | `old-final-dark-gradient-main-neutral.png` → `new-final-dark-gradient-main-neutral.png` | 2 | 1 | C15 |
| N71 | old-new | `old-final-dark-gradient-main-alpha-focus.png` → `new-final-dark-gradient-main-alpha-focus.png` | 2 | 1 | C15 |
| N72 | old-new | `old-final-dark-gradient-main-reset.png` → `new-final-dark-gradient-main-reset.png` | 2 | 1 | C15 |

## 离散坐标

### C1

`y=298: x=396,403`

### C2

`y=267: x=323`

### C3

`y=291: x=319；y=292: x=319,332；y=293: x=332；y=294: x=332；y=295: x=319,332；y=311: x=393,406；y=312: x=393,406；y=313: x=393,406；y=314: x=393,406；y=315: x=393,406；y=316: x=393,406`

### C4

`y=287: x=323`

### C5

`y=287: x=323；y=291: x=319；y=292: x=319,332；y=293: x=332；y=294: x=332；y=295: x=319,332；y=311: x=393,406；y=312: x=393,406；y=313: x=393,406；y=314: x=393,406；y=315: x=393,406；y=316: x=393,406`

### C6

`y=279: x=321；y=280: x=323,328`

### C7

`y=267: x=323；y=268: x=330；y=279: x=321；y=280: x=323,328`

### C8

`y=267: x=323；y=268: x=330`

### C9

`y=287: x=323；y=288: x=330；y=299: x=321；y=300: x=323,328`

### C10

`y=287: x=323；y=288: x=330`

### C11

`y=271: x=319；y=272: x=319,332；y=273: x=332；y=274: x=332；y=275: x=319,332`

### C12

`y=267: x=323；y=271: x=319；y=272: x=319,332；y=273: x=332；y=274: x=332；y=275: x=319,332`

### C13

`y=318: x=396,403`

### C14

`y=316: x=322,472`

### C15

`y=336: x=322,472`

### C16

`y=311: x=393,406；y=312: x=393,406；y=313: x=393,406；y=314: x=393,406；y=315: x=393,406；y=316: x=393,406`

### C17

`y=291: x=406；y=292: x=393,406；y=294: x=393,406；y=296: x=393,394`

### C18

`y=311: x=406；y=312: x=393,406；y=314: x=393,406；y=316: x=393,394`

### C19

`y=311: x=406；y=312: x=393,406；y=313: x=393,406；y=314: x=393,406；y=316: x=394`

### C20

`y=291: x=320,331；y=293: x=319,332；y=294: x=319,332；y=296: x=320,331`

## 审查状态

2026-10-25用户明确“接受表内72对限定差异，继续验证”。正式RGBA计数不被隐藏或改为成功，授权仅覆盖本表精确历史／点集／上限，不包含Git、原生最终显示面或完整家族。

审查后645张PNG与manifest共646文件已清理，Git媒体审计clean，仓库外run目录已删除。只保留数值／SHA文字账本，本文历史帧名不表示媒体仍存在。
