using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUIGallery.ShowCases.Tour;

public partial class TourShowCase : GalleryReactiveUserControl<TourViewModel>
{
    public const string LanguageId = nameof(TourShowCase);

    public TourShowCase()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

    }

    private void HandleBasicBeginTour(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.BasicCaseTourOpened = true;
        }
    }

    private void HandleNonMaskBeginTour(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.NonMaskTourOpened = true;
        }
    }

    private void HandlePlacementBeginTour(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.PlacementTourOpened = true;
        }
    }

    private void HandleCustomIndicatorBeginTour(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.CustomIndicatorTourOpened = true;
        }
    }

    private void HandleCustomMaskBeginTour(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.CustomMaskTourOpened = true;
        }
    }

    private void HandleCustomGapBeginTour(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.CustomGapTourOpened = true;
        }
    }

    private void HandleCustomActionBeginTour(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.CustomActionTourOpened = true;
        }
    }

    private void HandleSemanticStylesObjectBeginTour(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.SemanticStylesObjectTourOpened = true;
        }
    }

    private void HandleSemanticStylesFunctionBeginTour(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.SemanticStylesFunctionTourOpened = true;
        }
    }

    private void HandleTourExampleLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Control root)
        {
            return;
        }

        SetTourStepTarget(root, "BasicCaseUploadStep", "BasicCaseUpload");        SetTourStepTarget(root, "BasicCaseSaveStep", "BasicCaseSave");
        SetTourStepTarget(root, "BasicCaseEllipsisStep", "BasicCaseEllipsis");
        SetTourStepTarget(root, "NonMaskCaseUploadStep", "NonMaskCaseUpload");
        SetTourStepTarget(root, "NonMaskSaveStep", "NonMaskSave");
        SetTourStepTarget(root, "NonMaskEllipsisStep", "NonMaskEllipsis");
        SetTourStepTarget(root, "PlacementRightStep", "PlacementBeginTour");
        SetTourStepTarget(root, "PlacementTopStep", "PlacementBeginTour");
        SetTourStepTarget(root, "PlacementLeftStep", "PlacementBeginTour");
        SetTourStepTarget(root, "CustomIndicatorUploadStep", "CustomIndicatorUpload");
        SetTourStepTarget(root, "CustomIndicatorSaveStep", "CustomIndicatorSave");
        SetTourStepTarget(root, "CustomIndicatorEllipsisStep", "CustomIndicatorEllipsis");
        SetTourStepTarget(root, "CustomMaskUploadStep", "CustomMaskUpload");
        SetTourStepTarget(root, "CustomMaskSaveStep", "CustomMaskSave");
        SetTourStepTarget(root, "CustomMaskEllipsisStep", "CustomMaskEllipsis");
        SetTourStepTarget(root, "CustomActionUploadStep", "CustomActionUpload");
        SetTourStepTarget(root, "CustomActionSaveStep", "CustomActionSave");
        SetTourStepTarget(root, "CustomActionEllipsisStep", "CustomActionEllipsis");
        SetTourStepTarget(root, "CustomGapStep", "CustomGapControl");
        SetTourStepTarget(root, "SemanticStylesObjectUploadStep", "SemanticStylesUpload");
        SetTourStepTarget(root, "SemanticStylesObjectSaveStep", "SemanticStylesSave");
        SetTourStepTarget(root, "SemanticStylesObjectEllipsisStep", "SemanticStylesEllipsis");
        SetTourStepTarget(root, "SemanticStylesFunctionUploadStep", "SemanticStylesUpload");
        SetTourStepTarget(root, "SemanticStylesFunctionSaveStep", "SemanticStylesSave");
        SetTourStepTarget(root, "SemanticStylesFunctionEllipsisStep", "SemanticStylesEllipsis");
    }

    // Semantic preview stage: as in the antd _semantic demo, both steps anchor to the centered Show button.
    // The tour opens on Loaded because IsPopupPinnedOpen is set, so the anchor is assigned during the attach
    // phase to keep the first step on the Show button instead of an empty target.
    private void HandleSemanticStageAttached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Control stage)
        {
            return;
        }

        SetTourStepTarget(stage, "TourSemanticHelloStep", "TourSemanticAnchorButton");
        SetTourStepTarget(stage, "TourSemanticSaveStep", "TourSemanticAnchorButton");
    }

    internal static void SetTourStepTarget(Control root, string stepName, string targetName)
    {
        var step = FindDescendantByName<TourStep>(root, stepName)
                   ?? root.GetVisualDescendants()
                          .OfType<AtomUITour>()
                          .SelectMany(tour => tour.Steps.Cast<object?>())
                          .OfType<TourStep>()
                          .FirstOrDefault(candidate => candidate.Name == stepName);
        var target = FindDescendantByName<Control>(root, targetName);
        if (step is not null && target is not null)
        {
            step.Target = target;
        }
    }

    private static T? FindDescendantByName<T>(Control root, string name)
        where T : Control
    {
        if (root is T typedRoot && typedRoot.Name == name)
        {
            return typedRoot;
        }

        return root.GetVisualDescendants().OfType<T>().FirstOrDefault(control => control.Name == name)
               ?? root.GetLogicalDescendants().OfType<T>().FirstOrDefault(control => control.Name == name);
    }
}

public class SkipTourActionButton : AtomUIButton, ITourAction
{
    static SkipTourActionButton()
    {
        AtomUITour.StyleTypeProperty.AddOwner<SkipTourActionButton>();
        SizeTypeProperty.OverrideDefaultValue<SkipTourActionButton>(AtomUI.CustomizableSizeType.Small);
        ButtonTypeProperty.OverrideDefaultValue<SkipTourActionButton>(ButtonType.Default);
    }

    public int StepCount { get; set; }
    public int ActiveIndex { get; set; }
    public TourStyleType StyleType { get; set; }
}
