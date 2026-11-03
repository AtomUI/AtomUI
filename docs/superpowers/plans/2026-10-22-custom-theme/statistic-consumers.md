# CT15 Statistic 消费者分包（2026-11-01）

Empty 采样冻结期间只读准备。输入为 AbstractStatistic、Statistic、StatisticCountUp、TimerStatistic 四 metadata 与 StatisticToken 的 TitleFontSize、ContentFontSize 两 Own；保持 137/274 分母。先核对运行注册身份和主题继承，不给派生类型伪造新独立 fragment。

原默认分别取 Global.FontSize 和 FontSizeHeading3，冻结四算法八条 type/key/slot/default 后迁移 V2 revision 1。实际目标是 HeaderPresenter 和 ContentLayout/ValuePresenter，以及原 prefix/suffix 与 Icon。逐项检查 Root/标题/内容字体与前景优先级、原生字体、专用 Semantic Style 的真实命中和清除；保留数值格式、精度、前后缀、loading、计时方向/刷新/结束与持续动效。固定值静态与计数/计时持续态分开采样，不暂停动效取得假静态。

沿用现有主题机制；可由 AXAML 表达的绑定先放 ControlTheme。计时/动画订阅须在重模板、卸载和重挂时有明确退休与恢复路径，保留原始业务内容及输入合同。临时验证先 RED，执行正常 Skia 旧新/AOT 同历史、独立校准和恢复；判断后清理，再精确模块与 NET8 验收。同步正式文档、CSV、总表与作者自审；需要视觉批准时提供具体限定表。CT12/13/14、CT25/FW1 与其他未通过项不改，不提交、不建分支/worktree、不用子 agent、不跑全量。


清理后最终验收：共享锁独占下精确StatisticSemanticPartTests（Debug/net10）5PASS/0FAIL/0Skip（416ms，命令26.96s）；NET8 Desktop Release 0警告/0错误（18.08s，命令18.31s）。30临时3源22642字节、641取证、534原会话已删除，独立最终会话4文件与marker记录后删除。正式五文档、CSV4metadata/2Own、四总表及作者自审同步；12明确静态仅待限定视觉批准，原44失败、72旧新/46cross持续态、字号边界、平台/GC/极值与完整CT15保留，未提交。


阶段证据：[结果](statistic-consumers-results.md)、[12对限定视觉待审](statistic-consumers-visual-review.md)、[作者自审](statistic-consumers-review.md)。前述只读准备随后进入已授权工程；本包不关闭完整家族/CT15，下一QRCode。
