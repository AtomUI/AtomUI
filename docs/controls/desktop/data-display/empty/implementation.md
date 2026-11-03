# Empty 桌面版实现原理

本文档描述 Empty 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见
[Empty 桌面版架构设计](overview.md)，公开主题区域见 [Empty Semantic Part 契约](semantic-part.md)，变化记录见
[Empty Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Empty Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Empty 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Controls/Empty/AbstractEmpty.cs`
- `src/AtomUI.Controls/Empty/BuiltInImageBuilder.cs`
- `src/AtomUI.Controls/Empty/PresetEmptyImage.cs`
- `src/AtomUI.Desktop.Controls/Empty/Empty.cs`
- `src/AtomUI.Desktop.Controls/Empty/Empty.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Empty/EmptyToken.cs`
- `src/AtomUI.Desktop.Controls/Empty/Themes/EmptyTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `AbstractEmpty`：共享图片状态 owner，维护来源互斥、颜色计算、图片结果提交与模板生命周期同步。
- `BuiltInImageBuilder`：按传入颜色生成 Default/Simple SVG 字符串，不持有控件、主题资源或 renderer。
- `Empty`：桌面主题与 Semantic Part 的公开 owner，图片同步复用共享基类。
- `EmptyToken`：从全局 Token 派生图片高度及描述、Footer 间距；图形颜色由 ControlTheme 直接绑定 SharedToken。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- `PART_SvgImage` 只持有当前绘制结果，不能成为图片来源或主题状态的另一份权威数据。
- 模板重套用时替换 part 引用；图片内容更新和同一模板实例的摘挂不得替换 image Semantic target。
- 图片同步职责收敛在共享基类，不由列表、弹层宿主或 Gallery 分别维护恢复逻辑。

## 4. 状态与数据流

图片状态流：

```text
PresetImage / ImageSource / ImagePath
  + 五个内部颜色属性（由 ControlTheme 绑定 SharedToken）
  -> AbstractEmpty 校验来源、计算完整图片结果
  -> 同一个 PART_SvgImage 的 Source / Path
  -> 图形绘制与 Measure/Arrange
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Description`、`Footer`、`FooterTemplate`、`ImagePath`、`ImageSource`、`IsDescriptionVisible`、`PresetImage`。
- 视觉与布局：`SizeType`、`StrokeDashArray` 以及继承的 root 表面属性。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- `Footer` 与 `FooterTemplate` 替换由 ContentPresenter 原生内容生命周期管理，不保留旧 Control 或 DataTemplate 产物。
- 图片来源互斥检查和 `_svg` 更新仍由 `AbstractEmpty` 单一 owner 管理。
- 模板应用、重新挂树和挂树期间的相关属性变化必须使用同一套图片校验与更新规则。
- 离树期间允许修改输入属性，重新挂树时读取最新输入，不缓存旧来源或用额外刷新标记保存待更新状态。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 组合结构模型

`EmptyTheme.axaml` 直接表达以下模板结构，没有额外图片 presenter 或恢复宿主：

```text
Empty
  -> DashedBorder
     -> StackPanel
        -> Svg#PART_SvgImage
        -> TextBlock#Description
        -> ContentPresenter（Footer）
```

| 节点 | 生命周期 owner | 稳定性与维护边界 |
| --- | --- | --- |
| `Empty` | 使用方或组合控件 | public；持有来源、描述、Footer 与布局属性。 |
| `DashedBorder`、`StackPanel` | ControlTemplate | private；负责表面投影和纵向布局，不增加图片状态。 |
| `Svg#PART_SvgImage` | ControlTemplate；由 `AbstractEmpty` 接入 | template-stable；接收计算后的图片，公开定制使用 `image` Semantic Part。 |
| `TextBlock#Description`、Footer `ContentPresenter` | ControlTemplate | 分别承载 `description`、`footer` Semantic Part，内容与可见性由模板绑定维护。 |

## 6. 生命周期与模板接入

`AbstractEmpty` 在重新挂载后检查当前互斥输入，并回放到原 renderer；离树期间切换 Preset/Source 不再留下旧插画。互斥异常、Source/Path 优先清理和内置 SVG 生成公式保持。

原生 Footer 在换模板时由新 ContentPresenter 重新挂接。模板变更前仅保存原后代焦点的弱引用；Loaded 回调同时检查 owner generation、同一 TopLevel、当前仍为后代且没有后来用户焦点，才恢复焦点。卸载递增 generation 并清 pending，回调不强持有 owner 或 Footer。此处是运行时内容与焦点生命周期，无法通过 ControlTheme Setter 表达；样式、图片尺寸、间距和字体绑定仍由 AXAML 管理。该修复不改 Footer 的对象身份、文本、caret 或内容模板合同。

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取当前 part，把有效来源与颜色投影到新的 renderer，不继续更新旧 part。
- 重新挂树时执行基类生命周期逻辑，再按当前输入同步图片；模板尚未应用时不访问 renderer，由模板接入完成同步。
- 保留图片属性变更的挂树判断，离树期间不因属性赋值主动更新 renderer；重新挂树必须补齐这段期间的输入变化。
- 逻辑树、样式与可视树的变更有各自时序，不能把 `IsAttachedToVisualTree` 单独视为颜色资源已经齐备的证明。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

图片同步必须覆盖以下入口：

| 入口 | 输入与同步职责 |
| --- | --- |
| 模板应用或重套用 | 获取新的 `PART_SvgImage`，校验当前来源并提交可用图片结果。 |
| 重新挂回可视树 | 使用当前来源与主题资源重新同步，不依赖属性再次变化或模板重新创建。 |
| 挂树期间来源变化 | 执行互斥校验，更新同一 renderer，或在三种来源均为空时清空图片。 |
| 挂树期间颜色变化 | 对预设图形重新计算；资源暂缺时保留已有结果，资源齐备后的变化继续使用相同更新规则。 |

稳定 template part 接入点：

- `PART_SvgImage`：稳定模板协作入口，重命名前必须同步主题和实现。

Semantic marker 与真实节点映射：

| Part | Marker 节点 | 创建方式 | 可见性/内容状态 |
| --- | --- | --- | --- |
| `root` | `Empty` owner | 控件实例 | 由 owner 属性控制，不使用 `.semantic-root`。 |
| `image` | `PART_SvgImage` | ControlTemplate 静态创建 | 始终存在；图片来源变化只更新 renderer。 |
| `description` | 描述 `TextBlock` | ControlTemplate 静态创建 | `IsDescriptionVisible` 通过 TemplateBinding 控制可见性。 |
| `footer` | Footer `ContentPresenter` | ControlTemplate 静态创建 | `Footer=null` 时隐藏，非空时展示内容。 |

三个非 root marker 均使用静态 `Classes.semantic-*="True"`。默认主题不能用这些 class 驱动自身样式；生成的
`EmptyImageStyle`、`EmptyDescriptionStyle` 和 `EmptyFooterStyle` 只供应用侧定制。

ControlTemplate 使用一个不带 Semantic marker 的表面 `DashedBorder` 投影 owner 的 `Background`、`BorderBrush`、
`BorderThickness`、`CornerRadius`、`Padding` 和 `StrokeDashArray`，内部 `StackPanel` 只负责纵向排列三个非 root
区域。两者都属于 Composition Model，不是额外 Semantic Part。

## 7. 交互与事件处理

Empty 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 值提交或命令触发必须保持继承控件的事件顺序。

当前没有抽取到控件专属 public 事件；交互语义主要通过继承事件、命令、属性变化和 Gallery 可观察行为体现。

## 8. 内部算法与关键流程

### 8.1 图片计算与结果提交

来源和内部颜色属性是输入，renderer 的 `Source` / `Path` 是输出。计算和提交必须分开，遵守下列顺序：

1. 执行图片来源互斥校验；模板 part 尚不存在时不写入 renderer。
2. 对预设图形，先检查 `BorderColor`、`BorderColorSecondary`、`ShadowColor`、`ContentColor`、`BgColor` 是否齐备。
   任一资源暂缺时保留已有 `Source` / `Path`，首次没有结果时保持无图，不把暂缺转换成取消图片。
3. 资源齐备后完成背景叠色计算，由 `BuiltInImageBuilder` 生成完整 SVG 字符串，再清理旧 `Path` 并提交到 `Source`。
   禁止先把 `Source` 设为 `null` 再检查画刷或生成结果。
4. 对 `ImageSource` 或 `ImagePath`，使用调用方指定的内容或路径，清理另一个来源字段，不要求预设颜色资源齐备。
5. 三种来源均为 `null` 时清空 `Source` 与 `Path`，保证主动取消图片后不残留旧图。

提交必须幂等：目标 `Source` / `Path` 与当前结果相同时不写入，也不经过 `Source=null` 的中间状态。同步不替换模板节点，
不额外保存图片来源副本，不引入计时器、Dispatcher 延时、重试或强制刷新入口。

### 8.2 其他模板状态

- `IsDescriptionVisible` 由 AXAML `TemplateBinding` 直接投影到 description 节点，避免 C# 可见性同步状态。
- `Footer` 与 `FooterTemplate` 由 AXAML ContentPresenter 直接消费，Footer 的显示状态由 `Footer != null` 决定。
- root 的背景、边框、圆角、Padding 和虚线节奏由 `DashedBorder` 直接投影。
- SizeType selector 只更新 image 高度和 description margin；Footer 使用独立稳定间距 Token。

## 9. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。
- 五个颜色属性的 SharedToken 绑定保留在 ControlTheme，图片同步不能用本地颜色值覆盖资源绑定。
- SVG 字符串依赖运行时颜色叠加计算，由 C# 生成；这项计算不属于可等价迁移为 TemplateBinding 的固定模板关系。
- 重新挂树的同步使用控件生命周期入口和显式调用，不增加资源订阅、全局图片缓存或运行时反射。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- Footer 内容替换依赖 Avalonia ContentPresenter 的标准 logical/visual ownership，不额外缓存内容 Control。

## 10. 维护不变量

维护 Empty 时不得破坏：

- Public API、默认值、事件顺序，以及新增 `Footer`/`FooterTemplate` 的内容语义。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 同一实例摘挂后，图片必须与最新来源及主题一致，描述、Footer、尺寸与 Semantic target 身份符合各自契约。
- 颜色资源暂缺时保留已有图片，来源明确全部为空时清空；重复同步相同结果不先清空再重建。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 11. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

Empty 专项测试必须覆盖 descriptor 字段、静态 marker、三种图片来源、描述可见性、Footer 内容/模板替换、三档 SizeType、
re-template 和应用级生成 Style 命中。Empty 不涉及 Popup、跨 VisualRoot、运行时 marker 或反射发现，单控件阶段不要求额外
NativeAOT publish；批次收尾仍执行 Gallery NativeAOT 验证。

图片生命周期验证须保留同一实例、宿主与原有布局约束，使用 `Content=null` 后设回原控件的原始摘挂操作：

| 场景 | 必须验证的结果 |
| --- | --- |
| 直接使用 Default/Simple；ListBox、ListView、TreeView、CascaderView 与空 `DataGridLocalSource` 的 DataGrid | 首次图片有效；同一实例摘挂后图案、描述与尺寸保持，未重套用模板时 image target 不变。 |
| 预设颜色资源暂缺再恢复 | 暂缺时不清空已有图片；齐备后使用完整颜色输入生成结果，不依赖调用方切换预设。 |
| 自定义 `ImageSource` / `ImagePath` | 摘挂后仍使用指定来源，不依赖预设画刷，不残留另一来源字段。 |
| 离树期间修改图片来源或切换 Light/Dark | 挂回后显示最新有效输入的结果；在同步入口继续执行来源互斥校验。 |
| 挂树期间或离树期间明确清空三种来源 | 挂树同步后不残留旧图，描述与 Footer 不因图片清空而被重建。 |
| 对相同结果重复同步；模板重套用 | 相同结果不反复清空 renderer；重套用后新 target 接收当前图片，旧 target 不再更新。 |

现有 `EmptySemanticPartTests` 覆盖来源切换、模板重套用和 Semantic Part；`EmptyIndicatorContractTests` 覆盖默认空态组合与
自定义空态入口。上述生命周期场景的验证与测试去留遵循
[测试价值与生命周期规范](../../../../engineering/development/test-value-and-lifecycle.md)，验证结果不得仅以 `Source` 非空替代图案、
文字和布局检查。ComboBox 等弹层消费者还须按实际弹层的实例复用路径验证，不能仅从是否使用 Empty 推定运行结果。
