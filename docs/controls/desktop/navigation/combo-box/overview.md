# ComboBox 桌面版架构设计

本文档定义 `ComboBox` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。输入表面共享状态见 [输入控件共享架构设计](../../data-entry/input-control-architecture-design.md)。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，候选列表统一交互见 [候选列表统一交互设计](../../data-entry/select/candidate-interaction-design.md)，内部实现原理见 [ComboBox 桌面版实现原理](implementation.md)，ComboBox Token 的专项设计见 [ComboBox Token 设计](token.md)，设计和契约变化记录见 [ComboBox Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `ComboBox`，其 `IsPopupPinnedOpen` 为 public（与 `Select` / `AutoComplete` / `Mentions` 等同族控件一致），设置为 true 时在弹层打开前抑制 light-dismiss 并保持 open state（relay 到 template Popup），设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

ComboBox 公开 12 个 Semantic Part：隐式 `root` 加 11 个非 root 部件（`prefix`、`frame`、`content`、`placeholder`、`input`、`suffix`、`indicator`、`popup.root`、`popup.list`、`popup.listItem`、`popup.empty`）。区域分组参考 Ant Design Select 已公开的 Semantic DOM 词汇，只发布 ComboBox 自身确实拥有的区域；完整 Part 表、Selector 用法与定制边界见 [ComboBox Semantic Part 契约](semantic-part.md)。

> **准入定性：** ComboBox 的 Semantic Part 纳入**不是 §2.1 准入 Gate 通过**。上游 6.6.0 没有公开 `ComboBox` owner，原排除判定中「`Select` 的 internal combobox mode 不能作为公开 owner」继续有效；本次依据是用户指令加上 ComboBox 自身即职责完整的独立 public owner。范围记录见[全量改造设计 §2.4](../../../../superpowers/specs/2026-08-12-semantic-part-control-rollout-design.md) 的 2026-09-16 范围变更段。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Navigation/ComboBox` |
| 控件状态 | Stable |

ComboBox 是 AtomUI 桌面控件体系中的组合框控件，用于在可输入文本框和候选列表之间完成选择。

ComboBox 不负责远程自动完成、树形选择或多列数据选择。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/ComboBox`

## 2. 设计语言

ComboBox 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | ComboBox 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | ComboBox 是 AtomUI 桌面控件体系中的组合框控件，用于在可输入文本框和候选列表之间完成选择。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `ContentLeftAddOn`、`ContentLeftAddOnTemplate`、`ContentRightAddOn`、`ContentRightAddOnTemplate`、`FilterValue`、`FilterValueSelector`、`LeftAddOnTemplate`、`OptionFontSize` 等 9 项。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | open/close、collection/filter、input/value、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | ComboBox Token + ControlTheme。 |

## 3. API 与契约模型

ComboBox 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `ContentLeftAddOn`、`ContentLeftAddOnTemplate`、`ContentRightAddOn`、`ContentRightAddOnTemplate`、`FilterValue`、`FilterValueSelector`、`LeftAddOnTemplate`、`OptionFontSize`、`RightAddOnTemplate` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 选择与集合 | `SelectedItem`、`SelectedIndex`、`DropDownDisplayPageSize`、`Filter`、`IsFilterEnabled` | 维护选择、展开、过滤、分页、分组或集合状态。 |
| 交互与状态 | `IsAllowClear`、`IsMotionEnabled`、`ShouldUseOverlayPopup`、`IsPopupPinnedOpen`、`Status`、`IsShowOverflowTip`、`OverflowTipDelay`、`OverflowTipPlacement` | 表达用户可观察状态、可用性、清除、加载、反馈和非编辑态选中内容溢出提示语义。Form 校验扩展状态进入内部 `FormStatus`，不覆盖显式 `Status`。 |
| 视觉与布局 | `SizeType`、`StyleVariant` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 其他稳定入口 | `LeftAddOn`、`RightAddOn` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

当前没有抽取到控件专属 public 事件；交互通知主要来自继承事件、命令或 Gallery 可观察状态。

主要公开类型与枚举：

- 类型：`ComboBox`、`ComboBoxItem`。`ComboBoxHandle`、`ComboBoxTextBox`、`ComboBoxToken` 为 internal 协作类型，不是公共契约。
- 枚举：无。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_ComboBoxHandle` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_ContentRightAddOnPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_EditableTextBox` | `?` | 承载文本输入、过滤、显示或编辑入口。 |
| `PART_EmptyIndicator` | `?` | 展示指示器、进度、分页或状态反馈。 |
| `PART_FormFeedBack` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_ItemsPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_OpenIndicatorButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_Popup` | `?` | 承载弹层宿主、打开关闭或候选内容。 |
| `PART_TextPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

### 3.1 Semantic Parts

ComboBox 的公共 Semantic Part 契约如下。契约字段（Selector、ContractType、Cardinality、CrossVisualRoot、RuntimeCreated、CrossNestedOwners）的完整定义与每个部件的数据以 [ComboBox Semantic Part 契约](semantic-part.md) 为唯一真源，本节只做摘要。

| Part | 类型 | 职责 |
| --- | --- | --- |
| `root` | `ComboBox` | 数据源、选择、过滤、弹层与输入框状态的组织边界；不生成 Style。 |
| `prefix` | `ContentPresenter` | 内容框内联前缀区，承载 `ContentLeftAddOn`。 |
| `frame` | `PixelAlignedBorder` | 输入框边框盒，决定边框颜色、宽度、圆角与背景（最常用定制点）。 |
| `content` | `Panel` | 输入内容面板，承载占位符、非编辑态选中内容与编辑态输入框。 |
| `placeholder` | `Avalonia.Controls.TextBlock` | 未选择且非编辑态时的占位符文本。 |
| `input` | `Avalonia.Controls.TextBox` | `IsEditable=true` 时的编辑 / 过滤输入框。 |
| `suffix` | `StackPanel` | 右侧后缀区，承载用户后缀内容、Form 反馈与下拉指示器。 |
| `indicator` | `IconButton` | 下拉展开指示器（箭头按钮），位于 `ComboBoxHandle` 自有模板内。 |
| `popup.root` | `Border` | 弹层框体，承载候选列表与空态。 |
| `popup.list` | `Avalonia.Controls.ScrollViewer` | 候选列表滚动区，与空态互斥。 |
| `popup.listItem` | `ComboBoxItem` | 单个候选项容器，运行时容器创建时注入 marker。 |
| `popup.empty` | `Border` | 生效过滤模式下无匹配项时的弹层空态区（未开启过滤时不显示）。 |

**明确不发布的区域**：清除部件（`frame` 的 hover / focus / 校验态状态色由共享主题驱动，不作为独立部件发布）（`IsAllowClear` 为未实现 API）、非编辑态选中内容节点、Form 反馈节点、内置 AddOn 区、`ComboBoxItem` 作为独立 owner、`atom:Empty` 内部视觉、弹层宿主与定位。逐项理由见 [ComboBox Semantic Part 契约 §5](semantic-part.md)。

**定制入口**：应用使用生成的语义专用 Style（`ComboBoxFrameStyle`、`ComboBoxPrefixStyle`、`ComboBoxIndicatorStyle`、`ComboBoxPopupRootStyle` 等，命名空间 `AtomUI.Theme.Styling`，AXAML 命名空间 `https://atomui.net`）在 AXAML 中声明式定制。禁止在 code-behind 中获取节点后直接设置 `Background` / `Foreground` / `Padding` / `CornerRadius` 等属性作为定制手段。

## 4. 行为与状态模型

ComboBox 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- `DataValidationErrors` 是 native error 唯一真源；`FormStatus` 承载 Form 的 warning、success、validating 和 error 投影，`Status` 只表示用户显式请求。输入表面通过 `InputControlState.ResolveEffectiveStatus` 计算有效状态，native/Form error 优先于 Form warning，再优先于显式 warning/error。
- open/close、collection/filter、input/value、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 单选结果以继承的 `SelectedItem` / `SelectedIndex` 为准；AtomUI Form 集成使用 `SelectedItem` 作为 ComboBox 的默认表单值，`SetFormValue`、`GetFormValue` 和 `ClearFormValue` 不应转换为字符串或读取展示文本。
- 下拉项的 active candidate 与 `SelectedItem` / `SelectedIndex` 分离。鼠标移动和键盘 `Up` / `Down` 共享同一个 active candidate；迁移只改变候选视觉，`Enter` 才提交 `SelectedItem`。`:pointerover` 不得形成第二个候选高亮，selected 视觉优先于 active 视觉。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

ComboBox 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `ComboBoxHandleTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `ComboBoxItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `ComboBoxTheme.axaml` | 定义布局、内容承载或框架节点视觉。 |

非编辑态选中内容的完整文本提示复用共享 `OverflowTip` attached behavior。模板只在 `SelectedContentPresenter` 上接入 `IsShowOverflowTip`、`OverflowTipDelay`、`OverflowTipPlacement` 和 `SelectionBoxItem`；`IsEditable=true` 时编辑输入框不默认启用该提示。

ComboBox 使用 `ComboBoxToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 open/close、collection/filter、input/value、motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

ComboBox 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `ComboBox`：模板协作类型，承载内容展示、宿主或视觉边界。
- `ComboBoxHandle`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ComboBoxItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `ComboBoxToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 ComboBox 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 弹层与宿主模型

ComboBox 涉及弹层、窗口或 overlay 宿主时，打开状态、取消事件、定位和宿主释放必须保持一致。重复打开、关闭、窗口失活和 template reapply 都必须释放旧宿主引用。

ComboBox 的 active candidate 归属于当前 popup 的候选视图。popup 关闭、ItemsSource / filter 重建或容器回收时清除失效候选；重新打开后从当前视图重新建立候选，不继承旧容器的 active 投影。

### 8.2 集合与数据同步模型

ComboBox 的集合状态必须能处理 source replace、reset、clear 和 container recycle。业务数据对象不应反向持有视觉对象，虚拟化或懒创建路径必须在容器回收时清理旧状态。

### 8.3 Form 值模型

ComboBox 实现 `IFormItemAware` 时以 `SelectedItem` 作为表单值 owner：

- `SetFormValue(value)` 设置 `SelectedItem`，保留业务对象实例。
- `GetFormValue()` 返回当前 `SelectedItem`。
- `ClearFormValue()` 清空 `SelectedItem`，并回到未选择状态。
- `SelectionBoxItem` 只用于展示，不作为 Form 真实值，也不能把对象值降级为字符串。

### 8.4 动效模型

ComboBox 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.5 视觉选项模型

ComboBox 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [ComboBox 桌面版实现原理](implementation.md)
- [ComboBox Semantic Part 契约](semantic-part.md)
- [ComboBox Token 设计](token.md)
- [ComboBox Changelog](changelog.md)

改输入框边框颜色有**两个入口**，按场景选择：直接设置 `ComboBox.BorderBrush` / `Background` 是输入族标准做法（控件把这两个根表面画刷以 `LocalValue` 中继到共享输入帧，压过帧的 hover / focus / 校验状态机）；需要按语义节点精确控制边框宽度、圆角、背景或状态色时，用 `frame` 部件（`ComboBoxFrameStyle`）。两者落在同一帧节点上，`LocalValue` 优先级高于 `Style`，同时使用时根中继胜出。细节见
[ComboBox Semantic Part 契约](semantic-part.md) §5 与 [ComboBox 实现](implementation.md) §5.2。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 open/close、collection/filter、input/value、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Semantic Part | 检查 descriptor 声明与 `semantic-part.md` 一致、静态 marker 与运行时 marker 就位、生成专用 Style 恰好命中目标节点，并确认不存在 code-behind 属性回退。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
