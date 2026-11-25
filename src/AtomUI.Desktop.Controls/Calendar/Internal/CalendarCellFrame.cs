using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls.Internal.Calendar;

internal sealed class CalendarCellFrame : Border
{
    public static readonly StyledProperty<double> MinimumDesiredHeightProperty =
        AvaloniaProperty.Register<CalendarCellFrame, double>(nameof(MinimumDesiredHeight));

    public double MinimumDesiredHeight
    {
        get => GetValue(MinimumDesiredHeightProperty);
        set => SetValue(MinimumDesiredHeightProperty, value);
    }

    static CalendarCellFrame()
    {
        AffectsMeasure<CalendarCellFrame>(MinimumDesiredHeightProperty);
    }

    protected override Type StyleKeyOverride => typeof(Border);

    protected override Size MeasureOverride(Size availableSize)
    {
        var desired = base.MeasureOverride(availableSize);
        // Reserve the natural cell height on measure while allowing the grid to constrain its rows.
        var minimum = MinimumDesiredHeight;
        return double.IsFinite(minimum) ? desired.WithHeight(Math.Max(desired.Height, minimum)) : desired;
    }
}
