using System.ComponentModel;
using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A dialog with a header, a body and a footer that holds buttons, shown in the overlay layer of a window or in
/// a window of its own, as <see cref="AtomUI.Desktop.Controls.DialogHostType" /> selects. The control holds
/// the state of the open dialog; its host creates the visible surface and lays it out.
/// </summary>
public partial class Dialog : TemplatedControl,
                              IMotionAwareControl,
                              IDialog,
                              ISemanticPartCrossRootProvider
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Dialog, string?>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="TitleIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> TitleIconProperty =
        AvaloniaProperty.Register<Dialog, PathIcon?>(nameof(TitleIcon));

    /// <summary>
    /// Identifies the <see cref="Content" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty = ContentPresenter.ContentProperty.AddOwner<Dialog>();

    /// <summary>
    /// Identifies the <see cref="ContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentPresenter.ContentTemplateProperty.AddOwner<Dialog>();

    /// <summary>
    /// Identifies the <see cref="IsOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Dialog, bool>(
            nameof(IsOpen),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="IsModal" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsModal), true);

    /// <summary>
    /// Identifies the <see cref="IsResizable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsResizableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsResizable));

    /// <summary>
    /// Identifies the <see cref="IsClosable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsClosable), true);

    /// <summary>
    /// Identifies the <see cref="IsMaskClosable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMaskClosableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsMaskClosable), true);

    /// <summary>
    /// Identifies the <see cref="IsMaximizable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMaximizableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsMaximizable));

    /// <summary>
    /// Identifies the <see cref="IsMinimizable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMinimizableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsMinimizable), true);

    /// <summary>
    /// Identifies the <see cref="IsDragMovable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDragMovableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsDragMovable), true);

    /// <summary>
    /// Identifies the <see cref="PlacementTarget" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> PlacementTargetProperty =
        AvaloniaProperty.Register<Dialog, Control?>(nameof(PlacementTarget));

    /// <summary>
    /// Identifies the <see cref="OverlayScope" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> OverlayScopeProperty =
        AvaloniaProperty.Register<Dialog, Control?>(nameof(OverlayScope));

    /// <summary>
    /// Identifies the <see cref="HorizontalStartupLocation" /> property.
    /// </summary>
    public static readonly StyledProperty<DialogHorizontalAnchor> HorizontalStartupLocationProperty =
        AvaloniaProperty.Register<Dialog, DialogHorizontalAnchor>(nameof(HorizontalStartupLocation),
            DialogHorizontalAnchor.Center);

    /// <summary>
    /// Identifies the <see cref="VerticalStartupLocation" /> property.
    /// </summary>
    public static readonly StyledProperty<DialogVerticalAnchor> VerticalStartupLocationProperty =
        AvaloniaProperty.Register<Dialog, DialogVerticalAnchor>(nameof(VerticalStartupLocation),
            DialogVerticalAnchor.Center);

    /// <summary>
    /// Identifies the <see cref="HorizontalOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<Dimension?> HorizontalOffsetProperty =
        AvaloniaProperty.Register<Dialog, Dimension?>(nameof(HorizontalOffset));

    /// <summary>
    /// Identifies the <see cref="VerticalOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<Dimension?> VerticalOffsetProperty =
        AvaloniaProperty.Register<Dialog, Dimension?>(nameof(VerticalOffset));

    /// <summary>
    /// Identifies the <see cref="IsTopmost" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTopmostProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsTopmost));

    /// <summary>
    /// Identifies the <see cref="Result" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ResultProperty =
        AvaloniaProperty.Register<Dialog, object?>(nameof(Result));

    /// <summary>
    /// Identifies the <see cref="StandardButtons" /> property.
    /// </summary>
    public static readonly StyledProperty<DialogStandardButtons> StandardButtonsProperty =
        DialogButtonBox.StandardButtonsProperty.AddOwner<Dialog>();

    /// <summary>
    /// Identifies the <see cref="DefaultStandardButton" /> property.
    /// </summary>
    public static readonly StyledProperty<DialogStandardButton> DefaultStandardButtonProperty =
        DialogButtonBox.DefaultStandardButtonProperty.AddOwner<Dialog>();

    /// <summary>
    /// Identifies the <see cref="EscapeStandardButton" /> property.
    /// </summary>
    public static readonly StyledProperty<DialogStandardButton> EscapeStandardButtonProperty =
        DialogButtonBox.EscapeStandardButtonProperty.AddOwner<Dialog>();

    /// <summary>
    /// Identifies the <see cref="IsFooterVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFooterVisibleProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsFooterVisible), true);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Dialog>();

    /// <summary>
    /// Identifies the <see cref="DialogHostType" /> property.
    /// </summary>
    public static readonly StyledProperty<DialogHostType> DialogHostTypeProperty =
        AvaloniaProperty.Register<Dialog, DialogHostType>(nameof(DialogHostType), DialogHostType.Overlay);

    /// <summary>
    /// Identifies the <see cref="IsPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPinnedOpenProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsPinnedOpen));

    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsLoading));

    /// <summary>
    /// Identifies the <see cref="IsConfirmLoading" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsConfirmLoadingProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsConfirmLoading));

    // The dialog control only holds the state of the session and takes no part in the visual layout. The Host*
    // properties below size the surface that the presenter shows.
    /// <summary>
    /// Identifies the <see cref="HostWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HostWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostWidth), double.NaN);

    /// <summary>
    /// Identifies the <see cref="HostHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HostHeightProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostHeight), double.NaN);

    /// <summary>
    /// Identifies the <see cref="HostMinWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HostMinWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostMinWidth), 0d);

    /// <summary>
    /// Identifies the <see cref="HostMinHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HostMinHeightProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostMinHeight), 0d);

    /// <summary>
    /// Identifies the <see cref="HostMaxWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HostMaxWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostMaxWidth), double.PositiveInfinity);

    /// <summary>
    /// Identifies the <see cref="HostMaxHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HostMaxHeightProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostMaxHeight), double.PositiveInfinity);

    /// <summary>
    /// Gets or sets the text shown in the header of the dialog. The Window host uses it as the title of its window.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon shown in the header of the dialog. The Window host uses it as the logo of its window.
    /// </summary>
    public PathIcon? TitleIcon
    {
        get => GetValue(TitleIconProperty);
        set => SetValue(TitleIconProperty, value);
    }

    /// <summary>
    /// Gets or sets the content of the body of the dialog. In XAML, child elements of the dialog are assigned here.
    /// </summary>
    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that builds the body of the dialog from <see cref="Content" />. The default,
    /// <see langword="null" />, uses the data template that matches the content. A control is shown as it is.
    /// </summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets an optional <see cref="IAvaloniaDependencyResolver" /> for this dialog. The dialog and its
    /// presenters do not read this value. The default is <see langword="null" />.
    /// </summary>
    public IAvaloniaDependencyResolver? DependencyResolver { get; set; }

    /// <summary>
    /// Gets or sets whether the dialog is open. <see langword="true" /> opens the dialog, and
    /// <see langword="false" /> requests a close with the reason <see cref="DialogCloseReason.Programmatic" /> that
    /// raises <see cref="Closing" /> and can be stopped there or by <see cref="BeforeCloseAsync" />. A session can
    /// start only while the placement target or the dialog control is attached. The dialog sets the value itself,
    /// to <see langword="false" /> once a close is committed and back to <see langword="true" /> when a close is
    /// stopped. A binding receives these writes because the default binding mode is two-way.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the dialog is modal. In the Overlay host a modal dialog is shown with a mask behind
    /// it. In the Window host it is shown as a modal window of its owner where the platform supports window modal
    /// dialogs. A modal dialog cannot be minimized, whatever <see cref="IsMinimizable" /> says.
    /// </summary>
    public bool IsModal
    {
        get => GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can resize the dialog: with the resizer that the Overlay host shows, or by
    /// resizing the window in the Window host.
    /// </summary>
    public bool IsResizable
    {
        get => GetValue(IsResizableProperty);
        set => SetValue(IsResizableProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the dialog has a usable close button: the one in the header for the Overlay host, or
    /// the caption button of the window for the Window host. The other ways to close the dialog are not affected.
    /// </summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a click on the mask of a modal dialog, outside the surface, starts closing it. This
    /// applies to the Overlay host only: the Window host has no mask, so a click outside never closes a dialog
    /// there. The close button is controlled by <see cref="IsClosable" /> instead.
    /// </summary>
    public bool IsMaskClosable
    {
        get => GetValue(IsMaskClosableProperty);
        set => SetValue(IsMaskClosableProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can maximize the dialog. In the Overlay host this enables the maximize button
    /// in the header. In the Window host the window can be maximized only while <see cref="HostMaxWidth" /> and
    /// <see cref="HostMaxHeight" /> are not limited.
    /// </summary>
    public bool IsMaximizable
    {
        get => GetValue(IsMaximizableProperty);
        set => SetValue(IsMaximizableProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can minimize the dialog. It applies to the Window host only, and only while
    /// the dialog is not modal.
    /// </summary>
    public bool IsMinimizable
    {
        get => GetValue(IsMinimizableProperty);
        set => SetValue(IsMinimizableProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can move the dialog by dragging its header. In the Overlay host this does not
    /// work while the dialog is maximized.
    /// </summary>
    public bool IsDragMovable
    {
        get => GetValue(IsDragMovableProperty);
        set => SetValue(IsDragMovableProperty, value);
    }

    /// <summary>
    /// Gets or sets the control from which the overlay layer that hosts the dialog and the anchor of its motion
    /// are resolved. When it is <see langword="null" />, a static method uses the placement target that was passed
    /// to it.
    /// </summary>
    [ResolveByName]
    public Control? PlacementTarget
    {
        get => GetValue(PlacementTargetProperty);
        set => SetValue(PlacementTargetProperty, value);
    }

    /// <summary>
    /// Gets or sets a control that confines the overlay host of the dialog to a scope. The mask, the size of the
    /// surface and its centering are then bounded by that scope, as in the inline mode of the antd Modal. The
    /// default, <see langword="null" />, keeps the host in the overlay layer of the owning TopLevel. The Window
    /// host ignores the value. When the scope has no usable scope layer, the host falls back to the TopLevel
    /// host. A scoped host has no window frame contract, so it takes no part in the inset within the frame or
    /// in the suppression of the drawn window chrome.
    /// </summary>
    public Control? OverlayScope
    {
        get => GetValue(OverlayScopeProperty);
        set => SetValue(OverlayScopeProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal anchor of the dialog within the owner area when the dialog is shown.
    /// <see cref="DialogHorizontalAnchor.Custom" /> places it by <see cref="HorizontalOffset" />.
    /// </summary>
    public DialogHorizontalAnchor HorizontalStartupLocation
    {
        get => GetValue(HorizontalStartupLocationProperty);
        set => SetValue(HorizontalStartupLocationProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical anchor of the dialog within the owner area when the dialog is shown.
    /// <see cref="DialogVerticalAnchor.Custom" /> places it by <see cref="VerticalOffset" />.
    /// </summary>
    public DialogVerticalAnchor VerticalStartupLocation
    {
        get => GetValue(VerticalStartupLocationProperty);
        set => SetValue(VerticalStartupLocationProperty, value);
    }

    /// <summary>
    /// Gets or sets the distance of the dialog from the left edge of the owner area that
    /// <see cref="DialogHorizontalAnchor.Custom" /> uses. It is resolved against the width of the owner area;
    /// <see langword="null" /> counts as 0.
    /// </summary>
    public Dimension? HorizontalOffset
    {
        get => GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the distance of the dialog from the top edge of the owner area that
    /// <see cref="DialogVerticalAnchor.Custom" /> uses. It is resolved against the height of the owner area;
    /// <see langword="null" /> counts as 0.
    /// </summary>
    public Dimension? VerticalOffset
    {
        get => GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the host window of the dialog stays above the other windows. The Window host binds the
    /// value to <c>Topmost</c> of its window; the Overlay host does not use it.
    /// </summary>
    public bool IsTopmost
    {
        get => GetValue(IsTopmostProperty);
        set => SetValue(IsTopmostProperty, value);
    }

    /// <summary>
    /// Gets or sets the value the dialog closes with. A close requested in code, such as by setting
    /// <see cref="IsOpen" /> to <see langword="false" />, closes with the current value, and every close replaces
    /// the value with the result of its close request before <see cref="Finished" /> is raised.
    /// </summary>
    public object? Result
    {
        get => GetValue(ResultProperty);
        set => SetValue(ResultProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the dialog is shown in the overlay layer of a window or in a window of its own. The
    /// value is read when the dialog opens. <see cref="DialogHostType.Window" /> falls back to
    /// <see cref="DialogHostType.Overlay" /> on platforms without native window support.
    /// </summary>
    public DialogHostType DialogHostType
    {
        get => GetValue(DialogHostTypeProperty);
        set => SetValue(DialogHostTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the standard buttons that the footer creates, as a flag combination. The default,
    /// <see cref="DialogStandardButton.NoButton" />, shows none of them.
    /// </summary>
    public DialogStandardButtons StandardButtons
    {
        get => GetValue(StandardButtonsProperty);
        set => SetValue(StandardButtonsProperty, value);
    }

    /// <summary>
    /// Gets or sets the standard button that the Enter key triggers and that is shown as the primary button. It
    /// takes effect only while <see cref="StandardButtons" /> contains it. The default,
    /// <see cref="DialogStandardButton.NoButton" />, selects none.
    /// </summary>
    public DialogStandardButton DefaultStandardButton
    {
        get => GetValue(DefaultStandardButtonProperty);
        set => SetValue(DefaultStandardButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets the standard button that the Escape key triggers. It takes effect only while
    /// <see cref="StandardButtons" /> contains it. The default, <see cref="DialogStandardButton.NoButton" />, is
    /// not set: while it stays so, the Cancel button takes the role when <see cref="StandardButtons" /> contains
    /// it, and no button does once <see cref="DialogStandardButton.NoButton" /> is set.
    /// </summary>
    public DialogStandardButton EscapeStandardButton
    {
        get => GetValue(EscapeStandardButtonProperty);
        set => SetValue(EscapeStandardButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the footer of the dialog is shown. The footer is shown only while the dialog is not
    /// loading and has at least one standard or custom button.
    /// </summary>
    public bool IsFooterVisible
    {
        get => GetValue(IsFooterVisibleProperty);
        set => SetValue(IsFooterVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets whether motion is played for the dialog. The value is passed to the parts that the template
    /// creates, such as the header. Without a local value, the theme takes it from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the dialog stays open whatever the user does; the semantic preview uses this. While it
    /// is <see langword="true" />, the close entries that the user has no longer request a close: the close button
    /// in the header, a click on the mask, Escape and the buttons of the footer. Setting <see cref="IsOpen" /> to
    /// <see langword="false" /> in code still closes the dialog. It applies to the Overlay host only: the Window
    /// host follows the close flow of the native window and ignores this value.
    /// </summary>
    public bool IsPinnedOpen
    {
        get => GetValue(IsPinnedOpenProperty);
        set => SetValue(IsPinnedOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the body of the dialog shows a loading skeleton in place of <see cref="Content" />.
    /// While it is <see langword="true" />, the footer is hidden.
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether close requests that the user initiates are ignored, such as the click on a button of
    /// the footer. Use it while an asynchronous action runs. A close requested in code, for example by setting
    /// <see cref="IsOpen" /> to <see langword="false" />, still closes the dialog.
    /// </summary>
    public bool IsConfirmLoading
    {
        get => GetValue(IsConfirmLoadingProperty);
        set => SetValue(IsConfirmLoadingProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the whole dialog, header and footer included, in DIPs. <see cref="double.NaN" />
    /// lets the content determine the width.
    /// </summary>
    public double HostWidth
    {
        get => GetValue(HostWidthProperty);
        set => SetValue(HostWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the whole dialog, header and footer included, in DIPs. <see cref="double.NaN" />
    /// lets the content determine the height.
    /// </summary>
    public double HostHeight
    {
        get => GetValue(HostHeightProperty);
        set => SetValue(HostHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum width of the whole dialog, header and footer included, in DIPs. <c>0</c> sets no
    /// lower bound.
    /// </summary>
    public double HostMinWidth
    {
        get => GetValue(HostMinWidthProperty);
        set => SetValue(HostMinWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum height of the whole dialog, header and footer included, in DIPs. <c>0</c> sets no
    /// lower bound.
    /// </summary>
    public double HostMinHeight
    {
        get => GetValue(HostMinHeightProperty);
        set => SetValue(HostMinHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum width of the whole dialog, header and footer included, in DIPs.
    /// <see cref="double.PositiveInfinity" /> sets no upper bound.
    /// </summary>
    public double HostMaxWidth
    {
        get => GetValue(HostMaxWidthProperty);
        set => SetValue(HostMaxWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum height of the whole dialog, header and footer included, in DIPs.
    /// <see cref="double.PositiveInfinity" /> sets no upper bound.
    /// </summary>
    public double HostMaxHeight
    {
        get => GetValue(HostMaxHeightProperty);
        set => SetValue(HostMaxHeightProperty, value);
    }

    /// <summary>
    /// Gets the custom buttons that the footer shows in addition to <see cref="StandardButtons" />. Changes to the
    /// list take effect while the dialog is open. The footer is shown only when this list is not empty or
    /// <see cref="StandardButtons" /> has a button.
    /// </summary>
    public AvaloniaList<DialogButton> CustomButtons { get; } = new();

    /// <summary>
    /// Gets or sets a callback that receives the buttons of the footer so that their properties can be set in code.
    /// It is called with the buttons whenever the host of the dialog creates them, and at once with the current
    /// buttons when this property is set while they exist.
    /// </summary>
    public Action<IReadOnlyList<DialogButton>>? ButtonsConfigure
    {
        get => _buttonsConfigure;
        set
        {
            _buttonsConfigure = value;
            if (_surfaceButtons is not null)
            {
                value?.Invoke(_surfaceButtons);
            }
        }
    }

    /// <summary>
    /// Gets or sets a callback that is awaited before the dialog closes and decides whether the close proceeds. It
    /// receives a <see cref="DialogClosingContext" /> that describes the pending close and returns
    /// <see langword="true" /> to close the dialog or <see langword="false" /> to keep it open. It is not called for
    /// a forced close or when a <see cref="Closing" /> handler has already canceled the close. The default,
    /// <see langword="null" />, closes without asking.
    /// </summary>
    public Func<DialogClosingContext, ValueTask<bool>>? BeforeCloseAsync { get; set; }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when the dialog has closed and its host window or overlay has closed. It is the last event of a close
    /// and follows <see cref="Finished" />.
    /// </summary>
    public event EventHandler? Closed;
    /// <summary>
    /// Raised when the dialog has been shown and has received the input focus.
    /// </summary>
    public event EventHandler? Opened;
    /// <summary>
    /// Raised when a close is requested, before the dialog starts closing. Set <see cref="CancelEventArgs.Cancel" />
    /// to keep the dialog open. A forced close raises the event too, but closes the dialog regardless.
    /// </summary>
    public event EventHandler<CancelEventArgs>? Closing;
    /// <summary>
    /// Raised when the dialog closes with the result <see cref="DialogCode.Accepted" />, after
    /// <see cref="IDialog.Accept" /> or the click of a button whose role requests this result. The close is already
    /// committed; <see cref="Finished" /> is raised next.
    /// </summary>
    public event EventHandler? Accepted;
    /// <summary>
    /// Raised when the dialog closes with the result <see cref="DialogCode.Rejected" />, after
    /// <see cref="IDialog.Reject" /> or the click of a button whose role requests this result. The close is already
    /// committed; <see cref="Finished" /> is raised next.
    /// </summary>
    public event EventHandler? Rejected;
    /// <summary>
    /// Raised for every close that is committed, after <see cref="Accepted" /> or <see cref="Rejected" /> and before
    /// the host of the dialog is closed. The arguments carry the result of the close request, which is
    /// <see langword="null" /> when the dialog closed without a result.
    /// </summary>
    public event EventHandler<DialogFinishedEventArgs>? Finished;
    /// <summary>
    /// Raised when a button of the dialog is clicked, before the role of the button acts on the click. Set
    /// <see cref="DialogButtonClickedEventArgs.Handled" /> to keep the role of the button from requesting a close.
    /// </summary>
    public event EventHandler<DialogButtonClickedEventArgs>? ButtonClicked;

    #endregion

    #region ISemanticPartCrossRootProvider 实现

    /// <summary>
    /// Raised when a root returned by <see cref="GetCrossRoots" /> appears or disappears. The Window host raises it
    /// once its window is shown and again when the surface of the dialog is ready.
    /// </summary>
    public event EventHandler? CrossRootsChanged;

    /// <summary>
    /// Returns the visual root that hosts the dialog outside the visual tree of this control: the overlay presenter
    /// while it is in a visual tree, or the host window of the Window host while that window is visible.
    /// </summary>
    /// <returns>
    /// A list with one root, or an empty list when the dialog has no visible host.
    /// </returns>
    public IReadOnlyList<Visual> GetCrossRoots()
    {
        var presenter = _session?.Presenter;
        if (presenter is Visual overlayRoot && overlayRoot.GetVisualParent() is not null)
        {
            return [overlayRoot];
        }

        // Window host: the cross root is the host window, a TopLevel of its own. The semantic preview resolves and
        // highlights the parts in that window, and its adorner goes to the AdornerLayer of that window. The window is
        // reported for that resolution only: styles of the owner scope still do not cascade across TopLevels.
        if (presenter is WindowDialogPresenter { HostWindow.IsVisible: true } windowPresenter)
        {
            return [windowPresenter.HostWindow];
        }

        return Array.Empty<Visual>();
    }

    internal void NotifyCrossRootsChanged()
    {
        CrossRootsChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Dialog, double> OffsetXProperty =
        AvaloniaProperty.RegisterDirect<Dialog, double>(
            nameof(OffsetX),
            o => o.OffsetX,
            (o, v) => o.OffsetX = v);

    internal static readonly DirectProperty<Dialog, double> OffsetYProperty =
        AvaloniaProperty.RegisterDirect<Dialog, double>(
            nameof(OffsetY),
            o => o.OffsetY,
            (o, v) => o.OffsetY = v);

    internal static readonly DirectProperty<Dialog, bool> EffectiveMinimizableProperty =
        AvaloniaProperty.RegisterDirect<Dialog, bool>(
            nameof(EffectiveMinimizable),
            o => o.EffectiveMinimizable,
            (o, v) => o.EffectiveMinimizable = v);

    private double _offsetX;

    internal double OffsetX
    {
        get => _offsetX;
        set => SetAndRaise(OffsetXProperty, ref _offsetX, value);
    }

    private double _offsetY;

    internal double OffsetY
    {
        get => _offsetY;
        set => SetAndRaise(OffsetYProperty, ref _offsetY, value);
    }

    private bool _effectiveMinimizable;

    internal bool EffectiveMinimizable
    {
        get => _effectiveMinimizable;
        set => SetAndRaise(EffectiveMinimizableProperty, ref _effectiveMinimizable, value);
    }

    internal DialogMotionAnchorMode MotionAnchorMode { get; set; }

    internal bool UsesPlacementTargetAsMotionAnchor =>
        MotionAnchorMode switch
        {
            DialogMotionAnchorMode.ExplicitPlacementTarget => PlacementTarget is not null,
            DialogMotionAnchorMode.FallbackPlacementTarget => false,
            _ => PlacementTarget is not null
        };

    private protected IReadOnlyList<DialogButton> SurfaceButtons =>
        _surfaceButtons ?? Array.Empty<DialogButton>();

    #endregion

    #region 内部协作 API

    /// <summary>
    /// How the terminal's <c>MinWidth</c>/<c>MinHeight</c> Tokens size the surface content frame: the Dialog Tokens are
    /// the content viewport baseline (frame = Token + padding + border); a terminal returning true sizes the whole frame
    /// (frame = max(Token, padding + border)), like antd's confirm <c>width</c>.
    /// </summary>
    internal virtual bool UsesWholeFrameMinimum => false;

    #endregion

    #region 运行时字段

    private Action<IReadOnlyList<DialogButton>>? _buttonsConfigure;
    private IReadOnlyList<DialogButton>? _surfaceButtons;

    #endregion

    internal virtual object? GetSurfaceContent()
    {
        return Content;
    }

    internal virtual IDataTemplate? GetSurfaceContentTemplate()
    {
        return ContentTemplate;
    }

    internal virtual void ConfigureSurfaceButtons(IReadOnlyList<DialogButton> buttons)
    {
        _surfaceButtons = buttons;
        _buttonsConfigure?.Invoke(buttons);
    }

    internal virtual void ReleaseSurfaceButtons(IReadOnlyList<DialogButton> buttons)
    {
        if (ReferenceEquals(_surfaceButtons, buttons))
        {
            _surfaceButtons = null;
        }
    }
}
