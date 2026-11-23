using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractTransferToken"/> layer.</summary>
public static class ShadcnAbstractTransferDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractTransferToken token)
    {

        token.ListWidth    = 180;
        token.ListHeight   = 200;
        token.HeaderHeight = global.ControlHeightLG;
        token.HeaderPadding = new Thickness(
            global.UniformlyPaddingSM,
            Math.Ceiling((global.ControlHeightLG -
                          global.LineWidth -
                          global.FontHeight) / 2));
    }
}
