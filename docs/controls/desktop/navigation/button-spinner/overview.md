# ButtonSpinner 桌面版架构设计

本文档定义 `ButtonSpinner` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，公开语义区域契约见 [ButtonSpinner Semantic Part 契约](semantic-part.md)，内部实现原理见 [ButtonSpinner 桌面版实现原理](implementation.md)，ButtonSpinner Token 的专项设计见 [ButtonSpinner Token 设计](token.md)，设计和契约变化记录见 [ButtonSpinner Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Navigation/ButtonSpinner` |
| 控件状态 | Stable |

ButtonSpinner 是 AtomUI 桌面控件体系中的带步进按钮的输入基座，用于承载增减、选择或候选切换类输入。

ButtonSpinner 不负责普通按钮、完整数字输入或 ComboBox 选择器。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/ButtonSpinner`

## 2. 设计语言

ButtonSpinner 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | ButtonSpinner 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | ButtonSpinner 是 AtomUI 桌面控件体系中的带步进按钮的输入基座，用于承载增减、选择或候选切换类输入。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `ContentLeftShift`、`ContentPadding`、`ContentRightShift`、`InnerLeftContent`、`InnerLeftContentTemplate`、`InnerRightContent`、`InnerRightContentTemplate`、`LeftAddOnTemplate` 等 10 项。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | ButtonSpinner Token + ControlTheme。 |

## 3. API 与契约模型

ButtonSpinner 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `ContentLeftShift`、`ContentPadding`、`ContentRightShift`、`InnerLeftContent`、`InnerLeftContentTemplate`、`InnerRightContent`、`InnerRightContentTemplate`、`LeftAddOnTemplate`、`RightAddOnTemplate`、`SpinnerContent` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 交互与状态 | `IsButtonSpinnerFloatable`、`IsButtonSpinnerVisible`、`IsHandleFloatable`、`IsMotionEnabled`、`IsShowHandle`、`IsSpinEnabled`、`Status` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `HandleOffset`、`SizeType`、`SpinnerBorderThickness`、`SpinnerHandleWidth`、`StyleVariant` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 其他稳定入口 | `ButtonSpinnerLocation`、`HandleOpacity`、`LeftAddOn`、`RightAddOn` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

当前没有抽取到控件专属 public 事件；交互通知主要来自继承事件、命令或 Gallery 可观察状态。

主要公开类型与枚举：

- 类型：`ButtonSpinner`、`ButtonSpinnerContentPanel`、`ButtonSpinnerDecoratedBox`、`ButtonSpinnerHandle`。
- 枚举：`ButtonSpinnerLocation`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_ContentFrame` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_DecoratedBox` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_DecreaseButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_IncreaseButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_SpinnerHandle` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

控件专属或内部伪类包括 `ButtonSpinnerPseudoClass.Left`、`ButtonSpinnerPseudoClass.Right`、`Left=:left`、`Right=:right`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 4. 行为与状态模型

ButtonSpinner 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

ButtonSpinner 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `ButtonSpinnerTheme.axaml` | owner 模板：帧宿主（`semantic-scope-frame` 锚点）与 `SpinnerContent` 内的步进手柄；持有 `SizeType` 圆角映射与 `SpinnerHandleWidth` 默认值。 |
| `ButtonSpinnerDecoratedBoxTheme.axaml` | 帧模板（`BasedOn` 共享 `AddOnDecoratedBoxTheme`）：左右 addon 区、内容框、内容左/右槽与浮动手柄 presenter；`SizeType` padding 与 `MinHeight` 基线。 |
| `ButtonSpinnerHandleTheme.axaml` | 步进手柄模板：增加/减少按钮、手柄自身的背景与分隔线描边绘制、variant 与 disabled 状态视觉。 |

ButtonSpinner 使用 `ButtonSpinnerToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

ButtonSpinner 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `ButtonSpinner`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ButtonSpinnerContentPanel`：布局面板，负责测量、排列、虚拟化或集合内容布局。
- `ButtonSpinnerDecoratedBox`：模板协作类型，承载内容展示、宿主或视觉边界。
- `ButtonSpinnerHandle`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ButtonSpinnerToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 ButtonSpinner 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 动效模型

ButtonSpinner 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.2 视觉选项模型

ButtonSpinner 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [ButtonSpinner 桌面版实现原理](implementation.md)
- [ButtonSpinner Token 设计](token.md)
- [ButtonSpinner Changelog](changelog.md)

七个部件均为 `Single`，通过生成的强类型 Semantic Style 定制。完整 Selector、route、`ContractType`、状态矩阵与
布局基线见 [ButtonSpinner Semantic Part 契约](semantic-part.md)。

公开语义区域的准入范围映射上游 `InputNumber` 已公开并实际消费的 `root` / `prefix` / `suffix` / `input` / `actions`
键：帧内主内容区对应 `input` 职责（本控件不拥有文本编辑面，故发布为 `content`），帧内容左/右槽对应 `prefix` / `suffix`
职责（为不破坏 `NumericUpDown` 已发布的 `Single` route，改用与公开 API 同名的 `innerLeftContent` /
`innerRightContent`），步进按钮区对应 `actions`。单个上/下按钮上游没有公开语义键，属显式能力补充。
`LeftAddOn` / `RightAddOn` 外部区域不发布部件，与输入家族保持一致。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
