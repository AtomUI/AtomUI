namespace AtomUI.Desktop.Controls;

public interface ICompleteOptionsAsyncLoader
{
    Task<CompleteOptionLoadResult> LoadAsync(string? context, CancellationToken token);
}