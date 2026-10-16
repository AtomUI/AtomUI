using System.Collections.Immutable;
using AtomUI.Generator.Localization.Catalog;
using AtomUI.Generator.Localization.Xliff;
using AtomUI.Build.Tasks.LocalizationBuild;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Localization.LocalizationGeneratorTestHost;

namespace AtomUI.Generator.Tests.Localization;

public class LanguageCatalogCompilerGeneratorTests
{
    [Fact]
    public void Compiled_Bundle_Slots_Follow_Ordinal_Keys_Instead_Of_File_Order()
    {
        var catalog = new LanguageCatalogInfo(
            "Test.Package",
            CatalogMetadataName,
            "TestApp.Localization",
            "global::TestApp.Localization.LoginLangResourceKind",
            [
                new LanguageCatalogUnitInfo("Title", Location.None),
                new LanguageCatalogUnitInfo("ItemCount", Location.None)
            ],
            Location.None);
        var file = new LanguageFileInput(
            new AdditionalLanguageFile(
                "Localization/en-US.xlf",
                SourceText.From(SourceXliff),
                Xliff21Parser.Parse(SourceXliff).Document!),
            "Test.Package",
            LanguageFileSourceKind.ModuleBuiltIn,
            "Test.Package",
            LanguageFileContractValidation.Verified,
            null);

        var result = LanguageCatalogCompiler.Compile(
            [new LanguageCatalogParseResult(catalog, ImmutableArray<Diagnostic>.Empty)],
            [new LanguageFileInputResult(file, ImmutableArray<Diagnostic>.Empty)]);

        result.Diagnostics.ShouldBeEmpty();
        var compiled = result.Catalogs.ShouldHaveSingleItem();
        compiled.FormattedUnits.ShouldBe([false, true]);
        compiled.Bundles.ShouldHaveSingleItem().Values.ShouldBe(["Title", "Items {0}"]);
    }

    [Fact]
    public void Joins_Complete_BuiltIn_Languages_To_The_Catalog()
    {
        var result = Run(
            CatalogSource,
            SourceFile(),
            TargetFile("zh-CN", "标题", "项目 {0}"),
            TargetFile("zh-TW", "標題", "項目 {0}"));

        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Allows_An_Explicit_Empty_Translation_When_The_English_Source_Is_Empty()
    {
        var source = SourceXliff.Replace(
            "<source>Title</source>",
            "<source></source>");
        var target = TargetXliff("zh-CN", "", "项目 {0}").Replace(
            "<source>Title</source>",
            "<source></source>");
        var result = Run(
            CatalogSource,
            SourceFile().WithText(source),
            LanguageFile("Localization/zh-CN.xlf", target));

        result.Diagnostics.ShouldBeEmpty();
        var tree = result.GeneratedSources.Single(static sourceResult =>
            sourceResult.HintName == "GeneratedLanguageModuleRegistration.g.cs").SyntaxTree;
        var value = tree.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax>()
            .Single(static variable => variable.Identifier.ValueText == "Data").Initializer!.Value;
        var data = AtomUI.Localization.Data.LanguageModuleDataCodec.Read("Test.Package",
            ((Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)value).Token.ValueText);
        data.Bundles.Single(static bundle => bundle.Language == "zh-CN").Values[1].ShouldBe(string.Empty);
        data.Bundles.Single(static bundle => bundle.Language == "en-US").Values[1].ShouldBe(string.Empty);
    }

    [Fact]
    public void Reports_A_Missing_English_Source_Bundle()
    {
        var result = Run(
            CatalogSource,
            TargetFile("zh-CN", "标题", "项目 {0}"));

        AssertHasDiagnostic(result, "ATOMUILOC006", "complete en-US");
    }

    [Fact]
    public void Reports_Duplicate_English_Source_Bundles()
    {
        var result = Run(
            CatalogSource,
            SourceFile(),
            LanguageFile("Localization/duplicate.en-US.xlf", SourceXliff));

        AssertHasDiagnostic(result, "ATOMUILOC006", "exactly one en-US");
    }

    [Fact]
    public void Reports_A_File_Id_That_Does_Not_Match_The_Catalog()
    {
        var result = Run(
            CatalogSource,
            SourceFile().WithText(SourceXliff.Replace(CatalogMetadataName, "TestApp.Localization.OtherKind")));

        AssertHasDiagnostic(result, "ATOMUILOC006", "does not match");
    }

    [Theory]
    [InlineData("id=\"Title\"", "id=\"Heading\"", "unit Key")]
    public void Reports_A_Unit_Contract_Mismatch(
        string oldValue,
        string newValue,
        string messageFragment)
    {
        var result = Run(
            CatalogSource,
            SourceFile().WithText(SourceXliff.Replace(oldValue, newValue)));

        AssertHasDiagnostic(result, "ATOMUILOC006", messageFragment);
    }

    [Fact]
    public void Reports_Target_Source_Text_That_Differs_From_English()
    {
        var result = Run(
            CatalogSource,
            SourceFile(),
            TargetFile("zh-CN", "标题", "项目 {0}")
                .WithText(TargetXliff("zh-CN", "标题", "项目 {0}")
                    .Replace("Items {0}", "Changed {0}")));

        AssertHasDiagnostic(result, "ATOMUILOC006", "source text");
    }

    [Fact]
    public void Ignores_Obsolete_Historical_Units_In_A_Translation_File()
    {
        var target = TargetXliff("zh-CN", "标题", "项目 {0}")
            .Replace(
                "  </file>",
                "    <unit id=\"Removed\" translate=\"no\"><segment>" +
                "<source>Removed</source><target state=\"reviewed\">已移除</target>" +
                "</segment></unit>\n  </file>");
        var result = Run(
            CatalogSource,
            SourceFile(),
            LanguageFile("Localization/zh-CN.xlf", target));

        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Reports_A_Target_That_Is_Not_Publishable()
    {
        var result = Run(
            CatalogSource,
            SourceFile(),
            TargetFile("zh-CN", "标题", "项目 {0}")
                .WithText(TargetXliff("zh-CN", "标题", "项目 {0}")
                    .Replace("state=\"translated\"", "state=\"initial\"")));

        AssertHasDiagnostic(result, "ATOMUILOC007", "publishable");
    }

    [Theory]
    [InlineData("<target state=\"translated\">标题</target>", "<target state=\"translated\">   </target>")]
    [InlineData("<target state=\"translated\">标题</target>", "<target state=\"translated\" subState=\"needs-review\">标题</target>")]
    public void Reports_A_Target_With_Unpublishable_Content_Or_SubState(
        string currentTarget,
        string invalidTarget)
    {
        var result = Run(
            CatalogSource,
            SourceFile(),
            TargetFile("zh-CN", "标题", "项目 {0}")
                .WithText(TargetXliff("zh-CN", "标题", "项目 {0}")
                    .Replace(currentTarget, invalidTarget)));

        AssertHasDiagnostic(result, "ATOMUILOC007", "publishable");
    }

    [Fact]
    public void Reports_Duplicate_Same_Priority_Translation_Sources()
    {
        var result = Run(
            CatalogSource,
            SourceFile(),
            TargetFile("zh-CN", "标题", "项目 {0}"),
            TargetFile("zh-CN", "另一标题", "另一个 {0}", "Localization/duplicate.zh-CN.xlf"));

        AssertHasDiagnostic(result, "ATOMUILOC006", "same priority");
    }

    [Fact]
    public void Resolves_A_Static_Language_Pack_Catalog_From_Metadata()
    {
        var reference = CreateExternalCatalogReference();
        var result = Run(
            "namespace TestApp { public sealed class Marker { } }",
            [reference],
            ReferencedSourceFile(),
            StaticPackFile());

        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Resolves_An_Active_Deferred_Static_Language_Pack_Against_The_Referenced_Catalog()
    {
        var reference = CreateExternalCatalogReference();
        var result = Run(
            """
            namespace Avalonia { public class Application { } }
            namespace TestApp { public partial class App : Avalonia.Application { } }
            """,
            [reference],
            ReferencedSourceFile(),
            StaticPackFile(contractValidation: "Deferred"));

        result.Diagnostics.ShouldBeEmpty();
        var bootstrap = result.GeneratedSources
                              .Single(source =>
                                  source.HintName == "GeneratedApplicationLanguageBootstrap.g.cs")
                              .SourceText
                              .ToString();
        bootstrap.ShouldContain("GeneratedApplicationLanguageData.Register(builder)");
    }

    [Fact]
    public void Reports_A_Deferred_Static_Pack_With_An_Unknown_Unit_Key()
    {
        var reference = CreateExternalCatalogReference();
        var result = Run(
            "namespace TestApp { public sealed class Marker { } }",
            [reference],
            ReferencedSourceFile(),
            StaticPackFile(
                unitKey: "Heading",
                contractValidation: "Deferred"));

        AssertHasDiagnostic(result, "ATOMUILOC006", "unit Key");
    }

    [Fact]
    public void Reports_A_Deferred_Static_Pack_With_Changed_Source_Text()
    {
        var reference = CreateExternalCatalogReference();
        var result = Run(
            "namespace TestApp { public sealed class Marker { } }",
            [reference],
            ReferencedSourceFile(),
            StaticPackFile(
                contractValidation: "Deferred",
                titleSource: "Changed title"));

        AssertHasDiagnostic(result, "ATOMUILOC006", "source text");
        AssertHasDiagnostic(result, "ATOMUILOC006", "source fingerprint");
    }

    [Fact]
    public void Reports_A_Static_Language_Pack_Without_An_Authoritative_English_Source()
    {
        var reference = CreateExternalCatalogReference();
        var result = Run(
            "namespace TestApp { public sealed class Marker { } }",
            [reference],
            StaticPackFile());

        AssertHasDiagnostic(result, "ATOMUILOC006", "complete en-US");
    }

    [Fact]
    public void Ignores_A_Static_Pack_When_The_Target_Module_Is_Not_Referenced()
    {
        var result = Run(
            "namespace TestApp { public sealed class Marker { } }",
            [],
            StaticPackFile());

        result.Diagnostics.ShouldBeEmpty();
        result.GeneratedSources.ShouldBeEmpty();
    }

    [Fact]
    public void Keeps_A_Deferred_Static_Pack_Dormant_When_The_Target_Module_Is_Not_Referenced()
    {
        var result = Run(
            "namespace TestApp { public sealed class Marker { } }",
            [],
            StaticPackFile(contractValidation: "Deferred"));

        result.Diagnostics.ShouldBeEmpty();
        result.GeneratedSources.ShouldBeEmpty();
    }

    [Fact]
    public void Reports_A_Static_Pack_For_An_Unknown_Catalog_In_A_Referenced_Module()
    {
        var reference = CreateExternalModuleReference();
        var result = Run(
            "namespace TestApp { public sealed class Marker { } }",
            [reference],
            StaticPackFile());

        AssertHasDiagnostic(result, "ATOMUILOC006", "referenced Catalog");
    }

    [Fact]
    public void Reports_A_Deferred_Static_Pack_For_An_Unknown_Catalog_In_An_Active_Module()
    {
        var reference = CreateExternalModuleReference();
        var result = Run(
            "namespace TestApp { public sealed class Marker { } }",
            [reference],
            StaticPackFile(contractValidation: "Deferred"));

        AssertHasDiagnostic(result, "ATOMUILOC006", "referenced Catalog");
    }

    [Fact]
    public void Accepts_A_Partial_Application_Override()
    {
        var result = Run(
            CatalogSource,
            SourceFile(),
            ApplicationOverrideFile(includeItemCount: false));

        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Accepts_Disjoint_Application_Override_Fragments()
    {
        var result = Run(
            CatalogSource,
            SourceFile(),
            ApplicationOverrideFile(
                includeItemCount: false,
                path: "Localization/Overrides/title.zh-CN.xlf",
                sourceIdentity: "TestApp.Title"),
            ApplicationOverrideFile(
                includeTitle: false,
                path: "Localization/Overrides/item-count.zh-CN.xlf",
                sourceIdentity: "TestApp.ItemCount"));

        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Reports_Overlapping_Application_Override_Units()
    {
        var result = Run(
            CatalogSource,
            SourceFile(),
            ApplicationOverrideFile(
                includeItemCount: false,
                path: "Localization/Overrides/first.zh-CN.xlf",
                sourceIdentity: "TestApp.One"),
            ApplicationOverrideFile(
                includeItemCount: false,
                path: "Localization/Overrides/second.zh-CN.xlf",
                sourceIdentity: "TestApp.Two"));

        AssertHasDiagnostic(result, "ATOMUILOC006", "unit Key 'Title'");
    }

    [Fact]
    public void Reports_A_Static_Pack_Module_Id_Mismatch()
    {
        var reference = CreateExternalCatalogReference();
        var result = Run(
            "namespace TestApp { public sealed class Marker { } }",
            [reference],
            ReferencedSourceFile(),
            StaticPackFile(moduleId: "Other.Package"));

        AssertHasDiagnostic(result, "ATOMUILOC006", "language module");
    }

    [Fact]
    public void Resolves_Module_Id_From_Referenced_Assembly_Metadata()
    {
        var reference = CreateExternalCatalogReference(
            assemblyName: "External.Package.Runtime",
            moduleId: "External.Package");
        var result = Run(
            "namespace TestApp { public sealed class Marker { } }",
            [reference],
            ReferencedSourceFile(moduleId: "External.Package"),
            StaticPackFile());

        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Reports_A_Renamed_Referenced_Unit_As_A_Key_Change()
    {
        var reference = CreateExternalCatalogReference();
        var result = Run(
            "namespace TestApp { public sealed class Marker { } }",
            [reference],
            ReferencedSourceFile(unitKey: "Heading"),
            StaticPackFile());

        AssertHasDiagnostic(result, "ATOMUILOC006", "unit Key");
    }

    private static void AssertHasDiagnostic(
        GeneratorRunResult result,
        string id,
        string messageFragment)
    {
        var diagnostic = result.Diagnostics.FirstOrDefault(diagnostic =>
            diagnostic.Id == id &&
            diagnostic.GetMessage().Contains(messageFragment, StringComparison.Ordinal));
        diagnostic.ShouldNotBeNull();
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
    }

    private static TestAdditionalText SourceFile()
    {
        return LanguageFile("Localization/en-US.xlf", SourceXliff);
    }

    private static TestAdditionalText ReferencedSourceFile(
        string moduleId = "External.Package",
        string unitKey = "Title")
    {
        var content = SourceXliff.Replace("id=\"Title\"", $"id=\"{unitKey}\"");
        return new TestAdditionalText(
            $"packages/{moduleId}/Localization/en-US.xlf",
            content,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_metadata.AdditionalFiles.AtomUILanguage"] = "true",
                ["build_metadata.AdditionalFiles.AtomUILanguageModuleId"] = moduleId,
                ["build_metadata.AdditionalFiles.AtomUILanguageSourceKind"] = "ModuleBuiltIn",
                ["build_metadata.AdditionalFiles.AtomUILanguageSourceIdentity"] = moduleId
            });
    }

    private static TestAdditionalText TargetFile(
        string language,
        string title,
        string itemCount,
        string? path = null)
    {
        return LanguageFile(
            path ?? $"Localization/{language}.xlf",
            TargetXliff(language, title, itemCount));
    }

    private static TestAdditionalText LanguageFile(
        string path,
        string text)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["build_metadata.AdditionalFiles.AtomUILanguage"] = "true",
            ["build_metadata.AdditionalFiles.AtomUILanguageSourceKind"] = "ModuleBuiltIn",
            ["build_metadata.AdditionalFiles.AtomUILanguageSourceIdentity"] = "Test.Package"
        };
        return new TestAdditionalText(
            path,
            text,
            metadata);
    }

    private static TestAdditionalText StaticPackFile(
        string moduleId = "External.Package",
        string unitKey = "Title",
        string contractValidation = "Verified",
        string titleSource = "Title")
    {
        var content = TargetXliff("zh-CN", "标题", "项目 {0}")
            .Replace("id=\"Title\"", $"id=\"{unitKey}\"")
            .Replace("<source>Title</source>", $"<source>{titleSource}</source>");
        var sourceFingerprint = LanguageSourceFingerprint.Compute(
            Xliff21Parser.Parse(content).Document!);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["build_metadata.AdditionalFiles.AtomUILanguage"] = "true",
            ["build_metadata.AdditionalFiles.AtomUILanguageSourceKind"] = "StaticLanguagePack",
            ["build_metadata.AdditionalFiles.AtomUILanguageSourceIdentity"] = "External.Package.I18n.ZhCN",
            ["build_metadata.AdditionalFiles.AtomUILanguageModuleId"] = moduleId,
            ["build_metadata.AdditionalFiles.AtomUILanguageContractValidation"] = contractValidation,
            ["build_metadata.AdditionalFiles.AtomUILanguageSourceFingerprint"] = sourceFingerprint
        };

        return new TestAdditionalText(
            "packages/External.Package.I18n.ZhCN/zh-CN.xlf",
            content,
            metadata);
    }

    private static TestAdditionalText ApplicationOverrideFile(
        bool includeItemCount = true,
        bool includeTitle = true,
        string path = "Localization/Overrides/zh-CN.xlf",
        string sourceIdentity = "TestApp")
    {
        var content = TargetXliff("zh-CN", "覆盖标题", "覆盖项目 {0}");
        if (!includeItemCount)
        {
            const string itemCountUnit =
                "<unit id=\"ItemCount\"><segment><source>Items {0}</source>" +
                "<target state=\"translated\">覆盖项目 {0}</target></segment></unit>\n";
            content = content.Replace(itemCountUnit, string.Empty);
        }
        if (!includeTitle)
        {
            const string titleUnit =
                "<unit id=\"Title\"><segment><source>Title</source>" +
                "<target state=\"translated\">覆盖标题</target></segment></unit>\n";
            content = content.Replace(titleUnit, string.Empty);
        }

        return new TestAdditionalText(
            path,
            content,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_metadata.AdditionalFiles.AtomUILanguage"] = "true",
                ["build_metadata.AdditionalFiles.AtomUILanguageSourceKind"] = "ApplicationOverride",
                ["build_metadata.AdditionalFiles.AtomUILanguageSourceIdentity"] = sourceIdentity,
                ["build_metadata.AdditionalFiles.AtomUILanguageModuleId"] = "Test.Package"
            });
    }

    private static MetadataReference CreateExternalCatalogReference(
        string assemblyName = "External.Package",
        string? moduleId = null,
        string unitKey = "Title")
    {
        var assemblyMetadata = moduleId is null
            ? string.Empty
            : $"[assembly: System.Reflection.AssemblyMetadata(\"AtomUILanguageModuleId\", \"{moduleId}\")]";
        return LocalizationGeneratorTestHost.CreateMetadataReference(
            assemblyName,
            $$"""
            {{assemblyMetadata}}

            namespace AtomUI.Localization
            {
                [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = false)]
                public sealed class LanguageCatalogAttribute : System.Attribute;
            }

            namespace TestApp.Localization
            {
                [AtomUI.Localization.LanguageCatalog]
                public enum LoginLangResourceKind
                {
                    ItemCount,
                    {{unitKey}}
                }
            }
            """);
    }

    private static MetadataReference CreateExternalModuleReference()
    {
        return LocalizationGeneratorTestHost.CreateMetadataReference(
            "External.Package.Runtime",
            """
            [assembly: System.Reflection.AssemblyMetadata("AtomUILanguageModuleId", "External.Package")]

            namespace External.Package
            {
                public sealed class Marker;
            }
            """);
    }

    private static string TargetXliff(string language, string title, string itemCount)
    {
        return $$"""
            <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US" trgLang="{{language}}">
              <file id="{{CatalogMetadataName}}">
                <unit id="ItemCount"><segment><source>Items {0}</source><target state="translated">{{itemCount}}</target></segment></unit>
                <unit id="Title"><segment><source>Title</source><target state="translated">{{title}}</target></segment></unit>
              </file>
            </xliff>
            """;
    }

    private const string CatalogMetadataName = "TestApp.Localization.LoginLangResourceKind";

    private const string CatalogSource = """
        namespace AtomUI.Localization
        {
            [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = false)]
            public sealed class LanguageCatalogAttribute : System.Attribute;
        }

        namespace TestApp.Localization
        {
            using AtomUI.Localization;

            [LanguageCatalog]
            public enum LoginLangResourceKind
            {
                ItemCount,
                Title
            }
        }
        """;

    private const string SourceXliff = """
        <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US">
          <file id="TestApp.Localization.LoginLangResourceKind">
            <unit id="ItemCount"><segment><source>Items {0}</source></segment></unit>
            <unit id="Title"><segment><source>Title</source></segment></unit>
          </file>
        </xliff>
        """;
}

internal static class TestAdditionalTextExtensions
{
    internal static LocalizationGeneratorTestHost.TestAdditionalText WithText(
        this LocalizationGeneratorTestHost.TestAdditionalText source,
        string text)
    {
        return new LocalizationGeneratorTestHost.TestAdditionalText(
            source.Path,
            text,
            source.Metadata);
    }
}
