namespace AtomUI.Desktop.Controls;

/// <summary>
/// The horizontal anchor that <see cref="Dialog.HorizontalStartupLocation" /> uses to place a dialog within the
/// owner area when the dialog is shown. <c>OffsetX</c> is added to the position of every anchor.
/// </summary>
[Flags]
public enum DialogHorizontalAnchor
{
    /// <summary>
    /// Aligns the left edge of the dialog with the left edge of the owner area.
    /// </summary>
    Left,
    /// <summary>
    /// Centers the dialog horizontally in the owner area. A dialog that is wider than the owner area starts at the
    /// left edge of the owner area.
    /// </summary>
    Center,
    /// <summary>
    /// Aligns the right edge of the dialog with the right edge of the owner area. A dialog that is wider than the
    /// owner area starts at the left edge of the owner area.
    /// </summary>
    Right,
    /// <summary>
    /// Takes the distance from the left edge of the owner area from <see cref="Dialog.HorizontalOffset" />, which
    /// is resolved against the width of the owner area and counts as 0 when it is <see langword="null" />.
    /// </summary>
    Custom
}

/// <summary>
/// The vertical anchor that <see cref="Dialog.VerticalStartupLocation" /> uses to place a dialog within the
/// owner area when the dialog is shown. <c>OffsetY</c> is added to the position of every anchor.
/// </summary>
[Flags]
public enum DialogVerticalAnchor
{
    /// <summary>
    /// Aligns the top edge of the dialog with the top edge of the owner area.
    /// </summary>
    Top,
    /// <summary>
    /// Centers the dialog vertically in the owner area. A dialog that is taller than the owner area starts at the
    /// top edge of the owner area.
    /// </summary>
    Center,
    /// <summary>
    /// Aligns the bottom edge of the dialog with the bottom edge of the owner area. A dialog that is taller than
    /// the owner area starts at the top edge of the owner area.
    /// </summary>
    Bottom,
    /// <summary>
    /// Takes the distance from the top edge of the owner area from <see cref="Dialog.VerticalOffset" />, which is
    /// resolved against the height of the owner area and counts as 0 when it is <see langword="null" />.
    /// </summary>
    Custom
}
