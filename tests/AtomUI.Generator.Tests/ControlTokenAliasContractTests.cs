// Shared end-to-end contract: generated metadata, per-layer precedence and retained resource values.
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Registration.TypeMapRegistrationGeneratorTests;

namespace AtomUI.Generator.Tests;

public sealed class ControlTokenAliasContractTests
{
    [Fact]
    public void Generated_Alias_Metadata_Drives_Canonical_Merge_And_Compiler_Projection()
    {
        var input = Compilation(Source, [], "AtomUI.Registration.Fixtures");
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new TokenResourceKeyGenerator().AsSourceGenerator()],
            parseOptions: (CSharpParseOptions)input.SyntaxTrees.First().Options, optionsProvider: new Options());
        driver.RunGeneratorsAndUpdateCompilation(input, out var output, out var diagnostics, TestContext.Current.CancellationToken);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        using var stream = new MemoryStream();
        var result = output.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        LoadCore();
        var assembly = System.Reflection.Assembly.Load(stream.ToArray());
        var actual = (string)assembly.GetType("AliasDemo.Runner")!.GetMethod("Run")!.Invoke(null, null)!;
        actual.ShouldBe("0|1|9|9|0|0|ATMTHM4008");
    }

    private const string Source = """
        using System;
        using System.Linq;
        using System.Reflection;
        using AtomUI.Theme;
        using AtomUI.Theme.Compilation;
        using AtomUI.Theme.Configuration;
        using AtomUI.Theme.Definitions;
        using AtomUI.Theme.DesignTokens;
        using AtomUI.Theme.Schema;
        namespace AliasDemo;
        public sealed class Button : Avalonia.Controls.Control { }
        public static class Counter { public static int Initialized; }
        [ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision=1)]
        internal sealed partial class ButtonToken : AbstractControlDesignToken, IControlTokenDefaults, IControlTokenAliases
        {
            static ButtonToken() { Counter.Initialized++; }
            public double OwnPadding { get; set; }
            [ControlTokenAlias(nameof(OwnPadding))] public double TablePadding { get; set; }
            [ControlTokenDefault(nameof(OwnPadding))]
            private static double DefaultPadding(in ControlTokenDefaultContext context) => 4;
        }
        public static class Runner
        {
            public static string Run()
            {
                var factory=typeof(Runner).Assembly.GetTypes().Single(t=>t.Name=="GeneratedThemeSchemaDescriptorFactory");
                var method=factory.GetMethods(BindingFlags.Static|BindingFlags.NonPublic).Single(m=>m.Name.StartsWith("CreateControlDescriptor_", StringComparison.Ordinal));
                var descriptor=(ControlTokenDescriptor)method.Invoke(null,null)!;
                var initialized=Counter.Initialized;
                var registry=new ThemeSchemaRegistry(global::AtomUI.Generated.AtomUICore.GeneratedThemeSchema.GetGlobalTokens(),
                    new[]{descriptor}, global::AtomUI.Generated.AtomUICore.GeneratedThemeSchema.GetAlgorithms());
                registry.TryGetControl(descriptor.Identity, out var boundDescriptor);
                descriptor=boundDescriptor ?? throw new InvalidOperationException("Missing bound descriptor");
                NormalizedThemeConfig Layer(string name, string value) => ThemeConfigNormalizer.Normalize(new ThemeConfigBuilder()
                    .WithControl(descriptor.Identity, new ControlThemeConfigBuilder().WithToken(name,value).Build()).Build(),registry).Config!;
                var library=ThemeCompiler.CreateLibraryDefaults(registry);
                var parent=ThemeConfigMerger.Merge(library,null,Layer("TablePadding","7")).EffectiveConfig;
                var merged=ThemeConfigMerger.Merge(library,parent,Layer("OwnPadding","9")).EffectiveConfig;
                var definition=new PreparedThemeDefinition("Alias","Alias",ThemeAppearance.Light,ThemeAppearance.Light,true,[],[],[],new("test",0,0,"/Theme"));
                ControlThemeSnapshot Compile(NormalizedThemeConfig config) => new ThemeCompiler().Compile(new(definition,new("test","1","A1"),config,registry)).Snapshot!.Controls[descriptor.Slot];
                var first=Compile(merged);
                var zero=Compile(ThemeConfigMerger.Merge(library,null,Layer("TablePadding","0")).EffectiveConfig);
                var conflict=ThemeConfigNormalizer.Normalize(new ThemeConfigBuilder().WithControl(descriptor.Identity,
                    new ControlThemeConfigBuilder().WithToken("OwnPadding","9").WithToken("TablePadding","8").Build()).Build(),registry);
                return $"{initialized}|{merged.Controls.Single().OwnTokens.Count}|{first.ControlTokenValues.Get<double>(0)}|{first.ControlTokenValues.Get<double>(1)}|{zero.ControlTokenValues.Get<double>(0)}|{zero.ControlTokenValues.Get<double>(1)}|{conflict.Diagnostics.Single().Code}";
            }
        }
        """;
}
