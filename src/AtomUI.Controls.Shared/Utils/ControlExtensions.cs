using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;

namespace AtomUI.Controls.Utils;

internal static class ControlUtilsExtensions
{
    /// <summary>
    /// Captures the current visual state of the control as a <see cref="RenderTargetBitmap" />.
    /// </summary>
    /// <param name="control">
    /// The control to capture.
    /// </param>
    /// <returns>
    /// A new <see cref="RenderTargetBitmap" /> containing the rendered control.
    /// The caller is responsible for disposing the returned bitmap.
    /// </returns>
    public static RenderTargetBitmap CaptureCurrentBitmap(this Control control)
    {
        var scaling    = TopLevel.GetTopLevel(control)?.RenderScaling ?? 1.0;
        Size targetSize = default;
        if (control.DesiredSize == default)
        {
            targetSize = LayoutHelper.MeasureChild(control, Size.Infinity, new Thickness());
        }
        else
        {
            targetSize = control.DesiredSize;
        }
        var bitmap = new RenderTargetBitmap(
            new PixelSize((int)(targetSize.Width * scaling), (int)(targetSize.Height * scaling)),
            new Vector(96 * scaling, 96 * scaling));
        bitmap.Render(control);
        return bitmap;
    }

    public static bool IsAttachedToLogicalTree(this Control control)
    {
        return ((ILogical)control).IsAttachedToLogicalTree;
    }
}