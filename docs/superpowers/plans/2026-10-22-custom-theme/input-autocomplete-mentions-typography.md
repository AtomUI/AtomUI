# CT13 AM3：复合输入字体消费实施

> 使用superpowers:executing-plans单agent沿当前feature/custom-theme继续；不提交、不创建分支/worktree。AM2工程/清理通过、48场景视觉待批，字体为独立分包。

**Goal:** 四个公开root的有效Global字体及标准FontSize/FontFamily进入实际输入文本，保留未配置时的现有字形模型。

**Architecture:** 复用ControlTypographyBindings与实际InheritanceParent只读桥；字体选择在root ControlTheme，模板把最终值声明式投射至私有输入。编译Binding的Priority=LocalValue已用真实AutoCompleteTextArea原型验证，可越过私有TextArea的预设trigger，不写C#绑定/新状态旗标。

**Tech Stack:** 当前Avalonia/AXAML、现有Core有限字体转换器、NET10聚焦、NET8、Mac managed/AOT。

**Spec:** [字体审计](input-autocomplete-mentions-typography-audit.md)、[输入目录](inputs.md)、[消费规范](../../../engineering/development/theming/consumption.md)。

## 已取得的前证与默认合同

- 四root实际TextPresenter对各自Global.FontFamily=Times New Roman/FontSize=29共8有效FAIL、909ms；父Courier New/23保持，实际仍Courier，普通/搜索23，多行14。
- Root.AutoCompleteTextArea.FontSize=29另有1有效FAIL（实际14）。16个原默认节点记录已保存：普通/搜索四尺寸实际23；AutoCompleteTextArea/Mentions实际Large16/Middle14/Small12/Custom14；根原本都显示父23。字体族均真实继承父Courier。
- 普通/搜索保持真实父字号，只有对应Global字号显式时选主题值；多行保持实际预设16/14/12/14。多行root标准FontSize主题值会与既有可见预设对齐（原getter23但实际14的分离被消除）；这一可观察合同在最终具体审查中明确列出，默认可见字形不得漂移。
- FontFamily保持未配置时真实父继承。Root Local/普通Style优先于主题；私有输入是parent拥有的字体维度，不借LineEdit/Search/TextArea的另一身份覆盖。
- 所有最终属性由AXAML持有；多行的局部字体投射原型用CompiledBinding Priority=LocalValue已真实1PASS603ms，并逐字节恢复原主题。不给生产添加RelayBind/反射或定时器。
- 不新增Own、Global、公开标志、资源key、Part/route，不改原字号比例、行高/输入几何的独立职责。实际typeface/可读性必须验证；正值字体范围遵从原生字体属性合同，不把非法字体值静默夹取成另一个值。

## Task 1：完整兼容前证

**Files:** 临时`Temporary/CustomThemeAutoMentionTypographyAudit`与项目外媒体。

- [x] 明暗、四尺寸的输入本体/父继承/正常状态及实际typeface；标准Local/Style、foreign公共输入配置、清除/换父/重模板分别记录。
- [x] 默认同历史Skia AA/1DIP复位和配置/恢复，正常输入反馈不关闭；任何默认漂移先诊断。

## Task 2：root选择与模板投射

**Files:** AbstractAutoComplete.cs、Mentions.cs只实现现有internal IThemeInheritanceSource；AbstractAutoCompleteTheme及三个具体AutoCompleteTheme、MentionsTheme。

**Interfaces:** root readonly实际继承父桥；已有FontFamilyConverter、FontSizeConverter的8输入无flag向量、PresetFontSizeConverter。

- [x] root公共字体族可选Global三元组；普通/搜索root可选字号三档保持继承，多行root使用既有实际三档预设。
- [x] 四模板FontFamily/FontSize投射至私有输入，FontSize使用已实测声明式Local优先级，避免公共输入theme trigger串值；不添加代码回退。
- [x] 原9有效反例转绿，16默认实际文本值不变；标准API/Style覆盖、对应三档、Context隔离、foreign输入Own/Global、专用Input Style命中与撤回。

## Task 3：专项与清理

- [x] 实际字形/typeface和输入/selection/caret/只读/禁用/配置往返、重挂/重模板/GC；当前Mac managed/AOT、NET8、4个C#实际结构门禁。
- [x] 同步正式字体模型与changelog，具体新配置/标准API效果及多行root值对齐单独审查；不把字体分包当完整家族。
- [x] 删除本包临时源/原型/工程/媒体，精确AutoComplete/Mention及实际直接消费者最终验证，git diff --check；CT12临时源保留。

渲染补充：多行原LineHeight22在标准Arial31时小于实际GlyphRun.InkBounds32，两项真实RED（624ms），不能以属性通过代表可读。新增内部无状态CompoundInputTypographyBindings只负责默认行框容纳实际字体设计单位度量；对应owner FontHeight显式存在时保持其原值优先，未显式时取原主题行高与真实字体natural spacing较大值。私有输入LineHeight全部用AXAML Local优先级MultiBinding，不加代码绑定/owner缓存或新公开入口。必须重验原16默认字形/像素及显式行高优先级。

执行结果与仍待批准范围见[AM3结果](input-autocomplete-mentions-typography-results.md)及[限定审查](input-autocomplete-mentions-typography-visual-review.md)。独立字体分包不代替完整CT13。
