using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Options for the message box that a static method of <see cref="MessageBox" /> creates. Each option is the
/// initial value of the property of the same name on that message box, or of the dialog property that its
/// description names.
/// </summary>
public record MessageBoxOptions
{
    /// <summary>
    /// Initial value of <see cref="Dialog.DialogHostType" />: whether the message box is shown in the overlay
    /// layer of a window or in a window of its own. <see cref="DialogHostType.Window" /> falls back to
    /// <see cref="DialogHostType.Overlay" /> on platforms without native window support.
    /// </summary>
    public DialogHostType HostType { get; init; } = DialogHostType.Overlay;
    /// <summary>
    /// Initial value of <see cref="Dialog.Title" />: the text shown in the header of the message box.
    /// The Window host uses it as the title of its window.
    /// </summary>
    public string? Title { get; init; }
    /// <summary>
    /// Initial value of <see cref="MessageBox.Icon" />: the status icon shown beside the message.
    /// <see langword="null" /> keeps the icon that <see cref="MessageBox.Style" /> selects.
    /// </summary>
    public PathIcon? Icon { get; init; }
    /// <summary>
    /// Initial value of <see cref="MessageBox.Style" />: the preset that selects the status icon and the standard
    /// buttons. <see cref="MessageBoxStyle.Confirm" /> shows an exclamation icon with OK and Cancel buttons, and
    /// the Escape key triggers Cancel. Every other value shows an OK button only, with the icon of the preset, or
    /// none for <see cref="MessageBoxStyle.Normal" />.
    /// </summary>
    public MessageBoxStyle  Style { get; init; } = MessageBoxStyle.Information;
    /// <summary>
    /// Initial value of <see cref="Dialog.IsDragMovable" />: whether the user can move the message box by dragging
    /// its header.
    /// </summary>
    public bool IsDragMovable { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.IsMaskClosable" />: whether a click on the modal mask outside the
    /// message box starts closing it. This applies to the Overlay host only.
    /// </summary>
    public bool IsMaskClosable { get; init; } = true;
    /// <summary>
    /// Initial value of <see cref="Dialog.PlacementTarget" />: the control from which the overlay layer that hosts
    /// the message box and the anchor of its motion are resolved. <see langword="null" /> resolves the overlay layer
    /// from the <c>topLevel</c> argument of the static method and uses that layer as the placement target.
    /// </summary>
    public Control? PlacementTarget { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.HorizontalOffset" />: the distance of the message box from the left edge
    /// of the owner area that <see cref="DialogHorizontalAnchor.Custom" /> uses. It is resolved against the width
    /// of the owner area; <see langword="null" /> counts as 0. It applies only while
    /// <see cref="IsCenterOnStartup" /> is false.
    /// </summary>
    public Dimension? HorizontalOffset { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.VerticalOffset" />: the distance of the message box from the top edge
    /// of the owner area that <see cref="DialogVerticalAnchor.Custom" /> uses. It is resolved against the height
    /// of the owner area; <see langword="null" /> counts as 0. It applies only while
    /// <see cref="IsCenterOnStartup" /> is false.
    /// </summary>
    public Dimension? VerticalOffset { get; init; }
    /// <summary>
    /// Initial value of <see cref="MessageBox.IsCenterOnStartup" />: whether the message box is centered in the
    /// owner area when it is shown. While it is false, the message box is placed by
    /// <see cref="HorizontalOffset" /> and <see cref="VerticalOffset" />.
    /// </summary>
    public bool IsCenterOnStartup { get; init; } = true;

    /// <summary>
    /// Initial value of <see cref="Dialog.IsLoading" />: whether the message box is shown in its loading state.
    /// While it is loading, the content area shows a skeleton and the footer is hidden.
    /// </summary>
    public bool IsLoading { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.IsConfirmLoading" />: whether a confirm action of the user is still
    /// running. While it is true, buttons with an accept role show a loading state, and a close that the user
    /// starts is ignored. A forced close still closes the message box.
    /// </summary>
    public bool IsConfirmLoading { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.IsMotionEnabled" />: whether motion is played for the message box.
    /// The value is passed to the parts that its template creates, such as the header.
    /// </summary>
    public bool IsMotionEnabled { get; init; } = true;
    /// <summary>
    /// Initial value of <see cref="MessageBox.OkButtonStyle" />: whether the OK button is shown as a primary
    /// button and is the standard button that the Enter key triggers.
    /// </summary>
    public MessageBoxOkButtonStyle OkButtonStyle { get; init; } = MessageBoxOkButtonStyle.Primary;
    /// <summary>
    /// Initial value of <see cref="MessageBox.OkButtonText" />: the text of the OK button.
    /// <see langword="null" /> leaves the text to the theme of the button.
    /// </summary>
    public string? OkButtonText { get; init; }
    /// <summary>
    /// Initial value of <see cref="MessageBox.CancelButtonText" />: the text of the Cancel button.
    /// <see langword="null" /> leaves the text to the theme of the button.
    /// </summary>
    public string? CancelButtonText { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.BeforeCloseAsync" />: a callback that is awaited before the message box
    /// closes and decides whether the close proceeds. It receives a <see cref="DialogClosingContext" /> that
    /// describes the pending close and returns <see langword="true" /> to close the message box or
    /// <see langword="false" /> to keep it open. It is not called when the close is forced or was canceled by a
    /// <see cref="Dialog.Closing" /> handler; <see langword="null" /> closes without asking.
    /// </summary>
    public Func<DialogClosingContext, ValueTask<bool>>? BeforeCloseAsync { get; init; }

    /// <summary>
    /// Initial value of <see cref="Dialog.HostMaxHeight" />: the maximum height of the whole message box, header
    /// and footer included, in DIPs. <c>PositiveInfinity</c> sets no upper bound.
    /// </summary>
    public double MaxHeight { get; init; } = double.PositiveInfinity;
    /// <summary>
    /// Initial value of <see cref="Dialog.HostMaxWidth" />: the maximum width of the whole message box, header
    /// and footer included, in DIPs. <c>PositiveInfinity</c> sets no upper bound.
    /// </summary>
    public double MaxWidth { get; init; } = double.PositiveInfinity;
    /// <summary>
    /// Initial value of <see cref="Dialog.HostMinHeight" />: the minimum height of the whole message box, header
    /// and footer included, in DIPs.
    /// </summary>
    public double MinHeight { get; init; } = 0d;
    /// <summary>
    /// Initial value of <see cref="Dialog.HostMinWidth" />: the minimum width of the whole message box, header
    /// and footer included, in DIPs. <c>NaN</c>, the default, keeps the minimum width that the theme sets from the
    /// <c>MessageBoxToken.MinWidth</c> token.
    /// </summary>
    public double MinWidth { get; init; } = double.NaN;
    /// <summary>
    /// Initial value of <see cref="Dialog.HostWidth" />: the width of the whole message box, header and footer
    /// included, in DIPs. <c>NaN</c> lets the content determine the width.
    /// </summary>
    public double Width { get; init; } = double.NaN;
    /// <summary>
    /// Initial value of <see cref="Dialog.HostHeight" />: the height of the whole message box, header and footer
    /// included, in DIPs. <c>NaN</c> lets the content determine the height.
    /// </summary>
    public double Height { get; init; } = double.NaN;
}
