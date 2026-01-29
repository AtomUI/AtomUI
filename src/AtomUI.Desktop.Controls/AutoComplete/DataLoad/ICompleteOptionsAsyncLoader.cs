namespace AtomUI.Desktop.Controls;

public interface ICompleteOptionsAsyncLoader
{
    Task<CompleteOptionsLoadResult> LoadAsync(string? context, CancellationToken token);
}