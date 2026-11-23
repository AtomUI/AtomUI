using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractTimeViewToken"/> layer.</summary>
public static class AntDesignAbstractTimeViewDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractTimeViewToken token)
    {
        token.ItemMargin        = new Thickness(global.UniformlyMarginXXS, 0);
        token.PopupArrowVisible = true;
    }


    public static Thickness CalculateTimeItemPadding(double itemWidth, double itemHeight) =>
        new((itemWidth - itemHeight) / 2, 0, 0, 0);
}
