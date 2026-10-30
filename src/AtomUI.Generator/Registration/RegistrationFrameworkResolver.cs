using AtomUI.Build.Tasks.Registration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator;

// Framework choice belongs to the consuming compilation, not to the presence of similarly named
// types. This helper returns no symbols to the incremental graph and owns no dependency analysis.
internal static class RegistrationFrameworkResolver
{
    internal static bool TryResolve(Compilation compilation, AnalyzerConfigOptions options,
        out RegistrationFrameworkFamily family, out string error)
    {
        options.TryGetValue("build_property.TargetFramework", out var framework);
        options.TryGetValue("build_property.TargetFrameworkIdentifier", out var identifier);
        options.TryGetValue("build_property.TargetFrameworkVersion", out var versionText);
        var objectType = compilation.GetSpecialType(SpecialType.System_Object);
        var core = objectType.ContainingAssembly;
        int targetMajor;
        if (!string.IsNullOrWhiteSpace(framework) || !string.IsNullOrWhiteSpace(identifier) || !string.IsNullOrWhiteSpace(versionText))
        {
            if (!TryDeclaredFramework(framework, identifier, versionText, out family, out targetMajor))
            {
                error = $"target framework '{framework}' ({identifier}, {versionText}) is unsupported or inconsistent; registration requires net8.0 compatibility or .NETCoreApp 10.0+ official TypeMap";
                return false;
            }
        }
        else
        {
            // Standalone Roslyn callers have no MSBuild target properties. Infer only from the
            // actual System.Object definition's official framework identity, never a missing API.
            if (!OfficialObject(objectType, 0))
            {
                family = RegistrationFrameworkFamily.Unsupported;
                error = "target framework is unspecified and the System.Object framework identity cannot be established";
                return false;
            }
            targetMajor = core.Identity.Version.Major;
            family = RegistrationFrameworkPolicy.Classify(".NETCoreApp", targetMajor + "." + core.Identity.Version.Minor);
            if (family == RegistrationFrameworkFamily.Unsupported)
            {
                error = "unsupported System.Object framework identity: " + core.Identity.GetDisplayName();
                return false;
            }
        }
        if (!OfficialObject(objectType, targetMajor))
        {
            error = $"target framework major {targetMajor} requires the matching official System.Object framework definition; actual identity: " + core?.Identity.GetDisplayName();
            return false;
        }
        if (family == RegistrationFrameworkFamily.Net8Compatibility)
        {
            error = string.Empty;
            return true;
        }
        return ValidateOfficialTypeMap(compilation, targetMajor, out error);
    }

    private static bool TryDeclaredFramework(string? framework, string? identifier, string? versionText,
        out RegistrationFrameworkFamily family, out int targetMajor)
    {
        family = RegistrationFrameworkFamily.Unsupported;
        targetMajor = 0;
        var hasIdentity = !string.IsNullOrWhiteSpace(identifier) || !string.IsNullOrWhiteSpace(versionText);
        if (hasIdentity)
        {
            family = RegistrationFrameworkPolicy.Classify(identifier, versionText);
            if (family == RegistrationFrameworkFamily.Unsupported || !RegistrationFrameworkPolicy.TryParseVersion(versionText, out var version))
            {
                return false;
            }
            targetMajor = version.Major;
        }
        if (!string.IsNullOrWhiteSpace(framework))
        {
            var shortName = framework!.Split('-')[0];
            if (!shortName.StartsWith("net", StringComparison.OrdinalIgnoreCase) ||
                !RegistrationFrameworkPolicy.TryParseVersion(shortName.Substring(3), out var shortVersion))
            {
                return false;
            }
            var shortFamily = RegistrationFrameworkPolicy.Classify(".NETCoreApp", shortVersion.ToString());
            if (shortFamily == RegistrationFrameworkFamily.Unsupported ||
                hasIdentity && (!RegistrationFrameworkPolicy.TryParseVersion(versionText, out var declaredVersion) ||
                    shortVersion.Major != declaredVersion.Major || shortVersion.Minor != declaredVersion.Minor))
            {
                return false;
            }
            family = shortFamily;
            targetMajor = shortVersion.Major;
        }
        return family != RegistrationFrameworkFamily.Unsupported;
    }

    private static bool ValidateOfficialTypeMap(Compilation compilation, int targetMajor, out string error)
    {
        var type = compilation.GetTypeByMetadataName("System.Type");
        var attribute = compilation.GetTypeByMetadataName("System.Attribute");
        var dictionary = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyDictionary`2");
        if (!Official(type, targetMajor) || !Official(attribute, targetMajor) || !Official(dictionary, targetMajor))
        {
            error = "official TypeMap signature requires framework System.Type, System.Attribute and IReadOnlyDictionary<TKey,TValue>";
            return false;
        }
        var map = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.TypeMapAttribute`1");
        var target = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.TypeMapAssemblyTargetAttribute`1");
        var mapping = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.TypeMapping");
        if (!AttributeType(map, attribute!, targetMajor) || !Constructor(map!, [compilation.GetSpecialType(SpecialType.System_String), type!, type!]))
        {
            error = "missing or incompatible official TypeMapAttribute<TGroup>.ctor(string, System.Type, System.Type)";
            return false;
        }
        if (!AttributeType(target, attribute!, targetMajor) || !Constructor(target!, [compilation.GetSpecialType(SpecialType.System_String)]))
        {
            error = "missing or incompatible official TypeMapAssemblyTargetAttribute<TGroup>.ctor(string)";
            return false;
        }
        var methods = mapping?.GetMembers("GetOrCreateExternalTypeMapping").OfType<IMethodSymbol>().Where(method =>
            method.DeclaredAccessibility == Accessibility.Public && method.Arity == 1 && method.Parameters.Length == 0).ToArray();
        if (!Official(mapping, targetMajor) || !mapping!.IsStatic || methods?.Length != 1 || !methods.All(method =>
                method.MethodKind == MethodKind.Ordinary && method.IsStatic && !method.IsAbstract && !method.IsVararg &&
                !method.ReturnsByRef && !method.ReturnsByRefReadonly &&
                !ErrorObsolete(method) && AcceptsGeneratedGroup(method.TypeParameters[0]) &&
                method.ReturnType is INamedTypeSymbol result && SymbolEqualityComparer.Default.Equals(result.OriginalDefinition, dictionary) &&
                result.TypeArguments.Length == 2 && result.TypeArguments[0].SpecialType == SpecialType.System_String &&
                SymbolEqualityComparer.Default.Equals(result.TypeArguments[1], type)))
        {
            error = "missing or incompatible official TypeMapping.GetOrCreateExternalTypeMapping<TGroup>() returning IReadOnlyDictionary<string, System.Type>";
            return false;
        }
        error = string.Empty;
        return true;
    }

    private static bool OfficialObject(INamedTypeSymbol type, int targetMajor) => type.TypeKind != TypeKind.Error &&
        type.Locations.All(location => !location.IsInSource) && type.ContainingAssembly is { } core &&
        IsFrameworkAssembly(core, targetMajor) && core.Identity.Name is "System.Runtime" or "System.Private.CoreLib";

    private static bool Official(INamedTypeSymbol? type, int targetMajor) => type is not null &&
        type.DeclaredAccessibility == Accessibility.Public && type.Locations.All(location => !location.IsInSource) &&
        IsFrameworkAssembly(type.ContainingAssembly, targetMajor) && !ErrorObsolete(type);

    private static bool AttributeType(INamedTypeSymbol? type, INamedTypeSymbol attribute, int targetMajor) =>
        Official(type, targetMajor) && type!.TypeKind == TypeKind.Class && !type.IsAbstract && type.Arity == 1 &&
        AcceptsGeneratedGroup(type.TypeParameters[0]) && SymbolEqualityComparer.Default.Equals(type.BaseType, attribute);

    private static bool Constructor(INamedTypeSymbol type, ITypeSymbol[] parameters) => type.InstanceConstructors.Count(method =>
        method.DeclaredAccessibility == Accessibility.Public && !ErrorObsolete(method) && method.Parameters.Length == parameters.Length &&
        method.Parameters.Select((parameter, index) => parameter.RefKind == RefKind.None &&
            SymbolEqualityComparer.Default.Equals(parameter.Type, parameters[index])).All(matches => matches)) == 1;

    // Generated Groups are non-generic reference types with a private constructor and no custom base/interfaces.
    private static bool AcceptsGeneratedGroup(ITypeParameterSymbol parameter) =>
        !parameter.HasConstructorConstraint && !parameter.HasValueTypeConstraint && !parameter.HasUnmanagedTypeConstraint &&
        parameter.ConstraintTypes.All(type => type.SpecialType == SpecialType.System_Object);

    private static bool ErrorObsolete(ISymbol symbol) => symbol.GetAttributes().Any(attribute =>
        attribute.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute" &&
        attribute.ConstructorArguments.Length >= 2 && attribute.ConstructorArguments[1].Value is true);

    private static bool IsFrameworkAssembly(IAssemblySymbol assembly, int targetMajor)
    {
        var identity = assembly.Identity;
        if (targetMajor != 0 && identity.Version.Major != targetMajor || !string.IsNullOrEmpty(identity.CultureName) ||
            identity.PublicKeyToken.IsDefaultOrEmpty)
        {
            return false;
        }
        var token = string.Concat(identity.PublicKeyToken.Select(value => value.ToString("x2", System.Globalization.CultureInfo.InvariantCulture)));
        return identity.Name == "System.Private.CoreLib" && token == "7cec85d7bea7798e" ||
               identity.Name is "System.Runtime" or "System.Runtime.InteropServices" or "System.Collections" && token == "b03f5f7f11d50a3a";
    }
}
