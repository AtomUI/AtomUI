using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the <see cref="IMentionOption" /> of a mention candidate that was clicked.
/// </summary>
/// <remarks>
/// No control of the library raises an event with these arguments.
/// </remarks>
public class MentionOptionClickedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// The clicked option.
    /// </summary>
    public IMentionOption Option { get; }
    
    /// <summary>
    /// Creates the arguments for a click on an option.
    /// </summary>
    /// <param name="option">
    /// The clicked option; stored as <see cref="Option" />.
    /// </param>
    public MentionOptionClickedEventArgs(IMentionOption option)
    {
        Option = option;
    }
}