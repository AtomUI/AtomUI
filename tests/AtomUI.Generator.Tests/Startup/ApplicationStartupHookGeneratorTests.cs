using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using AtomUI.Generator.Startup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.Startup;

public class ApplicationStartupHookGeneratorTests
{
    [Fact]
    public void Generates_the_hook_for_every_concrete_partial_application()
    {
        var output = Run("""
            namespace Demo
            {
                public partial class App { }
                public partial class OtherApp : Avalonia.Application { }
                public abstract partial class AppBase : Avalonia.Application { }
            }
            """, out var diagnostics, additionalSources: ["namespace Demo { public partial class App : Avalonia.Application { } }"]);

        diagnostics.ShouldBeEmpty();
        var generated = string.Join("\n", output.SyntaxTrees.Skip(3).Select(tree => tree.ToString()));
        CountOccurrences(generated, "global::AtomUI.ApplicationStartup.Attach(this);").ShouldBe(2);
        generated.ShouldNotContain("class AppBase");
        output.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
    }

    // The decision must go by symbol, not by text: the same Attach may be called through an alias or a
    // qualified name, and text matching would report the alias form as a missing Attach.
    [Theory]
    [InlineData("AtomUI.ApplicationStartup.Attach(this);")]
    [InlineData("global::AtomUI.ApplicationStartup.Attach(this);")]
    [InlineData("Startup.Attach(this);")]
    public void Skips_an_application_that_already_calls_Attach_however_it_is_written(string call)
    {
        var alias = call.StartsWith("Startup.", StringComparison.Ordinal)
            ? "using Startup = AtomUI.ApplicationStartup;\n"
            : string.Empty;
        Run("public partial class App : Avalonia.Application { }", out var diagnostics, additionalSources: [alias + $$"""
            public partial class App
            {
                public override void RegisterServices()
                {
                    base.RegisterServices();
                    {{call}}
                }
            }
            """]);

        diagnostics.ShouldBeEmpty();
    }

    // The other direction: a same-named method written by the app is unrelated and must not skip the hook
    // because the text looks similar (a missing hook fails silently, which is worse than a false report).
    [Fact]
    public void Does_not_treat_an_unrelated_Attach_call_as_hooked()
    {
        Run("""
            public partial class App : Avalonia.Application
            {
                private static void Attach(App app) { }
                public override void RegisterServices()
                {
                    base.RegisterServices();
                    Attach(this);
                }
            }
            """, out var diagnostics);

        diagnostics.ShouldHaveSingleItem().Id.ShouldBe(AtomUIDiagnosticIds.StartupHookMissingAttach);
    }
    [Fact]
    public void Reports_an_override_without_Attach()
    {
        Run("""
            public partial class App : Avalonia.Application
            {
                public override void RegisterServices() { base.RegisterServices(); }
            }
            """, out var diagnostics);

        diagnostics.ShouldHaveSingleItem().Id.ShouldBe(AtomUIDiagnosticIds.StartupHookMissingAttach);
    }

    [Fact]
    public void Does_nothing_without_AtomUI_Core()
    {
        var output = Run("public partial class App : Avalonia.Application { }", out var diagnostics, includeAtomUI: false);
        diagnostics.ShouldBeEmpty();
        output.SyntaxTrees.Count().ShouldBe(2);
    }

    // Abstract classes produce nothing (their derived classes do). An abstract base class cannot be
    // instantiated and its overrides cannot fix themselves, so no missing Attach is reported: the diagnostic
    // only makes sense for concrete classes.
    [Fact]
    public void Does_not_report_an_abstract_base_that_overrides_without_Attach()
    {
        var output = Run("""
            public abstract partial class AppBase : Avalonia.Application
            {
                public override void RegisterServices() { base.RegisterServices(); }
            }
            public partial class App : AppBase { }
            """, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var generated = string.Join("\n", output.SyntaxTrees.Skip(2).Select(tree => tree.ToString()));
        CountOccurrences(generated, "global::AtomUI.ApplicationStartup.Attach(this);").ShouldBe(1);
        generated.ShouldNotContain("class AppBase");
    }

    // When same-named types live in different namespaces the hint name must be unique, otherwise AddSource
    // throws a duplicate key exception.
    [Fact]
    public void Generates_distinct_hints_for_same_named_applications_in_different_namespaces()
    {
        var output = Run("""
            namespace First { public partial class App : Avalonia.Application { } }
            namespace Second { public partial class App : Avalonia.Application { } }
            namespace A.B { public partial class App : Avalonia.Application { } }
            namespace A_B { public partial class App : Avalonia.Application { } }
            """, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var hints = output.SyntaxTrees.Skip(2).Select(tree => tree.FilePath).ToArray();
        hints.Length.ShouldBe(4);
        hints.Distinct(StringComparer.Ordinal).Count().ShouldBe(4);
        var generated = string.Join("\n", output.SyntaxTrees.Skip(2).Select(tree => tree.ToString()));
        generated.ShouldContain("namespace First");
        generated.ShouldContain("namespace Second");
    }

    // A concrete app class that is not declared partial gets no hook (the runtime guard explains the fix) and
    // no missing Attach report.
    [Fact]
    public void Does_not_generate_for_a_non_partial_application()
    {
        var output = Run("public class App : Avalonia.Application { }", out var diagnostics);

        diagnostics.ShouldBeEmpty();
        output.SyntaxTrees.Count().ShouldBe(2);
    }

    private static CSharpCompilation Run(string source, out ImmutableArray<Diagnostic> diagnostics, bool includeAtomUI = true,
        params string[] additionalSources)
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
            .Split(Path.PathSeparator)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToImmutableArray();
        var stubs = includeAtomUI ? AvaloniaStubs + AtomUIStubs : AvaloniaStubs;
        var compilation = CSharpCompilation.Create("StartupHookTests",
            new[] { source, stubs }.Concat(additionalSources).Select(text => CSharpSyntaxTree.ParseText(text)),
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        CSharpGeneratorDriver.Create(new ApplicationStartupHookGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics, TestContext.Current.CancellationToken);
        return (CSharpCompilation)output;
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var index = text.IndexOf(value, StringComparison.Ordinal); index >= 0;
             index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal))
        {
            count++;
        }
        return count;
    }

    private const string AvaloniaStubs = """
        namespace Avalonia
        {
            public class Application
            {
                public virtual void RegisterServices() { }
            }
        }
        """;

    private const string AtomUIStubs = """
        namespace AtomUI
        {
            public static class ApplicationStartup
            {
                public static void Attach(Avalonia.Application application) { }
            }
        }
        """;
}
