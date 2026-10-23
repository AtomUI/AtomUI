# Popup 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.Popup` 的公共契约、宿主策略、表面所有权和主题边界。内部实现见
[Popup 桌面版实现原理](implementation.md)，Token 语义见 [Popup Token 设计](token.md)，变化记录见
[Popup Changelog](changelog.md)。窗口 layer 的全局职责见
[AtomUI 视觉层规范](../../../../architecture/systems/rendering/visual-layers.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 |  |
| 控件状态 | Stable |

Popup 是 AtomUI 桌面弹层体系的共享低层原语，继承 Avalonia `Popup` 的 anchor、placement、light-dismiss、native
window 与 overlay host 能力，并增加 AtomUI 的定位、阴影、动效、翻转通知和可选表面契约。Flyout、ToolTip、
ContextMenu、菜单、选择器和 Picker 等专用控件复用该原语，但继续拥有各自的 Presenter 和内容表面。

Popup 不提供默认 Padding、边框、箭头或内容布局。调用方负责 Child 内容结构；Popup 只在 Child bounds 内提供可选的
frame surface 与 host shadow。

Popup 同时拥有弹层家族的内部钉住打开物理契约。`internal IsPopupPinnedOpen` 只供测试和内部诊断使用：内容、锚点、attach、effective visible/enabled、TopLevel 和 placement 全部有效后保持 Popup 打开，普通关闭和关闭动效被拦截；PlacementTarget、logical owner、content、TopLevel 或窗口生命周期失效时仍必须立即清理。解除 pin 不关闭已打开 Popup，但必须取消尚未打开的 pending 请求。控件语义 owner、Flyout、ToolTip 和 ContextMenu 的接入规则见 [Popup 钉住打开设计](popup-pinned-open-design.md)。

## 2. 设计语言

Popup 的设计语言是“可定位的透明承载原语”。宿主保持透明并提供窗口/layer 能力，内容默认拥有自己的表面；调用方
显式提供 frame surface 时，该表面只覆盖实际内容 bounds。阴影可以延伸到透明区域，但不能把整个 host 变成不透明矩形。

该模型把 host、frame 和 content 分成稳定职责：host 决定 native/overlay、定位和输入；frame 决定可选表面与阴影；
content 决定 Padding、边框、箭头和业务视觉。

## 3. API 与契约模型

| API | 类型 | 语义 |
| --- | --- | --- |
| `SurfaceBackground` | `IBrush?` | Popup frame 在 Child bounds 内绘制的可选表面；默认 `null`，表示内容拥有表面。只有显式非空 Brush 才启用 host-owned surface。 |
| `PopupRootShadow` | `BoxShadows` | native `PopupRoot` 路径的 frame shadow。 |
| `OverlayHostShadow` | `BoxShadows` | `OverlayPopupHost` 路径的 frame shadow。 |
| `RequestedPlacement` | `PlacementMode?` | AtomUI 自定义定位请求；有效值会投射到 Avalonia custom placement。 |
| `MarginToAnchor` | `double` | 内容 frame 与 anchor 之间的逻辑像素间距。 |
| `IsPointAtCenter` | `bool` | 箭头型内容是否按目标中心修正定位。 |
| `CustomPlacementCallback` | `CustomPlacementCallback?` | AtomUI 完成 placement 后的扩展回调。 |
| `IsHorizontalFlipped` / `IsVerticalFlipped` | `bool` | 当前定位相对请求方向的只读翻转结果。 |
| `PositionFlipped` | `EventHandler<PopupFlippedEventArgs>` | 翻转结果变化通知。 |
| `OpenMotion` / `CloseMotion` | `AbstractMotion?` | host 内容打开和关闭动效。 |
| `MotionDuration` / `IsMotionEnabled` | `TimeSpan` / `bool` | 当前 Popup 的动效时长和开关。 |

Avalonia `Popup` 继承属性继续负责 `Child`、`PlacementTarget`、`IsOpen`、`IsLightDismissEnabled`、offset、anchor、
gravity、overlay 选择和焦点行为。AtomUI 不复制这些契约。

## 4. 行为与状态模型

Popup 使用显式 surface ownership，不根据 Child 类型、Child Background、透明度、Dialog ancestry 或 Theme 应用时序推断。

| 模式 | `SurfaceBackground` | 表面所有者 | 适用场景 |
| --- | --- | --- | --- |
| content-owned | `null`，默认 | Child / Presenter / `PopupFrame` | Direct Popup、Flyout、ToolTip、ContextMenu、菜单、选择器、Picker 等。 |
| host-owned | 显式非 `null` | Popup frame | 调用方明确要求 Popup frame 提供表面，Child 只声明内容和 Padding。 |

`SurfaceBackground` 只改变 frame fill，不增加 wrapper、Padding、border、arrow、DesiredSize、placement offset 或 shadow
thickness。Direct Popup 的 host frame 默认透明；可见弹层内容必须由 Child 自己绘制背景，只有确实需要 frame 拥有表面时，
调用方才显式提供 Brush。

AtomUI 自有的 content-owned 消费者继承 Popup 原语的 `null` 默认值，不在 AXAML 入口或共享 C# 构造路径重复赋值。
新增 Popup-bearing 控件只有在明确选择 host-owned 模式时才设置非空 Brush，并更新库存测试和控件家族回归。

关闭分为普通交互关闭和生命周期 teardown。普通关闭在实际 PlacementTarget 与打开时的 owning `TopLevel` 仍属于同一
有效会话时允许播放 `CloseMotion`；若 Popup 打开时存在 logical owner，该 owner 也必须继续有效。PlacementTarget detach、
已建立的 Popup logical owner detach、PlacementTarget 切换到其他 `TopLevel` 或目标无法转换到原 TopLevel 时，关闭不得被
动效延迟，必须立即释放 Avalonia PopupHost 和定位订阅。自身没有 logical owner、但具有有效显式 PlacementTarget 的 Direct
Popup 仍可使用普通关闭动效。

Pinned 状态不改变上述会话分类：外点、Escape、失焦、window deactivation、`Close`/`Hide` 和业务 open state 写 `false`
属于普通关闭并被拒绝；content removal、target/owner detach、effective visible/enabled 失效、跨 TopLevel、模板重建和窗口销毁
属于生命周期 teardown。无效的 pinned `IsOpen=true` 只保留请求，不发布 `Opened`；unpin 时已打开 Popup 保持打开，pending
请求则被清理且不能在锚点恢复后复活。

## 5. 视觉与主题模型

```text
Popup
  -> PopupRoot or OverlayPopupHost       transparent host
     -> PopupMotionActor                 open/close motion
        -> VisualLayerManager
           -> ShadowsAwareContainer      frame surface + shadow
              -> Child                   direct content or specialized presenter
```

native 与 overlay 两条路径共享相同的 frame surface 实现：

| 路径 | 宿主 | frame shadow | layer 语义 |
| --- | --- | --- | --- |
| native | `PopupRoot` / OS popup window | `PopupRootShadow` | 独立窗口，不参与 owning Window 内部 Z-order。 |
| overlay | `OverlayPopupHost` / `PopupOverlayLayer` | `OverlayHostShadow` | owning `TopLevel` 的 popup layer，高于 Dialog `OverlayLayer`。 |

`PopupRoot.Background` 保持 `null`，`TransparencyLevelHint` 保持 `Transparent`。不透明表面只覆盖 Child bounds，不能填满
native popup 的透明 shadow buffer。overlay host 使用同一原则，避免 host 背景改变圆角、箭头或 shadow 几何。

## 6. 控件家族与集成关系

两类 host 的默认 `FontFamily`、`FontSize`、`FontWeight`、`FontStyle` 来自实际
`PlacementTarget` 的有效 `TextElement` 属性。已打开的弹层随 anchor 字体或 anchor 更换更新，
内容自身的 local/style 字体按 Avalonia 原生优先级覆盖继承值。host 的前景色继续消费有效主题
`ColorText`，不把主按钮的白色文字作为弹层默认前景。

Flyout、MenuFlyout、TreeViewFlyout、PopupConfirm、ToolTip、ContextMenu、ComboBox、Select、Cascader、TreeSelect、
AutoComplete、Mentions、Picker、菜单、NavMenu、Tour 与 ColorPicker 家族全部继承 content-owned 默认值。委托 Flyout 的
DropdownButton、SplitButton、AvatarGroup、Transfer、TabControl 和 DataGrid 不创建第二套表面策略，也不重复写入默认值。

Dialog/Drawer 只影响 Popup 的 owning TopLevel 和 layer 归属，不改变 surface ownership。完整家族和宿主不变量见
[Modal 内容弹层叠放设计](../../feedback/modal/popup-layering-design.md)。

## 7. 兼容性不变量

- Popup placement target、host 与 Dialog/Drawer presentation 必须属于同一 owning `TopLevel`。
- Pinned 打开只能在 content、anchor、attach、effective visible/enabled 和 placement 同时有效时发生；显式 `ShowAt` 与自动恢复使用同一门禁。
- 业务 open state 和 Popup `IsOpen` 的 coercion 不得发布瞬态关闭；`false` 必须保持真实的 false 请求语义。
- native/overlay 切换只改变宿主和 shadow token，不改变 `SurfaceBackground` 语义。
- `SurfaceBackground=null` 时 frame renderer 继续使用透明 fill，专用 Presenter 的背景、圆角、Padding、阴影和定位保持不变。
- Popup Child 内未被内部滚动控件消费的 wheel 事件在 popup 边界终止，避免滚动外层 placement target 祖先。
- 普通 close motion、快速重开和 motion completion 必须保持单一关闭状态流；placement target detach、logical detach、
  跨 `TopLevel` 与 transform 失效属于不可延迟的 host teardown。
- `SurfaceBackground` 是可选公共 StyledProperty；默认 `null` 保持 Direct Popup 与专用 Popup 家族的透明 host frame 契约；
  需要遮挡下层内容的 Direct Popup Child 必须拥有自己的背景。

## 8. 宿主与表面专项模型

host-owned/content-owned 是 Popup 家族唯一的表面分类维度；native/overlay 是独立的宿主维度。两者可以正交组合，
不得引入“Dialog Popup”“Linux Popup”或“透明 Child 自动检测”等第三套 surface 状态。平台只能影响 native window
能力和 host 选择，不能改写 `SurfaceBackground` 的语义。

## 9. 文档导航与验证策略

关联文档：

- [Popup 桌面版实现原理](implementation.md)
- [Popup Token 设计](token.md)
- [Popup 钉住打开设计](popup-pinned-open-design.md)
- [Popup Changelog](changelog.md)
- [Modal 内容弹层叠放设计](../../feedback/modal/popup-layering-design.md)

验证分层：

- `PopupShadowTests`：公开 surface API、`null` 默认值、显式 surface、透明 frame 和 shadow clipping。
- `PopupPlacementTests` / `PopupLifecycleTests` / `ToolTipPopupModeTests`：定位、普通关闭动效、生命周期强制 teardown、
  native/overlay host 和透明 `PopupRoot` 契约。
- `DialogPopupPrimitiveLayeringTests`：Direct Popup、Flyout/MenuFlyout、ToolTip/ContextMenu 共享 content-owned 默认值。
- `PopupEntryInventoryTests`：全部 runtime Popup 入口完成归类、不重复写入 `null` 默认值，且 Popup Theme 不覆盖该默认值。
- Dialog 控件家族矩阵与代表性控件测试：验证共享修复不改变其他控件行为和视觉所有权。

| 平台 | 实机状态 |
| --- | --- |
| Windows | 已测试 |
| macOS | 已测试 |
| Linux X11 / Wayland | 未测试 |

Headless 共享路径测试不能替代 Linux 实机证据。
