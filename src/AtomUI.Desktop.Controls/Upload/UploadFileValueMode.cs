namespace AtomUI.Desktop.Controls;

/// <summary>
/// What an upload reports as its form value: its file items or their upload results, given as an array.
/// </summary>
public enum UploadFileValueMode
{
    /// <summary>
    /// Every file item in the list, whatever its status.
    /// </summary>
    AllFiles,
    /// <summary>
    /// Only the file items whose upload succeeded. This is the default.
    /// </summary>
    SuccessfulFiles,
    /// <summary>
    /// The upload results of the files whose upload succeeded. Files without a result are left out.
    /// </summary>
    Results
}
