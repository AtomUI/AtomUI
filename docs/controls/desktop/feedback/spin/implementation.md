# Spin 桌面版实现原理

本文档描述 Spin 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Spin 桌面版架构设计](overview.md)，变化记录见 [Spin Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Spin Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Spin 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Controls/Spin/AbstractSpin.cs`
- `src/AtomUI.Controls/Spin/AbstractSpinIndicator.cs`
- `src/AtomUI.Controls/Spin/SpinIndicatorDotPanel.cs`
- `src/AtomUI.Desktop.Controls/Spin/Spin.cs`
- `src/AtomUI.Desktop.Controls/Spin/SpinIndicator.cs`
- `src/AtomUI.Desktop.Controls/Spin/SpinToken.cs`
- `src/AtomUI.Desktop.Controls/Spin/Spin.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Spin/SpinIndicator.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Spin/Themes/SpinIndicatorTheme.axaml`
- `src/AtomUI.Desktop.Controls/Spin/Themes/SpinTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。
- Semantic descriptor 由各 public owner 的 `[SemanticPart]` 声明生成；模板只使用静态 `Classes.semantic-*="True"` marker。

## 3. 核心类职责

- `AbstractSpin`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractSpinIndicator`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `Spin`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SpinIndicator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `SpinToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Spin 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`CustomIndicatorTemplate`。
- 交互与状态：`IsMaskBackgroundEnabled`、`IsMaskBlurEnabled`、`IsMotionEnabled`、`IsSpinning`、`IsTipVisible`。
- 视觉与布局：`DotBgBrush`、`DotSize`、`IndicatorSize`、`SizeType`。
- 动效与异步：`MotionDuration`、`MotionEasingCurve`。
- 其他稳定入口：`CustomIndicator`、`Tip`。

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

- `PART_CustomIndicatorPresenter`：展示用户内容、文本、图标或模板化数据。

`SpinTheme` 的模板使用两层叠加结构：`ContentPresenter.semantic-container` 承载用户内容，`MaskLayout` 面板在
`IsSpinning=True` 时可见，其内部 `Border.semantic-mask` 提供遮罩背景，`StackPanel.semantic-section` 承载
`SpinIndicator.semantic-indicator` 与 `TextBlock.semantic-description` 并居中。遮罩层组与内容容器是同一根面板下的
sibling，确保 Semantic Preview 高亮 `container` 时只覆盖用户内容区域。

`SpinIndicatorTheme` 的模板由两个静态替代实现构成：`SpinIndicatorDotPanel.semantic-content` 承载四个内置圆点，
`PART_CustomIndicatorPresenter.semantic-content` 承载自定义指示器；`IsCustomIndicator` 决定当前可见 target，两个
target 都保持 marker。四个圆点（`Ellipse.SpinIndicatorDot`）是内部 Composition 节点，不属于任何 Semantic Part。

## 6. 交互与事件处理

Spin 的交互事件应从输入源收敛到控件级语义事件：

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

根控件的 MaskOpacity 自动过渡使用 `TransitionInitializationSession`：构造时抑制、Loaded 完成基线、detach 复位，
恢复最新 Transitions 来源。指示器的持续反馈独立于该初始化作用域，不通过私有开关或 Dispatcher 猜测就绪。
- Semantic Part marker 不参与状态切换；`IsSpinning`、`IsTipVisible` 和 `IsCustomIndicator` 只改变目标节点的
  `IsVisible`，marker 身份和数量保持不变。
- 指示器旋转与圆点透明度动画由 `AbstractSpinIndicator` 通过 Composition 动画直接拥有；动画目标从
  `IsCustomIndicator` 推导，状态切换只重启动画，不增删 marker 节点。

`AbstractSpinIndicator` 的旋转和内置 dot opacity 由 Compositor 无限动画驱动。Compositor 不提供控件级有效可见性暂停语义，因此 indicator 在 attach 后跟踪自身及 Visual 祖先链；有效不可见时停止所有目标动画并复位视觉状态，恢复可见时重新解析当前 built-in/custom target 并启动动画。detach 时必须释放可见性订阅和所有 Compositor target。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不通过 VisualTree 扫描维护 Semantic Part；Gallery Preview 使用生成 descriptor 和 owner-scoped marker 解析。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 隐藏祖先下的 Spin 不得保留旋转或 dot opacity 的 Compositor 无限动画。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 Spin 时不得破坏：

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
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
