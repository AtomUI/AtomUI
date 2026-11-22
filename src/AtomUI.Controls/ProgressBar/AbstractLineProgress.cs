using System.Globalization;
using AtomUI.Icons.AntDesign;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

internal enum LineSizeTokenMode
{
    None,
    FontPresets,
    FontAndStrokePresets,
    FontAndStepPresets
}

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

    internal static readonly StyledProperty<LineSizeTokenMode> LineSizeTokenResourceModeProperty =
        AvaloniaProperty.Register<AbstractLineProgress, LineSizeTokenMode>(nameof(LineSizeTokenResourceMode));

    internal static readonly StyledProperty<IBrush?> PercentageLabelColorProperty =
        AvaloniaProperty.Register<AbstractLineProgress, IBrush?>(nameof(PercentageLabelColor));

    /// <summary>
    /// 填充比例（0–1），随 <see cref="AbstractProgressBar.Percentage"/> 写入。只影响绘制与填充部件的排布，
    /// 不使测量失效，主题可以直接对它设置过渡。
    /// </summary>
    internal static readonly StyledProperty<double> IndicatorRatioProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(nameof(IndicatorRatio));
    // Text-geometry minimum computed at runtime. The control theme projects it to MinWidth / MinHeight with a theme
    // Setter, so local values and application Styles keep their native precedence over this fallback.
    internal static readonly StyledProperty<double> FallbackMinWidthProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(nameof(FallbackMinWidth));

    internal static readonly StyledProperty<double> FallbackMinHeightProperty =
        AvaloniaProperty.Register<AbstractLineProgress, double>(nameof(FallbackMinHeight));

    internal double LineProgressPadding
    {
        get => _resourcePadding is { } padding && !IsSet(LineProgressPaddingProperty)
            ? padding
            : GetValue(LineProgressPaddingProperty);
        set => SetValue(LineProgressPaddingProperty, value);
    }

    internal double LineExtraInfoMargin
    {
        get => _resourceMargin is { } margin && !IsSet(LineExtraInfoMarginProperty)
            ? margin
            : GetValue(LineExtraInfoMarginProperty);
        set => SetValue(LineExtraInfoMarginProperty, value);
    }

    internal double LineInfoIconSize
    {
        get => GetValue(LineInfoIconSizeProperty);
        set => SetValue(LineInfoIconSizeProperty, value);
    }

    internal LineSizeTokenMode LineSizeTokenResourceMode
    {
        get => GetValue(LineSizeTokenResourceModeProperty);
        set => SetValue(LineSizeTokenResourceModeProperty, value);
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

    #region 令牌查询契约

    private protected const int LineFontPresetCount = 2;

    private OwnerTokenSet? _lineSizeTokens;
    private LineSizeTokenMode _activeLineSizeMode;
    private (double Normal, double Small) _fontPresets;
    private double? _resourcePadding;
    private double? _resourceMargin;

    private protected virtual OwnerTokenSet? CreateLineSizeTokenSet() => null;

    private protected OwnerTokenSet CreateLineSizeTokenSetFromKeys(ReadOnlySpan<object> ownKeys)
    {
        // Layout compares both font presets. The theme continues to set FontSize for the active visual state.
        var tokens = new OwnerTokenSet(this, LineFontPresetCount + ownKeys.Length, RefreshLineSizePresets);
        tokens.SetKind(0, SharedTokenKind.FontSize);
        tokens.SetKind(1, SharedTokenKind.FontSizeSM);
        for (var i = 0; i < ownKeys.Length; i++)
        {
            tokens.SetResourceKey(LineFontPresetCount + i, ownKeys[i]);
        }
        return tokens;
    }

    private protected double ReadLineSizeToken(int index, double fallback)
    {
        var value = _lineSizeTokens?.GetValue(index);
        return value is null || ReferenceEquals(value, AvaloniaProperty.UnsetValue) ? fallback : (double)value;
    }

    private protected Color? ReadLineColorToken(int index) => _lineSizeTokens?.GetValue(index) switch
    {
        Color color => color,
        ISolidColorBrush brush => brush.Color,
        _ => null
    };

    private protected bool SetResourceLineSpacing(double? padding, double? margin)
    {
        var previous = (LineProgressPadding, LineExtraInfoMargin);
        _resourcePadding = padding;
        _resourceMargin = margin;
        return previous != (LineProgressPadding, LineExtraInfoMargin);
    }

    private void RefreshLineSizePresets()
    {
        var next = (ReadFontPreset(0, _fontPresets.Normal), ReadFontPreset(1, _fontPresets.Small));
        var fontPresetsChanged = next != _fontPresets;
        _fontPresets = next;
        if (fontPresetsChanged)
        {
            _sizeTypeThresholdValuesInitialized = false;
        }
        OnLineSizeTokensChanged(fontPresetsChanged);
    }

    private double ReadFontPreset(int index, double previous)
    {
        var value = _lineSizeTokens?.GetValue(index);
        if (value is double size)
        {
            return size;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        // The original compiled font bindings convert local numeric resources and default if conversion fails.
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    private protected virtual void OnLineSizeTokensChanged(bool fontPresetsChanged)
    {
        if (fontPresetsChanged && this.IsAttachedToVisualTree())
        {
            NotifyLineMetricsChanged();
        }
    }

    private protected double QueryPresetFontSize(bool small)
    {
        var preset = small ? _fontPresets.Small : _fontPresets.Normal;
        return preset > 0 ? preset : FontSize;
    }

    /// <summary>线厚度三档查询（Large / Middle / Small）；无主题宿主回退 antd 8/6/4。</summary>
    private protected virtual double QueryLineStrokeThickness(SizeType sizeType) => sizeType switch
    {
        SizeType.Large => LARGE_STROKE_THICKNESS,
        SizeType.Small => SMALL_STROKE_THICKNESS,
        _              => MIDDLE_STROKE_THICKNESS,
    };

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
                var iconSize = LineInfoIconSize;
                return new Size(iconSize, iconSize);
            }

            return TextUtils.CalculateTextSize(string.Format(ProgressTextFormat, Percentage), fontSize, FontFamily);
        }
        return default;
    }
    
    /// <summary>
    /// The height the built-in info takes across a horizontal bar: the taller of the percentage text and the status
    /// icon, so the bar keeps one height whether it shows a value or an icon (antd gives both one line box) and stacked
    /// bars stay evenly spaced. Custom info content keeps its own measured size.
    /// </summary>
    private protected double CalculateInfoLineHeight(Size extraInfoSize, double fontSize, bool isInnerLabel)
    {
        // An inner label sits inside the bar, which keeps its own geometry.
        if (!IsProgressInfoPresented || ProgressInfoContent is not null || isInnerLabel)
        {
            return extraInfoSize.Height;
        }

        var textHeight = TextUtils.CalculateTextSize(string.Format(ProgressTextFormat, 100d), fontSize, FontFamily).Height;
        return Math.Max(extraInfoSize.Height, Math.Max(textHeight, LineInfoIconSize));
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

        if (change.Property == LineSizeTokenResourceModeProperty && ((ILogical)this).IsAttachedToLogicalTree)
        {
            UpdateLineSizeTokenSubscription();
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
                     change.Property == FontSizeProperty ||
                     (change.Property == StatusProperty && IsProgressInfoPresented &&
                      (Status == ProgressStatus.Exception || change.GetOldValue<ProgressStatus>() == ProgressStatus.Exception)))
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

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        UpdateLineSizeTokenSubscription();
    }

    private void UpdateLineSizeTokenSubscription()
    {
        // A replacement ControlTheme may omit preset styling altogether.
        var mode = LineSizeTokenResourceMode;
        if (_activeLineSizeMode == mode)
        {
            return;
        }

        _lineSizeTokens?.Dispose();
        _lineSizeTokens = null;
        _activeLineSizeMode = mode;
        try
        {
            if (mode != LineSizeTokenMode.None)
            {
                _lineSizeTokens = CreateLineSizeTokenSet();
            }
            RefreshLineSizePresets();
        }
        catch
        {
            _lineSizeTokens?.Dispose();
            _lineSizeTokens = null;
            _activeLineSizeMode = LineSizeTokenMode.None;
            throw;
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _lineSizeTokens?.Dispose();
        _lineSizeTokens = null;
        _activeLineSizeMode = LineSizeTokenMode.None;
        _fontPresets = default;
        _resourcePadding = null;
        _resourceMargin = null;
        _sizeTypeThresholdValuesInitialized = false;
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (HasTemplateProgressVisuals)
        {
            // Reused template parts may have received new scope resources before visual attachment.
            NotifyLineMetricsChanged();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UpdateEffectiveSizeTypeFromExplicitDimension();
        _extraInfoSize = CalculateExtraInfoSize(FontSize);
        NotifyOrientationChanged();
        
        ExceptionCompletedIconPresenter = e.NameScope.Find<IconPresenter>(ExceptionCompletedIconPresenterPart);
        SuccessCompletedIconPresenter   = e.NameScope.Find<IconPresenter>(SuccessCompletedIconPresenterPart);
        SetupDefaultCompletedIcons(static () => new CloseCircleFilled(), static () => new CheckCircleFilled());
    }

    private protected void UpdateEffectiveSizeTypeFromExplicitDimension()
    {
        var size = Orientation == Orientation.Horizontal ? Height : Width;
        if (!double.IsNaN(size))
        {
            EffectiveSizeType = CalculateEffectiveSizeType(size);
        }
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
