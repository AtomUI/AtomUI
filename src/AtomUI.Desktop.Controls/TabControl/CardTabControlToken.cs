using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of <see cref="CardTabControl" />. Every token but <see cref="TabAndContentGutter" /> is
/// inherited from <see cref="AbstractCardTabToken" />, which holds the tokens of the card tabs and the add-tab
/// button.
/// </summary>
[ControlDesignToken]
public sealed partial class CardTabControlToken : AbstractCardTabToken
{
    /// <summary>
    /// Gap between the card tab strip and the selected tab's content.
    /// </summary>
    public partial double TabAndContentGutter { get; set; }
}
