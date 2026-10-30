# InfoFlyout Semantic Part 契约

本文档定义 InfoFlyout 控件公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[InfoFlyout 桌面版架构设计](overview.md)，真实模板、marker 映射与状态流见
[InfoFlyout 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

InfoFlyout 的唯一 Semantic owner 是 `FlyoutHost`（`Flyout` 为 `PopupFlyoutBase`，非 Visual 控件，语义
registry 按单一 `ControlType` 索引，不能作为 owner）。公开 5 个 Semantic Part：`root`（隐式）+ 4 个
`popup.*`（`popup.root` / `popup.container` / `popup.content` / `popup.arrow`）。

语义对齐上游 Popover 的语义 DOM（`_semantic.tsx`，槽位：root / container / title / content / arrow）。
InfoFlyout 弹层没有标题节点，因此上游 `title` 槽位省略（后续引入标题节点时再补）。四个弹层槽位沿用
AtomUI Select 家族的 `popup.` 前缀惯例，对应关系：

| InfoFlyout Part | 上游 Popover 槽位 | AtomUI 节点 |
| --- | --- | --- |
| `root`（隐式） | 不适用（AtomUI 控件自身） | `FlyoutHost` owner |
| `popup.root` | `root` | `FlyoutPresenter`（`ArrowDecoratedBox`） |
| `popup.container` | `container` | `Border#PART_ContentDecorator` |
| `popup.content` | `content` | `ContentPresenter#ContentPresenter` |
| `popup.arrow` | `arrow` | `ArrowIndicator#PART_ArrowIndicator` |
| （省略） | `title` | 无标题节点 |

声明位于 `FlyoutHost.SemanticParts.cs` partial 文件。四个 `popup.*` 部件全部 `CrossVisualRoot=true`、
`RuntimeCreated=true`：弹层根是 `Flyout.CreatePresenter()` 代码创建、跨视觉根的 Popup 子节点，不在
owner 的 `FlyoutHostTheme` 模板内。marker 的注入策略如下：

- `popup.root` 的 `semantic-popup-root` 标记在 `Flyout.CreatePresenter()` 创建 `FlyoutPresenter` 时注入。
- `popup.container` / `popup.content` / `popup.arrow` 的标记在 `FlyoutPresenter.OnApplyTemplate` 中注入，
  分别命中共享 `ArrowDecoratedBoxTheme` 的 `Border#PART_ContentDecorator`、`ContentPresenter#ContentPresenter`
  与 `ArrowIndicator#PART_ArrowIndicator` 节点。这三个类 **不** 静态声明在 `ArrowDecoratedBoxTheme.axaml`
  上：该主题被 DatePicker / TimePicker / Menu / TreeView / PopupConfirm 等众多 `ArrowDecoratedBox` 弹层共用，
  静态追加会污染其它控件的语义标记，因此按 InfoFlyout 语义部件契约在代码路径注入（与 TreeSelect 的
  `popup.list` / `popup.listItem` 等 RuntimeCreated 部件同构）。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `FlyoutHost` |
| Part | `root` |
| Selector | FlyoutHost 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `FlyoutHost` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | FlyoutHost owner |
| 职责 | InfoFlyout 触发宿主，是内容、触发方式、定位、动效与弹层打开状态的组织边界。 |
| 相关 API | 全部 FlyoutHost public API |
| 相关 Token | FlyoutHostToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `FlyoutHost` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `>> .semantic-popup-root` |
| Style Type | `FlyoutHostPopupRootStyle` |
| ContractType | `FlyoutPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `Flyout.CreatePresenter()` 创建的 `FlyoutPresenter`（`ArrowDecoratedBox`） |
| 职责 | 弹层根节点，承载弹层背景、边框、内边距与箭头，对应上游 Popover `root` 槽位。 |
| 相关 API | `Flyout`、`FlyoutPresenterTheme`、`ShouldUseOverlayPopup` |
| 相关 Token | FlyoutHostToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.container`

| 字段 | 值 |
| --- | --- |
| Owner | `FlyoutHost` |
| Part | `popup.container` |
| Selector | `.semantic-popup-container` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-container` |
| Style Type | `FlyoutHostPopupContainerStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ArrowDecoratedBoxTheme.axaml` 的 `Border#PART_ContentDecorator`（背景/边框/圆角/内边距，模板绑定自 presenter） |
| 职责 | 弹层内容内层容器，承载背景、边框、圆角与内边距，对应上游 Popover `container` 槽位。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.content`

| 字段 | 值 |
| --- | --- |
| Owner | `FlyoutHost` |
| Part | `popup.content` |
| Selector | `.semantic-popup-content` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-content` |
| Style Type | `FlyoutHostPopupContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ArrowDecoratedBoxTheme.axaml` 的 `ContentPresenter#ContentPresenter` |
| 职责 | 弹层用户内容呈现区域，对应上游 Popover `content` 槽位。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.arrow`

| 字段 | 值 |
| --- | --- |
| Owner | `FlyoutHost` |
| Part | `popup.arrow` |
| Selector | `.semantic-popup-arrow` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-arrow` |
| Style Type | `FlyoutHostPopupArrowStyle` |
| ContractType | `ArrowIndicator` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ArrowDecoratedBoxTheme.axaml` 的 `ArrowIndicator#PART_ArrowIndicator` |
| 职责 | 指向锚点的浮动箭头，对应上游 Popover `arrow` 槽位。 |
| 相关 API | `IsArrowVisible`、`ArrowPosition`、`ArrowSize` |
| 相关 Token | ArrowDecoratedBoxToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

## 2. 职责与存在条件

- `popup.root` 是跨视觉根、代码创建的 Popup 子节点，其 `TemplatedParent` 不挂在 `FlyoutHost` 模板链上。
  因此四个 `popup.*` 部件全部声明 `CrossVisualRoot=true`、`RuntimeCreated=true`，生成器豁免宿主模板
  marker 校验，由控件行为测试兜底。
- `popup.container` / `popup.content` / `popup.arrow` 的 marker 在 `FlyoutPresenter.OnApplyTemplate`
  运行时注入到共享 `ArrowDecoratedBoxTheme` 的模板节点上；`popup.root` 的 marker 在
  `Flyout.CreatePresenter()` 创建 presenter 时注入。任何一次弹层重开或模板重应用都必须重新注入，不能
  依赖上一次应用残留。
- **存在条件**（与上游一致：槽位集合统一，实例化随打开状态变化）：
  - 弹层关闭时四个 `popup.*` 部件均不实例化。
  - 弹层打开时四者同时存在；`popup.arrow` 仅在 `IsArrowVisible=true` 时可见（节点仍在，仅可见性切换），
    `popup.container` / `popup.content` 始终可见。
  - `root`（`FlyoutHost`）与弹层打开状态无关，恒存在。

## 3. 数量语义

`root`、`popup.root`、`popup.container`、`popup.content`、`popup.arrow` 均为 `Single`：每个 InfoFlyout
宿主在弹层打开时各实例化唯一一个对应节点。打开状态、`IsArrowVisible`、主题切换只改变可见性或有效视觉
值，不增删 marker；弹层关闭销毁 presenter 及其模板子树，重开重建，marker 身份与数量保持不变。

## 4. Selector 用法

生成的 Semantic Style 类型命名为 `FlyoutHost<PartPathPascalCase>Style`，如 `FlyoutHostPopupRootStyle`、
`FlyoutHostPopupContainerStyle`、`FlyoutHostPopupContentStyle`、`FlyoutHostPopupArrowStyle`（命名空间
`AtomUI.Theme.Styling`，AXAML 命名空间 `https://atomui.net`）。`root` 不生成 Style 类型，owner 级 Setter
写在外层普通 Style 上。

**路由说明**：生成的 `FlyoutHostPopupXxxStyle` 选择器以 owner 为根、经 `>>` 后代组合器下钻
（生成器把 `SelectorRoute` 中的 `>>` 映射为 Avalonia `.Descendant()`，最终选择器形如
`atom|FlyoutHost >> .semantic-popup-root`）。虽然 InfoFlyout 的弹层根是代码创建、视觉上挂在独立
Popup 根里的 `FlyoutPresenter`，但它在**逻辑树**上仍是 `FlyoutHost` 的后代
（`FlyoutPresenter → Popup → 锚点/Content → FlyoutHost`），因此 `>>` 后代路由能跨越视觉根命中弹层
节点。这是与 DatePicker / TimePicker / TreeSelect 等把弹层静态放进 owner 模板、用 `/template/`
锚点的控件不同的地方——InfoFlyout 因此选择后代路由而非模板路由。

Gallery 的 StyleClass 示例即用此契约：在外层普通 Style 上限定宿主类，内嵌生成的
`FlyoutHostPopupXxxStyle` 定制对应槽位：

```xml
<Style Selector="atom|FlyoutHost.semantic-styles-object-demo">
    <atom:FlyoutHostPopupRootStyle x:SetterTargetType="atom:FlyoutPresenter">
        <Setter Property="Background" Value="#EEEEEE" />
        <Setter Property="Foreground" Value="#262626" />
        <Setter Property="Padding" Value="10" />
    </atom:FlyoutHostPopupRootStyle>
</Style>
```

`FlyoutPresenter`（`popup.root`）继承自 `ArrowDecoratedBox` / `ContentControl`：`Background`、
`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding` 经模板绑定作用于 `popup.container` 的
`Border`，`Foreground` 经属性继承作用于 `popup.content` 的文本，`Background` 同时经模板绑定作用于
`popup.arrow` 的 `FilledColor`；因此直接定制 `popup.root` 即可覆盖整层弹层的视觉。需要更细粒度命中
内部节点时，改用 `FlyoutHostPopupContainerStyle` / `FlyoutHostPopupContentStyle` /
`FlyoutHostPopupArrowStyle`。

不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `Border.semantic-popup-container` 等 `ContractType` 写入 Part 身份 selector。
- 穿过 `Content` / `ContentTemplate` 等用户模板继续匹配内部 Visual。

## 5. 定制边界

以下区域不属于 InfoFlyout Semantic Part：

- **标题槽位 `title`**：InfoFlyout 弹层没有标题节点，上游 antd Popover 的 `title` 槽位省略，另立特性时
  再补充（对应上游 `title` 语义 key）。
- **Menu / TreeView 内容**：`MenuFlyout` / `MenuFlyoutPresenter` / `TreeViewFlyout` /
  `TreeViewFlyoutPresenter` 及其菜单项 / 树节点内部结构不在本次范围，后续单独提交。
- **弹层 Popup 宿主与定位**：弹层 Popup 的定位、钉住打开、动画由共享 Popup 契约承担，`popup.root` 只
  覆盖弹层内容根 `FlyoutPresenter` 的视觉。
- **锚点、触发与动效**：`AnchorTarget`、`Trigger`、`TriggerType`、`OpenMotion` / `CloseMotion` /
  `MotionDuration` 等由行为 API 承担（上游同样以 prop 而非 semantic key 发布）。
- `PART_*` 名称、internal 类型与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态
订阅。Semantic Style 服从 Avalonia 原生属性优先级。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`（含把公共基类承诺收窄为具体实现
类型）、改变 cardinality，或让弹层模板变体缺少 marker，均属于公共主题契约变更。

与上游 antd Popover 的对照差异（有意保持）：

- 上游 `title` 槽位省略（InfoFlyout 无标题节点）。
- 上游 Popover 的 `root` 槽位对应 AtomUI `popup.root`；AtomUI 额外的隐式 `root` 是 `FlyoutHost` 触发宿主
  自身，属于 AtomUI 所有控件统一的 owner 惯例。

验证至少覆盖：

- owner descriptor 只包含 §1 的 5 个 Part，字段值与本文一致。
- `FlyoutHostTheme.axaml` 不声明 `Classes.semantic-*` 标记；四个 `popup.*` 标记全部在代码路径注入
  （`Flyout.CreatePresenter` 与 `FlyoutPresenter.OnApplyTemplate`）。
- 默认主题不消费 `.semantic-*` selector。
- 运行时解析：钉住打开后 `popup.root` / `popup.container` / `popup.content` / `popup.arrow` 各落在
  正确的跨视觉根节点（见 `tests/AtomUI.Desktop.Controls.Tests/Flyouts/FlyoutSemanticPartTests.cs`）。
- 生成的 `FlyoutHostPopupXxxStyle` 可编译并实例化（`Style` 派生类型），且经 `>>` 后代路由命中代码创建的
  跨视觉根弹层节点（§4）。
- Gallery Semantic Parts Tab 延迟创建 Preview，弹层钉住常开，5 个 Part 均可解析高亮（跨视觉根弹层根由
  `SemanticPartPreview.AdditionalRoots` 显式注册）。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
