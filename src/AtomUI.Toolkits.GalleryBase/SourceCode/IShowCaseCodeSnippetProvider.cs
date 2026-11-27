namespace AtomUI.Toolkits.GalleryBase.SourceCode;

/// <summary>
/// Supplies the source code that the code drawer shows for a showcase item.
/// </summary>
/// <remarks>
/// Assign it to <c>SnippetProvider</c> of the Gallery options. The source button of a showcase item opens the
/// code drawer only when a provider is assigned and <c>SourceCodeDisplay.CanShowSourceCode</c> is set.
/// </remarks>
public interface IShowCaseCodeSnippetProvider
{
    /// <summary>
    /// Looks up the snippet group of a showcase item.
    /// </summary>
    /// <param name="key">
    /// Identifies the showcase item and, through its source key, the group to return.
    /// </param>
    /// <param name="group">
    /// Receives the matching snippet group, or <see langword="null" /> when no group matches.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when a snippet group matches <paramref name="key" />, with <paramref name="group" />
    /// holding it. Otherwise <see langword="false" />, and <paramref name="group" /> is <see langword="null" />.
    /// </returns>
    bool TryGetSnippetGroup(ShowCaseCodeSnippetKey key, out ShowCaseCodeSnippetGroup group);
}
