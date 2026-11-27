using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="ButtonSpinner" /> control: the spinner button tokens of
/// <see cref="AbstractButtonSpinnerToken" /> plus the reserved <see cref="ControlWidth" />.
/// </summary>
[ControlDesignToken]
public sealed partial class ButtonSpinnerToken : AbstractButtonSpinnerToken
{
    /// <summary>
    /// Reserved default width of the button spinner. It is not applied: the control takes the width of its
    /// content or of its host layout.
    /// </summary>
    [ReservedToken("No default control width is applied; the input sizes to content or host layout.")]
    public partial double ControlWidth { get; set; }
}
