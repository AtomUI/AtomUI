namespace AtomUI.Localization;

/// <summary>
/// The direction in which the text of a language is laid out. The state of the current language carries it,
/// and the language resource provider maps it to the flow direction of the layout.
/// </summary>
public enum LanguageTextDirection : byte
{
    /// <summary>
    /// Text is laid out from left to right.
    /// </summary>
    LeftToRight = 0,
    /// <summary>
    /// Text is laid out from right to left.
    /// </summary>
    RightToLeft = 1
}
