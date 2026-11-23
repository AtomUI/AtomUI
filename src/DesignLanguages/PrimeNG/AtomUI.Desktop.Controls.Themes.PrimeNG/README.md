# AtomUI.Desktop.Controls.Themes.PrimeNG

Status: implemented and validated locally; not published to NuGet.

The PrimeNG values of the desktop controls (`AtomUI.Desktop.Controls`): one control derivation per control token class, under the Aura, Lara and Nora styles, light and dark, default and compact. Install it with the desktop control package; the language package and the shared values come with it.

```csharp
builder.UseDesktopControls();
builder.UsePrimeNG(language => language.AddDesktopControls());
```

## How the values are derived

Each derivation (`<Control>/PrimeNG<Control>Derivation.cs`) starts from the Ant Design formulas of the control (a copy
owned by PrimeNG, spec 5.6) and merges the PrimeNG control rules, which live in `Stages/` (ported from the control
stages of the previous `AtomUI.Themes.PrimeNG` package, one static class per control):

1. `EvaluateGlobal`: the global tokens this control sees differently, written on the control's view (`view.Global`),
   seeds included (for example the danger button fill `ColorError`, the button link colour, the severity colours of
   tags, alerts and message cards, the tooltip's `FontSize` and `ControlHeight`);
2. the control's formulas, which read those overrides;
3. `EvaluateOwn`: the preset's control values over the formulas; where a formula read a value the preset replaces
   (button small paddings, dropdown button extra margins, skeleton gradients, NavMenu popup height, dialog minimum
   heights), it runs again after it, as the previous second evaluation round did.

The only value that lived in a theme XML file, Lara's compact Select `PopupContentPadding 4`, is part of the Select
derivation. The controls without PrimeNG rules take the Ant Design formulas over the PrimeNG global tokens.

## Target levels

Spec 11.1: **A** (对标) — PrimeUIX has the component; its values follow the `@primeuix/themes` 3.1.1 standard preset and
are accepted visually against primeng.dev (PrimeNG 22.1.5, 16px root). **B** (协调) — PrimeUIX has no such component;
its values are composed from the PrimeNG global and language tokens and its key states are reviewed by hand.

| Control token | PrimeUIX 3.1.1 component | Level | Values |
|---|---|---|---|
| `AdornerLayerToken` | none | B | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGAdornerLayerTokens`) |
| `AlertToken` | `message` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGAlertTokens`) |
| `ArrowDecoratedBoxToken` | `popover` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGArrowDecoratedBoxTokens`) |
| `AutoCompleteSearchEditToken` | `autocomplete` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGAutoCompleteSearchEditTokens`) |
| `AutoCompleteTextAreaToken` | `autocomplete` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGAutoCompleteTextAreaTokens`) |
| `AutoCompleteToken` | `autocomplete` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGAutoCompleteTokens`) |
| `AvatarGroupToken` | `avatar (group)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGAvatarGroupTokens`) |
| `BorderBeamToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `BreadcrumbToken` | `breadcrumb` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGBreadcrumbTokens`) |
| `ButtonSpinnerToken` | `inputnumber (buttons)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGButtonSpinnerTokens`) |
| `ButtonToken` | `button` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGButtonFamilyTokens&lt;ButtonToken&gt;`) |
| `CalendarToken` | `datepicker (inline)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGCalendarTokens`) |
| `CardTabControlToken` | `tabs` | A | Ant Design formulas over the PrimeNG global tokens |
| `CardTabStripToken` | `tabs` | A | Ant Design formulas over the PrimeNG global tokens |
| `CardToken` | `card` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGCardTokens`) |
| `CarouselToken` | `carousel` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGCarouselTokens`) |
| `CascaderToken` | `cascadeselect` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGCascaderTokens`) |
| `CheckBoxToken` | `checkbox` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGCheckBoxTokens`) |
| `CheckableTagToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `CircleProgressToken` | none | B | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGProgressTokens&lt;CircleProgressToken&gt;`) |
| `CollapseToken` | `accordion` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGCollapseTokens`) |
| `ComboBoxToken` | `select (editable)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGComboBoxTokens`) |
| `CompactSpaceAddOnToken` | `inputgroup` | A | Ant Design formulas over the PrimeNG global tokens |
| `ContextMenuToken` | `contextmenu` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGContextMenuTokens`) |
| `CountBadgeToken` | `badge` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGCountBadgeTokens`) |
| `DashboardProgressToken` | none | B | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGProgressTokens&lt;DashboardProgressToken&gt;`) |
| `DatePickerToken` | `datepicker` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGDatePickerTokens`) |
| `DateViewerToken` | `datepicker (panel)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGDateViewerTokens`) |
| `DescriptionsToken` | none | B | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGDescriptionsTokens`) |
| `DialogToken` | `dialog` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGDialogTokens`) |
| `DotBadgeToken` | `badge (dot)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGDotBadgeTokens`) |
| `DrawerToken` | `drawer` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGDrawerTokens`) |
| `DropdownButtonToken` | `button + menu` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGButtonFamilyTokens&lt;DropdownButtonToken&gt;`) |
| `EmptyToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `ExpanderToken` | `panel` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGExpanderTokens`) |
| `FloatButtonToken` | `speeddial` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGFloatButtonTokens`) |
| `FlyoutHostToken` | `popover` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGFlyoutHostTokens`) |
| `FormToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `GroupBoxToken` | `fieldset` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGGroupBoxTokens`) |
| `HyperLinkButtonToken` | `button (link)` | A | Ant Design formulas over the PrimeNG global tokens |
| `ImageGroupPreviewerToken` | `galleria` | A | Ant Design formulas over the PrimeNG global tokens |
| `ImagePreviewerToken` | `image (preview)` | A | Ant Design formulas over the PrimeNG global tokens |
| `LineEditToken` | `inputtext` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGLineEditTokens`) |
| `ListBoxToken` | `listbox` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGListBoxTokens`) |
| `ListTransferToken` | `picklist` | A | Ant Design formulas over the PrimeNG global tokens |
| `ListViewToken` | `listbox` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGListViewTokens`) |
| `LunarCalendarToken` | none | B | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGLunarCalendarTokens`) |
| `MarqueeLabelToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `MentionsToken` | none | B | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGMentionsTokens`) |
| `MenuFlyoutPresenterToken` | `tieredmenu` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGMenuFlyoutPresenterTokens`) |
| `MenuItemGroupToken` | `menu (submenu label)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGMenuItemGroupTokens`) |
| `MenuToken` | `menubar` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGMenuTokens`) |
| `MessageBoxToken` | `confirmdialog` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGMessageBoxTokens`) |
| `MessageCardToken` | `message` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGMessageCardTokens`) |
| `NavMenuToken` | `panelmenu / menu` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGNavMenuTokens`) |
| `NotificationCardToken` | `toast` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGNotificationCardTokens`) |
| `NumericUpDownToken` | `inputnumber` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGNumericUpDownTokens`) |
| `OptionButtonToken` | `selectbutton` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGOptionButtonTokens`) |
| `OtpLineEditToken` | `inputotp` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGOtpLineEditTokens`) |
| `PaginationToken` | `paginator` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGPaginationTokens`) |
| `PopupConfirmToken` | `confirmpopup` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGPopupConfirmTokens`) |
| `PopupToken` | `overlay (select panel)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGPopupTokens`) |
| `ProgressBarToken` | `progressbar` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGProgressBarTokens`) |
| `QRCodeToken` | none | B | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGQRCodeTokens`) |
| `RadioButtonToken` | `radiobutton` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGRadioButtonTokens`) |
| `RangeDatePickerToken` | `datepicker (range)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGRangeDatePickerTokens`) |
| `RangeDateViewerToken` | `datepicker (range panel)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGRangeDateViewerTokens`) |
| `RangeTimePickerToken` | `datepicker (time only, range)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGRangeTimePickerTokens`) |
| `RateToken` | `rating` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGRateTokens`) |
| `ResetButtonToken` | `button` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGButtonFamilyTokens&lt;ResetButtonToken&gt;`) |
| `ResultToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `RibbonBadgeToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `ScrollViewerToken` | `scrollpanel` | A | Ant Design formulas over the PrimeNG global tokens |
| `SearchEditToken` | `iconfield + inputtext` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGSearchEditTokens`) |
| `SegmentedToken` | `selectbutton` | A | Ant Design formulas over the PrimeNG global tokens |
| `SelectToken` | `select / multiselect` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGSelectTokens`) |
| `SeparatorToken` | `divider` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGSeparatorTokens`) |
| `SimplePaginationToken` | `paginator` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGSimplePaginationTokens`) |
| `SkeletonToken` | `skeleton` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGSkeletonTokens`) |
| `SliderToken` | `slider` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGSliderTokens`) |
| `SpaceToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `SpinToken` | `progressspinner` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGSpinTokens`) |
| `SplitButtonToken` | `splitbutton` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGSplitButtonTokens`) |
| `SplitViewToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `SplitterToken` | `splitter` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGSplitterTokens`) |
| `StatisticCountUpToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `StatisticToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `StepsProgressBarToken` | none | B | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGProgressTokens&lt;StepsProgressBarToken&gt;`) |
| `StepsToken` | `stepper / steps` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGStepsTokens`) |
| `SubmitButtonToken` | `button` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGButtonFamilyTokens&lt;SubmitButtonToken&gt;`) |
| `TabControlToken` | `tabs` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGTabControlTokens`) |
| `TabStripToken` | `tabs` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGTabStripTokens`) |
| `TagToken` | `tag` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGTagTokens`) |
| `TextAreaToken` | `textarea` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGTextAreaTokens`) |
| `TextBoxToken` | `inputtext` | A | Ant Design formulas over the PrimeNG global tokens |
| `TimePickerToken` | `datepicker (time only)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGTimePickerTokens`) |
| `TimelineToken` | `timeline` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGTimelineTokens`) |
| `TimerStatisticToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `ToggleSwitchToken` | `toggleswitch` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGToggleSwitchTokens`) |
| `ToolTipToken` | `tooltip` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGToolTipTokens`) |
| `TourToken` | none | B | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGTourTokens`) |
| `TransferListViewToken` | `picklist (list)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGTransferListViewTokens`) |
| `TransferTreeViewToken` | `picklist (tree)` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGTransferTreeViewTokens`) |
| `TreeSelectToken` | `treeselect` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGTreeSelectTokens`) |
| `TreeTransferToken` | `picklist` | A | Ant Design formulas over the PrimeNG global tokens |
| `TreeViewFlyoutPresenterToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `TreeViewToken` | `tree` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGTreeViewTokens`) |
| `UploadToken` | `fileupload` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGUploadTokens`) |
| `WindowTitleBarButtonToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `WindowTitleBarToggleButtonToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `WindowTitleBarToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `WindowToken` | none | B | Ant Design formulas over the PrimeNG global tokens |

## Unsupported

The PrimeNG mapping tables, the AtomUI deviations and the unsupported list are in the
[language package README](../AtomUI.DesignLanguages.PrimeNG/README.md#unsupported-and-known-differences).
