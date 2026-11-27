namespace AtomUI.Desktop.Controls;

/// <summary>
/// Pseudo-classes that a <see cref="BreadcrumbItem" /> sets for its icon and for its position in a
/// <see cref="Breadcrumb" />. The default theme does not style them; it selects the last item through the
/// item's <c>IsLast</c> property.
/// </summary>
public static class BreadcrumbPseudoClass
{
    /// <summary>
    /// Pseudo-class of an item whose <see cref="BreadcrumbItem.Icon" /> is not <see langword="null" />.
    /// </summary>
    public const string HasIcon = ":has-icon";
    /// <summary>
    /// Pseudo-class of the last item of a breadcrumb, which has no separator after it.
    /// </summary>
    public const string IsLast = ":is-last";
}