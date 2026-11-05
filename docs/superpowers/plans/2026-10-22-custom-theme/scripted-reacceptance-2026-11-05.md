# CT20–CT23 脚本视觉复验（2026-11-05）

## 本轮契约

用户要求复跑本批已完成任务。当前feature/custom-theme / HEAD35f9cc8f5，保留当前未提交源码、暂存区和所有原文件；不重开CT11–19，不改产品/依赖，不创建分支/worktree/subagent，不提交。

- AC20：14个真实Custom root明暗四档、root限定普通Style17,9/13/23及撤回；Split总宽140→180→140与同高；Numeric Input/Spinner的Height/Max24/Padding0预算；Space17/19与Drawer实际220及切档/撤回。来源为CT20已批准V1–V4，不替代G1全局类型Style原历史。
- AC21：实际Header owner颜色/标题、Code13→17及语法设置保持；SemanticPreview850横排/800堆叠与密度、产品owner隔离；Shell真实footer/owner与撤回，均明暗。来源为CT21已批准T1–T3。
- AC22：8配方×明暗，实际Button/Small/Disabled、保留输入/选择、Alert叶子颜色、日期/颜色/Grid数据；完整JSON读取前后；默认Overlay与当前Mac NativePopup，shadcn/Bootstrap真实Headless hover/pressed。
- AC23：真实工作台鼠标点击、无效字段保护、隔离预览、字段继承、应用、diff/配方恢复与保存状态；独立A/A、1DIP及复位检错；原实例保持。

同一临时参数化场景源同时服务Headless Skia与Mac managed/AOT，采样只写官方系统临时run。使用真实DispatcherFrame/Timer推进动效，不用关闭Motion规避。必需断言失败记录FAIL并返回非零；其他独立场景继续执行，准备错误与产品失败分账。

本轮没有保留旧二进制/媒体，不把当前A/A与完整往返当旧版本前后比较；精确RGBA零容差。当前即时RTB、Headless合成器帧、Mac实际窗口/布局分别记账，不冒充原生最终屏幕像素。G1、FW1/FW2、其他OS/Browser、CT24及G2继续沿原权威边界。

## 重放入口

临时精确filter `FullyQualifiedName~Temporary.ScriptedReacceptance`，项目tests/AtomUIGallery.Tests；测试和媒体在验收/审查后删除，净增常驻0。构建与采样沿RunLock(.artifacts/verification)互斥。系统临时run scripted-visual-acceptance-tg4jr2kk；原有已删除驱动没有恢复。


## 首轮与夹具前置

第一轮Headless实际456检查，CT22/CT23通过，CT20预算与CT21宿主前置失败；14root明暗四档与root限定Style的采样/断言有效。Numeric漏设原Custom（Middle保留32预设，额外不匹配失败保留）；修正为Custom后两模式24预算通过。Shell缺ReactiveUI激活fetcher，按真实宿主UseReactiveUI初始化后通过。额外Drawer几何从公开GetCrossRoots读取，不误把代码创建的容器当Drawer视觉后代。修正后的59检查/CT20-Budgets、CT21、CT21-Surfaces通过。

修复Split前同一参数驱动Mac managed/AOT各518检查通过，实际WindowImpl/Native PopupRoot已核对；该结果仅属于当时有限场景，不包括随后用户指出的主图标+长文本重叠。原生截取为实际树即时RTB，不是最终屏幕。

## 用户补充：SplitButton文字/图标重叠

此前脚本的Split内容为短文字Split、无主图标，覆盖不足，未用其通过否定用户现象。补Gallery实际标签Custom，与主图标、Custom、Font23、Padding17,9、Width140/180单变量核对：最初4个短标签组合通过；Custom+图标+140有效FAIL。primary分配92宽，文字Bounds40而TextLayout.Width80.96，从x35绘制到115.96，次按钮图标从108开始；文字实际越界7.96。其他前置和失败均保留。

根因是隐式单行文字未配置宽度受限时的呈现策略；主次Button边界本身未重叠。仅在Split主动作ControlTheme增加继承的TextBlock.TextTrimming=CharacterEllipsis，没有缩字、缩图标、扩大140宽或重写节点属性。相同8组合通过；Custom+Icon140实际文字39.606（可用40），图标14保持。自然宽度与140→180→140→清除Width使用同一实例继续验证。8个明暗四档默认对照全部RGBA0。

该修复是用户本轮指出的实际问题，不回写旧通过范围，不归并G1/FW1/FW2；原全局Styles.Remove失败边界保持。正式Split overview/implementation/changelog已同步，临时去留仍为全部删除、常驻净增0。


## 修复后范围验证与视觉观察

- Split重叠原8组合修复后通过；自然宽度→140→180→140→清除Width同实例检查通过，字体23/留白17,9/主图标14保持，受限文字省略且不侵入次动作；自然宽度恢复全文。当前Mac managed/AOT各163检查0FAIL，实际WindowImpl后端；本轮修复后AOT SHA256 `30101440fbb0c3d0d8b634cfd74cbe19393644c522f7250520ded616f72be167`。只重跑受影响Split范围，其他batch证据保留其实际源码阶段。
- 即时RTB实际查看：主图标、C…与次图标分开，恢复自然宽度为完整Custom；managed/AOT实际树图一致方向。8对Split原明暗四档默认严格RGBA0，保留旧默认；原G1不包括在内。
- 整批当前对照53对中52对RGBA0：26个C20非循环root恢复、8个C21表面/字体恢复、16配方完整JSON重读、Workbench A/A及恢复。1DIP正对照76203像素/max234，符合检错目标；不把它算失败或放宽容差。
- 已查看关键C20Split/Numeric24/Space/Drawer、C21明暗Header/Code/Preview800与850/Shell/卡片Badge与Zoom、配方/工作台及修复后Split图。内容、边界和既定滚动限制保留。Spin是有意循环动效，只验证实际44/14几何/资源撤回，不把不同相位的图当静态零差异；Semantic Preview本轮验证布局/owner，未驱动完整Part检查器的选择/高亮交互，不冒充该交互通过。
- 本轮没有新增原生最终屏幕抓取、其他OS/Browser、OS文件选择器或G1完整历史验收；FW1/FW2/G2与旧严格FAIL继续保留。ReactiveUI的IL2104/IL3053及5条Homebrew部署版本警告未抑制，未改依赖、缓存或native。

## 清理与最终精确门禁

所有临时来源、媒体、专用驱动/发布产物按本次run清理；正式产品仅SplitButtonTheme新增3行Setter，另同步3份正式控件文档。最终数字在此追加，不把清理前临时用例或局部native数值当完整平台通过。


## 最终结果（限定范围PASS）

修复后的Split Mac managed/AOT各163检查0FAIL；修复前整批managed/AOT各518检查0FAIL按原有限场景保留，不能用其替代新增重叠失败。最终8个明暗四档Split默认对照RGBA0，52个恢复/完整往返对照RGBA0；1DIP76203像素/max234正对照有效。新缺陷的当前修复前FAIL、修复后PASS均有实际图与几何，不放宽容差。

媒体实际查看后，官方run删除341文件，audit clean/findings=[]/audit_error=null。删除临时C#3份36737字节、专用managed/ILC驱动458文件1490084392字节及AOT22文件339853911字节；共享构建缓存保持。所有临时测试删除，常驻净增0。

清理后精确重建：ButtonCustomSizeTests/SplitButtonLayoutTests/SplitButtonSeparatorStabilityTests共22 PASS/0Skip/4.0416秒；Gallery WorkspaceWindowLayoutTests/GalleryBuiltInThemeTests共11 PASS/0Skip/2.3128秒。合计33项精确门禁，不是全量；NET8 Desktop.Controls Release0警告0错误19.89秒。git diff --check通过。

作者另步自审确认仅3行Split ControlTheme Setter改变产品代码，没有新API/Token/优先级系统、代码找节点赋值或永久验证框架；3份正式文档同步。原7853文件存在，暂存区二进制diff保持；本次只修改原有Split主题与三文档，其他原文件hash保持。分支feature/custom-theme及HEAD35f9cc8f5保持，未创建分支/worktree/subagent，无commit/push/发布。

结论只覆盖本轮列明场景，原生最终屏幕、完整Part检查器交互、其他平台和G1/FW1/FW2等原未决不因此通过。
