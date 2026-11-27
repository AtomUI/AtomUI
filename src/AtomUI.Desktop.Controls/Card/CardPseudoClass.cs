namespace AtomUI.Desktop.Controls;

/// <summary>
/// Pseudo-class names of the card controls, for use in theme selectors.
/// </summary>
public static class CardPseudoClass
{
    /// <summary>
    /// Set on a <see cref="Card" /> while its header area has nothing to show: <c>Header</c>, <c>Extra</c> and
    /// their templates are all <see langword="null" />. The card theme then hides the header frame.
    /// </summary>
    public const string Headerless = ":headerless";
}