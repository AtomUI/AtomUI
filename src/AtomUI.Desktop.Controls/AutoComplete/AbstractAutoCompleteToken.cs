using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens that every AutoComplete variant shares: the metrics and text colors of the suggestion options
/// in the popup, and the width limits and padding of the popup.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractAutoCompleteToken : AbstractControlDesignToken
{

    /// <summary>
    /// Padding inside the suggestion popup.
    /// </summary>
    public partial Thickness PopupContentPadding { get; set; }

    /// <summary>
    /// Minimum height of one suggestion option row in the popup list.
    /// </summary>
    public partial double OptionHeight { get; set; }

    /// <summary>
    /// Outer margin of a candidate row (a gap between rows when non-zero).
    /// </summary>
    public partial Thickness OptionMargin { get; set; }

    /// <summary>
    /// Corner radius of a candidate row's background.
    /// </summary>
    public partial CornerRadius OptionBorderRadius { get; set; }

    /// <summary>
    /// Text color of a suggestion option that is not selected.
    /// </summary>
    public partial Color OptionColor { get; set; }

    /// <summary>
    /// Text color of the selected suggestion option.
    /// </summary>
    public partial Color OptionSelectedColor { get; set; }

    /// <summary>
    /// Minimum width of the suggestion popup.
    /// </summary>
    public partial double MinPopupWidth { get; set; }

    /// <summary>
    /// Maximum width of the suggestion popup when its width does not follow the anchor.
    /// </summary>
    public partial double MaxPopupWidth { get; set; }

}
