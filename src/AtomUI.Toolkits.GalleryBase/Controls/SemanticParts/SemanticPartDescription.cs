namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Describes one Semantic Part of a preview: the explanatory text and the code snippet shown for the
/// part.
/// </summary>
public sealed class SemanticPartDescription
{
    /// <summary>
    /// Path of the part this entry describes, equal to the path of its <c>[SemanticPart]</c> declaration.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The type of the owner the part belongs to. It tells apart parts with the same path when a single
    /// preview covers several owners (for example <c>root</c> of Message exists on both MessageCard and
    /// WindowMessageManager). It can be omitted for a single-owner preview.
    /// </summary>
    public Type? OwnerType { get; set; }

    /// <summary>
    /// Text shown for the part in the part list. When it is <see langword="null" />, or no entry describes
    /// the part, the preview shows a generated fallback description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Code snippet shown for the part. When it is <see langword="null" />, the preview builds a default
    /// snippet from the part's descriptor and owner type.
    /// </summary>
    public string? CodeSnippet { get; set; }
}
