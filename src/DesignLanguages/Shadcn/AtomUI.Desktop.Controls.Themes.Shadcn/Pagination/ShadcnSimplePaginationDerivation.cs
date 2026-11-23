using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="SimplePaginationToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnSimplePaginationDerivation : IControlDerivation<ShadcnToken, SimplePaginationToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<SimplePaginationToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // Inset the frame by the focus ring width while keeping the text's horizontal padding.
        var paginationPadding = global.InputPaddingSM;
        var paginationOutline = global.ControlOutlineWidth;
        global.InputPaddingSM = new Thickness(paginationPadding.Left, Math.Max(0, paginationPadding.Top - paginationOutline),
            paginationPadding.Right, Math.Max(0, paginationPadding.Bottom - paginationOutline));
        ShadcnAbstractPaginationDerivation.DeriveShared(in context, global, token);
        token.ItemInputBg = global.ColorBgContainer;
    }
}
