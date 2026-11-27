namespace AtomUI.Desktop.Controls;

/// <summary>
/// How an upload presents its file list and, for the picture-card and picture-circle lists, its trigger.
/// </summary>
public enum UploadListType
{
    /// <summary>
    /// Rows with the file name, a status icon and a progress line.
    /// </summary>
    Text,
    /// <summary>
    /// Rows with an image preview of the file.
    /// </summary>
    Picture,
    /// <summary>
    /// Items as picture cards of <c>PictureCardSize</c>; the trigger is a card of the same size.
    /// </summary>
    PictureCard,
    /// <summary>
    /// Items and trigger as in <see cref="PictureCard" />, but round.
    /// </summary>
    PictureCircle
}