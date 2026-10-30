# AutoComplete 桌面版架构设计

本文档定义 `AutoComplete` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，候选列表统一交互见 [候选列表统一交互设计](../select/candidate-interaction-design.md)，内部实现原理见 [AutoComplete 桌面版实现原理](implementation.md)，AutoComplete Token 的专项设计见 [AutoComplete Token 设计](token.md)，设计和契约变化记录见 [AutoComplete Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `AbstractAutoComplete`，其 internal `IsPopupPinnedOpen` 只供测试和内部诊断使用；设置为 true 时保持 candidate open state 并 relay 到 candidate Popup，设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/AutoComplete` |
| 控件状态 | Stable |

AutoComplete 是 AtomUI 桌面控件体系中的自动完成输入控件，用于把文本输入、候选弹层、过滤和提交事件组合在一起。

AutoComplete 不负责普通 Select、远程搜索协议或业务结果列表。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/AutoComplete`

## 2. 设计语言

AutoComplete 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | AutoComplete 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | AutoComplete 是 AtomUI 桌面控件体系中的自动完成输入控件，用于把文本输入、候选弹层、过滤和提交事件组合在一起。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `ClearIcon`、`ContentLeftAddOn`、`ContentLeftAddOnTemplate`、`ContentRightAddOn`、`ContentRightAddOnTemplate`、`DefaultValue`、`FilterValue`、`FilterValueSelector` 等 14 项。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | selection/checked/active、open/close、loading/async、collection/filter、input/value、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | AutoComplete Token + ControlTheme。 |

## 3. API 与契约模型

AutoComplete 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `ClearIcon`、`ContentLeftAddOn`、`ContentLeftAddOnTemplate`、`ContentRightAddOn`、`ContentRightAddOnTemplate`、`DefaultValue`、`FilterValue`、`FilterValueSelector`、`OptionTemplate`、`OptionsAsyncLoader` 等 14 项 | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 选择与集合 | `CaretIndex`、`ClearSelectionOnLostFocus`、`DisplayCandidateCount`、`Filter`、`IsShowCount` | 维护选择、展开、过滤、分页、分组或集合状态。 |
| 交互与状态 | `IsAllowClear`、`IsAutoFocus`、`IsAutoSize`、`IsCompletionEnabled`、`IsDropDownOpen`、`IsLoading`、`IsMotionEnabled`、`IsOperating`、`IsSearchOnEnterEnabled`、`IsPopupMatchSelectWidth`、`IsReadOnly` 等 14 项 | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `MaxDropDownHeight`、`PlaceholderForeground`、`Placement`、`SearchButtonStyle`、`SizeType`、`StyleVariant` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 动效与异步 | `AsyncLoadDebounce`、`AsyncLoadTimeout` | 约束动效开关、异步加载、播放速度、超时和任务边界。 |
| 其他稳定入口 | `Lines`、`MaxLength`、`MinimumPrefixLength` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

稳定事件包括 `SelectionChanged`、`ValueChanged`；`AutoCompleteSearchEdit` 还通过内部 SearchEdit box 转发 `SearchRequested`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`AbstractAutoComplete`、`AutoComplete`、`AutoCompleteLineEditBox`、`AutoCompleteSearchEdit`、`AutoCompleteSearchEditBox`、`AutoCompleteTextArea`、`AutoCompleteTextAreaBox`、`CompactSpaceAwareAutoComplete`、`CompleteOptionsLoadResult`、`CompleteOptionsLoadedEventArgs`、`CompletePopulatedEventArgs`、`CompletePopulatingEventArgs`、`CompleteValueChangedEventArgs`。
- 枚举：`AutoCompletePlacementMode`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_Popup` | `?` | 承载弹层宿主、打开关闭或候选内容。 |

控件专属或内部伪类包括 `AutoCompletePseudoClass.CandidatePopupOpen`、`CandidatePopupOpen=:candidateopen`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 4. 行为与状态模型

AutoComplete 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- selection/checked/active、open/close、loading/async、collection/filter、input/value、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

AutoComplete 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

`AutoCompleteLineEditBox`、`AutoCompleteSearchEditBox` 和 `AutoCompleteTextAreaBox` 分别复用 `LineEdit`、`SearchEdit` 和 `TextArea` 的 `AbstractTextInput` 逻辑层与 `InputControlFrame` 输入表面。候选 popup 和过滤状态属于 AutoComplete 自身，不得重新声明输入边框、状态或 Form error owner。

| 主题文件 | 职责 |
| --- | --- |
| `AbstractAutoCompleteTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `AutoCompleteSearchEditTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `AutoCompleteTextAreaTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `AutoCompleteTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

AutoComplete 使用 `AutoCompleteToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 selection/checked/active、open/close、loading/async、collection/filter、input/value、motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

AutoComplete 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `AbstractAutoComplete`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractAutoCompleteTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `AutoComplete`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `AutoCompleteLineEditBox`：继承 LineEdit 的模板协作类型，承载内容展示和候选宿主边界，输入表面由 shared frame 提供。
- `AutoCompleteSearchEdit`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `AutoCompleteSearchEditBox`：模板协作类型，承载内容展示、宿主或视觉边界。
- `AutoCompleteTextArea`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `AutoCompleteTextAreaBox`：继承 TextArea 的模板协作类型，承载多行内容和候选宿主边界，输入表面由 shared frame 提供。
- `AutoCompleteToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `CompactSpaceAwareAutoComplete`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `CompleteOptionsLoadResult`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 AutoComplete 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 选择与当前项模型

AutoComplete 的候选当前项由弹层中的 `CandidateList` 作为单一 active candidate owner。鼠标移动和 `Up` / `Down` 都只迁移这个候选，不提前改变输入值；`Enter` 使用同一个 active candidate 提交，提交后才更新 `Value` / `SelectionChanged` 并按控件契约关闭弹层。候选项容器只投影 active、selected 和 disabled 状态，`:pointerover` 不再形成第二个候选高亮。过滤结果、异步加载、集合替换、清空和模板重套用时，失效的 active candidate 必须被清除或重新从当前视图解析。

### 8.2 弹层与宿主模型

AutoComplete 涉及弹层、窗口或 overlay 宿主时，打开状态、取消事件、定位和宿主释放必须保持一致。重复打开、关闭、窗口失活和 template reapply 都必须释放旧宿主引用。

### 8.3 集合与数据同步模型

AutoComplete 的集合状态必须能处理 source replace、reset、clear 和 container recycle。业务数据对象不应反向持有视觉对象，虚拟化或懒创建路径必须在容器回收时清理旧状态。

### 8.4 动效模型

AutoComplete 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.5 视觉选项模型

AutoComplete 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [AutoComplete 桌面版实现原理](implementation.md)
- [AutoComplete Token 设计](token.md)
- [AutoComplete Semantic Part 契约](semantic-part.md)
- [AutoComplete Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 selection/checked/active、open/close、loading/async、collection/filter、input/value、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
