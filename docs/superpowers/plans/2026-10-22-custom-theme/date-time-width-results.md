# CT-16 D1 时间列宽度与 footer 消费结果

2026-10-24，在现有 feature/custom-theme 工作目录执行；无新分支、worktree、提交或子 agent。D1 完成，CT-16 其余子项及完整家族验收继续，见 [执行计划](date-time-consumers.md) 与 [总状态](tasks.md)。

## 实现与失败证明

TimePickerToken 的8个历史字段迁至 DependencyDefaultsV2 Revision1，保留名称、类型、ID、slot和原默认。PeriodHostWidth 依赖有效 ItemWidth，ItemPadding 依赖有效 ItemWidth／ItemHeight，下游明确配置优先。TimeView 的 AM/PM 列消费 PeriodHostWidth；TimePickerPresenter 的 footer 消费自己的 ButtonsMargin，修复原先借用 DatePicker.ButtonsPanelMargin 的所有权错误。DatePicker 自己的 footer 保持其原合同。没有新的 public API 或字段。

有效 RED 为6项中5 FAIL／1默认正对照 PASS，1s：Period84实际56、显式Period84被Width92覆盖、Width92／Height40的Padding应26而实际14、TimeButtons19实际8、DateButtons31泄漏到时间footer。候选6项通过后增加兄弟／重挂和长寿命provider下3个WeakReference释放检查，最终8临时项与133既有TimePicker／DatePicker项共 **141 PASS，10s**。无新增永久测试。

RangePickerArrowMargin／RangePickerIndicatorThickness 仍没有实际消费者；只验证了其默认兼容，不将它们写成有效调节项。后续D3核定与InfoPickerInput共用输入的优先级。公开 TimeViewCell、内部 TimeView／TimePickerPresenter／DateTimePickerPanel 的文档边界已修正，不增加虚构的模板部件。

## 渲染与原有外观

真实Skia、正常motion、UseHeadlessDrawing=false，AtomUI Window 1660×800／scale1；保留12／24小时、header／footer、7／8行、循环列、AM/PM非循环和原滚动设置。打开实际 TimePicker、RangeTimePicker、DatePicker、RangeDatePicker（后两者IsShowTime=true）四种popup组合，实际host均为AtomUI Window，不能据此声称原生PopupRoot通过。真实wheel改变offset且不提交时间，原公开日期／时间保持。

明暗8默认／ID／slot共16行在原版、新版、重复进程与NativeAOT完全相同，各主题表SHA256均为 `d6c6ca39c9881d07093f15b09420cabd8f4e011b1e96d0724cfbb81dce0afd4d`。同进程A/A为0；1DIP检错为Light35,616／Dark35,554像素，恢复0。旧有Width72／Height32／Period72／Padding6等有效配置继续兼容。新Period84／footer19／有效依赖的画面变化与旧有配置对照分别记录。

原版和新版跨进程的范围输入左下圆角出现3–4个RGBA像素变化。没有应用遮罩、容差或位置对齐：三次确切原源码重放证明原版自身也出现该变化；另找到一组跨源码完整画面0差异。两种范围输入的实际582／1150行几何与绘制参数逐字一致，包含Bounds、DesiredSize、变换、裁剪链、焦点、DashedBorder实际圆角6／描边1／画刷及透明度／背景模式／shadow／dash。结论限定为原生圆角绘制状态的证据推断，尚未证明具体Skia函数归因，继续CT-25。

最终当前Mac osx-arm64 NativeAOT启动并执行62帧：与JIT比较56帧0，余6帧均为上述范围输入圆角4像素变化（Light范围popup／AA、日期范围popup／AA；Dark日期范围popup／AA）。同一次进程的8组popup／AA均0。不能写成62帧全部0或完整家族视觉通过。无IL警告，5条已知Homebrew最低OS linker警告单列。已查看默认、日期组合popup及新配置画面。

## 清理与最终验证

删除8项临时case所在1文件、外部driver423文件、原版／候选各3文件的源码快照JSON、401份媒体及专用产物；媒体Git审计无发现。清理后精确TimePicker／DatePicker过滤 **133 PASS，8s**；NET8 Desktop Release（含Core／Common）**18.50s，0警告／0错误**。数字与几何证据保留于 `.superpowers/sdd/date-time-width/`，无图像编码。

本包只有作者自审，未使用独立评审agent。动态行高、TimeViewCell禁用selector／Global、范围历史两字段及其余43个日期Own字段继续D2／D3；当前Mac Headless/Skia与NativeAOT不能替代其他平台或原生最终显示面。
