# 第二十一批：Segmented默认依赖与第二十轮结果

当前基线6bd6076760e98bdf26f79260907df5fa105215ea，仍在现有feature/custom-theme与checkout。本批14文件尚未提交，拟一个提交、推送既有分支，用AtomUI凭证运行三OS／两Browser验证Actions。第二十批授权已执行，本片设计／视觉批准不替代Git批准；不创建分支／worktree，不发布NuGet。

## 审查范围

- [第二十轮实际结果](platform-twentieth-results.md)：8个job均成功，三OS各6405／6405选测、三NativeAOT各26项、两Browser各23项，源码均为6bd607676；各370资产、27对本地化阶段完整。只记录实际结果，没有改工作流、超时、断言或发布设置。
- [已批准Segmented设计](segmented-defaults-design.md)与[本机结果](segmented-defaults-results.md)：产品只改SegmentedToken.cs，14项V2默认，三条自动高度依赖有效TrackPadding并取0下限；明确下游及0、自然测量保持，控件／共享实现／AXAML全部不变。
- 原版五项临时检查4 FAIL／1框架约束PASS，候选5 PASS；28条标准默认不变、280条metadata／key／slot相同，仅18条Own依赖值预期变化。原版与候选各96个输入场景、288次鼠标及192次实际方向键通过，命中当前真实item边界及事件到达均验证。
- 120正式帧40组1DIP全检出、12恢复0；所示四阶段布局变化及[四对精确14点历史差异](segmented-defaults-noise-review.md)已于2026-10-26独立批准，原始FAIL保留。两版216诊断帧的12组新旧直接Skia、12组全节点重绘及候选4组直接A/A全0；223节点输入逐项相同，但未稳定重现历史14点或证明每次内部原因，没有通用容差或产品重绘补丁。
- 当前Mac实际NSWindow／scale2 managed1037／NativeAOT1038检查通过、336资源记录一致；AOT动态代码禁用。链接本地macOS26 dylib与最低链接12的警告保留，不声称macOS12或原生最终像素／输入验收。
- 四临时C#、339媒体文件及419文件／1,376,531,949字节原生工程已清理，原生进程结束、媒体审计clean，常驻测试净增0；清理后原Segmented30项、0Skip通过，NET8 Release零警告／错误、单Token成员审计无问题。视觉批准与媒体清理后产品SHA保持，沿用相同输入的聚焦证据。
- 正式四文档同步14默认／三依赖、0下限、自然测量和生成注册，明确SegmentedItem独立Global／无Own／显式复用Segmented Own；14Token／2Control目录回填，Reserved仍20。

## 精确14文件

- `src/AtomUI.Desktop.Controls/Segmented/SegmentedToken.cs`
- `docs/controls/desktop/data-display/segmented/overview.md`
- `docs/controls/desktop/data-display/segmented/implementation.md`
- `docs/controls/desktop/data-display/segmented/token.md`
- `docs/controls/desktop/data-display/segmented/changelog.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/segmented-defaults-design.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/segmented-defaults-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/segmented-defaults-noise-review.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/container-consumers.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/control-inventory.csv`
- `docs/superpowers/plans/2026-10-22-custom-theme/token-contract-review.csv`
- `docs/superpowers/plans/2026-10-22-custom-theme/tasks.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-twentieth-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-twenty-first-followups.md`

作者另步自审核对14公式／3依赖、资源身份、输入、原始像素失败和清理，不声称独立评审。完整家族、N2／N4、其余Global／字体／输入／持续动效、其他Host及原生最终像素仍开放；排除待批NavMenu产品方案、后续研发、所有临时源、媒体、外部工程和构建产物，本批Git另审。
