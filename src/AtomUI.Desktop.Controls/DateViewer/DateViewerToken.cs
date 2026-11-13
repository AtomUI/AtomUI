using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class DateViewerToken : AbstractDateViewerToken
{
    public partial Thickness FooterBorderThickness { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
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
