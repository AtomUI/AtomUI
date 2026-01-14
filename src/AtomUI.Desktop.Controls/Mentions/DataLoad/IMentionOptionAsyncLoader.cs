namespace AtomUI.Desktop.Controls.DataLoad;

public interface IMentionOptionAsyncLoader
{
    Task<MentionOptionLoadResult> LoadAsync(string? context, CancellationToken token);
}