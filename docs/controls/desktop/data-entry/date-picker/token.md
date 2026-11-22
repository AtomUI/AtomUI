# DatePicker Token 设计

DatePicker 的日期正文直接消费 [DateViewer Token](../date-viewer/token.md)。Picker 自身只保留产品组合资源；输入框继续消费共享输入控件 Token，TimeView 使用 TimePicker Token。

## 控件 Token

| Token | 类型 | 当前来源 | 用途 |
| --- | --- | --- | --- |
| PanelContentPadding | Thickness | `PaddingSM` | Popup 中 TimeView 的内容内边距（DatePicker / RangeDatePicker 各自 Own）。 |
| ButtonsPanelMargin | Thickness | `0, UniformlyMarginXS, 0, 0` | Today、Now、Confirm Footer 与正文的间距（DatePicker / RangeDatePicker 各自 Own）。 |
| RangeDatePicker.RangePanelSpacing | double | `20` | 双面板范围 Picker 的面板与双 Header 间距（仅 RangeDatePicker Own）。 |
| HeaderBorderThickness | Thickness | `0, 0, 0, LineWidth` | DatePicker 弹层面板 Header 底部分隔线（AtomUI 扩展，替换 `0,0,0,1` 字面量；RangeDatePicker 的 Header 没有分隔线，不读取它）。 |

弹层 Presenter 位于注册控件 ArrowDecoratedBox 内；Picker 把自己的 Token owner 转发给 Presenter，Presenter 再转发给弹层中的
DateViewer / RangeDateViewer（与 Calendar 相同，见 [主题运行时 §13](../../../../architecture/systems/theming/runtime.md)）。因此日期格
（选中、今天、文字）、日期面板文字、Header 与 Footer 边框、TimeView 与其 Token owner 透明的 TimeViewCell 都读取 Picker 的有效全局
Token：`Controls[DatePicker].ColorPrimary` 与 antd `components.DatePicker.colorPrimary` 一样改变选中日期格的颜色，
`Controls[DateViewer]` 的全局 Token 不再作用于 Picker 弹层。DateViewer 的 Own Token（`CellHoverBg`、`CellRangeBg`、`CellWidth`、
`PanelWidth` 等）仍按 DateViewer 身份读取，属于实际嵌入的公开 viewer：普通正文使用 `Controls[DateViewer]`，范围正文使用 `Controls[RangeDateViewer]`，不把两者身份合并。
弹层外框保持 ArrowDecoratedBox 自己的身份。Header 模板的全局 Token 以 `OwnerTokenResource` 读取，`HeaderBorderThickness` 以
`DatePickerTokenResource` 显式读取。
范围箭头 RangePickerArrowMargin 与激活条 RangePickerIndicatorThickness 属于实际 RangeDatePicker/RangeTimePicker；InfoPickerInput 不再拥有这些字段。

日期面板背景、Cell 尺寸、Hover、范围背景与面板宽度归 DateViewer 所有；Picker 用 RangePanelSpacing 把产品双面板间距固定为原体验的 20。Picker 不再注册旧 CalendarView 的 Cell、Header、MonthView 或 RangeCalendar 资源别名。

## 消费边界

Presenter 主题组合 DateViewer/RangeDateViewer、TimeView 与 Footer。固定布局和可见性放在 ControlTheme；动态颜色和尺寸通过对应控件的 Token 资源解析。候选值、活动端点和确认进度不进入 Token。

局部主题覆盖应作用于真实 owner：弹层日期正文、Header、Footer 与时间列的全局 Token 使用 `Controls[DatePicker]` / `Controls[RangeDatePicker]`；日期正文的 Own Token 使用 DateViewer Token，时间列 Own 使用当前 DatePicker/RangeDatePicker 自身的 ItemHeight/ItemWidth/PeriodHostWidth/ItemPadding/ItemMargin/HeaderMargin。

## 验证

验证单值、双面板范围和日期时间三种组合，覆盖浅色/深色、动态主题切换、局部覆盖和 Popup 重建。Token 验证只证明资源消费；视觉等价仍需按 [共享验收矩阵](../date-viewer/shared-panel-design.md#103-验收矩阵)比较完整 Popup。

## 独立 Token 身份与共享计算

DatePicker 与 RangeDatePicker 共享实际日期/时间计算，但 HeaderBorderThickness 仅 DatePicker 有真实 divider 消费；RangePanelSpacing 仅 RangeDatePicker。RangeDatePicker 的日期正文使用独立 RangeDateViewer Own；内嵌公开 Button/Icon 仍独立。将原 DatePicker.RangePanelSpacing 移到 RangeDatePicker，将日期时间列的原 TimePicker 覆盖移到当前日期终端。

## 2026-11-20 新增 Token

默认值与新增前的外观一致；PrimeNG 主题在各自控件阶段中赋值。

| Token | 默认值 | 用途 |
| --- | --- | --- |
| `PopupArrowVisible` | `true` | 弹层是否绘制箭头（`AbstractTimeViewToken`） |
