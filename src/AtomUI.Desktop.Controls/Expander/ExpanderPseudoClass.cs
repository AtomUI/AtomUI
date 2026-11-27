namespace AtomUI.Desktop.Controls;

/// <summary>
/// Pseudo-class names that the Expander control exposes to theme selectors.
/// </summary>
public static class ExpanderPseudoClass
{
    /// <summary>
    /// Set while the expander is expanded.
    /// </summary>
    public const string Expanded = ":expanded";
    /// <summary>
    /// Set when the content expands upward, above the header.
    /// </summary>
    public const string ExpandUp = ":up";
    /// <summary>
    /// Set when the content expands downward, below the header.
    /// </summary>
    public const string ExpandDown = ":down";
    /// <summary>
    /// Set when the content expands to the left of the header.
    /// </summary>
    public const string ExpandLeft = ":left";
    /// <summary>
    /// Set when the content expands to the right of the header.
    /// </summary>
    public const string ExpandRight = ":right";
    
    /// <summary>
    /// Set while the HeaderPadding of the expander has an explicit value. Only while it is unset does the theme
    /// apply the size-based header padding tokens.
    /// </summary>
    public const string CustomHeaderPadding = ":custom-header-padding";
    /// <summary>
    /// Set while the ContentPadding of the expander has an explicit value. Only while it is unset does the theme
    /// apply the size-based content padding tokens.
    /// </summary>
    public const string CustomContentPadding = ":custom-content-padding";
}