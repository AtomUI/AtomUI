namespace AtomUI.Theme;

/// <summary>
/// How many elements a semantic part has in one control instance. The source generator compares it with the
/// number of elements that carry the part's selector class in the theme template.
/// </summary>
public enum SemanticPartCardinality
{
    /// <summary>
    /// Exactly one element.
    /// </summary>
    Single,
    /// <summary>
    /// Zero or one element; the theme template may omit the part.
    /// </summary>
    Optional,
    /// <summary>
    /// One or more elements; the part is repeated, for example one per date cell.
    /// </summary>
    Multiple
}
