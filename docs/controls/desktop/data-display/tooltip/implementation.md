# Tooltip 桌面版实现原理

本文档描述 Tooltip 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Tooltip 桌面版架构设计](overview.md)，变化记录见 [Tooltip Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Tooltip Token 设计](token.md)。

Popup 接入边界：`ToolTip` 负责业务状态和内容准备，manually created Popup 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

本文档覆盖 Tooltip 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Tooltip/Themes/ToolTipTheme.axaml`
- `src/AtomUI.Desktop.Controls/Tooltip/ToolTip.cs`
- `src/AtomUI.Desktop.Controls/Tooltip/ToolTip.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Tooltip/ToolTipPseudoClass.cs`
- `src/AtomUI.Desktop.Controls/Tooltip/ToolTipToken.cs`
- `src/AtomUI.Desktop.Controls/Tooltip/OverflowTip.cs`
- `src/AtomUI.Desktop.Controls/Tooltip/ToolTipService.cs`
- `src/AtomUI.Desktop.Controls/Primitives/ArrowDecoratedBox/Themes/ArrowDecoratedBoxTheme.axaml`（共享 `container`/`arrow` marker 宿主）

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `ToolTip`：tooltip 内容与视觉宿主，持有游离 `Popup` 作为物理弹层，是打开状态调和流程的唯一执行者。
- `ToolTipPseudoClass`：伪类常量定义，维护 `:open` 主题可观察契约。
- `ToolTipToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `ToolTipService`：全局悬停服务，订阅 `InputManager` 原始指针事件识别 tooltip 宿主与出现时机，只写 `ToolTip.IsOpen` 附加属性，不直接持有或开关 popup。服务属于包入口的明确初始化逻辑，其存在由实际静态调用决定，不依赖目录裁剪归属；注册遵循 [TypeMap 架构](../../../../architecture/foundations/aot-and-trimming.md)。
- `OverflowTip`：附加到文本展示节点的共享溢出提示 behavior，只管理自己写入的 tooltip，并在文本实际超出可见宽度时启用提示。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

悬停服务按输入根节点身份识别宿主窗口和原生 Tooltip 窗口。原始输入进入服务前，控件事件可能已同步关闭窗口，
因此事件根节点仍存在不代表其 `RootVisual` 仍存在。Overlay Tooltip 与宿主共享输入根，服务通过命中元素的祖先链
识别 Tooltip 内部移动，不能把同窗全部输入都当作 Tooltip 窗口输入。

## 4. 状态与数据流

Tooltip 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：继承 ToolTip 的 `Content` 与目标控件 tooltip 绑定。
- 交互与状态：`IsMotionEnabled`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

打开状态按声明式调和模型流动：

```text
IsOpen / Tip / 宿主 attach-detach 变化
  -> ReconcileOpenState（唯一物理开关入口，幂等）
  -> ToolTipOpening（可取消，否决时回写 IsOpen=false）
  -> freestanding Popup.IsOpen
  -> :open 伪类 / Opened / Closed
  -> 弹层外部关闭时回写 IsOpen=false
```

调和维护要求：

- popup 的物理开关只能由调和入口改变；`ToolTipService`、`OverflowTip` 和其他调用方只写 `IsOpen`，由调和兑现物理状态。
- 调和是幂等的：同一输入重复触发不得产生重复打开、重复事件或残留订阅。
- `Tip` 未就绪时不重置 `IsOpen`；内容就绪后由 `TipProperty` 变化再次触发调和。

`OverflowTip` 的状态流：

```text
attached properties + target text/font
  -> owner bounds or published text viewport metric
  -> text width comparison
  -> owned ToolTip.Tip / placement / delay
```

用户显式设置的 `ToolTip.Tip` 始终优先。`OverflowTip` 只清理由自己写入的 tip，不参与目标控件的选择、输入或模板状态。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。
- `ToolTipService` 跟踪宿主时订阅其 visual-tree detach，宿主切换、移出窗口、detach 或服务 dispose 时解除该跟踪。
  detach 同步停止待显示定时器、清空定时器宿主引用并释放当前宿主；陈旧命中结果不能重新跟踪已卸载或属于其他输入根的宿主。
  detach 先解除 Tooltip 的指针退出处理器，避免原生弹层销毁时的退出事件被解释为用户关闭；关闭事件继续记录关闭时间并退订。
  服务结束跟踪不重置声明式 `IsOpen`，物理弹层关闭与重新挂载后的重开继续由 `ToolTip` 调和流程负责。
- `OverflowTipState` 对 owner bounds、文本、字体、tooltip 和内部 text viewport metric 的订阅必须由同一个 disposable owner 管理；禁用 behavior 时统一释放。
- TextBox/TextArea 的 template part 由输入控件自己获取和管理。`OverflowTip` 不得调用 `GetVisualDescendants()`、查找 `PART_TextPresenter` 或持有输入控件的 presenter/scroller。
- `IsOpen` 为 `true` 且宿主尚未挂入 visual tree 时，`ToolTip` 对宿主的 `AttachedToVisualTree` 建立一次性订阅；宿主挂入或 `IsOpen` 转为 `false` 时退订。该订阅生命周期自限，不随控件树重建累积。
- 宿主侧附加属性（定位、偏移、箭头、`TextWrapping`/`TextTrimming` 文本布局）在弹层打开时通过 `GetBindingObservable` 活绑定到 popup 与 `PART_ContentPresenter`，统一登记进弹层订阅集并在弹层关闭时释放；不使用打开时的一次性快照赋值，保证打开期间修改附加属性即时生效。
- Tip 为 `ToolTip` 实例时，呈现类附加属性的取值来源由 `ResolveValueSource`/`GetToolTipValue` 统一解析：实例显式设置（`IsSet`）优先，否则回落宿主；参与覆盖的属性集合见 overview 的 Tip 实例定制模型。新增呈现类附加属性时必须走同一解析入口，不允许出现只读宿主的取值路径。
- 宿主从 visual tree 卸载时物理关闭弹层，但不清除 `IsOpen`；重新挂入后由调和流程重开。

稳定 template part 接入点：

- `PART_ArrowDecorator`：稳定模板协作入口，重命名前必须同步主题和实现。主题未提供该部件时控件按无箭头降级：
  弹层阴影容器关闭箭头预留，custom placement 复用箭头隐藏补偿；部件在后续模板应用中出现时，箭头设置与弹层绑定
  自动补齐。
- `PART_ContentPresenter`：展示用户内容、文本、图标或模板化数据。

### 5.1 Semantic Part marker 映射

Tooltip 公开 `root`/`container`/`arrow` 三个 Semantic Part，descriptor 位于 `ToolTip.SemanticParts.cs`。
marker 与真实模板节点的对应关系：

- `root`：owner 本身，无 marker。
- `container`：marker `.semantic-container` 位于共享 `ArrowDecoratedBoxTheme.axaml` 的
  `Border#PART_ContentDecorator`。
- `arrow`：marker `.semantic-arrow` 位于共享 `ArrowDecoratedBoxTheme.axaml` 的
  `ArrowIndicator#PART_ArrowIndicator`。

`container` 与 `arrow` 声明 `CrossNestedOwners=true`。`ToolTip` 的模板只把自己的内容交给
`ArrowDecoratedBox#PART_ArrowDecorator`（带 `.semantic-scope-arrow-decorated-box` scope 锚点），marker 节点位于
`ArrowDecoratedBox` 自身模板中，`/template/` 链不能直接跨越嵌套 owner 进入该模板；生成 Style 通过 scope 锚点
`/template/ .semantic-scope-arrow-decorated-box /template/ .semantic-*` 路由命中，解析器被允许跨越嵌套 owner 边界。

descriptor 与 marker 均属于编译期生成与静态主题输入，不引入运行时反射或 VisualTree 扫描（系统规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)）。

## 6. 交互与事件处理

Tooltip 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 值提交或命令触发必须保持继承控件的事件顺序。

当前没有抽取到控件专属 public 事件；交互语义主要通过继承事件、命令、属性变化和 Gallery 可观察行为体现。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- 内容、命令和视觉状态在模板节点之间的同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

### 7.1 OverflowTip 宽度解析

宽度解析按稳定能力而不是模板类型判断：

1. AtomUI TextBox/TextArea 已发布内部 text viewport metric 时，使用该有效宽度并订阅后续变化。
2. metric 为 `NaN` 时表示 viewport 已接入但布局尚未完成，此时不显示 tip，等待度量更新，不使用瞬时 owner fallback。
3. 原生或第三方 Avalonia TextBox 没有发布 metric 时，使用 `Bounds.Width - horizontal Padding` 作为兼容降级路径。
4. TextBlock 使用自身 bounds 扣除 horizontal padding；其他受支持展示节点使用 owner bounds。

内部 metric 只输出宽度，不暴露输入模板对象。输入控件模板改变时，由输入控件在自己的所有权边界内更新计算方式，`OverflowTip` 算法不随模板结构修改。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不通过 VisualTree 遍历或反射发现其他控件的 template part；输入文本可视宽度只消费输入控件发布的内部 metric。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 Tooltip 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- Semantic Part 契约：descriptor 字段、selector class、route、`ContractType` 与 cardinality（见
  [Tooltip Semantic Part 契约](semantic-part.md)）；marker 位于共享 `ArrowDecoratedBoxTheme.axaml`，结构性变更必须
  同步评估 `ArrowDecoratedBox` 的家庭消费者。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
- TextBox 内部按钮、feedback、padding 或模板重套用改变有效 viewport 时，OverflowTip 必须由度量通知重新判断，不能依赖 owner Bounds 恰好变化。
- `IsOpen`、`Tip` 与宿主 attach/detach 变化只能经调和入口影响弹层物理开关；不新增第二条直接开关 popup 的路径。
- 只有 `ToolTipOpening` 否决和弹层外部关闭可以回写 `IsOpen=false`。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 打开状态调和回归：XAML 声明式 `IsOpen=True` 打开、`Tip` 晚于 `IsOpen` 就绪、宿主 detach/reattach 重开、`ToolTipOpening` 否决、attach 订阅无残留。
- 悬停服务回归使用真实输入分发验证指针事件中同步关闭窗口后仍能处理该条原始输入，并覆盖 Overlay 同窗宿主切换、
  原生 Tooltip 同时间戳跨根移动，以及待显示宿主卸载后定时器和强引用释放。
- Semantic Part 回归：descriptor 三键、`ToolTipTheme.axaml` scope 锚点、共享 `ArrowDecoratedBoxTheme.axaml` 两个 marker、生成 Style 跨嵌套 owner 命中最低 `ContractType`（见 `tests/AtomUI.Desktop.Controls.Tests/Tooltip/ToolTipSemanticPartTests.cs`）。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
- `OverflowTip` 回归测试覆盖 TextBox 精确 viewport、内部 viewport 独立变化、模板重套用、第三方 TextBox fallback 和禁止外部 `PART_TextPresenter` 查询。
- 文本布局回归覆盖长文本在 `ToolTipMaxWidth` 内换行且不裁剪、`TextWrapping`/`TextTrimming` 附加属性默认值、打开期间修改即时生效，以及主题 `PART_ContentPresenter` 的换行契约。
- Tip 实例定制回归覆盖实例直接使用、实例级 placement/preset color/文本换行覆盖宿主、实例未设置时回落宿主，以及实例 `Background` 局部值保持。
