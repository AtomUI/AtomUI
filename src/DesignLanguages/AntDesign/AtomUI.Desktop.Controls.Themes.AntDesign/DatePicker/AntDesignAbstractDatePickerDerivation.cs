using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractDatePickerToken"/> layer.</summary>
public static class AntDesignAbstractDatePickerDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractDatePickerToken token)
    {
        AntDesignAbstractTimeViewDerivation.DeriveShared(in context, global, token);
        token.HeaderMargin = new Thickness(0, 0, 0, 3);
        token.PanelContentPadding = global.PaddingSM;
        token.ButtonsPanelMargin = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
