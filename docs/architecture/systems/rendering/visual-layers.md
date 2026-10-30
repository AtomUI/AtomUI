# AtomUI 视觉层规范

本文档定义 AtomUI 中跨普通视觉树绘制的 layer、overlay、adorner 的职责边界和使用规则。这里的“层”指会把内容放到普通控件布局流之外的宿主容器或窗口级覆盖区域，不包含普通 `Panel`、`Border.BoxShadow`、模板内部装饰容器等常规视觉结构。

## 总体原则

- 先判断覆盖内容的作用域，再选择层：控件局部、滚动/作用域局部、窗口级反馈、弹出浮层、窗口模板专用层分别使用不同机制。
- 不允许用子元素 `ZIndex` 解决跨父层遮挡问题。不同父层之间的遮挡必须在父层职责和层级顺序上解决。
- 不允许把局部装饰迁到窗口级层。Badge、拖拽预览、水印、Drawer、sticky mirror 等都有不同的锚定和裁剪语义。
- 新增跨视觉树覆盖能力前，必须先查本规范，复用现有层。只有现有层职责无法表达时，才新增 layer 类型。
- 新增 layer 或改变 layer 归属时，必须补充层级回归测试。修窗口级反馈层时，还必须验证 Badge 等局部装饰不受影响。

## 层级总览

| 层 / 宿主 | 所在模块 | 作用域 | 典型使用者 | 层级约定 |
|---|---|---|---|---|
| `VisualLayerManager` | Avalonia，AtomUI Window 模板承载 | TopLevel / Window | Popup overlay、原生 adorner、AtomUI 自定义层 | 根宿主，不直接作为业务层使用 |
| native `PopupRoot` / OS popup | Avalonia Popup | 独立 popup host / 原生窗口 | `Popup.ShouldUseOverlayLayer == false` 的弹出内容 | 不参与 `VisualLayerManager` 内部 ZIndex |
| Avalonia `OverlayLayer` | Avalonia `VisualLayerManager` 内部层 | TopLevel / Window | Dialog 等窗口内 presentation | 高于普通内容，低于 light-dismiss 与 popup |
| Avalonia `PopupOverlayLayer` | Avalonia `VisualLayerManager` 内部层 | TopLevel / Window | Popup、ToolTip、ComboBox、Select、DatePicker、ImagePreviewer | 由 Avalonia 管理，AtomUI 通过 resolver 获取 |
| drawn decorations overlay | Avalonia `WindowDrawnDecorations` / AtomUI Window theme | Window 装饰视觉 | drawn title bar、caption buttons、shadow | 仅承载窗口装饰，不作为业务交互层 |
| Avalonia `AdornerLayer` | Avalonia 原生 adorner 层 | 控件局部装饰 | CountBadge、DotBadge、TreeView drag preview | 不用于窗口级反馈 |
| `ScopeAwareAdornerLayer` | `AtomUI.Controls.Primitives` | 控件、滚动区域或窗口中的作用域装饰 | Drawer、Watermark、Gallery sticky mirror | `int.MaxValue - 99` |
| `ScopeAwareOverlayLayerPanel` | `AtomUI.Controls.Primitives` | 模板内 host | ScrollViewer 内容区域 | 不是 layer，负责限定作用域 |
| `ScopeAwareOverlayLayer` | `AtomUI.Controls.Primitives` | 最近的 `ScopeAwareOverlayLayerPanel`、`VisualLayerManager` 或 TopLevel | FloatButton、滚动内容内的作用域浮动元素 | `int.MaxValue - 990` |
| `WindowFeedbackLayer` | `AtomUI.Desktop.Controls` | TopLevel / Window | Message、Notification | `int.MaxValue - 50`，高于 `ScopeAwareAdornerLayer` |
| `TourLayer` | `AtomUI.Desktop.Controls` | TopLevel / Window | Tour mask / spotlight | `int.MaxValue - 98` |
| `FullscreenPopoverLayer` | `AtomUI.Desktop.Controls.Window` | Window 模板内部 | macOS 全屏标题栏 popover | 模板固定层，不作为通用 overlay |

## 各层职责

### VisualLayerManager

`VisualLayerManager` 是 Avalonia 提供的窗口视觉层宿主。AtomUI Window 模板通过 `PART_VisualLayerManager` 承载窗口内容和多个 overlay/adorner 层。

使用规则：

- 业务控件不直接把自身当成 `VisualLayerManager` 的普通 child。
- 需要向 `VisualLayerManager` 注入自定义层时，必须通过明确的 layer 类型和固定职责实现，例如 `ScopeAwareAdornerLayer`、`ScopeAwareOverlayLayer`、`WindowFeedbackLayer`、`TourLayer`。
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

Avalonia 原生 `AdornerLayer` 用于紧贴控件或 TopLevel 的局部装饰。它适合与目标控件生命周期和布局强绑定的轻量覆盖，不适合作为 AtomUI 全局层级体系的最高层。

典型使用者：

- `AbstractCountBadge` / `AbstractDotBadge` 在 `DecoratedTarget` 模式下挂 Count/Dot Badge adorner。
- `TreeView` 拖拽预览。

使用规则：

- Count/Dot Badge 必须保持在原生 `AdornerLayer`，这是局部装饰语义。
- `RibbonBadge` 不使用窗口级 adorner，保持在目标内容的 inline visual tree 中。
- 不要通过抬高原生 `AdornerLayer` 来解决 Message、Notification 或其他窗口级反馈遮挡问题。这会破坏 Badge 等局部装饰的层级语义。

### ScopeAwareAdornerLayer

`ScopeAwareAdornerLayer` 是 AtomUI 的作用域感知 adorner 层。它会优先寻找最近的 `ScrollContentPresenter` 或 `VisualLayerManager`，必要时注入到内容宿主中，适合需要跟随目标作用域、滚动区域或局部窗口内容的覆盖元素。

典型使用者：

- `Drawer` / `DrawerContainer`。
- `Watermark`。
- Gallery `GalleryStickyTabsHost` 的 sticky mirror。

使用规则：

- 用于“覆盖某个目标区域或作用域”的内容，而不是窗口级全局反馈。
- 需要跟随 `ScrollContentPresenter`、被装饰元素或局部内容坐标时，应优先使用该层。
- visual 已位于现有 `ScopeAwareAdornerLayer` 内，且没有更具体、与 adorned target 匹配的滚动宿主时，必须复用当前包含层；嵌套 Drawer 不得重新注入平行 layer。
- 使用 `ScopeAwareAdornerLayer.SetAdornedElement` / `SetAdorner` 时，必须有对应的 detach / cleanup 路径。
- 它的层级高于 Avalonia 原生 adorner；因此原生 `AdornerLayer` 内的内容不能假设能盖住它。

### ScopeAwareOverlayLayer

`ScopeAwareOverlayLayer` 是作用域浮动层，通常配合 `ScopeAwareOverlayLayerPanel` 使用。ScrollViewer 模板在内容区域包裹 `ScopeAwareOverlayLayerPanel`，让滚动内容中的浮动元素可以覆盖滚动条区域或内容区域。

典型使用者：

- `FloatButton` / `FloatButtonGroup` / `FloatButtonHost`。
- ScrollViewer 内容区域内的局部 overlay。

使用规则：

- 用于“作用域内浮动控件”，不是 modal、全局反馈或目标装饰。
- 如果控件应限制在某个 `ScrollViewer` 或局部 host 范围内，优先选择该层。
- 不要用它承载 Drawer、Message、Notification、Dialog 或菜单下拉。

### WindowFeedbackLayer

`WindowFeedbackLayer` 是 AtomUI 桌面控件包的窗口级全局反馈层。它挂在 `VisualLayerManager` 中，层级高于 `ScopeAwareAdornerLayer`，用于保证全局反馈不会被 sticky mirror、Drawer 这类作用域层遮挡。

典型使用者：

- `WindowMessageManager`。
- `WindowNotificationManager`。

使用规则：

- Message、Notification 等窗口级反馈必须优先使用该层。
- 该层是跨 manager 叠放顺序的唯一 owner。manager 成功提交新 card 后将自身作为原子反馈组激活到直接子项末尾；同一
  manager 内部的卡片顺序仍由 `FeedbackStackPanel` 管理，不在宿主层交错多个 manager 的 card。
- 激活必须使用直接子项 collection `Move`；当前 manager 已位于末尾时不发布集合变化。禁止使用 `Remove` + `Add` 造成
  detach/attach，禁止使用递增 `ZIndex`、静态序号、全局注册表、timer 或 Dispatcher 延迟保存激活状态。
- 如果宿主没有 `VisualLayerManager`，可以 fallback 到 Avalonia 原生 `AdornerLayer`，但这是兼容路径，不是主路径。
- 原生 `AdornerLayer` fallback 使用相同的直接子项移动语义；无宿主的 inline manager 不参与窗口反馈层激活。
- 不要把 Badge、水印、Drawer、Tour、Popup、Dialog、ImagePreviewer 等迁入该层。
- 变更该层时，必须验证全局反馈层级和 Badge 局部装饰行为。

### TourLayer

`TourLayer` 是 Tour 控件专用的窗口遮罩层，用于绘制 spotlight mask 和目标区域洞穿。它是窗口级引导遮罩，不是通用 modal layer。

使用规则：

- 只用于 Tour。
- 其他控件需要 modal mask 时，应使用 popup overlay / dialog mask 体系，而不是复用 TourLayer。
- Tour 弹窗本体仍通过 popup overlay 路径定位，mask 和弹窗职责分离。

### FullscreenPopoverLayer

`FullscreenPopoverLayer` 是 Window 模板内部的固定层，用于 macOS 全屏场景下顶部标题栏 popover。它不是注入到 `VisualLayerManager` 的通用 layer。

使用规则：

- 只由 Window 模板和 Window 控件生命周期管理。
- 不得作为普通控件的 overlay 宿主。
- 不参与 Message、Notification、Popup、Badge、Drawer 等层级决策。

## 选择规则

| 场景 | 应使用 | 不应使用 |
|---|---|---|
| 全局 Message / Notification | `WindowFeedbackLayer` | `AdornerLayer`、`PopupOverlayLayer`、`ScopeAwareAdornerLayer` |
| 下拉、菜单、Tooltip、Flyout、选择器面板 | `PopupOverlayLayer` / `Popup.ShouldUseOverlayLayer` | `WindowFeedbackLayer` |
| Dialog / MessageBox overlay host | Window `OverlayLayer`；无 TopLevel 能力时使用局部 scope fallback | drawn decorations overlay、`PopupOverlayLayer`、平台硬编码 |
| ImagePreviewer overlay host | `OverlayLayerResolver` 获取 `PopupOverlayLayer` | 手动遍历窗口或主视图 |
| Count/Dot Badge | Avalonia `AdornerLayer` | `WindowFeedbackLayer`、`ScopeAwareAdornerLayer` |
| RibbonBadge | inline visual tree | 任意窗口级 layer |
| Drawer / Watermark / sticky mirror | `ScopeAwareAdornerLayer` | `WindowFeedbackLayer`、`PopupOverlayLayer` |
| FloatButton / 作用域浮动按钮 | `ScopeAwareOverlayLayer` | `PopupOverlayLayer`、`WindowFeedbackLayer` |
| Tour mask | `TourLayer` | Dialog mask、`WindowFeedbackLayer` |
| TreeView 拖拽预览 | Avalonia `AdornerLayer` | `WindowFeedbackLayer` |
| macOS 全屏标题栏 popover | `FullscreenPopoverLayer` | 通用 layer API |

## 层级顺序规则

- `WindowFeedbackLayer` 必须高于 `ScopeAwareAdornerLayer`，否则 Message / Notification 会被 sticky mirror 或作用域装饰遮挡。
- `TourLayer` 必须高于 `ScopeAwareAdornerLayer`，以保证引导遮罩覆盖作用域装饰。
- `ScopeAwareAdornerLayer` 可以高于 Avalonia 原生 `AdornerLayer`，这意味着原生 adorner 不应用于全局优先级覆盖。
- `ScopeAwareOverlayLayer` 是作用域 overlay，层级不应被用来表达全局优先级。
- Dialog 所在 `OverlayLayer` 必须低于 `LightDismissOverlayLayer` 与 `PopupOverlayLayer`，保证内容 popup 位于 Dialog 之上且外部点击可 light-dismiss。
- 新增 `int.MaxValue - N` 层级常量时，必须在本文档更新职责和相对顺序。不要裸写 `int.MaxValue` 或随机选择接近最大值的数字。

当前 AtomUI 自定义层级区间：

| 层 | ZIndex |
|---|---:|
| `WindowFeedbackLayer` | `int.MaxValue - 50` |
| `TourLayer` | `int.MaxValue - 98` |
| `ScopeAwareAdornerLayer` | `int.MaxValue - 99` |
| `ScopeAwareOverlayLayer` | `int.MaxValue - 990` |

## 生命周期与清理

- 所有动态加入 layer 的元素必须在 detach、close、dispose、template reapply 或 owner 变更时移除。
- `DialogOverlayLayer` 从第一个 presenter 开始持有实际 host/TopLevel size 订阅，并在最后一个 presenter 移除时解绑且从实际 host 删除。
- modal Dialog 与 Window Drawer 获取的 drawn chrome suppression 租约必须在 close、dispose、owner 变更和失败 teardown 中对称释放。
- 如果 layer child 设置了 adorned element、logical parent、事件订阅、timer、binding 或 resource host，释放路径必须和获取路径成对出现。
- 对 `WindowMessageManager`、`WindowNotificationManager` 这类 manager，宿主模板重套用时必须先从旧 host layer 移除，再重新安装。
- manager 激活不取得订阅、timer、cache 或额外强引用；`Move` 不得触发 manager 的 visual/logical detach。rehost 与
  `Dispose()` 继续通过既有 host collection 移除路径释放 manager。
- 对 `ScopeAwareAdornerLayer` / `ScopeAwareOverlayLayer` 注入式层，缓存 layer 时必须确认 visual parent 仍然有效。

## 测试要求

- 新增或改变 layer 归属时，应写层级回归测试，断言父层级而不是只断言 child `ZIndex`。
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
该状态必须与专项设计文档同步，不能把 Headless 共享路径通过解释为未执行平台或未执行控件的实机证据。

## 禁止做法

- 禁止为了让某个控件“显示在最上面”直接把它放入 `WindowFeedbackLayer`。
- 禁止通过提高 MessageCard、NotificationCard、Badge adorner 等 child 的 `ZIndex` 解决跨父层遮挡。
- 禁止把 Overlay Dialog 的 mask 与 Surface 拆成独立 popup 或不同生命周期 owner；二者必须位于同一个 presenter，并使用彼此独立的 motion actor 和几何约束。
- 禁止在每个控件中复制 TopLevel / MainWindow / SingleView 查找逻辑；应使用现有 resolver。
- 禁止新增 layer 后不更新本文档和回归测试。
