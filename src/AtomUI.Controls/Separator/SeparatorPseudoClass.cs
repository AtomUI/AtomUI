namespace AtomUI.Controls;

/// <summary>
/// Pseudo-class names that a separator exposes to style selectors.
/// </summary>
public static class SeparatorPseudoClass
{
    /// <summary>
    /// Set while the separator has a non-empty title, whatever its orientation. Select it with <c>:has-title</c>.
    /// </summary>
    public const string HasTitleText = ":has-title";
}