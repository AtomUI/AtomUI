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
    /// The highlight canvas of the semantic preview (PART_PreviewStage). When it is set, the marker stroke
    /// is intersected with the canvas: a part that overflows the canvas is highlighted only inside it, a
    /// part entirely outside the canvas is hidden, and no marker crosses the canvas edge.
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

        // AdornerLayer arranges its children at the size of the target. The negative Margin lets the adorner
        // cover the outset area itself, so a stroke drawn outside Bounds is not clipped on the left or top
        // side of the target.
        //
        // The primary marker draws a single golden stroke without a white outer ring. The box-shadow
        // `0 0 0 1px #fff` of the upstream dumi Marker would show a jarring white line on the light stage
        // background, so the product decided not to adopt it.
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

        // A target that fills its surface up to the edge (such as a popup root, or popup.root and body of the
        // native preview dialog) would have its outset stroke reach beyond the host window surface, where the
        // OS clips it. Clamp to the layer that hosts the adorner (the window or popup client area) and inset
        // by half the pen width only, so an edge-aligned stroke stays fully visible. This matches the upstream
        // Marker (border along the target edge, no inset padding). When the layer is unreachable or leaves too
        // little room, the marker is drawn with the original rect.
        var layer = _layer ??= this.GetVisualAncestors().OfType<AdornerLayer>().FirstOrDefault();
        if (layer is not null)
        {
            var transform = (RenderTransform as MatrixTransform)?.Matrix ?? Matrix.Identity;
            var positionInLayer = new Point(
                Bounds.Position.X + transform.M31,
                Bounds.Position.Y + transform.M32);

            // Clamping to the preview canvas comes first: a target in another visual root (a popup or a standalone
            // window host) cannot be converted to canvas coordinates, so the marker keeps the earlier behavior
            // and is clamped by the layer alone.
            if (_clipHost is { } host &&
                host.TranslatePoint(default, layer) is { } hostTopLeft)
            {
                var hostRect = new Rect(hostTopLeft.X, hostTopLeft.Y, host.Bounds.Width, host.Bounds.Height);
                var clampedByHost = ClampMarkerRectToClipHost(markerRect, hostRect, positionInLayer, pen.Thickness);
                if (clampedByHost is null)
                {
                    // The target lies entirely outside the canvas: hide it instead of drawing across the canvas edge.
                    return;
                }
                markerRect = clampedByHost.Value;
            }

            markerRect = ClampMarkerRect(markerRect, layer.Bounds, positionInLayer, pen.Thickness);
        }

        context.DrawRectangle(null, pen, markerRect);
    }

    /// <summary>
    /// Clamps the marker rect to the preview canvas, after converting the canvas rect to the adorner's
    /// local coordinate space, and insets the canvas by half the pen width only.
    /// </summary>
    /// <returns>
    /// The clamped rect, or <see langword="null" /> when the overlap with the inset canvas is less than one
    /// pixel wide or high; the caller then hides the marker.
    /// </returns>
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
    /// Clamps the marker rect to the layer (the window or popup client area), after converting the layer
    /// bounds to the adorner's local coordinate space, inset by half the pen width only: an edge beyond the
    /// layer moves onto the layer edge, so the stroke is centered on the layer boundary and half of its
    /// width stays visible.
    /// </summary>
    /// <returns>
    /// The clamped rect, or <paramref name="markerRect" /> when the intersection is degenerate (the layer
    /// is too small).
    /// </returns>
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
