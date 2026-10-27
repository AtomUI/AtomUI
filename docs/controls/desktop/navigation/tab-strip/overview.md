# TabStrip 桌面版架构设计

本文档定义 `TabStrip` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [TabStrip 桌面版实现原理](implementation.md)，TabControl / TabStrip 共用的溢出定制、快照和生命周期契约见 [TabControl / TabStrip 溢出弹层设计](../tab-control/overflow-popup-design.md)，公开 Semantic Part 契约见 [TabStrip Semantic Part 契约](semantic-part.md)。TabStrip 没有独立 Token 文档；主题主要复用 SharedToken 或 TabControl 家族 Token。设计和契约变化记录见 [TabStrip Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `BaseTabStrip`（由 `TabStrip` 与 `CardTabStrip` 继承）；internal `IsPopupPinnedOpen` 只供测试和内部诊断使用，并由统一 `TabScrollViewer` 直接收敛到 `PART_OverflowPopup`。控件卸载、锚点失效、TopLevel 改变、模板替换和模板重建始终拥有生命周期关闭权。

## 1. 控件定位

独立 TabStrip 的选中指示条首次显示时直接匹配当前选中容器，后续有效切换保留原有过渡；实例独立维护其加载、
模板及初始几何条件。共享契约见 [状态动效初始化设计](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)，
源码接入差异与证据见 [状态动效初始化验证](../../../../architecture/systems/control-infrastructure/state-motion-initialization-verification.md)。

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Navigation/TabStrip` |
| 控件状态 | Stable |

TabStrip 是 AtomUI 桌面控件体系中的标签条控件，用于展示、选择、关闭和拖动排序页签，不承载内容页。

TabStrip 不负责完整 TabControl 内容容器或主导航菜单。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/TabControl/TabStrip`

## 2. 设计语言

TabStrip 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | TabStrip 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | TabStrip 是 AtomUI 桌面控件体系中的标签条控件，用于展示、选择、关闭和拖动排序页签，不承载内容页。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `CloseIcon`、`HeaderEndEdgePadding`、`HeaderEndExtraContent`、`HeaderEndExtraContentTemplate`、`HeaderStartEdgePadding`、`HeaderStartExtraContent`、`HeaderStartExtraContentTemplate`、`Icon`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | selection/checked/active、reorder、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | SharedToken / 关联控件 Token + ControlTheme。 |

## 3. API 与契约模型

TabStrip 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `CloseIcon`、`HeaderEndEdgePadding`、`HeaderEndExtraContent`、`HeaderEndExtraContentTemplate`、`HeaderStartEdgePadding`、`HeaderStartExtraContent`、`HeaderStartExtraContentTemplate`、`Icon`、`OverflowPopupTemplate` | 定义控件展示内容、输入数据、模板或业务对象入口；`OverflowPopupTemplate` 的 data item 固定为 `TabOverflowPopupContext`。 |
| 选择与集合 | `IsTabReorderEnabled`、`TabActivationTrigger`、`SelectedIndex`、`SelectedItem`、`ItemsSource` | 维护页签选择触发时机、集合顺序和拖动排序状态。 |
| 交互与状态 | `IsAutoHideCloseButton`、`IsClosable`、`IsMotionEnabled`、`IsShowAddTabButton`、`IsTabAutoHideCloseButton`、`IsTabClosable` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `SizeType`、`TabAlignmentCenter`、`TabStripPlacement` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |

稳定事件包括 `AddTabRequest`、`Closed`、`Closing`、`TabReordering`、`TabReordered`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 控件与数据类型：`BaseTabStrip`、`CardTabStrip`、`TabStrip`、`TabStripClosedEventArgs`、`TabStripClosingEventArgs`、`TabStripItem`、`TabOverflowPopupContext`、`TabOverflowItem`。
- 枚举：`TabActivationTrigger`、`TabSharp`。
- `TabScrollViewer`、`ITabOverflowOwner`、默认 overflow menu 及其 item container 均为 internal 实现，不属于用户 API。

稳定 template part：

当前 owner 没有显式 `[TemplatePart]` 契约；共用 `TabScrollViewerTheme` 的 `PART_OverflowPopup`、
`PART_ScrollStartEdgeIndicator`、`PART_ScrollEndEdgeIndicator` 与 `PART_ScrollMenuIndicator` 是家族内部模板协作边界。

控件专属或内部伪类包括 `TabPseudoClass.Bottom`、`TabPseudoClass.Left`、`TabPseudoClass.Right`、`TabPseudoClass.Top`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 4. 行为与状态模型

TabStrip 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- selection/checked/active、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。
- Tab 激活触发由 `TabActivationTrigger` 控制，默认值为 `PointerReleased`；按下时只记录候选 Tab，只有鼠标在同一个 Tab 上松开才激活。
- `TabActivationTrigger=PointerPressed` 表达按下立即激活；该模式仍必须通过统一选择入口更新 `SelectedIndex`、`SelectedItem`、伪类和主题状态。
- `PointerReleased` 模式下，按下 Tab A、移动到 Tab B 或 Tab 外松开不应激活新 Tab；拖动排序进入 active reorder 后，释放事件不得再触发 Tab 激活。
- `IsTabClosable` 是生成 `TabStripItem` 的模板级默认值；overflow 菜单使用容器最终生效的 `IsClosable`，因此控件级默认、单项覆盖和 overflow 呈现必须保持同一语义。
- 拖动排序开启后，排序结果必须提交到 `ItemsSource` 或 `Items` 的逻辑集合顺序；拖动过程采用 Chrome 式轨道内实时让位预览，被拖 Tab 只沿 Tab 轨道主轴移动并覆盖在兄弟 Tab 上方，其他 Tab 通过临时 transform 让出目标位置，不能直接把 `ItemsPresenter.Panel.Children` 当作排序数据源。
- `TabStripPlacement=Top/Bottom` 时主轴为 X 轴，被拖 Tab 的 Y 位移必须保持为 0；`TabStripPlacement=Left/Right` 时主轴为 Y 轴，被拖 Tab 的 X 位移必须保持为 0。目标位置由被拖 Tab 的前进边缘跨过被覆盖兄弟 Tab 主轴中线决定：向后拖动使用 trailing edge，向前拖动使用 leading edge，相当于覆盖兄弟 Tab 约一半宽度或高度即触发让位，而不是由 pointer 的非主轴偏移决定。
- overflow 是当前打开会话的不可变 `TabOverflowItem` 快照，不拥有独立的选择或关闭语义；`TryActivate` 与 `TryClose` 必须经 `BaseTabStrip` 统一提交，旧会话 item 必须被拒绝。
- `OverflowPopupTemplate=null` 使用默认菜单；非空模板只替换弹层内容，不能改变溢出判定、placement、light-dismiss、选择或关闭 owner。
- `OverflowPopupTemplate` 默认值为 `null`，由 `TabStrip` 与 `CardTabStrip` 继承。模板 data item 固定为
  `TabOverflowPopupContext`；模板只能通过 `TryActivate`、`TryClose` 与 `Dismiss` 提交操作。
- pointer 点击 `PART_ScrollMenuIndicator` 打开 overflow Popup 时不得自动聚焦选中项、第一项、Popup 根节点或搜索框；焦点保持在激活器，只有用户后续显式 Tab/方向键导航或点击输入框时才进入弹层内容。
- 水平布局必须先为可见的 `PART_ScrollMenuIndicator` 保留空间；父级宽度缩窄、瀑布流换列或最终 arrange 小于先前 measure 时，激活器仍必须可见且完整落在 owner 边界内。

## 5. 视觉与主题模型

TabStrip 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

TabStrip 的 Line/Card ControlTheme 位于 `TabControl/Themes/TabStrip`，并复用 TabControl 家族统一的
`TabScrollViewerTheme` 与 `TabOverflowMenuTheme`。统一主题承载 edge indicator、更多按钮、静态 Popup shell 和默认菜单；
TabStrip 自身主题只负责 owner 布局与外观映射。

TabStrip 当前没有专属 Token 文档；主题通过 SharedToken、关联控件 Token 或继承主题资源表达视觉语义。运行时状态不得写入 Token 模型。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- 默认 `TabOverflowMenuTheme` 必须根据不可变 projection 的 `IsClosable` 控制关闭入口：不可关闭项不显示也不命中关闭按钮；可关闭项只通过 context `TryClose` 转发。
- 自定义 `OverflowPopupTemplate` 的 surface、搜索和空状态由应用模板负责；Popup host 仍由 AtomUI 负责定位、light-dismiss、pinned 与生命周期释放。
- Popup host 沿嵌套 `ContentPresenter` 解析最终 surface 的圆角；模板根与可见背景必须暴露一致的
  `CornerRadius`，item header/template 必须通过控件自身的 content pipeline 呈现，不能生成空白菜单项。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

家族Own资源显式复用`TabControl`，Global仍由实际owner决定。Middle水平Card的默认padding跟随有效`CardSize`；`CardTabStripAddStyle`可声明按钮尺寸，退出状态定制后恢复当前方向的尺寸关系。水平按钮高度跟随header自然布局，垂直按钮保持`CardSize`正方形。

TabStrip 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `BaseTabStrip`：public 基类，维护 public surface、逻辑集合、选择和统一关闭路径，并实现 internal `ITabOverflowOwner`。
- `CardTabStrip`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TabStrip`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TabStripItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `TabOverflowPopupContext` / `TabOverflowItem`：public overflow customization contract，公开不可变快照与受 owner 验证的 action。
- `TabScrollViewer`：internal sealed 共享宿主，维护滚动、溢出几何、Popup、快照缓存和释放。
- `TabOverflowMenu` / `TabOverflowMenuItem`：internal 默认呈现，不作为应用扩展点。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 TabStrip 时必须保持以下不变量：

- `OverflowPopupTemplate`、`TabOverflowPopupContext` 与 `TabOverflowItem` 是稳定 public customization contract；internal overflow host、默认菜单和容器不是兼容入口。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- 不把拖动排序实现为视觉容器重排；排序必须由集合 owner 提交，选择、overflow 菜单和滚动状态都从同一个集合顺序推导。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 选择与当前项模型

TabStrip 的当前项状态必须由单一 owner 推导。public 选择属性、集合项容器和伪类之间只能做单向同步，集合替换、清空和模板重套用时必须回放当前状态。

`TabActivationTrigger` 定义鼠标 pointer 触发选择的提交时机。默认 `PointerReleased` 适合避免误触：左键按下 Tab 时只记录候选项和 pointer，只有同一个 pointer 在同一个 Tab 上释放且未进入拖动排序时才提交选择。`PointerPressed` 则保持按下立即激活的桌面快速切换体验。

选择触发配置只影响 pointer 选择时机，不改变键盘导航、access key、程序化设置 `SelectedIndex` / `SelectedItem`、关闭 Tab 后自动选择和拖动排序后的选中项回放。实现中不得绕过 Avalonia `SelectingItemsControl.UpdateSelectionFromEvent`，否则会导致 item container、overflow 菜单和伪类状态不同步。

### 8.2 Tab 拖动排序模型

TabStrip 的拖动排序是选择与集合模型的扩展能力，由 `IsTabReorderEnabled` 控制。关闭该属性时，Tab 保持原有点击选择、关闭和溢出导航行为；开启后，左键按下并超过拖动阈值才进入排序状态，普通点击仍按原选择语义处理。

排序必须以逻辑 item 为单位提交，不以生成的 `TabStripItem` 视觉容器作为数据源。`ItemsSource` 为可写 `IList` 时移动 `ItemsSource`；没有 `ItemsSource` 时移动 `Items`；只读或不可写数据源不执行排序提交。拖动释放前触发 `TabReordering`，事件可取消；提交成功后触发 `TabReordered`。事件参数应表达拖动 item、原 index、目标 index 和取消状态，不要求业务层读取内部容器。

实时目标位置按 `TabStripPlacement` 选择主轴：`Top` / `Bottom` 使用 X 轴，`Left` / `Right` 使用 Y 轴。拖拽中的源 Tab 不做自由二维漂浮，只在主轴上跟随 pointer：横向轨道的 Y 位移为 0，纵向轨道的 X 位移为 0。兄弟 Tab 的让位位移和排序判断也只使用主轴坐标，目标 index 由源 Tab 的前进边缘跨过被覆盖兄弟 Tab 主轴中线决定：向后拖动使用 trailing edge，向前拖动使用 leading edge。`HeaderStartExtraContent`、`HeaderEndExtraContent`、添加按钮和 overflow 菜单项不参与排序目标计算；溢出场景下可在真实 Tab 视口边缘自动滚动，但不支持在 overflow 菜单内部直接拖动。

拖动视觉反馈必须接近 Chrome 浏览器标签行为：超过拖动阈值后源 Tab 提升到兄弟 Tab 上方并沿轨道移动；源 Tab 前进边缘未跨过被覆盖兄弟 Tab 的主轴中线前，兄弟 Tab 保持原位，不做让位也不切换排序目标；跨过半宽或半高阈值后，目标区间内的兄弟 Tab 通过临时 transform 按整格宽度或高度平滑让位。拖动中源 Tab 必须保持不透明背景；Line 模式使用当前激活面背景，Card 模式使用卡片激活面背景，避免覆盖时文字和图标叠穿。拖动中不使用插入线作为主反馈，不反复移动逻辑集合。释放时只提交一次真实集合 move；取消、capture lost、template reapply 或 detach 时恢复所有临时 transform、绘制层级和过渡设置。

选中状态必须跟随同一个逻辑 item，而不是跟随旧 index。重排完成后，`SelectedItem`、`SelectedIndex`、选中指示条、关闭按钮状态和 overflow 菜单都应从新的集合顺序重新推导，不能用延迟刷新或强制重设选择掩盖状态同步问题。拖动预览期间，如果选中 Tab 是被拖源或正在让位的兄弟 Tab，选中指示条必须叠加对应临时 transform 的主轴位移，使指示条跟随当前视觉位置，而不是停留在旧 layout bounds。

### 8.3 溢出页签、自定义与关闭模型

当页签空间不足时，统一 `TabScrollViewer` 将未完整显示的 `TabStripItem` 投影为不可变 `TabOverflowItem` 快照，并通过
`TabOverflowPopupContext` 提供给默认或自定义模板。完整 API、四控件映射、Popup Template、生命周期与性能门禁见
[TabControl / TabStrip 溢出弹层设计](../tab-control/overflow-popup-design.md)。

- `Item` 指向当前逻辑 item；`Header` / `HeaderTemplate` 来自源 `TabStripItem.Content` / `ContentTemplate`；`IsEnabled`、`IsSelected` 和 `IsClosable` 来自容器 effective state。
- `TryActivate` 与 `TryClose` 只接受当前会话 item。激活回到统一选择路径；关闭回到 `BaseTabStrip.CloseTab`，不能由 context、Popup 或菜单直接修改集合。
- 外部集合变化使当前快照失效并关闭 Popup；选择变化通过新不可变 projection 同步。
- 普通关闭可以保留无数据的模板视觉树以供重复打开复用，但 context 必须清空 item/header/template 与 owner action target。
- re-template、detach 和模板替换必须完全释放 Popup child、context、模板缓存、订阅与旧 owner。
- 默认与搜索 overflow 列表隐藏垂直滚动条但保留滚轮、触控板和程序化滚动；item surface 左右 inset 必须相等，不能由 scrollbar gutter 或单侧 item margin 改变内容宽度。

### 8.4 弹层与宿主模型

TabStrip 的 overflow 使用模板内静态 `PART_OverflowPopup` shell、overlay host、light-dismiss 与四向 edge-aligned placement。真实滚动 viewport 的阴影层包含静态 start/end edge indicator：`Top` / `Bottom` 使用 left/right 普通外阴影，`Left` / `Right` 使用 top/bottom 普通外阴影，方向与 Ant Design Tabs 一致；透明 indicator 位于 viewport 外侧，朝内的一面与真实裁剪边界重合，普通外阴影从该边界向内容区渐淡；阴影层按 viewport 裁剪且不参与命中测试。首次打开惰性创建内容，重复打开复用无数据视觉树；关闭释放打开态订阅与 projection，template reapply / detach 释放整个缓存。Pinned-open 不能阻止生命周期清理。

### 8.5 动效模型

TabStrip 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.6 视觉选项模型

TabStrip 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [TabStrip 桌面版实现原理](implementation.md)
- [TabControl / TabStrip 溢出弹层设计](../tab-control/overflow-popup-design.md)
- [TabStrip Semantic Part 契约](semantic-part.md)
- [TabStrip Changelog](changelog.md)

Part 的 Selector、ContractType、数量语义与定制边界以 [TabStrip Semantic Part 契约](semantic-part.md) 为唯一完整来源；内容页版本的 `TabControl` 家族语义区域见 [TabControl Semantic Part 契约](../tab-control/semantic-part.md)。

Token 说明：

- TabStrip 当前没有专属 `token.md`；Token 边界按第 5 节视觉与主题模型、SharedToken、控件家族 Token 或主题资源说明 Token 边界。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 selection/checked/active、reorder、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| 拖动排序 | 覆盖 Top/Bottom 横向排序、Left/Right 纵向排序、选中项保持、可写/只读 ItemsSource、取消事件、overflow 自动滚动和 close/add/extra 区域排除。 |
| 溢出与关闭 | 覆盖四控件共享 context、默认/自定义模板、四向 placement、partial overflow、disabled/selected/closable/header template、统一 owner action、外部集合失效和 stale item 拒绝；同时验证四向 directional outer shadow、透明绘制载体、viewport 阴影层裁剪边界与真实合成像素、真实 viewport 对齐、edge 可见性，以及隐藏 scrollbar 后的可滚动性和左右等距。 |
| 生命周期与性能 | 覆盖惰性首次创建、重复内容复用、100 次 open/close、旧 context、re-template、detach、DynamicResource anchor、Gallery 导航对象计数，以及分配/耗时回归门禁。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 确认没有新增 TabStrip 专属 Token；四向 overflow edge shadow 统一消费 TabControl family Token，并同步检查资源键、主题引用和 TabControl token.md。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
