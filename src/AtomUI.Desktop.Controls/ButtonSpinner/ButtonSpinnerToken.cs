using AtomUI.Media;
using AtomUI.Theme.TokenSystem;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ButtonSpinnerToken : LineEditToken
{
    public new const string ID = "ButtonSpinner";

    public ButtonSpinnerToken()
        : this(ID)
    {
    }

    protected ButtonSpinnerToken(string id)
        : base(id)
    {
    }

    /// <summary>
    /// 输入框宽度
    /// </summary>
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
        HandleWidth    = SharedToken.ControlHeightSM;
        HandleIconSize = SharedToken.FontSize / 2;
        HandleActiveBg = SharedToken.ColorFillAlter;
        HandleBg       = SharedToken.ColorBgContainer;
        FilledHandleBg = ColorUtils.OnBackground(SharedToken.ColorFillSecondary,
            HandleBg);
        HandleHoverColor  = SharedToken.ColorPrimary;
        HandleBorderColor = SharedToken.ColorBorder;
    }
}