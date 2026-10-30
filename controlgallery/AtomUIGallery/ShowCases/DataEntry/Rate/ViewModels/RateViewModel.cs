using System.Reactive;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Rate;

public class RateViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Rate";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private IList<string>? _tooltips;
    private double _twoWayValue;
    private string? _twoWayValueSummary;

    public IList<string>? Tooltips
    {
        get => _tooltips;
        set => this.RaiseAndSetIfChanged(ref _tooltips, value);
    }

    public double TwoWayValue
    {
        get => _twoWayValue;
        set
        {
            if (Math.Abs(_twoWayValue - value) < double.Epsilon)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _twoWayValue, value);
            UpdateTwoWayValueSummary();
        }
    }

    public string? TwoWayValueSummary
    {
        get => _twoWayValueSummary;
        set => this.RaiseAndSetIfChanged(ref _twoWayValueSummary, value);
    }

    private string? _activeTooltip;

    public string? ActiveTooltip
    {
        get => _activeTooltip;
        set => this.RaiseAndSetIfChanged(ref _activeTooltip, value);
    }

    public ReactiveCommand<Unit, Unit> SetFourStarsCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearTwoWayValueCommand { get; }

    public RateViewModel(IScreen screen)
    {
        HostScreen = screen;
        _twoWayValue = 2.0;
        SetFourStarsCommand      = ReactiveCommand.Create(HandleSetFourStars);
        ClearTwoWayValueCommand  = ReactiveCommand.Create(HandleClearTwoWayValue);
        UpdateTwoWayValueSummary();
    }

    public void RefreshLocalizedState()
    {
        UpdateTwoWayValueSummary();
    }

    private void HandleSetFourStars()
    {
        TwoWayValue = 4.0;
    }

    private void HandleClearTwoWayValue()
    {
        TwoWayValue = 0.0;
    }

    private void UpdateTwoWayValueSummary()
    {
        TwoWayValueSummary = RateShowCaseLanguage.Format(
            RateShowCaseLangResourceKind.P2TwoWayValueSummaryFormat,
            "Selected value: {0:0.#}",
            TwoWayValue);
    }

}
