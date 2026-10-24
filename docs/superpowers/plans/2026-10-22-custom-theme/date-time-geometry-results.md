# CT-16 D2-A 时间列动态行高结果

本包在D1清理后的同一feature/custom-theme目录执行，仅修改DateTimePickerPanel.cs；不创建分支／worktree、不提交、不使用子agent。整体及后续范围见 [tasks](tasks.md) 和 [日期时间计划](date-time-consumers.md)。

## 原故障与修复

真实AtomUI ScrollViewer中的时间列从28改为40行高后，旧像素offset未变：循环选中10的逻辑位置变成23.8，非循环变成7；浏览12.25变成1.375，AM/PM的0.6变成0.42。没有移除循环、header、高度、内容或滚动条件。

现在在旧行高下记录逻辑行与分数偏移，新的DesiredSize与ScrollViewer实际extent一致后恢复；短循环补行改变容器数量也保留位置。明确SelectedValue定位请求优先，几何引发的offset确认不提交新候选，随后真实滚动仍遵循IsChangeOnScroll。detach清除待恢复数值。没有新API／Token／资源ID／订阅，也没有在资源回调重建控件或改写Semantic Part样式。

第一版fixture错误使用Avalonia原生ScrollViewer，Panel实际依赖产品ScrollViewer，初始offset始终0；其6项失败不计产品证据。修正fixture后有效RED为 **8 FAIL／1正对照PASS，495ms**，显式选择优先原本已正确。相同9项候选 **PASS，482ms**。补充正常滚动选择、重挂、短序列viewport变化及分数行高后 **15临时PASS，573ms**；中间与133既有时间／日期项合跑 **146 PASS，9s**，后续两项viewport用例修正为真正的短序列，另加两项分数行高。

## 实际渲染与输入

真实Skia、UseHeadlessDrawing=false、正常motion，AtomUI Window1000×800／scale1。三个TimeView分别为24时7行、12时8行、15min／30sec短步长7行；HMS隐藏滚动条但可滚动，AM/PM原Auto滚动条保留。实际TimePicker弹层四列继续同样的行高往返。三处真实wheel证明offset变化，公开SelectedTime始终10:15:30；读取真实viewport／extent／offset／clip／host，不将请求参数当实际结果。

| 对照 | 结果 |
| --- | --- |
| 同版本A/A | Light／Dark均0 RGBA差异 |
| 单变量1DIP检错与恢复 | 13,746／15,085像素改变，恢复0 |
| 默认、原有输入、恢复与弹层兼容 | 旧新18帧全部0 |
| 行高40的修复画面 | 6帧意图改变：选值26,252／26,133，浏览27,722／27,605，弹层27,032／26,939像素 |
| 当前Mac osx-arm64 NativeAOT与JIT | 24帧全部0，全部几何／输入断言通过 |

旧新完整操作轨迹相同，没有遮罩、通用容差、移动对齐或关闭motion。已查看旧新亮色行高40和新版暗色弹层：普通10／15／30保持居中，已浏览的列保留视口，AM/PM滚动条可见。原暗色header黑字等默认外观保持，本包不宣称修复其可读性。

首次AOT临时project缺少本机openssl链接目录并包含不存在的harfbuzz目录，属于驱动配置失败；修正临时LinkerArg后实际publish和两主题运行通过。最终无IL警告，5条已知Homebrew dylib最低OS linker警告单列。该证据仍为Headless/Skia与当前Mac AOT，不能代替原生最终显示面或其他OS。

## 清理与后续

删除15临时case所在1文件、driver423文件、1份确切源码备份JSON及73份媒体／专用产物，净增永久case为0，媒体Git审计无发现。清理后TimePicker／DatePicker精确过滤 **133 PASS，9s**；NET8 Desktop Release（含Core／Common）**17.97s，0警告／0错误**。成员审计为1文件／1控件0问题。只有作者自审，未使用独立评审agent。

本包不完成D2-B公开TimeViewCell Global／禁用selector、非法行高合同或D3范围两字段／其余43个日期Own字段。完整家族验收计数仍为0。
