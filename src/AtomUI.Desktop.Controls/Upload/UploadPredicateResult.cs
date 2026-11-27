namespace AtomUI.Desktop.Controls;

/// <summary>
/// What an upload does with an added file. The handler of the <c>UploadTaskAboutToScheduling</c> event chooses
/// it through the <c>Result</c> of the event arguments.
/// </summary>
public enum UploadPredicateResult
{
    /// <summary>
    /// Queues the file for upload, but only while <see cref="Upload.AutoUpload" /> is <see langword="true" />;
    /// otherwise its item is left untouched.
    /// </summary>
    Schedule,
    /// <summary>
    /// Does not upload the file. Its item is marked as failed, with the cancel reason of the event as its error
    /// message.
    /// </summary>
    Cancel,
    /// <summary>
    /// Does not upload the file and leaves it in the task list. Its item is marked as failed, with the cancel
    /// reason of the event as its error message.
    /// </summary>
    CancelWithInTaskList
}