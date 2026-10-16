using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.Registration;

public class TypeMapRegistrationGeneratorTests
{
    [Fact]
    public void Emits_Resource_Only_Internal_Nested_Contracts_Without_Empty_Fragments()
    {
        var result = Run("""
            namespace Demo;
            public class Button : Avalonia.Controls.Control { }
            public class Empty : Button { }
            public class Container { internal class Presenter : Avalonia.Controls.Control { } }
            """,
            new Options(globalOptions: new Dictionary<string, string>
            {
                ["build_property.AtomUICriticalControls"] = "Button,Unused"
            }),
            Asset("Button", "<ControlTheme x:Key=\"{x:Type local:Button}\" TargetType=\"local:Button\" />"),
            Asset("Presenter", "<ControlTheme x:Key=\"{x:Type local:Container.Presenter}\" TargetType=\"local:Container.Presenter\" />"));
        var source = Source(result, "GeneratedTypeMapRegistration.g.cs");
        source.ShouldContain("typeof(global::Demo.Container.Presenter)");
        source.ShouldNotContain("typeof(global::Demo.Empty)");
        source.ShouldContain("ControlPackageMarker(");
        source.ShouldContain("GeneratedTypeMapAccessor(typeof(");
        source.ShouldContain("GetOrCreateExternalTypeMapping<");
        source.ShouldContain("ControlRegistrationRuntime.CollectFragments(builder, GetSelectionMap(), CandidateKeys)");
        source.ShouldNotContain("CollectCritical");
        source.ShouldNotContain("CollectDeferred");
        source.ShouldNotContain("UnitId");
        source.ShouldNotContain("AotTrim");
        AssertCompiles(result);
    }

    [Fact]
    public void Token_Only_Package_Has_All_Unique_Triggers_And_Typed_Keys()
    {
        var result = Run("""
            namespace Demo;
            public class Button : Avalonia.Controls.Control { }
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            internal sealed class ButtonToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public double Height { get; set; }
                public override void CalculateTokenValues(bool dark) { }
            }
            """);
        var source = Source(result, "GeneratedTypeMapRegistration.g.cs");
        source.ShouldContain("typeof(global::Demo.Button)");
        source.ShouldContain("typeof(global::Demo.DesignTokens.ButtonTokenResourceExtension)");
        source.ShouldContain("typeof(global::Demo.DesignTokens.ButtonTokenKey)");
        source.ShouldContain("typeof(global::Demo.DesignTokens.ButtonTokenKind)");
        var mapLines = source.Split('\n').Where(line => line.StartsWith("[assembly: global::System.Runtime.InteropServices.TypeMap<", StringComparison.Ordinal)).ToArray();
        mapLines.Length.ShouldBe(4);
        mapLines.Select(line => line.Substring(line.IndexOf("(\"", StringComparison.Ordinal), line.IndexOf(", typeof(", StringComparison.Ordinal) - line.IndexOf("(\"", StringComparison.Ordinal))).Distinct().Count().ShouldBe(4);
        Source(result, "GeneratedThemeSchema.g.cs").ShouldNotContain("static readonly ControlTokenDescriptor[]");
        source.ShouldNotContain("static readonly global::System.Type[]");
        Source(result, "TokenResourceConst.g.cs").ShouldContain("ControlTokenIdentity.ForControl(typeof(global::Demo.Button)");
        AssertCompiles(result);
    }

    [Fact]
    public void TypeMap_Keys_Are_Versioned_Fixed_Length_And_Identity_Sensitive()
    {
        var first = RegistrationNames.TypeMapKey("Demo.Group, Demo", "Demo.Button, Demo", "global::Demo.Button");
        var repeat = RegistrationNames.TypeMapKey("Demo.Group, Demo", "Demo.Button, Demo", "global::Demo.Button");
        var otherTrigger = RegistrationNames.TypeMapKey("Demo.Group, Demo", "Demo.Button, Demo", "global::Demo.ButtonToken");
        var otherGroup = RegistrationNames.TypeMapKey("Demo.OtherGroup, Demo", "Demo.Button, Demo", "global::Demo.Button");

        first.ShouldBe(repeat);
        first.ShouldStartWith("v1:");
        first.Length.ShouldBe(19);
        otherTrigger.ShouldNotBe(first);
        otherGroup.ShouldNotBe(first);
    }

    [Fact]
    public void Generated_Candidate_Keys_Do_Not_Embed_Assembly_Qualified_Names()
    {
        var result = Run("""
            namespace Demo;
            public class Button : Avalonia.Controls.Control { }
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            internal sealed class ButtonToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public double Height { get; set; }
                public override void CalculateTokenValues(bool dark) { }
            }
            """);
        var source = Source(result, "GeneratedTypeMapRegistration.g.cs");
        var candidateDeclaration = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)
            .GetRoot(TestContext.Current.CancellationToken).DescendantNodes()
            .Single(node => node is MethodDeclarationSyntax { Identifier.ValueText: "CreateCandidateKeys" } or
                            VariableDeclaratorSyntax { Identifier.ValueText: "CandidateKeys" });
        var initializer = candidateDeclaration.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().Single().Initializer!;
        initializer.Expressions.ShouldAllBe(expression => expression.IsKind(SyntaxKind.StringLiteralExpression));
        var candidateBlock = initializer.ToString();
        var keys = initializer.Expressions.Cast<LiteralExpressionSyntax>().Select(expression => expression.Token.ValueText).ToArray();

        keys.ShouldNotBeEmpty();
        keys.ShouldAllBe(key => key.StartsWith("v1:", StringComparison.Ordinal) && key.Length == 19);
        candidateBlock.ShouldNotContain("Version=");
        candidateBlock.ShouldNotContain("PublicKeyToken=");
    }

    [Fact]
    public void TypeMap_Key_Collision_Reports_Registration_Diagnostic_And_Emits_No_Source()
    {
        var control = new RegistrationControl(
            new RegistrationType("global::Demo.Button", "Demo.Button", "Demo.Button, Demo"),
            null,
            null,
            new ValueArray<string>(["global::Demo.Button", "global::Demo.ButtonToken"]),
            new ValueArray<string>([]),
            new ValueArray<string>([]));
        var package = new RegistrationPackage(
            "Demo",
            "Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            "Demo.Generated",
            new ValueArray<RegistrationControl>([control]),
            new ValueArray<RegistrationAsset>([]),
            new ValueArray<string>([]),
            RegistrationFingerprint.ComputeContractPrefix([]),
            RegistrationFingerprint.ComputeResourceKeyPrefix([]));
        var output = new GenerationOutput();

        TypeMapRegistrationWriter.Write(output, package, static _ => "0000000000000000");
        var result = output.Freeze();

        result.Diagnostics.ShouldContain(diagnostic =>
            diagnostic.Descriptor.Id == "ATOMUIREG005" && diagnostic.Message.Contains("TypeMap key collision"));
        result.Files.ShouldBeEmpty();
    }

    [Fact]
    public void TypeMap_Key_Collisions_Are_All_Reported_Against_The_First_Identity()
    {
        RegistrationControl Control(string name) => new(
            new RegistrationType("global::Demo." + name, "Demo." + name, "Demo." + name + ", Demo"),
            null,
            null,
            new ValueArray<string>(["global::Demo." + name]),
            new ValueArray<string>([]),
            new ValueArray<string>([]));
        var package = new RegistrationPackage(
            "Demo",
            "Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            "Demo.Generated",
            new ValueArray<RegistrationControl>([Control("Alpha"), Control("Beta"), Control("Gamma")]),
            new ValueArray<RegistrationAsset>([]),
            new ValueArray<string>([]),
            RegistrationFingerprint.ComputeContractPrefix([]),
            RegistrationFingerprint.ComputeResourceKeyPrefix([]));
        var output = new GenerationOutput();

        TypeMapRegistrationWriter.Write(output, package, static _ => "0000000000000000");
        var result = output.Freeze();

        var collisions = result.Diagnostics.Where(diagnostic => diagnostic.Descriptor.Id == "ATOMUIREG005").ToArray();
        collisions.Length.ShouldBe(2);
        collisions.ShouldAllBe(diagnostic => diagnostic.Message.Contains("Demo.Alpha, Demo"));
        collisions.ShouldContain(diagnostic => diagnostic.Message.Contains("Demo.Beta, Demo"));
        collisions.ShouldContain(diagnostic => diagnostic.Message.Contains("Demo.Gamma, Demo"));
        result.Files.ShouldBeEmpty();
    }

    [Fact]
    public void Both_Backends_Emit_A_Single_Collector()
    {
        foreach (var supportsTypeMap in new[] { false, true })
        {
            var output = new GenerationOutput();
            var package = CreateStagingPackage();
            TypeMapRegistrationWriter.Write(output, package, supportsTypeMap: supportsTypeMap);
            var result = output.Freeze();
            result.Diagnostics.ShouldBeEmpty();
            var source = result.Files.Single(file => file.HintName == "GeneratedTypeMapRegistration.g.cs").Text;
            source.ShouldContain("private static void Collect(");
            source.ShouldContain("ControlRegistrationRuntime.RegisterPackage(");
            source.ShouldNotContain("CollectCritical");
            source.ShouldNotContain("CollectDeferred");
            if (!supportsTypeMap)
            {
                var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);
                var full = tree.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
                    .Single(method => method.Identifier.ValueText == "CollectFull");
                full.Body!.Statements.ShouldAllBe(statement => statement is Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax);
                full.Body.Statements.Count.ShouldBe(2);
            }
        }
    }

    [Fact]
    public void Exports_Respect_Lexical_Scope_Named_Keys_And_Multiple_Targets()
    {
        var result = Run("""
            namespace Demo;
            public class Button : Avalonia.Controls.Control { }
            public class SearchEdit : Avalonia.Controls.Control { }
            public class Tab : Avalonia.Controls.Control { }
            public class Tour : Avalonia.Controls.Control { }
            internal class SearchButtonTheme : Avalonia.Styling.ControlTheme { }
            """,
            Asset("Button", """
                <ControlTheme x:Key="{x:Type local:Button}" TargetType="local:Button">
                  <ControlTheme.Resources><ControlTheme x:Key="Private" TargetType="local:SearchEdit" /></ControlTheme.Resources>
                </ControlTheme>
                """),
            Asset("Tab", "<ControlTheme x:Key=\"CardTab\" TargetType=\"local:Tab\" /><ControlTheme x:Key=\"Tour\" TargetType=\"local:Tour\" />"),
            new TextFile("Input/Themes/SearchButtonTheme.axaml", """
                <ControlTheme xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="using:Demo"
                    x:Class="Demo.SearchButtonTheme" TargetType="local:Button" />
                """));
        var source = Source(result, "GeneratedTypeMapRegistration.g.cs");
        source.ShouldContain("\"CardTab\"");
        source.ShouldContain("typeof(global::Demo.Tour)");
        source.ShouldNotContain("SearchButtonTheme.axaml");
        source.ShouldNotContain("typeof(global::Demo.SearchEdit)");
        source.ShouldNotContain("\"Private\"");
        AssertCompiles(result);
    }

    [Fact]
    public void Rejects_Private_Theme_Targets_At_The_Asset_Location()
    {
        var result = Run("namespace Demo { public class Host { private class Hidden : Avalonia.Controls.Control { } } }",
            Asset("Hidden", "<ControlTheme x:Key=\"Hidden\" TargetType=\"local:Host.Hidden\" />"));
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG003" && d.Location.GetLineSpan().Path.EndsWith("HiddenTheme.axaml"));
    }

    [Fact]
    public void Rejects_Explicit_Resource_Include_Cycles()
    {
        var result = Run("namespace Demo { public class Button : Avalonia.Controls.Control { } }",
            Asset("One", "<ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"/Two/Themes/TwoTheme.axaml\" /></ResourceDictionary.MergedDictionaries>"),
            Asset("Two", "<ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"/One/Themes/OneTheme.axaml\" /></ResourceDictionary.MergedDictionaries>"));
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG004");
    }

    internal static string RuntimeDirectory => Path.GetDirectoryName(typeof(TypeMapRegistrationGeneratorTests).Assembly.Location)!;

    private static RegistrationPackage CreateStagingPackage()
    {
        const string demoAssembly = "Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        static RegistrationControl Control(string name)
        {
            var type = new RegistrationType(
                "global::Demo." + name,
                "Demo." + name,
                "Demo." + name + ", " + demoAssembly);
            return new RegistrationControl(
                type,
                null,
                null,
                new ValueArray<string>([type.Name]),
                new ValueArray<string>([]),
                new ValueArray<string>([]),
                new ValueArray<RegistrationTrigger>([new RegistrationTrigger(type, "control")]));
        }

        return new RegistrationPackage(
            "Demo.Controls",
            demoAssembly,
            "Demo.Generated",
            new ValueArray<RegistrationControl>([Control("Button"), Control("Deferred")]),
            new ValueArray<RegistrationAsset>([]),
            new ValueArray<string>([]),
            RegistrationFingerprint.ComputeContractPrefix([]),
            RegistrationFingerprint.ComputeResourceKeyPrefix([]),
            new RegistrationType(
                "global::Demo.RegistrationBuilder",
                "Demo.RegistrationBuilder",
                "Demo.RegistrationBuilder, " + demoAssembly),
            new RegistrationType(
                "global::Demo.Void",
                "Demo.Void",
                "Demo.Void, " + demoAssembly));
    }

    private static readonly Dictionary<string, string> RuntimeDependencies = ReadRuntimeDependencies();
    private static Dictionary<string, string> ReadRuntimeDependencies()
    {
        var directory = new DirectoryInfo(RuntimeDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")) && !Directory.Exists(Path.Combine(directory.FullName, "src", "AtomUI.Core"))) directory = directory.Parent;
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory!.FullName, ".artifacts", "AtomUI.Core", "obj", "project.assets.json")));
        var root = assets.RootElement;
        var folder = root.GetProperty("packageFolders").EnumerateObject().First().Name;
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var library in root.GetProperty("targets").EnumerateObject().First(p => p.Name == "net10.0").Value.EnumerateObject())
        {
            if (!library.Value.TryGetProperty("runtime", out var runtime)) continue;
            var package = library.Name.ToLowerInvariant();
            foreach (var file in runtime.EnumerateObject().Where(p => p.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
            {
                var path = Path.Combine(folder, package, file.Name);
                if (File.Exists(path)) result[Path.GetFileNameWithoutExtension(path)] = path;
            }
        }
        return result;
    }
    internal static Assembly LoadCore()
    {
        AssemblyLoadContext.Default.Resolving += static (_, name) =>
        {
            var path = Path.Combine(RuntimeDirectory, name.Name + ".dll");
            if (!File.Exists(path) && !RuntimeDependencies.TryGetValue(name.Name!, out path)) return null;
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        };
        return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(RuntimeDirectory, "AtomUI.Core.dll"));
    }

    internal static TextFile Asset(string name, string content, string? resourceClass = null) => new($"{name}/Themes/{name}Theme.axaml", $"""
        <ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="using:Demo"{(resourceClass is null ? "" : " x:Class=\"" + resourceClass + "\"")}>{content}</ResourceDictionary>
        """);

    internal static GeneratorDriverRunResult Run(string source, params TextFile[] assets) => Run(source, new Options(), assets);

    internal static GeneratorDriverRunResult Run(string source, Options options, params TextFile[] assets)
    {
        var compilation = Compilation(source, assets);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { new TokenResourceKeyGenerator().AsSourceGenerator() },
            assets.ToImmutableArray<AdditionalText>(),
            (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
            options);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        if (!diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        return driver.GetRunResult();
    }

    internal static CSharpCompilation Compilation(string source, TextFile[] assets, string name = "TypeMapFixture")
    {
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Where(p => !Path.GetFileName(p).StartsWith("AtomUI.", StringComparison.Ordinal) && !Path.GetFileName(p).StartsWith("Avalonia", StringComparison.Ordinal))
            .Concat(RuntimeDependencies.Values.Where(path => Path.GetFileName(path).StartsWith("Avalonia", StringComparison.Ordinal)))
            .Concat(new[] { Path.Combine(RuntimeDirectory, "AtomUI.Core.dll") }).Distinct();
        var wrappers = string.Join("\n", assets.Where(a => a.Path.Contains("Theme.axaml") && a.Text.Contains("<ResourceDictionary"))
            .Select(a => $"internal sealed class {ThemeAssetInfo.GetGeneratedResourceClassName(a.Path)} : global::Avalonia.Controls.ResourceDictionary {{ }}"));
        return CSharpCompilation.Create(name,
            new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)),
                CSharpSyntaxTree.ParseText($"namespace {AtomUI.SourceGeneration.GeneratedCodeNamespace.ForAssembly(name)} {{ {wrappers} }}", new CSharpParseOptions(LanguageVersion.Preview)) },
            paths.Select(p => MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    internal static CSharpCompilation ReferencePackCompilation(string source, TextFile[] assets)
    {
        var compilation = Compilation(source, assets);
        var runtime = new DirectoryInfo(Path.GetDirectoryName(typeof(object).Assembly.Location)!);
        var packs = Path.Combine(runtime.Parent!.Parent!.Parent!.FullName, "packs", "Microsoft.NETCore.App.Ref");
        var pack = Directory.GetDirectories(packs).Where(path => Version.TryParse(Path.GetFileName(path), out var version) && version.Major == 10)
            .OrderByDescending(path => Version.Parse(Path.GetFileName(path))).First();
        var frameworkReferences = Directory.GetFiles(Path.Combine(pack, "ref", "net10.0"), "*.dll").Select(path => MetadataReference.CreateFromFile(path));
        return compilation.WithReferences(frameworkReferences.Concat(compilation.References.Where(reference =>
            (Path.GetFileName(reference.Display) ?? "").StartsWith("AtomUI.", StringComparison.Ordinal) ||
            (Path.GetFileName(reference.Display) ?? "").StartsWith("Avalonia", StringComparison.Ordinal))));
    }

    internal static string Source(GeneratorDriverRunResult result, string hint) => result.GeneratedTrees
        .Single(t => t.FilePath.EndsWith(hint, StringComparison.Ordinal)).ToString();

    internal static void AssertCompiles(GeneratorDriverRunResult result)
    {
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        // Runtime compilation is checked by callers retaining their input compilation.
    }

    internal sealed class TextFile(string path, string text) : AdditionalText
    {
        public override string Path => path;
        internal string Text => text;
        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(text);
    }

    internal sealed class Options(Dictionary<string, string>? fileOptions = null, Dictionary<string, string>? globalOptions = null) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new Values(globalOptions);
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new Values();
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new Values(fileOptions);
        private sealed class Values(Dictionary<string, string>? values = null) : AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, out string value)
            {
                if (values is not null && values.TryGetValue(key, out value!)) return true;
                value = key == "build_property.AtomUIRegistrationPackageId" ? "Demo.Controls" : "";
                return value.Length != 0;
            }
        }
    }
}
