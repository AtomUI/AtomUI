# Icon 桌面版架构设计

本文档定义 `Icon` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Icon 桌面版实现原理](implementation.md)，Icon Token 的专项设计见 [Icon Token 设计](token.md)，设计和契约变化记录见 [Icon Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/General/Icon` |
| 控件状态 | Stable |

Icon 是 AtomUI 桌面控件体系中的图标渲染控件，用于在按钮、菜单、表格和独立图标场景中稳定呈现矢量图标。

Icon 不负责业务图标语义、图标资源注册和图标字体打包。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Controls/Icon`

## 2. 设计语言

Icon 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Icon 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Icon 是 AtomUI 桌面控件体系中的图标渲染控件，用于在按钮、菜单、表格和独立图标场景中稳定呈现矢量图标。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Icon`、`IconBrush`、`IconTemplate`；Presenter 的 `IconBrush` 表达宿主单色前景投影。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | 基础交互和主题状态。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Icon Token + ControlTheme。 |

## 3. API 与契约模型

Icon 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Icon`、`IconBrush`、`IconTemplate` | 定义控件展示内容、输入数据、模板或业务对象入口；Presenter 场景下 `IconBrush` 会投影到 AtomUI `Icon` 的主画刷、辅助画刷和 fallback 画刷槽位。 |

当前没有抽取到控件专属 public 事件；交互通知主要来自继承事件、命令或 Gallery 可观察状态。

主要公开类型与枚举：

- 类型：`IconFuncTemplate`、`IconPresenter`、`IconTemplate`、`IconTemplatePresenter`。
- 枚举：无。

稳定 template part：

当前控件没有显式 `[TemplatePart]` 契约；主题节点仍通过 ControlTheme key、资源 key 和 Gallery 可观察行为形成稳定边界。

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

## 4. 行为与状态模型

Icon 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- 基础交互和主题状态 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- `IconPresenter` 和 `IconTemplatePresenter` 的 `IconBrush` 是宿主控件单色前景状态入口；当承载对象是 AtomUI `Icon` 时，应同步到 `StrokeBrush`、`FillBrush`、`SecondaryStrokeBrush`、`SecondaryFillBrush` 和 `FallbackBrush`，保证 Button、Menu、List 等宿主的 hover、pressed、disabled 前景状态能够完整接管多画刷图标。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

Icon 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `IconPresenterTheme.axaml` | 定义布局、内容承载或框架节点视觉。 |
| `IconTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `PathIconTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

Icon 使用 `IconToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 基础交互和主题状态 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Icon 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `IconFuncTemplate`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `IconPresenter`：模板协作类型，承载内容展示、宿主或视觉边界。
- `IconTemplate`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `IconTemplatePresenter`：模板协作类型，承载内容展示、宿主或视觉边界。
- `IconToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Icon 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

不适用。Icon 当前没有独立于公共 API、状态模型和主题模型之外的专项模型；维护重点是保持模板契约和资源边界稳定。

## 9. 文档导航与验证策略

关联文档：

- [Icon 桌面版实现原理](implementation.md)
- [Icon Token 设计](token.md)
- [Icon Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 基础交互和主题状态、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
