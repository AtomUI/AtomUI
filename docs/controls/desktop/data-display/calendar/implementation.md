# Calendar 桌面版实现原理

本文定义 Calendar 组合 DateViewer 的实现架构。关联 [架构](overview.md)、[行为](behavior-design.md)、
[农历](lunar-calendar-design.md)、[范围条](range-bar-design.md)、[Token](token.md)、[Semantic Part](semantic-part.md)、
[Changelog](changelog.md)与 [共享日期内核](../../data-entry/date-viewer/shared-panel-design.md)。

## 1. 实现定位

Calendar 只维护产品状态与业务组合。日期模型、焦点、容器池和激活来自 DateViewer。
LunarCalendar 投影公历数据，RangeBars 消费同一面板拓扑/几何，不形成第二个日期 owner。

## 2. 源码职责结构

| 子域 | 责任 |
| --- | --- |
| Calendar root | Value/Mode、Header 命令、事件、模式和模板参数。 |
| Header | 年、月和显示模式操作或 HeaderTemplate。 |
| 农历 | 纯历法、面板数据、Provider、次级内容与布局参数。 |
| RangeBars | 业务对象资源宿主、lane 计算和 overlay。 |
| Theme/Token | Calendar、农历和 overlay 的产品呈现。 |
| DateViewer 模块 | 日期拓扑、CellModel、焦点、输入、容器池与 Automation。 |

Calendar.DatePanelHost.cs 将 Value/Mode 投影为 DatePanelInput，并消费 DateCellSelection。
CalendarCellContext 派生自 DateViewerCellContext，构造参数与 CellType 使用 DateViewerCellType。
共享纯日期算法不引用 Calendar root 或 Token。

## 3. 核心角色

Calendar root 是 Value/Mode 与公开通知的唯一 owner，日期面板只提供有效投影与意图。
业务 Header 报告年/月/模式，不直接写入 DateViewer 的独立选择属性。
农历呈现层消费公历模型，范围条层消费拓扑和实际布局，均不拥有选择。

## 4. 状态与数据流

Calendar 属性/业务 Header → Calendar 转换 → 有效约束、选择与面板投影 → DateViewer → Cell。
Cell 激活 → Calendar 提交 → 属性/公开事件 → 新投影。程序赋值仅更新状态与呈现。
农历缓存以显示区域、PanelKind、Culture、开关与 Provider revision 失效，选择值不是唯一面板键。
RangeBars 变化只失效 overlay，不重建日期 CellModel。

## 5. 组合结构模型

Calendar root 组合业务 Header 与正文；正文叠放 DateViewer 和不命中的 RangeBarPanel。
DateViewer 导航 Header 关闭，DatePanel 组合 WeekHeader、CellHost 与统一 DateViewerCell。
CalendarTheme 的 PART_HeaderPresenter 是稳定 ContentControl 区域，承载 PART_DefaultHeader 与 PART_CustomHeader；
PART_BodyPresenter 叠放 PART_DateViewer 和 PART_RangeBarPanel。CalendarDateCellTheme 定义产品 Cell 布局，
LunarCalendarCellContentTheme 只定义农历文本与标记。
Calendar 在 OnApplyTemplate 已取得 DateViewer 部件，并通过内部只读 EffectiveDateViewer 属性通知模板更换；
RangeBarPanel 的 LayoutSource 在 ControlTheme 中以 TemplateBinding 连接该属性，沿用面板现有的订阅与解除订阅生命周期。
Mini 日期值框保持固定尺寸，Today 与键盘可见焦点边框由不参与文字布局的覆盖层绘制；鼠标按下不显示焦点描边，选中状态只改变背景和前景，日期文字保持原位。
Fullscreen 的顶部单元格边界仍由值框所在的视觉表面绘制。
日期文字通过继承 DateViewerCell 的 Foreground 获取选中、禁用、跨月和周序号颜色；主题只在状态 owner 上声明这些颜色，避免为每种状态重复匹配模板内的文字节点。

Header 为 internal-observable；业务条对象是 public 数据；DatePanel/Cell 内部节点只通过 Semantic 契约定制。
FullCellTemplate 不替换正文 overlay 或交互容器。

## 6. 生命周期与模板接入

re-template 前解除旧 Header/面板操作订阅，再将当前 Value/Mode 与呈现配置回放到新模板。
detach 解除语言监听、正文宿主/上下文/Automation 名称工厂和条目资源 attachment；农历 adapter 清空当前面板缓存。
attach 按当前状态恢复。
共享容器回收由 DateViewer 清除 owner/model/context/模板。Calendar 不重扫 VisualTree 修补 Cell。
集合替换、Reset、重复条目与移除的资源 attachment 计数由 Calendar 管理。

## 7. 交互与通知

Calendar Header 提交年/月日期，mode 只改变显示模式。Cell 报告目标日期或月份区间，由 Calendar 保留/截断日号。
三个事件的条件与顺序见 [行为设计](behavior-design.md)，不得因转发 DateViewer 通知造成重复。
Calendar 键盘只移动当前网格焦点；滚轮策略不能继承 Picker 的弹层消费规则。

## 8. 算法与关键流程

网格起点、日期区间、行列和导航使用共享内核。
Calendar 提交策略维护月首/月末禁用、日号截断、周号行首和自定义 Header 约束。
overlay 使用面板的拓扑与实际 arrange 几何，不按 Value 再计算一份网格。
农历只做内容与有效支持范围投影，详见 [农历设计](lunar-calendar-design.md)。

## 9. 资源、性能与 AOT

静态主题引用 DateViewer 的资产；农历资源属于农历消费，不强制独立面板保留 Provider 或历法表。
会话/快照无 DynamicResource，业务条非 Visual 对象使用 scoped resource host。
日期与 overlay 失效分离；同区域选中/焦点变化不查询农历 Provider。性能仍需实际测量，不以共享证明速度提升。

## 10. 维护不变量

一套日期引擎、一份 Calendar 选择、一份共享几何，无旧 CalendarView 兼容分支。
Header/CellTemplate/FullCellTemplate/RangeBars 的职责独立，所有输入与 Automation 使用同一有效选择防线。
农历不改变网格拓扑或焦点；业务条不改变选择、命中或 Cell 外间距。

## 11. 测试与验证

验证四种 Calendar 模式组合、事件、Header、日号、禁用、周号、模板、农历范围/Provider 与 overlay。
运行真实 Gallery 视觉、嵌套内容滚动、Style 命中和对象释放对照，覆盖重套模板/卸载/恢复。
按实际消费检查 Calendar/LunarCalendar 与公共面板的 TypeMap/主题保留，参见 [共享验收](../../data-entry/date-viewer/shared-panel-design.md#103-验收矩阵)。
