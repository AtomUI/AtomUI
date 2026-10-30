# ListBox 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.ListBox` 桌面版的最新设计定位、公共契约、行为状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，公共语义区域见 [ListBox Semantic Part 契约](semantic-part.md)，内部实现原理见 [ListBox 桌面版实现原理](implementation.md)，ListBox Token 的专项设计见 [ListBox Token 设计](token.md)，设计和契约变化记录见 [ListBox Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/List` |
| 控件状态 | Stable |

ListBox 是桌面端数据展示类轻量列表选择控件，用于展示一组纵向排列的简单选项、候选项、搜索结果或可点击条目。它以 Avalonia `ListBox` 为基础，扩展 AtomUI 的尺寸、选中指示器、空状态、文本过滤高亮、Token 和 CandidateList 复用能力。

ListBox 的职责是管理列表容器生成、单选或多选、键盘导航、条目点击、空状态展示、文本过滤展示和虚拟化容器状态回放。它不负责分组、排序、分页、复杂数据视图刷新、远程搜索请求、树形层级、表格列模型或业务命令编排；这些场景应使用 `ListView`、TreeView、DataGrid 或组合控件。

ListBox 支持两种条目提供方式：

- 直接放置 `ListBoxItem`。
- 通过 `ItemsSource` 绑定数据项，并由 ListBox 生成 `ListBoxItem` 容器。

直接子元素适合静态少量条目；`ItemsSource` 适合数据驱动列表、候选列表和需要模板复用的场景。

## 2. 设计语言

ListBox 表达的是“紧凑纵向条目 + 当前选择目标”的信息浏览语义。用户应能通过行距、hover 背景、selected 背景、可选选中标记、禁用状态和过滤高亮理解当前可操作条目。

设计语言由以下维度组成：

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 列表结构 | 一组同级条目按纵向排列。 | ScrollViewer、ItemsPresenter、条目间距。 |
| 选择状态 | 当前业务选择目标。 | selected 背景、`SelectedItem` / `SelectedItems`、选中指示器。 |
| 交互状态 | 鼠标、触摸、键盘导致的临时反馈。 | pointerover 背景、键盘移动选择。 |
| 禁用状态 | 条目或列表不可交互。 | disabled 文本色、禁用选择更新。 |
| 过滤状态 | 搜索或过滤命中的文本。 | 高亮文本、可选隐藏未命中项、过滤空状态。 |
| 空状态 | 无数据或过滤无结果。 | `EmptyIndicator` 内容区域。 |

ListBox 的视觉强度应保持克制。条目是可扫描的信息行，不应被绘制成卡片；选中指示器是补充信号，不应替代 selected 背景和文本状态。

## 3. API 与契约模型

ListBox 的公共契约由 ListBox API、ListBoxItem API、事件 API、template part 和主题入口组成。

ListBox 核心 API：

| API | 语义 |
| --- | --- |
| `IsSelectable` | 是否允许用户交互更新选择。关闭时清空当前选择。 |
| `SelectionMode` / `SelectedIndex` / `SelectedItem` / `SelectedItems` / `Selection` | 继承 Avalonia `ListBox` 的选择模型。 |
| `SizeType` | 控制整体圆角、空状态 padding 和条目高度、padding。 |
| `IsBorderless` | 是否隐藏 root 边框。 |
| `ItemHoverBg` / `ItemSelectedBg` | 条目 hover 和 selected 背景入口。 |
| `IsShowSelectedIndicator` | 是否在选中条目右侧显示选中标记。 |
| `SelectedIndicator` | 选中标记图标模板入口。 |
| `IsMotionEnabled` | 是否启用条目背景和前景过渡动效。 |
| `EmptyIndicator` / `EmptyIndicatorTemplate` / `EmptyIndicatorPadding` / `IsShowEmptyIndicator` | 空状态展示入口。 |
| `Filter` / `FilterValue` / `FilterValueSelector` | 文本过滤入口。 |
| `FilterHighlightStrategy` | 过滤文本高亮和隐藏未命中项的策略入口。 |
| `FilterResultCount` | 当前过滤命中数量。 |
| `IsFiltering` | 当前是否处于过滤状态。 |
| `FilterHighlightForeground` | 过滤高亮前景色入口。 |

ListBoxItem 容器 API：

| API | 语义 |
| --- | --- |
| `Clicked` | 条目 pointer press 触发的 routed event。 |
| `IsSelected` | 继承 Avalonia `ListBoxItem` 的选择状态。 |
| `Content` / `ContentTemplate` | 条目内容和模板。 |

事件 API：

| 事件 | 语义 |
| --- | --- |
| `ItemClicked` | ListBoxItem `Clicked` 冒泡到 ListBox 后触发，携带被点击容器。 |
| `ItemCountChanged` | `ItemCount` 变化后触发。 |

稳定 template part：

| Template Part | 所属控件 | 职责 |
| --- | --- | --- |
| `Frame` | `ListBox` | root 背景、边框、圆角、padding 与外框闭合边界。 |
| `PART_ScrollViewer` | `ListBox` | 列表滚动容器。 |
| `ItemsPresenter` | `ListBox` | 条目容器承载入口。 |
| `EmptyIndicator` | `ListBox` | 空状态内容展示。 |
| `Frame` | `ListBoxItem` | 条目背景与 padding 边界。 |
| `SplitLineFrame` | `ListBoxItem` | 条目底部分割线表面。 |
| `SelectedIndicator` | `ListBoxItem` | 选中标记展示入口。 |
| `ContentPresenter` | `ListBoxItem` | 普通内容展示入口。 |

ListBoxItem 内部还包含过滤文本高亮节点。该节点属于过滤展示实现，不作为用户替换模板时的 public template part。

伪类契约主要继承 Avalonia `ListBoxItem`：

- `:pointerover` 表示条目 hover。
- `:selected` 表示条目被选中。
- `:disabled` 表示条目不可用。

## 4. 行为与状态模型

ListBox 的状态模型由选择状态、交互状态、过滤状态、空状态、尺寸状态和动效状态组成。

选择行为：

- `IsSelectable=false` 时，ListBox 不响应 pointer 或 keyboard 选择更新，并清空 `SelectedIndex`、`SelectedItem` 和 `SelectedItems`。
- `SelectionMode` 继承 Avalonia `ListBox` 语义，单选使用 `SelectedItem`，多选使用 `SelectedItems`。
- 方向键按 Avalonia navigation direction 移动选择，`Shift` 支持范围选择，平台 select-all 手势在多选模式下选择全部。
- `IsShowSelectedIndicator=true` 时，选中容器右侧显示 `SelectedIndicator`。

点击行为：

- `ListBoxItem.Clicked` 是 routed event，ListBox 通过 class handler 收敛到 `ItemClicked`。
- 派生控件可重写 `NotifyListBoxItemClicked` 承接点击行为，例如 CandidateList 在单选场景中提交候选项。

过滤行为：

- `Filter`、`FilterValue` 和 `FilterValueSelector` 共同决定条目是否命中。
- `IsFiltering=true` 时条目进入过滤展示状态，并通过 `FilterHighlightStrategy` 决定高亮和隐藏未命中项。
- `FilterResultCount` 表示命中数量；过滤模式下空状态依据 `FilterResultCount == 0` 判断。
- 过滤展示主要面向文本内容。复杂自定义 `ItemTemplate` 需要维护者明确处理过滤态展示入口，避免过滤态绕过用户模板。

空状态行为：

- `ItemCount == 0` 时显示空状态。
- `IsFiltering && FilterResultCount == 0` 时显示过滤空状态。
- `IsShowEmptyIndicator=false` 时不显示空状态内容。

动效行为：

- ListBoxItem 初始化阶段禁用 transitions，loaded 后启用，避免初始状态产生非预期动画。
- `IsMotionEnabled=false` 时不应用背景和前景过渡。

## 5. 视觉与主题模型

ListBox 主题按 root 和 item 两层组织。

```text
ListBoxTheme
  Frame（ClipContentToCornerRadius=True）
  PART_ScrollViewer
  ItemsPresenter
  EmptyIndicator

ListBoxItemTheme
  Frame
  SelectedIndicator
  ContentPresenter
  HighlightableTextBlock
  SplitLineFrame
```

视觉规则：

- root 外框由 `BorderBrush`、`BorderThickness`、`CornerRadius` 和 `IsBorderless` 共同决定。默认外框颜色是 `ColorSplit`，与条目分割线同色，表达“外框即列表闭合线”。
- root `Frame` 开启 `ClipContentToCornerRadius`：内容被裁剪到外框圆角内边缘，条目 hover / selected 背景等溢出圆角内边缘的内容被裁剪，不在圆角口袋区溢出；外框环由 `Frame` 自身一次绘制（`ColorSplit`），无叠加节点。
- `SizeType` 控制 root 圆角、空状态 padding、条目最小高度和条目 padding。条目表面保持直角，主题不设置条目圆角。
- 条目直接贴合 root 外框内边缘：`ContentPadding` 与 `ItemMargin` 均为 `Thickness(0)`，条目之间不留垂直间距，列表紧凑感由条目高度与分割线表达。
- 条目底部分割线默认是 1 DIP `ColorSplit` 底边线，由条目 `BorderThickness` / `BorderBrush` 驱动。最后一项的底部分割线被抑制，由 root 外框下边缘承担闭合线；`IsBorderless` 时保留（外框消失后由分割线承担闭合线）。
- 默认条目背景透明，hover 使用 `ItemHoverBg`，selected 使用 `ItemSelectedBg`；hover / selected 背景延伸到外框内边缘，溢出圆角口袋区的部分由 `Frame` 的圆角内容裁剪约束。
- disabled 内容使用 SharedToken disabled 文本色。
- 选中指示器默认使用 默认 `CheckOutlined`，颜色使用 SharedToken 主色，尺寸使用 SharedToken icon size。
- 空状态默认使用 `Empty` 的 simple preset image。

ListBoxToken 提供内容 padding、条目文字颜色、条目状态背景、条目 padding、条目 margin、选中指示器 margin 和过滤高亮色。Token 详情见 [ListBox Token 设计](token.md)。

## 6. 控件家族或集成关系

ListBox 属于 Data Display 分类，与 ListView、TreeView、DataGrid、Card、Descriptions 等控件共同服务结构化数据展示。

集成关系：

- Avalonia `ListBox`：提供 ItemsControl、Selection、默认虚拟化面板、键盘导航和基础容器模型。
- `ListBoxItem`：条目容器，承载选择状态、点击事件、选中指示器和过滤文本展示状态。
- `IListItemData`：默认数据项契约，默认 ItemTemplate 读取 `Content`。
- `CandidateList`：继承 ListBox，作为 AutoComplete、Mentions 等候选项列表的基础控件。
- `CascaderViewFilterList`：继承 ListBox，用于 Cascader 过滤结果列表。
- Motion：条目背景和前景状态过渡遵守 `IsMotionEnabled`。

ListBox 不实现 ListView 的分组、排序、分页和 collection view 管理；Select 的候选列表使用 `SelectCandidateList`，它基于 ListView 而不是 ListBox。

## 7. 兼容性不变量

维护 ListBox 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 ListBox / ListBoxItem public API、事件和 Avalonia 属性语义。
- 不破坏 Avalonia `ListBox` 的 `SelectionMode`、`SelectedItem`、`SelectedItems`、`Selection` 和 keyboard navigation 语义。
- `IsSelectable=false` 必须阻止用户选择更新，并清空当前选择。
- `ItemClicked` 必须继续从 ListBoxItem routed event 收敛，保持 CandidateList 的提交入口稳定。
- `PART_ScrollViewer`、`ItemsPresenter`、`EmptyIndicator`、`SelectedIndicator` 和 `ContentPresenter` 的职责不得被无兼容说明地改变。
- 选中指示器、过滤高亮和空状态节点应留在 AXAML 静态模板中，通过 `IsVisible` 和状态属性控制，不作为普通性能优化迁移到 C# 动态创建。
- 虚拟化容器回收时，容器本地值必须对称清理，避免旧 item 的 disabled、filter、indicator 或 content 状态泄漏到新 item。
- 筛选命中数量和空状态契约不得只依赖当前已实现容器。
- 语义区域边界：`ListBox` 只发布 `root` / `item` 两个 Part；`item` 的 marker 在容器创建与 prepare 路径一次性幂等
  建立、不随状态切换，静态模板不增删 marker，默认主题不消费 `.semantic-*` selector；selection、filter 高亮与
  empty 不属于 Part。
- root 外框与条目分割线必须保持同一 `ColorSplit` 基线；root `Frame` 的 `ClipContentToCornerRadius` 圆角内容裁剪保证条目状态背景不溢出圆角内边缘。
- 条目表面保持直角与贴边：不恢复条目圆角，不重新引入 `ContentPadding` / `ItemMargin` 垂直间距。
- 最后一项分割线抑制规则由控件状态机决定：最后一项不显示底部分割线，由 root 外框下边缘闭合；`IsBorderless` 时
  保留；集合追加与删除后必须重新同步所有已实现容器，Semantic Style 不能绕过该规则。
- Token 只表达组件设计语义，不承载选择、过滤、空状态或虚拟化运行时状态。

## 8. 专项模型

### 8.1 轻量过滤模型

ListBox 的过滤模型面向简单文本列表和候选项列表。`FilterValueSelector` 优先从 item 数据中提取过滤值；未提供 selector 时，`IListItemData.Content` 和字符串内容是默认文本来源。

`FilterHighlightStrategy` 控制过滤文本展示策略。使用隐藏未命中项策略时，维护者必须保持原可见性恢复能力，并确保过滤清除后不留下本地 `IsVisible` 状态。

### 8.2 选中指示器模型

`IsShowSelectedIndicator` 是 ListBox 级开关，ListBoxItem 根据 `IsSelected && IsShowSelectedIndicator` 计算 `IsSelectedIndicatorVisible`。选中指示器模板由 `SelectedIndicator` 提供，默认是 check icon。

该模型只表达视觉辅助，不改变 selection model，不影响 `SelectedItem` / `SelectedItems` 的数据语义。

### 8.3 CandidateList 基座模型

CandidateList 继承 ListBox，并增加候选项键盘导航、候选高亮、commit / cancel 和最大选择数量控制。ListBox 的点击、选择、过滤、空状态和虚拟化上下文规则直接影响 CandidateList，因此 ListBox 变更必须走 CandidateList 验证。

## 9. 文档导航与验证策略

文档导航：

- [ListBox Semantic Part 契约](semantic-part.md)
- [ListBox 桌面版实现原理](implementation.md)
- [ListBox Token 设计](token.md)
- [ListBox Changelog](changelog.md)

验证策略：

| 分层 | 验证要求 |
| --- | --- |
| Public API | 检查 ListBox / ListBoxItem 属性、事件、默认值和 Avalonia selection 语义不变。 |
| 状态 | 覆盖 selectable、selected indicator、filter、empty、disabled、keyboard navigation 和 CandidateList commit。 |
| AXAML | 检查 root、item、empty、indicator、filter highlighter 在 light / dark 和三种 SizeType 下显示稳定，外框 / 分割线 / 圆角内容裁剪规则一致。 |
| Token | 检查 ListBoxTokenKind、AXAML token resource 和 token.md 语义说明保持一致。 |
| 虚拟化 | 覆盖 container prepare / clear、上下滚动后状态不串扰。 |
| 文档 | 运行 `git diff --check`，确认链接存在且只记录最新设计状态。 |
