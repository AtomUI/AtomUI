using AtomUI.Controls;
using Avalonia;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Steps;

public class StepsViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Steps";

    private const int InteractiveLastStepIndex = 2;

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    private int _current;

    public int Current
    {
        get => _current;
        set
        {
            this.RaiseAndSetIfChanged(ref _current, value);
            this.RaisePropertyChanged(nameof(CurrentText));
            PreviousButtonVisible = Current > 0;
            RefreshInteractiveText();
        }
    }

    public string CurrentText => Current.ToString(GalleryLocalization.GetFormattingCulture());

    public string InteractivePageContent => Current switch
    {
        0 => Lang(StepsShowCaseLangResourceKind.P2ContentFirstContent),
        1 => Lang(StepsShowCaseLangResourceKind.P2ContentSecondContent),
        2 => Lang(StepsShowCaseLangResourceKind.P2ContentLastContent),
        _ => string.Empty
    };

    private bool _previousButtonVisible;

    public bool PreviousButtonVisible
    {
        get => _previousButtonVisible;
        set => this.RaiseAndSetIfChanged(ref _previousButtonVisible, value);
    }

    private string _nextButtonText = Lang(StepsShowCaseLangResourceKind.P2ContentNext);

    public string NextButtonText
    {
        get => _nextButtonText;
        set => this.RaiseAndSetIfChanged(ref _nextButtonText, value);
    }

    public StepsViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

    public void ResetInteractiveStep()
    {
        Current = 0;
    }

    public void MoveToNextInteractiveStep()
    {
        if (Current < InteractiveLastStepIndex)
        {
            Current++;
        }
    }

    public void MoveToPreviousInteractiveStep()
    {
        if (Current > 0)
        {
            Current--;
        }
    }

    public void RefreshInteractiveText()
    {
        NextButtonText = Current == InteractiveLastStepIndex
            ? Lang(StepsShowCaseLangResourceKind.P2ContentDone)
            : Lang(StepsShowCaseLangResourceKind.P2ContentNext);
        this.RaisePropertyChanged(nameof(InteractivePageContent));
    }

    private static string Lang(StepsShowCaseLangResourceKind kind)
    {
        return Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)?.Get(kind) ?? kind.ToString()
            : kind.ToString();
    }
}
