using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>Shared appearance defaults for the Shadcn button owners.</summary>
internal abstract class ShadcnButtonFamilyTokens<TToken> : IControlTokenAlgorithm<TToken>
    where TToken : AbstractButtonGeometryToken
{
    private static readonly ButtonThemeRecipe s_flat = new(OutlineOnlyDefaultShadow: false);
    private static readonly ButtonThemeRecipe s_vega = new(OutlineOnlyDefaultShadow: true);
    private static readonly ButtonThemeRecipe s_sera = new(OutlineOnlyDefaultShadow: false, UnderlineLinkAtRest: true);

    public virtual void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var style = ShadcnStyles.Resolve(context);
        controlGlobal.BorderRadius      = new CornerRadius(style.ButtonRadius);
        controlGlobal.BorderRadiusLG    = new CornerRadius(style.ButtonRadiusLG);
        controlGlobal.BorderRadiusSM    = new CornerRadius(style.ButtonRadiusSM);
        controlGlobal.ColorFocusBorder  = controlGlobal.ColorControlOutline;
        controlGlobal.LineWidthFocus    = controlGlobal.ControlOutlineWidth;
        controlGlobal.EnableWaveSpirit = false;
        ShadcnButtonAppearance.ApplyGlobal(in context, controlGlobal);
    }

    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TToken token)
    {
        var style  = ShadcnStyles.Resolve(context);
        var recipe = style == ShadcnStyles.Vega ? s_vega : style == ShadcnStyles.Sera ? s_sera : s_flat;
        token.AppearanceSettings = recipe;
        token.DefaultShadow = recipe.OutlineOnlyDefaultShadow ? ShadcnStyles.Shadow("xs") : default;
        token.PrimaryShadow = default;
        token.DangerShadow  = default;
        ShadcnButtonAppearance.ApplyOwn(in context, effectiveGlobal, token);
        if (token is AbstractStandardButtonToken standard)
        {
            standard.FontWeight = style.ButtonWeight;
        }
    }
}

[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea.Dark", 1)]
internal sealed class ShadcnButtonTokens : ShadcnButtonFamilyTokens<ButtonToken>
{
    /// <inheritdoc />
    public override void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        base.EvaluateGlobal(in context, controlGlobal);

        if (context.Appearance == ThemeAppearance.Dark)
        {
            // An outlined or dashed danger button uses the hover border tone for its text as well.
            controlGlobal.ColorErrorBorderHover = controlGlobal.ColorErrorHover;
            controlGlobal.ColorErrorActive = ShadcnBaseColors.CompositeOver(
                controlGlobal.ColorError, 0.90, controlGlobal.ColorBgContainer);
        }
    }
}

[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea.Dark", 1)]
internal sealed class ShadcnDropdownButtonTokens : ShadcnButtonFamilyTokens<DropdownButtonToken>;

[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea.Dark", 1)]
internal sealed class ShadcnSubmitButtonTokens : ShadcnButtonFamilyTokens<SubmitButtonToken>;

[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea.Dark", 1)]
internal sealed class ShadcnResetButtonTokens : ShadcnButtonFamilyTokens<ResetButtonToken>;

[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea.Dark", 1)]
internal sealed class ShadcnSplitButtonTokens : IControlTokenAlgorithm<SplitButtonToken>
{
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var style = ShadcnStyles.Resolve(context);
        controlGlobal.BorderRadius = new CornerRadius(style.ButtonRadius);
        controlGlobal.BorderRadiusLG = new CornerRadius(style.ButtonRadiusLG);
        controlGlobal.BorderRadiusSM = new CornerRadius(style.ButtonRadiusSM);
        controlGlobal.EnableWaveSpirit = false;
    }

    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SplitButtonToken token)
    {
        // Reserve a layout slot for the inset separator.
        token.AppearanceSettings = true;
        // The upstream split example joins secondary buttons. On a primary fill, tint the seam
        // with the paired foreground so it stays understated against either polarity of the fill.
        token.SolidSeparatorColor = ShadcnStyles.WithAlpha(effectiveGlobal.ColorTextLightSolid, 0x2E);
        token.DangerSeparatorColor = ShadcnStyles.WithAlpha(effectiveGlobal.ColorError, 0x38);
    }
}
