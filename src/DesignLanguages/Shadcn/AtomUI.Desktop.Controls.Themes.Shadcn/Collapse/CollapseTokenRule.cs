using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="CollapseToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class CollapseTokenRule : IControlTokenRule<ShadcnToken, CollapseToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<CollapseToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.HeaderPadding             = new Thickness(global.UniformlyPadding, global.UniformlyPaddingSM);
        token.HeaderBg                  = global.ColorFillAlter;
        token.HeaderColor               = global.ColorTextHeading;
        token.HeaderActiveColor         = global.ColorTextHeading;
        token.HeaderFontWeight          = FontWeight.Normal;
        // Ant Design formula kept as is: no registry value is merged into the collapse tokens.
        token.ContentPadding            = new Thickness(16, global.UniformlyPadding);
        token.CollapseContentPaddingSM  = global.PaddingSM;
        token.CollapseContentPaddingLG  = global.PaddingLG;
        token.ContentBg                 = global.ColorBgContainer;
        token.CollapseHeaderPaddingSM   = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingXS);
        token.CollapseHeaderPaddingLG   = new Thickness(global.UniformlyPaddingLG, global.UniformlyPadding);
        token.CollapsePanelBorderRadius = global.BorderRadiusLG;
        token.LeftExpandButtonMarginSM  = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        token.LeftExpandButtonMargin    = new Thickness(0, 0, global.UniformlyMarginXS, 0);
        token.LeftExpandButtonMarginLG  = new Thickness(0, 0, global.UniformlyMarginSM, 0);
        token.RightExpandButtonMarginSM = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
        token.RightExpandButtonMargin   = new Thickness(global.UniformlyMarginXS, 0, 0, 0);
        token.RightExpandButtonMarginLG   = new Thickness(global.UniformlyMarginSM, 0, 0, 0);
    }
}
