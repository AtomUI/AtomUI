using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class ShowCaseItemToken : AbstractControlDesignToken
{

    public partial Thickness CardPadding { get; set; }
    public partial CornerRadius CardCornerRadius { get; set; }
    public partial BoxShadows CardShadow { get; set; }
    public partial Thickness PreviewMargin { get; set; }
    public partial Thickness BadgePreviewMargin { get; set; }
    public partial Thickness DescriptionMargin { get; set; }
    public partial FontWeight TitleFontWeight { get; set; }
    public partial double DeferredPlaceholderHeight { get; set; }
    public partial CornerRadius DeferredPlaceholderCornerRadius { get; set; }

    public ShowCaseItemToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        CardPadding       = new Thickness(EffectiveGlobalToken.SizeUnit * 5);
        CardCornerRadius  = EffectiveGlobalToken.BorderRadiusLG;
        CardShadow        = EffectiveGlobalToken.BoxShadowsTertiary;
        PreviewMargin     = new Thickness(0, 0, 0, EffectiveGlobalToken.SizeUnit * 8);
        BadgePreviewMargin = new Thickness(0, EffectiveGlobalToken.SizeUnit * 4, 0, EffectiveGlobalToken.SizeUnit * 8);
        DescriptionMargin = new Thickness(0, EffectiveGlobalToken.SizeUnit * 2 + 2, 0, 0);
        TitleFontWeight   = EffectiveGlobalToken.FontWeightStrong;
        DeferredPlaceholderHeight       = EffectiveGlobalToken.SizeUnit * 40;
        DeferredPlaceholderCornerRadius = EffectiveGlobalToken.BorderRadius;
    }

}
