# CT13 CB4：显示区文字的限定视觉审查（已批准）

范围是同一ComboBoxTheme内的占位默认/公开画刷接线，以及Filled/Borderless错误/警告文字在禁用时退出。没有新增字段/身份、根禁用Foreground Setter或代码找节点回退。根Foreground默认getter保持原状。

## 源码与实际语义

- 普通/可编辑占位默认读取有效Global.ColorTextPlaceholder；公开PlaceholderForeground同时进入两种显示路径。普通占位不再额外乘0.3透明度。
- 已有公开占位画刷的alpha及生成ComboBoxPlaceholderStyle的Foreground/Opacity按原生优先级生效；真实预编译AXAML已核对专用类、两个Setter及撤回。
- 禁用选择文字继续由已有共享frame读取ColorTextDisabled；新增的是让内部错误/警告状态色退出禁用状态。普通禁用文字原来已正确，不增加根Setter。
- 占位颜色仍由PlaceholderForeground负责，包含禁用态；作用域示例中占位粉色#cc3366、普通文字蓝色#2255aa、禁用选择文字绿色#118855。公开占位画刷示例为Purple。

## 完整帧与精确计数

Headless真实Skia合成器，640×570、缩放1、明暗各10实例；普通/可编辑、空/已选、公开占位画刷与根Foreground、默认/作用域配置/禁用/清除、原生校验错误及公开Warning。原版和候选均完整重放同一轨迹。

每侧26帧、260条根记录；根bounds、文本、选择、启用状态和编辑模式全部一致，根Foreground getter前后也完全一致。新增可见差异位于占位字形与禁用时的错误/警告选择字形，背景、框体、内容和布局保持；同版本AA、1DIP复位、启用/配置清除恢复均0。

| 同阶段旧→新 | 像素数 | 最大单通道差 |
| --- | ---: | ---: |
| Light-normal | 2812 | 188 |
| Light-aa | 2812 | 188 |
| Light-nudge | 2812 | 188 |
| Light-reset | 2812 | 188 |
| Light-disabled | 2987 | 227 |
| Light-enabled-return | 2812 | 188 |
| Light-configured | 3035 | 204 |
| Light-configured-disabled | 3035 | 213 |
| Light-clear | 2812 | 188 |
| Dark-normal | 2652 | 79 |
| Dark-aa | 2652 | 79 |
| Dark-nudge | 2652 | 79 |
| Dark-reset | 2652 | 79 |
| Dark-disabled | 2991 | 75 |
| Dark-enabled-return | 2652 | 79 |
| Dark-configured | 3035 | 184 |
| Dark-configured-disabled | 3035 | 173 |
| Dark-clear | 2652 | 79 |
| Light-error-disabled | 4157 | 227 |
| Light-error-enabled | 2812 | 188 |
| Dark-error-disabled | 4161 | 142 |
| Dark-error-enabled | 2652 | 79 |
| Light-warning-disabled | 4157 | 227 |
| Light-warning-enabled | 2812 | 188 |
| Dark-warning-disabled | 4161 | 138 |
| Dark-warning-enabled | 2652 | 79 |

这里逐对记录预期修复造成的差异，不构成任何通用容差；AA/扰动帧在两版本间也包含同一占位修复。1DIP检错：原Light12260/Dark12218，候选Light13690/Dark13636，全部实际检出。

## 验证与仍开放的边界

最初原版5FAIL/2PASS中两项根getter期望被实际消费者记录否决，已经明确作废，未删除原始日志。原版共享frame本来已正确消费普通禁用色。改为实际文字和原getter合同后，原6FAIL/4PASS（含专用Style撤回），两项Warning补充2FAIL；最终临时13PASS、0Skip、9秒。最终候选未保留多余的根禁用Setter。

实际当前Mac managed/最终arm64 NativeAOT各108项、exit0；ILC明确使用官方Avalonia12.1.4/net10.0，无IL2026/IL3050。NET8 Release零警告/错误、18.47s；中间版本构建未当最终证据。四份临时C#/AXAML、430文件/1,207,910,367 bytes原生工程、133PNG和manifest共134媒体文件已删除，auditclean；清理后原ComboBox69PASS/0Skip/2s。Gallery相关精确8PASS/0Skip/5s。媒体已审看并删除，仅保留精确文字/数值，没有失效媒体链接。

已有根Local Foreground在禁用时未传到实际frame文字的现象原→新均存在，根getter仍保留该Local值；本包未解决此独立优先级缺口，完整CT13/CT25保持开放。公开占位画刷与专用Placeholder Style的优先级已在本包实际验证。

本次申请只接受表中具体颜色/占位修复及其精确范围，不包含Git操作、其他控件或原生最终像素的通用豁免。没有提交、推送、新Actions或NuGet发布。

2026-10-28：用户通过本次独立问题明确回复“批准本包限定视觉范围”（call_290597fe543a449a96fd033c0196097e:0）。批准仅覆盖上表及所述占位/错误警告文字范围；没有通用容差、Git或完整家族批准。
