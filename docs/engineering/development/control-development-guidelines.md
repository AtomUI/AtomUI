# AtomUI 控件研发标准规范

本文档定义 AtomUI 控件研发时需要遵守或优先参考的工程标准，适用于 `AtomUI.Controls`、`AtomUI.Desktop.Controls`、DataGrid、ColorPicker 以及后续新增控件包。

Bug 修复必须遵守[测试价值与生命周期规范](test-value-and-lifecycle.md)，否则不算完成修复。本文要求的失败复现和
行为验证可以使用已有或临时测试；临时测试验证完成后默认删除，长期契约按准入条件保留最小充分保障，
不得将每次修复的验证要求解释为必须永久新增测试。

## API 兼容性

优化代码和修复 bug 时，默认不得修改任何既有 API、主题契约或可观察行为。即使重构后新的 API 看起来更合理，也不能擅自引入、重命名、删除或改变现有契约。

API 和主题契约包括但不限于：

- `public`、`protected` 类型、成员、构造函数、枚举值和默认值。
- Avalonia `StyledProperty`、`DirectProperty`、`RoutedEvent`、命令、事件和绑定语义。
- 控件主题资源 key、Design Token 名称、ControlTheme key、template part 名称、伪类和选择器可依赖的状态。
- Gallery、文档或示例中已经暴露给用户使用的 XAML 属性、样式入口和交互语义。

如果确认不修改 API 无法正确完成修复或优化，必须先停止实现，向用户说明：

1. 为什么当前 API 无法保持。
2. 受影响的 API 范围和兼容性风险。
3. 是否存在保持兼容的替代方案，以及代价。
4. 推荐的 API 变更方案和迁移方式。

只有获得用户明确授权后，才能进行 API 变更。没有授权时，必须选择保持兼容的实现，或把问题标记为受 API 约束阻塞。

## 控件文件拆分建议

这是一条推荐规范，不作为强制约定边界。拆分文件时应优先保证控件契约容易发现，而不是单纯追求主文件行数变少。

- 控件主文件建议保留公共契约入口，包括公共属性、事件、方法、Avalonia 属性注册、CLR wrapper、构造函数和主要 `override` 入口。
- 控件实现代码未超过约 `2000` 行时，原则上不做文件拆分，优先通过方法顺序调整、区域归类和私有方法提取来改善可读性。
- 控件实现代码超过约 `2000` 行后，才考虑按稳定职责拆分内部实现，例如视觉状态同步、布局计算、template part 生命周期、资源绑定辅助等。
- 按职责拆分，不按“看起来整齐”拆分。拆出的文件应有稳定、单一的维护主题。
- 生命周期入口建议留在主文件，复杂细节可下沉到私有 helper 或 partial 文件中。
- 不为未来可能复用提前抽象；只有出现真实重复，且抽象不会模糊生命周期、绑定优先级或主题契约时，再提取共享 helper。

## 控件级设计文档与 Changelog

控件级研发文档必须遵循 [AtomUI 控件文档规范](../contributing/control-documentation-guidelines.md)。控件设计文档只描述最新设计状态；设计、API、主题契约、Token 和实现结构的历史变化记录在对应控件目录下的 `changelog.md`。

## 非 Visual AvaloniaObject 资源宿主范式

当控件需要引入或改造 owner-managed 的非 Visual `AvaloniaObject`，并且该对象暴露 Avalonia 属性用于 XAML binding、`DynamicResource` 或 token-resource binding 时，默认必须按 [Scoped Resource Host 开发规范](scoped-resource-host.md) 处理。

要求：

- 对象主文件只保留业务 API、Avalonia 属性注册和 CLR wrapper。
- scoped `IResourceHost` / `IThemeVariantHost` 生命周期样板代码由 Source Generator 生成。
- owner 控件负责 attach/detach、属性订阅释放和 generated visual 映射清理。
- 不允许复制粘贴 `IResourceHost` / `IThemeVariantHost` 样板代码到每个描述对象中。
- 不允许为了规避泄露而删除动态资源能力、改成静态资源或清空 Gallery DataContext。

例外情况必须在方案或 PR 中说明原因、替代生命周期、测试覆盖和 AOT 影响。

## 控件 Token 设计

控件 Token 的分层、命名、计算、Theme Variables 边界、预设色和兼容性规则见 [AtomUI 控件 Token 设计规范](control-token-guidelines.md)。单个控件的 `token.md` 只记录该控件专属的 Token 语义、分类、使用范围和兼容边界，不重复全局 Token 系统规则。

## Semantic Part

Control 对稳定视觉区域提供公共定制入口时，必须遵循
[AtomUI Semantic Part 系统设计](../../architecture/systems/theming/semantic-parts.md)。Semantic Part 是主题兼容性契约，不是
模板节点清单。

- 除隐式 `root` 外，公开 Part 使用唯一 `.semantic-*` class，名称由语义职责产生，不包含 `PART_*`、序号或当前
  布局容器名称。
- Control 使用可重复 `[SemanticPart]` 显式声明非 root Part；`SelectorClass`、`ContractType`、cardinality 和 `Since`
  必须完整，不能依赖生成器从节点名称推断。
- 每个 Part 必须声明稳定 `ContractType` 和 `Single`、`Optional` 或 `Multiple` cardinality。
- 公共 Part selector 使用 owner 与生成 descriptor 的完整 `SelectorRoute`；`ContractType` 不得写成
  `Control.semantic-*`、`:is(Control).semantic-*` 或其他类型前缀。
- 包含 Setter 的 class-only Semantic Style 使用 `x:SetterTargetType="<ContractType>"` 提供 AXAML 编译期类型上下文；
  该指令不属于运行时 Part selector。
- 只有能够跨版本承诺的区域进入公开 descriptor；临时 frame、shadow、motion actor 和布局 wrapper 保留为内部
  Composition 节点。
- 所有内置 ControlTemplate、Desktop/Browser 主题和适用派生主题必须实现相同 Part 契约。
- AtomUI 自有 AXAML 模板使用静态 `Classes.semantic-*="True"` 声明 marker；字面量 `Classes="semantic-*"` 只作为
  Generator 兼容输入，不作为新模板规范。Application 用户继续消费 `.semantic-*` Selector，不接触 marker 声明语法。
- `Classes.semantic-*` 只能使用静态 `true`。不得使用 `False`、Binding 或其他动态值模拟 Optional Part 或状态切换；
  状态由 StyledProperty、伪类或有效状态属性表达。
- 动态创建节点使用生成的 semantic class 常量，并维护 logical parent、templated parent、回收和 re-template 生命周期。
- 父控件只建模自己拥有的稳定 route。复杂 Part 可以跨越多个由 owner 创建并用 `.semantic-scope-*` 明确标记的模板边界，
  不通过 Semantic Part 穿透未建模子 Control 的 internal 模板。
- Popup 和 Overlay 默认使用 Selector；跨 VisualRoot 本身不构成新增 Theme 属性的理由。
- ItemContainer Theme 和 Semantic Part Theme 只在真实 public 子 Control 允许完整 ControlTheme 替换时提供。
- AtomUI 内置主题不得使用 `.semantic-*` 实现默认视觉；marker 必须静态，用户 Semantic Style 的 class activator
  成本必须按真实候选节点和 listener 数量验证。
- 删除、重命名 Part、修改 selector class、收窄 ContractType 或改变 cardinality 必须按公共 API 破坏性变更处理。
- `ATOMUIGEN020-029` 必须在提交前清零；不得通过关闭诊断、把静态节点伪装为 `RuntimeCreated` 或放宽
  `ContractType` 掩盖模板缺失。
- 在新增或修改布局型 Semantic Part 前，必须完成并记录尺寸/状态基线矩阵：尺寸档及其默认含义、owner/Part 的尺寸属性
  owner、Token 映射、状态替代节点、模板路径和 Measure/Arrange 约束。必须先用失败回归证明基线缺失或尺寸混用问题，
  再实施根因修复；禁止先添加固定 Height、MinHeight、Padding 或像素偏移让示例通过。
- Semantic 示例必须先选择一套完整的 `Large`、`Middle`、`Small` 或 `Custom` 尺寸基线，再添加局部 Part Setter；未被
  Setter 覆盖的属性不得来自另一套尺寸分支。外部组件的 `default` 名称不能未经事实映射直接等同于 AtomUI 默认档位。
- 控件实现完成后必须同步 `semantic-part.md` 公共契约、`overview.md` 支持摘要、`implementation.md` 模板映射、
  主题契约测试和 NativeAOT 风险验证。

## 可自定义尺寸模式

只支持 `Large`、`Middle`、`Small` 三档预设尺寸的控件实现 `ISizeTypeAware`，其 `SizeType` 使用
`SizeType`。除预设尺寸外还允许调用方接管实例尺寸的控件实现 `ICustomizableSizeTypeAware`，其
`SizeType`、`SizeTypeProperty` 和 owner property 必须统一使用 `CustomizableSizeType`，不能只替换接口或只在
Theme 中增加 `Custom` selector。

`CustomizableSizeType.Custom` 表示退出 Theme 的预设尺寸分支，由调用方通过控件已有的 `Height`、`Width`、
`Margin`、`Padding`、`FontSize` 等布局或排版属性接管对应尺寸维度。它不是第四套固定 Token，也不应默认要求
新增 `CustomHeight`、`CustomMargin` 等重复 Public API。

对于允许 `Custom` 接管的尺寸维度，ControlTheme 按以下模式组织：

1. 先在适用的状态或方向作用域内设置一个稳定的基础默认值，保证 `Custom` 未显式覆盖时仍有可用基线。
2. 只为 `Small`、`Middle`、`Large` 增加预设 selector，并分别映射对应 Token。
3. 不为该尺寸维度增加 `SizeType=Custom` selector，也不把 `Custom` 合并进 `Middle` selector；否则 Theme 会继续
   占有本应由调用方接管的值。
4. 基础 setter 必须限制在实际适用的模式内。例如只有水平布局使用 block margin 时，基础 `Margin` 和三个预设
   selector 都应放在水平布局 selector 内，不能影响垂直布局。

```xml
<Style Selector="^[Orientation=Horizontal]">
    <Setter Property="Margin" Value="{atom:SeparatorTokenResource HorizontalMarginBlock}" />

    <Style Selector="^[SizeType=Small]">
        <Setter Property="Margin" Value="{atom:SeparatorTokenResource HorizontalMarginBlockSM}" />
    </Style>
    <Style Selector="^[SizeType=Middle]">
        <Setter Property="Margin" Value="{atom:SeparatorTokenResource HorizontalMarginBlock}" />
    </Style>
    <Style Selector="^[SizeType=Large]">
        <Setter Property="Margin" Value="{atom:SeparatorTokenResource HorizontalMarginBlockLG}" />
    </Style>
</Style>
```

直接使用控件时，调用方通过 `SizeType="Custom"` 和实例属性值接管尺寸。控件作为另一个 ControlTemplate 的
内部子控件时，模板节点上的属性值可能低于子控件 ControlTheme setter 的优先级；owner 应把子控件设为
`Custom`，并在 owner 的 ControlTheme 中使用作用域 selector 设置目标尺寸，不依赖模板节点属性碰巧覆盖成功。

测试至少覆盖：三档预设值与 Token 的映射、`Custom` 未覆盖时的基础默认值、实例属性覆盖、owner-scoped Style
覆盖，以及不适用方向或模式不受该尺寸规则影响。

## 控件成员排列建议

这是一条推荐规范，不作为强制约定边界。优化控件代码时应优先遵循现有控件的组织习惯，不要按个人偏好重排成员。其中控件类内部不得在公共契约之前放置 internal/private 实现成员，是为了保证契约入口稳定的强约束。

- 控件相关的 `enum`、小型公开类型通常放在控件类之前，便于先理解控件状态模型。
- 控件类开头优先放公共契约区，通常使用 `#region 公共属性定义`。
- 控件类内部的第一阅读入口必须是公共契约区。不得在 `#region 公共属性定义` 之前放置 `internal const`、`internal static` helper、跨控件 internal 协作 API、private helper、runtime field 或临时状态。
- 公共属性契约区建议先集中定义 `StyledProperty`、`DirectProperty` 等 Avalonia 属性注册字段，再按相同顺序集中放对应 CLR wrapper。
- `DirectProperty` 的 backing field 建议靠近对应 CLR wrapper，不放入普通 runtime 字段区。
- 公共事件建议单独成区，例如 `#region 公共事件定义`。`RoutedEvent` 注册字段和对应 .NET event wrapper 应保持在同一区域内。
- internal template/theme 契约建议放在公共契约之后，通常使用 `#region 内部属性定义`。这些成员包括模板依赖的 internal 属性、DirectProperty、状态 wrapper 等，不和普通 private 字段混放。
- 跨同模块控件使用的 internal 常量、属性和 helper 不属于用户公开 API，但属于内部协作契约，建议放在公共契约和 internal template/theme 契约之后，使用 `#region 内部协作 API`，不要抢在公共属性契约之前。
- 普通 runtime 字段建议放在契约区之后、构造函数之前，包括 template part、helper、disposable、运行时状态标志等。
- 构造函数区建议保持 `static` 构造函数在前，实例构造函数在后。
- 构造函数之后的方法不强制按访问级别排序。优先按控件功能流组织，例如 template 接入、属性变更分发、交互处理、布局计算、状态同步、渲染、表单适配等。
- 控件自身 API 成员应放在显式接口实现之前。这里的 API 成员不仅包括 `public` 方法，也包括作为控件扩展契约或生命周期入口的 `protected`、`protected virtual`、`protected override` 方法。
- `public` / `protected` API 方法可以靠近对应功能流，不要求全部放在构造函数后面；但如果是核心用户操作 API，应放在相对靠前且容易发现的位置。
- 控件实现的 public interface 也是对外能力契约。显式接口实现虽然语法上不是普通 public 成员，但应作为“接口契约区”组织，位置通常在控件自身 public/protected API 成员之后、private 实现方法之前。
- 接口契约区的优先级低于真正的 public/protected 控件 API 成员，但高于 private helper。
- 与显式接口实现直接配套的 protected virtual hook 可以留在同一个接口契约区，例如 `IFormItemAware.SetFormValue(...)` 与 `NotifySetFormValue(...)`。
- 如果接口只是纯内部协作接口，且不会作为控件对外能力被用户感知，可以按 internal 实现细节处理；但出现在控件 public 类型声明上的接口，默认按接口契约区处理。
- `protected override`、`protected virtual` 方法可以靠近其服务的功能块，不强制集中到一个 protected 区。
- explicit interface implementation 应按接口功能成区放置；对应的 `NotifyXxx` protected hook 建议留在同一区域。
- 大控件拆成 partial 时，按稳定功能域拆分，例如 Filter、DragAndDrop、AsyncLoad；不要按 public/protected/private 访问级别拆分。
- 代码优化时不要做大面积机械重排。只有当重排能明显改善阅读路径，且不掺杂行为修改时，才调整成员顺序。

## 控件 AXAML 视觉层级建议

这是一条推荐规范，不作为强制约定边界。优化控件 AXAML 时，应先保证可维护性、模板契约、视觉行为和交互语义不变，再减少视觉层级。

- AXAML 视觉树应保持必要最小层级。每一层容器都应有明确职责，例如布局、裁剪、背景/边框、命中测试、动画隔离、状态视觉或模板边界。
- 布局容器必须按实际需要的最小能力选择，不得在更轻量的容器能够完整表达相同测量与排列语义时默认使用 `Grid`。纯叠放使用 `Panel`，单轴顺序排列使用 `StackPanel`，停靠布局使用 `DockPanel`；只有确实需要行列定义、星号或自动尺寸分配、跨行跨列、共享尺寸组等网格能力时才使用 `Grid`。
- 将 `Grid` 替换为更轻量容器前，必须确认子元素的测量约束、最终排列区域、对齐、margin、z-order、裁剪和命中测试语义保持一致。不能仅凭节点名称判断容器可替换，也不能为了减少布局能力而引入额外 wrapper、C# 动态布局或行为补丁。
- 没有明确职责的 wrapper 应优先合并或删除，以减少最终 VisualTree 的深度和节点数量，降低模板实例化、布局遍历、selector 匹配和渲染遍历成本。
- 不要为了减少层级把 `ControlTemplate` / `ControlTheme` 中的功能视觉节点搬到 C# 动态创建。能用 AXAML 静态结构、`IsVisible`、selector 或 pseudo-class 表达的状态，优先留在 AXAML。
- 不要为了减少层级破坏 `PART_`、`/template/` selector、伪类、Design Token、资源绑定、ControlTheme key、template part 名称或样式入口。
- 不要合并承担不同职责的容器，例如裁剪层、动画层、命中测试层、边框背景层、popup shell 或 template contract 边界。
- 简化布局时优先替换无必要的 wrapper 和过度复杂的布局面板；删除或合并前必须确认圆角、裁剪、背景、命中测试、动画、焦点和 selector 命中行为不变。
- 在 `AddOnDecoratedBox`、`ButtonSpinner`、InfoPicker 输入壳体等外壳控件中嵌入文本输入时，外壳负责内容 padding、边框、背景、hover、focus 和 disabled 视觉；内部输入应使用 internal `EmbeddedTextBox`，由它提供 `SizeType=Custom`、`Padding=0`、`BorderThickness=0` 和无 chrome disabled 视觉。需要响应外层尺寸时继续绑定 `FontSize` / `FontFamily` / `FontStyle` / `FontWeight`。不要让外壳 content padding 与 `TextBoxToken` padding 叠加，也不要让内层输入再绘制自己的 disabled 背景或边框。
- AXAML 层级优化后应通过对应控件测试或 Gallery 走查验证外观、交互和主题切换不变。如果声明性能收益，需要提供前后 VisualTree 节点数、层级深度或测量数据。

### ControlTheme 模板边界

- 父控件的 `ControlTheme` selector 最多只进入自己的模板一层；禁止连续使用 `/template/` 穿过子控件模板继续选择其内部节点。
- 子控件需要专用模板、内部节点样式或交互视觉时，应新增 `internal` 专用子控件及其 `ControlTheme`，由子控件自己维护内部模板。
- 父主题可以根据自身状态设置直接模板子控件的属性，例如 `Foreground` 或 `IsVisible`，但不能依赖子控件的内部 `PART`、`TextPresenter` 或其他模板节点。
- 生产 AXAML 应由主题边界回归测试扫描：默认 ControlTheme 的嵌套 Style 遵守 Avalonia 12 单 template selector 限制；应用侧
  Semantic Style 的多段 `/template/` 仅在与 descriptor `SelectorRoute` 完全一致时允许。

## 布局、绘制与输入几何

- `DesiredSize` 是测量请求，包含外部 Margin；它用于自然尺寸、堆叠步进和 Dock 分配，不代表最终绘制或输入区域。
- 父布局分配排列槽，子控件的 `Arrange` 应用 Width/Height、Min/Max、Margin 和对齐；Stretch 的有效槽不能被期望尺寸意外压成零。
- `ArrangeOverride` 的 `finalSize` 与控件最终 `Bounds.Size` 是内容盒，不包含外部 Margin。绘制、背景、裁剪与命中使用实际内容盒。
- 子控件只有一个排列 owner。Canvas、专用面板和外层控件不能各自重复排列同一节点；父级依赖的子项几何必须有明确失效路径，不能只依赖聚合期望尺寸恰好变化。
- Raw pointer 坐标属于事件自己的 input root。跨根、变换和裁剪通过坐标转换及实际输入命中确认；仅平移原点后拼接未变换尺寸不能代替命中区域。
- 无界测量必须返回有限的期望尺寸；阴影、Padding 等占用的空间在测量与排列阶段使用同一份分配规则。

## 持续视觉工作生命周期

无限动画、Compositor 循环动画、自动播放定时器、倒计时刷新和其他周期性视觉工作必须以控件的有效可见性为运行边界。有效可见性同时包含控件自身和当前 Visual 祖先链；只检查实例 `IsVisible` 不能覆盖隐藏页面、隐藏容器或切页后仍保留在 VisualTree 中的场景。

- 手工调用 `Animation.RunAsync` 的持续 Avalonia 动画必须显式使用 `PlaybackBehavior.OnlyIfVisible`。不能依赖默认 `Auto`，因为手工启动动画的默认语义不会因有效不可见而暂停。
- 手工调用 `Animation.RunAsync` 的动画禁止使用 `IterationCount.Infinite`：Avalonia 会返回 faulted Task（`Looping animations must not use the Run method.`），无人观察时会在 GC 阶段触发 `TaskScheduler.UnobservedTaskException`。单实例持续动画应声明为 owner 私有的样式动画（`Style.Animations`，target 选模板叶子节点以避免样式作用域级联）；确需循环调用时使用有限迭代次数并观察返回的 Task。
- Compositor 的 `AnimationIterationBehavior.Forever` 动画和 `DispatcherTimer` 不会自动遵守控件有效可见性。owner 必须跟踪自身及 Visual 祖先链的可见性，在有效不可见时停止，在恢复可见时按控件语义重新启动。
- 暂停与恢复必须保持业务同步。纯装饰动画可以从初始状态重启；自动播放进度等与定时器耦合的状态必须和计时 owner 同步重置或恢复；基于绝对时间的统计值必须在恢复时先以当前时间重算，再继续周期刷新。
- 可见性订阅、animation cancellation、Compositor target 和 timer handler 必须由启动持续工作的控件持有，并在 detach、unload、re-template 或功能关闭的对应路径释放。
- 回归测试至少覆盖有效可见到不可见的停止路径，以及重新可见后的恢复路径。仅验证控件自身 `IsVisible=false` 不足以覆盖隐藏祖先场景。

## 输入控件验证集成

输入类控件不得在 Avalonia `DataValidationErrors` 之外另建一套独立 error 机制。AtomUI 的 Form、`InputControlStatus`、feedback 图标和 AddOn 视觉只能作为 native validation 的扩展投影：

- Error 状态以 `DataValidationErrors.HasErrors` / `DataValidationErrors.Errors` 为最高优先级；Form validator 产生的 error 也应写入对应内容控件的 `DataValidationErrors`。
- Form reset、验证成功或重新验证时只能清理由 Form 写入的 error，不能清掉 ViewModel、binding 或业务层写入的 native validation error。
- `InputControlStatus.Warning`、`FormValidateStatus.Warning`、`Validating` 和 `Success` 是 AtomUI 扩展状态；Avalonia 没有等价 warning 语义时，不应把 warning 写成 `DataValidationErrors`。
- AddOn 型输入壳体应根据 native validation error 计算有效视觉状态；显式 `Status=Error/Warning` 只能作为无 native error 时的手动视觉请求。
- 控件不应用手动 `Status=Error` 直接覆盖 Avalonia 原生 `:error` 伪类；`DataValidationErrors` 负责 error 伪类，AtomUI 手动状态应通过 `Status`、内部 effective status 或专用 selector 投射。
- 搜索按钮、range indicator、addon icon 等模板附属视觉不能直接读取原始 `Status` 判定 error；应读取壳体或宿主计算后的 effective status，使 native error 始终压过 warning/manual 状态。
- 组合输入控件若把 validation errors 转发到内部文本框，也必须确保外层输入壳体同步响应同一份 `DataValidationErrors`，避免“内部红、外框不红”或 Form 状态与 native error 分裂。
