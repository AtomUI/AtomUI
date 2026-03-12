using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class TransferSelectActionEventArgs : RoutedEventArgs
{
    public TransferSelectAction Action { get; }

    public TransferSelectActionEventArgs(TransferSelectAction action)
    {
        Action = action;
    }
}