using Avalonia;

namespace AtomUI.Desktop.Controls;

public class Statistic : AbstractStatistic
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<AbstractStatistic, object?>(nameof(Value));
    
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public Func<Statistic, object?, string?>? Formatter { get; set; }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Statistic, string?> EffectiveValueProperty =
        AvaloniaProperty.RegisterDirect<Statistic, string?>(nameof(EffectiveValue),
            o => o.EffectiveValue,
            (o, v) => o.EffectiveValue = v);
    
    private string? _effectiveValue;

    internal string? EffectiveValue
    {
        get => _effectiveValue;
        set => SetAndRaise(EffectiveValueProperty, ref _effectiveValue, value);
    }

    #endregion

    static Statistic()
    {
        ContentProperty.Changed.AddClassHandler<Statistic>((x, e) => x.HandleContentChanged(e));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty ||
            change.Property == DecimalSeparatorProperty ||
            change.Property == GroupSeparatorProperty ||
            change.Property == PrecisionProperty)
        {
            GenerateEffectiveValue();
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Content == null)
        {
            SetCurrentValue(ContentProperty, EffectiveValue);
        }
    }

    private void GenerateEffectiveValue()
    {
        if (Formatter != null)
        {
            SetCurrentValue(EffectiveValueProperty, Formatter(this, Value));
        }
        else if (Value != null)
        {
            var effectiveValue = StatisticUtils.FormatNumber(Value, GroupSeparator, DecimalSeparator, Precision);
            SetCurrentValue(EffectiveValueProperty, effectiveValue);
        }
    }
    
    private void HandleContentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is StatisticCountUp countUp)
        {
            countUp.DataContext = this;
        }
    }
}