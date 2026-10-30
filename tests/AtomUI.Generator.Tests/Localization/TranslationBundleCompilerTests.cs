using AtomUI.Generator.Localization.Catalog;
using AtomUI.Generator.Localization.Xliff;
using AtomUI.Build.Tasks.LocalizationBuild;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.Localization;

public class TranslationBundleCompilerTests
{
    [Fact]
    public void Compile_Maps_Validated_Values_To_Ordinal_Catalog_Slots()
    {
        var catalog = new LanguageCatalogInfo(
            "Installed.Module",
            "Installed.Strings",
            "Installed",
            "global::Installed.Strings",
            [
                new LanguageCatalogUnitInfo("Body", Location.None),
                new LanguageCatalogUnitInfo("Title", Location.None)
            ],
            Location.None);
        var source = Input(
            catalog,
            LanguageFileSourceKind.ModuleBuiltIn,
            SourceXliff(catalog.MetadataName));
        var target = Input(
            catalog,
            LanguageFileSourceKind.StaticLanguagePack,
            TargetXliff(catalog.MetadataName));
        var workItem = new CatalogCompilationWorkItem(
            new CatalogSymbolEntry(catalog, ownsCatalog: true),
            [Resolution(catalog, source), Resolution(catalog, target)]);
        var validation = CatalogSemanticValidator.Validate(workItem);

        validation.Diagnostics.ShouldBeEmpty();
        var compiled = TranslationBundleCompiler.Compile(validation.Input.ShouldNotBeNull());

        var bundle = compiled.Bundles.Single(bundle => bundle.Language == "zh-CN");
        bundle.Values.ShouldBe(["本文", "标题"]);
    }

    private static LanguageInputResolution Resolution(
        LanguageCatalogInfo catalog,
        LanguageFileInput input)
    {
        return new LanguageInputResolution(
            input,
            LanguageInputActivationState.Active,
            catalog);
    }

    private static LanguageFileInput Input(
        LanguageCatalogInfo catalog,
        LanguageFileSourceKind sourceKind,
        string content)
    {
        var document = Xliff21Parser.Parse(content).Document!;
        return new LanguageFileInput(
            new AdditionalLanguageFile(
                $"{sourceKind}/{document.TargetLanguage ?? "en-US"}.xlf",
                SourceText.From(content),
                document),
            catalog.ModuleId,
            sourceKind,
            $"{catalog.ModuleId}.{sourceKind}",
            sourceKind == LanguageFileSourceKind.StaticLanguagePack
                ? LanguageFileContractValidation.Deferred
                : LanguageFileContractValidation.Verified,
            sourceKind == LanguageFileSourceKind.StaticLanguagePack
                ? LanguageSourceFingerprint.Compute(document)
                : null);
    }

    private static string SourceXliff(string fileId)
    {
        return $$"""
            <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US">
              <file id="{{fileId}}">
                <unit id="Title"><segment><source>Title</source></segment></unit>
                <unit id="Body"><segment><source>Body</source></segment></unit>
              </file>
            </xliff>
            """;
    }

    private static string TargetXliff(string fileId)
    {
        return $$"""
            <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US" trgLang="zh-CN">
              <file id="{{fileId}}">
                <unit id="Title"><segment><source>Title</source><target state="final">标题</target></segment></unit>
                <unit id="Body"><segment><source>Body</source><target state="final">本文</target></segment></unit>
              </file>
            </xliff>
            """;
    }
}
