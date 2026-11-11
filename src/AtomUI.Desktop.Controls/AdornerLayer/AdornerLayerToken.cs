using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class AdornerLayerToken : AbstractControlDesignToken
{

    public AdornerLayerToken()

    {
    }
    
    /// <summary>
    /// Gap between a focused element and its keyboard focus outline (Ant Design outline-offset).
    /// </summary>
    public double FocusOutlineOffset { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        FocusOutlineOffset = EffectiveGlobalToken.LineWidth;
    }
    
}
