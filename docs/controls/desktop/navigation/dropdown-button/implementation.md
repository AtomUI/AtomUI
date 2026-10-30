# DropdownButton 桌面版实现原理

本文档描述 DropdownButton 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [DropdownButton 桌面版架构设计](overview.md)，Semantic Part 契约见 [DropdownButton Semantic Part 契约](semantic-part.md)，变化记录见 [DropdownButton Changelog](changelog.md)。DropdownButton 没有独立 Token 文档；涉及主题变量时应回到 overview 的视觉与主题模型。

Popup 接入边界：`DropdownButton` 负责业务状态和内容准备，`DropdownFlyout` / `MenuFlyout` 仅作为 relay 适配层，Flyout Popup 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

本文档覆盖 DropdownButton 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/DropdownButton/DropdownButton.cs`
- `src/AtomUI.Desktop.Controls/Buttons/Themes/DropdownButtonBaseTheme.axaml`
- `src/AtomUI.Desktop.Controls/Buttons/Themes/DropdownButtonTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `Button`：动作触发类型，负责点击、导航或局部操作状态。
- `DropdownButton`：动作触发类型，负责点击、导航或局部操作状态。
- `DropdownButtonTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `FlyoutStateHelper`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MenuFlyout`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

DropdownButton 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：继承 Button 的 `Content`、`Icon`、`Command` 等动作入口。
- 交互与状态：`IsArrowVisible`、`IsPointAtCenter`、`IsShowOpenIndicator`、`ShouldUseOverlayPopup`。
- 视觉与布局：继承 Button 的 `IconWidth`、`IconHeight`，并扩展 `MarginToAnchor`、`Placement`、`PlacementAnchor`、`PlacementGravity`。
- 弹层与窗口：`DropdownFlyout`。
- 动效与异步：`MouseEnterDelay`、`MouseLeaveDelay`。
- 其他稳定入口：`OpenIndicator`、`TriggerType`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Native 和 Browser 支持宿主下的主题加载顺序不得影响 public API 语义。

图标尺寸不在模板生命周期中手工同步。`DropdownButtonBaseTheme` 和具体 `DropdownButtonTheme` 中的 `PART_ButtonIcon`、`PART_LoadingIcon` 都通过 `TemplateBinding IconWidth/IconHeight` 读取 owner 属性；Theme selector 也只设置 owner 属性默认值。`PART_DropdownIndicator` 继续按 DropdownButton 的 OpenIndicator 主题规则独立设置尺寸。

稳定 template part 接入点：

- `PART_WaveSpirit`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_RootLayout`：承载根视觉、边框、背景或尺寸基线。
- `PART_DropdownIndicator`：展示指示器、进度、分页或状态反馈。
- `PART_LoadingIcon`：展示继承自 Button loading 状态的图标，并绑定 owner 的图标宽高。
- `PART_ButtonIcon`：承载用户触发入口、导航或关闭动作。
- `PART_ContentPresenter`：展示用户内容、文本、图标或模板化数据。

### 5.1 Semantic Part marker 注入

Semantic Part 的 marker 全部为运行时注入，完整契约见 [DropdownButton Semantic Part 契约](semantic-part.md)。
对齐上游 antd Dropdown，DropdownButton 不发布触发侧 Semantic Part，五个弹层部件
（`popup.root` / `itemTitle` / `item` / `itemContent` / `itemIcon`）为避免把 marker 静态写进被 SplitButton、
DataGrid、TabControl、Transfer 等复用的共享 MenuFlyout / MenuItem 控件（沿用 FlyoutHost → FlyoutPresenter 的
跨视觉根弹层先例）：

- `MenuFlyoutPresenter.OnApplyTemplate` 定位弹层根视觉面 `ArrowDecoratedBox` 时注入 `popup.root` marker（边框 /
  背景 / 圆角由 `ArrowDecoratedBox` 渲染，`MenuFlyoutPresenter` 是共享菜单宿主容器，不承载弹层根视觉）；
- `MenuFlyoutPresenter` 与 `MenuItem` 的 `CreateContainerForItemOverride` / `PrepareContainerForItemOverride`
  容器路径注入 `item` marker，覆盖顶层与嵌套子菜单项；
- `MenuItemGroup.OnApplyTemplate` 向 `GroupTitlePresenter` 注入 `itemTitle` marker（分组容器本身带
  `semantic-item-title-group` 中间标记类）；
- `MenuItem.OnApplyTemplate` 向 `ItemIconPresenter` / `ItemTextPresenter` 注入 `itemIcon` / `itemContent`
  marker。

## 6. 交互与事件处理

DropdownButton 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 弹层、窗口或 overlay 类路径必须稳定处理打开、关闭、取消、重复打开和宿主失活。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 值提交或命令触发必须保持继承控件的事件顺序。

稳定事件路径包括 `MenuItemClicked`。事件参数和触发时机属于兼容边界。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- 内容、命令和视觉状态在模板节点之间的同步。
- `SizeType` 到 `IconWidth`、`IconHeight` 默认值的映射，以及 `:icononly:loading` 对 `OnlyIconSize*` 的状态覆盖。
- 状态变化时避免创建不必要的视觉对象、订阅或动画对象。

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
- 图标尺寸使用 owner StyledProperty、Style 默认值与 `TemplateBinding` 单向投影，不为模板 part 建立运行时订阅或查找同步。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 DropdownButton 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- `IconWidth`、`IconHeight` 的本地值优先级、两个图标 part 的一致投影以及 `OpenIndicator` 的独立尺寸职责。
- 外部样式不得使用 `/template/` 或 part selector 修改用户 icon、loading icon 的宽高。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- 图标尺寸变更覆盖 Desktop / Browser、Large / Middle / Small / Custom、icon-only / loading、本地非正方形尺寸和 `OpenIndicator` 独立性。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
