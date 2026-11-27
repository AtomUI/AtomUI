using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="RangeDateViewer" />: the tokens of <see cref="AbstractDateViewerToken" /> and
/// the spacing between its panels.
/// </summary>
[ControlDesignToken]
public sealed partial class RangeDateViewerToken : AbstractDateViewerToken
{
    /// <summary>
    /// Horizontal spacing between the date panels and between their header sections.
    /// </summary>
    public partial double PanelSpacing { get; set; }
}
