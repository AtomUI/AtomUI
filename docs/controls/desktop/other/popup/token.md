# Popup Token 设计

本文档定义 Popup 专属 Token 与共享表面 Token 的语义。Popup 公共契约见
[Popup 桌面版架构设计](overview.md)，内部映射见 [Popup 桌面版实现原理](implementation.md)。控件 Token 通用规则见
[AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。

## 1. Popup 专属 Token

`PopupToken` 是 internal control token，服务 Popup frame 和多个 Popup-bearing 控件：

| Token | 派生来源 | 语义与消费者 |
| --- | --- | --- |
| `PopupRootShadow` | `BoxShadowsSecondary` | native `PopupRoot` frame shadow；菜单、选择器和自动建议等家族复用。与 overlay 相同，阴影区域输入穿透。 |
| `OverlayHostShadow` | `BoxShadowsSecondary` | `OverlayPopupHost` frame shadow；Flyout/Menu/ContextMenu 等 overlay 路径复用。 |
| `PopupCornerRadius` | `BorderRadiusLG` | 专用 Presenter / `PopupFrame` 的默认圆角。 |
| `MarginToAnchor` | `UniformlyMarginXXS` | Popup 内容 frame 与 anchor 的默认间距。 |

`FlyoutHostToken.PopupRootShadow` 同样派生自 `BoxShadowsSecondary`，Flyout、InfoFlyout、PopupConfirm 与其他 Popup 家族
保持一致。native 阴影此前刻意收小（`0 1 6 0` 两层）以减少被阴影拦截的输入面积；阴影输入穿透落地后不再需要该限制。

`PopupCornerRadius` 不由 transparent host 绘制；它由 Child、Presenter 或 `PopupFrame` 提供给
`ShadowsAwareContainer`，使 frame shadow 与显式可选 surface 和内容圆角一致。

## 2. 可选表面

`Popup.SurfaceBackground` 默认 `null`，Popup Theme 不为它映射颜色 Token。Popup 原语因此默认不拥有内容表面，
Flyout、选择器、菜单和 Picker 等控件继续消费各自 Presenter 或 `PopupFrame` 的背景 Token。

| surface ownership | Token 消费 |
| --- | --- |
| content-owned，默认 | Popup 不消费颜色 Token；Child、Presenter 或 `PopupFrame` 消费自己的背景 Token。 |
| host-owned，显式 opt-in | 调用方为 `SurfaceBackground` 提供 Brush；需要共享 elevated 语义时可在使用处绑定 `ColorBgElevated`。 |

`null` 是所有权选择，不是新的透明颜色 Token。不得用 `Transparent` Brush 替代 `null` 来表达 content-owned，因为非空
Brush 会使 host 仍被视为表面所有者。

## 3. 兼容与验证

- 改变 `ColorBgElevated` 不会隐式改变 Popup；只有显式绑定该资源的调用方受影响。
- 改变 shadow Token 不得改变 surface ownership、Child bounds 或内容相对 anchor 的位置：native 窗口尺寸随阴影变化，
  定位按朝向 anchor 一侧的残余阴影补偿，内容与 overlay 路径落在同一屏幕位置。
- 改变 `PopupCornerRadius` 只影响消费该 Token 的 Presenter/PopupFrame，不给 Popup 增加默认圆角或 wrapper。
- Token 验证必须覆盖 light/dark 主题、native/overlay shadow 选择、Popup 默认不消费表面 Token 和显式 Brush opt-in。

当前 Windows 与 macOS 已做实机验证；Linux X11/Wayland 未测试。
