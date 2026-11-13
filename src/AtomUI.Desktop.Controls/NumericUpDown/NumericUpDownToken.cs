using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class NumericUpDownToken : AbstractButtonSpinnerToken
{
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        // Explicit slots belong to the terminal identity; shared derivation runs after the base colour is resolved.
        if (!IsExplicit(NumericUpDownTokenKind.HandleBg))
        {
            HandleBg = EffectiveGlobalToken.ColorBgContainer;
        }
        FilledHandleBg = CalculateFilledHandleBackground(HandleBg);
    }
}
