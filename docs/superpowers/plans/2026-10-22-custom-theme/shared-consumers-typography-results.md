# CT-10 S4：图标、文字与共享绘制消费结果

日期：2026-10-23，基础提交 `79f2fc00e`；未提交，作者自审。本文记录已验证的分包，不表示 CT-10 或相关控件家族全部完成。

## 有效失败与修复

1. TextBox/LineEdit/TextArea/SearchEdit 的原生 FontFamily/Size/Weight/Style 已能到达真实文字，根 Foreground 则仍是内部 `#e0000000`，四项失败。新增具体 owner ColorText 默认，模板把根前景投射到框体继承链。控制级 ColorText 测试另发现单独 ControlTheme 文件的 TextBox 漏默认 Setter，实际为 Black；已取得额外失败并修正，四类实际文本的控制级覆盖/恢复通过。
2. IconTemplatePresenter 覆盖 child 的显式画刷/宽度，替换图标后旧图标继续跟随，强持有旧图标还会保留旧宿主。两类 Presenter 以 Template 优先级转发，替换/detach 释放，attach 对同一 child 恢复。原源码 A/B 的宽度/释放四项为 3 FAIL/1 PASS，修复后通过；原有 IconPresenter 的释放正对照保持。五个 AtomUI Icon 画刷槽与普通 PathIcon 都验证。
3. IconPresenter 默认画刷绕过自己的 Control Global，控制级 ColorIconHover=Red 仍为共享 `#e0000000`。主题改用其规范 Control 资源；控制级覆盖、兄弟隔离与 reset 通过。IconTemplatePresenter 没有独立 fragment，未增加身份。
4. Rate char/string 的脱树 VisualBrush 使用 `$Default`，不消费 root 的 monospace/Bold/Italic，两个真实失败。现在复制已支持字体，并在字体/字号/Width/Height 变化时重建。最初直接复制继承的 UI 字体造成字符默认 3440/3640 像素变化，已拒绝该结果。新增内容 Own `CharacterFontFamily=FontFamily.Default`，作为根 FontFamily 的原生主题默认；明确独立于普通 Global.FontFamily，保留原 glyph。主题字段、原生字体覆盖和快照换源通过。原 1195 字段为历史基线，当前新增 1 项；不是 Rate Own V2 完成。
5. 原生 TextSelectionHandle 缺少 Avalonia 必需的 Border#PART_Indicator，显示即抛异常；补齐后又复现 Background=null、宽 14 高 32。现在消费 Shared.SelectionBackground，保留原资源/Path 名称，采用 native :caret，两个尺寸都读取 TextSelectHandleSize。scope 色、显式 Background、clear、32→40→32 均通过。它是 framework asset-only，不虚构 AtomUI Own 或 Semantic Part。
6. Splash 进度条在准备阶段 BorderRadiusXS=0 时仍为 2，临时测试失败；非对称 CornerRadius 的实际 Skia 右上角仍为实心轨道，两种主题均取得失败。默认样式改读准备阶段 BorderRadiusXS；确定绘制及合成状态携带完整四角。确定进度的实际像素验证通过；合成循环动态验收继续 CT-11。

代码绑定仅保留无法等价声明 Template 优先级的运行时 icon content 转发，已写明原因。其余修复使用原生 ControlTheme/resource，未引入优先级标记、反射绘制或第二套主题引擎。

## 脚本与像素

外部 Visual.csproj 引用当前 worktree 的 Desktop 项目，使用实际 Skia/Headless Drawing=false、缩放 1。App 的初始 config 区分 Light/Dark；metadata 场景检查实际 ThemeManager.Appearance。所有阶段由程序与框架输入驱动，无 Computer Use。

| 场景 | Light / Dark 旧默认→最终默认 | 实际变化像素 | 恢复 |
| --- | --- | --- | --- |
| 四类文本，24 个实例，含尺寸/禁用/错误 | 0 / 0 | 27088 / 27088 | 与旧实现同恢复阶段比较 0 / 0 |
| Rate 六实例，SVG/char/string | 0 / 0 | 4184 / 4260 | 0 / 0 |
| IconPresenter，PathIcon/五画刷 Icon | 0 / 0 | 658 / 658 | 0 / 0 |
| SplashProgressBar 确定进度/四角 | 0 / 0 | 177 / 179 | 0 / 0 |
| native 手柄颜色/尺寸 | 原版崩溃，无正常旧图可比较 | 2060 / 2060 | 拖动后基线往返 0 / 0 |

四类文本 A/A=0，1 DIP 检错 810/811。其字体往返的 36/77 像素漂移由旧实现完整复现，旧新恢复帧为 0，未采用容差/遮罩；保留在 CT-25。源码 A/B 七个文件逐字节恢复；图标额外两个文件 A/B 亦精确恢复。

真实编译 AXAML 使用 `LineEditInputStyle` 设置 TextElement.Foreground=Green、FontWeight=Bold，根 Foreground=Blue，实际文字为 Green/Bold，明暗均通过。没有 code-behind 改语义部件。

手柄通过框架 MouseDown/Move/Up，实际 native 拖动开始/变化/结束计数 **1/1/1**。native 拖动合法改变位置，因此主题/尺寸 A→B→A 比较使用拖动后的基线；没有把位置复位来制造成功。未声称实机触摸/iOS 验收。

已查看四类输入动态画面、字符前后/最终变化、手柄变化、暗色编译 Part、Splash 非对称圆角等关键帧。一次外部 AXAML 夹具因缺 namespace 编译失败，后续误用旧二进制的 handle 命名截图无效，未计入结果；修正夹具并用新文件名、native PASS 与输入断言重新验证。

## 用例与清理

临时 Controls 13 项（icon 12、Splash 1），Desktop 11 项（输入 API 4、控制级前景 4、字符 2、Own 字体 1），共 24 项。Controls 加既有 SplashDefaultThemeTests **16/16，1.62 秒**；Desktop 加既有 RateBinding、IconPresenterBrush、IconLoadingAnimation、TextInputRootBrush、SearchEditBehavior/Layout **46/46，1 秒**。新增验证全部为临时源，不永久扩大全局测试。

重放入口为外部 Visual.csproj，阶段 `before/final/rate-before/rate-final/icon-before/icon-after/style/handle/splash-before/splash-after`，暗色环境为 `ATOMUI_FRAME_DARK=1`。源码 A/B、命令日志、像素计数与审查结论以文字记录；临时媒体/驱动和测试按生命周期删除后，追加清理后 focused/NET8 结果。

## 未关闭项

Tree 拖拽与 DataGrid control-content 的 VisualBrush 直接引用源控件，未复制字体；DataTemplate 分支和容器释放继续各自家族。SVG 数据、数学比例与白色 cutout fallback 不机械替换成背景 Token。Splash 的其他启动默认位于可覆盖的样式 Setter，仍遵循提交前只用全局资源的边界。

其他公开 owner 的字体/控制级字段/独立 public 子控件、Rate 完整尺寸与状态、Own V2、动效、原生按钮闪动及全部平台/AOT 尚未由本页验证。S3 排版的全 owner 勾选及 S5 总出口仍开放，不能以本页替代各家族逐项验收。

## 清理后验证与作者自审

24 项临时测试、外部驱动及 bin/obj 已删除；媒体目录 82 个文件清理，audit clean。清理后 Controls 的 Splash 精确组 **3/3，201ms**；Desktop 上述既有精确组 **35/35，1 秒**。实际 metadata 确认 Light/Dark，Avalonia **12.1.4.0**。本批五个 C# 控件成员审计 0 问题，624 个本地文档链接无缺失，无重复当天 changelog 标题，git diff --check 通过。作者自审，无独立 reviewer，不自行提交。NET8 结果同步至总进度记录。

Desktop NET8 Release（包含本批 Common/Shared 依赖）：**0 warning / 0 error，17.05 秒**。
