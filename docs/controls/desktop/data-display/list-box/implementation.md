# ListBox 桌面版实现原理

本文档描述 ListBox 桌面版的内部源码结构、状态流转、容器生命周期、过滤、点击、空状态和 CandidateList 复用边界。公共设计与 API 契约见 [ListBox 桌面版架构设计](overview.md)，Semantic Part 契约见 [ListBox Semantic Part 契约](semantic-part.md)，Token 语义见 [ListBox Token 设计](token.md)，变化记录见 [ListBox Changelog](changelog.md)。

Token默认由生成的V2静态计划求值，所有既有Own保持原字段和资源ID；本轮14字段仅有Global输入。disabled内容前景在既有ControlTheme style Setter读取自身ItemDisabledColor，不写本地值、不增加状态事件或资源订阅。容器、数据与回收逻辑未因该修复改动。

## 1. 实现定位

ListBox 的实现目标是在 Avalonia `ListBox` 基类上增加 AtomUI 的视觉、Token、空状态、选中指示器、过滤高亮和候选列表复用能力，并保持 Avalonia selection model 与 keyboard navigation 语义稳定。

本文档覆盖 `ListBox`、`ListBoxItem`、ListBox token、AXAML theme、虚拟化上下文接口和 CandidateList 继承关系。具体属性注册、CLR wrapper 和小型 guard 逻辑仍应直接阅读源码。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/ListBox/ListBox.cs`：public API、事件、容器生成、分割线同步、空状态、过滤、选择拦截、键盘导航和虚拟化上下文管理。
- `src/AtomUI.Desktop.Controls/ListBox/ListBoxItem.cs`：条目容器、点击 routed event、选中指示器状态、过滤文本展示状态、分割线状态、pointer selection 协同和动效启停。
- `src/AtomUI.Desktop.Controls/ListBox/ListBoxItemClickedEventArgs.cs`：`ItemClicked` 事件参数。
- `src/AtomUI.Desktop.Controls/ListBox/ListBoxToken.cs`：ListBox 专属 Token 定义。
- `src/AtomUI.Desktop.Controls/ListBox/Themes/ListBoxTheme.axaml`：root 模板、ScrollViewer、ItemsPresenter、EmptyIndicator、默认 ItemTemplate、root 圆角内容裁剪和 root 样式。
- `src/AtomUI.Desktop.Controls/ListBox/Themes/ListBoxItemTheme.axaml`：条目模板、选中指示器、普通内容、过滤高亮文本、分割线和条目状态样式。
- `src/AtomUI.Desktop.Controls/Primitives/CandidateList/CandidateList.cs`：基于 ListBox 的候选项列表扩展。
- `src/AtomUI.Controls.Shared/IListVirtualizingContextAware.cs`：虚拟化上下文保存、恢复和清理接口。

## 3. 核心类职责

`ListBox` 是列表根控件，负责注册 token scope、创建 `ListBoxItem` 容器、绑定 root 状态到 item 状态、处理空状态、过滤条目、阻止不可选择状态下的 selection 更新、派发 `ItemClicked` 和保存虚拟化容器上下文。

`ListBoxItem` 是条目容器，继承 Avalonia `ListBoxItem`。它负责承载内容、选中状态、pointer 输入、点击 routed event、选中指示器可见性、过滤文本展示输入和条目动效。

`ListBoxToken` 是组件级设计变量层，为 root、item、selected indicator 和 filter highlighter 提供组件语义值。

`CandidateList` 继承 ListBox，增加候选项导航、候选项临时高亮、commit / cancel 事件和最大选择数量控制。它复用 ListBox 的容器生成、过滤、空状态和 item theme。

## 4. 状态与数据流

容器生成流：

```text
Items / ItemsSource
      ↓
ListBox.CreateContainerForItemOverride
      ↓
ListBoxItem
      ↓
PrepareContainerForItemOverride
  ContentTemplate <- ItemTemplate
  SizeType <- ListBox.SizeType
  IsMotionEnabled <- ListBox.IsMotionEnabled
  SelectedIndicator <- ListBox.SelectedIndicator
  ItemHoverBg / ItemSelectedBg
  IsShowSelectedIndicator
  FilterHighlightStrategy / FilterHighlightForeground
  IsSplitLineVisible（按视图位置计算）
  virtual index / restored context
```

分割线状态流：

```text
视图位置 / IsBorderless
      ↓
ListBox.ConfigureSplitLineVisibility(container)
      ↓
ListBoxItem.IsSplitLineVisible
      ↓
ListBoxItem.ConfigureEffectiveBorderThickness
  IsSplitLineVisible && BorderThickness.Bottom > 0
      ↓
IsSplitLineEffectiveVisible + EffectiveBorderThickness
      ↓
SplitLineFrame 底边线
```

选择状态流：

```text
Pointer / keyboard event
      ↓
ListBoxItem.OnPointerPressed / OnPointerReleased 或 ListBox.OnKeyDown
      ↓
ListBox.UpdateSelectionFromPointerEvent / UpdateSelectionFromEvent
      ↓
Avalonia Selection model
      ↓
ListBoxItem.IsSelected
      ↓
IsSelectedIndicatorVisible = IsShowSelectedIndicator && IsSelected
```

过滤状态流：

```text
Filter / FilterValue / FilterValueSelector / FilterHighlightStrategy
      ↓
ListBox.FilterItems
      ↓
SelectFilterValue(item)
      ↓
IValueFilter.Filter(value, FilterValue)
      ↓
ListBoxItem.IsFiltering / FilterValue / IsVisible
      ↓
FilterResultCount + IsEffectiveEmptyVisible
```

空状态流：

```text
ItemCount changed
ItemsSource changed
IsFiltering changed
FilterResultCount changed
      ↓
ConfigureEmptyIndicator
      ↓
IsEffectiveEmptyVisible
      ↓
PART_ScrollViewer hidden / EmptyIndicator shown
```

## 5. 生命周期与模板接入

`ListBox` 静态构造注册三类 class handler：

- `ListBoxItem.ClickedEvent` 收敛到 `HandleListBoxItemClicked`。
- `IsSelectableProperty.Changed` 在关闭可选状态时清空选择。
- `ItemCountProperty.Changed` 触发 `ItemCountChanged`。

`ListBox` 实例构造注册 `ListBoxToken` scope，并监听 `Items.CollectionChanged`。集合变化时清空虚拟化恢复上下文，刷新空状态，并重新执行过滤。

`OnInitialized` 设置默认 `Filter`，刷新空状态和 `IsFiltering`。`OnLoaded` 在已有 `FilterValue` 时执行过滤。

`PrepareContainerForItemOverride` 是 root 状态同步到 item 的核心入口。它使用 Avalonia `[!]` direct binding 将同生命周期状态绑定到容器，并在虚拟化场景下设置 `VirtualIndex`、恢复缓存上下文和临时关闭 motion，避免回收容器出现初始化动画。它还按容器当前视图位置计算 `IsSplitLineVisible`，最后一项抑制与 `IsBorderless` 例外见 7.6。

root 模板的 `Frame`（`PixelAlignedBorder`）开启 `ClipContentToCornerRadius`：内容被裁剪到外框圆角的内边缘，条目
hover / selected 背景不会溢出圆角口袋区，视觉上与 CSS `border-radius + overflow: hidden` 一致。外框环由 `Frame`
自身一次绘制（`BorderBrush` = `ColorSplit`，与分割线同色同粗细），不存在二次叠加。实现见 7.6。

`ClearContainerForItemOverride` 是容器回收清理入口。它保存需要跨回收恢复的上下文，清理容器本地值，再交给基类释放容器。新增容器级状态时必须同时补齐 save、restore、clear 三个路径。

`ListBoxItem.OnInitialized` 初始化选中指示器状态；初始过渡由共享初始化会话管理，见下节。

### 5.1 Semantic Part 处置

Batch 2 Gate A 审计结论：ListBox 以 Ant Design 6.6.0 新增的 `Listy` 组件为上游基线，公开 `root`、`item` 两个
Semantic Part；`groupHeader` 不适用（ListBox 没有分组功能）。上游证据与 ListView 共用同一基线，详见
[ListView 桌面版实现原理](../list-view/implementation.md) 的 Semantic Part 处置一节（`classNames` / `styles` 均为
`{ root?, item?, groupHeader? }`，全部 6.6.0；`.ant-listy` 为滚动容器，`.ant-listy-item` 为内间距 `itemPaddingBlock ×
itemPaddingInline` + 底部分割线 `colorSplit` + hover `controlItemBgHover`）。

AtomUI 对应审计与映射：

- `root` Part → `ListBox` owner 本身。滚动容器结构（`Frame` + `PART_ScrollViewer` + `ItemsPresenter`）已经存在，
  无结构差距；隐式 Part，不生成 Style，不添加 `.semantic-root`。
- `item` Part → 每个 `ListBoxItem` 容器。已有 `ItemPadding*` 三档、`ItemHoverBg` / `ItemSelectedBg` 与
  `IsMotionEnabled` 背景过渡；与 Listy 基线的差距是条目底部分割线（`borderBottom: colorSplit`），默认视觉对齐与
  ListView 的同一决策一并批准：外框与分割线统一 `ColorSplit`、条目表面直角、`ContentPadding` / `ItemMargin` 归零、
  root `Frame` 开启 `ClipContentToCornerRadius` 内容裁剪、最后一项分割线由 root 外框下边缘闭合（`IsBorderless` 时
  保留）。marker `.semantic-item`
  在 `CreateContainerForItemOverride` 创建路径用生成常量一次性添加，`PrepareContainerForItemOverride` 幂等补齐
  （覆盖用户直接提供容器与 `CandidateListItem` 派生容器的路径，与 Collapse `.semantic-scope-item` 的建立纪律一致；
  ListBoxItem 只有一种身份，不存在切换）。Part 声明 `RuntimeCreated=true`，SelectorRoute 为 `> .semantic-item`
  （容器逻辑父级是 ListBox owner，沿逻辑树一步直达），`ContractType` 为公开 `ListBoxItem`，`Cardinality` 为
  `Multiple`。
- `groupHeader` 不适用于 ListBox：ListBox 是轻量选择列表，只有选择、过滤与 CandidateList 基座职责，没有分组
  功能，不虚构 Part。
- 行为 API 对照：`virtual`（默认虚拟化 ItemsPanel）、`height`（`Height`）、`items` / `itemRender`（`ItemsSource` /
  `ItemTemplate`）、`rowKey`（Avalonia selection 语义）、`onScroll`（ScrollViewer 事件）均有直接对应；`group`、
  `sticky`、`scrollTo` 不适用或不在本轮范围。
- 排除范围：selection（`SelectedIndicator`）、filter 高亮（`HighlightableTextBlock` 与
  `FilterHighlightForeground` / `FilterHighlightStrategy`）、empty 展示节点，均不发布为 Part。

重新评估触发条件：ListBox 新增分组能力时，重新评估 `groupHeader` 映射；上游新稳定版调整语义键时重新核对契约。

## 6. 交互与事件处理

pointer selection：

- 鼠标左键和右键按下时立即通过 owner ListBox 更新 selection。
- 触摸和笔输入延迟到 release，避免按下阶段阻断滚动手势。
- touch / pen release 成功更新 selection 后，ListBoxItem 会将事件重新 raise 到 owner，以便上层完成 commit 类行为。

click event：

- 左键 pressed 时，ListBoxItem raise `ClickedEvent`。
- ListBox 通过 class handler 接收后调用 `NotifyListBoxItemClicked`，再触发 public `ItemClicked`。
- CandidateList 通过重写 `NotifyListBoxItemClicked` 在单选模式提交候选项。

keyboard navigation：

- `IsSelectable=true` 时，方向键进入 Avalonia `MoveSelection`。
- 多选模式下，平台 select-all 手势调用 `Selection.SelectAll()`。
- `IsSelectable=false` 时，ListBox 不处理这些键盘选择路径。

context request：

- ListBoxItem 右键 release 且事件 source 为当前 item 时，创建 `ContextRequestedEventArgs` 并 raise context request。

## 7. 内部算法与关键流程

### 7.1 Effective border thickness

`IsBorderless` 和 `BorderThickness` 合成 `EffectiveBorderThickness`。模板只绑定 effective 值，从而保持 borderless 模式与普通边框模式共享同一 root 模板。

### 7.2 过滤值选择

`SelectFilterValue` 按以下顺序选择过滤值：

1. 使用 `FilterValueSelector(item.Content)`。
2. 对 `IListItemData` 使用 `Content?.ToString()`。
3. 对字符串内容直接使用字符串。
4. 其他内容使用 item content 本身。

该路径服务轻量文本列表。复杂模板场景应通过 `FilterValueSelector` 提供稳定文本值。

### 7.3 过滤上下文

隐藏未命中项时，ListBox 使用 `_filterContext` 备份进入过滤前的容器 `IsVisible`。清除过滤时按备份恢复可见性，并清理 item 的 `IsFiltering` 和 `FilterValue`。

维护此路径时必须保证：

- 筛选清除后不会遗留本地 `IsVisible`。
- 容器回收时不会把旧 filter 状态带到新 item。
- `FilterResultCount` 与空状态能表达完整数据结果，而不是只表达当前 viewport 中已实现容器。

### 7.4 虚拟化上下文

ListBox 实现 `IListVirtualizingContextAware`，用 `_virtualRestoreContext` 保存被回收容器的上下文。默认保存 `IsEnabled`，派生控件可以重写 `NotifySaveVirtualizingContext`、`NotifyRestoreVirtualizingContext` 和 `NotifyClearContainerForVirtualizingContext` 扩展上下文。

上下文保存以 `VirtualIndex` 为 key。`PrepareContainerForItemOverride` 会在相同 index 的容器重新实现时恢复上下文并移除缓存。

### 7.5 CandidateList 扩展点

CandidateList 使用 ListBox 的 container pipeline，但替换容器类型为 `CandidateListItem`，并覆盖：

- `CreateContainerForItemOverride`
- `NeedsContainerOverride`
- `ConfigureEmptyIndicator`
- `NotifyRestoreDefaultContext`
- `NotifyClearContainerForVirtualizingContext`

这些扩展点要求 ListBox 的基础 prepare / clear 顺序稳定，避免候选项临时高亮、disabled 状态和 selection 状态在虚拟化回收时串扰。

### 7.6 条目分割线与圆角内容裁剪

`ListBoxItem` 持有三个 internal DirectProperty：`IsSplitLineVisible`（ListBox 写入的分割线决策）、
`IsSplitLineEffectiveVisible` 与 `EffectiveBorderThickness`（由 `ConfigureEffectiveBorderThickness` 合成）。
合成规则为 `showSplitLine = IsSplitLineVisible && BorderThickness.Bottom > 0`；条目模板的 `SplitLineFrame` 只绑定
`EffectiveBorderThickness`、`IsSplitLineEffectiveVisible` 和 `BorderBrush`，因此 Semantic Style 对容器
`BorderThickness` / `BorderBrush` 的覆盖直接作用到分割线。

`IsSplitLineVisible` 由 ListBox 在 `PrepareContainerForItemOverride` 按容器视图位置计算：
`IsBorderless || index < Items.Count - 1`。最后一项的分割线被抑制，由 root 外框下边缘承担闭合线；`IsBorderless`
时保留（外框消失后由分割线承担闭合线）。容器回收后重新 prepare 会按新位置重算；集合变化
（`Items.CollectionChanged`）与 `IsBorderless` 变化时，ListBox 对所有已实现容器重新同步，保证追加条目后原最后一项
恢复分割线、删除后新最后一项被抑制。

圆角闭合由内容裁剪承担：root 模板的 `Frame`（`PixelAlignedBorder`）设置 `ClipContentToCornerRadius="True"`，
`DashedBorder` 按 `Frame` 自身的 `BorderThickness` / `CornerRadius` 用
`RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI`（`BackgroundSizing.InnerBorderEdge`，即外框环的
内边缘：边界缩小一个边框厚度、半径减半线宽）构建圆角矩形几何并赋给内容的 `Visual.Clip`。裁剪几何在子内容坐标系
中构建：子内容被 padding + 边框厚度内缩，因此先按 padding + 边框厚度把子内容边界膨胀回外框内边缘，再交给 builder
内缩（两次抵消后裁剪矩形恰好是子内容边界加回 padding）。条目 hover / selected 背景因此被裁剪到圆角内边缘，视觉上
与 CSS `border-radius + overflow: hidden` 一致；滚动时裁剪随内容平移持续生效。外框环由 `Frame` 自身一次绘制，无
任何叠加节点，`BorderBrush`（`ColorSplit`）与分割线同色同粗细。

裁剪会参与 Avalonia 组合器命中测试（`visual.Clip?.FillContains(point)`），因此 `DashedBorder` 在应用裁剪前用
裁剪图形自身探测平台几何命中能力（`FillContains(内部点) && !FillContains(外部点)`）：生产后端（Skia）对圆角几何的
包含判定正确，裁剪正常生效；若运行平台无法正确判定圆角几何包含（headless 测试平台的流式几何桩只跟踪 ArcTo 端点、
按连续三点三角扇近似），裁剪降级为不应用，保证列表内指针输入不受影响。探测点选择由 `SupportsGeometryClipHitTesting`
虚方法承载，测试可覆写以验证裁剪路径。

## 8. 资源、性能与 AOT 边界

ListBox 不通过反射访问模板内部结构。模板接入依赖稳定 part 名称和 Avalonia 属性绑定。

同生命周期的 root 到 item 状态同步使用 Avalonia `[!]` direct binding，不使用 `BindUtils.RelayBind` 和额外 disposable。容器被 ItemsControl 管理，binding 生命周期随容器生命周期结束。

选中指示器、过滤高亮和空状态内容是 AXAML 静态模板节点。隐藏状态依赖 `IsVisible`，不通过 C# 动态创建和销毁模板子节点。

`IsVisible=false` 的节点不参与 measure、arrange 和 render；因此过滤空状态、隐藏未命中项和默认隐藏选中指示器可以优先使用静态模板 + visible state 模式。

过滤和虚拟化路径不得引入固定延时、dispatcher timing hack 或全局订阅。新增缓存必须有明确失效点，并在 collection change、container clear 或 detach 相关路径中清理。

## 9. 维护不变量

内部重构必须保持以下不变量：

- `ListBox.cs` 保留 public API、事件、容器生命周期和虚拟化上下文入口。
- `ListBoxItem.cs` 保持容器角色，不承载业务数据源请求、排序、分页或跨列表全局状态。
- `PrepareContainerForItemOverride` 新增状态同步时，必须在 `ClearContainerForItemOverride` 或 `NotifyClearContainerForVirtualizingContext` 对称清理。
- `FilterValueSelector` 是复杂数据项的过滤值入口，不应在 ListBox 内部硬编码业务数据类型。
- `FilterHighlightStrategy` 只控制过滤展示，不改变 selection model。
- 选中指示器可见性只能由 `IsShowSelectedIndicator && IsSelected` 推导。
- `ItemClicked` 派发顺序必须允许 CandidateList 在 public event 前执行 `NotifyListBoxItemClicked`。
- `IsBorderless` 只影响边框厚度，不改变 root padding、corner radius 或 scroll behavior。
- Semantic Part 边界：`ListBox` 只发布 `root` / `item` 两个 Part；`item` 的 marker 在容器创建与 prepare 路径一次性
  幂等建立、不随状态切换，静态模板不增删 marker，默认主题不消费 `.semantic-*` selector；selection、filter 高亮与
  empty 不属于 Part。
- 分割线状态机：`IsSplitLineVisible` 只由 ListBox 按容器视图位置与 `IsBorderless` 计算，容器不得自行决定；集合变化
  与 borderless 变化必须重新同步已实现容器；Semantic Style 不能绕过最后一项抑制。
- 条目模板保持 `SplitLineFrame` 绑定 `EffectiveBorderThickness` / `IsSplitLineEffectiveVisible`，默认分割线为 1 DIP
  `ColorSplit`；root 外框与分割线共享同一 `ColorSplit` 基线。
- root 模板保持 `Frame` 开启 `ClipContentToCornerRadius`：内容被裁剪到外框圆角内边缘，条目状态背景不溢出圆角
  口袋区；外框环由 `Frame` 自身一次绘制（`ColorSplit`，与分割线同色），不得再叠加任何覆盖节点。裁剪应用前必须通过
  `SupportsGeometryClipHitTesting` 探测平台几何命中能力，无法正确判定圆角几何包含的平台降级为不应用裁剪。
- 条目表面保持直角与贴边：不恢复条目圆角，`ContentPadding` / `ItemMargin` 保持 `Thickness(0)`。
- Token 变更必须同步 `ListBoxTokenKind`、AXAML 引用和 token.md 语义说明。

## 10. 测试与验证

ListBox 相关验证入口：

- `tools/performances/AtomUI.Performance/Suites/ListBox/ListBoxStateVerification.cs`
- `tools/performances/AtomUI.Performance/Suites/ListBox/ListBoxScenarios.cs`
- `controlgallery/AtomUIGallery/ShowCases/DataDisplay/List/`
- AutoComplete、Mentions 和 Cascader 中使用 CandidateList / ListBox 派生控件的场景。

建议验证命令：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj -c Debug --framework net10.0 --no-restore -- --verify-listbox-states
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj -c Debug --framework net10.0 --no-build -- --suite listbox --count 60
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore
git diff --check
```

验证重点：

- 默认 ListBox 静态 shape：选中指示器和过滤文本节点默认隐藏。
- selected indicator 生命周期：选中、取消选中、切换选中项。
- filter 生命周期：进入过滤、清除过滤、隐藏未命中、空结果。
- CandidateList keyboard navigation、commit、cancel 和 empty navigation。
- 虚拟化回收后 disabled、selected indicator、candidate selected 和 filter 状态不串扰。
- Semantic Part 边界：N 个条目时 `semantic-item` 共 N 个；空集合为 0；容器复用/回收与 items 变化不增删 marker；
  root 表面投影生效；默认主题不消费 `.semantic-*` selector。
- 分割线与内容裁剪：hover / selected 背景不溢出圆角内边缘（被 `Frame` 的圆角内容裁剪约束）；最后一项无分割线且外框
  下边缘为单一闭合线；`IsBorderless` 时最后一项保留分割线；追加 / 删除条目后原最后一项恢复、新最后一项抑制；
  三档 SizeType 下规则一致。
- Gallery List ShowCase、AutoComplete、Mentions、Cascader 相关候选列表显示稳定。

## 状态动效初始化

`ListBoxItem` 在构造阶段抑制过渡，覆盖生成容器挂载后才写入的初始选择；自身加载完成后恢复最新集合。父控件在清理容器之前复位会话，使虚拟化清理和下次挂载共用初始化边界。
共享目标与生命周期契约见 [状态动效初始化架构](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)。
