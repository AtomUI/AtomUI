using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class QRCodeToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// QRCode 文字颜色
    /// Text color of QRCode
    /// </summary>
    public partial Color QRCodeTextColor { get; set; }

    /// <summary>
    /// QRCode 遮罩背景颜色
    /// Mask background color of QRCode
    /// </summary>
    public partial Color QRCodeMaskBackgroundColor { get; set; }

    public QRCodeToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var colorBgContainer = EffectiveGlobalToken.ColorBgContainer;
        QRCodeTextColor           = EffectiveGlobalToken.ColorText;
        QRCodeMaskBackgroundColor = Color.FromArgb(244, colorBgContainer.R, colorBgContainer.G, colorBgContainer.B);
    }
    
}
