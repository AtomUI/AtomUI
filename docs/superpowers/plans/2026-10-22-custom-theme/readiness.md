# 精确主题首批边界与失败基线执行计划

> **For agentic workers:** REQUIRED SUB-SKILL: 使用 superpowers:executing-plans 在当前会话逐项执行；用户明确选择委派时才使用 superpowers:subagent-driven-development。步骤使用checkbox跟踪。本文件等待计划审查，本次没有执行以下步骤。

**Goal:** 完成CT-00–CT-02的真实Control目录、Token合同分类与代表失败/Host基线，为机制原型及控件整改提供可靠输入。

**Architecture:** 复用现有Roslyn编译符号、注册/Token/资源事实与测试宿主。只清点元数据和实际公开合同，不构造所有Control或运行全部evaluator。代表反例在所属项目Temporary中执行，证据保留为过程记录，临时测试/辅助产物按规范删除。

**Tech Stack:** 现有.NET/Roslyn、C#/AXAML、xUnit/Shouldly、Avalonia现有Headless及实际Gallery Desktop Host、Python文档数据校验。

**Spec:** [路线图与全局约束](overview.md)、[Own求值](../../../architecture/systems/theming/own-token-evaluation.md)、[品牌配方](../../../architecture/systems/theming/brand-recipes.md)、[渲染合同](../../../architecture/systems/theming/control-rendering.md)、[验收规范](../../../engineering/development/theming/verification.md)。

## Global Constraints

- 遵守路线图全部Global Constraints；当前worktree继续使用feature/custom-theme，不新建分支或worktree。
- 本批不修改生产C#/AXAML/schema，不接入新Own或品牌协议，不改默认外观及业务行为。
- 建立[视觉一致性](../../../engineering/development/theming/verification.md#视觉一致性硬门槛)前证：同环境/配置/状态下，后续重构不得有视觉差异。原外观兼容和目标定制生效分别记录；代表基线不代替最终逐Control比较。
- 不把2,736个文件、95个Own类型或1,195个直接声明属性误当作Control数、扁平终端字段数或有效配置项数。
- 所有新增验证先搜索现有测试；需要internal/主题资产的临时测试放所属项目Temporary/CustomThemeBaseline，文件头注明“临时验证，功能或修复完成后删除”。
- 只用精确filter执行本批临时反例/已有对照。预期失败是本批要收集的证据，不能改断言、Skip或改编译规则制造通过。
- 真实UI取证使用atomui-gallery-local-debug及scripted-visual-acceptance；资源探针用atomui-resource-lifecycle。执行时阅读对应skill，不能以Headless通过代替实际Host表现。
- 本计划不包含自动commit；清理并审查后等待用户明确提交指令。

## Review Focus

| 风险 | 本批检验/交付 |
| --- | --- |
| 无Own公开Control、内部Presenter或抽象层漏记/伪造identity | CT-00分开记录public产品项、抽象契约、internal目标与实际注册模式 |
| 无typed直接引用便被判断为无效，历史internal导出字段被删 | CT-01追间接消费/资源键/导出和别名；未知项保留待决，不删除schema |
| 只有旧整体默认值测试，却声称覆盖后派生正确 | CT-02-B02走真实ThemeConfig→Compiler→ControlTheme，断言覆盖后padding |
| 控件属性改变但真实frame/绘制不变 | CT-02-B03/B05/B06/B08同时记录目标属性、布局/绘制和原始场景 |
| 临时失败或单Host通过被写成产品完成/全平台通过，视觉基线被覆盖 | CT-02分别记录red/pass/未复现/环境阻塞及B09视觉比较；不更新原基线掩盖差异，原问题状态不自动提升 |

---

## Task CT-00：核定产品Control、注册和owner目录

**Files**

- Read: src/AtomUI.Generator/Registration/RegistrationModelBuilder.cs、RegistrationModel.cs、StartupManifestWriter.cs。
- Read: src/AtomUI.Generator/DesignToken/ControlTokenPropertyWalker.cs、src/AtomUI.Generator/SemanticParts/及各包实际C#/Themes/公开控件文档。
- Read: tests/AtomUI.Generator.Tests/Registration/TypeMapRegistrationGeneratorTests.cs、ThemeSchemaGeneratorTests.cs。
- Create: 本目录control-inventory.csv、baseline.md中的“目录范围”小节。
- Temporary（仅元数据无法直接核对时）: tests/AtomUI.Generator.Tests/Temporary/CustomThemeBaseline/ControlInventoryProbeTests.cs。
- Modify: 本目录overview.md中的阶段进度；不改注册生成器、验证选择策略或生产注册目录。

**Interfaces**

- Consumes: 已有产品源码、Roslyn符号/注册事实及审计coverage.csv；文件路径只是候选，不决定Control归属。
- Produces: control-inventory.csv，字段固定为下列过程记录schema；不是新增运行时schema或注册协议。

~~~text
assembly,metadata_name,kind,package,public_owner,token_identity,
registration_mode,own_definition,semantic_contract,theme_assets,
source_paths,doc_paths,gallery_paths,test_paths,platform_boundary,
primary_task,review_state,notes
~~~

kind取ProductControl/AbstractContract/InternalTarget/OtherBoundary；review_state取StaticChecked/NeedsDecision。同一程序集+metadata name只出现一次；多个模板/平台差异记录在同一对象的资产及边界列，不能因此丢掉变体。

- [ ] **Step 1：建立源码候选并与编译事实交叉。** 对Core/Icon、Controls、Desktop、DataGrid、ColorPicker、GalleryBase及Gallery分别清点public Control；同时记录实际存在的Extras及平台边界。partial按符号归并；抽象、泛型和嵌套类型按现行注册合同分类，不能靠正则结果宣布完整。
- [ ] **Step 2：核对精确owner与注册模式。** 从Token/Semantic/主题导出及注册符号交叉确认identity、descriptor和资产。没有Own不排除；没有独立片段不直接判Bug；internal目标不能伪造公开owner，未确定项写NeedsDecision及证据位置。
- [ ] **Step 3：补齐文档、Gallery及测试入口并指派CT-10–CT-21/CT-25。** 一个实际产品Control只有一个主迁移任务；内部目标挂到真实owner，跨家族协作另记notes。没有原审计ID的Control同样必须有任务。
- [ ] **Step 4：执行目录完整性校验。** 校验元数据集合与CSV双向差集为空、主键唯一、现存源码/文档路径有效、每个产品项均有主任务。若有不可访问编译事实或owner争议，输出明确差集，不能把这些项删掉以获得零差集。

预期：新增未审计产品Control能被差集检出；去掉一个无Own的已纳入Control同样失败。必要临时probe使用namespace AtomUI.Generator.Tests.Temporary.CustomThemeBaseline，测试命名为Inventory_Covers_Public_Controls_And_Separates_Internal_Targets，断言集合相等及internal分类，不实例化Control。

~~~bash
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj \
  --framework net10.0 \
  --filter 'FullyQualifiedName~CustomThemeBaseline.ControlInventoryProbeTests'
~~~

仅在Step 2确实建立该probe时运行，发现数必须非零；不创建probe则用静态元数据交叉记录解释检查来源，不能报告该命令通过。

- [ ] **Step 5：清理并交付。** 删除本任务临时probe/helper；记录实际产品分母、分类数、所有未决项及负责任务。文档数据校验及git diff --check通过后交审查，不创建commit。

**出口：** 产品目录与注册/资源事实双向核定；争议项保留，机制代表Control的owner必须明确。CT-00完成不表示这些Control已渲染验收。

## Task CT-01：逐字段核对可定制合同

**Files**

- Read: 原审计token-consumption.csv、findings.md及问题矩阵。
- Read: 各实际Own定义、生成扁平schema、资源/代码/自绘消费者、控件token.md和Gallery Token表。
- Create: 本目录token-contract-review.csv、baseline.md中的“兼容/合同决策”小节。
- Modify: 本目录control-inventory.csv的审查备注。正式控件文档如需纠正现有事实，先说明具体范围，不能把目标设计描述成已经实现。

**Interfaces**

- Consumes: CT-00的精确终端identity/owner与定义层；原1,195项只是直接声明线索。
- Produces: 下列迁移审查记录，不成为第二份产品Token目录。扁平字段主键为identity+token_name；declaration保存继承来源。

~~~text
identity,token_name,declaration,current_contract,proposed_category,
value_type,unit_and_range,default_inputs,consumer_paths,
states_and_sizes,alias_or_compatibility,problem_ids,primary_task,
decision_state,notes
~~~

proposed_category取Active/Derived/Legacy/Reserved/Deprecated/Undecided；decision_state取SourceChecked/NeedsDecision/Approved。这些值是迁移审查分类，不伪造当前descriptor已拥有相同标志。

- [ ] **Step 1：建立终端字段清单。** 按实际继承链展开所有Own，包括internal导出字段；记录稳定名称和原声明。核对同一属性继承到多个identity的情况，不能按字段短名全局去重。
- [ ] **Step 2：追消费链与默认依赖。** 每项查直接/间接资源、模板、自绘、派生公式和状态分支；“137项无所扫描直接引用”不直接等于137个Bug。记录可修改上游以及显式下游应保留的关系。
- [ ] **Step 3：决定或登记兼容边界。** DataGrid的17项规范值与Table*历史入口逐一对照；Button/Separator/Avatar/Progress等悬空或历史字段以及Menu未公开能力不顺手新增功能。同层别名冲突和跨层覆盖分别记录。
- [ ] **Step 4：核对最小反例。** Active/Derived必须有实际或计划消费者及负责任务；合法0/null与负margin按字段判断；不建立Global消费白名单。未决分类必须给出具体问题、当前源码和决策所有者。
- [ ] **Step 5：校验覆盖并交付。** CSV覆盖全部已核定终端字段；已有90组ID可解析，字段没有消费者时不得填“当前已生效”。代表Button/LineEdit/Select/Skeleton/Timeline/DataGrid/ColorPicker形成清晰合同，余下待决项进入对应家族的开工门槛。

**检查：** 主键唯一；生成扁平schema与记录双向匹配；非空Active/Derived消费记录可定位；Legacy映射无隐式同名猜测；每项Undecided有后续主任务。

**出口：** 可确定哪些是接线修复、求值重构、历史兼容或新增能力决策；不要求先给所有字段发明新API。

## Task CT-02：建立代表失败与Host/性能基线

**Files**

- Read: tests/AtomUI.Desktop.Controls.Tests/Buttons/ButtonThemeScopeTests.cs、Theme/ControlEffectiveGlobalBindingTests.cs、Theme/ThemePopupInheritanceTests.cs、Timeline/TimelineSemanticStylePriorityTests.cs。
- Read: tests/AtomUI.Desktop.Controls.DataGrid.Tests/Theme/DataGridTokenAlignmentTests.cs（只验证默认对齐，不能代替覆盖后消费）。
- Read: src/AtomUI.Desktop.Controls/Buttons/ButtonToken.cs、Buttons/Themes/ButtonTheme.axaml、Primitives/AddOnDecoratedBox/，src/AtomUI.Controls/Primitives/ArrowDecoratedBox/ArrowIndicator.cs及ColorPicker实际消费者。
- Temporary: tests/AtomUI.Desktop.Controls.Tests/Temporary/CustomThemeBaseline/PreciseThemeBaselineTests.cs、PreciseThemeBaselineView.axaml、PreciseThemeBaselineView.axaml.cs。
- Temporary: tests/AtomUI.Desktop.Controls.DataGrid.Tests/Temporary/CustomThemeBaseline/DataGridThemeBaselineTests.cs。
- Create/Modify: 本目录baseline.md；截图、录屏、差异图及取证二进制全部放仓库外系统临时目录，查看和比较后清理，最终只保留文本结论，不提交或嵌入取证媒体。

**Interfaces**

- Consumes: CT-00的owner/Host与CT-01的字段合同；配置走ThemeConfigBuilder.WithToken及WithControl(identity, ControlThemeConfig)，不直接改token builder模拟用户覆盖。
- Produces: 每项包含触发输入、期望、实际、目标/来源、几何/像素、Host、执行命令、证据状态、测试处置的记录。后续任务必须复用同一输入和断言做red→green。

- [ ] **Step 1：核对已有保障并编写最小反例。** 临时测试namespace统一为AtomUI.Desktop.Controls.Tests.Temporary.CustomThemeBaseline。复用现有AvaloniaTestApp、窗口清理和调度方式，Desktop项目的UI用例使用既有ButtonThemeScopeTestCollection以隔离全局宿主；DataGrid项目沿用其已有串行宿主合同。只读查找真实节点可以取证，样式定制只能在AXAML使用生成专用Style。下列名字是待创建测试，不是已有或已通过用例。

| 场景/测试名 | 输入与断言 | 后续 |
| --- | --- | --- |
| B01 Button_Control_LineWidth_Is_Isolated_From_Siblings | 根LineWidth=1，Button identity覆盖4；目标框体4，未覆盖兄弟保持1，public子Control按自己的identity | CT-10/CT-12 |
| B02 Button_Own_LineHeight_Updates_Default_Padding | Middle、非圆形文字按钮；全局ControlHeight=50、LineWidth=2，Own ContentLineHeight=10且不覆盖padding，纵向padding=18；再显式ContentPadding=3,7，纵向保持7 | CT-03–CT-05/CT-12 |
| B03 LineEdit_Root_Frame_Inputs_Reach_The_Visual_Frame | 实例Padding=11,13、BorderThickness=3、CornerRadius=0；实际框体职责对应值相等，addon/compact/校验及普通状态保持 | CT-10/CT-13 |
| B04 DataGrid_Public_Overrides_Reach_Actual_Consumers | HeaderBg=#123456、CellPadding=3,5；真实表头色和单元格padding相等；记录实际Table*旧路径，不能只断言新builder字段 | CT-06/CT-18 |
| B05 Timeline_Part_Setters_Survive_Arrange | 使用生成的TimelineItemIconStyle设置BorderThickness=3、CornerRadius=0；Arrange及尺寸变化后真实目标仍取该值 | CT-15 |
| B06 Arrow_Size_Change_Invalidates_Geometry | 同一ArrowIndicator的ArrowSize由8→16→8；自然尺寸、绘制geometry相应更新，不以重建控件触发 | CT-10 |
| B07 Existing_Popup_Tracks_Owner_Typography | owner字体大小14→20→14，并改变Family/Weight/Style；已有Native/Overlay及新host实际文本更新，关闭后可释放 | CT-08/CT-10/CT-16 |
| B08 Gradient_Text_Updates_Without_Changing_Selection | 已有GradientColorPicker保持ActivatedStopIndex不变，改变前景/disabled/主题；目标文字取新状态色，旧实例不保留快照 | CT-19 |
| B09 Existing_Configuration_Matches_PreChange_Visual_Baseline | 每个适用Control有默认及既有已支持配置的同条件前后对照；位置/尺寸/排版/颜色/描边/阴影/自绘/状态及动效不得漂移。CT-02先做同版本A/A验证采样可重复，实际产品批次再做改动前/后比较 | CT-10–CT-21/CT-25/CT-26 |

B02关键断言须经过真实配置及模板，不能先改CalculateTokenValues再记录“修复前失败”。具体期望为：

~~~csharp
// 现有公开配置接口；此片段是测试输入/断言，不是本轮已执行的代码。
var own = new ControlThemeConfigBuilder()
    .WithToken("ContentLineHeight", "10")
    .Build();
var config = new ThemeConfigBuilder()
    .WithToken("ControlHeight", "50")
    .WithToken("LineWidth", "2")
    .WithControl(ButtonTokens.Identity, own)
    .Build();
// 将config给ThemeConfigProvider并显示Middle文字按钮，按现有宿主settle后：
button.Padding.Top.ShouldBe(18d); // (50 - 10) / 2 - 2
button.Padding.Bottom.ShouldBe(18d);
// 第二输入显式添加ContentPadding="3,7"，settle后应保持Top/Bottom=7。
~~~

- [ ] **Step 2：执行精确反例并分类。**

~~~bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj \
  --framework net10.0 \
  --filter 'FullyQualifiedName~CustomThemeBaseline.PreciseThemeBaselineTests'

dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj \
  --framework net10.0 \
  --filter 'FullyQualifiedName~CustomThemeBaseline.DataGridThemeBaselineTests'
~~~

预期：测试被真实发现；源码风险中可复现者因目标合同不满足而FAIL，不是编译错误、缺资产或零测试。每项分别记录失败、已有通过、未复现或环境受限；已有通过者保留作为对照，不人为制造失败。B07平台不足时保留未验证Host，不自动写成通过。

- [ ] **Step 3：建立视觉兼容基线与实际Host前证。** 为CT-00每个Control列出Host、主题/配置、尺寸/variant、状态、弹层及对应比较任务；CT-02先对代表场景同版本A/A采样，确认截图及布局可重复。使用当前worktree构建产物，记录真实路径/commit及未提交差异、OS/渲染Host/DPI/字体、ClientSize、语言、数据/资源、滚动/焦点和Native/Overlay模式；动态场景固定时间原点及采样时刻。保留原始高度、嵌套、内容、滚动和裁剪条件，B03/B05/B06/B08核对布局/绘制，不能只查看root属性。未采样Control保持待取证，实际修改前必须补齐；不能用代表截图宣称全范围通过。
- [ ] **Step 3a：证明视觉比较能够检错。** 在临时验证场景中仅将padding增加1个逻辑单位并确认差异被识别，再还原；这是取证流程的正对照，不改产品基线。原始前后图按相同坐标和尺寸比较并实际查看，不能缩放/对齐/遮罩控件区域，不能设置通用像素比例豁免。采样噪声先以同版本A/A定位并固定条件；有未解释差异则比较流程尚未就绪。
- [ ] **Step 4：冻结性能和生命周期采样条件。** 记录场景的实际控件/数据量、Host、构建模式、算法和scope。分别测首次使用、等价NoOp、Global更新、Own更新、嵌套和已打开弹层；首次使用保留冷启动样本，热场景先稳定再采样。固定后重复30次，保留中位数/p95、分配和对象数量；此步骤建立基线，不凭空宣布性能无回退。
- [ ] **Step 5：保存可重建证据并清理。** baseline.md保存反例配方、测试名、断言、真实结果、视觉兼容矩阵及后续任务；查看并记录不依赖媒体的实际数值/观察后，清理仓库外取证媒体以及临时C#/AXAML/helper/进程和专属构建产物。后续修复每批改码前重新开启同条件前后对照流程，前证在改码前采集、后证在改码后比较；不能只凭本轮文字或重建新期望宣布无差异，不建立永久失败测试或截图库。
- [ ] **Step 6：最终文档核对与M0审查。** 验证目录/分类/问题映射无漏项、临时文件零残留和git diff --check。若仅保留文档则不运行不相关产品回归；临时扩展过既有测试时先还原该临时修改，再用其精确filter确认恢复后的对照。

**出口：** 有可重复的失败与当前对照、视觉兼容矩阵、同版本A/A及差异检出正对照、Host/未验证边界和基线采样协议。未修复的问题仍待处置；本批没有green修复或改动后视觉一致性证据，不报告“主题系统已支持”或“全部控件视觉无差异”。

## 后续计划交接

M0审查通过后，先分别编写Own版本/生成/Compiler、别名兼容、品牌资格/协调三个接口明确的子计划；CT-10/CT-11可以同时准备其独立消费链计划。每个新子计划携带具体类型/签名、可失败断言、临时验证位置、最终focused命令及兼容边界，再进入实施。

不存在“完成目录后按路线图自动改全部控件”的步骤；每个家族从CT-00精确对象展开，仍按同一合同和证据闭合。
