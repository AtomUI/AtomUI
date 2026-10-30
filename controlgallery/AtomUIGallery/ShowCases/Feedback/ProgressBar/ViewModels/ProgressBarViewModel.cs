using AtomUI.Controls;
using Avalonia;
using Avalonia.Media;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ProgressBar;

public class ProgressBarViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "ProgressBar";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    private LinearGradientBrush _twoStopsGradientStrokeColor;

    public LinearGradientBrush TwoStopsGradientStrokeColor
    {
        get => _twoStopsGradientStrokeColor;
        set => this.RaiseAndSetIfChanged(ref _twoStopsGradientStrokeColor, value);
    }

    private LinearGradientBrush _threeStopsGradientStrokeColor;

    public LinearGradientBrush ThreeStopsGradientStrokeColor
    {
        get => _threeStopsGradientStrokeColor;
        set => this.RaiseAndSetIfChanged(ref _threeStopsGradientStrokeColor, value);
    }

    private List<IBrush> _stepsChunkBrushes;

    public List<IBrush> StepsChunkBrushes
    {
        get => _stepsChunkBrushes;
        set => this.RaiseAndSetIfChanged(ref _stepsChunkBrushes, value);
    }

    private PercentPosition _innerStartPercentPosition;

    public PercentPosition InnerStartPercentPosition
    {
        get => _innerStartPercentPosition;
        set => this.RaiseAndSetIfChanged(ref _innerStartPercentPosition, value);
    }

    private PercentPosition _innerCenterPercentPosition;

    public PercentPosition InnerCenterPercentPosition
    {
        get => _innerCenterPercentPosition;
        set => this.RaiseAndSetIfChanged(ref _innerCenterPercentPosition, value);
    }

    private PercentPosition _innerEndPercentPosition;

    public PercentPosition InnerEndPercentPosition
    {
        get => _innerEndPercentPosition;
        set => this.RaiseAndSetIfChanged(ref _innerEndPercentPosition, value);
    }

    private PercentPosition _outerStartPercentPosition;

    public PercentPosition OuterStartPercentPosition
    {
        get => _outerStartPercentPosition;
        set => this.RaiseAndSetIfChanged(ref _outerStartPercentPosition, value);
    }

    private PercentPosition _outerCenterPercentPosition;

    public PercentPosition OuterCenterPercentPosition
    {
        get => _outerCenterPercentPosition;
        set => this.RaiseAndSetIfChanged(ref _outerCenterPercentPosition, value);
    }

    private PercentPosition _outerEndPercentPosition;

    public PercentPosition OuterEndPercentPosition
    {
        get => _outerEndPercentPosition;
        set => this.RaiseAndSetIfChanged(ref _outerEndPercentPosition, value);
    }

    private double _progressValue = 30;

    public double ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }

    private string? _toggleDisabledText;

    public string? ToggleDisabledText
    {
        get => _toggleDisabledText;
        set => this.RaiseAndSetIfChanged(ref _toggleDisabledText, value);
    }

    private bool _toggleStatus;

    public bool ToggleStatus
    {
        get => _toggleStatus;
        set => this.RaiseAndSetIfChanged(ref _toggleStatus, value);
    }

    private int _semanticPreviewIndex;

    public int SemanticPreviewIndex
    {
        get => _semanticPreviewIndex;
        set
        {
            if (_semanticPreviewIndex == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _semanticPreviewIndex, value);
            this.RaisePropertyChanged(nameof(IsLineSemanticPreviewVisible));
            this.RaisePropertyChanged(nameof(IsStepsSemanticPreviewVisible));
            this.RaisePropertyChanged(nameof(IsCircleSemanticPreviewVisible));
            this.RaisePropertyChanged(nameof(IsDashboardSemanticPreviewVisible));
        }
    }

    public bool IsLineSemanticPreviewVisible => SemanticPreviewIndex == 0;
    public bool IsStepsSemanticPreviewVisible => SemanticPreviewIndex == 1;
    public bool IsCircleSemanticPreviewVisible => SemanticPreviewIndex == 2;
    public bool IsDashboardSemanticPreviewVisible => SemanticPreviewIndex == 3;

    private bool _isSemanticGradientEnabled;

    public bool IsSemanticGradientEnabled
    {
        get => _isSemanticGradientEnabled;
        set => this.RaiseAndSetIfChanged(ref _isSemanticGradientEnabled, value);
    }

    public LinearGradientBrush SemanticPreviewGradientBrush { get; }
    public LinearGradientBrush SemanticTrackBrush10 { get; }
    public LinearGradientBrush SemanticTrackBrush20 { get; }
    public LinearGradientBrush SemanticTrackBrush40 { get; }
    public LinearGradientBrush SemanticTrackBrush60 { get; }
    public LinearGradientBrush SemanticTrackBrush80 { get; }
    public LinearGradientBrush SemanticTrackBrush99 { get; }

    public ProgressBarViewModel(IScreen screen)
    {
        HostScreen = screen;
        _twoStopsGradientStrokeColor = new LinearGradientBrush
        {
            GradientStops =
            {
                new GradientStop(Color.Parse("#108ee9"), 0),
                new GradientStop(Color.Parse("#87d068"), 1)
            }
        };
        _threeStopsGradientStrokeColor = new LinearGradientBrush
        {
            GradientStops =
            {
                new GradientStop(Color.Parse("#87d068"), 0),
                new GradientStop(Color.Parse("#ffe58f"), 0.5),
                new GradientStop(Color.Parse("#ffccc7"), 1)
            }
        };
        SemanticPreviewGradientBrush = CreateHorizontalGradient(
            Color.Parse("#108ee9"),
            Color.Parse("#87d068"));
        SemanticTrackBrush10 = CreateSemanticTrackBrush(10);
        SemanticTrackBrush20 = CreateSemanticTrackBrush(20);
        SemanticTrackBrush40 = CreateSemanticTrackBrush(40);
        SemanticTrackBrush60 = CreateSemanticTrackBrush(60);
        SemanticTrackBrush80 = CreateSemanticTrackBrush(80);
        SemanticTrackBrush99 = CreateSemanticTrackBrush(99);
        _stepsChunkBrushes = new List<IBrush>
        {
            new SolidColorBrush(Colors.Green),
            new SolidColorBrush(Colors.Green),
            new SolidColorBrush(Colors.Red)
        };

        _innerStartPercentPosition = new PercentPosition
        {
            IsInner   = true,
            Alignment = LinePercentAlignment.Start
        };
        _innerCenterPercentPosition = new PercentPosition
        {
            IsInner   = true,
            Alignment = LinePercentAlignment.Center
        };
        _innerEndPercentPosition = new PercentPosition
        {
            IsInner   = true,
            Alignment = LinePercentAlignment.End
        };

        _outerStartPercentPosition = new PercentPosition
        {
            IsInner   = false,
            Alignment = LinePercentAlignment.Start
        };
        _outerCenterPercentPosition = new PercentPosition
        {
            IsInner   = false,
            Alignment = LinePercentAlignment.Center
        };
        _outerEndPercentPosition = new PercentPosition
        {
            IsInner   = false,
            Alignment = LinePercentAlignment.End
        };
        _toggleStatus       = true;
        _toggleDisabledText = "Disable";

        AddProgressValue = ReactiveCommand.Create(AddProgressValueImpl);
        SubProgressValue = ReactiveCommand.Create(SubProgressValueImpl);
        ToggleEnabledStatus = ReactiveCommand.Create(ToggleEnabledStatusImpl);
    }

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddProgressValue { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> SubProgressValue { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ToggleEnabledStatus { get; }

    private void AddProgressValueImpl()
    {
        var value = ProgressValue;
        value         += 10;
        ProgressValue =  Math.Min(value, 100);
    }

    private void SubProgressValueImpl()
    {
        var value = ProgressValue;
        value         -= 10;
        ProgressValue =  Math.Max(value, 0);
    }

    private void ToggleEnabledStatusImpl()
    {
        ToggleStatus = !ToggleStatus;
        if (ToggleStatus)
        {
            ToggleDisabledText = "Disable";
        }
        else
        {
            ToggleDisabledText = "Enable";
        }
    }

    private static LinearGradientBrush CreateSemanticTrackBrush(double percent)
    {
        var hue = Math.Round(200 - percent * 2);
        return CreateHorizontalGradient(
            HslColor.FromAhsl(1, hue, 0.85, 0.65).ToRgb(),
            HslColor.FromAhsl(0.95, hue + 30, 0.9, 0.55).ToRgb());
    }

    private static LinearGradientBrush CreateHorizontalGradient(Color start, Color end)
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(start, 0),
                new GradientStop(end, 1)
            }
        };
    }

}
