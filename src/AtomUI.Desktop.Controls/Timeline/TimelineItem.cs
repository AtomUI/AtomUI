using AtomUI.Controls.Commons;
using AtomUI.Theme.Resources;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public class TimelineItem : AbstractTimelineItem, IThemeInheritanceSource
{
    public TimelineItem()
    {
    }

    #region 实现 IThemeInheritanceSource 接口

    // ControlTheme cannot read the protected inheritance parent. Its Setters own
    // the final typography values; this bridge only exposes their actual source.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion
}
