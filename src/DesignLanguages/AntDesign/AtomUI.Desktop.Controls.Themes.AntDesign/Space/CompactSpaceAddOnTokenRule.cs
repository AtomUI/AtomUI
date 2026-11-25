using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="CompactSpaceAddOnToken"/>.</summary>
[ControlTokenRule]
internal sealed class CompactSpaceAddOnTokenRule : IControlTokenRule<AntDesignToken, CompactSpaceAddOnToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<CompactSpaceAddOnToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.AddonBg       = global.ColorFillAlter;

        token.AddOnPadding   = new Thickness(global.UniformlyPaddingSM, 0);
        token.AddOnPaddingSM = new Thickness(global.ControlPaddingHorizontalSM, 0);
        token.AddOnPaddingLG = new Thickness(global.ControlPaddingHorizontal, 0);
    }
}
