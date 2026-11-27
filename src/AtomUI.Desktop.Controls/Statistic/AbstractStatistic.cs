using AtomUI.Controls;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Base class of the statistics that show a header above a value row made of an optional prefix, the value and
/// an optional suffix. It declares the properties that the statistics share; each derived statistic brings its
/// own template.
/// </summary>
public abstract class AbstractStatistic : HeaderedContentControl
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="DecimalSeparator" /> property.
    /// </summary>
    public static readonly StyledProperty<string> DecimalSeparatorProperty =
        AvaloniaProperty.Register<AbstractStatistic, string>(nameof(DecimalSeparator), ".");
    
    /// <summary>
    /// Identifies the <see cref="GroupSeparator" /> property.
    /// </summary>
    public static readonly StyledProperty<string> GroupSeparatorProperty =
        AvaloniaProperty.Register<AbstractStatistic, string>(nameof(GroupSeparator), ",");
    
    /// <summary>
    /// Identifies the <see cref="Precision" /> property.
    /// </summary>
    public static readonly StyledProperty<int> PrecisionProperty =
        AvaloniaProperty.Register<AbstractStatistic, int>(nameof(Precision), 0);
    
    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<AbstractStatistic, bool>(nameof(IsLoading));
    
    /// <summary>
    /// Identifies the <see cref="ValuePrefixAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ValuePrefixAddOnProperty =
        AvaloniaProperty.Register<AbstractStatistic, object?>(nameof(ValuePrefixAddOn));
    
    /// <summary>
    /// Identifies the <see cref="ValuePrefixAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ValuePrefixAddOnTemplateProperty =
        AvaloniaProperty.Register<AbstractStatistic, IDataTemplate?>(nameof(ValuePrefixAddOnTemplate));

    /// <summary>
    /// Identifies the <see cref="ValueSuffixAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ValueSuffixAddOnProperty =
        AvaloniaProperty.Register<AbstractStatistic, object?>(nameof(ValueSuffixAddOn));
    
    /// <summary>
    /// Identifies the <see cref="ValueSuffixAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ValueSuffixAddOnTemplateProperty =
        AvaloniaProperty.Register<AbstractStatistic, IDataTemplate?>(nameof(ValueSuffixAddOnTemplate));
    
    /// <summary>
    /// Identifies the <see cref="ContentForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ContentForegroundProperty =
        AvaloniaProperty.Register<AbstractStatistic, IBrush?>(nameof(ContentForeground));
    
    /// <summary>
    /// Identifies the <see cref="ContentFontSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ContentFontSizeProperty =
        AvaloniaProperty.Register<AbstractStatistic, double>(nameof(ContentFontSize));

    /// <summary>
    /// Identifies the <see cref="StrokeDashArray" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> StrokeDashArrayProperty =
        AvaloniaProperty.Register<AbstractStatistic, IReadOnlyList<double>?>(nameof(StrokeDashArray));
    
    /// <summary>
    /// The text between the integer and the fractional digits of a value that <see cref="Statistic" /> formats with
    /// its built-in number formatting. It is not used while <see cref="Statistic.Formatter" /> is set. The default
    /// is <c>.</c>.
    /// </summary>
    public string DecimalSeparator
    {
        get => GetValue(DecimalSeparatorProperty);
        set => SetValue(DecimalSeparatorProperty, value);
    }
    
    /// <summary>
    /// The text between the digit groups of the integer part of a value that <see cref="Statistic" /> formats with
    /// its built-in number formatting. It is not used while <see cref="Statistic.Formatter" /> is set. The default
    /// is <c>,</c>.
    /// </summary>
    public string GroupSeparator
    {
        get => GetValue(GroupSeparatorProperty);
        set => SetValue(GroupSeparatorProperty, value);
    }
    
    /// <summary>
    /// The number of fractional digits in a value that <see cref="Statistic" /> formats with its built-in number
    /// formatting. It is not used while <see cref="Statistic.Formatter" /> is set. The default is 0.
    /// </summary>
    public int Precision
    {
        get => GetValue(PrecisionProperty);
        set => SetValue(PrecisionProperty, value);
    }
    
    /// <summary>
    /// Shows a skeleton in place of the value row while <see langword="true" />; the header is not affected. The
    /// default is <see langword="false" />.
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    
    /// <summary>
    /// The content shown before the value in the value row, for example a currency symbol or an <see cref="Icon" />.
    /// It is visible only while it is not <see langword="null" />. A control set here becomes a logical child of the
    /// statistic.
    /// </summary>
    [DependsOn(nameof(ValuePrefixAddOnTemplate))]
    public object? ValuePrefixAddOn
    {
        get => GetValue(ValuePrefixAddOnProperty);
        set => SetValue(ValuePrefixAddOnProperty, value);
    }
    
    /// <summary>
    /// The template that builds the prefix from <see cref="ValuePrefixAddOn" /> when the content is not a control.
    /// When it is <see langword="null" />, the content is presented without a template.
    /// </summary>
    public IDataTemplate? ValuePrefixAddOnTemplate
    {
        get => GetValue(ValuePrefixAddOnTemplateProperty);
        set => SetValue(ValuePrefixAddOnTemplateProperty, value);
    }

    /// <summary>
    /// The content shown after the value in the value row, for example a unit. It is visible only while it is not
    /// <see langword="null" />. A control set here becomes a logical child of the statistic.
    /// </summary>
    [DependsOn(nameof(ValueSuffixAddOnTemplate))]
    public object? ValueSuffixAddOn
    {
        get => GetValue(ValueSuffixAddOnProperty);
        set => SetValue(ValueSuffixAddOnProperty, value);
    }
    
    /// <summary>
    /// The template that builds the suffix from <see cref="ValueSuffixAddOn" /> when the content is not a control.
    /// When it is <see langword="null" />, the content is presented without a template.
    /// </summary>
    public IDataTemplate? ValueSuffixAddOnTemplate
    {
        get => GetValue(ValueSuffixAddOnTemplateProperty);
        set => SetValue(ValueSuffixAddOnTemplateProperty, value);
    }
    
    /// <summary>
    /// The foreground of the value row: the value and the prefix and suffix add-ons. It does not affect the header.
    /// When unset, the theme uses the global color token <c>ColorTextHeading</c>.
    /// </summary>
    public IBrush? ContentForeground
    {
        get => GetValue(ContentForegroundProperty);
        set => SetValue(ContentForegroundProperty, value);
    }

    /// <summary>
    /// The font size of the value row: the value and the prefix and suffix add-ons, including the size of an icon
    /// placed there. It does not affect the header. When unset, the theme of the concrete statistic supplies its own
    /// content font size token.
    /// </summary>
    public double ContentFontSize
    {
        get => GetValue(ContentFontSizeProperty);
        set => SetValue(ContentFontSizeProperty, value);
    }

    /// <summary>
    /// The dash pattern of the frame border. It takes effect in the themes that draw the frame as a dashed border,
    /// such as <see cref="Statistic" />; <see cref="TimerStatistic" /> draws a plain border and ignores it.
    /// </summary>
    public IReadOnlyList<double>? StrokeDashArray
    {
        get => GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    #endregion

    static AbstractStatistic()
    {
        HeaderProperty.Changed.AddClassHandler<AbstractStatistic>((x, e) => x.HandleChildChanged(e));
        ValuePrefixAddOnProperty.Changed.AddClassHandler<AbstractStatistic>((x, e) => x.HandleChildChanged(e));
        ValueSuffixAddOnProperty.Changed.AddClassHandler<AbstractStatistic>((x, e) => x.HandleChildChanged(e));
        AffectsMeasure<AbstractStatistic>(DecimalSeparatorProperty, GroupSeparatorProperty, PrecisionProperty);
    }
    
    public AbstractStatistic()
    {
    }
    
    private void HandleChildChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is ILogical oldChild)
        {
            LogicalChildren.Remove(oldChild);
            if (oldChild is Icon icon)
            {
                icon.SetTemplatedParent(null);
            }
        }

        if (e.NewValue is ILogical newChild)
        {
            LogicalChildren.Add(newChild);
            if (newChild is Icon icon)
            {
                icon.SetTemplatedParent(this);
            }
        }
    }
}
