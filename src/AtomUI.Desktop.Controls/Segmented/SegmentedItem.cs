using Avalonia;
using AtomUI.Theme.Resources;
using AtomUI.Controls.Commons;

namespace AtomUI.Desktop.Controls;

public class SegmentedItem : AbstractSegmentedItem,
    IThemeInheritanceSource
{
    public SegmentedItem()
    {
    }
    #region 实现 IThemeInheritanceSource 接口

    // The theme preserves the actual inheritance parent when owner typography is not explicit.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion
}
