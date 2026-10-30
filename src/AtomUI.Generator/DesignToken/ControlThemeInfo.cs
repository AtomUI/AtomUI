using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal sealed class ControlThemeInfo
{
    internal ControlThemeInfo(
        string controlNamespace,
        string controlName,
        string controlMetadataName,
        string controlTypeName,
        bool hasDescriptor,
        ControlTokenInfo? ownToken)
    {
        ControlNamespace = controlNamespace;
        ControlName = controlName;
        ControlMetadataName = controlMetadataName;
        ControlTypeName = controlTypeName;
        HasDescriptor = hasDescriptor;
        OwnToken = ownToken;
    }

    internal string ControlNamespace { get; }
    internal string ControlName { get; }
    internal string ControlMetadataName { get; }
    internal string ControlTypeName { get; }
    internal bool HasDescriptor { get; }
    internal ControlTokenInfo? OwnToken { get; }
    internal bool HasOwnToken => OwnToken is not null;
    internal string TokenKindType => $"{ControlName}TokenKind";
    internal string TokenKeyType => $"{ControlName}TokenKey";
    internal string TokensType => $"{ControlName}Tokens";
    internal string ResourceExtensionType => $"{ControlName}TokenResourceExtension";
    internal IEnumerable<TokenName> OwnTokens => OwnToken is null
        ? Array.Empty<TokenName>()
        : OwnToken.Tokens;
    internal IEnumerable<SchemaTokenInfo> OwnSchemaTokens => OwnToken is null
        ? Array.Empty<SchemaTokenInfo>()
        : OwnToken.SchemaTokens;

}

internal static class ControlThemeModelBuilder
{
    private const string ControlBaseType = "global::Avalonia.Controls.Control";

    internal static IReadOnlyList<ControlThemeInfo> Build(
        Compilation compilation,
        IEnumerable<ControlTokenInfo> ownTokens,
        ISet<string> globalTokenNames,
        Action<Diagnostic> reportDiagnostic)
    {
        var controls = GetPublicControls(compilation.Assembly.GlobalNamespace).ToArray();
        var result = new Dictionary<string, ControlThemeInfo>(StringComparer.Ordinal);
        var reportedTokenDiagnostics = new HashSet<string>(StringComparer.Ordinal);

        foreach (var ownToken in ownTokens)
        {
            foreach (var diagnostic in ownToken.Diagnostics)
            {
                if (reportedTokenDiagnostics.Add(GetDiagnosticKey(diagnostic)))
                {
                    reportDiagnostic(diagnostic);
                }
            }
            if (!ownToken.IsValid)
            {
                continue;
            }
            if (!ownToken.IsTerminal)
            {
                continue;
            }

            var conflictingTokenNames = ownToken.Tokens
                                                .Select(static token => token.Name)
                                                .Where(globalTokenNames.Contains)
                                                .OrderBy(static name => name, StringComparer.Ordinal)
                                                .ToArray();
            foreach (var tokenName in conflictingTokenNames)
            {
                reportDiagnostic(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.ControlTokenGlobalNameConflict,
                    ownToken.DeclarationLocation,
                    ownToken.ControlName,
                    tokenName));
            }
            if (conflictingTokenNames.Length != 0)
            {
                continue;
            }

            var matches = FindMatchingControls(compilation, controls, ownToken).ToArray();
            if (matches.Length == 0)
            {
                reportDiagnostic(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.ControlTokenMissingControl,
                    ownToken.DeclarationLocation,
                    ownToken.TokenName,
                    ownToken.ControlName));
                continue;
            }
            if (matches.Length > 1)
            {
                reportDiagnostic(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.ControlTokenAmbiguousControl,
                    ownToken.DeclarationLocation,
                    ownToken.TokenName,
                    ownToken.ControlName));
                continue;
            }

            var control = matches[0];
            var info = CreateInfo(
                control,
                ownToken,
                hasDescriptor: true);
            result[GetControlKey(control)] = info;
        }

        return result.Values.OrderBy(static info => info.ControlName, StringComparer.Ordinal).ToArray();
    }

    private static string GetDiagnosticKey(Diagnostic diagnostic)
    {
        var location = diagnostic.Location;
        var locationKey = location == Location.None
            ? "<none>"
            : $"{location.SourceTree?.FilePath ?? location.GetLineSpan().Path}:{location.SourceSpan.Start}:{location.SourceSpan.Length}";
        return $"{diagnostic.Id}\u001f{locationKey}\u001f{diagnostic.GetMessage()}";
    }

    private static IEnumerable<INamedTypeSymbol> FindMatchingControls(
        Compilation compilation,
        IReadOnlyList<INamedTypeSymbol> sourceControls,
        ControlTokenInfo ownToken)
    {
        var metadataName = string.IsNullOrWhiteSpace(ownToken.TokenNamespace)
            ? ownToken.ControlName!
            : $"{ownToken.TokenNamespace}.{ownToken.ControlName}";
        var exact = compilation.GetTypeByMetadataName(metadataName);
        if (exact is not null && RegistrationModelBuilder.IsControl(exact) && RegistrationModelBuilder.Accessible(exact))
        {
            return [exact];
        }

        var localMatches = RegistrationModelBuilder.Types(compilation.Assembly.GlobalNamespace)
            .Where(type => type.Name == ownToken.ControlName && type.ContainingNamespace.ToDisplayString() == ownToken.TokenNamespace &&
                RegistrationModelBuilder.IsControl(type) && RegistrationModelBuilder.Accessible(type)).ToArray();
        return localMatches.Length != 0 ? localMatches : FindPublicControlsByName(compilation, sourceControls, ownToken.ControlName!);
    }

    internal static IEnumerable<INamedTypeSymbol> FindPublicControlsByName(
        Compilation compilation,
        IReadOnlyList<INamedTypeSymbol> sourceControls,
        string name)
    {
        var sourceMatches = sourceControls
            .Where(control => string.Equals(control.Name, name, StringComparison.Ordinal))
            .ToArray();
        if (sourceMatches.Length != 0)
        {
            return sourceMatches;
        }

        var referencedMatches = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            AddPublicControlsByName(assembly.GlobalNamespace, name, referencedMatches);
        }
        var atomUIMatches = referencedMatches
            .Where(static type => ThemeGeneratorOptions.IsBuiltInControlCatalog(type.ContainingAssembly))
            .ToArray();
        if (atomUIMatches.Length != 0)
        {
            return atomUIMatches;
        }

        var result = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var namespaceName in new[]
                 {
                     "Avalonia.Controls",
                     "Avalonia.Controls.Primitives"
                 })
        {
            var type = compilation.GetTypeByMetadataName($"{namespaceName}.{name}");
            if (type is not null && IsPublicControl(type))
            {
                result.Add(type);
            }
        }

        return result.Count != 0 ? result : referencedMatches;
    }

    private static void AddPublicControlsByName(INamespaceSymbol ns, string name, ISet<INamedTypeSymbol> result)
    {
        foreach (var type in RegistrationModelBuilder.Types(ns).Where(type => type.Name == name && IsPublicControl(type)))
        {
            result.Add(type);
        }
    }

    private static ControlThemeInfo CreateInfo(
        INamedTypeSymbol control,
        ControlTokenInfo? ownToken,
        bool hasDescriptor)
    {
        return new ControlThemeInfo(
            ownToken?.TokenNamespace ??
            (control.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : control.ContainingNamespace.ToDisplayString()),
            control.Name,
            GetMetadataName(control),
            control.ToDisplayString(GeneratorSymbolDisplay.FullyQualifiedType),
            hasDescriptor,
            ownToken);
    }

    private static string GetMetadataName(INamedTypeSymbol symbol)
    {
        var typeNames = new Stack<string>();
        for (var current = symbol; current is not null; current = current.ContainingType)
        {
            typeNames.Push(current.MetadataName);
        }

        var typeName = string.Join("+", typeNames);
        return symbol.ContainingNamespace.IsGlobalNamespace
            ? typeName
            : symbol.ContainingNamespace.ToDisplayString() + "." + typeName;
    }

    internal static IEnumerable<INamedTypeSymbol> GetPublicControls(INamespaceSymbol ns) =>
        RegistrationModelBuilder.Types(ns).Where(IsPublicControl);

    internal static bool IsPublicControl(INamedTypeSymbol type)
    {
        if (type.DeclaredAccessibility != Accessibility.Public || !RegistrationModelBuilder.Accessible(type))
        {
            return false;
        }

        for (var current = type; current is not null; current = current.BaseType)
        {
            if (string.Equals(
                    current.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    ControlBaseType,
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetControlKey(INamedTypeSymbol control)
    {
        return control.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
