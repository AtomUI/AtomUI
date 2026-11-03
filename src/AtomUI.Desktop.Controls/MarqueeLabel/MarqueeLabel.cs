using AtomUI.Controls.Commons;
using AtomUI.Theme.Resources;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public class MarqueeLabel : AbstractMarqueeLabel, IThemeInheritanceSource
{
    #region 实现 IThemeInheritanceSource 接口

    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion
}
