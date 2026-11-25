using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CheckBoxToken : AbstractControlDesignToken
{
    
    /// <summary>Width and height of the checkbox indicator.</summary>
    public partial double CheckIndicatorSize { get; set; }
    /// <summary>Size of the check mark inside a checked checkbox indicator.</summary>
    public partial double CheckedMarkSize { get; set; }

    /// <summary>Size of the mark inside an indeterminate checkbox indicator.</summary>
    public partial double IndicatorTristateMarkSize { get; set; }
    
    /// <summary>Margin around the content displayed beside the checkbox indicator.</summary>
    public partial Thickness TextMargin { get; set; }

    /// <summary>The border of an unchecked (or indeterminate) indicator under the pointer.</summary>
    public partial Color IndicatorHoverBorderColor { get; set; }
    
}
