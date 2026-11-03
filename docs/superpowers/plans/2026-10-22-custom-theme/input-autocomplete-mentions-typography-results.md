# CT13 AM3：复合输入字体消费结果

状态更新：最后五表已于2026-10-30获用户明确限定批准，CT13既定范围已完成；下文原始执行/失败记录保留，CT25/FW1继续独立。见[批准汇总](input-final-visual-review.md)。


2026-10-30，现有feature/custom-theme与工作目录，单agent、无提交或新分支/worktree。候选实现、专项、原生及清理完成；本包限定视觉和多行root字号值对齐待用户批准，AM2视觉仍独立待批，完整CT13开放。

## 实现与责任

三个公开AutoComplete外观与Mentions消费自身有效Global字体。已有Core有限字体转换器在root选择字体，现有internal IThemeInheritanceSource仅提供真实InheritanceParent只读桥；四模板通过声明式绑定投射到私有输入，字号使用经实测的CompiledBinding Priority=LocalValue，阻止复用LineEdit/SearchEdit/TextArea的另一公开身份干扰。没有新增Own、Global、公开API/旗标、key、Semantic route或代码字体绑定。

普通/搜索未显式字号时仍继承父值。多行保持原实际Large16/Middle14/Small12/Custom14，root标准FontSize主题值与原可见值对齐：父23时原root getter23、实际14的分离消除。16个原默认实际文字字号/字体族全部保持，8个多行root getter记录改变；这一可观察合同单列审查，不以默认像素0掩盖。

大Arial31让原多行LineHeight22小于实际glyph ink32，两有效RED后新增内部无状态CompoundInputTypographyBindings.LineHeight：仅默认行框取原主题行高与实际Typeface设计单位natural spacing的较大值；对应owner FontHeight显式存在时保持精确值20，清除恢复自然行距。AXAML负责最终LineHeight，不保存owner、缓存、计时器或订阅。专用Input Style直接定制TextPresenter时以FontSize35+LineHeight42声明完整排版，撤回恢复当前值。

## 失败与专项

- 8个实际字体消费者原版FAIL（909ms），另root多行FontSize29未进入实际文字1FAIL；CompiledBinding原型1PASS后逐字节恢复，再采原版历史。2个glyph linebox真实RED（624ms）后最小修复。
- 最终24临时检查PASS、0FAIL/Skip、2s：四root各自Global三档、真实父继承、Style/Local及撤回、foreign公共输入身份隔离、4编译专用Input Style类型与Setter/实际命中、清除/换父/重模板/重挂、8个root/presenter图回收。
- 四root真实KeyTextInput、同一editor身份、Value/selection/caret保持；只读阻止写入、禁用状态与字体保持、标准Bold/Italic进入实际TextPresenter。
- 当前Mac真实NSWindow managed/NativeAOT各1686检查PASS：明暗16个真实输入、原默认/配置/禁用/标准Local/显式行高/清除/换父/重模板/重挂与4个编译专用Style。读取实际ShapedTextRun/GlyphRun typeface和行框/ink，不从root属性推断渲染字体；Courier New、Times New Roman、Arial均实际命中。
- NET8 Release构建0警告/错误、20.25s。NativeAOT无IL2026/IL3050或抑制；5条已知本机OpenSSL/Brotli库macOS deployment warning如实保留，不能称AOT零警告。
- 成员门禁3文件/2Control/0问题。作者另步自审范围覆盖root桥、12输入行高向量、identity、默认/优先级、生命周期及AOT；遵守用户禁止子agent，没有独立评审agent。不将自审写成外部批准。

## 绘制与范围

1130×560实际Skia完整窗口，四root×四尺寸、原文字Font Sample 012与245宽保持；父Courier New/23，明暗局部provider，外层原light host，正常动效。每阶段等待至少650ms与连续3帧一致。原版/首候选/修正行高候选/匹配表面补充共88PNG，首候选glyph缺陷保留为失败记录，不替换原历史。

原版与最终候选22对：16对默认、AA、1DIP相同历史、复位、清除、父变化/恢复与Local清除全RGBA0；6对明确配置字体或标准API变化，逐对计数见[限定审查](input-autocomplete-mentions-typography-visual-review.md)。各版AA/复位均0，标题1DIP检测2015点；匹配暗表面检出2017点。没有放宽容差、关闭输入反馈或删减原数据。

原dark局部provider放在白色外层时，禁用文字在旧/新都低对比至不可辨；原始两帧已实际查看，不能称该混合场景可读或已修复。单变量补充仅把背景改匹配#141414，禁用文字可见，字号/物理字体仍正确；补充不替代原比较，也没有改产品颜色。该混合表面/透明色问题列入CT25及完整状态出口继续核查。大字号单行保持原宽度与横向滚动/裁剪，多行保持原行数/滚动职责，默认行框容纳字形。

## 清理与最终验收

5个临时源（27642 bytes）、7份原主题/C#文字备份已删除；项目外原生工程427文件（1216741384 bytes）、88PNG+manifest共89媒体全部清理，媒体audit clean。只保留数值/文字轨迹与hash；常驻测试净增0，CT12六个临时文件与代码/进度保持。

清理后精确AutoComplete/Mention/CandidateListBehavior、实际TextInputLifecycle与两条直接wrapper消费者filter共52PASS、0FAIL/Skip、2s；git diff --check通过。完整字体/Part/状态家族矩阵、其余Global、Form/Upload、CT25与其他平台继续，不以AM3关闭CT13。
