using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractAutoCompleteToken" /> layer.
/// </summary>
public static class AutoCompleteTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractAutoCompleteToken" /> layer of every auto-complete variant.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractAutoCompleteToken token)
    {
        token.OptionHeight        = global.ControlHeight;
        token.OptionMargin        = default;
        token.OptionBorderRadius  = default;
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        // Ant Design formula kept as is: no registry value is merged into these control tokens.
        token.MinPopupWidth       = 120;
        token.OptionColor         = global.ColorTextSecondary;
        token.OptionSelectedColor = global.ColorText;
        token.MaxPopupWidth       = 200;
    }
}
