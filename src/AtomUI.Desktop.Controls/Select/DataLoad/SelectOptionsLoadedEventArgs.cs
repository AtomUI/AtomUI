namespace AtomUI.Desktop.Controls;

public class SelectOptionsLoadedEventArgs
{
    public object? Context { get; }
    public SelectOptionsLoadResult Result { get; }
    
    public SelectOptionsLoadedEventArgs(object? context, SelectOptionsLoadResult result)
    {
        Context = context;
        Result  = result;
    }
}