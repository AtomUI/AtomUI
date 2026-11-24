using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TagToken"/>.</summary>
[ControlTokenRule]
internal sealed class TagTokenRule : IControlTokenRule<AntDesignToken, TagToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<TagToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TagTokenCalculator.Calculate(in context, global, token);
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
