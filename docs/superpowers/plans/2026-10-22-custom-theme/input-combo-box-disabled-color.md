# CT13 CB2：ComboBox 禁用项 Own 文字色

承接CB1。ItemDisabledColor已有公开资源及“列表项文字禁用颜色”语义，当前主题disabled分支却直接读取Shared ColorTextDisabled；接回与普通/hover/selected同一ComboBox Own体系。保持默认公式、disabled选择器优先级、公开ComboBoxItem身份、所有尺寸/输入/滚动与Popup行为，无新API/字段/代码绑定。

完整源码确认HandleHoverColor已经在ComboBoxHandleTheme的内部IconButton:pointerover消费；仅做真实命中正对照，不因先前截断读取而重复改造。若命中或真实Icon投射失败，先记录原始失败、完整输入几何与值链，再另定修复，不直接覆盖部件属性。

- [x] CB2-R：已打开overlay popup的禁用项及实际文本对ItemDisabledColor配置当前失败；正常文字/HandleHoverColor实际IconBrush及Icon.StrokeBrush正对照，真实pointer命中证明。
- [x] CB2-B：原明暗、默认/输入/disabled/pinned popup与高度限制、AA/1DIP/复位、同轨迹Own定制/清除完整帧。
- [x] CB2-I：只替换ComboBoxItemTheme disabled Foreground的一个资源入口，保留其原条件与优先级；不改默认、事件、路由或无关代码。
- [x] CB2-V：明暗同实例改色/清除，实际Glyph输入与Local/专用Style优先级；当前Mac managed/AOT/NET8。现有输入/组装/回收行为保持。
- [x] CB2-C：必要精确视觉审查后清理临时源/工程/媒体，精确ComboBox模块、文档/CSV与作者另步自审；完整家族与CT25继续，无Git授权。

CB2：禁用Foreground唯一资源入口接Own ItemDisabledColor；原1FAIL/既有HandleHover与Frame2PASS→3PASS20s，明暗仅显式disabled文字各402点已单独批准，其他/AA/复位0。Mac managed/AOT各42、NET8零警告23.79s；两临时源/419文件工程/49媒体清理，原69项。专用Style优先级扩展另由CB3闭环，非全家族，无Git。
