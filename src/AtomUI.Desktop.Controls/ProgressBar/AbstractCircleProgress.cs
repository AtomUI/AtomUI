using AtomUI.Icons.AntDesign;
using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public abstract class AbstractCircleProgress : AbstractProgressBar
{
    // 默认的大小推荐，针对 SizeType
    protected const double LARGE_CIRCLE_SIZE = 120;
    protected const double MIDDLE_CIRCLE_SIZE = 90;
    protected const double SMALL_CIRCLE_SIZE = 60;
    protected const double CIRCLE_MIN_STROKE_THICKNESS = 3;

    #region 公共属性定义

    public static readonly StyledProperty<int> StepCountProperty =
        AvaloniaProperty.Register<AbstractCircleProgress, int>(nameof(StepCount), coerce: (o, v) => Math.Max(v, 0));

    public static readonly StyledProperty<double> StepGapProperty =
        AvaloniaProperty.Register<AbstractCircleProgress, double>(nameof(StepGap), 2, coerce: (o, v) => Math.Max(v, 0));

    public int StepCount
    {
        get => GetValue(StepCountProperty);
        set => SetValue(StepCountProperty, value);
    }

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

    #endregion

    internal Dictionary<SizeType, double> _sizeTypeThresholdValue;

    static AbstractCircleProgress()
    {
        AffectsMeasure<AbstractCircleProgress>(StepCountProperty,
            StepGapProperty);
    }

    public AbstractCircleProgress()
    {
        _sizeTypeThresholdValue = new Dictionary<SizeType, double>();
    }

    protected override SizeType CalculateEffectiveSizeType(double size)
    {
        var sizeType             = SizeType.Large;
        var largeThresholdValue  = _sizeTypeThresholdValue[SizeType.Large];
        var middleThresholdValue = _sizeTypeThresholdValue[SizeType.Middle];
        if (MathUtils.GreaterThanOrClose(size, largeThresholdValue))
        {
            sizeType = SizeType.Large;
        }
        else if (MathUtils.GreaterThanOrClose(size, middleThresholdValue))
        {
            sizeType = SizeType.Middle;
        }
        else
        {
            sizeType = SizeType.Small;
        }

        return sizeType;
    }

    private void CalculateSizeTypeThresholdValue()
    {
        _sizeTypeThresholdValue.Add(SizeType.Large, LARGE_CIRCLE_SIZE);
        _sizeTypeThresholdValue.Add(SizeType.Middle, MIDDLE_CIRCLE_SIZE);
        _sizeTypeThresholdValue.Add(SizeType.Small, SMALL_CIRCLE_SIZE);
    }

    // 是否考虑一个最小的值
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
        var sizeTypeDefaultValue = _sizeTypeThresholdValue[EffectiveSizeType];
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

    protected override void NotifyHandleExtraInfoVisibility()
    {
        // TODO 可能存在重复计算
        var circleSize = CalculateCircleSize();
        CalculateStrokeThickness();
        var extraInfoSize = circleSize - StrokeThickness - 1; // 写死一个像素的 padding 吧
        var extraInfo     = TextUtils.CalculateTextSize(string.Format(ProgressTextFormat, 100), FontSize, FontFamily);

        // 这三个是不可能同时满足的
        if (LayoutTransformLabel is not null)
        {
            if (extraInfo.Width > extraInfoSize || extraInfo.Height > extraInfoSize)
            {
                PercentLabelVisible = false;
            }
            else
            {
                PercentLabelVisible = true;
            }
        }

        if (ExceptionCompletedIconPresenter is not null)
        {
            var exceptionIconWidth  = ExceptionCompletedIconPresenter.Width;
            var exceptionIconHeight = ExceptionCompletedIconPresenter.Height;
            if (exceptionIconWidth > extraInfoSize || exceptionIconHeight > extraInfoSize)
            {
                StatusIconVisible = false;
            }
            else
            {
                StatusIconVisible = true;
            }
        }

        if (SuccessCompletedIconPresenter is not null)
        {
            var successIconWidth  = SuccessCompletedIconPresenter.Width;
            var successIconHeight = SuccessCompletedIconPresenter.Height;
            if (successIconWidth > extraInfoSize || successIconHeight > extraInfoSize)
            {
                StatusIconVisible = false;
            }
            else
            {
                StatusIconVisible = true;
            }
        }
    }

    protected override void NotifyPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.NotifyPropertyChanged(change);
        
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == WidthProperty || change.Property == HeightProperty)
            {
                CalculateStrokeThickness();
                SetupExtraInfoFontSize();
                SetupExtraInfoIconSize();
            }
        }
    }

    private void SetupExtraInfoFontSize()
    {
        var circleSize = CalculateCircleSize();
        var fontSize   = circleSize * 0.15 + 6;
        if (fontSize < CircleMinimumTextFontSize)
        {
            fontSize = CircleMinimumTextFontSize;
        }

        FontSize = fontSize;
    }

    private void SetupExtraInfoIconSize()
    {
        var circleSize     = CalculateCircleSize();
        var calculatedSize = Math.Max(circleSize / 4.5, CircleMinimumIconSize);
        ExceptionCompletedIconPresenter!.Width  = calculatedSize;
        ExceptionCompletedIconPresenter!.Height = calculatedSize;

        SuccessCompletedIconPresenter!.Width  = calculatedSize;
        SuccessCompletedIconPresenter!.Height = calculatedSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (IsShowProgressInfo)
        {
            var extraInfoRect = GetExtraInfoRect(new Rect(new Point(0, 0), finalSize));
            var extraInfoPos  = extraInfoRect.Position;
            if (LayoutTransformLabel is not null)
            {
                var labelSize = LayoutTransformLabel.DesiredSize;
                var offsetX   = (extraInfoRect.Width - labelSize.Width) / 2;
                var offsetY   = (extraInfoRect.Height - labelSize.Height) / 2;
                Canvas.SetLeft(LayoutTransformLabel, extraInfoPos.X + offsetX);
                Canvas.SetTop(LayoutTransformLabel, extraInfoPos.Y + offsetY);
            }

            if (SuccessCompletedIconPresenter is not null)
            {
                var size    = SuccessCompletedIconPresenter.DesiredSize;
                var offsetX = (extraInfoRect.Width - size.Width) / 2;
                var offsetY = (extraInfoRect.Height - size.Height) / 2;
                Canvas.SetLeft(SuccessCompletedIconPresenter, extraInfoPos.X + offsetX);
                Canvas.SetTop(SuccessCompletedIconPresenter, extraInfoPos.Y + offsetY);
            }

            if (ExceptionCompletedIconPresenter is not null)
            {
                var size    = ExceptionCompletedIconPresenter.DesiredSize;
                var offsetX = (extraInfoRect.Width - size.Width) / 2;
                var offsetY = (extraInfoRect.Height - size.Height) / 2;
                Canvas.SetLeft(ExceptionCompletedIconPresenter, extraInfoPos.X + offsetX);
                Canvas.SetTop(ExceptionCompletedIconPresenter, extraInfoPos.Y + offsetY);
            }
        }

        return base.ArrangeOverride(finalSize);
        ;
    }

    protected override Rect GetProgressBarRect(Rect controlRect)
    {
        return new Rect(new Point(0, 0), controlRect.Size);
    }

    protected override Rect GetExtraInfoRect(Rect controlRect)
    {
        return GetProgressBarRect(controlRect).Deflate(StrokeThickness);
    }

    protected override void CalculateStrokeThickness()
    {
        var circleSize      = CalculateCircleSize();
        var calculatedValue = MIDDLE_STROKE_THICKNESS / MIDDLE_CIRCLE_SIZE * circleSize;
        calculatedValue = Math.Max(calculatedValue, CIRCLE_MIN_STROKE_THICKNESS);
        if (!double.IsNaN(IndicatorThickness))
        {
            calculatedValue = Math.Max(IndicatorThickness, CIRCLE_MIN_STROKE_THICKNESS);
        }

        StrokeThickness = calculatedValue;
    }

    protected override void NotifyEffectSizeTypeChanged()
    {
        base.NotifyEffectSizeTypeChanged();
        SetupExtraInfoFontSize();
        SetupExtraInfoIconSize();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        CalculateSizeTypeThresholdValue();
        base.OnApplyTemplate(e);
        if (ExceptionCompletedIcon == null)
        {
            SetValue(ExceptionCompletedIconProperty, new CloseOutlined(), BindingPriority.Template);
        }
        
        if (SuccessCompletedIcon == null)
        {
            SetValue(SuccessCompletedIconProperty, new CheckOutlined(), BindingPriority.Template);
        }
    }
}