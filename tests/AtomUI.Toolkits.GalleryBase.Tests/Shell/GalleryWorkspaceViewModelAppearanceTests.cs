using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.Shell;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using Shouldly;
using Xunit;

namespace AtomUI.Toolkits.GalleryBase.Tests.Shell;

public class GalleryWorkspaceViewModelAppearanceTests
{
    static GalleryWorkspaceViewModelAppearanceTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public async Task Appearance_Mode_Applies_Explicit_Light_And_Dark_Algorithms()
    {
        var configuration = CreateNavigationConfiguration();
        var manager       = Application.Current!.GetThemeManager()!;
        var systemSource  = new TestSystemAppearanceSource(ThemeAppearance.Dark);

        await RestoreDefaultThemeAsync(manager);
        var viewModel = new GalleryWorkspaceViewModel(configuration, null, systemSource);
        try
        {
            await viewModel.SetAppearanceModeCommand.Execute(ThemePreference.Dark).ToTask();

            viewModel.AppearanceMode.ShouldBe(ThemePreference.Dark);
            viewModel.IsDarkAppearanceMode.ShouldBeTrue();
            viewModel.IsLightAppearanceMode.ShouldBeFalse();
            viewModel.IsSystemAppearanceMode.ShouldBeFalse();
            manager.CurrentTheme!.Appearance.ShouldBe(ThemeAppearance.Dark);
            manager.CurrentTheme.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Dark]);

            await viewModel.SetAppearanceModeCommand.Execute(ThemePreference.Light).ToTask();

            viewModel.AppearanceMode.ShouldBe(ThemePreference.Light);
            viewModel.IsLightAppearanceMode.ShouldBeTrue();
            viewModel.IsDarkAppearanceMode.ShouldBeFalse();
            manager.CurrentTheme!.Appearance.ShouldBe(ThemeAppearance.Light);
            manager.CurrentTheme.Algorithms.ShouldBe([ThemeAlgorithm.Default]);
            systemSource.ActiveSubscriptionCount.ShouldBe(0);
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task System_Appearance_Mode_Applies_Current_System_Appearance_And_Follows_Changes()
    {
        var configuration = CreateNavigationConfiguration();
        var manager       = Application.Current!.GetThemeManager()!;
        var systemSource  = new TestSystemAppearanceSource(ThemeAppearance.Dark);

        await RestoreDefaultThemeAsync(manager);
        var viewModel = new GalleryWorkspaceViewModel(configuration, null, systemSource);
        try
        {
            await viewModel.SetAppearanceModeCommand.Execute(ThemePreference.System).ToTask();

            viewModel.AppearanceMode.ShouldBe(ThemePreference.System);
            viewModel.IsSystemAppearanceMode.ShouldBeTrue();
            manager.CurrentTheme!.Appearance.ShouldBe(ThemeAppearance.Dark);
            manager.CurrentTheme.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Dark]);
            systemSource.ActiveSubscriptionCount.ShouldBe(1);

            systemSource.SetAppearance(ThemeAppearance.Light);
            await WaitForThemeAppearanceAsync(manager, ThemeAppearance.Light);

            viewModel.AppearanceMode.ShouldBe(ThemePreference.System);
            manager.CurrentTheme!.Algorithms.ShouldBe([ThemeAlgorithm.Default]);
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Leaving_System_Appearance_Mode_Unsubscribes_From_System_Changes()
    {
        var configuration = CreateNavigationConfiguration();
        var manager       = Application.Current!.GetThemeManager()!;
        var systemSource  = new TestSystemAppearanceSource(ThemeAppearance.Dark);

        await RestoreDefaultThemeAsync(manager);
        var viewModel = new GalleryWorkspaceViewModel(configuration, null, systemSource);
        try
        {
            await viewModel.SetAppearanceModeCommand.Execute(ThemePreference.System).ToTask();
            await viewModel.SetAppearanceModeCommand.Execute(ThemePreference.Light).ToTask();

            systemSource.ActiveSubscriptionCount.ShouldBe(0);
            manager.CurrentTheme!.Appearance.ShouldBe(ThemeAppearance.Light);

            systemSource.SetAppearance(ThemeAppearance.Dark);

            manager.CurrentTheme!.Appearance.ShouldBe(ThemeAppearance.Light);
            manager.CurrentTheme.Algorithms.ShouldBe([ThemeAlgorithm.Default]);
        }
        finally
        {
            viewModel.Dispose();
            await RestoreDefaultThemeAsync(manager);
        }
    }

    [Fact]
    public async Task Dispose_Unsubscribes_System_Appearance_Mode()
    {
        var configuration = CreateNavigationConfiguration();
        var manager       = Application.Current!.GetThemeManager()!;
        var systemSource  = new TestSystemAppearanceSource(ThemeAppearance.Dark);

        await RestoreDefaultThemeAsync(manager);
        var viewModel = new GalleryWorkspaceViewModel(configuration, null, systemSource);

        await viewModel.SetAppearanceModeCommand.Execute(ThemePreference.System).ToTask();
        systemSource.ActiveSubscriptionCount.ShouldBe(1);

        viewModel.Dispose();

        systemSource.ActiveSubscriptionCount.ShouldBe(0);
        await RestoreDefaultThemeAsync(manager);
    }

    private static GalleryBaseConfiguration CreateNavigationConfiguration()
    {
        var options = new GalleryBaseOptions();
        options.Navigation.DefaultRoute = "Overview";
        options.Navigation.AddPage("Overview", "Overview");
        options.Routes.Map(
            "Overview",
            screen => new TestRouteViewModel(screen),
            () => new TestRouteView());
        return options.BuildConfiguration();
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

    private static async Task WaitForThemeAppearanceAsync(IThemeManager manager, ThemeAppearance appearance)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            if (manager.CurrentTheme?.Appearance == appearance)
            {
                return;
            }
            await Task.Delay(25);
        }

        manager.CurrentTheme.ShouldNotBeNull();
        manager.CurrentTheme!.Appearance.ShouldBe(appearance);
    }

    private sealed class TestSystemAppearanceSource(ThemeAppearance currentAppearance)
        : IGallerySystemAppearanceSource
    {
        private readonly List<Action<ThemeAppearance>> _handlers = new();

        public int ActiveSubscriptionCount => _handlers.Count;

        public ThemeAppearance GetCurrentAppearance()
        {
            return currentAppearance;
        }

        public IDisposable Subscribe(Action<ThemeAppearance> handler)
        {
            _handlers.Add(handler);
            return Disposable.Create(() => _handlers.Remove(handler));
        }

        public void SetAppearance(ThemeAppearance appearance)
        {
            currentAppearance = appearance;
            foreach (var handler in _handlers.ToArray())
            {
                handler(appearance);
            }
        }
    }

    private sealed class TestRouteViewModel(IScreen hostScreen) : ReactiveObject, IRoutableViewModel
    {
        public string? UrlPathSegment => "Overview";

        public IScreen HostScreen { get; } = hostScreen;
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
}
