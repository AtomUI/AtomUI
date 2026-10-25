# CT25 Picker 模板替换：交付审查中

当前切片修复四类 Picker 的有效模板替换生命周期，不属于冻结的30文件提交。CT24继续按用户要求暂缓；本切片不表示CT16家族或完整CT25完成，没有自动提交或发布。

## 行为与实现

- 保留同一 public PickerPresenter、提交值和当前草稿。原始Generic13/9、null→有效模板的失败保留；有效模板的直接替换扩展到全部非复用旧部件、起/终点和普通／固定状态。
- 退休原生模板主题帧、复用Icon旧TemplatedParent、旧Popup.Child和原生合成父链四条真实保留路径。使用限定Core固定反射／精确保留，图标同步、正常Content交接和实际Visual合成节点退役，不改定位私有缓存或强写逻辑父节点。
- pin／打开及输入焦点恢复在模板完成后执行，避免Popup.Show同步布局重入；弱目标、版本、卸载／外部焦点取消及连续替换检查。范围时间接回新端点锚点，不重置编辑或明确placement；跳过活动文本的提交值重投射。

## 反例、功能与平台证据

相同诊断清理后的17项场景原产品17FAIL55.334s；候选通过。补充连续替换／外部焦点／卸载、应用合成子图保留、正常Content归属及后续TemplateBinding更新。空间反例8FAIL29.818s，修复后与16状态合计24PASS87.052s；起／终点文本投射反例2FAIL18.653s，修复后当前34个临时用例与45项具名相邻检查合计79PASS106.832s。只运行精确filter，没有全量／change回归。

真实Mac managed，Avalonia.Native.WindowImpl，scale2，16 OverlayPopupHost＋16 Native PopupRoot，共32场景／851检查通过136.378s。实际原生键盘设备／input manager／typed IInputRoot输入到达；每次重建检查实际屏幕位置和尺寸，范围时间检查新端点就是实际PlacementTarget。当前同源NativeAOT亦32／851项通过153.348s，DynamicCode=false，产物SHA256为08bb9ef8fc909893ad7e2fe78d0ff3fa9b7c21803d4ee1e10e1f4d05ab344ed6。发布先遇MSBuild复用worker提前退出，单worker／不复用服务重试98.096s通过；5项已有Homebrew库最低系统版本警告保留，无IL2026／IL3050。此前739项旧产物不作当前证明。

成员审计8文件／5控件／0问题，git diff检查通过。清理后的100项具名相邻／Semantic／图标／Popup验证通过22.866s；NET8最终构建16.784s零警告／错误。首次非共享csc在未修改的Native模块退出139，原模块定向检查与完整Desktop构建均在源码未变时通过，没有修改SDK、依赖或产品规避。9个临时验证C#源及64份TRX均已记录后删除，长期测试净新增0；正式TimePicker、DatePicker、Icon、Popup实现与changelog按候选范围同步，未改冻结的任务总表。

## 最终自然合成对照

Mac arm64 Headless/Skia、scale1、900×720产品Window，正常motion。每侧192帧，独立main／A/A／1DIP走完全相同8步骤；前缀只选文件名。源代码为candidate-final-source-hashes记录的8文件。

| 控件 | 模板替换后明色新旧变化像素 | 暗色新旧变化像素 |
| --- | ---: | ---: |
| TimePicker | 30032 | 61319 |
| RangeTimePicker | 30459 | 61586 |
| DatePicker | 37259 | 39572 |
| RangeDatePicker | 59130 | 69374 |

原版替换后错误关闭弹层；候选保持同一会话和弹层打开。候选同实例的再次展开与模板替换后整图，四控件明／暗8对全部0差异，范围时间已无偏移。查看了旧版关闭图及候选明／暗范围时间、单日期和范围日期；选值、时间列／日期网格、footer、箭头、范围指示条和锚点完整，无遮挡或隐藏内容替代修复。

192组匹配轨迹的owner位置／尺寸、ClientSize、scale、主题及提交值逐字段0差异。新旧同阶段64对，除上述8组预期行为变化外，仅Light RangeDatePicker初次展开两像素，区域(36,118)及(36,119)，单通道最大3；其余55对整图0。初始默认与全部关闭／解除固定／最终复位均0。

旧64组A/A全部0；最终候选63组0，Light RangeDatePicker初次展开仅上述2像素。全部128组1DIP检出。没有强制重绘、裁剪、平移、遮罩、通用容差或基线覆盖。原始候选还分别出现过Light RangeTime／Dark RangeDate相同位置2像素；详见[picker-corner-results](picker-corner-results.md)。该报告原审查范围不自动扩大至当前Light RangeDate，需明确补充批准。

## 待完成的出口

1. 所示有效模板后保持打开的预期视觉变化及最终Light RangeDate两像素限定差异审查。
2. AOT851项、NET8和相邻Semantic合同已通过；仍不把功能Host验证当最终Native像素验收。
3. 临时测试、Python驱动、13项源码备份／反编译参考、Native项目438文件／1428270068字节已清理；全部堆在前阶段已清理。640帧及派生媒体仍在登记的仓库外目录等待视觉审查，审计无仓库二进制发现；清理后的模块门禁已执行。
4. 作者自审，提交范围交用户审查；不得复用已花完的旧提交批准，也不混入待批准30文件或H1媒体。

## 本轮用户审查结论与媒体清理

用户明确回复“批准保持打开及终版2像素限定（推荐）”。只批准四类Picker有效模板替换后保持打开，以及终版Light RangeDate初展(36,118)/(36,119)两像素、单通道最大3；Host、历史和配置以报告为准。 此批准不含Git提交、原生最终像素或完整家族／CT任务验收。

已保留文本结果、原始差异数及640张PNG的逐文件SHA256；经标记目录工具清理仓库外本项媒体，删除641文件，清理审计clean且findings为空。前文待审／媒体保留描述为历史状态；本段为当前结论。常驻测试净新增0，清理后的既有模块验证见上文。
