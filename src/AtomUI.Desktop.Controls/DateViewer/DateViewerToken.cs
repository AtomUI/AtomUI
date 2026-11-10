using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class DateViewerToken : AbstractDateViewerToken
{
    public Thickness FooterBorderThickness { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(DateViewerTokenKind.CellHeight))
        {
            CellHeight = EffectiveGlobalToken.ControlHeightSM;
        }
        CellRowHeight = CalculateCellRowHeight(CellHeight);
        FooterBorderThickness = new Thickness(0, EffectiveGlobalToken.LineWidth, 0, 0);
    }
}
