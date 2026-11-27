using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="SimplePaginationToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SimplePaginationTokenRule : IControlTokenRule<AntDesignToken, SimplePaginationToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<SimplePaginationToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        PaginationTokenCalculator.Calculate(in context, global, token);
        token.ItemInputBg = global.ColorBgContainer;
    }
}
