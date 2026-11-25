using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class RangeDateViewerToken : AbstractDateViewerToken
{
    /// <summary>Horizontal spacing between the date panels and between their header sections.</summary>
    public partial double PanelSpacing { get; set; }
}
