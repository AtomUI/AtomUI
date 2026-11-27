namespace AtomUI.Controls.Primitives;

/// <summary>
/// A <see cref="DashedBorder" /> whose border drawing trims an overflow of less than one physical pixel past the
/// right or bottom edge of a clipping ancestor when layout rounding is on at a fractional layout scale.
/// The child layout is not affected.
/// </summary>
public class PixelAlignedBorder : DashedBorder
{
    // Avalonia's fractional-scale layout can round a child one physical pixel
    // beyond a ClipToBounds ancestor while a Wayland resize is in flight. Trim
    // only that sub-pixel trailing remainder for the border drawing; the child
    // layout and integer-scale rendering remain unchanged.
    //
    // TODO(Avalonia upgrade): Re-check the upstream fractional-scale resize issue
    // before changing the Avalonia version and remove this workaround once the
    // layout and clip quantization rules are unified:
    // https://github.com/AvaloniaUI/Avalonia/issues?q=is%3Aissue+wayland+fractional+scale
    internal override bool ClipTrailingEdgeAtFractionalScale => true;
}
