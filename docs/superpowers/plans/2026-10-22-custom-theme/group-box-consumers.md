# CT15 GroupBox 只读准备（2026-11-01）

Descriptions三版采样持有共享锁期间，仅做后续源码准备，不并行构建或修改GroupBox。输入1metadata/7Own，仍在137/274内。当前全部Own NeedsDecision3仅本组：TextPaddingInline、OrientationMarginPercent、VerticalMarginInline，尚未裁决，不自动标Reserved。

源码核对：三字段只见定义与默认，分别1.0(em)、0.05(比例)、Global.UniformlyMarginXS；未见消费者。其余四字段ContentPadding、HeaderContainerMargin、HeaderIconMargin、HeaderContentPadding有独立实际几何语义，不能把旧scalar强接到现有Thickness或擅自改变Header gap公式。先取得四算法28条默认/type/key/slot与三字段0/非零配置实际无效的运行前证，再提出保留兼容字段的具体裁决，保留删除/版本化启用等替代方案与重新启用条件，等限定批准后同步分类。

实际Root/Header/Icon/Body、标题位置、字体/Global/Own/原生/clear、专用Semantic Style、动态资源/布局与Header gap测量/重模板/重挂仍需逐目标验证。保留原内容、固定预算、裁剪、原生焦点/键盘和既有Shape，不因无scalar消费者把完整家族写通过。

复用现有控制主题/资源与CT11/已关闭家族机制，不改CT12/13/14批准或CT25/FW1原始边界，不换官方依赖、不扩大像素容差。临时先RED、正常Skia旧新/AOT同历史、校准/恢复/持续态与原失败留账；判断后清理，清理后精确模块/NET8、正式文档/CSV/总表/作者自审和具体限定视觉/分类审查。不全量/提交/新分支/worktree/子agent。


清理后最终验收（2026-11-01）：共享锁空闲独占后，GroupBox既有19PASS/0FAIL/0Skip（用例581ms，命令26.97s）；NET8 Desktop Release 0警告/0错误（构建18.08s，命令18.25s）。23临时用例/3源19113字节、545取证、414driver/15publish及503原会话文件已删除，6最终文件和两个marker也清理，audit clean、git diff --check通过。三版164静态/cross0，68主恢复0；Global恢复4/7/9/9 max1保留。仅40明确静态及三Reserved兼容提案待限定批准，NeedsDecision仍3，原裁剪/低对比/平台/完整出口及CT15保留；CT12/13/14和CT25/FW1不改，不提交。
