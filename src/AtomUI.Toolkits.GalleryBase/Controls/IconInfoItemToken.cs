using AtomUI.Theme.DesignTokens;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed class IconInfoItemToken : AbstractControlDesignToken
{
    public double IconNameFontSize { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        IconNameFontSize = 13;
    }
}
