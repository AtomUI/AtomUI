using AtomUI.Controls.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Controls;

internal class RoundRectWavePainter : AbstractWavePainter
{
    public Size OriginSize { get; set; }

    public CornerRadius CornerRadius { get; set; }
    private Geometry? _originGeometry;
    private Rect _originGeometryRect;
    private CornerRadius _originGeometryCornerRadius;

    public RoundRectWavePainter(Size size = default, Point originPoint = default)
        : base(originPoint)
    {
        OriginSize = size;
        WaveType   = WaveSpiritType.RoundRectWave;
    }

    public override void Paint(DrawingContext context, object newSize, double newOpacity)
    {
        var newSizeTyped = (Size)newSize;
        if (newSize is null)
        {
            throw new ArgumentException("newSize argument must be Size type.");
        }
        
        using var state = context.PushOpacity(newOpacity);

        var originRect = new Rect(OriginPoint, OriginSize);
        if (_originGeometry is null || _originGeometryRect != originRect ||
            _originGeometryCornerRadius != CornerRadius)
        {
            _originGeometry = BuildRoundedGeometry(originRect, CornerRadius);
            _originGeometryRect = originRect;
            _originGeometryCornerRadius = CornerRadius;
        }

        var deltaSize = newSizeTyped - OriginSize;
        var salt      = deltaSize.Width / WaveRange / 2;
        salt += 1;

        var currentCornerRadius = new CornerRadius(CornerRadius.TopLeft * salt,
            CornerRadius.TopRight * salt,
            CornerRadius.BottomRight * salt,
            CornerRadius.BottomLeft * salt);

        var newPoint = OriginPoint - new Point(deltaSize.Width / 2, deltaSize.Height / 2);

        var newGeometry = BuildRoundedGeometry(
            new Rect(newPoint.X, newPoint.Y, newSizeTyped.Width, newSizeTyped.Height),
            currentCornerRadius);
        var targetGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, newGeometry, _originGeometry);
        var geometryDrawing = new GeometryDrawing
        {
            Brush    = WaveBrush,
            Geometry = targetGeometry
        };
        geometryDrawing.Draw(context);
    }

    private Geometry? BuildRoundedGeometry(Rect rect, CornerRadius cornerRadius)
    {
        // 根据四个角是否一样看是使用简单的还是复杂的矢量图形生成算法
        if (cornerRadius.IsUniform)
        {
            return new RectangleGeometry(rect, cornerRadius.TopLeft, cornerRadius.TopLeft);
        }

        var backgroundOuterKeypoints = RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI(
            rect,
            new Thickness(0),
            cornerRadius,
            BackgroundSizing.OuterBorderEdge);

        var backgroundGeometry = new StreamGeometry();
        using (var ctx = backgroundGeometry.Open())
        {
            RoundRectGeometryBuilder.DrawRoundedCornersRectangle(ctx, ref backgroundOuterKeypoints);
        }

        return backgroundGeometry;
    }

    public override RoundRectWavePainter Clone()
    {
        return new RoundRectWavePainter(OriginSize, OriginPoint);
    }

    public override void NotifyBuildSizeAnimation(Animation animation, AvaloniaProperty targetProperty)
    {
        animation.Duration = SizeMotionDuration;
        animation.Easing   = SizeEasingCurve ?? DefaultLinearEasing;
        animation.Children.Add(new KeyFrame
        {
            Setters =
            {
                new Setter
                {
                    Property = targetProperty,
                    Value    = OriginSize
                }
            },
            KeyTime = TimeSpan.FromMilliseconds(0)
        });
        var endValue = OriginSize + new Size(WaveRange * 2, WaveRange * 2);
        animation.Children.Add(new KeyFrame
        {
            Setters =
            {
                new Setter
                {
                    Property = targetProperty,
                    Value    = endValue
                }
            },
            KeyTime = SizeMotionDuration
        });
    }
}
