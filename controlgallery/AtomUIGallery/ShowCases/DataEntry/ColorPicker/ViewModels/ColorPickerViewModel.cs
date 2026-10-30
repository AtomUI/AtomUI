using System.Reactive;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Media;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ColorPicker;

public class ColorPickerViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "ColorPicker";
    
    public IScreen HostScreen { get; }
    
    public string? UrlPathSegment => ID.ToString();

    private Color? _boundColorValue = Color.Parse("#1677ff");
    private LinearGradientBrush? _boundGradientValue = CreateGradient("#108ee9", "#87d068");

    public Color? BoundColorValue
    {
        get => _boundColorValue;
        set
        {
            if (_boundColorValue == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _boundColorValue, value);
            this.RaisePropertyChanged(nameof(BoundColorValueText));
        }
    }

    public string BoundColorValueText => BoundColorValue?.ToString() ?? "-";

    public LinearGradientBrush? BoundGradientValue
    {
        get => _boundGradientValue;
        set
        {
            if (ReferenceEquals(_boundGradientValue, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _boundGradientValue, value);
            this.RaisePropertyChanged(nameof(BoundGradientValueText));
        }
    }

    public string BoundGradientValueText => FormatGradientValue(BoundGradientValue);

    public ReactiveCommand<Unit, Unit> SetBoundColorValueCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearBoundColorValueCommand { get; }

    public ReactiveCommand<Unit, Unit> SetBoundGradientValueCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearBoundGradientValueCommand { get; }

    public ColorPickerViewModel(IScreen screen)
    {
        HostScreen                         = screen;
        SetBoundColorValueCommand          = ReactiveCommand.Create(SetBoundColorValue);
        ClearBoundColorValueCommand        = ReactiveCommand.Create(ClearBoundColorValue);
        SetBoundGradientValueCommand       = ReactiveCommand.Create(SetBoundGradientValue);
        ClearBoundGradientValueCommand     = ReactiveCommand.Create(ClearBoundGradientValue);
    }

    private void SetBoundColorValue()
    {
        BoundColorValue = Color.Parse("#722ed1");
    }

    private void ClearBoundColorValue()
    {
        BoundColorValue = null;
    }

    private void SetBoundGradientValue()
    {
        BoundGradientValue = CreateGradient("#f5222d", "#faad14");
    }

    private void ClearBoundGradientValue()
    {
        BoundGradientValue = null;
    }

    private static LinearGradientBrush CreateGradient(string startColor, string endColor)
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint   = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.Parse(startColor), 0),
                new GradientStop(Color.Parse(endColor), 1)
            ]
        };
    }

    private static string FormatGradientValue(LinearGradientBrush? brush)
    {
        if (brush?.GradientStops is not { Count: > 0 } stops)
        {
            return "-";
        }

        return string.Join(" → ", stops.Select(stop =>
            string.Format(GalleryLocalization.GetFormattingCulture(), "{0} {1:0.#}%", stop.Color, stop.Offset * 100)));
    }
}
