namespace AtomUI.Desktop.Controls;

/// <summary>
/// The pseudo-class names that <c>UploadDropZone</c> sets while the user drags over it and while a drop is
/// being processed.
/// </summary>
public static class UploadDropZonePseudoClass
{
    /// <summary>
    /// Set while a drag is over the zone, whether the dragged data is accepted or rejected.
    /// </summary>
    public const string DragOver = ":drag-over";
    /// <summary>
    /// Set while a drag is over the zone and the dragged data is accepted.
    /// </summary>
    public const string DragAccepting = ":drag-accepting";
    /// <summary>
    /// Set while a drag is over the zone and the dragged data is rejected.
    /// </summary>
    public const string DragRejecting = ":drag-rejecting";
    /// <summary>
    /// Set from the moment an accepted drop is handed to the upload until the dropped items are processed.
    /// </summary>
    public const string DropProcessing = ":drop-processing";
}
