using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A form button that raises a bubbling <see cref="Reset" /> event when it is clicked. The
/// <see cref="Form" /> that contains the button handles the event with <see cref="Form.Reset" />.
/// </summary>
public class ResetButton : Button
{
    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="Reset" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ResetEvent =
        RoutedEvent.Register<ResetButton, RoutedEventArgs>(nameof(Reset), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when the button is clicked, after the <c>Click</c> event. The event bubbles up the element tree.
    /// </summary>
    /// <remarks>
    /// A <see cref="Form" /> that contains the button handles it: the form calls <see cref="Form.Reset" />, then
    /// marks the event as handled. Handlers on the button and on the elements between the button and the form
    /// run before the reset. Handlers above the form do not receive it unless they are registered for handled
    /// events.
    /// </remarks>
    public event EventHandler<RoutedEventArgs>? Reset
    {
        add => AddHandler(ResetEvent, value);
        remove => RemoveHandler(ResetEvent, value);
    }
    #endregion
    
    /// <summary>
    /// Calls the base method, then raises <see cref="Reset" />.
    /// </summary>
    /// <remarks>
    /// An override that skips the base method must raise <see cref="Reset" /> itself.
    /// </remarks>
    protected override void OnClick()
    {
        base.OnClick();
        RaiseEvent(new RoutedEventArgs(ResetEvent));
    }
}