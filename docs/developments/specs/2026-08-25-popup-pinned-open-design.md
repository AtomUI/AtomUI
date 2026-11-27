# Popup Pinned-Open Design

**状态：** 设计稿，尚未进入运行时代码实现  
**分支：** `codex-popup-pinned-open`  
**日期：** 2026-08-25

## 1. 目标

为 AtomUI 所有基于 `AtomUI.Desktop.Controls.Popup` 的弹出控件提供一个仅供程序集内部和测试使用的“钉住打开”能力：

```csharp
select.IsPopupPinnedOpen = true;
```

设置为 `true` 后，控件应在条件满足时自动打开对应 Popup，并阻止普通用户交互或业务代码关闭它，便于测试人员观察弹出内容、测量布局、执行截图和验证 Popup 内部交互。

该能力必须满足以下约束：

- 属性不能进入 public/protected API。
- 控件本身和底层 Popup 都要有状态入口，不能只钉住物理 Popup。
- `true` 是请求状态，不绕过控件挂载、可见性、PlacementTarget 和 TopLevel 生命周期约束。
- `false` 只解除钉住，不强制关闭当前 Popup。
- 控件卸载、PlacementTarget 脱离视觉树、跨 TopLevel、窗口销毁等生命周期清理必须优先于钉住状态。
- 关闭动画不能在钉住模式下启动。
- 不修改现有 Popup 的定位、滚动、Overlay、模板和输入路由行为。

## 2. 术语和状态

### 2.1 请求状态与有效状态

`IsPopupPinnedOpen` 是测试请求状态，不等于“当前 Popup 正在显示”。

| 状态 | 含义 |
| --- | --- |
| `Pinned = false` | 正常的打开、关闭、失焦、Escape、外点和动画行为 |
| `Pinned = true` | 在 Anchor/TopLevel/可见性等条件满足时强制保持打开 |
| `Pinned = true` 但 Anchor 不可用 | 暂不打开或允许生命周期关闭；条件恢复后重新收敛 |

Popup 的有效显示状态由以下条件共同决定：

```text
PinnedRequested
&& host/control is attached
&& control is visible
&& placement target is attached
&& placement target belongs to the expected TopLevel
&& popup has a valid placement/content
```

因此，钉住不是把 `IsOpen` 永久写成 `true`，而是一个带生命周期边界的状态收敛机制。

### 2.2 关闭原因

所有关闭尝试都按两类处理：

1. **普通关闭**：外部点击、Escape、失焦、控件自身关闭逻辑、`Hide`/`Close`、`IsOpen = false`、关闭动画入口。
2. **生命周期关闭**：控件或 PlacementTarget 卸载、不可见、迁移到其他 TopLevel、窗口销毁、模板重建和明确的资源清理。

`Pinned = true` 只拦截普通关闭；生命周期关闭必须允许执行，并且不能被关闭动画延迟。

## 3. 方案选择

### 方案 A：只在 Popup 上增加属性

```csharp
popup.IsPopupPinnedOpen = true;
```

优点是实现范围小；缺点是 Select、ComboBox、DatePicker 等控件的业务状态仍可能先变成关闭，导致 `IsDropDownOpen`、伪类、候选项订阅和 Popup 实际状态不一致。该方案不采用。

### 方案 B：每个具体控件各自实现

在 `Select`、`TreeSelect`、`Cascader`、`DatePicker` 等叶子控件中复制钉住逻辑。该方案会复制关闭入口、模板重建、生命周期和调度代码，容易出现某个控件漏掉 Escape、失焦或卸载路径。该方案不采用。

### 方案 C：两层协调模型（推荐）

由 `Popup` 负责物理弹出层的关闭拦截、关闭动画和生命周期白名单；由每个控件语义基类或宿主负责业务状态、内容准备和重新打开；两层通过内部 Avalonia 属性 relay 连接。

```text
测试/内部代码
    |
    v
具体控件或语义基类的 internal IsPopupPinnedOpen
    |
    | 业务状态收敛：IsDropDownOpen / IsOpen / IsVisible
    v
实际 Popup 的 internal IsPopupPinnedOpen
    |
    | 物理关闭拦截、动画取消、Overlay 清理
    v
OverlayPopupHost / PopupRoot
```

该方案保留现有控件的业务契约，同时确保 Popup 实际显示状态与业务状态一致。

## 4. 内部 API 契约

### 4.1 Popup 根属性

在 [Popup.cs](/Users/chinboy/Projects/dotnet/AtomUIV6/.worktrees/feature/popup-pinned-open/src/AtomUI.Desktop.Controls/Popup/Popup.cs) 的内部属性区域增加：

```csharp
internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
    AvaloniaProperty.Register<Popup, bool>(nameof(IsPopupPinnedOpen));

internal bool IsPopupPinnedOpen
{
    get => GetValue(IsPopupPinnedOpenProperty);
    set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
}
```

约束：

- 字段、属性和任何辅助方法均为 `internal`，不新增 public/protected API。
- 默认值为 `false`。
- 使用 StyledProperty，是为了让宿主通过现有 binding relay 连接，不把模板部件名称暴露给测试。
- 不把该属性加入公开 AXAML 模板契约，也不让应用在普通 AXAML 中使用它。

### 4.2 语义控件属性

每个拥有独立业务打开状态的控件基类或宿主提供同名的 internal 属性。例如 `AbstractSelect`：

```csharp
internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
    AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsPopupPinnedOpen));

internal bool IsPopupPinnedOpen
{
    get => GetValue(IsPopupPinnedOpenProperty);
    set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
}
```

推荐的 owner：

| 控件族 | 属性 owner | 继承/覆盖对象 |
| --- | --- | --- |
| Select | `AbstractSelect` | `Select`、`TreeSelect`、`Cascader` |
| Date/Time Picker | `InfoPickerInput` | `DatePicker`、`TimePicker`、范围变体 |
| AutoComplete | `AbstractAutoComplete` | `AutoComplete`、TextArea 变体 |
| ComboBox | `ComboBox` | 直接 owner，因其继承 Avalonia ComboBox |
| ColorPicker | `AbstractColorPicker` | ColorPicker、GradientColorPicker |
| Mentions | `Mentions` | 直接 owner |
| Flyout 宿主 | `FlyoutHost` | PopupConfirm、DropdownButton、SplitButton、AvatarGroup 等宿主 |
| Flyout 本体 | `Flyout` | 直接使用 Flyout 的 TreeView、Transfer、菜单和溢出菜单 |
| Menu | `Menu` / `NavMenu` | 按现有菜单生命周期分别接入 |
| ToolTip | `ToolTip` | 继承或内部实例由 ToolTip 转发 |
| ContextMenu | `ContextMenu` | 转发给其手动创建的 Popup |
| Tour | `Tour` | 独立 owner，仍使用 Popup 物理层 |

不要为 `TreeSelect`、`Cascader`、`RangeDatePicker` 等叶子类型重复声明同一套逻辑。

### 4.3 测试可见性

`AtomUI.Desktop.Controls.csproj` 已经通过 `InternalsVisibleTo` 允许 `AtomUI.Desktop.Controls.Tests` 访问内部成员；不新增公开 API，也不新增运行时反射入口。可选包若需要自己的测试访问，应复用现有包级 `InternalsVisibleTo` 约定。

## 5. Popup 物理层设计

### 5.1 关闭事件的优先级

`Popup.HandlePopupClosing` 的顺序必须固定为：

1. 处理内部“生命周期关闭”作用域；如果处于作用域，允许关闭并跳过关闭动画。
2. 判断当前 Popup 是否已失去有效生命周期条件；如果是，允许关闭并跳过关闭动画。
3. 如果 `IsPopupPinnedOpen` 为 `true`，取消普通关闭，不进入 `CloseMotion`。
4. 继续现有的关闭动画逻辑。

不能简单地在现有处理函数最前面无条件写：

```csharp
if (IsPopupPinnedOpen)
{
    e.Cancel = true;
}
```

这样会让 Popup 在宿主卸载或窗口销毁时无法清理，造成 `OverlayPopupHost`、事件订阅和资源引用残留。

### 5.2 生命周期关闭机制

在 `Popup` 内增加内部生命周期关闭作用域/计数器，建议提供语义明确的方法：

```csharp
internal IDisposable AllowLifecycleClose();
internal void CloseForLifecycle();
```

`CloseForLifecycle()` 必须：

- 设置生命周期关闭标记或进入作用域。
- 取消正在执行的关闭动画。
- 调用现有 `Close()`/`IsOpen = false` 路径。
- 使 `Closing` 不被 pinned 拦截。
- 使 `OverlayPopupHost` 正常 detach。

优先改造 AtomUI 自己能够识别的生命周期调用点，例如 `HandlePlacementTransformChanged` 中 PlacementTarget 离开当前 TopLevel 可视区域的关闭路径；对 Avalonia 内部触发的 Closing 事件，则使用已记录的 `_openTopLevel`、逻辑树挂载状态和 PlacementTarget 状态作为兜底判断。

### 5.3 强制打开和重新收敛

Popup 需要一个可合并的 UI 调度队列，避免 property changed、`Closed`、模板重建和宿主 reattach 同时触发多次打开：

```text
QueuePinnedOpen()
    -> 只允许一个 pending callback
    -> 检查 IsPopupPinnedOpen
    -> 检查 IsAttachedToVisualTree / PlacementTarget / TopLevel
    -> 若不可用，保持请求状态但不重试空转
    -> 若可用且 IsOpen=false，设置 IsOpen=true
```

打开失败不能用无限 `Dispatcher.Post` 重试。重新尝试的触发点只能来自：

- `OnAttachedToVisualTree`。
- `OnApplyTemplate`。
- `IsPopupPinnedOpen` 变更为 `true`。
- PlacementTarget/TopLevel/可视状态重新变为有效。
- Popup 的定位跟踪回调发现目标重新回到有效区域。

### 5.4 动画约束

- pinned 状态下普通关闭被取消，因此不启动 `CloseMotion`。
- 生命周期关闭跳过关闭动画，保持当前 `PopupLifecycleTests` 对 detach 清理的预期。
- `CancelCloseAnimation()` 继续用于解除正在排队的普通动画，但必须不会把生命周期关闭重新打开。
- 打开动画可以保留；测试若需要稳定截图，可在控件现有 `IsMotionEnabled` 上关闭动画，不为 pinned 机制新增第二个动画开关。

## 6. Select 接入设计

### 6.1 属性放在 AbstractSelect

`Select`、`TreeSelect` 和 `Cascader` 共享 `AbstractSelect` 的 `IsDropDownOpen`、`Popup`、`EnsurePopupContent`、打开/关闭事件和可见性监听，因此属性和状态机必须放在 `AbstractSelect`。

模板 `SelectTheme.axaml` 保持不变。`OnApplyTemplate` 找到 `PART_Popup` 后建立 relay：

```csharp
Popup[!Popup.IsPopupPinnedOpenProperty] =
    this[!IsPopupPinnedOpenProperty];
```

不要把 internal 测试属性加入 AXAML `TemplateBinding`，避免改变模板契约和生成式样式路径。

### 6.2 业务关闭入口

`HandleIsDropDownOpenChanged` 必须在调用 `ClosingDropDown` 前收敛 pinned 状态：

```text
new IsDropDownOpen = false
    |
    +-- lifecycle cleanup? -> 允许关闭
    |
    +-- IsPopupPinnedOpen && host ready? -> 恢复 true，保持业务打开状态
    |
    +-- otherwise -> 进入原有 ClosingDropDown
```

这覆盖鼠标外点、Escape、失焦、Select 内部关闭和测试代码直接设置 `IsDropDownOpen = false`。恢复值时必须使用已有 `SetDropDownOpenWithoutPropertyHandling`，避免重复触发 `ClosingDropDown`。

### 6.3 打开收敛

`EnsurePinnedPopupOpen()` 的顺序：

1. 检查控件已挂载且可见。
2. 检查 `Popup` 已从模板取得。
3. 恢复 `IsDropDownOpen = true`，但跳过 property handler。
4. 调用 `EnsurePopupContent()`。
5. 设置 Popup 的 pinned relay 已生效。
6. 设置 `Popup.IsOpen = true`，由 `Popup` 负责最终物理层收敛。
7. 保持 `PopupHasOpened`、伪类和已有 `DropDownOpened` 生命周期一致。

`PopupClosed` 不能在 pinned 状态下直接把业务状态当作永久关闭；需要区分普通关闭被取消、生命周期关闭和模板重建。控件重新挂载或目标重新有效后，应再次收敛。

### 6.4 Select 测试使用方式

```csharp
var select = new Select
{
    OptionsSource =
    [
        new SelectOption { Header = "One" },
        new SelectOption { Header = "Two" }
    ],
    IsPopupPinnedOpen = true
};
```

测试不应通过 `PART_Popup` 查找后直接操作 Popup 来模拟 Select 的 pinned 行为；这样无法验证业务状态是否保持一致。

## 7. 其他控件族接入

### 7.1 InfoPickerInput（DatePicker/TimePicker）

`InfoPickerInput` 已持有 `PickerPopup`，DatePicker、TimePicker 和范围变体共用其打开/关闭和 PlacementTarget 设置。属性放在 `InfoPickerInput`，模板应用后 relay 到 `PickerPopup`。范围控件在切换主/副输入框作为 PlacementTarget 时，重新执行 pinned 收敛，不能复用失效的旧 TopLevel 判断。

### 7.2 AutoComplete、ComboBox、Mentions

这些控件当前保有各自的 `_popup` 和候选项状态，接入顺序必须是：

1. 先维护控件的业务 open state。
2. 再 relay 到真实 Popup。
3. 关闭入口在清理候选项、焦点或过滤状态前拦截。
4. 仅在生命周期关闭时清理订阅和 Popup host。

不能只把 `_popup.IsLightDismissEnabled` 设为 `false`，因为 Escape、程序关闭和绑定写回仍会改变业务状态。

### 7.3 Flyout / PopupFlyoutBase

`Flyout` 创建 Popup 的位置是统一的物理接入点。建议：

- `Flyout` 通过 `Popup.IsPopupPinnedOpenProperty.AddOwner<Flyout>()` 暴露同名 internal 属性。
- `CreatePopup()` 建立 Flyout 到 Popup 的 property relay。
- `HideCore()` 在 pinned 且非生命周期清理时返回不关闭，并且不启动 CloseMotion。
- `ShowAtCore()` 在 AnchorTarget 有效时支持 pinned 自动打开。
- `FlyoutStateHelper` 在 pinned 状态下停止 hover leave、focus lost、click outside 的 Hide 计时器和关闭动作。
- `FlyoutHost` 同样暴露属性，并把它 relay 到其 `Flyout`，让 `PopupConfirm`、`DropdownButton`、`SplitButton`、`AvatarGroup` 等宿主可从自身入口使用。

`Flyout` 的 `false` 语义与 Popup 一致：只解除钉住，不强制 Hide。

### 7.4 ToolTip / ContextMenu

它们不是通过 `FlyoutStateHelper` 管理 Popup，需要独立转发：

- `ToolTip` 在创建 `_popup` 时绑定 `IsPopupPinnedOpen`。
- `ToolTip.Close()` 在 pinned 且非生命周期时不关闭。
- `ContextMenu.CreatePopup()` 绑定属性；Escape、菜单项关闭和 `Close()` 走统一普通关闭判断。
- 宿主控件卸载、窗口关闭或 PlacementTarget 无效时调用 Popup 的生命周期关闭入口。

ToolTip 的 attached `IsOpen` 是现有业务属性，不能用它替代内部 pinned 属性；两者职责不同。

### 7.5 Menu、NavMenu、Tour、TreeView、Transfer、Tab 溢出和 DataGrid 筛选

这些控件分别通过 `Menu`/`NavMenuItem`、`Tour` 或 `Flyout`/`TreeViewFlyout` 持有 Popup。接入原则是“在离用户最近且能表达业务状态的 owner 加 internal 属性”，不要直接修改每个菜单项的模板。

- Menu/NavMenu：钉住当前菜单或子菜单时，保留菜单业务 `IsOpen`/submenu 状态；关闭 sibling 的逻辑必须尊重 pinned owner，生命周期关闭除外。
- Tour：保持 `Tour.IsOpen`、当前步骤和 Popup 同步；切换步骤只更新内容/定位，不被 pinned 阻止。
- TreeView、Transfer、Tab 溢出、DataGrid filter：优先通过 Flyout/FlyoutHost relay；只有存在独立业务 open state 时才增加宿主属性。

### 7.6 明确排除的 Overlay

`Dialog`、`Drawer` 和 `ImagePreviewer` 使用独立的 Dialog/Overlay Host 会话生命周期，不是普通 Popup 下拉层。第一版不把 `IsPopupPinnedOpen` 直接扩展到这些类型；若未来需要测试钉住，应设计与 Dialog/Drawer 会话相匹配的独立 internal `IsOverlayPinnedOpen`，避免混淆 Popup 和窗口/会话清理语义。

## 8. 生命周期与资源安全

必须保持以下不变量：

1. Popup 的 `Closed` 最终一定发生在宿主卸载和窗口销毁路径上。
2. `OverlayPopupHost` 不因 pinned 长期残留。
3. `OnDetachedFromVisualTree`、模板重建和 PlacementTarget 切换会清理旧 Popup 的 relay、事件订阅和跟踪器。
4. 重新挂载且 `IsPopupPinnedOpen == true` 时，新的 Popup/模板部件可以重新打开。
5. 旧模板部件不应继续收到 pinned binding 更新。
6. Dispatcher 中已经排队的 pinned-open callback 在 detach 后必须自我失效，不得重新打开已卸载控件。

推荐使用 attach generation/token：每次模板重建或 detach 时递增 generation，队列 callback 捕获 generation，执行时发现不匹配立即退出。这样不需要依赖取消 Dispatcher API，也能避免旧 Popup 回调复活。

## 9. 失败和边界处理

| 场景 | 预期 |
| --- | --- |
| 设置 pinned 时控件尚未挂载 | 记录请求，不打开；挂载后打开 |
| 控件不可见 | 不打开或允许 Popup 生命周期关闭；变为可见后重新收敛 |
| PlacementTarget 为 null | 不空转重试；目标有效后重新收敛 |
| PlacementTarget 换到另一个 TopLevel | 关闭旧 Popup；新 TopLevel 有效后重新打开 |
| 模板重建 | 解绑旧 Popup，使用新 `PART_Popup` 重新 relay 和收敛 |
| 普通 `Close()` | pinned 时被取消，不启动关闭动画 |
| Escape/外点/失焦 | pinned 时被取消；业务 open state 保持 |
| 控件 detach/窗口销毁 | 允许关闭，跳过动画，清理 Overlay host |
| pinned 改回 false | 不自动关闭；恢复正常关闭行为 |
| 关闭时 Popup 没有有效 PlacementTarget | 视为生命周期清理，允许关闭 |

## 10. 测试设计

### 10.1 Popup 核心测试

扩展 [PopupLifecycleTests.cs](/Users/chinboy/Projects/dotnet/AtomUIV6/.worktrees/feature/popup-pinned-open/tests/AtomUI.Desktop.Controls.Tests/Popup/PopupLifecycleTests.cs)：

- pinned Popup 可在有效 PlacementTarget 下自动打开。
- `Close()`、`IsOpen = false`、重复 Close 都不会关闭或启动 CloseMotion。
- pinned 改回 false 后，下一次普通 Close 恢复原有动画/关闭行为。
- PlacementTarget detach、逻辑 owner detach、TopLevel 改变仍会立即关闭。
- 生命周期关闭不会留下 `OverlayPopupHost`。
- detach 后重新 attach 会重新打开 pinned Popup。
- 反射或 AvaloniaProperty 元数据确认属性及 Property 字段不是 public。

### 10.2 Select 业务测试

在 `SelectBehaviorTests` 或新的 `SelectPopupPinningTests` 中覆盖：

- `IsPopupPinnedOpen = true` 后自动打开下拉层。
- 业务 `IsDropDownOpen = false` 会恢复为 true。
- 外点和 Escape 不关闭。
- 关闭动画不启动。
- `IsPopupPinnedOpen = false` 后普通关闭恢复。
- 控件 detach 时 Popup 清理，重新 attach 后重新打开。
- `TreeSelect` 和 `Cascader` 至少各有一条继承行为验证。

### 10.3 Flyout 和手动 Popup 测试

- `FlyoutHost` pinned 时 hover leave/focus lost/click outside 不 Hide。
- `Flyout.Hide()` 不启动关闭动画；detach 仍清理。
- ToolTip 和 ContextMenu 的手动 Popup relay 生效。
- Menu、Tour、Tab overflow、DataGrid filter 至少各有一个 owner-level contract test，确认使用的是业务入口而不是直接操纵模板部件。

### 10.4 测试环境

测试使用现有 `AvaloniaTestApp`、`Dispatcher.UIThread.RunJobs()` 和 `OverlayPopupHost` 检查方式；不改变原有 Popup 的尺寸、滚动、Overlay 或 Placement 条件来制造“通过”。

## 11. 分阶段实施边界

### 阶段 1：核心和 Select 参考实现

完成 Popup 生命周期白名单、内部属性、调度收敛，以及 `AbstractSelect` relay/业务状态拦截。先让 Select、TreeSelect、Cascader 和 Popup 生命周期测试建立稳定契约。

### 阶段 2：共享基类和 Flyout

完成 `InfoPickerInput`、`AbstractAutoComplete`、`ComboBox`、`AbstractColorPicker`、`Mentions`，以及 `Flyout`、`FlyoutHost`、`FlyoutStateHelper` 接入。

### 阶段 3：手动 Popup 和菜单类控件

完成 ToolTip、ContextMenu、Menu、NavMenu、Tour、TreeView、Transfer、Tab 溢出和 DataGrid filter 的 owner-level relay 与关闭路径。

### 阶段 4：全量盘点和回归

对所有 `new Popup`、`<atom:Popup>`、`Flyout` 和 `PopupFlyoutBase` 使用点建立 inventory，确认每个点属于已接入族、明确排除族或需要单独 owner；运行 Desktop Controls 及可选包测试、`git diff --check`，必要时做 NativeAOT/trim 构建验证。

## 12. 验收标准

方案实现完成后，必须能够在测试程序集内写出：

```csharp
select.IsPopupPinnedOpen = true;
```

并满足：

- 无需访问 `PART_Popup` 或模板私有实现即可让 Select 的下拉层自动出现。
- 外点、Escape、失焦和业务关闭不会改变其有效打开状态。
- 控件卸载、窗口销毁和跨 TopLevel 迁移仍然完成清理。
- 解除 pinned 后恢复现有关闭语义。
- `IsPopupPinnedOpen` 不出现在 public API、公开模板契约或普通应用文档中。
- 所有已接入 Popup 控件族遵循同一语义，而不是各自定义不同的“强制打开”行为。

