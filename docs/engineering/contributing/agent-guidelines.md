# AtomUI AI 协作规范

这份文档承载面向 AI 编程助手的具体工程约束。`AGENTS.md` 只引用它；后续如果某条约束属于 AOT、Gallery、资源生命周期、发布流程等更具体领域，应移动到对应专项文档，并在这里保留简短索引或原则。

## 约束放置原则

- 根目录 `AGENTS.md` 只写项目入口、文档路由和稳定协作原则。
- 具体、可执行、会增长的规则放到 `docs/engineering/`、`docs/gallery/`、`docs/modules/` 或控件文档中。
- 新增规则前先判断归属：跨项目协作放本文档；AOT 放 `aot-programming-guidelines.md`；Gallery 页面放 `gallery-showcase-design-pattern.md`；发布放 release workflow 文档。
- 当一个规则只适用于某个子系统，不要为了可见性塞进 `AGENTS.md`，应在该子系统文档中写清楚并从相关入口引用。

## 参考项目源码

当用户说“参考某个项目源码”或表达同等意图时，必须先执行 [参考项目源码查找规范](reference-project-source-guidelines.md) 中规定的本地优先查找流程。不得跳过项目目录下的 `.referenceprojects`、逐级向上的 `ReferenceProjects`，直接去 GitHub；只有所有本地位置都未命中时，GitHub 才是允许的兜底来源。

## 新增功能与 Bug 修复的测试生命周期

所有新增功能和 Bug 修复必须遵守[测试价值与生命周期规范](../development/test-value-and-lifecycle.md)。新增测试默认
属于临时验证，必须优先放在系统临时目录或所属测试项目的 `Temporary/<任务标识>/` 目录，并在功能或修复完成后
删除。最终评审、合并、发布或交付分支不得残留临时测试及其专用 helper、fixture 和资源。

全局测试集合默认净新增为 0。只有现有测试、编译器、生成器诊断、ApiCompat、真实打包和专项验证均无法覆盖，
并且测试同时满足契约明确、独立检错、长期收益和成本合理四项条件时，才允许永久保留。保留时必须在交付记录中
写明不可替代性、全局运行必要性、运行成本和重复测试处置。未完成临时测试清理或无理由扩大全局测试集合，
不算完成新增功能或 Bug 修复。

## Bug 修复

修复 bug 时必须优先寻找从根源杜绝问题的方案，而不是只在触发点打补丁。

新建复现测试按上一节的临时目录规则执行。未完成修复前后验证、测试去留判定、清理、最终模块验证和交付记录，
不算完成 Bug 修复，不得宣称“已修复”。TDD 要求有效验证，不要求永久保存复现代码。

编写 BUG 类型 Issue 时，按 [BUG Issue 规范](bug-issue-guidelines.md) 收集上下文并描述期望行为；不要在 Issue 中混入实现方案、验收清单或测试计划。

执行顺序：

1. 复现或从错误栈、日志、截图、用户步骤中还原触发路径。
2. 追踪坏状态、异常或错误行为的来源，直到能解释为什么会发生。
3. 搜索同类实现和同类触发入口，判断是否存在同源 bug。
4. 优先复用已有测试；必要时编写临时测试，确认修复前因目标问题失败。确实无法自动化时，说明原因并记录可复查的手动步骤与前后证据。
5. 在源头修复共享逻辑、状态模型、生成器、生命周期或契约，而不是只拦截当前样例。
6. 在原始触发条件下验证修复，按测试价值规范决定删除、合并或保留，清理临时测试及专用辅助产物。
7. 按“回归测试范围”验证清理后的最终工作树，提供测试处置记录；满足测试价值规范的全部完成门禁后才能结案。

不要把这些行为当作根因修复：

- 捕获异常后吞掉。
- 对当前控件或当前 Demo 写特殊判断，但共享路径仍然会失败。
- 加延时、重试或强制刷新来掩盖状态同步问题。
- 用 `UnconditionalSuppressMessage` 或条件编译隐藏 AOT/trim warning，但没有改变运行时动态行为。

### UI 输入与滚动 Bug

调查 hover、pointer、hit testing、滚轮、滚动、裁剪或覆盖层问题时，必须遵循以下纪律：

- 把用户指出的鼠标位置、截图和视频作为一手证据。只有局部区域响应时，优先调查几何范围、裁剪、覆盖层和命中边界，不要先假设是事件处理逻辑错误。
- 修改前先锁定完整 UX 契约，包括控件尺寸、可见滚动条、嵌套滚动、滚动链、布局和交互。未经用户明确允许，不得通过降低或删除这些行为来规避问题。
- 必须保留原始触发条件复现。不得删除高度限制、溢出、嵌套滚动、标题、内容或其他触发因素，只为了让症状消失。
- 修改事件代码前，检查完整输入几何链：visual/logical ancestors、Bounds、transform、viewport、extent、offset、clip chain、ZIndex、overlay、`IsHitTestVisible`，以及指针位置实际命中的 Visual。
- 使用单变量 A/B 证明因果关系。裁剪、布局、标题、高度约束和事件路由一次只改变一个因素。
- 临时验证与常驻回归测试都必须保留原始复现条件，并且能在修复前失败。UI 测试同时断言视觉与交互契约，例如滚动条可见性、viewport 范围、行左/中/右位置的 hover、内层滚动，以及到达边界后的外层滚动链；测试是否长期保留按测试价值规范判定。
- 测试通过不等于产品体验正确。必须对照用户要求检查最终渲染结果；原有正确设计没有保留时，不得声称问题已经修复。
- 优先提交最小根因修复。通过删除触发条件或牺牲预期体验让 bug 消失的方案只能标记为 workaround，未经用户明确同意不得作为正式修复。

## 临时复现 Demo

用户要求“跑起来看看”、复现 Issue、做最小可视化 Demo 或修复后复核 Demo 时，默认把 Demo 当作临时取证工具，
而不是仓库产物。

- 默认在 `/private/tmp`、`/tmp` 或当前 sandbox 允许写入的临时目录创建 Demo；除非用户明确要求保存示例项目，
  不得把复现 Demo 放进仓库源码、测试目录、Gallery 或文档示例中。
- 临时目录命名应包含项目或 Issue 线索，例如 `atomui-issue449-live-*`，便于识别和清理。
- Demo 必须引用当前工作区源码或当前待验证产物，并尽量复用仓库中的目标框架、Avalonia 版本、AtomUI 注册方式和
  启动路径；不要用不同依赖组合制造新的变量。
- Demo 必须保留原始触发条件，例如尺寸约束、滚动、冻结列、标题、Header 可见性、GridLine 设置、数据量和溢出
  条件。不得通过移除触发条件让症状消失后宣称修复。
- 如果 Demo 只用于复现或人工确认，完成原始场景验证及必要的人工复核后，必须停止相关进程、关闭可控会话，并在
  宣称修复完成前删除临时 Demo 目录。删除前如需保留证据，只保留必要日志、截图路径或简短结果说明，不保留可运行 Demo 工程。
- 临时 Demo 不能替代可自动化验证。优先通过已有或临时测试完成修复前后验证，验证代码的去留按测试价值规范判定；
  确实无法自动化时，才记录手动 Demo 验证步骤、前后证据和无法自动化的原因。
- 最终汇报要说明临时 Demo 是否已删除、仓库中保留了哪些正式修复和测试，以及是否仍有长运行进程需要用户处理。

## Public API 变更约束

修复 bug 时，默认不得引入新的 Public API，除非已经得到用户明确允许。

Public API 包括但不限于 `public` / `protected` 类型、成员、构造函数、Avalonia 属性、事件、公开接口、可被 XAML 引用的公开控件类型、主题资源 key、`ControlTheme` key、`TemplatePart`、伪类等对外契约。

修复应优先复用现有公开契约、`private` / `internal` 实现、模板内部结构或已有共享基础设施。不得为了当前 bug 修复方便，新增空的 `public` 类型、包装类、属性、事件或临时公开扩展点。

如果判断根因修复确实需要新增或调整 Public API，必须先停止实现，并向用户说明：

1. 为什么不改 Public API 无法从源头修复。
2. 具体会新增或变更哪些 API。
3. 对兼容性、文档、测试和示例的影响。
4. 是否存在不改 Public API 的替代方案及代价。

如果不确定某个类型、成员或 XAML 契约是否属于 Public API，应先询问用户，不要自行决定。

## AOT 与动态行为

新增功能和修复 bug 默认都要做一次 AOT 兼容判断。具体规则以 [AOT 编程规范](../development/aot-programming-guidelines.md) 为准，不在本文档重复展开。

重点约束：

- 同一需求有 AOT 友好实现和运行时反射/动态发现实现时，选择 AOT 友好实现。
- 能用 source generator、显式注册或 descriptor 的路径，不使用反射扫描。
- 发布路径、source generator 运行时注册、动态数据 path 等风险点按 AOT 文档验证。

## Gallery

Gallery 页面结构、懒加载、示例组织和旧 API/Token sidecar 禁用规则以 [Gallery ShowCase Design Pattern](../../gallery/authoring/gallery-showcase-design-pattern.md) 为准。

Gallery 改动时注意：

- 不把已移除的 API/Token sidecar 重新手写成普通布局。
- 不在页面结构调整中顺手改 Demo 行为。
- 延迟创建只能改变创建时机，不能改变示例控件树语义。
- 涉及 NativeAOT 发布或发布脚本时，阅读 [Gallery NativeAOT Release Workflow](../workflows/gallery-aot-release-workflow.md)。

## 资源与生命周期

涉及 `DynamicResource`、非 Visual `AvaloniaObject`、template part、event handler、binding、subscription、cache 时，必须能说明释放或失效位置。

典型检查：

- `OnApplyTemplate` 重新应用时释放旧 part 绑定。
- detach、owner 变更、container recycle 时解绑事件和资源宿主。
- 非 Visual 对象使用动态资源时，需要明确资源宿主生命周期。
- owner-managed 非 Visual `AvaloniaObject` 需要 scoped resource host 时，默认遵循 [Scoped Resource Host 开发规范](../development/scoped-resource-host.md)，不要复制手写 `IResourceHost` / `IThemeVariantHost` 样板代码。
- 修复内存保留问题时，按同类对象成组审计，不只修 dump 中看到的第一个类型。

参考：[Avalonia DynamicResource 内存泄露案例](../case-studies/avalonia-dynamic-resource-memory-leak-case-study.md)。

## 测试与验证

测试准入、临时目录优先、默认删除、存量清理，以及新增功能和 Bug 修复交付记录统一遵循
[测试价值与生命周期规范](../development/test-value-and-lifecycle.md)。一般 skill 中“补测试”的要求不自动赋予测试
永久保留资格；模块测试通过也不能代替测试处置和清理验收。最终分支存在 `Temporary/<任务标识>/` 测试，或全局
测试净新增却没有完整保留理由时，不得宣称工作完成。

日常验证必须先使用 [按改动影响选择验证](../workflows/affected-verification.md) 的统一入口：

- `python3 scripts/verification/test.py plan`：检查改动、功能域、消费路径与专项义务。
- Bug 修复开发过程中必须使用 `run --scope iterate --path <缺陷模块路径>`；`--path` 可重复提供生产文件和对应测试文件。
- 本地 `change` 只允许显式提供 `--base <目标分支>` 的分支/CI 比较，不得作为 Bug 修复过程中的扩大验证步骤。
- `--include-test <测试源文件模式>` 只追加检查；不得用手选子集代替 change 计划后声称完成交付验证。
- 不因修改一个控件而默认运行整个 Desktop 测试项目。需要扩大范围时由共享规则或明确影响证据说明原因。
- 规则 gap、零测试、skip、陈旧产物或 pending 专项义务都不能报告为完整通过；按报告处理缺口和真实发布验证。
- 无新增改动且输入、配置、产物匹配的成功 receipt 可以复用；不得为了收尾重复已通过的同一份验证。
- 直接 `dotnet test --filter ...` 用于稳定复现和红绿迭代；agent 的最终全量入口只能使用 `scripts/run-full-regression.sh`。
- 每次收尾仍运行 `git diff --check`，保留原始 UX、AOT、资源生命周期验证要求。

测试运行产生的本地临时结果必须由仓库构建入口自动清理。不得在 `tests/**/TestResults`、`.artifacts/TestResults`
或工作树副本中长期保留 VSTest 结果、hang dump、crash dump、coverage 临时文件等占用存储的测试产物；需要诊断 dump
时必须将采集位置移出仓库，并在排障结束后主动清理。

### 回归测试范围（硬约束）

- Bug 修复必须先以精确失败用例复现；修复迭代期间只运行缺陷所属模块和该模块显式映射的测试，不得运行 affected-wide 或全量回归。
- focused runner 必须显式传入一个或多个 `--path`。禁止直接对包含多个无关改动的整个工作树运行 iterate，并禁止用 `--include-test` 冒充模块路径范围。
- Bug 修复与普通代码改动的执行门禁都是“所修改模块的测试全绿”，不再自行追加全量回归或本地 change 范围。
  Bug 修复还必须完成测试价值规范中的测试处置、清理、适用专项和交付记录；只通过模块测试不算完成修复。
- `scripts/run-full-regression.sh` 只在用户明确要求或发版（执行 `version-release` skill）时运行。
- 确需运行的全量回归遇到失败时必须让脚本继续收集所有项目结果；随后逐项做 focused 修复。全部局部转绿后只再运行一次最终全量确认，禁止每修一个失败就重跑全量。
- 验证结果必须明确记录实际执行的测试范围、命令和结果；未执行的全量回归测试不得表述为已完成。

不要在没有最新验证输出时声称“已修复”“测试通过”或“发布可用”。

## 文档更新

当一次改动形成可复用规则时，更新对应文档：

- 跨项目 AI 工作约束：本文档。
- AOT、trim、source generator：`aot-programming-guidelines.md`。
- Gallery 页面结构：`gallery-showcase-design-pattern.md`。
- Gallery AOT 发布：`gallery-aot-release-workflow.md`。
- 版本发布准备与破坏性变更判定：`../workflows/release-preparation.md`。
- 控件研发标准：`control-development-guidelines.md`。
- 模块架构：`docs/modules/**/overview.md`。
- 控件用户文档：`docs/controls/**`。

文档应记录“以后怎么做”和“为什么”，不要写成一次性修复记录。

长期架构文档和实现维护文档必须以 AtomUI 自身的公共契约、不变量和验证条件为准。不得把外部项目的
具体版本、tag、commit、本地参考仓库路径或源码行号写成 AtomUI 架构的事实来源。确需保留上游差异或
调查证据时，应放入带日期的迁移记录、发布记录或问题分析；长期文档只保留验证后的结论和重新验证条件。

## 代码改动边界

- 遵循当前模块已有风格、命名和文件组织。
- 不做与用户请求无关的格式化、重排、重命名或清理。
- 删除只因本次改动变得无用的代码；既有无关遗留先报告，不直接处理。
- 抽象只在能消除实际重复、降低共享复杂度或匹配既有模式时引入。
- 生成文件不要手改；改 generator 或输入源，并验证输出。

### 命名约定

C# 命名约定的唯一事实来源是仓库根目录的 [.editorconfig](../../../.editorconfig)（`dotnet_naming_rule` 机器规则，IDE 与
`dotnet format style` 均可执行）。核心约定：

- 局部变量、模式匹配命名变量（`is Type name`）、`out var` 命名使用 camelCase。
- 方法参数使用 camelCase。
- 私有实例字段使用 `_camelCase`；私有静态可变字段使用 `s_camelCase`；私有 `static readonly` 与 `const` 字段使用 PascalCase。
- 公共/受保护/内部成员与类型使用 PascalCase；接口以 `I` 为前缀。

提交前可用 `dotnet format style --verify-no-changes` 检查命名违规（默认报告 warning 级以上，`--severity info`
可查看全部）。存量违规随触及文件逐步清理，不做一次性全仓重命名。

## 控件研发标准

控件 C# 实现、AXAML 主题、API 和主题契约变更必须遵循 [AtomUI 控件研发标准规范](../development/control-development-guidelines.md)。优化代码和修复 bug 时，如果涉及控件既有 API、主题契约或可观察行为变化，必须先获得用户明确授权。

## 语义部件复用

为控件新增或改造 Semantic Part 前，必须先读 [Semantic Part 系统设计](../../architecture/systems/theming/semantic-parts.md)
的相关章节，并复用既有底层机制，禁止重新发明等价轮子：

- Gallery 示例、文档示例与测试的样式定制必须用生成的专用 Semantic Part Style 类在 AXAML 声明式应用（见该文档
  5.4）；禁止在 code-behind 获取目标节点后直接设置属性。生成 Style 未命中目标节点属于 route/拓扑契约缺陷，先修
  `SelectorRoute` 并补命中回归，不写“无法命中”限制、不做代码回退。
- 目标在内嵌控件自己的模板内（宿主把功能区整体委托给内嵌控件）→ `CrossNestedOwners=true` + `>>` 或二次
  `/template/` 路由（见该文档 3.3.1），生成器自动切换为跨主题资产校验。
- 目标在模板 Popup 独立可视根内 → `CrossVisualRoot=true` + `popup.root` / `popup.list` / `popup.listItem` 三级键
  （见该文档 9.1）。
- 目标在独立宿主弹层内（Flyout / FlyoutHost 代码创建的 presenter，经 Popup `PlacementTarget` 挂在 owner 逻辑树）→
  `CrossVisualRoot=true` + `RuntimeCreated=true` + 以 `>>` 开头的 `SelectorRoute`（见该文档 9.2；先例 InfoFlyout 的
  `popup.root` 等四个部件）。
- 运行时创建的列表容器 → 容器创建时注入 semantic class（见该文档 8.3；先例 `ListBox`、`CandidateList`），
  marker 不得放在 item 的 ControlTheme 模板内部。
- Gallery 语义预览钉住弹层 → `IsDropDownOpen` + `IsPopupPinnedOpen`；light-dismiss 遮罩抑制由产品控件在弹层打开前
  完成（见 [Semantic Part Gallery Preview](../../gallery/authoring/semantic-part-preview.md) 第 10 节），预览基础设施
  不干预 Popup 行为。
- Popup 首次物化 → 分别验证请求、业务 open、物理 `Popup.IsOpen` 和视觉 actor 状态，并覆盖 `Opened` 与 actor-ready
  的两种顺序；不得用永久关闭 motion 掩盖时序问题。案例与全局审计见
  [Semantic Part Popup 首次打开生命周期竞态案例](../case-studies/semantic-part-popup-first-open-lifecycle-case-study.md)。

## Changelog 与发布

Changelog 维护规则见 [changelog-guidelines.md](changelog-guidelines.md)。普通修复、功能和内部重构不默认修改 `CHANGELOG.md`，只有用户明确要求准备 changelog、release、版本发布，或变更包含必须提前暴露的兼容性信息时才整理。

发布相关改动必须检查对应脚本、workflow、项目属性和产物校验逻辑，不能只看普通 Debug build。
