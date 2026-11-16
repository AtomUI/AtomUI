using System.Reflection;
using System.Reactive.Threading.Tasks;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.Navigation;
using AtomUI.Toolkits.GalleryBase.Routing;
using AtomUI.Toolkits.GalleryBase.Shell;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
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

    [Fact]
    public async Task Workspace_ViewModel_Disabling_Motion_Also_Disables_WaveSpirit()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager       = Application.Current!.GetThemeManager()!;
        ThemeConfig? appliedConfig = null;
        void HandleThemeChanged(object? _, ThemeChangedEventArgs args)
        {
            if (args.Request.Reason == ThemeTransitionReason.UserRequest)
            {
                appliedConfig = args.Request.Config;
            }
        }

        manager.ThemeChanged += HandleThemeChanged;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.ToggleMotionCommand.Execute(false).ToTask();

            appliedConfig.ShouldNotBeNull();
            ReadToken(appliedConfig, nameof(SharedTokenKind.EnableMotion)).ShouldBe("false");
            ReadToken(appliedConfig, nameof(SharedTokenKind.EnableWaveSpirit)).ShouldBe("false");
        }
        finally
        {
            manager.ThemeChanged -= HandleThemeChanged;
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Workspace_ViewModel_Enabling_WaveSpirit_Also_Enables_Motion()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager       = Application.Current!.GetThemeManager()!;
        ThemeConfig? appliedConfig = null;
        void HandleThemeChanged(object? _, ThemeChangedEventArgs args)
        {
            if (args.Request.Reason == ThemeTransitionReason.UserRequest)
            {
                appliedConfig = args.Request.Config;
            }
        }

        manager.ThemeChanged += HandleThemeChanged;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.ToggleMotionCommand.Execute(false).ToTask();
            appliedConfig = null;

            await viewModel.ToggleWaveSpiritCommand.Execute(true).ToTask();

            appliedConfig.ShouldNotBeNull();
            ReadToken(appliedConfig, nameof(SharedTokenKind.EnableMotion)).ShouldBe("true");
            ReadToken(appliedConfig, nameof(SharedTokenKind.EnableWaveSpirit)).ShouldBe("true");
        }
        finally
        {
            manager.ThemeChanged -= HandleThemeChanged;
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Workspace_ViewModel_Switches_Theme_Id_And_Preserves_Orthogonal_Settings()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        ThemeRequest? committedRequest = null;
        void HandleThemeChanged(object? _, ThemeChangedEventArgs args)
        {
            if (args.Request.Reason == ThemeTransitionReason.UserRequest)
            {
                committedRequest = args.Request;
            }
        }

        manager.ThemeChanged += HandleThemeChanged;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            viewModel.AvailableThemes.ShouldBe(manager.AvailableThemes);
            viewModel.CurrentThemeId.ShouldBe(manager.CurrentTheme!.ThemeId);

            await viewModel.ToggleDarkModeCommand.Execute(true).ToTask();
            await viewModel.ToggleCompactModeCommand.Execute(true).ToTask();
            await viewModel.ToggleMotionCommand.Execute(false).ToTask();
            committedRequest = null;

            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();
            await viewModel.SetBrandColorCommand.Execute(Color.Parse("#52C41A")).ToTask();

            viewModel.CurrentThemeId.ShouldBe("DaybreakBlue");
            manager.CurrentTheme!.ThemeId.ShouldBe("DaybreakBlue");
            ReadToken(committedRequest!.Config, nameof(SharedTokenKind.ColorPrimary)).ShouldBe("#52C41A");
            manager.CurrentTheme.Appearance.ShouldBe(ThemeAppearance.Dark);
            manager.CurrentTheme.Algorithms.ShouldBe([
                ThemeAlgorithm.Default,
                ThemeAlgorithm.Compact,
                ThemeAlgorithm.Dark
            ]);
            committedRequest.ShouldNotBeNull();
            ReadToken(committedRequest!.Config, nameof(SharedTokenKind.EnableMotion)).ShouldBe("false");
            ReadToken(committedRequest.Config, nameof(SharedTokenKind.EnableWaveSpirit)).ShouldBe("false");
        }
        finally
        {
            manager.ThemeChanged -= HandleThemeChanged;
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    // Profile families (PrimeNG, shadcn/ui, Bootstrap, Material) ship explicit .Dark / .Compact / .DarkCompact profiles
    // whose own algorithms decide appearance and density; the gallery must not replace them with the previous theme's.
    [Fact]
    public async Task Workspace_ViewModel_Applies_A_Profile_With_Its_Own_Algorithms()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            manager.CurrentTheme!.Appearance.ShouldBe(ThemeAppearance.Light);

            await viewModel.SwitchThemeCommand.Execute("PrimeNGLara.Dark").ToTask();

            manager.CurrentTheme!.ThemeId.ShouldBe("PrimeNGLara.Dark");
            manager.CurrentTheme.Appearance.ShouldBe(ThemeAppearance.Dark);
            manager.CurrentTheme.Algorithms.ShouldBe([new ThemeAlgorithm("PrimeNG", "Lara.Dark")]);
            viewModel.AppearanceMode.ShouldBe(ThemePreference.Dark);
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Workspace_ViewModel_Toggles_Select_The_Matching_Profile_Variant()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.SwitchThemeCommand.Execute("PrimeNGLara.Dark").ToTask();

            await viewModel.ToggleCompactModeCommand.Execute(true).ToTask();
            manager.CurrentTheme!.ThemeId.ShouldBe("PrimeNGLara.DarkCompact");

            await viewModel.SetAppearanceModeCommand.Execute(ThemePreference.Light).ToTask();
            manager.CurrentTheme!.ThemeId.ShouldBe("PrimeNGLara.Compact");
            manager.CurrentTheme.Appearance.ShouldBe(ThemeAppearance.Light);

            await viewModel.ToggleCompactModeCommand.Execute(false).ToTask();
            manager.CurrentTheme!.ThemeId.ShouldBe("PrimeNGLara");
            viewModel.CurrentThemeId.ShouldBe("PrimeNGLara");
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Workspace_ViewModel_Leaving_A_Profile_Does_Not_Carry_Its_Algorithm()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.SwitchThemeCommand.Execute("PrimeNGAura.Dark").ToTask();

            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();

            manager.CurrentTheme!.ThemeId.ShouldBe("DaybreakBlue");
            manager.CurrentTheme.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Dark]);
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    // SwitchThemeCommand keeps its pre-design-language contract: a theme that is its language's only (base) definition
    // takes the gallery's current Dark/Compact toggles instead of resetting them to its own light, normal profile.
    [Fact]
    public async Task Workspace_ViewModel_SwitchTheme_To_A_Base_Theme_Keeps_Dark_Toggle()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.SwitchThemeCommand.Execute("PrimeNGAura.Dark").ToTask();

            await viewModel.SwitchThemeCommand.Execute("DaybreakBlue").ToTask();

            manager.CurrentTheme!.ThemeId.ShouldBe("DaybreakBlue");
            manager.CurrentTheme.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Dark]);
            manager.CurrentTheme.Appearance.ShouldBe(ThemeAppearance.Dark);
            viewModel.AppearanceMode.ShouldBe(ThemePreference.Dark);
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Workspace_ViewModel_SwitchTheme_To_A_Base_Theme_Keeps_Compact_Toggle()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.SwitchThemeCommand.Execute("PrimeNGLara.Compact").ToTask();

            await viewModel.SwitchThemeCommand.Execute("DaybreakBlue").ToTask();

            manager.CurrentTheme!.ThemeId.ShouldBe("DaybreakBlue");
            manager.CurrentTheme.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Compact]);
            viewModel.AppearanceMode.ShouldBe(ThemePreference.Light);

            await viewModel.ToggleDarkModeCommand.Execute(true).ToTask();
            manager.CurrentTheme!.Algorithms.ShouldBe([
                ThemeAlgorithm.Default,
                ThemeAlgorithm.Compact,
                ThemeAlgorithm.Dark
            ]);
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Workspace_ViewModel_SwitchDesignLanguage_Ignores_An_Unknown_Language()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        await RestoreDefaultThemeAsync(manager);
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            var transitionId = ReadNextThemeTransitionId(manager);

            await viewModel.SwitchDesignLanguageCommand.Execute("NoSuchLanguage").ToTask();

            ReadNextThemeTransitionId(manager).ShouldBe(transitionId);
            manager.CurrentTheme!.ThemeId.ShouldBe(IThemeManager.DEFAULT_THEME_ID);
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Workspace_ViewModel_Does_Not_Reapply_Selected_Theme_Id()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager       = Application.Current!.GetThemeManager()!;

        await RestoreDefaultThemeAsync(manager);
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            viewModel.CurrentThemeId.ShouldBe(manager.CurrentTheme!.ThemeId);
            var transitionId = ReadNextThemeTransitionId(manager);

            await viewModel.SwitchThemeCommand.Execute(viewModel.CurrentThemeId).ToTask();

            ReadNextThemeTransitionId(manager).ShouldBe(transitionId);
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
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

    [Fact]
    public async Task Workspace_Remembers_Brand_Color_Per_Design_Language()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();
            viewModel.CanChooseBrandColor.ShouldBeTrue();
            await viewModel.SetBrandColorCommand.Execute(Color.Parse("#52C41A")).ToTask();

            await viewModel.SwitchDesignLanguageCommand.Execute("PrimeNG.Aura").ToTask();
            await viewModel.SetBrandColorCommand.Execute(Color.Parse("#6366F1")).ToTask();
            ReadRootColorPrimary(manager).ShouldBe(Color.Parse("#6366F1"));

            await viewModel.SwitchDesignLanguageCommand.Execute("Shadcn.Nova").ToTask();
            viewModel.CanChooseBrandColor.ShouldBeTrue();
            viewModel.BrandColor.ShouldBeNull();
            ReadRootColorPrimary(manager).ShouldBe(Color.Parse("#171717"));

            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();
            viewModel.CurrentDesignLanguage.ShouldBe("AntDesign");
            viewModel.BrandColor.ShouldBe(Color.Parse("#52C41A"));
            ReadRootColorPrimary(manager).ShouldBe(Color.Parse("#52C41A"));

            await viewModel.SwitchDesignLanguageCommand.Execute("PrimeNG.Aura").ToTask();
            ReadRootColorPrimary(manager).ShouldBe(Color.Parse("#6366F1"));
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task PrimeNG_Defaults_To_Noir_Until_A_Brand_Color_Is_Chosen()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.SwitchDesignLanguageCommand.Execute("PrimeNG.Aura").ToTask();
            manager.CurrentTheme!.Appearance.ShouldBe(ThemeAppearance.Light);
            viewModel.BrandColor.ShouldBeNull();
            ReadRootColorPrimary(manager).ShouldBe(Color.Parse("#020617"));

            await viewModel.SetBrandColorCommand.Execute(Color.Parse("#10B981")).ToTask();
            viewModel.BrandColor.ShouldBe(Color.Parse("#10B981"));
            ReadRootColorPrimary(manager).ShouldBe(Color.Parse("#10B981"));

            // Noir is the theme's own accent colour, so choosing it clears the brand choice.
            await viewModel.SetBrandColorCommand.Execute(Color.Parse("#020617")).ToTask();
            viewModel.BrandColor.ShouldBeNull();
            ReadRootColorPrimary(manager).ShouldBe(Color.Parse("#020617"));
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Fixed_Language_Ignores_SetBrandColor()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.SwitchDesignLanguageCommand.Execute("Bootstrap").ToTask();
            var before = ReadRootColorPrimary(manager);

            await viewModel.SetBrandColorCommand.Execute(Color.Parse("#FF0000")).ToTask();

            viewModel.BrandColor.ShouldBeNull();
            ReadRootColorPrimary(manager).ShouldBe(before);
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Toggles_Keep_The_Brand_Color()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.SwitchDesignLanguageCommand.Execute("PrimeNG.Aura").ToTask();
            var defaultPrimary = ReadRootColorPrimary(manager);
            await viewModel.SetBrandColorCommand.Execute(Color.Parse("#123456")).ToTask();

            await viewModel.ToggleDarkModeCommand.Execute(true).ToTask();
            await viewModel.ToggleCompactModeCommand.Execute(true).ToTask();

            manager.CurrentTheme!.ThemeId.ShouldBe("PrimeNGAura.DarkCompact");
            viewModel.BrandColor.ShouldBe(Color.Parse("#123456"));
            ReadRootColorPrimary(manager).ShouldNotBe(defaultPrimary);
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Neutral_Color_Is_Remembered_Per_Language()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.SwitchDesignLanguageCommand.Execute("Shadcn.Nova").ToTask();
            viewModel.CanChooseNeutralColor.ShouldBeTrue();

            await viewModel.SetNeutralColorCommand.Execute(Color.Parse("#71717B")).ToTask();
            viewModel.NeutralColor.ShouldBe(Color.Parse("#71717B"));

            // The choice is per language: another language neither shows nor keeps it.
            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();
            viewModel.CanChooseNeutralColor.ShouldBeFalse();
            viewModel.NeutralColor.ShouldBeNull();

            await viewModel.SwitchDesignLanguageCommand.Execute("Shadcn.Nova").ToTask();
            viewModel.NeutralColor.ShouldBe(Color.Parse("#71717B"));
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task NeutralColor_Is_Not_Applied_To_AntDesign()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();
            var plain = CaptureGlobals(manager);

            await viewModel.SwitchDesignLanguageCommand.Execute("Shadcn.Nova").ToTask();
            await viewModel.SetNeutralColorCommand.Execute(Color.Parse("#71717B")).ToTask();
            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();

            var after = CaptureGlobals(manager);
            after.Count.ShouldBe(plain.Count);
            foreach (var (name, value) in plain)
            {
                after.TryGetValue(name, out var afterValue).ShouldBeTrue(name);
                afterValue.ShouldBe(value, name);
            }
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Choosing_The_Definition_Neutral_Clears_The_Choice()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            await viewModel.SwitchDesignLanguageCommand.Execute("Shadcn.Nova").ToTask();
            await viewModel.SetNeutralColorCommand.Execute(Color.Parse("#71717B")).ToTask();

            // The definition's own seed (#737373) means "no custom neutral".
            await viewModel.SetNeutralColorCommand.Execute(Color.Parse("#737373")).ToTask();

            viewModel.NeutralColor.ShouldBeNull();
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    private static Dictionary<string, object?> CaptureGlobals(IThemeManager manager)
    {
        var snapshot = ((ThemeManager)manager).CurrentSnapshot!;
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var descriptor in snapshot.Registry.GlobalTokens)
        {
            result[descriptor.Name] = snapshot.GlobalTokenValues.GetValue(descriptor.Slot);
        }

        return result;
    }

    private static Color ReadRootColorPrimary(IThemeManager manager)
    {
        var snapshot = ((ThemeManager)manager).CurrentSnapshot!;
        snapshot.Registry.TryGetGlobalToken(nameof(SharedTokenKind.ColorPrimary), out var token).ShouldBeTrue();
        return snapshot.GlobalTokenValues.Get<Color>(token!.Slot);
    }

    private static string ReadToken(ThemeConfig? config, string name)
    {
        config.ShouldNotBeNull();
        config!.Tokens.TryGetValue(name, out var value).ShouldBeTrue();
        return value!;
    }

    private static long ReadNextThemeTransitionId(IThemeManager manager)
    {
        var field = manager.GetType().GetField(
            "_nextTransitionId",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (long)field!.GetValue(manager)!;
    }

    private static async Task RestoreDefaultThemeAsync(IThemeManager manager)
    {
        var config = new ThemeConfigBuilder()
                     .WithAlgorithms(ThemeAlgorithm.Default)
                     .WithToken(nameof(SharedTokenKind.EnableMotion), "true")
                     .WithToken(nameof(SharedTokenKind.EnableWaveSpirit), "true")
                     .Build();
        await manager.ApplyThemeAsync(
            new ThemeRequest(
                IThemeManager.DEFAULT_THEME_ID,
                config,
                ThemeTransitionReason.UserRequest));
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

    [Fact]
    public async Task Theme_Declared_WaveSpirit_Wins_Until_The_User_Toggles()
    {
        var configuration = CreateNavigationConfiguration(new Dictionary<EntityKey, int>());
        var manager = Application.Current!.GetThemeManager()!;
        var viewModel = new GalleryWorkspaceViewModel(configuration);
        try
        {
            // The Shadcn profiles declare EnableWaveSpirit=false; the gallery toggle defaults to on and the
            // user has not touched it, so the declaration must reach the rendered snapshot.
            await viewModel.SwitchDesignLanguageCommand.Execute("Shadcn.Nova").ToTask();
            ReadRootBool(manager, nameof(SharedTokenKind.EnableWaveSpirit)).ShouldBeFalse();

            // Back on a theme without a declaration the global toggle keeps its value (on).
            await viewModel.SwitchDesignLanguageCommand.Execute("AntDesign").ToTask();
            ReadRootBool(manager, nameof(SharedTokenKind.EnableWaveSpirit)).ShouldBeTrue();

            // Once the user toggles, the explicit choice overrides any declaration, on any theme.
            await viewModel.SwitchDesignLanguageCommand.Execute("Shadcn.Nova").ToTask();
            await viewModel.ToggleWaveSpiritCommand.Execute(true).ToTask();
            ReadRootBool(manager, nameof(SharedTokenKind.EnableWaveSpirit)).ShouldBeTrue();
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    private static bool ReadRootBool(IThemeManager manager, string name)
    {
        var snapshot = ((ThemeManager)manager).CurrentSnapshot!;
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue(name);
        return snapshot.GlobalTokenValues.Get<bool>(token!.Slot);
    }
}
