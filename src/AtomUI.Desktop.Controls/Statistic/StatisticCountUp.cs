using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A number that counts up to <see cref="EndValue" />: when the control is loaded it animates from the
/// current <see cref="AnimatingValue" />, which starts at 0, and it animates again whenever
/// <see cref="EndValue" /> changes while the control is attached to a visual tree.
/// </summary>
public class StatisticCountUp : TemplatedControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="EndValue" /> property.
    /// </summary>
    public static readonly StyledProperty<double> EndValueProperty =
        AvaloniaProperty.Register<StatisticCountUp, double>(nameof(EndValue));
    
    /// <summary>
    /// Identifies the <see cref="DecimalSeparator" /> property.
    /// </summary>
    public static readonly StyledProperty<string> DecimalSeparatorProperty =
        AvaloniaProperty.Register<StatisticCountUp, string>(nameof(DecimalSeparator), ".");
    
    /// <summary>
    /// Identifies the <see cref="GroupSeparator" /> property.
    /// </summary>
    public static readonly StyledProperty<string> GroupSeparatorProperty =
        AvaloniaProperty.Register<StatisticCountUp, string>(nameof(GroupSeparator), ",");
    
    /// <summary>
    /// Identifies the <see cref="Precision" /> property.
    /// </summary>
    public static readonly StyledProperty<int> PrecisionProperty =
        AvaloniaProperty.Register<StatisticCountUp, int>(nameof(Precision), 0);
    
    /// <summary>
    /// The number the count-up ends at. Changing it while the control is attached to a visual tree starts a new
    /// animation from the value that is shown at that moment.
    /// </summary>
    public double EndValue
    {
        get => GetValue(EndValueProperty);
        set => SetValue(EndValueProperty, value);
    }

    /// <summary>
    /// The text between the integer part and the decimal digits of the number. The default is a period.
    /// </summary>
    public string DecimalSeparator
    {
        get => GetValue(DecimalSeparatorProperty);
        set => SetValue(DecimalSeparatorProperty, value);
    }
    
    /// <summary>
    /// The text between the groups of integer digits of the number. The default is a comma.
    /// </summary>
    public string GroupSeparator
    {
        get => GetValue(GroupSeparatorProperty);
        set => SetValue(GroupSeparatorProperty, value);
    }
    
    /// <summary>
    /// The number of decimal digits the number shows. The default is 0.
    /// </summary>
    public int Precision
    {
        get => GetValue(PrecisionProperty);
        set => SetValue(PrecisionProperty, value);
    }

    #endregion

    #region 内部属性定义
    
    /// <summary>
    /// Identifies the <see cref="AnimatingValue" /> property.
    /// </summary>
    public static readonly StyledProperty<double> AnimatingValueProperty =
        AvaloniaProperty.Register<StatisticCountUp, double>(nameof(AnimatingValue), 0);
    
    internal static readonly DirectProperty<StatisticCountUp, string?> FormattedValueProperty =
        AvaloniaProperty.RegisterDirect<StatisticCountUp, string?>(nameof(FormattedValue),
            o => o.FormattedValue,
            (o, v) => o.FormattedValue = v);
    
    /// <summary>
    /// The value that is shown while the count-up runs. It starts at 0, the theme animates it to
    /// <see cref="EndValue" />, and the displayed text follows every change of it.
    /// </summary>
    public double AnimatingValue
    {
        get => GetValue(AnimatingValueProperty);
        set => SetValue(AnimatingValueProperty, value);
    }
    
    private string? _formattedValue;

    internal string? FormattedValue
    {
        get => _formattedValue;
        set => SetAndRaise(FormattedValueProperty, ref _formattedValue, value);
    }

    #endregion

    static StatisticCountUp()
    {
        AffectsMeasure<StatisticCountUp>(FormattedValueProperty);
    }

    public StatisticCountUp()
    {
    }

    /// <summary>
    /// Calls the base member and then, while the control is attached to a visual tree, animates towards a
    /// changed <see cref="EndValue" />. It also regenerates the displayed text whenever <see cref="AnimatingValue" />,
    /// <see cref="GroupSeparator" />, <see cref="Precision" /> or <see cref="DecimalSeparator" /> changes. An
    /// override must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == EndValueProperty)
            {
                SetCurrentValue(AnimatingValueProperty, EndValue);
            }
        }
        
        if (change.Property == AnimatingValueProperty ||
            change.Property == GroupSeparatorProperty ||
            change.Property == PrecisionProperty ||
            change.Property == DecimalSeparatorProperty)
        {
            var effectiveValue = StatisticUtils.FormatNumber(AnimatingValue, GroupSeparator, DecimalSeparator, Precision);
            SetCurrentValue(FormattedValueProperty, effectiveValue);
        }
    }

    /// <summary>
    /// Calls the base member and then starts the count-up from the current <see cref="AnimatingValue" /> to
    /// <see cref="EndValue" />. An override must call the base member.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        AnimatingValue = EndValue;
    }
}