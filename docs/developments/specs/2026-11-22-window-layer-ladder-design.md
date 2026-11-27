# 全局视觉层级阶梯设计

日期：2026-11-22
状态：待审批（第二版：由"窗口级阶梯"扩展为"全局层级模型"）

## 1. 意图与动因

全屏标题栏（`FullScreenTitleBarHost`）在 Windows 实机上被 Gallery 吸顶 Tab 盖住（Space 页面滚动后把鼠标移到顶部，可稳定复现）。直接原因是：吸顶 Tab 被提升到窗口级 `ScopeAwareAdornerLayer`（`Max−99`），而全屏标题栏宿主位于 `PART_VisualLayerManager` 的内容根内，低于所有注入层和 Avalonia 原生层。

全局盘点后确认，这是层级体系本身缺少全局规则的一个表现。同一类缺陷还有：

1. **没有自有的层级阶梯。** AtomUI 的号码是散落的常量，紧贴 Avalonia 的固定号码。`AdornerLayer`(−100) 与 `OverlayLayer`(−98) 之间只剩 −99 一个整数，新的层插不进去。`TourLayer` 与 Dialog 同为 −98，先后取决于插入顺序。
2. **按类分层而不是按角色分层，一个类混放了多种职责。**
   - 窗口级 `ScopeAwareAdornerLayer` 同时承载内容覆盖（吸顶 Tab、Watermark）和呈现（Drawer）。
   - `ScopeAwareOverlayLayer` 同时承载内容浮动（FloatButton）和作用域 Dialog。
3. **全屏标题栏是窗口 chrome，却没有登记为层。**
4. **提升层没有专属装饰层。** Avalonia 的 `OverlayLayer` 自带专属 `AdornerLayer`，`AdornerLayer.GetAdornerLayer` 会沿祖先链找最近的 `OverlayLayer` 或 `VisualLayerManager`。但 AtomUI 的提升层（Drawer、Message/Notification、吸顶 Tab、FloatButton）都没有专属装饰层。它们内部控件的键盘焦点描边、Badge 会落到共享的 `AdornerLayer`（−100），而这些层都在它之上，所以描边会被层自身的表面盖住。*（这一条是按代码路径推出的，尚未实测；实现第一步先实测确认，见第 10 节。）*

本设计建立一套**全局层级模型**：先定规则，再由规则推出号码。所有叠放宿主遵循同一套角色顺序。

## 2. 目标、非目标、反目标

**目标**

- 定义一套在所有叠放宿主中都成立的角色顺序：Window、PopupRoot、OverlayPopupHost、EmbeddableControlRoot、浏览器 Gallery shell、嵌套 `VisualLayerManager`、ScrollViewer 作用域宿主。号码集中在一处定义，各级之间留有间隔。
- 全屏标题栏位于所有内容角色之上，位于所有窗口级呈现和反馈之下。
- 每个能承载交互控件的提升表面，其装饰（焦点描边、Badge）都画在该表面之上、下一个更高表面之下。
- 消除混放：每个层类只承担一种角色。
- Tour 与 Dialog 的先后由号码决定。

**非目标**

- 不修 bug 1（"首次划到顶部出现又消失"，未复现），另行跟踪。
- 不改 Avalonia `OverlayLayer` / `LightDismissOverlayLayer` / `TextSelectorLayer` / `PopupOverlayLayer` 的号码（Max−98..−95）。
- 不改控件内部兄弟元素之间的局部 ZIndex（SplitButton、CompactSpace、AddOnDecoratedBox、Slider、DataGrid、Tab 拖拽、Steps、Avatar、SplitView、Calendar、ColorPicker 滑块、FeedbackStackPanel 卡片排序等）。它们只在本控件内部排序，不参与全局阶梯。
- 不改 AtomUI 从不注入层的 `VisualLayerManager`。
- 不改全屏标题栏的显隐状态机、模式和几何。

**反目标（即使"能跑"也算失败）**

- 用局部 ZIndex、同号插入顺序、延迟插入或提高 child ZIndex 来解决跨层遮挡。（唯一记录在案的例外：同一宿主的作用域 Drawer 与作用域 Dialog 共用一个 T5 实例，二者按打开顺序排列，后打开的在上，与 antd DOM 追加顺序一致；这是同一角色内的先后，不是跨层优先级。）
- 某一类宿主（例如 PopupRoot、浏览器 shell）中的相对顺序与 Window 不一致。
- 第 7 节行为变化清单以外的相对顺序发生改变。
- 提升表面内部控件的焦点描边仍被表面自身盖住。
- 引入新的反射或 AOT 不安全路径。
- 破坏 Drawer 的 public / protected API。

## 3. 约束（已实测并与源码核对）

以下内容在本仓库实际引用的 Avalonia 12.1.6 上，用 Headless 临时探针实测确认，并与 GitHub 官方仓库 tag `12.1.6` 的源码交叉核对（`src/Avalonia.Controls/Primitives/VisualLayerManager.cs`、`AdornerLayer.cs`，检索日期 2026-11-22）：

- `VisualLayerManager` 是 sealed 类，`AddLayer` 与各层 getter 都是 non-public。AtomUI 已有 `VisualLayerManagerReflectionExtensions.AddLayer`（反射调用，已登记）。
- 内置层懒创建：首次访问 getter（含公开的 `AdornerLayer.GetAdornerLayer`）时才创建，`ZIndex` 只在私有 `AddLayer` 中赋值一次（Adorner −100、Overlay −98、LightDismiss −97、TextSelector −96、PopupOverlay −95），此后再没有任何回写。实测把 `AdornerLayer.ZIndex` 改成 Max−900 后，经过换内容、开关 overlay Popup、FullScreen 往返，值都保持不变，实例也没变。
- `AdornerLayer.GetAdornerLayer(visual)` 沿 self-and-ancestors 查找第一个 `OverlayLayer { AdornerLayer: not null }` 或 `VisualLayerManager`（取其 `AdornerLayer`），找不到时退回 TopLevel 下第一个 `VisualLayerManager`。
- 嵌套 `VisualLayerManager` 默认只启用 `AdornerLayer`（Overlay/PopupOverlay 为 false）。嵌套宿主内的元素：`GetAdornerLayer` 解析到嵌套层；`OverlayLayer.GetOverlayLayer` 解析到外层窗口；`ScopeAwareAdornerLayer.GetLayer` 解析到嵌套宿主。
- Avalonia 中 ZIndex 只比较同一父级下的兄弟元素。跨宿主的先后由宿主在树中的位置决定。
- ScrollViewer 模板（`ScrollViewerTheme.axaml`、`AvaScrollViewerTheme.axaml`）：`ScopeAwareOverlayLayerPanel#PART_ContentHost` 包住 `ScrollContentPresenter` 和滚动条。作用域 `ScopeAwareAdornerLayer` 被注入到 `ScrollContentPresenter` 的内容包裹面板里，随滚动内容一起移动；作用域 `ScopeAwareOverlayLayer` 被注入到 `PART_ContentHost`，固定在视口上。
- 项目约束：ControlTheme 优先（AGENTS.md）、AOT 一等约束、C# 控制流必须带 `{}`、测试遵循 temporary-first 生命周期。

## 4. 方案比较

| 方案 | 内容 | 取舍 |
|---|---|---|
| **A（选定）** | AtomUI 自有阶梯；只把 Avalonia `AdornerLayer` 下移；按角色分层；新增 chrome 层与呈现层；提升表面自带装饰层 | 解决全部缺陷；只改写 Avalonia 一个层的号码，且不改变它与 Avalonia 其他层的相对顺序。代价：依赖"Avalonia 不回写 ZIndex"，用守护测试兜底 |
| B | 同 A，但 Avalonia 五个内置层全部重新编号 | 号码更整齐，但改写 LightDismiss / Popup 等依赖相对顺序的层没有收益，只有风险 |
| C | 不动 Avalonia，在 Window 模板内容根下加内容装饰层 | Badge、焦点描边仍会盖住标题栏，不是源头修复 |

用户选择 A，并要求它在全局范围内成立。本版即 A 的全局化。

## 5. 全局层级模型

### 5.1 五条规则

1. **角色顺序（每个宿主都相同）：** 第 5.2 节定义 T1–T8 共 8 个角色。任何叠放宿主中，存在的角色都按这个顺序排列；宿主中没有的角色直接缺省（例如只有 Window 宿主有 T6）。
2. **嵌套：** 一个作用域宿主（ScrollViewer 的两个注入点、嵌套 `VisualLayerManager`、**提升表面**）整体属于父宿主的某个角色。它内部的全部角色，都低于父宿主中比该角色更高的所有角色。例如：
   - ScrollViewer 滚动内容里的 Drawer 低于该 ScrollViewer 视口上的 FloatButton，也低于窗口 chrome；
   - Window Drawer 里的 FloatButton、Watermark、作用域 Dialog 留在这个 Drawer 表面之内，不会掉到窗口的 T4/T5，也就不会被 Drawer 自己盖住。

   **内容角色（T3–T5）就近解析**：落在最近的作用域宿主，提升表面也算。**TopLevel 角色（T6–T8）一律解析到 TopLevel 的宿主**，跳过所有作用域宿主。*（我的判断：这是规则 2 对提升表面的直接推广。）*
3. **装饰跟着表面走：** 共享 `AdornerLayer`（T2）只服务 T1 内容。T3 及以上，每个**能承载可聚焦或可装饰控件的提升表面**，必须自带一个只启用 `AdornerLayer` 的嵌套 `VisualLayerManager`（"表面宿主"），使表面内控件的装饰紧贴在该表面之上。这是对 Avalonia `OverlayLayer` 自带装饰层做法的对齐。纯绘制、不含交互控件的表面（Watermark、Tour 遮罩）不需要。
4. **一个层类只承担一种角色：** 角色决定层，消费者按角色选层，不再按"是不是 adorner"选层。
5. **ZIndex 只排同宿主内的兄弟层：** 跨宿主的先后由规则 2 决定。树外的两类按各自的机制处理：`WindowDrawnDecorations` overlay（TopLevel 的同级子树）继续由 Dialog/Drawer 的抑制租约控制；原生 popup 窗口由操作系统排序。控件内部的局部 ZIndex 不得用来表达跨层优先级。

### 5.2 角色阶梯与号码（自下而上，均为 `int.MaxValue − N`）

| 角色 | N | 层 | 承载 | 变化 |
|---|---:|---|---|---|
| T1 内容 | ZIndex 0 | 宿主的内容子项 | 内容本身 | 不变 |
| T2 内容装饰 | 1000 | Avalonia `AdornerLayer`（共享） | T1 内容的 Badge、焦点描边、TreeView 拖拽预览 | 由 100 下移 |
| T3 内容覆盖 | 900 | `ScopeAwareAdornerLayer` | Watermark、Gallery 吸顶 Tab | 由 99 下移；不再承载 Drawer |
| T4 内容浮动 | 800 | `ScopeAwareOverlayLayer` | FloatButton / FloatButtonGroup | 由 990 上移到 T2 之上；不再承载 Dialog |
| T5 内容呈现 | 700 | **`ContentPresentationLayer`**（新） | 作用域 Drawer、作用域 Dialog（`OverlayScope`） | 新增 |
| T6 窗口 chrome | 600 | **`WindowChromeLayer`**（新，仅 Window 宿主） | 全屏标题栏 | 新增 |
| T7 窗口呈现 | 500 | **`TopLevelPresentationLayer`**（新） | TopLevel Drawer（含 Window Drawer） | 新增 |
| T7 窗口呈现 | 400 | `TourLayer` | Tour 遮罩 | 由 98 下移 |
| T7 窗口呈现 | 98 / 97 / 96 / 95 | Avalonia Overlay / LightDismiss / TextSelector / PopupOverlay | 窗口 Dialog、light-dismiss、文本选择、overlay 弹层 | 不变 |
| T8 窗口反馈 | 50 | `WindowFeedbackLayer` | Message / Notification | 不变 |

每一级之间都留了空隙，供后续扩展。T7 内部的先后：Window Drawer < Tour < Dialog < LightDismiss < TextSelector < 弹层。

### 5.3 宿主矩阵

| 宿主 | 出现的角色 | 说明 |
|---|---|---|
| AtomUI Window `PART_VisualLayerManager` | T1–T8 | 唯一有 T6 的宿主 |
| PopupRoot / OverlayPopupHost / EmbeddableControlRoot 的 `VisualLayerManager`，浏览器 Gallery shell 自建的 `VisualLayerManager`，Gallery Tour 舞台的嵌套 `VisualLayerManager` | T1–T5、T7、T8（按需） | 号码与 Window 相同 |
| 表面宿主（规则 3 的嵌套 `VisualLayerManager`，包括 chrome 层内的那一个） | T1–T5 | 是内容角色的作用域宿主；TopLevel 角色（T6–T8）的解析跳过它（见 5.7） |
| ScrollViewer `ScrollContentPresenter` 的内容包裹面板（随滚动） | T1、T3、T5 | 作用域 Drawer 放在这里的 T5，不在 T3 |
| ScrollViewer `ScopeAwareOverlayLayerPanel#PART_ContentHost`（视口固定） | T1（滚动区与滚动条）、T4、T5 | 作用域 Dialog（`OverlayScope` 落在这里时）放在 T5，高于 FloatButton |
| 树外：drawn decorations overlay、原生 popup 窗口 | — | 按规则 5 处理 |

同一个号码表适用于全部宿主类型，按宿主类型分号码是被禁止的。

### 5.4 消费者归属表

| 消费者 | 角色 | 解析方式 |
|---|---|---|
| Count/Dot Badge、键盘焦点描边、TreeView 拖拽预览 | T2（或所在表面的装饰宿主） | Avalonia `GetAdornerLayer`，不变 |
| Watermark | T3 | `ScopeAwareAdornerLayer.GetLayer`，不变 |
| Gallery 吸顶 Tab | T3（窗口宿主） | `GalleryStickyTabsHost.ResolveStickyElevationLayer`，不变；吸顶 Tab 本身改为带表面宿主 |
| FloatButton / FloatButtonGroup | T4 | `ScopeAwareOverlayLayer.GetLayer`，不变；FloatButton 表面带装饰宿主 |
| Drawer：`OpenOn is TopLevel`（TopLevel Drawer。没有父 Drawer 时，`OnAttachedToVisualTree` 会把 `OpenOn` 绑定到视觉祖先 TopLevel；其中 `OpenOn is Window` 的那部分会额外获取 Window chrome 抑制租约） | T7 `TopLevelPresentationLayer`，位于该 TopLevel 的宿主 | 新路由；对 Window、PopupRoot、EmbeddableControlRoot、浏览器 shell 一视同仁 *（我的判断：按 TopLevel 而不是按 Window 判定，所有宿主的先后才一致）* |
| Drawer：其他情况（`OpenOn` 为 null 或指向局部元素） | T5 `ContentPresentationLayer`，位于现在解析出的同一个作用域宿主 | 作用域宿主的解析规则（滚动面板 / 包含层 / 窗口宿主）不变，只是把落点从 T3 改成同宿主的 T5 |
| 嵌套 Drawer | 复用父 Drawer 所在的层 | 现有的"复用包含层"规则，对两个新呈现层同样适用 |
| Dialog：默认 | T7 Avalonia `OverlayLayer` | 不变（Avalonia 自带专属装饰层） |
| Dialog：`OverlayScope` 指定作用域 | T5 `ContentPresentationLayer`（作用域宿主） | 由 `ScopeAwareOverlayLayer` 改为 T5 |
| Dialog：无 TopLevel 的 fallback | T5 `ContentPresentationLayer` | 由 `ScopeAwareOverlayLayer` 改为 T5 |
| Tour 遮罩 | T7 `TourLayer` | 解析规则不变，但跳过表面宿主 |
| Message / Notification | T8 | 不变；每个 manager 的卡片宿主带表面宿主 |
| 全屏标题栏 | T6 | 见 5.6 |
| ImagePreviewer、下拉、菜单、Tooltip、Flyout | T7 PopupOverlay 或原生 popup | 不变 |

### 5.5 号码真源与统一注入入口

- 在 `AtomUI.Controls`（`Primitives/VisualLayers/`）新增一个 internal 静态类，作为**唯一号码真源**，定义 5.2 节的全部号码，包括对 Avalonia `AdornerLayer` 的目标号码。Desktop 层用到的号码也定义在这里，保证阶梯在一处可读。现有分散常量全部改为引用它。`VisualLayerManagerUtils.ScopeAwareAdornerLayerZIndex` / `ScopeAwareOverlayZIndex` 两个 internal 名字保留，改为转发到新值，因为 Mobile 仓库对 `AtomUI.Controls` 有 internal 可见性，可能引用它们。*（我的判断）*
- 同一个类提供**唯一的注入入口**：向任意宿主（`VisualLayerManager`、滚动内容包裹面板、`ScopeAwareOverlayLayerPanel`）加入角色层时都必须经过它。对 `VisualLayerManager` 宿主，它先确保 Avalonia `AdornerLayer` 已创建（通过公开 API `AdornerLayer.GetAdornerLayer`）并被设为 T2 号码（幂等），再调用已有的 `AddLayer` 反射扩展。对 Panel 宿主，它以同一号码作为子项 ZIndex 加入。现有的全部注入路径（`ScopeAwareAdornerLayer.InjectLayer`、`ScopeAwareOverlayLayer` 注入、`TourLayer`、`WindowFeedbackLayer`）都改走这个入口，新增的三层也一样。
- AtomUI Window 取得 `PART_VisualLayerManager` 时（模板应用），也调用一次"确保 T2 号码"，保证还没注入任何层时 Badge 已在阶梯位置。模板重套用时对新宿主重复执行。
- 不新增反射。Avalonia 不回写 ZIndex（第 3 节），因此不需要持续监听，由守护测试兜底（第 10 节）。

### 5.6 窗口 chrome 层与全屏标题栏

- `WindowChromeLayer`（Desktop 包，internal，Panel，透明，自身不参与命中）由 Window 在模板应用时经统一入口注入 `PART_VisualLayerManager`。模板重套用和窗口关闭时成对移除。
- 层内是一个表面宿主（只启用 Adorner 的嵌套 `VisualLayerManager`），它的 Child 是 `FullScreenTitleBarHost`。这样标题栏按钮的焦点描边和标题栏内的 Badge 都在 chrome 层自己的装饰层里；标题栏打开的 Dialog 仍解析到窗口的 `OverlayLayer`（已实测）。
- **宿主仍在三个 Window 模板中声明**，保留 ControlTheme 与 `TemplateBinding` 契约（Mode、IsOpen、SurfaceMargin、Padding、Content、ContentTemplate）。Window 在 `OnApplyTemplate` 中把它从内容根移入 chrome 层的装饰宿主，模板解除时释放。*（我的判断：保持 ControlTheme 优先。实现第一步先用临时测试确认重新挂接后 `TemplateBinding` 仍然生效；若不成立，退回为由 Window 代码创建宿主并做代码绑定，在注释中写明 ControlTheme 不可行的原因，并同步更新标题栏设计文档的 6.4 契约。）*
- 几何：chrome 层铺满 `VisualLayerManager`。实现者须确认迁移前后宿主在窗口坐标中的位置一致：三个模板中宿主现在分别位于 `WindowVisualLayerClip`、`OffScreenMargin`（作用于 `VisualLayerManager` 的 Child，不作用于层）、`FrameShadowThickness` 等边距容器内。全屏状态下表面顶端在窗口坐标中的 Y 值必须与迁移前相同。

### 5.7 表面宿主（提升表面自带的作用域宿主）

- 形态：只启用 `AdornerLayer` 的嵌套 `VisualLayerManager`，作为提升表面视觉根的直接包裹，**每个表面一个**，不是每个层一个。这样同一层中较低表面的装饰，不会画到较高的兄弟表面之上。
- 需要它的表面：`DrawerContainer`、作用域 `OverlayDialogPresenter`（窗口级 Dialog 已由 Avalonia `OverlayLayer` 提供，不需要）、FloatButton / FloatButtonGroup 的浮动宿主、`WindowFeedbackLayer` 中每个 manager 的卡片宿主、Gallery 吸顶 Tab 宿主、全屏标题栏宿主。尽量在各自的 ControlTheme 模板中声明；无法声明时（宿主由代码创建），在代码中创建，并按 AGENTS.md 写明原因。
- **解析器按角色区分（规则 2）：**
  - 内容角色的解析器（`ScopeAwareAdornerLayer.GetLayer`、`ScopeAwareOverlayLayer.GetLayer`、`ContentPresentationLayer` 的解析、`DialogOverlayLayer` 的作用域解析）把表面宿主当作普通作用域宿主，就近落入其中。
  - TopLevel 角色的解析器（`TopLevelPresentationLayer`、`TourLayer`、`WindowFeedbackLayer`，以及 popup light-dismiss 层）在向上查找 `VisualLayerManager` 时跳过带标记的表面宿主，直达 TopLevel 宿主，否则 Tour 遮罩、TopLevel Drawer 会被关进某个表面里，表面内 popup 的 light-dismiss 层也只铺满该表面。Gallery Tour 舞台那种刻意嵌套、没有表面宿主标记的 `VisualLayerManager` 不受影响，仍按现有规则作为 Tour 的作用域。`WindowChromeLayer` 不经解析器，由 Window 在模板应用时直接注入 `PART_VisualLayerManager`（见 5.6）。
  - light-dismiss：Avalonia `Popup.Open()` 按 placement target 最近的 `VisualLayerManager` 取 `LightDismissOverlayLayer`，AtomUI 不改 Avalonia 行为，而是在 AtomUI `Popup` 的既有兼容边界 `PopupLightDismissRegistration` 中，于 `Opened` 时检测落点是否属于表面宿主，是则把租约与处理器改挂到 TopLevel 角色宿主的 dismiss 层（复用已有反射成员，不新增反射）。
  - 标记方式由实现者选定（例如一个 internal attached property），统一由号码真源类提供判定方法。
- **"包含层"复用规则收窄：** `ScopeAwareAdornerLayer.GetLayer` 中"已位于某个 `ScopeAwareAdornerLayer` 内则复用它"这一步只匹配精确类型的 T3 实例，不匹配呈现层子类。否则表面内的 Watermark 会以兄弟身份落进 T5/T7，画到更高的 Drawer 之上。嵌套 Drawer 复用父 Drawer 所在呈现层，属于 Drawer 自己的现有规则（`FindParentDrawer` 与有效 target 继承），由 Drawer 显式处理，不依赖这一步。
- Avalonia 自身的解析行为符合预期：`GetAdornerLayer` 解析到表面宿主（正是目的）；`GetOverlayLayer` 与 overlay popup 解析到外层（嵌套宿主未启用 Overlay/PopupOverlay）。实现者须用测试确认 overlay popup 的宿主解析同样落到外层。

### 5.8 呈现层

- `ContentPresentationLayer`（T5）与 `TopLevelPresentationLayer`（T7）都继承 `ScopeAwareAdornerLayer`，都放在 `AtomUI.Controls`，因为 PopupRoot、EmbeddableControlRoot、浏览器 shell 同样需要它们。Drawer 的签名 `NotifyBeforeOpen(ScopeAwareAdornerLayer)` / `NotifyBeforeClose(ScopeAwareAdornerLayer)` 保持不变。Drawer 中 `GetType() != typeof(ScopeAwareAdornerLayer)` 这类精确类型判断改为 `is` 判断。`ScopeAwareAdornerLayer` 自己查找 T3 层时（`FindAdornerLayer`）必须只返回精确类型的 T3 实例，不能误取呈现层子类。
- `ContentPresentationLayer` 同时作为作用域 `DialogOverlayLayer` 的宿主。`DialogOverlayLayer` 要求宿主是一个 Panel，并把自己铺满作用域；实现者需保证它在继承自 Canvas 的呈现层中仍然铺满作用域、几何不变。如果做不到，`ContentPresentationLayer` 可以改为内部组合两个子容器（Drawer 子层 + Dialog 子层，Dialog 子层在上），但对外仍是同一个 T5 角色。*（留给实现者，约束如上。）*
- Drawer 的 `OpenOn` 动态迁移（Window target ⇄ 局部 target、跨 Window、父子 Drawer 同时迁移）按现有规则在 T5 与 T7 之间同样适用：一次状态迁移中移动 container、交接 lease、释放旧层。
- Drawer 的 drawn chrome 抑制租约不变：AutoHide 标题栏在 Window Drawer 或 Dialog 活跃时仍然关闭；Pinned/Docked 模式下由层级保证 Window Drawer 和 Dialog 盖住标题栏。

## 6. 跨仓库与兼容

- **Mobile 仓库（用户决定：后续跟进）：** 本次不改 AtomUI.Mobile 仓库。新层（`ContentPresentationLayer`、`TopLevelPresentationLayer`）、号码真源与统一注入入口保持 internal，Mobile 通过既有的 internal 可见性使用。changelog 写清迁移说明：
  - Avalonia `AdornerLayer` 号码的变化及其不变量（仍高于内容，仍低于全部 AtomUI 提升层）；
  - T3/T4 先后对调；
  - 呈现类表面（抽屉、ActionSheet、对话框等）应迁到 T5/T7 呈现层，并自带表面宿主，否则升级后会落到 FloatButton 之下。

  Mobile 的适配在 Mobile 仓库另开任务完成。
- 第三方如果自行向 `VisualLayerManager` 注入层，并依赖 `AdornerLayer` 为 −100，会受影响。changelog 中注明。
- Drawer、Dialog、FloatButton、Watermark、Message、Tour 的 public API 均不变。

## 7. 行为变化清单（除此之外，相对顺序都不得改变）

1. 全屏标题栏位于内容、Badge、焦点描边、Watermark、吸顶 Tab、FloatButton、作用域 Drawer/Dialog 之上；位于 Window Drawer、Tour、Dialog、弹层、Message 之下。
2. FloatButton 从共享 `AdornerLayer` 下方移到它上方：内容中的 Badge 和焦点描边不再画在 FloatButton 之上。FloatButton 仍低于同宿主的作用域 Drawer/Dialog。
3. 作用域 Drawer 从 T3 移到 T5：它位于同宿主的 Watermark、吸顶 Tab、FloatButton 之上。
4. 作用域 Dialog（`OverlayScope`）从 `ScopeAwareOverlayLayer` 移到 T5：它位于同宿主的 FloatButton 之上（目前与 FloatButton 同层，先后取决于插入顺序）。
5. TopLevel Drawer（含 Window Drawer）从 T3 移到 T7：在 Window 中位于全屏标题栏之上；在所有宿主中位于作用域 Dialog 之上、TopLevel Dialog 之下。
6. 同一宿主中，FloatButton（T4）从 Watermark 和吸顶 Tab（T3）之下移到它们之上，与 Ant Design 一致（watermark 9 < affix 10 < FloatButton 1000）。
7. 作用域 Dialog 与无 TopLevel 时的 fallback Dialog（T5）从 Watermark 之下移到它之上。
8. 提升表面内部的 Watermark、FloatButton、作用域 Dialog、作用域 Drawer 留在该表面内（规则 2），不再落到窗口层级而被表面盖住。
9. Tour 固定在 Dialog 之下。已知后果：在已打开的模态 Dialog 内启动 Tour 时，遮罩会落到 Dialog 之下。Tour 文档明确说明 Tour 不负责 Dialog，Gallery 也没有这种用法，所以视为不受支持的偶然行为，在 Tour 的 changelog 中记录。
10. Drawer、Message/Notification、FloatButton、吸顶 Tab、全屏标题栏内控件的焦点描边和 Badge，改为画在各自表面之上（修复第 1 节第 4 条；不论实测结果如何都实施，见第 10 节）。

明确**不变**的：
- T3 层内吸顶 Tab（−1）在 Watermark 之下。
- ScrollViewer 滚动内容中的 Drawer 低于该 ScrollViewer 视口上的 FloatButton（由规则 2 推出）。
- Avalonia 四个 overlay 层的号码与行为。
- Message/Notification 始终在最上。

## 8. 文档与契约更新

- `docs/architecture/systems/rendering/visual-layers.md`：按本模型重写层清单、号码表、规则、选择规则表和测试要求；加入五条规则、宿主矩阵和表面宿主。
- `docs/controls/desktop/window/window/full-screen-title-bar-design.md`：6.1 模板结构与层级位置、6.4 稳定契约、第 10 节验证。
- Drawer、Dialog、FloatButton、Tour、Message/Notification、Watermark、Window 的 implementation 文档中涉及层归属的段落，以及它们的 changelog（按 changelog 规范）。

## 9. 留给实现者

- 号码真源类、三个新层类、表面宿主标记的命名与文件位置（须遵守模块边界：号码真源、`ContentPresentationLayer`、`TopLevelPresentationLayer` 放在 `AtomUI.Controls`，因为非 Window 的 TopLevel 宿主同样需要它们；`WindowChromeLayer` 放在 `AtomUI.Desktop.Controls`）。
- 5.6 节中宿主迁入 chrome 层后的偏移与裁剪如何等价还原。
- 5.8 节中 `DialogOverlayLayer` 在呈现层中的铺满方式。
- 各表面宿主在 ControlTheme 中的具体声明位置。
- 测试的去留按 test-value-and-lifecycle 规范判断（见第 10 节）。

## 10. AOT、性能与验证

**AOT 与性能**
- 不新增反射：`AdornerLayer` 通过公开 API 创建，注入沿用已登记的 `AddLayer` 反射扩展。
- 表面宿主是懒创建的 `VisualLayerManager`，只有在表面内有装饰时才创建 `AdornerLayer`，每个表面多一层视觉节点，性能影响需用 Message/Notification 快速堆叠的场景确认，不得出现可测量的退化。

**验证**
- **第一步实测（Temporary）：** 记录第 1 节第 4 条的修复前证据（Window Drawer 内按钮的焦点描边、Drawer 内 Badge 被 Drawer 表面盖住）。实测结果只作为证据，**不决定规则 3 是否实施**：Drawer 内 Badge 落到共享 `AdornerLayer` 是确定的代码路径，规则 3 是全局模型的一部分，六类表面全部实施。*（我的判断）* 同时：确认 `TemplateBinding` 在宿主重新挂接后仍然有效；确认 overlay popup 从表面宿主内打开时解析到外层。
- **守护测试（Headless，常驻候选）：** 对每一类宿主（Window、PopupRoot、OverlayPopupHost、EmbeddableControlRoot、ScrollViewer 两个注入点）断言角色层按 5.2 节排序（断言父层归属和号码，不只断言 child ZIndex），并在 Popup 开关、FullScreen 往返、模板重套用后仍然成立。它同时守护"Avalonia 不回写 ZIndex"，具有独立的长期价值；最终去留按规范评估。
- **归属回归：** 按 5.4 节逐个消费者断言其落层；嵌套 Drawer 复用包含层；`OpenOn` 在 T5 与 T7 之间迁移时，旧层和 lease 被释放；Tour 与作用域层不会被关进表面宿主。
- **遮挡与装饰：** 全屏标题栏打开时，标题栏区域的 `InputHitTest` 命中标题栏，而不是吸顶 Tab、Badge、Watermark 或 FloatButton；Dialog 打开时命中 Dialog 遮罩；Drawer、Message、标题栏内按钮的键盘焦点描边可见，且落在各自表面的装饰宿主中。
- **实机视觉：** 用 Windows 实机脚本复现原 bug（Space 页滚动后鼠标移到顶部），修复前后各取一组截帧证据（媒体文件只放临时目录，结论写入记录）；另外各留一组 Drawer 内焦点描边、FloatButton 与 Badge 叠放的截帧。macOS / Linux 未实测时如实标注。
- **模块验证：** 对改动模块运行 focused 验证（`run --scope iterate --path`，或精确的 `dotnet test --filter`）。本机 `python3` 不可用时用 `dotnet test`。不主动跑全量回归。
