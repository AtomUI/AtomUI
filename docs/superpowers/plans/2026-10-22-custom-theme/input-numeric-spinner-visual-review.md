# NU1：ButtonSpinner有效Own默认的限定视觉审查（已批准）

本包保留8历史字段/默认及key-ID-slot，修复FilledHandleBg读取有效Own.HandleBg；只改Token默认协议，不改模板、绘制、尺寸、输入或动效。ControlWidth作为Legacy保留90默认及资源读写，不新增内置Width行为。

## 实际场景与可见修复

真实Skia合成器900×530、缩放1，明暗各12实例：独立ButtonSpinner与NumericUpDown Input/Spinner模式，Large/Middle/Small/Custom，左右固定手柄、浮动手柄，实际悬停4行与Up/Down输入。原/候选完整轨迹一致；每侧44主帧，逐阶段另有44张A/A。

配置A：有效Global.ColorFillSecondary=#80000000，显式Own.HandleBg=#c86432，Filled应为#ff643219。配置B：透明HandleBg，Filled为#80000000。配置C：两个输入均透明，Filled透明。显式Filled=#663399始终优先。NumericUpDown Spinner模式保持14图标，上下箭头仍7；根几何/值/文本/动作布局及记录的偏移/透明度/变换共528条前后一致。

## 分离两种绘制证据

记录的显式过渡输入达到基值，逐阶段A/A均0；完整轨迹的自然合成器仍有小差异。同一候选从头重放也出现2–34像素/最大单通道11的差异，因此不能将其单独归因于V2，也不能宣称此现象已修复。

独立实例、同完整轨迹的直接RenderTargetBitmap绘制：26个非依赖修复阶段旧新均0，18个有效Own变化阶段仅相应手柄背景变化，逐阶段A/A均0。直接绘制不代替自然合成器或原生最终像素，CT25继续开放。

键盘往返在两版本均使12显示为12.0（既有decimal scale行为）；未把初始12画面当作它的零恢复断言，也未更改数字转换来消差。

## 每对完整帧的精确计数

| 同阶段旧→新 | 自然合成器像素 / 最大通道 | 直接绘制像素 / 最大通道 |
| --- | ---: | ---: |
| Light-normal | 0 / 0 | 0 / 0 |
| Light-aa | 0 / 0 | 0 / 0 |
| Light-nudge | 0 / 0 | 0 / 0 |
| Light-reset | 0 / 0 | 0 / 0 |
| Light-default-hover0 | 0 / 0 | 0 / 0 |
| Light-default-hover1 | 12 / 6 | 0 / 0 |
| Light-default-hover2 | 13 / 6 | 0 / 0 |
| Light-default-hover3 | 18 / 6 | 0 / 0 |
| Light-upstream | 2619 / 102 | 2598 / 102 |
| Light-upstream-hover0 | 3423 / 102 | 3414 / 102 |
| Light-upstream-hover1 | 3271 / 102 | 3248 / 102 |
| Light-upstream-hover2 | 3101 / 102 | 3080 / 102 |
| Light-upstream-hover3 | 3266 / 102 | 3248 / 102 |
| Light-explicit-filled | 21 / 6 | 0 / 0 |
| Light-clear | 21 / 6 | 0 / 0 |
| Light-keys-return | 3 / 2 | 0 / 0 |
| Light-disabled | 34 / 1 | 0 / 0 |
| Light-enabled-return | 30 / 1 | 0 / 0 |
| Dark-normal | 0 / 0 | 0 / 0 |
| Dark-aa | 0 / 0 | 0 / 0 |
| Dark-nudge | 0 / 0 | 0 / 0 |
| Dark-reset | 0 / 0 | 0 / 0 |
| Dark-default-hover0 | 0 / 0 | 0 / 0 |
| Dark-default-hover1 | 30 / 11 | 0 / 0 |
| Dark-default-hover2 | 43 / 11 | 0 / 0 |
| Dark-default-hover3 | 43 / 11 | 0 / 0 |
| Dark-upstream | 2651 / 90 | 2598 / 90 |
| Dark-upstream-hover0 | 3437 / 90 | 3414 / 90 |
| Dark-upstream-hover1 | 3275 / 90 | 3248 / 90 |
| Dark-upstream-hover2 | 3110 / 90 | 3080 / 90 |
| Dark-upstream-hover3 | 3274 / 90 | 3248 / 90 |
| Dark-explicit-filled | 46 / 11 | 0 / 0 |
| Dark-clear | 46 / 11 | 0 / 0 |
| Dark-keys-return | 19 / 6 | 0 / 0 |
| Dark-disabled | 0 / 0 | 0 / 0 |
| Dark-enabled-return | 0 / 0 | 0 / 0 |
| Light-transparent-upstream | 2616 / 6 | 2598 / 5 |
| Light-transparent-hover0 | 3417 / 5 | 3414 / 5 |
| Light-zero-upstream | 2616 / 10 | 2598 / 10 |
| Light-zero-hover0 | 3417 / 10 | 3414 / 10 |
| Dark-transparent-upstream | 2634 / 11 | 2598 / 9 |
| Dark-transparent-hover0 | 3433 / 9 | 3414 / 9 |
| Dark-zero-upstream | 2644 / 18 | 2598 / 18 |
| Dark-zero-hover0 | 3433 / 18 | 3414 / 18 |

## 自然小差异的独立范围

下列14对直接绘制均0，所列自然差异单独申请接受；不是通用容差，也不覆盖后续任意同量级变化。bbox为含端点物理像素坐标，差异集中于浮动手柄输入框边缘。逐阶段A/A对应帧与主帧一致，全部原始比较保持。

| 编号 / 阶段 | 像素 / 最大通道 | bbox |
| --- | ---: | --- |
| N01 Light-default-hover1 | 12 / 6 | [557,53,564,89] |
| N02 Light-default-hover2 | 13 / 6 | [557,53,564,152] |
| N03 Light-default-hover3 | 18 / 6 | [557,53,564,273] |
| N04 Light-explicit-filled | 21 / 6 | [558,50,564,181] |
| N05 Light-clear | 21 / 6 | [558,50,564,181] |
| N06 Light-keys-return | 3 / 2 | [559,152,562,181] |
| N07 Light-disabled | 34 / 1 | [275,54,625,339] |
| N08 Light-enabled-return | 30 / 1 | [607,230,625,339] |
| N09 Dark-default-hover1 | 30 / 11 | [557,50,564,89] |
| N10 Dark-default-hover2 | 43 / 11 | [557,50,564,182] |
| N11 Dark-default-hover3 | 43 / 11 | [557,50,564,182] |
| N12 Dark-explicit-filled | 46 / 11 | [558,50,564,384] |
| N13 Dark-clear | 46 / 11 | [558,50,564,384] |
| N14 Dark-keys-return | 19 / 6 | [559,152,564,384] |

## 结论边界

8项临时合同/帧验证、当前Mac实际NSWindow managed/arm64 NativeAOT各246项通过，官方Avalonia12.1.4；NET8零警告/错误20.98s。临时2源、422文件/1,117,804,821 bytes工程、564PNG+manifest全部清理，auditclean；清理后ButtonSpinner/Numeric原65PASS/0Skip/2s、Gallery1PASS/17ms。只有作者另步自审；没有永久新测试、框架补丁、Git操作或通用像素豁免。

本次只申请上述18对背景修复和N01–N14的具体自然差异；完整家族/其他Global消费者、动效/平台/原生最终像素及CT25继续开放，不含Git操作。

2026-10-28首次独立批准：call_b46a0bd3abf9463ba84cdf5f3d4a6c10:0，用户明确“批准此限定范围”。相同范围被错误重复询问一次（call_6d2bfd0d4a6d469ebc0aae461ec99c9e:0），不扩大范围、不重复消耗批准；无Git许可。
