using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public enum MessageBoxOkButtonStyle
{
    Default,
    Primary
}

public partial class MessageBox : Dialog
{
    #region Public Properties

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<MessageBox, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<MessageBoxStyle> StyleProperty =
        AvaloniaProperty.Register<MessageBox, MessageBoxStyle>(
            nameof(Style),
            MessageBoxStyle.Information);

    public static readonly StyledProperty<MessageBoxOkButtonStyle> OkButtonStyleProperty =
        AvaloniaProperty.Register<MessageBox, MessageBoxOkButtonStyle>(
            nameof(OkButtonStyle),
            MessageBoxOkButtonStyle.Primary);

    public static readonly StyledProperty<string?> OkButtonTextProperty =
        AvaloniaProperty.Register<MessageBox, string?>(nameof(OkButtonText));

    public static readonly StyledProperty<string?> CancelButtonTextProperty =
        AvaloniaProperty.Register<MessageBox, string?>(nameof(CancelButtonText));

    public static readonly StyledProperty<bool> IsCenterOnStartupProperty =
        AvaloniaProperty.Register<MessageBox, bool>(nameof(IsCenterOnStartup), true);

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public MessageBoxStyle Style
    {
        get => GetValue(StyleProperty);
        set => SetValue(StyleProperty, value);
    }

    public MessageBoxOkButtonStyle OkButtonStyle
    {
        get => GetValue(OkButtonStyleProperty);
        set => SetValue(OkButtonStyleProperty, value);
    }

    public string? OkButtonText
    {
        get => GetValue(OkButtonTextProperty);
        set => SetValue(OkButtonTextProperty, value);
    }

    public string? CancelButtonText
    {
        get => GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    public bool IsCenterOnStartup
    {
        get => GetValue(IsCenterOnStartupProperty);
        set => SetValue(IsCenterOnStartupProperty, value);
    }

    #endregion

    #region Public Events

    public event EventHandler? Cancelled;
    public event EventHandler? Confirmed;

    #endregion

    private readonly MessageBoxContent _messageContent = new();

    static MessageBox()
    {
        IsDragMovableProperty.OverrideDefaultValue<MessageBox>(false);
    }

    public MessageBox()
    {
        Accepted += HandleAccepted;
        Rejected += HandleRejected;
        ConfigureStyle();
        ConfigureOkButton();
        ConfigurePositionOnStartup();
        UpdateMessageContent();
    }

    public void Cancel()
    {
        Reject();
    }

    public void Confirm()
    {
        Accept();
    }

    internal override object? GetSurfaceContent()
    {
        return _messageContent;
    }

    internal override IDataTemplate? GetSurfaceContentTemplate()
    {
        return null;
    }

    internal override void ConfigureSurfaceButtons(IReadOnlyList<DialogButton> buttons)
    {
        ApplyButtonConfiguration(buttons);
        base.ConfigureSurfaceButtons(buttons);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty ||
            change.Property == ContentTemplateProperty ||
            change.Property == IconProperty)
        {
            UpdateMessageContent();
        }
        else if (change.Property == StyleProperty)
        {
            ConfigureStyle();
            UpdateMessageContent();
        }
        else if (change.Property == OkButtonStyleProperty)
        {
            ConfigureOkButton();
            ApplyButtonConfiguration();
        }
        else if (change.Property == OkButtonTextProperty ||
                 change.Property == CancelButtonTextProperty)
        {
            ApplyButtonConfiguration();
        }
        else if (change.Property == IsCenterOnStartupProperty)
        {
            ConfigurePositionOnStartup();
        }
    }

    private void HandleAccepted(object? sender, EventArgs e)
    {
        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    private void HandleRejected(object? sender, EventArgs e)
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    private void ConfigureStyle()
    {
        SetValue(
            EscapeStandardButtonProperty,
            DialogStandardButton.NoButton,
            BindingPriority.Template);
        switch (Style)
        {
            case MessageBoxStyle.Normal:
                SetValue(IconProperty, null, BindingPriority.Template);
                SetValue(StandardButtonsProperty, DialogStandardButton.Ok, BindingPriority.Template);
                break;
            case MessageBoxStyle.Confirm:
                SetValue(IconProperty, new ExclamationCircleFilled(), BindingPriority.Template);
                SetValue(
                    StandardButtonsProperty,
                    DialogStandardButton.Ok | DialogStandardButton.Cancel,
                    BindingPriority.Template);
                SetValue(
                    EscapeStandardButtonProperty,
                    DialogStandardButton.Cancel,
                    BindingPriority.Template);
                break;
            case MessageBoxStyle.Information:
                SetValue(IconProperty, new InfoCircleFilled(), BindingPriority.Template);
                SetValue(StandardButtonsProperty, DialogStandardButton.Ok, BindingPriority.Template);
                break;
            case MessageBoxStyle.Success:
                SetValue(IconProperty, new CheckCircleFilled(), BindingPriority.Template);
                SetValue(StandardButtonsProperty, DialogStandardButton.Ok, BindingPriority.Template);
                break;
            case MessageBoxStyle.Warning:
                SetValue(IconProperty, new ExclamationCircleFilled(), BindingPriority.Template);
                SetValue(StandardButtonsProperty, DialogStandardButton.Ok, BindingPriority.Template);
                break;
            case MessageBoxStyle.Error:
                SetValue(IconProperty, new CloseCircleFilled(), BindingPriority.Template);
                SetValue(StandardButtonsProperty, DialogStandardButton.Ok, BindingPriority.Template);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ConfigureOkButton()
    {
        SetValue(
            DefaultStandardButtonProperty,
            OkButtonStyle == MessageBoxOkButtonStyle.Primary
                ? DialogStandardButton.Ok
                : DialogStandardButton.NoButton,
            BindingPriority.Template);
    }

    private void ConfigurePositionOnStartup()
    {
        SetValue(
            HorizontalStartupLocationProperty,
            IsCenterOnStartup ? DialogHorizontalAnchor.Center : DialogHorizontalAnchor.Custom,
            BindingPriority.Template);
        SetValue(
            VerticalStartupLocationProperty,
            IsCenterOnStartup ? DialogVerticalAnchor.Center : DialogVerticalAnchor.Custom,
            BindingPriority.Template);
    }

    private void UpdateMessageContent()
    {
        _messageContent.StyleIcon      = Icon;
        _messageContent.Style          = Style;
        _messageContent.Content        = Content;
        _messageContent.ContentTemplate = ContentTemplate;
    }

    private void ApplyButtonConfiguration()
    {
        var buttons = SurfaceButtons;
        if (buttons.Count == 0)
        {
            return;
        }

        ApplyButtonConfiguration(buttons);
        base.ConfigureSurfaceButtons(buttons);
    }

    private void ApplyButtonConfiguration(IReadOnlyList<DialogButton> buttons)
    {
        foreach (var button in buttons)
        {
            if (button.StandardButtonType == DialogStandardButton.Ok)
            {
                button.ButtonType = OkButtonStyle == MessageBoxOkButtonStyle.Primary
                    ? ButtonType.Primary
                    : ButtonType.Default;
                if (OkButtonText is null)
                {
                    button.ClearValue(Button.ContentProperty);
                }
                else
                {
                    button.SetValue(Button.ContentProperty, OkButtonText);
                }
            }
            else if (button.StandardButtonType == DialogStandardButton.Cancel)
            {
                if (CancelButtonText is null)
                {
                    button.ClearValue(Button.ContentProperty);
                }
                else
                {
                    button.SetValue(Button.ContentProperty, CancelButtonText);
                }
            }
        }
    }

    #region Static API

    public static async Task<object?> ShowMessageBoxAsync<TView, TViewModel>(
        TViewModel? dataContext,
        MessageBoxOptions? options = null,
        TopLevel? topLevel = null,
        CancellationToken cancellationToken = default)
        where TView : Control, new()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return await Dispatcher.UIThread.InvokeAsync(() =>
                ShowMessageBoxAsync<TView, TViewModel>(dataContext, options, topLevel, cancellationToken));
        }

        return await ShowMessageBoxAsync(new TView(), dataContext, options, topLevel, cancellationToken);
    }

    public static async Task<object?> ShowMessageBoxModalAsync<TView, TViewModel>(
        TViewModel? dataContext,
        MessageBoxOptions? options = null,
        TopLevel? topLevel = null,
        CancellationToken cancellationToken = default)
        where TView : Control, new()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return await Dispatcher.UIThread.InvokeAsync(() =>
                ShowMessageBoxModalAsync<TView, TViewModel>(dataContext, options, topLevel, cancellationToken));
        }

        return await ShowMessageBoxModalAsync(new TView(), dataContext, options, topLevel, cancellationToken);
    }

    public static Task<object?> ShowMessageBoxAsync(
        Control content,
        object? dataContext = null,
        MessageBoxOptions? options = null,
        TopLevel? topLevel = null,
        CancellationToken cancellationToken = default)
    {
        return ShowMessageBoxCoreAsync(
            content,
            dataContext,
            options,
            topLevel,
            isModal: false,
            cancellationToken);
    }

    public static Task<object?> ShowMessageBoxModalAsync(
        Control content,
        object? dataContext = null,
        MessageBoxOptions? options = null,
        TopLevel? topLevel = null,
        CancellationToken cancellationToken = default)
    {
        return ShowMessageBoxCoreAsync(
            content,
            dataContext,
            options,
            topLevel,
            isModal: true,
            cancellationToken);
    }

    private static async Task<object?> ShowMessageBoxCoreAsync(
        Control content,
        object? dataContext,
        MessageBoxOptions? options,
        TopLevel? topLevel,
        bool isModal,
        CancellationToken cancellationToken)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return await Dispatcher.UIThread.InvokeAsync(() => ShowMessageBoxCoreAsync(
                content,
                dataContext,
                options,
                topLevel,
                isModal,
                cancellationToken));
        }

        var overlayLayer = ResolveOverlayLayer(options, topLevel);
        var messageBox   = CreateMessageBox(content, dataContext, options, overlayLayer);
        messageBox.IsModal = isModal;
        overlayLayer.Children.Add(messageBox);
        try
        {
            await messageBox.OpenAsync(cancellationToken);
            return messageBox.Result;
        }
        finally
        {
            overlayLayer.Children.Remove(messageBox);
        }
    }

    private static MessageBox CreateMessageBox(
        Control content,
        object? dataContext,
        MessageBoxOptions? options,
        Control placementTarget)
    {
        var messageBox = new MessageBox
        {
            Title             = options?.Title,
            IsDragMovable     = options?.IsDragMovable ?? false,
            IsMaskClosable    = options?.IsMaskClosable ?? true,
            Style             = options?.Style ?? MessageBoxStyle.Information,
            PlacementTarget   = options?.PlacementTarget ?? placementTarget,
            MotionAnchorMode  = options?.PlacementTarget is null
                ? DialogMotionAnchorMode.FallbackPlacementTarget
                : DialogMotionAnchorMode.ExplicitPlacementTarget,
            HorizontalOffset  = options?.HorizontalOffset,
            VerticalOffset    = options?.VerticalOffset,
            DialogHostType    = options?.HostType ?? DialogHostType.Overlay,
            IsCenterOnStartup = options?.IsCenterOnStartup ?? true,
            Content           = content,
            DataContext       = dataContext,
            HostWidth         = options?.Width ?? double.NaN,
            HostHeight        = options?.Height ?? double.NaN,
            HostMinHeight     = options?.MinHeight ?? 0d,
            HostMaxWidth      = options?.MaxWidth ?? double.PositiveInfinity,
            HostMaxHeight     = options?.MaxHeight ?? double.PositiveInfinity,
            IsConfirmLoading  = options?.IsConfirmLoading ?? false,
            IsLoading         = options?.IsLoading ?? false,
            IsMotionEnabled   = options?.IsMotionEnabled ?? true,
            OkButtonStyle     = options?.OkButtonStyle ?? MessageBoxOkButtonStyle.Primary,
            OkButtonText      = options?.OkButtonText,
            CancelButtonText  = options?.CancelButtonText,
            BeforeCloseAsync  = options?.BeforeCloseAsync
        };
        if (options?.Icon is not null)
        {
            messageBox.Icon = options.Icon;
        }

        if (options is { MinWidth: var minWidth } && !double.IsNaN(minWidth))
        {
            messageBox.HostMinWidth = minWidth;
        }

        return messageBox;
    }

    private static Panel ResolveOverlayLayer(MessageBoxOptions? options, TopLevel? topLevel)
    {
        return OverlayLayerResolver.ResolveOverlayLayer(
            options?.PlacementTarget,
            topLevel,
            nameof(MessageBox));
    }

    #endregion
}
