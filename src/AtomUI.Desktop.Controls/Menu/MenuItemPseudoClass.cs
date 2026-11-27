namespace AtomUI.Desktop.Controls;

/// <summary>
/// Pseudo-class names of <see cref="MenuItem" />.
/// </summary>
public static class MenuItemPseudoClass
{
    /// <summary>
    /// Set on a menu item while its <c>IsTopLevel</c> is true, that is on an entry of the first level of a
    /// <see cref="Menu" />, so that styles can target menu bar entries with <c>:toplevel</c>.
    /// </summary>
    public const string TopLevel = ":toplevel";
}
