using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="ResultToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ResultTokenRule : IControlTokenRule<ShadcnToken, ResultToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ResultToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: shadcn has no result, so the language has no values of its own.
        token.HeaderFontSize    = global.FontSizeHeading3;
        token.SubHeaderFontSize = global.FontSize;
        token.StatusIconSize    = global.FontSizeHeading3 * 3;
        token.ExtraMargin       = new Thickness(0, global.UniformlyMargin, 0, 0);

        token.ImageWidth  = 250;
        token.ImageHeight = 295;

        token.ResultInfoIconColor    = global.ColorInfo;
        token.ResultSuccessIconColor = global.ColorSuccess;
        token.ResultWarningIconColor = global.ColorWarning;
        token.ResultErrorIconColor   = global.ColorError;

        token.ContentPadding    = new Thickness(global.UniformlyPadding * 2.5, global.UniformlyPaddingLG);
        token.ContentMargin     = new Thickness(0, global.UniformlyPaddingLG, 0, 0);
        token.StatusImageMargin = new Thickness(0, 0, 0, global.UniformlyMargin);
        token.HeaderMargin      = new Thickness(0, global.UniformlyMarginXS);
        token.FramePadding      = new Thickness(global.UniformlyPaddingLG * 2, global.UniformlyMarginXL);
    }
}
