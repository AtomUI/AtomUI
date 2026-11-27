using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractPopupCornerToken" /> layer.
/// </summary>
public static class PopupCornerTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn popup corner radius to <paramref name="token" />, the shared
    /// <see cref="AbstractPopupCornerToken" /> layer.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractPopupCornerToken token)
    {
        token.PopupCornerRadius = global.BorderRadiusLG;
    }
}
