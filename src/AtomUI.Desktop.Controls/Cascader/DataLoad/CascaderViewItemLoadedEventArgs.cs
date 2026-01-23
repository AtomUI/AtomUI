namespace AtomUI.Desktop.Controls;

public class CascaderViewItemLoadedEventArgs : EventArgs
{
    public CascaderItemLoadResult Result { get; }
    public CascaderViewItem Target { get; }
    
    public CascaderViewItemLoadedEventArgs(CascaderViewItem target, CascaderItemLoadResult result)
    {
        Target = target;
        Result = result;
    }
}