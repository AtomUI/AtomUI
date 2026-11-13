using AtomUI.Theme.DesignTokens;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class IconInfoItemToken : AbstractControlDesignToken
{
    public partial double IconNameFontSize { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        IconNameFontSize = 13;
    }
}
