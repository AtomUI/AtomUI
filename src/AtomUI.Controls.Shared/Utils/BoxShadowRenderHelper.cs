using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls.Utils;

/// <summary>
/// Draws a rectangle with box shadows so that shadow offsets follow the render scaling.
/// </summary>
/// <remarks>
/// Avalonia's Skia backend translates a box shadow by its offset after the current transform has been
/// applied, so offsets are interpreted in device pixels while the rectangle, spread and blur follow the
/// render scaling and any render transform. At 150% / 200% scaling an offset shadow therefore moves half
/// as far as intended, and an inset shadow whose offset is cancelled by a negative spread disappears.
/// Shadows whose result is affected are drawn in an equivalent form without an offset:
/// <list type="bullet">
/// <item>inset shadows become a zero-offset inset shadow on a larger rectangle, clipped back to the box;</item>
/// <item>outer shadows without blur become a filled geometry (shifted shadow shape minus the box).</item>
/// </list>
/// Other shadows, including blurred outer shadows with an offset, are passed to Avalonia unchanged.
/// </remarks>
internal static class BoxShadowRenderHelper
{
    /// <summary>
    /// Equivalent of <see cref="DrawingContext.DrawRectangle(IBrush?, IPen?, RoundedRect, BoxShadows)" />
    /// with scale-independent shadow offsets.
    /// </summary>
    public static void DrawRectangle(DrawingContext context, IBrush? brush, IPen? pen, RoundedRect rect,
                                     BoxShadows boxShadows)
    {
        if (!NeedsWorkaround(boxShadows))
        {
            context.DrawRectangle(brush, pen, rect, boxShadows);
            return;
        }

        // 与 Avalonia 的绘制顺序一致：外阴影 -> 背景与描边 -> 内阴影
        foreach (var shadow in boxShadows)
        {
            if (shadow != default && !shadow.IsInset)
            {
                DrawOuterShadow(context, rect, shadow);
            }
        }

        if (brush != null || pen != null)
        {
            context.DrawRectangle(brush, pen, rect);
        }

        foreach (var shadow in boxShadows)
        {
            if (shadow != default && shadow.IsInset)
            {
                DrawInsetShadow(context, rect, shadow);
            }
        }
    }

    private static bool NeedsWorkaround(BoxShadows boxShadows)
    {
        foreach (var shadow in boxShadows)
        {
            if (shadow != default && HasOffset(shadow))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasOffset(BoxShadow shadow) => shadow.OffsetX != 0 || shadow.OffsetY != 0;

    private static void DrawOuterShadow(DrawingContext context, RoundedRect rect, BoxShadow shadow)
    {
        if (!HasOffset(shadow) || shadow.Blur > 0 || !TryCreateGeometry(rect, out var boxGeometry))
        {
            context.DrawRectangle(null, null, rect, new BoxShadows(shadow));
            return;
        }

        var shadowRect = Translate(rect.Inflate(shadow.Spread, shadow.Spread), shadow.OffsetX, shadow.OffsetY);
        if (!TryCreateGeometry(shadowRect, out var shadowGeometry))
        {
            context.DrawRectangle(null, null, rect, new BoxShadows(shadow));
            return;
        }

        // 无模糊的外阴影即“平移、外扩后的形状减去盒子本身”
        context.DrawGeometry(new SolidColorBrush(shadow.Color), null,
            new CombinedGeometry(GeometryCombineMode.Exclude, shadowGeometry, boxGeometry));
    }

    private static void DrawInsetShadow(DrawingContext context, RoundedRect rect, BoxShadow shadow)
    {
        if (!HasOffset(shadow))
        {
            context.DrawRectangle(null, null, rect, new BoxShadows(shadow));
            return;
        }

        // CSS 语义下 inset 阴影的“洞”为：按扩展量内缩、再按偏移平移后的盒子；洞外区域模糊后只绘制在盒子内。
        // 以覆盖整个盒子的零偏移矩形内缩出同一个洞，再裁剪回盒子，结果与原阴影等价。
        var box      = rect.Rect;
        var holeRect = Translate(rect.Deflate(shadow.Spread, shadow.Spread), shadow.OffsetX, shadow.OffsetY);
        var hole     = holeRect.Rect;
        var spread = Math.Max(0, Math.Max(Math.Max(hole.Left - box.Left, box.Right - hole.Right),
            Math.Max(hole.Top - box.Top, box.Bottom - hole.Bottom)));
        var equivalent = new BoxShadow
        {
            Blur    = shadow.Blur,
            Spread  = spread,
            IsInset = true,
            Color   = shadow.Color
        };
        using (context.PushClip(rect))
        {
            context.DrawRectangle(null, null, holeRect.Inflate(spread, spread), new BoxShadows(equivalent));
        }
    }

    private static RoundedRect Translate(RoundedRect rect, double offsetX, double offsetY) =>
        new(rect.Rect.Translate(new Vector(offsetX, offsetY)),
            rect.RadiiTopLeft, rect.RadiiTopRight, rect.RadiiBottomRight, rect.RadiiBottomLeft);

    private static bool TryCreateGeometry(RoundedRect rect, out Geometry geometry)
    {
        if (!rect.IsRounded)
        {
            geometry = new RectangleGeometry(rect.Rect);
            return true;
        }

        var radius = rect.RadiiTopLeft;
        if (rect.RadiiTopRight == radius && rect.RadiiBottomRight == radius && rect.RadiiBottomLeft == radius)
        {
            geometry = new RectangleGeometry(rect.Rect, radius.X, radius.Y);
            return true;
        }

        // 非一致圆角没有现成几何，交给 Avalonia 绘制
        geometry = null!;
        return false;
    }
}
