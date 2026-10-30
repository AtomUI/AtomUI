# DateViewer 桌面版架构设计

DateViewer 是无需输入框或 Popup 的单值日期面板，RangeDateViewer 是原子范围日期面板。二者位于
`AtomUI.Desktop.Controls` 包与同名命名空间，AXAML 使用 `https://atomui.net`。内部职责见
[实现原理](implementation.md)，家族设计见 [共享日期面板设计](shared-panel-design.md)，视觉资源见
[Token 设计](token.md)，定制入口见 [Semantic Part 契约](semantic-part.md)，变化见 [Changelog](changelog.md)。

## 1. 控件定位

公开面板负责日期浏览与选择，支持 Date、Week、Month、Quarter、Year 五种单位。单值和范围各自拥有
明确的值类型，共享日期算法、交互会话和基础 Cell。日期格式化、输入框、时间编辑、Form、Popup 关闭、
Calendar 业务 Header 与 RangeBars 属于对应宿主。
独立 DateViewer 的默认日期视图含四方向导航和 Today 页脚；Today 通过与日期 Cell 相同的有效性检查选择当天。

## 2. 设计语言

| 维度 | 契约 |
| --- | --- |
| 值与浏览 | Value、DisplayDate、焦点与 Hover 分开；浏览不提交选择。 |
| 内容 | CellTemplate 增加内容，FullCellTemplate 接管内部内容，保留交互容器。 |
| 呈现 | Compact 使用面板自然宽度；Content 展开列宽，日期位于带顶部分隔线的单元格右上方，内容位于其下。 |
| 范围 | 一份 DateViewerRange 表达两端和中间状态，左右面板不分别写端点。 |
| 定制 | 生成专用 Semantic Style 封装真实 owner-relative route。 |

Content 中普通 Today 日期的主色顶边、浅主色整格背景和主色文字独立于日期是否可选择；禁用状态仍阻止选择。

## 3. API 与契约模型

以下属性同时存在于 DateViewer 与 RangeDateViewer；值类型由 owner 区分。

| API | 类型 / 默认值 | 语义 |
| --- | --- | --- |
| DateViewer.Value | DateTime? / null | 单值，默认 TwoWay，启用数据验证。 |
| RangeDateViewer.Value | DateViewerRange? / null | 不可变范围，默认 TwoWay，启用数据验证。 |
| DisplayDate | DateTime / 实例创建时 DateTime.Today | TwoWay 浏览锚点，不代表选择。 |
| SelectionUnit | DateViewerSelectionUnit / Date | Date、Week、Month、Quarter、Year。 |
| Presentation | DateViewerPresentation / Compact | Compact 或 Content。 |
| MinDate / MaxDate | DateTime? / null | 包含式边界；与 DisabledDate 共同决定可选择周期。 |
| DisabledDate | Func<DateTime, bool>? / null | 返回 true 表示禁用，不放宽边界。 |
| FirstDayOfWeek | DayOfWeek? / null | 未指定时使用有效语言的周首。 |
| ShowWeek | bool / false | 日期网格显示周号列；Week 单位保留整周选择反馈。 |
| ShowHeader | bool / true | 显示独立导航 Header。 |
| HeaderTemplate | IDataTemplate? / null | DateViewerHeaderContext 强类型内容。 |
| CellTemplate / FullCellTemplate | IDataTemplate? / null | DateViewerCellContext 强类型内容；FullCellTemplate 优先。 |

DateViewerRange 是 `sealed record DateViewerRange(DateTime? Start, DateTime? End)`。允许单端点、反向、
含时间或失效输入；控件保留调用方输入，用户完成有效范围时提交升序的规范化端点。

PanelKind 为 DateViewerPanelKind，只读值 Date、Month、Quarter、Year；FocusedValue 和 HoveredValue 为
只读 DateTime?。这些状态通过 DirectProperty 提供，不是可写选择 owner。

| 事件 | DateViewer 参数 | RangeDateViewer 参数 | 语义 |
| --- | --- | --- | --- |
| ValueChanged | DateViewerValueChangedEventArgs | RangeDateViewerValueChangedEventArgs | 用户提交改变值时提供 OldValue 与 Value。 |
| Selected | DateViewerSelectedEventArgs | RangeDateViewerSelectedEventArgs | 用户有效激活；同值重选仍通知。 |
| PanelChanged | DateViewerPanelChangedEventArgs | 同左 | 提供 DisplayDate 与 PanelKind。 |
| HoveredValueChanged | DateViewerHoveredValueChangedEventArgs | 同左 | 提供可选 Hover 值或 null。 |

Selected 参数同时提供 SelectionUnit；范围参数额外提供本次 SelectedValue。上述事件是强类型 CLR 事件，
不是 RoutedEvent；程序赋值通过 Avalonia 属性通知观察，不模拟用户选择事件。

### 强类型模板上下文

DateViewerCellContext 提供 Value、Today、CellType、DisplayValue，以及 IsToday、IsInView、IsSelected、
IsDisabled、IsFocused、IsRangeStart/End/Middle、IsRangePreviewStart/End/Middle。模板内容只消费状态，
不接管 Pointer、键盘、焦点或 Automation。

DateViewerHeaderContext 提供 DisplayDate、PanelKind、SelectionUnit、DisplayText、ParentPanelKind、
PreviousPage、PreviousPeriod、NextPeriod、NextPage、ShowPeriodNavigation、NavigateCommand、
ChangePanelCommand。NavigateCommand 参数必须是 int；Date 面板单周期为一个月，Page 参数为 ±12；
其他面板 Page 参数为 ±1。ChangePanelCommand 参数必须是 DateViewerPanelKind，
CanExecute 限制有效层级。范围的每个可见 Header 接收对应面板的上下文。

## 4. 行为与状态模型

浏览、焦点和 Hover 不修改 Value。外部失效值不隐式 clamp 或清空，面板按有效约束计算高亮和激活。
清除选择保留浏览区域。用户选择按单位输出日期、周首、月首、季度首或年首。

范围第一次选择输出单端点，第二次有效选择输出升序范围；Hover 预览不写回 Value。
完整预览范围存在时，它替代已提交范围参与 Cell 视觉投影：仅规范化后的预览起点、终点与中段高亮，
不同时保留旧端点的选中视觉；尚不能组成范围时只显示弱 Hover 与 pending selection。
四种网格与层级、周规则、禁用周期策略见 [共享设计](shared-panel-design.md#4-面板呈现与宿主策略)。

## 5. 视觉与主题模型

Week 的周首、首周规则、周号与跨年身份统一由有效语言计算；英语美国、繁体中文和葡萄牙语巴西使用
Sunday 周首与包含元旦的首周，简体中文使用 Monday 周首与包含 1 月 4 日的首周。显式 FirstDayOfWeek
覆盖周首。周模式 Hover、选中与范围端点覆盖周号和七个日期；周号使用半透明文字。

DateViewer、RangeDateViewer、DateViewerHeader、DatePanel、DateViewerCell 分别维护自己的 ControlTheme。
基础 Cell 是内部 TemplatedControl，使用同一激活路径，不要求用户依赖旧 Button 类型。

DateViewerToken 的十八个资源为独立面板提供默认视觉，状态文字、主色和边框复用 SharedToken。
Compact 日期网格采用 288 的自然面板宽度、36 的列与行节距、24 的值框和正文左右 18 的留白。
独立 DateViewer 默认收紧到面板自然宽度；Content 仍使用父布局分配的宽度。
Compact 的月、季度、年值框应用 PeriodCellWidth 和 PeriodCellPadding，在整行内垂直居中。
空值浏览不制造选中态。
内容模板不改变禁用、选择、命中测试、焦点或 Automation。Semantic 支持 root、header、body、content、
cell、cellContent，以及独立 DateViewer 的 footer；类型、数量和专用 Style 见 [完整契约](semantic-part.md)。

## 6. 控件家族与集成

单值与范围没有公开抽象基类。DatePanelSession 管理交互 cursor，实际选择只由独立公共组件或产品宿主拥有。
托管接口和 PanelTheme/CellTheme 等协作入口都是 internal，不作为应用 API。

Calendar 关闭公共面板 Header 并保留自己的业务 Header；LunarCalendar 提供内容投影；
Picker 由编辑会话控制候选和提交。托管面板隐藏 DateViewer 默认 Today 页脚，由 Picker 自己的 Footer 承担相应操作。
家族职责与等价验收由 [共享设计](shared-panel-design.md)定义。

## 7. 重建与定制边界

应用使用公共值、模板上下文、Token Resource 和生成 Semantic Style，不依赖 internal 类型、PART 名称或
内部伪类。没有公开 IsHosted/SuppressCommit 开关，也没有多日期、多范围、时间或日程排布能力。

## 8. 验证策略

独立控件的模型、模板、事件、Style 和交互使用定向测试。Gallery 独立入口是
[DateViewerShowCase](../../../../../controlgallery/AtomUIGallery/ShowCases/DataDisplay/DateViewer/Views/DateViewerShowCase.axaml)，
覆盖原子范围、五种单位、空值浏览、Compact/Content、边界/禁用、内容模板、Header 命令和专用 Style。

公共面板、主题、生成契约及 Calendar/Picker 宿主迁移均已落地；自动化覆盖业务行为、Popup 几何、Gallery 和
osx-arm64 NativeAOT 发布/启动。由于本机自动化无法附加当前 checkout 的原生 Gallery，最终像素与真实 Pointer
手感仍保留人工复核边界，不能由 Headless 测试替代。
测试生命周期和最终工作树门槛见 [共享验收](shared-panel-design.md#10-重建边界与等价验收)。
