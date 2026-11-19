using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

public abstract class AbstractGeneralStepsProgressBar : AbstractLineProgress
{
    protected const double LARGE_CHUNK_WIDTH = 14;
    protected const double MIDDLE_CHUNK_WIDTH = 6;
    protected const double SMALL_CHUNK_WIDTH = 2;
    protected const double DEFAULT_CHUNK_SPACE = 2;

    #region 公共属性定义

    public static readonly StyledProperty<LinePercentAlignment> PercentPositionProperty =
        AvaloniaProperty.Register<AbstractGeneralStepsProgressBar, LinePercentAlignment>(nameof(PercentPosition),
            LinePercentAlignment.End);

    public static readonly StyledProperty<List<IBrush>?> StepsStrokeBrushProperty =
        AvaloniaProperty.Register<AbstractGeneralStepsProgressBar, List<IBrush>?>(nameof(StepsStrokeBrush));

    public static readonly StyledProperty<int> StepsProperty =
        AvaloniaProperty.Register<AbstractGeneralStepsProgressBar, int>(nameof(Steps), 1, coerce: (o, v) => Math.Max(v, 1));

    public static readonly StyledProperty<double> ChunkWidthProperty =
        AvaloniaProperty.Register<AbstractGeneralStepsProgressBar, double>(nameof(ChunkWidth), double.NaN,
            coerce: (o, v) => Math.Max(v, 1));

    public static readonly StyledProperty<double> ChunkHeightProperty =
        AvaloniaProperty.Register<AbstractGeneralStepsProgressBar, double>(nameof(ChunkHeight), double.NaN,
            coerce: (o, v) => Math.Max(v, 1));

    public LinePercentAlignment PercentPosition
    {
        get => GetValue(PercentPositionProperty);
        set => SetValue(PercentPositionProperty, value);
    }

    public List<IBrush>? StepsStrokeBrush
    {
        get => GetValue(StepsStrokeBrushProperty);
        set => SetValue(StepsStrokeBrushProperty, value);
    }

    public int Steps
    {
        get => GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }

    public double ChunkWidth
    {
        get => GetValue(ChunkWidthProperty);
        set => SetValue(ChunkWidthProperty, value);
    }

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

    private protected override bool HasTemplateProgressVisuals => _progressBody is not null;

    static AbstractGeneralStepsProgressBar()
    {
        AffectsMeasure<AbstractGeneralStepsProgressBar>(StepsProperty, ChunkWidthProperty, ChunkHeightProperty);
        AffectsArrange<AbstractGeneralStepsProgressBar>(PercentPositionProperty);
        AffectsRender<AbstractGeneralStepsProgressBar>(StepsStrokeBrushProperty);
    }

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

    protected override void CalculateStrokeThickness()
    {
        // 不改变高度
        var strokeThickness = QueryLineStrokeThickness(SizeType.Large);
        if (!double.IsNaN(ChunkHeight))
        {
            strokeThickness = ChunkHeight;
        }

        EffectiveStrokeThickness = strokeThickness;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _progressBody = null;
        CalculateSizeTypeThresholdValue();
        CalculateMinBarThickness();
        base.OnApplyTemplate(e);
        _progressBody = e.NameScope.Find<StepsProgressPanel>(ProgressBodyPart);
        _progressBody?.SynchronizeTracks();
    }

    protected override void NotifyProgressInfoContentChanged()
    {
        base.NotifyProgressInfoContentChanged();
        CalculateMinBarThickness();
    }

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
            thickness += extraInfoSize.Height;
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

    protected override void NotifyEffectSizeTypeChanged()
    {
        base.NotifyEffectSizeTypeChanged();
        CalculateMinBarThickness();
        _progressBody?.InvalidateMeasure();
        _progressBody?.InvalidateArrange();
    }

    protected override void NotifyOrientationChanged()
    {
        base.NotifyOrientationChanged();
        CalculateMinBarThickness();
    }

    private protected override void NotifyLineMetricsChanged()
    {
        base.NotifyLineMetricsChanged();
        CalculateSizeTypeThresholdValue();
        CalculateMinBarThickness();
        _progressBody?.InvalidateMeasure();
        _progressBody?.InvalidateArrange();
    }

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
