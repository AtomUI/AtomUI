namespace AtomUI.Toolkits.GalleryBase.SourceCode;

/// <summary>
/// The source code of one showcase item, shown as the tabs of the code drawer.
/// </summary>
/// <remarks>
/// The drawer uses <see cref="Title" /> as its title when the source code request carries none.
/// <see cref="Snippets" /> holds one entry per tab.
/// </remarks>
public sealed record ShowCaseCodeSnippetGroup(
    string Title,
    IReadOnlyList<ShowCaseCodeSnippet> Snippets);
