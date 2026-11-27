using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="Mentions" /> control for its candidate popup.
/// </summary>
[ControlDesignToken]
public sealed partial class MentionsToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Space around the candidate list inside the suggestion popup.
    /// </summary>
    public partial Thickness PopupContentPadding { get; set; }
    
    /// <summary>
    /// The height of one candidate row in the suggestion popup.
    /// No template reads this token; the themes take the minimum row height from <c>ControlHeight</c> instead.
    /// </summary>
    public partial double OptionHeight { get; set; }
    
    /// <summary>
    /// Text color of a candidate row in the suggestion popup while the row is not selected, also while the
    /// pointer is over the row.
    /// </summary>
    public partial Color OptionColor { get; set; }

    /// <summary>
    /// Text color of the candidate row that is selected in the suggestion popup; pressing Enter inserts that row.
    /// </summary>
    public partial Color OptionSelectedColor { get; set; }

    /// <summary>
    /// Minimum width of the mention suggestion popup.
    /// </summary>
    public partial double MinPopupWidth { get; set; }
    
}
