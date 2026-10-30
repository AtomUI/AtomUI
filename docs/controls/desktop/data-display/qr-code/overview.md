# QRCode 桌面版架构设计

本文档定义 `QRCode` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见
[控件研发标准](../../../../engineering/development/control-development-guidelines.md)，公开主题区域见
[QRCode Semantic Part 契约](semantic-part.md)，内部实现原理见 [QRCode 桌面版实现原理](implementation.md)，QRCode Token 的专项设计见
[QRCode Token 设计](token.md)，设计和契约变化记录见 [QRCode Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/QRCode` |
| 控件状态 | Stable |

QRCode 是 AtomUI 桌面控件体系中的二维码控件，用于把文本或业务字符串渲染为可扫描二维码。

QRCode 不负责条码识别器、扫码硬件接口或业务短链服务。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/QRCode`
- `src/AtomUI.Controls/QRCode`

## 2. 设计语言

QRCode 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | QRCode 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | QRCode 是 AtomUI 桌面控件体系中的二维码控件，用于把文本或业务字符串渲染为可扫描二维码。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `ExpiredContent`、`ExpiredContentTemplate`、`Icon`、`IconBgColor`、`IconSize`、`LoadingContent`、`LoadingContentTemplate`、`ScannedContent` 等 10 项。 |
| 状态反馈 | public API、内部状态和模板节点如何形成用户可感知反馈。 | `Active`、`Loading`、`Expired`、`Scanned` 与刷新请求。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | QRCode Token + ControlTheme。 |

## 3. API 与契约模型

QRCode 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `ExpiredContent`、`ExpiredContentTemplate`、`Icon`、`IconBgColor`、`IconSize`、`LoadingContent`、`LoadingContentTemplate`、`ScannedContent`、`ScannedContentTemplate`、`Value` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 交互与状态 | `IsBordered`、`Status`、`RefreshRequested` | 表达边框模式、状态遮罩和过期状态下的刷新请求。 |
| 视觉与布局 | `Color`、`Size` 及继承的 root 表面属性 | `Size` 统一拥有二维码方形边长；颜色、背景、边框、圆角和 Padding 形成 root 视觉。 |
| 其他稳定入口 | `EccLevel` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

`RefreshRequested` 是 QRCode 的控件专属 public 事件，由默认过期状态中的 `PART_RefreshButton` 触发。自定义
`ExpiredContent` 不会自动转发该事件，调用方需要在自定义内容中显式处理自己的命令或事件。

主要公开类型与枚举：

- 类型：`AbstractQRCode`、`QRCode`。
- 枚举：`QRCodeEccLevel`、`QRCodeStatus`。
- 本地化 Catalog：`QRCodeLangResourceKind`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_RefreshButton` | `Button` | 承载用户触发入口、导航或关闭动作。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

QRCode 公开 `root` 和 `cover` 两个 Semantic Part。`root` 是 QRCode owner；`cover` 是状态遮罩与状态内容共同占用的静态 overlay
区域。完整 Selector、ContractType、cardinality 和定制边界见 [QRCode Semantic Part 契约](semantic-part.md)。二维码 bitmap、中心图标、
刷新按钮和各状态内部内容不单独公开为 Semantic Part。

## 4. 行为与状态模型

QRCode 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- `Active` 隐藏 cover；`Loading`、`Expired`、`Scanned` 显示同一个 cover 节点，并在节点内部切换对应状态内容。
- `Value`、`Color`、`EccLevel` 或 `Size` 变化时重新生成透明背景 bitmap，不替换 Semantic target；`Background` 只更新 root 表面。
- `Icon` 只控制二维码中心图标内容，不增加 Semantic Part，也不改变 root/cover 数量。
- 模板重套用时重新接入 `PART_RefreshButton`，先移除旧按钮订阅，再回放当前 public API 状态。
- `Size` 是二维码外框与绘制源的统一边长；Semantic Style 不建立第二套 Width/Height 尺寸 owner。

## 5. 视觉与主题模型

QRCode 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `QRCodeTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

QRCode 使用 `QRCodeToken` 作为控件 Token scope。Token 只表达文字色和 cover 背景色等视觉语义，不承载 `Status`、`Value` 或 bitmap
运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

QRCode 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `AbstractQRCode`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `QRCode`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `QRCodeToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `QRCodeLangResourceKind`：稳定的本地化 Catalog enum；内置翻译由同目录三种语言 XLIFF 提供并在编译期生成。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 QRCode 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级；Semantic 示例必须保持与对应公开上游示例一致。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 方形尺寸与绘制模型

`Size` 同时决定二维码 bitmap 的像素边长和控件 root 的方形边长。root 的 Border、Padding 与 CornerRadius 位于该固定方形内部，二维码
图像在扣除边框与 Padding 后的内容区域内缩放。调用方通过 `Size` 改变整体尺寸，不通过 Semantic Style 的 Width/Height 建立另一套尺寸源。

### 8.2 状态 cover 模型

cover 是一个静态、单一的 overlay 区域。`Active` 状态下它保留在模板中但不可见；其他状态下它覆盖完整 root，并承载 Loading、Expired
或 Scanned 内容。该结构避免状态切换时创建或销毁 Visual，也保证 Semantic marker 身份稳定。

## 9. 文档导航与验证策略

关联文档：

- [QRCode 桌面版实现原理](implementation.md)
- [QRCode Semantic Part 契约](semantic-part.md)
- [QRCode Token 设计](token.md)
- [QRCode Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 Active/Loading/Expired/Scanned、刷新事件、三组自定义状态内容、bitmap 更新和 re-template。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
