using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Tour;

public class TourViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Tour";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private bool _basicCaseTourOpened;

    public bool BasicCaseTourOpened
    {
        get => _basicCaseTourOpened;
        set => this.RaiseAndSetIfChanged(ref _basicCaseTourOpened, value);
    }

    private bool _nonMaskTourOpened;

    public bool NonMaskTourOpened
    {
        get => _nonMaskTourOpened;
        set => this.RaiseAndSetIfChanged(ref _nonMaskTourOpened, value);
    }

    private bool _placementTourOpened;

    public bool PlacementTourOpened
    {
        get => _placementTourOpened;
        set => this.RaiseAndSetIfChanged(ref _placementTourOpened, value);
    }

    private bool _customIndicatorTourOpened;

    public bool CustomIndicatorTourOpened
    {
        get => _customIndicatorTourOpened;
        set => this.RaiseAndSetIfChanged(ref _customIndicatorTourOpened, value);
    }

    private bool _customMaskTourOpened;

    public bool CustomMaskTourOpened
    {
        get => _customMaskTourOpened;
        set => this.RaiseAndSetIfChanged(ref _customMaskTourOpened, value);
    }

    private bool _customGapTourOpened;

    public bool CustomGapTourOpened
    {
        get => _customGapTourOpened;
        set => this.RaiseAndSetIfChanged(ref _customGapTourOpened, value);
    }

    private double _customGapRadius = 2;

    public double CustomGapRadius
    {
        get => _customGapRadius;
        set => this.RaiseAndSetIfChanged(ref _customGapRadius, value);
    }

    private double _customGapOffsetX;

    public double CustomGapOffsetX
    {
        get => _customGapOffsetX;
        set => this.RaiseAndSetIfChanged(ref _customGapOffsetX, value);
    }

    private double _customGapOffsetY;

    public double CustomGapOffsetY
    {
        get => _customGapOffsetY;
        set => this.RaiseAndSetIfChanged(ref _customGapOffsetY, value);
    }

    private IList<ITourStepOption>? _basicCaseSteps;

    public IList<ITourStepOption>? BasicCaseSteps
    {
        get => _basicCaseSteps;
        set => this.RaiseAndSetIfChanged(ref _basicCaseSteps, value);
    }

    private bool _customActionTourOpened;

    public bool CustomActionTourOpened
    {
        get => _customActionTourOpened;
        set => this.RaiseAndSetIfChanged(ref _customActionTourOpened, value);
    }

    private bool _semanticStylesObjectTourOpened;

    public bool SemanticStylesObjectTourOpened
    {
        get => _semanticStylesObjectTourOpened;
        set => this.RaiseAndSetIfChanged(ref _semanticStylesObjectTourOpened, value);
    }

    private bool _semanticStylesFunctionTourOpened;

    public bool SemanticStylesFunctionTourOpened
    {
        get => _semanticStylesFunctionTourOpened;
        set => this.RaiseAndSetIfChanged(ref _semanticStylesFunctionTourOpened, value);
    }

    public TourViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

}
