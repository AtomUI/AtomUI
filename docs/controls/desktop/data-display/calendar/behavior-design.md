# Calendar 行为设计

本文定义 Calendar 的产品行为。关联 [架构](overview.md)、[实现](implementation.md)、
[农历](lunar-calendar-design.md)、[范围条](range-bar-design.md)及 [共享面板](../../data-entry/date-viewer/shared-panel-design.md)。

## 1. 设计定位

Calendar 是业务日历展示与单值选择宿主，日期交互复用 DateViewer。
Month/Year 是产品显示模式，不等同于日期内核 SelectionUnit。

## 2. 状态与提交 owner

Calendar 拥有 Value、Mode 和公开事件，正文只读取有效投影。
Value 按日期解释；LunarCalendar 另按支持范围收敛。程序赋值不触发用户事件。
改变语言、主题、RangeBars 或焦点不提交选择。

## 3. 操作来源

| 来源 | Calendar 行为 |
| --- | --- |
| Header Year/Month | 保留日号并截断到目标月末，按 Year/Month 来源提交。 |
| 日期 Cell | 按 Date 来源提交。 |
| 月份 Cell | 保留日号并截断，按 Month 来源提交。 |
| 周号 | 行首日期，Date 来源，不进入日期/月键盘序列。 |
| 自定义 Header | Customize 来源；命令约束由模板负责。 |
| Mode 切换 | 更新 Mode 和一次 PanelChanged，不触发选择事件。 |

正文关闭 DateViewer 默认导航 Header。不能把默认面板的浏览规则施加给业务 Header。

## 4. 事件顺序

一次有效提交先更新 Value，再按需发布 PanelChanged → ValueChanged → Selected。
Month 跨自然月触发面板变化；Year 跨自然年触发。实际日期不变不触发 ValueChanged，有效同值重选仍 Selected。
程序设置 Value/Mode 只渲染。宿主不重复转发 DateViewer 的内部或公开通知。

## 5. 约束与周规则

日期可用性由 ValidRange 和 DisabledDate 合并，范围首尾包含，谓词异常传播。
月份分别检查月首/月末，仅两端均禁用才禁用整月；不能用保留的日号禁用整个月。
周号取行首的合并禁用状态；禁用不提交，启用可 Pointer/Automation 激活。

Calendar 周首、周号与文本跟随有效语言规则，与 Picker ISO 周策略分别配置。
自定义 Header 的命令不自动叠加正文规则；ValidRange 不隐式重写普通 Calendar 外部 Value。

## 6. 焦点与输入

方向键只移动当前已生成网格的 roving focus，跳过禁用和周号，无合法目标时停留。
日期步长左右一天、上下一周；月份步长左右一月、上下三月。Enter/Space/Pointer/Automation 激活进入统一提交。
进入面板优先可用选中项，否则首个可用项。Cell 内内容滚动与外层滚动链不因共享 Picker 策略改变。

## 7. 内容与布局

CellTemplate 保留日期值，FullCellTemplate 优先替换内部内容，两者不替换交互容器。
日期/月内容使用强类型上下文，周标题/周号不消费业务日期模板。
Fullscreen/Mini 只改变呈现与布局，RangeBars 仅 Fullscreen Month 显示且不命中。

## 8. 语言、生命周期与验证

语言变化失效有效周规则、文本、Header 和模型；农历政策数据仍来自应用 Provider。
模板替换/卸载先解除旧订阅，面板回收清除业务 context。
验证操作来源、事件序列、四种模式组合、禁用、周号、Header/Cell 模板、键盘与实际嵌套滚动。
共享拓扑、极值和 Pointer 激活规则不复制，使用 [统一验收](../../data-entry/date-viewer/shared-panel-design.md#103-验收矩阵)。
