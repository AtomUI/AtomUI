using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Step indicator that shows the position in the tour as text such as <c>2 / 5</c>: the one-based number of the
/// active step, a slash, and the number of steps. The text follows <see cref="TourIndicator.ActiveIndex" /> and
/// <see cref="TourIndicator.StepCount" />. Assign it to <see cref="Tour.Indicator" /> of the tour.
/// </summary>
public class TextTourIndicator : TourIndicator
{
    #region 内部属性定义

    internal static readonly DirectProperty<TextTourIndicator, string?> IndicatorTextProperty =
        AvaloniaProperty.RegisterDirect<TextTourIndicator, string?>(nameof(IndicatorText),
            o => o.IndicatorText,
            (o, v) => o.IndicatorText = v);
    
    private string? _indicatorText;

    internal string? IndicatorText
    {
        get => _indicatorText;
        private set => SetAndRaise(IndicatorTextProperty, ref _indicatorText, value);
    }

    #endregion

    /// <summary>
    /// Called when the value of a property changes. After the base call it rebuilds the indicator text when <see
    /// cref="TourIndicator.ActiveIndex" /> or <see cref="TourIndicator.StepCount" /> changed. A further override must
    /// call this member to keep the text up to date.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ActiveIndexProperty ||
            change.Property == StepCountProperty)
        {
            NotifyBuildIndicatorText();
        }
    }

    /// <summary>
    /// Builds the indicator text from <see cref="TourIndicator.ActiveIndex" /> and <see cref="TourIndicator.StepCount" /> and stores it in
    /// <see cref="IndicatorText" />, which the template shows. It is called whenever one of the two properties
    /// changes. Override it to change the format; call the base member to apply the default text.
    /// </summary>
    protected virtual void NotifyBuildIndicatorText()
    {
        SetCurrentValue(IndicatorTextProperty, $"{ActiveIndex + 1} / {StepCount}");
    }
}