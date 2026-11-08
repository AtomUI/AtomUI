using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens shared by every progress control (antd Progress <c>defaultColor</c> / <c>remainingColor</c>). Each
/// progress control has its own terminal Token, so <c>Controls[CircleProgress]</c> and <c>Controls[ProgressBar]</c>
/// compute and override these values independently.
/// </summary>
[ControlDesignToken]
internal abstract class AbstractProgressToken : AbstractControlDesignToken
{
    /// <summary>
    /// 进度条默认颜色
    /// Default color of the progress indicator
    /// </summary>
    public Color DefaultColor { get; set; }

    /// <summary>
    /// 进度条剩余部分颜色
    /// Color of the remaining (unfilled) part of the progress bar
    /// </summary>
    public Color RemainingColor { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        DefaultColor   = EffectiveGlobalToken.ColorInfo;
        RemainingColor = EffectiveGlobalToken.ColorFillSecondary;
    }
}
