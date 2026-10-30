using System.Reactive;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Slider;

public class SliderViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Slider";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private List<SliderMark>? _sliderMarks;
    private IReadOnlyList<double> _boundRangeValues = [20, 60];
    private IReadOnlyList<bool> _disabledHandles = [false, false, false];
    private bool _isHandle1Disabled;
    private bool _isHandle2Disabled;
    private bool _isHandle3Disabled;

    public List<SliderMark>? SliderMarks
    {
        get => _sliderMarks;
        set => this.RaiseAndSetIfChanged(ref _sliderMarks, value);
    }

    private bool _normalEnabled = true;

    public bool NormalEnabled
    {
        get => _normalEnabled;
        set => this.RaiseAndSetIfChanged(ref _normalEnabled, value);
    }

    public IReadOnlyList<double> DefaultRangeValues { get; } = [20, 80];

    public IReadOnlyList<double> MultiHandleRangeValues { get; } = [0, 35, 100];

    public IReadOnlyList<double> DisabledHandleRangeValues { get; } = [20, 50, 80];

    public IReadOnlyList<double> SemanticPartPreviewRangeValues { get; } = [20, 30, 50];

    public IReadOnlyList<bool> DisabledHandles => _disabledHandles;

    public bool IsHandle1Disabled
    {
        get => _isHandle1Disabled;
        set
        {
            if (_isHandle1Disabled == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _isHandle1Disabled, value);
            UpdateDisabledHandles();
        }
    }

    public bool IsHandle2Disabled
    {
        get => _isHandle2Disabled;
        set
        {
            if (_isHandle2Disabled == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _isHandle2Disabled, value);
            UpdateDisabledHandles();
        }
    }

    public bool IsHandle3Disabled
    {
        get => _isHandle3Disabled;
        set
        {
            if (_isHandle3Disabled == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _isHandle3Disabled, value);
            UpdateDisabledHandles();
        }
    }

    public IReadOnlyList<double> BoundRangeValues
    {
        get => _boundRangeValues;
        set
        {
            if (_boundRangeValues.SequenceEqual(value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _boundRangeValues, value);
            this.RaisePropertyChanged(nameof(BoundRangeValuesText));
        }
    }

    public string BoundRangeValuesText => string.Join(
        " - ",
        BoundRangeValues.Select(value => value.ToString("0.#", GalleryLocalization.GetFormattingCulture())));

    public ReactiveCommand<Unit, Unit> SetBoundRangeValuesCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearBoundRangeValuesCommand { get; }

    public SliderViewModel(IScreen screen)
    {
        HostScreen                   = screen;
        SetBoundRangeValuesCommand   = ReactiveCommand.Create(SetBoundRangeValues);
        ClearBoundRangeValuesCommand = ReactiveCommand.Create(ClearBoundRangeValues);
    }

    private void SetBoundRangeValues()
    {
        BoundRangeValues = [35, 85];
    }

    private void ClearBoundRangeValues()
    {
        BoundRangeValues = [0, 0];
    }

    private void UpdateDisabledHandles()
    {
        _disabledHandles = [IsHandle1Disabled, IsHandle2Disabled, IsHandle3Disabled];
        this.RaisePropertyChanged(nameof(DisabledHandles));
    }
}
