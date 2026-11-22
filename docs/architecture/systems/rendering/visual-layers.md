# AtomUI 视觉层规范

本文档定义 AtomUI 中跨普通视觉树绘制的 layer、overlay、adorner 的职责边界和使用规则。这里的“层”指会把内容放到普通控件布局流之外的宿主容器或窗口级覆盖区域，不包含普通 `Panel`、`Border.BoxShadow`、模板内部装饰容器等常规视觉结构。

设计依据与决策记录见 [全局视觉层级阶梯设计](../../../superpowers/specs/2026-11-22-window-layer-ladder-design.md)。

## 总体原则

- 先判断覆盖内容的**角色**，再选择层：层按角色而不是按类型选择，一个层类只承担一种角色。
- 不允许用子元素 `ZIndex` 解决跨父层遮挡问题。不同父层之间的遮挡必须在父层职责和层级顺序上解决。
- 不允许把局部装饰迁到窗口级层。Badge、拖拽预览、水印、Drawer、sticky mirror 等都有不同的锚定和裁剪语义。
- 新增跨视觉树覆盖能力前，必须先查本规范，复用现有层。只有现有层职责无法表达时，才新增 layer 类型。
- 新增 layer 或改变 layer 归属时，必须补充层级回归测试。修窗口级反馈层时，还必须验证 Badge 等局部装饰不受影响。
- 所有层号码只能来自 `LayerLadder`，所有向 `VisualLayerManager` 注入层的路径只能经过 `LayerLadder`。

## 全局层级模型

AtomUI 拥有一套自己的层级阶梯：先定规则，再由规则推出号码。同一套角色顺序适用于所有叠放宿主，不允许按宿主类型分号码。

### 五条规则

1. **角色顺序（每个宿主都相同）：** 角色 T1–T8 自下而上排列。任何叠放宿主中，存在的角色都按这个顺序排列；宿主中没有的角色直接缺省（例如只有 Window 宿主有 T6）。
2. **嵌套：** 作用域宿主（ScrollViewer 的两个注入点、嵌套 `VisualLayerManager`、提升表面）整体属于父宿主的某个角色，它内部的全部角色都低于父宿主中比该角色更高的所有角色。**内容角色（T3–T5）就近解析**，落在最近的作用域宿主，表面宿主也算；**TopLevel 角色（T6–T8：窗口 chrome、TopLevel 呈现、Tour、窗口反馈）一律解析到 TopLevel 的宿主**，跳过所有表面宿主。
3. **装饰跟着表面走：** 共享 `AdornerLayer`（T2）只服务 T1 内容。T3 及以上，每个能承载可聚焦或可装饰控件的提升表面，必须自带一个只启用 `AdornerLayer` 的嵌套 `VisualLayerManager`（表面宿主），使表面内控件的焦点描边和 Badge 紧贴在该表面之上。纯绘制、不含交互控件的表面（Watermark、Tour 遮罩）不需要。
4. **一个层类只承担一种角色：** 角色决定层，消费者按角色选层。
5. **ZIndex 只排同宿主内的兄弟层：** 跨宿主的先后由规则 2 决定。树外的两类按各自机制处理：`WindowDrawnDecorations` overlay 由 Dialog / Drawer 的抑制租约控制，原生 popup 窗口由操作系统排序。控件内部的局部 ZIndex 不得用来表达跨层优先级。

### 角色阶梯与号码

号码均为 `int.MaxValue − N`，真源是 `AtomUI.Controls.Primitives.LayerLadder`（public sealed，号码常量为 internal）。每一级之间留有空隙，供后续扩展。

| 角色 | N | 层 | 承载 |
|---|---:|---|---|
| T1 内容 | ZIndex 0 | 宿主的内容子项 | 内容本身 |
| T2 内容装饰 | 1000 | Avalonia `AdornerLayer`（共享，被 `LayerLadder` 从 −100 改号） | T1 内容的 Badge、焦点描边、TreeView 拖拽预览 |
| T3 内容覆盖 | 900 | `ScopeAwareAdornerLayer` | Watermark、Gallery 吸顶 Tab |
| T4 内容浮动 | 800 | `ScopeAwareOverlayLayer` | FloatButton / FloatButtonGroup |
| T5 内容呈现 | 700 | `ContentPresentationLayer` | 作用域 Drawer、作用域 Dialog（`OverlayScope` 与无 TopLevel fallback） |
| T6 窗口 chrome | 600 | `WindowChromeLayer`（仅 Window 宿主） | 全屏标题栏 |
| T7 TopLevel 呈现 | 500 | `TopLevelPresentationLayer` | `OpenOn` 为 TopLevel 的 Drawer（含 Window Drawer） |
| T7 TopLevel 呈现 | 400 | `TourLayer` | Tour 遮罩 |
| T7 TopLevel 呈现 | 98 / 97 / 96 / 95 | Avalonia `OverlayLayer` / `LightDismissOverlayLayer` / `TextSelectorLayer` / `PopupOverlayLayer`（不改号） | 窗口 Dialog、light-dismiss、文本选择、overlay 弹层 |
| T8 窗口反馈 | 50 | `WindowFeedbackLayer` | Message / Notification |

T7 内部的先后：TopLevel Drawer < Tour < Dialog < LightDismiss < TextSelector < 弹层。

`VisualLayerManagerUtils.ScopeAwareAdornerLayerZIndex` / `ScopeAwareOverlayZIndex` 两个 internal 名字保留并转发到 `LayerLadder` 的新值，因为 Mobile 仓库对 `AtomUI.Controls` 有 internal 可见性。

### 宿主矩阵

| 宿主 | 出现的角色 | 说明 |
|---|---|---|
| AtomUI Window `PART_VisualLayerManager` | T1–T8 | 唯一有 T6 的宿主 |
| PopupRoot / OverlayPopupHost / EmbeddableControlRoot 的 `VisualLayerManager`、浏览器 Gallery shell 自建的 `VisualLayerManager`、Gallery Tour 舞台的嵌套 `VisualLayerManager` | T1–T5、T7、T8（按需） | 号码与 Window 相同 |
| 表面宿主（见下文，包括 chrome 层内的那一个） | T1–T5 | 内容角色的作用域宿主；TopLevel 角色（T6–T8）的解析跳过它 |
| ScrollViewer `ScrollContentPresenter` 的内容包裹面板（随滚动） | T1、T3、T5 | 作用域 Drawer 在这里的 T5 |
| ScrollViewer `ScopeAwareOverlayLayerPanel#PART_ContentHost`（视口固定） | T1（滚动区与滚动条）、T4、T5 | 作用域 Dialog（`OverlayScope` 落在这里时）在 T5，高于 FloatButton |
| 树外：drawn decorations overlay、原生 popup 窗口 | — | 按规则 5 处理 |

### 消费者归属表

| 消费者 | 角色 | 解析方式 |
|---|---|---|
| Count/Dot Badge、键盘焦点描边、TreeView 拖拽预览 | T2（或所在表面的装饰宿主） | Avalonia `AdornerLayer.GetAdornerLayer` |
| Watermark | T3 | `ScopeAwareAdornerLayer.GetLayer` |
| Gallery 吸顶 Tab | T3（窗口宿主），自带表面宿主 | `GalleryStickyTabsHost.ResolveStickyElevationLayer` |
| FloatButton / FloatButtonGroup | T4，浮动表面自带表面宿主 | `ScopeAwareOverlayLayer.GetLayer` |
| Drawer，`OpenOn is TopLevel`（没有父 Drawer 时，`OnAttachedToVisualTree` 把 `OpenOn` 绑定到视觉祖先 TopLevel） | T7 `TopLevelPresentationLayer`，位于该 TopLevel 的宿主 | 按 TopLevel 而不是按 Window 判定，所有宿主一视同仁；`OpenOn is Window` 的部分额外获取 Window chrome 抑制租约 |
| Drawer，其他情况（`OpenOn` 为 null 或指向局部元素） | T5 `ContentPresentationLayer`，位于解析出的作用域宿主（滚动面板 / 包含层 / 窗口宿主） | 滚动内容宿主入口 |
| 嵌套 Drawer | 复用父 Drawer 所在的呈现层 | 对两个呈现层同样适用；`OpenOn` 指向父 Drawer 之外时也始终复用父层 |
| Dialog，默认 | T7 Avalonia `OverlayLayer` | 不变（Avalonia 自带专属装饰层） |
| Dialog，`OverlayScope` 指定作用域 / 无 TopLevel fallback | T5 `ContentPresentationLayer` | 视口宿主入口 |
| Tour 遮罩 | T7 `TourLayer` | 跳过表面宿主 |
| Message / Notification | T8 `WindowFeedbackLayer`，每个 manager 的卡片宿主带表面宿主 | 跳过表面宿主 |
| 全屏标题栏 | T6 `WindowChromeLayer` | 见下文 |
| ImagePreviewer、下拉、菜单、Tooltip、Flyout | T7 PopupOverlay 或原生 popup | 不变 |

### 号码真源与统一注入入口

`LayerLadder` 是唯一号码真源，同时是**唯一的层注入入口**：

- 向 `VisualLayerManager` 宿主注入角色层时，先确保 Avalonia `AdornerLayer` 已创建（通过公开 API `AdornerLayer.GetAdornerLayer`）并被设为 T2 号码（幂等），再调用既有的 `VisualLayerManagerReflectionExtensions.AddLayer`；向 Panel 宿主（滚动内容包裹面板、`ScopeAwareOverlayLayerPanel`）注入时，以同一号码作为子项 ZIndex。
- AtomUI Window 取得 `PART_VisualLayerManager`（模板应用，含重套用）时，也会调用一次“确保 T2 号码”，保证未注入任何层时 Badge 也已在阶梯位置。
- 不新增反射。Avalonia 12.1.6 只在私有 `AddLayer` 中赋值一次 `ZIndex`，此后不回写；这个前提由守护测试兜底（见测试要求）。
- `LayerLadder.IsSurfaceHost` 是 public 附加属性（设置在 `VisualLayerManager` 上），标记一个宿主是表面宿主。

### 表面宿主

表面宿主是提升表面自带的、只启用 `AdornerLayer` 的嵌套 `VisualLayerManager`（`LayerLadder.IsSurfaceHost = true`），作为提升表面视觉根的直接包裹，**每个表面一个**，不是每个层一个，这样同一层中较低表面的装饰不会画到较高的兄弟表面之上。

当前使用表面宿主的表面：`DrawerContainer`、`OverlayDialogPresenter`（作用域 Dialog 与窗口级 Dialog 共用）、`WindowMessageManager` / `WindowNotificationManager` 的卡片宿主、浮动状态下的 FloatButton / FloatButtonGroup、Gallery 吸顶 Tab 宿主、全屏标题栏（位于 chrome 层内）。尽量在各自 ControlTheme 中声明。

两处落点相对最初设计有调整，记录如下：

- **窗口级 Dialog 也带表面宿主。** 设计最初只要求作用域 `OverlayDialogPresenter` 带表面宿主，窗口级 Dialog 依赖 Avalonia `OverlayLayer` 自带的专属装饰层。实现中表面宿主 `PART_SurfaceHost` 声明在 `OverlayDialogPresenterTheme.axaml` 的模板根，作用域与窗口级 Dialog 共用同一模板，因此窗口级 Dialog 也带一个表面宿主：其内控件的焦点描边与 Badge 由 `GetAdornerLayer` 解析到这个更近的表面宿主，而不是 `OverlayLayer` 的专属装饰层。两者都紧贴该 Dialog 表面之上，窗口级 Dialog 本身仍位于 Avalonia `OverlayLayer`（T7），层级不变；它把“表面内装饰留在表面内”（行为变化清单第 8 条）扩展到了窗口级 Dialog。
- **Gallery 吸顶 Tab 的表面宿主位于 `PART_StickyContentHost` 之内。** `GalleryStickyTabsHostTheme.axaml` 把表面宿主声明为 `PART_StickyContentHost`（吸顶背景与分隔线所在的 `PixelAlignedBorder`）的 Child、吸顶内容 `ContentPresenter` 的父级；吸顶背景与边框属于 `PART_StickyContentHost` 自身，表面宿主只包裹 Tab 内容。

解析器按角色区分（规则 2）：

- 内容角色解析器（`ScopeAwareAdornerLayer.GetLayer`、`ScopeAwareOverlayLayer.GetLayer`、`ContentPresentationLayer` 的解析、`DialogOverlayLayer` 的作用域解析）把表面宿主当作普通作用域宿主，就近落入其中。因此表面内的 Watermark、FloatButton、作用域 Dialog、作用域 Drawer 留在该表面之内，不会掉到窗口层级而被表面盖住。
- TopLevel 角色解析器（`TopLevelPresentationLayer`、`TourLayer`、`WindowFeedbackLayer`，以及 popup light-dismiss 层）向上查找 `VisualLayerManager` 时跳过带标记的表面宿主，直达 TopLevel 宿主。Gallery Tour 舞台那种刻意嵌套、没有标记的 `VisualLayerManager` 仍是 Tour 的有效作用域。`WindowChromeLayer` 不经解析器：Window 在模板应用时经 `LayerLadder` 把它直接注入自己的 `PART_VisualLayerManager`。
- light-dismiss 是 TopLevel 角色。Avalonia `Popup.Open()` 按 placement target 最近的 `VisualLayerManager` 取 `LightDismissOverlayLayer`，target 位于表面宿主内（例如作用域 Drawer 中的 ComboBox）时，该层只铺满这个表面，表面外的按下不会关闭弹层。AtomUI `Popup` 在 `Opened`（`Open()` 末尾同步触发）中由 `PopupLightDismissRegistration` 检测到落点属于表面宿主时，释放表面层上的原生租约与处理器，改为在 `LayerLadder.FindTopLevelLayerManager` 解析出的宿主的 `LightDismissOverlayLayer` 上注册。表面宿主上的 dismiss 层随即回到无注册、不可见状态，不拦截输入。不使用 AtomUI `Popup` 的 Avalonia 原生弹层（例如 Avalonia 自带的 ContextMenu）不经过这条重定向。
- `ScopeAwareAdornerLayer.GetLayer` 中“已位于某个 `ScopeAwareAdornerLayer` 内则复用它”这一步只匹配精确类型的 T3 实例，不匹配 `ContentPresentationLayer` / `TopLevelPresentationLayer`。
- 表面宿主未启用 Overlay / PopupOverlay：`GetOverlayLayer` 与 overlay popup 解析到外层窗口，`GetAdornerLayer` 解析到表面宿主。

## 层级总览

| 层 / 宿主 | 所在模块 | 作用域 | 典型使用者 | 号码 |
|---|---|---|---|---|
| `VisualLayerManager` | Avalonia，AtomUI Window 模板承载 | TopLevel / Window | Popup overlay、原生 adorner、AtomUI 自定义层 | 根宿主，不直接作为业务层使用 |
| native `PopupRoot` / OS popup | Avalonia Popup | 独立 popup host / 原生窗口 | `Popup.ShouldUseOverlayLayer == false` 的弹出内容 | 不参与 `VisualLayerManager` 内部 ZIndex |
| Avalonia `AdornerLayer`（T2） | Avalonia 原生 adorner 层 | 控件局部装饰 | CountBadge、DotBadge、TreeView drag preview、键盘焦点描边 | `int.MaxValue - 1000`（由 `LayerLadder` 改号，Avalonia 默认 −100） |
| `ScopeAwareAdornerLayer`（T3） | `AtomUI.Controls.Primitives` | 控件、滚动区域或窗口中的作用域内容覆盖 | Watermark、Gallery sticky mirror | `int.MaxValue - 900` |
| `ScopeAwareOverlayLayer`（T4） | `AtomUI.Controls.Primitives` | 最近的 `ScopeAwareOverlayLayerPanel`、`VisualLayerManager` 或 TopLevel | FloatButton | `int.MaxValue - 800` |
| `ScopeAwareOverlayLayerPanel` | `AtomUI.Controls.Primitives` | 模板内 host | ScrollViewer 内容区域 | 不是 layer，负责限定作用域 |
| `ContentPresentationLayer`（T5，internal） | `AtomUI.Controls.Primitives` | 作用域宿主 | 作用域 Drawer、作用域 / fallback Dialog | `int.MaxValue - 700` |
| `WindowChromeLayer`（T6，internal） | `AtomUI.Desktop.Controls` | Window | 全屏标题栏 | `int.MaxValue - 600` |
| `TopLevelPresentationLayer`（T7，internal） | `AtomUI.Controls.Primitives` | TopLevel | `OpenOn` 为 TopLevel 的 Drawer | `int.MaxValue - 500` |
| `TourLayer`（T7） | `AtomUI.Desktop.Controls` | TopLevel / Window | Tour mask / spotlight | `int.MaxValue - 400` |
| Avalonia `OverlayLayer` / `LightDismissOverlayLayer` / `TextSelectorLayer` / `PopupOverlayLayer`（T7） | Avalonia `VisualLayerManager` 内部层 | TopLevel / Window | Dialog、light-dismiss、文本选择、Popup、ToolTip、ComboBox、Select、DatePicker、ImagePreviewer | `int.MaxValue - 98 / 97 / 96 / 95`，不改号；AtomUI 通过 resolver 获取 |
| `WindowFeedbackLayer`（T8） | `AtomUI.Desktop.Controls` | TopLevel / Window | Message、Notification | `int.MaxValue - 50` |
| drawn decorations overlay | Avalonia `WindowDrawnDecorations` / AtomUI Window theme | Window 装饰视觉 | drawn title bar、caption buttons、shadow | 树外，仅承载窗口装饰，不作为业务交互层 |

## 各层职责


### VisualLayerManager

`VisualLayerManager` 是 Avalonia 提供的窗口视觉层宿主。AtomUI Window 模板通过 `PART_VisualLayerManager` 承载窗口内容和多个 overlay/adorner 层。

使用规则：

- 业务控件不直接把自身当成 `VisualLayerManager` 的普通 child。
- 需要向 `VisualLayerManager` 注入自定义层时，必须通过明确的 layer 类型和固定职责实现，例如 `ScopeAwareAdornerLayer`、`ScopeAwareOverlayLayer`、`ContentPresentationLayer`、`WindowChromeLayer`、`TopLevelPresentationLayer`、`WindowFeedbackLayer`、`TourLayer`，并且只能经 `LayerLadder` 注入。
- 对 Avalonia 内部 layer 的访问必须集中在 `VisualLayerManagerReflectionExtensions` 等已有边界内，避免分散反射。

交互式 presentation 必须保留在 owning `TopLevel` 的视觉子树中。`WindowDrawnDecorations` 的 overlay 是 `TopLevelHost` 的同级装饰子树，并不是 `TopLevel` 后代；把 Dialog、Drawer 或其他可包含 Popup 的业务内容挂入该子树会使 `TopLevel.GetTopLevel()` 返回 `null`，从而破坏 Popup、焦点与输入服务。业务层不得通过嵌套 `VisualLayerManager`、局部 ZIndex、延迟打开或强制切换 native popup 绕过该所有权边界。

### OverlayLayer

Avalonia `OverlayLayer` 专门承载高于普通内容、低于 light-dismiss 与 popup 的窗口内 presentation。Overlay Dialog 使用该层，因此 Dialog 内 popup 仍由同一 Window 的 `PopupOverlayLayer` 托管，外部点击则由两者之间的 `LightDismissOverlayLayer` 处理。

使用规则：

- Dialog、modal surface 等仍可能包含 Popup 的窗口内 presentation 使用 `OverlayLayer`，不得直接放入 `PopupOverlayLayer`。
- presentation target、Dialog 内容和 popup placement target 必须解析到同一 `TopLevel`。
- `OverlayLayer` 不承载 anchored popup 本体；Popup 的创建、定位、light-dismiss 和释放继续由 Avalonia 管理。

### PopupOverlayLayer

`PopupOverlayLayer` 是 Avalonia `VisualLayerManager` 的内部 popup overlay 层。AtomUI 通过 `GetPopupOverlayLayer()` 和 `OverlayLayerResolver` 获取它，用于需要窗口坐标系和 popup 定位的浮层。

典型使用者：

- `Popup` 的 `ShouldUseOverlayLayer` 路径。
- `ToolTip`、`Flyout`、菜单、选择器、日期选择器等基于 popup 的控件。
- `ImagePreviewer` 的 overlay host。

使用规则：

- 弹出层、菜单、下拉面板和图片预览窗口内浮层优先使用 popup overlay 路径。
- 需要解析宿主时使用 `OverlayLayerResolver`，不要在各控件里重复查找主窗口、TopLevel 或 SingleView。
- 不要把 Message/Notification 放入 popup overlay。它们是窗口级反馈，不是 anchored popup。

#### Popup host 与 frame surface

Popup layer/host 与内容表面是两个独立职责。`PopupRoot` 和 `OverlayPopupHost` 保持透明，只负责窗口/layer、输入、定位、
light-dismiss 和透明 shadow buffer；实际 surface 由 host 内共享的 `ShadowsAwareContainer` 在 Child bounds 内绘制。

`AtomUI.Desktop.Controls.Popup` 的 `SurfaceBackground` 默认 `null`，由内容绘制背景、圆角、Padding、箭头和边框；
只有调用方显式提供非空 Brush 时，frame 才拥有表面。不得在每个消费控件重复设置 `null`，不得把 host Background
改为不透明，也不得根据 Child 类型、Background 或 Theme 时序猜测表面所有者。完整契约见
[Popup 桌面版架构设计](../../../controls/desktop/other/popup/overview.md)。

### Drawn decorations overlay

AtomUI 的 `WindowDrawnDecorationsTheme` 在 decorations overlay 中绘制 drawn title bar、caption buttons 与 shadow。该 overlay 由 Avalonia `TopLevelHost` 承载，与 Window `TopLevel` 是同级视觉子树。

使用规则：

- decorations overlay 不提供 Dialog、Drawer 或其他业务 presentation host。
- modal Dialog 与 Window Drawer 仍挂在 Window `TopLevel` 内；它们活跃期间通过 Window 的引用计数租约隐藏 drawn chrome overlay，使 mask 覆盖同一可绘制窗口区域并阻断 caption input。
- 多个 Dialog、Drawer 重叠时，每个 owner 独立持有租约；最后一个 owner 释放后才恢复 drawn chrome。
- Overlay Dialog 的 mask 和 Surface 保持在同一个 presenter；mask actor 使用完整 layer bounds，Surface 使用 Window visible frame bounds，二者不拆成独立 popup。
- Window overlay 几何按职责分为三套真源：完整 layer bounds 保留 TopLevel client surface 坐标系；visible frame bounds 只排除 `FrameShadowThickness`，包含 managed/drawn 标题栏；content bounds 再排除标题栏和内容装饰。Dialog Surface 和 Drawer 使用 visible frame，普通 Window 内容或明确要求正文安全区的反馈才使用 content bounds。
- visible frame 计算仍是 frame shadow 排除量的共享真源。业务 overlay 不按 `OsType`、`IsCsdEnabled` 或 `WindowDecorationMargin` 复制另一套 frame 计算。
- 原生系统 chrome 如果位于 Avalonia client visual tree 外，客户端 overlay 不负责为该区域模拟第二套 mask；原生模态行为由平台 Window owner 关系负责。
- `WindowDrawnDecorationsReflectionExtensions` 只读取 frame/titlebar 几何和 resize-grip 能力，不承担业务 host 发现。

### Native PopupRoot / OS popup

当 `Popup.ShouldUseOverlayLayer == false` 且平台支持原生窗口时，Popup 可以走 native popup root / OS popup 路径。该路径不在 `VisualLayerManager` 的内部 layer 顺序中，定位、阴影和裁剪行为由 Popup 与平台窗口管理共同决定。

使用规则：

- 普通下拉、菜单、Tooltip 等可以按控件属性选择原生 popup 或 overlay popup。
- 浏览器、无原生窗口或显式 overlay 场景必须走 overlay layer。
- 不要用 native popup 承载 modal mask 或窗口级反馈。
- 修 popup 阴影、定位、裁剪时要同时确认 native popup root 和 overlay popup 两条路径是否受影响。
- 修 popup surface 时只在 Child bounds 内绘制；`PopupRoot.Background` 必须保持 `null`，不得填充 native shadow buffer。

### Avalonia AdornerLayer

Avalonia 原生 `AdornerLayer` 是共享的 T2 内容装饰层，紧贴控件的局部装饰；`LayerLadder` 把它的号码由 −100 改为 `int.MaxValue - 1000`（每个 AtomUI 注入层的 `VisualLayerManager` 与 AtomUI Window 模板应用时都会确保）。它适合与目标控件生命周期和布局强绑定的轻量覆盖，不适合作为全局层级体系的最高层；提升表面内控件的装饰由该表面自带的表面宿主承接。

典型使用者：

- `AbstractCountBadge` / `AbstractDotBadge` 在 `DecoratedTarget` 模式下挂 Count/Dot Badge adorner。
- `TreeView` 拖拽预览。

使用规则：

- Count/Dot Badge 必须保持在原生 `AdornerLayer`，这是局部装饰语义。
- `RibbonBadge` 不使用窗口级 adorner，保持在目标内容的 inline visual tree 中。
- 不要通过抬高原生 `AdornerLayer` 来解决 Message、Notification 或其他窗口级反馈遮挡问题。这会破坏 Badge 等局部装饰的层级语义；它的号码只由 `LayerLadder` 决定。

#### 键盘焦点描边

`AdornerLayer.DefaultFocusAdorner` 是全局的键盘焦点描边，对应 Ant Design `genFocusOutline`。它由 `AdornerLayerTheme.axaml` 中的 internal `FocusOutlineAdorner` 实现，具体规则如下：

- **触发**：只在 Tab / Directional 导航获得焦点时出现，对应 `:focus-visible`；指针获得焦点时不出现。
- **外观**：
  - 描边宽度为 `LineWidthFocus`（`LineWidth * 3`），颜色为 `ColorFocusBorder`（即 `ColorPrimaryBorder`）；
  - 描边画在目标外侧，与目标边缘之间留出 `AdornerLayerToken.FocusOutlineOffset`（`LineWidth`）的间距；
  - 圆角等于目标圆角加外扩量，并夹到外扩后矩形短边的一半，所以圆形目标的描边仍是正圆。
- **目标圆角的来源**，按以下优先级取：
  1. 焦点目标自身实现的 `IFocusOutlineShapeProvider`，如 Button 的有效圆角、RadioIndicator 的正圆；
  2. 焦点目标的模板宿主实现的 `IFocusOutlineShapeProvider`，用于目标自身没有圆角、由宿主外框裁剪的情况，如 Expander 的 Header；
  3. `TemplatedControl.CornerRadius`；
  4. `Border.CornerRadius` / `DashedBorder.CornerRadius`。
- **描边范围**：默认是整个控件。只需描边控件的一部分时，在主题中给该部件设置 `TemplatedControl.IsTemplateFocusTarget="True"`，例如 CheckBox / Radio 的指示器、CollapseItem / Expander 的标题、线型标签的按钮行。
- **内收**：在焦点目标上设置 `FocusOutlineAdorner.IsInset="True"`，描边画在目标内侧（偏移为负的描边宽度），对应 Ant Design `genFocusOutline(token, -3)`，用于卡片标签。
- **裁剪**：描边关闭了 `AdornerLayer.IsClipEnabled`。原因是 Avalonia 的装饰器裁剪从目标自身边界开始计算，而 `TemplatedControl` 默认会裁剪，开启后描边会被整个切掉。代价是：焦点元素被滚出可视区时，描边不会随祖先一起被裁剪。
- **自行表达焦点的控件**：这类控件必须把 `FocusAdorner` 设为空，不再叠加描边。例如：
  - 输入家族用外框的 `:focus-within` 表达焦点（`InputControlFrameTheme` 统一设置）；
  - Slider 手柄放大并画外圈、OtpLineEdit 当前格外发光、Calendar 全屏日期格顶部主色线、DataGrid 展开按钮变色；
  - Carousel 指示点没有描边（Ant Design `outline: none`），键盘焦点复用悬停透明度。
- **宿主持有焦点的列表**：TreeView、NavMenu 的键盘焦点留在宿主上，键盘位置是内部活动项。它们把 `FocusAdorner` 设为空，改用内部的 `ManagedFocusOutline` 在活动项上显示同一个 `DefaultFocusAdorner`：
  - `ManagedFocusOutline` 从目标自己的装饰层构建并挂载描边，所以弹层里的项画在弹层的装饰层中；
  - 显示条件模拟 `:focus-visible`：宿主持有焦点并且最近一次交互是键盘；
  - 拥有者在目标离开视觉树时必须移除描边。
- **键盘停靠点**：一个可切换部件只保留一个停靠点，落在实际执行切换的元素上，例如 Collapse / Expander 按 `TriggerType` 在标题与展开按钮之间二选一。

### ScopeAwareAdornerLayer（T3 内容覆盖）

`ScopeAwareAdornerLayer` 是 AtomUI 的作用域感知内容覆盖层。它会优先寻找最近的 `ScrollContentPresenter` 或 `VisualLayerManager`（表面宿主也算），必要时注入到内容宿主中，适合需要跟随目标作用域、滚动区域或局部窗口内容的覆盖元素。它只承载内容覆盖，不再承载 Drawer。

典型使用者：

- `Watermark`。
- Gallery `GalleryStickyTabsHost` 的 sticky mirror。

使用规则：

- 用于“覆盖某个目标区域或作用域”的内容，而不是窗口级全局反馈、Drawer 或 Dialog。
- 需要跟随 `ScrollContentPresenter`、被装饰元素或局部内容坐标时，应优先使用该层。
- visual 已位于现有 `ScopeAwareAdornerLayer` 内、且没有更具体的滚动宿主时，复用当前包含层；该匹配只认**精确类型**的 T3 实例，不匹配 `ContentPresentationLayer` / `TopLevelPresentationLayer`，`FindAdornerLayer` 同理。
- 使用 `ScopeAwareAdornerLayer.SetAdornedElement` / `SetAdorner` 时，必须有对应的 detach / cleanup 路径。
- 它高于共享 `AdornerLayer`（T2），低于 FloatButton（T4）；T2 内的内容不能假设能盖住它。

### ScopeAwareOverlayLayer（T4 内容浮动）

`ScopeAwareOverlayLayer` 是作用域浮动层，通常配合 `ScopeAwareOverlayLayerPanel` 使用。ScrollViewer 模板在内容区域包裹 `ScopeAwareOverlayLayerPanel`，让滚动内容中的浮动元素可以覆盖滚动条区域或内容区域。

典型使用者：

- `FloatButton` / `FloatButtonGroup` / `FloatButtonHost`。

使用规则：

- 用于“作用域内浮动控件”，不是 modal、全局反馈或目标装饰。它只承载 FloatButton，不再承载 Dialog。
- 如果控件应限制在某个 `ScrollViewer` 或局部 host 范围内，优先选择该层。
- 不要用它承载 Drawer、Message、Notification、Dialog 或菜单下拉。

### ContentPresentationLayer（T5 内容呈现）

`ContentPresentationLayer`（internal，继承 `ScopeAwareAdornerLayer`，位于 `AtomUI.Controls`）承载作用域内的呈现类表面：

- 作用域 Drawer：`OpenOn` 为 null 或指向局部元素。入口是滚动内容宿主（随滚动内容移动），作用域解析规则不变。
- 作用域 Dialog：`OverlayScope` 指定的作用域，以及无 TopLevel 时的 fallback。入口是视口宿主（固定在视口上，高于同宿主的 FloatButton）；`DialogOverlayLayer` 仍铺满作用域。

使用规则：

- Drawer 的 public / protected API 不变，`NotifyBeforeOpen` / `NotifyBeforeClose` 仍接收 `ScopeAwareAdornerLayer`，精确类型判断改为 `is` 判断。
- 嵌套 Drawer 复用父 Drawer 所在的呈现层；`OpenOn` 指向父 Drawer 之外时也始终复用父层。
- 同一宿主的作用域 Drawer 与作用域 Dialog 共用同一个 T5 实例，二者按打开顺序排列，后打开的在上（与 antd DOM 追加顺序一致）。这是“同号插入顺序”禁令中唯一有文档记录的例外，见“禁止做法”。
- T5 不绕过 FlowDirection 策略：与 T3 一样随宿主镜像，RTL 下作用域 Drawer 仍贴合其 target。作用域 Dialog 由 `DialogOverlayLayer` 铺满作用域，镜像不改变其几何。

### TopLevelPresentationLayer（T7 TopLevel 呈现）

`TopLevelPresentationLayer`（internal，继承 `ScopeAwareAdornerLayer`，位于 `AtomUI.Controls`）承载 `OpenOn is TopLevel` 的 Drawer，位于该 TopLevel 的宿主，对 Window、PopupRoot、EmbeddableControlRoot、浏览器 shell 一视同仁。TopLevel 角色解析跳过表面宿主，因此 TopLevel Drawer 不会被关进某个表面里。

使用规则：

- 在所有宿主中位于作用域 Dialog 之上、TopLevel Dialog（Avalonia `OverlayLayer`）与 Tour 之下；在 Window 中位于全屏标题栏之上。
- `OpenOn` 在 T5 与 T7 之间动态迁移（Window target ⇄ 局部 target、跨 Window、父子 Drawer 同时迁移）时，一次状态迁移中移动 container、交接 lease、释放旧层。
- Drawer 的 drawn chrome 抑制租约不变：AutoHide 标题栏在 Window Drawer 或 Dialog 活跃时仍然关闭；Pinned / Docked 模式下由层级保证 Window Drawer 和 Dialog 盖住标题栏。

### WindowChromeLayer（T6 窗口 chrome）

`WindowChromeLayer`（internal，`AtomUI.Desktop.Controls`，透明 Panel，自身不参与命中）只存在于 AtomUI Window 宿主，由 Window 在模板应用时经 `LayerLadder` 注入 `PART_VisualLayerManager`；模板重套用和窗口关闭时被释放（表面宿主清空，标题栏宿主离开该层）。Avalonia 的 `VisualLayerManager` 没有移除层的 API，清空的层随旧模板的 `VisualLayerManager` 一起丢弃，不为此新增反射。`WindowChromeLayerTheme.axaml` 把父 `VisualLayerManager` 的 `Padding`（`OffScreenMargin`）投影为层的 `Margin`，使标题栏表面保持迁移前的窗口位置。层内是一个表面宿主，其 Child 是 `FullScreenTitleBarHost`，因此标题栏按钮的焦点描边和标题栏内的 Badge 都在 chrome 层自己的装饰层中；标题栏打开的 Dialog 仍解析到窗口的 `OverlayLayer`。

宿主仍在三个 Window 模板中声明为 `PART_FullScreenTitleBarHost`，保留 ControlTheme 与 `TemplateBinding` 契约；Window 在 `OnApplyTemplate` 中把它迁入 chrome 层，模板解除时释放。详见 [Window 全屏标题栏设计](../../../controls/desktop/window/window/full-screen-title-bar-design.md)。

使用规则：

- 只由 Window 和 `LayerLadder` 注入，不得作为普通控件的 overlay 宿主。
- 位于内容、Badge、焦点描边、Watermark、吸顶 Tab、FloatButton、作用域 Drawer / Dialog 之上；位于 TopLevel Drawer、Tour、Dialog、弹层、Message 之下。
- 不参与 Message、Notification、Popup、Badge、Drawer 的层级决策。

### WindowFeedbackLayer（T8 窗口反馈）

`WindowFeedbackLayer` 是 AtomUI 桌面控件包的窗口级全局反馈层。它挂在 `VisualLayerManager` 中，是阶梯最顶层，用于保证全局反馈不会被任何作用域层、Drawer、Tour 或 Dialog 遮挡。

典型使用者：

- `WindowMessageManager`。
- `WindowNotificationManager`。

使用规则：

- Message、Notification 等窗口级反馈必须优先使用该层。
- 每个 manager 的卡片宿主是一个表面宿主，卡片内控件的焦点描边和 Badge 画在该宿主之上。
- 该层是跨 manager 叠放顺序的唯一 owner。manager 成功提交新 card 后将自身作为原子反馈组激活到直接子项末尾；同一
  manager 内部的卡片顺序仍由 `FeedbackStackPanel` 管理，不在宿主层交错多个 manager 的 card。
- 激活必须使用直接子项 collection `Move`；当前 manager 已位于末尾时不发布集合变化。禁止使用 `Remove` + `Add` 造成
  detach/attach，禁止使用递增 `ZIndex`、静态序号、全局注册表、timer 或 Dispatcher 延迟保存激活状态。
- 如果宿主没有 `VisualLayerManager`，可以 fallback 到 Avalonia 原生 `AdornerLayer`，但这是兼容路径，不是主路径。
- 原生 `AdornerLayer` fallback 使用相同的直接子项移动语义；无宿主的 inline manager 不参与窗口反馈层激活。
- 不要把 Badge、水印、Drawer、Tour、Popup、Dialog、ImagePreviewer 等迁入该层。
- 变更该层时，必须验证全局反馈层级和 Badge 局部装饰行为。

### TourLayer（T7）

`TourLayer` 是 Tour 控件专用的窗口遮罩层，用于绘制 spotlight mask 和目标区域洞穿。它是窗口级引导遮罩，不是通用 modal layer。号码为 `int.MaxValue - 400`：高于 TopLevel Drawer，低于 Dialog（Avalonia `OverlayLayer`）。

使用规则：

- 只用于 Tour。
- 其他控件需要 modal mask 时，应使用 popup overlay / dialog mask 体系，而不是复用 TourLayer。
- Tour 弹窗本体仍通过 popup overlay 路径定位，mask 和弹窗职责分离。
- 解析时跳过表面宿主；Gallery Tour 舞台这类未标记的嵌套 `VisualLayerManager` 仍是有效作用域。
- Tour 固定在 Dialog 之下：在已打开的模态 Dialog 内启动 Tour 时遮罩会落到 Dialog 之下，这是不受支持的用法（Tour 不负责 Dialog）。

### FullScreenTitleBarHost

`FullScreenTitleBarHost` 是全屏标题栏的表面，在 Window 模板中以 `PART_FullScreenTitleBarHost` 声明，由 Window 在模板应用时迁入 `WindowChromeLayer`（T6）内的表面宿主，因此位于所有内容角色之上、所有 TopLevel 呈现和反馈之下；它不是注入到 `VisualLayerManager` 的通用 layer。模式、显隐状态机和内容区占位见 [Window 全屏标题栏设计](../../../controls/desktop/window/window/full-screen-title-bar-design.md)。

使用规则：

- 只由 Window 模板和 Window 控件生命周期管理。
- 不得作为普通控件的 overlay 宿主。
- 不参与 Message、Notification、Popup、Badge、Drawer 等层级决策。

## 选择规则

| 场景 | 应使用 | 不应使用 |
|---|---|---|
| 全局 Message / Notification | `WindowFeedbackLayer`（T8） | `AdornerLayer`、`PopupOverlayLayer`、`ScopeAwareAdornerLayer` |
| 下拉、菜单、Tooltip、Flyout、选择器面板 | `PopupOverlayLayer` / `Popup.ShouldUseOverlayLayer` | `WindowFeedbackLayer` |
| Dialog / MessageBox overlay host | Window `OverlayLayer`；`OverlayScope` 或无 TopLevel 能力时使用 `ContentPresentationLayer` | drawn decorations overlay、`PopupOverlayLayer`、`ScopeAwareOverlayLayer`、平台硬编码 |
| ImagePreviewer overlay host | `OverlayLayerResolver` 获取 `PopupOverlayLayer` | 手动遍历窗口或主视图 |
| Count/Dot Badge、焦点描边 | Avalonia `AdornerLayer`（所在表面的装饰宿主） | `WindowFeedbackLayer`、`ScopeAwareAdornerLayer` |
| RibbonBadge | inline visual tree | 任意窗口级 layer |
| Watermark / sticky mirror | `ScopeAwareAdornerLayer`（T3） | `WindowFeedbackLayer`、`PopupOverlayLayer` |
| FloatButton / 作用域浮动按钮 | `ScopeAwareOverlayLayer`（T4） | `PopupOverlayLayer`、`WindowFeedbackLayer` |
| Drawer（`OpenOn` 为 null 或局部元素） | `ContentPresentationLayer`（T5） | `ScopeAwareAdornerLayer`、`ScopeAwareOverlayLayer` |
| Drawer（`OpenOn is TopLevel`） | `TopLevelPresentationLayer`（T7） | `ScopeAwareAdornerLayer`、`ContentPresentationLayer` |
| Tour mask | `TourLayer` | Dialog mask、`WindowFeedbackLayer` |
| TreeView 拖拽预览 | Avalonia `AdornerLayer` | `WindowFeedbackLayer` |
| 全屏状态下的标题栏表面 | `FullScreenTitleBarHost`（经 `WindowChromeLayer`） | 通用 layer API |
| 需要新的提升表面并含可聚焦控件 | 在该表面自带表面宿主（`LayerLadder.IsSurfaceHost`） | 共享 `AdornerLayer`、局部 ZIndex |

## 层级顺序规则

- 角色顺序固定为 T1 < T2 < T3 < T4 < T5 < T6 < T7 < T8，号码见上表，同一阶梯适用于所有宿主类型。
- Avalonia `AdornerLayer` 被 `LayerLadder` 从 −100 下移到 `int.MaxValue - 1000`，仍高于内容、仍低于全部 AtomUI 提升层，且不改变它与 Avalonia 其他层的相对顺序。
- T3（内容覆盖）低于 T4（内容浮动）：同一宿主中 FloatButton 高于 Watermark 和吸顶 Tab。
- `WindowFeedbackLayer` 必须高于其他全部层，否则 Message / Notification 会被作用域层、Drawer 或 Tour 遮挡。
- Dialog 所在 `OverlayLayer` 必须低于 `LightDismissOverlayLayer` 与 `PopupOverlayLayer`，保证内容 popup 位于 Dialog 之上且外部点击可 light-dismiss。
- 内层宿主（滚动内容、表面宿主、嵌套 `VisualLayerManager`）整体低于父宿主中更高的角色：例如滚动内容里的 Drawer 低于该 ScrollViewer 视口上的 FloatButton，也低于窗口 chrome。
- 新增 `int.MaxValue - N` 层级常量时，必须先在 `LayerLadder` 定义，并在本文档更新职责和相对顺序。不要裸写 `int.MaxValue` 或随机选择接近最大值的数字。

## 生命周期与清理

- 所有动态加入 layer 的元素必须在 detach、close、dispose、template reapply 或 owner 变更时移除。
- `DialogOverlayLayer` 从第一个 presenter 开始持有实际 host/TopLevel size 订阅，并在最后一个 presenter 移除时解绑且从实际 host 删除。
- modal Dialog 与 Window Drawer 获取的 drawn chrome suppression 租约必须在 close、dispose、owner 变更和失败 teardown 中对称释放。
- 如果 layer child 设置了 adorned element、logical parent、事件订阅、timer、binding 或 resource host，释放路径必须和获取路径成对出现。
- 对 `WindowMessageManager`、`WindowNotificationManager` 这类 manager，宿主模板重套用时必须先从旧 host layer 移除，再重新安装。
- `WindowChromeLayer` 随 Window 模板应用注入；模板重套用和窗口关闭时释放，把 `PART_FullScreenTitleBarHost` 从 chrome 层移出。清空的层留在旧模板的 `VisualLayerManager` 中随之丢弃（Avalonia 没有移除层的 API），新模板注入自己的 chrome 层，任一时刻只有一个 chrome 层承载标题栏宿主。
- manager 激活不取得订阅、timer、cache 或额外强引用；`Move` 不得触发 manager 的 visual/logical detach。rehost 与
  `Dispose()` 继续通过既有 host collection 移除路径释放 manager。
- 对 `ScopeAwareAdornerLayer` / `ScopeAwareOverlayLayer` / 呈现层等注入式层，缓存 layer 时必须确认 visual parent 仍然有效。

## 测试要求

- 新增或改变 layer 归属时，应写层级回归测试，断言父层级而不是只断言 child `ZIndex`。
- **守护测试：** 对每一类宿主（Window、PopupRoot、OverlayPopupHost、EmbeddableControlRoot、ScrollViewer 两个注入点）断言角色层按号码表排序（断言父层归属和号码），并在 Popup 开关、FullScreen 往返、模板重套用后仍然成立；它同时守护“Avalonia 不回写 `AdornerLayer.ZIndex`”这一前提。常驻实现为 `tests/AtomUI.Desktop.Controls.Tests/Primitives/LayerLadderHostOrderTests.cs`，当前覆盖 Window 宿主（含 T6 chrome 层承载 `PART_FullScreenTitleBarHost` 且模板重套用后唯一）、PopupRoot 宿主与 ScrollViewer 视口注入点；OverlayPopupHost、EmbeddableControlRoot 与 ScrollViewer 滚动内容注入点尚无常驻守护，改动这些宿主的注入路径时需补临时验证。
- **归属回归：** 按消费者归属表逐个断言其落层；嵌套 Drawer 复用包含层；`OpenOn` 在 T5 与 T7 之间迁移时旧层和 lease 被释放；Tour 与作用域层不会被关进表面宿主；overlay popup 从表面宿主内打开时解析到外层。
- **遮挡与装饰：** 全屏标题栏打开时，标题栏区域的 `InputHitTest` 命中标题栏而不是吸顶 Tab、Badge、Watermark 或 FloatButton；Drawer、Message、标题栏内按钮的键盘焦点描边可见，且落在各自表面的装饰宿主中。
- 涉及 `WindowDrawnDecorations` 时，测试必须用与 Window `TopLevel` 同级的真实 `TopLevelHost` 拓扑，不能把模拟 decorations 子树挂回 Window 内容树制造错误的 `TopLevel` 祖先。
- 修窗口级反馈遮挡时，必须覆盖 Message 和 Notification。
- manager 激活测试必须覆盖 `A.Show -> B.Show -> A.Show`、Message / Notification 混合 manager、已在栈顶的无操作快路径、
  单次 collection `Move`、无 attach/detach，以及 dispose 后 manager / presenter / card / callback owner 的可回收性。
- 涉及 Badge、Watermark、Drawer、sticky mirror、TreeView drag preview 时，应分别验证其既有层归属没有被误改。
- Gallery sticky mirror、Message / Notification 和 Badge 的组合是当前层级体系的关键回归场景。
- Dialog 内容 popup 由 `DialogPopupPrimitiveLayeringTests`、`DialogPopupControlFamilyTests` 与 DataGrid 专项测试覆盖；直接 Popup、Flyout/MenuFlyout、ToolTip/ContextMenu 与控件家族矩阵见 [Modal 内容弹层叠放设计](../../../controls/desktop/feedback/modal/popup-layering-design.md)。
- Popup surface 变更必须同时证明 Direct Popup 与 content-owned 家族继承透明 host frame 默认值、显式非空 surface 仍生效，并确认 Child bounds、圆角、shadow thickness 和 placement 没有变化。

当前 Dialog 内容 popup 分层实机证据为 Windows、macOS 和 Ubuntu GNOME Wayland 已测试；Wayland 证据仅覆盖
Dialog 内容 Popup 的真实窗口人工回归。Linux X11 仍未测试；Drawer 在 Linux X11 和 Linux Wayland 的实机状态也仍为未测试。
全局层级阶梯（本模型）的实机视觉状态：

- **Windows（已实测，Windows 10，2560×1600、150% 缩放，单显示器，Light 主题，Gallery Debug 构建）：** 原始故障场景（全屏 → 布局 / Space → 滚动 8 格 → 鼠标从内容区滑到顶边）中，AutoHide 标题栏在全宽呈现并盖住吸顶 Tab，连续截帧中没有吸顶 Tab 画在标题栏之上的帧；Pinned 模式下标题栏常驻于吸顶 Tab 之上。全屏下打开 Window Drawer（Drawer 示例“抽屉表单”），键盘 Tab 焦点描边在 Drawer 表面内的关闭、取消、提交按钮上可见。FloatButton 示例“徽标”中 FloatButton 自身的 Badge 画在各自表面之上、未被相邻 FloatButton 盖住。
- **Windows 未实测：** FloatButton 盖住内容 Badge（Gallery 没有该组合的示例，只有 Headless 证据）、作用域 Drawer / Dialog、窗口级 Dialog、Tour、Watermark、Message / Notification 与标题栏内按钮的焦点描边、Dark 主题、多显示器。
- **macOS、Linux X11、Linux Wayland：** 未实测，只有 Headless 证据。

该状态必须与专项设计文档同步，不能把 Headless 共享路径通过解释为未执行平台或未执行控件的实机证据。

## 变更说明

### 2026-11-22 全局层级阶梯

- 引入 `LayerLadder`、T1–T8 角色阶梯、表面宿主和三个新层（`ContentPresentationLayer`、`TopLevelPresentationLayer`、`WindowChromeLayer`）；Avalonia `AdornerLayer` 由 −100 改号为 `int.MaxValue - 1000`；`ScopeAwareAdornerLayer` 与 `ScopeAwareOverlayLayer` 对调先后并收窄为单一角色；`TourLayer` 由 −98 改为 `int.MaxValue - 400`。
- 新增 public 附加属性 `LayerLadder.IsSurfaceHost`。第三方如果自行向 `VisualLayerManager` 注入层并依赖 `AdornerLayer` 为 −100，需要改为经 `LayerLadder` 注入，或重新核对相对顺序。
- 各控件的行为变化见对应控件 changelog。

**Mobile 迁移说明（AtomUI.Mobile 仓库后续跟进，本次未改）：** 新层、号码真源与注入入口对 Mobile 通过既有 internal 可见性使用。迁移时需注意：

- Avalonia `AdornerLayer` 号码变化：仍高于内容、仍低于全部 AtomUI 提升层；
- T3 / T4 先后对调：FloatButton 现在高于 Watermark / 吸顶 Tab；
- 呈现类表面（抽屉、ActionSheet、对话框等）应迁到 T5 / T7 呈现层并自带表面宿主，否则升级后会落到 FloatButton 之下。

`changelog-guidelines.md` 没有为跨控件的 `AtomUI.Controls` 基础设施变更规定单独的 changelog 位置，正式版本说明由 release owner 写入根目录 `CHANGELOG.md`；因此这份迁移说明暂记在这里。

## 禁止做法

- 禁止为了让某个控件“显示在最上面”直接把它放入 `WindowFeedbackLayer`。
- 禁止通过提高 MessageCard、NotificationCard、Badge adorner 等 child 的 `ZIndex` 解决跨父层遮挡。
- 禁止绕过 `LayerLadder` 直接向 `VisualLayerManager` 注入层，或在 `LayerLadder` 之外定义层号码。
- 禁止用局部 ZIndex、同号插入顺序、延迟插入来表达跨层优先级；TopLevel 角色解析不得落入表面宿主。唯一有文档记录的例外：同一宿主的作用域 Drawer 与作用域 Dialog 共用同一个 T5 `ContentPresentationLayer` 实例，二者之间按打开（插入）顺序排列，后打开的在上，与 antd 的 DOM 追加顺序一致。这是同一角色内的先后，不是跨层优先级。
- 禁止把 Overlay Dialog 的 mask 与 Surface 拆成独立 popup 或不同生命周期 owner；二者必须位于同一个 presenter，并使用彼此独立的 motion actor 和几何约束。
- 禁止在每个控件中复制 TopLevel / MainWindow / SingleView 查找逻辑；应使用现有 resolver。
- 禁止新增 layer 后不更新本文档和回归测试。
