using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTimeViewToken : AbstractControlDesignToken
{
    /// <summary>Height of an item in a time selection column.</summary>
    public partial double ItemHeight { get; set; }
    /// <summary>Width of an item in a time selection column.</summary>
    public partial double ItemWidth { get; set; }
    /// <summary>Width of the container for the AM or PM selection column.</summary>
    public partial double PeriodHostWidth { get; set; }
    /// <summary>Padding inside an item in a time selection column.</summary>
    public partial Thickness ItemPadding { get; set; }
    /// <summary>Margin around an item in a time selection column.</summary>
    public partial Thickness ItemMargin { get; set; }

    /// <summary>
    /// Whether the picker popup draws its pointer arrow toward the input (true by default).
    /// </summary>
    public partial bool PopupArrowVisible { get; set; }
}
