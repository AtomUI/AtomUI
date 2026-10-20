using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using AtomUI.Localization;
using AtomUI.Toolkits.GalleryBase.Shell;
using AtomUIGallery.Workspace.Views;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomUIToolTip = AtomUI.Desktop.Controls.ToolTip;
using AvaloniaToolTip = Avalonia.Controls.ToolTip;
using DesktopButton = AtomUI.Desktop.Controls.Button;

namespace AtomUIGallery.Tests.Workspace;

public class CaseNavigationLayoutTests
{
    [Fact]
    public void Sidebar_Navigation_Implements_The_Explicit_NavMenu_Host_Contract()
    {
        AvaloniaTestApp.EnsureInitialized();
        var navigation = new CaseNavigation();
        var navMenu = navigation.FindControl<NavMenu>("ShowCaseNavMenu");
        var collapseButton = GetSidebarHeaderAction(navigation);

        navMenu.ShouldNotBeNull();
        ((IGallerySidebarNavMenuHost)navigation).SidebarNavMenu.ShouldBeSameAs(navMenu);
        collapseButton.Name.ShouldBe("NavigationCollapseButton");
    }

    [Fact]
    public void Sidebar_Collapse_Button_Changes_Only_The_NavMenu_Collapsed_State()
    {
        AvaloniaTestApp.EnsureInitialized();
        var navigation = new CaseNavigation();
        var navMenu = navigation.FindControl<NavMenu>("ShowCaseNavMenu")!;
        var collapseButton = GetSidebarHeaderAction(navigation);
        var firstEntry = navMenu.Items[0];
        var manager = Application.Current.ShouldNotBeNull().GetLanguageManager().ShouldNotBeNull();
        var window = new Avalonia.Controls.Window { Content = navigation };

        try
        {
            manager.ChangeLanguage(LanguageTags.EnUS);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            navMenu.IsInlineCollapsed.ShouldBeFalse();
            collapseButton.Icon.ShouldBeOfType<MenuFoldOutlined>();
            collapseButton.HorizontalAlignment.ShouldBe(HorizontalAlignment.Center);
            AutomationProperties.GetName(collapseButton).ShouldBe("Collapse navigation");

            collapseButton.RaiseEvent(new RoutedEventArgs(DesktopButton.ClickEvent));

            navMenu.IsInlineCollapsed.ShouldBeTrue();
            navMenu.Items[0].ShouldBeSameAs(firstEntry);
            collapseButton.Icon.ShouldBeOfType<MenuUnfoldOutlined>();
            collapseButton.HorizontalAlignment.ShouldBe(HorizontalAlignment.Center);
            AutomationProperties.GetName(collapseButton).ShouldBe("Expand navigation");

            collapseButton.RaiseEvent(new RoutedEventArgs(DesktopButton.ClickEvent));
            navMenu.IsInlineCollapsed.ShouldBeFalse();
            navMenu.Items[0].ShouldBeSameAs(firstEntry);
            collapseButton.Icon.ShouldBeOfType<MenuFoldOutlined>();
            collapseButton.HorizontalAlignment.ShouldBe(HorizontalAlignment.Center);
            AutomationProperties.GetName(collapseButton).ShouldBe("Collapse navigation");

            manager.ChangeLanguage(LanguageTags.ZhCN);
            Dispatcher.UIThread.RunJobs();
            AutomationProperties.GetName(collapseButton).ShouldBe("收起导航");

            collapseButton.RaiseEvent(new RoutedEventArgs(DesktopButton.ClickEvent));
            navMenu.IsInlineCollapsed.ShouldBeTrue();
            AutomationProperties.GetName(collapseButton).ShouldBe("展开导航");
        }
        finally
        {
            manager.ChangeLanguage(LanguageTags.EnUS);
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Sidebar_Collapse_Button_Does_Not_Create_A_ToolTip()
    {
        AvaloniaTestApp.EnsureInitialized();
        var navigation = new CaseNavigation();
        var collapseButton = GetSidebarHeaderAction(navigation);

        AtomUIToolTip.GetTip(collapseButton).ShouldBeNull();
        AvaloniaToolTip.GetTip(collapseButton).ShouldBeNull();

        collapseButton.RaiseEvent(new RoutedEventArgs(DesktopButton.ClickEvent));

        AtomUIToolTip.GetTip(collapseButton).ShouldBeNull();
        AvaloniaToolTip.GetTip(collapseButton).ShouldBeNull();
    }

    private static DesktopButton GetSidebarHeaderAction(CaseNavigation navigation)
    {
        return ((IGallerySidebarNavMenuHost)navigation).SidebarHeaderAction.ShouldBeOfType<DesktopButton>();
    }
}
