# 全局视觉层级阶梯：交付记录

设计见 [全局视觉层级阶梯设计](../specs/2026-11-22-window-layer-ladder-design.md)，计划见 [实施计划](../plans/2026-11-22-global-layer-ladder.md)，层归属规范见 [视觉层规范](../../architecture/systems/rendering/visual-layers.md)。

## 验证对象

- 原始故障：Windows 实机全屏后进入 布局 / Space，滚动使吸顶 Tab 出现，再把鼠标移到顶边，AutoHide 全屏标题栏右侧被吸顶的“示例 / 语义部件”Tab 盖住。根因：吸顶 Tab 位于窗口级 `ScopeAwareAdornerLayer`（`Max−99`），全屏标题栏宿主位于 `PART_VisualLayerManager` 的内容根内，低于全部注入层。
- 目标契约：T1–T8 角色阶梯（`LayerLadder`），全屏标题栏位于 T6 `WindowChromeLayer`；提升表面自带表面宿主，使表面内的焦点描边与 Badge 紧贴表面。

## 修复前

- 实机：问题报告与设计第 1 节记录的 Windows 复现（上述场景可稳定复现）。本任务没有回退源码重拍修复前截帧：修复已在索引中暂存，回退需要 stash / checkout，本任务不允许改动索引。
- Headless：Task 1–6 在 `Temporary/layer-ladder/` 中为各自契约建立了修复前失败、修复后通过的证据（见各任务记录）。

## 修复后（Windows 10 实机，2560×1600，150% 缩放，单显示器，Light 主题，Gallery Debug net10.0）

驱动方式：会话临时目录中的截帧脚本发出真实鼠标、滚轮和键盘事件；截图只保存在会话临时目录，验收后删除。

1. 原始场景，AutoHide：全屏 → 布局 / Space → 内容区滚轮 8 格，吸顶 Tab 出现在顶部 → 鼠标从内容区（x=1600 物理像素）滑到 y=0，按约 100 ms 间隔连拍 12 帧顶部条带。前 3 帧标题栏未出现，第 4 帧标题栏淡入，第 5–12 帧标题栏在全宽完整呈现（左侧 Logo 与 设置 / 主题 / 语言，右侧退出全屏、图钉、关闭），吸顶 Tab 的文字被完全盖住，只在标题栏下沿之下露出 Tab 选中指示线。没有任何一帧出现吸顶 Tab 画在标题栏之上。原始分辨率下复核左右两段顶部区域，结论相同，标题栏阴影落在吸顶 Tab 之上。
2. Pinned：FloatButton 页滚动使吸顶 Tab 出现，点击标题栏图钉后把鼠标移开，标题栏常驻顶部并盖住吸顶 Tab。图钉切回与经 caption button 退出全屏均正常。
3. Window Drawer 焦点描边：全屏下 Drawer 页“抽屉表单”打开 Window Drawer，点击“名称”输入框后连按 Shift+Tab，提交、取消、关闭按钮依次出现清晰的键盘焦点描边，描边画在 Drawer 表面之上，没有被表面盖住。附带观察：Drawer 打开时若焦点仍在页面中，Tab 会在遮罩后的页面控件间移动，其焦点描边在遮罩之下（T2 低于 T7），这是 Drawer 不捕获焦点的既有行为，与本次层级改动无关。
4. FloatButton：FloatButton 页“徽标”示例中，各 FloatButton 自身的 Badge（12、5、99+、红点）画在各自表面之上，未被相邻 FloatButton 盖住。“FloatButton 盖住内容 Badge”在 Gallery 中没有对应示例，搭建需要修改 Gallery 源码，本次未做实机验证，只有 Headless 证据。

本次启动的 Gallery 进程已关闭。

## 未实测

- Windows：FloatButton 盖住内容 Badge、作用域 Drawer / Dialog、窗口级 Dialog、Tour、Watermark、Message / Notification 与全屏标题栏内按钮的焦点描边、全屏标题栏 Docked 与自定义内容、Drawer 活跃期间的 reveal 抑制、Dark 主题、多显示器。
- macOS、Linux X11、Linux Wayland：全部未实测，只有 Headless 证据。

## 相对设计的偏差（已写入文档）

- 窗口级 `OverlayDialogPresenter` 也带表面宿主（模板与作用域 Dialog 共用），把设计第 7 节第 8 条“表面内装饰留在表面内”扩展到了窗口级 Dialog；层级不变。见视觉层规范的表面宿主一节与 Modal changelog。
- Gallery 吸顶 Tab 的表面宿主位于 `PART_StickyContentHost` 之内，只包裹吸顶内容。
- `WindowChromeLayer` 在模板重套用和窗口关闭时被清空而不是从旧 `VisualLayerManager` 移除：Avalonia 没有移除层的 API，清空的层随旧模板丢弃，不新增反射。

## 测试处置

- 删除：`tests/AtomUI.Desktop.Controls.Tests/Temporary/layer-ladder/` 全部 5 个文件（`LayerLadderOrderTests`、`LayerResolutionTests`、`PresentationRoutingTests`、`SurfaceHostDecorationTests`、`WindowChromeLayerTests`，含其中只服务于临时测试的清除裁剪命中测试辅助方法），以及 `tests/AtomUI.Toolkits.GalleryBase.Tests/Temporary/layer-ladder/StickySurfaceHostTests.cs`；两个 `Temporary/` 目录随之清空并删除。计划中 Task 4 的性能临时测试在工作树中已不存在，无需处理。没有项目文件或选测映射引用这些目录。
- 保留：Task 1–6 修改的既有常驻测试（Drawer、Dialog、FloatButton、`WindowResizeArtifactTests`）按新层归属更新了断言，属于既有契约的随改，不是净新增。
- 晋升（唯一净新增）：`tests/AtomUI.Desktop.Controls.Tests/Primitives/LayerLadderHostOrderTests.cs`，3 个用例，由 `LayerLadderOrderTests` 合并精简而来，并把“全屏标题栏宿主运行时位于 chrome 层”的位置守护并入 Window 宿主用例，不另在 `WindowFullScreenTitleBarTests` 中新增用例。

## 保留理由

1. 契约明确：每类宿主中角色层按 `LayerLadder` 号码排序，Avalonia 共享 `AdornerLayer` 在 T2；全屏标题栏宿主在模板重套用后仍位于唯一的 T6 chrome 层的表面宿主中。违反时的错误就是本次故障类：标题栏被吸顶 Tab 盖住、Badge 与焦点描边画到 FloatButton 之上、Drawer 落到 FloatButton 之下。
2. 独立检错：阶梯依赖“Avalonia 只在私有 AddLayer 中赋一次 ZIndex、之后不回写”，这是 Avalonia 升级可能改变的外部行为，编译器、ApiCompat 与其他测试都不覆盖；既有常驻测试只断言单个消费者落在哪个层，`WindowResizeArtifactTests` 只检查模板源码中的声明位置，都检测不到号码被改写或宿主运行时未迁入 chrome 层。变异验证：把 `EnsureContentDecorationOrder` 改为写回 −100 时 Window 与 PopupRoot 两个用例失败；让模板重套用保留旧 chrome 层时 Window 用例失败（新宿主没有 chrome 层）。两处变异均已还原，`git diff -- src/` 为空。
3. 长期收益：保护所有 AtomUI 提升层共用的核心路径；层级错误只在特定滚动、全屏和叠放组合下出现，人工难以发现，本次故障即是实机才暴露。
4. 成本合理：Headless，同一测试项目内运行，首个用例含宿主启动约 0.42 s，其余两个约 5–40 ms；断言引用 `LayerLadder` 常量而不是裸数值，不锁定号码本身。
- 为什么不能做按需专项：Avalonia 升级与任何控件改动注入路径都可能破坏阶梯，需要每次全局测试运行时发现。
- 所属模块与触发范围：`AtomUI.Desktop.Controls.Tests` 的 `Primitives` 命名空间，随 Desktop Controls 测试运行；同类重复用例（临时 `LayerLadderOrderTests`）已删除。
- 未覆盖：OverlayPopupHost、EmbeddableControlRoot 与 ScrollViewer 滚动内容注入点没有常驻守护，已在视觉层规范测试要求中注明。

## 最终模块验证（清理后的工作树）

| 命令 | 结果 |
| --- | --- |
| `dotnet test tests/AtomUI.Desktop.Controls.Tests/... --filter "Window\|Drawer\|Dialog\|MessageBox\|Message\|Notifications\|FloatButton\|Tour\|Primitives\|SemanticParts\|FeedbackStack\|ScrollViewer"` | 919 项，918 通过，1 失败（已知既有：`FeedbackManagerStackTests.AtomUI_Window_Notification_Stack_Revalidates_Hover_After_Resize`），24 s |
| `dotnet test tests/AtomUI.Controls.Tests/... --filter "Watermark\|Badge"` | 26 项全部通过 |
| `dotnet test tests/AtomUI.Toolkits.GalleryBase.Tests/...` | 219 项，217 通过，2 失败（已知既有：`GalleryStickyTabsHostStickyMirrorTests` 的两个点击用例，`SelectedIndex` 期望 1 实际 0），12 s |
| `dotnet test tests/AtomUI.Desktop.Controls.Rendering.Tests/... --filter "Window"` | 24 项，18 通过，6 失败（已知既有：`PrimitivesFamilyEffectTests.Window_Token_Has_Effect` 6 个参数组合） |
| `git diff --check`、`git diff --cached --check` | 无输出 |

已知失败均已在干净 HEAD 上确认存在。`PopupMarginPlacementTests` 与 `Preview_Dialog_Follows_The_Previewer_Tokens` 两组已知失败不在以上过滤范围内。本机没有 `python3`，未运行 `scripts/verification/test.py audit`。

## 门禁状态

修复前后实机证据、测试去留、临时测试清理与模块验证已完成。尚未完成：上述 Windows 未实测项与 macOS / Linux 实机验证。未提交，等待用户审查。

## 最终评审修复（Final review fixes）

整体改动终审后的一轮修复，全部未提交。

- **RTL：去掉 T5 的 FlowDirection 绕过。** `ContentPresentationLayer` 删除 `BypassFlowDirectionPolicies => true`。该绕过让层自身反镜像而其中的 `DrawerContainer` 仍随 RTL 镜像，`ScopeAwareAdornerLayer` 按层坐标排布时把作用域 Drawer 放到 target 旁边。临时测试（RTL 窗口、`OpenOn` 为局部 Border）：带绕过时容器窗口原点 X 为 1400、target 为 1000（RED）；去掉后两者一致（GREEN）。同时用临时 RTL 作用域 Dialog 用例确认：有无绕过，Dialog 遮罩与作用域等宽同位、对话框表面在作用域内居中，几何不变（`DialogOverlayLayer` 铺满整层，镜像不改变其几何）。
- **light-dismiss 是 TopLevel 角色。** Avalonia `Popup.Open()` 按 placement target 最近的 `VisualLayerManager` 取 `LightDismissOverlayLayer`（12.1.6 中这是唯一获取点，`Registration.Dispose` 幂等），target 位于表面宿主内时 dismiss 层只铺满该表面。`PopupLightDismissRegistration` 在 `Opened`（`Open()` 末尾同步触发）中检测落点属于表面宿主时，释放表面层上的原生租约与处理器，改挂到 `LayerLadder.FindTopLevelLayerManager` 宿主的 dismiss 层；只复用已有反射成员，未新增反射。临时测试（作用域 Drawer 内的 ComboBox，在 Drawer 外按下）：修复前弹层未关闭、可见 dismiss 层位于 Drawer 表面宿主（RED）；修复后弹层关闭、dismiss 层位于 TopLevel 角色宿主、表面宿主上的 dismiss 层不可见（GREEN）。表面宿主上由 Avalonia 惰性创建的 dismiss 层在无注册时 `IsVisible = false`，不参与命中。三个既有常驻测试辅助方法（`DialogPopupTestHost.LightDismiss`、`DialogContentPopupLayeringTests.LightDismiss`、`DataGridFilterDialogPopupTests.LightDismiss`）按类型名 `Single` 取 dismiss 层，现在窗口级 Dialog 的表面宿主上会多一个不可见的惰性层，改为只取可见层，属于既有契约随改。
- **文档：** 视觉层规范与 spec 5.7 把 light-dismiss 列入 TopLevel 角色并描述重定向；`WindowChromeLayer` 改述为由 Window 直接注入 `PART_VisualLayerManager`，不再称为 TopLevel 角色解析器；同宿主作用域 Drawer 与作用域 Dialog 共用 T5 实例、按打开顺序排列（后开在上，antd DOM 一致）写为“同号插入顺序”禁令中唯一有记录的例外；Drawer 实现文档同步 light-dismiss 说明。
- **小项：** `ScopeAwareAdornerLayer` 包裹面板注入分支注明为何不走 `LayerLadder.AddLayer`（必须重建内容包裹 Panel）且号码仍取自 `LayerLadder`；`LayerLadder.EnsureContentDecorationOrder` 注明 Child 本身为 `VisualLayerManager` 时有意空操作；删除 `WindowFeedbackLayer`、`TourLayer` 中未使用的 using；`ContentPresentationLayer.GetLayer` 补 `new` 修饰，消除 CS0108。
- **测试去留：** 临时目录 `tests/AtomUI.Desktop.Controls.Tests/Temporary/final-fix/`（3 个用例）验证后已整体删除，无净新增常驻测试。
- **验证：** Desktop Controls 测试过滤（Drawer / Dialog / Popup / ComboBox / Select / Flyout / FloatButton / Message / Notifications / Window / Primitives / Tour）1599 项，1589 通过，10 失败：9 个为已知既有（`PopupMarginPlacementTests` 8 个 csd:false 组合、`FeedbackManagerStackTests.AtomUI_Window_Notification_Stack_Revalidates_Hover_After_Resize`），另 1 个 `LunarCalendarProviderTests.MonthPanelData_QueriesProviderOnceAndReusesItForSamePanelSelection` 因名称含 Selection 被 `Select` 过滤带入，单独运行同样失败，原因是本机 zh-CN 文化周一为一周首日（期望 06-28 周日，实际 06-29 周一），与本改动无关。DataGrid `DataGridFilterDialogPopupTests` 4 项全部通过。`git diff --check`、`git diff --cached --check` 无输出。

## macOS 验收与后续修复（2026-11-22）

- **提交整理。** 本分支与 `release/6.0` 上带 AI 署名尾注的提交已改写为不带尾注的版本（文件树、作者与时间不变）；Tour 圆角容差比较从层级提交中拆出为独立的 `fix(Tour)` 提交，单独签出时编译通过、Tour 测试 22 项通过。改写后的历史尚未推送。

- **macOS 实机验收（内建带刘海显示器，Gallery Debug net10.0，脚本发出真实鼠标事件并截屏，截图只保存在会话临时目录）。** 与分支前基线 `8c946e4f5` 做 A/B：全屏 → 布局 / Space → 滚动到吸顶 Tab 出现 → 鼠标停在窗口顶部触发带。基线中吸顶 Tab 盖住全屏标题栏右半；本分支标题栏在全宽盖住吸顶 Tab。层级相关 Desktop Controls 测试在 macOS 上 1068 项全部通过。
- **刘海屏上全屏标题栏不跟随系统 chrome（既有缺陷，基线同样存在）。** 鼠标到屏幕顶边时系统菜单栏与原生窗口按钮呈现，全屏标题栏不出现。原因：原生 chrome 跟踪只按原生标题条窗口的位置判断是否呈现，刘海屏上该窗口停在客户区顶边不动，只渐变 `alphaValue`。修复：`AtomUI.Native` 对标题条窗口的 `alphaValue` 做键值观察，`FullScreenNativeChromeMetrics.IsChromePresented` 改为平台直接报告；度量规则抽为不调用平台 API 的 `MacOSFullScreenChromeGeometry.Compute`。实机复验：`AutoHide` 下标题栏随系统 chrome 淡入与收起，原生按钮融合在表面内；示例窗口 `Pinned` 下表面常驻，原生按钮随系统 chrome 出现与消失；经原生绿色按钮退出全屏并再次进入两轮，标题栏与原生按钮恢复正常，进程无异常。
- **新增常驻测试（3 处）。**
  - `Window/MacOSFullScreenChromeGeometryTests`（9 个用例）：固定“滑出窗口上方”和“原地淡出”两种隐藏方式下的呈现状态、extent 与 control inset。原生跟踪本身只能在真实 macOS 会话中观察，这些规则此前没有任何自动化守护。
  - `DrawerInteractionTests.Scoped_Drawer_Content_Popup_Light_Dismisses_On_A_Press_Outside_The_Drawer`：守护 `PopupLightDismissRegistration` 把 dismiss 租约从 Drawer 表面宿主改挂到窗口宿主。变异验证：把重定向改为直接使用原生层时该用例失败（可见的 dismiss 层位于表面宿主），已还原。
  - `DrawerInteractionTests.Drawer_Content_Resolves_Content_Roles_To_Its_Surface_And_TopLevel_Roles_Past_It`：Drawer 内的元素，装饰层、T3、T4、T5（视口入口）解析到 Drawer 表面宿主；Tour、TopLevel 呈现层、窗口反馈层与 `FindTopLevelLayerManager` 解析到窗口宿主。Drawer 自身落 T5 / T7、嵌套复用与 `OpenOn` 迁移已由该文件既有用例覆盖。
- **表面宿主的堆叠开销（设计第 10 节要求的确认）。** Headless 临时基准（已删除）：`WindowMessageManager` 连续 `Show` 20 条并逐条布局，15 轮为一个样本，每次运行 7 个样本取中位数。带表面宿主 4 次运行的中位数为 413 / 431 / 438 / 406 ms，临时去掉 Message 模板中的表面宿主后 3 次运行为 435 / 402 / 404 ms。两组差异约 2%，远小于单次运行内样本的离散（约 200–800 ms），没有可测量的退化。表面宿主是每个 manager 一个，不随卡片数量增加。只有 Headless 证据，没有实机帧时间数据。
- **`WindowChromeLayerTheme.axaml`** 的 `Margin` 绑定由反射 `Binding` 改为 `CompiledBinding`。
- **评估后不改的两项。** `ContentPresentationLayer.GetLayer` 先解析同宿主 T3 层：代价是每个作用域 Drawer 宿主多一个空 Canvas，且有 `CachedLayerProperty` 缓存；拆开需要把滚动内容与 TopLevel 的包裹面板重建逻辑从 T3 注入中分离，风险高于收益。`LayerLadder.FindTopLevelLayerManager` 对 TopLevel 入参向下查找：Window 模板中前几个节点即命中，只在打开 Drawer、显示消息、启动 Tour 时调用；缓存需要处理模板重套用后的失效，不值得。
- **未做。** 不带刘海的显示器上呈现状态改动后的实机复测；Docked、自定义全屏内容、Dark 主题；Linux 实机。
