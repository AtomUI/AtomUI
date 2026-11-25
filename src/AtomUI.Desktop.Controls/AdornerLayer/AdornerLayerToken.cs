using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class AdornerLayerToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Gap between a focused element and its keyboard focus outline (Ant Design outline-offset).
    /// </summary>
    public partial double FocusOutlineOffset { get; set; }
    
}
