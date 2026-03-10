using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Media;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

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
    }
    
    public void AddProgressValue()
    {
        var value = ProgressValue;
        value         += 10;
        ProgressValue =  Math.Min(value, 100);
    }

    public void SubProgressValue()
    {
        var value = ProgressValue;
        value         -= 10;
        ProgressValue =  Math.Max(value, 0);
    }

    public void ToggleEnabledStatus()
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
}