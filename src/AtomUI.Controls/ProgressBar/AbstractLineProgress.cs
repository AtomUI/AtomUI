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

/// <summary>
/// Base class of the line progress bars and the common root of
/// <see cref="AbstractGeneralProgressBar" /> and <see cref="AbstractGeneralStepsProgressBar" />. It owns the
/// orientation, the fill ratio, the size of the progress info area and the subscription to the size presets
/// that the control theme provides.
/// </summary>
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
    /// Gets or sets whether the bar runs horizontally or vertically; horizontal is the default. The value selects
    /// the <c>vertical</c> or <c>horizontal</c> pseudo class, and the explicit height of a horizontal bar or width
    /// of a vertical bar determines the size type.
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
    /// Fill ratio from 0 to 1, updated whenever <see cref="AbstractProgressBar.Percentage" /> changes. It only
    /// affects rendering and the arrangement of the fill part and does not invalidate measure, so a control theme
    /// can transition it directly.
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

    /// <summary>
    /// The side length of the status icon of the bar, in DIPs. The theme of the derived control sets it from its
    /// Token and projects it to the width of the status icon in the template. The control reserves a square of this
    /// size for the icon in the progress info area and takes it as the least height of that area.
    /// </summary>
    internal double LineInfoIconSize
    {
        get => GetValue(LineInfoIconSizeProperty);
        set => SetValue(LineInfoIconSizeProperty, value);
    }

    /// <summary>
    /// Selects which size presets the control reads from Tokens: the font size presets alone, together with the
    /// stroke presets, or together with the step presets; <see cref="LineSizeTokenMode.None" /> reads none, so
    /// every preset keeps its fallback value. The theme of the derived control sets it. The subscription starts
    /// when the control joins a logical tree and when the value changes there, and ends when it leaves the tree.
    /// </summary>
    internal LineSizeTokenMode LineSizeTokenResourceMode
    {
        get => GetValue(LineSizeTokenResourceModeProperty);
        set => SetValue(LineSizeTokenResourceModeProperty, value);
    }

    /// <summary>
    /// The brush of the percentage text. The theme binds the foreground of the text to it. This class does not set
    /// the value.
    /// </summary>
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

    /// <summary>
    /// A minimum width of the control computed from the size of its text at runtime; the derived control sets it.
    /// The theme projects it to <c>MinWidth</c> with a Setter, so a local <c>MinWidth</c> or an application Style
    /// takes precedence over this fallback.
    /// </summary>
    internal double FallbackMinWidth
    {
        get => GetValue(FallbackMinWidthProperty);
        set => SetValue(FallbackMinWidthProperty, value);
    }

    /// <summary>
    /// A minimum height of the control computed from the size of its text at runtime; the derived control sets it.
    /// The theme projects it to <c>MinHeight</c> with a Setter, so a local <c>MinHeight</c> or an application Style
    /// takes precedence over this fallback.
    /// </summary>
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

    /// <summary>
    /// Creates the Token set that holds the font size presets of the line followed by the own Tokens of the
    /// derived control. <see cref="UpdateLineSizeTokenSubscription" /> calls it when
    /// <see cref="LineSizeTokenResourceMode" /> changes to a mode other than <c>None</c>, and the control owns and
    /// disposes the returned set. The base member returns <see langword="null" />, which makes every preset keep
    /// its fallback value. An override returns the set built by <see cref="CreateLineSizeTokenSetFromKeys" />,
    /// whose first <see cref="LineFontPresetCount" /> slots hold the presets.
    /// </summary>
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

    /// <summary>
    /// Called when a value of the line-size Token set changes, after the font size presets were read again.
    /// <c>fontPresetsChanged</c> is <see langword="true" /> when the pair of preset font sizes changed; the base
    /// member then calls <c>NotifyLineMetricsChanged</c>, but only while the control is attached to a visual
    /// tree. An override that reads own Tokens from the set calls the base member with <see langword="true" />
    /// also when its own change affects the line metrics.
    /// </summary>
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

    /// <summary>
    /// Returns the built-in Ant Design line thickness of the bar for a size type, which is 8 for large, 6 for middle
    /// and 4 for small.
    /// </summary>
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
    /// <summary>
    /// Size of the progress info area of the bar: the measured custom info content, the status icon box, or the
    /// percentage text. It holds <see cref="Size.Infinity" /> until it is first calculated.
    /// </summary>
    protected Size _extraInfoSize = Size.Infinity;
    /// <summary>
    /// Bounds of the groove (track) of the bar, relative to the top-left corner of the control. It is empty until
    /// a derived control assigns it, and the derived controls read it to place the indicator.
    /// </summary>
    protected Rect _grooveRect;

    static AbstractLineProgress()
    {
        AffectsRender<AbstractLineProgress>(IndicatorRatioProperty);
        AffectsMeasure<AbstractLineProgress>(LineProgressPaddingProperty,
            LineExtraInfoMarginProperty,
            LineInfoIconSizeProperty);
    }

    /// <summary>
    /// Calculates the size of the progress info area from the current state: the desired size of the custom info
    /// content presenter when custom content is set and the template provides the presenter, a square of
    /// <see cref="LineInfoIconSize" /> when the status is exception or the bar has reached 100 percent,
    /// and otherwise the size of the percentage text. The result is empty when no progress info is presented.
    /// </summary>
    /// <param name="fontSize">
    /// Font size used to measure the percentage text.
    /// </param>
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
                // Only the status icon is shown in this state, not the percentage text.
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

    /// <summary>
    /// Extends the property change handling of the base class: mirrors
    /// <see cref="AbstractProgressBar.Percentage" /> into <see cref="IndicatorRatio" />, rebuilds the size
    /// Token subscription when <see cref="LineSizeTokenResourceMode" /> changes in a logical tree, and, once
    /// attached to a visual tree, updates the size type and stroke thickness, the info area size and the
    /// orientation state in response to the related properties. Derived overrides must call the base member.
    /// </summary>
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
    /// Recalculates the size of the info area and invalidates the measurement and the arrangement, so that a
    /// change of a Token-driven line metric re-lays the bar out instead of keeping values computed when the
    /// template was applied.
    /// </summary>
    /// <remarks>
    /// The base class calls it only while the control is attached to a visual tree: when the info icon size, the
    /// bar padding, the info margin or the font size changes, when the status switches to or from
    /// <see cref="ProgressStatus.Exception" /> while progress info is presented, when a change of the line-size
    /// Token set changed the preset font sizes, and on attachment to a visual tree whose template supplies the
    /// progress visuals. Derived overrides must call the base member.
    /// </remarks>
    private protected virtual void NotifyLineMetricsChanged()
    {
        _extraInfoSize = CalculateExtraInfoSize(FontSize);
        InvalidateMeasure();
        InvalidateArrange();
    }

    /// <summary>
    /// Starts the Token subscription that <see cref="LineSizeTokenResourceMode" /> selects; it exists only while
    /// the control is in a logical tree. Derived overrides must call the base member.
    /// </summary>
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

    /// <summary>
    /// Disposes the size Token subscription and clears the values read from it (font presets, bar padding and the
    /// gap to the info area) and the cached size type thresholds, so that they are resolved again in the tree the
    /// control is attached to next. Derived overrides must call the base member.
    /// </summary>
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

    /// <summary>
    /// Recalculates the info area size and invalidates the layout when the template supplies the progress visuals,
    /// because a reused template may have received new scope resources before the control was attached. Derived
    /// overrides must call the base member.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (HasTemplateProgressVisuals)
        {
            // Reused template parts may have received new scope resources before visual attachment.
            NotifyLineMetricsChanged();
        }
    }

    /// <summary>
    /// Locates the exception and success icon presenters of the applied template, sets the default completed icons
    /// when the icon properties are <see langword="null" />, and refreshes the size type, the info area size and
    /// the orientation state from the current property values. Derived overrides must call the base member.
    /// </summary>
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

    /// <summary>
    /// Recalculates the info area size and invalidates the layout after custom progress info content is set or
    /// cleared. Derived overrides must call the base member.
    /// </summary>
    protected override void NotifyProgressInfoContentChanged()
    {
        base.NotifyProgressInfoContentChanged();
        _extraInfoSize = CalculateExtraInfoSize(FontSize);
        InvalidateMeasure();
        InvalidateArrange();
    }

    /// <summary>
    /// Measures the control. When custom progress info content is set, the info area size is refreshed first, so
    /// that the layout uses the current size of that content.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        // Custom info content can change size after it was set; the info area follows its current desired size.
        if (ProgressInfoContent is not null)
        {
            RefreshProgressInfoContentSize();
        }

        return base.MeasureOverride(availableSize);
    }

    /// <summary>
    /// Recalculates the size of the info area, so that the layout follows the current size of custom progress
    /// info content.
    /// </summary>
    /// <remarks>
    /// The base class calls it from <see cref="MeasureOverride" /> before measuring, but only while
    /// <see cref="AbstractProgressBar.ProgressInfoContent" /> is set, because custom content can change its
    /// size after it was set. An override must call the base member.
    /// </remarks>
    private protected virtual void RefreshProgressInfoContentSize()
    {
        _extraInfoSize = CalculateExtraInfoSize(FontSize);
    }

    /// <summary>
    /// Updates the <c>vertical</c> and <c>horizontal</c> pseudo classes from <see cref="Orientation" />. It is called
    /// after a template is applied and when <see cref="Orientation" /> changes while the control is attached to a
    /// visual tree. Derived overrides must call the base member.
    /// </summary>
    protected virtual void NotifyOrientationChanged()
    {
        UpdatePseudoClasses();
    }

    private bool _lastCompletedStatus;

    /// <summary>
    /// Recalculates the info area size when the bar switches between complete and incomplete, because the complete
    /// state shows a status icon instead of the percentage text. The base class calls it when the value is updated
    /// while the percentage label is shown, and when the width or height changes in a visual tree. Derived
    /// overrides must call the base member.
    /// </summary>
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
