using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class DateViewerToken : AbstractDateViewerToken
{
    /// <summary>Thickness of the separator above the date viewer footer.</summary>
    public partial Thickness FooterBorderThickness { get; set; }
}
