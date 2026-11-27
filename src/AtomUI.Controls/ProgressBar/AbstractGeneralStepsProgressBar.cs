using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class for line progress bars that divide the bar into a fixed number of equal steps and fill whole
/// steps according to the value.
/// </summary>
/// <remarks>
/// A template that has a <c>StepsProgressPanel</c> named <c>ProgressBodyPart</c> owns the step visuals; then
/// <see cref="RenderGroove" /> and <see cref="RenderIndicatorBar" /> are not called. Without such a panel the
/// control draws the steps itself.
/// </remarks>
public abstract class AbstractGeneralStepsProgressBar : AbstractLineProgress
{
    /// <summary>
    /// Length of one step, in DIPs, when <see cref="ChunkWidth" /> is <see cref="double.NaN" /> and the effective
    /// size type is <see cref="SizeType.Large" />.
    /// </summary>
    protected const double LARGE_CHUNK_WIDTH = 14;
    /// <summary>
    /// Length of one step, in DIPs, when <see cref="ChunkWidth" /> is <see cref="double.NaN" /> and the effective
    /// size type is <see cref="SizeType.Middle" />.
    /// </summary>
    protected const double MIDDLE_CHUNK_WIDTH = 6;
    /// <summary>
    /// Length of one step, in DIPs, when <see cref="ChunkWidth" /> is <see cref="double.NaN" /> and the effective
    /// size type is <see cref="SizeType.Small" />.
    /// </summary>
    protected const double SMALL_CHUNK_WIDTH = 2;
    /// <summary>
    /// Gap between adjacent steps, in DIPs, used until a derived control supplies another gap through
    /// <see cref="SetResourceStepSize" />.
    /// </summary>
    protected const double DEFAULT_CHUNK_SPACE = 2;

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="PercentPosition" /> property.
    /// </summary>
    public static readonly StyledProperty<LinePercentAlignment> PercentPositionProperty =
        AvaloniaProperty.Register<AbstractGeneralStepsProgressBar, LinePercentAlignment>(nameof(PercentPosition),
            LinePercentAlignment.End);

    /// <summary>
    /// Identifies the <see cref="StepsStrokeBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<List<IBrush>?> StepsStrokeBrushProperty =
        AvaloniaProperty.Register<AbstractGeneralStepsProgressBar, List<IBrush>?>(nameof(StepsStrokeBrush));

    /// <summary>
    /// Identifies the <see cref="Steps" /> property.
    /// </summary>
    public static readonly StyledProperty<int> StepsProperty =
        AvaloniaProperty.Register<AbstractGeneralStepsProgressBar, int>(nameof(Steps), 1, coerce: (o, v) => Math.Max(v, 1));

    /// <summary>
    /// Identifies the <see cref="ChunkWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ChunkWidthProperty =
        AvaloniaProperty.Register<AbstractGeneralStepsProgressBar, double>(nameof(ChunkWidth), double.NaN,
            coerce: (o, v) => Math.Max(v, 1));

    /// <summary>
    /// Identifies the <see cref="ChunkHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ChunkHeightProperty =
        AvaloniaProperty.Register<AbstractGeneralStepsProgressBar, double>(nameof(ChunkHeight), double.NaN,
            coerce: (o, v) => Math.Max(v, 1));

    /// <summary>
    /// Where the progress info sits relative to the step row: before it, after it, or centered below a horizontal
    /// row and beside a vertical one.
    /// </summary>
    /// <remarks>
    /// Takes effect only while progress info is presented. It is read when the control measures, when it computes
    /// the rectangles of the step row and of the info area, and by the default theme to align the template's
    /// <c>IconPresenter</c>.
    /// The property of the same name on <see cref="AbstractGeneralProgressBar" /> has another type: this one
    /// takes a <see cref="LinePercentAlignment" />.
    /// </remarks>
    public LinePercentAlignment PercentPosition
    {
        get => GetValue(PercentPositionProperty);
        set => SetValue(PercentPositionProperty, value);
    }

    /// <summary>
    /// Brushes of the filled steps, one per step counted from the start of the bar.
    /// </summary>
    /// <remarks>
    /// A filled step whose index is outside the list, and every filled step while the list is
    /// <see langword="null" />, uses <see cref="AbstractProgressBar.StrokeBrush" />.
    /// </remarks>
    public List<IBrush>? StepsStrokeBrush
    {
        get => GetValue(StepsStrokeBrushProperty);
        set => SetValue(StepsStrokeBrushProperty, value);
    }

    /// <summary>
    /// Number of equal steps the bar is divided into. Values below 1 are coerced to 1.
    /// </summary>
    /// <remarks>
    /// The filled step count is this value times <see cref="AbstractProgressBar.Percentage" /> divided by 100, rounded.
    /// </remarks>
    public int Steps
    {
        get => GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }

    /// <summary>
    /// Length of one step along the bar, in DIPs.
    /// </summary>
    /// <remarks>
    /// <see cref="double.NaN" /> (the default) selects the preset of the effective size type: 14, 6 or 2 for the
    /// large, middle or small size type. Values below 1 are coerced to 1.
    /// The result is never smaller than the minimum step length a derived control supplies through
    /// <see cref="SetResourceStepSize" />.
    /// </remarks>
    public double ChunkWidth
    {
        get => GetValue(ChunkWidthProperty);
        set => SetValue(ChunkWidthProperty, value);
    }

    /// <summary>
    /// Thickness of the steps across the bar, in DIPs.
    /// </summary>
    /// <remarks>
    /// <see cref="double.NaN" /> (the default) uses the line stroke thickness preset of the large size type.
    /// Values below 1 are coerced to 1.
    /// Setting this property or <see cref="AbstractProgressBar.IndicatorThickness" /> assigns the new value to the
    /// other one.
    /// </remarks>
    public double ChunkHeight
    {
        get => GetValue(ChunkHeightProperty);
        set => SetValue(ChunkHeightProperty, value);
    }

    #endregion

    private (double Gap, double MinimumWidth) _resourceStepSize = (DEFAULT_CHUNK_SPACE, 0);

    internal double ChunkSpace => _resourceStepSize.Gap;

    private protected bool SetResourceStepSize(double gap, double minimumWidth)
    {
        var previous = _resourceStepSize;
        _resourceStepSize = (gap, minimumWidth);
        return _resourceStepSize != previous;
    }

    #region 内部协作 API

    internal double GetChunkWidth()
    {
        return Math.Max(GetPresetChunkWidth(), _resourceStepSize.MinimumWidth);
    }

    private double GetPresetChunkWidth()
    {
        var chunkWidth = 0d;
        if (!double.IsNaN(ChunkWidth))
        {
            chunkWidth = ChunkWidth;
        }
        else
        {
            if (EffectiveSizeType == SizeType.Large)
            {
                chunkWidth = LARGE_CHUNK_WIDTH;
            }
            else if (EffectiveSizeType == SizeType.Middle)
            {
                chunkWidth = MIDDLE_CHUNK_WIDTH;
            }
            else
            {
                chunkWidth = SMALL_CHUNK_WIDTH;
            }
        }

        return chunkWidth;
    }

    internal double GetChunkHeight()
    {
        var chunkHeight = 0d;
        if (!double.IsNaN(ChunkHeight))
        {
            chunkHeight = ChunkHeight;
        }
        else
        {
            chunkHeight = EffectiveStrokeThickness;
        }

        return chunkHeight;
    }

    internal Rect GetStepsProgressBarRect(Size size)
    {
        return GetProgressBarRect(new Rect(default, size));
    }

    internal Rect GetStepsProgressIndicatorRect(Size size)
    {
        return GetExtraInfoRect(new Rect(default, size));
    }

    internal IBrush? GetStepBrush(int index)
    {
        if (StepsStrokeBrush is not null && index >= 0 && index < StepsStrokeBrush.Count)
        {
            return StepsStrokeBrush[index];
        }

        return StrokeBrush;
    }

    #endregion

    private StepsProgressPanel? _progressBody;

    /// <summary>
    /// Whether the applied template supplied the <c>PART_ProgressBody</c> panel, which then draws the steps
    /// of the bar itself.
    /// </summary>
    private protected override bool HasTemplateProgressVisuals => _progressBody is not null;

    static AbstractGeneralStepsProgressBar()
    {
        AffectsMeasure<AbstractGeneralStepsProgressBar>(StepsProperty, ChunkWidthProperty, ChunkHeightProperty);
        AffectsArrange<AbstractGeneralStepsProgressBar>(PercentPositionProperty);
        AffectsRender<AbstractGeneralStepsProgressBar>(StepsStrokeBrushProperty);
    }

    /// <summary>
    /// Stores the step row rectangle computed from the control bounds as the groove for
    /// <see cref="RenderIndicatorBar" />, and fills one rectangle per step with <c>GrooveBrush</c>.
    /// </summary>
    /// <remarks>
    /// The base class calls it from <c>Render</c> only when the template has no step panel.
    /// </remarks>
    protected override void RenderGroove(DrawingContext context)
    {
        _grooveRect = GetProgressBarRect(new Rect(new Point(0, 0), Bounds.Size));

        if (Orientation == Orientation.Horizontal)
        {
            var chunkWidth  = GetChunkWidth();
            var chunkHeight = GetChunkHeight();
            var offsetX     = _grooveRect.X;
            var offsetY     = _grooveRect.Y;
            for (var i = 0; i < Steps; ++i)
            {
                var chunkRect = new Rect(offsetX, offsetY, chunkWidth, chunkHeight);
                context.FillRectangle(GrooveBrush!, chunkRect);
                offsetX += chunkWidth + ChunkSpace;
            }
        }
        else
        {
            var chunkWidth  = GetChunkHeight();
            var chunkHeight = GetChunkWidth();
            var offsetX     = _grooveRect.X;
            var offsetY     = _grooveRect.Y;
            for (var i = 0; i < Steps; ++i)
            {
                var chunkRect = new Rect(offsetX, offsetY, chunkWidth, chunkHeight);
                context.FillRectangle(GrooveBrush!, chunkRect);
                offsetY += chunkHeight + ChunkSpace;
            }
        }
    }

    private IBrush GetChunkBrush(int i)
    {
        if (StepsStrokeBrush is not null)
        {
            if (i >= 0 && i < StepsStrokeBrush.Count)
            {
                return StepsStrokeBrush[i];
            }
        }

        return StrokeBrush!;
    }

    /// <summary>
    /// Paints the steps that the value fills, counted from the start, on the row stored by
    /// <see cref="RenderGroove" />. Each step takes its brush from <see cref="StepsStrokeBrush" /> or falls back to
    /// <see cref="AbstractProgressBar.StrokeBrush" />.
    /// </summary>
    /// <remarks>
    /// The base class calls it from <c>Render</c> only when the template has no step panel.
    /// </remarks>
    protected override void RenderIndicatorBar(DrawingContext context)
    {
        var filledSteps = (int)Math.Round(Steps * Percentage / 100);

        if (Orientation == Orientation.Horizontal)
        {
            var chunkWidth  = GetChunkWidth();
            var chunkHeight = GetChunkHeight();
            var offsetX     = _grooveRect.X;
            var offsetY     = _grooveRect.Y;
            for (var i = 0; i < filledSteps; ++i)
            {
                var chunkRect = new Rect(offsetX, offsetY, chunkWidth, chunkHeight);
                context.FillRectangle(GetChunkBrush(i), chunkRect);
                offsetX += chunkWidth + ChunkSpace;
            }
        }
        else
        {
            var chunkWidth  = GetChunkHeight();
            var chunkHeight = GetChunkWidth();
            var offsetX     = _grooveRect.X;
            var offsetY     = _grooveRect.Y;
            for (var i = 0; i < filledSteps; ++i)
            {
                var chunkRect = new Rect(offsetX, offsetY, chunkWidth, chunkHeight);
                context.FillRectangle(GetChunkBrush(i), chunkRect);
                offsetY += chunkHeight + ChunkSpace;
            }
        }
    }

    /// <summary>
    /// Sets <see cref="AbstractProgressBar.EffectiveStrokeThickness" /> to <see cref="ChunkHeight" />, or to the
    /// line stroke thickness preset of the large size type when <see cref="ChunkHeight" /> is
    /// <see cref="double.NaN" />.
    /// </summary>
    /// <remarks>
    /// The step height and the row geometry read that value whenever <see cref="ChunkHeight" /> is
    /// <see cref="double.NaN" />, so an override must keep it up to date.
    /// </remarks>
    protected override void CalculateStrokeThickness()
    {
        // The step height does not follow the effective size type: the large preset is used for every size type.
        var strokeThickness = QueryLineStrokeThickness(SizeType.Large);
        if (!double.IsNaN(ChunkHeight))
        {
            strokeThickness = ChunkHeight;
        }

        EffectiveStrokeThickness = strokeThickness;
    }

    /// <summary>
    /// Applies the template, then takes the <c>StepsProgressPanel</c> named <c>ProgressBodyPart</c>, when the
    /// template has one, and makes its tracks match <see cref="Steps" />.
    /// </summary>
    /// <remarks>
    /// The size type thresholds and the fallback minimum bar extent are recomputed before the base member runs.
    /// Call the base member from an override; it resolves the panel.
    /// </remarks>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _progressBody = null;
        CalculateSizeTypeThresholdValue();
        CalculateMinBarThickness();
        base.OnApplyTemplate(e);
        _progressBody = e.NameScope.Find<StepsProgressPanel>(ProgressBodyPart);
        _progressBody?.SynchronizeTracks();
    }

    /// <summary>
    /// Recomputes the fallback minimum bar extent after the base member handled a change of the custom progress
    /// info content.
    /// </summary>
    /// <remarks>
    /// The base class calls it when custom progress info content is set or cleared. Call the base member first.
    /// </remarks>
    protected override void NotifyProgressInfoContentChanged()
    {
        base.NotifyProgressInfoContentChanged();
        CalculateMinBarThickness();
    }

    /// <summary>
    /// Calls the base member, then recomputes the fallback minimum bar extent when the size of the info area
    /// changed.
    /// </summary>
    private protected override void RefreshProgressInfoContentSize()
    {
        var previous = _extraInfoSize;
        base.RefreshProgressInfoContentSize();
        if (previous != _extraInfoSize)
        {
            CalculateMinBarThickness();
        }
    }

    // Runtime text geometry is a theme-level fallback (projected to MinWidth / MinHeight by the control theme), not a
    // replacement for the caller's constraints.
    private void CalculateMinBarThickness()
    {
        var thickness     = 0d;
        var extraInfoSize = CalculateExtraInfoSize(FontSize);
        if (Orientation == Orientation.Horizontal)
        {
            thickness += CalculateInfoLineHeight(extraInfoSize, FontSize, false);
            FallbackMinHeight = thickness;
            FallbackMinWidth  = 0d;
        }
        else
        {
            thickness += extraInfoSize.Width;
            FallbackMinWidth  = thickness;
            FallbackMinHeight = 0d;
        }
    }

    /// <summary>
    /// Recomputes, for the current orientation, the thresholds that <see cref="CalculateEffectiveSizeType" />
    /// compares the bar extent against: for each size type the larger of its line stroke thickness preset and the
    /// extent the progress info needs across the bar.
    /// </summary>
    /// <remarks>
    /// The large and middle thresholds measure the info at the default font size, the small threshold at the small
    /// font size. It runs when the template is applied, when line metrics change and when
    /// <see cref="CalculateEffectiveSizeType" /> finds the thresholds not initialized yet.
    /// </remarks>
    protected void CalculateSizeTypeThresholdValue()
    {
        // The preset font sizes come from the control's effective Tokens through the token-query contract.
        var fontSize   = QueryPresetFontSize(small: false);
        var fontSizeSM = QueryPresetFontSize(small: true);
        var defaultExtraInfoSize = CalculateExtraInfoSize(fontSize);
        var smallExtraInfoSize   = CalculateExtraInfoSize(fontSizeSM);
        if (Orientation == Orientation.Horizontal)
        {
            _largeSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Large), defaultExtraInfoSize.Height));
            _middleSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Middle), defaultExtraInfoSize.Height));
            _smallSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Small), smallExtraInfoSize.Height));
        }
        else
        {
            _largeSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Large), defaultExtraInfoSize.Width));
            _middleSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Middle), defaultExtraInfoSize.Width));
            _smallSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(QueryLineStrokeThickness(SizeType.Small), smallExtraInfoSize.Width));
        }

        _sizeTypeThresholdValuesInitialized = true;
    }

    /// <summary>
    /// Maps the extent of the control across the bar to a size type.
    /// </summary>
    /// <param name="size">
    /// The height of a horizontal control or the width of a vertical one.
    /// </param>
    /// <returns>
    /// <see cref="SizeType.Large" /> from the large threshold upward, <see cref="SizeType.Middle" /> from the middle
    /// threshold upward, otherwise <see cref="SizeType.Small" />.
    /// </returns>
    /// <remarks>
    /// The thresholds come from <see cref="CalculateSizeTypeThresholdValue" />.
    /// </remarks>
    protected override SizeType CalculateEffectiveSizeType(double size)
    {
        if (!_sizeTypeThresholdValuesInitialized)
        {
            CalculateSizeTypeThresholdValue();
        }

        var largeThresholdValue  = _largeSizeTypeThresholdValue;
        var middleThresholdValue = _middleSizeTypeThresholdValue;
        var sizeType             = SizeType.Middle;
        if (MathUtils.GreaterThanOrClose(size, largeThresholdValue.NormalStateValue))
        {
            sizeType = SizeType.Large;
        }
        else if (MathUtils.GreaterThanOrClose(size, middleThresholdValue.NormalStateValue))
        {
            sizeType = SizeType.Middle;
        }
        else
        {
            sizeType = SizeType.Small;
        }

        return sizeType;
    }

    /// <summary>
    /// Recomputes the fallback minimum bar extent and invalidates the layout of the step panel after the base member
    /// handled the size type change, because the preset step length depends on the size type.
    /// </summary>
    /// <remarks>
    /// The base class calls it after the template is applied and whenever the effective size type changes while the
    /// control is attached. Call the base member from an override.
    /// </remarks>
    protected override void NotifyEffectSizeTypeChanged()
    {
        base.NotifyEffectSizeTypeChanged();
        CalculateMinBarThickness();
        _progressBody?.InvalidateMeasure();
        _progressBody?.InvalidateArrange();
    }

    /// <summary>
    /// Recomputes the fallback minimum bar extent for the new orientation after the base member handled the change.
    /// </summary>
    /// <remarks>
    /// The base class calls it after the template is applied and whenever the orientation changes. Call the base
    /// member from an override.
    /// </remarks>
    protected override void NotifyOrientationChanged()
    {
        base.NotifyOrientationChanged();
        CalculateMinBarThickness();
    }

    /// <summary>
    /// Calls the base member, then recomputes the size type thresholds and the fallback minimum bar extent,
    /// and invalidates the measurement and the arrangement of the step panel.
    /// </summary>
    private protected override void NotifyLineMetricsChanged()
    {
        base.NotifyLineMetricsChanged();
        CalculateSizeTypeThresholdValue();
        CalculateMinBarThickness();
        _progressBody?.InvalidateMeasure();
        _progressBody?.InvalidateArrange();
    }

    /// <summary>
    /// Keeps <see cref="ChunkHeight" /> and <see cref="AbstractProgressBar.IndicatorThickness" /> in step after
    /// the base member handled the change: a change of either property assigns its new value to the other one.
    /// </summary>
    /// <remarks>
    /// The base class calls it for every property change on the control. Call the base member from an override.
    /// </remarks>
    protected override void NotifyPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.NotifyPropertyChanged(change);
        if (change.Property == ChunkHeightProperty)
        {
            IndicatorThickness = change.GetNewValue<double>();
        }
        else if (change.Property == IndicatorThicknessProperty)
        {
            ChunkHeight = change.GetNewValue<double>();
        }
    }

    /// <summary>
    /// Returns the size the step row needs along the bar (<see cref="Steps" /> step lengths and the gaps between
    /// them), enlarged by the progress info when it is visible.
    /// </summary>
    /// <remarks>
    /// The extent across the bar is raised to <c>MinHeight</c> for a horizontal control and to <c>MinWidth</c> for a
    /// vertical one. The row is not stretched to the available size.
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);
        double targetWidth  = 0;
        double targetHeight = 0;

        if (Orientation == Orientation.Horizontal)
        {
            var chunkWidth  = GetChunkWidth();
            var chunkHeight = GetChunkHeight();
            targetWidth = chunkWidth * Steps + ChunkSpace * (Steps - 1);
            if (IsProgressInfoVisible)
            {
                if (PercentPosition == LinePercentAlignment.Center)
                {
                    chunkHeight += _extraInfoSize.Height + LineExtraInfoMargin;
                }
                else
                {
                    targetWidth += _extraInfoSize.Width + LineExtraInfoMargin;
                }
            }

            targetHeight = Math.Max(chunkHeight, MinHeight);
        }
        else
        {
            var chunkWidth  = GetChunkHeight();
            var chunkHeight = GetChunkWidth();
            targetHeight = chunkHeight * Steps + ChunkSpace * (Steps - 1);
            if (IsProgressInfoVisible)
            {
                if (PercentPosition == LinePercentAlignment.Center)
                {
                    chunkWidth += _extraInfoSize.Width + LineExtraInfoMargin;
                }
                else
                {
                    targetHeight += _extraInfoSize.Height + LineExtraInfoMargin;
                }
            }

            targetWidth = Math.Max(chunkWidth, MinWidth);
        }

        return new Size(targetWidth, targetHeight);
    }

    /// <summary>
    /// Computes the rectangle of the step row inside the control rectangle.
    /// </summary>
    /// <param name="controlRect">
    /// The bounds of the control; the result uses the same coordinates.
    /// </param>
    /// <returns>
    /// A rectangle as long as the space left along the bar and as thick as
    /// <see cref="AbstractProgressBar.EffectiveStrokeThickness" /> across it, centered.
    /// </returns>
    /// <remarks>
    /// Presented progress info reserves its size and the info margin before the row or after it, or its size across
    /// the bar when centered.
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
                var percentLabelWidth  = _extraInfoSize.Width;
                var percentLabelHeight = _extraInfoSize.Height;
                if (PercentPosition == LinePercentAlignment.Start)
                {
                    deflateLeft = percentLabelWidth + LineExtraInfoMargin;
                }
                else if (PercentPosition == LinePercentAlignment.Center)
                {
                    deflateBottom = percentLabelHeight;
                }
                else if (PercentPosition == LinePercentAlignment.End)
                {
                    deflateRight = percentLabelWidth + LineExtraInfoMargin;
                }
            }
        }
        else
        {
            if (IsProgressInfoPresented)
            {
                var percentLabelWidth  = _extraInfoSize.Width;
                var percentLabelHeight = _extraInfoSize.Height;
                if (PercentPosition == LinePercentAlignment.Start)
                {
                    deflateTop = percentLabelHeight + LineExtraInfoMargin;
                }
                else if (PercentPosition == LinePercentAlignment.Center)
                {
                    deflateRight = percentLabelWidth;
                }
                else if (PercentPosition == LinePercentAlignment.End)
                {
                    deflateBottom = percentLabelHeight + LineExtraInfoMargin;
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
    /// Computes the rectangle of the progress info inside the control rectangle.
    /// </summary>
    /// <param name="controlRect">
    /// The bounds of the control; the result uses the same coordinates.
    /// </param>
    /// <returns>
    /// A rectangle of the progress info size, placed as <see cref="PercentPosition" /> selects: before the row or
    /// after it, centered across the bar, or below a horizontal row and beside a vertical one.
    /// </returns>
    /// <remarks>
    /// Without presented progress info the rectangle starts at the origin of <paramref name="controlRect" />.
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
                if (PercentPosition == LinePercentAlignment.Start)
                {
                    offsetX = 0;
                    offsetY = (controlRect.Height - targetHeight) / 2;
                }
                else if (PercentPosition == LinePercentAlignment.Center)
                {
                    offsetX = (controlRect.Width - targetWidth) / 2;
                    offsetY = controlRect.Bottom - targetHeight;
                }
                else if (PercentPosition == LinePercentAlignment.End)
                {
                    offsetX = controlRect.Right - targetWidth;
                    offsetY = (controlRect.Height - targetHeight) / 2;
                }
            }
        }
        else
        {
            if (IsProgressInfoPresented)
            {
                if (PercentPosition == LinePercentAlignment.Start)
                {
                    offsetX = (controlRect.Width - targetWidth) / 2;
                    offsetY = 0;
                }
                else if (PercentPosition == LinePercentAlignment.Center)
                {
                    offsetX = controlRect.Right - targetWidth;
                    offsetY = (controlRect.Height - targetHeight) / 2;
                }
                else if (PercentPosition == LinePercentAlignment.End)
                {
                    offsetX = (controlRect.Width - targetWidth) / 2;
                    offsetY = controlRect.Bottom - targetHeight;
                }
            }
        }

        return new Rect(new Point(offsetX, offsetY), _extraInfoSize);
    }
}
