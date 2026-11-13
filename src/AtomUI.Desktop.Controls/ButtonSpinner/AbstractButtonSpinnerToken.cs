using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractButtonSpinnerToken : AbstractControlDesignToken
{

/// <summary>
    /// 输入框宽度
    /// </summary>

    /// <summary>
    /// 操作按钮宽度
    /// </summary>
    public partial double HandleWidth { get; set; }

    /// <summary>
    /// 操作按钮图标大小
    /// </summary>
    public partial double HandleIconSize { get; set; }

    /// <summary>
    /// 操作按钮背景色
    /// </summary>
    public partial Color HandleBg { get; set; }

    /// <summary>
    /// 操作按钮激活背景色
    /// </summary>
    public partial Color HandleActiveBg { get; set; }

    /// <summary>
    /// 操作按钮悬浮颜色
    /// </summary>
    public partial Color HandleHoverColor { get; set; }

    /// <summary>
    /// 操作按钮边框颜色
    /// </summary>
    public partial Color HandleBorderColor { get; set; }

    /// <summary>
    /// 面性变体操作按钮背景色
    /// </summary>
    public partial Color FilledHandleBg { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        HandleWidth    = EffectiveGlobalToken.ControlHeightSM;
        HandleIconSize = EffectiveGlobalToken.FontSize / 2;
        HandleActiveBg = EffectiveGlobalToken.ColorFillAlter;

        HandleHoverColor  = EffectiveGlobalToken.ColorPrimary;
        HandleBorderColor = EffectiveGlobalToken.ColorBorder;
    }

    protected Color CalculateFilledHandleBackground(Color handleBg)
    {
        return ColorUtils.OnBackground(EffectiveGlobalToken.ColorFillSecondary, handleBg);
    }
}
