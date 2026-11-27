namespace AtomUI.Toolkits.GalleryBase.SourceCode;

/// <summary>
/// One tab of the code drawer: a piece of source code and the title and language it is shown with.
/// </summary>
/// <remarks>
/// <see cref="TabTitle" /> is the header of the tab and <see cref="Language" /> names the language of
/// <see cref="Text" /> for the code viewer. <see cref="SourceFilePath" /> is the file the text comes from, or
/// <see langword="null" /> when it has none. <see cref="StartLine" /> and <see cref="EndLine" /> are the 1-based
/// numbers of its first and last line in that file. The code drawer does not read these three values.
/// </remarks>
public sealed record ShowCaseCodeSnippet(
    string TabTitle,
    string Language,
    string Text,
    string? SourceFilePath,
    int StartLine,
    int EndLine);
