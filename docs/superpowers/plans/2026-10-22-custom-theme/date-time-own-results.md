# CT-16 D3-B 日期 Own 默认依赖

DatePicker3、DateViewer18、Calendar11、LunarCalendar11共43个历史字段迁入DependencyDefaultsV2 Revision1。名称、类型、声明顺序、资源ID／slot和明暗默认保持。此包处理默认求值与指定依赖，不表示全部日期家族、Global、业务状态、极值或平台完成。

## 实现合同

| 终端 | 有效 Own 依赖 | 其余公式 |
| --- | --- | --- |
| DatePicker | 无新增依赖 | 保持原三个独立Global／常量公式 |
| DateViewer | CellHeight → CellRowHeight | 保持独立Global公式；PanelWidth不改成CellWidth派生 |
| Calendar | 无新增依赖 | 保持原Global／常量公式 |
| LunarCalendar | MiniDateCellSize → MiniContentHeight；SecondaryTextLineHeight → FullCellMinHeight／RangeBarTopOffset | MiniMonthCellWidth继续取Global ControlHeightLG×2，其余原Global公式保持 |

明确下游值优先，清除下游后按仍有效的上游重新求值。CellRowHeight的0为明确最小值，内容自然高度仍有效；MiniContentHeight的0为明确高度。文字LineHeight的正数合同不能由其他字段的0值推导，非法值诊断仍单列边界包。没有新增控件API、Own、资源ID、订阅或缓存。

## 失败与聚焦检查

初始13项临时验证10失败／3正对照通过（3秒）；补充自然高度日期行的真实测量1失败（682ms）。改版14项全部通过（3秒）。覆盖资源、实际Cell最小高度与自然行增长、农历Mini／Fullscreen真实展示属性、下游明确值／清除和两类合法0值。

现有模块检查首次350通过／1失败：一个农历公式测试仍调用旧整体计算入口。V2规范禁止该实例计算，测试改用生成EvaluateDefaults入口，11个原期望值保持；原用例单独通过16ms，随后14临时加337既有共351项通过18秒。未新增常驻用例。

## 实际渲染与消费

实际AtomUI Window 1600×1300、scale1、真实Skia与正常motion；固定2026-10-15，范围至19日。包含DateViewer／RangeDateViewer、普通／农历Mini和Fullscreen、Month／Year、业务范围条及实际鼠标打开的DatePicker／RangeDatePicker弹层。选值全程保持。业务数据的Background通过公开CalendarRangeBar初始化，模板节点只读观察。

同一轨迹明暗15阶段：旧版各7项失败、改版各0。86行默认资源／ID／slot逐项一致。行高上游40正确派生52，农历MiniDateCellSize增11使内容高度增66；SecondaryTextLineHeight增11使完整正文862→928，并使范围条相对所在日期行的避让48→59。绝对Y350→383包含前两行共22的增长及避让11，不能只比较绝对坐标增量。

旧新30帧中23帧兼容差异0、6帧是上述有效Own几何变化。另1暗色own-line帧底边2像素，由原版支持的MiniContentHeight=374往返完整复现；原版该状态与候选整帧差异0，原版旧mini配置也与候选新依赖的实际画面完全一致。具体原生边缘绘制函数仍归CT25，不使用mask、白名单或容差。

明暗A/A均0，1DIP检错42365／45724像素且恢复0；普通清除恢复0。月／年模式往返后原版本身有农历字形区域28／124像素变化，候选相同，旧新恢复帧逐像素0。未把既有往返差异填成原版自恢复0。

当前Mac NativeAOT实际运行全部30阶段，断言全部通过，与JIT30帧逐像素差异全部0；无IL警告，有5条已记录的本机OpenSSL／Brotli部署版本警告。它不代替原生最终显示面、其他OS或Browser验证。

## 交付边界

14项专属临时用例及驱动、媒体、源备份已清理；CT25原有重套模板临时失败仍独立保留。正式Token／实现／Changelog及43字段目录同步，四个实际依赖的消费单独核定，其余字段不因迁V2自动标为消费通过。当前V2终端17个、声明243字段，其中历史迁移218、此前Dropdown新增23与OTP动效新增2；Control目录721和Own目录1221不变。

清理后最终DateViewer／Calendar／DatePicker精确337项通过（18秒），NET8 Release构建零警告／错误（18.69秒）。5个C#文件成员审计无问题，40条相对链接有效；43字段目录更新、1221唯一键保持，暂存为空。删除14临时用例、423个驱动／构建文件、两个含8份源字节的备份及151个取证文件，审计干净。作者另步自审核对四个依赖、独立Global公式、实际布局与默认对照、已有公式测试的独立期望及清理处置；完整家族和非法边界没有填为通过。本包不关闭完整CT-16或CT-25。
