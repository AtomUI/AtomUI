using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Localization.LocalizationGeneratorTestHost;

namespace AtomUI.Generator.Tests.Localization;

public class LocalizationGeneratorDeterminismTests
{
    [Fact]
    public void Reordered_Catalogs_Files_And_Units_Preserve_Translations_And_Update_Enum_Mappings()
    {
        var first = Run(
            RuntimeSource + CatalogAThenBSource,
            LanguageFile("Localization/A/en-US.xlf", CatalogAXliff),
            LanguageFile("Localization/B/en-US.xlf", CatalogBXliff));
        var second = Run(
            RuntimeSource + CatalogBThenASource,
            LanguageFile("Localization/B/en-US.xlf", ReverseUnits(CatalogBXliff)),
            LanguageFile("Localization/A/en-US.xlf", ReverseUnits(CatalogAXliff)));

        first.Diagnostics.ShouldBeEmpty();
        second.Diagnostics.ShouldBeEmpty();
        var before = GetSources(first);
        var after = GetSources(second);
        before.Remove("GeneratedLanguageModuleRegistration.g.cs");
        after.Remove("GeneratedLanguageModuleRegistration.g.cs");
        after.ShouldBe(before);
        var firstData = ReadModule(first);
        var secondData = ReadModule(second);
        for (var index = 0; index < firstData.Catalogs.Length; index++)
        {
            secondData.Catalogs[index].CatalogId.ShouldBe(firstData.Catalogs[index].CatalogId);
            secondData.Catalogs[index].Units.Select(static unit => unit.Key)
                .ShouldBe(firstData.Catalogs[index].Units.Select(static unit => unit.Key));
            secondData.Catalogs[index].Units.Select(static unit => unit.EnumValue)
                .ShouldBe(firstData.Catalogs[index].Units.Select(static unit => 1 - unit.EnumValue));
            secondData.Bundles[index].Values.ShouldBe(firstData.Bundles[index].Values);
        }
    }

    [Fact]
    public void Updating_One_Xliff_Changes_Only_The_Module_Data_Source()
    {
        var original = LanguageFile("Localization/A/en-US.xlf", CatalogAXliff);
        var replacement = LanguageFile(
            "Localization/A/en-US.xlf",
            CatalogAXliff.Replace("Alpha", "Changed alpha"));
        var execution = RunWithUpdatedAdditionalText(
            RuntimeSource + CatalogAThenBSource,
            original,
            replacement,
            LanguageFile("Localization/B/en-US.xlf", CatalogBXliff));
        var first = execution.FirstRun.Results.ShouldHaveSingleItem();
        var second = execution.SecondRun.Results.ShouldHaveSingleItem();

        first.Diagnostics.ShouldBeEmpty();
        second.Diagnostics.ShouldBeEmpty();
        var before = GetSources(first);
        var after = GetSources(second);
        var changedHints = before.Keys.Where(hint => before[hint] != after[hint]).ToArray();

        changedHints.ShouldBe([
            "GeneratedLanguageModuleRegistration.g.cs"
        ]);
        after["GeneratedApplicationLanguageBootstrap.g.cs"]
            .ShouldBe(before["GeneratedApplicationLanguageBootstrap.g.cs"]);

        var languageFileReasons = second.TrackedSteps["LocalizationLanguageFiles"]
                                        .SelectMany(static step => step.Outputs)
                                        .Select(static output => output.Reason)
                                        .ToArray();
        languageFileReasons.Count(static reason =>
            reason == IncrementalStepRunReason.Modified).ShouldBe(1);
        languageFileReasons.ShouldContain(IncrementalStepRunReason.Cached);
        second.TrackedSteps["LocalizationCompilation"]
              .SelectMany(static step => step.Outputs)
              .Select(static output => output.Reason)
              .ShouldContain(IncrementalStepRunReason.Modified);
    }

    private static AtomUI.Localization.Data.LanguageModuleData ReadModule(GeneratorRunResult result)
    {
        var tree = result.GeneratedSources.Single(static source =>
            source.HintName == "GeneratedLanguageModuleRegistration.g.cs").SyntaxTree;
        var value = tree.GetRoot(TestContext.Current.CancellationToken).DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax>()
            .Single(static variable => variable.Identifier.ValueText == "Data").Initializer!.Value;
        return AtomUI.Localization.Data.LanguageModuleDataCodec.Read("Test.Package",
            ((Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)value).Token.ValueText);
    }

    private static SortedDictionary<string, string> GetSources(GeneratorRunResult result)
    {
        return new SortedDictionary<string, string>(
            result.GeneratedSources.ToDictionary(
                static source => source.HintName,
                static source => source.SourceText.ToString(),
                StringComparer.Ordinal),
            StringComparer.Ordinal);
    }

    private static TestAdditionalText LanguageFile(string path, string content)
    {
        return new TestAdditionalText(
            path,
            content,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_metadata.AdditionalFiles.AtomUILanguage"] = "true",
                ["build_metadata.AdditionalFiles.AtomUILanguageSourceKind"] = "ModuleBuiltIn",
                ["build_metadata.AdditionalFiles.AtomUILanguageSourceIdentity"] = "Test.Package"
            });
    }

    private static string ReverseUnits(string source)
    {
        var firstUnitStart = source.IndexOf("    <unit", StringComparison.Ordinal);
        var firstUnitEnd = source.IndexOf("</unit>", firstUnitStart, StringComparison.Ordinal) +
                           "</unit>".Length;
        var secondUnitStart = source.IndexOf("    <unit", firstUnitEnd, StringComparison.Ordinal);
        var secondUnitEnd = source.IndexOf("</unit>", secondUnitStart, StringComparison.Ordinal) +
                            "</unit>".Length;
        var firstUnit = source.Substring(firstUnitStart, firstUnitEnd - firstUnitStart);
        var secondUnit = source.Substring(secondUnitStart, secondUnitEnd - secondUnitStart);
        return source.Substring(0, firstUnitStart) + secondUnit + "\n" + firstUnit +
               source.Substring(secondUnitEnd);
    }

    private const string RuntimeSource = """
        namespace Avalonia
        {
            public abstract class Application { }
        }

        namespace AtomUI.Localization
        {
            [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = false)]
            public sealed class LanguageCatalogAttribute : System.Attribute;
        }
        """;

    private const string CatalogAThenBSource = """
        namespace TestApp.Localization
        {
            [AtomUI.Localization.LanguageCatalog]
            public enum CatalogALangResourceKind
            {
                Count,
                Title
            }

            [AtomUI.Localization.LanguageCatalog]
            public enum CatalogBLangResourceKind
            {
                Confirm,
                Cancel
            }
        }

        namespace TestApp
        {
            public partial class App : Avalonia.Application { }
        }
        """;

    private const string CatalogBThenASource = """
        namespace TestApp.Localization
        {
            [AtomUI.Localization.LanguageCatalog]
            public enum CatalogBLangResourceKind
            {
                Cancel,
                Confirm
            }

            [AtomUI.Localization.LanguageCatalog]
            public enum CatalogALangResourceKind
            {
                Title,
                Count
            }
        }

        namespace TestApp
        {
            public partial class App : Avalonia.Application { }
        }
        """;

    private const string CatalogAXliff = """
        <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US">
          <file id="TestApp.Localization.CatalogALangResourceKind">
            <unit id="Count"><segment><source>Alpha {0}</source></segment></unit>
            <unit id="Title"><segment><source>Alpha</source></segment></unit>
          </file>
        </xliff>
        """;

    private const string CatalogBXliff = """
        <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US">
          <file id="TestApp.Localization.CatalogBLangResourceKind">
            <unit id="Confirm"><segment><source>Confirm</source></segment></unit>
            <unit id="Cancel"><segment><source>Cancel</source></segment></unit>
          </file>
        </xliff>
        """;
}
