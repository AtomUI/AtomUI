# TabStrip 桌面版实现原理

本文档描述 TabStrip 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [TabStrip 桌面版架构设计](overview.md)，共用 overflow 架构见 [TabControl / TabStrip 溢出弹层设计](../tab-control/overflow-popup-design.md)，公开 Semantic Part 契约见 [TabStrip Semantic Part 契约](semantic-part.md)，变化记录见 [TabStrip Changelog](changelog.md)。TabStrip 没有独立 Token 文档；涉及主题变量时应回到 overview 的视觉与主题模型。

Popup 接入边界：`BaseTabStrip` 实现 internal `ITabOverflowOwner`，统一 `TabScrollViewer` 负责 overflow 几何、静态 Popup shell、快照与缓存。二者直接协作，不建立 relay binding。普通外点、Escape、失焦和业务关闭在 pinned 状态下可被拦截；detach、窗口销毁、跨 TopLevel、模板替换和模板重建必须走生命周期关闭并释放 Popup host、缓存内容和订阅。完整 pinned 状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

本文档覆盖 TabStrip 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/BaseTabStrip.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/CardTabStrip.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/CardTabStrip.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/TabStrip.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/TabStrip.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/TabStripItem.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/TabStripItem.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabScrollViewer.cs`：四控件共用的 internal sealed 滚动、溢出、Popup 与会话 owner。
- `src/AtomUI.Desktop.Controls/TabControl/TabOverflowPopupContext.cs`：public context、不可变 item projection 与内部 weak action bridge。
- `src/AtomUI.Desktop.Controls/TabControl/TabOverflowMenu.cs`：默认菜单及其 internal item container；不承载 owner-specific 分支。
- `src/AtomUI.Desktop.Controls/TabControl/Themes/TabScrollViewerTheme.axaml`：三个 indicator、更多按钮、滚动 presenter 与静态 `PART_OverflowPopup` shell。
- `src/AtomUI.Desktop.Controls/TabControl/Themes/TabOverflowMenuTheme.axaml`：默认 menu-like 内容、selected/disabled/closable 视觉。

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。
- Tab 拖动排序属于 `BaseTabStrip` 的集合与选择协作路径；实现应落在 `BaseTabStrip`、`TabStripItem`、滚动视口和内部拖动协作对象之间，不能把排序状态散落到 Gallery、theme 或业务数据对象中。
- overflow 属于 `TabScrollViewer` 打开会话的不可变 projection；public context 只提供经 owner 验证的 action，`BaseTabStrip` 仍是选择、关闭事件和集合变更的唯一 owner。
- 垂直页签图标对齐属于 `BaseTabStrip` 的 owner 级布局状态；`Left` / `Right` placement 下由 owner 统一判断同组是否存在图标，再把内部保留图标槽状态投射到 `TabStripItem`，不能通过 Gallery 手工补空图标或新增 public API。
- 默认 Line TabStrip 的 `Left` / `Right` placement 应保持紧凑的垂直节奏；相邻间距和 item 自身垂直 padding 都应按 Line 紧凑模型处理。Card TabStrip 使用独立 `CardGutter` 和 Card padding 视觉节奏，本规则不得改变 Card 外观。

## 3. 核心类职责

- `BaseTabStrip`：public owner，维护 public surface、页签选择、逻辑集合、统一关闭流程、拖动排序提交与 overflow projection。
- `CardTabStrip`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TabStrip`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TabStripItem`：集合项、节点或容器类型，承载单项选择、关闭、拖动源和插入目标状态。
- `ITabOverflowOwner`：internal owner bridge，提供容器 projection、逻辑 item 解析、选择/关闭提交和打开态失效通知。
- `TabScrollViewer`：internal sealed session owner，收集 overflow、控制 Popup、缓存 template root 并执行关闭/teardown。
- `TabOverflowPopupContext`：public weak action facade，关闭后清空 Items、SelectedItem 与 action target。
- `TabOverflowItem`：public immutable projection，不持有事件、命令或 owner 操作入口。
- `TabOverflowMenu` / `TabOverflowMenuItem`：internal 默认呈现，使用类级事件处理，不保存源 Tab 容器。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。
- 拖动排序临时状态只属于控件实例当前交互会话；拖动 item、原 index、目标 index、临时 transform、临时绘制层级和 pointer capture 必须在提交、取消、capture lost、template reapply 或 detach 时统一释放。

## 4. 状态与数据流

TabStrip 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`CloseIcon`、`HeaderEndEdgePadding`、`HeaderEndExtraContent`、`HeaderEndExtraContentTemplate`、`HeaderStartEdgePadding`、`HeaderStartExtraContent`、`HeaderStartExtraContentTemplate`、`Icon`。
- 选择与集合：`IsTabReorderEnabled`、`TabActivationTrigger`、`SelectedIndex`、`SelectedItem`、`ItemsSource`。
- 交互与状态：`IsAutoHideCloseButton`、`IsClosable`、`IsMotionEnabled`、`IsShowAddTabButton`、`IsTabAutoHideCloseButton`、`IsTabClosable`。
- 视觉与布局：`SizeType`、`TabAlignmentCenter`、`TabStripPlacement`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。
- Pointer 选择状态流必须按 `TabActivationTrigger` 收敛。`PointerReleased` 是默认值，按下阶段只记录候选 Tab、pointer 和起点，释放时确认仍是同一个 Tab 且未进入拖动排序后再提交选择；`PointerPressed` 模式在按下阶段直接通过统一选择入口提交选择。
- 候选激活状态属于一次 pointer 会话，必须在 pointer released、capture lost、template reapply、detach、控件禁用或进入 reorder 时释放；不得保存在 item container、theme 或业务数据对象中。
- 拖动排序状态流必须按 `IsTabReorderEnabled` -> pointer threshold -> Chrome-like live reorder preview -> `TabReordering` -> 逻辑集合 move -> selection/overflow recompute -> `TabReordered` 收敛。
- 拖动过程中只能更新被拖 Tab 和兄弟 Tab 的临时 `RenderTransform`、候选目标 index 与自动滚动请求；被拖 Tab 必须被限制在当前 Tab 轨道主轴内移动，释放前不得实时移动 `ItemsSource`、`Items` 或 visual children，避免集合通知、选择状态和 container recycle 多次抖动。
- 排序提交后选中状态按逻辑 item 重新计算，选中指示条、close button 可见性和 overflow 菜单都从同一个集合顺序派生。
- overflow 打开时从对应 `TabStripItem` 投影逻辑 `Item`、`Content` / `ContentTemplate` 与 effective `IsEnabled` / `IsSelected` / `IsClosable`；列表发布后不可原位修改。
- context action 先验证 item 属于当前会话，再经 weak action target 回到 `BaseTabStrip`。ScrollViewer、默认菜单和自定义模板不得直接调用 `Items.Remove`。
- selection 变化发布新的不可变 projection；外部 collection add/remove/move/replace/reset 关闭当前 Popup，使下一次打开基于新布局重新计算。
- `OverflowPopupTemplate` 改变时必须关闭当前会话、释放旧 template root 和 context，并在下一次打开使用新模板。
- `BaseTabStrip.CloseTab` 通过统一的逻辑 `IList` 解析支持可写 `ItemsSource` 和 `Items`；只读或固定大小数据源返回 false，不触发集合删除或 `Closed`。
- `IsTabClosable` 变化时，owner 必须把新的模板级默认值同步到已生成且未被单项覆盖的 `TabStripItem`；下一次 overflow 构建再读取容器最终 `IsClosable`，不读取过期的 owner 值。
- 垂直图标槽状态必须从 `TabStripPlacement`、同组 item 的 `HasIcon` 和容器生成状态单向推导：`Top` / `Bottom` 保持紧凑布局，不默认保留图标槽；`Left` / `Right` 中只要同一 owner 下任一 Tab 有图标，全部 `TabStripItem` 都保留同宽图标槽，未配置图标的 item 渲染空槽而不是伪造图标。
- 图标槽保留状态是内部模板状态，不属于 public API、业务数据或 Token。它应随 item icon 变化、placement 变化、ItemsSource reset/replace/clear、container prepare/clear 和 template reapply 重新计算。
- `TabStripPlacement` 变化是纯布局变化，必须保留当前 `SelectedItem` / `SelectedIndex` 语义，不得为了更新方向重建 item containers；否则直接作为 `TabStripItem` 的容器会把旧 `IsSelected` 容器状态反向写回 owner selection。
- 默认 Line TabStrip 垂直 spacing 和垂直 item padding 只由默认 `TabStrip` theme 消费，Card theme 继续使用 `CardGutter` 与 `VerticalItemPadding`；不要通过全局修改 Card token 或 Card theme 来修正 Line 布局。

## 5. 组合结构模型

### 控件角色图

```text
BaseTabStrip
  -> TabStrip / CardTabStrip ControlTheme
     -> TabScrollViewer#PART_TabsContainer or #PART_CardTabStripScrollViewer
        -> IconButton#PART_ScrollMenuIndicator
        -> Panel#ScrollContentViewport
           -> TabScrollContentPresenter#ScrollViewContent
           -> Canvas (viewport-sized, clipped, input-transparent)
              -> TabOverflowEdgeIndicator#PART_ScrollStartEdgeIndicator
              -> TabOverflowEdgeIndicator#PART_ScrollEndEdgeIndicator
        -> Popup#PART_OverflowPopup
           -> cached custom template root
           -> or TabOverflowMenu
              -> TabOverflowMenuItem × N
```

### 协作节点

| 节点 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- |
| `BaseTabStrip` | C# | control instance | 选择、关闭、`OverflowPopupTemplate` | public | 用户只依赖 public API |
| `TabScrollViewer` | ControlTheme + C# | current template | overflow 的全部可观察行为 | internal-observable | 只用于维护，不指导用户引用 |
| `ScrollContentViewport` + edge indicators | `TabScrollViewerTheme` | current template | 四向滚动边界阴影 | internal-observable | 必须覆盖真实 presenter，不能用外层 magic margin 猜测边界 |
| `PART_OverflowPopup` | `TabScrollViewerTheme` | `TabScrollViewer` | placement、light-dismiss、template content | template-stable | 主题变更需同步实现和测试 |
| `TabOverflowPopupContext` | C# | `TabScrollViewer` session | 自定义模板 data item | public | action 只对当前会话有效 |
| `TabOverflowMenu` | default theme | cached template root | `OverflowPopupTemplate=null` 的默认体验 | internal-observable | 不作为应用 theme key |

## 6. 生命周期与模板接入

Line `TabStrip` 的选中指示条由 `TabStrip.SetupSelectedIndicator` 根据当前 `TabStripItem.Bounds`、items presenter
偏移及 placement 更新。构造时创建 `StateMotionSession`，attach、detach 与模板变更复位；选中容器与 presenter 已排列且
尺寸有效时直接提交完整 transform，再在已加载的当前生命周期中释放过渡覆盖。选中变化、Bounds 订阅和排列
仍可更新指示条；相同矩阵的 base target 不重复写入，避免动画中布局重新启动过渡。detach 释放选中项 Bounds
订阅。item 容器也持有自身会话，避免初始选中前景及背景过渡。拖动校准使用独立的可释放作用域，异常路径也恢复最新过渡集合。共享规则由 [状态动效初始化设计](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)
定义，动态验证范围见 [状态动效初始化验证](../../../../architecture/systems/control-infrastructure/state-motion-initialization-verification.md)。

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。
- `PointerReleased` 激活候选项不依赖 template part；模板重套用、detach 或 container recycle 时必须清理候选 Tab 和 pointer，避免旧容器在下一次释放事件中被错误激活。
- 拖动排序获得 pointer capture、应用临时 transform、订阅 pointer move/release 或启动边缘自动滚动时，必须在 pointer released、capture lost、cancel、collection reset、template reapply、detach 中走同一释放路径。
- 模板重套用后不得复用旧 `TabStripItem`、旧 scroll viewer、旧 transform、旧 z-index 或旧拖动会话状态；新的模板只从 public state 和当前集合重新生成可观察状态。
- 图标槽对齐状态必须由 owner 在模板接入和容器生命周期中统一同步。`TabStripItem` 只消费内部保留图标槽状态；容器回收或重新准备时必须清理旧 item 的图标槽状态，避免上一组带图标页签影响下一组无图标页签。
- 普通 Popup 关闭释放 collection/selection subscription、item-to-source map、context Items/SelectedItem 与 weak action target；可以保留无数据的模板视觉树。
- template reapply、detach 或 `OverflowPopupTemplate` 替换在普通关闭之后继续释放 Popup child、cached template root、context、template 和 owner part 引用。
- context 与默认菜单不建立 per-item event/command subscription；关闭流程必须幂等，Popup `Closed`、collection invalidation 与 detach 只能解除一次资源。
- Popup `Opened` 不承担默认焦点迁移，也不得注册用于聚焦第一项或选中项的 handler。pointer 打开后保留 `PART_ScrollMenuIndicator` 焦点；自定义搜索根节点保持不可聚焦，输入和菜单项只响应用户显式的焦点导航。
- 两个 edge indicator 是静态 AXAML `TabOverflowEdgeIndicator`，位于与 `ScrollContentViewport` 同尺寸的 Canvas 内。Canvas 不参与命中测试，并按 viewport 裁剪输出；indicator 的实体矩形在 viewport 外侧，朝内的一面贴合首尾边界，只有普通外阴影进入内容区。位置与横向跨度使用到 Canvas `Bounds` 的 compiled binding，调整尺寸与 placement 时随模板布局更新；只在滚动状态变化时切换可见性。
- 默认与 Gallery 搜索 overflow 列表必须隐藏垂直滚动条但保留滚轮、触控板和程序化滚动能力；列表内容左右 inset 必须相等，不能让 scrollbar gutter 或 item 外边距单独侵占右侧宽度。
- Popup 阴影容器必须逐层解包宿主和 overflow 内容产生的嵌套 `ContentPresenter`，以最终 surface 的
  `CornerRadius` 生成 shadow mask；presenter child 变化或 detach 时必须解除整条实例订阅链和旧 surface relay binding。
- `SetupIndicatorsVisibility` 改变更多按钮可见性后必须触发有效布局，使 DockPanel 在主轴上为激活器保留尺寸；窄列页面回归需同时断言 owner、viewer 与激活器的实际 bounds，而不能仅断言 extent 大于 viewport。

稳定 template part 接入点：

- `PART_ScrollMenuIndicator`、`PART_ScrollStartEdgeIndicator`、`PART_ScrollEndEdgeIndicator` 与 `PART_OverflowPopup` 位于统一 `TabScrollViewerTheme`；TabStrip owner theme 负责提供 scroll viewer 实例。

Semantic marker 接入点：

- `TabStrip.item` 与 `CardTabStrip.item` 为运行时创建的语义标记（`RuntimeCreated = true`），路由
  `> .semantic-item`，即 marker 挂在 owner 的直接逻辑子节点（`TabStripItem` container）上：owner 在
  `CreateContainerForItemOverride` 中把 `semantic-item` 应用到新建容器，并在 `PrepareContainerForItemOverride`
  中对复用容器或直接提供的 `TabStripItem` 实例重新确认 marker；不依赖模板作用域标记。overflow 菜单项
  （`TabOverflowMenuItem`）不携带 marker。
- `CardTabStrip.add` 为静态标记：`CardTabStripTheme.axaml` 在 `PART_AddTabButton`（`IconButton`）上声明
  `Classes.semantic-add="True"`；`IsShowAddTabButton` 只控制可见性，marker 不增删。Line 风格 `TabStrip`
  没有加号按钮，descriptor 不含 `add`。
- `TabStripItem.icon` / `TabStripItem.label` / `TabStripItem.close` 为静态标记：
  `BaseTabStripItemTheme.axaml`（Line）与 `CardTabStripItemTheme.axaml`（Card）两套 item 模板分别在
  `ItemIconPresenter`、标题 `ContentPresenter` 与 `PART_ItemCloseButton` 上声明
  `Classes.semantic-icon="True"` / `Classes.semantic-label="True"` / `Classes.semantic-close="True"`，
  每个模板各恰好一个 marker；`HasIcon` / `IsClosable` / `IsAutoHideCloseButton` 只控制可见性或透明度，
  marker 不随状态增删。
- 独立页签条不承载内容页，`TabStrip` / `CardTabStrip` 不公开 `content` Part；选中指示墨条
  `PART_SelectedItemIndicator`、`HeaderStartExtraContent` / `HeaderEndExtraContent` 与滚动容器是内部协作
  节点，不声明语义 marker。

## 7. 交互与事件处理

TabStrip 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 值提交或命令触发必须保持继承控件的事件顺序。
- Pointer 触发选择由 `TabActivationTrigger` 决定，`PointerReleased` 默认要求 press/release 命中同一个 Tab；实现应在 owner 控件统一判断，不在 `TabStripItem` 中直接修改 `SelectedIndex`。
- 键盘导航、focus directional navigation、access key 和程序化选择不受 `TabActivationTrigger` 影响。
- 默认与自定义 overflow 内容只通过 context `TryActivate`、`TryClose` 与 `Dismiss` 交互。关闭按钮只在 projection `IsClosable` 时显示；`TryClose` 仍由 `BaseTabStrip.CloseTab` 统一执行 `Closing` / cancel / 集合 / selection / `Closed`。
- 搜索模板的方向键把焦点交给菜单，Enter 激活，Escape 请求 dismiss；disabled item 必须跳过且不能触发 action。
- 拖动排序只响应可拖动 Tab item 的主按钮拖动；关闭按钮、添加按钮、`HeaderStartExtraContent`、`HeaderEndExtraContent`、overflow 菜单项和外部内容区域不得成为 reorder target。
- `TabStripPlacement=Top/Bottom` 时排序主轴为 X 轴，`TabStripPlacement=Left/Right` 时排序主轴为 Y 轴。被拖 Tab 只能沿主轴移动：Top/Bottom 的 Y 位移为 0，Left/Right 的 X 位移为 0；兄弟 Tab 让位和目标 index 也只能由主轴计算。
- 真实 Tab 视口靠近边缘时允许自动滚动以暴露更多排序目标；overflow 菜单只用于导航和选择，不承载拖动排序。

稳定事件路径包括 `AddTabRequest`、`Closed`、`Closing`、`TabReordering`、`TabReordered`。事件参数和触发时机属于兼容边界。

## 8. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- 内容、命令和视觉状态在模板节点之间的同步。
- overflow 构建：沿 placement 主轴用 container bounds - scroll offset 与 viewport 比较，收集部分或完全不可见的 Tab；按逻辑顺序发布 immutable projection，并建立只对当前打开会话有效的内部 action map。
- overflow 首开与重开：首次打开创建 context 和 template root；重开复用 root，只刷新 context 和打开态 subscription。没有溢出 item 时不打开 Popup。
- 打开事务在发布 context 通知前订阅 owner 变化；通知和自定义模板构建返回后都复核当前会话身份。应用模板只构建一次，已构建根通过保持 DataContext 继承的 presenter 接入 Popup；同步 dismiss、集合变化、detach 或模板替换会中止本次打开并释放废弃内容。关闭事务包含完整资源清理，期间拒绝回调重开。
- overflow 关闭与 teardown：普通关闭清空 snapshot/action/subscription；re-template、detach 或模板替换进一步释放 Popup child、cached root、context、template 与 owner。
- edge shadow 映射：`Top` / `Bottom` 沿 X 轴使用 left/right 普通外阴影；`Left` / `Right` 沿 Y 轴使用 top/bottom 普通外阴影。开始端阴影向右、结束端阴影向左，或在纵向分别向下、向上扩展，始终进入真实 viewport。开始端仅在 offset 大于 0 时显示，结束端仅在 offset 小于 scroll maximum 时显示；阴影载体主轴尺寸等于 `MenuEdgeThickness`，位于真实 `ScrollViewContent` viewport 外侧；它的内侧边与 viewport 边界重合，不能把载体本身放在 viewport 内部。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。
- 激活触发：`PointerPressed` 模式直接在 press 阶段触发选择；`PointerReleased` 模式在 press 阶段记录候选项，release 阶段校验同一 pointer、同一 Tab、指针仍在 Tab bounds 内且未进入 reorder 后触发选择。
- 取消激活：press 后移动到其他 Tab、移出当前 Tab、capture lost、控件 detach、模板重套用、进入拖动排序或源 item 被删除时清理候选激活状态，不提交选择。
- 拖动开始：记录逻辑 item、原 index、pointer 起点和当前 `TabStripPlacement` 主轴；超过平台拖动阈值才进入排序态。
- 目标计算：只遍历真实可见的 `TabStripItem` 容器，使用被拖 Tab 的前进边缘判断是否跨过被覆盖兄弟 Tab 主轴中线；向后拖动使用 trailing edge，向前拖动使用 leading edge，header extra、add button、close button、overflow menu 和非 Tab 容器不进入候选集合。
- Chrome 式预览：拖动进入 active reorder 后，源 Tab 使用主轴 pointer 偏移量作为临时 transform 并提高绘制层级，非主轴位移保持为 0；兄弟 Tab 是否让位必须由同一个目标 index 阈值决定，不能按任意重叠距离提前移动。源 Tab 前进边缘跨过被覆盖兄弟 Tab 主轴中线后，目标区间内的兄弟 Tab 必须按相邻真实 layout slot 的主轴起点差值平移到前后相邻槽位，不能只按源 Tab 尺寸位移，因为 Line/Card 的 gutter 和可变宽度也属于 layout slot；位移使用短时过渡避免位置瞬移；目标 index 回退时，已让位兄弟 Tab 应沿同一 preview transform 动画归位，不能在拖动会话中直接清理原始 transform；半宽或半高阈值前兄弟 Tab 保持原位，拖动中不绘制插入线，不提交集合 move。
- 选中指示条：选中指示条的位置以选中容器 layout bounds 为基础，并在 active reorder 期间叠加选中容器当前预览 transform 的主轴位移；选中源 Tab 和被让位的选中兄弟 Tab 都必须跟随视觉预览位置。当前被拖 Tab 同时也是选中 Tab 时，拖动会话内必须临时禁用 `SelectedIndicatorRenderTransform` transition；释放提交后要把 selection/bounds/scroll 触发的中间 indicator 刷新延迟到最终 layout 完成，再在同一清理路径恢复 transition，避免指示条先跳到旧布局再回到正确位置。
- 拖动源视觉：源 Tab 必须在拖动期间使用不透明背景，避免覆盖兄弟 Tab 时文字、图标或边框叠穿。Line 模式使用当前激活面背景，Card 模式使用卡片激活面背景；hover、pressed、selected 与 drag state 的颜色过渡必须继续走主题 transition。
- 集合提交：`ItemsSource` 可写且实现 `IList` 时移动 source list；未设置 `ItemsSource` 时移动 `Items`；只读、固定大小或不可写 source 不提交 reorder，并清理临时视觉状态。
- 回调边界：关闭在 `Closing` 及选择变更回调后重新验证原始 source、当前容器与逻辑 item，不能沿用旧 index；目标已移除或 source 已替换时返回 false，不再删除其他项或触发本次 `Closed`。`TabReordering` 回调改变集合或 source 时取消本次 move，保留调用方的修改；未通知的可写 `IList` 通过前后项快照校验。
- 事件顺序：释放时先触发可取消的 `TabReordering`；未取消且集合 move 成功后重新计算选择，再触发 `TabReordered`。
- 异常边界：拖动期间集合 reset、item 被删除、控件禁用或模板失效时取消当前排序，不吞异常、不延迟强刷，也不把旧 index 当作可靠状态。
- overflow action 边界：旧会话、disabled、不可关闭或源容器失效时返回 false；成功 close 必须按 owner 事件顺序完成集合和选择状态更新，collection invalidation 随后关闭过期快照。
- 垂直图标槽计算：owner 只扫描当前有效 `TabStripItem` 容器或对应逻辑 item 的图标状态，得到同组 `HasAnyIconInVerticalPlacement` 语义后下发内部状态；主题结构应统一为稳定的 `IconSlot` + `ContentPresenter` + `CloseButton` 顺序。图标槽宽度沿用 TabControl 家族现有 `IconSize` / `IconSizeSM` 和 `ItemIconMargin` 语义，不新增 Token；无图标 item 的 `IconSlot` 保持占位但不显示内容。
- 主轴布局：卡片容器测量时先为添加按钮保留空间，再测量页签视口；主轴期望尺寸为两者之和，极窄排列不产生负尺寸。Line 与 Card 均沿 placement 主轴居中。真实水平滚轮输入按 `FlowDirection` 转换方向，垂直滚轮的横向回退保持逻辑顺序；边界继续遵守 scroll chaining 设置。
- Placement 切换流程：owner 更新 pseudo-class、header padding、现有 container 的 `TabStripPlacement` 和内部布局状态即可；不得调用 container refresh 作为布局刷新手段。

`CardTabStripTheme`声明添加按钮的尺寸和placement对齐。宽度跟随`CardSize`，Top/Bottom高度跟随同模板`ItemsPresenter.Bounds.Height`，Left/Right保持`CardSize`正方形。水平编译绑定放在模板内Style Setter中，由Setter保留编译路径的NameScope；专用Style覆盖退出后恢复当前关系，整个绑定随模板释放，不新增代码订阅。owner不再通过Arrange或CardSize回调回写按钮尺寸。

`TabStrip`和`CardTabStrip`具有独立Global owner且没有自己的Own定义；主题显式复用`TabControl` Own。34个V2默认及`CardPadding`到有效`CardSize`的依赖见[家族Token合同](../tab-control/token.md)。生成item的既有LocalValue圆角/边框绑定保留原优先级，并在容器释放时解除。

### 8.1 Semantic Part 运行时 marker 同步

- `semantic-item` marker 由 owner 在两个入口同步：`CreateContainerForItemOverride` 把 marker 应用到新建
  `TabStripItem` 容器，`PrepareContainerForItemOverride` 对复用容器或直接加入 `Items` 的 `TabStripItem`
  实例重新确认 marker；两条路径共用同一个 class 常量（`TabStripSemanticParts.ItemClass` /
  `CardTabStripSemanticParts.ItemClass`，均对应 `semantic-item`）。
- marker 同步与容器可见性、选中、禁用、拖动和溢出状态解耦：container recycle、模板重套用、拖动排序提交和
  `TabStripPlacement` 切换都不增删 marker。
- overflow 菜单项（`TabStripOverflowMenuItem`）是独立呈现节点，不携带 marker；页签溢出进菜单只是切换呈现宿主，
  不迁移 `semantic-item` marker。
- `SizeType` 通过 `tabStripItem[!TabStripItem.SizeTypeProperty] = this[!SizeTypeProperty]` 从 owner 单向下发到
  容器，Semantic Style 不创建新的尺寸档。

### 8.2 尺寸与状态基线矩阵

| SizeType | 默认值来源 | FontSize（`BaseTabStripItemTheme` `^[SizeType=...]`） | Line Padding（Top/Bottom） | Card Padding | 图标尺寸 |
| --- | --- | --- | --- | --- | --- |
| `Large` | 显式设置 | `TitleFontSizeLG` | `HorizontalItemPaddingLG` | `CardPaddingLG` | SharedToken `IconSize` |
| `Middle` | `SizeTypeControlProperty.SizeTypeProperty` 默认值 | `TitleFontSize` | `HorizontalItemPadding` | `CardPadding` | SharedToken `IconSize` |
| `Small` | 显式设置 | `TitleFontSizeSM` | `HorizontalItemPaddingSM` | `CardPaddingSM` | SharedToken `IconSizeSM` |

- `BaseTabStrip.SizeTypeProperty` 与 `TabStripItem.SizeTypeProperty` 都通过 `SizeTypeControlProperty.SizeTypeProperty.AddOwner`
  注册，默认 `Middle`；owner 在 prepare 阶段用 binding 单向下发到容器。
- 垂直 placement（`Left` / `Right`）不按上表取值：Line item 三档全部固定 `Padding = 8,4`
  （`TabStripItemTheme.axaml` 硬编码的紧凑垂直节奏），Card item 三档全部使用 `VerticalItemPadding`；
  `CardGutter` 属于 Card 视觉节奏，不受 Line 紧凑规则影响。
- item 无固定 `Height`，自然高度由 `TitleFontSize` + item padding + 图标槽测量决定；Semantic Setter 修改
  `FontSize` / `Padding` / `Margin` 会直接改变自然高度，必须按三档 `SizeType` 与四向 `TabStripPlacement`
  验证，不能通过固定 `Height` 或像素偏移掩盖测量不一致。
- `add` 按钮图标为 SharedToken `IconSize`，按钮 Margin 由 `AddTabButtonMarginHorizontal` /
  `AddTabButtonMarginVertical` 提供。
- 基线矩阵是稳定契约：改变任一格的 Token、selector 或默认值必须同步 `TabStripItemTheme.axaml`、
  `CardTabStripItemTheme.axaml`、关联控件 Token 文档与 `TabStripSemanticPartTests` 的尺寸基线测试。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 9. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- edge indicator 使用模板内固定 `TabOverflowEdgeIndicator` 和 family Token 化 `BoxShadows`；滚动或测量期间只切换可见性，不创建 `LinearGradientBrush`、gradient stop、binding 或额外视觉树。绘制时创建不可变的 `Rect` / `BoxShadows` 快照，由专用 draw operation 消费。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。
- 拖动 move 帧内只更新轻量 transform、目标 index 和自动滚动请求；主轴约束和目标 index 计算必须是纯几何计算，不得在 pointer move 中反复移动集合、重建 item 容器或重新应用模板。
- 拖动预览 transform、绘制层级、计时器和订阅应按交互会话缓存并在会话结束释放；不得因一次拖动永久保留视觉对象或数据 item。
- 拖动排序不得引入运行时反射、动态类型扫描或 AOT 不友好的事件发现路径。
- 图标槽对齐不得为无图标 Tab 创建额外图标控件、动态占位对象或 C# 运行时模板分支；应复用静态 AXAML 槽位、现有资源绑定和内部布尔状态，避免增加模板实例化和 container recycle 成本。
- 未打开实例只有一个无 Child 的静态 Popup shell，不建立打开态订阅；第一次打开创建一次内容树，重复打开只重建 immutable projection。
- `PART_ScrollMenuIndicator.Click` 与 `PART_OverflowPopup.Closed` 是 template-part 生命周期订阅，可在关闭态存在，但必须在 re-template / detach 时成对解除；“无打开态订阅”只指 collection/selection、per-item、command、dispatcher 与 owner-action 会话订阅。
- 每次打开不得创建 Flyout、`CompositeDisposable`、relay binding、per-item delegate、command 或 dispatcher closure；默认
  item container 关闭时清除旧 item/header/template、DataContext 及标题 presenter 的模板子树。默认菜单拥有仅包含空容器的局部缓存，容量随最近一次非空快照缩小；Context 切换时丢弃缓存，完整 teardown 后随菜单根一起释放。
- 自定义搜索 item 直接把 `Header` / `HeaderTemplate` 交给按钮的原生 content pipeline；模板根圆角与可见 surface
  必须一致。替换嵌套 presenter 的最终 surface 后，旧 surface 必须可以被 GC 回收。
- 重复 open/close 的 allocated bytes/op 与 Gen0 压力必须相对旧基线至少下降 30%；never-open 创建、布局、滚动不得出现超过 5% 的稳定回退。
- context 只弱引用 action target；关闭态 Items 为空且无活动打开会话订阅，完整 teardown 后 cached root、context、template 与 DynamicResource anchor 均不得被旧会话保留。
- Public context、compiled AXAML 和静态 Theme 注册必须保持 AOT 友好，不引入 reflection binding、type scan 或动态注册。

## 10. 维护不变量

维护 TabStrip 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- Semantic Part descriptor、静态 `Classes.semantic-*="True"` marker、运行时 `semantic-item` marker 同步规则与生成的
  `TabStripItemStyle` / `CardTabStripAddStyle` / `CardTabStripItemStyle` / `TabStripItemIconStyle` /
  `TabStripItemLabelStyle` / `TabStripItemCloseStyle` 等 Style 类型。选中指示墨条、header extra 与 overflow
  菜单项不携带语义 marker 属于稳定契约，不能通过主题或代码改动破坏。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- 拖动排序释放时必须修改逻辑集合顺序，拖动中允许用 `RenderTransform` 和临时 `ZIndex` 做实时视觉预览，但不能只调整 `Panel.Children`、`ZIndex` 或 transform 作为最终排序结果。
- 选中项必须跟随同一个逻辑 item，不能跟随旧 index；重排后指示条、overflow 菜单和关闭状态必须从新顺序统一推导。
- overflow 内容不能提供独立于源 Tab 的选择或关闭能力；所有 action 必须验证当前会话并经过 `BaseTabStrip`。
- `Closing` 被取消或 owner 拒绝关闭时，源 Tab、集合和选中状态保持不变；任何外部 collection mutation 都使 projection 失效并关闭。
- 普通关闭后不得保留 snapshot 数据或 owner action target；完整 teardown 后不得保留 Popup child、context、template root 或打开态订阅。
- edge indicator 必须与真实 scroll content viewport 同边对齐；四个 placement 的方向阴影、尺寸和开始/结束可见性必须一致，不能以 DockPanel 外层 margin 或渐变遮罩代替。
- overflow 列表隐藏 scrollbar 时必须继续可滚动，item hover/selected surface 的左右间距必须相等。
- `TabActivationTrigger` 只能改变 pointer 激活提交时机，不能改变键盘选择、access key、关闭后选择、程序化选择或拖动排序后的选中项回放语义。
- `PointerReleased` 候选激活状态必须由控件 owner 持有并按 pointer 会话释放，不能让旧 `TabStripItem` 或旧 pointer 引用跨 template reapply / detach 存活。
- 所有拖动临时状态必须在提交、取消、capture lost、template reapply 和 detach 时释放，不能保留旧容器或旧 adorner。
- 垂直图标槽对齐不能改变 `Top` / `Bottom` 的紧凑布局；不能新增 public API、Token 或 Gallery-only workaround；`TabControl`、`TabStrip`、`CardTabControl` 和 `CardTabStrip` 的同组混合有图标/无图标布局必须使用同一套 owner 推导规则。
- 默认 Line TabStrip 的 `Left` / `Right` spacing / padding 调整不得影响 Card TabStrip、拖动排序阈值、选中指示条定位或 overflow 计算；选中指示条高度必须继续跟随 Line item 的真实 bounds。
- 切换 `TabStripPlacement` 后当前选中项必须继续跟随同一个逻辑 item，不能因 container 重新准备或旧 `IsSelected` 状态回流而改变。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 11. 测试与验证

- `tests/AtomUI.Desktop.Controls.Rendering.Tests/TabOverflowShadowRenderingTests.cs` 使用独立 Skia Headless 合成器验证四类 owner、四向 placement、Light/Dark、开始/中间/末尾 offset、缩窄和无溢出状态。像素必须紧贴实际边界，且不能在页签内部形成分离条纹或覆盖更多按钮。

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- Tab 激活触发变更需覆盖默认 `PointerReleased`、`PointerPressed`、press/release 同 Tab 激活、press 后移出不激活、press A release B 不激活、键盘选择不受影响，以及拖动排序释放不触发额外激活。
- Tab 拖动排序变更需覆盖 Top/Bottom 横向排序、Left/Right 纵向排序、选中 item 跟随、可写 `ItemsSource`、未设置 `ItemsSource`、只读 source 不提交、`TabReordering` 取消、overflow 边缘自动滚动、关闭/添加/extra 区域排除、template reapply 与 detach 释放。
- 共用 `TabOverflowEdgeIndicator` 的 Skia offset 变换补偿；Token 保持逻辑 DIP，绘制操作不持有 owner/window。DPI 根因、升级检查及离屏限制见 [TabControl implementation](../tab-control/implementation.md)。生产 TabStrip Gallery 页另由 `GalleryTabOverflowShadowRenderingTests.cs` 保留双列、外层滚动和延迟内容进行 1×/2× 像素验证。
- overflow 需覆盖 public API/default、default/custom template、四控件与四向 placement、partial visibility、content template、disabled/selected/closable、Items/ItemsSource、stale action 和 external collection invalidation；四个 owner × 四个 placement 还需断言 directional outer shadow 参数、透明绘制载体、viewport 阴影层裁剪边界与真实合成像素、真实 viewport 边界、开始/结束可见性与 `ControlHeight` 尺寸。
- 默认与搜索 overflow 列表需断言 scrollbar 不可见且不保留 gutter、滚动 offset 可变化、item hover/selected surface 左右 inset 相等。
- 搜索体验需覆盖 case-insensitive filter、clear、empty、keyboard、disabled skip、selection 清 query、light-dismiss 保留 query 和 detach 清理。
- 生命周期需覆盖首次惰性创建、重复复用、100 次 open/close、保存旧 context、re-template、template replace、detach/reattach、pinned lifecycle close、Gallery navigation 与 DynamicResource anchor weak-reference 回收。
- 性能需记录 cold/closed、first open、reopen/close、scroll 的耗时、allocated bytes、Gen0 与对象计数；任一稳定回退或 retained object 阻断交付。
- 垂直图标槽对齐变更需覆盖 `Left` / `Right` 下同组混合图标与无图标 Tab 的文本起点一致、全部无图标时不额外占位、`Top` / `Bottom` 保持紧凑、Line/Card 两类主题一致，以及 icon/placement/items 变化和 container recycle 后状态不串组。
- 默认 Line 垂直 spacing / padding 变更需覆盖 `TabStrip` 在 `Left` / `Right` 下的相邻 container 主轴间距和 item 高度，并明确 Card theme 不被本规则修改。
- `TabStripPlacement` 行为变更需覆盖直接 `TabStripItem` 与数据 item 场景，确保切换 `Top` / `Right` / `Bottom` / `Left` 后 `SelectedItem` 不变。
- Semantic Part 契约、marker 或生成 Style 类型变更运行 `tests/AtomUI.Desktop.Controls.Tests/TabControl/TabStripSemanticPartTests.cs` 与 `tests/AtomUI.Desktop.Controls.Tests/TabControl/TabControlSemanticPartTests.cs`。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
