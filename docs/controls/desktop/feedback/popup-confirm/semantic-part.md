# PopupConfirm Semantic Part 契约

本文档定义 PopupConfirm 控件公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[PopupConfirm 桌面版架构设计](overview.md)，真实模板、marker 映射与状态流见
[PopupConfirm 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

PopupConfirm 的唯一 Semantic owner 是 `PopupConfirm`。`PopupConfirm` 继承 `FlyoutHost`，弹层由
`PopupConfirmFlyout.CreatePresenter()` 在运行时创建、跨视觉根；internal 的 `PopupConfirmContainer`
不作为 public Semantic owner，它只承载确认体节点。公开 9 个 Semantic Part：`root`（隐式）+
8 个 `popup.*`。

语义对齐上游 Popconfirm 的语义 DOM（槽位：`root` / `container` / `icon` / `title` / `content` / `arrow`）。映射关系中有一处刻意差异：上游 `content` 槽位（描述正文）在 AtomUI
映射为 `popup.description`，因为 FlyoutHost 家族的 `popup.content` 已固定表示弹层框体的内容呈现面
（`ContentPresenter#ContentPresenter`），不能再用作描述节点。上游并未把按钮行
发布为语义 key，AtomUI 以 `popup.actions` 发布操作区，对齐 Alert `actions` 的稳定先例。

| PopupConfirm Part | 上游 Popconfirm 槽位 | AtomUI 节点 |
| --- | --- | --- |
| `root`（隐式） | 不适用（AtomUI 控件自身） | `PopupConfirm` owner |
| `popup.root` | `root` | `FlyoutPresenter`（`ArrowDecoratedBox`） |
| `popup.container` | `container` | `Border#PART_ContentDecorator` |
| `popup.content` | 不适用（Popover 家族框体内容面） | `ContentPresenter#ContentPresenter` |
| `popup.arrow` | `arrow` | `ArrowIndicator#PART_ArrowIndicator` |
| `popup.icon` | `icon` | `IconPresenter#PART_IconPresenter` |
| `popup.title` | `title` | `TextBlock#PART_Title` |
| `popup.description` | `content` | `ContentPresenter#PART_Content` |
| `popup.actions` | 不适用（上游按钮行未发布为语义 key） | `StackPanel#PART_ButtonLayout` |

声明位于 `PopupConfirm.SemanticParts.cs` partial 文件；`root` 为生成器隐式注册，不在该文件中显式声明。全部
8 个 `popup.*` 部件声明 `CrossVisualRoot=true` + `RuntimeCreated=true`：承载节点由
`PopupConfirmFlyout.CreatePresenter()` 在运行时创建，不在 `PopupConfirm` 自身 `PopupConfirmTheme.axaml`
模板内，`SelectorRoute` 因此使用从 owner 逻辑祖先链经 `popup.root` 下钻的后代路由。marker 注入策略分两类：

- 四个弹层框体件 `popup.root` / `popup.container` / `popup.content` / `popup.arrow` 复用 `FlyoutHost`
  家族共享的代码注入路径：`Flyout.CreatePresenter()` 注入 `popup.root`，`FlyoutPresenter.OnApplyTemplate`
  注入 `popup.container` / `popup.content` / `popup.arrow`。这三类 marker 不能静态声明在共享
  `ArrowDecoratedBoxTheme.axaml` 上，否则会污染 DatePicker / TimePicker / Menu / TreeView 等其它
  `ArrowDecoratedBox` 弹层。
- 四个确认体件 `popup.icon` / `popup.title` / `popup.description` / `popup.actions` 的 marker 静态声明在
  `PopupConfirmContainerTheme.axaml` 上；该主题只服务 PopupConfirm 内部容器，静态标记不会污染其它控件。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `root` |
| Selector | PopupConfirm 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `PopupConfirm` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | PopupConfirm owner |
| 职责 | 触发宿主与确认状态的组织边界，承载 public API、确认/取消事件与主题入口。 |
| 相关 API | 全部 PopupConfirm public API |
| 相关 Token | PopupConfirmToken、FlyoutHostToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `>> .semantic-popup-root` |
| Style Type | `PopupConfirmPopupRootStyle` |
| ContractType | `FlyoutPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `Flyout.CreatePresenter()` 创建的 `FlyoutPresenter`（`ArrowDecoratedBox`） |
| 职责 | 弹层根节点，承载弹层背景、边框、内边距与箭头，对应上游 Popconfirm `root` 槽位。 |
| 相关 API | `Flyout`、`ShouldUseOverlayPopup`、`IsArrowVisible` |
| 相关 Token | FlyoutHostToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.container`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.container` |
| Selector | `.semantic-popup-container` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-container` |
| Style Type | `PopupConfirmPopupContainerStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ArrowDecoratedBoxTheme.axaml` 的 `Border#PART_ContentDecorator`（背景/边框/圆角/内边距，模板绑定自 presenter） |
| 职责 | 弹层内层容器，承载背景、边框、圆角与内边距，对应上游 Popconfirm `container` 槽位。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.content`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.content` |
| Selector | `.semantic-popup-content` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-content` |
| Style Type | `PopupConfirmPopupContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ArrowDecoratedBoxTheme.axaml` 的 `ContentPresenter#ContentPresenter` |
| 职责 | 弹层框体的内容呈现面，承载确认体容器；同为 FlyoutHost 家族的内容槽位，不对应上游 Popconfirm 单独语义 key。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.arrow`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.arrow` |
| Selector | `.semantic-popup-arrow` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-arrow` |
| Style Type | `PopupConfirmPopupArrowStyle` |
| ContractType | `ArrowIndicator` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ArrowDecoratedBoxTheme.axaml` 的 `ArrowIndicator#PART_ArrowIndicator` |
| 职责 | 指向锚点的浮动箭头，对应上游 Popconfirm `arrow` 槽位。 |
| 相关 API | `IsArrowVisible`、`Placement` |
| 相关 Token | ArrowDecoratedBoxToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.icon`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.icon` |
| Selector | `.semantic-popup-icon` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-icon` |
| Style Type | `PopupConfirmPopupIconStyle` |
| ContractType | `IconPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `PopupConfirmContainerTheme.axaml` 的 `IconPresenter#PART_IconPresenter` |
| 职责 | 确认状态图标，`ConfirmStatus` 通过 `IconBrush` 切换主题/警告/错误色，对应上游 Popconfirm `icon` 槽位。 |
| 相关 API | `Icon`、`ConfirmStatus` |
| 相关 Token | PopupConfirmToken、SharedToken（`IconSizeLG`、`ColorPrimary/ColorWarning/ColorError`） |
| 稳定性 | stable since 6.2.0 |

#### `popup.title`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.title` |
| Selector | `.semantic-popup-title` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-title` |
| Style Type | `PopupConfirmPopupTitleStyle` |
| ContractType | `Avalonia.Controls.TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `PopupConfirmContainerTheme.axaml` 的 `TextBlock#PART_Title` |
| 职责 | 确认框标题，对应上游 Popconfirm `title` 槽位；标题文字由 `SharedToken ColorTextHeading` 与 `FontWeight=SemiBold` 承载。 |
| 相关 API | `Title` |
| 相关 Token | PopupConfirmToken（`TitleMargin`）、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.description`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.description` |
| Selector | `.semantic-popup-description` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-description` |
| Style Type | `PopupConfirmPopupDescriptionStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `PopupConfirmContainerTheme.axaml` 的 `ContentPresenter#PART_Content` |
| 职责 | 确认描述正文，对应上游 Popconfirm `content` 槽位（AtomUI 因 `popup.content` 已占用而改名为 `popup.description`）。 |
| 相关 API | `ConfirmContent`、`ConfirmContentTemplate` |
| 相关 Token | PopupConfirmToken（`ContentContainerMargin`）、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.actions`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.actions` |
| Selector | `.semantic-popup-actions` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-actions` |
| Style Type | `PopupConfirmPopupActionsStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `PopupConfirmContainerTheme.axaml` 的 `StackPanel#PART_ButtonLayout` |
| 职责 | 确认/取消操作区，承载 `PART_CancelButton` 与 `PART_OkButton`；对齐 Alert `actions` 先例，上游未把按钮行发布为语义 key。 |
| 相关 API | `OkText`、`CancelText`、`OkButtonType`、`IsShowCancelButton` |
| 相关 Token | PopupConfirmToken（`ButtonSpacing`、`ButtonContainerMargin`）、SharedToken |
| 稳定性 | stable since 6.2.0 |

## 2. 职责与存在条件

- 弹层由 `PopupConfirmFlyout.CreatePresenter()` 在弹层打开时才创建，是跨视觉根的 Popup 子节点，
  `TemplatedParent` 不挂在 `PopupConfirm` 模板链上。因此 8 个 `popup.*` 部件全部声明
  `CrossVisualRoot=true` + `RuntimeCreated=true`，生成器豁免宿主模板 marker 校验，由控件行为测试兜底。
- 四个弹层框体件的 marker 由共享 FlyoutHost 家族代码路径注入：`popup.root` 在 `Flyout.CreatePresenter()`
  创建 `FlyoutPresenter` 时注入，`popup.container` / `popup.content` / `popup.arrow` 在
  `FlyoutPresenter.OnApplyTemplate` 注入到共享 `ArrowDecoratedBoxTheme` 的模板节点上。任何一次弹层重开或
  模板重应用都必须重新注入，不能依赖上一次应用残留。
- 四个确认体件的 marker 静态声明在 `PopupConfirmContainerTheme.axaml` 的模板节点上，随容器模板实例化
  一次性设置。
- **存在条件**：
  - 弹层关闭时 8 个 `popup.*` 部件均不实例化。
  - 弹层打开时 8 者同时存在；`popup.arrow` 仅在 `IsArrowVisible=true` 时可见（节点仍在，仅可见性切换）；
    `popup.description` 仅在 `ConfirmContent != null` 时可见（`:empty-content` 伪类驱动）；
    `popup.actions` 内 `PART_CancelButton` 仅在 `IsShowCancelButton=true` 时可见。
  - `root`（`PopupConfirm`）与弹层打开状态无关，恒存在。

## 3. 数量语义

`root` 与 8 个 `popup.*` 均为 `Single`：每个 PopupConfirm 宿主在弹层打开时各实例化唯一一个对应节点。
打开状态、`IsArrowVisible`、`ConfirmStatus`、`ConfirmContent` 的有无和主题切换只改变可见性或有效视觉值，
不增删 marker；弹层关闭销毁 presenter 及其模板子树，重开重建，marker 身份与数量保持不变。

## 4. Selector 用法

生成的 Semantic Style 类型命名为 `PopupConfirm<PartPathPascalCase>Style`，如 `PopupConfirmPopupRootStyle`、
`PopupConfirmPopupIconStyle`、`PopupConfirmPopupActionsStyle`（命名空间 `AtomUI.Theme.Styling`，AXAML 命名空间
`https://atomui.net`）。`root` 不生成 Style 类型，owner 级 Setter 写在外层普通 Style 上。

**路由说明**：生成的 `PopupConfirmPopupXxxStyle` 选择器以 owner 为根、经 `>>` 后代组合器下钻（生成器把
`SelectorRoute` 中的 `>>` 映射为 Avalonia `.Descendant()`）。弹层根是代码创建、视觉上挂在独立 Popup 根里的
`FlyoutPresenter`，但它在**逻辑树**上仍是 `PopupConfirm` 的后代
（`FlyoutPresenter → Popup → 锚点/Content → PopupConfirm`），因此 `>>` 后代路由能跨越视觉根命中弹层节点。

```xml
<Style Selector="atom|PopupConfirm.semantic-demo">
    <atom:PopupConfirmPopupContainerStyle x:SetterTargetType="Border">
        <Setter Property="Padding" Value="12" />
    </atom:PopupConfirmPopupContainerStyle>
</Style>
```

`FlyoutPresenter`（`popup.root`）继承自 `ArrowDecoratedBox` / `ContentControl`：`Background`、
`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding` 经模板绑定作用于 `popup.container` 的
`Border`，`Foreground` 经属性继承作用于 `popup.content` 的文本；因此直接定制 `popup.root` 即可覆盖整层
弹层的视觉。需要更细粒度命中内部节点时，改用 `PopupConfirmPopupContainerStyle` /
`PopupConfirmPopupIconStyle` / `PopupConfirmPopupTitleStyle` / `PopupConfirmPopupDescriptionStyle` /
`PopupConfirmPopupActionsStyle` / `PopupConfirmPopupArrowStyle`。

**标题前景色必须走 `PopupConfirmPopupTitleStyle`**：`PopupConfirmContainerTheme.axaml` 在
`TextBlock#PART_Title` 上静态设置了 `SharedToken ColorTextHeading`，其优先级高于 `popup.root` 的
`Foreground` 继承。因此在深色弹层上改变标题颜色（例如改为浅色文字）时只设置 `popup.root` 的 `Foreground`
不会生效，必须用 `PopupConfirmPopupTitleStyle` 显式覆盖；`popup.description` 无静态前景色，继续跟随
`popup.root` 的 `Foreground` 继承。

不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型（含 `PopupConfirmContainer`）或视觉祖先顺序
  作为应用主题契约。
- 把 `Border.semantic-popup-container` 等 `ContractType` 写入 Part 身份 selector。
- 穿过 `ConfirmContent` / `ConfirmContentTemplate` 等用户内容模板继续匹配内部 Visual。
- 把 `PART_OkButton` / `PART_CancelButton` 当作 PopupConfirm 的 Semantic Part；它们是 `popup.actions`
  内部的 Button 控件，属于嵌套 owner，PopupConfirm 不穿透其 descriptor。

## 5. 定制边界

以下区域不属于 PopupConfirm Semantic Part：

- **操作按钮内部**：`PART_OkButton` / `PART_CancelButton` 是 `Button` 控件实例，其 icon/content 等语义
  属于 Button 自己的契约，由 Button 的 owner 隔离；PopupConfirm 只开放承载它们的 `popup.actions`。
- **弹层 Popup 宿主与定位**：弹层 Popup 的定位、钉住打开、动画由共享 Popup 契约承担，`popup.root`
  只覆盖弹层内容根 `FlyoutPresenter` 的视觉。
- **锚点、触发与动效**：`Content`（触发器）、`Trigger`、`TriggerType`、`Placement`、`IsArrowVisible`、
  motion 相关属性由行为 API 承担，不以 semantic key 发布。
- **内部布局 wrapper**：`PART_MainLayout`、`PART_ButtonLayout` 的父 `DockPanel` 等布局节点不进入公开
  Part；其中 `PART_ButtonLayout` 本身以 `popup.actions` 发布。
- `PART_*` 名称、internal 类型（`PopupConfirmContainer`、`PopupConfirmFlyout`）与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态
订阅。Semantic Style 服从 Avalonia 原生属性优先级。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`（含把公共基类承诺收窄为具体实现
类型）、改变 cardinality，或让弹层模板变体缺少 marker，均属于公共主题契约变更。

与上游 Popconfirm 的对照差异（有意保持）：

- 上游 `content` 槽位映射为 `popup.description`；`popup.content` 保留为 FlyoutHost 家族的弹层框体内容面。
- 上游未把按钮行发布为语义 key；AtomUI 以 `popup.actions` 发布操作区（对齐 Alert `actions` 先例）。
- AtomUI 额外的隐式 `root` 是 `PopupConfirm` 触发宿主自身，属于 AtomUI 所有控件统一的 owner 惯例。

验证至少覆盖：

- owner descriptor 只包含 §1 的 9 个 Part，字段值与本文一致（`tests/AtomUI.Desktop.Controls.Tests/PopupConfirm/PopupConfirmSemanticPartTests.cs`）。
- `PopupConfirmContainerTheme.axaml` 静态声明 4 个确认体 marker；`PopupConfirmTheme.axaml` 不声明任何
  弹层 marker；两者默认主题均不消费 `.semantic-*` selector。
- 运行时解析：钉住打开后 8 个 `popup.*` 各落在正确的跨视觉根节点（弹层框体件在 `FlyoutPresenter` 子树，
  确认体件在 `PopupConfirmContainer` 子树）。
- 生成的 `PopupConfirmPopupXxxStyle` 可编译并实例化（`Style` 派生类型），且经 `>>` 后代路由命中代码创建的
  跨视觉根弹层节点。
- Gallery Semantic Parts Tab 延迟创建 Preview，弹层钉住常开，9 个 Part 均可解析高亮（跨视觉根弹层根由
  `SemanticPartPreview.AdditionalRoots` 显式注册）。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
