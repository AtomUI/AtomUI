namespace AtomUI.Desktop.Controls;

/// <summary>
/// Pseudo-class names that <see cref="NavMenu" /> sets on itself for its mode, its collapsed state and its style.
/// </summary>
public static class NavMenuPseudoClass
{
    /// <summary>
    /// Set while <c>Mode</c> is <see cref="NavMenuMode.Inline" />.
    /// </summary>
    public const string InlineMode = ":inline-mode";
    /// <summary>
    /// Set while <c>IsEffectiveInlineCollapsed</c> is true.
    /// </summary>
    public const string InlineCollapsed = ":inline-collapsed";
    /// <summary>
    /// Set while <c>Mode</c> is <see cref="NavMenuMode.Horizontal" />.
    /// </summary>
    public const string HorizontalMode = ":horizontal-mode";
    /// <summary>
    /// Set while <c>Mode</c> is <see cref="NavMenuMode.Vertical" />.
    /// </summary>
    public const string VerticalMode = ":vertical-mode";
    /// <summary>
    /// Set while <c>IsDarkStyle</c> is true.
    /// </summary>
    public const string DarkStyle = ":dark";
    /// <summary>
    /// Set while <c>IsDarkStyle</c> is false, so that exactly one of this class and <see cref="DarkStyle" /> is set.
    /// </summary>
    public const string LightStyle = ":light";
}

/// <summary>
/// Pseudo-class names that <see cref="NavMenuItem" /> sets on itself and on its item header.
/// </summary>
public static class NavMenuItemPseudoClass
{
    /// <summary>
    /// Set on the item while <c>IsTopLevel</c> is true, that is on an entry of the first level of a menu.
    /// </summary>
    public const string TopLevel = ":toplevel";
    /// <summary>
    /// Pseudo-class of a <see cref="NavMenuItem" /> that represents a separator. It is only declared on the item:
    /// no NavMenu code or theme sets or selects it.
    /// </summary>
    public const string Separator = ":separator";
    /// <summary>
    /// Set on the item while its <c>Icon</c> is a <c>PathIcon</c>, and on an item header while its <c>Icon</c> is set.
    /// </summary>
    public const string Icon = ":icon";
}
