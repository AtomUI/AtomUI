# LunarCalendar 农历能力设计

本文定义共享日期面板上的农历呈现架构。关联 [Calendar 架构](overview.md)、
[实现](implementation.md)、[行为](behavior-design.md)、[Token](token.md)、[范围条](range-bar-design.md)与
[共享内核](../../data-entry/date-viewer/shared-panel-design.md)。

## 1. 定位与原则

LunarCalendar 是 Calendar 的公历日期状态上的农历呈现，归属 Calendar 模块，不拥有农历选择值或第二套日期引擎。
公历为业务值，农历、节气、节日和调休是投影。专用内容/容器只扩展呈现，复用 DateViewerCell 激活、焦点和 Automation。

## 2. 公共模型

| API | 默认与语义 |
| --- | --- |
| SupportedRange | 1900-01-01 至 2100-12-31，两端包含。 |
| ShowSolarTerms | true，节气可进入次级内容。 |
| ShowTraditionalFestivals | true，内置传统节日可进入次级内容。 |
| ShowHolidays | true，允许应用 Provider 数据。 |
| HighlightWeekends | true，调休工作日覆盖普通周末。 |
| HolidayProvider | null，同步提供面板节假日/调休，不内置政策日历。 |
| SelectedLunarDateInfo | 只读，从宿主公历 Value 投影。 |
| RefreshHolidayData | 增加 Provider 数据 revision，失效当前面板投影。 |

Value 规范化并收敛支持范围；ValidRange 保持原值，与支持范围求交集。
交集为空时全部业务 Cell 禁用，Header 无有效年月选项，Month/Year 模式仍可切换。
程序赋值不模拟用户选择；DisabledDate 不参与程序值收敛。

## 3. 模式矩阵

| 模式 | 呈现 |
| --- | --- |
| Fullscreen Month | 公历日期、农历次级行、业务模板，RangeBars 在次级行后。 |
| Mini Month | 公历/农历双行，按次级行与 Cell 尺寸分配六周空间。 |
| Fullscreen Year | 公历月份与该月农历月范围，业务内容。 |
| Mini Year | 紧凑公历月份与农历月范围。 |

ShowWeek 只增加周号，不改变日期列内容尺寸、算法或选择。
公开 Header 使用 Calendar 的业务选择规则，农历年月文案由呈现投影提供。

## 4. 架构与 ownership

纯历法计算 → 显示区域面板数据 → 次级内容/上下文 → 统一 Cell 或专用呈现扩展。
Calendar 拥有值/模式与事件；农历投影拥有缓存与 Provider revision，DateViewer 拥有日期拓扑和交互。
内容模型不持有 Visual/Cell，农历层不创建自己的 Grid 或复制键盘算法。

## 5. 算法与内容投影

公历转换提供农历年/月/日、闰月、干支生肖；月份范围来自实际公历月交集，不通过选中日期猜测整月。
节气使用纯日期数据，传统节日按农历/公历规则解析；法定政策与调休始终由 Provider 提供。
次级内容优先级：应用 Holiday/Workday → 传统节日 → 节气 → 普通农历日；Year 模式显示农历月范围。
关闭对应开关移除该候选，调休工作日不按普通周末高亮。

## 6. 缓存、失效与生命周期

缓存键包含 PanelKind、显示年月、真实可见日期首尾、Culture、呈现开关和 Provider identity/revision；
有效周首改变可见区时必须失效。Month 面板的农历月交集只依赖年份与 Culture，不查询节假日 Provider。
同区域只改选择或焦点不重复查询 Provider。更换 Provider/刷新、换区域/模式/语言或开关使对应数据失效。
只保留有界当前面板数据；呈现解除、detach 或 owner 释放解除订阅并清除与旧 context/页面的联系。
重新 attach 按当前 owner 状态恢复，不能复用已脱离面板的 Cell 引用。

## 7. Theme、Token 与 Semantic

农历增量资源只拥有次级字号/行高/颜色、日期/月 Cell 内容尺寸、周末/节假日标记和 overlay 避让。
根、Header 和普通选中视觉由 Calendar/DateViewer 表达，不复制完整普通面板 Token。
农历 Content 日期选中时使用主色值框，公历与农历副文字统一使用浅色；默认副文字颜色由内容主题的 Style 提供，使选中状态能够覆盖。
根主题不穿透子 Cell；专用主题维护次级内容。Semantic 使用 Calendar 家族与统一 Cell 的真实 route。

## 8. AOT 与验证

纯历法与 Provider 调用均强类型，资源静态注册，独立 DateViewer 不反向保留农历表。
验证全支持范围与闰月、跨年/月交集、节气节日优先级、Provider revision、空交集、四种模式、
双行布局、RangeBars 避让、上下文释放、Automation 文案和实际 AOT 消费。
扩展支持范围必须同步扩展历法与节气数据，并完成全范围验证，不仅修改常量。
