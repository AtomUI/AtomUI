using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

public class FlyoutPresenter : ArrowDecoratedBox,
                               IControlSharedTokenResourcesHost
{
    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FlyoutHostToken.ID;

    #endregion
    
    // 我们在这里并没有增加任何元素或者样式
    protected override Type StyleKeyOverride => typeof(ArrowDecoratedBox);

    public FlyoutPresenter()
    {
        this.RegisterResources();
        SetValue(CursorProperty, new Cursor(StandardCursorType.Arrow), BindingPriority.Template);
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            var host = this.FindLogicalAncestorOfType<Popup>();
            if (host != null)
            {
                host.IsOpen = false;
                e.Handled   = true;
            }
        }

        base.OnKeyDown(e);
    }
}