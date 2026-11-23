using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="StepsProgressBarToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignStepsProgressBarDerivation : IControlDerivation<AntDesignToken, StepsProgressBarToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<StepsProgressBarToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractLineProgressDerivation.DeriveShared(in context, global, token);
        token.ProgressStepMarginInlineEnd = global.UniformlyMarginXXS / 2;

        token.ProgressStepMinWidth = token.ProgressStepMarginInlineEnd;
    }
}
