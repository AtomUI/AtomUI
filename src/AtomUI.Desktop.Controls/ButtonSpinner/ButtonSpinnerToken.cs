using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ButtonSpinnerToken : AbstractButtonSpinnerToken
{
    /// <summary>Reserved default width for the button spinner.</summary>
    [ReservedToken("No default control width is applied; the input sizes to content or host layout.")]
    public partial double ControlWidth { get; set; }
}
