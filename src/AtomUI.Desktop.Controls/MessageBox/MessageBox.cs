using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Selects whether the OK button of a <see cref="MessageBox" /> is shown as a primary button and is the
/// standard button that the Enter key triggers.
/// </summary>
public enum MessageBoxOkButtonStyle
{
    /// <summary>
    /// Shows the OK button as a default button, and the Enter key triggers no standard button.
    /// </summary>
    Default,
    /// <summary>
    /// Shows the OK button as a primary button and makes it the standard button that the Enter key triggers.
    /// </summary>
    Primary
}

/// <summary>
/// A <see cref="Dialog" /> that shows a status icon beside the message in <c>Content</c>, an OK button and,
/// for <see cref="MessageBoxStyle.Confirm" />, a Cancel button. Dragging is disabled by default; set
/// <see cref="Dialog.IsDragMovable" /> to allow it. The static <c>ShowMessageBoxAsync</c> and
/// <c>ShowMessageBoxModalAsync</c> methods create a message box, show it and return the result of its close.
/// </summary>
public partial class MessageBox : Dialog
{
    // MessageBox.MinWidth/MinHeight size the whole box (main delivered 410 as HostMinWidth; antd ConfirmDialog width 416).
    internal override bool UsesWholeFrameMinimum => true;

    #region Public Properties

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<MessageBox, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="Style" /> property.
    /// </summary>
    public static readonly StyledProperty<MessageBoxStyle> StyleProperty =
        AvaloniaProperty.Register<MessageBox, MessageBoxStyle>(
            nameof(Style),
            MessageBoxStyle.Information);

    /// <summary>
    /// Identifies the <see cref="OkButtonStyle" /> property.
    /// </summary>
    public static readonly StyledProperty<MessageBoxOkButtonStyle> OkButtonStyleProperty =
        AvaloniaProperty.Register<MessageBox, MessageBoxOkButtonStyle>(
            nameof(OkButtonStyle),
            MessageBoxOkButtonStyle.Primary);

    /// <summary>
    /// Identifies the <see cref="OkButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> OkButtonTextProperty =
        AvaloniaProperty.Register<MessageBox, string?>(nameof(OkButtonText));

    /// <summary>
    /// Identifies the <see cref="CancelButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> CancelButtonTextProperty =
        AvaloniaProperty.Register<MessageBox, string?>(nameof(CancelButtonText));

    /// <summary>
    /// Identifies the <see cref="IsCenterOnStartup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCenterOnStartupProperty =
        AvaloniaProperty.Register<MessageBox, bool>(nameof(IsCenterOnStartup), true);

    /// <summary>
    /// Gets or sets the status icon shown beside the message, aligned to its top. The current <see cref="Style" />
    /// supplies this value at template priority, so a value set here replaces the icon of the preset, and clearing
    /// it returns to that icon. <see langword="null" /> hides the icon. The color of the icon follows
    /// <see cref="Style" />, not this value.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the preset that selects the status icon, its color, the standard buttons and the button that
    /// the Escape key activates. A change applies the preset again: <see cref="Icon" />,
    /// <c>StandardButtons</c> and <c>EscapeStandardButton</c> are set at template priority, so a local value set
    /// on one of them stays in effect.
    /// </summary>
    public MessageBoxStyle Style
    {
        get => GetValue(StyleProperty);
        set => SetValue(StyleProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the OK button is shown as a primary button and is the standard button that the Enter
    /// key triggers.
    /// </summary>
    public MessageBoxOkButtonStyle OkButtonStyle
    {
        get => GetValue(OkButtonStyleProperty);
        set => SetValue(OkButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the text of the OK button. <see langword="null" /> leaves the localized standard text that the
    /// button box of the dialog provides.
    /// </summary>
    public string? OkButtonText
    {
        get => GetValue(OkButtonTextProperty);
        set => SetValue(OkButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the text of the Cancel button, which only <see cref="MessageBoxStyle.Confirm" /> shows.
    /// <see langword="null" /> leaves the localized standard text that the button box of the dialog provides.
    /// </summary>
    public string? CancelButtonText
    {
        get => GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the message box is centered in the owner area when it is shown. While it is
    /// <see langword="false" />, the message box is placed by <see cref="Dialog.HorizontalOffset" /> and
    /// <see cref="Dialog.VerticalOffset" />. The value is applied to <c>HorizontalStartupLocation</c> and
    /// <c>VerticalStartupLocation</c> at template priority, so a local value on either stays in effect.
    /// </summary>
    public bool IsCenterOnStartup
    {
        get => GetValue(IsCenterOnStartupProperty);
        set => SetValue(IsCenterOnStartupProperty, value);
    }

    #endregion

    #region Public Events

    /// <summary>
    /// Raised when the message box closes as rejected, right after the <see cref="Dialog.Rejected" /> event; the
    /// close is already committed. This happens when the user activates the Cancel button or the Escape key of
    /// <see cref="MessageBoxStyle.Confirm" />, or when <see cref="Cancel" /> is called.
    /// </summary>
    public event EventHandler? Cancelled;
    /// <summary>
    /// Raised when the message box closes as accepted, right after the <see cref="Dialog.Accepted" /> event; the
    /// close is already committed. This happens when the user activates the OK button, presses the Enter key
    /// while <see cref="OkButtonStyle" /> is <see cref="MessageBoxOkButtonStyle.Primary" />, or calls
    /// <see cref="Confirm" />.
    /// </summary>
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

    /// <summary>
    /// Requests the message box to close as rejected, which raises <see cref="Cancelled" /> once the close is
    /// committed. A handler of <see cref="Dialog.Closing" /> or <see cref="Dialog.BeforeCloseAsync" /> can keep it
    /// open. The call has no effect unless the message box is open.
    /// </summary>
    public void Cancel()
    {
        Reject();
    }

    /// <summary>
    /// Requests the message box to close as accepted, which raises <see cref="Confirmed" /> once the close is
    /// committed. A handler of <see cref="Dialog.Closing" /> or <see cref="Dialog.BeforeCloseAsync" /> can keep it
    /// open. The call has no effect unless the message box is open.
    /// </summary>
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

    /// <summary>
    /// Called when the value of a property of this control changes. After the base call, a change of
    /// <c>Content</c>, <c>ContentTemplate</c> or <see cref="Icon" /> updates the message content, a change of
    /// <see cref="Style" /> applies the preset again and updates the message content, a change of
    /// <see cref="OkButtonStyle" />, <see cref="OkButtonText" /> or <see cref="CancelButtonText" /> updates the
    /// standard buttons, and a change of <see cref="IsCenterOnStartup" /> updates the startup location. An override
    /// has to call the base member.
    /// </summary>
    /// <param name="change">
    /// The property that changed, with its old and new values.
    /// </param>
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

    /// <summary>
    /// Shows a message box that is not modal, with a new <typeparamref name="TView" /> as its message. The task
    /// completes when the message box has closed and returns the result of the close. The method can be called
    /// from any thread.
    /// </summary>
    /// <typeparam name="TView">
    /// The type of the control that shows the message. A parameterless constructor creates the instance.
    /// </typeparam>
    /// <typeparam name="TViewModel">
    /// The type of the value assigned to the data context of the message box.
    /// </typeparam>
    /// <param name="dataContext">
    /// The value assigned to <c>DataContext</c> of the message box. May be <see langword="null" />.
    /// </param>
    /// <param name="options">
    /// The initial values of the message box. <see langword="null" /> applies the default of every option.
    /// </param>
    /// <param name="topLevel">
    /// The top level whose overlay layer hosts the message box when
    /// <see cref="MessageBoxOptions.PlacementTarget" /> is <see langword="null" /> or has no overlay layer. When it
    /// is <see langword="null" /> too, the main window or the main view of a single view application is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the session: the message box is closed with a forced close and the task ends canceled.
    /// </param>
    /// <returns>
    /// <c>DialogCode.Accepted</c> or <c>DialogCode.Rejected</c> according to how the message box closed, or
    /// <see langword="null" /> after a forced close.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// None of the placement target, the top level, the main window or the main view of a single view application
    /// has an overlay layer.
    /// </exception>
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

    /// <summary>
    /// Shows a modal message box, with a new <typeparamref name="TView" /> as its message. The task completes when
    /// the message box has closed and returns the result of the close. The method can be called from any thread.
    /// </summary>
    /// <typeparam name="TView">
    /// The type of the control that shows the message. A parameterless constructor creates the instance.
    /// </typeparam>
    /// <typeparam name="TViewModel">
    /// The type of the value assigned to the data context of the message box.
    /// </typeparam>
    /// <param name="dataContext">
    /// The value assigned to <c>DataContext</c> of the message box. May be <see langword="null" />.
    /// </param>
    /// <param name="options">
    /// The initial values of the message box. <see langword="null" /> applies the default of every option.
    /// </param>
    /// <param name="topLevel">
    /// The top level whose overlay layer hosts the message box when
    /// <see cref="MessageBoxOptions.PlacementTarget" /> is <see langword="null" /> or has no overlay layer. When it
    /// is <see langword="null" /> too, the main window or the main view of a single view application is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the session: the message box is closed with a forced close and the task ends canceled.
    /// </param>
    /// <returns>
    /// <c>DialogCode.Accepted</c> or <c>DialogCode.Rejected</c> according to how the message box closed, or
    /// <see langword="null" /> after a forced close.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// None of the placement target, the top level, the main window or the main view of a single view application
    /// has an overlay layer.
    /// </exception>
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

    /// <summary>
    /// Shows a message box that is not modal, with <paramref name="content" /> as its message. The task completes
    /// when the message box has closed and returns the result of the close. The method can be called from any
    /// thread.
    /// </summary>
    /// <param name="content">
    /// The control shown as the message of the message box.
    /// </param>
    /// <param name="dataContext">
    /// The value assigned to <c>DataContext</c> of the message box. May be <see langword="null" />.
    /// </param>
    /// <param name="options">
    /// The initial values of the message box. <see langword="null" /> applies the default of every option.
    /// </param>
    /// <param name="topLevel">
    /// The top level whose overlay layer hosts the message box when
    /// <see cref="MessageBoxOptions.PlacementTarget" /> is <see langword="null" /> or has no overlay layer. When it
    /// is <see langword="null" /> too, the main window or the main view of a single view application is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the session: the message box is closed with a forced close and the task ends canceled.
    /// </param>
    /// <returns>
    /// <c>DialogCode.Accepted</c> or <c>DialogCode.Rejected</c> according to how the message box closed, or
    /// <see langword="null" /> after a forced close.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// None of the placement target, the top level, the main window or the main view of a single view application
    /// has an overlay layer.
    /// </exception>
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

    /// <summary>
    /// Shows a modal message box, with <paramref name="content" /> as its message. The task completes when the
    /// message box has closed and returns the result of the close. The method can be called from any thread.
    /// </summary>
    /// <param name="content">
    /// The control shown as the message of the message box.
    /// </param>
    /// <param name="dataContext">
    /// The value assigned to <c>DataContext</c> of the message box. May be <see langword="null" />.
    /// </param>
    /// <param name="options">
    /// The initial values of the message box. <see langword="null" /> applies the default of every option.
    /// </param>
    /// <param name="topLevel">
    /// The top level whose overlay layer hosts the message box when
    /// <see cref="MessageBoxOptions.PlacementTarget" /> is <see langword="null" /> or has no overlay layer. When it
    /// is <see langword="null" /> too, the main window or the main view of a single view application is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the session: the message box is closed with a forced close and the task ends canceled.
    /// </param>
    /// <returns>
    /// <c>DialogCode.Accepted</c> or <c>DialogCode.Rejected</c> according to how the message box closed, or
    /// <see langword="null" /> after a forced close.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// None of the placement target, the top level, the main window or the main view of a single view application
    /// has an overlay layer.
    /// </exception>
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
