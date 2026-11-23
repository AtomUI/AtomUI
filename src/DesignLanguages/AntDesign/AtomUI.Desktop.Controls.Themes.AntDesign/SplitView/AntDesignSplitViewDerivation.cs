using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia.Animation.Easings;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="SplitViewToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignSplitViewDerivation : IControlDerivation<AntDesignToken, SplitViewToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<SplitViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.OpenPaneThemeLength     = 320;
        token.CompactPaneThemeLength  = 48;
        token.PaneOpenMotionDuration  = TimeSpan.FromMilliseconds(200);
        token.PaneCloseMotionDuration = TimeSpan.FromMilliseconds(100);
        token.PaneMotionEasing        = Easing.Parse("0.1,0.9,0.2,1.0");
    }
}
