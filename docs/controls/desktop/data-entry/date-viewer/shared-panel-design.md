# DateViewer 日期面板共享设计

本文定义 DateViewer、RangeDateViewer、DatePicker、RangeDatePicker、Calendar 和 LunarCalendar 的共享日期面板架构。
DateViewer 的公共设计见 [架构设计](overview.md)，内部职责见 [实现原理](implementation.md)，视觉资源见 [Token 设计](token.md)，
定制边界见 [Semantic Part 契约](semantic-part.md)。宿主设计见 [DatePicker](../date-picker/overview.md) 与
[Calendar](../../data-display/calendar/overview.md)。

本文保存共享日期面板的批准设计与验收边界。DateViewer、Calendar 和 DatePicker 已迁入该结构；视觉、性能与 NativeAOT 结论仍以各自实际执行记录为准。
旧 API、内部类型、继承关系、模板节点、Part 名称、selector route 和资源键不构成重建约束，
但 DatePicker 家族的视觉体验、使用逻辑、数据绑定与 Form 可观察行为必须与重建前一致。名称或类型重建不能成为行为差异的理由。

## 1. 设计定位

DateViewer 是可以直接嵌入页面的单值日期选择面板，提供日期浏览、层级导航、键盘焦点、Pointer 预览、有效选择和单元格内容定制。
RangeDateViewer 是范围选择面板，提供两端点编辑和连续范围预览。二者共享一个日期内核和内部 DatePanel，均不要求 Popup 宿主。
RangeDatePicker 在 Popup 宿主中继续拥有提交、确认和活动端点轮转状态机。

Calendar 组合 DateViewer 承载日历正文，拥有业务 Header、Fullscreen/Mini、公开日期与模式、业务内容和 RangeBars。
LunarCalendar 在同一公历模型上增加农历呈现。DatePicker 与 RangeDatePicker 组合日期面板、TimeView 和 Footer，拥有输入、格式化、
原生数据校验、Form、弹层和编辑提交，不再各自生成日期网格。

组件归属 `AtomUI.Desktop.Controls`，公共命名空间为 `AtomUI.Desktop.Controls`，AXAML 命名空间为 `https://atomui.net`。
不新增 NuGet 包，不下沉桌面交互到 Core，不增加多日期、多区间选择或任意历法插件框架。

## 2. 设计原则

1. 日期拓扑、选择单位比较、范围视觉和导航计算只有一个实现入口。
2. 单值与范围提供不同的强类型公共契约，不使用 `object Value` 或一组互斥属性承载不同选择形态。
3. 选择单位、显示面板、浏览锚点、选择值、焦点和 Hover 分别建模。
4. 每个可写状态只有一个 owner；不可变渲染快照和 CellModel 不是第二个选择 owner。
5. Cell 只消费模型并报告意图，不决定 Picker 是否确认、Calendar 是否保留日号或范围是否交换。
6. 单元格使用统一 `DateViewerCell : TemplatedControl`；密度和产品视觉由 ControlTheme 表达。
7. 模板内容替换不绕过禁用、选择、焦点、命中测试和 Automation。
8. DatePicker 重建必须保持现有体验；发现现有行为疑似缺陷时单独记录，不借重建调整。
9. 生命周期、性能和 AOT 同时构成验收边界；完成形态不保留旧日期引擎或兼容分支。

## 3. 模型与 Public API

### 3.1 日期单位与显示面板

日期值继续使用 `DateTime`，日期内核忽略时间部分；时间拼接只发生在 Picker 编辑会话。

| 模型 | 取值或内容 | 责任 |
| --- | --- | --- |
| SelectionUnit | Date、Week、Month、Quarter、Year | 最终选择颗粒度与单位相等性。 |
| PanelKind | Date、Month、Quarter、Year | 当前显示网格；Year 表示十年区域中的年份网格。 |
| DisplayDate | 日期锚点 | 确定浏览区域，不代表已选值。 |
| FocusedValue | 日期或单位锚点 | roving focus，不自动选择。 |
| HoveredValue | 可空日期或单位锚点 | Pointer 预览，不修改实际选择。 |
| 单值选择投影 | 可空选择值、有效性 | 从当前 owner 推导选择标记。 |
| 范围选择投影 | 两端点、活动端点、已选择/确认状态 | 区分端点编辑与确认进度。 |
| 约束投影 | 包含边界、禁用规则、支持范围 | 决定导航与选择可用性，不改写调用方原始边界。 |

显示一个月份 Cell 不一定提交月份值。Date/Week 选择经过月份和年份网格时继续下钻；Month/Quarter/Year 到达目标网格时选择。
模式变更一次性生成新的有效快照，不用多次属性回调互相修补中间状态。

### 3.2 DateViewer 公共契约

下表规定新组件 API 的语义；未在源码中存在的成员不是现行可用 API。

| API | 类型/默认值 | 契约 |
| --- | --- | --- |
| Value | `DateTime?` / `null` | 单值选择入口，默认 TwoWay，启用 Avalonia 原生数据验证。 |
| DisplayDate | `DateTime` / 创建时的今天 | 浏览锚点，默认 TwoWay；浏览不改变 Value。 |
| SelectionUnit | 枚举 / Date | 最终选择单位，独立使用时非日期选择输出单位起始日。 |
| MinDate / MaxDate | `DateTime?` / `null` | 包含式边界，空值表示对应方向无界，按选择单位解释。 |
| DisabledDate | `Func<DateTime, bool>?` / `null` | 增加业务禁用约束，不能放宽边界；异常向调用方传播。 |
| ShowWeek | `bool` / `false` | 日期网格周序号；Week 选择仍必须显示周列与整周选择反馈。 |
| FirstDayOfWeek | `DayOfWeek?` / `null` | 空值跟随当前语言；周身份、周号和行首必须来自同一有效周规则。 |
| Presentation | 枚举 / Compact | Compact 或 Content；改变布局与呈现，不改变单位及选择。 |
| ShowHeader | `bool` / `true` | 显示内置导航 Header；Calendar 的业务 Header 由 Calendar 拥有。 |
| HeaderTemplate | `IDataTemplate?` / `null` | 强类型上下文和导航命令，不暴露内部节点。 |
| CellTemplate | `IDataTemplate?` / `null` | 替换默认日期值下面的内容。 |
| FullCellTemplate | `IDataTemplate?` / `null` | 替换完整内部内容，优先于 CellTemplate。 |

PanelKind、FocusedValue、HoveredValue 为只读运行状态；Header 命令表达导航、切换面板和定位操作。
Cell 上下文提供值、单位区间、显示文本、Today、面板归属、选择/禁用和范围/预览状态，保留强类型绑定。
Calendar/LunarCalendar 可以投影自己的上下文类型；DatePanel 不依赖这些公共宿主类型。

DateViewer 导航不改写 Value。外部 Value 失效时保留输入值，渲染有效选择投影不高亮该值；不得以 clamp 或清空隐式写回 ViewModel。
清除选择不清空浏览区域。独立选择将日期归一到 `.Date`，Month/Quarter/Year 分别输出月首、季度首日、年首；Week 使用有效周规则的起始日。
DatePicker 与独立面板使用同一有效语言周规则，周首、周号、范围单位比较和默认输入格式保持一致。

### 3.3 RangeDateViewer 公共契约

RangeDateViewer 复用日期单位、浏览、约束、Header、Presentation 与 Cell 定制语义，但 Value 使用可空、不可变的强类型范围对象。
范围对象包含可空 Start/End；空对象表示无选择，单端点表示编辑中间状态，双端点表示完整候选。不把两个端点做成相互回调的选择 owner。

活动端点与确认进度分开：已有两端点不代表本次编辑两端都完成确认，活动端点也不能由哪个值为空临时推断。
反向候选与正向视觉区间分开建模；视觉按日期顺序绘制，宿主确定实际输出的交换或拒绝规则。
独立范围面板完成有效两端选择后输出升序范围；Picker 使用现有端点编辑和最终提交规则。
活动端点变化不能清空任一已提交端点或输入 owner 的可视文本；对向端点的选中态必须继续由提交值或候选值投影。

面板数量属于展示配置，不由日期是否含时间猜测：普通范围使用双面板，日期时间 Picker 使用现有单面板与活动端点切换。
两个面板消费同一范围状态，第二面板不持有自己的选择值、Hover 或确认进度。

### 3.4 事件与通知

| 通知 | 触发条件 |
| --- | --- |
| ValueChanged | 用户有效选择导致组件所拥有的值实际变化。 |
| Selected | 每次有效选择，同值重选也通知。 |
| PanelChanged | 用户改变浏览区域或显示面板；同一区域的重复设置不通知。 |
| HoveredValueChanged | 预览变化，供宿主更新输入文本或范围预览。 |

程序赋值只更新属性与渲染，不模拟前三类用户事件。一次操作先完成状态与模型提交，再发布通知；跨面板选择按
PanelChanged → ValueChanged → Selected 顺序省略不适用事件。宿主自己的公开通知由宿主发布，不能同时转发 DateViewer 通知和宿主通知产生重复。
DatePicker 的绑定通知、Form 通知、部分确认与最终确认时机以等价基线为准，不套用新的公共面板通知替代。

## 4. 面板、呈现与宿主策略

### 4.1 网格与周期

| PanelKind | 行×列/内容 | 相邻区域 |
| --- | --- | --- |
| Date | 6×7 日期；周列启用时 6×8，含 6 个周号 | 相邻自然月。 |
| Month | 4×3 月份 | 相邻自然年。 |
| Quarter | 1×4 季度 | 相邻自然年。 |
| Year | 4×3 年份，十年区域加相邻两年 | 相邻十年。 |

所有日期网格同时提供行列几何和日期区间。相邻面板通过当前周期计算，不固定为 DisplayDate 加一个月。
边界附近不能溢出 DateTime；不可表示槽位不能激活，网格、文本、占位和可见范围须满足宿主基线，不得用回绕日期掩盖溢出。

### 4.2 产品组合

| 宿主/模式 | 日期正文 | 附属能力 |
| --- | --- | --- |
| 独立 DateViewer | 单面板，Compact 默认 | 浏览 Header、单值选择；Date 单位含 Today 页脚。 |
| 独立 RangeDateViewer | 双周期面板，Compact 默认 | 同一范围会话、端点与连续预览。 |
| Calendar Month | DateViewer 日期面板 | Calendar Header、业务内容；Fullscreen 可显示 RangeBars。 |
| Calendar Year | DateViewer 月份面板 | Calendar Header；月份激活保留并截断宿主日号。 |
| LunarCalendar | Calendar 对应面板 | 公历模型上的农历内容、支持范围和 Provider 投影。 |
| 单值 Picker Date/Week | 单日期面板 | Footer；Week 使用整周反馈。 |
| 单值 Picker Month/Quarter/Year | 对应单面板 | 到达目标单位后选择，不继续下钻。 |
| 单值 Picker Date + Time | 单日期面板 | TimeView、Footer，按原规则拼接与确认。 |
| 范围 Picker 无有效时间 | 双月/双年/双十年面板 | 两端点编辑、部分确认与最终确认。 |
| 范围 Picker Date + Time | 单日期面板 | TimeView、活动端点切换，保留原显式确认流程。 |

时间只在 Date 选择单位下有效。Calendar Mini、Calendar Fullscreen 与 Picker Compact 是各自完整的主题配置，
不能把 Calendar Mini 的 256 高度等布局默认值直接用作 Picker 基线。

### 4.3 宿主语义

- Calendar Header 年/月操作保持自己的选择提交语义；DateViewer 导航 Header 只浏览。
- Calendar 月份候选保留原日号并截断到目标月末；Month Picker 输出月首。模型提供区间，提交转换由 owner 完成。
- Calendar 的月份首尾禁用、周号行首选择和自定义 Header 命令约束由 Calendar 策略保持，不强行改成 Picker 策略。
- Picker 外部越界值、反向边界收敛、整周规则和导航限制保持现行行为；LunarCalendar 保留支持范围与 ValidRange 交集及 Value 收敛。
- 日期禁用、选择单位禁用和能否继续下钻必须分别计算；不能只检查单位首日或把导航格误当最终提交值。
- 输入事件的消费规则由宿主决定：Picker 滚轮翻页不得自动施加给内嵌 Calendar 或破坏外层滚动链。

## 5. 架构与状态 ownership

```text
独立 DateViewer / RangeDateViewer / Calendar / Picker Presenter
  → 日期面板宿主契约：状态投影 + 操作提交
  → DatePanel：一份面板交互会话
  → 纯日期算法与不可变 PanelModel / CellModel
  → 有界 DateViewerCell 容器池
  → ControlTheme / 内容模板 / Automation
```

| 责任单元 | 拥有的状态与输出 | 不负责 |
| --- | --- | --- |
| 日期算法 | 单位区间、周期、拓扑、有效性和导航目标计算 | 控件、订阅、Popup、提交值。 |
| DatePanel | 浏览、层级、焦点、Hover；模型生成与容器实现 | 输入格式化、时间、业务选择提交。 |
| 独立 DateViewer | 单值属性与有效选择提交 | Calendar 业务 Header 和 Picker 编辑事务。 |
| 独立 RangeDateViewer | 原子范围值、活动端点、独立范围完成规则 | 输入端点文本与弹层确认进度。 |
| Calendar | Value、Mode、公开事件、Header、RangeBars | 第二份日期引擎或农历选择值。 |
| Picker 输入控件 | 受控提交值、显示文本、Form/验证、打开状态 | CellModel 与日期网格。 |
| Picker 编辑会话 | 候选日期时间、端点编辑、确认进度 | 另建 hover、焦点或双面板选择副本。 |

宿主契约只负责提供有效输入并接收 Navigate、ChangePanel、MoveFocus、Hover、Activate 等明确意图。
独立控件与嵌入宿主使用同一面板管线；不暴露 IsHosted、SuppressCommit 等开关，不以内部模式分叉重写选择算法。
候选状态集中在宿主会话中，DateViewer 的托管投影不再成为可写选择 owner，也不参与宿主值之间的 TwoWay 环形绑定。

## 6. Template 与定制契约

### 6.1 统一容器

DateViewerCell 的外层保留状态、Pointer、焦点与 Automation，内部承载默认值、次级呈现和内容区域。
CellTemplate 保留默认值，FullCellTemplate 优先替换内部内容。周标题和只作周号的节点不接受业务日期模板。
农历专用容器或内容投影可以扩展呈现，但必须复用相同激活路径和 CellModel。

从 Button 重建为 TemplatedControl 必须明确按下、释放、捕获取消、移出、拖动、禁用切换及嵌套内容的激活规则，
保证一次操作只提交一次；Pointer、Enter/Space 和 Automation 通过同一有效性检查。
日期容器按模型运行时生成；Header、Footer、布局结构、Transitions 和固定属性关系仍声明在 AXAML。
独立 DateViewer 的 Today 页脚在托管时隐藏；Picker 继续拥有弹层 Footer，Calendar 不出现日期面板页脚。

### 6.2 Theme 与 Token

DateViewer 默认主题从 SharedToken 派生完整视觉参数，独立使用不依赖 DatePicker、TimeView 或农历资源。
Picker 主题明确提供现有面板宽高、网格尺寸、Header/Footer、双面板间距、字号、状态色、连续范围形状和动效。
Calendar 与 LunarCalendar 主题提供自己的密度、内容行和 overlay 避让参数。

资源名称、Token owner 和旧公式的组织可以重建，重建后的输出与局部覆盖效果必须满足对应产品基线。
宿主通过 owner-scoped ControlTheme Setter 投射参数，子主题维护自己的模板，不通过父主题连续穿透子模板。
状态不进入 Token；运行态快照不持有 DynamicResource。农历次级内容优先级仍由 Calendar 家族呈现层决定。

### 6.3 Semantic Part

DateViewer 家族的定制区域为 root、header、body、content、cell、cellContent；Header 关闭时 header 为 Optional，
cell 与 cellContent 是运行时多节点区域。公共 descriptor、ContractType、cardinality 和专用 Style 从真实实现产生，
本设计不提前声明不存在的生成类、Since 或 SelectorRoute。

Picker、Calendar 以及 DateViewer 分别拥有自己的 Semantic owner。组合后重新建立真实 route，并验证跨模板、逻辑树和 Popup 根的命中。
不为保留旧 Button ContractType 创建重复容器，也不把新 marker 的存在当成旧 Style 等价的证据。
应用示例只使用生成的专用 Style；未命中必须修正 route，禁止 code-behind 属性设置回退。

## 7. 数据流、编辑会话与生命周期

### 7.1 状态转换

```text
外部属性输入或用户意图
  → owner 执行一次确定状态转换
  → 有效约束、选择投影与面板快照
  → PanelModel / CellModel
  → 更新池化容器与主题状态
  → 发布用户通知
```

显示月份改变才改变日期拓扑；选择、焦点和 Hover 更新只改变必要状态。日期计算不进入 Measure/Arrange 热路径。
提交前重新验证有效性，避免模型生成后约束发生变化使过期 Cell 被提交。
范围真实端点、候选端点和 hover preview 分别推导；预览中间区域与端点不污染真实选中标记。
激活有效 Cell 时必须先清理旧 Hover preview，再让宿主提交和切换活动端，防止旧 hover 在新活动端上下文中覆盖已提交范围选中态。
双面板范围选择中，非本面板周期的 outside cell 只作为浏览填充，不投影 selected、visual endpoint、range endpoint、range middle 或 preview 状态；同一个真实端点只能在所属面板周期内高亮一次。
周模式按整行输出连续视觉，周号与七个日期参与同一范围；命中区域包含日期格间隙和两个面板，不只依赖 Cell PointerEntered。

### 7.2 Picker 编辑会话

| 操作 | 日期面板 | Picker 编辑会话与输入 owner |
| --- | --- | --- |
| 打开 | 接收单位、约束、选择投影和锚点 | 按既有顺序初始化候选值、时间、活动端点与确认进度。 |
| 浏览 | 改变 DisplayDate/PanelKind | 不改提交值，不触发 Form 值变化。 |
| Hover | 更新唯一预览状态 | 按原规则拼接日期时间；范围输入按固定端与 hover 端的日期顺序更新两侧预览文本。 |
| 选择 | 报告有效日期/单位 | 更新候选值，决定立即提交还是等待确认。 |
| 时间编辑 | 不拥有时间 | 更新活动端点候选时间及原有预览。 |
| Today/Now | 接收相应日期状态 | 保持原按钮可用性、时间来源及确认规则。 |
| 部分确认 | 接收活动端点变化 | 按原时机写回刚确认端点、自动切换到另一端，不伪装成最终确认。 |
| 最终确认 | 保持选择投影 | 按原顺序校验、交换需要交换的端点、写回与关闭。 |
| 普通关闭/Escape/light-dismiss | 清理交互状态 | 丢弃未确认候选与 Hover 文本，保留已确认端点；关闭后的输入文本只投影提交值。 |

范围活动端点的显示定位使用该端点与当前面板周期，不允许开始值同步将结束面板拉回开始区域。
RangeDatePicker 每次打开都重建本次端点确认进度：空范围从 start 到 end；单端点范围把已有端点作为固定端；
完整范围保留提交值作为初始显示，但本次编辑仍要求从当前活动端开始重新确认两个端点。
活动端或约束变化不把 Presenter 候选端点提升为提交值；只有外部端点更新及有效确认改变提交值。
完整范围使用 start 所在周期作为双面板左锚点；active end 只表达当前编辑端点，不能通过倒推一个周期改变左面板月份。
设置当前端点后，活动端、输入焦点、Popup 箭头和 DatePanel active part 必须同步翻转到另一端。
端点翻转和弹层关闭不得清理对向端点的值或本端刚确认值；显式 Clear、表单清空才清除提交值。单端范围重开默认定位到缺失端，用户明确点击某端时以点击端为准。
范围日期时间的有效确认行为与用户原始 IsNeedConfirm 请求分开表示，不需要覆盖 public 属性再保存 backup 来表示内部派生状态；
对外可观察结果仍须与原行为逐步比较，属性通知的差异不能被会话封装隐藏。

### 7.3 生命周期

| 获取 | 对应释放/重建边界 |
| --- | --- |
| 模板节点、节点事件和可替换目标绑定 | re-template 前释放旧目标，再回放当前快照。 |
| 语言、宿主与主题订阅 | detach/宿主替换释放，重新 attach 时按当前语言恢复。 |
| Cell owner、model、context、内容模板和专用呈现 | 归还容器池、面板收缩、detach 或呈现策略替换时清除。 |
| Picker 会话与其订阅 | 按关闭路径结束，生命周期关闭和 owner detach 不受 pinned-open 阻止。 |
| 农历面板数据及 Provider 引用 | 显示区域/配置/Provider 版本失效、呈现解除或 owner 释放。 |
| RangeBars 资源宿主 attachment | 仍由 Calendar 在集合变更、detach 和 owner 释放时成对处理。 |

重套模板不创建第二个会话；重开按现有规则初始化，旧会话不得保留新模板或旧 Gallery 页面。
容器池有界，隐藏面板不保留过期 context 或业务内容订阅。

## 8. Calendar、农历与业务范围条

Calendar 组合结构为 CalendarHeader/HeaderTemplate 与 Body，其中 Body 叠放 DateViewer 和 RangeBarPanel。
Calendar Value/Mode 及用户事件仍由 Calendar 提交；共享面板不改变其日期规范化、月份日号截断、周号来源或模板优先级。

LunarCalendar 使用公历日期、支持范围交集和有界农历数据投影。缓存键由显示区域、PanelKind、Culture、呈现开关、Provider identity
与 revision 组成；同一区域只改变选择或焦点不重复查询 Provider。选中农历信息仍来自宿主公历 Value，不产生第二个选中日期。
农历 Header 内容归 Calendar，农历呈现不拥有通用面板 Header 或选择算法。

RangeBars 是业务区间标记，不是范围选择。集合、条目资源宿主、lane 排列和 overlay 仍由 Calendar 拥有，
仅在 Fullscreen Month 显示并保持 IsHitTestVisible=false。overlay 消费共享日期拓扑与实际布局几何；不维护第二套网格起点或行列计算。
农历避让按次级文本行与布局 metrics 计算，不通过改变 Cell 外 margin 或覆盖选择区域完成。

## 9. 资源、性能与 AOT 边界

遵循 [AOT 编程规范](../../../../engineering/development/aot-programming-guidelines.md)与
[控件注册契约](../../../../architecture/foundations/control-registration-contracts.md)。新组件、主题、Token 与 Semantic descriptor
通过现有生成式注册参与 TypeMap 和主题资产保留，不新增运行时发现机制。

独立 DateViewer 的最小消费必须能保留自身主题、Token 和 Style，不依赖全量 Gallery；它不反向保留 DatePicker 输入壳、TimeView
或 LunarCalendar。宿主消费必须形成真实静态资源依赖，不能因开发环境已加载全部主题而漏掉裁剪边界。
算法和快照使用强类型 C#，模板使用编译绑定；生成文件不手工修改。

性能比较覆盖首次实例化、首次打开、同月选中、Hover、跨周期、双面板和 Gallery 页面加载，并记录容器数量、分配与释放。
减少类型或代码数量不构成速度提升证据。测量必须保持相同 UX、主题、日期内容、实例数量和执行环境，不能通过删面板或关闭动效降低成本。

## 10. 重建边界与等价验收

### 10.1 契约边界

重建不保留旧内部结构，也不以旧 API 名称、模板节点、Semantic route 或资源键限制新设计。共享输入和 Popup 基础设施中的其他控件不在重建范围内。
DatePicker 的选项含义、默认体验、数据绑定结果、Form 值、校验反馈、通知时机与视觉结果均为等价要求；名称变化不解除这些义务。
非原生错误状态继续作为 native validation 的投影，不能增加平行 error 机制。

### 10.2 基线与比较方法

在产品源码重建前采集旧实现的视觉与操作轨迹。固定窗口、可用空间、缩放、字体、语言、主题、Today/Now 和动画采样时刻；
状态轨迹逐步记录提交值、候选值、输入文本、浏览区域、焦点、活动端点、确认进度、事件/绑定/Form 通知顺序及弹层打开状态。
同一操作序列应用于新旧实现，比较每个阶段，不只比较最终值或最终截图。

视觉比较覆盖面板/Header/Footer/时间区域的 bounds、行列对齐、文字、背景、边框、圆角、连续范围与动效。
几何和状态差异必须消除；采集时间与渲染噪声通过控制环境处理，不通过放宽断言接受产品偏差。
Pointer 比较保留原尺寸、裁剪、格间隙、嵌套内容、可见滚动条和外层滚动链；旧 Button 的按下/释放取消行为也属于基线。

### 10.3 验收矩阵

| 层级 | 最小覆盖 |
| --- | --- |
| 纯日期逻辑 | 闰年、跨年周、五种单位、四种面板、双周期、包含边界、反向边界、DateTime 极值、范围端点与预览隔离。 |
| 独立面板 | 空值浏览、清除、外部值失效不回写、下钻/选择、同值重选、通知顺序、Cell/FullCell 优先级、Header 命令、原子范围。 |
| Picker 操作 | 单值/范围/日期时间，默认值/reset/clear、显示锚点优先级、打开端点、hover 输入、部分/最终确认、全部关闭路径、运行时模式与边界变化。 |
| Picker 视觉 | 五种颗粒度、单/双面板、时间与 Footer、全部选择/禁用/预览状态、Light/Dark、语言、尺寸、显式宽度和 Stretch 下输入宽度稳定。 |
| 输入与 Automation | Pointer 按下释放/移出取消、跨格/整周命中、键盘、滚轮、外层滚动链、Selection/SelectionItem/激活的一致 owner。 |
| Calendar 家族 | Month/Year × Fullscreen/Mini、日号截断、禁用规则、Header、事件、模板、RangeBars、农历支持范围、Provider 缓存与次级行避让。 |
| Theme/Semantic | 真实 descriptor、marker、专用 Style 命中及 Setter 生效；静态 AXAML 绑定、主题边界与动态资源覆盖。 |
| 生命周期 | re-template、detach/attach、关闭重开、宿主切换、容器收缩和回收；旧上下文、会话及页面可释放。 |
| AOT/裁剪 | 独立 DateViewer/RangeDateViewer 消费与 Picker/Calendar/LunarCalendar 宿主消费，按实际平台支持分别验证。 |

测试采用 [测试价值与生命周期规范](../../../../engineering/development/test-value-and-lifecycle.md)：先复用现有契约测试，新增探针临时存放、
执行后判定去留并清理，再对最终工作树执行 [模块验证](../../../../engineering/workflows/affected-verification.md)。
验收同时要求真实 Gallery 视觉与操作对照，不能以 Headless 通过替代产品体验证据。
旧日期引擎、重复容器与兼容分支清理完成后才能认证最终架构；未采集等价基线或未完成对照不能宣称无视觉/逻辑偏差。
