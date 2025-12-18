using AtomUI.Data;
using AtomUI.Media;
using AtomUI.Theme.Styling;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public record struct PercentPosition
{
    public PercentPosition() {}
    public bool IsInner { get; set; } = false;
    public LinePercentAlignment Alignment { get; set; } = LinePercentAlignment.End;
}

[PseudoClasses(ProgressBarPseudoClass.Indeterminate, ProgressBarPseudoClass.Completed)]
public class ProgressBar : AbstractLineProgress
{
    #region 公共属性定义

    public static readonly StyledProperty<PercentPosition> PercentPositionProperty =
        AvaloniaProperty.Register<ProgressBar, PercentPosition>(nameof(PercentPosition), new PercentPosition());

    public PercentPosition PercentPosition
    {
        get => GetValue(PercentPositionProperty);
        set => SetValue(PercentPositionProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<IBrush?> ColorTextLabelProperty =
        AvaloniaProperty.Register<ProgressBar, IBrush?>(nameof(ColorTextLabel));

    internal static readonly StyledProperty<IBrush?> ColorTextLightSolidProperty =
        AvaloniaProperty.Register<ProgressBar, IBrush?>(nameof(ColorTextLightSolid));

    internal IBrush? ColorTextLabel
    {
        get => GetValue(ColorTextLabelProperty);
        set => SetValue(ColorTextLabelProperty, value);
    }

    internal IBrush? ColorTextLightSolid
    {
        get => GetValue(ColorTextLightSolidProperty);
        set => SetValue(ColorTextLightSolidProperty, value);
    }

    #endregion

    private IDisposable? _percentageLabelBindingDisposable;
    
    static ProgressBar()
    {
        AffectsMeasure<ProgressBar>(IndicatorThicknessProperty, PercentPositionProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);
        double targetWidth  = 0;
        double targetHeight = 0;
        if (Orientation == Orientation.Horizontal)
        {
            targetHeight = StrokeThickness;
            if (!PercentPosition.IsInner && IsShowProgressInfo)
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
            targetWidth = StrokeThickness;
            if (!PercentPosition.IsInner && IsShowProgressInfo)
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

    protected override void RenderGroove(DrawingContext context)
    {
        var controlRect = new Rect(new Point(0, 0), Bounds.Size);
        _grooveRect = GetProgressBarRect(controlRect);
        if (StrokeLineCap == PenLineCap.Round)
        {
            context.DrawPilledRect(GrooveBrush, null, _grooveRect, Orientation);
        }
        else
        {
            context.FillRectangle(GrooveBrush!, _grooveRect);
        }
    }

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

        deflateValue = range * (1 - _percentage / 100);
        DrawIndicatorBar(context, deflateValue, StrokeBrush!);

        // 绘制成功阈值
        if (!double.IsNaN(SuccessThreshold))
        {
            var successThreshold             = Math.Clamp(SuccessThreshold, Minimum, Maximum);
            var successThresholdDeflateValue = range * (1 - successThreshold / (Maximum - Minimum));
            DrawIndicatorBar(context, successThresholdDeflateValue, SuccessStrokeBrush!);
        }
    }

    protected void DrawIndicatorBar(DrawingContext context, double deflateValue, IBrush brush)
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
            if (StrokeLineCap == PenLineCap.Round)
            {
                context.DrawPilledRect(brush, null, indicatorRect, Orientation);
            }
            else
            {
                context.FillRectangle(brush, indicatorRect);
            }
        }
    
    }

    protected override void CalculateStrokeThickness()
    {
        double strokeThickness;
        if (EffectiveSizeType == SizeType.Large)
        {
            strokeThickness = LARGE_STROKE_THICKNESS;
        }
        else if (EffectiveSizeType == SizeType.Middle)
        {
            strokeThickness = MIDDLE_STROKE_THICKNESS;
        }
        else
        {
            strokeThickness = SMALL_STROKE_THICKNESS;
        }

        if (!double.IsNaN(IndicatorThickness))
        {
            strokeThickness = IndicatorThickness;
        }

        if (IsShowProgressInfo && PercentPosition.IsInner)
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

        StrokeThickness = strokeThickness;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        CalculateSizeTypeThresholdValue();
        CalculateMinBarThickness();
        base.OnApplyTemplate(e);
        SetupPercentLabelForegroundBrush();
    }

    protected override SizeType CalculateEffectiveSizeType(double size)
    {
        var middleThresholdValue = _sizeTypeThresholdValue[SizeType.Middle];
        var smallThresholdValue  = _sizeTypeThresholdValue[SizeType.Small];
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
                InnerStateValue  = defaultExtraInfoSize.Height + LineProgressPadding * 2,
                NormalStateValue = Math.Max(LARGE_STROKE_THICKNESS, defaultExtraInfoSize.Height)
            };
            _sizeTypeThresholdValue.Add(SizeType.Large, largeSizeTypeThresholdValue);
            var middleSizeTypeThresholdValue = new SizeTypeThresholdValue
            {
                InnerStateValue  = defaultExtraInfoSize.Height + LineProgressPadding * 2,
                NormalStateValue = Math.Max(MIDDLE_STROKE_THICKNESS, defaultExtraInfoSize.Height)
            };
            _sizeTypeThresholdValue.Add(SizeType.Middle, middleSizeTypeThresholdValue);

            var smallSizeTypeThresholdValue = new SizeTypeThresholdValue
            {
                InnerStateValue  = smallExtraInfoSize.Height + LineProgressPadding * 2,
                NormalStateValue = Math.Max(SMALL_STROKE_THICKNESS, smallExtraInfoSize.Height)
            };
            _sizeTypeThresholdValue.Add(SizeType.Small, smallSizeTypeThresholdValue);
        }
        else
        {
            var largeSizeTypeThresholdValue = new SizeTypeThresholdValue
            {
                InnerStateValue  = defaultExtraInfoSize.Width + LineProgressPadding * 2,
                NormalStateValue = Math.Max(LARGE_STROKE_THICKNESS, defaultExtraInfoSize.Width)
            };
            _sizeTypeThresholdValue.Add(SizeType.Large, largeSizeTypeThresholdValue);
            var middleSizeTypeThresholdValue = new SizeTypeThresholdValue
            {
                InnerStateValue  = defaultExtraInfoSize.Width + LineProgressPadding * 2,
                NormalStateValue = Math.Max(MIDDLE_STROKE_THICKNESS, defaultExtraInfoSize.Width)
            };
            _sizeTypeThresholdValue.Add(SizeType.Middle, middleSizeTypeThresholdValue);

            var smallSizeTypeThresholdValue = new SizeTypeThresholdValue
            {
                InnerStateValue  = smallExtraInfoSize.Width + LineProgressPadding * 2,
                NormalStateValue = Math.Max(SMALL_STROKE_THICKNESS, smallExtraInfoSize.Width)
            };
            _sizeTypeThresholdValue.Add(SizeType.Small, smallSizeTypeThresholdValue);
        }
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
            if (IsShowProgressInfo)
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
                if (PercentPosition.IsInner)
                {
                    var grooveRect = GetProgressBarRect(controlRect);
                    offsetY = grooveRect.Y + (grooveRect.Height - targetHeight) / 2;
                    var range         = grooveRect.Width;
                    var deflateValue  = range * (1 - Value / (Maximum - Minimum));
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
                var deflateValue  = range * (1 - Value / (Maximum - Minimum));
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

    protected override Size CalculateExtraInfoSize(double fontSize)
    {
        if (IsShowProgressInfo)
        {
            if ((Status == ProgressStatus.Exception || MathUtils.AreClose(Value, Maximum)) &&
                !PercentPosition.IsInner)
            {
                // 只要图标
                return new Size(LineInfoIconSize, LineInfoIconSize);
            }

            var textSize = TextUtils.CalculateTextSize(string.Format(ProgressTextFormat, Value), fontSize, FontFamily);
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

    protected override void NotifyEffectSizeTypeChanged()
    {
        base.NotifyEffectSizeTypeChanged();
        CalculateMinBarThickness();
    }

    protected override void NotifyOrientationChanged()
    {
        base.NotifyOrientationChanged();
        CalculateMinBarThickness();
    }

    protected override void NotifyPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.NotifyPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == StrokeBrushProperty)
            {
                SetupPercentLabelForegroundBrush();
            }
        }

        if (change.Property == PercentPositionProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == IsShowProgressInfoProperty)
        {
            CalculateMinBarThickness();
        }
    }

    // 需要评估是否需要
    private void CalculateMinBarThickness()
    {
        var thickness     = LineProgressPadding * 2;
        var extraInfoSize = CalculateExtraInfoSize(FontSize);
        if (Orientation == Orientation.Horizontal)
        {
            thickness += extraInfoSize.Height;
            MinHeight =  thickness;
            MinWidth  =  extraInfoSize.Width;
        }
        else
        {
            thickness += extraInfoSize.Width;
            MinWidth  =  thickness;
            MinHeight =  extraInfoSize.Height;
        }
    }

    private void SetupPercentLabelForegroundBrush()
    {
        if (!PercentPosition.IsInner)
        {
            _percentageLabelBindingDisposable?.Dispose();
            _percentageLabelBindingDisposable = BindUtils.RelayBind(this, ForegroundProperty, PercentageLabel!, ForegroundProperty);
        }
        else
        {
            if (ColorTextLabel != null && ColorTextLightSolid != null)
            {
                // 根据当前的 Stroke 笔刷计算可读性
                // 但是渐变笔刷就麻烦了，暂时不支持吧
                var colorTextLabel      = (ColorTextLabel as ISolidColorBrush)!.Color;
                var colorTextLightSolid = (ColorTextLightSolid as ISolidColorBrush)!.Color;
                var colors              = new List<Color> { colorTextLabel, colorTextLightSolid };
                if (MathUtils.AreClose(Value, 0))
                {
                    if (GrooveBrush is ISolidColorBrush grooveBrush)
                    {
                        var mostReadable = ColorUtils.MostReadable(grooveBrush.Color, colors);
                        if (mostReadable.HasValue)
                        {
                            PercentageLabel?.SetValue(ForegroundProperty, new SolidColorBrush(mostReadable.Value), BindingPriority.Template);
                        }
                    }
                }
                else
                {
                    if (StrokeBrush is ISolidColorBrush solidColorBrush)
                    {
                        var mostReadable = ColorUtils.MostReadable(solidColorBrush.Color, colors);
                        if (mostReadable.HasValue)
                        {
                            PercentageLabel?.SetValue(ForegroundProperty, new SolidColorBrush(mostReadable.Value), BindingPriority.Template);
                        }
                    }
                }
            }
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ProgressBarPseudoClass.PercentLabelInner, PercentPosition.IsInner);
        PseudoClasses.Set(ProgressBarPseudoClass.PercentLabelInnerStart, PercentPosition.IsInner && PercentPosition.Alignment == LinePercentAlignment.Start);
        PseudoClasses.Set(ProgressBarPseudoClass.PercentLabelInnerCenter, PercentPosition.IsInner && PercentPosition.Alignment == LinePercentAlignment.Center);
        PseudoClasses.Set(ProgressBarPseudoClass.PercentLabelInnerCenter, PercentPosition.IsInner && PercentPosition.Alignment == LinePercentAlignment.End);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePseudoClasses();
    }
}