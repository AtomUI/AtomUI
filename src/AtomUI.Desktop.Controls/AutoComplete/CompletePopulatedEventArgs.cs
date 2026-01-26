namespace AtomUI.Desktop.Controls;

public class CompletePopulatedEventArgs : EventArgs
{
    public IReadOnlyList<IAutoCompleteOption>? Data { get; init; }
    
    public CompletePopulatedEventArgs(IReadOnlyList<IAutoCompleteOption>? data)
    {
        Data = data;
    }
}