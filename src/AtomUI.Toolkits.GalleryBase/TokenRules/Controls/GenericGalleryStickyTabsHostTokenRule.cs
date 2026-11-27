using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.TokenRules;

/// <summary>
/// The Generic view of <see cref="GalleryStickyTabsHostToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class GenericGalleryStickyTabsHostTokenRule : IControlTokenRule<LanguageDesignToken, GalleryStickyTabsHostToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<GalleryStickyTabsHostToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.StickyContentPadding = new Thickness(global.SizeUnit * 7, 0);
        token.StickyBackground     = new SolidColorBrush(global.ColorBgLayout);
        token.StickyBorderBrush    = new SolidColorBrush(global.ColorBorderSecondary);
    }
}
