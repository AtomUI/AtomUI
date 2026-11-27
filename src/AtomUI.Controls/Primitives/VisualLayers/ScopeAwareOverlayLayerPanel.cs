using Avalonia.Controls;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// A plain panel whose type marks a layer scope. <see cref="ScopeAwareOverlayLayer.GetLayer" /> picks the
/// nearest panel of this type around a visual as the host of the overlay layer and adds the layer to its
/// children, so floating content is laid out relative to this panel instead of the window. The ScrollViewer
/// themes use it as <c>PART_ContentHost</c>.
/// </summary>
public class ScopeAwareOverlayLayerPanel : Panel
{
}