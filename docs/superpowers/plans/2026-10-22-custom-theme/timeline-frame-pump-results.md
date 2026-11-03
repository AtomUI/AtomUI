# CT15 P3-B：正常帧推进与持续动画补证

2026-10-30，沿既定feature/custom-theme目录/分支、单agent执行，不创建分支/worktree、不提交。P3-A表内8对限定批准保持，其他原始记录不覆盖。本包只修正临时验收驱动，**产品源码没有新增修改**。

## 单变量根因

上一轮驱动在UI线程RunJobs后Thread.Sleep，只消费排队操作，没有推进Dispatcher定时器。官方本地源码的MediaContext通过DispatcherTimer继续颜色动画；这解释了图像/属性连续稳定却仍停在中间色的假终态。不能把延长Thread.Sleep或几个相等帧作为就绪证明。

同一个公开Timeline、原Button内容、正常motion、相同Global ColorText=#802080/FontSize23及一秒等待，两个独立进程仅改变帧推进方式：

| 路径 | 初始Foreground | 配置后 | 清除后 |
| --- | --- | --- | --- |
| RunJobs＋Thread.Sleep | #e0000000 | #e20b030b | #e20b030b |
| 正常DispatcherFrame＋16ms DispatcherTimer | #e0000000 | #ff802080 | #e0000000 |

因此P3-A中的跨运行中间色数据仍是原始失败事实，但该最小反例已定位为验收驱动遗漏，不是Button消费或框架产品修复。未修改Button、Core、官方Avalonia12.1.4或缓存程序集；CT12/13/14结论和CT25/FW1其他原始问题不变。

## 完整静态重放

保留原1320×760/scale1、六根Timeline、Height300、原长内容/标签/Button/静态PathIcon pending及四算法配方。原16阶段操作顺序保持，唯一驱动差异是正常推进Dispatcher；仍至少700ms与12个图像/属性稳定帧，5s超时，不关动效。

旧源码只在原目录回放P3-A两文件，持有checkout OS锁，finally恢复候选并验证源码相同。原/新managed及当前Mac osx-arm64 NativeAOT各**440项检查通过**，新增实际断言包括Global目标/清除终态、11Own的dot/rail/icon/末项/pending/margin、8专用Style及实际紫色rail/Opacity Setter、真实鼠标一次与Enter一次。

- 旧新64对：**56对0**，另8对为已批准的删除pending/末项rail变化，仍分别651/751及Compact531/691像素，最大通道15/28；批准范围未扩大。
- JIT/AOT64对：**全部0**。四配方默认、Global配置/清除、Part配置/清除、RTL、删除/重插、隐藏/倒序及输入阶段逐对比较，无遮罩、对齐或容差。
- 同版本A/A、1DIP复位、RTL复位及Global配置清除均0；1DIP正对照24693/24181/28174/27661像素，保持前一轮检错能力。
- **Part资格退出到最初默认仍有102/102/82/82个max1像素**，旧/新/AOT同阶段彼此为0。此项没有因修正帧推进而消失，继续保留未通过；不继承其他包的合成批准，不自动归为已解决的框架缺陷。
- 发布无IL警告；五条已知Homebrew dylib最低macOS linker警告保留。本AOT宿主仍为Headless/Skia，不代替NSWindow系统最终面或其他平台。

已实际查看AOT暗色Global配置的完整画面和默认Spinner关键相位。固定尺寸下的大字换行/原有裁剪及暗色缺省黑字保持，不宣称解决这些独立外观问题。

## 默认pending的同相位轨迹

复用CT11既有临时真实Compositor Stopwatch输入方式，仅在测试中输入递增0/125/300/750/1000/1125/1300/1750/2000ms，finally恢复原Clock全部状态。模板、Spin、默认1s周期和真实Skia合成器保持，不在产品增加时钟。真实时钟连续运行及卸载证据继续复用P1/P3-A，静止采样点不作为实际循环运行证明。

两主题×两方向：36个轨迹画面加16个AA/1DIP/恢复校准画面，**旧新52对全部0**。AA和复位0，1DIP正对照分别2555/2884/2472/2800像素，0→300ms实际变化。两版共享同一时间原点与采样序列，不挑选相近帧。

额外严格周期端点断言在候选4项、旧源码4项均**FAIL**（分别4.2431s与3.6099s）：0与1000/2000ms的画面不等，而125/1125、300/1300、750/1750ms的对应相位相同。1000ms画面与之前750ms相同，0→1000ms四场景各50像素不同、最大通道明233/暗200，差异局限默认spinner。此为保留的端点观察，不声称根因已定位，不用旧新兼容0把该FAIL改成PASS，也不修改官方依赖。完整持续动画正确性/最终平台面仍开放。

## 清理与最终门槛

临时4项源码所在5205-byte文件、外部工程434文件/1088161581 bytes及489取证文件已全部删除；evidence cleanup审计clean。清理后实际重建测试程序集，精确Timeline **52 PASS/0 FAIL/0 Skip，1s**。本包没有产品修改，复用相同产品SHA的既有NET8零警告结果，不为重复确认扩大测试。作者另步自审，非独立评审。P3-B正常帧推进没有创建产品helper、字段、API或测试基础设施；原始状态/源码/几何合同保持，完整P3与CT15不因本包补证而关闭。


P3-C后续定位：[新端点结果](timeline-endpoint-results.md)在同一官方12.1.4运行中记录显式0首key的NaN，原四项端点FAIL继续保留为历史。当前消费者使用明确0隐式起点+唯一2π终点，原始相位与新增四算法/RTL/原生Spin-Pulse受控AOT验证通过。仅该工程缺陷修正，尚待96对限定批准；Part恢复max1、正常clock时相、平台和完整P3/CT15仍保留，不把本轮消费者修正写成FW1/CT25框架修复。


限定批准登记：2026-11-01，用户明确回复“批准表内96对，其余边界保留”（request_user_input_async / call_d2f7bcbd00ab46e7b6cd3610f38aa4bb / item 0）。仅P3-C表内96个独立端点变化获批准；64张second为相同图像复核，不新增产品状态。各15真实clock非零、Part恢复max1、原失败账本、平台及完整P3/CT15继续保留。官方显式0模型NaN仍可复现，不改CT25/FW1，不授权提交。


2026-11-01 Timeline P3 Part恢复定位：四算法/八专用Style32独立A-B-A，32AA0、实际命中/实例/几何恢复；8 icon/title恢复0，其他24严格非零。明暗item/rail采集值相同，背景根重绘均0。无AtomUI独立Avalonia12.1.4工程managed/AOT各20检查17PASS/3恢复FAIL，rail各14/max30、Opacity19/max14，15帧cross全0；公开开关试验未消除，不改产品/依赖/容差。23临时case/3源28395字节、446取证及339会话848665821字节已清理；最终精确Timeline52PASS/0Skip/1s，产品未变复用NET8零警告21.10s。FW2限定归属提案待用户批准，原P3-B102/82、FW1/CT25及CT12/13/14和完整CT15保留，见timeline-part-restore-audit.md、feedback-partial-redraw-visual-review.md，不提交。


## FW2限定归属批准登记

2026-11-01 用户明确回复“仅批准表列路径归 CT25/FW2，其余保留”（request_user_input_async / call_58d9ca164cbe4d43b4254cc21caa6b70 / item 0）。仅feedback-partial-redraw-visual-review.md列出的3个无AtomUI独立反例及24个当前Timeline局部恢复路径归CT25/FW2继续调查；非零仍FAIL，不设置或扩大任何像素容差，不修改既有FW1。原P3-B102/102/82/82、其他家族/native/平台/GC及完整CT15均保留，不授权提交。


最终清理后精确Timeline52PASS/0Skip/4.5166s，实际重建测试程序集；本轮无产品源码改动，复用相同最终源码NET8零警告零错误17.41s。1130取证经官方audit/cleanup clean删除，外部454会话文件1374532913字节、全部恢复的历史命令/驱动/发布产物与marker均删除；最后helper读取后清理。没有执行历史命令中的Git或旧源码替换，没有新增永久测试/全量/分支/worktree/提交。

作者另步自审：原主批64/440保持完整；组合/Content/排除Content与重绘分别分账，透明懒滚动条节点例外明确披露；208跨运行/96只读图全0不替代恢复失败。8路径限定归属表已完成工程、清理、精确门禁，等待用户是否交FW2调查的明确裁决，未自动修改原归属、FW1或完整P3/CT15结论。


## 原 P3-B 八路径限定归属批准登记

用户明确回复“仅批准表列8路径归CT25/FW2，其余保留”（request_user_input_async / call_cd2233cdfe1848fc889d777f91b52754 / item 0）。仅timeline-original-part-visual-review.md列出的4条原P3-B组合恢复102/102/82/82及4条原值Content0.7恢复124/102/98/82交CT25/FW2继续调查；非零仍FAIL，不增像素容差、不改既有FW1，不把透明滚动条懒节点差别抹去。其他原故障账本、家族/原生平台/完整P3与CT15保留，不授权提交。此前3独立模型/24路径批准范围不自动扩成其他家族。
