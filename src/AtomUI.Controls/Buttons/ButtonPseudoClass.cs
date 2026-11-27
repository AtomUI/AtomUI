namespace AtomUI.Controls;

/// <summary>
/// The pseudo-classes that buttons raise from their state, for the <c>PseudoClasses</c> attribute of button classes
/// and for style selectors.
/// </summary>
public static class ButtonPseudoClass
{
    /// <summary>
    /// Set while a button shows an icon and no content. The themes pad such a button with the icon-only padding and
    /// drop the icon margin.
    /// </summary>
    public const string IconOnly = ":icononly";
    /// <summary>
    /// Set while <c>IsLoading</c> is <see langword="true" />. The themes hide the icon, show a spinning loading icon
    /// and lower the opacity of the button.
    /// </summary>
    public const string Loading = ":loading";
    /// <summary>
    /// Set while <c>IsDanger</c> is <see langword="true" />. The theme of the hyperlink button paints such a button
    /// in the error color.
    /// </summary>
    public const string IsDanger = ":danger";
    /// <summary>
    /// Set while <c>ButtonType</c> is <c>Default</c>.
    /// </summary>
    public const string DefaultType = ":default";
    /// <summary>
    /// Set while <c>ButtonType</c> is <c>Dashed</c>.
    /// </summary>
    public const string DashedType = ":dashed";
    /// <summary>
    /// Set while <c>ButtonType</c> is <c>Primary</c>.
    /// </summary>
    public const string PrimaryType = ":primary";
    /// <summary>
    /// Set while <c>ButtonType</c> is <c>Link</c>.
    /// </summary>
    public const string LinkType = ":link";
    /// <summary>
    /// Set while <c>ButtonType</c> is <c>Text</c>.
    /// </summary>
    public const string TextType = ":text";
    /// <summary>
    /// Set while <c>IsVisited</c> of a hyperlink button is <see langword="true" />, which happens after its
    /// <c>NavigateUri</c> was opened. The default theme does not style it.
    /// </summary>
    public const string Visited = ":visited";
}