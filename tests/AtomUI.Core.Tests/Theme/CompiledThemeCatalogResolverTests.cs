using System.Text;
using AtomUI.Theme;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class CompiledThemeCatalogResolverTests
{
    [Fact]
    public async Task Manager_Merges_Resolvers_In_Registration_And_Source_Order()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var green = new MemorySource(
                "memory://green",
                "7",
                ThemeXml("PolarGreen", "Polar Green", "#52C41A"));
            var orange = new MemorySource(
                "memory://orange",
                "3",
                ThemeXml("SunsetOrange", "Sunset Orange", "#FA8C16"));
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeDefinitionResolver(new MemoryResolver("Gallery", green, orange));
            var manager = builder.Build();

            manager.InitializeApplication(Application.Current!);

            manager.AvailableThemes.Select(static theme => theme.Id).ShouldBe([
                IThemeManager.DEFAULT_THEME_ID,
                "PolarGreen",
                "SunsetOrange"
            ]);
            manager.AvailableThemes
                   .Single(static theme => theme.Id == "PolarGreen")
                   .AccentColor.ShouldBe(Color.Parse("#52C41A"));
            green.OpenCount.ShouldBe(1);
            orange.OpenCount.ShouldBe(1);
            manager.CompiledThemeCatalog.Get("PolarGreen").Revision.ShouldBe(
                new ThemeDefinitionRevision("memory://green", "7", green.ContentDigest));

            var result = await manager.ApplyThemeAsync(
                new ThemeRequest("PolarGreen", null, ThemeTransitionReason.UserRequest));

            result.Status.ShouldBe(ThemeTransitionStatus.Committed);
            manager.CurrentTheme!.ThemeId.ShouldBe("PolarGreen");
            manager.CurrentSnapshot!.Global<Color>(nameof(DesignToken.ColorPrimary))
                   .ShouldBe(Color.Parse("#52C41A"));
        });
    }

    [Fact]
    public async Task Reload_Reuses_A_Successfully_Read_Definition_Revision()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var source = new MemorySource(
                "memory://reload-cache",
                "1",
                ThemeXml("ReloadCache", "Reload Cache", "#52C41A"));
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeDefinitionResolver(new ReloadableMemoryResolver(source));
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);

            source.OpenCount.ShouldBe(1);
            var result = await manager.ReloadThemesAsync(TestContext.Current.CancellationToken);

            result.Status.ShouldBe(ThemeCatalogReloadStatus.NoOp);
            source.OpenCount.ShouldBe(2);
        });
    }

    [Fact]
    public void Definition_Cache_Reuses_Reads_And_Keys_Bindings_By_Registry_Revision()
    {
        var source = new MemorySource(
            "memory://registry-cache",
            "1",
            ThemeXml("RegistryCache", "Registry Cache", "#52C41A", isDefault: true));
        var resolver = new MemoryResolver("RegistryCache", source);
        var context = new ThemeDefinitionResolveContext("Tests", string.Empty, false, 0);
        var firstRegistry = TypedThemeSnapshotCacheTests.CreateRegistry();
        var secondRegistry = TypedThemeSnapshotCacheTests.CreateRegistry(
            [ThemeCompilerTests.CreateCompilerButtonDescriptor()]);
        using var cache = new ThemeDefinitionLoadCache();

        var first = CompiledThemeCatalog.LoadInitial(
            firstRegistry,
            [resolver],
            context,
            cache);
        var sameRegistry = CompiledThemeCatalog.LoadInitial(
            firstRegistry,
            [resolver],
            context,
            cache);
        var nextRegistry = CompiledThemeCatalog.LoadInitial(
            secondRegistry,
            [resolver],
            context,
            cache);

        first.Success.ShouldBeTrue();
        sameRegistry.Success.ShouldBeTrue();
        nextRegistry.Success.ShouldBeTrue();
        source.OpenCount.ShouldBe(3);
        sameRegistry.Catalog!.Get("RegistryCache").Definition
                    .ShouldBeSameAs(first.Catalog!.Get("RegistryCache").Definition);
        nextRegistry.Catalog!.Get("RegistryCache").Definition
                    .ShouldNotBeSameAs(first.Catalog.Get("RegistryCache").Definition);
    }

    [Fact]
    public void Definition_Cache_Rebinds_When_Source_Content_Changes_With_The_Same_Revision()
    {
        var source = new MutableMemorySource(
            "memory://mutable-cache",
            "1",
            ThemeXml("MutableCache", "Mutable Cache", "#52C41A", isDefault: true));
        var resolver = new MutableMemoryResolver(source);
        var context = new ThemeDefinitionResolveContext("Tests", string.Empty, false, 0);
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry();
        using var cache = new ThemeDefinitionLoadCache();

        var first = CompiledThemeCatalog.LoadInitial(
            registry,
            [resolver],
            context,
            cache);
        source.Replace(ThemeXml("MutableCache", "Mutable Cache", "#FA8C16", isDefault: true));
        var second = CompiledThemeCatalog.LoadInitial(
            registry,
            [resolver],
            context,
            cache);

        first.Success.ShouldBeTrue();
        second.Success.ShouldBeTrue();
        source.OpenCount.ShouldBe(2);
        second.Catalog!.Get("MutableCache").Revision.ContentDigest
              .ShouldNotBe(first.Catalog!.Get("MutableCache").Revision.ContentDigest);
        second.Catalog.Get("MutableCache").Definition
              .ShouldNotBeSameAs(first.Catalog.Get("MutableCache").Definition);
        second.Catalog.Get("MutableCache").Definition.Tokens
              .Single(static token => token.Descriptor.Name == nameof(DesignToken.ColorPrimary))
              .Value
              .ShouldBe(Color.Parse("#FA8C16"));
    }

    [Fact]
    public void Definition_Cache_Reuses_The_Parsed_Document_When_Only_Source_Revision_Changes()
    {
        var source = new MutableMemorySource(
            "memory://revision-only-cache",
            "1",
            ThemeXml("RevisionOnlyCache", "Revision Only Cache", "#52C41A", isDefault: true));
        var resolver = new MutableMemoryResolver(source);
        var context = new ThemeDefinitionResolveContext("Tests", string.Empty, false, 0);
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry();
        using var cache = new ThemeDefinitionLoadCache();

        var first = CompiledThemeCatalog.LoadInitial(registry, [resolver], context, cache);
        var digest = first.Catalog!.Get("RevisionOnlyCache").Revision.ContentDigest;
        cache.TryGetRead(
                 new ThemeSourceCacheKey(source.SourceIdentity, digest),
                 out var firstRead)
             .ShouldBeTrue();
        source.SetRevision("2");

        var second = CompiledThemeCatalog.LoadInitial(registry, [resolver], context, cache);

        second.Success.ShouldBeTrue();
        source.OpenCount.ShouldBe(2);
        second.Catalog!.Get("RevisionOnlyCache").Revision.SourceRevision.ShouldBe("2");
        cache.TryGetRead(
                 new ThemeSourceCacheKey(source.SourceIdentity, digest),
                 out var secondRead)
             .ShouldBeTrue();
        secondRead.ShouldBeSameAs(firstRead);
    }

    [Fact]
    public void Duplicate_Theme_Id_Across_Static_Resolvers_Fails_Startup()
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeDefinitionResolver(new MemoryResolver(
                "Gallery",
                new MemorySource(
                    "memory://duplicate",
                    "1",
                    ThemeXml(IThemeManager.DEFAULT_THEME_ID, "Duplicate", "#52C41A"))));
            var manager = builder.Build();

            var exception = Should.Throw<ThemeLoadException>(() =>
                manager.InitializeApplication(Application.Current!));

            exception.Message.ShouldContain("DaybreakBlue");
            exception.Message.ToLowerInvariant().ShouldContain("duplicate");
        });
    }

    [Fact]
    public void Second_Default_Theme_In_A_Static_Resolver_Fails_Startup()
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeDefinitionResolver(new MemoryResolver(
                "Gallery",
                new MemorySource(
                    "memory://second-default",
                    "1",
                    ThemeXml("OtherDefault", "Other Default", "#52C41A", isDefault: true))));
            var manager = builder.Build();

            var exception = Should.Throw<ThemeLoadException>(() =>
                manager.InitializeApplication(Application.Current!));

            exception.Message.ToLowerInvariant().ShouldContain("default");
        });
    }

    [Fact]
    public void Invalid_Static_Source_Fails_Startup_Without_Opening_Other_Sources_Twice()
    {
        HeadlessTestApp.Run(() =>
        {
            var valid = new MemorySource(
                "memory://valid",
                "1",
                ThemeXml("Valid", "Valid", "#52C41A"));
            var invalid = new MemorySource("memory://invalid", "1", "<Theme>");
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeDefinitionResolver(new MemoryResolver("Gallery", valid, invalid));
            var manager = builder.Build();

            Should.Throw<ThemeLoadException>(() =>
                manager.InitializeApplication(Application.Current!));

            valid.OpenCount.ShouldBe(1);
            invalid.OpenCount.ShouldBe(1);
        });
    }

    [Fact]
    public void Successful_Resolver_Warnings_Are_Preserved_In_Catalog_Diagnostics()
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeDefinitionResolver(new WarningResolver(
                new MemorySource(
                    "memory://warning-theme",
                    "1",
                    ThemeXml("WarningTheme", "Warning Theme", "#52C41A"))));
            var manager = builder.Build();

            manager.InitializeApplication(Application.Current!);

            manager.ThemeCatalogDiagnostics.ShouldContain(static diagnostic =>
                diagnostic.Code == "TEST-WARNING" &&
                diagnostic.Severity == ThemeDiagnosticSeverity.Warning);
        });
    }

    [Fact]
    public void Theme_Without_Explicit_ColorPrimary_Has_No_Accent_Color()
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeDefinitionResolver(new MemoryResolver(
                "NoAccent",
                new MemorySource(
                    "memory://no-accent",
                    "1",
                    ThemeXmlWithoutTokens("NoAccent", "No Accent"))));
            var manager = builder.Build();

            manager.InitializeApplication(Application.Current!);

            manager.AvailableThemes
                   .Single(static theme => theme.Id == "NoAccent")
                   .AccentColor.ShouldBeNull();
        });
    }

    private static string ThemeXml(
        string id,
        string name,
        string colorPrimary,
        bool isDefault = false)
    {
        var defaultAttribute = isDefault ? " IsDefault=\"true\"" : string.Empty;
        return $$"""
                 <Theme xmlns="https://atomui.net/schemas/theme/v1"
                        Id="{{id}}"
                        Name="{{name}}"
                        Appearance="Light"{{defaultAttribute}}>
                   <Algorithms><Algorithm Id="Default" /></Algorithms>
                   <Tokens><Token Name="ColorPrimary" Value="{{colorPrimary}}" /></Tokens>
                 </Theme>
                 """;
    }

    private static string ThemeXmlWithoutTokens(string id, string name)
    {
        return $$"""
                 <Theme xmlns="https://atomui.net/schemas/theme/v1"
                        Id="{{id}}"
                        Name="{{name}}"
                        Appearance="Light">
                   <Algorithms><Algorithm Id="Default" /></Algorithms>
                 </Theme>
                 """;
    }

    private sealed class MemoryResolver(
        string id,
        params MemorySource[] sources) : IThemeDefinitionResolver
    {
        public string Id { get; } = id;
        public bool SupportsReload => false;

        public ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context)
        {
            return new ThemeDefinitionResolveResult(sources, []);
        }
    }

    private sealed class WarningResolver(MemorySource source) : IThemeDefinitionResolver
    {
        public string Id => "WarningResolver";
        public bool SupportsReload => false;

        public ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context)
        {
            return new ThemeDefinitionResolveResult(
                [source],
                [new ThemeDiagnostic(
                    "TEST-WARNING",
                    ThemeDiagnosticSeverity.Warning,
                    Id,
                    "$",
                    "warning")]);
        }
    }

    private sealed class ReloadableMemoryResolver(MemorySource source) : IThemeDefinitionResolver
    {
        public string Id => "ReloadableMemory";
        public bool SupportsReload => true;

        public ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context)
        {
            return new ThemeDefinitionResolveResult([source], []);
        }
    }

    private sealed class MutableMemoryResolver(MutableMemorySource source) : IThemeDefinitionResolver
    {
        public string Id => "MutableMemory";
        public bool SupportsReload => false;

        public ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context)
        {
            return new ThemeDefinitionResolveResult([source], []);
        }
    }

    private sealed class MemorySource : IThemeDefinitionSource
    {
        private readonly byte[] _bytes;

        internal MemorySource(
            string identity,
            string revision,
            string xml)
        {
            SourceIdentity = identity;
            SourceRevision = revision;
            _bytes = Encoding.UTF8.GetBytes(xml);
            ContentDigest = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(_bytes));
        }

        public string SourceIdentity { get; }
        public string SourceRevision { get; }
        internal string ContentDigest { get; }
        internal int OpenCount { get; private set; }

        public Stream OpenRead()
        {
            OpenCount++;
            return new MemoryStream(_bytes, writable: false);
        }
    }

    private sealed class MutableMemorySource : IThemeDefinitionSource
    {
        private byte[] _bytes = [];

        internal MutableMemorySource(
            string identity,
            string revision,
            string xml)
        {
            SourceIdentity = identity;
            SourceRevision = revision;
            Replace(xml);
        }

        public string SourceIdentity { get; }
        public string SourceRevision { get; private set; }
        internal int OpenCount { get; private set; }

        internal void Replace(string xml)
        {
            _bytes = Encoding.UTF8.GetBytes(xml);
        }

        internal void SetRevision(string revision)
        {
            SourceRevision = revision;
        }

        public Stream OpenRead()
        {
            OpenCount++;
            return new MemoryStream(_bytes, writable: false);
        }
    }
}
