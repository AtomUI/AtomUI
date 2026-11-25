using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="SimplePaginationToken"/>.</summary>
[ControlTokenRule]
internal sealed class SimplePaginationTokenRule : IControlTokenRule<ShadcnToken, SimplePaginationToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<SimplePaginationToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Inset the frame by the focus ring width while keeping the text's horizontal padding.
        var paginationPadding = global.InputPaddingSM;
        var paginationOutline = global.ControlOutlineWidth;
        global.InputPaddingSM = new Thickness(paginationPadding.Left, Math.Max(0, paginationPadding.Top - paginationOutline),
            paginationPadding.Right, Math.Max(0, paginationPadding.Bottom - paginationOutline));
        PaginationTokenCalculator.Calculate(in context, global, token);
        token.ItemInputBg = global.ColorBgContainer;
    }
}
