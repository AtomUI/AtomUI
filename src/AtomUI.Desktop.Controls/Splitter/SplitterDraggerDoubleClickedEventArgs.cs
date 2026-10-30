namespace AtomUI.Desktop.Controls;

public class SplitterDraggerDoubleClickedEventArgs : EventArgs
{
    public SplitterDraggerDoubleClickedEventArgs(int handleIndex)
    {
        HandleIndex = handleIndex;
    }

    public int HandleIndex { get; }
}
