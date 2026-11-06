using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Theme.Resources;
using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.Shell;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Toolkits.GalleryBase.Tests.Shell;

public class GalleryShellViewCollapseTests
{
    [Fact]
    public void Media_BreakPoint_Compacts_And_Restores_Collapsible_Navigation()
    {
        AvaloniaTestApp.EnsureInitialized();
        var navigationView = new TestCollapsibleNavigationView();
        var mediaHost = new TestMediaBreakHost(MediaBreakPoint.ExtraLarge);
        using var shell = new GalleryShellView(CreateConfiguration(), navigationView, new RoutingState());
        mediaHost.Children.Add(shell);
        var window = ShowInWindow(mediaHost);

        try
        {
            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();

            mediaHost.SetMediaBreakPoint(MediaBreakPoint.Large);
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();

            mediaHost.SetMediaBreakPoint(MediaBreakPoint.Medium);
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeTrue();

            mediaHost.SetMediaBreakPoint(MediaBreakPoint.ExtraLarge);
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Media_BreakPoint_Property_Change_Compacts_When_Event_Is_Delayed()
    {
        AvaloniaTestApp.EnsureInitialized();
        var navigationView = new TestCollapsibleNavigationView();
        var mediaHost = new TestDelayedMediaBreakHost(MediaBreakPoint.ExtraLarge);
        using var shell = new GalleryShellView(CreateConfiguration(), navigationView, new RoutingState());
        mediaHost.Children.Add(shell);
        var window = ShowInWindow(mediaHost);

        try
        {
            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();

            mediaHost.SetMediaBreakPointWithoutEvent(MediaBreakPoint.ExtraSmall);
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Manual_Collapse_Choice_Is_Preserved_Until_Media_BreakPoint_Band_Changes()
    {
        AvaloniaTestApp.EnsureInitialized();
        var navigationView = new TestCollapsibleNavigationView();
        var mediaHost = new TestMediaBreakHost(MediaBreakPoint.ExtraLarge);
        using var shell = new GalleryShellView(CreateConfiguration(), navigationView, new RoutingState());
        mediaHost.Children.Add(shell);
        var window = ShowInWindow(mediaHost);

        try
        {
            mediaHost.SetMediaBreakPoint(MediaBreakPoint.Large);
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();

            mediaHost.SetMediaBreakPoint(MediaBreakPoint.Medium);
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeTrue();

            navigationView.SidebarNavMenu.IsInlineCollapsed = false;
            Dispatcher.UIThread.RunJobs();
            mediaHost.SetMediaBreakPoint(MediaBreakPoint.Small);
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();

            mediaHost.SetMediaBreakPoint(MediaBreakPoint.ExtraLarge);
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();

            navigationView.SidebarNavMenu.IsInlineCollapsed = true;
            Dispatcher.UIThread.RunJobs();
            mediaHost.SetMediaBreakPoint(MediaBreakPoint.ExtraExtraLarge);
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeTrue();

            mediaHost.SetMediaBreakPoint(MediaBreakPoint.Medium);
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Dispose_Releases_Media_BreakPoint_Subscription()
    {
        AvaloniaTestApp.EnsureInitialized();
        var navigationView = new TestCollapsibleNavigationView();
        var mediaHost = new TestMediaBreakHost(MediaBreakPoint.ExtraLarge);
        var shell = new GalleryShellView(CreateConfiguration(), navigationView, new RoutingState());
        mediaHost.Children.Add(shell);
        var window = ShowInWindow(mediaHost);

        try
        {
            shell.Dispose();
            mediaHost.SetMediaBreakPoint(MediaBreakPoint.Medium);
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Browser_Shell_Media_BreakPoint_Uses_Outer_Shell_Width()
    {
        AvaloniaTestApp.EnsureInitialized();
        var browserShell = new TestBrowserShellView(CreateConfiguration());
        var window = new AvaloniaWindow
        {
            Width   = 1300,
            Height  = 700,
            Content = browserShell
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            browserShell.MediaBreakPoint.ShouldBe(MediaBreakPoint.ExtraLarge);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Browser_Shell_Media_BreakPoint_Styled_Property_Uses_Outer_Shell_Width()
    {
        AvaloniaTestApp.EnsureInitialized();
        var browserShell = new TestBrowserShellView(CreateConfiguration());
        var window = new AvaloniaWindow
        {
            Width   = 1300,
            Height  = 700,
            Content = browserShell
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            browserShell.MediaBreakPoint.ShouldBe(MediaBreakPoint.ExtraLarge);
            browserShell.GetValue(MediaBreakAwareControlProperty.MediaBreakPointProperty)
                        .ShouldBe(MediaBreakPoint.ExtraLarge);

            window.Width = 1000;
            Dispatcher.UIThread.RunJobs();

            browserShell.MediaBreakPoint.ShouldBe(MediaBreakPoint.Large);
            browserShell.GetValue(MediaBreakAwareControlProperty.MediaBreakPointProperty)
                        .ShouldBe(MediaBreakPoint.Large);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Browser_Shell_Media_BreakPoint_Uses_Screen_Token_Resource_Thresholds()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var resourceOverride = OverrideApplicationResource(SharedTokenKind.ScreenXLMin, 1100);
        var browserShell = new TestBrowserShellView(CreateConfiguration());
        var window = new AvaloniaWindow
        {
            Width   = 1120,
            Height  = 700,
            Content = browserShell
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            browserShell.MediaBreakPoint.ShouldBe(MediaBreakPoint.ExtraLarge);
            browserShell.GetValue(MediaBreakAwareControlProperty.MediaBreakPointProperty)
                        .ShouldBe(MediaBreakPoint.ExtraLarge);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Browser_Routed_Content_Uses_Content_Media_BreakPoint_Owner()
    {
        AvaloniaTestApp.EnsureInitialized();
        var browserShell = new TestBrowserShellView(CreateConfiguration());
        var window = new AvaloniaWindow
        {
            Width   = 1300,
            Height  = 700,
            Content = browserShell
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var routedViewHost = browserShell.GetVisualDescendants()
                                             .OfType<RoutedViewHost>()
                                             .Single();
            var contentMediaOwner = routedViewHost.GetSelfAndVisualAncestors()
                                                  .OfType<IMediaBreakAwareControl>()
                                                  .First();

            contentMediaOwner.ShouldNotBeSameAs(browserShell);
            contentMediaOwner.MediaBreakPoint.ShouldBe(MediaBreakPoint.Large);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Browser_Routed_Content_Media_BreakPoint_Styled_Property_Uses_Content_Width()
    {
        AvaloniaTestApp.EnsureInitialized();
        var browserShell = new TestBrowserShellView(CreateConfiguration());
        var window = new AvaloniaWindow
        {
            Width   = 1700,
            Height  = 700,
            Content = browserShell
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var routedViewHost = browserShell.GetVisualDescendants()
                                             .OfType<RoutedViewHost>()
                                             .Single();
            var contentMediaOwner = routedViewHost.GetSelfAndVisualAncestors()
                                                  .OfType<IMediaBreakAwareControl>()
                                                  .First();

            contentMediaOwner.ShouldNotBeSameAs(browserShell);
            contentMediaOwner.MediaBreakPoint.ShouldBe(MediaBreakPoint.ExtraLarge);
            ((AvaloniaObject)contentMediaOwner)
                .GetValue(MediaBreakAwareControlProperty.MediaBreakPointProperty)
                .ShouldBe(MediaBreakPoint.ExtraLarge);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Browser_Sidebar_Collapses_From_Outer_Shell_Media_BreakPoint()
    {
        AvaloniaTestApp.EnsureInitialized();
        var browserShell = new TestBrowserShellView(CreateConfiguration());
        var window = new AvaloniaWindow
        {
            Width   = 1300,
            Height  = 700,
            Content = browserShell
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var sidebarNavMenu = browserShell.GetVisualDescendants()
                                             .OfType<NavMenu>()
                                             .Single();

            browserShell.MediaBreakPoint.ShouldBe(MediaBreakPoint.ExtraLarge);
            sidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();

            window.Width = 1000;
            Dispatcher.UIThread.RunJobs();

            browserShell.MediaBreakPoint.ShouldBe(MediaBreakPoint.Large);
            sidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();

            window.Width = 900;
            Dispatcher.UIThread.RunJobs();

            browserShell.MediaBreakPoint.ShouldBe(MediaBreakPoint.Medium);
            sidebarNavMenu.IsInlineCollapsed.ShouldBeTrue();

            window.Width = 1300;
            Dispatcher.UIThread.RunJobs();

            browserShell.MediaBreakPoint.ShouldBe(MediaBreakPoint.ExtraLarge);
            sidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Browser_Sidebar_Remains_Expanded_When_Browser_Media_BreakPoints_Are_Disabled()
    {
        AvaloniaTestApp.EnsureInitialized();
        var browserShell = new TestBrowserShellView(CreateConfiguration(enableBrowserMediaBreakpoints: false));
        var window = new AvaloniaWindow
        {
            Width   = 1000,
            Height  = 700,
            Content = browserShell
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var sidebarNavMenu = browserShell.GetVisualDescendants()
                                             .OfType<NavMenu>()
                                             .Single();

            sidebarNavMenu.IsInlineCollapsed.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Desktop_AtomUI_Window_Media_BreakPoint_Compacts_Collapsible_Navigation()
    {
        AvaloniaTestApp.EnsureInitialized();
        var navigationView = new TestCollapsibleNavigationView();
        using var shell = new GalleryShellView(CreateConfiguration(), navigationView, new RoutingState());
        var window = new AtomUIWindow
        {
            Width   = 520,
            Height  = 700,
            Content = shell
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            navigationView.SidebarNavMenu.IsInlineCollapsed.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Collapsible_Navigation_Host_Follows_NavMenu_Effective_Width()
    {
        AvaloniaTestApp.EnsureInitialized();
        var navigationView = new TestCollapsibleNavigationView();
        using var shell = new GalleryShellView(CreateConfiguration(), navigationView, new RoutingState());
        var window = ShowInWindow(shell);

        try
        {
            var sidebar = FindNamedBorder(shell, "WorkspaceSidebar");

            navigationView.SidebarNavMenu.Width.ShouldBe(280);
            sidebar.Width.ShouldBe(280);

            navigationView.SidebarNavMenu.IsInlineCollapsed = true;
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.Width.ShouldBe(64);
            sidebar.Width.ShouldBe(64);

            navigationView.SidebarNavMenu.IsInlineCollapsed = false;
            Dispatcher.UIThread.RunJobs();

            navigationView.SidebarNavMenu.Width.ShouldBe(280);
            sidebar.Width.ShouldBe(280);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Plain_Navigation_View_Keeps_Configured_Fixed_Sidebar_Width()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var shell = new GalleryShellView(CreateConfiguration(), new Border(), new RoutingState());
        var window = ShowInWindow(shell);

        try
        {
            var sidebar = FindNamedBorder(shell, "WorkspaceSidebar");
            var rootLayout = FindNamedGrid(shell, "WorkspaceRootLayout");

            sidebar.Width.ShouldBe(double.NaN);
            rootLayout.ColumnDefinitions[0].Width.ShouldBe(new GridLength(280));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Dispose_Releases_Collapsible_Navigation_Width_Binding()
    {
        AvaloniaTestApp.EnsureInitialized();
        var navigationView = new TestCollapsibleNavigationView();
        var shell = new GalleryShellView(CreateConfiguration(), navigationView, new RoutingState());
        var window = ShowInWindow(shell);

        try
        {
            var sidebar = FindNamedBorder(shell, "WorkspaceSidebar");

            sidebar.Width.ShouldBe(280);
            shell.Dispose();
            double.IsNaN(sidebar.Width).ShouldBeTrue();

            navigationView.SidebarNavMenu.IsInlineCollapsed = true;
            Dispatcher.UIThread.RunJobs();

            double.IsNaN(sidebar.Width).ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Brand_Header_Action_And_Footer_Follow_The_Effective_Inline_Collapsed_State()
    {
        AvaloniaTestApp.EnsureInitialized();
        var navigationView = new TestCollapsibleNavigationView();
        using var shell = new GalleryShellView(CreateConfiguration(), navigationView, new RoutingState());
        var window = ShowInWindow(shell);

        try
        {
            var brandHeader = FindNamedGrid(shell, "WorkspaceBrandHost");
            var brandContent = FindNamedBorder(shell, "WorkspaceBrandContent");
            var actionHost = FindNamedBorder(shell, "WorkspaceSidebarHeaderActionHost");
            var footer = FindNamedBorder(shell, "WorkspaceSidebarFooter");

            actionHost.Child.ShouldBeSameAs(navigationView.SidebarHeaderAction);
            brandHeader.Margin.ShouldBe(new Thickness(0, 20, 0, 16));
            brandContent.IsVisible.ShouldBeTrue();
            actionHost.IsVisible.ShouldBeTrue();
            actionHost.HorizontalAlignment.ShouldBe(HorizontalAlignment.Right);
            actionHost.Margin.ShouldBe(new Thickness(0, 0, 4, 0));

            navigationView.SidebarNavMenu.IsInlineCollapsed = true;
            Dispatcher.UIThread.RunJobs();

            brandHeader.IsVisible.ShouldBeTrue();
            brandHeader.Margin.ShouldBe(new Thickness(0, 20, 0, 4));
            brandContent.IsVisible.ShouldBeFalse();
            actionHost.IsVisible.ShouldBeTrue();
            actionHost.HorizontalAlignment.ShouldBe(HorizontalAlignment.Center);
            actionHost.Margin.ShouldBe(default);
            footer.IsVisible.ShouldBeFalse();

            navigationView.SidebarNavMenu.Mode = NavMenuMode.Vertical;
            Dispatcher.UIThread.RunJobs();

            brandContent.IsVisible.ShouldBeTrue();
            actionHost.HorizontalAlignment.ShouldBe(HorizontalAlignment.Right);
            actionHost.Margin.ShouldBe(new Thickness(0, 0, 4, 0));
            footer.IsVisible.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Collapsed_Brand_Header_Bottom_Margin_Follows_UniformlyMarginXXS_Token()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var resourceOverride = OverrideApplicationResource(SharedTokenKind.UniformlyMarginXXS, 6d);
        var navigationView = new TestCollapsibleNavigationView();
        using var shell = new GalleryShellView(CreateConfiguration(), navigationView, new RoutingState());
        var window = ShowInWindow(shell);

        try
        {
            var brandHeader = FindNamedGrid(shell, "WorkspaceBrandHost");

            navigationView.SidebarNavMenu.IsInlineCollapsed = true;
            Dispatcher.UIThread.RunJobs();

            brandHeader.Margin.ShouldBe(new Thickness(0, 20, 0, 6));

            navigationView.SidebarNavMenu.IsInlineCollapsed = false;
            Dispatcher.UIThread.RunJobs();

            brandHeader.Margin.ShouldBe(new Thickness(0, 20, 0, 16));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Collapsed_Brand_Header_Bottom_Margin_Uses_Configured_Spacing_Over_Token()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var resourceOverride = OverrideApplicationResource(SharedTokenKind.UniformlyMarginXXS, 6d);
        var navigationView = new TestCollapsibleNavigationView();
        using var shell = new GalleryShellView(CreateConfiguration(collapsedSidebarHeaderSpacing: 12),
                                               navigationView,
                                               new RoutingState());
        var window = ShowInWindow(shell);

        try
        {
            var brandHeader = FindNamedGrid(shell, "WorkspaceBrandHost");

            navigationView.SidebarNavMenu.IsInlineCollapsed = true;
            Dispatcher.UIThread.RunJobs();

            brandHeader.Margin.ShouldBe(new Thickness(0, 20, 0, 12));

            navigationView.SidebarNavMenu.IsInlineCollapsed = false;
            Dispatcher.UIThread.RunJobs();

            brandHeader.Margin.ShouldBe(new Thickness(0, 20, 0, 16));
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Configuration_Rejects_Invalid_Collapsed_Sidebar_Header_Spacing(double spacing)
    {
        Should.Throw<GalleryConfigurationException>(() => CreateConfiguration(collapsedSidebarHeaderSpacing: spacing))
              .Message.ShouldContain("CollapsedSidebarHeaderSpacing");
    }

    private static AvaloniaWindow ShowInWindow(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width = 1000,
            Height = 700,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static Border FindNamedBorder(Control root, string name)
    {
        return root.GetVisualDescendants()
                   .OfType<Border>()
                   .Single(control => control.Name == name);
    }

    private static Grid FindNamedGrid(Control root, string name)
    {
        return FindNamedControl<Grid>(root, name);
    }

    private static TControl FindNamedControl<TControl>(Control root, string name)
        where TControl : Control
    {
        return root.GetVisualDescendants()
                   .OfType<TControl>()
                   .Single(control => control.Name == name);
    }

    private static IDisposable OverrideApplicationResource(object key, object value)
    {
        var resources = Application.Current!.Resources;
        var hadValue = resources.TryGetValue(key, out var previousValue);
        resources[key] = value;
        return new ApplicationResourceOverride(resources, key, hadValue, previousValue);
    }

    private static GalleryBaseConfiguration CreateConfiguration(bool enableBrowserMediaBreakpoints = true,
                                                                double? collapsedSidebarHeaderSpacing = null)
    {
        var options = new GalleryBaseOptions();
        options.Shell.CollapsedSidebarHeaderSpacing = collapsedSidebarHeaderSpacing;
        options.Platform.EnableBrowserMediaBreakpoints = enableBrowserMediaBreakpoints;
        options.Branding.VersionText = "v1";
        options.Navigation.DefaultRoute = "Overview";
        options.Navigation.AddPage("Overview", "Overview");
        options.Routes.Map(
            "Overview",
            screen => new TestRouteViewModel(screen),
            () => new TestRouteView());
        return options.BuildConfiguration();
    }

    private sealed class ApplicationResourceOverride(
        IResourceDictionary resources,
        object key,
        bool hadValue,
        object? previousValue) : IDisposable
    {
        public void Dispose()
        {
            if (hadValue)
            {
                resources[key] = previousValue;
            }
            else
            {
                resources.Remove(key);
            }
        }
    }

    private sealed class TestCollapsibleNavigationView : UserControl, IGallerySidebarNavMenuHost
    {
        public NavMenu SidebarNavMenu { get; } = new()
        {
            InlineCollapsedWidth = 64,
            IsMotionEnabled = false
        };

        public Control? SidebarHeaderAction { get; } = new Border
        {
            Width  = 40,
            Height = 40
        };

        public TestCollapsibleNavigationView()
        {
            Content = SidebarNavMenu;
        }
    }

    private sealed class TestMediaBreakHost : Panel, IMediaBreakAwareControl
    {
        public TestMediaBreakHost(MediaBreakPoint mediaBreakPoint)
        {
            MediaBreakPoint = mediaBreakPoint;
        }

        public MediaBreakPoint MediaBreakPoint { get; private set; }

        public event EventHandler<MediaBreakPointChangedEventArgs>? MediaBreakPointChanged;

        public void SetMediaBreakPoint(MediaBreakPoint mediaBreakPoint)
        {
            MediaBreakPoint = mediaBreakPoint;
            MediaBreakPointChanged?.Invoke(this, new MediaBreakPointChangedEventArgs(mediaBreakPoint));
        }
    }

    private sealed class TestDelayedMediaBreakHost : Panel, IMediaBreakAwareControl
    {
        public static readonly StyledProperty<MediaBreakPoint> MediaBreakPointProperty =
            MediaBreakAwareControlProperty.MediaBreakPointProperty.AddOwner<TestDelayedMediaBreakHost>();

        public TestDelayedMediaBreakHost(MediaBreakPoint mediaBreakPoint)
        {
            SetCurrentValue(MediaBreakPointProperty, mediaBreakPoint);
        }

        public MediaBreakPoint MediaBreakPoint => GetValue(MediaBreakPointProperty);

        public event EventHandler<MediaBreakPointChangedEventArgs>? MediaBreakPointChanged
        {
            add { }
            remove { }
        }

        public void SetMediaBreakPointWithoutEvent(MediaBreakPoint mediaBreakPoint)
        {
            SetCurrentValue(MediaBreakPointProperty, mediaBreakPoint);
        }
    }

    private sealed class TestBrowserShellView : GalleryBrowserShellView
    {
        public TestBrowserShellView(GalleryBaseConfiguration configuration)
            : base(configuration,
                   workspaceConfiguration => new GalleryWorkspaceViewModel(workspaceConfiguration),
                   _ => new TestCollapsibleNavigationView())
        {
        }
    }

    private sealed class TestRouteViewModel(IScreen hostScreen) : ReactiveObject, IRoutableViewModel
    {
        public string UrlPathSegment => "Overview";

        public IScreen HostScreen { get; } = hostScreen;
    }

    private sealed class TestRouteView : ReactiveUserControl<TestRouteViewModel>
    {
    }
}
