using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class RangeDateViewerToken : AbstractDateViewerToken
{
    public double PanelSpacing { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(RangeDateViewerTokenKind.CellHeight))
        {
            CellHeight = EffectiveGlobalToken.ControlHeightSM;
        }
        CellRowHeight = CalculateCellRowHeight(CellHeight);
        PanelSpacing = EffectiveGlobalToken.UniformlyMargin;
    }
}
