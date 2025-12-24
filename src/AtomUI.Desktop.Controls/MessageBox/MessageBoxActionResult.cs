namespace AtomUI.Desktop.Controls;

internal class MessageBoxActionResult : IMessageBoxActionResult
{
    public object? Result { get; }

    public MessageBoxActionResult(object? result)
    {
        Result = result;
    }
}