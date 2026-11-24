using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="WindowToken"/>.</summary>
[ControlTokenRule]
internal sealed class WindowTokenRule : IControlTokenRule<PrimeNGToken, WindowToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<WindowToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.DefaultBackground        = global.ColorBgContainer;
        token.DefaultForeground        = global.ColorText;
        token.CornerRadius             = new CornerRadius(12);
        token.SystemBarColor           = new SolidColorBrush(global.ColorBgContainer);
        // 窗口装饰语义不属于密度算法作用域：紧凑算法会把 ControlHeightLG / SizeLG 拉小，
        // 连带 TitleBarHeight 一起缩，因此使用跨平台稳定值切开 compact 链路。
        token.TitleBarHeight               = 40;
        // 取值对应 Win10 DWM 阴影纹理在窗口顶边的半透明合成：白底上激活态约 #707070、
        // 非激活态约 #AAAAAA，因此边框本身使用带透明度的深灰而不是固定实色。
        token.FrameBorderActive        = Color.FromArgb(0xA8, 0x26, 0x26, 0x26);
        token.FrameBorderInactive      = Color.FromArgb(0x64, 0x26, 0x26, 0x26);
        token.FrameShadows             = global.BoxShadowsSecondary;
        token.FullscreenPopoverShadows = new BoxShadows(
            new BoxShadow
            {
                OffsetX = 0, OffsetY = 6, Blur = 16, Spread = 0, Color
                    = Color.FromArgb(20, 0, 0, 0)
            },
            [
                new BoxShadow
                {
                    OffsetX = 0, OffsetY = 3, Blur = 6, Spread = -4,
                    Color   = Color.FromArgb(31, 0, 0, 0)
                },
                new BoxShadow
                {
                    OffsetX = 0, OffsetY = 9, Blur = 28, Spread = 8,
                    Color   = Color.FromArgb(13, 0, 0, 0)
                },
            ]);
    }
}
