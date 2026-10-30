using AtomUI.Data;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

[PseudoClasses(ProgressBarPseudoClass.Indeterminate, ProgressBarPseudoClass.Completed)]
public abstract class AbstractGeneralProgressBar : AbstractLineProgress
{
    #region 公共属性定义

    public static readonly StyledProperty<PercentPosition> PercentPositionProperty =
        AvaloniaProperty.Register<AbstractGeneralProgressBar, PercentPosition>(nameof(PercentPosition), new PercentPosition());

    public PercentPosition PercentPosition
    {
        get => GetValue(PercentPositionProperty);
        set => SetValue(PercentPositionProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<IBrush?> ColorTextLabelProperty =
        AvaloniaProperty.Register<AbstractGeneralProgressBar, IBrush?>(nameof(ColorTextLabel));

    internal static readonly StyledProperty<IBrush?> ColorTextLightSolidProperty =
        AvaloniaProperty.Register<AbstractGeneralProgressBar, IBrush?>(nameof(ColorTextLightSolid));

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

    private LineProgressPanel? _progressBody;

    private protected override bool HasTemplateProgressVisuals => _progressBody is not null;

    static AbstractGeneralProgressBar()
    {
        AffectsMeasure<AbstractGeneralProgressBar>(IndicatorThicknessProperty, PercentPositionProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);
        double targetWidth  = 0;
        double targetHeight = 0;
        if (Orientation == Orientation.Horizontal)
        {
            targetHeight = StrokeThickness;
            if (!PercentPosition.IsInner && IsProgressInfoVisible)
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
            if (!PercentPosition.IsInner && IsProgressInfoVisible)
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

    protected override void RenderGroove(DrawingContext context)
    {
        var controlRect = new Rect(new Point(0, 0), Bounds.Size);
        _grooveRect = GetProgressBarRect(controlRect);
        DrawProgressRectangle(context, GrooveBrush, _grooveRect);
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

        deflateValue = range * (1 - CalculateProgressRatio(Value));
        DrawIndicatorBar(context, deflateValue, StrokeBrush);

        // 绘制成功阈值
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

        if (IsProgressInfoVisible && PercentPosition.IsInner)
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
        _progressBody = null;
        CalculateSizeTypeThresholdValue();
        CalculateMinBarThickness();
        base.OnApplyTemplate(e);
        _progressBody = e.NameScope.Find<LineProgressPanel>(ProgressBodyPart);
        _progressBody?.InvalidateArrange();
    }

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

    protected void CalculateSizeTypeThresholdValue()
    {
        double fontSize   = default;
        double fontSizeSM = default;
        {
            if (TokenResourceUtils.FindTokenResource(this, SharedTokenKind.FontSize) is double value)
            {
                fontSize = value;
            }
        }

        {
            if (TokenResourceUtils.FindTokenResource(this, SharedTokenKind.FontSizeSM) is double value)
            {
                fontSizeSM = value;
            }
        }
        var defaultExtraInfoSize = CalculateExtraInfoSize(fontSize);
        var smallExtraInfoSize   = CalculateExtraInfoSize(fontSizeSM);
        if (Orientation == Orientation.Horizontal)
        {
            _largeSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(LARGE_STROKE_THICKNESS, defaultExtraInfoSize.Height),
                defaultExtraInfoSize.Height + LineProgressPadding * 2);
            _middleSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(MIDDLE_STROKE_THICKNESS, defaultExtraInfoSize.Height),
                defaultExtraInfoSize.Height + LineProgressPadding * 2);
            _smallSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(SMALL_STROKE_THICKNESS, smallExtraInfoSize.Height),
                smallExtraInfoSize.Height + LineProgressPadding * 2);
        }
        else
        {
            _largeSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(LARGE_STROKE_THICKNESS, defaultExtraInfoSize.Width),
                defaultExtraInfoSize.Width + LineProgressPadding * 2);
            _middleSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(MIDDLE_STROKE_THICKNESS, defaultExtraInfoSize.Width),
                defaultExtraInfoSize.Width + LineProgressPadding * 2);
            _smallSizeTypeThresholdValue = new SizeTypeThresholdValue(
                Math.Max(SMALL_STROKE_THICKNESS, smallExtraInfoSize.Width),
                smallExtraInfoSize.Width + LineProgressPadding * 2);
        }

        _sizeTypeThresholdValuesInitialized = true;
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
            if (IsProgressInfoVisible)
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
            if (IsProgressInfoVisible)
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
        if (IsProgressInfoVisible)
        {
            targetWidth  = _extraInfoSize.Width;
            targetHeight = _extraInfoSize.Height;
        }

        if (Orientation == Orientation.Horizontal)
        {
            if (IsProgressInfoVisible)
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

    protected override Size CalculateExtraInfoSize(double fontSize)
    {
        if (IsProgressInfoVisible)
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
        if (change.Property == StrokeBrushProperty ||
            change.Property == ForegroundProperty ||
            change.Property == PercentPositionProperty ||
            change.Property == ColorTextLabelProperty ||
            change.Property == ColorTextLightSolidProperty)
        {
            SetupPercentLabelColor();
        }
        if (change.Property == PercentPositionProperty)
        {
            UpdatePseudoClasses();
        }

        if (change.Property == IsProgressInfoVisibleProperty)
        {
            CalculateMinBarThickness();
        }

        if (change.Property == ValueProperty ||
            change.Property == MinimumProperty ||
            change.Property == MaximumProperty ||
            change.Property == SuccessThresholdProperty ||
            change.Property == OrientationProperty ||
            change.Property == StrokeThicknessProperty ||
            change.Property == StrokeLineCapProperty ||
            change.Property == IsProgressInfoVisibleProperty ||
            change.Property == PercentPositionProperty)
        {
            _progressBody?.InvalidateArrange();
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

    private void SetupPercentLabelColor()
    {
        if (!PercentPosition.IsInner)
        {
            SetPercentageLabelColorIfChanged(Foreground);
        }
        else
        {
            if (ColorTextLabel != null && ColorTextLightSolid != null)
            {
                // 根据当前的 Stroke 笔刷计算可读性
                // 但是渐变笔刷就麻烦了，暂时不支持吧
                var colorTextLabel      = (ColorTextLabel as ISolidColorBrush)!.Color;
                var colorTextLightSolid = (ColorTextLightSolid as ISolidColorBrush)!.Color;
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
        var range = Orientation == Orientation.Horizontal ? grooveRect.Width : grooveRect.Height;
        var deflateValue = range * (1 - CalculateProgressRatio(value));
        return Orientation == Orientation.Horizontal
            ? grooveRect.Deflate(new Thickness(0, 0, deflateValue, 0))
            : grooveRect.Deflate(new Thickness(0, 0, 0, deflateValue));
    }

    internal Rect GetLineProgressIndicatorRect(Size size)
    {
        return IsProgressInfoVisible
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
