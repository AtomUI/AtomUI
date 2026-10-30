# Statistic 桌面版实现原理

本文档描述 Statistic 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Statistic 桌面版架构设计](overview.md)，公开主题区域见 [Statistic Semantic Part 契约](semantic-part.md)，变化记录见 [Statistic Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Statistic Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Statistic 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Statistic/AbstractStatistic.cs`
- `src/AtomUI.Desktop.Controls/Statistic/Statistic.cs`
- `src/AtomUI.Desktop.Controls/Statistic/Statistic.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Statistic/StatisticCountUp.cs`
- `src/AtomUI.Desktop.Controls/Statistic/StatisticToken.cs`
- `src/AtomUI.Desktop.Controls/Statistic/StatisticUtils.cs`
- `src/AtomUI.Desktop.Controls/Statistic/Themes/AbstractStatisticTheme.axaml`
- `src/AtomUI.Desktop.Controls/Statistic/Themes/AbstractStatisticTheme.cs`
- `src/AtomUI.Desktop.Controls/Statistic/Themes/StatisticCountUpTheme.axaml`
- `src/AtomUI.Desktop.Controls/Statistic/Themes/StatisticTheme.axaml`
- `src/AtomUI.Desktop.Controls/Statistic/Themes/TimerStatisticTheme.axaml`
- `src/AtomUI.Desktop.Controls/Statistic/TimerStatistic.cs`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- `StatisticTheme.axaml` 负责 `Statistic` 的叶子模板、静态 Semantic marker、root 表面投影和间距；`AbstractStatisticTheme.axaml` 只提供家族共享 selector。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `AbstractStatistic`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractStatisticTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `Statistic`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `StatisticCountUp`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `StatisticToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `TimerStatistic`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Statistic 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`AnimatingValue`、`ContentFontSize`、`ContentForeground`、`EndValue`、`Value`、`ValuePrefixAddOn`、`ValuePrefixAddOnTemplate`、`ValueSuffixAddOn`、`ValueSuffixAddOnTemplate`。
- 选择与集合：`GroupSeparator`。
- 交互与状态：`IsLoading`。
- 动效与异步：`RefreshDuration`。
- 视觉与格式：`DecimalSeparator`、`Format`、`GroupSeparator`、`Precision`、`StrokeDashArray`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- 当前没有显式 `[TemplatePart]` 字段；`Statistic` 的公开主题边界由静态 Semantic marker 表达。

Semantic marker 与真实节点映射：

| Part | Marker 节点 | 创建方式 | 状态 |
| --- | --- | --- | --- |
| `root` | `Statistic` owner | 控件实例 | 始终存在，不使用 `.semantic-root`。 |
| `header` | `Border#HeaderLayout` | ControlTemplate 静态创建 | Header 为 null 时隐藏，节点身份不变。 |
| `title` | `ContentPresenter#HeaderPresenter` | ControlTemplate 静态创建 | 跟随 header 可见性。 |
| `content` | `StackPanel#ContentLayout` | ControlTemplate 静态创建 | 始终存在；由 Skeleton 包装。 |
| `value` | `ContentPresenter#ContentPresenter` | ControlTemplate 静态创建 | 始终存在，内容可替换。 |
| `prefix` | `ContentPresenter#ValuePrefixAddOn` | ControlTemplate 静态创建 | AddOn 为 null 时隐藏，节点身份不变。 |
| `suffix` | `ContentPresenter#ValueSuffixAddOn` | ControlTemplate 静态创建 | AddOn 为 null 时隐藏，节点身份不变。 |

六个 selector Part 使用静态 `Classes.semantic-*="True"`。默认主题只按类型和 Name selector 提供基线，不能消费 `.semantic-*` 驱动自身视觉。

## 6. 交互与事件处理

Statistic 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 输入类路径必须保持 Form、validation、clear、placeholder 和键盘行为一致。

当前没有抽取到控件专属 public 事件；交互语义主要通过继承事件、命令、属性变化和 Gallery 可观察行为体现。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

1. `Statistic` 把 `Value`、分隔符和 `Precision` 归一为 `EffectiveValue`，在未提供自定义 `Content` 时同步生成内容。
2. `StatisticTheme.axaml` 的叶子模板把 Header、Content、前后缀和 root 表面属性投影到固定节点；`TimerStatistic` 保持自己的独立模板和契约。
3. `content` 节点通过 `TextElement.Foreground` 与 `TextElement.FontSize` 向 value、prefix、suffix 继承默认或 Semantic Style 值。
4. prefix 内的 AtomUI Icon 从 `content` 的继承文本属性同步 FillBrush、StrokeBrush 和尺寸，使 content 级颜色、字号定制保持一致。
5. root 使用 `DashedBorder` 投影 Background、BorderBrush、BorderThickness、CornerRadius、Padding 与 `StrokeDashArray`；内部 frame 不是额外 Semantic Part。
6. `IsLoading` 只切换 Skeleton 状态和 root spacing，不创建、删除或重新标记 Semantic target。

`TimerStatistic` 以绝对 `DateTime` 作为值 owner，`DispatcherTimer` 只负责刷新显示，不累计相对 tick。控件 attach 后跟踪自身及 Visual 祖先链；有效不可见时停止 timer，重新可见时先使用 `DateTime.Now` 重算 `RemainingTime`，再恢复周期刷新。该规则保证隐藏期间不产生 UI 线程 tick，同时倒计时和正计时不会因暂停刷新而产生时间漂移。

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
- 隐藏祖先下的 TimerStatistic 不运行刷新 timer；恢复时必须从绝对时间重算，不补发隐藏期间的 tick。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。
- Semantic Part 使用生成期 descriptor 和静态 marker，不增加运行时 VisualTree 搜索、反射或 selector 字符串组装。

## 9. 维护不变量

维护 Statistic 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- `root`、`header`、`title`、`content`、`value`、`prefix`、`suffix` 的名称、selector、ContractType、cardinality 和静态 marker 身份。
- `Statistic` 叶子模板与 `TimerStatistic` 独立模板的边界；不得把 Statistic 契约无意发布到 TimerStatistic 或 StatisticCountUp。
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

Statistic 专项测试必须覆盖 descriptor、静态 marker、六个生成 Style 的真实投影、root 虚线表面、Header/Prefix/Suffix 可见性、格式化 Content、loading 和 re-template。Gallery 测试必须覆盖 Semantic Preview 延迟实例化、七项描述和对应公开上游 6.6.0 双 Statistic 样式示例。
