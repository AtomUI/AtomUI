# 执行记录 — plan: docs/superpowers/plans/2026-11-02-combobox-empty-state.md

## 范围与决定

- 用户已批准按先前写入 ComboBox 设计文档的完整空态模型开始重构修复。
- 当前分支与工作目录执行；不创建分支/worktree，不提交。Gallery 既有 `x:Name` 改动保留。
- 窄范围正确性修复，主文件 1662 行，不拆分、不机械重排，不改选择与过滤匹配语义。
- API：四个兼容新增 StyledProperty；行为：空集合显示空态；语义外框与路由保持。Token 不新增。
- 现有 pinned relay 的目标为 internal Popup 成员，AXAML 编译不可直接访问，保持代码绑定及其模板替换释放路径；补充原因注释。
- Pre-flight：Task 1 的属性和主题由 Task 2 Gallery 消费，名称与默认值均来自同一设计；无接口冲突。
- Ruling：用户已指示开始执行，复用现有设计批准，不重复要求计划确认；技能要求的隔离与提交由项目强约束覆盖。

## 验收契约

| AC | 来源与条件 | 操作与独立预期 |
| --- | --- | --- |
| 01 | 原 issue：ItemsSource=[]，实际弹层打开 | 读真实空态节点与布局；空态有效可见且有尺寸，列表隐藏，弹层宽度和最大高度保留。 |
| 02 | 状态风险：同一实例空/非空、过滤往返 | 增删/Reset/替换集合、输入与清空过滤；数据与空态双向恢复，选择契约保持。 |
| 03 | API 设计 | 开关、自定义内容、仅模板、清空定制、运行时 Padding；默认与自定义互斥，开关不恢复已过滤行。 |
| 04 | 主题兼容 | 空态内外框/marker保持；专用 Style 声明式命中；正常候选行与受限弹层保留。 |
| 05 | 生命周期风险 | 模板重应用及重开、卸载重挂后按当前数据恢复，无旧容器投影。 |

## 执行与证据

- Ruling：当前 Gallery 规范已经移除 API/Token 表，修正计划中的旧机制表述；本次只同步示例与语义说明，公开 API 由控件文档和源码维护。
- 修复前命令：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --filter FullyQualifiedName~Issue526EmptyStateTests --results-directory <系统临时目录>/results --logger 'console;verbosity=normal'`。
- RED：实际发现并执行 8 例，8 例失败，测试执行 7.317 秒；三种空数据入口、集合清空、清空过滤、重开场景失败于真实空态可见性，开关与内容测试失败于缺失的公开属性。编译成功，非编译错误造成的失败。
- 构建存在两个既有 `NavMenuInlineCollapsedTests` 的 xUnit1051 警告，未修改该模块。
- 中间验证：21 例中 20 例通过；自定义内容用例取错 TextBlock 类型。实际树已包含 `Avalonia.Controls.TextBlock`、文本 `Custom empty` 且有效可见；原断言解析到外层 AtomUI 命名空间的同名类型。改为独立别名查询，产品实现不因此改动。
- Ruling：临时测试将 Template 先置空会按现有 Popup 生命周期关闭弹层；模板恢复后显式通过公开 API 重开再断言，不把未请求重开造成的节点缺失归为产品缺陷。
- Gallery RED：精确执行 `ComboBox_ShowCase_Filter_And_Empty_Content_Examples_Render`，1 例按预期失败于缺少专用 `ComboBoxPopupEmptyStyle` 的声明；随后添加四种空态示例与四语言资源。
- 成员审查：8 个 C# 文件、3 个控件。发现主文件中两个既有位置问题，单独调整未初始化的 Form 事件字段及静态 marker helper 位置，无初始化/行为变化；复查 0 项。
- Skia 验收 AC-01/04：相同当前工作区产物、640/480 Headless 逻辑宿主与独立 400×420 Skia 截图宿主。Skia RenderScaling=1、默认 motion=true、实际 Popup 均打开。修复前空弹层 320×8、空态 0×0 且隐藏；修复后空弹层 320×100、空态 312×92 且有效可见，最大高度仍为 328。已查看前后截图：修复前只有空白条，修复后为 Simple 插图和 No data 文案，边框与阴影完整。
- Skia 正常候选对照：三行列表前后均为 320×104；两侧各自 A/A 像素一致，1 DIP 位移检错有效，恢复后零差异；前后整张 400×420 RGBA 像素完全相同。仅证明该 Skia 场景；Windows 11 与 macOS 原生窗口未运行，不将其计为已验收。
- Skia 重放命令：`dotnet run --project /tmp/atomui-issue526-H9OSGw/visual/Visual.csproj -- <证据运行目录>/artifacts before` 与同一驱动的 `after` 阶段；驱动直接引用本工作区构建产物，未创建源码副本或隔离检出。程序等待真实帧稳定且覆盖 700ms 后采样，媒体通过 evidence.py 登记。
- GREEN：临时 8 例加既有语义 13 例，共 21/21，通过时间 4.5747 秒；命令为同一 Desktop 测试项目，filter=`FullyQualifiedName~Issue526EmptyStateTests|FullyQualifiedName~ComboBoxSemanticPartTests`。
- 测试处置：扩展既有 ComboBox 语义用例和 Gallery 页面用例，不新增永久测试。临时复现用例在生命周期补验通过后已删除；最终模块验证在删除后执行。
- 生命周期补验：同一 `Reopen_And_Retemplate_Keep_Empty_Presentation` 用例加入卸载/重挂后的显式重开，精确执行 1/1 通过，4.1252 秒。
- Gallery 页面操作核对：新增卡片位于页面下方，先通过公开 `BringIntoView` 滚入视口，再打开实际 Popup；保留页面原滚动布局。专用 Style 已命中外框，XAML brush 为不可变实现，测试按 `ISolidColorBrush` 的颜色契约验证。
- 独立审查：一名只读 reviewer 检查本任务 diff 与当前源码，未发现可执行缺陷或阻塞项；未运行构建和测试，不将审查扩大为平台/发布验证。
- AOT 判断：四个静态属性注册和编译主题绑定均沿用既有路径，无新类型发现、反射、动态注册或发布配置变更；本次不执行 NativeAOT publish。
- 清理后验证首轮构建暴露 Gallery 测试的 nullable 接口断言，已修正；随后控件 9 类共 102/102 通过，Gallery 用例暴露仅模板示例未物化的实际问题，未将该轮计为成功。
- 仅模板取证：实际 `ContentTemplate` 已绑定且 presenter 可见，但 XAML `DataTemplate.Match(null)=false`，子级为空；原因是示例的 `x:DataType="x:Object"` 被作为模板类型约束。对照本地 `/Users/chinboy/Projects/ReferenceProjects/Avalonia`（12.1.3）的 DataTemplate/ContentPresenter 源码，并以当前 12.1.4 实例观测确证。
- Ruling：遵循既有 `IDataTemplate.Match` 选择契约，不绕过匹配强行 Build，也不引入模板适配类；仅模板示例改为可匹配 null 的无类型 DataTemplate，带类型模板需提供匹配内容。公共文档和 XML 注释补充这一输入契约，原失败页面用例保留相同操作再验证。

- Gallery 仅模板修正后，保持相同原页面操作，精确用例 1/1 通过（3.6072 秒），四种示例均实际打开，专用 Style 与模板内容断言通过。
- 清理：临时测试目录、独立视觉驱动及其编译产物均已删除；evidence.py 清理 5 个文件，审计 findings=[]，未将截图/二进制写入仓库。两个既有测试文件的永久测试声明分别保持 13→13、5→5。

## 最终验证

- 清理后的工作树执行 `python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls/ComboBox --path controlgallery/AtomUIGallery/ShowCases/Navigation/ComboBox`，结果 `iteration-passed`，总耗时 43.873 秒。
- Desktop：9 类、102/102，通过 6.920 秒；Gallery：2 类、8/8，通过 6.252 秒。共 110/110，未运行 change、全量回归、原生平台实机或 NativeAOT 发布。无剩余专项义务。
- 文档 48 个相对链接与锚点检查通过；四份 XLF 各 60 个唯一 unit，Key 集合一致；成员检查 8 文件/3 控件，0 问题；`git diff --check` 通过。
- 只扩展现有测试，不净增永久用例。临时测试、视觉驱动、媒体与专用产物已清理；本段保存后清理会话备份、原始日志与测试结果。未创建分支/worktree，未提交、推送或发布 Issue 评论。

## 2026-11-02 临时 Demo 原生脚本化视觉验收

- 用户要求：在可操作临时 Demo 上执行脚本化视觉验收。使用同一个参数化临时驱动，经公开 API、真实 Demo Click 路由及 TextInput 路由操作，不使用桌面坐标点击或 Computer Use。
- 来源：原始空 ItemsSource 复现与已确认的六组 Demo 场景；候选行尺寸、原有最大高度、滚动宿主、默认 motion 与专用语义 Style 保留。
- 身份：`/private/tmp/atomui-issue526-demo-OUX3Ok/bin/Debug/net10.0/AtomUIIssue526Demo`，原生运行 PID 73735，NSWindow 104320；ClientSize=1120×860 DIP、RenderScaling=2，截图=2240×1720。Demo 引用当前仓库源码项目，运行时控件 DLL SHA256 与工作区产物一致。
- 重放入口（临时验收代码，完成后移除）：`ATOMUI_ISSUE526_VERIFY_OUTPUT=<证据目录>/artifacts /private/tmp/atomui-issue526-demo-OUX3Ok/bin/Debug/net10.0/AtomUIIssue526Demo`。应用自动完成打开、数据切换、输入、断言、截图和退出，进程返回 0；验收构建 0 警告/0 错误。
- 截图通过 own NSWindow.windowNumber 调用 `/usr/sbin/screencapture -x -o -l <window-id> <registered-path>`，最终全部使用 macOS 原生窗口截图，未启用 RTB fallback。每次动作后覆盖 800ms 并完成布局，再采样；比较使用完整、同尺寸 RGBA，不裁剪或对齐。

| AC | 操作与预期 | 实测与视觉复核 |
| --- | --- | --- |
| Native-01 | 空 ItemsSource 打开弹层，应有默认空态 | 实际 Popup 打开，frame=300×103，空态=292×95，最大高度=328；插图和“暂无数据”完整，列表隐藏。PASS。 |
| Native-02 | 同实例列表 A/A、1 DIP 检错、数据非空→空→非空 | A/A=0 像素差异；1 DIP 扰动检出 24510 像素，恢复后=0；数据 A/B/A=0。三行列表 frame=300×104。PASS。 |
| Native-03 | 固定已打开弹层，经 Demo 按钮加载、Clear、替换集合 | 0→3→0，加载恢复 Alpha/Beta/Gamma，清空和替换恢复默认空态；按钮 Click 路由实际到达。PASS。 |
| Native-04 | 通过编辑框 TextInput 输入 zzz，然后清空 | 路由计数=1，ComboBox.Text=zzz，无匹配空态可见；清空后 3 行均有效可见。实际画面与断言一致。PASS。 |
| Native-05 | 自定义内容、仅模板与专用 Style | 自定义文案和默认 Empty 互斥；null-matching XAML 模板两行内容完整，专用 Style 背景 #F6FFED、Padding=24 命中真实外框。PASS。 |
| Native-06 | 运行时关闭/开启空态、内边距 0/32、空集合清空输入 | 开关双向绑定生效；关闭时保留普通空列表框，开启恢复空态；空态高度 63→127 DIP，差 64，与两侧 padding 一致，弹层正常翻转且内容不裁剪。清空输入后仍为空态。PASS。 |

- 首轮完整操作轨迹保留后分析：A/B/A 出现 114956 像素差异，原图明确显示后一帧标题栏失活且弹层关闭，采样条件已不同；不能据此判绘制回归。第二轮统一在每次采样前公开调用 Window.Activate，记录 Activated/Deactivated 与每张截图前后的 IsActive/IsDropDownOpen，两侧轨迹一致且所有采样 IsActive=True，以上 A/A、扰动恢复和 A/B/A 均为零差异。未通过强开弹层、删除高度限制或更换数据绕过原触发条件。
- 视觉：最终 17 张截图中的各关键状态已实际打开复核；默认/清空、三行列表、无匹配/恢复、自定义内容/模板、开关和 0/32 内边距均与预期一致，无内容重叠或裁剪异常。首轮失败截图与最终截图统一在复核后清理，不进入仓库。
- 结论：本次 macOS 原生宿主、2× 缩放、浅色中文 Demo 六组场景 PASS；文本为框架 TextInput 路由验证，不等同于硬件键盘/IME 验证。Windows 原生和 NativeAOT 发布仍未覆盖。之前记录的 110 项模块结果继续适用于未变更的产品源码，本轮不重复运行。
- 清理：原生验收进程已自动退出；临时驱动与验收入口移除，恢复普通 Demo 并重新构建；媒体、失败轨迹原始日志和派生文件通过 evidence.py 清理。保留用户请求的可操作 Demo，未修改产品代码、未提交。

- 清理确认：evidence.py 已删除 29 个文件，审计 findings=[]；验收源文件、环境变量入口和原始日志均移除。恢复后的普通 Demo 构建通过，0 警告/0 错误（4.90 秒），已重新启动供用户操作。

## 最终交付验收复核

- 复用已通过的 110 项模块验证与 macOS 原生六组视觉验收：按成功 receipt 的 manifest 重新计算 369 个产物，全部字节指纹一致；24 个相关源码、测试和依赖配置输入在成功运行后均无更新，无新增测试重跑依据。
- 当前控件 DLL SHA256 仍为 `6bc417d66fe0723f816738b74c6e70896a92d455235ec27c3b1c0d670bb5fae6`，与原生取证产物一致；`git diff --check` 通过，临时测试、验收入口与证据目录均已移除。
- 在已验证范围内交付验收通过，无阻塞项。普通 Demo 目录按用户要求保留，复核时进程已退出；Windows 原生与 NativeAOT 发布仍未覆盖，未提交代码。
