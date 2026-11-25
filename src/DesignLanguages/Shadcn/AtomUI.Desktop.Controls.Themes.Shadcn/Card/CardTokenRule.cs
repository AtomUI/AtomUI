using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CardToken"/>.</summary>
[ControlTokenRule]
internal sealed class CardTokenRule : IControlTokenRule<ShadcnToken, CardToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<CardToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // The --card surface: in dark it is a step lighter than the page.
        global.ColorBgContainer = context.Language.Card;
        token.HeaderBg        = Colors.Transparent;
        token.HeaderFontSizeLG = global.FontSizeLG;
        token.HeaderFontSize   = global.FontSize;
        token.HeaderFontSizeSM = global.FontSize;

        token.HeaderHeightLG   = global.FontHeightLG + global.UniformlyPadding * 2;
        token.HeaderHeight     = global.FontHeight + global.UniformlyPaddingXS * 2;
        token.HeaderHeightSM   = global.FontHeightSM;
        token.ActionsBg        = global.ColorBgContainer;
        token.ActionsSpacing   = global.SpacingSM;
        token.TabsMarginBottom = new Thickness(0, 0, 0, -global.UniformlyPadding - global.LineWidth);
        token.ExtraColor       = global.ColorText;

        token.BodyPaddingLG = global.PaddingLG;
        token.BodyPadding   = global.PaddingSM;
        token.BodyPaddingSM = global.PaddingXS;

        token.HeaderPaddingLG  = new Thickness(global.UniformlyPaddingLG, 0);
        token.HeaderPadding    = new Thickness(global.UniformlyPaddingSM, 0);
        token.HeaderPaddingSM  = new Thickness(global.UniformlyPaddingXS, 0);
        // The style's card ring (the foreground at the card ring strength) ahead of its card shadow level.
        var language = context.Language;
        token.CardShadows = ShadcnRecipes.RingThenShadow(
            ShadcnRecipes.WithPercent(language.Foreground, language.CardRingAlpha), language.CardShadow);
        token.RestingShadows = Transparent(token.CardShadows);
        token.CardGridHoverShadows = language.CardShadow;

        token.CardHeadPadding     = global.Padding;
        token.CardPaddingBase     = global.PaddingLG;
        token.CardActionsIconSize = global.FontSize;
        // Ant Design's grid items carry no resting shadow.
        token.CardGridItemShadows = new BoxShadows();
        token.BodyPadding = new Thickness(language.CardPadding);
    }


    /// <summary>The same shadow layers with a transparent colour: the resting start shape of the hover transition.</summary>
    private static BoxShadows Transparent(BoxShadows shadows)
    {
        if (shadows.Count == 0)
        {
            return shadows;
        }

        static BoxShadow Clear(BoxShadow shadow) => shadow with { Color = Colors.Transparent };
        var rest = new BoxShadow[shadows.Count - 1];
        for (var index = 1; index < shadows.Count; index++)
        {
            rest[index - 1] = Clear(shadows[index]);
        }

        return new BoxShadows(Clear(shadows[0]), rest);
    }
}
