# AtomUI 精确主题定制问题清单

配套入口：[调研与审计总览](overview.md)。代码基线与范围、原始候选、Ant Design 源码链路和分期建议见总览。

本清单中的“确认”表示已核对源码消费链、树拓扑或写值路径，**不表示已经运行复现**。
P1：阻断核心定制合同，建议优先验证；P2：局部配置失效或能力缺口；P3：工具与设计完整性。
待运行风险、兼容性决策与合理默认分别标注，不混为已复现 Bug。

为控制表格宽度，以下缩写只表示源码目录，所有路径以 AtomUI 仓库根目录为基准：

| 缩写 | 路径 |
| --- | --- |
| Core | `src/AtomUI.Core/` |
| C | `src/AtomUI.Controls/` |
| D | `src/AtomUI.Desktop.Controls/` |
| G | `src/AtomUI.Desktop.Controls.DataGrid/` |
| P | `src/AtomUI.Desktop.Controls.ColorPicker/` |
| B | `src/AtomUI.Toolkits.GalleryBase/` |
| V | `controlgallery/AtomUIGallery/` |

## 1. 跨控件的根因

### S01 · P1 · ControlTheme 绕过组件 Effective Global

`Core/Theme/Resources/TokenResourceExtension.cs:21-31` 使 SharedTokenResource 读取普通全局 key；它不从 TargetType、BasedOn 或祖先推断 identity。`Core/Data/TokenResourceBinder.cs:57-66` 已提供 owner-aware 控件 key；生成的 XxxTokenResource 也支持相同语义。

核心例子：`D/Buttons/Button.cs:1141` 的色态读取 Effective Global，`D/Buttons/Themes/ButtonTheme.axaml:240,249-280,379-386` 的边厚、高度、圆角、图标尺寸和禁用色却读取 Shared。因此只改 Button identity 的品牌配置会产生不同视觉维度各读一套值的问题。

其他已核位置：

| 家族 | 证据 | 影响 |
| --- | --- | --- |
| 输入共享框体 | `D/Primitives/AddOnDecoratedBox/Themes/InputControlFrameTheme.axaml:35,48,59,73,87` | LineEdit/TextArea/Select/TreeSelect/Cascader/NumericUpDown/InfoPicker 的主色、校验色、半径、边厚和 active shadow 无法自动使用各自 owner 的覆盖 |
| Select | `D/Select/Themes/SelectTheme.axaml:143,164` 等 | 选项字号、高度、placeholder、popup 背景/禁用色等未形成完整 Select 配方 |
| Avatar | `C/Avatar/Themes/AvatarTheme.axaml:51,97,100,103,106` | motion 和圆角不响应 Avatar identity 的 Global 覆盖 |
| Icon | `C/Icon/Themes/IconTheme.axaml:17-20,26-27,31-32`；`C/Icon/Themes/IconPresenterTheme.axaml:7` | 尺寸、基础图标色、主色和 info 背景绕过相应 identity |
| ColorPicker | `P/Themes/AbstractColorPickerTheme.axaml:14-16,25,42,47,51-62,69,76-79` | 常态、hover、error/warning、圆角、字号和 disabled 不完整响应组件覆盖 |
| DataGrid | `G/Themes/DataGridColumnHeaderTheme.axaml:64`；`G/Themes/Filters/DataGridFilterIndicatorTheme.axaml:22,27,32` | 表头字重及过滤器状态色使用全局值 |

Desktop 共 1,716 条 Shared 动态消费/全局绑定命中，另有 166 条静态 Value；数量较大的家族含 Primitives、Buttons、Upload、TabControl、Input、Cascader、Steps、OptionButtonGroup、ImagePreviewer、Select、Calendar、OtpLineEdit、Pagination。全部候选位置保留在 CSV。

方向：逐家族明确 public owner，把应响应控件覆盖的消费接到正确 identity；组合模板接受 owner 视觉输入。保留刻意共享值和公开子控件自身 identity，禁止机械批量替换为内部 Part 类型。

### S02 · P1 · 静态动效读取不属于当前 owner 的动态资源

`Core/Data/SharedTokenResourceValue.cs:22-28` 从 Application 按应用 ThemeVariant 查询并返回普通值，目标对象/属性没有用于 owner scope 查询，也没有更新订阅。Desktop 有 166 次使用，分布于 74 个文件；DataGrid 4 次，ColorPicker 3 次。

代表：`D/Buttons/Themes/ButtonTheme.axaml:227-231`、`D/Switch/Themes/ToggleSwitchTheme.axaml:55`。已存在 Transition 的时长不能通过这条读取链自动跟随局部主题或后续切换。

方向：验证 Transition 的可绑定属性、clock 挂载和释放边界后改用 owner-aware 更新；Transitions 继续放 ControlTheme 的 style setter，不能移入模板构造内容。不能只将普通值改成永不释放的 Application 订阅。

### S03 · P1 · DataGrid 17 个公开 Token 被提前派生的活跃别名截断

调用链已经交叉核实：

```text
Core/Theme/Compilation/ThemeCompiler.cs:523-527
  AssignEffectiveGlobalToken
  descriptor.Evaluate -> DataGridToken.CalculateTokenValues
    TableHeaderBg = HeaderBg 等
  ApplyAllValues(config.OwnTokens) -> 用户的新 HeaderBg
  冻结并投影 HeaderBg 与仍为旧值的 TableHeaderBg
  DataGridColumnHeaderTheme.axaml:72 -> 读取 TableHeaderBg
```

`G/DataGridToken.cs:333-362` 维护别名；活跃消费者还包括 `G/Themes/DataGridCellTheme.axaml:49-50`、`G/Themes/DataGridRowTheme.axaml:66,82,85,98`、`G/Themes/DataGridTheme.axaml:204,263-302`。

受影响的 17 个公开语义属性：`HeaderBg`、`HeaderColor`、`RowHoverBg`、`RowSelectedBg`、`RowSelectedHoverBg`、`RowExpandedBg`、`CellPadding`、`CellPaddingMD`、`CellPaddingSM`、`BorderColor`、`HeaderBorderRadius`、`FooterBg`、`FooterColor`、`CellFontSize`、`CellFontSizeMD`、`CellFontSizeSM`、`FilterDropdownBg`。

补充边界：当前Generator也把可访问的internal属性纳入Own schema，因此手动配置`TableHeaderBg`等内部别名可规避对应问题（`Core/Theme/Configuration/ThemeConfigNormalizer.cs:239-244`；`src/AtomUI.Generator/DesignToken/ControlTokenPropertyWalker.cs:155-216,290-295`）。问题是公开语义Token与别名消费者不同步，不是相关视觉绝对不能配置。

方向：优先让主题直接消费稳定公开语义值，移除重复别名；必须在覆盖后计算的真正派生值另行明确阶段。不要简单在 Own 覆盖后再次 CalculateTokenValues，否则会覆盖用户的其他显式值。

### S04 · P2 · 跨根字体投射合同不完整

`D/Popup/Themes/PopupRootTheme.axaml:26`、`D/Popup/Themes/OverlayPopupHostTheme.axaml:21` 等显式设置 Shared 字体、字号、前景，字重/字形为 Normal。即使 ThemeContext bridge 正常，也不代表 PlacementTarget 上的局部字体/字重能自然跨独立视觉根继承。

方向：定义 Native Popup 与 Overlay 两路排版投射，并验证 owner 的局部配置。当前**没有证据证明整个 Popup ThemeContext 丢失**；已有 CrossVisualRoot、CrossNestedOwners 和 RuntimeCreated 路由应继续复用。

### S05 · P1/P2 · 多个家族的 Own 覆盖没有进入下游派生结果

这是S03以外的另一组确定性消费断点，不能简单把“无直接引用”解释为“没有任何用途”：

| 家族 | 计算与消费者 | 差距 |
| --- | --- | --- |
| Button / HyperLinkButton / DropdownButton / OptionButton | `D/Buttons/ButtonToken.cs:285-296,368-390`；`D/Buttons/HyperLinkButtonToken.cs:38-62`；`D/DropdownButton/DropdownButtonToken.cs:44-74`；`D/OptionButtonGroup/OptionButtonToken.cs:130-153` | ContentLineHeight三档参与默认padding派生，但覆盖时padding已经算完，且没有实际LineHeight消费者；只覆写行高不改变内置布局。OptionButton还有S06的独立错误。 |
| Skeleton | `D/Skeleton/SkeletonToken.cs:79-90,99-109`；`D/Skeleton/Themes/AbstractSkeletonTheme.axaml:33-37` | GradientToColor/GradientFromColor先生成三个LoadingBackground；后续覆盖颜色不会重建brush。FromColor还直接给静态背景，可能出现静态底色改变而active渐变仍旧色。可显式覆盖三个brush规避。 |
| ColorPicker | `P/ColorPickerToken.cs:159,161`；`P/Themes/ColorSlider/ColorSliderTheme.axaml:53`、`P/Themes/ColorSlider/GradientColorSliderTheme.axaml:46`、`P/Themes/ColorSlider/ColorSliderThumbTheme.axaml:34,36` | HandlerSizeSM先派生SliderSize/SliderThumbSize，单改HandlerSizeSM不会传递；可分别覆盖两个下游Token规避。 |

方向：先明确这些字段是输入、最终可覆写值还是兼容保留；对真正的依赖保持有序求值，并让最终显式覆盖具有最高优先级。不要全控件一律重算，避免覆盖用户已配置的下游值。

### S06 · P1，待运行验收 · OptionButton 尺寸配方的公式错配

`D/OptionButtonGroup/OptionButtonToken.cs:150-151` 的ContentPaddingLG读取Small的高度/字体/行高；`:152-153` 的ContentPaddingSM使用`controlHeightLG - controlHeightLG * controlHeightLG`，正常高度大于1时纵向padding恒被夹到0。`D/OptionButtonGroup/Themes/OptionButtonTheme.axaml:50,93`实际消费这些值。

这是明确的静态公式错配，直接妨碍字号/高度/密度品牌配方。需要以原始三档和Custom约束补失败证据，再统一单位与档位；本轮未运行渲染，不宣称已复现截断或已修复。

## 2. C# 本地值、缓存和实际框体的阻断

| ID / 优先级 | 位置 | 断点、影响及改造方向 |
| --- | --- | --- |
| L01 / P1 | `D/Buttons/Button.cs:574-582,1201-1207` | Circle/Round 在 Measure 直接写公共 CornerRadius，本地值在 Shape 回 Default 时未清理，可能压过新 Token/Style。保持公开值，由可覆盖的有效半径计算形状；几何圆形本身合理。 |
| L02 / P1 | `D/Input/Themes/LineEditTheme.axaml:12-32`；`D/Input/AbstractTextInput.cs:288-290,493-513` | root 未将 CornerRadius/BorderThickness/Padding 投射到实际 AddOnDecoratedBox；中继仅含 brush。LineEdit Parts 没有 frame，RootStyle 的框体参数缺真实入口。统一 root/frame 投射，验证 addon/compact/各 variant/校验态。 |
| L03 / P1 | `C/Timeline/TimelineIndicator.cs:264-265` | Arrange 每次写 PART_Dot.BorderThickness 和 CornerRadius，本地值压过可命中的 Timeline ItemIcon 专用 Style。改为主题默认/可覆盖的派生输入。 |
| L04 / P1 | `C/Timeline/TimelineIndicator.cs:196`；`D/Timeline/Themes/TimelineIndicatorTheme.axaml:34` | `IndicatorColor ??= DefaultIndicatorColor` 把首次动态主色复制为本地 brush，没有默认色变化传播；已有实例存在旧色风险。使用声明式 null fallback/有效色，不固化默认值。 |
| L05 / P1 | `C/ProgressBar/AbstractCircleProgress.cs:277-298` | 固定 `circleSize*0.15+6` 后重写公共 FontSize，状态图标尺寸也为本地值；尺寸变化可覆盖用户字号。保留自适应默认，使显式排版和 Part Style 有效。 |
| L06 / P1 | `C/Primitives/ArrowDecoratedBox/ArrowIndicator.cs:58-76` | geometry 缓存仅在空或 force 时创建，没有 ArrowSize 变化重建路径；动态 SizePopupArrow 更新后旧形状/自然尺寸风险。按依赖失效 measure/render/geometry。 |
| L07 / P1 | `C/Rate/RateItem.cs:183-187,254-280`；`C/Rate/RateCharacter.cs:97-98` | VisualBrush 内 RateCharacter 只接 Character/Foreground/FontSize，不处于普通文本逻辑继承链，FontFamily/Style/Weight 未传递也未参与更新。显式传递完整排版合同。 |
| L08 / P1 | `D/Calendar/Internal/CalendarRangeBarPanel.cs:426,476-477` | 动态标签本地写 White、FontSize=12，ApplySegment 反复写 padding=8；范围条只开放 Background/Height，没有稳定标签 Part。增加 owner 视觉输入/稳定 Part，保留描述对象 scoped host。 |
| L09 / P2，待运行 | `P/GradientColorPicker.cs:136-138,315,319` | 子 TextBlock.Foreground 是颜色快照，主要由 ActivatedStopIndex 变化刷新；Foreground/InactiveStopTextColor 更新无相应分支。已有文本的主题/disabled 切换有旧色风险。使用状态样式或有释放 owner 的实时绑定。 |
| L10 / P2 | `P/Themes/GradientColorPickerTheme.axaml:30,68` | PART_ColorTextPanel 实际是 WrapPanel，disabled selector 却匹配 TextBlock，静态类型不能命中。修正正确节点及继承前景，同时处理 L09 的本地值。 |
| L11 / P2 | `C/Primitives/WaveSpiritDecorator.cs:26-30,198,230-231,273-278` | 已声明的 easing 属性更新被监听，但 painter 仍固定 CubicEaseOut；时长额外+50ms，范围 clamp<=8。真正消费 easing，并明确范围/时长合同，验证共享波纹家族。 |

L04/L06/L09 的最终用户可见时序需 focused 运行验证；本轮没有声称已经复现。其他行同样需要修复前失败证据和修复后验收。

## 3. 有定义但缺少正确视觉消费者的 Token

| ID / 优先级 | 定义/实际路径 | 差距与处理 |
| --- | --- | --- |
| T01 / P2 | `C/Icon/IconToken.cs:23,45`；`C/Icon/Themes/IconTheme.axaml:37` | FallbackColor 已定义，实际 FallbackBrush 固定 White；接入现有 Token，保留 White 作为合法默认。 |
| T02 / P2 | `D/Buttons/ButtonToken.cs:16,299` | FontWeight 无生产消费；ButtonTheme 应消费，保留实例优先级。 |
| T03 / P2 | `D/Buttons/ButtonToken.cs:56,342`；`D/Buttons/Themes/ButtonTheme.axaml:383` | DefaultBorderColorDisabled 无消费；实际使用另一 BorderColorDisabled。明确兼容/弃用或接入区别语义，不能继续展示为有效参数。 |
| T04 / P2 | `D/Input/LineEditToken.cs:11-28` | InputFontSize/LG 未在 LineEdit 本体消费，SM 无生产消费；把三档接回正确 owner 和 IsCustomFontSize 路径。 |
| T05 / P2 | `D/Select/Themes/SelectFilterTextBoxTheme.axaml:15-23` | 内部过滤输入借 LineEdit identity；Small 仍读 InputFontSize 而不是 SM。以 Select 真实排版合同消费，避免跨控件配置污染。 |
| T06 / P2 | `D/Select/SelectToken.cs:75`；`D/Select/Themes/SelectCandidateListItemTheme.axaml:7` | OptionPadding 无消费，复用的 ListViewItemTheme 从 ListView 取 padding；Select 主题需要接入自己的选项语义。 |
| T07 / P2 | `D/Input/TextAreaToken.cs:24,50`；`D/Input/Themes/ResizeHandleTheme.axaml:14-15`；`D/Input/TextAreaDecoratedBox.cs:160-164` | ResizeHandleSize 未消费；命中区8×8、绘制偏移2/长度7与4/线宽1固定。同步输入、命中区、测量和绘制几何。 |
| T08 / P2 | `D/RadioButton/Themes/RadioButtonTheme.axaml:74` | RadioBgColor 未消费，未选中背景直接 Shared ColorBgContainer；接入 Own 语义。 |
| T09 / P2 | `D/ListBox/Themes/ListBoxItemTheme.axaml:87-90`；`D/ListView/Themes/ListViewItemTheme.axaml:79-82`；`D/ComboBox/Themes/ComboBoxItemTheme.axaml:51` | 三类 ItemDisabledColor 未消费，读取 Shared ColorTextDisabled；确认派生候选列表 owner 后逐家族接入。 |
| T10 / P2 | `D/PopupConfirm/PopupConfirmToken.cs:18,23,53-54` | PopupMinWidth/PopupMinHeight 无消费，配置最小弹层尺寸不起作用；投射到实际 surface。 |
| T11 / P2 | `D/Tooltip/ToolTipToken.cs:44,55`；`D/Tooltip/Themes/ToolTipTheme.axaml:25` | MotionDuration 未消费，实际为 Shared Mid；接入正确 Token 和动态更新机制。 |
| T12 / P2 | `D/ImagePreviewer/ImagePreviewerToken.cs:40`；`D/ImagePreviewer/Themes/ImagePreviewNavButtonTheme.axaml:12` | ImagePreviewSwitchSize 未消费，实际尺寸由 PreviewOperationSize+Shared PaddingXS 决定；分清按钮与图标尺寸再接入。 |
| T13 / P2，需合同确认 | `D/ProgressBar/ProgressBarToken.cs:24,29,59,62`；`D/ProgressBar/Themes/ProgressBarTheme.axaml:66` | CircleTextColor/LineBorderRadius 无消费，Round 端帽采用大半径几何。先确认 Token 的适用范围/历史兼容，再接入或明确弃用。 |
| T14 / P2 | `G/DataGridToken.cs:141,147,153,165,171` | FixedHeaderSortActiveBg、HeaderFilterHoverBg、FilterDropdownMenuBg、ExpandIconBg、SelectionColumnWidth 无活跃消费者，部分 Table* 别名也未消费；与 S03 的17个活跃别名问题分开。 |
| T15 / P2 | `P/ColorPickerToken.cs:83,89,154-155`；`P/Themes/ColorView/ColorPickerInput.axaml:69-75` | AlphaInputWidth/InputNumberHandleWidth 未消费，实际为48/16；字体11、整体宽262、spacing5固定。先接已有 Token，再验证HEX/HSV/RGB与字体/密度联动。 |
| T16 / P3，需合同确认 | `C/Avatar/AvatarToken.cs:15-19`；`C/Avatar/Themes/AvatarTheme.axaml:40-44,80,86,92` | TextFontSize* 仅给 EffectiveIconSize，文字头像未绑定字号；文档归入图标尺寸，可能为历史命名。确认文字/图标语义后调整，不能直接扩大已有 Token 含义。 |
| T17 / P2 | `D/FloatButton/FloatButtonToken.cs:30,35,68-69`；`D/FloatButton/Themes/AbstractFloatButtonTheme.axaml:32-42`、`D/FloatButton/Themes/FloatButtonHostTheme.axaml:12-22` | SquareBadgeOffset/CircleBadgeOffset无消费者；实际四组BadgeOffset固定为(-10,4)/(-10,-2)/(-5,-1)/(-10,-5)。接入相应形态语义并保持默认。 |
| T18 / P2 | `D/Tag/TagToken.cs:24,39`；`D/Tag/Themes/TagTheme.axaml:70-74` | TagCloseIconSize无消费，实际关闭按钮取Shared IconSizeXS；可由专用close Style补救，但Token本身不生效。 |
| T19 / P2，需合同确认 | `D/Timeline/TimelineToken.cs:33,80`；`D/Timeline/Themes/TimelineItemTheme.axaml:39-42,130-132` | LastItemContentMinHeight未投射至内容MinHeight，末项只处理padding；明确末项最小高度语义后接线。 |
| T20 / P2，需合同确认 | `D/Separator/SeparatorToken.cs:49,60`；`D/Separator/Themes/SeparatorTheme.axaml:56-74` | HorizontalWithTextGutterMargin未消费；带标题与无标题均用HorizontalMarginBlock三档。当前文档也描述统一间距，需决定保留/弃用/差异语义，而非擅自改变现状。 |
| T21 / P2/P3 | `D/ProgressBar/ProgressBarToken.cs:42-44` | ProgressStepMinWidth、ProgressStepMarginInlineEnd、ProgressActiveMotionDuration没有消费者或默认计算；不能作为当前已生效的控制项，需清理展示合同或接入真实实现。 |
| T22 / P2/P3，文档与消费不一致 | `D/Card/CardToken.cs:100-130,160-162,241-242`；`D/Card/Themes/CardTheme.axaml:24-29,37-42`；`docs/controls/desktop/data-display/card/token.md:56,64,90-91` | ActionsSpacing、TabsMarginBottom、ExtraColor、CardHeadPadding、CardPaddingBase无typed生产消费；文档明确声称tabs间距和Extra文字色作用。先修Extra等真实区域接线，核对未实现/历史语义，不为悬空字段凭空创建新功能。 |
| T23 / P2 | `D/Alert/AlertToken.cs:28,53,73,79`；`D/Alert/Themes/AlertTheme.axaml:144-163` | MessageWithDescriptionMargin/CloseIconSize无消费；描述实际读DescriptionLabelMargin、关闭图标读Shared IconSizeSM。区分消息与描述布局后接入正确节点。 |
| T24 / P2 | `D/Carousel/CarouselToken.cs:54,61`；`D/Carousel/Carousel.cs:640-652` | ArrowOffset无消费；导航箭头间距实际复用PaginationOffset，无法独立调节箭头与分页位置。 |
| T25 / P2/P3，需合同确认 | `D/Expander/ExpanderToken.cs:61-62,80-81`；`D/Expander/Themes/ExpanderTheme.axaml:108-127` | LeftExpandButtonVMargin/RightExpandButtonVMargin无消费；当前主题使用HMargin和EffectiveExpandButtonMargin。核对方向支持和历史合同后再接入。 |
| T26 / P2 | `D/Pagination/PaginationToken.cs:39,57,63,87` | ItemLinkBg/ItemInputBg/InputOutlineOffset/PaginationMiniQuickJumperInputWidth无生产消费，但token.md将其列为可配置视觉值；需要将导航/输入实际节点接入或明确当前边界。 |
| T27 / P2/P3，需合同确认 | `D/Slider/SliderToken.cs:131,171` | MarginPartWithMark只定义和计算，带mark的布局未读取此Token；接入前确认marks测量/裁剪和边界交互，不靠额外固定Margin修外观。 |
| T28 / P2/P3，需合同确认 | `D/TabControl/TabControlToken.cs:70,213`；`D/TabControl/Themes/TabControlTheme.axaml:25` | HorizontalMargin无typed消费，模板读取TabStripMargin；公开Token目录与实际布局输入需同步。 |
| T29 / P3，消费/兼容审计 | `D/Buttons/ButtonToken.cs:158-173,208,253`；`D/Buttons/Themes/DropdownButtonTheme.axaml:97-118`、`D/Buttons/Themes/SplitButtonTheme.axaml:45` | Button的ExtraContentItemSpacing/ExtraContentMargin三档/GroupBorderColor/GutterToFlyout无自身typed消费者；相关派生控件用各自identity下的字段。不得把基类配置自动继承到其他identity，应核对Button字段保留理由与文档。 |
| T30 / P3，目录合同 | `D/ButtonSpinner/ButtonSpinnerToken.cs:19,59`；`D/Message/MessageCardToken.cs:24` | ControlWidth/CardHeight无生产消费；目录仍把它们作为尺寸参数。需确认预留/历史字段并修正展示，不能直接删schema破坏外部主题。 |
| T31 / P2/P3，需合同确认 | `D/Select/SelectToken.cs:89,91,161,176` | SelectAffixPadding/FixedItemMargin无typed生产消费，不能据此承诺附件留白和选项间距已支持精确配置；结合现有Part和密度合同核对。 |
| T32 / P2 | `D/NavMenu/NavMenuToken.cs:36,61,81,91,151,161,211,216,353`；`D/NavMenu/Themes/BaseNavMenuItemHeaderTheme.axaml:17,25,31,176`、`D/NavMenu/Themes/HorizontalNavMenuItemHeaderTheme.axaml:82-98`、`D/NavMenu/Themes/NavMenuItemTheme.axaml:168,192-201,216` | 五个Horizontal专属hover/selected/背景/半径、SubMenuItemBorderRadius、TopLevelItemPopupMarginToAnchor、ItemBackgroundMotionEasing、MenuSubMenuBg无有效当前消费者；实际用通用项值、固定Transparent/0、Shared offset或MenuPopupBg。保留正常方向/暗色语义逐项接线。 |
| T33 / P2，状态合同待确认 | `D/Menu/MenuToken.cs:100,115,120,130,135`；`D/Menu/Themes/MenuItemTheme.axaml:166`、`D/Menu/Themes/TopLevelMenuItemTheme.axaml:72,80-91,175` | ItemIconMarginInlineEnd被ItemMargin替代；TopLevelItemColor/SelectedColor/Bg/SelectedBg均未消费，实际常态Transparent、pointerover/open使用Hover系列。先定义Menu的selected/open差别再改状态映射。 |
| T34 / P2，需合同确认 | `D/OptionButtonGroup/OptionButtonToken.cs:16,21,31`；`D/OptionButtonGroup/Themes/OptionButtonTheme.axaml:134,172` | ButtonBackground/ButtonCheckedBackground/ButtonPadding无消费者；当前背景Transparent或SolidChecked，padding用ContentPadding三档。未找到明确deprecated标记，需决定支持语义和兼容处置。 |
| T35 / P2 | `D/Skeleton/SkeletonToken.cs:27`；`D/Skeleton/SkeletonTitle.cs:3-5`；`D/Skeleton/Themes/SkeletonLineTheme.axaml:13` | TitleHeight无消费，标题复用SkeletonLine样式和ParagraphLineHeight，无法单独调整标题高度。 |
| T36 / P2 | `D/TimePicker/TimePickerToken.cs:27,37,42,47`；`D/TimePicker/Themes/TimeViewTheme.axaml:132`、`D/TimePicker/Themes/TimePickerPresenterTheme.axaml:60`；`D/Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.axaml:134,154` | PeriodHostWidth被ItemWidth替代；ButtonsMargin实际读DatePicker；RangeArrowMargin/IndicatorThickness读InfoPickerInput identity。TimePicker自身这些Own覆盖不到真实视觉；时段宽度还与semantic-part.md的明确说明矛盾。 |
| T37 / P3，功能未公开 | `D/NavMenu/NavMenuToken.cs:66`；`D/Menu/MenuToken.cs:35,70,75` | NavMenu.KeyGestureColor无快捷键文字节点；Menu.DangerItemColor/DangerItemHoverColor/MenuTearOffHeight缺当前危险项/tear-off入口且未明确弃用。标为未接线能力/合同待确认，不为清除字段顺手新增功能。 |

无消费的判定限定本轮生产源码/AXAML检索；生成 descriptor/资源键不算视觉消费者。Ant Design 本身也有显式标记 deprecated/not-used 的兼容字段；AtomUI 的关键是明确合同，不能单凭“有字段”承诺生效。

完整清点见[token-consumption.csv](token-consumption.csv)。137个public无直接引用候选中，以下已经对照文档确认属于保留/范围边界，应排除“当前功能回归”的误报：

- NavMenu.CollapsedWidth：`docs/controls/desktop/navigation/nav-menu/token.md:80`明确兼容保留，新实现使用InlineCollapsedWidth。
- NavMenu五个Danger及五个DarkDanger字段：同文档相关说明表明当前节点没有Danger API，保留主题契约；不为清除候选而新增行为。
- WindowTitleBar的旧HoverBackgroundColor/PressedBackgroundColor/CloseHoverBackgroundColor/ClosePressedBackgroundColor/ForegroundColor：`docs/controls/desktop/window/window-title-bar/token.md:75-85`相关兼容说明；实际主题采用Active/Inactive和WindowsClose系列。
- TreeView.DirectoryNodeSelectedColor/DirectoryNodeSelectedBg：`docs/controls/desktop/data-display/tree-view/token.md`明确默认主题不消费。
- GroupBox.TextPaddingInline/OrientationMarginPercent/VerticalMarginInline：`docs/controls/desktop/data-display/group-box/token.md:37`明确fieldset语义保留。
- Form.VerticalLabelPadding：`docs/controls/desktop/data-entry/form/token.md:49`说明稳定契约与当前使用VerticalLabelMargin的边界。

剩余候选保留为需合同审查，不能自动删除、自动补新功能或宣称全部是Bug。存在直接引用的字段也仍可能有identity、优先级或更新断点，清点表不替代实际渲染验证。

## 4. 固定外观与自绘能力缺口

这些项需区分“既有设计合同”与“新品牌表现能力”，不能未经设计评审改变默认外观。

| ID / 优先级 | 位置 | 可定制边界与建议 |
| --- | --- | --- |
| V01 / P2 | `D/CheckBox/Themes/CheckBoxTheme.axaml:56` | CheckedMarkBrush=White，不跟 ColorTextLightSolid；icon Part 可 qualified Setter 补救，属于 Token 层缺口。 |
| V02 / P2 | `D/Skeleton/Themes/SkeletonImageTheme.axaml:38` | ImageFilled.FillBrush=#bfbfbf，缺独立 icon Part；接 owner 语义色或稳定图标入口。 |
| V03 / P2 | `G/Themes/DataGridColumnHeaderTheme.axaml:156-157` | drag-indicator 背景#808080；提供明确拖拽状态语义，不能直接复用普通hover假定等价。 |
| V04 / P2 | `P/ColorBlock.cs:16,185-187` | 空颜色斜线固定 Red、2px笔、2px偏移，自绘不对应独立节点；现有 Part Setter 不能改笔。提供空值指示 Token/属性，保留无颜色语义。 |
| V05 / P2，形态能力 | `C/Switch/AbstractToggleSwitch.cs:456-460`；`C/Switch/SwitchKnob.cs:208-225` | sealed Render 固定 pill/ellipse且无pen，不消费CornerRadius/BorderBrush/BorderThickness；专用indicator Style的对应Setter无法改变绘制。明确可变形态范围，保留按压拉伸。 |
| V06 / P2 | `C/Switch/SwitchKnob.cs:229,240,286` | loading inset2.5、笔宽1固定；弧角属于动效语义，尺寸/线宽应有稳定输入。 |
| V07 / P2 | `C/ProgressBar/AbstractGeneralStepsProgressBar.cs:17,73`；`C/ProgressBar/StepsProgressPanel.cs:109,114` | 步块间距固定2，track Margin不能同步改变自然排列步进；统一可配置间距。 |
| V08 / P2，需合同确认 | `C/ProgressBar/AbstractCircleProgress.cs:21,399-403` | IndicatorThickness即使1/2仍被最小3夹住；这是细线品牌能力边界，默认120/90/60等尺寸本身合理。 |
| V09 / P2 | `C/FloatButton/FloatButtonSeparatorLayer.cs:57` | 分隔线传入固定1.0，只有brush入口；可配置语义线宽，保留像素对齐。 |
| V10 / P2，API能力 | `C/Watermark/Glyphs/TextGlyph.cs:57`；`C/Watermark/Glyphs/TextGlyphExtension.cs:20-25` | 使用Typeface.Default，仅有FontSize/Foreground，缺字体/字重/字形合同；补完整排版输入，继续使用已有scoped host。 |
| V11 / P2 | `D/Buttons/Button.cs:1104` | 预设色按钮阴影在C#固定Blur3/Spread0，未被Default/Primary/DangerShadow覆盖；提供统一阴影语义或有效Part，避免扩展无穷状态Token。 |
| V12 / P2 | `D/NumericUpDown/Themes/NumericUpDownSpinnerTheme.axaml:44`；`D/OtpLineEdit/Themes/OtpLineEditCellTheme.axaml:91` | spinner方向分隔边和OTP下划线固定1；接owner有效线宽，保留下划线的0半径语义。 |
| V13 / P2 | `D/ListBox/Themes/ListBoxItemTheme.axaml:51`；`D/ListView/Themes/ListViewItemTheme.axaml:41` | item底边1且色读Shared；改变LineWidth或局部ColorSplit不完整响应。 |
| V14 / P2 | `D/ComboBox/Themes/ComboBoxItemTheme.axaml:41`；`D/Pagination/Themes/PaginationNavItemTheme.axaml:93` | selected字重固定SemiBold；接有效FontWeightStrong或明确的Own值。 |
| V15 / P2 | `D/TabControl/Themes/TabItemTheme.axaml:30`；`D/TabControl/Themes/TabStrip/TabStripItemTheme.axaml:33` | 竖向Left/Right三档均固定Padding8,4，未形成对应密度配方；同步两套Tab实现。 |
| V16 / P2 | `D/Menu/Themes/MenuItemGroupTheme.axaml:16` | 组标题Padding16,8固定，前景Shared；已有title Part可补救，组件配方仍缺组标题语义。 |
| V17 / P2 | `D/TextBlock/HighlightableTextBlock.cs:129`；`D/TreeView/TreeViewItemHeader.cs:678` | 匹配Run本地Bold，开关只能选择加粗与否；root字重不能改变它。明确匹配字重输入并从owner读取。 |
| V18 / P2 | `D/Calendar/Themes/CalendarDateCellTheme.axaml:105` | 全屏Margin4,0、Padding8,4,8,0、顶边/焦点线2固定；接密度/描边，是否保留全屏方形另行定义。 |
| V19 / P2 | `D/DatePicker/Themes/DatePickerPresenterResources.axaml:7` | 头高33、底边1、标题Bold固定；核对大字号测量及状态，尊重内部public Button的身份。 |
| V20 / P2 | `D/TimePicker/Themes/TimeViewTheme.axaml:108`；`D/PopupConfirm/Themes/PopupConfirmContainerTheme.axaml:58` | 标题SemiBold及部分背景/分隔色Shared；接实际owner排版或既有title Style。 |
| V21 / P2，形态能力 | `D/ImagePreviewer/Themes/ImagePreviewerOverlayHostTheme.axaml:32`；`D/ImagePreviewer/Themes/ImagePreviewFloatToolbarTheme.axaml:55` | 关闭/工具栏固定半径100，导航动态强制圆形，缺细粒度按钮Part；方形品牌工具栏需明确shape/radius入口。 |
| V22 / P2 | `G/Themes/DataGridColumnHeaderTheme.axaml:37-38`；`G/Themes/DataGridTheme.axaml:121-122`；`G/Themes/Filters/*FilterFlyoutPresenterTheme.axaml:41-45` | 分隔/底线1固定；底线占高绑定owner边厚但画线仍1。统一语义线宽、测量和绘制。 |
| V23 / P2 | `G/Column/DataGridColumnDraggingOverIndicator.cs:47,111`；`G/Themes/DataGridRowExpanderTheme.axaml:17,23,70,74` | 拖拽线宽1/固定Dash；展开加减号1px、长9；IndicatorThickness只参与布局偏移。明确拖拽/展开图形输入。 |
| V24 / P2 | `P/Themes/ColorView/AbstractColorPickerViewTheme.axaml:46`；`P/Themes/ColorView/ColorSpectrumTheme.axaml:41`；`P/Themes/ColorSlider/ColorSliderThumbTheme.axaml:14` | 分隔线1、选择环2、thumb padding2，不能通过现有品牌线宽统一；是否独立语义优先于机械套LineWidth。 |
| V25 / P2 | `G/Themes/DataGridTheme.axaml:275`、RowHeader:50、ColumnGroupHeader:41、Cell:52、ColumnHeader:92、Row:108；`P/Themes/AbstractColorPickerTheme.axaml:55`、`P/Themes/ColorBlockTheme.axaml:54,71`；`D/Primitives/AddOnDecoratedBox/Themes/InputControlFrameTheme.axaml:50`等 | 可选包已核9处把Custom合并进Middle；扩展清点共58处/45文件（主包另49处），完整位置见custom-size-candidates.csv。需逐属性分离基础默认与三档selector，不擅自删除状态/方向样式；不是“实例LocalValue无法覆盖”。 |

## 5. 工具、预设和表现层的差距

| ID / 优先级 | 证据 | 差距 |
| --- | --- | --- |
| G01 / P3 | `V/Assets/Themes/{PolarGreen,SunsetOrange,GoldenPurple,Magenta}.theme.xml:10-11` | 四套主题都只有ColorPrimary/ColorLink；缺字体、圆角、阴影、边厚、尺寸、状态及Part样式的完整品牌配方。 |
| G02 / P3 | `V/Workspace/Views/WorkspaceWindow.axaml.cs:164-222`；`V/Workspace/Views/WorkspaceWindow.axaml:48-79`；`B/Shell/GalleryWorkspaceViewModel.cs:250-290` | 已有目录、accent、Light/Dark/System、Compact、motion/wave切换；应复用，不能把“缺工作台”误述成“没有主题切换”。 |
| G03 / P3 | `V/ShowCases/General/CustomizeTheme/ViewModels/CustomizeThemeViewModel.cs:44-99`；对应Views:38-207 | 有全局/暗色/动态/组件算法/嵌套示例，但运行时只演示三色、局部主要Button/LineEdit主色；缺整套品牌的状态和部件比较。 |
| G04 / P3 | Gallery范围主题关联的Import/Export/Serialize/StorageProvider/文件picker检索及页面阅读 | 未发现通用Token编辑表单、主题保存/导入导出、配置diff、并排状态矩阵、AI生成流程。Core已有用户目录/XML不等同于Gallery UI。 |
| G05 / P3 | `B/Controls/Themes/SemanticPartPreviewTheme.axaml:19,31,36-37,53,77,105-115`；`B/Controls/SemanticParts/SemanticPartPreviewItemControl.axaml:27-42` | 检查面板自身字号/字重/边框/留白固定，品牌密度预览与工具壳不统一；使用Gallery专属Token，合理保留断点和工具布局。 |
| G06 / P3 | `B/Controls/Themes/GalleryShowCaseHeaderTheme.axaml:66,83,107,131`；`B/Shell/GalleryShellView.cs:391-392,443-444` | 元数据标签Bold、后备品牌文本20/SemiBold及footer边宽/留白固定；页面标题自身已消费TitleFontSize/TitleFontWeight，不属于本项。 |
| G07 / P3 | `B/Controls/GalleryCodeViewer.axaml:9-17,31-32`；`B/Controls/GalleryCodeViewer.cs:439-480,619-630` | 代码查看器使用独立字体/13px、颜色与LightPlus/DarkPlus；不是统一品牌Token预览。不能据白色资源直接断言暗色失效，需运行检查底色。 |
| G08 / P3，设计项 | `C/Result/{NoFoundImage,UnauthorizedImage,ServerErrorImage}.cs`；`C/Result/AbstractResult.cs:258-268` | 插画固定SVG色板，品牌色局部重染缺入口；只提取品牌/背景或提供替换，不把人物肤色等全部Token化。 |
| G09 / P3，需合同确认 | `C/Badge/BadgeColorUtils.cs:8-36`；对比`C/Tag/AbstractTag.cs:273-289` | Badge命名色静态表与Tag的snapshot palette路径不同。先确认是否支持修改同名palette；不要求“Blue自动等于ColorPrimary”。 |
| G10 / P3，覆盖项 | `C/Splash/Themes/SplashDefaultTheme.axaml`；`C/TextSelectionHandle/Themes/TextSelectionHandleTheme.axaml` | Splash默认尺寸可由BasedOn覆盖，但未联动全部密度Token；触控手柄用Avalonia选择色资源，未确认AtomUI映射。无Mobile运行证据，作为专项覆盖。 |
| G11 / P3，悬空目录字段 | `B/Controls/GalleryShowCaseHeaderToken.cs:23,52`；`B/Controls/Themes/GalleryShowCaseHeaderTheme.axaml:72,77,101,125` | MetadataItemSpacing无生产消费者，实际使用MetadataLineSpacing/MetadataPairSpacing；确认历史含义，不随意把它映射成另一种gap。 |

## 6. 不计为主题缺陷的项目

- Token 默认计算中的颜色、半径、尺寸、阴影，只要覆盖后被正确消费，就是合法默认。
- 透明 hit surface、0/1 opacity、几何中心/角度、24栅格、算法系数、缓存容量、输入数值范围等不能一概改成主题参数。
- 色谱的白/黑/透明梯度、HSV/RGB范围、当前颜色brush属于数据语义；ColorPicker的棋盘已消费ColorFillSecondary/TransparentBgSize。
- 显式Color/PresetColor API的本地背景、输入root用户Brush中继是既有优先级合同；不能为了让hover恢复而撤销用户定制。
- SwitchKnob阴影已有Style优先级写入；RadioIndicator已优先消费CornerRadius；不能与真正LocalValue或sealed Render问题混淆。
- 圆形头像/轨道/端帽/slider handle默认圆形，以及underlined半径0，是形态语义；是否开放其他形状需明确设计。
- Gallery业务演示配色、专用Semantic Style的Setter、Inspector高亮金色、IsFake透明/零阴影，不作为控件品牌硬编码。
- Extras目前仅有包注册，没有Control/Theme；没有可声称“已完成视觉验收”的控件。

## 7. 验证处置

已搜索现有Theme/Token/SemanticPart测试，确认测试入口存在；例如DataGridTokenAlignmentTests只验证默认计算值，不能证明用户覆盖已经到达最终消费者。没有运行或修改这些测试。

后续修复按总览验收矩阵使用现有用例或临时失败测试，锁定原始结构/尺寸/交互。最终交付时删除临时专用产物、完成受影响模块focused验证；不把全量通过或编译成功当成品牌效果完整的证据。

本轮未修改产品代码、未新增永久测试、未创建分支/worktree、未提交。原始候选并非逐项人工判定完成，运行期与跨平台证据仍需后续补充。
