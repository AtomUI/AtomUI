using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="CardToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class CardTokenRule : IControlTokenRule<PrimeNGToken, CardToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<CardToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (ColorBgContainer read by the formulas below and by the Card theme for
        // Background; BorderRadiusLG, ColorBorderSecondary read by the Card theme only (CornerRadius, BorderBrush)).
        PrimeNGCardTokens.EvaluateGlobal(in context, global);
        // Ant Design formula kept as is: the card stage (PrimeNGCardTokens) replaces the body padding, the resting
        // shadow and the dark header background; the header metrics keep the Ant Design formula.
        token.HeaderBg         = Colors.Transparent;
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
        // antd cardShadow: the shadow of a hoverable card. PrimeUIX card.shadow is the resting shadow, which the
        // stage writes over RestingShadows below, so CardShadows stays the hover shadow.
        token.CardShadows         = new BoxShadows(new BoxShadow()
            {
                OffsetX = 0,
                OffsetY = 1,
                Blur    = 2,
                Spread  = -1,
                Color   = ColorUtils.FromRgbF(0.16, 0, 0, 0)
            }, [
                new BoxShadow
                {
                    OffsetX = 0,
                    OffsetY = 3,
                    Blur    = 6,
                    Spread  = 0,
                    Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
                },
                new BoxShadow
                {
                    OffsetX = 0,
                    OffsetY = 5,
                    Blur    = 12,
                    Spread  = 4,
                    Color   = ColorUtils.FromRgbF(0.09, 0, 0, 0)
                }
            ]);
        // Ant Design gives an outlined card no resting shadow, so the hover shadow is kept as transparent layers.
        // Under PrimeNG the stage replaces RestingShadows with the PrimeUIX card.shadow (EvaluateOwn below).
        token.RestingShadows = Transparent(token.CardShadows);
        token.CardGridHoverShadows = token.CardShadows;

        token.CardHeadPadding     = global.Padding;
        token.CardPaddingBase     = global.PaddingLG;
        token.CardActionsIconSize = global.FontSize;
        // Ant Design's grid items carry no resting shadow.
        token.CardGridItemShadows = new BoxShadows();
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGCardTokens.EvaluateOwn(in context, global, token);
    }


    /// <summary>
    /// The same shadow layers with a transparent color: the resting start shape of the hover transition.
    /// </summary>
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
