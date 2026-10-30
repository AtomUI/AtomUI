# SplitButton 桌面版架构设计

本文档定义 `SplitButton` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [SplitButton 桌面版实现原理](implementation.md)，Semantic Part 契约见 [SplitButton Semantic Part 契约](semantic-part.md)。SplitButton 没有独立 Token 文档；主题主要复用 SharedToken 或关联控件 Token。设计和契约变化记录见 [SplitButton Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `SplitButton`，其 public `IsPopupPinnedOpen` 供测试、诊断与 Gallery 语义预览钉住弹层使用；设置为 true 时保持 Flyout open state，并 relay 到关联 `Flyout` 及其 Popup，设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/General/SplitButton` |
| 控件状态 | Stable |

SplitButton 是 AtomUI 桌面控件体系中的拆分按钮，用于把主命令和次级下拉命令组合成一个紧凑动作入口。

SplitButton 不负责完整菜单系统、导航菜单或多步骤向导。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Buttons`

## 2. 设计语言

SplitButton 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | SplitButton 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | SplitButton 是 AtomUI 桌面控件体系中的拆分按钮，用于把主命令和次级下拉命令组合成一个紧凑动作入口。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Content`、`Icon`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | open/close、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | SharedToken / 关联控件 Token + ControlTheme。 |

## 3. API 与契约模型

SplitButton 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Content`、`Icon` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 交互与状态 | `IsArrowVisible`、`IsDanger`、`IsMotionEnabled`、`IsPointAtCenter`、`IsPopupPinnedOpen`、`IsPrimaryButtonType`、`IsWaveSpiritEnabled`、`ShouldUseOverlayPopup` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `Placement`、`PlacementAnchor`、`PlacementGravity`、`SizeType` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 弹层与窗口 | `Flyout`、`GutterToFlyout` | 控制 popup、flyout、dialog、window 或 overlay 宿主协作。 |
| 动效与异步 | `MouseEnterDelay`、`MouseLeaveDelay` | 约束动效开关、异步加载、播放速度、超时和任务边界。 |
| 其他稳定入口 | `Command`、`CommandParameter`、`HotKey`、`OpenIndicator`、`TriggerType` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

稳定事件包括 `Click`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`SplitButton`、`Button`、`FlyoutStateHelper`。
- 枚举：无。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_MainLayout` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_PrimaryButton` | `Button` | 承载用户触发入口、导航或关闭动作。 |
| `PART_SecondaryButton` | `Button` | 承载用户触发入口、导航或关闭动作。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

## 4. 行为与状态模型

SplitButton 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- open/close、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

SplitButton 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `SplitButtonTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |

SplitButton 当前没有专属 Token 文档；主题通过 SharedToken、关联控件 Token 或继承主题资源表达视觉语义。运行时状态不得写入 Token 模型。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

SplitButton 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `Button`：动作触发类型，负责点击、导航或局部操作状态。
- `FlyoutStateHelper`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SplitButton`：动作触发类型，负责点击、导航或局部操作状态。
- `SplitButtonTheme`：ControlTheme 类型入口，连接主题资源和控件类型。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 SplitButton 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 弹层与宿主模型

SplitButton 涉及弹层、窗口或 overlay 宿主时，打开状态、取消事件、定位和宿主释放必须保持一致。重复打开、关闭、窗口失活和 template reapply 都必须释放旧宿主引用。

### 8.2 动效模型

SplitButton 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.3 视觉选项模型

SplitButton 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [SplitButton 桌面版实现原理](implementation.md)
- [SplitButton Semantic Part 契约](semantic-part.md)
- [SplitButton Changelog](changelog.md)

Token 说明：

- SplitButton 当前没有专属 `token.md`；Token 边界按第 5 节视觉与主题模型、SharedToken、控件家族 Token 或主题资源说明 Token 边界。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 open/close、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 确认没有新增专属 Token，主题仍复用 SharedToken 或关联控件 Token。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
