using AtomUI.Data;
using AtomUI.Theme.Styling;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class StepsProgressBar : AbstractLineProgress
{
    protected const double LARGE_CHUNK_WIDTH = 14;
    protected const double MIDDLE_CHUNK_WIDTH = 6;
    protected const double SMALL_CHUNK_WIDTH = 2;
    protected const double DEFAULT_CHUNK_SPACE = 2;

    #region 公共属性定义

    public static readonly StyledProperty<LinePercentAlignment> PercentPositionProperty =
        AvaloniaProperty.Register<StepsProgressBar, LinePercentAlignment>(nameof(PercentPosition),
            LinePercentAlignment.End);

    public static readonly StyledProperty<List<IBrush>?> StepsStrokeBrushProperty =
        AvaloniaProperty.Register<StepsProgressBar, List<IBrush>?>(nameof(StepsStrokeBrush));

    public static readonly StyledProperty<int> StepsProperty =
        AvaloniaProperty.Register<StepsProgressBar, int>(nameof(Steps), 1, coerce: (o, v) => Math.Max(v, 1));

    public static readonly StyledProperty<double> ChunkWidthProperty =
        AvaloniaProperty.Register<StepsProgressBar, double>(nameof(ChunkWidth), double.NaN,
            coerce: (o, v) => Math.Max(v, 1));

    public static readonly StyledProperty<double> ChunkHeightProperty =
        AvaloniaProperty.Register<StepsProgressBar, double>(nameof(ChunkHeight), double.NaN,
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

    static StepsProgressBar()
    {
        AffectsMeasure<StepsProgressBar>(StepsProperty, ChunkWidthProperty, ChunkHeightProperty);
        AffectsArrange<StepsProgressBar>(PercentPositionProperty);
        AffectsRender<StepsProgressBar>(StepsStrokeBrushProperty);
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
                offsetX += chunkWidth + DEFAULT_CHUNK_SPACE;
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
                offsetY += chunkHeight + DEFAULT_CHUNK_SPACE;
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
                offsetX += chunkWidth + DEFAULT_CHUNK_SPACE;
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
                offsetY += chunkHeight + DEFAULT_CHUNK_SPACE;
            }
        }
    }

    protected override void CalculateStrokeThickness()
    {
        // 不改变高度
        var strokeThickness = LARGE_STROKE_THICKNESS;
        if (!double.IsNaN(ChunkHeight))
        {
            strokeThickness = ChunkHeight;
        }

        StrokeThickness = strokeThickness;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        CalculateSizeTypeThresholdValue();
        CalculateMinBarThickness();
        base.OnApplyTemplate(e);
    }

    // 需要评估是否需要
    private void CalculateMinBarThickness()
    {
        var thickness     = 0d;
        var extraInfoSize = CalculateExtraInfoSize(FontSize);
        if (Orientation == Orientation.Horizontal)
        {
            thickness += extraInfoSize.Height;
            MinHeight =  thickness;
        }
        else
        {
            thickness += extraInfoSize.Width;
            MinWidth  =  thickness;
        }
    }

    protected void CalculateSizeTypeThresholdValue()
    {
        double fontSize   = default;
        double fontSizeSM = default;
        {
            if (TokenResourceUtils.FindTokenResource(this, SharedTokenKey.FontSize) is double value)
            {
                fontSize = value;
            }
        }

        {
            if (TokenResourceUtils.FindTokenResource(this, SharedTokenKey.FontSizeSM) is double value)
            {
                fontSizeSM = value;
            }
        }
        var defaultExtraInfoSize = CalculateExtraInfoSize(fontSize);
        var smallExtraInfoSize   = CalculateExtraInfoSize(fontSizeSM);
        if (Orientation == Orientation.Horizontal)
        {
            var largeSizeTypeThresholdValue = new SizeTypeThresholdValue
            {
                NormalStateValue = Math.Max(LARGE_STROKE_THICKNESS, defaultExtraInfoSize.Height)
            };
            _sizeTypeThresholdValue.Add(SizeType.Large, largeSizeTypeThresholdValue);
            var middleSizeTypeThresholdValue = new SizeTypeThresholdValue
            {
                NormalStateValue = Math.Max(MIDDLE_STROKE_THICKNESS, defaultExtraInfoSize.Height)
            };
            _sizeTypeThresholdValue.Add(SizeType.Middle, middleSizeTypeThresholdValue);

            var smallSizeTypeThresholdValue = new SizeTypeThresholdValue
            {
                NormalStateValue = Math.Max(SMALL_STROKE_THICKNESS, smallExtraInfoSize.Height)
            };
            _sizeTypeThresholdValue.Add(SizeType.Small, smallSizeTypeThresholdValue);
        }
        else
        {
            var largeSizeTypeThresholdValue = new SizeTypeThresholdValue
            {
                NormalStateValue = Math.Max(LARGE_STROKE_THICKNESS, defaultExtraInfoSize.Width)
            };
            _sizeTypeThresholdValue.Add(SizeType.Large, largeSizeTypeThresholdValue);
            var middleSizeTypeThresholdValue = new SizeTypeThresholdValue
            {
                NormalStateValue = Math.Max(MIDDLE_STROKE_THICKNESS, defaultExtraInfoSize.Width)
            };
            _sizeTypeThresholdValue.Add(SizeType.Middle, middleSizeTypeThresholdValue);

            var smallSizeTypeThresholdValue = new SizeTypeThresholdValue
            {
                NormalStateValue = Math.Max(SMALL_STROKE_THICKNESS, smallExtraInfoSize.Width)
            };
            _sizeTypeThresholdValue.Add(SizeType.Small, smallSizeTypeThresholdValue);
        }
    }

    protected override SizeType CalculateEffectiveSizeType(double size)
    {
        var largeThresholdValue  = _sizeTypeThresholdValue[SizeType.Large];
        var middleThresholdValue = _sizeTypeThresholdValue[SizeType.Middle];
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

        // 计算 chunk width
        if (double.IsNaN(ChunkWidth))
        {
            if (EffectiveSizeType == SizeType.Large)
            {
                ChunkWidth = LARGE_CHUNK_WIDTH;
            }
            else if (EffectiveSizeType == SizeType.Middle)
            {
                ChunkWidth = MIDDLE_CHUNK_WIDTH;
            }
            else
            {
                ChunkWidth = SMALL_CHUNK_WIDTH;
            }
        }
    }

    protected override void NotifyOrientationChanged()
    {
        base.NotifyOrientationChanged();
        CalculateMinBarThickness();
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
            targetWidth = chunkWidth * Steps + DEFAULT_CHUNK_SPACE * (Steps - 1);
            if (IsShowProgressInfo)
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
            targetHeight = chunkHeight * Steps + DEFAULT_CHUNK_SPACE * (Steps - 1);
            if (IsShowProgressInfo)
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

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (IsShowProgressInfo)
        {
            var extraInfoRect = GetExtraInfoRect(new Rect(new Point(0, 0), finalSize));
            if (LayoutTransformLabel is not null)
            {
                Canvas.SetTop(LayoutTransformLabel, extraInfoRect.Top);
                Canvas.SetLeft(LayoutTransformLabel, extraInfoRect.Left);
            }

            if (SuccessCompletedIconPresenter is not null)
            {
                Canvas.SetLeft(SuccessCompletedIconPresenter, extraInfoRect.Left);
                Canvas.SetTop(SuccessCompletedIconPresenter, extraInfoRect.Top);
            }

            if (ExceptionCompletedIconPresenter is not null)
            {
                Canvas.SetLeft(ExceptionCompletedIconPresenter, extraInfoRect.Left);
                Canvas.SetTop(ExceptionCompletedIconPresenter, extraInfoRect.Top);
            }
        }

        return base.ArrangeOverride(finalSize);
    }

    private double GetChunkWidth()
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

    private double GetChunkHeight()
    {
        var chunkHeight = 0d;
        if (!double.IsNaN(ChunkHeight))
        {
            chunkHeight = ChunkHeight;
        }
        else
        {
            chunkHeight = StrokeThickness;
        }

        return chunkHeight;
    }

    protected override Rect GetProgressBarRect(Rect controlRect)
    {
        double deflateLeft     = 0;
        double deflateTop      = 0;
        double deflateRight    = 0;
        double deflateBottom   = 0;
        var    strokeThickness = StrokeThickness;
        if (Orientation == Orientation.Horizontal)
        {
            if (IsShowProgressInfo)
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
            if (IsShowProgressInfo)
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
        if (IsShowProgressInfo)
        {
            targetWidth  = _extraInfoSize.Width;
            targetHeight = _extraInfoSize.Height;
        }

        if (Orientation == Orientation.Horizontal)
        {
            if (IsShowProgressInfo)
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
            if (IsShowProgressInfo)
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