# QRCode 桌面版实现原理

本文档描述 QRCode 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见
[QRCode 桌面版架构设计](overview.md)，公开主题区域见 [QRCode Semantic Part 契约](semantic-part.md)，变化记录见
[QRCode Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [QRCode Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 QRCode 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/QRCode/Localization/QRCodeLangResourceKind.cs`
- `src/AtomUI.Desktop.Controls/QRCode/Localization/en-US.xlf`
- `src/AtomUI.Desktop.Controls/QRCode/Localization/zh-CN.xlf`
- `src/AtomUI.Desktop.Controls/QRCode/Localization/zh-TW.xlf`
- `src/AtomUI.Desktop.Controls/QRCode/QRCode.cs`
- `src/AtomUI.Desktop.Controls/QRCode/QRCode.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/QRCode/QRCodeToken.cs`
- `src/AtomUI.Desktop.Controls/QRCode/Themes/QRCodeTheme.axaml`
- `src/AtomUI.Controls/QRCode/AbstractQRCode.cs`
- `src/AtomUI.Controls/QRCode/QRCodeEnums.cs`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `AbstractQRCode`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `QRCode`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `QRCodeToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `QRCodeLangResourceKind`：稳定的本地化 Catalog enum；三个 XLIFF 文件提供随模块发布的内置翻译，生成器负责编译资源表和 XAML 扩展。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

QRCode 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`ExpiredContent`、`ExpiredContentTemplate`、`Icon`、`IconBgColor`、`IconSize`、`LoadingContent`、`LoadingContentTemplate`、`ScannedContent`、`ScannedContentTemplate`、`Value`。
- 交互与状态：`IsBordered`、`Status`、`RefreshRequested`。
- 视觉与布局：`Color`、`Size` 及继承的 Background、Border、CornerRadius、Padding。
- 其他稳定入口：`EccLevel`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- `Status` 是 cover 可见性和状态内容选择的唯一 owner，不在状态节点之间维护第二套状态。
- bitmap 由 `AbstractQRCode` 单一 owner 生成和释放；属性变化替换 bitmap 时必须释放旧实例。
- `PART_RefreshButton` 的订阅必须在每次 re-template 前解除，再连接新模板按钮。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_RefreshButton`：承载用户触发入口、导航或关闭动作。

Semantic marker 与真实节点映射：

| Part | Marker 节点 | 创建方式 | 状态 |
| --- | --- | --- | --- |
| `root` | `QRCode` owner | 控件实例 | 始终存在，不使用 `.semantic-root`。 |
| `cover` | 状态 overlay `Border` | ControlTemplate 静态创建 | `Active` 隐藏，其他状态显示；marker 身份不变。 |

`cover` 使用静态 `Classes.semantic-cover="True"`。默认主题不能使用该 class 驱动自身视觉；生成的 `QRCodeCoverStyle` 只供应用侧
Semantic 定制。

## 6. 交互与事件处理

QRCode 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 输入类路径必须保持 Form、validation、clear、placeholder 和键盘行为一致。

默认过期内容中的 `PART_RefreshButton` 把 Click 收敛为 `RefreshRequested`。自定义 `ExpiredContent` 由调用方拥有自己的交互，不依赖
模板内部按钮。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

1. `SetupQRCode()` 以 `Value`、`EccLevel`、`Size`、`Color`、`Icon` 和 `IconSize` 生成透明背景的方形 Skia bitmap，并在替换时释放旧 bitmap。
2. 存在 Icon 时，bitmap 在 `IconSize` 对应的中心区域执行透明挖空；模板中的透明 Icon frame 让 root Background 自然透出，不额外叠加背景色。
3. 模板 root 使用固定 `Size × Size` 的 `PixelAlignedBorder`。内部内容表面消费 owner `Padding`，Viewbox 让二维码与中心图标在统一的 `Size × Size` 坐标面中缩放。
4. owner `Background` 只由 root 表面绘制，不写入 bitmap，避免半透明背景在二维码内容区重复叠加。
5. cover 位于 Padding 外侧并覆盖完整 root；其 Background、CornerRadius、Opacity 等属性可由 `QRCodeCoverStyle` 定制。
6. `Status` selector 只切换同一个 cover 和三个静态状态布局的可见性，不创建、删除或重新标记 Visual。
7. `OnApplyTemplate` 先解除旧 `PART_RefreshButton` 事件，再接入新按钮并回放 bitmap。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 状态切换不得重新生成 Semantic marker、cover 或状态内容容器；只有二维码绘制输入变化时才重新生成 bitmap。

## 9. 维护不变量

维护 QRCode 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- `root`、`cover` 的名称、selector、ContractType、cardinality 和静态 marker 身份。
- `Size` 对外框与 bitmap 的统一方形尺寸 ownership。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

QRCode 专项测试必须覆盖 descriptor、静态 marker、四种 Status、状态内容替换、刷新事件、root Padding/边框投影、cover Style、Size
方形基线、bitmap 更新和 re-template。QRCode 不涉及 Popup、跨 VisualRoot 或运行时 marker；单控件阶段不要求额外 NativeAOT publish，
批次收尾仍执行 Gallery NativeAOT 验证。
