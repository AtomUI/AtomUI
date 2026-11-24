using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ListViewToken"/>.</summary>
[ControlTokenRule]
internal sealed class ListViewTokenRule : IControlTokenRule<AntDesignToken, ListViewToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ListViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ListViewTokenCalculator.Calculate(in context, global, token);
        token.GroupHeaderPadding = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingXS);
        token.SelectedIndicatorMargin = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
        token.ItemSelectedIndicatorColor = global.ColorPrimary;
        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXS);
    }
}
