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

    // 操作组示例：卡片内右下角放两个按钮，分别关闭本条与全部通知。
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

    // 样式定制示例：默认分支为浅绿卡片，error 分支整卡转红。
    // 反馈层弹在页面视觉树之外，页面内声明的 Style 命中不到卡片，因此样式资源挂到 manager 自身
    //（卡片是 manager 的视觉后代，按 owner 作用域命中），class 随通知内容传入。
    private void HandleShowDefaultStyleNotification(object? sender, RoutedEventArgs e)
    {
        // 默认分支按 Information 类型展示。
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

    // 语义样式以页面 AXAML 的 Styles 资源声明（仍是生成的专用 Style 类），这里挂到 manager 自身。
    // 样式声明在本页 UserControl.Resources 中，必须从本页的资源宿主查找；从 Application.Current 查找
    // 会命中不到页面级资源，样式将静默失效。挂载是唯一的代码步骤，不做任何部件属性改写。
    private void ApplySemanticStyleStyles(WindowNotificationManager manager)
    {
        // 以 manager 实例去重：同一 manager 只挂一次，页面 detach 后重建的新 manager 会重新挂载。
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
