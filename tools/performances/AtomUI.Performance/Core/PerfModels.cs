using System.Reflection;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Performance;

internal sealed record PerfScenario(string Name, Func<int, Control> Create);

internal sealed record PerfResult(
    string Name,
    int Count,
    TimeSpan Elapsed,
    long AllocatedBytes,
    TreeStats TreeStats,
    AddOnDecoratedBoxPerfSnapshot ProbeSnapshot)
{
    public double MillisecondsPerItem => Elapsed.TotalMilliseconds / Count;
    public double KilobytesPerItem    => AllocatedBytes / 1024.0 / Count;
}

internal sealed class RealizedScenario : IDisposable
{
    public RealizedScenario(Avalonia.Controls.Window window, IReadOnlyList<Control> rootControls)
    {
        Window       = window;
        RootControls = rootControls;
    }

    public Avalonia.Controls.Window Window { get; }
    public IReadOnlyList<Control> RootControls { get; }

    public void Dispose()
    {
        Window.Close();
        Dispatcher.UIThread.RunJobs();
    }
}

internal sealed record TreeStats(
    double VisualPerRoot,
    double LogicalPerRoot,
    double ContentPresenterPerRoot,
    double SpacePerRoot,
    double CompactSpacePerRoot,
    double CompactSpaceItemPerRoot,
    double CompactSpaceAddOnPerRoot,
    double ButtonPerRoot,
    double IconButtonPerRoot,
    double TextBlockPerRoot,
    double PanelPerRoot,
    double BorderPerRoot,
    double DockPanelPerRoot,
    double ScrollViewerPerRoot,
    double ScrollBarPerRoot,
    double ScrollBarThumbPerRoot,
    double ScrollBarRepeatButtonPerRoot,
    double ScrollContentPresenterPerRoot,
    double ScopeAwareOverlayLayerPanelPerRoot,
    double ScopeAwareOverlayLayerPerRoot,
    double IconPerRoot,
    double IconPresenterPerRoot,
    double SizeTypeAwareIconPresenterPerRoot,
    double ButtonIconPresenterPerRoot,
    double PathIconPerRoot,
    double StackPanelPerRoot,
    double SegmentedPerRoot,
    double SegmentedItemPerRoot,
    double SegmentedStackPanelPerRoot,
    double SegmentedIconPresenterPerRoot,
    double WaveSpiritDecoratorPerRoot,
    double DashedBorderPerRoot,
    double ButtonLoadingHostPerRoot,
    double ButtonSpinnerPerRoot,
    double ButtonSpinnerDecoratedBoxPerRoot,
    double ButtonSpinnerHandlePerRoot,
    double ButtonSpinnerContentPanelPerRoot,
    double CardPerRoot,
    double CardActionPanelPerRoot,
    double CardActionButtonPerRoot,
    double CardMetaContentPerRoot,
    double CardGridContentPerRoot,
    double CardGridItemPerRoot,
    double CardTabsContentPerRoot,
    double CollapsePerRoot,
    double CollapseItemPerRoot,
    double CollapseContentViewportPerRoot,
    double CollapseExpandButtonPerRoot,
    double CollapseAddOnPresenterPerRoot,
    double CarouselPerRoot,
    double CarouselPagePerRoot,
    double CarouselPaginationPerRoot,
    double CarouselPageIndicatorPerRoot,
    double CarouselNavButtonPerRoot,
    double CarouselLayoutTransformPerRoot,
    double CarouselProgressBorderPerRoot,
    double CarouselPageTransitionFieldPerRoot,
    double CarouselAutoPlayTimerFieldPerRoot,
    double CarouselIndicatorAnimationFieldPerRoot,
    double SkeletonPerRoot,
    double SkeletonAvatarPerRoot,
    double SkeletonTitlePerRoot,
    double SkeletonParagraphPerRoot,
    double SkeletonLinePerRoot,
    double AddOnDecoratedBoxPerRoot,
    double SelectPerRoot,
    double TreeSelectPerRoot,
    double CascaderPerRoot,
    double ComboBoxPerRoot,
    double ComboBoxItemPerRoot,
    double ComboBoxHandlePerRoot,
    double ComboBoxAccessoryHostPerRoot,
    double ComboBoxPopupFramePerRoot,
    double ComboBoxScrollViewerPerRoot,
    double ComboBoxItemsPresenterPerRoot,
    double SelectHandlePerRoot,
    double SelectAccessoryHostPerRoot,
    double SelectCandidateListPerRoot,
    double SelectFilterTextBoxPerRoot,
    double SelectResultOptionsBoxPerRoot,
    double SelectTagAwareTextBoxPerRoot,
    double TreeSelectTreeViewPerRoot,
    double CascaderViewPerRoot,
    double PopupPerRoot,
    double AutoCompletePerRoot,
    double AutoCompleteSearchEditPerRoot,
    double AutoCompleteTextAreaPerRoot,
    double CandidateListPerRoot,
    double AutoCompletePopupFieldPerRoot,
    double AutoCompleteCandidateListFieldPerRoot,
    double AvatarPerRoot,
    double AvatarGroupPerRoot,
    double ImagePerRoot,
    double SvgPerRoot,
    double FlyoutHostPerRoot,
    double CountBadgePerRoot,
    double DotBadgePerRoot,
    double RibbonBadgePerRoot,
    double CountBadgeAdornerPerRoot,
    double DotBadgeAdornerPerRoot,
    double RibbonBadgeAdornerPerRoot,
    double DotBadgeIndicatorPerRoot,
    double MotionTargetPerRoot,
    double LabelPerRoot,
    double FloatButtonHostPerRoot,
    double FloatButtonGroupHostPerRoot,
    double FloatButtonPerRoot,
    double BackTopFloatButtonPerRoot,
    double FloatButtonGroupPerRoot,
    double FloatButtonItemsControlPerRoot,
    double FloatButtonSeparatorLayerPerRoot,
    double CheckBoxPerRoot,
    double CheckBoxGroupPerRoot,
    double CheckBoxItemsControlPerRoot,
    double CheckBoxIndicatorPerRoot,
    double CheckBoxCheckedMarkPerRoot,
    double CheckBoxTristateMarkPerRoot,
    double RadioButtonPerRoot,
    double RadioButtonGroupPerRoot,
    double RadioIndicatorPerRoot,
    double FormPerRoot,
    double FormItemPerRoot,
    double FormActionsItemPerRoot,
    double FormValidateFeedbackPerRoot,
    double SubmitButtonPerRoot,
    double ResetButtonPerRoot,
    double ItemDeleteButtonPerRoot,
    double FormTooltipIconPresenterPerRoot,
    double GroupBoxPerRoot,
    double GroupBoxHeaderIconPresenterPerRoot,
    double DescriptionsPerRoot,
    double DescriptionDefaultItemPerRoot,
    double DescriptionBorderedItemLabelPerRoot,
    double DescriptionBorderedItemContentPerRoot,
    double DialogPerRoot,
    double MessageBoxPerRoot,
    double OverlayDialogPresenterPerRoot,
    double DialogWindowPerRoot,
    double DialogSurfacePerRoot,
    double DialogButtonBoxPerRoot,
    double DialogButtonPerRoot,
    double DialogCaptionButtonPerRoot,
    double OverlayDialogMaskPerRoot,
    double OverlayDialogResizerPerRoot,
    double MessageBoxContentPerRoot)
{
    public static TreeStats Collect(IReadOnlyList<Control> roots)
    {
        var visualCount              = 0;
        var logicalCount             = 0;
        var contentPresenterCount    = 0;
        var spaceCount               = 0;
        var compactSpaceCount        = 0;
        var compactSpaceItemCount    = 0;
        var compactSpaceAddOnCount   = 0;
        var buttonCount              = 0;
        var iconButtonCount          = 0;
        var textBlockCount           = 0;
        var panelCount               = 0;
        var borderCount              = 0;
        var dockPanelCount           = 0;
        var scrollViewerCount        = 0;
        var scrollBarCount           = 0;
        var scrollBarThumbCount      = 0;
        var scrollBarRepeatButtonCount = 0;
        var scrollContentPresenterCount = 0;
        var scopeAwareOverlayLayerPanelCount = 0;
        var scopeAwareOverlayLayerCount = 0;
        var iconCount                = 0;
        var iconPresenterCount       = 0;
        var sizeTypeAwareIconPresenterCount = 0;
        var buttonIconPresenterCount = 0;
        var pathIconCount            = 0;
        var stackPanelCount          = 0;
        var segmentedCount           = 0;
        var segmentedItemCount       = 0;
        var segmentedStackPanelCount = 0;
        var segmentedIconPresenterCount = 0;
        var waveSpiritDecoratorCount = 0;
        var dashedBorderCount        = 0;
        var buttonLoadingHostCount   = 0;
        var buttonSpinnerCount       = 0;
        var buttonSpinnerDecoratedBoxCount   = 0;
        var buttonSpinnerHandleCount         = 0;
        var buttonSpinnerContentPanelCount   = 0;
        var cardCount                        = 0;
        var cardActionPanelCount             = 0;
        var cardActionButtonCount            = 0;
        var cardMetaContentCount             = 0;
        var cardGridContentCount             = 0;
        var cardGridItemCount                = 0;
        var cardTabsContentCount             = 0;
        var collapseCount                    = 0;
        var collapseItemCount                = 0;
        var collapseContentViewportCount  = 0;
        var collapseExpandButtonCount        = 0;
        var collapseAddOnPresenterCount      = 0;
        var carouselCount                    = 0;
        var carouselPageCount                = 0;
        var carouselPaginationCount          = 0;
        var carouselPageIndicatorCount       = 0;
        var carouselNavButtonCount           = 0;
        var carouselLayoutTransformCount     = 0;
        var carouselProgressBorderCount      = 0;
        var carouselPageTransitionFieldCount = 0;
        var carouselAutoPlayTimerFieldCount  = 0;
        var carouselIndicatorAnimationFieldCount = 0;
        var skeletonCount                    = 0;
        var skeletonAvatarCount              = 0;
        var skeletonTitleCount               = 0;
        var skeletonParagraphCount           = 0;
        var skeletonLineCount                = 0;
        var addOnDecoratedBoxCount   = 0;
        var selectCount              = 0;
        var treeSelectCount          = 0;
        var cascaderCount            = 0;
        var comboBoxCount            = 0;
        var comboBoxItemCount        = 0;
        var comboBoxHandleCount      = 0;
        var comboBoxAccessoryHostCount = 0;
        var comboBoxPopupFrameCount  = 0;
        var comboBoxScrollViewerCount = 0;
        var comboBoxItemsPresenterCount = 0;
        var selectHandleCount        = 0;
        var selectAccessoryHostCount = 0;
        var selectCandidateListCount = 0;
        var selectFilterTextBoxCount = 0;
        var selectResultOptionsBoxCount = 0;
        var selectTagAwareTextBoxCount  = 0;
        var treeSelectTreeViewCount     = 0;
        var cascaderViewCount           = 0;
        var popupCount                  = 0;
        var autoCompleteCount           = 0;
        var autoCompleteSearchEditCount = 0;
        var autoCompleteTextAreaCount   = 0;
        var candidateListCount          = 0;
        var autoCompletePopupFieldCount = 0;
        var autoCompleteCandidateListFieldCount = 0;
        var avatarCount                 = 0;
        var avatarGroupCount            = 0;
        var imageCount                  = 0;
        var svgCount                    = 0;
        var flyoutHostCount             = 0;
        var countBadgeCount             = 0;
        var dotBadgeCount               = 0;
        var ribbonBadgeCount            = 0;
        var countBadgeAdornerCount      = 0;
        var dotBadgeAdornerCount        = 0;
        var ribbonBadgeAdornerCount     = 0;
        var dotBadgeIndicatorCount      = 0;
        var motionTargetCount            = 0;
        var labelCount                  = 0;
        var floatButtonHostCount        = 0;
        var floatButtonGroupHostCount   = 0;
        var floatButtonCount            = 0;
        var backTopFloatButtonCount     = 0;
        var floatButtonGroupCount       = 0;
        var floatButtonItemsControlCount = 0;
        var floatButtonSeparatorLayerCount = 0;
        var checkBoxCount               = 0;
        var checkBoxGroupCount          = 0;
        var checkBoxItemsControlCount   = 0;
        var checkBoxIndicatorCount      = 0;
        var checkBoxCheckedMarkCount    = 0;
        var checkBoxTristateMarkCount   = 0;
        var radioButtonCount            = 0;
        var radioButtonGroupCount       = 0;
        var radioIndicatorCount         = 0;
        var formCount                   = 0;
        var formItemCount               = 0;
        var formActionsItemCount        = 0;
        var formValidateFeedbackCount   = 0;
        var submitButtonCount           = 0;
        var resetButtonCount            = 0;
        var itemDeleteButtonCount       = 0;
        var formTooltipIconPresenterCount = 0;
        var groupBoxCount               = 0;
        var groupBoxHeaderIconPresenterCount = 0;
        var descriptionsCount           = 0;
        var descriptionDefaultItemCount = 0;
        var descriptionBorderedItemLabelCount = 0;
        var descriptionBorderedItemContentCount = 0;
        var dialogCount                 = 0;
        var messageBoxCount             = 0;
        var overlayDialogPresenterCount = 0;
        var dialogWindowCount           = 0;
        var dialogSurfaceCount          = 0;
        var dialogButtonBoxCount        = 0;
        var dialogButtonCount           = 0;
        var dialogCaptionButtonCount    = 0;
        var overlayDialogMaskCount      = 0;
        var overlayDialogResizerCount   = 0;
        var messageBoxContentCount      = 0;

        foreach (var root in roots)
        {
            var visuals = root.GetSelfAndVisualDescendants().ToList();
            visualCount += visuals.Count;

            foreach (var visual in visuals)
            {
                var type = visual.GetType();
                if (type.Name == "ContentPresenter")
                {
                    contentPresenterCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Space"))
                {
                    spaceCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CompactSpace"))
                {
                    compactSpaceCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CompactSpaceItem"))
                {
                    compactSpaceItemCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CompactSpaceAddOn"))
                {
                    compactSpaceAddOnCount++;
                }
                if (visual is Avalonia.Controls.Button)
                {
                    buttonCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.IconButton"))
                {
                    iconButtonCount++;
                }
                if (type.Name == "TextBlock")
                {
                    textBlockCount++;
                }
                if (visual is Panel panel)
                {
                    panelCount++;
                    if (panel.Name == "PART_LoadingIconHost")
                    {
                        buttonLoadingHostCount++;
                    }
                }
                if (visual is Border)
                {
                    borderCount++;
                }
                if (visual is DockPanel)
                {
                    dockPanelCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ScrollViewer") ||
                    IsTypeOrDerived(type, "Avalonia.Controls.ScrollViewer"))
                {
                    scrollViewerCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ScrollBar") ||
                    IsTypeOrDerived(type, "Avalonia.Controls.Primitives.ScrollBar"))
                {
                    scrollBarCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ScrollBarThumb"))
                {
                    scrollBarThumbCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Controls.Commons.ScrollBarRepeatButton"))
                {
                    scrollBarRepeatButtonCount++;
                }
                if (IsTypeOrDerived(type, "Avalonia.Controls.Presenters.ScrollContentPresenter"))
                {
                    scrollContentPresenterCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Controls.Primitives.ScopeAwareOverlayLayerPanel"))
                {
                    scopeAwareOverlayLayerPanelCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Controls.Primitives.ScopeAwareOverlayLayer"))
                {
                    scopeAwareOverlayLayerCount++;
                }
                if (type.Name.EndsWith("Icon", StringComparison.Ordinal) || IsAtomIcon(type))
                {
                    iconCount++;
                }
                if (visual is IconPresenter iconPresenter)
                {
                    iconPresenterCount++;
                    if (iconPresenter.Name == "PART_ButtonIcon")
                    {
                        buttonIconPresenterCount++;
                    }
                    if ((iconPresenter.Name == "IconPresenter" || iconPresenter.Name == "PART_IconPresenter") &&
                        iconPresenter.GetVisualAncestors().Any(ancestor =>
                            IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.SegmentedItem")))
                    {
                        segmentedIconPresenterCount++;
                    }
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SizeTypeAwareIconPresenter"))
                {
                    sizeTypeAwareIconPresenterCount++;
                }
                if (visual is PathIcon)
                {
                    pathIconCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ButtonSpinner"))
                {
                    buttonSpinnerCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ButtonSpinnerDecoratedBox"))
                {
                    buttonSpinnerDecoratedBoxCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ButtonSpinnerHandle"))
                {
                    buttonSpinnerHandleCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ButtonSpinnerContentPanel"))
                {
                    buttonSpinnerContentPanelCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Card"))
                {
                    cardCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardActionPanel"))
                {
                    cardActionPanelCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardActionButton"))
                {
                    cardActionButtonCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardMetaContent"))
                {
                    cardMetaContentCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardGridContent"))
                {
                    cardGridContentCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardGridItem"))
                {
                    cardGridItemCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardTabsContent"))
                {
                    cardTabsContentCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Collapse"))
                {
                    collapseCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CollapseItem"))
                {
                    collapseItemCount++;
                }
                if (visual is Control { Name: "PART_ContentViewport" } contentViewport &&
                    contentViewport.GetVisualAncestors().Any(ancestor =>
                        IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.CollapseItem")))
                {
                    collapseContentViewportCount++;
                }
                if (visual is Control { Name: "PART_ExpandButton" } expandButton &&
                    expandButton.GetVisualAncestors().Any(ancestor =>
                        IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.CollapseItem")))
                {
                    collapseExpandButtonCount++;
                }
                if (visual is ContentPresenter { Name: "PART_AddOnContentPresenter" } addOnPresenter &&
                    addOnPresenter.GetVisualAncestors().Any(ancestor =>
                        IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.CollapseItem")))
                {
                    collapseAddOnPresenterCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Carousel"))
                {
                    carouselCount++;
                    if (visual is AtomUI.Desktop.Controls.Carousel carousel && carousel.PageTransition is not null)
                    {
                        carouselPageTransitionFieldCount++;
                    }
                    if (HasFieldValue(visual, "AtomUI.Desktop.Controls.Carousel", "_autoPlayTimer"))
                    {
                        carouselAutoPlayTimerFieldCount++;
                    }
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CarouselPage"))
                {
                    carouselPageCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CarouselPagination"))
                {
                    carouselPaginationCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CarouselPageIndicator"))
                {
                    carouselPageIndicatorCount++;
                    if (HasFieldValue(visual, "AtomUI.Desktop.Controls.CarouselPageIndicator", "_animation"))
                    {
                        carouselIndicatorAnimationFieldCount++;
                    }
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CarouselNavButton"))
                {
                    carouselNavButtonCount++;
                }
                if (visual is LayoutTransformControl { Name: "PaginationLayoutTransform" })
                {
                    carouselLayoutTransformCount++;
                }
                if (visual is Border { Name: "Progress" } progressBorder &&
                    progressBorder.GetVisualAncestors().Any(ancestor =>
                        IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.CarouselPageIndicator")))
                {
                    carouselProgressBorderCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Skeleton"))
                {
                    skeletonCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SkeletonAvatar"))
                {
                    skeletonAvatarCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SkeletonTitle"))
                {
                    skeletonTitleCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SkeletonParagraph"))
                {
                    skeletonParagraphCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SkeletonLine"))
                {
                    skeletonLineCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Avatar"))
                {
                    avatarCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.AvatarGroup"))
                {
                    avatarGroupCount++;
                }
                if (visual is Image)
                {
                    imageCount++;
                }
                if (IsTypeOrDerived(type, "Avalonia.Svg.Svg"))
                {
                    svgCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.FlyoutHost"))
                {
                    flyoutHostCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CountBadge"))
                {
                    countBadgeCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DotBadge"))
                {
                    dotBadgeCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.RibbonBadge"))
                {
                    ribbonBadgeCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CountBadgeAdorner"))
                {
                    countBadgeAdornerCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DotBadgeAdorner"))
                {
                    dotBadgeAdornerCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.RibbonBadgeAdorner"))
                {
                    ribbonBadgeAdornerCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Controls.Commons.DotBadgeIndicator"))
                {
                    dotBadgeIndicatorCount++;
                }
                if (visual is Control motionTarget &&
                    (IsTypeOrDerived(type, "AtomUI.Controls.Primitives.ContentViewport") ||
                     motionTarget.Name is "PART_SurfaceMotionRoot" or "PART_Surface" or "PART_MenuSurface" or
                         "PART_VisibilityRoot" or "PART_PopupSurface" or "PART_IndicatorSurface" ||
                     (motionTarget.Name is "Frame" or "PART_Frame" &&
                      motionTarget.GetVisualParent()?.GetType().Name is "MessageCard" or "NotificationCard")))
                {
                    motionTargetCount++;
                }
                if (visual is Label)
                {
                    labelCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.FloatButtonHost"))
                {
                    floatButtonHostCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.FloatButtonGroupHost"))
                {
                    floatButtonGroupHostCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.FloatButton"))
                {
                    floatButtonCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.BackTopFloatButton"))
                {
                    backTopFloatButtonCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.FloatButtonGroup"))
                {
                    floatButtonGroupCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.FloatButtonItemsControl"))
                {
                    floatButtonItemsControlCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Controls.Commons.FloatButtonSeparatorLayer"))
                {
                    floatButtonSeparatorLayerCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CheckBox"))
                {
                    checkBoxCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CheckBoxGroup"))
                {
                    checkBoxGroupCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CheckBoxItemsControl"))
                {
                    checkBoxItemsControlCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Controls.CheckBoxIndicator"))
                {
                    checkBoxIndicatorCount++;
                }
                if (visual is Control { Name: "CheckedMark" })
                {
                    checkBoxCheckedMarkCount++;
                }
                if (visual is Control { Name: "TristateMark" })
                {
                    checkBoxTristateMarkCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.RadioButton"))
                {
                    radioButtonCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.RadioButtonGroup"))
                {
                    radioButtonGroupCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Controls.Commons.RadioIndicator"))
                {
                    radioIndicatorCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Form"))
                {
                    formCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.FormActionsItem"))
                {
                    formActionsItemCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.FormItem"))
                {
                    formItemCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Controls.FormValidateFeedback"))
                {
                    formValidateFeedbackCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SubmitButton"))
                {
                    submitButtonCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ResetButton"))
                {
                    resetButtonCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ItemDeleteButton"))
                {
                    itemDeleteButtonCount++;
                }
                if (visual is IconPresenter { Name: "TooltipIconPresenter" } tooltipIconPresenter &&
                    tooltipIconPresenter.GetVisualAncestors().Any(ancestor =>
                        IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.FormItem")))
                {
                    formTooltipIconPresenterCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.GroupBox"))
                {
                    groupBoxCount++;
                }
                if (visual is IconPresenter { Name: "PART_HeaderIconPresenter" } groupBoxIconPresenter &&
                    groupBoxIconPresenter.GetVisualAncestors().Any(ancestor =>
                        IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.GroupBox")))
                {
                    groupBoxHeaderIconPresenterCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Descriptions"))
                {
                    descriptionsCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DescriptionDefaultItem"))
                {
                    descriptionDefaultItemCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DescriptionBorderedItemLabel"))
                {
                    descriptionBorderedItemLabelCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DescriptionBorderedItemContent"))
                {
                    descriptionBorderedItemContentCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Dialog"))
                {
                    dialogCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.MessageBox"))
                {
                    messageBoxCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.OverlayDialogPresenter"))
                {
                    overlayDialogPresenterCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DialogWindow"))
                {
                    dialogWindowCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DialogSurface"))
                {
                    dialogSurfaceCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DialogButtonBox"))
                {
                    dialogButtonBoxCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DialogButton"))
                {
                    dialogButtonCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DialogCaptionButton"))
                {
                    dialogCaptionButtonCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.OverlayDialogMask"))
                {
                    overlayDialogMaskCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.OverlayDialogResizer"))
                {
                    overlayDialogResizerCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.MessageBoxContent"))
                {
                    messageBoxContentCount++;
                }
                if (visual is StackPanel)
                {
                    stackPanelCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Segmented"))
                {
                    segmentedCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SegmentedItem"))
                {
                    segmentedItemCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Controls.Commons.SegmentedStackPanel"))
                {
                    segmentedStackPanelCount++;
                }
                if (type.Name == "WaveSpiritDecorator")
                {
                    waveSpiritDecoratorCount++;
                }
                if (type.Name == "DashedBorder")
                {
                    dashedBorderCount++;
                }
                if (IsAddOnDecoratedBox(type))
                {
                    addOnDecoratedBoxCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Select"))
                {
                    selectCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.TreeSelect"))
                {
                    treeSelectCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Cascader"))
                {
                    cascaderCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ComboBox"))
                {
                    comboBoxCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ComboBoxItem"))
                {
                    comboBoxItemCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ComboBoxHandle"))
                {
                    comboBoxHandleCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ComboBoxAccessoryHost"))
                {
                    comboBoxAccessoryHostCount++;
                }
                if (visual is Border { Name: "PopupFrame" } &&
                    visual.GetVisualAncestors().Any(ancestor =>
                        IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.ComboBox")))
                {
                    comboBoxPopupFrameCount++;
                }
                if (visual is AtomUI.Desktop.Controls.ScrollViewer &&
                    visual.GetVisualAncestors().Any(ancestor =>
                        IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.ComboBox")))
                {
                    comboBoxScrollViewerCount++;
                }
                if (visual is ItemsPresenter { Name: "PART_ItemsPresenter" } &&
                    visual.GetVisualAncestors().Any(ancestor =>
                        IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.ComboBox")))
                {
                    comboBoxItemsPresenterCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SelectHandle"))
                {
                    selectHandleCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SelectAccessoryHost"))
                {
                    selectAccessoryHostCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SelectCandidateList"))
                {
                    selectCandidateListCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SelectFilterTextBox"))
                {
                    selectFilterTextBoxCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SelectResultOptionsBox"))
                {
                    selectResultOptionsBoxCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SelectTagAwareTextBox"))
                {
                    selectTagAwareTextBoxCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.TreeSelectTreeView"))
                {
                    treeSelectTreeViewCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CascaderView"))
                {
                    cascaderViewCount++;
                }
                if (visual is Avalonia.Controls.Primitives.Popup)
                {
                    popupCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.AbstractAutoComplete"))
                {
                    autoCompleteCount++;
                    if (HasFieldValue(visual, "AtomUI.Desktop.Controls.AbstractAutoComplete", "_popup"))
                    {
                        autoCompletePopupFieldCount++;
                    }
                    if (HasFieldValue(visual, "AtomUI.Desktop.Controls.AbstractAutoComplete", "_candidateList"))
                    {
                        autoCompleteCandidateListFieldCount++;
                    }
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.AutoCompleteSearchEdit"))
                {
                    autoCompleteSearchEditCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.AutoCompleteTextArea"))
                {
                    autoCompleteTextAreaCount++;
                }
                if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Primitives.CandidateList"))
                {
                    candidateListCount++;
                }
            }

            logicalCount += root.GetSelfAndLogicalDescendants().Count();
        }

        var rootCount = Math.Max(1, roots.Count);
        return new TreeStats(
            visualCount / (double)rootCount,
            logicalCount / (double)rootCount,
            contentPresenterCount / (double)rootCount,
            spaceCount / (double)rootCount,
            compactSpaceCount / (double)rootCount,
            compactSpaceItemCount / (double)rootCount,
            compactSpaceAddOnCount / (double)rootCount,
            buttonCount / (double)rootCount,
            iconButtonCount / (double)rootCount,
            textBlockCount / (double)rootCount,
            panelCount / (double)rootCount,
            borderCount / (double)rootCount,
            dockPanelCount / (double)rootCount,
            scrollViewerCount / (double)rootCount,
            scrollBarCount / (double)rootCount,
            scrollBarThumbCount / (double)rootCount,
            scrollBarRepeatButtonCount / (double)rootCount,
            scrollContentPresenterCount / (double)rootCount,
            scopeAwareOverlayLayerPanelCount / (double)rootCount,
            scopeAwareOverlayLayerCount / (double)rootCount,
            iconCount / (double)rootCount,
            iconPresenterCount / (double)rootCount,
            sizeTypeAwareIconPresenterCount / (double)rootCount,
            buttonIconPresenterCount / (double)rootCount,
            pathIconCount / (double)rootCount,
            stackPanelCount / (double)rootCount,
            segmentedCount / (double)rootCount,
            segmentedItemCount / (double)rootCount,
            segmentedStackPanelCount / (double)rootCount,
            segmentedIconPresenterCount / (double)rootCount,
            waveSpiritDecoratorCount / (double)rootCount,
            dashedBorderCount / (double)rootCount,
            buttonLoadingHostCount / (double)rootCount,
            buttonSpinnerCount / (double)rootCount,
            buttonSpinnerDecoratedBoxCount / (double)rootCount,
            buttonSpinnerHandleCount / (double)rootCount,
            buttonSpinnerContentPanelCount / (double)rootCount,
            cardCount / (double)rootCount,
            cardActionPanelCount / (double)rootCount,
            cardActionButtonCount / (double)rootCount,
            cardMetaContentCount / (double)rootCount,
            cardGridContentCount / (double)rootCount,
            cardGridItemCount / (double)rootCount,
            cardTabsContentCount / (double)rootCount,
            collapseCount / (double)rootCount,
            collapseItemCount / (double)rootCount,
            collapseContentViewportCount / (double)rootCount,
            collapseExpandButtonCount / (double)rootCount,
            collapseAddOnPresenterCount / (double)rootCount,
            carouselCount / (double)rootCount,
            carouselPageCount / (double)rootCount,
            carouselPaginationCount / (double)rootCount,
            carouselPageIndicatorCount / (double)rootCount,
            carouselNavButtonCount / (double)rootCount,
            carouselLayoutTransformCount / (double)rootCount,
            carouselProgressBorderCount / (double)rootCount,
            carouselPageTransitionFieldCount / (double)rootCount,
            carouselAutoPlayTimerFieldCount / (double)rootCount,
            carouselIndicatorAnimationFieldCount / (double)rootCount,
            skeletonCount / (double)rootCount,
            skeletonAvatarCount / (double)rootCount,
            skeletonTitleCount / (double)rootCount,
            skeletonParagraphCount / (double)rootCount,
            skeletonLineCount / (double)rootCount,
            addOnDecoratedBoxCount / (double)rootCount,
            selectCount / (double)rootCount,
            treeSelectCount / (double)rootCount,
            cascaderCount / (double)rootCount,
            comboBoxCount / (double)rootCount,
            comboBoxItemCount / (double)rootCount,
            comboBoxHandleCount / (double)rootCount,
            comboBoxAccessoryHostCount / (double)rootCount,
            comboBoxPopupFrameCount / (double)rootCount,
            comboBoxScrollViewerCount / (double)rootCount,
            comboBoxItemsPresenterCount / (double)rootCount,
            selectHandleCount / (double)rootCount,
            selectAccessoryHostCount / (double)rootCount,
            selectCandidateListCount / (double)rootCount,
            selectFilterTextBoxCount / (double)rootCount,
            selectResultOptionsBoxCount / (double)rootCount,
            selectTagAwareTextBoxCount / (double)rootCount,
            treeSelectTreeViewCount / (double)rootCount,
            cascaderViewCount / (double)rootCount,
            popupCount / (double)rootCount,
            autoCompleteCount / (double)rootCount,
            autoCompleteSearchEditCount / (double)rootCount,
            autoCompleteTextAreaCount / (double)rootCount,
            candidateListCount / (double)rootCount,
            autoCompletePopupFieldCount / (double)rootCount,
            autoCompleteCandidateListFieldCount / (double)rootCount,
            avatarCount / (double)rootCount,
            avatarGroupCount / (double)rootCount,
            imageCount / (double)rootCount,
            svgCount / (double)rootCount,
            flyoutHostCount / (double)rootCount,
            countBadgeCount / (double)rootCount,
            dotBadgeCount / (double)rootCount,
            ribbonBadgeCount / (double)rootCount,
            countBadgeAdornerCount / (double)rootCount,
            dotBadgeAdornerCount / (double)rootCount,
            ribbonBadgeAdornerCount / (double)rootCount,
            dotBadgeIndicatorCount / (double)rootCount,
            motionTargetCount / (double)rootCount,
            labelCount / (double)rootCount,
            floatButtonHostCount / (double)rootCount,
            floatButtonGroupHostCount / (double)rootCount,
            floatButtonCount / (double)rootCount,
            backTopFloatButtonCount / (double)rootCount,
            floatButtonGroupCount / (double)rootCount,
            floatButtonItemsControlCount / (double)rootCount,
            floatButtonSeparatorLayerCount / (double)rootCount,
            checkBoxCount / (double)rootCount,
            checkBoxGroupCount / (double)rootCount,
            checkBoxItemsControlCount / (double)rootCount,
            checkBoxIndicatorCount / (double)rootCount,
            checkBoxCheckedMarkCount / (double)rootCount,
            checkBoxTristateMarkCount / (double)rootCount,
            radioButtonCount / (double)rootCount,
            radioButtonGroupCount / (double)rootCount,
            radioIndicatorCount / (double)rootCount,
            formCount / (double)rootCount,
            formItemCount / (double)rootCount,
            formActionsItemCount / (double)rootCount,
            formValidateFeedbackCount / (double)rootCount,
            submitButtonCount / (double)rootCount,
            resetButtonCount / (double)rootCount,
            itemDeleteButtonCount / (double)rootCount,
            formTooltipIconPresenterCount / (double)rootCount,
            groupBoxCount / (double)rootCount,
            groupBoxHeaderIconPresenterCount / (double)rootCount,
            descriptionsCount / (double)rootCount,
            descriptionDefaultItemCount / (double)rootCount,
            descriptionBorderedItemLabelCount / (double)rootCount,
            descriptionBorderedItemContentCount / (double)rootCount,
            dialogCount / (double)rootCount,
            messageBoxCount / (double)rootCount,
            overlayDialogPresenterCount / (double)rootCount,
            dialogWindowCount / (double)rootCount,
            dialogSurfaceCount / (double)rootCount,
            dialogButtonBoxCount / (double)rootCount,
            dialogButtonCount / (double)rootCount,
            dialogCaptionButtonCount / (double)rootCount,
            overlayDialogMaskCount / (double)rootCount,
            overlayDialogResizerCount / (double)rootCount,
            messageBoxContentCount / (double)rootCount);
    }

    private static bool IsAtomIcon(Type type)
    {
        while (type.BaseType != null)
        {
            if (type.BaseType.FullName == "AtomUI.Controls.Icon")
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private static bool IsAddOnDecoratedBox(Type type)
    {
        return IsTypeOrDerived(type, "AtomUI.Desktop.Controls.AddOnDecoratedBox");
    }

    private static bool IsTypeOrDerived(Type type, string fullName)
    {
        if (type.FullName == fullName)
        {
            return true;
        }

        while (type.BaseType != null)
        {
            if (type.BaseType.FullName == fullName)
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private static bool HasFieldValue(object target, string declaringTypeName, string fieldName)
    {
        var type = target.GetType();
        while (type is not null)
        {
            if (type.FullName == declaringTypeName)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                return field?.GetValue(target) is not null;
            }

            type = type.BaseType;
        }

        return false;
    }
}
