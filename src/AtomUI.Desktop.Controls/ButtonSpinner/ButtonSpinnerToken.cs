using AtomUI.Media;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class ButtonSpinnerToken : AbstractControlDesignToken
{
    
    public ButtonSpinnerToken()

    {
    }

    /// <summary>
    /// 输入框宽度
    /// </summary>
    [ReservedToken("antd InputNumber controlWidth. ButtonSpinner and NumericUpDown size to their content or host " +
                   "layout; a themed default width would override every layout, so no default width is applied yet.")]
    public double ControlWidth { get; set; }

    /// <summary>
    /// 操作按钮宽度
    /// </summary>
    public double HandleWidth { get; set; }

    /// <summary>
    /// 操作按钮图标大小
    /// </summary>
    public double HandleIconSize { get; set; }

    /// <summary>
    /// 操作按钮背景色
    /// </summary>
    public Color HandleBg { get; set; }

    /// <summary>
    /// 操作按钮激活背景色
    /// </summary>
    public Color HandleActiveBg { get; set; }

    /// <summary>
    /// 操作按钮悬浮颜色
    /// </summary>
    public Color HandleHoverColor { get; set; }

    /// <summary>
    /// 操作按钮边框颜色
    /// </summary>
    public Color HandleBorderColor { get; set; }

    /// <summary>
    /// 面性变体操作按钮背景色
    /// </summary>
    public Color FilledHandleBg { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ControlWidth   = 90;
        HandleWidth    = EffectiveGlobalToken.ControlHeightSM;
        HandleIconSize = EffectiveGlobalToken.FontSize / 2;
        HandleActiveBg = EffectiveGlobalToken.ColorFillAlter;

        if (!IsExplicit(ButtonSpinnerTokenKind.HandleBg))
        {
            HandleBg       = EffectiveGlobalToken.ColorBgContainer;
        }

        FilledHandleBg = ColorUtils.OnBackground(EffectiveGlobalToken.ColorFillSecondary,
            HandleBg);
        HandleHoverColor  = EffectiveGlobalToken.ColorPrimary;
        HandleBorderColor = EffectiveGlobalToken.ColorBorder;
    }
    
}
