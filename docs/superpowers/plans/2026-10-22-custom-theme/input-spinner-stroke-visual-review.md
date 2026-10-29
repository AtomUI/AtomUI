# NU2-B：Spinner内部描边 owner 限定视觉审查（已批准）

产品仅将DecoratedBox与Handle的两处SpinnerBorderThickness从Shared.BorderThickness改为OwnerTokenResource；独立ButtonSpinner与NumericUpDown分别使用真实公开owner，默认1不变，没有新字段/identity/绘制/C#绑定或inline分隔线改动。

原四目标4FAIL1s→候选5PASS14s；独立owner3与Numeric5、0/清除1、身份干扰隔离成立。既有完整Thickness与绘制取Left规则保持，未把Disabled算法下LineWidth单值错误当成BorderThickness已重推。

真实Skia620×510、明暗各8实例/4尺寸，左右固定/浮动手柄、40阶段同完整轨迹，配置3/5下实际hover及中心点击增减通过。320根尺寸/值/文本记录前后一致；数值12到12.0的既有格式行为两侧相同。

独立直接绘制22个非修复阶段均0，18个配置阶段改变实际内描边。自然默认hover6对小差异另列，不沿用此前任何范围；CT25绘制历史问题开放，没有通用容差。

## 每对完整帧

| 阶段 | 自然像素 / 最大通道 | 直接绘制像素 / 最大通道 |
| --- | ---: | ---: |
| Light-normal | 0 / 0 | 0 / 0 |
| Light-aa | 0 / 0 | 0 / 0 |
| Light-nudge | 0 / 0 | 0 / 0 |
| Light-reset | 0 / 0 | 0 / 0 |
| Light-default-hover0 | 0 / 0 | 0 / 0 |
| Light-default-hover1 | 14 / 10 | 0 / 0 |
| Light-default-hover2 | 19 / 10 | 0 / 0 |
| Light-default-hover3 | 23 / 10 | 0 / 0 |
| Light-configured | 1049 / 38 | 1041 / 38 |
| Light-configured-hover0 | 1618 / 38 | 1610 / 38 |
| Light-configured-hover1 | 1565 / 38 | 1541 / 38 |
| Light-configured-hover2 | 1521 / 38 | 1480 / 38 |
| Light-configured-hover3 | 1576 / 38 | 1541 / 38 |
| Light-ready-click | 1651 / 38 | 1610 / 38 |
| Light-clicked | 1082 / 38 | 1041 / 38 |
| Light-zero | 296 / 38 | 296 / 38 |
| Light-zero-hover | 378 / 38 | 378 / 38 |
| Light-clear | 0 / 0 | 0 / 0 |
| Light-disabled | 0 / 0 | 0 / 0 |
| Light-enabled-return | 0 / 0 | 0 / 0 |
| Dark-normal | 0 / 0 | 0 / 0 |
| Dark-aa | 0 / 0 | 0 / 0 |
| Dark-nudge | 0 / 0 | 0 / 0 |
| Dark-reset | 0 / 0 | 0 / 0 |
| Dark-default-hover0 | 0 / 0 | 0 / 0 |
| Dark-default-hover1 | 6 / 9 | 0 / 0 |
| Dark-default-hover2 | 22 / 9 | 0 / 0 |
| Dark-default-hover3 | 27 / 9 | 0 / 0 |
| Dark-configured | 1070 / 47 | 1064 / 47 |
| Dark-configured-hover0 | 1656 / 47 | 1650 / 47 |
| Dark-configured-hover1 | 1603 / 47 | 1574 / 47 |
| Dark-configured-hover2 | 1555 / 47 | 1503 / 47 |
| Dark-configured-hover3 | 1629 / 47 | 1574 / 47 |
| Dark-ready-click | 1695 / 47 | 1650 / 47 |
| Dark-clicked | 1123 / 47 | 1064 / 47 |
| Dark-zero | 366 / 48 | 366 / 48 |
| Dark-zero-hover | 486 / 48 | 486 / 48 |
| Dark-clear | 0 / 0 | 0 / 0 |
| Dark-disabled | 0 / 0 | 0 / 0 |
| Dark-enabled-return | 0 / 0 | 0 / 0 |

## 本次申请范围

接受上述18对3/5/0内部描边修复，以及Light-default-hover1/2/3的14/19/23像素（最大通道10）、Dark-default-hover1/2/3的6/22/27像素（最大通道9）。六对直接绘制均0，仅限表内场景，不包含未来相近量级变化。

实际当前Mac NSWindow managed/arm64 NativeAOT各196项通过，0/非均匀/清除及真实owner的两个内部资产均由已知AvaloniaProperty注册名有限查询观察，没有CLR反射扫描或新公开API。原生检查不冒充原生最终像素或原生点击；官方Avalonia12.1.4，NET8零警告/错误19.48s。

临时2源、422文件/1,117,558,899 bytes原生工程、160PNG+manifest共161文件已清理，auditclean；清理后原65PASS/0Skip/2s，Gallery精确1PASS/0Skip/15ms。只作者另步自审，无Git；完整家族、inline两条分隔边、其余Global/状态/Part/Custom/平台与CT25仍开放。

2026-10-28用户独立批准call_091b06ad586b4ee2bc90c2a2fdcac01a:0，仅18描边变化与六对指定hover自然差异，不含Git或通用容差。
