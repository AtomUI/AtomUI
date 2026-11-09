# DatePicker Token 设计

DatePicker 的日期正文直接消费 [DateViewer Token](../date-viewer/token.md)。Picker 自身只保留产品组合资源；输入框继续消费共享输入控件 Token，TimeView 使用 TimePicker Token。

## 控件 Token

| Token | 类型 | 当前来源 | 用途 |
| --- | --- | --- | --- |
| PanelContentPadding | Thickness | `PaddingSM` | Popup 中 TimeView 的内容内边距。 |
| ButtonsPanelMargin | Thickness | `0, UniformlyMarginXS, 0, 0` | Today、Now、Confirm Footer 与正文的间距。 |
| RangePanelSpacing | double | `20` | 双面板范围 Picker 的面板与双 Header 间距（RangeDatePicker 借用，`Controls[DatePicker]` 到达它）。 |
| HeaderBorderThickness | Thickness | `0, 0, 0, LineWidth` | 弹层面板 Header 底部分隔线（AtomUI 扩展，替换 `0,0,0,1` 字面量）。 |

弹层 Presenter 位于注册控件 ArrowDecoratedBox 内；Picker 把自己的 Token owner 转发给 Presenter，因此 Footer 边框、TimeView 与其
Token owner 透明的 TimeViewCell 读取 Picker 的有效全局 Token（`Controls[DatePicker]` / `Controls[RangeDatePicker]` 到达它们）。
弹层中的 DateViewer / RangeDateViewer 保持自己的身份：选中日期等面板视觉跟随 `Controls[DateViewer]`，弹层外框跟随
`Controls[ArrowDecoratedBox]`。Header 模板以 `DatePickerTokenResource` / `RangeDatePickerTokenResource` 显式读取。
输入区的箭头与激活条 Own Token 属于抽象身份 `InfoPickerInput`（`RangePickerArrowMargin`、`RangePickerIndicatorThickness`、
`RangeMarginToAnchor`），以 `Controls[InfoPickerInput]` 配置。

日期面板背景、Cell 尺寸、Hover、范围背景与面板宽度归 DateViewer 所有；Picker 用 RangePanelSpacing 把产品双面板间距固定为原体验的 20。Picker 不再注册旧 CalendarView 的 Cell、Header、MonthView 或 RangeCalendar 资源别名。

## 消费边界

Presenter 主题组合 DateViewer/RangeDateViewer、TimeView 与 Footer。固定布局和可见性放在 ControlTheme；动态颜色和尺寸通过对应控件的 Token 资源解析。候选值、活动端点和确认进度不进入 Token。

局部主题覆盖应作用于真实 owner：日期正文使用 DateViewer/RangeDateViewer Token 或专用 Semantic Style，Footer 使用 DatePicker Token，时间列使用 TimePicker Token。

## 验证

验证单值、双面板范围和日期时间三种组合，覆盖浅色/深色、动态主题切换、局部覆盖和 Popup 重建。Token 验证只证明资源消费；视觉等价仍需按 [共享验收矩阵](../date-viewer/shared-panel-design.md#103-验收矩阵)比较完整 Popup。
