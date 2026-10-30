# Empty 桌面版架构设计

本文档定义 `Empty` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见
[控件研发标准](../../../../engineering/development/control-development-guidelines.md)，公开主题区域见
[Empty Semantic Part 契约](semantic-part.md)，内部实现原理见 [Empty 桌面版实现原理](implementation.md)，Empty Token 的专项设计见
[Empty Token 设计](token.md)，设计和契约变化记录见 [Empty Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Empty` |
| 控件状态 | Stable |

Empty 是 AtomUI 桌面控件体系中的空状态控件，用于表达无数据、无结果或占位状态。

Empty 不负责加载状态、错误状态或业务异常处理。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Controls/Empty`
- `src/AtomUI.Desktop.Controls/Empty`

## 2. 设计语言

Empty 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Empty 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Empty 是 AtomUI 桌面控件体系中的空状态控件，用于表达无数据、无结果或占位状态。 |
| 内容承载 | 用户数据、展示内容或操作入口如何进入控件。 | `Description`、`Footer`、`FooterTemplate`、`ImagePath`、`ImageSource`、`PresetImage`。 |
| 状态反馈 | public API 和模板绑定如何形成用户可感知反馈。 | 描述与 Footer 可见性、图片来源、三档 SizeType。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Empty Token + ControlTheme。 |

## 3. API 与契约模型

Empty 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Description`、`Footer`、`FooterTemplate`、`ImagePath`、`ImageSource`、`IsDescriptionVisible`、`PresetImage` | 定义空状态图片、描述和后续操作内容。 |
| 视觉与布局 | `SizeType`、`StrokeDashArray` | `SizeType` 选择预设尺寸基线；`StrokeDashArray` 配置 root 边框的虚线节奏。 |

当前没有抽取到控件专属 public 事件；交互通知主要来自继承事件、命令或 Gallery 可观察状态。

主要公开类型与枚举：

- 类型：`AbstractEmpty`、`Empty`。
- 枚举：`PresetEmptyImage`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_SvgImage` | `Avalonia.Svg.Svg` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

Empty 公开 `root`、`image`、`description`、`footer` 四个 Semantic Part。完整 Selector、ContractType、cardinality 和定制边界见
[Empty Semantic Part 契约](semantic-part.md)。`Footer` 与 `FooterTemplate` 是 6.0 新增的公共内容入口，用于承载创建、刷新、
返回等空状态后续操作；`StrokeDashArray` 为 root 表面提供可绑定的虚线边框入口。Empty 保持
`TemplatedControl` 基类不变。

## 4. 行为与状态模型

Empty 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- `PresetImage`、`ImagePath`、`ImageSource` 三者互斥，并始终更新同一个 image 模板节点。
- `IsDescriptionVisible` 直接控制 description 模板节点的可见性，不通过 C# 动态创建或删除 Visual。
- `Footer=null` 时 footer Presenter 隐藏；非空时由 `FooterTemplate` 或 Avalonia DataTemplate 机制生成内容。
- Large、Middle、Small 只改变 image 高度和描述间距，不改变 Semantic marker 数量。
- 模板重套用时必须把 public API 对应状态回放到新的 part 和主题变量。

## 5. 视觉与主题模型

Empty 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `EmptyTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

Empty 使用 `EmptyToken` 作为控件 Token scope。Token 只表达图片高度、描述间距、Footer 间距和图形颜色等视觉语义，不承载
实例内容或可见性状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Empty 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `AbstractEmpty`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `Empty`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `EmptyToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Empty 时必须保持以下不变量：

- 除已经批准的 `Footer`、`FooterTemplate` 外，不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 图片来源模型

`PresetImage`、`ImagePath` 和 `ImageSource` 是同一 image 职责的三个互斥输入。它们只更新 `PART_SvgImage` 的绘制来源，不替换
Semantic target。内置 Default/Simple 图形的颜色由 EmptyToken 与 SharedToken 计算，调用方不能同时设置多个图片来源。

### 8.2 描述与 Footer 模型

`Description` 负责本地化默认描述或调用方文本；`IsDescriptionVisible` 负责显示状态。`Footer` 与 `FooterTemplate` 负责 Empty
内部的后续操作区域，避免 Gallery 或业务页面通过外部 StackPanel 模拟一个不属于 Empty owner 的 footer。

## 9. 文档导航与验证策略

关联文档：

- [Empty 桌面版实现原理](implementation.md)
- [Empty Semantic Part 契约](semantic-part.md)
- [Empty Token 设计](token.md)
- [Empty Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖三种图片来源、描述可见性、Footer 内容替换和三档 SizeType。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
