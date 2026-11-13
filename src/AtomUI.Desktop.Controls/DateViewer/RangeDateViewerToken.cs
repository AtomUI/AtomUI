using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class RangeDateViewerToken : AbstractDateViewerToken
{
    public partial double PanelSpacing { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
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
