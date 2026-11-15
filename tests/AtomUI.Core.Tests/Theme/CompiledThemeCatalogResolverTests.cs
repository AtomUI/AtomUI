using System.Text;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
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

    [Fact]
    public void Catalog_Publishes_Design_Language_Metadata_In_ThemeInfo()
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddThemeDefinitionResolver(new MemoryResolver(
                "Meta",
                new MemorySource(
                    "memory://meta",
                    "1",
                    """
                    <Theme xmlns="https://atomui.net/schemas/theme/v1" Id="Meta" Name="Meta" Appearance="Dark"
                           DesignLanguage="PrimeNG.Aura" Density="Compact" BrandColor="Seed">
                      <Algorithms><Algorithm Id="Dark" /><Algorithm Id="Compact" /></Algorithms>
                    </Theme>
                    """)));
            var manager = builder.Build();

            manager.InitializeApplication(Application.Current!);

            var info = manager.AvailableThemes.Single(static theme => theme.Id == "Meta");
            info.DesignLanguage.ShouldBe("PrimeNG.Aura");
            info.Density.ShouldBe(ThemeDensity.Compact);
            info.BrandColor.ShouldBe(ThemeBrandColorSupport.Seed);
            info.Algorithms.ShouldBe([ThemeAlgorithm.Dark, ThemeAlgorithm.Compact]);
        });
    }

    [Fact]
    public async Task Catalog_Demotes_User_Theme_That_Duplicates_A_Package_Variant()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var manager = BuildLanguageManager(
                LanguageXml("PkgBlue", "TestLang", "Light", "Normal", "Seed"),
                LanguageXml("UserBlue", "TestLang", "Light", "Normal", "Seed"));

            var themes = manager.AvailableThemes;
            themes.Single(static theme => theme.Id == "PkgBlue").DesignLanguage.ShouldBe("TestLang");
            themes.Single(static theme => theme.Id == "UserBlue").DesignLanguage.ShouldBeNull();
            manager.ThemeCatalogDiagnostics.ShouldContain(static diagnostic =>
                diagnostic.Code == "ATMTHM3103" && diagnostic.Severity == ThemeDiagnosticSeverity.Warning);
            manager.CompiledThemeCatalog.Get("UserBlue").Info.DesignLanguage.ShouldBeNull();

            var result = await manager.ApplyThemeAsync(
                new ThemeRequest("UserBlue", null, ThemeTransitionReason.UserRequest));
            result.Status.ShouldBe(ThemeTransitionStatus.Committed);
        });
    }

    [Fact]
    public void Catalog_Lets_A_User_Theme_Add_A_Missing_Variant()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = BuildLanguageManager(
                LanguageXml("PkgBlue", "TestLang", "Light", "Normal", "Seed"),
                LanguageXml("UserDark", "TestLang", "Dark", "Normal", "Seed"));

            manager.AvailableThemes.Single(static theme => theme.Id == "UserDark")
                   .DesignLanguage.ShouldBe("TestLang");
            manager.ThemeCatalogDiagnostics.ShouldNotContain(static diagnostic =>
                diagnostic.Code == "ATMTHM3103" || diagnostic.Code == "ATMTHM3104");
        });
    }

    [Fact]
    public void Catalog_Demotes_A_Variant_With_Different_Brand_Support()
    {
        HeadlessTestApp.Run(() =>
        {
            var manager = BuildLanguageManager(
                LanguageXml("PkgBlue", "TestLang", "Light", "Normal", "Seed"),
                LanguageXml("UserDark", "TestLang", "Dark", "Normal", "Fixed"));

            manager.AvailableThemes.Single(static theme => theme.Id == "UserDark")
                   .DesignLanguage.ShouldBeNull();
            manager.ThemeCatalogDiagnostics.ShouldContain(static diagnostic =>
                diagnostic.Code == "ATMTHM3104" && diagnostic.Severity == ThemeDiagnosticSeverity.Warning);
        });
    }

    [Fact]
    public void Catalog_Demoted_Definition_Does_Not_Occupy_A_Slot_Or_Set_Brand_Support()
    {
        HeadlessTestApp.Run(() =>
        {
            // The language's first definition is Seed; the Fixed variant is demoted (3104) and must not
            // block the later Dark/Compact variant, nor may a demoted duplicate (3103) claim its slot.
            var manager = BuildLanguageManager(
                LanguageXml("PkgBlue", "TestLang", "Light", "Normal", "Seed"),
                LanguageXml("FixedDark", "TestLang", "Dark", "Normal", "Fixed"),
                LanguageXml("SeedDark", "TestLang", "Dark", "Normal", "Seed"));

            var themes = manager.AvailableThemes;
            themes.Single(static theme => theme.Id == "FixedDark").DesignLanguage.ShouldBeNull();
            themes.Single(static theme => theme.Id == "SeedDark").DesignLanguage.ShouldBe("TestLang");
        });
    }

    [Fact]
    public void Append_Registry_Keeps_Original_Slice_Info_So_A_Later_Reload_Restores_Grouping()
    {
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry();
        var context = new ThemeDefinitionResolveContext("Tests", string.Empty, false, 0);
        var staticResolver = new MemoryResolver("Static", new MemorySource(
            "memory://static", "1", LanguageXml("StaticBlue", "TestLang", "Light", "Normal", "Seed")
                .Replace("Appearance=\"Light\"", "Appearance=\"Light\" IsDefault=\"true\"")));
        var user = new MutableMemorySource(
            "memory://user", "1", LanguageXml("UserBlue", "TestLang", "Light", "Normal", "Seed"));
        var userResolver = new ReloadableMutableResolver(user);
        // The reloadable user theme is ordered first and claims the variant, so the static one is demoted.
        var initial = CompiledThemeCatalog.LoadInitial(registry, [userResolver, staticResolver], context);
        initial.Success.ShouldBeTrue();
        initial.Catalog!.Get("StaticBlue").Info.DesignLanguage.ShouldBeNull();
        initial.Catalog.Get("UserBlue").Info.DesignLanguage.ShouldBe("TestLang");

        var rebound = initial.Catalog.AppendRegistry(registry, registry, []);
        rebound.Get("StaticBlue").Info.DesignLanguage.ShouldBeNull();

        // The conflict goes away on reload (the user theme moves to another variant); the static slice is reused
        // verbatim, so it must still hold its original Info for the static theme to rejoin the group.
        user.Replace(LanguageXml("UserBlue", "TestLang", "Dark", "Normal", "Seed"));
        user.SetRevision("2");
        var reloaded = CompiledThemeCatalog.LoadReload(
            registry, rebound, [userResolver, staticResolver], context);
        reloaded.Success.ShouldBeTrue();
        reloaded.Catalog!.Get("StaticBlue").Info.DesignLanguage.ShouldBe("TestLang");
        reloaded.Catalog.Get("UserBlue").Info.DesignLanguage.ShouldBe("TestLang");
    }

    private sealed class ReloadableMutableResolver(MutableMemorySource source) : IThemeDefinitionResolver
    {
        public string Id => "ReloadableMutable";
        public bool SupportsReload => true;

        public ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context)
        {
            return new ThemeDefinitionResolveResult([source], []);
        }
    }

    private static ThemeManager BuildLanguageManager(params string[] xmls)
    {
        var builder = new ThemeManagerBuilder(Application.Current!.GetType());
        var index = 0;
        foreach (var xml in xmls)
        {
            index++;
            builder.AddThemeDefinitionResolver(new MemoryResolver(
                $"Lang{index}",
                new MemorySource($"memory://lang-{index}", "1", xml)));
        }
        var manager = builder.Build();
        manager.InitializeApplication(Application.Current!);
        return manager;
    }

    internal static string LanguageXml(
        string id,
        string language,
        string appearance,
        string density,
        string brand)
    {
        var algorithms = appearance == "Dark"
            ? "<Algorithm Id=\"Dark\" />"
            : "<Algorithm Id=\"Default\" />";
        return $$"""
                 <Theme xmlns="https://atomui.net/schemas/theme/v1" Id="{{id}}" Name="{{id}}" Appearance="{{appearance}}"
                        DesignLanguage="{{language}}" Density="{{density}}" BrandColor="{{brand}}">
                   <Algorithms>{{algorithms}}</Algorithms>
                 </Theme>
                 """;
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
