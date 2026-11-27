using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="TagToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TagTokenRule : IControlTokenRule<PrimeNGToken, TagToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TagToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides and the status colors, read by the formulas below.
        var statuses = PrimeNGTagTokens.EvaluateGlobal(in context, global);
        TagTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: the PrimeNG tag stage at the end of the method replaces TagPadding,
        // TagLineHeight, TagFontWeight, TagIconSize, FilledStatusSolid, DefaultBg and DefaultColor.
        token.TagCloseIconSize = global.IconSizeXS;
        token.DefaultBg        = ColorUtils.OnBackground(global.ColorFillTertiary, global.ColorBgContainer);
        token.DefaultColor     = global.ColorText;
        token.FilledStatusSolid = false;
        // The status colors are the preset's severity colors: the text on the tint in Aura, the solid fill in Lara
        // and Nora.
        token.SuccessColor      = statuses.Success;
        token.InfoColor         = statuses.Info;
        token.WarningColor      = statuses.Warning;
        token.ErrorColor        = statuses.Error;
        token.SolidTextColor   = ColorUtils.IsBright(global.ColorBgSolid, Colors.White)
            ? Colors.Black
            : Colors.White;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTagTokens.EvaluateOwn(in context, global, token);
    }
}
