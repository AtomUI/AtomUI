# 全局视觉层级阶梯 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 建立 AtomUI 全局层级模型（T1–T8 角色阶梯、按角色分层、提升表面自带表面宿主、全屏标题栏登记为窗口 chrome 层），从源头修复全屏标题栏被吸顶 Tab 等内容装饰遮挡的问题。

**Architecture:** 新增唯一号码真源与统一注入入口 `LayerLadder`。所有 AtomUI 层都经它注入，注入时把 Avalonia `AdornerLayer` 重新编号到 T2。原来混放职责的两个层按角色拆开，新增 T5/T7 呈现层与 T6 chrome 层。每个提升表面用一个带标记的、只启用 Adorner 的嵌套 `VisualLayerManager` 作为"表面宿主"：内容角色的解析器就近落入表面宿主，TopLevel 角色的解析器跳过它。

**Tech Stack:** .NET 10、Avalonia 12.1.6、xUnit v3 + Avalonia.Headless、Shouldly。

**Spec:** `docs/developments/specs/2026-11-22-window-layer-ladder-design.md`（已批准）。执行者必须同时读 spec。

## Global Constraints

- 在主线 `release/6.0` 上开发，不得创建分支或 worktree（AGENTS.md）。
- **未经用户明确指令不得 git commit**：每个任务结束时只准备提交信息草稿，交用户审查。
- 号码（`int.MaxValue − N`）：T2 内容装饰 1000；T3 内容覆盖 900；T4 内容浮动 800；T5 内容呈现 700；T6 窗口 chrome 600；T7 TopLevel Drawer 500；T7 Tour 400；Avalonia Overlay/LightDismiss/TextSelector/PopupOverlay 98/97/96/95（不改）；T8 窗口反馈 50。
- 不新增反射；Avalonia `AdornerLayer` 通过公开 API `AdornerLayer.GetAdornerLayer` 创建，注入沿用 `VisualLayerManagerReflectionExtensions.AddLayer`。
- ControlTheme 优先（AGENTS.md）；只有 ControlTheme 无法表达时才写代码绑定，并在注释中写明原因。
- C# 控制流体必须带 `{}`。
- Drawer、Dialog、FloatButton、Watermark、Message、Tour 的 public / protected API 不变。
- 新测试先放 `tests/AtomUI.Desktop.Controls.Tests/Temporary/layer-ladder/`，文件头注明"临时验证，功能或修复完成后删除"，最终按 test-value-and-lifecycle 规范处置（Task 7）。
- 本机 `python3` 不可用：focused 验证用精确的 `dotnet test --filter`。不主动跑全量回归。
- 局部 ZIndex（spec 第 2 节非目标列出的控件内部排序）一律不动。

## Review Focus

1. **Window 模板重套用**（切换 `IsCsdEnabled` 会换模板）：新 `PART_VisualLayerManager` 中只有一个 `WindowChromeLayer`，宿主已迁入，旧层已移除，AdornerLayer 已重新编号。测试归 Task 5。
2. **非 Window 的 TopLevel 中的 Drawer**（PopupRoot 内容里没有父 Drawer 的 Drawer）：落入该宿主的 T7 `TopLevelPresentationLayer`，并且不获取 chrome 租约。测试归 Task 3。
3. **打开状态下 `OpenOn` 从 Window 改为局部元素**：container 从 T7 移到 T5，旧层不再含 container，Window 的抑制计数归零。测试归 Task 3。
4. **从表面宿主内打开 overlay Popup**（Drawer 内按钮的 `Popup.ShouldUseOverlayLayer = true`）：Popup 落在 TopLevel 的 `PopupOverlayLayer`，在 Drawer 之上，而不是被关进 Drawer。测试归 Task 4。
5. **全屏标题栏 Pinned 且 TopLevel Drawer 打开时弹出 Message**：Message 区域的命中测试落在 Message 卡片上。测试归 Task 5。

---

### Task 1: 号码真源、统一注入入口与现有层迁移

**Files:**
- Create: `src/AtomUI.Controls/Primitives/VisualLayers/LayerLadder.cs`
- Modify: `src/AtomUI.Controls/Primitives/VisualLayers/VisualLayerManagerUtils.cs:8-9`（常量改为转发）
- Modify: `src/AtomUI.Controls/Primitives/VisualLayers/ScopeAwareAdornerLayer.cs:425-460`（`InjectLayer`）
- Modify: `src/AtomUI.Controls/Primitives/VisualLayers/ScopeAwareOverlayLayer.cs:55-85`（`InjectLayer`）
- Modify: `src/AtomUI.Desktop.Controls/Tour/TourLayer.cs:15,129`
- Modify: `src/AtomUI.Desktop.Controls/Primitives/WindowFeedbackLayer.cs:12,34`
- Modify: `src/AtomUI.Desktop.Controls/Window/Window.cs:1115-1122`（`OnApplyTemplate` 中确保 T2 号码）
- Test: `tests/AtomUI.Desktop.Controls.Tests/Temporary/layer-ladder/LayerLadderOrderTests.cs`

**Interfaces:**
- Produces（`AtomUI.Controls.Primitives.LayerLadder`，`public sealed class`，private ctor）：
  - `internal const int ContentDecoration = int.MaxValue - 1000;` `ContentOverlay = int.MaxValue - 900;` `ContentFloating = int.MaxValue - 800;` `ContentPresentation = int.MaxValue - 700;` `WindowChrome = int.MaxValue - 600;` `TopLevelPresentation = int.MaxValue - 500;` `Tour = int.MaxValue - 400;` `WindowFeedback = int.MaxValue - 50;`
  - `internal static void EnsureContentDecorationOrder(VisualLayerManager manager)`：幂等；取得该 manager 自己的 `AdornerLayer`（父级必须是该 manager），把 `ZIndex` 设为 `ContentDecoration`。
  - `internal static void AddLayer(Layoutable host, Control layer, int zIndex)`：host 是 `VisualLayerManager` 时先 `EnsureContentDecorationOrder` 再走反射 `AddLayer`；host 是 `Panel` 时设 `layer.ZIndex = zIndex` 后加入 `Children`；其他 host 抛 `ArgumentException`。

- [ ] **Step 1: 写失败测试** `LayerLadderOrderTests`：
  - `Window_Host_Orders_Every_Role_By_The_Ladder`：显示 AtomUI Window，依次触发 `ScopeAwareAdornerLayer.GetLayer(content)`、`ScopeAwareOverlayLayer.GetLayer(content)`、`TourLayer.GetTourLayer(content)`、`WindowFeedbackLayer.GetLayer(window)`，对 `PART_VisualLayerManager` 的直接子层断言：`AdornerLayer.ZIndex == int.MaxValue-1000`、`ScopeAwareAdornerLayer == -900`、`ScopeAwareOverlayLayer == -800`、`TourLayer == -400`、`WindowFeedbackLayer == -50`，并且按 ZIndex 排序后的类型序列为 `[content Panel, AdornerLayer, ScopeAwareAdornerLayer, ScopeAwareOverlayLayer, TourLayer, OverlayLayer Panel, LightDismiss, TextSelector, PopupOverlay, WindowFeedbackLayer]`（只断言已创建的层）。
  - `AdornerLayer_Is_Renumbered_Before_Any_Layer_Injection`：Window 显示后、未注入任何层时，`PART_VisualLayerManager` 的 `AdornerLayer.ZIndex == int.MaxValue-1000`。
  - `Ladder_Survives_Popup_FullScreen_And_Template_Reapply`：完成上一条后，开关一次 overlay Popup、FullScreen 往返、`IsCsdEnabled` 翻转一次，再对新的 `PART_VisualLayerManager` 断言同样的号码。
  - `PopupRoot_Host_Uses_The_Same_Ladder`：打开一个原生 Popup（`ShouldUseOverlayLayer = false`），对 Popup 内容调 `ScopeAwareAdornerLayer.GetLayer`，断言其宿主 `VisualLayerManager` 的 `AdornerLayer == -1000` 且 `ScopeAwareAdornerLayer == -900`。
  - `ScrollViewer_Injection_Points_Use_The_Same_Ladder`：ScrollViewer 内的元素调 `ScopeAwareOverlayLayer.GetLayer`，断言注入到 `PART_ContentHost` 的层 `ZIndex == -800`。
- [ ] **Step 2: 运行确认失败**：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~Temporary.layer_ladder.LayerLadderOrderTests"`，Expected：号码断言失败（−100 / −99 / −990 / −98）。目录名 `layer-ladder` 会映射到命名空间 `layer_ladder`，以实际命名空间为准。
- [ ] **Step 3: 实现 `LayerLadder`**，把四个注入路径改为调用 `LayerLadder.AddLayer(host, layer, LayerLadder.X)`；`VisualLayerManagerUtils.ScopeAwareAdornerLayerZIndex` / `ScopeAwareOverlayZIndex` 改为转发到 `LayerLadder.ContentOverlay` / `ContentFloating`；删除 `TourLayer`、`WindowFeedbackLayer` 中的私有号码常量；`Window.OnApplyTemplate` 在找到 `PART_VisualLayerManager` 后调用 `LayerLadder.EnsureContentDecorationOrder`。
- [ ] **Step 4: 运行确认通过**：同上 filter，Expected：全部 PASS。
- [ ] **Step 5: 回归现有层测试**：`--filter "FullyQualifiedName~Tour|FullyQualifiedName~Message|FullyQualifiedName~Notifications|FullyQualifiedName~FloatButton|FullyQualifiedName~Primitives|FullyQualifiedName~Window"`（Desktop.Controls.Tests）；以及 `dotnet test tests/AtomUI.Controls.Tests/AtomUI.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~Watermark|FullyQualifiedName~Badge"`。Expected：PASS；若有断言旧号码（−99 / −990 / −98）的测试，改为断言相对顺序或引用 `LayerLadder` 常量。
- [ ] **Step 6: 准备提交信息草稿**（`refactor(VisualLayers): introduce the global layer ladder`），不提交。

### Task 2: 表面宿主标记与按角色区分的解析

**Files:**
- Modify: `src/AtomUI.Controls/Primitives/VisualLayers/LayerLadder.cs`
- Modify: `src/AtomUI.Controls/Primitives/VisualLayers/ScopeAwareAdornerLayer.cs:72-117`（`GetLayer`、`FindFirstDescendantLayerManager`）
- Modify: `src/AtomUI.Controls/Primitives/VisualLayers/ScopeAwareOverlayLayer.cs`（宿主解析）
- Modify: `src/AtomUI.Desktop.Controls/Tour/TourLayer.cs:104-140`
- Modify: `src/AtomUI.Desktop.Controls/Primitives/WindowFeedbackLayer.cs`（`FindFirstLayerManager`）
- Test: `tests/AtomUI.Desktop.Controls.Tests/Temporary/layer-ladder/LayerResolutionTests.cs`

**Interfaces:**
- Consumes: Task 1 的 `LayerLadder`。
- Produces：
  - `public static readonly AttachedProperty<bool> IsSurfaceHostProperty`（owner `LayerLadder`，host `VisualLayerManager`，默认 false），以及 `public static bool GetIsSurfaceHost(VisualLayerManager)` / `public static void SetIsSurfaceHost(VisualLayerManager, bool)`。设为 public 是因为 Desktop、GalleryBase 的 AXAML 要跨程序集设置它；需要 XML doc，并写入 changelog。
  - `internal static VisualLayerManager? FindContentLayerManager(Visual visual)`：最近的 `VisualLayerManager` 祖先（含表面宿主），找不到时退回 TopLevel 下第一个。
  - `internal static VisualLayerManager? FindTopLevelLayerManager(Visual visual)`：最近的非表面宿主 `VisualLayerManager` 祖先，找不到时退回 TopLevel 下第一个非表面宿主。

- [ ] **Step 1: 写失败测试** `LayerResolutionTests`（用代码构造一个 `new VisualLayerManager { Child = inner }`，`SetIsSurfaceHost(true)`，放进 Window 的一个 `ScopeAwareAdornerLayer` 中模拟提升表面）：
  - `Tour_Layer_Skips_Surface_Hosts`：`TourLayer.GetTourLayer(inner)` 的父级是 Window 的 `PART_VisualLayerManager`。
  - `Feedback_Layer_Skips_Surface_Hosts`：`WindowFeedbackLayer.GetLayer(window)` 不落在表面宿主。
  - `Content_Roles_Resolve_Into_The_Surface_Host`：`ScopeAwareOverlayLayer.GetLayer(inner)` 与 `ScopeAwareAdornerLayer.GetLayer(inner)` 的宿主是表面宿主，且号码为 −800 / −900。
  - `Unmarked_Nested_Manager_Remains_A_Tour_Scope`：未标记的嵌套 `VisualLayerManager`（模拟 Gallery Tour 舞台）中，`TourLayer.GetTourLayer` 仍落在该嵌套宿主。
- [ ] **Step 2: 运行确认失败**：filter `~Temporary.layer_ladder.LayerResolutionTests`，Expected：`Tour_Layer_Skips_Surface_Hosts` 失败（今天落在表面宿主）。
- [ ] **Step 3: 实现**：加入附加属性与两个查找方法；内容角色解析器（`ScopeAwareAdornerLayer.GetLayer` 中的 VLM 回退、`ScopeAwareOverlayLayer` 的 VLM 回退）改用 `FindContentLayerManager`；`TourLayer`、`WindowFeedbackLayer` 改用 `FindTopLevelLayerManager`。
- [ ] **Step 4: 运行确认通过**，并重跑 Task 1 的 filter 与 Step 5 回归 filter。
- [ ] **Step 5: 准备提交信息草稿**（`feat(VisualLayers): resolve content and top-level roles through surface hosts`），不提交。

### Task 3: T5 / T7 呈现层与 Drawer、作用域 Dialog 路由

**Files:**
- Create: `src/AtomUI.Controls/Primitives/VisualLayers/ContentPresentationLayer.cs`
- Create: `src/AtomUI.Controls/Primitives/VisualLayers/TopLevelPresentationLayer.cs`
- Modify: `src/AtomUI.Controls/Primitives/VisualLayers/ScopeAwareAdornerLayer.cs`（`GetLayer` 中"包含层"一步只匹配精确类型；`FindAdornerLayer` 只返回精确类型）
- Modify: `src/AtomUI.Desktop.Controls/Drawer/Drawer.cs:354-360,405-460,530-590`
- Modify: `src/AtomUI.Desktop.Controls/Dialog/OverlayHost/DialogOverlayLayer.cs:40-62`（`ResolveHostLayer`）
- Test: `tests/AtomUI.Desktop.Controls.Tests/Temporary/layer-ladder/PresentationRoutingTests.cs`

**Interfaces:**
- Consumes: Task 1 `LayerLadder.AddLayer` / 号码；Task 2 `FindContentLayerManager` / `FindTopLevelLayerManager`。
- Produces（都在 `AtomUI.Controls.Primitives`，`internal sealed`，继承 `ScopeAwareAdornerLayer`）：
  - `ContentPresentationLayer` 提供两个解析入口，对应 spec 5.3 的两种作用域：
    - `internal static ContentPresentationLayer? GetLayer(Visual visual)`（Drawer 用）：落在 `ScopeAwareAdornerLayer.GetLayer` 会解析出的同一个宿主（滚动内容包裹面板 / 内容 `VisualLayerManager`），随滚动移动；
    - `internal static ContentPresentationLayer? GetViewportLayer(Visual visual)`（作用域 Dialog 用）：落在 `ScopeAwareOverlayLayer.GetLayer` 会解析出的同一个宿主（`ScopeAwareOverlayLayerPanel#PART_ContentHost` / 内容 `VisualLayerManager`），固定在视口，位于同宿主的 FloatButton 之上。

    两者都经 `LayerLadder.AddLayer(host, layer, LayerLadder.ContentPresentation)` 注入，每个宿主最多一个实例。
  - `TopLevelPresentationLayer`：`internal static TopLevelPresentationLayer? GetLayer(TopLevel topLevel)`，注入到 `FindTopLevelLayerManager(topLevel)`，号码 `TopLevelPresentation`。

- [ ] **Step 1: 写失败测试** `PresentationRoutingTests`：
  - `TopLevel_Drawer_Opens_In_TopLevel_Presentation_Layer`：Window 内无父 Drawer、`OpenOn` 自动绑定为 TopLevel 的 Drawer 打开后，container 的父级是 `TopLevelPresentationLayer`，`ZIndex == int.MaxValue-500`。
  - `Scoped_Drawer_Opens_In_Content_Presentation_Layer`：`OpenOn` 指向局部 Border 的 Drawer → 父级是 `ContentPresentationLayer`（−700）；ScrollViewer 中局部 target 的 Drawer → `ContentPresentationLayer` 位于滚动内容包裹面板内。
  - `Nested_Drawer_Reuses_The_Parent_Layer`：TopLevel Drawer 内再开子 Drawer，子 container 与父 container 同层。
  - `OpenOn_Migration_Moves_Between_Presentation_Layers`（Review Focus 3）：打开状态下把 `OpenOn` 从 Window 改为局部元素 → container 父级变为 `ContentPresentationLayer`，`TopLevelPresentationLayer` 不再包含它，Window 的 drawn chrome 抑制已释放（`IsDrawnChromeOverlayVisible == true`）。
  - `Popup_Root_TopLevel_Drawer_Uses_Its_Own_Presentation_Layer`（Review Focus 2）：原生 Popup 内容里的无父 Drawer 打开后，落在该 PopupRoot 宿主的 `TopLevelPresentationLayer`，且所属 Window 的 `IsDrawnChromeOverlayVisible` 不受影响。
  - `Scoped_Dialog_Uses_Content_Presentation_Layer`：`OverlayScope` 指向 ScrollViewer 内元素的 Dialog，其 `DialogOverlayLayer` 位于该 ScrollViewer `PART_ContentHost` 中的 `ContentPresentationLayer`，铺满视口（Bounds 等于 `PART_ContentHost` 的 Bounds），且该 `ContentPresentationLayer` 的 ZIndex 高于同面板中的 `ScopeAwareOverlayLayer`。
  - `Watermark_Inside_A_Presentation_Layer_Does_Not_Join_It`：TopLevel Drawer 内容中的 Watermark，其层不是 `TopLevelPresentationLayer`。
- [ ] **Step 2: 运行确认失败**：filter `~Temporary.layer_ladder.PresentationRoutingTests`。
- [ ] **Step 3: 实现两个呈现层**；`ScopeAwareAdornerLayer` 的包含层匹配与 `FindAdornerLayer` 改为精确类型（`layer.GetType() == typeof(ScopeAwareAdornerLayer)`）。
- [ ] **Step 4: 实现 Drawer 路由**：抽出 `private ScopeAwareAdornerLayer? ResolvePresentationLayer(Control? openOn)`。规则：`openOn is TopLevel topLevel` → `TopLevelPresentationLayer.GetLayer(topLevel)`；否则 → `ContentPresentationLayer.GetLayer(openOn ?? this)`；位于父 Drawer 内（`FindParentDrawer() != null`）→ 复用父 container 所在层。`Open`、`Close` 的回退分支、`OnPropertyChanged(OpenOnProperty)` 都改用它。把 `GetType() != typeof(ScopeAwareAdornerLayer)` 的四处精确比较（354-360、534、571、588 附近）改为 `is not ScopeAwareAdornerLayer`。
- [ ] **Step 5: 实现 Dialog 路由**：`ResolveHostLayer` 中的 `ScopeAwareOverlayLayer.GetLayer(overlayScope)` 和最后的 fallback 改为 `ContentPresentationLayer.GetViewportLayer(...)`。如果 `DialogOverlayLayer` 在 Canvas 派生层中无法铺满作用域，按 spec 5.8 让 `ContentPresentationLayer` 内部组合"Drawer 子层 + Dialog 子层"，对外接口不变。
- [ ] **Step 6: 运行确认通过**，再跑回归：`--filter "FullyQualifiedName~Drawer|FullyQualifiedName~Dialog|FullyQualifiedName~MessageBox"` 与 Task 1 的回归 filter。Expected：PASS；现有 `DrawerScopeLayerTrackingTests` 中断言父层为 `ScopeAwareAdornerLayer` 的测试，以 `is ScopeAwareAdornerLayer` 判断仍成立。
- [ ] **Step 7: 准备提交信息草稿**（`feat(Drawer,Dialog): route presentations to the T5 and T7 layers`），不提交。

### Task 4: 提升表面的表面宿主

**Files:**
- Modify: `src/AtomUI.Desktop.Controls/Drawer/Themes/DrawerContainerTheme.axaml`（模板根包一层 `VisualLayerManager atom:LayerLadder.IsSurfaceHost="True"`）
- Modify: `src/AtomUI.Desktop.Controls/Dialog/Themes/OverlayDialogPresenterTheme.axaml`（同上）
- Modify: `src/AtomUI.Desktop.Controls/Message/Themes/WindowMessageManagerTheme.axaml`、`src/AtomUI.Desktop.Controls/Notifications/Themes/WindowNotificationManagerTheme.axaml`（同上）
- Modify: `src/AtomUI.Controls/FloatButton/AbstractFloatButtonHost.cs:217-221`、`src/AtomUI.Desktop.Controls/FloatButton/FloatButtonGroupHost.cs:160-164`：浮动控件以代码创建的表面宿主包裹后再加入层。注释写明 ControlTheme 不可行的原因：浮动控件由宿主在运行时移入另一个视觉根的层，FloatButton 自身模板还用于内联场景。
- Modify: `src/AtomUI.Toolkits.GalleryBase/Controls/Themes/GalleryStickyTabsHostTheme.axaml`（`PART_StickyContentHost` 内容包一层表面宿主）
- Test: `tests/AtomUI.Desktop.Controls.Tests/Temporary/layer-ladder/SurfaceHostDecorationTests.cs`

**Interfaces:**
- Consumes: Task 2 `LayerLadder.IsSurfaceHostProperty`；Task 3 呈现层。
- Produces：每个提升表面的视觉根下有且只有一个表面宿主。

- [ ] **Step 1: 记录修复前证据**（spec 第 10 节第一步）：先不改主题，写 `Drawer_Focus_Outline_And_Badge_Land_On_The_Drawer_Surface`：TopLevel Drawer 内放一个 Button 和一个 DotBadge 装饰目标，`AdornerLayer.GetAdornerLayer(button)` 的父级应该是 Drawer 的表面宿主。运行后记录"修复前它是 Window 的 `PART_VisualLayerManager`，其 ZIndex 低于 Drawer 所在层"，结论写入 Task 7 的交付记录。
- [ ] **Step 2: 补齐其余失败测试**：
  - `Every_Elevated_Surface_Owns_A_Surface_Host`：分别打开 Drawer、作用域 Dialog、Message、Notification、FloatButton（浮动模式），对其表面内的一个 Button 断言 `AdornerLayer.GetAdornerLayer(button)` 的父级是带 `IsSurfaceHost == true` 的 `VisualLayerManager`，且该宿主位于对应的角色层之内。
  - `Overlay_Popup_From_A_Surface_Lands_On_The_TopLevel_Popup_Layer`（Review Focus 4）：Drawer 内一个 `ShouldUseOverlayLayer = true` 的 Popup 打开后，其宿主位于 Window `PART_VisualLayerManager` 的 `PopupOverlayLayer`。
  - `Dialog_From_A_Surface_Uses_The_TopLevel_Overlay_Layer`：Drawer 内按钮调用 `OverlayLayer.GetOverlayLayer` 得到 Window 的 OverlayLayer。
- [ ] **Step 3: 运行确认失败**：filter `~Temporary.layer_ladder.SurfaceHostDecorationTests`。
- [ ] **Step 4: 按 Files 列表加入表面宿主。**
- [ ] **Step 5: 运行确认通过**，再跑回归 `--filter "FullyQualifiedName~Drawer|FullyQualifiedName~Dialog|FullyQualifiedName~Message|FullyQualifiedName~Notifications|FullyQualifiedName~FloatButton|FullyQualifiedName~SemanticParts|FullyQualifiedName~FeedbackStack"`，以及 `dotnet test tests/AtomUI.Toolkits.GalleryBase.Tests/AtomUI.Toolkits.GalleryBase.Tests.csproj --framework net10.0 --no-restore`。Semantic Part 的 SelectorRoute 如果因多了一层 `VisualLayerManager` 而失配，按 AGENTS.md 的 Semantic PART 约束修正 route，并补命中测试。
- [ ] **Step 6: 性能确认**：用一个临时测试连续推入 50 条 Message，记录推入前后 `Stopwatch` 耗时，与本任务之前的基线对比，偏差在噪声范围内即可。结论写入交付记录，测试在 Task 7 删除。
- [ ] **Step 7: 准备提交信息草稿**（`feat(Primitives): give elevated surfaces their own surface host`），不提交。

### Task 5: 窗口 chrome 层与全屏标题栏迁移

**Files:**
- Create: `src/AtomUI.Desktop.Controls/Window/Utils/WindowChromeLayer.cs`
- Modify: `src/AtomUI.Desktop.Controls/Window/Window.cs:1115-1122`（`OnApplyTemplate`）
- Modify: `src/AtomUI.Desktop.Controls/Window/Window.FullScreenTitleBar.cs:68-100`（`AttachFullScreenTitleBarHost` / `DetachFullScreenTitleBar`）
- Test: `tests/AtomUI.Desktop.Controls.Tests/Temporary/layer-ladder/WindowChromeLayerTests.cs`

**Interfaces:**
- Consumes: Task 1 `LayerLadder.AddLayer` / `WindowChrome`；Task 2 表面宿主标记。
- Produces：`internal sealed class WindowChromeLayer : Panel`（`IsHitTestVisible` 只对自身背景为 false，子项可命中），持有一个 `IsSurfaceHost = true` 的 `VisualLayerManager`。`internal void Host(FullScreenTitleBarHost host)` / `internal void Release()` 负责把模板声明的宿主移入或移出。

- [ ] **Step 1: 先做 `TemplateBinding` 可行性探针**（spec 5.6）：把模板中的 `PART_FullScreenTitleBarHost` 从原父级移到另一个容器后，修改 `Window.FullScreenTitleBarMode`，断言 `host.Mode` 随之变化。若不成立，改走 spec 5.6 的代码绑定退路（Step 4 的实现随之调整，并同步更新标题栏设计文档的 6.4）。
- [ ] **Step 2: 写失败测试** `WindowChromeLayerTests`：
  - `Full_Screen_Title_Bar_Sits_Above_Content_Roles`：Window 全屏 + AutoHide，在窗口级 `ScopeAwareAdornerLayer` 顶部放一个 600×60 的 Border（模拟吸顶 Tab），在内容顶部放一个带 DotBadge 的元素，在窗口级 `ScopeAwareOverlayLayer` 放一个 FloatButton 覆盖顶部；设 `IsFullScreenTitleBarOpen = true`，`window.InputHitTest(new Point(300, 10))` 命中元素的祖先包含 `FullScreenTitleBarHost`。
  - `Window_Roles_Sit_Above_The_Chrome`：在同一窗口打开 TopLevel Drawer，(300,10) 命中 Drawer container；关闭后打开 Dialog，命中 Dialog 遮罩。
  - `Message_Sits_Above_Everything`（Review Focus 5）：`FullScreenTitleBarMode = Pinned`，TopLevel Drawer 打开，弹出 Message，在 Message 卡片中心命中卡片。
  - `Title_Bar_Focus_Outline_Uses_The_Chrome_Surface_Host`：`AdornerLayer.GetAdornerLayer(标题栏内一个按钮)` 的父级是 chrome 层内的表面宿主。
  - `Title_Bar_Surface_Keeps_Its_Window_Position`：全屏打开后，`PART_Surface.TranslatePoint(0,0, window).Y` 等于迁移前的值。迁移前的值在 Step 1 之前用一次临时运行记录下来，写进断言常量，并注明来源。
  - `Template_Reapply_Keeps_A_Single_Chrome_Layer`（Review Focus 1）：翻转 `IsCsdEnabled` 后，新 `PART_VisualLayerManager` 中恰好一个 `WindowChromeLayer`，其中的宿主就是新模板的 `PART_FullScreenTitleBarHost`；旧宿主不再处于任何 `WindowChromeLayer` 中。
- [ ] **Step 3: 运行确认失败**：filter `~Temporary.layer_ladder.WindowChromeLayerTests`。Expected：`Full_Screen_Title_Bar_Sits_Above_Content_Roles` 命中吸顶 Border（复现原 bug）。
- [ ] **Step 4: 实现**：`OnApplyTemplate` 中释放旧 chrome 层，创建新层，经 `LayerLadder.AddLayer(vlm, layer, LayerLadder.WindowChrome)` 注入，再 `layer.Host(host)`；`DetachFullScreenTitleBar` 与窗口关闭时调用 `Release` 并移除层。宿主的几何如果因不再受 `OffScreenMargin` 或模板边距容器约束而偏移，在 `WindowChromeLayer` 上用同一份 Window 属性还原（例如绑定 `Margin` 到 `OffScreenMargin` / `FrameShadowThickness`），以 Step 2 的位置测试为准。
- [ ] **Step 5: 运行确认通过**，再跑回归 `--filter "FullyQualifiedName~Window"`（含现有的 `WindowFullScreenTitleBarTests`、`FullScreenTitleBarRevealControllerTests`）以及 `dotnet test tests/AtomUI.Desktop.Controls.Rendering.Tests/AtomUI.Desktop.Controls.Rendering.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~Window"`。
- [ ] **Step 6: 准备提交信息草稿**（`fix(Window): host the full-screen title bar in the window chrome layer`），不提交。

### Task 6: 文档与 changelog

**Files:**
- Modify: `docs/architecture/systems/rendering/visual-layers.md`
- Modify: `docs/controls/desktop/window/window/full-screen-title-bar-design.md`（6.1、6.4、第 10 节）
- Modify: Drawer、Dialog/Modal、FloatButton、Tour、Message、Notification、Watermark、Window 的 `implementation.md` 中涉及层归属的段落（`docs/controls/desktop/**`；Watermark 在 Controls 文档树下，以实际路径为准）
- Modify: 上述控件与 Window 的 `changelog.md`

- [ ] **Step 1: 重写 `visual-layers.md`**：五条规则、5.2 号码表、5.3 宿主矩阵、5.4 消费者归属表、表面宿主章节、选择规则表、测试要求；删除"`ScopeAwareAdornerLayer` 承载 Drawer""`TourLayer` −98"等旧描述。
- [ ] **Step 2: 更新标题栏设计文档**：宿主改为位于 `WindowChromeLayer`（T6）的表面宿主内；6.4 写明替换 Window ControlTheme 时仍需保留 `PART_FullScreenTitleBarHost`（由 Window 在运行时迁入 chrome 层）。
- [ ] **Step 3: 更新各控件的 implementation 段落与 changelog**：按 `docs/engineering/contributing/changelog-guidelines.md` 的格式写；Tour changelog 写明 spec 第 7 节第 9 项；`AtomUI.Controls` 层面的 changelog 写明 `AdornerLayer` 号码变化、T3/T4 对调、`LayerLadder.IsSurfaceHost` 新 public 附加属性，以及给 Mobile 的迁移说明（spec 第 6 节）。
- [ ] **Step 4: `git diff --check`**，Expected：无输出。
- [ ] **Step 5: 准备提交信息草稿**（`docs(VisualLayers): document the global layer ladder`），不提交。

### Task 7: 实机验证、测试处置、清理与最终模块验证

- [ ] **Step 1: 实机复现对比**：用 Windows 实机截帧脚本（会话临时目录中的 `vis.ps1`），对修复后的 Gallery 复现原场景：进入全屏 → 布局 / Space → 滚动 8 格 → 鼠标移到顶部，截取顶部条带。Expected：标题栏在全宽可见，吸顶 Tab 被标题栏盖住。再各截一组：Drawer 内按钮的 Tab 焦点描边可见；FloatButton 盖住内容的 Badge。截图只放临时目录，结论写入交付记录。macOS / Linux 标注"未实测"。
- [ ] **Step 2: 测试处置**：逐个按 test-value-and-lifecycle 第 2 节评估。预期只保留一个常驻测试：`LayerLadderOrderTests` 的宿主排序守护（守护"Avalonia 不回写 ZIndex"与跨宿主一致性），并入 `tests/AtomUI.Desktop.Controls.Tests/Primitives/`，四项准入理由写入交付记录。其余全部删除，包括 Task 4 的性能临时测试。
- [ ] **Step 3: 清理**：删除 `tests/AtomUI.Desktop.Controls.Tests/Temporary/layer-ladder/` 及其专用 helper；关闭本任务启动的 Gallery 进程。
- [ ] **Step 4: 最终模块验证**（在清理后的工作树上）：
  - `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~Window|FullyQualifiedName~Drawer|FullyQualifiedName~Dialog|FullyQualifiedName~MessageBox|FullyQualifiedName~Message|FullyQualifiedName~Notifications|FullyQualifiedName~FloatButton|FullyQualifiedName~Tour|FullyQualifiedName~Primitives|FullyQualifiedName~SemanticParts|FullyQualifiedName~FeedbackStack|FullyQualifiedName~ScrollViewer"`
  - `dotnet test tests/AtomUI.Controls.Tests/AtomUI.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~Watermark|FullyQualifiedName~Badge"`
  - `dotnet test tests/AtomUI.Toolkits.GalleryBase.Tests/AtomUI.Toolkits.GalleryBase.Tests.csproj --framework net10.0 --no-restore`
  - `dotnet test tests/AtomUI.Desktop.Controls.Rendering.Tests/AtomUI.Desktop.Controls.Rendering.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~Window"`
  - `git diff --check`

  Expected：全部 PASS，`git diff --check` 无输出。
- [ ] **Step 5: 交付记录**：写入 `docs/developments/progress/2026-11-22-global-layer-ladder.md`（只写文字，不放媒体），内容包括修复前后证据结论、测试处置与保留理由、最终模块验证结果、未实测平台。准备整体提交信息草稿，交用户审查，**不提交**。
