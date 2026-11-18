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
    /// 操作按钮悬浮背景色
    /// </summary>
    public partial Color HandleHoverBg { get; set; }

    /// <summary>
    /// 操作按钮图标静息颜色
    /// </summary>
    public partial Color HandleIconColor { get; set; }

    /// <summary>
    /// 操作按钮按下颜色
    /// </summary>
    public partial Color HandlePressedColor { get; set; }

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
        // Ant Design has no hover fill on the handle buttons (only the icon changes) and the resting icon is the
        // shared icon colour; both slots exist so a design language can give the handle its own surface.
        HandleHoverBg     = Colors.Transparent;
        HandleIconColor   = EffectiveGlobalToken.ColorIcon;
        HandlePressedColor = EffectiveGlobalToken.ColorPrimaryActive;
        HandleBorderColor = EffectiveGlobalToken.ColorBorder;
    }

    protected Color CalculateFilledHandleBackground(Color handleBg)
    {
        return ColorUtils.OnBackground(EffectiveGlobalToken.ColorFillSecondary, handleBg);
    }
}
