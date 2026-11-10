using Avalonia.Data;
using Avalonia.VisualTree;
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

    private IDisposable? _countUpTypographyBinding;

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
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureCountUpTypography();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _countUpTypographyBinding?.Dispose();
        _countUpTypographyBinding = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void ConfigureCountUpTypography()
    {
        _countUpTypographyBinding?.Dispose();
        _countUpTypographyBinding = null;
        if (this.IsAttachedToVisualTree() && Content is StatisticCountUp countUp)
        {
            // This runtime Content object is not a template child: ControlTheme forbids direct child selectors, and
            // /template/ cannot reach it. The host's explicit ContentFontSize projects at Style priority so a caller's
            // local CountUp.FontSize still wins. Release on content replacement or visual detach.
            _countUpTypographyBinding = countUp.Bind(FontSizeProperty, this.GetObservable(ContentFontSizeProperty), BindingPriority.Style);
        }
    }

    private void HandleContentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        ConfigureCountUpTypography();
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
