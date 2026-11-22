using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Dialog under the PrimeNG presets: the PrimeUIX <c>overlay.modal.border.radius</c> (<c>border.radius.xl</c>), the
/// <c>overlay.modal.padding</c> (Aura and Nora 18, Lara 20) and the <c>dialog.title.font.size</c> (18) of the standard
/// <c>@primeuix/themes</c> 3.1.1 presets. The title weight 600 is the global <c>FontWeightStrong</c> already.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGDialogTokens : IControlTokenAlgorithm<DialogToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, mid: PrimeNGMotion.Dialog, slow: PrimeNGMotion.Dialog);
        PrimeNGModalStage.ApplyGlobal(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DialogToken token)
    {
        var p = PrimeNGPresets.Resolve(context).Controls.DialogPadding;
        token.HeaderPadding        = new Thickness(p);
        token.ContentPadding       = new Thickness(p, 0, p, p);
        token.WindowContentPadding = new Thickness(p);
        token.FooterPadding        = new Thickness(p, 0, p, p);
        token.HeaderMarginBottom   = new Thickness(0);
        token.FooterMarginTop      = new Thickness(0);
        token.ButtonGroupSpacing   = PrimeNGModalStage.FooterGap(context);
        token.CloseBtnSize         = PrimeNGModalStage.CloseButtonSize(context);
        token.HeaderFontSize       = PrimeNGPresets.Resolve(context).Controls.DialogHeaderFontSize;
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
    internal static void ApplyGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var preset  = PrimeNGPresets.Resolve(context);
        var rules   = preset.For(context.Appearance);
        controlGlobal.BorderRadiusLG = PrimeNGOverlayRadius.Of(controlGlobal, preset.Sizes);
        controlGlobal.BorderRadiusSM = new CornerRadius(CloseButtonSize(context) / 2);
        controlGlobal.BoxShadows = PrimeNGPopupTokens.WithOutlineRing(controlGlobal.BoxShadows,
            rules.OverlayBorder.Resolve(rules.Surface.AsSpan()), controlGlobal.LineWidth);
        if (PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Nora)
        {
            controlGlobal.FontWeightStrong = Avalonia.Media.FontWeight.Bold;
        }
    }

    internal static double FooterGap(in ThemeAlgorithmContext context) =>
        PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara ? 8 : 6;

    internal static double CloseButtonSize(in ThemeAlgorithmContext context) =>
        PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara ? 38 : 36;
}

/// <summary>
/// MessageBox (PrimeUIX <c>confirmdialog</c> on <c>dialog</c>) under the PrimeNG presets: the dialog surface and
/// rhythm of <see cref="PrimeNGModalStage"/>, the dialog title size and <c>icon.size</c> 1.5rem (Lara 1.75rem).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGMessageBoxTokens : IControlTokenAlgorithm<MessageBoxToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, mid: PrimeNGMotion.Dialog, slow: PrimeNGMotion.Dialog);
        PrimeNGModalStage.ApplyGlobal(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, MessageBoxToken token)
    {
        var controls = PrimeNGPresets.Resolve(context).Controls;
        var p = controls.DialogPadding;
        token.HeaderPadding        = new Thickness(p);
        token.ContentPadding       = new Thickness(p, 0, p, p);
        token.WindowContentPadding = new Thickness(p);
        token.FooterPadding        = new Thickness(p, 0, p, p);
        token.HeaderMarginBottom   = new Thickness(0);
        token.FooterMarginTop      = new Thickness(0);
        token.ButtonGroupSpacing   = PrimeNGModalStage.FooterGap(context);
        token.CloseBtnSize         = PrimeNGModalStage.CloseButtonSize(context);
        token.HeaderFontSize       = controls.DialogHeaderFontSize;
        token.StyleIconSize        = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara ? 28 : 24;
    }
}

/// <summary>
/// Drawer under the PrimeNG presets, PrimeUIX 3.1.1 <c>drawer</c>: <c>header.padding</c> p, <c>content.padding</c>
/// 0 p p, <c>footer.padding</c> p (p = 1.125rem, Lara 1.25rem), <c>title.font.size</c> 1.125rem (Lara 1.25rem), the
/// non-directional modal shadow and a 1px <c>border.color</c> edge (overlay.modal.border.color).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGDrawerTokens : IControlTokenAlgorithm<DrawerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, slow: PrimeNGMotion.Drawer);
        controlGlobal.FontSizeLG = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara ? 20 : 18;
        if (PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Nora)
        {
            controlGlobal.FontWeightStrong = Avalonia.Media.FontWeight.Bold;
        }
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DrawerToken token)
    {
        var preset = PrimeNGPresets.Resolve(context);
        var rules  = preset.For(context.Appearance);
        var p      = preset.Controls.DialogPadding;
        token.HeaderMargin        = new Thickness(p);
        token.ContentPadding      = new Thickness(p, 0, p, p);
        token.FooterPadding       = new Thickness(p);
        token.BoxShadowDrawerLeft  = effectiveGlobal.BoxShadows;
        token.BoxShadowDrawerRight = effectiveGlobal.BoxShadows;
        token.BoxShadowDrawerUp    = effectiveGlobal.BoxShadows;
        token.BoxShadowDrawerDown  = effectiveGlobal.BoxShadows;
        token.EdgeBorderColor      = rules.OverlayBorder.Resolve(rules.Surface.AsSpan());
        token.EdgeBorderThickness  = effectiveGlobal.LineWidth;
    }
}
