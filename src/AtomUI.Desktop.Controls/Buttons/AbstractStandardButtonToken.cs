using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens that a standard button has in addition to the shared variant and geometry layers: the font
/// weight of its label and the border color of a disabled default button. <c>ButtonToken</c>,
/// <c>ResetButtonToken</c> and <c>SubmitButtonToken</c> inherit them; each design language fills them in its
/// <c>StandardButtonTokenCalculator</c>.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractStandardButtonToken : AbstractButtonGeometryToken
{
    /// <summary>
    /// Font weight of the button label.
    /// </summary>
    public partial FontWeight FontWeight { get; set; }
    /// <summary>
    /// Border color of a disabled button with the default color when its disabled style uses color tokens.
    /// </summary>
    public partial Color DefaultBorderColorDisabled { get; set; }
}
