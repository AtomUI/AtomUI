namespace AtomUI.Desktop.Controls;

/// <summary>
/// Pseudo-class names that mirror the style variant of an add-on decorated box. The box sets them from its
/// <c>StyleVariant</c>; themes select them to style one variant.
/// </summary>
public class AddOnDecoratedBoxPseudoClass
{
    /// <summary>
    /// Set while the style variant is <c>Outlined</c>.
    /// </summary>
    public const string Outline = ":outline";
    /// <summary>
    /// Set while the style variant is <c>Filled</c>.
    /// </summary>
    public const string Filled = ":filled";
    /// <summary>
    /// Set while the style variant is <c>Borderless</c>.
    /// </summary>
    public const string Borderless = ":borderless";
    /// <summary>
    /// Set while the style variant is <c>Underlined</c>.
    /// </summary>
    public const string Underlined = ":underlined";
}
