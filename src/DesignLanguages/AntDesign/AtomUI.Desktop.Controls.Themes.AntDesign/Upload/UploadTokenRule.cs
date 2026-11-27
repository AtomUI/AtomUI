using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="UploadToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class UploadTokenRule : IControlTokenRule<AntDesignToken, UploadToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<UploadToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
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
