# NU3：Numeric inline 分隔边限定视觉审查（已批准）

本包只让左右分隔边消费已有父 BorderThickness：减号段右边取 Right，加号段左边取 Left，Top/Bottom 不进入竖边。复用既有 BorderThicknessFilterConverter，通过两条 TemplateBinding 声明；默认1保持，没有新 Own 字段或代码绑定。

原全局3、非均匀(3,5,7,9)、本地非均匀、0四项反例均失败，原分隔边固定1。候选专项加完整帧共5项通过。场景为 Skia 660×430、明暗各6根（四尺寸和自定义高50/60）、固定宽260，同完整操作轨迹。配置3下真实中心加减点击成功；156根记录的尺寸、值、可见性、Bounds前后相同，按钮本体仍为方形。分隔边在按钮之外占位，配置后按钮位置及中间可用空间相应变化。

## 全帧精确计数

自然合成器与独立直接绘制的每对计数完全相同，没有新增自然小差异，也不采用通用容差。

| 阶段 | Light 像素 / 最大通道 | Dark 像素 / 最大通道 |
| --- | ---: | ---: |
| normal | 0 / 0 | 0 / 0 |
| aa | 0 / 0 | 0 / 0 |
| nudge | 0 / 0 | 0 / 0 |
| reset | 0 / 0 | 0 / 0 |
| uniform | 824 / 38 | 824 / 46 |
| clicked | 824 / 38 | 824 / 46 |
| nonuniform | 2250 / 224 | 2251 / 199 |
| zero | 472 / 38 | 472 / 46 |
| local | 754 / 224 | 754 / 199 |
| clear | 0 / 0 | 0 / 0 |
| hidden-actions | 0 / 0 | 0 / 0 |
| disabled | 0 / 0 | 0 / 0 |
| enabled-return | 0 / 0 | 0 / 0 |

uniform/clicked 为全局3；nonuniform 为全局(3,5,7,9)；zero 为全局0；local 为清除全局后第六根本地(1,4,6,2)。A/A零差异、1 DIP扰动可检出且恢复零，两版本历史一致。已查看非均匀完整帧前后及暗色0候选：分隔边按对应侧加粗或消失，外框、文本、内容与按钮几何保留。

本次需要批准的仅为上述10对显式厚度场景变化。默认及其余16对均为0。实际NSWindow managed/arm64 NativeAOT各590项、NET8零警告/错误20.56s；临时2源/422文件原生工程/142媒体清理后Numeric精确49PASS、0Skip、2s，audit clean。NU1、NU2-A、NU2-B的批准不能替代本包审查；完整家族、原生最终像素及CT25仍开放。详细结果见[结果](input-numeric-divider-results.md)。

2026-10-28用户独立批准call_8c6c10577cc94e12b7d37d281865fcfc:0，仅上述10对具体变化，不含Git或通用容差。
