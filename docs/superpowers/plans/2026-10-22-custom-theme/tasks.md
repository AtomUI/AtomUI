# 精确主题优化与重构任务目录

本文的进度表是CT任务状态的唯一总入口；全局约束与阶段出口见[路线图](overview.md)。下表是任务包，CT-00–CT-02的详细步骤见[首批执行计划](readiness.md)；其余包须在前置出口通过后细化文件、接口和验证，不作为现成产品实现计划。

## 0. 执行进度

更新日期：2026-10-22。工作位置为`feature/custom-theme`及现有worktree，源码审计基线`85407138f`；本轮阶段提交记录截至CT-08A的成果，整体重构尚未完成。CT-00/CT-01完成的是目录与源码分类，不表示控件或主题机制已经修复。**已完成CT-04/CT-05及CT-06机制和DataGrid17项兼容接入；Own V2产品迁移与完整控件家族视觉验收仍为0项**，不能把准备工作换算成视觉验收完成率。

| ID | 状态 | 已完成与证据 | 下一步／未完成门槛 |
| --- | --- | --- | --- |
| CT-00 | 已完成（目录） | [目录](control-inventory.csv)720个元数据主键双向一致；[基线](baseline.md#目录范围) | 305个owner待决保留给对应家族，不伪造identity |
| CT-01 | 已完成（源码分类） | [字段记录](token-contract-review.csv)95个Own终端／1195字段，默认依赖及消费者源码；90组ID映射校验通过 | 123未决字段、124合同决策由对应家族处理；无Approved字段，不代表实际消费已验收 |
| CT-02 | 已完成（代表基线） | [执行清单](readiness.md#task-ct-02建立代表失败与host性能基线)、[结果](baseline.md)：12个目标反例失败／1正对照通过；Skia A/A及1DIP检错；实际Window/Popup Host；启动/热更新和释放样本 | 独立审查Critical/阻断Important均0；原生最终显示面无输入闪动仍归CT-12/CT-25专项，不能声明无闪烁 |
| CT-03 | 已完成（接口合同） | [正式具体接口](../../../architecture/systems/theming/own-token-evaluation.md#9-ct-03具体构建与运行接口)及[实施计划](own-defaults.md)已核对旧路径／版本／跨包／格式边界 | API已由CT-04/05实现；尚未发布，不代表95个产品Token已迁移 |
| CT-04 | 已完成（实现与专项） | [CT-04A/B清单](own-defaults.md#task-ct-04a可用的版本化声明与分派合同)：版本分派／生成计划；20个临时生成用例、75项聚焦对照；.NET8构建、跨包macOS26/arm64 NativeAOT运行通过 | 已随CT-05清理并通过机制审查；没有迁移产品控件或批准全平台 |
| CT-05 | 已完成（实现与专项） | [实现与验证](own-defaults-results.md)：有效Own／存在位、算法／appearance、缓存、惰性／重试及scope；清理后Core47／Generator63通过，跨包AOT实际Compiler通过 | 机制最终审查Critical/Important/Minor均0；进入CT-06，不代表产品控件已迁移 |
| CT-06 | 已完成（机制与17项兼容） | [结果](aliases-results.md)：实际消费者4/4；产品Skia37对差异0/1DIP检错8649像素；清理后Core58/Generator64/DataGrid22通过，NET8构建和当前Mac动态AOT通过；作者自审完成 | 全部临时源/媒体已清理；完整DataGrid家族、compiled literal别名NativeAOT及原生显示面仍按CT-18/CT-25验收 |
| CT-07 | 已完成（最小原型） | [结果](qualification-results.md)：9/9PASS，Card编译专用Style、重置、优先级、资源拒绝及10代frame清理；清理后Card11/Core21/Generator1通过，作者自审完成 | 临时源已清理；产品品牌装配/跨根事务、性能与最终视觉仍属CT-08/CT-25 |
| CT-08 | 进行中（A已完成） | [A结果](style-assets-results.md)：4个internal准备组件；Core临时15/15、编译AXAML产品资产1/1；清理后Core29/Card11及NET8零警告通过，作者自审完成 | 组件尚无产品调用；下一子项接context/Provider/跨根lease，再细化协调结果与全部事务入口，不能关闭CT-08 |
| CT-09 | 未开始 | — | 等CT-08；独立版本化品牌配方格式 |
| CT-10 | 未开始 | 框体、箭头和跨根字体已有失败前证 | 共享消费链详细计划、前后对照、生命周期 |
| CT-11 | 未开始 | — | 动态Transition／motion／wave逐链路计划与证据 |
| CT-12 | 未开始 | Button依赖失败及B10离屏稳定性对照 | 等所需机制；逐按钮identity、实际视觉与原生闪动专项 |
| CT-13 | 未开始 | LineEdit可见frame未消费实例输入的前证 | 逐输入／选择控件及状态整改，保留焦点与输入 |
| CT-14 | 未开始 | [目录](control-inventory.csv)与字段候选 | 容器／导航逐Control合同、回收和状态验收 |
| CT-15 | 未开始 | Timeline专用Style被Arrange覆盖的前证 | 反馈／展示控件逐Control、Part、弹层及形态验收 |
| CT-16 | 未开始 | 日期／时间控件目录、跨根字体前证 | 每个日期／时间终端及公开子Control验收 |
| CT-17 | 未开始 | Skeleton端点helper依赖已补查 | 进度／骨架等绘制消费、显式派生与缓存 |
| CT-18 | 未开始 | 已有失败前证；CT-06已接17对别名，代表header/cell/footer/filter消费及视觉通过 | 等CT-06及共享机制；虚拟化、冻结、裁剪与实际消费 |
| CT-19 | 未开始 | 同选择下渐变文字保留旧前景的前证 | ColorPicker完整尺寸／颜色／状态、scope与释放 |
| CT-20 | 未开始 | 尺寸／Custom候选已挂各家族 | 随家族逐属性区分基础值和预设，不批量替换 |
| CT-21 | 未开始 | GalleryBase／Gallery目录已纳入 | 工具壳自身Token、预览owner隔离及视觉对照 |
| CT-22 | 未开始 | — | 等品牌与控件机制；三品牌及边界配方真实渲染 |
| CT-23 | 未开始 | — | 手动工作台编辑／预览／应用／保存／往返闭环 |
| CT-24 | 未开始 | — | 手动闭环后接受限AI草稿与实际服务，独立验收 |
| CT-25 | 未开始（已有基线样本） | [基线](baseline.md)记录macOS Host、性能／释放及未验证边界 | 随机制／家族补真实trim/AOT／各Host平台和性能对照；无输入原生闪动待验 |
| CT-26 | 未开始 | — | 汇总逐Control／90组子项、兼容与视觉证据，清理并最终审查 |

状态规则：`未开始`→`规划中`→`进行中`→`最终审查中`→`已完成`。有真实外部障碍才标`受阻`并写明条件；依赖尚未满足不伪报外部阻塞。每项开始、验证、清理和审查后在此表更新状态和证据链接，详细步骤在其子计划勾选，实际数值写入结果记录。中途问题或状态问答不会撤销继续执行授权；处理后回到当前任务，不以一条总结代替继续工作。提交须单独遵守用户的明确提交指令。用户已明确要求后续所有工作由主agent独立完成，不再使用subagent；后续最终审查由作者另起审查步骤执行，如实标明自审边界。

## 1. 基础合同与机制

| ID | 依赖 | 工作和边界 | 独立交付/验收 |
| --- | --- | --- | --- |
| CT-00 | 无 | 对照产品public Control、生成注册、Token、Semantic、主题导出和Gallery建立完整主目录；内部目标/抽象层/无Own控件单独分类 | 精确程序集/metadata name/identity/owner/平台/源路径及文档入口；候选不冒充完整Control数 |
| CT-01 | CT-00 | 核对每个终端Own语义及默认依赖，区分Active/Derived/Legacy/Reserved/Deprecated；明确历史字段、别名和未公开能力 | 分类记录及逐Control文档修订范围；无消费者不能标Active已交付，不为字段补造不存在功能 |
| CT-02 | CT-00/CT-01 | 复现代表性owner、Own依赖、框体、优先级、自绘、别名和动态问题；建立视觉兼容比较矩阵及同版本A/A对照，冻结真实Host、原始UX和采样条件 | 可重建失败配方、可重复视觉基线及每个Control的前后比较安排；未复现/未取证项不填通过 |
| CT-03 | CT-01/CT-02 | 明确新版Own声明、可访问静态默认函数、存在状态、依赖/重定义、descriptor/evaluator修订与旧路径迁移的具体API | 正式文档中的版本及公开签名审定；旧whole evaluator、slot与schema边界保持；作为CT-04/CT-05唯一接口输入 |
| CT-04 | CT-03 | Generator读取同/跨程序集元数据，验证依赖图并生成扁平静态求值顺序；声明层移动不改终端schema | 循环/未知输入/类型/重定义/版本混合诊断，确定输出、跨程序集和实际AOT消费 |
| CT-05 | CT-04 | Compiler按显式存在状态保留用户值，其余默认读取有效上游；缓存纳入evaluator修订，保持独立惰性槽 | 上游覆盖/显式下游/恢复/0/null/明暗/算法均成立；先用Button、Skeleton、ColorPicker证明再迁移 |
| CT-06 | CT-01/CT-02 | 别名在每个配置层先规范化，再按层合并；DataGrid17项规范字段与旧Table入口保持兼容 | 同层不同值冲突、同值归一、跨层正常覆盖；公开字段到真实消费者，导出及internal历史字段有版本决策 |
| CT-07 | CT-01/CT-02 | 以最小原型证明owner资格工厂、嵌套继承/重置、selector优先级和候选资源隔离 | 父padding/重置子foreground反例通过；实例/状态/专用Style覆盖符合原生规则；没有运行期反射或找节点改值兜底 |
| CT-08 | CT-07 | ThemeManager协调准备、接线、预激活、再校验/Token提交、不可回滚Publish与退役；版本化表现结果和跨根lease | 已打开/新建Native及Overlay一致；重入/stamp/激活/恢复/退役故障独立注入；普通观察者warning与表现失效区分 |
| CT-09 | CT-08 | 独立版本化品牌组合manifest及注册资产引用/修订/能力要求；复用Token编译与现有格式边界 | Token-only与完整配方范围明确，往返等价；未知资产/Control/版本有诊断，XML v1/ABI v1不静默变化 |

主要文件边界：

- CT-00/CT-01：现有审计CSV、控件公开合同、主题导出；参考[注册构建器](../../../../src/AtomUI.Generator/Registration/RegistrationModelBuilder.cs)、[Token定义遍历](../../../../src/AtomUI.Generator/DesignToken/ControlTokenPropertyWalker.cs)和[Semantic Part设计](../../../architecture/systems/theming/semantic-parts.md)。不为目录清点新增产品运行时注册路径。
- CT-03–CT-05：[ControlTokenDescriptor](../../../../src/AtomUI.Core/Theme/Schema/ControlTokenDescriptor.cs)、[ThemeCompiler](../../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs)、[配置规范化](../../../../src/AtomUI.Core/Theme/Configuration/ThemeConfigNormalizer.cs)、[继承模型](../../../../src/AtomUI.Generator/DesignToken/ControlTokenInheritanceModel.cs)、[校验器](../../../../src/AtomUI.Generator/DesignToken/ControlTokenCalculationValidator.cs)、[schema生成](../../../../src/AtomUI.Generator/ThemeSchema/GeneratedThemeSchemaWriter.cs)及[生成入口](../../../../src/AtomUI.Generator/TokenResourceKeyGenerator.cs)。
- CT-06：[DataGridToken](../../../../src/AtomUI.Desktop.Controls.DataGrid/DataGridToken.cs)、实际模板/消费者及同一规范化边界；不能只改默认值测试。
- CT-07–CT-09：[ThemeManager](../../../../src/AtomUI.Core/Theme/ThemeManager.cs)、[事务](../../../../src/AtomUI.Core/Theme/ThemeTransaction.cs)、[Scope](../../../../src/AtomUI.Core/Theme/ThemeScope.cs)、[Provider](../../../../src/AtomUI.Core/Theme/ThemeConfigProvider.cs)、[专用Style生成](../../../../src/AtomUI.Generator/SemanticParts/SemanticPartManifestWriter.cs)及现有资源资产/格式所有者。是否新增文件、属性或结果类型由各子计划明确。

## 2. 共享消费与逐控件迁移

| ID | 依赖 | 家族/主要职责 | 独立交付/验收 |
| --- | --- | --- | --- |
| CT-10 | CT-02 | shared/Effective Global选择、InputControlFrame/AddOnDecoratedBox、root/frame、Icon、箭头、排版、Splash/TextSelectionHandle及跨根字体 | 正确公开owner和public子Control身份；既有实例/已打开弹层更新；无本地值压制或资源泄漏 |
| CT-11 | CT-02 | 动态Transition、motion/wave输入、WaveSpirit实际easing/时长/范围 | owner变化/主题切换更新且可释放，Transitions保留style setter；自绘实际消费公开输入 |
| CT-12 | CT-05/CT-10/CT-11 | Button、HyperLinkButton、DropdownButton、SplitButton、OptionButton及实际按钮派生项 | 行高/padding依赖、字重、阴影、shape恢复、三档公式与字段兼容；各identity独立 |
| CT-13 | CT-10/CT-11；有Own依赖者加CT-05 | LineEdit/TextArea/NumericUpDown/OTP、Select/ComboBox、CheckBox/Radio等输入选择 | 字体、frame、option padding、禁用态、resize/下划线几何、public属性和Part有效；保留输入与焦点 |
| CT-14 | CT-10/CT-11 | ListBox/ListView、Tree、Menu/NavMenu、TabControl/TabStrip、Cascader/TreeSelect/Transfer、Pagination/Expander | 容器owner/回收、方向与selected/open状态、组标题、密度；T37未公开能力按CT-01结论处置 |
| CT-15 | CT-10/CT-11 | Alert、Card、Message/Notification/Dialog、ToolTip/PopupConfirm、Timeline、Avatar、Tag/Badge、FloatButton、ImagePreviewer/Carousel及Result | 实际区域/关闭图标/弹层尺寸/字体/阴影/形态入口有效；稳定Part；插画/命名色保留内容语义 |
| CT-16 | CT-10/CT-11 | DatePicker、TimePicker、Calendar及范围条/内部Presenter/公开子Button | Own取正确owner，头部与范围标签可定制，字体/描边/密度不破坏日期状态、焦点和弹层 |
| CT-17 | CT-05/CT-10/CT-11；不含Own依赖者不等待CT-05 | Skeleton、Progress各形态、Rate、Slider、Switch、Separator、Watermark及相关绘制目标 | 标题/渐变/线宽/字体/几何实际消费；极值、细线及动态缓存；固定几何与开放形态明确 |
| CT-18 | CT-06/CT-10/CT-11 | DataGrid表头/单元格/过滤器/选择列/拖拽/展开图形及虚拟化 | 全部规范字段/别名、内部非Visual对象、自绘线条及Custom；原有滚动、裁剪、冻结和回收保持 |
| CT-19 | CT-05/CT-10/CT-11 | ColorPicker/GradientColorPicker、ColorBlock/Spectrum/SliderThumb/输入区域 | 尺寸依赖、HEX/HSV/RGB布局、文字/disabled更新、空值标记/线宽/选择环、scope与释放 |
| CT-20 | CT-01/CT-02；逐家族协调 | 58处Custom/Middle候选逐属性分离基础默认与预设；扩展至CT-00发现的其他尺寸入口 | 每处有实际职责及结论，Custom不接管其未拥有维度；保留三档、方向、Min/Max和输入几何 |

CT-12–CT-19的边界由CT-00精确Control目录补齐：表中示例不能排除未列出的Control，也不能以某个共享修复替代每个实际模板、状态和Part的验证。跨家族同名字段不自动继承配置。

全部控件任务共同遵守[视觉一致性硬门槛](../../../engineering/development/theming/verification.md#视觉一致性硬门槛)：各Control修改前采样，修改后同环境/配置/状态比较；正常和适用交互状态、明暗、尺寸/密度及弹层均有结论。未解释或未经批准的差异为零才可完成，不能只证明Token或Part已生效。共享修复涉及的消费者逐项纳入，CT-21工具壳也不豁免。

CT-10重点路径：[InputControlFrame](../../../../src/AtomUI.Desktop.Controls/Primitives/InputControlFrame.cs)、[框体主题](../../../../src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/Themes/InputControlFrameTheme.axaml)、[ArrowIndicator](../../../../src/AtomUI.Controls/Primitives/ArrowDecoratedBox/ArrowIndicator.cs)、Core/Icon及实际跨根host。CT-11参考[WaveSpiritDecorator](../../../../src/AtomUI.Controls/Primitives/WaveSpiritDecorator.cs)和[主题](../../../../src/AtomUI.Desktop.Controls/Primitives/Themes/WaveSpiritDecoratorTheme.axaml)。

各家族子计划必须列出具体C#/AXAML、测试、控件overview/implementation/token/semantic-part/changelog及Gallery示例，说明哪些原合同保持、哪些需要版本决策。CT-20的结论随对应家族提交，不另起一次无差别批量修改。

## 3. 品牌、工具和交付

| ID | 依赖 | 工作和边界 | 独立交付/验收 |
| --- | --- | --- | --- |
| CT-21 | CT-10/CT-11 | GalleryBase中立工具壳、SemanticPartPreview、元数据/页头、代码查看器 | 工具自己的排版/密度Token，不污染被预览控件owner；代码字体/语法主题及合理断点单独说明 |
| CT-22 | CT-08/CT-09；完整交付加CT-12–CT-20 | Ant默认、shadcn、Bootstrap及零圆角/粗线/大字/紧凑/关闭动效配方 | 配方多维进入真实渲染；状态/尺寸/嵌套/弹层一致；缺Host能力有诊断 |
| CT-23 | CT-09/CT-21；完整交付加CT-22 | 复用workspace目录/主题菜单，完成草稿/有效值/预览/应用/保存、diff/恢复、完整导入导出 | 同一runtime，兄弟预览隔离，已有实例状态保持，Token与表现结果不混淆；取消/过期不覆盖新草稿 |
| CT-24 | CT-23手动闭环通过 | AI仅生成受限schema/已注册资产/类型化Part Setter草稿，接实际服务 | 不执行代码/运行期AXAML、不应用半流；非法输入/旧响应/取消/超时与权限分别验证；独立里程碑 |
| CT-25 | 机制及各家族逐批接入；最终等CT-04–CT-23适用任务 | 注册/资产/跨程序集、真实trim/NativeAOT、Browser与各OS、性能/生命周期对照 | 每个Host/平台分别有证据；无全量实例化、反射旁路、无界缓存或旧Visual保留；无证据平台不填通过 |
| CT-26 | CT-12–CT-23/CT-25；AI交付另核CT-24 | 汇总逐Control/问题处置及视觉兼容矩阵，清理临时验证/废弃接线，同步正式文档与迁移资料 | 全目录有结论，90组每个子项闭合或有范围/兼容决策；无未解释/未经批准视觉差异，最终focused验证和审查完成后再申请提交 |

CT-21/CT-23使用现有[GalleryBase](../../../../src/AtomUI.Toolkits.GalleryBase)及[CustomizeTheme](../../../../controlgallery/AtomUIGallery/ShowCases/General/CustomizeTheme)；产品品牌及生成服务属于Gallery，稳定中立能力才下沉GalleryBase。CT-22/CT-24不得另外建立ThemeManager、Token字典或Style优先级引擎。

## 4. 审计追踪与新增发现

[映射表](problem-task-map.csv)为每个已有审计ID给出唯一主任务。协作任务描述共享修复、同组其他Control或最终证据义务；不代表主任务做完就能跳过协作方验收。TH/EVT及原始位置继续读原问题矩阵。

目录或运行验证产生的新问题先补审计事实与负责任务，再安排整改；不能只追90组而漏掉未被静态候选命中的控件。兼容保留不等于删除字段，范围外必须说明语义或平台原因。

本轮新增机制事实（不冒充原M0源码清单的已修控件）：

| 发现 | 责任与处置 | 当前证据/状态 |
| --- | --- | --- |
| inactive Style.Resources仍进入资源搜索，theme-only字典可能被HasResources遗漏 | CT-08准备期检查完整资源结构 | [CT-07反例](qualification-results.md)及[A实现](style-assets-results.md)已验证；产品host装配尚未接入 |
| 原生Remove与Add对逻辑/继承子树覆盖不同，模板frame可残留 | CT-08 lease退役先失活，再清frame及验证释放 | CT-07原型10代清理通过；实际跨根/性能/失败恢复仍待验 |
| 后赋资格root不能自动证明子规则Nesting，独立子selector可绕过资格 | CT-08A重新执行原生Nesting校验并在失败时不标记图 | A内部准备已实现/验证；不使用私有selector反射或找节点改值 |

任务拆分以独立可审查的合同为准：一项默认依赖、一条真实消费链或一个状态/形态合同可以独立交付；不按文件数量或任意固定工期拆分。暂不估算全项目日期，先以CT-00–CT-02确定真实规模。
