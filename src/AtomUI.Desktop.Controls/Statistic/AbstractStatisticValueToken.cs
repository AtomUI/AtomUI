using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractStatisticValueToken : AbstractControlDesignToken
{
    /// <summary>Font size of the displayed statistic value.</summary>
    public partial double ContentFontSize { get; set; }
}
