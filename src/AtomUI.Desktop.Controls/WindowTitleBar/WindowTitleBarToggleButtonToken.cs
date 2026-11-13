using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class WindowTitleBarToggleButtonToken : AbstractTitleBarActionToken
{
    public partial CornerRadius CaptionButtonCornerRadius { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(WindowTitleBarToggleButtonTokenKind.CaptionButtonIconSize))
        {
            CaptionButtonIconSize = EffectiveGlobalToken.IconSize;
        }
        if (!IsExplicit(WindowTitleBarToggleButtonTokenKind.CaptionButtonPadding))
        {
            CaptionButtonPadding = new Thickness(EffectiveGlobalToken.SizeUnit * 2);
        }
        CaptionButtonCornerRadius = CalculateCaptionCornerRadius(CaptionButtonIconSize, CaptionButtonPadding);
    }
}
