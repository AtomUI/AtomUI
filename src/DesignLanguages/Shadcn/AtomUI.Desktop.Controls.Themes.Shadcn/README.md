# AtomUI.Desktop.Controls.Themes.Shadcn

`AtomUI.Desktop.Controls` 在 shadcn 设计语言下的取值包。对标 shadcn/ui 现行版本（registry 2026-11-15 实测）与
Tailwind CSS v4.3.5 色板；取值以取值附录为唯一基准：`docs/superpowers/specs/2026-11-15-shadcn-latest-alignment-data.md`
（附录 A Base Color、附录 B Theme、附录 C 各风格 registry class）。语言本身（轴、种子、语言 Token、全局映射）见
`../AtomUI.DesignLanguages.Shadcn/README.md`。

## 启用

```csharp
builder.UseDesktopControls();
builder.UseShadcn(language => language.AddDesktopControls());
```

`AddDesktopControls()` 同时登记 `AtomUI.Controls.Themes.Shadcn`（Avatar、Icon 与共享的抽象 Token 层）。

## 取值的由来

每个控件 Token 类一个推导（`<控件目录>/Shadcn<Xxx>Derivation.cs`）。起点是 Ant Design 取值包的同名推导（公式
复制而来，此后归本语言所有），shadcn 的取值按下表并入：对该控件视图的全局 Token 覆盖写在方法开头；控件 Token 中
被后面的公式读取的项就地替换（Card `CardShadows`、Splitter `HandleGripColor`、TimePicker/RangeTimePicker
`ItemPadding`），其余写在方法末尾。没有列出的控件只用 Ant 公式，颜色与几何随 shadcn 的全局 Token 变化。

| 控件 | 控件视图的全局 Token | 控件 Token | 依据 |
| --- | --- | --- | --- |
| Button、DropdownButton、SubmitButton、ResetButton | 圆角阶梯 = 风格按钮圆角；`ColorFocusBorder`/`LineWidthFocus` = 焦点环；`EnableWaveSpirit=false`；彩色 Theme 下 secondary 用 zinc 100/800；`ColorErrorBg(Hover)` = destructive 10%/20%（暗色 20%/30%）；未配置 `ColorLink` 时 `ColorLink(Hover/Active)` = 主色；暗色 Button 的危险描边与按下色 | 变体配方开关、禁用 50% 透明、按下位移、链接下划线（Sera 常驻）；outline 变体底/悬停/描边（`bg-background`、`dark:bg-input/30`）；Vega 的 outline 阴影 xs；按钮字重 | registry `button` |
| SplitButton | 圆角阶梯、`EnableWaveSpirit=false` | 内嵌分隔、分隔色 | registry `button-group` |
| FloatButton | outline 描边、`BorderRadius = min(ButtonRadiusLG, ControlHeightLG/3)`、`PaddingLG`、`SpacingXS` | outline 外观开关与合成底色、描述字重与行高、Vega 阴影 xs | `button` 的 outline 变体（官方无 FloatButton） |
| LineEdit、TextArea、Select | `ColorPrimary`/`ColorPrimaryHover` = `--ring`、`ColorBorder` = `--input`、暗色底 = input 的风格份额、圆角 = 风格输入框圆角；Sera 只画底边无焦点阴影，Luma/Rhea 底 = input 50% | 边框外观（Luma/Rhea 透明、Sera 底边）；Select 选项内缩一线、Luma 弱化选择指示 | registry `input`、`textarea`、`select` |
| Cascader、DatePicker、TimePicker、AutoComplete、Mentions | 同上，不含 Sera/Luma/Rhea 的框体规则 | TimePicker/RangeTimePicker 项居中、背景内缩一线、无项内边距 | registry `input` |
| CheckBox | `BorderRadiusSM` = 风格勾选框圆角；暗色 `ColorWhite` = 主色前景 | — | registry `checkbox` |
| RadioButton | — | 暗色选中点 = 主色前景 | registry `radio-group` |
| ToggleSwitch | 暗色 Neutral Theme：`ColorPrimary` = primary 45% 合成、悬停 48%、`ColorTextLightSolid` = foreground | 轨道色、关闭按压拉伸 | registry `switch` |
| Card | `ColorBgContainer` = `--card` | 描边环 + 风格卡片阴影、栅格悬停阴影、内边距 = 风格卡片内边距 | registry `card` |
| CardTabControl、CardTabStrip | 圆角阶梯与 8 取小 | — | registry `tabs`（卡片页签为 AtomUI 协调） |
| Dialog | `BoxShadows` = 描边环 + 风格 Dialog 阴影 | — | registry `dialog` |
| Drawer | — | 四向阴影 = 风格 Drawer 阴影；Vega 贴边 1px 环色边框 | registry `sheet` |
| Popup | — | 描边环 + 菜单阴影、风格菜单圆角 | registry `dropdown-menu`、`select` 面板 |
| FlyoutHost、ArrowDecoratedBox | — | Popover 阴影；环色一体描边、Popover 内边距、Tooltip 箭头 | registry `popover` |
| Menu、ContextMenu、MenuFlyoutPresenter | — | 描边环 + 菜单阴影；Menu 上下留白；Presenter 一体描边 | registry `menubar`、`context-menu`、`dropdown-menu` |
| NavMenu | — | 子菜单透明底、选中/悬停合成色、暗色样式的行色、弹出阴影 | registry `navigation-menu` / `sidebar` |
| ToolTip | — | 反色底与字、风格圆角、12×6 内边距、无阴影、箭头 | registry `tooltip` |
| ProgressBar | — | 指示 `--primary`、轨道 `--muted`、风格单一高度与圆角 | registry `progress` |
| Empty | 填充阶梯按 Ant 的 15%/6%/4%/2% 前景合成重建 | — | registry `empty`（插画为 AtomUI 内置） |
| Segmented | — | 轨道 = `--muted` | 官方无对应组件 |
| Splitter | — | 把手线与握点的合成色 | registry `resizable` |
| Steps | — | 面板圆角与 8 取小；暗色填充面板的合成色 | 官方无对应组件 |
| SimplePagination | `InputPaddingSM` 上下各减焦点环宽度 | — | registry `pagination` |
| Calendar、LunarCalendar、ComboBox、TreeView（暗色） | — | 选中/激活底 = 前景 14% 合成 | 暗色下两个源表面同档 |
| Descriptions（暗色） | — | 标签色 = `ColorTextDescription` | — |
| OtpLineEdit（Luma） | — | 单元格连成一体 | registry `input-otp` |

## 不支持 / 近似

| 项 | 说明 |
| --- | --- |
| 全大写与加宽字距（Sera） | 只对齐字号与字重 |
| 焦点外环（边框外 1-3px 半透明环） | 聚焦边框改用 ring 色 |
| 遮罩背景模糊 | 只对齐遮罩颜色与浓度 |
| Popover 圆角 | 弹层框体无圆角 Token，随全局 `BorderRadius` |
| 子菜单阴影比主菜单高一级（部分风格） | 都用主菜单档 |
| Nova 等的 Card/Dialog 灰底底栏 | 不做 |
| Toast/sonner | 官方由 sonner 绘制、registry 无 class，沿用 Ant 公式 |
| `ColorLink` 恰好配置成与 `ColorInfo` 相同的值 | 按未配置处理：按钮族链接取主色 |

## 控件目标等级（方案 11.1）

A 对标：官方有对应组件，按视觉验收口径对官方实测；B 协调：官方无对应组件，用语言 Token 与全局 Token 组合取值，
关键状态人工评审。「取值来源」一列标出推导是否在 Ant 公式之上并入了 shadcn 的取值。

| 控件 Token | 目标等级 | 对标的官方组件 | 取值来源 |
| --- | --- | --- | --- |
| `AdornerLayerToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `AlertToken` | A 对标 | Alert | Ant 公式（经 shadcn 全局 Token 着色） |
| `ArrowDecoratedBoxToken` | A 对标 | Popover | shadcn 覆盖 + Ant 公式起点 |
| `AutoCompleteToken` | A 对标 | Combobox | shadcn 覆盖 + Ant 公式起点 |
| `AutoCompleteSearchEditToken` | A 对标 | Combobox | Ant 公式（经 shadcn 全局 Token 着色） |
| `AutoCompleteTextAreaToken` | A 对标 | Combobox | Ant 公式（经 shadcn 全局 Token 着色） |
| `AvatarGroupToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `BorderBeamToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `BreadcrumbToken` | A 对标 | Breadcrumb | Ant 公式（经 shadcn 全局 Token 着色） |
| `ButtonToken` | A 对标 | Button | shadcn 覆盖 + Ant 公式起点 |
| `ButtonSpinnerToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `CalendarToken` | A 对标 | Calendar | shadcn 覆盖 + Ant 公式起点 |
| `CardToken` | A 对标 | Card | shadcn 覆盖 + Ant 公式起点 |
| `CardTabControlToken` | A 对标 | Tabs | shadcn 覆盖 + Ant 公式起点 |
| `CardTabStripToken` | A 对标 | Tabs | shadcn 覆盖 + Ant 公式起点 |
| `CarouselToken` | A 对标 | Carousel | Ant 公式（经 shadcn 全局 Token 着色） |
| `CascaderToken` | A 对标 | Select | shadcn 覆盖 + Ant 公式起点 |
| `CheckBoxToken` | A 对标 | Checkbox | shadcn 覆盖 + Ant 公式起点 |
| `CheckableTagToken` | A 对标 | Toggle | Ant 公式（经 shadcn 全局 Token 着色） |
| `CircleProgressToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `CollapseToken` | A 对标 | Accordion | Ant 公式（经 shadcn 全局 Token 着色） |
| `ComboBoxToken` | A 对标 | Select | shadcn 覆盖 + Ant 公式起点 |
| `CompactSpaceAddOnToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `ContextMenuToken` | A 对标 | Context Menu | shadcn 覆盖 + Ant 公式起点 |
| `CountBadgeToken` | A 对标 | Badge | Ant 公式（经 shadcn 全局 Token 着色） |
| `DashboardProgressToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `DatePickerToken` | A 对标 | Date Picker | shadcn 覆盖 + Ant 公式起点 |
| `DateViewerToken` | A 对标 | Calendar | shadcn 覆盖 + Ant 公式起点 |
| `DescriptionsToken` | B 协调 | — | shadcn 覆盖 + Ant 公式起点 |
| `DialogToken` | A 对标 | Dialog | shadcn 覆盖 + Ant 公式起点 |
| `DotBadgeToken` | A 对标 | Badge | Ant 公式（经 shadcn 全局 Token 着色） |
| `DrawerToken` | A 对标 | Sheet | shadcn 覆盖 + Ant 公式起点 |
| `DropdownButtonToken` | A 对标 | Dropdown Menu | shadcn 覆盖 + Ant 公式起点 |
| `EmptyToken` | A 对标 | Empty | shadcn 覆盖 + Ant 公式起点 |
| `ExpanderToken` | A 对标 | Collapsible | Ant 公式（经 shadcn 全局 Token 着色） |
| `FloatButtonToken` | B 协调 | — | shadcn 覆盖 + Ant 公式起点 |
| `FlyoutHostToken` | A 对标 | Popover | shadcn 覆盖 + Ant 公式起点 |
| `FormToken` | A 对标 | Field | Ant 公式（经 shadcn 全局 Token 着色） |
| `GroupBoxToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `HyperLinkButtonToken` | A 对标 | Button (link) | Ant 公式（经 shadcn 全局 Token 着色） |
| `ImageGroupPreviewerToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `ImagePreviewerToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `LineEditToken` | A 对标 | Input | shadcn 覆盖 + Ant 公式起点 |
| `ListBoxToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `ListTransferToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `ListViewToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `LunarCalendarToken` | B 协调 | — | shadcn 覆盖 + Ant 公式起点 |
| `MarqueeLabelToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `MentionsToken` | B 协调 | — | shadcn 覆盖 + Ant 公式起点 |
| `MenuToken` | A 对标 | Menubar | shadcn 覆盖 + Ant 公式起点 |
| `MenuFlyoutPresenterToken` | A 对标 | Dropdown Menu | shadcn 覆盖 + Ant 公式起点 |
| `MenuItemGroupToken` | A 对标 | Dropdown Menu | Ant 公式（经 shadcn 全局 Token 着色） |
| `MessageBoxToken` | A 对标 | Alert Dialog | Ant 公式（经 shadcn 全局 Token 着色） |
| `MessageCardToken` | A 对标 | Sonner | Ant 公式（经 shadcn 全局 Token 着色） |
| `NavMenuToken` | A 对标 | Navigation Menu | shadcn 覆盖 + Ant 公式起点 |
| `NotificationCardToken` | A 对标 | Sonner | Ant 公式（经 shadcn 全局 Token 着色） |
| `NumericUpDownToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `OptionButtonToken` | A 对标 | Toggle Group | Ant 公式（经 shadcn 全局 Token 着色） |
| `OtpLineEditToken` | A 对标 | Input OTP | shadcn 覆盖 + Ant 公式起点 |
| `PaginationToken` | A 对标 | Pagination | Ant 公式（经 shadcn 全局 Token 着色） |
| `PopupToken` | A 对标 | Popover | shadcn 覆盖 + Ant 公式起点 |
| `PopupConfirmToken` | A 对标 | Popover | Ant 公式（经 shadcn 全局 Token 着色） |
| `ProgressBarToken` | A 对标 | Progress | shadcn 覆盖 + Ant 公式起点 |
| `QRCodeToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `RadioButtonToken` | A 对标 | Radio Group | shadcn 覆盖 + Ant 公式起点 |
| `RangeDatePickerToken` | A 对标 | Date Picker | Ant 公式（经 shadcn 全局 Token 着色） |
| `RangeDateViewerToken` | A 对标 | Calendar | shadcn 覆盖 + Ant 公式起点 |
| `RangeTimePickerToken` | B 协调 | — | shadcn 覆盖 + Ant 公式起点 |
| `RateToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `ResetButtonToken` | A 对标 | Button | shadcn 覆盖 + Ant 公式起点 |
| `ResultToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `RibbonBadgeToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `ScrollViewerToken` | A 对标 | Scroll Area | Ant 公式（经 shadcn 全局 Token 着色） |
| `SearchEditToken` | A 对标 | Input | Ant 公式（经 shadcn 全局 Token 着色） |
| `SegmentedToken` | B 协调 | — | shadcn 覆盖 + Ant 公式起点 |
| `SelectToken` | A 对标 | Select | shadcn 覆盖 + Ant 公式起点 |
| `SeparatorToken` | A 对标 | Separator | Ant 公式（经 shadcn 全局 Token 着色） |
| `SimplePaginationToken` | A 对标 | Pagination | shadcn 覆盖 + Ant 公式起点 |
| `SkeletonToken` | A 对标 | Skeleton | Ant 公式（经 shadcn 全局 Token 着色） |
| `SliderToken` | A 对标 | Slider | Ant 公式（经 shadcn 全局 Token 着色） |
| `SpaceToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `SpinToken` | A 对标 | Spinner | Ant 公式（经 shadcn 全局 Token 着色） |
| `SplitButtonToken` | A 对标 | Button Group | shadcn 覆盖 + Ant 公式起点 |
| `SplitViewToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `SplitterToken` | A 对标 | Resizable | shadcn 覆盖 + Ant 公式起点 |
| `StatisticToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `StatisticCountUpToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `StepsToken` | B 协调 | — | shadcn 覆盖 + Ant 公式起点 |
| `StepsProgressBarToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `SubmitButtonToken` | A 对标 | Button | shadcn 覆盖 + Ant 公式起点 |
| `TabControlToken` | A 对标 | Tabs | Ant 公式（经 shadcn 全局 Token 着色） |
| `TabStripToken` | A 对标 | Tabs | Ant 公式（经 shadcn 全局 Token 着色） |
| `TagToken` | A 对标 | Badge | Ant 公式（经 shadcn 全局 Token 着色） |
| `TextAreaToken` | A 对标 | Textarea | shadcn 覆盖 + Ant 公式起点 |
| `TextBoxToken` | A 对标 | Input | Ant 公式（经 shadcn 全局 Token 着色） |
| `TimePickerToken` | B 协调 | — | shadcn 覆盖 + Ant 公式起点 |
| `TimelineToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `TimerStatisticToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `ToggleSwitchToken` | A 对标 | Switch | shadcn 覆盖 + Ant 公式起点 |
| `ToolTipToken` | A 对标 | Tooltip | shadcn 覆盖 + Ant 公式起点 |
| `TourToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `TransferListViewToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `TransferTreeViewToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `TreeSelectToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `TreeTransferToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `TreeViewToken` | B 协调 | — | shadcn 覆盖 + Ant 公式起点 |
| `TreeViewFlyoutPresenterToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `UploadToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `WindowToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `WindowTitleBarToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `WindowTitleBarButtonToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `WindowTitleBarToggleButtonToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
