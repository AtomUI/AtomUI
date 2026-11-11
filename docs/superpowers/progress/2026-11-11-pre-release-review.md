# AtomUI 发版前改动评审记录

2026-11-11，评审范围为 `release/6.0` 上 `v6.2.13..94134fe42` 的 4 个提交及当时的工作区。范围内有 167 个相对基线变更的已跟踪文件和 49 个未跟踪文件，共 216 个文件。`6.2.14` 是候选版本号，最终范围以工作区改动审查并提交后的发布候选为准。

先修复已复现的运行时问题，同时决定旧 API 和模板的兼容策略。其余问题按下列触发条件复验，发布验证使用最终候选源码。发版门禁见[发布准备规范](../../engineering/workflows/release-preparation.md)。

## 已复现的运行时问题

### ProgressBar 换模板时重复挂载内容（P1）

在已挂树的线形进度条上，把 `Border` 用作 `ProgressInfoContent`，再切换到另一个含 `PART_ProgressInfoContentPresenter` 的模板。新呈现器接收该 `Border` 时抛出 `InvalidOperationException`，旧呈现器仍是它的视觉父级。换模板时先释放旧呈现器持有的内容，再由新呈现器接管。同一控件实例在模板切换前后应只有一个视觉父级。 [AbstractProgressBar.cs](../../../src/AtomUI.Controls/ProgressBar/AbstractProgressBar.cs#L382)

### CountBadge 初始隐藏时漏挂目标控件（P1）

首次挂树前设置 `DecoratedTarget`、`Count=5` 和空字符串 `Content`。实测目标控件不在视觉树中，因为隐藏角标时跳过了同时负责挂载目标的 `PrepareAdorner()`。同一结构也影响已有的初始零计数路径。将目标挂载和替换独立于角标可见性，覆盖首次隐藏、显示及再次隐藏。 [AbstractCountBadge.cs](../../../src/AtomUI.Controls/Badge/AbstractCountBadge.cs#L278)

### TreeView 扩展多选范围时发出中间空值（P2）

用 Shift+↓ 将选区从 A–B 扩展到 A–C。期望只新增 C，实测 `SelectionChanged` 的（新增数、移除数）依次为 `(0, 2)`、`(1, 0)`、`(1, 0)`、`(1, 0)`。表单和事件订阅者可观察到空选区和部分选区。保留已选成员，只同步选区差量。 [TreeView.KeyboardNavigation.cs](../../../src/AtomUI.Desktop.Controls/TreeView/TreeView.KeyboardNavigation.cs#L289)

### MarqueeLabel 在悬停时无法恢复滚动（P2）

设置 `IsPausedOnPointerOver=false`，让指针留在溢出文本上，关闭并重新开启 `IsMotionEnabled`。两次都等待 dispatcher 处理门控，实测滚动停止后没有恢复。源码还显示布局和延时回调的启动条件缺少悬停判断。把溢出、挂树、运动门控和悬停策略统一到启动判断，分别验证禁用悬停暂停时的恢复、启用时重排和延时均保持暂停。 [AbstractMarqueeLabel.cs](../../../src/AtomUI.Controls/MarqueeLabel/AbstractMarqueeLabel.cs#L122)

## 已确认的兼容性变化

### Skeleton 公开基类和自定义模板入口变更（P1）

`AtomUI.Desktop.Controls.AbstractSkeleton` 移到 `AtomUI.Controls` 命名空间和程序集。显式引用旧类型的源码、程序集及使用旧命名空间的 AXAML 需要迁移。标准 `https://atomui.net` 的源码映射仍可解析新类型。只补一个类型别名无法恢复旧继承关系中的可赋值性。 [AbstractSkeleton.cs](../../../src/AtomUI.Controls/Skeleton/AbstractSkeleton.cs#L14)

旧模板的 `PART_ActiveAnimationLayer` 失去动画入口。受保护的 `StopActiveAnimation()` 改为请求重新评估，在其他运行条件仍成立时可能继续播放。`MotionEasingCurve` 现在从 compositor 渲染线程求值，已有自定义 `Easing` 若依赖 UI 线程，需要迁移到可在该线程执行的实现。决定是否保留旧入口，并在真实包和自定义模板消费场景验证。 [AbstractSkeleton.cs](../../../src/AtomUI.Controls/Skeleton/AbstractSkeleton.cs#L266) · [SkeletonShimmerVisualHandler.cs](../../../src/AtomUI.Controls/Skeleton/SkeletonShimmerVisualHandler.cs#L169)

### SpaceItemsAlignment 改变类型身份（P1）

枚举从 `AtomUI.Desktop.Controls` 移到 `AtomUI.Controls`，`Space.ItemsAlignment` 及其 `StyledProperty<T>` 的签名随之变更。旧显式枚举引用和已编译调用方受影响。若要维持 Desktop 兼容，在 Desktop 边界保留原枚举和属性契约，再映射给共享布局。 [SpaceEnums.cs](../../../src/AtomUI.Controls/Space/SpaceEnums.cs#L3) · [AbstractSpace.cs](../../../src/AtomUI.Controls/Space/AbstractSpace.cs#L35)

### Empty.Description 和 Result.Icon 改变公开签名（P1）

`Empty.Description` 从 `string?` 变为 `object?`，`Result.Icon` 从 `PathIcon?` 变为 `object?`，对应的 `StyledProperty<T>` 同时改变。原签名的已编译调用方需要迁移，原来直接按 `string` 或 `PathIcon` 读取的源码也受影响。若本版要保持兼容，保留原签名，通过独立内容属性扩展；有意保留新签名时，写清旧属性读取、绑定和重新编译的迁移方式。 [AbstractEmpty.cs](../../../src/AtomUI.Controls/Empty/AbstractEmpty.cs#L38) · [AbstractResult.cs](../../../src/AtomUI.Controls/Result/AbstractResult.cs#L22)

### ProgressBar 清除旧模板的可见性值（P2）

应用模板时，即使 `ProgressInfoContent` 为默认空值，也会清除百分比包装节点和状态图标的本地 `IsVisible`。自定义内容设值再清除还会覆盖原本的本地绑定。自带主题用 AXAML 表达显示状态，保留旧自定义模板的值和绑定；用带本地 `IsVisible=false` 及绑定的模板验证。 [AbstractProgressBar.cs](../../../src/AtomUI.Controls/ProgressBar/AbstractProgressBar.cs#L478)

## 源码审查发现与复验条件

### ImageLoading 和 Avatar

- **AsyncImage 在自适应布局可能一直处于 `Idle`（P2）。** 在垂直 `StackPanel` 中仅设置宽度，默认主题在 `Idle` 隐藏 `LoadingContent`，图片没有高度，视口检查持续拒绝启动。固定原点的控件从零尺寸增长时，有效视口矩形还可能保持不变。应让占位能撑开布局，并在布局尺寸和有效视口变化时重判，同时检查有效可见性。 [AsyncImage.cs](../../../src/AtomUI.Controls/ImageLoading/AsyncImage.cs#L396) · [AsyncImageTheme.axaml](../../../src/AtomUI.Controls/ImageLoading/Themes/AsyncImageTheme.axaml#L33)
- **Cover 解码可能欠采样（P2）。** `257×127` 覆盖 `16×16` 时，舍入得 `32×16`，当前比较可能选 `DecodeToWidth(32)`，Avalonia 12.1.5 的 Skia 实现会截断另一轴到 15。按舍入前的比例选择决定覆盖的轴，用真实 Skia 验证解码尺寸。 [RasterImageCodec.cs](../../../src/AtomUI.Controls.Shared/ImageLoading/RasterImageCodec.cs#L26)
- **Avatar 动态改尺寸会累积属性帧（P2）。** 宽、高和圆角反复写入 Template 优先级时丢弃释放句柄，Avalonia 可为同一属性创建新帧。首次设置自定义 `Size` 还可经同步 `SizeType` 回调重复配置。为每个派生值提供单一可替换来源，在条件退出时释放其值。 [AbstractAvatar.cs](../../../src/AtomUI.Controls/Avatar/AbstractAvatar.cs#L229) · [尺寸与圆角赋值](../../../src/AtomUI.Controls/Avatar/AbstractAvatar.cs#L365)

### 键盘、焦点和弹层

- **TreeView 选中背景遮住活动项提示（P2）。** 默认主题用同一个背景属性表达悬停、活动和选中，后置选中样式覆盖活动样式，同时 TreeView 移除了焦点装饰。给键盘活动项增加可以和选中态并存的视觉提示，验证选中行的 Tab 移入、移出及 Ctrl 方向键。 [TreeViewItemHeaderTheme.axaml](../../../src/AtomUI.Desktop.Controls/TreeView/Themes/TreeViewItemHeaderTheme.axaml#L129) · [TreeViewTheme.axaml](../../../src/AtomUI.Desktop.Controls/TreeView/Themes/TreeViewTheme.axaml#L45)
- **TreeSelect 可能截获用户模板中输入框的键盘操作（P2）。** 隧道处理接收路由中的任意 AtomUI `TextBox`，在项模板或附加区输入框按 Enter 可能提交活动节点并关闭下拉。限定为控件本身和实际搜索输入框，用原始按键验证。 [TreeSelect.KeyboardNavigation.cs](../../../src/AtomUI.Desktop.Controls/TreeSelect/TreeSelect.KeyboardNavigation.cs#L14)
- **Dialog 的右键激活和键盘焦点可能分离（待复验）。** 叠层仍在每次指针按下时激活，Avalonia 12.1.5 的指针聚焦处理只覆盖左键。打开两个非模态 Dialog，聚焦 B 的输入框后右键 A 的可见表面，检查键入和 Escape 的实际接收者。若确认为缺陷，仅在 Dialog 自身表面补足焦点交接，保留内容 Popup 的焦点路径。 [OverlayDialogPresenter.cs](../../../src/AtomUI.Desktop.Controls/Dialog/OverlayHost/OverlayDialogPresenter.cs#L1162)
- **ComboBox 离场滚动保护依赖默认面板（待复验）。** 调用方显式设置标准 `VirtualizingStackPanel` 会绕过新增的离场保护。复用 60 项、过滤 A*、限高 4 行、外部选择未实现 B29 的历史场景，仅切换 `ItemsPanel` 验证闪行与滚动。 [ComboBoxVirtualizingStackPanel.cs](../../../src/AtomUI.Desktop.Controls/ComboBox/ComboBoxVirtualizingStackPanel.cs#L9) · [原始复现记录](../plans/2026-11-10-combobox-filter-close.md#L40)
- **ComboBox 清除动作缺少键盘和自动化入口（P2）。** 非编辑模式下 Tab 可使按钮出现，按钮不可聚焦，现有控件未提供对应键盘清除命令。为同一个清除操作提供键盘入口及本地化自动化名称，验证清除后的焦点和绑定。 [ComboBoxTheme.axaml](../../../src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxTheme.axaml#L61)

### 模板、布局和资源生命周期

- **ProgressBar 自定义信息区尺寸可能过期（P2）。** 线形和 Steps 使用缓存尺寸。同一个 `Border` 改宽、模板内绑定文本变长、只换 `ProgressInfoContentTemplate` 时，缓存可能未失效。让布局读取当前期望尺寸，验证轨道和信息区边界。 [AbstractLineProgress.cs](../../../src/AtomUI.Controls/ProgressBar/AbstractLineProgress.cs#L159) · [AbstractGeneralStepsProgressBar.cs](../../../src/AtomUI.Controls/ProgressBar/AbstractGeneralStepsProgressBar.cs#L375)
- **Skeleton 微光可能覆盖自定义边框（P2）。** 合成层绘制在普通内容之上，当前裁剪矩形包含 `BorderThickness` 的区域。用生成的专用 Semantic Part Style 设置非零边框，比较活动帧和静止帧的描边，必要时裁剪到背景内缘或调整绘制层级。 [AbstractSkeleton.cs](../../../src/AtomUI.Controls/Skeleton/AbstractSkeleton.cs#L376)
- **Empty 的新内容入口缺少 Desktop 模板和语义部件接线（P2）。** 默认模板接受 `Image`、`Title` 及对应模板属性，但不显示它们。`Image` 仍进入互斥校验，并阻止 SVG 路径。对象描述另走没有 marker 的呈现器，`EmptyDescriptionStyle` 仍只命中文本部件。决定哪些入口由 Desktop 支持，保留文本部件既有契约，为对象呈现路径提供明确的样式入口。 [AbstractEmpty.cs](../../../src/AtomUI.Controls/Empty/AbstractEmpty.cs#L218) · [EmptyTheme.axaml](../../../src/AtomUI.Desktop.Controls/Empty/Themes/EmptyTheme.axaml#L15)
- **Empty 相同值的预设写入会推迟冲突校验（已记录的边界）。** 当派生类型有默认 `PresetImage`，将同一个值改为本地显式值时，有效值不变，冲突可能到再次挂树才抛出。用同一派生控件验证挂树中的动态来源切换，再决定是否改用确定性优先级或补齐校验入口。 [Empty 变更记录](../../controls/desktop/data-display/empty/changelog.md#L23)
- **Watermark 共享字形在重入时可能遗失订阅句柄（P2）。** 首次应用主题前景同步触发属性回调。若回调将同一字形挂到另一已排列宿主，外层赋值可覆盖内层保存的前景和订阅句柄。发布属性前建立可释放的 owner 槽或使用 owner 版本校验，验证两宿主重入及最后离树完整退订。 [TextGlyph.cs](../../../src/AtomUI.Controls/Watermark/Glyphs/TextGlyph.cs#L109)
- **Watermark 离树后的前景约定与实现不一致（P3）。** 文档约定无 owner 时回到黑色，现有实现保留上个主题的前景，测试也断言保留。决定离树契约后统一实现、测试和文档。 [TextGlyph.cs](../../../src/AtomUI.Controls/Watermark/Glyphs/TextGlyph.cs#L109) · [Watermark 变更记录](../../controls/desktop/feedback/watermark/changelog.md#L15) · [WatermarkTileRendererTests.cs](../../../tests/AtomUI.Controls.Tests/Watermark/WatermarkTileRendererTests.cs#L355)
- **Spin 自定义动画启动可同步重入（P2）。** `StartCustomAnimation()` 返回后才登记运行状态。派生实现若在回调中修改 `MotionDuration` 或请求重启，可再次进入启动，破坏启停配对。将启动过程纳入状态管理，并覆盖回调内变更属性的场景。 [AbstractSpinIndicator.cs](../../../src/AtomUI.Controls/Spin/AbstractSpinIndicator.cs#L272)
- **核对 SpinIndicator 和 MarqueeLabel 是否应跟随全局运动 Token（待确认契约）。** Desktop Spin 主题在父级绑定 `EnableMotion`，内部 SpinIndicator 未显式接收该属性。用关闭全局运动的主题验证内置与独立控件，明确需要传递的范围。 [SpinTheme.axaml](../../../src/AtomUI.Desktop.Controls/Spin/Themes/SpinTheme.axaml#L22) · [AbstractMarqueeLabel.cs](../../../src/AtomUI.Controls/MarqueeLabel/AbstractMarqueeLabel.cs#L108)
- **Card 清空模板时可能保留旧 LoadingHost（待复验）。** 旧 host 和指示器只在下一次 `OnApplyTemplate` 清理。Avalonia 12.1.5 清除模板时不执行该回调。验证 `IsLoading=true` 时清空模板，再切换加载状态是否仍操作脱树 host。 [AbstractCard.cs](../../../src/AtomUI.Controls/Card/AbstractCard.cs#L143)
- **Space 首项超宽时可能产生空行间距（基线已有边界）。** 固定项宽大于布局宽度时，首项触发换行分支，空行计入 `LineSpacing`。复验时保留单项超宽和正行距，核对水平和垂直布局的首行原点。 [AbstractSpace.cs](../../../src/AtomUI.Controls/Space/AbstractSpace.cs#L234)

## 性能、测试和文档改进

- **ComboBox 关闭时重复过滤。** 每次物理关闭都会刷新查询，即使 Escape 或外点关闭前没有待应用查询，固定打开期间的提交也可能触发类似全表过滤。按查询待应用状态刷新，并用自定义过滤器调用次数验证。 [ComboBox.cs](../../../src/AtomUI.Desktop.Controls/ComboBox/ComboBox.cs#L1180)
- **Space 默认排布可省一次遍历。** 每行单独计数可见子项，即使默认 `Justify=Start` 不使用计数。复用分行时的计数，或只为确实需要计数的分布方式执行。 [AbstractSpace.cs](../../../src/AtomUI.Controls/Space/AbstractSpace.cs#L329)
- **Watermark 每帧重复计算字形尺寸。** `Watermark` 与 `WatermarkTileRenderer` 都调用 `GetDesiredSize()`。把单次结果传下去，并核对任意单元尺寸、内边距和测试用 `MatrixRebuildCount` 是否有实际消费方。 [Watermark.cs](../../../src/AtomUI.Controls/Watermark/Watermark.cs#L170) · [WatermarkTileRenderer.cs](../../../src/AtomUI.Controls/Watermark/WatermarkTileRenderer.cs#L22)
- **核对为下游预留的入口。** `AsyncImage.ReleaseLoad()` 当前仅由仓库内测试调用，应对照真实回收方再决定保留时机。`AbstractListItem` 当前未接入 Desktop，`AbstractTag.CreateCloseButton()` 的按需路径在仓库内仅有测试派生实现，`DotBadge.IsBordered` 在 Desktop 默认主题也没有视觉效果。确认实际下游主题和使用方，再决定是否随本版公开及如何验收。 [AsyncImage.cs](../../../src/AtomUI.Controls/ImageLoading/AsyncImage.cs#L207) · [AbstractListItem.cs](../../../src/AtomUI.Controls/List/AbstractListItem.cs#L13) · [AbstractTag.cs](../../../src/AtomUI.Controls/Tag/AbstractTag.cs#L159) · [Badge 概览](../../controls/desktop/data-display/badge/overview.md#L61)
- **ComboBox 清除 Semantic Part 的版本元数据早于实际引入。** `Since="6.2.13"` 与该标签的内容不符。正式定版后同步源码和语义部件文档。 [ComboBox.SemanticParts.cs](../../../src/AtomUI.Desktop.Controls/ComboBox/ComboBox.SemanticParts.cs#L11) · [ComboBox Semantic Part](../../controls/desktop/navigation/combo-box/semantic-part.md)
- **审查新增测试的常驻价值。** 当时有 28 个未跟踪测试文件。按用例保留可独立捕捉用户行为、资源或兼容性缺陷的断言，合并重复覆盖，删除一次性探针和只锁定枚举总数等实现清单的断言。 [测试价值与生命周期规范](../../engineering/development/test-value-and-lifecycle.md#L51) · [TagExtensionTests.cs](../../../tests/AtomUI.Controls.Tests/Tag/TagExtensionTests.cs#L133)
- **视觉验收宿主规则应与原始场景一致。** AtomUI Gallery 和依赖 `atom:Window` chrome 的问题使用实际 AtomUI 宿主。针对普通 Avalonia 宿主的报告，验证其实际主题是否提供 `VisualLayerManager` 及 Popup overlay。Avalonia 12.1.5 的 Fluent 默认 Window 模板也提供 `VisualLayerManager`，应把报错说明限定到缺少 Popup 平台宿主和 overlay 的配置。 [协作规范](../../engineering/contributing/agent-guidelines.md#L78) · [AtomUI 视觉验收技能](../../../.agents/skills/atomui-visual-acceptance/SKILL.md#L20)

## 验证记录

- 临时 Headless 探针分别触发 ProgressBar 异常、CountBadge 目标缺失、TreeView 多次选区通知、MarqueeLabel 无法恢复。探针源文件已删除，Desktop 测试程序集已按清理后的源码重建。
- 清理后 `net10.0` 定向测试通过 371 项，Desktop 控件 146 项、共享控件 218 项、图片请求协调器 7 项。失败和跳过均为 0。测试审计 `python3 scripts/verification/test.py audit` 与 `git diff --check v6.2.13` 均通过。
- 最终候选仍需完成默认全量回归、以 `6.2.13` 为基线的包 API 和布局校验，以及 Gallery NativeAOT 验证。保留的破坏性变更应进入中英文迁移说明，并从对应 changelog 条目链接。
