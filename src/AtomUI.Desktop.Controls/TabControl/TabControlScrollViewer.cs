using AtomUI.Data;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

internal class TabControlScrollViewer : BaseTabScrollViewer
{
    #region 内部属性定义

    internal BaseTabControl? TabControl { get; set; }

    #endregion

    protected override Type StyleKeyOverride => typeof(BaseTabScrollViewer);
    
    private IDisposable? _flyoutBindingDisposable;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (MenuIndicator is not null)
        {
            MenuIndicator.Click += HandleMenuIndicatorClicked;
        }
    }

    private void HandleMenuIndicatorClicked(object? sender, RoutedEventArgs args)
    {
        if (MenuFlyout != null)
        {
            return;
        }   
        MenuFlyout = new MenuFlyout
        {
            IsShowArrow              = false,
            ClickHideFlyoutPredicate = ClickHideFlyoutPredicate,
            ShouldUseOverlayLayer = true
        };
        MenuFlyout.Closed += (o, eventArgs) =>
        {
            MenuFlyout = null;
        };
        _flyoutBindingDisposable?.Dispose();
        _flyoutBindingDisposable = BindUtils.RelayBind(this, IsMotionEnabledProperty, MenuFlyout, MenuFlyout.IsMotionEnabledProperty);
 
        if (TabStripPlacement == Dock.Top)
        {
            MenuFlyout.Placement = PlacementMode.BottomEdgeAlignedLeft;
        }
        else if (TabStripPlacement == Dock.Bottom)
        {
            MenuFlyout.Placement = PlacementMode.TopEdgeAlignedLeft;
        }
        else if (TabStripPlacement == Dock.Right)
        {
            MenuFlyout.Placement = PlacementMode.LeftEdgeAlignedBottom;
        }
        else
        {
            MenuFlyout.Placement = PlacementMode.RightEdgeAlignedBottom;
        }

        // 收集没有完全显示的 Tab 列表
        var oldItems = MenuFlyout.Items.ToList();
        foreach (var item in oldItems)
        {
            MenuFlyout.Items.Remove(item);
        }

        if (TabControl is not null)
        {
            for (var i = 0; i < TabControl.ItemCount; i++)
            {
                var itemContainer = TabControl.ContainerFromIndex(i)!;
                if (itemContainer is TabItem tabItem)
                {
                    var needAddToMenu = false;
                    var itemBounds    = itemContainer.Bounds;
                    if (TabStripPlacement == Dock.Top || TabStripPlacement == Dock.Bottom)
                    {
                        var left  = Math.Floor(itemBounds.Left - Offset.X);
                        var right = Math.Floor(itemBounds.Right - Offset.X);
                        if (left < 0 || right > Viewport.Width)
                        {
                            needAddToMenu = true;
                        }
                    }
                    else
                    {
                        var top    = Math.Floor(itemBounds.Top - Offset.Y);
                        var bottom = Math.Floor(itemBounds.Bottom - Offset.Y);

                        if (top < 0 || bottom > Viewport.Height)
                        {
                            needAddToMenu = true;
                        }
                    }

                    if (needAddToMenu)
                    {
                        var menuItem = new TabControlOverflowMenuItem
                        {
                            Header     = tabItem.Header,
                            TabItem    = tabItem,
                            IsClosable = tabItem.IsClosable
                        };
                        menuItem.Click    += HandleMenuItemClicked;
                        menuItem.CloseTab += HandleCloseTabRequest;
                        MenuFlyout.Items.Add(menuItem);
                    }
                }
            }

            if (MenuFlyout.Items.Count > 0)
            {
                MenuFlyout.ShowAt(MenuIndicator!);
            }
        }
    }

    private void HandleMenuItemClicked(object? sender, RoutedEventArgs args)
    {
        if (TabControl is not null)
        {
            Dispatcher.UIThread.Post(sender =>
            {
                if (sender is TabControlOverflowMenuItem tabControlMenuItem)
                {
                    var tabItem = tabControlMenuItem.TabItem;
                    if (tabItem is not null)
                    {
                        tabItem.BringIntoView();
                        TabControl.SelectedItem = tabItem;
                    }
                }
            }, sender);
        }
    }

    private void HandleCloseTabRequest(object? sender, RoutedEventArgs args)
    {
        if (sender is TabControlOverflowMenuItem tabControlMenuItem)
        {
            if (TabControl is not null)
            {
                if (TabControl.SelectedItem is TabItem selectedItem)
                {
                    if (selectedItem == tabControlMenuItem.TabItem)
                    {
                        var     selectedIndex   = TabControl.SelectedIndex;
                        object? newSelectedItem = null;
                        if (selectedIndex != 0)
                        {
                            newSelectedItem = TabControl.Items[--selectedIndex];
                        }

                        TabControl.Items.Remove(tabControlMenuItem.TabItem);
                        TabControl.SelectedItem = newSelectedItem;
                    }
                    else
                    {
                        TabControl.Items.Remove(tabControlMenuItem.TabItem);
                    }
                }
            }
        }
    }
}