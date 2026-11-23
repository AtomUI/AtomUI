using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TagToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTagDerivation : IControlDerivation<AntDesignToken, TagToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TagToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractTagDerivation.DeriveShared(in context, global, token);
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
