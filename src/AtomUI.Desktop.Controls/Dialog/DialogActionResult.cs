namespace AtomUI.Desktop.Controls;

internal class DialogActionResult : IDialogActionResult
{
    public object? Result { get; }

    public DialogActionResult(object? result)
    {
        Result = result;
    }
}