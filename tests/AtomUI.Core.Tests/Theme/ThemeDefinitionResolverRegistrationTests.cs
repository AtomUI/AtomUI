using AtomUI.Theme;
using AtomUI.Theme.Definitions;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeDefinitionResolverRegistrationTests
{
    [Fact]
    public void Builder_Uses_The_Concrete_Application_Assembly_As_The_Default_Id()
    {
        var builder = new ThemeManagerBuilder(typeof(TestApplication));

        builder.ApplicationId.ShouldBe(typeof(TestApplication).Assembly.GetName().Name);
    }

    [Fact]
    public void Explicit_Application_Id_Overrides_The_Default()
    {
        var builder = new ThemeManagerBuilder(typeof(TestApplication));

        builder.WithApplicationId("AtomUIGallery");

        builder.ApplicationId.ShouldBe("AtomUIGallery");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("Gallery/Themes")]
    [InlineData("Gallery\\Themes")]
    [InlineData("Gallery Themes")]
    [InlineData("画廊")]
    public void Builder_Rejects_Application_Ids_That_Are_Not_Safe_Path_Segments(string id)
    {
        var builder = new ThemeManagerBuilder(typeof(TestApplication));

        Should.Throw<ArgumentException>(() => builder.WithApplicationId(id));
    }

    [Fact]
    public void Builder_Rejects_Empty_And_Duplicate_Resolver_Ids()
    {
        var builder = new ThemeManagerBuilder(typeof(TestApplication));
        builder.AddThemeDefinitionResolver(new StubResolver("Gallery"));

        Should.Throw<ArgumentException>(() =>
            builder.AddThemeDefinitionResolver(new StubResolver(" ")));
        Should.Throw<ThemeResourceRegisterException>(() =>
            builder.AddThemeDefinitionResolver(new StubResolver("Gallery")));
    }

    [Fact]
    public void Resolve_Result_Defensively_Copies_Sources_And_Diagnostics()
    {
        var sources = new List<IThemeDefinitionSource>
        {
            new StubSource("theme-a")
        };
        var diagnostics = new List<ThemeDiagnostic>
        {
            new("TEST001", ThemeDiagnosticSeverity.Warning, "test", "$", "warning")
        };

        var result = new ThemeDefinitionResolveResult(sources, diagnostics);
        sources.Clear();
        diagnostics.Clear();

        result.Sources.Count.ShouldBe(1);
        result.Diagnostics.Count.ShouldBe(1);
        result.Sources.ShouldNotBeSameAs(sources);
        result.Diagnostics.ShouldNotBeSameAs(diagnostics);
    }

    private sealed class StubResolver(string id) : IThemeDefinitionResolver
    {
        public string Id { get; } = id;
        public bool SupportsReload => false;

        public ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context)
        {
            return new ThemeDefinitionResolveResult([], []);
        }
    }

    private sealed class StubSource(string identity) : IThemeDefinitionSource
    {
        public string SourceIdentity { get; } = identity;
        public string SourceRevision => "1";

        public Stream OpenRead() => new MemoryStream([]);
    }
}
