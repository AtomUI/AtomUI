using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractStandardButtonToken : AbstractButtonGeometryToken
{
    /// <summary>Font weight of the button label.</summary>
    public partial FontWeight FontWeight { get; set; }
    /// <summary>Border color of a disabled button with the default color when its disabled style uses color tokens.</summary>
    public partial Color DefaultBorderColorDisabled { get; set; }
}
