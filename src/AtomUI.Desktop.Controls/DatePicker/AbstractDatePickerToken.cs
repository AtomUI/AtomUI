using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens shared by <see cref="DatePickerToken" /> and <see cref="RangeDatePickerToken" />:
/// the spacing of the popup footer and of the time view shown beside the calendar.
/// Each design language fills the values in its <c>DatePickerTokenCalculator</c>.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractDatePickerToken : AbstractTimeViewToken
{
    /// <summary>
    /// Space around the header text of the time view shown beside the calendar.
    /// It has no visible effect while the time view is hidden.
    /// </summary>
    public partial Thickness HeaderMargin { get; set; }
    /// <summary>
    /// Padding inside the time view shown beside the calendar.
    /// It has no visible effect while the time view is hidden.
    /// </summary>
    public partial Thickness PanelContentPadding { get; set; }
    /// <summary>
    /// Space around the action button group in the picker panel.
    /// </summary>
    public partial Thickness ButtonsPanelMargin { get; set; }
}
