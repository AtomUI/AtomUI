using AtomUI.Utils;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Draws a glyph repeatedly over an area. Shared by the desktop <see cref="Watermark"/> adorner and by
/// controls that tile a glyph themselves. A tile is a cell with the glyph centered in it; the cell grows by
/// the glyph's <see cref="WatermarkGlyph.HorizontalSpace"/> and <see cref="WatermarkGlyph.VerticalSpace"/>
/// to give the grid step, and rotation, mirroring and cross offset are applied per cell.
/// </summary>
internal sealed class WatermarkTileRenderer
{
    // Rotation matrices of a cell (normal / mirrored), rebuilt only when the cell size or the angle changes.
    private Matrix _normalRotationMatrix;
    private Matrix _mirrorRotationMatrix;
    private Size   _cachedCell;
    private double _cachedRotate;
    private bool   _matrixCacheValid;

    internal int MatrixRebuildCount { get; private set; }

    public void Render(DrawingContext context, Size area, Size cell, WatermarkGlyph glyph)
    {
        var glyphSize        = glyph.GetDesiredSize();
        var columnStep       = cell.Width + glyph.HorizontalSpace;
        var rowStep          = cell.Height + glyph.VerticalSpace;
        var horizontalOffset = glyph.HorizontalOffset;
        var verticalOffset   = glyph.VerticalOffset;
        // Negative spacing allows overprinting, but the step must be finite and strictly advancing. A glyph
        // without a finite size (for example an image that has no size yet) must not reach drawing or the
        // rotation math either.
        if (!double.IsFinite(area.Width) || !double.IsFinite(area.Height) ||
            !double.IsFinite(cell.Width) || cell.Width <= 0 ||
            !double.IsFinite(cell.Height) || cell.Height <= 0 ||
            !double.IsFinite(glyphSize.Width) || glyphSize.Width <= 0 ||
            !double.IsFinite(glyphSize.Height) || glyphSize.Height <= 0 ||
            !double.IsFinite(columnStep) || columnStep <= 0 ||
            !double.IsFinite(rowStep) || rowStep <= 0 ||
            !double.IsFinite(horizontalOffset) || !double.IsFinite(verticalOffset) ||
            !double.IsFinite(glyph.Rotate))
        {
            return;
        }

        if (!_matrixCacheValid || _cachedCell != cell || _cachedRotate != glyph.Rotate)
        {
            RebuildMatrixCache(cell, glyph.Rotate);
        }

        // Offset that centers the glyph in its cell; zero when the cell is the glyph's own size.
        var inset    = new Vector((cell.Width - glyphSize.Width) / 2, (cell.Height - glyphSize.Height) / 2);
        var hasInset = inset.X != 0 || inset.Y != 0;

        // Only built-in glyphs with a known drawing extent skip invisible tiles. A custom glyph or subclass
        // may draw outside its desired size, so it keeps its existing Render contract.
        Rect? drawingBounds = glyph.GetType() == typeof(TextGlyph)
            ? ((TextGlyph)glyph).GetDrawingBounds()
            : glyph.GetType() == typeof(ImageGlyph) ? new Rect(glyphSize) : null;
        if (drawingBounds is null && (horizontalOffset + columnStep <= horizontalOffset ||
                                      verticalOffset + rowStep <= verticalOffset))
        {
            return;
        }

        using (context.PushClip(new Rect(area)))
        using (context.PushOpacity(glyph.Opacity))
        {
            Rect? tileBounds = null;
            if (drawingBounds is { } bounds)
            {
                var glyphBounds = bounds.Translate(inset);
                tileBounds = glyphBounds.TransformToAABB(_normalRotationMatrix);
                if (glyph.IsMirrorUsed)
                {
                    tileBounds = tileBounds.Value.Union(glyphBounds.TransformToAABB(_mirrorRotationMatrix));
                }
            }

            var (t, isOddRow) = tileBounds is { } rowBounds
                ? FirstVisibleTile(verticalOffset, rowStep, -rowBounds.Bottom)
                : (verticalOffset, false);
            while (t < area.Height)
            {
                var pushState = new DrawingContext.PushedState();
                if (isOddRow && glyph.IsCrossUsed)
                {
                    pushState = context.PushTransform(Matrix.CreateTranslation(columnStep / 2, 0));
                }

                using (pushState)
                {
                    var crossOffset = isOddRow && glyph.IsCrossUsed ? columnStep / 2 : 0;
                    var (l, isOddColumn) = tileBounds is { } columnBounds
                        ? FirstVisibleTile(horizontalOffset, columnStep, -columnBounds.Right - crossOffset)
                        : (horizontalOffset, false);
                    while (l < area.Width)
                    {
                        // Cached matrices: no trigonometry or allocation per tile.
                        var m = isOddColumn && glyph.IsMirrorUsed
                            ? _mirrorRotationMatrix
                            : _normalRotationMatrix;

                        using (context.PushTransform(Matrix.CreateTranslation(l, t)))
                        using (context.PushTransform(m))
                        {
                            if (hasInset)
                            {
                                using (context.PushTransform(Matrix.CreateTranslation(inset.X, inset.Y)))
                                {
                                    glyph.Render(context);
                                }
                            }
                            else
                            {
                                glyph.Render(context);
                            }
                        }

                        var nextColumn = l + columnStep;
                        if (nextColumn <= l)
                        {
                            break;
                        }
                        l = nextColumn;
                        isOddColumn = !isOddColumn;
                    }

                    var nextRow = t + rowStep;
                    if (nextRow <= t)
                    {
                        break;
                    }
                    t = nextRow;
                    isOddRow = !isOddRow;
                }
            }
        }
    }

    private void RebuildMatrixCache(Size cell, double rotate)
    {
        var angleRad = rotate * Math.PI / 180;
        _normalRotationMatrix = MatrixUtils.CreateRotationRadians(angleRad, cell.Width / 2, cell.Height / 2);
        _mirrorRotationMatrix = MatrixUtils.CreateRotationRadians(-angleRad, cell.Width / 2, cell.Height / 2);
        _cachedCell           = cell;
        _cachedRotate         = rotate;
        _matrixCacheValid     = true;
        MatrixRebuildCount++;
    }

    private static (double Position, bool IsOdd) FirstVisibleTile(double offset, double step, double boundary)
    {
        var period = step * 2;
        if (offset >= boundary || !double.IsFinite(period) || !double.IsFinite(boundary))
        {
            return (offset, false);
        }

        // Take the remainders separately so a huge quotient multiplied back by step cannot cancel the
        // offset, and keep the alternating row / column phase.
        var phase = (offset % period - boundary % period) % period;
        if (phase < 0)
        {
            phase += period;
        }
        var isOdd = phase >= step;
        if (isOdd)
        {
            phase -= step;
        }
        return (boundary + phase, isOdd);
    }
}
