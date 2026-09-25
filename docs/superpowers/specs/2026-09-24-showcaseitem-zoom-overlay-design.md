# ShowCaseItem 放大 Overlay 设计

> 状态：2026-09-24 方案已获用户确认，待按实施计划执行。
>
> 证据来源：用户需求描述与截图（标题行红框）、`ShowCaseItem`/`ShowCaseItemTheme` 现状、
> `WindowTheme.axaml` 标题栏与内容区布局、`GalleryShellView` 根布局、
> `GalleryShowCaseCodeDrawerHost` 路由事件宿主先例、`ImagePreviewerOverlayHost` overlay
> reparent/关闭/detach 清理先例。

## 1. 结论

为 Gallery `ShowCaseItem` 增加放大功能：卡片标题行（现有 `PART_ShowSourceButton` 旁）新增
`PART_ZoomButton` 图标按钮；点击后该卡片的演示内容以 overlay 方式覆盖主窗口**标题栏以下的全部
区域**（含侧边导航栏）；特定示例可通过 `IsZoomEnabled=false` 关闭该功能（按钮不出现）。

核心决策：

- **宿主方案**：在 `GalleryShellView` 根 Grid（`WorkspaceRootLayout`）追加顶层子控件
  `ShowCaseZoomOverlayHost`。`GalleryShellView` 恰好填满标题栏以下全部区域，该宿主天然精确
  覆盖目标区域，零标题栏高度追踪，宿主应用零改动，功能完全落在 GalleryBase 工具包内。
- **内容呈现**：内容迁移（reparent）。点击时把 `ShowCaseItem.Content` 的实际视觉子树移动到
  overlay 舞台区，关闭时原样移回；控件实例不变，演示状态（输入、选中态、动画、绑定）完整
  保留并无损还原。
- **通信机制**：复刻 `SourceCodeRequested` 的路由事件宿主模式——`ShowCaseItem` 冒泡
  `ZoomRequested`，Shell 级 `ShowCaseZoomOverlayHost` 截获并驱动 overlay。

## 2. 已验证现状

| 事实 | 位置 |
|---|---|
| `ShowCaseItem : ContentControl`，标题行 `Grid ColumnDefinitions="*,Auto"`，代码按钮走「PART 命名 + internal 可见性属性 + 类级 `Button.ClickEvent` 处理」模式 | `Controls/ShowCaseItem.axaml.cs`、`Controls/Themes/ShowCaseItemTheme.axaml` |
| 窗口模板 `VisualLayerManager → DockPanel → TitleBarPanel(Dock=Top) + 内容区`；内容区即 `WorkspaceWindow` 的 Grid → `GalleryShellView`（侧栏 + 内容宿主），恰好填满标题栏以下区域 | `WindowTheme.axaml`、`WorkspaceWindow.axaml`、`Shell/GalleryShellView.cs` |
| Shell 级服务响应 showcase 路由事件的既有模式（`SourceCodeRequested` → Drawer） | `Controls/GalleryShowCaseCodeDrawerHost.cs` |
| 同窗口 overlay 注入先例（reparent、Escape 关闭、焦点管理、detach 清理） | `ImagePreviewerOverlayHost.cs`、`AbstractImagePreviewer.OpenOverlayDialog` |
| Token 体系：`[ControlDesignToken]` 从 GlobalToken 派生，AXAML 用 `gallery:XxxTokenResource` 引用 | `Controls/ShowCaseItemToken.cs` |
| 图标：`ExpandOutlined`（放大）、`ShrinkOutlined`（关闭）已在 `AtomUI.Icons.AntDesign` 生成 | `src/AtomUI.Icons.AntDesign/GeneratedIcons/` |

## 3. 方案选型

### 3.1 宿主方案（已选定 A）

**A. Shell 内嵌 Zoom Overlay Host（采用）**：`ShowCaseZoomOverlayHost` 作为
`GalleryShellView` 根 Grid 最后一个子元素。窗口最大化、macOS 原生全屏、`IsTitleBarVisible`
切换均自动正确；TopLevel 级弹层（源码 Drawer、Flyout、通知）仍在它之上，放大期间可继续拉出
源码抽屉，符合期望。

**B. TopLevel OverlayLayer 注入（否决）**：需用 `Canvas.SetTop = 标题栏高度` 定位并持续追踪
标题栏/窗口尺寸变化；标题栏高度受 token、最大化、CSD/非 CSD、可见性开关多因素影响，追踪脆弱，
且把 GalleryBase 功能耦合到 Desktop.Controls 窗口模板细节。

**C. 独立原生窗口（否决）**：违背「保留主窗口标题栏、覆盖其下区域」的需求，内容跨窗口迁移
的状态与焦点管理成本最高。

### 3.2 内容呈现策略（已选定 reparent）

**采用内容迁移（reparent）**：卡片背后空洞由 overlay 蒙层遮盖；迁移时显式同步 `DataContext`；
`IsDeferredContentEnabled` 且未物化时先 `MaterializeDeferredContent()`。

**否决「就地撑满/重模板」**：卡片处于面板布局与 ScrollViewer 内，逃逸布局容器实质仍需
overlay 协作，却引入两套布局状态机，复杂度更高。

## 4. 组件设计

全部新增组件位于 `src/AtomUI.Toolkits.GalleryBase/Controls`。

### 4.1 ShowCaseItem 扩展（改现有文件）

```csharp
public static readonly StyledProperty<bool> IsZoomEnabledProperty;   // 默认 true
public static readonly RoutedEvent<ShowCaseZoomRequestedEventArgs> ZoomRequestedEvent; // Bubble
internal static readonly StyledProperty<bool> IsZoomActionVisibleProperty; // IsZoomEnabled && 全局配置
```

处理逻辑复刻 `PART_ShowSourceButton` 模式：类级 `AddHandler(Button.ClickEvent, ...)`，命中
`PART_ZoomButton` 时校验开关 → 物化延迟内容 → `RaiseEvent(ZoomRequested)` 并 `e.Handled = true`。

### 4.2 ShowCaseZoomRequestedEventArgs

携带 `Title`、`Description`、发起的 `ShowCaseItem` 引用。

### 4.3 ShowCaseZoomOverlayHost : ContentControl

挂在 `GalleryShellView` 根 Grid 最后一个子位置：

- `AddHandler(ShowCaseItem.ZoomRequestedEvent, ...)` 接管请求；
- 持有单例 `ShowCaseZoomOverlay`，记录源 item 与被迁出的内容引用；
- `RestoreContent()` 是唯一归还出口，三条路径全部收敛到它：
  1. 用户关闭（关闭按钮 / Escape）；
  2. 源 item `DetachedFromVisualTree`（路由切换页面时强制归还，参照 ImagePreviewer 对
     placementTarget 的 detach 订阅）；
  3. shell `Dispose`；
- 重入保护：overlay 打开期间蒙层阻断输入。

### 4.4 ShowCaseZoomOverlay : TemplatedControl

```text
Border PART_RootBorder（兜底背景，正常态不可见）
└── Border PART_CardBorder（贴边铺满标题栏以下区域：ColorBgContainer 背景、无边距/圆角/阴影）
    └── DockPanel（CardPadding 大内边距：上 48、左右 40、下 32）
        ├── 标题行（Dock=Top）：Separator(Title, TitlePosition=Center) …… IconButton PART_CloseButton（ShrinkOutlined）
        ├── 描述行（Dock=Bottom）：PART_DescriptionText（有内容时可见）
        └── Grid PART_StageRoot
            └── ShowCaseZoomStagePanel（StageMeasureWidth 模板绑定）
                └── ContentPresenter PART_StagePresenter
```

舞台行为（2026-09-24 第六轮反馈后终态，用户指令"移除缩放机制"）：**零缩放保真居中**。
`ShowCaseZoomStagePanel` 以捕获的卡片内布局宽度 `StageMeasureWidth`（未捕获时以舞台宽度）测量内容
（高度无限），按期望尺寸水平垂直居中——**不做任何拉伸，也不做任何 RenderTransform 缩放**（模板中
不存在缩放包裹层）。放大态内容与卡片内的布局、间距、控件尺寸完全一致。

历史教训链（同类问题六次迭代，缩放机制两进两出）：ScrollViewer 无限测量废对齐（v1）→ 无上限
fit-contain 缩放文字 4 倍失真（v2）→ 舞台宽测量 + fillsWidth 拉伸破坏间距（v3）→ 冻结捕获宽度导致
固定尺寸示例留白随窗口膨胀（v3.1）→ 上限 1.5× 等比放大对单个控件读作渲染 bug（v5）→ **终态 v6：
内容永不缩放永不拉伸**。结论：控件尺寸是系统常量，"放大"只提供更大画布与居中呈现，绝不改变内容
本身的视觉尺寸；对空旷感的取舍由用户拍板（当前接受留白）。

- `Focusable = true`，打开时聚焦；`KeyDown(Escape)` 关闭（Tunnel，参照
  `ImagePreviewerOverlayHost.HandleDialogKeyDown`）；
- 打开/关闭动效：淡入 + 轻微缩放（0.96→1），`Transitions` 声明在 ControlTheme 的 Style
  Setter 中（主题绑定优先约束），联动全局 Motion 开关，关闭动效结束后再归还内容。

### 4.5 数据流

```text
[卡片] PART_ZoomButton 点击
   ↓ (ShowCaseItem 类级 Click 处理，同 ShowSourceButton 模式)
校验 IsZoomEnabled → MaterializeDeferredContent()（若需要）
   ↓ RaiseEvent(ZoomRequested) 冒泡
[Shell] ShowCaseZoomOverlayHost 截获
   ↓ 记录源 item → 暂存内容引用 → item.Content 置空
   ↓ overlay.DataContext = 原 DataContext；Stage.Content = 内容；显示 overlay
[用户] 关闭按钮 / Escape
   ↓ RestoreContent()：Stage.Content 清空 → item.Content 还原（引用相等）→ 隐藏 overlay
```

## 5. 主题与 Token

- 新增 `ShowCaseZoomOverlayToken : AbstractControlDesignToken`（`[ControlDesignToken]`）：
  蒙层颜色（明/暗分别取 `ColorBgLayout`/加深值）、舞台水平留白、舞台最大宽度比例、圆角
  `BorderRadiusLG`、阴影 `BoxShadowsTertiary`（与卡片一致）、标题字重等，全部从
  `EffectiveGlobalToken` 派生；
- 新增 `Themes/ShowCaseZoomOverlayTheme.axaml`，注册进 GalleryBase 的 ControlTheme 提供者；
- `ShowCaseItemTheme.axaml` 两份模板（普通 + RibbonBadge 版）标题行改为 `*,Auto,Auto`，插入
  与代码按钮同规格的 `PART_ZoomButton`（IconButton 28×28，`ExpandOutlined`），可见性绑定
  `IsZoomActionVisible`；
- 全局开关：`GalleryBaseConfiguration` 增加 `ShowCaseZoomOptions { IsEnabled = true }`，对齐
  `SourceCodeDisplay` 的配置模式。

## 6. 边界与风险

1. **页面级样式丢失（2026-09-24 第七轮反馈修复）**：示例样式常声明在页面级
   `UserControl.Styles`（如 Grid 展示页的格子背景/文字色）。reparent 进 overlay 后内容脱离页面
   逻辑树，页面级 Styles/Resources 停止命中（Grid 示例格子只剩深色文字、蓝色背景全丢）。修复：
   迁移前先 `ISetLogicalParent.SetParent(item)` 恢复对原卡片的逻辑挂载，再挂入舞台
   （`ImagePreviewerOverlayHost` 先例；面板集合只认领无父子级，顺序必须"先挂载后入面板"）；
   舞台内容直接由 `PART_StagePanel`（普通 Panel）承载而非 ContentPresenter（presenter 会认领
   逻辑子级，与该机制冲突）。归还时先 `SetParent(null)` 再回填 `item.Content`。
   测试：`ZoomRequest_Keeps_Page_Scope_Styles_Applied_To_The_Content`。

1. **钉住页签的 z-order 冲突（2026-09-24 第四轮反馈修复）**：`GalleryStickyTabsHost` 钉住时把页签宿主
   提升到窗口级 `ScopeAwareAdornerLayer`（ZIndex = int.MaxValue-99，渲染于所有内容之上），而放大
   overlay 在内容树内——不处理则钉住的页签会浮在放大视图之上。契约：放大打开期间挂起粘滞提升
   （`GalleryStickyTabsHost.IsStickyElevationSuppressed`，已提升的降级回面板），关闭后由粘滞机制
   自动重新提升。测试：`ShowCaseZoomSuppressesStickyElevationTests`。

1. **页面路由切换**：源 item detach → 强制归还并关闭；内容随后随页面树正常销毁，无泄漏。
2. **资源生命周期**：token 引用一律走 `TokenResourceBinder`/markup 既有模式，避免
   DynamicResource 泄漏。
3. **主题/明暗切换在放大期间发生**：overlay 资源随主题系统自动刷新，无特殊处理。
4. **AOT**：全部编译型 XAML + 强类型代码，无反射路径。
5. **`IsFake` 项**：不显示放大按钮（模板样式联动）。
6. **焦点还原**：关闭后焦点回到源卡片按钮（P2）。
7. **放大期间的「查看源码」入口**：overlay 标题行复用源按钮并转发源 item 的
   `SourceCodeRequested`（P2，第一期不做）。
8. **共享元素缩放动画**（卡片→舞台连续 transform）：P2 后续增强，第一期仅 fade+scale。

## 7. 测试与验收

**单测（TDD）— `tests/AtomUI.Toolkits.GalleryBase.Tests/Controls/`**：

- `IsZoomEnabled=false` → 按钮不可见、事件不触发；默认 → 点击触发 `ZoomRequested` 且携带
  Title；
- 延迟内容未物化时点击 → 物化后再迁移；
- Host 收到事件 → overlay 打开且舞台内容与原内容引用相等；关闭 → 归还后 `item.Content`
  引用相等；
- 源 item detach → 自动归还并关闭；
- Token 注册与默认值测试。

**AtomUIGallery.Tests**：挑一个现有 showcase 页做 headless 集成断言（按钮存在 + overlay 行为）。

**视觉验收**：实现完成后产出书面步骤（含示例入口与分类路径，例如「组件页 → Card 卡片分类 →
典型卡片示例 → 标题行右侧图标按钮」），由用户截图/录屏回传确认；截图不入库，结论仅留文字
记录；headless 通过不等于通过，以真机回传为准。

## 8. 改动文件清单与实施顺序

| 顺序 | 文件 | 动作 |
|---|---|---|
| 1 | `Controls/ShowCaseZoomOverlayToken.cs` | 新增 |
| 2 | `Controls/ShowCaseZoomOverlay.cs`、`ShowCaseZoomOverlayHost.cs`、`ShowCaseZoomRequestedEventArgs.cs` | 新增 |
| 3 | `Controls/Themes/ShowCaseZoomOverlayTheme.axaml` | 新增（含注册） |
| 4 | `Controls/ShowCaseItem.axaml.cs` + `Themes/ShowCaseItemTheme.axaml` | 扩展属性/事件/按钮（两份模板） |
| 5 | `Shell/GalleryShellView.cs` | 根 Grid 挂 host |
| 6 | `Configuration/GalleryBaseConfiguration.cs` | 全局开关 |
| 7 | `tests/AtomUI.Toolkits.GalleryBase.Tests`、`tests/AtomUIGallery.Tests` | 测试 |
| 8 | 视觉验收步骤文档 | 交用户执行 |

## 9. 非目标

- 不改动 `ShowCasePanel`/`ShowCaseMasonryPanel` 布局逻辑。
- 不改动 `Window`/`WindowTitleBar` 模板与标题栏行为。
- 不引入独立原生窗口方案。
- 不在本期实现共享元素缩放动画、放大期间源码按钮、焦点还原（列为 P2）。
