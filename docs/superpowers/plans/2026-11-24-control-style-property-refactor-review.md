# 控件样式属性重构确认清单

2026-11-24 · 已实施，定向测试通过，墙钟耗时仍有测量不确定性

依据 [控件属性形态与命名](../../engineering/development/control-development-guidelines.md#控件属性形态与命名) 和 [控件样式属性 API 与回归验收方案](../specs/2026-11-24-control-style-property-naming-design.md)。

## 1. 确认范围

现有源码中盘点到 14 个定义文件、156 项 Avalonia 属性声明。拟改 149 个内部旧注册名，其中 2 项同时形成标准公开属性。FloatButton 的 7 项语义名称沿用现状，迁移其注册类名。定义文件之外检出 91 个文件命中，含 78 个生产源文件、7 个已有测试和 6 个正式或历史文档。末节区分直接迁移、复核和历史引用。

本轮范围由下表的公开入口、第 3 节文件清单、第 4 节逐项命名和第 5 节引用索引组成。以现有工作区为实施位置。本文件记录确认范围与实施验收结果。

## 2. 建议公开的 API

| 公开入口 | 类型和实际语义 | 注册及默认值 |
| --- | --- | --- |
| `Button.ContentPadding`，SubmitButton 和 ResetButton 继承 | `Thickness`，普通内容状态采用的内边距。另有圆形、仅图标状态及显式 `Padding` 的现有规则。 | 在 `Button.cs` 公共属性区复用现有一项 StyledProperty 注册。Button、SubmitButton、ResetButton 继续按各自现有 Token 和尺寸分支提供主题默认值。 |
| `ColorPickerView.SliderTrackHeight`、`GradientColorPickerView.SliderTrackHeight` | `double`，模板中滑轨绘制的高度。零值和清除覆盖的行为以现有轨道 Height 规则验收。 | 在现有 `AbstractColorPickerView.cs` 公共属性区使用重命名后的内部附加属性 `AddOwner<AbstractColorPickerView>()`，声明为普通 `StyledProperty<double>` 和 CLR 包装。两个视图主题在视图上设置同一个公开属性，滑轨编译绑定读取同一注册的继承值。 |

Avalonia 12.1.6 的本地参考源码及当前 NuGet XML 说明表明，`AttachedProperty.AddOwner` 将同一属性作为普通属性注册到指定类型并返回同一个实例。首批须验证正常 AXAML、Style Setter、C# 和绑定都能生效，并断言普通入口与内部继承入口是同一属性。这样能沿用现有传值路径和观察者数量。若编译或实际优先级证明此做法无法成立，先回报该项证据，再评审替代路径。

待后续审议的公开范围包括 `Button.CirclePadding`、`Button.IconOnlyPadding`、`SpectrumHeight`、`SliderThumbSize` 以及外层弹出式 ColorPicker 和 GradientColorPicker 的转接入口。本清单建议这些项目本批仅做表中约定的内部迁移。其他家族的属性沿用现有公开控件、Token 或 Semantic Part 定制入口。

## 3. 批次和会增加的修改点

| 批次 | 组别 | 除直接引用索引外的修改点和验收 |
| --- | --- | --- |
| 1 | Button、Button 固定样式值、取色器 | 新增入口位于 [Button.cs](../../../src/AtomUI.Desktop.Controls/Buttons/Button.cs) 和 [AbstractColorPickerView.cs](../../../src/AtomUI.Desktop.Controls.ColorPicker/ColorView/AbstractColorPickerView.cs)。修订 Button 和 ColorPicker 的 overview、implementation、changelog，Token 文档更新实际接线。Gallery [ButtonShowCase](../../../controlgallery/AtomUIGallery/ShowCases/General/Button/Views/ButtonShowCase.axaml) 和 [ColorPickerShowCase](../../../controlgallery/AtomUIGallery/ShowCases/DataEntry/ColorPicker/Views/ColorPickerShowCase.axaml) 以正常入口演示公开属性，并核对现有 API 展示入口。固定样式值的全部引用在本批同步，包括 FloatButton 主题中的 DisabledOpacity。 |
| 2 | DateViewer、DatePicker、RangeInfoPickerInput、TimePicker | 先核对 DateViewer/RangeDateViewer 单元格，再覆盖 Calendar 与四个 picker 的正常模板、取消和重开场景，确认日期和时间两组共用范围输入的引用在同一批修改。 |
| 2 | Menu、SelectTag、TreeViewItemHeader | 迁移 Menu/ContextMenu/MenuFlyout、Select/Cascader/TreeSelect、TreeView/TreeSelect/Transfer 的直接消费者。对密集日期单元格和必要的树节点路径做基线比较。 |
| 3 | Dialog、ImagePreviewer、FloatButton | 验证托管与独立使用、关闭重开、重套模板、detach 和替换后的资源查找及回收。FloatButton 本批迁移其外观绑定类型、选择器和生命周期使用处。 |
| 4 | 各改动模块 | 清理临时验证产物，按实际改动路径做 focused 验证。同步受影响家族实现文档中出现的实际名称。新公开 API 按项目规则记录迁移和兼容性。 |

### 实施和验收边界

- 每个迁移项一起核对注册名称、`XxxProperty`、CLR 包装、`GetXxx`/`SetXxx`、Setter、绑定、限定属性、选择器、`x:Static` 和字符串测试。下面清单的名字属于属性。Token 的类型、枚举键、字符串和资源键按现有定义解析。
- 默认状态在旧、新源码上使用同一实例参数、数据及主题核对。公开字段另验证适用状态、局部值、Style Setter、绑定更新、主题切换和清除覆盖后的回落。已有 Theme 资源键、Semantic Part 和用户 API 以现有契约作为对照。
- 性能基线由取色器首次套模板和主题发布，以及密集日期单元格的同配置分配与耗时组成。核对绑定、观察者和继承路径数量。结果有可复现退化时，相关批次停止验收。
- 资源释放优先复用 DialogLifecycle、ImagePreviewerCoverOwnerInput 和 FloatButton 的既有场景，必要时使用临时 WeakReference 探针。验证重开、替换和脱离后旧对象可回收，最终对象数、回调数按基线比较。
- 编译和改动模块验收采用 [按改动影响选择验证](../../engineering/workflows/affected-verification.md)。首批基线优先使用现有 `ButtonCustomSizeTests`、`ButtonIconOnlyVisualContractTests`、`ButtonFamilyTokenScopeTests`、`ColorPickerFamilyEffectTests.Isolation`。日期组使用 `DateViewerThemeTests`、`DateViewerLifecycleTests` 和 `DataFamilyEffectTests.PickerIsolation`。共享部件组使用 `MenuPopupScrollModeTests`、`TreeSelectThemeContractTests`、`TreeViewItemHeaderLifecycleTests`、`TransferTreeViewTests` 等实际命中的用例。作用域和生命周期组使用下述直接引用测试及 `ButtonFamilyEffectTests.FloatButton`。
- 临时验证找到原有或新增失配时记录场景和证据。验证完成后按 [测试价值与生命周期](../../engineering/development/test-value-and-lifecycle.md) 处置验证代码和产物，再对最终源码运行相关模块。

## 4. 定义文件和逐项名称

`公开` 表示本轮建议加入正式 API。其余列明内部迁移和沿用项。属性值类型来自当前注册。

### Button

迁移前定义 `src/AtomUI.Desktop.Controls/Buttons/Button.ThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/Buttons/Button.Properties.cs`](../../../src/AtomUI.Desktop.Controls/Buttons/Button.Properties.cs)。

拟调整到 `Button.cs（公共属性区）和 Button.Properties.cs（其余属性）`。

ContentPadding 成为公开属性。其余属性继续由原 Button 声明和主题消费。DefaultFontWeight 与已有 FontWeight 区分。IconOnlyPadding 同时修正原属性拼写，现有 IconOnyPadding 等 Token 名称沿用现有契约。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `ThemeBorderColorDisabled` | `DisabledBorderBrush` | `IBrush?` | 内部 |
| `ThemeCirclePadding` | `CirclePadding` | `Thickness` | 内部 |
| `ThemeContentFontSize` | `ContentFontSize` | `double` | 内部 |
| `ThemeContentPadding` | `ContentPadding` | `Thickness` | 公开 |
| `ThemeDefaultBorderColorDisabled` | `DefaultDisabledBorderBrush` | `IBrush?` | 内部 |
| `ThemeFontWeight` | `DefaultFontWeight` | `FontWeight` | 内部 |
| `ThemeIconEndMargin` | `IconEndMargin` | `Thickness` | 内部 |
| `ThemeIconMargin` | `IconMargin` | `Thickness` | 内部 |
| `ThemeIconOnyPadding` | `IconOnlyPadding` | `Thickness` | 内部 |
| `ThemeOnlyIconSize` | `IconOnlySize` | `double` | 内部 |

### ButtonThemeInputs

迁移前定义 `src/AtomUI.Desktop.Controls/Buttons/ButtonThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/Buttons/ButtonStyleValues.cs`](../../../src/AtomUI.Desktop.Controls/Buttons/ButtonStyleValues.cs)。

拟调整到 `ButtonStyleValues.cs`。

DisabledOpacity 和 LinkUnderline 沿用现有值、初始化和读法。Button、DropdownButton、SplitButton、FloatButton 的 AXAML 静态引用同步换类型名。

### ColorPickerThemeInputs

迁移前定义 `src/AtomUI.Desktop.Controls.ColorPicker/ColorPickerThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls.ColorPicker/ColorPickerAttachedProperties.cs`](../../../src/AtomUI.Desktop.Controls.ColorPicker/ColorPickerAttachedProperties.cs)。

拟调整到 `ColorPickerAttachedProperties.cs，并在 AbstractColorPickerView.cs 添加公开入口`。

SliderTrackHeight 在两个视图上成为普通公开属性。内部仍以同一属性身份沿树传值，其余附加属性的 owner、优先级与继承范围继续按当前路径。检查普通、渐变和触发器各自的 Token 隔离。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `ThemeColorPickerWidth` | `SpectrumMinWidth` | `double` | 内部 |
| `ThemeColorSpectrumHeight` | `SpectrumHeight` | `double` | 内部 |
| `ThemeColorPickerInsetShadow` | `InsetShadows` | `BoxShadows` | 内部 |
| `ThemeColorPickerHandlerSizeLG` | `OuterSpectrumHandleSize` | `double` | 内部 |
| `ThemeColorPickerHandlerSize` | `SpectrumHandleSize` | `double` | 内部 |
| `ThemeColorPickerHandlerLightColor` | `SpectrumHandleLightBrush` | `IBrush?` | 内部 |
| `ThemeColorPickerHandlerDarkColor` | `SpectrumHandleDarkBrush` | `IBrush?` | 内部 |
| `ThemeColorPickerSliderSize` | `SliderHeight` | `double` | 内部 |
| `ThemeColorPickerSliderTrackSize` | `SliderTrackHeight` | `double` | 公开 |
| `ThemeColorPickerSliderThumbSize` | `SliderThumbSize` | `double` | 内部 |
| `ThemeColorPickerPreviewSize` | `PreviewSize` | `double` | 内部 |
| `ThemeColorPickerAlphaInputWidth` | `AlphaInputWidth` | `double` | 内部 |
| `ThemeColorPickerInputNumberHandleWidth` | `NumberInputSpinnerHandleWidth` | `double` | 内部 |
| `ThemeColorPickerPresetColorSize` | `PresetColorSize` | `double` | 内部 |
| `ThemeColorPickerPresetPanelWidth` | `PresetPanelWidth` | `double` | 内部 |
| `ThemeCheckedMarkSize` | `CheckedMarkSize` | `double` | 内部 |
| `ThemeColorPickerPresetColorGroupPadding` | `PresetColorGroupPadding` | `Thickness` | 内部 |
| `ThemeColorBlockInnerShadows` | `ColorBlockInnerShadows` | `BoxShadows` | 内部 |
| `ThemeTriggerPadding` | `TriggerPadding` | `Thickness` | 内部 |
| `ThemeTriggerTextMargin` | `TriggerTextMargin` | `Thickness` | 内部 |
| `ThemeSliderContainerMargin` | `SliderContainerMargin` | `Thickness` | 内部 |
| `ThemeTransparentBgSize` | `CheckerboardTileSize` | `double` | 内部 |
| `ThemeColorBlockDisabledOpacity` | `DisabledColorBlockOpacity` | `double` | 内部 |
| `ThemeEditorFontSize` | `EditorFontSize` | `double` | 内部 |
| `ThemeSeparatorThickness` | `SeparatorThickness` | `Thickness` | 内部 |

### DateViewerCell

迁移前定义 `src/AtomUI.Desktop.Controls/DateViewer/Internal/DateViewerCell.ThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/DateViewer/Internal/DateViewerCell.AttachedProperties.cs`](../../../src/AtomUI.Desktop.Controls/DateViewer/Internal/DateViewerCell.AttachedProperties.cs)。

拟调整到 `DateViewerCell.AttachedProperties.cs`。

同步修改 DateViewer 与 RangeDateViewer、共享日期单元格、面板和标题。Calendar 和 DatePicker 作为消费者做基线复核。DateViewerCell 是内部部件。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `ThemePanelBg` | `PanelBackground` | `IBrush?` | 内部 |
| `ThemeCellHoverBg` | `CellHoverBackground` | `IBrush?` | 内部 |
| `ThemeCellActiveColor` | `ActiveCellForeground` | `IBrush?` | 内部 |
| `ThemeCellRangeBg` | `RangeCellBackground` | `IBrush?` | 内部 |
| `ThemePanelWidth` | `PanelWidth` | `double` | 内部 |
| `ThemeCellHeight` | `CellHeight` | `double` | 内部 |
| `ThemeCellWidth` | `CellWidth` | `double` | 内部 |
| `ThemeCellRowHeight` | `CellRowHeight` | `double` | 内部 |
| `ThemePeriodCellWidth` | `PeriodCellWidth` | `double` | 内部 |
| `ThemePeriodCellPadding` | `PeriodCellPadding` | `Thickness` | 内部 |
| `ThemeContentCellMinHeight` | `ContentCellMinHeight` | `double` | 内部 |
| `ThemeContentCellSurfaceMargin` | `ContentCellSurfaceMargin` | `Thickness` | 内部 |
| `ThemeContentCellTopBorderThickness` | `ContentCellTopBorderThickness` | `Thickness` | 内部 |
| `ThemeContentCellValueMargin` | `ContentCellValueMargin` | `Thickness` | 内部 |
| `ThemeContentCellContentMargin` | `ContentCellContentMargin` | `Thickness` | 内部 |
| `ThemePanelPadding` | `PanelPadding` | `Thickness` | 内部 |
| `ThemeHeaderHorizontalPadding` | `HeaderHorizontalPadding` | `Thickness` | 内部 |
| `ThemeDateBodyPadding` | `DateBodyPadding` | `Thickness` | 内部 |
| `ThemeHeaderBorderThickness` | `HeaderBorderThickness` | `Thickness` | 内部 |
| `ThemeWeekNumberOpacity` | `WeekNumberOpacity` | `double` | 内部 |

### DatePickerPresenter

迁移前定义 `src/AtomUI.Desktop.Controls/DatePicker/DatePickerPresenter.ThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/DatePicker/DatePickerPresenter.AttachedProperties.cs`](../../../src/AtomUI.Desktop.Controls/DatePicker/DatePickerPresenter.AttachedProperties.cs)。

拟调整到 `DatePickerPresenter.AttachedProperties.cs`。

同步普通和范围日期选择器的 Setter、presenter 主题与 Header 样式资源。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `ThemePanelContentPadding` | `PanelContentPadding` | `Thickness` | 内部 |
| `ThemeButtonsPanelMargin` | `ButtonsPanelMargin` | `Thickness` | 内部 |
| `ThemeHeaderBorderThickness` | `HeaderBorderThickness` | `Thickness` | 内部 |

### RangeInfoPickerInputTheme

迁移前定义 `src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.ThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.AttachedProperties.cs`](../../../src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.AttachedProperties.cs)。

拟调整到 `RangeInfoPickerInputTheme.AttachedProperties.cs`。

同步 RangeDatePicker、RangeTimePicker 和范围输入基主题。主题类型继续作为原注册所有者。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `ThemeRangePickerArrowMargin` | `RangeArrowMargin` | `Thickness` | 内部 |
| `ThemeRangePickerIndicatorThickness` | `RangeIndicatorThickness` | `double` | 内部 |

### TimePickerPresenter

迁移前定义 `src/AtomUI.Desktop.Controls/TimePicker/TimePickerPresenter.ThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/TimePicker/TimePickerPresenter.AttachedProperties.cs`](../../../src/AtomUI.Desktop.Controls/TimePicker/TimePickerPresenter.AttachedProperties.cs)。

拟调整到 `TimePickerPresenter.AttachedProperties.cs`。

同时迁移 TimePicker 和 RangeTimePicker 的主题、presenter 样式及 C# 宿主映射。Token 名 ButtonsMargin 继续由现有映射读取。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `ThemeButtonsMargin` | `ButtonsMargin` | `Thickness` | 内部 |

### MenuPopupScrollHost

迁移前定义 `src/AtomUI.Desktop.Controls/Menu/MenuPopupScrollHost.ThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/Menu/MenuPopupScrollHost.AttachedProperties.cs`](../../../src/AtomUI.Desktop.Controls/Menu/MenuPopupScrollHost.AttachedProperties.cs)。

拟调整到 `MenuPopupScrollHost.AttachedProperties.cs`。

同时迁移 Menu、ContextMenu、MenuFlyoutPresenter 以及共享顶层项、普通项、分组和分隔线。用实际弹出路径验证。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `ThemeMenuPopupContentPadding` | `PopupContentPadding` | `Thickness` | 内部 |
| `ThemeMenuPopupMinWidth` | `PopupMinWidth` | `double` | 内部 |
| `ThemeMenuPopupMaxWidth` | `PopupMaxWidth` | `double` | 内部 |
| `ThemeMenuPopupBgColor` | `PopupBackground` | `IBrush?` | 内部 |
| `ThemeItemColor` | `ItemForeground` | `IBrush?` | 内部 |
| `ThemeKeyGestureColor` | `KeyGestureForeground` | `IBrush?` | 内部 |
| `ThemeItemMargin` | `ItemMargin` | `Thickness` | 内部 |
| `ThemeItemHoverColor` | `ItemHoverForeground` | `IBrush?` | 内部 |
| `ThemeItemDisabledColor` | `ItemDisabledForeground` | `IBrush?` | 内部 |
| `ThemeDangerItemColor` | `DangerItemForeground` | `IBrush?` | 内部 |
| `ThemeDangerItemHoverColor` | `DangerItemHoverForeground` | `IBrush?` | 内部 |
| `ThemeItemBg` | `ItemBackground` | `IBrush?` | 内部 |
| `ThemeItemHoverBg` | `ItemHoverBackground` | `IBrush?` | 内部 |
| `ThemeItemHeight` | `ItemHeight` | `double` | 内部 |
| `ThemeItemIconSize` | `ItemIconSize` | `double` | 内部 |
| `ThemeItemIconMarginInlineEnd` | `ItemIconEndMargin` | `double` | 内部 |
| `ThemeGroupTitlePadding` | `GroupTitlePadding` | `Thickness` | 内部 |
| `ThemeItemBorderRadius` | `ItemCornerRadius` | `CornerRadius` | 内部 |
| `ThemeItemPaddingInline` | `ItemInlinePadding` | `Thickness` | 内部 |
| `ThemeSeparatorItemHeight` | `SeparatorItemHeight` | `double` | 内部 |
| `ThemeMenuPopupShadows` | `PopupShadows` | `BoxShadows` | 内部 |

### SelectTag

迁移前定义 `src/AtomUI.Desktop.Controls/Select/SelectTag.ThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/Select/SelectTag.AttachedProperties.cs`](../../../src/AtomUI.Desktop.Controls/Select/SelectTag.AttachedProperties.cs)。

拟调整到 `SelectTag.AttachedProperties.cs`。

Select、Cascader、TreeSelect 同批。保留现有 CustomTagHeight 样式分支与优先级。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `ThemeMultipleItemBg` | `TagBackground` | `IBrush?` | 内部 |
| `ThemeMultipleItemHeight` | `TagHeight` | `double` | 内部 |
| `ThemeMultipleSelectorBgDisabled` | `DisabledSelectorBackground` | `IBrush?` | 内部 |
| `ThemeMultipleItemColorDisabled` | `DisabledTagForeground` | `IBrush?` | 内部 |
| `ThemeOptionFontSize` | `OptionFontSize` | `double` | 内部 |
| `ThemeMultiModePadding` | `MultipleSelectionPadding` | `Thickness` | 内部 |
| `ThemeCompactMultiModePadding` | `CompactMultipleSelectionPadding` | `Thickness` | 内部 |
| `ThemeMultiModePrefixIndent` | `MultipleSelectionPrefixIndent` | `Thickness` | 内部 |
| `ThemeSingleModePadding` | `SingleSelectionPadding` | `Thickness` | 内部 |

### TreeViewItemHeader

迁移前定义 `src/AtomUI.Desktop.Controls/TreeView/TreeViewItemHeader.ThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/TreeView/TreeViewItemHeader.AttachedProperties.cs`](../../../src/AtomUI.Desktop.Controls/TreeView/TreeViewItemHeader.AttachedProperties.cs)。

拟调整到 `TreeViewItemHeader.AttachedProperties.cs`。

TreeView、TreeSelectTreeView、TransferTreeView 和共享 NodeSwitcher 及 header 同批。FilterHighlightBrush 是已有公开 FilterHighlightForeground 的默认传值路径之一。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `ThemeHeaderHeight` | `HeaderHeight` | `double` | 内部 |
| `ThemeNodeHoverBg` | `NodeHoverBackground` | `IBrush?` | 内部 |
| `ThemeNodeSelectedBg` | `NodeSelectedBackground` | `IBrush?` | 内部 |
| `ThemeNodeSelectedColor` | `NodeSelectedForeground` | `IBrush?` | 内部 |
| `ThemeTreeItemMargin` | `ItemMargin` | `Thickness` | 内部 |
| `ThemeTreeItemHeaderPadding` | `ItemHeaderPadding` | `Thickness` | 内部 |
| `ThemeTreeItemHeaderMargin` | `ItemHeaderMargin` | `Thickness` | 内部 |
| `ThemeTreeNodeSwitcherMargin` | `NodeSwitcherMargin` | `Thickness` | 内部 |
| `ThemeTreeNodeIconMargin` | `NodeIconMargin` | `Thickness` | 内部 |
| `ThemeFilterHighlightColor` | `FilterHighlightBrush` | `IBrush?` | 内部 |

### DialogSurface

迁移前定义 `src/AtomUI.Desktop.Controls/Dialog/DialogSurface.ThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/Dialog/DialogSurface.AttachedProperties.cs`](../../../src/AtomUI.Desktop.Controls/Dialog/DialogSurface.AttachedProperties.cs)。

拟调整到 `DialogSurface.AttachedProperties.cs`。

同步 Dialog、MessageBox 与共享 surface、按钮区和标题。DefaultViewportMinHeight/Width 和目标 ContentViewportMinHeight/Width 分别代表默认输入与现有布局值。C# 传入绑定的属性字段更新，Token 字符串按原 key 查找。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `ThemeHeaderBg` | `HeaderBackground` | `IBrush?` | 内部 |
| `ThemeHeaderMarginBottom` | `HeaderBottomMargin` | `Thickness` | 内部 |
| `ThemeLogoSize` | `LogoSize` | `double` | 内部 |
| `ThemeHeaderFontSize` | `HeaderFontSize` | `double` | 内部 |
| `ThemeHeaderColor` | `HeaderForeground` | `IBrush?` | 内部 |
| `ThemeContentBg` | `ContentBackground` | `IBrush?` | 内部 |
| `ThemeHeaderPadding` | `HeaderPadding` | `Thickness` | 内部 |
| `ThemeContentPadding` | `ContentPadding` | `Thickness` | 内部 |
| `ThemeWindowContentPadding` | `WindowContentPadding` | `Thickness` | 内部 |
| `ThemeFooterBg` | `FooterBackground` | `IBrush?` | 内部 |
| `ThemeFooterPadding` | `FooterPadding` | `Thickness` | 内部 |
| `ThemeFooterMarginTop` | `FooterTopMargin` | `Thickness` | 内部 |
| `ThemeCloseBtnSize` | `CloseButtonSize` | `double` | 内部 |
| `ThemeMinHeight` | `DefaultViewportMinHeight` | `double` | 内部 |
| `ThemeMinWidth` | `DefaultViewportMinWidth` | `double` | 内部 |
| `ThemeButtonGroupSpacing` | `ButtonGroupSpacing` | `double` | 内部 |
| `ThemeLoadingIndicatorMargin` | `LoadingIndicatorMargin` | `Thickness` | 内部 |

### ImageViewer

迁移前定义 `src/AtomUI.Desktop.Controls/ImagePreviewer/ImageViewer.ThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/ImagePreviewer/ImageViewer.AttachedProperties.cs`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/ImageViewer.AttachedProperties.cs)。

拟调整到 `ImageViewer.AttachedProperties.cs`。

同步单图、图组、弹层、标题、cover 与工具栏。CoverImageSize 对应 cover 同时使用的宽和高。现有 ImageGroupPreviewer.CoverWidth 继续承担读写入口。DefaultMotionEnabled 与现有 IsMotionEnabled 的实例覆盖区分。C# 传入绑定字段更新，Token 名沿用当前值。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `ThemeColorBgLayout` | `LayoutBackground` | `IBrush?` | 内部 |
| `ThemeColorBgMask` | `OverlayMaskBackground` | `IBrush?` | 内部 |
| `ThemeColorBgTextActive` | `ActiveTextBackground` | `IBrush?` | 内部 |
| `ThemeColorBgTextHover` | `HoverTextBackground` | `IBrush?` | 内部 |
| `ThemeColorFillTertiary` | `TertiaryFill` | `IBrush?` | 内部 |
| `ThemeColorTextLightSolid` | `InverseTextForeground` | `IBrush?` | 内部 |
| `ThemeColorTextTertiary` | `TertiaryTextForeground` | `IBrush?` | 内部 |
| `ThemeControlHeightSM` | `SmallToolbarButtonSize` | `double` | 内部 |
| `ThemeCoverImageWidth` | `CoverImageSize` | `double` | 内部 |
| `ThemeDialogMinHeight` | `DialogMinHeight` | `double` | 内部 |
| `ThemeDialogMinWidth` | `DialogMinWidth` | `double` | 内部 |
| `ThemeEnableMotion` | `DefaultMotionEnabled` | `bool` | 内部 |
| `ThemeErrorIconSize` | `ErrorIconSize` | `double` | 内部 |
| `ThemeFloatToolbarIndicatorPadding` | `FloatToolbarIndicatorPadding` | `Thickness` | 内部 |
| `ThemeFloatToolbarPadding` | `FloatToolbarPadding` | `Thickness` | 内部 |
| `ThemeIconSize` | `IconSize` | `double` | 内部 |
| `ThemeIconSizeLG` | `LargeIconSize` | `double` | 内部 |
| `ThemeMarginSM` | `MarginSM` | `Thickness` | 内部 |
| `ThemeMarginXL` | `MarginXL` | `Thickness` | 内部 |
| `ThemeMaskBgColor` | `MaskBackground` | `IBrush?` | 内部 |
| `ThemeNavButtonBgColor` | `NavButtonBackground` | `IBrush?` | 内部 |
| `ThemeNavButtonBgHoverColor` | `NavButtonHoverBackground` | `IBrush?` | 内部 |
| `ThemePaddingSM` | `PaddingSM` | `Thickness` | 内部 |
| `ThemePaddingXS` | `PaddingXS` | `Thickness` | 内部 |
| `ThemePreviewOperationColor` | `PreviewOperationForeground` | `IBrush?` | 内部 |
| `ThemePreviewOperationColorDisabled` | `DisabledPreviewOperationForeground` | `IBrush?` | 内部 |
| `ThemePreviewOperationHoverColor` | `HoverPreviewOperationForeground` | `IBrush?` | 内部 |
| `ThemePreviewOperationSize` | `PreviewOperationSize` | `double` | 内部 |
| `ThemeSpacing` | `Spacing` | `double` | 内部 |
| `ThemeSpacingXXS` | `SpacingXXS` | `double` | 内部 |
| `ThemeTitleBarBackgroundColor` | `TitleBarBackground` | `IBrush?` | 内部 |

### FloatButtonThemeInputs

迁移前定义 `src/AtomUI.Desktop.Controls/FloatButton/FloatButtonThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/FloatButton/FloatButtonAppearanceBinding.cs`](../../../src/AtomUI.Desktop.Controls/FloatButton/FloatButtonAppearanceBinding.cs)。

拟调整到 `FloatButtonAppearanceBinding.cs`。

七个注册名已有明确角色。原位只改绑定类及文件名，静态成员和 Selector 中的 owner 名一并更新。FloatButton、BackTopFloatButton 和 FloatButtonItemsControl 沿现有挂接与释放路径持有该对象。

| 当前属性名 | 建议属性名 | 值类型 | 处置 |
| --- | --- | --- | --- |
| `HasOutline` | `HasOutline` | `bool` | 内部沿用 |
| `UseExplicitPrimaryShadow` | `UseExplicitPrimaryShadow` | `bool` | 内部沿用 |
| `ExplicitPrimaryShadow` | `ExplicitPrimaryShadow` | `BoxShadows` | 内部沿用 |
| `CapsuleDescription` | `CapsuleDescription` | `bool` | 内部沿用 |
| `UppercaseDescription` | `UppercaseDescription` | `bool` | 内部沿用 |
| `DescriptionWeight` | `DescriptionWeight` | `FontWeight` | 内部沿用 |
| `DescriptionLineHeight` | `DescriptionLineHeight` | `double` | 内部沿用 |

### OwnerThemeInputs

迁移前定义 `src/AtomUI.Desktop.Controls/Primitives/OwnerThemeInputs.cs`。迁移后见 [`src/AtomUI.Desktop.Controls/Primitives/PartTokenBinder.cs`](../../../src/AtomUI.Desktop.Controls/Primitives/PartTokenBinder.cs)。

共享绑定设施改名为 `PartTokenBinder.cs`，映射项改名为 `PartTokenMapping`。

共享绑定设施。Bind、BindFromOwner 以及内部映射、优先级和释放属于本轮验收边界。四个调用处同步采用新类型名。TimeView 继续作为部件自己读取宿主 Token 的验证入口。

## 5. 文件引用索引

每组列出旧标识符的直接命中位置，行号基于盘点时工作树。文件跨组时可以重复出现。第 3 节的公开 API 所有者、Gallery 和受影响文档属于此次新增联动点。

### Button

- [`src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml) · 迁移引用 · 9、10、11、12 等
- [`src/AtomUI.Desktop.Controls/Form/Themes/ResetButtonTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Form/Themes/ResetButtonTheme.axaml) · 迁移引用 · 6、7、8、9 等
- [`src/AtomUI.Desktop.Controls/Form/Themes/SubmitButtonTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Form/Themes/SubmitButtonTheme.axaml) · 迁移引用 · 6、7、8、9 等

### ButtonThemeInputs

- [`docs/controls/desktop/general/button/changelog.md`](../../../docs/controls/desktop/general/button/changelog.md) · 历史核对 · 24
- [`docs/controls/desktop/general/button/implementation.md`](../../../docs/controls/desktop/general/button/implementation.md) · 同步当前文档 · 23
- [`src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml) · 迁移引用 · 438
- [`src/AtomUI.Desktop.Controls/Buttons/Themes/DropdownButtonBaseTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Buttons/Themes/DropdownButtonBaseTheme.axaml) · 迁移引用 · 381
- [`src/AtomUI.Desktop.Controls/Buttons/Themes/SplitButtonTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Buttons/Themes/SplitButtonTheme.axaml) · 迁移引用 · 77
- [`src/AtomUI.Desktop.Controls/FloatButton/Themes/AbstractFloatButtonTheme.axaml`](../../../src/AtomUI.Desktop.Controls/FloatButton/Themes/AbstractFloatButtonTheme.axaml) · 迁移引用 · 137

### ColorPickerThemeInputs

- [`docs/controls/desktop/data-entry/color-picker/implementation.md`](../../../docs/controls/desktop/data-entry/color-picker/implementation.md) · 同步当前文档 · 16
- [`docs/controls/desktop/data-entry/color-picker/token.md`](../../../docs/controls/desktop/data-entry/color-picker/token.md) · 同步当前文档 · 11
- [`src/AtomUI.Desktop.Controls.ColorPicker/ColorBlock.cs`](../../../src/AtomUI.Desktop.Controls.ColorPicker/ColorBlock.cs) · 迁移引用 · 130、164
- [`src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/AbstractColorSlider.cs`](../../../src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/AbstractColorSlider.cs) · 迁移引用 · 123、142
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/AbstractColorPickerTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/AbstractColorPickerTheme.axaml) · 迁移引用 · 19、20、75、87
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorBlockTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorBlockTheme.axaml) · 迁移引用 · 23
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorPickerPaletteGroupTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorPickerPaletteGroupTheme.axaml) · 迁移引用 · 11、42
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorPickerTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorPickerTheme.axaml) · 迁移引用 · 8、9、10、11 等
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorSlider/ColorSliderTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorSlider/ColorSliderTheme.axaml) · 迁移引用 · 14、19、22、24 等
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorSlider/ColorSliderThumbTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorSlider/ColorSliderThumbTheme.axaml) · 迁移引用 · 35、37
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorSlider/GradientColorSliderTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorSlider/GradientColorSliderTheme.axaml) · 迁移引用 · 14、19、22、24 等
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/AbstractColorPickerViewTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/AbstractColorPickerViewTheme.axaml) · 迁移引用 · 26、27、32、41 等
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/ColorPickerInput.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/ColorPickerInput.axaml) · 迁移引用 · 77、79、104
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/ColorPickerViewTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/ColorPickerViewTheme.axaml) · 迁移引用 · 8、9、10、11 等
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/ColorSpectrumTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/ColorSpectrumTheme.axaml) · 迁移引用 · 60、74、79、87 等
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/GradientColorPickerViewTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/GradientColorPickerViewTheme.axaml) · 迁移引用 · 8、9、10、11 等
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/GradientColorPickerTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/GradientColorPickerTheme.axaml) · 迁移引用 · 8、9、10、11 等
- [`src/AtomUI.Desktop.Controls.ColorPicker/Themes/PaletteColorItemTheme.axaml`](../../../src/AtomUI.Desktop.Controls.ColorPicker/Themes/PaletteColorItemTheme.axaml) · 迁移引用 · 18、35、37、61 等
- [`tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/ColorPickerFamilyEffectTests.Isolation.cs`](../../../tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/ColorPickerFamilyEffectTests.Isolation.cs) · 迁移引用 · 50、51、55、56 等

### DateViewerCell

- [`src/AtomUI.Desktop.Controls/DateViewer/Themes/DatePanelTheme.axaml`](../../../src/AtomUI.Desktop.Controls/DateViewer/Themes/DatePanelTheme.axaml) · 迁移引用 · 7、10、21
- [`src/AtomUI.Desktop.Controls/DateViewer/Themes/DateViewerCellTheme.axaml`](../../../src/AtomUI.Desktop.Controls/DateViewer/Themes/DateViewerCellTheme.axaml) · 迁移引用 · 13、21、23、25 等
- [`src/AtomUI.Desktop.Controls/DateViewer/Themes/DateViewerHeaderTheme.axaml`](../../../src/AtomUI.Desktop.Controls/DateViewer/Themes/DateViewerHeaderTheme.axaml) · 迁移引用 · 14、15
- [`src/AtomUI.Desktop.Controls/DateViewer/Themes/DateViewerTheme.axaml`](../../../src/AtomUI.Desktop.Controls/DateViewer/Themes/DateViewerTheme.axaml) · 迁移引用 · 8、9、10、11 等
- [`src/AtomUI.Desktop.Controls/DateViewer/Themes/RangeDateViewerTheme.axaml`](../../../src/AtomUI.Desktop.Controls/DateViewer/Themes/RangeDateViewerTheme.axaml) · 迁移引用 · 8、9、11、13 等

### DatePickerPresenter

- [`src/AtomUI.Desktop.Controls/DatePicker/Themes/DatePickerPresenterResources.axaml`](../../../src/AtomUI.Desktop.Controls/DatePicker/Themes/DatePickerPresenterResources.axaml) · 迁移引用 · 11
- [`src/AtomUI.Desktop.Controls/DatePicker/Themes/DatePickerPresenterTheme.axaml`](../../../src/AtomUI.Desktop.Controls/DatePicker/Themes/DatePickerPresenterTheme.axaml) · 迁移引用 · 76、80
- [`src/AtomUI.Desktop.Controls/DatePicker/Themes/DatePickerTheme.axaml`](../../../src/AtomUI.Desktop.Controls/DatePicker/Themes/DatePickerTheme.axaml) · 迁移引用 · 7、8、9
- [`src/AtomUI.Desktop.Controls/DatePicker/Themes/RangeDatePickerPresenterTheme.axaml`](../../../src/AtomUI.Desktop.Controls/DatePicker/Themes/RangeDatePickerPresenterTheme.axaml) · 迁移引用 · 47、50
- [`src/AtomUI.Desktop.Controls/DatePicker/Themes/RangeDatePickerTheme.axaml`](../../../src/AtomUI.Desktop.Controls/DatePicker/Themes/RangeDatePickerTheme.axaml) · 迁移引用 · 11、12

### RangeInfoPickerInputTheme

- [`src/AtomUI.Desktop.Controls/DatePicker/Themes/RangeDatePickerTheme.axaml`](../../../src/AtomUI.Desktop.Controls/DatePicker/Themes/RangeDatePickerTheme.axaml) · 迁移引用 · 8、9
- [`src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.axaml) · 迁移引用 · 160、172
- [`src/AtomUI.Desktop.Controls/TimePicker/Themes/RangeTimePickerTheme.axaml`](../../../src/AtomUI.Desktop.Controls/TimePicker/Themes/RangeTimePickerTheme.axaml) · 迁移引用 · 8、9

### TimePickerPresenter

- [`src/AtomUI.Desktop.Controls/TimePicker/Themes/RangeTimePickerTheme.axaml`](../../../src/AtomUI.Desktop.Controls/TimePicker/Themes/RangeTimePickerTheme.axaml) · 迁移引用 · 11
- [`src/AtomUI.Desktop.Controls/TimePicker/Themes/TimePickerPresenterTheme.axaml`](../../../src/AtomUI.Desktop.Controls/TimePicker/Themes/TimePickerPresenterTheme.axaml) · 迁移引用 · 62
- [`src/AtomUI.Desktop.Controls/TimePicker/Themes/TimePickerTheme.axaml`](../../../src/AtomUI.Desktop.Controls/TimePicker/Themes/TimePickerTheme.axaml) · 迁移引用 · 7
- [`src/AtomUI.Desktop.Controls/TimePicker/TimePickerPresenter.cs`](../../../src/AtomUI.Desktop.Controls/TimePicker/TimePickerPresenter.cs) · 迁移引用 · 21

### MenuPopupScrollHost

- [`src/AtomUI.Desktop.Controls/Flyouts/Themes/MenuFlyoutPresenterTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Flyouts/Themes/MenuFlyoutPresenterTheme.axaml) · 迁移引用 · 7、8、9、10 等
- [`src/AtomUI.Desktop.Controls/Menu/Themes/ContextMenuTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Menu/Themes/ContextMenuTheme.axaml) · 迁移引用 · 7、8、9、10 等
- [`src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemGroupTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemGroupTheme.axaml) · 迁移引用 · 16
- [`src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemTheme.axaml) · 迁移引用 · 118、127、128、129 等
- [`src/AtomUI.Desktop.Controls/Menu/Themes/MenuSeparatorTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Menu/Themes/MenuSeparatorTheme.axaml) · 迁移引用 · 7
- [`src/AtomUI.Desktop.Controls/Menu/Themes/MenuTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Menu/Themes/MenuTheme.axaml) · 迁移引用 · 7、8、9、10 等
- [`src/AtomUI.Desktop.Controls/Menu/Themes/TopLevelMenuItemTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Menu/Themes/TopLevelMenuItemTheme.axaml) · 迁移引用 · 84、85、142、146 等

### SelectTag

- [`src/AtomUI.Desktop.Controls/Cascader/Themes/CascaderAddOnDecoratedBoxTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Cascader/Themes/CascaderAddOnDecoratedBoxTheme.axaml) · 迁移引用 · 20、22、24、27 等
- [`src/AtomUI.Desktop.Controls/Cascader/Themes/CascaderTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Cascader/Themes/CascaderTheme.axaml) · 迁移引用 · 8、9、10、11 等
- [`src/AtomUI.Desktop.Controls/Select/Themes/SelectAddOnDecoratedBoxTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Select/Themes/SelectAddOnDecoratedBoxTheme.axaml) · 迁移引用 · 17、19、21、24 等
- [`src/AtomUI.Desktop.Controls/Select/Themes/SelectTagTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Select/Themes/SelectTagTheme.axaml) · 迁移引用 · 13、18、22、24 等
- [`src/AtomUI.Desktop.Controls/Select/Themes/SelectTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Select/Themes/SelectTheme.axaml) · 迁移引用 · 7、8、9、10 等
- [`src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectAddOnDecoratedBoxTheme.axaml`](../../../src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectAddOnDecoratedBoxTheme.axaml) · 迁移引用 · 20、22、24、27 等
- [`src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectTheme.axaml`](../../../src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectTheme.axaml) · 迁移引用 · 7、8、9、10 等
- [`tests/AtomUI.Desktop.Controls.Tests/TreeSelect/TreeSelectThemeContractTests.cs`](../../../tests/AtomUI.Desktop.Controls.Tests/TreeSelect/TreeSelectThemeContractTests.cs) · 迁移引用 · 20、27

### TreeViewItemHeader

- [`src/AtomUI.Desktop.Controls/Transfer/Themes/TransferTreeViewTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Transfer/Themes/TransferTreeViewTheme.axaml) · 迁移引用 · 7、8、9、10 等
- [`src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectTreeViewTheme.axaml`](../../../src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectTreeViewTheme.axaml) · 迁移引用 · 9、10、11、12 等
- [`src/AtomUI.Desktop.Controls/TreeView/Themes/NodeSwitcherButtonTheme.axaml`](../../../src/AtomUI.Desktop.Controls/TreeView/Themes/NodeSwitcherButtonTheme.axaml) · 迁移引用 · 53
- [`src/AtomUI.Desktop.Controls/TreeView/Themes/TreeViewItemHeaderTheme.axaml`](../../../src/AtomUI.Desktop.Controls/TreeView/Themes/TreeViewItemHeaderTheme.axaml) · 迁移引用 · 101、102、112、118 等
- [`src/AtomUI.Desktop.Controls/TreeView/Themes/TreeViewTheme.axaml`](../../../src/AtomUI.Desktop.Controls/TreeView/Themes/TreeViewTheme.axaml) · 迁移引用 · 9、10、11、12 等

### DialogSurface

- [`src/AtomUI.Desktop.Controls/Dialog/DialogSurface.cs`](../../../src/AtomUI.Desktop.Controls/Dialog/DialogSurface.cs) · 迁移引用 · 233、234、235、236 等
- [`src/AtomUI.Desktop.Controls/Dialog/Themes/DialogButtonBoxTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Dialog/Themes/DialogButtonBoxTheme.axaml) · 迁移引用 · 32
- [`src/AtomUI.Desktop.Controls/Dialog/Themes/DialogSurfaceTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Dialog/Themes/DialogSurfaceTheme.axaml) · 迁移引用 · 10、11、68、74 等
- [`src/AtomUI.Desktop.Controls/Dialog/Themes/DialogTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Dialog/Themes/DialogTheme.axaml) · 迁移引用 · 7、8、9、10 等
- [`src/AtomUI.Desktop.Controls/Dialog/Themes/OverlayDialogHeaderTheme.axaml`](../../../src/AtomUI.Desktop.Controls/Dialog/Themes/OverlayDialogHeaderTheme.axaml) · 迁移引用 · 45、46、48、49 等
- [`src/AtomUI.Desktop.Controls/MessageBox/Themes/MessageBoxTheme.axaml`](../../../src/AtomUI.Desktop.Controls/MessageBox/Themes/MessageBoxTheme.axaml) · 迁移引用 · 7、8、9、10 等
- [`tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/FeedbackFamilyEffectTests.HostedOwnerInputs.cs`](../../../tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/FeedbackFamilyEffectTests.HostedOwnerInputs.cs) · 迁移引用 · 48、49、50

### ImageViewer

- [`src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerCover.cs`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerCover.cs) · 迁移引用 · 193、194、195、196 等
- [`src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImageGroupPreviewerTheme.axaml`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImageGroupPreviewerTheme.axaml) · 迁移引用 · 8、9、10、11 等
- [`src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewFloatToolbarTheme.axaml`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewFloatToolbarTheme.axaml) · 迁移引用 · 58、60、61、62 等
- [`src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewNavButtonTheme.axaml`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewNavButtonTheme.axaml) · 迁移引用 · 12、13、14、15 等
- [`src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewToolbarTheme.axaml`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewToolbarTheme.axaml) · 迁移引用 · 42、43、44、45 等
- [`src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerCoverTheme.axaml`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerCoverTheme.axaml) · 迁移引用 · 23、24、25、43 等
- [`src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerDialogTheme.axaml`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerDialogTheme.axaml) · 迁移引用 · 21、22、23、25 等
- [`src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerOverlayHostTheme.axaml`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerOverlayHostTheme.axaml) · 迁移引用 · 15、23、28、29 等
- [`src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerTheme.axaml`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerTheme.axaml) · 迁移引用 · 8、9、10、11 等
- [`src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerTitleBarTheme.axaml`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerTitleBarTheme.axaml) · 迁移引用 · 18、49、50、125 等
- [`src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImageViewerTheme.axaml`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImageViewerTheme.axaml) · 迁移引用 · 47、91、97、103 等
- [`tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/DataFamilyEffectTests.HostedOwnerInputs.cs`](../../../tests/AtomUI.Desktop.Controls.Rendering.Tests/Effect/Families/DataFamilyEffectTests.HostedOwnerInputs.cs) · 迁移引用 · 59、60、67
- [`tests/AtomUI.Desktop.Controls.Tests/ImagePreviewer/ImagePreviewerCoverOwnerInputTests.cs`](../../../tests/AtomUI.Desktop.Controls.Tests/ImagePreviewer/ImagePreviewerCoverOwnerInputTests.cs) · 迁移引用 · 25、38、39、67 等
- [`tests/AtomUI.Desktop.Controls.Tests/ImagePreviewer/ImagePreviewerLoadingVisualTests.cs`](../../../tests/AtomUI.Desktop.Controls.Tests/ImagePreviewer/ImagePreviewerLoadingVisualTests.cs) · 迁移引用 · 26、27、28、50 等
- [`tests/AtomUI.Desktop.Controls.Tests/ImagePreviewer/ImagePreviewerTitleBarThemeTests.cs`](../../../tests/AtomUI.Desktop.Controls.Tests/ImagePreviewer/ImagePreviewerTitleBarThemeTests.cs) · 迁移引用 · 129、131

### FloatButtonThemeInputs

- [`src/AtomUI.Desktop.Controls/FloatButton/BackTopFloatButton.cs`](../../../src/AtomUI.Desktop.Controls/FloatButton/BackTopFloatButton.cs) · 迁移引用 · 21、26
- [`src/AtomUI.Desktop.Controls/FloatButton/FloatButton.cs`](../../../src/AtomUI.Desktop.Controls/FloatButton/FloatButton.cs) · 迁移引用 · 11、16
- [`src/AtomUI.Desktop.Controls/FloatButton/FloatButtonItemsControl.cs`](../../../src/AtomUI.Desktop.Controls/FloatButton/FloatButtonItemsControl.cs) · 迁移引用 · 83、106
- [`src/AtomUI.Desktop.Controls/FloatButton/Themes/AbstractFloatButtonTheme.axaml`](../../../src/AtomUI.Desktop.Controls/FloatButton/Themes/AbstractFloatButtonTheme.axaml) · 迁移引用 · 118、119、131、132 等
- [`src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonItemsControlTheme.axaml`](../../../src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonItemsControlTheme.axaml) · 迁移引用 · 63

### OwnerThemeInputs

- [`docs/architecture/systems/theming/runtime.md`](../../../docs/architecture/systems/theming/runtime.md) · 同步当前文档 · 1150、1160
- [`docs/controls/desktop/data-entry/time-picker/changelog.md`](../../../docs/controls/desktop/data-entry/time-picker/changelog.md) · 历史核对 · 10
- [`src/AtomUI.Desktop.Controls/Dialog/DialogSurface.cs`](../../../src/AtomUI.Desktop.Controls/Dialog/DialogSurface.cs) · 复核共享路径 · 228、231、258
- [`src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerCover.cs`](../../../src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerCover.cs) · 复核共享路径 · 188、191、231
- [`src/AtomUI.Desktop.Controls/TimePicker/TimePickerPresenter.cs`](../../../src/AtomUI.Desktop.Controls/TimePicker/TimePickerPresenter.cs) · 复核共享路径 · 16、19、30
- [`src/AtomUI.Desktop.Controls/TimePicker/TimeView/TimeView.cs`](../../../src/AtomUI.Desktop.Controls/TimePicker/TimeView/TimeView.cs) · 复核共享路径 · 332

## 6. 确认内容

确认第 2 节列出的两个公开属性、第 4 节的内部名称和剩余项处置，以及第 3 节的批次与验收范围后，才开始修改控件实现。若名称或公开范围需要调整，以确认后的清单为准。

## 7. 实施与验收记录

- 2026-11-24 用户确认开始实施，并要求清除全部旧属性输入文件名。14 个源码路径均已迁移。共享部件 Token 绑定按实际职责改名为 `PartTokenBinder`。生产源码、AXAML 和测试已复核旧类型及字段名。
- 96 个源码及测试文件与预定标识符替换逐字节相符。三处原迁移文件另有公开属性接入，分别是两份取色器视图主题的 Setter 和 Button 属性声明位置。取色器的公开 `AddOwner` 注册在现有视图基类。
- 旧实现的 Button、取色器构建依赖、日期、Menu、树、ImagePreviewer、Dialog、FloatButton 定向测试共 95 项通过。新实现的临时 API 验证通过，覆盖 C# 设置、Style Setter、绑定、清除、普通和渐变视图主题更新以及实际滑轨高度。视图公开入口和附加属性经 `ReferenceEquals` 验证为同一注册。
- FloatButton 和独立 ImagePreviewer cover 反复挂接后移除。按项目现有 Headless 回收测试推进渲染时钟并排空 dispatcher，两个对象的 `WeakReference` 均已失效。未推进渲染时钟的首次探针仍观察到 Headless 对象留存。临时验证源码已清理。

### 性能基线和复测

同一 macOS 工作区、.NET SDK 11.0.100、net10.0 Debug 和 Headless 探针。每项预热五次，采样九组，每组八次。表中为单次操作分配与墙钟耗时中位数。第一轮候选与 API 验证同次运行，第二轮候选独立复测。

| 场景 | 基线 分配 / 时间 | 候选第一轮 分配 / 时间 | 候选独立复测 分配 / 时间 |
| --- | --- | --- | --- |
| 取色器首次模板和布局 | 11,794,083 B / 55.46 ms | 11,753,579 B / 62.94 ms | 11,826,908 B / 78.40 ms |
| 日期视图首次模板和布局（超过 30 个日期单元） | 13,669,066 B / 41.92 ms | 13,679,709 B / 43.70 ms | 13,669,551 B / 57.37 ms |
| 取色器两次主题发布和布局 | 118,932 B / 1.60 ms | 119,466 B / 1.76 ms | 120,211 B / 1.98 ms |

分配采样范围重叠。日期路径为标识符变更，取色器用同一属性注册和原有继承绑定，资源绑定的创建与释放路径逐条对应。两次候选墙钟结果有明显波动，独立复测期间主机负载较高，另有并行构建与测试，日期模板复测中位数超出基线采样上限 44.82 ms。此组墙钟数据未确立可复现的改动效应，也不足以证明耗时等价。


### 清理后定向验证

| 项目 | 选择范围 | 结果 |
| --- | --- | --- |
| AtomUI.Desktop.Controls.Tests | Button、ColorPicker、DateViewer、DatePicker、TimePicker、Calendar、Menu、Select、Cascader、TreeSelect、TreeView、Transfer、Dialog、ImagePreviewer、FloatButton、Form 及 Button 跨平台主题资产检查 | 1,060 通过，0 失败，0 跳过 |
| AtomUI.Desktop.Controls.Rendering.Tests | 公开取色器身份隔离、当前 Token 生效、日期和范围选择面板、独立 TimePicker presenter 和图片 Cover、Dialog placement target、Menu 局部资源、Tree、FloatButton 和 Button 视觉检查 | 69 通过，0 失败，0 跳过 |
| AtomUIGallery.Tests | ColorPicker 页和自定义尺寸，含 Gallery 中两个公开属性示例的编译 | 16 通过，0 失败，0 跳过 |

改动范围内的 diff 格式检查通过。14 个新源码路径均存在。生产、测试和 Gallery 中旧文件名搜索结果为零。迁移方案和确认清单保留旧注册名作为迁移前对照。
