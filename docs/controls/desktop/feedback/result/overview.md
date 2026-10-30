# Result 桌面版架构设计

本文档定义 `Result` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，公开主题区域见 [Result Semantic Part 契约](semantic-part.md)，内部实现原理见 [Result 桌面版实现原理](implementation.md)，Result Token 的专项设计见 [Result Token 设计](token.md)，设计和契约变化记录见 [Result Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Feedback/Result` |
| 控件状态 | Stable |

Result 是 AtomUI 桌面控件体系中的结果页控件，用于展示成功、失败、警告和信息结果状态。

Result 不负责表单验证器、通知系统或异常处理框架。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Controls/Result`
- `src/AtomUI.Desktop.Controls/Result`

## 2. 设计语言

Result 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Result 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Result 是 AtomUI 桌面控件体系中的结果页控件，用于展示成功、失败、警告和信息结果状态。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `ExtraTemplate`、`Header`、`HeaderFontSize`、`HeaderTemplate`、`Icon`、`SubHeader`、`SubHeaderFontSize`、`SubHeaderTemplate`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Result Token + ControlTheme。 |

## 3. API 与契约模型

Result 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Header`、`HeaderTemplate`、`SubHeader`、`SubHeaderTemplate`、`Extra`、`ExtraTemplate`、`Content`、`ContentTemplate` | 定义标题、副标题、操作区域和正文内容入口。 |
| 状态与图标 | `Status`、`Icon` | 选择普通反馈图标或 403/404/500 异常图，并允许普通状态使用自定义 `PathIcon`。 |
| 排版 | `HeaderFontSize`、`SubHeaderFontSize` | 覆盖标题与副标题字号；行高仍由控件根据相对行高计算。 |
| 根表面 | 继承的 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding` 与 `StrokeDashArray` | 由根 owner Style 控制 Result 的背景、边框、圆角、内边距和虚线节奏。 |

当前没有抽取到控件专属 public 事件；交互通知主要来自继承事件、命令或 Gallery 可观察状态。

主要公开类型与枚举：

- 类型：`AbstractResult`、`Result`。
- 枚举：`ResultStatus`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_ErrorCodeImage` | `Svg` | 承载 403、404、500 的异常状态图像。 |
| `PART_StatusIconPresenter` | `ContentPresenter` | 承载 Info、Success、Warning、Error 的默认或自定义状态图标。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

## 4. 行为与状态模型

Result 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- `Status` 是状态视觉的唯一 owner。Info、Success、Warning、Error 显示普通图标 presenter，403、404、500 显示异常 SVG。
- `Icon` 只替换四种普通反馈状态的图标内容，不改变 `icon` Semantic Part 的 target 身份或数量。
- `Header`、`SubHeader` 和 `Content` 的空值只改变对应 presenter 的可见性；`Extra` 为空时保留空 presenter。
- 模板重套用时重新获取两个图标 template part，并把当前状态、图标尺寸、画刷和文本行高回放到新模板。

## 5. 视觉与主题模型

Result 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `ResultTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

Result 使用 `ResultToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不删除或重命名 `root`、`icon`、`title`、`subTitle`、`extra`、`body`，不改变 selector class、ContractType 或 cardinality。
- `DashedBorder#Frame` 必须继续投影 Result 的标准根表面属性；该内部 frame 不成为独立 Part。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Result 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `AbstractResult`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `Result`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ResultIndicator`：内部 SVG source 提供者，只服务 403、404、500 异常图，不构成 public owner。
- `ResultToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- Result 没有 ItemsSource、Popup、Flyout、Window host、独立 motion owner 或 SizeType 分支。
- Gallery Semantic Preview 通过静态 descriptor 展示 Part，并保持首次选择 Semantic Parts Tab 前零 Preview 实例。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Result 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不破坏六个 Semantic Part 的名称、selector、ContractType、cardinality 和 marker 身份。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. Semantic Part

Result 公开 `root`、`icon`、`title`、`subTitle`、`extra` 和 `body`。完整 Selector、Style Type、数量语义、状态矩阵与排除边界见
[Result Semantic Part 契约](semantic-part.md)。

## 9. 文档导航与验证策略

关联文档：

- [Result 桌面版实现原理](implementation.md)
- [Result Semantic Part 契约](semantic-part.md)
- [Result Token 设计](token.md)
- [Result Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查 Semantic Preview、owner-scoped Style 示例、源码片段入口和延迟实例化。 |
