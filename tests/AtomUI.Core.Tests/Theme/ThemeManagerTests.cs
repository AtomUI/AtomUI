using AtomUI.Theme.Algorithms;
using System.Reflection;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeManagerTests
{
    [Fact]
    public async Task Manager_Normalizes_The_Initial_Config_Once_After_Registry_Creation()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var builder = new ThemeManagerBuilder();
            builder.WithInitialTheme(
                IThemeManager.DEFAULT_THEME_ID,
                new ThemeConfigBuilder()
                    .WithToken(nameof(DesignToken.BorderRadius), "12")
                    .Build());
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);
            var field = typeof(ThemeManager)
                .GetField("_normalizedInitialConfig", BindingFlags.Instance | BindingFlags.NonPublic)
                .ShouldNotBeNull();
            var initialNormalization = field.GetValue(manager).ShouldNotBeNull();

            var result = await manager.ApplyThemeAsync(
                new ThemeRequest(
                    IThemeManager.DEFAULT_THEME_ID,
                    new ThemeConfigBuilder()
                        .WithToken(nameof(DesignToken.ColorPrimary), "#00b96b")
                        .Build(),
                    ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);

            result.Status.ShouldBe(ThemeTransitionStatus.Committed);
            field.GetValue(manager).ShouldBeSameAs(initialNormalization);
        });
    }

    [Fact]
    public async Task Unknown_Theme_Fails_Without_Changing_The_Committed_State()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var manager = new ThemeManagerBuilder().Build();
            manager.InitializeApplication(Application.Current!);
            var state = manager.CurrentTheme;
            var snapshot = manager.CurrentSnapshot;

            var result = await manager.ApplyThemeAsync(
                new ThemeRequest("Missing", null, ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);

            result.Status.ShouldBe(ThemeTransitionStatus.Failed);
            manager.CurrentTheme.ShouldBeSameAs(state);
            manager.CurrentSnapshot.ShouldBeSameAs(snapshot);
        });
    }

    [Fact]
    public void Available_Themes_Are_Read_From_The_Bound_V1_Catalog()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = new ThemeManagerBuilder().Build();

            manager.InitializeApplication(Application.Current!);

            manager.AvailableThemes.ShouldBe(
            [
                new ThemeInfo(
                    IThemeManager.DEFAULT_THEME_ID,
                    "Daybreak Blue",
                    ThemeAppearance.Light,
                    true,
                    Color.Parse("#1677FF"))
                {
                    Algorithms = [new ThemeAlgorithm("AtomUI", "Default")]
                }
            ]);
        });
    }

    [Fact]
    public void Dispose_Releases_Application_Resources_And_Registered_Scopes()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            var manager = new ThemeManagerBuilder().Build();
            manager.InitializeApplication(application);
            var provider = new ThemeConfigProvider
            {
                Config = new ThemeConfigBuilder().Build(),
                Child = new Border()
            };
            var root = new LogicalRoot();
            root.SetValue(ThemeScope.ContextProperty, manager.RootContext);
            root.Child = provider;
            manager.ScopeGraph.TryGetNode(provider, out _).ShouldBeTrue();

            manager.Dispose();

            application.Styles.ShouldNotContain(manager);
            manager.Resources.MergedDictionaries
                   .OfType<ThemeTokenResourceProvider>()
                   .ShouldBeEmpty();
            manager.ScopeGraph.TryGetNode(provider, out _).ShouldBeFalse();
            provider.Resources.MergedDictionaries
                    .OfType<ThemeTokenResourceProvider>()
                    .ShouldBeEmpty();
            Should.Throw<ObjectDisposedException>(() => manager.ApplyThemeAsync(
                new ThemeRequest(
                    IThemeManager.DEFAULT_THEME_ID,
                    null,
                    ThemeTransitionReason.UserRequest)));
            root.Child = null;
        });
    }

    [Fact]
    public void Dispose_Releases_The_Complete_Scope_Graph_When_A_Provider_Callback_Throws()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            var manager = new ThemeManagerBuilder().Build();
            manager.InitializeApplication(application);
            var provider = new ThrowingThemeConfigProvider
            {
                Child = new Border()
            };
            var root = new LogicalRoot();
            root.SetValue(ThemeScope.ContextProperty, manager.RootContext);
            root.Child = provider;
            manager.ScopeGraph.TryGetNode(provider, out _).ShouldBeTrue();
            provider.ThrowOnNextContextChange = true;

            manager.Dispose();

            manager.ScopeGraph.TryGetNode(provider, out _).ShouldBeFalse();
            provider.Resources.MergedDictionaries
                    .OfType<ThemeTokenResourceProvider>()
                    .ShouldBeEmpty();
            provider.IsSet(ThemeConfigProvider.RequestedThemeVariantProperty).ShouldBeFalse();
            root.Child = null;
        });
    }

    private sealed class LogicalRoot : Decorator, ILogicalRoot
    {
    }

    private sealed class ThrowingThemeConfigProvider : ThemeConfigProvider
    {
        internal bool ThrowOnNextContextChange { get; set; }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (ThrowOnNextContextChange && change.Property == ThemeScope.ContextProperty)
            {
                ThrowOnNextContextChange = false;
                throw new InvalidOperationException("context update failed");
            }
        }
    }
}
