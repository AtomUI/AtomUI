# CT15 Flyouts 作者另步自审（2026-11-01）

本记录为主 agent 另步自审，用户禁止子 agent，未委派或建立独立 checkout。输入4metadata/11Own，限定工程切片，不代表完整家族或CT15关闭。

- 两Own V2R1保留11字段/44四算法默认、类型、key与slot；没有增加公共token或改官方依赖。
- FlyoutPresenter保留共享Arrow StyleKey；仅真实Host逻辑祖先存在时建立Style优先背景lease，attach替换、detach释放，无Host无虚构identity。原生/四专用Style实际覆盖与clear已验证。
- Pointer offset仅Pointer消费，普通placement归零；target切换先关闭旧生命周期，再订阅新scope，避免旧关闭回调误释放新lease。Native/clear/重开/双scope验证通过。
- Tree背景从根进入既有TemplateBinding，原null→Own有效值纠正必须待审。尺寸、内容padding及全部40原行保留，233高限制内增加真实ScrollViewer并透传原滚动选项；竖bar可见、真实wheel及末端不越界有效验证通过。没有移除约束或内容来规避裁剪。
- 原TreeViewItem.Click触发但Presenter事件0的有效RED已修为冒泡class handler；无逐项MenuItem递归delegate。原树容器准备保留。
- 成员纯整理14源/7注册类型37→0，纯阶段指纹、注册数组、静态与实例字段顺序保持；后续行为改动单列，不混称纯移动。原BOM span错误由只读审查捕获并还原。

逐帧另步审查发现两个有效缺口并修复：打开中的实际Popup offset原来滞留23，Native Flyout41没有回放；pinned Host重模板后IsOpen=false、图中弹层消失。两个offset RED和一个重模板RED取得后，分别在现有OnPropertyChanged回放当前Popup、OnApplyTemplate复用QueuePinnedOpen。运行时Popup超出ControlTheme链的理由已写注释，不换内容或绕过焦点，原scroll/500偏移保留。

最终临时阶段40PASS/0FAIL/0Skip（23临时+17既有，37s），包括两个live owner下关闭后的弱引用PASS。末次成员14源/7控件0问题。候选/AOT各196帧/144检查PASS，180静态跨运行全0，12定时非零保留。旧版144主帧/100检查40FAIL、缺52帧及四次原异常全部留账；独立16cal不填补主历史。48明确静态全部跨运行0，其他4微差/12定时不申请。四组主预期恢复各15，共60比较56零/4 micro；点击后选择和指针位置不同的tree-reopen不伪称恢复断言。

已看13张四算法状态对照表、关键全尺寸帧及四张三版motion过程表；表仅辅助观察，量化均为800×600同坐标完整RGBA零容差。Tree默认窗口上缘裁剪、暗Own浅绿/白字及Compact offset8→4列明；Host采样正文在Offset500视口外，字体由有效值验证，不夸大glyph视觉覆盖。23临时/3源25325字节、1191取证、320driver/两publish各15及488原会话文件已清理，audit clean。

清理后精确17项/NET8及最终CSV/总表同步完成（详见文末），48对待限定批准。OS/Browser真实输入、完整GC/焦点/平台/极值出口及持续motion时相未由当前Headless证据关闭；CT12/13/14批准及CT25/FW1原始边界保持，无提交授权。


清理后最终验收（2026-11-01）：共享锁空闲后独占执行，Flyouts既有17PASS/0FAIL/0Skip（用例760ms，命令31.37s）；NET8 Desktop Release 0警告/0错误（构建19.83s，命令20.01s）。23临时/3源25325字节、1191取证、320driver/两publish各15、488原会话文件已清理，最后6文件及final marker也删除。git diff --check通过。48对及列明有效默认纠正待限定批准，原40失败/52缺帧、4恢复micro/12跨运行定时、平台/完整出口及CT15保留；不改CT12/13/14批准及CT25/FW1，无提交。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。
