using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A bar that a <see cref="Calendar" /> draws across the date cells from <see cref="StartDate" /> to
/// <see cref="EndDate" />, with an optional <see cref="Label" />.
/// Add instances to <see cref="Calendar.RangeBars" />, which also states when the calendar shows them.
/// </summary>
[GenerateScopedResourceHost]
public partial class CalendarRangeBar : AvaloniaObject
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="StartDate" /> property.
    /// </summary>
    public static readonly DirectProperty<CalendarRangeBar, DateTime?> StartDateProperty =
        AvaloniaProperty.RegisterDirect<CalendarRangeBar, DateTime?>(
            nameof(StartDate),
            o => o.StartDate,
            (o, v) => o.StartDate = v);

    /// <summary>
    /// Identifies the <see cref="EndDate" /> property.
    /// </summary>
    public static readonly DirectProperty<CalendarRangeBar, DateTime?> EndDateProperty =
        AvaloniaProperty.RegisterDirect<CalendarRangeBar, DateTime?>(
            nameof(EndDate),
            o => o.EndDate,
            (o, v) => o.EndDate = v);

    /// <summary>
    /// Identifies the <see cref="Label" /> property.
    /// </summary>
    public static readonly DirectProperty<CalendarRangeBar, object?> LabelProperty =
        AvaloniaProperty.RegisterDirect<CalendarRangeBar, object?>(
            nameof(Label),
            o => o.Label,
            (o, v) => o.Label = v);

    /// <summary>
    /// Identifies the <see cref="Background" /> property.
    /// </summary>
    public static readonly DirectProperty<CalendarRangeBar, IBrush?> BackgroundProperty =
        AvaloniaProperty.RegisterDirect<CalendarRangeBar, IBrush?>(
            nameof(Background),
            o => o.Background,
            (o, v) => o.Background = v);

    /// <summary>
    /// Identifies the <see cref="Height" /> property.
    /// </summary>
    public static readonly DirectProperty<CalendarRangeBar, double> HeightProperty =
        AvaloniaProperty.RegisterDirect<CalendarRangeBar, double>(
            nameof(Height),
            o => o.Height,
            (o, v) => o.Height = v);

    private DateTime? _startDate;

    /// <summary>
    /// The first date covered by the bar, inclusive. A time of day is ignored.
    /// <see langword="null" />, the default, means that the bar is not drawn.
    /// </summary>
    public DateTime? StartDate
    {
        get => _startDate;
        set => SetAndRaise(StartDateProperty, ref _startDate, value);
    }

    private DateTime? _endDate;

    /// <summary>
    /// The last date covered by the bar, inclusive. A time of day is ignored.
    /// <see langword="null" />, the default, means that the bar is not drawn. A value earlier than
    /// <see cref="StartDate" /> is swapped with it.
    /// </summary>
    public DateTime? EndDate
    {
        get => _endDate;
        set => SetAndRaise(EndDateProperty, ref _endDate, value);
    }

    private object? _label;

    /// <summary>
    /// The text drawn inside the bar. Any object is accepted; its <c>ToString()</c> is shown.
    /// <see langword="null" />, the default, or an empty text draws no text.
    /// When the bar spans several rows of the date grid, only the segment that contains <see cref="StartDate" />
    /// shows it.
    /// </summary>
    public object? Label
    {
        get => _label;
        set => SetAndRaise(LabelProperty, ref _label, value);
    }

    private IBrush? _background;

    /// <summary>
    /// The fill of the bar.
    /// <see langword="null" />, the default, fills it transparently, so that only the label is visible.
    /// </summary>
    public IBrush? Background
    {
        get => _background;
        set => SetAndRaise(BackgroundProperty, ref _background, value);
    }

    private double _height = double.NaN;

    /// <summary>
    /// The height of the bar in pixels.
    /// <see cref="double.NaN" />, the default, a value of zero or less, or an infinite value makes the calendar use
    /// its <c>RangeBarHeight</c> instead; the bar is not drawn when that is also unusable.
    /// </summary>
    public double Height
    {
        get => _height;
        set => SetAndRaise(HeightProperty, ref _height, value);
    }

    #endregion
}

/// <summary>
/// A mutable collection of <see cref="CalendarRangeBar" /> instances, the type of <see cref="Calendar.RangeBars" />.
/// </summary>
public class CalendarRangeBarCollection : AvaloniaList<CalendarRangeBar>
{
}
