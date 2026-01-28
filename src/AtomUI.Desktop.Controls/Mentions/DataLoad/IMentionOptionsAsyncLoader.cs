namespace AtomUI.Desktop.Controls.DataLoad;

public interface IMentionOptionsAsyncLoader
{
    Task<MentionOptionsLoadResult> LoadAsync(string? context, CancellationToken token);
}