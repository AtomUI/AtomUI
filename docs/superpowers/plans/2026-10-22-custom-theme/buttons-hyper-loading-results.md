# CT-12：HyperLinkButton loading 状态接线

基于346732602a及当前HorizontalMargin注释/合同改动；保持官方Avalonia12.1.4，单agent、原分支/目录，无提交。

## 失败、根因和最小修复

原始5项临时用例全部失败：首次挂载前及挂载后设置IsLoading=true，有/无用户图标，实际Classes均不包含`:loading`。原有尺寸测试只读图标宽高，未证明loading图标可见。AbstractHyperLinkButton已经监听IsLoading变化并调用UpdatePseudoClasses，但只同步Visited；Desktop模板的loading可见性、用户图标替换、Spin和OpacityLoading没有激活者。

修复只在原UpdatePseudoClasses增加Loading同步。保持现有AXAML选择器及原生优先级，没有新增API、资源、代码绑定、导航、Click、命令或enabled行为。没有将无动效/完整家族出口算作本次修复。

临时测试位于Desktop.Tests/Temporary/CustomThemeHyperLoading；原始5FAIL→5PASS。补强真实模板替换：旧loading节点脱离、新节点为不同实例，状态仍有效。最终6PASS包括实际画面取证；Control Global OpacityLoading依次0.35→0.72→清除0.65，退出loading恢复1；visited/loading在分离重挂后保留，正常恢复后真实Headless指针与Enter共2次Click。首次夹具事件名/namespace编译失败不计产品RED。

## 精确视觉及待审差异

Skia合成器，310×290、scale1，四个真实HyperLinkButton、有/无图标，正常动效启用；相同完整操作历史。最初仅设置Avalonia Dark variant的两组均实际浅色，保留原始比较但不计暗色验收。修正为ThemeConfigProvider的Default／Default+Dark算法，并在采帧前断言实际snapshot.Appearance；背景读取同scope ColorBgContainer。真实明暗像素不同89900，标签已验证。

| 比较 | Light | Dark |
| --- | ---: | ---: |
| 前/后各自A/A | 0 / 0 | 0 / 0 |
| 1DIP检错，前/后 | 3194 / 3194 | 3192 / 3192 |
| 1DIP恢复，前/后 | 0 / 0 | 0 / 0 |
| loading退出恢复，前/后 | 0 / 0 | 0 / 0 |
| 前→后idle/AA/扰动恢复/退出 | 全部0 | 全部0 |
| 前→后loading1 / loading2 | 3372 / 3388 | 3392 / 3404 |
| loading两时点差，前/后 | 0 / 116 | 0 / 140 |

loading是已定位的预期修复：原画面无loading图标、用户图标继续显示、文字不降透明度；候选显示旋转loading图标并使用原有0.65透明度。最大通道差Light233、Dark200；未使用通用容差、裁剪、对齐或覆盖基线。实际查看完整明暗loading及退出图；静止/退出保持零差异。两时点实际时钟间隔约175ms，说明合成输出发生旋转；不声称时间相位完全相等或已验收全部动画帧。

按主题验证规范，此有意修复改变IsLoading=true输入的外观，单独提交了明确视觉审查；不因Bug修复自动豁免。用户已批准本修复及所示四帧精确差异（call_405a75b955cc4af3b00f14b569d03610:0），范围不含Git提交、无动效合同或完整家族验收。

## 出口

当前Mac原生managed/NativeAOT、清理与最终精确模块验证待完成后追加。CT12完整家族、IsMotionEnabled对连续loading旋转的最终开关、其他平台和已有原生边缘差异继续开放。未创建提交、分支、worktree或clone，未发布NuGet。

后续[连续动效分包](buttons-continuous-motion-results.md)已验证四owner开关并获准精确差异；最终当前Mac managed及NativeAOT各63PASS，包含本修复的真实明暗、显示与生命周期。官方依赖不变；临时探针身份日志的IL3000与本机最低OS链接警告如实保留。强化后的10项临时验证通过。清理和最后模块验证继续后附，不以本包通过关闭完整CT12。

三份临时测试、148PNG+manifest与全部外部产物已删除，媒体audit clean、常驻测试净增0；清理后精确模块132PASS/0Skip，成员/本地文档链接/diff检查通过。当前分包视觉已批准，但完整CT12及其他平台仍开放；本轮没有Git提交授权或提交行为。

NET8 Release产品构建0 warning/0 error（18.71秒）。本loading接线分包完成，完整家族出口继续推进。
