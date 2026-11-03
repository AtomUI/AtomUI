# CT15 Message 原图元差异核对

接续message-restore-audit.md未通过，不重复Own/Global/字体/GC。先只读导出当前正常Message四卡的真实Frame几何、Corner/BoxShadow/画刷/边框/字号与位置，再用无AtomUI独立官方Avalonia12.1.4工程构建相同Border几何/画刷的有限对照，分别正常入场、1DIP往返、局部颜色往返、完整背景重绘。只可证明所列图元，不用近似模型自动吞并原29/9/44/28或native3497/max2；若无复现保留反证，不猜测产品修复，不改FW1/FW2归属。

原Message采集仍920×700、四类型、zero expiration、normal motion/真实Dispatcher与>=750ms/12稳定原始帧；完整文本不删。纯图元不冒称保留Message正文/图标合同。媒体与原始JSON只放官方evidence外部临时目录，清理后精确Message门禁。无全量或提交。


首实际导出1PASS/5.2615s：四Frame分别(296,170/116/62/8)、327×38、radius8、无border、white、原三shadow。无AtomUI独立官方12.1.4原RegionDirtyRectClipping=true模型8场景（静态/fade/translate/combined×无字/原全文）每组7完整920×700帧；A/A、1DIP恢复、purple→white恢复、背景重绘均RGBA0。1DIP检错无字6584/max20、有字14984/max224。8组均为有力负对照，不能归因于孤立Shadow/文本/正常淡入或平移；无产品修复。

首纯工程translate用Animation直接给ITransform属性无注册animator，4场景完成后抛异常exit134，原29帧留本批账；改用官方TransformOperationsTransition，后四场景完整。未吞异常或把缺帧算通过。额外只读导出初误选了同命名空间的AtomUI.SelectableTextBlock（真实目标为Avalonia.SelectableTextBlock），1FAIL属探针类型错误；改全名后1PASS/5.0267s。

实际icon相对Frame为(9,9)/20×20，正文(37,8)、默认解析Helvetica。继续独立复刻四个公开glyph路径/原DrawGeometry调用及共享几何transform算法，仅测试独立绘图，不引用AtomUI程序集，不删原Message合同。此复制只作外部临时诊断，验证结束删除；不声称其已经是完全同产品树/完整Message兼容前证。


加入四个实际glyph路径、原渲染缩放/共享Geometry transform算法后，独立8场景仍全部A/A/1DIP恢复/改色恢复/完整背景重绘0；1DIP分别无字7081/max207、有字15465/max224。只有入场/图元层级尚与产品不同，不能按此负对照声称Icon缺陷或框架根因。正在进一步用外层opacity+内层ContentPresenter原64平移的同几何模型；仍不改产品、依赖或FW2范围。


关键纠正：纯Control默认ClipToBounds=false，真实Icon继承官方TemplatedControl默认true（12.1.4 TemplatedControl静态默认）。未裁剪模型child画刷往返在正文首字坐标(333/334,20..187)产生16/max1，单变量改本地Rect或恢复实际Clip=true均消除；该模型反例不能代表默认Message。真实Icon默认裁剪三检查3PASS/9.7902s，因此没有修改Core Icon。额外显式Clip=false/Orange组合3项2PASS/1FAIL仅证明此非原Message条件的背景越界，保留诊断，不据此重开CT12或归FW1/FW2。

模型精确配平Helvetica/LineHeight22/真实Clip、20×20/(9,9)与正文(37,8)后，plain/combined两完整920×700默认图和实际Message当前默认图均RGBA0；glyph往返/重绘也0。原8层级模型、8稳定锚点模型及对应负对照保持账本，不能将单独图元通过当产品恢复通过。

继续自然测量/重新居中与正文颜色/字体往返。首自然模型674/max235默认差异来自clone缩放忘记Deflate(Margin)，不是产品RED；已按真实Icon.Render公式修正、坏批独立命名，必须先证默认整帧0再解释恢复数值。


## 本轮结论与保留

当前有56组有效独立图元场景（按实际已收齐文件分组计数，不使用早期程序硬编码“8 scenes”日志），2组坏margin模型另账。默认整帧匹配模型在plain/combined下均与真实920×700默认全RGBA0；自然测量、bounded父尺寸、templated glyph/独立IconPresenter层级、真实角色中间值（23/Menlo/LineHeight31、glyph26、Padding4.5）模型均恢复0。自定义帧与真实scope自定义图仅余11/max23，范围(147,35)-(777,74)，未复现真实残留的完整历史；不能因此认定独立框架根因。

真实四算法资源往返各严格FAIL（28.3024s），A/A均0，恢复Default5/max11、Dark10/max33、Compact9/max12、DC28/max33；最后完整背景重绘相对初始分别7/max15、8/max16、13/max29、0。另Light只读角色采样（命令10.9022s）恢复12/max15；节点采样9.6116s仍严格FAIL，四Card/Frame/Caption/Icon在initial/custom/restored的实例标识逐项全同。所有角色值/几何恢复，模板换代未发生。采样日志中的namespace/错误Caption类型及无注册transform animator失败分账；没有吞错/Skip或将负对照当产品PASS。

已查看完整原Message/Global Menlo23图、纯模型图及完整放大差异图（x20仅用于观察，数值仍原始同坐标RGBA）。没有编辑产品源码、Core Icon、宿主开关、官方依赖、FW1/FW2归属或容差。原P1/旧restore/native3497仍保留，本新四路径不倒写历史。本轮没有进行新的AOT：没有得到与真实默认/资源历史一致的独立框架反例，也没有产品改动；既有managed/AOT恢复失败证据继续保留。下一应针对资源发布/损伤记录而非按猜测打重绘补丁。

临时当前10case/2源14466字节（导出/四配方/两读值诊断/三Icon边界），媒体及外部工程待按官方工具清理；清理后仅Message精确门禁。


清理结果：当前10临时case/2源14466字节已删除；官方evidence cleanup clean/findings=[]、removed=true/550文件。外部111会话文件569551767字节（含独立工程/原失败与坏模型日志）及marker删除。清理后实际重建MessageSemanticPartTests13PASS/0Skip/3.7454s；首namespace误写Feedback而Padding实际为FeedbackStack，未把未匹配项计入，随后同一重建程序集精确Padding4PASS/0Skip/3.1545s，合计17不同既有用例。未改产品源码，复用相同最终源码NET8零警告零错误16.76s。最终helper/marker随读取清理，无新常驻测试、无全量或提交。

作者另步自审：纯模型默认图0只用于确认图元等价，独立裁剪错误已纠正并保留反证；不将未复现产品历史的模型解释为框架修复。原真实恢复仍FAIL，当前无新视觉批准或FW2归属请求，完整Message/CT15保持开放。
