# Modal 桌面版架构设计

本文档定义 `Dialog` 和 `MessageBox` 的当前公共设计。内部状态机与宿主实现见 [Modal 桌面版实现原理](implementation.md)，公共 Semantic Part 契约见 [Modal / Dialog Semantic Part 契约](semantic-part.md)，关闭动效的视觉层与生命周期边界见 [Modal Dialog 关闭动效设计](dialog-close-motion-design.md)，宿主尺寸与交互缩放见 [Modal 宿主尺寸与 Resize 设计](host-sizing-design.md)，内容区弹层叠放见 [Modal 内容弹层叠放设计](popup-layering-design.md)，视觉变量见 [Modal Token 设计](token.md)，历史变化见 [Modal Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Feedback/Modal` |
| 控件状态 | Stable |

`Dialog` 用于需要独立内容表面、明确结果和受控关闭流程的桌面对话。它支持 Overlay 与原生 Window 两种宿主、modal 与 modeless 两种交互模式、声明式 `IsOpen` 和静态异步 API。

`MessageBox` 是 `Dialog` 的语义专化，用于信息、成功、警告、错误和确认消息。它复用 Dialog 的 Session、Presenter、关闭、尺寸、资源和焦点模型，不维护第二套隐藏 Dialog。

Modal 不承担通知队列、轻量 Tooltip、Popup 菜单或业务级导航服务职责。

## 2. 设计语言

- 对话表面由标题、内容、Footer 和操作按钮组成，Overlay 与 Window 共享同一个内容模型。
- modal 通过 mask 或原生 owner 关系阻断底层输入；modeless 保持底层可交互。
- Presenter 的真实展示边界属于 Session 生命周期的一部分。Overlay 在入场 motion 完成后触发 `Opened`，Window 在原生 `DialogWindow.Opened` 后触发；关闭任务分别等待 Overlay 退出 motion 或原生 `DialogWindow.Closed`，并在宿主移除和资源释放完成后结束。
- Overlay modal mask 打开时直接显示，关闭期间保持固定透明度，随 presenter 移除；不执行淡入淡出。
- Overlay 关闭时，外层 Surface motion 与 `DialogSurface` 内容层的前景 opacity 动画并行；内容和按钮保持附着，直到所有关闭任务完成后才 teardown。Window 继续使用原生 Window 生命周期。
- `MessageBoxStyle` 只表达消息语义和默认图标/按钮策略，不改变 Dialog 生命周期。

## 3. API 与契约模型

### 3.1 Dialog 契约组

| 契约组 | 代表成员 | 语义 |
| --- | --- | --- |
| 内容 | `Title`, `TitleIcon`, `Content`, `ContentTemplate`, `DataContext` | 定义标题和任意内容对象或模板。 |
| 打开状态 | `IsOpen`, `OpenAsync(...)` | `IsOpen` 是默认 TwoWay 的声明式意图；`OpenAsync` 表示一次完整 Session。 |
| 展示方式 | `DialogHostType`, `IsModal`, `PlacementTarget`, `OverlayScope`, startup anchor/offset | 选择 Overlay/Window、交互模态、初始位置与 overlay 宿主作用域。直接实例化与静态 API 的水平、垂直 startup anchor 默认均为 `Center`；显式 `Custom` 时由对应 offset 决定位置。`OverlayScope` 默认 `null` 表示宿主解析到 owning TopLevel；指定后 mask 与正文尺寸限定在该作用域内（对齐上游内联模态语义），仅 Overlay 宿主有效。 |
| 尺寸与窗口能力 | `HostWidth/Height/Min/Max`, `IsResizable`, `IsClosable`, `IsMaskClosable`, `IsDragMovable`, `IsMaximizable`, `IsMinimizable`, `IsTopmost` | 同一组 Surface 正文尺寸请求映射到 Overlay 或原生 Window。初次 `NaN` 选择自然尺寸；Overlay 未被显式请求或 resize 接管的自然轴持续参与布局。有效最小尺寸还必须满足 Dialog 的结构性下限。`IsClosable` 控制标题栏关闭入口，`IsMaskClosable` 控制 Overlay modal mask 外点关闭入口，两者正交且默认都为 `true`。 |
| 操作 | `StandardButtons`, `CustomButtons`, `DefaultStandardButton`, `EscapeStandardButton`, `ButtonsConfigure` | 生成标准按钮、加入自定义按钮并配置当前有效按钮序列。 |
| 状态与策略 | `IsLoading`, `IsConfirmLoading`, `IsFooterVisible`, `IsMotionEnabled`, `IsPinnedOpen`, `BeforeCloseAsync` | 控制加载、确认按钮 loading、Footer、motion、预览钉住和关闭前校验。 |
| 结果 | `Result`, `Accept()`, `Reject()`, `Done(...)` | 所有关闭来源归一为结果与 `DialogCloseReason`。 |

静态入口只有异步形式：

- `ShowDialogAsync(...)` 创建 modeless Dialog。
- `ShowDialogModalAsync(...)` 创建 modal Dialog。
- 两者都在完整 teardown 后返回 `Task<object?>`。

同一个实例存在任何尚未进入 `Closed` 的 Session 时，再次调用 `OpenAsync` 会抛出 `InvalidOperationException`。声明式 `IsOpen=true` 可以在旧 Session 完整关闭后创建新 Session。

### 3.2 关闭事件

Dialog 公开 `Opened`、`Closing`、`Accepted`、`Rejected`、`Finished`、`Closed` 和 `ButtonClicked`。

正常关闭顺序：

```text
按钮来源时 ButtonClicked
  -> Closing
  -> BeforeCloseAsync
  -> 提交 IsOpen=false 与 Result
  -> Accepted 或 Rejected
  -> Finished
  -> 退出 motion 与 presenter teardown
  -> IDialogAwareDataContext.NotifyClosed
  -> Closed
```

`ButtonClicked.Handled=true`、`Closing.Cancel=true` 或 `BeforeCloseAsync` 返回 `false` 会取消本次普通关闭。owner close、placement target detach、外部取消和 presenter 打开失败属于强制 teardown：Session 仍触发一次 `Closing`，但忽略 `Cancel`、跳过 `BeforeCloseAsync`，也不受 confirm loading 阻止，随后执行完整释放。

事件处理器或 presenter 在结果提交后抛出的首个异常，只会在 teardown 完成后传播。

### 3.3 MessageBox 契约

`MessageBox : Dialog` 增加 `Style`、`Icon`、`OkButtonStyle`、`OkButtonText`、`CancelButtonText`、`IsCenterOnStartup`、`Confirmed`、`Cancelled`、`Confirm()` 和 `Cancel()`。

`ShowMessageBoxAsync(...)` 与 `ShowMessageBoxModalAsync(...)` 复用继承的异步 Session 语义。未显式指定 `MessageBoxOptions.MinWidth` 时保留 MessageBox Token 的默认最小宽度。`IsMaskClosable` 由 `Dialog` 继承并经 `MessageBoxOptions` 透传，语义与 Dialog 一致。

### 3.4 Template Parts

| Part | 所属类型 | 职责 |
| --- | --- | --- |
| `PART_Header` | `DialogSurface` | Overlay 标题栏、拖动与 caption 操作。 |
| `PART_ButtonBox` | `DialogSurface` | 当前标准/自定义按钮序列。 |
| `PART_Resizer` | `DialogSurface` | Overlay resize handles。 |
| `PART_LeftGroup` / `PART_CenterGroup` / `PART_RightGroup` | `DialogButtonBox` | 按按钮角色布局。 |
| `PART_MaskMotionActor` | `OverlayDialogPresenter` | modal mask 容器；保留现有部件名，不执行 motion。 |
| `PART_SurfaceMotionActor` | `OverlayDialogPresenter` | DialogSurface 入场/退出 motion。 |
| `PART_SurfaceContentLayer` | `DialogSurface` 内部模板节点 | 包围 Header、ContentFrame 和 FooterFrame；Overlay 关闭时承载前景 opacity 动画，不是 public Semantic Part。 |

当前没有 Modal 专属 pseudo class。

上表是**模板部件**（`PART_*`）清单，与公共 Semantic Part 是两套契约。`Dialog` 没有 `ControlTemplate`，全部视觉节点位于运行时创建的 `DialogSurface`/`OverlayDialogPresenter`/`OverlayDialogHeader` 模板中，因此公开的 `container`/`header`/`title`/`body`/`footer`/`close`/`mask`/`wrapper` 全部声明 `RuntimeCreated`，并与上表的 `PART_*` 名称并存、职责不同。完整契约见 [Modal / Dialog Semantic Part 契约](semantic-part.md)。

## 4. 行为与状态模型

- `IsOpen` 表示最新声明式意图；`DialogSession` 表示一次实际展示。两者不能由 presenter 或 template part 反向拥有。
- modal Overlay 的真实 pointer 输入命中 mask，modeless Overlay 在 Surface 外穿透到底层。只有栈顶 presenter 响应 mask 与 Escape。栈顶 modal mask 外点默认以 `HostCloseRequest` 发起普通关闭；`IsMaskClosable=false` 时该次点击被吞掉且不产生任何关闭请求，不进入 `Closing`/`BeforeCloseAsync` 管道。Window host 没有 mask，外点本来就不触发关闭。
- 所有平台的 `AtomUI.Window` 都把 Overlay presenter 放在 owning `TopLevel` 的 Avalonia `OverlayLayer`。modal 活跃时通过 Window 引用计数租约隐藏 managed/drawn chrome overlay，使 mask 覆盖完整 Avalonia 可绘制窗口轮廓并阻断 caption input；位于客户端 visual tree 外的原生系统 chrome 仍由平台管理。
- Dialog 内容区内的 popup 类控件(ComboBox、Select、DatePicker、Tooltip、Flyout、ContextMenu 等)由同一 Window 的 `PopupOverlayLayer` 或原生 Popup host 承载，位于 Dialog `OverlayLayer` 之上并保持可命中；二者之间的 `LightDismissOverlayLayer` 保证外点关闭与输入穿透语义和普通页面一致。层级与所有权契约见 [Modal 内容弹层叠放设计](popup-layering-design.md)。
- Overlay mask bounds、Window visible frame 与 Dialog 正文 owner bounds 独立：mask 使用完整 layer bounds；所有平台的 Surface 正文定位、拖动、resize 和 maximize 使用 visible frame 按当前有效 drawn frame thickness 内缩后的范围，允许进入 managed/drawn title bar，但不能覆盖窗口 frame。Dialog BoxShadow 只参与绘制并允许在窗口边缘由统一 visual-layer clip 裁剪。
- Enter/Escape 根据当前有效按钮序列查找 default/escape 按钮，运行时修改标准按钮或自定义按钮会立即生效。
- `IsConfirmLoading=true` 只阻止用户发起的普通关闭，不阻止 owner close、detach、取消和失败 teardown。
- 打开后焦点进入 DialogSurface；嵌套 Dialog 关闭时恢复下层 Surface，最后一层关闭时恢复原触发控件。
- Overlay 等待 Surface 的 opening/closing motion；关闭时同一 presenter 还等待内容层 opacity 动画，并在聚合任务完成后才断开 composition children、释放 Surface 和移除 layer。`IsMotionEnabled=false` 只跳过这些 motion，不跳过宿主附加、移除和释放。Window 不创建 Surface `MotionActor`，其打开与关闭分别等待原生 `DialogWindow.Opened` 和 `DialogWindow.Closed`。
- `IsResizable=true` 允许在有效尺寸区间内交互缩放，不表示无约束 resize。结构性最小尺寸在宿主容量允许时始终保留标题、Footer 和非零正文 viewport；`HostMin*` 只能提高该下限，`HostMax*=PositiveInfinity` 仍受 owner 或 screen capacity 限制。Overlay handle 捕获 pointer，release 或 capture lost 都会完整结束当前 resize，不复用上一次拖拽 origin。

## 5. 视觉与主题模型

| 主题文件 | 职责 |
| --- | --- |
| `DialogTheme.axaml` | Dialog 默认属性和 Token 映射。 |
| `DialogSurfaceTheme.axaml` | 共享标题、内容、Footer、按钮与 resize 结构。 |
| `OverlayDialogPresenterTheme.axaml` | 同一 presenter 内组合 mask 与 Surface motion。 |
| `DialogButtonBoxTheme.axaml` | 三组按钮布局。 |
| `OverlayDialogHeaderTheme.axaml` | Overlay 标题栏。 |
| `OverlayDialogMaskTheme.axaml` | modal mask。 |
| `OverlayDialogResizerTheme.axaml` | Overlay resize handles。 |
| `MessageBoxTheme.axaml` / `MessageBoxContentTheme.axaml` | MessageBox 默认尺寸和语义内容。 |

`DialogToken` 提供背景、文字、间距、尺寸和 Footer 视觉。motion duration 使用 Dialog scope 的 SharedToken `MotionDurationMid`，不在代码中硬编码。

## 6. 控件家族或集成关系

- `MessageBox` 继承 `Dialog`，只增加语义内容和按钮策略。
- Overlay 优先使用 placement target 所属 `TopLevel` 的 Avalonia `OverlayLayer`；无可用 TopLevel overlay 时才使用最近的 `ScopeAwareOverlayLayer` fallback。宿主解析由实际能力决定，不按操作系统硬编码，也不使用全局静态 TopLevel 字典。
- Drawer 与 Overlay Dialog 在 Window 中遵循相同的 visible frame、标题栏覆盖和窗口 frame clip 规则，但各自保留独立的 layer、stack 与关闭生命周期。
- Window 使用 Avalonia 原生 `Window` modal owner 能力；不支持原生 Window 的平台会回退到 Overlay。
- `IDialogAwareDataContext` 在 DataContext attach/detach 和 Session closed 时接收通知。
- Gallery 展示 Overlay/Window、modal/modeless、异步关闭、loading、自定义按钮、拖动与尺寸场景。

## 7. 兼容性不变量

后续维护必须保持当前稳定契约：

- 一次实际展示只有一个 `DialogSession`，一次关闭只有一个提交结果。
- Overlay 与 Window 使用相同内容、按钮、关闭、焦点和 teardown 语义。
- Content 可以是字符串、POCO 或 Control；实现不得修改用户 Control 的 TemplatedParent。
- 自定义按钮集合的 Add/Remove/Replace/Move/Reset/Clear 都要更新有效序列并对称管理事件订阅。
- mask、Surface、内容、按钮、binding、逻辑/资源 parent、owner/target 订阅必须在所有关闭路径释放。
- Window mask 必须覆盖完整 Avalonia 可绘制窗口轮廓；存在 drawn title bar 时必须位于其上方。所有平台的 Dialog Surface 正文都使用包含 managed/drawn 标题栏、排除透明 frame shadow 与有效 drawn frame 的 owner bounds；不能把 mask bounds、visible frame bounds、正文 owner bounds 与 BoxShadow 绘制范围合并为同一个矩形。
- Dialog 内容、placement target 与 owning Window 必须保持在同一 `TopLevel`；Dialog 使用 `OverlayLayer`，内容弹层使用更高的 `PopupOverlayLayer`，并保留中间的 light-dismiss 层。
- Window 外轮廓只能由现有 `WindowVisualLayerClip` 统一裁剪；Overlay Dialog 不单独复制 frame shadow margin 或 CornerRadius。
- Overlay 与 Window 必须使用同一套 Surface 正文尺寸解析。Window 只允许在 presenter 边界加回 chrome；不能把 Surface `HostMin/Max` 直接解释为包含标题栏和 frame 的 Window client constraints。
- 用户 resize、runtime `HostMin/Max`、主题或宿主容量变化不得无条件重置已调整的定尺寸；其 actual size 只有越出最新有效区间时才被 clamp。Overlay 自然轴按最新约束测量，自动受限不改变尺寸模式。
- owner 容量变化对 Overlay Surface 的边界约束不改变用户位置偏移；未拖动的居中 Dialog 随最新 owner bounds 重新居中，已拖动 Dialog 在容量恢复后保留用户偏移。
- Overlay 单轴显式请求、交互 resize 和 maximize/restore 必须保留未被接管的自然轴；不能将当前实际矩形中的两轴同时固化成尺寸意图。
- 不重新引入同步 DispatcherFrame、callback close、隐藏 MessageBox Dialog 或分离的 Popup mask。
- 关闭入口开关保持正交：`IsClosable` 管标题栏 X，`IsMaskClosable` 管 Overlay modal mask 外点，互不推导；`IsMaskClosable=false` 时 mask 外点不产生 `HostCloseRequest`。

## 8. 专项模型

### 8.1 Session 状态

```text
Created -> Opening -> Open -> ClosePending -> Closing -> Closed
                         ^          |
                         +-- veto --+
```

`ClosePending` 只用于一个正在执行的普通关闭策略。强制关闭可从 Created、Opening、Open 或 ClosePending 进入 Closing。

### 8.2 宿主选择

- `DialogHostType.Overlay` 使用 owner 范围内的 Dialog overlay stack。Window/TopLevel 使用 Avalonia `OverlayLayer`；无 TopLevel overlay layer 的 scope 使用 `ScopeAwareOverlayLayer` fallback。
- `WindowDrawnDecorations` 只承担 chrome 绘制，不作为 Dialog host；自定义 decorations theme 不改变 Dialog 的 `TopLevel` 所有权。
- `DialogHostType.Window` 使用原生 Window；平台不支持时回退 Overlay。
- `IsModal` 只控制交互模态，不改变 `OpenAsync` 的任务边界。

### 8.3 宿主尺寸与 Resize

`HostWidth/Height/Min/Max` 统一描述 `DialogSurface` 正文 DIP。Dialog 以 Header、Footer 和 `DialogToken.MinWidth/MinHeight` 正文 viewport 基线解析结构性最小尺寸，再与调用方 min/max 和当前 host capacity 合成 effective constraints。

normal 状态的 Overlay 按宽、高分别维护自然轴或定尺寸轴。初次 `HostWidth/Height=NaN` 选择自然模式；owner 容量、内容或约束变化只改变该自然轴的实际测量结果，不把临时收缩固定下来。finite 尺寸请求或交互 resize 只接管对应轴，用户 resize 不反向写回 public request。运行时设置 `NaN` 不强制清除已经形成的定尺寸。

Overlay maximize 临时使用 host capacity，restore 恢复正常态的按轴模式和用户位置偏移，自然轴按恢复时的约束重新测量。Window presenter 在原生 Show 前解析初始自然尺寸，显示后保持 native actual size 与原生 resize 策略，并在 Surface constraints 上加回当前 Window chrome。原生 Window 任一轴配置 finite `HostMax*` 时禁用 native maximize，只有两轴都为默认 `PositiveInfinity` 才允许平台窗口铺满 working area。完整公式、失效条件、宿主策略和验证矩阵见 [Modal 宿主尺寸与 Resize 设计](host-sizing-design.md)。

### 8.4 Overlay 窗口几何

Overlay 使用彼此独立的几何语义：

- mask bounds 等于完整 Dialog layer bounds，覆盖标题栏、正文和窗口可见 frame。
- AtomUI Window 的 visible frame 等于完整 layer bounds 按 `FrameShadowThickness` 内缩后的范围。Dialog 正文 owner bounds 在此基础上继续按当前 drawn decorations 发布的有效 `FrameThickness` 内缩；Windows 的 1 DIP frame 会按实际 render scaling 取整，不能硬编码为 1 DIP 或 2px。
- 普通 TopLevel 和局部 scope 没有 AtomUI Window frame shadow 契约，Dialog owner bounds 保持其 layer bounds。
- `HostWidth` / `HostHeight` 始终表示 Dialog 正文尺寸。BoxShadow 只参与绘制，不改变最大尺寸、placement、拖动、resize 或 maximize 的正文约束；位于窗口边缘之外的阴影由现有 Window visual-layer clip 裁剪。
- Overlay 拖动期间通过复用的 render-only translation 更新 Surface 位置，不在 `PointerMoved` 热路径修改 `Margin`；`Dialog.OffsetX` / `OffsetY` 同步记录逻辑位置增量，供 resize、owner reflow 和 restore 继续使用。

owner resize、frame shadow、drawn frame thickness、Window state 和 `ClientSize` 变化后，mask 继续使用完整 layer bounds，Dialog Surface 正文按最新 owner bounds 重新约束。

## 9. 文档导航与验证策略

- [实现原理](implementation.md)
- [Semantic Part 契约](semantic-part.md)
- [关闭动效设计](dialog-close-motion-design.md)
- [宿主尺寸与 Resize 设计](host-sizing-design.md)
- [内容弹层叠放设计](popup-layering-design.md)
- [Token 设计](token.md)
- [控件级 Changelog](changelog.md)

公共 Semantic Part：

`Dialog` 与 `MessageBox` 公开同一组 8 个 Semantic Part，对齐上游 Ant Design `Modal` 的公开 Semantic DOM。每个 Part 的
`SelectorClass`/`SelectorRoute`/`Style Type`/`ContractType`/`Cardinality`、存在条件、尺寸与几何基线、定制边界和
兼容性定义见 [Modal / Dialog Semantic Part 契约](semantic-part.md)。

| Part | AtomUI 节点 | 职责 | Cardinality |
| --- | --- | --- | --- |
| `root` | `Dialog` / `MessageBox` | public 打开意图、内容、结果和事件入口。 | Single |
| `mask` | `OverlayDialogMaskTheme` 的 `Border#Frame` | modal 遮罩（`ColorBgMask`）。 | Optional |
| `wrapper` | `OverlayDialogPresenterTheme` 的 `PART_SurfaceMotionActor` | Surface 动画/过渡包裹层。 | Optional |
| `container` | `DialogSurfaceTheme` 的 `Border#Frame` | 对话主体框体（背景、圆角、裁剪）。 | Single |
| `header` | `OverlayDialogHeaderTheme` 的 `Border#HeaderFrame` | 标题区框体（背景、内边距）。 | Single |
| `title` | `OverlayDialogHeaderTheme` 的 `TextBlock#Title` | 标题文字排版。 | Single |
| `body` | `DialogSurfaceTheme` 的 `Border#ContentFrame` | 正文区（内边距、内容承载）。 | Single |
| `footer` | `DialogSurfaceTheme` 的 `Border#FooterFrame` | 底部操作区框体。 | Optional |
| `close` | `OverlayDialogHeaderTheme` 的 `PART_CloseButton` | 关闭按钮视觉。 | Optional |

宿主边界：`mask`/`wrapper` 仅 Overlay 宿主物化；Window 宿主内 marker 完整存在，但 owner 作用域 Semantic Style 不跨
`TopLevel` 级联（先例：ImagePreviewer 的 native dialog）。

验证按改动范围运行 Dialog/MessageBox 定向测试、完整 Desktop Controls 测试、Gallery 测试与构建；涉及 AOT 发布路径时执行 Gallery NativeAOT publish，并始终运行 `git diff --check`。Dialog 内容 Popup 家族由原语、控件家族、DataGrid 专项与入口库存测试覆盖，完整清单和断言见 [Modal 内容弹层叠放设计](popup-layering-design.md)。

当前最终 Popup 分层方案已在 Windows CSD、macOS 原生 chrome，以及 Ubuntu GNOME Wayland 环境完成实机测试；
Wayland 证据仅覆盖 Dialog 内容 Popup 真实窗口人工回归。Linux X11 尚未测试，不属于当前已验证平台。
