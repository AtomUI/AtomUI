using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The status of a <see cref="PopupConfirm" /> popup. It selects the color of the icon of the popup.
/// </summary>
public enum PopupConfirmStatus
{
    /// <summary>
    /// Information. The icon of the popup uses the primary color.
    /// </summary>
    Info,
    /// <summary>
    /// Warning. The icon of the popup uses the warning color.
    /// </summary>
    Warning,
    /// <summary>
    /// Error. The icon of the popup uses the error color.
    /// </summary>
    Error
}

/// <summary>
/// A flyout host whose popup asks the user to confirm or cancel with an icon, a title, a description, an OK
/// button and a Cancel button. The popup opens at the content of this control; the properties of this control
/// provide what it shows. Clicking a button raises <see cref="Confirmed" /> or <see cref="Cancelled" /> on this
/// control and then starts closing the popup.
/// </summary>
public partial class PopupConfirm : FlyoutHost
{
    #region 公共属性属性

    /// <summary>
    /// Identifies the <see cref="OkText" /> property.
    /// </summary>
    public static readonly StyledProperty<string> OkTextProperty =
        AvaloniaProperty.Register<PopupConfirm, string>(nameof(OkText));

    /// <summary>
    /// Identifies the <see cref="CancelText" /> property.
    /// </summary>
    public static readonly StyledProperty<string> CancelTextProperty =
        AvaloniaProperty.Register<PopupConfirm, string>(nameof(CancelText));

    /// <summary>
    /// Identifies the <see cref="OkButtonType" /> property.
    /// </summary>
    public static readonly StyledProperty<ButtonType> OkButtonTypeProperty =
        AvaloniaProperty.Register<PopupConfirm, ButtonType>(nameof(OkButtonType), ButtonType.Primary);

    /// <summary>
    /// Identifies the <see cref="IsShowCancelButton" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowCancelButtonProperty =
        AvaloniaProperty.Register<PopupConfirm, bool>(nameof(IsShowCancelButton), true);

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<PopupConfirm, string>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="ConfirmContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ConfirmContentProperty =
        AvaloniaProperty.Register<PopupConfirm, object?>(nameof(ConfirmContent));

    /// <summary>
    /// Identifies the <see cref="ConfirmContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ConfirmContentTemplateProperty =
        AvaloniaProperty.Register<PopupConfirm, IDataTemplate?>(nameof(ConfirmContentTemplate));

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<PopupConfirm, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="ConfirmStatus" /> property.
    /// </summary>
    public static readonly StyledProperty<PopupConfirmStatus> ConfirmStatusProperty =
        AvaloniaProperty.Register<PopupConfirm, PopupConfirmStatus>(nameof(ConfirmStatus),
            PopupConfirmStatus.Warning);

    /// <summary>
    /// Identifies the <see cref="Cancelled" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> CancelledEvent =
        RoutedEvent.Register<PopupConfirm, RoutedEventArgs>(nameof(Cancelled), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="Confirmed" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ConfirmedEvent =
        RoutedEvent.Register<PopupConfirm, RoutedEventArgs>(nameof(Confirmed), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="PopupClick" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<PopupConfirmClickEventArgs> PopupClickEvent =
        RoutedEvent.Register<PopupConfirm, PopupConfirmClickEventArgs>(nameof(PopupClick), RoutingStrategies.Bubble);

    /// <summary>
    /// The text of the OK button of the popup. When no value is set, the theme supplies the localized text for OK.
    /// </summary>
    public string OkText
    {
        get => GetValue(OkTextProperty);
        set => SetValue(OkTextProperty, value);
    }

    /// <summary>
    /// The text of the Cancel button of the popup, which is shown only while <see cref="IsShowCancelButton" /> is
    /// <see langword="true" />. When no value is set, the theme supplies the localized text for Cancel.
    /// </summary>
    public string CancelText
    {
        get => GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    /// <summary>
    /// The type of the OK button of the popup. The default is <see cref="ButtonType.Primary" />.
    /// </summary>
    public ButtonType OkButtonType
    {
        get => GetValue(OkButtonTypeProperty);
        set => SetValue(OkButtonTypeProperty, value);
    }

    /// <summary>
    /// Whether the popup shows its Cancel button. <see langword="true" />, the default, shows the button;
    /// <see langword="false" /> hides it and leaves the OK button as the only action button.
    /// </summary>
    public bool IsShowCancelButton
    {
        get => GetValue(IsShowCancelButtonProperty);
        set => SetValue(IsShowCancelButtonProperty, value);
    }

    /// <summary>
    /// The title shown above the description of the popup. With the default theme, a <see langword="null" /> or
    /// empty value leaves the title element in the layout with its margin but without text.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The description shown under the title of the popup. While this is <see langword="null" />, the description
    /// is hidden and the theme removes the margin around the action buttons. A string is shown as text.
    /// </summary>
    [DependsOn(nameof(ConfirmContentTemplate))]
    public object? ConfirmContent
    {
        get => GetValue(ConfirmContentProperty);
        set => SetValue(ConfirmContentProperty, value);
    }

    /// <summary>
    /// The template that presents <see cref="ConfirmContent" /> in the popup. It applies only while that is not
    /// <see langword="null" />; <see langword="null" /> leaves the choice of the template to the content presenter.
    /// </summary>
    public IDataTemplate? ConfirmContentTemplate
    {
        get => GetValue(ConfirmContentTemplateProperty);
        set => SetValue(ConfirmContentTemplateProperty, value);
    }

    /// <summary>
    /// The icon shown at the leading side of the popup, next to the title and the description.
    /// <see langword="null" /> shows a default filled exclamation-circle icon. <see cref="ConfirmStatus" /> selects
    /// the color of the icon.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// The status of the popup. It selects the color of the popup icon, which does not change with the status: the
    /// primary color for <see cref="PopupConfirmStatus.Info" />, the warning color for
    /// <see cref="PopupConfirmStatus.Warning" /> and the error color for <see cref="PopupConfirmStatus.Error" />.
    /// The default is <see cref="PopupConfirmStatus.Warning" />.
    /// </summary>
    public PopupConfirmStatus ConfirmStatus
    {
        get => GetValue(ConfirmStatusProperty);
        set => SetValue(ConfirmStatusProperty, value);
    }

    /// <summary>
    /// Raised on this control when the user clicks the Cancel button of the popup, before <see cref="PopupClick" />.
    /// Closing the popup starts after both events have been raised. Marking the event as handled does not keep the
    /// popup open.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Cancelled
    {
        add => AddHandler(CancelledEvent, value);
        remove => RemoveHandler(CancelledEvent, value);
    }

    /// <summary>
    /// Raised on this control when the user clicks the OK button of the popup, before <see cref="PopupClick" />.
    /// Closing the popup starts after both events have been raised. Marking the event as handled does not keep the
    /// popup open.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Confirmed
    {
        add => AddHandler(ConfirmedEvent, value);
        remove => RemoveHandler(ConfirmedEvent, value);
    }

    /// <summary>
    /// Raised on this control after <see cref="Confirmed" /> or <see cref="Cancelled" /> when the user clicks the OK
    /// or the Cancel button of the popup; <see cref="PopupConfirmClickEventArgs.IsConfirmed" /> tells which one.
    /// Closing the popup starts after this event has been raised.
    /// </summary>
    public event EventHandler<PopupConfirmClickEventArgs>? PopupClick
    {
        add => AddHandler(PopupClickEvent, value);
        remove => RemoveHandler(PopupClickEvent, value);
    }

    #endregion

    public PopupConfirm()
    {
    }

    /// <summary>
    /// Creates the flyout that shows the confirmation popup when this control is attached to the visual tree and
    /// <c>Flyout</c> is <see langword="null" />, then calls the base member. An override that skips the base call
    /// must set <c>Flyout</c> itself.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Flyout ??= new PopupConfirmFlyout(this);
    }
}

/// <summary>
/// Tells which action button of a <see cref="PopupConfirm" /> popup was clicked, for the
/// <see cref="PopupConfirm.PopupClick" /> event.
/// </summary>
public class PopupConfirmClickEventArgs : RoutedEventArgs
{
    /// <summary>
    /// <see langword="true" /> when the OK button was clicked, <see langword="false" /> when the Cancel button was
    /// clicked.
    /// </summary>
    public bool IsConfirmed { get; }

    /// <summary>
    /// Creates an instance that reports which action button of the popup was clicked.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event that these arguments are raised with.
    /// </param>
    /// <param name="isConfirmed">
    /// Whether the OK button was clicked.
    /// </param>
    public PopupConfirmClickEventArgs(RoutedEvent? routedEvent, bool isConfirmed)
        : base(routedEvent)
    {
        IsConfirmed = isConfirmed;
    }
}