# 桌面控件 Token 读取重构实施计划

> 按本计划逐项执行。在每项开始前确定受影响的控件、测试与验收场景，完成该项的脚本视觉验收后再开始下一项开发。原有 Git 授权和测试范围约束贯穿执行过程。

**目标**　消除仅用于传递 Token 的属性和重复订阅，保持所有受影响控件的逻辑、交互和渲染与重构前一致。

**架构**　把供同一段 C# 计算或自绘使用的 Token 合并到现有 `OwnerTokenSet`。由模板直接渲染的值仍在 AXAML 消费。跨模板、语义部件、动画、公开覆盖与计算结果的属性按各自契约保留。每次资源刷新先更新整组值，再触发一次相关计算。

**技术栈**　AtomUI 主题资源、Avalonia 属性与 ControlTheme、xUnit、Skia 渲染用例、`atom:Window` 原生窗口与脚本截图。

**设计依据**　当前会话对 `fix/shadcn` 的全库审查，以及用户要求的逻辑与视觉等价、逐项脚本视觉验收。本文包含设计和可执行任务清单，提交实现前供用户审查。

## 范围与完成条件

- 在已有 `fix/shadcn` 工作树开展。以该工作树开始实施时的完整源码和依赖为初始基线，包括现有未提交修改。继续遵守本次先不提交的要求，执行过程不新建分支或 worktree。
- 范围是桌面控件、其共享基类、Core 的资源读取器、ColorPicker 和 GalleryBase。DataGrid、Extras 与图标包已纳入审查，已确认具备独立职责的路径见末节。Mobile 项目与其独立工作区排除在任务外。共用基类的现有公开及受保护契约仍需保持兼容。
- 表中每个编号有独立结论。涉及共享实现的一行列出多个控件时，每个受影响的终端控件都要有独立场景和验收结果，不能用其中一个控件的通过覆盖其他控件。
- 每个已修改的任务必须有结构或成本证据、逻辑等价证据、原生窗口脚本视觉证据、测试处置以及清理后的模块验证。单纯减少代码行数不足以决定合并。只读核实后保留的条目记录实际消费者、优先级或成本依据。
- 原生截图及差异图按任务登记在仓库外，留给用户审查。主任务统一管理这些目录，包括子代理交回的媒体。授权提交后，实际执行 `git commit` 前完成取证审计和清理。用户明确放弃对应工作时，也清理该范围的媒体。
- 出现逻辑失败、像素差异、资源释放问题或验收前置缺失时，把本项记为待解决。必要时撤下本项改动，独立且基于已通过状态的任务才能继续。不得调整预期来掩盖差异。

## 统一实现决策

1. **由消费位置选机制。** 普通模板属性在 AXAML 直接读取，前提是选择器可达、资源宿主与优先级一致。C# 计算和自绘成组使用 `OwnerTokenSet`。跨边界的最终投影仍使用现有属性通道，对共享宿主内部的重复观察单独收敛。
2. **保留原来的资源查找。** 区分裸 Global 键、随当前 owner 解析的 Effective Global、指定目标的 Effective Global、生成 AXAML 使用的裸 Own 枚举键，以及原有的包装 Own 键。固定键保留原键对象，`Type` 和 identity 的形式各自维持。沿用原查找宿主、实际主题变体、查找顺序和缺值行为。局部 `Resources`、`ThemeConfigProvider` 和用户属性覆盖都应到达原消费点。
3. **扩展现有内部接口。** 在 `OwnerTokenSet.cs` 增加固定资源键入口 `SetResourceKey(int index, object? key)`，以及整组回调构造重载 `OwnerTokenSet(StyledElement target, int count, Action changed)`。既有 `SetKind`、惰性读取和逐项回调继续服务 `InputControlFrame`。新模式先完成所有受影响槽的更新，再在至少一个有效值变化时回调一次。固定键用 `ThemePublishDelta.IsChanged` 跳过无关发布。保留原 `GetValue` 缺值为 `null` 的读取契约，迁移既有绑定时按该绑定的空值、缺值到 `UnsetValue` 和转换器规则投影，恢复原 Style 回退。对 Button 和 Calendar 的运行时换键，在单次更新中替换并解析整组键，随后通知消费者一次。只有首个换键任务证实确有需要时才增加内部批量换键入口。
4. **只保留有效状态和输出。** 一组读取器可以直接提供已解析的值，控件只有在保留已计算的布局或绘图结果时才额外缓存。尺寸、颜色和动画分别触发各自原有的失效范围。对只重绘的输入不追加 Measure。运行时 popup、adorner、模板替换和卸载的退出路径要配对释放。
5. **逐项衡量收益。** 多输入优先核对中转属性数、绑定或订阅数，以及一次资源变更触发的重算次数。单输入和已直接作用于最终属性的通道，先查找已有同宿主读取器或等价 AXAML 终点。新增读取器成本高于省下的绑定或会扩大生命周期时，记录保留依据。每次迁移量取能独立验证的最小值，不批量替换相似名称。
6. **维持注册和 AOT 契约。** 直接引用已有生成的 `*TokenKind` 和 identity。验证迁移后的 Token 消费发现和现有生效用例。额外发布验证仅针对实际触及的 AOT 或平台边界，在相关源码稳定后合批。

实现依据见 [OwnerTokenSet](../../../src/AtomUI.Core/Theme/Resources/OwnerTokenSet.cs)、[Token 消费规范](../../engineering/development/control-token-guidelines.md) 和 [资源键生成](../../../src/AtomUI.Generator/DesignToken/ResourceKeyClassWriter.cs)。

## 评审重点

| 易回归的入口 | 计划中的验证 |
| --- | --- |
| 固定键被替换成继承 owner，或局部资源覆写失效 | G01 对照裸 Global、继承 owner 的 Effective Global、显式 Effective Global、裸 Own 和包装 Own。为 `Type` 和 identity 形式设置不同局部资源值，验证原键命中。继承 owner 缺失或未注册时保留原回退，显式键的未注册错误仍按原路径报告。 |
| 一个发布里同时改多个尺寸，瞬间用新旧混合值 | G01、A01、A03 对整批输入和回调计数。用无关颜色更新证明布局不失效。 |
| 默认值在更换模板、宿主或弹层时失效，旧对象仍被事件引用 | G01 和各跨宿主任务做换 scope、重挂树及弱引用或订阅计数检查。 |
| 删除属性后破坏公开覆写、语义部件、动画状态或选择器优先级 | 每项先做属性消费者盘点。B、C 和 D 表独立验证样式覆写和状态往返。 |
| 截图见证的状态和实际操作轨迹不一致 | 同实例往返、原生 A/A、可检出的 1 DIP 扰动和复位、相同 A/B 轨迹分别取证。 |

## 每项固定流程

1. [ ] 定责。查本项属性的 AXAML、C#、测试和文档引用，记录输入键、原资源宿主、缺值与覆盖规则、结果输出、事件的获取和释放。以表内场景为最小范围，给每个场景写独立的状态与视觉判据。
2. [ ] 取修改前证据。复用现有精准用例，缺失时使用临时用例。兼容场景在 A 上应满足旧契约，并保存基线。优化证据可以是存在的重复绑定、重复刷新或分配。实质缺陷按同场景失败到通过验证。用相同的固定数据和动作轨迹构建并采样 A。
3. [ ] 实施最小改动。先跑本项精确测试和编译，修正任何逻辑及资源生命周期失败。批量输入的通知计数与未变值的布局状态用断言确认。涉及布局、焦点、命中或滚动时保留原始尺寸、标题、内容、限制及原操作顺序。
4. [ ] **本项立即做原生脚本视觉验收。** 同一临时驱动重放 A/B。检查图像和状态，两者都通过才能结项。当前项变化集中后再构建或采样，下一项若可沿用经核对的上一项 B 产物作为 A 则直接复用。
5. [ ] 清理验证代码并复验。写完证据文字后停止进程并清理本项临时用例、专用构建产物和无效映射，登记留供审查的项目外媒体目录。共享参数化驱动在后续仍有任务时留在项目外，完成最后一项后清理。最终测试采用显式 `--path` 的 iterate 或精确 `dotnet test --filter`，再执行 `git diff --check`。
6. [ ] 记结论。更新本表任务状态和关键数值，链接到一个统一的文字证据记录。完成后留工作区交付审查。先确认本项已结论或安全撤下，再进入下一项。

验证命令入口见 [按改动影响选择验证](../../engineering/workflows/affected-verification.md) 和 [测试价值与生命周期](../../engineering/development/test-value-and-lifecycle.md)。验收时按本行的源目录替换 `--path`，共享基类和桌面模板均发生变化时显式列出这两个目录。只有归属范围内的选择进入定向验证。

### 脚本视觉门槛

- 以默认 **Ant Design** 设计与主题为原生视觉基准。A/B 使用同一套主题及任务所需的 Ant Design 状态。已有其他主题的局部覆写与行为按原契约定向回归。
- 使用项目外的参数化临时 Demo，窗口继承 `AtomUI.Desktop.Controls.Window`，依场景使用真实的应用启动与主题注册路径。通过公开 API 和框架输入驱动，不用桌面坐标点击。固定文本、语言、数据、窗口尺寸和缩放，保留本次涉及的正常动效。测试埋点只观察。
- 每项修改前冻结完整可运行依赖产物，记录源码和产物指纹。初始 A 来自现有工作树，不从 HEAD 回滚工作区。后续 A 来自刚通过的上一状态。原生采样用当前窗口编号执行 `screencapture -x -o -l`，记录进程、窗口编号、客户区、缩放和实际加载程序集。
- 先验证同一版本同状态 **A/A 像素差为 0**，在独立实例引入 **1 DIP** 可检出的扰动，复位后差为 0。主 A/B 采用一致的完整操作历史，在同坐标、同尺寸逐像素比较，被测区域 **差异像素数为 0**。通用容差、图片缩放、偏移对齐和遮罩不得用于通过门槛。
- 每个像素断言搭配逻辑断言，例如实际 bounds、有效值、焦点、命中、滚动行为、动作次数或卸载后的存活对象。动效核对状态轨迹和时长，在可控且等价的阶段取样。采样条件无法稳定时保留结论缺口并解决原因，不能只删除动画或原触发步骤。
- 查看关键截图后再判定视觉通过。记录运行命令、A/B 指纹、差异像素数、实际状态值和视觉观察。媒体存放在项目外的登记目录，直到提交前清理。终态验收和用户审查使用同一份截图。Headless 渲染与原生实测分别记证据。

## G 阶段　先证明公共机制

| 任务 | 实现边界 | 本项脚本验收与定向检查 |
| --- | --- | --- |
| G00 基线和驱动 | 只准备会话临时驱动、取证目录和当前工作树的冻结产物。给终端控件划分参数化场景，复用同一比较工具。 | 在原生窗口演练普通控件和实际 popup 两个样本。A/A、1 DIP、复位全部通过。固定窗口身份和启动入口。 |
| G01 Core 读取器 | `src/AtomUI.Core/Theme/Resources/OwnerTokenSet.cs`。增固定键和整组回调。复用 `ThemePublishDelta` 和原查找，不改变现有逐项 API。 | `OwnerTokenSetTests`、`ThemePublishDeltaTests`、`InputFrameThemeVariableTests`。临时覆盖五类资源键、局部覆盖、缺值及转换器、两值同批一次回调、同值零回调、换 owner、释放与惰性读取。真实窗口中让 InputControlFrame 在原有 variant、错误与恢复态往返，比较边框、填充、焦点环。 |
| G02 固定键试点 | `src/AtomUI.Toolkits.GalleryBase/Shell/GalleryMediaBreakPointResolver.cs`。合并六个全局断点资源的观察，去掉其私有中转属性。保留阈值与枚举映射、判定顺序和公开断点事件。若一次资源发布会通知多个断点，沿用逐项模式和原通知顺序。 | 在 `atom:Window` 承载已开启断点模式的 GalleryBrowserShellView，并覆盖 GalleryContentMediaBreakHost 路径。对六个阈值分别验证下方、恰好等于、上方和返回，必要时用同一套局部资源覆盖把阈值放进可测区间。再固定内容宽度，一次发布修改两个阈值，对比 A/B 的 `MediaBreakPointChanged` 序列。断言实际宽度、断点和壳布局，原生 A/B 比较后跑 `GalleryShellViewCollapseTests` 及该模块定向验证。 |

G01 的初始精确命令如下。G02 以及后续任务通过本行源目录驱动 focused 选测。核心试点通过后再扩大控件范围。

```bash
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --filter 'FullyQualifiedName~OwnerTokenSetTests|FullyQualifiedName~ThemePublishDeltaTests'
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --filter 'FullyQualifiedName~InputFrameThemeVariableTests'
```

## A 阶段　成组布局、自绘和派生值

A01 和 A03 分别证明尺寸输入和配色输入能稳定迁移。其余任务按下表执行，相互独立的证据可复用同一驱动。

A01 分为两个独立验收单元。A01a 处理两档字号和三档线宽。A01b 处理其余间距、对比色和分段布局输入。

| 任务 | 改动入口与动作 | 必须验证的操作与结果 |
| --- | --- | --- |
| A01 ProgressBar、StepsProgressBar | `src/AtomUI.Controls/ProgressBar` 与 `src/AtomUI.Desktop.Controls/ProgressBar`。移除两档字号、三档线宽的中转，再合并间距、线形内置标签候选色、分段布局输入。实际字号、图标尺寸和模板结果沿用现有通道。 | 两个控件分别做 Ant Design 原生 A/B。检查横竖、SizeType 和固定宽高重选档、显式厚度、内外标签、0% 透明度、完成和异常状态、主题作用域迁移。仅改颜色保持测量有效，同次尺寸变更只重算一次。跑线形与分段 Token 生效、行为测试和现有 Shadcn 线宽与圆角覆写测试。 |
| A02 CircleProgress、DashboardProgress | 同 ProgressBar 目录。合并两个尺寸下限，各读自己的固定 Own 键，保留模板字号和图标输出。 | 各自检查小尺寸、指定宽高、完成和异常图标、轨道与扇区几何及主题往返。比较逐步变化时的绘制和原有布局失效计数。 |
| A03 Empty | `src/AtomUI.Controls/Empty` 与 `src/AtomUI.Desktop.Controls/Empty`。五个调色输入整组更新，汇成一次插画创建。 | Ant Design 浅深主题及局部配色往返。插画层次、标题、占位尺寸相同，自定义图像继续有效，单次整组变化只重建一次。复跑已有 Shadcn 插画配色测试。 |
| A04 DotBadge、CountBadge | `src/AtomUI.Controls/Badge` 与 `src/AtomUI.Desktop.Controls/Badge`。合并各自阴影颜色与扩展量，保留 Count 的阴影输出及调用方覆盖。 | 两种 badge 独立跑默认、动态变化和不存在阴影的分支。比较阴影区域、数字与位置。 |
| A05 RibbonBadge | Badge 目录内的丝带折角度量。合并偏移、暗化与变换。 | 左右附着放置、不同标题宽度、外部 Offset 和颜色覆写。比较折角轮廓和占位。 |
| A06 Separator | `src/AtomUI.Controls/Separator`、`src/AtomUI.Desktop.Controls/Separator`。合并文字间距、方向比例及竖向边距。 | 竖线、无标题横线、标题三种定位、尺寸变化。检查线段端点、宽高和标题间隙。 |
| A07 Carousel | `src/AtomUI.Desktop.Controls/Carousel`。合并箭头与分页偏移，保留有效 Margin 输出。 | 切换分页位置、箭头显隐、翻页及尺寸变化。断言按钮、分页与内容相对位置。 |
| A08 Result | `src/AtomUI.Controls/Result`、`src/AtomUI.Desktop.Controls/Result`。合并两项行高比例及状态图标尺寸、画刷。 | 标题与副标题字号覆盖、Success/Warning/Error、自定义图标、状态往返。比对文本行高及图标落位。 |
| A09 Segmented | `src/AtomUI.Controls/Segmented`、`src/AtomUI.Desktop.Controls/Segmented`。合并选中滑块背景与阴影。 | 横竖或相关布局分支、连续选择、主题切换、项目隐藏后的重新分配。核对滑块动画起止和落位。 |
| A10 Skeleton | `src/AtomUI.Controls/Skeleton`、`src/AtomUI.Desktop.Controls/Skeleton`。合并 shimmer 起止色，保留公开时长。 | 加载状态开关、普通和运动帧、主题切换。检查渐变颜色、周期与加载后内容。 |
| A11 BorderBeam | `src/AtomUI.Desktop.Controls/BorderBeam`。去掉宿主的重复参数通道，在保持原资源宿主的前提下共享四项读取。 | Card 边界、大小圆角、Count、Outset、自定义停靠点原地改色和比例、卸载与重挂树。核对原像素轨迹、停靠点即时更新、已验证的 GC 和缓存行为。 |
| A12 NotificationProgressBar | `src/AtomUI.Desktop.Controls/Notifications`。合并线宽、轨道与进度色。 | 定时通知从完整、进行中到归零，显示和关闭过程。比较轨道宽高、填充比例及清理。 |
| A13 BackTopProgressRing | `src/AtomUI.Controls/FloatButton`、`src/AtomUI.Desktop.Controls/FloatButton`。合并环的三个绘制输入，保留进度状态。 | 真实滚动容器中比较显示阈值、0/中段/满进度、点击回顶及环位置。 |
| A14 TextArea ResizeHandle | `src/AtomUI.Desktop.Controls/Input` 的 ResizeHandle。合并笔刷和线宽，保留模板尺寸。 | 可拉伸文本区执行框架指针拖动，断言捕获与释放、调整方向、最小尺寸和指示线像素。 |
| A15 FocusOutline | `src/AtomUI.Desktop.Controls/AdornerLayer`。合并描边画刷、厚度和偏移，保持原 adorner 资源宿主。 | 键盘聚焦、失焦、描边外置及内嵌、嵌套裁剪和经验证的大圆角场景。比较边界和绘制，检查失焦释放。 |

## B 阶段　含状态选择器、继承或运行时拓扑

| 任务 | 改动入口与动作 | 必须验证的操作与结果 |
| --- | --- | --- |
| B01 Slider | `src/AtomUI.Desktop.Controls/Slider`。先合并轨道尺寸及静态刻度输入，处理 Track 到 MarksElement 的共享数据。 | 单值、范围、横竖、被选中与未选中刻度、刻度命中和禁用。核对布局与画刷过渡，保留动画目标。 |
| B02 Steps | `src/AtomUI.Desktop.Controls/Steps`。先合并进度画笔，再合并各类连接线的计算输入，保留状态 selector 与末项覆盖。 | 横竖连接线、带副标题、末项、Panel/Navigation 模式与进度环。可视几何和连接线命中分别验证。 |
| B03 RadioButton | `src/AtomUI.Controls/RadioButton`、`src/AtomUI.Desktop.Controls/RadioButton`。合并指示器几何输入，动画及模板宽高仍走现有属性。 | 普通、选中、禁用与键盘选择往返。比较边框、圆点几何、点选区以及过渡。 |
| B04 ToggleSwitch、SwitchKnob | `src/AtomUI.Controls/Switch`、`src/AtomUI.Desktop.Controls/Switch`。合并边距和加载绘制输入，保留可动画属性。 | 普通和小号、开关两态、禁用、加载起止及作用域变更。断言滑块位置、点击区域和动画释放。 |
| B05 Collapse | `src/AtomUI.Desktop.Controls/Collapse`。合并 Header/Content 的默认内边距，保持 Custom 和调用方优先级。 | 尺寸切换、展开折叠、局部内边距覆盖及自定义模板。校验可见范围和动画结束位置。 |
| B06 TabControl、CardTabControl、CardTabStrip | `src/AtomUI.Desktop.Controls/TabControl`。合并基础分隔线输入和卡片原始圆角，保留实际输出。 | 线型及两种卡片终端、上/左方向、选中、关闭和溢出菜单。核对选中背景、墨条、间距和圆角。 |
| B07 NavMenu | `src/AtomUI.Desktop.Controls/NavMenu`。合并弹层框尺寸输入和触发项最小宽度计算，保留语义尺寸优先级。 | 折叠和展开菜单、展开子菜单、受限高度滚动与局部宽度覆盖。像素以及实际 viewport、滚动和命中一并对比。 |
| B08 TreeView 拖放 | `src/AtomUI.Desktop.Controls/TreeView`。拖放线宽与画刷用同一组来源，保留行头的继承与拖放预览原逻辑作用域。 | 拖到行前、行内、行后和取消，断言目标、插入线坐标和像素，验证卸载释放。 |
| B09 Calendar 范围条 | `src/AtomUI.Desktop.Controls/Calendar` 的范围条面板。批读缩进、字号和画刷，保留程序传入标签样式。 | 横跨一行和跨行日历范围，应用局部标签样式，切换月份再返回。比对首末端、标签和占位。 |
| B10 Timeline | `src/AtomUI.Controls/Timeline`、`src/AtomUI.Desktop.Controls/Timeline`。合并行高比例和默认节点色，保留公开几何与有效颜色输出。 | 默认与自定义节点、长短内容、缺尾线、字号及颜色覆盖，对比节点位置与线段。 |
| B11 ColorPicker | `src/AtomUI.Desktop.Controls.ColorPicker`。合并 ColorBlock 空色和棋盘格计算输入，检查 ColorSlider 共享图案读取，保留继承型样式。 | 空色、透明色、渐变、展开和关闭，颜色编辑及主题往返。比对斜线与棋盘格，检查回写和弹层状态。 |
| B12 AddOnDecoratedBox | `src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox`。合并运行时附加项的状态色输入。先按模板确认实际继承该路径的终端，至少核对 LineEdit、TextArea、SearchEdit、ButtonSpinner、Select/TreeSelect/Cascader、RangeDatePicker 和 RangeTimePicker 的消费位置。 | 每个受影响终端用同一参数化场景分别跑正常、警告、错误、禁用与恢复。覆盖自定义 Part 样式、优先级、内容替换和卸载。只覆盖共用基础类的独立实例不足以验收。 |
| B13 WaveSpirit | `src/AtomUI.Controls/Primitives` 的 WaveSpiritDecorator 及桌面 ControlTheme。合并范围、透明度与时长。 | 使用真实点击，覆盖默认和自定义画刷、不同宿主及取消。对比起止轮廓、时长、视觉裁剪和释放。 |
| B14 OverlayDialogPresenter | `src/AtomUI.Desktop.Controls/Dialog/OverlayHost`。合并开场和常规时长的观察，保留合法时长覆盖位置。 | 同一定位原点的打开、关闭、中途取消、重开。遮罩与内容焦点、缩放轨迹、释放和窗口位置均需等价。 |
| B15 Select、TreeSelect、Cascader | 三个桌面目录及 AbstractSelect。合并候选项高度及弹层内边距的计算读取，保留模板实际输出与匹配宽度。 | 各终端分别打开、筛选或展开层级、键盘选择、限高溢出、滚轮到边界与关闭。保持可见滚动条、候选行命中和 scroll chaining。 |
| B16 AutoComplete 终端族 | `src/AtomUI.Desktop.Controls/AutoComplete`。合并高度计算输入，核对 PopupBackground/MinPopupWidth 的 AXAML 终点。 | 常规、SearchEdit、TextArea 各自输入、候选为空及溢出、选择、取消、重新打开。核对弹层底色、宽高、选中项和键盘焦点。 |
| B17 Mentions | `src/AtomUI.Desktop.Controls/Mentions`。合并弹层最大高度输入并检查最小宽度的 AXAML。 | 用文本输入触发 mention，操作候选和长列表滚动。比较插入文本、锚点、可见区域及关闭后的焦点。 |
| B18 ComboBox | `src/AtomUI.Desktop.Controls/ComboBox`。同源读取行高和列表内边距，保留行容器高度更新通道。 | 可编辑及普通入口分别覆盖打开、筛选、选择、关闭、固定行数溢出。检查跨主题行高、视口、点击命中和清除流程。 |

## C 阶段　合并订阅，维持输出契约

| 任务 | 改动入口与动作 | 必须验证的操作与结果 |
| --- | --- | --- |
| C01 Button 变体 | `src/AtomUI.Desktop.Controls/Buttons/Button.Variants.cs`。按激活槽惰性观察，保持终端身份、预设色和阴影计算、模板结果。 | Button、DropdownButton、SubmitButton、ResetButton 各检查实际变体状态。覆盖使用 Button 样式键的 DialogButton 和 Gallery 的 IconInfoItem。局部 Own/Global 更新、hover/pressed/disabled、预设色和焦点环保持等价。DropdownButton 还需打开弹层，DialogButton 验证实际默认操作。 |
| C02 Calendar 适配器 | `src/AtomUI.Desktop.Controls/Calendar/Calendar.cs`。合并公历／农历适配器的三个订阅，保留模板输出。 | 公历→农历→公历、月份切换、选择与局部配置，检查日期单元格、标题、颜色和旧适配器释放。 |
| C03 OwnerThemeInputs | `src/AtomUI.Desktop.Controls/Primitives/OwnerThemeInputs.cs`。仅收拢独立或宿主缺席时的观察，继续按 Style 优先级输出继承属性。 | 分别验证 ImagePreviewerCover、Dialog/MessageBox、TimePicker/RangeTimePicker 的独立与宿主模式、卸载和宿主复接。保持模板后代读值、本地覆盖、空资源回退、弹层焦点和几何。每条宿主路径独立给结果。 |
| C04 Breadcrumb | `src/AtomUI.Desktop.Controls/Breadcrumb/BreadcrumbSeparatorManager.cs`。合并运行时分隔符的两个源，并维持每个分隔符的可定制入口。 | 动态增删项、文字与自定义模板分隔符、局部资源切换。比对内容、间距、焦点和旧分隔符释放。 |
| C05 Flyout | `src/AtomUI.Desktop.Controls/Flyouts/Flyout.cs`。合并定位目标上的默认阴影、时长和指针偏移来源。阴影与时长保持 Template 优先级，指针偏移保持 Style 优先级和自定义资源键。 | 打开、关掉、变更锚点再开。普通与按指针定位各自验证外部边界、阴影、位置、焦点、局部覆写、资源清除和旧宿主订阅数量。保留公开值优先级和非 Visual 资源宿主契约。 |
| C06 Drawer | `src/AtomUI.Desktop.Controls/Drawer`。合并默认宽高、推移、边缘颜色和厚度来源，维持公开覆盖。 | 左右或与改动有关的方向、父子 Drawer、推移、显式大小与边框覆盖、关闭恢复。核对在途动画及 Overlay 边界。 |
| C07 Space | `src/AtomUI.Desktop.Controls/Space`。同一 Token 读取一次，继续输出两个可独立覆盖的间距。 | Small/Middle/Large/Custom 往返，并在实际模板和应用样式优先级下改各自间距。比较换行和子项位置。 |
| C08 MarqueeLabel | `src/AtomUI.Desktop.Controls/MarqueeLabel` 及共享基类。合并两个默认 Token 观察，保留公开配置。 | 短文本、长文本滚动、显式覆盖、切主题及卸载。核对文字裁剪、速度、周期和停止。 |
| C09 GalleryShell 运行时样式 | `src/AtomUI.Toolkits.GalleryBase/Shell/GalleryShellView.cs`。仅收拢同一目标上可成组的 footer 等读取。 | 同一 Shell 布局切浅深色、改变窗口尺寸。比较 footer 边线及其余 Shell 几何，保留转换、缺值和优先级。 |

## D 阶段　针对终端 AXAML 转发和单值候选

这一阶段先验证能否直接保留原 owner、优先级和输出。如果单值重构需要额外观察器或导致实际路径成本上升，按本项记录保留证据。发生源码变更的每一行都执行独立视觉验收。

| 任务 | 核查和处理的入口 | 各项视觉验收条件 |
| --- | --- | --- |
| D01 ButtonSpinner、NumericUpDown | `src/AtomUI.Desktop.Controls/ButtonSpinner`、`src/AtomUI.Desktop.Controls/NumericUpDown`。复核七项手柄投影及边框厚度，等价时让终端主题直接提供实际属性。 | 两个终端各跑状态、尺寸、填充模式、禁用及独立 Own Token 覆盖。手柄仍与输入边界对齐。 |
| D02 WindowNotificationManager | `src/AtomUI.Desktop.Controls/Notifications` 的 `ThemePadding`。投影到最终 Padding 时保留位置选择器和公开 Padding 覆盖。 | 上下两类位置分别显示多条通知，切换局部间距，比较落位、堆叠和关闭。 |
| D03 RangeDateViewer、DatePanel | `src/AtomUI.Desktop.Controls/DateViewer` 的 `PanelSpacing` 和 `Internal/DatePanel.cs` 的 `WeekHeaderMinHeight`。同时核实 DateViewer/RangeDateViewer、DatePicker/RangeDatePicker presenter 的消费链及 Calendar 的局部覆写。 | 各受影响终端分别取证。选范围、换月和改变可见窗口大小，比较双面板间距、星期行、选择区域和弹层布局。 |
| D04 Form | `src/AtomUI.Desktop.Controls/Form` 的 `FormLayoutSpacing` 与错误、警告前景。模板间距优先直接读取，状态前景保留各自来源和优先级。 | inline 布局和错误、警告、恢复各自取证，检查表项位置、提示色和焦点。 |
| D05 CarouselPagination | `src/AtomUI.Desktop.Controls/Carousel` 的分页 `ItemSpacing`。只验证面板终点可达且作用域一致的路径。 | 多点分页及自定义局部间距，核对点位、当前项和焦点。 |
| D06 PaginationNav | `src/AtomUI.Desktop.Controls/Pagination` 的导航 `ItemSpacing`。保留跨已注册图标的宿主 Token 投影。 | 普通／简易模式页项位置、间距、跳页与禁用态。 |
| D07 CardTab 的厚度 | B06 所在 TabControl 目录的 `CardBorderThickness`。同时核对 AXAML 边框和 C# 向生成容器的绑定。两条路径都等价才删除中转。 | CardTabControl 与 CardTabStrip 分别验证边框、方向、语义覆写，以及生成和复用容器后的原有厚度。 |
| D08 纯时长转发 | 分别评估 CollapseItem、Expander 的 `MotionDuration` 与 NavMenuItem 的 `OpenCloseMotionDuration` 到 ContentViewport 的终点，以及 DialogSurface 自身的时长消费者。 | 每个改变的入口都重放开合、中断和默认或自定义模板。Dialog/MessageBox 只在查实无独立消费者并保持公共契约后处理。 |
| D09 AvatarGroup | `src/AtomUI.Desktop.Controls/Avatar` 的 `GroupOverlapping`。保留运行时 StackPanel 使用的 `GroupSpace` 更新。 | 重叠宽度、折叠计数、显示溢出头像、尺寸变化。 |
| D10 ListBox、ListView | 各自桌面目录的 `SplitLineWidth`。评估是否能在既有读取通道共享，保留本地 `BorderThickness`。 | 每种列表分别检查选中、hover、行分隔、边框覆盖和滚动。 |
| D11 Descriptions、Upload | 各自的 `RelativeLineHeight`。保留计算后的模板行高。 | 分别验证有框和普通描述项，以及上传列表标题的换行、字体变化与行高。 |
| D12 TimeView | `src/AtomUI.Desktop.Controls/TimePicker/TimeView` 的 `SpacerLineWidth`。保留缩放换算和最终宽度输出。 | 时间选择面板中的间隔线、列宽、选项对齐和切换主题。涉及像素舍入时在当前可测缩放下按目标证据验证。 |
| D13 Tour | `src/AtomUI.Desktop.Controls/Tour` 的 `PopupMarginToAnchor`。复核与调用方间距的求和及原位置更新。 | 多位置切换、目标边界、遮罩、切步与关闭。保留锚定且无裁剪差异。 |
| D14 Rate | `src/AtomUI.Controls/Rate`、`src/AtomUI.Desktop.Controls/Rate` 的 `HoverScale`。保留计算后的变换。 | 用框架指针连续经过相邻图标、选中与清除，检查放大幅度、命中和最终评分。 |
| D15 OptionButtonGroup、SplitButton | 各自目录的选中边框与左右分隔画刷。查原状态来源及颜色覆盖。 | OptionButtonGroup 核对普通、选中、禁用。SplitButton 核对普通、主按钮、Danger、禁用。分别比较边线位置和像素。 |
| D16 小型布局部件 | 候选列表的 `CandidateIndicatorMargin`、WindowTitleBar 的 `HorizontalSpacing`、GalleryShowCaseMetadataRowPanel 的 `PairSpacing`。在各自源码确认真实消费者后决定处理。 | 三个部件分别按候选行、真实标题栏及元数据行的原布局重放。比较间距、裁剪、可按区域和窗口状态。 |

## 保留契约的范围

- **跨部件的继承输入。** Button、Calendar、DatePicker、DateViewer、Dialog、ImagePreviewer、TimePicker、ListView、Menu、Select、Tab、TreeView、Pagination、RangeInfoPickerInput 和 ColorPicker 的继承型属性为共享模板提供 owner fan-out。已有专门的 C03 处理独立宿主时重复的资源观察。模板后代继续按已建立的宿主与身份契约消费。
- **已有明确用途。** InputControlFrame 是 `OwnerTokenSet` 已有消费者，随 G01 验证逐项回调。DataGrid 的表头间距、展开器、冻结列阴影及列资源宿主各有用途。图标、Tag、FloatButton 以及非 Visual TextGlyph 的外部输入或资源宿主契约保持原状。Extras 没有本次确认的成组冗余输入。
- **直接的终点绑定。** SplitterPanel、TreeView 拖放预览等单个资源直接送到实际目标。ToolTip 的 popup 转发、Masonry 的面板转发与 Window 原生阴影均有实际消费者。此类路径只在发现更简的等价终点且仍保持原 owner 与优先级时进入对应任务。

## 状态与证据

2026-11-19，计划表 61 个编号均已结项。A01 分两次独立验收。D 阶段实际修改 D01、D08，其余候选按各自的消费和优先级契约保留。
已在 `fix/shadcn` 的清理后源码完成最终定向复验，等待工作树审查。当前 HEAD 为 `eab63ea2c`。

当前状态和验证证据见 [执行记录](../progress/2026-11-18-owner-token-set-refactoring.md)。记录包含任务编号、源码或产物指纹、精确命令、性能或结构数据、逻辑断言、A/B 像素、结论和清理结果。截图、录屏和逐帧差异图在项目外按任务登记，提交前清理。执行日志按各项的测试生命周期清理。

所有实施任务完成后按实际影响合批复验已改模块，确认工作树只保留起始改动和本次验收通过的改动，验证代码和专用构建产物均已清理。仍待审查的媒体在项目外独立登记。授权提交后先逐个运行 `evidence.py audit` 与 `evidence.py cleanup`，确认媒体已清除且仓库、暂存区无副本，再按授权范围提交。任何一个未完成的验收仍可定位到具体任务和受影响范围。
