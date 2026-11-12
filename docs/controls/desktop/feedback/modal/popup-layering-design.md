# Modal 内容弹层叠放设计

本文档定义 Overlay Dialog 内容区内弹层的宿主归属、叠放顺序、输入语义与生命周期。公共契约见 [Modal 桌面版架构设计](overview.md)，内部实现见 [Modal 桌面版实现原理](implementation.md)，宿主尺寸算法见 [Modal 宿主尺寸与 Resize 设计](host-sizing-design.md)，Popup 的 surface ownership 见 [Popup 桌面版架构设计](../../other/popup/overview.md)。

## 1. 设计定位

Dialog 内容可以包含 ComboBox、Select、DatePicker、TimePicker、AutoComplete、Tooltip、Flyout、ContextMenu 等基于 Avalonia `Popup` 的控件。本专项保证这些内容弹层与所属 Dialog 使用同一个 Window `TopLevel`，并沿用 Avalonia 的 popup 定位、light-dismiss、焦点与输入服务。

`DialogHostType.Window` 拥有独立 Window 和 `VisualLayerManager`，其内容弹层由该 Window 自身托管。本文主要描述 `DialogHostType.Overlay`。

## 2. 设计原则

- Dialog 内容、popup placement target 与 owning Window 必须位于同一个 `TopLevel` 视觉树中。
- `DialogOverlayLayer` 使用 Avalonia `OverlayLayer`；内容 popup 使用同一 `VisualLayerManager` 的 `PopupOverlayLayer`，两者之间保留 `LightDismissOverlayLayer`。
- 内容弹层必须渲染在 Dialog mask 与 Surface 之上，并保持可命中、可选择和可 light-dismiss。
- `WindowDrawnDecorations` overlay 只绘制窗口 chrome，不承载 Dialog、Drawer 或任意可能打开 Popup 的业务内容。
- 不创建嵌套 `VisualLayerManager`，不复制 Popup 的 host 创建、定位或输入算法，也不通过 child `ZIndex` 修正跨父层关系。
- 平台差异只影响 drawn chrome 是否存在和几何输入，不改变 presentation 的 `TopLevel` 所有权。

## 3. 专项模型与 Public API

| 术语 | 定义 |
| --- | --- |
| Presentation layer | `DialogOverlayLayer` 的实际父层；Window 场景为 Avalonia `OverlayLayer`。 |
| Content popup | Dialog 内容子树中的控件打开的 `OverlayPopupHost` 或原生 `PopupRoot`。 |
| Drawn chrome overlay | `TopLevelHost` 中与 Window `TopLevel` 同级的装饰视觉，仅包含 title bar、caption buttons 与 shadow。 |
| Chrome suppression lease | modal presentation 活跃期间隐藏 drawn chrome overlay 的 Window 内部引用计数租约。 |

本设计不新增 Public API。Popup 的 `ShouldUseOverlayLayer`、`IsLightDismissEnabled`、`OverlayInputPassThroughElement` 和 `OverlayDismissEventPassThrough` 保持 Avalonia 原语义。

## 4. 变体与宿主策略

| 场景 | Dialog presentation host | 内容弹层 host | Drawn chrome |
| --- | --- | --- | --- |
| AtomUI Window / 普通 TopLevel | owning `VisualLayerManager.OverlayLayer` | 同一 manager 的 `PopupOverlayLayer` 或平台 `PopupRoot` | modal 活跃时由租约隐藏 |
| 局部 overlay scope，且仍在 TopLevel 内 | 最近可用的 `ScopeAwareOverlayLayer` fallback | owning TopLevel 的 Popup 服务 | 不改变 |
| `DialogHostType.Window` | 独立 Dialog Window | 独立 Window 的 Popup 服务 | 由该 Window 管理 |

Windows CSD、Linux managed decorations 与 macOS 原生 chrome 共享同一 presentation 所有权规则。没有 drawn chrome overlay 时，suppression lease 不改变可见结果。

## 5. 架构与职责

```text
TopLevelHost
  -> Window (TopLevel)
     -> VisualLayerManager
        -> Window content
        -> OverlayLayer
           -> DialogOverlayLayer
              -> OverlayDialogPresenter (0..n)
        -> LightDismissOverlayLayer
        -> PopupOverlayLayer
           -> OverlayPopupHost (0..n)
  -> WindowDrawnDecorations overlay
     -> title bar / caption buttons / shadow only
```

| Owner | 职责 | 不负责 |
| --- | --- | --- |
| `DialogOverlayLayer` | 解析 presentation layer、维护 presenter 栈、同步 Window 可用尺寸、在最后一个 presenter 移除时释放宿主订阅 | 创建 Popup host、管理 light-dismiss、发现 drawn decorations 业务 host |
| `OverlayDialogPresenter` | 组合 mask 与 Surface；modal 活跃时获取和释放 chrome suppression lease | 改写 Popup 状态或定位 |
| `Window` | 引用计数管理 drawn chrome overlay 可见性 | 持有 Dialog 内容或 Popup host |
| Avalonia `VisualLayerManager` | 维护 Overlay、LightDismiss、Popup 等固定相对层级 | Dialog 会话状态 |
| Avalonia `Popup` | 解析 placement target、创建 host、定位、light-dismiss 与关闭 | Dialog presenter 栈 |

## 6. Template 与集成契约

`OverlayDialogPresenterTheme.axaml` 继续在同一个 presenter 中组合 mask 与 Surface。`WindowDrawnDecorationsTheme.axaml` 的 Overlay 只包含 chrome 视觉，不定义 `PART_DialogOverlayLayerHost` 或 `PART_DrawerOverlayLayerHost`。

Dialog 内容的模板可以正常创建任意 Popup，只要 placement target 保持在 Dialog 内容视觉树内。应用模板不得把内容转移到 decorations overlay 或其他无法解析 owning `TopLevel` 的视觉根。

## 7. 核心流程与生命周期

### 7.1 打开

1. `OverlayDialogPresenter` 从 placement target 解析 owning Window。
2. `DialogOverlayLayer` 从同一 TopLevel 获取 Avalonia `OverlayLayer`，并把 presenter 加入共享 Dialog 栈。
3. modal presenter 获取 Window chrome suppression lease。
4. 内容控件打开 Popup 时，Avalonia 从 placement target 解析同一 TopLevel，再选择该 Window 的 `PopupOverlayLayer` 或原生 popup。

### 7.2 叠放与输入

Avalonia 的固定层序为 Overlay、LightDismiss、Popup。Dialog 位于 Overlay，透明 light-dismiss 层覆盖 Dialog 的外部点击区域，popup host 位于最上层。点击 popup item 命中 popup；点击 popup 外部先触发 light-dismiss，再按 Popup 的 pass-through 配置决定是否继续传递。Dialog mask 的 `IsMaskClosable` 不接管 Popup 的关闭逻辑。

内容弹层可通过逻辑祖先把 pointer 事件路由到所属 `OverlayDialogPresenter`，即使其视觉分支位于独立的
`PopupOverlayLayer`。Presenter 的隧道 pointer handler 只激活 Dialog 的叠放顺序，不提前聚焦 Surface；
实际焦点由 Avalonia 根据命中目标和视觉祖先分配。否则内容会先经历一次与用户目标无关的失焦，破坏 Picker 的
候选值、确认与弹层关闭语义。点击 Surface 空白区时仍由框架把焦点分配给可聚焦的 Surface。
框架分配之后（鼠标在按下处理之后，触控与笔在同一指针抬起之后，因为它们在抬起时才聚焦目标），若焦点仍在
该 Dialog 及其内容弹层之外（不可聚焦区域、非左键），才把焦点交给 Surface，使 Escape / Enter 与输入作用于同一个 Dialog。

激活只移动已有 presenter 的集合位置，保持逻辑/视觉挂载、内容焦点和 Popup owner 生命周期；不得以移除再添加
presenter 的方式调整层序。DialogSession 打开时的初始聚焦与关闭时的焦点恢复保持独立。

### 7.3 关闭与重叠

- presenter close 或 dispose 时先从 `DialogOverlayLayer` 移除，再释放 chrome suppression lease和逻辑/资源 parent。
- 多个 modal Dialog 或 Window Drawer 可以同时持有租约；任一 owner 关闭不会提前恢复 drawn chrome，最后一个租约释放后恢复。
- 最后一个 presenter 移除后，`DialogOverlayLayer` 解绑 host/TopLevel size 订阅并从 `OverlayLayer` 删除。
- placement target detach 时，Popup 按 Avalonia 生命周期自行关闭；Dialog 不保存 Popup host 引用。

## 8. 资源、性能与 AOT 边界

本设计不引入新的 `VisualLayerManager`、Popup host、动态资源或运行时扫描。Dialog 打开只增加一个轻量 suppression lease；Window 使用整数引用计数，关闭路径常数时间完成。

业务宿主解析使用 Avalonia 公共 `OverlayLayer.GetOverlayLayer()` 与 `TopLevel.GetTopLevel()`。`WindowDrawnDecorationsReflectionExtensions` 只保留 frame/titlebar 几何和 resize-grip 兼容边界，不再反射发现 Dialog/Drawer host，因此不新增 trimming root。

## 9. 兼容性与定制边界

- Public API、默认值、Dialog template part、ControlTheme key 与 Token 不变。
- 稳定行为是：Dialog 内 Popup 可见、可命中、可选择、可 light-dismiss，并与普通页面使用相同 Popup 属性语义。
- 用户不应依赖 `OverlayPopupHost`、`DialogOverlayLayer` 或 Avalonia内部 layer 的具体实例和子节点顺序。
- 自定义 Window decorations theme 可以替换 chrome 视觉，但不能把交互式业务 overlay 重新放入 `WindowDrawnDecorations` 子树。

## 10. 全局 Popup 风险面与覆盖模型

ComboBox 只是 `Popup` 宿主失效的一个触发入口。只要控件最终以 Dialog 内容子树中的 Control 作为 placement target，
`Popup`、`PopupFlyoutBase`、ToolTip 和 ContextMenu 都依赖同一个 owning `TopLevel` 不变量。回归范围因此按 Popup
创建路径划分，而不是按 Issue 中出现的单个控件划分。

### 10.1 Popup 入口清单

| 创建路径 | 直接实现或模板入口 | 主要公共控件与消费者 |
| --- | --- | --- |
| Template 内 `AtomUI.Popup` | `PART_Popup`、AutoComplete popup part | ComboBox、AutoComplete 三种输入形态、Select、Cascader、TreeSelect、Mentions、DatePicker、TimePicker、RangeDatePicker、RangeTimePicker、ColorPicker、GradientColorPicker、Menu/MenuItem、NavMenu、Tour |
| C# 创建 `AtomUI.Popup` | `new Popup` | ToolTip、ContextMenu |
| Avalonia `PopupFlyoutBase` | `Flyout.CreatePopup()` | Flyout、MenuFlyout、TreeViewFlyout、PopupConfirm |
| Flyout 消费控件 | `ShowAt(anchor)` 或 `FlyoutHost` | DropdownButton、SplitButton、AvatarGroup fold、Transfer dropdown、TabControl overflow、DataGrid filter |
| Attached popup 服务 | `ToolTip.Tip`、attached/context flyout | Slider、Form、Steps、Upload、DataGrid、NavMenu，以及应用代码附加到任意 Dialog 内容的 ToolTip/ContextMenu |

该清单描述当前源码中不同的 Popup 构造与接入路径。新增 Popup-bearing 控件时必须归入已有路径并复用其回归，或者在
引入新路径的同一变更中扩充清单与测试矩阵。

### 10.2 分层覆盖策略

回归采用四层证据，避免只证明一个控件，也避免为共享实现复制大量脆弱测试：

1. **共享不变量**：在真实 `TopLevelHost` sibling decorations 拓扑中，Dialog presentation、内容 Control 与 popup
   placement target 必须始终解析到 owning Window。
2. **Popup 原语**：分别覆盖直接 `Popup`、Flyout/MenuFlyout、ToolTip 和 ContextMenu 的创建、交互、light-dismiss 与释放。
3. **控件家族**：10.3 表中列出的公共控件逐项进入 Dialog 集成矩阵；共享基类不能替代不同模板或独立 Popup
   生命周期的派生控件覆盖。
4. **库存守卫**：自动契约测试扫描全部 Desktop runtime 项目中的 AXAML `<atom:Popup`、C# `new Popup`、
   `PopupFlyoutBase`/`Flyout` 派生、直接 `ShowAt`、委托 Flyout 构造和 popup 类型别名，并与测试内的已审计 allowlist
   比较；出现新入口时测试失败，维护者必须将其归入已有路径或扩充矩阵。

库存守卫同时约束表面所有权：`Popup.SurfaceBackground` 的原语默认值为 `null`，17 个 AXAML Popup 入口和 Flyout、
ToolTip、ContextMenu 三个共享 C# 构造路径不得重复设置该默认值。新增入口不能只加入 allowlist；只有明确选择 host-owned
surface 时才能显式提供非空 Brush，否则由 content Presenter 拥有表面。

当前库存基线为 17 个 AXAML Popup 入口、2 个 C# Popup 构造入口、6 个 Flyout 家族定义、4 个直接 `ShowAt`
入口、9 个委托 Flyout 构造消费者和 4 个 popup 类型别名文件。数量不是 Public API，但任何变化都必须经过重新分类和
回归覆盖，不能只更新 allowlist 让守卫恢复通过。

仅复用已经覆盖的 Flyout/ToolTip 原语、没有改变 anchor 或 popup owner 的消费控件，可以通过委托关系审计和既有控件
测试证明；自行创建 Popup、转移 placement target 或维护独立 open/close 状态的控件必须加入直接 Dialog 集成测试。

### 10.3 控件家族矩阵

| 家族 | 必须直接覆盖的控件或原语 | 关键行为 |
| --- | --- | --- |
| Popup 原语 | `Popup`、Flyout、MenuFlyout、ToolTip、ContextMenu | host 建立、命中、关闭与释放 |
| 选择器 | ComboBox、Select、Cascader、TreeSelect | 打开、选择、值回写、light-dismiss |
| 自动建议 | AutoComplete、AutoCompleteSearchEdit、AutoCompleteTextArea、Mentions | 文本 anchor、候选 popup、选择或关闭 |
| 日期时间 | DatePicker、TimePicker、RangeDatePicker、RangeTimePicker | shared InfoPicker 与 range template 两条路径、确认或关闭 |
| 颜色 | ColorPicker、GradientColorPicker | optional package template、选择或关闭 |
| 菜单 | Menu/MenuItem、NavMenu | 顶层与级联 popup、命中和关闭 |
| Flyout 消费 | DropdownButton、SplitButton、PopupConfirm、AvatarGroup fold、Transfer dropdown、TabControl overflow、DataGrid filter | `ShowAt` anchor 归属、操作和关闭 |
| 特殊 popup | Tour | 独立 placement 或动态创建路径 |

### 10.4 每个直接场景的断言契约

- 打开前 placement target 已附着，并且 `TopLevel.GetTopLevel(target)` 是 owning Window。
- 打开后存在实际 popup host；不能只断言 `IsOpen` 或 `IsDropDownOpen` 请求状态。
- overlay popup host 位于 Dialog presentation 之上，Popup item 或主操作区域可命中。
- 选择、确认或菜单命令能更新控件的公开状态；外部点击可 light-dismiss 且不关闭 Dialog。
- Popup 关闭、内容 detach、Dialog close/dispose 后不残留 host、订阅或错误的 open 状态。
- Direct Popup、Flyout/MenuFlyout、ToolTip、ContextMenu 与所有专用控件的 owning Popup 默认使用
  `SurfaceBackground=null`；专用控件继续由既有 Presenter / `PopupFrame` 绘制表面。
- 显式非空 `SurfaceBackground` 只启用 Popup frame fill，不改变其他层级、输入和几何契约。
- surface ownership 不改变 host/Child bounds、corner radius、shadow thickness、Padding、placement 或 z-order。
- `ShouldUseOverlayPopup=false` 的 native popup 路径仍必须满足同一 TopLevel 所有权；Headless 无法证明的原生窗口行为在
  对应桌面平台做实机验证，不通过强制 overlay 替代原有契约。

### 10.5 失败处理边界

- 如果共享不变量失败，在 Dialog/presentation owner 修复，禁止给具体控件增加 Dialog 判断。
- 如果只有一个 Popup 原语失败，在该原语的 host、anchor 或生命周期所有者修复，并回归所有消费控件。
- 如果只有一个控件家族失败，先证明其模板、placement target 或 open/close 状态与已通过路径的差异，再在家族共享基类
  或模板源头修复。
- 禁止通过延迟打开、重试、强制 `ShouldUseOverlayLayer`、替换 placement target、复制 Popup host 或新增嵌套
  `VisualLayerManager` 规避失败。
- 全局矩阵全部通过时，不为制造“修复量”逐个修改控件；测试证据与设计约束就是共享根因已覆盖的证明。

### 10.6 自动化回归入口

自动化入口按职责拆分：`DialogPopupPrimitiveLayeringTests` 覆盖五类原语，`DialogPopupControlFamilyTests` 覆盖 Desktop
Controls 的直接及委托消费家族，`DataGridFilterDialogPopupTests` 在可选 DataGrid 包内覆盖真实过滤菜单，
`PopupEntryInventoryTests` 守卫六类源码入口。测试必须通过公开 open state 或真实 pointer/keyboard 路径打开控件，
不得用反射或 internal open property 代替用户交互；关闭后还必须确认 owning Window 不残留 `OverlayPopupHost`。

## 11. 验证要求

- TopLevel 所有权：使用与 Window 同级的真实 `TopLevelHost` decorations 拓扑，断言任意 Dialog 内容仍解析到 owning Window。
- Popup 原语：直接 Popup、Flyout/MenuFlyout、ToolTip 和 ContextMenu 都能建立 host、交互、light-dismiss 并释放。
- 控件家族：执行 10.3 的矩阵；独立模板或独立 Popup 生命周期不能只由 ComboBox 代表。
- 输入语义：点击 Popup 外部可 light-dismiss，且 Dialog 保持打开；级联菜单按其自身关闭协议收敛。
- 生命周期：最后一个 presenter 关闭后释放 Dialog layer；多个 modal/Drawer 重叠时最后一个 lease 才恢复 drawn chrome。
- 库存守卫：自动 allowlist 契约测试覆盖直接入口、委托构造和类型别名；新增入口时测试失败并要求补充归类和回归。
- 表面所有权：Direct Popup 和专用 Popup 家族默认继承 `SurfaceBackground=null`，host frame 保持透明且既有 Presenter
  的背景、圆角、Padding、阴影和位置不变；显式非空 Brush 的 opt-in 路径单独验证。
- 平台实机验收：分别记录 Windows CSD、macOS 原生 chrome、Linux X11 与 Linux Wayland 下代表性 Popup 家族的可见、可点、可关闭、focus 和进程稳定性证据；不得由共享代码路径推断未执行平台已通过。

当前实机证据状态：

| 平台 | 状态 | 已验证范围 |
| --- | --- | --- |
| Windows | 已测试 | CSD Window 下最终 owning TopLevel/Overlay/Popup 分层与 Dialog 内容 Popup 基本交互。 |
| macOS | 已测试 | 原生 chrome Window 下最终 owning TopLevel/Overlay/Popup 分层与 Dialog 内容 Popup 基本交互。 |
| Linux Wayland (GNOME) | 已测试 | Ubuntu 26.04、GNOME Shell 50.1 下完成 Dialog 内容 Popup 真实窗口人工回归；不代表 Drawer 已在 Linux 验证。 |
| Linux X11 | 未测试 | 尚无实机证据；不能标记为通过。 |
