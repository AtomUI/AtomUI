using System.Collections.Immutable;
using System.Text;
using AtomUI.Generator.Diagnostics;
using AtomUI.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator.DesignLanguages;

/// <summary>
/// For a language package: the schema of every <c>[LanguageDesignToken]</c> class (one token descriptor per property,
/// the inherited ones included) and <c>DesignLanguageRegistration.Register(IThemeManagerBuilder)</c>, which
/// registers every <c>[DesignLanguage]</c> class with the schema of its language token type.
/// </summary>
[Generator]
public sealed class DesignLanguageRegistrationGenerator : IIncrementalGenerator
{
    private const string BuilderMetadataName = "AtomUI.Theme.IThemeManagerBuilder";

    /// <summary>
    /// Registers the pipeline that collects every class marked with <c>[LanguageDesignToken]</c> or
    /// <c>[DesignLanguage]</c>, and builds the language token schemas and the registration class once per compilation.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var tokens = initContext.SyntaxProvider.ForAttributeWithMetadataName(
            TargetMarkConstants.LanguageDesignTokenAttribute,
            static (node, _) => node is ClassDeclarationSyntax,
            static (context, _) => RegistrationType.MetadataNameOf((INamedTypeSymbol)context.TargetSymbol)).Collect();
        var languages = initContext.SyntaxProvider.ForAttributeWithMetadataName(
            TargetMarkConstants.DesignLanguageAttribute,
            static (node, _) => node is ClassDeclarationSyntax,
            static (context, _) => RegistrationType.MetadataNameOf((INamedTypeSymbol)context.TargetSymbol)).Collect();
        var output = initContext.CompilationProvider.Combine(tokens).Combine(languages)
            .Select(static (input, cancellationToken) =>
                Build(input.Left.Left, input.Left.Right, input.Right, cancellationToken));
        initContext.RegisterSourceOutput(output, static (context, output) => output.Write(context));
    }

    private static GenerationResult Build(Compilation compilation, ImmutableArray<string> tokenNames,
        ImmutableArray<string> languageNames, CancellationToken cancellationToken)
    {
        var output = new GenerationOutput();
        if (tokenNames.IsEmpty && languageNames.IsEmpty)
        {
            return output.Freeze();
        }

        var schemas = new List<LanguageTokenSchema>();
        foreach (var name in tokenNames.Distinct(StringComparer.Ordinal).OrderBy(static name => name, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (compilation.GetTypeByMetadataName(name) is not { } type)
            {
                continue;
            }

            if (LanguageTokenSchema.Create(compilation, type, output.ReportDiagnostic) is { } schema)
            {
                schemas.Add(schema);
            }
        }

        // A language package does not reference any control package.
        if (!languageNames.IsEmpty)
        {
            foreach (var reference in compilation.SourceModule.ReferencedAssemblySymbols)
            {
                if (TokenSchemaDigestWriter.Read(reference) is not null)
                {
                    output.ReportDiagnostic(Diagnostic.Create(AtomUIDiagnosticDescriptors.PackageDependencyDirection, Location.None,
                        $"A language package does not reference control packages; it references '{reference.Name}'."));
                }
            }
        }

        var languages = new List<(string LanguageType, LanguageTokenSchema Schema)>();
        var languageBase = compilation.GetTypeByMetadataName(TargetMarkConstants.DesignLanguageBase);
        foreach (var name in languageNames.Distinct(StringComparer.Ordinal).OrderBy(static name => name, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (compilation.GetTypeByMetadataName(name) is not { } type || languageBase is null)
            {
                continue;
            }

            var location = type.Locations.FirstOrDefault(static location => location.IsInSource) ?? Location.None;
            var tokenType = FindLanguageTokenType(type, languageBase);
            if (type.IsAbstract || type.IsGenericType || tokenType is null ||
                !type.InstanceConstructors.Any(static constructor =>
                    constructor.DeclaredAccessibility == Accessibility.Public && constructor.Parameters.Length == 0))
            {
                Report(output, location,
                    $"'{type.ToDisplayString()}' must be a non-abstract, non-generic class derived from DesignLanguage<TLanguageToken> with a public parameterless constructor.");
                continue;
            }

            var schema = schemas.FirstOrDefault(candidate =>
                string.Equals(candidate.TypeName, tokenType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparison.Ordinal));
            if (schema is null)
            {
                Report(output, location,
                    $"The language token type '{tokenType.ToDisplayString()}' of '{type.ToDisplayString()}' must be declared with [LanguageDesignToken] in the same assembly.");
                continue;
            }

            languages.Add((type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), schema));
        }

        var generatedNamespace = GeneratedCodeNamespace.ForAssembly(compilation.AssemblyName);
        if (schemas.Count != 0)
        {
            output.AddSource("GeneratedLanguageTokenSchema.g.cs",
                GeneratedSourceText.From(WriteSchemas(generatedNamespace, schemas)));
        }

        if (languages.Count != 0 && compilation.GetTypeByMetadataName(BuilderMetadataName) is not null)
        {
            output.AddSource("DesignLanguageRegistration.g.cs",
                GeneratedSourceText.From(WriteRegistration(generatedNamespace, languages)));
        }

        return output.Freeze();
    }

    private static INamedTypeSymbol? FindLanguageTokenType(INamedTypeSymbol type, INamedTypeSymbol languageBase)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, languageBase))
            {
                return current.TypeArguments[0] as INamedTypeSymbol;
            }
        }

        return null;
    }

    private static string WriteSchemas(string generatedNamespace, IReadOnlyList<LanguageTokenSchema> schemas)
    {
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("using global::AtomUI.Theme.Schema;");
        source.AppendLine();
        source.Append("namespace ").Append(generatedNamespace).AppendLine(";");
        source.AppendLine();
        source.AppendLine("internal static class GeneratedLanguageTokenSchema");
        source.AppendLine("{");
        foreach (var schema in schemas)
        {
            source.Append("    internal static global::System.Collections.Generic.IReadOnlyList<TokenDescriptor> ")
                  .Append(schema.MemberName).Append("() => s_").Append(schema.MemberName).AppendLine(";");
            source.AppendLine();
            source.Append("    private static readonly TokenDescriptor[] s_").Append(schema.MemberName).AppendLine(" =");
            source.AppendLine("    [");
            for (var slot = 0; slot < schema.Tokens.Count; slot++)
            {
                var token = schema.Tokens[slot];
                source.Append("        TokenDescriptor.CreateGenerated(")
                      .Append(SymbolDisplay.FormatLiteral(token.Name, quote: true)).Append(", ").Append(slot)
                      .Append(", TokenStage.").Append(token.Stage)
                      .Append(", TokenValueCodec<").Append(token.ValueType).Append(">.Instance, ")
                      .Append(SymbolDisplay.FormatLiteral(schema.ShortName + "." + token.Name, quote: true))
                      .Append(", ").Append(schema.AccessorName).Append(".Instance");
                if (ThemeLiteralCodec.Supports(token.ValueType))
                {
                    source.Append(", 1");
                }
                source.AppendLine("),");
            }
            source.AppendLine("    ];");
            source.AppendLine();
        }
        source.AppendLine("}");
        foreach (var schema in schemas)
        {
            source.AppendLine();
            WriteAccessor(source, schema);
        }
        return source.ToString();
    }

    private static void WriteAccessor(StringBuilder source, LanguageTokenSchema schema)
    {
        const string tokenType = "global::AtomUI.Theme.DesignTokens.AbstractDesignToken";
        source.Append("internal sealed class ").Append(schema.AccessorName).AppendLine(" : TokenValueAccessor");
        source.AppendLine("{");
        source.Append("    internal static readonly ").Append(schema.AccessorName).Append(" Instance = new ")
              .Append(schema.AccessorName).AppendLine("();");
        source.AppendLine();
        source.Append("    public override object GetValue(").Append(tokenType).AppendLine(" token, int slot)");
        WriteSwitch(source, schema, static (builder, schema, token) => builder
            .Append("return ((").Append(schema.TypeName).Append(")token).").Append(token.Name).Append(';'));
        source.AppendLine();
        source.Append("    public override void SetValue(").Append(tokenType).AppendLine(" token, int slot, object value)");
        WriteSwitch(source, schema, static (builder, schema, token) => builder
            .Append("((").Append(schema.TypeName).Append(")token).").Append(token.Name)
            .Append(" = (").Append(token.ValueType).Append(")value!; return;"));
        source.AppendLine();
        source.Append("    public override object ProjectResourceValue(").Append(tokenType).AppendLine(" token, int slot)");
        WriteSwitch(source, schema, static (builder, schema, token) => builder
            .Append("return global::AtomUI.Theme.Schema.ThemeResourceValue.Project(((").Append(schema.TypeName)
            .Append(")token).").Append(token.Name).Append(");"));
        source.AppendLine("}");
    }

    private static void WriteSwitch(StringBuilder source, LanguageTokenSchema schema,
        Action<StringBuilder, LanguageTokenSchema, SchemaTokenInfo> writeCase)
    {
        source.AppendLine("    {");
        source.AppendLine("        switch (slot)");
        source.AppendLine("        {");
        for (var slot = 0; slot < schema.Tokens.Count; slot++)
        {
            source.Append("            case ").Append(slot).Append(": ");
            writeCase(source, schema, schema.Tokens[slot]);
            source.AppendLine();
        }
        source.AppendLine("            default: throw new global::System.ArgumentOutOfRangeException(nameof(slot));");
        source.AppendLine("        }");
        source.AppendLine("    }");
    }

    private static string WriteRegistration(string generatedNamespace,
        IReadOnlyList<(string LanguageType, LanguageTokenSchema Schema)> languages)
    {
        const string descriptorType = "global::AtomUI.Theme.Schema.DesignLanguageDescriptor";
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.Append("namespace ").Append(generatedNamespace).AppendLine(";");
        source.AppendLine();
        source.AppendLine("public static class DesignLanguageRegistration");
        source.AppendLine("{");
        source.AppendLine("    public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder)");
        source.AppendLine("    {");
        source.AppendLine("        global::System.ArgumentNullException.ThrowIfNull(builder);");
        for (var index = 0; index < languages.Count; index++)
        {
            source.Append("        builder.AddDesignLanguage(s_language").Append(index).AppendLine(");");
        }
        source.AppendLine("    }");
        // Created once and shared: compilation caches key a language by its descriptor instance.
        for (var index = 0; index < languages.Count; index++)
        {
            source.AppendLine();
            source.Append("    private static readonly ").Append(descriptorType).Append(" s_language").Append(index)
                  .Append(" = new ").Append(descriptorType).Append("(new ").Append(languages[index].LanguageType)
                  .Append("(), GeneratedLanguageTokenSchema.").Append(languages[index].Schema.MemberName).AppendLine("());");
        }
        source.AppendLine("}");
        source.AppendLine();
        // Preserve the type and method used by existing language packages and compiled callers.
        source.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
        source.AppendLine("public static class GeneratedDesignLanguageRegistration");
        source.AppendLine("{");
        source.AppendLine("    public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder) =>");
        source.AppendLine("        DesignLanguageRegistration.Register(builder);");
        source.AppendLine("}");
        return source.ToString();
    }

    private static void Report(GenerationOutput output, Location location, string message) =>
        output.ReportDiagnostic(Diagnostic.Create(AtomUIDiagnosticDescriptors.CompiledThemeBindingFailed, location, message));
}

/// <summary>
/// The token properties of one language token class, its own and the inherited ones, ordered by name.
/// </summary>
internal sealed class LanguageTokenSchema
{
    private LanguageTokenSchema(string typeName, string shortName, string memberName, IReadOnlyList<SchemaTokenInfo> tokens)
    {
        TypeName   = typeName;
        ShortName  = shortName;
        MemberName = memberName;
        Tokens     = tokens;
    }

    public string TypeName { get; }
    public string ShortName { get; }
    public string MemberName { get; }
    public string AccessorName => "GeneratedLanguageTokenAccessor_" + MemberName;
    public IReadOnlyList<SchemaTokenInfo> Tokens { get; }

    internal static LanguageTokenSchema? Create(Compilation compilation, INamedTypeSymbol type, Action<Diagnostic> report)
    {
        var languageToken = compilation.GetTypeByMetadataName(TargetMarkConstants.LanguageDesignToken);
        var location = type.Locations.FirstOrDefault(static location => location.IsInSource) ?? Location.None;
        if (languageToken is null || type.ContainingType is not null || type.IsGenericType ||
            !InheritsFrom(type, languageToken))
        {
            report(Diagnostic.Create(AtomUIDiagnosticDescriptors.CompiledThemeBindingFailed, location,
                $"'{type.ToDisplayString()}' must be a top-level, non-generic class derived from LanguageDesignToken."));
            return null;
        }

        var tokens = new Dictionary<string, SchemaTokenInfo>(StringComparer.Ordinal);
        for (var current = type; current is not null && !SymbolEqualityComparer.Default.Equals(current, languageToken);
             current = current.BaseType)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is not IPropertySymbol { IsStatic: false, IsIndexer: false, DeclaredAccessibility: Accessibility.Public } property ||
                    property.SetMethod is null || HasAttribute(property, TargetMarkConstants.NotTokenDefinitionAttribute) ||
                    tokens.ContainsKey(property.Name))
                {
                    continue;
                }

                tokens.Add(property.Name, new SchemaTokenInfo(property.Name,
                    property.Type.ToDisplayString(GeneratorSymbolDisplay.FullyQualifiedType),
                    current.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    IsSeed(property) ? SchemaTokenStage.Seed : SchemaTokenStage.Language));
            }
        }

        var typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return new LanguageTokenSchema(typeName, type.Name, "Tokens_" + Sanitize(type.ToDisplayString()),
            tokens.Values.OrderBy(static token => token.Name, StringComparer.Ordinal).ToArray());
    }

    private static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSeed(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == TargetMarkConstants.DesignTokenKindAttribute &&
                attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0].Value is 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAttribute(ISymbol symbol, string metadataName) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == metadataName);

    private static string Sanitize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        return builder.ToString();
    }
}

/// <summary>
/// The design languages declared in a compilation, for its build contracts: the identity (read from the constant the
/// <c>Id</c> override returns) and the schema of the language token class.
/// </summary>
internal static class LocalLanguageModel
{
    internal static IReadOnlyList<ThemeBuildContracts.LocalLanguage> Collect(Compilation compilation,
        IEnumerable<string> tokenNames, IEnumerable<string> languageNames, Action<Diagnostic> report,
        CancellationToken cancellationToken)
    {
        var languageBase = compilation.GetTypeByMetadataName(TargetMarkConstants.DesignLanguageBase);
        if (languageBase is null)
        {
            return Array.Empty<ThemeBuildContracts.LocalLanguage>();
        }

        var schemas = new Dictionary<string, LanguageTokenSchema>(StringComparer.Ordinal);
        foreach (var name in tokenNames.Distinct(StringComparer.Ordinal))
        {
            if (compilation.GetTypeByMetadataName(name) is { } type &&
                LanguageTokenSchema.Create(compilation, type, static _ => { }) is { } schema)
            {
                schemas[schema.TypeName] = schema;
            }
        }

        var result = new List<ThemeBuildContracts.LocalLanguage>();
        foreach (var name in languageNames.Distinct(StringComparer.Ordinal).OrderBy(static name => name, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (compilation.GetTypeByMetadataName(name) is not { } type)
            {
                continue;
            }

            INamedTypeSymbol? tokenType = null;
            for (var current = type.BaseType; current is not null; current = current.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, languageBase))
                {
                    tokenType = current.TypeArguments[0] as INamedTypeSymbol;
                    break;
                }
            }

            var location = type.Locations.FirstOrDefault(static location => location.IsInSource) ?? Location.None;
            if (tokenType is null ||
                !schemas.TryGetValue(tokenType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), out var languageSchema))
            {
                continue;
            }

            // A class the registration generator rejects (and reports) is never registered, so it publishes no contract:
            // a theme package must not compile themes for a language nothing enables.
            if (!IsRegistrable(type))
            {
                continue;
            }

            if (!TryGetConstantId(compilation, type, cancellationToken, out var id))
            {
                report(Diagnostic.Create(AtomUIDiagnosticDescriptors.CompiledThemeBindingFailed, location,
                    $"'{type.ToDisplayString()}' must return a constant string from its Id property."));
                continue;
            }

            result.Add(new ThemeBuildContracts.LocalLanguage(id, languageSchema));
        }

        return result;
    }

    /// <summary>
    /// The registration creates the language with <c>new T()</c>: a concrete, non-generic class with a public
    /// parameterless constructor.
    /// </summary>
    internal static bool IsRegistrable(INamedTypeSymbol type) =>
        !type.IsAbstract && !type.IsGenericType &&
        type.InstanceConstructors.Any(static constructor =>
            constructor.Parameters.Length == 0 && constructor.DeclaredAccessibility == Accessibility.Public);

    /// <summary>
    /// Reads the language identity from the <c>Id</c> override: any compile-time constant string (a literal, a
    /// <c>const</c> or <c>nameof</c>) that is a valid theme build identifier.
    /// </summary>
    internal static bool TryGetConstantId(Compilation compilation, INamedTypeSymbol type, CancellationToken cancellationToken,
        out string id)
    {
        id = "";
        var property = type.GetMembers("Id").OfType<IPropertySymbol>().FirstOrDefault(static property => property.IsOverride);
        var syntax = property?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) as PropertyDeclarationSyntax;
        ExpressionSyntax? expression = syntax?.ExpressionBody?.Expression;
        if (expression is null && syntax?.AccessorList?.Accessors.FirstOrDefault(static a => a.Keyword.Text == "get") is { } getter)
        {
            expression = getter.ExpressionBody?.Expression ??
                         (getter.Body?.Statements.SingleOrDefault() as ReturnStatementSyntax)?.Expression;
        }

        if (expression is null)
        {
            return false;
        }

        var constant = compilation.GetSemanticModel(expression.SyntaxTree).GetConstantValue(expression, cancellationToken);
        if (constant is { HasValue: true, Value: string text } && ThemeBuildIdentifierValid(text))
        {
            id = text;
            return true;
        }

        return false;
    }

    private static bool ThemeBuildIdentifierValid(string text) => AtomUI.Theme.Build.ThemeBuildIdentifier.IsValid(text);
}
