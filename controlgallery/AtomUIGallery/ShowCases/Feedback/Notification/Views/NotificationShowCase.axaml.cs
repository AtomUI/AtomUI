using AtomUI;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUIGallery.ShowCases.Notification;

public partial class NotificationShowCase : GalleryReactiveUserControl<NotificationViewModel>
{
    public const string LanguageId = nameof(NotificationShowCase);

    private WindowNotificationManager? _basicManager;
    private WindowNotificationManager? _topLeftManager;
    private WindowNotificationManager? _topManager;
    private WindowNotificationManager? _topRightManager;
    private WindowNotificationManager? _bottomLeftManager;
    private WindowNotificationManager? _bottomManager;
    private WindowNotificationManager? _bottomRightManager;
    private WindowNotificationManager? _stackManager;
    private bool _isPauseOnHover = true;
    private bool _isStackEnabled = true;
    private int _stackThreshold = 3;
    private int _stackNotificationIndex;
    private WindowNotificationManager? _semanticStylesOwner;

    public NotificationShowCase()
    {
        InitializeComponent();
        AddHandler(AbstractOptionButtonGroup.OptionCheckedChangedEvent, HandleHoverOptionGroupCheckedChanged);
    }

    private void HandleHoverOptionGroupCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        _isPauseOnHover = args.Index == 0;
        if (_basicManager is not null)
        {
            _basicManager.IsPauseOnHover = _isPauseOnHover;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DisposeManager(ref _basicManager);
        DisposeManager(ref _topLeftManager);
        DisposeManager(ref _topManager);
        DisposeManager(ref _topRightManager);
        DisposeManager(ref _bottomLeftManager);
        DisposeManager(ref _bottomManager);
        DisposeManager(ref _bottomRightManager);
        DisposeManager(ref _stackManager);
    }

    private WindowNotificationManager? GetBasicManager()
    {
        var manager = GetManager(ref _basicManager, NotificationPosition.TopRight);
        if (manager is not null)
        {
            manager.IsPauseOnHover = _isPauseOnHover;
        }
        return manager;
    }

    private WindowNotificationManager? GetManager(ref WindowNotificationManager? manager, NotificationPosition position)
    {
        if (manager is not null)
        {
            return manager;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return null;
        }

        manager = new WindowNotificationManager(topLevel)
        {
            MaxItems = 0,
            Position = position
        };
        return manager;
    }

    private static void DisposeManager(ref WindowNotificationManager? manager)
    {
        manager?.Dispose();
        manager = null;
    }

    private void ShowSimpleNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowNeverCloseNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            expiration: TimeSpan.Zero,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationNeverCloseContent,
                "I will never close automatically. This is a purposely very very long description that has many many characters and words.")
        ));
    }

    private void ShowSuccessNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            type: NotificationType.Success,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification.")
        ));
    }

    private void ShowInfoNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            type: NotificationType.Information,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification.")
        ));
    }

    private void ShowWarningNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            type: NotificationType.Warning,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification.")
        ));
    }

    private void ShowErrorNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            type: NotificationType.Error,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification.")
        ));
    }

    private void ShowTopNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _topManager, NotificationPosition.TopCenter)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationTopTitle, "Notification Top"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowBottomNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _bottomManager, NotificationPosition.BottomCenter)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationBottomTitle, "Notification Bottom"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowTopLeftNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _topLeftManager, NotificationPosition.TopLeft)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationTopLeftTitle, "Notification TopLeft"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowTopRightNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _topRightManager, NotificationPosition.TopRight)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationTopRightTitle, "Notification TopRight"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowBottomLeftNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _bottomLeftManager, NotificationPosition.BottomLeft)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationBottomLeftTitle, "Notification BottomLeft"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowBottomRightNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _bottomRightManager, NotificationPosition.BottomRight)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationBottomRightTitle, "Notification BottomRight"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowCustomIconNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification."),
            icon: new SettingOutlined()
        ));
    }

    private void ShowProgressNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            type: NotificationType.Information,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification."),
            showProgress: true
        ));
    }

    private void HandleStackEnabledChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not AtomUIToggleSwitch toggleSwitch)
        {
            return;
        }

        _isStackEnabled = toggleSwitch.IsChecked == true;
        if (_stackManager is not null)
        {
            _stackManager.IsStackEnabled = _isStackEnabled;
        }
    }

    private void HandleStackThresholdChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (sender is not AtomUINumericUpDown { Value: { } value })
        {
            return;
        }

        _stackThreshold = (int)value;
        if (_stackManager is not null)
        {
            _stackManager.StackThreshold = _stackThreshold;
        }
    }

    private void ShowStackNotification(object? sender, RoutedEventArgs e)
    {
        var index = ++_stackNotificationIndex;
        var title = Format(
            NotificationShowCaseLangResourceKind.P2NotificationStackedTitleFormat,
            "Notification {0}",
            index);
        var content = index % 2 == 0
            ? Format(
                NotificationShowCaseLangResourceKind.P2NotificationLongStackedFormat,
                "Notification {0}: This is a deliberately longer stacked notification used to verify variable-height cards.",
                index)
            : Format(
                NotificationShowCaseLangResourceKind.P2NotificationStackedFormat,
                "Notification {0}: This is a stacked notification.",
                index);

        GetStackManager()?.Show(new AtomUINotification(
            title: title,
            content: content,
            expiration: TimeSpan.Zero));
    }

    private void DestroyStackNotifications(object? sender, RoutedEventArgs e)
    {
        _stackManager?.DestroyAll();
    }

    private WindowNotificationManager? GetStackManager()
    {
        if (_stackManager is not null)
        {
            return _stackManager;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return null;
        }

        _stackManager = new WindowNotificationManager(topLevel)
        {
            MaxItems = 0,
            Position = NotificationPosition.TopRight,
            IsStackEnabled = _isStackEnabled,
            StackThreshold = _stackThreshold
        };
        return _stackManager;
    }

    // Actions example: two buttons at the bottom right of the card close this notification and all
    // notifications.
    private void ShowActionsNotification(object? sender, RoutedEventArgs e)
    {
        var manager = GetBasicManager();
        if (manager is null)
        {
            return;
        }

        var actions = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing     = 8
        };
        actions.Children.Add(CreateActionButton(
            Lang(NotificationShowCaseLangResourceKind.P2ContentDestroyAll, "Destroy All"),
            button => CloseHostNotifications(button)));
        actions.Children.Add(CreateActionButton(
            Lang(NotificationShowCaseLangResourceKind.P2ContentConfirm, "Confirm"),
            button => button.GetVisualAncestors().OfType<NotificationCard>().FirstOrDefault()?.Close()));

        manager.Show(new AtomUINotification(
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationActionsContent,
                "A function will be called after the notification is closed (automatically after the \"duration\" time or manually)."),
            expiration: TimeSpan.Zero,
            actions: actions));
    }

    private static AtomUIButton CreateActionButton(string content, Action<AtomUIButton> onClick)
    {
        var button = new AtomUIButton
        {
            Content  = content,
            ButtonType = ButtonType.Link,
            SizeType   = CustomizableSizeType.Small
        };
        button.Click += (sender, _) =>
        {
            if (sender is AtomUIButton source)
            {
                onClick(source);
            }
        };
        return button;
    }

    private static void CloseHostNotifications(AtomUIButton button)
    {
        var host = button.GetVisualAncestors().OfType<WindowNotificationManager>().FirstOrDefault();
        if (host is null)
        {
            return;
        }

        foreach (var card in host.GetVisualDescendants().OfType<NotificationCard>().ToArray())
        {
            card.Close();
        }
    }

    // Style customization example: the default branch is a light green card and the error branch turns the
    // whole card red. The feedback layer is outside the page visual tree, so styles declared in the page
    // cannot reach the cards. The style resource is therefore attached to the manager itself (the cards are
    // visual descendants of the manager and are matched in owner scope) and the class travels with the
    // notification content.
    private void HandleShowDefaultStyleNotification(object? sender, RoutedEventArgs e)
    {
        // The default branch is shown with the Information type.
        ShowStyledNotification(NotificationType.Information, "semantic-default-style-demo");
    }

    private void HandleShowErrorStyleNotification(object? sender, RoutedEventArgs e)
    {
        ShowStyledNotification(NotificationType.Error, "semantic-error-style-demo");
    }

    private void ShowStyledNotification(NotificationType type, string styleClass)
    {
        var manager = GetBasicManager();
        if (manager is null)
        {
            return;
        }

        ApplySemanticStyleStyles(manager);
        manager.Show(
            new AtomUINotification(
                type: type,
                title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
                content: Lang(NotificationShowCaseLangResourceKind.P2NotificationDescription,
                    "This is a notification description."),
                expiration: TimeSpan.FromSeconds(3)),
            [styleClass]);
    }

    // The semantic styles are declared in the page AXAML as a Styles resource (still the generated dedicated
    // style classes) and attached to the manager itself here. They live in this page's UserControl.Resources,
    // so they must be looked up from this page's resource host: a lookup from Application.Current misses
    // page-level resources and the styles silently do nothing. Attaching is the only code step; no part
    // property is rewritten.
    private void ApplySemanticStyleStyles(WindowNotificationManager manager)
    {
        // Deduplicates by manager instance: a manager is attached only once, and a new manager created after
        // the page detaches is attached again.
        if (ReferenceEquals(_semanticStylesOwner, manager))
        {
            return;
        }

        if (Resources.TryGetResource("NotificationSemanticStyleStyles", null, out var styles) &&
            styles is Styles semanticStyles)
        {
            manager.Styles.Add(semanticStyles);
            _semanticStylesOwner = manager;
        }
    }

    private static string Lang(NotificationShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }

    private static string Format(
        NotificationShowCaseLangResourceKind resourceKind,
        string fallback,
        params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }
}
