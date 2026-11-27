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
            if (ownPackage is not null)
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
            var fields = new List<string> { "startup-image", StartupImageFormat.Id.ToString(System.Globalization.CultureInfo.InvariantCulture) };
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
            var valuePackages = ReadValuePackages(compilation);
            if (valuePackages.Length != 0)
            {
                fields.Add("control-value-packages");
                fields.AddRange(valuePackages);
            }
            var stamp = ThemeBuildContracts.Digest(fields).Substring(0, 16);
            var source = new StringBuilder("// <auto-generated />\n#nullable enable\nnamespace " +
                GeneratedCodeNamespace.ForAssembly(compilation.AssemblyName ?? "Application") + ";\n");
            source.AppendLine("internal static class GeneratedApplicationStartupImage\n{");
            source.Append("    internal static readonly global::AtomUI.Registration.StartupImage Instance = new(").Append(StartupImageFormat.Id).Append("U, 0x")
                .Append(stamp).AppendLine("UL,");
            WritePackageProofs(source, encoded);
            source.AppendLine(",");
            WriteStrings(source, identities);
            source.AppendLine(",");
            WritePairs(source, resources);
            source.AppendLine(",");
            WriteStrings(source, languages.Keys);
            source.AppendLine(",");
            WritePairs(source, languageModules);
            if (valuePackages.Length != 0)
            {
                source.AppendLine(",\n        controlValuePackages: [");
                foreach (var member in valuePackages)
                {
                    source.Append("            ").Append(member).AppendLine(",");
                }
                source.Append("        ]");
            }
            source.AppendLine(");");
            // Packed literals keep this type initializer to a few calls; one IL sequence per entry made
            // JIT-compiled startup pay several milliseconds for it on the UI thread.
            source.AppendLine("    private static global::System.Collections.Generic.KeyValuePair<string, string>[] Pairs(int count, string data)\n    {");
            source.AppendLine("        var fields = Strings(count * 2, data);");
            source.AppendLine("        var pairs = new global::System.Collections.Generic.KeyValuePair<string, string>[count];");
            source.AppendLine("        for (var index = 0; index < count; index++)\n        {");
            source.AppendLine("            pairs[index] = new(fields[2 * index], fields[2 * index + 1]);\n        }");
            source.AppendLine("        return pairs;\n    }");
            source.AppendLine("    private static string[] Strings(int count, string data)\n    {");
            source.AppendLine("        if (count == 0)\n        {\n            return global::System.Array.Empty<string>();\n        }");
            source.Append("        var fields = data.Split(").Append(L(PackedSeparator.ToString())).AppendLine(");");
            source.AppendLine("        if (fields.Length != count)\n        {");
            source.AppendLine("            throw new global::System.InvalidOperationException(\"Corrupt application startup image; rebuild the application.\");\n        }");
            source.AppendLine("        return fields;\n    }\n}");
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

    private static string[] ReadValuePackages(Compilation compilation) =>
        compilation.SourceModule.ReferencedAssemblySymbols.SelectMany(static assembly => assembly.GetAttributes())
            .Where(static attribute => attribute.AttributeClass?.ToDisplayString() ==
                                       "AtomUI.Theme.Languages.ControlValuePackageRegistrationAttribute")
            .Select(static attribute =>
            {
                var arguments = attribute.ConstructorArguments;
                if (arguments.Length is 2 or 3 && arguments[0].Value is INamedTypeSymbol owner &&
                    arguments[1].Value is string name &&
                    owner.GetMembers(name).OfType<IPropertySymbol>().SingleOrDefault(static property =>
                        property.IsStatic && property.DeclaredAccessibility == Accessibility.Public &&
                        property.Type.ToDisplayString() == "AtomUI.Theme.Languages.ControlValuePackage") is { } property)
                {
                    var ownerName = owner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (arguments.Length == 2 || arguments[2].IsNull)
                    {
                        return ownerName + "." + property.Name;
                    }
                    if (arguments[2].Value is string resolver &&
                        owner.GetMembers(resolver).OfType<IMethodSymbol>().Any(static method =>
                            method.IsStatic && !method.IsGenericMethod && method.Parameters.Length == 0 &&
                            method.DeclaredAccessibility == Accessibility.Public &&
                            method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                .TrimEnd('?') == "global::AtomUI.Theme.Languages.ControlValuePackage"))
                    {
                        return ownerName + "." + resolver + "()";
                    }
                    throw new InvalidDataException("Invalid control value package resolver.");
                }
                throw new InvalidDataException("Invalid control value package registration.");
            })
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static member => member, StringComparer.Ordinal).ToArray();

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

    private const char PackedSeparator = '\u001F';

    private static void WritePackageProofs(StringBuilder source, IEnumerable<KeyValuePair<string, string>> packages)
    {
        source.AppendLine("        [");
        foreach (var package in packages)
        {
            source.Append("            new global::AtomUI.Registration.StartupPackageProof(")
                .Append(L(package.Key)).Append(", ").Append(StartupManifestCodec.Version).Append("U, ")
                .Append(package.Value.Length).Append("U, 0, ");
            StartupManifestWriter.AppendManifestLiteral(source, package.Value);
            source.AppendLine("),");
        }
        source.Append("        ]");
    }

    private static void WritePairs(StringBuilder source, IEnumerable<KeyValuePair<string, string>> values)
    {
        var pairs = values.ToArray();
        source.Append("        Pairs(").Append(pairs.Length).Append(", ")
              .Append(L(Pack(pairs.SelectMany(pair => new[] { pair.Key, pair.Value })))).Append(')');
    }

    private static void WriteStrings(StringBuilder source, IEnumerable<string> values)
    {
        var strings = values.ToArray();
        source.Append("        Strings(").Append(strings.Length).Append(", ").Append(L(Pack(strings))).Append(')');
    }

    // The generated Strings(count, data) splits on the separator and checks the count, so empty fields stay exact.
    private static string Pack(IEnumerable<string> fields)
    {
        var values = fields.ToArray();
        foreach (var value in values)
        {
            if (value.IndexOf(PackedSeparator) >= 0)
            {
                throw new InvalidDataException("Startup image field '" + value + "' contains the packed separator.");
            }
        }
        return string.Join(PackedSeparator.ToString(), values);
    }

    private static string L(string value) => RegistrationNames.Literal(value);
}
