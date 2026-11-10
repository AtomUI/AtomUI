using AtomUI.Theme.DesignTokens;
using AtomUI.Media;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TabControlToken : AbstractLineTabToken
{
    [ReservedToken("The overflow shadow casters sit outside the viewport; caster thickness has no visible meaning.")]
    public double MenuEdgeThickness { get; set; }

    public double TabAndContentGutter { get; set; }
    [ReservedToken("antd Tabs horizontalMargin: no outer margin is consumed by the current tab layout.")]
    public Thickness HorizontalMargin { get; set; }
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        MenuEdgeThickness = EffectiveGlobalToken.ControlHeight;
        TabAndContentGutter = EffectiveGlobalToken.UniformlyMarginSM;
        HorizontalMargin = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMargin, 0);
    }
}
