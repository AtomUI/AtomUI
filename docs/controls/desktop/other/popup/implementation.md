# Popup 桌面版实现原理

表面登记与动效清理以请求代际和资源身份为边界：先退役旧目标/准备，再释放旧覆盖；
属性通知若同步登记新表面，旧调用不得清除新准备或回写旧目标。

本文档描述 Popup 的宿主选择、frame surface/shadow、定位、动效和生命周期实现。公共契约见
[Popup 桌面版架构设计](overview.md)，Token 语义见 [Popup Token 设计](token.md)，变化记录见
[Popup Changelog](changelog.md)。

## 1. 实现定位

Popup 实现把 Avalonia 的 native/overlay host 能力与 AtomUI 的定位、动效、阴影和可选 surface 合并为一个共享原语。
实现边界是：host 默认透明，frame 只在调用方显式提供 Brush 时于 Child bounds 内绘制表面，content-owned 控件继续由自身
Presenter 绘制视觉。

Popup 的物理关闭策略还承载内部钉住打开状态。`IsPopupPinnedOpen` 由控件或 Flyout owner relay 到实际 Popup；普通 Closing 在 pinned 状态下取消且不启动 CloseMotion，content removal、宿主 detach、PlacementTarget/effective state 失效、跨 TopLevel、模板重建和窗口销毁使用生命周期关闭路径。解除 pin 不关闭已打开 Popup，但会取消未打开的 pending request。状态机和 owner 边界见 [Popup 钉住打开设计](popup-pinned-open-design.md)。

## 2. 源码文件结构

- `src/AtomUI.Desktop.Controls/Popup/Popup.cs`：公共 API、自定义定位、翻转通知、frame shadow 选择、动效和 wheel guard。
- `src/AtomUI.Desktop.Controls/Popup/PopupReflectionExtensions.cs`
- `src/AtomUI.Desktop.Controls/Popup/PopupLightDismissRegistration.cs`：取得当前 open session 的 light-dismiss registration，负责配置变化时的更新和释放。
- `src/AtomUI.Desktop.Controls/Popup/PopupUtils.cs`：placement 算法、popup scope 和 owning popup 查询。
- `src/AtomUI.Desktop.Controls/Popup/PopupToken.cs`：Popup 家族的阴影、圆角和 anchor margin Token。
- `src/AtomUI.Core/Animations/Motion`：可复用动效定义、原生播放、请求身份和属性覆盖。
- `src/AtomUI.Desktop.Controls/Popup/PopupSurface.cs`：实际表面与显示前的 owner 就绪通知。
- `src/AtomUI.Desktop.Controls/Popup/Themes/PopupTheme.axaml`：Popup shadow 和 motion Theme 值；不覆盖 surface 默认值。
- `src/AtomUI.Desktop.Controls/Popup/Themes/PopupRootTheme.axaml`：native transparent host 组合。
- `src/AtomUI.Desktop.Controls/Popup/Themes/OverlayPopupHostTheme.axaml`：overlay host 组合。
- `src/AtomUI.Desktop.Controls/Primitives/ShadowsAwareContainer.cs`：两类 host 共享的 frame surface/shadow renderer 与几何适配。
- `src/AtomUI.Native/NativePopupShadowHitTest.cs`、`src/AtomUI.Native/Windows/WindowsPopupShadowHitTestHook.cs`：窗口型 Popup
  阴影区域输入穿透（Windows），由 `Popup` 在每次打开会话挂接与释放。

## 3. 核心类职责

`Popup` 拥有公开 surface/shadow/placement/motion 状态。Avalonia 创建的 `PopupRoot` 或 `OverlayPopupHost` 拥有实际 host
生命周期；host Theme 中的 `ShadowsAwareContainer` 拥有 frame renderer 和从 ancestor Popup 取得的 relay bindings。

`ShadowsAwareContainer` 不创建第二套 popup host，也不拥有 Child 内容表面。它只负责读取 Popup 当前 frame 状态、解析
Child 圆角/箭头几何、在 Child 之前绘制 surface/shadow，以及为 native shadow 预留有效 thickness。内部
`PopupFrameRenderer` 是纯 frame 绘制节点，不处理 placement、输入或 open state。

## 4. 状态与数据流

```text
Popup property metadata
  SurfaceBackground <- null
PopupTheme
  PopupRootShadow   <- PopupToken.PopupRootShadow
  OverlayHostShadow <- PopupToken.OverlayHostShadow
Caller local/style value (optional)
  SurfaceBackground <- explicit Brush
        ↓
Popup.ConfigureFrameShadow(host mode)
        ↓
Popup.FrameShadow + Popup.SurfaceBackground
        ↓ logical attach relay bindings
ShadowsAwareContainer
        ↓ one-way renderer bindings
PopupFrameRenderer.Render
```

surface ownership 由 Popup 的最终属性值决定：没有 local/style value 时使用属性元数据的 `null` 默认值；调用方显式提供
非空 Brush 时切换为 host-owned。Theme 和 AtomUI 内置消费控件都不重复设置 `null`。renderer 不检查 Child 类型、
Background 或 opacity。

Pinned open state 使用请求值和有效显示值分离的数据流：

```text
semantic owner IsPopupPinnedOpen
  -> business open-state coercion
  -> Popup.IsPopupPinnedOpen relay
  -> Popup.IsOpen coercion
  -> content + target + attach + effective state + TopLevel + placement gate
  -> physical PopupHost
```

`Popup.IsOpen=false` 始终保持 false；pinned 状态下无效的 `true` 只形成 pending request，不触发 `Opened`。拥有业务 open state
的控件使用 lifecycle close depth 区分普通 false 与 teardown false，普通关闭不会发布瞬态 `false -> true` 通知。

## 5. 内部算法与关键流程

### 5.1 frame renderer

`PopupFrameRenderer` 在 shadow 非空或 surface 非 `null` 时惰性创建，位于 Child 之前，不增加 layout wrapper。
renderer 使用现有 Child bounds；arrow 可见时从 frame body bounds 扣除 indicator 占用。圆角从
`IArrowAwareShadowMaskInfoProvider`、`Border` 或 `TemplatedControl` 解析。

fill 固定为 `SurfaceBackground ?? Brushes.Transparent`。shadow 为空且 surface 为 `null` 时不绘制；surface 非 `null`
时即使没有 shadow也绘制。`SurfaceBackground` 只触发 render，不加入 `AffectsMeasure`。

### 5.2 layout 与 placement

native host 使用同一份有效 box-shadow thickness 分配 measure/arrange 空间：测量先扣除 shadow buffer，再叠加 Child
期望尺寸；排列将 Child 放入扣除 shadow buffer 和 Decorator Padding 的最终内容槽。arrow 可见时按方向扣除 indicator
已占用的 thickness；frame body 从 Child 的实际原点继续扣除箭头区域，保留 shadow、Margin 和 Padding 的偏移。
overlay host 使用 `IsOverlayMode=true`，不把 native shadow buffer 加入 layout。

Popup 将普通 Avalonia placement 转换为 custom placement，并统一计算 anchor、gravity、offset、shadow thickness、arrow
center 修正和 flip。native 路径的主轴偏移为 `MarginToAnchor - 残余阴影`：残余阴影是朝向 anchor 一侧的阴影厚度减去
箭头占用（`PopupUtils.GetAnchorSideResidualShadow`），与 `ShadowsAwareContainer.GetEffectiveShadowThickness` 预留的空间一致；
箭头隐藏、部件不可用或 Child 没有箭头时 arrow bounds 为空，残余阴影即完整阴影厚度，不再需要单独补偿。副轴（edge-aligned
对齐）继续由 `CalculateShadowOffset` 按完整阴影厚度对齐内容边缘。由此内容相对 anchor 的位置与阴影厚度无关，与 overlay 路径一致。center 使用 owning `TopLevel.ClientSize`；pointer 使用 owning TopLevel 的 client 坐标。surface 不进入
任何 placement 输入，因此不能改变位置或尺寸。

翻转预测使用定位请求中的有效尺寸：从 host 的 `PopupSize` 扣除 `Deflate` 后计算边界约束，
native 路径再按 desktop scaling 转换到屏幕坐标。CSD overlay 的可见区域约束和最终 anchor 回写使用同一有效矩形，
host 定位完成后再恢复 margin 占用，避免非对称 Margin 导致预测翻转与实际内容位置不一致。
`Deflate` 为零时沿用完整 host 尺寸；不通过删除 Child Margin、阴影或改变模板结构来修正定位。

## 6. 生命周期与模板接入

logical attach 时，`ShadowsAwareContainer` 创建一个 `CompositeDisposable`，统一持有 shadow 与 surface 两条 relay binding；
重新 attach 前先释放旧 binding，logical detach 时对称释放。固定模板关系不能直接跨 Avalonia popup host 的 logical owner
使用 `TemplateBinding` 表达，因此 binding 由实际 runtime container 持有。

ContentPresenter Child observable 在 Child/Presenter 替换时重新配置圆角和 arrow geometry，detach 时释放。frame renderer
设置 logical parent 并随 container 生命周期存在。

`IArrowAwareShadowMaskInfoProvider.GetArrowDecoratedBox()` 是可空契约：提供方模板尚未应用或当前主题未提供
`PART_ArrowDecorator` 时返回 `null`。container 探测到 `null` 时按无箭头降级（`IsArrowVisible=false`、圆角归零），
并在降级期间订阅提供方的 `TemplateApplied`，下一次模板应用时重新探测，部件就绪即恢复箭头 relay binding；订阅随
`surface bindings` 统一释放。custom placement 同样把部件不可用按箭头隐藏处理，复用阴影厚度补偿路径。

Popup 持有本次宿主请求版本和局部 `Idle -> Pending -> Playing -> Completing` 关闭阶段；Core Motion 拥有活动播放资源。
快速重开使旧版本失效，由新入场连续替代离场；同一时刻最多调度一次有效关闭。完成结果核对请求与表面身份，
最终排队的 Close 再次核对版本、阶段和独立关闭意图。绑定已将 IsOpen 设为 false 时仍必须完成物理关闭；
false→true 或显式重开在 Completing 阶段也使旧关闭失效。`Closed` 取消播放并清空表面、打开时 TopLevel、tracker 与 wheel guard。
离场期间更换模板时，旧表面不再可用，新表面先保持隐藏，再按当前版本稳定关闭；重新打开可取消这次排队关闭。

`PopupSurface` 直接继承 Panel，是宿主实际表面，承载原有的阴影和 VisualLayerManager，不实现 Actor。
开启动画接受表面先就绪或 `Opened` 先发生两种顺序。逻辑 attach 在宿主显示前建立作用域透明度覆盖；
无 rooted logical owner 的 Direct Popup 还通过 visual attach 注册表面，在首个绘制前完成准备。
Direct Popup 需要在其可达资源作用域提供宿主主题；没有 AtomUI 表面的自定义宿主按无动效路径关闭，
不能用未挂载的模拟目标代表实际宿主支持动画。
OpenMotion/CloseMotion 接收 AbstractMotion 定义。一次 Loaded 调度跨越布局准备边界，调用定义实例的 `PlayAsync` 后释放初始覆盖。无可播放轨迹时恢复底层属性来源。
异步任务捕获请求版本和目标身份，旧宿主不能修改新打开周期；有效时长通过请求传入，不修改共享 Motion 定义。

关闭动效只能延迟仍然有效的普通关闭。Popup 打开时记录该会话的 owning `TopLevel` 以及当时是否已有 logical owner；
`Closing` 发生时验证当前实际 PlacementTarget 仍在 visual tree、目标仍属于原 TopLevel，并且目标能够转换到该 TopLevel。
若打开时已有 logical owner，还要求 Popup 继续挂在 rooted logical tree；打开时没有 logical owner 的 Direct Popup 不虚构该
约束。任一已建立的会话条件失效都表示 Avalonia host 生命周期已经结束或即将结束，AtomUI 不取消关闭，允许 Avalonia 同步
释放 PopupHost、定位订阅和 open state。placement target detach、既有 Popup logical owner detach、跨 TopLevel target 切换
因此不会留下无锚点但仍打开的 Popup。

Pinned request 获取当前 PlacementTarget 后，订阅 target attach/detach，并订阅 target 到 TopLevel 祖先链上的 visible、bounds
和 effective enabled 变化。target replacement、unpin 和 logical detach 释放旧订阅；generation 使旧 Dispatcher callback 在
replacement、re-template、detach 或 unpin 后失效。显式 `Flyout.ShowAt` 在 `OnOpening` 使用同一物理门禁，无效 target 不发布
Flyout/Popup opened state。

生命周期关闭先使 generation 失效、取消 motion、释放初始覆盖，再进入可嵌套的 close bypass scope。即使 Popup 当前
有效 `IsOpen=false`，该入口仍调用 Avalonia close，以清理 coercion 下保留的 base open request 和 AtomUI pending request。
Unpin 对已打开 Popup 只释放 close guard；对 pending Popup 则同时清理 PlacementTarget、Popup parent、target tracking 和
Flyout global resource bindings。Window deactivation 是普通关闭来源，只有窗口销毁或会话失效才属于 lifecycle teardown。

打开时安装 Child wheel guard，关闭时释放。placement transform tracking 只在打开且 placement 需要 anchor 时存在；目标仍在
有效会话但滚出可见区域时可以走普通关闭动效，target 或宿主生命周期失效时则由上述强制 teardown 规则立即关闭。

## 7. 默认消费路径

AtomUI 自有弹层通过 Popup 原语统一继承 `SurfaceBackground=null`：

- 17 个 runtime AXAML Popup 文件覆盖选择器、自动建议、Picker、菜单、NavMenu、Tour 与 ColorPicker 家族。
- `Flyout.CreatePopup()`、ToolTip 和 ContextMenu 是三个共享 C# 构造路径。

这些入口不写入冗余 local value。Flyout 派生与委托消费控件通过共享 `Flyout.CreatePopup()` 继承同一默认值。新增入口由
`PopupEntryInventoryTests` 的闭集扫描捕获；只有明确需要 host-owned surface 的入口才能显式提供非空 Brush。

## 8. 输入与宿主边界

Popup placement target 和实际 host 必须解析到同一 owning `TopLevel`；Popup 存在 logical owner 时，该 owner 也必须属于
同一会话。没有 logical owner 的 Direct Popup 由显式 PlacementTarget 建立宿主归属。Dialog 使用低于 popup 的
`OverlayLayer`，content popup 使用同一 Window 的 `PopupOverlayLayer`；native popup 则使用独立 `PopupRoot`。两条 host
路径共享 frame renderer 和 surface ownership。

Child 内部控件已处理的 wheel 不被重复消费；未处理的 wheel 在 popup 边界终止，避免滚动 placement target 外层祖先。
focus 与 host teardown 继续使用 Avalonia Popup 协议。产品 owner 只提供 light-dismiss 原始配置与 pin 请求，
共享 Popup 通过 coercion 计算 `configured && !pinned`，保留原绑定和值来源；取消 pin 不应写死恢复为 true。

`PopupLightDismissRegistration` 是共享框架兼容边界：打开后取得当前 Popup 会话自己的遮罩 registration，
在 effective 配置或穿透目标变化时成对释放/注册，并复用框架的外点处理器。关闭时释放当前 lease 与订阅。
它不改写共享遮罩的 `IsVisible`、不关闭并重开 host、不按穿透目标猜测其他 Popup 的 registration。
框架私有会话与 registration 布局由固定类型/成员引用描述；依赖升级后须重新执行多 Popup、实际命中、关闭事件、
宿主复用与 NativeAOT 验证。普通 Debug 测试不能代替 trimming 和 NativeAOT 发布证据。

### 8.1 窗口型 Popup 阴影输入穿透（Windows）

native popup 的阴影画在窗口自身 surface 中，窗口矩形 = 内容 + 阴影 buffer。阴影与内容保持同一 surface（动效严格同帧），
只把输入区域收缩到内容形状。被否决的独立阴影窗口方案与各平台调研见
[窗口型 Popup 阴影与鼠标穿透方案记录](../../../../superpowers/progress/2026-10-29-window-popup-shadow-hit-test-plan.md)。

挂接：`Popup.Opened` 时若 Child 的 TopLevel 是 `PopupRoot`（`Host` 与 `IPopupHostProvider` 在 Avalonia 12.1.4 均为 internal，
用 `TopLevel.GetTopLevel(Child)` 判定，不使用反射），调用 `NativePopupShadowHitTest.TryAttach`；`Closed` 时释放。
非 Windows 平台返回 `null`。

内容形状直接复用 Avalonia 命中测试：`popupRoot.InputHitTest(point, enabledElementsOnly: false) is not null`。
`PopupFrameRenderer` 以非空填充绘制圆角矩形，箭头以几何绘制，Avalonia 绘制列表命中测试按圆角矩形与几何计算且不包含
`BoxShadow`，因此阴影区域天然不命中；`PopupRoot`、`PopupSurface`、`ShadowsAwareContainer` 无背景，
`PART_TransparencyFallback` 不可命中。禁用的内容仍属于内容形状（不按 enabled 过滤）。

动效期间整窗穿透：`Popup.IsPlayingMotion` = 打开动效（从 `StartOpenMotion` 排队到播放结束，包含首帧前的透明准备期）
或关闭动效阶段（`Pending`/`Playing`/`Completing`）。命中判定每次 `WM_NCHITTEST` 拉取该状态，无需通知；
动效结束后下一次鼠标移动即恢复按内容形状命中。

两层判定（`WindowsPopupShadowHitTestHook`，`Win32Properties.AddWndProcHookCallback` 挂接）：

1. 同进程：`WM_NCHITTEST` 在内容形状外（或动效期间）返回 `HTTRANSPARENT`，系统把鼠标事件交给同线程下方窗口
   （所属主窗口、其他 Popup）。内容形状内不处理，交还 Avalonia。
2. 跨进程：`HTTRANSPARENT` 只在同线程窗口之间转发。若该点同时位于所属主窗口（`GA_ROOTOWNER` 的
   `DWMWA_EXTENDED_FRAME_BOUNDS`）之外，加 `WS_EX_TRANSPARENT`，之后由 16ms `DispatcherTimer` + `GetCursorPos` 轮询；
   光标回到内容形状、回到主窗口内或移出 Popup 窗口时移除并停止轮询，交还第一层。Popup 完全位于主窗口内时不会进入该路径。

`WS_EX_TRANSPARENT` 只在 `WS_EX_LAYERED` 窗口上跨进程生效。挂接时即设置 `WS_EX_LAYERED` +
`SetLayeredWindowAttributes(alpha=255)`；运行时只切换 `WS_EX_TRANSPARENT`，不调用 `SetWindowPos(SWP_FRAMECHANGED)`，
不触发重绘。`WM_DESTROY` 时停止轮询，释放时若窗口仍存在则恢复原扩展样式。

与 light dismiss 的关系：命中穿透只决定事件交给哪个窗口。主窗口中 `LightDismissOverlayLayer` 在 light-dismiss Popup
打开期间覆盖整个客户区，因此点击阴影与点击 Popup 外部任何位置完全相同：照常关闭，只有
`OverlayDismissEventPassThrough=True` 时下方控件同时收到点击；该期间主窗口控件也不会进入悬停态（overlay Popup 相同）。
无 light dismiss 的 Popup（如 `Trigger=Hover` 浮层、ToolTip）下方控件可直接收到悬停。

## 9. 资源、性能与 AOT 边界

可选 surface 不增加 host 或 wrapper；只扩展既有 renderer。renderer 因 shadow 或显式 surface 按需创建并复用，surface
更新只 invalidates render。默认路径没有 surface Theme resource，也不创建全局非 Visual resource host。

Pinned target/ancestor 订阅只在请求有效期内存在，并由 target replacement、unpin、logical detach 或 lifecycle teardown
释放。Pinned open 不使用 retry timer、程序集扫描、runtime type discovery、反射查找控件或字符串 binding；恢复只依赖
generation-checked Dispatcher callback 和 AvaloniaProperty relay。

`PopupReflectionExtensions` 集中反射 Avalonia Popup 的非公开 closing event、parent setter 与定位刷新入口，
每个反射成员都通过 `DynamicDependency` 声明 NativeAOT 保留要求；Popup 不做程序集扫描或 runtime type discovery。
StyledProperty 和 ControlTheme 均为静态/AOT 可发现契约。源码库存测试使用正则扫描，但只存在于测试项目，不进入 runtime
或 NativeAOT 路径。

## 10. 维护不变量

- `PopupRoot.Background` 必须保持 `null`，native window 继续透明合成。
- native 与 overlay host 必须共享 `ShadowsAwareContainer`，不得复制 surface 实现。
- surface 不得参与 measure、arrange、placement、Padding、border 或 arrow 计算。
- `SurfaceBackground` 的属性默认值必须为 `null`，Popup Theme 不得覆盖该默认值。
- content-owned Popup 不重复设置 `null`；host-owned Popup 必须显式提供非空 Brush。
- relay binding 的 attach/re-attach/detach 必须有单一 owner 和对称释放。
- close motion 只能延迟仍连接到打开时 owning TopLevel 的普通关闭；打开时已存在的 logical owner、host 或 anchor 生命周期
  失效时不得保留 Avalonia open state。没有 logical owner 的 Direct Popup 以显式 PlacementTarget 会话为准。
- Pinned open 的有效性必须同时包含 content、target attach、effective visible/enabled、TopLevel 和 placement transform；无效 true 不得发布 `Opened`。
- Pinned 普通关闭包括外点、Escape、失焦、window deactivation、`Close`/`Hide` 和业务 open state false；lifecycle close 必须跳过 motion 并释放 host、binding、subscription、tracker、wheel guard 和 timer。
- Unpin 不关闭已打开 Popup；pending unpin 必须清除隐藏 open request，target 恢复后不得复活旧请求。
- 业务 open state coercion 不得通过 suppression flag 发布瞬态 false；lifecycle close scope 是唯一允许 pinned 业务状态变为 false 的路径。
- Popup 以自身 `PopupCloseState` 表达关闭动效阶段；`Pending`、`Playing` 和 `Completing` 单向收敛，重复
  close 不得创建并行关闭动效，`Closed` 必须回到 `Idle`。
- Popup 开启动画必须覆盖 `Opened` 与实际表面就绪的两种先后顺序；物理 `IsOpen=true` 但表面仍透明不是有效打开终态。
- native 阴影输入穿透不得拆出独立阴影窗口，也不得在运行时切换 `WS_EX_LAYERED`；只切换 `WS_EX_TRANSPARENT`。
- 内容形状判定必须与实际绘制一致：不得给 `PopupRoot`、`PopupSurface` 或 `ShadowsAwareContainer` 设置非空背景，
  否则阴影区域会重新拦截输入。
- native placement 的主轴偏移必须扣除朝向 anchor 一侧的残余阴影；改变阴影 Token 不得改变内容相对 anchor 的位置。

## 11. 测试与验证

- `PopupShadowTests` 验证公开 surface API、`null` 默认值、显式 renderer fill、透明 frame 和 shadow clipping。
- `PopupPlacementTests` 与 `PopupLifecycleTests` 验证 placement、开启动画 actor 乱序、关闭动效状态、pinned 有效性门禁、显式 `ShowAt`、pending unpin、placement target/logical owner detach、跨 TopLevel target 切换和 host teardown。
- `PopupPinnedOpenContractTests` 验证 Property 字段、CLR wrapper 和 ToolTip attached accessors 都保持 internal，且 Gallery/API/AXAML surface 不包含该能力。
- `ToolTipPopupModeTests` 验证 host mode 和 transparent PopupRoot。
- `DialogPopupPrimitiveLayeringTests` 验证 Direct Popup 与四类 content-owned 原语。
- `PopupEntryInventoryTests` 守卫所有 runtime 入口、无冗余默认值覆盖和 Popup Theme 的 `null` 默认契约。
- `DialogPopupControlFamilyTests`、DataGrid popup tests 和代表性控件测试验证家族行为与视觉所有权未变。

实机验证状态为 Windows、macOS 已测试；Linux X11/Wayland 未测试。

窗口型 Popup 阴影输入穿透与定位（Windows 11，150% 缩放，`RedirectionSurface`）使用脚本化视觉验证
（本地 `tmp/visual-verification/windows/`，`ddagrab` 录制 + UI Automation 定位）：

- 定位：InfoFlyout 箭头示例 12 个方向 × 箭头显示/隐藏/居中共 36 组，窗口型与 overlay 截图逐像素对比，
  frame、阴影、箭头与 margin 全部重合（12 组完全一致，其余仅文字子像素栅格化差异）。
- 第一层：Hover 浮层阴影盖住相邻按钮时，录像中存在“浮层可见且下方按钮悬停”的帧；指针从内容移到阴影后 Hover 浮层关闭；
  点击 Click 浮层阴影 light dismiss，`OverlayDismissEventPassThrough=True` 时下方按钮同时获得焦点。
- 第二层：浮层超出主窗口 107px，独立进程 WinForms 靶子在阴影下收到 ENTER/CLICK，该点 `WindowFromPoint` 命中靶子进程；
  光标回到（主窗口外的）内容后 `WS_EX_TRANSPARENT` 移除、靶子收到 LEAVE、点击内容浮层不关闭；主窗口内阴影不启用第二层。
- 视觉：`WS_EX_LAYERED` + alpha 255 下透明圆角与阴影无黑框；打开/关闭动效逐帧阴影与内容同帧；两次样式切换期间
  内容区 204 帧无任何变化。
