using Avalonia;

namespace AtomUI.Desktop.Controls;

public partial class Statistic : AbstractStatistic
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<Statistic, object?>(nameof(Value));
    
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
    private object? _generatedContent;
    private bool _isUsingGeneratedContent;
    private bool _isInitialized;

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
        _isInitialized = true;
        if (Content == null)
        {
            SetGeneratedContent(EffectiveValue);
        }
    }

    private void GenerateEffectiveValue()
    {
        string? effectiveValue = null;
        if (Formatter != null)
        {
            effectiveValue = Formatter(this, Value);
        }
        else if (Value != null)
        {
            effectiveValue = StatisticUtils.FormatNumber(Value, GroupSeparator, DecimalSeparator, Precision);
        }

        SetCurrentValue(EffectiveValueProperty, effectiveValue);
        if (_isInitialized && (Content == null || _isUsingGeneratedContent))
        {
            SetGeneratedContent(effectiveValue);
        }
    }

    private void SetGeneratedContent(object? content)
    {
        _generatedContent         = content;
        _isUsingGeneratedContent = true;
        SetCurrentValue(ContentProperty, content);
    }
    
    private void HandleContentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is StatisticCountUp oldCountUp &&
            ReferenceEquals(oldCountUp.DataContext, this))
        {
            oldCountUp.DataContext = null;
        }

        if (e.NewValue is StatisticCountUp countUp)
        {
            countUp.DataContext = this;
        }

        if (!ReferenceEquals(e.NewValue, _generatedContent))
        {
            _isUsingGeneratedContent = false;
        }
    }
}
