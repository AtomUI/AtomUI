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

    // 判定必须按符号而不是文本：同一个 Attach 可能经别名或限定名调用，
    // 文本匹配会把别名写法误报成「缺少 Attach」。
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

    // 反向：与自己写的同名方法无关，不得因文本相似而跳过挂接（漏挂接是静默失败，比误报严重）。
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

    // spec §4：抽象类不生成（由派生类生成）。抽象基类无法实例化，其重写也不能自行修复，
    // 因此不得报「缺少 Attach」——诊断只对具体类有意义。
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

    // 同名类型分布在不同命名空间时，hint 名必须唯一，否则 AddSource 会抛重复键异常。
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

    // 未声明 partial 的具体应用类不生成挂接（运行时守卫会给出修复说明），也不报缺少 Attach。
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
