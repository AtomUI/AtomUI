# TabControl / TabStrip 溢出弹层设计

本文档定义 `TabControl`、`CardTabControl`、`TabStrip` 与 `CardTabStrip` 共用的溢出弹层契约。控件公共定位分别见
[TabControl 桌面版架构设计](overview.md) 与
[TabStrip 桌面版架构设计](../tab-strip/overview.md)，内部维护入口分别见
[TabControl 桌面版实现原理](implementation.md) 与
[TabStrip 桌面版实现原理](../tab-strip/implementation.md)。

## 1. 设计定位

页签溢出弹层把当前视口中未完整显示的页签投影为可导航、可关闭的临时列表，并允许应用用一个
`IDataTemplate` 替换默认列表内容。它只改变溢出内容的呈现，不改变页签集合、选择、关闭、拖动排序或内容页的
owner。

四个控件共享同一套模型、宿主和生命周期。Line/Card 只影响页签与默认菜单的视觉，`TabControl` / `TabStrip`
只影响页签标题如何投影以及选择、关闭如何提交。

## 2. 设计原则

- `BaseTabControl` 与 `BaseTabStrip` 是集合、选择和关闭语义的唯一 owner；弹层和自定义内容只调用 owner 能力。
- 溢出检测、Popup 宿主、快照、默认菜单与生命周期由一个 internal `TabScrollViewer` 实现，不按控件或外观复制。
- Popup shell 静态存在于 ControlTemplate；Popup 内容第一次打开时才创建，并在控件仍挂载且模板不变时复用。
- 每次打开重新生成不可变的溢出快照；关闭后立即清除快照中的 item、header、template 和 owner action 引用。
- 外部集合在弹层打开期间发生变化时关闭弹层，不在旧快照上做增量同步。
- 选择变化可以重新发布不可变快照；任何 action 都必须验证参数属于当前打开会话。
- 每个 item 不建立实例事件或命令订阅；默认菜单使用类级事件处理和直接方法调用。
- template reapply、detach、模板替换和生命周期关闭拥有完整释放权，不能被测试用 pinned-open 状态阻止。
- 运行时路径不使用反射、字符串成员访问、动态类型发现或运行时生成代码。

## 3. 专项模型与 Public API

### 3.1 自定义入口

`BaseTabControl` 与 `BaseTabStrip` 分别注册同名 StyledProperty；其四个 public 派生控件直接继承：

```csharp
public static readonly StyledProperty<IDataTemplate?> OverflowPopupTemplateProperty;

public IDataTemplate? OverflowPopupTemplate { get; set; }
```

默认值为 `null`。`null` 使用 AtomUI 默认溢出菜单；非空模板替换 Popup 内容。模板的 data item 为当前
`TabOverflowPopupContext`，应用不需要访问 internal scroll viewer、Popup 或 item container。

属性改变时，当前 Popup 走生命周期关闭并释放旧模板生成的缓存内容；下一次打开使用新模板。该属性不改变 Popup
定位、触发方式、light-dismiss、选择、关闭和集合所有权。

### 3.2 `TabOverflowPopupContext`

`TabOverflowPopupContext` 是 public、sealed、实现 `INotifyPropertyChanged` 的会话视图模型：

```csharp
public sealed class TabOverflowPopupContext : INotifyPropertyChanged
{
    public IReadOnlyList<TabOverflowItem> Items { get; }
    public TabOverflowItem? SelectedItem { get; }

    public bool TryActivate(TabOverflowItem item);
    public bool TryClose(TabOverflowItem item);
    public void Dismiss();
}
```

契约如下：

- `Items` 只包含当前打开时未完整显示的页签，顺序与 owner 逻辑集合一致；调用方不能修改该列表。
- `SelectedItem` 仅在当前选中页签也位于 `Items` 中时非空。
- `TryActivate` 只在参数属于当前会话、源页签仍有效且可用时提交选择并把源页签带入视口；它不隐式调用
  `Dismiss`，默认模板与 Gallery 搜索模板在成功后显式关闭。
- `TryClose` 只在参数属于当前会话、源页签仍有效且可关闭时调用 owner 的统一关闭路径；它不直接修改集合。
- `Dismiss` 请求普通关闭；pinned-open 可以拦截普通关闭，但不能拦截生命周期清理。
- 弹层关闭后 `Items` 变为空列表、`SelectedItem` 变为 `null`，全部 action 返回 `false` 或成为无操作。
- context 只以弱引用访问当前 action target。应用即使保存旧 context，也不能因此保留 owner、scroll viewer、Popup
  或旧模板视觉树。
- 打开或选择刷新以“替换整个只读列表”的方式更新 context，不发送增量集合通知；先同时写入新的 `Items` 与
  `SelectedItem`，再按 `Items`、`SelectedItem` 顺序仅为实际变化的属性触发 `PropertyChanged`。普通关闭使用同一通知顺序
  发布空列表与 `null`。

### 3.3 `TabOverflowItem`

`TabOverflowItem` 是 public、sealed、不可变的页签投影：

```csharp
public sealed class TabOverflowItem
{
    public object? Item { get; }
    public object? Header { get; }
    public IDataTemplate? HeaderTemplate { get; }
    public bool IsEnabled { get; }
    public bool IsSelected { get; }
    public bool IsClosable { get; }
}
```

投影映射为：

| Owner | `Item` | `Header` | `HeaderTemplate` | action owner |
| --- | --- | --- | --- | --- |
| `BaseTabControl` | 当前逻辑 item | `TabItem.Header` | `TabItem.HeaderTemplate` | `BaseTabControl` |
| `BaseTabStrip` | 当前逻辑 item | `TabStripItem.Content` | `TabStripItem.ContentTemplate` | `BaseTabStrip` |

`IsEnabled`、`IsSelected` 与 `IsClosable` 都取自打开时容器的 effective state。投影不公开容器操作入口；内部会话
维护投影与源容器的映射，并在关闭时整体清除。

应用主动保存单独的 `TabOverflowItem` 时，该对象会按普通 CLR 引用语义保留自身的 `Item`、`Header` 与
`HeaderTemplate` 值；这是调用方拥有的快照，不是控件泄漏。无论调用方保存多久，旧 projection 都不能继续操作 owner，
也不能通过内部映射保留 owner、容器或 Popup。

## 4. 控件与状态策略

### 4.1 控件矩阵

| 控件 | Owner | 外观 | 内容页 | 自定义 API |
| --- | --- | --- | --- | --- |
| `TabControl` | `BaseTabControl` | Line | 有 | 继承 `OverflowPopupTemplate` |
| `CardTabControl` | `BaseTabControl` | Card | 有 | 继承 `OverflowPopupTemplate` |
| `TabStrip` | `BaseTabStrip` | Line | 无 | 继承 `OverflowPopupTemplate` |
| `CardTabStrip` | `BaseTabStrip` | Card | 无 | 继承 `OverflowPopupTemplate` |

### 4.2 Placement 映射

| `TabStripPlacement` | Popup placement | 对齐语义 |
| --- | --- | --- |
| `Top` | `BottomEdgeAlignedLeft` | 弹层位于更多按钮下方，左边缘对齐 |
| `Bottom` | `TopEdgeAlignedLeft` | 弹层位于更多按钮上方，左边缘对齐 |
| `Right` | `LeftEdgeAlignedBottom` | 弹层位于更多按钮左侧，底边缘对齐 |
| `Left` | `RightEdgeAlignedBottom` | 弹层位于更多按钮右侧，底边缘对齐 |

Popup 始终使用 overlay host、light-dismiss，不显示箭头。更多按钮仍只在存在溢出时可见，普通入口使用 click
打开。Popup 阴影层必须沿宿主 `ContentPresenter` 链解析到最终 surface，并使用最终 surface 的
`CornerRadius` 绘制阴影；不能以中间 presenter 的默认零圆角绘制直角外框。默认菜单和 Gallery 搜索模板的最终
surface 均使用 `PopupCornerRadius` / `BorderRadiusLG` 语义。

Tab 轨道的溢出提示与 Popup surface 阴影是两种不同语义。轨道提示采用 Ant Design 参数的四向普通外阴影，且只沿
滚动主轴出现：

| 主轴边缘 | `BoxShadow` | 使用位置 |
| --- | --- | --- |
| Left | `10 0 8 -8 rgba(0,0,0,0.08)` | `Top` / `Bottom` 的起始边缘 |
| Right | `-10 0 8 -8 rgba(0,0,0,0.08)` | `Top` / `Bottom` 的结束边缘 |
| Top | `0 10 8 -8 rgba(0,0,0,0.08)` | `Left` / `Right` 的起始边缘 |
| Bottom | `0 -10 8 -8 rgba(0,0,0,0.08)` | `Left` / `Right` 的结束边缘 |

`ScrollContentViewport` 中的 Canvas 与真实 presenter 同尺寸，只负责阴影定位和裁剪，不参与命中测试。
普通外阴影不绘制在载体矩形内部，因此两个透明 `TabOverflowEdgeIndicator` 必须位于 viewport 外侧：水平 start 的右边等于 `0`，
end 的左边等于 viewport width；垂直 start 的底边等于 `0`，end 的顶边等于 viewport height。
主轴尺寸使用 `MenuEdgeThickness`，交叉轴尺寸和 Canvas 定位使用 compiled binding 跟随真实 `Bounds`。
阴影向内容区渐淡，Canvas 的裁剪边界使更多按钮和相邻内容保持干净；不能关闭所有祖先裁剪来掩盖几何错误。
`TabOverflowEdgeIndicator` 在 Skia 实际绘制时按当前矩阵的线性部分补偿 offset，保持 Token 的逻辑 DIP 参数；
这修正当前 Avalonia 在高 DPI 下 offset 与 blur/spread 缩放不一致的问题。draw operation 只保存值快照，不持有
页面或窗口。升级 Avalonia 时需重新核对后端变换顺序，参见 [implementation.md](implementation.md)。
这些节点不占用页签布局空间，也不增加运行时 C# binding、timer 或事件订阅。

### 4.3 会话状态

| 状态 | 内容与订阅 | 允许的转换 |
| --- | --- | --- |
| Closed / never opened | 只有静态 Popup shell；没有 context 数据、item 引用或打开态订阅 | click、pinned-open 或有效状态收敛到 Open |
| Open / first use | 创建一次模板内容，发布当前快照，订阅集合与选择 | dismiss、action、invalidation、teardown |
| Open / reused | 复用模板视觉树，重新发布当前快照和 action target | 与 first use 相同 |
| Closed / cached | 保留无数据的模板视觉树；context 已清空，打开态订阅已释放 | reopen 或 teardown |
| Torn down | Popup child、context、模板引用、owner、订阅全部释放 | 新模板应用后重新进入 never opened |

普通 light-dismiss 保留自定义内容自己的纯视觉状态；Gallery 搜索词因此可以跨 light-dismiss 保留。选择成功时搜索
组件主动清空搜索词后再关闭。template reapply、detach 或 `OverflowPopupTemplate` 替换不保留该状态。

## 5. 架构、文件结构与职责

```text
BaseTabControl / BaseTabStrip (ITabOverflowOwner)
  -> TabScrollViewer
     -> Popup#PART_OverflowPopup
        -> cached template root
           -> default TabOverflowMenu
              -> TabOverflowMenuItem × N
           -> or application IDataTemplate root
```

| 节点 | 可见性 | 生命周期 owner | 职责 | 不负责 |
| --- | --- | --- | --- | --- |
| `ITabOverflowOwner` | internal | 对应 base control | 容器投影、逻辑 item 解析、选择与关闭提交、集合/选择通知 | Popup 视觉和溢出几何 |
| `TabScrollViewer` | internal sealed | ControlTemplate | 滚动、溢出几何、Popup 会话、缓存与释放 | 业务集合变更和关闭事件 |
| `TabOverflowPopupContext` | public sealed | `TabScrollViewer` | 向模板公开不可变快照和受验证 action | 保存强 owner 引用 |
| `TabOverflowItem` | public sealed | 当前快照 | 描述一个溢出页签的只读状态 | 事件、命令和集合操作 |
| `TabOverflowMenu` | internal | 默认模板内容 | 默认列表、焦点与键盘导航 | owner-specific 分支 |
| `TabOverflowMenuItem` | internal | 默认菜单容器 | 标题、选择态、禁用态和关闭入口视觉 | 保存源 Tab 容器 |

`TabScrollViewer`、context 和默认菜单都只实现一份。Line/Card、TabControl/TabStrip 的差异通过 owner projection 和
ControlTheme 输入表达，不创建派生 scroll viewer 或派生 overflow menu item。

Popup 打开不是焦点导航动作。pointer 点击更多按钮后，焦点继续保留在 `PART_ScrollMenuIndicator`；实现不得在
`Popup.Opened` 中自动聚焦 selected/first item，也不得让自定义内容根节点成为默认焦点目标。用户通过 Tab、方向键或
直接点击输入框显式进入弹层内容后，才执行菜单/搜索自身的键盘导航。

## 6. Template、组合与集成契约

`TabScrollViewerTheme` 保留 `PART_ScrollStartEdgeIndicator`、`PART_ScrollEndEdgeIndicator` 与
`PART_ScrollMenuIndicator`，并增加 `PART_OverflowPopup`。Popup shell 与滚动内容处于同一个 ControlTemplate：

```text
TabScrollViewer
  -> Panel#RootLayout
     -> DockPanel#ScrollViewLayout
        -> IconButton#PART_ScrollMenuIndicator
        -> Panel#ScrollContentViewport
           -> TabScrollContentPresenter#ScrollViewContent
           -> Canvas (viewport-sized, clipped, input-transparent)
              -> TabOverflowEdgeIndicator#PART_ScrollStartEdgeIndicator
              -> TabOverflowEdgeIndicator#PART_ScrollEndEdgeIndicator
     -> Popup#PART_OverflowPopup
        -> lazy cached content root
```

`TabScrollViewer` 通过 internal DirectProperty 向静态模板提供 `IsOverflowPopupOpen`、
`OverflowPopupContext`、`OverflowPopupTemplate` 与 `OverflowPopupPlacement`。owner ControlTheme 使用
`TemplateBinding` 把 public `OverflowPopupTemplate` 传入 scroll viewer；Popup 的打开状态、context 和 placement 不通过
`RelayBind` 或字符串 binding 连接。

默认模板使用一个 `TabOverflowMenuTheme`。默认 item container 通过类级 handler 处理激活与关闭，不为每个 item
注册 `Click`、`CloseTab` 或 `ICommand.CanExecuteChanged`。

应用模板负责自己的布局、搜索、空状态和可访问名称，但必须只通过 context 操作页签。应用不得依赖 internal
scroll viewer、Popup、默认菜单类型、默认 item container 或其视觉树。

更多按钮是滚动 viewport 的固定相邻槽位，而不是可被滚动内容覆盖的浮层。只要 `Extent` 超过 `Viewport`，布局就必须
在主轴上为激活器保留完整尺寸；父级响应式缩窄、瀑布流换列以及最终 arrange 小于先前 measure 时也不能把激活器排到
owner 或裁剪区域之外。

默认与自定义 overflow 列表使用隐藏滚动条模式：内容超过 `300` 高度时仍支持滚轮、触控板、键盘和程序化 offset
滚动，但滚动条不生成可见 gutter，也不能覆盖条目右侧视觉面。菜单内容 padding 和条目外边距必须使 hover/selected
背景的左右留白相等。

## 7. 核心算法、数据流与生命周期

### 7.1 溢出快照

输入为 owner 当前容器序列、`TabStripPlacement`、scroll `Offset` 与 `Viewport`。坐标均使用 scroll content 的布局
坐标，判断前先沿主轴减去当前 offset：

```text
horizontal: floor(item.Left - Offset.X) < 0
         || floor(item.Right - Offset.X) > Viewport.Width

vertical:   floor(item.Top - Offset.Y) < 0
         || floor(item.Bottom - Offset.Y) > Viewport.Height
```

命中任一条件即进入快照；未生成容器或容器已失效的 item 跳过。输出按逻辑 index 排序。快照发布后不可原位修改；
选择变化需要重新投影时发布新列表。外部 collection add/remove/move/replace/reset 立即关闭当前会话，下一次打开从新
布局重新计算。

打开过程中，先建立当前会话的 owner 订阅，再发布 context 通知。每次通知或模板构建返回后都校验会话、owner 与
context 身份；同步回调导致 dismiss、集合失效、detach 或模板替换时，本次打开立即结束。应用模板在挂载前只构建一次，
已构建根经 presenter 保持 DataContext 继承与应用局部覆盖，校验通过后才成为 Popup child。整个关闭事务拒绝同步
回调重开，清理完成后才恢复可打开状态。

### 7.2 Action 流

```text
custom/default view action
  -> context validates current session and item identity
  -> weak action target
  -> ITabOverflowOwner
     -> unified selection or CloseTab
        -> Items / ItemsSource + events + selected state
```

`TryActivate` 对 disabled、已失效或旧会话 item 返回 `false`。成功选择后默认呈现先清理自己的瞬时输入状态，再调用
`Dismiss`。`TryClose` 必须经过 owner 的 `IsClosable`、`Closing`、cancel、集合更新、选择回放和 `Closed` 顺序；集合通知使
当前快照失效并关闭 Popup。

### 7.3 Acquire / release 对

| Acquire | Release |
| --- | --- |
| 模板应用取得更多按钮与 Popup part | 下一次 `OnApplyTemplate` 开始时解除旧 part、关闭并清空 |
| 首次打开创建 context 和模板视觉树 | template reapply、detach 或模板替换时释放 |
| 每次打开发布 item/header/template 快照 | 任意关闭路径清空 |
| 打开时订阅 owner collection/selection | 关闭、re-template、detach 时解除 |
| pinned-open 读取 owner 状态并打开 Popup | 生命周期关闭优先执行，detach 后不自旋重试 |
| Popup surface 穿过嵌套 `ContentPresenter` 建立圆角 relay | presenter child 变化、surface 替换或宿主 detach 时解除旧链和旧 surface binding |

关闭流程必须幂等。重复 close、Popup 自身 `Closed`、collection invalidation 与 detach 可以到达同一清理入口，但每个
handler 只解除一次。Pinned-open 的重新收敛只由 attach、模板应用、布局/可见性或属性变化驱动，不使用
`Dispatcher.Post` 循环和延迟重试。

## 8. 资源、性能与 AOT 边界

### 8.1 关闭态资源不变量

- 没有打开会话级 collection/selection、per-item button/command、dispatcher callback 或 owner-action subscription。
  `PART_ScrollMenuIndicator.Click` 与 `PART_OverflowPopup.Closed` 属于 template-part 生命周期订阅，可以在关闭态继续存在，
  但必须在下一次 re-template 或 detach 时成对解除。
- context 的 `Items` 为空，`SelectedItem` 和 weak action target 为空。
- 缓存模板视觉树可以保留自身控件状态，但其 DataContext 不能保留上一次快照或 owner。
- detach/re-template 后不保留缓存视觉树、context、`IDataTemplate`、Popup child、owner 或旧 template part。
- DynamicResource 模板与其 anchor 只能由当前 owner/property/template tree 的正常生命周期保持，不能被旧 context 或
  static cache 额外持有。
- Popup host 对嵌套 `ContentPresenter` 的 surface 追踪必须使用可枚举、可成对解除的实例订阅；surface 替换后不得由
  旧 `CornerRadius` binding 保留旧模板根，宿主 detach 后不得保留 presenter chain。

### 8.2 分配与性能门禁

关闭且从未打开的实例只增加一个无 Child 的 Popup shell，不建立打开会话级监听或创建默认菜单；模板生命周期的
more-button / Popup part handler 只建立一次，并在 re-template / detach 时解除。第一次打开只创建一次内容视觉树；
重复打开只分配不可避免的不可变 projection 和快照数组。

实现不得在每次打开创建 Flyout、`CompositeDisposable`、relay binding、per-item delegate、command 或 dispatcher
closure。默认菜单沿正常的容器 Create/Prepare 流程复用空容器与控件模板；关闭时清除快照、Header、HeaderTemplate、
DataContext 与标题 presenter 的旧模板子树。缓存由单个菜单根拥有，上限随最近一次非空快照缩小，Context 更换时
丢弃，不保留历史最大规模或旧会话数据。性能回归以相同页签数量、viewport、主题和运行时参数比较：

- 重复 open/close 的 allocated bytes/op 与 Gen0 压力相对基线至少降低 30%。
- 重复 open/close 的中位耗时不得回退；5% 以内且落在测量噪声中的差异需用多轮结果证明。
- 未打开控件的创建、首次布局、steady-state measure/arrange 和滚动不得出现超过 5% 的稳定回退。
- 100 次 open/close 后，对象计数必须回到第一次 close 后的 warm baseline：只允许保留当前空 context 与一棵缓存视觉树，
  不允许旧 projection、数据、携带旧会话状态的 item container、订阅或批次间正斜率增长。完整 teardown 后，context、缓存视觉树与 owner
  都必须可回收。

任一稳定性能回退或对象保留都阻断交付，不能以功能对齐抵消。

### 8.3 AOT

Public context 是强类型 CLR API，Gallery 模板使用 compiled binding 或直接属性访问。实现不使用 reflection binding、
assembly/type 扫描、`Activator.CreateInstance(Type)`、表达式编译或运行时注册。默认 ControlTheme、`IDataTemplate` 和
静态注册遵循 [TypeMap 控件与资源契约](../../../../architecture/foundations/control-registration-contracts.md)；新类型、主题资产和语义样式必须进入正确的逐控件事实并通过
真实 NativeAOT Gallery publish。

## 9. 兼容性与定制边界

- `OverflowPopupTemplate=null` 的默认呈现继续提供 menu-like 导航、禁用、选中和可关闭语义。
- `PART_ScrollMenuIndicator`、四向 placement、溢出判定、选择/关闭事件顺序和逻辑集合 ownership 保持稳定。
- `OverflowPopupTemplate`、`TabOverflowPopupContext` 与 `TabOverflowItem` 是 public customization contract。
- `TabScrollViewer`、`ITabOverflowOwner`、`TabOverflowMenu`、`TabOverflowMenuItem`、Popup part 以外的内部节点和缓存策略
  不是应用扩展点。
- 应用模板不得假设 `Items` 长期稳定；它必须响应 `PropertyChanged`，并把旧 item 视为仅对当前会话有效。
- 本专项不改变 Tab activation、drag reorder、close API、add button、content presenter、TabItem theme 或非 overflow
  滚动行为。
- 本专项为 TabControl 家族定义 `BoxShadowTabsOverflowLeft/Right/Top/Bottom` 四个方向阴影 Token，并以
  `MenuEdgeThickness=ControlHeight` 约束阴影承载矩形；默认 popup、menu、input 和空状态继续复用既有 SharedToken 与
  Popup/Menu/Input Token。

## 10. Gallery 搜索体验

Gallery 使用一个可复用的 `SearchableTabOverflowPopup` 展示 public customization contract，四个控件共用同一实现。

- surface 宽 `200`，使用 elevated/container 背景、LG 圆角、Popup 阴影并裁剪溢出。
- 模板根与可见 surface 使用同一个 LG 圆角，使 Popup host 的阴影 mask 与内部背景完全一致；只给内部 `Border`
  设置圆角而让模板根保持零圆角属于错误实现。
- 搜索区使用 `paddingXS × paddingSM` 与底边框；输入框包含 Search 图标、clear affordance 和
  `Search tabs...` placeholder。
- 列表最大高度 `300`，超出后仅列表纵向滚动；滚动条保持隐藏且不占用或覆盖右侧内容宽度，条目 hover/selected
  背景的左右留白相同；没有命中项时显示居中 disabled-color 文本
  `No matching tabs`。
- 匹配使用 `StringComparison.OrdinalIgnoreCase`，不为每个 item 创建 `ToLower` 临时字符串。
- `ArrowUp` / `ArrowDown` 从输入框把焦点交给菜单；菜单跳过 disabled item，并支持方向键、Home/End、Enter 与 Escape。
- 弹层打开时搜索根节点、搜索框和第一菜单项均不自动获得焦点；pointer 激活后的焦点保持在更多按钮。
- 选择时先清空 query，再调用 `TryActivate` 和 `Dismiss`；普通 light-dismiss 保留 query，控件 detach 或模板释放时清除。
- 每次打开都从 context 的最新 `Items` 过滤，不缓存旧 `TabOverflowItem` 或 owner。
- item button 直接绑定 `Content=Header` 与 `ContentTemplate=HeaderTemplate`，由按钮自身的 content presenter 呈现；不得
  再把一个手工 `ContentPresenter` 作为按钮内容，否则数据模板上下文可能丢失并生成空白菜单项。

TabControl 与 TabStrip 页面各提供一个稳定 `ShowCaseItem.SourceKey`，每个示例同时覆盖 Line 与 Card 变体；示例源码只
使用 public API。示例 owner 使用 `MaxWidth=720` 与 stretch 对齐，不能使用会越过窄 Gallery 列的固定 `Width`。

## 11. 验证要求

| 层级 | 必须证明 |
| --- | --- |
| Public API | 两个 base owner 的属性名称、类型、默认值、继承；context/item 类型与方法语义 |
| Projection | 四个控件、四向 placement、部分可见、disabled、selected、closable、custom header template |
| Default UI | 默认菜单导航、关闭、空快照不打开、placement、light-dismiss、focus 返回、隐藏滚动条仍可滚动、最终 surface 圆角与阴影 mask 一致 |
| Custom UI | 搜索、clear、无结果、大小写、键盘、selection 清 query、light-dismiss 保留 query、隐藏滚动条仍可滚动、左右留白对称、真实 header/template 文本可见、模板根与 host 圆角一致 |
| Collection | Items 与可写/只读 ItemsSource、外部 add/remove/move/reset 关闭、旧 action 拒绝 |
| Lifecycle | 首次惰性创建、重复复用、100 次 open/close、template replace、re-template、detach、pinned lifecycle close |
| Retention | ordinary close 回到固定 warm baseline；teardown 后 owner、context、snapshot item、header/template、Popup child、DynamicResource anchor 的 weak-reference 回收；嵌套 presenter 替换后的旧 surface 可回收 |
| Performance | cold/closed、首次打开、重复打开、关闭与 Gallery 导航的时间、分配、Gen0 和对象计数门禁 |
| Theme | Light/Dark、Line/Card、Desktop/Browser、四向 placement 的普通外阴影参数/方向、视口外侧绘制载体、阴影层裁剪边界与 ControlTheme asset 注册 |
| AOT | AOT/trim analyzer、TypeMap 精细保留与真实 Gallery NativeAOT publish/start smoke |

自动化测试必须保留触发溢出的真实尺寸约束，不得通过扩大 viewport 或移除 header/add button 来绕过原始布局。
