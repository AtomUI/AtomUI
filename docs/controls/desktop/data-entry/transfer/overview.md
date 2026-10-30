# Transfer 桌面版架构设计

本文档定义 `Transfer` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Transfer 桌面版实现原理](implementation.md)，Transfer Token 的专项设计见 [Transfer Token 设计](token.md)，设计和契约变化记录见 [Transfer Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `AbstractTransfer`，其 internal `IsPopupPinnedOpen` 只供测试和内部诊断使用；设置为 true 时保持 dropdown open state，并 relay 到内部 `TransferSelectDropdown` 的 MenuFlyout，设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/Transfer` |
| 控件状态 | Stable |

Transfer 是 AtomUI 桌面控件体系中的穿梭框控件，用于在源列表和目标列表之间移动、搜索和选择数据项。

Transfer 不负责表格编辑器、树形穿梭或远程数据同步协议。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Transfer`

## 2. 设计语言

Transfer 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Transfer 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Transfer 是 AtomUI 桌面控件体系中的穿梭框控件，用于在源列表和目标列表之间移动、搜索和选择数据项。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Content`、`ContentTemplate`、`FilterPlaceholderText`、`FilterValueSelector`、`FooterTemplate`、`ItemTemplate`、`SelectionsIcon`、`SelectionsIconTemplate` 等 22 项。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | selection/checked/active、collection/filter、input/value、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Transfer Token + ControlTheme。 |

## 3. API 与契约模型

Transfer 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Content`、`ContentTemplate`、`FilterPlaceholderText`、`FilterValueSelector`、`FooterTemplate`、`ItemTemplate`、`SelectionsIcon`、`SelectionsIconTemplate`、`SourceTitle`、`SourceTitleTemplate` 等 22 项 | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 选择与集合 | `TargetKeys`、`SelectedKeys`、`Filter`、`IsAllSelected`、`IsFilterEnabled`、`PageSize` | 维护目标集合、当前面板选择、过滤、分页和集合状态。 |
| 交互与状态 | `IsMasked`、`IsMotionEnabled`、`IsOneWay`、`IsPaginationEnabled`、`IsShowSearch`、`IsShowSelectAll`、`IsShowSelectAllCheckbox`、`IsShowSelectDropdownMenu`、`IsStretchView`、`Status` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `ListHeight`、`ListWidth`、`SizeType` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 其他稳定入口 | `Footer`、`TargetView`、`TargetViewFooter`、`ViewType` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

`TargetKeys` 表示已经移动到目标面板的条目 key，是 Transfer 的提交值；`SelectedKeys` 表示当前源面板和目标面板内被选中的条目 key。两者默认 `BindingMode.TwoWay`，并支持 `INotifyCollectionChanged` 集合的原地 `Add`、`Remove`、`Replace`、`Move` 和 `Reset`。当绑定集合可写时，Transfer 交互优先原地更新已有集合，避免替换绑定源造成外部状态不同步。

稳定事件包括 `SelectActionRequest`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`AbstractTransfer`、`ListTransfer`、`TransferItemDecorator`、`TransferItemsRemovedEventArgs`、`TransferListItem`、`TransferListView`、`TransferRemoveItemButton`、`TransferSelectActionEventArgs`、`TransferSelectDropdown`、`TransferSelectionChangedEventArgs`、`TransferTreeView`、`TransferTreeViewItem`、`TransferTreeViewItemHeader`、`TransferViewCreatedEventArgs` 等 18 项。
- 枚举：`TransferDirection`、`TransferSelectAction`、`TransferViewType`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_IconPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

## 4. 行为与状态模型

Transfer 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- selection/checked/active、collection/filter、input/value、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- `TargetKeys` 是目标集合的 public owner，源/目标面板数据由 `ItemsSource` 与 `TargetKeys` 推导；`SelectedKeys` 是当前选择的 public owner，内部源面板选择和目标面板选择按 key 是否存在于 `TargetKeys` 自动拆分。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

Transfer 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `AbstractTransferTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `ListTransferTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `TransferItemDecoratorTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `TransferListItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `TransferListViewTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `TransferSelectDropdownTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `TransferTreeViewItemHeaderTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `TransferTreeViewItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `TransferTreeViewTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `TreeTransferTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

Transfer 使用 `TransferToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 selection/checked/active、collection/filter、input/value、motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Transfer 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `AbstractTransfer`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractTransferTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `ListTransfer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TransferItemDecorator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TransferListItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `TransferListView`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TransferRemoveItemButton`：动作触发类型，负责点击、导航或局部操作状态。
- `TransferSelectDropdown`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TransferToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `TransferTreeView`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TransferTreeViewItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `TransferTreeViewItemHeader`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TreeTransfer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TransferLangResourceKind`：稳定的本地化 Catalog enum；内置翻译由同目录三种语言 XLIFF 提供并在编译期生成。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Transfer 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 选择与当前项模型

Transfer 的当前项状态必须由单一 owner 推导。`SelectedKeys` 保存跨源面板和目标面板的当前选中 key 集合，内部 `TransferListView.SelectedKeys` / `TransferTreeView.SelectedKeys` 只承载面板级投影，不另建业务选择状态。集合替换、清空、原地变更和模板重套用时必须从 public key 集合回放到列表选中项、树勾选项和计数状态。

### 8.2 集合与数据同步模型

Transfer 的集合状态必须能处理 source replace、reset、clear 和 container recycle。`TargetKeys` 是目标面板数据的稳定来源，Transfer 按 `ItemsSource` membership 生成源面板和目标面板集合；对可写绑定集合执行移动、移除和清空时优先原地变更，只有 `null`、只读或 fixed-size 集合才回退为属性替换。业务数据对象不应反向持有视觉对象，虚拟化或懒创建路径必须在容器回收时清理旧状态。

### 8.3 动效模型

Transfer 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.4 视觉选项模型

Transfer 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

### 8.5 Semantic Part

`ListTransfer` 与 `TreeTransfer` 公开 `root`、`source.section`、`target.section`、`actions`、`header`、`title`、
`body`、`list`、`footer` 九个职责区域及十个方向限定变体（`source.header` / `target.header` / `source.title` /
`target.title` / `source.body` / `target.body` / `source.list` / `target.list` / `source.footer` /
`target.footer`），与上游 Transfer 语义键逐字对齐（`.` 为层级分隔符）；两个 owner 发布相同 Part 集合，
`source.section` / `target.section` /
`actions` 为 `Single`，`header` / `title` / `body` / `list` / `footer` 位于共享分区模板内为 `Multiple`（源、目标
各一）。条目容器 `TransferListItem` 自身发布 `itemIcon` / `itemContent`，上游 `item` 键由 `TransferListView`
继承 `ListView` 的 `item` 契约覆盖。完整 selector route、`ContractType`、状态矩阵和定制边界见
[Transfer Semantic Part 契约](semantic-part.md)。

操作按钮属于 Button 家族、过滤输入属于 LineEdit 家族，均不由 Transfer Part 承担；上游的方向作用域内层区域
方向差异化由分区级与条目级限定 Part 直接承担：分区内部件经 `.semantic-source` / `.semantic-target` 方向锚点
路由限定，条目级（`source.item` / `target.item` / `source.itemIcon` 等）由视图 `ViewType` 在 prepare 路径一次性
补挂方向条目类区分。

## 9. 文档导航与验证策略

关联文档：

- [Transfer 桌面版实现原理](implementation.md)
- [Transfer Semantic Part 契约](semantic-part.md)
- [Transfer Token 设计](token.md)
- [Transfer Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、默认绑定模式、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 `TargetKeys` / `SelectedKeys` 替换和集合原地变更、selection/checked/active、collection/filter、input/value、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
