namespace AtomUI.Desktop.Controls;

public interface ISelectOptionsAsyncLoader
{
    Task<SelectOptionsLoadResult> LoadAsync(object? context, CancellationToken token);
}