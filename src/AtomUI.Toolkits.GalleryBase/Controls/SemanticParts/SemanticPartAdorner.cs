using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace AtomUI.Toolkits.GalleryBase.Controls;

internal sealed class SemanticPartAdorner : Control
{
    private const double PrimaryLayoutOutset = 2;
    private const double SecondaryLayoutOutset = 1;

    private static readonly ImmutablePen PrimaryPen = new(0xFFFAAD14, 2);
    private static readonly ImmutablePen SecondaryPen = new(0xD9FAAD14, 1);

    private readonly bool _isPrimary;
    private readonly double _layoutOutset;
    private readonly Visual? _clipHost;

    /// <summary>
    /// 语义预览的高亮画布（PART_PreviewStage）。声明后描边与画布求交：
    /// 溢出画布的部件只高亮画布内的部分，完全在画布之外的部件整体隐藏，
    /// 高亮框绝不越出画布边界。
    /// </summary>
    internal Visual? ClipHost => _clipHost;

    private SemanticPartAdorner(bool isPrimary, Visual? clipHost)
    {
        _isPrimary       = isPrimary;
        _layoutOutset    = isPrimary ? PrimaryLayoutOutset : SecondaryLayoutOutset;
        _clipHost        = clipHost;
        Focusable        = false;
        IsHitTestVisible = false;
        Margin           = new Thickness(-_layoutOutset);

        // Semantic markers are an inspection overlay, not content constrained by the
        // target's ancestor layout. Keep this invariant on the adorner itself so every
        // creation path remains safe when the target sits under ClipToBounds ancestors.
        AdornerLayer.SetIsClipEnabled(this, false);
    }

    internal static SemanticPartAdorner Create(Visual target, bool isPrimary, Visual? clipHost = null)
    {
        ArgumentNullException.ThrowIfNull(target);
        var adorner = new SemanticPartAdorner(isPrimary, clipHost);
        AdornerLayer.SetAdornedElement(adorner, target);
        return adorner;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // AdornerLayer 按目标尺寸排列 child。负 Margin 让 adorner 自身覆盖外扩区域，
        // 避免把描边画到 Bounds 之外后在目标左侧或上侧被裁掉。
        //
        // 主标记只画一条金色描边，不带白色外环。上游 dumi Marker 的
        // `box-shadow: 0 0 0 1px #fff` 会在浅色舞台背景上显出一条突兀的白线，
        // 产品决定不采用。
        if (_isPrimary)
        {
            DrawMarker(context, PrimaryPen, 1);
        }
        else
        {
            DrawMarker(context, SecondaryPen, 0.5);
        }
    }

    private void DrawMarker(DrawingContext context, IPen pen, double markerOutset)
    {
        var markerRect = GetMarkerRect(Bounds.Size, _layoutOutset, markerOutset);

        // 贴边满区目标（如 popup 根、native 预览对话框的 popup.root/body）外扩描边会越出
        // 宿主窗口表面被 OS 裁剪。钳制到 adorner 所在层（窗口/弹层客户区）内，仅内收
        // 半个笔宽让描边贴边完整可见——对齐上游 Marker（border 沿目标边缘、不内收留白）；
        // 层不可达或余量不足时按原矩形绘制。
        var layer = _layer ??= this.GetVisualAncestors().OfType<AdornerLayer>().FirstOrDefault();
        if (layer is not null)
        {
            var transform = (RenderTransform as MatrixTransform)?.Matrix ?? Matrix.Identity;
            var positionInLayer = new Point(
                Bounds.Position.X + transform.M31,
                Bounds.Position.Y + transform.M32);

            // 预览画布钳制优先：跨视觉根的目标（popup/独立窗口宿主）无法换算画布坐标，
            // 退回仅按层钳制的旧行为。
            if (_clipHost is { } host &&
                host.TranslatePoint(default, layer) is { } hostTopLeft)
            {
                var hostRect = new Rect(hostTopLeft.X, hostTopLeft.Y, host.Bounds.Width, host.Bounds.Height);
                var clampedByHost = ClampMarkerRectToClipHost(markerRect, hostRect, positionInLayer, pen.Thickness);
                if (clampedByHost is null)
                {
                    // 目标完全在画布之外：整体隐藏，不跨画布边界绘制。
                    return;
                }
                markerRect = clampedByHost.Value;
            }

            markerRect = ClampMarkerRect(markerRect, layer.Bounds, positionInLayer, pen.Thickness);
        }

        context.DrawRectangle(null, pen, markerRect);
    }

    /// <summary>
    /// 把描边矩形钳制到预览画布在 adorner 本地坐标空间的范围内，仅内收半个笔宽。
    /// 与画布无交集（目标完全在画布外）时返回 null，调用方应整体隐藏该标记。
    /// </summary>
    internal static Rect? ClampMarkerRectToClipHost(
        Rect markerRect,
        Rect clipHostRectInLayer,
        Point adornerPositionInLayer,
        double penThickness)
    {
        var hostLocal = new Rect(
            clipHostRectInLayer.X - adornerPositionInLayer.X,
            clipHostRectInLayer.Y - adornerPositionInLayer.Y,
            clipHostRectInLayer.Width,
            clipHostRectInLayer.Height);
        var clamped = markerRect.Intersect(hostLocal.Deflate(penThickness / 2));
        return clamped.Width >= 1 && clamped.Height >= 1 ? clamped : null;
    }

    /// <summary>
    /// 把描边矩形钳制到层（窗口/弹层客户区）在 adorner 本地坐标空间的范围内。仅内收
    /// 半个笔宽：超出层的边贴到层边缘，描边中心落在层边界上，一半线宽完整可见；
    /// 交集退化（层过小）时返回原矩形。
    /// </summary>
    internal static Rect ClampMarkerRect(
        Rect markerRect,
        Rect layerBounds,
        Point adornerPositionInLayer,
        double penThickness)
    {
        var layerLocal = new Rect(
            -adornerPositionInLayer.X,
            -adornerPositionInLayer.Y,
            layerBounds.Width,
            layerBounds.Height);
        var clamped = markerRect.Intersect(layerLocal.Deflate(penThickness / 2));
        return clamped.Width >= 1 && clamped.Height >= 1 ? clamped : markerRect;
    }

    internal static Rect GetMarkerRect(Size adornerBounds, double layoutOutset, double markerOutset)
    {
        var inset = layoutOutset - markerOutset;
        return new Rect(inset,
                        inset,
                        Math.Max(0, adornerBounds.Width - inset * 2),
                        Math.Max(0, adornerBounds.Height - inset * 2));
    }

    private AdornerLayer? _layer;
}
