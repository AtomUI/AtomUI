using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="WindowTitleBarToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnWindowTitleBarDerivation : IControlDerivation<ShadcnToken, WindowTitleBarToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<WindowTitleBarToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractTitleBarActionDerivation.DeriveShared(in context, global, token);
        token.LogoAndTitleSpacing         = global.SizeUnit * 2;

        token.LogoAndLeftAddOnSpacing     = global.SpacingXXS;
        token.TitleBarPadding             = new Thickness(token.LogoAndTitleSpacing * 1.8, 0);
        token.CaptionButtonIconSize       = global.IconSize;

        token.WindowsCaptionIconSize      = 11;
        token.LogoSize                    = global.SizeUnit * 4;
        token.CaptionButtonPadding = new Thickness(global.SizeUnit * 2);

        token.CaptionGroupSpacing  = global.SizeUnit * 2;

        // managed caption button 按自身实测尺寸取 max(w, h) / 2 得到正圆圆角；
        // 内容尺寸固定为 icon + padding，这里用同一公式把该几何暴露给主题，
        // 使 macOS AddOn 按钮与相邻 caption 按钮共用同一圆角来源。


        // 窗口装饰语义不走密度算法：紧凑模式下 ControlHeightLG / SizeLG / SpacingXS 都会被拉小，
        // 标题栏 / DockPanel 间距不应跟着缩，因此使用跨平台稳定值。
        token.Height                  = 40;
        token.HeaderHorizontalSpacing = 8;
        token.WindowsCloseButtonHoverBgColor   = Color.FromRgb(244, 67, 54);

        token.WindowsCloseButtonPressedBgColor = Color.FromArgb(190, 244, 67, 54);
        token.WindowsCloseButtonHoverColor     = ColorUtils.OnBackground(Colors.White, token.WindowsCloseButtonHoverBgColor);

        token.TitleFontSize = 13;
        token.TitleFontWeight = FontWeight.Bold;
    }
}
