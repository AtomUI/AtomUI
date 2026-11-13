using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ButtonSpinnerToken : AbstractButtonSpinnerToken
{
    [ReservedToken("No default control width is applied; the input sizes to content or host layout.")]
    public partial double ControlWidth { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ControlWidth = 90;
        // Explicit slots belong to the terminal identity; shared derivation runs after the base colour is resolved.
        if (!IsExplicit(ButtonSpinnerTokenKind.HandleBg))
        {
            HandleBg = EffectiveGlobalToken.ColorBgContainer;
        }
        FilledHandleBg = CalculateFilledHandleBackground(HandleBg);
    }
}
