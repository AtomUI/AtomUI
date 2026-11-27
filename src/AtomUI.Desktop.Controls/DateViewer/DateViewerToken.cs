using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="DateViewer" />: the tokens of <see cref="AbstractDateViewerToken" /> and the
/// thickness of the separator above the footer.
/// </summary>
[ControlDesignToken]
public sealed partial class DateViewerToken : AbstractDateViewerToken
{
    /// <summary>
    /// Thickness of the separator above the date viewer footer.
    /// </summary>
    public partial Thickness FooterBorderThickness { get; set; }
}
