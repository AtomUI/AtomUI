using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractDatePickerToken : AbstractTimeViewToken
{
    /// <summary>Space around the picker panel header.</summary>
    public partial Thickness HeaderMargin { get; set; }
    /// <summary>Padding around the content of the date picker panel.</summary>
    public partial Thickness PanelContentPadding { get; set; }
    /// <summary>Space around the action button group in the picker panel.</summary>
    public partial Thickness ButtonsPanelMargin { get; set; }
}
