# Calendar 桌面版架构设计

本文定义 Calendar/LunarCalendar 组合 DateViewer 的架构与产品契约。
关联 [实现](implementation.md)、[行为](behavior-design.md)、[农历](lunar-calendar-design.md)、
[范围条](range-bar-design.md)、[Token](token.md)、[Semantic Part](semantic-part.md)和 [Changelog](changelog.md)。
通用日期模型以 [共享日期面板设计](../../data-entry/date-viewer/shared-panel-design.md)为唯一来源。

## 1. 控件定位

Calendar 是按日期组织业务内容的日历，归属 AtomUI.Desktop.Controls。它组合公共 DateViewer，
拥有业务 Header、单值、模式、Fullscreen/Mini、模板和 RangeBars，不再拥有独立日期引擎。
LunarCalendar 以同一公历模型提供农历、节气、传统节日和应用节假日投影。

Calendar 不提供日期输入、时间编辑、范围选择或复杂日程排布。独立轻量选择使用 DateViewer，内嵌范围选择使用 RangeDateViewer。

## 2. 设计语言

| 维度 | 契约 |
| --- | --- |
| 产品语义 | 日历业务展示与单值日期/月区域选择。 |
| 内容 | 默认日期值、CellTemplate、FullCellTemplate 和独立业务条 overlay。 |
| 状态 | Calendar 拥有 Value/Mode，正文只接受投影并报告意图。 |
| 视觉 | Mini/Fullscreen 与农历次级内容通过完整主题配置表达，不改变日期拓扑。 |

## 3. API 与契约模型

| API | 默认值/语义 |
| --- | --- |
| Value | 创建时的今天，TwoWay，日期规范化；Calendar 自己决定提交。 |
| Mode | Month 默认；Month 显示日期，Year 显示月份。 |
| Fullscreen | true；完整内容布局或 Mini。 |
| ShowWeek | false；日期面板周号列。 |
| ValidRange | null；首尾包含，范围对象拒绝反向端点。 |
| DisabledDate | null；增加业务禁用，异常传播。 |
| HeaderTemplate | null；业务 Header 的强类型上下文与提交命令。 |
| CellTemplate | 默认日期值下方的业务内容。 |
| FullCellTemplate | 完整内部内容，优先于 CellTemplate。 |
| RangeBars | 空集合；连续业务日期标记，不代表选择范围。 |

事件为 PanelChanged、ValueChanged、Selected。有效用户提交先更新 Value，再按该顺序省略不适用事件。
程序赋值不模拟用户事件，同值重选仅 Selected。Value/Mode 与 DateViewer 的独立 API 不相互 TwoWay 竞争。

LunarCalendar 增加支持范围、呈现开关、HolidayProvider、SelectedLunarDateInfo 和 RefreshHolidayData，详见 [农历设计](lunar-calendar-design.md)。

## 4. 行为与状态模型

Calendar 将 Value/Mode 投影为浏览区域、面板与选择状态，接收日期、月份、Header 或自定义命令意图后提交。
Month 使用共享日期网格，Year 使用共享月份网格。月份激活保留 Value 日号并截断到月末，不套用 Month Picker 的月首输出。
Header 年月操作仍是 Calendar 选择，DateViewer 自身导航 Header 在此关闭。

周号使用行首日期并以 Date 来源提交，不进入日期/月 roving focus。
日期禁用由 ValidRange 与 DisabledDate 合并；月份可用性保持月首/月末合并判定，不能只检查保留日号。
自定义 Header 命令的约束责任明确，不由正文策略悄悄改变。

## 5. 视觉与主题模型

正文使用统一 DateViewerCell。Mini 是紧凑值布局，Fullscreen 是日期值/业务内容布局；共享内核不决定产品尺寸。
Calendar 主题定义根、Header、日期正文、Mini/Fullscreen 和 overlay；Cell 主题自己处理内部结构。
CellTemplate 保留值，FullCellTemplate 只替换内部内容，均不替换容器的禁用、焦点、命中和 Automation。

农历主题添加次级内容与 markers，不通过根主题穿透内部日期 Cell。
业务条 overlay 不参与命中、不改变行列与 Cell 外间距。Semantic 定制见 [正式边界](semantic-part.md)。

## 6. 家族与集成

Calendar → 业务 Header + DateViewer + RangeBarPanel。
LunarCalendar → 同一结构 + 农历投影与专用呈现。
DatePicker 与 Calendar 共用日期内核和基础 Cell，保留各自的 Header、选择、约束和输入消费策略。
DateViewer 默认资源不依赖 Calendar/农历，Calendar 按静态引用保留面板资源。

## 7. 重建与定制边界

日期正文统一使用 DateViewer，共享 DatePanel/DateViewerCell 的拓扑和交互，不保留独立日期引擎。
产品 Value、事件、模板优先级、月份日号、周号、农历支持范围和业务条隔离保持明确语义。
不把范围选择、时间编辑或 Picker 旧 API 暴露到 Calendar。
用户依赖公共上下文和专用 Semantic Style，不依赖 DatePanel/Cell 内部 CLR 类型。

## 8. 文档导航与验证

[行为设计](behavior-design.md)拥有 Calendar 提交和导航语义，[农历设计](lunar-calendar-design.md)拥有历法与 Provider，
[范围条设计](range-bar-design.md)拥有业务 overlay。公共拓扑、Cell 和输入管线引用共享设计，不复制算法。
验证 Month/Year × Mini/Fullscreen、Header、模板、事件、禁用、周列、农历与业务条，以及资源释放、Style 命中与 AOT。
