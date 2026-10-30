using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public enum PopupConfirmStatus
{
    Info,
    Warning,
    Error
}

public partial class PopupConfirm : FlyoutHost
{
    #region 公共属性属性

    public static readonly StyledProperty<string> OkTextProperty =
        AvaloniaProperty.Register<PopupConfirm, string>(nameof(OkText));

    public static readonly StyledProperty<string> CancelTextProperty =
        AvaloniaProperty.Register<PopupConfirm, string>(nameof(CancelText));

    public static readonly StyledProperty<ButtonType> OkButtonTypeProperty =
        AvaloniaProperty.Register<PopupConfirm, ButtonType>(nameof(OkButtonType), ButtonType.Primary);

    public static readonly StyledProperty<bool> IsShowCancelButtonProperty =
        AvaloniaProperty.Register<PopupConfirm, bool>(nameof(IsShowCancelButton), true);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<PopupConfirm, string>(nameof(Title));

    public static readonly StyledProperty<object?> ConfirmContentProperty =
        AvaloniaProperty.Register<PopupConfirm, object?>(nameof(ConfirmContent));

    public static readonly StyledProperty<IDataTemplate?> ConfirmContentTemplateProperty =
        AvaloniaProperty.Register<PopupConfirm, IDataTemplate?>(nameof(ConfirmContentTemplate));

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<PopupConfirm, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<PopupConfirmStatus> ConfirmStatusProperty =
        AvaloniaProperty.Register<PopupConfirm, PopupConfirmStatus>(nameof(ConfirmStatus),
            PopupConfirmStatus.Warning);

    public static readonly RoutedEvent<RoutedEventArgs> CancelledEvent =
        RoutedEvent.Register<PopupConfirm, RoutedEventArgs>(nameof(Cancelled), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> ConfirmedEvent =
        RoutedEvent.Register<PopupConfirm, RoutedEventArgs>(nameof(Confirmed), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<PopupConfirmClickEventArgs> PopupClickEvent =
        RoutedEvent.Register<PopupConfirm, PopupConfirmClickEventArgs>(nameof(PopupClick), RoutingStrategies.Bubble);

    public string OkText
    {
        get => GetValue(OkTextProperty);
        set => SetValue(OkTextProperty, value);
    }

    public string CancelText
    {
        get => GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    public ButtonType OkButtonType
    {
        get => GetValue(OkButtonTypeProperty);
        set => SetValue(OkButtonTypeProperty, value);
    }

    public bool IsShowCancelButton
    {
        get => GetValue(IsShowCancelButtonProperty);
        set => SetValue(IsShowCancelButtonProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    [DependsOn(nameof(ConfirmContentTemplate))]
    public object? ConfirmContent
    {
        get => GetValue(ConfirmContentProperty);
        set => SetValue(ConfirmContentProperty, value);
    }

    public IDataTemplate? ConfirmContentTemplate
    {
        get => GetValue(ConfirmContentTemplateProperty);
        set => SetValue(ConfirmContentTemplateProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public PopupConfirmStatus ConfirmStatus
    {
        get => GetValue(ConfirmStatusProperty);
        set => SetValue(ConfirmStatusProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? Cancelled
    {
        add => AddHandler(CancelledEvent, value);
        remove => RemoveHandler(CancelledEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? Confirmed
    {
        add => AddHandler(ConfirmedEvent, value);
        remove => RemoveHandler(ConfirmedEvent, value);
    }

    public event EventHandler<PopupConfirmClickEventArgs>? PopupClick
    {
        add => AddHandler(PopupClickEvent, value);
        remove => RemoveHandler(PopupClickEvent, value);
    }

    #endregion

    public PopupConfirm()
    {
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Flyout ??= new PopupConfirmFlyout(this);
    }
}

public class PopupConfirmClickEventArgs : RoutedEventArgs
{
    public bool IsConfirmed { get; }

    public PopupConfirmClickEventArgs(RoutedEvent? routedEvent, bool isConfirmed)
        : base(routedEvent)
    {
        IsConfirmed = isConfirmed;
    }
}