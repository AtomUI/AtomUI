using Avalonia.Interactivity;

namespace AtomUI.Toolkits.GalleryBase.SourceCode;

/// <summary>
/// Carries the request of a showcase item to show its source code.
/// </summary>
/// <remarks>
/// <c>ShowCaseItem</c> raises the request when the user clicks its source button. The code drawer host resolves
/// <see cref="Key" /> through <see cref="IShowCaseCodeSnippetProvider" /> and opens the code drawer.
/// </remarks>
public sealed class ShowCaseSourceCodeRequestedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the arguments for a source code request.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event that carries these arguments.
    /// </param>
    /// <param name="key">
    /// Identifies the showcase item whose source code is requested.
    /// </param>
    /// <param name="title">
    /// The title requested for the code drawer. When it is null or white space, the drawer uses the title of the
    /// snippet group, and a default title when no group is found.
    /// </param>
    public ShowCaseSourceCodeRequestedEventArgs(RoutedEvent routedEvent,
                                                ShowCaseCodeSnippetKey key,
                                                string? title)
        : base(routedEvent)
    {
        Key   = key;
        Title = title;
    }

    /// <summary>
    /// Identifies the showcase item whose source code is requested.
    /// </summary>
    public ShowCaseCodeSnippetKey Key { get; }

    /// <summary>
    /// The title requested for the code drawer, or <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// When this value is null or white space, the drawer uses the title of the snippet group, and a default title
    /// when no group is found.
    /// </remarks>
    public string? Title { get; }
}
