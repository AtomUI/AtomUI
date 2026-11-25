# Linux Wayland · Avalonia 12.1.5 验收记录

日期：2026-11-05。分支 `feature/upgrade-12.1.5`；升级提交 `00197a821`，对照基线 `031135f11247bd418ca28e5aa5cc948d50969a22`（Avalonia 12.1.4）。本次包含工作树中的菜单淡出修复，未创建 commit。

本页记录功能、构建和发布验收；[追加源码与性能复核](2026-11-05-avalonia-1215-clock-performance-source-audit.md)单独记录三个时间域与真实成本，整体无性能回退和逐帧精准同步尚不能判为通过。

## 判定边界

真实 GNOME Wayland 主路径已通过当前平台探针；菜单默认模板的透明度合成问题已有红绿像素证据。整体测试并非全绿：选测 2298 项，2273 通过、25 失败、0 跳过。25 项在相同 Linux 环境的 12.1.4 源码快照中均复现为相同失败，未发现这些失败由本次升级引入。NativeAOT 与清理后模块验证结果见下文。

不能据此声称多输出混合缩放、Wayland 连接中断后恢复、各 GPU/驱动、真实鼠标/键盘/IME、所有控件人工视觉验收或全量测试已经通过。

## 环境与影响面

- Ubuntu 26.04 / Linux 7.0.0-29 / GNOME Wayland，x86_64。
- 单个 eDP-1：2560×1600、60.001 Hz；逻辑 1536×960，分数缩放 5/3；提供 wp_presentation v2。
- SDK 10.0.400；Avalonia `12.1.5+5825bf8c66fba0c11e67c980d8ece6444576bffa`；NWayland 0.11.0。
- 按 `031135f11..00197a821` 的 25 个改动文件规划，ownership gap 为 0。依赖版本属于跨模块变更，按实际消费链扩展到 10 项目、226 类精确 filter，未执行 full regression。
- 涵盖：Native 的 Wayland 私有契约、提交/缩放/帧节奏保护、Dispatcher 初始化时钟、Window 启动与装饰，Core 动效/资源/反射，Controls 图像与 Splash，Shared 集合视图，DataGrid 全部 43 类（绑定复制、编辑/排序/焦点/虚拟化），Popup/Menu/NavMenu/输入/选择器/对话框，渲染，Generator/Toolchain 注册及 receipt，GalleryBase 与 Gallery 启动/页面接线。
- 本次未执行本地化集成、Browser 运行、各平台冷启动九包消费矩阵；此前升级记录不能冒充本轮 Linux 的新验证。

## 升级选测

| 项目 | 总数 | 通过 | 失败 |
| --- | ---: | ---: | ---: |
| AtomUI.Build.Tasks.Tests | 52 | 52 | 0 |
| AtomUI.Controls.Tests | 27 | 27 | 0 |
| AtomUI.Controls.Shared.Tests | 10 | 10 | 0 |
| AtomUI.Core.Tests | 82 | 82 | 0 |
| AtomUI.Desktop.Controls.DataGrid.Tests | 316 | 296 | 20 |
| AtomUI.Desktop.Controls.Rendering.Tests | 77 | 77 | 0 |
| AtomUI.Desktop.Controls.Tests | 1462 | 1458 | 4 |
| AtomUI.Generator.Tests | 189 | 189 | 0 |
| AtomUI.Toolkits.GalleryBase.Tests | 28 | 28 | 0 |
| AtomUIGallery.Tests | 55 | 54 | 1 |

命令形态：`dotnet test <project> --framework net10.0 --filter "FullyQualifiedName~<class>|..." -p:BuildInParallel=false -p:UseSharedCompilation=false`。TRX 用于逐项核对失败与跳过；原始日志和临时快照按仓库规则清理。

### 25 项既有失败

对照只复跑失败所属类：DataGrid 48 项（28 通过/20 失败）、Desktop 70 项（66/4）、Gallery 2 项（1/1）。两个版本的失败名称与主要症状一致。DataGrid 主要为点击/焦点、排序等待和虚拟化布局；Desktop 为自然尺寸对话框 resize；Gallery 为 DataGrid 语义预览目标数。未修改这些既有问题。

#### AtomUI.Desktop.Controls.DataGrid.Tests

- `AtomUI.Desktop.Controls.Tests.DataGrid.Interaction.DataGridAutoCompleteTemplateFocusTests.AutoComplete_In_TemplateColumn_Keeps_Text_Input_Focus_After_Click(kind: Default)`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Interaction.DataGridAutoCompleteTemplateFocusTests.AutoComplete_In_TemplateColumn_Keeps_Text_Input_Focus_After_Click(kind: SearchEdit)`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Interaction.DataGridAutoCompleteTemplateFocusTests.AutoComplete_In_TemplateColumn_Keeps_Text_Input_Focus_After_Click(kind: TextArea)`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Interaction.DataGridAutoCompleteTemplateFocusTests.AutoComplete_Outside_DataGrid_Keeps_Text_Input_Focus_After_Click`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Interaction.DataGridDetailExpanderColumnRecycleTests.Scrolling_Up_Through_Expanded_Row_Details_Does_Not_Bounce_Back`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Interaction.DataGridEmptyTitleFocusTests.Title_Button_Click_Does_Not_Crash_When_Source_And_Columns_Are_Missing`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Interaction.DataGridEmptyTitleFocusTests.Title_Button_Click_Does_Not_Crash_When_Source_Is_Empty`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Interaction.DataGridEmptyTitleFocusTests.Title_Button_Click_Does_Not_Crash_When_Source_Is_Null`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Interaction.DataGridSingleSelectionIssue461Tests.Single_Click_Moves_Selection_To_Another_Row(clickTarget: BodyCell)`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Interaction.DataGridSingleSelectionIssue461Tests.Single_Click_Moves_Selection_To_Another_Row(clickTarget: SelectionIndicator)`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Layout.DataGridFrameCornerRadiusTests.Frame_Preserves_CornerRadius_And_Blank_Area_Input_When_Border_Is_Hidden`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Layout.DataGridFrameCornerRadiusTests.Frame_Preserves_CornerRadius_And_Blank_Area_Input_When_Border_Is_Visible`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Layout.DataGridFrameCornerRadiusTests.Last_Displayed_Row_Does_Not_Draw_Bottom_Grid_Line_When_Frame_Owns_Bottom_Border`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Sorting.DataGridSortColumnSelectedRowCellStyleIssue454Tests.SortedColumnCell_InSelectedRow_ShouldRenderSelectedRowBackground`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Sorting.DataGridSortColumnSelectedRowCellStyleIssue454Tests.SortedColumnCell_InUnselectedRow_KeepsSortTint_AfterSorting`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Sorting.DataGridSortingTests.Clicking_Right_Edge_Of_Sort_Indicator_Works_When_Column_Can_Resize`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Sorting.DataGridSortingTests.Clicking_Sort_Indicator_Reorders_Range_Rows`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Sorting.DataGridSortingTests.Column_Level_Sort_Works_When_Grid_Level_Sorting_Is_Disabled`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Sorting.DataGridSortingTests.Sort_Keeps_Source_Ordinal_As_Tie_Break_For_Duplicate_Ages`
- `AtomUI.Desktop.Controls.Tests.DataGrid.Sorting.DataGridSortingTests.Sort_Reorders_Rows_When_Cell_Bindings_Are_Compiled`

#### AtomUI.Desktop.Controls.Tests

- `AtomUI.Desktop.Controls.Tests.Dialog.OverlayDialogOwnerResizeTests.Non_draggable_natural_dialog_restores_its_viewport_and_center_through_owner_resize(dark: False)`
- `AtomUI.Desktop.Controls.Tests.Dialog.OverlayDialogOwnerResizeTests.Non_draggable_natural_dialog_restores_its_viewport_and_center_through_owner_resize(dark: True)`
- `AtomUI.Desktop.Controls.Tests.Dialog.OverlayDialogOwnerResizeTests.Owner_resize_temporarily_clamps_surface_without_rewriting_user_offsets`
- `AtomUI.Desktop.Controls.Tests.Dialog.OverlayDialogPresenterTests.Modal_With_Stretch_Steps_Keeps_Its_Natural_Size_In_Different_Owner_Widths`

#### AtomUIGallery.Tests

- `AtomUIGallery.Tests.ShowCases.DataGridSemanticPartHighlightTests.DataGrid_Semantic_Preview_Highlights_All_Declared_Parts`

## 菜单淡出修复

原始契约：保留菜单尺寸、颜色、选中/悬停状态、阴影、布局、动效时长和交互，让菜单背景、项目背景和文字作为同一组淡出。

根因：Skia 默认透明度快速路径会对重叠绘制分别混合。在黑底、25% 表面透明度下，两层白背景得到灰度 110，而整组混合应为 64。Avalonia 12.1.4 与 12.1.5 对应 Skia/Composition 源码相同，不能归因为该补丁版本新增。Composition 先应用当前节点 opacity，再压入其 RenderOptions，因此整体合成选项必须声明在动画节点父级。

修复：native PopupRoot 复用既有 ContentPresenter，overlay PopupSurface 增加一个不裁剪 Panel，Inline NavMenu 复用既有 StackPanel；均以静态 AXAML 声明 `RenderOptions.RequiresFullOpacityHandling=True`。该选项是 CLR attached API，不能写入普通 StyledProperty Setter。没有新增公共 API、代码绑定、计时器或订阅。新增 overlay 父级随模板释放，原动效和 host 生命周期保持原实现。

默认 MenuItem 子菜单、ContextMenu、NavMenu 的 Horizontal/Vertical/折叠弹层复用共享 Popup；Inline 分支单独处理视口。共享 Popup 的 Flyout、ToolTip、选择器等也受到影响，最终模块验证包含对应现有契约。用户完全替换模板的路径需自行保留相同合成边界。

| 复现 | 修复前 | 单变量 A/B / 修复后 |
| --- | --- | --- |
| ContextMenu，25% opacity | 预期 64，实际 110；2/2 失败 | 只改变分组合成恢复 64；正式模板 2/2 通过 |
| 深色选中 Inline NavMenu | 最大 RGB 误差 4.5/255，超过阈值 3 | 最大误差 0.5/255 |
| 浅色选中 Inline NavMenu | 误差 1/255，通过 | 误差 0.75/255，通过 |

像素测试使用真实 ContextMenu/NavMenu 模板与 headless Skia Composition；请求 native/overlay 的两个 headless 用例最终都退化为 overlay，不算原生 PopupRoot 帧缓冲像素验收。真实 Wayland 补充验证了两种实际宿主、父级合成选项命中、关闭过程 opacity 从 1 经中间值降到 0，以及关闭后会话结束；未冒充原生截图或人工视觉检查。

四项临时测试位于 `tests/AtomUI.Desktop.Controls.Rendering.Tests/Temporary/Avalonia1215`，红绿验证后已删除；零新增常驻测试。长期保障由现有 Popup/NavMenu 生命周期、主题、布局、动效、资源释放和渲染契约测试承担；像素探针属于本次专项证据，未来不自动运行。

## 真实 Linux 平台探针

- 使用本 checkout 的 Debug Gallery 构建（0 warning/error）及 launcher 核对精确可执行路径和 PID，运行后停止。临时原生探针复用 Gallery BuildAvaloniaApp / ClassicDesktop lifetime，不使用无头平台冒充 Wayland。
- 后端为 `Avalonia.Wayland.WindowImpl`（修正原记录的类型名笔误，追加真实宿主对照也核对到此类型）。Window 构造时与映射后缩放均为 1.6666666667。三轮 native 阴影 Popup + nested Popup 的显示、resize、关闭和跨根坐标往返通过（误差不超过 1.5 DIP）。
- 真正 native ContextMenu 的 root 为 PopupRoot，overlay 为 Window；两者父级整体合成声明生效，实际关闭 opacity 有中间帧并到达 0。
- 最终运行的呈现反馈：presented 192、predicted 189、prediction hits 79、started 199、inFlight 0；无 frame-pacing 安装或提交错误。这些是内部计数，不是 FPS、丢帧率或性能优于旧版的结论。
- 帧节奏关闭的单变量原生对照通过。初始化前先创建 Dispatcher、预排 600ms timer、等待 400ms 后初始化 Wayland，timer 在 657.94ms 触发；后续 30 个 16ms timer 平均/最大间隔通过阈值（最大 21.45ms），未出现新的时钟原点间隙。
- X11/XWayland 的相同嵌套几何/resize/关闭场景执行完成，但严格错误日志检查因 IBus `org.freedesktop.DBus.Error.UnknownMethod: Destroy` 失败；12.1.4 原生对照也出现相同错误，不能报告 X11 完整通过。
- 现有 GalleryPerformance 原生 popup 工具因 `missed 59 compositor batches` 退出，没有可用性能结果。源码显示 AfterCommit 时读取 `_pendingBatch` 会与同步处理完成后清空该字段竞态；这不是可直接解释为屏幕丢 59 帧的证据，未为本任务改动性能工具。

## 构建、发布与最终清理

- 清理仓库 Temporary 后，按 Popup/NavMenu iterate 计划并加入实际 Menu/Motion 消费者，精确运行 Desktop 50 类 **552/552**；Skia 渲染 5 类 **77/77**；进一步验证 Flyout/Tooltip/ColorPicker/Dropdown/组合按钮 14 类 **97/97**。本轮最终模块验证合计 **726/726，0 失败、0 跳过**，与前述升级选测有重叠，不能相加冒充独立用例总数。
- `dotnet build src/AtomUI.Desktop.Controls.DataGrid/AtomUI.Desktop.Controls.DataGrid.csproj -c Release --framework net8.0 --no-restore -p:BuildInParallel=false -p:UseSharedCompilation=false`：成功，0 warning/error；覆盖 Native/Core/Shared/Controls/Desktop/DataGrid 的 net8.0 编译。net10.0 已由 Debug Gallery、选测及 Release AOT 构建验证。
- Linux NativeAOT：`dotnet publish controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj -c Release -r linux-x64 -p:GalleryPublishTrimmed=true -p:GalleryPublishAot=true -p:BuildInParallel=false -p:UseSharedCompilation=false -o <系统临时目录>/aot-publish`。使用临时安装的 10.0.14 runtime 承载 SDK 10.0.400；发布退出 0，出现 Generating native code，ILC 和 nativeaot runtime 引用均为 **10.0.14**。
- 产物为 x86-64 ELF，大小 95204576 字节；不包含 `libcoreclr.so`、`System.Private.CoreLib.dll` 或托管入口 `AtomUIGallery.Desktop.dll`。主程序 SHA-256：`4c8a542a1095e3005f171c7a5d953081feac3ed4730c5d88bf159c56e1cb57f8`。
- 用该 ELF 强制 Wayland 启动，核对 `/proc/<pid>/exe` 为本次发布文件，持续 12.02 秒且未加载 CoreCLR、未崩溃，随后主动停止。另以 gdb 启动同一 ELF，实际命中 `WaylandFramePacing.TryInstall` 返回 **true**，并命中真实 Wayland worker 的 `WaylandPresentationTiming.OnPresented`（refresh 16666300ns、Vsync）；因此私有契约保留与 prepared-frame 呈现链不是仅凭未崩溃推断。
- AOT 日志保留 ReactiveUI / ReactiveUI.Avalonia 的 4 条 IL2104/IL3053 汇总 warning；未出现 IL2026、IL3050、NU1903，未以 suppress 隐藏。AOT DataGrid 编辑场景未人工操作；本轮用现有绑定/保留契约测试和实际链接验证，不能把它表述为原生编辑交互已验收。
- 生产主题 diff SHA-256：`ec7f48c5f94919a8a24977f6d510d930b58ca0af26899de5086bac7b9826aa37`。新变更仅 3 处主题与对应文档、计划/验收文字记录；保留用户原有 5 份未跟踪文档，未提交。
- `python3 scripts/verification/test.py audit` 退出 0；`git diff --check` 通过。临时测试、外部探针、基线快照、AOT 发布/临时 runtime、PNG/TRX/原始日志均已删除；本任务启动的 Gallery、探针、调试器进程已停止。

## 审查与剩余边界

- 独立只读审查未发现两处 Popup 模板修复的确定性缺陷；指出原生像素与 Inline 覆盖缺口。补充了真实宿主/实际淡出探针与深色选中 Inline 红绿验证；原生帧缓冲像素和人工 UX 检查仍未覆盖。
- 透明度不为 1 时整体合成需要离屏层；overlay 多一个静态 Panel。未宣称性能无变化，现有工具本轮未产生有效性能数据。
- 所有已知测试失败、X11 IME、平台覆盖缺口均保留为限制，不用 skip 或改变原始 UX 隐藏问题。
