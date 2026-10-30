using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

/// <summary>
/// BackTop 悬浮按钮边缘的滚动进度环，视觉对齐 antd FloatButton.BackTop showProgress：
/// 轨道色/进度色/线宽由 ControlTheme 注入（ColorBorderSecondary/ColorPrimary/LineWidthBold），
/// 从 12 点方向顺时针。进度指示为扇形填充并裁剪到环带（等价 antd conic-gradient 的角度语义）。
/// 扇形角度是运行时连续值，ControlTheme 选择器无法表达，
/// 故按主题绑定约束的兜底条款采用自定义 Render（与 AbstractGeneralCircleProgress 同惯用法）。
/// </summary>
internal class BackTopProgressRing : Control
{
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<BackTopProgressRing, double>(nameof(Progress));

    public static readonly StyledProperty<IBrush?> TrackBrushProperty =
        AvaloniaProperty.Register<BackTopProgressRing, IBrush?>(nameof(TrackBrush));

    public static readonly StyledProperty<IBrush?> IndicatorBrushProperty =
        AvaloniaProperty.Register<BackTopProgressRing, IBrush?>(nameof(IndicatorBrush));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<BackTopProgressRing, double>(nameof(StrokeThickness), 2d);

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<BackTopProgressRing, CornerRadius>(nameof(CornerRadius));

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public IBrush? TrackBrush
    {
        get => GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public IBrush? IndicatorBrush
    {
        get => GetValue(IndicatorBrushProperty);
        set => SetValue(IndicatorBrushProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    static BackTopProgressRing()
    {
        AffectsRender<BackTopProgressRing>(ProgressProperty, TrackBrushProperty,
            IndicatorBrushProperty, StrokeThicknessProperty, CornerRadiusProperty);
    }

    private IPen? _trackPen;

    /// <summary>
    /// 计算环的几何：外/中心线/内边界矩形与对应圆角半径。
    /// 外边界半径必须等于按钮自身的圆角半径，使环的外缘与按钮轮廓完全重合；
    /// 中心线半径为外边界半径减去半线宽，内边界半径为外边界半径减去线宽。
    /// </summary>
    internal static BackTopProgressRingMetrics CalculateRingMetrics(
        Size size, double cornerRadius, double thickness)
    {
        var outerRect      = new Rect(size);
        var centerlineRect = outerRect.Deflate(thickness / 2);
        var innerRect      = outerRect.Deflate(thickness);
        var halfThickness  = thickness / 2;

        var outerLimit    = Math.Min(outerRect.Width, outerRect.Height) / 2;
        var outerRadius   = Math.Clamp(cornerRadius, 0d, outerLimit);
        var centerlineRadius = Math.Max(0d, outerRadius - halfThickness);
        var innerRadius      = Math.Max(0d, outerRadius - thickness);

        return new BackTopProgressRingMetrics(outerRect, centerlineRect, innerRect,
            outerRadius, centerlineRadius, innerRadius);
    }

    public override void Render(DrawingContext context)
    {
        var bounds    = new Rect(Bounds.Size);
        var thickness = StrokeThickness;
        if (bounds.Width <= 0 || bounds.Height <= 0 || thickness <= 0)
        {
            return;
        }

        var metrics = CalculateRingMetrics(bounds.Size, CornerRadius.TopLeft, thickness);

        PenUtils.TryModifyOrCreate(ref _trackPen, TrackBrush, thickness);
        if (_trackPen is not null)
        {
            // 轨道描边以中心线为基准；外缘与按钮轮廓重合，两种形状共用同一路径
            context.DrawRectangle(null, _trackPen, metrics.CenterlineRect,
                metrics.CenterlineRadius, metrics.CenterlineRadius);
        }

        var sweepAngle = 360d * Math.Clamp(Progress, 0d, 1d);
        if (sweepAngle <= 0 || IndicatorBrush is null)
        {
            return;
        }

        // 进度扇形填充裁剪到「外圆角矩形−内圆角矩形」环带，Circle/Square 统一路径。
        // 指示器必须是扇形填充而非环矩形的内切圆弧描边：Square 形状下圆角方向的
        // 内切圆弧半径小于外边界，会落入内孔几何被裁掉，进度环退化为四段侧边短线
        var bandGeometry = new CombinedGeometry(GeometryCombineMode.Exclude,
            BuildRoundedRectGeometry(metrics.OuterRect, metrics.OuterRadius),
            BuildRoundedRectGeometry(metrics.InnerRect, metrics.InnerRadius));

        if (sweepAngle >= 360d)
        {
            // 扇形起终点重合退化，直接用进度色填充整个环带
            context.DrawGeometry(IndicatorBrush, null, bandGeometry);
            return;
        }

        // 以环中心为顶点的扇形，半径取半对角线以覆盖环带最外点，
        // 从 -90°（12 点方向）顺时针扫 sweepAngle。
        // 角度→点换算与 CommonShapeBuilder.GetRingPoint 同约定：x = cx + r·cos(deg)，y = cy + r·sin(deg)
        var center = metrics.CenterlineRect.Center;
        var sectorRadius = Math.Sqrt(metrics.CenterlineRect.Width * metrics.CenterlineRect.Width / 4
                                     + metrics.CenterlineRect.Height * metrics.CenterlineRect.Height / 4);

        var startPoint = new Point(
            center.X + sectorRadius * Math.Cos(MathUtils.Deg2Rad(-90d)),
            center.Y + sectorRadius * Math.Sin(MathUtils.Deg2Rad(-90d)));
        var endPoint = new Point(
            center.X + sectorRadius * Math.Cos(MathUtils.Deg2Rad(sweepAngle - 90d)),
            center.Y + sectorRadius * Math.Sin(MathUtils.Deg2Rad(sweepAngle - 90d)));

        var sectorGeometry = new StreamGeometry();
        using (var ctx = sectorGeometry.Open())
        {
            ctx.BeginFigure(center, true);
            ctx.LineTo(startPoint);
            ctx.ArcTo(endPoint, new Size(sectorRadius, sectorRadius), 0d,
                sweepAngle > 180d, SweepDirection.Clockwise);
            ctx.EndFigure(true);
        }

        using (context.PushGeometryClip(bandGeometry))
        {
            context.DrawGeometry(IndicatorBrush, null, sectorGeometry);
        }
    }

    private static StreamGeometry BuildRoundedRectGeometry(Rect rect, double radius)
    {
        var geometry = new StreamGeometry();
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return geometry;
        }

        radius = Math.Clamp(radius, 0d, Math.Min(rect.Width, rect.Height) / 2);
        using var ctx = geometry.Open();
        if (radius <= 0)
        {
            ctx.BeginFigure(rect.TopLeft, true);
            ctx.LineTo(rect.TopRight);
            ctx.LineTo(rect.BottomRight);
            ctx.LineTo(rect.BottomLeft);
            ctx.EndFigure(true);
            return geometry;
        }

        ctx.BeginFigure(new Point(rect.Left + radius, rect.Top), true);
        ctx.LineTo(new Point(rect.Right - radius, rect.Top));
        ctx.ArcTo(new Point(rect.Right, rect.Top + radius),
            new Size(radius, radius), 0d, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(rect.Right, rect.Bottom - radius));
        ctx.ArcTo(new Point(rect.Right - radius, rect.Bottom),
            new Size(radius, radius), 0d, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(rect.Left + radius, rect.Bottom));
        ctx.ArcTo(new Point(rect.Left, rect.Bottom - radius),
            new Size(radius, radius), 0d, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(rect.Left, rect.Top + radius));
        ctx.ArcTo(new Point(rect.Left + radius, rect.Top),
            new Size(radius, radius), 0d, false, SweepDirection.Clockwise);
        ctx.EndFigure(true);
        return geometry;
    }
}

/// <summary>
/// 进度环的几何参数。外边界为按钮轮廓（半径等于按钮圆角半径），
/// 中心线与内边界由线宽向内偏移得到。
/// </summary>
internal readonly record struct BackTopProgressRingMetrics(
    Rect OuterRect,
    Rect CenterlineRect,
    Rect InnerRect,
    double OuterRadius,
    double CenterlineRadius,
    double InnerRadius);
