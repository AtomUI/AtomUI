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

/// <summary>
/// Base class for line progress bars. <see cref="PercentPosition" /> selects whether the progress info is
/// placed beside the bar or inside it.
/// It sizes the control, resolves the size type and the stroke thickness, and computes the rectangles of the
/// groove and of the info area for a horizontal or a vertical bar.
/// A template that provides a <c>LineProgressPanel</c> as <c>PART_ProgressBody</c> lets that panel draw the
/// bar; without such a panel the control draws the groove and the indicator itself.
/// </summary>
[PseudoClasses(ProgressBarPseudoClass.Indeterminate, ProgressBarPseudoClass.Completed)]
public abstract class AbstractGeneralProgressBar : AbstractLineProgress
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="PercentPosition" /> property.
    /// </summary>
    public static readonly StyledProperty<PercentPosition> PercentPositionProperty =
        AvaloniaProperty.Register<AbstractGeneralProgressBar, PercentPosition>(nameof(PercentPosition), new PercentPosition());

    /// <summary>
    /// Positions the progress info either inside the bar or outside it, at the start, the center or the end.
    /// </summary>
    /// <remarks>
    /// <see cref="PercentPosition.IsInner" /> selects inside or outside the bar and
    /// <see cref="PercentPosition.Alignment" /> the place. Outside the bar, <c>Start</c> and <c>End</c> place the
    /// info before and after the bar, and <c>Center</c> below a horizontal bar or beside a vertical one. Inside
    /// the bar, the info is drawn over the filled part at its start, center or end, and the bar is thickened so
    /// that the info fits. The position applies only while progress info is presented; it also affects the size
    /// of the control and sets the <c>:labelinner</c>, <c>:labelinner-start</c>, <c>:labelinner-center</c> and
    /// <c>:labelinner-end</c> pseudo-classes. The default places the info outside the bar, at its end.
    /// </remarks>
    public PercentPosition PercentPosition
    {
        get => GetValue(PercentPositionProperty);
        set => SetValue(PercentPositionProperty, value);
    }

    #endregion

    private LineProgressPanel? _progressBody;
    private (Color? Label, Color? Solid) _innerLabelColors;

    /// <summary>
    /// Whether the applied template supplied the <c>PART_ProgressBody</c> panel, which then draws the groove
    /// and the indicator of the bar itself.
    /// </summary>
    private protected override bool HasTemplateProgressVisuals => _progressBody is not null;

    static AbstractGeneralProgressBar()
    {
        AffectsMeasure<AbstractGeneralProgressBar>(IndicatorThicknessProperty, PercentPositionProperty);
    }

    /// <summary>
    /// Sizes the control to the available size along the bar and to the stroke thickness across it.
    /// </summary>
    /// <remarks>
    /// Info placed outside the bar at the center adds its own size and the info margin across the bar. Along the
    /// bar the control takes the available size when it is finite, otherwise the minimum size
    /// (<see cref="Layoutable.MinWidth" /> or <see cref="Layoutable.MinHeight" />), otherwise zero.
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);
        double targetWidth  = 0;
        double targetHeight = 0;
        if (Orientation == Orientation.Horizontal)
        {
            targetHeight = EffectiveStrokeThickness;
            if (!PercentPosition.IsInner && IsProgressInfoPresented)
            {
                if (PercentPosition.Alignment == LinePercentAlignment.Center)
                {
                    targetHeight += _extraInfoSize.Height + LineExtraInfoMargin;
                }
            }

            if (!double.IsInfinity(availableSize.Width))
            {
                targetWidth = availableSize.Width;
            }
            else if (!double.IsNaN(MinWidth))
            {
                targetWidth = MinWidth;
            }
        }
        else
        {
            targetWidth = EffectiveStrokeThickness;
            if (!PercentPosition.IsInner && IsProgressInfoPresented)
            {
                if (PercentPosition.Alignment == LinePercentAlignment.Center)
                {
                    targetWidth += _extraInfoSize.Width + LineExtraInfoMargin;
                }
            }

            targetWidth = Math.Max(targetWidth, MinWidth);
            if (!double.IsInfinity(availableSize.Height))
            {
                targetHeight = availableSize.Height;
            }
            else if (!double.IsNaN(MinHeight))
            {
                targetHeight = MinHeight;
            }
        }

        return new Size(targetWidth, targetHeight);
    }

    /// <summary>
    /// Draws the groove on the bar rectangle computed from the control bounds, and stores that rectangle as the
    /// groove used by <see cref="RenderIndicatorBar" />.
    /// </summary>
    /// <remarks>
    /// The base class calls it from <c>Render</c> only when the template has no progress body panel.
    /// </remarks>
    protected override void RenderGroove(DrawingContext context)
    {
        var controlRect = new Rect(new Point(0, 0), Bounds.Size);
        _grooveRect = GetProgressBarRect(controlRect);
        DrawProgressRectangle(context, GrooveBrush, _grooveRect);
    }

    /// <summary>
    /// Draws the indicator on the groove stored by <see cref="RenderGroove" />, over the filled fraction of the bar
    /// given by <see cref="AbstractLineProgress.IndicatorRatio" />.
    /// </summary>
    /// <remarks>
    /// When <see cref="AbstractProgressBar.SuccessThreshold" /> is not <see cref="double.NaN" />, the segment from
    /// the start of the bar up to the threshold, clamped to the range, is painted with
    /// <see cref="AbstractProgressBar.SuccessStrokeBrush" />, whether or not the indicator reaches the threshold. A
    /// segment shorter than half the bar thickness with round line caps, or shorter than 1 without them, is not
    /// drawn.
    /// The base class calls it from <c>Render</c> only when the template has no progress body panel.
    /// </remarks>
    protected override void RenderIndicatorBar(DrawingContext context)
    {
        var deflateValue = 0d;
        var range        = 0d;
        if (Orientation == Orientation.Horizontal)
        {
            range = _grooveRect.Width;
        }
        else
        {
            range = _grooveRect.Height;
        }

        deflateValue = range * (1 - IndicatorRatio);
        DrawIndicatorBar(context, deflateValue, StrokeBrush);

        // A set success threshold paints the part of the bar up to the threshold with its own brush.
        if (!double.IsNaN(SuccessThreshold))
        {
            var successThreshold             = Math.Clamp(SuccessThreshold, Minimum, Maximum);
            var successThresholdDeflateValue = range * (1 - CalculateProgressRatio(successThreshold));
            DrawIndicatorBar(context, successThresholdDeflateValue, SuccessStrokeBrush);
        }
    }

    private void DrawIndicatorBar(
        DrawingContext context,
        double deflateValue,
        IBrush? fallbackBrush)
    {
        Rect indicatorRect = default;
        bool isEmpty       = false;
        if (Orientation == Orientation.Horizontal)
        {
            indicatorRect = _grooveRect.Deflate(new Thickness(0, 0, deflateValue, 0));
            if (StrokeLineCap == PenLineCap.Round)
            {
                isEmpty = indicatorRect.Width < indicatorRect.Height / 2;
            }
            else
            {
                isEmpty = indicatorRect.Width < 1.0;
            }
        }
        else
        {
            indicatorRect = _grooveRect.Deflate(new Thickness(0, 0, 0, deflateValue));
            if (StrokeLineCap == PenLineCap.Round)
            {
                isEmpty = indicatorRect.Height < indicatorRect.Width / 2;
            }
            else
            {
                isEmpty = indicatorRect.Height < 1.0;
            }
        }

        if (!isEmpty)
        {
            DrawProgressRectangle(context, fallbackBrush, indicatorRect);
        }
    }

    private void DrawProgressRectangle(
        DrawingContext context,
        IBrush? fallbackBrush,
        Rect rect)
    {
        var brush = fallbackBrush;
        if (brush is null)
        {
            return;
        }

        if (StrokeLineCap == PenLineCap.Round)
        {
            context.DrawPilledRect(brush, null, rect, Orientation);
        }
        else
        {
            context.FillRectangle(brush, rect);
        }
    }

    /// <summary>
    /// Updates <see cref="AbstractProgressBar.EffectiveStrokeThickness" /> with the line thickness preset of the
    /// effective size type, or with <see cref="AbstractProgressBar.IndicatorThickness" /> when that is not <see
    /// cref="double.NaN" />.
    /// </summary>
    /// <remarks>
    /// When the progress info is presented inside the bar, the thickness becomes the minimum height of a
    /// horizontal control or the width of the info area of a vertical one, ignoring the preset and the explicit
    /// thickness.
    /// </remarks>
    protected override void CalculateStrokeThickness()
    {
        double strokeThickness = QueryLineStrokeThickness(EffectiveSizeType);

        if (!double.IsNaN(IndicatorThickness))
        {
            strokeThickness = IndicatorThickness;
        }

        if (IsProgressInfoPresented && PercentPosition.IsInner)
        {
            if (Orientation == Orientation.Horizontal)
            {
                strokeThickness = MinHeight;
            }
            else
            {
                if (_extraInfoSize == Size.Infinity)
                {
                    _extraInfoSize = CalculateExtraInfoSize(FontSize);
                }

                if (PercentPosition.IsInner)
                {
                    strokeThickness = _extraInfoSize.Width;
                }
                else
                {
                    strokeThickness = MinWidth;
                }
            }
        }

        EffectiveStrokeThickness = strokeThickness;
    }

    /// <summary>
    /// Applies the template, then takes the <c>LineProgressPanel</c> named <c>PART_ProgressBody</c>, when the
    /// template has one, and invalidates its arrangement so that it uses the current geometry.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _progressBody = null;
        CalculateSizeTypeThresholdValue();
        CalculateMinBarThickness();
        base.OnApplyTemplate(e);
        _progressBody = e.NameScope.Find<LineProgressPanel>(ProgressBodyPart);
        _progressBody?.InvalidateArrange();
    }

    /// <summary>
    /// Maps an explicit size across the bar to a size type: <see cref="SizeType.Small" /> up to the small
    /// threshold, <see cref="SizeType.Middle" /> up to the middle threshold and <see cref="SizeType.Large" />
    /// above it.
    /// </summary>
    /// <param name="size">
    /// The height of a horizontal control or the width of a vertical one.
    /// </param>
    /// <remarks>
    /// The thresholds come from <see cref="CalculateSizeTypeThresholdValue" />, which runs when they are not
    /// initialized yet; they are different for info placed inside the bar and outside it.
    /// </remarks>
    protected override SizeType CalculateEffectiveSizeType(double size)
    {
        if (!_sizeTypeThresholdValuesInitialized)
        {
            CalculateSizeTypeThresholdValue();
        }

        var middleThresholdValue = _middleSizeTypeThresholdValue;
        var smallThresholdValue  = _smallSizeTypeThresholdValue;
        var sizeType             = SizeType.Middle;
        if (PercentPosition.IsInner)
        {
            if (size < smallThresholdValue.InnerStateValue ||
                MathUtils.AreClose(size, smallThresholdValue.InnerStateValue))
            {
                sizeType = SizeType.Small;
            }
            else if (size > smallThresholdValue.InnerStateValue && (size < middleThresholdValue.InnerStateValue ||
                                                                    MathUtils.AreClose(size,
                                                                        middleThresholdValue.InnerStateValue)))
            {
                sizeType = SizeType.Middle;
            }
            else
            {
                sizeType = SizeType.Large;
            }
        }
        else
        {
            if (size < smallThresholdValue.NormalStateValue ||
                MathUtils.AreClose(size, smallThresholdValue.NormalStateValue))
            {
                sizeType = SizeType.Small;
            }
            else if (size > smallThresholdValue.NormalStateValue && (size < middleThresholdValue.NormalStateValue ||
                                                                     MathUtils.AreClose(size,
                                                                         middleThresholdValue.NormalStateValue)))
            {
                sizeType = SizeType.Middle;
            }
            else
            {
                sizeType = SizeType.Large;
            }
        }

        return sizeType;
    }

    /// <summary>
    /// Recomputes the thresholds used by <see cref="CalculateEffectiveSizeType" /> for the current orientation.
    /// </summary>
    /// <remarks>
    /// A threshold for info outside the bar is the larger of the line thickness preset and the extent of the
    /// built-in info across the bar; for info inside the bar it is that extent plus the info padding on both
    /// sides. The small threshold uses the small font preset. <see cref="OnApplyTemplate" /> and
    /// <see cref="NotifyLineMetricsChanged" /> call it, and <see cref="CalculateEffectiveSizeType" /> calls it
    /// when the thresholds are not initialized yet.
    /// </remarks>
    protected void CalculateSizeTypeThresholdValue()
    {
        // The preset font sizes come from the control's effective Tokens through the token-query contract; a host
        // without a theme falls back to the control's own FontSize inside the query itself.
        var fontSize   = QueryPresetFontSize(small: false);
        var fontSizeSM = QueryPresetFontSize(small: true);
        var defaultExtraInfoSize = CalculateExtraInfoSize(fontSize);
        var smallExtraInfoSize   = CalculateExtraInfoSize(fontSizeSM);
        if (Orientation == Orientation.Horizontal)
        {
            _largeSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Large), defaultExtraInfoSize.Height),
                defaultExtraInfoSize.Height + LineProgressPadding * 2);
            _middleSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Middle), defaultExtraInfoSize.Height),
                defaultExtraInfoSize.Height + LineProgressPadding * 2);
            _smallSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Small), smallExtraInfoSize.Height),
                smallExtraInfoSize.Height + LineProgressPadding * 2);
        }
        else
        {
            _largeSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Large), defaultExtraInfoSize.Width),
                defaultExtraInfoSize.Width + LineProgressPadding * 2);
            _middleSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Middle), defaultExtraInfoSize.Width),
                defaultExtraInfoSize.Width + LineProgressPadding * 2);
            _smallSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Small), smallExtraInfoSize.Width),
                smallExtraInfoSize.Width + LineProgressPadding * 2);
        }

        _sizeTypeThresholdValuesInitialized = true;
    }

    /// <summary>
    /// Returns the rectangle of the groove in control coordinates.
    /// </summary>
    /// <param name="controlRect">
    /// The bounds of the control; the result uses the same coordinates.
    /// </param>
    /// <remarks>
    /// The rectangle is as long as the control along the bar and as thick as <see
    /// cref="AbstractProgressBar.EffectiveStrokeThickness" /> across it, centered. Info presented outside the bar
    /// reserves its size and the info margin at the start or at the end of the bar, or its size across the bar when
    /// centered.
    /// </remarks>
    protected override Rect GetProgressBarRect(Rect controlRect)
    {
        double deflateLeft     = 0;
        double deflateTop      = 0;
        double deflateRight    = 0;
        double deflateBottom   = 0;
        var    strokeThickness = EffectiveStrokeThickness;
        if (Orientation == Orientation.Horizontal)
        {
            if (IsProgressInfoPresented)
            {
                if (!PercentPosition.IsInner)
                {
                    var percentLabelWidth  = _extraInfoSize.Width;
                    var percentLabelHeight = _extraInfoSize.Height;
                    if (PercentPosition.Alignment == LinePercentAlignment.Start)
                    {
                        deflateLeft = percentLabelWidth + LineExtraInfoMargin;
                    }
                    else if (PercentPosition.Alignment == LinePercentAlignment.Center)
                    {
                        deflateBottom = percentLabelHeight;
                    }
                    else if (PercentPosition.Alignment == LinePercentAlignment.End)
                    {
                        deflateRight = percentLabelWidth + LineExtraInfoMargin;
                    }
                }
            }
        }
        else
        {
            if (IsProgressInfoPresented)
            {
                if (!PercentPosition.IsInner)
                {
                    var percentLabelWidth  = _extraInfoSize.Width;
                    var percentLabelHeight = _extraInfoSize.Height;
                    if (PercentPosition.Alignment == LinePercentAlignment.Start)
                    {
                        deflateTop = percentLabelHeight + LineExtraInfoMargin;
                    }
                    else if (PercentPosition.Alignment == LinePercentAlignment.Center)
                    {
                        deflateRight = percentLabelWidth;
                    }
                    else if (PercentPosition.Alignment == LinePercentAlignment.End)
                    {
                        deflateBottom = percentLabelHeight + LineExtraInfoMargin;
                    }
                }
            }
        }

        var deflatedControlRect =
            controlRect.Deflate(new Thickness(deflateLeft, deflateTop, deflateRight, deflateBottom));
        if (Orientation == Orientation.Horizontal)
        {
            return new Rect(new Point(deflatedControlRect.X, (deflatedControlRect.Height - strokeThickness) / 2),
                new Size(deflatedControlRect.Width, strokeThickness));
        }

        return new Rect(new Point((deflatedControlRect.Width - strokeThickness) / 2, deflatedControlRect.Y),
            new Size(strokeThickness, deflatedControlRect.Height));
    }

    /// <summary>
    /// Returns the rectangle of the progress info in control coordinates.
    /// </summary>
    /// <param name="controlRect">
    /// The bounds of the control; the result uses the same coordinates.
    /// </param>
    /// <remarks>
    /// Info inside the bar is placed at the start, the center or the end of the filled part of the groove. Info
    /// outside the bar is placed before the bar, after it, below a horizontal bar or beside a vertical one.
    /// </remarks>
    protected override Rect GetExtraInfoRect(Rect controlRect)
    {
        double offsetX      = 0;
        double offsetY      = 0;
        double targetWidth  = 0;
        double targetHeight = 0;
        if (IsProgressInfoPresented)
        {
            targetWidth  = _extraInfoSize.Width;
            targetHeight = _extraInfoSize.Height;
        }

        if (Orientation == Orientation.Horizontal)
        {
            if (IsProgressInfoPresented)
            {
                if (PercentPosition.IsInner)
                {
                    var grooveRect = GetProgressBarRect(controlRect);
                    offsetY = grooveRect.Y + (grooveRect.Height - targetHeight) / 2;
                    var range         = grooveRect.Width;
                    var deflateValue  = range * (1 - CalculateProgressRatio(Value));
                    var indicatorRect = grooveRect.Deflate(new Thickness(0, 0, deflateValue, 0));
                    if (PercentPosition.Alignment == LinePercentAlignment.Start)
                    {
                        offsetX = LineProgressPadding * 2;
                    }
                    else if (PercentPosition.Alignment == LinePercentAlignment.Center)
                    {
                        offsetX = (indicatorRect.Width - targetWidth) / 2;
                    }
                    else if (PercentPosition.Alignment == LinePercentAlignment.End)
                    {
                        offsetX = indicatorRect.Right - targetWidth - LineProgressPadding * 2;
                    }
                }
                else
                {
                    if (PercentPosition.Alignment == LinePercentAlignment.Start)
                    {
                        offsetX = 0;
                        offsetY = (controlRect.Height - targetHeight) / 2;
                    }
                    else if (PercentPosition.Alignment == LinePercentAlignment.Center)
                    {
                        offsetX = (controlRect.Width - targetWidth) / 2;
                        offsetY = controlRect.Bottom - targetHeight;
                    }
                    else if (PercentPosition.Alignment == LinePercentAlignment.End)
                    {
                        offsetX = controlRect.Right - targetWidth;
                        offsetY = (controlRect.Height - targetHeight) / 2;
                    }
                }
            }
        }
        else
        {
            if (PercentPosition.IsInner)
            {
                var grooveRect = GetProgressBarRect(controlRect);
                offsetX = grooveRect.X + (grooveRect.Width - targetWidth) / 2;
                var range         = grooveRect.Height;
                var deflateValue  = range * (1 - CalculateProgressRatio(Value));
                var indicatorRect = grooveRect.Deflate(new Thickness(0, 0, 0, deflateValue));
                if (PercentPosition.Alignment == LinePercentAlignment.Start)
                {
                    offsetY = LineExtraInfoMargin;
                }
                else if (PercentPosition.Alignment == LinePercentAlignment.Center)
                {
                    offsetY = (indicatorRect.Height - targetHeight) / 2;
                }
                else if (PercentPosition.Alignment == LinePercentAlignment.End)
                {
                    offsetY = indicatorRect.Bottom - targetHeight - LineExtraInfoMargin;
                }
            }
            else
            {
                if (PercentPosition.Alignment == LinePercentAlignment.Start)
                {
                    offsetX = (controlRect.Width - targetWidth) / 2;
                    offsetY = 0;
                }
                else if (PercentPosition.Alignment == LinePercentAlignment.Center)
                {
                    offsetX = controlRect.Right - targetWidth;
                    offsetY = (controlRect.Height - targetHeight) / 2;
                }
                else if (PercentPosition.Alignment == LinePercentAlignment.End)
                {
                    offsetX = (controlRect.Width - targetWidth) / 2;
                    offsetY = controlRect.Bottom - targetHeight;
                }
            }
        }

        return new Rect(new Point(offsetX, offsetY), _extraInfoSize);
    }

    /// <summary>
    /// Returns the size needed by the progress info, or an empty size when no progress info is presented.
    /// </summary>
    /// <param name="fontSize">
    /// The font size used to measure the percentage text.
    /// </param>
    /// <remarks>
    /// Custom content uses its measured size. Otherwise, info outside the bar needs only the status icon when
    /// the status is <see cref="ProgressStatus.Exception" /> or the percentage is 100, and takes the icon size
    /// in both directions; all other info takes the size of the formatted percentage text. For info inside a
    /// vertical bar the width and the height of the text size are swapped.
    /// </remarks>
    protected override Size CalculateExtraInfoSize(double fontSize)
    {
        if (IsProgressInfoPresented)
        {
            if (MeasureProgressInfoContent() is { } contentSize)
            {
                return contentSize;
            }

            if ((Status == ProgressStatus.Exception || MathUtils.AreClose(Percentage, 100)) &&
                !PercentPosition.IsInner)
            {
                // Only the status icon is shown, so the info area is a square of the icon size.
                var iconSize = LineInfoIconSize;
                return new Size(iconSize, iconSize);
            }

            var textSize = TextUtils.CalculateTextSize(string.Format(ProgressTextFormat, Percentage), fontSize, FontFamily);
            if (PercentPosition.IsInner)
            {
                if (Orientation == Orientation.Vertical)
                {
                    textSize = new Size(textSize.Height, textSize.Width);
                }
            }

            return textSize;
        }

        return default;
    }

    /// <summary>
    /// Recomputes the minimum bar thickness after the base member recomputed the stroke thickness for the new
    /// effective size type.
    /// </summary>
    /// <remarks>
    /// The base class calls it after the template is applied, and whenever the effective size type changes
    /// while the control is attached. Call the base member to keep the stroke thickness in step.
    /// </remarks>
    protected override void NotifyEffectSizeTypeChanged()
    {
        base.NotifyEffectSizeTypeChanged();
        CalculateMinBarThickness();
    }

    /// <summary>
    /// Recomputes the minimum bar thickness for the new orientation, after the base member updated the
    /// pseudo-classes.
    /// </summary>
    /// <remarks>
    /// The base class calls it after the template is applied and whenever the orientation changes.
    /// </remarks>
    protected override void NotifyOrientationChanged()
    {
        base.NotifyOrientationChanged();
        CalculateMinBarThickness();
    }

    /// <summary>
    /// Calls the base member, then recomputes the size type thresholds, the minimum bar thickness and the
    /// stroke thickness, and arranges the progress body panel again.
    /// </summary>
    /// <remarks>
    /// Call the base member first: it recalculates the size of the info area before the derived values are
    /// computed. <see cref="AbstractLineProgress.NotifyLineMetricsChanged" /> lists when the base class calls
    /// this member.
    /// </remarks>
    private protected override void NotifyLineMetricsChanged()
    {
        base.NotifyLineMetricsChanged();
        CalculateSizeTypeThresholdValue();
        CalculateMinBarThickness();
        CalculateStrokeThickness();
        _progressBody?.InvalidateArrange();
    }

    /// <summary>
    /// Updates the geometry that depends on changed properties after the base member handled the change: the
    /// stroke thickness, the minimum bar thickness, the info size, the percent label color and the arrangement
    /// of the progress panel.
    /// </summary>
    protected override void NotifyPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.NotifyPropertyChanged(change);
        if (change.Property == StrokeBrushProperty ||
            change.Property == ForegroundProperty ||
            change.Property == PercentPositionProperty)
        {
            SetupPercentLabelColor();
        }
        if (change.Property == PercentPositionProperty)
        {
            UpdatePseudoClasses();
        }

        // An explicit indicator thickness (a host Token such as Upload.TextListProgressLineHeight) replaces the
        // size preset; a later change must recompute the stroke, not wait for the next size or metrics change.
        if (change.Property == IndicatorThicknessProperty)
        {
            CalculateStrokeThickness();
        }

        if (change.Property == IsProgressInfoVisibleProperty ||
            change.Property == ProgressInfoContentProperty)
        {
            CalculateMinBarThickness();
        }

        if (change.Property == ValueProperty ||
            change.Property == MinimumProperty ||
            change.Property == MaximumProperty ||
            change.Property == SuccessThresholdProperty ||
            change.Property == OrientationProperty ||
            change.Property == EffectiveStrokeThicknessProperty ||
            change.Property == StrokeLineCapProperty ||
            change.Property == IsProgressInfoVisibleProperty ||
            change.Property == ProgressInfoContentProperty ||
            change.Property == PercentPositionProperty)
        {
            _progressBody?.InvalidateArrange();
        }

        if ((change.Property == ProgressTextFormatProperty || change.Property == FontSizeProperty) &&
            _progressBody is not null && this.IsAttachedToVisualTree())
        {
            InvalidateExtraInfoSize();
        }

        if (change.Property == IndicatorRatioProperty)
        {
            // When only the ratio changes (a theme transition), just the fill part is arranged again; the measurement
            // and the arrangement of the panel and of its host stay valid.
            _progressBody?.SyncTrackToIndicatorRatio();
        }
    }

    /// <summary>
    /// Recomputes the size of the info area and the minimum bar thickness, then invalidates the measurement and
    /// the arrangement of the control.
    /// </summary>
    /// <remarks>
    /// Call it when an input of the info size that only a derived class knows has changed. Changes of
    /// <see cref="AbstractProgressBar.ProgressTextFormat" /> and <see cref="TemplatedControl.FontSize" /> are already
    /// handled by this class.
    /// </remarks>
    protected void InvalidateExtraInfoSize()
    {
        _extraInfoSize = CalculateExtraInfoSize(FontSize);
        CalculateMinBarThickness();
        InvalidateMeasure();
        InvalidateArrange();
    }

    /// <summary>
    /// Calls the base member, then recomputes the minimum bar thickness and arranges the progress body panel
    /// again when the size of the info area changed.
    /// </summary>
    /// <remarks>
    /// Custom info content can change its size after it was set. The panel arranges by the cached info area, so
    /// it has to be arranged again once that size changed.
    /// </remarks>
    private protected override void RefreshProgressInfoContentSize()
    {
        var previous = _extraInfoSize;
        base.RefreshProgressInfoContentSize();
        if (previous != _extraInfoSize)
        {
            CalculateMinBarThickness();
            _progressBody?.InvalidateArrange();
        }
    }

    // Runtime text geometry is a theme-level fallback (projected to MinWidth / MinHeight by the control theme), not a
    // replacement for the caller's constraints.
    private void CalculateMinBarThickness()
    {
        var thickness     = LineProgressPadding * 2;
        var extraInfoSize = CalculateExtraInfoSize(FontSize);
        if (Orientation == Orientation.Horizontal)
        {
            thickness += CalculateInfoLineHeight(extraInfoSize, FontSize, PercentPosition.IsInner);
            FallbackMinHeight = thickness;
            FallbackMinWidth  = extraInfoSize.Width;
        }
        else
        {
            thickness += extraInfoSize.Width;
            FallbackMinWidth  = thickness;
            FallbackMinHeight = extraInfoSize.Height;
        }
    }

    private protected void SetInnerLabelColorCandidates(Color? label, Color? solid)
    {
        if (_innerLabelColors != (label, solid))
        {
            _innerLabelColors = (label, solid);
            SetupPercentLabelColor();
        }
    }

    private void SetupPercentLabelColor()
    {
        if (!PercentPosition.IsInner)
        {
            SetPercentageLabelColorIfChanged(Foreground);
        }
        else
        {
            if (_innerLabelColors is ({ } colorTextLabel, { } colorTextLightSolid))
            {
                // Pick the candidate that reads best against the brush under the label: the groove at value zero,
                // the indicator otherwise. Only solid brushes are supported; with a gradient brush the color is not
                // changed.
                if (MathUtils.AreClose(Value, 0))
                {
                    if (GrooveBrush is ISolidColorBrush grooveBrush)
                    {
                        var mostReadable = SelectMostReadable(grooveBrush.Color, colorTextLabel, colorTextLightSolid);
                        SetPercentageLabelColorIfChanged(mostReadable);
                    }
                }
                else
                {
                    if (StrokeBrush is ISolidColorBrush solidColorBrush)
                    {
                        var mostReadable = SelectMostReadable(solidColorBrush.Color, colorTextLabel, colorTextLightSolid);
                        SetPercentageLabelColorIfChanged(mostReadable);
                    }
                }
            }
        }
    }

    private static Color SelectMostReadable(Color baseColor, Color first, Color second)
    {
        return ColorUtils.Readability(baseColor, first) >= ColorUtils.Readability(baseColor, second)
            ? first
            : second;
    }

    private void SetPercentageLabelColorIfChanged(IBrush? brush)
    {
        if (!Equals(PercentageLabelColor, brush))
        {
            SetCurrentValue(PercentageLabelColorProperty, brush);
        }
    }

    private void SetPercentageLabelColorIfChanged(Color color)
    {
        if (PercentageLabelColor is ISolidColorBrush solidColorBrush &&
            solidColorBrush.Color == color)
        {
            return;
        }

        SetCurrentValue(PercentageLabelColorProperty, new SolidColorBrush(color));
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ProgressBarPseudoClass.PercentLabelInner, PercentPosition.IsInner);
        PseudoClasses.Set(ProgressBarPseudoClass.PercentLabelInnerStart, PercentPosition.IsInner && PercentPosition.Alignment == LinePercentAlignment.Start);
        PseudoClasses.Set(ProgressBarPseudoClass.PercentLabelInnerCenter, PercentPosition.IsInner && PercentPosition.Alignment == LinePercentAlignment.Center);
        PseudoClasses.Set(ProgressBarPseudoClass.PercentLabelInnerEnd, PercentPosition.IsInner && PercentPosition.Alignment == LinePercentAlignment.End);
    }

    /// <summary>
    /// Refreshes the pseudo-classes that reflect <see cref="PercentPosition" /> once the control is attached.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePseudoClasses();
    }

    internal Rect GetLineProgressBarRect(Size size)
    {
        _grooveRect = GetProgressBarRect(new Rect(default, size));
        return _grooveRect;
    }

    internal Rect GetLineTrackRect(Rect grooveRect, double value)
    {
        return GetLineTrackRectByRatio(grooveRect, CalculateProgressRatio(value));
    }

    internal Rect GetLineTrackRectByRatio(Rect grooveRect, double ratio)
    {
        var range = Orientation == Orientation.Horizontal ? grooveRect.Width : grooveRect.Height;
        var deflateValue = range * (1 - ratio);
        return Orientation == Orientation.Horizontal
            ? grooveRect.Deflate(new Thickness(0, 0, deflateValue, 0))
            : grooveRect.Deflate(new Thickness(0, 0, 0, deflateValue));
    }

    internal Rect GetLineProgressIndicatorRect(Size size)
    {
        return IsProgressInfoPresented
            ? GetExtraInfoRect(new Rect(default, size))
            : default;
    }

    internal bool IsLineTrackVisible(Rect rect)
    {
        return Orientation == Orientation.Horizontal
            ? StrokeLineCap == PenLineCap.Round ? rect.Width >= rect.Height / 2 : rect.Width >= 1
            : StrokeLineCap == PenLineCap.Round ? rect.Height >= rect.Width / 2 : rect.Height >= 1;
    }
}
