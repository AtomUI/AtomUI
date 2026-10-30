using AtomUI.Icons.AntDesign;
using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using ProgressPath = Avalonia.Controls.Shapes.Path;

namespace AtomUI.Controls.Commons;

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

    private PenLineCap _shapeStrokeLineCap;

    internal PenLineCap ShapeStrokeLineCap
    {
        get => _shapeStrokeLineCap;
        private set => SetAndRaise(ShapeStrokeLineCapProperty, ref _shapeStrokeLineCap, value);
    }

    #endregion

    private ProgressPath? _progressRail;
    private ProgressPath? _progressTrack;
    private ProgressPath? _progressSuccess;

    private protected override bool HasTemplateProgressVisuals =>
        _progressRail is not null && _progressTrack is not null && _progressSuccess is not null;

    static AbstractCircleProgress()
    {
        AffectsMeasure<AbstractCircleProgress>(StepCountProperty,
            StepGapProperty);
    }

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

    protected override void NotifyHandleExtraInfoVisibility()
    {
        var circleSize = CalculateCircleSize();
        CalculateStrokeThickness(circleSize);
        var extraInfoSize = circleSize - StrokeThickness - 1; // 写死一个像素的 padding 吧
        var extraInfo     = TextUtils.CalculateTextSize(string.Format(ProgressTextFormat, 100), FontSize, FontFamily);

        // 这三个是不可能同时满足的
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

        if (ExceptionCompletedIconPresenter is not null)
        {
            var exceptionIconWidth  = ExceptionCompletedIconPresenter.Width;
            var exceptionIconHeight = ExceptionCompletedIconPresenter.Height;
            if (exceptionIconWidth > extraInfoSize || exceptionIconHeight > extraInfoSize)
            {
                IsStatusIconVisible = false;
            }
            else
            {
                IsStatusIconVisible = true;
            }
        }

        if (SuccessCompletedIconPresenter is not null)
        {
            var successIconWidth  = SuccessCompletedIconPresenter.Width;
            var successIconHeight = SuccessCompletedIconPresenter.Height;
            if (successIconWidth > extraInfoSize || successIconHeight > extraInfoSize)
            {
                IsStatusIconVisible = false;
            }
            else
            {
                IsStatusIconVisible = true;
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
                var circleSize = CalculateCircleSize();
                CalculateStrokeThickness(circleSize);
                SetupExtraInfoFontSize(circleSize);
                SetupExtraInfoIconSize(circleSize);
            }
        }

        if (change.Property == ValueProperty ||
            change.Property == MinimumProperty ||
            change.Property == MaximumProperty ||
            change.Property == IndicatorAngleProperty ||
            change.Property == SuccessThresholdProperty ||
            change.Property == StepCountProperty ||
            change.Property == StepGapProperty ||
            change.Property == StrokeThicknessProperty ||
            change.Property == StrokeLineCapProperty)
        {
            ShapeStrokeLineCap = StepCount > 0 && StepGap > 0
                ? PenLineCap.Flat
                : StrokeLineCap;
            RefreshTemplateProgressVisuals();
        }
    }

    private void SetupExtraInfoFontSize(double circleSize)
    {
        var fontSize = circleSize * 0.15 + 6;
        if (fontSize < CircleMinimumTextFontSize)
        {
            fontSize = CircleMinimumTextFontSize;
        }

        FontSize = fontSize;
    }

    private void SetupExtraInfoIconSize(double circleSize)
    {
        var calculatedSize = Math.Max(circleSize / 4.5, CircleMinimumIconSize);
        if (ExceptionCompletedIconPresenter is not null)
        {
            ExceptionCompletedIconPresenter.Width  = calculatedSize;
            ExceptionCompletedIconPresenter.Height = calculatedSize;
        }

        if (SuccessCompletedIconPresenter is not null)
        {
            SuccessCompletedIconPresenter.Width  = calculatedSize;
            SuccessCompletedIconPresenter.Height = calculatedSize;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Rect progressIndicatorRect = default;
        if (IsProgressInfoVisible)
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
        }

        var arrangedSize = base.ArrangeOverride(finalSize);
        UpdateTemplateProgressVisuals(finalSize);
        ArrangeProgressIndicator(progressIndicatorRect);
        return arrangedSize;
    }

    private Size GetVisibleProgressIndicatorContentSize()
    {
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

        StrokeThickness = calculatedValue;
    }

    protected override void NotifyEffectSizeTypeChanged()
    {
        var circleSize = CalculateCircleSize();
        CalculateStrokeThickness(circleSize);
        SetupExtraInfoFontSize(circleSize);
        SetupExtraInfoIconSize(circleSize);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _progressRail = null;
        _progressTrack = null;
        _progressSuccess = null;
        base.OnApplyTemplate(e);
        _progressRail = e.NameScope.Find<ProgressPath>(ProgressRailPart);
        _progressTrack = e.NameScope.Find<ProgressPath>(ProgressTrackPart);
        _progressSuccess = e.NameScope.Find<ProgressPath>(ProgressSuccessPart);
        ShapeStrokeLineCap = StepCount > 0 && StepGap > 0
            ? PenLineCap.Flat
            : StrokeLineCap;
        RefreshTemplateProgressVisuals();
        if (ExceptionCompletedIcon == null)
        {
            SetValue(ExceptionCompletedIconProperty, new CloseOutlined(), BindingPriority.Template);
        }
        
        if (SuccessCompletedIcon == null)
        {
            SetValue(SuccessCompletedIconProperty, new CheckOutlined(), BindingPriority.Template);
        }
    }

    private protected abstract Geometry BuildRailGeometry(Rect grooveRect);
    private protected abstract Geometry BuildTrackGeometry(Rect grooveRect);
    private protected abstract Geometry BuildSuccessGeometry(Rect grooveRect);

    private protected void RefreshTemplateProgressVisuals()
    {
        UpdateTemplateProgressVisuals(Bounds.Size);
    }

    internal Rect GetCircleProgressPathRect(Size size)
    {
        var strokeThickness = double.IsFinite(StrokeThickness)
            ? Math.Max(0, StrokeThickness)
            : 0;
        var width = Math.Floor(Math.Max(0, size.Width - strokeThickness));
        var height = Math.Floor(Math.Max(0, size.Height - strokeThickness));
        return new Rect(
            (size.Width - width) / 2,
            (size.Height - height) / 2,
            width,
            height);
    }

    private void UpdateTemplateProgressVisuals(Size size)
    {
        if (!HasTemplateProgressVisuals || size.Width <= 0 || size.Height <= 0)
        {
            return;
        }

        var grooveRect = new Rect(default, GetCircleProgressPathRect(size).Size);

        _progressRail!.Data = BuildRailGeometry(grooveRect);
        _progressTrack!.Data = BuildTrackGeometry(grooveRect);
        _progressSuccess!.IsVisible = !double.IsNaN(SuccessThreshold);
        _progressSuccess.Data = _progressSuccess.IsVisible
            ? BuildSuccessGeometry(grooveRect)
            : null;
    }

}
