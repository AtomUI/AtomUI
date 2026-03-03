namespace AtomUI.Desktop.Controls;

public class CascaderOptionsSelectedChangedEventArgs : EventArgs
{
    public IList<ICascaderOption>? OldOptions { get; }
    public IList<ICascaderOption>? NewOptions { get; }
    
    public CascaderOptionsSelectedChangedEventArgs(IList<ICascaderOption>? oldOptions, IList<ICascaderOption>? newOptions)
    {
        OldOptions = oldOptions;
        NewOptions = newOptions;
    }
}