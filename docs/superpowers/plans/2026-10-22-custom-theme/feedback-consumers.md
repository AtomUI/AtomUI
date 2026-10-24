# CT-15 反馈与展示控件执行计划

沿现有feature/custom-theme单agent执行，不创建分支/worktree、不提交。目录137项metadata、274个Own字段为范围，覆盖Alert/Card/反馈弹层、Timeline、Avatar/Tag/Badge/FloatButton、ImagePreviewer/Carousel/Result等实际终端与内部目标；一个修复不代替整族验收。

## P1 Timeline 的真实部件与默认输入

源码入口：Common TimelineIndicator及Desktop TimelineToken、TimelineItemTheme、TimelineIndicatorTheme。L03在Arrange写Dot.BorderThickness/CornerRadius本地值，L04在OnApply把DefaultIndicatorColor复制进IndicatorColor；T19的末项MinHeight没有消费者。先真实编译专用TimelineItemIconStyle/ContentStyle及公开Timeline/Item取得失败，保持方向、模式、pending/reverse、原root尺寸、rail溢出和可见内容。

- 将dot描边/半径及颜色fallback放ControlTheme，保留原boxSize=dotSize+borderWidth及半径一半；代码只保留实际layout。用有限转换器/编译绑定，不反射、不找Part写样式、不加时钟。IconHost按自身已测量盒排列，证明专用border/padding实际进入盒并保持未配置几何。
- 默认色保持原Primary，未配置IndicatorColor保持null并实时fallback；明确颜色、动态默认、清除和图标同实例更新。内部Global选择实际公开TimelineItem owner，独立primitive按Shared回退；不伪造identity。Default输入等价的现有代码绑定一并审查，Direct layout状态若不能Style表达，记录具体原因。
- Timeline11字段迁V2Revision1，ItemPaddingBottomLG明确依赖有效ItemPaddingBottom；其余按原Global/常量公式，明暗22个默认及公开ID/slot不变。未配置末项旧MinHeight0必须保持，历史Token资源默认48保留；LastItemContentMinHeight定义为明确Own配置才启用的OptionalOverride，非继承属性Unset回退0。最后可见item的content在两方向生效，0/清除及专用Style优先级验证，不把默认48自动施加导致旧布局变化。
- 先验证Setter/owner/依赖失败，再Skia旧新默认/配置/恢复、专用Style命中、真实布局与输入、GC及当前Mac NativeAOT。Pending布局用实际Pending状态，静态自定义PendingIcon矩阵与默认循环动画分开记录，不暂停默认循环制造稳定帧。其他平台及完整持续动效归CT25。
- 临时测试在Temporary/CT15Timeline，驱动/图像在外部临时位置；验证后删除，清理后Common/Desktop Timeline精确filter、NET8、成员审计、正式文档/CSV与作者自审。

P1实现、专项、清理及最终聚焦验证已完成，证据见 [结果](timeline-consumers-results.md)。P2继续根排版/其余Global、逐Own配置/极值和完整状态；其余控件的Own、Global、尺寸、弹层、完整业务状态和原生最终显示面仍待各自开工，不能计作CT15完成。

## P2 Timeline 排版、Global 与其余 Own 消费

基于 P1 完成后的确切源码，先采同一继承父节点的亮暗默认与已有公开字体配置，取得实际 consumer 失败。Timeline 与公开 TimelineItem 分别消费有效 Global FontFamily、FontSize、ColorText；未明确配置时保留真实 InheritanceParent 的字体与前景，不自动改变旧暗色文字。复用 Core 有限类型化继承订阅，扩展无 SizeType 的三输入字号与前景选择；ControlTheme 拥有 Setter，控件只提供受保护继承父的只读接口。公开本地值及专用 Title／Content Style 优先，清除、重挂、兄弟隔离与 GC 必须验证。

根已有背景／边框颜色读取 Timeline Global，section spacing 读取公开 TimelineItem Global，不自动增加边框或圆角。11 Own 在实际 dot／icon／rail、末项、pending、三模式、两方向逐字段验证明确值及零值，保留原始尺寸与内容。真实 Skia 比较旧新默认、A/A、1 DIP、A→B→A及输入，并执行当前 Mac NativeAOT。临时源与媒体清理后，再运行 Core helper、Desktop Timeline 与既有字号消费者的精确检查、NET8、成员和文档审查。默认颜色变更仍须单独审查，不以默认一致证明可读性。

P2已完成本包实现、专项、清理与最终聚焦验证，见 [排版／Global／Own结果](timeline-typography-results.md)。P3仍需Compact/RTL、完整业务和持续动效、极值及原生最终面；其他反馈展示控件继续，不据此关闭CT15。
