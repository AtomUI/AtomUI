using Avalonia;

namespace AtomUI.Theme;

/// <summary>
/// Carries the state through which a Gallery semantic preview reveals a part that is transparent or hidden at
/// rest. When the preview activates such a part, marked <c>RestHidden</c>, it sets
/// <see cref="IsPreviewTargetProperty" /> on the resolved target nodes. The ControlTheme of each control responds
/// with a property-condition selector and decides itself how to reveal the part, for example by raising the
/// opacity of a hover mask to 1.
/// </summary>
/// <remarks>
/// The contract lives in AtomUI.Core because its writer, the GalleryBase highlight session, and its readers, the
/// control ControlThemes, are in different assemblies and the dependency direction requires the shared contract to
/// sit below both. A pseudo-class cannot be set from outside the control because pseudo-classes are protected, so
/// the state is an attached property instead. The state changes only how a part looks in the preview; it does not
/// change how the control behaves at run time.
/// </remarks>
public sealed class SemanticPartPreviewState
{
    private SemanticPartPreviewState()
    {
    }

    /// <summary>
    /// Marks an element as a node that the preview of the active semantic part must reveal. The Gallery highlight
    /// session sets it to <see langword="true" /> on every target of a <c>RestHidden</c> part while that part is
    /// previewed, and resets it to <see langword="false" /> when the highlight ends. Only property-condition
    /// selectors in the ControlThemes read the value, and only on the element that carries it, because the value is
    /// not inherited. The default, <see langword="false" />, leaves the part in its normal appearance.
    /// </summary>
    public static readonly AttachedProperty<bool> IsPreviewTargetProperty =
        AvaloniaProperty.RegisterAttached<SemanticPartPreviewState, Visual, bool>(
            "IsPreviewTarget");

    /// <summary>
    /// Returns whether <paramref name="visual" /> is the preview target of the active semantic part.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="visual" /> is <see langword="null" />.
    /// </exception>
    public static bool GetIsPreviewTarget(Visual visual)
    {
        ArgumentNullException.ThrowIfNull(visual);
        return visual.GetValue(IsPreviewTargetProperty);
    }

    /// <summary>
    /// Sets whether <paramref name="visual" /> is the preview target of the active semantic part.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="visual" /> is <see langword="null" />.
    /// </exception>
    public static void SetIsPreviewTarget(Visual visual, bool value)
    {
        ArgumentNullException.ThrowIfNull(visual);
        visual.SetValue(IsPreviewTargetProperty, value);
    }
}
