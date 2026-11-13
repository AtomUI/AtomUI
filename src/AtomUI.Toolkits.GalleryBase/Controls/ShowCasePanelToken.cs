using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class ShowCasePanelToken : AbstractControlDesignToken
{

    public partial Thickness ContentMargin { get; set; }
    public partial double MinItemWidth { get; set; }
    public partial int MaxColumns { get; set; }
    public partial double ColumnGap { get; set; }
    public partial double RowGap { get; set; }

    public ShowCasePanelToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ContentMargin = new Thickness(EffectiveGlobalToken.SizeUnit * 6);
        MinItemWidth  = EffectiveGlobalToken.SizeUnit * 90;
        MaxColumns    = 2;
        ColumnGap     = EffectiveGlobalToken.SizeUnit * 4;
        RowGap        = EffectiveGlobalToken.SizeUnit * 4;
    }

}
