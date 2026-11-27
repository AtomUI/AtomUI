namespace AtomUI.Desktop.Controls.DataLoad;

/// <summary>
/// Loads the candidates of a <see cref="Mentions" /> control asynchronously.
/// Assign an implementation to <see cref="Mentions.OptionsAsyncLoader" />; without a loader the control filters
/// <see cref="Mentions.OptionsSource" /> itself.
/// </summary>
public interface IMentionOptionsAsyncLoader
{
    /// <summary>
    /// Loads the candidates for the text typed after a trigger character.
    /// </summary>
    /// <param name="context">
    /// The text typed after the trigger character: an empty string right after the trigger character, or
    /// <see langword="null" /> when the popup opens without a trigger character.
    /// </param>
    /// <param name="token">
    /// Canceled when a newer request supersedes this one or when <see cref="Mentions.AsyncLoadTimeout" /> elapses.
    /// </param>
    /// <returns>
    /// A task that completes when the load finished. The control takes <see cref="MentionOptionsLoadResult.Data" />
    /// as its candidates only when the result reports success; a request that was superseded, canceled or timed
    /// out leaves the candidates unchanged.
    /// </returns>
    Task<MentionOptionsLoadResult> LoadAsync(string? context, CancellationToken token);
}