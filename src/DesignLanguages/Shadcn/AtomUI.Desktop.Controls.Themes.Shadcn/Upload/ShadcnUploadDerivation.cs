using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="UploadToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnUploadDerivation : IControlDerivation<ShadcnToken, UploadToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<UploadToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.ActionsColor             = global.ColorIcon;
        token.PictureCardSize          = global.ControlHeightLG * 2.55;
        token.TextListItemMargin       = new Thickness(0, global.UniformlyMarginXS, 0, 0);
        token.TextListNamePadding = new Thickness(global.UniformlyPaddingXS, 0);
        token.TextListItemPadding = new Thickness(global.UniformlyPaddingXS, global.UniformlyPaddingXXS);

        token.TextListProgressLineHeight = 2;
        token.TextListProgressPadding = new Thickness(
                token.TextListItemPadding.Left + global.FontSize + token.TextListNamePadding.Left,
                0, token.TextListItemPadding.Right, 0);

        token.UploadThumbnailSize      = global.FontSizeHeading2;
        token.DragIconSize             = global.FontSizeHeading3 * 2;
        token.DragIconMargin           = new Thickness(0, 0, 0, global.UniformlyMargin);
        token.DragHeaderMargin         = new Thickness(0, 0, 0, global.UniformlyMarginXXS);
        var sideInset = global.LineWidth;
        token.PictureListItemMargin = new Thickness(sideInset, global.UniformlyMarginXS, sideInset, 0);

        token.PictureListPreviewerSize = global.SizeXXL;
    }
}
