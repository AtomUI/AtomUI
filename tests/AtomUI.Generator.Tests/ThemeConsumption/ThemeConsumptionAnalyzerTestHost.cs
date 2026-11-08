using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.ThemeConsumption;

/// <summary>
/// Shared compilation host for the control-code analyzer tests (ATOMUIGEN043/044/047). The compilation references real
/// Avalonia assemblies (they are part of the test runtime) and adds minimal stubs for the AtomUI Token types.
/// </summary>
internal static class ThemeConsumptionAnalyzerTestHost
{
    /// <summary>Value of the opt-in build property that marks the test compilation as a control package.</summary>
    public const string DefaultConsumptionAnalysis = "true";

    public const string Stubs = """
        using System;

        namespace AtomUI.Theme.Resources
        {
            public enum SharedTokenKind
            {
                ColorPrimary,
                MotionDurationMid,
            }

            public static class ControlTokenResourceKey
            {
                public static object Global(object identity, SharedTokenKind kind) => kind;
                public static object Own(object identity, object resourceKey) => resourceKey;
            }
        }

        namespace AtomUI.Theme.DesignTokens
        {
            [AttributeUsage(AttributeTargets.Class)]
            public sealed class ControlDesignTokenAttribute : Attribute
            {
            }
        }
        """;

    public const string Usings = """
        using System;
        using System.Collections.Generic;
        using Avalonia;
        using Avalonia.Controls;
        using Avalonia.Data;
        using Avalonia.Media;
        using Avalonia.Styling;
        using AtomUI.Theme.DesignTokens;
        using AtomUI.Theme.Resources;
        """;

    /// <summary>Wraps members in a <c>Control</c>-derived class together with a generated-style <c>ButtonTokenKey</c> enum.</summary>
    public static string ControlWith(string members, string extraTypes = "")
    {
        return $$"""
            {{Usings}}

            namespace Demo
            {
                public enum ButtonTokenKey
                {
                    ColorPrimary,
                }

                {{extraTypes}}

                public class Sample : Control
                {
                    {{members}}
                }
            }
            """;
    }

    // Framework plus real Avalonia assemblies. The runtime closure of Avalonia is resolved by the same helper the
    // TypeMap generator tests use; AtomUI assemblies are excluded so the Token stubs below stay the only Token types.
    private static readonly Lazy<ImmutableArray<MetadataReference>> s_platformReferences = new(static () =>
        [.. Registration.TypeMapRegistrationGeneratorTests.Compilation("namespace Demo { }", [])
                        .References
                        .Where(static reference => !(Path.GetFileName(reference.Display) ?? string.Empty)
                                                   .StartsWith("AtomUI.", StringComparison.Ordinal))]);

    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync<TAnalyzer>(string source, bool withStubs = true, string? consumptionAnalysis = DefaultConsumptionAnalysis)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var references = new List<MetadataReference>(s_platformReferences.Value);

        if (withStubs)
        {
            // The Token types live in a referenced assembly, like AtomUI.Core does for the control packages.
            var stubCompilation = CSharpCompilation.Create(
                "AtomUI.Theme.Stubs",
                [CSharpSyntaxTree.ParseText(Stubs)],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            references.Add(stubCompilation.ToMetadataReference());
        }

        var trees = new List<SyntaxTree> { CSharpSyntaxTree.ParseText(source) };

        var compilation = CSharpCompilation.Create(
            "ThemeConsumptionAnalyzerTests",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        compilation.GetDiagnostics(TestContext.Current.CancellationToken)
                   .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                   .ShouldBeEmpty();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new TAnalyzer());
        var options = new AnalyzerOptions(
            [],
            new GlobalOptionsProvider(consumptionAnalysis is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string> { ["build_property.AtomUIThemeConsumptionAnalysis"] = consumptionAnalysis }));
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers, options);
        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
        return [.. diagnostics.OrderBy(static d => d.Location.SourceSpan.Start)];
    }

    public static string Flagged(Diagnostic diagnostic)
    {
        return diagnostic.Location.SourceTree!
                         .GetText(TestContext.Current.CancellationToken)
                         .ToString(diagnostic.Location.SourceSpan);
    }

    private sealed class GlobalOptionsProvider(IReadOnlyDictionary<string, string> globalOptions) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new Options(globalOptions);

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;

        private sealed class Options(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, out string value)
            {
                if (values.TryGetValue(key, out var found))
                {
                    value = found;
                    return true;
                }

                value = null!;
                return false;
            }
        }
    }
}
