using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Registration.TypeMapRegistrationGeneratorTests;
using static AtomUI.Generator.Tests.Registration.PlatformAvailabilityTests;

namespace AtomUI.Generator.Tests.Registration;

public class ResourcePlatformAvailabilityTests
{
    private const string Controls = "namespace Demo { public class Button : Avalonia.Controls.Control { } }";
    private const string Resource = "namespace Demo { [System.Runtime.Versioning.UnsupportedOSPlatform(\"browser\")] internal class ChromeTheme : Avalonia.Controls.ResourceDictionary { } }";
    private const string Export = "<ControlTheme x:Key=\"Chrome\" TargetType=\"local:Button\" />";

    [Theory]
    [InlineData("Styles", "Avalonia.Styling.Styles")]
    [InlineData("UserControl", "Avalonia.Controls.UserControl")]
    public void Other_Valid_AXAML_Roots_Are_Not_Registration_Resources(string element, string baseClass)
    {
        var asset = new TextFile("Themes/Accent.axaml", $"""
            <{element} xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Class="Demo.Accent" />
            """);
        var (package, diagnostics) = Build("namespace Demo { internal class Accent : " + baseClass + " { } }", asset);
        diagnostics.ShouldBeEmpty();
        package.Assets.ShouldBeEmpty();
        package.Controls.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("Chrome/Themes/ChromeTheme.axaml")]
    [InlineData("Moved/Themes/RenamedTheme.axaml")]
    public void Resource_Restriction_Does_Not_Restrict_Target_Or_Neighboring_Asset(string path)
    {
        var restricted = Dictionary(path, "Demo.ChromeTheme", Export);
        var portable = Dictionary("Chrome/Themes/PortableTheme.axaml", null, Export.Replace("Chrome", "Portable"));
        var (package, diagnostics) = Build(Controls + Resource, restricted, portable);
        diagnostics.ShouldBeEmpty();
        EvaluateGuards(package.Assets.Single(a => a.Uri.EndsWith(path)).Guards, "browser").ShouldBeFalse();
        EvaluateGuards(package.Assets.Single(a => a.Uri.EndsWith(path)).Guards, "windows").ShouldBeTrue();
        EvaluateGuards(package.Assets.Single(a => a.Uri.EndsWith("PortableTheme.axaml")).Guards, "browser").ShouldBeTrue();
        EvaluateGuards(package.Controls.ShouldHaveSingleItem().Guards, "browser").ShouldBeTrue();
    }

    [Fact]
    public void Untyped_Resource_Uses_Owning_Assembly_Domain_For_External_Target()
    {
        var (package, diagnostics) = Build("[assembly: System.Runtime.Versioning.UnsupportedOSPlatform(\"browser\")]",
            Dictionary("Themes/ChromeTheme.axaml", null, "<ControlTheme x:Key=\"Chrome\" TargetType=\"Button\" />"));
        diagnostics.ShouldBeEmpty();
        EvaluateGuards(package.Assets.ShouldHaveSingleItem().Guards, "browser").ShouldBeFalse();
        EvaluateGuards(package.Controls.ShouldHaveSingleItem().Guards, "browser").ShouldBeTrue();
    }

    [Theory]
    [InlineData("windows", "6.2", false)]
    [InlineData("windows", "7.0", true)]
    [InlineData("windows", "10.0", false)]
    [InlineData("linux", "7.0", false)]
    public void Resource_Class_Containing_Type_And_Assembly_Share_Normal_Version_Semantics(string os, string version, bool expected)
    {
        var (package, diagnostics) = Build("""
            [assembly: System.Runtime.Versioning.UnsupportedOSPlatform("windows10.0")]
            namespace Demo;
            public class Button : Avalonia.Controls.Control { }
            [System.Runtime.Versioning.SupportedOSPlatform("windows6.2")]
            internal class Outer {
                [System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
                internal class ChromeTheme : Avalonia.Controls.ResourceDictionary { }
            }
            """, Dictionary("Themes/ChromeTheme.axaml", "Demo.Outer+ChromeTheme", Export));
        diagnostics.ShouldBeEmpty();
        EvaluateGuards(package.Assets.ShouldHaveSingleItem().Guards, os, version).ShouldBe(expected);
    }

    [Theory]
    [InlineData("Demo.Missing")]
    [InlineData("Demo.Button")]
    [InlineData("Avalonia.Controls.ResourceDictionary")]
    [InlineData("")]
    public void Explicit_Invalid_Resource_Class_Reports_Located_Error(string resourceClass)
    {
        var (_, diagnostics) = Build(Controls, Dictionary("Themes/ChromeTheme.axaml", resourceClass, Export));
        diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error &&
            d.Location.GetLineSpan().Path == "Themes/ChromeTheme.axaml" && d.GetMessage().Contains("x:Class"));
    }

    [Fact]
    public void Resource_Restriction_Can_Make_Indivisible_Exports_Compatible()
    {
        var (package, diagnostics) = Build(Controls + Resource + """
            namespace Demo { [System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
                public class Native : Avalonia.Controls.Control { } }
            """, Dictionary("Themes/ChromeTheme.axaml", "Demo.ChromeTheme", Export + "<ControlTheme x:Key=\"Native\" TargetType=\"local:Native\" />"));
        diagnostics.ShouldBeEmpty();
        EvaluateGuards(package.Assets.ShouldHaveSingleItem().Guards, "browser").ShouldBeFalse();
    }

    [Fact]
    public void Resource_Class_Identity_Cannot_Bind_To_A_Referenced_Assembly()
    {
        var foreign = Compilation(Resource, [], "Foreign");
        using var stream = new MemoryStream();
        foreign.Emit(stream, cancellationToken: TestContext.Current.CancellationToken).Success.ShouldBeTrue();
        var asset = Dictionary("Themes/ChromeTheme.axaml", "Demo.ChromeTheme", Export);
        var compilation = Compilation(Controls, [asset]).AddReferences(MetadataReference.CreateFromImage(stream.ToArray()));
        var (_, diagnostics) = Build(compilation, asset);
        diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error && d.GetMessage().Contains("Demo.ChromeTheme"));
    }

    [Fact]
    public void Local_Resource_Class_Wins_Over_An_Identically_Named_Reference()
    {
        using var stream = new MemoryStream();
        Compilation(Resource.Replace("internal class", "public class"), [], "Foreign").Emit(stream, cancellationToken: TestContext.Current.CancellationToken).Success.ShouldBeTrue();
        var asset = Dictionary("Themes/ChromeTheme.axaml", "Demo.ChromeTheme", Export);
        var compilation = Compilation(Controls + "namespace Demo { internal class ChromeTheme : Avalonia.Controls.ResourceDictionary { } }", [asset])
            .AddReferences(MetadataReference.CreateFromImage(stream.ToArray()));
        var (package, diagnostics) = Build(compilation, asset);
        diagnostics.ShouldBeEmpty();
        EvaluateGuards(package.Assets.ShouldHaveSingleItem().Guards, "browser").ShouldBeTrue();
    }

    [Theory]
    [InlineData("ButtonTheme", true)]
    [InlineData("SpecialButtonTheme", false)]
    public void ControlTheme_Class_Availability_Preserves_Default_Theme_Ownership(string name, bool isDefault)
    {
        var asset = new TextFile("Themes/" + name + ".axaml", """
            <ControlTheme xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:local="using:Demo" TargetType="local:Button" x:Class="Demo.THEME" />
            """.Replace("THEME", name));
        var (package, diagnostics) = Build(Controls + "namespace Demo { [System.Runtime.Versioning.UnsupportedOSPlatform(\"browser\")] internal class " + name + " : Avalonia.Styling.ControlTheme { } }", asset);
        diagnostics.ShouldBeEmpty();
        if (isDefault)
        {
            EvaluateGuards(package.Assets.ShouldHaveSingleItem().Guards, "browser").ShouldBeFalse();
            EvaluateGuards(package.Controls.ShouldHaveSingleItem().Guards, "browser").ShouldBeTrue();
        }
        else package.Assets.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Resource_Domain_Must_Be_Covered_By_A_Required_Token_Owner(bool restrictResource)
    {
        var source = Controls + (restrictResource ? Resource : Resource.Replace("[System.Runtime.Versioning.UnsupportedOSPlatform(\"browser\")]", "")) + """
            namespace Demo {
                [System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
                public class Native : Avalonia.Controls.Control { }
                [AtomUI.Theme.DesignTokens.ControlDesignToken]
                internal sealed class NativeToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken {
                    public double Height { get; set; }
                    public override void CalculateTokenValues(bool dark) { }
                }
            }
            """;
        var result = Run(source, Dictionary("Themes/ChromeTheme.axaml", "Demo.ChromeTheme", """
            <ControlTheme x:Key="Chrome" TargetType="local:Button" xmlns:tokens="using:Demo.DesignTokens">
                <Setter Property="Tag" Value="{tokens:NativeTokenResource Height}" />
            </ControlTheme>
            """));
        if (restrictResource) result.Diagnostics.ShouldBeEmpty();
        else result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG006" && d.GetMessage().Contains("required Token owner"));
    }

    [Fact]
    public void Default_Type_Key_Coverage_Uses_Resource_Class_Domain()
    {
        var source = Controls + Resource + "namespace Demo { public class Host : Avalonia.Controls.Control { } }";
        var provider = Dictionary("Themes/ChromeTheme.axaml", "Demo.ChromeTheme", Export.Replace("x:Key=\"Chrome\"", "x:Key=\"{x:Type local:Button}\""));
        var consumer = Asset("Host", "<ControlTheme x:Key=\"Host\" TargetType=\"local:Host\"><Setter Property=\"Tag\" Value=\"{StaticResource {x:Type local:Button}}\" /></ControlTheme>");
        var result = Run(source, provider, consumer);
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG004");
        result = Run(source.Replace("public class Host", "[System.Runtime.Versioning.UnsupportedOSPlatform(\"browser\")] public class Host"), provider, consumer);
        result.Diagnostics.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Changing_Only_Resource_Or_Assembly_Annotation_Invalidates_Incremental_Guard(bool assemblyAnnotation)
    {
        var asset = Dictionary("Themes/ChromeTheme.axaml", "Demo.ChromeTheme", Export);
        var code = assemblyAnnotation
            ? "[assembly: System.Runtime.Versioning.UnsupportedOSPlatform(\"browser\")]" + Controls + Resource.Replace("[System.Runtime.Versioning.UnsupportedOSPlatform(\"browser\")]", "")
            : Controls + Resource;
        var compilation = Compilation(code, [asset]);
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new TokenResourceKeyGenerator().AsSourceGenerator()],
            ImmutableArray.Create<AdditionalText>(asset), (CSharpParseOptions)compilation.SyntaxTrees.First().Options, new Options(),
            new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        Source(driver.GetRunResult(), "GeneratedTypeMapRegistration.g.cs").ShouldContain("IsBrowser()");
        var replacement = CSharpSyntaxTree.ParseText(code.Replace("browser", "linux"), (CSharpParseOptions)compilation.SyntaxTrees.First().Options, cancellationToken: TestContext.Current.CancellationToken);
        compilation = compilation.ReplaceSyntaxTree(compilation.SyntaxTrees.First(), replacement);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        var source = Source(driver.GetRunResult(), "GeneratedTypeMapRegistration.g.cs");
        source.ShouldContain("IsLinux()");
        source.ShouldNotContain("IsBrowser()");
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        driver.GetRunResult().Results.Single().TrackedSteps["ControlRegistrationOutput"].Single().Outputs.Single().Reason
            .ShouldBe(IncrementalStepRunReason.Cached);
    }

    private static TextFile Dictionary(string path, string? resourceClass, string content)
        => new(path, "<ResourceDictionary xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:local=\"using:Demo\"" +
            (resourceClass is null ? "" : " x:Class=\"" + resourceClass + "\"") + ">" + content + "</ResourceDictionary>");

    private static (RegistrationPackage Package, List<Diagnostic> Diagnostics) Build(string code, params TextFile[] assets)
        => Build(Compilation(code, assets), assets);

    private static (RegistrationPackage Package, List<Diagnostic> Diagnostics) Build(CSharpCompilation compilation, params TextFile[] assets)
    {
        compilation.GetDiagnostics(TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var diagnostics = new List<Diagnostic>();
        var documents = new Dictionary<string, XElement>();
        var inputs = assets.Select(a => {
            var input = ThemeAssetInfo.Create(a, null, null, TestContext.Current.CancellationToken, out var root);
            documents.Add(a.Path, root!);
            return input;
        }).ToArray();
        var builder = new RegistrationModelBuilder(compilation, "Demo.Controls", "Demo", diagnostics.Add);
        builder.ReadResources(inputs, documents, []);
        return (builder.Build([], [], []), diagnostics);
    }
}
