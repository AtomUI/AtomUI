# CT20 尺寸与 Custom 出口

本记录是 CT20 当前实施和证据的唯一入口。沿既有 `feature/custom-theme`、HEAD `35f9cc8f5` 与全部未提交成果续作；不创建隔离副本、不使用子 agent、不操作暂存区或提交。

## 有限范围与完成条件

1. 原 `custom-size-candidates.csv` 的 58 行 / 45 文件逐属性对账，复用 CT11–CT19 已关闭的有限修复。当前原文件中仍有 35 个 mixed selector；数量仅表示复核入口，不表示 35 个缺陷。
2. 补核实际 `ICustomizableSizeTypeAware` 入口及独立 Custom 分支，包含 SplitButton 固定 Width140 的溢出。只处理控件实际拥有的维度；固定几何、内容、方向、Min/Max、输入/滚动和公开尺寸协议保留。
3. 每个真实剩余入口记录消费者、基础默认与三档预设、原生优先级、更新/撤回及适用验证。私有 Preset 输入与用户可接管属性分别判定，不按文本批量替换。
4. 先取得原场景失败/同轨迹基线，再修复；参数化临时验证清理后执行明确 filter 的模块验收、必要 NET8/AOT 与视觉检查。

既有家族最终批准不重开，CT24 排除。CT25/FW1/FW2 的历史 FAIL、异常、缺帧、未配对和平台缺口不转移、不豁免。

## 执行清单

- [x] 工作目录、分支、HEAD、四份总入口及既有修改指纹恢复；index 空且已记录 SHA256。
- [x] 原 58 行与当前 45 文件结构交叉核对；已迁移文件保留，不重新实现。
- [ ] 分组探测普通应用 Style 的 Custom 接管、撤回和三档往返；恢复 SplitButton 原 Width140 场景及实际子布局边界。
- [ ] 根据有效 RED 最小整改，ControlTheme 优先；保留不属于 Custom 接管的私有布局协议。
- [ ] 有限明暗/尺寸/输入及动态 Token 验证，实际受影响目标的默认前后比较。
- [ ] 清理、精确最终验收、正式文档与目录同步、作者另步自审。

## 续作与依赖

CT20 → CT21 工具壳 → CT22 三品牌与五边界配方 → CT23 手动闭环 → CT25 实际未决 → CT26 汇总。后项不以本记录代替工程或验收；单项阻塞只阻止实际依赖。

## 当前证据

2026-11-04：真实基线已恢复。CT21 目录 146 metadata；CT22/23 尚无完整出口。CT25 最新第二十一轮 `40291050592`、`278429b5` 的八个 job 均成功，范围严格依其结果记录；总表的第二十轮不是最新事实。既有目录中的历史文字和 `.superpowers` 旧路径只作为历史引用，本轮不写入该目录。

## 已执行工程与反证（截至当前，尚未关闭 CT20）

- 14 个公开 root 的普通 Style 对 Padding/CornerRadius 接管，连同 Split Width140 与 Spin 两指标，共 16 项原始有效 FAIL；相同断言已全部通过。Width140 的原子按钮右边界达到 209，修复后 140→180→140 的实际边界均不超出总宽。
- Button/Dropdown/Hyper 的仅图标或 loading 普通 Style 尺寸另有 3 个有效 FAIL（目标 31×29，原仍 16）；类型化基础默认修复后均通过。字体/已有输入草稿与 selection 保留为正对照。
- 默认前后 112 对（14 root × 明暗 × 四档）即时 RTB：首轮仅 Split Custom 两图非零（1304/max225、2076/max235），根因是既有无条件 TemplateBinding 在默认降级后压住子 Button 的默认。移除未设置时的 Padding/FontSize 绑定、保留既有条件实例投射后 112 对全零。该首轮 FAIL 不删除。
- 独立实例 28 组：A/A、Style 清除、1 DIP 复位均 0，Style 实际变化 349–9677 像素，1 DIP 检错 219–3475。画面审查发现固定高度的 Dropdown/Hyper 大字配方须显式选择 Auto；仅这两配方补齐高度选择，不改产品默认 32 DIP。早先不完整配方的裁切记录保留。
- Split 自然高度 23 字号/17,9 留白下，主/次动作分别 41/32 DIP，形成新的有效 RED；模板垂直 Stretch 后同一行高，既有 10 项 ButtonCustomSize 均通过。补充明暗配方/默认图只重采实际受影响的三个按钮，不重跑全部已有效帧。
- 原 58 行中的 ButtonSpinnerDecoratedBox.MinHeight 经真实 Numeric Input/Spinner、Height=MaxHeight=24、Padding=0 复现：外层 24 而内 frame 32，2 个有效 RED。只把实际高度预算投射到原 frame；保留三档 MinHeight 与无预算时的 32 默认，不删除高度/MaxHeight 条件。2 项及 20 既有 Handle 检查合计 22 PASS/0Skip，4.5399 秒。
- 新的最小高度转换器只计算已有 preset 与 Height/MinHeight/MaxHeight 的交集，不创建 Token、布局引擎或缓存。为了检验空 ButtonSpinner 的默认，原两份主题资产已确认与会话开始 SHA 相同，再从 HEAD 只读恢复到临时编译字典；没有替换产品文件或创建隔离副本。

### 验证位置与边界

临时源码仅在 `tests/AtomUI.Desktop.Controls.Tests/Temporary/CustomThemeSize/`。会话数据在系统临时目录 `atomui-ct20-26-tm7b4mr6`，取证由 `scripted-visual-acceptance-kls5dzle` 管理；清理前先归纳结论，媒体不入库。暂存区基线为空（SHA256 `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`）。

本轮原生驱动准备错误单列：首次 `/var` 与 `/private/var` 路径导致 transitive project 相对路径不成立；规范化路径已排除。外部驱动缺 namespace/nullable 问题不是产品 RED。首次本地 AOT 构建退出 0 但 ILC 明确报告 `LoadSkia` 将抛异常，因此不计通过；驱动现显式引用已批准 compat 工程并调用类型化 UseSkia，没有改缓存、native 依赖或 compat 源码。原生运行、最终发布和清理门禁仍待实际执行。

### 原生全局普通 Style 的未关闭失败

修正 compat 后本地 AOT 构建退出 0；新执行 managed 8.355 秒后未通过，AOT 运行尚未执行。源码/产物对应 native-publish，Mach-O SHA256 `06d2340a2cd5e06328c8d883926be23d3bbe13d72a0b0610670cebd7d68c13c7`（其后 Space/Drawer 新尺寸修复不在此产物内）。

原操作是 16 个真实 root 完成明暗/四档，再向 Window.Styles 添加全局类型 Style（包含所有 Button），配置后移除全部 Style，接着 Split 固定总宽 140→180→140、清除根 Width/MinWidth/Height。根已恢复 Font14/Padding0，但模板子按钮仍 Font23/Padding17,9；只读观测为 root Bounds128×45.5、Desired140×57.5（包含两边 6 的 Margin），secondary Desired48×32、primary80×45.5。最初 `<140` 的 Desired 断言混入 Margin，本身不准确，现改为与原配置前 DesiredSize 比较；**子按钮 Style 残留是另一个实际失败，不因纠正该断言而删除**。

该现象与既有普通 Styles.Remove 模板子树清理边界相关，当前尚未把这条新具体路径归并或转交 FW1/FW2。保留当前严格失败。参数驱动增加仅限定 root Name 的单变量 profile，用于区分全局类型 Style 对模板后代的匹配；新 profile 的通过不能覆盖原 global-types 失败。不在产品加找节点重写属性、强制重绘、重建实例或修改框架依赖。

## 原 58 候选的逐入口尺寸归属

原审计 CSV 保持历史原样。下表按实际模板分组，CS 编号对应其原始行序；所有 58 行均列入。三档规则的存在不是缺陷，只有其压住实际可接管维度时整改。既有出口的批准、视觉限制与 CT25/FW 账本保持原范围。

| 原入口 | 模板与消费者 | 尺寸职责、优先级与处置 |
| --- | --- | --- |
| CS01 | [AbstractColorPickerTheme.axaml](../../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/AbstractColorPickerTheme.axaml) | CT19 已关闭：公开 picker 的有限 Custom 根回退已完成；实际 preview 的 Size 与固定 inset 协议不扩张，复用 color-picker-family-exit.md 的最终批准。 |
| CS02/CS03 | [ColorBlockTheme.axaml](../../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorBlockTheme.axaml) | 内部预览表面：Width/Height 由 IsCustomSize=False 才取档位，显式 Size 经现有 Template 优先级投射；Radius 仍按 preview 名义尺寸。它不独立拥有公开 picker 的 Padding/Height；保留 CT19 已批准的 Size/固定 inset 协议。 |
| CS04 | [DataGridCellTheme.axaml](../../../../src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridCellTheme.axaml) | CT18 D4 已关闭：数据 cell/row header 的 Padding/FontSize 与 header/details/title/footer 的基础回退已分离；原生 Style/专用 Style 接管及密度往返复用 data-grid-family-exit.md。虚拟化、冻结及滚动不改。 |
| CS05 | [DataGridColumnGroupHeaderTheme.axaml](../../../../src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridColumnGroupHeaderTheme.axaml) | CT18 D4 已关闭：数据 cell/row header 的 Padding/FontSize 与 header/details/title/footer 的基础回退已分离；原生 Style/专用 Style 接管及密度往返复用 data-grid-family-exit.md。虚拟化、冻结及滚动不改。 |
| CS06 | [DataGridColumnHeaderTheme.axaml](../../../../src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridColumnHeaderTheme.axaml) | CT18 D4 已关闭：数据 cell/row header 的 Padding/FontSize 与 header/details/title/footer 的基础回退已分离；原生 Style/专用 Style 接管及密度往返复用 data-grid-family-exit.md。虚拟化、冻结及滚动不改。 |
| CS07 | [DataGridRowHeaderTheme.axaml](../../../../src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridRowHeaderTheme.axaml) | CT18 D4 已关闭：数据 cell/row header 的 Padding/FontSize 与 header/details/title/footer 的基础回退已分离；原生 Style/专用 Style 接管及密度往返复用 data-grid-family-exit.md。虚拟化、冻结及滚动不改。 |
| CS08 | [DataGridRowTheme.axaml](../../../../src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridRowTheme.axaml) | CT18 D4 已关闭：数据 cell/row header 的 Padding/FontSize 与 header/details/title/footer 的基础回退已分离；原生 Style/专用 Style 接管及密度往返复用 data-grid-family-exit.md。虚拟化、冻结及滚动不改。 |
| CS09 | [DataGridTheme.axaml](../../../../src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridTheme.axaml) | CT18 D4 已关闭：数据 cell/row header 的 Padding/FontSize 与 header/details/title/footer 的基础回退已分离；原生 Style/专用 Style 接管及密度往返复用 data-grid-family-exit.md。虚拟化、冻结及滚动不改。 |
| CS10/CS11 | [ButtonSpinnerDecoratedBoxTheme.axaml](../../../../src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/ButtonSpinnerDecoratedBoxTheme.axaml) | PresetContentPadding 是私有默认，最终 padding 有独立显式入口；保留该 mixed。实际 MinHeight 退出 Custom 预设，基础 Setter 用真实 owner 指标及 Height/MinHeight/MaxHeight 预算；三档保留，资源动态更新/撤回见本轮原 2+3 个 RED。内容 frame 的 Padding 绑定是全档位协议，不是第四档尺寸。 |
| CS12 | [ButtonSpinnerTheme.axaml](../../../../src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/ButtonSpinnerTheme.axaml) | 私有 PresetPadding/PresetCornerRadius 仅给公开 Padding/CornerRadius 基础 Setter 提供默认；普通 Style 可覆盖公开属性，不必删除私有档位映射。Numeric 内部 spinner 继续归 Numeric owner。 |
| CS13 | [SizeTypeAwareIconPresenter.axaml](../../../../src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/SizeTypeAwareIconPresenter.axaml) | CT13 已关闭：Custom 只有基础 IconSize，公开子 IconPresenter 的颜色身份与显式宽高保留；复用 inputs.md 最新出口，不重做默认迁移。 |
| CS14/CS15 | [CascaderAddOnDecoratedBoxTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Cascader/Themes/CascaderAddOnDecoratedBoxTheme.axaml) | Select 家族内部 PresetContentPadding/compact padding、标签最小行高与 prefix margin 按模式提供受管默认；公开 root Padding 有独立显式输入。保留多选/空选择及标签预算，不把根高度写入每个内部最小文本高度。 |
| CS16 | [CascaderTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Cascader/Themes/CascaderTheme.axaml) | 本轮 root Radius 与 mode/empty 相关 Padding 使用基础 Setter；既有类型化转换器保留标签最小高度和紧凑留白预算。原 3 档预设独立，普通 Style/LocalValue 可接管并撤回，未改选择/树/多列 popup 状态。 |
| CS17/CS18/CS19/CS20 | [CollapseItemTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Collapse/Themes/CollapseItemTheme.axaml) | DefaultHeaderPadding/DefaultContentPadding 是私有回退，公开 HeaderPadding/ContentPadding 及 owner 投射独立；header 排版、Start/End 箭头列号/间距属于模板的方向协议。专用 Style 可定制实际部件，不把 CollapseItem 根 Margin/Height 等同于这些内部维度。保留 CT15 出口。 |
| CS21 | [ComboBoxTheme.axaml](../../../../src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxTheme.axaml) | 本轮实际 root：可接管 Padding/Radius/FontSize/LineHeight 按该控件原有公开维度改为基础默认；三档保留。普通 Style 更新/撤回、原实例输入草稿与选择、明暗默认前后取证见本记录。TextBox 的 Custom 仍不应用三档 ContentPadding；字体 opt-out 保持。 |
| CS22/CS23/CS24 | [ExpanderTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Expander/Themes/ExpanderTheme.axaml) | CT15 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS25 | [FormItemTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Form/Themes/FormItemTheme.axaml) | label 行 MinHeight 与删除动作 Height 对齐输入名义行高，属于 FormItem 标签/动作布局；不等同 FormItem 根 Height。公开 label/删除动作专用 Style 与内容输入尺寸各有入口，保留 CT13 已批准的表单布局。 |
| CS26 | [TextAreaDecoratedBoxTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Input/Themes/TextAreaDecoratedBoxTheme.axaml) | 内部 frame 固定零 Padding、PresetContentPadding 和右附加区的留白协议；公开 TextArea.Padding 在 root 接管后经已有显式入口投射，内部默认不得替代用户值。保留 resize/滚动与附加区协议。 |
| CS27/CS28 | [TextAreaTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Input/Themes/TextAreaTheme.axaml) | 本轮实际 root：可接管 Padding/Radius/FontSize/LineHeight 按该控件原有公开维度改为基础默认；三档保留。普通 Style 更新/撤回、原实例输入草稿与选择、明暗默认前后取证见本记录。TextBox 的 Custom 仍不应用三档 ContentPadding；字体 opt-out 保持。 |
| CS29 | [TextBoxTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Input/Themes/TextBoxTheme.axaml) | 本轮实际 root：可接管 Padding/Radius/FontSize/LineHeight 按该控件原有公开维度改为基础默认；三档保留。普通 Style 更新/撤回、原实例输入草稿与选择、明暗默认前后取证见本记录。TextBox 的 Custom 仍不应用三档 ContentPadding；字体 opt-out 保持。 |
| CS30 | [ListBoxItemTheme.axaml](../../../../src/AtomUI.Desktop.Controls/ListBox/Themes/ListBoxItemTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS31 | [ListBoxTheme.axaml](../../../../src/AtomUI.Desktop.Controls/ListBox/Themes/ListBoxTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS32/CS33 | [ListViewItemTheme.axaml](../../../../src/AtomUI.Desktop.Controls/ListView/Themes/ListViewItemTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS34 | [ListViewTheme.axaml](../../../../src/AtomUI.Desktop.Controls/ListView/Themes/ListViewTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS35 | [MenuTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Menu/Themes/MenuTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS36 | [TopLevelMenuItemTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Menu/Themes/TopLevelMenuItemTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS37 | [OtpLineEditCellTheme.axaml](../../../../src/AtomUI.Desktop.Controls/OtpLineEdit/Themes/OtpLineEditCellTheme.axaml) | 内部 digit cell 的受管字号回退；宽高已由 owner 尺寸协议分别处理。独立 CellFont/生成的专用 Cell Style 与 IsCustomFontSize 路径继续遵循 CT13 既定出口，不把根字体/高度机械复制到每格。 |
| CS38 | [PaginationNavItemTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Pagination/Themes/PaginationNavItemTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS39 | [PaginationNavTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Pagination/Themes/PaginationNavTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS40 | [PaginationTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Pagination/Themes/PaginationTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS41 | [SimplePaginationTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Pagination/Themes/SimplePaginationTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS42 | [AddOnDecoratedBoxTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/Themes/AddOnDecoratedBoxTheme.axaml) | 内部 SizeCornerRadius/PresetContentPadding、附加区 padding 与 ContentPresenter 的行高/最小文本度量；公开 root 几何沿模板显式入口投射并覆盖默认。不是公开 root Padding/MinHeight 的同一个属性目标；保留 CT13 frame/输入协议。 |
| CS43 | [InputControlFrameTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/Themes/InputControlFrameTheme.axaml) | 内部 SizeCornerRadius/PresetContentPadding、附加区 padding 与 ContentPresenter 的行高/最小文本度量；公开 root 几何沿模板显式入口投射并覆盖默认。不是公开 root Padding/MinHeight 的同一个属性目标；保留 CT13 frame/输入协议。 |
| CS44 | [InfoPickerInputTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/InfoPickerInputTheme.axaml) | CT16 已关闭：picker 输入 typography 基础默认与三档已分离；保留普通/范围输入及 popup 高度、marker 与批准边界，复用 date-time-family-exit.md。 |
| CS45/CS46 | [SegmentedItemTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Segmented/Themes/SegmentedItemTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS47 | [SegmentedTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Segmented/Themes/SegmentedTheme.axaml) | CT14 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS48/CS49 | [SelectAddOnDecoratedBoxTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Select/Themes/SelectAddOnDecoratedBoxTheme.axaml) | Select 家族内部 PresetContentPadding/compact padding、标签最小行高与 prefix margin 按模式提供受管默认；公开 root Padding 有独立显式输入。保留多选/空选择及标签预算，不把根高度写入每个内部最小文本高度。 |
| CS50 | [SelectFilterTextBoxTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Select/Themes/SelectFilterTextBoxTheme.axaml) | 内部过滤输入只有 IsCustomFontSize=False 才取预设字号；实际公开 owner 的字体通过已有 opt-out/模板路径进入，保留 CT13 字体出口与 embedded 输入协议。 |
| CS51 | [SelectTagTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Select/Themes/SelectTagTheme.axaml) | 内部 tag 的名义高度与 HasCustomTagHeight 的实际内容预算是两个职责；Custom 有预算时取现有 CustomTagHeight，三档与无预算回退保留。生成专用 tag Style 的优先级及声明形态继续复用 CT13。 |
| CS52 | [SelectTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Select/Themes/SelectTheme.axaml) | 本轮 root Radius 与 mode/empty 相关 Padding 使用基础 Setter；既有类型化转换器保留标签最小高度和紧凑留白预算。原 3 档预设独立，普通 Style/LocalValue 可接管并撤回，未改选择/树/多列 popup 状态。 |
| CS53 | [CompactSpaceAddOnTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Space/Themes/CompactSpaceAddOnTheme.axaml) | 本轮 CompactSpaceAddOn 的 FontSize/Radius/Padding/MinHeight 为中档资源基础默认；三档、方向、Compact 接缝和内容测量保持。普通 Style/撤回及明暗四档默认已取证。 |
| CS54 | [SpinIndicatorTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Spin/Themes/SpinIndicatorTheme.axaml) | 本轮公开 IndicatorSize/DotSize 基础默认；三档及真实外框/四点 TemplateBinding 保留。44/14 Style 的实际图元、撤回及原动效协议分别验收，不以图标或根 Height 代替。 |
| CS55 | [ToggleSwitchTheme.axaml](../../../../src/AtomUI.Desktop.Controls/Switch/Themes/ToggleSwitchTheme.axaml) | CT17 已关闭的有限 Custom：基础默认与三档已分离，保留默认、方向、内容测量、公开实例/专用 Style 接管及资源撤回；复用该家族最终出口，不重复整改。 |
| CS56/CS57 | [TreeSelectAddOnDecoratedBoxTheme.axaml](../../../../src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectAddOnDecoratedBoxTheme.axaml) | Select 家族内部 PresetContentPadding/compact padding、标签最小行高与 prefix margin 按模式提供受管默认；公开 root Padding 有独立显式输入。保留多选/空选择及标签预算，不把根高度写入每个内部最小文本高度。 |
| CS58 | [TreeSelectTheme.axaml](../../../../src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectTheme.axaml) | 本轮 root Radius 与 mode/empty 相关 Padding 使用基础 Setter；既有类型化转换器保留标签最小高度和紧凑留白预算。原 3 档预设独立，普通 Style/LocalValue 可接管并撤回，未改选择/树/多列 popup 状态。 |


## CT00 扩展尺寸入口与本轮最后增量

扫描到的直接 ICustomizableSizeTypeAware 声明为 Common7/Desktop30/DataGrid1/ColorPicker2；它们不是40个独立新缺陷，继承/组合入口按现有元数据目录和实际 owner 合并。接口扫描不是闭合证据，原58候选及下列实际消费者分别核对。

| 扩展入口 | 实际拥有维度与处置 |
| --- | --- |
| Button/Dropdown/Hyper/Split、LineEdit/SearchEdit/Numeric | 原58外的独立 Custom/新增根选择器已纳入本轮实际 RED/默认/撤回；Split 总宽只属于组合，root 半径基础默认；按钮固定 Circle、仅图标与 Dropdown 指示器独立协议保持。 |
| Space.ItemSpacing/LineSpacing | 原 C# switch 的 Custom 继续取 GapSmallSize8，但 Template 优先级压住普通 Style（目标17/19仍8），取得有效 RED。仅 Custom 改为原生 Style 优先级，三档保持 Template；两个既有绑定的创建/释放不增数量。普通 Style、实际两子项间距37及撤回已通过，四档明暗默认8对0。 |
| Drawer.DialogSize | 原 C# switch 的 Custom 继续取 SmallSize378，但 Template 优先级压住220px Style，有效 RED。仅 Custom 改 Style 优先级，三档保持 Template；原有百分比、placement/visible frame 及绑定 acquire/release 协议保持。实际打开 dialog 的220宽与撤回已通过，四档明暗默认8对0。 |
| Avatar / SkeletonAvatar / SkeletonElement / ColorBlock | Size/显式 Size 与尺寸档保存/恢复协议独立，固定圆形、box/icon 内在比例不机械变成独立 Width/Height 预设。Shape 的圆角与其尺寸协议分别归属。复用 CT15/CT19 最终出口及源码，不在 CT20 借机重审完整家族。 |
| Form/FormItemDecorator、Transfer/TransferItemDecorator、CompactSpace | 尺寸通过既有 child/生成容器协议传递；控件自身内容、移动动作固定 Small、方向和接缝不随 Custom 任意改写。已批准 CT13/14 及原类型合同复用；不把每个组合实例伪造为新终端 identity。 |
| AutoComplete/Mentions/SelectCandidateListItem 的全档位选择器 | 共同适用状态/模板投射，不单独占用公开 root 尺寸，保留四枚举限定（不扩成任意枚举值），复用 CT13。 |
| Separator / 各 derived input、picker、list/menu/pagination、toggle/segmented | 复用原家族已完成的有限 Custom 及方向/状态映射；根公开属性、专用 Style 与内部受管文本/标签度量不混为同一维度。 |
| Steps 与只含三档的独立尺寸 API | 不借 CT17 绘制完成关闭独立 Steps 家族；只记录本任务适用的现有尺寸入口，无 Custom 承诺的 API 不新增第四档，不开展完整 Steps 重审。 |

Space/Drawer 的尝试 Inherited 级绑定被当前框架明确拒绝 Invalid priority，属于方案反证，已撤销；现行源码使用合法 Style3，与三档 Template2 分开。该试验未计产品修复前 RED，也未忽略异常。代码保留声明式选择器会改变预设 Template 优先级/Custom StyleTrigger 优先级的具体原因，未新增资源副本、全局订阅或缓存。

ButtonSpinner 最小高度的首次模板内 MultiBinding 压住 Small 预设，精确旧资产对照检出两项（内容312、空262像素）。绑定已迁到 frame 自身 ControlTheme 的基础 Setter；旧源码 SHA 校验后的空/内容、明暗四档16对全0，原2个Height24及3个owner资源RED均已通过；不把该回归当原产品缺陷。

本轮真实原生运行仍保留上一节 global-types FAIL；没有扩大既有FW1/FW2归属或标题栏像素接受。Default/原生数值/AOT/最终屏幕像素分账。尚未进行临时源码/媒体清理与最终模块门禁，因此当前不能标 CT20 完成。

## 当前实现更新：Raw 控件 Custom 默认归 ControlTheme

Style3 代码绑定的首轮普通 Style 添加/移除通过，但更强的 Small→Custom 重入断言和 native root-scoped 同时检出新绑定覆盖已有 Style。根因已缩小到同级命令式绑定重建的顺序；没有把首轮通过作为完整结论。

现行实现改为 SpaceTheme 两个基础 Setter、DrawerTheme 一个基础 Setter；C# 只维持原三档的 Template2 绑定，进入 Custom 释放它们后交给原生主题默认。不再使用 Custom 代码 Style3 绑定。普通 Style 的实际间距37、实际 dialog宽220、切档重入、LocalValue和撤回通过；16对原明暗四档 raw默认全0。Space 新增一个主题资产，不新增Control/Token字段/资源identity；正式目录须同步该资产。

核对本地官方 Avalonia 12.1.4（/Users/chinboy/Projects/ReferenceProjects/Avalonia，tag 2d79ba55fae1b1e71c69f93a668080c9029e6a31）：Name selector 返回 AlwaysThisInstance、不创建class activator，因此 native root-scoped 对照没有通过提高 StyleTrigger 优先级制造通过。StyledElement.DetachStyles 只遍历逻辑 children；结合原实际子按钮残留和限定到root后的14字号/15,4与4,4留白恢复，支持模板后代解绑边界这一方向。原 global-types 具体路径仍未归并或关闭，未修改框架源码。

## 最后工程验证与清理前结论

- 最终临时精确29 PASS/0Skip，8.6683秒；包含28个行为用例与最后 Numeric/TextBox 16帧增量采样。四档/明暗默认112对采用同源有效组合：Split用final2、Numeric/TextBox用last，其余复用new，全部0；另raw16与精确旧spinner16对全0，合计144默认比较。
- 28组独立A/A、Style撤回、DIP复位均0；DIP全部检出。Dropdown/Hyper大字采用显式Auto高度后的补充画面已查看，Split主次同高与边界保持。早先不完整配方、Small回归和无效优先级异常继续按历史反证保留。
- 最后 Desktop NET8 Release构建0警告/0错误，18.13秒。成员审计6文件/3Control/0问题；只把 Space.SizeType wrapper 移到注册顺序，成员文本SHA faa95ae3f37b90cc8979525aee8bcbe217e3f7ab756c0275e257466bc5e8c86b 不变，无初始化顺序调整。
- 当前Mac实际 Avalonia.Native.WindowImpl：managed(root-scoped)292检查，2.519秒；同源码NativeAOT(root-scoped)292检查，3.334秒，Mach-O SHA256 26f51db50c789341b6616e536a4b0fa0f27719842c1a13e9d560daacaaaf9eed。实际root尺寸/同实例撤回、numeric两模式24高度、真实Space间距/打开Drawer宽度、四档owner更新和输入状态均包含。
- 此Mac包未捕获最终屏幕像素，不把API/几何通过当最终像素通过。global-types失败仍是未关闭具体路径，不能拿root-scoped替代。其他平台/Browser未对本轮新源码重跑，不用旧CI为其背书。
- 本地AOT原有五条Homebrew部署版本链接警告保留；此次NuGet审计报告SDK ILCompiler10.0.8的NU1903，未擅自升级SDK/native依赖或抑制诊断。产品NET8无CS/IL警告。没有全量、Actions、Git操作或NuGet发布。
- 取证官方audit：clean/findings=[]。下一步删除本包全部临时测试、旧资产字典、参数driver/专用发布/媒体，再以清理后的确切模块检查完成工程门禁。CT21独立推进，当前CT20不标完整完成。

## 清理后的工程门禁与限定审查材料

已删除3份临时源码/旧资产字典（53096字节）；参数driver266文件/298952703字节、两份专用发布各16文件/182718456与184359628字节全部删除。官方cleanup删除550取证文件，audit clean/findings=[]、audit_error=null。原始7832文件指纹清单逐路径核对无缺失，暂存区仍为空且字节不变；没有清空共享产物或用户文件。

清理后实际重建 Desktop.Tests，精确25个具名测试类共274 PASS/0Skip，12.4903秒。范围为 Button/Hyper/Split、TextBox/Search/TextArea/输入生命周期、Numeric、Select/前缀/frame、Cascader selection、Space/Compact、Drawer geometry/interaction/AOT-binding、Spin及尺寸合同；完整filter暂存于本会话文本，未跑整个项目或全量。产品源码与NET8/AOT验证时相同，清理只移除临时测试和外部产物。作者另步自审：主题/实例优先级、显式0/Auto、公开子控件、已有状态、无新全局监听/缓存、注册key及原批准范围分别核对；非独立审查。

以下四项为配置原来不生效或实际几何错误的有限修复，默认144对全0；请按表限定审查，不批准未列平台/最终屏幕像素或Git操作。

| 范围 | 原表现 → 当前表现 | 保留边界 |
| --- | --- | --- |
| V1 | 14 root 的普通 Style 留白/圆角、三类按钮31×29图标、Spin44/14等被预设压住 → 实际公开属性与图元生效并撤回 | 四档默认、Circle、状态/内部文本和标签协议；无新Token字段或Custom API |
| V2 | Split总宽140时子边界209；自然行高41/32 → 总宽140/180往返均容纳，主次同高 | 根总宽不复制给两个子按钮；未配置留白/字体保留子Button默认；G1不包括在内 |
| V3 | Numeric两模式24高度/Max24/零留白时内frame32；owner高度60仍40 → frame遵守24预算，三档和Custom owner更新/撤回 | 原数值、步进、输入/模式；无预算的空Spinner及内容默认16对0 |
| V4 | Space普通Style17/19仍8；Drawer220仍378 → 实际间距37、实际打开dialog宽220并在切档后恢复 | 三档Template优先级、Small回退、方向/百分比与visible frame；raw默认16对0 |

**G1未决路径**：仅本文“原生全局普通Style”所列900×700/16 roots/完整历史，`Window.Styles`移除全局Button等类型Style后，Split子按钮仍23/17,9而根14/0；严格FAIL保留。`root-scoped`只改变静态Name匹配范围并恢复14及原留白；两profile不相互替代。当前未转移到CT25/FW1/FW2，也未接受像素差异。无依赖升级、框架修改或绕过重绘。该具体路径需要单独的调查归属裁决；CT20整体仍开放。

V1–V4限定审查及G1单路径是否交CT25/FW1调查，已在本会话通过 request_user_input_async 两项发起，当前等待用户回复；没有默认批准。CT21不依赖该裁决，继续按原授权实施。`git diff --check`最后通过；Space移位包装器末尾仅清除一个空行尾空格，不改变成员token或运行逻辑。

## 最终批准登记与 CT20 结论

2026-11-04 用户回复（request_user_input_async / call_d43dbc48582e4e8d80154dc2314ce4fe）：

1. item 0：“批准 V1–V4 限定范围”。仅上表四项配置/几何修复及其既定工程出口；不含其他平台、最终屏幕像素或 Git。
2. item 1：“仅批准 G1 归 CT25/FW1 调查”。仅上述900×700/16 roots完整历史的global-types撤回路径；严格FAIL、原边界和其他失败保持。不授权修改依赖或Git，不自动扩展其他路径或FW2。

CT20 在本次明确批准范围完成。原58候选及扩展入口已逐项归属，默认/更新/撤回、工程、临时清理、最终模块及作者自审按本文结果闭环。G1只按本批准交CT25/FW1继续调查，不以root-scoped通过抹去；平台与最终屏幕证据保持各自边界。没有提交、推送或发布。
