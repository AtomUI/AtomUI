using System.Collections.Immutable;
using System.Text;
using AtomUI.Generator.Diagnostics;
using AtomUI.Generator.Localization;
using AtomUI.SourceGeneration;
using AtomUI.Startup.Data;
using AtomUI.Theme.Definitions.Compiled;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator;

internal static class StartupImageWriter
{
    internal static void Write(GenerationOutput output, Compilation compilation, RegistrationPackage? ownPackage,
        ImmutableArray<StartupReferenceData> references, LocalizationGenerationResult localization,
        ThemeBuildContracts? contracts, CompiledThemeManifest? ownTheme)
    {
        if (compilation.GetTypeByMetadataName("AtomUI.Registration.IGeneratedStartupImageHost") is null)
        {
            return;
        }
        var applications = RegistrationModelBuilder.Types(compilation.Assembly.GlobalNamespace).Where(IsApplication).ToArray();
        if (applications.Length == 0)
        {
            return;
        }
        var applicationLocation = applications[0].Locations.FirstOrDefault() ?? Location.None;
        var location = applicationLocation;
        var diagnostic = AtomUIDiagnosticDescriptors.StartupManifestInvalid;
        try
        {
            var encoded = new SortedDictionary<string, string>(StringComparer.Ordinal);
            var packages = new List<StartupPackageData>();
            foreach (var reference in references)
            {
                if (reference.Error is { } error)
                {
                    throw new InvalidDataException(reference.AssemblyName + ": " + error);
                }
                if (reference.PackageId is not { } id || reference.PackageData is not { } data)
                {
                    continue;
                }
                if (reference.Version != StartupManifestCodec.Version)
                {
                    throw new InvalidDataException("Unsupported startup manifest from '" + reference.AssemblyName + "'");
                }
                var package = StartupManifestDecoder.Read("Control package", id, reference.AssemblyName, data, StartupManifestCodec.Decode);
                if (package.Id != id || package.AssemblyName != reference.AssemblyName)
                {
                    throw new InvalidDataException("Conflicting startup package identity '" + id + "' in '" + reference.AssemblyName + "'");
                }
                if (encoded.ContainsKey(id))
                {
                    diagnostic = AtomUIDiagnosticDescriptors.StartupPackageConflict;
                    throw new InvalidDataException("Duplicate startup package identity '" + id + "' in '" + reference.AssemblyName + "'");
                }
                encoded.Add(id, data);
                packages.Add(package);
            }
            diagnostic = AtomUIDiagnosticDescriptors.StartupPackageConflict;
            // Empty and assets-only own packages use exactly the same manifest as the package writer.
            if (ownPackage is { SupportsStartupManifest: true })
            {
                var groupName = "ControlPackageGroup_" + RegistrationNames.Hash(ownPackage.AssemblyIdentity);
                var groupIdentity = ownPackage.AssemblyIdentity + "|" + ownPackage.Namespace + "." + groupName;
                var keys = ownPackage.Controls.SelectMany(control => control.Triggers.Select(trigger =>
                    new TypeMapRegistrationWriter.TypeMapEntry(control, trigger,
                        RegistrationNames.TypeMapKey(groupIdentity, control.Type.AssemblyQualifiedName, trigger)))).ToArray();
                var data = StartupManifestWriter.Create(ownPackage, keys);
                encoded.Add(data.Id, StartupManifestCodec.Encode(data));
                packages.Add(data);
            }
            StartupManifestLinker.Validate(packages, contracts?.Tokens.Values.Where(token => token.Catalog.Length == 0)
                .Select(token => token.Name));
            diagnostic = AtomUIDiagnosticDescriptors.StartupThemeContractMismatch;
            var themes = StartupThemeLinker.Validate(references, ownPackage?.Id ?? compilation.AssemblyName ?? "Application", ownTheme, contracts, packages, compilation.AssemblyName);
            diagnostic = AtomUIDiagnosticDescriptors.StartupLanguageContractMismatch;
            var languages = StartupLanguageDeclarations.Read(compilation);
            var languageModules = StartupLanguageLinker.Validate(references, localization, languages.Keys,
                language => location = languages[language] != Location.None ? languages[language] : applicationLocation);
            location = applicationLocation;
            var identities = packages.SelectMany(package => package.Controls).Where(control => control.HasTokenDescriptor)
                .OrderBy(control => control.Catalog, StringComparer.Ordinal).ThenBy(control => control.Id, StringComparer.Ordinal)
                .Select(control => control.Identity).ToArray();
            diagnostic = AtomUIDiagnosticDescriptors.StartupPackageConflict;
            var resources = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var package in packages)
            {
                var controlKeys = package.Controls.Where(control => control.HasTokenDescriptor || control.HasSemantic)
                    .SelectMany(control => new[] { "t:" + (control.AssemblyName ?? package.AssemblyName) + "|" + control.TypeName, "i:" + control.Identity });
                foreach (var key in package.Assets.SelectMany(asset => asset.Exports).Concat(controlKeys))
                {
                    if (resources.TryGetValue(key, out var owner) && owner != package.Id)
                    {
                        throw new InvalidDataException("Conflicting resource ownership '" + key + "' in packages '" + owner + "' and '" + package.Id + "'");
                    }
                    resources[key] = package.Id;
                }
            }
            var fields = new List<string> { "startup-image-v2" };
            void Add(string kind, IEnumerable<KeyValuePair<string, string>> values)
            {
                fields.Add(kind);
                foreach (var pair in values)
                {
                    fields.Add(pair.Key);
                    fields.Add(pair.Value);
                }
            }
            Add("packages", encoded);
            Add("themes", themes);
            Add("languages", languageModules);
            fields.AddRange(languages.Keys);
            if (contracts is not null)
            {
                fields.Add("owners");
                fields.AddRange(contracts.Owners.OrderBy(pair => pair.Key, StringComparer.Ordinal).SelectMany(pair => pair.Value.Fields));
                fields.Add("tokens");
                fields.AddRange(contracts.Tokens.OrderBy(pair => pair.Key, StringComparer.Ordinal).SelectMany(pair => pair.Value.Fields));
            }
            var stamp = ThemeBuildContracts.Digest(fields).Substring(0, 16);
            var source = new StringBuilder("// <auto-generated />\n#nullable enable\nnamespace " +
                GeneratedCodeNamespace.ForAssembly(compilation.AssemblyName ?? "Application") + ";\n");
            source.AppendLine("internal static class GeneratedApplicationStartupImage\n{");
            source.Append("    internal static readonly global::AtomUI.Registration.StartupImage Instance = new(").Append(StartupManifestCodec.Version).Append(", 0x")
                .Append(stamp).AppendLine("UL,");
            WritePairs(source, encoded);
            source.AppendLine(",");
            WriteStrings(source, identities);
            source.AppendLine(",");
            WritePairs(source, resources);
            source.AppendLine(",");
            WriteStrings(source, languages.Keys);
            source.AppendLine(",");
            WritePairs(source, languageModules);
            source.AppendLine(");\n}");
            output.AddSource("GeneratedApplicationStartupImage.g.cs", GeneratedSourceText.From(source.ToString()));
            foreach (var app in applications.Where(app => app.IsGenericType || app.ContainingType is not null ||
                         app.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() is not ClassDeclarationSyntax declaration || !declaration.Modifiers.Any(SyntaxKind.PartialKeyword))))
            {
                output.ReportDiagnostic(Diagnostic.Create(AtomUIDiagnosticDescriptors.StartupImageManualHost,
                    app.Locations.FirstOrDefault() ?? Location.None, app.ToDisplayString()));
            }
        }
        catch (Exception error) when (error is InvalidDataException || error is FormatException || error is ArgumentException || error is EndOfStreamException)
        {
            output.ReportDiagnostic(Diagnostic.Create(diagnostic, location, error.Message));
            WriteFailure(output, compilation, diagnostic.Id, error.Message);
        }
    }

    private static void WriteFailure(GenerationOutput output, Compilation compilation, string diagnosticId, string message)
    {
        // The independent Application hook still references this symbol. Keep that reference valid,
        // but never provide a usable image if the primary link diagnostic is suppressed.
        var source = new StringBuilder("// <auto-generated />\n#nullable enable\nnamespace " +
            GeneratedCodeNamespace.ForAssembly(compilation.AssemblyName ?? "Application") + ";\n");
        source.AppendLine("internal static class GeneratedApplicationStartupImage\n{");
        source.Append("    internal static global::AtomUI.Registration.StartupImage Instance => throw new global::System.InvalidOperationException(")
            .Append(L("Application startup image is unavailable because " + diagnosticId + ": " + message)).AppendLine(");\n}");
        output.AddSource("GeneratedApplicationStartupImage.g.cs", GeneratedSourceText.From(source.ToString()));
    }

    private static bool IsApplication(INamedTypeSymbol type)
    {
        if (type.IsAbstract)
        {
            return false;
        }
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == "Avalonia.Application")
            {
                return true;
            }
        }
        return false;
    }

    private static void WritePairs(StringBuilder source, IEnumerable<KeyValuePair<string, string>> values)
    {
        source.AppendLine("        new global::System.Collections.Generic.KeyValuePair<string, string>[]\n        {");
        foreach (var pair in values)
        {
            source.Append("            new(").Append(L(pair.Key)).Append(", ").Append(L(pair.Value)).AppendLine("),");
        }
        source.Append("        }");
    }

    private static void WriteStrings(StringBuilder source, IEnumerable<string> values)
    {
        source.AppendLine("        new string[]\n        {");
        foreach (var value in values)
        {
            source.Append("            ").Append(L(value)).AppendLine(",");
        }
        source.Append("        }");
    }

    private static string L(string value) => RegistrationNames.Literal(value);
}
