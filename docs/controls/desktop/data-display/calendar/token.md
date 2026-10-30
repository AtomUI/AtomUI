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
业务布局、模板内容、动态资源覆盖和尺寸语义必须完整；普通主题修改不能让农历次级行与业务条重叠。
状态选择在 Theme Selector，资源更新由 scoped owner 链传播，不能以静态值规避生命周期。

## 6. 验证策略

覆盖 Month/Year × Mini/Fullscreen、普通/农历、ShowWeek、Cell/FullCell 模板与业务条。
比较行列几何、文本与内容高度；验证局部资源覆盖/移除、主题更新、Style 生效及资源释放。
按 Calendar/LunarCalendar 与公共面板实际消费验证 TypeMap 和主题资产保留。
