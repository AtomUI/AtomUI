# Transfer 桌面版实现原理

本文档描述 Transfer 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Transfer 桌面版架构设计](overview.md)，变化记录见 [Transfer Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Transfer Token 设计](token.md)。

Popup 接入边界：`AbstractTransfer` 负责业务状态和内容准备，内部 `TransferSelectDropdown` 及其 MenuFlyout 仅作为 relay 适配层，transfer dropdown Popup 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

本文档覆盖 Transfer 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Transfer`：21 个文件，代表文件 `AbstractTransfer.cs`、`ITransferTreeView.cs`、`ITransferView.cs`、`ListTransfer.cs`、`TransferDirection.cs` 等。
- `src/AtomUI.Desktop.Controls/Transfer/Localization`：`TransferLangResourceKind.cs` 定义稳定 Catalog，`en-US.xlf`、`zh-CN.xlf`、`zh-TW.xlf` 提供内置翻译。
- `src/AtomUI.Desktop.Controls/Transfer/Themes`：12 个文件，代表文件 `AbstractTransferTheme.axaml`、`AbstractTransferTheme.cs`、`ListTransferTheme.axaml`、`TransferItemDecoratorTheme.axaml`、`TransferListItemTheme.axaml` 等。

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `AbstractTransfer`：跨平台或共享基类，承载公共 API、`TargetKeys` / `SelectedKeys` 状态归一和模板生命周期。
- `AbstractTransferTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `ListTransfer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TransferItemDecorator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TransferListItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `TransferListView`：列表面板协作类型，负责将面板级 `SelectedKeys` 投影到 `SelectedItems`，并把列表选择变化回写为 key 集合。
- `TransferRemoveItemButton`：动作触发类型，负责点击、导航或局部操作状态。
- `TransferSelectDropdown`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TransferToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `TransferTreeView`：树面板协作类型，负责将面板级 `SelectedKeys` 投影到 `CheckedItems`，并在目标 key mask 后过滤不可选节点。
- `TransferTreeViewItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `TransferTreeViewItemHeader`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TreeTransfer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TransferLangResourceKind`：稳定的本地化 Catalog enum；生成器从三个 XLIFF 文件编译资源表和 XAML 扩展。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Transfer 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Content`、`ContentTemplate`、`FilterPlaceholderText`、`FilterValueSelector`、`FooterTemplate`、`ItemTemplate`、`SelectionsIcon`、`SelectionsIconTemplate`、`SourceTitle`、`SourceTitleTemplate` 等 22 项。
- 选择与集合：`TargetKeys`、`SelectedKeys`、`Filter`、`IsAllSelected`、`IsFilterEnabled`、`PageSize`。
- 交互与状态：`IsMasked`、`IsMotionEnabled`、`IsOneWay`、`IsPaginationEnabled`、`IsShowSearch`、`IsShowSelectAll`、`IsShowSelectAllCheckbox`、`IsShowSelectDropdownMenu`、`IsStretchView`、`Status`。
- 视觉与布局：`ListHeight`、`ListWidth`、`SizeType`。
- 其他稳定入口：`Footer`、`TargetView`、`TargetViewFooter`、`ViewType`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- `TargetKeys` 是源/目标列表分割的唯一 public owner；`SelectedKeys` 是当前选择的唯一 public owner。`AbstractTransfer` 只负责按 `TargetKeys` membership 把 `SelectedKeys` 拆分到源视图和目标视图，不让视图容器反向持有业务状态。
- `TargetKeys` / `SelectedKeys` 绑定到 `INotifyCollectionChanged` 集合时，集合替换和集合原地变更都必须刷新面板数据、选中项、树 mask、按钮状态和选中计数。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- `AbstractTransfer`、`TransferListView` 和 `TransferTreeView` 的 key 集合订阅只在控件 attached 时启用；detach 时释放订阅，重新 attached 时从当前 public 集合回放状态。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_IconPresenter`：展示用户内容、文本、图标或模板化数据。

### 5.1 Semantic Part 接入

上游准入审计（本地参考源码 `/Users/chinboy/Projects/ReferenceProjects/ant-design`，6.6 稳定线，commit
`2b258157ff`，`6.6.1-22-g2b258157ff`）：Ant Design `Transfer` owner 在 `TransferSemanticType` 公开分区式
`classNames` / `styles`，键为 `root`、`section`、`header`、`title`、`body`、`list`、`item`、`itemIcon`、
`itemContent`、`footer`、`actions` 与方向作用域 `source.*` / `target.*`；`index.tsx` 经 `useMergeSemantic` 合并后
由 `Section.tsx`（`section`/`header`/`title`/`body`/`footer`）、`ListBody.tsx`（`list`）、`ListItem.tsx`
（`item`/`itemIcon`/`itemContent`）实际消费，deprecated `listStyle` / `operationStyle` 分别指向
`styles.section` / `styles.actions` —— 满足系统设计 2.1 的完整准入 Gate。上游 `TransferProps` 没有 `size` 类
Props，不存在外部尺寸档映射。

AtomUI 对应审计与映射（契约见 [Transfer Semantic Part 契约](semantic-part.md)）：

- owner 边界：`AbstractTransfer` 是抽象基类且自身 ControlTheme 不含 ControlTemplate，不声明 Part；两个具体
  public owner `ListTransfer` / `TreeTransfer` 各自持有模板，在 `ListTransfer.SemanticParts.cs` /
  `TreeTransfer.SemanticParts.cs` 声明完全相同的 Part 集合（同 ProgressBar / Badge 家族的多 owner 先例）。条目
  容器 `TransferListItem` 是独立 public owner，在 `TransferListItem.SemanticParts.cs` 声明 `itemIcon` /
  `itemContent`，与上游条目语义键对齐；上游 `item` 键由 `TransferListView` 继承 `ListView` 的 `item` 契约覆盖，
  不重复声明。
- `root` → owner 本身，隐式 Part，不生成 Style、不加 `.semantic-root`。
- `source.section` / `target.section` → owner 模板内的 internal `TransferItemDecorator` 实例
  （`SourceDecoratorView` / `TargetDecoratorView`），命名与上游 `source.section` / `target.section` 键逐字对齐
  （`.` 为层级分隔符；跨实例合并的 `section` 键因"一个模板节点只承担一个公开 Part"不发布）。`ContractType` 为
  `TemplatedControl`（装饰器的最低 public 类型）；`BorderBrush` / `BorderThickness` / `CornerRadius` 经既有
  TemplateBinding 投影到分区 `Frame`。
- `actions` → owner 模板内 `StackPanel#ActionsLayout`；上游 `actions` 的容器职责对应，两个操作按钮是 public
  `Button`（嵌套 Button 家族契约）。
- `header` / `title` / `body` / `list` / `footer` → internal `TransferItemDecorator` 共享模板内的
  `PixelAlignedBorder#HeaderFrame`、`ContentPresenter#TitleContentPresenter`、`DockPanel#BodyLayout`、
  `ContentPresenter#ContentPresenter` 与 `PixelAlignedBorder#FooterFrame`，`Multiple`（源、目标两个装饰器实例
  同时实例化）。route 统一为 `/template/ .semantic-scope-section /template/ .semantic-*`：owner 模板在两个装饰器
  实例上同时声明 `.semantic-scope-section` 路由边界（仅用于路由，不进入 Part 表），一步进入装饰器模板后同时命中
  两个实例——与 Collapse `> .semantic-scope-item /template/ .semantic-header` 的共享模板纪律一致。
- 方向限定分区部件 `source.header` / `target.header` / `source.title` / `target.title` / `source.body` /
  `target.body` / `source.list` / `target.list` / `source.footer` / `target.footer`：与未限定部件共享终端
  marker，route 中间锚点替换为 `.semantic-source` / `.semantic-target`，`Single`、`RuntimeCreated=true`。
  生成器部件名与运行时描述符均接受点分驼峰名；`ControlSemanticDescriptor` 的去重键由 selector class 放宽为
  解析路由（限定与未限定部件共享终端 class 属于合法形态）。运行时高亮解析（`SemanticPartTargetResolver`）
  按锚链匹配：候选节点到 owner 的祖先链必须携带路由中的全部中间锚点类。
- `itemIcon` / `itemContent` → `TransferListItem` 模板内 `CheckBox#SelectedIndicator` 与
  `ContentPresenter#ContentPresenter`，静态模板节点、默认 route，`Multiple` 随条目数变化；容器由
  `TransferListView` 创建路径建立继承 `.semantic-item` marker。
- 条目级 Part 与方向限定（路 A，owner 收敛）：`item` / `itemIcon` / `itemContent` 与全部 `source.*` /
  `target.*` 条目变体统一由 `ListTransfer` / `TreeTransfer` 声明（`TransferListItem` / `TransferListView` /
  `TransferTreeView` 不再注册为 Semantic owner），route 使用 `>>` 后代步进（如
  `>> .semantic-source-item /template/ .semantic-item-icon`，生成器翻译为 `.Descendant()`），生成的条目样式
  直接挂在 Transfer 作用域下即可命中。方向条目类由视图在 prepare 路径按自身 `ViewType` 幂等补挂
  （`ListTransferSemanticParts.SourceItemClass` / `TargetItemClass`、`TreeTransferSemanticParts` 同名常量）：
  容器与视图绑定后不迁移、`ViewType` 不可变，marker 保持"一次建立、不再切换"纪律。树侧条目模板无
  `.semantic-item-icon` / `.semantic-item-content` 节点，`TreeTransfer` 不声明 itemIcon / itemContent。
- 嵌套视图 owner 集成：`TransferListView` / `TransferTreeView` 的 `CreateContainerForItemOverride` 重写了基类
  容器创建路径，必须用生成常量（`ListViewSemanticParts.ItemClass` / `TreeViewSemanticParts.ItemClass`）在创建
  时一次性补回继承的 `.semantic-item` marker，并在 prepare 路径幂等补齐；Transfer 场景不使用分组数据，
  `groupHeader` 分支不需要建立。容器 prepare、restore、recycle、选择或过滤都不切换 marker。
- 默认视觉对齐（属默认模板变更，随 Gate A 一并批准）：装饰器模板 `Frame` 增加
  `Background="{TemplateBinding Background}"` 投影，使 `source` / `target` 分区级 Part 的 `Background` Setter
  有真实落点（默认值为 null，分区默认外观不变，header 保留自身 `ColorBgContainer` 背景 token）；装饰器模板
  增加 `DockPanel#BodyLayout` 主体包裹节点（Dock 顺序 header 顶、footer 底、body 填充，内部过滤输入顶、视图
  宿主填充），使 `body` Part 与上游语义键对齐且布局测量链不变；`TransferListItem` 模板为 `SelectedIndicator`
  与内容 presenter 建立条目级 marker。
- 排除范围：嵌套视图分组 / 分页 / 滚动区域（`ListView` / `TreeView` 家族契约）、`TransferTreeViewItem` 条目区域
  （`TreeViewItem` 家族契约，树侧条目指示 / 图标 / 标题已由其 `itemIndicator` / `itemIcon` / `itemTitle` 覆盖）、
  操作按钮（Button 家族）、过滤输入（LineEdit 家族）、header 内部全选 CheckBox / internal 下拉指示按钮及其运行时
  MenuFlyout、条目右缘移除按钮（上游语义键无对应分区），均不发布为 Transfer Part。跨实例合并的 `section` 键与
  树侧 `TransferTreeViewItem` 条目区域的例外见上文。

marker 纪律：所有 marker 静态声明（`Classes.semantic-*="True"`），运行期间不随 `IsOneWay`、footer 存在性、
过滤开关、分页或数据状态增删；默认主题不消费 `.semantic-*` selector；控件代码不为 Semantic Part 增加
VisualTree 扫描或实例级索引。

### 5.2 尺寸与状态基线

| 维度 | 事实 |
| --- | --- |
| 尺寸档 | owner `SizeType` 存在（`ICustomizableSizeTypeAware`，默认 `Middle`），经模板绑定转发到两个装饰器；装饰器主题不消费 `SizeType`，分区 header 高度 / 内距由 `ListTransferToken` / `TreeTransferToken` 的 `HeaderHeight` / `HeaderPadding`（`ControlHeightLG` 派生）固定。 |
| 视图档位 | `TransferListView` 是 `ListView`（SizeType-aware，创建时未显式设档，默认 `Middle`）；`TransferTreeView` 基类 `TreeView` 无 SizeType API。过滤 `LineEdit` 未绑定档位（默认 `Middle`）。 |
| 固定档位 | 操作区两个按钮与 `TransferListView` 底部分页器在模板 / 代码中固定 `SizeType=Small`。 |
| 外部映射 | 上游 `TransferProps` 无 `size` 类 API，外部尺寸映射不适用（见 5.1 上游审计）。 |
| 布局 owner | 分区宽度由 owner `ListWidth` / `IsStretchView` 模板投影到装饰器 `Width`；视图宿主高度由 `ListHeight` 投影；header 高度由内部 `HeaderHeight` 投影；`RootLayout` 列定义（Star/Auto）由 `ConfigureRootLayout` 代码维护。 |
| 状态矩阵 | `IsOneWay`（隐藏回移按钮与目标选择）、`IsStretchView`（列布局）、`IsFilterEnabled`（过滤输入可见性）、footer 存在性（footer 可见性与 body 圆角）、`Status`（分区边框状态色）、空数据、分页开关——均不改变 Part 集合、marker 或数量语义。 |
| 失败回归 | owner `SizeType` 切换 Large / Small 不改变任何测量值（当前无视觉分支的事实基线）；语义布局 Setter（如 `header` 的 `Height`、`list` 的 `Height`）与模板投影值按 Avalonia 原生优先级竞争后必须仍在 owner `ListWidth` / `ListHeight` 布局边界内收敛。 |

当前结论：owner `SizeType` 只做属性转发、不驱动任何视觉分支，本控件没有跨档位尺寸协调需求；该事实由失败回归
固化，若未来接入尺寸分支必须先更新本基线与 `semantic-part.md` 的相关承诺。

## 6. 交互与事件处理

Transfer 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 输入类路径必须保持 Form、validation、clear、placeholder 和键盘行为一致。

稳定事件路径包括 `SelectActionRequest`。事件参数和触发时机属于兼容边界。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- ItemsSource、selection、checked、expanded、filter、paging 或 upload task 的集合同步。
- Transfer 操作通过 `AddTargetKeys`、`RemoveTargetKeys` 和 `ClearTargetKeys` 更新目标 key。可写 `TargetKeys` 集合优先原地更新，避免 `ObservableCollection` 绑定源被替换；只读或 fixed-size 集合才通过 `TargetKeysProperty` 替换。
- 源面板和目标面板的选择变化统一回收到 public `SelectedKeys`，再由 `AbstractTransfer` 按 `TargetKeys` 拆分回各面板，形成一条可回放的闭环。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 Transfer 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- `TargetKeys` / `SelectedKeys` 不能与 `TransferListView.SelectedItems`、`TransferTreeView.CheckedItems` 或容器状态形成多个业务 owner。
- 对绑定集合的移动、移除和清空不能无条件替换集合实例；可写集合必须原地更新。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
- Semantic Part marker、selector class、route、`ContractType` 与 cardinality（见 [Transfer Semantic Part 契约](semantic-part.md)）；嵌套视图容器创建 / 回收路径稳定携带继承 `.semantic-item` marker；marker 不随状态增删，默认主题不消费 `.semantic-*` selector。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试；Transfer key 选择语义由 `tests/AtomUI.Desktop.Controls.Tests/Transfer/TransferBehaviorTests.cs` 覆盖。
- Semantic Part 变更运行 `tests/AtomUI.Desktop.Controls.Tests/Transfer/TransferSemanticPartTests.cs`，覆盖 descriptor 字段、模板 marker、生成 Style 命中、状态数量语义、嵌套视图容器 marker 与尺寸基线失败回归（见 5.1 / 5.2）。
- Generator Semantic 测试、`tests/AtomUI.Toolkits.GalleryBase.Tests` 与 Gallery 定向测试按 [Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md) 的验证矩阵执行。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
