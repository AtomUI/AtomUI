using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="DescriptionsToken"/>.</summary>
[ControlTokenRule]
internal sealed class DescriptionsTokenRule : IControlTokenRule<ShadcnToken, DescriptionsToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<DescriptionsToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.LabelBg      = global.ColorFillAlter;
        token.LabelColor   = global.ColorTextTertiary;
        token.TitleColor   = global.ColorText;
        token.HeaderMargin = new Thickness(0, 0, 0, global.FontSizeSM * global.RelativeLineHeightSM);
        token.ItemPaddingLG  = new Thickness(global.UniformlyPaddingLG, global.UniformlyPadding);
        token.ItemPadding  = new Thickness(global.UniformlyPaddingLG, global.UniformlyPaddingSM);
        token.ItemPaddingSM  = new Thickness(global.UniformlyPadding, global.UniformlyPaddingXS);
        token.ColonMargin  = new Thickness(global.UniformlyMarginXXS / 2, 0, global.UniformlyMarginXS, 0);
        token.ContentColor = global.ColorText;
        token.ExtraColor   = global.ColorText;
        if (context.Variant.IsDark)
        {
            token.LabelColor = global.ColorTextDescription;
        }
    }
}
