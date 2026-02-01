namespace AtomUI.Desktop.Controls;

public class CompletePopulatedEventArgs : EventArgs
{
    public IList<IAutoCompleteOption>? Data { get; init; }
    
    public CompletePopulatedEventArgs(IList<IAutoCompleteOption>? data)
    {
        Data = data;
    }
}