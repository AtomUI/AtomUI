using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractTimePickerToken"/> layer.</summary>
public static class AntDesignAbstractTimePickerDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractTimePickerToken token)
    {
        AntDesignAbstractTimeViewDerivation.DeriveShared(in context, global, token);
        token.ButtonsMargin = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
