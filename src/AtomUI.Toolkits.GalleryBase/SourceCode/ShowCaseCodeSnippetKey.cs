namespace AtomUI.Toolkits.GalleryBase.SourceCode;

/// <summary>
/// Identifies the source code of one showcase item.
/// </summary>
/// <remarks>
/// <see cref="ViewTypeName" /> is the full name of the showcase page type, <see cref="PanelKey" /> the name of
/// the enclosing <c>ShowCasePanel</c> and <see cref="ItemIndex" /> the zero-based position of the
/// <c>ShowCaseItem</c> among the <c>ShowCaseItem</c> children of that panel.
/// <see cref="SourceKey" /> selects one of several snippet groups of the same item. A group that declares no
/// source key matches <see langword="null" /> or an empty value; a group with a source key matches only that value.
/// </remarks>
public readonly record struct ShowCaseCodeSnippetKey(
    string ViewTypeName,
    string PanelKey,
    int ItemIndex,
    string? SourceKey = null);
