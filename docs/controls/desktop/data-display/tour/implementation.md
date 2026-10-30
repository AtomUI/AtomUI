# Tour 桌面版实现原理

本文档描述 Tour 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Tour 桌面版架构设计](overview.md)，Semantic Part 契约见 [Tour Semantic Part 契约](semantic-part.md)，变化记录见 [Tour Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Tour Token 设计](token.md)。

Popup 接入边界：`Tour` 负责业务状态和内容准备，`PART_Popup` 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

本文档覆盖 Tour 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Tour/DefaultTourIndicator.cs`
- `src/AtomUI.Desktop.Controls/Tour/ITourAction.cs`
- `src/AtomUI.Desktop.Controls/Tour/Localization/TourLangResourceKind.cs`
- `src/AtomUI.Desktop.Controls/Tour/Localization/en-US.xlf`
- `src/AtomUI.Desktop.Controls/Tour/Localization/zh-CN.xlf`
- `src/AtomUI.Desktop.Controls/Tour/Localization/zh-TW.xlf`
- `src/AtomUI.Desktop.Controls/Tour/TextTourIndicator.cs`
- `src/AtomUI.Desktop.Controls/Tour/Themes/DefaultTourIndicatorTheme.axaml`
- `src/AtomUI.Desktop.Controls/Tour/Themes/TextTourIndicatorTheme.axaml`
- `src/AtomUI.Desktop.Controls/Tour/Themes/TourStepTheme.axaml`
- `src/AtomUI.Desktop.Controls/Tour/Themes/TourStepsViewTheme.axaml`
- `src/AtomUI.Desktop.Controls/Tour/Themes/TourTheme.axaml`
- `src/AtomUI.Desktop.Controls/Tour/Tour.cs`
- `src/AtomUI.Desktop.Controls/Tour/Tour.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Tour/TourIndicator.cs`
- `src/AtomUI.Desktop.Controls/Tour/TourLayer.cs`
- `src/AtomUI.Desktop.Controls/Tour/TourPlacementMode.cs`
- `src/AtomUI.Desktop.Controls/Tour/TourStep.cs`
- `src/AtomUI.Desktop.Controls/Tour/TourStepNavRequestEventArgs.cs`
- `src/AtomUI.Desktop.Controls/Tour/TourStepOption.cs`
- `src/AtomUI.Desktop.Controls/Tour/TourStepsView.cs`
- `src/AtomUI.Desktop.Controls/Tour/TourToken.cs`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `DefaultTourIndicator`：默认分步指示器，模板托管代码物化的圆点（见 5.1），维护 public surface 与主题可观察行为。
- `TextTourIndicator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `Tour`：public API 与运行状态 owner，并实现 `ISemanticPartCrossRootProvider` 把打开态的共享遮罩层上报为跨根宿主。
- `TourIndicator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TourLayer`：共享遮罩层，自绘镂空几何（目标区域外全铺 mask）；经 `VisualLayerManager` 挂载，逻辑父归属当前打开的 Tour。
- `TourStep`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TourStepOption`：集合项、节点或容器类型，承载单项状态和模板协作。
- `TourStepsView`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TourToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `TourLangResourceKind`：稳定的本地化 Catalog enum；三个 XLIFF 文件提供随模块发布的内置翻译，生成器负责编译资源表和 XAML 扩展。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。
- 共享 `TourLayer` 遮罩遵循“谁打开谁拥有”：打开路径挂逻辑父并上报跨根，关闭路径解除逻辑父；跨 Tour 复用必须先 `SetParent(null)` 再挂新 owner（Avalonia `SetParent` 非 null 到非 null 会抛异常）。

## 4. 状态与数据流

Tour 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`CloseIcon`、`CoverTemplate`、`Description`、`DescriptionTemplate`、`ItemSpacing`、`ItemTemplate`、`Title`、`TitleTemplate`。
- 选择与集合：`ActiveIndex`、`CurrentIndex`、`IndicatorActiveColor`、`StepCount`。`CurrentIndex` 默认双向绑定。
- 交互与状态：`IsArrowVisible`、`IsDisabledInteraction`、`IsMotionEnabled`、`IsOpen`、`IsPointAtCenter`、`IsPopupPinnedOpen`、`IsScrollIntoView`、`IsShowMask`。`IsOpen` 默认双向绑定；`IsPopupPinnedOpen` 为 public，供语义预览与设计期检查钉住弹层。
- 视觉与布局：`Background`、`GapOffsetX`、`GapOffsetY`、`GapRadius`、`IndicatorColor`、`IndicatorSize`、`MaskColor`、`Placement`、`StyleType`、`TargetRegionCornerRadius`。
- 其他稳定入口：`Cover`、`Indicator`、`Target`、`TargetRegion`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- `IsOpen` 与 `CurrentIndex` 必须保持默认 `TwoWay`；开始、关闭、下一步和上一步应通过 current value 语义更新受控状态，确保 ViewModel 与 popup/indicator 同步。
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

- `PART_ArrowDecorator`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_Popup`：承载弹层宿主、打开关闭或候选内容。

### 5.1 语义部件与共享遮罩归属

Semantic Part 的公共契约（13 个部件的字段、存在条件、数量语义与 Selector 用法）见 [Tour Semantic Part 契约](semantic-part.md)。实现侧需要维护的不变量：

- marker 分布：`TourTheme` 的 `PART_ArrowDecorator`（ArrowDecoratedBox）挂 `popup.root`；`TourStepTheme` 挂 `popup.section` / `popup.header` / `popup.cover` / `popup.title` / `popup.close` / `popup.description`；`TourStepsViewTheme` 挂 `popup.footer` / `popup.indicators` / `popup.actions`；`DefaultTourIndicator` 的物化圆点由控件代码用生成常量挂 `popup.indicator`。内置主题默认视觉不使用 `.semantic-*` selector。
- 共享遮罩跨根命中：`popup.mask` 的载体 `TourLayer` 由 `VisualLayerManager` 呈现，与 Tour 不在同一视觉子树；`AttachMaskLayer` 在打开路径把它挂为当前 Tour 的逻辑子节点并抛 `CrossRootsChanged`，owner 作用域 Semantic Style（`Tour.TourPopupMaskStyle`，即 `>> .semantic-popup-mask` descendant 路由）沿逻辑父链跨视觉根命中；关闭路径 `DetachMaskLayer` 解除逻辑父并再次通知。
- `DefaultTourIndicator` 物化圆点没有 `TemplatedParent`，任何以 `/template/` 为锚的 selector 链都无法命中它们；圆点的尺寸/颜色/间距样式声明在模板内 `DotsLayout` 的 `Styles` 集合中，沿逻辑树流到物化圆点，绑定经 `RelativeSource AncestorType` 回到控件属性。`MeasureOverride` 布局契约保持 `StepCount*size + (StepCount+1)*spacing`。

## 6. 交互与事件处理

Tour 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 弹层、窗口或 overlay 类路径必须稳定处理打开、关闭、取消、重复打开和宿主失活。
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
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 Tour 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
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
- Semantic Part 声明或 marker 变更时同步 `semantic-part.md`，并保持 `TourSemanticPartTests` 与 Gallery Tour 语义预览/高亮测试通过。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
