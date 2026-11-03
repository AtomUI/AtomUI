# CT15 Descriptions 只读准备（2026-11-01）

Collapse正在执行本包验证；本文件不启动并行构建或产品修改。原计划输入6metadata/10Own，属于137/274：Descriptions、DescriptionItem、DescriptionDefaultItem、DescriptionBorderedCell、DescriptionBorderedItemLabel、DescriptionBorderedItemContent。DescriptionItem / DescriptionBorderedCell两项metadata的身份/归属未决要按源码确认，不等于Own新增未决；Own当前3个NeedsDecision仍仅GroupBox。实际DescriptionItem是带既有GenerateScopedResourceHost的non-Visual AvaloniaObject数据，BorderedCell是内部ContentControl共享基类；无独立片段本身不判缺陷，须核对生成scope attachment/退役及公开owner真实消费，不盲目增加注册身份。

DescriptionsToken10字段：LabelBg、LabelColor、TitleColor、HeaderMargin、ItemPaddingLG、ItemPadding、ItemPaddingSM、ColonMargin、ContentColor、ExtraColor。默认依赖均为原Global表达式，先冻结四算法40条默认/type/key/slot，保留真实SizeType的Small/Middle/Large三档、默认/有边框布局和冒号语义，再逐实际目标验证V2R1、字体/Global/Own、原生边距/字体/尺寸及clear。此公开API使用SizeType，不新增不存在的Custom枚举值。

原Header/Extra、Label/Content、原数据和Span/列数、横纵布局、RTL/Compact、嵌套内容、长文裁剪/滚动、动态集合/模板/重挂及资源退休不能为便于验证删除。所有Semantic Part定制用生成专用AXAML Style，命中失败先修实际route，不用节点设置回退。归属复用已批准家族机制但不扩写CT12/13/14结论，CT25/FW1原始问题保留。

先取有效RED及明确UX，再小范围修复；正常Skia旧新/AOT完整同历史，独立A/A与1DIP检错，异常/缺帧/非零留账。媒体与驱动仅临时目录，判断后清理，清理后精确Descriptions模块/NET8；同步正式文档、CSV、总表和作者自审，最后提出具体限定视觉审查。不自行全量、不提交、不建分支/worktree、不用子agent。


清理后最终验收（2026-11-01）：共享锁空闲独占后，Descriptions既有33PASS/0FAIL/0Skip（用例923ms，命令28.04s）；NET8 Desktop Release 0警告/0错误（构建18.03s，命令18.22s）。48临时用例/3源24053字节、1033取证、415driver/两publish各15及569原会话文件已删除，6最终文件和两个marker也清理，audit clean、git diff --check通过。主164静态cross0、56+4往返0，独立可见Native44cross0；原缺帧/异常及无模板探针/校准失败留账。仅16明确静态待限定批准，原520宽/三列/长文Wrap及窄槽、越窗/低对比、持续态/平台/完整出口与CT15边界保留；CT12/13/14与CT25/FW1不改，不提交。
