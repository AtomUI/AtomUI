# DatePicker 桌面版架构设计

本文说明当前基于 DateViewer 的桌面版架构。关联 [实现](implementation.md)、
[共享日期面板设计](../date-viewer/shared-panel-design.md)、[Token](token.md)、[Semantic Part](semantic-part.md)和
[Changelog](changelog.md)。

## 1. 控件定位

DatePicker 提供单值日期时间输入，RangeDatePicker 提供范围日期时间输入，归属 AtomUI.Desktop.Controls。
二者组合公共 DateViewer/RangeDateViewer、TimeView 与 Footer，拥有输入、格式化、Form、原生校验和弹层。
独立内嵌选择直接使用 [DateViewer 家族](../date-viewer/overview.md)。Picker 不生成日期网格，不拥有第二套选择或导航算法。

## 2. 设计语言

| 维度 | 定义 |
| --- | --- |
| 产品语义 | 浏览、预览、编辑并按现有规则提交日期或日期时间。 |
| 状态 | 提交值、默认值、打开锚点、候选值、活动端点、Hover 文本与确认进度分离。 |
| 面板 | 五种选择单位，四种网格；双面板共享原子范围状态。 |
| 主题 | 输入壳、日期、时间、Footer 各自拥有模板，完整 Picker 配置保持原体验。 |

## 3. API 与契约模型

下表定义输入语义与等价基线。

| 契约组 | 入口与语义 |
| --- | --- |
| 单值 | SelectedDateTime：可空日期时间，TwoWay，原生数据验证。 |
| 范围 | RangeStartSelectedDate / RangeEndSelectedDate：可空端点，TwoWay，原生数据验证；面板接收一次范围快照。 |
| 默认/重置 | DefaultDateTime、RangeStartDefaultDate、RangeEndDefaultDate；Clear 清空，Reset 使用默认值。 |
| 单位 | PickerMode：Date、Week、Month、Quarter、Year，独立于 PanelKind。 |
| 打开定位 | PickerDisplayDate：空选中时的锚点，不写入值或改变清除状态。 |
| 约束 | MinDate / MaxDate：包含边界，忽略时间并按单位解释；反向有效边界收敛到最小单位。 |
| 时间 | IsShowTime 与 ClockIdentifier：只有 Date 单位有效。 |
| 提交 | IsNeedConfirm、IsShowNow：保持按钮、自动与显式确认规则。 |
| 文本 | Format、Placeholder、Text/SecondaryText；保持格式化结果与预留宽度。 |
| 输入与弹层 | 共享输入表面与 Popup 打开、关闭、定位能力。 |

内部 Calendar、CalendarItem、Presenter、HeaderBackground 与指示器不是用户日期 API。
Picker owner 发布绑定、Form、部分/最终确认，不把 DateViewer.Selected 机械转发成提交。

## 4. 行为与状态模型

输入 owner 拥有受控提交值，编辑会话拥有打开期间候选日期时间与确认进度。
日期面板只消费有效投影并报告意图；浏览、焦点和 Hover 不修改提交值或 Form。
Hover 只预览活动输入文本，不污染端点。

| 场景 | 等价要求 |
| --- | --- |
| 打开 | 已选值优先于显示锚点；范围先确定活动端点再定位，另一端点不能拉回面板。 |
| 自动确认 | 保留原提交/关闭与范围分步流程。 |
| 显式确认 | 选择更新候选，Confirm 按原时机校验、提交与关闭。 |
| 时间范围 | 保留有效显式确认、活动端点时间和单日期面板布局。 |
| 外部越界 | 不改写输入值，面板不高亮无效值，不能确认提交该值。 |
| 部分范围 | 端点存在、端点确认和范围完成分别建模。 |
| 关闭 | 范围的普通、Escape、light-dismiss 与生命周期关闭丢弃未确认候选和 Hover 文本，保留已确认端点；显式 Clear 才清空范围提交值。单值沿用原关闭策略。 |

RangeDatePicker 的范围编辑使用端点轮转状态机。每次用户成功设置当前活动端点后，活动端必须自动转移到另一端；
输入焦点、下划线、Popup 箭头、日期面板 active part 和后续 Hover 预览必须同步到新的活动端。
空范围从 start 到 end 选择；已有完整范围重新打开时不先清空提交值，但本次编辑仍从当前活动端开始，
设置一端后继续要求用户设置另一端。Hover 预览由“固定端 + 当前 hover 端”推导，按日期顺序更新两侧输入文本和范围高亮，
但不写回提交值；最终确认同样只按日期顺序归一显示与提交，等日不同时间保持各端时间值。
需要确认的范围中，日期或时间选择只更新活动候选；每次 Confirm 只确认当前端并切换到另一端。关闭弹层时输入文本立即恢复为已确认值，重开时从尚未填写的端点继续，除非用户明确点选另一端。

非 Date 单位输出周首、月首、季度首或年首；周格式、跨年身份与整行反馈保持 Picker 基线。
疑似旧缺陷单独记录，不在重建中修正；取消不统一转换为打开值回滚。

## 5. 视觉与主题模型

| 场景 | 组合 |
| --- | --- |
| 单值 Date/Week | 单日期面板，Week 周列与整行反馈，Footer 按原条件。 |
| 单值 Month/Quarter/Year | 对应单面板：月份 4×3、季度 1×4、年份 4×3。 |
| 单值 Date + Time | 单日期面板、TimeView、Footer。 |
| 范围无有效时间 | 双月/双年/双十年，共享范围状态。 |
| 范围 Date + Time | 单日期面板、TimeView、活动端点切换与 Footer。 |

Picker 主题完整规定面板、Cell、Header/Footer、时间、双面板间距、连续范围形状与动效。
统一 Cell 不继承 Calendar Mini 尺寸；输入宽度、placeholder 截断、显式宽度与 Stretch 保持原体验。

## 6. 家族与集成

输入 owner → 编辑会话 → DateViewer/RangeDateViewer + TimeView + Footer。
Presenter 采用组合，不靠旧 Calendar 子类表达布局差异。原生 validation、Status、Form 由共享输入表面负责。
[pinned-open](../../other/popup/popup-pinned-open-design.md) 不阻止 detach、模板重建或无效锚点清理。

## 7. 重建与定制边界

旧 CalendarView/CalendarItem/Button 网格已经移除，不保留兼容 Cell 或回退引擎。
值、文本、焦点、视觉、事件/绑定/Form、确认及弹层结果必须逐步等价。共享基础设施的其他控件不在改动范围。
专用 Style 的实际输出纳入验收，不保留旧 Button 类型作为新 Cell 约束。

## 8. 验证策略

完整 [等价矩阵](../date-viewer/shared-panel-design.md#10-重建边界与等价验收)由共享设计拥有。
固定环境的视觉与操作轨迹用于逐步对照，不能只比较最后值或最后截图。
模块测试、真实 Gallery、资源释放与实际 AOT 消费分别记录验证证据。
