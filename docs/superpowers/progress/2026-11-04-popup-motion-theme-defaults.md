# Popup Motion 默认配置简化执行记录

## 起点与边界

2026-11-04，feature/motion，HEAD 4513cfd2，已有 131 个暂存路径。用户同意简化并追加零资源泄漏要求。用户 ComboBox 格式改动和 Temporary/Issue529Evaluation 不属于本轮。仅在现有目录执行，不 commit。

## 当前任务

- [x] 默认值/独立实例 RED 证据
- [x] 默认创建和几何准备改造，所有接线迁移
- [x] 功能、回收、性能和原生专项
- [x] 正式文档同步，临时产物清理
- [x] 清理后模块验证、一次整体审阅、暂存

## 设计与框架证据

默认值不再经 Popup→owner 回传。主题两个 Setter 直接引用内部 ITemplate 默认创建器；创建器只持有静态函数，按样式实例产生可独立修改的定义。六个私有内置子类通过 IPopupMotion 的可执行准备方法接受值快照，产生当前请求所需的普通 Slide/Zoom 定义，不捕获 owner/target，不新增观察、时钟、CTS 或全局缓存。普通用户 Motion 直接返回原对象。

本机已核对 Avalonia 12.1.4 源码：`Styling/Setter.cs` 的 ITemplate 分支为每个样式实例构造 `PropertySetterTemplateInstance`，其 GetValue 延迟创建并缓存该实例值；普通 Setter 值直接共享。`StyledPropertyMetadata<T>` 无 per-instance factory，普通 XAML Template/FuncTemplate 仅支持 Control，故使用最小的内部 Motion 创建模板。源路径位于 `/Users/chinboy/Projects/ReferenceProjects/Avalonia/src/`，这属于本次依赖核验，不提升为永久外部实现合同。

Ruling：Menu 层级和横向 NavMenu 差异放入请求前的语义值快照，禁止使用高优先级条件默认 Setter。实际临时测试已证明条件 Setter 会压过应用普通 Style；改造后通过。同一属性只保留正常主题默认，显式 null/Style/Binding/ClearValue 均遵守既有优先级。

## RED→GREEN 与功能/内存

命令均为 `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~MotionTheme`，诊断时精确到单个测试类。

- 原暂存实现：无反向默认属性、定义跨几何保持身份的两项目标测试均失败；简化后两项通过。
- 接线初稿：20 项中仅菜单应用 Style 覆盖失败；语义改为值快照后 20/20 通过。
- 最终专项：33/33 通过。13 类 owner 验证独立定义、实例修改不串扰、null/ClearValue 和绑定覆盖/释放；10 个方向/原点组合及菜单 RTL/来源/横向 NavMenu；不修改定义配置；真实 Headless 鼠标 hover 入/离场中间值及物理关闭。
- 5 个回收场景通过：窗口继续存活且强持有定义时，DatePicker/Popup 在正常结束、离场中卸载、替换定义并重建模板、入场中卸载四种退役顺序均回收；独立 Flyout/Popup 在定义仍被持有时回收。NoInlining 退役函数、渲染队列排空、5 次完整 GC；未清 DataContext、用户内容或全局资源。
- Hover 驱动最初没有命中：原空 Border 不参与命中，窗口内容还有标题栏坐标偏移。保留实际窗口尺寸后，用真实 TranslatePoint 与命中值定位，仅把测试内容改为可见背景。后续把逻辑 IsOpen 误作物理关闭判据，按真实合同改为 IsHostOpen/IsPlayingCloseMotion。产品输入代码没有更改。最终同实例入场/离场均取到中间透明度并最终拆宿主。

## 性能对比

仅比较本轮简化前的原暂存源码与最终源码，均在当前 checkout 构建；没有创建分支/worktree。运行基线期间完整备份当前 41 个源文件，finally 逐字节恢复并核对 Git index tree 未变。没有使用历史版本替换本轮基线。

同一临时 DatePicker 驱动、Headless 原生宿主替身、960×800 窗口、200ms MotionDuration，每方向固定 25 次10ms clock 脉冲，10 次预热后100次正式周期，前后各3个独立进程，共600个周期。每个周期均断言打开宿主、离场保留宿主、最终物理关闭。测试前设置 COMPlus_TieredCompilation=0；测量只包含操作/Pump/固定 clock，构建不计入样本。采集线程分配与 Stopwatch，结果为正式样本中位数。

| 指标 | 简化前 | 当前 | 计算 | 变化 | 结论 |
| --- | ---: | ---: | --- | ---: | --- |
| 默认回传/几何属性注册 | 15 | 0 | 15÷15 | −100% | 移除冗余属性 |
| 默认相关绑定声明 | 67 | 0 | 67÷67 | −100% | 移除回传、几何、默认视图绑定；不计必要公开属性转发 |
| 单次打开准备分配 | 4,277,416 B | 4,273,700 B | 3,716÷4,277,416 | −0.087% | 包含真实 Picker/host 准备，不当作页面速度收益 |
| 单次关闭准备分配 | 6,856 B | 6,944 B | 88÷6,856 | +1.284% | 新增一个88B独立请求定义，有限分配；不是驻留或每帧分配 |
| 打开25个时钟脉冲分配 | 4,120 B | 4,120 B | 0÷4,120 | 0% | 没有新增帧阶段分配 |
| 关闭25个脉冲及物理收尾分配 | 645,840 B | 645,784 B | 56÷645,840 | −0.009% | 无帧阶段分配增加 |
| 完整开关周期分配 | 4,934,200 B | 4,930,804 B | 3,396÷4,934,200 | −0.069% | 全周期中位数独立计算，不是各分项中位数之和 |

时间数据存在显著 A/A 波动：当前三个同版本进程的打开准备中位数46,372.9/13,740.4/15,996.35µs，关闭准备178.35/49.8/59.65µs；基线合并中位数分别15,985.6/51.25µs，当前合并16,199.35/62.25µs。差异被同版本波动覆盖，不将其当作速度提升或可靠速度退化证据。结构与固定脉冲分配已核对，Core 帧管线源码未改；不宣称 FPS、显示器 present 或 UI 打开时间提升。本轮不继续增加近似模型或重复采样。

## 原生、AOT 与审查

- Release GalleryPerformance 构建：0 warnings / 0 errors。
- macOS 原生 NavMenu Popup：`run_native_motion.py --binary .artifacts/bin/Release/net10.0/AtomUI.GalleryPerformance --scenario popup --processes 1 --warmup 1 --iterations 2 --sample-ms 450`，Schema3 Completed、exit0，4次正式动作分别23/24/23/24个真实中间值。该次仅为原生功能冒烟，不作为速度对比。Hover 的本轮输入证据为 Headless；不混作本轮 macOS 原生 hover 实测。
- `PublishToLocal.ps1 -publishRootPath /tmp/atomui-motion-theme-defaults/aot -runtime osx-arm64 -buildType Release -publishAot true`：发布及 NativeAOT output validation 均通过。既有 ReactiveUI IL2104/IL3053、DataGridBoundColumn IL2026/IL3050、Homebrew deployment linker warnings 不属于本轮新增。
- 精确启动 `/private/tmp/atomui-motion-theme-defaults/aot/packages/AtomUIGallery.Desktop`，ps 核对 PID 91928 的实际路径；5秒内持续运行、启动日志0B，随后结束该验证进程。仅为启动冒烟，不声称 AOT 全交互验收。
- 执行计划要求的一次独立只读审查 `/root/motion_defaults_final_review`：未发现有证据的 Critical/Important；确认 ITemplate 实例隔离、无新增反向持有、Style 覆盖对称释放及 Menu/NavMenu 准备边界。审查没有另跑测试。

## 清理与最终模块验收

不运行全量回归，不关闭或评论 GitHub issue。Windows 原始环境与 Linux 原生动效未新增实机验证。

### 清理后检查中的 DataGrid 输入测试

第一轮 iterate 已构建成功，Build.Tasks 1项通过，DataGrid 314/316通过，两个 FrameCornerRadius 输入测试失败。完整原序列为左侧右键打开菜单→Close→中部右键→Close→右侧右键；第二次命中得到 LightDismissOverlayLayer。ContextMenu 的 IsOpen 由物理 Closed 收敛，Close 只发起默认离场；原测试把 Close 当同步拆宿主且未推进动画。未移除三点、内容、限高、边框或任何输入条件，也未禁用产品/测试动画。

修改既有测试的同一循环：Close 后以2秒故障超时推进 Headless 渲染时钟/Dispatcher，等待真实 menu.IsOpen=false，再进行下一点。每5ms轮询状态，不用固定等待代替验收；保留原所有几何与三点鼠标输入断言并新增关闭收敛断言。原失败两例随整类6项精确验证全部通过（测试总耗时4.566s含宿主启动）。没有新增永久测试或 helper。此修正仅改测试等待契约，产品源码、Release/AOT产物和33项专项的输入不变，复用既有专项证据。

### 最终结果与并行工作隔离

自动 iterate 的后续批次受到并行 Issue529 工作影响：原43类 DataGrid 名单变为44类，新增临时类要求专用 ATOMUI_ISSUE529_EVIDENCE_DIR。该轮337项中9项失败，其中8项属于并行临时类，另1项为分组标记；该分组原测试独立执行通过。没有设置另一任务的取证目录、运行它的媒体取证流程或修改/删除其代码。

随后冻结原43类名单（未排除任何原永久测试），与 Desktop 并行的316项批次出现3项排序超时。实际观察到同一工作目录的其他任务正在变更 DataGrid 测试宿主并恢复它，且有构建/test命令重写共用 DLL；当时输入未冻结，未把这些结果记作成功，也未修改排序逻辑/断言。最终单独运行原完整名单，同时核对全部相关源码及 AtomUI DLL 的 SHA256和mtime：4,312个输入运行前后无变化，316/316通过。原三项排序和分组断言均保留在这次完整运行中。该次是有稳定输入证据的验收，不把早期并行批次错误改称产品修复。

最终对 iterate 选出的原有类使用精确 `dotnet test --filter`等价 runsettings 执行；只隔离另一任务新加入的 Temporary.Issue529 类，未改全局选测策略、Skip或任何本轮断言。自动runner的失败历史保留，不伪写成 runner 整体成功；以下为清理后直接命令结果：

| 项目/范围 | 通过 | 时间（含相应宿主） |
| --- | ---: | --- |
| Build.Tasks，ThemeAssetWrapperTaskTests | 1 | 1.859s |
| Desktop，原136类 | 1,355 | 47.652s |
| DataGrid，原43类，独立且输入冻结 | 316 | 53.05s命令耗时 |
| Rendering，ButtonFocusVisualRenderingTests | 1 | 3.032s |
| Generator，选中3类 | 89 | 10.425s |
| Gallery，选中8类 | 34 | 9.734s |
| Localization，OfficialPtBrLanguagePackTests | 1 | 5.376min，真实打包、布局、完整/部分消费者及损坏包检测 |
| 合计 | 1,797 | 相关模块，非全量回归 |

例：`dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore --no-build --settings <本轮原43类精确名单.runsettings>`。该名单来自 iterate 的原永久测试选择；全类名 OR 过滤，无方法/数据行删减。新增临时 MotionTheme 文件已删除，最终 Desktop 测试 DLL 中无 MotionThemeCost/Lifecycle/Defaults/Hover/GeometryTests 类型；没有净新增永久测试，既有 DataGrid frame 测试仅同步真实关闭等待合同。

38个本轮 Markdown 的相对链接检查无缺失；暂存差异格式检查通过；暂存源码中 DefaultMotions/DefaultMotionPreset/IsMotionAllowed/PopupMotionPair/MotionGeometryProperty 均已无引用。默认准备不新增包文件、资产布局或 build-task 条件；选中主题资产/生成器检查及真实语言包布局消费通过，NativeAOT发布/启动另已完成，不扩大为无关全包发布。

本轮80个修改路径已暂存，保留原已有暂存变更。ComboBox 的用户格式改动未一并暂存（`git diff -w --ignore-blank-lines`为空），Empty 文档/AbstractEmpty实现及另一任务日志均未触碰。HEAD仍为4513cfd2898c66d90e2d137f8ac8fc2d171f5598，feature/motion，未创建branch/worktree，未commit/push。

本轮临时源码、驱动、runsettings、41文件版本备份、原始采样、AOT产物、日志以及失败runner的专用临时目录在记录必要文字后删除。语言包消费测试自带finally清理它的临时包源/消费者；AOT验证进程已停止。仅保留本权威文字记录与既有工具正常生成的验证历史，不删除其他任务临时文件。

实际清理确认：atomui-motion-theme-defaults 删除151个文件。上述专用目录均已不存在。


## 审查补充：Rider 静态成员类型错误

用户截图中 CascaderTheme 的 x:Static 报 expected AbstractMotion? / actual PopupMotionTemplate。前轮有效验证覆盖了 Setter 的 ITemplate 实例化，漏掉 IDE 静态类型合同；不能用编译成功否定此诊断。

有限范围：PopupMotion.cs、Flyout.cs、MenuFlyout.cs、DataGridTreeFilterFlyout.cs 的默认创建表示；保留全部公开 API、各族 AXAML 内容与既有帧管线。移除 PopupMotionTemplate，十四个静态成员声明 AbstractMotion 并持有真实可播放定义；六个内部定义通过 ITemplate.Build 返回独立配置副本，C# Flyout 显式使用同一复制入口。没有新订阅、目标引用或缓存。

临时元数据断言运行前失败：`SelectOpen: expected AbstractMotion, actual PopupMotionTemplate`；同批原有实例独立性检查通过（1 failed / 1 passed）。修正后两项均通过。后续专项补配置五属性复制、真实 Cascader/DatePicker/ColorPicker 主题、非视觉 Flyout 与原型/定义仍被持有时的回收；本节记录最终结果，不新建重复权威记录。

Rider 离线 inspect.sh 受当前已运行实例限制（Only one instance of Rider can be run at a time），未关闭或重启用户 IDE。读取现有 Rider 的实际 Cascader diff：修正后两条 x:Static 不再有原红线，该文件对应2个错误从全局计数中消失（21→19）。其余文件的解决方案分析不在这一读取结果内，不把此记录当作全局IDE错误归零。没有抑制检查或降低属性类型。


### 本次类型修正的最终验收

- 当前 NuGet 12.1.4 的 avalonia.nuspec 声明 repository commit 为 `2d79ba55fae1b1e71c69f93a668080c9029e6a31`。参考 checkout 实际 tag 是12.1.3/HEAD `8eeda4f6f546165b3f72e63c9f42247abb306905`；本次用本地 git show 直接读取 NuGet commit 的 Setter.cs 与 PropertySetterTemplateInstance.cs，并比较 StyledPropertyMetadata、FuncTemplate，四文件内容相同。结论基于当前 NuGet 指定提交与真实运行检查，不以参考 checkout 的 tag 冒称版本一致。
- `PopupMotionStaticTypingTests` 初次：1失败/1通过，精确失败为截图相同的静态成员类型不兼容；修改后2/2通过。
- 增补全部配置复制与实例/回收后9/9通过（4.841s含宿主）：14个静态原型的声明类型与实际类型均符合 AbstractMotion；逐一创建独立副本，复制 Duration/Delay/Easing/Origin/FillMode，不更改原型；真实Cascader、DatePicker、ColorPicker的主题实例及null/ClearValue；普通Flyout/MenuFlyout默认复制；窗口存活并强持有原型/定义时，DatePicker/Popup和独立Flyout/Popup均可GC。没有清空DataContext、用户内容或全局资源。
- 两个临时测试源文件随即删除。清理后精确模块检查：Desktop 41/41（9.278s含宿主）、DataGridFilterDialogPopupTests 4/4；本次只检查该修正的相关范围，没有把上一轮1,797项结果冒作本次新源码的完整复验。
- Debug net10.0 的 Desktop/ColorPicker、DataGrid构建通过（均0警告0错误）；Release net8.0 对应库构建通过（24.19s/8.73s，0警告0错误）。调试目标和生产目标均实际核验。
- 本次新源码 osx-arm64 NativeAOT 发布及output validation通过；原有ReactiveUI与DataGridBoundColumn/系统库警告保持原记录，未新增静态原型相关警告。精确启动新AOT产物，核对PID43269路径，5秒持续运行后停止该验证进程；不将启动冒烟扩展为全交互验收。
- 独立只读审查 `/root/popup_static_type_review` 未发现Critical/Important：确认声明类型真实兼容，六个复制入口创建独立副本并保留五配置，非视觉路径不会二次模板复制，无新增owner、订阅或运行状态引用。
- 这次属于类型合同修正：去掉1个模板包装类型，没有改变逐帧管线、几何、动效选择、控件树或公开属性；不声称速度提升，不重复600周期性能采样。前轮已测播放算法与关闭时88B请求定义没有改动。
- Rider原位置红线已实际查看消失；后续只读状态显示全局错误计数由21继续降为1，未展开的剩余诊断不作归因或归零声明。没有关闭IDE、重启、修改检查配置或添加类型抑制。

本次仅补暂存4个源文件与相应正式文档/原执行计划记录，保留其他工作。临时测试、源备份、日志和AOT产物清理后记录最终结果；不commit。

本次清理实际完成：`/tmp/atomui-motion-static-type` 的70个专用文件已删除（含AOT产物、备份和原始日志）；两个临时测试文件/目录已不存在。最终只暂存上述9个既有路径的修正，不含其他任务及用户格式改动；未commit。


## 全组件默认值复审

用户要求复审全部其他控件的类型、泄漏与动画成本。起点为当前已暂存的真实AbstractMotion原型方案；工作分支和HEAD不变，保留其他任务改动。初步清单：15个声明入口（14个弹层配置owner加TreeView），15份AXAML主题30个默认Setter，7对/14个静态原型；C#非视觉装配复用同一原型复制路径。TreeView的两个私有共享定义均为AbstractMotion的真实Expand/Collapse实例；其null回退属于独立内容展开合同。

- [x] 全部默认引用及声明/实际类型
- [x] 逐族实例/覆盖/缓存矩阵
- [x] owner/定义回收与绑定释放
- [x] 无新增帧成本，必要模块验证
- [x] 清理、独立复审与暂存


### 全组件复审发现与修正

- 完整实例矩阵发现 PopupConfirm 的两个配置均为null；独立主题没有BasedOn FlyoutHost，也没有自己的默认Setter。并非类型不兼容，而是默认值缺失。精确原实例断言先失败，再在该独立主题添加与FlyoutHost相同的PopoverOpen/PopoverClose两个类型正确的Setter；矩阵随后通过。
- AXAML扫描驱动初稿未覆盖InfoPickerInput的Primitives命名空间，导致扫描器NullReference；修正为按实际程序集类型解析后扫描通过。它属于驱动错误，不作为产品缺陷。并行用户导入清理期间曾观察到Tooltip暂缺AbstractMotion导入的编译失败；当前已恢复有效导入，没有将并行编辑状态算作动效缺陷或覆盖用户其他导入/格式改动。
- 新增实际 PopupConfirm 展示/关闭、并强持有Flyout与默认定义的退役场景，暴露owner无法GC。定位到两条路径：PopupFlyoutBase._lastPlacementTarget强持有上次目标；以及Flyout缓存的presenter Border._borderVisual→旧合成祖先→OverlayPopupHost._popupPositionRequest.Target→旧owner。单变量探针只清前者仍失败；在此前提下再复用既有ReleaseDetachedCompositionCache释放已脱离Border缓存后，通过。原始PopupConfirm开关历史、强持有、窗口存活和5轮GC均保留。
- 正式修正在匹配旧owner的ClearMotionConfigurationOwner边界：已有旧物理宿主关闭完成后，且没有新活动宿主时，清空框架默认定位目标并释放已脱离的内容Border缓存，再恢复独立默认值。普通Hide不清上次目标；新增三种托管入口的普通关闭/IsOpen重开与退役后不再打开旧目标的合同检查。
- 私有方法访问集中在现有PopupFlyoutBaseReflectionExtensions固定兼容边界，使用精确DynamicDependency保留SetDefaultPlacementTarget；没有新增逐帧订阅、时钟、全局对象缓存，也不清除用户内容或DataContext。Border释放复用既有已脱离/当前视觉为空保护，不修改合成树。
- 所有临时诊断的环境分支、手动清理和对象图扫描已从回归驱动移除；正式产品修正后，原未干预的43项审计/交互/回收测试全部通过。另2项DataGrid实际菜单/树筛选Flyout的默认类型、独立副本与真实装配检查通过。
- 既有FlyoutResourceBindingLifetimeTests的“固定私有方法名存在”断言没有行为保护价值，本次用同一个Fact位置替换为真实共用Flyout强持有下的旧owner GC检查；全局测试数量不增加。该共享释放合同会泄漏整页、编译/生成器不能检测，需在生命周期检查中长期保护；使用既有Headless宿主、无外部工程/发布成本，不为单个控件新增永久测试集。


### 全组件复审最终清单与验收

| 默认族 | 实际核对范围 | 结果 |
| --- | --- | --- |
| Select | Select/Cascader/TreeSelect/ComboBox，DatePicker/RangeDatePicker/TimePicker/RangeTimePicker，AutoComplete/SearchEdit/TextArea | 静态及实际类型正确，逐实例复制 |
| Popover | ColorPicker/GradientColorPicker、FlyoutHost、PopupConfirm、独立Flyout/TreeViewFlyout | PopupConfirm补齐独立主题，其余无同类遗漏 |
| Dropdown | DropdownButton/SplitButton/ContextMenu、MenuFlyout、两种DataGrid筛选Flyout | 静态/工厂返回类型及真实装配正确 |
| Menu/HorizontalMenu/NavMenu | 普通菜单项、实际顶层Menu容器、垂直/水平NavMenu容器 | 默认类型、实例、覆盖与缓存通过 |
| Tooltip | 实际ToolTip主题和两方向 | 类型与实例正确 |
| Generic Popup / TreeView | 分别保留无动效、null→Expand/Collapse回退的独立合同 | 声明/默认类型正确，未改语义 |

最终15个属性声明入口、16份主题32个默认Setter、14个静态原型全部核对。AXAML逐条读取真实TargetType，验证被引用成员的声明类型、实际对象和ITemplate物化结果都可赋给对应属性；不是仅检查运行结果非空。C#非视觉工厂覆盖普通/Menu/TreeViewFlyout以及DataGrid菜单/树筛选两种派生。

- 临时完整审计43/43通过：23类实际owner，逐一双方向独立实例、用户Binding/显式null/ClearValue恢复、缓存身份；每类每方向1000次读取两个实例，总计92,000次读取额外线程分配均0B。11种owner/定义强持有退役回收场景通过，包含持有用过的PopupConfirmFlyout；实际PopupConfirm默认→Flyout→Popup定义身份一致，并与显式正确Popover配置的固定clock入/离场透明度轨迹一致，包含真实中间值与最终物理关闭。3种托管入口验证普通关闭仍可IsOpen重开、退役不再打开旧目标。
- DataGrid临时2/2通过，覆盖真实筛选Flyout方法声明/副本/ShowAt后的默认类型。两个临时目录及其源码已删除；没有环境探针或对象图扫描遗留。
- 用真实GC替换旧私有方法存在断言后，同批45/45通过；清理后最终精确模块：Desktop 82/82（10.889s含宿主）、DataGridFilterDialogPopupTests 4/4，共86项。覆盖PopupConfirm、Flyout三种托管/钉住/语义接入、资源生命周期、主题继承、TreeView展开及默认合同、选择器和Picker相关路径。不是全量回归，也未重新宣称上一轮1,797项为新源码验证。
- Debug net10.0相关项目构建随测试通过；Release net8.0实际编译通过（21.99s，0警告0错误）。本次新源码的osx-arm64 NativeAOT发布与output validation通过，未新增定位缓存反射相关trim警告；原ReactiveUI/DataGridBoundColumn/平台库警告维持既有记录。以新原生exe绝对路径核对PID86011，启动5秒持续运行后停止，仅声称启动冒烟。
- 私有定位目标方法再次以NuGet12.1.4指定commit `2d79ba55fae1b1e71c69f93a668080c9029e6a31` 的源码核对：SetDefaultPlacementTarget仅赋值_lastPlacementTarget，普通Hide会保留该字段以支持IsOpen重开；此处只在owner退役清空。固定方法名及Control参数签名受DynamicDependency保护。
- 一次独立只读审查 `/root/all_motion_defaults_review` 未发现Critical/Important；确认默认继承清单、旧owner身份和活动宿主保护、重开语义、Border释放保护与AOT边界。没有重复审查整个既有分支。

性能结论：默认读取缓存没有额外分配，帧驱动/资源策略与既有几何算法未变；新增定位/Border释放只在owner注销或替换边界执行，每次已关闭内容树遍历一次，不在普通开关周期或每帧执行。PopupConfirm此前无动画是功能缺陷，修后与显式正确动画基线轨迹一致；不拿“无动画”的CPU成本当优化基线，也不宣称FPS或时延改善。

所有验证详细证据集中在本记录。最终必要事实写入后删除本轮专用原始日志、对象路径及AOT产物；仅暂存本轮修正，保留并行导入/格式和Empty任务改动。不创建branch/worktree、不commit/push。

全组件复审实际清理确认：两个临时驱动目录已删除，`/tmp/atomui-motion-defaults-audit` 的71个专用文件（包括原始日志、对象路径与AOT产物）已删除，验证进程已退出。最终仅补暂存本轮14个既有路径，Flyout.cs仅暂存退役修正，用户导入清理、格式与Empty任务仍保留。HEAD/branch未变，未commit。
