using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTimePickerToken : AbstractTimeViewToken
{
    public partial Thickness ButtonsMargin { get; set; }

    /// <summary>
    /// Insets the time option background within its interactive row.
    /// </summary>
    public partial Thickness ItemBackgroundInset { get; set; }

    /// <summary>
    /// Centers the text within each time option's content area.
    /// </summary>
    public partial bool CenterItemContent { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ButtonsMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
    }
}
