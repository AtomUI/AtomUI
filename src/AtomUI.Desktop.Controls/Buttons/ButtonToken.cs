using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ButtonToken : AbstractStandardButtonToken
{

    /// <summary>Stroke separating adjacent buttons in a button group.</summary>
    [ReservedToken("Compact-space primary buttons do not draw a group separator yet.")]
    public partial Color GroupBorderColor { get; set; }
}
