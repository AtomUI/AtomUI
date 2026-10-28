# Drawer 桌面版实现原理

本文档描述 Drawer 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Drawer 桌面版架构设计](overview.md)，变化记录见 [Drawer Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Drawer Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Drawer 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Drawer/Drawer.cs`
- `src/AtomUI.Desktop.Controls/Drawer/Drawer.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Drawer/DrawerContainer.cs`
- `src/AtomUI.Desktop.Controls/Drawer/DrawerInfoContainer.cs`
- `src/AtomUI.Desktop.Controls/Drawer/DrawerPlacement.cs`
- `src/AtomUI.Desktop.Controls/Drawer/DrawerToken.cs`
- `src/AtomUI.Desktop.Controls/Primitives/TopLevelMarginBinder.cs`
- `src/AtomUI.Desktop.Controls/Window/Utils/WindowVisualLayerClip.cs`
- `src/AtomUI.Desktop.Controls/Window/Utils/WindowDrawnDecorationsReflectionExtensions.cs`
- `src/AtomUI.Desktop.Controls/Drawer/Themes/DrawerContainerTheme.axaml`
- `src/AtomUI.Desktop.Controls/Drawer/Themes/DrawerInfoContainerTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `Drawer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `DrawerContainer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `DrawerInfoContainer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `DrawerToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Drawer 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Content`、`ContentTemplate`、`ExtraTemplate`、`FooterTemplate`、`Title`。
- 交互与状态：`IsCloseOnMaskClick`、`IsMotionEnabled`、`IsOpen`、`IsShowCloseButton`、`IsShowMask`。
- 视觉与布局：`DialogSize`、`Placement`、`PushOffsetPercent`、`SizeType`。
- 其他稳定入口：`Extra`、`Footer`、`OpenOn`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_CloseButton`：承载用户触发入口、导航或关闭动作。
- `PART_InfoContainer`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_Surface`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_Mask`：稳定模板协作入口，重命名前必须同步主题和实现。

### 语义部件基础设施

- 全部非 root 语义部件位于运行时创建的 `DrawerContainer` 内，统一 `CrossVisualRoot + RuntimeCreated`，marker 静态声明在 `DrawerContainerTheme.axaml`（`PART_Mask`）与 `DrawerInfoContainerTheme.axaml`（`Frame`、`InfoHeader`、`HeaderText`、`ExtraContentPresenter`、`InfoContainer`、`InfoFooter`、`PART_CloseButton`）。
- 容器可达性不变量：`AttachToLayer` 在 `layer.Children.Add` 之前调用 `((ISetLogicalParent)this).SetParent(drawer)`（Panel 不会覆盖已显式设置的逻辑父），`DetachFromLayer` 置空；owner 嵌套生成 Semantic Style 的命中依赖此不变量。
- `BindToDrawer` 显式中继 `ThemeVariantScope.ActualThemeVariant`（ImagePreviewer 先例），保证容器跟随 owner 主题变体。
- `Drawer : ISemanticPartCrossRootProvider`：容器挂层（`Open` 末尾）、`NotifyClosed`、`ReleaseDrawerContainer` 三处触发 `CrossRootsChanged`；`GetCrossRoots()` 仅在容器有视觉父时返回 `[container]`。
- `IsPinnedOpen`：`DrawerContainer.OnPointerReleased`（遮罩点击）与 `HandleCloseRequested`（关闭按钮）两处拦截，钉住时不落 `IsOpen=false`。

## 6. 交互与事件处理

Drawer 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 弹层、窗口或 overlay 类路径必须稳定处理打开、关闭、取消、重复打开和宿主失活。
- TopLevel Drawer 保留在 owning Window 的 `ScopeAwareAdornerLayer`，不读取 OS 类型或用 CSD 标志复制平台路由。
- 嵌套 Drawer 的 layer owner 是父 Drawer container 已进入的 scope layer；resolver 在排除不匹配的滚动宿主后复用当前包含层，打开和关闭必须在同一 layer 上成对完成。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 值提交或命令触发必须保持继承控件的事件顺序。

当前没有抽取到控件专属 public 事件；交互语义主要通过继承事件、命令、属性变化和 Gallery 可观察行为体现。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- 内容、命令和视觉状态在模板节点之间的同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。
- Window host 与 visible frame：Drawer container 使用 owning TopLevel 的 `ScopeAwareAdornerLayer`；root 与百分比尺寸统一使用排除 `FrameShadowThickness`、保留标题栏的 visible frame。
- Window Drawer 打开时获取 chrome suppression lease，关闭、释放或 host 变更时对称释放；多个 Dialog/Drawer 由 Window 引用计数防止提前恢复 drawn chrome。
- Drawer 内容 popup 沿 visual placement target 使用同一 TopLevel 的 Avalonia `LightDismissOverlayLayer` 与 `PopupOverlayLayer`。关闭时直接从 scope layer 移除 container，不创建内部 `VisualLayerManager` 或第二套 popup owner。
- 打开期间 `OpenOn` 变化时，`Drawer` 从新 target 重新解析 `ScopeAwareAdornerLayer`，`DrawerContainer` 在一次状态迁移中更新 adorned element、visible-frame 订阅和 chrome suppression lease，并把自身从旧 layer 移到新 layer。关闭必须优先从 container 的实际 visual parent 移除，不能再根据已变化的 `OpenOn` 猜测旧 host。
- 嵌套 Drawer 继承父 container 的 effective target；父 Drawer 的 `OpenOn` 迁移时，所有已打开子 Drawer 必须迁移到同一 scope，并释放旧 Window 的全部 suppression lease。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

### 动效目标与完成

`PART_Surface` 是静态 Border，只包裹实际面板，并按 Placement 使用有效 DialogSize 约束主轴。
面板 ±100% 位移以该表面的实际尺寸为基准，不使用全窗口或遮罩尺寸。其子节点的 push 变换保持独立。
`PART_Mask` 使用另一个 Motion 目标，保留原有遮罩范围、命中和 Token 背景。

面板采用对应边的 ±100%↔0、opacity .7↔1，遮罩采用 opacity 0↔1；两者使用 CSS ease 和有效 Slow 时长。
这是与参考规格的参数校准，区别于原先基础 Move 的 0↔1 和 CubicEaseOut。
两个播放请求在同一布局准备之后启动，统一等待真实播放结果；不通过 Task.Delay 猜测过渡结束。
首次显示的透明度准备使用作用域覆盖，反向时保留实际矩阵和透明度，由 Core 连续替代。
物理移除和 Opened/Closed 通知前核对本次版本、实际表面、业务 owner、挂载状态及最新 IsOpen。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 Drawer 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- Windows、Linux、macOS 以及 CSD/non-CSD 设计上使用同一 visible-frame 语义；平台差异只存在于 Window 如何发布 frame、titlebar 与 shadow metrics，不能把共享设计规则误写成尚未执行平台的测试证据。
- Drawer 内容、popup placement target 与 owning Window 必须保持在同一 `TopLevel`；不得通过 drawn decorations host、局部 ZIndex、延迟打开或强制 native popup 掩盖跨父层遮挡。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- Popup-bearing 内容集成变更复用 [Modal 内容弹层叠放设计](../modal/popup-layering-design.md) 的入口库存、原语和控件家族矩阵；Drawer 只额外验证自身 scope layer 与 chrome lease 生命周期。
- Drawer lifecycle 回归必须覆盖：Window target 到局部 target、Window 到另一 Window，以及父子 Drawer 同时打开时的 `OpenOn` 动态迁移；每条路径都断言旧 layer/lease 已释放且新 layer/lease 已建立。
- 共享 owning TopLevel popup 路径由 Modal Popup 原语、控件家族、DataGrid 与入口库存测试覆盖；Drawer 的容器生命周期由自动化回归直接覆盖，真实窗口证据按平台独立记录。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

本专项当前实机证据为 Windows 已测试、macOS 已测试；Linux X11/Wayland 未测试。未执行平台不得根据共享代码路径
或 Headless 结果推断为已通过。
