using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class QRCodeToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// QRCode 文字颜色
    /// Text color of QRCode
    /// </summary>
    public Color QRCodeTextColor { get; set; }

    /// <summary>
    /// QRCode 遮罩背景颜色
    /// Mask background color of QRCode
    /// </summary>
    public Color QRCodeMaskBackgroundColor { get; set; }

    [ControlTokenDefault(nameof(QRCodeTextColor))]
    private static Color DefaultForQRCodeTextColor(in ControlTokenDefaultContext context) => context.Global.ColorText;

    [ControlTokenDefault(nameof(QRCodeMaskBackgroundColor))]
    private static Color DefaultForQRCodeMaskBackgroundColor(in ControlTokenDefaultContext context)
    {
        var colorBgContainer = context.Global.ColorBgContainer;
        return Color.FromArgb(244, colorBgContainer.R, colorBgContainer.G, colorBgContainer.B);
    }
    
}
