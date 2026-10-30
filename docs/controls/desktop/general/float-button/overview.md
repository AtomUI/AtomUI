# FloatButton 桌面版架构设计

本文档定义 `FloatButton` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [FloatButton 桌面版实现原理](implementation.md)，FloatButton Token 的专项设计见 [FloatButton Token 设计](token.md)，设计和契约变化记录见 [FloatButton Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/General/FloatButton` |
| 控件状态 | Stable |

FloatButton 是 AtomUI 桌面控件体系中的悬浮动作按钮，用于页面或窗口边缘提供全局快捷操作、回到顶部、帮助和工具入口。

FloatButton 不负责普通表单提交按钮、菜单导航或复杂工具栏。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/FloatButton`
- `src/AtomUI.Controls/FloatButton`

## 2. 设计语言

FloatButton 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | FloatButton 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | FloatButton 是 AtomUI 桌面控件体系中的悬浮动作按钮，用于页面或窗口边缘提供全局快捷操作、回到顶部、帮助和工具入口。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `CloseIcon`、`Description`、`DescriptionTemplate`、`Icon`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | open/close、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | FloatButton Token + ControlTheme。 |

## 3. API 与契约模型

FloatButton 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `CloseIcon`、`Description`、`DescriptionTemplate`、`Icon` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 选择与集合 | `BadgeCount`、`BadgeOverflowCount`、`IsTriggerMode` | 维护选择、展开、过滤、分页、分组或集合状态。 |
| 命令与动作 | `Command`、`CommandParameter`、`Href` | 通过 Avalonia `Button` 命令语义触发业务动作；host 只做投影转发，不自行执行命令。 |
| 交互与状态 | `IsBadgeEnabled`、`IsDotBadge`、`IsMotionEnabled`、`IsOpen`、`IsShowProgress` | 表达用户可观察状态、可用性、清除、加载或反馈语义；`FloatButtonGroup.IsOpen` 与 `FloatButtonGroupHost.IsOpen` 默认双向绑定；`IsShowProgress` 是 BackTop 进度环显示开关。 |
| 视觉与布局 | `BadgeColor`、`BadgeOffset`、`BoxShadow`、`FloatOffsetX`、`FloatOffsetY`、`MenuPlacement`、`Orientation`、`Placement`、`SeparatorBrush`、`Shape` 等 12 项 | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 动效与异步 | `MenuMotionDuration`、`MotionDuration`、`ToTopDuration` | 约束动效开关、异步加载、播放速度、超时和任务边界。 |
| 其他稳定入口 | `ButtonType`、`Target`、`Tooltip`、`Trigger` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

稳定事件包括 `Clicked`、`Closed`、`Opened`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

`FloatButton` 本体继承 Avalonia `Button` 命令契约，`Command`、`CommandParameter`、`CanExecute` 和禁用状态必须遵循 Avalonia 原生按钮语义。`FloatButtonHost` 与 `BackTopFloatButtonHost` 是 overlay host/adaptor，公开的 `Command` 和 `CommandParameter` 必须投影到运行时创建的真实按钮；host 不直接调用 `ICommand.Execute`，也不复制一套命令状态机。

`FloatButtonGroupHost` 的 `Trigger` 是展开/收起语义，不默认承载业务 `Command`。业务动作应绑定在 group 内部的 `FloatButton` 子项上；这些子项必须能继承 host 的 `DataContext`，同时不得覆盖子项显式设置的本地 `DataContext`。

`IsOpen` 是用户可拥有的受控打开状态，Avalonia Binding 默认使用 `TwoWay`；hover、click 和 host 转接触发的打开/关闭都必须回写同一 public 属性。该状态不是 Form value，不写入 `DataValidationErrors`。

`IsShowProgress` 是回到顶部进度环的显示开关，类型 `bool`，默认 `false`，语义对齐上游参考实现 `FloatButton.BackTop` 的 `showProgress`（版本注记见专项模型与变更记录）。该属性注册在 `AbstractBackTopFloatButton` 上并由 `BackTopFloatButton` 直接继承；`AbstractBackTopFloatButtonHost` 通过 `AddOwner` 暴露同一属性，`BackTopFloatButtonHost` 将其投影到 overlay 中创建的真实按钮。开关只控制进度环显隐，不改变 `Target`、`VisibilityHeight` 与回到顶部动作语义。

主要公开类型与枚举：

- 类型：`AbstractBackTopFloatButton`、`AbstractBackTopFloatButtonHost`、`AbstractFloatButton`、`AbstractFloatButtonHost`、`BackTopFloatButton`、`BackTopFloatButtonHost`、`FloatButton`、`FloatButtonGroup`、`FloatButtonGroupHost`、`FloatButtonHost`、`FloatButtonItemsControl`、`FloatButtonSeparatorLayer`。
- 枚举：`FloatButtonGroupMenuPlacement`、`FloatButtonGroupTrigger`、`FloatButtonPlacement`、`FloatButtonShape`、`FloatButtonType`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_BadgeLayout` | `Canvas` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_ItemsLayout` | `?` | 承载集合项、布局面板或虚拟化内容。 |

控件专属或内部伪类包括 `ButtonPseudoClass.IconOnly`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 4. 行为与状态模型

FloatButton 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- `Command.CanExecute=false` 必须通过 Avalonia Button 语义反映为不可执行状态；BackTop 场景下不可执行命令也应阻止回到顶部动作。
- open/close、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- `IsShowProgress` 开启时，滚动进度由 `Target` 滚动几何派生为 0~1 的 internal 状态；进度值是派生状态，不通过 public API 暴露，也不参与命令或显隐阈值语义。
- `IsOpen` 是默认 `TwoWay` 的受控状态；交互路径应使用 current value 语义更新，不得用样式优先级写入破坏用户绑定。
- Host 创建的 overlay 按钮只能接收 host 公共属性投影；命令执行、`CanExecute`、事件顺序和禁用状态仍由真实 `FloatButton` 作为交互 owner。
- Group host 搬移子按钮到 overlay group 时必须保留原有绑定语义，使未设置本地 `DataContext` 的子项继承 host 数据上下文。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

FloatButton 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `AbstractFloatButtonTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `BackTopFloatButtonHostTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `BackTopFloatButtonTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `FloatButtonGroupHostTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `FloatButtonGroupTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `FloatButtonHostTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `FloatButtonItemsControlTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `FloatButtonTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |

FloatButton 使用 `FloatButtonToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 open/close、motion、visual option 运行时状态。

BackTop 进度环不使用控件专属 Token：`BackTopFloatButtonTheme.axaml` 中的进度环节点直接引用全局 SharedToken（`LineWidthBold`、`ColorBorderSecondary`、`ColorPrimary`），显隐由 `IsShowProgress` 驱动的主题 selector 控制。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

FloatButton 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `AbstractBackTopFloatButton`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractBackTopFloatButtonHost`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractFloatButton`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractFloatButtonHost`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractFloatButtonTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `BackTopFloatButton`：动作触发类型，负责点击、导航或局部操作状态。
- `BackTopFloatButtonHost`：模板协作类型，承载内容展示、宿主或视觉边界。
- `FloatButton`：动作触发类型，负责点击、导航或局部操作状态。
- `FloatButtonGroup`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `FloatButtonGroupHost`：模板协作类型，承载内容展示、宿主或视觉边界。
- `FloatButtonHost`：模板协作类型，承载内容展示、宿主或视觉边界。
- `FloatButtonItemsControl`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `FloatButtonSeparatorLayer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `FloatButtonToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。
- Host 家族负责把 public API 投影到 overlay 中真实交互控件；交互控件本体仍是命令、点击、禁用和焦点语义 owner。

## 7. 兼容性不变量

维护 FloatButton 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- 不手动绕过 Avalonia Button 命令机制执行 `ICommand`；所有命令状态必须由真实 `FloatButton` 统一承载。
- 不让 overlay 搬移破坏 group 子按钮的 `DataContext` 继承，也不覆盖子项显式设置的本地数据上下文。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 选择与当前项模型

FloatButton 的当前项状态必须由单一 owner 推导。public 选择属性、集合项容器和伪类之间只能做单向同步，集合替换、清空和模板重套用时必须回放当前状态。

### 8.2 弹层与宿主模型

FloatButton 涉及弹层、窗口或 overlay 宿主时，打开状态、取消事件、定位和宿主释放必须保持一致。重复打开、关闭、窗口失活和 template reapply 都必须释放旧宿主引用。

### 8.3 动效模型

FloatButton 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.4 视觉选项模型

FloatButton 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

### 8.5 命令与 overlay 投影模型

FloatButton 家族的业务命令只属于真实交互按钮。普通 `FloatButton` 直接使用 Avalonia Button 命令模型；host 类型只负责把 `Command`、`CommandParameter`、视觉属性和数据上下文投影给 overlay 中创建的真实按钮。`FloatButtonGroupHost` 的 trigger 负责展开/收起，不默认表达业务动作命令；group 内部子 `FloatButton` 通过继承 host `DataContext` 绑定业务命令。

该模型保证 host、overlay 和 group 搬移不会形成第二套命令系统，也不会让 `CanExecute`、禁用状态、点击事件顺序和 BackTop 行为出现分裂。

### 8.6 回到顶部进度环模型

`IsShowProgress` 开启后，`BackTopFloatButton` 在按钮边缘显示滚动进度环：进度从 `Target` 的滚动几何派生为 0~1 的 internal 状态，视觉由 `BackTopFloatButtonTheme.axaml` 中的进度环节点表达，线宽与颜色取全局 Token。该模型对齐 antd 6.6.0 `FloatButton.BackTop` 的 `showProgress`。进度环只提供滚动反馈，不改变 `VisibilityHeight` 显隐阈值、命令语义和动效模型；开关关闭时进度环默认隐藏。

## 9. 文档导航与验证策略

关联文档：

- [FloatButton 桌面版实现原理](implementation.md)
- [FloatButton Token 设计](token.md)
- [FloatButton Semantic Part 契约](semantic-part.md)
- [FloatButton Changelog](changelog.md)

`BackTopFloatButton` 的 `root/icon/content` 与 `FloatButtonGroup` 的 `root/trigger/list` descriptor 与主控件分开维护，
完整契约见 [FloatButton Semantic Part 契约](semantic-part.md)。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 open/close、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
