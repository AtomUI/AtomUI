namespace AtomUI.Controls;

/// <summary>
/// Describes why an image load failed.
/// </summary>
/// <param name="Code">
/// The failure category.
/// </param>
/// <param name="Message">
/// A fixed description that is safe to show and names neither the source nor any credential.
/// </param>
/// <param name="HttpStatus">
/// The HTTP status code when <paramref name="Code" /> is
/// <see cref="ImageLoadErrorCode.HttpStatus" />; otherwise <see langword="null" />.
/// </param>
/// <param name="SourceDisplayName">
/// The <see cref="ImageSource.DisplayName" /> of the source that failed, when it has one.
/// </param>
/// <param name="Exception">
/// The exception that was caught as the cause, when there is one.
/// </param>
public sealed record ImageLoadError(
    ImageLoadErrorCode Code,
    string Message,
    int? HttpStatus = null,
    string? SourceDisplayName = null,
    Exception? Exception = null);
