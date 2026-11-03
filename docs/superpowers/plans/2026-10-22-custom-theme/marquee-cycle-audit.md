# CT15 MarqueeLabel 正常时钟整周期核对

复用原2metadata/2Own/8对批准与22项/参数/hover/字体/释放证据，不重做默认迁移。原DefaultSpeed150、CycleSpace200及4×distance/speed时长语义保持，0速度原异常不改。本轮只补正常真实时钟两完整循环、offset有限/范围/回绕周期、原220×28裁剪和隐藏暂停/恢复。

真实Skia/UseHeadlessDrawing=false、normal Dispatcher和render tick，不冻结/替换clock，不用任意相近帧作逐像素跨运行兼容。每20ms采样实际PivotOffset与渲染帧，回绕时间误差限由实际最大采样间隔给出（时间采样误差，非像素容差）；保存必要整图检查。default150及公开native450两组，完整长文不变。临时验证及媒体完成后清理、精确模块；未取得失败不改产品。


## 正常时钟结果

两临时theory全PASS（41.97s命令），实际cycleWidth=489.5。speed150原4×公式期望13053.333ms，回绕12815.932/25869.750ms，相邻周期13053.818ms；最大采样间隔43.589ms、27个独立阶段hash。native450期望4351.111ms，回绕4093.481/8467.560ms，相邻4374.079ms，最大gap38.428ms、10个hash。实际offset始终有限且在[-489.5,0]（只作double算术epsilon1e-7边界，不是像素容差）。

同一临时场景生成外部独立小型断言驱动，官方Headless/实际Skia+Desktop源码引用，正常clock不冻结/替换。当前Mac NativeAOT2874检查0FAIL、exit0；期望相同，speed150回绕12792.585/25840.413ms、周期13047.828ms/maxGap53.680ms/hash27；native450回绕4088.446/8461.676ms、周期4373.230ms/maxGap42.491ms/hash9。周期误差界仅用实际最大采样gap×2+40ms描述采样时间，不作为视觉容差。

两运行每组均完成隐藏200ms稳定后600ms实际offset不变，显示恢复后实际offset再次变化；原Text、220×28 Bounds和ClipToBounds=True保持。已查看正常回绕完整600×100图，文字仍沿原视口裁剪，不删长文/加背景绕过命中或改速度语义。没有把不同时钟阶段的图像比较为兼容0；原48旧新/55跨运行持续态非零仍保留，0速度原异常与4倍语义不改。

AOT21281696字节，SHA256 86fdd3c8dab8d2674520aa472f3adf9d0fe5573d0565080f9b5a5a4404346aa9，零IL/CS warning，五条既有Homebrew linker最低OS warning单列。无产品源修改，没有新独立公开Part。临时清理与清理后精确模块正在执行，不把本两组当作整个Marquee/CT15完成。


## 清理后验收与作者自审

2026-11-01 Marquee正常周期补证：原220×28/Clip/全文及4×时长保持，default150/native450两组2临时PASS；AOT2874检查PASS/exit0，各两次实际回绕与offset有限/范围、隐藏600ms冻结/显示恢复通过。default实测周期13053.818ms，AOT13047.828ms；native450实测4374.079ms/AOT4373.230ms，期望13053.333/4351.111ms，误差界来自实际采样间隔，非像素容差。不冻结clock或作跨时相RGBA0声明。2临时/1源4618字节、16取证、303会话448595997字节已清理audit clean；最终精确原Marquee显隐模块1PASS/0Skip25ms（本轮不是旧10项），产品未变复用NET8零警告19.98s。原48/55持续态非零、4倍语义/0速度异常、平台/GC与完整CT15保留，见marquee-cycle-audit.md，不提交。

所有临时进程退出，最终run.py/marker在门禁后清理。原精确filter同时写了未匹配的Alert namespace，但实际运行只有既有Marquee显隐1项，不报旧10项。无产品源变化、不添加新常驻测试或全量。作者另步自审，非独立审查；正常周期正确性与不同时间原点像素兼容分别记录，不扩大既有批准。


最终结果：六不同场景Debug均PASS（原四12.4786s/新增二8.5300s），Release均PASS（四13.0868s/二7.4010s）；没有产品反例或源码修改，不把重复运行计为更多用例。6临时case/1源5368字节与7会话文件25204字节已删除，无媒体。清理后实际重建精确Statistic5+QRCode8+Empty13+Spin16+Marquee1共43PASS/0Skip/3.8264s；最终产品源码未变，复用Drawer最终NET8零警告零错误16.76s。git diff --check通过。作者另步自审确认provider/window保持存活与NoInlining观测，没有强持有被测对象；只补本六条对象退出，不关闭其他恢复/持续态/平台/极值或完整CT15。
