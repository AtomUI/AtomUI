using Avalonia.Data;
using Avalonia.VisualTree;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Displays a number with an optional title, prefix and suffix, formatted as a number with a fixed number of
/// decimal digits or by a formatter of your own.
/// </summary>
public partial class Statistic : AbstractStatistic
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<Statistic, object?>(nameof(Value));
    
    /// <summary>
    /// The number the statistic displays. With no <see cref="Formatter" /> it is formatted with
    /// <see cref="AbstractStatistic.Precision" />, <see cref="AbstractStatistic.GroupSeparator" /> and
    /// <see cref="AbstractStatistic.DecimalSeparator" />; when it is <see langword="null" /> and no formatter is
    /// set, the value area is empty. The formatted text becomes <c>Content</c> while <c>Content</c> is unset or
    /// was produced by the control, so a <see cref="StatisticCountUp" /> set as <c>Content</c> is shown instead.
    /// </summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    /// <summary>
    /// Converts <see cref="Value" /> into the text to display instead of the number formatting. It receives the
    /// statistic and the current value, also when the value is <see langword="null" />; a <see langword="null" />
    /// result leaves the value area empty. The text is regenerated only when <see cref="Value" /> or one of
    /// <see cref="AbstractStatistic.Precision" />, <see cref="AbstractStatistic.GroupSeparator" /> or
    /// <see cref="AbstractStatistic.DecimalSeparator" /> changes, so set the formatter before them.
    /// </summary>
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

    /// <summary>
    /// Calls the base member and then regenerates the displayed text when <see cref="Value" />,
    /// <see cref="AbstractStatistic.Precision" />, <see cref="AbstractStatistic.GroupSeparator" /> or
    /// <see cref="AbstractStatistic.DecimalSeparator" /> changes. An override must call the base member.
    /// </summary>
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

    /// <summary>
    /// Calls the base member and then writes the formatted text to <c>Content</c> when no Content was supplied.
    /// An override must call the base member.
    /// </summary>
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
    
    /// <summary>
    /// Calls the base member and then binds the font size of a <see cref="StatisticCountUp" /> content to
    /// <c>ContentFontSize</c> at style priority, so a local font size set on the count-up still wins. An
    /// override must call the base member.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureCountUpTypography();
    }

    /// <summary>
    /// Disposes the font size binding of a <see cref="StatisticCountUp" /> content and then calls the base
    /// member. An override must call the base member.
    /// </summary>
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
