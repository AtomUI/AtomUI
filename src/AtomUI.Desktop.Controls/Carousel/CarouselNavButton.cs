using Avalonia.Controls;
using AtomUI.Theme.Resources;
namespace AtomUI.Desktop.Controls;

public class CarouselNavButton : IconButton, IControlThemeGlobalOwnerSource
{
    #region 实现 IControlThemeGlobalOwnerSource 接口

    Control? IControlThemeGlobalOwnerSource.ThemeGlobalOwner => CarouselThemeOwner.Resolve(this);

    #endregion
}