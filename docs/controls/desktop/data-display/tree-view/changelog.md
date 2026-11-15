# TreeView Changelog

本文档记录 TreeView 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-11-14

- Fix
  - 虚拟化面板下 `Shift` + ↑↓ / Home / End 范围多选只包含已实现的行：范围原先在已实现容器的序列上计算，锚点容器被回收后锚点丢失（Shift+End 甚至只剩 1 项）。选择锚点改为保存项，范围在节点数据的先序序列上计算，覆盖未实现的节点。
  - `SelectedItems` 变化（键盘范围、代码增删）作用于未实现的节点后，节点数据不同步，之后实现的行按旧的 `IsSelected` 显示为未选中。节点 `IsSelected` 现在跟随 `SelectedItems`。
  - 范围选择内部集合改按引用比较：`TreeItemNode` 是 record，值相等的不同节点会被当成同一项。
  - 直接添加 `TreeViewItem` 子项的树在虚拟化面板下键盘导航仍停在视口内：面板隐藏它回收的自身容器项，`ContainerFromIndex` 却仍返回它们，导航把它们当作被过滤隐藏而跳过。现在只把有索引（已实现）的容器当作容器，其余按需实现。
- Implementation
  - 新增 `TreeView.SelectionState.cs`：attach 期间订阅 `SelectedItems` 集合变化，把增删同步到 `ITreeItemNode.IsSelected`，按引用维护镜像，Reset / 重新 attach 时求差；detach 时退订。
  - `TreeView.KeyboardNavigation.cs` 新增 `GetRealizedContainer`（`ContainerFromIndex` 且 `IndexFromContainer >= 0`），逐项导航与范围遍历都只把已实现的容器当作容器。
  - `TreeView.KeyboardNavigation.cs` 的选择锚点从容器改为项，新增按节点数据的先序遍历 `EnumerateNavigableTreeItems`，`SelectKeyboardRange` 改用它；容器离开视觉树不再清空锚点。
- Tests
  - `TreeViewKeyboardNavigationTests` 新增直接 `TreeViewItem` 子项在虚拟化面板下 ↑↓ Home End 与 Shift 范围越过视口的用例。
  - `TreeViewKeyboardNavigationTests` 新增 `TreeItemNode` / `BindableTreeItemNode` 各 4 项：Shift+End 覆盖未实现的根与子节点并保持先序、锚点容器回收后扩展范围、收缩范围取消未实现节点的选择、代码增删未实现节点后行的选择显示。
- Docs
  - `keyboard-navigation-design.md` §7.1 增加范围选择算法，§8 性能、§9 兼容性（移除“范围只含已实现部分”，改为过滤只作用于已实现容器）、§10 验证同步；`implementation.md` 源码职责、§7.8 锚点与选择镜像；`overview.md` 选择行为补充 `SelectedItems` 权威与节点镜像。

- Fix
  - 虚拟化面板下 checkbox 级联只覆盖已实现的子节点：勾选有 15 个子节点、视口只实现约 8 个的已展开节点时，`CheckedItems` 只得到已实现部分，父节点显示半选。级联改为沿节点数据（`ITreeItemNode.Children` / `ParentNode`）计算，父级 true / null 按全部子节点得出。
  - 虚拟化面板回收 `BindableTreeItemNode` 容器时 `CheckedItems` 被整体清空：清理容器的 `ItemsSource` 被当成子集合 `Reset`，处理为 `CheckedItems.Clear()`。容器生命周期内的 `ItemsSource` 变化不再清理勾选；真实的子集合 `Reset` 只移除从根不可达的勾选项，替换子集合只移除离开的节点及其子树。
  - 代码向 `CheckedItems` 加入或移除未实现节点时，节点数据与之后实现的容器不再与集合不一致；赋值新的 `CheckedItems` 集合时其中的节点显示为勾选（此前已实现容器全部显示未勾选）。
- Implementation
  - `TreeView.CheckedState.cs` 改为“勾选计划 + 提交”：计划按节点数据生成目标状态，提交依次更新 `CheckedItems`、节点 `IsChecked` 与已实现容器；不再为级联临时展开子树（无节点模型的数据除外）。新增按引用比较的 `CheckedItems` 镜像、`ReconcilePreparedCheckedState`（容器准备时对齐）、`IsApplyingCheckedState`（投射不触发交互处理器）、私有保护的 `RecursiveCheckNodeDataPredicate` / `RecursiveUnCheckNodeDataPredicate`。
  - `TreeViewItem` 新增容器生命周期作用域 `BeginContainerLifecycle`，`TreeView` 重写 `PrepareContainerForItemOverride` 进入该作用域；`ItemsSource` 变化期间忽略 `Items` 变化，生命周期外的替换按数据删除处理。
  - `TransferTreeView` 的 `SelectAll` / `DeselectAll` 按根数据项执行，覆盖未实现的根节点；未实现节点按 `MaskKeys` 判断屏蔽。
  - 路径回放在目标容器已显示勾选时直接执行勾选处理，`CheckedItems` 清空后仍能恢复。
- Tests
  - `TreeViewVirtualizationStateTests` 新增：虚拟化下级联覆盖未实现子节点（16 项）、取消一个子节点后父级半选再恢复、`TreeItemNode` / `BindableTreeItemNode` 回收时 `CheckedItems` 不变（滚离与滚回两处断言）、代码增删未实现节点、`Clear` 作用于未实现节点、子集合 `Reset` 与 `BindableTreeItemNode.Children` 替换只移除离开的节点、严格模式不级联，以及非虚拟化面板结果不变。
- Docs
  - `overview.md` 勾选行为与兼容性不变量改为 `CheckedItems` 权威、节点数据级联、容器回收不是数据删除；`implementation.md` 更新勾选流、§7.3 勾选计划、§7.7 容器生命周期与数据删除的区分、性能边界与维护不变量。

- Fix
  - `ItemsPanel` 为虚拟化面板时，键盘导航可以越过视口：↑↓ / Home / End / ← → 的目标索引没有容器时，先布局一次，再对所属 `ItemsControl` 调用 `ScrollIntoView(index)` 实现容器。此前方向键停在视口内最后一个已实现节点，Home / End 落到已实现范围的首尾。
  - 虚拟化面板回收活动项容器后，活动项不再丢失：容器离开视觉树时只释放容器引用和指示状态，保留 active 路径（每层索引和项）；该项的下一个容器完成准备时重新绑定，按键时仍未绑定则沿路径重新实现。
  - 虚拟化面板回收并重新实现容器后，`TreeItemNode` 与自定义 `ITreeItemNode` 节点不再丢失状态：此前只有 `BindableTreeItemNode` 回写容器状态，普通节点重新准备时用过期数据覆盖，导致展开收起、选中底色消失（`SelectedItem` 仍在）；勾选模式下回收容器重用时以过期 `IsChecked` 触发取消勾选级联，Debug 构建断言终止。
- Implementation
  - `TreeViewItem` 对普通 `ITreeItemNode` 回写 `IsChecked` / `IsSelected` / `IsExpanded`（`ApplyNodeData` 之后开始，容器清理时停止），与绑定型节点共用 `SyncTreeItemNodeFromContainer`；`ApplyNodeData` 期间写到尚无索引容器上的 `IsChecked` 变化不触发勾选级联。
  - `TreeView.KeyboardNavigation.cs` 新增 active 路径、容器准备通知 `NotifyTreeViewItemPrepared` 与按需实现容器的 `RealizeContainer`；`TreeView` / `TreeViewItem.ContainerForItemPreparedOverride` 末尾通知 TreeView。非虚拟化面板的导航路径不变。
- Tests
  - 新增 `TreeViewVirtualizationStateTests`：`TreeItemNode` 与 `BindableTreeItemNode` 在虚拟化回收后保留展开、选中，以及勾选级联后的容器与数据勾选状态。
  - `TreeViewKeyboardNavigationTests` 新增顶层与子层均为虚拟化面板时 ↑↓ Home End 越过视口、从下方进入已展开节点末子节点的用例。
- Docs
  - 更新 `keyboard-navigation-design.md` 的定位、§7.1 遍历、§7.3 active item 生命周期、§8 性能、§9 兼容性（虚拟化下回收的容器从节点数据恢复状态、Shift 范围只含已实现部分）与 §10 验证；同步 `implementation.md` §7.8 与 `overview.md` §8.8。
  - `overview.md` 节点契约补充容器与节点的双向状态流（所有 `ITreeItemNode` 回写）；`implementation.md` 数据流图、§7.7 与维护不变量补充普通节点回写与重新准备时不触发级联。

## 2026-11-11

- Design
  - 新增 `keyboard-navigation-design.md`，定义 TreeView 键盘导航：TreeView 是树内唯一焦点目标和 Tab 停靠点，内部维护键盘活动项；方向键按可见、可用节点的先序移动活动项，左右键展开收起或进入 / 返回层级，RTL 互换左右。
  - 提交策略由 `IsSelectable`、`SelectionMode`、`ToggleType` 推导：单选且无勾选入口时选择跟随活动项，其余模式由 Enter / Space 提交勾选或选择；不新增公共模式属性。
  - 键盘与 switcher 共用 `TreeViewItem` 展开请求入口（含异步加载）；TreeSelect 等浮层宿主通过内部 `HandleKeyDown` 转发按键。
- Theme
  - 定义 `TreeViewItem` / `TreeViewItemHeader` 的 `:keyboard-active` 伪类，复用 `NodeHoverBg`；TreeView 可聚焦，header、switcher、checkbox / radio indicator 固定不可聚焦。
- Implementation
  - 新增 `TreeView.KeyboardNavigation.cs`：重写 `OnKeyDown` / `OnGotFocus` / `OnLostFocus`，重新实现 `ICustomKeyboardNavigation`；新增内部宿主入口 `HandleKeyDown`、`ClearKeyboardActiveItem` 与开关 `IsKeyboardMoveSelectionEnabled`。
  - `TreeViewItem` 新增统一展开入口 `RequestToggleExpand`，`NodeSwitcherButton.Toggle` 改为调用它；`TreeViewItem.OnKeyDown` 不再处理按键。
  - TreeView 主题设置 `Focusable=True` 与空 `FocusAdorner`；TreeSelect 下拉树不可聚焦并关闭移动即选择。
- Gallery
  - TreeView ShowCase 新增“键盘操作”示例（单选、勾选两棵树）。
- Tests
  - 新增 `TreeViewKeyboardNavigationTests`（22 项）与 `TreeSelectKeyboardNavigationTests`（3 项）。
- Docs
  - 更新 `overview.md` 键盘行为、伪类、集成关系、兼容性不变量、§8.8 键盘导航模型与验证策略。
  - 更新 `implementation.md` 源码结构、键盘导航流、键盘与焦点处理、§7.8 活动项维护、维护不变量与验证范围；更新 `token.md` 的活动项 Token 使用说明。
- Docs
  - `implementation.md` 增加维护约束：替换节点 Template 的派生主题必须把 `IsKeyboardActive` 投射到 header。
- Fix
  - Shift+方向键扩展多选范围时只同步差量：保留仍在范围内的已选项，移除范围外的项并一次加入新增项，`SelectionChanged` 不再先报告清空再逐项加入的中间状态。
- Theme
  - 键盘活动项新增焦点描边（Ant Design 列表 `:focus-visible` 时给活动节点的内容框加 `genFocusOutline`）：TreeView 自身持有焦点且 active indicator 可见时，描边画在 `PART_HeaderContentFrame` 外侧；TreeSelect 等宿主转发按键时只显示背景，不描边。
  - `DragIndicatorLineWidth` 改取 `LineWidthBold`（2px）：`LineWidthFocus` 调整为 3px 后，拖拽指示线保持原粗细。
- Implementation
  - `TreeViewItem` / `TreeViewItemHeader` 新增内部状态 `IsKeyboardFocusOutlineVisible`；header 通过共享的 `ManagedFocusOutline` 显示 `DefaultFocusAdorner`，离开视觉树时移除。设计见 `keyboard-navigation-design.md` §6.3.1。

## 2026-10-28

- Motion
  - `OpenMotion` / `CloseMotion` 的值类型改为 `AbstractMotion`，`Motion` 专门拥有附加属性和目标级操作；TreeView 子项的 AXAML 接入与布局协议保持不变。

## 2026-10-27

- Motion
  - `OpenMotion` / `CloseMotion` 改为 `Motion`，控件不再修改共享定义的 Duration。
  - 子项采用 `ContentViewport#PART_ChildrenViewport`，TreeSelect/Transfer 复用相同布局能力。
  - 反向意图连续替代播放，移除 switcher 的播放期禁点和丢弃请求 guard；旧模板的完成无权修改当前状态。

## 2026-10-19

- Behavior
  - Preserve pre-reset TreeView selection and checked state across `ItemsSource` replay and publish only final `SelectionChanged`, `CheckedItemsChanged` and Form value differences. Consecutive explicit collection edits and assignments made before replay completes take precedence; a nested source replacement takes over the pending replay; a filter failure clears the replay transaction.

## 2026-10-03

- Implementation
  - Establish drag state synchronously and finish it before completion callbacks. Release previews through their actual visual parent; detach cancels pending drops and ends the drag without queued access to a cleared node.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record TreeView as the semantic owner, with TreeViewFlyout used only as the relay adapter.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-20

- Design
  - 新增 `semantic-part.md`，定义 TreeView Semantic Part 契约：TreeView 是递归层级容器，用 `TreeView` 与
    `TreeViewItem` 两个递归 Semantic owner 表达 `root`、`item`、`itemSwitcher`、`itemIndicator`、`itemIcon`、
    `itemTitle` 六个 Part，对齐 Ant Design 6.6.0 `TreeSemanticType`（`root` / `item` / `itemIcon` / `itemTitle` /
    `itemSwitcher`），并额外公开 AtomUI 扩展 `itemIndicator`（checkbox / radio 勾选指示）。
  - `TreeView` owner 公开 `root`、`item`（顶层容器）；`TreeViewItem` owner 公开 `root`、`item`（子容器）与
    `itemSwitcher` / `itemIndicator` / `itemIcon` / `itemTitle`（节点内容区域）。二者用同一 `.semantic-item` 身份保证
    任意深度节点可达。
  - `TreeViewItemHeader`、`NodeSwitcherButton`、`CheckBoxIndicator`、`FloatableTreeView` 不持有独立 Semantic
    descriptor：`itemSwitcher` 与 `itemIndicator` 的 `ContractType` 使用公开基类 `ToggleButton`。
  - 定义 marker 路由：`.semantic-item`（容器，`> .semantic-item`，`TreeView` 与 `TreeViewItem` 各自声明）、
    `.semantic-scope-header`（internal 跳点）、`.semantic-item-switcher` / `.semantic-item-indicator` /
    `.semantic-item-icon` / `.semantic-item-title`（header 模板静态 marker，route 从 `TreeViewItem` owner 出发）。
    `itemIndicator` 是 checkbox / radio 两个备选节点共用的单一 Part。
- Docs
  - 更新 `overview.md` §3.1 Semantic Part 摘要、§9 LLMS 语义区域映射与验证策略。
  - 更新 `implementation.md` 源码文件结构、§5.1 Semantic Part marker 放置与路由、维护不变量与验证范围。
- Tests
  - 新增 `TreeViewSemanticPartTests`，覆盖两个 descriptor 的 Part 数量/顺序/字段、模板 marker 静态声明、运行时 marker
    数量、`TreeView` 顶层 route 与 `TreeViewItem` 递归 route 命中、生成 Style 的 Setter 生效。

## 2026-07-08

- Docs
  - 明确 TreeView 拖拽重排采用数据源驱动设计：UI 容器只负责命中和视觉状态，结构修改由内部数据控制器操作 root 数据源或节点 `Children`。
  - 记录轻量 `TreeNodeIndex` 的性能边界：drop 定位使用增量索引，拖拽过程中不按节点引用全树扫描。
  - 补充拖拽 move 的状态一致性要求：节点移动不按删除清理选中、勾选和展开状态，跨父级移动必须同步 parent node、权威集合和索引。

## 2026-07-05

- Implementation
  - 修复多选 TreeView 在 ItemsSource 变化或首次容器状态回放时优先恢复 `SelectedItem`，导致 `SelectedItems` 被折叠成单选的问题；多选模式现在以 `SelectedItems` 路径作为运行期选择恢复的权威来源。
  - 修复 `IFormItemAware.SetFormValue` 写入 TreeView 选择值时把节点对象转换为字符串的问题；单选保持 `SelectedItem` 节点实例，多选保持 `SelectedItems` 列表实例。
- Gallery
  - 新增 `SelectedItem` / `SelectedItems` 绑定示例，并标记 `v6.0.8`。
- Tests
  - 新增 TreeView 多选 ItemsSource 回放优先级测试，覆盖 `SelectedItem` 与 `SelectedItems` 同时存在时的恢复顺序。
  - 新增 TreeView Form 值读写回归测试，覆盖单选、多选、读取和清空语义。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `TreeView`.
  - Align generated output paths with `controls/tree-view/index-cn.md` and `controls/tree-view/semantic-cn.md`.

## 2026-06-22

- Docs
  - 补充 TreeView 节点模型分层设计，明确 `TreeItemNode` 保持轻量 POCO / record 定位，不直接改造成 `AvaloniaObject`。
  - 记录 `BindableTreeItemNode` 绑定型节点设计边界，用于 XAML binding target、`DynamicResource` 和动态资源场景。
  - 补充绑定型节点的 scoped resource host、owner attach/release、属性同步和容器生命周期约束。
- Implementation
  - 新增 `BindableTreeItemNode`，通过 Avalonia DirectProperty 承载节点属性，并使用 scoped resource host generator 管理动态资源宿主。
  - TreeView / TreeViewItem 生成容器时接入绑定型节点的资源宿主 attach、属性同步、checked / selected / expanded 回写和容器 clear 释放。

## 2026-06-19

- Docs
  - 新增 TreeView 桌面版架构设计文档，记录控件定位、公共契约、行为状态模型、视觉主题模型、兼容性不变量和专项模型。
  - 新增 TreeView 桌面版实现原理文档，记录源码职责、默认状态回放、路径遍历、勾选同步、过滤、异步加载、拖拽和维护不变量。
  - 新增 TreeView Token 设计文档，记录节点尺寸、状态色、结构间距、拖拽和过滤 Token 边界。
  - 在 Data Display 分类入口中登记 TreeView 文档。
