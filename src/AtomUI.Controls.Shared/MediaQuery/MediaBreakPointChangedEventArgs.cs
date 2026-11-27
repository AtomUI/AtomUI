namespace AtomUI.Controls;

/// <summary>
/// Carries the breakpoint that a control implementing <see cref="IMediaBreakAwareControl" /> has switched to.
/// The breakpoint before the change is not included.
/// </summary>
public class MediaBreakPointChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes the arguments with the new breakpoint of the control.
    /// </summary>
    /// <param name="breakPoint">
    /// The breakpoint that the control has switched to.
    /// </param>
    public MediaBreakPointChangedEventArgs(MediaBreakPoint breakPoint)
    {
        MediaBreakPoint = breakPoint;
    }
    
    /// <summary>
    /// Gets the breakpoint after the change.
    /// </summary>
    public MediaBreakPoint MediaBreakPoint { get; }
}