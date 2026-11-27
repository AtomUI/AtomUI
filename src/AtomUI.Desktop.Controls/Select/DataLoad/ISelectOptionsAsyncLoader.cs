namespace AtomUI.Desktop.Controls;

/// <summary>
/// Loads the options of a <see cref="Select" /> asynchronously. Set it on <see cref="Select.OptionsLoader" />;
/// the select then calls it when the dropdown is requested and no load has succeeded yet.
/// </summary>
public interface ISelectOptionsAsyncLoader
{
    /// <summary>
    /// Loads one batch of options. The select applies its <see cref="Select.AsyncLoadTimeout" /> to the task.
    /// </summary>
    /// <param name="context">
    /// The <see cref="Select.OptionsAsyncLoadContext" /> of the select, which tells the loader what to request.
    /// </param>
    /// <param name="token">
    /// Canceled when the select abandons the load, which it does when it is detached from the visual tree.
    /// </param>
    /// <returns>
    /// The outcome of the load. When the task completes with a result, the select takes
    /// <see cref="SelectOptionsLoadResult.Data" /> as its <see cref="Select.OptionsSource" /> and reports the
    /// result through <see cref="Select.OptionsLoaded" />. When the task throws or the timeout elapses, the select
    /// reports a failed result instead.
    /// </returns>
    Task<SelectOptionsLoadResult> LoadAsync(object? context, CancellationToken token);
}