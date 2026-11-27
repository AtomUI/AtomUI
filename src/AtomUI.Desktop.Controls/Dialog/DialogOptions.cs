using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Options for the dialog shown by a static method of <see cref="Dialog" />. Each option is the initial value
/// of the <see cref="Dialog" /> property of the same name on the dialog that is created.
/// </summary>
public record DialogOptions
{
    /// <summary>
    /// Initial value of <see cref="Dialog.Title" />: the text shown in the header of the dialog. The Window host uses
    /// it as the title of its window.
    /// </summary>
    public string? Title { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.TitleIcon" />: the icon shown in the header of the dialog. The Window host
    /// uses it as the logo of its window.
    /// </summary>
    public PathIcon? TitleIcon { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.IsResizable" />: whether the user can resize the dialog, with the resizer
    /// that the Overlay host shows or by resizing the window in the Window host.
    /// </summary>
    public bool IsResizable { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.IsClosable" />: whether the dialog has a usable close button, the one in the
    /// header for the Overlay host or the caption button of the window for the Window host.
    /// </summary>
    public bool IsClosable { get; init; } = true;
    /// <summary>
    /// Initial value of <see cref="Dialog.IsMaskClosable" />: whether a click on the modal mask outside the dialog starts
    /// closing it. This applies to the Overlay host only; the Window host has no mask, so a click outside never
    /// closes a dialog there. The close button is controlled by <see cref="IsClosable" /> instead.
    /// </summary>
    public bool IsMaskClosable { get; init; } = true;
    /// <summary>
    /// Initial value of <see cref="Dialog.IsMaximizable" />: whether the user can maximize the dialog. In the Overlay
    /// host this enables the maximize button in the header.
    /// </summary>
    public bool IsMaximizable { get; init; }
    
    /// <summary>
    /// Initial value of <see cref="Dialog.HostMaxHeight" />: the maximum height of the whole dialog, header and footer
    /// included, in DIPs. <c>PositiveInfinity</c> sets no upper bound.
    /// </summary>
    public double HostMaxHeight { get; init; } = double.PositiveInfinity;
    /// <summary>
    /// Initial value of <see cref="Dialog.HostMaxWidth" />: the maximum width of the whole dialog, header and footer
    /// included, in DIPs. <c>PositiveInfinity</c> sets no upper bound.
    /// </summary>
    public double HostMaxWidth { get; init; } = double.PositiveInfinity;
    /// <summary>
    /// Initial value of <see cref="Dialog.HostMinHeight" />: the minimum height of the whole dialog, header and footer
    /// included, in DIPs. <c>0</c> sets no lower bound.
    /// </summary>
    public double HostMinHeight { get; init; } = 0.0d;
    /// <summary>
    /// Initial value of <see cref="Dialog.HostMinWidth" />: the minimum width of the whole dialog, header and footer
    /// included, in DIPs. <c>0</c> sets no lower bound.
    /// </summary>
    public double HostMinWidth { get; init; } = 0.0d;
    /// <summary>
    /// Initial value of <see cref="Dialog.HostWidth" />: the width of the whole dialog, header and footer included, in
    /// DIPs. <c>NaN</c> lets the content determine the width.
    /// </summary>
    public double HostWidth { get; init; } = double.NaN;
    /// <summary>
    /// Initial value of <see cref="Dialog.HostHeight" />: the height of the whole dialog, header and footer included,
    /// in DIPs. <c>NaN</c> lets the content determine the height.
    /// </summary>
    public double HostHeight { get; init; } = double.NaN;

    /// <summary>
    /// Initial value of <see cref="Dialog.IsMinimizable" />: whether the user can minimize the dialog. It applies to
    /// the Window host only, and only while the dialog is not modal.
    /// </summary>
    public bool IsMinimizable { get; init; } = true;
    
    /// <summary>
    /// Initial value of <see cref="Dialog.IsDragMovable" />: whether the user can move the dialog by dragging its
    /// header.
    /// </summary>
    public bool IsDragMovable { get; init; } = true;
    /// <summary>
    /// Initial value of <see cref="Dialog.IsFooterVisible" />: whether the footer of the dialog is shown. The footer is
    /// shown only while the dialog is not loading and has at least one standard or custom button.
    /// </summary>
    public bool IsFooterVisible { get; init; } = true;
    /// <summary>
    /// Initial value of <see cref="Dialog.IsMotionEnabled" />: whether motion is played for the dialog. The value is
    /// passed to the parts that its template creates, such as the header.
    /// </summary>
    public bool IsMotionEnabled { get; init; } = true;
    /// <summary>
    /// Initial value of <see cref="Dialog.PlacementTarget" />: the control from which the overlay layer that hosts the
    /// dialog and the anchor of its motion are resolved. <see langword="null" /> uses the placement target passed
    /// to the static method.
    /// </summary>
    public Control? PlacementTarget { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.HorizontalOffset" />: the distance of the dialog from the left edge of the owner
    /// area that <see cref="DialogHorizontalAnchor.Custom" /> uses. It is resolved against the width of the owner
    /// area; <see langword="null" /> counts as 0.
    /// </summary>
    public Dimension? HorizontalOffset { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.VerticalOffset" />: the distance of the dialog from the top edge of the owner
    /// area that <see cref="DialogVerticalAnchor.Custom" /> uses. It is resolved against the height of the owner
    /// area; <see langword="null" /> counts as 0.
    /// </summary>
    public Dimension? VerticalOffset { get; init; }
    /// <summary>
    /// Initial value of <see cref="Dialog.DialogHostType" />: whether the dialog is shown in the overlay layer of a
    /// window or in a window of its own. <see cref="DialogHostType.Window" /> falls back to <see
    /// cref="DialogHostType.Overlay" /> on platforms without native window support.
    /// </summary>
    public DialogHostType DialogHostType { get; init; } = DialogHostType.Overlay;
    /// <summary>
    /// Initial value of <see cref="Dialog.StandardButtons" />: the standard buttons shown in the footer of the dialog,
    /// as a flag combination. <see cref="DialogStandardButton.NoButton" /> shows none of them.
    /// </summary>
    public DialogStandardButtons StandardButtons { get; init; } = DialogStandardButton.NoButton;
    /// <summary>
    /// Initial value of <see cref="Dialog.DefaultStandardButton" />: the standard button that the Enter key triggers.
    /// It takes effect only when <see cref="StandardButtons" /> includes it.
    /// </summary>
    public DialogStandardButton DefaultStandardButton { get; init; } = DialogStandardButton.Ok;
    /// <summary>
    /// Initial value of <see cref="Dialog.HorizontalStartupLocation" />: the horizontal anchor of the dialog within the
    /// owner area when it is shown. <see cref="DialogHorizontalAnchor.Custom" /> places it by
    /// <see cref="HorizontalOffset" />.
    /// </summary>
    public DialogHorizontalAnchor HorizontalStartupLocation { get; init; } = DialogHorizontalAnchor.Center;
    /// <summary>
    /// Initial value of <see cref="Dialog.VerticalStartupLocation" />: the vertical anchor of the dialog within the
    /// owner area when it is shown. <see cref="DialogVerticalAnchor.Custom" /> places it by <see cref="VerticalOffset"
    /// />.
    /// </summary>
    public DialogVerticalAnchor VerticalStartupLocation { get; init; } = DialogVerticalAnchor.Center;
    /// <summary>
    /// Initial value of <see cref="Dialog.BeforeCloseAsync" />: a callback that is awaited before the dialog closes and
    /// decides whether the close proceeds. It receives a <see cref="DialogClosingContext" /> that describes the
    /// pending close and returns <see langword="true" /> to close the dialog or <see langword="false" /> to keep it
    /// open. It is not called when the close is forced or was canceled by a <see cref="Dialog.Closing" /> handler;
    /// <see langword="null" /> closes without asking.
    /// </summary>
    public Func<DialogClosingContext, ValueTask<bool>>? BeforeCloseAsync { get; init; }
}
