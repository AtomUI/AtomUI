using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="ListViewToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ListViewTokenRule : IControlTokenRule<ShadcnToken, ListViewToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ListViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ListViewTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: the list item recipe is merged by ListViewTokenCalculator above.
        token.GroupHeaderPadding = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingXS);
        token.SelectedIndicatorMargin = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
        token.ItemSelectedIndicatorColor = global.ColorPrimary;
        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXS);
    }
}
