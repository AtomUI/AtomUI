using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CardTabControlToken : AbstractCardTabToken
{
    /// <summary>Gap between the card tab strip and the selected tab's content.</summary>
    public partial double TabAndContentGutter { get; set; }
}
