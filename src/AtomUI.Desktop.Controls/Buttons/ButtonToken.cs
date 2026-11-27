using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the standard button. It inherits the layers shared with other standard buttons from
/// <see cref="AbstractStandardButtonToken" /> and adds the stroke that separates adjacent buttons in a button
/// group. Each design language fills the values in its own <c>ButtonTokenRule</c>.
/// </summary>
[ControlDesignToken]
public sealed partial class ButtonToken : AbstractStandardButtonToken
{

    /// <summary>
    /// Stroke separating adjacent buttons in a button group.
    /// </summary>
    [ReservedToken("Compact-space primary buttons do not draw a group separator yet.")]
    public partial Color GroupBorderColor { get; set; }
}
