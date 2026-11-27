namespace AtomUI.Desktop.Controls.DataLoad;

/// <summary>
/// Loads the children of a cascader option on demand, for options whose children are not known before the option
/// is expanded. Set an instance on the <c>DataLoader</c> property of the cascader to enable the loading.
/// </summary>
public interface ICascaderItemDataLoader
{
    /// <summary>
    /// Loads the children of an option that is being expanded, when the option has no children, is not a leaf and
    /// has not been loaded yet.
    /// </summary>
    /// <param name="targetNode">
    /// The option being expanded.
    /// </param>
    /// <param name="token">
    /// Canceled when the load takes longer than the <c>AsyncLoadTimeout</c> of the view, or when the view cancels
    /// its running loads because its items source changed or it was detached from the visual tree.
    /// </param>
    /// <returns>
    /// The children of the option, or a failure. Completion means that the load has ended: the view then raises its
    /// <c>ItemAsyncLoaded</c> event with the result and appends a successful
    /// <see cref="CascaderItemLoadResult.Data" /> to the children of the option. A load that throws or is canceled
    /// yields no result for the view; it reports the failure itself.
    /// </returns>
    /// <remarks>
    /// The method is called on the UI thread, so work before its first await blocks the UI.
    /// </remarks>
    Task<CascaderItemLoadResult> LoadAsync(ICascaderOption targetNode, CancellationToken token);
}