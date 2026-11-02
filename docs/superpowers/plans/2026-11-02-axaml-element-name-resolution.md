# AXAML 名称绑定解析修复计划

> **For agentic workers:** 使用 superpowers:executing-plans 在当前目录执行。用户已要求全部修复；不另行请求执行方式确认。

**Goal:** 修复截图及仓库内同源 AXAML 名称绑定解析错误，保持模板与绑定行为。

**Architecture:** 在 Rider 原文件对单个名称声明做单变量实验，将被名称绑定引用的普通 `Name` 改为 `x:Name`。保留全部原始绑定表达式，不改公开 API、节点名称、绑定优先级、模式、转换器和布局。

**Tech Stack:** Avalonia AXAML、.NET 10、Rider、现有 Headless 测试。

**Spec:** 本次用户截图与“这种类似的要全部修复”。

## 约束和完成条件

- 当前分支 release/6.0、当前工作目录；不创建分支/worktree，不提交。
- 不修改已有 AtomUI.slnx、README 和其他任务文档的改动。
- 修复原场景、扫描全部同源引用、验证受影响模块、清理临时测试。
- 不扩展为清除 Rider 所有类别的诊断，不运行全量回归或无关发布。

## Review Focus

- 名称仍在同一个 NameScope 中，前向引用可用。
- 保留 Child 可见性、取反、转换器、TwoWay 及附加属性路径。
- 保留三类滚动模板的手势同步，不改变滚动 UX。
- Gallery Root 引用仅在确认同源后调整。

## Task 1：原场景 A/B 与同类修复

- [x] Rider 原文件显示 4 个错误：两组 Child / PART_* 无法解析；节点名称均存在。
- [x] 单独调整左 AddOn 引用，观察对应诊断；仅改 `#` 仍有名称诊断，排除“只需改绑定语法”的假设。
- [x] 左侧声明改 `x:Name` 后保留原 `ElementName`，对应两条诊断消失；右侧仍报错，单变量实验成立。
- [x] 检查全部 ElementName、#名称引用及相应声明，修正 62 个 AXAML 文件的 98 个名称声明（9 个控件库主题、53 个 Gallery 页面）。

## Task 2：验证与交付

- [x] 复用已有测试，临时验证三类 AddOn 动态显隐、三类滚动手势属性同步，最终方案 6/6 通过（600ms）。
- [x] Rider 原文件的 Child 与两个 PART_* 红线消失，Child 类型提示正确；编辑器错误标记由红变绿。全库命名引用扫描无同类遗漏，最终 AXAML 已真实编译。
- [x] 临时测试和专用目录已删除；模块 iterate 已完成，发现的 5 个源码断言失败已修正，精确 5 类 23/23 通过；最终 Gallery 412/412 通过。
- [x] 最终独立审查确认 62 文件 98 处仅名称声明变化，Avalonia NameScope 与名称生成字段范围不变；XML 与 diff 检查通过。

## 证据与决策

- 基线：Rider Problems → File，ButtonSpinnerDecoratedBoxTheme.axaml 4 problems，行 23/40 各 Child 与对应 ElementName 无法解析；全解决方案另有其他类别诊断，暂不扩大范围。
- 测试策略：IDE 专属解析诊断不能由普通 dotnet test 重现，使用可复查的 Rider Problems 前后结果；运行测试验证模板绑定行为未退化。
- 执行规则：按用户强约束覆盖技能中的自动分支、提交、全量测试和重复确认步骤；所有有效证据集中于本记录。
- A/B：原文件 4 problems；仅将两处绑定改为 `#` 后剩 2 problems（名称仍无法解析）；仅左名称声明改 `x:Name` 后剩右侧 1 problem；左绑定恢复原 `ElementName` 后仍只剩右侧 1 problem。由此最终撤回全部 20 处绑定语法改写，改为修复引用目标的名称声明。
- 初步运行验证：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~NamedTemplateBindingTests|FullyQualifiedName~AddOnContentTemplateTests|FullyQualifiedName~MessageStackAnimationTests'` 通过 28/28（1 秒，不含构建），覆盖三类 AddOn 动态显隐、三类滚动手势和消息背板；该次在最终名称声明方案前，只证明原绑定语法改写未破坏运行时行为，最终方案另行验收。
- 审查：初次独立审查指出 IDE 证据缺口；已按最终名称声明方案补齐原场景证据，并完成最终 diff 独立审查。
- 最终实现审查：确认 62 个 AXAML、98 处仅 `Name → x:Name`，原绑定、名称值、布局与其他内容逐字保持；名称生成器原本同时处理两种写法，不新增公开成员；`src/`、`controlgallery/` 扫描未发现仍用字面量 `Name` 声明的命名绑定目标。
- 最终模块验证（清理后）：以 62 个修改 AXAML 的精确 `--path` 执行 `python3 scripts/verification/test.py run --scope iterate`。构建成功；Rendering 1/1（3.165s）、Desktop 653/653（17.526s）、GalleryBase Generator 10/10（3.499s）通过。Gallery 初次 407/412 通过，5 个失败均为现有 Semantic Part/布局断言写死旧 `Name` 文本。
- 断言同步：Calendar、Tooltip、Message、Notification、Modal 5 个既有测试文件的 6 个定位/匹配字符串改为 `x:Name`，未删除或放宽其余断言；定向 5 类 23/23 通过（4s）。无永久测试净新增。
- 最终 Gallery 确认：复用已成功构建产物，执行 `dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-build --no-restore --filter <同一 iterate 计划选出的 116 类>`，412/412 通过、0 skipped（43s）。其他 3 个项目的源码、依赖和配置均未变化，复用其有效结果。合计 1,076 个相关模块测试通过；未运行全量回归。
- 专项判断：只改声明语法，不新增 binding/reflection、运行逻辑、生命周期或布局，无新增 AOT 发布义务；既有窗口绘制、滚动、Form 和 Semantic Part 验证覆盖本次风险。无需控件 API、Token 或 Gallery 维护架构文档变更。
- 清理与交付：临时测试及专用目录已删除，临时验证日志/计划 JSON 已清理；没有新增长期测试、helper 或运行进程。`git diff --check` 通过。保留正式修复 62 AXAML、5 个既有测试文件的断言同步和本记录；未创建分支/worktree，未提交。

## 后续截图：Gallery 引用 internal Setter 类型

范围：ImagePreviewer Gallery 两处 `x:SetterTargetType="atom:ImagePreviewRenderer"` 及其三份控件文档；全 Gallery 的 internal 类型引用扫描仅这两处生产引用。`ImagePreviewRenderer` 是 internal，`image` 的现有公开 ContractType 为 Avalonia `Control`，因此使用 `x:SetterTargetType="Control"` 和限定属性 `Border.CornerRadius`，不改公开 API、生成 Style 或实现类型可见性。

- [x] 临时测试扫描 Gallery 的 atom Setter 上下文，检查实际 Type.IsVisible；修复前精确失败于两处 ImagePreviewRenderer（575ms）。
- [x] Gallery 两处上下文使用 Control，限定属性和生成 Style 不变；同步三份控件文档，在既有 Semantic Part 架构文档明确公开 ContractType 规则。
- [x] 同一临时检查、真实页面 deferred 物化与圆角值、Semantic Part 高亮共 5/5 通过（5s）。
- [x] 临时检查已删除，Rider 复核及 diff 检查已通过；ImagePreviewer 清理后模块精确验证 6/6 通过（6s）。

验收边界：不以提高 internal 类型可见性、授予 Gallery 内部访问权限或 code-behind 设置圆角规避问题；不重跑先前不受影响的 62 文件验证。

IDE 复核：ImagePreviewerShowCase.axaml 的两条 `ImagePreviewRenderer is not accessible` 消失，File 面板仅剩原有 SourceKey `imagepreviewer` 拼写提示（行 271），不改示例键值规避提示。

测试生命周期：可访问性扫描放在 `tests/AtomUIGallery.Tests/Temporary/axaml-accessibility/`，修复前失败、修复后通过，现已删除文件及专用目录；长期保障复用现有页面实例化、专用 Style 命中和圆角断言，无新增永久测试。既有页面测试仅更新已失效的说明注释。

独立审查：两处 AXAML 无契约、圆角或新增 AOT 风险；审查发现 changelog 沿用旧 internal 上下文异常说明，已改为公开 Border 属性解析的当前事实。

最终验证选择：`run --scope iterate --path <ImagePreviewerShowCase.axaml> --path <ImagePreviewerShowCasePageTests.cs>` 因仓库输入在选测后变化返回 stale，未进入执行阶段；按同一映射精确运行 ImagePreviewerAssetLoadingTests、ImagePreviewerSemanticPartHighlightTests、ImagePreviewerShowCasePageTests 三类，不扩大到其他模块。

最终结果：`dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUIGallery.Tests.ShowCases.ImagePreviewerAssetLoadingTests|FullyQualifiedName~AtomUIGallery.Tests.ShowCases.ImagePreviewerSemanticPartHighlightTests|FullyQualifiedName~AtomUIGallery.Tests.ShowCases.ImagePreviewerShowCasePageTests'` 构建与 6/6 测试通过、0 skipped（6s）。`git diff --check` 通过；临时日志已清理，无新增常驻测试、公开 API、运行时分支或发布义务；未提交。

## 后续截图：WindowDrawnDecorations HasTitleBar 类型解析

目标：消除同一主题 3 处 HasTitleBar IDE 错误，保留 TemplatedParent 来源、And 转换器和 IsTitleBarVisible 条件。当前分支/目录内执行，不提交、不创建隔离环境，不扩大其他诊断。

- [x] Rider 原文件确认 3 个错误；当前 Avalonia 12.1.4 包和本地参考源码均确认 HasTitleBar 为公开 DirectProperty。WindowDrawnDecorationsTemplate 缺少 TargetType 上下文。参考路径：`/Users/chinboy/Projects/ReferenceProjects/Avalonia`。
- [x] 单变量 A/B：仅一处 Path 显式限定 WindowDrawnDecorations 类型，错误 3→2，剩余诊断精确对应 55/70 行；已推广剩余两处。第一处改写后真实主题的六次状态切换与三层显隐断言通过（1/1，222ms）。
- [x] 临时真实主题测试覆盖 HasTitleBar 与 IsTitleBarVisible 的四种组合及动态切换，验证三层同步；已有契约测试只同步类型限定路径，不放宽显隐条件。全三处改写后精确运行 `DrawnTitleBarVisibilityTests|WindowResizeArtifactTests`，63/63 通过（668ms）。临时测试已删除，不新增常驻用例。
- [x] 删除本次临时测试后执行 Window 精确模块验证、Rider 原文件复核和 diff 检查，补最终审查。

本节为本次唯一记录。IDE 专有诊断采用原文件 Rider 前后证据：dotnet 编译并不执行 Rider 的类型推断检查，不能以其通过冒充原错误修复。执行流程按用户约束省略重复确认、自动分支/提交和技能全量测试。

最终验收：

- Rider 原文件红色错误计数 3→2→0，Problems 仅余 9 条既有命名提示；所有 HasTitleBar 解析错误均消失。保留原窗口结构、布局、状态条件和资源绑定。
- 最终清理后执行 `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Window`，238/238 通过、0 skipped（测试 2s）。统一入口 iterate 曾因并发工作区改动在发现阶段退出，未启动测试、不作为通过证据；按用户允许的精确过滤方式完成 Window 模块验证。构建保留既有 NavMenu xUnit1051 warning，本次无新增编译错误。
- 独立只读审查无发现：普通路径与限定路径在编译期均解析同一个 HasTitleBarProperty；运行时通过 AvaloniaProperty 读取和订阅，不新增反射或 AOT 发布义务。绑定来源、优先级、And 逻辑和公开 API 不变，不需要控件用户文档调整。
- 临时测试、专用目录和本次临时日志均已清理，无常驻测试净新增；保留三处正式主题路径修改、一处既有契约断言同步及本节。未创建分支/worktree，未提交；`git diff --check` 通过。

## 后续截图：SemanticPartPreviewLangResource IDE 类型缺失

范围：恢复 GalleryBase 语言资源扩展类型在 IDE 的生成和解析，保留资源键、动态语言切换、现有 AXAML 与公开 API。继续在当前分支/目录执行，不提交、不新建分支/worktree，不扩大为其他类别的红线清理。

- [x] 原始 Rider ItemControl 中两处错误；类型搜索仅有资源枚举，没有资源扩展类；相同项目 Token 扩展类可见。AssemblyInfo 的 Localization xmlns 映射正确，普通构建生成的扩展类也正确。
- [x] 核对 IDE 生成器输入和诊断：临时生成器探针确认 IDE 仅收到 Header 的四份 XLIFF，SemanticPartPreview 四份全部缺失，诊断 ATOMUILOC006 导致 Catalog 输出为 0。Rider RoslynDocumentsSyncService 日志明确记录 `Ignore file: .../SemanticPartPreviewLang/*.xlf, build action AtomUILanguage`。重启生成器不改变该输入；问题为项目内显式自定义 Item，而非命名空间或译文。
- [x] 建立原文件 IDE 失败证据后删除 csproj 中重复 AtomUILanguage ItemGroup，复用公共 targets 自动发现；同一探针变为 Catalogs=2、8 份 XLIFF 全部 marked=true、无诊断，原 ItemControl 两处红线消失。`dotnet msbuild -p:TargetFramework=net10.0 -getItem:AtomUILanguage` 前后核对：8 个输入的 Identity、模块、来源和打包路径逐项相等；被删除的 ContractVersion=3 已无任何生产读取点。无需新增或放宽本地化行为/公开 API。临时生成器代码已撤除。
- [x] 原截图三份 AXAML 在 Rider 复核，运行本地化及预览模块验证，清理临时代码与证据文件，完成只读审查。

最终验收：

- 原始条件复核：不修改 AXAML，ItemControl 资源错误 2→0；CodeExampleControl、InfoControl 均为绿色分析状态，无语言扩展类型错误。正常编译生成的扩展形态与译文输入保持一致，保留动态语言切换和原布局。
- 失败证据采用原文件 IDE 与实际生成器输入探针：dotnet 编译不复现 Rider 的 Build Action 过滤，不能代替该证据。没有新增常驻测试，复用 GalleryBaseCatalogTests、SemanticPartPreviewTests 等模块验证。
- 临时探针及项目重载注释已撤除，LocalizationGenerator.cs 与基线完全一致，并重新构建恢复干净生成器（0 warning / 0 error）；本次生成的临时探针文档与日志已清理。最终只保留 csproj 删除重复声明及本节记录。
- 清理后执行 `dotnet test tests/AtomUI.Toolkits.GalleryBase.Tests/AtomUI.Toolkits.GalleryBase.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Toolkits.GalleryBase.Tests`，209/209 通过、0 skipped（测试 15s）。该范围为所修改 GalleryBase 模块，不运行全量回归。
- 独立只读审查无发现，确认公共 targets 继续提供 AdditionalFiles 和打包输入，删除的 ContractVersion 无生产消费者。未改运行时、公开 API、译文、生成器逻辑或裁剪配置，无新增 AOT 发布义务；无需控件用户文档变化。
- `git diff --check` 通过；未提交，未创建分支或 worktree。

## 后续截图：FormShowCase InitializeComponent IDE 缺失

目标：恢复 FormShowCase 及同源页面的 Avalonia 生成成员在 Rider 中的识别，保留页面构造顺序、源码镜像与片段 catalog。范围为产品 Gallery 的源码收集配置及其权威维护文档，不改页面初始化逻辑，不提交、不创建分支/worktree、不运行全量。

- [x] 原截图方法报红；普通构建生成文件含 InitializeComponent。Rider 日志明确忽略 Form 的 AXAML（GallerySourceCodeDisplaySource/AvaloniaXaml）和 code-behind（GallerySourceCodeDisplaySource）。该自定义 Item 在项目求值阶段重复占有真实源码的 IDE Build Action。
- [x] 记录镜像 target 当前输入；把三类源文件的收集移入 PrepareGallerySourceCodeDisplayAdditionalFiles 内。通过 MSBuild target 前后输出比对，292 个输入的 Identity、GallerySourceOriginalPath、GallerySourceMirrorPath 逐项一致。保留两阶段收集与元数据投影；直接把 glob 合并到投影 Item 的试验会令内置元数据为空，已撤回并清理其无效镜像。
- [x] 在原 Form 文件及代表性同源页面复核 Rider，验证页面物化与源码片段 catalog，更新 source-code-display.md 的收集时机约束。原 Form 文件已经确认 InitializeComponent 正常识别、红色错误 1→0；Cascader 同源文件也已正常识别。构造函数与页面代码完全未改。
- [x] 清理临时输入快照，最终 Gallery 模块/源码生成模块精确验证和独立审查。

最终验收与源头约束：

- 原始 IDE 故障由 Rider 文件角色/生成输入造成，dotnet 编译不能复现该 IDE 专有失败；使用原文件前后证据与 292 项真实 target 输出比对，不另建近似页面或手写初始化方法。
- 最终精确验证：`dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~FormShowCase|FullyQualifiedName~CascaderShowCasePageTests|FullyQualifiedName~CheckBoxShowCasePageTests|FullyQualifiedName~ColorPickerShowCasePageTests|FullyQualifiedName~DatePickerShowCasePageTests|FullyQualifiedName~DateViewerShowCaseTests'`，18/18 通过、0 skipped（9s），覆盖页面物化、Form 语义 Tab 及真实片段 provider。
- `dotnet test tests/AtomUI.Toolkits.GalleryBase.Generator.Tests/AtomUI.Toolkits.GalleryBase.Generator.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~ShowCaseCodeSnippetCatalogGeneratorTests`，10/10 通过、0 skipped（1s）。无常驻测试净新增；输入快照、临时日志与无效镜像已清理。
- 独立审查确认生产改动保留 glob、两阶段 metadata 投影、复制和 AdditionalFiles 注册。按审查意见，将维护文档中的 IDE 结论限定为已经验证的 Rider 场景，避免声称日志直接证明 Compile 输入被移除。
- 用户追加要求“从源头优美地解决”。最终实现只调整所有 ShowCase 共用的收集入口，真实文件维持原生角色，新增页面自动沿用同一流程；不增加页面补丁、生成方法兜底、条件关闭诊断或专用验证框架。仓库 src/controlgallery/tests 的 csproj 求值阶段辅助源码自定义角色扫描无剩余候选；共享 build 中三处语言自动发现是既有权威入口，已由上一节 8 份实际 IDE 输入验证，不做无证据改写。
- 更新两处权威维护约束：source-code-display.md 的 target 内收集时机、theming-localization.md 的 GalleryBase 标准目录自动发现。未新增自动审计门禁，不把文档规范或本次静态扫描表述为未来自动保障。
- 无运行时、公开 API、AXAML 结构或生成器实现变更；无新增 AOT 发布风险。未运行全量回归、未提交、未创建分支/worktree，`git diff --check` 通过。

## 后续截图：DataGridColumn 从未赋值字段

范围：删除 `_settingWidthInternally` 失效状态及恒真分支，保留原有 Width 回调、InheritsWidth 行为和 `_setWidthInternalNoCallback` 的真实重入控制。属于 L0 等价清理，不改公开属性、布局、模板、资源、成员顺序或文件拆分。

- [x] 全仓引用只有声明和一处取反读取，无赋值、无反射/生成器访问；追踪内部宽度更新均走 SetWidthInternalNoCallback。字段始终 false，原分支等价于直接设置 InheritsWidth=false。
- [x] 以当前真实 DataGrid 项目、CS0649 作为错误建立失败编译证据：`dotnet build src/AtomUI.Desktop.Controls.DataGrid/AtomUI.Desktop.Controls.DataGrid.csproj --framework net10.0 --no-restore -p:BuildProjectReferences=false -p:WarningsAsErrors=CS0649` 在强制检查当前源文件后失败，唯一错误为该字段 CS0649（27.43s）。首次增量命中缓存的 0 warning 不作为有效失败证据。
- [x] 移除字段和条件，保持现有回调其余逻辑；同一 CS0649 错误级编译检查通过，0 warning / 0 error（9.22s）。Git 历史进一步确认：36bca5d7f 已删除零引用 SetWidthInternal 方法及该字段全部赋值，留下了字段与条件；本次清理这两个遗留点，不恢复已删除方法。
- [x] 验证列宽布局模块、运行两个完整文件的成员审计、清理临时日志并完成审查。无新增常驻测试，无分支/worktree，无提交。

最终验收：

- `dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --filter 'FullyQualifiedName~DataGridStarColumnLayoutTests|FullyQualifiedName~DataGridFrozenColumnLayoutTests|FullyQualifiedName~DataGridColumnGroupHeaderIndexTests'`，15/15 通过、0 skipped（测试 3s）。覆盖 star/fixed 列宽、拖动调整、空/有数据转换、最小最大宽度、冻结与分组列头。首次 no-restore 因该测试项目缺 assets 退出，恢复依赖后同一过滤范围通过；没有吞掉测试失败。
- 原始 IDE 文件已不含失效字段，对应编辑器警告消失；Rider Build Output 保留的是用户上一轮构建的历史 CS0649 记录，不把它当作当前源码诊断。当前真实模块的 CS0649 错误级编译为 0 warning / 0 error。
- 成员审计范围为两个完整文件、一个 partial 控件类型；前后均有相同 26 项存量排列问题（wrapper 顺序 1、内部协作区域 3、事件区域 2、接口区域 1、API/接口相对顺序 19）。逐项身份比较无新增，按用户有限任务范围保留记录，不为本字段警告扩展成员重排；未声称成员审计全绿。
- 独立只读源码/历史审查无新增问题，确认原分支恒成立、真实重入标志和宽度回调未改。无公开契约、模板、主题绑定、资源生命周期或 AOT 动态行为变化，无控件文档/API 更新或额外发布义务。
- 复用编译器诊断与已有行为测试，无新增测试、helper 或临时工程；本次临时日志已清理。正式改动仅两个遗留点和本节记录；`git diff --check` 通过。未运行全量回归、未提交。
