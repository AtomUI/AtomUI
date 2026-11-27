using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Media;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="ArrowDecoratedBoxToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ArrowDecoratedBoxTokenRule : IControlTokenRule<AntDesignToken, ArrowDecoratedBoxToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ArrowDecoratedBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // antd genRoundedArrow: sizePopupArrow wide, half as high, borderRadiusXS at the tip and borderRadiusOuter
        // where the sides meet the box.
        token.ArrowSize            = global.SizePopupArrow;
        token.ArrowTipRadius       = global.BorderRadiusXS.TopLeft;
        token.ArrowBaseRadius      = global.BorderRadiusOuter.TopLeft;
        token.ContentPadding       = global.PaddingXS;
        token.ArrowStrokeColor     = ColorUtils.FromRgbF(0.07, 0, 0, 0);
        token.ArrowStrokeThickness = 1;
        token.ContentBorderColor     = global.ColorBorderSecondary;
        token.ContentBorderThickness = default;
        token.ContentMinHeight       = global.ControlHeight;
    }
}
