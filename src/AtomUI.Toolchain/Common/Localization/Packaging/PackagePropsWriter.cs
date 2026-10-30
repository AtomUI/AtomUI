using System.Xml.Linq;

namespace AtomUI.Build.Tasks.LocalizationBuild;

internal static class PackagePropsWriter
{
    internal static string Write(
        string packageId,
        IReadOnlyList<LanguagePackageCatalogEntry> catalogs,
        string sourceKind = "StaticLanguagePack")
    {
        var itemGroup = new XElement(
            "ItemGroup",
            // XLIFF files live in contentFiles so the generated props can read them from the package.
            // NuGet also adds those files as visible None items; remove the compiler-only projection
            // while leaving the package payload available to AtomUILanguage.
            new XElement(
                "None",
                new XAttribute(
                    "Remove",
                    "$(MSBuildThisFileDirectory)../contentFiles/any/any/**/*.xlf")),
            new XElement(
                "None",
                new XAttribute(
                    "Remove",
                    "$(MSBuildThisFileDirectory)../contentFiles/any/any/AtomUI.LanguagePack.xml")));
        foreach (var catalog in catalogs
                                        .OrderBy(static entry => entry.ModuleId, StringComparer.Ordinal)
                                        .ThenBy(static entry => entry.CatalogId, StringComparer.Ordinal))
        {
            var element = new XElement(
                "AtomUILanguage",
                new XAttribute(
                    "Include",
                    "$(MSBuildThisFileDirectory)../contentFiles/any/any/" + catalog.Path),
                new XAttribute("AtomUILanguageSourceKind", sourceKind),
                new XAttribute("AtomUILanguageSourceIdentity", packageId),
                new XAttribute("AtomUILanguageModuleId", catalog.ModuleId),
                new XAttribute("AtomUILanguageContractValidation", catalog.ContractValidation),
                new XAttribute("AtomUILanguagePackagePath", catalog.Path),
                new XAttribute("AtomUILanguageSourceFingerprint", catalog.SourceFingerprint));
            itemGroup.Add(element);
        }

        return DeterministicXml.Write(
            new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("Project", itemGroup)));
    }
}
