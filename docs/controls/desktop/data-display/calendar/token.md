# Calendar Token 设计

本文定义 Calendar/农历的视觉资源。关联 [架构](overview.md)、[实现](implementation.md)、
[行为](behavior-design.md)、[农历](lunar-calendar-design.md)、[范围条](range-bar-design.md)、[Changelog](changelog.md)和
[公共面板资源](../../data-entry/date-viewer/token.md)。全局规则见
[控件 Token 规范](../../../../engineering/development/control-token-guidelines.md)。

## 1. 定位

Calendar Token 表达业务日历根、Header、Mini/Fullscreen 内容布局与 overlay。
DateViewer 提供共享面板和独立默认资源；Calendar 通过完整产品主题提供自己的呈现，不反向要求面板依赖 Calendar。

## 2. 语义分类

| 分类 | ownership |
| --- | --- |
| 根与正文 | Calendar 背景、边界与正文视觉。 |
| 周标题 | Fullscreen 周标题高度、右侧与底部内边距。 |
| Header | 年月选择和模式切换的尺寸与间距。 |
| 日期内容 | Mini 内容高度、月份内容宽度、Fullscreen 最小内容区域与选中反馈。 |
| 业务条 | 条高与 overlay metrics；单条颜色/高度属于实例。 |
| 农历次级内容 | 字号、行高、颜色、双行 Cell 尺寸与月份范围宽度。 |
| 农历状态/避让 | 周末、节假日、工作日 markers 与业务条次级行避让。 |

CalendarToken 持有根/正文背景、Header 宽度、月份内容宽度、Mini 高度、Fullscreen 周标题高度与内边距、完整内容最小高度与范围条高度；
LunarCalendarToken 持有双行内容尺寸、文字/标记颜色与范围条避让。

新增的 Own Token（AtomUI 扩展，替换主题与代码中的字面量，默认值与原渲染一致）：

| Token | 默认 | 语义 |
| --- | --- | --- |
| `Calendar.FullCellMargin` | 左右 `UniformlyMarginXS / 2`（4） | Fullscreen 单元左右外边距（antd `margin: 0 marginXS/2`）。 |
| `Calendar.FullCellPadding` | `UniformlyPaddingXS, UniformlyPaddingXS / 2, UniformlyPaddingXS, 0`（8,4,8,0） | Fullscreen 单元内边距（antd `padding: paddingXS/2 paddingXS 0`）。 |
| `Calendar.FullCellBorderThickness` | 顶边 `LineWidthBold`（2） | Fullscreen 单元顶部分隔线与键盘焦点顶边（antd `borderTop: lineWidthBold`）。 |
| `Calendar.RangeBarGap` | `UniformlyMarginXXS / 2`（2） | 同一单元内上下堆叠的范围条间距。 |
| `Calendar.RangeBarLabelPadding` | 左右 `UniformlyPaddingXS`（8） | 范围条标签的水平内边距。 |
| `LunarCalendar.MarkerSize` | `UniformlyMarginXXS`（4） | 节假日/调休标记圆点直径。 |
| `LunarCalendar.MarkerMargin` | 右侧 `UniformlyMarginXXS`（4） | 标记圆点与次要文字的间距。 |
| `LunarCalendar.MarkerCornerRadius` | `MarkerSize / 2` | 标记圆点圆角。 |

范围条标签的字号与颜色读宿主的 `FontSizeSM` / `ColorTextLightSolid`；Mini 键盘焦点描边读 `FocusVisualBorderThickness`。
日期、模式、约束、节日优先级、Provider revision 和确认状态不属于 Token。

## 3. 专项模型消费

Calendar 主题向 DateViewer 投射密度与内容参数，DateViewer/Cell 主题维护自己的内部布局。
Mini 和 Fullscreen 使用完整配置，不把另一分支的尺寸混入当前分支。
农历呈现消费增量参数，不复制普通根、Header 和选中状态的资源定义。
RangeBars 消费共享几何，顶部偏移与农历次级行/内容留白一致，不通过 Cell 外 margin 避让。

## 4. 控件家族影响

普通 Calendar 和 LunarCalendar 共用日期拓扑和输入，呈现各自有完整尺寸基线。
DatePicker 不能采用 Calendar Mini 的内容高度或农历行高作为默认面板参数。
独立 DateViewer 消费不保留 Calendar 业务条或农历资源；宿主静态依赖各自产品资源。

## 5. 重建与覆盖边界

CalendarDateCellTheme 与 LunarCalendar 的 CellTheme 资源消费对应产品尺寸；正文背景消费 Calendar FullPanelBg，
农历次级内容消费独立 Token。

Token owner：Calendar 把自己的 Token owner 转发给模板中的 DateViewer（`TokenOwnerForwarding`），因此 Calendar 提供的面板与单元主题以
`OwnerTokenResource` 读取宿主 Calendar / LunarCalendar 的有效全局 Token，`Controls[Calendar]` / `Controls[LunarCalendar]` 到达日期格；
DateViewer 自己的 Own Token 仍读 DateViewer 身份。Header 中的 ComboBox、OptionButtonGroup 保持各自身份。
LunarCalendar 复用模板和 AbstractCalendarToken 的真实公共 Own 计算，但由自己的终端设置样式输入。MiniContentHeight/FullCellMinHeight 等月历度量保持各自公式；ItemActiveBg 由两个终端独立拥有：农历 Fullscreen Year 的选中月份使用该 Own（默认 active item 浅蓝色），农历 Month 的选中日期仍使用主色。YearMonthCellWidth 只被 Gregorian 消费，不添加给 Lunar；农历 Compact Year 使用自己的 MiniMonthCellWidth。Controls[Calendar] 的 Own 不再进入 Lunar。ItemActiveColor（默认 ColorPrimary）是 Gregorian Fullscreen 选中单元的文字色，画在 ItemActiveBg 上；选中底色为实色的主题设为对比色。农历单元的选中文字由 LunarCalendarCellContent 使用 ColorTextLightSolid，不消费它。
业务布局、模板内容、动态资源覆盖和尺寸语义必须完整；普通主题修改不能让农历次级行与业务条重叠。
状态选择在 Theme Selector，资源更新由 scoped owner 链传播，不能以静态值规避生命周期。

## 6. 验证策略

覆盖 Month/Year × Mini/Fullscreen、普通/农历、ShowWeek、Cell/FullCell 模板与业务条。
比较行列几何、文本与内容高度；验证局部资源覆盖/移除、主题更新、Style 生效及资源释放。
按 Calendar/LunarCalendar 与公共面板实际消费验证 TypeMap 和主题资产保留。
