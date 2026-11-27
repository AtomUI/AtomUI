using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.TokenRules;

/// <summary>
/// The Generic view of <see cref="GalleryWindowTitleBarToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class GenericGalleryWindowTitleBarTokenRule : IControlTokenRule<LanguageDesignToken, GalleryWindowTitleBarToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<GalleryWindowTitleBarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.MenuFontWeight = FontWeight.Normal;
        token.MenuMargin     = new Thickness(0, 0, global.SizeUnit * 2, 0);
    }
}
