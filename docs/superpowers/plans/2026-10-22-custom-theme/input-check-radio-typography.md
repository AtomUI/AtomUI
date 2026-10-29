# CT13 CR4：CheckBox / RadioButton 字体继承与有效 Global

承接已批准的CT13字体合同及CR3，不从头重做前包。四个公开owner为CheckBox、RadioButton、CheckBoxGroup、RadioButtonGroup；默认继续真实InheritanceParent的FontFamily/FontSize。只有相应有效Global显式配置时启用主题值；未配置、清除、换父后恢复原生继承。保留LocalValue/普通Style优先级，不新增SizeType、IsCustomFontSize、Own字段或public API。

唯一采用现有ControlTypographyBindings的三输入可选FontFamily/FontSize转换器；四个薄封装通过现有内部IThemeInheritanceSource只读桥读取受保护InheritanceParent，八个最终Setter由ControlTheme声明。直接typed默认字体会压制原继承，代码写最终属性会改变优先级，两者均不采用。Group内部公开item保留自身身份及继承链，不借用Group身份。

- [x] CR4-R：四owner字体族与字号共八个真实失败；先断言原父继承和实际文本/字形，配置有效Global后仍未变化才算RED。验证可用字体的真实glyph family不同，避免缺字体回退假通过。
- [x] CR4-B：改码前实际明暗、完整相同输入历史、AA/1DIP/复位及配置清除基线；自然合成与直接绘制分开标识，CR3批准不扩大到本包。
- [x] CR4-I：仅四控制薄桥与四ControlTheme的八个Setter；复用现有转换器和订阅释放，不改Core/组选择/数据容器/颜色及Own公式。任何文本子树不继承的独立缺陷先证明，再决定边界。
- [x] CR4-V：实际GlyphTypeface/FontRenderingEmSize，兄弟隔离、Local/Style覆盖、清除、换父、重挂与GC；当前Mac managed/AOT、NET8及完整成员审计。
- [x] CR4-C：临时源码/外部工程/媒体清理后精确模块；文档和目录只回填本片，完整家族/其他Host/原生最终像素及CT25继续，无提交授权。

CR4：四个公开owner复用内部真实继承父只读桥与三输入字体转换器，八个AXAML Setter显式消费有效Global FontFamily/FontSize；默认/清除继承、Style/Local及相等值存在位正确。8RED→13合同/GC通过（7对象回收），完整默认/父字体Frame通过，224条非配置数据一致；Mac managed/AOT各8窗口433项，NET8零警告34.62s，清理后原51项。配置字体及N01–N07精确25/32点差异已批准，原始FAIL留CT25。两临时源、419文件原生工程、314PNG+manifest均清理；非全家族，无Git授权。
