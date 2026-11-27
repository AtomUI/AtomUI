using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractPopupCornerToken" /> layer.
/// </summary>
public static class PopupCornerTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the shared <see cref="AbstractPopupCornerToken" /> layer to
    /// <paramref name="token" />. The corner radius of a popup is the large border radius of
    /// <paramref name="global" />.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractPopupCornerToken token)
    {
        token.PopupCornerRadius = global.BorderRadiusLG;
    }
}
