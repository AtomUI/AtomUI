using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The data of one notification to show through an <see cref="INotificationManager" />: what its card shows,
/// how long it stays open and which callbacks to run. The manager copies the values into the card when the
/// notification is shown and does not keep the object, so later changes do not reach a card that is shown.
/// </summary>
public class Notification : INotification, INotifyPropertyChanged
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromSeconds(4.5);

    private string _title;
    private object? _content;
    private bool _showProgress;
    private PathIcon? _icon;
    private object? _actions;
    private IDataTemplate? _actionsTemplate;

    /// <summary>
    /// Creates a notification. A <see langword="null" /> <paramref name="expiration" /> means 4.5 seconds, and
    /// <see cref="TimeSpan.Zero" /> keeps the notification open until it is closed.
    /// </summary>
    public Notification(string title,
                        object? content,
                        NotificationType type = NotificationType.Default,
                        PathIcon? icon = null,
                        TimeSpan? expiration = null,
                        bool showProgress = false,
                        Action? onClick = null,
                        Action? onClose = null,
                        object? actions = null,
                        IDataTemplate? actionsTemplate = null)
    {
        _title           = title;
        _content         = content;
        _icon            = icon;
        _actions         = actions;
        _actionsTemplate = actionsTemplate;
        Type             = type;
        Expiration       = expiration.HasValue ? expiration.Value : DefaultExpiration;
        ShowProgress     = showProgress;
        OnClick          = onClick;
        OnClose          = onClose;
    }

    /// <summary>
    /// The title of the notification, shown as the headline of its card.
    /// </summary>
    public string Title
    {
        get => _title;

        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// The message of the notification, shown below the title of its card. A string is shown as wrapped, selectable
    /// text; <see langword="null" /> shows no message.
    /// </summary>
    public object? Content
    {
        get => _content;

        set
        {
            if (!ReferenceEquals(_content, value))
            {
                _content = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// The icon shown in front of the text. When <see langword="null" />, the card shows the status icon of
    /// <see cref="Type" />, if that type has one.
    /// </summary>
    public PathIcon? Icon
    {
        get => _icon;

        set
        {
            if (!ReferenceEquals(_icon, value))
            {
                _icon = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Shows a remaining-time bar along the bottom edge of the card. It needs an <see cref="Expiration" /> other
    /// than <see cref="TimeSpan.Zero" />; a notification that stays open shows no bar.
    /// </summary>
    public bool ShowProgress
    {
        get => _showProgress;

        set
        {
            if (_showProgress != value)
            {
                _showProgress = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Content of the action area below the description. <see langword="null" />, the default, hides the area.
    /// </summary>
    public object? Actions
    {
        get => _actions;

        set
        {
            if (!ReferenceEquals(_actions, value))
            {
                _actions = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Data template used to present <see cref="Actions" />. <see langword="null" /> by default.
    /// </summary>
    public IDataTemplate? ActionsTemplate
    {
        get => _actionsTemplate;

        set
        {
            if (!ReferenceEquals(_actionsTemplate, value))
            {
                _actionsTemplate = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// The type of the notification. The card takes the status look of this type and, when <see cref="Icon" /> is
    /// <see langword="null" />, shows the icon of this type, if it has one.
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// The time after which the card closes by itself, counted from the moment the notification is shown.
    /// <see cref="WindowNotificationManager.IsPauseOnHover" /> pauses the countdown while the pointer is over the
    /// card. <see cref="TimeSpan.Zero" /> keeps the card open until it is closed another way.
    /// </summary>
    public TimeSpan Expiration { get; set; }

    /// <summary>
    /// Run each time the user presses the card with the pointer. Pressing the card does not close the notification.
    /// </summary>
    public Action? OnClick { get; set; }

    /// <summary>
    /// Run once the card has closed, whichever way it was closed: by its expiry, by its close button or by the
    /// manager. It does not run when the manager is disposed, and an exception thrown by the callback is not
    /// propagated.
    /// </summary>
    public Action? OnClose { get; set; }

    /// <summary>
    /// Raised after <see cref="Title" />, <see cref="Content" />, <see cref="Icon" />,
    /// <see cref="ShowProgress" />, <see cref="Actions" /> or <see cref="ActionsTemplate" /> changed to a
    /// different value, with the new value already stored. The other properties do not raise it.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises <see cref="PropertyChanged" /> for the named property. The setters of <see cref="Title" />,
    /// <see cref="Content" />, <see cref="Icon" />, <see cref="ShowProgress" />, <see cref="Actions" /> and
    /// <see cref="ActionsTemplate" /> call it. An override must call the base method for the event to be raised.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the changed property. It defaults to the name of the calling member.
    /// </param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
