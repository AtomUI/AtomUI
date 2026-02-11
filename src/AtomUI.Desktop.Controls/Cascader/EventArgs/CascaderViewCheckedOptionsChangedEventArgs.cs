namespace AtomUI.Desktop.Controls;

public class CascaderViewCheckedOptionsChangedEventArgs : EventArgs
{
    public IList<ICascaderOption>? OldOptions { get; }
    public IList<ICascaderOption>? NewOptions { get; }
    
    public CascaderViewCheckedOptionsChangedEventArgs(IList<ICascaderOption>? oldOptions, IList<ICascaderOption>? newOptions)
    {
        OldOptions = oldOptions;
        NewOptions = newOptions;
    }
}