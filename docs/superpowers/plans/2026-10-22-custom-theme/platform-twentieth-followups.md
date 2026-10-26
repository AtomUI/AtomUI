# 第二十批：Expander 默认函数与第十九轮实际结果

当前基线0b5e70c71e7580572364a28c9e2b5fec82adc978，仍在现有feature/custom-theme与checkout。本批13文件尚未提交，拟一个提交、推送既有远端分支，并用AtomUI凭证运行三OS／两Browser验证Actions。第十九批授权已执行，不覆盖本批；不创建分支／worktree，不发布NuGet。

## 审查内容

- [第十九轮实际结果](platform-nineteenth-results.md)：8/8job成功，三OS各6405／6405选测、三NativeAOT各26项、两Browser各23项实际运行检查，源码均为0b5e70c71。各OS的370主题资产及27对本地化阶段完整，损坏feed的exit1属于原有预期反例。没有改工作流、超时、断言或发布设置。
- [Expander执行清单](container-expander-defaults.md)与[本机结果](container-expander-defaults-results.md)：产品只改ExpanderToken.cs，13项常量／Global-only默认接入V2；字段顺序、名称／类型、原公式、identity／key数值／dense slot及明确值优先级保持，没有Own依赖。Expander.cs与AXAML完全不变。
- 原版有效V2合同1项失败、明暗消费2项通过；候选3项通过，扩充13Own后原版两项兼容仍通过。明暗26默认及182条原始／投射资源、类型和身份记录新旧相同；ContentPadding水平常量16、实例padding的明确0／清除、Ghost实际背景及重挂保持。
- 正式96帧保留产品Window、真实应用明暗、四方向／四尺寸、两图标位置及默认motion；32组A/A、12组恢复、48组新旧全0，32组1DIP全检出。关键画面已查看，未新增视觉变化或借用其他控件的有限噪声批准。
- 当前Mac真实NSWindow、scale2：managed245／NativeAOT246项通过，182条资源记录相同，AOT禁用动态代码。保留本地macOS26 dylib与最低链接12的警告，不声称验证macOS12、原生最终像素或原生输入。
- 两份临时C#、97媒体文件及419文件外部工程已清理、原生进程结束，媒体审计clean，常驻测试净增0；清理后原ExpanderControl40项、0Skip通过，NET8 Release零警告／错误，完整Token成员审计1文件／0Control／0问题。
- 正式四文档同步有效Global／V2、常量16及Ghost消费者，纠正旧ScopeProvider构造描述。13Token／1Control回填，两个垂直margin仅保留兼容，容器Reserved累计20；不新增垂直图标布局。

## 精确13文件

- `src/AtomUI.Desktop.Controls/Expander/ExpanderToken.cs`
- `docs/controls/desktop/data-display/expander/overview.md`
- `docs/controls/desktop/data-display/expander/implementation.md`
- `docs/controls/desktop/data-display/expander/token.md`
- `docs/controls/desktop/data-display/expander/changelog.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/container-expander-defaults.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/container-expander-defaults-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/container-consumers.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/control-inventory.csv`
- `docs/superpowers/plans/2026-10-22-custom-theme/token-contract-review.csv`
- `docs/superpowers/plans/2026-10-22-custom-theme/tasks.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-nineteenth-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-twentieth-followups.md`

作者另步自审核对13公式、真实消费者、原始比较及生命周期证据，不声称独立评审。完整Expander、其余Global／字体／输入／连续动效、N2／N4、各Host与原生最终像素及完整CT11–CT19／CT25仍开放。排除待批NavMenu产品方案、后续Segmented研发、全部临时源、外部工程、媒体和构建产物；本批Git需单独批准。
