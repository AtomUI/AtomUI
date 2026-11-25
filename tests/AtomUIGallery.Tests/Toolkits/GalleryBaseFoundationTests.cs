using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.Navigation;
using AtomUI.Toolkits.GalleryBase.Routing;
using AtomUI.Toolkits.GalleryBase.Shell;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Toolkits;

public class GalleryBaseFoundationTests
{
    static GalleryBaseFoundationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Route_Registry_Creates_ViewModel_And_Rejects_Duplicate_Routes()
    {
        var registry = new GalleryRouteRegistry();
        registry.Map("Overview", screen => new TestRouteViewModel(screen), () => new TestRouteView());

        var viewModel = registry.CreateViewModel("Overview", new TestScreen());

        viewModel.ShouldBeOfType<TestRouteViewModel>();
        registry.ContainsRoute("Overview").ShouldBeTrue();
        Should.Throw<GalleryConfigurationException>(() =>
            registry.Map("Overview", screen => new TestRouteViewModel(screen), () => new TestRouteView()))
            .Message.ShouldContain("Overview");
    }

    [Fact]
    public void Configuration_Requires_Default_Route_To_Be_Registered_Page()
    {
        var missingRouteOptions = new GalleryBaseOptions();
        missingRouteOptions.Navigation.DefaultRoute = "Overview";
        missingRouteOptions.Navigation.AddPage("Overview", "Overview");

        Should.Throw<GalleryConfigurationException>(() => missingRouteOptions.BuildConfiguration())
              .Message.ShouldContain("Overview");

        var groupRouteOptions = new GalleryBaseOptions();
        groupRouteOptions.Navigation.DefaultRoute = "Components";
        groupRouteOptions.Navigation.AddGroup("Components", "Components");
        groupRouteOptions.Routes.Map("Components", screen => new TestRouteViewModel(screen), () => new TestRouteView());

        Should.Throw<GalleryConfigurationException>(() => groupRouteOptions.BuildConfiguration())
              .Message.ShouldContain("Components");

        var validOptions = new GalleryBaseOptions();
        validOptions.Navigation.DefaultRoute = "Overview";
        validOptions.Navigation.AddPage("Overview", "Overview");
        validOptions.Routes.Map("Overview", screen => new TestRouteViewModel(screen), () => new TestRouteView());

        var configuration = validOptions.BuildConfiguration();

        configuration.DefaultRoute.ShouldBe(new EntityKey("Overview"));
        configuration.NavigationNodes.Count.ShouldBe(1);
        configuration.Routes.ContainsRoute("Overview").ShouldBeTrue();
    }

    [Fact]
    public void Configuration_Creates_ReadOnly_Route_Snapshot()
    {
        var options = new GalleryBaseOptions();
        options.Navigation.DefaultRoute = "Overview";
        options.Navigation.AddPage("Overview", "Overview");
        options.Routes.Map("Overview", screen => new TestRouteViewModel(screen), () => new TestRouteView());

        var configuration = options.BuildConfiguration();

        options.Routes.Map("Button", screen => new TestRouteViewModel(screen), () => new TestRouteView());

        configuration.Routes.ContainsRoute("Overview").ShouldBeTrue();
        configuration.Routes.ContainsRoute("Button").ShouldBeFalse();
        Should.Throw<GalleryConfigurationException>(() =>
            configuration.Routes.Map("Card", screen => new TestRouteViewModel(screen), () => new TestRouteView()))
            .Message.ShouldContain("read-only");
        Should.Throw<NotSupportedException>(() =>
            ((IList<GalleryRouteDescriptor>)configuration.Routes.Routes).Clear());
        configuration.Routes.ContainsRoute("Overview").ShouldBeTrue();
    }

    [Fact]
    public void Configuration_Requires_Default_Open_Keys_To_Be_Registered_Groups()
    {
        var missingOpenKeyOptions = new GalleryBaseOptions();
        missingOpenKeyOptions.Navigation.DefaultRoute = "Overview";
        missingOpenKeyOptions.Navigation.DefaultOpenKeys.Add("Missing");
        missingOpenKeyOptions.Navigation.AddPage("Overview", "Overview");
        missingOpenKeyOptions.Routes.Map("Overview", screen => new TestRouteViewModel(screen), () => new TestRouteView());

        Should.Throw<GalleryConfigurationException>(() => missingOpenKeyOptions.BuildConfiguration())
              .Message.ShouldContain("Missing");

        var pageOpenKeyOptions = new GalleryBaseOptions();
        pageOpenKeyOptions.Navigation.DefaultRoute = "Overview";
        pageOpenKeyOptions.Navigation.DefaultOpenKeys.Add("Overview");
        pageOpenKeyOptions.Navigation.AddPage("Overview", "Overview");
        pageOpenKeyOptions.Routes.Map("Overview", screen => new TestRouteViewModel(screen), () => new TestRouteView());

        Should.Throw<GalleryConfigurationException>(() => pageOpenKeyOptions.BuildConfiguration())
              .Message.ShouldContain("group");

        var validOptions = new GalleryBaseOptions();
        validOptions.Navigation.DefaultRoute = "Overview";
        validOptions.Navigation.DefaultOpenKeys.Add("Components");
        validOptions.Navigation.AddPage("Overview", "Overview");
        validOptions.Navigation.AddGroup("Components", "Components")
                    .AddPage("Button", "Button");
        validOptions.Routes.Map("Overview", screen => new TestRouteViewModel(screen), () => new TestRouteView());
        validOptions.Routes.Map("Button", screen => new TestRouteViewModel(screen), () => new TestRouteView());

        var configuration = validOptions.BuildConfiguration();

        configuration.DefaultOpenKeys.ShouldBe([new EntityKey("Components")]);
    }

    [Fact]
    public void Navigation_Builder_Rejects_Duplicate_Keys_Across_Tree()
    {
        var builder = new GalleryNavigationBuilder();
        builder.AddPage("Overview", "Overview");

        Should.Throw<GalleryConfigurationException>(() => builder.AddGroup("Overview", "Duplicate"))
              .Message.ShouldContain("Overview");

        var components = builder.AddGroup("Components", "Components");
        components.AddPage("Button", "Button");

        Should.Throw<GalleryConfigurationException>(() => builder.AddPage("Button", "Duplicate"))
              .Message.ShouldContain("Button");
    }

    [Fact]
    public void Navigation_ViewModel_Does_Not_Navigate_Group_Or_Current_Route()
    {
        var routeCreationCounts = new Dictionary<EntityKey, int>();
        var configuration       = CreateNavigationConfiguration(routeCreationCounts);
        var screen              = new TestScreen();
        var viewModel           = new GalleryNavigationViewModel(screen, configuration);

        viewModel.NavigateToCommand.Execute("Components").Subscribe();
        viewModel.NavigateToCommand.Execute("Overview").Subscribe();
        viewModel.NavigateToCommand.Execute("Overview").Subscribe();

        routeCreationCounts.GetValueOrDefault("Overview").ShouldBe(1);
        routeCreationCounts.ContainsKey("Components").ShouldBeFalse();
        screen.Router.NavigationStack.Count.ShouldBe(1);
        screen.Router.NavigationStack[0].ShouldBeOfType<TestRouteViewModel>();
    }

    [Fact]
    public void Navigation_ViewModel_Diagnostics_Disables_ShowCase_Deferred_Loading_Until_Stopped()
    {
        var oldValue = Environment.GetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable);
        Environment.SetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable,
            null);
        GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();

        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var viewModel     = new GalleryNavigationViewModel(new TestScreen(), configuration);

        try
        {
            viewModel.TestNavigatePagesCommand.Execute(TimeSpan.FromMilliseconds(300))
                     .Subscribe();

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeTrue();

            viewModel.StopTestNavigatePagesCommand.Execute()
                     .Subscribe();

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable,
                oldValue);
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        }
    }

    [Fact]
    public void Navigation_ViewModel_Dispose_Stops_Diagnostics_And_Resets_ShowCase_Deferred_Loading()
    {
        var oldValue = Environment.GetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable);
        Environment.SetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable,
            null);
        GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();

        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var viewModel     = new GalleryNavigationViewModel(new TestScreen(), configuration);

        try
        {
            viewModel.TestNavigatePagesCommand.Execute(TimeSpan.FromMilliseconds(300))
                     .Subscribe();

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeTrue();

            ((IDisposable)viewModel).Dispose();

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable,
                oldValue);
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        }
    }

    [Fact]
    public void Navigation_ViewModel_Dispose_Does_Not_Reset_External_ShowCase_Deferred_Loading_Override()
    {
        var oldValue = Environment.GetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable);
        Environment.SetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable,
            null);
        GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();

        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var viewModel     = new GalleryNavigationViewModel(new TestScreen(), configuration);

        try
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = true;

            ((IDisposable)viewModel).Dispose();

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable,
                oldValue);
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        }
    }

    [Fact]
    public void Workspace_ViewModel_Dispose_Disposes_Navigation_Runtime()
    {
        var oldValue = Environment.GetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable);
        Environment.SetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable,
            null);
        GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();

        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var viewModel     = new GalleryWorkspaceViewModel(configuration);

        try
        {
            viewModel.Navigation.TestNavigatePagesCommand.Execute(TimeSpan.FromMilliseconds(300))
                     .Subscribe();

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeTrue();

            ((IDisposable)viewModel).Dispose();

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable,
                oldValue);
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        }
    }

    [Fact]
    public void Workspace_ViewModel_Dispose_Uses_Product_Navigation_Dispose()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        TestDisposableNavigationViewModel? navigation = null;
        var viewModel = new GalleryWorkspaceViewModel(
            configuration,
            screen =>
            {
                navigation = new TestDisposableNavigationViewModel(screen, configuration);
                return navigation;
            });

        ((IDisposable)viewModel).Dispose();

        navigation.ShouldNotBeNull();
        navigation.DisposeCallCount.ShouldBe(1);
    }

    private static GalleryBaseConfiguration CreateNavigationConfiguration(
        IDictionary<EntityKey, int> routeCreationCounts)
    {
        var options = new GalleryBaseOptions();
        options.Navigation.DefaultRoute = "Overview";
        options.Navigation.AddPage("Overview", "Overview");
        options.Navigation.AddGroup("Components", "Components")
               .AddPage("Button", "Button");

        options.Routes.Map(
            "Overview",
            screen => CreateRouteViewModel(routeCreationCounts, "Overview", screen),
            () => new TestRouteView());
        options.Routes.Map(
            "Button",
            screen => CreateRouteViewModel(routeCreationCounts, "Button", screen),
            () => new TestRouteView());

        return options.BuildConfiguration();
    }

    private static TestRouteViewModel CreateRouteViewModel(IDictionary<EntityKey, int> routeCreationCounts,
                                                           EntityKey routeKey,
                                                           IScreen screen)
    {
        routeCreationCounts.TryGetValue(routeKey, out var count);
        routeCreationCounts[routeKey] = count + 1;
        return new TestRouteViewModel(screen);
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }

    private sealed class TestRouteViewModel : ReactiveObject, IRoutableViewModel
    {
        public string UrlPathSegment => "test";

        public IScreen HostScreen { get; }

        public TestRouteViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }
    }

    private sealed class TestRouteView : UserControl, IViewFor<TestRouteViewModel>
    {
        public static readonly StyledProperty<TestRouteViewModel?> ViewModelProperty =
            AvaloniaProperty.Register<TestRouteView, TestRouteViewModel?>(nameof(ViewModel));

        public TestRouteViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TestRouteViewModel?)value;
        }
    }

    private sealed class TestDisposableNavigationViewModel : GalleryNavigationViewModel
    {
        public int DisposeCallCount { get; private set; }

        public TestDisposableNavigationViewModel(IScreen hostScreen, GalleryBaseConfiguration configuration)
            : base(hostScreen, configuration)
        {
        }

        public override void Dispose()
        {
            DisposeCallCount++;
            base.Dispose();
        }
    }
}
