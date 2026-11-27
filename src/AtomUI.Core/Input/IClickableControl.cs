using Avalonia.Interactivity;

namespace AtomUI.Input;

/// <summary>
/// A control whose <c>Click</c> can be raised by shared interaction code through <c>RaiseClick</c>.
/// AtomUI.Desktop.Controls implements it on the NavMenu item, and the NavMenu interaction handler calls
/// <c>RaiseClick</c> from <c>RaiseItemInvocation</c> to invoke an item.
/// </summary>
internal interface IClickableControl
{
    event EventHandler<RoutedEventArgs> Click;
    
    void RaiseClick();
    
    /// <summary>
    /// Gets a value indicating whether this control and all its parents are enabled.
    /// </summary>
    bool IsEffectivelyEnabled { get; }
}