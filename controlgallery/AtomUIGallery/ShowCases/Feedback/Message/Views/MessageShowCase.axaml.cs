using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using Avalonia.Styling;

namespace AtomUIGallery.ShowCases.Message;

public partial class MessageShowCase : GalleryReactiveUserControl<MessageViewModel>
{
    public const string LanguageId = nameof(MessageShowCase);

    private WindowMessageManager? _defaultMessageManager;
    private WindowMessageManager? _stackMessageManager;
    private bool _isStackEnabled = true;
    private int _stackThreshold = 3;
    private int _stackMessageIndex;

    public MessageShowCase()
    {
        InitializeComponent();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _defaultMessageManager?.Dispose();
        _defaultMessageManager = null;
        _stackMessageManager?.Dispose();
        _stackMessageManager = null;
    }

    private void ShowSimpleMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            Lang(MessageShowCaseLangResourceKind.P2MessageHelloAtomUIAvalonia, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowInfoMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Information,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageInformation, "This is an information message.")
        ));
    }

    private void ShowSuccessMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Success,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageSuccess, "This is a success message.")
        ));
    }

    private void ShowWarningMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Warning,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageWarning, "This is a warning message.")
        ));
    }

    private void ShowErrorMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Error,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageError, "This is an error message.")
        ));
    }

    private void ShowLoadingMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Loading,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageActionInProgress, "Action in progress...")
        ));
    }

    private void ShowSequentialMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Loading,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageActionInProgress, "Action in progress..."),
            expiration: TimeSpan.FromSeconds(2.5),
            onClose: () =>
            {
                ShowMessage(new AtomUIMessage(
                    type: MessageType.Success,
                    expiration: TimeSpan.FromSeconds(2.5),
                    content: Lang(MessageShowCaseLangResourceKind.P2MessageLoadingFinished, "Loading finished"),
                    onClose: () =>
                    {
                        ShowMessage(new AtomUIMessage(
                            type: MessageType.Information,
                            expiration: TimeSpan.FromSeconds(2.5),
                            content: Lang(MessageShowCaseLangResourceKind.P2MessageLoadingFinished, "Loading finished")
                        ));
                    }
                ));
            }
        ));
    }

    private void HandleStackEnabledChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not AtomUIToggleSwitch toggleSwitch)
        {
            return;
        }

        _isStackEnabled = toggleSwitch.IsChecked == true;
        if (_stackMessageManager is not null)
        {
            _stackMessageManager.IsStackEnabled = _isStackEnabled;
        }
    }

    private void HandleStackThresholdChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (sender is not AtomUINumericUpDown { Value: { } value })
        {
            return;
        }

        _stackThreshold = (int)value;
        if (_stackMessageManager is not null)
        {
            _stackMessageManager.StackThreshold = _stackThreshold;
        }
    }

    private void ShowStackMessage(object? sender, RoutedEventArgs e)
    {
        var index = ++_stackMessageIndex;
        var isLongMessage = index % 2 == 0;
        var content = isLongMessage
            ? Format(
                MessageShowCaseLangResourceKind.P2MessageLongStackedFormat,
                "Message {0}: This is a slightly longer stacked message.",
                index)
            : Format(
                MessageShowCaseLangResourceKind.P2MessageStackedFormat,
                "Message {0}: This is a stacked message.",
                index);

        GetStackMessageManager()?.Show(new AtomUIMessage(
            content,
            type: MessageType.Information,
            expiration: TimeSpan.Zero));
    }

    private void DestroyStackMessages(object? sender, RoutedEventArgs e)
    {
        _stackMessageManager?.DestroyAll();
    }

    private void ShowMessage(AtomUIMessage message)
    {
        GetDefaultMessageManager()?.Show(message);
    }

    // As in the upstream components/message/demo/style-class.tsx: Object style shows a success message with
    // object styles (green card), Function style shows an error message with function styles. The upstream
    // stylesFn turns the whole card red when the type is error; AtomUI expresses that with the :error
    // pseudo-class branch on the card. Styles travel with the message (the counterpart of the upstream
    // styles argument). Part properties are never rewritten in code.
    private void HandleShowObjectStyleMessage(object? sender, RoutedEventArgs e)
    {
        ShowStyledMessage(
            MessageType.Success,
            MessageShowCaseLangResourceKind.P2MessageObjectStyles,
            "This is a message with object styles",
            "semantic-object-style-demo");
    }

    private void HandleShowFunctionStyleMessage(object? sender, RoutedEventArgs e)
    {
        ShowStyledMessage(
            MessageType.Error,
            MessageShowCaseLangResourceKind.P2MessageFunctionStyles,
            "This is a message with function styles",
            "semantic-function-style-demo");
    }

    private void ShowStyledMessage(
        MessageType type,
        MessageShowCaseLangResourceKind resourceKind,
        string fallback,
        string styleClass)
    {
        // As upstream: the demo uses the default duration of messageApi.open, 3 seconds, then auto-closes.
        // Keep the duration finite: persistent messages pile up without bound when the button is repeated.
        GetDefaultMessageManager()?.Show(
            new AtomUIMessage(
                type: type,
                content: Lang(resourceKind, fallback),
                expiration: TimeSpan.FromSeconds(3)),
            [styleClass]);
    }

    private static string Lang(MessageShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }

    private static string Format(
        MessageShowCaseLangResourceKind resourceKind,
        string fallback,
        params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }

    private WindowMessageManager? GetDefaultMessageManager()
    {
        if (_defaultMessageManager is not null)
        {
            return _defaultMessageManager;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return null;
        }

        _defaultMessageManager = new WindowMessageManager(topLevel)
        {
            MaxItems = 0
        };

        // The window feedback layer (WindowFeedbackLayer) is outside the page visual tree, so styles declared in
        // the page cannot select the cards it pops up. The semantic styles are declared in the page AXAML as a
        // Styles resource (still the generated dedicated style classes) and attached to the manager itself here:
        // the cards are visual descendants of the manager and are matched in owner scope. Attaching is the only
        // code step; part properties are not rewritten.
        if (Resources.TryGetResource("MessageSemanticStyleStyles", null, out var styles) &&
            styles is Styles semanticStyles)
        {
            _defaultMessageManager.Styles.Add(semanticStyles);
        }

        return _defaultMessageManager;
    }

    private WindowMessageManager? GetStackMessageManager()
    {
        if (_stackMessageManager is not null)
        {
            return _stackMessageManager;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return null;
        }

        _stackMessageManager = new WindowMessageManager(topLevel)
        {
            MaxItems       = 0,
            IsStackEnabled = _isStackEnabled,
            StackThreshold = _stackThreshold
        };
        return _stackMessageManager;
    }

    // The manager takes messages only through Show(IMessage) and has no declarative items entry, so the
    // semantic preview stage seeds one persistent message (expiration = Zero) on first load. listContent and
    // the card stay resolvable and highlightable while the preview lives. This seeds example data; it does
    // not rewrite semantic part properties in code (part customization always goes through the generated
    // dedicated styles). Loaded fires on every re-attach, so the instance reference deduplicates and the
    // same manager is never seeded twice.
    private WindowMessageManager? _seededSemanticPreviewManager;

    private void HandleSemanticPreviewOwnerLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not WindowMessageManager manager ||
            ReferenceEquals(_seededSemanticPreviewManager, manager))
        {
            return;
        }

        _seededSemanticPreviewManager = manager;
        manager.Show(new AtomUIMessage(
            type: MessageType.Information,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageInformation, "This is an information message."),
            expiration: TimeSpan.Zero));
    }
}
