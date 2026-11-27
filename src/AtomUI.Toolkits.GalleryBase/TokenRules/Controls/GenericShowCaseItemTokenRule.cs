using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.TokenRules;

/// <summary>
/// The Generic view of <see cref="ShowCaseItemToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class GenericShowCaseItemTokenRule : IControlTokenRule<LanguageDesignToken, ShowCaseItemToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<ShowCaseItemToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.CardPadding       = new Thickness(global.SizeUnit * 5);
        token.CardCornerRadius  = global.BorderRadiusLG;
        token.CardShadow        = global.BoxShadowsTertiary;
        token.PreviewMargin     = new Thickness(0, 0, 0, global.SizeUnit * 8);
        token.BadgePreviewMargin = new Thickness(0, global.SizeUnit * 4, 0, global.SizeUnit * 8);
        token.DescriptionMargin = new Thickness(0, global.SizeUnit * 2 + 2, 0, 0);
        token.TitleFontWeight   = global.FontWeightStrong;
        token.DeferredPlaceholderHeight       = global.SizeUnit * 40;
        token.DeferredPlaceholderCornerRadius = global.BorderRadius;
    }
}
