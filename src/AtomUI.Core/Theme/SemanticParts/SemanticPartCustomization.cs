namespace AtomUI.Theme;

/// <summary>
/// The ways in which an application can restyle a semantic part.
/// </summary>
public enum SemanticPartCustomization
{
    /// <summary>
    /// The implicit root part, which is the control itself. The source generator adds it; a declaration cannot
    /// use it.
    /// </summary>
    Root,
    /// <summary>
    /// The part is restyled through its dedicated style class only. This is the default of a declared part.
    /// </summary>
    Selector,
    /// <summary>
    /// The part is restyled through its dedicated style class and through a
    /// <see cref="Avalonia.Styling.ControlTheme" /> that the control exposes in the property named by
    /// <see cref="SemanticPartAttribute.ThemePropertyName" />.
    /// </summary>
    SelectorAndTheme
}
