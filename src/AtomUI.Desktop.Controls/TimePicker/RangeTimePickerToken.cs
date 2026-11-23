using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class RangeTimePickerToken : AbstractTimePickerToken
{
    /// <summary>Space around the arrow between the two range values.</summary>
    public partial Thickness RangePickerArrowMargin { get; set; }
    /// <summary>Thickness of the indicator that marks the active range field.</summary>
    public partial double RangePickerIndicatorThickness { get; set; }
    /// <summary>Gap between the time-range popup and its input anchor.</summary>
    public partial double RangeMarginToAnchor { get; set; }
}
