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

    // 对齐上游 components/message/demo/style-class.tsx：Object style 以对象式样式弹出成功消息（绿色卡片），
    // Function style 以函数式样式弹出错误消息——上游 stylesFn 在 type === 'error' 时整卡转红，AtomUI 由
    // 卡片上的 :error 伪类分支表达。样式随消息传入（上游 styles 的等价物），不做代码侧部件属性改写。
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
        // 与上游一致：demo 走 messageApi.open 的默认 duration = 3 秒，到期自动消失。
        // 注意保持有限时长：常驻消息在连点时会无限堆积。
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

        // 窗口反馈层（WindowFeedbackLayer）在页面视觉树之外，页面内声明的 Style 选不中它弹出的卡片。
        // 语义样式在页面 AXAML 中以 Styles 资源声明（仍是生成的专用 Style 类），这里挂到 manager 自身
        // ——卡片是 manager 的视觉后代，按 owner 作用域命中。挂载是唯一的代码步骤，不做部件属性改写。
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

    // manager 只通过 Show(IMessage) 接收消息，没有声明式 items 入口；语义预览舞台在首次加载时
    // 注入一条常驻消息（expiration = Zero），让 listContent 与卡片在预览期间一直可解析、可高亮。
    // 这是示例数据播种，不是用代码改写语义部件属性（部件定制一律走生成的专用 Style）。
    // Loaded 在每次重新挂载时都会触发，用实例引用去重，避免同一 manager 播种多条消息。
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
