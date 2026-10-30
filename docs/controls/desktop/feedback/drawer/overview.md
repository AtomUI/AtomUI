# Drawer 桌面版架构设计

本文档定义 `Drawer` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Drawer 桌面版实现原理](implementation.md)，Drawer Token 的专项设计见 [Drawer Token 设计](token.md)，设计和契约变化记录见 [Drawer Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Feedback/Drawer` |
| 控件状态 | Stable |

Drawer 是 AtomUI 桌面控件体系中的抽屉控件，用于从窗口边缘滑入承载附加内容或操作面板。

Drawer 不负责模态对话框、普通 Popup 或 SplitView 布局容器。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Drawer`

## 2. 设计语言

Drawer 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Drawer 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Drawer 是 AtomUI 桌面控件体系中的抽屉控件，用于从窗口边缘滑入承载附加内容或操作面板。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Content`、`ContentTemplate`、`ExtraTemplate`、`FooterTemplate`、`Title`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | open/close、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Drawer Token + ControlTheme。 |

## 3. API 与契约模型

Drawer 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Content`、`ContentTemplate`、`ExtraTemplate`、`FooterTemplate`、`Title` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 交互与状态 | `IsCloseOnMaskClick`、`IsMotionEnabled`、`IsOpen`、`IsPinnedOpen`、`IsShowCloseButton`、`IsShowMask` | 表达用户可观察状态、可用性、清除、加载或反馈语义；`IsPinnedOpen` 钉住常开（语义预览用），拦截遮罩点击与关闭按钮，不拦截外部 `IsOpen` 赋值。 |
| 视觉与布局 | `DialogSize`、`Placement`、`PushOffsetPercent`、`SizeType` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 其他稳定入口 | `Extra`、`Footer`、`OpenOn` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

当前没有抽取到控件专属 public 事件；交互通知主要来自继承事件、命令或 Gallery 可观察状态。

主要公开类型与枚举：

- 类型：`Drawer`、`DrawerContainer`、`DrawerInfoContainer`。
- 枚举：`DrawerPlacement`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_CloseButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_InfoContainer` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_Surface` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_Mask` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

## 4. 行为与状态模型

Drawer 的状态流按以下路径收敛：

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
- TopLevel Drawer 使用 Window visible frame：包含 managed/drawn 标题栏，排除透明 frame shadow；该规则不按 OS 或 CSD 模式分叉。
- Drawer container 始终保留在 owning `TopLevel` 的 `ScopeAwareAdornerLayer`；Window drawn decorations 只绘制 chrome，不作为 Drawer host。

## 5. 视觉与主题模型

Drawer 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `DrawerContainerTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `DrawerInfoContainerTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

Drawer 使用 `DrawerToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 open/close、motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Drawer 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `Drawer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `DrawerContainer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `DrawerInfoContainer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `DrawerToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。
- 与 Overlay Dialog 共享 owning `TopLevel`、visible frame、`WindowVisualLayerClip` 外轮廓和 drawn chrome suppression 租约规则，但不共享容器、motion、嵌套 push 或关闭状态。

## 7. 兼容性不变量

维护 Drawer 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 不按 `OsType` 或 `IsCsdEnabled` 为 Drawer 建立平行窗口几何；Window 发布的 frame 与 titlebar metrics 是唯一几何信号。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 弹层与宿主模型

Drawer 涉及弹层、窗口或 overlay 宿主时，打开状态、取消事件、定位和宿主释放必须保持一致。重复打开、关闭、窗口失活和 template reapply 都必须释放旧宿主引用。

TopLevel Drawer 的宿主和几何遵循以下规则：

1. Drawer container 使用 placement target 所属 `ScopeAwareAdornerLayer`，保持在 owning Window `TopLevel` 的视觉树中。
2. Window Drawer 活跃时获取 drawn chrome suppression lease，使 mask 覆盖 managed/drawn titlebar 区域；最后一个 Dialog/Drawer lease 释放后恢复 chrome。
3. 无论 CSD 状态，Drawer root 和百分比 `DialogSize` 都使用 Window visible frame；visible frame 由完整 layer 只排除 `FrameShadowThickness` 得到，标题栏仍属于可用范围。
4. Window CornerRadius 只用于 Drawer root clip；窗口最终外轮廓继续由 `WindowVisualLayerClip` 统一裁剪。
5. 嵌套 Drawer 复用父 Drawer container 当前所在的 `ScopeAwareAdornerLayer`，不得重新注入平行 layer。
6. 任意 popup-bearing 内容沿 placement target 解析同一 TopLevel 的 Avalonia popup/light-dismiss 层；关闭时直接从 scope layer 移除 container，并对称释放 host geometry 与 chrome lease。直接 Popup、Flyout、ToolTip、ContextMenu 及控件家族清单统一维护在 [Modal 内容弹层叠放设计](../modal/popup-layering-design.md)，Drawer 不复制第二份清单。
7. Drawer 已打开时修改 `OpenOn`，container 必须同步迁移 adorned target、scope layer、visible-frame 订阅和 Window chrome lease；嵌套 Drawer 跟随同一 effective target，旧 Window 不得残留 suppression 引用。

### 8.2 动效模型

Drawer 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.3 视觉选项模型

Drawer 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [Drawer 桌面版实现原理](implementation.md)
- [Drawer Semantic Part 契约](semantic-part.md)
- [Drawer Token 设计](token.md)
- [Drawer Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 open/close、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Window/Popup 集成 | 运行 Drawer/Window 回归，并复用 Modal Popup 原语、控件家族、DataGrid 与入口库存测试验证共享 owning TopLevel popup 不变量。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |

当前本专项平台证据：Windows 已测试，macOS 已测试；Linux X11/Wayland 未测试。该状态只说明最终 owning
TopLevel/chrome suppression/popup 分层方案的实机证据，不代表 Drawer 的其他平台能力已被本次验收覆盖。
