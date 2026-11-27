# 控件级全局 Token 改写（EvaluateGlobal）清单

> **用途已变更（2026-11-22）：** 方案改为由控件推导直接输出控件视图（方案 6.1），逐控件的全局改写换写为 `view.Global`
> 上的覆盖，不再改成 Own Token。本表现作为 M4 换写的对照清单：Table 1 给出每处改写，「EvaluateOwn 依赖」标出换写时须改读
> `view.Global` 的地方。Table 2 的「去向 A/B/C」与「新增 Own Token 名」两列按旧做法填写，已不适用。

> 日期：2026-11-22 · 工作树：`feature/design-language-products`（基于 `9f54cdf2a`，扫描时工作树有他人进行中的未提交改动，见“不确定项”第 1 条）

本清单覆盖 `src/AtomUI.Themes.Shadcn` 与 `src/AtomUI.Themes.PrimeNG` 中所有 `IControlTokenAlgorithm<TToken>.EvaluateGlobal` 对控件级全局 Token 副本的写入（含经 helper / 基类 / 元组赋值的间接写入），并为每个“控件 Token 类 × 被改写全局 Token”列出在该控件作用域内的全部读取者，给出 A/B/C 去向，供删除 `EvaluateGlobal` 时逐项改用 Own Token 表达。
方法：逐个阅读阶段类与其调用链得到改写清单；读取者分四类检索——Token 类 `CalculateTokenValues`（含基类）对 `EffectiveGlobalToken.X` 的读取；AXAML 中固定 identity 的 `{atom:<Control>TokenResource X}`（含元素语法 `Kind=`）；按 owner 解析的 `OwnerTokenResource` 与 `atom:TokenTransitions.Duration`（按下文 owner 规则展开控件模板、弹层与 BasedOn 链）；C# 中的 `ControlTokenResourceKey.Global(...)`、`OwnerTokenSet.SetKind`、`ControlTokenAccessor.GetEffectiveGlobal`、`ThemeTokenResolver.GetEffectiveGlobal`。此外单独标注阶段类自身 `EvaluateOwn` 对被改写值的读取（它读到的是改写后的 effective global，删除写入后必须由语言自行重算，否则像素会变）。

## 0. 约定与 owner 解析规则（决定哪些读取算“本控件作用域”）

- 路径：未写前缀的路径均相对 `src/AtomUI.Desktop.Controls/`；`AtomUI.Controls/…` 相对 `src/`；阶段类路径 `Shadcn/…`、`PrimeNG/…` 相对 `src/AtomUI.Themes.`。行号为扫描时刻工作树的行号。
- `SharedTokenResource` 永远读全局，不计入。`<Control>TokenResource X` 固定读该 identity 的 Effective Global，无论出现在哪个模板（例如 `FloatButtonItemsControlTheme`、`WindowMessageManagerTheme` 里的读取也计入对应控件）。
- `OwnerTokenResource X`、`TokenTransitions.Duration="X"`（标注 TT）、C# `OwnerTokenSet.SetKind(…, X)` 读“最近 owner”的 Effective Global。owner 由 `ThemeManager.EnsureTokenOwnerSetter` 注入：只有**按类型键解析的默认 ControlTheme** 带 `^:is(OfType(T))` 的 `ThemeScope.TokenOwner` Setter。因此：
  - 已注册且使用默认主题的控件是屏障（自身成为 owner），其内部读取不属于外层控件；`[TokenOwnerTransparent]` 的注册控件（Popup、CandidateList(Item)、ListBoxItem、ListViewItem、TreeViewItem、TabItem、TabStripItem、CollapseItem、TimeViewCell、Skeleton、UploadTrigger 等）不设 owner，内容继承宿主；
  - `StyleKeyOverride` 指向已注册类型的部件同样成为屏障：`AutoCompleteLineEditBox`→LineEdit、`AutoCompleteSearchEditBox`→SearchEdit、`QuickJumpEdit`→LineEdit、`DialogButton`→Button。故 AutoComplete / AutoCompleteSearchEdit 的输入框框架读的是 LineEdit / SearchEdit 的全局，而非 AutoComplete；
  - 显式转发：`InfoPickerInput`→PickerPresenter（DatePicker/TimePicker 弹层）、`DatePickerPresenter`→DateViewer、`TimeView`→各列、`Dialog`→`DialogSurface`（`TokenOwnerForwarding.Forward`）；SearchEdit 的搜索按钮由 `SearchButtonTheme.cs` 显式设 owner=Button。
- “共享基元”：被多个公开控件模板复用、只能用 `OwnerTokenResource` 读全局的部件主题（下表“读取者束”）。它们没有 Own Token 入口，B 类改造时无法直接“改读新 Own Token”，见不确定项第 2 条。

### 0.1 读取者束（Owner 作用域的共享基元，Table 2 中以 `[束名]` 引用）

| 束 | 文件（Owner 读取） | 按全局 Token 的行号 |
| --- | --- | --- |
| `FRAME` | Primitives/AddOnDecoratedBox/Themes/InputControlFrameTheme.axaml（Owner）、Primitives/InputControlFrame.cs `UpdateFrameTokenKeys`（OwnerTokenSet.SetKind，按 StyleVariant/Status 选键） | `BorderRadiusLG` InputControlFrameTheme.axaml:42；`BorderRadius` InputControlFrameTheme.axaml:45；`BorderRadiusSM` InputControlFrameTheme.axaml:49；`ColorError` InputControlFrameTheme.axaml:104；`ColorWarning` InputControlFrameTheme.axaml:109；`ColorFillTertiary` InputControlFrameTheme.axaml:114,115, InputControlFrame.cs:599；`ColorErrorBg` InputControlFrameTheme.axaml:120, InputControlFrame.cs:599；`ColorWarningBg` InputControlFrameTheme.axaml:125；`ColorTextDisabled` InputControlFrameTheme.axaml:139；`ColorBorder` InputControlFrameTheme.axaml:145,157,161, InputControlFrame.cs:588,608；`ColorBgContainer` InputControlFrame.cs:590；`ColorPrimaryHover` InputControlFrame.cs:591,610；`ColorErrorBorderHover` InputControlFrame.cs:591,610；`ColorPrimary` InputControlFrame.cs:593,604,612；`InputActiveShadow` InputControlFrame.cs:595；`InputErrorActiveShadow` InputControlFrame.cs:596；`InputWarningActiveShadow` InputControlFrame.cs:596；`ColorFillSecondary` InputControlFrame.cs:602 |
| `ADDON` | Primitives/AddOnDecoratedBox/Themes/AddOnDecoratedBoxTheme.axaml（Owner）、Primitives/AddOnDecoratedBox/AddOnDecoratedBox.cs `UpdateStatusPaletteTokenSubscription`（SetKind） | `ColorFillAlter` AddOnDecoratedBoxTheme.axaml:111,116；`ColorBorder` AddOnDecoratedBoxTheme.axaml:112,117；`ColorTextQuaternary` AddOnDecoratedBoxTheme.axaml:128；`InputPaddingSM` AddOnDecoratedBoxTheme.axaml:181,184,187；`ColorTextDisabled` AddOnDecoratedBox.cs:369 |
| `CLEAR` | Input/Themes/InputClearIconButtonTheme.axaml（Owner；InputClearIconButton 未注册，BasedOn 新实例 IconButtonTheme） | `ColorTextQuaternary` InputClearIconButtonTheme.axaml:22 |
| `REVEAL` | Input/Themes/RevealButtonTheme.axaml（Owner） | `ColorIcon` RevealButtonTheme.axaml:18；`ColorIconHover` RevealButtonTheme.axaml:22；`ColorTextDisabled` RevealButtonTheme.axaml:32 |
| `TEXTBOX` | Input/Themes/TextBoxTheme.axaml（Owner；经 EmbeddedTextBoxTheme / SelectFilterTextBoxTheme / InfoPickerTextBoxTheme 的 BasedOn 进入；其模板另含 FRAME、CLEAR、REVEAL） | `BorderRadiusLG` TextBoxTheme.axaml:157,178；`BorderRadius` TextBoxTheme.axaml:162,172,182；`BorderRadiusSM` TextBoxTheme.axaml:167,186 |
| `PICKER` | Primitives/InfoPickerInput/Themes/InfoPickerInputTheme.axaml、PickerClearUpButtonTheme.axaml（Owner；后者 BasedOn InputClearIconButtonTheme，另含 CLEAR） | `ColorTextDisabled` InfoPickerInputTheme.axaml:133；`ColorTextQuaternary` PickerClearUpButtonTheme.axaml:40 |
| `RPICKER` | Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.axaml（Owner；BasedOn InfoPickerInputTheme，另含 PICKER） | `ColorIcon` RangeInfoPickerInputTheme.axaml:165；`ColorTextDisabled` RangeInfoPickerInputTheme.axaml:167；`ColorPrimary` RangeInfoPickerInputTheme.axaml:177 |
| `DATECELL` | DateViewer/Themes/DateViewerCellTheme.axaml（Owner；弹层由 InfoPickerInput→DatePickerPresenter→DateViewer 经 TokenOwnerForwarding 转发 owner） | `BorderRadiusSM` DateViewerCellTheme.axaml:47；`ColorPrimary` DateViewerCellTheme.axaml:114,142,239,245；`ColorTextDisabled` DateViewerCellTheme.axaml:168,171,181 |
| `TIMECELL` | TimePicker/Themes/TimeViewCellTheme.axaml（Owner；TimeViewCell 为 TokenOwnerTransparent，TimeView 把 owner 转发给各列） | `BorderRadiusSM` TimeViewCellTheme.axaml:50；`ColorTextDisabled` TimeViewCellTheme.axaml:73 |
| `TEXTAREA` | Input/Themes/TextAreaTheme.axaml（Owner；TextArea 默认主题，亦经 MentionTextAreaTheme / AutoCompleteTextAreaBoxTheme 的 BasedOn 进入） | `ColorTextDisabled` TextAreaTheme.axaml:239 |
| `CANDIDATE` | Primitives/CandidateList/Themes/CandidateListTheme.axaml、CandidateListItemTheme.axaml、ListBox/Themes/ListBoxTheme.axaml（Owner；CandidateList/CandidateListItem 为 TokenOwnerTransparent，BasedOn ListBoxTheme） | `ColorPrimary` CandidateListTheme.axaml:9；`BorderRadiusLG` ListBoxTheme.axaml:76；`BorderRadius` ListBoxTheme.axaml:81；`BorderRadiusSM` ListBoxTheme.axaml:86；`ColorTextDisabled` CandidateListItemTheme.axaml:43 |
| `SELECTLIST` | ListView/Themes/ListViewTheme.axaml、GroupHeaderItemTheme.axaml（Owner；Select 弹层 SelectCandidateList : ListView 为未注册子类，主题 BasedOn ListViewTheme；GroupHeaderItem 仅分组选项时出现） | `BorderRadiusLG` ListViewTheme.axaml:111；`BorderRadius` ListViewTheme.axaml:116；`BorderRadiusSM` ListViewTheme.axaml:121；`ColorFillAlter` GroupHeaderItemTheme.axaml:14 |
| `TAGBOX` | Select/Themes/SelectTagAwareTextBoxTheme.axaml（Owner） | `ColorTextDisabled` SelectTagAwareTextBoxTheme.axaml:31 |
| `TREEPOPUP` | TreeView/Themes/TreeViewItemHeaderTheme.axaml、NodeSwitcherButtonTheme.axaml（Owner；TreeSelect 弹层 TreeSelectTreeView 为未注册子类，TreeViewItem 透明） | `ColorTextDisabled` TreeViewItemHeaderTheme.axaml:236,240, NodeSwitcherButtonTheme.axaml:49 |
| `SPINNER` | ButtonSpinner/Themes/ButtonSpinnerTheme.axaml（Owner；NumericUpDownSpinner 主题 BasedOn 已解析的 ButtonSpinner 默认主题，注入的 OfType(ButtonSpinner) 不匹配子类） | `BorderRadiusLG` ButtonSpinnerTheme.axaml:76；`BorderRadius` ButtonSpinnerTheme.axaml:79；`BorderRadiusSM` ButtonSpinnerTheme.axaml:82 |

各输入类控件包含的束：LineEdit = FRAME+ADDON+CLEAR+REVEAL；TextArea = FRAME+ADDON+CLEAR+TEXTAREA；SearchEdit = FRAME+ADDON+CLEAR+REVEAL；Select = FRAME+ADDON+CLEAR+REVEAL+TEXTBOX+SELECTLIST；Cascader = FRAME+ADDON+CLEAR+REVEAL+TEXTBOX+TAGBOX（弹层 CascaderView 为屏障）；TreeSelect = 同 Cascader + TREEPOPUP；DatePicker = FRAME+ADDON+CLEAR+REVEAL+TEXTBOX+PICKER+DATECELL+TIMECELL；RangeDatePicker = 同 DatePicker + RPICKER；TimePicker = FRAME+ADDON+CLEAR+REVEAL+TEXTBOX+PICKER+TIMECELL；RangeTimePicker = 同 TimePicker + RPICKER；AutoComplete / AutoCompleteSearchEdit = CANDIDATE（输入框为 LineEdit/SearchEdit 屏障）；AutoCompleteTextArea / Mentions = FRAME+ADDON+CLEAR+TEXTAREA+CANDIDATE；NumericUpDown = FRAME+ADDON+CLEAR+REVEAL+TEXTBOX+SPINNER；OtpLineEdit = FRAME（OtpLineEditCell BasedOn InputControlFrameTheme）+CLEAR。

## 1. 阶段类与算法

- Shadcn：所有含写入的阶段类都声明 Shadcn 全部 16 个（Nova/Vega/Maia/Lyra/Mira/Luma/Sera/Rhea × 亮/暗）。（`ShadcnCalendarTokens`/`ShadcnLunarCalendarTokens`/`ShadcnComboBoxTokens`/`ShadcnDescriptionsTokens`/`ShadcnTreeViewTokens` 只声明 8 个暗色 id，`ShadcnOtpLineEditTokens` 只声明 Luma/Luma.Dark，但它们的 `EvaluateGlobal` 为空。）
- PrimeNG：85 个阶段类全部声明 PrimeNG Aura/Lara/Nora 亮+暗（6 个）。
- `EvaluateGlobal` 为空、无需处理的阶段类：Shadcn 的 Calendar、LunarCalendar、ComboBox、Descriptions、Drawer、ProgressBar、RadioButton、Segmented、Splitter、Steps、ToolTip、TreeView、OtpLineEdit、Menu、ContextMenu、MenuFlyoutPresenter、NavMenu、Popup、FlyoutHost、ArrowDecoratedBox、RangeTimePicker；PrimeNG 的 Calendar、LunarCalendar、DateViewer、RangeDateViewer、AdornerLayer、ButtonSpinner、Slider、Rate、ListBox、ComboBox、Breadcrumb、Splitter、CountBadge、DotBadge、AvatarGroup、Timeline、ListView、TransferListView、TransferTreeView、Menu、MenuFlyoutPresenter、MenuItemGroup、ArrowDecoratedBox、FloatButton、OptionButton、ProgressBar/StepsProgressBar/CircleProgress/DashboardProgress（基类 `PrimeNGProgressTokens<TToken>`）、RadioButton、Spin、SplitButton、Steps、Tour。
- 泛型基类展开：`ShadcnButtonFamilyTokens<TToken>` → ShadcnButtonTokens(ButtonToken，另 override 追加暗色两项) / ShadcnDropdownButtonTokens(DropdownButtonToken) / ShadcnSubmitButtonTokens(SubmitButtonToken) / ShadcnResetButtonTokens(ResetButtonToken)；`ShadcnCardTabTokens<TToken>` → ShadcnCardTabControlTokens(CardTabControlToken) / ShadcnCardTabStripTokens(CardTabStripToken)；`PrimeNGButtonFamilyTokens<TToken>` → PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（同四个 Token 类）。

## 2. 改写清单（Table 1）

一行对应一次写入：元组赋值按分量拆成多行；共享 helper 中的一处写入作用于多个阶段类时在同一行列出所有阶段类与 Token 类；`PrimeNGMotion.cs:48/52` 这类由不同阶段类以不同参数调用的 helper 写入，按调用方分行。

| 语言 | 阶段类 | 控件 Token 类 | 被改写的全局 Token | 新值表达式 | 条件 | 源位置 |
| --- | --- | --- | --- | --- | --- | --- |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `BorderRadius` | `new CornerRadius(style.ButtonRadius)` | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:16` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `BorderRadiusLG` | `style.ButtonRadiusLG` | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:17` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `BorderRadiusSM` | `style.ButtonRadiusSM` | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:18` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorFocusBorder` | `controlGlobal.ColorControlOutline` | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:19` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `LineWidthFocus` | `controlGlobal.ControlOutlineWidth` | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:20` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `EnableWaveSpirit` | `false` | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:21` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal）（经 ShadcnButtonTokens.cs:22 `ShadcnButtonAppearance.ApplyGlobal`） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ControlItemColorActive` | 有彩色主题 ? zinc 50/900 : `surface.SecondaryForeground` | 无（值随 ShadcnThemeColor 分支） | `Shadcn/Algorithms/Controls/ShadcnButtonAppearance.cs:21` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal）（经 ShadcnButtonTokens.cs:22 `ShadcnButtonAppearance.ApplyGlobal`） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorFillTertiary` | `secondary`（彩色主题 zinc 800/100，否则 surface.Secondary） | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonAppearance.cs:24` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal）（经 ShadcnButtonTokens.cs:22 `ShadcnButtonAppearance.ApplyGlobal`） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorFillSecondary` | `MixSecondaryHover(secondary, ColorText)` | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonAppearance.cs:25` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal）（经 ShadcnButtonTokens.cs:22 `ShadcnButtonAppearance.ApplyGlobal`） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorErrorBg` | `ColorError` × α(暗 0.2 / 亮 0.1) | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonAppearance.cs:27` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal）（经 ShadcnButtonTokens.cs:22 `ShadcnButtonAppearance.ApplyGlobal`） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorErrorBgHover` | `ColorError` × α(暗 0.3 / 亮 0.2) | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonAppearance.cs:28` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal）（经 ShadcnButtonTokens.cs:22 `ShadcnButtonAppearance.ApplyGlobal`） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorLink` | `ColorPrimary` | `!context.HasConfiguredValue(SharedTokenKind.ColorLink)` | `Shadcn/Algorithms/Controls/ShadcnButtonAppearance.cs:32` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal）（经 ShadcnButtonTokens.cs:22 `ShadcnButtonAppearance.ApplyGlobal`） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorLinkHover` | `ColorPrimary` | `!context.HasConfiguredValue(SharedTokenKind.ColorLink)` | `Shadcn/Algorithms/Controls/ShadcnButtonAppearance.cs:33` |
| Shadcn | ShadcnButtonTokens / ShadcnDropdownButtonTokens / ShadcnSubmitButtonTokens / ShadcnResetButtonTokens（基类 ShadcnButtonFamilyTokens<TToken>.EvaluateGlobal）（经 ShadcnButtonTokens.cs:22 `ShadcnButtonAppearance.ApplyGlobal`） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorLinkActive` | `ColorPrimary` | `!context.HasConfiguredValue(SharedTokenKind.ColorLink)` | `Shadcn/Algorithms/Controls/ShadcnButtonAppearance.cs:34` |
| Shadcn | ShadcnButtonTokens（override EvaluateGlobal） | ButtonToken | `ColorErrorBorderHover` | `ColorErrorHover` | 暗色 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:74` |
| Shadcn | ShadcnButtonTokens（override EvaluateGlobal） | ButtonToken | `ColorErrorActive` | `CompositeOver(ColorError, 0.90, ColorBgContainer)` | 暗色 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:75` |
| Shadcn | ShadcnSplitButtonTokens | SplitButtonToken | `BorderRadius` | `style.ButtonRadius` | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:156` |
| Shadcn | ShadcnSplitButtonTokens | SplitButtonToken | `BorderRadiusLG` | `style.ButtonRadiusLG` | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:157` |
| Shadcn | ShadcnSplitButtonTokens | SplitButtonToken | `BorderRadiusSM` | `style.ButtonRadiusSM` | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:158` |
| Shadcn | ShadcnSplitButtonTokens | SplitButtonToken | `EnableWaveSpirit` | `false` | 无 | `Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs:159` |
| Shadcn | ShadcnCardTabControlTokens / ShadcnCardTabStripTokens（基类 ShadcnCardTabTokens<TToken>） | CardTabControlToken / CardTabStripToken | `BorderRadiusSM` | `LimitRadius(BorderRadiusSM)`（各角 ≤ 8） | 无 | `Shadcn/Algorithms/Controls/ShadcnCardTabTokens.cs:15` |
| Shadcn | ShadcnCardTabControlTokens / ShadcnCardTabStripTokens（基类 ShadcnCardTabTokens<TToken>） | CardTabControlToken / CardTabStripToken | `BorderRadius` | `LimitRadius(BorderRadius)`（各角 ≤ 8） | 无 | `Shadcn/Algorithms/Controls/ShadcnCardTabTokens.cs:16` |
| Shadcn | ShadcnCardTabControlTokens / ShadcnCardTabStripTokens（基类 ShadcnCardTabTokens<TToken>） | CardTabControlToken / CardTabStripToken | `BorderRadiusLG` | `LimitRadius(BorderRadiusLG)`（各角 ≤ 8） | 无 | `Shadcn/Algorithms/Controls/ShadcnCardTabTokens.cs:17` |
| Shadcn | ShadcnCardTokens | CardToken | `ColorBgContainer` | `surface.Card` | 无 | `Shadcn/Algorithms/Controls/ShadcnCardTokens.cs:36` |
| Shadcn | ShadcnCheckBoxTokens | CheckBoxToken | `BorderRadiusSM` | `style.CheckBoxRadius` | 无 | `Shadcn/Algorithms/Controls/ShadcnCheckBoxTokens.cs:38` |
| Shadcn | ShadcnCheckBoxTokens | CheckBoxToken | `ColorWhite` | `ColorTextLightSolid` | 暗色 | `Shadcn/Algorithms/Controls/ShadcnCheckBoxTokens.cs:43` |
| Shadcn | ShadcnDialogTokens | DialogToken | `BoxShadows` | `RingThenShadow(OutlineRing, style.Overlay.DialogShadow)` | 无 | `Shadcn/Algorithms/Controls/ShadcnDialogTokens.cs:39` |
| Shadcn | ShadcnEmptyTokens | EmptyToken | `ColorFill` | `CompositeOver(Foreground, 0.15, Background)` | 无 | `Shadcn/Algorithms/Controls/ShadcnEmptyTokens.cs:42` |
| Shadcn | ShadcnEmptyTokens | EmptyToken | `ColorFillSecondary` | `CompositeOver(Foreground, 0.06, Background)` | 无 | `Shadcn/Algorithms/Controls/ShadcnEmptyTokens.cs:43` |
| Shadcn | ShadcnEmptyTokens | EmptyToken | `ColorFillTertiary` | `CompositeOver(Foreground, 0.04, Background)` | 无 | `Shadcn/Algorithms/Controls/ShadcnEmptyTokens.cs:44` |
| Shadcn | ShadcnEmptyTokens | EmptyToken | `ColorFillQuaternary` | `CompositeOver(Foreground, 0.02, Background)` | 无 | `Shadcn/Algorithms/Controls/ShadcnEmptyTokens.cs:45` |
| Shadcn | ShadcnEmptyTokens | EmptyToken | `ColorBorderSecondary` | `CompositeOver(Foreground, 0.06, Background)` | 无 | `Shadcn/Algorithms/Controls/ShadcnEmptyTokens.cs:46` |
| Shadcn | ShadcnFloatButtonTokens | FloatButtonToken | `ColorBorder` | `ResolveOutline(controlGlobal).Border` | 无（值随 style/暗色分支） | `Shadcn/Algorithms/Controls/ShadcnFloatButtonTokens.cs:29` |
| Shadcn | ShadcnFloatButtonTokens | FloatButtonToken | `BorderRadius` | `min(style.ButtonRadiusLG, ControlHeightLG/3)` | 无 | `Shadcn/Algorithms/Controls/ShadcnFloatButtonTokens.cs:30` |
| Shadcn | ShadcnFloatButtonTokens | FloatButtonToken | `BorderRadiusLG` | `style.ButtonRadiusLG` | 无 | `Shadcn/Algorithms/Controls/ShadcnFloatButtonTokens.cs:31` |
| Shadcn | ShadcnFloatButtonTokens | FloatButtonToken | `PaddingLG` | `new Thickness(min(style.PaddingXSM,10), 2)` | 无 | `Shadcn/Algorithms/Controls/ShadcnFloatButtonTokens.cs:32` |
| Shadcn | ShadcnFloatButtonTokens | FloatButtonToken | `SpacingXS` | Mira/Sera ? 4 : 6 | 无 | `Shadcn/Algorithms/Controls/ShadcnFloatButtonTokens.cs:33` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（经 ApplyFieldBorder:19）/ ShadcnCascaderTokens / ShadcnDatePickerTokens / ShadcnTimePickerTokens / ShadcnAutoCompleteTokens / ShadcnMentionsTokens（ShadcnInputStage.Apply） | LineEditToken / TextAreaToken / SelectToken / CascaderToken / DatePickerToken / TimePickerToken / AutoCompleteToken / MentionsToken | `ColorPrimary` | `surface.Ring` | 无 | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:49` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（经 ApplyFieldBorder:19）/ ShadcnCascaderTokens / ShadcnDatePickerTokens / ShadcnTimePickerTokens / ShadcnAutoCompleteTokens / ShadcnMentionsTokens（ShadcnInputStage.Apply） | LineEditToken / TextAreaToken / SelectToken / CascaderToken / DatePickerToken / TimePickerToken / AutoCompleteToken / MentionsToken | `ColorPrimaryHover` | `surface.Ring` | 无（LineEdit/TextArea/Select 在 Sera 下随后被 :23 覆盖） | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:50` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（经 ApplyFieldBorder:19）/ ShadcnCascaderTokens / ShadcnDatePickerTokens / ShadcnTimePickerTokens / ShadcnAutoCompleteTokens / ShadcnMentionsTokens（ShadcnInputStage.Apply） | LineEditToken / TextAreaToken / SelectToken / CascaderToken / DatePickerToken / TimePickerToken / AutoCompleteToken / MentionsToken | `ColorBorder` | `surface.Input` | 无 | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:51` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（经 ApplyFieldBorder:19）/ ShadcnCascaderTokens / ShadcnDatePickerTokens / ShadcnTimePickerTokens / ShadcnAutoCompleteTokens / ShadcnMentionsTokens（ShadcnInputStage.Apply） | LineEditToken / TextAreaToken / SelectToken / CascaderToken / DatePickerToken / TimePickerToken / AutoCompleteToken / MentionsToken | `ColorBgContainer` | 暗色且 DarkInputFill>0 ? Input×DarkInputFill : Transparent | 无（LineEdit/TextArea/Select 在 Luma/Rhea 下随后被 :31 覆盖） | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:54` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（经 ApplyFieldBorder:19）/ ShadcnCascaderTokens / ShadcnDatePickerTokens / ShadcnTimePickerTokens / ShadcnAutoCompleteTokens / ShadcnMentionsTokens（ShadcnInputStage.Apply） | LineEditToken / TextAreaToken / SelectToken / CascaderToken / DatePickerToken / TimePickerToken / AutoCompleteToken / MentionsToken | `BorderRadius` | `style.InputRadius` | 无 | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:59` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（经 ApplyFieldBorder:19）/ ShadcnCascaderTokens / ShadcnDatePickerTokens / ShadcnTimePickerTokens / ShadcnAutoCompleteTokens / ShadcnMentionsTokens（ShadcnInputStage.Apply） | LineEditToken / TextAreaToken / SelectToken / CascaderToken / DatePickerToken / TimePickerToken / AutoCompleteToken / MentionsToken | `BorderRadiusLG` | `style.InputRadius` | 无 | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:60` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（经 ApplyFieldBorder:19）/ ShadcnCascaderTokens / ShadcnDatePickerTokens / ShadcnTimePickerTokens / ShadcnAutoCompleteTokens / ShadcnMentionsTokens（ShadcnInputStage.Apply） | LineEditToken / TextAreaToken / SelectToken / CascaderToken / DatePickerToken / TimePickerToken / AutoCompleteToken / MentionsToken | `BorderRadiusSM` | `style.InputRadius` | 无 | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:61` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（ShadcnInputStage.ApplyFieldBorder） | LineEditToken / TextAreaToken / SelectToken | `ColorPrimaryHover` | `ColorBorder`（即 surface.Input） | style == Sera | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:23` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（ShadcnInputStage.ApplyFieldBorder） | LineEditToken / TextAreaToken / SelectToken | `InputActiveShadow` | `default` | style == Sera | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:24` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（ShadcnInputStage.ApplyFieldBorder） | LineEditToken / TextAreaToken / SelectToken | `InputErrorActiveShadow` | `default` | style == Sera | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:25` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（ShadcnInputStage.ApplyFieldBorder） | LineEditToken / TextAreaToken / SelectToken | `InputWarningActiveShadow` | `default` | style == Sera | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:26` |
| Shadcn | ShadcnLineEditTokens / ShadcnTextAreaTokens / ShadcnSelectTokens（ShadcnInputStage.ApplyFieldBorder） | LineEditToken / TextAreaToken / SelectToken | `ColorBgContainer` | `ColorBorder` 半透明（A/2） | style ∈ {Luma, Rhea} | `Shadcn/Algorithms/Controls/ShadcnInputTokens.cs:31` |
| Shadcn | ShadcnSimplePaginationTokens | SimplePaginationToken | `InputPaddingSM` | InputPaddingSM 上下各减 ControlOutlineWidth（≥0） | 无 | `Shadcn/Algorithms/Controls/ShadcnSimplePaginationTokens.cs:32` |
| Shadcn | ShadcnToggleSwitchTokens | ToggleSwitchToken | `ColorPrimary` | `CompositeOver(Primary, 0.45, Background)` | 暗色且 `ColorPrimary == surface.Primary`（中性主色） | `Shadcn/Algorithms/Controls/ShadcnToggleSwitchTokens.cs:40` |
| Shadcn | ShadcnToggleSwitchTokens | ToggleSwitchToken | `ColorPrimaryHover` | `CompositeOver(Primary, 0.48, Background)` | 暗色且 `ColorPrimary == surface.Primary`（中性主色） | `Shadcn/Algorithms/Controls/ShadcnToggleSwitchTokens.cs:41` |
| Shadcn | ShadcnToggleSwitchTokens | ToggleSwitchToken | `ColorTextLightSolid` | `surface.Foreground` | 暗色且 `ColorPrimary == surface.Primary`（中性主色） | `Shadcn/Algorithms/Controls/ShadcnToggleSwitchTokens.cs:42` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorLink` | `ColorPrimary` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:37` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorLinkHover` | `ColorPrimary` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:38` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorLinkActive` | `ColorPrimary` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:39` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorError` | `DangerFills(...)` 元组（error 调色板 500/600/700；暗 400/300/200；Nora 600/700/800、暗 500/400/300） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:41` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorErrorHover` | `DangerFills(...)` 元组（error 调色板 500/600/700；暗 400/300/200；Nora 600/700/800、暗 500/400/300） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:41` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorErrorActive` | `DangerFills(...)` 元组（error 调色板 500/600/700；暗 400/300/200；Nora 600/700/800、暗 500/400/300） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:41` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorBgSolid` | Surface(暗 0 / 亮 950) | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:44` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorBgSolidHover` | Surface(暗 100 / 亮 900) | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:45` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `ColorBgSolidActive` | Surface(暗 200 / 亮 800) | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:46` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `BorderRadiusSM` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:48` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `BorderRadiusLG` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:49` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `EnableWaveSpirit` | `false` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:50` |
| PrimeNG | PrimeNGButtonTokens / PrimeNGDropdownButtonTokens / PrimeNGSubmitButtonTokens / PrimeNGResetButtonTokens（基类 PrimeNGButtonFamilyTokens<TToken>.EvaluateGlobal） | ButtonToken / DropdownButtonToken / SubmitButtonToken / ResetButtonToken | `OpacityLoading` | `0.6` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs:51` |
| PrimeNG | PrimeNGCardTokens | CardToken | `ColorBgContainer` | `ColorBgElevated` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGCardTokens.cs:27` |
| PrimeNG | PrimeNGCardTokens | CardToken | `BorderRadiusLG` | `BorderRadius.TopLeft + CardBorderRadiusOffset` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGCardTokens.cs:28` |
| PrimeNG | PrimeNGCardTokens | CardToken | `ColorBorderSecondary` | `rules.CardBorderSecondary.Resolve(surface)` | `rules.CardBorderSecondary` 非空（仅 Nora.Dark：neutral 700） | `PrimeNG/Algorithms/Controls/PrimeNGCardTokens.cs:33` |
| PrimeNG | PrimeNGCheckBoxTokens | CheckBoxToken | `ColorWhite` | `ColorTextLightSolid` | `PrimeNGNoir.IsActive`（noir 主色） | `PrimeNG/Algorithms/Controls/PrimeNGCheckBoxTokens.cs:29` |
| PrimeNG | PrimeNGNumericUpDownTokens（PrimeNGButtonSpinnerTokens.cs:51）、PrimeNGOtpLineEditTokens（PrimeNGChoiceTokens.cs:117）、PrimeNGSelectTokens（PrimeNGSelectTokens.cs:25）、PrimeNGInputTokens.cs 中 LineEdit:105 / SearchEdit:131 / TextArea:154 / AutoComplete:175 / AutoCompleteSearchEdit:200 / AutoCompleteTextArea:224 / Mentions:248 / Cascader:272 / TreeSelect:311 / DatePicker:344 / RangeDatePicker:371 / TimePicker:400 / RangeTimePicker:423（均调用 PrimeNGInputStage.Apply） | NumericUpDownToken / OtpLineEditToken / SelectToken / LineEditToken / SearchEditToken / TextAreaToken / AutoCompleteToken / AutoCompleteSearchEditToken / AutoCompleteTextAreaToken / MentionsToken / CascaderToken / TreeSelectToken / DatePickerToken / RangeDatePickerToken / TimePickerToken / RangeTimePickerToken | `InputActiveShadow` | `BoxShadow{Spread=LineWidth, Color=ColorPrimary}` | Nora 预设 | `PrimeNG/Algorithms/Controls/PrimeNGInputTokens.cs:52` |
| PrimeNG | PrimeNGNumericUpDownTokens（PrimeNGButtonSpinnerTokens.cs:51）、PrimeNGOtpLineEditTokens（PrimeNGChoiceTokens.cs:117）、PrimeNGSelectTokens（PrimeNGSelectTokens.cs:25）、PrimeNGInputTokens.cs 中 LineEdit:105 / SearchEdit:131 / TextArea:154 / AutoComplete:175 / AutoCompleteSearchEdit:200 / AutoCompleteTextArea:224 / Mentions:248 / Cascader:272 / TreeSelect:311 / DatePicker:344 / RangeDatePicker:371 / TimePicker:400 / RangeTimePicker:423（均调用 PrimeNGInputStage.Apply） | NumericUpDownToken / OtpLineEditToken / SelectToken / LineEditToken / SearchEditToken / TextAreaToken / AutoCompleteToken / AutoCompleteSearchEditToken / AutoCompleteTextAreaToken / MentionsToken / CascaderToken / TreeSelectToken / DatePickerToken / RangeDatePickerToken / TimePickerToken / RangeTimePickerToken | `InputErrorActiveShadow` | 同上 ring | Nora 预设 | `PrimeNG/Algorithms/Controls/PrimeNGInputTokens.cs:53` |
| PrimeNG | PrimeNGNumericUpDownTokens（PrimeNGButtonSpinnerTokens.cs:51）、PrimeNGOtpLineEditTokens（PrimeNGChoiceTokens.cs:117）、PrimeNGSelectTokens（PrimeNGSelectTokens.cs:25）、PrimeNGInputTokens.cs 中 LineEdit:105 / SearchEdit:131 / TextArea:154 / AutoComplete:175 / AutoCompleteSearchEdit:200 / AutoCompleteTextArea:224 / Mentions:248 / Cascader:272 / TreeSelect:311 / DatePicker:344 / RangeDatePicker:371 / TimePicker:400 / RangeTimePicker:423（均调用 PrimeNGInputStage.Apply） | NumericUpDownToken / OtpLineEditToken / SelectToken / LineEditToken / SearchEditToken / TextAreaToken / AutoCompleteToken / AutoCompleteSearchEditToken / AutoCompleteTextAreaToken / MentionsToken / CascaderToken / TreeSelectToken / DatePickerToken / RangeDatePickerToken / TimePickerToken / RangeTimePickerToken | `ColorPrimaryHover` | hoverBorder（Lara: ColorPrimary；Nora: ColorBorder；Aura: surface 暗500/亮400） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGInputTokens.cs:62` |
| PrimeNG | PrimeNGNumericUpDownTokens（PrimeNGButtonSpinnerTokens.cs:51）、PrimeNGOtpLineEditTokens（PrimeNGChoiceTokens.cs:117）、PrimeNGSelectTokens（PrimeNGSelectTokens.cs:25）、PrimeNGInputTokens.cs 中 LineEdit:105 / SearchEdit:131 / TextArea:154 / AutoComplete:175 / AutoCompleteSearchEdit:200 / AutoCompleteTextArea:224 / Mentions:248 / Cascader:272 / TreeSelect:311 / DatePicker:344 / RangeDatePicker:371 / TimePicker:400 / RangeTimePicker:423（均调用 PrimeNGInputStage.Apply） | NumericUpDownToken / OtpLineEditToken / SelectToken / LineEditToken / SearchEditToken / TextAreaToken / AutoCompleteToken / AutoCompleteSearchEditToken / AutoCompleteTextAreaToken / MentionsToken / CascaderToken / TreeSelectToken / DatePickerToken / RangeDatePickerToken / TimePickerToken / RangeTimePickerToken | `ColorErrorBorderHover` | hoverBorder（同上） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGInputTokens.cs:63` |
| PrimeNG | PrimeNGNumericUpDownTokens（PrimeNGButtonSpinnerTokens.cs:51）、PrimeNGOtpLineEditTokens（PrimeNGChoiceTokens.cs:117）、PrimeNGSelectTokens（PrimeNGSelectTokens.cs:25）、PrimeNGInputTokens.cs 中 LineEdit:105 / SearchEdit:131 / TextArea:154 / AutoComplete:175 / AutoCompleteSearchEdit:200 / AutoCompleteTextArea:224 / Mentions:248 / Cascader:272 / TreeSelect:311 / DatePicker:344 / RangeDatePicker:371 / TimePicker:400 / RangeTimePicker:423（均调用 PrimeNGInputStage.Apply） | NumericUpDownToken / OtpLineEditToken / SelectToken / LineEditToken / SearchEditToken / TextAreaToken / AutoCompleteToken / AutoCompleteSearchEditToken / AutoCompleteTextAreaToken / MentionsToken / CascaderToken / TreeSelectToken / DatePickerToken / RangeDatePickerToken / TimePickerToken / RangeTimePickerToken | `ColorTextDisabled` | `ColorTextSecondary` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGInputTokens.cs:64` |
| PrimeNG | PrimeNGNumericUpDownTokens（PrimeNGButtonSpinnerTokens.cs:51）、PrimeNGOtpLineEditTokens（PrimeNGChoiceTokens.cs:117）、PrimeNGSelectTokens（PrimeNGSelectTokens.cs:25）、PrimeNGInputTokens.cs 中 LineEdit:105 / SearchEdit:131 / TextArea:154 / AutoComplete:175 / AutoCompleteSearchEdit:200 / AutoCompleteTextArea:224 / Mentions:248 / Cascader:272 / TreeSelect:311 / DatePicker:344 / RangeDatePicker:371 / TimePicker:400 / RangeTimePicker:423（均调用 PrimeNGInputStage.Apply） | NumericUpDownToken / OtpLineEditToken / SelectToken / LineEditToken / SearchEditToken / TextAreaToken / AutoCompleteToken / AutoCompleteSearchEditToken / AutoCompleteTextAreaToken / MentionsToken / CascaderToken / TreeSelectToken / DatePickerToken / RangeDatePickerToken / TimePickerToken / RangeTimePickerToken | `ColorTextQuaternary` | icon（Lara: surface 暗400/亮500；Nora: 暗 White/亮 surface 900；Aura: surface 400） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGInputTokens.cs:65` |
| PrimeNG | PrimeNGNumericUpDownTokens（PrimeNGButtonSpinnerTokens.cs:51）、PrimeNGOtpLineEditTokens（PrimeNGChoiceTokens.cs:117）、PrimeNGSelectTokens（PrimeNGSelectTokens.cs:25）、PrimeNGInputTokens.cs 中 LineEdit:105 / SearchEdit:131 / TextArea:154 / AutoComplete:175 / AutoCompleteSearchEdit:200 / AutoCompleteTextArea:224 / Mentions:248 / Cascader:272 / TreeSelect:311 / DatePicker:344 / RangeDatePicker:371 / TimePicker:400 / RangeTimePicker:423（均调用 PrimeNGInputStage.Apply） | NumericUpDownToken / OtpLineEditToken / SelectToken / LineEditToken / SearchEditToken / TextAreaToken / AutoCompleteToken / AutoCompleteSearchEditToken / AutoCompleteTextAreaToken / MentionsToken / CascaderToken / TreeSelectToken / DatePickerToken / RangeDatePickerToken / TimePickerToken / RangeTimePickerToken | `ColorIcon` | icon（Lara: surface 暗400/亮500；Nora: 暗 White/亮 surface 900；Aura: surface 400） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGInputTokens.cs:66` |
| PrimeNG | PrimeNGNumericUpDownTokens（PrimeNGButtonSpinnerTokens.cs:51）、PrimeNGOtpLineEditTokens（PrimeNGChoiceTokens.cs:117）、PrimeNGSelectTokens（PrimeNGSelectTokens.cs:25）、PrimeNGInputTokens.cs 中 LineEdit:105 / SearchEdit:131 / TextArea:154 / AutoComplete:175 / AutoCompleteSearchEdit:200 / AutoCompleteTextArea:224 / Mentions:248 / Cascader:272 / TreeSelect:311 / DatePicker:344 / RangeDatePicker:371 / TimePicker:400 / RangeTimePicker:423（均调用 PrimeNGInputStage.Apply） | NumericUpDownToken / OtpLineEditToken / SelectToken / LineEditToken / SearchEditToken / TextAreaToken / AutoCompleteToken / AutoCompleteSearchEditToken / AutoCompleteTextAreaToken / MentionsToken / CascaderToken / TreeSelectToken / DatePickerToken / RangeDatePickerToken / TimePickerToken / RangeTimePickerToken | `ColorIconHover` | icon（Lara: surface 暗400/亮500；Nora: 暗 White/亮 surface 900；Aura: surface 400） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGInputTokens.cs:67` |
| PrimeNG | PrimeNGNumericUpDownTokens（PrimeNGButtonSpinnerTokens.cs:51）、PrimeNGOtpLineEditTokens（PrimeNGChoiceTokens.cs:117）、PrimeNGSelectTokens（PrimeNGSelectTokens.cs:25）、PrimeNGInputTokens.cs 中 LineEdit:105 / SearchEdit:131 / TextArea:154 / AutoComplete:175 / AutoCompleteSearchEdit:200 / AutoCompleteTextArea:224 / Mentions:248 / Cascader:272 / TreeSelect:311 / DatePicker:344 / RangeDatePicker:371 / TimePicker:400 / RangeTimePicker:423（均调用 PrimeNGInputStage.Apply） | NumericUpDownToken / OtpLineEditToken / SelectToken / LineEditToken / SearchEditToken / TextAreaToken / AutoCompleteToken / AutoCompleteSearchEditToken / AutoCompleteTextAreaToken / MentionsToken / CascaderToken / TreeSelectToken / DatePickerToken / RangeDatePickerToken / TimePickerToken / RangeTimePickerToken | `ColorFillAlter` | addOnBackground（Lara: surface 暗800/亮50；其余 ColorBgContainer） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGInputTokens.cs:68` |
| PrimeNG | PrimeNGNumericUpDownTokens（PrimeNGButtonSpinnerTokens.cs:51）、PrimeNGOtpLineEditTokens（PrimeNGChoiceTokens.cs:117）、PrimeNGSelectTokens（PrimeNGSelectTokens.cs:25）、PrimeNGInputTokens.cs 中 LineEdit:105 / SearchEdit:131 / TextArea:154 / AutoComplete:175 / AutoCompleteSearchEdit:200 / AutoCompleteTextArea:224 / Mentions:248 / Cascader:272 / TreeSelect:311 / DatePicker:344 / RangeDatePicker:371 / TimePicker:400 / RangeTimePicker:423（均调用 PrimeNGInputStage.Apply） | NumericUpDownToken / OtpLineEditToken / SelectToken / LineEditToken / SearchEditToken / TextAreaToken / AutoCompleteToken / AutoCompleteSearchEditToken / AutoCompleteTextAreaToken / MentionsToken / CascaderToken / TreeSelectToken / DatePickerToken / RangeDatePickerToken / TimePickerToken / RangeTimePickerToken | `BorderRadiusSM` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGInputTokens.cs:69` |
| PrimeNG | PrimeNGNumericUpDownTokens（PrimeNGButtonSpinnerTokens.cs:51）、PrimeNGOtpLineEditTokens（PrimeNGChoiceTokens.cs:117）、PrimeNGSelectTokens（PrimeNGSelectTokens.cs:25）、PrimeNGInputTokens.cs 中 LineEdit:105 / SearchEdit:131 / TextArea:154 / AutoComplete:175 / AutoCompleteSearchEdit:200 / AutoCompleteTextArea:224 / Mentions:248 / Cascader:272 / TreeSelect:311 / DatePicker:344 / RangeDatePicker:371 / TimePicker:400 / RangeTimePicker:423（均调用 PrimeNGInputStage.Apply） | NumericUpDownToken / OtpLineEditToken / SelectToken / LineEditToken / SearchEditToken / TextAreaToken / AutoCompleteToken / AutoCompleteSearchEditToken / AutoCompleteTextAreaToken / MentionsToken / CascaderToken / TreeSelectToken / DatePickerToken / RangeDatePickerToken / TimePickerToken / RangeTimePickerToken | `BorderRadiusLG` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGInputTokens.cs:70` |
| PrimeNG | PrimeNGTabControlTokens / PrimeNGTabStripTokens（PrimeNGTabStage.ApplyGlobal，PrimeNGContainerTokens.cs:56/74） | TabControlToken / TabStripToken | `MotionDurationSlow` | 250ms（经 PrimeNGContainerTokens.cs:22 `PrimeNGMotion.Restore`） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:52` |
| PrimeNG | PrimeNGTabControlTokens / PrimeNGTabStripTokens（PrimeNGTabStage.ApplyGlobal，PrimeNGContainerTokens.cs:56/74） | TabControlToken / TabStripToken | `ColorBorderSecondary` | Lara ? Transparent : `ContentBorder(context)` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGContainerTokens.cs:23` |
| PrimeNG | PrimeNGCollapseTokens | CollapseToken | `MotionDurationSlow` | 200ms（经 PrimeNGContainerTokens.cs:100） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:52` |
| PrimeNG | PrimeNGCollapseTokens | CollapseToken | `ColorBorder` | `ContentBorder(context)` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGContainerTokens.cs:101` |
| PrimeNG | PrimeNGExpanderTokens | ExpanderToken | `MotionDurationSlow` | 200ms（经 PrimeNGContainerTokens.cs:144） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:52` |
| PrimeNG | PrimeNGExpanderTokens | ExpanderToken | `ColorBorder` | `ContentBorder(context)` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGContainerTokens.cs:145` |
| PrimeNG | PrimeNGGroupBoxTokens | GroupBoxToken | `ColorBorder` | `ContentBorder(context)` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGContainerTokens.cs:181` |
| PrimeNG | PrimeNGGroupBoxTokens | GroupBoxToken | `ColorBgContainer` | `ColorBgElevated` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGContainerTokens.cs:182` |
| PrimeNG | PrimeNGTagTokens | TagToken | `BorderRadiusSM` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:63` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorSuccess` | `Pair(...).Text`（Aura: 暗300/亮700；Lara/Nora: Solid 填充色） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:68` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorSuccessBg` | `Pair(...).Background`（暗 500@16%，亮 100） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:68` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorInfo` | `Pair(...).Text`（Aura: 暗300/亮700；Lara/Nora: Solid 填充色） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:69` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorInfoBg` | `Pair(...).Background`（暗 500@16%，亮 100） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:69` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorWarning` | `Pair(...).Text`（Aura: 暗300/亮700；Lara/Nora: Solid 填充色） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:70` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorWarningBg` | `Pair(...).Background`（暗 500@16%，亮 100） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:70` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorError` | `Pair(...).Text`（Aura: 暗300/亮700；Lara/Nora: Solid 填充色） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:71` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorErrorBg` | `Pair(...).Background`（暗 500@16%，亮 100） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:71` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorSuccessBorder` | `ColorSuccessBg`（新值） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:72` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorInfoBorder` | `ColorInfoBg`（新值） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:73` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorWarningBorder` | `ColorWarningBg`（新值） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:74` |
| PrimeNG | PrimeNGTagTokens | TagToken | `ColorErrorBorder` | `ColorErrorBg`（新值） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:75` |
| PrimeNG | PrimeNGAvatarTokens | AvatarToken | `BorderRadiusSM` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:203` |
| PrimeNG | PrimeNGAvatarTokens | AvatarToken | `BorderRadiusLG` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:204` |
| PrimeNG | PrimeNGSkeletonTokens | SkeletonToken | `BorderRadiusSM` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:254` |
| PrimeNG | PrimeNGSeparatorTokens | SeparatorToken | `ColorSplit` | `ContentBorder(context)` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:296` |
| PrimeNG | PrimeNGTreeViewTokens | TreeViewToken | `MotionDurationSlow` | 200ms（经 PrimeNGDataDisplayTokens.cs:432） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:52` |
| PrimeNG | PrimeNGCarouselTokens | CarouselToken | `MotionDurationSlow` | 500ms（经 PrimeNGDataDisplayTokens.cs:474） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:52` |
| PrimeNG | PrimeNGDescriptionsTokens | DescriptionsToken | `ColorSplit` | `ContentBorder(context)` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:505` |
| PrimeNG | PrimeNGDescriptionsTokens | DescriptionsToken | `ColorBorderSecondary` | `ContentBorder(context)` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:506` |
| PrimeNG | PrimeNGQRCodeTokens | QRCodeToken | `ColorSplit` | `ContentBorder(context)` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:532` |
| PrimeNG | PrimeNGQRCodeTokens | QRCodeToken | `BorderRadiusLG` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:533` |
| PrimeNG | PrimeNGPaginationTokens | PaginationToken | `UniformlyMarginXS` | `4` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:561` |
| PrimeNG | PrimeNGSimplePaginationTokens | SimplePaginationToken | `UniformlyMarginXS` | `4` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDataDisplayTokens.cs:608` |
| PrimeNG | PrimeNGDialogTokens / PrimeNGMessageBoxTokens | DialogToken / MessageBoxToken | `MotionDurationMid` | 300ms（经 PrimeNGDialogTokens.cs:24/89） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:48` |
| PrimeNG | PrimeNGDialogTokens / PrimeNGMessageBoxTokens | DialogToken / MessageBoxToken | `MotionDurationSlow` | 300ms（经 PrimeNGDialogTokens.cs:24/89） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:52` |
| PrimeNG | PrimeNGDialogTokens / PrimeNGMessageBoxTokens（经 PrimeNGDialogTokens.cs:25/90 `PrimeNGModalStage.ApplyGlobal`） | DialogToken / MessageBoxToken | `BorderRadiusLG` | `PrimeNGOverlayRadius.Of(controlGlobal, preset.Sizes)` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDialogTokens.cs:57` |
| PrimeNG | PrimeNGDialogTokens / PrimeNGMessageBoxTokens（经 PrimeNGDialogTokens.cs:25/90 `PrimeNGModalStage.ApplyGlobal`） | DialogToken / MessageBoxToken | `BorderRadiusSM` | `CloseButtonSize/2`（Lara 19，其余 18） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDialogTokens.cs:58` |
| PrimeNG | PrimeNGDialogTokens / PrimeNGMessageBoxTokens（经 PrimeNGDialogTokens.cs:25/90 `PrimeNGModalStage.ApplyGlobal`） | DialogToken / MessageBoxToken | `BoxShadows` | `WithOutlineRing(BoxShadows, OverlayBorder, LineWidth)` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDialogTokens.cs:59` |
| PrimeNG | PrimeNGDialogTokens / PrimeNGMessageBoxTokens（经 PrimeNGDialogTokens.cs:25/90 `PrimeNGModalStage.ApplyGlobal`） | DialogToken / MessageBoxToken | `FontWeightStrong` | `FontWeight.Bold` | Nora 预设 | `PrimeNG/Algorithms/Controls/PrimeNGDialogTokens.cs:63` |
| PrimeNG | PrimeNGDrawerTokens | DrawerToken | `MotionDurationSlow` | 500ms（经 PrimeNGDialogTokens.cs:127） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:52` |
| PrimeNG | PrimeNGDrawerTokens | DrawerToken | `FontSizeLG` | Lara ? 20 : 18 | 无 | `PrimeNG/Algorithms/Controls/PrimeNGDialogTokens.cs:128` |
| PrimeNG | PrimeNGDrawerTokens | DrawerToken | `FontWeightStrong` | `FontWeight.Bold` | Nora 预设 | `PrimeNG/Algorithms/Controls/PrimeNGDialogTokens.cs:131` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorInfoBg` | Severity(Blue).Background（Nora 填充 800/200；其余 暗500@16%/亮50@95%） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:26` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorInfoBorder` | Severity(Blue).Border（Lara Transparent；Nora=填充；Aura 暗700@36%/亮200） | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:26` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorInfo` | Severity(Blue).Text | 无 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:28` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorPrimary` | Severity(Blue).Text | 无 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:29` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorSuccessBg` | Severity(success 调色板).Background | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:30` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorSuccessBorder` | Severity(success 调色板).Border | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:30` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorSuccess` | Severity(success 调色板).Text | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:30` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorWarningBg` | Severity(Yellow（Nora 亮色填充 600）).Background | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:32` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorWarningBorder` | Severity(Yellow（Nora 亮色填充 600）).Border | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:32` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorWarning` | Severity(Yellow（Nora 亮色填充 600）).Text | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:32` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorErrorBg` | Severity(error 调色板).Background | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:34` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorErrorBorder` | Severity(error 调色板).Border | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:34` |
| PrimeNG | PrimeNGAlertTokens（PrimeNGFeedbackTokens.cs:117）/ PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（经 ApplyForCard:49 → PrimeNGMessageSeverity.Apply） | AlertToken / MessageCardToken / NotificationCardToken | `ColorError` | Severity(error 调色板).Text | 无（值随预设） | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:34` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorInfoText` | `ColorInfo`（Apply 后的新值） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:52` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorSuccessText` | `ColorSuccess`（Apply 后的新值） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:53` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorWarningText` | `ColorWarning`（Apply 后的新值） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:54` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorErrorText` | `ColorError`（Apply 后的新值） | 无 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:55` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorInfoBorder` | StartBar(500；暗 700@36%) | Lara 预设 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:60` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorSuccessBorder` | StartBar(500；暗 700@36%) | Lara 预设 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:61` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorWarningBorder` | StartBar(500；暗 700@36%) | Lara 预设 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:62` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorErrorBorder` | StartBar(500；暗 700@36%) | Lara 预设 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:63` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorInfo` | `ColorInfoBg` | Nora 预设 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:67` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorPrimary` | `ColorInfoBg` | Nora 预设 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:68` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorSuccess` | `ColorSuccessBg` | Nora 预设 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:69` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorWarning` | `ColorWarningBg` | Nora 预设 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:70` |
| PrimeNG | PrimeNGMessageCardTokens / PrimeNGNotificationCardTokens（PrimeNGMessageSeverity.ApplyForCard） | MessageCardToken / NotificationCardToken | `ColorError` | `ColorErrorBg` | Nora 预设 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:71` |
| PrimeNG | PrimeNGAlertTokens | AlertToken | `BorderRadiusLG` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:118` |
| PrimeNG | PrimeNGPopupConfirmTokens | PopupConfirmToken | `MotionDurationMid` | 300ms（经 PrimeNGFeedbackTokens.cs:155） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:48` |
| PrimeNG | PrimeNGPopupConfirmTokens | PopupConfirmToken | `IconSizeLG` | `20` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:156` |
| PrimeNG | PrimeNGUploadTokens | UploadToken | `BorderRadiusLG` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:205` |
| PrimeNG | PrimeNGUploadTokens | UploadToken | `ColorFillAlter` | `ColorBgElevated` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:206` |
| PrimeNG | PrimeNGUploadTokens | UploadToken | `ColorBorder` | `ContentBorder(context)` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGFeedbackTokens.cs:207` |
| PrimeNG | PrimeNGContextMenuTokens | ContextMenuToken | `MotionDurationMid` | 300ms（经 PrimeNGMenuTokens.cs:128） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:48` |
| PrimeNG | PrimeNGNavMenuTokens | NavMenuToken | `MotionDurationMid` | 200ms（经 PrimeNGNavMenuTokens.cs:37） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:48` |
| PrimeNG | PrimeNGFlyoutHostTokens | FlyoutHostToken | `MotionDurationMid` | 300ms（经 PrimeNGOverlayTokens.cs:67） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:48` |
| PrimeNG | PrimeNGPopupTokens | PopupToken | `MotionDurationMid` | 300ms（经 PrimeNGOverlayTokens.cs:100） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:48` |
| PrimeNG | PrimeNGToggleSwitchTokens | ToggleSwitchToken | `MotionDurationMid` | 100ms（经 PrimeNGToggleSwitchTokens.cs:28） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:48` |
| PrimeNG | PrimeNGMessageCardTokens | MessageCardToken | `MotionDurationMid` | 300ms（经 PrimeNGMessageCardTokens.cs:28） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:48` |
| PrimeNG | PrimeNGMessageCardTokens | MessageCardToken | `BorderRadiusLG` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGMessageCardTokens.cs:31` |
| PrimeNG | PrimeNGNotificationCardTokens | NotificationCardToken | `MotionDurationMid` | 300ms（经 PrimeNGNotificationCardTokens.cs:31） | EnableMotion 且预设 TransitionDuration==0（仅 Nora / Nora.Dark） | `PrimeNG/Algorithms/Controls/PrimeNGMotion.cs:48` |
| PrimeNG | PrimeNGNotificationCardTokens | NotificationCardToken | `BorderRadiusLG` | `BorderRadius` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGNotificationCardTokens.cs:35` |
| PrimeNG | PrimeNGNotificationCardTokens | NotificationCardToken | `FontSizeLG` | `FontSize` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGNotificationCardTokens.cs:36` |
| PrimeNG | PrimeNGNotificationCardTokens | NotificationCardToken | `IconSizeSM` | `14` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGNotificationCardTokens.cs:37` |
| PrimeNG | PrimeNGNotificationCardTokens | NotificationCardToken | `BorderRadiusSM` | Lara ? 14 : 12 | 无 | `PrimeNG/Algorithms/Controls/PrimeNGNotificationCardTokens.cs:38` |
| PrimeNG | PrimeNGToolTipTokens | ToolTipToken | `FontSize` | Lara ? 10 : 12 | 无 | `PrimeNG/Algorithms/Controls/PrimeNGToolTipTokens.cs:33` |
| PrimeNG | PrimeNGToolTipTokens | ToolTipToken | `ControlHeight` | `fontSize*1.5 + ToolTipPadding.Top + .Bottom` | 无 | `PrimeNG/Algorithms/Controls/PrimeNGToolTipTokens.cs:34` |

Table 1 共 169 行，展开为 151（Shadcn）+ 349（PrimeNG）= 500 个“语言 × Token 类 × 全局 Token”写入对。注：题述 125 行 `controlGlobal.X =` 不含 `ShadcnButtonAppearance.ApplyGlobal` 中的 8 行 `token.X =` 与 PrimeNG 的 7 处元组写入（`PrimeNGButtonTokens.cs:41`、`PrimeNGFeedbackTokens.cs:26/30/32/34`、`PrimeNGDataDisplayTokens.cs:68-71` 共 9 行），本表已全部纳入。

## 3. 读取与去向（Table 2）

一行对应一个“控件 Token 类 × 全局 Token”（两种语言写同一对时合并，“语言”列标注）。去向：A = 只被 `CalculateTokenValues` 读取；B = 被模板或 C# 读取（同时列出 A 部分）；C = 无读取者。B† = 至少一处读取是 `OwnerTokenResource`（含 `[束]`）或 `TokenTransitions.Duration`（TT），这两种 AXAML 形式只接受 Global Token，不能直接改读 Own Token（见不确定项第 2 条）；C# 的 `OwnerTokenSet.SetKind` / `ControlTokenResourceKey.Global` 读取可在代码中换成 Own 键，未标 †。“EvaluateOwn 依赖”= 该语言阶段类自己的 `EvaluateOwn` 读取了改写后的值，删除写入时须在 `EvaluateOwn` 内按改写表达式本地重算。新增 Own Token 名为建议名，其 Ant 默认公式即该全局 Token。

| 控件 Token 类 | 全局 Token | 语言 | 读取者 | 去向 | 新增 Own Token 名（仅 B） |
| --- | --- | --- | --- | --- | --- |
| `AlertToken` | `BorderRadiusLG` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:82` | B | `AlertBorderRadius` |
| `AlertToken` | `ColorError` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:33`<br>EvaluateOwn（语言阶段类）: PrimeNG PrimeNGFeedbackTokens.cs:132 | B；EvaluateOwn 依赖 | `ErrorIconColor` |
| `AlertToken` | `ColorErrorBg` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:113` | B | `ErrorBg` |
| `AlertToken` | `ColorErrorBorder` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:114` | B | `ErrorBorderColor` |
| `AlertToken` | `ColorInfo` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGFeedbackTokens.cs:130 | C；EvaluateOwn 依赖 |  |
| `AlertToken` | `ColorInfoBg` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:101` | B | `InfoBg` |
| `AlertToken` | `ColorInfoBorder` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:102` | B | `InfoBorderColor` |
| `AlertToken` | `ColorPrimary` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:25` | B | `InfoIconColor` |
| `AlertToken` | `ColorSuccess` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:21`<br>EvaluateOwn（语言阶段类）: PrimeNG PrimeNGFeedbackTokens.cs:129 | B；EvaluateOwn 依赖 | `SuccessIconColor` |
| `AlertToken` | `ColorSuccessBg` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:95` | B | `SuccessBg` |
| `AlertToken` | `ColorSuccessBorder` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:96` | B | `SuccessBorderColor` |
| `AlertToken` | `ColorWarning` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:29`<br>EvaluateOwn（语言阶段类）: PrimeNG PrimeNGFeedbackTokens.cs:131 | B；EvaluateOwn 依赖 | `WarningIconColor` |
| `AlertToken` | `ColorWarningBg` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:107` | B | `WarningBg` |
| `AlertToken` | `ColorWarningBorder` | PrimeNG | AXAML: `Alert/Themes/AlertTheme.axaml:108` | B | `WarningBorderColor` |
| `AutoCompleteSearchEditToken` | `BorderRadiusLG` | PrimeNG | AXAML: [CANDIDATE] `ListBoxTheme.axaml:76` | B† | `InputBorderRadiusLG` |
| `AutoCompleteSearchEditToken` | `BorderRadiusSM` | PrimeNG | AXAML: [CANDIDATE] `ListBoxTheme.axaml:86` | B† | `InputBorderRadiusSM` |
| `AutoCompleteSearchEditToken` | `ColorErrorBorderHover` | PrimeNG | 无 | C |  |
| `AutoCompleteSearchEditToken` | `ColorFillAlter` | PrimeNG | 无 | C |  |
| `AutoCompleteSearchEditToken` | `ColorIcon` | PrimeNG | 无 | C |  |
| `AutoCompleteSearchEditToken` | `ColorIconHover` | PrimeNG | 无 | C |  |
| `AutoCompleteSearchEditToken` | `ColorPrimaryHover` | PrimeNG | 无 | C |  |
| `AutoCompleteSearchEditToken` | `ColorTextDisabled` | PrimeNG | AXAML: [CANDIDATE] `CandidateListItemTheme.axaml:43` | B† | `DisabledColor` |
| `AutoCompleteSearchEditToken` | `ColorTextQuaternary` | PrimeNG | 无 | C |  |
| `AutoCompleteSearchEditToken` | `InputActiveShadow` | PrimeNG | 无 | C |  |
| `AutoCompleteSearchEditToken` | `InputErrorActiveShadow` | PrimeNG | 无 | C |  |
| `AutoCompleteTextAreaToken` | `BorderRadiusLG` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42`；[CANDIDATE] `ListBoxTheme.axaml:76` | B† | `InputBorderRadiusLG` |
| `AutoCompleteTextAreaToken` | `BorderRadiusSM` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49`；[CANDIDATE] `ListBoxTheme.axaml:86` | B† | `InputBorderRadiusSM` |
| `AutoCompleteTextAreaToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `AutoCompleteTextAreaToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `AutoCompleteTextAreaToken` | `ColorIcon` | PrimeNG | 无 | C |  |
| `AutoCompleteTextAreaToken` | `ColorIconHover` | PrimeNG | 无 | C |  |
| `AutoCompleteTextAreaToken` | `ColorPrimaryHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `AutoCompleteTextAreaToken` | `ColorTextDisabled` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[TEXTAREA] `TextAreaTheme.axaml:239`；[CANDIDATE] `CandidateListItemTheme.axaml:43`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `AutoCompleteTextAreaToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22` | B† | `SuffixIconColor` |
| `AutoCompleteTextAreaToken` | `InputActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `AutoCompleteTextAreaToken` | `InputErrorActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `AutoCompleteToken` | `BorderRadius` | Shadcn | AXAML: [CANDIDATE] `ListBoxTheme.axaml:81` | B† | `InputBorderRadius` |
| `AutoCompleteToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: [CANDIDATE] `ListBoxTheme.axaml:76` | B† | `InputBorderRadiusLG` |
| `AutoCompleteToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: [CANDIDATE] `ListBoxTheme.axaml:86` | B† | `InputBorderRadiusSM` |
| `AutoCompleteToken` | `ColorBgContainer` | Shadcn | 无 | C |  |
| `AutoCompleteToken` | `ColorBorder` | Shadcn | 无 | C |  |
| `AutoCompleteToken` | `ColorErrorBorderHover` | PrimeNG | 无 | C |  |
| `AutoCompleteToken` | `ColorFillAlter` | PrimeNG | 无 | C |  |
| `AutoCompleteToken` | `ColorIcon` | PrimeNG | 无 | C |  |
| `AutoCompleteToken` | `ColorIconHover` | PrimeNG | 无 | C |  |
| `AutoCompleteToken` | `ColorPrimary` | Shadcn | AXAML: [CANDIDATE] `CandidateListTheme.axaml:9` | B† | `ActiveBorderColor` |
| `AutoCompleteToken` | `ColorPrimaryHover` | Shadcn+PrimeNG | 无 | C |  |
| `AutoCompleteToken` | `ColorTextDisabled` | PrimeNG | AXAML: [CANDIDATE] `CandidateListItemTheme.axaml:43` | B† | `DisabledColor` |
| `AutoCompleteToken` | `ColorTextQuaternary` | PrimeNG | 无 | C |  |
| `AutoCompleteToken` | `InputActiveShadow` | PrimeNG | 无 | C |  |
| `AutoCompleteToken` | `InputErrorActiveShadow` | PrimeNG | 无 | C |  |
| `AvatarToken` | `BorderRadiusLG` | PrimeNG | AXAML: `AtomUI.Controls/Avatar/Themes/AvatarTheme.axaml:98` | B | `AvatarBorderRadiusLG` |
| `AvatarToken` | `BorderRadiusSM` | PrimeNG | AXAML: `AtomUI.Controls/Avatar/Themes/AvatarTheme.axaml:104` | B | `AvatarBorderRadiusSM` |
| `ButtonToken` | `BorderRadius` | Shadcn | AXAML: `Buttons/Themes/ButtonTheme.axaml:263,290（Owner）` | B† | `ButtonBorderRadius` |
| `ButtonToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:249（Owner）` | B† | `ButtonBorderRadiusLG` |
| `ButtonToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:277（Owner）` | B† | `ButtonBorderRadiusSM` |
| `ButtonToken` | `ColorBgSolid` | PrimeNG | 计算: SolidTextColor（AbstractButtonVariantToken.cs:157，经 isBright）<br>C#: `Buttons/Button.Variants.cs:245,248` | B | `SolidBg` |
| `ButtonToken` | `ColorBgSolidActive` | PrimeNG | C#: `Buttons/Button.Variants.cs:247,250` | B | `SolidActiveBg` |
| `ButtonToken` | `ColorBgSolidHover` | PrimeNG | C#: `Buttons/Button.Variants.cs:246,249` | B | `SolidHoverBg` |
| `ButtonToken` | `ColorError` | PrimeNG | C#: `Buttons/Button.Variants.cs:147`；`AdornerLayer/FocusOutlineAdorner.cs:153（仅 UsesThemeRecipe=true；PrimeNG 不满足）`<br>EvaluateOwn（语言阶段类）: PrimeNG PrimeNGButtonTokens.cs `DangerColor`（暗色）经 PrimeNGSeverity.Error → PrimeNGDataDisplayTokens.cs:26 读 ColorError | B；EvaluateOwn 依赖 | `DangerBaseColor` |
| `ButtonToken` | `ColorErrorActive` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:149` | B | `DangerActiveColor` |
| `ButtonToken` | `ColorErrorBg` | Shadcn | C#: `Buttons/Button.Variants.cs:157,197,211` | B | `DangerFilledBg` |
| `ButtonToken` | `ColorErrorBgHover` | Shadcn | C#: `Buttons/Button.Variants.cs:158,198` | B | `DangerFilledHoverBg` |
| `ButtonToken` | `ColorErrorBorderHover` | Shadcn | C#: `Buttons/Button.Variants.cs:182,188` | B | `DangerHoverBorderColor` |
| `ButtonToken` | `ColorErrorHover` | PrimeNG | C#: `Buttons/Button.Variants.cs:148` | B | `DangerHoverColor` |
| `ButtonToken` | `ColorFillSecondary` | Shadcn | C#: `Buttons/Button.Variants.cs:295` | B | `FilledHoverBg` |
| `ButtonToken` | `ColorFillTertiary` | Shadcn | 计算: TextHoverBg（AbstractButtonVariantToken.cs:146）<br>C#: `Buttons/Button.Variants.cs:294` | B | `FilledBg` |
| `ButtonToken` | `ColorFocusBorder` | Shadcn | C#: `AdornerLayer/FocusOutlineAdorner.cs:150（仅 Button.UsesThemeRecipe=true）` | B | `FocusBorderColor` |
| `ButtonToken` | `ColorLink` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:318` | B | `LinkColor` |
| `ButtonToken` | `ColorLinkActive` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:322（非 Primary 色时）` | B | `LinkActiveColor` |
| `ButtonToken` | `ColorLinkHover` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:319` | B | `LinkHoverColor` |
| `ButtonToken` | `ControlItemColorActive` | Shadcn | C#: `Buttons/Button.Variants.cs:289（仅 RecipeVariants=true 时作为 DefaultColor 的回退）` | B | `FilledTextColor` |
| `ButtonToken` | `EnableWaveSpirit` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:241（Owner）` | B† | `WaveSpiritEnabled` |
| `ButtonToken` | `LineWidthFocus` | Shadcn | C#: `AdornerLayer/FocusOutlineAdorner.cs:151（仅 Button.UsesThemeRecipe=true）` | B | `FocusLineWidth` |
| `ButtonToken` | `OpacityLoading` | PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:362（Owner）` | B† | `LoadingOpacity` |
| `CardTabControlToken` | `BorderRadius` | Shadcn | C#: `TabControl/CardTabControl.cs:54` | B | `CardBorderRadius` |
| `CardTabControlToken` | `BorderRadiusLG` | Shadcn | C#: `TabControl/CardTabControl.cs:55` | B | `CardBorderRadiusLG` |
| `CardTabControlToken` | `BorderRadiusSM` | Shadcn | C#: `TabControl/CardTabControl.cs:53` | B | `CardBorderRadiusSM` |
| `CardTabStripToken` | `BorderRadius` | Shadcn | C#: `TabControl/TabStrip/CardTabStrip.cs:64` | B | `CardBorderRadius` |
| `CardTabStripToken` | `BorderRadiusLG` | Shadcn | C#: `TabControl/TabStrip/CardTabStrip.cs:65` | B | `CardBorderRadiusLG` |
| `CardTabStripToken` | `BorderRadiusSM` | Shadcn | C#: `TabControl/TabStrip/CardTabStrip.cs:63` | B | `CardBorderRadiusSM` |
| `CardToken` | `BorderRadiusLG` | PrimeNG | AXAML: `Card/Themes/CardTheme.axaml:71` | B | `CardBorderRadius` |
| `CardToken` | `ColorBgContainer` | Shadcn+PrimeNG | 计算: ActionsBg（CardToken.cs:183）<br>AXAML: `Card/Themes/CardTheme.axaml:74`<br>EvaluateOwn（语言阶段类）: PrimeNG PrimeNGCardTokens.cs:45 | B；EvaluateOwn 依赖 | `CardBg` |
| `CardToken` | `ColorBorderSecondary` | PrimeNG | AXAML: `Card/Themes/CardTheme.axaml:72,92`；`Card/Themes/CardGridContentTheme.axaml:22`；`Card/Themes/CardActionPanelTheme.axaml:20` | B | `CardBorderColor` |
| `CarouselToken` | `MotionDurationSlow` | PrimeNG | AXAML: `Carousel/Themes/CarouselTheme.axaml:45` | B | `MotionDuration` |
| `CascaderToken` | `BorderRadius` | Shadcn | 计算: OptionBorderRadius（CascaderToken.cs:108）<br>AXAML: [FRAME] `InputControlFrameTheme.axaml:45`；[TEXTBOX] `TextBoxTheme.axaml:162,172,182` | B† | `InputBorderRadius` |
| `CascaderToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42`；[TEXTBOX] `TextBoxTheme.axaml:157,178` | B† | `InputBorderRadiusLG` |
| `CascaderToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49`；[TEXTBOX] `TextBoxTheme.axaml:167,186` | B† | `InputBorderRadiusSM` |
| `CascaderToken` | `ColorBgContainer` | Shadcn | C#: [FRAME] `InputControlFrame.cs:590` | B | `DefaultBg` |
| `CascaderToken` | `ColorBorder` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:145,157,161`；[ADDON] `AddOnDecoratedBoxTheme.axaml:112,117`<br>C#: [FRAME] `InputControlFrame.cs:588,608` | B† | `DefaultBorderColor` |
| `CascaderToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `CascaderToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `CascaderToken` | `ColorIcon` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:18` | B† | `IconColor` |
| `CascaderToken` | `ColorIconHover` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:22` | B† | `IconHoverColor` |
| `CascaderToken` | `ColorPrimary` | Shadcn | AXAML: `Cascader/Themes/CascaderAddOnDecoratedBoxTheme.axaml:82`<br>C#: [FRAME] `InputControlFrame.cs:593,604,612` | B | `ActiveBorderColor` |
| `CascaderToken` | `ColorPrimaryHover` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `CascaderToken` | `ColorTextDisabled` | PrimeNG | 计算: MultipleItemColorDisabled（AbstractSelectInputToken.cs:125）<br>AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[REVEAL] `RevealButtonTheme.axaml:32`；[TAGBOX] `SelectTagAwareTextBoxTheme.axaml:31`；`Cascader/Themes/CascaderTheme.axaml:312,315`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `CascaderToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22` | B† | `SuffixIconColor` |
| `CascaderToken` | `InputActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `CascaderToken` | `InputErrorActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `CheckBoxToken` | `BorderRadiusSM` | Shadcn | AXAML: `CheckBox/Themes/CheckBoxTheme.axaml:54` | B | `IndicatorBorderRadius` |
| `CheckBoxToken` | `ColorWhite` | Shadcn+PrimeNG | AXAML: `CheckBox/Themes/CheckBoxTheme.axaml:58` | B | `CheckedMarkColor` |
| `CollapseToken` | `ColorBorder` | PrimeNG | AXAML: `Collapse/Themes/CollapseItemTheme.axaml:14,87,96`；`Collapse/Themes/CollapseTheme.axaml:25` | B | `CollapsePanelBorderColor` |
| `CollapseToken` | `MotionDurationSlow` | PrimeNG | AXAML: `Collapse/Themes/CollapseItemTheme.axaml:84` | B | `ContentMotionDuration` |
| `ContextMenuToken` | `MotionDurationMid` | PrimeNG | AXAML: `Menu/Themes/ContextMenuTheme.axaml:64` | B | `MotionDuration` |
| `DatePickerToken` | `BorderRadius` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:45`；[TEXTBOX] `TextBoxTheme.axaml:162,172,182` | B† | `InputBorderRadius` |
| `DatePickerToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42`；[TEXTBOX] `TextBoxTheme.axaml:157,178` | B† | `InputBorderRadiusLG` |
| `DatePickerToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49`；[TEXTBOX] `TextBoxTheme.axaml:167,186`；[DATECELL] `DateViewerCellTheme.axaml:47`；[TIMECELL] `TimeViewCellTheme.axaml:50` | B† | `InputBorderRadiusSM` |
| `DatePickerToken` | `ColorBgContainer` | Shadcn | C#: [FRAME] `InputControlFrame.cs:590` | B | `DefaultBg` |
| `DatePickerToken` | `ColorBorder` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:145,157,161`；[ADDON] `AddOnDecoratedBoxTheme.axaml:112,117`<br>C#: [FRAME] `InputControlFrame.cs:588,608` | B† | `DefaultBorderColor` |
| `DatePickerToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `DatePickerToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `DatePickerToken` | `ColorIcon` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:18` | B† | `IconColor` |
| `DatePickerToken` | `ColorIconHover` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:22` | B† | `IconHoverColor` |
| `DatePickerToken` | `ColorPrimary` | Shadcn | AXAML: [DATECELL] `DateViewerCellTheme.axaml:114,142,239,245`<br>C#: [FRAME] `InputControlFrame.cs:593,604,612` | B† | `ActiveBorderColor` |
| `DatePickerToken` | `ColorPrimaryHover` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `DatePickerToken` | `ColorTextDisabled` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[REVEAL] `RevealButtonTheme.axaml:32`；[PICKER] `InfoPickerInputTheme.axaml:133`；[DATECELL] `DateViewerCellTheme.axaml:168,171,181`；[TIMECELL] `TimeViewCellTheme.axaml:73`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `DatePickerToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22`；[PICKER] `PickerClearUpButtonTheme.axaml:40` | B† | `SuffixIconColor` |
| `DatePickerToken` | `InputActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `DatePickerToken` | `InputErrorActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `DescriptionsToken` | `ColorBorderSecondary` | PrimeNG | 无 | C |  |
| `DescriptionsToken` | `ColorSplit` | PrimeNG | AXAML: `Descriptions/Themes/DescriptionsTheme.axaml:39`；`Descriptions/Themes/DescriptionDefaultItemTheme.axaml:117,128`；`Descriptions/Themes/DescriptionBorderedItemLabelTheme.axaml:30`；`Descriptions/Themes/DescriptionBorderedItemContentTheme.axaml:29` | B | `ItemBorderColor` |
| `DialogToken` | `BorderRadiusLG` | PrimeNG | AXAML: `Dialog/Themes/DialogSurfaceTheme.axaml:9（Owner）` | B† | `ContentBorderRadius` |
| `DialogToken` | `BorderRadiusSM` | PrimeNG | AXAML: `Dialog/Themes/DialogCaptionButtonTheme.axaml:41（Owner）` | B† | `CloseBtnBorderRadius` |
| `DialogToken` | `BoxShadows` | Shadcn+PrimeNG | AXAML: `Dialog/Themes/DialogSurfaceTheme.axaml:81（Owner）` | B† | `ContentShadows` |
| `DialogToken` | `FontWeightStrong` | PrimeNG | AXAML: `Dialog/Themes/DialogSurfaceTheme.axaml:75（Owner）` | B† | `HeaderFontWeight` |
| `DialogToken` | `MotionDurationMid` | PrimeNG | AXAML: `Dialog/Themes/DialogCaptionButtonTheme.axaml:59（TT）`；`Dialog/Themes/OverlayDialogHeaderTheme.axaml:69（TT）`<br>C#: `Dialog/OverlayHost/OverlayDialogPresenter.cs:500` | B† | `MotionDuration` |
| `DialogToken` | `MotionDurationSlow` | PrimeNG | C#: `Dialog/OverlayHost/OverlayDialogPresenter.cs:501` | B | `OpeningMotionDuration` |
| `DrawerToken` | `FontSizeLG` | PrimeNG | AXAML: `Drawer/Themes/DrawerInfoContainerTheme.axaml:128` | B | `TitleFontSize` |
| `DrawerToken` | `FontWeightStrong` | PrimeNG | AXAML: `Drawer/Themes/DrawerInfoContainerTheme.axaml:130` | B | `TitleFontWeight` |
| `DrawerToken` | `MotionDurationSlow` | PrimeNG | AXAML: `Drawer/Themes/DrawerContainerTheme.axaml:41` | B | `MotionDuration` |
| `DropdownButtonToken` | `BorderRadius` | Shadcn | AXAML: `Buttons/Themes/DropdownButtonBaseTheme.axaml:212,240` | B | `ButtonBorderRadius` |
| `DropdownButtonToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/DropdownButtonBaseTheme.axaml:198` | B | `ButtonBorderRadiusLG` |
| `DropdownButtonToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/DropdownButtonBaseTheme.axaml:226` | B | `ButtonBorderRadiusSM` |
| `DropdownButtonToken` | `ColorBgSolid` | PrimeNG | 计算: SolidTextColor（AbstractButtonVariantToken.cs:157，经 isBright）<br>C#: `Buttons/Button.Variants.cs:245,248` | B | `SolidBg` |
| `DropdownButtonToken` | `ColorBgSolidActive` | PrimeNG | C#: `Buttons/Button.Variants.cs:247,250` | B | `SolidActiveBg` |
| `DropdownButtonToken` | `ColorBgSolidHover` | PrimeNG | C#: `Buttons/Button.Variants.cs:246,249` | B | `SolidHoverBg` |
| `DropdownButtonToken` | `ColorError` | PrimeNG | C#: `Buttons/Button.Variants.cs:147`；`AdornerLayer/FocusOutlineAdorner.cs:153（仅 UsesThemeRecipe=true；PrimeNG 不满足）`<br>EvaluateOwn（语言阶段类）: PrimeNG PrimeNGButtonTokens.cs `DangerColor`（暗色）经 PrimeNGSeverity.Error → PrimeNGDataDisplayTokens.cs:26 读 ColorError | B；EvaluateOwn 依赖 | `DangerBaseColor` |
| `DropdownButtonToken` | `ColorErrorActive` | PrimeNG | C#: `Buttons/Button.Variants.cs:149` | B | `DangerActiveColor` |
| `DropdownButtonToken` | `ColorErrorBg` | Shadcn | C#: `Buttons/Button.Variants.cs:157,197,211` | B | `DangerFilledBg` |
| `DropdownButtonToken` | `ColorErrorBgHover` | Shadcn | C#: `Buttons/Button.Variants.cs:158,198` | B | `DangerFilledHoverBg` |
| `DropdownButtonToken` | `ColorErrorHover` | PrimeNG | C#: `Buttons/Button.Variants.cs:148` | B | `DangerHoverColor` |
| `DropdownButtonToken` | `ColorFillSecondary` | Shadcn | C#: `Buttons/Button.Variants.cs:295` | B | `FilledHoverBg` |
| `DropdownButtonToken` | `ColorFillTertiary` | Shadcn | 计算: TextHoverBg（AbstractButtonVariantToken.cs:146）<br>C#: `Buttons/Button.Variants.cs:294` | B | `FilledBg` |
| `DropdownButtonToken` | `ColorFocusBorder` | Shadcn | C#: `AdornerLayer/FocusOutlineAdorner.cs:150（仅 Button.UsesThemeRecipe=true）` | B | `FocusBorderColor` |
| `DropdownButtonToken` | `ColorLink` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:318` | B | `LinkColor` |
| `DropdownButtonToken` | `ColorLinkActive` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:322（非 Primary 色时）` | B | `LinkActiveColor` |
| `DropdownButtonToken` | `ColorLinkHover` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:319` | B | `LinkHoverColor` |
| `DropdownButtonToken` | `ControlItemColorActive` | Shadcn | C#: `Buttons/Button.Variants.cs:289（仅 RecipeVariants=true 时作为 DefaultColor 的回退）` | B | `FilledTextColor` |
| `DropdownButtonToken` | `EnableWaveSpirit` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/DropdownButtonBaseTheme.axaml:191` | B | `WaveSpiritEnabled` |
| `DropdownButtonToken` | `LineWidthFocus` | Shadcn | C#: `AdornerLayer/FocusOutlineAdorner.cs:151（仅 Button.UsesThemeRecipe=true）` | B | `FocusLineWidth` |
| `DropdownButtonToken` | `OpacityLoading` | PrimeNG | AXAML: `Buttons/Themes/DropdownButtonBaseTheme.axaml:310` | B | `LoadingOpacity` |
| `EmptyToken` | `ColorBorderSecondary` | Shadcn | C#: `Empty/Empty.cs:15` | B | `PresetImageBorderColor` |
| `EmptyToken` | `ColorFill` | Shadcn | C#: `Empty/Empty.cs:14` | B | `PresetImageFill` |
| `EmptyToken` | `ColorFillQuaternary` | Shadcn | C#: `Empty/Empty.cs:17` | B | `PresetImageFillQuaternary` |
| `EmptyToken` | `ColorFillSecondary` | Shadcn | 无 | C |  |
| `EmptyToken` | `ColorFillTertiary` | Shadcn | C#: `Empty/Empty.cs:16` | B | `PresetImageFillTertiary` |
| `ExpanderToken` | `ColorBorder` | PrimeNG | AXAML: `Expander/Themes/ExpanderTheme.axaml:76,97` | B | `ExpanderBorderColor` |
| `ExpanderToken` | `MotionDurationSlow` | PrimeNG | AXAML: `Expander/Themes/ExpanderTheme.axaml:90` | B | `ContentMotionDuration` |
| `FloatButtonToken` | `BorderRadius` | Shadcn | AXAML: `FloatButton/Themes/AbstractFloatButtonTheme.axaml:147` | B | `FloatButtonBorderRadius` |
| `FloatButtonToken` | `BorderRadiusLG` | Shadcn | AXAML: `FloatButton/Themes/AbstractFloatButtonTheme.axaml:159`；`FloatButton/Themes/AbstractFloatButtonTheme.axaml:63（Owner）` | B† | `FloatButtonBorderRadiusLG` |
| `FloatButtonToken` | `ColorBorder` | Shadcn | AXAML: `FloatButton/Themes/AbstractFloatButtonTheme.axaml:120`；`FloatButton/Themes/FloatButtonItemsControlTheme.axaml:65`<br>EvaluateOwn（语言阶段类）: Shadcn ShadcnFloatButtonTokens → ShadcnButtonAppearance.ResolveOutline（ShadcnButtonAppearance.cs:82 读 ColorBorder） | B；EvaluateOwn 依赖 | `DefaultBorderColor` |
| `FloatButtonToken` | `PaddingLG` | Shadcn | AXAML: `FloatButton/Themes/AbstractFloatButtonTheme.axaml:158` | B | `DescriptionPadding` |
| `FloatButtonToken` | `SpacingXS` | Shadcn | AXAML: `FloatButton/Themes/AbstractFloatButtonTheme.axaml:167` | B | `DescriptionSpacing` |
| `FlyoutHostToken` | `MotionDurationMid` | PrimeNG | AXAML: `Flyouts/Themes/FlyoutHostTheme.axaml:18`<br>C#: `Flyouts/Flyout.cs:524（ControlTokenResourceKey.Global(typeof(FlyoutHost), …)）` | B | `MotionDuration` |
| `GroupBoxToken` | `ColorBgContainer` | PrimeNG | AXAML: `GroupBox/Themes/GroupBoxTheme.axaml:50` | B | `GroupBoxBg` |
| `GroupBoxToken` | `ColorBorder` | PrimeNG | AXAML: `GroupBox/Themes/GroupBoxTheme.axaml:51` | B | `GroupBoxBorderColor` |
| `LineEditToken` | `BorderRadius` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:45` | B† | `InputBorderRadius` |
| `LineEditToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42` | B† | `InputBorderRadiusLG` |
| `LineEditToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49` | B† | `InputBorderRadiusSM` |
| `LineEditToken` | `ColorBgContainer` | Shadcn | C#: [FRAME] `InputControlFrame.cs:590` | B | `DefaultBg` |
| `LineEditToken` | `ColorBorder` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:145,157,161`；[ADDON] `AddOnDecoratedBoxTheme.axaml:112,117`<br>C#: [FRAME] `InputControlFrame.cs:588,608` | B† | `DefaultBorderColor` |
| `LineEditToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `LineEditToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `LineEditToken` | `ColorIcon` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:18` | B† | `IconColor` |
| `LineEditToken` | `ColorIconHover` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:22` | B† | `IconHoverColor` |
| `LineEditToken` | `ColorPrimary` | Shadcn | C#: [FRAME] `InputControlFrame.cs:593,604,612` | B | `ActiveBorderColor` |
| `LineEditToken` | `ColorPrimaryHover` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `LineEditToken` | `ColorTextDisabled` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[REVEAL] `RevealButtonTheme.axaml:32`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `LineEditToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22` | B† | `SuffixIconColor` |
| `LineEditToken` | `InputActiveShadow` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `LineEditToken` | `InputErrorActiveShadow` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `LineEditToken` | `InputWarningActiveShadow` | Shadcn | C#: [FRAME] `InputControlFrame.cs:596` | B | `WarningActiveShadow` |
| `MentionsToken` | `BorderRadius` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:45`；[CANDIDATE] `ListBoxTheme.axaml:81` | B† | `InputBorderRadius` |
| `MentionsToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42`；[CANDIDATE] `ListBoxTheme.axaml:76` | B† | `InputBorderRadiusLG` |
| `MentionsToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49`；[CANDIDATE] `ListBoxTheme.axaml:86` | B† | `InputBorderRadiusSM` |
| `MentionsToken` | `ColorBgContainer` | Shadcn | C#: [FRAME] `InputControlFrame.cs:590` | B | `DefaultBg` |
| `MentionsToken` | `ColorBorder` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:145,157,161`；[ADDON] `AddOnDecoratedBoxTheme.axaml:112,117`<br>C#: [FRAME] `InputControlFrame.cs:588,608` | B† | `DefaultBorderColor` |
| `MentionsToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `MentionsToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `MentionsToken` | `ColorIcon` | PrimeNG | 无 | C |  |
| `MentionsToken` | `ColorIconHover` | PrimeNG | 无 | C |  |
| `MentionsToken` | `ColorPrimary` | Shadcn | AXAML: [CANDIDATE] `CandidateListTheme.axaml:9`<br>C#: [FRAME] `InputControlFrame.cs:593,604,612` | B† | `ActiveBorderColor` |
| `MentionsToken` | `ColorPrimaryHover` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `MentionsToken` | `ColorTextDisabled` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[TEXTAREA] `TextAreaTheme.axaml:239`；[CANDIDATE] `CandidateListItemTheme.axaml:43`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `MentionsToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22` | B† | `SuffixIconColor` |
| `MentionsToken` | `InputActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `MentionsToken` | `InputErrorActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `MessageBoxToken` | `BorderRadiusLG` | PrimeNG | AXAML: `Dialog/Themes/DialogSurfaceTheme.axaml:9（Owner）` | B† | `ContentBorderRadius` |
| `MessageBoxToken` | `BorderRadiusSM` | PrimeNG | AXAML: `Dialog/Themes/DialogCaptionButtonTheme.axaml:41（Owner）` | B† | `CloseBtnBorderRadius` |
| `MessageBoxToken` | `BoxShadows` | PrimeNG | AXAML: `Dialog/Themes/DialogSurfaceTheme.axaml:81（Owner）` | B† | `ContentShadows` |
| `MessageBoxToken` | `FontWeightStrong` | PrimeNG | AXAML: `Dialog/Themes/DialogSurfaceTheme.axaml:75（Owner）` | B† | `HeaderFontWeight` |
| `MessageBoxToken` | `MotionDurationMid` | PrimeNG | AXAML: `Dialog/Themes/DialogCaptionButtonTheme.axaml:59（TT）`；`Dialog/Themes/OverlayDialogHeaderTheme.axaml:69（TT）`<br>C#: `Dialog/OverlayHost/OverlayDialogPresenter.cs:500` | B† | `MotionDuration` |
| `MessageBoxToken` | `MotionDurationSlow` | PrimeNG | C#: `Dialog/OverlayHost/OverlayDialogPresenter.cs:501` | B | `OpeningMotionDuration` |
| `MessageCardToken` | `BorderRadiusLG` | PrimeNG | AXAML: `Message/Themes/MessageCardTheme.axaml:107`；`Message/Themes/WindowMessageManagerTheme.axaml:55` | B | `MessageBorderRadius` |
| `MessageCardToken` | `ColorError` | PrimeNG | 计算: ErrorIconColor（MessageCardToken.cs:149） | A |  |
| `MessageCardToken` | `ColorErrorBg` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGMessageCardTokens.cs:51 | C；EvaluateOwn 依赖 |  |
| `MessageCardToken` | `ColorErrorBorder` | PrimeNG | 无 | C |  |
| `MessageCardToken` | `ColorErrorText` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGMessageCardTokens.cs:51 | C；EvaluateOwn 依赖 |  |
| `MessageCardToken` | `ColorInfo` | PrimeNG | 无 | C |  |
| `MessageCardToken` | `ColorInfoBg` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGMessageCardTokens.cs:45 | C；EvaluateOwn 依赖 |  |
| `MessageCardToken` | `ColorInfoBorder` | PrimeNG | 无 | C |  |
| `MessageCardToken` | `ColorInfoText` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGMessageCardTokens.cs:45 | C；EvaluateOwn 依赖 |  |
| `MessageCardToken` | `ColorPrimary` | PrimeNG | 计算: InformationIconColor（MessageCardToken.cs:140）<br>AXAML: `Message/Themes/MessageCardTheme.axaml:51` | B | `LoadingIconColor` |
| `MessageCardToken` | `ColorSuccess` | PrimeNG | 计算: SuccessIconColor（MessageCardToken.cs:143） | A |  |
| `MessageCardToken` | `ColorSuccessBg` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGMessageCardTokens.cs:47 | C；EvaluateOwn 依赖 |  |
| `MessageCardToken` | `ColorSuccessBorder` | PrimeNG | 无 | C |  |
| `MessageCardToken` | `ColorSuccessText` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGMessageCardTokens.cs:47 | C；EvaluateOwn 依赖 |  |
| `MessageCardToken` | `ColorWarning` | PrimeNG | 计算: WarningIconColor（MessageCardToken.cs:146） | A |  |
| `MessageCardToken` | `ColorWarningBg` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGMessageCardTokens.cs:49 | C；EvaluateOwn 依赖 |  |
| `MessageCardToken` | `ColorWarningBorder` | PrimeNG | 无 | C |  |
| `MessageCardToken` | `ColorWarningText` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGMessageCardTokens.cs:49 | C；EvaluateOwn 依赖 |  |
| `MessageCardToken` | `MotionDurationMid` | PrimeNG | AXAML: `Message/Themes/MessageCardTheme.axaml:80`；`Message/Themes/MessageCardTheme.axaml:87,90（TT）` | B† | `MotionDuration` |
| `NavMenuToken` | `MotionDurationMid` | PrimeNG | AXAML: `NavMenu/Themes/NavMenuItemTheme.axaml:231`；`NavMenu/Themes/BaseNavMenuItemHeaderTheme.axaml:217（TT）`；`NavMenu/Themes/HorizontalNavMenuItemHeaderTheme.axaml:150,151（TT）`；`NavMenu/Themes/InlineNavMenuItemHeaderTheme.axaml:96,97（TT）`<br>C#: `NavMenu/NavMenu.cs:1008（ControlTokenAccessor.Capture<NavMenu>）` | B† | `MotionDuration` |
| `NotificationCardToken` | `BorderRadiusLG` | PrimeNG | 计算: NotificationProgressMargin（NotificationCardToken.cs:348）<br>AXAML: `Notifications/Themes/NotificationCardTheme.axaml:121` | B | `NotificationBorderRadius` |
| `NotificationCardToken` | `BorderRadiusSM` | PrimeNG | AXAML: `Notifications/Themes/NotificationCardTheme.axaml:220` | B | `CloseButtonBorderRadius` |
| `NotificationCardToken` | `ColorError` | PrimeNG | 计算: ErrorIconColor（NotificationCardToken.cs:328） | A |  |
| `NotificationCardToken` | `ColorErrorBg` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:67 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `ColorErrorBorder` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:92 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `ColorErrorText` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:67 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `ColorInfo` | PrimeNG | 无 | C |  |
| `NotificationCardToken` | `ColorInfoBg` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:58 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `ColorInfoBorder` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:89 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `ColorInfoText` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:58 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `ColorPrimary` | PrimeNG | 计算: InformationIconColor（NotificationCardToken.cs:307）、NotificationProgressBg（NotificationCardToken.cs:357） | A |  |
| `NotificationCardToken` | `ColorSuccess` | PrimeNG | 计算: SuccessIconColor（NotificationCardToken.cs:314） | A |  |
| `NotificationCardToken` | `ColorSuccessBg` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:61 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `ColorSuccessBorder` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:90 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `ColorSuccessText` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:61 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `ColorWarning` | PrimeNG | 计算: WarningIconColor（NotificationCardToken.cs:321） | A |  |
| `NotificationCardToken` | `ColorWarningBg` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:64 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `ColorWarningBorder` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:91 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `ColorWarningText` | PrimeNG | EvaluateOwn（语言阶段类）: PrimeNG PrimeNGNotificationCardTokens.cs:64 | C；EvaluateOwn 依赖 |  |
| `NotificationCardToken` | `FontSizeLG` | PrimeNG | 计算: NotificationIconSize（NotificationCardToken.cs:334）<br>AXAML: `Notifications/Themes/NotificationCardTheme.axaml:187` | B | `TitleFontSize` |
| `NotificationCardToken` | `IconSizeSM` | PrimeNG | AXAML: `Notifications/Themes/NotificationCardTheme.axaml:222,224` | B | `CloseIconSize` |
| `NotificationCardToken` | `MotionDurationMid` | PrimeNG | AXAML: `Notifications/Themes/NotificationCardTheme.axaml:89`；`Notifications/Themes/NotificationCardTheme.axaml:95,98,101（TT）` | B† | `MotionDuration` |
| `NumericUpDownToken` | `BorderRadiusLG` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42`；[TEXTBOX] `TextBoxTheme.axaml:157,178`；[SPINNER] `ButtonSpinnerTheme.axaml:76` | B† | `InputBorderRadiusLG` |
| `NumericUpDownToken` | `BorderRadiusSM` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49`；[TEXTBOX] `TextBoxTheme.axaml:167,186`；[SPINNER] `ButtonSpinnerTheme.axaml:82` | B† | `InputBorderRadiusSM` |
| `NumericUpDownToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `NumericUpDownToken` | `ColorFillAlter` | PrimeNG | 计算: HandleActiveBg（AbstractButtonSpinnerToken.cs:71）<br>AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `NumericUpDownToken` | `ColorIcon` | PrimeNG | 计算: HandleIconColor（AbstractButtonSpinnerToken.cs:77）<br>AXAML: [REVEAL] `RevealButtonTheme.axaml:18` | B† | `IconColor` |
| `NumericUpDownToken` | `ColorIconHover` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:22` | B† | `IconHoverColor` |
| `NumericUpDownToken` | `ColorPrimaryHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `NumericUpDownToken` | `ColorTextDisabled` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[REVEAL] `RevealButtonTheme.axaml:32`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `NumericUpDownToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22` | B† | `SuffixIconColor` |
| `NumericUpDownToken` | `InputActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `NumericUpDownToken` | `InputErrorActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `OtpLineEditToken` | `BorderRadiusLG` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42` | B† | `InputBorderRadiusLG` |
| `OtpLineEditToken` | `BorderRadiusSM` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49` | B† | `InputBorderRadiusSM` |
| `OtpLineEditToken` | `ColorErrorBorderHover` | PrimeNG | AXAML: `OtpLineEdit/Themes/OtpLineEditCellTheme.axaml:150`<br>C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `OtpLineEditToken` | `ColorFillAlter` | PrimeNG | 无 | C |  |
| `OtpLineEditToken` | `ColorIcon` | PrimeNG | 无 | C |  |
| `OtpLineEditToken` | `ColorIconHover` | PrimeNG | 无 | C |  |
| `OtpLineEditToken` | `ColorPrimaryHover` | PrimeNG | AXAML: `OtpLineEdit/Themes/OtpLineEditCellTheme.axaml:117`<br>C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `OtpLineEditToken` | `ColorTextDisabled` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；`OtpLineEdit/Themes/OtpLineEditCellTheme.axaml:203` | B† | `DisabledColor` |
| `OtpLineEditToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [CLEAR] `InputClearIconButtonTheme.axaml:22` | B† | `SuffixIconColor` |
| `OtpLineEditToken` | `InputActiveShadow` | PrimeNG | AXAML: `OtpLineEdit/Themes/OtpLineEditCellTheme.axaml:129`<br>C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `OtpLineEditToken` | `InputErrorActiveShadow` | PrimeNG | AXAML: `OtpLineEdit/Themes/OtpLineEditCellTheme.axaml:162`<br>C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `PaginationToken` | `UniformlyMarginXS` | PrimeNG | 计算: PaginationLayoutSpacing（AbstractPaginationToken.cs:48）<br>AXAML: `Pagination/Themes/PaginationNavTheme.axaml:25` | B | `ItemSpacing` |
| `PopupConfirmToken` | `IconSizeLG` | PrimeNG | AXAML: `PopupConfirm/Themes/PopupConfirmContainerTheme.axaml:80,81` | B | `IconSize` |
| `PopupConfirmToken` | `MotionDurationMid` | PrimeNG | AXAML: `PopupConfirm/Themes/PopupConfirmTheme.axaml:22` | B | `MotionDuration` |
| `PopupToken` | `MotionDurationMid` | PrimeNG | AXAML: `Popup/Themes/PopupTheme.axaml:9` | B | `MotionDuration` |
| `QRCodeToken` | `BorderRadiusLG` | PrimeNG | AXAML: `QRCode/Themes/QRCodeTheme.axaml:103` | B | `QRCodeBorderRadius` |
| `QRCodeToken` | `ColorSplit` | PrimeNG | AXAML: `QRCode/Themes/QRCodeTheme.axaml:107` | B | `QRCodeBorderColor` |
| `RangeDatePickerToken` | `BorderRadiusLG` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42`；[TEXTBOX] `TextBoxTheme.axaml:157,178` | B† | `InputBorderRadiusLG` |
| `RangeDatePickerToken` | `BorderRadiusSM` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49`；[TEXTBOX] `TextBoxTheme.axaml:167,186`；[DATECELL] `DateViewerCellTheme.axaml:47`；[TIMECELL] `TimeViewCellTheme.axaml:50` | B† | `InputBorderRadiusSM` |
| `RangeDatePickerToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `RangeDatePickerToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `RangeDatePickerToken` | `ColorIcon` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:18`；[RPICKER] `RangeInfoPickerInputTheme.axaml:165` | B† | `IconColor` |
| `RangeDatePickerToken` | `ColorIconHover` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:22` | B† | `IconHoverColor` |
| `RangeDatePickerToken` | `ColorPrimaryHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `RangeDatePickerToken` | `ColorTextDisabled` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[REVEAL] `RevealButtonTheme.axaml:32`；[PICKER] `InfoPickerInputTheme.axaml:133`；[RPICKER] `RangeInfoPickerInputTheme.axaml:167`；[DATECELL] `DateViewerCellTheme.axaml:168,171,181`；[TIMECELL] `TimeViewCellTheme.axaml:73`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `RangeDatePickerToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22`；[PICKER] `PickerClearUpButtonTheme.axaml:40` | B† | `SuffixIconColor` |
| `RangeDatePickerToken` | `InputActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `RangeDatePickerToken` | `InputErrorActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `RangeTimePickerToken` | `BorderRadiusLG` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42`；[TEXTBOX] `TextBoxTheme.axaml:157,178` | B† | `InputBorderRadiusLG` |
| `RangeTimePickerToken` | `BorderRadiusSM` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49`；[TEXTBOX] `TextBoxTheme.axaml:167,186`；[TIMECELL] `TimeViewCellTheme.axaml:50` | B† | `InputBorderRadiusSM` |
| `RangeTimePickerToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `RangeTimePickerToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `RangeTimePickerToken` | `ColorIcon` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:18`；[RPICKER] `RangeInfoPickerInputTheme.axaml:165` | B† | `IconColor` |
| `RangeTimePickerToken` | `ColorIconHover` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:22` | B† | `IconHoverColor` |
| `RangeTimePickerToken` | `ColorPrimaryHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `RangeTimePickerToken` | `ColorTextDisabled` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[REVEAL] `RevealButtonTheme.axaml:32`；[PICKER] `InfoPickerInputTheme.axaml:133`；[RPICKER] `RangeInfoPickerInputTheme.axaml:167`；[TIMECELL] `TimeViewCellTheme.axaml:73`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `RangeTimePickerToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22`；[PICKER] `PickerClearUpButtonTheme.axaml:40` | B† | `SuffixIconColor` |
| `RangeTimePickerToken` | `InputActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `RangeTimePickerToken` | `InputErrorActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `ResetButtonToken` | `BorderRadius` | Shadcn | AXAML: `Buttons/Themes/ButtonTheme.axaml:263,290（Owner）` | B† | `ButtonBorderRadius` |
| `ResetButtonToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:249（Owner）` | B† | `ButtonBorderRadiusLG` |
| `ResetButtonToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:277（Owner）` | B† | `ButtonBorderRadiusSM` |
| `ResetButtonToken` | `ColorBgSolid` | PrimeNG | 计算: SolidTextColor（AbstractButtonVariantToken.cs:157，经 isBright）<br>C#: `Buttons/Button.Variants.cs:245,248` | B | `SolidBg` |
| `ResetButtonToken` | `ColorBgSolidActive` | PrimeNG | C#: `Buttons/Button.Variants.cs:247,250` | B | `SolidActiveBg` |
| `ResetButtonToken` | `ColorBgSolidHover` | PrimeNG | C#: `Buttons/Button.Variants.cs:246,249` | B | `SolidHoverBg` |
| `ResetButtonToken` | `ColorError` | PrimeNG | C#: `Buttons/Button.Variants.cs:147`；`AdornerLayer/FocusOutlineAdorner.cs:153（仅 UsesThemeRecipe=true；PrimeNG 不满足）`<br>EvaluateOwn（语言阶段类）: PrimeNG PrimeNGButtonTokens.cs `DangerColor`（暗色）经 PrimeNGSeverity.Error → PrimeNGDataDisplayTokens.cs:26 读 ColorError | B；EvaluateOwn 依赖 | `DangerBaseColor` |
| `ResetButtonToken` | `ColorErrorActive` | PrimeNG | C#: `Buttons/Button.Variants.cs:149` | B | `DangerActiveColor` |
| `ResetButtonToken` | `ColorErrorBg` | Shadcn | C#: `Buttons/Button.Variants.cs:157,197,211` | B | `DangerFilledBg` |
| `ResetButtonToken` | `ColorErrorBgHover` | Shadcn | C#: `Buttons/Button.Variants.cs:158,198` | B | `DangerFilledHoverBg` |
| `ResetButtonToken` | `ColorErrorHover` | PrimeNG | C#: `Buttons/Button.Variants.cs:148` | B | `DangerHoverColor` |
| `ResetButtonToken` | `ColorFillSecondary` | Shadcn | C#: `Buttons/Button.Variants.cs:295` | B | `FilledHoverBg` |
| `ResetButtonToken` | `ColorFillTertiary` | Shadcn | 计算: TextHoverBg（AbstractButtonVariantToken.cs:146）<br>C#: `Buttons/Button.Variants.cs:294` | B | `FilledBg` |
| `ResetButtonToken` | `ColorFocusBorder` | Shadcn | C#: `AdornerLayer/FocusOutlineAdorner.cs:150（仅 Button.UsesThemeRecipe=true）` | B | `FocusBorderColor` |
| `ResetButtonToken` | `ColorLink` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:318` | B | `LinkColor` |
| `ResetButtonToken` | `ColorLinkActive` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:322（非 Primary 色时）` | B | `LinkActiveColor` |
| `ResetButtonToken` | `ColorLinkHover` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:319` | B | `LinkHoverColor` |
| `ResetButtonToken` | `ControlItemColorActive` | Shadcn | C#: `Buttons/Button.Variants.cs:289（仅 RecipeVariants=true 时作为 DefaultColor 的回退）` | B | `FilledTextColor` |
| `ResetButtonToken` | `EnableWaveSpirit` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:241（Owner）` | B† | `WaveSpiritEnabled` |
| `ResetButtonToken` | `LineWidthFocus` | Shadcn | C#: `AdornerLayer/FocusOutlineAdorner.cs:151（仅 Button.UsesThemeRecipe=true）` | B | `FocusLineWidth` |
| `ResetButtonToken` | `OpacityLoading` | PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:362（Owner）` | B† | `LoadingOpacity` |
| `SearchEditToken` | `BorderRadiusLG` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42` | B† | `InputBorderRadiusLG` |
| `SearchEditToken` | `BorderRadiusSM` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49` | B† | `InputBorderRadiusSM` |
| `SearchEditToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `SearchEditToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `SearchEditToken` | `ColorIcon` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:18` | B† | `IconColor` |
| `SearchEditToken` | `ColorIconHover` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:22` | B† | `IconHoverColor` |
| `SearchEditToken` | `ColorPrimaryHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `SearchEditToken` | `ColorTextDisabled` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[REVEAL] `RevealButtonTheme.axaml:32`；`Input/Themes/SearchButtonTheme.axaml:34`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `SearchEditToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22` | B† | `SuffixIconColor` |
| `SearchEditToken` | `InputActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `SearchEditToken` | `InputErrorActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `SelectToken` | `BorderRadius` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:45`；[TEXTBOX] `TextBoxTheme.axaml:162,172,182`；[SELECTLIST] `ListViewTheme.axaml:116` | B† | `InputBorderRadius` |
| `SelectToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42`；[TEXTBOX] `TextBoxTheme.axaml:157,178`；[SELECTLIST] `ListViewTheme.axaml:111` | B† | `InputBorderRadiusLG` |
| `SelectToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49`；[TEXTBOX] `TextBoxTheme.axaml:167,186`；[SELECTLIST] `ListViewTheme.axaml:121` | B† | `InputBorderRadiusSM` |
| `SelectToken` | `ColorBgContainer` | Shadcn | C#: [FRAME] `InputControlFrame.cs:590` | B | `DefaultBg` |
| `SelectToken` | `ColorBorder` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:145,157,161`；[ADDON] `AddOnDecoratedBoxTheme.axaml:112,117`<br>C#: [FRAME] `InputControlFrame.cs:588,608` | B† | `DefaultBorderColor` |
| `SelectToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `SelectToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116`；[SELECTLIST] `GroupHeaderItemTheme.axaml:14` | B† | `AddonBg` |
| `SelectToken` | `ColorIcon` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:18` | B† | `IconColor` |
| `SelectToken` | `ColorIconHover` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:22` | B† | `IconHoverColor` |
| `SelectToken` | `ColorPrimary` | Shadcn | 计算: OptionSelectedIndicatorColor（SelectToken.cs:101）<br>AXAML: `Select/Themes/SelectAddOnDecoratedBoxTheme.axaml:59`<br>C#: [FRAME] `InputControlFrame.cs:593,604,612` | B | `ActiveBorderColor` |
| `SelectToken` | `ColorPrimaryHover` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `SelectToken` | `ColorTextDisabled` | PrimeNG | 计算: MultipleItemColorDisabled（AbstractSelectInputToken.cs:125）<br>AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[REVEAL] `RevealButtonTheme.axaml:32`；`Select/Themes/SelectTheme.axaml:261`；`Select/Themes/SelectCandidateListItemTheme.axaml:89`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `SelectToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22`；`Select/Themes/SelectTheme.axaml:84（元素语法 Kind=）` | B† | `SuffixIconColor` |
| `SelectToken` | `InputActiveShadow` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `SelectToken` | `InputErrorActiveShadow` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `SelectToken` | `InputWarningActiveShadow` | Shadcn | C#: [FRAME] `InputControlFrame.cs:596` | B | `WarningActiveShadow` |
| `SeparatorToken` | `ColorSplit` | PrimeNG | AXAML: `Separator/Themes/SeparatorTheme.axaml:37` | B | `LineColor` |
| `SimplePaginationToken` | `InputPaddingSM` | Shadcn | AXAML: `Pagination/Themes/SimplePaginationTheme.axaml:61` | B | `QuickJumperInputPaddingSM` |
| `SimplePaginationToken` | `UniformlyMarginXS` | PrimeNG | 计算: PaginationLayoutSpacing（AbstractPaginationToken.cs:48） | A |  |
| `SkeletonToken` | `BorderRadiusSM` | PrimeNG | 计算: BlockRadius（AtomUI.Controls/Skeleton/AbstractSkeletonToken.cs:51） | A |  |
| `SplitButtonToken` | `BorderRadius` | Shadcn | AXAML: `Buttons/Themes/SplitButtonTheme.axaml:93,97` | B | `SplitButtonBorderRadius` |
| `SplitButtonToken` | `BorderRadiusLG` | Shadcn | AXAML: `Buttons/Themes/SplitButtonTheme.axaml:89` | B | `SplitButtonBorderRadiusLG` |
| `SplitButtonToken` | `BorderRadiusSM` | Shadcn | AXAML: `Buttons/Themes/SplitButtonTheme.axaml:101` | B | `SplitButtonBorderRadiusSM` |
| `SplitButtonToken` | `EnableWaveSpirit` | Shadcn | AXAML: `Buttons/Themes/SplitButtonTheme.axaml:59` | B | `WaveSpiritEnabled` |
| `SubmitButtonToken` | `BorderRadius` | Shadcn | AXAML: `Buttons/Themes/ButtonTheme.axaml:263,290（Owner）` | B† | `ButtonBorderRadius` |
| `SubmitButtonToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:249（Owner）` | B† | `ButtonBorderRadiusLG` |
| `SubmitButtonToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:277（Owner）` | B† | `ButtonBorderRadiusSM` |
| `SubmitButtonToken` | `ColorBgSolid` | PrimeNG | 计算: SolidTextColor（AbstractButtonVariantToken.cs:157，经 isBright）<br>C#: `Buttons/Button.Variants.cs:245,248` | B | `SolidBg` |
| `SubmitButtonToken` | `ColorBgSolidActive` | PrimeNG | C#: `Buttons/Button.Variants.cs:247,250` | B | `SolidActiveBg` |
| `SubmitButtonToken` | `ColorBgSolidHover` | PrimeNG | C#: `Buttons/Button.Variants.cs:246,249` | B | `SolidHoverBg` |
| `SubmitButtonToken` | `ColorError` | PrimeNG | C#: `Buttons/Button.Variants.cs:147`；`AdornerLayer/FocusOutlineAdorner.cs:153（仅 UsesThemeRecipe=true；PrimeNG 不满足）`<br>EvaluateOwn（语言阶段类）: PrimeNG PrimeNGButtonTokens.cs `DangerColor`（暗色）经 PrimeNGSeverity.Error → PrimeNGDataDisplayTokens.cs:26 读 ColorError | B；EvaluateOwn 依赖 | `DangerBaseColor` |
| `SubmitButtonToken` | `ColorErrorActive` | PrimeNG | C#: `Buttons/Button.Variants.cs:149` | B | `DangerActiveColor` |
| `SubmitButtonToken` | `ColorErrorBg` | Shadcn | C#: `Buttons/Button.Variants.cs:157,197,211` | B | `DangerFilledBg` |
| `SubmitButtonToken` | `ColorErrorBgHover` | Shadcn | C#: `Buttons/Button.Variants.cs:158,198` | B | `DangerFilledHoverBg` |
| `SubmitButtonToken` | `ColorErrorHover` | PrimeNG | C#: `Buttons/Button.Variants.cs:148` | B | `DangerHoverColor` |
| `SubmitButtonToken` | `ColorFillSecondary` | Shadcn | C#: `Buttons/Button.Variants.cs:295` | B | `FilledHoverBg` |
| `SubmitButtonToken` | `ColorFillTertiary` | Shadcn | 计算: TextHoverBg（AbstractButtonVariantToken.cs:146）<br>C#: `Buttons/Button.Variants.cs:294` | B | `FilledBg` |
| `SubmitButtonToken` | `ColorFocusBorder` | Shadcn | C#: `AdornerLayer/FocusOutlineAdorner.cs:150（仅 Button.UsesThemeRecipe=true）` | B | `FocusBorderColor` |
| `SubmitButtonToken` | `ColorLink` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:318` | B | `LinkColor` |
| `SubmitButtonToken` | `ColorLinkActive` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:322（非 Primary 色时）` | B | `LinkActiveColor` |
| `SubmitButtonToken` | `ColorLinkHover` | Shadcn+PrimeNG | C#: `Buttons/Button.Variants.cs:319` | B | `LinkHoverColor` |
| `SubmitButtonToken` | `ControlItemColorActive` | Shadcn | C#: `Buttons/Button.Variants.cs:289（仅 RecipeVariants=true 时作为 DefaultColor 的回退）` | B | `FilledTextColor` |
| `SubmitButtonToken` | `EnableWaveSpirit` | Shadcn+PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:241（Owner）` | B† | `WaveSpiritEnabled` |
| `SubmitButtonToken` | `LineWidthFocus` | Shadcn | C#: `AdornerLayer/FocusOutlineAdorner.cs:151（仅 Button.UsesThemeRecipe=true）` | B | `FocusLineWidth` |
| `SubmitButtonToken` | `OpacityLoading` | PrimeNG | AXAML: `Buttons/Themes/ButtonTheme.axaml:362（Owner）` | B† | `LoadingOpacity` |
| `TabControlToken` | `ColorBorderSecondary` | PrimeNG | C#: `TabControl/BaseTabControl.cs:892（UseSeparatorTokenResources=True，BaseTabControlTheme.axaml:9）` | B | `SeparatorColor` |
| `TabControlToken` | `MotionDurationSlow` | PrimeNG | AXAML: `TabControl/Themes/TabControlTheme.axaml:218（TT）` | B† | `InkBarMotionDuration` |
| `TabStripToken` | `ColorBorderSecondary` | PrimeNG | AXAML: `TabControl/Themes/TabStrip/BaseTabStripTheme.axaml:14（Owner）` | B† | `SeparatorColor` |
| `TabStripToken` | `MotionDurationSlow` | PrimeNG | AXAML: `TabControl/Themes/TabStrip/TabStripTheme.axaml:194（TT）` | B† | `InkBarMotionDuration` |
| `TagToken` | `BorderRadiusSM` | PrimeNG | AXAML: `Tag/Themes/TagTheme.axaml:58` | B | `TagBorderRadius` |
| `TagToken` | `ColorError` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:396（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `ErrorColor` |
| `TagToken` | `ColorErrorBg` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:396（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `ErrorBg` |
| `TagToken` | `ColorErrorBorder` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:396（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `ErrorBorderColor` |
| `TagToken` | `ColorInfo` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:392（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `InfoColor` |
| `TagToken` | `ColorInfoBg` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:392（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `InfoBg` |
| `TagToken` | `ColorInfoBorder` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:392（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `InfoBorderColor` |
| `TagToken` | `ColorSuccess` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:389（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `SuccessColor` |
| `TagToken` | `ColorSuccessBg` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:389（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `SuccessBg` |
| `TagToken` | `ColorSuccessBorder` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:389（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `SuccessBorderColor` |
| `TagToken` | `ColorWarning` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:394（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `WarningColor` |
| `TagToken` | `ColorWarningBg` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:394（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `WarningBg` |
| `TagToken` | `ColorWarningBorder` | PrimeNG | C#: `AtomUI.Controls/Tag/AbstractTag.cs:394（经 :354-355 按 Tag 的 slot 读 Effective Global）` | B | `WarningBorderColor` |
| `TextAreaToken` | `BorderRadius` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:45` | B† | `InputBorderRadius` |
| `TextAreaToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42` | B† | `InputBorderRadiusLG` |
| `TextAreaToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49` | B† | `InputBorderRadiusSM` |
| `TextAreaToken` | `ColorBgContainer` | Shadcn | C#: [FRAME] `InputControlFrame.cs:590` | B | `DefaultBg` |
| `TextAreaToken` | `ColorBorder` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:145,157,161`；[ADDON] `AddOnDecoratedBoxTheme.axaml:112,117`<br>C#: [FRAME] `InputControlFrame.cs:588,608` | B† | `DefaultBorderColor` |
| `TextAreaToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `TextAreaToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `TextAreaToken` | `ColorIcon` | PrimeNG | 无 | C |  |
| `TextAreaToken` | `ColorIconHover` | PrimeNG | 无 | C |  |
| `TextAreaToken` | `ColorPrimary` | Shadcn | C#: [FRAME] `InputControlFrame.cs:593,604,612` | B | `ActiveBorderColor` |
| `TextAreaToken` | `ColorPrimaryHover` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `TextAreaToken` | `ColorTextDisabled` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[TEXTAREA] `TextAreaTheme.axaml:239`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `TextAreaToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22` | B† | `SuffixIconColor` |
| `TextAreaToken` | `InputActiveShadow` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `TextAreaToken` | `InputErrorActiveShadow` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `TextAreaToken` | `InputWarningActiveShadow` | Shadcn | C#: [FRAME] `InputControlFrame.cs:596` | B | `WarningActiveShadow` |
| `TimePickerToken` | `BorderRadius` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:45`；[TEXTBOX] `TextBoxTheme.axaml:162,172,182` | B† | `InputBorderRadius` |
| `TimePickerToken` | `BorderRadiusLG` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42`；[TEXTBOX] `TextBoxTheme.axaml:157,178` | B† | `InputBorderRadiusLG` |
| `TimePickerToken` | `BorderRadiusSM` | Shadcn+PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49`；[TEXTBOX] `TextBoxTheme.axaml:167,186`；[TIMECELL] `TimeViewCellTheme.axaml:50` | B† | `InputBorderRadiusSM` |
| `TimePickerToken` | `ColorBgContainer` | Shadcn | C#: [FRAME] `InputControlFrame.cs:590` | B | `DefaultBg` |
| `TimePickerToken` | `ColorBorder` | Shadcn | AXAML: [FRAME] `InputControlFrameTheme.axaml:145,157,161`；[ADDON] `AddOnDecoratedBoxTheme.axaml:112,117`<br>C#: [FRAME] `InputControlFrame.cs:588,608` | B† | `DefaultBorderColor` |
| `TimePickerToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `TimePickerToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `TimePickerToken` | `ColorIcon` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:18` | B† | `IconColor` |
| `TimePickerToken` | `ColorIconHover` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:22` | B† | `IconHoverColor` |
| `TimePickerToken` | `ColorPrimary` | Shadcn | C#: [FRAME] `InputControlFrame.cs:593,604,612` | B | `ActiveBorderColor` |
| `TimePickerToken` | `ColorPrimaryHover` | Shadcn+PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `TimePickerToken` | `ColorTextDisabled` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[REVEAL] `RevealButtonTheme.axaml:32`；[PICKER] `InfoPickerInputTheme.axaml:133`；[TIMECELL] `TimeViewCellTheme.axaml:73`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `TimePickerToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22`；[PICKER] `PickerClearUpButtonTheme.axaml:40` | B† | `SuffixIconColor` |
| `TimePickerToken` | `InputActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `TimePickerToken` | `InputErrorActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `ToggleSwitchToken` | `ColorPrimary` | Shadcn | 计算: SwitchColor（ToggleSwitchToken.cs:239）<br>AXAML: `Primitives/Themes/WaveSpiritDecoratorTheme.axaml:7（Owner；仅当 AbstractToggleSwitch.ConfigureWaveSpiritBrush 未设本地 WaveBrush 时生效）` | B† | `WaveColor` |
| `ToggleSwitchToken` | `ColorPrimaryHover` | Shadcn | 计算: TrackCheckedHoverBg（ToggleSwitchToken.cs:226） | A |  |
| `ToggleSwitchToken` | `ColorTextLightSolid` | Shadcn | 计算: UncheckedContentColor（ToggleSwitchToken.cs:240）<br>AXAML: `Switch/Themes/ToggleSwitchTheme.axaml:44` | B | `ContentColor` |
| `ToggleSwitchToken` | `MotionDurationMid` | PrimeNG | AXAML: `Switch/Themes/ToggleSwitchTheme.axaml:64,65,66,67,68,69（TT）` | B† | `MotionDuration` |
| `ToolTipToken` | `ControlHeight` | PrimeNG | AXAML: `Tooltip/Themes/ToolTipTheme.axaml:38` | B | `ToolTipMinHeight` |
| `ToolTipToken` | `FontSize` | PrimeNG | AXAML: `Tooltip/Themes/ToolTipTheme.axaml:35` | B | `ToolTipFontSize` |
| `TreeSelectToken` | `BorderRadiusLG` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:42`；[TEXTBOX] `TextBoxTheme.axaml:157,178` | B† | `InputBorderRadiusLG` |
| `TreeSelectToken` | `BorderRadiusSM` | PrimeNG | AXAML: [FRAME] `InputControlFrameTheme.axaml:49`；[TEXTBOX] `TextBoxTheme.axaml:167,186` | B† | `InputBorderRadiusSM` |
| `TreeSelectToken` | `ColorErrorBorderHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `ErrorHoverBorderColor` |
| `TreeSelectToken` | `ColorFillAlter` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:111,116` | B† | `AddonBg` |
| `TreeSelectToken` | `ColorIcon` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:18` | B† | `IconColor` |
| `TreeSelectToken` | `ColorIconHover` | PrimeNG | AXAML: [REVEAL] `RevealButtonTheme.axaml:22` | B† | `IconHoverColor` |
| `TreeSelectToken` | `ColorPrimaryHover` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:591,610` | B | `HoverBorderColor` |
| `TreeSelectToken` | `ColorTextDisabled` | PrimeNG | 计算: MultipleItemColorDisabled（AbstractSelectInputToken.cs:125）<br>AXAML: [FRAME] `InputControlFrameTheme.axaml:139`；[REVEAL] `RevealButtonTheme.axaml:32`；[TAGBOX] `SelectTagAwareTextBoxTheme.axaml:31`；[TREEPOPUP] `TreeViewItemHeaderTheme.axaml:236,240`；[TREEPOPUP] `NodeSwitcherButtonTheme.axaml:49`；`TreeSelect/Themes/TreeSelectTheme.axaml:231`<br>C#: [ADDON] `AddOnDecoratedBox.cs:369` | B† | `DisabledColor` |
| `TreeSelectToken` | `ColorTextQuaternary` | PrimeNG | AXAML: [ADDON] `AddOnDecoratedBoxTheme.axaml:128`；[CLEAR] `InputClearIconButtonTheme.axaml:22` | B† | `SuffixIconColor` |
| `TreeSelectToken` | `InputActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:595` | B | `ActiveShadow` |
| `TreeSelectToken` | `InputErrorActiveShadow` | PrimeNG | C#: [FRAME] `InputControlFrame.cs:596` | B | `ErrorActiveShadow` |
| `TreeViewToken` | `MotionDurationSlow` | PrimeNG | AXAML: `TreeView/Themes/TreeViewTheme.axaml:68（Owner）` | B† | `MotionDuration` |
| `UploadToken` | `BorderRadiusLG` | PrimeNG | AXAML: `Upload/Themes/AbstractUploadPictureContentTheme.axaml:10`；`Upload/Themes/UploadTriggerTheme.axaml:38`；`Upload/Themes/UploadDefaultDropAreaTheme.axaml:30` | B | `TriggerBorderRadius` |
| `UploadToken` | `ColorBorder` | PrimeNG | AXAML: `Upload/Themes/AbstractUploadPictureContentTheme.axaml:11`；`Upload/Themes/UploadTriggerTheme.axaml:39`；`Upload/Themes/UploadDefaultDropAreaTheme.axaml:31` | B | `TriggerBorderColor` |
| `UploadToken` | `ColorFillAlter` | PrimeNG | AXAML: `Upload/Themes/UploadTriggerTheme.axaml:37`；`Upload/Themes/UploadDefaultDropAreaTheme.axaml:29` | B | `TriggerBg` |

## 4. 合计

- Table 1：169 行（覆盖 125 行 `controlGlobal.X =`、8 行 `ShadcnButtonAppearance` 的 `token.X =`、9 行元组写入）；500 个“语言 × Token 类 × 全局 Token”写入对。
- Table 2：442 行（Token 类 × 全局 Token，跨语言合并），涉及 56 个控件 Token 类。
- 去向：A = 10，B = 376（其中 148 行标 †：至少一处读取是只接受 Global 的 AXAML 形式——`OwnerTokenResource` 或 `TokenTransitions.Duration`），C = 56。
- 另有 30 行带“EvaluateOwn 依赖”（不改变 A/B/C 判定，但删除写入前必须先改 `EvaluateOwn`）：`AlertToken.ColorError`、`AlertToken.ColorInfo`、`AlertToken.ColorSuccess`、`AlertToken.ColorWarning`、`ButtonToken.ColorError`、`CardToken.ColorBgContainer`、`DropdownButtonToken.ColorError`、`FloatButtonToken.ColorBorder`、`MessageCardToken.ColorErrorBg`、`MessageCardToken.ColorErrorText`、`MessageCardToken.ColorInfoBg`、`MessageCardToken.ColorInfoText`、`MessageCardToken.ColorSuccessBg`、`MessageCardToken.ColorSuccessText`、`MessageCardToken.ColorWarningBg`、`MessageCardToken.ColorWarningText`、`NotificationCardToken.ColorErrorBg`、`NotificationCardToken.ColorErrorBorder`、`NotificationCardToken.ColorErrorText`、`NotificationCardToken.ColorInfoBg`、`NotificationCardToken.ColorInfoBorder`、`NotificationCardToken.ColorInfoText`、`NotificationCardToken.ColorSuccessBg`、`NotificationCardToken.ColorSuccessBorder`、`NotificationCardToken.ColorSuccessText`、`NotificationCardToken.ColorWarningBg`、`NotificationCardToken.ColorWarningBorder`、`NotificationCardToken.ColorWarningText`、`ResetButtonToken.ColorError`、`SubmitButtonToken.ColorError`。
- 新增 Own Token：376 个（Token 类 × 名称）；按 Token 类去重后的名称 110 种。若按共享基类声明（如 `AbstractButtonGeometryToken` 一次覆盖 Button/DropdownButton/SubmitButton/ResetButton，`AbstractSelectInputToken`、`AbstractDatePickerToken` 等），实际新增属性数会更少。
- B 类需要修改的 AXAML / C# 文件（92 个）：

  - `AdornerLayer/FocusOutlineAdorner.cs`
  - `Alert/Themes/AlertTheme.axaml`
  - `AtomUI.Controls/Avatar/Themes/AvatarTheme.axaml`
  - `AtomUI.Controls/Tag/AbstractTag.cs`
  - `ButtonSpinner/Themes/ButtonSpinnerTheme.axaml`
  - `Buttons/Button.Variants.cs`
  - `Buttons/Themes/ButtonTheme.axaml`
  - `Buttons/Themes/DropdownButtonBaseTheme.axaml`
  - `Buttons/Themes/SplitButtonTheme.axaml`
  - `Card/Themes/CardActionPanelTheme.axaml`
  - `Card/Themes/CardGridContentTheme.axaml`
  - `Card/Themes/CardTheme.axaml`
  - `Carousel/Themes/CarouselTheme.axaml`
  - `Cascader/Themes/CascaderAddOnDecoratedBoxTheme.axaml`
  - `Cascader/Themes/CascaderTheme.axaml`
  - `CheckBox/Themes/CheckBoxTheme.axaml`
  - `Collapse/Themes/CollapseItemTheme.axaml`
  - `Collapse/Themes/CollapseTheme.axaml`
  - `DateViewer/Themes/DateViewerCellTheme.axaml`
  - `Descriptions/Themes/DescriptionBorderedItemContentTheme.axaml`
  - `Descriptions/Themes/DescriptionBorderedItemLabelTheme.axaml`
  - `Descriptions/Themes/DescriptionDefaultItemTheme.axaml`
  - `Descriptions/Themes/DescriptionsTheme.axaml`
  - `Dialog/OverlayHost/OverlayDialogPresenter.cs`
  - `Dialog/Themes/DialogCaptionButtonTheme.axaml`
  - `Dialog/Themes/DialogSurfaceTheme.axaml`
  - `Dialog/Themes/OverlayDialogHeaderTheme.axaml`
  - `Drawer/Themes/DrawerContainerTheme.axaml`
  - `Drawer/Themes/DrawerInfoContainerTheme.axaml`
  - `Empty/Empty.cs`
  - `Expander/Themes/ExpanderTheme.axaml`
  - `FloatButton/Themes/AbstractFloatButtonTheme.axaml`
  - `FloatButton/Themes/FloatButtonItemsControlTheme.axaml`
  - `Flyouts/Flyout.cs`
  - `Flyouts/Themes/FlyoutHostTheme.axaml`
  - `GroupBox/Themes/GroupBoxTheme.axaml`
  - `Input/Themes/InputClearIconButtonTheme.axaml`
  - `Input/Themes/RevealButtonTheme.axaml`
  - `Input/Themes/SearchButtonTheme.axaml`
  - `Input/Themes/TextAreaTheme.axaml`
  - `Input/Themes/TextBoxTheme.axaml`
  - `ListBox/Themes/ListBoxTheme.axaml`
  - `ListView/Themes/GroupHeaderItemTheme.axaml`
  - `ListView/Themes/ListViewTheme.axaml`
  - `Menu/Themes/ContextMenuTheme.axaml`
  - `Message/Themes/MessageCardTheme.axaml`
  - `Message/Themes/WindowMessageManagerTheme.axaml`
  - `NavMenu/NavMenu.cs`
  - `NavMenu/Themes/BaseNavMenuItemHeaderTheme.axaml`
  - `NavMenu/Themes/HorizontalNavMenuItemHeaderTheme.axaml`
  - `NavMenu/Themes/InlineNavMenuItemHeaderTheme.axaml`
  - `NavMenu/Themes/NavMenuItemTheme.axaml`
  - `Notifications/Themes/NotificationCardTheme.axaml`
  - `OtpLineEdit/Themes/OtpLineEditCellTheme.axaml`
  - `Pagination/Themes/PaginationNavTheme.axaml`
  - `Pagination/Themes/SimplePaginationTheme.axaml`
  - `Popup/Themes/PopupTheme.axaml`
  - `PopupConfirm/Themes/PopupConfirmContainerTheme.axaml`
  - `PopupConfirm/Themes/PopupConfirmTheme.axaml`
  - `Primitives/AddOnDecoratedBox/AddOnDecoratedBox.cs`
  - `Primitives/AddOnDecoratedBox/Themes/AddOnDecoratedBoxTheme.axaml`
  - `Primitives/AddOnDecoratedBox/Themes/InputControlFrameTheme.axaml`
  - `Primitives/CandidateList/Themes/CandidateListItemTheme.axaml`
  - `Primitives/CandidateList/Themes/CandidateListTheme.axaml`
  - `Primitives/InfoPickerInput/Themes/InfoPickerInputTheme.axaml`
  - `Primitives/InfoPickerInput/Themes/PickerClearUpButtonTheme.axaml`
  - `Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.axaml`
  - `Primitives/InputControlFrame.cs`
  - `Primitives/Themes/WaveSpiritDecoratorTheme.axaml`
  - `QRCode/Themes/QRCodeTheme.axaml`
  - `Select/Themes/SelectAddOnDecoratedBoxTheme.axaml`
  - `Select/Themes/SelectCandidateListItemTheme.axaml`
  - `Select/Themes/SelectTagAwareTextBoxTheme.axaml`
  - `Select/Themes/SelectTheme.axaml`
  - `Separator/Themes/SeparatorTheme.axaml`
  - `Switch/Themes/ToggleSwitchTheme.axaml`
  - `TabControl/BaseTabControl.cs`
  - `TabControl/CardTabControl.cs`
  - `TabControl/TabStrip/CardTabStrip.cs`
  - `TabControl/Themes/TabControlTheme.axaml`
  - `TabControl/Themes/TabStrip/BaseTabStripTheme.axaml`
  - `TabControl/Themes/TabStrip/TabStripTheme.axaml`
  - `Tag/Themes/TagTheme.axaml`
  - `TimePicker/Themes/TimeViewCellTheme.axaml`
  - `Tooltip/Themes/ToolTipTheme.axaml`
  - `TreeSelect/Themes/TreeSelectTheme.axaml`
  - `TreeView/Themes/NodeSwitcherButtonTheme.axaml`
  - `TreeView/Themes/TreeViewItemHeaderTheme.axaml`
  - `TreeView/Themes/TreeViewTheme.axaml`
  - `Upload/Themes/AbstractUploadPictureContentTheme.axaml`
  - `Upload/Themes/UploadDefaultDropAreaTheme.axaml`
  - `Upload/Themes/UploadTriggerTheme.axaml`

## 5. 不确定项

1. **工作树并发改动**：扫描期间 `src/AtomUI.Desktop.Controls/Buttons/Button.Variants.cs`、`AdornerLayer/FocusOutlineAdorner.cs`、`Primitives/InputControlFrame.cs`、`PrimeNGButtonTokens.cs`、`ShadcnButtonTokens.cs`、`ShadcnInputTokens.cs` 等文件正被他人修改（AppearanceSettings 拆为行为 Own Token），其中 `InputControlFrame.cs` 行号在扫描中途整体后移约 31 行。本表行号已按最后一次读取更新，但执行改造前应按内容（Token 名）重新定位，而不是只信行号。`EvaluateGlobal` 的写入内容在这些改动中未变。
2. **B 类中的共享基元无法“改读新 Own Token”**：FRAME/ADDON/CLEAR/REVEAL/TEXTBOX/PICKER/DATECELL/TIMECELL/TEXTAREA/CANDIDATE/SELECTLIST/TAGBOX/TREEPOPUP/SPINNER、`ButtonTheme.axaml`（Button/SubmitButton/ResetButton 共用）、`WaveSpiritDecoratorTheme.axaml`、`BaseTabStripTheme.axaml`、`TreeViewTheme.axaml`、`AbstractFloatButtonTheme.axaml:63`、`DialogSurfaceTheme`/`DialogCaptionButtonTheme`/`OverlayDialogHeaderTheme`，以及所有 `TokenTransitions.Duration` 与 `OwnerTokenSet.SetKind` 读取都只接受 Global Token。要让这些读取改走 Own Token，需要（a）为基元引入“按 owner 读 Own Token”的机制，或（b）由各 owner 主题以样式覆盖基元属性（按控件逐个加 Setter），或（c）把基元改成读 owner 的主题输入属性。本清单仅列出位置，不替你选择方案；同一新 Own Token 名需在所有拥有该基元的 Token 类上一致声明。
3. **Owner 作用域的展开靠模板静态分析**：代码中动态创建的弹层/项容器（Select 的 `SelectCandidateList`、TreeSelect 的 `TreeSelectTreeView`、Dialog 的 `OverlayDialogPresenter`/`DialogSurface`、各 ItemsControl 的容器）按源码阅读补入；未逐一运行验证 `ThemeScope.TokenOwner` 的实际值。特别是：`OverlayDialogPresenter.cs:500/501` 的 owner 假定为 Dialog/MessageBox；MessageBox 的 `DialogSurface` 经 `TokenOwnerForwarding.Forward(dialog, …)` 得到 MessageBox identity 为推断；DatePicker 弹层单元格使用默认 `DateViewerCellTheme`（`DatePanel.CellTheme` 为空时）为推断。
4. **StyleKeyOverride 屏障的结论需要一次运行时确认**：`AutoCompleteLineEditBox`/`AutoCompleteSearchEditBox` 以 LineEdit/SearchEdit 为 StyleKey，按注入逻辑（`OfType(LineEdit)` 匹配 StyleKey）其 owner 为 LineEdit/SearchEdit。由此 PrimeNG 对 AutoComplete / AutoCompleteSearchEdit 写入的大多数输入框全局（以及 Shadcn 对 AutoComplete 写入的 ColorPrimaryHover/ColorBorder/ColorBgContainer）判为 C。若运行时 owner 实际是 AutoComplete，这些行应改判为 B（读取者同 LineEdit / SearchEdit 一行）。
5. **被遮蔽或条件生效的读取者仍按读取者计**：例如 `WaveSpiritDecoratorTheme.axaml:7` 只在 ToggleSwitch 未设本地 `WaveBrush` 时生效；`FocusOutlineAdorner.cs:150/151/153` 只在 `Button.UsesThemeRecipe`（`RecipeVariants`）为 true 时读按钮作用域（Shadcn 满足，PrimeNG 不满足，所以 PrimeNG 的 `ColorError` 在此处并不生效）；`Button.Variants.cs:289` 只在 RecipeVariants 时生效；`GroupHeaderItemTheme.axaml:14` 只在 Select 分组选项时出现；`REVEAL` 在非密码框中不可见；TEXTBOX 内 `EmbeddedTextBoxTheme` 可能以更高优先级覆盖部分属性。是否可因“永不可见/被覆盖”而降级为 C，需逐项视觉验证，本表未降级。
6. **颜色/尺寸“值相同”未做判定**：部分写入在某些风格下与原值相同（如 PrimeNG Input 在 Aura/Nora 下 `ColorFillAlter = ColorBgContainer`），本表不据此降级。
7. **Motion 写入的条件**：`PrimeNGMotion.Restore` 只在 `EnableMotion` 且预设 `TransitionDuration == 0`（仅 Nora / Nora.Dark）时写入；Aura/Lara 下这些行实际不写。去向按读取者给出，与条件无关。
8. **未纳入的读取路径**：未检查 Gallery、测试与第三方包；`HasExplicitThemeResource` / `ExplicitTokenLookup`（InputControlFrame.cs:361-367、FocusOutlineAdorner.cs 5-8 号槽、Button.Variants `OwnOverride`）只探测用户显式配置，不读阶段写入值，未计为读取者。`ShadcnButtonAppearance.ResolveOutline` 在 FloatButton 的 `EvaluateGlobal` 中读的是改写前的 ColorBorder、在 `EvaluateOwn` 中读的是改写后的 ColorBorder，二者在 `darkInputOutline` 分支下结果相同、在其余分支下 EvaluateOwn 会读到自身写入值——迁移时需保持该计算顺序。
9. **新 Own Token 名仅为建议**：遵循邻近命名（如 `CollapsePanelBorderRadius`→`CollapsePanelBorderColor`、Ant Input 的 `ActiveBorderColor`/`HoverBorderColor`/`ActiveShadow`/`AddonBg`）；需与 Own/Global 禁止同名规则及现有属性（如 `ToolTipToken.MotionDuration`、`AbstractButtonVariantToken.DangerColor`/`SolidTextColor`）再核对一次。按钮族四个 Token 类、输入族多个 Token 类应在共同基类上声明同名 Token。
