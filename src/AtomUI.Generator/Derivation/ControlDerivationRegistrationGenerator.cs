using System.Collections.Immutable;
using System.Text;
using AtomUI.Generator.Diagnostics;
using AtomUI.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator.Derivation;

/// <summary>
/// For a value package: <c>GeneratedControlDerivationRegistration.Register(IThemeManagerBuilder)</c>, which registers one
/// <c>ControlDerivationDescriptor</c> per <c>[ControlDerivation]</c> class of the compilation. Each class must implement
/// <c>IControlDerivation&lt;TLanguageToken, TToken&gt;</c> exactly once for a control token bound to a control.
/// </summary>
[Generator]
public sealed class ControlDerivationRegistrationGenerator : IIncrementalGenerator
{
    private const string BuilderMetadataName = "AtomUI.Theme.IThemeManagerBuilder";
    private const string DescriptorType = "global::AtomUI.Theme.Schema.ControlDerivationDescriptor";
    private const string PackageAttribute = "AtomUI.Theme.Languages.ControlValuePackageRegistrationAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var derivations = initContext.SyntaxProvider.ForAttributeWithMetadataName(
            TargetMarkConstants.ControlDerivationAttribute,
            static (node, _) => node is ClassDeclarationSyntax,
            static (context, _) => RegistrationType.MetadataNameOf((INamedTypeSymbol)context.TargetSymbol)).Collect();
        var output = initContext.CompilationProvider.Combine(derivations)
            .Select(static (input, cancellationToken) => Build(input.Left, input.Right, cancellationToken));
        initContext.RegisterSourceOutput(output, static (context, output) => output.Write(context));
    }

    private static GenerationResult Build(Compilation compilation, ImmutableArray<string> names,
        CancellationToken cancellationToken)
    {
        var output = new GenerationOutput();
        var derivationInterface = compilation.GetTypeByMetadataName(TargetMarkConstants.ControlDerivationInterface);
        var controlToken = compilation.GetTypeByMetadataName(TargetMarkConstants.ControlTokenInterface);
        if (names.IsEmpty || derivationInterface is null || controlToken is null ||
            compilation.GetTypeByMetadataName(BuilderMetadataName) is null)
        {
            return output.Freeze();
        }

        var entries = new List<(string Language, string Token, string Derivation, string Owner)>();
        var seen = new Dictionary<(string, string), string>();
        var covered = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var analyzed = new List<DerivationSymbols>();
        foreach (var name in names.Distinct(StringComparer.Ordinal).OrderBy(static name => name, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (compilation.GetTypeByMetadataName(name) is not { } type)
            {
                continue;
            }

            var location = type.Locations.FirstOrDefault(static location => location.IsInSource) ?? Location.None;
            var typeName = type.ToDisplayString();
            if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsGenericType ||
                !RegistrationModelBuilder.Accessible(type) ||
                !type.InstanceConstructors.Any(static constructor =>
                    constructor.DeclaredAccessibility == Accessibility.Public && constructor.Parameters.Length == 0))
            {
                Report(output, location,
                    $"'{typeName}' must be a non-abstract, non-generic class with a public parameterless constructor, accessible within its assembly.");
                continue;
            }

            var implemented = type.AllInterfaces
                .Where(candidate => SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, derivationInterface))
                .ToArray();
            if (implemented.Length != 1)
            {
                Report(output, location, $"'{typeName}' must implement IControlDerivation<TLanguageToken, TToken> exactly once.");
                continue;
            }

            var language = implemented[0].TypeArguments[0];
            var token = implemented[0].TypeArguments[1];
            // A token class of this compilation gets its IControlToken half from another generator, which this one
            // cannot see: it counts as bound when it is a [ControlDesignToken] class that matches exactly one control,
            // the rule that generator applies.
            if (token is not INamedTypeSymbol { IsAbstract: false } namedToken ||
                !(namedToken.AllInterfaces.Any(candidate => SymbolEqualityComparer.Default.Equals(candidate, controlToken)) ||
                  (SymbolEqualityComparer.Default.Equals(namedToken.ContainingAssembly, compilation.Assembly) &&
                   namedToken.GetAttributes().Any(static attribute =>
                       attribute.AttributeClass?.ToDisplayString() == TargetMarkConstants.ControlDesignTokenAttribute) &&
                   ControlThemeModelBuilder.MatchesOneControl(compilation, namedToken))))
            {
                Report(output, location, $"'{typeName}' derives '{token.ToDisplayString()}', which is not a control token bound to a control.");
                continue;
            }

            var languageName = language.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var tokenName = token.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var derivationName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (seen.TryGetValue((languageName, tokenName), out var existing))
            {
                Report(output, location,
                    $"'{typeName}' and '{existing}' both derive '{token.ToDisplayString()}' for '{language.ToDisplayString()}'.");
                continue;
            }

            seen.Add((languageName, tokenName), typeName);
            entries.Add((languageName, tokenName, derivationName, namedToken.ContainingAssembly.Name));
            analyzed.Add(new DerivationSymbols(type, namedToken, language, location));
            // A derived control package's token schema digest, recorded for the startup check (spec 3.3).
            if (!SymbolEqualityComparer.Default.Equals(namedToken.ContainingAssembly, compilation.Assembly) &&
                TokenSchemaDigestWriter.Read(namedToken.ContainingAssembly) is { } digest)
            {
                covered[namedToken.ContainingAssembly.Name] = digest;
            }
        }

        DerivationAnalysis.Run(compilation, analyzed, output.ReportDiagnostic, cancellationToken);
        if (entries.Count != 0)
        {
            var packages = compilation.GetTypeByMetadataName(PackageAttribute) is null
                ? Array.Empty<(string Owner, string Language)>()
                : analyzed.Where(item => !SymbolEqualityComparer.Default.Equals(item.Token.ContainingAssembly, compilation.Assembly))
                    .Select(static item => (Owner: item.Token.ContainingAssembly.Name,
                        Language: item.Language.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))
                    .Distinct().OrderBy(static item => item.Owner, StringComparer.Ordinal)
                    .ThenBy(static item => item.Language, StringComparer.Ordinal).ToArray();
            output.AddSource("GeneratedControlDerivationRegistration.g.cs",
                GeneratedSourceText.From(Write(GeneratedCodeNamespace.ForAssembly(compilation.AssemblyName), entries,
                    compilation.GetTypeByMetadataName(TokenSchemaDigestWriter.CoversAttribute) is null
                        ? new SortedDictionary<string, string>()
                        : covered, packages, compilation.Assembly.Name)));
        }

        return output.Freeze();
    }

    private static string Write(string generatedNamespace, IReadOnlyList<(string Language, string Token, string Derivation, string Owner)> entries,
        IEnumerable<KeyValuePair<string, string>> covered, IReadOnlyList<(string Owner, string Language)> packages,
        string assemblyName)
    {
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        foreach (var pair in covered)
        {
            source.Append("[assembly: global::").Append(TokenSchemaDigestWriter.CoversAttribute).Append('(')
                  .Append(ThemeBuildContracts.Literal(pair.Key)).Append(", ").Append(ThemeBuildContracts.Literal(pair.Value))
                  .AppendLine(")]");
        }
        for (var index = 0; index < packages.Count; index++)
        {
            var member = PackageMember(index, packages.Count);
            source.Append("[assembly: global::").Append(PackageAttribute).Append("(typeof(global::")
                  .Append(generatedNamespace).Append(".GeneratedControlDerivationRegistration), ")
                  .Append(ThemeBuildContracts.Literal(member)).AppendLine(")]");
        }
        source.Append("namespace ").Append(generatedNamespace).AppendLine(";");
        source.AppendLine();
        source.AppendLine("public static class GeneratedControlDerivationRegistration");
        source.AppendLine("{");
        source.AppendLine("    public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder)");
        source.AppendLine("    {");
        source.AppendLine("        global::System.ArgumentNullException.ThrowIfNull(builder);");
        var local = entries.Where(entry => entry.Owner == assemblyName).ToArray();
        if (packages.Count == 0)
        {
            source.AppendLine("        builder.AddControlDerivations(s_derivations);");
        }
        else
        {
            for (var index = 0; index < packages.Count; index++)
            {
                source.Append("        builder.AddControlDerivations(Values").Append(index).AppendLine(".Items);");
            }
            if (local.Length != 0)
            {
                source.AppendLine("        builder.AddControlDerivations(LocalValues.Items);");
            }
        }
        source.AppendLine("    }");
        source.AppendLine();
        // Created once and shared: compilation caches key a derivation by its descriptor instance.
        if (packages.Count == 0)
        {
            source.Append("    private static readonly ").Append(DescriptorType).AppendLine("[] s_derivations =");
            WriteEntries(source, "    ", entries);
        }
        else
        {
            WriteValuePackages(source, assemblyName, packages);
            for (var index = 0; index < packages.Count; index++)
            {
                var pair = packages[index];
                WriteGroup(source, "Values" + index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    entries.Where(entry => entry.Owner == pair.Owner && entry.Language == pair.Language));
            }
            if (local.Length != 0)
            {
                WriteGroup(source, "LocalValues", local);
            }
        }
        source.AppendLine("}");
        return source.ToString();
    }

    private static void WriteGroup(StringBuilder source, string name,
        IEnumerable<(string Language, string Token, string Derivation, string Owner)> entries)
    {
        source.Append("    private static class ").AppendLine(name);
        source.AppendLine("    {");
        source.Append("        internal static readonly ").Append(DescriptorType).AppendLine("[] Items =");
        WriteEntries(source, "        ", entries);
        source.AppendLine("    }");
    }

    private static void WriteEntries(StringBuilder source, string indent,
        IEnumerable<(string Language, string Token, string Derivation, string Owner)> entries)
    {
        source.Append(indent).AppendLine("[");
        foreach (var entry in entries)
        {
            source.Append(indent).Append("    ").Append(DescriptorType).Append(".Create<").Append(entry.Language).Append(", ")
                  .Append(entry.Token).Append(", ").Append(entry.Derivation).AppendLine(">(),");
        }
        source.Append(indent).AppendLine("];");
    }

    private static string PackageMember(int index, int count) =>
        count == 1 ? "Package" : "Package" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static void WriteValuePackages(StringBuilder source, string assemblyName,
        IReadOnlyList<(string Owner, string Language)> packages)
    {
        for (var index = 0; index < packages.Count; index++)
        {
            var (owner, language) = packages[index];
            var ownerLiteral = ThemeBuildContracts.Literal(owner);
            source.Append("    public static global::AtomUI.Theme.Languages.ControlValuePackage ")
                .Append(PackageMember(index, packages.Count)).Append(" { get; } = new(")
                .Append(ThemeBuildContracts.Literal(assemblyName)).Append(", ").Append(ownerLiteral)
                .Append(", typeof(").Append(language).AppendLine("),")
                .Append("        static builder => builder.AddControlDerivations(Values").Append(index).AppendLine(".Items));");
        }
        source.AppendLine();
    }

    private static void Report(GenerationOutput output, Location location, string message) =>
        output.ReportDiagnostic(Diagnostic.Create(AtomUIDiagnosticDescriptors.CompiledThemeBindingFailed, location, message));
}
