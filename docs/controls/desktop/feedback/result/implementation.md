# Result 桌面版实现原理

本文档描述 Result 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Result 桌面版架构设计](overview.md)，完整 Semantic Part 公共契约见 [Result Semantic Part 契约](semantic-part.md)，变化记录见 [Result Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Result Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Result 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Controls/Result/AbstractResult.cs`
- `src/AtomUI.Controls/Result/NoFoundImage.cs`
- `src/AtomUI.Controls/Result/ResultStatus.cs`
- `src/AtomUI.Controls/Result/ServerErrorImage.cs`
- `src/AtomUI.Controls/Result/UnauthorizedImage.cs`
- `src/AtomUI.Desktop.Controls/Result/Result.cs`
- `src/AtomUI.Desktop.Controls/Result/ResultToken.cs`
- `src/AtomUI.Desktop.Controls/Result/Themes/ResultTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `AbstractResult`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `Result`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `ResultIndicator`：内部提供 403、404、500 SVG source，不参与 public API 或 Semantic descriptor。
- `ResultToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Result 的状态流遵循下面路径：

```text
Status / Icon / Header / SubHeader / Extra / Content
  -> AbstractResult public property owner
  -> StatusIcon、异常 SVG source、文本行高
  -> ResultTheme 静态 presenter / AXAML 状态 selector
  -> 可见的图标、标题、副标题、操作区和正文
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Header`、`HeaderTemplate`、`SubHeader`、`SubHeaderTemplate`、`Extra`、`ExtraTemplate`、`Content`、`ContentTemplate`。
- 状态与图标：`Status`、`Icon`。
- 排版：`HeaderFontSize`、`SubHeaderFontSize` 与内部相对行高。
- 根表面：`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding` 与 `StrokeDashArray`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- `StatusIcon` 与异常 SVG source 都由 `Status` 单向推导，template part 不反向修改 public 状态。
- Header、SubHeader、Extra 和 Content 通过 TemplateBinding 投影到固定 presenter，不创建集合或异步状态。
- 根表面属性通过 TemplateBinding 投影到静态 `DashedBorder#Frame`，不使用 C# 同步或内部 Name selector。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 模板重套用时先解除旧 `PART_StatusIconPresenter.PropertyChanged` 订阅，再订阅新 presenter。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_ErrorCodeImage`：静态 `Svg`，根据 403、404、500 状态替换 Source。
- `PART_StatusIconPresenter`：静态 `ContentPresenter`，其 Child 变化时重新应用普通图标尺寸与画刷。

Semantic marker 与真实节点映射：

| Part | 模板节点 | target 数量 | 存在与可见性 |
| --- | --- | ---: | --- |
| `icon` | `PART_StatusIconPresenter`、`PART_ErrorCodeImage` | 2 | 两者始终存在，`Status` 决定当前可见 target。 |
| `title` | `ContentPresenter#Header` | 1 | 始终存在，`Header=null` 时隐藏。 |
| `subTitle` | `ContentPresenter#SubHeader` | 1 | 始终存在，`SubHeader=null` 时隐藏。 |
| `extra` | `ContentPresenter#ExtraContent` | 1 | 始终存在；横向拉伸到内容区并让 Extra 内容居中，空内容不删除 presenter。 |
| `body` | `ContentPresenter#Content` | 1 | 始终存在，`Content=null` 时隐藏。 |

所有 marker 都由 AXAML 静态声明。状态、内容或模板数据变化不创建、删除或重新标记 target；模板重套用会整体替换 target 集合。
不带 marker 的 `DashedBorder#Frame` 只负责投影 root 的背景、边框、圆角、Padding 和虚线节奏，不构成额外 Semantic Part。

## 6. 交互与事件处理

Result 没有控件专属 pointer、keyboard、command 或 public 事件。它是纯展示 owner，不引入 Popup、全局输入捕获、自动跳转或业务重试语义；`Extra` 和 `Content` 中由调用方提供的交互控件拥有各自事件生命周期。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

1. `Status` 或 `Icon` 变化时，普通状态更新 `StatusIcon`；异常状态更新静态 SVG 的 Source。
2. `PART_StatusIconPresenter.Child` 变化时，`UpdateStatusIcon` 把 `StatusIconSize` 和 `StatusIconBrush` 应用到新 Child；`Icon` 子类同时同步 FillBrush 与 StrokeBrush。
3. `HeaderFontSize`、`SubHeaderFontSize` 或相对行高变化时重新计算绝对 `LineHeight`。
4. 模板重套用先释放旧 presenter 订阅，再获取两个新图标 part，并回放图标与文本行高状态。

Result 没有 `SizeType`。默认尺寸基线由 `FramePadding`、`StatusIconSize`、`ImageWidth`、`ImageHeight`、`HeaderFontSize`、
`SubHeaderFontSize` 与两组相对行高共同定义。Semantic Style 可以覆盖公开表面和 Part 属性，但不得依赖固定 Height、MinHeight
或像素偏移来替代这组完整基线。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。
- Semantic Part 使用生成期 descriptor 和六个静态 marker target，不增加运行时 VisualTree 搜索、反射或 selector 字符串组装。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 Result 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- `root`、`icon`、`title`、`subTitle`、`extra`、`body` 的名称、selector、ContractType、cardinality 和 marker 身份。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 `PART_StatusIconPresenter` 的 PropertyChanged 订阅释放路径与新 presenter 的重新订阅顺序。
- Light/Dark、Browser/Desktop 下的主题一致性，以及无 `SizeType` 的 Token 尺寸基线。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

Result 专项测试必须覆盖 descriptor、六个静态 marker target、五个生成 Style 的实际命中、七种 `Status`、自定义 Icon、可选内容显隐、
`extra` 区域拉伸与内容居中、普通图标 presenter re-template 订阅和默认主题不消费 `.semantic-*`。Gallery 测试必须覆盖六项
Part 描述、与 Ant Design 对齐的 owner-scoped 样式示例、AtomUI `v6.1.3` Tag 和 Semantic Preview 的延迟实例化。
