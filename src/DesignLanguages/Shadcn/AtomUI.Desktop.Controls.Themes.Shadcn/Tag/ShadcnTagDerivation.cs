using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TagToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTagDerivation : IControlDerivation<ShadcnToken, TagToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TagToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractTagDerivation.DeriveShared(in context, global, token);
        token.TagCloseIconSize = global.IconSizeXS;
        token.DefaultBg        = ColorUtils.OnBackground(global.ColorFillTertiary, global.ColorBgContainer);
        token.DefaultColor     = global.ColorText;
        token.FilledStatusSolid = false;
        token.SuccessColor      = global.ColorSuccess;
        token.InfoColor         = global.ColorInfo;
        token.WarningColor      = global.ColorWarning;
        token.ErrorColor        = global.ColorError;
        token.SolidTextColor   = ColorUtils.IsBright(global.ColorBgSolid, Colors.White)
            ? Colors.Black
            : Colors.White;
    }
}
