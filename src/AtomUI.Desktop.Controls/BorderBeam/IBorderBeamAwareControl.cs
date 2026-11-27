namespace AtomUI.Desktop.Controls;

/// <summary>
/// Implemented by a control that is the content of a <see cref="BorderBeam" /> and reports the ring the beam
/// paints on. The beam then follows the border of this control instead of the border set on the
/// <see cref="BorderBeam" />.
/// </summary>
public interface IBorderBeamAwareControl
{
    /// <summary>
    /// Raised after the geometry that <see cref="GetBorderBeamGeometry" /> returns has changed. A hosting
    /// <see cref="BorderBeam" /> then reads the geometry again.
    /// </summary>
    event EventHandler? BorderBeamGeometryChanged;

    /// <summary>
    /// Returns the border thickness and corner radius of the ring the beam paints on.
    /// </summary>
    BorderBeamGeometry GetBorderBeamGeometry();
}
