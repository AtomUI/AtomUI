using Avalonia;

namespace AtomUI.Theme;

/// <summary>
/// Declares that a control hosts live semantic parts outside the owner's visual tree, for example in a
/// standalone preview window. The Gallery semantic preview uses it to discover and follow such cross-root
/// hosts: when the set of hosts changes, the control raises <see cref="CrossRootsChanged" /> and the preview
/// session collects the roots again and refreshes the highlight. The outline adorner is still added to the
/// <c>AdornerLayer</c> of the window that contains the target, and the preview state
/// (<see cref="SemanticPartPreviewState" />) is set directly on the target, so both already cross roots.
/// The semantic preview subscribes to Avalonia <c>Popup</c> hosts by itself; such controls do not need to
/// implement this interface.
/// </summary>
public interface ISemanticPartCrossRootProvider
{
    /// <summary>
    /// Raised when a cross-root host appears or disappears, for example when a preview window opens, closes or
    /// its surface becomes ready.
    /// </summary>
    event EventHandler? CrossRootsChanged;

    /// <summary>
    /// Returns the visual roots of the cross-root hosts that are currently alive, or an empty collection when
    /// no host is alive.
    /// </summary>
    IReadOnlyList<Visual> GetCrossRoots();
}
