using Microsoft.CodeAnalysis;
using AtomUI.Build.Tasks.LocalizationBuild;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Localization.LocalizationGeneratorTestHost;

namespace AtomUI.Generator.Tests.Localization;

public class ApplicationLanguageBootstrapGeneratorTests
{
    [Fact]
    public void Generates_An_Explicit_Bootstrap_For_A_Partial_Application()
    {
        var execution = RunApplication(
            "public partial class App : Avalonia.Application { }",
            SourceFile());

        execution.Result.Diagnostics.ShouldBeEmpty();
        var source = GetBootstrapSource(execution.Result);
        source.ShouldContain("public partial class App");
        source.ShouldContain(
            "void global::AtomUI.Localization.IGeneratedApplicationLanguageBootstrap.RegisterApplicationLanguages(");
        CountOccurrences(
            source,
            "global::AtomUI.Generated.TestApp.GeneratedLanguageModuleRegistration.Register(builder);")
            .ShouldBe(1);
        source.ShouldNotContain("Assembly.GetTypes");
        source.ShouldNotContain("GetCustomAttributes");
        source.ShouldNotContain("Activator.CreateInstance");
        AssertCompiles(execution);
    }

    [Fact]
    public void Ignores_An_Abstract_Application_Base_When_Selecting_The_Concrete_Host()
    {
        var execution = RunApplication(
            """
            public abstract partial class ApplicationBase : Avalonia.Application { }
            public partial class App : ApplicationBase { }
            """,
            SourceFile());

        execution.Result.Diagnostics.ShouldBeEmpty();
        GetBootstrapSource(execution.Result).ShouldContain("public partial class App");
        AssertCompiles(execution);
    }

    [Fact]
    public void Compiles_Static_Language_Pack_Bundles_Directly_Into_The_Application_Bootstrap()
    {
        var reference = CreateExternalCatalogReference();
        var execution = RunWithOutputCompilation(
            RuntimeSource + "\nnamespace TestApp { public partial class App : Avalonia.Application { } }",
            [reference],
            ReferencedSourceFile(),
            StaticPackFile());

        execution.Result.Diagnostics.ShouldBeEmpty();
        execution.Result.GeneratedSources.Length.ShouldBe(2);
        var source = GetBootstrapSource(execution.Result);
        source.ShouldNotContain("GeneratedLanguageModuleRegistration.Register(builder)");
        source.ShouldContain("TranslationSourceKind.StaticLanguagePack");
        source.ShouldContain("External.Package:External.Localization.ExternalLangResourceKind");
        AssertCompiles(execution);
    }

    [Fact]
    public void Uses_A_Referenced_Module_Source_To_Validate_A_Static_Language_Pack()
    {
        var reference = CreateExternalCatalogReference();
        var execution = RunWithOutputCompilation(
            RuntimeSource + "\nnamespace TestApp { public partial class App : Avalonia.Application { } }",
            [reference],
            ReferencedSourceFile(),
            StaticPackFile());

        execution.Result.Diagnostics.ShouldBeEmpty();
        var source = GetBootstrapSource(execution.Result);
        source.ShouldContain("TranslationSourceKind.StaticLanguagePack");
        source.ShouldNotContain("TranslationSourceKind.ModuleBuiltIn");
        AssertCompiles(execution);
    }

    [Fact]
    public void Registers_Application_Overrides_Outside_The_Module_Registration()
    {
        var execution = RunApplication(
            "public partial class App : Avalonia.Application { }",
            SourceFile(),
            OverrideFile());

        execution.Result.Diagnostics.ShouldBeEmpty();
        var bootstrap = GetBootstrapSource(execution.Result);
        bootstrap.ShouldContain("TranslationSourceKind.ApplicationOverride");
        bootstrap.ShouldContain("null,");
        GetGeneratedSource(execution.Result, "GeneratedLanguageModuleRegistration.g.cs")
            .ShouldNotContain("TranslationSourceKind.ApplicationOverride");
        AssertCompiles(execution);
    }

    [Fact]
    public void Does_Not_Generate_A_Bootstrap_Without_Localization_Input()
    {
        var execution = RunWithOutputCompilation(
            RuntimeSource + "\nnamespace TestApp { public class App : Avalonia.Application { } }");

        execution.Result.Diagnostics.ShouldBeEmpty();
        execution.Result.GeneratedSources.ShouldBeEmpty();
    }

    [Fact]
    public void Generates_Only_Module_Registration_For_A_Class_Library()
    {
        var execution = RunWithOutputCompilation(
            RuntimeSource + CatalogSource,
            SourceFile());

        execution.Result.Diagnostics.ShouldBeEmpty();
        execution.Result.GeneratedSources.Select(static source => source.HintName)
                 .ShouldContain("GeneratedLanguageModuleRegistration.g.cs");
        execution.Result.GeneratedSources.Select(static source => source.HintName)
                 .ShouldNotContain("GeneratedApplicationLanguageBootstrap.g.cs");
    }

    [Fact]
    public void Reports_A_Non_Partial_Application_At_Its_Identifier()
    {
        var execution = RunApplication(
            "public class App : Avalonia.Application { }",
            SourceFile());

        AssertApplicationDiagnostic(execution.Result, "App", "partial");
        execution.Result.GeneratedSources.Select(static source => source.HintName)
                 .ShouldNotContain("GeneratedApplicationLanguageBootstrap.g.cs");
    }

    [Fact]
    public void Reports_Each_Concrete_Application_When_The_Host_Is_Ambiguous()
    {
        var execution = RunApplication(
            """
            public partial class AppA : Avalonia.Application { }
            public partial class AppB : Avalonia.Application { }
            """,
            SourceFile());

        var diagnostics = execution.Result.Diagnostics
                                   .Where(static diagnostic => diagnostic.Id == "ATOMUILOC008")
                                   .ToArray();
        diagnostics.Length.ShouldBe(2);
        diagnostics.ShouldAllBe(static diagnostic =>
            diagnostic.GetMessage().Contains("more than one", StringComparison.Ordinal));
        diagnostics.Select(GetLocationText).OrderBy(static text => text)
                   .ShouldBe(["AppA", "AppB"]);
    }

    [Theory]
    [InlineData(
        "public partial class Host { public partial class App : Avalonia.Application { } }",
        "top-level")]
    [InlineData(
        "public partial class App<T> : Avalonia.Application { }",
        "non-generic")]
    public void Reports_Unsupported_Application_Shapes(string declaration, string messageFragment)
    {
        var execution = RunApplication(declaration, SourceFile());

        AssertApplicationDiagnostic(execution.Result, "App", messageFragment);
    }

    private static TestGeneratorExecution RunApplication(
        string applicationDeclaration,
        params TestAdditionalText[] files)
    {
        return RunWithOutputCompilation(
            RuntimeSource + CatalogSource + "\nnamespace TestApp { " + applicationDeclaration + " }",
            files);
    }

    private static void AssertApplicationDiagnostic(
        GeneratorRunResult result,
        string locationText,
        string messageFragment)
    {
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Id == "ATOMUILOC008");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage().ShouldContain(messageFragment);
        GetLocationText(diagnostic).ShouldBe(locationText);
    }

    private static string GetLocationText(Diagnostic diagnostic)
    {
        return diagnostic.Location.SourceTree!.GetText(TestContext.Current.CancellationToken)
                         .ToString(diagnostic.Location.SourceSpan);
    }

    private static void AssertCompiles(TestGeneratorExecution execution)
    {
        execution.DriverDiagnostics.ShouldBeEmpty();
        execution.OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                 .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                 .ShouldBeEmpty();
    }

    private static string GetBootstrapSource(GeneratorRunResult result)
    {
        return GetGeneratedSource(result, "GeneratedApplicationLanguageBootstrap.g.cs");
    }

    private static string GetGeneratedSource(GeneratorRunResult result, string hintName)
    {
        return result.GeneratedSources.Single(source => source.HintName == hintName)
                     .SourceText.ToString();
    }

    private static int CountOccurrences(string source, string value)
    {
        return source.Split([value], StringSplitOptions.None).Length - 1;
    }

    private static TestAdditionalText SourceFile()
    {
        return LanguageFile(
            "Localization/en-US.xlf",
            SourceXliff,
            "ModuleBuiltIn",
            "Test.Package",
            "Test.Package");
    }

    private static TestAdditionalText OverrideFile()
    {
        return LanguageFile(
            "Localization/Overrides/zh-CN.xlf",
            TargetXliff(
                "TestApp.Localization.AppLangResourceKind",
                "zh-CN",
                "Application title",
                "应用标题"),
            "ApplicationOverride",
            "TestApp",
            "Test.Package");
    }

    private static TestAdditionalText StaticPackFile()
    {
        return LanguageFile(
            "packages/External.Package.I18n.ZhCN/zh-CN.xlf",
            TargetXliff(
                "External.Localization.ExternalLangResourceKind",
                "zh-CN",
                "External title",
                "外部标题"),
            "StaticLanguagePack",
            "External.Package.I18n.ZhCN",
            "External.Package");
    }

    private static TestAdditionalText ReferencedSourceFile()
    {
        return LanguageFile(
            "packages/External.Package/Localization/en-US.xlf",
            """
            <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US">
              <file id="External.Localization.ExternalLangResourceKind">
                <unit id="Title"><segment><source>External title</source></segment></unit>
              </file>
            </xliff>
            """,
            "ModuleBuiltIn",
            "External.Package",
            "External.Package");
    }

    private static TestAdditionalText LanguageFile(
        string path,
        string content,
        string sourceKind,
        string sourceIdentity,
        string moduleId)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["build_metadata.AdditionalFiles.AtomUILanguage"] = "true",
            ["build_metadata.AdditionalFiles.AtomUILanguageSourceKind"] = sourceKind,
            ["build_metadata.AdditionalFiles.AtomUILanguageSourceIdentity"] = sourceIdentity,
            ["build_metadata.AdditionalFiles.AtomUILanguageModuleId"] = moduleId
        };
        if (sourceKind == "StaticLanguagePack")
        {
            metadata["build_metadata.AdditionalFiles.AtomUILanguageContractValidation"] =
                "Verified";
            metadata["build_metadata.AdditionalFiles.AtomUILanguageSourceFingerprint"] =
                LanguageSourceFingerprint.Compute(Xliff21Parser.Parse(content).Document!);
        }
        return new TestAdditionalText(path, content, metadata);
    }

    private static MetadataReference CreateExternalCatalogReference()
    {
        return CreateMetadataReference(
            "External.Package",
            """
            namespace AtomUI.Localization
            {
                [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = false)]
                public sealed class LanguageCatalogAttribute : System.Attribute;
            }

            namespace External.Localization
            {
                [AtomUI.Localization.LanguageCatalog]
                public enum ExternalLangResourceKind
                {
                    Title
                }
            }
            """);
    }

    private static string TargetXliff(
        string catalogMetadataName,
        string language,
        string source,
        string target)
    {
        return $$"""
            <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US" trgLang="{{language}}">
              <file id="{{catalogMetadataName}}">
                <unit id="Title"><segment><source>{{source}}</source><target state="translated">{{target}}</target></segment></unit>
              </file>
            </xliff>
            """;
    }

    private const string RuntimeSource = """
        #nullable enable
        namespace Avalonia
        {
            public abstract class Application { }
        }

        namespace AtomUI.Localization
        {
            [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = false)]
            public sealed class LanguageCatalogAttribute : System.Attribute;

            public interface IGeneratedApplicationLanguageBootstrap
            {
                void RegisterApplicationLanguages(ILocalizationBuilder builder);
            }

            public abstract class LanguageResourceExtension<TResourceKind>
                where TResourceKind : struct, System.Enum
            {
                protected LanguageResourceExtension() { }
                protected LanguageResourceExtension(TResourceKind kind) { }
            }

            public interface ILocalizationBuilder
            {
                void AddCatalog(LanguageCatalogDescriptor descriptor);
                void AddTranslationBundle(TranslationBundleDescriptor descriptor);
            }

            public abstract class LanguageCatalogDescriptor { }

            public sealed class LanguageCatalogDescriptor<TResourceKind> : LanguageCatalogDescriptor
                where TResourceKind : struct, System.Enum
            {
                public LanguageCatalogDescriptor(
                    string catalogId,
                    System.Collections.Generic.IReadOnlyList<LanguageCatalogUnitDescriptor> units,
                    System.Func<TResourceKind, int> unitSlotResolver) { }
            }

            public sealed class LanguageCatalogUnitDescriptor
            {
                public LanguageCatalogUnitDescriptor(string key, bool isFormatted = false) { }
            }

            public readonly struct LanguageTag
            {
                public static LanguageTag Parse(string value) => default;
            }

            public enum TranslationSourceKind : byte
            {
                ModuleBuiltIn,
                StaticLanguagePack,
                ApplicationOverride
            }

            public sealed class TranslationBundleDescriptor
            {
                public TranslationBundleDescriptor(
                    string catalogId,
                    LanguageTag language,
                    TranslationSourceKind sourceKind,
                    string sourceIdentity,
                    System.Collections.Generic.IReadOnlyList<string?> values) { }
            }
        }
        """;

    private const string CatalogSource = """

        namespace TestApp.Localization
        {
            [AtomUI.Localization.LanguageCatalog]
            public enum AppLangResourceKind
            {
                Title,
                Description
            }
        }
        """;

    private const string SourceXliff = """
        <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US">
          <file id="TestApp.Localization.AppLangResourceKind">
            <unit id="Title"><segment><source>Application title</source></segment></unit>
            <unit id="Description"><segment><source>Application description</source></segment></unit>
          </file>
        </xliff>
        """;
}
