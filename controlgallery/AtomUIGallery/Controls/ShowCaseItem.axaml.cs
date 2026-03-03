using System;
using AtomUI;
using AtomWindow = AtomUI.Desktop.Controls.Window;
using AtomButton = AtomUI.Desktop.Controls.Button;
using AtomToolTip = AtomUI.Desktop.Controls.ToolTip;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUI.Data;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using AtomUIGallery.Controls.Localization.ShowCaseItemLang;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUIGallery.Controls;

public class ShowCaseItem : ContentControl
{
    public const string LanguageId = nameof(ShowCaseItem);

    internal const string ToggleCodeButtonPart = "PART_ToggleCodeButton";
    internal const string CopyCodeButtonPart = "PART_CopyCodeButton";
    internal const string OpenInNewWindowButtonPart = "PART_OpenInNewWindowButton";
    internal const string OpenRepositoryButtonPart = "PART_OpenRepositoryButton";

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ShowCaseItem, string>(nameof(Title));

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<ShowCaseItem, string>(nameof(Description));

    public static readonly StyledProperty<bool> IsOccupyEntireRowProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsOccupyEntireRow));

    internal static readonly StyledProperty<bool> IsFakeProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsFake), false);

    public static readonly StyledProperty<string?> AxamlSourceProperty =
        AvaloniaProperty.Register<ShowCaseItem, string?>(nameof(AxamlSource));

    public static readonly StyledProperty<string?> DeferredAxamlSourceProperty =
        AvaloniaProperty.Register<ShowCaseItem, string?>(nameof(DeferredAxamlSource));

    public static readonly StyledProperty<bool> IsCodeExpandedProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsCodeExpanded), false);

    public static readonly StyledProperty<string?> RepositorySourceUrlProperty =
        AvaloniaProperty.Register<ShowCaseItem, string?>(nameof(RepositorySourceUrl));

    public static readonly DirectProperty<ShowCaseItem, bool> HasAxamlSourceProperty =
        AvaloniaProperty.RegisterDirect<ShowCaseItem, bool>(
            nameof(HasAxamlSource),
            o => o.HasAxamlSource);

    public static readonly DirectProperty<ShowCaseItem, bool> HasRepositorySourceUrlProperty =
        AvaloniaProperty.RegisterDirect<ShowCaseItem, bool>(
            nameof(HasRepositorySourceUrl),
            o => o.HasRepositorySourceUrl);

    private static TopLevel? _messageHostTopLevel;
    private static WindowMessageManager? _messageManager;

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public bool IsOccupyEntireRow
    {
        get => GetValue(IsOccupyEntireRowProperty);
        set => SetValue(IsOccupyEntireRowProperty, value);
    }

    public bool IsFake
    {
        get => GetValue(IsFakeProperty);
        set => SetValue(IsFakeProperty, value);
    }

    public string? AxamlSource
    {
        get => GetValue(AxamlSourceProperty);
        set => SetValue(AxamlSourceProperty, value);
    }

    public string? DeferredAxamlSource
    {
        get => GetValue(DeferredAxamlSourceProperty);
        set => SetValue(DeferredAxamlSourceProperty, value);
    }

    public bool IsCodeExpanded
    {
        get => GetValue(IsCodeExpandedProperty);
        set => SetValue(IsCodeExpandedProperty, value);
    }

    public string? RepositorySourceUrl
    {
        get => GetValue(RepositorySourceUrlProperty);
        set => SetValue(RepositorySourceUrlProperty, value);
    }

    private bool _hasAxamlSource;

    public bool HasAxamlSource
    {
        get => _hasAxamlSource;
        private set => SetAndRaise(HasAxamlSourceProperty, ref _hasAxamlSource, value);
    }

    private bool _hasRepositorySourceUrl;

    public bool HasRepositorySourceUrl
    {
        get => _hasRepositorySourceUrl;
        private set => SetAndRaise(HasRepositorySourceUrlProperty, ref _hasRepositorySourceUrl, value);
    }

    private AtomButton? _toggleCodeButton;
    private AtomButton? _copyCodeButton;
    private AtomButton? _openInNewWindowButton;
    private AtomButton? _openRepositoryButton;
    private AtomWindow? _sourceCodeWindow;
    private AxamlCodeBlock? _sourceCodeBlock;

    static ShowCaseItem()
    {
        AxamlSourceProperty.Changed.AddClassHandler<ShowCaseItem>((item, _) =>
        {
            item.HandleAxamlSourceAvailabilityChanged();
        });

        DeferredAxamlSourceProperty.Changed.AddClassHandler<ShowCaseItem>((item, _) =>
        {
            item.HandleAxamlSourceAvailabilityChanged();
        });

        IsCodeExpandedProperty.Changed.AddClassHandler<ShowCaseItem>((item, args) =>
        {
            item.HandleCodeExpandedChanged(args.GetNewValue<bool>());
        });

        RepositorySourceUrlProperty.Changed.AddClassHandler<ShowCaseItem>((item, args) =>
        {
            item.HandleRepositorySourceUrlChanged(args.GetNewValue<string?>());
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_toggleCodeButton is not null)
        {
            _toggleCodeButton.Click -= HandleToggleCodeButtonClick;
        }

        if (_copyCodeButton is not null)
        {
            _copyCodeButton.Click -= HandleCopyCodeButtonClick;
        }

        if (_openInNewWindowButton is not null)
        {
            _openInNewWindowButton.Click -= HandleOpenInNewWindowButtonClick;
        }

        if (_openRepositoryButton is not null)
        {
            _openRepositoryButton.Click -= HandleOpenRepositoryButtonClick;
        }

        _toggleCodeButton = e.NameScope.Find<AtomButton>(ToggleCodeButtonPart);
        _copyCodeButton = e.NameScope.Find<AtomButton>(CopyCodeButtonPart);
        _openInNewWindowButton = e.NameScope.Find<AtomButton>(OpenInNewWindowButtonPart);
        _openRepositoryButton = e.NameScope.Find<AtomButton>(OpenRepositoryButtonPart);

        if (_toggleCodeButton is not null)
        {
            _toggleCodeButton.Click += HandleToggleCodeButtonClick;
        }

        if (_copyCodeButton is not null)
        {
            _copyCodeButton.Click += HandleCopyCodeButtonClick;
        }

        if (_openInNewWindowButton is not null)
        {
            _openInNewWindowButton.Click += HandleOpenInNewWindowButtonClick;
        }

        if (_openRepositoryButton is not null)
        {
            _openRepositoryButton.Click += HandleOpenRepositoryButtonClick;
        }

        base.OnApplyTemplate(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CloseCodeWindow();
        base.OnDetachedFromVisualTree(e);
    }

    private void HandleToggleCodeButtonClick(object? sender, RoutedEventArgs e)
    {
        if (!HasAxamlSource)
        {
            return;
        }

        SetCurrentValue(IsCodeExpandedProperty, !IsCodeExpanded);
        e.Handled = true;
    }

    private async void HandleCopyCodeButtonClick(object? sender, RoutedEventArgs e)
    {
        var code = GetEffectiveAxamlSource();
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        await CopyCodeToClipboardAsync(code, TopLevel.GetTopLevel(this));
        e.Handled = true;
    }

    private async void HandleOpenRepositoryButtonClick(object? sender, RoutedEventArgs e)
    {
        if (!HasRepositorySourceUrl || string.IsNullOrWhiteSpace(RepositorySourceUrl))
        {
            return;
        }

        if (!Uri.TryCreate(RepositorySourceUrl, UriKind.Absolute, out var uri))
        {
            return;
        }

        var launcher = TopLevel.GetTopLevel(this)?.Launcher;
        if (launcher is not null)
        {
            await launcher.LaunchUriAsync(uri);
        }

        e.Handled = true;
    }

    private void HandleOpenInNewWindowButtonClick(object? sender, RoutedEventArgs e)
    {
        var code = GetEffectiveAxamlSource();
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        OpenOrActivateCodeWindow(code);
        e.Handled = true;
    }

    private void HandleCodeExpandedChanged(bool isExpanded)
    {
        if (isExpanded)
        {
            EnsureAxamlSourceLoaded();
        }
    }

    private void EnsureAxamlSourceLoaded()
    {
        if (!string.IsNullOrWhiteSpace(AxamlSource) || string.IsNullOrWhiteSpace(DeferredAxamlSource))
        {
            return;
        }

        SetCurrentValue(AxamlSourceProperty, DeferredAxamlSource);
        SetCurrentValue(DeferredAxamlSourceProperty, null);
    }

    private void HandleAxamlSourceAvailabilityChanged()
    {
        HasAxamlSource = !string.IsNullOrWhiteSpace(AxamlSource) || !string.IsNullOrWhiteSpace(DeferredAxamlSource);
        if (!HasAxamlSource && IsCodeExpanded)
        {
            SetCurrentValue(IsCodeExpandedProperty, false);
        }
    }

    private void HandleRepositorySourceUrlChanged(string? sourceUrl)
    {
        HasRepositorySourceUrl = !string.IsNullOrWhiteSpace(sourceUrl);
    }

    private string? GetEffectiveAxamlSource()
    {
        return !string.IsNullOrWhiteSpace(AxamlSource) ? AxamlSource : DeferredAxamlSource;
    }

    private static WindowMessageManager? GetOrCreateMessageManager(TopLevel? topLevel)
    {
        if (topLevel is null)
        {
            return null;
        }

        if (_messageManager is null || !ReferenceEquals(_messageHostTopLevel, topLevel))
        {
            _messageHostTopLevel = topLevel;
            _messageManager = new WindowMessageManager(topLevel)
            {
                MaxItems = 3,
                Position = NotificationPosition.TopCenter
            };
        }

        return _messageManager;
    }

    private void ShowCopySuccessMessage(TopLevel? topLevel)
    {
        var messageManager = GetOrCreateMessageManager(topLevel);
        var messageText = LanguageResourceBinder.GetLangResource(ShowCaseItemLangResourceKey.CopySuccessMessage)
                          ?? "Code copied";
        messageManager?.Show(new Message(
            type: MessageType.Success,
            expiration: TimeSpan.FromSeconds(1.6),
            content: messageText
        ));
    }

    private Control BuildCodeWindowContent(string code, out AxamlCodeBlock codeBlock)
    {
        var copyButton = new AtomButton
        {
            Shape = ButtonShape.Circle,
            ButtonType = ButtonType.Text,
            SizeType = SizeType.Small,
            Icon = new CopyOutlined(),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        AtomToolTip.SetTip(
            copyButton,
            LanguageResourceBinder.GetLangResource(ShowCaseItemLangResourceKey.ToolTipCopyCode) ?? "Copy Code");
        copyButton.Click += async (_, args) =>
        {
            await CopyCodeToClipboardAsync(code, TopLevel.GetTopLevel(copyButton));
            args.Handled = true;
        };

        var topBarPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        topBarPanel.Children.Add(copyButton);

        var topBar = new Border
        {
            Padding = new Thickness(12, 10, 12, 0),
            Child = topBarPanel
        };

        codeBlock = new AxamlCodeBlock
        {
            Code = code,
            FontFamily = new FontFamily("Consolas,Menlo,Monaco,monospace"),
            FontSize = 14,
            Margin = new Thickness(16)
        };

        var container = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*")
        };
        container.Children.Add(topBar);
        container.Children.Add(codeBlock);
        Grid.SetRow(codeBlock, 1);
        return container;
    }

    private void OpenOrActivateCodeWindow(string code)
    {
        if (_sourceCodeWindow is { IsVisible: true } openedWindow &&
            _sourceCodeBlock is not null)
        {
            _sourceCodeBlock.Code = code;
            openedWindow.Activate();
            return;
        }

        var content = BuildCodeWindowContent(code, out var codeBlock);
        var window = new AtomWindow
        {
            Title = $"{Title} - Source",
            CanResize = true,
            IsPinCaptionButtonEnabled = true,
            IsFullScreenCaptionButtonEnabled = true,
            IsCloseCaptionButtonEnabled = true,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = content
        };

        window.Closed += HandleCodeWindowClosed;

        _sourceCodeWindow = window;
        _sourceCodeBlock = codeBlock;

        if (TopLevel.GetTopLevel(this) is AvaloniaWindow owner)
        {
            window.Show(owner);
        }
        else
        {
            window.Show();
        }
    }

    private void HandleCodeWindowClosed(object? sender, EventArgs e)
    {
        if (sender is AtomWindow window)
        {
            window.Closed -= HandleCodeWindowClosed;
        }

        _sourceCodeWindow = null;
        _sourceCodeBlock = null;
    }

    private void CloseCodeWindow()
    {
        _sourceCodeWindow?.Close();
        _sourceCodeWindow = null;
        _sourceCodeBlock = null;
    }

    private async Task CopyCodeToClipboardAsync(string code, TopLevel? topLevel)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        var clipboard = topLevel?.Clipboard;
        if (clipboard is null)
        {
            return;
        }

        await clipboard.SetTextAsync(code);
        ShowCopySuccessMessage(topLevel);
    }
}
