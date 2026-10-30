# DropdownButton 桌面版架构设计

本文档定义 `DropdownButton` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [DropdownButton 桌面版实现原理](implementation.md)。DropdownButton 没有独立 Token 文档；主题主要复用 SharedToken 或关联控件 Token。设计和契约变化记录见 [DropdownButton Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `DropdownButton`，其 public `IsPopupPinnedOpen` 供测试、诊断与 Gallery 语义预览钉住弹层使用；设置为 true 时保持 dropdown Flyout open state，并 relay 到 `DropdownFlyout` 及其 MenuFlyout Popup，设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Navigation/DropdownButton` |
| 控件状态 | Stable |

DropdownButton 是 AtomUI 桌面控件体系中的下拉按钮，用于从一个动作按钮展开次级命令或菜单内容。

DropdownButton 不负责拆分主次命令、完整 Menu 导航或上下文菜单。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Buttons`

## 2. 设计语言

DropdownButton 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | DropdownButton 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | DropdownButton 是 AtomUI 桌面控件体系中的下拉按钮，用于从一个动作按钮展开次级命令或菜单内容。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | 继承 Button 的 `Content`、`Icon`、`Command` 等动作入口。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | open/close、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | SharedToken / 关联控件 Token + ControlTheme。 |

## 3. API 与契约模型

DropdownButton 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Content`、`Icon`、`Command` | 继承 Button 的展示内容、图标和动作命令入口。 |
| 交互与状态 | `IsArrowVisible`、`IsPointAtCenter`、`IsShowOpenIndicator`、`ShouldUseOverlayPopup` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `IconWidth`、`IconHeight`、`MarginToAnchor`、`Placement`、`PlacementAnchor`、`PlacementGravity` | 继承 Button 的用户图标和 loading 图标尺寸入口，并管理下拉定位、密度和模板视觉变量。 |
| 弹层与窗口 | `DropdownFlyout` | 控制 popup、flyout、dialog、window 或 overlay 宿主协作。 |
| 动效与异步 | `MouseEnterDelay`、`MouseLeaveDelay` | 约束动效开关、异步加载、播放速度、超时和任务边界。 |
| 其他稳定入口 | `OpenIndicator`、`TriggerType` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

稳定事件包括 `MenuItemClicked`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`DropdownButton`、`Button`、`MenuFlyout`、`FlyoutStateHelper`。
- 枚举：无。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_ButtonIcon` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_LoadingIcon` | `Icon` | 展示继承自 Button loading 状态的图标。 |
| `PART_ContentPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_DropdownIndicator` | `?` | 展示指示器、进度、分页或状态反馈。 |
| `PART_RootLayout` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_WaveSpirit` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

DropdownButton 继承 Button 的 `IconWidthProperty`、`IconHeightProperty` 及 CLR wrapper。`PART_ButtonIcon` 与 `PART_LoadingIcon` 通过 `TemplateBinding` 消费这两个属性，保持与 Button 相同的 SizeType 默认值、本地值优先级和 icon-only loading 状态语义。`PART_DropdownIndicator` 展示的 `OpenIndicator` 是独立图标角色，不受 `IconWidth`、`IconHeight` 控制。

## 4. 行为与状态模型

DropdownButton 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- open/close、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

DropdownButton 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `DropdownButtonBaseTheme.axaml` | 定义 DropdownButton 与 Button 共享的模板、图标尺寸和基础状态视觉。 |
| `DropdownButtonTheme.axaml` | 定义 DropdownButton 的下拉指示器和具体视觉入口。 |

DropdownButton 当前没有专属 Token 文档；主题通过 SharedToken、关联控件 Token 或继承主题资源表达视觉语义。运行时状态不得写入 Token 模型。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Native 和 Browser 支持宿主必须使用同一套 DropdownButton 主题资产；平台差异不能通过 Browser 专用主题分叉复制视觉。
- 用户 icon 与 loading icon 的尺寸只能从 DropdownButton 自身的 `IconWidth`、`IconHeight` 投影；应用和 Gallery 不得通过 `/template/` 或 `PART_ButtonIcon`、`PART_LoadingIcon` selector 修改内部尺寸。
- `OpenIndicator` 尺寸由 DropdownButton 主题单独管理，不复用用户 icon 的宽高属性。

## 6. 控件家族或集成关系

DropdownButton 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `Button`：动作触发类型，负责点击、导航、局部操作状态以及继承给 DropdownButton 的 `IconWidth`、`IconHeight` 尺寸契约。
- `DropdownButton`：动作触发类型，负责点击、导航或局部操作状态。
- `DropdownButtonTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `FlyoutStateHelper`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MenuFlyout`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 DropdownButton 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- `IconWidth`、`IconHeight` 本地值必须同时覆盖用户 icon 与 loading icon 的 Theme 默认值，且不得改变 `OpenIndicator` 尺寸。
- 非 loading 的 icon-only 用户图标继续使用普通 `IconSize*` 默认值；只有 icon-only loading 使用 DropdownButton 对应的 `OnlyIconSize*` 默认值。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 弹层与宿主模型

DropdownButton 涉及弹层、窗口或 overlay 宿主时，打开状态、取消事件、定位和宿主释放必须保持一致。重复打开、关闭、窗口失活和 template reapply 都必须释放旧宿主引用。

### 8.2 视觉选项模型

DropdownButton 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [DropdownButton 桌面版实现原理](implementation.md)
- [DropdownButton Semantic Part 契约](semantic-part.md)
- [DropdownButton Changelog](changelog.md)

Token 说明：

- DropdownButton 当前没有专属 `token.md`；Token 边界按第 5 节视觉与主题模型、SharedToken、控件家族 Token 或主题资源说明 Token 边界。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令，以及从 Button 继承的 `IconWidth`、`IconHeight` 语义。 |
| 状态模型 | 覆盖 open/close、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查两个图标 part 的 `TemplateBinding`、`OpenIndicator` 独立尺寸、伪类、资源 key、Light/Dark 主题和 Browser 共享主题注册。 |
| Token | 确认没有新增专属 Token，主题仍复用 SharedToken 或关联控件 Token。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
