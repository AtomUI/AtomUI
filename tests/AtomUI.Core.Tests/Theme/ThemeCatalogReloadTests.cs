using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeCatalogReloadTests
{
    [Fact]
    public async Task Catalog_Changes_Publish_An_Atomic_Catalog_Event()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            using var directory = new TemporaryDirectory();
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);
            ThemeCatalogChangedEventArgs? observed = null;
            var observedAtomicState = false;
            manager.ThemeCatalogChanged += (_, args) =>
            {
                observed = args;
                observedAtomicState = manager.AvailableThemes.SequenceEqual(args.AvailableThemes) &&
                                      ReferenceEquals(manager.CurrentTheme, args.CurrentTheme) &&
                                      ReferenceEquals(manager.RootContext.Snapshot, manager.CurrentSnapshot);
            };
            WriteTheme(directory.Path, "green.theme.xml", "PolarGreen", "#52C41A");

            var result = await manager.ReloadThemesAsync();
            result.Status.ShouldBe(ThemeCatalogReloadStatus.Committed);
            result.AvailableThemes.ShouldBe(manager.AvailableThemes);
            manager.AvailableThemes.Select(static theme => theme.Id).ShouldBe([
                IThemeManager.DEFAULT_THEME_ID,
                "PolarGreen"
            ]);
            observed.ShouldNotBeNull();
            observedAtomicState.ShouldBeTrue();
            observed!.Generation.ShouldBeGreaterThan(0);
            observed.Generation.ShouldBe(result.Generation);
        });
    }

    [Fact]
    public async Task Equivalent_Reload_Is_A_NoOp_Without_Events()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            using var directory = new TemporaryDirectory();
            WriteTheme(directory.Path, "green.theme.xml", "PolarGreen", "#52C41A");
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);
            var events = 0;
            manager.ThemeCatalogChanged += (_, _) => events++;

            var result = await manager.ReloadThemesAsync();

            result.Status.ShouldBe(ThemeCatalogReloadStatus.NoOp);
            events.ShouldBe(0);
        });
    }

    [Fact]
    public async Task Invalid_Reload_Preserves_The_Previous_Catalog_Theme_And_Snapshots()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            using var directory = new TemporaryDirectory();
            WriteTheme(directory.Path, "green.theme.xml", "PolarGreen", "#52C41A");
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);
            await manager.ApplyThemeAsync(new ThemeRequest(
                "PolarGreen",
                null,
                ThemeTransitionReason.UserRequest));
            var themes = manager.AvailableThemes;
            var state = manager.CurrentTheme;
            var root = manager.CurrentSnapshot;
            File.WriteAllText(System.IO.Path.Combine(directory.Path, "green.theme.xml"), "<Theme>");

            var result = await manager.ReloadThemesAsync();

            result.Status.ShouldBe(ThemeCatalogReloadStatus.Failed);
            manager.AvailableThemes.ShouldBeSameAs(themes);
            manager.CurrentTheme.ShouldBeSameAs(state);
            manager.CurrentSnapshot.ShouldBeSameAs(root);
            manager.RootContext.Snapshot.ShouldBeSameAs(root);
            manager.ThemeCatalogDiagnostics.ShouldContain(static diagnostic =>
                diagnostic.Severity == ThemeDiagnosticSeverity.Error);
        });
    }

    [Fact]
    public async Task Reload_Recompiles_The_Current_Theme_When_Its_Definition_Changes()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            using var directory = new TemporaryDirectory();
            WriteTheme(directory.Path, "custom.theme.xml", "Custom", "#52C41A");
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);
            await manager.ApplyThemeAsync(new ThemeRequest(
                "Custom",
                null,
                ThemeTransitionReason.UserRequest));
            WriteTheme(directory.Path, "custom.theme.xml", "Custom", "#FA8C16");

            var result = await manager.ReloadThemesAsync();

            result.Status.ShouldBe(ThemeCatalogReloadStatus.Committed);
            manager.CurrentTheme!.ThemeId.ShouldBe("Custom");
            manager.CurrentSnapshot!.Global<Color>(nameof(DesignToken.ColorPrimary))
                   .ShouldBe(Color.Parse("#FA8C16"));
        });
    }

    [Fact]
    public async Task Deleting_The_Current_User_Theme_Falls_Back_To_Default_And_Preserves_Runtime_Config()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            using var directory = new TemporaryDirectory();
            var path = WriteTheme(directory.Path, "custom.theme.xml", "Custom", "#52C41A");
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);
            var config = new ThemeConfigBuilder()
                         .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Compact, ThemeAlgorithm.Dark)
                         .WithToken(nameof(DesignToken.EnableMotion), "false")
                         .WithToken(nameof(DesignToken.EnableWaveSpirit), "false")
                         .Build();
            await manager.ApplyThemeAsync(new ThemeRequest(
                "Custom",
                config,
                ThemeTransitionReason.UserRequest));
            File.Delete(path);

            var result = await manager.ReloadThemesAsync();

            result.Status.ShouldBe(ThemeCatalogReloadStatus.Committed);
            manager.CurrentTheme!.ThemeId.ShouldBe(IThemeManager.DEFAULT_THEME_ID);
            manager.CurrentTheme.Algorithms.ShouldBe([
                ThemeAlgorithm.Default,
                ThemeAlgorithm.Compact,
                ThemeAlgorithm.Dark
            ]);
            var snapshot = manager.CurrentSnapshot.ShouldNotBeNull();
            snapshot.Global<bool>(nameof(DesignToken.EnableMotion)).ShouldBeFalse();
            snapshot.Global<bool>(nameof(DesignToken.EnableWaveSpirit)).ShouldBeFalse();
            manager.CurrentTheme.Appearance.ShouldBe(ThemeAppearance.Dark);
        });
    }

    [Fact]
    public async Task New_Theme_Request_Supersedes_A_Reload_Before_Commit()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var resolver = new BlockingReloadResolver();
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeDefinitionResolver(resolver);
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);

            var reloadTask = manager.ReloadThemesAsync(TestContext.Current.CancellationToken);
            await resolver.Started.Task.WaitAsync(TestContext.Current.CancellationToken);
            var config = new ThemeConfigBuilder()
                         .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Compact)
                         .Build();
            var themeTask = manager.ApplyThemeAsync(
                new ThemeRequest(
                    IThemeManager.DEFAULT_THEME_ID,
                    config,
                    ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);
            resolver.Release.SetResult();

            (await reloadTask).Status.ShouldBe(ThemeCatalogReloadStatus.Superseded);
            (await themeTask).Status.ShouldBe(ThemeTransitionStatus.Committed);
            manager.CurrentTheme!.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Compact]);
        });
    }

    [Fact]
    public async Task Catalog_Observer_Failure_Is_A_Publish_Warning_After_Commit()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            using var directory = new TemporaryDirectory();
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);
            var laterObserverRan = false;
            manager.ThemeCatalogChanged += (_, _) => throw new InvalidOperationException("observer failure");
            manager.ThemeCatalogChanged += (_, _) => laterObserverRan = true;
            WriteTheme(directory.Path, "green.theme.xml", "PolarGreen", "#52C41A");

            var result = await manager.ReloadThemesAsync(TestContext.Current.CancellationToken);

            result.Status.ShouldBe(ThemeCatalogReloadStatus.Committed);
            laterObserverRan.ShouldBeTrue();
            result.PublishDiagnostics.ShouldContain(static diagnostic =>
                diagnostic.Severity == ThemeDiagnosticSeverity.Warning &&
                diagnostic.Message.Contains("observer failure", StringComparison.Ordinal));
            manager.AvailableThemes.ShouldContain(static theme => theme.Id == "PolarGreen");
        });
    }

    [Fact]
    public async Task Catalog_Event_Observes_Root_And_Local_Snapshots_From_The_New_Generation()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            using var directory = new TemporaryDirectory();
            WriteTheme(directory.Path, "custom.theme.xml", "Custom", "#52C41A");
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);
            await manager.ApplyThemeAsync(
                new ThemeRequest("Custom", null, ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);
            var provider = new ThemeConfigProvider
            {
                Config = new ThemeConfigBuilder()
                         .WithToken(nameof(DesignToken.BorderRadius), "12")
                         .Build(),
                Child = new Border()
            };
            using var root = new TestRoot();
            root.SetValue(ThemeScope.ContextProperty, manager.RootContext);
            root.Child = provider;
            var context = provider.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
            var oldLocalSnapshot = context.Snapshot;
            var observed = false;
            manager.ThemeCatalogChanged += (_, _) =>
            {
                observed = true;
                context.Snapshot.ShouldNotBeSameAs(oldLocalSnapshot);
                context.Snapshot.Global<Color>(nameof(DesignToken.ColorPrimary))
                       .ShouldBe(Color.Parse("#FA8C16"));
                context.Snapshot.Global<CornerRadius>(nameof(DesignToken.BorderRadius))
                       .ShouldBe(new CornerRadius(12));
                manager.RootContext.Snapshot.ShouldBeSameAs(manager.CurrentSnapshot);
            };
            WriteTheme(directory.Path, "custom.theme.xml", "Custom", "#FA8C16");

            var result = await manager.ReloadThemesAsync(TestContext.Current.CancellationToken);

            result.Status.ShouldBe(ThemeCatalogReloadStatus.Committed);
            observed.ShouldBeTrue();
        });
    }

    private static string WriteTheme(
        string directory,
        string fileName,
        string id,
        string colorPrimary)
    {
        var path = System.IO.Path.Combine(directory, fileName);
        File.WriteAllText(path, $$"""
                                <Theme xmlns="https://atomui.net/schemas/theme/v1"
                                       Id="{{id}}"
                                       Name="{{id}}"
                                       Appearance="Light">
                                  <Algorithms><Algorithm Id="Default" /></Algorithms>
                                  <Tokens><Token Name="ColorPrimary" Value="{{colorPrimary}}" /></Tokens>
                                </Theme>
                                """);
        return path;
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        internal TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"atomui-theme-reload-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }

    private sealed class BlockingReloadResolver : IThemeDefinitionResolver
    {
        internal TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Release { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public string Id => "BlockingReload";
        public bool SupportsReload => true;

        public ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context)
        {
            if (context.IsReload)
            {
                Started.TrySetResult();
                Release.Task.GetAwaiter().GetResult();
            }
            return new ThemeDefinitionResolveResult([], []);
        }
    }

    private sealed class TestRoot : Decorator, ILogicalRoot, IDisposable
    {
        public void Dispose()
        {
            Child = null;
            ClearValue(ThemeScope.ContextProperty);
        }
    }
}
