using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class ResetButton : Button
{
    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ResetEvent =
        RoutedEvent.Register<ResetButton, RoutedEventArgs>(nameof(Reset), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Reset
    {
        add => AddHandler(ResetEvent, value);
        remove => RemoveHandler(ResetEvent, value);
    }
    #endregion
    
    protected override void OnClick()
    {
        base.OnClick();
        RaiseEvent(new RoutedEventArgs(ResetEvent));
    }
}