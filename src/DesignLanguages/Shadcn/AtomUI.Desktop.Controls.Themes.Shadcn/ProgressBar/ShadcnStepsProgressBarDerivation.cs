using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="StepsProgressBarToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnStepsProgressBarDerivation : IControlDerivation<ShadcnToken, StepsProgressBarToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<StepsProgressBarToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractLineProgressDerivation.DeriveShared(in context, global, token);
        token.ProgressStepMarginInlineEnd = global.UniformlyMarginXXS / 2;

        token.ProgressStepMinWidth = token.ProgressStepMarginInlineEnd;
    }
}
