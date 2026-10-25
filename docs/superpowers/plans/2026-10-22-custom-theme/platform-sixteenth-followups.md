# 第十六批审查：色谱／单分量滑轨键盘与棋盘重挂

本批在已提交393d718b38a704a1d29ef6a94168c856b4249188之上，让普通／渐变面板的色谱自然获得键盘焦点，修正VS纵向映射，并为单分量／Alpha滑轨接通已批准的标准数值键；离树改变透明棋盘输入后，同模板回树更新派生画刷。既有绑定、舍入、指针、完整面板和多stop轨道合同保留，没有新公开签名、Token、Part、字段或订阅。

## 已批准的设计与有限视觉

用户分别批准[C9设计](color-spectrum-keyboard.md)、[棋盘视觉与31对限定表](color-spectrum-keyboard-noise-review.md)、[C10设计](color-slider-keyboard.md)及[72对限定表](color-slider-keyboard-noise-review.md)。有限接受只限逐行Host／历史／坐标／数值上限；全部原始计数保留，无通用容差。上述批准不含Git、完整家族或原生最终像素。

## 实现与可验证边界

- 色谱Focusable由ControlTheme声明，使用现有框架adorner，移除无目标FocusEllipse选择器；只修正VS纵向分支，保留六模式、RTL、Ctrl和边界。
- 具体ColorSlider单一OnKeyDown读取SmallChange／LargeChange和Min／Max，RTL仅反转水平键；SetCurrentValue保留绑定及既有颜色通知。修饰键、Handled和禁用不更新；共享基类没有键盘入口，多stop编辑不扩展。
- ColorBlock／AbstractColorSlider的运行时派生DrawingBrush跟随离树标量输入；已有32项有界cache不变，没有新owner引用、字段或订阅。计算不能由模板标量Setter替代，代码已有具体边界说明。
- AbstractColorSlider单独留有纯成员布局快照：只整理internal属性注册／wrapper分区，token fingerprints及静态初始化顺序一致。获批后先做纯布局提交，再恢复完整候选提交功能与文档。

## 验证和清理

[C9结果](color-spectrum-keyboard-results.md)：24项临时、清理后原ColorPicker27项、NET8 Release零警告／错误、三控件审计通过；168自然帧、56组1DIP及独立直接Skia／状态观测完成，所列视觉和31对差异已批准。真实Mac managed388／388；AOT首轮383／388缺四事件仍未证明原因，trace及同一原二进制重跑各388／388不抹去首轮。该项仍属CT25未决，不能称全家族通过。

[C10结果](color-slider-keyboard-results.md)：空值与双stop正常数据各原8项键盘RED／候选26项GREEN；既有Alpha舍入独立表征2项，没有为过严预期改变产品。真实Mac managed／NativeAOT各8个NSWindow、5688／5688断言、1248目标原始按键，无缺失送达／非active／外来焦点。

C10共640帧，128组1DIP全检出，八组新旧默认0；独立直接Skia128组A/A／32恢复／40兼容0，416对公开树／源位图记录一致。72对自然小差异经精确审查接受，实际画面及差异图已查看。两份临时C#、外部工程／产物428文件及645 PNG＋manifest共646文件已删除，媒体审计clean；C9临时源／驱动和457 PNG＋manifest共458文件也已清理。没有新永久测试，没有取证二进制进入项目或Git。

最终清理后原ColorPicker精确27项通过，NET8 Release构建零警告／错误；ColorBlock／AbstractColorSlider／ColorSlider／ColorSpectrum四文件四控件完整成员审计零问题。文档与链接、source SHA、原生身份和作者另步自审核对完成；按主agent独立执行要求没有评审agent，不声称独立评审。未运行本地full／change或无范围回归。

## Git方案与排除项

现有feature/custom-theme和原checkout，不创建分支／worktree；本批25个唯一文件，分两个提交：先1文件纯成员布局，再功能与正式文档。之后推送现有远端同名分支，用AtomUI凭证运行验证Actions三OS／两Browser，full=false；不发布NuGet，不取消其他运行。C9／C10的当前源身份与本批冻结文件逐项匹配，最终摘要在审查提问及本地文字manifest中给出。

排除后续CT14 TreeView Token研发、其临时探针／计划、全部临时验证和产物。C10模块／NET8及原生证据是在该TreeView后续改动前获得，匹配本批相对于393d718b3的产品快照；TreeView不随本批提交。第十五轮8个job已成功，但它不包含本批，实际其他平台确认与完整CT19／CT25继续。

没有新的Git授权；本文不是自行提交许可。依据AGENTS的“未经明确指令不得提交”，等待对本次25文件／两个提交／推送与验证Actions的明确审查。

## 精确文件

- `docs/controls/desktop/data-entry/color-picker/changelog.md`
- `docs/controls/desktop/data-entry/color-picker/implementation.md`
- `docs/controls/desktop/data-entry/color-picker/overview.md`
- `docs/controls/desktop/data-entry/color-picker/token.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-picker-checkerboard-lifecycle.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-picker-keyboard-audit.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-slider-keyboard-noise-review.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-slider-keyboard-plan.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-slider-keyboard-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-slider-keyboard.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-spectrum-components-noise-review.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-spectrum-components-plan.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-spectrum-components-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-spectrum-keyboard-noise-review.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-spectrum-keyboard-plan.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-spectrum-keyboard-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/color-spectrum-keyboard.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-fifteenth-followups.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-sixteenth-followups.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/tasks.md`
- `src/AtomUI.Desktop.Controls.ColorPicker/ColorBlock.cs`
- `src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/AbstractColorSlider.cs`
- `src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/ColorSlider.cs`
- `src/AtomUI.Desktop.Controls.ColorPicker/ColorView/ColorSpectrum.cs`
- `src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/ColorSpectrumTheme.axaml`
