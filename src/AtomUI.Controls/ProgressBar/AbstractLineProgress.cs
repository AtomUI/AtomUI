using AtomUI.Icons.AntDesign;
using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

internal readonly struct SizeTypeThresholdValue
{
    internal SizeTypeThresholdValue(double normalStateValue, double innerStateValue = 0)
    {
        NormalStateValue = normalStateValue;
        InnerStateValue  = innerStateValue;
    }

    internal double NormalStateValue { get; }
    internal double InnerStateValue { get; }
}

[PseudoClasses(ProgressBarPseudoClass.Vertical, ProgressBarPseudoClass.Horizontal)]
public abstract class AbstractLineProgress : AbstractProgressBar
{
    #region 公共属性定义
    /// <summary>
    /// Defines the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<AbstractLineProgress, Orientation>(nameof(Orientation));

    /// <summary>
    /// Gets or sets the orientation of the <see cref="ProgressBar" />.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<double> LineProgressPaddingProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(
            nameof(LineProgressPadding));

    internal static readonly StyledProperty<double> LineExtraInfoMarginProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(
            nameof(LineExtraInfoMargin));

    internal static readonly StyledProperty<double> LineInfoIconSizeProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(
            nameof(LineInfoIconSize));
    
    internal static readonly StyledProperty<IBrush?> PercentageLabelColorProperty =
        AvaloniaProperty.Register<AbstractLineProgress, IBrush?>(nameof(PercentageLabelColor));

    /// <summary>
    /// 填充比例（0–1），随 <see cref="AbstractProgressBar.Percentage"/> 写入。只影响绘制与填充部件的排布，
    /// 不使测量失效，主题可以直接对它设置过渡。
    /// </summary>
    internal static readonly StyledProperty<double> IndicatorRatioProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(nameof(IndicatorRatio));
    // Status icon sizes of the large / small presets, filled by the control theme from the control's own Tokens.
    internal static readonly StyledProperty<double> PresetLineInfoIconSizeProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(nameof(PresetLineInfoIconSize));

    internal static readonly StyledProperty<double> PresetLineInfoIconSizeSMProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(nameof(PresetLineInfoIconSizeSM));

    // Font sizes of the large / small presets, used to compute the size-type thresholds of an explicit bar thickness.
    internal static readonly StyledProperty<double> PresetFontSizeProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(nameof(PresetFontSize));

    internal static readonly StyledProperty<double> PresetFontSizeSMProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(nameof(PresetFontSizeSM));

    // Text-geometry minimum computed at runtime. The control theme projects it to MinWidth / MinHeight with a theme
    // Setter, so local values and application Styles keep their native precedence over this fallback.
    internal static readonly StyledProperty<double> FallbackMinWidthProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(nameof(FallbackMinWidth));

    internal static readonly StyledProperty<double> FallbackMinHeightProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(nameof(FallbackMinHeight));

    internal double LineProgressPadding
    {
        get => GetValue(LineProgressPaddingProperty);
        set => SetValue(LineProgressPaddingProperty, value);
    }

    internal double LineExtraInfoMargin
    {
        get => GetValue(LineExtraInfoMarginProperty);
        set => SetValue(LineExtraInfoMarginProperty, value);
    }

    internal double LineInfoIconSize
    {
        get => GetValue(LineInfoIconSizeProperty);
        set => SetValue(LineInfoIconSizeProperty, value);
    }
    
    internal IBrush? PercentageLabelColor
    {
        get => GetValue(PercentageLabelColorProperty);
        set => SetValue(PercentageLabelColorProperty, value);
    }

    internal double IndicatorRatio
    {
        get => GetValue(IndicatorRatioProperty);
        set => SetValue(IndicatorRatioProperty, value);
    }

    internal double PresetLineInfoIconSize
    {
        get => GetValue(PresetLineInfoIconSizeProperty);
        set => SetValue(PresetLineInfoIconSizeProperty, value);
    }

    internal double PresetLineInfoIconSizeSM
    {
        get => GetValue(PresetLineInfoIconSizeSMProperty);
        set => SetValue(PresetLineInfoIconSizeSMProperty, value);
    }

    internal double PresetFontSize
    {
        get => GetValue(PresetFontSizeProperty);
        set => SetValue(PresetFontSizeProperty, value);
    }

    internal double PresetFontSizeSM
    {
        get => GetValue(PresetFontSizeSMProperty);
        set => SetValue(PresetFontSizeSMProperty, value);
    }

    internal double FallbackMinWidth
    {
        get => GetValue(FallbackMinWidthProperty);
        set => SetValue(FallbackMinWidthProperty, value);
    }

    internal double FallbackMinHeight
    {
        get => GetValue(FallbackMinHeightProperty);
        set => SetValue(FallbackMinHeightProperty, value);
    }

    #endregion

    private protected SizeTypeThresholdValue _largeSizeTypeThresholdValue;
    private protected SizeTypeThresholdValue _middleSizeTypeThresholdValue;
    private protected SizeTypeThresholdValue _smallSizeTypeThresholdValue;
    private protected bool _sizeTypeThresholdValuesInitialized;
    protected Size _extraInfoSize = Size.Infinity;
    protected Rect _grooveRect;

    static AbstractLineProgress()
    {
        AffectsRender<AbstractLineProgress>(IndicatorRatioProperty);
        AffectsMeasure<AbstractLineProgress>(LineProgressPaddingProperty,
            LineExtraInfoMarginProperty,
            LineInfoIconSizeProperty);
    }

    // 根据当前的状态进行计算
    protected virtual Size CalculateExtraInfoSize(double fontSize)
    {
        if (IsProgressInfoPresented)
        {
            if (MeasureProgressInfoContent() is { } contentSize)
            {
                return contentSize;
            }

            if (Status == ProgressStatus.Exception || MathUtils.AreClose(Percentage, 100))
            {
                // 只要图标
                return new Size(LineInfoIconSize, LineInfoIconSize);
            }

            return TextUtils.CalculateTextSize(string.Format(ProgressTextFormat, Percentage), fontSize, FontFamily);
        }
        return default;
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ProgressBarPseudoClass.Vertical, Orientation == Orientation.Vertical);
        PseudoClasses.Set(ProgressBarPseudoClass.Horizontal, Orientation == Orientation.Horizontal);
    }

    protected override void NotifyPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.NotifyPropertyChanged(change);

        if (change.Property == PercentageProperty)
        {
            IndicatorRatio = Math.Clamp(Percentage / 100, 0, 1);
        }

        if (this.IsAttachedToVisualTree())
        {
            if ((change.Property == WidthProperty && Orientation == Orientation.Vertical) ||
                (change.Property == HeightProperty && Orientation == Orientation.Horizontal))
            {
                EffectiveSizeType = CalculateEffectiveSizeType(change.GetNewValue<double>());
                CalculateStrokeThickness();
            }
            else if (change.Property == EffectiveSizeTypeProperty ||
                     change.Property == IsProgressInfoVisibleProperty ||
                     change.Property == ProgressInfoContentTemplateProperty)
            {
                _extraInfoSize = CalculateExtraInfoSize(FontSize);
            }
            else if (change.Property == OrientationProperty)
            {
                NotifyOrientationChanged();
            }
            else if (change.Property == LineInfoIconSizeProperty ||
                     change.Property == LineProgressPaddingProperty ||
                     change.Property == LineExtraInfoMarginProperty ||
                     change.Property == PresetFontSizeProperty ||
                     change.Property == PresetFontSizeSMProperty ||
                     change.Property == FontSizeProperty)
            {
                NotifyLineMetricsChanged();
            }
        }
    }

    /// <summary>
    /// Recomputes the text geometry after a Token-driven metric (icon size, padding, gap, font size) changed, so a theme
    /// change re-lays the bar out instead of keeping values computed when the template was applied.
    /// </summary>
    private protected virtual void NotifyLineMetricsChanged()
    {
        _extraInfoSize = CalculateExtraInfoSize(FontSize);
        InvalidateMeasure();
        InvalidateArrange();
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        var    calculateEffectiveSize = false;
        double sizeValue              = 0;
        if (Orientation == Orientation.Horizontal && !double.IsNaN(Height))
        {
            sizeValue              = Height;
            calculateEffectiveSize = true;
        }
        else if (Orientation == Orientation.Vertical && !double.IsNaN(Width))
        {
            sizeValue              = Width;
            calculateEffectiveSize = true;
        }

        if (calculateEffectiveSize)
        {
            EffectiveSizeType = CalculateEffectiveSizeType(sizeValue);
        }

        _extraInfoSize = CalculateExtraInfoSize(FontSize);
        NotifyOrientationChanged();
        
        ExceptionCompletedIconPresenter = e.NameScope.Find<IconPresenter>(ExceptionCompletedIconPresenterPart);
        SuccessCompletedIconPresenter   = e.NameScope.Find<IconPresenter>(SuccessCompletedIconPresenterPart);
        SetupDefaultCompletedIcons(static () => new CloseCircleFilled(), static () => new CheckCircleFilled());
    }

    protected override void NotifyProgressInfoContentChanged()
    {
        base.NotifyProgressInfoContentChanged();
        _extraInfoSize = CalculateExtraInfoSize(FontSize);
        InvalidateMeasure();
        InvalidateArrange();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Custom info content can change size after it was set; the info area follows its current desired size.
        if (ProgressInfoContent is not null)
        {
            RefreshProgressInfoContentSize();
        }

        return base.MeasureOverride(availableSize);
    }

    private protected virtual void RefreshProgressInfoContentSize()
    {
        _extraInfoSize = CalculateExtraInfoSize(FontSize);
    }

    protected virtual void NotifyOrientationChanged()
    {
        UpdatePseudoClasses();
    }

    private bool _lastCompletedStatus;

    protected override void NotifyHandleExtraInfoVisibility()
    {
        base.NotifyHandleExtraInfoVisibility();
        var currentStatus = false;
        if (MathUtils.AreClose(Value, Maximum))
        {
            currentStatus = true;
        }
        else
        {
            currentStatus = false;
        }

        if (currentStatus != _lastCompletedStatus)
        {
            _lastCompletedStatus = currentStatus;
            _extraInfoSize       = CalculateExtraInfoSize(FontSize);
        }
    }
}
