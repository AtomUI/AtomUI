using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeAlgorithmRegistrationTests
{
    [Fact]
    public void Builder_Algorithms_Reach_The_Startup_Registry()
    {
        HeadlessTestApp.Run(() =>
        {
            var descriptor = Descriptor("Test", "Flat");
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeAlgorithms([descriptor]);
            var manager = builder.Build();

            manager.InitializeApplication(Application.Current!);

            manager.CurrentSnapshot!.Registry.TryGetAlgorithm(new ThemeAlgorithm("Test", "Flat"), out var found)
                   .ShouldBeTrue();
            found.ShouldBeSameAs(descriptor);
        });
    }

    [Fact]
    public void Builder_Algorithms_Reach_The_Global_Only_Registry_By_Reference()
    {
        HeadlessTestApp.Run(() =>
        {
            var descriptor = Descriptor("Test", "Flat");
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeAlgorithms([descriptor]);

            var inputs = builder.CaptureStartupInputs();
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);
            var fullRegistry = manager.CurrentSnapshot!.Registry;

            inputs.Algorithms.ShouldContain(algorithm => ReferenceEquals(algorithm, descriptor));
            fullRegistry.ExtendsGlobalSchemaOf(ThemeSchemaRegistry.CreateGlobalOnly(inputs.Algorithms)).ShouldBeTrue();
        });
    }

    [Fact]
    public void Duplicate_Algorithm_Identity_Across_Packages_Is_Rejected()
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeAlgorithms([Descriptor("Test", "Flat")]);
            builder.AddThemeAlgorithms([Descriptor("Test", "Flat")]);

            Should.Throw<ThemeSchemaException>(() => builder.Build())
                  .Message.ShouldContain("Duplicate algorithm identity 'Test:Flat'");
        });
    }

    [Fact]
    public async Task A_Theme_Using_A_Package_Algorithm_Compiles_And_Passes_It_The_Seed()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            using var directory = new TemporaryDirectory();
            WriteTheme(directory.Path, "flat.theme.xml", "Flat", "Test", "Flat");
            var seeds = new List<Color>();
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeAlgorithms([Descriptor("Test", "Flat", seeds)]);
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);

            var result = await manager.ApplyThemeAsync(new ThemeRequest(
                "Flat",
                null,
                ThemeTransitionReason.UserRequest));

            result.Status.ShouldBe(ThemeTransitionStatus.Committed);
            seeds.ShouldNotBeEmpty();
            seeds.ShouldAllBe(seed => seed == Color.Parse("#52C41A"));
            manager.CurrentSnapshot!.Global<CornerRadius>(nameof(DesignToken.BorderRadiusLG))
                   .ShouldBe(new CornerRadius(0));
        });
    }

    [Fact]
    public void A_User_Directory_Theme_Xml_Can_Reference_A_Registered_Package_Algorithm()
    {
        HeadlessTestApp.Run(() =>
        {
            using var directory = new TemporaryDirectory();
            WriteTheme(directory.Path, "flat.theme.xml", "Flat", "Test", "Flat");
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeAlgorithms([Descriptor("Test", "Flat")]);
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();

            manager.InitializeApplication(Application.Current!);

            manager.AvailableThemes.Select(static theme => theme.Id).ShouldBe([
                IThemeManager.DEFAULT_THEME_ID,
                "Flat"
            ]);
            manager.ThemeCatalogDiagnostics.ShouldNotContain(static diagnostic =>
                diagnostic.Severity == ThemeDiagnosticSeverity.Error);
        });
    }

    [Fact]
    public void A_User_Directory_Theme_Xml_Referencing_An_Unregistered_Algorithm_Is_Reported_And_The_App_Still_Starts()
    {
        HeadlessTestApp.Run(() =>
        {
            using var directory = new TemporaryDirectory();
            WriteTheme(directory.Path, "missing.theme.xml", "Missing", "Test", "Missing");
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeAlgorithms([Descriptor("Test", "Flat")]);
            builder.UseUserThemeDirectory(directory.Path);
            var manager = builder.Build();

            manager.InitializeApplication(Application.Current!);

            manager.AvailableThemes.Select(static theme => theme.Id).ShouldBe([
                IThemeManager.DEFAULT_THEME_ID
            ]);
            manager.ThemeCatalogDiagnostics.ShouldContain(static diagnostic =>
                diagnostic.Code == "ATMTHM2001" && diagnostic.Message.Contains("Test:Missing"));
            manager.CurrentTheme!.ThemeId.ShouldBe(IThemeManager.DEFAULT_THEME_ID);
        });
    }

    private static ThemeAlgorithmDescriptor Descriptor(string catalog, string id, List<Color>? seeds = null)
    {
        return new ThemeAlgorithmDescriptor(
            new ThemeAlgorithm(catalog, id),
            1,
            ThemeAppearanceEffect.Preserve,
            () => new FlatAlgorithm(seeds));
    }

    private static void WriteTheme(string directory, string fileName, string id, string catalog, string algorithmId)
    {
        File.WriteAllText(System.IO.Path.Combine(directory, fileName), $$"""
            <Theme xmlns="https://atomui.net/schemas/theme/v1"
                   Id="{{id}}"
                   Name="{{id}}"
                   Appearance="Light">
              <Algorithms>
                <Algorithm Id="Default" />
                <Algorithm Catalog="{{catalog}}" Id="{{algorithmId}}" />
              </Algorithms>
              <Tokens><Token Name="ColorPrimary" Value="#52C41A" /></Tokens>
            </Theme>
            """);
    }

    private sealed class FlatAlgorithm(List<Color>? seeds) : IThemeAlgorithm
    {
        public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
        {
            seeds?.Add(effectiveSeed.ColorPrimary);
            nextMap.BorderRadiusLG = new CornerRadius(0);
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        internal TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"atomui-theme-algorithm-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
