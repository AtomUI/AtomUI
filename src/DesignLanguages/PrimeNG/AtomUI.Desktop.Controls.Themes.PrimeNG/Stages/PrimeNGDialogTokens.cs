using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// Dialog under the PrimeNG presets: the PrimeUIX <c>overlay.modal.border.radius</c> (<c>border.radius.xl</c>), the
/// <c>overlay.modal.padding</c> (Aura and Nora 18, Lara 20) and the <c>dialog.title.font.size</c> (18) of the standard
/// <c>@primeuix/themes</c> 3.1.1 presets. The title weight 600 is the global <c>FontWeightStrong</c> already.
/// </summary>
internal static class PrimeNGDialogTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.MotionDurationMid = PrimeNGMotion.Dialog;
        controlGlobal.MotionDurationSlow = PrimeNGMotion.Dialog;
        PrimeNGModalStage.ApplyGlobal(context, controlGlobal);
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, DialogToken token)
    {
        // overlay.modal.padding p: header.padding p, content.padding 0 p p and footer.padding 0 p p, so no extra gaps.
        var p = context.Language.GetDialogOverlayRecipe(effectiveGlobal, context.Variant).Padding.Left;
        token.HeaderPadding        = new Thickness(p);
        token.ContentPadding       = new Thickness(p, 0, p, p);
        token.WindowContentPadding = new Thickness(p);
        token.FooterPadding        = new Thickness(p, 0, p, p);
        token.HeaderMarginBottom   = new Thickness(0);
        token.FooterMarginTop      = new Thickness(0);
        token.ButtonGroupSpacing   = PrimeNGModalStage.FooterGap(context);
        token.CloseBtnSize         = PrimeNGModalStage.CloseButtonSize(context);
        token.HeaderFontSize       = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant).DialogTitleFontSize;
    }
}

/// <summary>
/// The modal surfaces (Dialog, MessageBox) under the PrimeNG presets, PrimeUIX 3.1.1 <c>dialog</c>: the
/// <c>border.radius.xl</c> corners, a 1px <c>border.color</c> (overlay.modal.border.color) ring ahead of the modal
/// shadow, <c>header.padding</c> then <c>content.padding</c> and <c>footer.padding</c> 0 p p (no extra gaps),
/// <c>footer.gap</c> 6 (Lara 8), the close button as a rounded text button (<c>button.icon.only.width</c>
/// 2.25rem, Lara 2.375rem) and the Nora title weight 700.
/// </summary>
internal static class PrimeNGModalStage
{
    internal static void ApplyGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        var dialog = context.Language.GetDialogOverlayRecipe(controlGlobal, context.Variant);
        // overlay.modal.border.radius (border.radius.xl); the small radius rounds the close button.
        controlGlobal.BorderRadiusLG = dialog.CornerRadius;
        controlGlobal.BorderRadiusSM = new CornerRadius(CloseButtonSize(context) / 2);
        // A 1px overlay.modal.border.color ring ahead of the modal shadow.
        controlGlobal.BoxShadows = dialog.ShadowsWithOutline;
        // Nora dialog.title.font.weight 700.
        if (context.Variant.Style == PrimeNGLanguage.Nora)
        {
            controlGlobal.FontWeightStrong = Avalonia.Media.FontWeight.Bold;
        }
    }

    // dialog.footer.gap 6 (Lara 8).
    internal static double FooterGap(in TokenEvaluationContext<PrimeNGToken> context) =>
        context.Variant.Style == PrimeNGLanguage.Lara ? 8 : 6;

    // button.icon.only.width 2.25rem (Lara 2.375rem).
    internal static double CloseButtonSize(in TokenEvaluationContext<PrimeNGToken> context) =>
        context.Variant.Style == PrimeNGLanguage.Lara ? 38 : 36;
}

/// <summary>
/// MessageBox (PrimeUIX <c>confirmdialog</c> on <c>dialog</c>) under the PrimeNG presets: the dialog surface and
/// rhythm of <see cref="PrimeNGModalStage" />, the dialog title size and <c>icon.size</c> 1.5rem (Lara 1.75rem).
/// </summary>
internal static class PrimeNGMessageBoxTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.MotionDurationMid = PrimeNGMotion.Dialog;
        controlGlobal.MotionDurationSlow = PrimeNGMotion.Dialog;
        PrimeNGModalStage.ApplyGlobal(context, controlGlobal);
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, MessageBoxToken token)
    {
        // The dialog rhythm: overlay.modal.padding p on the header, content and footer (0 p p) with no extra gaps.
        var p = context.Language.GetDialogOverlayRecipe(effectiveGlobal, context.Variant).Padding.Left;
        token.HeaderPadding        = new Thickness(p);
        token.ContentPadding       = new Thickness(p, 0, p, p);
        token.WindowContentPadding = new Thickness(p);
        token.FooterPadding        = new Thickness(p, 0, p, p);
        token.HeaderMarginBottom   = new Thickness(0);
        token.FooterMarginTop      = new Thickness(0);
        token.ButtonGroupSpacing   = PrimeNGModalStage.FooterGap(context);
        token.CloseBtnSize         = PrimeNGModalStage.CloseButtonSize(context);
        token.HeaderFontSize       = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant).DialogTitleFontSize;
        // confirmdialog.icon.size 1.5rem (Lara 1.75rem).
        token.StyleIconSize        = context.Variant.Style == PrimeNGLanguage.Lara ? 28 : 24;
    }
}

/// <summary>
/// Drawer under the PrimeNG presets, PrimeUIX 3.1.1 <c>drawer</c>: <c>header.padding</c> p, <c>content.padding</c>
/// 0 p p, <c>footer.padding</c> p (p = 1.125rem, Lara 1.25rem), <c>title.font.size</c> 1.125rem (Lara 1.25rem), the
/// Nora title weight 700, the non-directional modal shadow and a 1px <c>border.color</c> edge
/// (overlay.modal.border.color).
/// </summary>
internal static class PrimeNGDrawerTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.MotionDurationSlow = PrimeNGMotion.Drawer;
        // drawer.title.font.size 1.125rem (Lara 1.25rem).
        controlGlobal.FontSizeLG = context.Variant.Style == PrimeNGLanguage.Lara ? 20 : 18;
        // Nora drawer.title.font.weight 700.
        if (context.Variant.Style == PrimeNGLanguage.Nora)
        {
            controlGlobal.FontWeightStrong = Avalonia.Media.FontWeight.Bold;
        }
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, DrawerToken token)
    {
        // drawer paddings p / 0 p p / p; the modal shadow on every side and a 1px border.color edge.
        var dialog = context.Language.GetDialogOverlayRecipe(effectiveGlobal, context.Variant);
        var p      = dialog.Padding.Left;
        token.HeaderMargin        = new Thickness(p);
        token.ContentPadding      = new Thickness(p, 0, p, p);
        token.FooterPadding       = new Thickness(p);
        token.BoxShadowDrawerLeft  = effectiveGlobal.BoxShadows;
        token.BoxShadowDrawerRight = effectiveGlobal.BoxShadows;
        token.BoxShadowDrawerUp    = effectiveGlobal.BoxShadows;
        token.BoxShadowDrawerDown  = effectiveGlobal.BoxShadows;
        token.EdgeBorderColor      = dialog.BorderColor;
        token.EdgeBorderThickness  = effectiveGlobal.LineWidth;
    }
}
