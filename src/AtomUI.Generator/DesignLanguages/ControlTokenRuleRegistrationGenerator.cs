using System.Collections.Immutable;
using System.Text;
using AtomUI.Generator.Diagnostics;
using AtomUI.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator.DesignLanguages;

/// <summary>
/// For a value package: <c>GeneratedControlTokenRuleRegistration.Register(IThemeManagerBuilder)</c>, which registers one
/// <c>ControlTokenRuleDescriptor</c> per <c>[ControlTokenRule]</c> class of the compilation. Each class must implement
/// <c>IControlTokenRule&lt;TLanguageToken, TToken&gt;</c> exactly once for a control token bound to a control.
/// </summary>
[Generator]
public sealed partial class ControlTokenRuleRegistrationGenerator : IIncrementalGenerator
{
    private const string BuilderMetadataName = "AtomUI.Theme.IThemeManagerBuilder";
    private const string RegistrationClassName = "GeneratedControlTokenRuleRegistration";
    private const string DescriptorType = "global::AtomUI.Theme.Schema.ControlTokenRuleDescriptor";
    private const string PackageAttribute = "AtomUI.Theme.Languages.ControlValuePackageRegistrationAttribute";

    /// <summary>
    /// Registers the pipeline that collects every class marked with <c>[ControlTokenRule]</c> and builds the
    /// registration class once per compilation.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var rules = initContext.SyntaxProvider.ForAttributeWithMetadataName(
            TargetMarkConstants.ControlTokenRuleAttribute,
            static (node, _) => node is ClassDeclarationSyntax,
            static (context, _) => RegistrationType.MetadataNameOf((INamedTypeSymbol)context.TargetSymbol)).Collect();
        var output = initContext.CompilationProvider.Combine(rules).Combine(initContext.AnalyzerConfigOptionsProvider)
            .Select(static (input, cancellationToken) => Build(input.Left.Left, input.Left.Right,
                input.Right.GlobalOptions, cancellationToken));
        initContext.RegisterSourceOutput(output, static (context, output) => output.Write(context));
    }

    private static GenerationResult Build(Compilation compilation, ImmutableArray<string> names,
        Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions options, CancellationToken cancellationToken)
    {
        var output = new GenerationOutput();
        var ruleInterface = compilation.GetTypeByMetadataName(TargetMarkConstants.ControlTokenRuleInterface);
        var controlToken = compilation.GetTypeByMetadataName(TargetMarkConstants.ControlTokenInterface);
        var builder = compilation.GetTypeByMetadataName(BuilderMetadataName);
        if (names.IsEmpty || ruleInterface is null || controlToken is null ||
            builder is null)
        {
            return output.Freeze();
        }

        var entries = new List<RuleEntry>();
        var seen = new Dictionary<(string, string), string>();
        var covered = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var analyzed = new List<ControlTokenRuleSymbols>();
        var supportsTypedDigest = compilation.GetTypeByMetadataName("AtomUI.Theme.Schema.ControlTokenRuleDescriptor")
            ?.GetMembers("Create").OfType<IMethodSymbol>().Any(static method => method.Arity == 3 &&
                method.Parameters.Length == 1 && method.Parameters[0].Type.SpecialType == SpecialType.System_String) == true;
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
                .Where(candidate => SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, ruleInterface))
                .ToArray();
            if (implemented.Length != 1)
            {
                Report(output, location, $"'{typeName}' must implement IControlTokenRule<TLanguageToken, TToken> exactly once.");
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
                Report(output, location, $"'{typeName}' writes '{token.ToDisplayString()}', which is not a control token bound to a control.");
                continue;
            }

            var languageName = language.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var tokenName = token.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var ruleName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (seen.TryGetValue((languageName, tokenName), out var existing))
            {
                Report(output, location,
                    $"'{typeName}' and '{existing}' both register a token rule for '{token.ToDisplayString()}' under '{language.ToDisplayString()}'.");
                continue;
            }

            seen.Add((languageName, tokenName), typeName);
            analyzed.Add(new ControlTokenRuleSymbols(type, implemented[0], namedToken, language, location));
            // A covered control package's token schema digest, recorded for the startup check.
            string? expectedDigest = null;
            if (!SymbolEqualityComparer.Default.Equals(namedToken.ContainingAssembly, compilation.Assembly) &&
                TokenSchemaDigestWriter.Read(namedToken.ContainingAssembly) is { } digest)
            {
                covered[namedToken.ContainingAssembly.Name] = digest;
                expectedDigest = supportsTypedDigest ? digest : null;
            }
            entries.Add(new(languageName, tokenName, ruleName, namedToken.ContainingAssembly.Name, expectedDigest));
        }

        ControlTokenRuleAnalysis.Run(compilation, analyzed, output.ReportDiagnostic, cancellationToken);
        if (entries.Count != 0)
        {
            var packages = compilation.GetTypeByMetadataName(PackageAttribute) is null
                ? Array.Empty<(string Owner, string Language)>()
                : analyzed.Where(item => !SymbolEqualityComparer.Default.Equals(item.Token.ContainingAssembly, compilation.Assembly))
                    .Select(static item => (Owner: item.Token.ContainingAssembly.Name,
                        Language: item.Language.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))
                    .Distinct().OrderBy(static item => item.Owner, StringComparer.Ordinal)
                    .ThenBy(static item => item.Language, StringComparer.Ordinal).ToArray();
            var conditional = compilation.GetTypeByMetadataName("AtomUI.Registration.ControlValueMapAttribute") is not null &&
                RegistrationFrameworkResolver.TryResolve(compilation, options, out var framework, out _) &&
                framework == AtomUI.Build.Tasks.Registration.RegistrationFrameworkFamily.OfficialTypeMap;
            var generatedNamespace = GeneratedCodeNamespace.ForAssembly(compilation.AssemblyName);
            var hasBehaviors = ControlBehaviorRegistrationGenerator.HasValidContributions(compilation, cancellationToken);
            var hasThemePackage = ControlBehaviorRegistrationGenerator.HasGeneratedThemePackage(compilation, options, generatedNamespace);
            output.AddSource(RegistrationClassName + ".g.cs",
                GeneratedSourceText.From(Write(generatedNamespace, entries,
                    compilation.GetTypeByMetadataName(TokenSchemaDigestWriter.CoversAttribute) is null
                        ? new SortedDictionary<string, string>()
                        : covered, packages, compilation.Assembly.Name, conditional,
                    !builder.GetMembers("AddDeferredControlTokenRules").IsEmpty,
                    hasBehaviors,
                    hasThemePackage)));
        }

        return output.Freeze();
    }

    private static string Write(string generatedNamespace, IReadOnlyList<RuleEntry> entries,
        IEnumerable<KeyValuePair<string, string>> covered, IReadOnlyList<(string Owner, string Language)> packages,
        string assemblyName, bool conditional, bool deferExplicitRegistration, bool hasBehaviors = false,
        bool hasThemePackage = false)
    {
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        if (conditional)
        {
            source.AppendLine("#nullable enable");
        }
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
                  .Append(generatedNamespace).Append('.').Append(RegistrationClassName).Append("), ")
                  .Append(ThemeBuildContracts.Literal(member));
            if (conditional)
            {
                source.Append(", ").Append(ThemeBuildContracts.Literal("Resolve" + member));
            }
            source.AppendLine(")]");
        }
        if (conditional)
        {
            WriteConditionalRegistration(source, generatedNamespace, entries, packages, assemblyName, deferExplicitRegistration, hasBehaviors, hasThemePackage);
            return source.ToString();
        }
        source.Append("namespace ").Append(generatedNamespace).AppendLine(";");
        source.AppendLine();
        source.Append("public static class ").AppendLine(RegistrationClassName);
        source.AppendLine("{");
        WriteExplicitRegistrationStart(source, deferExplicitRegistration);
        var local = entries.Where(entry => entry.Owner == assemblyName).ToArray();
        if (packages.Count == 0)
        {
            source.AppendLine("        builder.AddControlTokenRules(s_tokenRules);");
        }
        else
        {
            for (var index = 0; index < packages.Count; index++)
            {
                source.Append("        builder.AddControlTokenRules(Values").Append(index).AppendLine(".Items);");
            }
            if (local.Length != 0)
            {
                source.AppendLine("        builder.AddControlTokenRules(LocalValues.Items);");
            }
        }
        if (hasBehaviors)
        {
            source.AppendLine("        " + ControlBehaviorRegistrationGenerator.Registration + ".Register(builder);");
        }
        source.AppendLine("    }");
        source.AppendLine();
        // Created once and shared: compilation caches key a rule by its descriptor instance.
        if (packages.Count == 0)
        {
            source.Append("    private static readonly ").Append(DescriptorType).AppendLine("[] s_tokenRules =");
            WriteEntries(source, "    ", entries);
        }
        else
        {
            WriteValuePackages(source, assemblyName, packages, hasBehaviors, hasThemePackage);
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
        IEnumerable<RuleEntry> entries)
    {
        source.Append("    private static class ").AppendLine(name);
        source.AppendLine("    {");
        source.Append("        internal static readonly ").Append(DescriptorType).AppendLine("[] Items =");
        WriteEntries(source, "        ", entries);
        source.AppendLine("    }");
    }

    private static void WriteExplicitRegistrationStart(StringBuilder source, bool deferred)
    {
        source.AppendLine("    public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder)");
        source.AppendLine("    {");
        source.AppendLine("        global::System.ArgumentNullException.ThrowIfNull(builder);");
        if (deferred)
        {
            source.AppendLine("        builder.AddDeferredControlTokenRules(RegisterRules);");
            source.AppendLine("    }");
            source.AppendLine();
            source.AppendLine("    private static void RegisterRules(global::AtomUI.Theme.IThemeManagerBuilder builder)");
            source.AppendLine("    {");
        }
    }

    private static void WriteEntries(StringBuilder source, string indent,
        IEnumerable<RuleEntry> entries)
    {
        source.Append(indent).AppendLine("[");
        foreach (var entry in entries)
        {
            source.Append(indent).Append("    ");
            WriteCreateRule(source, entry).AppendLine(",");
        }
        source.Append(indent).AppendLine("];");
    }

    private static StringBuilder WriteCreateRule(StringBuilder source, RuleEntry entry) =>
        source.Append(DescriptorType).Append(".Create<").Append(entry.Language).Append(", ")
              .Append(entry.Token).Append(", ").Append(entry.Rule).Append(">(")
              .Append(entry.ExpectedDigest is { } digest ? ThemeBuildContracts.Literal(digest) : null).Append(')');

    private static string PackageMember(int index, int count) =>
        count == 1 ? "Package" : "Package" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static void WriteValuePackages(StringBuilder source, string assemblyName,
        IReadOnlyList<(string Owner, string Language)> packages, bool hasBehaviors = false, bool hasThemePackage = false)
    {
        var themeSuffix = hasThemePackage ? ", PackageMetadata.Definition);" : ");";
        for (var index = 0; index < packages.Count; index++)
        {
            var (owner, language) = packages[index];
            var ownerLiteral = ThemeBuildContracts.Literal(owner);
            source.Append("    public static global::AtomUI.Theme.Languages.ControlValuePackage ")
                .Append(PackageMember(index, packages.Count)).Append(" { get; } = new(")
                .Append(ThemeBuildContracts.Literal(assemblyName)).Append(", ").Append(ownerLiteral)
                .Append(", typeof(").Append(language).AppendLine("),");
            if (hasBehaviors)
            {
                source.Append("        static builder => { builder.AddControlTokenRules(Values").Append(index)
                    .Append(".Items); ").Append(ControlBehaviorRegistrationGenerator.Registration).Append(".Register(builder); }")
                    .AppendLine(themeSuffix);
            }
            else
            {
                source.Append("        static builder => builder.AddControlTokenRules(Values").Append(index).Append(".Items)")
                    .AppendLine(themeSuffix);
            }
        }
        source.AppendLine();
    }

    private sealed record RuleEntry(string Language, string Token, string Rule, string Owner, string? ExpectedDigest);

    private static void Report(GenerationOutput output, Location location, string message) =>
        output.ReportDiagnostic(Diagnostic.Create(AtomUIDiagnosticDescriptors.CompiledThemeBindingFailed, location, message));
}
