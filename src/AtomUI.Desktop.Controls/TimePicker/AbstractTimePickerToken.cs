using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTimePickerToken : AbstractTimeViewToken
{
    /// <summary>Space around the action buttons in the time picker panel.</summary>
    public partial Thickness ButtonsMargin { get; set; }

    /// <summary>
    /// Insets the time option background within its interactive row.
    /// </summary>
    public partial Thickness ItemBackgroundInset { get; set; }

    /// <summary>
    /// Centers the text within each time option's content area.
    /// </summary>
    public partial bool CenterItemContent { get; set; }
}
