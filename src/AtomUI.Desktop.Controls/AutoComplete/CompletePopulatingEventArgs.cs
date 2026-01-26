using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

public class CompletePopulatingEventArgs : CancelEventArgs
{
    public string? Predicate { get; }

    public CompletePopulatingEventArgs(string? predicate)
    {
        Predicate = predicate;
    }
}