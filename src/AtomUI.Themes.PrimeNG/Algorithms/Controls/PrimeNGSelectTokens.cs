using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Select under the PrimeNG presets: option and panel padding, computed from the control's effective global size
/// tokens by the preset's rule table: PrimeUIX 3.1.1 <c>list.option.padding</c> and <c>list.padding</c> (Aura 10,4 and 4;
/// Lara 14,8 and 0,8; Nora 10,4 and 0,2). PrimeUIX has no compact density; the option padding follows the size ladder.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGSelectTokens : IControlTokenAlgorithm<SelectToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SelectToken token)
    {
        // PrimeUIX form.field.sm.font.size 0.75rem.
        token.InputFontSizeSM = effectiveGlobal.FontSizeSM;
        var controls = PrimeNGPresets.Resolve(context).Controls;
        token.OptionPadding       = controls.SelectOptionPadding(effectiveGlobal);
        token.PopupContentPadding = controls.SelectPopupContentPadding(effectiveGlobal);
        // PrimeUIX .p-select-option has no minimum height: the row is the option padding around the 1.5 line box
        // (Aura and Nora 29, Lara 37); option.selected.font.weight is normal.
        token.OptionHeight             = effectiveGlobal.FontSize * 1.5 + token.OptionPadding.Top + token.OptionPadding.Bottom;
        token.OptionSelectedFontWeight = FontWeight.Normal;
        // The check mark of a selected item takes the selected text colour (PrimeUIX highlight.color): the primary check
        // vanishes on a solid primary highlight (noir, Nora), and on the pale highlight it is the primary 700 text.
        token.OptionSelectedIndicatorColor = token.OptionSelectedColor;
        // chip.background (Aura and Lara surface.100 / 800, Nora surface.200 / 700) = list.option.focus.background.
        token.OptionActiveBg = effectiveGlobal.ControlItemBgHover;
        if (PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Aura)
        {
            // Aura list.option.border.radius {border.radius.sm} (4; the form field stage has moved the scope's
            // BorderRadiusSM to the field radius) and list.gap 2px, measured on primeng.dev. The gap is drawn by
            // insetting each option background 1px top and bottom inside a row 2px taller, and the list padding loses
            // that 1px so the first and last backgrounds stay list.padding (4) from the panel edge. Lara and Nora have
            // square options without a gap.
            token.OptionBorderRadius    = new CornerRadius(4);
            token.OptionBackgroundInset = new Thickness(0, 1);
            token.OptionHeight         += 2;
            token.PopupContentPadding   = new Thickness(token.PopupContentPadding.Left, token.PopupContentPadding.Top - 1,
                token.PopupContentPadding.Right, token.PopupContentPadding.Bottom - 1);
        }
        token.MultipleItemBg = effectiveGlobal.ControlItemBgHover;
    }
}
