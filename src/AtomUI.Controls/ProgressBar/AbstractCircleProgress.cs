using System.Globalization;
using AtomUI.Icons.AntDesign;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using ProgressPath = Avalonia.Controls.Shapes.Path;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of the circular progress controls. It lays out the circle, derives the stroke thickness and the
/// sizes of the centered info text and icon from the circle size, provides the step mode, and caches the
/// geometries that derived types build.
/// </summary>
/// <remarks>
/// Derived types implement <see cref="BuildRailGeometry" />, <see cref="BuildTrackGeometry" /> and
/// <see cref="BuildSuccessGeometry" />. A template that contains the path parts <c>PART_ProgressRail</c>,
/// <c>PART_ProgressTrack</c> and <c>PART_ProgressSuccess</c> renders the geometries itself; without all three
/// parts <see cref="AbstractProgressBar.RenderGroove" /> and <see cref="AbstractProgressBar.RenderIndicatorBar" /> draw them.
/// </remarks>
public abstract class AbstractCircleProgress : AbstractProgressBar
{
    /// <summary>
    /// Circle diameter, in DIPs, used when <see cref="SizeType" /> is Large and neither <see cref="Layoutable.Width" />
    /// nor <see cref="Layoutable.Height" /> is set.
    /// </summary>
    protected const double LARGE_CIRCLE_SIZE = 120;
    /// <summary>
    /// Circle diameter, in DIPs, used when <see cref="SizeType" /> is Middle and neither <see cref="Layoutable.Width"
    /// /> nor <see cref="Layoutable.Height" /> is set. A circle of this size is drawn with <see
    /// cref="AbstractProgressBar.MIDDLE_STROKE_THICKNESS" />, and other sizes scale that thickness in proportion to
    /// their diameter.
    /// </summary>
    protected const double MIDDLE_CIRCLE_SIZE = 90;
    /// <summary>
    /// Circle diameter, in DIPs, used when <see cref="SizeType" /> is Small and neither <see cref="Layoutable.Width" />
    /// nor <see cref="Layoutable.Height" /> is set.
    /// </summary>
    protected const double SMALL_CIRCLE_SIZE = 60;
    /// <summary>
    /// Smallest stroke thickness, in DIPs, the circle is drawn with. An <see
    /// cref="AbstractProgressBar.IndicatorThickness" /> below this value is raised to it.
    /// </summary>
    protected const double CIRCLE_MIN_STROKE_THICKNESS = 3;

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="StepCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> StepCountProperty =
        AvaloniaProperty.Register<AbstractCircleProgress, int>(nameof(StepCount), coerce: (o, v) => Math.Max(v, 0));

    /// <summary>
    /// Identifies the <see cref="StepGap" /> property.
    /// </summary>
    public static readonly StyledProperty<double> StepGapProperty =
        AvaloniaProperty.Register<AbstractCircleProgress, double>(nameof(StepGap), 2, coerce: (o, v) => Math.Max(v, 0));

    /// <summary>
    /// Number of equal steps the circle is divided into. Steps are drawn only while this value and
    /// <see cref="StepGap" /> are both greater than zero; the indicator then fills the <see cref="AbstractProgressBar.Percentage" />
    /// share of the steps, rounded to the nearest step.
    /// </summary>
    /// <remarks>
    /// Step mode draws flat line ends and ignores <see cref="AbstractProgressBar.StrokeLineCap" />. Negative values are
    /// coerced to zero.
    /// </remarks>
    public int StepCount
    {
        get => GetValue(StepCountProperty);
        set => SetValue(StepCountProperty, value);
    }

    /// <summary>
    /// Width of the gap between adjacent steps, in degrees. Steps are drawn only while this value and
    /// <see cref="StepCount" /> are both greater than zero, so a value of zero or less draws a continuous arc
    /// even when <see cref="StepCount" /> is set. The default is 2.
    /// </summary>
    public double StepGap
    {
        get => GetValue(StepGapProperty);
        set => SetValue(StepGapProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> IndicatorAngleProperty =
        AvaloniaProperty.Register<AbstractCircleProgress, double>(nameof(IndicatorAngle));

    internal static readonly StyledProperty<double> CircleMinimumTextFontSizeProperty =
        AvaloniaProperty.Register<AbstractCircleProgress, double>(
            nameof(CircleMinimumTextFontSize));

    internal static readonly StyledProperty<double> CircleMinimumIconSizeProperty =
        AvaloniaProperty.Register<AbstractCircleProgress, double>(
            nameof(CircleMinimumIconSize));

    internal static readonly StyledProperty<bool> UseExtraInfoSizeTokenResourcesProperty =
        AvaloniaProperty.Register<AbstractCircleProgress, bool>(nameof(UseExtraInfoSizeTokenResources));

    // Percentage font size and status icon size computed from the circle size (bounded below by the Tokens).
    // The control theme projects them to FontSize and to the icon presenters, so a local FontSize or an
    // application Style still wins.
    internal static readonly StyledProperty<double> ExtraInfoFontSizeProperty =
        AvaloniaProperty.Register<AbstractCircleProgress, double>(nameof(ExtraInfoFontSize),
            FontSizeProperty.GetDefaultValue(typeof(AbstractCircleProgress)));

    internal static readonly StyledProperty<double> ExtraInfoIconSizeProperty =
        AvaloniaProperty.Register<AbstractCircleProgress, double>(nameof(ExtraInfoIconSize));

    internal static readonly DirectProperty<AbstractCircleProgress, PenLineCap> ShapeStrokeLineCapProperty =
        AvaloniaProperty.RegisterDirect<AbstractCircleProgress, PenLineCap>(
            nameof(ShapeStrokeLineCap),
            owner => owner.ShapeStrokeLineCap,
            (owner, value) => owner.ShapeStrokeLineCap = value);

    internal double IndicatorAngle
    {
        get => GetValue(IndicatorAngleProperty);
        set => SetValue(IndicatorAngleProperty, value);
    }

    internal double CircleMinimumTextFontSize
    {
        get => GetValue(CircleMinimumTextFontSizeProperty);
        set => SetValue(CircleMinimumTextFontSizeProperty, value);
    }

    internal double CircleMinimumIconSize
    {
        get => GetValue(CircleMinimumIconSizeProperty);
        set => SetValue(CircleMinimumIconSizeProperty, value);
    }

    /// <summary>
    /// Lets Tokens bound the sizes of the centered info text and status icon from below. While it is
    /// <see langword="true" /> and the derived type supplies <see cref="ExtraInfoSizeTokenResourceKeys" />, the
    /// control subscribes to those Tokens once it is in a logical tree and raises
    /// <see cref="ExtraInfoFontSize" /> and <see cref="ExtraInfoIconSize" /> to at least the Token values; a set
    /// <see cref="CircleMinimumTextFontSize" /> or <see cref="CircleMinimumIconSize" /> is used as the minimum
    /// instead. The base class supplies no keys, so the value has no effect there. With <see langword="false" />,
    /// the default, the sizes come from the circle size alone.
    /// </summary>
    internal bool UseExtraInfoSizeTokenResources
    {
        get => GetValue(UseExtraInfoSizeTokenResourcesProperty);
        set => SetValue(UseExtraInfoSizeTokenResourcesProperty, value);
    }

    /// <summary>
    /// The font size of the centered info text, in DIPs. The control calculates it from the circle size as 15
    /// percent of the diameter plus 6, raised to the minimum described at
    /// <see cref="UseExtraInfoSizeTokenResources" />, and recalculates it while the control is attached to a visual
    /// tree, after the template is applied and whenever the circle size or that minimum changes. The theme projects
    /// it to the font size of the info text through a Setter, so a local <c>FontSize</c> or an application Style
    /// still wins. Until the first calculation it holds the default font size of the control.
    /// </summary>
    internal double ExtraInfoFontSize
    {
        get => GetValue(ExtraInfoFontSizeProperty);
        set => SetValue(ExtraInfoFontSizeProperty, value);
    }

    /// <summary>
    /// The side length of the centered status icon, in DIPs. The control calculates it from the circle size as the
    /// diameter divided by 4.5, raised to the minimum described at <see cref="UseExtraInfoSizeTokenResources" />,
    /// at the same times as <see cref="ExtraInfoFontSize" />. The theme projects it to the size of the icon
    /// presenters through a Setter. The status icon is shown only while this size fits into the circle.
    /// </summary>
    internal double ExtraInfoIconSize
    {
        get => GetValue(ExtraInfoIconSizeProperty);
        set => SetValue(ExtraInfoIconSizeProperty, value);
    }

    private PenLineCap _shapeStrokeLineCap;

    /// <summary>
    /// The line cap that the circle arcs are drawn with: <see cref="AbstractProgressBar.StrokeLineCap" />, or
    /// <see cref="PenLineCap.Flat" /> in step mode. The control writes it when the template is applied and when the
    /// progress value, the value range, the stroke, the cap or the step settings change. The themes pass it to the
    /// template paths as their line cap.
    /// </summary>
    internal PenLineCap ShapeStrokeLineCap
    {
        get => _shapeStrokeLineCap;
        private set => SetAndRaise(ShapeStrokeLineCapProperty, ref _shapeStrokeLineCap, value);
    }

    #endregion

    private OwnerTokenSet? _circleMinimumTokens;
    private (double Text, double Icon) _circleMinimums;

    /// <summary>
    /// Resource keys of the Tokens that give the minimum size of the extra info text and of the extra info icon.
    /// </summary>
    /// <remarks>
    /// The keys are subscribed only while <see cref="UseExtraInfoSizeTokenResources" /> is
    /// <see langword="true" />. The base class returns <see langword="null" />, which leaves both minimum sizes
    /// at zero; <c>CircleProgress</c> in AtomUI.Desktop.Controls returns the keys of its Tokens.
    /// </remarks>
    private protected virtual (object Text, object Icon)? ExtraInfoSizeTokenResourceKeys => null;

    private ProgressPath? _progressRail;
    private ProgressPath? _progressTrack;
    private ProgressPath? _progressSuccess;
    private RailGeometryKey? _railGeometryKey;
    private Geometry? _railGeometry;
    private SuccessGeometryKey? _successGeometryKey;
    private Geometry? _successGeometry;
    private StepTrackGeometryKey? _stepTrackGeometryKey;
    private Geometry? _stepTrackGeometry;
    private FullTextSizeKey? _fullTextSizeKey;
    private Size _fullTextSize;

    // The rail geometry depends only on the groove rect, the step settings, the stroke and the line cap; a
    // progress change does not rebuild it
    private readonly record struct RailGeometryKey(
        Rect GrooveRect,
        int StepCount,
        double StepGap,
        double StrokeThickness,
        PenLineCap LineCap);

    // The success arc depends only on the rail parameters, the threshold and the value range; in step mode it
    // also depends on the number of filled steps
    private readonly record struct SuccessGeometryKey(
        RailGeometryKey Rail,
        double Threshold,
        double Minimum,
        double Maximum,
        int FilledSteps);

    private readonly record struct StepTrackGeometryKey(RailGeometryKey Rail, int FilledSteps);

    private readonly record struct FullTextSizeKey(string Format, double FontSize, FontFamily FontFamily);

    /// <summary>
    /// Number of times the rail geometry has been built. The template paths and the self-drawn rendering share
    /// one cache, so tests and benchmarks read this value to tell whether the cache was hit.
    /// </summary>
    internal int RailGeometryBuildCount { get; private set; }

    /// <summary>
    /// Number of times the size of the "100%" text has been measured. Tests and benchmarks read this value to
    /// tell whether the size cache was hit.
    /// </summary>
    internal int FullTextMeasureCount { get; private set; }

    /// <summary>
    /// Whether the applied template supplied all three path parts <c>PART_ProgressRail</c>,
    /// <c>PART_ProgressTrack</c> and <c>PART_ProgressSuccess</c>, which then render the geometries in place of
    /// <see cref="AbstractProgressBar.RenderGroove" /> and
    /// <see cref="AbstractProgressBar.RenderIndicatorBar" />.
    /// </summary>
    private protected override bool HasTemplateProgressVisuals =>
        _progressRail is not null && _progressTrack is not null && _progressSuccess is not null;

    static AbstractCircleProgress()
    {
        AffectsMeasure<AbstractCircleProgress>(StepCountProperty,
            StepGapProperty);
        AffectsRender<AbstractCircleProgress>(IndicatorAngleProperty);
    }

    /// <summary>
    /// Maps a circle diameter to its size type: Large from <see cref="LARGE_CIRCLE_SIZE" /> up, Middle from
    /// <see cref="MIDDLE_CIRCLE_SIZE" /> up, and Small below that.
    /// </summary>
    /// <remarks>
    /// Nothing calls this implementation: a circle progress takes its effective size type from the
    /// <see cref="SizeType" /> property, so it only satisfies the abstract member of the base class.
    /// </remarks>
    protected override SizeType CalculateEffectiveSizeType(double size)
    {
        var sizeType             = SizeType.Large;
        if (MathUtils.GreaterThanOrClose(size, LARGE_CIRCLE_SIZE))
        {
            sizeType = SizeType.Large;
        }
        else if (MathUtils.GreaterThanOrClose(size, MIDDLE_CIRCLE_SIZE))
        {
            sizeType = SizeType.Middle;
        }
        else
        {
            sizeType = SizeType.Small;
        }

        return sizeType;
    }

    private static double GetSizeTypeDefaultValue(SizeType sizeType)
    {
        return sizeType switch
        {
            SizeType.Large => LARGE_CIRCLE_SIZE,
            SizeType.Middle => MIDDLE_CIRCLE_SIZE,
            _ => SMALL_CIRCLE_SIZE
        };
    }

    /// <summary>
    /// Requests a square whose side is the circle size: <see cref="Layoutable.Width" /> or <see
    /// cref="Layoutable.Height" />, the smaller of the two when both are set, and otherwise the default diameter of the
    /// size type. The side never exceeds the smaller available dimension, and with stretch alignment it takes that
    /// dimension even when the circle would be smaller.
    /// </summary>
    /// <remarks>
    /// The base implementation is measured first and its result is discarded.
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);
        var targetSize = CalculateCircleSize();
        if (!double.IsInfinity(availableSize.Width) && !double.IsInfinity(availableSize.Height))
        {
            var minSize = Math.Min(availableSize.Width, availableSize.Height);
            if (minSize < targetSize || IsStretchAlignment())
            {
                targetSize = minSize;
            }
        }
        else if (!double.IsInfinity(availableSize.Width) && double.IsInfinity(availableSize.Height))
        {
            if (availableSize.Width < targetSize || IsStretchAlignment())
            {
                targetSize = availableSize.Width;
            }
        }
        else if (!double.IsInfinity(availableSize.Height) && double.IsInfinity(availableSize.Width))
        {
            if (availableSize.Height < targetSize || IsStretchAlignment())
            {
                targetSize = availableSize.Height;
            }
        }

        return new Size(targetSize, targetSize);
    }

    private bool IsStretchAlignment()
    {
        return HorizontalAlignment == HorizontalAlignment.Stretch || VerticalAlignment == VerticalAlignment.Stretch;
    }

    private double CalculateCircleSize()
    {
        var targetSize           = 0d;
        var sizeTypeDefaultValue = GetSizeTypeDefaultValue(EffectiveSizeType);
        if (double.IsNaN(Width) && double.IsNaN(Height))
        {
            targetSize = sizeTypeDefaultValue;
        }
        else if (double.IsNaN(Width) && !double.IsNaN(Height))
        {
            targetSize = Height;
        }
        else if (!double.IsNaN(Width) && double.IsNaN(Height))
        {
            targetSize = Width;
        }
        else
        {
            targetSize = Math.Min(Width, Height);
        }

        return targetSize;
    }

    /// <summary>
    /// Called after <see cref="AbstractProgressBar.ProgressInfoContent" /> is set or cleared. It calls the base
    /// implementation and invalidates the arrangement, because the centered info area is sized and positioned in <see
    /// cref="ArrangeOverride" />.
    /// </summary>
    protected override void NotifyProgressInfoContentChanged()
    {
        base.NotifyProgressInfoContentChanged();
        InvalidateArrange();
    }

    /// <summary>
    /// Recalculates the stroke thickness for the current circle size and records whether the info elements fit into the
    /// circle, each while its template part exists: <see cref="AbstractProgressBar.IsPercentLabelVisible" /> when the
    /// "100%" text fits into the square inside the stroke, and <see cref="AbstractProgressBar.IsStatusIconVisible" />
    /// when the status icon does.
    /// </summary>
    /// <remarks>
    /// The base class calls it from <see cref="AbstractProgressBar.NotifyUpdateProgress" /> while the progress info is
    /// visible, and when <see cref="Layoutable.Width" /> or <see cref="Layoutable.Height" /> change while the control
    /// is attached to the visual tree.
    /// </remarks>
    protected override void NotifyHandleExtraInfoVisibility()
    {
        var circleSize = CalculateCircleSize();
        CalculateStrokeThickness(circleSize);
        var extraInfoSize = circleSize - EffectiveStrokeThickness - 1; // A fixed padding of 1 DIP
        var extraInfo     = GetFullTextSize();

        // The percent label and the status icons are treated as alternatives: each one alone is checked against the
        // whole inner size
        if (LayoutTransformLabel is not null)
        {
            if (extraInfo.Width > extraInfoSize || extraInfo.Height > extraInfoSize)
            {
                IsPercentLabelVisible = false;
            }
            else
            {
                IsPercentLabelVisible = true;
            }
        }

        if (ExceptionCompletedIconPresenter is not null || SuccessCompletedIconPresenter is not null)
        {
            IsStatusIconVisible = ExtraInfoIconSize <= extraInfoSize;
        }
    }

    // Every progress update checks whether the full text fits into the circle, so its size is cached by format
    // and font
    private Size GetFullTextSize()
    {
        var key = new FullTextSizeKey(ProgressTextFormat, FontSize, FontFamily);
        if (_fullTextSizeKey != key)
        {
            _fullTextSize    = TextUtils.CalculateTextSize(string.Format(ProgressTextFormat, 100), FontSize, FontFamily);
            _fullTextSizeKey = key;
            FullTextMeasureCount++;
        }

        return _fullTextSize;
    }

    /// <summary>
    /// Calls the base implementation, then keeps the circle in step with property changes. While the control is
    /// attached to the logical tree, a change of <see cref="UseExtraInfoSizeTokenResources" /> renews the Token
    /// subscription of the info sizes. While it is attached to the visual tree, a change of <see
    /// cref="Layoutable.Width" />, <see cref="Layoutable.Height" />, <see cref="CircleMinimumTextFontSize" /> or <see
    /// cref="CircleMinimumIconSize" /> recalculates the stroke thickness and the info sizes.
    /// </summary>
    /// <remarks>
    /// A change of <see cref="IndicatorAngle" /> refreshes only the track data of the template paths. A change of
    /// the value, the value range, <see cref="AbstractProgressBar.SuccessThreshold" />, the step settings, the stroke
    /// thickness or <see cref="AbstractProgressBar.StrokeLineCap" /> also updates <see cref="ShapeStrokeLineCap" /> and
    /// refreshes the rail, track and success data.
    /// </remarks>
    protected override void NotifyPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.NotifyPropertyChanged(change);

        if (change.Property == UseExtraInfoSizeTokenResourcesProperty && ((ILogical)this).IsAttachedToLogicalTree)
        {
            UpdateExtraInfoSizeTokenSubscription();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == WidthProperty || change.Property == HeightProperty ||
                change.Property == CircleMinimumTextFontSizeProperty ||
                change.Property == CircleMinimumIconSizeProperty)
            {
                UpdateCircleMetrics();
            }
        }

        if (change.Property == IndicatorAngleProperty)
        {
            // An angle change alone, for example during a theme transition, rebuilds only the filled arc
            RefreshTemplateTrackVisual();
        }
        else if (change.Property == ValueProperty ||
            change.Property == MinimumProperty ||
            change.Property == MaximumProperty ||
            change.Property == SuccessThresholdProperty ||
            change.Property == StepCountProperty ||
            change.Property == StepGapProperty ||
            change.Property == EffectiveStrokeThicknessProperty ||
            change.Property == StrokeLineCapProperty)
        {
            ShapeStrokeLineCap = StepCount > 0 && StepGap > 0
                ? PenLineCap.Flat
                : StrokeLineCap;
            RefreshTemplateProgressVisuals();
        }
    }

    /// <summary>
    /// Calls the base implementation, then subscribes to the Tokens that bound the info sizes from below when
    /// <see cref="UseExtraInfoSizeTokenResources" /> is set and the derived type supplies
    /// <see cref="ExtraInfoSizeTokenResourceKeys" />.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        UpdateExtraInfoSizeTokenSubscription();
    }

    /// <summary>
    /// Disposes the Token subscription of the info sizes and resets the minimum sizes to zero, then calls the
    /// base implementation.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _circleMinimumTokens?.Dispose();
        _circleMinimumTokens = null;
        _circleMinimums = default;
        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    /// Calls the base implementation, then recalculates the info sizes from the subscribed Tokens when the Token
    /// subscription exists.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_circleMinimumTokens is not null)
        {
            // A reused template may have picked up a new resource scope before visual attachment.
            SetupExtraInfoSize(CalculateCircleSize());
        }
    }

    private void UpdateExtraInfoSizeTokenSubscription()
    {
        if (UseExtraInfoSizeTokenResources && ExtraInfoSizeTokenResourceKeys is { } keys)
        {
            if (_circleMinimumTokens is not null)
            {
                return;
            }

            _circleMinimumTokens = new OwnerTokenSet(this, 2, RefreshCircleMinimums);
            try
            {
                _circleMinimumTokens.SetResourceKey(0, keys.Text);
                _circleMinimumTokens.SetResourceKey(1, keys.Icon);
                RefreshCircleMinimums();
            }
            catch
            {
                _circleMinimumTokens.Dispose();
                _circleMinimumTokens = null;
                _circleMinimums = default;
                throw;
            }
        }
        else if (_circleMinimumTokens is not null)
        {
            _circleMinimumTokens.Dispose();
            _circleMinimumTokens = null;
            RefreshCircleMinimums();
        }
    }

    private void RefreshCircleMinimums()
    {
        var next = (ReadMinimum(_circleMinimumTokens?.GetValue(0), _circleMinimums.Text),
                    ReadMinimum(_circleMinimumTokens?.GetValue(1), _circleMinimums.Icon));
        if (_circleMinimums.Equals(next))
        {
            return;
        }

        _circleMinimums = next;
        if (this.IsAttachedToVisualTree())
        {
            SetupExtraInfoSize(CalculateCircleSize());
        }
    }

    private static double ReadMinimum(object? value, double previous)
    {
        if (value is double size)
        {
            return size;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }

        // Match the original typed theme setters for numeric resources and invalid values.
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    private void SetupExtraInfoSize(double circleSize)
    {
        var textMinimum = IsSet(CircleMinimumTextFontSizeProperty) ? CircleMinimumTextFontSize : _circleMinimums.Text;
        var iconMinimum = IsSet(CircleMinimumIconSizeProperty) ? CircleMinimumIconSize : _circleMinimums.Icon;
        var fontSize = circleSize * 0.15 + 6;
        if (fontSize < textMinimum)
        {
            fontSize = textMinimum;
        }

        ExtraInfoFontSize = fontSize;
        ExtraInfoIconSize = Math.Max(circleSize / 4.5, iconMinimum);
    }

    /// <summary>
    /// Centers the percent label, the status icon presenters and the custom info content, each in a band that
    /// spans the width of the arranged area and is as high as the visible content. The base implementation
    /// arranges the template; the template path data is then refreshed for the arranged size and the info
    /// panel is arranged into the band.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        Rect progressIndicatorRect = default;
        if (IsProgressInfoPresented)
        {
            var contentSize = GetVisibleProgressIndicatorContentSize();
            progressIndicatorRect = new Rect(
                0,
                (finalSize.Height - contentSize.Height) / 2,
                finalSize.Width,
                contentSize.Height);

            if (LayoutTransformLabel is not null)
            {
                var labelSize = LayoutTransformLabel.DesiredSize;
                var offsetX = (progressIndicatorRect.Width - labelSize.Width) / 2;
                var offsetY = (progressIndicatorRect.Height - labelSize.Height) / 2;
                Canvas.SetLeft(LayoutTransformLabel, offsetX);
                Canvas.SetTop(LayoutTransformLabel, offsetY);
            }

            if (SuccessCompletedIconPresenter is not null)
            {
                var size    = SuccessCompletedIconPresenter.DesiredSize;
                var offsetX = (progressIndicatorRect.Width - size.Width) / 2;
                var offsetY = (progressIndicatorRect.Height - size.Height) / 2;
                Canvas.SetLeft(SuccessCompletedIconPresenter, offsetX);
                Canvas.SetTop(SuccessCompletedIconPresenter, offsetY);
            }

            if (ExceptionCompletedIconPresenter is not null)
            {
                var size    = ExceptionCompletedIconPresenter.DesiredSize;
                var offsetX = (progressIndicatorRect.Width - size.Width) / 2;
                var offsetY = (progressIndicatorRect.Height - size.Height) / 2;
                Canvas.SetLeft(ExceptionCompletedIconPresenter, offsetX);
                Canvas.SetTop(ExceptionCompletedIconPresenter, offsetY);
            }

            if (ProgressInfoContentPresenter is not null && ProgressInfoContent is not null)
            {
                var size    = ProgressInfoContentPresenter.DesiredSize;
                var offsetX = (progressIndicatorRect.Width - size.Width) / 2;
                var offsetY = (progressIndicatorRect.Height - size.Height) / 2;
                Canvas.SetLeft(ProgressInfoContentPresenter, offsetX);
                Canvas.SetTop(ProgressInfoContentPresenter, offsetY);
            }
        }

        var arrangedSize = base.ArrangeOverride(finalSize);
        UpdateTemplateProgressVisuals(finalSize);
        ArrangeProgressIndicator(progressIndicatorRect);
        return arrangedSize;
    }

    private Size GetVisibleProgressIndicatorContentSize()
    {
        if (MeasureProgressInfoContent() is { } contentSize &&
            contentSize.Width > 0 && contentSize.Height > 0)
        {
            return contentSize;
        }

        if (LayoutTransformLabel?.IsVisible == true)
        {
            LayoutTransformLabel.Measure(Size.Infinity);
            if (LayoutTransformLabel.DesiredSize.Width > 0 && LayoutTransformLabel.DesiredSize.Height > 0)
            {
                return LayoutTransformLabel.DesiredSize;
            }
        }

        if (SuccessCompletedIconPresenter?.IsVisible == true)
        {
            SuccessCompletedIconPresenter.Measure(Size.Infinity);
            if (SuccessCompletedIconPresenter.DesiredSize.Width > 0 &&
                SuccessCompletedIconPresenter.DesiredSize.Height > 0)
            {
                return SuccessCompletedIconPresenter.DesiredSize;
            }
        }

        if (ExceptionCompletedIconPresenter?.IsVisible == true)
        {
            ExceptionCompletedIconPresenter.Measure(Size.Infinity);
            if (ExceptionCompletedIconPresenter.DesiredSize.Width > 0 &&
                ExceptionCompletedIconPresenter.DesiredSize.Height > 0)
            {
                return ExceptionCompletedIconPresenter.DesiredSize;
            }
        }

        return new Size(0, Math.Max(FontSize, 1));
    }

    /// <summary>
    /// Returns the largest square centered in <paramref name="controlRect" />. It bounds the circle.
    /// </summary>
    /// <param name="controlRect">
    /// The layout rectangle of the control.
    /// </param>
    protected override Rect GetProgressBarRect(Rect controlRect)
    {
        var size = controlRect.Size;
        var diameter = Math.Min(size.Width, size.Height);
        return new Rect(size).CenterRect(new Rect(new Size(diameter, diameter)));
    }

    /// <summary>
    /// Returns the square for the info elements in the center of the circle: the circle rectangle shrunk by the
    /// effective stroke thickness on each side.
    /// </summary>
    /// <param name="controlRect">
    /// The layout rectangle of the control.
    /// </param>
    protected override Rect GetExtraInfoRect(Rect controlRect)
    {
        return GetProgressBarRect(controlRect).Deflate(EffectiveStrokeThickness);
    }

    /// <summary>
    /// Sets <see cref="AbstractProgressBar.EffectiveStrokeThickness" /> for the current circle size. An
    /// <see cref="AbstractProgressBar.IndicatorThickness" /> that is not <see langword="NaN" /> is used, raised to at least
    /// <see cref="CIRCLE_MIN_STROKE_THICKNESS" />; otherwise the default thickness is scaled in proportion to the
    /// circle size, with the same minimum.
    /// </summary>
    protected override void CalculateStrokeThickness()
    {
        CalculateStrokeThickness(CalculateCircleSize());
    }

    private void CalculateStrokeThickness(double circleSize)
    {
        var calculatedValue = MIDDLE_STROKE_THICKNESS / MIDDLE_CIRCLE_SIZE * circleSize;
        calculatedValue = Math.Max(calculatedValue, CIRCLE_MIN_STROKE_THICKNESS);
        if (!double.IsNaN(IndicatorThickness))
        {
            calculatedValue = Math.Max(IndicatorThickness, CIRCLE_MIN_STROKE_THICKNESS);
        }

        EffectiveStrokeThickness = calculatedValue;
    }

    /// <summary>
    /// Recalculates the stroke thickness and the sizes of the info text and icon for the current circle size. It
    /// replaces the base implementation, which only recalculates the stroke thickness.
    /// </summary>
    /// <remarks>
    /// The base class calls it when <see cref="AbstractProgressBar.EffectiveSizeType" /> changes while the control is
    /// attached to the visual tree, and once after the template is applied.
    /// </remarks>
    protected override void NotifyEffectSizeTypeChanged()
    {
        UpdateCircleMetrics();
    }

    private void UpdateCircleMetrics()
    {
        var circleSize = CalculateCircleSize();
        CalculateStrokeThickness(circleSize);
        SetupExtraInfoSize(circleSize);
    }

    /// <summary>
    /// Clears the template part references and the geometry cache, calls the base implementation, then looks up
    /// the template paths <c>PART_ProgressRail</c>, <c>PART_ProgressTrack</c> and <c>PART_ProgressSuccess</c>,
    /// fills their data and installs the default completed icons: a close icon for the exception state and a check
    /// icon for the success state, each only when no icon is set.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _progressRail = null;
        _progressTrack = null;
        _progressSuccess = null;
        InvalidateGeometryCache();
        base.OnApplyTemplate(e);
        _progressRail = e.NameScope.Find<ProgressPath>(ProgressRailPart);
        _progressTrack = e.NameScope.Find<ProgressPath>(ProgressTrackPart);
        _progressSuccess = e.NameScope.Find<ProgressPath>(ProgressSuccessPart);
        ShapeStrokeLineCap = StepCount > 0 && StepGap > 0
            ? PenLineCap.Flat
            : StrokeLineCap;
        RefreshTemplateProgressVisuals();
        SetupDefaultCompletedIcons(static () => new CloseOutlined(), static () => new CheckOutlined());
    }

    /// <summary>
    /// Builds the geometry of the rail, the background of the ring, inside <paramref name="grooveRect" />.
    /// </summary>
    /// <param name="grooveRect">
    /// Square the ring is drawn in, deflated by half the stroke thickness so that the stroke is centered on
    /// its edge.
    /// </param>
    /// <remarks>
    /// It is requested through <see cref="GetRailGeometry" /> when no cached rail matches
    /// <see cref="RailGeometryKey" />, for the template part <c>PART_ProgressRail</c> and for the rail that a
    /// derived <see cref="AbstractProgressBar.RenderGroove" /> draws. The result is cached, so it may depend
    /// only on the groove rectangle, the step settings, the stroke thickness and the line cap. A rail that
    /// depends on another property requires <see cref="InvalidateGeometryCache" /> on a change of that property.
    /// </remarks>
    private protected abstract Geometry BuildRailGeometry(Rect grooveRect);
    /// <summary>
    /// Builds the geometry of the track, the filled part of the ring, inside <paramref name="grooveRect" />.
    /// </summary>
    /// <param name="grooveRect">
    /// Square the ring is drawn in, deflated by half the stroke thickness so that the stroke is centered on
    /// its edge.
    /// </param>
    /// <remarks>
    /// It is requested through <see cref="GetTrackGeometry" /> for the template part <c>PART_ProgressTrack</c>
    /// and for the indicator that a derived <see cref="AbstractProgressBar.RenderIndicatorBar" /> draws. Outside
    /// step mode it is built on every request and may follow <see cref="IndicatorAngle" />. In step mode it is
    /// cached by <see cref="FilledStepCount" />, so there it must not depend on the angle.
    /// </remarks>
    private protected abstract Geometry BuildTrackGeometry(Rect grooveRect);
    /// <summary>
    /// Builds the geometry of the success arc, the part of the ring from its start up to the angle of
    /// <see cref="AbstractProgressBar.SuccessThreshold" />, inside <paramref name="grooveRect" />.
    /// </summary>
    /// <param name="grooveRect">
    /// Square the ring is drawn in, deflated by half the stroke thickness so that the stroke is centered on
    /// its edge.
    /// </param>
    /// <remarks>
    /// It is requested only while <see cref="AbstractProgressBar.SuccessThreshold" /> is not
    /// <see langword="NaN" />: through <see cref="GetSuccessGeometry" />, for the template part
    /// <c>PART_ProgressSuccess</c> and for the success arc that a derived
    /// <see cref="AbstractProgressBar.RenderIndicatorBar" /> draws. The result is cached, so it may depend only
    /// on the inputs of the rail geometry, the threshold, <see cref="RangeBase.Minimum" />,
    /// <see cref="RangeBase.Maximum" /> and, in step mode, the number of filled steps.
    /// </remarks>
    private protected abstract Geometry BuildSuccessGeometry(Rect grooveRect);

    private protected void RefreshTemplateProgressVisuals()
    {
        UpdateTemplateProgressVisuals(Bounds.Size);
    }

    /// <summary>
    /// Drops the cached rail, success and step geometries, so they are rebuilt the next time they are requested.
    /// A derived type calls it when a parameter its geometries depend on changes, such as the gap of the
    /// dashboard.
    /// </summary>
    private protected void InvalidateGeometryCache()
    {
        _railGeometryKey      = null;
        _railGeometry         = null;
        _successGeometryKey   = null;
        _successGeometry      = null;
        _stepTrackGeometryKey = null;
        _stepTrackGeometry    = null;
    }

    private protected bool IsStepMode => StepCount > 0 && StepGap > 0;

    private protected int FilledStepCount => (int)Math.Round(StepCount * Percentage / 100);

    private RailGeometryKey CreateRailGeometryKey(Rect grooveRect)
    {
        return new RailGeometryKey(grooveRect, StepCount, StepGap, EffectiveStrokeThickness, ShapeStrokeLineCap);
    }

    /// <summary>
    /// Returns the rail geometry for <paramref name="grooveRect" />, reusing the object built last while
    /// <see cref="RailGeometryKey" /> is unchanged.
    /// </summary>
    private protected Geometry GetRailGeometry(Rect grooveRect)
    {
        var key = CreateRailGeometryKey(grooveRect);
        if (_railGeometry is null || _railGeometryKey != key)
        {
            _railGeometry    = BuildRailGeometry(grooveRect);
            _railGeometryKey = key;
            RailGeometryBuildCount++;
        }

        return _railGeometry;
    }

    /// <summary>
    /// Returns the success arc geometry for <paramref name="grooveRect" />, reusing the object built last while
    /// the key is unchanged. The caller makes sure that <see cref="AbstractProgressBar.SuccessThreshold" /> is not
    /// <see langword="NaN" />.
    /// </summary>
    private protected Geometry GetSuccessGeometry(Rect grooveRect)
    {
        var key = new SuccessGeometryKey(CreateRailGeometryKey(grooveRect),
            SuccessThreshold,
            Minimum,
            Maximum,
            IsStepMode ? FilledStepCount : -1);
        if (_successGeometry is null || _successGeometryKey != key)
        {
            _successGeometry    = BuildSuccessGeometry(grooveRect);
            _successGeometryKey = key;
        }

        return _successGeometry;
    }

    /// <summary>
    /// Returns the geometry of the filled part for <paramref name="grooveRect" />. Outside step mode it is built
    /// on every call because it follows <see cref="IndicatorAngle" />; in step mode it is cached by the number of
    /// filled steps, so an angle transition does not rebuild it.
    /// </summary>
    private protected Geometry GetTrackGeometry(Rect grooveRect)
    {
        if (!IsStepMode)
        {
            return BuildTrackGeometry(grooveRect);
        }

        var key = new StepTrackGeometryKey(CreateRailGeometryKey(grooveRect), FilledStepCount);
        if (_stepTrackGeometry is null || _stepTrackGeometryKey != key)
        {
            _stepTrackGeometry    = BuildTrackGeometry(grooveRect);
            _stepTrackGeometryKey = key;
        }

        return _stepTrackGeometry;
    }

    private void RefreshTemplateTrackVisual()
    {
        var size = Bounds.Size;
        if (!HasTemplateProgressVisuals || size.Width <= 0 || size.Height <= 0)
        {
            return;
        }

        _progressTrack!.Data = GetTrackGeometry(new Rect(default, GetCircleProgressPathRect(size).Size));
    }

    internal Rect GetCircleProgressPathRect(Size size)
    {
        var strokeThickness = double.IsFinite(EffectiveStrokeThickness)
            ? Math.Max(0, EffectiveStrokeThickness)
            : 0;
        // Keep both radii equal when the parent arranges the control into a rectangular slot.
        var diameter = Math.Floor(Math.Max(0, Math.Min(size.Width, size.Height) - strokeThickness));
        return new Rect(size).CenterRect(new Rect(new Size(diameter, diameter)));
    }

    private void UpdateTemplateProgressVisuals(Size size)
    {
        if (!HasTemplateProgressVisuals || size.Width <= 0 || size.Height <= 0)
        {
            return;
        }

        var grooveRect = new Rect(default, GetCircleProgressPathRect(size).Size);
        _progressRail!.Data = GetRailGeometry(grooveRect);
        _progressTrack!.Data = GetTrackGeometry(grooveRect);
        _progressSuccess!.IsVisible = !double.IsNaN(SuccessThreshold);
        _progressSuccess.Data = _progressSuccess.IsVisible
            ? GetSuccessGeometry(grooveRect)
            : null;
    }

}
