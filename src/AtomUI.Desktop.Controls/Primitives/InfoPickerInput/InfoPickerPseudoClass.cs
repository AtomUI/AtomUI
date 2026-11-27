namespace AtomUI.Desktop.Controls.Primitives;

/// <summary>
/// Pseudo-class names that an info picker input sets to reflect its open, choosing and previewing states.
/// </summary>
public static class InfoPickerPseudoClass
{
    /// <summary>
    /// Set on the picker input while its picker flyout is open.
    /// </summary>
    public const string FlyoutOpen = ":flyout-open";
    /// <summary>
    /// Set on the picker input while its picker panel reports that a choice is still in progress.
    /// </summary>
    public const string Choosing = ":choosing";
    /// <summary>
    /// Set on the picker input while its input text shows a hover preview rather than the candidate; the preview is
    /// dimmed.
    /// </summary>
    public const string Previewing = ":previewing";
}