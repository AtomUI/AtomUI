# AtomUI Popup 可选表面所有权设计

## 1. 设计定位

`AtomUI.Desktop.Controls.Popup` 是 Popup 家族的共享宿主原语，负责 native/overlay host、定位、light-dismiss、动效、阴影和
翻转状态。Popup host 与内容表面是独立职责：host 默认透明，内容控件默认拥有自己的背景、圆角、Padding、箭头和边框。

`SurfaceBackground` 只提供显式可选的 frame surface。它不得成为 Popup Theme 的默认视觉，也不得要求每个消费控件写入
`null` 来抵消 Theme。

## 2. 设计原则

- `SurfaceBackground` 的默认值是 `null`，Popup Theme 不覆盖该值。
- `null` 表示 content-owned：Popup frame 不绘制可见表面，Child、Presenter 或 `PopupFrame` 拥有内容视觉。
- 非空 Brush 表示 host-owned：共享 frame renderer 在 Child bounds 内绘制该 Brush。
- 不根据 Child 类型、Child Background、透明度、Dialog ancestry 或 Theme 应用时序推断表面所有权。
- native `PopupRoot` 与 `OverlayPopupHost` 使用同一个 frame renderer 和同一属性语义。
- 表面属性不参与 measure、arrange、placement、Padding、border、arrow 或 z-order 计算。
- AtomUI 内置 Popup 消费入口继承原语默认值，不重复写入 `SurfaceBackground=null`。

## 3. Public API

`Popup` 公开以下 StyledProperty：

```csharp
public static readonly StyledProperty<IBrush?> SurfaceBackgroundProperty =
    AvaloniaProperty.Register<Popup, IBrush?>(nameof(SurfaceBackground), defaultValue: null);

public IBrush? SurfaceBackground
{
    get => GetValue(SurfaceBackgroundProperty);
    set => SetValue(SurfaceBackgroundProperty, value);
}
```

稳定契约：

- 默认 `null`，Direct Popup 和专用 Popup 家族都保持透明 host frame；需要遮挡下层内容时由 Child 绘制背景。
- 显式非空 Brush 只在 Child bounds 内绘制 frame fill。
- 属性不提供默认 Padding、Border、CornerRadius 或颜色 Token。
- 调用方需要共享 elevated 颜色时，可以显式绑定 `ColorBgElevated`；Popup Theme 不替调用方选择颜色。

## 4. 表面所有权模型

| 模式 | `SurfaceBackground` | 所有者 | 典型入口 |
| --- | --- | --- | --- |
| content-owned，默认 | `null` | Child / Presenter / `PopupFrame` | Direct Popup、Flyout、ToolTip、ContextMenu、菜单、选择器、Picker |
| host-owned，显式 opt-in | 非空 Brush | Popup frame | 明确只提供内容与 Padding、要求 host 绘制表面的自定义 Popup |

host-owned/content-owned 与 native/overlay host 正交。Dialog、Drawer、Windows CSD、macOS 原生 chrome、Linux X11 或
Wayland 都不能改写该属性默认值。

## 5. 共享实现

```text
Popup
  SurfaceBackground = null by property metadata
  -> PopupRoot or OverlayPopupHost       transparent host
     -> PopupMotionActor                 open/close motion
        -> VisualLayerManager
           -> ShadowsAwareContainer      optional frame surface + shadow
              -> Child                   content or specialized presenter
```

`PopupTheme.axaml` 只映射 shadow 和 motion，不设置 `SurfaceBackground`。`ShadowsAwareContainer` 从 owning Popup relay bind
shadow 与 surface；logical reattach 前释放旧 binding，detach 时对称释放。

`PopupFrameRenderer` 使用同一 Child bounds 和 CornerRadius 绘制：

```csharp
var fill = SurfaceBackground ?? Brushes.Transparent;
context.DrawRectangle(fill, null, roundedRect, BoxShadow);
```

- shadow 为空且 surface 为 `null` 时不绘制。
- shadow 非空且 surface 为 `null` 时沿用透明 fill，只绘制阴影。
- surface 非空时即使 shadow 为空也绘制表面。
- surface 变化只 invalidates render，不触发 layout。

## 6. 控件家族集成

17 个 runtime AXAML Popup 入口以及 `Flyout.CreatePopup()`、ToolTip、ContextMenu 三个共享 C# 构造路径均继承原语默认值，
不得保留冗余 `SurfaceBackground="{x:Null}"` 或 `SurfaceBackground = null`。

Flyout、MenuFlyout、TreeViewFlyout、PopupConfirm、ToolTip、ContextMenu、ComboBox、Select、Cascader、TreeSelect、
AutoComplete、Mentions、Picker、Menu、NavMenu、Tour 和 ColorPicker 继续由各自 Presenter 或 `PopupFrame` 绘制内容表面。
DropdownButton、SplitButton、AvatarGroup、Transfer、TabControl 和 DataGrid 等委托消费控件不建立第二套策略。

库存守卫继续发现全部 Popup 构造与委托入口，并额外阻止内置入口重复写入 `null` 默认值。明确选择 host-owned 的入口必须
使用非空 Brush，并由对应行为测试证明其视觉责任。

## 7. 生命周期、性能与 AOT

- surface 与 shadow relay binding 由同一个 `ShadowsAwareContainer` logical lifecycle owner 管理并对称释放。
- 不新增 Popup host、Visual wrapper、反射、运行时类型扫描或动态注册。
- renderer 按 shadow 或显式 surface 需求惰性创建并复用。
- 默认 `null` 路径不建立颜色 Token resource binding。
- StyledProperty、ControlTheme 和静态组合保持 NativeAOT 友好。

## 8. 兼容性与视觉边界

- Direct Popup 恢复并保持透明 host frame 默认契约，其可见 Child 自己拥有主题化表面。
- Popup-bearing 控件的有效 surface 值仍为 `null`；删除冗余 local value 不改变其 Presenter 背景、圆角、Padding、阴影、
  尺寸、位置或输入行为。
- 显式设置 `SurfaceBackground` 的应用继续获得 host-owned surface。
- `PopupRoot.Background` 保持 `null`，不得填充 native shadow buffer。
- Popup placement target、host 与 Dialog/Drawer presentation 继续属于同一 owning `TopLevel`。

## 9. 验证要求

严格执行 Red-Green-Refactor：

1. 先让“应用完整 Popup Theme 后 `SurfaceBackground` 仍为 `null`”的行为测试失败。
2. 让库存测试证明 AXAML 和 C# 内置入口不再重复写入 `null`。
3. 删除 Theme setter 和全部消费端冗余覆盖，使测试转绿。
4. 验证显式非空 Brush 仍传递到共享 frame renderer。
5. 运行 Popup、Dialog 原语、Dialog 控件家族、DataGrid popup 和相邻 Desktop Controls 测试。
6. 验证 Child bounds、CornerRadius、shadow thickness、placement、light-dismiss 和 host cleanup 不变。
7. 运行 LLMS 生成/验证与 `git diff --check`。

Direct Popup 的自动化回归验证默认 frame 不产生额外表面；其余家族测试验证内容表面和交互没有变化。

平台实机证据保持独立记录：Windows、macOS 已测试；Linux X11/Wayland 未测试。Headless 共享路径测试不能把 Linux 状态
标记为已测试。
