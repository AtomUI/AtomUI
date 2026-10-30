# Alert 桌面版架构设计

本文档定义 `Alert` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，公开主题区域见 [Alert Semantic Part 契约](semantic-part.md)，内部实现原理见 [Alert 桌面版实现原理](implementation.md)，Alert Token 的专项设计见 [Alert Token 设计](token.md)，设计和契约变化记录见 [Alert Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Feedback/Alert` |
| 控件状态 | Stable |

Alert 是 AtomUI 桌面控件体系中的警告提示控件，用于展示页面内的成功、信息、警告和错误反馈。

Alert 不负责全局消息系统、模态确认或通知中心。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Alert`

## 2. 设计语言

Alert 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Alert 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Alert 是 AtomUI 桌面控件体系中的警告提示控件，用于展示页面内的成功、信息、警告和错误反馈。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `CloseIcon`、`Description`、`IsShowIcon`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | 基础交互和主题状态。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Alert Token + ControlTheme。 |

## 3. API 与契约模型

Alert 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Message`、`Description`、`ExtraAction`、`CloseIcon` | 定义标题、详情、辅助操作和关闭图标内容。 |
| 交互与状态 | `Type`、`IsShowIcon`、`IsClosable`、`IsMessageMarqueeEnabled` | 表达反馈类型、图标、关闭入口和标题呈现状态。 |
| 视觉表面 | `StrokeDashArray` 与继承的 TemplatedControl 表面属性 | 支持 root Semantic Style 定制实线或虚线边框、背景、圆角与 padding。 |

`CloseRequest` 是控件专属 public 事件。点击 `PART_CloseBtn` 时触发该事件；Alert 不自动移除自身，也不拥有关闭动画或业务关闭策略。

主要公开类型与枚举：

- 类型：`Alert`。
- 枚举：`AlertType`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_CloseBtn` | `IconButton` | 关闭请求入口；模板重套用时解除旧订阅并订阅新 part。 |

控件专属或内部伪类包括 `AlertPseudoClass.HasDescription`、`AlertPseudoClass.HasExtraAction`、`HasDescription=:has-description`、`HasExtraAction=:has-extra-action`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 4. 行为与状态模型

Alert 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- `Type` 决定背景、边框和当前可见的状态图标。
- `Description`、`ExtraAction`、`IsShowIcon`、`IsClosable` 与 `IsMessageMarqueeEnabled` 只切换静态模板节点状态，不改变 Semantic Part 数量。
- `CloseRequest` 由当前模板的 close button 触发，业务层决定是否隐藏、移除或替换 Alert。
- 模板重套用时必须解除旧 close button 订阅，并把 public API 对应状态投影到新模板。

## 5. 视觉与主题模型

Alert 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `AlertTheme.axaml` | 提供单一静态模板、状态 selector、Token 绑定和 Semantic marker。 |

Alert 使用 `AlertToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 基础交互和主题状态 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不删除或重命名 `root`、`icon`、`section`、`title`、`description`、`actions`、`close`，不改变 selector class、ContractType 或 cardinality。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Alert 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `Alert`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `AlertToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- Alert 没有 ItemsSource、Popup、Flyout、Window host 或独立 motion owner；不要为关闭请求引入隐式全局宿主或自动移除语义。
- Gallery Semantic Preview 通过静态 descriptor 展示 Part，并保持首次选择 Semantic Parts Tab 前零 Preview 实例。

## 7. 兼容性不变量

维护 Alert 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不破坏七个 Semantic Part 的名称、selector、ContractType、cardinality 和 marker 身份。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. Semantic Part

Alert 公开 `root`、`icon`、`section`、`title`、`description`、`actions` 和 `close`。完整 Selector、Style Type、数量语义、
状态矩阵与排除边界见 [Alert Semantic Part 契约](semantic-part.md)。

## 9. 文档导航与验证策略

关联文档：

- [Alert 桌面版实现原理](implementation.md)
- [Alert Semantic Part 契约](semantic-part.md)
- [Alert Token 设计](token.md)
- [Alert Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 基础交互和主题状态、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查 Semantic Preview、owner-scoped Style 示例、源码片段入口和延迟实例化。 |
