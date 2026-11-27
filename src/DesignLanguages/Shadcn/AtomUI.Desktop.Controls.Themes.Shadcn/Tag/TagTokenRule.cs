using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="TagToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TagTokenRule : IControlTokenRule<ShadcnToken, TagToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TagToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TagTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: no registry value is merged into the tag tokens.
        token.TagCloseIconSize = global.IconSizeXS;
        token.DefaultBg        = ColorUtils.OnBackground(global.ColorFillTertiary, global.ColorBgContainer);
        token.DefaultColor     = global.ColorText;
        token.FilledStatusSolid = false;
        token.SuccessColor      = global.ColorSuccess;
        token.InfoColor         = global.ColorInfo;
        token.WarningColor      = global.ColorWarning;
        token.ErrorColor        = global.ColorError;
        token.SolidTextColor   = ColorUtils.IsBright(global.ColorBgSolid, Colors.White)
            ? Colors.Black
            : Colors.White;
    }
}
