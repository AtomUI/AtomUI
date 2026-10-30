using Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

public class DialogPopupPrimitiveLayeringTests
{
    static DialogPopupPrimitiveLayeringTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Direct_Popup_In_Dialog_Uses_The_Owning_Window_Popup_Layer()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var anchor = new AtomUI.Desktop.Controls.Button { Content = "Anchor" };
            var action = new AtomUI.Desktop.Controls.Button { Content = "Popup action" };
            var popup = new AtomUI.Desktop.Controls.Popup
            {
                PlacementTarget = anchor,
                Child = action,
                IsLightDismissEnabled = true,
                ShouldUseOverlayLayer = true
            };
            var content = new Avalonia.Controls.Grid { Children = { anchor, popup } };

            using var host = DialogPopupTestHost.Open(content);
            popup.IsOpen = true;

            var popupHost = host.FindPopupHost(popup);
            popupHost.GetVisualParent().ShouldNotBeSameAs(host.DialogLayer.GetVisualParent());
            popupHost.GetVisualDescendants().ShouldContain(action);
            FindOwningPopup(action).SurfaceBackground.ShouldBeNull();

            host.LightDismiss();
            popup.IsOpen.ShouldBeFalse();
            host.AssertNoPopupHosts();
            host.Presenter.Parent.ShouldBeSameAs(host.DialogLayer);
        });
    }

    [Fact]
    public void Flyout_In_Dialog_Uses_The_Owning_Window_Popup_Layer_And_Light_Dismisses()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var anchor = new AtomUI.Desktop.Controls.Button { Content = "Flyout anchor" };
            var flyoutContent = new TextBlock { Text = "Flyout content" };
            var flyout = new AtomUI.Desktop.Controls.Flyout
            {
                Content = flyoutContent,
                IsMotionEnabled = false,
                ShouldUseOverlayPopup = true,
                IsLightDismissEnabled = true
            };

            using var host = DialogPopupTestHost.Open(anchor);
            flyout.ShowAt(anchor);
            DialogPopupTestHost.Pump();

            host.FindPopupHost().ShouldNotBeNull();
            flyout.IsOpen.ShouldBeTrue();
            FindOwningPopup(flyoutContent).SurfaceBackground.ShouldBeNull();

            host.LightDismiss();
            flyout.IsOpen.ShouldBeFalse();
            host.AssertNoPopupHosts();
            host.Presenter.Parent.ShouldBeSameAs(host.DialogLayer);
        });
    }

    [Fact]
    public void MenuFlyout_In_Dialog_Can_Invoke_An_Item()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var anchor = new AtomUI.Desktop.Controls.Button { Content = "Menu anchor" };
            var clicked = false;
            var item = new AtomUI.Desktop.Controls.MenuItem { Header = "Menu action" };
            item.Click += (_, _) => clicked = true;
            var flyout = new AtomUI.Desktop.Controls.MenuFlyout
            {
                IsMotionEnabled = false,
                ShouldUseOverlayPopup = true
            };
            flyout.Items.Add(item);

            using var host = DialogPopupTestHost.Open(anchor);
            flyout.ShowAt(anchor);
            var popupHost = host.FindPopupHost();
            var popupItem = popupHost.GetVisualDescendants()
                                     .OfType<AtomUI.Desktop.Controls.MenuItem>()
                                     .Single(control => control.Header?.ToString() == "Menu action");
            FindOwningPopup(popupItem).SurfaceBackground.ShouldBeNull();

            host.Click(popupItem);
            clicked.ShouldBeTrue();
            flyout.IsOpen.ShouldBeFalse();
            host.AssertNoPopupHosts();
        });
    }

    [Fact]
    public void ToolTip_In_Dialog_Uses_The_Owning_Window_Popup_Layer()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var anchor = new AtomUI.Desktop.Controls.Button { Content = "Tooltip anchor" };
            var toolTip = new AtomUI.Desktop.Controls.ToolTip
            {
                Content = "Tooltip content",
                IsMotionEnabled = false
            };
            AtomUI.Desktop.Controls.ToolTip.SetTip(anchor, toolTip);

            using var host = DialogPopupTestHost.Open(anchor);
            AtomUI.Desktop.Controls.ToolTip.SetIsOpen(anchor, true);

            host.FindPopupHost().ShouldNotBeNull();
            AtomUI.Desktop.Controls.ToolTip.GetIsOpen(anchor).ShouldBeTrue();
            FindOwningPopup(toolTip).SurfaceBackground.ShouldBeNull();

            AtomUI.Desktop.Controls.ToolTip.SetIsOpen(anchor, false);
            DialogPopupTestHost.Pump();
            AtomUI.Desktop.Controls.ToolTip.GetIsOpen(anchor).ShouldBeFalse();
            host.AssertNoPopupHosts();
        });
    }

    [Fact]
    public void ContextMenu_In_Dialog_Uses_The_Owning_Window_Popup_Layer_And_Closes()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var anchor = new AtomUI.Desktop.Controls.Button { Content = "Context anchor" };
            var contextMenu = new AtomUI.Desktop.Controls.ContextMenu
            {
                IsMotionEnabled = false,
                ShouldUseOverlayPopup = true,
                Items = { new AtomUI.Desktop.Controls.MenuItem { Header = "Context action" } }
            };

            using var host = DialogPopupTestHost.Open(anchor);
            contextMenu.Open(anchor);

            host.FindPopupHost().ShouldNotBeNull();
            contextMenu.IsOpen.ShouldBeTrue();
            FindOwningPopup(contextMenu).SurfaceBackground.ShouldBeNull();

            contextMenu.Close();
            DialogPopupTestHost.Pump();
            contextMenu.IsOpen.ShouldBeFalse();
            host.AssertNoPopupHosts();
        });
    }

    private static AtomUI.Desktop.Controls.Popup FindOwningPopup(Visual visual)
    {
        return AtomUI.Desktop.Controls.PopupUtils.FindOwningPopup(visual)
                     .ShouldBeOfType<AtomUI.Desktop.Controls.Popup>();
    }
}
