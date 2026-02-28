using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class SubmitButton : Button
{
    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> SubmitEvent =
        RoutedEvent.Register<SubmitButton, RoutedEventArgs>(nameof(Submit), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Submit
    {
        add => AddHandler(SubmitEvent, value);
        remove => RemoveHandler(SubmitEvent, value);
    }
    #endregion

    protected override void OnClick()
    {
        base.OnClick();
        RaiseEvent(new RoutedEventArgs(SubmitEvent));
    }
}