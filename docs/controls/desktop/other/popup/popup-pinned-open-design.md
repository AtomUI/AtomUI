# Popup 钉住打开设计

本文档定义 AtomUI 桌面 Popup 家族的内部钉住打开契约。它覆盖 `Popup` 物理层、拥有业务打开状态的控件语义层、`Flyout` 宿主以及手动创建 Popup 的控件。公共 Popup 契约见 [Popup 桌面版架构设计](overview.md)，实现边界见 [Popup 桌面版实现原理](implementation.md)。

## 1. 设计定位

钉住打开是测试和内部诊断能力，不是应用层公共 API。它允许测试在不修改正常交互触发条件的情况下，让一个有效 Popup 自动打开并保持可观察状态。

该能力只适用于 Popup 下拉、菜单、选择器、Flyout、Tooltip 和类似弹层。Dialog、Drawer、ImagePreviewer 使用独立的会话或 Overlay Host 生命周期，不属于本契约。

## 2. 设计原则

- 控件语义层和 Popup 物理层必须同时拥有钉住状态。
- `true` 表示打开请求，只在内容、锚点、visual attach、effective visible/enabled、TopLevel 和 placement 都有效时收敛为打开；它不是无条件写入 `IsOpen=true`。
- `false` 只解除关闭拦截，不强制关闭已经打开的 Popup；尚未实际打开的 pending 请求必须取消，并清除底层隐藏的 open request、target tracking 和临时资源。
- 普通交互关闭可以被拦截；生命周期 teardown 必须放行。
- 生命周期关闭不得被关闭动效延迟。
- owner 提供 light-dismiss 配置；共享 Popup 在首次打开前及运行期间按 `configured && !pinned` 计算有效值，并同步自身的遮罩注册。取消 pin 恢复原配置，不重开 host、不影响其他 Popup 的注册。
- 模板、定位、滚动、Overlay、输入路由和视觉尺寸不因测试状态而改变。
- internal 属性不进入 public API、公开 AXAML 模板契约或 Token。

## 3. 专项模型与内部 API

### 3.1 Popup 物理层

`Popup` 持有内部 StyledProperty：

```csharp
internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
    AvaloniaProperty.Register<Popup, bool>(nameof(IsPopupPinnedOpen));

internal bool IsPopupPinnedOpen
{
    get => GetValue(IsPopupPinnedOpenProperty);
    set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
}
```

该属性默认值为 `false`。StyledProperty 用于建立宿主到实际 Popup 的 relay；CLR 属性和 Property 字段均保持 `internal`。

### 3.2 控件语义层

拥有业务打开状态的具体控件在自身或最近的共享基类上提供同名 internal 属性。属性 owner 必须靠近业务状态，而不是靠近模板部件；`FlyoutHost`、滚动查看器和其他内部宿主只能作为 relay 目标，不能替代由测试直接取得的具体控件语义入口。这样 `DropdownButton`、`SplitButton`、`AvatarGroup`、`TabControl` 等组合控件也都能通过自身（或其可继承基类）的 internal 属性驱动弹层。

| 控件族 | 语义 owner（internal `IsPopupPinnedOpen`） | relay / 物理 Popup 来源 |
| --- | --- | --- |
| Select、TreeSelect、Cascader | `AbstractSelect` | `PART_Popup` |
| DatePicker、TimePicker | `InfoPickerInput` | `PickerPopup` |
| AutoComplete | `AbstractAutoComplete` | 内部 Popup |
| ComboBox | `ComboBox` | 模板 Popup |
| ColorPicker | `AbstractColorPicker` | 模板 Popup |
| Mentions | `Mentions` | 内部 Popup |
| Flyout 家族 | `FlyoutHost` / `Flyout` | `Flyout` 创建的 Popup |
| Menu | `Menu` / `MenuFlyout` | MenuFlyout Popup |
| NavMenu | `NavMenu` / `NavMenuItem` | submenu Popup |
| Tour | `Tour` | `PART_Popup` |
| ToolTip、ContextMenu | 各自控件或服务 owner | 手动创建 Popup |
| TreeView | `TreeView` | `TreeViewFlyout` Popup |
| Transfer | `AbstractTransfer` | `TransferSelectDropdown` -> `MenuFlyout` Popup |
| DataGrid | `DataGrid` | column-filter Flyout -> Popup |
| TabControl、TabStrip | `BaseTabControl`、`BaseTabStrip` | overflow ScrollViewer -> `MenuFlyout` Popup |
| DropdownButton | `DropdownButton` | `DropdownFlyout` -> `MenuFlyout` Popup |
| SplitButton | `SplitButton` | `Flyout` Popup |
| AvatarGroup | `AvatarGroup` | fold-count `FlyoutHost` -> `Flyout` Popup |
| PopupConfirm | `PopupConfirm`（继承 `FlyoutHost` 的语义入口） | `PopupConfirmFlyout` Popup |

叶子控件复用家族 owner，不重复注册相同逻辑；组合控件只在自身或共享基类注册一次，再把状态 relay 到内部 Flyout、ScrollViewer 或 Popup。内部测试访问依赖现有 `InternalsVisibleTo`，不增加公开入口。

## 4. 状态与关闭策略

钉住请求与物理显示是两个状态：

```text
IsPopupPinnedOpen
    + Popup content valid
    + host/control attached
    + control effectively visible and enabled
    + PlacementTarget attached
    + PlacementTarget and ancestors effectively visible
    + PlacementTarget belongs to a valid TopLevel
    + target transform and placement valid
    -> Popup remains open
```

普通关闭包括外点、Escape、失焦、window deactivation、`Close`/`Hide`、业务 open state 写回 `false` 和关闭动效入口。生命周期关闭包括 owner 或 target detach、content removal、effective visible/enabled 失效、跨 TopLevel、窗口销毁、模板重建和明确的资源清理。

`Popup.Closing` 的决策顺序固定为：

1. 生命周期关闭作用域或已失效会话优先放行，并跳过关闭动效。
2. `IsPopupPinnedOpen=true` 时取消普通关闭，不进入 `CloseMotion`。
3. 其余情况执行现有关闭动效和 Avalonia close 流程。

Popup 提供内部的生命周期关闭入口，使宿主可以明确表达 teardown，而不是通过永久保持 `IsOpen` 来阻止资源清理。

`Popup.IsOpen` 使用 coercion 保持请求值与有效显示值分离：`false` 永远保持 `false` 语义；pinned 状态下无效的 `true` 只形成待满足请求，不得产生 `Opened`。显式 `Flyout.ShowAt` 也必须经过相同门禁，不能先发布打开事件再补偿关闭。

拥有公开或业务 open state 的控件必须在该状态的 owner 上做等价 coercion。Pinned 状态下普通 `false` 不得先发布一次可观察的 `false` 再写回 `true`；只有 lifecycle close scope 可以让业务状态真正变为 `false`。这样 property observer、two-way binding、伪类和打开/关闭事件不会观察到伪关闭。

## 5. 架构、组合与职责

```text
控件 internal IsPopupPinnedOpen
    -> 业务 open state 收敛
    -> Popup.IsPopupPinnedOpen relay
    -> Popup Closing policy / motion policy
    -> PopupRoot 或 OverlayPopupHost
```

`Popup` 负责：

- 物理关闭拦截；
- 生命周期关闭白名单；
- 关闭动效取消和跳过；
- 有效宿主恢复后的打开收敛；
- OverlayPopupHost、定位跟踪和 wheel guard 的释放边界。

具体控件语义 owner 负责：

- 业务 open state；
- Popup 内容准备；
- 首次物理打开前的 pinned light-dismiss 抑制，以及取消 pin 后的原配置恢复；
- 伪类和业务事件的一致性；
- 模板重建时重新 relay；
- detach 后禁止旧 callback 复活。

内部 `FlyoutHost`、`MenuFlyout`、overflow `ScrollViewer` 或其他 adapter 只负责把语义 owner 的状态投射到实际 Popup，并不成为测试设置 pinned 状态的第二个入口。对继承 `FlyoutHost` 的控件（例如 `PopupConfirm`），该属性可以由基类声明并由派生控件继承。

`Flyout` 负责把 `Flyout.HideCore`、`ShowAtCore` 和 `FlyoutStateHelper` 的自动关闭行为接入同一策略。ToolTip 和 ContextMenu 负责把手动创建的 Popup 纳入同一策略。

## 6. Template 与集成契约

控件模板不新增公开的 `IsPopupPinnedOpen` 绑定。模板应用后，由控件 owner 将内部属性 relay 到真实 Popup，例如：

```csharp
popup[!Popup.IsPopupPinnedOpenProperty] =
    this[!IsPopupPinnedOpenProperty];
```

这样自定义模板只需继续提供原有 `PART_Popup` 或既有 Popup 结构，不承担新的公开模板契约。模板重建时先解绑旧 Popup，再对新 Popup 建立 relay。

## 7. 数据流与生命周期

钉住打开使用合并调度，不允许状态变化触发无限重试：

```text
pin=true / attach / template apply / popup closed / target restored
    -> QueuePinnedOpen
    -> verify generation and host validity
    -> prepare content and business state
    -> set Popup.IsOpen=true
```

显式 `ShowAt` 与控件自动打开共享同一有效性判断。Popup 在 pending 或打开期间只订阅当前 PlacementTarget 的 attach/detach，以及 target 到 TopLevel 祖先链上会改变有效性的 visible、bounds 和 effective enabled 状态；target replacement 先释放旧订阅，再建立新订阅。

控件 detach、模板重建、Popup owner 变化或 unpin 时递增 generation；旧队列 callback 发现 generation 不匹配后退出。Popup 重新 attach 且条件有效时，再根据仍然为真的 pinned 请求恢复打开。Unpin 对已打开 Popup 只解除拦截；对尚未打开的 pending request 则同步清除隐藏 open request、PlacementTarget、Popup parent、target tracking 和本次全局资源 binding，后续 target 恢复不能复活旧请求。

生命周期 teardown 必须：

- 允许 Popup 关闭；
- 跳过 CloseMotion；
- 释放 PopupRoot/OverlayPopupHost、target/ancestor 事件订阅、定位 tracker、wheel guard、Flyout 全局资源 binding、timer 和内容订阅；
- 使业务 owner 与物理 Popup 最终回到一致状态。

## 8. 控件家族策略

- `AbstractSelect` 拦截 `IsDropDownOpen=false`，恢复业务状态后调用 `EnsurePopupContent`；`Select`、`TreeSelect`、`Cascader` 共享该规则。
- `InfoPickerInput` 负责 DatePicker/TimePicker 的 `PickerPopup` relay，并在范围输入切换 PlacementTarget 时重新验证会话；`IsPickerOpen` coercion 不发布瞬态 `false`。
- AutoComplete、ComboBox、Mentions 在候选清理前拦截普通关闭；候选、焦点和过滤状态仍由各自 owner 管理，业务 open state coercion 不发布瞬态 `false`。
- 具体控件先把自身的 internal 属性收敛到业务 open state，再由 `FlyoutHost`、`FlyoutStateHelper`、overflow `ScrollViewer` 或专用 adapter 传递到 Flyout/Popup；hover leave、focus lost 和外点 Hide 在 pinned 状态下被拦截，但 detach teardown 始终放行。
- Menu、NavMenu、Tour、TreeView、Transfer、Tab overflow、DataGrid filter、DropdownButton、SplitButton 和 AvatarGroup 都由各自具体控件或共享基类拥有语义属性，不直接修改每个模板项。
- ToolTip、ContextMenu 在创建手动 Popup 时 relay，并把 `Close`、Escape、window deactivation 和菜单项关闭分为普通关闭与生命周期清理；pending unpin 必须同时清理 attached open state。

## 9. 资源、性能与 AOT 边界

该能力不引入新的视觉资源、Token、Popup wrapper 或 layout pass。Pinned callback 只在 UI 调度队列中合并一次；状态恢复使用现有 Avalonia StyledProperty 和 binding 机制。

不使用程序集扫描、运行时类型发现或反射查找控件。内部 Property 注册、binding relay 和 generation 检查均为静态可分析路径。旧模板、旧 Popup 和旧 callback 必须在 detach/reapply 时释放，避免非 Visual host 被长期持有。

## 10. 兼容性、定制与验证

- 不改变 Popup 的 placement、shadow、surface ownership、scrolling 或 native/overlay 选择；普通模式保留原 light-dismiss 输入路由，
  pinned 模式只在首次打开前抑制本来无法完成关闭的 dismiss overlay。
- 不改变控件现有 public API、公开模板 part、伪类、Token 和 Gallery 用法。
- 自定义模板继续提供原有 Popup part；内部钉住属性不要求应用改写 AXAML。
- `Dialog`、`Drawer`、`ImagePreviewer` 继续使用各自会话生命周期，不读取 Popup pinned 状态。

验证必须覆盖：

- Popup 自动打开、普通 Close/Escape/外点拦截、关闭动效不启动；
- PlacementTarget/owner detach、TopLevel 切换、模板重建和窗口销毁正常清理；
- Select/TreeSelect/Cascader 的业务状态和 Popup 状态一致；
- Flyout、ToolTip、ContextMenu 和代表性菜单/Picker 家族的 owner relay；
- 首次 pinned 打开前 light-dismiss 已禁用，取消 pin 后恢复 Click/Hover/Focus 或模板原配置；
- 属性和 Property 字段不为 public；
- OverlayPopupHost 不残留，重新 attach 后 pinned Popup 能恢复；
- Desktop Controls、DataGrid/ColorPicker 专项测试和文档链接检查。
