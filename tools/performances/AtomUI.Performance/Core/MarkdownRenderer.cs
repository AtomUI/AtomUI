using System.Globalization;
using System.Text;

namespace AtomUI.Performance;

internal static partial class Program
{
        private static string RenderTable(IReadOnlyList<PerfResult> results)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Scenario                                Count  Total ms  ms/item  KB/item  Visual  Logical  CP Space  CSp CSpIt CSpAO  Button IconBtn  TB Panel Border Dock    SV    SB  SBTh SBRep   SCP OvrPan OvrLay  Icon  IconP STIconP BtnIconP  PathI  Stack Seg SegIt SegPan SegIconP  Wave Dashed LoadHost BtnSpin BSpinBox BSpinH BSpinCP Card CardAP CardBtn CardMeta CardGrid CardGI CardTabs Collapse CollItem CollMotion CollBtn CollAddOn Carous CarPage CarPag CarInd CarNav CarLay CarProg CarTrans CarTimer CarAnim Skeleton SkelAv SkelTitle SkelPara SkelLine  AODB Select TreeSel Cascader ComboBox ComboItem ComboH ComboHost ComboPop ComboSV ComboIP SelHandle SelHost SelList SelFilter SelResult SelTags TreeView CascView Popup AutoC ACSearch ACTArea CandList ACPopFld ACCandFld Avatar AvGroup Image Svg Flyout CBadge DBadge RBadge CBAd DBAd RBAd DotInd Motion Label FBtnHost FBtnGrpHost FBtn BackTopFBtn FBtnGrp FBtnItems FBtnSep CheckBox CBGroup CBItems CBInd CBMark CBTri Radio RadioGrp RadioInd Form FItem FAct FFeed Submit Reset DelBtn FTip GBox GBoxIcon Desc DescDef DescLbl DescCnt Dialog MsgBox OvlHost WinHost WinContent DlgBtnBox DlgBtn CapBtn Mask Resizer MsgContent  IconUpdates  BrushCalls  Scanned");
            builder.AppendLine("--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

            foreach (var result in results)
            {
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.Name,-39}{result.Count,5}{result.Elapsed.TotalMilliseconds,10:0.00}{result.MillisecondsPerItem,9:0.000}{result.KilobytesPerItem,9:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.VisualPerRoot,8:0.0}{result.TreeStats.LogicalPerRoot,9:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.ContentPresenterPerRoot,4:0.0}{result.TreeStats.SpacePerRoot,6:0.0}{result.TreeStats.CompactSpacePerRoot,5:0.0}{result.TreeStats.CompactSpaceItemPerRoot,6:0.0}{result.TreeStats.CompactSpaceAddOnPerRoot,6:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.ButtonPerRoot,8:0.0}{result.TreeStats.IconButtonPerRoot,7:0.0}{result.TreeStats.TextBlockPerRoot,5:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.PanelPerRoot,6:0.0}{result.TreeStats.BorderPerRoot,7:0.0}{result.TreeStats.DockPanelPerRoot,5:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.ScrollViewerPerRoot,6:0.0}{result.TreeStats.ScrollBarPerRoot,6:0.0}{result.TreeStats.ScrollBarThumbPerRoot,6:0.0}{result.TreeStats.ScrollBarRepeatButtonPerRoot,6:0.0}{result.TreeStats.ScrollContentPresenterPerRoot,6:0.0}{result.TreeStats.ScopeAwareOverlayLayerPanelPerRoot,7:0.0}{result.TreeStats.ScopeAwareOverlayLayerPerRoot,7:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.IconPerRoot,6:0.0}{result.TreeStats.IconPresenterPerRoot,7:0.0}{result.TreeStats.SizeTypeAwareIconPresenterPerRoot,8:0.0}{result.TreeStats.ButtonIconPresenterPerRoot,9:0.0}{result.TreeStats.PathIconPerRoot,7:0.0}{result.TreeStats.StackPanelPerRoot,7:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.SegmentedPerRoot,4:0.0}{result.TreeStats.SegmentedItemPerRoot,6:0.0}{result.TreeStats.SegmentedStackPanelPerRoot,7:0.0}{result.TreeStats.SegmentedIconPresenterPerRoot,9:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.WaveSpiritDecoratorPerRoot,6:0.0}{result.TreeStats.DashedBorderPerRoot,7:0.0}{result.TreeStats.ButtonLoadingHostPerRoot,9:0.0}{result.TreeStats.ButtonSpinnerPerRoot,8:0.0}{result.TreeStats.ButtonSpinnerDecoratedBoxPerRoot,9:0.0}{result.TreeStats.ButtonSpinnerHandlePerRoot,7:0.0}{result.TreeStats.ButtonSpinnerContentPanelPerRoot,8:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.CardPerRoot,5:0.0}{result.TreeStats.CardActionPanelPerRoot,7:0.0}{result.TreeStats.CardActionButtonPerRoot,8:0.0}{result.TreeStats.CardMetaContentPerRoot,9:0.0}{result.TreeStats.CardGridContentPerRoot,9:0.0}{result.TreeStats.CardGridItemPerRoot,7:0.0}{result.TreeStats.CardTabsContentPerRoot,9:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.CollapsePerRoot,9:0.0}{result.TreeStats.CollapseItemPerRoot,9:0.0}{result.TreeStats.CollapseContentViewportPerRoot,11:0.0}{result.TreeStats.CollapseExpandButtonPerRoot,8:0.0}{result.TreeStats.CollapseAddOnPresenterPerRoot,10:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.CarouselPerRoot,7:0.0}{result.TreeStats.CarouselPagePerRoot,8:0.0}{result.TreeStats.CarouselPaginationPerRoot,7:0.0}{result.TreeStats.CarouselPageIndicatorPerRoot,7:0.0}{result.TreeStats.CarouselNavButtonPerRoot,7:0.0}{result.TreeStats.CarouselLayoutTransformPerRoot,7:0.0}{result.TreeStats.CarouselProgressBorderPerRoot,8:0.0}{result.TreeStats.CarouselPageTransitionFieldPerRoot,9:0.0}{result.TreeStats.CarouselAutoPlayTimerFieldPerRoot,9:0.0}{result.TreeStats.CarouselIndicatorAnimationFieldPerRoot,8:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.SkeletonPerRoot,9:0.0}{result.TreeStats.SkeletonAvatarPerRoot,7:0.0}{result.TreeStats.SkeletonTitlePerRoot,10:0.0}{result.TreeStats.SkeletonParagraphPerRoot,9:0.0}{result.TreeStats.SkeletonLinePerRoot,9:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.AddOnDecoratedBoxPerRoot,6:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.SelectPerRoot,7:0.0}{result.TreeStats.TreeSelectPerRoot,8:0.0}{result.TreeStats.CascaderPerRoot,9:0.0}{result.TreeStats.ComboBoxPerRoot,9:0.0}{result.TreeStats.ComboBoxItemPerRoot,10:0.0}{result.TreeStats.ComboBoxHandlePerRoot,7:0.0}{result.TreeStats.ComboBoxAccessoryHostPerRoot,10:0.0}{result.TreeStats.ComboBoxPopupFramePerRoot,9:0.0}{result.TreeStats.ComboBoxScrollViewerPerRoot,8:0.0}{result.TreeStats.ComboBoxItemsPresenterPerRoot,8:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.SelectHandlePerRoot,10:0.0}{result.TreeStats.SelectAccessoryHostPerRoot,8:0.0}{result.TreeStats.SelectCandidateListPerRoot,8:0.0}{result.TreeStats.SelectFilterTextBoxPerRoot,10:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.SelectResultOptionsBoxPerRoot,10:0.0}{result.TreeStats.SelectTagAwareTextBoxPerRoot,8:0.0}{result.TreeStats.TreeSelectTreeViewPerRoot,9:0.0}{result.TreeStats.CascaderViewPerRoot,9:0.0}{result.TreeStats.PopupPerRoot,6:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.AutoCompletePerRoot,6:0.0}{result.TreeStats.AutoCompleteSearchEditPerRoot,9:0.0}{result.TreeStats.AutoCompleteTextAreaPerRoot,8:0.0}{result.TreeStats.CandidateListPerRoot,9:0.0}{result.TreeStats.AutoCompletePopupFieldPerRoot,9:0.0}{result.TreeStats.AutoCompleteCandidateListFieldPerRoot,10:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.AvatarPerRoot,7:0.0}{result.TreeStats.AvatarGroupPerRoot,8:0.0}{result.TreeStats.ImagePerRoot,6:0.0}{result.TreeStats.SvgPerRoot,4:0.0}{result.TreeStats.FlyoutHostPerRoot,7:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.CountBadgePerRoot,7:0.0}{result.TreeStats.DotBadgePerRoot,7:0.0}{result.TreeStats.RibbonBadgePerRoot,7:0.0}{result.TreeStats.CountBadgeAdornerPerRoot,5:0.0}{result.TreeStats.DotBadgeAdornerPerRoot,5:0.0}{result.TreeStats.RibbonBadgeAdornerPerRoot,5:0.0}{result.TreeStats.DotBadgeIndicatorPerRoot,7:0.0}{result.TreeStats.MotionTargetPerRoot,7:0.0}{result.TreeStats.LabelPerRoot,6:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.FloatButtonHostPerRoot,9:0.0}{result.TreeStats.FloatButtonGroupHostPerRoot,12:0.0}{result.TreeStats.FloatButtonPerRoot,5:0.0}{result.TreeStats.BackTopFloatButtonPerRoot,11:0.0}{result.TreeStats.FloatButtonGroupPerRoot,8:0.0}{result.TreeStats.FloatButtonItemsControlPerRoot,9:0.0}{result.TreeStats.FloatButtonSeparatorLayerPerRoot,8:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.CheckBoxPerRoot,9:0.0}{result.TreeStats.CheckBoxGroupPerRoot,8:0.0}{result.TreeStats.CheckBoxItemsControlPerRoot,8:0.0}{result.TreeStats.CheckBoxIndicatorPerRoot,6:0.0}{result.TreeStats.CheckBoxCheckedMarkPerRoot,7:0.0}{result.TreeStats.CheckBoxTristateMarkPerRoot,6:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.RadioButtonPerRoot,6:0.0}{result.TreeStats.RadioButtonGroupPerRoot,9:0.0}{result.TreeStats.RadioIndicatorPerRoot,9:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.FormPerRoot,5:0.0}{result.TreeStats.FormItemPerRoot,6:0.0}{result.TreeStats.FormActionsItemPerRoot,5:0.0}{result.TreeStats.FormValidateFeedbackPerRoot,6:0.0}{result.TreeStats.SubmitButtonPerRoot,7:0.0}{result.TreeStats.ResetButtonPerRoot,6:0.0}{result.TreeStats.ItemDeleteButtonPerRoot,7:0.0}{result.TreeStats.FormTooltipIconPresenterPerRoot,5:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.GroupBoxPerRoot,5:0.0}{result.TreeStats.GroupBoxHeaderIconPresenterPerRoot,9:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.DescriptionsPerRoot,5:0.0}{result.TreeStats.DescriptionDefaultItemPerRoot,8:0.0}{result.TreeStats.DescriptionBorderedItemLabelPerRoot,8:0.0}{result.TreeStats.DescriptionBorderedItemContentPerRoot,8:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.DialogPerRoot,7:0.0}{result.TreeStats.MessageBoxPerRoot,7:0.0}{result.TreeStats.OverlayDialogPresenterPerRoot,8:0.0}{result.TreeStats.DialogWindowPerRoot,8:0.0}{result.TreeStats.DialogSurfacePerRoot,11:0.0}{result.TreeStats.DialogButtonBoxPerRoot,10:0.0}{result.TreeStats.DialogButtonPerRoot,7:0.0}{result.TreeStats.DialogCaptionButtonPerRoot,7:0.0}{result.TreeStats.OverlayDialogMaskPerRoot,5:0.0}{result.TreeStats.OverlayDialogResizerPerRoot,8:0.0}{result.TreeStats.MessageBoxContentPerRoot,11:0.0}");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.ProbeSnapshot.UpdateIconStatusColorsCalls,13}{result.ProbeSnapshot.ApplyIconBrushCalls,12}{result.ProbeSnapshot.ApplyIconBrushScannedVisuals,9}");
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static string RenderMarkdown(IReadOnlyList<PerfResult> results, PerfOptions options)
        {
            var builder = new StringBuilder();
            builder.AppendLine(options.Suite.ToLowerInvariant() switch
            {
                "icon" => "# Icon Baseline",
                "avatar" => "# Avatar Baseline",
                "badge" => "# Badge Baseline",
                "breadcrumb" => "# Breadcrumb Baseline",
                "button" => "# Button Baseline",
                "buttonspinner" => "# ButtonSpinner Baseline",
                "calendar" => "# Calendar Baseline",
                "card" => "# Card Baseline",
                "carousel" => "# Carousel Baseline",
                "cascader" => "# Cascader Baseline",
                "checkbox" => "# CheckBox Baseline",
                "collapse" => "# Collapse Baseline",
                "combobox" => "# ComboBox Baseline",
                "datepicker" => "# DatePicker Baseline",
                "descriptions" => "# Descriptions Baseline",
                "dialog" => "# Dialog / MessageBox Baseline",
                "dropdownbutton" => "# DropdownButton Baseline",
                "drawer" => "# Drawer Baseline",
                "expander" => "# Expander Baseline",
                "empty" => "# Empty Baseline",
                "flyouts" => "# Flyouts Baseline",
                "floatbutton" => "# FloatButton Baseline",
                "form" => "# Form Baseline",
                "groupbox" => "# GroupBox Baseline",
                "imagepreviewer" => "# ImagePreviewer Baseline",
                "infoflyout" => "# InfoFlyout Baseline",
                "marqueelabel" => "# MarqueeLabel / Alert Baseline",
                "menu" => "# Menu Baseline",
                "navmenu" => "# NavMenu Baseline",
                "numericupdown" => "# NumericUpDown Baseline",
                "pagination" => "# Pagination Baseline",
                "popupconfirm" => "# PopupConfirm Baseline",
                "progressbar" => "# ProgressBar Baseline",
                "radiobutton" => "# RadioButton Baseline",
                "qrcode" => "# QRCode Baseline",
                "rate" => "# Rate Baseline",
                "segmented" => "# Segmented Baseline",
                "slider" => "# Slider Baseline",
                "spin" => "# Spin Baseline",
                "splitbutton" => "# SplitButton Baseline",
                "steps" => "# Steps Baseline",
                "statistic" => "# Statistic Baseline",
                "timeline" => "# Timeline Baseline",
                "splitview" => "# SplitView Baseline",
                "splitter" => "# Splitter Baseline",
                "skeleton" => "# Skeleton Baseline",
                "scrollviewer" => "# ScrollViewer Baseline",
                "space" => "# Space Baseline",
                "switch" => "# ToggleSwitch Baseline",
                "select" => "# Select Baseline",
                "autocomplete" => "# AutoComplete Baseline",
                _ => "# AddOnDecoratedBox / LineEdit Baseline"
            });
            builder.AppendLine();
            builder.AppendLine($"- Date: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
            builder.AppendLine($"- Configuration: Debug");
            builder.AppendLine($"- Suite: `{options.Suite}`");
            builder.AppendLine($"- Count per scenario: {options.Count}");
            builder.AppendLine($"- Runner: `tools/performances/AtomUI.Performance`");
            builder.AppendLine();
            builder.AppendLine("| Scenario | Count | Total ms | ms/item | KB/item | Visual/root | Logical/root | ContentPresenter/root | Space/root | CompactSpace/root | CompactSpaceItem/root | CompactSpaceAddOn/root | Button/root | IconButton/root | TextBlock/root | Panel/root | Border/root | DockPanel/root | ScrollViewer/root | ScrollBar/root | ScrollBarThumb/root | ScrollBarRepeatButton/root | ScrollContentPresenter/root | ScopeAwareOverlayLayerPanel/root | ScopeAwareOverlayLayer/root | Icon/root | IconPresenter/root | SizeTypeAwareIconPresenter/root | ButtonIconPresenter/root | PathIcon/root | StackPanel/root | Segmented/root | SegmentedItem/root | SegmentedStackPanel/root | Segmented IconPresenter/root | WaveSpiritDecorator/root | DashedBorder/root | ButtonLoadingHost/root | ButtonSpinner/root | ButtonSpinnerDecoratedBox/root | ButtonSpinnerHandle/root | ButtonSpinnerContentPanel/root | Card/root | CardActionPanel/root | CardActionButton/root | CardMetaContent/root | CardGridContent/root | CardGridItem/root | CardTabsContent/root | Collapse/root | CollapseItem/root | Collapse content motion/root | Collapse expand button/root | Collapse addon presenter/root | Carousel/root | CarouselPage/root | CarouselPagination/root | CarouselPageIndicator/root | CarouselNavButton/root | CarouselLayoutTransform/root | CarouselProgressBorder/root | Carousel PageTransition field/root | Carousel timer field/root | Carousel indicator animation field/root | Skeleton/root | SkeletonAvatar/root | SkeletonTitle/root | SkeletonParagraph/root | SkeletonLine/root | AddOnDecoratedBox/root | Select/root | TreeSelect/root | Cascader/root | ComboBox/root | ComboBoxItem/root | ComboBoxHandle/root | ComboBoxAccessoryHost/root | ComboBox PopupFrame/root | ComboBox ScrollViewer/root | ComboBox ItemsPresenter/root | SelectHandle/root | SelectAccessoryHost/root | SelectCandidateList/root | SelectFilterTextBox/root | SelectResultOptionsBox/root | SelectTagAwareTextBox/root | TreeSelectTreeView/root | CascaderView/root | Popup/root | AutoComplete/root | AutoCompleteSearchEdit/root | AutoCompleteTextArea/root | CandidateList/root | AutoComplete popup field/root | AutoComplete candidate field/root | Avatar/root | AvatarGroup/root | Image/root | Svg/root | FlyoutHost/root | CountBadge/root | DotBadge/root | RibbonBadge/root | CountBadgeAdorner/root | DotBadgeAdorner/root | RibbonBadgeAdorner/root | DotBadgeIndicator/root | MotionTarget/root | Label/root | FloatButtonHost/root | FloatButtonGroupHost/root | FloatButton/root | BackTopFloatButton/root | FloatButtonGroup/root | FloatButtonItemsControl/root | FloatButtonSeparatorLayer/root | CheckBox/root | CheckBoxGroup/root | CheckBoxItemsControl/root | CheckBoxIndicator/root | CheckBox checked mark/root | CheckBox tristate mark/root | RadioButton/root | RadioButtonGroup/root | RadioIndicator/root | Form/root | FormItem/root | FormActionsItem/root | FormValidateFeedback/root | SubmitButton/root | ResetButton/root | ItemDeleteButton/root | Form tooltip icon/root | GroupBox/root | GroupBox header icon/root | Descriptions/root | DescriptionDefaultItem/root | DescriptionBorderedItemLabel/root | DescriptionBorderedItemContent/root | Dialog/root | MessageBox/root | OverlayDialogPresenter/root | DialogWindow/root | DialogSurface/root | DialogButtonBox/root | DialogButton/root | DialogCaptionButton/root | OverlayDialogMask/root | OverlayDialogResizer/root | MessageBoxContent/root | Icon status calls | Icon brush calls | Icon scan visuals | Icon matches |");
            builder.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");

            foreach (var result in results)
            {
                builder.Append(CultureInfo.InvariantCulture,
                    $"| {result.Name} | {result.Count} | {result.Elapsed.TotalMilliseconds:0.00} | {result.MillisecondsPerItem:0.000} | {result.KilobytesPerItem:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.VisualPerRoot:0.0} | {result.TreeStats.LogicalPerRoot:0.0} | {result.TreeStats.ContentPresenterPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.SpacePerRoot:0.0} | {result.TreeStats.CompactSpacePerRoot:0.0} | {result.TreeStats.CompactSpaceItemPerRoot:0.0} | {result.TreeStats.CompactSpaceAddOnPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.ButtonPerRoot:0.0} | {result.TreeStats.IconButtonPerRoot:0.0} | {result.TreeStats.TextBlockPerRoot:0.0} | {result.TreeStats.PanelPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.BorderPerRoot:0.0} | {result.TreeStats.DockPanelPerRoot:0.0} | {result.TreeStats.ScrollViewerPerRoot:0.0} | {result.TreeStats.ScrollBarPerRoot:0.0} | {result.TreeStats.ScrollBarThumbPerRoot:0.0} | {result.TreeStats.ScrollBarRepeatButtonPerRoot:0.0} | {result.TreeStats.ScrollContentPresenterPerRoot:0.0} | {result.TreeStats.ScopeAwareOverlayLayerPanelPerRoot:0.0} | {result.TreeStats.ScopeAwareOverlayLayerPerRoot:0.0} | {result.TreeStats.IconPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.IconPresenterPerRoot:0.0} | {result.TreeStats.SizeTypeAwareIconPresenterPerRoot:0.0} | {result.TreeStats.ButtonIconPresenterPerRoot:0.0} | {result.TreeStats.PathIconPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.StackPanelPerRoot:0.0} | {result.TreeStats.SegmentedPerRoot:0.0} | {result.TreeStats.SegmentedItemPerRoot:0.0} | {result.TreeStats.SegmentedStackPanelPerRoot:0.0} | {result.TreeStats.SegmentedIconPresenterPerRoot:0.0} | {result.TreeStats.WaveSpiritDecoratorPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.DashedBorderPerRoot:0.0} | {result.TreeStats.ButtonLoadingHostPerRoot:0.0} | {result.TreeStats.ButtonSpinnerPerRoot:0.0} | {result.TreeStats.ButtonSpinnerDecoratedBoxPerRoot:0.0} | {result.TreeStats.ButtonSpinnerHandlePerRoot:0.0} | {result.TreeStats.ButtonSpinnerContentPanelPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.CardPerRoot:0.0} | {result.TreeStats.CardActionPanelPerRoot:0.0} | {result.TreeStats.CardActionButtonPerRoot:0.0} | {result.TreeStats.CardMetaContentPerRoot:0.0} | {result.TreeStats.CardGridContentPerRoot:0.0} | {result.TreeStats.CardGridItemPerRoot:0.0} | {result.TreeStats.CardTabsContentPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.CollapsePerRoot:0.0} | {result.TreeStats.CollapseItemPerRoot:0.0} | {result.TreeStats.CollapseContentViewportPerRoot:0.0} | {result.TreeStats.CollapseExpandButtonPerRoot:0.0} | {result.TreeStats.CollapseAddOnPresenterPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.CarouselPerRoot:0.0} | {result.TreeStats.CarouselPagePerRoot:0.0} | {result.TreeStats.CarouselPaginationPerRoot:0.0} | {result.TreeStats.CarouselPageIndicatorPerRoot:0.0} | {result.TreeStats.CarouselNavButtonPerRoot:0.0} | {result.TreeStats.CarouselLayoutTransformPerRoot:0.0} | {result.TreeStats.CarouselProgressBorderPerRoot:0.0} | {result.TreeStats.CarouselPageTransitionFieldPerRoot:0.0} | {result.TreeStats.CarouselAutoPlayTimerFieldPerRoot:0.0} | {result.TreeStats.CarouselIndicatorAnimationFieldPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.SkeletonPerRoot:0.0} | {result.TreeStats.SkeletonAvatarPerRoot:0.0} | {result.TreeStats.SkeletonTitlePerRoot:0.0} | {result.TreeStats.SkeletonParagraphPerRoot:0.0} | {result.TreeStats.SkeletonLinePerRoot:0.0} | {result.TreeStats.AddOnDecoratedBoxPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.SelectPerRoot:0.0} | {result.TreeStats.TreeSelectPerRoot:0.0} | {result.TreeStats.CascaderPerRoot:0.0} | {result.TreeStats.ComboBoxPerRoot:0.0} | {result.TreeStats.ComboBoxItemPerRoot:0.0} | {result.TreeStats.ComboBoxHandlePerRoot:0.0} | {result.TreeStats.ComboBoxAccessoryHostPerRoot:0.0} | {result.TreeStats.ComboBoxPopupFramePerRoot:0.0} | {result.TreeStats.ComboBoxScrollViewerPerRoot:0.0} | {result.TreeStats.ComboBoxItemsPresenterPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.SelectHandlePerRoot:0.0} | {result.TreeStats.SelectAccessoryHostPerRoot:0.0} | {result.TreeStats.SelectCandidateListPerRoot:0.0} | {result.TreeStats.SelectFilterTextBoxPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.SelectResultOptionsBoxPerRoot:0.0} | {result.TreeStats.SelectTagAwareTextBoxPerRoot:0.0} | {result.TreeStats.TreeSelectTreeViewPerRoot:0.0} | {result.TreeStats.CascaderViewPerRoot:0.0} | {result.TreeStats.PopupPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.AutoCompletePerRoot:0.0} | {result.TreeStats.AutoCompleteSearchEditPerRoot:0.0} | {result.TreeStats.AutoCompleteTextAreaPerRoot:0.0} | {result.TreeStats.CandidateListPerRoot:0.0} | {result.TreeStats.AutoCompletePopupFieldPerRoot:0.0} | {result.TreeStats.AutoCompleteCandidateListFieldPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.AvatarPerRoot:0.0} | {result.TreeStats.AvatarGroupPerRoot:0.0} | {result.TreeStats.ImagePerRoot:0.0} | {result.TreeStats.SvgPerRoot:0.0} | {result.TreeStats.FlyoutHostPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.CountBadgePerRoot:0.0} | {result.TreeStats.DotBadgePerRoot:0.0} | {result.TreeStats.RibbonBadgePerRoot:0.0} | {result.TreeStats.CountBadgeAdornerPerRoot:0.0} | {result.TreeStats.DotBadgeAdornerPerRoot:0.0} | {result.TreeStats.RibbonBadgeAdornerPerRoot:0.0} | {result.TreeStats.DotBadgeIndicatorPerRoot:0.0} | {result.TreeStats.MotionTargetPerRoot:0.0} | {result.TreeStats.LabelPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.FloatButtonHostPerRoot:0.0} | {result.TreeStats.FloatButtonGroupHostPerRoot:0.0} | {result.TreeStats.FloatButtonPerRoot:0.0} | {result.TreeStats.BackTopFloatButtonPerRoot:0.0} | {result.TreeStats.FloatButtonGroupPerRoot:0.0} | {result.TreeStats.FloatButtonItemsControlPerRoot:0.0} | {result.TreeStats.FloatButtonSeparatorLayerPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.CheckBoxPerRoot:0.0} | {result.TreeStats.CheckBoxGroupPerRoot:0.0} | {result.TreeStats.CheckBoxItemsControlPerRoot:0.0} | {result.TreeStats.CheckBoxIndicatorPerRoot:0.0} | {result.TreeStats.CheckBoxCheckedMarkPerRoot:0.0} | {result.TreeStats.CheckBoxTristateMarkPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.RadioButtonPerRoot:0.0} | {result.TreeStats.RadioButtonGroupPerRoot:0.0} | {result.TreeStats.RadioIndicatorPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.FormPerRoot:0.0} | {result.TreeStats.FormItemPerRoot:0.0} | {result.TreeStats.FormActionsItemPerRoot:0.0} | {result.TreeStats.FormValidateFeedbackPerRoot:0.0} | {result.TreeStats.SubmitButtonPerRoot:0.0} | {result.TreeStats.ResetButtonPerRoot:0.0} | {result.TreeStats.ItemDeleteButtonPerRoot:0.0} | {result.TreeStats.FormTooltipIconPresenterPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.GroupBoxPerRoot:0.0} | {result.TreeStats.GroupBoxHeaderIconPresenterPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.DescriptionsPerRoot:0.0} | {result.TreeStats.DescriptionDefaultItemPerRoot:0.0} | {result.TreeStats.DescriptionBorderedItemLabelPerRoot:0.0} | {result.TreeStats.DescriptionBorderedItemContentPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.TreeStats.DialogPerRoot:0.0} | {result.TreeStats.MessageBoxPerRoot:0.0} | {result.TreeStats.OverlayDialogPresenterPerRoot:0.0} | {result.TreeStats.DialogWindowPerRoot:0.0} | {result.TreeStats.DialogSurfacePerRoot:0.0} | {result.TreeStats.DialogButtonBoxPerRoot:0.0} | {result.TreeStats.DialogButtonPerRoot:0.0} | {result.TreeStats.DialogCaptionButtonPerRoot:0.0} | {result.TreeStats.OverlayDialogMaskPerRoot:0.0} | {result.TreeStats.OverlayDialogResizerPerRoot:0.0} | {result.TreeStats.MessageBoxContentPerRoot:0.0} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.ProbeSnapshot.UpdateIconStatusColorsCalls} | {result.ProbeSnapshot.ApplyIconBrushCalls} | ");
                builder.Append(CultureInfo.InvariantCulture,
                    $"{result.ProbeSnapshot.ApplyIconBrushScannedVisuals} | {result.ProbeSnapshot.ApplyIconBrushMatchedIcons} |");
                builder.AppendLine();
            }

            builder.AppendLine();
            builder.AppendLine("Notes:");
            builder.AppendLine();
            builder.AppendLine("- `Visual/root` and `Logical/root` include the scenario root control itself.");
            if (options.Suite.Equals("addon", StringComparison.OrdinalIgnoreCase))
            {
                builder.AppendLine("- CompactSpace scenarios use three `LineEdit` children per root.");
            }
            builder.AppendLine("- The suite measures materialization, template application and layout in headless mode; it does not isolate GPU/platform render cost.");
            builder.AppendLine("- Icon probe data is Debug-only and records `AddOnDecoratedBox.UpdateIconStatusColors()` plus `ApplyIconBrush()` scans.");
            builder.AppendLine("- Binding expression count is not directly measured yet; current baseline uses node counts, allocation, timing, and AddOnDecoratedBox probe counters.");
            builder.AppendLine("- This measures control-level template/style/materialization cost, not Gallery navigation.");
            return builder.ToString();
        }
}
