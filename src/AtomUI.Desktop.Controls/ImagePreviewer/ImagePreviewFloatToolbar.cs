using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class ImagePreviewFloatToolbar : ImagePreviewBaseToolbar
{
    public static readonly DirectProperty<ImagePreviewFloatToolbar, string?> IndicatorTextProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewFloatToolbar, string?>(
            nameof(IndicatorText),
            o => o.IndicatorText,
            (o, v) => o.IndicatorText = v);
    
    private string? _indicatorText;

    public string? IndicatorText
    {
        get => _indicatorText;
        internal set => SetAndRaise(IndicatorTextProperty, ref _indicatorText, value);
    }

    static ImagePreviewFloatToolbar()
    {
        AffectsMeasure<ImagePreviewFloatToolbar>(IndicatorTextProperty);
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CurrentIndexProperty ||
            change.Property == CountProperty)
        {
            if (Count != 0)
            {
                SetCurrentValue(IndicatorTextProperty, $"{CurrentIndex + 1} / {Count}");
            }
        }
    }
}