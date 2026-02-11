using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class MentionOptionClickedEventArgs : RoutedEventArgs
{
    public IMentionOption Option { get; }
    
    public MentionOptionClickedEventArgs(IMentionOption option)
    {
        Option = option;
    }
}