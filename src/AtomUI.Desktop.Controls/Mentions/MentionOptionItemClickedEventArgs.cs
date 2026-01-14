using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class MentionOptionItemClickedEventArgs : RoutedEventArgs
{
    public IMentionOption Option { get; }
    
    public MentionOptionItemClickedEventArgs(IMentionOption option)
    {
        Option = option;
    }
}