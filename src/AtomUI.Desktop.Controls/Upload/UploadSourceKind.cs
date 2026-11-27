namespace AtomUI.Desktop.Controls;

/// <summary>
/// Whether an upload trigger or a drop zone picks files or folders when the user clicks it.
/// </summary>
public enum UploadSourceKind
{
    /// <summary>
    /// Clicking opens the file picker; the picked files are added to the upload.
    /// </summary>
    Files,
    /// <summary>
    /// Clicking opens the folder picker; the files directly inside the picked folders are added to the upload.
    /// </summary>
    Directories
}
